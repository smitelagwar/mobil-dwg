#Requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

<#
.SYNOPSIS
    AŞAMA 27 — Android v1 artifact / yayın / handoff kabul testi gate scripti.
#>

function Fail([string]$Message) {
    Write-Host "A27_FAIL: $Message" -ForegroundColor Red
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
$ProbeProject = Join-Path $RepoRoot "compliance\Stage02.DependencyProbe\Stage02.DependencyProbe.csproj"
$ReleaseDir  = Join-Path $RepoRoot "release"
$ArtifactsFullPath = Join-Path $RepoRoot "artifacts\a27-android-release-handoff"
$Package     = "com.smitelagwar.mobildwg"

New-Item -ItemType Directory -Force -Path $ReleaseDir | Out-Null
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
Write-Host "[A27-GATE] Connected device: $Serial (API $AndroidApi)"

# ── STEP 1: Verify Release Documentation ─────────────────────────────────────
Write-Host "[A27-GATE] STEP 1: Verifying release documentation..."
$RequiredDocs = @(
    "docs\release\BUILD_INSTRUCTIONS.md",
    "docs\release\PLAY_STORE_SUBMISSION_GUIDE.md",
    "docs\release\PRIVACY_POLICY.md",
    "docs\release\COMPATIBILITY_AND_LIMITATIONS.md",
    "docs\release\THIRD_PARTY_NOTICES.md"
)
foreach ($doc in $RequiredDocs) {
    $p = Join-Path $RepoRoot $doc
    if (-not (Test-Path $p)) { Fail "Missing release documentation file: $doc" }
    Write-Host "  [OK] $doc"
}
Write-Host "A27_DOCUMENTATION_PASS"

# ── STEP 2: Host Contract Tests ───────────────────────────────────────────────
Write-Host "[A27-GATE] STEP 2: Running host tests..."
$hostOut = & dotnet run --project $TestProject -c Release 2>&1 | Out-String
if ($hostOut -notmatch "STAGE26_FINAL_AUDIT_TESTS_PASS") { Fail "Host tests failed" }
Write-Host "A27_HOST_TESTS_PASS"

# ── STEP 3: Build Clean Production Release AAB ───────────────────────────────
Write-Host "[A27-GATE] STEP 3: Building clean production Release AAB..."
$aabBuildArgs = @(
    "build", $AppProject,
    "-c", "Release",
    "-f", "net10.0-android36.0",
    "-p:AndroidPackageFormat=aab",
    "-p:AndroidKeyStore=false",
    "--nologo"
)
$aabOut = & dotnet @aabBuildArgs 2>&1 | Out-String
Require-ExitCode "Production AAB Build"

$aabPattern = Join-Path $RepoRoot "src\MobilDwg.App\bin\Release\net10.0-android36.0\*.aab"
$aabSrc = Get-ChildItem -Path $aabPattern -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*-Signed*" } |
    Select-Object -First 1
if (-not $aabSrc) {
    $aabSrc = Get-ChildItem -Path $aabPattern -ErrorAction SilentlyContinue | Select-Object -First 1
}
if (-not $aabSrc) { Fail "No production AAB generated" }

$FinalAab = Join-Path $ReleaseDir "MobilDwg-v1.0.0.aab"
Copy-Item $aabSrc.FullName $FinalAab -Force
$aabSizeMb = [math]::Round((Get-Item $FinalAab).Length / 1MB, 2)
Write-Host "[A27-GATE] Release AAB: $FinalAab ($aabSizeMb MB)"
if ((Get-Item $FinalAab).Length -gt 45L * 1024 * 1024) { Fail "AAB size exceeds 45 MB budget" }

# ── STEP 4: Build Clean Production Release APK ───────────────────────────────
Write-Host "[A27-GATE] STEP 4: Building clean production Release APK..."
$apkBuildArgs = @(
    "build", $AppProject,
    "-c", "Release",
    "-f", "net10.0-android36.0",
    "-p:AndroidKeyStore=false",
    "--nologo"
)
$apkOut = & dotnet @apkBuildArgs 2>&1 | Out-String
Require-ExitCode "Production APK Build"

$apkPattern = Join-Path $RepoRoot "src\MobilDwg.App\bin\Release\net10.0-android36.0\*.apk"
$apkSrc = Get-ChildItem -Path $apkPattern -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*-Signed*" } |
    Select-Object -First 1
if (-not $apkSrc) {
    $apkSrc = Get-ChildItem -Path $apkPattern -ErrorAction SilentlyContinue | Select-Object -First 1
}
if (-not $apkSrc) { Fail "No production APK generated" }

$FinalApk = Join-Path $ReleaseDir "MobilDwg-v1.0.0.apk"
Copy-Item $apkSrc.FullName $FinalApk -Force
$apkSizeMb = [math]::Round((Get-Item $FinalApk).Length / 1MB, 2)
Write-Host "[A27-GATE] Release APK: $FinalApk ($apkSizeMb MB)"
if ((Get-Item $FinalApk).Length -gt 45L * 1024 * 1024) { Fail "APK size exceeds 45 MB budget" }

# ── STEP 5: Generate Checksums ───────────────────────────────────────────────
Write-Host "[A27-GATE] STEP 5: Generating SHA256SUMS.txt..."
$apkSha = (Get-FileHash -Path $FinalApk -Algorithm SHA256).Hash.ToLowerInvariant()
$aabSha = (Get-FileHash -Path $FinalAab -Algorithm SHA256).Hash.ToLowerInvariant()
$Checksums = @(
    "$apkSha  MobilDwg-v1.0.0.apk",
    "$aabSha  MobilDwg-v1.0.0.aab"
)
$ChecksumsFile = Join-Path $ReleaseDir "SHA256SUMS.txt"
Set-Content -Path $ChecksumsFile -Value $Checksums -Encoding ascii
foreach ($line in $Checksums) { Write-Host "  $line" }
Write-Host "A27_CHECKSUMS_PASS"

# ── STEP 6: Install Clean Production APK on Emulator ─────────────────────────
Write-Host "[A27-GATE] STEP 6: Installing production APK on $Serial..."
$installOut = & $AdbExe -s $Serial install -r $FinalApk 2>&1 | Out-String
Write-Host $installOut
if ($installOut -notmatch "Success") { Fail "Production APK install failed" }

# ── STEP 7: Launch Pure Production App ───────────────────────────────────────
Write-Host "[A27-GATE] STEP 7: Launching pure production app..."
$resolveOut = (& $AdbExe -s $Serial shell cmd package resolve-activity --brief $Package 2>&1 | Out-String)
$launcherLine = ($resolveOut -split "`r?`n" | Where-Object { $_ -match "^$Package/" } | Select-Object -First 1)
if (-not $launcherLine) {
    $launcherLine = "$Package/crc64d52a5cdc4f267319.MainActivity"
}
Write-Host "[A27-GATE] Launcher: $launcherLine"

& $AdbExe -s $Serial shell input keyevent KEYCODE_WAKEUP | Out-Null
& $AdbExe -s $Serial shell wm dismiss-keyguard | Out-Null
& $AdbExe -s $Serial logcat -c 2>&1 | Out-Null
& $AdbExe -s $Serial shell am force-stop $Package 2>&1 | Out-Null
& $AdbExe -s $Serial shell am start -n $launcherLine 2>&1 | Out-Null
Require-ExitCode "Production App Launch"

# Wait 6 seconds for smooth startup and UI stabilization
Start-Sleep -Seconds 6

$pidOutput = ((& $AdbExe -s $Serial shell pidof -s $Package 2>$null) | Out-String).Trim()
if ($pidOutput -notmatch '^\d+$') { Fail "Production app crashed or did not stay running!" }
$appPid = [int]$pidOutput
Write-Host "[A27-GATE] Production App Running PID: $appPid"
Write-Host "A27_REAL_APP_PRODUCTION_LAUNCH_PASS pid=$appPid"

# ── STEP 8: UI Hierarchy Dump ────────────────────────────────────────────────
Write-Host "[A27-GATE] STEP 8: Inspecting production UI hierarchy..."
$RemoteDump = "/sdcard/a27_window_dump.xml"
& $AdbExe -s $Serial shell uiautomator dump $RemoteDump | Out-Null
$LocalDump = Join-Path $ArtifactsFullPath "a27_window.xml"
& $AdbExe -s $Serial pull $RemoteDump $LocalDump
& $AdbExe -s $Serial shell rm -f $RemoteDump | Out-Null

$UiContent = [System.IO.File]::ReadAllText($LocalDump)
if ($UiContent -notmatch "DWG/DXF" -and $UiContent -notmatch "Mobil DWG") {
    Fail "Production UI hierarchy does not contain standard viewer UI controls"
}
Write-Host "[A27-GATE] Production UI hierarchy verified!"

# ── STEP 9: Byte-Safe Production Screenshot ──────────────────────────────────
Write-Host "[A27-GATE] STEP 9: Capturing production screenshot..."
$ScreenshotPath = Join-Path $ArtifactsFullPath "a27-real-app-release-production.png"
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $ScreenshotPath
Assert-PngSignature -Path $ScreenshotPath
Write-Host "[A27-GATE] Screenshot saved: $ScreenshotPath ($([math]::Round((Get-Item $ScreenshotPath).Length/1KB,1)) KB)"

$BrainDir = "C:\Users\hsyn\.gemini\antigravity\brain\9b6886c2-7816-4bdb-afa5-f004834ccfbe"
if (Test-Path $BrainDir) {
    Copy-Item $ScreenshotPath (Join-Path $BrainDir "a27-real-app-release-production.png") -Force
}

# ── STEP 10: Measure Production Memory PSS ───────────────────────────────────
Write-Host "[A27-GATE] STEP 10: Measuring production Total PSS..."
$meminfo = (& $AdbExe -s $Serial shell dumpsys meminfo $Package | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath "meminfo_a27.txt") -Value $meminfo -Encoding UTF8
$totalPssMatch = [regex]::Match($meminfo, "TOTAL PSS:\s+(\d+)")
if ($totalPssMatch.Success) {
    $pssKb = [int]$totalPssMatch.Groups[1].Value
    $pssMb = [math]::Round($pssKb / 1024, 1)
    Write-Host "[A27-GATE] Total PSS: $pssMb MB"
    if ($pssMb -gt 250.0) { Fail "Total PSS exceeds 250 MB budget" }
    Write-Host "A27_MEMINFO_PSS_PASS pss=$pssMb MB"
}

# ── STEP 11: Cleanup ─────────────────────────────────────────────────────────
Write-Host "[A27-GATE] STEP 11: Cleaning up..."
& $AdbExe -s $Serial uninstall $Package | Out-Null

Write-Host "═══════════════════════════════════════════════════════════════════"
Write-Host "ANDROID_STAGE27_RELEASE_HANDOFF_PASS" -ForegroundColor Green
