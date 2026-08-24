#!/usr/bin/env bash
set -euo pipefail

WORK_ROOT="${1:?work root required}"
EVIDENCE_PATH="${2:?evidence path required}"
PIN_FILE="spikes/ProCad.Android/source-pin.json"

rm -rf "$WORK_ROOT"
mkdir -p "$WORK_ROOT" "$(dirname "$EVIDENCE_PATH")"

readarray -t PINS < <(python - "$PIN_FILE" <<'PY'
import json, sys
p = json.load(open(sys.argv[1], encoding='utf-8'))
print(p['procad']['repository'])
print(p['procad']['commit'])
print(p['submodules']['external/ACadSharp']['commit'])
print(p['submodules']['external/ACadSharp']['official_upstream_repository'])
print(p['submodules']['external/ACadSharp']['approved_mobil_dwg_source_commit'])
print(p['submodules']['external/ACadSharp']['approved_source_ahead_by_commits'])
print(p['submodules']['external/ProEdit']['commit'])
PY
)
PROCAD_REPO="${PINS[0]}"
PROCAD_COMMIT="${PINS[1]}"
ACAD_COMMIT="${PINS[2]}"
ACAD_OFFICIAL="${PINS[3]}"
ACAD_APPROVED="${PINS[4]}"
ACAD_EXPECTED_AHEAD="${PINS[5]}"
PROEDIT_COMMIT="${PINS[6]}"
PROCAD_DIR="$WORK_ROOT/ProCad"

# Source-only spike: never add ProCad to the mobil-dwg production graph.
git clone --no-checkout "$PROCAD_REPO" "$PROCAD_DIR"
git -C "$PROCAD_DIR" checkout --detach "$PROCAD_COMMIT"
git -C "$PROCAD_DIR" submodule update --init --recursive

test "$(git -C "$PROCAD_DIR" rev-parse HEAD)" = "$PROCAD_COMMIT"
test "$(git -C "$PROCAD_DIR/external/ACadSharp" rev-parse HEAD)" = "$ACAD_COMMIT"
test "$(git -C "$PROCAD_DIR/external/ProEdit" rev-parse HEAD)" = "$PROEDIT_COMMIT"
grep -F 'MIT License' "$PROCAD_DIR/LICENSE"
grep -F 'MIT License' "$PROCAD_DIR/external/ACadSharp/LICENSE"
grep -F 'MIT License' "$PROCAD_DIR/external/ProEdit/LICENSE"
echo 'STAGE07_SOURCE_PIN_PASS'

# Resolve lineage against the official upstream repository. The pinned fork SHA
# must be an actual official upstream commit; then measure drift to the approved
# mobil-dwg 3.7.1 source baseline.
if ! git -C "$PROCAD_DIR/external/ACadSharp" remote | grep -qx official; then
  git -C "$PROCAD_DIR/external/ACadSharp" remote add official "$ACAD_OFFICIAL"
fi
git -C "$PROCAD_DIR/external/ACadSharp" fetch --no-tags official "$ACAD_APPROVED"
git -C "$PROCAD_DIR/external/ACadSharp" cat-file -e "$ACAD_COMMIT^{commit}"
git -C "$PROCAD_DIR/external/ACadSharp" cat-file -e "$ACAD_APPROVED^{commit}"
MERGE_BASE="$(git -C "$PROCAD_DIR/external/ACadSharp" merge-base "$ACAD_COMMIT" "$ACAD_APPROVED")"
test "$MERGE_BASE" = "$ACAD_COMMIT"
ACAD_AHEAD="$(git -C "$PROCAD_DIR/external/ACadSharp" rev-list --count "$ACAD_COMMIT..$ACAD_APPROVED")"
test "$ACAD_AHEAD" = "$ACAD_EXPECTED_AHEAD"
echo "STAGE07_ACAD_LINEAGE_PASS official_same_sha=$ACAD_COMMIT approved_ahead=$ACAD_AHEAD"

