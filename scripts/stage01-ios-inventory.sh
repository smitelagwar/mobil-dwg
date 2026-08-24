#!/usr/bin/env bash
set -euo pipefail

APPLE_ACCESS="${APPLE_DEVELOPER_ACCESS:-unknown}"
case "$APPLE_ACCESS" in
  yes|no|unknown) ;;
  *)
    printf 'STAGE01_IOS_INVENTORY_FAIL: APPLE_DEVELOPER_ACCESS must be yes, no, or unknown\n' >&2
    exit 2
    ;;
esac

printf 'STAGE01_IOS_ACCESS_INVENTORY\n'
printf 'apple_developer_access=%s\n' "$APPLE_ACCESS"

if [[ "$(uname -s)" != "Darwin" ]]; then
  printf 'mac_access=NO_LOCAL_MAC\n'
  printf 'xcode_access=NOT_CHECKED\n'
  printf 'physical_iphone_access=NOT_CHECKED\n'
  printf 'codesigning_identity_count=NOT_CHECKED\n'
  printf 'inventory_complete=NO\n'
  printf 'reason=Run this helper on an accessible Mac, or record Mac access=NO manually in docs/STAGE_01_IOS_ACCESS_INVENTORY.md.\n'
  exit 0
fi

printf 'mac_access=YES\n'
printf 'macos_version=%s\n' "$(sw_vers -productVersion 2>/dev/null || printf 'unknown')"
printf 'mac_arch=%s\n' "$(uname -m)"

XCODE_ACCESS=NO
XCODE_VERSION=NOT_AVAILABLE
if command -v xcode-select >/dev/null 2>&1 && xcode-select -p >/dev/null 2>&1 && command -v xcodebuild >/dev/null 2>&1; then
  XCODE_ACCESS=YES
  XCODE_VERSION="$(xcodebuild -version 2>/dev/null | paste -sd ';' - || printf 'unknown')"
fi
printf 'xcode_access=%s\n' "$XCODE_ACCESS"
printf 'xcode_version=%s\n' "$XCODE_VERSION"

IPHONE_COUNT=0
if [[ "$XCODE_ACCESS" == "YES" ]] && command -v xcrun >/dev/null 2>&1; then
  DEVICE_LIST="$(xcrun xctrace list devices 2>/dev/null || true)"
  IPHONE_COUNT="$(awk '/== Simulators ==/{exit} /iPhone/ {count++} END {print count+0}' <<<"$DEVICE_LIST")"
fi
if [[ "$IPHONE_COUNT" -gt 0 ]]; then
  printf 'physical_iphone_access=YES\n'
else
  printf 'physical_iphone_access=NO\n'
fi
printf 'physical_iphone_count=%s\n' "$IPHONE_COUNT"

IDENTITY_COUNT=0
if command -v security >/dev/null 2>&1; then
  IDENTITY_COUNT="$(security find-identity -v -p codesigning 2>/dev/null | grep -c '"' || true)"
fi
printf 'codesigning_identity_count=%s\n' "$IDENTITY_COUNT"

if [[ "$APPLE_ACCESS" == "unknown" ]]; then
  printf 'inventory_complete=NO\n'
  printf 'reason=Set APPLE_DEVELOPER_ACCESS=yes or no after manually checking account access; do not provide Apple ID, password, team ID, token, or certificate material.\n'
else
  printf 'inventory_complete=YES\n'
fi
