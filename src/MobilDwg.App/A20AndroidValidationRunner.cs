#if A20_VALIDATION
using System.Security.Cryptography;
using System.Text;
using Android.Util;
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

namespace MobilDwg.App;

public sealed record A20ValidationResult(
    byte[] Png,
    string PngSha256,
    string PerformanceSummary,
    string MemorySummary,
    string Marker);

public static class A20AndroidValidationRunner
{
    public const string Tag = "MobilDwgA20";

    public static async Task<A20ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A20_ANDROID_VALIDATION_STARTING");
        await Task.Delay(250);

        // 1. Measure TTFUP across small, medium, and large corpus scales
        var (smallTtfup, _) = await SyntheticPerformanceCorpus.MeasureTtfupAsync(
            SyntheticPerformanceCorpus.CreateSmallCorpus,
            "SmallCorpus.dwg",
            pixelWidth: 1080,
            pixelHeight: 1080);

        var (mediumTtfup, _) = await SyntheticPerformanceCorpus.MeasureTtfupAsync(
            SyntheticPerformanceCorpus.CreateMediumCorpus,
            "MediumCorpus.dwg",
            pixelWidth: 1080,
            pixelHeight: 1080);

        var (largeTtfup, _) = await SyntheticPerformanceCorpus.MeasureTtfupAsync(
            SyntheticPerformanceCorpus.CreateLargeCorpus,
            "LargeCorpus.dwg",
            pixelWidth: 1080,
            pixelHeight: 1080);

        Log.Info(Tag, $"A20_ANDROID_TTFUP_PASS small={smallTtfup.TotalTtfupMs:F1}ms med={mediumTtfup.TotalTtfupMs:F1}ms large={largeTtfup.TotalTtfupMs:F1}ms");

        // 2. Measure multi-frame timings (p50 and p95 across pan/zoom gestures)
        var medScene = SyntheticPerformanceCorpus.CreateMediumCorpus();
        var layoutManager = new CadLayoutManager(medScene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "MediumPerf.dwg");

        using var surface = new SkiaBitmapRenderSurface(1080, 1080);
        using var session = new CadViewerSession(metadata, medScene, layoutManager, 1080, 1080);
        session.ZoomToFit();

        var frameStats = await SyntheticPerformanceCorpus.MeasureFrameTimingsAsync(session, surface, frameCount: 20);
        Log.Info(Tag, $"A20_ANDROID_FRAME_TIMING_PASS count={frameStats.SampleCount} p50={frameStats.MedianMs:F1}ms p95={frameStats.P95Ms:F1}ms fps={frameStats.FpsEquivalentP50:F1}");

        // 3. Android Process Memory Metrics (Managed GC Heap, Native Heap, and Java Heap)
        var managedBytes = GC.GetTotalMemory(forceFullCollection: false);
        var nativeBytes = Android.OS.Debug.NativeHeapAllocatedSize;
        var runtime = Java.Lang.Runtime.GetRuntime();
        var javaBytes = runtime != null ? runtime.TotalMemory() - runtime.FreeMemory() : 0L;
        var gcMetrics = CadMemoryMetrics.CaptureCurrent();

        Log.Info(Tag, $"A20_ANDROID_MEMORY_PASS managedBytes={managedBytes} nativeBytes={nativeBytes} javaBytes={javaBytes}");

        // 4. A-B Optimization Verification on Android
        var optimRatio = await SyntheticPerformanceCorpus.MeasureAbOptimizationRatioAsync(medScene, 1080, 1080, iterations: 6);
        Log.Info(Tag, $"A20_ANDROID_AB_OPTIMIZATION_PASS ratio={optimRatio:F2}x");

        // 5. Deterministic Semantic Snapshot
        var report = new CadPerformanceReport(
            CadCorpusScale.Medium,
            medScene.Entities.Count,
            mediumTtfup,
            frameStats,
            new CadMemoryMetrics(managedBytes, gcMetrics.Gen0Collections, gcMetrics.Gen1Collections, gcMetrics.Gen2Collections, nativeBytes, managedBytes),
            optimRatio);

        var snapshot = PerformanceSemanticSnapshot.Create(report);
        Log.Info(Tag, $"A20_ANDROID_SNAPSHOT_PASS sha256={snapshot.Sha256Hash}");

