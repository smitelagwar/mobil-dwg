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

[[ "$(uname -s)" == "Darwin" ]] || { echo "STAGE08_HOST_BLOCKED expected=Darwin actual=$(uname -s)" >&2; exit 10; }
ARCH="$(uname -m)"
case "$ARCH" in
  arm64) RID="iossimulator-arm64" ;;
  x86_64) RID="iossimulator-x64" ;;
  *) echo "STAGE08_UNSUPPORTED_MAC_ARCH arch=$ARCH" >&2; exit 14 ;;
esac

DOTNET_VERSION="$(dotnet --version)"
[[ "$DOTNET_VERSION" == "10.0.400" ]] || { echo "STAGE08_DOTNET_MISMATCH expected=10.0.400 actual=$DOTNET_VERSION" >&2; exit 11; }
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

APP_ROOT="spikes/Stage08.iOS/bin/Release/$TFM/$RID"
SIM_UDID="$(xcrun simctl list devices available --json | python3 -c '
import json,sys
j=json.load(sys.stdin)
for devices in j.get("devices",{}).values():
    for d in devices:
        if d.get("isAvailable") and d.get("name","").startswith("iPhone"):
            print(d["udid"]); raise SystemExit(0)
raise SystemExit(1)
' || true)"

if [[ -n "$SIM_UDID" ]]; then
  xcrun simctl boot "$SIM_UDID" >/dev/null 2>&1 || true
  xcrun simctl bootstatus "$SIM_UDID" -b || true
  trap 'xcrun simctl terminate "$SIM_UDID" "$BUNDLE_ID" >/dev/null 2>&1 || true; xcrun simctl shutdown "$SIM_UDID" >/dev/null 2>&1 || true' EXIT
fi

run_probe_app() {
  local app_path="$1" log_path="$2"
  [[ -n "$SIM_UDID" ]] || return 13
  xcrun simctl terminate "$SIM_UDID" "$BUNDLE_ID" >/dev/null 2>&1 || true
  xcrun simctl uninstall "$SIM_UDID" "$BUNDLE_ID" >/dev/null 2>&1 || true
  xcrun simctl install "$SIM_UDID" "$app_path" || return $?
  set +e
  xcrun simctl launch --console "$SIM_UDID" "$BUNDLE_ID" 2>&1 | tee "$log_path"
  local launch_exit=${PIPESTATUS[0]}
  set -e
  [[ "$launch_exit" -eq 0 ]] || return "$launch_exit"
  grep -F 'STAGE08_IOS_SIMULATOR_PARSE_PASS' "$log_path" >/dev/null || return 31
  grep -F 'STAGE08_IOS_SIMULATOR_SKIA_PASS' "$log_path" >/dev/null || return 32
  grep -F 'STAGE08_IOS_SIMULATOR_SMOKE_PASS' "$log_path" >/dev/null || return 33
  return 0
}

# 1) Baseline Release with iOS-supported linker disable switch.
set +e
dotnet build "$PROJECT" -c Release -f "$TFM" -r "$RID" --no-restore -p:MtouchLink=None 2>&1 | tee "$BASELINE_BUILD_LOG"
BASELINE_BUILD_EXIT=${PIPESTATUS[0]}
set -e
BASELINE_SIM_EXIT=-1
BASELINE_RUNTIME_PASS=false
BASELINE_BLOCKER=""
if [[ "$BASELINE_BUILD_EXIT" -eq 0 ]]; then
  BASELINE_APP="$(find "$APP_ROOT" -maxdepth 4 -type d -name '*.app' -print -quit)"
  if [[ -n "$BASELINE_APP" ]]; then
    BASELINE_EXECUTABLE="$(find "$BASELINE_APP" -maxdepth 1 -type f -perm +111 -print -quit)"
    [[ -z "$BASELINE_EXECUTABLE" ]] || file "$BASELINE_EXECUTABLE" | tee "$OUT_DIR/app-executable-file.txt"
    set +e; run_probe_app "$BASELINE_APP" "$BASELINE_SIM_LOG"; BASELINE_SIM_EXIT=$?; set -e
    [[ "$BASELINE_SIM_EXIT" -ne 0 ]] || BASELINE_RUNTIME_PASS=true
  else
    BASELINE_SIM_EXIT=16
  fi
