using System.Collections.ObjectModel;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Geometry;

public abstract record RenderGeometryPrimitive
{
    public abstract WorldBounds2 Bounds { get; }
}

public sealed record PointPrimitive : RenderGeometryPrimitive
{
    public PointPrimitive(WorldPoint2 position) => Position = position;
    public WorldPoint2 Position { get; }
    public override WorldBounds2 Bounds => new(Position.X, Position.Y, Position.X, Position.Y);
}

public sealed record LinePrimitive : RenderGeometryPrimitive
{
    public LinePrimitive(WorldPoint2 start, WorldPoint2 end)
    {
        Start = start;
        End = end;
    }

    public WorldPoint2 Start { get; }
    public WorldPoint2 End { get; }
    public override WorldBounds2 Bounds => GeometryBounds.FromPoints([Start, End]);
}

public sealed record ArcPrimitive : RenderGeometryPrimitive
{
    public ArcPrimitive(WorldPoint2 center, double radius, double startRadians, double sweepRadians)
    {
        if (!double.IsFinite(radius) || radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius));
        if (!double.IsFinite(startRadians)) throw new ArgumentOutOfRangeException(nameof(startRadians));
        if (!double.IsFinite(sweepRadians) || sweepRadians == 0 || Math.Abs(sweepRadians) > GeometryMath.Tau + 1e-12)
            throw new ArgumentOutOfRangeException(nameof(sweepRadians));

        Center = center;
        Radius = radius;
        StartRadians = startRadians;
        SweepRadians = sweepRadians;
        Bounds = GeometryBounds.ForArc(center, radius, startRadians, sweepRadians);
    }

    public WorldPoint2 Center { get; }
    public double Radius { get; }
    public double StartRadians { get; }
    public double SweepRadians { get; }
    public override WorldBounds2 Bounds { get; }
}

public sealed record EllipsePrimitive : RenderGeometryPrimitive
{
    public EllipsePrimitive(
        WorldPoint2 center,
        double majorRadius,
        double minorRadius,
        double rotationRadians,
        double startParameter = 0,
        double sweepParameter = GeometryMath.Tau)
    {
        if (!double.IsFinite(majorRadius) || majorRadius <= 0) throw new ArgumentOutOfRangeException(nameof(majorRadius));
        if (!double.IsFinite(minorRadius) || minorRadius <= 0) throw new ArgumentOutOfRangeException(nameof(minorRadius));
        if (!double.IsFinite(rotationRadians)) throw new ArgumentOutOfRangeException(nameof(rotationRadians));
        if (!double.IsFinite(startParameter)) throw new ArgumentOutOfRangeException(nameof(startParameter));
        if (!double.IsFinite(sweepParameter) || sweepParameter == 0 || Math.Abs(sweepParameter) > GeometryMath.Tau + 1e-12)
            throw new ArgumentOutOfRangeException(nameof(sweepParameter));

        Center = center;
        MajorRadius = majorRadius;
        MinorRadius = minorRadius;
        RotationRadians = rotationRadians;
        StartParameter = startParameter;
        SweepParameter = sweepParameter;
        Bounds = GeometryBounds.ForEllipse(this);
    }

    public WorldPoint2 Center { get; }
    public double MajorRadius { get; }
    public double MinorRadius { get; }
    public double RotationRadians { get; }
    public double StartParameter { get; }
    public double SweepParameter { get; }
    public override WorldBounds2 Bounds { get; }

    public WorldPoint2 Evaluate(double parameter)
    {
        if (!double.IsFinite(parameter)) throw new ArgumentOutOfRangeException(nameof(parameter));
        var cosT = Math.Cos(parameter);
        var sinT = Math.Sin(parameter);
        var cosR = Math.Cos(RotationRadians);
        var sinR = Math.Sin(RotationRadians);
        return new WorldPoint2(
            Center.X + (MajorRadius * cosT * cosR) - (MinorRadius * sinT * sinR),
            Center.Y + (MajorRadius * cosT * sinR) + (MinorRadius * sinT * cosR));
    }
}

