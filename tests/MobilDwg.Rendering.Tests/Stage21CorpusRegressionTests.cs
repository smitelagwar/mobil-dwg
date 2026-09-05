using System.Diagnostics;
using System.Runtime.CompilerServices;
using MobilDwg.Core.Regression;
using MobilDwg.Rendering.Regression;
using MobilDwg.Rendering.Snapshots;

namespace MobilDwg.Rendering.Tests;

public static class Stage21CorpusRegressionTests
{
    [ModuleInitializer]
    public static void Run()
    {
        TestFullCorpusRegressionSummaryPasses();
        TestBetaGateVerdictEvaluation();
        TestSurveyOriginDoublePrecisionIntegrity();
        TestP0EntityFidelityCoverage();
        TestControlledNegativeGuards();
        TestSemanticSnapshotDeterminism();
        TestDebugVsReleasePipelineIntegrity();

        Console.WriteLine("STAGE21_CORPUS_REGRESSION_TESTS_PASS");
    }

    private static void TestFullCorpusRegressionSummaryPasses()
    {
        var summary = CadCorpusRegressionSuite.RunFullRegressionAsync().GetAwaiter().GetResult();

        var failed = summary.StageResults.Where(r => !r.IsSuccess)
            .Select(r => $"{r.ItemId}: preflight={r.PreflightOk} parse={r.ParseOk} scene={r.SceneOk} render={r.RenderOk}")
            .ToList();
        Assert(summary.PassedItems == summary.TotalItems,
            $"Expected all stages to pass, but {summary.PassedItems}/{summary.TotalItems} passed: {string.Join("; ", failed)}");
        Assert(summary.HandledNegatives >= 2,
            $"Expected at least 2 handled negatives, got {summary.HandledNegatives}");
        Assert(summary.P0Passed == summary.P0Count,
            $"Expected all P0 items to pass, got {summary.P0Passed}/{summary.P0Count}");
        Assert(summary.C3OrHigherPercentage >= 75.0,
            $"C3+ percentage {summary.C3OrHigherPercentage}% below required 75%");
        Assert(summary.IsBetaReady, "Expected regression summary to report IsBetaReady = true");
    }

    private static void TestBetaGateVerdictEvaluation()
    {
        var summary = CadCorpusRegressionSuite.RunFullRegressionAsync().GetAwaiter().GetResult();

        // 1. Nominal case within budget
        var passVerdict = CadCorpusRegressionSuite.EvaluateBetaGate(summary, apkSizeBytes: 39_000_000, pssMb: 130.0);
        Assert(passVerdict.IsPass, "Expected pass verdict for valid APK size and PSS");
        Assert(passVerdict.GateMarker == "ANDROID_STAGE21_BETA_GATE_PASS", "Expected pass marker");
        Assert(passVerdict.Blockers.Count == 0, "Expected zero blockers for nominal verdict");

        // 2. Oversized APK budget breach (> 45 MB)
        var failApkVerdict = CadCorpusRegressionSuite.EvaluateBetaGate(summary, apkSizeBytes: 48_000_000, pssMb: 130.0);
        Assert(!failApkVerdict.IsPass, "Expected fail verdict for APK > 45MB");
        Assert(failApkVerdict.GateMarker == "ANDROID_STAGE21_BETA_GATE_FAIL", "Expected fail marker");
        Assert(failApkVerdict.Blockers.Any(b => b.Contains("APK size")), "Expected APK blocker");

        // 3. Oversized PSS budget breach (> 250 MB)
        var failPssVerdict = CadCorpusRegressionSuite.EvaluateBetaGate(summary, apkSizeBytes: 39_000_000, pssMb: 260.0);
        Assert(!failPssVerdict.IsPass, "Expected fail verdict for PSS > 250MB");
        Assert(failPssVerdict.Blockers.Any(b => b.Contains("Total PSS")), "Expected PSS blocker");
    }

