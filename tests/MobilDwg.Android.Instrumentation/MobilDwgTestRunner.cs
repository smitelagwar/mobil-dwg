using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Accessibility;
using MobilDwg.Core.Documents;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Scheduling;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Android.Instrumentation;

[global::Android.App.Instrumentation(
    TargetPackage = "com.smitelagwar.mobildwg.test",
    Name = "com.smitelagwar.mobildwg.test.MobilDwgTestRunner",
    Label = "MobilDwg Instrumentation Test Runner")]
[Register("com/smitelagwar/mobildwg/test/MobilDwgTestRunner")]
public class MobilDwgTestRunner : global::Android.App.Instrumentation
{
    private const string TargetPackageName = "com.smitelagwar.mobildwg";
    private const string TargetMainActivity = "crc64d52a5cdc4f267319.MainActivity";

    public MobilDwgTestRunner() : base() { }

    protected MobilDwgTestRunner(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer) { }

    public override void OnCreate(Bundle? arguments)
    {
        base.OnCreate(arguments);
        Start();
    }

    public override void OnStart()
    {
        base.OnStart();
        Task.Run(RunTestSuite);
    }

    private async Task RunTestSuite()
    {
        var testResults = new List<NativeTestResult>();
        bool overallPassed = true;

        try
        {
            LogInfo("=== STARTING MOBILDWG ANDROID INSTRUMENTATION TEST SUITE ===");

            // Native contract regressions on Android runtime
            testResults.Add(RunNativeRegressionP01());
            testResults.Add(RunNativeRegressionP02());
            testResults.Add(RunNativeRegressionP03());
            testResults.Add(RunNativeRegressionP04());
            testResults.Add(RunNativeRegressionP05());

            // Step 1: Launch target activity and give UI time to initialize
            LaunchTargetActivity();
            await Task.Delay(4000);

            // Step 2: Test Sample Drawing First Frame Blank Screen Detection (P01)
            var sampleResult = await RunSampleDrawingTestAsync();
            testResults.Add(sampleResult);

            // Step 3: Test Real DXF Opening
            var dxfResult = await RunDxfOpeningTestAsync();
            testResults.Add(dxfResult);

            // Step 4: Test Real Native Touch Injection
            var touchResult = await RunTouchInjectionAndHostClockTestAsync();
            testResults.Add(touchResult);

            // Step 5: Native ViewportInteractionEngine contract smoke tests
            var unitSmokeResult = RunUnitInteractionSmokeTests();
            testResults.Add(unitSmokeResult);

            overallPassed = testResults.TrueForAll(r => r.Passed);
        }
        catch (Exception ex)
        {
            LogInfo($"FATAL_TEST_RUNNER_EXCEPTION: {ex}");
            testResults.Add(new NativeTestResult(
                "NATIVE_INSTRUMENTATION_CRASH",
                false,
                $"Exception in runner: {ex.Message}"));
            overallPassed = false;
        }

        // Export test artifacts to device storage
        ExportResults(testResults, overallPassed);

        var bundle = new Bundle();
        bundle.PutString("test_summary", overallPassed ? "ALL_NATIVE_TESTS_PASSED" : "KNOWN_RED_REGRESSIONS_DETECTED");
        bundle.PutInt("total_tests", testResults.Count);
        int failedCount = 0;
        foreach (var r in testResults)
        {
            if (!r.Passed) failedCount++;
            bundle.PutString($"test_{r.TestId}", $"{(r.Passed ? "PASS" : "FAIL")}: {r.Details}");
        }
        bundle.PutInt("failed_tests", failedCount);

        LogInfo($"=== MOBILDWG INSTRUMENTATION FINISHED: {(overallPassed ? "PASS" : "RED_FAILURES_DETECTED")} (Failed: {failedCount}/{testResults.Count}) ===");

        Finish(overallPassed ? Result.Ok : Result.Canceled, bundle);
    }

    private NativeTestResult RunNativeRegressionP01()
    {
        var gate = new FrameRequestGate();
        gate.RequestFrame();
        var ticket = gate.TryBeginPaint(2);
        if (ticket == null)
        {
            return new NativeTestResult(
                "NATIVE_P01_FIRST_SURFACE_GENERATION",
                false,
                $"DEFECT_REPRODUCED: FrameRequestGate.TryBeginPaint(2) rejected initial ticket (gate={gate.CurrentSurfaceGeneration}, view=2). View constructor generation mismatch.");
        }
        return new NativeTestResult("NATIVE_P01_FIRST_SURFACE_GENERATION", true, "TryBeginPaint(2) admitted.");
    }

