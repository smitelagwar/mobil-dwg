using System;
using System.Collections.Generic;
using MobilDwg.Core.Documents;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.References;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Transforms;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Rendering.Tests;

public static class PreparedGeometryCacheTests
{
    public static void Run()
    {
        Test100PanFramesZeroRetessellationAndZeroRedecode();
        TestLodBandHysteresisPreventsThrashing();
        TestCacheBudgetAndLruEviction();
        TestSessionCloseResetsCacheOwnership();
        TestInteractionHatchNeverOpacifies();
        TestGeometryImmutabilityDuringPan();
        TestHatchCoverageMissOutsideBounds();
        Console.WriteLine("STAGE07_PREPARED_GEOMETRY_CACHE_TESTS_PASS");
    }

    private static void Test100PanFramesZeroRetessellationAndZeroRedecode()
    {
        // 1. Build a scene with curved primitives and raster image
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        // Arc
        var arc = new ArcPrimitive(new WorldPoint2(100, 100), 50.0, 0.0, Math.PI);
        // Ellipse
        var ellipse = new EllipsePrimitive(new WorldPoint2(200, 100), 40.0, 20.0, 0.0, 0.0, Math.PI * 1.5);
        // Polyline with bulge
        var vertices = new[]
        {
            new PolylineVertex(new WorldPoint2(0, 0), 0.5),
            new PolylineVertex(new WorldPoint2(50, 0), 0.0),
            new PolylineVertex(new WorldPoint2(50, 50), 0.0)
        };
        var poly = new PolylinePrimitive(vertices, closed: false);
        // Spline
        var controlPoints = new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(10, 30),
            new WorldPoint2(40, 40),
            new WorldPoint2(60, 0)
        };
        var knots = new[] { 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0 };
        var spline = new SplinePrimitive(3, controlPoints, knots);

        // Raster dummy 16x16 PNG bytes
        using var dummyBmp = new SKBitmap(16, 16);
        using var dummyImg = SKImage.FromBitmap(dummyBmp);
        using var dummyData = dummyImg.Encode(SKEncodedImageFormat.Png, 100);
        var pngBytes = dummyData.ToArray();
        var raster = new RasterImagePrimitive(
            referenceId: "DUMMY_IMG_01",
            resolvedPath: null,
            imageBytes: pngBytes,
            imageBounds: new WorldBounds2(300, 300, 350, 350),
            transform: Transform2D.Identity,
            pixelWidth: 16,
            pixelHeight: 16);

        var geomEntity = new RenderSceneEntity(
            new RenderEntityId("CURVES_01"),
            new WorldBounds2(0, 0, 400, 400),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("SYNTHETIC", "H1", 1),
            new RenderGeometryPrimitive[] { arc, ellipse, poly, spline, raster });

        assembler.AddEntity(geomEntity);
        var scene = assembler.Build();

        var geomCache = new PreparedGeometryCache(32 * 1024 * 1024);
        var resCache = new RenderResourceCache(64 * 1024 * 1024);

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var canvas = surface.Canvas;
        var frameContext = new RenderFrameContext(800, 600, 1.0, EnableOptimization: true);

        // Warmup frame
        var initialCamera = new Camera2D(800, 600, new WorldPoint2(200, 200), 1.0);
        var warmupSnapshot = new RenderSnapshot(
            Scene: scene,
            LayerTable: scene.LayerTable,
            Camera: initialCamera,
            QualityMode: RenderQualityMode.Interaction,
            GeometryCache: geomCache,
            ResourceCache: resCache);

        SkiaScenePainter.DrawFrame(canvas, warmupSnapshot, frameContext);

        var warmTessCount = geomCache.TessellationCount;
        Assert(warmTessCount > 0, "Warmup frame must perform initial tessellations");
        Assert(resCache.RasterDecodeMisses == 1, "Raster image must decode exactly once on first view");

        // 100 pan frames within the same scope (varying camera center, same WUPP)
        for (var i = 0; i < 100; i++)
        {
            var panCamera = new Camera2D(800, 600, new WorldPoint2(200 + (i * 0.5), 200 + (i * 0.2)), 1.0);
            var panSnapshot = new RenderSnapshot(
                Scene: scene,
                LayerTable: scene.LayerTable,
                Camera: panCamera,
                QualityMode: RenderQualityMode.Interaction,
                GeometryCache: geomCache,
                ResourceCache: resCache);

            SkiaScenePainter.DrawFrame(canvas, panSnapshot, frameContext);
        }