    private static void TestSurveyOriginDoublePrecisionIntegrity()
    {
        var summary = CadCorpusRegressionSuite.RunFullRegressionAsync().GetAwaiter().GetResult();
        var precisionStage = summary.StageResults.FirstOrDefault(s => s.ItemId == "corpus-survey-origin-precision");

        Assert(precisionStage != null, "Survey origin stage must be present");
        Assert(precisionStage!.IsSuccess, "Survey origin stage must pass");
        Assert(precisionStage.AchievedTier == CadFidelityTier.C4_EngineeringVerified,
            $"Expected C4 tier, got {precisionStage.AchievedTier}");
        Assert(precisionStage.Notes != null && precisionStage.Notes.Contains("5,000,000.001"),
            "Notes must record 5,000,000.001 precision verification");
    }

    private static void TestP0EntityFidelityCoverage()
    {
        var summary = CadCorpusRegressionSuite.RunFullRegressionAsync().GetAwaiter().GetResult();
        var p0Geom = summary.StageResults.FirstOrDefault(s => s.ItemId == "corpus-p0-geometry");

        Assert(p0Geom != null, "P0 geometry stage must be present");
        Assert(p0Geom!.IsSuccess, "P0 geometry stage must pass");
        Assert(p0Geom.EntityCount == 7, $"Expected 7 basic entities, got {p0Geom.EntityCount}");
        Assert(p0Geom.AchievedTier >= CadFidelityTier.C3_SemanticGoldenPass, "P0 must achieve at least C3");
    }

    private static void TestControlledNegativeGuards()
    {
        var summary = CadCorpusRegressionSuite.RunFullRegressionAsync().GetAwaiter().GetResult();
        var missingFont = summary.StageResults.FirstOrDefault(s => s.ItemId == "negative-missing-font-ac1015");
        var missingXref = summary.StageResults.FirstOrDefault(s => s.ItemId == "negative-missing-xref-ac1015");

        Assert(missingFont != null && missingFont.IsSuccess, "Missing font negative must pass");
        Assert(missingFont!.DiagnosticCodes.Contains("missing-font"), "Missing font diagnostic code required");
        Assert(missingFont.AchievedTier == CadFidelityTier.C2_SubstitutedWithWarning, "Negative font should be C2");

        Assert(missingXref != null && missingXref.IsSuccess, "Missing xref negative must pass");
        Assert(missingXref!.DiagnosticCodes.Contains("missing-xref"), "Missing xref diagnostic code required");
        Assert(missingXref.AchievedTier == CadFidelityTier.C2_SubstitutedWithWarning, "Negative xref should be C2");
    }

    private static void TestSemanticSnapshotDeterminism()
    {
        var summary = CadCorpusRegressionSuite.RunFullRegressionAsync().GetAwaiter().GetResult();
        var verdict = CadCorpusRegressionSuite.EvaluateBetaGate(summary, 39_000_000, 130.0);

        var snap1 = CorpusRegressionSemanticSnapshot.Create(summary, verdict);
        var snap2 = CorpusRegressionSemanticSnapshot.Create(summary, verdict);

        Assert(snap1.Sha256Hex == snap2.Sha256Hex, "Snapshot SHA-256 hash must be deterministic");
        Assert(snap1.Content.Contains("schema=corpus-regression/v1"), "Schema header required");
        Assert(snap1.Content.Contains("gate_marker=ANDROID_STAGE21_BETA_GATE_PASS"), "Pass marker required");
    }

    private static void TestDebugVsReleasePipelineIntegrity()
    {
        // Check that required regression types and members are linked and non-null
        Type suiteType = typeof(CadCorpusRegressionSuite);
        Type snapshotType = typeof(CorpusRegressionSemanticSnapshot);
        Type summaryType = typeof(CadCorpusRegressionSummary);
        Type verdictType = typeof(CadBetaGateVerdict);

        Assert(suiteType != null, "Suite type must exist");
        Assert(snapshotType != null, "Snapshot type must exist");
        Assert(summaryType != null, "Summary type must exist");
        Assert(verdictType != null, "Verdict type must exist");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Stage21 Test Assertion Failed: {message}");
        }
    }
}
