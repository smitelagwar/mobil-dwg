<#
.SYNOPSIS
    Android validation V02 dependency, lockfile, license, vulnerability, and native-boundary gate.
.DESCRIPTION
    This gate validates the current repository revision without launching an emulator.
    It proves only the dependency policy and Android-oriented probe boundary defined by V02.
#>
param(
    [string]$ArtifactsDir = "artifacts/v02-validation"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Host "V02_FAIL: $Message" -ForegroundColor Red
    exit 1
}

function Require-ExitCode([string]$Step) {
    if ($LASTEXITCODE -ne 0) {
        Fail "$Step failed with exit code $LASTEXITCODE"
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

$ArtifactsFullPath = Join-Path $RepoRoot $ArtifactsDir
if (Test-Path $ArtifactsFullPath) {
    Remove-Item -Recurse -Force $ArtifactsFullPath
}
New-Item -ItemType Directory -Path $ArtifactsFullPath -Force | Out-Null

Write-Host "=========================================================="
Write-Host "  ANDROID VALIDATION V02 - DEPENDENCY GATE"
Write-Host "=========================================================="

$DotnetVersion = (& dotnet --version | Out-String).Trim()
if ($DotnetVersion -ne '10.0.400') {
    Fail "Expected .NET SDK 10.0.400, found '$DotnetVersion'"
}

$Workloads = (& dotnet workload list 2>&1 | Out-String)
Require-ExitCode "dotnet workload list"
if ($Workloads -notmatch '(?m)^maui-android\s') {
    Fail "maui-android workload is not installed"
}
Write-Host "V02_TOOLCHAIN_PASS"

$PythonExe = $null
$PythonPrefixArgs = @()
$PythonCommand = Get-Command python -ErrorAction SilentlyContinue
if ($PythonCommand) {
    $PythonExe = $PythonCommand.Source
}
else {
    $PyCommand = Get-Command py -ErrorAction SilentlyContinue
    if ($PyCommand) {
        $PythonExe = $PyCommand.Source
        $PythonPrefixArgs = @('-3')
    }
}
if (-not $PythonExe) {
    Fail "Python 3 is required to execute scripts/stage02-audit-packages.py"
}

& $PythonExe @PythonPrefixArgs --version | Out-Host
Require-ExitCode "Python version check"
& $PythonExe @PythonPrefixArgs -m py_compile scripts/stage02-audit-packages.py
Require-ExitCode "stage02 audit script compile"

$Probe = 'compliance/Stage02.DependencyProbe/Stage02.DependencyProbe.csproj'
$LockFile = 'compliance/Stage02.DependencyProbe/packages.lock.json'
$Manifest = 'compliance/stage02-package-manifest.json'

& dotnet restore $Probe --locked-mode | Out-Host
Require-ExitCode "locked restore"
& git diff --exit-code -- $LockFile | Out-Host
Require-ExitCode "lockfile reproducibility"
Write-Host "V02_LOCKED_RESTORE_PASS"

$ResolvedPath = Join-Path $ArtifactsFullPath 'resolved-packages.json'
$ResolvedOutput = (& dotnet list $Probe package --include-transitive --format json 2>&1 | Out-String)
Require-ExitCode "resolved dependency graph"
Set-Content -Path $ResolvedPath -Value $ResolvedOutput -Encoding utf8

& $PythonExe @PythonPrefixArgs scripts/stage02-audit-packages.py | Out-Host
Require-ExitCode "package/license/native boundary audit"
& git diff --exit-code -- $Manifest | Out-Host
Require-ExitCode "package manifest reproducibility"
Write-Host "V02_PACKAGE_AUDIT_PASS"
Write-Host "V02_ANDROID_BOUNDARY_PASS"

$VulnerabilityPath = Join-Path $ArtifactsFullPath 'vulnerabilities.txt'
$VulnerabilityOutput = (& dotnet list $Probe package --vulnerable --include-transitive 2>&1 | Out-String)
$VulnerabilityExit = $LASTEXITCODE
Set-Content -Path $VulnerabilityPath -Value $VulnerabilityOutput -Encoding utf8
Write-Host $VulnerabilityOutput
if ($VulnerabilityExit -ne 0) {
    Fail "NuGet vulnerability command failed with exit code $VulnerabilityExit"
}
if ($VulnerabilityOutput -match '(?i)has the following vulnerable packages') {
    Fail "NuGet reported one or more vulnerable packages"
}
Write-Host "V02_VULNERABILITY_PASS"

Copy-Item $LockFile (Join-Path $ArtifactsFullPath 'packages.lock.json') -Force
Copy-Item $Manifest (Join-Path $ArtifactsFullPath 'stage02-package-manifest.json') -Force

$ChecksumsPath = Join-Path $ArtifactsFullPath 'checksums.txt'
$ChecksumTargets = @(
    $LockFile,
    $Manifest,
    $ResolvedPath,
    $VulnerabilityPath
)
$ChecksumLines = foreach ($Path in $ChecksumTargets) {
    $Hash = Get-FileHash -Algorithm SHA256 $Path
    "$($Hash.Hash.ToLowerInvariant())  $Path"
}
Set-Content -Path $ChecksumsPath -Value $ChecksumLines -Encoding ascii
$ChecksumLines | ForEach-Object { Write-Host $_ }

$HeadSha = (& git rev-parse HEAD | Out-String).Trim()
Require-ExitCode "git rev-parse HEAD"
$SummaryPath = Join-Path $ArtifactsFullPath 'summary.txt'
$Summary = @"
ANDROID VALIDATION V02 SUMMARY
Exact SHA: $HeadSha
.NET SDK: $DotnetVersion
Probe: net10.0-android / resolved net10.0-android36.0
Locked restore: PASS
Strict exact package range policy: PASS
Package license/hash reproducibility: PASS
Android native asset boundary: PASS
Production src package/TFM/native boundary: PASS
NuGet vulnerability check: PASS
ProCad/test-only/iOS leakage: NOT DETECTED by V02 policy checks
Emulator: NOT REQUIRED for V02 (no real installable MobilDwg.App exists yet)
Result: ANDROID_VALIDATION_V02_PASS
"@
Set-Content -Path $SummaryPath -Value $Summary -Encoding ascii

Write-Host "=========================================================="
Write-Host "ANDROID_VALIDATION_V02_PASS" -ForegroundColor Green
Write-Host "=========================================================="
exit 0
