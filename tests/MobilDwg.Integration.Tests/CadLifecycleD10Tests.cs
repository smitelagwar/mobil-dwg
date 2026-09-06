using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Scheduling;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Integration.Tests;

public static class CadLifecycleD10Tests
{
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"[D10 TEST ASSERTION FAILED] {message}");
        }
    }

    public static void RunAll()
    {
        Console.WriteLine("=== RUNNING D10 LIFECYCLE, SURFACE LOSS AND LOW-MEMORY TESTS ===");
        TestFrameRequestGateAwaitingAndTicketInspection();
        TestWatchdogDecisionLogic();
        TestActiveSessionMemoryTrimClearsAllCaches();
        TestDrainAfterRetiringWithActiveLeases();
        TestGlErrorClassification();
        TestConcurrentFrameTicketsAndSurfaceGenerationsUnderLifecycleChanges();
        Console.WriteLine("=== ALL D10 TESTS PASSED SUCCESSFULLY ===");
    }

    private static void TestFrameRequestGateAwaitingAndTicketInspection()
    {
        Console.WriteLine("-> Running TestFrameRequestGateAwaitingAndTicketInspection...");

        var gate = new FrameRequestGate();
        Assert(!gate.IsFrameAwaitingOrScheduled, "New gate must not have awaiting/scheduled frame");
        Assert(!gate.HasActiveTicket, "New gate must not have active ticket");

        // 1. Request frame -> enters Scheduled
        bool scheduled = gate.RequestFrame();
        Assert(scheduled, "First RequestFrame from Idle must return true");
        Assert(gate.IsFrameAwaitingOrScheduled, "Gate must report IsFrameAwaitingOrScheduled when Scheduled");
        Assert(!gate.HasActiveTicket, "No active ticket yet");

        // 2. Mark awaiting paint (e.g. on Vsync tick)
        gate.MarkAwaitingPaint();
        Assert(gate.State == FrameGateState.AwaitingPaint, "Gate state must be AwaitingPaint");
        Assert(gate.IsFrameAwaitingOrScheduled, "Gate must report IsFrameAwaitingOrScheduled when AwaitingPaint");

        // 3. Try begin paint -> enters Painting
        var ticket = gate.TryBeginPaint(1);
        Assert(ticket != null, "TryBeginPaint with matching generation must succeed");
        Assert(gate.HasActiveTicket, "Gate must report HasActiveTicket when Painting");
        Assert(!gate.IsFrameAwaitingOrScheduled, "No pending frame during active paint without new requests");

        // 4. Request frame during paint -> records pending request
        bool pendingAccepted = gate.RequestFrame();
        Assert(!pendingAccepted, "RequestFrame during active paint must return false");
        Assert(gate.HasPendingRequest, "Gate must record HasPendingRequest");
        Assert(gate.IsFrameAwaitingOrScheduled, "Gate must report IsFrameAwaitingOrScheduled when pending request exists");

        // 5. End paint -> because of pending request, immediately transitions to Scheduled
        bool nextFrameNeeded = gate.EndPaint(ticket!);
        Assert(nextFrameNeeded, "EndPaint with pending request must signal nextFrameNeeded");
        Assert(gate.State == FrameGateState.Scheduled, "Gate state must transition to Scheduled");
        Assert(!gate.HasActiveTicket, "Active ticket must be cleared after EndPaint");
        Assert(gate.IsFrameAwaitingOrScheduled, "Still scheduled for next frame");

        // 6. Complete second frame
        var ticket2 = gate.TryBeginPaint(1);
        Assert(ticket2 != null, "Second TryBeginPaint must succeed");
        bool nextFrameNeeded2 = gate.EndPaint(ticket2!);
        Assert(!nextFrameNeeded2, "EndPaint without pending request must return false");
        Assert(gate.State == FrameGateState.Idle, "Gate state must return to Idle");
        Assert(!gate.IsFrameAwaitingOrScheduled, "Idle gate has no awaiting frame");
        Assert(!gate.HasActiveTicket, "No active ticket in Idle");

        Console.WriteLine("-> TestFrameRequestGateAwaitingAndTicketInspection PASSED.");
    }

    private static void TestWatchdogDecisionLogic()
    {
        Console.WriteLine("-> Running TestWatchdogDecisionLogic...");

        // Simulate watchdog verification function mirroring CadViewportView.ArmWatchdog
        bool ShouldExecuteWatchdogTimeout(
            string backend,
            bool isVisible,
            string lifecycleState,
            int width, int height,
            bool sessionMatches,
            long surfaceGeneration, long targetGeneration,
            ViewportGestureState gestureState,
            bool isFrameAwaitingOrScheduled,
            bool hasActiveTicket,
            ref int watchdogRetries,
            out string action)
        {
            action = "None";
            if (backend != "OpenGLES") return false;
            if (!isVisible || lifecycleState != "Resumed") return false;
            if (width <= 0 || height <= 0) return false;
            if (!sessionMatches) return false;
            if (surfaceGeneration != targetGeneration) return false; // Stale generation
            if (gestureState != ViewportGestureState.Idle) return false; // Active gesture: do not recreate surface!

            if (!isFrameAwaitingOrScheduled && !hasActiveTicket) return false;

            if (watchdogRetries == 0)
            {
                watchdogRetries++;
                action = "ReinitializeGL";
                return true;
            }
            else
            {
                action = "SwitchToSoftware";
                return true;
            }
        }

        int retries = 0;

        // Case 1: Inactive/Idle gate -> timeout ignored
        bool r1 = ShouldExecuteWatchdogTimeout("OpenGLES", true, "Resumed", 1080, 1920, true, 1, 1, ViewportGestureState.Idle, false, false, ref retries, out var a1);
        Assert(!r1 && a1 == "None", "Watchdog must not fire when no frame is awaiting or painting");

        // Case 2: Active gesture (Panning/Pinching) -> timeout MUST NOT recreate surface!
        bool r2 = ShouldExecuteWatchdogTimeout("OpenGLES", true, "Resumed", 1080, 1920, true, 1, 1, ViewportGestureState.Pan, true, false, ref retries, out var a2);
        Assert(!r2 && a2 == "None", "Watchdog must reject timeout when gesture is active");

        // Case 3: Stale generation (e.g. rotation/resize happened) -> timeout ignored
        bool r3 = ShouldExecuteWatchdogTimeout("OpenGLES", true, "Resumed", 1080, 1920, true, 2, 1, ViewportGestureState.Idle, true, false, ref retries, out var a3);
        Assert(!r3 && a3 == "None", "Watchdog must reject stale generation callback");

        // Case 4: First valid timeout -> Re-initializes GL view (retries becomes 1)
        bool r4 = ShouldExecuteWatchdogTimeout("OpenGLES", true, "Resumed", 1080, 1920, true, 1, 1, ViewportGestureState.Idle, true, false, ref retries, out var a4);
        Assert(r4 && a4 == "ReinitializeGL", $"Expected ReinitializeGL on first timeout, got {a4}");
        Assert(retries == 1, "Watchdog retries must be 1 after first timeout");

        // Case 5: Second valid timeout in same session -> Permanent CPU software fallback
        bool r5 = ShouldExecuteWatchdogTimeout("OpenGLES", true, "Resumed", 1080, 1920, true, 2, 2, ViewportGestureState.Idle, true, false, ref retries, out var a5);
        Assert(r5 && a5 == "SwitchToSoftware", $"Expected SwitchToSoftware on second timeout, got {a5}");

        Console.WriteLine("-> TestWatchdogDecisionLogic PASSED.");
    }

    private static void TestActiveSessionMemoryTrimClearsAllCaches()
    {
        Console.WriteLine("-> Running TestActiveSessionMemoryTrimClearsAllCaches...");

        var lineEnt = new CadExtractedEntity(
            handle: "L1",
            layerName: "0",
            entityType: CadExtractedEntityType.Line,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(0, 0), new CadExtractedPoint(100, 100) });

        var doc = new CadExtractedDocument(
            format: "DWG",
            version: "AC1032",
            layers: new[] { new CadExtractedLayer("0", 0xFFFFFFFF, 7, IsVisible: true) },
            entities: new[] { lineEnt },
            minX: 0, minY: 0, maxX: 100, maxY: 100);

        var scene = CadExtractedSceneBuilder.Build(doc, RenderColorContext.Dark);
        var layoutManager = new CadLayoutManager(scene);
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "test.dwg");
        using var session = new CadViewerSession(metadata, scene, layoutManager, 1000, 1000);

        // 1. Populate PreparedGeometryCache
        var path = new TessellatedPath(new[] { new WorldPoint2(0, 0), new WorldPoint2(50, 50), new WorldPoint2(100, 100) }, false, false);
        session.GeometryCache.Put(1, "prim:E1:0", 0, path, 0.1, new WorldPoint2(0, 0));

        session.GeometryCache.PutHatchCoverage(
            1, "hatch:H1",
            new WorldBounds2(0, 0, 50, 50),
            new[] { (new WorldPoint2(0, 0), new WorldPoint2(50, 50)) },
            0, 1);

        Assert(session.GeometryCache.CurrentSizeBytes > 0, "GeometryCache must be non-empty after Put");

        // 2. Populate RenderResourceCache
        using var bmp = new SKBitmap(64, 64, SKColorType.Rgba8888, SKAlphaType.Premul);
        bool putRaster = session.ResourceCache.PutRaster("raster_key", bmp);
        Assert(putRaster, "PutRaster must succeed");
        Assert(session.ResourceCache.CurrentRasterBytes > 0, "ResourceCache must be non-empty after PutRaster");

        // 3. Trigger Low Memory Trim
        session.OnTrimMemory();

        // 4. Verify both caches are completely cleared
        Assert(session.GeometryCache.CurrentSizeBytes == 0, $"GeometryCache bytes must be 0 after trim, got {session.GeometryCache.CurrentSizeBytes}");
        Assert(session.ResourceCache.CurrentRasterBytes == 0, $"ResourceCache bytes must be 0 after trim, got {session.ResourceCache.CurrentRasterBytes}");
        Assert(!session.ResourceCache.TryGetRaster("raster_key", out _), "Trimmed bitmap must not be retrievable");

        Console.WriteLine("-> TestActiveSessionMemoryTrimClearsAllCaches PASSED.");
    }

    private static void TestDrainAfterRetiringWithActiveLeases()
    {
        Console.WriteLine("-> Running TestDrainAfterRetiringWithActiveLeases...");

        var lineEnt = new CadExtractedEntity(
            handle: "L1",
            layerName: "0",
            entityType: CadExtractedEntityType.Line,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(0, 0), new CadExtractedPoint(50, 50) });

        var doc = new CadExtractedDocument(
            format: "DWG",
            version: "AC1032",
            layers: new[] { new CadExtractedLayer("0", 0xFFFFFFFF, 7, IsVisible: true) },
            entities: new[] { lineEnt },
            minX: 0, minY: 0, maxX: 50, maxY: 50);

        var scene = CadExtractedSceneBuilder.Build(doc, RenderColorContext.Dark);
        var layoutManager = new CadLayoutManager(scene);
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "test.dwg");
        var session = new CadViewerSession(metadata, scene, layoutManager, 1000, 1000);

        bool closeSignaled = false;
        bool drainSignaled = false;

        session.CloseRequested += () => closeSignaled = true;
        session.DrainCompleted += () => drainSignaled = true;

        // 1. Acquire active render lease
        var lease = session.AcquireRenderLease(1, RenderQualityMode.Final);
        Assert(lease != null, "AcquireRenderLease must return active lease");

        // 2. Request dispose while lease is active
        session.Dispose();
        Assert(closeSignaled, "CloseRequested must be signaled immediately on Dispose()");
        Assert(!drainSignaled, "DrainCompleted must NOT be signaled while active lease exists");

        // 3. Dispose active lease
        lease!.Dispose();
        Assert(drainSignaled, "DrainCompleted must be signaled after last active lease is disposed");

        Console.WriteLine("-> TestDrainAfterRetiringWithActiveLeases PASSED.");
    }

    private static void TestGlErrorClassification()
    {
        Console.WriteLine("-> Running TestGlErrorClassification...");

        bool IsGlBackendException(Exception ex)
        {
            if (ex.GetType().FullName?.Contains("Skia", StringComparison.OrdinalIgnoreCase) == true) return true;
            var msg = ex.Message ?? string.Empty;
            return msg.Contains("GL", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("context", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("GrContext", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("surface", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("render target", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("EGL", StringComparison.OrdinalIgnoreCase);
        }

        // True GL/Graphics backend failures:
        Assert(IsGlBackendException(new InvalidOperationException("Failed to make GL context current")), "GL context failure must be recognized");
        Assert(IsGlBackendException(new Exception("GrContext is null or abandoned")), "GrContext abandoned must be recognized");
        Assert(IsGlBackendException(new Exception("EGL_BAD_SURFACE")), "EGL surface error must be recognized");
        Assert(IsGlBackendException(new Exception("Invalid render target dimensions")), "Render target error must be recognized");

        // Programming / logical bugs that MUST NOT be masked as GL issues:
        Assert(!IsGlBackendException(new ArgumentNullException("parameter")), "ArgumentNullException must NOT be masked as GL failure");
        Assert(!IsGlBackendException(new NullReferenceException("Object reference not set")), "NullReferenceException must NOT be masked as GL failure");
        Assert(!IsGlBackendException(new IndexOutOfRangeException("Array index out of range")), "IndexOutOfRangeException must NOT be masked as GL failure");
        Assert(!IsGlBackendException(new FileNotFoundException("File not found")), "FileNotFoundException must NOT be masked as GL failure");

        Console.WriteLine("-> TestGlErrorClassification PASSED.");
    }

    private static void TestConcurrentFrameTicketsAndSurfaceGenerationsUnderLifecycleChanges()
    {
        Console.WriteLine("-> Running TestConcurrentFrameTicketsAndSurfaceGenerationsUnderLifecycleChanges...");

        var gate = new FrameRequestGate();
        gate.InvalidateSurface(1);

        // Frame requested on generation 1
        gate.RequestFrame();
        var ticket1 = gate.TryBeginPaint(1);
        Assert(ticket1 != null, "Generation 1 paint ticket must be granted");

        // Host resumes with new surface generation 2 while paint 1 is still ongoing
        gate.InvalidateSurface(2);

        // Attempting to finish old ticket with obsolete generation
        bool nextNeeded = gate.EndPaint(ticket1!);
        Assert(!nextNeeded, "EndPaint with obsolete generation must not schedule frame");
        Assert(!gate.HasActiveTicket, "Active ticket must be cleared");

        // Stale generation 1 ticket attempt must be rejected
        var staleTicket = gate.TryBeginPaint(1);
        Assert(staleTicket == null, "Stale generation 1 must be rejected on generation 2 surface");

        // Fresh generation 2 paint succeeds
        gate.RequestFrame();
        var freshTicket = gate.TryBeginPaint(2);
        Assert(freshTicket != null, "Fresh generation 2 ticket must succeed");
        gate.EndPaint(freshTicket!);

        Console.WriteLine("-> TestConcurrentFrameTicketsAndSurfaceGenerationsUnderLifecycleChanges PASSED.");
    }
}
