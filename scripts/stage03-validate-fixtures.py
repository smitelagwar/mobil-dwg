#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import urllib.request

REQUIRED_FEATURES = {"basic_geometry", "turkish_text", "nested_block", "dimension", "hatch", "paper_space"}
ALLOWED_LICENSES = {"0bsd", "apache-2.0", "bsd-2-clause", "bsd-3-clause", "isc", "mit"}


def fail(message: str) -> None:
    raise RuntimeError(message)


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def git_blob_sha1(data: bytes) -> str:
    prefix = f"blob {len(data)}\0".encode("ascii")
    return hashlib.sha1(prefix + data).hexdigest()


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def check_git_private_ignore(repo_root: Path) -> None:
    sentinel = "fixtures/private/stage03-sentinel.dwg"
    proc = subprocess.run(["git", "check-ignore", "-q", sentinel], cwd=repo_root, check=False)
    if proc.returncode != 0:
        fail(f"private fixture path is not ignored: {sentinel}")
    tracked = subprocess.check_output(
        ["git", "ls-files", "fixtures/private", "**/private-fixtures"],
        cwd=repo_root,
        text=True,
    ).strip()
    if tracked:
        fail(f"private fixture content is tracked:\n{tracked}")


def check_cad_byte_attributes(repo_root: Path, paths: list[str]) -> None:
    attributes_file = repo_root / ".gitattributes"
    if not attributes_file.is_file():
        fail(".gitattributes is required to preserve CAD fixture bytes")
    for path in paths:
        proc = subprocess.run(
            ["git", "check-attr", "text", "--", path],
            cwd=repo_root,
            text=True,
            capture_output=True,
            check=True,
        )
        if not proc.stdout.strip().endswith(": text: unset"):
            fail(f"CAD fixture line-ending normalization is not disabled: {path}: {proc.stdout.strip()}")


def fetch_remote(url: str, target: Path) -> bytes:
    target.parent.mkdir(parents=True, exist_ok=True)
    request = urllib.request.Request(url, headers={"User-Agent": "mobil-dwg-stage03-fixture-audit/2"})
    with urllib.request.urlopen(request, timeout=60) as response:
        data = response.read()
    target.write_bytes(data)
    return data


def validate_magic(entry: dict, data: bytes) -> None:
    version = entry["acad_version"]
    if entry["format"] == "dwg":
        actual = data[:6].decode("ascii", errors="replace")
        if actual != version:
            fail(f"{entry['id']}: DWG magic/version {actual!r} != {version!r}")
        return
    if data.startswith(b"AutoCAD Binary DXF"):
        return
    text = data[:65536].decode("latin-1", errors="replace")
    if "$ACADVER" not in text or version not in text:
        fail(f"{entry['id']}: ASCII DXF does not expose expected $ACADVER {version}")


def validate_hash(entry: dict, data: bytes) -> None:
    if len(data) != entry["size_bytes"]:
        fail(f"{entry['id']}: size {len(data)} != manifest {entry['size_bytes']}")
    algorithm = entry["hash"]["algorithm"]
    expected = entry["hash"]["value"]
    actual = sha256(data) if algorithm == "sha256" else git_blob_sha1(data) if algorithm == "git-blob-sha1" else None
    if actual is None:
        fail(f"{entry['id']}: unsupported hash algorithm {algorithm}")
    if actual != expected:
        fail(f"{entry['id']}: {algorithm} {actual} != manifest {expected}")


def validate_rights(profile: dict | None, context: str, require_committed: bool = False) -> None:
    if not profile:
        fail(f"{context}: rights profile missing")
    license_id = str(profile.get("license", "")).lower()
    if license_id not in ALLOWED_LICENSES:
        fail(f"{context}: unresolved or rejected fixture license {profile.get('license')!r}")
    if require_committed and profile.get("redistribution") != "committed-permitted":
        fail(f"{context}: committed/generated smoke fixture is not explicitly redistribution-permitted")


def validate_entry_shape(entry: dict, manifest: dict, repo_root: Path) -> None:
    required = ("id", "corpus", "role", "format", "acad_version", "version_family", "storage", "size_bytes", "hash", "features", "expected", "golden")
    for key in required:
        if key not in entry:
            fail(f"fixture missing required key {key}: {entry.get('id', '<unknown>')}")
    if entry["corpus"] == "private" and entry["storage"]["mode"] != "private-local":
        fail(f"{entry['id']}: private fixture must be private-local")
    if entry["golden"]["image_status"] == "present" and entry["golden"]["redistribution"] != "permitted":
        fail(f"{entry['id']}: committed image golden lacks permitted redistribution status")
    if entry["storage"]["mode"] == "committed":
        path = str(entry["storage"].get("path", ""))
        if not path.startswith("fixtures/public/"):
            fail(f"{entry['id']}: committed CAD fixture must live under fixtures/public/")
        rights = manifest.get("rights_profiles", {}).get(entry["storage"].get("rights_profile"))
        validate_rights(rights, entry["id"], require_committed=True)
        target = (repo_root / path).resolve()
        public_root = (repo_root / "fixtures/public").resolve()
        if public_root not in target.parents:
            fail(f"{entry['id']}: committed fixture escapes fixtures/public")


