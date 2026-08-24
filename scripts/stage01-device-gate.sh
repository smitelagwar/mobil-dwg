#!/usr/bin/env bash
set -euo pipefail

fail() {
  printf 'STAGE01_DEVICE_GATE_FAIL: %s\n' "$*" >&2
  exit 1
}

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || fail "required command not found: $1"
}

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

require_cmd dotnet
require_cmd java
require_cmd adb
require_cmd sed
require_cmd grep
require_cmd awk
require_cmd find

DOTNET_VERSION="$(dotnet --version)"
[[ "$DOTNET_VERSION" == "10.0.400" ]] || fail "dotnet version is $DOTNET_VERSION; expected 10.0.400"

dotnet workload list | grep -q 'maui-android' || fail "maui-android workload is not installed"

JAVA_VERSION="$(java -version 2>&1)"
grep -q '21\.0\.12' <<<"$JAVA_VERSION" || fail "Java 21.0.12 is required"

ADB_VERSION="$(adb version)"
grep -q 'Version 37\.0\.1' <<<"$ADB_VERSION" || fail "ADB / Platform-Tools 37.0.1 is required"

SDK_ROOT="${ANDROID_SDK_ROOT:-${ANDROID_HOME:-}}"
[[ -n "$SDK_ROOT" ]] || fail "ANDROID_SDK_ROOT or ANDROID_HOME must be set"
[[ -f "$SDK_ROOT/platforms/android-36/android.jar" ]] || fail "Android SDK Platform 36 is not installed under $SDK_ROOT"
[[ -d "$SDK_ROOT/build-tools/36.0.0" ]] || fail "Android Build-Tools 36.0.0 is not installed under $SDK_ROOT"

adb start-server >/dev/null

DEVICE_SERIALS=()
while IFS= read -r candidate; do
  [[ -n "$candidate" ]] && DEVICE_SERIALS[${#DEVICE_SERIALS[@]}]="$candidate"
done < <(adb devices | awk 'NR > 1 && $2 == "device" { print $1 }')

if [[ -n "${ANDROID_SERIAL:-}" ]]; then
  SERIAL="$ANDROID_SERIAL"
  FOUND=0
  for candidate in "${DEVICE_SERIALS[@]:-}"; do
    if [[ "$candidate" == "$SERIAL" ]]; then
      FOUND=1
      break
    fi
  done
  [[ "$FOUND" == "1" ]] || fail "ANDROID_SERIAL does not identify an adb device in state=device"
else
  [[ "${#DEVICE_SERIALS[@]}" -eq 1 ]] || fail "connect exactly one authorized adb device, or set ANDROID_SERIAL"
  SERIAL="${DEVICE_SERIALS[0]}"
fi

IS_EMULATOR="$(adb -s "$SERIAL" shell getprop ro.kernel.qemu 2>/dev/null | tr -d '\r')"
[[ "$IS_EMULATOR" != "1" ]] || fail "connected target is an emulator; a physical Android device is required"

MANUFACTURER="$(adb -s "$SERIAL" shell getprop ro.product.manufacturer | tr -d '\r')"
MODEL="$(adb -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ANDROID_RELEASE="$(adb -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"
ANDROID_API="$(adb -s "$SERIAL" shell getprop ro.build.version.sdk | tr -d '\r')"

WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/mobil-dwg-stage01.XXXXXX")"
trap 'rm -rf "$WORK_DIR"' EXIT
APP_DIR="$WORK_DIR/Stage01Smoke"

printf 'Creating clean MAUI smoke app...\n'
dotnet new maui -n Stage01Smoke -o "$APP_DIR" >/dev/null

CSPROJ="$APP_DIR/Stage01Smoke.csproj"
sed -i.bak 's/>21\.0<\/SupportedOSPlatformVersion>/>24.0<\/SupportedOSPlatformVersion>/' "$CSPROJ"
rm -f "$CSPROJ.bak"
grep -q '>24\.0</SupportedOSPlatformVersion>' "$CSPROJ" || fail "failed to pin Android minimum API to 24.0"

printf 'Building Debug...\n'
dotnet build "$CSPROJ" -f net10.0-android -c Debug --no-restore
printf 'Building Release...\n'
dotnet build "$CSPROJ" -f net10.0-android -c Release --no-restore

MANIFEST="$APP_DIR/obj/Debug/net10.0-android/android/manifest/AndroidManifest.xml"
[[ -f "$MANIFEST" ]] || fail "generated Android manifest not found"
grep -Eq 'minSdkVersion="24"|minSdkVersion="24\.0"' "$MANIFEST" || fail "generated manifest does not contain minSdkVersion=24"
grep -Eq 'targetSdkVersion="36"|targetSdkVersion="36\.0"' "$MANIFEST" || fail "generated manifest does not contain targetSdkVersion=36"

DEBUG_APK="$(find "$APP_DIR/bin/Debug/net10.0-android" -type f -name '*-Signed.apk' -print -quit)"
if [[ -z "$DEBUG_APK" ]]; then
  DEBUG_APK="$(find "$APP_DIR/bin/Debug/net10.0-android" -type f -name '*.apk' -print -quit)"
fi
[[ -n "$DEBUG_APK" && -f "$DEBUG_APK" ]] || fail "Debug APK not found"

PACKAGE_NAME="com.companyname.stage01smoke"
printf 'Installing Debug APK on physical device...\n'
adb -s "$SERIAL" install -r "$DEBUG_APK"
adb -s "$SERIAL" shell pm path "$PACKAGE_NAME" | grep -q '^package:' || fail "package install could not be verified"

LAUNCHER_COMPONENT="$(adb -s "$SERIAL" shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER "$PACKAGE_NAME" 2>/dev/null | tr -d '\r' | tail -n 1)"
[[ "$LAUNCHER_COMPONENT" == */* ]] || fail "launcher activity could not be resolved"

printf 'Launching smoke app...\n'
LAUNCH_OUTPUT="$(adb -s "$SERIAL" shell am start -W "$LAUNCHER_COMPONENT")"
grep -q 'Status: ok' <<<"$LAUNCH_OUTPUT" || fail "Android activity launch did not report Status: ok"

printf '\nSTAGE01_DEVICE_GATE_PASS\n'
printf 'dotnet=%s\n' "$DOTNET_VERSION"
printf 'java=21.0.12\n'
printf 'adb=37.0.1\n'
printf 'android_sdk=36\n'
printf 'build_tools=36.0.0\n'
printf 'maui_android=installed\n'
printf 'manifest=minSdk24,targetSdk36\n'
printf 'device_state=device,physical\n'
printf 'device=%s %s; Android %s; API %s\n' "$MANUFACTURER" "$MODEL" "$ANDROID_RELEASE" "$ANDROID_API"
printf 'debug_build=PASS\n'
printf 'release_build=PASS\n'
printf 'install=PASS\n'
printf 'launch=PASS\n'
