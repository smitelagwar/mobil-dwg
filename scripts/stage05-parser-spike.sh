#!/usr/bin/env bash
set -euo pipefail

test "$(dotnet --version)" = "10.0.400"

CACHE_ROOT="${1:?cache root required}"
EVIDENCE_PATH="${2:?evidence path required}"

python scripts/stage03-validate-fixtures.py \
  --manifest fixtures/manifest/stage03-mini.json \
  --cache "$CACHE_ROOT" \
  --evidence "${CACHE_ROOT}/stage03-fixture-audit.json"

# The production parser adapter has its own committed exact lock file.
dotnet restore src/MobilDwg.Cad/MobilDwg.Cad.csproj --locked-mode

dotnet restore MobilDwg.sln

dotnet build MobilDwg.sln \
  --configuration Release \
  --no-restore \
  /warnaserror

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
  /warnaserror

dotnet run --project tools/Stage05.ParserProbe/Stage05.ParserProbe.csproj \
  --configuration Release \
  --no-build \
  -- \
  --manifest fixtures/manifest/stage03-mini.json \
  --cache "$CACHE_ROOT" \
  --evidence "$EVIDENCE_PATH"

dotnet list src/MobilDwg.Cad/MobilDwg.Cad.csproj package --include-transitive | tee "${CACHE_ROOT}/stage05-package-graph.txt"
grep -E 'ACadSharp[[:space:]]+3\.7\.1' "${CACHE_ROOT}/stage05-package-graph.txt"

echo "STAGE05_T3_PASS"
