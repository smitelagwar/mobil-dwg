using System;
using MobilDwg.Core.Documents;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Scheduling;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Rendering.Tests;

public static class CorrectionRegressionsP01ToP09
{
    public record RegressionResult(string Id, bool Passed, string Description, string Details);

    public static List<RegressionResult> RunAll(bool throwOnFailures = false)
    {
        var results = new List<RegressionResult>();

        Console.WriteLine("=== RUNNING CORRECTION REGRESSION TESTS P01 TO P09 ===");

        results.Add(RunTest("P01", "FrameRequestGate First Surface Generation Admission", TestP01_FirstSurfaceGeneration));
        results.Add(RunTest("P02", "FrameRequestGate Concurrent Paint Ticket Guard", TestP02_ConcurrentPaintGuard));
        results.Add(RunTest("P03", "CadViewerSession Zoom Host Frame Clock Arming", TestP03_HostFrameClockArming));
        results.Add(RunTest("P04", "CadViewerSession CameraRevision Increment on Zoom", TestP04_CameraRevisionIncrement));
        results.Add(RunTest("P05", "ViewportInteractionEngine Final Frame Notification on UP", TestP05_FinalFrameNotification));
        results.Add(RunTest("P06", "PreparedGeometryCache Coarse LOD Rejection", TestP06_CoarseLodRejection));
        results.Add(RunTest("P07", "PreparedGeometryCache Hatch Eviction Under Memory Budget", TestP07_HatchBudgetEviction));
        results.Add(RunTest("P08", "RenderResourceCache Raster Bitmap Lifetime Protection", TestP08_RasterLifetimeProtection));
        results.Add(RunTest("P09", "TextLayout Bounds Encompassing Actual SKFont Measure", TestP09_TextLayoutBoundsEncompassing));

        int passed = results.Count(r => r.Passed);
        int failed = results.Count(r => !r.Passed);

        Console.WriteLine($"=== CORRECTION REGRESSIONS P01-P09 SUMMARY: {passed} PASSED, {failed} FAILED ===");

        if (throwOnFailures && failed > 0)
        {
            throw new InvalidOperationException($"CORRECTION_REGRESSIONS_FAILED: {failed}/{results.Count} tests failed. Bugs detected as expected RED regressions.");
        }

        return results;
    }

    private static RegressionResult RunTest(string id, string name, Action testAction)
    {
        try
        {
            testAction();
            Console.WriteLine($"  [PASS] {id}: {name}");
            return new RegressionResult(id, true, name, "PASS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [FAIL] {id}: {name} - {ex.Message}");
            return new RegressionResult(id, false, name, ex.Message);
        }
    }

    /// <summary>
    /// P01: When viewport view is created, surface generation is incremented to 2.
    /// When session connects, gate must admit TryBeginPaint(2) for the initial frame.
    /// In current buggy code: gate is created with generation 1; TryBeginPaint(2) returns null!
    /// </summary>
    public static void TestP01_FirstSurfaceGeneration()
    {
        var gate = new FrameRequestGate();
        gate.RequestFrame();
        var ticket = gate.TryBeginPaint(2);
        if (ticket == null)
        {
            throw new InvalidOperationException($"TryBeginPaint(2) returned null. Gate surface generation mismatch (gate={gate.CurrentSurfaceGeneration}, requested=2). Initial frame rejected.");
        }
    }

    /// <summary>
    /// P02: Two consecutive TryBeginPaint calls on the same generation while the first ticket
    /// is active must NOT both succeed. The second call must return null to prevent race conditions.
    /// In current buggy code: both calls return non-null tickets!
    /// </summary>
    public static void TestP02_ConcurrentPaintGuard()
    {
        var gate = new FrameRequestGate();
        gate.RequestFrame();
        var t1 = gate.TryBeginPaint(1);
        if (t1 == null)
        {
            throw new InvalidOperationException("First TryBeginPaint(1) unexpectedly failed.");
        }

        var t2 = gate.TryBeginPaint(1);
        if (t2 != null)
        {
            throw new InvalidOperationException($"Two concurrent tickets admitted simultaneously (t1={t1.TicketId}, t2={t2.TicketId}). Active paint ticket guard missing.");
        }
    }

    /// <summary>
    /// P03: Calling session.Zoom must allow the host application to arm its frame clock.
    /// In current buggy code: session.Zoom calls _frameGate.RequestFrame(), making gate dirty;
    /// then host's session.FrameGate.RequestFrame() returns false, so host frame clock is not armed!
    /// </summary>
    public static void TestP03_HostFrameClockArming()
    {
        var scene = SampleCadDrawings.CreateArchitecturalPlan();
        using var session = new CadViewerSession(new(CadFormat.Dxf, "AC1015", "test"), scene, new CadLayoutManager(scene));
        session.Zoom(1.25, 300, 400);

        bool armed = session.FrameGate.RequestFrame();
        if (!armed)
        {
            throw new InvalidOperationException("session.FrameGate.RequestFrame() returned false after session.Zoom. Host cannot arm frame clock.");
        }
    }

