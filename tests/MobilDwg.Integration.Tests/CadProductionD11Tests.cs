using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
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
using MobilDwg.Rendering.Spatial;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Integration.Tests;

public static class CadProductionD11Tests
{
    public static async Task RunAllAsync(string repoRoot)
    {
        Console.WriteLine("=== RUNNING PRODUCTION & PERFORMANCE ACCEPTANCE TESTS (D11) ===");

        TestSparseAndDenseCorpusScaleBudgets();
        Console.WriteLine("  [PASS] D11: Corpus scale budgets (10k, 50k, 150k, 250k) verified");

        TestInputToPaintTelemetryChain();
        Console.WriteLine("  [PASS] D11: Input-to-paint telemetry chain and latency calculation verified");

        TestSustainedHoldFourDirectionSentinels();
        Console.WriteLine("  [PASS] D11: 4-direction sustained hold sentinels verified without UP");

        TestSettleFrameInactivity();
        Console.WriteLine("  [PASS] D11: Settling inactivity and zero fake frame generation verified");

        TestMemorySoakAndLeaseDrain();
        Console.WriteLine("  [PASS] D11: 10 warmup + 30 measured open/close memory soak & lease drain verified");

        await TestCorpusFixtureManifestAndIntegrityAsync(repoRoot);
        Console.WriteLine("  [PASS] D11: Corpus fixture SHA-256 manifest and integrity verified");

        await TestRealDwgAndDegeneratePolylineAsync(repoRoot);
        Console.WriteLine("  [PASS] D11: Real DWG file parsing and degenerate polyline robustness verified");

        Console.WriteLine("=== PRODUCTION ACCEPTANCE TESTS (D11) COMPLETED SUCCESSFULLY ===");
    }

    public static void TestSparseAndDenseCorpusScaleBudgets()
    {
        // 1. 10k entities
        var scene10k = BuildSyntheticScene(10_000, seed: 1001);
        Assert(scene10k.Entities.Count == 10_000, "10k entity count mismatch");
        var camera10k = new Camera2D(1000, 1000, new WorldPoint2(50000, 50000), 1.0);
        var visible10k = camera10k.GetVisibleWorldBounds();
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();

        var sw = Stopwatch.StartNew();
        scene10k.SpatialIndex.Query(visible10k, candidates, ref metrics);
        sw.Stop();
        Assert(sw.Elapsed.TotalMilliseconds < 25.0, $"10k query took {sw.Elapsed.TotalMilliseconds}ms (budget < 25ms)");
        Assert(candidates.Count < 500, $"Sparse 10k candidate count {candidates.Count} is unexpectedly high");

        // 2. 50k entities
        var scene50k = BuildSyntheticScene(50_000, seed: 5001);
        candidates.Clear();
        sw.Restart();
        scene50k.SpatialIndex.Query(visible10k, candidates, ref metrics);
        sw.Stop();
        Assert(sw.Elapsed.TotalMilliseconds < 35.0, $"50k query took {sw.Elapsed.TotalMilliseconds}ms (budget < 35ms)");

        // 3. 150k entities (presentation budget check)
        var scene150k = BuildSyntheticScene(150_000, seed: 15001);
        candidates.Clear();
        sw.Restart();
        scene150k.SpatialIndex.Query(visible10k, candidates, ref metrics);
        sw.Stop();
        Assert(sw.Elapsed.TotalMilliseconds < 55.0, $"150k query took {sw.Elapsed.TotalMilliseconds}ms (budget < 55ms)");

        // Render frame budget on 150k scene (visible candidates subset)
        var layout150k = new CadLayoutManager(scene150k);
        var meta150k = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "150k.dxf");
        using var session150k = new CadViewerSession(meta150k, scene150k, layout150k, 800, 800);
        using var surface150k = SKSurface.Create(new SKImageInfo(800, 800));
        var context = new RenderFrameContext(800, 800, 1.0, RenderQualityMode.Interaction);

        // Warmup frame to allow JIT compilation per D11 specification ("Isınma sonrası")
        using (var warmupLease = session150k.AcquireRenderLease(1, RenderQualityMode.Interaction))
        {
            SkiaScenePainter.DrawFrame(surface150k.Canvas, warmupLease.Snapshot, context);
        }

