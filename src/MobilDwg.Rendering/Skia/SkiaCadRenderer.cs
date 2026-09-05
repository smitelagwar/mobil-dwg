using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.References;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;
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
        var visibleWorldBounds = camera.GetVisibleWorldBounds(paddingFraction: 0.05d);
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

                if (OptimizationMode == RenderOptimizationMode.Optimized && !entity.Bounds.Intersects(visibleWorldBounds))
                {
                    continue;
                }

                var resolved = CadStyleResolver.Resolve(
                    entity.CadStyle,
                    entity.Layer,
                    renderScene.LayerTable,
                    renderScene.ColorContext,
                    viewport.WorldUnitsPerPixel,
                    surface.Density,
                    displayLineweights: true);

                if (!resolved.IsVisible)
                {
                    continue;
                }

                var entityColor = ToSkColor(resolved.ArgbColor);
                strokePaint.Color = entityColor;
                strokePaint.StrokeWidth = resolved.StrokeWidthPixels;
                fillPaint.Color = entityColor;

                SKPathEffect? pathEffect = null;
                if (resolved.DashPatternPixels is { Length: > 0 } pattern)
                {
                    pathEffect = SKPathEffect.CreateDash(pattern, 0);
                    strokePaint.PathEffect = pathEffect;
                }
                else
                {
                    strokePaint.PathEffect = null;
                }

                try
                {
                    foreach (var primitive in entity.Geometry)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        DrawPrimitive(canvas, primitive, camera, tessellation, strokePaint, fillPaint, surface.Density);
                    }
                }
                finally
                {
                    if (pathEffect is not null)
                    {
                        strokePaint.PathEffect = null;
                        pathEffect.Dispose();
                    }
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

    private void DrawPrimitive(
        SKCanvas canvas,
        RenderGeometryPrimitive primitive,
        Camera2D camera,
        GeometryTessellationOptions tessellation,
        SKPaint strokePaint,
        SKPaint fillPaint,
        double density)
    {
        if (primitive is TextPrimitive textPrimitive)
        {
            DrawTextPrimitive(canvas, textPrimitive, camera, fillPaint);
            return;
        }

        if (primitive is HatchPrimitive hatchPrimitive)
        {
            DrawHatchPrimitive(canvas, hatchPrimitive, camera, strokePaint, fillPaint);
            return;
        }

        if (primitive is ViewportPrimitive viewportPrimitive)
        {
            DrawViewportPrimitive(canvas, viewportPrimitive, camera, tessellation, strokePaint, fillPaint, density);
            return;
        }

        if (primitive is MissingReferencePrimitive missingRef)
        {
            DrawMissingReferencePrimitive(canvas, missingRef, camera, strokePaint, density);
            return;
        }

        if (primitive is RasterImagePrimitive rasterImg)
        {
            DrawRasterImagePrimitive(canvas, rasterImg, camera, strokePaint, density);
            return;
        }

        if (OptimizationMode == RenderOptimizationMode.Optimized)
        {
            if (primitive is LinePrimitive line)
            {
                var p0 = CameraTransform.WorldToScreen(line.Start, camera);
                var p1 = CameraTransform.WorldToScreen(line.End, camera);
                canvas.DrawLine(ToFloat(p0.X), ToFloat(p0.Y), ToFloat(p1.X), ToFloat(p1.Y), strokePaint);
                return;
            }

            if (primitive is PointPrimitive point)
            {
                var screen = CameraTransform.WorldToScreen(point.Position, camera);
                canvas.DrawCircle(ToFloat(screen.X), ToFloat(screen.Y), Math.Max(2f, (float)(2d * density)), fillPaint);
                return;
            }
        }

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

    private static void DrawTextPrimitive(
        SKCanvas canvas,
        TextPrimitive textPrimitive,
        Camera2D camera,
        SKPaint fillPaint)
    {
        if (string.IsNullOrEmpty(textPrimitive.Text)) return;

        var screenHeight = textPrimitive.Height / camera.WorldUnitsPerPixel;
        if (screenHeight < 0.5d) return;

        var screenPos = CameraTransform.WorldToScreen(textPrimitive.Position, camera);
        var typeface = FontSubstitutionResolver.GetSkiaTypeface(textPrimitive.ResolvedFont);

        using var font = new SKFont(typeface, ToFloat(screenHeight));
        font.ScaleX = ToFloat(textPrimitive.WidthFactor * (textPrimitive.MirrorFlags.HasFlag(CadTextMirrorFlags.Backward) ? -1d : 1d));
        font.SkewX = ToFloat(-Math.Tan(textPrimitive.ObliqueAngleRadians));

        font.MeasureText(textPrimitive.Text, out var textBounds, fillPaint);
        var height = textBounds.Height;

        var textAlign = textPrimitive.HorizontalAlignment switch
        {
            CadTextHorizontalAlignment.Center or CadTextHorizontalAlignment.Middle => SKTextAlign.Center,
            CadTextHorizontalAlignment.Right => SKTextAlign.Right,
            _ => SKTextAlign.Left,
        };

        float offsetY = textPrimitive.VerticalAlignment switch
        {
            CadTextVerticalAlignment.Top => height,
            CadTextVerticalAlignment.Middle => height / 2f,
            CadTextVerticalAlignment.Bottom => 0f,
            _ => 0f,
        };

        var saveCount = canvas.Save();
        try
        {
            canvas.Translate(ToFloat(screenPos.X), ToFloat(screenPos.Y));

            var rotationDegrees = (float)-(textPrimitive.RotationRadians * (180.0 / Math.PI));
            canvas.RotateDegrees(rotationDegrees);

            if (textPrimitive.MirrorFlags.HasFlag(CadTextMirrorFlags.UpsideDown))
            {
                canvas.Scale(1f, -1f);
            }

            canvas.DrawText(textPrimitive.Text, 0f, offsetY, textAlign, font, fillPaint);
        }
        finally
        {
            canvas.RestoreToCount(saveCount);
        }
    }

    private static void DrawHatchPrimitive(
        SKCanvas canvas,
        HatchPrimitive hatch,
        Camera2D camera,
        SKPaint strokePaint,
        SKPaint fillPaint)
    {
        if (hatch.Loops.Count == 0) return;

        using var builder = new SKPathBuilder();
        builder.FillType = SKPathFillType.EvenOdd;
        foreach (var loop in hatch.Loops)
        {
            if (loop.Vertices.Count < 2) continue;
            var start = CameraTransform.WorldToScreen(loop.Vertices[0], camera);
            builder.MoveTo(ToFloat(start.X), ToFloat(start.Y));
            for (var i = 1; i < loop.Vertices.Count; i++)
            {
                var pt = CameraTransform.WorldToScreen(loop.Vertices[i], camera);
                builder.LineTo(ToFloat(pt.X), ToFloat(pt.Y));
            }
            builder.Close();
        }

        using var skPath = builder.Detach();

        if (hatch.IsSolid)
        {
            canvas.DrawPath(skPath, fillPaint);
        }
        else
        {
            canvas.DrawPath(skPath, strokePaint);
            foreach (var line in hatch.PatternLines)
            {
                var p1 = CameraTransform.WorldToScreen(line.Start, camera);
                var p2 = CameraTransform.WorldToScreen(line.End, camera);
                canvas.DrawLine(ToFloat(p1.X), ToFloat(p1.Y), ToFloat(p2.X), ToFloat(p2.Y), strokePaint);
            }
        }
    }

    private void DrawViewportPrimitive(
        SKCanvas canvas,
        ViewportPrimitive vp,
        Camera2D camera,
        GeometryTessellationOptions tessellation,
        SKPaint strokePaint,
        SKPaint fillPaint,
        double density)
    {
        var minScreen = CameraTransform.WorldToScreen(new WorldPoint2(vp.PaperBounds.MinX, vp.PaperBounds.MaxY), camera);
        var maxScreen = CameraTransform.WorldToScreen(new WorldPoint2(vp.PaperBounds.MaxX, vp.PaperBounds.MinY), camera);

        var left = ToFloat(Math.Min(minScreen.X, maxScreen.X));
        var top = ToFloat(Math.Min(minScreen.Y, maxScreen.Y));
        var right = ToFloat(Math.Max(minScreen.X, maxScreen.X));
        var bottom = ToFloat(Math.Max(minScreen.Y, maxScreen.Y));

        var clipRect = new SKRect(left, top, right, bottom);

        var saveCount = canvas.Save();
        try
        {
            if (vp.ClipBoundary != null && vp.ClipBoundary.Count >= 3)
            {
                using var clipBuilder = new SKPathBuilder();
                var p0 = CameraTransform.WorldToScreen(vp.ClipBoundary[0], camera);
                clipBuilder.MoveTo(ToFloat(p0.X), ToFloat(p0.Y));
                for (var i = 1; i < vp.ClipBoundary.Count; i++)
                {
                    var pt = CameraTransform.WorldToScreen(vp.ClipBoundary[i], camera);
                    clipBuilder.LineTo(ToFloat(pt.X), ToFloat(pt.Y));
                }
                clipBuilder.Close();
                using var clipPath = clipBuilder.Detach();
                canvas.ClipPath(clipPath, SKClipOperation.Intersect, antialias: true);
            }
            else
            {
                canvas.ClipRect(clipRect, SKClipOperation.Intersect, antialias: true);
            }

            foreach (var inner in vp.InnerPrimitives)
            {
                DrawPrimitive(canvas, inner, camera, tessellation, strokePaint, fillPaint, density);
            }
        }
        finally
        {
            canvas.RestoreToCount(saveCount);
        }
    }

    private static void DrawMissingReferencePrimitive(
        SKCanvas canvas,
        MissingReferencePrimitive missing,
        Camera2D camera,
        SKPaint strokePaint,
        double density)
    {
        // 1. Draw placeholder border box
        foreach (var borderLine in missing.GenerateBorderLines())
        {
            var p1 = CameraTransform.WorldToScreen(borderLine.Start, camera);
            var p2 = CameraTransform.WorldToScreen(borderLine.End, camera);
            canvas.DrawLine(ToFloat(p1.X), ToFloat(p1.Y), ToFloat(p2.X), ToFloat(p2.Y), strokePaint);
        }

        // 2. Draw diagonal cross
        foreach (var crossLine in missing.GenerateCrossLines())
        {
            var p1 = CameraTransform.WorldToScreen(crossLine.Start, camera);
            var p2 = CameraTransform.WorldToScreen(crossLine.End, camera);
            canvas.DrawLine(ToFloat(p1.X), ToFloat(p1.Y), ToFloat(p2.X), ToFloat(p2.Y), strokePaint);
        }

        // 3. Draw warning text label inside/below placeholder
        var labelWorldPoint = new WorldPoint2(missing.PlaceholderBounds.MinX, missing.PlaceholderBounds.MinY);
        var labelScreen = CameraTransform.WorldToScreen(labelWorldPoint, camera);

        using var font = new SKFont(SKTypeface.Default, Math.Max(10f, (float)(10d * density)));
        canvas.DrawText(missing.Label, ToFloat(labelScreen.X + 4), ToFloat(labelScreen.Y - 6), SKTextAlign.Left, font, strokePaint);
    }

    private static void DrawRasterImagePrimitive(
        SKCanvas canvas,
        RasterImagePrimitive raster,
        Camera2D camera,
        SKPaint strokePaint,
        double density)
    {
        byte[]? bytes = raster.ImageBytes;
        if (bytes == null && raster.ResolvedPath != null && File.Exists(raster.ResolvedPath))
        {
            try
            {
                bytes = File.ReadAllBytes(raster.ResolvedPath);
            }
            catch
            {
                // File read fallback
            }
        }

        if (bytes == null || bytes.Length == 0)
        {
            var b = raster.ImageBounds;
            var p1 = CameraTransform.WorldToScreen(new WorldPoint2(b.MinX, b.MinY), camera);
            var p2 = CameraTransform.WorldToScreen(new WorldPoint2(b.MaxX, b.MaxY), camera);
            canvas.DrawRect(new SKRect(ToFloat(Math.Min(p1.X, p2.X)), ToFloat(Math.Min(p1.Y, p2.Y)), ToFloat(Math.Max(p1.X, p2.X)), ToFloat(Math.Max(p1.Y, p2.Y))), strokePaint);
            return;
        }

        using var bitmap = SKBitmap.Decode(bytes);
        if (bitmap == null) return;

        var saveCount = canvas.Save();
        try
        {
            if (raster.ClipBoundary != null && raster.ClipBoundary.Count >= 3)
            {
                using var clipBuilder = new SKPathBuilder();
                var p0 = CameraTransform.WorldToScreen(raster.ClipBoundary[0], camera);
                clipBuilder.MoveTo(ToFloat(p0.X), ToFloat(p0.Y));
                for (var i = 1; i < raster.ClipBoundary.Count; i++)
                {
                    var pt = CameraTransform.WorldToScreen(raster.ClipBoundary[i], camera);
                    clipBuilder.LineTo(ToFloat(pt.X), ToFloat(pt.Y));
                }
                clipBuilder.Close();
                using var clipPath = clipBuilder.Detach();
                canvas.ClipPath(clipPath, SKClipOperation.Intersect, antialias: true);
            }

            var minScreen = CameraTransform.WorldToScreen(new WorldPoint2(raster.ImageBounds.MinX, raster.ImageBounds.MaxY), camera);
            var maxScreen = CameraTransform.WorldToScreen(new WorldPoint2(raster.ImageBounds.MaxX, raster.ImageBounds.MinY), camera);

            var left = ToFloat(Math.Min(minScreen.X, maxScreen.X));
            var top = ToFloat(Math.Min(minScreen.Y, maxScreen.Y));
            var right = ToFloat(Math.Max(minScreen.X, maxScreen.X));
            var bottom = ToFloat(Math.Max(minScreen.Y, maxScreen.Y));

            var destRect = new SKRect(left, top, right, bottom);

            using var imagePaint = new SKPaint
            {
                IsAntialias = true
            };

            if (raster.Fade > 0)
            {
                var alpha = (byte)Math.Clamp((1d - (raster.Fade / 100d)) * 255d, 0d, 255d);
                imagePaint.Color = new SKColor(255, 255, 255, alpha);
            }

            canvas.DrawBitmap(bitmap, destRect, new SKSamplingOptions(SKFilterMode.Linear), imagePaint);
        }
        finally
        {
            canvas.RestoreToCount(saveCount);
        }
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