else
  if grep -Fq 'unable to find utility "install_name_tool"' "$BASELINE_BUILD_LOG"; then
    BASELINE_BLOCKER="GITHUB_HOSTED_MACOS26_XCODE26_6_INSTALL_NAME_TOOL_MISSING"
    echo "STAGE08_BASELINE_BUILD_BLOCKED_HOSTED_RUNNER_TOOLCHAIN"
  else
    BASELINE_BLOCKER="BUILD_FAILURE_OTHER"
    echo "STAGE08_BASELINE_BUILD_FAIL_OTHER exit=$BASELINE_BUILD_EXIT"
  fi
fi

# 2) Trim compatibility probe with detailed warnings and warnings not promoted to errors.
set +e
dotnet build "$PROJECT" -c Release -f "$TFM" -r "$RID" --no-restore \
  -p:PublishTrimmed=true -p:TrimMode=partial -p:TrimmerSingleWarn=false -p:ILLinkTreatWarningsAsErrors=false \
  2>&1 | tee "$TRIM_BUILD_LOG"
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
TRIM_BLOCKER=""
if [[ "$TRIM_BUILD_EXIT" -eq 0 ]]; then
  TRIM_APP="$(find "$APP_ROOT" -maxdepth 4 -type d -name '*.app' -print -quit)"
  if [[ -n "$TRIM_APP" ]]; then
    set +e; run_probe_app "$TRIM_APP" "$TRIM_SIM_LOG"; TRIM_SIM_EXIT=$?; set -e
    [[ "$TRIM_SIM_EXIT" -ne 0 ]] || TRIM_RUNTIME_PASS=true
  else
    TRIM_SIM_EXIT=16
  fi
else
  if grep -Fq 'unable to find utility "install_name_tool"' "$TRIM_BUILD_LOG"; then
    TRIM_BLOCKER="GITHUB_HOSTED_MACOS26_XCODE26_6_INSTALL_NAME_TOOL_MISSING"
  elif grep -Fq "ACadSharp" "$TRIM_BUILD_LOG" && grep -Eq 'IL[0-9]{4}' "$TRIM_BUILD_LOG"; then
    TRIM_BLOCKER="ACADSHARP_TRIM_COMPATIBILITY"
  else
    TRIM_BLOCKER="BUILD_FAILURE_OTHER"
  fi
fi
echo "STAGE08_TRIM_RESULT build_exit=$TRIM_BUILD_EXIT warnings=$TRIMMER_WARNING_COUNT blocker=${TRIM_BLOCKER:-none}"

# 3) NativeAOT characterization. Failure is evidence, not converted to PASS.
set +e
dotnet publish "$PROJECT" -c Release -f "$TFM" -r "$RID" \
  -p:PublishAot=true -p:PublishTrimmed=true -p:TrimmerSingleWarn=false -p:ILLinkTreatWarningsAsErrors=false \
  2>&1 | tee "$AOT_PUBLISH_LOG"
AOT_PUBLISH_EXIT=${PIPESTATUS[0]}
set -e
grep -E 'IL(2|3)[0-9]{3}' "$AOT_PUBLISH_LOG" > "$OUT_DIR/aot-warnings.txt" || true
AOT_WARNING_COUNT="$(wc -l < "$OUT_DIR/aot-warnings.txt" | tr -d ' ')"
AOT_SIM_EXIT=-1
AOT_RUNTIME_PASS=false
AOT_BLOCKER=""
if [[ "$AOT_PUBLISH_EXIT" -eq 0 ]]; then
  AOT_APP="$(find "$APP_ROOT" -maxdepth 6 -type d -name '*.app' -print -quit)"
  if [[ -n "$AOT_APP" ]]; then
    set +e; run_probe_app "$AOT_APP" "$AOT_SIM_LOG"; AOT_SIM_EXIT=$?; set -e
    [[ "$AOT_SIM_EXIT" -ne 0 ]] || AOT_RUNTIME_PASS=true
  else
    AOT_SIM_EXIT=16
  fi
