<#
.SYNOPSIS
    AŞAMA 15 real MobilDwg.App API36 Dimension / Leader / Hatch acceptance gate.
.DESCRIPTION
    Builds a validation-only Release APK with -p:A15Validation=true, executes
    Anonymous block (*D...) expansion, procedural dimensions (Aligned, Rotated Linear, Radial, Diametric),
    degenerate dimension guards (identical defpoints, NaN points), Leader & MultiLeader generation,
    Hatch boundary loop auto-closure within 1mm, broken boundary diagnostics (> 1mm),
    EvenOdd nested island handling, ANSI31 clipped pattern hatch line generation, and real Skia CAD rendering
    inside the real app process, verifies markers and UI hierarchy, captures PNG evidence,
    and validates package-scoped crash/ANR/liveness.
#>
param(
    [string]$Configuration = "Release",
    [string]$ArtifactsDir = "artifacts/a15-android-dim-hatch"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "A15_FAIL: $Message" -ForegroundColor Red
    exit 1
}

function Require-ExitCode([string]$Step) {
    if ($LASTEXITCODE -ne 0) { Fail "$Step failed with exit code $LASTEXITCODE" }
}

function Invoke-AdbBinaryToFile {
    param(
        [Parameter(Mandatory = $true)][string]$AdbPath,
        [Parameter(Mandatory = $true)][string]$Serial,
        [Parameter(Mandatory = $true)][string]$OutputPath
    )
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $AdbPath
    $startInfo.Arguments = "-s $Serial exec-out screencap -p"
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    if (-not $process.Start()) { Fail "failed to start adb screencap" }
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $stream = [System.IO.File]::Create($OutputPath)
    try { $process.StandardOutput.BaseStream.CopyTo($stream) }
    finally { $stream.Dispose() }
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) { Fail "adb screencap failed: $($stderrTask.Result)" }
}

