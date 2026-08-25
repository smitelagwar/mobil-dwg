#!/usr/bin/env bash
set -euo pipefail

test "$(dotnet --version)" = "10.0.400"

CACHE_ROOT="${1:?cache root required}"
EVIDENCE_PATH="${2:?evidence path required}"
LOCK_MODE="${STAGE05_LOCK_MODE:-locked}"

python scripts/stage03-validate-fixtures.py \
  --manifest fixtures/manifest/stage03-mini.json \
  --cache "$CACHE_ROOT" \
  --evidence "${CACHE_ROOT}/stage03-fixture-audit.json"

if [[ "$LOCK_MODE" == "generate" ]]; then
  dotnet restore src/MobilDwg.Cad/MobilDwg.Cad.csproj --force-evaluate
  cp src/MobilDwg.Cad/packages.lock.json "${CACHE_ROOT}/generated-MobilDwg.Cad.packages.lock.json"
elif [[ "$LOCK_MODE" == "locked" ]]; then
  dotnet restore src/MobilDwg.Cad/MobilDwg.Cad.csproj --locked-mode
else
  echo "Unsupported STAGE05_LOCK_MODE: $LOCK_MODE" >&2
  exit 2
fi

dotnet restore MobilDwg.sln

dotnet build MobilDwg.sln \
  --configuration Release \
  --no-restore \
  -warnaserror

dotnet run --project tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj \
  --configuration Release \
  --no-build

dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj \
  --configuration Release \
  --no-build

dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj \
  --configuration Release \
  --no-build

dotnet restore tools/Stage05.ParserProbe/Stage05.ParserProbe.csproj

dotnet build tools/Stage05.ParserProbe/Stage05.ParserProbe.csproj \
  --configuration Release \
  --no-restore \
  -warnaserror

dotnet run --project tools/Stage05.ParserProbe/Stage05.ParserProbe.csproj \
  --configuration Release \
  --no-build \
  -- \
  --manifest fixtures/manifest/stage03-mini.json \
  --cache "$CACHE_ROOT" \
  --evidence "$EVIDENCE_PATH"

# Preserve the human-readable graph as evidence, but do not parse localized CLI text.
dotnet list src/MobilDwg.Cad/MobilDwg.Cad.csproj package --include-transitive | tee "${CACHE_ROOT}/stage05-package-graph.txt"

python - <<'PY'
import json
import xml.etree.ElementTree as ET
from pathlib import Path

expected = "3.7.1"
props = ET.parse("Directory.Packages.props").getroot()
versions = {
    node.attrib.get("Include"): node.attrib.get("Version")
    for node in props.iter("PackageVersion")
}
actual_range = versions.get("ACadSharp")
if actual_range != f"[{expected}]":
    raise SystemExit(f"ACadSharp central exact version mismatch: {actual_range!r}")

lock = json.loads(Path("src/MobilDwg.Cad/packages.lock.json").read_text(encoding="utf-8"))
dep = lock["dependencies"]["net10.0"]["ACadSharp"]
if dep.get("type") != "Direct" or dep.get("resolved") != expected:
    raise SystemExit(f"ACadSharp lock mismatch: {dep!r}")

assets = json.loads(Path("src/MobilDwg.Cad/obj/project.assets.json").read_text(encoding="utf-8"))
if f"ACadSharp/{expected}" not in assets.get("libraries", {}):
    raise SystemExit("ACadSharp exact resolved package missing from project.assets.json")

print(f"STAGE05_ACADSHARP_PACKAGE_PASS central=[{expected}] resolved={expected}")
PY

echo "STAGE05_T3_PASS"
