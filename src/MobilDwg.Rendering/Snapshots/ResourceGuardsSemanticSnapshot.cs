using System.Security.Cryptography;
using System.Text;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Guards;

namespace MobilDwg.Rendering.Snapshots;

public sealed record ResourceGuardsSnapshotResult(string Text, string Sha256Hash);

public static class ResourceGuardsSemanticSnapshot
{
    public static ResourceGuardsSnapshotResult Create(
        CadPreflightResult preflight,
        CadResourceBudget budget,
        IEnumerable<CadDiagnostic> diagnostics,
        bool cycleDetected = false,
        bool nanSanitized = false,
        int fuzzTestPasses = 0)
    {
        ArgumentNullException.ThrowIfNull(preflight);
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var sb = new StringBuilder();
        sb.AppendLine("schema=resource-guards/v1");
        sb.AppendLine($"preflight.status={preflight.Status}");
        sb.AppendLine($"preflight.format={preflight.Format?.ToString() ?? "None"}");
        sb.AppendLine($"preflight.version={preflight.Version ?? "None"}");
        sb.AppendLine($"preflight.code={preflight.DiagnosticCode}");

        sb.AppendLine($"budget.max_file_size={budget.MaxFileSizeBytes}");
        sb.AppendLine($"budget.max_entities={budget.MaxEntities}");
        sb.AppendLine($"budget.max_block_depth={budget.MaxBlockNestingDepth}");
        sb.AppendLine($"budget.max_text_len={budget.MaxTextLength}");
        sb.AppendLine($"budget.max_hatch_seg={budget.MaxHatchBoundarySegments}");
        sb.AppendLine($"budget.max_raster_dim={budget.MaxRasterDimensionPixels}");
        sb.AppendLine($"budget.max_raster_mp={budget.MaxRasterTotalPixels / 1_000_000}MP");

        sb.AppendLine($"guards.cycle_detected={cycleDetected.ToString().ToLowerInvariant()}");
        sb.AppendLine($"guards.nan_sanitized={nanSanitized.ToString().ToLowerInvariant()}");
        sb.AppendLine($"guards.fuzz_passes={fuzzTestPasses}");

        var sortedDiagnostics = diagnostics
            .OrderBy(d => d.Code, StringComparer.Ordinal)
            .ThenBy(d => d.Severity)
            .ToList();

        sb.AppendLine($"diagnostics.count={sortedDiagnostics.Count}");
        foreach (var diag in sortedDiagnostics)
        {
            sb.AppendLine($"diagnostic={diag.Code}|{diag.Severity}|{diag.Message}");
        }

        string text = sb.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hash = SHA256.HashData(bytes);
        string hashString = Convert.ToHexString(hash).ToLowerInvariant();

        return new ResourceGuardsSnapshotResult(text, hashString);
    }
}
