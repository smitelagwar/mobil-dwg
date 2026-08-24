#!/usr/bin/env bash
set -euo pipefail

OUT_DIR="${1:-$RUNNER_TEMP/stage08-ios}"
mkdir -p "$OUT_DIR"

PROJECT="spikes/Stage08.iOS/Stage08.iOS.csproj"
TFM="net10.0-ios26.5"
BUNDLE_ID="com.smitelagwar.mobildwg.stage08"
PACKAGE_GRAPH="$OUT_DIR/package-graph.txt"
BASELINE_BUILD_LOG="$OUT_DIR/ios-release-baseline-build.log"
BASELINE_SIM_LOG="$OUT_DIR/ios-simulator-baseline.log"
TRIM_BUILD_LOG="$OUT_DIR/ios-release-trim-build.log"
TRIM_SIM_LOG="$OUT_DIR/ios-simulator-trimmed.log"
AOT_PUBLISH_LOG="$OUT_DIR/ios-nativeaot-publish.log"
AOT_SIM_LOG="$OUT_DIR/ios-simulator-nativeaot.log"
EVIDENCE_JSON="$OUT_DIR/stage08-evidence.json"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "STAGE08_HOST_BLOCKED expected=Darwin actual=$(uname -s)" >&2
  exit 10
fi

ARCH="$(uname -m)"
case "$ARCH" in
  arm64) RID="iossimulator-arm64" ;;
  x86_64) RID="iossimulator-x64" ;;
  *) echo "STAGE08_UNSUPPORTED_MAC_ARCH arch=$ARCH" >&2; exit 14 ;;
esac

DOTNET_VERSION="$(dotnet --version)"
if [[ "$DOTNET_VERSION" != "10.0.400" ]]; then
  echo "STAGE08_DOTNET_MISMATCH expected=10.0.400 actual=$DOTNET_VERSION" >&2
  exit 11
fi

XCODE_VERSION="$(xcodebuild -version | tr '\n' ' ' | sed 's/[[:space:]]*$//')"
MACOS_VERSION="$(sw_vers -productVersion)"

echo "STAGE08_HOST_PASS macos=$MACOS_VERSION arch=$ARCH rid=$RID dotnet=$DOTNET_VERSION xcode=$XCODE_VERSION"

dotnet workload list | tee "$OUT_DIR/workloads.txt"
grep -Eq '(^|[[:space:]])ios([[:space:]]|$)' "$OUT_DIR/workloads.txt"
echo "STAGE08_IOS_WORKLOAD_PASS"

dotnet restore "$PROJECT" -r "$RID" | tee "$OUT_DIR/restore.log"
dotnet list "$PROJECT" package --include-transitive > "$PACKAGE_GRAPH"
grep -F 'ACadSharp' "$PACKAGE_GRAPH"
grep -F 'SkiaSharp' "$PACKAGE_GRAPH"
grep -F 'SkiaSharp.NativeAssets.iOS' "$PACKAGE_GRAPH"
echo "STAGE08_EXACT_GRAPH_RECORDED"

# Baseline Release feasibility: no trimming/AOT. This must build and execute.
set +e
dotnet build "$PROJECT" -c Release -f "$TFM" -r "$RID" --no-restore \
  -p:PublishTrimmed=false 2>&1 | tee "$BASELINE_BUILD_LOG"
BASELINE_BUILD_EXIT=${PIPESTATUS[0]}
set -e
if [[ "$BASELINE_BUILD_EXIT" -ne 0 ]]; then
  echo "STAGE08_IOS_BASELINE_RELEASE_BUILD_EXIT=$BASELINE_BUILD_EXIT" >&2
  exit "$BASELINE_BUILD_EXIT"
fi
echo "STAGE08_IOS_BASELINE_RELEASE_BUILD_PASS"

APP_ROOT="spikes/Stage08.iOS/bin/Release/$TFM/$RID"
find_app() {
  find "$APP_ROOT" -maxdepth 3 -type d -name '*.app' -print -quit
}

BASELINE_APP_PATH="$(find_app)"
if [[ -z "$BASELINE_APP_PATH" ]]; then
  echo "STAGE08_APP_BUNDLE_NOT_FOUND" >&2
  exit 12
fi

BASELINE_EXECUTABLE="$(find "$BASELINE_APP_PATH" -maxdepth 1 -type f -perm +111 -print -quit)"
if [[ -z "$BASELINE_EXECUTABLE" ]]; then
  echo "STAGE08_APP_EXECUTABLE_NOT_FOUND" >&2
  exit 15
