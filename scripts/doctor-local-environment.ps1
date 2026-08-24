<#
.SYNOPSIS
    Diagnoses and verifies the health of the local Android build/test environment.
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
    } else {
        Write-Host " [FAIL] ${Name}: $Details" -ForegroundColor Red
        $script:AllOk = $false
    }
}

# 1. Hyper-V / Virtualization
$cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
Report-Check "Hardware Virtualization" ($cpu.VirtualizationFirmwareEnabled -eq $true) "Firmware Virtualization Enabled = $($cpu.VirtualizationFirmwareEnabled)"

# 2. .NET SDK
$dotnetVer = (& dotnet --version 2>$null).Trim()
Report-Check ".NET SDK Version" ($dotnetVer -eq "10.0.400") "Found: $dotnetVer (Expected: 10.0.400)"

# 3. .NET MAUI Workload
$workloads = (& dotnet workload list 2>$null | Out-String)
$hasMauiAndroid = $workloads -match "maui-android"
Report-Check ".NET MAUI Android Workload" $hasMauiAndroid "Installed Workload: maui-android ($($workloads.Trim()))"

# 4. Java / JDK
$javaHome = if ($env:JAVA_HOME) { 
    $env:JAVA_HOME 
} else { 
    [Environment]::GetEnvironmentVariable('JAVA_HOME', 'User') 
}
if (-not $javaHome) {
    $javaHome = [Environment]::GetEnvironmentVariable('JAVA_HOME', 'Machine')
}
if (-not $javaHome -and (Test-Path "C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot")) {
    $javaHome = "C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot"
}

$javaExe = if ($javaHome) { Join-Path $javaHome "bin\java.exe" } else { "java" }
$javaVer = if (Test-Path $javaExe) { 
    (& "$javaExe" -version 2>&1 | Select-Object -First 1)
} else { 
    try { & java -version 2>&1 | Select-Object -First 1 } catch { "Missing" }
}
$javaOk = ($javaVer -match "21\.") -and (Test-Path (Join-Path $javaHome "bin\javac.exe"))
Report-Check "Microsoft OpenJDK 21" $javaOk "JAVA_HOME=$javaHome ($javaVer)"

# 5. Android SDK Environment Variables
$sdkRoot = if ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } elseif ($env:ANDROID_HOME) { $env:ANDROID_HOME } else { "$env:LOCALAPPDATA\Android\Sdk" }
$sdkExists = Test-Path $sdkRoot
Report-Check "Android SDK Root" $sdkExists "Path: $sdkRoot"

# 6. Android SDK Platform 36
$plat36 = Test-Path "$sdkRoot\platforms\android-36\android.jar"
Report-Check "Android Platform 36" $plat36 "platforms\android-36\android.jar"

# 7. Android Build-Tools 36.0.0
$bt36 = Test-Path "$sdkRoot\build-tools\36.0.0\aapt2.exe"
Report-Check "Android Build-Tools 36.0.0" $bt36 "build-tools\36.0.0\aapt2.exe"

# 8. ADB & Platform-Tools
$adb = "$sdkRoot\platform-tools\adb.exe"
$adbOk = Test-Path $adb
$adbVer = if ($adbOk) { (& "$adb" --version | Select-Object -First 1) } else { "Missing" }
Report-Check "Android Platform-Tools (ADB)" $adbOk "$adbVer"

# 9. Android Emulator
$emulator = "$sdkRoot\emulator\emulator.exe"
$emulatorOk = Test-Path $emulator
$emulatorVer = if ($emulatorOk) { (& "$emulator" -version | Select-Object -First 1) } else { "Missing" }
Report-Check "Android Emulator" $emulatorOk "$emulatorVer"

# 10. AVD mobil-dwg-api36
$avdDir = "$env:USERPROFILE\.android\avd\mobil-dwg-api36.avd"
$avdOk = Test-Path $avdDir
Report-Check "AVD 'mobil-dwg-api36'" $avdOk "AVD Path: $avdDir"

# 11. GitHub Actions Runner
$runnerDir = "C:\actions-runner"
$runnerOk = Test-Path "$runnerDir\run.cmd"
Report-Check "GitHub Actions Runner Dir" $runnerOk "Path: $runnerDir"

Write-Host "=========================================================="
if ($AllOk) {
    Write-Host " ENVIRONMENT STATUS: 100% HEALTHY & READY FOR AUTOMATION!" -ForegroundColor Green
} else {
    Write-Host " ENVIRONMENT STATUS: ONE OR MORE CHECKS FAILED." -ForegroundColor Red
}
Write-Host "=========================================================="
