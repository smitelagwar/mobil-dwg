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
    "Microsoft.Maui.Controls": "10.0.100",
    "IxMilia.Dxf": "0.8.4",
}
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
ALLOWED_PRODUCTION_PACKAGE_REFS = {"ACadSharp", "SkiaSharp", "Microsoft.Maui.Controls"}
FORBIDDEN_PRODUCTION_TFM_TOKENS = ("-ios", "-maccatalyst", "-windows")
NATIVE_FILE_SUFFIXES = {".so", ".aar", ".jar", ".dylib"}

ROOT = Path(__file__).resolve().parents[1]
LOCK = ROOT / "compliance/Stage02.DependencyProbe/packages.lock.json"
OUTPUT = ROOT / "compliance/stage02-package-manifest.json"
CPM = ROOT / "Directory.Packages.props"
SRC = ROOT / "src"


def local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def fail_append(failures: list[str], message: str) -> None:
    failures.append(message)


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
        fail_append(failures, f"missing central package file: {CPM.relative_to(ROOT)}")
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
        fail_append(failures, f"central package set mismatch: expected {sorted(EXPECTED_CENTRAL)}, got {sorted(versions)}")
    for package_id, expected_version in EXPECTED_CENTRAL.items():
        actual = versions.get(package_id)
        exact = f"[{expected_version}]"
        if actual != exact:
            fail_append(failures, f"{package_id}: central version must use strict NuGet exact range {exact}, got {actual!r}")
        if actual and ("*" in actual or "," in actual or "latest" in actual.lower()):
            fail_append(failures, f"{package_id}: floating/open version syntax is forbidden: {actual!r}")


def validate_production_source_boundary(failures: list[str]) -> None:
    src_root = SRC.resolve()
    for csproj in sorted(SRC.rglob("*.csproj")):
        root = ET.parse(csproj).getroot()
        package_refs: set[str] = set()
        tfms: list[str] = []
        for element in root.iter():
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
                        fail_append(failures, f"{csproj.relative_to(ROOT)}: production ProjectReference escapes src/: {include}")
        unexpected = package_refs - ALLOWED_PRODUCTION_PACKAGE_REFS
        if unexpected:
            fail_append(failures, f"{csproj.relative_to(ROOT)}: unexpected production PackageReference(s): {sorted(unexpected)}")
        for tfm in tfms:
            lowered = tfm.lower()
            if any(token in lowered for token in FORBIDDEN_PRODUCTION_TFM_TOKENS):
                fail_append(failures, f"{csproj.relative_to(ROOT)}: inactive-platform TFM leaked into production src/: {tfm}")
    vendored: list[str] = []
    for path in SRC.rglob("*"):
        if not path.is_file():
            continue
        lowered_parts = [part.lower() for part in path.parts]
        if path.suffix.lower() in NATIVE_FILE_SUFFIXES or any(part.endswith(".framework") for part in lowered_parts):
            vendored.append(path.relative_to(ROOT).as_posix())
    if vendored:
        fail_append(failures, f"vendored native binaries found under src/: {sorted(vendored)}")


def validate_lock_graph(lock: dict[str, object], failures: list[str]) -> tuple[str, dict[str, dict[str, object]]]:
    targets = lock.get("dependencies", {})
    if not isinstance(targets, dict) or len(targets) != 1:
        fail_append(failures, f"expected exactly one target graph, got {list(targets) if isinstance(targets, dict) else targets!r}")
        return "UNKNOWN", {}
    target_name, raw_graph = next(iter(targets.items()))
    if target_name != EXPECTED_TARGET:
        fail_append(failures, f"target mismatch: expected {EXPECTED_TARGET}, got {target_name}")
    if not isinstance(raw_graph, dict):
        fail_append(failures, "lockfile target graph is not an object")
        return str(target_name), {}
    graph = {str(package_id): node for package_id, node in raw_graph.items() if isinstance(node, dict)}
    if set(graph) != set(EXPECTED_GRAPH):
        fail_append(failures, f"resolved graph mismatch: expected {sorted(EXPECTED_GRAPH)}, got {sorted(graph)}")
    for package_id, (expected_type, expected_version) in EXPECTED_GRAPH.items():
        node = graph.get(package_id)
        if node is None:
            continue
        if str(node.get("type", "")) != expected_type:
            fail_append(failures, f"{package_id}: dependency type {node.get('type')!r}, expected {expected_type!r}")
        if str(node.get("resolved", "")) != expected_version:
            fail_append(failures, f"{package_id}: resolved {node.get('resolved')!r}, expected {expected_version!r}")
        if expected_type == "Direct":
            exact = f"[{expected_version}]"
            if str(node.get("requested", "")) != exact:
                fail_append(failures, f"{package_id}: lockfile requested range must be exact {exact}, got {node.get('requested')!r}")
    skia_dependencies = graph.get("SkiaSharp", {}).get("dependencies", {})
    if skia_dependencies != {"SkiaSharp.NativeAssets.Android": "4.151.1"}:
        fail_append(failures, "SkiaSharp transitive boundary mismatch: expected only SkiaSharp.NativeAssets.Android 4.151.1")
    rejected = [name for name in graph if name.lower().startswith("procad") or name.lower().startswith("ixmilia")]
    if rejected:
        fail_append(failures, f"rejected/test-only package leaked into Android probe graph: {sorted(rejected)}")
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
            lower_id = package_id.lower()
            lower_version = version.lower()
            url = f"https://api.nuget.org/v3-flatcontainer/{lower_id}/{lower_version}/{lower_id}.{lower_version}.nupkg"
            destination = temp / f"{lower_id}.{lower_version}.nupkg"
            try:
                urllib.request.urlretrieve(url, destination)
            except Exception as exc:
                fail_append(failures, f"{package_id} {version}: nupkg download failed: {exc}")
                continue
            sha256 = hashlib.sha256(destination.read_bytes()).hexdigest()
            with zipfile.ZipFile(destination) as archive:
                names = archive.namelist()
                nuspecs = [name for name in names if name.lower().endswith(".nuspec")]
                if len(nuspecs) != 1:
                    fail_append(failures, f"{package_id} {version}: expected one nuspec, got {nuspecs}")
                    continue
                license_value = nuspec_license(archive.read(nuspecs[0]))
                native_entries = sorted(name for name in names if name.lower().endswith((".so", ".aar", ".jar", ".dylib")) or ".framework/" in name.lower())
            if license_value not in ALLOW_LICENSES:
                fail_append(failures, f"{package_id} {version}: license {license_value!r} is not automatic GREEN")
            if package_id in {"ACadSharp", "SkiaSharp"} and native_entries:
                fail_append(failures, f"{package_id} {version}: unexpected native entries: {native_entries}")
            if package_id == "SkiaSharp.NativeAssets.Android":
                if set(native_entries) != EXPECTED_ANDROID_NATIVE:
                    fail_append(failures, f"SkiaSharp.NativeAssets.Android native inventory mismatch: expected {sorted(EXPECTED_ANDROID_NATIVE)}, got {native_entries}")
                if any(re.search(r"ios|maccatalyst|osx|win|linux", entry, re.IGNORECASE) for entry in native_entries):
                    fail_append(failures, f"SkiaSharp.NativeAssets.Android contains non-Android native entry: {native_entries}")
            packages.append({
                "id": package_id,
                "version": version,
                "dependency_type": node.get("type"),
                "license": license_value,
                "nupkg_sha256": sha256,
                "native_entries": native_entries,
            })
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