    /// <summary>
    /// P04: Calling session.Zoom must advance session.CameraRevision so caches and observers detect change.
    /// In current buggy code: session.CameraRevision remains unchanged after Zoom!
    /// </summary>
    public static void TestP04_CameraRevisionIncrement()
    {
        var scene = SampleCadDrawings.CreateArchitecturalPlan();
        using var session = new CadViewerSession(new(CadFormat.Dxf, "AC1015", "test"), scene, new CadLayoutManager(scene));
        var revBefore = session.CameraRevision;
        session.Zoom(1.25, 300, 400);

        if (session.CameraRevision <= revBefore)
        {
            throw new InvalidOperationException($"CameraRevision did not increment on session.Zoom (before={revBefore}, after={session.CameraRevision}).");
        }
    }

    /// <summary>
    /// P05: When gesture ends with PointerAction.Up at the same coordinate as last Move,
    /// engine must notify host (e.g. via CameraChanged or frame event) to render final high-quality frame.
    /// In current buggy code: only InteractionEnded is emitted, CameraChanged is omitted, so production host never paints final frame!
    /// </summary>
    public static void TestP05_FinalFrameNotification()
    {
        var engine = new ViewportInteractionEngine(new ViewportController(new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0), new WorldBounds2(-100, -100, 100, 100)));
        int cameraChanges = 0;
        engine.CameraChanged += _ => cameraChanges++;

        PointerPacket MakePacket(PointerAction a, int x, long time) =>
            new(a, 0, 0, time, new[] { new PointerSample(0, new ScreenPoint2(x, 100)) }, 1);

        engine.ProcessPacket(MakePacket(PointerAction.Down, 100, 0));
        engine.ProcessPacket(MakePacket(PointerAction.Move, 120, 10));
        var changesBeforeUp = cameraChanges;

        engine.ProcessPacket(MakePacket(PointerAction.Up, 120, 20));

        if (cameraChanges <= changesBeforeUp)
        {
            throw new InvalidOperationException($"PointerAction.Up did not emit CameraChanged for final high-quality render frame (changes before={changesBeforeUp}, after={cameraChanges}).");
        }
    }

    /// <summary>
    /// P06: PreparedGeometryCache.TryGet must NOT return a coarse tessellation whose chord error
    /// exceeds the requested maxChordError.
    /// In current buggy code: TryGet returns true and provides coarse geometry (error 1.0) when 0.25 is requested!
    /// </summary>
    public static void TestP06_CoarseLodRejection()
    {
        using var geometry = new PreparedGeometryCache();
        geometry.Put(1, "curve", 0, new TessellatedPath(new[] { new WorldPoint2(0, 0), new WorldPoint2(1, 1) }, false, false), 1.0, new WorldPoint2(0, 0));

        bool found = geometry.TryGet(1, "curve", 0, 0.25, out var coarse);
        if (found && coarse != null && coarse.MaxChordError > 0.25)
        {
            throw new InvalidOperationException($"Cache returned coarse LOD (MaxChordError={coarse.MaxChordError}) when maxChordError <= 0.25 was requested.");
        }
    }

    /// <summary>
    /// P07: Hatch coverage entries in PreparedGeometryCache must be subject to cache eviction
    /// under memory pressure, keeping CurrentSizeBytes <= MaxSizeBytes.
    /// In current buggy code: eviction excludes hatch entries, so CurrentSizeBytes > MaxSizeBytes!
    /// </summary>
    public static void TestP07_HatchBudgetEviction()
    {
        using var hatch = new PreparedGeometryCache(64);
        hatch.PutHatchCoverage(1, "hatch", new WorldBounds2(0, 0, 1, 1), new[] { (new WorldPoint2(0, 0), new WorldPoint2(1, 1)) }, 0, 1);

        if (hatch.CurrentSizeBytes > hatch.MaxSizeBytes)
        {
            throw new InvalidOperationException($"Hatch coverage exceeded cache budget (CurrentSizeBytes={hatch.CurrentSizeBytes}, MaxSizeBytes={hatch.MaxSizeBytes}). Hatch eviction missing.");
        }
    }

    /// <summary>
    /// P08: RenderResourceCache.PutRaster must not dispose the bitmap instance while the caller/painter
    /// holds reference to draw it.
    /// In current buggy code: PutRaster evicts and disposes the passed-in bitmap, zeroing its native handle!
    /// </summary>
    public static void TestP08_RasterLifetimeProtection()
    {
        using var raster = new RenderResourceCache(1);
        var bitmap = new SKBitmap(10, 10);
        raster.PutRaster("image", bitmap);

        if (bitmap.Handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("PutRaster prematurely disposed the passed-in SKBitmap (bitmap.Handle is IntPtr.Zero).");
        }
    }

    /// <summary>
    /// P09: TextLayout TotalWidth for a given text and textHeight must not be smaller than
    /// actual measured font width using SKFont.MeasureText.
    /// In current buggy code: SKFont.MeasureText('WWWW') is greater than TextLayout.TotalWidth!
    /// </summary>
    public static void TestP09_TextLayoutBoundsEncompassing()
    {
        using var font = new SKFont(SKTypeface.Default, 100);
        var actualWidth = font.MeasureText("WWWW");
        var layout = new TextLayout("WWWW", new WorldPoint2(0, 0), 100);

        if (actualWidth > layout.TotalWidth)
        {
            throw new InvalidOperationException($"TextLayout TotalWidth ({layout.TotalWidth:F2}) is smaller than actual SKFont measured width ({actualWidth:F2}). Text bounds underestimated.");
        }
    }
}