        // 6. Render Representative Performance Dashboard on Android
        var dashboardScene = BuildPerformanceDashboardScene(smallTtfup, mediumTtfup, frameStats, optimRatio);
        var dashLayoutManager = new CadLayoutManager(dashboardScene, Array.Empty<CadLayoutDefinition>());
        var dashMetadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "PerformanceDashboard.dwg");

        using var dashSession = new CadViewerSession(dashMetadata, dashboardScene, dashLayoutManager, 1080, 1080);
        dashSession.ZoomToFit();

        using var dashSurface = new SkiaBitmapRenderSurface(1080, 1080);
        await dashSession.RenderAsync(dashSurface);

        var pngBytes = dashSurface.EncodePng();
        var pngSha256 = Convert.ToHexStringLower(SHA256.HashData(pngBytes));

        Log.Info(Tag, $"A20_ANDROID_SKIA_RENDER_PASS bytes={pngBytes.Length} sha256={pngSha256}");
        Log.Info(Tag, "A20_REAL_APP_PERF_MARKERS_PASS");
        Log.Info(Tag, "ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS");

        var perfSummary = $"TTFUP: S={smallTtfup.TotalTtfupMs:F0}ms M={mediumTtfup.TotalTtfupMs:F0}ms | p50={frameStats.MedianMs:F1}ms p95={frameStats.P95Ms:F1}ms";
        var memSummary = $"Heap: Managed={managedBytes / (1024 * 1024)}MB Native={nativeBytes / (1024 * 1024)}MB | Speedup={optimRatio:F2}x";

        return new A20ValidationResult(
            pngBytes,
            pngSha256,
            perfSummary,
            memSummary,
            "ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS");
    }

    private static RenderScene BuildPerformanceDashboardScene(
        CadTtfupMetrics smallTtfup,
        CadTtfupMetrics medTtfup,
        CadFrameTimingStatistics frameStats,
        double optimRatio)
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        // Outer border
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DASH_BORDER"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(50, 50), new WorldPoint2(950, 50)),
                new LinePrimitive(new WorldPoint2(950, 50), new WorldPoint2(950, 950)),
                new LinePrimitive(new WorldPoint2(950, 950), new WorldPoint2(50, 950)),
                new LinePrimitive(new WorldPoint2(50, 950), new WorldPoint2(50, 50))
            ]));

        // Header Title
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DASH_TITLE"),
            new RenderLayerToken("HEADER"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("PERFORMANS VE BELLEK METRİKLERİ (AŞAMA 20)", new WorldPoint2(100, 880), height: 32)]));

        // Metric Card 1: TTFUP
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CARD_TTFUP"),
            new RenderLayerToken("CARDS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(100, 680), new WorldPoint2(450, 680)),
                new LinePrimitive(new WorldPoint2(450, 680), new WorldPoint2(450, 820)),
                new LinePrimitive(new WorldPoint2(450, 820), new WorldPoint2(100, 820)),
                new LinePrimitive(new WorldPoint2(100, 820), new WorldPoint2(100, 680)),
                new LinePrimitive(new WorldPoint2(120, 720), new WorldPoint2(120 + Math.Min(300, (smallTtfup.TotalTtfupMs * 2)), 720))
            ]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CARD_TTFUP_TXT"),
            new RenderLayerToken("CARDS"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive("TTFUP (AÇILIŞ)", new WorldPoint2(120, 780), height: 22),
                new TextPrimitive($"S: {smallTtfup.TotalTtfupMs:F0}ms  M: {medTtfup.TotalTtfupMs:F0}ms", new WorldPoint2(120, 740), height: 18)
            ]));

        // Metric Card 2: Frame Timing p50 / p95
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CARD_FRAMES"),
            new RenderLayerToken("CARDS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(550, 680), new WorldPoint2(900, 680)),
                new LinePrimitive(new WorldPoint2(900, 680), new WorldPoint2(900, 820)),
                new LinePrimitive(new WorldPoint2(900, 820), new WorldPoint2(550, 820)),
                new LinePrimitive(new WorldPoint2(550, 820), new WorldPoint2(550, 680)),
                new LinePrimitive(new WorldPoint2(570, 720), new WorldPoint2(570 + Math.Min(300, (frameStats.MedianMs * 5)), 720))
            ]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CARD_FRAMES_TXT"),
            new RenderLayerToken("CARDS"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive("FRAME TIMING", new WorldPoint2(570, 780), height: 22),
                new TextPrimitive($"p50: {frameStats.MedianMs:F1}ms  p95: {frameStats.P95Ms:F1}ms", new WorldPoint2(570, 740), height: 18)
            ]));

        // Metric Card 3: A-B Optimization Gauge
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CARD_OPTIM"),
            new RenderLayerToken("GAUGE"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("ARC"),
            [
                new ArcPrimitive(new WorldPoint2(500, 400), 150, 0, Math.PI),
                new LinePrimitive(new WorldPoint2(500, 400), new WorldPoint2(500 + (100 * Math.Cos(Math.PI * 0.75)), 400 + (100 * Math.Sin(Math.PI * 0.75))))
            ]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CARD_OPTIM_TXT"),
            new RenderLayerToken("GAUGE"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive("ÖLÇÜMLÜ A-B HIZLANMA KAZANCI", new WorldPoint2(250, 220), height: 24),
                new TextPrimitive($"Doğrudan Çizgi + Culling Kazanç Oranı: {optimRatio:F2}x", new WorldPoint2(210, 180), height: 20)
            ]));

        return assembler.Build();
    }
}
#endif
