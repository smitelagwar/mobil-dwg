using System;
using System.Runtime.CompilerServices;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Tests;

internal static class Stage11ViewportGestureTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestPanAccuracy();
        TestPinchZoomFocalPointPreservation();
        TestMinMaxZoomClamping();
        TestResize();
        TestFitExtents();
        TestViewportControllerStateMachine();
        Console.WriteLine("STAGE11_VIEWPORT_GESTURE_TESTS_PASS");
    }

    private static void TestPanAccuracy()
    {
        var initialCamera = new Camera2D(1080, 2400, new WorldPoint2(5000000.001, -25.5), 0.5);

        // Pick a world point and find its screen coordinates
        var worldPoint = new WorldPoint2(5000010.0, -10.0);
        var initialScreenPoint = CameraTransform.WorldToScreen(worldPoint, initialCamera);

        // Pan by (120, -85) screen pixels
        double deltaScreenX = 120.0;
        double deltaScreenY = -85.0;
        var pannedCamera = initialCamera.PanBy(deltaScreenX, deltaScreenY);

        // After panning, the visual entity on screen must have moved by exactly (deltaScreenX, deltaScreenY)
        var newScreenPoint = CameraTransform.WorldToScreen(worldPoint, pannedCamera);

        AssertNear(initialScreenPoint.X + deltaScreenX, newScreenPoint.X, 1e-9, "Pan X screen delta mismatch");
        AssertNear(initialScreenPoint.Y + deltaScreenY, newScreenPoint.Y, 1e-9, "Pan Y screen delta mismatch");

        // WorldUnitsPerPixel must remain unchanged
        AssertNear(initialCamera.WorldUnitsPerPixel, pannedCamera.WorldUnitsPerPixel, 1e-12, "Pan changed WUPP");

        // Invalid args
        AssertThrows<ArgumentOutOfRangeException>(() => initialCamera.PanBy(double.NaN, 0));
        AssertThrows<ArgumentOutOfRangeException>(() => initialCamera.PanBy(0, double.PositiveInfinity));
    }

    private static void TestPinchZoomFocalPointPreservation()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(100.0, 200.0), 0.25);

        // Test with different focal points: center, corner, arbitrary touch point
        var focalPoints = new[]
        {
            new ScreenPoint2(540.0, 1200.0), // center
            new ScreenPoint2(100.0, 200.0),  // top-left
            new ScreenPoint2(980.0, 2200.0), // bottom-right
            new ScreenPoint2(345.67, 891.23) // arbitrary
        };

        var factors = new[] { 1.5, 0.5, 3.25, 0.3333333333333333 };

        foreach (var focal in focalPoints)
        {
            foreach (var factor in factors)
            {
                var worldBefore = CameraTransform.ScreenToWorld(focal, camera);
                var zoomedCamera = camera.ZoomAt(focal, factor);
                var worldAfter = CameraTransform.ScreenToWorld(focal, zoomedCamera);

                AssertNear(worldBefore.X, worldAfter.X, 1e-9, $"Focal point X drift for factor {factor}");
                AssertNear(worldBefore.Y, worldAfter.Y, 1e-9, $"Focal point Y drift for factor {factor}");

                var expectedWupp = camera.WorldUnitsPerPixel / factor;
                AssertNear(expectedWupp, zoomedCamera.WorldUnitsPerPixel, 1e-12, $"ZoomAt WUPP mismatch for factor {factor}");
            }
        }

        // Invalid factors
        AssertThrows<ArgumentOutOfRangeException>(() => camera.ZoomAt(new ScreenPoint2(100, 100), 0));
        AssertThrows<ArgumentOutOfRangeException>(() => camera.ZoomAt(new ScreenPoint2(100, 100), -2.0));
        AssertThrows<ArgumentOutOfRangeException>(() => camera.ZoomAt(new ScreenPoint2(100, 100), double.NaN));
    }

    private static void TestMinMaxZoomClamping()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0, minWorldUnitsPerPixel: 1e-4, maxWorldUnitsPerPixel: 1e4);

        // Extreme zoom-in: should clamp to min
        var zoomedIn = camera.ZoomBy(1e10);
        AssertNear(1e-4, zoomedIn.WorldUnitsPerPixel, 1e-12, "Min zoom clamp mismatch");

        // Extreme zoom-out: should clamp to max
        var zoomedOut = camera.ZoomBy(1e-10);
        AssertNear(1e4, zoomedOut.WorldUnitsPerPixel, 1e-12, "Max zoom clamp mismatch");

        // ZoomAt should also clamp
        var focal = new ScreenPoint2(500, 500);
        var clampedFocalIn = camera.ZoomAt(focal, 1e12);
        AssertNear(1e-4, clampedFocalIn.WorldUnitsPerPixel, 1e-12, "ZoomAt min clamp mismatch");

        var clampedFocalOut = camera.ZoomAt(focal, 1e-12);
        AssertNear(1e4, clampedFocalOut.WorldUnitsPerPixel, 1e-12, "ZoomAt max clamp mismatch");
    }

    private static void TestResize()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(42.0, -84.0), 0.75);

        // Resize from portrait to landscape
        var resized = camera.Resize(2400, 1080);

        if (resized.PixelWidth != 2400 || resized.PixelHeight != 1080)
        {
            throw new InvalidOperationException("Resize did not update pixel dimensions.");
        }

        AssertNear(camera.Center.X, resized.Center.X, 1e-12, "Resize modified Center.X");
        AssertNear(camera.Center.Y, resized.Center.Y, 1e-12, "Resize modified Center.Y");
        AssertNear(camera.WorldUnitsPerPixel, resized.WorldUnitsPerPixel, 1e-12, "Resize modified WUPP");

        AssertThrows<ArgumentOutOfRangeException>(() => camera.Resize(0, 1080));
        AssertThrows<ArgumentOutOfRangeException>(() => camera.Resize(1080, -100));
    }

    private static void TestFitExtents()
    {
        var bounds = new WorldBounds2(-50, -25, 150, 75); // width = 200, height = 100, center = (50, 25)
        var camera = Camera2D.Fit(bounds, 1000, 500, paddingFraction: 0.1);

        AssertNear(50.0, camera.Center.X, 1e-12, "Fit Center.X mismatch");
        AssertNear(25.0, camera.Center.Y, 1e-12, "Fit Center.Y mismatch");

        // Usable width = 1000 * 0.8 = 800. scaleX = 200 / 800 = 0.25
        // Usable height = 500 * 0.8 = 400. scaleY = 100 / 400 = 0.25
        AssertNear(0.25, camera.WorldUnitsPerPixel, 1e-9, "Fit WUPP mismatch");

        // Verify bounds are fully inside the screen
        var minScreen = CameraTransform.WorldToScreen(new WorldPoint2(-50, -25), camera);
        var maxScreen = CameraTransform.WorldToScreen(new WorldPoint2(150, 75), camera);

        if (minScreen.X < 0 || minScreen.X > 1000 || minScreen.Y < 0 || minScreen.Y > 500)
        {
            throw new InvalidOperationException("Fitted bounds min corner outside screen bounds.");
        }
        if (maxScreen.X < 0 || maxScreen.X > 1000 || maxScreen.Y < 0 || maxScreen.Y > 500)
        {
            throw new InvalidOperationException("Fitted bounds max corner outside screen bounds.");
        }
    }

    private static void TestViewportControllerStateMachine()
    {
        var initialCamera = new Camera2D(1080, 2400, new WorldPoint2(0, 0), 1.0);
        var bounds = new WorldBounds2(-100, -100, 100, 100);
        var controller = new ViewportController(initialCamera, bounds);

        if (controller.IsInteracting) throw new InvalidOperationException("Controller should start not interacting.");
        if (controller.UpdateCount != 0) throw new InvalidOperationException("UpdateCount should start at 0.");

        controller.BeginInteraction();
        if (!controller.IsInteracting) throw new InvalidOperationException("IsInteracting should be true.");

        // Pan
        controller.Pan(50, -30);
        if (controller.UpdateCount != 1) throw new InvalidOperationException("UpdateCount should be 1 after pan.");

        // PinchZoom
        controller.PinchZoom(new ScreenPoint2(540, 1200), 2.0);
        if (controller.UpdateCount != 2) throw new InvalidOperationException("UpdateCount should be 2 after pinch.");

        controller.EndInteraction();
        if (controller.IsInteracting) throw new InvalidOperationException("IsInteracting should be false after end.");

        // DoubleTap zoom-in
        var preTapWupp = controller.CurrentCamera.WorldUnitsPerPixel;
        controller.DoubleTap(new ScreenPoint2(540, 1200), 2.0);
        if (controller.CurrentCamera.WorldUnitsPerPixel >= preTapWupp)
        {
            throw new InvalidOperationException("DoubleTap should have zoomed in.");
        }

        // Multiple DoubleTaps should eventually trigger reset to fit extents
        controller.DoubleTap(new ScreenPoint2(540, 1200), 2.0);
        controller.DoubleTap(new ScreenPoint2(540, 1200), 2.0);
        controller.DoubleTap(new ScreenPoint2(540, 1200), 2.0);

        // Fit extents
        var fitCamera = controller.FitExtents();
        AssertNear(0.0, fitCamera.Center.X, 1e-9, "FitExtents Center.X mismatch");
        AssertNear(0.0, fitCamera.Center.Y, 1e-9, "FitExtents Center.Y mismatch");

        // Resize
        controller.Resize(2400, 1080);
        if (controller.CurrentCamera.PixelWidth != 2400) throw new InvalidOperationException("Resize did not update width.");
    }

    private static void AssertNear(double expected, double actual, double tolerance, string message)
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException($"{message}. Expected: {expected}, Actual: {actual}, Delta: {Math.Abs(expected - actual)}");
        }
    }

    private static void AssertThrows<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Expected exception {typeof(TException).Name} but caught {ex.GetType().Name}.");
        }

        throw new InvalidOperationException($"Expected exception {typeof(TException).Name} was not thrown.");
    }
}
