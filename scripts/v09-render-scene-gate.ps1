param(
    [string]$ArtifactsDir = "artifacts/v09-render-scene"
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

function Require-Line([string]$Path, [string]$Line, [string]$FailureMessage) {
    $lines = @(Get-Content -LiteralPath $Path)
    if (-not ($lines -contains $Line)) {
        Fail $FailureMessage
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $repoRoot

$artifactPath = Join-Path $repoRoot $ArtifactsDir
New-Item -ItemType Directory -Path $artifactPath -Force | Out-Null
$script:SummaryPath = Join-Path $artifactPath "v09-summary.txt"
Set-Content -LiteralPath $script:SummaryPath -Value "ANDROID_VALIDATION_V09_EVIDENCE"

$head = (& git rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($head)) { Fail "Unable to resolve tested revision." }
Add-Content -LiteralPath $script:SummaryPath -Value ("TESTED_REVISION=" + $head)

if ($env:OS -ne "Windows_NT") { Fail "V09 must run on the configured Windows validation runner." }
Pass "V09_WINDOWS_VALIDATION_HOST_PASS"

$dotnetVersion = (& dotnet --version).Trim()
if ($LASTEXITCODE -ne 0 -or $dotnetVersion -ne "10.0.400") { Fail ("Expected .NET SDK 10.0.400, got " + $dotnetVersion) }
& dotnet --info 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "dotnet-info.txt")
if ($LASTEXITCODE -ne 0) { Fail "dotnet --info failed." }
Pass "V09_DOTNET_PIN_PASS version=10.0.400"

$renderTestSource = Join-Path $repoRoot "tests/MobilDwg.Rendering.Tests/Program.cs"
$renderTestText = Get-Content -LiteralPath $renderTestSource -Raw
$requiredContractTokens = @(
    "survey-origin 1 mm detail must survive camera transform",
    "screen/world roundtrip must retain double precision",
    "overflowing world-to-view delta must fail instead of propagating infinity",
    "very large finite extrusion normal must normalize without overflow",
    "same semantic input must produce identical snapshot regardless of insertion order",
    "unsupported taxonomy",
    "substituted taxonomy",
    "dropped taxonomy",
    "error taxonomy",
    "duplicate stable ID must fail"
)
foreach ($token in $requiredContractTokens) {
    if ($renderTestText.IndexOf($token, [StringComparison]::Ordinal) -lt 0) {
        Fail ("Required V09 executable test contract is missing: " + $token)
    }
}
Pass "V09_REQUIRED_TEST_CONTRACT_PRESENT_PASS"

$renderProject = Join-Path $repoRoot "tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj"
& dotnet restore $renderProject 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "render-restore.log")
if ($LASTEXITCODE -ne 0) { Fail "Rendering test restore failed." }
& dotnet build $renderProject -c Release --no-restore -warnaserror 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "render-build.log")
if ($LASTEXITCODE -ne 0) { Fail "Rendering test Release build failed." }
Pass "V09_RENDER_FOUNDATION_BUILD_PASS"

$renderLog = Join-Path $artifactPath "render-scene.log"
& dotnet run --project $renderProject -c Release --no-build 2>&1 | Tee-Object -FilePath $renderLog
if ($LASTEXITCODE -ne 0) { Fail "RenderScene/camera executable tests failed." }
Require-Line $renderLog "STAGE04_RENDER_CONTRACT_TESTS_PASS" "Stage04 rendering contract regression marker missing."
Require-Line $renderLog "STAGE09_RENDER_SCENE_TESTS_PASS" "Stage09 render-scene marker missing."
Require-Line $renderLog "render-scene/v1" "Deterministic semantic snapshot version marker missing."
Require-Line $renderLog "entity=E-001|0|BYLAYER|5000000,-25,5000000.001,100|SYNTHETIC|A1|1" "Survey-origin semantic snapshot precision line missing."
Require-Line $renderLog "diagnostic=Substituted|STYLE_FALLBACK|E-001|Style substituted deterministically." "Substituted diagnostic snapshot line missing."
Require-Line $renderLog "diagnostic=Unsupported|UNSUPPORTED_PROXY|E-002|Proxy entity retained as compatibility evidence." "Unsupported diagnostic snapshot line missing."
Pass "V09_RENDER_SCENE_CAMERA_OCS_DIAGNOSTICS_PASS"
Pass "V09_SEMANTIC_SNAPSHOT_DETERMINISM_PASS"
Pass "V09_SURVEY_ORIGIN_DOUBLE_PRECISION_PASS delta=0.001"

