<#
.SYNOPSIS
    Android Emulator Build, Install, and Launch Automated Gate.
.DESCRIPTION
    1. Validates exact Android/.NET toolchain requirements.
    2. Reuses or boots the pinned Android emulator.
    3. Builds MobilDwg.sln and executes every executable contract/architecture harness with marker checks.
    4. Builds the temporary Stage01Smoke MAUI Android APK used only for V01 infrastructure validation.
    5. Installs and launches that APK.
    6. Requires a live numeric process PID and checks package/PID-scoped crash and post-launch ANR evidence.
    7. Captures byte-safe PNG, logcat, meminfo, ANR and device artifacts.
    8. Emits ANDROID_EMULATOR_GATE_PASS only when every V01 infrastructure check passes.

    IMPORTANT: Stage01Smoke is an infrastructure smoke APK. It is NOT MobilDwg.App and this gate must
    never be interpreted as viewer/parser/rendering fidelity PASS.
#>
param(
    [string]$Configuration = "Debug",
    [string]$ArtifactsDir = "artifacts/android-emulator-result",
    [string]$AvdName = "mobil-dwg-api36",
    [int]$BootTimeoutSeconds = 180,
    [switch]$Headless,
    [switch]$BuildBothConfigs
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Fail([string]$Message) {
    Write-Host "`n[FAIL] ANDROID_EMULATOR_GATE_FAIL: $Message" -ForegroundColor Red
    exit 1
}

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Fail "Required command not found in PATH: $Name"
    }
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
    if (-not $process.Start()) {
        Fail "Failed to start adb screencap process."
    }

    $stderrTask = $process.StandardError.ReadToEndAsync()
    $fileStream = [System.IO.File]::Create($OutputPath)
    try {
        $process.StandardOutput.BaseStream.CopyTo($fileStream)
    }
    finally {
        $fileStream.Dispose()
    }

    $process.WaitForExit()
    $stderr = $stderrTask.Result
    if ($process.ExitCode -ne 0) {
        Fail "adb screencap failed with exit code $($process.ExitCode): $stderr"
    }
}

function Assert-PngSignature([string]$Path) {
    if (-not (Test-Path $Path)) {
        Fail "Screenshot file was not created: $Path"
    }

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $expected = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
    if ($bytes.Length -lt $expected.Length) {
        Fail "Screenshot is too small to be a valid PNG ($($bytes.Length) bytes)."
    }

    for ($i = 0; $i -lt $expected.Length; $i++) {
        if ($bytes[$i] -ne $expected[$i]) {
            $actualHex = (($bytes[0..7] | ForEach-Object { $_.ToString('X2') }) -join ' ')
            Fail "Screenshot PNG magic bytes are invalid. Expected '89 50 4E 47 0D 0A 1A 0A', got '$actualHex'."
        }
    }
}

