<#
.SYNOPSIS
    AŞAMA 20 real MobilDwg.App API36 Measured Performance & Memory acceptance gate.
.DESCRIPTION
    Builds a validation-only Release APK with -p:A20Validation=true, executes
    TTFUP (Time To First Usable Paint) across small/medium/large corpus scenes,
    frame timing benchmarks (p50/p95 percentiles across multi-frame pan/zoom),
    Android process memory measurements (managed GC heap, native heap, dumpsys PSS),
    A-B line/culling optimization evidence, captures PNG screenshot evidence,
    and validates package-scoped crash/ANR/liveness.
#>
param(
    [string]$Configuration = 'Release',
    [string]$ArtifactsDir = 'artifacts/a20-android-perf-memory'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "A20_FAIL: $Message" -ForegroundColor Red
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
if (-not $AdbCommand) { Fail "adb not found in PATH" }
$AdbExe = $AdbCommand.Source

$Devices = @(& adb devices | Where-Object { $_ -match '\tdevice$' })
if ($Devices.Count -eq 0) { Fail "no authorized Android device or emulator available" }
$Serial = ($Devices[0] -split '\s+')[0]

$AndroidRelease = ((& adb -s $Serial shell getprop ro.build.version.release | Out-String).Trim())
$AndroidApi = [int]((& adb -s $Serial shell getprop ro.build.version.sdk | Out-String).Trim())
$Abi = ((& adb -s $Serial shell getprop ro.product.cpu.abi | Out-String).Trim())
if ($AndroidApi -ne 36) { Fail "expected API 36 emulator, found API $AndroidApi on $Serial" }
Write-Host "A20_EMULATOR_API36_PASS serial=$Serial android=$AndroidRelease abi=$Abi"

$AppProject = 'src/MobilDwg.App/MobilDwg.App.csproj'
$BinDir = Join-Path $RepoRoot 'src/MobilDwg.App/bin/Release/net10.0-android36.0'
if (Test-Path $BinDir) { Remove-Item -Path $BinDir -Recurse -Force | Out-Null }

& dotnet build $AppProject -c $Configuration -f net10.0-android36.0 -p:A20Validation=true | Out-Host
Require-ExitCode "A20 MobilDwg.App build"

$Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*-Signed.apk' -File | Select-Object -First 1
if (-not $Apk) { $Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*.apk' -File | Select-Object -First 1 }
if (-not $Apk) { Fail "no A20 validation APK produced" }

$MaxApkBytes = 45 * 1024 * 1024
if ($Apk.Length -gt $MaxApkBytes) {
    Fail "APK package size ($($Apk.Length) bytes) exceeded budget ($MaxApkBytes bytes)"
}

$ApkHash = (Get-FileHash -LiteralPath $Apk.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
$EvidenceApk = Join-Path $ArtifactsFullPath 'MobilDwg.App-A20-Signed.apk'
Copy-Item -LiteralPath $Apk.FullName -Destination $EvidenceApk -Force
Write-Host "A20_REAL_APP_APK_PASS bytes=$($Apk.Length) sha256=$ApkHash"

$PackageName = 'com.smitelagwar.mobildwg'
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& adb -s $Serial uninstall $PackageName 2>$null | Out-Null
$ErrorActionPreference = $previousEap
& adb -s $Serial install -r $EvidenceApk | Out-Host
Require-ExitCode "A20 adb install"
$Resolved = @(& adb -s $Serial shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $PackageName)
$Launcher = $Resolved | Where-Object { $_ -match '/' } | Select-Object -Last 1
if (-not $Launcher) { Fail "launcher could not be resolved" }
$Launcher = $Launcher.Trim()
Write-Host "A20_REAL_APP_INSTALL_PASS package=$PackageName launcher=$Launcher"

& adb -s $Serial shell input keyevent KEYCODE_WAKEUP | Out-Null
& adb -s $Serial shell wm dismiss-keyguard | Out-Null
& adb -s $Serial shell am force-stop $PackageName | Out-Null
& adb -s $Serial logcat -c | Out-Null
& adb -s $Serial logcat -b crash -c | Out-Null
& adb -s $Serial logcat -b events -c | Out-Null

$StartOutput = (& adb -s $Serial shell am start -W -n $Launcher | Out-String)
$AppPid = ''
for ($attempt = 0; $attempt -lt 40; $attempt++) {
    Start-Sleep -Milliseconds 500
    $AppPid = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
    if ($AppPid) { break }
}
if (-not $AppPid) { Fail "could not resolve PID for $PackageName after launch. am start: $StartOutput" }
Write-Host "A20_REAL_APP_LAUNCH_PASS pid=$AppPid"

Write-Host "Waiting for A20 Performance & Memory validation to complete..."
$Completed = $false
for ($i = 0; $i -lt 90; $i++) {
    Start-Sleep -Seconds 1
    $Raw = (& adb -s $Serial logcat -d | Out-String)
    if ($Raw.IndexOf('ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS', [StringComparison]::Ordinal) -ge 0) {
        $Completed = $true
        break
    }
    if ($Raw.IndexOf('ANDROID_STAGE20_PERFORMANCE_MEMORY_FAIL', [StringComparison]::Ordinal) -ge 0) {
        Fail "A20 validation failed in app logcat: $Raw"
    }
}
if (-not $Completed) { Fail "timed out waiting for A20 validation markers" }

$RawLogcat = (& adb -s $Serial logcat -d | Out-String)
$RedactedLogcat = $RawLogcat -replace '(?i)\b(bearer|token|password|auth)=[^&\s]+', '$1=REDACTED'
Set-Content -Path (Join-Path $ArtifactsFullPath 'logcat.txt') -Value $RedactedLogcat -Encoding utf8
foreach ($marker in @(
    'A20_ANDROID_TTFUP_PASS',
    'A20_ANDROID_FRAME_TIMING_PASS',
    'A20_ANDROID_MEMORY_PASS',
    'A20_ANDROID_AB_OPTIMIZATION_PASS',
    'A20_ANDROID_SNAPSHOT_PASS',
    'A20_ANDROID_SKIA_RENDER_PASS',
    'A20_REAL_APP_PERF_MARKERS_PASS',
    'ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS',
    'A20_REAL_APP_UI_IMAGE_READY sha256='
)) {
    if ($RawLogcat.IndexOf($marker, [StringComparison]::Ordinal) -lt 0) { Fail "required app marker missing: $marker" }
}
Write-Host "A20_REAL_APP_PERF_MARKERS_PASS"

$UiRemote = '/sdcard/a20-window.xml'
$UiLocal = Join-Path $ArtifactsFullPath 'window.xml'
& adb -s $Serial shell uiautomator dump $UiRemote | Out-Null
Require-ExitCode "A20 uiautomator dump"
& adb -s $Serial pull $UiRemote $UiLocal | Out-Null
Require-ExitCode "A20 uiautomator pull"
$UiXml = Get-Content -Raw $UiLocal
if ($UiXml.IndexOf('ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS', [StringComparison]::Ordinal) -lt 0) {
    Fail "A20 validation status is not visible in the real app UI hierarchy"
}
Write-Host "A20_REAL_APP_UI_STATUS_PASS"

$Screenshot = Join-Path $ArtifactsFullPath 'a20-real-app-perf.png'
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $Screenshot
Assert-PngSignature $Screenshot
$ScreenshotHash = (Get-FileHash -LiteralPath $Screenshot -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "A20_SCREENSHOT_PNG_PASS bytes=$((Get-Item $Screenshot).Length) sha256=$ScreenshotHash"

$RawCrash = (& adb -s $Serial logcat -b crash -d | Out-String)
$RawEvents = (& adb -s $Serial logcat -b events -d | Out-String)
$Meminfo = (& adb -s $Serial shell dumpsys meminfo $PackageName | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath 'crash-logcat.txt') -Value $RawCrash -Encoding utf8
Set-Content -Path (Join-Path $ArtifactsFullPath 'events-logcat.txt') -Value $RawEvents -Encoding utf8
Set-Content -Path (Join-Path $ArtifactsFullPath 'meminfo.txt') -Value $Meminfo -Encoding utf8

# Parse TOTAL PSS from dumpsys meminfo
$TotalPssKb = 0
if ($Meminfo -match 'TOTAL\s+PSS:\s+(\d+)') {
    $TotalPssKb = [int]$matches[1]
} elseif ($Meminfo -match 'TOTAL\s+(\d+)') {
    $TotalPssKb = [int]$matches[1]
}
$TotalPssMb = [math]::Round($TotalPssKb / 1024, 1)
Write-Host "A20_MEMINFO_PSS_PASS total_pss=$TotalPssMb MB"

# Verify PSS stays within 250 MB budget
if ($TotalPssMb -gt 250) {
    Fail "Total PSS memory ($TotalPssMb MB) exceeded 250MB budget"
}

$PackagePattern = [regex]::Escape($PackageName)
$AppPidPattern = "(?<!\d)$AppPid(?!\d)"
$HasCrash = ($RawCrash -match '(?i)FATAL EXCEPTION|Fatal signal|Process .* has died') -and (($RawCrash -match $PackagePattern) -or ($RawCrash -match $AppPidPattern))
if ($HasCrash) { Fail "package/PID scoped crash evidence detected" }
$HasAnr = ($RawEvents -match '(?i)am_anr') -and (($RawEvents -match $PackagePattern) -or ($RawEvents -match $AppPidPattern))
if ($HasAnr) { Fail "post-launch ANR evidence detected" }
$AppPidStillAlive = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
if ($AppPidStillAlive -ne $AppPid) { Fail "A20 app process did not remain alive" }
Write-Host "A20_REAL_APP_STABILITY_PASS pid=$AppPid"

$HeadSha = (& git rev-parse HEAD | Out-String).Trim()
$Summary = @"
A20 PERFORMANCE AND MEMORY ANDROID SUMMARY
Exact checked-out SHA: $HeadSha
.NET SDK: $DotnetVersion
Emulator: $Serial / Android $AndroidRelease / API $AndroidApi / $Abi
Package: $PackageName
PID: $AppPid
APK bytes: $($Apk.Length)
APK SHA-256: $ApkHash
Screenshot bytes: $((Get-Item $Screenshot).Length)
Screenshot SHA-256: $ScreenshotHash
Total PSS: $TotalPssMb MB
"@
Set-Content -Path (Join-Path $ArtifactsFullPath 'summary.txt') -Value $Summary -Encoding utf8

Write-Host "ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS"
Write-Host "CLAIM_LIMIT=A20_PERFORMANCE_MEMORY_API36_ONLY_NOT_PHYSICAL_DEVICE_FIDELITY"
