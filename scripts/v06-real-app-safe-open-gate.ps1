param(
    [string]$Configuration = "Release",
    [string]$ArtifactsDir = "artifacts/v06-safe-open"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "V06_FAIL: $Message" -ForegroundColor Red
    exit 1
}

function Require-ExitCode([string]$Step) {
    if ($LASTEXITCODE -ne 0) { Fail "$Step failed with exit code $LASTEXITCODE" }
}

function Get-BootedEmulator {
    foreach ($line in (& adb devices)) {
        if ($line.Trim() -match '^(emulator-\d+)\s+device$') {
            $candidate = $Matches[1]
            $boot = ((& adb -s $candidate shell getprop sys.boot_completed 2>$null) | Out-String).Trim()
            if ($boot -eq '1') { return $candidate }
        }
    }
    return $null
}

function Get-UiXml {
    param([string]$Serial, [string]$Stem)
    $remote = "/sdcard/$Stem.xml"
    $local = Join-Path $ArtifactsFull "$Stem.xml"
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & adb -s $Serial shell uiautomator dump $remote 2>$null | Out-Null
    $dumpExit = $LASTEXITCODE
    if ($dumpExit -eq 0) {
        & adb -s $Serial pull $remote $local 2>$null | Out-Null
    }
    $pullExit = $LASTEXITCODE
    $ErrorActionPreference = $previous
    if ($dumpExit -ne 0 -or $pullExit -ne 0 -or -not (Test-Path $local)) { return $null }
    try { return [xml](Get-Content -Raw $local) } catch { return $null }
}

function Find-UiBounds {
    param([xml]$Xml, [string]$Text, [switch]$Contains)
    if ($null -eq $Xml) { return $null }
    foreach ($node in $Xml.SelectNodes('//node')) {
        $nodeText = [string]$node.GetAttribute('text')
        $nodeDesc = [string]$node.GetAttribute('content-desc')
        $match = if ($Contains) {
            $nodeText.Contains($Text, [StringComparison]::OrdinalIgnoreCase) -or
            $nodeDesc.Contains($Text, [StringComparison]::OrdinalIgnoreCase)
        } else {
            [string]::Equals($nodeText, $Text, [StringComparison]::OrdinalIgnoreCase) -or
            [string]::Equals($nodeDesc, $Text, [StringComparison]::OrdinalIgnoreCase)
        }
        if ($match) {
            $bounds = [string]$node.GetAttribute('bounds')
            if ($bounds -match '^\[(\d+),(\d+)\]\[(\d+),(\d+)\]$') { return $bounds }
        }
    }
    return $null
}

function Click-Bounds {
    param([string]$Serial, [string]$Bounds)
    if ($Bounds -notmatch '^\[(\d+),(\d+)\]\[(\d+),(\d+)\]$') { return $false }
    $x = [int](($Matches[1] + $Matches[3]) / 2)
    $y = [int](($Matches[2] + $Matches[4]) / 2)
    & adb -s $Serial shell input tap $x $y | Out-Null
    return $LASTEXITCODE -eq 0
}

function Try-ClickUiText {
    param([string]$Serial, [string]$Text, [string]$Stem, [switch]$Contains)
    $xml = Get-UiXml -Serial $Serial -Stem $Stem
    $bounds = Find-UiBounds -Xml $xml -Text $Text -Contains:$Contains
    if (-not $bounds) { return $false }
    return Click-Bounds -Serial $Serial -Bounds $bounds
}

function Wait-ClickUiText {
    param([string]$Serial, [string]$Text, [string]$Stem, [int]$Attempts = 30, [switch]$Contains)
    for ($i = 0; $i -lt $Attempts; $i++) {
        if (Try-ClickUiText -Serial $Serial -Text $Text -Stem "$Stem-$i" -Contains:$Contains) { return }
        Start-Sleep -Milliseconds 500
    }
    Fail "UI text was not clickable: $Text"
}

function Wait-UiText {
    param([string]$Serial, [string]$Text, [string]$Stem, [int]$Attempts = 30, [switch]$Contains)
    for ($i = 0; $i -lt $Attempts; $i++) {
        $xml = Get-UiXml -Serial $Serial -Stem "$Stem-$i"
        if (Find-UiBounds -Xml $xml -Text $Text -Contains:$Contains) { return }
        Start-Sleep -Milliseconds 500
    }
    Fail "UI text was not observed: $Text"
}

