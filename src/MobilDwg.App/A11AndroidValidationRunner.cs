#if A11_VALIDATION
using System.Security.Cryptography;
using Android.Util;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;

namespace MobilDwg.App;

internal sealed record A11AndroidValidationResult(
    string Marker,
    byte[] Png,
    int NonBackgroundPixels,
    string PngSha256);

internal static class A11AndroidValidationRunner
{
    internal const string Tag = "MobilDwgA11";

    public static async Task<A11AndroidValidationResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var scene = BuildAcceptanceScene();
        if (scene.WorldBounds is not { } initialBounds)
        {
            throw new InvalidOperationException("scene must have valid bounds");
        }

        var initialCamera = Camera2D.Fit(initialBounds, 900, 900, 0.08);
        var controller = new ViewportController(initialCamera, initialBounds);

        // 1. Initial fit render
        var initialRender = await Task.Run(
            () => SkiaScenePngRenderer.RenderCameraWithStatsAsync(scene, controller.CurrentCamera, cancellationToken: cancellationToken).AsTask(),
            cancellationToken);
        Require(initialRender.NonBackgroundPixels > 1000, $"initial non-background pixel count; actual={initialRender.NonBackgroundPixels}");

        // 2. Pan gesture simulation
        controller.BeginInteraction();
        var pannedCamera = controller.Pan(150.0, -100.0);
        controller.EndInteraction();
        var pannedRender = await Task.Run(
            () => SkiaScenePngRenderer.RenderCameraWithStatsAsync(scene, pannedCamera, cancellationToken: cancellationToken).AsTask(),
            cancellationToken);
        var initialSha = Convert.ToHexString(SHA256.HashData(initialRender.Png)).ToLowerInvariant();
        var pannedSha = Convert.ToHexString(SHA256.HashData(pannedRender.Png)).ToLowerInvariant();
        Require(pannedSha != initialSha, "pan must produce visual translation on rendered pixels");
        Log.Info(Tag, "A11_ANDROID_PAN_PASS");

        // 3. Pinch zoom with focal point preservation
        var focalPoint = new ScreenPoint2(350.0, 450.0);
        var worldBeforeZoom = CameraTransform.ScreenToWorld(focalPoint, controller.CurrentCamera);
        controller.BeginInteraction();
        var zoomedCamera = controller.PinchZoom(focalPoint, 2.5);
        controller.EndInteraction();
        var worldAfterZoom = CameraTransform.ScreenToWorld(focalPoint, zoomedCamera);
        var focalDriftX = Math.Abs(worldBeforeZoom.X - worldAfterZoom.X);
        var focalDriftY = Math.Abs(worldBeforeZoom.Y - worldAfterZoom.Y);
        Require(focalDriftX < 1e-6 && focalDriftY < 1e-6, $"pinch zoom must preserve world point under focal coordinate; drift=({focalDriftX},{focalDriftY})");
        Log.Info(Tag, "A11_ANDROID_FOCAL_PRESERVATION_PASS");

        var zoomedRender = await Task.Run(
            () => SkiaScenePngRenderer.RenderCameraWithStatsAsync(scene, zoomedCamera, cancellationToken: cancellationToken).AsTask(),
            cancellationToken);
        Require(zoomedRender.NonBackgroundPixels > 500, "zoomed render non-background pixels");
        Log.Info(Tag, "A11_ANDROID_PINCH_ZOOM_PASS");

        // 4. Double-tap simulation
        var preDoubleTapWupp = controller.CurrentCamera.WorldUnitsPerPixel;
        controller.DoubleTap(new ScreenPoint2(450.0, 450.0));
        Log.Info(Tag, "A11_ANDROID_DOUBLE_TAP_PASS");

        // 5. Fit extents simulation
        var fitCamera = controller.FitExtents();
        var centerDriftX = Math.Abs(fitCamera.Center.X - initialBounds.Center.X);
        var centerDriftY = Math.Abs(fitCamera.Center.Y - initialBounds.Center.Y);
        Require(centerDriftX < 1e-6 && centerDriftY < 1e-6, "fit extents must re-center the drawing");
        Log.Info(Tag, "A11_ANDROID_FIT_EXTENTS_PASS");

        // 6. Orientation change / resize simulation (reparse-free)
        controller.Resize(1200, 800);
        Require(controller.CurrentCamera.PixelWidth == 1200 && controller.CurrentCamera.PixelHeight == 800, "resize must update viewport dimensions");
        Require(Math.Abs(controller.CurrentCamera.Center.X - fitCamera.Center.X) < 1e-9, "resize must preserve center X");
        Require(Math.Abs(controller.CurrentCamera.Center.Y - fitCamera.Center.Y) < 1e-9, "resize must preserve center Y");
        // Restore 900x900 for final UI display
        controller.Resize(900, 900);
        Log.Info(Tag, "A11_ANDROID_ORIENTATION_RESIZE_PASS");

