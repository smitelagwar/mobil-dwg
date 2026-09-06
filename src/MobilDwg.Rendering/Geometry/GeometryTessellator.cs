using System.Collections.ObjectModel;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.References;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Geometry;

public sealed record GeometryTessellationOptions
{
    public GeometryTessellationOptions(
        double maxChordError,
        int minSegments = 4,
        int maxSegments = 4096,
        int splineSegmentsPerSpan = 8)
    {
        if (!double.IsFinite(maxChordError) || maxChordError <= 0) throw new ArgumentOutOfRangeException(nameof(maxChordError));
        if (minSegments < 1) throw new ArgumentOutOfRangeException(nameof(minSegments));
        if (maxSegments < minSegments) throw new ArgumentOutOfRangeException(nameof(maxSegments));
        if (splineSegmentsPerSpan < 1) throw new ArgumentOutOfRangeException(nameof(splineSegmentsPerSpan));

        MaxChordError = maxChordError;
        MinSegments = minSegments;
        MaxSegments = maxSegments;
        SplineSegmentsPerSpan = splineSegmentsPerSpan;
    }

    public double MaxChordError { get; }
    public int MinSegments { get; }
    public int MaxSegments { get; }
    public int SplineSegmentsPerSpan { get; }

    public static readonly GeometryTessellationOptions Default = new(0.1, 4, 1024, 8);
}

public sealed record TessellatedPath
{
    private readonly ReadOnlyCollection<WorldPoint2> _points;

    public TessellatedPath(IEnumerable<WorldPoint2> points, bool closed, bool filled)
    {
        ArgumentNullException.ThrowIfNull(points);
        var copy = points.ToArray();
        if (copy.Length == 0) throw new ArgumentException("A tessellated path requires at least one point.", nameof(points));
        if (closed && copy.Length < 3) throw new ArgumentException("A closed path requires at least three points.", nameof(points));
        _points = Array.AsReadOnly(copy);
        Closed = closed;
        Filled = filled;
    }

    public IReadOnlyList<WorldPoint2> Points => _points;
    public bool Closed { get; }
    public bool Filled { get; }
}

public static class GeometryTessellator
{
    public static TessellatedPath Tessellate(RenderGeometryPrimitive primitive, GeometryTessellationOptions options)
    {
        ArgumentNullException.ThrowIfNull(primitive);
        ArgumentNullException.ThrowIfNull(options);

        return primitive switch
        {
            PointPrimitive point => new TessellatedPath([point.Position], closed: false, filled: false),
            LinePrimitive line => new TessellatedPath([line.Start, line.End], closed: false, filled: false),
            ArcPrimitive arc => TessellateArc(arc, options),
            EllipsePrimitive ellipse => TessellateEllipse(ellipse, options),
            PolylinePrimitive polyline => TessellatePolyline(polyline, options),
            PolygonPrimitive polygon => new TessellatedPath(polygon.Vertices, closed: true, filled: true),
            SplinePrimitive spline => TessellateSpline(spline, options),
            TextPrimitive text => new TessellatedPath([text.Position], closed: false, filled: false),
            HatchPrimitive hatch => hatch.Loops.Count > 0 && hatch.Loops[0].Vertices.Count >= 3
                ? new TessellatedPath(hatch.Loops[0].Vertices, closed: true, filled: hatch.IsSolid)
                : new TessellatedPath([new WorldPoint2(0, 0)], closed: false, filled: false),
            ViewportPrimitive vp => new TessellatedPath(
                [
                    new WorldPoint2(vp.PaperBounds.MinX, vp.PaperBounds.MinY),
                    new WorldPoint2(vp.PaperBounds.MaxX, vp.PaperBounds.MinY),
                    new WorldPoint2(vp.PaperBounds.MaxX, vp.PaperBounds.MaxY),
                    new WorldPoint2(vp.PaperBounds.MinX, vp.PaperBounds.MaxY)
                ], closed: true, filled: false),
            MissingReferencePrimitive missing => new TessellatedPath(
                [
                    new WorldPoint2(missing.PlaceholderBounds.MinX, missing.PlaceholderBounds.MinY),
                    new WorldPoint2(missing.PlaceholderBounds.MaxX, missing.PlaceholderBounds.MinY),
                    new WorldPoint2(missing.PlaceholderBounds.MaxX, missing.PlaceholderBounds.MaxY),
                    new WorldPoint2(missing.PlaceholderBounds.MinX, missing.PlaceholderBounds.MaxY)
                ], closed: true, filled: false),
            RasterImagePrimitive img => new TessellatedPath(
                [
                    new WorldPoint2(img.ImageBounds.MinX, img.ImageBounds.MinY),
                    new WorldPoint2(img.ImageBounds.MaxX, img.ImageBounds.MinY),
                    new WorldPoint2(img.ImageBounds.MaxX, img.ImageBounds.MaxY),
                    new WorldPoint2(img.ImageBounds.MinX, img.ImageBounds.MaxY)
                ], closed: true, filled: false),
            _ => throw new NotSupportedException($"Unsupported geometry primitive: {primitive.GetType().Name}"),
        };
    }