function Wait-LogMarker {
    param([string]$Serial, [string]$Marker, [int]$Attempts = 40)
    for ($i = 0; $i -lt $Attempts; $i++) {
        $log = (& adb -s $Serial logcat -d | Out-String)
        if ($log -match [regex]::Escape($Marker)) { return }
        if ($log -match 'V06_(PICKER|OPEN|CLOSE)_FAIL') { Fail "real app emitted a V06 failure marker" }
        Start-Sleep -Milliseconds 750
    }
    Fail "log marker not observed: $Marker"
}

function Select-Document {
    param([string]$Serial, [string]$FileName, [string]$Stem)
    $clickedDownloads = $false
    for ($i = 0; $i -lt 40; $i++) {
        if (Try-ClickUiText -Serial $Serial -Text $FileName -Stem "$Stem-file-$i") { return }
        if (-not $clickedDownloads -and (Try-ClickUiText -Serial $Serial -Text 'Downloads' -Stem "$Stem-downloads-$i" -Contains)) {
            $clickedDownloads = $true
            Start-Sleep -Seconds 1
            continue
        }
        Start-Sleep -Milliseconds 500
    }
    Fail "DocumentsUI did not expose selected test file: $FileName"
}

function Invoke-AdbBinaryToFile {
    param([string]$AdbPath, [string]$Serial, [string]$OutputPath)
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $AdbPath
    $psi.Arguments = "-s $Serial exec-out screencap -p"
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $true
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    if (-not $process.Start()) { Fail "failed to start adb screencap" }
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $stream = [System.IO.File]::Create($OutputPath)
    try { $process.StandardOutput.BaseStream.CopyTo($stream) } finally { $stream.Dispose() }
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) { Fail "adb screencap failed: $($stderrTask.Result)" }
}