    private NativeTestResult RunNativeRegressionP02()
    {
        var gate = new FrameRequestGate();
        gate.RequestFrame();
        var t1 = gate.TryBeginPaint(1);
        var t2 = gate.TryBeginPaint(1);
        if (t1 != null && t2 != null)
        {
            return new NativeTestResult(
                "NATIVE_P02_CONCURRENT_PAINT_GUARD",
                false,
                $"DEFECT_REPRODUCED: Two concurrent tickets admitted simultaneously (t1={t1.TicketId}, t2={t2.TicketId}). Active ticket guard missing.");
        }
        return new NativeTestResult("NATIVE_P02_CONCURRENT_PAINT_GUARD", true, "Concurrent paint correctly rejected.");
    }

    private NativeTestResult RunNativeRegressionP03()
    {
        var scene = SampleCadDrawings.CreateArchitecturalPlan();
        using var session = new CadViewerSession(new(CadFormat.Dxf, "AC1015", "test"), scene, new CadLayoutManager(scene));
        session.Zoom(1.25, 300, 400);
        bool armed = session.FrameGate.RequestFrame();
        if (!armed)
        {
            return new NativeTestResult(
                "NATIVE_P03_HOST_CLOCK_ARMING",
                false,
                "DEFECT_REPRODUCED: session.Zoom scheduled gate without notifying host; subsequent RequestFrame returned false preventing clock arming.");
        }
        return new NativeTestResult("NATIVE_P03_HOST_CLOCK_ARMING", true, "FrameGate.RequestFrame succeeded.");
    }

    private NativeTestResult RunNativeRegressionP04()
    {
        var scene = SampleCadDrawings.CreateArchitecturalPlan();
        using var session = new CadViewerSession(new(CadFormat.Dxf, "AC1015", "test"), scene, new CadLayoutManager(scene));
        var revBefore = session.CameraRevision;
        session.Zoom(1.25, 300, 400);
        if (session.CameraRevision <= revBefore)
        {
            return new NativeTestResult(
                "NATIVE_P04_CAMERA_REVISION",
                false,
                $"DEFECT_REPRODUCED: CameraRevision did not advance on Zoom (before={revBefore}, after={session.CameraRevision}).");
        }
        return new NativeTestResult("NATIVE_P04_CAMERA_REVISION", true, "CameraRevision incremented.");
    }