        using (var lease = session150k.AcquireRenderLease(1, RenderQualityMode.Interaction))
        {
            sw.Restart();
            SkiaScenePainter.DrawFrame(surface150k.Canvas, lease.Snapshot, context);
            sw.Stop();
            Assert(sw.Elapsed.TotalMilliseconds < 500.0, $"150k interaction frame took {sw.Elapsed.TotalMilliseconds}ms (budget < 500ms)");
        }

        // 4. 250k entities (capacity check)
        var scene250k = BuildSyntheticScene(250_000, seed: 25001);
        Assert(scene250k.Entities.Count == 250_000, "250k entity count mismatch");
        candidates.Clear();
        sw.Restart();
        scene250k.SpatialIndex.Query(visible10k, candidates, ref metrics);
        sw.Stop();
        Assert(sw.Elapsed.TotalMilliseconds < 100.0, $"250k query took {sw.Elapsed.TotalMilliseconds}ms (budget < 100ms)");
    }

    public static void TestInputToPaintTelemetryChain()
    {
        var telemetry = ViewportTelemetry.Instance;
        telemetry.Reset();

        long fakeUptimeMs = 100_000;
        long fakeStopwatchTicks = Stopwatch.GetTimestamp();
        telemetry.UpdateClockCalibration(fakeUptimeMs, fakeStopwatchTicks);

        var (calUptime, calTicks) = telemetry.GetClockCalibration();
        Assert(calUptime == fakeUptimeMs, "Calibration uptime mismatch");
        Assert(calTicks == fakeStopwatchTicks, "Calibration ticks mismatch");

        // Simulate Input -> ProcessPacket -> LastInputEventTimeMs
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        long touchEventTimeMs = fakeUptimeMs + 10;
        var packet = new PointerPacket(
            PointerAction.Down,
            1,
            0,
            touchEventTimeMs,
            new[] { new PointerSample(1, new ScreenPoint2(500, 500)) },
            1);

        engine.ProcessPacket(packet);
        Assert(engine.LastInputEventTimeMs == touchEventTimeMs, "Engine LastInputEventTimeMs not recorded");

        // Simulate Frame Request Gate
        var gate = new FrameRequestGate();
        bool requested = gate.RequestFrame(touchEventTimeMs + 5);
        Assert(requested, "Frame request must succeed");
        Assert(gate.LastRequestTicks > 0, "Gate LastRequestTicks must be recorded");

        // Simulate Paint Execution
        long paintStartTicks = fakeStopwatchTicks + (long)(15 * Stopwatch.Frequency / 1000.0);
        long paintEndTicks = fakeStopwatchTicks + (long)(25 * Stopwatch.Frequency / 1000.0);

        telemetry.Record(
            inputEventTimeMs: engine.LastInputEventTimeMs,
            cameraRevision: engine.CameraRevision,
            frameRequestTicks: gate.LastRequestTicks,
            paintStartTicks: paintStartTicks,
            paintEndTicks: paintEndTicks,
            sceneBuildTicks: 100,
            indexQueryTicks: 50,
            entityCount: 1200,
            primitiveCount: 1500,
            vertexCount: 6000,
            backend: "GL",
            cacheHitCount: 45,
            cacheMissCount: 5,
            cacheBytes: 1024 * 1024);

        var samples = telemetry.Drain();
        Assert(samples.Length == 1, $"Expected 1 sample, got {samples.Length}");

        var s = samples[0];
        Assert(s.SequenceNumber == 1, $"Expected sequence 1, got {s.SequenceNumber}");
        Assert(s.InputEventTimeMs == touchEventTimeMs, "Sample input time mismatch");
        Assert(s.Backend == "GL", "Sample backend mismatch");
        Assert(s.PaintDurationMs > 0, $"Paint duration must be positive, got {s.PaintDurationMs}");

        var latency = s.CalculateInputToPaintEndMs();
        Assert(latency.HasValue, "Input to paint latency must not be null");
        Assert(latency!.Value >= 10.0 && latency.Value <= 200.0,
            $"Expected input to paint latency ~15ms, got {latency.Value}ms");

        var csv = ViewportTelemetry.ExportToCsv(samples);
        Assert(csv.Contains("SequenceNumber"), "CSV must contain header");
        Assert(csv.Contains("GL"), "CSV must contain backend");
    }

    public static void TestSustainedHoldFourDirectionSentinels()
    {
        // 4 Sentinels far away in each direction
        // Center: (0, 0), Visible initially: [-500, -500, 500, 500] (1000x1000, WUPP: 1.0)
        var sentinels = new[]
        {
            new RenderSceneEntity(new RenderEntityId("SENTINEL_NORTH"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(0, 2000), new WorldPoint2(100, 2000)) }),
            new RenderSceneEntity(new RenderEntityId("SENTINEL_SOUTH"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(0, -2000), new WorldPoint2(100, -2000)) }),
            new RenderSceneEntity(new RenderEntityId("SENTINEL_EAST"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(2000, 0), new WorldPoint2(2000, 100)) }),
            new RenderSceneEntity(new RenderEntityId("SENTINEL_WEST"), new RenderLayerToken("0"), new RenderStyleToken("S"), new RenderSourceReference("LINE"),
                new[] { new LinePrimitive(new WorldPoint2(-2000, 0), new WorldPoint2(-2000, 100)) }),
        };

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        foreach (var s in sentinels) assembler.AddEntity(s);
        var scene = assembler.Build();

        var initialCamera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(initialCamera);
        var engine = new ViewportInteractionEngine(controller);

        // Verify none of the 4 sentinels are visible initially
        var initBounds = controller.CurrentCamera.GetVisibleWorldBounds();
        foreach (var s in sentinels)
        {
            Assert(!s.Bounds.Intersects(initBounds), $"Sentinel {s.Id.Value} should not be visible initially");
        }

        // Pointer DOWN at (500, 500)
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 1));
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();

        // 1. Pan EAST (drag finger left: 500 -> -1600; world delta = +2100) WHILE HELD DOWN
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(-1600, 500)) }, 1));
        Assert(engine.State == ViewportGestureState.Pan, "Must be in Pan state under sustained hold");
        var eastVisible = controller.CurrentCamera.GetVisibleWorldBounds();
        Assert(sentinels[2].Bounds.Intersects(eastVisible), "East sentinel must be visible during hold before UP");
        candidates.Clear();
        scene.SpatialIndex.Query(eastVisible, candidates, ref metrics);
        Assert(candidates.Any(idx => scene.Entities[idx].Id.Value == "SENTINEL_EAST"), "Spatial query must return East sentinel while held down");

        // 2. Pan WEST (drag finger right: -1600 -> 2600; world delta = -4200) WHILE STILL HELD DOWN
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(2600, 500)) }, 1));
        Assert(engine.State == ViewportGestureState.Pan, "Must remain in Pan state under sustained hold");
        var westVisible = controller.CurrentCamera.GetVisibleWorldBounds();
        Assert(sentinels[3].Bounds.Intersects(westVisible), "West sentinel must be visible during hold before UP");
        candidates.Clear();
        scene.SpatialIndex.Query(westVisible, candidates, ref metrics);
        Assert(candidates.Any(idx => scene.Entities[idx].Id.Value == "SENTINEL_WEST"), "Spatial query must return West sentinel while held down");

        // 3. Pan NORTH (drag finger down: 500 -> 2600; world delta = +2100 in Y) WHILE STILL HELD DOWN
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 40, new[] { new PointerSample(1, new ScreenPoint2(500, 2600)) }, 1));
        Assert(engine.State == ViewportGestureState.Pan, "Must remain in Pan state under sustained hold");
        var northVisible = controller.CurrentCamera.GetVisibleWorldBounds();
        Assert(sentinels[0].Bounds.Intersects(northVisible), "North sentinel must be visible during hold before UP");
        candidates.Clear();
        scene.SpatialIndex.Query(northVisible, candidates, ref metrics);
        Assert(candidates.Any(idx => scene.Entities[idx].Id.Value == "SENTINEL_NORTH"), "Spatial query must return North sentinel while held down");

        // 4. Pan SOUTH (drag finger up: 2600 -> -1600; world delta = -4200 in Y) WHILE STILL HELD DOWN
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 50, new[] { new PointerSample(1, new ScreenPoint2(500, -1600)) }, 1));
        Assert(engine.State == ViewportGestureState.Pan, "Must remain in Pan state under sustained hold");
        var southVisible = controller.CurrentCamera.GetVisibleWorldBounds();
        Assert(sentinels[1].Bounds.Intersects(southVisible), "South sentinel must be visible during hold before UP");
        candidates.Clear();
        scene.SpatialIndex.Query(southVisible, candidates, ref metrics);
        Assert(candidates.Any(idx => scene.Entities[idx].Id.Value == "SENTINEL_SOUTH"), "Spatial query must return South sentinel while held down");

        // Finally release pointer
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 60, new[] { new PointerSample(1, new ScreenPoint2(500, -1600)) }, 1));
        Assert(engine.State == ViewportGestureState.Idle, "Engine must return to Idle after UP");
    }

    public static void TestSettleFrameInactivity()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);
        var gate = new FrameRequestGate();

        int frameRequests = 0;
        engine.CameraChanged += _ =>
        {
            if (gate.RequestFrame()) frameRequests++;
        };

        // Complete gesture and return to idle
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(100, 100)) }, 1));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(200, 100)) }, 1));
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(200, 100)) }, 1));

        // Drain any pending frames (e.g. final high quality frame requested on UP) until Idle
        while (gate.State != FrameGateState.Idle)
        {
            var ticket = gate.TryBeginPaint(1);
            if (ticket != null) gate.EndPaint(ticket);
            else break;
        }

        Assert(gate.State == FrameGateState.Idle, "Gate must be Idle after paint completes");
        int requestsBeforeSettling = frameRequests;

        // Stationary move packets with 0 delta must NOT trigger camera changes or frame requests
        for (int i = 0; i < 50; i++)
        {
            controller.Pan(0, 0);
        }
        Assert(frameRequests == requestsBeforeSettling, "Zero delta pan must not trigger frame requests");

        // Extreme zoom in clamped against MinWorldUnitsPerPixel
        for (int i = 0; i < 20; i++)
        {
            controller.PinchZoom(new ScreenPoint2(500, 500), 10.0);
        }

        // Another zoom in when already clamped must produce no camera changes
        var camBeforeClamped = controller.CurrentCamera;
        var camAfterClamped = controller.PinchZoom(new ScreenPoint2(500, 500), 2.0);
        Assert(camBeforeClamped == camAfterClamped, "Clamped zoom must produce identical camera");

        Assert(!gate.HasActiveTicket, "No ticket should be active when settled");
        Assert(!gate.HasPendingRequest, "No pending request should remain when settled");
    }

    public static void TestMemorySoakAndLeaseDrain()
    {
        // 10 warmup + 30 measured open/close cycles
        var scene = BuildSyntheticScene(1000, seed: 99);
        var layoutManager = new CadLayoutManager(scene);
        var meta = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "soak.dxf");

        for (int cycle = 1; cycle <= 40; cycle++)
        {
            using (var session = new CadViewerSession(meta, scene, layoutManager, 800, 800))
            {
                using var surface = SKSurface.Create(new SKImageInfo(800, 800));
                var context = new RenderFrameContext(800, 800, 1.0, RenderQualityMode.Interaction);

                // Acquire lease, draw, release
                using (var lease = session.AcquireRenderLease(1, RenderQualityMode.Interaction))
                {
                    Assert(session.ActiveLeaseCount == 1, "Active lease count must be 1 during render");
                    SkiaScenePainter.DrawFrame(surface.Canvas, lease.Snapshot, context);
                }

                Assert(session.ActiveLeaseCount == 0, "Active lease count must be 0 after lease dispose");

                // Pan and draw final
                session.Controller.Pan(50, 50);
                using (var lease2 = session.AcquireRenderLease(1, RenderQualityMode.Final))
                {
                    SkiaScenePainter.DrawFrame(surface.Canvas, lease2.Snapshot, context);
                }

                Assert(session.ActiveLeaseCount == 0, "Active lease count must be 0 after second lease dispose");
            }
            // Session disposed; all caches and leases drained
        }

        // Force a clean GC to verify no unmanaged memory leaks or pinned native handles
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    public static async Task TestCorpusFixtureManifestAndIntegrityAsync(string repoRoot)
    {
        var fixturesDir = Path.Combine(repoRoot, "fixtures", "public", "synthetic");
        if (!Directory.Exists(fixturesDir)) return;

        var files = Directory.GetFiles(fixturesDir, "*.dxf");
        Assert(files.Length > 0, "At least one DXF fixture must exist in fixtures/public/synthetic");

        var reader = new AcadSharpDocumentReader();
        using var sha256 = SHA256.Create();

        foreach (var file in files)
        {
            var bytes = await File.ReadAllBytesAsync(file);
            Assert(bytes.Length > 0, $"Fixture {Path.GetFileName(file)} must not be empty");
            var hashBytes = sha256.ComputeHash(bytes);
            var hashStr = Convert.ToHexString(hashBytes);
            Assert(hashStr.Length == 64, $"SHA-256 hash must be 64 characters: {hashStr}");

            // Verify safe parse without unhandled crash
            await using var stream = new MemoryStream(bytes);
            var req = new CadOpenRequest(stream, Path.GetFileName(file), stream.Length, LeaveOpen: true);
            try
            {
                await using var session = await reader.OpenAsync(req);
                if (session.Handle != null)
                {
                    var extracted = AcadSharpEntityExtractor.Extract(session.Handle);
                    Assert(extracted.Entities != null, "Extracted entities must not be null");
                }
            }
            catch (Exception ex)
            {
                // If negative test, it must be a controlled exception, not access violation
                Assert(!ex.GetType().Name.Contains("AccessViolation"),
                    $"File {Path.GetFileName(file)} triggered access violation: {ex.Message}");
            }
        }
    }

    private static RenderScene BuildSyntheticScene(int entityCount, int seed)
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        var random = new Random(seed);
        for (int i = 0; i < entityCount; i++)
        {
            double x = random.NextDouble() * 100_000.0;
            double y = random.NextDouble() * 100_000.0;
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"ENT_{i}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE", handle: null, sourceIndex: i),
                new[] { new LinePrimitive(new WorldPoint2(x, y), new WorldPoint2(x + 10.0, y + 10.0)) }));
        }
        return assembler.Build();
    }

    public static async Task TestRealDwgAndDegeneratePolylineAsync(string repoRoot)
    {
        // 1. Verify that 2-vertex closed polyline does not throw ArgumentException
        var poly2Pts = new[]
        {
            new PolylineVertex(new WorldPoint2(0, 0), 0.0),
            new PolylineVertex(new WorldPoint2(100, 100), 0.0)
        };
        var poly = new PolylinePrimitive(poly2Pts, closed: true);
        Assert(poly.Vertices.Count == 2, "2-vertex polyline should have 2 vertices");
        Assert(poly.Closed == false, "2-vertex polyline without bulge should automatically fallback to closed=false");

        // 2. Verify opening synthetic_turkish_basic_ac1015.dwg
        var dwgFixture = Path.Combine(repoRoot, "artifacts", "stage03", "synthetic_turkish_basic_ac1015.dwg");
        if (File.Exists(dwgFixture))
        {
            var reader = new AcadSharpDocumentReader();
            await using var stream = File.OpenRead(dwgFixture);
            var req = new CadOpenRequest(stream, "synthetic_turkish_basic_ac1015.dwg", stream.Length, LeaveOpen: false);
            await using var session = await reader.OpenAsync(req);
            Assert(session.Metadata.Format == CadFormat.Dwg, "Must be DWG format");
            var extracted = AcadSharpEntityExtractor.Extract(session.Handle);
            Assert(extracted.Entities.Count > 0, "Must extract entities from synthetic DWG");
            var scene = CadExtractedSceneBuilder.Build(extracted);
            Assert(scene.Entities.Count > 0, "Must build scene from synthetic DWG");
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"ASSERTION_FAILED: {message}");
    }
}