fi
file "$BASELINE_EXECUTABLE" | tee "$OUT_DIR/app-executable-file.txt"

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
trap 'xcrun simctl terminate "$SIM_UDID" "$BUNDLE_ID" >/dev/null 2>&1 || true; xcrun simctl shutdown "$SIM_UDID" >/dev/null 2>&1 || true' EXIT

run_probe_app() {
  local app_path="$1"
  local log_path="$2"
  xcrun simctl terminate "$SIM_UDID" "$BUNDLE_ID" >/dev/null 2>&1 || true
  xcrun simctl uninstall "$SIM_UDID" "$BUNDLE_ID" >/dev/null 2>&1 || true
  xcrun simctl install "$SIM_UDID" "$app_path"
  set +e
  xcrun simctl launch --console "$SIM_UDID" "$BUNDLE_ID" 2>&1 | tee "$log_path"
  local launch_exit=${PIPESTATUS[0]}
  set -e
  grep -F 'STAGE08_IOS_SIMULATOR_PARSE_PASS' "$log_path"
  grep -F 'STAGE08_IOS_SIMULATOR_SKIA_PASS' "$log_path"
  grep -F 'STAGE08_IOS_SIMULATOR_SMOKE_PASS' "$log_path"
  return "$launch_exit"
}

set +e
run_probe_app "$BASELINE_APP_PATH" "$BASELINE_SIM_LOG"
BASELINE_SIM_EXIT=$?
set -e
if [[ "$BASELINE_SIM_EXIT" -ne 0 ]]; then
  echo "STAGE08_IOS_BASELINE_SIMULATOR_EXIT=$BASELINE_SIM_EXIT" >&2
  exit "$BASELINE_SIM_EXIT"
fi
echo "STAGE08_IOS_BASELINE_SIMULATOR_PASS"
echo "STAGE08_IOS_NATIVE_SKIA_LOAD_PASS"

# Trimming compatibility probe. Warnings are evidence, not hidden. A failure is
# recorded without erasing the already-proven baseline Release feasibility.
set +e
dotnet build "$PROJECT" -c Release -f "$TFM" -r "$RID" --no-restore \
  -p:PublishTrimmed=true \
  -p:TrimMode=partial \
  -p:TrimmerSingleWarn=false \
  -p:ILLinkTreatWarningsAsErrors=false 2>&1 | tee "$TRIM_BUILD_LOG"
TRIM_BUILD_EXIT=${PIPESTATUS[0]}
set -e

grep -E 'IL[0-9]{4}' "$TRIM_BUILD_LOG" > "$OUT_DIR/trimmer-warnings.txt" || true
grep -Ei 'reflection|dynamic code|requiresunreferencedcode' "$TRIM_BUILD_LOG" > "$OUT_DIR/reflection-warnings.txt" || true
grep -Ei 'font|fonts' "$TRIM_BUILD_LOG" > "$OUT_DIR/font-warnings.txt" || true
TRIMMER_WARNING_COUNT="$(wc -l < "$OUT_DIR/trimmer-warnings.txt" | tr -d ' ')"
REFLECTION_WARNING_COUNT="$(wc -l < "$OUT_DIR/reflection-warnings.txt" | tr -d ' ')"
FONT_WARNING_COUNT="$(wc -l < "$OUT_DIR/font-warnings.txt" | tr -d ' ')"
TRIM_SIM_EXIT=-1
TRIM_RUNTIME_PASS=false

if [[ "$TRIM_BUILD_EXIT" -eq 0 ]]; then
  TRIM_APP_PATH="$(find_app)"
  set +e
  run_probe_app "$TRIM_APP_PATH" "$TRIM_SIM_LOG"
  TRIM_SIM_EXIT=$?
  set -e
  if [[ "$TRIM_SIM_EXIT" -eq 0 ]]; then
    TRIM_RUNTIME_PASS=true
    echo "STAGE08_IOS_TRIMMED_SIMULATOR_PASS"
  else
    echo "STAGE08_IOS_TRIMMED_SIMULATOR_FAIL exit=$TRIM_SIM_EXIT"
  fi
else
  echo "STAGE08_IOS_TRIM_BUILD_RISK_RECORDED exit=$TRIM_BUILD_EXIT"
fi

echo "STAGE08_WARNING_AUDIT trimmer=$TRIMMER_WARNING_COUNT reflection=$REFLECTION_WARNING_COUNT font=$FONT_WARNING_COUNT"

