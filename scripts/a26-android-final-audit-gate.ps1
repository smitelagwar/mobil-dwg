#Requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

<#
.SYNOPSIS
    AŞAMA 26 — Dependency freeze / final audit / RC approval kabul testi gate scripti.
#>

function Fail([string]$Message) {
    Write-Host "A26_FAIL: $Message" -ForegroundColor Red
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
$ArtifactsFullPath = Join-Path $RepoRoot "artifacts\a26-android-final-audit"
$Package     = "com.smitelagwar.mobildwg"
$LogcatLog   = Join-Path $ArtifactsFullPath "logcat_a26.txt"

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
Write-Host "[A26-GATE] Device connected: $Serial (API $AndroidApi)"

# ── STEP 1: Toolchain and Locked-Mode Verification ───────────────────────────
Write-Host "[A26-GATE] STEP 1: Toolchain and locked restore verification..."
$DotnetVer = (& dotnet --version | Out-String).Trim()
if ($DotnetVer -ne "10.0.400") { Fail "Expected .NET SDK 10.0.400, found $DotnetVer" }

$restoreOut = & dotnet restore $ProbeProject --locked-mode 2>&1 | Out-String
Write-Host $restoreOut
Require-ExitCode "locked restore"

$vulnOut = & dotnet list $ProbeProject package --vulnerable --include-transitive 2>&1 | Out-String
Write-Host $vulnOut
if ($vulnOut -match "(?i)has the following vulnerable packages") {
    Fail "Vulnerable package detected in dependency graph!"
}
Write-Host "[A26-GATE] Locked restore & vulnerability check: PASS"

# ── STEP 2: Host Tests ────────────────────────────────────────────────────────
Write-Host "[A26-GATE] STEP 2: Running host tests (net10.0)..."
$hostOut = & dotnet run --project $TestProject -c Release 2>&1 | Out-String
Write-Host $hostOut
if ($hostOut -notmatch "STAGE26_FINAL_AUDIT_TESTS_PASS") { Fail "Host tests failed" }
Set-Content -Path (Join-Path $ArtifactsFullPath "host_test_output.txt") -Value $hostOut -Encoding UTF8

# ── STEP 3: Build Release AAB ────────────────────────────────────────────────
Write-Host "[A26-GATE] STEP 3: Building Release AAB bundle..."
$aabBuildArgs = @(
    "build", $AppProject,
    "-c", "Release",
    "-f", "net10.0-android36.0",
    "-p:AndroidPackageFormat=aab",
    "-p:AndroidKeyStore=false",
    "--nologo"
)
$aabBuildOut = & dotnet @aabBuildArgs 2>&1 | Out-String
Require-ExitCode "AAB Build"

$aabPattern = Join-Path $RepoRoot "src\MobilDwg.App\bin\Release\net10.0-android36.0\*.aab"
$aabPath = Get-ChildItem -Path $aabPattern -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*-Signed*" } |
    Select-Object -First 1
if (-not $aabPath) {
    $aabPath = Get-ChildItem -Path $aabPattern -ErrorAction SilentlyContinue | Select-Object -First 1
}
if (-not $aabPath) { Fail "AAB not found after build" }
$aabSizeMb = [math]::Round($aabPath.Length / 1MB, 2)
Write-Host "[A26-GATE] AAB: $($aabPath.FullName) ($aabSizeMb MB)"
if ($aabPath.Length -gt 45L * 1024 * 1024) { Fail "AAB size exceeds 45 MB budget: $aabSizeMb MB" }

# ── STEP 4: Build Release APK with A26Validation ─────────────────────────────
Write-Host "[A26-GATE] STEP 4: Building APK with A26Validation=true..."
$apkBuildArgs = @(
    "build", $AppProject,
    "-c", "Release",
    "-f", "net10.0-android36.0",
    "-p:A26Validation=true",
    "-p:AndroidKeyStore=false",
    "--nologo"
)
$buildOut = & dotnet @apkBuildArgs 2>&1 | Out-String
Write-Host $buildOut
Require-ExitCode "APK Build"

$apkPattern = Join-Path $RepoRoot "src\MobilDwg.App\bin\Release\net10.0-android36.0\*.apk"
$apkPath = Get-ChildItem -Path $apkPattern -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*-Signed*" } |
    Select-Object -First 1
if (-not $apkPath) {
    $apkPath = Get-ChildItem -Path $apkPattern -ErrorAction SilentlyContinue | Select-Object -First 1
}
if (-not $apkPath) { Fail "APK not found after build" }
$apkSizeMb = [math]::Round($apkPath.Length / 1MB, 2)
Write-Host "[A26-GATE] APK: $($apkPath.FullName) ($apkSizeMb MB)"
if ($apkPath.Length -gt 45L * 1024 * 1024) { Fail "APK size exceeds 45 MB budget: $apkSizeMb MB" }

# ── STEP 5: APK Internal Binary & Font Inspection ────────────────────────────
Write-Host "[A26-GATE] STEP 5: Inspecting APK internal binaries and assets..."
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($apkPath.FullName)
try {
    $soEntries = @($zip.Entries | Where-Object { $_.FullName -like "*.so" })
    Write-Host "[A26-GATE] Native libraries found in APK ($($soEntries.Count)):"
    foreach ($entry in $soEntries) {
        $name = [System.IO.Path]::GetFileName($entry.FullName)
        $isDotNetRuntime = $name.StartsWith("libaot-") -or
                           $name.StartsWith("libmono") -or
                           $name.StartsWith("libxamarin") -or
                           $name.StartsWith("libSystem.") -or
                           $name -in @("libSystem.Native.so", "libassembly-store.so", "libarc.bin.so", "libnet7.so")
        $isApprovedSkia = ($name -eq "libSkiaSharp.so")

        if (-not $isDotNetRuntime -and -not $isApprovedSkia) {
            Fail "Unauthorized native binary detected in APK: $($entry.FullName)"
        }
    }
    Write-Host "[A26-GATE] Native binary audit passed: 100% SkiaSharp and official .NET runtime components."

    $shxEntries = @($zip.Entries | Where-Object { $_.FullName -like "*.shx" })
    if ($shxEntries.Count -gt 0) {
        Fail "Proprietary SHX font file bundled in APK! Count=$($shxEntries.Count)"
    }
    Write-Host "[A26-GATE] Zero proprietary SHX fonts verified in APK."
}
finally {
    $zip.Dispose()
}

# ── STEP 6: Install APK ───────────────────────────────────────────────────────
Write-Host "[A26-GATE] STEP 6: Installing APK on $Serial..."
$installOut = & $AdbExe -s $Serial install -r $apkPath.FullName 2>&1 | Out-String
Write-Host $installOut
if ($installOut -notmatch "Success") { Fail "APK install failed: $installOut" }

# ── STEP 7: Resolve launcher + Launch app ────────────────────────────────────
Write-Host "[A26-GATE] STEP 7: Launching application..."
$resolveOut = (& $AdbExe -s $Serial shell cmd package resolve-activity --brief $Package 2>&1 | Out-String)
$launcherLine = ($resolveOut -split "`r?`n" | Where-Object { $_ -match "^$Package/" } | Select-Object -First 1)
if (-not $launcherLine) {
    $launcherLine = "$Package/crc64d52a5cdc4f267319.MainActivity"
}
Write-Host "[A26-GATE] Launcher: $launcherLine"

& $AdbExe -s $Serial shell input keyevent KEYCODE_WAKEUP | Out-Null
& $AdbExe -s $Serial shell wm dismiss-keyguard | Out-Null
& $AdbExe -s $Serial logcat -c 2>&1 | Out-Null
& $AdbExe -s $Serial shell am force-stop $Package 2>&1 | Out-Null
& $AdbExe -s $Serial shell am start -n $launcherLine 2>&1 | Out-Null
Require-ExitCode "App Launch"

# ── STEP 8: Poll logcat for completion ───────────────────────────────────────
Write-Host "[A26-GATE] STEP 8: Waiting for A26 validation in logcat..."
$Done = $false
for ($i = 0; $i -lt 45; $i++) {
    Start-Sleep -Seconds 1
    $Logs = (& $AdbExe -s $Serial logcat -d -s MobilDwgA26:V | Out-String)
    if ($Logs -match "A26_REAL_APP_UI_IMAGE_READY") {
        $Done = $true
        break
    }
    if ($Logs -match "ANDROID_STAGE26_RC_APPROVAL_FAIL") {
        Fail "validation runner reported failure: $Logs"
    }
}
if (-not $Done) { Fail "timed out waiting for A26_REAL_APP_UI_IMAGE_READY" }

Start-Sleep -Milliseconds 1500
$LogcatDump = (& $AdbExe -s $Serial logcat -d -s MobilDwgA26:V | Out-String)
Set-Content -Path $LogcatLog -Value $LogcatDump -Encoding UTF8
Write-Host $LogcatDump

# ── STEP 9: Verify required markers ──────────────────────────────────────────
Write-Host "[A26-GATE] STEP 9: Verifying logcat markers..."
$ReqMarkers = @(
    "A26_TOOLCHAIN_FREEZE_PASS",
    "A26_DEPENDENCY_FREEZE_PASS",
    "A26_NATIVE_ASSET_AUDIT_PASS",
    "A26_FONT_SUBSTITUTION_AUDIT_PASS",
    "A26_DATA_SAFETY_AUDIT_PASS",
    "A26_FINAL_RC_APPROVAL_PASS",
    "A26_RC_APPROVAL_SNAPSHOT_PASS",
    "A26_PROOF_PNG_READY",
    "ANDROID_STAGE26_RC_APPROVAL_PASS",
    "A26_REAL_APP_UI_IMAGE_READY"
)
foreach ($m in $ReqMarkers) {
    if ($LogcatDump -notmatch $m) {
        Fail "missing expected marker in logcat: $m"
    }
    $line = ($LogcatDump -split "`r?`n" | Where-Object { $_ -match $m } | Select-Object -First 1)
    Write-Host "  [OK] $line"
}

# ── STEP 10: UI Hierarchy Dump ───────────────────────────────────────────────
Write-Host "[A26-GATE] STEP 10: UI hierarchy dump..."
$RemoteDump = "/sdcard/a26_window_dump.xml"
& $AdbExe -s $Serial shell uiautomator dump $RemoteDump | Out-Null
$LocalDump = Join-Path $ArtifactsFullPath "a26_window.xml"
& $AdbExe -s $Serial pull $RemoteDump $LocalDump
& $AdbExe -s $Serial shell rm -f $RemoteDump | Out-Null

$UiContent = [System.IO.File]::ReadAllText($LocalDump)
if ($UiContent -notmatch "ANDROID_STAGE26_RC_APPROVAL_PASS") {
    Fail "UI hierarchy does not contain ANDROID_STAGE26_RC_APPROVAL_PASS text"
}
Write-Host "[A26-GATE] UI hierarchy verified!"

# ── STEP 11: Byte-Safe Screenshot ────────────────────────────────────────────
Write-Host "[A26-GATE] STEP 11: Capturing screenshot..."
$ScreenshotPath = Join-Path $ArtifactsFullPath "a26-real-app-rc-approval.png"
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $ScreenshotPath
Assert-PngSignature -Path $ScreenshotPath
Write-Host "[A26-GATE] Screenshot saved: $ScreenshotPath ($([math]::Round((Get-Item $ScreenshotPath).Length/1KB,1)) KB)"

# Copy to brain artifact folder
$BrainDir = "C:\Users\hsyn\.gemini\antigravity\brain\9b6886c2-7816-4bdb-afa5-f004834ccfbe"
if (Test-Path $BrainDir) {
    Copy-Item $ScreenshotPath (Join-Path $BrainDir "a26-real-app-rc-approval.png") -Force
}

# ── STEP 12: Memory PSS Measurement ──────────────────────────────────────────
Write-Host "[A26-GATE] STEP 12: Measuring Dumpsys Meminfo PSS..."
$meminfo = (& $AdbExe -s $Serial shell dumpsys meminfo $Package | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath "meminfo_a26.txt") -Value $meminfo -Encoding UTF8
$totalPssMatch = [regex]::Match($meminfo, "TOTAL PSS:\s+(\d+)")
if ($totalPssMatch.Success) {
    $pssKb = [int]$totalPssMatch.Groups[1].Value
    $pssMb = [math]::Round($pssKb / 1024, 1)
    Write-Host "[A26-GATE] Total PSS: $pssMb MB"
    if ($pssMb -gt 250.0) { Fail "Total PSS $pssMb MB exceeds 250 MB budget" }
}

# ── STEP 13: Cleanup ─────────────────────────────────────────────────────────
Write-Host "[A26-GATE] STEP 13: Cleaning up app..."
& $AdbExe -s $Serial uninstall $Package | Out-Null

Write-Host "═══════════════════════════════════════════════════════════════════"
Write-Host "ANDROID_STAGE26_RC_APPROVAL_PASS" -ForegroundColor Green