function Assert-PngSignature([string]$Path) {
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $expected = [byte[]](0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A)
    if ($bytes.Length -lt 8) { Fail "screenshot too small" }
    for ($i = 0; $i -lt 8; $i++) { if ($bytes[$i] -ne $expected[$i]) { Fail "screenshot is not PNG" } }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot
$ArtifactsFull = Join-Path $RepoRoot $ArtifactsDir
if (Test-Path $ArtifactsFull) { Remove-Item $ArtifactsFull -Recurse -Force }
New-Item -ItemType Directory -Force -Path $ArtifactsFull | Out-Null

Write-Host "=========================================================="
Write-Host " ANDROID VALIDATION V06 - REAL APP FILEPICKER / SAF"
Write-Host "=========================================================="

$dotnetVersion = (& dotnet --version | Out-String).Trim()
if ($dotnetVersion -ne '10.0.400') { Fail "expected .NET SDK 10.0.400, got $dotnetVersion" }
$workloads = (& dotnet workload list 2>&1 | Out-String)
Require-ExitCode "dotnet workload list"
if ($workloads -notmatch '(?m)^maui-android\s') { Fail "maui-android workload missing" }

$adbCommand = Get-Command adb -ErrorAction SilentlyContinue
if (-not $adbCommand) { Fail "adb not found" }
$AdbExe = $adbCommand.Source
$Serial = Get-BootedEmulator
if (-not $Serial) { Fail "no booted emulator found after V01 prerequisite" }
$api = ((& adb -s $Serial shell getprop ro.build.version.sdk) | Out-String).Trim()
if ($api -ne '36') { Fail "expected API 36 emulator, got $api" }
Write-Host "V06_EMULATOR_API36_PASS serial=$Serial"

# Re-run the historical Stage 06 headless safe-open semantics on the exact revision.
$inputs = Join-Path $ArtifactsFull 'inputs'
New-Item -ItemType Directory -Force -Path $inputs | Out-Null
$generatedDwg = Join-Path $inputs 'v06_test.dwg'
& powershell -NoProfile -ExecutionPolicy Bypass -File 'scripts/stage03-generate-synthetic-dwg.ps1' -OutputDwg $generatedDwg | Out-Host
Require-ExitCode "V03 synthetic DWG generation"
$generatedDwg = (Resolve-Path $generatedDwg).Path
$dxfSource = (Resolve-Path 'fixtures/public/synthetic/synthetic_turkish_basic_ac1015.dxf').Path

& dotnet restore 'MobilDwg.sln' | Out-Host
Require-ExitCode "solution restore"
& dotnet build 'MobilDwg.sln' -c $Configuration --no-restore --nologo -warnaserror | Out-Host
Require-ExitCode "solution build"
& dotnet run --project 'tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj' -c $Configuration --no-build | Out-Host
Require-ExitCode "architecture harness"

& dotnet restore 'tools/Stage06.OpenFlowProbe/Stage06.OpenFlowProbe.csproj' | Out-Host
Require-ExitCode "Stage06 probe restore"
& dotnet build 'tools/Stage06.OpenFlowProbe/Stage06.OpenFlowProbe.csproj' -c $Configuration --no-restore --nologo -warnaserror | Out-Host
Require-ExitCode "Stage06 probe build"
$headlessEvidence = Join-Path $ArtifactsFull 'stage06-headless-evidence.json'
$headlessCache = Join-Path $ArtifactsFull 'headless-cache'
$probeOutput = (& dotnet run --project 'tools/Stage06.OpenFlowProbe/Stage06.OpenFlowProbe.csproj' -c $Configuration --no-build -- --cache-root $headlessCache --dwg $generatedDwg --dxf $dxfSource --evidence $headlessEvidence 2>&1 | Out-String)
$probeExit = $LASTEXITCODE
Set-Content (Join-Path $ArtifactsFull 'stage06-headless-console.txt') $probeOutput -Encoding utf8
Write-Host $probeOutput
if ($probeExit -ne 0) { Fail "Stage06 headless probe failed with exit code $probeExit" }
foreach ($marker in @('STAGE06_ACTUAL_DWG_DXF_PASS','STAGE06_SAFE_COPY_GUARDS_PASS','STAGE06_LAST_REQUEST_WINS_PASS','STAGE06_CANCEL_SEMANTICS_PASS','STAGE06_T2_HEADLESS_PASS')) {
    if ($probeOutput -notmatch [regex]::Escape($marker)) { Fail "Stage06 headless marker missing: $marker" }
}
Write-Host "V06_HEADLESS_SAFE_OPEN_REGRESSION_PASS"

$pickerSource = Get-Content -Raw 'src/MobilDwg.App/Opening/MauiCadFilePickerAdapter.cs'
if ($pickerSource -notmatch 'FilePicker\.Default\.PickAsync' -or $pickerSource -notmatch 'OpenReadAsync') { Fail "production MAUI picker adapter is not wired to FilePicker/OpenReadAsync" }
if ($pickerSource -match 'FullPath') { Fail "production picker adapter must not depend on provider FullPath" }
$appOpeningSource = (Get-ChildItem 'src/MobilDwg.App/Opening' -Filter '*.cs' -Recurse | Get-Content -Raw) -join "`n"
if ($appOpeningSource -match 'TakePersistableUriPermission') { Fail "V06 immediate-copy path must not take persistable URI permission" }
Write-Host "V06_STREAM_SAF_BRIDGE_STATIC_PASS"

# Build the real repository app with validation-only logging enabled.
$appProject = 'src/MobilDwg.App/MobilDwg.App.csproj'
& dotnet build $appProject -f net10.0-android36.0 -c $Configuration -t:Rebuild --nologo -warnaserror '-p:V06Validation=true' | Out-Host
Require-ExitCode "V06 validation MobilDwg.App build"
$binDir = Join-Path $RepoRoot "src/MobilDwg.App/bin/$Configuration/net10.0-android36.0"
$apk = Get-ChildItem $binDir -Filter '*-Signed.apk' -Recurse | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (-not $apk) { $apk = Get-ChildItem $binDir -Filter '*.apk' -Recurse | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1 }
if (-not $apk) { Fail "V06 validation APK not found" }
$apkHash = (Get-FileHash -Algorithm SHA256 $apk.FullName).Hash.ToLowerInvariant()
$evidenceApk = Join-Path $ArtifactsFull 'MobilDwg.App-V06-Signed.apk'
Copy-Item $apk.FullName $evidenceApk -Force
Write-Host "V06_REAL_APP_APK_PASS bytes=$($apk.Length) sha256=$apkHash"

$manifest = Get-ChildItem 'src/MobilDwg.App/obj' -Recurse -Filter 'AndroidManifest.xml' | Where-Object { $_.FullName -match [regex]::Escape($Configuration) } | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (-not $manifest) { Fail "generated AndroidManifest.xml not found" }
$manifestText = Get-Content -Raw $manifest.FullName
if ($manifestText -match 'android\.permission\.(READ_EXTERNAL_STORAGE|WRITE_EXTERNAL_STORAGE|MANAGE_EXTERNAL_STORAGE)') { Fail "real app requests broad external storage permission" }
Copy-Item $manifest.FullName (Join-Path $ArtifactsFull 'AndroidManifest.xml') -Force
Write-Host "V06_NO_BROAD_STORAGE_PERMISSION_PASS"

$package = 'com.smitelagwar.mobildwg'
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& adb -s $Serial uninstall $package 2>$null | Out-Null
$ErrorActionPreference = $previousEap
& adb -s $Serial install $apk.FullName | Out-Host
Require-ExitCode "V06 validation APK install"
$launcher = ((& adb -s $Serial shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $package 2>$null) | Select-Object -Last 1 | Out-String).Trim()
if (-not $launcher -or $launcher -notmatch [regex]::Escape($package)) { Fail "launcher resolution failed" }
Write-Host "V06_REAL_APP_INSTALL_PASS package=$package launcher=$launcher"

$externalDwg = '/sdcard/Download/v06_test.dwg'
$externalDxf = '/sdcard/Download/v06_test.dxf'
& adb -s $Serial shell mkdir -p /sdcard/Download | Out-Null
& adb -s $Serial push $generatedDwg $externalDwg | Out-Host
Require-ExitCode "push V06 DWG"
& adb -s $Serial push $dxfSource $externalDxf | Out-Host
Require-ExitCode "push V06 DXF"
$dwgHashBefore = (Get-FileHash -Algorithm SHA256 $generatedDwg).Hash.ToLowerInvariant()
$dxfHashBefore = (Get-FileHash -Algorithm SHA256 $dxfSource).Hash.ToLowerInvariant()

& adb -s $Serial shell am force-stop $package | Out-Null
& adb -s $Serial logcat -c | Out-Null
& adb -s $Serial logcat -b crash -c | Out-Null
& adb -s $Serial logcat -b events -c | Out-Null
$launch = (& adb -s $Serial shell am start -W -n $launcher | Out-String)
Set-Content (Join-Path $ArtifactsFull 'launch.txt') $launch -Encoding utf8
Write-Host $launch
if ($launch -notmatch 'Status:\s+ok') { Fail "real app launch did not report Status: ok" }
Wait-UiText -Serial $Serial -Text 'DWG/DXF seç' -Stem 'main-ready'
Wait-LogMarker -Serial $Serial -Marker 'V06_REAL_APP_READY'
$pidInitial = ((& adb -s $Serial shell pidof -s $package 2>$null) | Out-String).Trim()
if ($pidInitial -notmatch '^\d+$') { Fail "real app PID missing after launch" }

# Real Android FilePicker / DocumentsUI / SAF: DWG selection.
Wait-ClickUiText -Serial $Serial -Text 'DWG/DXF seç' -Stem 'open-dwg'
Wait-LogMarker -Serial $Serial -Marker 'V06_PICKER_LAUNCH'
Select-Document -Serial $Serial -FileName 'v06_test.dwg' -Stem 'select-dwg'
Wait-LogMarker -Serial $Serial -Marker 'V06_REAL_APP_SAFE_OPEN_PASS format=Dwg'
Wait-UiText -Serial $Serial -Text 'Hazır: Dwg' -Stem 'dwg-ready' -Contains
Write-Host "V06_REAL_APP_DWG_SAF_PASS"

# Rapid second selection: real picker returns a DXF and the latest generation owns the UI/session.
Wait-ClickUiText -Serial $Serial -Text 'DWG/DXF seç' -Stem 'open-dxf'
Select-Document -Serial $Serial -FileName 'v06_test.dxf' -Stem 'select-dxf'
Wait-LogMarker -Serial $Serial -Marker 'V06_REAL_APP_SAFE_OPEN_PASS format=Dxf'
Wait-UiText -Serial $Serial -Text 'Hazır: Dxf' -Stem 'dxf-ready' -Contains
Write-Host "V06_REAL_APP_SECOND_SELECTION_PASS"

# Picker cancellation must return safely without changing the active drawing.
Wait-ClickUiText -Serial $Serial -Text 'DWG/DXF seç' -Stem 'open-cancel'
Start-Sleep -Seconds 1
& adb -s $Serial shell input keyevent 4 | Out-Null
Wait-LogMarker -Serial $Serial -Marker 'V06_PICKER_CANCEL_PASS'
Write-Host "V06_REAL_APP_PICKER_CANCEL_PASS"

# Rotate configuration change: process stays alive and current safe-open state remains visible.
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& adb -s $Serial shell cmd window user-rotation lock 1 2>$null | Out-Null
$rotationExit = $LASTEXITCODE
$ErrorActionPreference = $previousEap
if ($rotationExit -ne 0) {
    & adb -s $Serial shell settings put system accelerometer_rotation 0 | Out-Null
    & adb -s $Serial shell settings put system user_rotation 1 | Out-Null
}
Start-Sleep -Seconds 2
$pidAfterRotate = ((& adb -s $Serial shell pidof -s $package 2>$null) | Out-String).Trim()
if ($pidAfterRotate -ne $pidInitial) { Fail "real app PID changed during rotate configuration test" }
Wait-UiText -Serial $Serial -Text 'Hazır: Dxf' -Stem 'rotate-state' -Contains
Write-Host "V06_REAL_APP_ROTATE_PASS pid=$pidAfterRotate"
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& adb -s $Serial shell cmd window user-rotation free 2>$null | Out-Null
$ErrorActionPreference = $previousEap

# Background/foreground: session survives and process remains healthy.
& adb -s $Serial shell input keyevent 3 | Out-Null
Start-Sleep -Seconds 2
$pidBackground = ((& adb -s $Serial shell pidof -s $package 2>$null) | Out-String).Trim()
if ($pidBackground -ne $pidInitial) { Fail "real app process died in background" }
$foreground = (& adb -s $Serial shell am start -W -n $launcher | Out-String)
if ($foreground -notmatch 'Status:\s+ok') { Fail "real app foreground resume did not report Status: ok" }
Start-Sleep -Seconds 1
$pidForeground = ((& adb -s $Serial shell pidof -s $package 2>$null) | Out-String).Trim()
if ($pidForeground -ne $pidInitial) { Fail "real app PID changed across background/foreground" }
Wait-UiText -Serial $Serial -Text 'Hazır: Dxf' -Stem 'foreground-state' -Contains
Write-Host "V06_REAL_APP_BACKGROUND_FOREGROUND_PASS pid=$pidForeground"

# Close cleans the private copy/session, then reopen through the real picker again.
Wait-ClickUiText -Serial $Serial -Text 'Çizimi kapat' -Stem 'close'
Wait-LogMarker -Serial $Serial -Marker 'V06_CLOSE_CLEANUP_PASS files=0'
Write-Host "V06_REAL_APP_CLOSE_CLEANUP_PASS"
Wait-ClickUiText -Serial $Serial -Text 'DWG/DXF seç' -Stem 'reopen'
Select-Document -Serial $Serial -FileName 'v06_test.dxf' -Stem 'reopen-dxf'
Wait-UiText -Serial $Serial -Text 'Hazır: Dxf' -Stem 'reopen-ready' -Contains
Write-Host "V06_REAL_APP_REOPEN_PASS"

# Selected source bytes must remain immutable after FilePicker/SAF/private-copy/parse lifecycle.
$roundtrip = Join-Path $ArtifactsFull 'roundtrip'
New-Item -ItemType Directory -Force -Path $roundtrip | Out-Null
& adb -s $Serial pull $externalDwg (Join-Path $roundtrip 'v06_test_after.dwg') | Out-Host
Require-ExitCode "pull V06 DWG after test"
& adb -s $Serial pull $externalDxf (Join-Path $roundtrip 'v06_test_after.dxf') | Out-Host
Require-ExitCode "pull V06 DXF after test"
$dwgHashAfter = (Get-FileHash -Algorithm SHA256 (Join-Path $roundtrip 'v06_test_after.dwg')).Hash.ToLowerInvariant()
$dxfHashAfter = (Get-FileHash -Algorithm SHA256 (Join-Path $roundtrip 'v06_test_after.dxf')).Hash.ToLowerInvariant()
if ($dwgHashAfter -ne $dwgHashBefore -or $dxfHashAfter -ne $dxfHashBefore) { Fail "selected external CAD source bytes changed" }
Write-Host "V06_ORIGINAL_INPUT_IMMUTABLE_PASS"

$rawCrash = (& adb -s $Serial logcat -b crash -d | Out-String)
$rawEvents = (& adb -s $Serial logcat -b events -d | Out-String)
$rawLog = (& adb -s $Serial logcat -d | Out-String)
$v06Lines = @($rawLog -split "`r?`n" | Where-Object { $_ -match 'MobilDwgV06' })
$v06Log = ($v06Lines -join [Environment]::NewLine)
Set-Content (Join-Path $ArtifactsFull 'v06-markers-logcat.txt') $v06Log -Encoding utf8
Set-Content (Join-Path $ArtifactsFull 'crash-logcat.txt') $rawCrash -Encoding utf8
Set-Content (Join-Path $ArtifactsFull 'events-logcat.txt') $rawEvents -Encoding utf8
foreach ($marker in @('V06_PICKER_SELECTION_PASS','V06_REAL_APP_SAFE_OPEN_PASS format=Dwg','V06_REAL_APP_SAFE_OPEN_PASS format=Dxf','V06_PICKER_CANCEL_PASS','V06_CLOSE_CLEANUP_PASS files=0')) {
    if ($v06Log -notmatch [regex]::Escape($marker)) { Fail "real app V06 log marker missing: $marker" }
}
$packagePattern = [regex]::Escape($package)
$pidPattern = "(?<!\d)$pidInitial(?!\d)"
$hasCrash = ($rawCrash -match '(?i)FATAL EXCEPTION|Fatal signal|Process .* has died') -and (($rawCrash -match $packagePattern) -or ($rawCrash -match $pidPattern))
if ($hasCrash) { Fail "package/PID scoped crash detected" }
$hasAnr = ($rawEvents -match '(?i)am_anr') -and (($rawEvents -match $packagePattern) -or ($rawEvents -match $pidPattern))
if ($hasAnr) { Fail "post-launch ANR detected" }
$pidFinal = ((& adb -s $Serial shell pidof -s $package 2>$null) | Out-String).Trim()
if ($pidFinal -ne $pidInitial) { Fail "real app process did not remain alive" }
Write-Host "V06_REAL_APP_STABILITY_PASS pid=$pidFinal"

$screenshot = Join-Path $ArtifactsFull 'v06-safe-open-pass.png'
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $screenshot
Assert-PngSignature $screenshot

$headSha = (& git rev-parse HEAD | Out-String).Trim()
@"
Android validation V06
Tested revision: $headSha
Emulator: $Serial / API $api
Package: $package
Launcher: $launcher
PID: $pidFinal
APK SHA-256: $apkHash
DWG source SHA-256: $dwgHashBefore
DXF source SHA-256: $dxfHashBefore
Historical Stage06 headless safe-open semantics: PASS
Real MAUI FilePicker + Android DocumentsUI/SAF + FileResult.OpenReadAsync: PASS
Real app DWG safe-open: PASS
Real app DXF safe-open: PASS
Rapid second selection latest-state: PASS
Picker cancel: PASS
Rotate: PASS
Background/foreground: PASS
Close cleanup/reopen: PASS
Original external CAD immutable: PASS
Broad storage permission: ABSENT
Persistable URI grant: NOT_TAKEN_NOT_NEEDED_IMMEDIATE_PRIVATE_COPY
Physical Android/provider differences: DEFERRED_RELEASE_DEVICE_GATE
Claim limit: REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY
"@ | Set-Content (Join-Path $ArtifactsFull 'summary.txt') -Encoding utf8

Write-Host "ANDROID_VALIDATION_V06_PASS"
Write-Host "CLAIM_LIMIT=REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY"
