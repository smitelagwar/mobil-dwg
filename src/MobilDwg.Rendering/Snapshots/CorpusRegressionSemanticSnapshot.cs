using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MobilDwg.Core.Regression;

namespace MobilDwg.Rendering.Snapshots;

public sealed record CorpusRegressionSemanticSnapshot(string Content, string Sha256Hex)
{
    public static CorpusRegressionSemanticSnapshot Create(CadCorpusRegressionSummary summary, CadBetaGateVerdict verdict)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"schema=corpus-regression/v1");
        sb.AppendLine(CultureInfo.InvariantCulture, $"gate_marker={verdict.GateMarker}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"is_pass={verdict.IsPass}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"total_items={summary.TotalItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"passed_items={summary.PassedItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"handled_negatives={summary.HandledNegatives}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"p0_count={summary.P0Count}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"p0_passed={summary.P0Passed}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"p1_count={summary.P1Count}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"p1_passed={summary.P1Passed}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"c3_or_higher_pct={summary.C3OrHigherPercentage:F1}");

        var sortedResults = summary.StageResults.OrderBy(r => r.ItemId, StringComparer.Ordinal).ToList();
        sb.AppendLine(CultureInfo.InvariantCulture, $"stages_count={sortedResults.Count}");
        foreach (var r in sortedResults)
        {
            var diags = r.DiagnosticCodes.Count > 0 ? string.Join(",", r.DiagnosticCodes.OrderBy(x => x, StringComparer.Ordinal)) : "none";
            sb.AppendLine(CultureInfo.InvariantCulture, $"stage={r.ItemId}|tier={r.AchievedTier}|entities={r.EntityCount}|preflight={r.PreflightOk}|parse={r.ParseOk}|scene={r.SceneOk}|render={r.RenderOk}|diags={diags}");
        }

        string text = sb.ToString();
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
        return new CorpusRegressionSemanticSnapshot(text, hash);
    }
}