def derive_negative(source: bytes, transform: dict) -> bytes:
    kind = transform["type"]
    if kind == "truncate":
        count = int(transform["bytes"])
        if count <= 6 or count >= len(source):
            fail("invalid truncation byte count")
        return source[:count]
    if kind == "xor-byte":
        offset = int(transform["offset"])
        value = int(transform["value"])
        if offset < 6 or offset >= len(source):
            fail("invalid corruption offset")
        mutated = bytearray(source)
        mutated[offset] ^= value
        return bytes(mutated)
    fail(f"unsupported negative transform: {kind}")


def validate_coverage(manifest: dict) -> None:
    fixtures = manifest["fixtures"]
    positives = [f for f in fixtures if f["role"] == "positive"]
    dwgs = [f for f in positives if f["format"] == "dwg"]
    dxfs = [f for f in positives if f["format"] == "dxf"]
    requirements = manifest["coverage_requirements"]
    if len(dwgs) < requirements["minimum_positive_dwg"]:
        fail("mini corpus does not contain enough positive DWG fixtures")
    if len(dxfs) < requirements["minimum_positive_dxf"]:
        fail("mini corpus does not contain enough positive DXF fixtures")
    if len({f["version_family"] for f in dwgs}) < requirements["minimum_distinct_dwg_version_families"]:
        fail("DWG version-family coverage is insufficient")
    feature_union = {feature for fixture in positives for feature in fixture["features"]}
    missing = set(requirements["required_features"]) - feature_union
    if missing:
        fail(f"required positive features missing: {sorted(missing)}")
    fixture_negative = {feature for f in fixtures if f["role"] == "negative" for feature in f["features"] if feature in {"missing_font", "missing_xref"}}
    derived_negative = {"truncated" if d["transform"]["type"] == "truncate" else "corrupt" for d in manifest["negative_derivations"]}
    available_negative = fixture_negative | derived_negative
    required_negative = set(requirements["required_negative_categories"])
    if not required_negative.issubset(available_negative):
        fail(f"negative coverage missing: {sorted(required_negative - available_negative)}")
    if not REQUIRED_FEATURES.issubset(feature_union):
        fail(f"validator required feature set missing: {sorted(REQUIRED_FEATURES - feature_union)}")


