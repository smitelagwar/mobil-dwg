using System.Diagnostics;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Performance;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Dimensions;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Rendering.Performance;

public static class SyntheticPerformanceCorpus
{
    public static RenderScene CreateSmallCorpus()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        // Frame
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("S_FRAME"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(1000, 0)),
                new LinePrimitive(new WorldPoint2(1000, 0), new WorldPoint2(1000, 1000)),
                new LinePrimitive(new WorldPoint2(1000, 1000), new WorldPoint2(0, 1000)),
                new LinePrimitive(new WorldPoint2(0, 1000), new WorldPoint2(0, 0))
            ]));

        // 80 concentric circles and cross lines
        for (int i = 1; i <= 40; i++)
        {
            var r = i * 10.0;
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"S_ARC_{i}"),
                new RenderLayerToken("GEOMETRY"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("ARC"),
                [new ArcPrimitive(new WorldPoint2(500, 500), r, 0, Math.PI * 2)]));

            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"S_LINE_{i}"),
                new RenderLayerToken("GEOMETRY"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                [new LinePrimitive(new WorldPoint2(500 - r, 500), new WorldPoint2(500 + r, 500))]));
        }

        // Title and notes
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("S_TXT_TITLE"),
            new RenderLayerToken("ANNOTATION"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("KÜÇÜK ÖRNEK ÇİZİM - MOBIL DWG", new WorldPoint2(100, 920), height: 35)]));

        return assembler.Build();
    }

    public static RenderScene CreateMediumCorpus()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        // Outer border
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("M_BORDER"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(5000, 0)),
                new LinePrimitive(new WorldPoint2(5000, 0), new WorldPoint2(5000, 4000)),
                new LinePrimitive(new WorldPoint2(5000, 4000), new WorldPoint2(0, 4000)),
                new LinePrimitive(new WorldPoint2(0, 4000), new WorldPoint2(0, 0))
            ]));

        // Grid lines (e.g. 200 lines)
        for (int x = 100; x < 5000; x += 50)
        {
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"M_GRID_V_{x}"),
                new RenderLayerToken("GRID"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                [new LinePrimitive(new WorldPoint2(x, 100), new WorldPoint2(x, 3900))]));
        }

        for (int y = 100; y < 4000; y += 50)
        {
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"M_GRID_H_{y}"),
                new RenderLayerToken("GRID"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                [new LinePrimitive(new WorldPoint2(100, y), new WorldPoint2(4900, y))]));
        }

        // 500 elements (circles, polygons, hatches, dimensions)
        for (int i = 0; i < 500; i++)
        {
            double cx = 300 + ((i % 25) * 180);
            double cy = 300 + ((i / 25) * 160);

            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"M_OBJ_{i}"),
                new RenderLayerToken("WALLS"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                [
                    new LinePrimitive(new WorldPoint2(cx, cy), new WorldPoint2(cx + 80, cy)),
                    new LinePrimitive(new WorldPoint2(cx + 80, cy), new WorldPoint2(cx + 80, cy + 80)),
                    new LinePrimitive(new WorldPoint2(cx + 80, cy + 80), new WorldPoint2(cx, cy + 80)),
                    new LinePrimitive(new WorldPoint2(cx, cy + 80), new WorldPoint2(cx, cy))
                ]));

            if (i % 5 == 0)
            {
                assembler.AddEntity(new RenderSceneEntity(
                    new RenderEntityId($"M_DIM_{i}"),
                    new RenderLayerToken("DIMENSIONS"),
                    new RenderStyleToken("TRUECOLOR"),
                    new RenderSourceReference("DIMENSION"),
                    [new LinePrimitive(new WorldPoint2(cx, cy - 20), new WorldPoint2(cx + 80, cy - 20))]));
            }
        }

        // Title and notes
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("M_TXT_TITLE"),
            new RenderLayerToken("TEXT"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("ORTA ÖLÇEKLİ MİMARİ PLAN - 2000+ VARLIK", new WorldPoint2(200, 3950), height: 45)]));

        return assembler.Build();
    }

    public static RenderScene CreateLargeCorpus()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        // Dense engineering layout with 20,000 entities
        // Grid pattern: 200 columns x 100 rows = 20,000 cells
        for (int row = 0; row < 100; row++)
        {
            double y = row * 100.0;
            for (int col = 0; col < 200; col++)
            {
                double x = col * 100.0;
                int id = (row * 200) + col;

                assembler.AddEntity(new RenderSceneEntity(
                    new RenderEntityId($"L_E_{id}"),
                    new RenderLayerToken(col % 4 == 0 ? "STRUCTURE" : "SERVICES"),
                    new RenderStyleToken("BYLAYER"),
                    new RenderSourceReference("LINE"),
                    [
                        new LinePrimitive(new WorldPoint2(x, y), new WorldPoint2(x + 90, y + 90))
                    ]));
            }
        }

        // Header entity
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("L_HEADER"),
            new RenderLayerToken("0"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("BÜYÜK ÖLÇEKLİ CORPUS - 20,000 VARLIK", new WorldPoint2(0, 10100), height: 120)]));

        return assembler.Build();
    }

    public static async Task<(CadTtfupMetrics Metrics, byte[] Png)> MeasureTtfupAsync(
        Func<RenderScene> sceneFactory,
        string fileName = "Synthetic.dwg",
        int pixelWidth = 1080,
        int pixelHeight = 1080)
    {
        var swTotal = Stopwatch.StartNew();

        // Phase 1: Prep
        var swPhase = Stopwatch.StartNew();
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", fileName);
        await Task.Delay(1); // minimal simulated stream/io tick
        swPhase.Stop();
        var prepDuration = swPhase.Elapsed.TotalMilliseconds;

        // Phase 2: Parse / scene generation
        swPhase.Restart();
        var scene = sceneFactory();
        swPhase.Stop();
        var parseDuration = swPhase.Elapsed.TotalMilliseconds;

        // Phase 3: Layout / bounds resolution & session setup
        swPhase.Restart();
        var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var session = new CadViewerSession(metadata, scene, layoutManager, pixelWidth, pixelHeight);
        session.ZoomToFit();
        swPhase.Stop();
        var assemblyDuration = swPhase.Elapsed.TotalMilliseconds;

        // Phase 4: First paint (Skia rendering & PNG encode)
        swPhase.Restart();
        using var surface = new SkiaBitmapRenderSurface(session.ViewportPixelWidth, session.ViewportPixelHeight);
        await session.RenderAsync(surface);
        var png = surface.EncodePng();
        swPhase.Stop();
        var firstPaintDuration = swPhase.Elapsed.TotalMilliseconds;

        swTotal.Stop();
        var totalTtfup = swTotal.Elapsed.TotalMilliseconds;

        var metrics = new CadTtfupMetrics(
            Math.Round(prepDuration, 2),
            Math.Round(parseDuration, 2),
            Math.Round(assemblyDuration, 2),
            Math.Round(firstPaintDuration, 2),
            Math.Round(totalTtfup, 2));

        return (metrics, png);
    }

    public static async Task<CadFrameTimingStatistics> MeasureFrameTimingsAsync(
        CadViewerSession session,
        SkiaBitmapRenderSurface surface,
        int frameCount = 20)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(surface);

        var frameTimes = new List<double>(frameCount);
        var sw = new Stopwatch();

        for (int i = 0; i < frameCount; i++)
        {
            // Simulate interaction: pan slightly back and forth
            var panDelta = (i % 2 == 0) ? 25.0 : -25.0;
            session.Pan(panDelta, panDelta * 0.5);

            sw.Restart();
            await session.RenderAsync(surface);
            sw.Stop();

            frameTimes.Add(sw.Elapsed.TotalMilliseconds);
        }

        return CadFrameTimingStatistics.FromSamples(frameTimes);
    }

    public static async Task<double> MeasureAbOptimizationRatioAsync(
        RenderScene scene,
        int pixelWidth = 1080,
        int pixelHeight = 1080,
        int iterations = 10)
    {
        var layoutManagerA = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "ABTest.dwg");

        // Baseline (Unoptimized)
        using var surfaceA = new SkiaBitmapRenderSurface(pixelWidth, pixelHeight);
        var sessionA = new CadViewerSession(metadata, scene, layoutManagerA, pixelWidth, pixelHeight);
        sessionA.Renderer.OptimizationMode = RenderOptimizationMode.BaselineUnoptimized;
        sessionA.ZoomToFit();

        // Warmup
        await sessionA.RenderAsync(surfaceA);

        var swA = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await sessionA.RenderAsync(surfaceA);
        }
        swA.Stop();
        var elapsedBaseline = swA.Elapsed.TotalMilliseconds;

        // Optimized
        var layoutManagerB = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        using var surfaceB = new SkiaBitmapRenderSurface(pixelWidth, pixelHeight);
        var sessionB = new CadViewerSession(metadata, scene, layoutManagerB, pixelWidth, pixelHeight);
        sessionB.Renderer.OptimizationMode = RenderOptimizationMode.Optimized;
        sessionB.ZoomToFit();

        // Warmup
        await sessionB.RenderAsync(surfaceB);

        var swB = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await sessionB.RenderAsync(surfaceB);
        }
        swB.Stop();
        var elapsedOptimized = swB.Elapsed.TotalMilliseconds;

        if (elapsedOptimized <= 0.001) elapsedOptimized = 0.001;
        return Math.Round(elapsedBaseline / elapsedOptimized, 2);
    }
}
