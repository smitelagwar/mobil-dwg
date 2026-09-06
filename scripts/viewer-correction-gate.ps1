param(
    [string]$ArtifactsDir = "artifacts/viewer-correction",
    [string]$RunId = (Get-Date -Format "yyyyMMdd-HHmmss"),
    [switch]$RequireNative,
    [switch]$SkipNative,
    [switch]$AllowRegressions,
    [switch]$ReinstallApk
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $repoRoot

$outDir = Join-Path $repoRoot (Join-Path $ArtifactsDir $RunId)
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

$summaryLog = Join-Path $outDir "gate-run.log"
$summaryJson = Join-Path $outDir "gate-summary.json"

function Log([string]$message) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$timestamp] $message"
    Write-Host $line
    Add-Content -LiteralPath $summaryLog -Value $line
}

Log "=== MobilDwg Viewer Correction Gate ==="
Log "RunId: $RunId"
Log "Output: $outDir"

# 1. Environment & Revision Check
$head = (& git rev-parse HEAD).Trim()
Log "Git HEAD: $head"

$gitStatus = (& git status --porcelain)
Log "Git Status: $(if ($gitStatus) { ($gitStatus -join ', ') } else { 'clean' })"

$suiteResults = @{}
$allPassed = $true

function Run-DotnetSuite([string]$name, [string]$projectPath, [string[]]$extraArgs = @()) {
    Log "--- Running $name ---"
    $logFile = Join-Path $outDir "$name.log"
    $cmdArgs = @("run", "--project", $projectPath, "-c", "Release") + $extraArgs
    
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & dotnet @cmdArgs 2>&1 | Out-File -LiteralPath $logFile -Encoding utf8
        $exitCode = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $prevEap
    }

    $logContent = if (Test-Path $logFile) { Get-Content -LiteralPath $logFile -Raw } else { "" }
    
    $failedRegressions = @()
    if ($logContent) {
        $matches = [regex]::Matches($logContent, '\[FAIL\]\s+(P\d+):\s+([^\r\n]+)')
        foreach ($m in $matches) {
            $failedRegressions += [PSCustomObject]@{
                Id = $m.Groups[1].Value
                Detail = $m.Groups[2].Value
            }
        }
    }

    $passed = ($exitCode -eq 0)
    Log "$name finished with exit code $exitCode. Passed=$passed. Failed regressions detected: $($failedRegressions.Count)"

    $script:suiteResults[$name] = [PSCustomObject]@{
        Name = $name
        ExitCode = $exitCode
        Passed = $passed
        FailedRegressions = $failedRegressions
        LogFile = $logFile
    }

    if (-not $passed) {
        $script:allPassed = $false
    }
}

# 2. Run Architecture Tests
Run-DotnetSuite -name "ArchitectureTests" -projectPath "tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj"

# 3. Run Core Tests
Run-DotnetSuite -name "CoreTests" -projectPath "tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj"

# 4. Run Rendering Tests with regressions enabled
Run-DotnetSuite -name "RenderingTests" -projectPath "tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj" -extraArgs @("--", "--regressions")

# 5. Run Integration Tests with regressions enabled
Run-DotnetSuite -name "IntegrationTests" -projectPath "tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj" -extraArgs @("--", "--regressions")

# 6. Android Native Instrumentation Tests
$nativeExecuted = $false
$nativePassed = $false
$nativeDetails = @()

