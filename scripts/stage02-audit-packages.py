#!/usr/bin/env python3
from __future__ import annotations

import hashlib
import json
import re
import sys
import tempfile
import urllib.request
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

ALLOW_LICENSES = {"MIT", "Apache-2.0", "BSD-2-Clause", "BSD-3-Clause", "ISC", "0BSD"}
EXPECTED_CENTRAL = {
    "ACadSharp": "3.7.1",
    "SkiaSharp": "4.151.1",
    "IxMilia.Dxf": "0.8.4",
}
EXPECTED_DIRECT = {"ACadSharp": "3.7.1", "SkiaSharp": "4.151.1"}
EXPECTED_GRAPH = {
    "ACadSharp": ("Direct", "3.7.1"),
    "SkiaSharp": ("Direct", "4.151.1"),
    "SkiaSharp.NativeAssets.Android": ("Transitive", "4.151.1"),
}
EXPECTED_TARGET = "net10.0-android36.0"
EXPECTED_ANDROID_NATIVE = {
    "runtimes/android-arm/native/libSkiaSharp.so",
    "runtimes/android-arm64/native/libSkiaSharp.so",
    "runtimes/android-x64/native/libSkiaSharp.so",
    "runtimes/android-x86/native/libSkiaSharp.so",
}
ALLOWED_PRODUCTION_PACKAGE_REFS = {"ACadSharp", "SkiaSharp"}
FORBIDDEN_PRODUCTION_TFM_TOKENS = ("-ios", "-maccatalyst", "-windows")
NATIVE_FILE_SUFFIXES = {".so", ".aar", ".jar", ".dylib"}

ROOT = Path(__file__).resolve().parents[1]
LOCK = ROOT / "compliance/Stage02.DependencyProbe/packages.lock.json"
OUTPUT = ROOT / "compliance/stage02-package-manifest.json"
CPM = ROOT / "Directory.Packages.props"
SRC = ROOT / "src"


def local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def nuspec_license(data: bytes) -> str:
    root = ET.fromstring(data)
    for element in root.iter():
        if local_name(element.tag) == "license":
            license_type = element.attrib.get("type", "").strip().lower()
            value = (element.text or "").strip()
            if license_type == "expression":
                return value
            if license_type == "file":
                return f"FILE:{value}"
    for element in root.iter():
        if local_name(element.tag) == "licenseUrl":
            return f"URL:{(element.text or '').strip()}"
    return "UNKNOWN"


def validate_central_versions(failures: list[str]) -> None:
    if not CPM.exists():
        failures.append(f"missing central package file: {CPM.relative_to(ROOT)}")
        return

    root = ET.parse(CPM).getroot()
    versions: dict[str, str] = {}
    for element in root.iter():
        if local_name(element.tag) != "PackageVersion":
            continue
        package_id = element.attrib.get("Include", "").strip()
        version = element.attrib.get("Version", "").strip()
        if package_id:
            versions[package_id] = version

    if set(versions) != set(EXPECTED_CENTRAL):
        failures.append(
            f"central package set mismatch: expected {sorted(EXPECTED_CENTRAL)}, got {sorted(versions)}"
        )

    for package_id, expected_version in EXPECTED_CENTRAL.items():
        actual = versions.get(package_id)
        exact = f"[{expected_version}]"
        if actual != exact:
            failures.append(
                f"{package_id}: central version must use strict NuGet exact range {exact}, got {actual!r}"
            )
        if actual and ("*" in actual or "," in actual or "latest" in actual.lower()):
            failures.append(f"{package_id}: floating/open version syntax is forbidden: {actual!r}")


