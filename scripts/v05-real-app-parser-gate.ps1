param(
    [string]$Configuration = "Release",
    [string]$ArtifactsDir = "artifacts/v05-parser"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "V05_FAIL: $Message" -ForegroundColor Red
    exit 1
}

function Require-ExitCode([string]$Step) {
    if ($LASTEXITCODE -ne 0) { Fail "$Step failed with exit code $LASTEXITCODE" }
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
Write-Host " ANDROID VALIDATION V05 - REAL APP ACadSharp PARSER"
Write-Host "=========================================================="

$dotnetVersion = (& dotnet --version | Out-String).Trim()
if ($dotnetVersion -ne '10.0.400') { Fail "expected .NET SDK 10.0.400, got $dotnetVersion" }
$workloads = (& dotnet workload list 2>&1 | Out-String)
Require-ExitCode "dotnet workload list"
if ($workloads -notmatch '(?m)^maui-android\s') { Fail "maui-android workload missing" }

$adbCommand = Get-Command adb -ErrorAction SilentlyContinue
if (-not $adbCommand) { Fail "adb not found" }
$AdbExe = $adbCommand.Source
$Serial = $null
foreach ($line in (& adb devices)) {
    if ($line.Trim() -match '^(emulator-\d+)\s+device$') {
        $candidate = $Matches[1]
        $boot = ((& adb -s $candidate shell getprop sys.boot_completed 2>$null) | Out-String).Trim()
        if ($boot -eq '1') { $Serial = $candidate; break }
    }
}
if (-not $Serial) { Fail "no booted emulator found after V01 prerequisite" }
$api = ((& adb -s $Serial shell getprop ro.build.version.sdk) | Out-String).Trim()
if ($api -ne '36') { Fail "expected API 36 emulator, got $api" }
Write-Host "V05_EMULATOR_API36_PASS serial=$Serial"

# Re-run the historical AŞAMA 05 host parser/corpus/diagnostics gate.
$gitExe = (Get-Command git -ErrorAction Stop).Source
$gitRoot = Split-Path -Parent (Split-Path -Parent $gitExe)
$bashExe = Join-Path $gitRoot 'bin\bash.exe'
if (-not (Test-Path $bashExe)) { Fail "Git Bash not found at $bashExe" }
$hostCache = 'artifacts/v05-parser/host-cache'
$hostEvidence = 'artifacts/v05-parser/host-parser-evidence.json'
$hostOutput = (& $bashExe 'scripts/stage05-parser-spike.sh' $hostCache $hostEvidence 2>&1 | Out-String)
$hostExit = $LASTEXITCODE
Set-Content -Path (Join-Path $ArtifactsFull 'host-parser-console.txt') -Value $hostOutput -Encoding utf8
Write-Host $hostOutput
if ($hostExit -ne 0) { Fail "historical Stage05 parser gate failed with exit code $hostExit" }
foreach ($marker in @('STAGE05_MINI_CORPUS_PASS','STAGE05_T3_PASS','STAGE04_ARCHITECTURE_TESTS_PASS','STAGE05_DEPENDENCY_BOUNDARY_PASS')) {
    if ($hostOutput -notmatch [regex]::Escape($marker)) { Fail "host parser marker missing: $marker" }
}
Copy-Item "$hostCache/stage03-fixture-audit.json" (Join-Path $ArtifactsFull 'host-stage03-fixture-audit.json') -Force
Copy-Item "$hostCache/stage05-package-graph.txt" (Join-Path $ArtifactsFull 'host-stage05-package-graph.txt') -Force
Remove-Item $hostCache -Recurse -Force
Write-Host "V05_HOST_PARSER_REGRESSION_PASS"

# Generate only the redistributable V03 synthetic DWG input; writer remains outside production src/.
$appInputs = Join-Path $ArtifactsFull 'app-inputs'
New-Item -ItemType Directory -Force -Path $appInputs | Out-Null
$generatedDwg = Join-Path $appInputs 'synthetic_turkish_basic_ac1015.dwg'
& powershell -NoProfile -ExecutionPolicy Bypass -File 'scripts/stage03-generate-synthetic-dwg.ps1' -OutputDwg $generatedDwg | Out-Host
Require-ExitCode "V03 synthetic DWG generation"
if (-not (Test-Path $generatedDwg)) { Fail "generated DWG missing" }
$generatedDwg = (Resolve-Path $generatedDwg).Path
$dwgHash = (Get-FileHash -Algorithm SHA256 $generatedDwg).Hash.ToLowerInvariant()
$dwgBytes = (Get-Item $generatedDwg).Length
Write-Host "V05_GENERATED_DWG_READY bytes=$dwgBytes sha256=$dwgHash"

$writerHits = @(Get-ChildItem 'src' -Recurse -Filter '*.cs' | Select-String -Pattern '\b(DwgWriter|DxfWriter)\b')
if ($writerHits.Count -ne 0) { Fail "production src contains CAD writer symbol" }
Write-Host "V05_PRODUCTION_WRITER_ABSENT_PASS"

# Build a real-app validation variant. Test assets/constants exist only when V05Validation=true.
$appProject = 'src/MobilDwg.App/MobilDwg.App.csproj'
& dotnet build $appProject -f net10.0-android36.0 -c $Configuration -t:Rebuild --nologo /warnaserror "-p:V05Validation=true" "-p:V05GeneratedDwgPath=$generatedDwg" | Out-Host
Require-ExitCode "V05 validation MobilDwg.App build"
$binDir = Join-Path $RepoRoot "src/MobilDwg.App/bin/$Configuration/net10.0-android36.0"
$apk = Get-ChildItem $binDir -Filter '*-Signed.apk' -Recurse | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (-not $apk) { Fail "signed validation APK not found" }
$apkHash = (Get-FileHash -Algorithm SHA256 $apk.FullName).Hash.ToLowerInvariant()
$evidenceApk = Join-Path $ArtifactsFull 'MobilDwg.App-V05-Signed.apk'
Copy-Item $apk.FullName $evidenceApk -Force
Write-Host "V05_REAL_APP_APK_PASS bytes=$($apk.Length) sha256=$apkHash"

$package = 'com.smitelagwar.mobildwg'
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& adb -s $Serial uninstall $package 2>$null | Out-Null
$ErrorActionPreference = $previousEap
& adb -s $Serial install $apk.FullName | Out-Host
Require-ExitCode "V05 validation APK install"
$launcher = ((& adb -s $Serial shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $package 2>$null) | Select-Object -Last 1 | Out-String).Trim()
if (-not $launcher -or $launcher -notmatch [regex]::Escape($package)) { Fail "launcher resolution failed" }
Write-Host "V05_REAL_APP_INSTALL_PASS package=$package launcher=$launcher"

& adb -s $Serial shell am force-stop $package | Out-Null
& adb -s $Serial logcat -c | Out-Null
& adb -s $Serial logcat -b crash -c | Out-Null
$launch = (& adb -s $Serial shell am start -W -n $launcher | Out-String)
Set-Content (Join-Path $ArtifactsFull 'launch.txt') $launch -Encoding utf8
Write-Host $launch
if ($launch -notmatch 'Status:\s+ok') { Fail "real app launch did not report Status: ok" }

$uiRemote = '/sdcard/v05-window.xml'
$uiLocal = Join-Path $ArtifactsFull 'window.xml'
$passSeen = $false
for ($attempt = 0; $attempt -lt 30; $attempt++) {
    Start-Sleep -Seconds 1
    $previousEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & adb -s $Serial shell uiautomator dump $uiRemote 2>$null | Out-Null
    & adb -s $Serial pull $uiRemote $uiLocal 2>$null | Out-Null
    $ErrorActionPreference = $previousEap
    if (Test-Path $uiLocal) {
        $uiText = Get-Content -Raw $uiLocal
        if ($uiText -match 'ANDROID_VALIDATION_V05_FAIL') { Fail "app reported V05 validation failure" }
        if ($uiText -match 'ANDROID_VALIDATION_V05_PASS') { $passSeen = $true; break }
    }
}
if (-not $passSeen) { Fail "V05 pass marker not observed in real app UI" }
Write-Host "V05_REAL_APP_UI_PARSE_PASS"

$appPid = ((& adb -s $Serial shell pidof -s $package 2>$null) | Out-String).Trim()
if ($appPid -notmatch '^\d+$') { Fail "real app PID missing" }
$rawLog = (& adb -s $Serial logcat -d | Out-String)
$v05Lines = @($rawLog -split "`r?`n" | Where-Object { $_ -match 'MobilDwgV05' })
$v05Log = ($v05Lines -join [Environment]::NewLine)
Set-Content (Join-Path $ArtifactsFull 'v05-markers-logcat.txt') $v05Log -Encoding utf8
foreach ($marker in @('V05_DXF_PARSE_PASS','V05_DWG_PARSE_PASS','V05_NEGATIVE_PASS id=missing-font','V05_NEGATIVE_PASS id=missing-xref','V05_INPUT_IMMUTABLE_PASS','V05_REDACTED_DIAGNOSTICS_PASS','ANDROID_VALIDATION_V05_PASS')) {
    if ($v05Log -notmatch [regex]::Escape($marker)) { Fail "real app log marker missing: $marker" }
}

$crash = (& adb -s $Serial logcat -b crash -d | Out-String)
$events = (& adb -s $Serial logcat -b events -d | Out-String)
$packagePattern = [regex]::Escape($package)
$pidPattern = "(?<!\d)$appPid(?!\d)"
$hasCrash = ($crash -match '(?i)FATAL EXCEPTION|Fatal signal|Process .* has died') -and (($crash -match $packagePattern) -or ($crash -match $pidPattern))
if ($hasCrash) { Fail "package/PID scoped crash detected" }
$hasAnr = ($events -match '(?i)am_anr') -and (($events -match $packagePattern) -or ($events -match $pidPattern))
if ($hasAnr) { Fail "post-launch ANR detected" }
$appPidAfter = ((& adb -s $Serial shell pidof -s $package 2>$null) | Out-String).Trim()
if ($appPidAfter -ne $appPid) { Fail "real app process did not remain alive" }
Write-Host "V05_REAL_APP_STABILITY_PASS pid=$appPid"

$screenshot = Join-Path $ArtifactsFull 'v05-parser-pass.png'
Invoke-AdbBinaryToFile -AdbPath $AdbExe -Serial $Serial -OutputPath $screenshot
Assert-PngSignature $screenshot

$headSha = (& git rev-parse HEAD | Out-String).Trim()
@"
Android validation V05
Tested revision: $headSha
Emulator: $Serial / API $api
Package: $package
Launcher: $launcher
PID: $appPid
APK SHA-256: $apkHash
Generated DWG SHA-256: $dwgHash
Host parser regression: PASS
Real app DXF parse: PASS
Real app DWG parse: PASS
Missing font diagnostic: PASS
Missing XREF diagnostic: PASS
Input immutability: PASS
Diagnostic evidence: code/count only; raw parser messages/resources are not emitted by the Android validation runner
Claim limit: REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
"@ | Set-Content (Join-Path $ArtifactsFull 'summary.txt') -Encoding utf8

Write-Host "ANDROID_VALIDATION_V05_PASS"
Write-Host "CLAIM_LIMIT=REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY"
