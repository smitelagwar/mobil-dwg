using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Rendering.Skia;

public sealed class SkiaBitmapRenderSurface : IRenderSurface, IDisposable
{
    private bool _disposed;

    public SkiaBitmapRenderSurface(int pixelWidth, int pixelHeight, double density = 1d)
    {
        if (pixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pixelWidth));
        if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight));
        if (!double.IsFinite(density) || density <= 0) throw new ArgumentOutOfRangeException(nameof(density));

        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        Density = density;
        Bitmap = new SKBitmap(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        Canvas = new SKCanvas(Bitmap);
    }

    public int PixelWidth { get; }
    public int PixelHeight { get; }
    public double Density { get; }
    public SKBitmap Bitmap { get; }
    internal SKCanvas Canvas { get; }

    public byte[] EncodePng()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using var image = SKImage.FromBitmap(Bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public byte[] EncodeJpeg(int quality = 85)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using var image = SKImage.FromBitmap(Bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, Math.Clamp(quality, 1, 100));
        return data.ToArray();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Canvas.Dispose();
        Bitmap.Dispose();
    }
}

public enum RenderOptimizationMode
{
    Optimized,
    BaselineUnoptimized
}

public sealed class SkiaCadRenderer : ICadRenderer
{
    public RenderOptimizationMode OptimizationMode { get; set; }

    public SkiaCadRenderer(RenderOptimizationMode optimizationMode = RenderOptimizationMode.Optimized)
    {
        OptimizationMode = optimizationMode;
    }

    public ValueTask RenderAsync(
        IRenderScene scene,
        IRenderSurface surface,
        RenderViewport viewport,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(surface);
        if (scene is not RenderScene renderScene)
            throw new ArgumentException("SkiaCadRenderer requires MobilDwg.Rendering.Scene.RenderScene.", nameof(scene));
        if (surface is not SkiaBitmapRenderSurface skiaSurface)
            throw new ArgumentException("SkiaCadRenderer requires SkiaBitmapRenderSurface.", nameof(surface));
        if (surface.PixelWidth != viewport.PixelWidth || surface.PixelHeight != viewport.PixelHeight)
            throw new ArgumentException("Render surface and viewport pixel dimensions must match.", nameof(viewport));

        cancellationToken.ThrowIfCancellationRequested();
        var camera = Camera2D.FromViewport(viewport);
        var snapshot = new RenderSnapshot(renderScene, renderScene.LayerTable, camera);
        var context = new RenderFrameContext(
            surface.PixelWidth,
            surface.PixelHeight,
            surface.Density,
            RenderQualityMode.Final,
            OptimizationMode == RenderOptimizationMode.Optimized);

        SkiaScenePainter.DrawFrame(skiaSurface.Canvas, snapshot, context);
        return ValueTask.CompletedTask;
    }
}

public sealed record ScenePngRenderResult(byte[] Png, int NonBackgroundPixels);

public static class SkiaScenePngRenderer
{
    public static async ValueTask<byte[]> RenderFitAsync(
        RenderScene scene,
        int pixelWidth,
        int pixelHeight,
        double density = 1d,
        double paddingFraction = 0.08,
        CancellationToken cancellationToken = default)
    {
        var result = await RenderFitWithStatsAsync(
            scene,
            pixelWidth,
            pixelHeight,
            density,
            paddingFraction,
            cancellationToken);
        return result.Png;
    }

    public static async ValueTask<ScenePngRenderResult> RenderFitWithStatsAsync(
        RenderScene scene,
        int pixelWidth,
        int pixelHeight,
        double density = 1d,
        double paddingFraction = 0.08,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (scene.WorldBounds is not { } bounds) throw new ArgumentException("Cannot fit-render an empty scene.", nameof(scene));

        var camera = Camera2D.Fit(bounds, pixelWidth, pixelHeight, paddingFraction);
        return await RenderCameraWithStatsAsync(scene, camera, density, cancellationToken);
    }

    public static async ValueTask<ScenePngRenderResult> RenderCameraWithStatsAsync(
        RenderScene scene,
        Camera2D camera,
        double density = 1d,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (!camera.IsValid) throw new ArgumentException("Camera must be valid.", nameof(camera));

        using var surface = new SkiaBitmapRenderSurface(camera.PixelWidth, camera.PixelHeight, density);
        await new SkiaCadRenderer().RenderAsync(scene, surface, camera.ToViewport(), cancellationToken);

        var background = scene.ColorContext.BackgroundArgb;
        var nonBackgroundPixels = surface.Bitmap.Pixels.Count(pixel =>
            pixel.Alpha != 0 && ToArgb(pixel) != background);
        return new ScenePngRenderResult(surface.EncodePng(), nonBackgroundPixels);
    }

    private static uint ToArgb(SKColor color) =>
        ((uint)color.Alpha << 24) | ((uint)color.Red << 16) | ((uint)color.Green << 8) | color.Blue;
}

public static class SkiaFastRenderer
{
    public static async ValueTask<byte[]> RenderCameraJpegAsync(
        RenderScene scene,
        Camera2D camera,
        int quality = 85,
        double density = 1d,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (!camera.IsValid) throw new ArgumentException("Camera must be valid.", nameof(camera));

        using var surface = new SkiaBitmapRenderSurface(camera.PixelWidth, camera.PixelHeight, density);
        await new SkiaCadRenderer().RenderAsync(scene, surface, camera.ToViewport(), cancellationToken).ConfigureAwait(false);
        return surface.EncodeJpeg(quality);
    }

    public static async ValueTask<byte[]> RenderFitJpegAsync(
        RenderScene scene,
        int pixelWidth,
        int pixelHeight,
        int quality = 85,
        double density = 1d,
        double paddingFraction = 0.08,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (scene.WorldBounds is not { } bounds) throw new ArgumentException("Cannot fit-render an empty scene.", nameof(scene));

        var camera = Camera2D.Fit(bounds, pixelWidth, pixelHeight, paddingFraction);
        return await RenderCameraJpegAsync(scene, camera, quality, density, cancellationToken).ConfigureAwait(false);
    }
}
