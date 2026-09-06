using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Performance;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Performance;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Scheduling;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Spatial;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Rendering.Tests;

public static class ViewerPerformanceTests
{
    [ModuleInitializer]
    public static void Run()
    {
        TestTouchFidelityCameraInvariants();
        TestSentinelRenderingBeforeUp();
        TestSparseAndDenseCorpusBudgets();
        TestResidentGeometryCacheWarmPan();
        TestFrameSchedulingAndIdleAccuracy();

        Console.WriteLine("STAGE13_VIEWER_PERFORMANCE_TESTS_PASS");
        Console.WriteLine("STAGE13_TOUCH_FIDELITY_FRAME_BUDGETS_PASS");
    }

    private static void TestTouchFidelityCameraInvariants()
    {
        // 1. Pan drift bounds: 100 consecutive pan steps forward and backward
        var initialCamera = new Camera2D(1080, 2400, new WorldPoint2(100.0, 200.0), 0.5);
        var controller = new ViewportController(initialCamera);
        var engine = new ViewportInteractionEngine(controller);

        // Down at 500, 1000
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(500, 1000)) }, 0));

        // Pan +10px then -10px 100 times
        for (int i = 0; i < 100; i++)
        {
            engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20 + (i * 2), new[] { new PointerSample(1, new ScreenPoint2(510, 1000)) }, 0));
            engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 21 + (i * 2), new[] { new PointerSample(1, new ScreenPoint2(500, 1000)) }, 0));
        }

        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 300, new[] { new PointerSample(1, new ScreenPoint2(500, 1000)) }, 0));

        // Camera center must have returned to exact initial center with near-zero drift (< 1e-9)
        AssertNear(100.0, controller.CurrentCamera.Center.X, 1e-9, "Pan back-and-forth Center X drift");
        AssertNear(200.0, controller.CurrentCamera.Center.Y, 1e-9, "Pan back-and-forth Center Y drift");
        Assert(!double.IsNaN(controller.CurrentCamera.Center.X) && !double.IsInfinity(controller.CurrentCamera.Center.X), "Center X is finite");
        Assert(!double.IsNaN(controller.CurrentCamera.Center.Y) && !double.IsInfinity(controller.CurrentCamera.Center.Y), "Center Y is finite");

        // 2. Pinch focus preservation: screen pivot must preserve its world location across zoom
        var pivotScreen = new ScreenPoint2(540, 1200);
        var worldBefore = CameraTransform.ScreenToWorld(pivotScreen, controller.CurrentCamera);

        // Zoom by factor 2 around pivot
        controller.PinchZoom(pivotScreen, 2.0);
        var worldAfter = CameraTransform.ScreenToWorld(pivotScreen, controller.CurrentCamera);

        AssertNear(worldBefore.X, worldAfter.X, 1e-9, "Zoom pivot World X invariant");
        AssertNear(worldBefore.Y, worldAfter.Y, 1e-9, "Zoom pivot World Y invariant");
        AssertNear(0.25, controller.CurrentCamera.WorldUnitsPerPixel, 1e-9, "WUPP halved after 2x zoom");

        // 3. Finite numbers under extreme scale operations
        controller.PinchZoom(pivotScreen, 1e15);
        Assert(double.IsFinite(controller.CurrentCamera.WorldUnitsPerPixel), "WUPP must remain finite even with extreme zoom");
        Assert(controller.CurrentCamera.WorldUnitsPerPixel >= controller.CurrentCamera.MinWorldUnitsPerPixel, "WUPP clamped to min");

        controller.PinchZoom(pivotScreen, 1e-15);
        Assert(double.IsFinite(controller.CurrentCamera.WorldUnitsPerPixel), "WUPP must remain finite with extreme zoom out");
        Assert(controller.CurrentCamera.WorldUnitsPerPixel <= controller.CurrentCamera.MaxWorldUnitsPerPixel, "WUPP clamped to max");
    }

    private static void TestSentinelRenderingBeforeUp()
    {
        // Place sentinel entities outside the initial viewport in 4 directions: North, South, East, West
        // Initial viewport: 1000x1000, Center: (500, 500), WUPP: 1.0 -> visible: [0, 0, 1000, 1000]
        var sentinels = new[]
        {
            new RenderSceneEntity(new RenderEntityId("SENTINEL_NORTH"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(500, 2000), new WorldPoint2(600, 2000)) }),
            new RenderSceneEntity(new RenderEntityId("SENTINEL_SOUTH"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(500, -1000), new WorldPoint2(600, -1000)) }),
            new RenderSceneEntity(new RenderEntityId("SENTINEL_EAST"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(2000, 500), new WorldPoint2(2000, 600)) }),
            new RenderSceneEntity(new RenderEntityId("SENTINEL_WEST"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(-1000, 500), new WorldPoint2(-1000, 600)) }),
        };

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        foreach (var s in sentinels) assembler.AddEntity(s);
        var scene = assembler.Build();

        var initialCamera = new Camera2D(1000, 1000, new WorldPoint2(500, 500), 1.0);
        var controller = new ViewportController(initialCamera);
        var engine = new ViewportInteractionEngine(controller);

        // Initial visible bounds: none of the sentinels should intersect visible bounds
        var initialVisible = controller.CurrentCamera.GetVisibleWorldBounds();
        foreach (var s in sentinels)
        {
            Assert(!s.Bounds.Intersects(initialVisible), $"Sentinel {s.Id.Value} must not be visible initially");
        }

        // Pan toward East sentinel: finger down at (500, 500) and move left by 1500px to reveal East sentinel (world delta = +1500)
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(-1000, 500)) }, 0));

        // POINTER IS STILL DOWN! Check that camera has moved and East sentinel is now within visible bounds
        Assert(engine.State == ViewportGestureState.Pan, "Pointer must be down in Pan state");
        var movedVisible = controller.CurrentCamera.GetVisibleWorldBounds();
        Assert(sentinels[2].Bounds.Intersects(movedVisible), "East sentinel must be visible DURING MOVE before UP packet");

        // Query spatial index to confirm sentinel is fetched during pan before UP
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();
        scene.SpatialIndex.Query(movedVisible, candidates, ref metrics);
        var foundSentinel = candidates.Any(idx => scene.Entities[idx].Id.Value == "SENTINEL_EAST");
        Assert(foundSentinel, "Sentinel East must be queried from spatial index before UP");

        // Release pointer (UP)
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(-1000, 500)) }, 0));
        Assert(engine.State == ViewportGestureState.Idle, "Engine must return to Idle after UP");
    }

    private static void TestSparseAndDenseCorpusBudgets()
    {
        // 1. Sparse corpus (10,000 entities)
        var sparseAssembler = new RenderSceneAssembler(RenderColorContext.Dark);
        var random = new Random(42);
        for (int i = 0; i < 10000; i++)
        {
            double x = random.NextDouble() * 100000;
            double y = random.NextDouble() * 100000;
            sparseAssembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"S_{i}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(x, y), new WorldPoint2(x + 10, y + 10)) }));
        }
        var sparseScene = sparseAssembler.Build();

        // Narrow 1000x1000 viewport in sparse scene (should cover only ~0.01% of area)
        var camera = new Camera2D(1000, 1000, new WorldPoint2(50000, 50000), 1.0);
        var visible = camera.GetVisibleWorldBounds();

        var sw = Stopwatch.StartNew();
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();
        sparseScene.SpatialIndex.Query(visible, candidates, ref metrics);
        sw.Stop();

        Assert(sw.Elapsed.TotalMilliseconds < 10.0, $"Sparse spatial query took {sw.Elapsed.TotalMilliseconds} ms, expected < 10ms");
        Assert(candidates.Count < 100, $"Sparse visible count {candidates.Count} should be small");

        // 2. Dense Fit Viewport render timing test
        var denseAssembler = new RenderSceneAssembler(RenderColorContext.Dark);
        for (int i = 0; i < 2000; i++)
        {
            double x = (i % 50) * 20.0;
            double y = (i / 50) * 20.0;
            denseAssembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"D_{i}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(x, y), new WorldPoint2(x + 15, y + 15)) }));
        }
        var denseScene = denseAssembler.Build();
        var denseLayout = new CadLayoutManager(denseScene);
        var denseMeta = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "Dense.dxf");
        using var session = new CadViewerSession(denseMeta, denseScene, denseLayout, 800, 800);
        using var surface = SKSurface.Create(new SKImageInfo(800, 800));
        var frameContext = new RenderFrameContext(800, 800, 1.0, EnableOptimization: true);

        // Interaction LOD vs Final Refine timing
        using (var lease = session.AcquireRenderLease(1, RenderQualityMode.Interaction))
        {
            sw.Restart();
            SkiaScenePainter.DrawFrame(surface.Canvas, lease.Snapshot, frameContext);
            sw.Stop();
            Assert(sw.Elapsed.TotalMilliseconds < 50.0, $"Interaction frame took {sw.Elapsed.TotalMilliseconds} ms, budget < 50ms");
        }

        // Final refine timing
        using (var lease = session.AcquireRenderLease(1, RenderQualityMode.Final))
        {
            sw.Restart();
            SkiaScenePainter.DrawFrame(surface.Canvas, lease.Snapshot, frameContext);
            sw.Stop();
            Assert(sw.Elapsed.TotalMilliseconds < 100.0, $"Final refine frame took {sw.Elapsed.TotalMilliseconds} ms, budget < 100ms");
        }
    }

    private static void TestResidentGeometryCacheWarmPan()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        var knots = new[] { 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0 };
        for (int i = 0; i < 100; i++)
        {
            var pts = new[]
            {
                new WorldPoint2(i * 10, 0),
                new WorldPoint2((i * 10) + 2, 10),
                new WorldPoint2((i * 10) + 6, 10),
                new WorldPoint2((i * 10) + 8, 0)
            };
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"P_{i}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("SPLINE"),
                new[] { new SplinePrimitive(3, pts, knots) }));
        }
        var scene = assembler.Build();
        var layoutManager = new CadLayoutManager(scene);
        var meta = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "Pan.dxf");
        using var session = new CadViewerSession(meta, scene, layoutManager, 800, 800);
        using var surface = SKSurface.Create(new SKImageInfo(800, 800));
        var frameContext = new RenderFrameContext(800, 800, 1.0, EnableOptimization: true);

        // Frame 1: Cold render - populates cache
        using (var lease1 = session.AcquireRenderLease(1, RenderQualityMode.Final))
        {
            SkiaScenePainter.DrawFrame(surface.Canvas, lease1.Snapshot, frameContext);
        }

        var coldTessCount = session.GeometryCache.TessellationCount;
        Assert(coldTessCount > 0, "Cold frame must populate geometry cache");

        // Frame 2: Small pan within resident geometry
        session.Controller.Pan(-20, 0);

        using (var lease2 = session.AcquireRenderLease(1, RenderQualityMode.Final))
        {
            SkiaScenePainter.DrawFrame(surface.Canvas, lease2.Snapshot, frameContext);
        }

        var warmTessCount = session.GeometryCache.TessellationCount;

        // Warm pan should register zero re-tessellations for resident geometry
        Assert(warmTessCount == coldTessCount, $"Warm pan must have 0 re-tessellations: expected {coldTessCount}, got {warmTessCount}");
        Assert(session.GeometryCache.CurrentSizeBytes <= session.GeometryCache.MaxSizeBytes, "Cache must not exceed max size");
    }

    private static void TestFrameSchedulingAndIdleAccuracy()
    {
        var camera = new Camera2D(800, 800, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);
        var gate = new FrameRequestGate();

        // 1. In Idle state, no unsolicited frame requests
        Assert(engine.State == ViewportGestureState.Idle, "Engine is Idle");
        Assert(!gate.HasPendingRequest, "No frame request in Idle");

        // 2. Rapid input bursts: bounded request gate (max 1 pending request)
        for (int i = 0; i < 50; i++)
        {
            gate.RequestFrame(1);
        }
        Assert(gate.HasPendingRequest, "Gate has pending request");
        var ticket = gate.TryBeginPaint(1);
        Assert(ticket != null, "BeginPaint returns active ticket");
        gate.EndPaint(ticket!);
        Assert(!gate.HasPendingRequest, "Pending request cleared after complete");

        // 3. Active paint serialization: lease limits in-flight rendering
        var meta = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "Idle.dxf");
        var scene = new RenderSceneAssembler(RenderColorContext.Dark).Build();
        var layoutManager = new CadLayoutManager(scene);
        using var session = new CadViewerSession(meta, scene, layoutManager, 800, 800);

        Assert(session.ActiveLeaseCount == 0, "Initial active lease count 0");
        using (var lease = session.AcquireRenderLease(1))
        {
            Assert(session.ActiveLeaseCount == 1, "Active lease count exactly 1 during paint");
        }
        Assert(session.ActiveLeaseCount == 0, "Active lease count returned to 0 after lease disposed");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"STAGE 13 VIEWER PERF TEST ASSERTION FAILED: {message}");
        }
    }

    private static void AssertNear(double expected, double actual, double tolerance, string message)
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException($"{message}. Expected: {expected:R}, Actual: {actual:R}");
        }
    }
}