def validate_production_source_boundary(failures: list[str]) -> None:
    if not SRC.exists():
        failures.append("src directory is missing")
        return

    src_root = SRC.resolve()
    for csproj in sorted(SRC.rglob("*.csproj")):
        project_root = ET.parse(csproj).getroot()
        package_refs: set[str] = set()
        tfms: list[str] = []

        for element in project_root.iter():
            name = local_name(element.tag)
            if name == "PackageReference":
                package_id = (element.attrib.get("Include") or element.attrib.get("Update") or "").strip()
                if package_id:
                    package_refs.add(package_id)
            elif name in {"TargetFramework", "TargetFrameworks"}:
                tfms.extend(part.strip() for part in (element.text or "").split(";") if part.strip())
            elif name == "ProjectReference":
                include = element.attrib.get("Include", "").strip()
                if include:
                    resolved = (csproj.parent / include).resolve()
                    try:
                        resolved.relative_to(src_root)
                    except ValueError:
                        failures.append(
                            f"{csproj.relative_to(ROOT)}: production ProjectReference escapes src/: {include}"
                        )

        unexpected_refs = package_refs - ALLOWED_PRODUCTION_PACKAGE_REFS
        if unexpected_refs:
            failures.append(
                f"{csproj.relative_to(ROOT)}: unexpected production PackageReference(s): {sorted(unexpected_refs)}"
            )

        for tfm in tfms:
            lowered = tfm.lower()
            if any(token in lowered for token in FORBIDDEN_PRODUCTION_TFM_TOKENS):
                failures.append(
                    f"{csproj.relative_to(ROOT)}: inactive-platform TFM leaked into production src/: {tfm}"
                )

    vendored_native: list[str] = []
    for path in SRC.rglob("*"):
        if not path.is_file():
            continue
        lowered_parts = [part.lower() for part in path.parts]
        if path.suffix.lower() in NATIVE_FILE_SUFFIXES or any(part.endswith(".framework") for part in lowered_parts):
            vendored_native.append(path.relative_to(ROOT).as_posix())
    if vendored_native:
        failures.append(f"vendored native binaries found under src/: {sorted(vendored_native)}")


def validate_lock_graph(lock: dict[str, object], failures: list[str]) -> tuple[str, dict[str, dict[str, object]]]:
    targets = lock.get("dependencies", {})
    if not isinstance(targets, dict) or len(targets) != 1:
        failures.append(f"expected exactly one target graph, got {list(targets) if isinstance(targets, dict) else targets!r}")
        return "UNKNOWN", {}

    target_name, raw_graph = next(iter(targets.items()))
    if target_name != EXPECTED_TARGET:
        failures.append(f"target mismatch: expected {EXPECTED_TARGET}, got {target_name}")
    if not isinstance(raw_graph, dict):
        failures.append("lockfile target graph is not an object")
        return str(target_name), {}

    graph: dict[str, dict[str, object]] = {
        str(package_id): node
        for package_id, node in raw_graph.items()
        if isinstance(node, dict)
    }
    if set(graph) != set(EXPECTED_GRAPH):
        failures.append(f"resolved graph mismatch: expected {sorted(EXPECTED_GRAPH)}, got {sorted(graph)}")

    for package_id, (expected_type, expected_version) in EXPECTED_GRAPH.items():
        node = graph.get(package_id)
        if node is None:
            continue
        actual_type = str(node.get("type", ""))
        actual_version = str(node.get("resolved", ""))
        if actual_type != expected_type:
            failures.append(f"{package_id}: dependency type {actual_type!r}, expected {expected_type!r}")
        if actual_version != expected_version:
            failures.append(f"{package_id}: resolved {actual_version!r}, expected {expected_version!r}")
        if expected_type == "Direct":
            requested = str(node.get("requested", ""))
            exact = f"[{expected_version}]"
            if requested != exact:
                failures.append(f"{package_id}: lockfile requested range must be exact {exact}, got {requested!r}")

    skia = graph.get("SkiaSharp", {})
    skia_dependencies = skia.get("dependencies", {}) if isinstance(skia, dict) else {}
    if skia_dependencies != {"SkiaSharp.NativeAssets.Android": "4.151.1"}:
        failures.append(
            "SkiaSharp transitive boundary mismatch: expected only SkiaSharp.NativeAssets.Android 4.151.1"
        )

    rejected_names = [name for name in graph if name.lower().startswith("procad") or name.lower().startswith("ixmilia")]
    if rejected_names:
        failures.append(f"rejected/test-only package leaked into Android probe graph: {sorted(rejected_names)}")

    return str(target_name), graph