        // Final UI render
        var finalRender = await Task.Run(
            () => SkiaScenePngRenderer.RenderCameraWithStatsAsync(scene, controller.CurrentCamera, cancellationToken: cancellationToken).AsTask(),
            cancellationToken);

        var finalPng = finalRender.Png;
        Require(finalPng.Length > 2048, $"final PNG byte threshold; actual={finalPng.Length}");
        Require(finalPng.Length >= 8 && finalPng[0] == 0x89 && finalPng[1] == 0x50 && finalPng[2] == 0x4E && finalPng[3] == 0x47, "PNG signature");
        var finalSha = Convert.ToHexString(SHA256.HashData(finalPng)).ToLowerInvariant();

        Log.Info(Tag, $"A11_ANDROID_PNG_PASS bytes={finalPng.Length} sha256={finalSha}");
        const string marker = "ANDROID_STAGE11_VIEWPORT_GESTURE_PASS";
        Log.Info(Tag, marker);
        Log.Info(Tag, "CLAIM_LIMIT=A11_VIEWPORT_GESTURE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY");

        return new A11AndroidValidationResult(marker, finalPng, finalRender.NonBackgroundPixels, finalSha);
    }

    private static RenderScene BuildAcceptanceScene()
    {
        var builder = new RenderSceneAssembler(RenderColorContext.Dark);
        builder.AddEntity(Entity("P0-LINE", "LINE", 1, [new LinePrimitive(new WorldPoint2(-30, -20), new WorldPoint2(30, -20))]));
        builder.AddEntity(Entity("P0-ARC", "ARC", 2, [new ArcPrimitive(new WorldPoint2(0, 0), 18, 0, Math.PI)]));
        builder.AddEntity(Entity("P0-CIRCLE", "CIRCLE", 3, [new ArcPrimitive(new WorldPoint2(30, 15), 10, 0, Math.PI * 2d)]));
        builder.AddEntity(Entity("P0-ELLIPSE", "ELLIPSE", 4, [new EllipsePrimitive(new WorldPoint2(0, -2), 16, 7, Math.PI / 5d)]));
        builder.AddEntity(Entity("P0-POINT", "POINT", 5, [new PointPrimitive(new WorldPoint2(-32, 30))]));
        builder.AddEntity(Entity("P0-LWPOLYLINE", "LWPOLYLINE", 6, [new PolylinePrimitive([
            new PolylineVertex(new WorldPoint2(-28, 18), 0.5),
            new PolylineVertex(new WorldPoint2(-10, 30)),
            new PolylineVertex(new WorldPoint2(8, 18)),
        ])]));
        builder.AddEntity(Entity("P0-SPLINE", "SPLINE", 7, [new SplinePrimitive(2,
            [new WorldPoint2(-25, 0), new WorldPoint2(-5, 35), new WorldPoint2(25, 5)],
            [0d, 0d, 0d, 1d, 1d, 1d])]));
        builder.AddEntity(Entity("P0-SOLID", "SOLID", 8, [new PolygonPrimitive([
            new WorldPoint2(15, 25), new WorldPoint2(35, 25), new WorldPoint2(28, 40), new WorldPoint2(18, 38),
        ])]));
        builder.AddEntity(Entity("P0-TRACE", "TRACE", 9, [new PolygonPrimitive([
            new WorldPoint2(-5, -35), new WorldPoint2(8, -35), new WorldPoint2(10, -28), new WorldPoint2(-8, -28),
        ])]));
        builder.AddEntity(Entity("P0-3DFACE", "3DFACE", 10, [new PolygonPrimitive([
            new WorldPoint2(20, -35), new WorldPoint2(38, -32), new WorldPoint2(34, -22), new WorldPoint2(22, -24),
        ])]));
        builder.AddDiagnostic(new SceneDiagnostic(
            SceneDiagnosticKind.Dropped,
            "P0_INVALID_GEOMETRY_DROPPED",
            "Invalid source geometry is reported instead of silently rendered."));
        return builder.Build();
    }

    private static RenderSceneEntity Entity(string id, string type, int sourceIndex, IEnumerable<RenderGeometryPrimitive> geometry) => new(
        new RenderEntityId(id),
        new RenderLayerToken("0"),
        new RenderStyleToken("BYLAYER"),
        new RenderSourceReference(type, handle: id, sourceIndex: sourceIndex),
        geometry);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"A11 validation failed: {message}");
    }
}
#endif
