$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Fail([string]$Message) {
    throw "STAGE01_DEVICE_GATE_FAIL: $Message"
}

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Fail "required command not found: $Name"
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

Require-Command 'dotnet'
Require-Command 'java'
Require-Command 'adb'

$DotnetVersion = (& dotnet --version).Trim()
if ($LASTEXITCODE -ne 0 -or $DotnetVersion -ne '10.0.400') {
    Fail "dotnet version is '$DotnetVersion'; expected 10.0.400"
}

$Workloads = (& dotnet workload list | Out-String)
if ($LASTEXITCODE -ne 0 -or $Workloads -notmatch '(?m)^maui-android\s') {
    Fail 'maui-android workload is not installed'
}
if ($Workloads -notmatch '10\.0\.400') {
    Fail 'maui-android is not resolved from workload set 10.0.400'
}

$JavaVersion = (& java -version 2>&1 | Out-String)
if ($LASTEXITCODE -ne 0 -or $JavaVersion -notmatch '21\.0\.12') {
    Fail 'Java 21.0.12 is required'
}

$AdbVersion = (& adb version | Out-String)
if ($LASTEXITCODE -ne 0 -or $AdbVersion -notmatch 'Version 37\.0\.1') {
    Fail 'ADB / Platform-Tools 37.0.1 is required'
}

$SdkRoot = if ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } elseif ($env:ANDROID_HOME) { $env:ANDROID_HOME } else { $null }
if (-not $SdkRoot) {
    Fail 'ANDROID_SDK_ROOT or ANDROID_HOME must be set'
}

if (-not (Test-Path (Join-Path $SdkRoot 'platforms\android-36\android.jar'))) {
    Fail "Android SDK Platform 36 is not installed under $SdkRoot"
}
if (-not (Test-Path (Join-Path $SdkRoot 'build-tools\36.0.0'))) {
    Fail "Android Build-Tools 36.0.0 is not installed under $SdkRoot"
}

& adb start-server | Out-Null
if ($LASTEXITCODE -ne 0) { Fail 'adb start-server failed' }

$DeviceSerials = @()
foreach ($Line in (& adb devices)) {
    if ($Line -match '^([^\s]+)\s+device\s*$') {
        $DeviceSerials += $Matches[1]
    }
}

if ($env:ANDROID_SERIAL) {
    $Serial = $env:ANDROID_SERIAL
    if ($DeviceSerials -notcontains $Serial) {
        Fail 'ANDROID_SERIAL does not identify an adb device in state=device'
    }
}
else {
    if ($DeviceSerials.Count -ne 1) {
        Fail 'connect exactly one authorized adb device, or set ANDROID_SERIAL'
    }
    $Serial = $DeviceSerials[0]
}

$IsEmulator = ((& adb -s $Serial shell getprop ro.kernel.qemu) | Out-String).Trim()
if ($LASTEXITCODE -ne 0) { Fail 'failed to query connected Android device' }
if ($IsEmulator -eq '1') { Fail 'connected target is an emulator; a physical Android device is required' }

$Manufacturer = ((& adb -s $Serial shell getprop ro.product.manufacturer) | Out-String).Trim()
$Model = ((& adb -s $Serial shell getprop ro.product.model) | Out-String).Trim()
$AndroidRelease = ((& adb -s $Serial shell getprop ro.build.version.release) | Out-String).Trim()
$AndroidApi = ((& adb -s $Serial shell getprop ro.build.version.sdk) | Out-String).Trim()

$WorkDir = Join-Path ([IO.Path]::GetTempPath()) ("mobil-dwg-stage01-" + [Guid]::NewGuid().ToString('N'))
$AppDir = Join-Path $WorkDir 'Stage01Smoke'

