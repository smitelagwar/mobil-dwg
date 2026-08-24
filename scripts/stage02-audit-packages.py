#!/usr/bin/env python3
from __future__ import annotations

import hashlib
import json
import sys
import tempfile
import urllib.request
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

ALLOW_LICENSES = {"MIT", "Apache-2.0", "BSD-2-Clause", "BSD-3-Clause", "ISC", "0BSD"}
EXPECTED_DIRECT = {"ACadSharp": "3.7.1", "SkiaSharp": "4.151.1"}
LOCK = Path("compliance/Stage02.DependencyProbe/packages.lock.json")
OUTPUT = Path("compliance/stage02-package-manifest.json")


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


def main() -> int:
    if not LOCK.exists():
        print(f"missing lockfile: {LOCK}", file=sys.stderr)
        return 2

    lock = json.loads(LOCK.read_text(encoding="utf-8"))
    targets = lock.get("dependencies", {})
    if len(targets) != 1:
        print(f"expected exactly one target graph, got {list(targets)}", file=sys.stderr)
        return 3

    target_name, graph = next(iter(targets.items()))
    packages: list[dict[str, object]] = []
    failures: list[str] = []

    direct = {
        package_id: node.get("resolved")
        for package_id, node in graph.items()
        if node.get("type") == "Direct"
    }
    if direct != EXPECTED_DIRECT:
        failures.append(f"direct package mismatch: expected {EXPECTED_DIRECT}, got {direct}")

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
            except Exception as exc:  # CI evidence should expose fetch failures.
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
                failures.append(
                    f"{package_id} {version}: license {license_value!r} is not automatic GREEN"
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
        "schema": 1,
        "target": target_name,
        "allow_licenses": sorted(ALLOW_LICENSES),
        "packages": packages,
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
    print("STAGE02_PACKAGE_AUDIT_PASS")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