if (-not $SkipNative) {
    Log "--- Checking Android Device / Emulator for Native Instrumentation ---"
    & adb start-server | Out-Null
    Start-Sleep -Seconds 2
    $deviceLines = @()
    for ($retry = 0; $retry -lt 5; $retry++) {
        $devicesOut = & adb devices
        $deviceLines = @($devicesOut | Where-Object { $_ -match '\tdevice$' })
        if ($deviceLines.Length -gt 0) { break }
        Start-Sleep -Seconds 1
    }

    if ($deviceLines.Length -eq 0) {
        if ($RequireNative) {
            Log "ERROR: RequireNative was specified but no running Android device/emulator was found."
            $allPassed = $false
        } else {
            Log "WARNING: No running Android emulator found. Skipping native instrumentation."
            Log "Note: Native PASS marker will NOT be emitted without actual native test execution."
        }
    } else {
        $deviceSerial = ($deviceLines[0] -split '\s+')[0]
        Log "Using Android Device: $deviceSerial"

        $testApkPath = Join-Path $repoRoot "tests/MobilDwg.Android.Instrumentation/bin/Release/net10.0-android36.0/com.smitelagwar.mobildwg.test-Signed.apk"
        if (-not (Test-Path $testApkPath)) {
            Log "Building Android Test APK in Release mode..."
            & dotnet build tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj -c Release
        }

        if (Test-Path $testApkPath) {
            $prevEap = $ErrorActionPreference
            $ErrorActionPreference = "Continue"
            try {
                Log "Stopping any running test or app processes..."
                & adb -s $deviceSerial shell am force-stop com.smitelagwar.mobildwg.test | Out-Null
                & adb -s $deviceSerial shell am force-stop com.smitelagwar.mobildwg | Out-Null

                $isInstalled = (& adb -s $deviceSerial shell pm list packages com.smitelagwar.mobildwg.test) -match "com.smitelagwar.mobildwg.test"
                if (-not $isInstalled -or $ReinstallApk) {
                    Log "Installing test APK: $testApkPath"
                    & adb -s $deviceSerial install -r -d --no-incremental $testApkPath | Out-Host
                } else {
                    Log "Test APK already installed on $deviceSerial. Skipping re-install."
                }

                # Push sample DXF fixture to device if not present
                $sampleDxf = Join-Path $repoRoot "fixtures/public/synthetic/synthetic_turkish_basic_ac1015.dxf"
                if (Test-Path $sampleDxf) {
                    & adb -s $deviceSerial push $sampleDxf /sdcard/synthetic_turkish_basic_ac1015.dxf | Out-Host
                }

                Log "Executing am instrument MobilDwgTestRunner..."
                $instLog = Join-Path $outDir "android-instrumentation.log"
                & adb -s $deviceSerial shell am instrument -w com.smitelagwar.mobildwg.test/com.smitelagwar.mobildwg.test.MobilDwgTestRunner 2>&1 | Out-File -LiteralPath $instLog -Encoding utf8

                $rawInstText = if (Test-Path $instLog) { Get-Content -LiteralPath $instLog -Raw } else { "" }
                $instText = if ([string]::IsNullOrEmpty($rawInstText)) { "" } else { $rawInstText }
                $nativeExecuted = $true

                # Parse results from instrumentation output
                $failedTestsMatch = [regex]::Match($instText, 'INSTRUMENTATION_RESULT:\s+failed_tests=(\d+)')
                $failedTestsCount = if ($failedTestsMatch.Success) { [int]$failedTestsMatch.Groups[1].Value } else { -1 }

                $codeMatch = [regex]::Match($instText, 'INSTRUMENTATION_CODE:\s+(-?\d+)')
                $instCode = if ($codeMatch.Success) { [int]$codeMatch.Groups[1].Value } else { 0 }

                Log "Android Instrumentation: Code=$instCode, FailedTestsCount=$failedTestsCount"

                # Pull artifacts from app files directory
                $deviceArtifactsDir = "/sdcard/Android/data/com.smitelagwar.mobildwg.test/files"
                & adb -s $deviceSerial pull $deviceArtifactsDir $outDir | Out-Host

                # Check pulled JSON if available
                $pulledJson = Join-Path $outDir "files/mobildwg_native_test_result.json"
                if (-not (Test-Path $pulledJson)) {
                    $pulledJson = Join-Path $outDir "mobildwg_native_test_result.json"
                }
                if (Test-Path $pulledJson) {
                    $jsonContent = Get-Content -LiteralPath $pulledJson -Raw | ConvertFrom-Json
                    if ($jsonContent.results) {
                        foreach ($r in $jsonContent.results) {
                            $nativeDetails += [PSCustomObject]@{
                                TestId = $r.TestId
                                Passed = [bool]$r.Passed
                                Details = [string]$r.Details
                            }
                            Log "  [NATIVE] $($r.TestId): $(if ($r.Passed) { 'PASS' } else { 'FAIL' }) - $($r.Details)"
                        }
                    }
                }

                if ($instCode -eq -1 -and $failedTestsCount -eq 0) {
                    $nativePassed = $true
                    Log "STAGE05_NATIVE_INSTRUMENTATION_PASS"
                    Log "STAGE13_NATIVE_TOUCH_FIDELITY_PASS"
                } else {
                    Log "NATIVE_INSTRUMENTATION_DEFECTS_DETECTED: $failedTestsCount test(s) failed on Android runtime."
                    $allPassed = $false
                }
            } finally {
                $ErrorActionPreference = $prevEap
            }
        } else {
            Log "ERROR: Test APK could not be found at $testApkPath"
            $allPassed = $false
        }
    }
}

# 7. Summary & Gate Verdict
$summaryObj = [PSCustomObject]@{
    RunId = $RunId
    Timestamp = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    GitHead = $head
    Suites = $suiteResults
    Native = [PSCustomObject]@{
        Executed = $nativeExecuted
        Passed = $nativePassed
        Details = $nativeDetails
    }
    AllPassed = $allPassed
}

$summaryObj | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $summaryJson

Log "========================================"
Log "=== GATE RUN SUMMARY ==="
Log "All Passed: $allPassed"
Log "Native Executed: $nativeExecuted, Native Passed: $nativePassed"
foreach ($k in $suiteResults.Keys) {
    $s = $suiteResults[$k]
    Log "  $($s.Name): Passed=$($s.Passed), FailedRegressions=$($s.FailedRegressions.Count)"
}
Log "Full report written to: $summaryJson"
Log "========================================"

if ($allPassed) {
    Log "VIEWER_CORRECTION_GATE_PASS"
    exit 0
} else {
    if ($AllowRegressions) {
        Log "GATE FINISHED WITH KNOWN REGRESSIONS (Allowed by -AllowRegressions flag)."
        exit 0
    } else {
        Log "VIEWER_CORRECTION_GATE_FAILED: Regressions detected. Fix defects before gate passes."
        exit 1
    }
}