function Invoke-ExecutableHarness {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectPath,
        [Parameter(Mandatory = $true)][string[]]$RequiredMarkers,
        [Parameter(Mandatory = $true)][string]$Configuration
    )

    Write-Host " Running executable harness: $ProjectPath"
    $output = (& dotnet run --project $ProjectPath -c $Configuration 2>&1 | Out-String)
    $exitCode = $LASTEXITCODE
    Write-Host $output

    if ($exitCode -ne 0) {
        Fail "Executable harness failed ($ProjectPath), exit code $exitCode."
    }

    foreach ($marker in $RequiredMarkers) {
        if ($output -notmatch [regex]::Escape($marker)) {
            Fail "Executable harness '$ProjectPath' did not emit required marker '$marker'."
        }
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

Write-Host "=========================================================="
Write-Host "  ANDROID EMULATOR AUTOMATED TEST GATE - V01 HARDENED"
Write-Host "=========================================================="
Write-Host "Configuration : $Configuration"
Write-Host "AVD Name      : $AvdName"
Write-Host "Artifacts Dir : $ArtifactsDir"
Write-Host "Scope         : Stage01Smoke infrastructure only; NOT MobilDwg.App viewer validation"
Write-Host "=========================================================="

# 1. Environment & Toolchain Verification
Write-Host "`n[1/7] Verifying Toolchain & Environment..."

$JavaHome = if ($env:JAVA_HOME) {
    $env:JAVA_HOME
} else {
    [Environment]::GetEnvironmentVariable('JAVA_HOME', 'User')
}
if (-not $JavaHome) {
    $JavaHome = [Environment]::GetEnvironmentVariable('JAVA_HOME', 'Machine')
}
if (-not $JavaHome -and (Test-Path "C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot")) {
    $JavaHome = "C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot"
}
if ($JavaHome -and (Test-Path $JavaHome)) {
    $env:JAVA_HOME = $JavaHome
    $javaBin = Join-Path $JavaHome "bin"
    if ($env:PATH -notmatch [regex]::Escape($javaBin)) {
        $env:PATH = "$javaBin;$env:PATH"
    }
}

$SdkRoot = if ($env:ANDROID_SDK_ROOT) {
    $env:ANDROID_SDK_ROOT
} elseif ($env:ANDROID_HOME) {
    $env:ANDROID_HOME
} else {
    "$env:LOCALAPPDATA\Android\Sdk"
}
if (-not (Test-Path $SdkRoot)) {
    Fail "Android SDK root not found at: $SdkRoot"
}
$env:ANDROID_SDK_ROOT = $SdkRoot
$env:ANDROID_HOME = $SdkRoot

$PlatformTools = Join-Path $SdkRoot "platform-tools"
$EmulatorDir = Join-Path $SdkRoot "emulator"
$BuildToolsDir = Join-Path $SdkRoot "build-tools\36.0.0"
if ($env:PATH -notmatch [regex]::Escape($PlatformTools)) {
    $env:PATH = "$PlatformTools;$EmulatorDir;$BuildToolsDir;$env:PATH"
}

Require-Command 'dotnet'
Require-Command 'java'
Require-Command 'adb'

$AdbExe = (Get-Command adb).Source
$prevEAP = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
$DotnetVer = (& dotnet --version).Trim()
$Workloads = (& dotnet workload list 2>&1 | Out-String)
$JavaVer = (& java -version 2>&1 | Out-String)
$AdbVer = (& adb version 2>&1 | Out-String)
$ErrorActionPreference = $prevEAP

if ($DotnetVer -ne '10.0.400') { Fail "dotnet version is '$DotnetVer'; expected 10.0.400" }
if ($Workloads -notmatch '(?m)^maui-android\s') { Fail "maui-android workload is not installed" }
if ($JavaVer -notmatch '21\.0\.12') { Fail "Java 21.0.12 is required (Found: $JavaVer)" }
if ($AdbVer -notmatch 'Version 37\.0\.1') { Fail "ADB / Platform-Tools 37.0.1 is required" }
if (-not (Test-Path (Join-Path $SdkRoot 'platforms\android-36\android.jar'))) { Fail "Android SDK Platform 36 is not installed under $SdkRoot" }
if (-not (Test-Path (Join-Path $BuildToolsDir 'aapt2.exe'))) { Fail "Android Build-Tools 36.0.0 is not installed under $SdkRoot" }

$EmulatorExe = Join-Path $EmulatorDir "emulator.exe"
if (-not (Test-Path $EmulatorExe)) { Fail "Android Emulator executable not found at $EmulatorExe" }
Write-Host " [PASS] Toolchain verified: .NET 10.0.400, maui-android, Java 21.0.12, ADB 37.0.1, API 36, Build-Tools 36.0.0"

# 2. Android Emulator Startup & Health Check
Write-Host "`n[2/7] Checking Android Emulator State..."
& adb start-server | Out-Null

$Serial = $null
$RunningDevices = @()
foreach ($Line in (& adb devices)) {
    $trimmed = $Line.Trim()
    if ($trimmed -match '^(emulator-\d+)\s+device$') {
        $RunningDevices += $Matches[1]
    }
}

$EmulatorRunning = $false
foreach ($dev in $RunningDevices) {
    $bootCompleted = ((& adb -s $dev shell getprop sys.boot_completed 2>$null) | Out-String).Trim()
    if ($bootCompleted -eq "1") {
        $Serial = $dev
        $EmulatorRunning = $true
        Write-Host " [REUSE] Running emulator detected ($Serial) with sys.boot_completed=1"
        break
    }
}

if (-not $EmulatorRunning) {
    Write-Host " Starting Android Emulator ($AvdName)..."
    $emuArgs = @("-avd", $AvdName, "-no-boot-anim", "-no-snapshot")
    if ($Headless) { $emuArgs += @("-no-window", "-gpu", "swiftshader_indirect") }
    $emuProcess = Start-Process -FilePath $EmulatorExe -ArgumentList $emuArgs -PassThru

    Write-Host " Waiting for emulator to appear and boot (Timeout: ${BootTimeoutSeconds}s)..."
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $booted = $false
    while ($sw.Elapsed.TotalSeconds -lt $BootTimeoutSeconds) {
        if ($emuProcess.HasExited) { Fail "Emulator process exited unexpectedly with code $($emuProcess.ExitCode)." }
        $deviceLines = & adb devices
        $foundSerial = $null
        foreach ($line in $deviceLines) {
            $trimmed = $line.Trim()
            if ($trimmed -match '^(emulator-\d+)\s+device$') {
                $foundSerial = $Matches[1]
                break
            }
        }
        if ($foundSerial) {
            $bootVal = ((& adb -s $foundSerial shell getprop sys.boot_completed 2>$null) | Out-String).Trim()
            if ($bootVal -eq "1") {
                $Serial = $foundSerial
                $booted = $true
                break
            }
        }
        Start-Sleep -Seconds 3
    }
    if (-not $booted -or -not $Serial) { Fail "Emulator failed to boot and report sys.boot_completed=1 within $BootTimeoutSeconds seconds." }
}

$IsQemu = ((& adb -s $Serial shell getprop ro.kernel.qemu 2>$null) | Out-String).Trim()
$Model = ((& adb -s $Serial shell getprop ro.product.model 2>$null) | Out-String).Trim()
$Manufacturer = ((& adb -s $Serial shell getprop ro.product.manufacturer 2>$null) | Out-String).Trim()
$AndroidRelease = ((& adb -s $Serial shell getprop ro.build.version.release 2>$null) | Out-String).Trim()
$AndroidApi = ((& adb -s $Serial shell getprop ro.build.version.sdk 2>$null) | Out-String).Trim()
$Abi = ((& adb -s $Serial shell getprop ro.product.cpu.abi 2>$null) | Out-String).Trim()
Write-Host " [PASS] Target Emulator: $Serial ($Manufacturer $Model, Android $AndroidRelease, API $AndroidApi, ABI $Abi, QEMU=$IsQemu)"

# 3. Solution Build + executable test harnesses
Write-Host "`n[3/7] Building solution and executing contract/architecture harnesses..."
$slnPath = Join-Path $RepoRoot "MobilDwg.sln"
if (-not (Test-Path $slnPath)) { Fail "MobilDwg.sln not found." }

& dotnet build $slnPath -c $Configuration | Out-Host
if ($LASTEXITCODE -ne 0) { Fail "MobilDwg.sln build failed." }

$Harnesses = @(
    @{
        Path = "tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj"
        Markers = @("STAGE04_CORE_CONTRACT_TESTS_PASS")
    },
    @{
        Path = "tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj"
        Markers = @("STAGE04_RENDER_CONTRACT_TESTS_PASS", "STAGE09_RENDER_SCENE_TESTS_PASS")
    },
    @{
        Path = "tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj"
        Markers = @("STAGE04_ARCHITECTURE_TESTS_PASS", "STAGE05_DEPENDENCY_BOUNDARY_PASS")
    }
)
foreach ($harness in $Harnesses) {
    Invoke-ExecutableHarness -ProjectPath $harness.Path -RequiredMarkers $harness.Markers -Configuration $Configuration
}
Write-Host " [PASS] EXECUTABLE_HARNESS_MARKERS_PASS"

# 4. Temporary Stage01Smoke MAUI Android Build
Write-Host "`n[4/7] Building temporary Stage01Smoke MAUI Android package..."
$WorkDir = Join-Path ([IO.Path]::GetTempPath()) ("mobil-dwg-gate-" + [Guid]::NewGuid().ToString('N'))
$AppDir = Join-Path $WorkDir 'Stage01Smoke'
$BuiltApkPath = $null
$PackageName = 'com.smitelagwar.mobildwg.stage01smoke'

$ArtifactsFullPath = Join-Path $RepoRoot $ArtifactsDir
if (-not (Test-Path $ArtifactsFullPath)) { New-Item -ItemType Directory -Path $ArtifactsFullPath -Force | Out-Null }
$ScreenshotsDir = Join-Path $ArtifactsFullPath "screenshots"
if (-not (Test-Path $ScreenshotsDir)) { New-Item -ItemType Directory -Path $ScreenshotsDir -Force | Out-Null }

try {
    New-Item -ItemType Directory -Path $WorkDir | Out-Null
    & dotnet new maui -n Stage01Smoke -o $AppDir | Out-Null
    if ($LASTEXITCODE -ne 0) { Fail "dotnet new maui failed" }

    $Csproj = Join-Path $AppDir 'Stage01Smoke.csproj'
    $CsprojText = Get-Content -Raw $Csproj
    $CsprojText = $CsprojText -replace '<TargetFrameworks Condition="!\$\(\[MSBuild\]::IsOSPlatform\(''linux''\)\)">\$\(TargetFrameworks\);net10.0-ios;net10.0-maccatalyst</TargetFrameworks>', ''
    $CsprojText = $CsprojText -replace '<TargetFrameworks Condition="\$\(\[MSBuild\]::IsOSPlatform\(''windows''\)\)">\$\(TargetFrameworks\);net10.0-windows10\.0\.19041\.0</TargetFrameworks>', ''
    $CsprojText = $CsprojText -replace '>21\.0</SupportedOSPlatformVersion>', '>24.0</SupportedOSPlatformVersion>'
    $CsprojText = $CsprojText -replace '<ApplicationId>com\.companyname\.stage01smoke</ApplicationId>', "<ApplicationId>$PackageName</ApplicationId>"
    $standaloneProps = @"
    <EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
    <AndroidFastDeploymentType>None</AndroidFastDeploymentType>
  </PropertyGroup>
"@
    $CsprojText = $CsprojText -replace '</PropertyGroup>', $standaloneProps
    Set-Content -Path $Csproj -Value $CsprojText -Encoding utf8

    if ($BuildBothConfigs) {
        Write-Host " Building Debug..."
        & dotnet build $Csproj -f net10.0-android -c Debug | Out-Host
        if ($LASTEXITCODE -ne 0) { Fail "Debug build failed" }
        Write-Host " Building Release..."
        & dotnet build $Csproj -f net10.0-android -c Release | Out-Host
        if ($LASTEXITCODE -ne 0) { Fail "Release build failed" }
    } else {
        Write-Host " Building $Configuration..."
        & dotnet build $Csproj -f net10.0-android -c $Configuration | Out-Host
        if ($LASTEXITCODE -ne 0) { Fail "$Configuration build failed" }
    }

    $TargetConfig = if ($BuildBothConfigs) { "Release" } else { $Configuration }
    $BinDir = Join-Path $AppDir "bin\$TargetConfig\net10.0-android"
    $Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*-Signed.apk' -File | Select-Object -First 1
    if (-not $Apk) { $Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*.apk' -File | Select-Object -First 1 }
    if (-not $Apk) { Fail "No APK found in $BinDir" }
    $BuiltApkPath = $Apk.FullName
    Write-Host " [PASS] Stage01Smoke APK generated: $($Apk.Name) ($TargetConfig)"

    # 5. Install on Emulator
    Write-Host "`n[5/7] Installing Stage01Smoke APK on Android Emulator..."
    & adb -s $Serial install -r $BuiltApkPath | Out-Host
    if ($LASTEXITCODE -ne 0) { Fail "adb install failed" }
    $PackagePath = ((& adb -s $Serial shell pm path $PackageName) | Out-String).Trim()
    if ($PackagePath -notmatch '^package:') { Fail "Package install verification failed for $PackageName" }
    Write-Host " [PASS] Package installed successfully: $PackagePath"

    # 6. Launch Application and require a live PID
    Write-Host "`n[6/7] Launching Stage01Smoke on Emulator..."
    & adb -s $Serial logcat -c | Out-Null
    & adb -s $Serial logcat -b crash -c | Out-Null
    & adb -s $Serial logcat -b events -c | Out-Null

    $Resolved = @(& adb -s $Serial shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $PackageName)
    $LauncherComponent = $Resolved | Where-Object { $_ -match '/' } | Select-Object -Last 1
    if (-not $LauncherComponent) { $LauncherComponent = "$PackageName/crc6409ad6dd81ea0ff3c.MainActivity" }
    $LauncherComponent = $LauncherComponent.Trim()

    Write-Host " Resolved Launcher Component: $LauncherComponent"
    $LaunchOutput = ((& adb -s $Serial shell am start -W $LauncherComponent) | Out-String)
    Write-Host $LaunchOutput
    if ($LaunchOutput -notmatch 'Status:\s+ok') { Fail "Android activity launch did not report 'Status: ok'" }

    Write-Host " Waiting 5 seconds for UI stabilization and telemetry..."
    Start-Sleep -Seconds 5

    $pidText = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
    if ($pidText -notmatch '^\d+$') { Fail "Launcher process PID was not found for $PackageName after successful am start." }
    $pidNum = $pidText

    # 7. Collect and validate diagnostic artifacts
    Write-Host "`n[7/7] Collecting and validating diagnostic artifacts..."

    $ScreenshotPath = Join-Path $ScreenshotsDir "emulator_launch.png"
    Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $ScreenshotPath
    Assert-PngSignature -Path $ScreenshotPath
    Write-Host " [PASS] Screenshot is byte-safe PNG (magic bytes 89 50 4E 47 0D 0A 1A 0A) -> $ScreenshotPath"

    $LogcatPath = Join-Path $ArtifactsFullPath "logcat.txt"
    $rawLogcat = (& adb -s $Serial logcat -d -t 500 | Out-String)
    $sanitizedLogcat = $rawLogcat -replace '(?i)(token|secret|authorization|bearer)\s*[:=]\s*[^\s]+', '$1=[REDACTED]'
    Set-Content -Path $LogcatPath -Value $sanitizedLogcat -Encoding utf8

    $CrashLogPath = Join-Path $ArtifactsFullPath "crash-logcat.txt"
    $rawCrashLog = (& adb -s $Serial logcat -b crash -d | Out-String)
    Set-Content -Path $CrashLogPath -Value $rawCrashLog -Encoding utf8

    $AnrEventsPath = Join-Path $ArtifactsFullPath "anr-events.txt"
    $rawEvents = (& adb -s $Serial logcat -b events -d | Out-String)
    $lastAnr = (& adb -s $Serial shell dumpsys activity lastanr 2>&1 | Out-String)
    $anrEvidence = "=== POST-LAUNCH EVENTS BUFFER ===`r`n$rawEvents`r`n=== DUMPSYS ACTIVITY LASTANR (context only; may be historical) ===`r`n$lastAnr"
    Set-Content -Path $AnrEventsPath -Value $anrEvidence -Encoding utf8

    $MeminfoPath = Join-Path $ArtifactsFullPath "meminfo.txt"
    $meminfo = (& adb -s $Serial shell dumpsys meminfo $PackageName | Out-String)
    Set-Content -Path $MeminfoPath -Value $meminfo -Encoding utf8

    $DeviceInfoPath = Join-Path $ArtifactsFullPath "device-info.txt"
    $deviceProps = (& adb -s $Serial shell getprop | Out-String)
    Set-Content -Path $DeviceInfoPath -Value $deviceProps -Encoding utf8

    $packagePattern = [regex]::Escape($PackageName)
    $pidPattern = "(?<!\d)$pidNum(?!\d)"
    $hasPackageCrash = ($rawCrashLog -match '(?i)FATAL EXCEPTION|Fatal signal|Process .* has died') -and (($rawCrashLog -match $packagePattern) -or ($rawCrashLog -match $pidPattern))
    if ($hasPackageCrash) { Fail "Package/PID-scoped crash evidence detected for $PackageName (PID $pidNum)." }

    $hasPostLaunchAnr = ($rawEvents -match '(?i)am_anr') -and (($rawEvents -match $packagePattern) -or ($rawEvents -match $pidPattern))
    if ($hasPostLaunchAnr) { Fail "Post-launch ANR event detected for $PackageName (PID $pidNum)." }

    $pidStillAlive = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
    if ($pidStillAlive -ne $pidNum) { Fail "Application process did not remain alive with expected PID $pidNum (found '$pidStillAlive')." }

    Write-Host " [PASS] App process verified (PID: $pidNum); no package/PID crash or post-launch ANR evidence."

    $SummaryPath = Join-Path $ArtifactsFullPath "summary.txt"
    $timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
    $summary = @"
==========================================================
ANDROID EMULATOR GATE TEST SUMMARY - V01 HARDENED
==========================================================
Timestamp        : $timestamp
Test Scope       : Stage01Smoke infrastructure only; NOT MobilDwg.App/viewer validation
Target Emulator  : $Serial
Device Model     : $Manufacturer $Model
Android OS       : Android $AndroidRelease (API $AndroidApi, $Abi)
QEMU / Emulator  : $IsQemu
.NET SDK         : $DotnetVer
MAUI Workload    : maui-android
Java Runtime     : OpenJDK 21.0.12
ADB Version      : 37.0.1
Build Config     : $Configuration
Harnesses        : PASS (Core, Rendering, Architecture markers verified)
Built APK        : $($Apk.Name)
Package Name     : $PackageName
App Process PID  : $pidNum
Activity Launch  : Status: ok
Screenshot PNG   : PASS (89 50 4E 47 0D 0A 1A 0A)
Crash Check      : PASS (package/PID-scoped crash buffer)
ANR Check        : PASS (no post-launch am_anr event for package/PID; dumpsys lastanr retained as context)
Claim Limit      : INFRASTRUCTURE_SMOKE_ONLY
Result           : ANDROID_EMULATOR_GATE_PASS
==========================================================
"@
    Set-Content -Path $SummaryPath -Value $summary -Encoding utf8
    Write-Host " Summary saved -> $SummaryPath"

    Write-Host "`n=========================================================="
    Write-Host "ANDROID_EMULATOR_GATE_PASS" -ForegroundColor Green
    Write-Host "CLAIM_LIMIT=INFRASTRUCTURE_SMOKE_ONLY" -ForegroundColor Yellow
    Write-Host "=========================================================="
    exit 0
}
finally {
    if (Test-Path $WorkDir) {
        Remove-Item -Recurse -Force $WorkDir -ErrorAction SilentlyContinue
    }
}
