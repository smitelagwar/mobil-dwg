using System.Collections.ObjectModel;
using MobilDwg.Rendering.Layouts;
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
        var points = new List<WorldPoint2>();
        var nonEmptySpans = new List<(double Start, double End)>();
        for (var i = spline.Degree; i < spline.ControlPoints.Count; i++)
        {
            var start = spline.Knots[i];
            var end = spline.Knots[i + 1];
            if (end > start) nonEmptySpans.Add((start, end));
        }
        if (nonEmptySpans.Count == 0) throw new InvalidOperationException("Spline has no non-empty knot span.");

        var segmentsPerSpan = Math.Max(options.SplineSegmentsPerSpan, options.MinSegments);
        var requested = checked(nonEmptySpans.Count * segmentsPerSpan);
        var totalSegments = Math.Min(requested, options.MaxSegments);
        var allocated = 0;
        for (var spanIndex = 0; spanIndex < nonEmptySpans.Count; spanIndex++)
        {
            var spansRemaining = nonEmptySpans.Count - spanIndex;
            var segmentsRemaining = totalSegments - allocated;
            var spanSegments = Math.Max(1, segmentsRemaining / spansRemaining);
            var (start, end) = nonEmptySpans[spanIndex];
            for (var i = 0; i <= spanSegments; i++)
            {
                if (spanIndex > 0 && i == 0) continue;
                var t = (double)i / spanSegments;
                points.Add(spline.Evaluate(start + ((end - start) * t)));
            }
            allocated += spanSegments;
        }

        return new TessellatedPath(points, closed: false, filled: false);
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