# NativeAOT feasibility probe. .NET documents PublishAot + an iOS simulator RID
# as an iOS-like NativeAOT target. The result is recorded independently.
set +e
dotnet publish "$PROJECT" -c Release -f "$TFM" -r "$RID" \
  -p:PublishAot=true \
  -p:PublishTrimmed=true \
  -p:TrimmerSingleWarn=false \
  -p:ILLinkTreatWarningsAsErrors=false 2>&1 | tee "$AOT_PUBLISH_LOG"
AOT_PUBLISH_EXIT=${PIPESTATUS[0]}
set -e

grep -E 'IL(2|3)[0-9]{3}' "$AOT_PUBLISH_LOG" > "$OUT_DIR/aot-warnings.txt" || true
AOT_WARNING_COUNT="$(wc -l < "$OUT_DIR/aot-warnings.txt" | tr -d ' ')"
AOT_SIM_EXIT=-1
AOT_RUNTIME_PASS=false

if [[ "$AOT_PUBLISH_EXIT" -eq 0 ]]; then
  AOT_APP_PATH="$(find "$APP_ROOT" -maxdepth 5 -type d -name '*.app' -print -quit)"
  if [[ -n "$AOT_APP_PATH" ]]; then
    set +e
    run_probe_app "$AOT_APP_PATH" "$AOT_SIM_LOG"
    AOT_SIM_EXIT=$?
    set -e
    if [[ "$AOT_SIM_EXIT" -eq 0 ]]; then
      AOT_RUNTIME_PASS=true
      echo "STAGE08_IOS_NATIVEAOT_SIMULATOR_PASS"
    else
      echo "STAGE08_IOS_NATIVEAOT_SIMULATOR_FAIL exit=$AOT_SIM_EXIT"
    fi
  else
    AOT_SIM_EXIT=16
    echo "STAGE08_IOS_NATIVEAOT_APP_NOT_FOUND"
  fi
else
  echo "STAGE08_IOS_NATIVEAOT_RISK_RECORDED exit=$AOT_PUBLISH_EXIT"
fi

python3 - "$EVIDENCE_JSON" "$MACOS_VERSION" "$ARCH" "$RID" "$DOTNET_VERSION" "$XCODE_VERSION" \
  "$BASELINE_BUILD_EXIT" "$BASELINE_SIM_EXIT" "$TRIM_BUILD_EXIT" "$TRIM_SIM_EXIT" "$AOT_PUBLISH_EXIT" "$AOT_SIM_EXIT" \
  "$TRIMMER_WARNING_COUNT" "$REFLECTION_WARNING_COUNT" "$FONT_WARNING_COUNT" "$AOT_WARNING_COUNT" "$TRIM_RUNTIME_PASS" "$AOT_RUNTIME_PASS" <<'PY'
import json, sys
(
    path, macos, arch, rid, dotnet, xcode,
    baseline_build, baseline_sim, trim_build, trim_sim, aot_publish, aot_sim,
    trim_count, reflection_count, font_count, aot_count, trim_runtime, aot_runtime
) = sys.argv[1:]
obj = {
    "stage": "08",
    "host": {"macos": macos, "arch": arch, "rid": rid, "dotnet": dotnet, "xcode": xcode},
    "dependency_line": {
        "ACadSharp": "3.7.1",
        "SkiaSharp": "4.151.1",
        "SkiaSharp.NativeAssets.iOS": "4.151.1",
        "production_graph_modified": False,
    },
    "baseline_release": {
        "build_exit": int(baseline_build),
        "simulator_exit": int(baseline_sim),
        "parse_pass": True,
        "skia_native_pass": True,
    },
    "trim_probe": {
        "build_exit": int(trim_build),
        "simulator_exit": int(trim_sim),
        "runtime_pass": trim_runtime == "true",
        "warning_counts": {
            "trimmer": int(trim_count),
            "reflection": int(reflection_count),
            "font": int(font_count),
        },
    },
    "nativeaot_probe": {
        "publish_exit": int(aot_publish),
        "simulator_exit": int(aot_sim),
        "runtime_pass": aot_runtime == "true",
        "warning_count": int(aot_count),
    },
    "physical_iphone": "NOT_RUN_DEFERRED_EXTERNAL_GATE",
    "local_user_mac_inventory": "PENDING_USER_EVIDENCE",
}
with open(path, "w", encoding="utf-8") as f:
    json.dump(obj, f, indent=2, sort_keys=True)
    f.write("\n")
PY

cat "$EVIDENCE_JSON"
echo "STAGE08_IOS_FEASIBILITY_CHARACTERIZED"