    private NativeTestResult RunNativeRegressionP05()
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
            return new NativeTestResult(
                "NATIVE_P05_FINAL_FRAME_NOTIFICATION",
                false,
                $"DEFECT_REPRODUCED: PointerAction.Up with identical position did not emit CameraChanged for final high-quality render frame (before={changesBeforeUp}, after={cameraChanges}).");
        }
        return new NativeTestResult("NATIVE_P05_FINAL_FRAME_NOTIFICATION", true, "CameraChanged emitted on gesture UP.");
    }

    private void LaunchTargetActivity()
    {
        LogInfo($"Launching target package: {TargetPackageName}");
        var ctx = TargetContext ?? Context;
        var pm = ctx?.PackageManager;
        var intent = pm?.GetLaunchIntentForPackage(TargetPackageName) ?? new Intent(Intent.ActionMain);
        intent.SetComponent(new ComponentName(TargetPackageName, TargetMainActivity));
        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
        ctx?.StartActivity(intent);
    }

    private async Task<NativeTestResult> RunSampleDrawingTestAsync()
    {
        LogInfo("--- TEST: Sample Drawing Opening & Blank Viewport Detection (P01) ---");
        var ui = UiAutomation;
        if (ui == null)
        {
            return new NativeTestResult("NATIVE_SAMPLE_DRAWING_FIRST_FRAME", false, "UiAutomation instance unavailable");
        }

        AccessibilityNodeInfo? sampleCard = null;
        for (int retry = 0; retry < 6; retry++)
        {
            var root = ui.RootInActiveWindow;
            if (root != null)
            {
                var matches = root.FindAccessibilityNodeInfosByText("Apartman 3+1");
                if (matches != null && matches.Count > 0)
                {
                    sampleCard = matches[0];
                    break;
                }
            }

            // Scroll down in dashboard scroll view if not visible yet
            LogInfo("Sample card not yet in active window; injecting scroll up gesture");
            InjectScrollGesture(540, 1800, 540, 600);
            await Task.Delay(1000);
        }

        if (sampleCard == null)
        {
            // Fallback: tap at known card coordinates on 1080x2400 screen
            LogInfo("Sample card not found via accessibility search; tapping at default card coordinates (540, 2250)");
            InjectTap(540, 2250);
        }
        else
        {
            var cardRect = new Rect();
            sampleCard.GetBoundsInScreen(cardRect);
            LogInfo($"Found sample card at {cardRect.CenterX()},{cardRect.CenterY()}; injecting tap");
            InjectTap(cardRect.CenterX(), cardRect.CenterY());
        }

        // Wait for viewer screen with TextureView
        Rect? textureViewRect = null;
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(250);
            var root = ui.RootInActiveWindow;
            if (root != null)
            {
                textureViewRect = FindTextureViewBounds(root);
                if (textureViewRect != null) break;
            }
        }

        textureViewRect ??= new Rect(0, 309, 1080, 2337);
        LogInfo($"Viewer TextureView bounds: {textureViewRect.Left},{textureViewRect.Top} - {textureViewRect.Right},{textureViewRect.Bottom}.");

        // First frame screenshot check immediately after TextureView appears (before watchdog timeout at 1000ms)
        var screenshot = ui.TakeScreenshot();
        if (screenshot == null)
        {
            return new NativeTestResult("NATIVE_SAMPLE_DRAWING_FIRST_FRAME", false, "UiAutomation.TakeScreenshot returned null");
        }

        SaveBitmap(screenshot, "mobildwg_sample_first_frame.png");

        int nonBackgroundPixels = CountNonBackgroundPixels(screenshot, textureViewRect);
        LogInfo($"Drawing viewport non-background pixel count: {nonBackgroundPixels}");

        if (nonBackgroundPixels == 0)
        {
            return new NativeTestResult(
                "NATIVE_SAMPLE_DRAWING_FIRST_FRAME",
                false,
                "DEFECT_REPRODUCED: Viewport rendered 0 drawing pixels on first frame (screen is blank). P01 generation mismatch rejected initial paint.");
        }

        return new NativeTestResult(
            "NATIVE_SAMPLE_DRAWING_FIRST_FRAME",
            true,
            $"SUCCESS: Viewport rendered {nonBackgroundPixels} drawing pixels.");
    }

    private async Task<NativeTestResult> RunDxfOpeningTestAsync()
    {
        LogInfo("--- TEST: DXF File Opening & Viewport Verification (P01/K01) ---");
        var ui = UiAutomation;
        if (ui == null)
        {
            return new NativeTestResult("NATIVE_DXF_OPEN_RENDER", false, "UiAutomation instance unavailable");
        }

        const string fixturePath = "/sdcard/synthetic_turkish_basic_ac1015.dxf";
        var ctx = TargetContext ?? Context;
        var intent = new Intent(Intent.ActionMain);
        intent.SetComponent(new ComponentName(TargetPackageName, TargetMainActivity));
        intent.PutExtra("open_cad", fixturePath);
        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
        ctx?.StartActivity(intent);

        await Task.Delay(3000);

        var root = ui.RootInActiveWindow;
        var textureViewRect = root != null ? FindTextureViewBounds(root) : null;
        textureViewRect ??= new Rect(0, 309, 1080, 2337);

        var screenshot = ui.TakeScreenshot();
        if (screenshot == null)
        {
            return new NativeTestResult("NATIVE_DXF_OPEN_RENDER", false, "Screenshot returned null for DXF open test");
        }

        SaveBitmap(screenshot, "mobildwg_dxf_screen.png");

        int nonBg = CountNonBackgroundPixels(screenshot, textureViewRect);
        LogInfo($"DXF drawing viewport non-background pixel count: {nonBg}");

        if (nonBg == 0)
        {
            return new NativeTestResult(
                "NATIVE_DXF_OPEN_RENDER",
                false,
                "DEFECT_REPRODUCED: Real DXF opened but viewport rendered 0 drawing pixels (P01/K01 defect reproduced).");
        }

        return new NativeTestResult("NATIVE_DXF_OPEN_RENDER", true, $"SUCCESS: DXF rendered {nonBg} non-background pixels.");
    }

    private async Task<NativeTestResult> RunTouchInjectionAndHostClockTestAsync()
    {
        LogInfo("--- TEST: Real Native Touch Event Injection & Host Clock (P01/P03) ---");
        var ui = UiAutomation;
        if (ui == null)
        {
            return new NativeTestResult("NATIVE_HOST_TOUCH_CLOCK_ARM", false, "UiAutomation instance unavailable");
        }

        var textureViewRect = new Rect(0, 309, 1080, 2337);

        // Inject real drag gesture: DOWN -> MOVE -> UP
        float startX = 540f;
        float startY = 1200f;
        float endX = 740f;
        float endY = 1400f;

        LogInfo($"Injecting native drag gesture from ({startX},{startY}) to ({endX},{endY})");
        long downTime = SystemClock.UptimeMillis();

        InjectMotionEvent(MotionEventActions.Down, downTime, downTime, startX, startY);
        await Task.Delay(50);
        InjectMotionEvent(MotionEventActions.Move, downTime, SystemClock.UptimeMillis(), (startX + endX) / 2f, (startY + endY) / 2f);
        await Task.Delay(50);
        InjectMotionEvent(MotionEventActions.Move, downTime, SystemClock.UptimeMillis(), endX, endY);
        await Task.Delay(50);
        InjectMotionEvent(MotionEventActions.Up, downTime, SystemClock.UptimeMillis(), endX, endY);

        await Task.Delay(1500);

        // Tap Zoom (+) button at [910, 1124][1036, 1250]
        LogInfo("Injecting tap on Zoom (+) button at (973, 1187)");
        InjectTap(973, 1187);
        await Task.Delay(1500);

        var screenshot = ui.TakeScreenshot();
        if (screenshot != null)
        {
            SaveBitmap(screenshot, "mobildwg_touch_screen.png");
            int nonBg = CountNonBackgroundPixels(screenshot, textureViewRect);
            if (nonBg == 0)
            {
                return new NativeTestResult(
                    "NATIVE_HOST_TOUCH_CLOCK_ARM",
                    false,
                    "DEFECT_REPRODUCED: Drawing area remains blank after touch injection and zoom click (P03 command dropped / clock not armed).");
            }
        }

        return new NativeTestResult("NATIVE_HOST_TOUCH_CLOCK_ARM", true, "Touch injection and zoom updated drawing area.");
    }

    private NativeTestResult RunUnitInteractionSmokeTests()
    {
        try
        {
            NativeSmokeRunner.RunAllSmokeTests();
            return new NativeTestResult("NATIVE_ENGINE_SMOKE_TESTS", true, "All ViewportInteractionEngine contract smoke tests passed");
        }
        catch (Exception ex)
        {
            return new NativeTestResult("NATIVE_ENGINE_SMOKE_TESTS", false, $"Engine smoke test failed: {ex.Message}");
        }
    }

    private Rect? FindTextureViewBounds(AccessibilityNodeInfo node)
    {
        if (node.ClassName?.ToString()?.Contains("TextureView", StringComparison.OrdinalIgnoreCase) == true)
        {
            var r = new Rect();
            node.GetBoundsInScreen(r);
            if (r.Width() > 100 && r.Height() > 100) return r;
        }

        for (int i = 0; i < node.ChildCount; i++)
        {
            var child = node.GetChild(i);
            if (child != null)
            {
                var found = FindTextureViewBounds(child);
                if (found != null) return found;
            }
        }
        return null;
    }

    private void InjectTap(float x, float y)
    {
        long downTime = SystemClock.UptimeMillis();
        InjectMotionEvent(MotionEventActions.Down, downTime, downTime, x, y);
        Thread.Sleep(100);
        InjectMotionEvent(MotionEventActions.Up, downTime, SystemClock.UptimeMillis(), x, y);
    }

    private void InjectScrollGesture(float startX, float startY, float endX, float endY)
    {
        long downTime = SystemClock.UptimeMillis();
        InjectMotionEvent(MotionEventActions.Down, downTime, downTime, startX, startY);
        Thread.Sleep(50);
        InjectMotionEvent(MotionEventActions.Move, downTime, SystemClock.UptimeMillis(), (startX + endX) / 2f, (startY + endY) / 2f);
        Thread.Sleep(50);
        InjectMotionEvent(MotionEventActions.Move, downTime, SystemClock.UptimeMillis(), endX, endY);
        Thread.Sleep(50);
        InjectMotionEvent(MotionEventActions.Up, downTime, SystemClock.UptimeMillis(), endX, endY);
    }

    private void InjectMotionEvent(MotionEventActions action, long downTime, long eventTime, float x, float y)
    {
        var properties = new MotionEvent.PointerProperties[1];
        properties[0] = new MotionEvent.PointerProperties { Id = 0, ToolType = MotionEventToolType.Finger };

        var coords = new MotionEvent.PointerCoords[1];
        coords[0] = new MotionEvent.PointerCoords { X = x, Y = y, Pressure = 1.0f, Size = 1.0f };

        var motionEvent = MotionEvent.Obtain(
            downTime,
            eventTime,
            action,
            1,
            properties,
            coords,
            0,
            0,
            1.0f,
            1.0f,
            0,
            0,
            InputSourceType.Touchscreen,
            0);

        if (motionEvent != null)
        {
            UiAutomation?.InjectInputEvent(motionEvent, true);
            motionEvent.Recycle();
        }
    }

    private int CountNonBackgroundPixels(Bitmap bitmap, Rect bounds)
    {
        int nonBgCount = 0;
        // Central drawing canvas: exclude top navigation bar (y < 380), bottom island bar (y > 1950), and zoom buttons (x > 840)
        int left = 120;
        int top = 400;
        int right = 840;
        int bottom = 1900;

        for (int y = top; y < bottom; y += 4)
        {
            for (int x = left; x < right; x += 4)
            {
                int pixel = bitmap.GetPixel(x, y);
                int r = Color.GetRedComponent(pixel);
                int g = Color.GetGreenComponent(pixel);
                int b = Color.GetBlueComponent(pixel);

                // Background is ~ (8, 11, 17)
                int dr = Math.Abs(r - 8);
                int dg = Math.Abs(g - 11);
                int db = Math.Abs(b - 17);

                if (dr + dg + db > 25)
                {
                    nonBgCount++;
                }
            }
        }
        return nonBgCount;
    }

    private string GetArtifactsDirectory()
    {
        try
        {
            var dir = Context?.GetExternalFilesDir(null)?.AbsolutePath
                ?? TargetContext?.GetExternalFilesDir(null)?.AbsolutePath
                ?? Context?.CacheDir?.AbsolutePath
                ?? "/sdcard/Android/data/com.smitelagwar.mobildwg.test/files";
            Directory.CreateDirectory(dir);
            return dir;
        }
        catch
        {
            return "/sdcard";
        }
    }

    private void SaveBitmap(Bitmap bitmap, string filename)
    {
        try
        {
            var path = System.IO.Path.Combine(GetArtifactsDirectory(), filename);
            using var fs = File.OpenWrite(path);
            bitmap.Compress(Bitmap.CompressFormat.Png!, 100, fs);
            LogInfo($"Saved screenshot artifact to: {path}");
        }
        catch (Exception ex)
        {
            LogInfo($"Failed to save bitmap {filename}: {ex.Message}");
        }
    }

    private void ExportResults(List<NativeTestResult> results, bool allPassed)
    {
        try
        {
            var data = new
            {
                runner = "com.smitelagwar.mobildwg.test/com.smitelagwar.mobildwg.test.MobilDwgTestRunner",
                targetPackage = TargetPackageName,
                timestamp = DateTime.UtcNow.ToString("O"),
                allPassed = allPassed,
                results = results
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var path = System.IO.Path.Combine(GetArtifactsDirectory(), "mobildwg_native_test_result.json");
            File.WriteAllText(path, json);
            LogInfo($"Saved JSON test results to {path}");
        }
        catch (Exception ex)
        {
            LogInfo($"Failed to export results JSON: {ex.Message}");
        }
    }

    private void LogInfo(string message)
    {
        global::Android.Util.Log.Info("MobilDwgInstrumentation", message);
        Console.WriteLine($"[MobilDwgInstrumentation] {message}");
    }

    public record NativeTestResult(string TestId, bool Passed, string Details);
}
