<#
.SYNOPSIS
    One-shot V01 checkpoint closer for the remote GitHub execution protocol.
.DESCRIPTION
    Updates only V01/V02 checkpoint metadata and evidence after the authoritative
    V01 Android emulator run has already passed. This script is removed by its
    own commit and must not remain in the final feature branch.
#>
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

$TestedSha = '698c6e901672a736f2803894efb5bda34af08212'
$RunId = '32821991333'
$JobId = '97721878468'
$ArtifactId = '9553530359'
$ArtifactDigest = 'sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7'
$Date = '2026-08-25'
$FeatureBranch = 'v01-harden-android-emulator-gate'

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText((Join-Path $RepoRoot $Path), [System.Text.Encoding]::UTF8)
}

function Write-Utf8([string]$Path, [string]$Text) {
    $full = Join-Path $RepoRoot $Path
    $parent = Split-Path -Parent $full
    if (-not (Test-Path $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($full, $Text, $utf8NoBom)
}

function Replace-Line([string]$Text, [string]$Key, [string]$Value) {
    $pattern = '(?m)^' + [regex]::Escape($Key) + '.*$'
    if ($Text -notmatch $pattern) {
        throw "Required checkpoint key not found: $Key"
    }
    return [regex]::Replace($Text, $pattern, $Value, 1)
}

# ANDROID_DOGRULAMA_PLANI.md
$androidPlan = Read-Utf8 'ANDROID_DOGRULAMA_PLANI.md'
$androidPlan = Replace-Line $androidPlan 'CURRENT_VALIDATION_STAGE:' 'CURRENT_VALIDATION_STAGE: V02'
$androidPlan = Replace-Line $androidPlan 'CURRENT_STATUS:' 'CURRENT_STATUS: NOT_STARTED'
$androidPlan = Replace-Line $androidPlan 'NEXT_ACTION:' 'NEXT_ACTION: V02 - verify dependency, lockfile, license/vulnerability policy and Android artifact boundary; do not start V03 in the same turn'
$androidPlan = [regex]::Replace($androidPlan, '(?m)^### V01 .+$', '### V01 - Toolchain, runner and emulator infrastructure - `VALIDATED`', 1)
$v01Result = @"

V01 validation result ($Date): `VALIDATED`.

- Exact tested SHA: `$TestedSha`.
- GitHub Actions run/job: `$RunId` / `$JobId`, Release, self-hosted Windows runner.
- Toolchain doctor: .NET 10.0.400, maui-android, OpenJDK 21.0.12, Android API 36, Build-Tools 36.0.0, ADB 37.0.1, AVD `mobil-dwg-api36`.
- Executable harness markers: `STAGE04_CORE_CONTRACT_TESTS_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`.
- Emulator: Android 16 / API 36 / x86_64 / QEMU=1; Stage01Smoke Release APK install and cold launch `Status: ok`; live PID 3374.
- Screenshot: byte-safe PNG with full signature `89 50 4E 47 0D 0A 1A 0A`; artifact screenshot was opened and shows the running MAUI Stage01Smoke UI without a crash dialog.
- Crash/ANR: package/PID-scoped crash buffer empty; post-launch events contain create/start/resume/draw for PID 3374; `dumpsys activity lastanr` reports no ANR since boot.
- Artifact: `$ArtifactId`, `$ArtifactDigest`, 7 files, 271043-byte ZIP.
- Claim limit remains `INFRASTRUCTURE_SMOKE_ONLY`: this is not a real `MobilDwg.App` viewer/DWG/DXF fidelity PASS.
- Physical Android device differences remain deferred to the release device gate.
"@
if ($androidPlan -notmatch [regex]::Escape("Exact tested SHA: `$TestedSha`")) {
    $androidPlan = $androidPlan -replace '(?m)^### V02 ', ($v01Result + "`r`n### V02 ")
}
Write-Utf8 'ANDROID_DOGRULAMA_PLANI.md' $androidPlan

# Canonical plan checkpoint.
$canonical = Read-Utf8 'Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md'
$canonical = Replace-Line $canonical 'CURRENT_STAGE:' 'CURRENT_STAGE: V02 - Dependency, lockfile and Android artifact boundary'
$canonical = Replace-Line $canonical 'CURRENT_SUBSTEP:' 'CURRENT_SUBSTEP: V02.ready'
$canonical = Replace-Line $canonical 'STATUS:' 'STATUS: NOT_STARTED'
$canonical = Replace-Line $canonical 'LAST_VERIFIED_REVISION:' "LAST_VERIFIED_REVISION: $TestedSha - V01 exact Release emulator validation SHA; final closure changes are documentation/workflow-cleanup only"
$canonical = Replace-Line $canonical 'NEXT_ACTION:' 'NEXT_ACTION: Start V02 only: verify locked dependency graph, license/vulnerability policy and Android artifact boundary; do not start V03 in the same turn.'
$canonical = Replace-Line $canonical 'LAST_UPDATE:' "LAST_UPDATE: $Date"
Write-Utf8 'Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md' $canonical

# DEVAM.md checkpoint.
$devam = Read-Utf8 'DEVAM.md'
$devam = Replace-Line $devam 'ANDROID_VALIDATION_CURRENT:' 'ANDROID_VALIDATION_CURRENT: V02 - NOT_STARTED'
$devam = Replace-Line $devam 'CURRENT_GATE_TRUTH:' 'CURRENT_GATE_TRUTH: V01 hardened gate executes real executable harnesses, builds/installs/launches Stage01Smoke Release APK, requires numeric PID, verifies byte-safe PNG and package/PID crash plus post-launch ANR evidence'
$devam = Replace-Line $devam 'CURRENT_GATE_CLAIM_LIMIT:' 'CURRENT_GATE_CLAIM_LIMIT: INFRASTRUCTURE_SMOKE_ONLY; Stage01Smoke is not MobilDwg.App/viewer fidelity evidence'
$devam = Replace-Line $devam 'NEXT_ACTION:' 'NEXT_ACTION: V02 only - dependency/lockfile/license-vulnerability policy and Android artifact-boundary revalidation; do not start V03 in the same turn.'
Write-Utf8 'DEVAM.md' $devam

# gecmis.md checkpoint.
$history = Read-Utf8 'gecmis.md'
$history = Replace-Line $history 'ANDROID_VALIDATION_CURRENT:' 'ANDROID_VALIDATION_CURRENT: V02 - NOT_STARTED'
$history = Replace-Line $history 'ANDROID_VALIDATION_NEXT:' 'ANDROID_VALIDATION_NEXT: V02 - dependency, lockfile and Android artifact boundary'
$history = Replace-Line $history 'CURRENT_GATE_TRUTH:' 'CURRENT_GATE_TRUTH: V01 hardened Stage01Smoke infrastructure gate passed exact Release run with executable harnesses, byte-safe PNG, live PID and crash/ANR evidence'
$history = Replace-Line $history 'CURRENT_GATE_CLAIM_LIMIT:' 'CURRENT_GATE_CLAIM_LIMIT: INFRASTRUCTURE_SMOKE_ONLY - MobilDwg.App/viewer PASS is not claimed'
$history = Replace-Line $history 'NEXT_ACTION:' 'NEXT_ACTION: V02 only - revalidate dependency/lockfile/license-vulnerability policy and Android artifact boundary; do not start V03 in the same turn.'
$history = Replace-Line $history 'LAST_UPDATE:' "LAST_UPDATE: $Date"
$v01History = @"

### Android revalidation V01 - VALIDATED ($Date)

Exact tested SHA `$TestedSha` passed self-hosted Windows Android Emulator Release run `$RunId` / job `$JobId`. The hardened gate executed all Core/Rendering/Architecture executable harnesses and required markers, built and installed the temporary Stage01Smoke signed APK, cold-launched it on Android 16 API 36, required live PID 3374, captured and verified a byte-safe PNG, and found no package/PID crash or post-launch ANR. Artifact `$ArtifactId` digest `$ArtifactDigest` contains 7 evidence files. The screenshot was independently opened and shows the running MAUI Stage01Smoke screen. Scope is infrastructure smoke only; real MobilDwg.App viewer/fidelity and physical-device behavior are not claimed.
"@
if ($history -notmatch 'Android revalidation V01 - VALIDATED') {
    $history = $history + $v01History
}
Write-Utf8 'gecmis.md' $history

# New V01 evidence.
$evidence = @"
# Android Validation V01 - Toolchain, Runner and Emulator Infrastructure

Status: `VALIDATED`
Date: `$Date`
Execution context: `CHATGPT_REMOTE_GITHUB`
Exact tested SHA: `$TestedSha`

## Scope

V01 validates the GitHub -> self-hosted Windows -> Android Emulator infrastructure bridge and the gate evidence quality. It does not validate the real MobilDwg.App viewer, DWG/DXF fidelity, SAF behavior, physical Android devices, or iOS.

## Authoritative run

- Workflow: `.github/workflows/android-emulator-test.yml`
- Gate: `scripts/android-emulator-gate.ps1`
- Run ID: `$RunId`
- Job ID: `$JobId`
- Configuration: `Release`
- Result: `success`
- Gate marker: `ANDROID_EMULATOR_GATE_PASS`
- Claim marker: `CLAIM_LIMIT=INFRASTRUCTURE_SMOKE_ONLY`

## Toolchain and runner evidence

- Runner: `DESKTOP-PKLGPNQ-mobil-dwg-runner`, labels self-hosted/windows/android-test/mobil-dwg.
- .NET SDK: `10.0.400`.
- Workload: `maui-android`.
- Java: Microsoft OpenJDK `21.0.12` baseline (`21.0.12.1` runtime reported by the runner).
- Android Platform: API `36`.
- Android Build-Tools: `36.0.0`.
- ADB / Platform-Tools: `37.0.1`.
- AVD: `mobil-dwg-api36`.
- Emulator boot evidence: Android 16, API 36, x86_64, QEMU=1.

## Executable harness evidence

The hardened gate no longer treats `dotnet test MobilDwg.sln` as execution proof for the custom executable test projects. It builds the solution and explicitly executes the harness projects with marker assertions.

Observed markers:

- `STAGE04_CORE_CONTRACT_TESTS_PASS`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- Aggregate gate marker: `EXECUTABLE_HARNESS_MARKERS_PASS`

Solution Release build reported 0 warnings / 0 errors before harness execution.

## APK, launch and process evidence

The gate generated `com.smitelagwar.mobildwg.stage01smoke-Signed.apk`, installed package `com.smitelagwar.mobildwg.stage01smoke`, resolved the launcher activity and cold-launched it with `Status: ok`.

- Emulator serial: `emulator-5554`.
- Launch state: `COLD`.
- Total launch time reported by Android: 1410 ms.
- Required live process PID: `3374`.
- Events evidence contains process start/bind and activity create/start/resume/draw for the package/PID.

## Screenshot, crash and ANR evidence

The screenshot path was captured with byte-preserving ADB stdout handling and validated against the full PNG signature `89 50 4E 47 0D 0A 1A 0A`.

The downloaded artifact was independently inspected in this execution turn:

- `screenshots/emulator_launch.png`: 257013 bytes and opens successfully.
- Visual content: running default MAUI Stage01Smoke screen (`Home`, `Hello, World!`, `.NET Multi-platform App UI`, `Click me`); no crash dialog is visible.
- `crash-logcat.txt`: empty apart from encoding marker; no package/PID crash evidence.
- `anr-events.txt`: post-launch event stream includes the package/PID lifecycle; `dumpsys activity lastanr` says `<no ANR has occurred since boot>`.
- Process remains alive with the expected PID after evidence collection.

## Artifact

- Artifact ID: `$ArtifactId`
- Name: `android-emulator-result`
- Files: 7
- ZIP size: 271043 bytes
- Digest: `$ArtifactDigest`
- Evidence files: `summary.txt`, `logcat.txt`, `crash-logcat.txt`, `anr-events.txt`, `meminfo.txt`, `device-info.txt`, `screenshots/emulator_launch.png`.

## Precursor failures and false-PASS prevention

Earlier V01 hardening attempts failed before an authoritative PASS: doctor-output parsing was corrected, then a Windows PowerShell 5.1/BOM-less UTF-8 parser issue was removed by making the gate script ASCII-safe. These failures were not promoted to PASS. Run `$RunId` is the authoritative V01 success.

## Decision

`V01 = VALIDATED` for infrastructure scope only.

The gate is now strong enough to prove the pinned toolchain, runner, emulator bridge, executable harness execution, temporary MAUI APK install/launch, byte-safe screenshot, live PID, and package/PID-scoped crash/post-launch ANR checks for the exact tested SHA.

It is explicitly not evidence that `MobilDwg.App` is installable or that any DWG/DXF viewer behavior is correct. That boundary remains for later validation stages, beginning with the dependency/artifact audit in V02 and the real app shell in V04. Physical Android differences remain deferred to the release device gate. iOS remains inactive future scope.
"@
Write-Utf8 'docs/evidence/android-validation/V01.md' $evidence

# Append a concise execution log entry.
$logPath = 'docs/EXECUTION_LOG.md'
$log = Read-Utf8 $logPath
$logEntry = @"

## $Date - Android revalidation V01 validated

- Context: `CHATGPT_REMOTE_GITHUB`.
- Exact tested SHA: `$TestedSha`.
- Self-hosted Release run/job: `$RunId` / `$JobId` - success.
- Hardened gate now explicitly executes Core/Rendering/Architecture harnesses and verifies required markers.
- Stage01Smoke signed APK installed and cold-launched on Android 16 API 36; live PID 3374 required.
- Byte-safe screenshot passed full PNG signature and was independently opened; package/PID crash buffer empty; no post-launch ANR and `lastanr` reports none since boot.
- Artifact `$ArtifactId`, 7 files, digest `$ArtifactDigest`.
- Decision: `V01 VALIDATED`, claim limit `INFRASTRUCTURE_SMOKE_ONLY`; V02 is next but was not started in this turn.
"@
if ($log -notmatch 'Android revalidation V01 validated') {
    $log = $log + $logEntry
}
Write-Utf8 $logPath $log

# Remove the temporary close step and restore least privilege in the workflow.
$workflowPath = '.github/workflows/android-emulator-test.yml'
$workflow = Read-Utf8 $workflowPath
$workflow = $workflow.Replace('contents: write', 'contents: read')
$workflow = $workflow.Replace('persist-credentials: true', 'persist-credentials: false')
$workflow = [regex]::Replace($workflow, '(?ms)\r?\n\s*# V01_CLOSE_BEGIN.*?# V01_CLOSE_END\r?\n?', "`r`n")
Write-Utf8 $workflowPath $workflow

# Remove this one-shot script from the final branch.
Remove-Item -LiteralPath $PSCommandPath -Force

& git config user.name 'github-actions[bot]'
& git config user.email '41898282+github-actions[bot]@users.noreply.github.com'
& git add --all

$status = (& git status --porcelain | Out-String).Trim()
if (-not $status) {
    throw 'V01 checkpoint closer produced no changes.'
}

& git commit -m 'docs(v01): record validated emulator infrastructure'
if ($LASTEXITCODE -ne 0) { throw 'git commit failed' }

& git push origin "HEAD:refs/heads/$FeatureBranch"
if ($LASTEXITCODE -ne 0) { throw 'git push to V01 feature branch failed' }

Write-Host 'V01_CHECKPOINT_CLOSE_PASS'
