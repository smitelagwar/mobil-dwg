using System;
using System.Runtime.CompilerServices;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Tests;

public static class ViewportCameraTests
{
    public static void Run()
    {
        TestScreenWorldRoundTrip();
        TestFocalManipulationMovingCentroid();
        Test1000PinchCyclesDrift();
        TestLargeSurveyCoordinatesAndPrecisionLimits();
        TestSamplingFrequencyInvariance();
        TestViewerZoomPolicyBoundsEdgeCases();
        TestDoubleTapAlwaysZoomsIn();
        TestResizePreservesCenterAndWupp();
        TestCoordinateGuardAtBoundaries();
        Console.WriteLine("STAGE03_VIEWPORT_CAMERA_TESTS_PASS");
    }

    private static void TestScreenWorldRoundTrip()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(12345.67, -9876.54), 0.125);

        var testPoints = new[]
        {
            new ScreenPoint2(0, 0),
            new ScreenPoint2(1080, 2400),
            new ScreenPoint2(540, 1200),
            new ScreenPoint2(-150.5, -320.75), // negative screen coordinates
            new ScreenPoint2(2500.25, 4000.75), // beyond screen bounds
            new ScreenPoint2(0.0001, 2399.9999)
        };

        foreach (var pt in testPoints)
        {
            var world = CameraTransform.ScreenToWorld(pt, camera);
            var roundTrip = CameraTransform.WorldToScreen(world, camera);

            AssertNear(pt.X, roundTrip.X, 1e-9, $"Screen-to-world roundtrip X mismatch for ({pt.X}, {pt.Y})");
            AssertNear(pt.Y, roundTrip.Y, 1e-9, $"Screen-to-world roundtrip Y mismatch for ({pt.X}, {pt.Y})");
        }
    }

    private static void TestFocalManipulationMovingCentroid()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(500.0, 1000.0), 0.5);

        // Case 1: Pure pan (factor = 1.0)
        var m0 = new ScreenPoint2(200, 300);
        var m1 = new ScreenPoint2(350, 420);
        var panned = camera.Manipulate(m0, m1, 1.0);
        AssertNear(camera.WorldUnitsPerPixel, panned.WorldUnitsPerPixel, 1e-12, "Pure pan must not alter WUPP");
        var worldUnderM0Before = CameraTransform.ScreenToWorld(m0, camera);
        var worldUnderM1After = CameraTransform.ScreenToWorld(m1, panned);
        AssertNear(worldUnderM0Before.X, worldUnderM1After.X, 1e-9, "Panned world point must map to new centroid X");
        AssertNear(worldUnderM0Before.Y, worldUnderM1After.Y, 1e-9, "Panned world point must map to new centroid Y");

        // Case 2: Pure zoom at focal point (m0 == m1)
        var focal = new ScreenPoint2(600, 1500);
        var zoomed = camera.Manipulate(focal, focal, 2.5);
        AssertNear(camera.WorldUnitsPerPixel / 2.5, zoomed.WorldUnitsPerPixel, 1e-12, "Zoom factor mismatch");
        var worldFocalBefore = CameraTransform.ScreenToWorld(focal, camera);
        var worldFocalAfter = CameraTransform.ScreenToWorld(focal, zoomed);
        AssertNear(worldFocalBefore.X, worldFocalAfter.X, 1e-9, "Pure zoom focal world X mismatch");
        AssertNear(worldFocalBefore.Y, worldFocalAfter.Y, 1e-9, "Pure zoom focal world Y mismatch");

        // Case 3: Simultaneous pan + pinch (moving centroid)
        var prevCentroid = new ScreenPoint2(300, 800);
        var currCentroid = new ScreenPoint2(450, 650);
        double factor = 1.75;
        var manipulated = camera.Manipulate(prevCentroid, currCentroid, factor);

        var focalWorld = CameraTransform.ScreenToWorld(prevCentroid, camera);
        var mappedScreen = CameraTransform.WorldToScreen(focalWorld, manipulated);

        // Step focal error must be <= 0.25 physical pixels (in double precision it should be < 1e-9)
        var driftX = Math.Abs(mappedScreen.X - currCentroid.X);
        var driftY = Math.Abs(mappedScreen.Y - currCentroid.Y);
        Assert(driftX <= 0.25, $"Manipulate step drift X ({driftX}) exceeded 0.25px");
        Assert(driftY <= 0.25, $"Manipulate step drift Y ({driftY}) exceeded 0.25px");
        AssertNear(currCentroid.X, mappedScreen.X, 1e-9, "Moving centroid focal mapping X");
        AssertNear(currCentroid.Y, mappedScreen.Y, 1e-9, "Moving centroid focal mapping Y");
    }

    private static void Test1000PinchCyclesDrift()
    {
        var camera = new Camera2D(1080, 2400, new WorldPoint2(100.0, 200.0), 0.5);
        var initialFocalWorld = CameraTransform.ScreenToWorld(new ScreenPoint2(540, 1200), camera);

        var focal = new ScreenPoint2(640, 1100);
        var factor = 1.05;

        var current = camera;
        for (int i = 0; i < 1000; i++)
        {
            // Zoom in
            current = current.Manipulate(focal, focal, factor);
            // Zoom out
            current = current.Manipulate(focal, focal, 1.0 / factor);
        }

        var finalFocalWorld = CameraTransform.ScreenToWorld(new ScreenPoint2(540, 1200), current);
        var screenAfter1000 = CameraTransform.WorldToScreen(initialFocalWorld, current);

        var totalDriftPx = Math.Sqrt(Math.Pow(screenAfter1000.X - 540, 2) + Math.Pow(screenAfter1000.Y - 1200, 2));
        Assert(totalDriftPx <= 0.5, $"1000 pinch cycles total drift ({totalDriftPx} px) exceeded 0.5 px limit");
        AssertNear(0.5, current.WorldUnitsPerPixel, 1e-6, "WUPP drift after 1000 cycles");
    }

    private static void TestLargeSurveyCoordinatesAndPrecisionLimits()
    {
        // 5 million survey coordinates with 0.001 mm detail
        var center = new WorldPoint2(5_000_000.0, 5_000_000.0);
        var bounds = new WorldBounds2(4_999_990, 4_999_990, 5_000_010, 5_000_010);

        var (minWupp, maxWupp) = ViewerZoomPolicy.CalculateZoomLimits(bounds, center, null, 1080, 2400);

        // ULP of 5e6 is ~9.31e-10, 8*ulp is ~7.45e-9
        var ulp5M = ViewerZoomPolicy.Ulp(5_000_000.0);
        AssertNear(ulp5M, 9.313225746154785e-10, 1e-15, "ULP of 5M mismatch");
        AssertNear(minWupp, 8.0 * ulp5M, 1e-15, "minWupp floor calculation");

        // Detail of 0.001 world units: at 100 pixels, WUPP is 0.001 / 100 = 1e-5.
        // This is well above minWupp (~7.45e-9).
        Assert(1e-5 > minWupp, "0.001 detail must be zoomable above precision floor");

        var controller = new ViewportController(new Camera2D(1080, 2400, center, 0.01, minWupp, maxWupp), bounds);

        // Manipulate near survey origin
        var p0 = new ScreenPoint2(540, 1200);
        var p1 = new ScreenPoint2(550, 1190);
        controller.Manipulate(p0, p1, 1.2);

        Assert(double.IsFinite(controller.CurrentCamera.Center.X), "Survey center X must remain finite");
        Assert(double.IsFinite(controller.CurrentCamera.Center.Y), "Survey center Y must remain finite");
        Assert(double.IsFinite(controller.CurrentCamera.WorldUnitsPerPixel), "Survey WUPP must remain finite");
        Assert(controller.CurrentCamera.WorldUnitsPerPixel >= minWupp, "Survey WUPP must respect precision floor");

        // Verify limits near 1e12 boundary
        var nearLimitCenter = new WorldPoint2(9.99e11, -9.99e11);
        var (limitMin, limitMax) = ViewerZoomPolicy.CalculateZoomLimits(null, nearLimitCenter, null, 1080, 2400);
        Assert(double.IsFinite(limitMin) && limitMin > 0, "Near-limit minWupp must be finite and positive");
        Assert(double.IsFinite(limitMax) && limitMax >= limitMin, "Near-limit maxWupp must be >= minWupp");
    }

    private static void TestSamplingFrequencyInvariance()
    {
        // 1-second linear pan of (240, -120) screen pixels
        double totalDeltaX = 240.0;
        double totalDeltaY = -120.0;
        var startCamera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);

        Camera2D RunPanSimulation(int steps)
        {
            var cam = startCamera;
            var stepDx = totalDeltaX / steps;
            var stepDy = totalDeltaY / steps;
            for (int i = 0; i < steps; i++)
            {
                cam = cam.PanBy(stepDx, stepDy);
            }
            return cam;
        }

        var cam15 = RunPanSimulation(15);
        var cam30 = RunPanSimulation(30);
        var cam60 = RunPanSimulation(60);
        var cam120 = RunPanSimulation(120);

        AssertNear(cam15.Center.X, cam30.Center.X, 1e-10, "Pan 15Hz vs 30Hz Center.X mismatch");
        AssertNear(cam30.Center.X, cam60.Center.X, 1e-10, "Pan 30Hz vs 60Hz Center.X mismatch");
        AssertNear(cam60.Center.X, cam120.Center.X, 1e-10, "Pan 60Hz vs 120Hz Center.X mismatch");
        AssertNear(cam15.Center.Y, cam120.Center.Y, 1e-10, "Pan 15Hz vs 120Hz Center.Y mismatch");

        // 1-second pinch zoom of factor 4.0 at focal point (300, 400)
        double totalFactor = 4.0;
        var focal = new ScreenPoint2(300, 400);

        Camera2D RunPinchSimulation(int steps)
        {
            var cam = startCamera;
            var stepFactor = Math.Pow(totalFactor, 1.0 / steps);
            for (int i = 0; i < steps; i++)
            {
                cam = cam.Manipulate(focal, focal, stepFactor);
            }
            return cam;
        }

        var pinch15 = RunPinchSimulation(15);
        var pinch30 = RunPinchSimulation(30);
        var pinch60 = RunPinchSimulation(60);
        var pinch120 = RunPinchSimulation(120);

        AssertNear(pinch15.WorldUnitsPerPixel, pinch120.WorldUnitsPerPixel, 1e-10, "Pinch 15Hz vs 120Hz WUPP mismatch");
        AssertNear(pinch30.WorldUnitsPerPixel, pinch60.WorldUnitsPerPixel, 1e-10, "Pinch 30Hz vs 60Hz WUPP mismatch");
        AssertNear(pinch15.Center.X, pinch120.Center.X, 1e-10, "Pinch 15Hz vs 120Hz Center.X mismatch");
        AssertNear(pinch15.Center.Y, pinch120.Center.Y, 1e-10, "Pinch 15Hz vs 120Hz Center.Y mismatch");
    }

    private static void TestViewerZoomPolicyBoundsEdgeCases()
    {
        // 1. Null / empty bounds: default (0,0) center, WUPP 1.0
        var emptyFit = ViewerZoomPolicy.CreateFitCamera(null, 1000, 500);
        AssertNear(0.0, emptyFit.Center.X, 1e-12, "Empty fit Center.X");
        AssertNear(0.0, emptyFit.Center.Y, 1e-12, "Empty fit Center.Y");
        AssertNear(1.0, emptyFit.WorldUnitsPerPixel, 1e-12, "Empty fit WUPP");

        // 2. Single point: width=0, height=0
        var pointBounds = new WorldBounds2(100, 200, 100, 200);
        var pointFit = ViewerZoomPolicy.CreateFitCamera(pointBounds, 1000, 500, paddingFraction: 0.05);
        AssertNear(100.0, pointFit.Center.X, 1e-12, "Point fit Center.X");
        AssertNear(200.0, pointFit.Center.Y, 1e-12, "Point fit Center.Y");
        // Virtual 1 drawing unit extent: usable width = 900, usable height = 450. Max(1/900, 1/450) = 1/450
        AssertNear(1.0 / 450.0, pointFit.WorldUnitsPerPixel, 1e-12, "Point fit WUPP");

        // 3. Horizontal line: width=200, height=0
        var hLineBounds = new WorldBounds2(0, 50, 200, 50);
        var hLineFit = ViewerZoomPolicy.CreateFitCamera(hLineBounds, 1000, 500, paddingFraction: 0.05);
        AssertNear(100.0, hLineFit.Center.X, 1e-12, "HLine fit Center.X");
        AssertNear(50.0, hLineFit.Center.Y, 1e-12, "HLine fit Center.Y");
        // Usable width = 900 -> 200 / 900
        AssertNear(200.0 / 900.0, hLineFit.WorldUnitsPerPixel, 1e-12, "HLine fit WUPP");

        // 4. Vertical line: width=0, height=180
        var vLineBounds = new WorldBounds2(30, 0, 30, 180);
        var vLineFit = ViewerZoomPolicy.CreateFitCamera(vLineBounds, 1000, 500, paddingFraction: 0.05);
        AssertNear(30.0, vLineFit.Center.X, 1e-12, "VLine fit Center.X");
        AssertNear(90.0, vLineFit.Center.Y, 1e-12, "VLine fit Center.Y");
        // Usable height = 450 -> 180 / 450
        AssertNear(180.0 / 450.0, vLineFit.WorldUnitsPerPixel, 1e-12, "VLine fit WUPP");
    }

    private static void TestDoubleTapAlwaysZoomsIn()
    {
        var initialCamera = new Camera2D(1000, 1000, new WorldPoint2(0, 0), 1.0);
        var bounds = new WorldBounds2(-50, -50, 50, 50);
        var controller = new ViewportController(initialCamera, bounds);

        var tap = new ScreenPoint2(500, 500);
        double prevWupp = controller.CurrentCamera.WorldUnitsPerPixel;

        // Perform 6 consecutive double taps
        for (int i = 0; i < 6; i++)
        {
            controller.DoubleTap(tap, 2.0);
            var currentWupp = controller.CurrentCamera.WorldUnitsPerPixel;
            Assert(currentWupp < prevWupp, $"DoubleTap step {i} did not zoom in: prev={prevWupp}, curr={currentWupp}");
            AssertNear(prevWupp / 2.0, currentWupp, 1e-9, $"DoubleTap step {i} factor mismatch");
            prevWupp = currentWupp;
        }

        // Even when zoomed in 64x compared to initial/fit, it NEVER resets to fit
        Assert(controller.CurrentCamera.WorldUnitsPerPixel < 0.02, "Camera must stay deeply zoomed in");
    }

    private static void TestResizePreservesCenterAndWupp()
    {
        var camera = new Camera2D(1080, 1920, new WorldPoint2(123.45, 678.90), 0.25);
        var bounds = new WorldBounds2(0, 0, 500, 500);
        var controller = new ViewportController(camera, bounds);

        controller.Resize(1920, 1080);

        Assert(controller.CurrentCamera.PixelWidth == 1920, "PixelWidth mismatch after resize");
        Assert(controller.CurrentCamera.PixelHeight == 1080, "PixelHeight mismatch after resize");
        AssertNear(123.45, controller.CurrentCamera.Center.X, 1e-12, "Center.X altered by resize");
        AssertNear(678.90, controller.CurrentCamera.Center.Y, 1e-12, "Center.Y altered by resize");
        AssertNear(0.25, controller.CurrentCamera.WorldUnitsPerPixel, 1e-12, "WUPP altered by resize");
    }

    private static void TestCoordinateGuardAtBoundaries()
    {
        var camera = new Camera2D(1000, 1000, new WorldPoint2(1e12, 1e12), 1.0);
        var controller = new ViewportController(camera);

        // Attempt to pan even further past 1e12
        controller.Pan(-1000, -1000);

        Assert(Math.Abs(controller.CurrentCamera.Center.X) <= 1e12, "Center.X must not exceed 1e12");
        Assert(Math.Abs(controller.CurrentCamera.Center.Y) <= 1e12, "Center.Y must not exceed 1e12");
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