else
  if grep -Fq 'unable to find utility "install_name_tool"' "$AOT_PUBLISH_LOG"; then
    AOT_BLOCKER="GITHUB_HOSTED_MACOS26_XCODE26_6_INSTALL_NAME_TOOL_MISSING"
  elif grep -Fq "ACadSharp" "$AOT_PUBLISH_LOG" && grep -Eq 'IL(2|3)[0-9]{3}' "$AOT_PUBLISH_LOG"; then
    AOT_BLOCKER="ACADSHARP_NATIVEAOT_COMPATIBILITY"
  else
    AOT_BLOCKER="PUBLISH_FAILURE_OTHER"
  fi
fi
echo "STAGE08_NATIVEAOT_RESULT publish_exit=$AOT_PUBLISH_EXIT warnings=$AOT_WARNING_COUNT blocker=${AOT_BLOCKER:-none}"

python3 - "$EVIDENCE_JSON" "$MACOS_VERSION" "$ARCH" "$RID" "$DOTNET_VERSION" "$XCODE_VERSION" \
  "$BASELINE_BUILD_EXIT" "$BASELINE_SIM_EXIT" "$BASELINE_RUNTIME_PASS" "$BASELINE_BLOCKER" \
  "$TRIM_BUILD_EXIT" "$TRIM_SIM_EXIT" "$TRIM_RUNTIME_PASS" "$TRIM_BLOCKER" \
  "$AOT_PUBLISH_EXIT" "$AOT_SIM_EXIT" "$AOT_RUNTIME_PASS" "$AOT_BLOCKER" \
  "$TRIMMER_WARNING_COUNT" "$REFLECTION_WARNING_COUNT" "$FONT_WARNING_COUNT" "$AOT_WARNING_COUNT" <<'PY'
import json, sys
(
 path, macos, arch, rid, dotnet, xcode,
 bbuild, bsim, bruntime, bblock,
 tbuild, tsim, truntime, tblock,
 apublish, asim, aruntime, ablock,
 twarn, rwarn, fwarn, awarn
) = sys.argv[1:]
obj = {
  "stage": "08",
  "classification": "BLOCKED_PARTIAL_EVIDENCE",
  "host": {"macos": macos, "arch": arch, "rid": rid, "dotnet": dotnet, "xcode": xcode},
  "dependency_line": {
    "ACadSharp": "3.7.1", "SkiaSharp": "4.151.1", "SkiaSharp.NativeAssets.iOS": "4.151.1",
    "production_graph_modified": False
  },
  "baseline_release": {"build_exit": int(bbuild), "simulator_exit": int(bsim), "runtime_pass": bruntime == "true", "blocker": bblock or None},
  "trim_probe": {"build_exit": int(tbuild), "simulator_exit": int(tsim), "runtime_pass": truntime == "true", "blocker": tblock or None,
                 "warning_counts": {"trimmer": int(twarn), "reflection": int(rwarn), "font": int(fwarn)}},
  "nativeaot_probe": {"publish_exit": int(apublish), "simulator_exit": int(asim), "runtime_pass": aruntime == "true", "blocker": ablock or None, "warning_count": int(awarn)},
  "physical_iphone": "NOT_RUN_DEFERRED_EXTERNAL_GATE",
  "local_user_mac_inventory": "PENDING_USER_EVIDENCE",
  "hosted_runner_note": "Do not patch the hosted Xcode bundle to fabricate a pass. Re-run on a complete local/managed Mac toolchain."
}
with open(path, "w", encoding="utf-8") as f:
    json.dump(obj, f, indent=2, sort_keys=True); f.write("\n")
PY

cat "$EVIDENCE_JSON"
echo "STAGE08_IOS_FEASIBILITY_CHARACTERIZATION_COMPLETE"
