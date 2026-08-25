<#
.SYNOPSIS
    Android validation V04 real MobilDwg.App runtime gate.
.DESCRIPTION
    This script assumes the separate hardened V01 infrastructure prerequisite has
    already completed in the same job. It validates only the repository's real
    MobilDwg.App dependency/build/install/launch/UI/stability evidence.
#>
param(
    [string]$Configuration = "Release",
    [string]$ArtifactsDir = "artifacts/v04-real-app"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "V04_FAIL: $Message" -ForegroundColor Red
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
if (-not (Test-Path $ArtifactsFullPath)) { New-Item -ItemType Directory -Path $ArtifactsFullPath -Force | Out-Null }

Write-Host "=========================================================="
Write-Host " ANDROID VALIDATION V04 - REAL MobilDwg.App RUNTIME"
Write-Host "=========================================================="

$DotnetVersion = (& dotnet --version | Out-String).Trim()
if ($DotnetVersion -ne '10.0.400') { Fail "expected .NET SDK 10.0.400, got $DotnetVersion" }
$Workloads = (& dotnet workload list 2>&1 | Out-String)
Require-ExitCode "dotnet workload list"
if ($Workloads -notmatch '(?m)^maui-android\s') { Fail "maui-android workload is not installed" }

$AdbCommand = Get-Command adb -ErrorAction SilentlyContinue
if (-not $AdbCommand) { Fail "adb not found" }
$AdbExe = $AdbCommand.Source
$Serial = $null
foreach ($line in (& adb devices)) {
    if ($line.Trim() -match '^(emulator-\d+)\s+device$') {
        $candidate = $Matches[1]
        $boot = ((& adb -s $candidate shell getprop sys.boot_completed 2>$null) | Out-String).Trim()
        if ($boot -eq '1') { $Serial = $candidate; break }
    }
}
if (-not $Serial) { Fail "no booted emulator found after V01 infrastructure prerequisite" }
$AndroidApi = ((& adb -s $Serial shell getprop ro.build.version.sdk) | Out-String).Trim()
$AndroidRelease = ((& adb -s $Serial shell getprop ro.build.version.release) | Out-String).Trim()
$Abi = ((& adb -s $Serial shell getprop ro.product.cpu.abi) | Out-String).Trim()
if ($AndroidApi -ne '36') { Fail "expected emulator API 36, got API $AndroidApi" }
Write-Host "V04_EMULATOR_API36_PASS serial=$Serial android=$AndroidRelease abi=$Abi"

$Cpm = [xml](Get-Content -Raw 'Directory.Packages.props')
$mauiVersionNodes = @(@($Cpm.Project.ItemGroup.PackageVersion) | Where-Object { $_.Include -eq 'Microsoft.Maui.Controls' })
if ($mauiVersionNodes.Count -ne 1 -or $mauiVersionNodes[0].Version -ne '[10.0.100]') {
    Fail "Microsoft.Maui.Controls must be centrally pinned to exact [10.0.100]"
}

$AppProject = 'src/MobilDwg.App/MobilDwg.App.csproj'
$PackageGraphPath = Join-Path $ArtifactsFullPath 'app-package-graph.json'
$PackageGraph = (& dotnet list $AppProject package --include-transitive --format json 2>&1 | Out-String)
Require-ExitCode "real app package graph"
Set-Content -Path $PackageGraphPath -Value $PackageGraph -Encoding utf8
if ($PackageGraph -notmatch 'Microsoft\.Maui\.Controls' -or $PackageGraph -notmatch '10\.0\.100') {
    Fail "real app package graph did not resolve Microsoft.Maui.Controls 10.0.100"
}

$VulnerabilityPath = Join-Path $ArtifactsFullPath 'app-vulnerabilities.json'
$VulnerabilityJson = (& dotnet list $AppProject package --vulnerable --include-transitive --format json 2>&1 | Out-String)
Require-ExitCode "real app vulnerability query"
Set-Content -Path $VulnerabilityPath -Value $VulnerabilityJson -Encoding utf8
if ($VulnerabilityJson -match '"severity"\s*:') { Fail "NuGet reported a vulnerable real-app dependency" }

$GlobalPackagesLine = (& dotnet nuget locals global-packages --list | Out-String).Trim()
if ($GlobalPackagesLine -notmatch ':\s*(.+)$') { Fail "could not resolve NuGet global-packages folder" }
$GlobalPackages = $Matches[1].Trim()
$MauiPackageDir = Join-Path $GlobalPackages 'microsoft.maui.controls\10.0.100'
$MauiNuspec = Join-Path $MauiPackageDir 'microsoft.maui.controls.nuspec'
if (-not (Test-Path $MauiNuspec)) { Fail "Microsoft.Maui.Controls 10.0.100 nuspec not found" }
[xml]$NuspecXml = Get-Content -Raw $MauiNuspec
$LicenseNode = $NuspecXml.SelectSingleNode("//*[local-name()='license']")
if (-not $LicenseNode -or $LicenseNode.InnerText.Trim() -ne 'MIT') { Fail "Microsoft.Maui.Controls license is not exact MIT" }
$MauiNupkg = Get-ChildItem -Path $MauiPackageDir -Filter '*.nupkg' -File | Select-Object -First 1
if (-not $MauiNupkg) { Fail "Microsoft.Maui.Controls nupkg missing from NuGet cache" }
$MauiNupkgHash = (Get-FileHash -Algorithm SHA256 $MauiNupkg.FullName).Hash.ToLowerInvariant()
Set-Content -Path (Join-Path $ArtifactsFullPath 'maui-controls-direct-package.txt') -Encoding ascii -Value @(
    'id=Microsoft.Maui.Controls',
    'version=10.0.100',
    'license=MIT',
    "nupkg_sha256=$MauiNupkgHash"
)
Write-Host "V04_MAUI_EXACT_LICENSE_PASS sha256=$MauiNupkgHash"

& dotnet build $AppProject -f net10.0-android36.0 -c $Configuration --nologo | Out-Host
Require-ExitCode "real MobilDwg.App build"
$BinDir = Join-Path $RepoRoot "src/MobilDwg.App/bin/$Configuration/net10.0-android36.0"
$Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*-Signed.apk' -File | Select-Object -First 1
if (-not $Apk) { $Apk = Get-ChildItem -Path $BinDir -Recurse -Filter '*.apk' -File | Select-Object -First 1 }
if (-not $Apk) { Fail "no real MobilDwg.App APK found under $BinDir" }
$ApkHash = (Get-FileHash -Algorithm SHA256 $Apk.FullName).Hash.ToLowerInvariant()
$EvidenceApk = Join-Path $ArtifactsFullPath 'MobilDwg.App-Signed.apk'
Copy-Item -LiteralPath $Apk.FullName -Destination $EvidenceApk -Force
Write-Host "V04_REAL_APP_APK_PASS file=$($Apk.Name) bytes=$($Apk.Length) sha256=$ApkHash"

$PackageName = 'com.smitelagwar.mobildwg'
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& adb -s $Serial uninstall $PackageName 2>$null | Out-Null
$ErrorActionPreference = $previousEap
& adb -s $Serial install -r $EvidenceApk | Out-Host
Require-ExitCode "real MobilDwg.App adb install"
$PackagePath = ((& adb -s $Serial shell pm path $PackageName) | Out-String).Trim()
if ($PackagePath -notmatch '^package:') { Fail "installed package path not found for $PackageName" }
$Resolved = @(& adb -s $Serial shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $PackageName)
$Launcher = $Resolved | Where-Object { $_ -match '/' } | Select-Object -Last 1
if (-not $Launcher) { Fail "launcher activity could not be resolved for $PackageName" }
$Launcher = $Launcher.Trim()
Write-Host "V04_REAL_APP_INSTALL_PASS package=$PackageName launcher=$Launcher"

& adb -s $Serial shell am force-stop $PackageName | Out-Null
& adb -s $Serial logcat -c | Out-Null
& adb -s $Serial logcat -b crash -c | Out-Null
& adb -s $Serial logcat -b events -c | Out-Null
$LaunchOutput = ((& adb -s $Serial shell am start -W $Launcher) | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath 'launch.txt') -Value $LaunchOutput -Encoding utf8
Write-Host $LaunchOutput
if ($LaunchOutput -notmatch 'Status:\s+ok') { Fail "real app activity launch did not report Status: ok" }
Start-Sleep -Seconds 6
$AppPid = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
if ($AppPid -notmatch '^\d+$') { Fail "real app PID not found after launch" }
Write-Host "V04_REAL_APP_LAUNCH_PASS pid=$AppPid"

$UiRemote = '/sdcard/v04-window.xml'
$UiLocal = Join-Path $ArtifactsFullPath 'window.xml'
& adb -s $Serial shell uiautomator dump $UiRemote | Out-Null
Require-ExitCode "uiautomator dump"
& adb -s $Serial pull $UiRemote $UiLocal | Out-Null
Require-ExitCode "uiautomator pull"
$UiXml = Get-Content -Raw $UiLocal
if ($UiXml -notmatch 'Mobil DWG' -or $UiXml -notmatch 'Android app shell ready') {
    Fail "real app UI markers were not visible in UIAutomator hierarchy"
}
$WindowState = (& adb -s $Serial shell dumpsys window windows | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath 'window-state.txt') -Value $WindowState -Encoding utf8
if ($WindowState -notmatch [regex]::Escape($PackageName)) { Fail "window manager evidence does not contain the real package name" }
Write-Host "V04_REAL_APP_UI_PASS"

$Screenshot = Join-Path $ArtifactsFullPath 'real-app-launch.png'
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $Screenshot
Assert-PngSignature $Screenshot
$RawCrash = (& adb -s $Serial logcat -b crash -d | Out-String)
$RawEvents = (& adb -s $Serial logcat -b events -d | Out-String)
$RawLogcat = (& adb -s $Serial logcat -d -t 800 | Out-String)
$Meminfo = (& adb -s $Serial shell dumpsys meminfo $PackageName | Out-String)
Set-Content -Path (Join-Path $ArtifactsFullPath 'crash-logcat.txt') -Value $RawCrash -Encoding utf8
Set-Content -Path (Join-Path $ArtifactsFullPath 'events-logcat.txt') -Value $RawEvents -Encoding utf8
Set-Content -Path (Join-Path $ArtifactsFullPath 'logcat.txt') -Value ($RawLogcat -replace '(?i)(token|secret|authorization|bearer)\s*[:=]\s*[^\s]+', '$1=[REDACTED]') -Encoding utf8
Set-Content -Path (Join-Path $ArtifactsFullPath 'meminfo.txt') -Value $Meminfo -Encoding utf8
$PackagePattern = [regex]::Escape($PackageName)
$AppPidPattern = "(?<!\d)$AppPid(?!\d)"
$HasCrash = ($RawCrash -match '(?i)FATAL EXCEPTION|Fatal signal|Process .* has died') -and (($RawCrash -match $PackagePattern) -or ($RawCrash -match $AppPidPattern))
if ($HasCrash) { Fail "package/PID scoped crash evidence detected for real app" }
$HasAnr = ($RawEvents -match '(?i)am_anr') -and (($RawEvents -match $PackagePattern) -or ($RawEvents -match $AppPidPattern))
if ($HasAnr) { Fail "post-launch ANR evidence detected for real app" }
$AppPidStillAlive = ((& adb -s $Serial shell pidof -s $PackageName 2>$null) | Out-String).Trim()
if ($AppPidStillAlive -ne $AppPid) { Fail "real app process did not remain alive with PID $AppPid" }
Write-Host "V04_REAL_APP_STABILITY_PASS"

$HeadSha = (& git rev-parse HEAD | Out-String).Trim()
$Summary = @"
ANDROID VALIDATION V04 SUMMARY
Exact checked-out SHA: $HeadSha
Scope: real installable MobilDwg.App Android shell only
.NET SDK: $DotnetVersion
Target: net10.0-android36.0
Emulator: $Serial / Android $AndroidRelease / API $AndroidApi / $Abi
Package: $PackageName
Launcher: $Launcher
PID: $AppPid
APK: $($Apk.Name)
APK SHA-256: $ApkHash
Microsoft.Maui.Controls: 10.0.100 exact / MIT / nupkg $MauiNupkgHash
Architecture/Core/Rendering prerequisite: PASS in separate hardened V01 workflow step
Install: PASS
Cold launch: PASS (Status: ok)
UI hierarchy markers: PASS
PNG screenshot: PASS
Crash/ANR/liveness: PASS
Physical Android: DEFERRED_RELEASE_DEVICE_GATE
Claim limit: REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
Result: ANDROID_VALIDATION_V04_PASS
"@
Set-Content -Path (Join-Path $ArtifactsFullPath 'summary.txt') -Value $Summary -Encoding ascii
Write-Host "ANDROID_VALIDATION_V04_PASS" -ForegroundColor Green
Write-Host "CLAIM_LIMIT=REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY" -ForegroundColor Yellow
exit 0