public readonly record struct PolylineVertex
{
    public PolylineVertex(WorldPoint2 position, double bulge = 0)
    {
        if (!double.IsFinite(bulge)) throw new ArgumentOutOfRangeException(nameof(bulge));
        Position = position;
        Bulge = bulge;
    }

    public WorldPoint2 Position { get; }
    public double Bulge { get; }
}

public sealed record PolylinePrimitive : RenderGeometryPrimitive
{
    private readonly ReadOnlyCollection<PolylineVertex> _vertices;

    public PolylinePrimitive(IEnumerable<PolylineVertex> vertices, bool closed = false, double maxWidth = 0.0)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        var copy = vertices.ToArray();
        if (copy.Length < 2) throw new ArgumentException("Polyline requires at least two vertices.", nameof(vertices));
        if (closed && copy.Length == 2 && copy[0].Bulge == 0 && copy[1].Bulge == 0)
        {
            closed = false;
        }

        _vertices = Array.AsReadOnly(copy);
        Closed = closed;
        MaxWidth = Math.Max(0.0, maxWidth);
        Bounds = GeometryBounds.ForPolyline(copy, closed, MaxWidth);
    }

    public IReadOnlyList<PolylineVertex> Vertices => _vertices;
    public bool Closed { get; }
    public double MaxWidth { get; }
    public override WorldBounds2 Bounds { get; }
}

public sealed record PolygonPrimitive : RenderGeometryPrimitive
{
    private readonly ReadOnlyCollection<WorldPoint2> _vertices;

    public PolygonPrimitive(IEnumerable<WorldPoint2> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        var copy = vertices.ToArray();
        if (copy.Length < 3) throw new ArgumentException("Polygon requires at least three vertices.", nameof(vertices));
        _vertices = Array.AsReadOnly(copy);
        Bounds = GeometryBounds.FromPoints(copy);
    }

    public IReadOnlyList<WorldPoint2> Vertices => _vertices;
    public override WorldBounds2 Bounds { get; }
}

public sealed record SplinePrimitive : RenderGeometryPrimitive
{
    private readonly ReadOnlyCollection<WorldPoint2> _controlPoints;
    private readonly ReadOnlyCollection<double> _knots;
    private readonly ReadOnlyCollection<double> _weights;

    public SplinePrimitive(
        int degree,
        IEnumerable<WorldPoint2> controlPoints,
        IEnumerable<double> knots,
        IEnumerable<double>? weights = null)
    {
        ArgumentNullException.ThrowIfNull(controlPoints);
        ArgumentNullException.ThrowIfNull(knots);

        var controlCopy = controlPoints.ToArray();
        var knotCopy = knots.ToArray();
        if (degree < 1) throw new ArgumentOutOfRangeException(nameof(degree));
        if (controlCopy.Length < degree + 1) throw new ArgumentException("Spline has too few control points for its degree.", nameof(controlPoints));
        if (knotCopy.Length != controlCopy.Length + degree + 1)
            throw new ArgumentException("Knot count must equal control point count + degree + 1.", nameof(knots));
        for (var i = 0; i < knotCopy.Length; i++)
        {
            if (!double.IsFinite(knotCopy[i])) throw new ArgumentOutOfRangeException(nameof(knots));
            if (i > 0 && knotCopy[i] < knotCopy[i - 1]) throw new ArgumentException("Knots must be nondecreasing.", nameof(knots));
        }

        var weightCopy = weights?.ToArray() ?? Enumerable.Repeat(1d, controlCopy.Length).ToArray();
        if (weightCopy.Length != controlCopy.Length) throw new ArgumentException("Weight count must equal control point count.", nameof(weights));
        if (weightCopy.Any(weight => !double.IsFinite(weight) || weight <= 0))
            throw new ArgumentOutOfRangeException(nameof(weights), "Spline weights must be finite and positive.");

        var domainStart = knotCopy[degree];
        var domainEnd = knotCopy[controlCopy.Length];
        if (!(domainEnd > domainStart)) throw new ArgumentException("Spline knot domain must have positive length.", nameof(knots));

        Degree = degree;
        _controlPoints = Array.AsReadOnly(controlCopy);
        _knots = Array.AsReadOnly(knotCopy);
        _weights = Array.AsReadOnly(weightCopy);
        DomainStart = domainStart;
        DomainEnd = domainEnd;
        Bounds = GeometryBounds.FromPoints(controlCopy);
    }

