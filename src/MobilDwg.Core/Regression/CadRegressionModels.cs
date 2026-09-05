using System.Globalization;
using MobilDwg.Core.Documents;

namespace MobilDwg.Core.Regression;

public enum CadCorpusItemType
{
    CommittedFixture,
    GeneratedFixture,
    SyntheticGeometry,
    NegativeGuard,
    PrecisionBenchmark,
    PerformanceStress,
}

public enum CadFidelityTier
{
    C0_Unsupported = 0,
    C1_ParsedOnly = 1,
    C2_SubstitutedWithWarning = 2,
    C3_SemanticGoldenPass = 3,
    C4_EngineeringVerified = 4,
}

public sealed record CadCorpusItem(
    string Id,
    string DisplayName,
    CadFormat Format,
    string Role,
    CadCorpusItemType ItemType,
    string RequiredFeature,
    int ExpectedMinimumEntities,
    CadFidelityTier TargetTier);

public sealed record CadRegressionStageResult(
    string ItemId,
    bool PreflightOk,
    bool ParseOk,
    bool SceneOk,
    bool RenderOk,
    int EntityCount,
    CadFidelityTier AchievedTier,
    IReadOnlyList<string> DiagnosticCodes,
    double ElapsedMs,
    string? Notes = null)
{
    public bool IsSuccess => PreflightOk && ParseOk && SceneOk && RenderOk;

    public string ToSummaryLine()
    {
        var diag = DiagnosticCodes.Count > 0 ? string.Join(",", DiagnosticCodes) : "none";
        return string.Format(
            CultureInfo.InvariantCulture,
            "id={0}|tier={1}|entities={2}|time={3:F1}ms|diag={4}",
            ItemId,
            AchievedTier,
            EntityCount,
            ElapsedMs,
            diag);
    }
}

public sealed record CadCorpusRegressionSummary(
    int TotalItems,
    int PassedItems,
    int HandledNegatives,
    int P0Count,
    int P0Passed,
    int P1Count,
    int P1Passed,
    double C3OrHigherPercentage,
    double TotalElapsedMs,
    IReadOnlyList<CadRegressionStageResult> StageResults)
{
    public bool IsBetaReady =>
        PassedItems == TotalItems &&
        P0Passed == P0Count &&
        P1Passed == P1Count &&
        C3OrHigherPercentage >= 85.0;
}

public sealed record CadBetaGateVerdict(
    bool IsPass,
    string GateMarker,
    CadCorpusRegressionSummary Summary,
    IReadOnlyList<string> Blockers);
