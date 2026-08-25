#!/usr/bin/env bash
set -euo pipefail

test "$(dotnet --version)" = "10.0.400"

CACHE_ROOT="${1:?cache root required}"
EVIDENCE_PATH="${2:?evidence path required}"
REPO_ROOT="$(pwd)"

rm -rf "$CACHE_ROOT"
mkdir -p "$CACHE_ROOT"

python scripts/stage03-validate-fixtures.py \
  --manifest fixtures/manifest/stage03-mini.json \
  --cache "$CACHE_ROOT/fixtures" \
  --evidence "$CACHE_ROOT/stage03-fixture-audit.json"

dotnet restore src/MobilDwg.Cad/MobilDwg.Cad.csproj --locked-mode
dotnet restore MobilDwg.sln

dotnet build MobilDwg.sln \
  --configuration Release \
  --no-restore \
  -warnaserror

dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj \
  --configuration Release \
  --no-build

dotnet restore tools/Stage06.OpenFlowProbe/Stage06.OpenFlowProbe.csproj

dotnet build tools/Stage06.OpenFlowProbe/Stage06.OpenFlowProbe.csproj \
  --configuration Release \
  --no-restore \
  -warnaserror

dotnet run --project tools/Stage06.OpenFlowProbe/Stage06.OpenFlowProbe.csproj \
  --configuration Release \
  --no-build \
  -- \
  --cache-root "$CACHE_ROOT/open-flow" \
  --dwg "$CACHE_ROOT/fixtures/remote/acadsharp-ac1015-dwg.dwg" \
  --dxf "$REPO_ROOT/fixtures/public/synthetic/synthetic_turkish_basic_ac1015.dxf" \
  --evidence "$EVIDENCE_PATH"

grep -F 'Task.Run(' src/MobilDwg.App/Opening/CadFileOpenCoordinator.cs
grep -F 'FilePicker.Default.PickAsync' spikes/Stage06.Android/Stage06MainPage.cs
grep -F 'picked.OpenReadAsync()' spikes/Stage06.Android/Stage06MainPage.cs
if grep -F 'FullPath' spikes/Stage06.Android/Stage06MainPage.cs; then
  echo 'Stage 06 MAUI adapter must not depend on provider physical paths.' >&2
  exit 1
fi
if grep -R -F 'TakePersistableUriPermission' spikes/Stage06.Android src/MobilDwg.App/Opening; then
  echo 'Stage 06 immediate-copy flow must not take a persistable URI grant.' >&2
  exit 1
fi

echo 'STAGE06_STREAM_NOT_PATH_PASS'
echo 'STAGE06_NO_PERSISTABLE_GRANT_NEEDED_PASS'

SMOKE_ROOT="$CACHE_ROOT/android-smoke/Stage06AndroidSmoke"
mkdir -p "$(dirname "$SMOKE_ROOT")"
dotnet new maui -n Stage06AndroidSmoke -o "$SMOKE_ROOT"
SMOKE_CSPROJ="$SMOKE_ROOT/Stage06AndroidSmoke.csproj"

sed -i 's/>21.0<\/SupportedOSPlatformVersion>/>24.0<\/SupportedOSPlatformVersion>/' "$SMOKE_CSPROJ"
sed -i 's#<ApplicationId>com.companyname.stage06androidsmoke</ApplicationId>#<ApplicationId>com.smitelagwar.mobildwg.stage06smoke</ApplicationId>#' "$SMOKE_CSPROJ"
grep -F '>24.0</SupportedOSPlatformVersion>' "$SMOKE_CSPROJ"
grep -F '<ApplicationId>com.smitelagwar.mobildwg.stage06smoke</ApplicationId>' "$SMOKE_CSPROJ"

dotnet add "$SMOKE_CSPROJ" reference "$REPO_ROOT/src/MobilDwg.App/MobilDwg.App.csproj"
dotnet add "$SMOKE_CSPROJ" reference "$REPO_ROOT/src/MobilDwg.Cad/MobilDwg.Cad.csproj"
cp spikes/Stage06.Android/Stage06MainPage.cs "$SMOKE_ROOT/Stage06MainPage.cs"

grep -F 'local:MainPage' "$SMOKE_ROOT/AppShell.xaml"
sed -i 's/local:MainPage/local:Stage06MainPage/g' "$SMOKE_ROOT/AppShell.xaml"
grep -F 'local:Stage06MainPage' "$SMOKE_ROOT/AppShell.xaml"

dotnet restore "$SMOKE_CSPROJ"
dotnet build "$SMOKE_CSPROJ" -f net10.0-android -c Debug --no-restore -warnaserror
echo 'STAGE06_ANDROID_DEBUG_BUILD_PASS'
dotnet build "$SMOKE_CSPROJ" -f net10.0-android -c Release --no-restore -warnaserror
echo 'STAGE06_ANDROID_RELEASE_BUILD_PASS'

MANIFEST="$SMOKE_ROOT/obj/Debug/net10.0-android/android/manifest/AndroidManifest.xml"
test -f "$MANIFEST"
grep -E 'minSdkVersion="24"|minSdkVersion="24.0"' "$MANIFEST"
grep -E 'targetSdkVersion="36"|targetSdkVersion="36.0"' "$MANIFEST"
grep -F 'package="com.smitelagwar.mobildwg.stage06smoke"' "$MANIFEST"
if grep -E 'android.permission.(READ_EXTERNAL_STORAGE|WRITE_EXTERNAL_STORAGE|MANAGE_EXTERNAL_STORAGE)' "$MANIFEST"; then
  echo 'Stage 06 smoke unexpectedly requests broad storage permission.' >&2
  exit 1
fi

echo 'STAGE06_NO_BROAD_STORAGE_PERMISSION_PASS'

python - "$EVIDENCE_PATH" "$MANIFEST" <<'PY'
import json
import sys
from pathlib import Path

evidence_path = Path(sys.argv[1])
manifest_path = Path(sys.argv[2])
evidence = json.loads(evidence_path.read_text(encoding='utf-8'))
evidence.update({
    'maui_filepicker_adapter': 'PASS',
    'android_debug_build': 'PASS',
    'android_release_build': 'PASS',
    'android_manifest_min_sdk': 24,
    'android_manifest_target_sdk': 36,
    'broad_storage_permission': 'ABSENT',
    'persistable_uri_grant': 'NOT_TAKEN_NOT_NEEDED_IMMEDIATE_PRIVATE_COPY',
    'physical_android_device': 'DEFERRED_EXTERNAL_GATE',
    'manifest_path': str(manifest_path),
})
evidence_path.write_text(json.dumps(evidence, indent=2, sort_keys=True) + '\n', encoding='utf-8')
PY

echo 'STAGE06_CI_GATE_PASS'
