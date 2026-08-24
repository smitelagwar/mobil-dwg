#!/usr/bin/env bash
set -euo pipefail

OUT_DIR="${1:-$RUNNER_TEMP/stage08-ios}"
mkdir -p "$OUT_DIR"

PROJECT="spikes/Stage08.iOS/Stage08.iOS.csproj"
TFM="net10.0-ios26.0"
RID="iossimulator-x64"
BUNDLE_ID="com.smitelagwar.mobildwg.stage08"
BUILD_LOG="$OUT_DIR/ios-release-build.log"
SIM_LOG="$OUT_DIR/ios-simulator.log"
PACKAGE_GRAPH="$OUT_DIR/package-graph.txt"
EVIDENCE_JSON="$OUT_DIR/stage08-evidence.json"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "STAGE08_HOST_BLOCKED expected=Darwin actual=$(uname -s)" >&2
  exit 10
fi

DOTNET_VERSION="$(dotnet --version)"
if [[ "$DOTNET_VERSION" != "10.0.400" ]]; then
  echo "STAGE08_DOTNET_MISMATCH expected=10.0.400 actual=$DOTNET_VERSION" >&2
  exit 11
fi

XCODE_VERSION="$(xcodebuild -version | tr '\n' ' ' | sed 's/[[:space:]]*$//')"
MACOS_VERSION="$(sw_vers -productVersion)"
ARCH="$(uname -m)"

echo "STAGE08_HOST_PASS macos=$MACOS_VERSION arch=$ARCH dotnet=$DOTNET_VERSION xcode=$XCODE_VERSION"

dotnet workload list | tee "$OUT_DIR/workloads.txt"
grep -Eq '(^|[[:space:]])ios([[:space:]]|$)' "$OUT_DIR/workloads.txt"

echo "STAGE08_IOS_WORKLOAD_PASS"

dotnet restore "$PROJECT" -r "$RID" | tee "$OUT_DIR/restore.log"
dotnet list "$PROJECT" package --include-transitive > "$PACKAGE_GRAPH"
grep -F 'ACadSharp' "$PACKAGE_GRAPH"
grep -F 'SkiaSharp' "$PACKAGE_GRAPH"

echo "STAGE08_EXACT_GRAPH_RECORDED"

set +e
dotnet build "$PROJECT" -c Release -f "$TFM" -r "$RID" --no-restore 2>&1 | tee "$BUILD_LOG"
BUILD_EXIT=${PIPESTATUS[0]}
set -e
if [[ "$BUILD_EXIT" -ne 0 ]]; then
  echo "STAGE08_IOS_RELEASE_BUILD_EXIT=$BUILD_EXIT" >&2
  exit "$BUILD_EXIT"
fi

echo "STAGE08_IOS_RELEASE_BUILD_PASS"

APP_PATH="$(find "spikes/Stage08.iOS/bin/Release/$TFM/$RID" -maxdepth 2 -type d -name '*.app' -print -quit)"
if [[ -z "$APP_PATH" ]]; then
  echo "STAGE08_APP_BUNDLE_NOT_FOUND" >&2
  exit 12
fi

APP_EXECUTABLE="$APP_PATH/MobilDwg.Stage08.iOS"
if [[ ! -f "$APP_EXECUTABLE" ]]; then
  APP_EXECUTABLE="$(find "$APP_PATH" -maxdepth 1 -type f -perm +111 -print -quit)"
fi
file "$APP_EXECUTABLE" | tee "$OUT_DIR/app-executable-file.txt"

grep -E 'IL[0-9]{4}' "$BUILD_LOG" > "$OUT_DIR/trimmer-warnings.txt" || true
grep -Ei 'reflection|dynamic code|requiresunreferencedcode' "$BUILD_LOG" > "$OUT_DIR/reflection-warnings.txt" || true
grep -Ei 'font|fonts' "$BUILD_LOG" > "$OUT_DIR/font-warnings.txt" || true
TRIMMER_WARNING_COUNT="$(wc -l < "$OUT_DIR/trimmer-warnings.txt" | tr -d ' ')"
REFLECTION_WARNING_COUNT="$(wc -l < "$OUT_DIR/reflection-warnings.txt" | tr -d ' ')"
FONT_WARNING_COUNT="$(wc -l < "$OUT_DIR/font-warnings.txt" | tr -d ' ')"

echo "STAGE08_WARNING_AUDIT trimmer=$TRIMMER_WARNING_COUNT reflection=$REFLECTION_WARNING_COUNT font=$FONT_WARNING_COUNT"

SIM_UDID="$(xcrun simctl list devices available --json | python3 -c '
import json,sys
j=json.load(sys.stdin)
for runtime, devices in j.get("devices",{}).items():
    for d in devices:
        if d.get("isAvailable") and d.get("name","").startswith("iPhone"):
            print(d["udid"])
            raise SystemExit(0)
raise SystemExit(1)
')"

if [[ -z "$SIM_UDID" ]]; then
  echo "STAGE08_SIMULATOR_BLOCKED no_available_iphone_simulator" >&2
  exit 13
fi

xcrun simctl boot "$SIM_UDID" >/dev/null 2>&1 || true
xcrun simctl bootstatus "$SIM_UDID" -b
xcrun simctl install "$SIM_UDID" "$APP_PATH"

set +e
xcrun simctl launch --console "$SIM_UDID" "$BUNDLE_ID" 2>&1 | tee "$SIM_LOG"
SIM_EXIT=${PIPESTATUS[0]}
set -e

grep -F 'STAGE08_IOS_SIMULATOR_PARSE_PASS' "$SIM_LOG"
grep -F 'STAGE08_IOS_SIMULATOR_SKIA_PASS' "$SIM_LOG"
grep -F 'STAGE08_IOS_SIMULATOR_SMOKE_PASS' "$SIM_LOG"

echo "STAGE08_IOS_SIMULATOR_LAUNCH_EXIT=$SIM_EXIT"
echo "STAGE08_IOS_NATIVE_SKIA_LOAD_PASS"

python3 - "$EVIDENCE_JSON" "$MACOS_VERSION" "$ARCH" "$DOTNET_VERSION" "$XCODE_VERSION" "$BUILD_EXIT" "$SIM_EXIT" "$TRIMMER_WARNING_COUNT" "$REFLECTION_WARNING_COUNT" "$FONT_WARNING_COUNT" <<'PY'
import json, sys
(path, macos, arch, dotnet, xcode, build_exit, sim_exit, trim_count, reflection_count, font_count) = sys.argv[1:]
obj = {
    "stage": "08",
    "host": {"macos": macos, "arch": arch, "dotnet": dotnet, "xcode": xcode},
    "dependency_line": {"ACadSharp": "3.7.1", "SkiaSharp": "4.151.1", "production_graph_modified": False},
    "ios_release_simulator_build_exit": int(build_exit),
    "simulator_launch_exit": int(sim_exit),
    "simulator_parse_pass": True,
    "simulator_skia_native_pass": True,
    "warning_counts": {
        "trimmer": int(trim_count),
        "reflection": int(reflection_count),
        "font": int(font_count),
    },
    "physical_iphone": "NOT_RUN_DEFERRED_EXTERNAL_GATE",
    "local_user_mac_inventory": "PENDING_USER_EVIDENCE",
}
with open(path, "w", encoding="utf-8") as f:
    json.dump(obj, f, indent=2, sort_keys=True)
    f.write("\n")
PY

cat "$EVIDENCE_JSON"
echo "STAGE08_IOS_FEASIBILITY_PARTIAL_PASS"
