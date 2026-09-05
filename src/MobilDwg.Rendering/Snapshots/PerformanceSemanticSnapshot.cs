using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MobilDwg.Core.Performance;

namespace MobilDwg.Rendering.Snapshots;

public sealed record PerformanceSnapshotResult(string Text, string Sha256Hash);

public static class PerformanceSemanticSnapshot
{
    public static PerformanceSnapshotResult Create(CadPerformanceReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("schema=performance-metrics/v1");
        sb.AppendLine($"report.scale={report.Scale}");
        sb.AppendLine($"report.entities={report.EntityCount}");

        sb.AppendLine($"ttfup.total_ms={report.Ttfup.TotalTtfupMs.ToString("F2", inv)}");
        sb.AppendLine($"ttfup.prep_ms={report.Ttfup.DocumentPrepDurationMs.ToString("F2", inv)}");
        sb.AppendLine($"ttfup.parse_ms={report.Ttfup.ParseDurationMs.ToString("F2", inv)}");
        sb.AppendLine($"ttfup.assembly_ms={report.Ttfup.SceneAssemblyDurationMs.ToString("F2", inv)}");
        sb.AppendLine($"ttfup.paint_ms={report.Ttfup.FirstPaintDurationMs.ToString("F2", inv)}");

        sb.AppendLine($"frames.samples={report.FrameTimings.SampleCount}");
        sb.AppendLine($"frames.mean_ms={report.FrameTimings.MeanMs.ToString("F2", inv)}");
        sb.AppendLine($"frames.p50_ms={report.FrameTimings.MedianMs.ToString("F2", inv)}");
        sb.AppendLine($"frames.p95_ms={report.FrameTimings.P95Ms.ToString("F2", inv)}");
        sb.AppendLine($"frames.min_ms={report.FrameTimings.MinMs.ToString("F2", inv)}");
        sb.AppendLine($"frames.max_ms={report.FrameTimings.MaxMs.ToString("F2", inv)}");
        sb.AppendLine($"frames.fps_p50={report.FrameTimings.FpsEquivalentP50.ToString("F1", inv)}");

        sb.AppendLine($"memory.allocated_delta_bytes={report.Memory.AllocatedBytesDelta}");
        sb.AppendLine($"memory.gen0={report.Memory.Gen0Collections}");
        sb.AppendLine($"memory.gen1={report.Memory.Gen1Collections}");
        sb.AppendLine($"memory.gen2={report.Memory.Gen2Collections}");
        sb.AppendLine($"memory.total_bytes={report.Memory.TotalMemoryBytes}");
        sb.AppendLine($"optimization.gain_ratio={report.OptimizationRatio.ToString("F2", inv)}");

        string text = sb.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hash = SHA256.HashData(bytes);
        string hashString = Convert.ToHexString(hash).ToLowerInvariant();

        return new PerformanceSnapshotResult(text, hashString);
    }
}
