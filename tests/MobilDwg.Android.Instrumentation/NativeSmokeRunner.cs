using System;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Android.Instrumentation;

public static class NativeSmokeRunner
{
    public static void RunAllSmokeTests()
    {
        TestNativePanSmoke();
        TestNativePinchSmoke();
        TestNativeOneTwoOneSmoke();
        TestNativeCancelSmoke();
        TestNativeSentinelBeforeUpSmoke();
        TestNativeGlCpuSwitchSmoke();
        TestNativeResizeSmoke();
        Console.WriteLine("STAGE05_NATIVE_INSTRUMENTATION_PASS");
    }

    private static void TestNativePanSmoke()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(500, 1000)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(600, 1000)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(600, 1000)) }, 0));

        AssertNear(-100.0, controller.CurrentCamera.Center.X, 1e-9, "Pan smoke delta mismatch");
    }

    private static void TestNativePinchSmoke()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        var p1 = new PointerSample(1, new ScreenPoint2(400, 1000));
        var p2 = new PointerSample(2, new ScreenPoint2(600, 1000));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { p1 }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerDown, 2, 1, 20, new[] { p1, p2 }, 0));

        var p1M = new PointerSample(1, new ScreenPoint2(300, 1000));
        var p2M = new PointerSample(2, new ScreenPoint2(700, 1000));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 30, new[] { p1M, p2M }, 0));

        AssertNear(0.5, controller.CurrentCamera.WorldUnitsPerPixel, 1e-9, "Pinch smoke 2x factor mismatch");
    }

    private static void TestNativeOneTwoOneSmoke()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        var p1 = new PointerSample(1, new ScreenPoint2(500, 1000));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { p1 }, 0));

        var p2 = new PointerSample(2, new ScreenPoint2(700, 1000));
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerDown, 2, 1, 20, new[] { p1, p2 }, 0));

        // Lift p2
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerUp, 2, 1, 30, new[] { p1 }, 0));
        Assert(engine.State == ViewportGestureState.Pan, "Must return to Pan state");

        // Pan with p1
        var p1M = new PointerSample(1, new ScreenPoint2(550, 1000));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 40, new[] { p1M }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 50, new[] { p1M }, 0));

        Assert(engine.State == ViewportGestureState.Idle, "Must return to Idle");
    }

    private static void TestNativeCancelSmoke()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(500, 1000)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Cancel, 1, 0, 20, Array.Empty<PointerSample>(), 0));

        Assert(engine.State == ViewportGestureState.Idle, "Cancel must reset state to Idle");
        Assert(engine.ActivePointerCount == 0, "Cancel must clear active pointers");
    }

    private static void TestNativeSentinelBeforeUpSmoke()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(500, 1000)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(550, 1000)) }, 0));

        var preUpCenter = controller.CurrentCamera.Center.X;
        // UP delivers a final point at 570 (20px additional delta)
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(570, 1000)) }, 0));

        AssertNear(preUpCenter - 20.0, controller.CurrentCamera.Center.X, 1e-9, "Final UP sample delta must be applied");
    }

    private static void TestNativeGlCpuSwitchSmoke()
    {
        var view = new MobilDwg.App.Viewer.CadViewportView();
        Assert(view.CurrentBackend == MobilDwg.App.Viewer.CadViewportBackend.OpenGLES, "Initial backend OpenGLES");

        view.SwitchToSoftware();
        Assert(view.CurrentBackend == MobilDwg.App.Viewer.CadViewportBackend.Software, "Switched backend Software");

        view.SwitchToOpenGLES();
        Assert(view.CurrentBackend == MobilDwg.App.Viewer.CadViewportBackend.OpenGLES, "Switched back to OpenGLES");
    }

    private static void TestNativeResizeSmoke()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(42.0, 99.0), 0.5);
        var controller = new ViewportController(camera);
        controller.Resize(2400, 1080);

        Assert(controller.CurrentCamera.PixelWidth == 2400, "Resized width 2400");
        Assert(controller.CurrentCamera.PixelHeight == 1080, "Resized height 1080");
        AssertNear(42.0, controller.CurrentCamera.Center.X, 1e-9, "Center X preserved");
        AssertNear(99.0, controller.CurrentCamera.Center.Y, 1e-9, "Center Y preserved");
        AssertNear(0.5, controller.CurrentCamera.WorldUnitsPerPixel, 1e-9, "WUPP preserved");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertNear(double expected, double actual, double tolerance, string message)
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException($"{message}. Expected: {expected:R}, Actual: {actual:R}");
        }
    }
}