function Assert-PngSignature([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { Fail "screenshot was not created" }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $expected = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
    if ($bytes.Length -lt 8) { Fail "screenshot is too small" }
    for ($i = 0; $i -lt 8; $i++) {
        if ($bytes[$i] -ne $expected[$i]) { Fail "screenshot is not a byte-safe PNG" }
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot
$ArtifactsFullPath = Join-Path $RepoRoot $ArtifactsDir
New-Item -ItemType Directory -Path $ArtifactsFullPath -Force | Out-Null

$DotnetVersion = (& dotnet --version | Out-String).Trim()
if ($DotnetVersion -ne '10.0.400') { Fail "expected .NET SDK 10.0.400, got $DotnetVersion" }

$AdbCommand = Get-Command adb -ErrorAction SilentlyContinue
if (-not $AdbCommand) { Fail "adb not found" }
$AdbExe = $AdbCommand.Source
$Serial = $null
foreach ($line in (& adb devices)) {
    if ($line.Trim() -match '^(emulator-\d+)\s+device$') {
        $candidate = $Matches[1]
        $boot = ((& adb -s $candidate shell getprop sys.boot_completed 2>$null) | Out-String).Trim()
        if ($boot -eq '1') { $Serial = $candidate; break }
    }
}
if (-not $Serial) { Fail "no booted emulator found" }
$AndroidApi = ((& adb -s $Serial shell getprop ro.build.version.sdk) | Out-String).Trim()
$AndroidRelease = ((& adb -s $Serial shell getprop ro.build.version.release) | Out-String).Trim()
$Abi = ((& adb -s $Serial shell getprop ro.product.cpu.abi) | Out-String).Trim()
if ($AndroidApi -ne '36') { Fail "expected API 36, got $AndroidApi" }
Write-Host "A15_EMULATOR_API36_PASS serial=$Serial android=$AndroidRelease abi=$Abi"

$AppProject = 'src/MobilDwg.App/MobilDwg.App.csproj'
& dotnet build $AppProject -f net10.0-android36.0 -c $Configuration -p:A15Validation=true -warnaserror --nologo | Tee-Object -FilePath (Join-Path $ArtifactsFullPath 'app-build.log')
Require-ExitCode "A15 validation MobilDwg.App build"
$BinDir = Join-Path $RepoRoot "src/MobilDwg.App/bin/$Configuration/net10.0-android36.0"
$Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*-Signed.apk' -File | Select-Object -First 1
if (-not $Apk) { $Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*.apk' -File | Select-Object -First 1 }
if (-not $Apk) { Fail "no A15 validation APK produced" }
$ApkHash = (Get-FileHash -LiteralPath $Apk.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
$EvidenceApk = Join-Path $ArtifactsFullPath 'MobilDwg.App-A15-Signed.apk'
Copy-Item -LiteralPath $Apk.FullName -Destination $EvidenceApk -Force
Write-Host "A15_REAL_APP_APK_PASS bytes=$($Apk.Length) sha256=$ApkHash"

$PackageName = 'com.smitelagwar.mobildwg'
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& adb -s $Serial uninstall $PackageName 2>$null | Out-Null
$ErrorActionPreference = $previousEap
& adb -s $Serial install -r $EvidenceApk | Out-Host
Require-ExitCode "A15 adb install"
$Resolved = @(& adb -s $Serial shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $PackageName)
$Launcher = $Resolved | Where-Object { $_ -match '/' } | Select-Object -Last 1
if (-not $Launcher) { Fail "launcher could not be resolved" }
$Launcher = $Launcher.Trim()
Write-Host "A15_REAL_APP_INSTALL_PASS package=$PackageName launcher=$Launcher"

& adb -s $Serial shell input keyevent KEYCODE_WAKEUP | Out-Null
& adb -s $Serial shell wm dismiss-keyguard | Out-Null
& adb -s $Serial shell am force-stop $PackageName | Out-Null
& adb -s $Serial logcat -c | Out-Null
& adb -s $Serial logcat -b crash -c | Out-Null
& adb -s $Serial logcat -b events -c | Out-Null
$LaunchOutput = ((& adb -s $Serial shell am start -W $Launcher) | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath 'launch.txt') -Value $LaunchOutput -Encoding utf8
if ($LaunchOutput -notmatch 'Status:\s+ok') { Fail "A15 app launch did not report Status: ok" }
$AppPid = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
if ($AppPid -notmatch '^\d+$') {
    Start-Sleep -Seconds 2
    $AppPid = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
}
if ($AppPid -notmatch '^\d+$') { Fail "A15 app PID not found" }
Write-Host "A15_REAL_APP_LAUNCH_PASS pid=$AppPid"

Write-Host "Waiting for A15 dimension/hatch validation to complete..."
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$markerFound = $false
while ($sw.Elapsed.TotalSeconds -lt 35) {
    $currentLog = (& adb -s $Serial logcat -d -t 300 | Out-String)
    if ($currentLog.IndexOf('A15_REAL_APP_UI_IMAGE_READY', [System.StringComparison]::Ordinal) -ge 0) {
        $markerFound = $true
        break
    }
    Start-Sleep -Seconds 2
}
if (-not $markerFound) {
    Write-Warning "A15_REAL_APP_UI_IMAGE_READY not found within 35 seconds, checking logcat..."
}
Start-Sleep -Milliseconds 500

$RawLogcat = (& adb -s $Serial logcat -d -t 2000 | Out-String)
$RedactedLogcat = $RawLogcat -replace '(?i)(token|secret|authorization|bearer)\s*[:=]\s*[^\s]+', '$1=[REDACTED]'
Set-Content -Path (Join-Path $ArtifactsFullPath 'logcat.txt') -Value $RedactedLogcat -Encoding utf8
foreach ($marker in @(
    'A15_ANDROID_ANONYMOUS_BLOCK_PASS',
    'A15_ANDROID_PROCEDURAL_DIMENSIONS_PASS',
    'A15_ANDROID_DEGENERATE_GUARDS_PASS',
    'A15_ANDROID_LEADER_PASS',
    'A15_ANDROID_HATCH_PROCESSING_PASS',
    'A15_SCENE_ENTITIES_COUNT=',
    'A15_HATCH_ISLAND_EVENODD_VERIFIED loops=',
    'A15_ANSI31_PATTERN_LINES_COUNT=',
    'A15_RENDER_PIXELS=',
    'A15_SNAPSHOT_HASH=',
    'A15_ANDROID_SKIA_RENDER_PASS bytes=',
    'ANDROID_STAGE15_DIMENSION_HATCH_PASS',
    'A15_REAL_APP_UI_IMAGE_READY sha256='
)) {
    if ($RawLogcat.IndexOf($marker, [StringComparison]::Ordinal) -lt 0) { Fail "required app marker missing: $marker" }
}
Write-Host "A15_REAL_APP_DIMENSION_HATCH_MARKERS_PASS"

$UiRemote = '/sdcard/a15-window.xml'
$UiLocal = Join-Path $ArtifactsFullPath 'window.xml'
& adb -s $Serial shell uiautomator dump $UiRemote | Out-Null
Require-ExitCode "A15 uiautomator dump"
& adb -s $Serial pull $UiRemote $UiLocal | Out-Null
Require-ExitCode "A15 uiautomator pull"
$UiXml = Get-Content -Raw $UiLocal
if ($UiXml.IndexOf('ANDROID_STAGE15_DIMENSION_HATCH_PASS', [StringComparison]::Ordinal) -lt 0) {
    Fail "A15 validation status is not visible in the real app UI hierarchy"
}
Write-Host "A15_REAL_APP_UI_STATUS_PASS"

$Screenshot = Join-Path $ArtifactsFullPath 'a15-real-app-dim-hatch.png'
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $Screenshot
Assert-PngSignature $Screenshot
$ScreenshotHash = (Get-FileHash -LiteralPath $Screenshot -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "A15_SCREENSHOT_PNG_PASS bytes=$((Get-Item $Screenshot).Length) sha256=$ScreenshotHash"

$RawCrash = (& adb -s $Serial logcat -b crash -d | Out-String)
$RawEvents = (& adb -s $Serial logcat -b events -d | Out-String)
$Meminfo = (& adb -s $Serial shell dumpsys meminfo $PackageName | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath 'crash-logcat.txt') -Value $RawCrash -Encoding utf8
Set-Content -Path (Join-Path $ArtifactsFullPath 'events-logcat.txt') -Value $RawEvents -Encoding utf8
Set-Content -Path (Join-Path $ArtifactsFullPath 'meminfo.txt') -Value $Meminfo -Encoding utf8
$PackagePattern = [regex]::Escape($PackageName)
$AppPidPattern = "(?<!\d)$AppPid(?!\d)"
$HasCrash = ($RawCrash -match '(?i)FATAL EXCEPTION|Fatal signal|Process .* has died') -and (($RawCrash -match $PackagePattern) -or ($RawCrash -match $AppPidPattern))
if ($HasCrash) { Fail "package/PID scoped crash evidence detected" }
$HasAnr = ($RawEvents -match '(?i)am_anr') -and (($RawEvents -match $PackagePattern) -or ($RawEvents -match $AppPidPattern))
if ($HasAnr) { Fail "post-launch ANR evidence detected" }
$AppPidStillAlive = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
if ($AppPidStillAlive -ne $AppPid) { Fail "A15 app process did not remain alive" }
Write-Host "A15_REAL_APP_STABILITY_PASS pid=$AppPid"

$HeadSha = (& git rev-parse HEAD | Out-String).Trim()
$Summary = @"
A15 DIMENSION LEADER HATCH ANDROID SUMMARY
Exact checked-out SHA: $HeadSha
.NET SDK: $DotnetVersion
Emulator: $Serial / Android $AndroidRelease / API $AndroidApi / $Abi
Package: $PackageName
PID: $AppPid
APK bytes: $($Apk.Length)
APK SHA-256: $ApkHash
Screenshot SHA-256: $ScreenshotHash
Anonymous dimension block (*D...) expansion: PASS
Procedural dimensions (Aligned, Rotated, Radial, Diametric): PASS
Degenerate dimension guards (identical defpoints, NaN): PASS
Leader & MultiLeader (tip arrowhead, dogleg, text): PASS
Hatch auto-closure within 1mm: PASS
Hatch broken boundary diagnostic (> 1mm): PASS
EvenOdd nested island handling: PASS
ANSI31 clipped pattern line generation: PASS
Real Skia CAD render of dimensions & hatches to PNG: PASS
Real app UI status: PASS
Crash/ANR/liveness: PASS
Physical Android: DEFERRED_RELEASE_DEVICE_GATE
Claim limit: A15_DIMENSION_HATCH_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
Result: ANDROID_STAGE15_DIMENSION_HATCH_PASS
"@

Set-Content -Path (Join-Path $ArtifactsFullPath 'summary.txt') -Value $Summary -Encoding utf8
Write-Host "ANDROID_STAGE15_DIMENSION_HATCH_PASS"
Write-Host "CLAIM_LIMIT=A15_DIMENSION_HATCH_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY"
exit 0
