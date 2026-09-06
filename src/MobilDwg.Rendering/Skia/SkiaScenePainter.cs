using System;
using System.Collections.Generic;
using System.IO;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Hatch;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.References;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Spatial;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Rendering.Skia;

public static class SkiaScenePainter
{
    [ThreadStatic]
    private static SKPathBuilder? t_cachedPolyBuilder;

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

            var maxChordError = RenderQualityPolicy.GetMaxChordError(snapshot.QualityMode, camera.WorldUnitsPerPixel);
            var lodBand = RenderQualityPolicy.ComputeLodBand(camera.WorldUnitsPerPixel);
            var tessellation = new GeometryTessellationOptions(maxChordError, minSegments: 4, maxSegments: 4096, splineSegmentsPerSpan: 12);

            IReadOnlyList<int>? candidateIndices = null;
            if (context.EnableOptimization)
            {
                // Screen stroke margin: Max stroke in CAD (2.11mm = 211 hundredths of mm) / 2 + 2px AA margin
                var maxStrokePixels = CadLineweight.FromHundredthsOfMm(211).ToPixels(context.Density, displayLineweights: true);
                var marginPixels = (maxStrokePixels / 2.0) + 2.0;
                var marginWorld = marginPixels * camera.WorldUnitsPerPixel;

                var baseBounds = camera.GetVisibleWorldBounds(paddingFraction: 0d);
                var queryBounds = new WorldBounds2(
                    baseBounds.MinX - marginWorld,
                    baseBounds.MinY - marginWorld,
                    baseBounds.MaxX + marginWorld,
                    baseBounds.MaxY + marginWorld);

                var candidates = new List<int>();
                var metrics = new SpatialQueryMetrics();
                scene.SpatialIndex.Query(queryBounds, candidates, ref metrics);
                candidateIndices = candidates;
            }

            // Adaptive sub-pixel culling threshold in world units:
            // Interaction mode: 1.5 to 2.0 screen pixels (scales with scene density)
            // Final mode: 0.25 screen pixels (microscopic subpixel noise)
            double basePixelThreshold = snapshot.QualityMode == RenderQualityMode.Interaction
                ? (scene.Entities.Count > 40_000 ? 2.0 : 1.5)
                : 0.25;
            double subPixelThresholdWorld = basePixelThreshold * camera.WorldUnitsPerPixel;

            var isInteraction = snapshot.QualityMode == RenderQualityMode.Interaction;
            var resourceCache = snapshot.ResourceCache;
            var geometryCache = snapshot.GeometryCache;

