using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Canvas.Dispose();
        Bitmap.Dispose();
    }
}

public sealed class SkiaCadRenderer : ICadRenderer
{
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
        var canvas = skiaSurface.Canvas;
        canvas.Clear(ToSkColor(renderScene.ColorContext.BackgroundArgb));
        var saveCount = canvas.Save();
        try
        {
            canvas.ClipRect(new SKRect(0, 0, surface.PixelWidth, surface.PixelHeight));

            var foreground = ToSkColor(renderScene.ColorContext.DefaultForegroundArgb);
            using var strokePaint = new SKPaint
            {
                Color = foreground,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(1f, (float)surface.Density),
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
            };
            using var fillPaint = new SKPaint
            {
                Color = foreground,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };

            var maxChordError = Math.Max(viewport.WorldUnitsPerPixel * 0.25d, 1e-12);
            var tessellation = new GeometryTessellationOptions(maxChordError, minSegments: 4, maxSegments: 4096, splineSegmentsPerSpan: 12);

            foreach (var entity in renderScene.Entities)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var primitive in entity.Geometry)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    DrawPrimitive(canvas, primitive, camera, tessellation, strokePaint, fillPaint, surface.Density);
                }
            }
        }
        finally
        {
            canvas.RestoreToCount(saveCount);
            canvas.Flush();
        }

        return ValueTask.CompletedTask;
    }

    private static void DrawPrimitive(
        SKCanvas canvas,
        RenderGeometryPrimitive primitive,
        Camera2D camera,
        GeometryTessellationOptions tessellation,
        SKPaint strokePaint,
        SKPaint fillPaint,
        double density)
    {
        var path = GeometryTessellator.Tessellate(primitive, tessellation);
        if (primitive is PointPrimitive)
        {
            var screen = CameraTransform.WorldToScreen(path.Points[0], camera);
            canvas.DrawCircle(ToFloat(screen.X), ToFloat(screen.Y), Math.Max(2f, (float)(2d * density)), fillPaint);
            return;
        }

        using var builder = new SKPathBuilder();
        var first = CameraTransform.WorldToScreen(path.Points[0], camera);
        builder.MoveTo(ToFloat(first.X), ToFloat(first.Y));
        for (var i = 1; i < path.Points.Count; i++)
        {
            var screen = CameraTransform.WorldToScreen(path.Points[i], camera);
            builder.LineTo(ToFloat(screen.X), ToFloat(screen.Y));
        }
        if (path.Closed) builder.Close();
        using var skPath = builder.Detach();

        if (path.Filled) canvas.DrawPath(skPath, fillPaint);
        else canvas.DrawPath(skPath, strokePaint);
    }

    private static float ToFloat(double value)
    {
        if (!double.IsFinite(value) || value < -float.MaxValue || value > float.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), "Screen coordinate cannot be represented as float at the final Skia boundary.");
        return (float)value;
    }

    private static SKColor ToSkColor(uint argb) => new(
        red: (byte)((argb >> 16) & 0xFF),
        green: (byte)((argb >> 8) & 0xFF),
        blue: (byte)(argb & 0xFF),
        alpha: (byte)((argb >> 24) & 0xFF));
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