        // Assert 0 re-tessellations
        Assert(geomCache.TessellationCount == warmTessCount,
            $"100 pan frames must have ZERO re-tessellations: expected {warmTessCount}, got {geomCache.TessellationCount}");
        // Assert 0 re-decodes
        Assert(resCache.RasterDecodeMisses == 1,
            $"100 pan frames must have ZERO raster re-decodes: expected 1 miss, got {resCache.RasterDecodeMisses}");
        Assert(resCache.RasterDecodeHits >= 100,
            $"Raster cache must hit on every pan frame: hits={resCache.RasterDecodeHits}");
    }

    private static void TestLodBandHysteresisPreventsThrashing()
    {
        // Band for WUPP = 1.0 is log2(1.0) = 0
        var band0 = RenderQualityPolicy.ComputeLodBand(1.0);
        Assert(band0 == 0, $"LOD band for 1.0 should be 0, got {band0}");

        // WUPP shifts by +15%: continuous = log2(1.15) ~= 0.20
        // Diff from 0 is 0.20, which is < 0.5 + 0.20 (0.70). Hysteresis must keep band = 0!
        var bandShiftedSmall = RenderQualityPolicy.ComputeLodBand(1.15, previousLodBand: 0, hysteresis: 0.20);
        Assert(bandShiftedSmall == 0, $"Small zoom variation must preserve LOD band (hysteresis): got {bandShiftedSmall}");

        // WUPP shifts by -15%: continuous = log2(0.85) ~= -0.23
        var bandShiftedNeg = RenderQualityPolicy.ComputeLodBand(0.85, previousLodBand: 0, hysteresis: 0.20);
        Assert(bandShiftedNeg == 0, $"Small negative zoom variation must preserve LOD band: got {bandShiftedNeg}");

        // Large zoom step: WUPP = 2.0 (1 full octave zoom out, continuous = 1.0)
        // Diff from 0 is 1.0, which exceeds 0.70. Band must advance to 1.
        var band2 = RenderQualityPolicy.ComputeLodBand(2.0, previousLodBand: 0, hysteresis: 0.20);
        Assert(band2 == 1, $"Large zoom step must transition LOD band: got {band2}");
    }

    private static void TestCacheBudgetAndLruEviction()
    {
        // Small cache of 2000 bytes
        var cache = new PreparedGeometryCache(maxSizeBytes: 2000);

        var dummyPath = new TessellatedPath(new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(10, 10),
            new WorldPoint2(20, 0)
        }, closed: false, filled: false);

        // Put 100 items into the small cache
        for (var i = 0; i < 100; i++)
        {
            cache.Put(1, $"ITEM_{i:D3}", 0, dummyPath, 0.1, new WorldPoint2(0, 0));
        }

        Assert(cache.CurrentSizeBytes <= cache.MaxSizeBytes,
            $"Cache CurrentSizeBytes ({cache.CurrentSizeBytes}) must remain within MaxSizeBytes ({cache.MaxSizeBytes})");
        Assert(cache.EvictionCount > 0, "Eviction count must be greater than 0");

        // Max 2 LOD levels per primitive test:
        // Adding 3 LODs for the same key must leave at most 2 entries
        cache.Put(1, "MULTI_LOD_TEST", 0, dummyPath, 1.0, new WorldPoint2(0, 0));
        cache.Put(1, "MULTI_LOD_TEST", 1, dummyPath, 0.5, new WorldPoint2(0, 0));
        cache.Put(1, "MULTI_LOD_TEST", 2, dummyPath, 0.25, new WorldPoint2(0, 0));

        // The oldest LOD (0) should have been evicted
        var hasLod0 = cache.TryGet(1, "MULTI_LOD_TEST", 0, 1.0, out var entry0) && entry0?.LodBand == 0;
        var hasLod1 = cache.TryGet(1, "MULTI_LOD_TEST", 1, 0.5, out var entry1) && entry1?.LodBand == 1;
        var hasLod2 = cache.TryGet(1, "MULTI_LOD_TEST", 2, 0.25, out var entry2) && entry2?.LodBand == 2;

        Assert(!hasLod0, "Oldest LOD band (0) must be evicted when adding 3rd LOD");
        Assert(hasLod1 && hasLod2, "Most recent 2 LOD bands (1 and 2) must be preserved");
    }

    private static void TestSessionCloseResetsCacheOwnership()
    {
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "test.dwg");
        var assembler = new RenderSceneAssembler();
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E1"),
            new WorldBounds2(0, 0, 10, 10),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEST", "1", 1)));
        var scene = assembler.Build();
        var layoutMgr = new CadLayoutManager(scene);

        var session = new CadViewerSession(metadata, scene, layoutMgr);

        // Put some items into session caches
        var dummyPath = new TessellatedPath(new[] { new WorldPoint2(0, 0), new WorldPoint2(1, 1) }, false, false);
        session.GeometryCache.Put(1, "PRIM_01", 0, dummyPath, 0.1, new WorldPoint2(0, 0));
        Assert(session.GeometryCache.CurrentSizeBytes > 0, "Geometry cache has items");

        // Dispose session
        session.Dispose();

        Assert(session.GeometryCache.CurrentSizeBytes == 0, "Geometry cache size must be reset to 0 after session close");
        Assert(session.ResourceCache.CurrentRasterBytes == 0, "Resource cache size must be reset to 0 after session close");
    }

    private static void TestInteractionHatchNeverOpacifies()
    {
        // Non-solid hatch with 100 pattern lines
        var loop = new HatchLoop(new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(100, 0),
            new WorldPoint2(100, 100),
            new WorldPoint2(0, 100)
        });

        var patternLines = new List<LinePrimitive>();
        for (var i = 0; i < 100; i++)
        {
            patternLines.Add(new LinePrimitive(new WorldPoint2(0, i), new WorldPoint2(100, i)));
        }

        var hatch = new HatchPrimitive(
            loops: new[] { loop },
            patternName: "ANSI31",
            patternAngleRadians: 0.0,
            patternScale: 1.0,
            islandStyle: HatchIslandStyle.Normal,
            isSolid: false,
            patternLines: patternLines);

        Assert(!hatch.IsSolid, "Pattern hatch must NEVER have IsSolid = true");

        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        var canvas = surface.Canvas;
        var camera = new Camera2D(200, 200, new WorldPoint2(50, 50), 1.0);

        // Render in Interaction mode: should execute cleanly without crashing or changing hatch to solid
        var assembler = new RenderSceneAssembler();
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("HATCH_01"),
            new WorldBounds2(0, 0, 100, 100),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEST", "H1", 1),
            new[] { hatch }));
        var scene = assembler.Build();

        var snapshot = new RenderSnapshot(scene, scene.LayerTable, camera, QualityMode: RenderQualityMode.Interaction);
        var context = new RenderFrameContext(200, 200, 1.0, EnableOptimization: true);

        SkiaScenePainter.DrawFrame(canvas, snapshot, context);
        Assert(!hatch.IsSolid, "Pattern hatch must remain non-solid after interaction paint");
    }

    private static void TestGeometryImmutabilityDuringPan()
    {
        var p0 = new WorldPoint2(10, 20);
        var p1 = new WorldPoint2(50, 80);
        var line = new LinePrimitive(p0, p1);
        var boundsBefore = line.Bounds;

        var entity = new RenderSceneEntity(
            new RenderEntityId("LINE_IMMUTABLE"),
            boundsBefore,
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEST", "L1", 1),
            new[] { line });

        var assembler = new RenderSceneAssembler();
        assembler.AddEntity(entity);
        var scene = assembler.Build();

        var cache = new PreparedGeometryCache();
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        var canvas = surface.Canvas;
        var context = new RenderFrameContext(100, 100, 1.0, EnableOptimization: true);

        for (var i = 0; i < 50; i++)
        {
            var camera = new Camera2D(100, 100, new WorldPoint2(i, i), 1.0);
            var snapshot = new RenderSnapshot(scene, scene.LayerTable, camera, GeometryCache: cache);
            SkiaScenePainter.DrawFrame(canvas, snapshot, context);
        }

        // Assert that line start, end, and entity bounds are bitwise unchanged
        Assert(line.Start.X == p0.X && line.Start.Y == p0.Y, "Primitive start point unchanged");
        Assert(line.End.X == p1.X && line.End.Y == p1.Y, "Primitive end point unchanged");
        Assert(line.Bounds.MinX == boundsBefore.MinX && line.Bounds.MaxX == boundsBefore.MaxX, "Primitive bounds unchanged");
        Assert(entity.Bounds.MinX == boundsBefore.MinX && entity.Bounds.MaxX == boundsBefore.MaxX, "Entity bounds unchanged");
    }

    private static void TestHatchCoverageMissOutsideBounds()
    {
        var cache = new PreparedGeometryCache();

        var coverage = new WorldBounds2(0, 0, 100, 100);
        var lines = new[] { (new WorldPoint2(10, 10), new WorldPoint2(90, 90)) };
        cache.PutHatchCoverage(1, "HATCH_KEY", coverage, lines, lodBand: 0, styleRevision: 1);

        // Query inside coverage
        var insideQuery = new WorldBounds2(10, 10, 50, 50);
        var hit = cache.TryGetHatchCoverage(1, "HATCH_KEY", insideQuery, 0, 1, out var entryInside);
        Assert(hit && entryInside != null, "Query inside coverage bounds must be cache hit");

        // Query outside coverage: [50, 50, 150, 150] extends beyond [0, 100]
        var outsideQuery = new WorldBounds2(50, 50, 150, 150);
        var miss = cache.TryGetHatchCoverage(1, "HATCH_KEY", outsideQuery, 0, 1, out var entryOutside);
        Assert(!miss && entryOutside == null, "Query outside coverage bounds must be cache MISS");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"[PreparedGeometryCacheTests] Assertion failed: {message}");
        }
    }
}
