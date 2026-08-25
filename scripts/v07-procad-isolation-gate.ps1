<#
.SYNOPSIS
    Android validation V07: ProCad NO-GO, production graph isolation and precision regression.
.DESCRIPTION
    This gate does not build, install or execute the rejected ProCad candidate.
    It validates the committed NO-GO decision/pin, proves ProCad is absent from the
    current production/resolved Android graph and APK, reproduces the rejected
    candidate's deterministic float-collapse boundary, and reruns the production
    double-precision rendering regression.
#>
param(
    [string]$ArtifactsDir = "artifacts/v07-procad-isolation"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "V07_FAIL: $Message" -ForegroundColor Red
    exit 1
}

function Require-ExitCode([string]$Step) {
    if ($LASTEXITCODE -ne 0) {
        Fail "$Step failed with exit code $LASTEXITCODE"
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot
$ArtifactsFull = Join-Path $RepoRoot $ArtifactsDir
if (Test-Path $ArtifactsFull) { Remove-Item -Recurse -Force $ArtifactsFull }
New-Item -ItemType Directory -Path $ArtifactsFull -Force | Out-Null

$ExpectedProCad = 'f8a862b3e7634e27664fee02ff5d68774b102985'
$ExpectedACad = '0ed79df48de0806af3c3028d0e2826447cbc1d36'
$ExpectedProEdit = '64759b79289a024d08463ed1a9094fdcd9a270df'
$ExpectedApprovedACad = 'bbc8b14a92ebfac35bb77c0c1a4af70de90ebb50'

Write-Host "=========================================================="
Write-Host "  ANDROID VALIDATION V07 - PROCAD ISOLATION / PRECISION"
Write-Host "=========================================================="

# 1) Revalidate the committed decision and exact pin without refetching/rebuilding ProCad.
$Pin = Get-Content -Raw 'spikes/ProCad.Android/source-pin.json' | ConvertFrom-Json
if ($Pin.procad.commit -ne $ExpectedProCad) { Fail "Unexpected ProCad pin: $($Pin.procad.commit)" }
if ($Pin.submodules.'external/ACadSharp'.commit -ne $ExpectedACad) { Fail "Unexpected ProCad ACadSharp submodule pin" }
if ($Pin.submodules.'external/ACadSharp'.approved_mobil_dwg_source_commit -ne $ExpectedApprovedACad) { Fail "Unexpected approved ACadSharp source baseline" }
if ($Pin.submodules.'external/ProEdit'.commit -ne $ExpectedProEdit) { Fail "Unexpected ProEdit submodule pin" }

$Adr = Get-Content -Raw 'docs/ADR/0002-procad-pinned-source-no-go.md'
$Stage07Evidence = Get-Content -Raw 'docs/evidence/STAGE_07.md'
foreach ($Required in @(
    '- Durum: Rejected',
    $ExpectedProCad,
    'production renderer/control reuse kararı **NO-GO**',
    '5,000,000.0',
    '0.001'
)) {
    if (-not $Adr.Contains($Required, [StringComparison]::Ordinal)) { Fail "ADR 0002 invariant missing: $Required" }
}
if (-not $Stage07Evidence.Contains('AŞAMA 07 sonucu: **NO-GO**', [StringComparison]::Ordinal)) { Fail "Historical Stage07 NO-GO evidence missing" }
if (-not $Stage07Evidence.Contains($ExpectedProCad, [StringComparison]::Ordinal)) { Fail "Historical Stage07 exact pin missing" }
Write-Host "V07_ADR_PIN_DECISION_PASS"

# 2) Static production graph boundary: no ProCad/ProCadSharp references under src,
# central package versions, or solution membership.
$StaticFiles = @()
$StaticFiles += Get-ChildItem 'src' -Recurse -File | Where-Object { $_.Extension -in @('.cs', '.csproj', '.props', '.targets', '.json', '.xml') }
$StaticFiles += Get-Item 'Directory.Packages.props'
$StaticFiles += Get-Item 'MobilDwg.sln'
foreach ($File in $StaticFiles) {
    $Text = Get-Content -Raw $File.FullName
    if ($Text -match '(?i)\bProCad(?:Sharp)?\b') {
        Fail "ProCad token detected in production graph/source boundary: $($File.FullName)"
    }
}
$NamedLeak = Get-ChildItem 'src' -Recurse -Force | Where-Object { $_.Name -match '(?i)procad' } | Select-Object -First 1
if ($NamedLeak) { Fail "ProCad-named production path detected: $($NamedLeak.FullName)" }
Write-Host "V07_STATIC_PRODUCTION_GRAPH_ISOLATION_PASS"

# 3) Restore current exact graph, then inspect lockfiles and project.assets.json.
& dotnet restore 'MobilDwg.sln' --locked-mode | Out-Host
Require-ExitCode "locked solution restore"

$LockFiles = Get-ChildItem 'src' -Recurse -Filter 'packages.lock.json' -File
foreach ($Lock in $LockFiles) {
    if ((Get-Content -Raw $Lock.FullName) -match '(?i)"ProCad(?:Sharp)?') {
        Fail "ProCad found in production lockfile: $($Lock.FullName)"
    }
}

$Assets = Get-ChildItem 'src' -Recurse -Filter 'project.assets.json' -File
if (-not $Assets) { Fail "No production project.assets.json files were produced" }
$AssetInventory = New-Object System.Collections.Generic.List[string]
foreach ($Asset in $Assets) {
    $Json = Get-Content -Raw $Asset.FullName | ConvertFrom-Json
    foreach ($Library in $Json.libraries.PSObject.Properties.Name) {
        $AssetInventory.Add("$($Asset.FullName): $Library")
        if ($Library -match '^(?i)ProCad(?:Sharp)?[/.]') {
            Fail "ProCad found in resolved production asset graph: $Library"
        }
    }
}
$AssetInventory | Sort-Object | Set-Content (Join-Path $ArtifactsFull 'resolved-production-libraries.txt') -Encoding utf8
Write-Host "V07_RESOLVED_PRODUCTION_GRAPH_ISOLATION_PASS"

# 4) Build the current real Android app and inspect the produced APK entries.
& dotnet build 'src/MobilDwg.App/MobilDwg.App.csproj' -f net10.0-android36.0 -c Release --no-restore -warnaserror | Tee-Object -FilePath (Join-Path $ArtifactsFull 'android-app-build.log')
Require-ExitCode "MobilDwg.App Release build"

$Apk = Get-ChildItem 'src/MobilDwg.App/bin/Release' -Recurse -Filter '*.apk' -File | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (-not $Apk) { Fail "Release APK was not produced" }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$Zip = [System.IO.Compression.ZipFile]::OpenRead($Apk.FullName)
try {
    $Entries = @($Zip.Entries | ForEach-Object { $_.FullName })
} finally {
    $Zip.Dispose()
}
$Entries | Set-Content (Join-Path $ArtifactsFull 'apk-entries.txt') -Encoding utf8
$ProCadApkEntry = $Entries | Where-Object { $_ -match '(?i)procad' } | Select-Object -First 1
if ($ProCadApkEntry) { Fail "ProCad entry found in APK: $ProCadApkEntry" }
$ApkHash = (Get-FileHash -Algorithm SHA256 $Apk.FullName).Hash.ToLowerInvariant()
Write-Host "V07_APK_PROCAD_ABSENT_PASS bytes=$($Apk.Length) sha256=$ApkHash"

# 5) Deterministically reproduce the rejected ProCad absolute-float precision blocker.
$FloatOrigin = [single]5000000.0
$FloatDetailPoint = [single]5000000.001
$FloatObservedDelta = [double]($FloatDetailPoint - $FloatOrigin)
if ($FloatObservedDelta -ne 0.0d) { Fail "Expected rejected ProCad float boundary to collapse, observed delta=$FloatObservedDelta" }
Write-Host "V07_PROCAD_FLOAT_PRECISION_BLOCKER_REPRODUCED observed_delta=$FloatObservedDelta"

# 6) Prove current production double boundary retains the millimetre detail, then
# rerun the rendering executable tests that contain the survey-origin camera/scene checks.
$DoubleObservedDelta = 5000000.001d - 5000000.0d
if ([Math]::Abs($DoubleObservedDelta - 0.001d) -gt 1e-9) { Fail "Production double precision delta unexpected: $DoubleObservedDelta" }
Write-Host "V07_PRODUCTION_DOUBLE_SCALAR_PASS observed_delta=$($DoubleObservedDelta.ToString('R', [Globalization.CultureInfo]::InvariantCulture))"

$RenderingOutput = (& dotnet run --project 'tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj' --configuration Release 2>&1 | Out-String)
$RenderingExit = $LASTEXITCODE
Set-Content (Join-Path $ArtifactsFull 'rendering-regression.log') -Value $RenderingOutput -Encoding utf8
Write-Host $RenderingOutput
if ($RenderingExit -ne 0) { Fail "Rendering precision regression failed with exit code $RenderingExit" }
if ($RenderingOutput -notmatch 'STAGE09_RENDER_SCENE_TESTS_PASS') { Fail "Stage09 rendering marker missing" }
if ($RenderingOutput -notmatch 'STAGE04_RENDER_CONTRACT_TESTS_PASS') { Fail "Stage04 rendering contract marker missing" }
Write-Host "V07_PRODUCTION_DOUBLE_PRECISION_REGRESSION_PASS"

$PackageGraph = (& dotnet list 'src/MobilDwg.App/MobilDwg.App.csproj' package --include-transitive --format json 2>&1 | Out-String)
Require-ExitCode "MobilDwg.App package graph"
Set-Content (Join-Path $ArtifactsFull 'mobil-dwg-app-package-graph.json') -Value $PackageGraph -Encoding utf8
if ($PackageGraph -match '(?i)ProCad(?:Sharp)?') { Fail "ProCad token detected in current app package graph" }
Write-Host "V07_APP_PACKAGE_GRAPH_PROCAD_ABSENT_PASS"

$HeadSha = (& git rev-parse HEAD | Out-String).Trim()
Require-ExitCode "git rev-parse HEAD"
$Summary = @"
ANDROID VALIDATION V07 SUMMARY
Exact checkout SHA: $HeadSha
Rejected ProCad pin: $ExpectedProCad
ADR 0002 decision: REJECTED / NO-GO
Production static graph ProCad: ABSENT
Production resolved assets/lockfiles ProCad: ABSENT
Release APK ProCad entries: ABSENT
Rejected candidate absolute-float survey delta: $FloatObservedDelta
Production double survey delta: $($DoubleObservedDelta.ToString('R', [Globalization.CultureInfo]::InvariantCulture)
)
Rendering survey-origin regression: PASS
Physical Android / rejected ProCad emulator install: NOT REQUIRED / NOT RUN BY V07
Result: ANDROID_VALIDATION_V07_PASS
Claim limit: PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY
"@
Set-Content (Join-Path $ArtifactsFull 'summary.txt') -Value $Summary -Encoding ascii

Write-Host "ANDROID_VALIDATION_V07_PASS" -ForegroundColor Green
Write-Host "CLAIM_LIMIT=PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY"
exit 0