            int count = candidateIndices?.Count ?? scene.Entities.Count;
            for (var candidateIdx = 0; candidateIdx < count; candidateIdx++)
            {
                var entityIndex = candidateIndices != null ? candidateIndices[candidateIdx] : candidateIdx;
                var entity = scene.Entities[entityIndex];

                // Sub-pixel culling: cull entities whose full bounding dimensions are below threshold
                var entBounds = entity.Bounds;
                if (entBounds.Width < subPixelThresholdWorld && entBounds.Height < subPixelThresholdWorld)
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
                bool ownsPathEffect = false;
                if (!isInteraction && resolved.DashPatternPixels is { Length: > 0 } pattern)
                {
                    if (resourceCache != null)
                    {
                        pathEffect = resourceCache.GetOrCreateDashEffect(pattern);
                    }
                    else
                    {
                        pathEffect = SKPathEffect.CreateDash(pattern, 0);
                        ownsPathEffect = true;
                    }
                    strokePaint.PathEffect = pathEffect;
                }
                else
                {
                    strokePaint.PathEffect = null;
                }

                try
                {
                    for (var primIdx = 0; primIdx < entity.Geometry.Count; primIdx++)
                    {
                        var primitive = entity.Geometry[primIdx];
                        string? primitiveKey = null;
                        if (geometryCache != null &&
                            (primitive is ArcPrimitive or EllipsePrimitive or SplinePrimitive or HatchPrimitive or ViewportPrimitive))
                        {
                            primitiveKey = $"{entity.Id.Value}:{primIdx}";
                        }

                        DrawPrimitive(
                            canvas,
                            primitive,
                            camera,
                            tessellation,
                            strokePaint,
                            fillPaint,
                            context.Density,
                            context.EnableOptimization,
                            primitiveKey,
                            snapshot.SceneRevision,
                            lodBand,
                            snapshot.QualityMode,
                            geometryCache,
                            resourceCache);
                    }
                }
                finally
                {
                    if (pathEffect is not null)
                    {
                        strokePaint.PathEffect = null;
                        if (ownsPathEffect)
                        {
                            pathEffect.Dispose();
                        }
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
        bool enableOptimization,
        string? primitiveKey = null,
        long sceneRevision = 1,
        int lodBand = 0,
        RenderQualityMode qualityMode = RenderQualityMode.Final,
        PreparedGeometryCache? geometryCache = null,
        RenderResourceCache? resourceCache = null)
    {
        if (primitive is TextPrimitive textPrimitive)
        {
            DrawTextPrimitive(canvas, textPrimitive, camera, fillPaint, strokePaint, qualityMode);
            return;
        }

        if (primitive is HatchPrimitive hatchPrimitive)
        {
            DrawHatchPrimitive(canvas, hatchPrimitive, camera, strokePaint, fillPaint, qualityMode, geometryCache, primitiveKey, sceneRevision, lodBand);
            return;
        }

        if (primitive is ViewportPrimitive viewportPrimitive)
        {
            DrawViewportPrimitive(canvas, viewportPrimitive, camera, tessellation, strokePaint, fillPaint, density, enableOptimization, geometryCache, resourceCache, sceneRevision, lodBand, qualityMode, primitiveKey);
            return;
        }

        if (primitive is MissingReferencePrimitive missingRef)
        {
            DrawMissingReferencePrimitive(canvas, missingRef, camera, strokePaint, density);
            return;
        }

        if (primitive is ReferencePlaceholderPrimitive placeholder)
        {
            DrawReferencePlaceholderPrimitive(canvas, placeholder, camera, strokePaint, density);
            return;
        }

        if (primitive is RasterImagePrimitive rasterImg)
        {
            DrawRasterImagePrimitive(canvas, rasterImg, camera, strokePaint, density, resourceCache);
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
                var screenRadius = ToFloat(arc.Radius / camera.WorldUnitsPerPixel);
                var minRadius = qualityMode == RenderQualityMode.Interaction ? 1.0f : 0.05f;
                if (screenRadius > minRadius)
                {
                    var screenCenter = CameraTransform.WorldToScreen(arc.Center, camera);
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
                    if (polyline.Vertices.Count == 2)
                    {
                        var lineP0 = CameraTransform.WorldToScreen(polyline.Vertices[0].Position, camera);
                        var lineP1 = CameraTransform.WorldToScreen(polyline.Vertices[1].Position, camera);
                        canvas.DrawLine(ToFloat(lineP0.X), ToFloat(lineP0.Y), ToFloat(lineP1.X), ToFloat(lineP1.Y), strokePaint);
                        return;
                    }

                    var polyBuilder = t_cachedPolyBuilder ??= new SKPathBuilder();
                    polyBuilder.Reset();
                    var startPt = CameraTransform.WorldToScreen(polyline.Vertices[0].Position, camera);
                    polyBuilder.MoveTo(ToFloat(startPt.X), ToFloat(startPt.Y));
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

        TessellatedPath path;
        if (enableOptimization && geometryCache != null && primitiveKey != null &&
            (primitive is ArcPrimitive or EllipsePrimitive or SplinePrimitive or PolylinePrimitive))
        {
            if (geometryCache.TryGet(sceneRevision, primitiveKey, lodBand, tessellation.MaxChordError, out var cached) && cached != null)
            {
                path = cached.Path;
            }
            else
            {
                path = GeometryTessellator.Tessellate(primitive, tessellation);
                geometryCache.Put(sceneRevision, primitiveKey, lodBand, path, tessellation.MaxChordError, primitive.Bounds.Center);
                System.Threading.Interlocked.Increment(ref geometryCache.TessellationCount);
            }
        }
        else
        {
            path = GeometryTessellator.Tessellate(primitive, tessellation);
        }

        if (primitive is PointPrimitive)
        {
            var screen = CameraTransform.WorldToScreen(path.Points[0], camera);
            canvas.DrawCircle(ToFloat(screen.X), ToFloat(screen.Y), Math.Max(2f, (float)(2d * density)), fillPaint);
            return;
        }

        var builder = t_cachedPolyBuilder ??= new SKPathBuilder();
        builder.Reset();
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
        SKPaint fillPaint,
        SKPaint strokePaint,
        RenderQualityMode qualityMode)
    {
        if (string.IsNullOrEmpty(textPrimitive.Text)) return;

        var screenHeight = textPrimitive.Height / camera.WorldUnitsPerPixel;
        if (screenHeight < 0.5d) return;

        if (qualityMode == RenderQualityMode.Interaction)
        {
            if (screenHeight < 1.5d) return;
            if (screenHeight < 6.0d)
            {
                var p0 = CameraTransform.WorldToScreen(textPrimitive.Position, camera);
                var estLen = (textPrimitive.Text.Length * textPrimitive.Height * 0.75 * textPrimitive.WidthFactor) / camera.WorldUnitsPerPixel;
                canvas.DrawLine(ToFloat(p0.X), ToFloat(p0.Y), ToFloat(p0.X + estLen), ToFloat(p0.Y), strokePaint);
                return;
            }
        }

        var screenPos = CameraTransform.WorldToScreen(textPrimitive.Position, camera);
        var typeface = FontSubstitutionResolver.GetSkiaTypeface(textPrimitive.ResolvedFont);

        using var font = new SKFont(typeface, ToFloat(screenHeight));
        font.ScaleX = ToFloat(textPrimitive.WidthFactor * (textPrimitive.MirrorFlags.HasFlag(CadTextMirrorFlags.Backward) ? -1d : 1d));
        font.SkewX = ToFloat(-Math.Tan(textPrimitive.ObliqueAngleRadians));

        var textAlign = textPrimitive.HorizontalAlignment switch
        {
            CadTextHorizontalAlignment.Center or CadTextHorizontalAlignment.Middle => SKTextAlign.Center,
            CadTextHorizontalAlignment.Right => SKTextAlign.Right,
            _ => SKTextAlign.Left,
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

            var lines = textPrimitive.Layout.Lines;
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                float lineScreenY = ToFloat(-line.OffsetY / camera.WorldUnitsPerPixel);
                canvas.DrawText(line.Text, 0f, lineScreenY, textAlign, font, fillPaint);
            }
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
        SKPaint fillPaint,
        RenderQualityMode qualityMode,
        PreparedGeometryCache? geometryCache = null,
        string? primitiveKey = null,
        long sceneRevision = 1,
        int lodBand = 0)
    {
        if (hatch.Loops.Count == 0) return;

        using var builder = new SKPathBuilder();
        builder.FillType = hatch.IslandStyle == HatchIslandStyle.Normal
            ? SKPathFillType.EvenOdd
            : SKPathFillType.Winding;

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
            return;
        }

        // Draw boundary loop outline
        canvas.DrawPath(skPath, strokePaint);

        // Thinning rule: project spacing onto screen pixels.
        // When projected spacing < 3.0 px, thin by step = ceil(3.0 / projectedSpacing)
        var projectedSpacingPixels = hatch.PatternScale / camera.WorldUnitsPerPixel;
        int thinningStep = 1;
        if (projectedSpacingPixels < 3.0 && projectedSpacingPixels > 0)
        {
            thinningStep = (int)Math.Ceiling(3.0 / projectedSpacingPixels);
        }
        if (qualityMode == RenderQualityMode.Interaction)
        {
            thinningStep = Math.Max(thinningStep, 8);
        }

        // Try to obtain coverage from PreparedGeometryCache
        IReadOnlyList<(WorldPoint2 Start, WorldPoint2 End)>? cachedLines = null;
        var visibleBounds = camera.GetVisibleWorldBounds(paddingFraction: 0.05);

        if (geometryCache != null && primitiveKey != null)
        {
            var queryBounds = hatch.Bounds.Intersect(visibleBounds);
            if (geometryCache.TryGetHatchCoverage(sceneRevision, primitiveKey, queryBounds, lodBand, 0, out var coverageEntry) && coverageEntry != null)
            {
                cachedLines = coverageEntry.Lines;
            }
            else if (qualityMode == RenderQualityMode.Final && queryBounds.Width > 0 && queryBounds.Height > 0)
            {
                var coverageGen = HatchProcessor.GeneratePatternLines(
                    hatch.Loops,
                    hatch.PatternAngleRadians,
                    hatch.PatternScale,
                    queryBounds,
                    hatch.PatternOrigin,
                    hatch.IslandStyle);

                var linePairs = coverageGen.Select(l => (l.Start, l.End)).ToList();
                geometryCache.PutHatchCoverage(sceneRevision, primitiveKey, queryBounds, linePairs, lodBand, 0);
                cachedLines = linePairs;
            }
        }

        if (cachedLines != null)
        {
            for (var i = 0; i < cachedLines.Count; i++)
            {
                if (thinningStep > 1 && (i % thinningStep) != 0) continue;
                var pair = cachedLines[i];
                var p1 = CameraTransform.WorldToScreen(pair.Start, camera);
                var p2 = CameraTransform.WorldToScreen(pair.End, camera);
                canvas.DrawLine(ToFloat(p1.X), ToFloat(p1.Y), ToFloat(p2.X), ToFloat(p2.Y), strokePaint);
            }
        }
        else
        {
            var lineCount = hatch.PatternLines.Count;
            for (var i = 0; i < lineCount; i++)
            {
                if (thinningStep > 1 && (i % thinningStep) != 0) continue;
                var line = hatch.PatternLines[i];
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
        bool enableOptimization,
        PreparedGeometryCache? geometryCache = null,
        RenderResourceCache? resourceCache = null,
        long sceneRevision = 1,
        int lodBand = 0,
        RenderQualityMode qualityMode = RenderQualityMode.Final,
        string? primitiveKey = null)
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

            for (var innerIdx = 0; innerIdx < vp.InnerPrimitives.Count; innerIdx++)
            {
                var inner = vp.InnerPrimitives[innerIdx];
                var innerKey = $"{primitiveKey ?? "vp"}:inner:{innerIdx}";
                DrawPrimitive(
                    canvas,
                    inner,
                    camera,
                    tessellation,
                    strokePaint,
                    fillPaint,
                    density,
                    enableOptimization,
                    innerKey,
                    sceneRevision,
                    lodBand,
                    qualityMode,
                    geometryCache,
                    resourceCache);
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

    private static void DrawReferencePlaceholderPrimitive(
        SKCanvas canvas,
        ReferencePlaceholderPrimitive placeholder,
        Camera2D camera,
        SKPaint strokePaint,
        double density)
    {
        var b = placeholder.Bounds;
        var p00 = CameraTransform.WorldToScreen(new WorldPoint2(b.MinX, b.MinY), camera);
        var p10 = CameraTransform.WorldToScreen(new WorldPoint2(b.MaxX, b.MinY), camera);
        var p11 = CameraTransform.WorldToScreen(new WorldPoint2(b.MaxX, b.MaxY), camera);
        var p01 = CameraTransform.WorldToScreen(new WorldPoint2(b.MinX, b.MaxY), camera);

        canvas.DrawLine(ToFloat(p00.X), ToFloat(p00.Y), ToFloat(p10.X), ToFloat(p10.Y), strokePaint);
        canvas.DrawLine(ToFloat(p10.X), ToFloat(p10.Y), ToFloat(p11.X), ToFloat(p11.Y), strokePaint);
        canvas.DrawLine(ToFloat(p11.X), ToFloat(p11.Y), ToFloat(p01.X), ToFloat(p01.Y), strokePaint);
        canvas.DrawLine(ToFloat(p01.X), ToFloat(p01.Y), ToFloat(p00.X), ToFloat(p00.Y), strokePaint);

        canvas.DrawLine(ToFloat(p00.X), ToFloat(p00.Y), ToFloat(p11.X), ToFloat(p11.Y), strokePaint);
        canvas.DrawLine(ToFloat(p01.X), ToFloat(p01.Y), ToFloat(p10.X), ToFloat(p10.Y), strokePaint);

        var label = $"[{placeholder.ReferenceType}: {placeholder.ReferenceName}]";
        using var font = new SKFont(SKTypeface.Default, Math.Max(10f, (float)(10d * density)));
        canvas.DrawText(label, ToFloat(p00.X + 4), ToFloat(p00.Y - 6), SKTextAlign.Left, font, strokePaint);
    }

    private static void DrawRasterImagePrimitive(
        SKCanvas canvas,
        RasterImagePrimitive raster,
        Camera2D camera,
        SKPaint strokePaint,
        double density,
        RenderResourceCache? resourceCache = null)
    {
        SKBitmap? bitmap = null;
        var cacheKey = raster.ResolvedPath ?? raster.ReferenceId;
        bool isCached = false;

        if (resourceCache != null && !string.IsNullOrEmpty(cacheKey))
        {
            if (resourceCache.TryGetRaster(cacheKey, out var cachedBmp) && cachedBmp != null)
            {
                bitmap = cachedBmp;
                isCached = true;
            }
        }

        if (bitmap == null)
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

            var decoded = SKBitmap.Decode(bytes);
            if (decoded == null) return;

            if (resourceCache != null && !string.IsNullOrEmpty(cacheKey))
            {
                bool admitted = resourceCache.PutRaster(cacheKey, decoded);
                bitmap = decoded;
                isCached = admitted;
            }
            else
            {
                bitmap = decoded;
            }
        }

        try
        {
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
        finally
        {
            if (!isCached)
            {
                bitmap.Dispose();
            }
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
