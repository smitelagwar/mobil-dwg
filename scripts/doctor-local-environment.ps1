<#
.SYNOPSIS
    Diagnoses and verifies the local Windows/Android build and emulator test environment.
.DESCRIPTION
    Emits LOCAL_ENVIRONMENT_DOCTOR_PASS and exits 0 only when every required V01 environment check passes.
    Emits LOCAL_ENVIRONMENT_DOCTOR_FAIL and exits 1 otherwise.
#>
$ErrorActionPreference = 'Continue'
Set-StrictMode -Version Latest

Write-Host "=========================================================="
Write-Host "  MOBIL-DWG LOCAL BUILD/TEST ENVIRONMENT DOCTOR"
Write-Host "=========================================================="

$AllOk = $true

function Report-Check([string]$Name, [bool]$Pass, [string]$Details) {
    if ($Pass) {
        Write-Host " [PASS] ${Name}: $Details" -ForegroundColor Green
    }
    else {
        Write-Host " [FAIL] ${Name}: $Details" -ForegroundColor Red
        $script:AllOk = $false
    }
}

# 1. Hyper-V / virtualization
try {
    $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
    Report-Check "Hardware Virtualization" ($null -ne $cpu -and $cpu.VirtualizationFirmwareEnabled -eq $true) "Firmware Virtualization Enabled = $($cpu.VirtualizationFirmwareEnabled)"
}
catch {
    Report-Check "Hardware Virtualization" $false $_.Exception.Message
}

# 2. .NET SDK
$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnetVer = if ($dotnetCommand) { (& dotnet --version 2>$null | Out-String).Trim() } else { "Missing" }
Report-Check ".NET SDK Version" ($dotnetVer -eq "10.0.400") "Found: $dotnetVer (Expected: 10.0.400)"

# 3. .NET MAUI Android workload
$workloads = if ($dotnetCommand) { (& dotnet workload list 2>$null | Out-String) } else { "" }
$hasMauiAndroid = $workloads -match "(?m)^maui-android\s"
Report-Check ".NET MAUI Android Workload" $hasMauiAndroid "maui-android installed = $hasMauiAndroid"

# 4. Java / JDK
$javaHome = if ($env:JAVA_HOME) {
    $env:JAVA_HOME
}
else {
    [Environment]::GetEnvironmentVariable('JAVA_HOME', 'User')
}
if (-not $javaHome) {
    $javaHome = [Environment]::GetEnvironmentVariable('JAVA_HOME', 'Machine')
}
if (-not $javaHome -and (Test-Path "C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot")) {
    $javaHome = "C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot"
}

$javaExe = if ($javaHome) { Join-Path $javaHome "bin\java.exe" } else { $null }
$javacExe = if ($javaHome) { Join-Path $javaHome "bin\javac.exe" } else { $null }
$javaVer = if ($javaExe -and (Test-Path $javaExe)) {
    (& $javaExe -version 2>&1 | Out-String).Trim()
}
else {
    "Missing"
}
$javaOk = ($javaVer -match '21\.0\.12') -and $javacExe -and (Test-Path $javacExe)
Report-Check "Microsoft OpenJDK 21.0.12" $javaOk "JAVA_HOME=$javaHome ($javaVer)"

# 5. Android SDK root
$sdkRoot = if ($env:ANDROID_SDK_ROOT) {
    $env:ANDROID_SDK_ROOT
}
elseif ($env:ANDROID_HOME) {
    $env:ANDROID_HOME
}
else {
    "$env:LOCALAPPDATA\Android\Sdk"
}
$sdkExists = Test-Path $sdkRoot
Report-Check "Android SDK Root" $sdkExists "Path: $sdkRoot"

# 6. Android SDK Platform 36
$plat36 = Test-Path (Join-Path $sdkRoot "platforms\android-36\android.jar")
Report-Check "Android Platform 36" $plat36 "platforms\android-36\android.jar"

# 7. Android Build-Tools 36.0.0
$buildToolsPath = Join-Path $sdkRoot "build-tools\36.0.0\aapt2.exe"
$bt36 = Test-Path $buildToolsPath
Report-Check "Android Build-Tools 36.0.0" $bt36 "build-tools\36.0.0\aapt2.exe"

# 8. ADB / Platform-Tools 37.0.1
$adb = Join-Path $sdkRoot "platform-tools\adb.exe"
$adbExists = Test-Path $adb
$adbVer = if ($adbExists) { (& $adb version 2>&1 | Out-String).Trim() } else { "Missing" }
$adbOk = $adbExists -and ($adbVer -match 'Version 37\.0\.1')
Report-Check "Android Platform-Tools 37.0.1" $adbOk $adbVer

# 9. Android Emulator executable
$emulator = Join-Path $sdkRoot "emulator\emulator.exe"
$emulatorExists = Test-Path $emulator
$emulatorVer = if ($emulatorExists) { (& $emulator -version 2>&1 | Select-Object -First 1 | Out-String).Trim() } else { "Missing" }
Report-Check "Android Emulator" $emulatorExists $emulatorVer

# 10. AVD mobil-dwg-api36
$avdDir = "$env:USERPROFILE\.android\avd\mobil-dwg-api36.avd"
$avdOk = Test-Path $avdDir
Report-Check "AVD 'mobil-dwg-api36'" $avdOk "AVD Path: $avdDir"

# 11. GitHub Actions runner directory
$runnerDir = "C:\actions-runner"
$runnerOk = Test-Path "$runnerDir\run.cmd"
Report-Check "GitHub Actions Runner Dir" $runnerOk "Path: $runnerDir"

Write-Host "=========================================================="
if ($AllOk) {
    Write-Host " ENVIRONMENT STATUS: 100% HEALTHY & READY FOR AUTOMATION!" -ForegroundColor Green
    Write-Host "LOCAL_ENVIRONMENT_DOCTOR_PASS" -ForegroundColor Green
    Write-Host "=========================================================="
    exit 0
}

Write-Host " ENVIRONMENT STATUS: ONE OR MORE CHECKS FAILED." -ForegroundColor Red
Write-Host "LOCAL_ENVIRONMENT_DOCTOR_FAIL" -ForegroundColor Red
Write-Host "=========================================================="
exit 1
