param(
    [string]$Stage = "01",
    [string]$ArtifactsDir = "artifacts/viewer-stability"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Fail([string]$Message) {
    Write-Error $Message
    exit 1
}

function Pass([string]$Marker) {
    Write-Host $Marker
    if ($script:SummaryPath) {
        Add-Content -LiteralPath $script:SummaryPath -Value $Marker
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $repoRoot

$stageNum = [int]$Stage
$stageArtifacts = Join-Path $repoRoot (Join-Path $ArtifactsDir ("stage" + $Stage.PadLeft(2, '0')))
New-Item -ItemType Directory -Path $stageArtifacts -Force | Out-Null
$script:SummaryPath = Join-Path $stageArtifacts "gate-summary.txt"
Set-Content -LiteralPath $script:SummaryPath -Value "VIEWER_STABILITY_GATE_EVIDENCE stage=$Stage"

Write-Host "=== Running Viewer Stability Gate for Stage $Stage ==="

# 1. Environment & Revision Check
$head = (& git rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($head)) { Fail "Unable to resolve git HEAD." }
Add-Content -LiteralPath $script:SummaryPath -Value ("GIT_HEAD=" + $head)

$manifestPath = Join-Path $stageArtifacts "source-baseline-manifest.json"
if (-not (Test-Path $manifestPath)) {
    # Check if in stage01
    $stage01Manifest = Join-Path $repoRoot "artifacts/viewer-stability/stage01/source-baseline-manifest.json"
    if (Test-Path $stage01Manifest) {
        $manifestPath = $stage01Manifest
    }
}
if (Test-Path $manifestPath) {
    Pass "STAGE01_SOURCE_BASELINE_MANIFEST_PRESENT"
} else {
    Fail "Source baseline manifest not found at $manifestPath"
}

# 2. Run Exact Test Projects
Write-Host "Running Architecture Tests..."
$archLog = Join-Path $stageArtifacts "architecture-tests.log"
& dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release 2>&1 | Tee-Object -FilePath $archLog
if ($LASTEXITCODE -ne 0) { Fail "Architecture tests failed with exit code $LASTEXITCODE" }
$archText = Get-Content -LiteralPath $archLog -Raw
if (-not ($archText -match "STAGE04_ARCHITECTURE_TESTS_PASS")) { Fail "Architecture pass marker missing" }
Pass "STAGE01_ARCHITECTURE_TESTS_PASS"

Write-Host "Running Core Tests..."
$coreLog = Join-Path $stageArtifacts "core-tests.log"
& dotnet run --project tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj -c Release 2>&1 | Tee-Object -FilePath $coreLog
if ($LASTEXITCODE -ne 0) { Fail "Core tests failed with exit code $LASTEXITCODE" }
$coreText = Get-Content -LiteralPath $coreLog -Raw
if (-not ($coreText -match "STAGE04_CORE_CONTRACT_TESTS_PASS")) { Fail "Core contract pass marker missing" }
Pass "STAGE01_CORE_TESTS_PASS"

Write-Host "Running Rendering Tests..."
$rendLog = Join-Path $stageArtifacts "rendering-tests.log"
& dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release 2>&1 | Tee-Object -FilePath $rendLog
if ($LASTEXITCODE -ne 0) { Fail "Rendering tests failed with exit code $LASTEXITCODE" }
$rendText = Get-Content -LiteralPath $rendLog -Raw
if (-not ($rendText -match "STAGE04_RENDER_CONTRACT_TESTS_PASS")) { Fail "Render contract pass marker missing" }
Pass "STAGE01_RENDERING_TESTS_PASS"

Write-Host "Running Integration Tests..."
$integLog = Join-Path $stageArtifacts "integration-tests.log"
& dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release 2>&1 | Tee-Object -FilePath $integLog
if ($LASTEXITCODE -ne 0) { Fail "Integration tests failed with exit code $LASTEXITCODE" }
$integText = Get-Content -LiteralPath $integLog -Raw
if (-not ($integText -match "STAGE01_INTEGRATION_TESTS_PASS")) { Fail "Integration pass marker missing" }
Pass "STAGE01_INTEGRATION_TESTS_PASS"

# 3. Telemetry Verification (Stage 01+)
$telemetryFile = Join-Path $repoRoot "src/MobilDwg.Rendering/Performance/ViewportTelemetry.cs"
if (-not (Test-Path $telemetryFile)) { Fail "ViewportTelemetry.cs missing" }
Pass "STAGE01_VIEWPORT_TELEMETRY_SOURCE_PASS"

if ($stageNum -ge 1) {
    Pass "VIEWER_STABILITY_STAGE01_PASS"
}

# Stage 02 checks
if ($stageNum -ge 2) {
    Write-Host "Running Stage 02 Package Audit..."
    $pkgAuditLog = Join-Path $stageArtifacts "stage02-package-audit.log"
    & python scripts/stage02-audit-packages.py 2>&1 | Tee-Object -FilePath $pkgAuditLog
    if ($LASTEXITCODE -ne 0) { Fail "Package audit failed with exit code $LASTEXITCODE" }
    $pkgAuditText = Get-Content -LiteralPath $pkgAuditLog -Raw
    if (-not ($pkgAuditText -match "STAGE02_PACKAGE_AUDIT_PASS")) { Fail "Package audit pass marker missing" }
    Pass "STAGE02_PACKAGE_AUDIT_PASS"

    Write-Host "Running Locked Restore on App..."
    & dotnet restore --locked-mode src/MobilDwg.App/MobilDwg.App.csproj
    if ($LASTEXITCODE -ne 0) { Fail "Locked restore failed with exit code $LASTEXITCODE" }
    Pass "STAGE02_LOCKED_RESTORE_PASS"

    Write-Host "Building App for Android Release..."
    $appBuildLog = Join-Path $stageArtifacts "app-build-android.log"
    & dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release 2>&1 | Tee-Object -FilePath $appBuildLog
    if ($LASTEXITCODE -ne 0) { Fail "Android release build failed with exit code $LASTEXITCODE" }
    Pass "STAGE02_APP_ANDROID_BUILD_PASS"

    Pass "VIEWER_STABILITY_STAGE02_PASS"
}

# Stage 03 checks
if ($stageNum -ge 3) {
    Write-Host "Verifying Stage 03 Camera & Numerical Contracts..."
    if (-not ($rendText -match "STAGE03_VIEWPORT_CAMERA_TESTS_PASS")) { Fail "STAGE03_VIEWPORT_CAMERA_TESTS_PASS marker missing" }
    Pass "STAGE03_VIEWPORT_CAMERA_TESTS_PASS"

    $zoomPolicyFile = Join-Path $repoRoot "src/MobilDwg.Rendering/Camera/ViewerZoomPolicy.cs"
    if (-not (Test-Path $zoomPolicyFile)) { Fail "ViewerZoomPolicy.cs missing" }
    Pass "STAGE03_VIEWER_ZOOM_POLICY_SOURCE_PASS"

    $inputContractsFile = Join-Path $repoRoot "src/MobilDwg.Rendering/Interaction/ViewportInputContracts.cs"
    if (-not (Test-Path $inputContractsFile)) { Fail "ViewportInputContracts.cs missing" }
    Pass "STAGE03_VIEWPORT_INPUT_CONTRACTS_SOURCE_PASS"

    Pass "VIEWER_STABILITY_STAGE03_PASS"
}

# Stage 04 checks
if ($stageNum -ge 4) {
    Write-Host "Verifying Stage 04 Native Input & Gesture State Machine..."
    if (-not ($rendText -match "STAGE04_VIEWPORT_INTERACTION_TESTS_PASS")) { Fail "STAGE04_VIEWPORT_INTERACTION_TESTS_PASS marker missing" }
    Pass "STAGE04_VIEWPORT_INTERACTION_TESTS_PASS"

    $interactionEngineFile = Join-Path $repoRoot "src/MobilDwg.Rendering/Interaction/ViewportInteractionEngine.cs"
    if (-not (Test-Path $interactionEngineFile)) { Fail "ViewportInteractionEngine.cs missing" }
    Pass "STAGE04_VIEWPORT_INTERACTION_ENGINE_SOURCE_PASS"

    $androidAdapterFile = Join-Path $repoRoot "src/MobilDwg.App/Viewer/Platforms/Android/AndroidViewportInputAdapter.cs"
    if (-not (Test-Path $androidAdapterFile)) { Fail "AndroidViewportInputAdapter.cs missing" }
    Pass "STAGE04_ANDROID_INPUT_ADAPTER_SOURCE_PASS"

    Pass "VIEWER_STABILITY_STAGE04_PASS"
}

# Stage 05 checks
if ($stageNum -ge 5) {
    Write-Host "Verifying Stage 05 Session, Scheduling and Swap-Chain Coordination..."
    
    $frameGateFile = Join-Path $repoRoot "src/MobilDwg.Rendering/Scheduling/FrameRequestGate.cs"
    if (-not (Test-Path $frameGateFile)) { Fail "FrameRequestGate.cs missing" }
    Pass "STAGE05_FRAME_REQUEST_GATE_SOURCE_PASS"

    $renderLeaseFile = Join-Path $repoRoot "src/MobilDwg.Rendering/Viewer/RenderSessionLease.cs"
    if (-not (Test-Path $renderLeaseFile)) { Fail "RenderSessionLease.cs missing" }
    Pass "STAGE05_RENDER_SESSION_LEASE_SOURCE_PASS"

    $cadViewportFile = Join-Path $repoRoot "src/MobilDwg.App/Viewer/CadViewportView.cs"
    if (-not (Test-Path $cadViewportFile)) { Fail "CadViewportView.cs missing" }
    Pass "STAGE05_CAD_VIEWPORT_VIEW_SOURCE_PASS"

    $androidClockFile = Join-Path $repoRoot "src/MobilDwg.App/Viewer/Platforms/Android/AndroidFrameClock.cs"
    if (-not (Test-Path $androidClockFile)) { Fail "AndroidFrameClock.cs missing" }
    Pass "STAGE05_ANDROID_FRAME_CLOCK_SOURCE_PASS"

    $instrumentationProj = Join-Path $repoRoot "tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj"
    if (-not (Test-Path $instrumentationProj)) { Fail "MobilDwg.Android.Instrumentation.csproj missing" }
    Pass "STAGE05_ANDROID_INSTRUMENTATION_PROJECT_PASS"

    if (-not ($archText -match "STAGE05_DEPENDENCY_BOUNDARY_PASS")) { Fail "STAGE05_DEPENDENCY_BOUNDARY_PASS missing" }
    Pass "STAGE05_DEPENDENCY_BOUNDARY_PASS"

    Pass "VIEWER_STABILITY_STAGE05_PASS"
}

Write-Host "=== Viewer Stability Gate Passed for Stage $Stage ==="
exit 0


