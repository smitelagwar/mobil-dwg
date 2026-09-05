namespace MobilDwg.Core.Performance;

public enum CadCorpusScale
{
    Small,
    Medium,
    Large
}

public sealed record CadTtfupMetrics(
    double DocumentPrepDurationMs,
    double ParseDurationMs,
    double SceneAssemblyDurationMs,
    double FirstPaintDurationMs,
    double TotalTtfupMs)
{
    public static CadTtfupMetrics Zero => new(0, 0, 0, 0, 0);
}

public sealed record CadFrameTimingStatistics
{
    public int SampleCount { get; init; }
    public double MeanMs { get; init; }
    public double MedianMs { get; init; }
    public double P95Ms { get; init; }
    public double MinMs { get; init; }
    public double MaxMs { get; init; }
    public double FpsEquivalentP50 => MedianMs > 0.001 ? 1000.0 / MedianMs : 0;

    public static CadFrameTimingStatistics FromSamples(IReadOnlyList<double> samples)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (samples.Count == 0)
        {
            return new CadFrameTimingStatistics
            {
                SampleCount = 0,
                MeanMs = 0,
                MedianMs = 0,
                P95Ms = 0,
                MinMs = 0,
                MaxMs = 0
            };
        }

        var sorted = samples.OrderBy(x => x).ToList();
        var count = sorted.Count;
        var mean = sorted.Average();
        var min = sorted[0];
        var max = sorted[^1];

        double median;
        if (count % 2 == 1)
        {
            median = sorted[count / 2];
        }
        else
        {
            median = (sorted[(count / 2) - 1] + sorted[count / 2]) / 2.0;
        }

        var p95Index = (int)Math.Ceiling(0.95 * count) - 1;
        p95Index = Math.Clamp(p95Index, 0, count - 1);
        var p95 = sorted[p95Index];

        return new CadFrameTimingStatistics
        {
            SampleCount = count,
            MeanMs = Math.Round(mean, 2),
            MedianMs = Math.Round(median, 2),
            P95Ms = Math.Round(p95, 2),
            MinMs = Math.Round(min, 2),
            MaxMs = Math.Round(max, 2)
        };
    }
}

public sealed record CadMemoryMetrics(
    long AllocatedBytesDelta,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    long NativeHeapBytes,
    long TotalMemoryBytes)
{
    public static CadMemoryMetrics CaptureCurrent(long baselineAllocatedBytes = 0, int baselineGen0 = 0, int baselineGen1 = 0, int baselineGen2 = 0)
    {
        var currentAllocated = GC.GetAllocatedBytesForCurrentThread();
        var currentGen0 = GC.CollectionCount(0);
        var currentGen1 = GC.CollectionCount(1);
        var currentGen2 = GC.CollectionCount(2);
        var totalMem = GC.GetTotalMemory(forceFullCollection: false);

        return new CadMemoryMetrics(
            AllocatedBytesDelta: Math.Max(0, currentAllocated - baselineAllocatedBytes),
            Gen0Collections: Math.Max(0, currentGen0 - baselineGen0),
            Gen1Collections: Math.Max(0, currentGen1 - baselineGen1),
            Gen2Collections: Math.Max(0, currentGen2 - baselineGen2),
            NativeHeapBytes: 0,
            TotalMemoryBytes: totalMem);
    }
}

public sealed record CadPerformanceReport(
    CadCorpusScale Scale,
    int EntityCount,
    CadTtfupMetrics Ttfup,
    CadFrameTimingStatistics FrameTimings,
    CadMemoryMetrics Memory,
    double OptimizationRatio = 1.0);

public sealed record CadPerformanceBudget(
    double MaxTtfupMs,
    double MaxP50Ms,
    double MaxP95Ms,
    long MaxAllocatedBytes)
{
    public static CadPerformanceBudget ForScale(CadCorpusScale scale) => scale switch
    {
        CadCorpusScale.Small => new CadPerformanceBudget(
            MaxTtfupMs: 500.0,
            MaxP50Ms: 33.3,
            MaxP95Ms: 66.7,
            MaxAllocatedBytes: 15 * 1024 * 1024),

        CadCorpusScale.Medium => new CadPerformanceBudget(
            MaxTtfupMs: 2000.0,
            MaxP50Ms: 66.7,
            MaxP95Ms: 120.0,
            MaxAllocatedBytes: 50 * 1024 * 1024),

        CadCorpusScale.Large => new CadPerformanceBudget(
            MaxTtfupMs: 6000.0,
            MaxP50Ms: 250.0,
            MaxP95Ms: 400.0,
            MaxAllocatedBytes: 150 * 1024 * 1024),

        _ => throw new ArgumentOutOfRangeException(nameof(scale))
    };
}