# Source graph and package-band audit.
grep -F '<VersionPrefix Condition="'"'"'$(VersionPrefix)'"'"' == '"'"''"'"' and '"'"'$(Version)'"'"' == '"'"''"'"'">0.1.1</VersionPrefix>' "$PROCAD_DIR/Directory.Build.targets"
grep -F '<PackageVersion Include="SkiaSharp" Version="3.119.4" />' "$PROCAD_DIR/Directory.Packages.props"
grep -F '<PackageVersion Include="SkiaSharp.Views.Maui.Controls" Version="4.147.0-preview.2.1" />' "$PROCAD_DIR/Directory.Packages.props"
grep -F '..\external\ACadSharp\src\ACadSharp\ACadSharp.csproj' "$PROCAD_DIR/ProCad.Rendering/ProCad.Rendering.csproj"
grep -F '..\external\ACadSharp\src\ACadSharp\ACadSharp.csproj' "$PROCAD_DIR/ProCad.Core/ProCad.Core.csproj"
echo 'STAGE07_SOURCE_GRAPH_PASS'

# Check whether the package IDs declared by the pinned source are actually
# available as 0.1.1 on nuget.org. Absence is evidence, not a harness failure.
NUGET_JSON="$WORK_ROOT/nuget-availability.json"
python - "$NUGET_JSON" <<'PY'
import json, sys
json.dump({}, open(sys.argv[1], 'w', encoding='utf-8'))
PY
for PACKAGE in ProCadSharp.Core ProCadSharp.Rendering ProCadSharp.Controls ProCadSharp.Controls.Skia ProCadSharp.Controls.Maui; do
  LOWER="$(printf '%s' "$PACKAGE" | tr '[:upper:]' '[:lower:]')"
  INDEX="$WORK_ROOT/${LOWER}.json"
  if curl -fsSL --retry 3 "https://api.nuget.org/v3-flatcontainer/${LOWER}/index.json" -o "$INDEX"; then
    python - "$NUGET_JSON" "$PACKAGE" "$INDEX" <<'PY'
import json, sys
out, package, idx = sys.argv[1:]
data = json.load(open(out, encoding='utf-8'))
versions = json.load(open(idx, encoding='utf-8')).get('versions', [])
data[package] = {'available': True, 'versions': versions, 'has_0_1_1': '0.1.1' in versions}
json.dump(data, open(out, 'w', encoding='utf-8'), indent=2, sort_keys=True)
PY
  else
    python - "$NUGET_JSON" "$PACKAGE" <<'PY'
import json, sys
out, package = sys.argv[1:]
data = json.load(open(out, encoding='utf-8'))
data[package] = {'available': False, 'versions': [], 'has_0_1_1': False}
json.dump(data, open(out, 'w', encoding='utf-8'), indent=2, sort_keys=True)
PY
  fi
done
echo 'STAGE07_NUGET_SOURCE_GRAPH_RECORDED'

# Deterministic precision gate. ProCad converts ACadSharp double XYZ to
# System.Numerics.Vector2 by direct float casts. Reproduce that exact numeric
# boundary for a building-scale point and a survey-origin + millimetre detail.
grep -F 'return new Vector2((float)point.X, (float)point.Y);' "$PROCAD_DIR/ProCad.Rendering/RenderTransformUtils.cs"
PRECISION_JSON="$WORK_ROOT/precision.json"
python - "$PRECISION_JSON" <<'PY'
import json, math, struct, sys

def f32(v):
    return struct.unpack('<f', struct.pack('<f', float(v)))[0]

def case(name, origin, detail):
    a, b = f32(origin), f32(origin + detail)
    observed = float(b - a)
    return {
        'name': name,
        'origin': origin,
        'detail': detail,
        'float_origin': a,
        'float_origin_plus_detail': b,
        'observed_delta': observed,
        'collapsed': observed == 0.0,
        'relative_delta_error': None if detail == 0 else abs(observed-detail)/abs(detail),
    }