    public int Degree { get; }
    public IReadOnlyList<WorldPoint2> ControlPoints => _controlPoints;
    public IReadOnlyList<double> Knots => _knots;
    public IReadOnlyList<double> Weights => _weights;
    public double DomainStart { get; }
    public double DomainEnd { get; }
    public override WorldBounds2 Bounds { get; }

    public WorldPoint2 Evaluate(double parameter)
    {
        if (!double.IsFinite(parameter) || parameter < DomainStart || parameter > DomainEnd)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        var n = _controlPoints.Count - 1;
        var span = parameter >= DomainEnd ? n : FindSpan(parameter, n, Degree, _knots);
        var d = new HomogeneousPoint[Degree + 1];
        for (var j = 0; j <= Degree; j++)
        {
            var index = span - Degree + j;
            var weight = _weights[index];
            var point = _controlPoints[index];
            d[j] = new HomogeneousPoint(point.X * weight, point.Y * weight, weight);
        }

        for (var r = 1; r <= Degree; r++)
        {
            for (var j = Degree; j >= r; j--)
            {
                var index = span - Degree + j;
                var denominator = _knots[index + Degree - r + 1] - _knots[index];
                var alpha = denominator == 0 ? 0 : (parameter - _knots[index]) / denominator;
                d[j] = HomogeneousPoint.Lerp(d[j - 1], d[j], alpha);
            }
        }

        var result = d[Degree];
        if (!double.IsFinite(result.W) || result.W <= 0) throw new InvalidOperationException("Spline evaluation produced an invalid homogeneous weight.");
        return new WorldPoint2(result.X / result.W, result.Y / result.W);
    }

    private static int FindSpan(double parameter, int n, int degree, IReadOnlyList<double> knots)
    {
        if (parameter >= knots[n + 1]) return n;
        var low = degree;
        var high = n + 1;
        var mid = (low + high) / 2;
        while (parameter < knots[mid] || parameter >= knots[mid + 1])
        {
            if (parameter < knots[mid]) high = mid;
            else low = mid;
            mid = (low + high) / 2;
        }
        return mid;
    }

    private readonly record struct HomogeneousPoint(double X, double Y, double W)
    {
        public static HomogeneousPoint Lerp(HomogeneousPoint a, HomogeneousPoint b, double t) => new(
            a.X + ((b.X - a.X) * t),
            a.Y + ((b.Y - a.Y) * t),
            a.W + ((b.W - a.W) * t));
    }
}

public sealed record ReferencePlaceholderPrimitive : RenderGeometryPrimitive
{
    public ReferencePlaceholderPrimitive(
        WorldBounds2 bounds,
        string referenceName,
        string referenceType,
        bool isResolved,
        string statusMessage)
    {
        Bounds = bounds;
        ReferenceName = referenceName;
        ReferenceType = referenceType;
        IsResolved = isResolved;
        StatusMessage = statusMessage;
    }

    public override WorldBounds2 Bounds { get; }
    public string ReferenceName { get; }
    public string ReferenceType { get; }
    public bool IsResolved { get; }
    public string StatusMessage { get; }
}

internal static class GeometryMath
{
    public const double Tau = Math.PI * 2d;

    public static bool AngleIsOnSweep(double candidate, double start, double sweep)
    {
        var distance = sweep > 0
            ? NormalizePositive(candidate - start)
            : NormalizePositive(start - candidate);
        return distance <= Math.Abs(sweep) + 1e-12;
    }

    public static double NormalizePositive(double angle)
    {
        var normalized = angle % Tau;
        return normalized < 0 ? normalized + Tau : normalized;
    }
}

internal static class GeometryBounds
{
    public static WorldBounds2 FromPoints(IReadOnlyList<WorldPoint2> points)
    {
        if (points.Count == 0) throw new ArgumentException("At least one point is required.", nameof(points));
        var minX = points[0].X;
        var minY = points[0].Y;
        var maxX = minX;
        var maxY = minY;
        for (var i = 1; i < points.Count; i++)
        {
            minX = Math.Min(minX, points[i].X);
            minY = Math.Min(minY, points[i].Y);
            maxX = Math.Max(maxX, points[i].X);
            maxY = Math.Max(maxY, points[i].Y);
        }
        return new WorldBounds2(minX, minY, maxX, maxY);
    }

