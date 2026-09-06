using System;
using System.IO;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.References;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Rendering.Skia;

public static class SkiaScenePainter
{
    public static void DrawFrame(
        SKCanvas canvas,
        RenderSnapshot snapshot,
        RenderFrameContext context)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(context);

        var scene = snapshot.Scene;
        var camera = snapshot.Camera;
        var layerTable = snapshot.LayerTable ?? scene.LayerTable;

        var visibleWorldBounds = camera.GetVisibleWorldBounds(paddingFraction: 0.05d);

        canvas.Clear(ToSkColor(scene.ColorContext.BackgroundArgb));
        var saveCount = canvas.Save();
        try
        {
            canvas.ClipRect(new SKRect(0, 0, context.PixelWidth, context.PixelHeight));

            var foreground = ToSkColor(scene.ColorContext.DefaultForegroundArgb);
            using var strokePaint = new SKPaint
            {
                Color = foreground,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(1f, (float)context.Density),
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
            };
            using var fillPaint = new SKPaint
            {
                Color = foreground,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };

            var chordFactor = snapshot.QualityMode == RenderQualityMode.Interaction ? 1.0d : 0.25d;
            var maxChordError = Math.Max(camera.WorldUnitsPerPixel * chordFactor, 1e-12);
            var tessellation = new GeometryTessellationOptions(maxChordError, minSegments: 4, maxSegments: 4096, splineSegmentsPerSpan: 12);

            foreach (var entity in scene.Entities)
            {
                if (context.EnableOptimization && !entity.Bounds.Intersects(visibleWorldBounds))
                {
                    continue;
                }

                var resolved = CadStyleResolver.Resolve(
                    entity.CadStyle,
                    entity.Layer,
                    layerTable,
                    scene.ColorContext,
                    camera.WorldUnitsPerPixel,
                    context.Density,
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
                        DrawPrimitive(canvas, primitive, camera, tessellation, strokePaint, fillPaint, context.Density, context.EnableOptimization);
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
    }

    public static void DrawPrimitive(
        SKCanvas canvas,
        RenderGeometryPrimitive primitive,
        Camera2D camera,
        GeometryTessellationOptions tessellation,
        SKPaint strokePaint,
        SKPaint fillPaint,
        double density,
        bool enableOptimization)
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
            DrawViewportPrimitive(canvas, viewportPrimitive, camera, tessellation, strokePaint, fillPaint, density, enableOptimization);
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

        if (enableOptimization)
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

            if (primitive is ArcPrimitive arc)
            {
                var screenCenter = CameraTransform.WorldToScreen(arc.Center, camera);
                var screenRadius = ToFloat(arc.Radius / camera.WorldUnitsPerPixel);
                if (screenRadius > 0.05f)
                {
                    if (Math.Abs(arc.SweepRadians) >= GeometryMath.Tau - 1e-4)
                    {
                        canvas.DrawCircle(ToFloat(screenCenter.X), ToFloat(screenCenter.Y), screenRadius, strokePaint);
                        return;
                    }
                    else
                    {
                        var oval = new SKRect(
                            ToFloat(screenCenter.X - screenRadius),
                            ToFloat(screenCenter.Y - screenRadius),
                            ToFloat(screenCenter.X + screenRadius),
                            ToFloat(screenCenter.Y + screenRadius));
                        var startDeg = ToFloat(-arc.StartRadians * (180.0 / Math.PI));
                        var sweepDeg = ToFloat(-arc.SweepRadians * (180.0 / Math.PI));
                        canvas.DrawArc(oval, startDeg, sweepDeg, false, strokePaint);
                        return;
                    }
                }
                return;
            }

            if (primitive is PolylinePrimitive polyline)
            {
                bool hasBulge = false;
                for (int i = 0; i < polyline.Vertices.Count; i++)
                {
                    if (polyline.Vertices[i].Bulge != 0)
                    {
                        hasBulge = true;
                        break;
                    }
                }

                if (!hasBulge && polyline.Vertices.Count > 1)
                {
                    using var polyBuilder = new SKPathBuilder();
                    var p0 = CameraTransform.WorldToScreen(polyline.Vertices[0].Position, camera);
                    polyBuilder.MoveTo(ToFloat(p0.X), ToFloat(p0.Y));
                    for (int i = 1; i < polyline.Vertices.Count; i++)
                    {
                        var pi = CameraTransform.WorldToScreen(polyline.Vertices[i].Position, camera);
                        polyBuilder.LineTo(ToFloat(pi.X), ToFloat(pi.Y));
                    }
                    if (polyline.Closed) polyBuilder.Close();
                    using var polySkPath = polyBuilder.Detach();
                    canvas.DrawPath(polySkPath, strokePaint);
                    return;
                }
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

    private static void DrawViewportPrimitive(
        SKCanvas canvas,
        ViewportPrimitive vp,
        Camera2D camera,
        GeometryTessellationOptions tessellation,
        SKPaint strokePaint,
        SKPaint fillPaint,
        double density,
        bool enableOptimization)
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
                DrawPrimitive(canvas, inner, camera, tessellation, strokePaint, fillPaint, density, enableOptimization);
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
        foreach (var borderLine in missing.GenerateBorderLines())
        {
            var p1 = CameraTransform.WorldToScreen(borderLine.Start, camera);
            var p2 = CameraTransform.WorldToScreen(borderLine.End, camera);
            canvas.DrawLine(ToFloat(p1.X), ToFloat(p1.Y), ToFloat(p2.X), ToFloat(p2.Y), strokePaint);
        }

        foreach (var crossLine in missing.GenerateCrossLines())
        {
            var p1 = CameraTransform.WorldToScreen(crossLine.Start, camera);
            var p2 = CameraTransform.WorldToScreen(crossLine.End, camera);
            canvas.DrawLine(ToFloat(p1.X), ToFloat(p1.Y), ToFloat(p2.X), ToFloat(p2.Y), strokePaint);
        }

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

    public static SKColor ToSkColor(uint argb) => new(
        red: (byte)((argb >> 16) & 0xFF),
        green: (byte)((argb >> 8) & 0xFF),
        blue: (byte)(argb & 0xFF),
        alpha: (byte)((argb >> 24) & 0xFF));
}