    private static TessellatedPath TessellateArc(ArcPrimitive arc, GeometryTessellationOptions options)
    {
        var count = SegmentCount(arc.Radius, Math.Abs(arc.SweepRadians), options);
        var points = new WorldPoint2[count + 1];
        for (var i = 0; i <= count; i++)
        {
            var t = (double)i / count;
            var angle = arc.StartRadians + (arc.SweepRadians * t);
            points[i] = new WorldPoint2(
                arc.Center.X + (arc.Radius * Math.Cos(angle)),
                arc.Center.Y + (arc.Radius * Math.Sin(angle)));
        }
        return new TessellatedPath(points, closed: IsFullSweep(arc.SweepRadians), filled: false);
    }

    private static TessellatedPath TessellateEllipse(EllipsePrimitive ellipse, GeometryTessellationOptions options)
    {
        var radius = Math.Max(ellipse.MajorRadius, ellipse.MinorRadius);
        var count = SegmentCount(radius, Math.Abs(ellipse.SweepParameter), options);
        var points = new WorldPoint2[count + 1];
        for (var i = 0; i <= count; i++)
        {
            var t = (double)i / count;
            points[i] = ellipse.Evaluate(ellipse.StartParameter + (ellipse.SweepParameter * t));
        }
        return new TessellatedPath(points, closed: IsFullSweep(ellipse.SweepParameter), filled: false);
    }

    private static TessellatedPath TessellatePolyline(PolylinePrimitive polyline, GeometryTessellationOptions options)
    {
        var points = new List<WorldPoint2> { polyline.Vertices[0].Position };
        var segmentCount = polyline.Closed ? polyline.Vertices.Count : polyline.Vertices.Count - 1;
        for (var i = 0; i < segmentCount; i++)
        {
            var start = polyline.Vertices[i];
            var end = polyline.Vertices[(i + 1) % polyline.Vertices.Count];
            if (PointsEqual(start.Position, end.Position))
            {
                continue;
            }
            if (start.Bulge == 0)
            {
                AppendWithoutDuplicate(points, end.Position);
                continue;
            }

            var arc = GeometryBounds.BulgeArc(start.Position, end.Position, start.Bulge);
            var arcPath = TessellateArc(arc, options);
            for (var pointIndex = 1; pointIndex < arcPath.Points.Count; pointIndex++)
            {
                AppendWithoutDuplicate(points, arcPath.Points[pointIndex]);
            }
        }

        if (polyline.Closed && points.Count > 1 && PointsEqual(points[0], points[^1])) points.RemoveAt(points.Count - 1);
        return new TessellatedPath(points, closed: polyline.Closed, filled: false);
    }

    private static TessellatedPath TessellateSpline(SplinePrimitive spline, GeometryTessellationOptions options)
    {
        var nonEmptySpans = new List<(double Start, double End)>();
        for (var i = spline.Degree; i < spline.ControlPoints.Count; i++)
        {
            var start = spline.Knots[i];
            var end = spline.Knots[i + 1];
            if (end > start) nonEmptySpans.Add((start, end));
        }
        if (nonEmptySpans.Count == 0) throw new InvalidOperationException("Spline has no non-empty knot span.");

        var points = new List<WorldPoint2>();
        int maxSegments = Math.Max(options.MinSegments, options.MaxSegments);
        double maxChordError = Math.Max(1e-9, options.MaxChordError);

        for (var spanIndex = 0; spanIndex < nonEmptySpans.Count; spanIndex++)
        {
            var (spanStart, spanEnd) = nonEmptySpans[spanIndex];
            var pStart = spline.Evaluate(spanStart);
            var pEnd = spline.Evaluate(spanEnd);

            if (points.Count == 0 || !PointsEqual(points[^1], pStart))
            {
                points.Add(pStart);
            }

            SubdivideSplineSpan(spline, spanStart, spanEnd, pStart, pEnd, maxChordError, points, maxSegments, depth: 0);
        }

        return new TessellatedPath(points, closed: false, filled: false);
    }

