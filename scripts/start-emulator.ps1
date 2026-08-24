<#
.SYNOPSIS
    Starts the mobil-dwg-api36 Android Emulator in background with GPU acceleration.
.DESCRIPTION
    Launches the Android Virtual Device 'mobil-dwg-api36' (API 36, x86_64, Google APIs)
    and waits until ADB reports sys.boot_completed=1.
#>
param(
    [string]$AvdName = "mobil-dwg-api36",
    [int]$TimeoutSeconds = 120,
    [switch]$Headless
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$SdkRoot = if ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } elseif ($env:ANDROID_HOME) { $env:ANDROID_HOME } else { "$env:LOCALAPPDATA\Android\Sdk" }
$EmulatorExe = Join-Path $SdkRoot "emulator\emulator.exe"
$AdbExe = Join-Path $SdkRoot "platform-tools\adb.exe"

if (-not (Test-Path $EmulatorExe)) {
    throw "Emulator executable not found at: $EmulatorExe"
}
if (-not (Test-Path $AdbExe)) {
    throw "ADB executable not found at: $AdbExe"
}

# Check if already running
$devices = & $AdbExe devices
if ($devices -match 'emulator-\d+\s+device') {
    Write-Host "[OK] Android Emulator is already running and ready."
    exit 0
}

Write-Host "Starting Android Emulator ($AvdName)..."
$argsList = @("-avd", $AvdName, "-no-boot-anim", "-no-snapshot")
if ($Headless) {
    $argsList += @("-no-window", "-gpu", "swiftshader_indirect")
}

Start-Process -FilePath $EmulatorExe -ArgumentList $argsList

Write-Host "Waiting for emulator to appear in ADB..."
& $AdbExe wait-for-device

Write-Host "Waiting for sys.boot_completed=1..."
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$booted = $false

while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
    $val = (& $AdbExe shell getprop sys.boot_completed 2>$null).Trim()
    if ($val -eq "1") {
        $booted = $true
        break
    }
    Start-Sleep -Seconds 2
}

if (-not $booted) {
    throw "Emulator failed to boot within $TimeoutSeconds seconds."
}

$sdkVer = (& $AdbExe shell getprop ro.build.version.sdk 2>$null).Trim()
$releaseVer = (& $AdbExe shell getprop ro.build.version.release 2>$null).Trim()
$model = (& $AdbExe shell getprop ro.product.model 2>$null).Trim()

Write-Host "=========================================="
Write-Host "Emulator Started Successfully!"
Write-Host "Device Model : $model"
Write-Host "Android OS   : $releaseVer"
Write-Host "API Level    : $sdkVer"
Write-Host "=========================================="