$coreProject = Join-Path $repoRoot "tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj"
& dotnet restore $coreProject 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "core-restore.log")
if ($LASTEXITCODE -ne 0) { Fail "Core test restore failed." }
& dotnet build $coreProject -c Release --no-restore -warnaserror 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "core-build.log")
if ($LASTEXITCODE -ne 0) { Fail "Core test Release build failed." }
$coreLog = Join-Path $artifactPath "core-tests.log"
& dotnet run --project $coreProject -c Release --no-build 2>&1 | Tee-Object -FilePath $coreLog
if ($LASTEXITCODE -ne 0) { Fail "Core executable regression failed." }
Require-Line $coreLog "STAGE04_CORE_CONTRACT_TESTS_PASS" "Core contract regression marker missing."
Pass "V09_CORE_RENDER_CONTRACT_REGRESSION_PASS"

$architectureProject = Join-Path $repoRoot "tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj"
& dotnet restore $architectureProject 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "architecture-restore.log")
if ($LASTEXITCODE -ne 0) { Fail "Architecture test restore failed." }
& dotnet build $architectureProject -c Release --no-restore -warnaserror 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "architecture-build.log")
if ($LASTEXITCODE -ne 0) { Fail "Architecture test Release build failed." }
$architectureLog = Join-Path $artifactPath "architecture-tests.log"
& dotnet run --project $architectureProject -c Release --no-build 2>&1 | Tee-Object -FilePath $architectureLog
if ($LASTEXITCODE -ne 0) { Fail "Architecture executable regression failed." }
Require-Line $architectureLog "STAGE04_ARCHITECTURE_TESTS_PASS" "Architecture regression marker missing."
Require-Line $architectureLog "STAGE05_DEPENDENCY_BOUNDARY_PASS" "Dependency-boundary architecture marker missing."
Require-Line $architectureLog "V04_REAL_ANDROID_APP_PROJECT_PASS" "Real Android app project composition marker missing."
Pass "V09_CORE_CAD_RENDERING_APP_COMPOSITION_BOUNDARY_PASS"

$solutionPath = Join-Path $repoRoot "MobilDwg.sln"
& dotnet restore $solutionPath 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "solution-restore.log")
if ($LASTEXITCODE -ne 0) { Fail "Full solution restore failed." }
& dotnet build $solutionPath -c Release --no-restore -warnaserror 2>&1 | Tee-Object -FilePath (Join-Path $artifactPath "solution-release-build.log")
if ($LASTEXITCODE -ne 0) { Fail "Full solution Release build failed." }
Pass "V09_FULL_SOLUTION_RELEASE_BUILD_PASS"

$appProject = Join-Path $repoRoot "src/MobilDwg.App/MobilDwg.App.csproj"
$appText = Get-Content -LiteralPath $appProject -Raw
if ($appText -notmatch '<TargetFramework>net10\.0-android36\.0</TargetFramework>') { Fail "MobilDwg.App Android target changed." }

$apk = Get-ChildItem -LiteralPath (Join-Path $repoRoot "src/MobilDwg.App/bin/Release") -Recurse -File -Filter *.apk | Sort-Object Length -Descending | Select-Object -First 1
if ($null -eq $apk) { Fail "Real MobilDwg.App Release APK was not produced by the exact V09 revision." }
$apkHash = (Get-FileHash -LiteralPath $apk.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
Add-Content -LiteralPath $script:SummaryPath -Value ("APK_BYTES=" + $apk.Length)
Add-Content -LiteralPath $script:SummaryPath -Value ("APK_SHA256=" + $apkHash)
Pass ("V09_REAL_ANDROID_APP_COMPOSITION_BUILD_PASS bytes=" + $apk.Length + " sha256=" + $apkHash)

Pass "ANDROID_VALIDATION_V09_PASS"
Pass "CLAIM_LIMIT=RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY"
