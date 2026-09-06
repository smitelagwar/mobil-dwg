using System;
using System.Collections.Generic;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Tests;

public static class ViewportInteractionTests
{
    public static void Run()
    {
        TestOneToTwoToOneTransition();
        TestTwoToThreeToTwoTransition();
        TestPointerIdIndependenceAndOrderChange();
        TestSameCountPointerIdReplacement();
        TestSlopThresholdAndSinglePanCommit();
        TestUpAtSamePositionProducesZeroDelta();
        TestUpAtNewPositionAppliesFinalDeltaOnce();
        TestCancelClearsPointersAndPreservesCamera();
        TestDoubleTapAndMeasurementMode();
        TestMinSpanThresholdPinch();
        TestOutOfBoundsPointerTracking();
        Console.WriteLine("STAGE04_VIEWPORT_INTERACTION_TESTS_PASS");
    }

    private static void TestOneToTwoToOneTransition()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var config = new ViewportInputConfiguration { TouchSlopPx = 10.0 };
        var engine = new ViewportInteractionEngine(controller, config);

        // 1. First finger down at (200, 200)
        var p1 = new PointerSample(1, new ScreenPoint2(200, 200));
        var res1 = engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 100, new[] { p1 }, 0));
        Assert(engine.State == ViewportGestureState.TapCandidate, "State should be TapCandidate");
        Assert(engine.ActivePointerCount == 1, "Pointer count 1");

        // 2. Second finger down at (400, 200) -> 1 -> 2 transition
        var p2 = new PointerSample(2, new ScreenPoint2(400, 200));
        var res2 = engine.ProcessPacket(new PointerPacket(PointerAction.PointerDown, 2, 1, 150, new[] { p1, p2 }, 0));
        Assert(engine.State == ViewportGestureState.Pinch, "State should transition to Pinch");
        Assert(engine.ActivePointerCount == 2, "Pointer count 2");
        // Adding pointer must NOT jump camera
        AssertNear(0.0, controller.CurrentCamera.Center.X, 1e-9, "Camera X jump on PointerDown");
        AssertNear(0.0, controller.CurrentCamera.Center.Y, 1e-9, "Camera Y jump on PointerDown");

        // 3. Move both fingers: distance increases from 200 to 400 (factor = 2x)
        var p1Moved = new PointerSample(1, new ScreenPoint2(100, 200));
        var p2Moved = new PointerSample(2, new ScreenPoint2(500, 200));
        var res3 = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 200, new[] { p1Moved, p2Moved }, 0));
        Assert(res3.CameraChanged, "Camera should have changed on pinch move");
        AssertNear(0.5, controller.CurrentCamera.WorldUnitsPerPixel, 1e-9, "WUPP should be halved on 2x pinch");

        // 4. Second finger lifts -> 2 -> 1 transition
        var preLiftCamera = controller.CurrentCamera;
        var res4 = engine.ProcessPacket(new PointerPacket(PointerAction.PointerUp, 2, 1, 250, new[] { p1Moved }, 0));
        Assert(engine.State == ViewportGestureState.Pan, "State should transition to Pan");
        Assert(engine.ActivePointerCount == 1, "Pointer count 1");
        // Lifting pointer without movement must NOT jump camera
        AssertNear(preLiftCamera.Center.X, controller.CurrentCamera.Center.X, 1e-9, "Camera jump on PointerUp");
        AssertNear(preLiftCamera.Center.Y, controller.CurrentCamera.Center.Y, 1e-9, "Camera jump on PointerUp");

        // 5. First finger moves by (50, 0)
        var p1Panned = new PointerSample(1, new ScreenPoint2(150, 200));
        var res5 = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 300, new[] { p1Panned }, 0));
        Assert(res5.CameraChanged, "Pan move should change camera");
        AssertNear(preLiftCamera.Center.X - (50 * 0.5), controller.CurrentCamera.Center.X, 1e-9, "Pan delta X mismatch");

        // 6. Up
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 350, new[] { p1Panned }, 0));
        Assert(engine.State == ViewportGestureState.Idle, "State should return to Idle");
        Assert(engine.ActivePointerCount == 0, "Pointer count 0");
    }

    private static void TestTwoToThreeToTwoTransition()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        var p1 = new PointerSample(1, new ScreenPoint2(200, 200));
        var p2 = new PointerSample(2, new ScreenPoint2(400, 200));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 100, new[] { p1 }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerDown, 2, 1, 120, new[] { p1, p2 }, 0));
        Assert(engine.State == ViewportGestureState.Pinch, "State should be Pinch");

        // Add 3rd finger -> MultiTouchHold
        var p3 = new PointerSample(3, new ScreenPoint2(300, 400));
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerDown, 3, 2, 150, new[] { p1, p2, p3 }, 0));
        Assert(engine.State == ViewportGestureState.MultiTouchHold, "State should be MultiTouchHold");

        var frozenCamera = controller.CurrentCamera;

        // Move during 3 fingers: camera must remain frozen
        var p1M = new PointerSample(1, new ScreenPoint2(210, 210));
        var p2M = new PointerSample(2, new ScreenPoint2(410, 210));
        var p3M = new PointerSample(3, new ScreenPoint2(310, 410));
        var resMove = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 180, new[] { p1M, p2M, p3M }, 0));
        Assert(!resMove.CameraChanged, "Camera must NOT change in MultiTouchHold");
        AssertNear(frozenCamera.Center.X, controller.CurrentCamera.Center.X, 1e-9, "Center X changed in MultiTouchHold");

        // 3rd finger lifts -> back to Pinch (2 pointers)
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerUp, 3, 2, 200, new[] { p1M, p2M }, 0));
        Assert(engine.State == ViewportGestureState.Pinch, "State should transition back to Pinch");
        AssertNear(frozenCamera.Center.X, controller.CurrentCamera.Center.X, 1e-9, "Center X jump on back-to-pinch");

        // Resumed 2-finger move should work smoothly
        var p1M2 = new PointerSample(1, new ScreenPoint2(150, 210));
        var p2M2 = new PointerSample(2, new ScreenPoint2(470, 210));
        var resResumed = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 250, new[] { p1M2, p2M2 }, 0));
        Assert(resResumed.CameraChanged, "Resumed pinch move should change camera");
    }

    private static void TestPointerIdIndependenceAndOrderChange()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        // Down with id=7
        var p7 = new PointerSample(7, new ScreenPoint2(100, 100));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 7, 0, 10, new[] { p7 }, 0));

        // PointerDown with id=3
        var p3 = new PointerSample(3, new ScreenPoint2(300, 100));
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerDown, 3, 1, 20, new[] { p7, p3 }, 0));

        // Swap order in the packet pointers list: id 3 at index 0, id 7 at index 1
        var p3Swapped = new PointerSample(3, new ScreenPoint2(350, 100));
        var p7Swapped = new PointerSample(7, new ScreenPoint2(50, 100));
        var res = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 3, 0, 30, new[] { p3Swapped, p7Swapped }, 0));

        Assert(res.CameraChanged, "Pointers correctly tracked regardless of order in list");
        AssertNear(200.0 / 300.0, controller.CurrentCamera.WorldUnitsPerPixel, 1e-9, "Span increased from 200 to 300 -> WUPP 200/300");
    }

    private static void TestSameCountPointerIdReplacement()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        // Down with id=1
        var p1 = new PointerSample(1, new ScreenPoint2(200, 200));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { p1 }, 0));

        // Up id=1
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 20, new[] { p1 }, 0));

        // New Down with id=2
        var p2 = new PointerSample(2, new ScreenPoint2(500, 500));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 2, 0, 30, new[] { p2 }, 0));

        Assert(engine.State == ViewportGestureState.TapCandidate, "New pointer id establishes clean candidate");
        Assert(engine.ActivePointerCount == 1, "Pointer count 1");
    }

    private static void TestSlopThresholdAndSinglePanCommit()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var config = new ViewportInputConfiguration { TouchSlopPx = 15.0 };
        var engine = new ViewportInteractionEngine(controller, config);

        var pDown = new PointerSample(1, new ScreenPoint2(100, 100));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { pDown }, 0));

        // Move 5px: below 15px slop
        var pSmall = new PointerSample(1, new ScreenPoint2(105, 100));
        var resSmall = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { pSmall }, 0));
        Assert(!resSmall.CameraChanged, "Move below slop must NOT change camera");
        Assert(engine.State == ViewportGestureState.TapCandidate, "State must remain TapCandidate below slop");

        // Move to 20px (exceeds slop by 5px): total 20px displacement must be committed once
        var pCross = new PointerSample(1, new ScreenPoint2(120, 100));
        var resCross = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 30, new[] { pCross }, 0));
        Assert(resCross.CameraChanged, "Crossing slop must change camera");
        Assert(engine.State == ViewportGestureState.Pan, "State must transition to Pan");
        AssertNear(-20.0, controller.CurrentCamera.Center.X, 1e-9, "Entire 20px displacement must be committed");
    }

    private static void TestUpAtSamePositionProducesZeroDelta()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var config = new ViewportInputConfiguration { TouchSlopPx = 5.0 };
        var engine = new ViewportInteractionEngine(controller, config);

        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(100, 100)) }, 0));
        // Move to 150
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(150, 100)) }, 0));
        var camAfterMove = controller.CurrentCamera;

        // UP at same 150 coordinate
        var resUp = engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(150, 100)) }, 0));
        Assert(!resUp.CameraChanged, "UP at same coordinate must produce 0 camera delta");
        AssertNear(camAfterMove.Center.X, controller.CurrentCamera.Center.X, 1e-9, "Center.X must not drift on UP");
    }

    private static void TestUpAtNewPositionAppliesFinalDeltaOnce()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var config = new ViewportInputConfiguration { TouchSlopPx = 5.0 };
        var engine = new ViewportInteractionEngine(controller, config);

        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(100, 100)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(150, 100)) }, 0));
        var centerBeforeUp = controller.CurrentCamera.Center.X;

        // UP at 165 (an additional 15px delta)
        var resUp = engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(165, 100)) }, 0));
        Assert(resUp.CameraChanged, "UP at new position must apply final delta");
        AssertNear(centerBeforeUp - 15.0, controller.CurrentCamera.Center.X, 1e-9, "Final 15px delta applied once");
    }

    private static void TestCancelClearsPointersAndPreservesCamera()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(100, 100)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(150, 100)) }, 0));
        var validCamera = controller.CurrentCamera;

        engine.ProcessPacket(new PointerPacket(PointerAction.Cancel, 1, 0, 30, new PointerSample[0], 0));
        Assert(engine.State == ViewportGestureState.Idle, "Cancel resets to Idle");
        Assert(engine.ActivePointerCount == 0, "Cancel clears all pointers");
        AssertNear(validCamera.Center.X, controller.CurrentCamera.Center.X, 1e-9, "Cancel preserves camera");
    }

    private static void TestDoubleTapAndMeasurementMode()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var config = new ViewportInputConfiguration { DoubleTapTimeoutMs = 300, DoubleTapSlopPx = 25.0, DoubleTapZoomFactor = 2.0 };
        var engine = new ViewportInteractionEngine(controller, config);

        // Tap 1
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 100, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 0));
        var resUp1 = engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 150, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 0));
        Assert(resUp1.SingleTapDetected, "First tap must be detected");
        Assert(!resUp1.CameraChanged, "First tap must not alter camera in normal mode");

        // Tap 2 within timeout & slop (at time 280ms, dt=130ms <= 300ms)
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 250, new[] { new PointerSample(1, new ScreenPoint2(505, 505)) }, 0));
        var resUp2 = engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 280, new[] { new PointerSample(1, new ScreenPoint2(505, 505)) }, 0));
        Assert(resUp2.DoubleTapDetected, "DoubleTap must be detected");
        Assert(resUp2.CameraChanged, "DoubleTap must zoom camera");
        AssertNear(0.5, controller.CurrentCamera.WorldUnitsPerPixel, 1e-9, "DoubleTap must zoom 2x");

        // Test measurement mode disables double tap
        engine.IsMeasurementMode = true;
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 400, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 450, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 500, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 0));
        var resMeasUp2 = engine.ProcessPacket(new PointerPacket(PointerAction.Up, 1, 0, 550, new[] { new PointerSample(1, new ScreenPoint2(500, 500)) }, 0));
        Assert(!resMeasUp2.DoubleTapDetected, "Measurement mode must disable double tap zoom");
        Assert(resMeasUp2.SingleTapDetected, "Measurement mode must keep single tap");
    }

    private static void TestMinSpanThresholdPinch()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var config = new ViewportInputConfiguration { TouchSlopPx = 8.0 }; // minSpan = max(8, 16) = 16
        var engine = new ViewportInteractionEngine(controller, config);

        // Pointers very close: distance = 10 px (< 16 px)
        var p1 = new PointerSample(1, new ScreenPoint2(500, 500));
        var p2 = new PointerSample(2, new ScreenPoint2(510, 500));
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { p1 }, 0));
        engine.ProcessPacket(new PointerPacket(PointerAction.PointerDown, 2, 1, 20, new[] { p1, p2 }, 0));

        // Move both by (20, 0) keeping span 12 px (still < 16)
        var p1M = new PointerSample(1, new ScreenPoint2(520, 500));
        var p2M = new PointerSample(2, new ScreenPoint2(532, 500));
        var res = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 30, new[] { p1M, p2M }, 0));

        // Factor must be 1.0 (translation only, no accidental explosive zoom)
        AssertNear(1.0, controller.CurrentCamera.WorldUnitsPerPixel, 1e-9, "Below minSpan, WUPP must remain 1.0");
        Assert(res.CameraChanged, "Centroid translation still occurs");
    }

    private static void TestOutOfBoundsPointerTracking()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var controller = new ViewportController(camera);
        var engine = new ViewportInteractionEngine(controller);

        // Down inside view
        engine.ProcessPacket(new PointerPacket(PointerAction.Down, 1, 0, 10, new[] { new PointerSample(1, new ScreenPoint2(950, 500)) }, 0));

        // Drag out of view: coordinate 1250 (> 1000)
        var resOut = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 20, new[] { new PointerSample(1, new ScreenPoint2(1250, 500)) }, 0));
        Assert(resOut.CameraChanged, "Pointer dragging outside view bounds must continue panning");
        AssertNear(-300.0, controller.CurrentCamera.Center.X, 1e-9, "Out-of-bounds pan tracking");

        // Drag back to negative coordinates: -100
        var resNeg = engine.ProcessPacket(new PointerPacket(PointerAction.Move, 1, 0, 30, new[] { new PointerSample(1, new ScreenPoint2(-100, 500)) }, 0));
        Assert(resNeg.CameraChanged, "Negative screen coordinates must continue panning");
        AssertNear(1050.0, controller.CurrentCamera.Center.X, 1e-9, "Negative coordinate pan tracking");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertNear(double expected, double actual, double tolerance, string message)
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException($"{message}. Expected: {expected:R}, Actual: {actual:R}, Delta: {Math.Abs(expected - actual):R}");
        }
    }
}
