#!/usr/bin/env bash
set -euo pipefail

test "$(dotnet --version)" = "10.0.400"

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

echo "STAGE04_T0_PASS"
