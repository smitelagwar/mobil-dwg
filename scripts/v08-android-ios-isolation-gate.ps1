param(
    [string]$ArtifactsDir = "artifacts/v08-android-ios-isolation"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Fail([string]$Message) {
    Write-Error $Message
    exit 1
}

function Pass([string]$Marker) {
    Write-Host $Marker
    Add-Content -LiteralPath $script:SummaryPath -Value $Marker
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $repoRoot

$artifactPath = Join-Path $repoRoot $ArtifactsDir
New-Item -ItemType Directory -Path $artifactPath -Force | Out-Null
$script:SummaryPath = Join-Path $artifactPath "v08-summary.txt"
Set-Content -LiteralPath $script:SummaryPath -Value "ANDROID_VALIDATION_V08_EVIDENCE"

$head = (& git rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($head)) { Fail "Unable to resolve tested revision." }
Add-Content -LiteralPath $script:SummaryPath -Value ("TESTED_REVISION=" + $head)

if ($env:OS -ne "Windows_NT") { Fail "V08 Android graph-isolation gate must run on the Windows Android validation runner." }
Pass "V08_WINDOWS_ANDROID_HOST_PASS"

$appProject = Join-Path $repoRoot "src/MobilDwg.App/MobilDwg.App.csproj"
$appText = Get-Content -LiteralPath $appProject -Raw
if ($appText -notmatch '<TargetFramework>net10\.0-android36\.0</TargetFramework>') { Fail "MobilDwg.App is not pinned to net10.0-android36.0." }
if ($appText -match '(?i)net10\.0-ios|ios-arm64|iossimulator|SkiaSharp\.NativeAssets\.iOS|Microsoft\.iOS') { Fail "MobilDwg.App contains an iOS production target/dependency token." }
Pass "V08_ANDROID_APP_TFM_ISOLATION_PASS target=net10.0-android36.0"

$productionProjectFiles = Get-ChildItem -LiteralPath (Join-Path $repoRoot "src") -Recurse -File -Include *.csproj,packages.lock.json
$forbiddenProduction = '(?i)net10\.0-ios|ios-arm64|iossimulator|SkiaSharp\.NativeAssets\.iOS|Microsoft\.iOS|<RuntimeIdentifier>ios|<RuntimeIdentifiers>[^<]*ios'
foreach ($file in $productionProjectFiles) {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    if ($text -match $forbiddenProduction) { Fail ("iOS token leaked into production project/lockfile: " + $file.FullName) }
}
Pass "V08_STATIC_PRODUCTION_IOS_ISOLATION_PASS"

$solutionText = Get-Content -LiteralPath (Join-Path $repoRoot "MobilDwg.sln") -Raw
if ($solutionText -match '(?i)Stage08\.iOS|spikes\\Stage08|net10\.0-ios') { Fail "Historical iOS spike is included in the production solution." }
Pass "V08_SOLUTION_IOS_SPIKE_EXCLUDED_PASS"

$centralText = Get-Content -LiteralPath (Join-Path $repoRoot "Directory.Packages.props") -Raw
if ($centralText -match '(?i)SkiaSharp\.NativeAssets\.iOS|Microsoft\.iOS') { Fail "iOS native/package dependency leaked into central production package declarations." }
Pass "V08_CENTRAL_PACKAGE_IOS_ABSENT_PASS"

$historicalWorkflowPath = Join-Path $repoRoot ".github/workflows/stage08-ios-feasibility.yml"
$historicalWorkflow = Get-Content -LiteralPath $historicalWorkflowPath -Raw
if ($historicalWorkflow -notmatch '(?m)^\s*workflow_dispatch:\s*$') { Fail "Historical Stage08 iOS workflow is not manually dispatchable." }
if ($historicalWorkflow -match '(?m)^\s*(push|pull_request|schedule):\s*$') { Fail "Historical Stage08 iOS workflow has an automatic trigger." }
Pass "V08_HISTORICAL_IOS_WORKFLOW_ARCHIVED_MANUAL_ONLY_PASS"

$activeWorkflowForbidden = '(?i)runs-on:\s*macos|dotnet\s+workload\s+install\s+ios|net10\.0-ios|SkiaSharp\.NativeAssets\.iOS|stage08-ios-feasibility\.sh'
$workflowFiles = Get-ChildItem -LiteralPath (Join-Path $repoRoot ".github/workflows") -File -Include *.yml,*.yaml
foreach ($workflow in $workflowFiles) {
    if ($workflow.Name -eq "stage08-ios-feasibility.yml") { continue }
    $text = Get-Content -LiteralPath $workflow.FullName -Raw
    if ($text -match $activeWorkflowForbidden) { Fail ("Active/non-historical workflow requires iOS/macOS tooling: " + $workflow.Name) }
}
Pass "V08_ACTIVE_CI_IOS_TOOLCHAIN_ABSENT_PASS"

& dotnet --info 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "dotnet-info.txt")
if ($LASTEXITCODE -ne 0) { Fail "dotnet --info failed." }
& dotnet workload list 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "workloads.txt")
if ($LASTEXITCODE -ne 0) { Fail "dotnet workload list failed." }