def validate_generated_and_smoke_set(manifest: dict, repo_root: Path) -> dict:
    fixtures = {f["id"]: f for f in manifest["fixtures"]}
    generated_items = manifest.get("generated_fixtures", [])
    generated = {f["id"]: f for f in generated_items}
    if len(generated) != len(generated_items):
        fail("generated fixture IDs are not unique")
    overlap = set(fixtures) & set(generated)
    if overlap:
        fail(f"fixture and generated fixture IDs overlap: {sorted(overlap)}")

    for item in generated_items:
        source_id = item.get("source_fixture_id")
        source = fixtures.get(source_id)
        if not source or source["storage"]["mode"] != "committed":
            fail(f"{item['id']}: generated fixture source must be a committed fixture")
        rights = manifest.get("rights_profiles", {}).get(item.get("rights_profile"))
        validate_rights(rights, item["id"], require_committed=True)
        generator = repo_root / str(item.get("generator", ""))
        if not generator.is_file():
            fail(f"{item['id']}: generator script missing: {item.get('generator')}")
        dependency = item.get("generator_dependency", {})
        if dependency.get("id") != "ACadSharp" or dependency.get("version") != "3.7.1":
            fail(f"{item['id']}: generated DWG must use exact ACadSharp 3.7.1 provenance")
        if item.get("artifact_policy") != "generated-at-validation-time-not-committed":
            fail(f"{item['id']}: unexpected generated artifact policy")

    smoke = manifest.get("android_smoke_set") or {}
    positive_ids = smoke.get("positive_fixture_ids", [])
    generated_ids = smoke.get("generated_fixture_ids", [])
    negative_ids = smoke.get("negative_fixture_ids", [])
    if not positive_ids or not generated_ids:
        fail("Android smoke set must contain committed and generated positive fixtures")

    formats: set[str] = set()
    for fixture_id in positive_ids:
        item = fixtures.get(fixture_id)
        if not item or item["role"] != "positive" or item["storage"]["mode"] != "committed":
            fail(f"Android positive smoke fixture is not committed-positive: {fixture_id}")
        rights = manifest.get("rights_profiles", {}).get(item["storage"].get("rights_profile"))
        validate_rights(rights, fixture_id, require_committed=True)
        formats.add(item["format"])
    for fixture_id in generated_ids:
        item = generated.get(fixture_id)
        if not item or item["role"] != "positive":
            fail(f"Android generated smoke fixture missing/invalid: {fixture_id}")
        formats.add(item["format"])
    for fixture_id in negative_ids:
        item = fixtures.get(fixture_id)
        if not item or item["role"] != "negative" or item["storage"]["mode"] != "committed":
            fail(f"Android negative smoke fixture is not committed-negative: {fixture_id}")

    required_formats = set(smoke.get("minimum_positive_formats", []))
    if not required_formats.issubset(formats) or not {"dwg", "dxf"}.issubset(formats):
        fail(f"Android smoke set must cover positive DWG and DXF; got {sorted(formats)}")

    print(
        "V03_ANDROID_SMOKE_SET_PASS "
        f"committed_positive={len(positive_ids)} generated_positive={len(generated_ids)} "
        f"negative={len(negative_ids)} formats={','.join(sorted(formats))}"
    )
    return {
        "positive_fixture_ids": positive_ids,
        "generated_fixture_ids": generated_ids,
        "negative_fixture_ids": negative_ids,
        "formats": sorted(formats),
        "claim_limit": smoke.get("claim_limit"),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", default="fixtures/manifest/stage03-mini.json")
    parser.add_argument("--cache", default=None)
    parser.add_argument("--evidence", default=None)
    args = parser.parse_args()
    repo_root = Path(__file__).resolve().parents[1]
    manifest_path = (repo_root / args.manifest).resolve()
    manifest = load_json(manifest_path)
    if manifest.get("schema_version") != 1:
        fail("unsupported fixture manifest schema_version")

    fixtures = manifest["fixtures"]
    fixture_ids = [f.get("id") for f in fixtures]
    if len(set(fixture_ids)) != len(fixture_ids):
        fail("fixture IDs are not unique")

    check_git_private_ignore(repo_root)
    committed_cad_paths = [str(f["storage"]["path"]) for f in fixtures if f.get("storage", {}).get("mode") == "committed"]
    check_cad_byte_attributes(repo_root, committed_cad_paths)
    validate_coverage(manifest)
    smoke_evidence = validate_generated_and_smoke_set(manifest, repo_root)

    cache_root = Path(args.cache) if args.cache else Path(tempfile.mkdtemp(prefix="mobil-dwg-stage03-"))
    cache_root.mkdir(parents=True, exist_ok=True)
    payload_by_id: dict[str, bytes] = {}
    evidence = {
        "schema_version": 2,
        "manifest": str(manifest_path.relative_to(repo_root)),
        "fixtures": [],
        "derived_negatives": [],
        "generated_fixture_contracts": manifest.get("generated_fixtures", []),
        "android_smoke_set": smoke_evidence,
    }

    for entry in fixtures:
        validate_entry_shape(entry, manifest, repo_root)
        mode = entry["storage"]["mode"]
        if mode == "remote-pinned":
            source_key = entry["storage"].get("source")
            source = manifest.get("sources", {}).get(source_key)
            if not source:
                fail(f"{entry['id']}: unknown remote source profile {source_key!r}")
            validate_rights(source, entry["id"])
            revision = source.get("revision") or ""
            base_url = source.get("base_url") or ""
            if not revision or revision not in base_url or "/master/" in base_url or "/main/" in base_url:
                fail(f"{entry['id']}: remote source URL is not immutable")
            url = base_url + entry["storage"]["path"]
            target = cache_root / "remote" / f"{entry['id']}.{entry['format']}"
            data = fetch_remote(url, target)
        elif mode == "committed":
            target = repo_root / entry["storage"]["path"]
            if not target.is_file():
                fail(f"{entry['id']}: committed fixture missing: {target}")
            data = target.read_bytes()
        elif mode == "private-local":
            continue
        else:
            fail(f"{entry['id']}: unsupported storage mode {mode}")
        validate_hash(entry, data)
        validate_magic(entry, data)
        payload_by_id[entry["id"]] = data
        evidence["fixtures"].append(
            {
                "id": entry["id"],
                "bytes": len(data),
                "manifest_hash": entry["hash"],
                "sha256_observed": sha256(data),
                "git_blob_sha1_observed": git_blob_sha1(data),
                "magic": data[:6].decode("ascii", errors="replace"),
            }
        )

    for derived in manifest["negative_derivations"]:
        source_id = derived["source_id"]
        if source_id not in payload_by_id:
            fail(f"derived negative source unavailable: {source_id}")
        data = derive_negative(payload_by_id[source_id], derived["transform"])
        out = cache_root / "derived-negative" / f"{derived['id']}.dwg"
        out.parent.mkdir(parents=True, exist_ok=True)
        out.write_bytes(data)
        evidence["derived_negatives"].append(
            {
                "id": derived["id"],
                "source_id": source_id,
                "bytes": len(data),
                "sha256_observed": sha256(data),
                "magic": data[:6].decode("ascii", errors="replace"),
                "expected": derived["expected"],
            }
        )

    if args.evidence:
        evidence_path = Path(args.evidence)
        evidence_path.parent.mkdir(parents=True, exist_ok=True)
        evidence_path.write_text(json.dumps(evidence, indent=2, sort_keys=True) + "\n", encoding="utf-8")

    print(f"STAGE03_FIXTURE_AUDIT_PASS fixtures={len(evidence['fixtures'])} derived_negatives={len(evidence['derived_negatives'])}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"STAGE03_FIXTURE_AUDIT_FAIL: {exc}", file=sys.stderr)
        raise