    public static WorldBounds2 ForArc(WorldPoint2 center, double radius, double start, double sweep)
    {
        var points = new List<WorldPoint2>
        {
            EvaluateCircle(center, radius, start),
            EvaluateCircle(center, radius, start + sweep),
        };
        foreach (var candidate in new[] { 0d, Math.PI / 2d, Math.PI, 3d * Math.PI / 2d })
        {
            if (GeometryMath.AngleIsOnSweep(candidate, start, sweep)) points.Add(EvaluateCircle(center, radius, candidate));
        }
        return FromPoints(points);
    }

    public static WorldBounds2 ForEllipse(EllipsePrimitive ellipse)
    {
        var candidates = new List<double>
        {
            ellipse.StartParameter,
            ellipse.StartParameter + ellipse.SweepParameter,
        };
        var rotation = ellipse.RotationRadians;
        var xCritical = Math.Atan2(-ellipse.MinorRadius * Math.Sin(rotation), ellipse.MajorRadius * Math.Cos(rotation));
        var yCritical = Math.Atan2(ellipse.MinorRadius * Math.Cos(rotation), ellipse.MajorRadius * Math.Sin(rotation));
        foreach (var candidate in new[] { xCritical, xCritical + Math.PI, yCritical, yCritical + Math.PI })
        {
            if (GeometryMath.AngleIsOnSweep(candidate, ellipse.StartParameter, ellipse.SweepParameter)) candidates.Add(candidate);
        }
        return FromPoints(candidates.Select(ellipse.Evaluate).ToArray());
    }

    public static WorldBounds2 ForPolyline(IReadOnlyList<PolylineVertex> vertices, bool closed, double maxWidth = 0.0)
    {
        var bounds = FromPoints(vertices.Select(vertex => vertex.Position).ToArray());
        var segmentCount = closed ? vertices.Count : vertices.Count - 1;
        for (var i = 0; i < segmentCount; i++)
        {
            var start = vertices[i];
            var end = vertices[(i + 1) % vertices.Count];
            if (start.Bulge == 0) continue;
            var dx = end.Position.X - start.Position.X;
            var dy = end.Position.Y - start.Position.Y;
            if ((dx * dx) + (dy * dy) < 1e-24) continue;
            var arc = BulgeArc(start.Position, end.Position, start.Bulge);
            bounds = bounds.Union(arc.Bounds);
        }

        if (maxWidth > 0.0)
        {
            double halfW = maxWidth / 2.0;
            bounds = new WorldBounds2(
                bounds.MinX - halfW,
                bounds.MinY - halfW,
                bounds.MaxX + halfW,
                bounds.MaxY + halfW);
        }

        return bounds;
    }

    public static ArcPrimitive BulgeArc(WorldPoint2 start, WorldPoint2 end, double bulge)
    {
        if (!double.IsFinite(bulge) || bulge == 0) throw new ArgumentOutOfRangeException(nameof(bulge));
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var chord = Math.Sqrt((dx * dx) + (dy * dy));
        if (!double.IsFinite(chord) || chord <= 1e-12)
        {
            // Degenerate segment fallback: minimal point arc
            return new ArcPrimitive(start, 1e-6, 0d, GeometryMath.Tau);
        }

        var midpointX = (start.X / 2d) + (end.X / 2d);
        var midpointY = (start.Y / 2d) + (end.Y / 2d);
        var offset = chord * (1d - (bulge * bulge)) / (4d * bulge);
        var normalX = -dy / chord;
        var normalY = dx / chord;
        var center = new WorldPoint2(midpointX + (normalX * offset), midpointY + (normalY * offset));
        var radius = chord * (1d + (bulge * bulge)) / (4d * Math.Abs(bulge));
        var startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
        var sweep = 4d * Math.Atan(bulge);
        return new ArcPrimitive(center, radius, startAngle, sweep);
    }

    private static WorldPoint2 EvaluateCircle(WorldPoint2 center, double radius, double angle) => new(
        center.X + (radius * Math.Cos(angle)),
        center.Y + (radius * Math.Sin(angle)));
}