    private static void SubdivideSplineSpan(
        SplinePrimitive spline,
        double uStart,
        double uEnd,
        WorldPoint2 pStart,
        WorldPoint2 pEnd,
        double maxChordError,
        List<WorldPoint2> points,
        int maxTotalPoints,
        int depth)
    {
        if (points.Count >= maxTotalPoints || depth >= 12)
        {
            if (!PointsEqual(points[^1], pEnd)) points.Add(pEnd);
            return;
        }

        // Multi-point sampling (0.25, 0.50, 0.75) to reliably capture high curvature, inflections and weighted poles
        double u1 = uStart + (0.25 * (uEnd - uStart));
        double uMid = uStart + (0.50 * (uEnd - uStart));
        double u3 = uStart + (0.75 * (uEnd - uStart));

        var p1 = spline.Evaluate(u1);
        var pMid = spline.Evaluate(uMid);
        var p3 = spline.Evaluate(u3);

        double d1 = PointToSegmentDistance(p1, pStart, pEnd);
        double dMid = PointToSegmentDistance(pMid, pStart, pEnd);
        double d3 = PointToSegmentDistance(p3, pStart, pEnd);

        double maxDeviation = Math.Max(d1, Math.Max(dMid, d3));

        if (maxDeviation <= maxChordError && depth >= 1)
        {
            if (!PointsEqual(points[^1], pEnd)) points.Add(pEnd);
            return;
        }

        SubdivideSplineSpan(spline, uStart, uMid, pStart, pMid, maxChordError, points, maxTotalPoints, depth + 1);
        SubdivideSplineSpan(spline, uMid, uEnd, pMid, pEnd, maxChordError, points, maxTotalPoints, depth + 1);
    }

    private static double PointToSegmentDistance(WorldPoint2 p, WorldPoint2 a, WorldPoint2 b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lenSq = (dx * dx) + (dy * dy);
        if (lenSq < 1e-24)
        {
            var dax = p.X - a.X;
            var day = p.Y - a.Y;
            return Math.Sqrt((dax * dax) + (day * day));
        }
        var cross = Math.Abs((dy * p.X) - (dx * p.Y) + (b.X * a.Y) - (b.Y * a.X));
        return cross / Math.Sqrt(lenSq);
    }

    private static int SegmentCount(double radius, double sweep, GeometryTessellationOptions options)
    {
        if (!double.IsFinite(radius) || radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius));
        if (!double.IsFinite(sweep) || sweep <= 0) throw new ArgumentOutOfRangeException(nameof(sweep));

        double maxAngle;
        if (options.MaxChordError >= radius)
        {
            maxAngle = Math.PI / 2d;
        }
        else
        {
            var cosine = 1d - (options.MaxChordError / radius);
            cosine = Math.Clamp(cosine, -1d, 1d);
            maxAngle = 2d * Math.Acos(cosine);
            if (!double.IsFinite(maxAngle) || maxAngle <= 0) maxAngle = sweep;
        }

        var requested = (int)Math.Ceiling(sweep / maxAngle);
        return Math.Clamp(requested, options.MinSegments, options.MaxSegments);
    }

    private static bool IsFullSweep(double sweep) => Math.Abs(Math.Abs(sweep) - GeometryMath.Tau) <= 1e-12;

    private static void AppendWithoutDuplicate(List<WorldPoint2> points, WorldPoint2 point)
    {
        if (points.Count == 0 || !PointsEqual(points[^1], point)) points.Add(point);
    }

    private static bool PointsEqual(WorldPoint2 a, WorldPoint2 b) => a.X == b.X && a.Y == b.Y;
}
