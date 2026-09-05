using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Performance;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Performance;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Rendering.Tests;

public static class Stage20PerformanceMemoryTests
{
    [ModuleInitializer]
    public static void Run()
    {
        TestSmallCorpusTtfupAndFrameTimingWithinBudgets();
        TestMediumCorpusTtfupAndFrameTimingWithinBudgets();
        TestLargeCorpusTtfupAndFrameTimingWithinBudgets();
        TestFrameTimingStatisticsDistributionCalculation();
        TestMemoryTrackingAndGcCollections();
        TestSkiaCadRendererLineOptimizationAbBenchmark();
        TestViewportCullingOptimizationAbBenchmark();
        TestSyntheticTurkishDxfPerformanceMetrics();
        TestPerformanceSemanticSnapshotDeterminism();
        TestInvalidPerformanceInputsHandledSafely();

        Console.WriteLine("STAGE20_PERFORMANCE_MEMORY_TESTS_PASS");
    }

    private static void TestSmallCorpusTtfupAndFrameTimingWithinBudgets()
    {
        var budget = CadPerformanceBudget.ForScale(CadCorpusScale.Small);
        var (ttfup, png) = SyntheticPerformanceCorpus.MeasureTtfupAsync(
            () => SyntheticPerformanceCorpus.CreateSmallCorpus(),
            "SmallPlan.dwg",
            pixelWidth: 800,
            pixelHeight: 800).GetAwaiter().GetResult();

        Assert(ttfup.TotalTtfupMs > 0, "TTFUP must be positive");
        Assert(ttfup.TotalTtfupMs <= budget.MaxTtfupMs,
            $"Small TTFUP {ttfup.TotalTtfupMs} ms exceeded budget {budget.MaxTtfupMs} ms");
        Assert(png.Length > 100, "Rendered PNG must be non-empty");

        var scene = SyntheticPerformanceCorpus.CreateSmallCorpus();
        var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "SmallPlan.dwg");
        using var surface = new SkiaBitmapRenderSurface(800, 800);
        using var session = new CadViewerSession(metadata, scene, layoutManager, 800, 800);
        session.ZoomToFit();

