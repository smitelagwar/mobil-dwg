#Requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

<#
.SYNOPSIS
    AŞAMA 25 — Android Beta ve Blocker Düzeltmeleri kabul testi gate scripti.
#>

function Fail([string]$Message) {
    Write-Host "A25_FAIL: $Message" -ForegroundColor Red
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
    if (-not (Test-Path -LiteralPath $Path)) { Fail "screenshot was not created: $Path" }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $expected = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
    if ($bytes.Length -lt 8) { Fail "screenshot is too small" }
    for ($i = 0; $i -lt 8; $i++) {
        if ($bytes[$i] -ne $expected[$i]) { Fail "screenshot is not a byte-safe PNG" }
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
$AppProject  = Join-Path $RepoRoot "src\MobilDwg.App\MobilDwg.App.csproj"
$TestProject = Join-Path $RepoRoot "tests\MobilDwg.Rendering.Tests\MobilDwg.Rendering.Tests.csproj"
$ArtifactsFullPath = Join-Path $RepoRoot "artifacts\a25-android-beta-blocker"
$Package     = "com.smitelagwar.mobildwg"
$LogcatLog   = Join-Path $ArtifactsFullPath "logcat_a25.txt"

New-Item -ItemType Directory -Force -Path $ArtifactsFullPath | Out-Null

$AdbCommand = Get-Command adb -ErrorAction SilentlyContinue
if ($AdbCommand) {
    $AdbExe = $AdbCommand.Source
} else {
    $AdbExe = Join-Path $env:LOCALAPPDATA "Android\Sdk\platform-tools\adb.exe"
}
if (-not (Test-Path $AdbExe)) { Fail "adb not found at $AdbExe" }

$Devices = @(& $AdbExe devices | Where-Object { $_ -match '\tdevice$' })
if ($Devices.Count -eq 0) { Fail "no authorized Android device or emulator available" }
$Serial = ($Devices[0] -split '\s+')[0]

$AndroidApi = [int]((& $AdbExe -s $Serial shell getprop ro.build.version.sdk | Out-String).Trim())
Write-Host "[A25-GATE] Device connected: $Serial (API $AndroidApi)"

# ── STEP 1: Host Tests ────────────────────────────────────────────────────────
Write-Host "[A25-GATE] STEP 1: Running host tests (net10.0)..."
$hostOut = & dotnet run --project $TestProject -c Release 2>&1 | Out-String
Write-Host $hostOut
if ($hostOut -notmatch "STAGE25_BETA_BLOCKER_TESTS_PASS") { Fail "Host tests failed" }
Set-Content -Path (Join-Path $ArtifactsFullPath "host_test_output.txt") -Value $hostOut -Encoding UTF8

# ── STEP 2: Build APK with A25Validation ─────────────────────────────────────
Write-Host "[A25-GATE] STEP 2: Building APK with A25Validation=true..."
$apkBuildArgs = @(
    "build", $AppProject,
    "-c", "Release",
    "-f", "net10.0-android36.0",
    "-p:A25Validation=true",
    "-p:AndroidKeyStore=false",
    "--nologo"
)
$buildOut = & dotnet @apkBuildArgs 2>&1 | Out-String
Write-Host $buildOut
Require-ExitCode "APK Build"

$apkPattern = Join-Path $RepoRoot "src\MobilDwg.App\bin\Release\net10.0-android36.0\*.apk"
$apkPath = Get-ChildItem -Path $apkPattern -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "*-Signed*" } |
    Select-Object -First 1
if (-not $apkPath) {
    $apkPath = Get-ChildItem -Path $apkPattern -ErrorAction SilentlyContinue | Select-Object -First 1
}
if (-not $apkPath) { Fail "APK not found after build" }
Write-Host "[A25-GATE] APK: $($apkPath.FullName) ($([math]::Round($apkPath.Length/1MB,1)) MB)"

# ── STEP 3: Install APK ───────────────────────────────────────────────────────
Write-Host "[A25-GATE] STEP 3: Installing APK on $Serial..."
$installOut = & $AdbExe -s $Serial install -r $apkPath.FullName 2>&1 | Out-String
Write-Host $installOut
if ($installOut -notmatch "Success") { Fail "APK install failed: $installOut" }

# ── STEP 4: Clear logcat + launch ────────────────────────────────────────────
Write-Host "[A25-GATE] STEP 4: Resolving launcher, clearing logcat and launching app..."
$resolveOut = (& $AdbExe -s $Serial shell cmd package resolve-activity --brief $Package 2>&1 | Out-String)
$launcherLine = ($resolveOut -split "`r?`n" | Where-Object { $_ -match "^$Package/" } | Select-Object -First 1)
if (-not $launcherLine) {
    $launcherLine = "$Package/crc64d52a5cdc4f267319.MainActivity"
}
Write-Host "[A25-GATE] Launcher activity: $launcherLine"

& $AdbExe -s $Serial shell input keyevent KEYCODE_WAKEUP | Out-Null
& $AdbExe -s $Serial shell wm dismiss-keyguard | Out-Null
& $AdbExe -s $Serial logcat -c 2>&1 | Out-Null
& $AdbExe -s $Serial shell am force-stop $Package 2>&1 | Out-Null
& $AdbExe -s $Serial shell am start -n $launcherLine 2>&1 | Out-Null
Require-ExitCode "App Launch"

# ── STEP 5: Poll logcat for completion ───────────────────────────────────────
Write-Host "[A25-GATE] STEP 5: Waiting for A25 validation completion in logcat..."
$Done = $false
for ($i = 0; $i -lt 45; $i++) {
    Start-Sleep -Seconds 1
    $Logs = (& $AdbExe -s $Serial logcat -d -s MobilDwgA25:V | Out-String)
    if ($Logs -match "A25_REAL_APP_UI_IMAGE_READY") {
        $Done = $true
        break
    }
    if ($Logs -match "ANDROID_STAGE25_BETA_BLOCKER_FAIL") {
        Fail "validation runner reported failure: $Logs"
    }
}
if (-not $Done) { Fail "timed out waiting for A25_REAL_APP_UI_IMAGE_READY" }

Start-Sleep -Milliseconds 1500
$LogcatDump = (& $AdbExe -s $Serial logcat -d -s MobilDwgA25:V | Out-String)
Set-Content -Path $LogcatLog -Value $LogcatDump -Encoding UTF8
Write-Host $LogcatDump

# ── STEP 6: Verify all markers ────────────────────────────────────────────────
Write-Host "[A25-GATE] STEP 6: Verifying logcat markers..."
$ReqMarkers = @(
    "A25_DISPOSE_CHAIN_PASS",
    "A25_CACHE_PURGE_PASS",
    "A25_RENDER_ERROR_SURFACE_PASS",
    "A25_COORDINATOR_RESET_PASS",
    "A25_PROOF_PNG_READY",
    "ANDROID_STAGE25_BETA_BLOCKER_PASS",
    "A25_REAL_APP_UI_IMAGE_READY"
)
foreach ($m in $ReqMarkers) {
    if ($LogcatDump -notmatch $m) {
        Fail "missing expected marker in logcat: $m"
    }
    $line = ($LogcatDump -split "`r?`n" | Where-Object { $_ -match $m } | Select-Object -First 1)
    Write-Host "  [OK] $line"
}

# ── STEP 7: UI hierarchy dump ────────────────────────────────────────────────
Write-Host "[A25-GATE] STEP 7: UI hierarchy dump..."
$RemoteDump = "/sdcard/a25_window_dump.xml"
& $AdbExe -s $Serial shell uiautomator dump $RemoteDump | Out-Null
$LocalDump = Join-Path $ArtifactsFullPath "a25_window.xml"
& $AdbExe -s $Serial pull $RemoteDump $LocalDump
& $AdbExe -s $Serial shell rm -f $RemoteDump | Out-Null

$UiContent = [System.IO.File]::ReadAllText($LocalDump)
if ($UiContent -notmatch "ANDROID_STAGE25_BETA_BLOCKER_PASS") {
    Fail "UI hierarchy does not contain ANDROID_STAGE25_BETA_BLOCKER_PASS text"
}
Write-Host "[A25-GATE] UI hierarchy verified!"

# ── STEP 8: Byte-safe screenshot ─────────────────────────────────────────────
Write-Host "[A25-GATE] STEP 8: Capturing byte-safe screenshot..."
$ScreenshotPath = Join-Path $ArtifactsFullPath "a25-real-app-beta-blocker.png"
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $ScreenshotPath
Assert-PngSignature -Path $ScreenshotPath
Write-Host "[A25-GATE] Screenshot saved: $ScreenshotPath ($([math]::Round((Get-Item $ScreenshotPath).Length/1KB,1)) KB)"

# ── STEP 9: Uninstall app ────────────────────────────────────────────────────
Write-Host "[A25-GATE] STEP 9: Cleaning up app..."
& $AdbExe -s $Serial uninstall $Package | Out-Null

Write-Host "═══════════════════════════════════════════════════════════════════"
Write-Host "ANDROID_STAGE25_BETA_BLOCKER_PASS" -ForegroundColor Green