& dotnet restore $appProject --locked-mode 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "restore.log")
if ($LASTEXITCODE -ne 0) { Fail "Locked Android app restore failed." }
Pass "V08_ANDROID_LOCKED_RESTORE_PASS"

$assetsPath = Join-Path $repoRoot "src/MobilDwg.App/obj/project.assets.json"
if (-not (Test-Path -LiteralPath $assetsPath)) { Fail "MobilDwg.App project.assets.json was not produced." }
$assetsText = Get-Content -LiteralPath $assetsPath -Raw
Set-Content -LiteralPath (Join-Path $artifactPath "project-assets-scan.txt") -Value $assetsText
if ($assetsText -match '(?i)SkiaSharp\.NativeAssets\.iOS|Microsoft\.iOS|net10\.0-ios|ios-arm64|iossimulator') { Fail "Resolved Android project.assets.json contains iOS-specific dependency/target data." }
Pass "V08_RESOLVED_ANDROID_GRAPH_IOS_ABSENT_PASS"

& dotnet build $appProject -c Release -f net10.0-android36.0 --no-restore -warnaserror 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "android-release-build.log")
if ($LASTEXITCODE -ne 0) { Fail "Android Release build failed." }
Pass "V08_ANDROID_RELEASE_BUILD_WITHOUT_IOS_TOOLCHAIN_PASS"

$apk = Get-ChildItem -LiteralPath (Join-Path $repoRoot "src/MobilDwg.App/bin/Release") -Recurse -File -Filter *.apk | Sort-Object Length -Descending | Select-Object -First 1
if ($null -eq $apk) { Fail "Android Release APK not found." }
$apkHash = (Get-FileHash -LiteralPath $apk.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
Add-Content -LiteralPath $script:SummaryPath -Value ("APK_BYTES=" + $apk.Length)
Add-Content -LiteralPath $script:SummaryPath -Value ("APK_SHA256=" + $apkHash)

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($apk.FullName)
try {
    $entryNames = @($zip.Entries | ForEach-Object { $_.FullName })
    $entryNames | Set-Content -LiteralPath (Join-Path $artifactPath "apk-entries.txt")
    $badEntries = @($entryNames | Where-Object { $_ -match '(?i)SkiaSharp\.NativeAssets\.iOS|Microsoft\.iOS|ios-arm64|iossimulator|\.framework/' })
    if ($badEntries.Count -gt 0) { Fail ("Android APK contains iOS-specific entries: " + ($badEntries -join ", ")) }
}
finally {
    $zip.Dispose()
}
Pass ("V08_ANDROID_APK_IOS_NATIVE_ABSENT_PASS bytes=" + $apk.Length + " sha256=" + $apkHash)

Pass "V08_XCODE_NOT_REQUIRED_PASS host=windows"
Pass "ANDROID_VALIDATION_V08_PASS"
Pass "CLAIM_LIMIT=ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED"