        var frames = SyntheticPerformanceCorpus.MeasureFrameTimingsAsync(session, surface, frameCount: 15).GetAwaiter().GetResult();
        Assert(frames.SampleCount == 15, "Frame sample count must match");
        Assert(frames.MedianMs <= budget.MaxP50Ms,
            $"Small p50 {frames.MedianMs} ms exceeded budget {budget.MaxP50Ms} ms");
        Assert(frames.P95Ms <= budget.MaxP95Ms,
            $"Small p95 {frames.P95Ms} ms exceeded budget {budget.MaxP95Ms} ms");
        Assert(frames.FpsEquivalentP50 > 0, "FPS equivalent must be positive");
    }

    private static void TestMediumCorpusTtfupAndFrameTimingWithinBudgets()
    {
        var budget = CadPerformanceBudget.ForScale(CadCorpusScale.Medium);
        var (ttfup, png) = SyntheticPerformanceCorpus.MeasureTtfupAsync(
            () => SyntheticPerformanceCorpus.CreateMediumCorpus(),
            "MediumPlan.dwg",
            pixelWidth: 800,
            pixelHeight: 800).GetAwaiter().GetResult();

        Assert(ttfup.TotalTtfupMs > 0, "Medium TTFUP must be positive");
        Assert(ttfup.TotalTtfupMs <= budget.MaxTtfupMs,
            $"Medium TTFUP {ttfup.TotalTtfupMs} ms exceeded budget {budget.MaxTtfupMs} ms");

        var scene = SyntheticPerformanceCorpus.CreateMediumCorpus();
        var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "MediumPlan.dwg");
        using var surface = new SkiaBitmapRenderSurface(800, 800);
        using var session = new CadViewerSession(metadata, scene, layoutManager, 800, 800);
        session.ZoomToFit();

        var frames = SyntheticPerformanceCorpus.MeasureFrameTimingsAsync(session, surface, frameCount: 15).GetAwaiter().GetResult();
        Assert(frames.SampleCount == 15, "Medium frame sample count must match");
        Assert(frames.MedianMs <= budget.MaxP50Ms,
            $"Medium p50 {frames.MedianMs} ms exceeded budget {budget.MaxP50Ms} ms");
        Assert(frames.P95Ms <= budget.MaxP95Ms,
            $"Medium p95 {frames.P95Ms} ms exceeded budget {budget.MaxP95Ms} ms");
    }

    private static void TestLargeCorpusTtfupAndFrameTimingWithinBudgets()
    {
        var budget = CadPerformanceBudget.ForScale(CadCorpusScale.Large);
        var (ttfup, _) = SyntheticPerformanceCorpus.MeasureTtfupAsync(
            () => SyntheticPerformanceCorpus.CreateLargeCorpus(),
            "LargePlan.dwg",
            pixelWidth: 800,
            pixelHeight: 800).GetAwaiter().GetResult();

        Assert(ttfup.TotalTtfupMs <= budget.MaxTtfupMs,
            $"Large TTFUP {ttfup.TotalTtfupMs} ms exceeded budget {budget.MaxTtfupMs} ms");

        var scene = SyntheticPerformanceCorpus.CreateLargeCorpus();
        Assert(scene.Entities.Count >= 20000, "Large corpus must have at least 20,000 entities");

        var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "LargePlan.dwg");
        using var surface = new SkiaBitmapRenderSurface(800, 800);
        using var session = new CadViewerSession(metadata, scene, layoutManager, 800, 800);
        session.ZoomToFit();

        var frames = SyntheticPerformanceCorpus.MeasureFrameTimingsAsync(session, surface, frameCount: 10).GetAwaiter().GetResult();
        Assert(frames.SampleCount == 10, "Large frame sample count must match");
        Assert(frames.MedianMs <= budget.MaxP50Ms,
            $"Large p50 {frames.MedianMs} ms exceeded budget {budget.MaxP50Ms} ms");
        Assert(frames.P95Ms <= budget.MaxP95Ms,
            $"Large p95 {frames.P95Ms} ms exceeded budget {budget.MaxP95Ms} ms");
    }

    private static void TestFrameTimingStatisticsDistributionCalculation()
    {
        // 100 samples with known values 1 to 100
        var samples = Enumerable.Range(1, 100).Select(i => (double)i).ToList();
        var stats = CadFrameTimingStatistics.FromSamples(samples);

        Assert(stats.SampleCount == 100, "Sample count must be 100");
        Assert(Math.Abs(stats.MinMs - 1.0) < 0.01, "Min must be 1");
        Assert(Math.Abs(stats.MaxMs - 100.0) < 0.01, "Max must be 100");
        Assert(Math.Abs(stats.MeanMs - 50.5) < 0.01, "Mean must be 50.5");
        Assert(Math.Abs(stats.MedianMs - 50.5) < 0.01, "Median must be 50.5");
        Assert(Math.Abs(stats.P95Ms - 95.0) < 0.01, "P95 must be 95");
        Assert(stats.FpsEquivalentP50 > 19.7 && stats.FpsEquivalentP50 < 19.9, "FPS equivalent for 50.5ms");
    }

    private static void TestMemoryTrackingAndGcCollections()
    {
        var baselineAllocated = GC.GetAllocatedBytesForCurrentThread();
        var baselineGen0 = GC.CollectionCount(0);
        var baselineGen1 = GC.CollectionCount(1);
        var baselineGen2 = GC.CollectionCount(2);

        // Allocate a 2 MB array
        var dummyData = new byte[2 * 1024 * 1024];
        dummyData[0] = 42;

        var memory = CadMemoryMetrics.CaptureCurrent(baselineAllocated, baselineGen0, baselineGen1, baselineGen2);
        Assert(memory.AllocatedBytesDelta >= 2 * 1024 * 1024,
            $"Allocated bytes delta {memory.AllocatedBytesDelta} should be at least 2MB");
        Assert(memory.TotalMemoryBytes > 0, "Total memory must be positive");
    }

    private static void TestSkiaCadRendererLineOptimizationAbBenchmark()
    {
        // Build a scene with 3,000 line primitives
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        for (int i = 0; i < 3000; i++)
        {
            double x = (i % 100) * 10.0;
            double y = (i / 100) * 10.0;
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"LINE_{i}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                [new LinePrimitive(new WorldPoint2(x, y), new WorldPoint2(x + 8, y + 8))]));
        }
        var scene = assembler.Build();

        var ratio = SyntheticPerformanceCorpus.MeasureAbOptimizationRatioAsync(
            scene,
            pixelWidth: 800,
            pixelHeight: 800,
            iterations: 8).GetAwaiter().GetResult();

        Assert(ratio >= 1.05,
            $"Direct line rendering optimization ratio ({ratio}x) must provide measurable improvement (>= 1.05x)");
    }

    private static void TestViewportCullingOptimizationAbBenchmark()
    {
        // Build a scene spread over a large area: 0 to 10,000 in X and Y
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        for (int i = 0; i < 2000; i++)
        {
            double x = (i % 50) * 200.0;
            double y = (i / 50) * 200.0;
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"GRID_{i}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                [new LinePrimitive(new WorldPoint2(x, y), new WorldPoint2(x + 50, y + 50))]));
        }
        var scene = assembler.Build();

        // Create camera zoomed tightly into center (5000, 5000) with small view area
        var camera = new Camera2D(800, 800, new WorldPoint2(5000, 5000), worldUnitsPerPixel: 0.2);
        var visibleBounds = camera.GetVisibleWorldBounds(paddingFraction: 0.05);

        // Count how many entities intersect the visible bounds vs total
        int visibleCount = scene.Entities.Count(e => e.Bounds.Intersects(visibleBounds));
        Assert(visibleCount < scene.Entities.Count / 4,
            $"Zoomed view should only see a small subset of entities ({visibleCount}/{scene.Entities.Count})");
    }

    private static void TestSyntheticTurkishDxfPerformanceMetrics()
    {
        // Create synthetic turkish scene (representing the turkish basic DXF content)
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TURKISH_E1"),
            new RenderLayerToken("PLAN"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive("ZEMİN KAT PLANI - ÇŞĞİÖÜ", new WorldPoint2(50, 50), 25),
                new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(1000, 0)),
                new LinePrimitive(new WorldPoint2(1000, 0), new WorldPoint2(1000, 1000)),
                new ArcPrimitive(new WorldPoint2(500, 500), 200, 0, Math.PI)
            ]));
        var scene = assembler.Build();

        var (ttfup, png) = SyntheticPerformanceCorpus.MeasureTtfupAsync(
            () => scene,
            "TurkishSample.dxf",
            pixelWidth: 600,
            pixelHeight: 600).GetAwaiter().GetResult();

        Assert(ttfup.TotalTtfupMs <= 500, "Turkish synthetic TTFUP should be fast (< 500ms)");
        Assert(png.Length > 0, "PNG output must be generated");
    }

    private static void TestPerformanceSemanticSnapshotDeterminism()
    {
        var ttfup = new CadTtfupMetrics(1.2, 5.4, 2.1, 10.3, 19.0);
        var frames = new CadFrameTimingStatistics
        {
            SampleCount = 20,
            MeanMs = 12.5,
            MedianMs = 12.0,
            P95Ms = 15.2,
            MinMs = 10.1,
            MaxMs = 16.0
        };
        var memory = new CadMemoryMetrics(1024 * 1024, 2, 0, 0, 0, 8 * 1024 * 1024);
        var report = new CadPerformanceReport(CadCorpusScale.Medium, 2000, ttfup, frames, memory, OptimizationRatio: 2.15);

        var snap1 = PerformanceSemanticSnapshot.Create(report);
        var snap2 = PerformanceSemanticSnapshot.Create(report);

        Assert(snap1.Sha256Hash == snap2.Sha256Hash, "Snapshots of identical report must produce identical SHA256");
        Assert(snap1.Text.Contains("schema=performance-metrics/v1", StringComparison.Ordinal),
            "Snapshot must contain schema identifier");
        Assert(snap1.Text.Contains("report.scale=Medium", StringComparison.Ordinal),
            "Snapshot must record scale");
        Assert(snap1.Text.Contains("ttfup.total_ms=19.00", StringComparison.Ordinal),
            "Snapshot must record TTFUP");
        Assert(snap1.Text.Contains("frames.p50_ms=12.00", StringComparison.Ordinal),
            "Snapshot must record p50");
        Assert(snap1.Text.Contains("optimization.gain_ratio=2.15", StringComparison.Ordinal),
            "Snapshot must record optimization gain ratio");
    }

    private static void TestInvalidPerformanceInputsHandledSafely()
    {
        var emptyStats = CadFrameTimingStatistics.FromSamples(Array.Empty<double>());
        Assert(emptyStats.SampleCount == 0, "Empty samples count must be 0");
        Assert(emptyStats.MedianMs == 0, "Empty samples median must be 0");
        Assert(emptyStats.FpsEquivalentP50 == 0, "Empty samples FPS must be 0");

        var zeroTtfup = CadTtfupMetrics.Zero;
        Assert(zeroTtfup.TotalTtfupMs == 0, "Zero TTFUP total must be 0");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"STAGE 20 PERF TEST ASSERTION FAILED: {message}");
        }
    }
}