try {
    New-Item -ItemType Directory -Path $WorkDir | Out-Null

    Write-Host 'Creating clean MAUI smoke app...'
    & dotnet new maui -n Stage01Smoke -o $AppDir | Out-Host
    if ($LASTEXITCODE -ne 0) { Fail 'dotnet new maui failed' }

    $Csproj = Join-Path $AppDir 'Stage01Smoke.csproj'
    $CsprojText = Get-Content -Raw $Csproj
    $PinnedText = $CsprojText -replace '>21\.0</SupportedOSPlatformVersion>', '>24.0</SupportedOSPlatformVersion>'
    $PinnedText = $PinnedText -replace '<ApplicationId>com\.companyname\.stage01smoke</ApplicationId>', '<ApplicationId>com.smitelagwar.mobildwg.stage01smoke</ApplicationId>'
    if ($PinnedText -eq $CsprojText -or $PinnedText -notmatch '>24\.0</SupportedOSPlatformVersion>') {
        Fail 'failed to pin Android minimum API to 24.0'
    }
    if ($PinnedText -notmatch '<ApplicationId>com\.smitelagwar\.mobildwg\.stage01smoke</ApplicationId>') {
        Fail 'failed to pin smoke application id'
    }
    Set-Content -Path $Csproj -Value $PinnedText -Encoding utf8

    Write-Host 'Building Debug...'
    & dotnet build $Csproj -f net10.0-android -c Debug --no-restore | Out-Host
    if ($LASTEXITCODE -ne 0) { Fail 'Debug build failed' }

    Write-Host 'Building Release...'
    & dotnet build $Csproj -f net10.0-android -c Release --no-restore | Out-Host
    if ($LASTEXITCODE -ne 0) { Fail 'Release build failed' }

    $Manifest = Join-Path $AppDir 'obj\Debug\net10.0-android\android\manifest\AndroidManifest.xml'
    if (-not (Test-Path $Manifest)) { Fail 'generated Android manifest not found' }
    $ManifestText = Get-Content -Raw $Manifest
    if ($ManifestText -notmatch 'minSdkVersion="24(?:\.0)?"') { Fail 'generated manifest does not contain minSdkVersion=24' }
    if ($ManifestText -notmatch 'targetSdkVersion="36(?:\.0)?"') { Fail 'generated manifest does not contain targetSdkVersion=36' }

    $DebugDir = Join-Path $AppDir 'bin\Debug\net10.0-android'
    $DebugApk = Get-ChildItem -Path $DebugDir -Recurse -Filter '*-Signed.apk' -File | Select-Object -First 1
    if (-not $DebugApk) {
        $DebugApk = Get-ChildItem -Path $DebugDir -Recurse -Filter '*.apk' -File | Select-Object -First 1
    }
    if (-not $DebugApk) { Fail 'Debug APK not found' }

    $PackageName = 'com.smitelagwar.mobildwg.stage01smoke'
    Write-Host 'Installing Debug APK on physical device...'
    & adb -s $Serial install -r $DebugApk.FullName | Out-Host
    if ($LASTEXITCODE -ne 0) { Fail 'adb install failed' }

    $PackagePath = (& adb -s $Serial shell pm path $PackageName | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or $PackagePath -notmatch '^package:') {
        Fail 'package install could not be verified'
    }

    $Resolved = @(& adb -s $Serial shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $PackageName)
    if ($LASTEXITCODE -ne 0) { Fail 'launcher activity resolution failed' }
    $LauncherComponent = $Resolved | Where-Object { $_ -match '/' } | Select-Object -Last 1
    if (-not $LauncherComponent) { Fail 'launcher activity could not be resolved' }
    $LauncherComponent = $LauncherComponent.Trim()

    Write-Host 'Launching smoke app...'
    $LaunchOutput = (& adb -s $Serial shell am start -W $LauncherComponent | Out-String)
    if ($LASTEXITCODE -ne 0 -or $LaunchOutput -notmatch 'Status:\s+ok') {
        Fail 'Android activity launch did not report Status: ok'
    }

    Write-Host ''
    Write-Host 'STAGE01_DEVICE_GATE_PASS'
    Write-Host "dotnet=$DotnetVersion"
    Write-Host 'workload_set=10.0.400'
    Write-Host 'java=21.0.12'
    Write-Host 'adb=37.0.1'
    Write-Host 'android_sdk=36'
    Write-Host 'build_tools=36.0.0'
    Write-Host 'maui_android=installed'
    Write-Host 'manifest=minSdk24,targetSdk36'
    Write-Host 'device_state=device,physical'
    Write-Host "device=$Manufacturer $Model; Android $AndroidRelease; API $AndroidApi"
    Write-Host 'debug_build=PASS'
    Write-Host 'release_build=PASS'
    Write-Host 'install=PASS'
    Write-Host 'launch=PASS'
}
finally {
    if (Test-Path $WorkDir) {
        Remove-Item -Recurse -Force $WorkDir
    }
}