def main() -> int:
    failures: list[str] = []
    validate_central_versions(failures)
    validate_production_source_boundary(failures)

    if not LOCK.exists():
        print(f"missing lockfile: {LOCK.relative_to(ROOT)}", file=sys.stderr)
        return 2

    lock = json.loads(LOCK.read_text(encoding="utf-8"))
    target_name, graph = validate_lock_graph(lock, failures)
    packages: list[dict[str, object]] = []

    with tempfile.TemporaryDirectory(prefix="stage02-nupkg-") as temp_dir:
        temp = Path(temp_dir)
        for package_id in sorted(graph, key=str.casefold):
            node = graph[package_id]
            version = str(node.get("resolved", ""))
            if not version:
                failures.append(f"{package_id}: missing resolved version")
                continue

            lower_id = package_id.lower()
            lower_version = version.lower()
            url = (
                f"https://api.nuget.org/v3-flatcontainer/{lower_id}/"
                f"{lower_version}/{lower_id}.{lower_version}.nupkg"
            )
            destination = temp / f"{lower_id}.{lower_version}.nupkg"
            try:
                urllib.request.urlretrieve(url, destination)
            except Exception as exc:
                failures.append(f"{package_id} {version}: nupkg download failed: {exc}")
                continue

            sha256 = hashlib.sha256(destination.read_bytes()).hexdigest()
            with zipfile.ZipFile(destination) as archive:
                names = archive.namelist()
                nuspec_names = [name for name in names if name.lower().endswith(".nuspec")]
                if len(nuspec_names) != 1:
                    failures.append(f"{package_id} {version}: expected one nuspec, got {nuspec_names}")
                    continue
                license_value = nuspec_license(archive.read(nuspec_names[0]))
                native_entries = sorted(
                    name
                    for name in names
                    if name.lower().endswith((".so", ".aar", ".jar", ".dylib"))
                    or ".framework/" in name.lower()
                )

            if license_value not in ALLOW_LICENSES:
                failures.append(f"{package_id} {version}: license {license_value!r} is not automatic GREEN")

            if package_id in {"ACadSharp", "SkiaSharp"} and native_entries:
                failures.append(f"{package_id} {version}: unexpected native entries: {native_entries}")
            if package_id == "SkiaSharp.NativeAssets.Android":
                if set(native_entries) != EXPECTED_ANDROID_NATIVE:
                    failures.append(
                        "SkiaSharp.NativeAssets.Android native inventory mismatch: "
                        f"expected {sorted(EXPECTED_ANDROID_NATIVE)}, got {native_entries}"
                    )
                if any(
                    re.search(r"ios|maccatalyst|osx|win|linux", entry, flags=re.IGNORECASE)
                    for entry in native_entries
                ):
                    failures.append(
                        f"SkiaSharp.NativeAssets.Android contains non-Android native entry: {native_entries}"
                    )

            packages.append(
                {
                    "id": package_id,
                    "version": version,
                    "dependency_type": node.get("type"),
                    "license": license_value,
                    "nupkg_sha256": sha256,
                    "native_entries": native_entries,
                }
            )

    manifest = {
        "schema": 2,
        "target": target_name,
        "allow_licenses": sorted(ALLOW_LICENSES),
        "packages": packages,
        "production_boundary": {
            "allowed_package_references": sorted(ALLOWED_PRODUCTION_PACKAGE_REFS),
            "forbidden_tfm_tokens": list(FORBIDDEN_PRODUCTION_TFM_TOKENS),
            "vendored_native_under_src": "FORBIDDEN",
        },
        "result": "PASS" if not failures else "FAIL",
        "failures": failures,
    }
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps(manifest, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    print(OUTPUT.read_text(encoding="utf-8"))

    if failures:
        for failure in failures:
            print(f"FAIL: {failure}", file=sys.stderr)
        return 1

    print("V02_EXACT_VERSION_POLICY_PASS")
    print("V02_ANDROID_BOUNDARY_PASS")
    print("STAGE02_PACKAGE_AUDIT_PASS")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