small = case('small-building-mm-detail', 100.0, 0.001)
survey = case('survey-origin-mm-detail', 5_000_000.0, 0.001)
result = {'cases': [small, survey], 'precision_gate': 'FAIL' if survey['collapsed'] else 'PASS'}
json.dump(result, open(sys.argv[1], 'w', encoding='utf-8'), indent=2, sort_keys=True)
if not survey['collapsed']:
    raise SystemExit('expected survey-origin millimetre detail to expose float precision loss')
print('STAGE07_FLOAT_PRECISION_BLOCKER_REPRODUCED')
PY

# Reuse gate: pinned MAUI CadViewer supports one-pointer pan but contains no
# pinch gesture path. Record absence as a technical reuse gap.
grep -F 'case SKTouchAction.Moved:' "$PROCAD_DIR/ProCad.Controls.Maui/CadViewer.cs"
PINCH_PRESENT=false
if grep -R -E 'PinchGestureRecognizer|PinchUpdated|ScaleGesture|pinch' "$PROCAD_DIR/ProCad.Controls.Maui" --include='*.cs' --include='*.xaml' -i; then
  PINCH_PRESENT=true
fi
if "$PINCH_PRESENT"; then
  echo 'STAGE07_PINCH_PRESENT'
else
  echo 'STAGE07_PINCH_REUSE_GAP_RECORDED'
fi

# Candidate build: source project first, then a clean MAUI APK referencing only
# the isolated source checkout. A candidate build failure is a ProCad NO-GO
# signal and is recorded rather than treated as a harness failure.
BUILD_LOG="$WORK_ROOT/procad-android-build.log"
SMOKE_LOG="$WORK_ROOT/procad-maui-smoke.log"
set +e
dotnet restore "$PROCAD_DIR/ProCad.Controls.Maui/ProCad.Controls.Maui.csproj" -p:TargetFramework=net10.0-android >"$BUILD_LOG" 2>&1
RESTORE_EXIT=$?
if [ "$RESTORE_EXIT" -eq 0 ]; then
  dotnet build "$PROCAD_DIR/ProCad.Controls.Maui/ProCad.Controls.Maui.csproj" -f net10.0-android -c Release --no-restore >>"$BUILD_LOG" 2>&1
  SOURCE_BUILD_EXIT=$?
else
  SOURCE_BUILD_EXIT=$RESTORE_EXIT
fi
set -e

SMOKE_BUILD_EXIT=99
SMOKE_APK=""
if [ "$SOURCE_BUILD_EXIT" -eq 0 ]; then
  SMOKE_DIR="$WORK_ROOT/Stage07ProCadAndroidSmoke"
  dotnet new maui -n Stage07ProCadAndroidSmoke -o "$SMOKE_DIR" >"$SMOKE_LOG" 2>&1
  SMOKE_CSPROJ="$SMOKE_DIR/Stage07ProCadAndroidSmoke.csproj"
  sed -i 's/>21.0<\/SupportedOSPlatformVersion>/>24.0<\/SupportedOSPlatformVersion>/' "$SMOKE_CSPROJ"
  sed -i 's#<ApplicationId>com.companyname.stage07procadandroidsmoke</ApplicationId>#<ApplicationId>com.smitelagwar.mobildwg.stage07procadsmoke</ApplicationId>#' "$SMOKE_CSPROJ"
  dotnet add "$SMOKE_CSPROJ" reference "$PROCAD_DIR/ProCad.Controls.Maui/ProCad.Controls.Maui.csproj" >>"$SMOKE_LOG" 2>&1
  cat > "$SMOKE_DIR/Stage07CompileProbe.cs" <<'CS'
