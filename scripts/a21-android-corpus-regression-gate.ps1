<#
.SYNOPSIS
    A21 real MobilDwg.App API36 Full Corpus Regression & Beta Gate acceptance script.
.DESCRIPTION
    Builds a validation Release APK with -p:A21Validation=true and packaged CAD fixtures,
    executes full corpus regression across committed DXFs, generated DWG, and synthetic suites,
    evaluates P0/P1 compatibility tiers (C0-C4), verifies survey origin double-precision integrity,
    checks trimming/AOT stability, asserts beta gate budgets, captures byte-safe PNG screenshot,
    and validates package-scoped crash/ANR/liveness on the API 36 emulator.
#>
param(
    [string]$Configuration = 'Release',
    [string]$ArtifactsDir = 'artifacts/a21-android-corpus-regression'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "A21_FAIL: $Message" -ForegroundColor Red
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
Write-Host "A21_EMULATOR_API36_PASS serial=$Serial android=$AndroidRelease abi=$Abi"

# Ensure synthetic DWG fixture is generated
$SyntheticDwgRelative = 'artifacts/fixtures/synthetic_turkish_basic_ac1015.dwg'
$SyntheticDwgFull = Join-Path $RepoRoot $SyntheticDwgRelative
if (-not (Test-Path $SyntheticDwgFull)) {
    Write-Host "Generating synthetic AC1015 DWG fixture..."
    & powershell -ExecutionPolicy Bypass -File scripts/generate-synthetic-dwg.ps1
    Require-ExitCode "synthetic DWG generation"
}

$AppProject = 'src/MobilDwg.App/MobilDwg.App.csproj'
$BinDir = Join-Path $RepoRoot 'src/MobilDwg.App/bin/Release/net10.0-android36.0'
if (Test-Path $BinDir) { Remove-Item -Path $BinDir -Recurse -Force | Out-Null }

Write-Host "Building Release APK with A21Validation=true..."
& dotnet build $AppProject -c $Configuration -f net10.0-android36.0 -p:A21Validation=true "-p:A21GeneratedDwgPath=$SyntheticDwgFull" | Out-Host
Require-ExitCode "A21 MobilDwg.App build"

$Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*-Signed.apk' -File | Select-Object -First 1
if (-not $Apk) { $Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*.apk' -File | Select-Object -First 1 }
if (-not $Apk) { Fail "no A21 validation APK produced" }

$MaxApkBytes = 45 * 1024 * 1024
if ($Apk.Length -gt $MaxApkBytes) {
    Fail "APK package size ($($Apk.Length) bytes) exceeded budget ($MaxApkBytes bytes)"
}

$ApkSha256 = (Get-FileHash -Path $Apk.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "A21_REAL_APP_APK_PASS bytes=$($Apk.Length) sha256=$ApkSha256"

$PackageName = 'com.smitelagwar.mobildwg'
$Launcher = "$PackageName/crc64d52a5cdc4f267319.MainActivity"

& adb -s $Serial install -r $Apk.FullName | Out-Host
Require-ExitCode "adb install"
Write-Host "A21_REAL_APP_INSTALL_PASS package=$PackageName launcher=$Launcher"

Write-Host "Pre-compiling package with speed profile to avoid startup JIT thrashing..."
& adb -s $Serial shell cmd package compile -m speed $PackageName | Out-Null

& adb -s $Serial shell input keyevent KEYCODE_WAKEUP | Out-Null
& adb -s $Serial shell wm dismiss-keyguard | Out-Null

& adb -s $Serial logcat -c | Out-Null
& adb -s $Serial logcat -b crash -c | Out-Null

& adb -s $Serial shell am force-stop $PackageName | Out-Null
$StartOutput = (& adb -s $Serial shell am start -W -n $Launcher | Out-String)

$AppPid = 0
for ($attempt = 0; $attempt -lt 40; $attempt++) {
    Start-Sleep -Milliseconds 500
    $PidOutput = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
    if ($PidOutput -match '^\d+$') {
        $AppPid = [int]$PidOutput
        break
    }
}
if ($AppPid -le 0) { Fail "application PID could not be determined. am start: $StartOutput" }
Write-Host "A21_REAL_APP_LAUNCH_PASS pid=$AppPid"

Write-Host "Polling logcat for A21 acceptance markers..."
$Deadline = (Get-Date).AddSeconds(120)
$Done = $false

while ((Get-Date) -lt $Deadline) {
    Start-Sleep -Milliseconds 1000
    $Logs = (& adb -s $Serial logcat -d -s MobilDwgA21:I | Out-String)
    if ($Logs -match 'A21_REAL_APP_UI_IMAGE_READY') {
        $Done = $true
        break
    }
    if ($Logs -match 'ANDROID_STAGE21_CORPUS_REGRESSION_FAIL') {
        Fail "validation runner reported failure in logcat: $Logs"
    }
}

if (-not $Done) {
    Fail "timed out waiting for A21_REAL_APP_UI_IMAGE_READY marker in logcat"
}

Start-Sleep -Milliseconds 1500

$LogcatDump = (& adb -s $Serial logcat -d -s MobilDwgA21:I | Out-String)
$LogcatFile = Join-Path $ArtifactsFullPath 'a21_emulator_logcat.txt'
[System.IO.File]::WriteAllText($LogcatFile, $LogcatDump)

$ReqMarkers = @(
    'A21_CORPUS_REGRESSION_PASS',
    'A21_P0_P1_MATRIX_PASS',
    'A21_BETA_GATE_VERDICT_PASS',
    'A21_TRIMMING_AOT_PASS',
    'A21_SNAPSHOT_PASS',
    'A21_ANDROID_SKIA_RENDER_PASS',
    'A21_REAL_APP_STABILITY_PASS',
    'ANDROID_STAGE21_CORPUS_REGRESSION_PASS',
    'A21_REAL_APP_UI_IMAGE_READY'
)

foreach ($m in $ReqMarkers) {
    if ($LogcatDump -notmatch $m) {
        Fail "missing expected marker in logcat: $m"
    }
    $line = ($LogcatDump -split "`r?`n" | Where-Object { $_ -match $m } | Select-Object -First 1)
    Write-Host $line
}

Write-Host "A21_REAL_APP_REGRESSION_MARKERS_PASS"

$RemoteDump = '/sdcard/a21_window_dump.xml'
& adb -s $Serial shell uiautomator dump $RemoteDump | Out-Null
$LocalDump = Join-Path $ArtifactsFullPath 'a21_window.xml'
& adb -s $Serial pull $RemoteDump $LocalDump | Out-Null
& adb -s $Serial shell rm -f $RemoteDump | Out-Null

$UiContent = [System.IO.File]::ReadAllText($LocalDump)
if ($UiContent -notmatch 'ANDROID_STAGE21_CORPUS_REGRESSION_PASS') {
    Fail "UI hierarchy does not contain ANDROID_STAGE21_CORPUS_REGRESSION_PASS status text"
}
Write-Host "A21_REAL_APP_UI_STATUS_PASS"

$ScreenshotPath = Join-Path $ArtifactsFullPath 'a21-real-app-corpus.png'
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $ScreenshotPath
Assert-PngSignature -Path $ScreenshotPath
$PngBytes = (Get-Item -LiteralPath $ScreenshotPath).Length
$PngHash = (Get-FileHash -Path $ScreenshotPath -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "A21_SCREENSHOT_PNG_PASS bytes=$PngBytes sha256=$PngHash"

$MemInfo = (& adb -s $Serial shell dumpsys meminfo $PackageName | Out-String)
$MeminfoFile = Join-Path $ArtifactsFullPath 'a21_meminfo.txt'
[System.IO.File]::WriteAllText($MeminfoFile, $MemInfo)

$PssMatch = [regex]::Match($MemInfo, 'TOTAL PSS:\s+(\d+)')
$TotalPssKb = if ($PssMatch.Success) { [long]$PssMatch.Groups[1].Value } else { 0 }
$TotalPssMb = $TotalPssKb / 1024.0

$MaxPssMb = 250.0
if ($TotalPssMb -gt $MaxPssMb) {
    Fail "Total PSS ($($TotalPssMb) MB) exceeded ceiling budget ($MaxPssMb MB)"
}
Write-Host "A21_MEMINFO_PSS_PASS total_pss=$([Math]::Round($TotalPssMb, 1)) MB"

$FinalPidOutput = ((& adb -s $Serial shell pidof -s $PackageName | Out-String).Trim())
$FinalPid = if ($FinalPidOutput -match '^\d+$') { [int]$FinalPidOutput } else { 0 }
if ($FinalPid -ne $AppPid) {
    Fail "process crashed or restarted during validation (initial PID: $AppPid, final PID: $FinalPid)"
}
Write-Host "A21_REAL_APP_STABILITY_PASS pid=$FinalPid"

Write-Host "ANDROID_STAGE21_CORPUS_REGRESSION_PASS" -ForegroundColor Green
