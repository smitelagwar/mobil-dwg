<#
.SYNOPSIS
    Gracefully terminates any running Android Emulator instances.
#>
$ErrorActionPreference = 'SilentlyContinue'
Set-StrictMode -Version Latest

$SdkRoot = if ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } elseif ($env:ANDROID_HOME) { $env:ANDROID_HOME } else { "$env:LOCALAPPDATA\Android\Sdk" }
$AdbExe = Join-Path $SdkRoot "platform-tools\adb.exe"

if (Test-Path $AdbExe) {
    Write-Host "Sending emu kill via ADB..."
    & $AdbExe emu kill | Out-Null
    Start-Sleep -Seconds 2
}

$procs = Get-Process -Name "emulator", "qemu-system-x86_64" -ErrorAction SilentlyContinue
if ($procs) {
    Write-Host "Stopping emulator processes..."
    $procs | Stop-Process -Force
}

Write-Host "Android Emulator stopped."