using ProCad.Controls.Maui;
namespace Stage07ProCadAndroidSmoke;
internal static class Stage07CompileProbe
{
    internal static object CreateViewer() => new CadViewer();
}
CS
  set +e
  dotnet restore "$SMOKE_CSPROJ" -p:TargetFramework=net10.0-android >>"$SMOKE_LOG" 2>&1
  SMOKE_RESTORE_EXIT=$?
  if [ "$SMOKE_RESTORE_EXIT" -eq 0 ]; then
    dotnet build "$SMOKE_CSPROJ" -f net10.0-android -c Release --no-restore >>"$SMOKE_LOG" 2>&1
    SMOKE_BUILD_EXIT=$?
  else
    SMOKE_BUILD_EXIT=$SMOKE_RESTORE_EXIT
  fi
  set -e
  if [ "$SMOKE_BUILD_EXIT" -eq 0 ]; then
    SMOKE_APK="$(find "$SMOKE_DIR/bin/Release/net10.0-android" -name '*.apk' -type f | head -n 1 || true)"
  fi
fi

PACKAGE_GRAPH="$WORK_ROOT/source-package-graph.txt"
if [ "$SOURCE_BUILD_EXIT" -eq 0 ]; then
  dotnet list "$PROCAD_DIR/ProCad.Controls.Maui/ProCad.Controls.Maui.csproj" package --include-transitive > "$PACKAGE_GRAPH" 2>&1 || true
else
  : > "$PACKAGE_GRAPH"
fi

# Exact pinned candidate is NO-GO once the deterministic survey/mm precision
# blocker is reproduced. Build/pinch/preview data remain supporting evidence.
python - "$EVIDENCE_PATH" "$PIN_FILE" "$NUGET_JSON" "$PRECISION_JSON" "$PROCAD_COMMIT" "$ACAD_COMMIT" "$ACAD_APPROVED" "$ACAD_AHEAD" "$PROEDIT_COMMIT" "$SOURCE_BUILD_EXIT" "$SMOKE_BUILD_EXIT" "$PINCH_PRESENT" "$SMOKE_APK" <<'PY'
import json, sys
(evidence_path, pin_path, nuget_path, precision_path, procad, acad, approved,
 ahead, proedit, source_build, smoke_build, pinch, apk) = sys.argv[1:]
pins = json.load(open(pin_path, encoding='utf-8'))
nuget = json.load(open(nuget_path, encoding='utf-8'))
precision = json.load(open(precision_path, encoding='utf-8'))
result = {
  'stage': '07',
  'decision': 'NO-GO',
  'decision_scope': 'exact pinned ProCad source candidate, unpatched',
  'procad_commit': procad,
  'acadsharp_submodule_commit': acad,
  'acadsharp_official_same_commit': True,
  'acadsharp_mobil_dwg_approved_source_commit': approved,
  'acadsharp_approved_ahead_by_commits': int(ahead),
  'proedit_commit': proedit,
  'licenses': {'ProCad': 'MIT', 'ACadSharp': 'MIT', 'ProEdit': 'MIT'},
  'source_package_versions': pins['pinned_source_packages'],
  'nuget_package_availability': nuget,
  'source_android_build_exit': int(source_build),
  'maui_release_smoke_build_exit': int(smoke_build),
  'maui_release_apk': apk or None,
  'maui_pinch_path_present': pinch.lower() == 'true',
  'precision': precision,
  'hard_blockers': [
    'survey-origin millimetre detail collapses at the direct double-to-float RenderScene boundary'
  ],
  'additional_risks': [
    'pinned ACadSharp baseline is 592 official commits behind the mobil-dwg approved 3.7.1 source baseline',
    'pinned MAUI CadViewer has one-pointer pan but no pinch implementation in its MAUI source',
    'pinned source mixes SkiaSharp 3.119.4 with SkiaSharp.Views.Maui.Controls 4.147.0-preview.2.1'
  ],
  'physical_android_t3': 'NOT_RUN_AFTER_DETERMINISTIC_BLOCKER',
  'production_graph_modified': False,
}
json.dump(result, open(evidence_path, 'w', encoding='utf-8'), indent=2, sort_keys=True)
PY

echo "STAGE07_SOURCE_BUILD_EXIT=$SOURCE_BUILD_EXIT"
echo "STAGE07_MAUI_SMOKE_BUILD_EXIT=$SMOKE_BUILD_EXIT"
echo 'STAGE07_DECISION_NO_GO_PASS'
