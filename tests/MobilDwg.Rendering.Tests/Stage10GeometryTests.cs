using System.Runtime.CompilerServices;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

internal static class Stage10GeometryTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        var options = new GeometryTessellationOptions(maxChordError: 0.01, minSegments: 2, maxSegments: 2048, splineSegmentsPerSpan: 8);

        var line = new LinePrimitive(new WorldPoint2(5_000_000d, -2d), new WorldPoint2(5_000_000.001d, 3d));
        var linePath = GeometryTessellator.Tessellate(line, options);
        Assert(linePath.Points.Count == 2, "line tessellation must retain two exact endpoints");
        AssertNear(linePath.Points[1].X - linePath.Points[0].X, 0.001d, 1e-9, "large-coordinate 1 mm line detail");

        var quarterArc = new ArcPrimitive(new WorldPoint2(0, 0), 10, 0, Math.PI / 2d);
        AssertNear(quarterArc.Bounds.MinX, 0, 1e-12, "quarter arc min x");
        AssertNear(quarterArc.Bounds.MinY, 0, 1e-12, "quarter arc min y");
        AssertNear(quarterArc.Bounds.MaxX, 10, 1e-12, "quarter arc max x");
        AssertNear(quarterArc.Bounds.MaxY, 10, 1e-12, "quarter arc max y");
        var arcPath = GeometryTessellator.Tessellate(quarterArc, options);
        Assert(arcPath.Points.Count > 2, "arc must be sampled into multiple deterministic segments");
        AssertNear(arcPath.Points[0].X, 10, 1e-12, "arc start x");
        AssertNear(arcPath.Points[^1].Y, 10, 1e-12, "arc end y");

        var ellipse = new EllipsePrimitive(new WorldPoint2(100, 200), 20, 5, Math.PI / 6d);
        var ellipsePath = GeometryTessellator.Tessellate(ellipse, options);
        Assert(ellipsePath.Closed, "full ellipse must tessellate as closed");
        Assert(ellipsePath.Points.All(point => double.IsFinite(point.X) && double.IsFinite(point.Y)), "ellipse tessellation must remain finite");

        var bulged = new PolylinePrimitive([
            new PolylineVertex(new WorldPoint2(0, 0), bulge: 1),
            new PolylineVertex(new WorldPoint2(10, 0)),
            new PolylineVertex(new WorldPoint2(10, 10)),
        ]);
        var bulgedPath = GeometryTessellator.Tessellate(bulged, options);
        Assert(bulgedPath.Points.Count > 3, "bulged polyline segment must be expanded into an arc");
        AssertNear(bulgedPath.Points[0].X, 0, 1e-12, "bulged polyline start x");
        AssertNear(bulgedPath.Points[^1].Y, 10, 1e-12, "bulged polyline final y");

        var polygon = new PolygonPrimitive([
            new WorldPoint2(-2, -1),
            new WorldPoint2(3, -1),
            new WorldPoint2(0, 4),
        ]);
        var polygonPath = GeometryTessellator.Tessellate(polygon, options);
        Assert(polygonPath.Closed && polygonPath.Filled, "SOLID/TRACE/3DFACE-style polygon path must be closed and fillable");

        var spline = new SplinePrimitive(
            degree: 2,
            controlPoints: [new WorldPoint2(0, 0), new WorldPoint2(5, 10), new WorldPoint2(10, 0)],
            knots: [0d, 0d, 0d, 1d, 1d, 1d]);
        var midpoint = spline.Evaluate(0.5d);
        AssertNear(midpoint.X, 5, 1e-12, "quadratic spline midpoint x");
        AssertNear(midpoint.Y, 5, 1e-12, "quadratic spline midpoint y");
        var splinePath = GeometryTessellator.Tessellate(spline, options);
        Assert(splinePath.Points.Count >= 9, "spline tessellation must sample the non-empty knot span deterministically");
        AssertNear(splinePath.Points[0].X, 0, 1e-12, "spline start x");
        AssertNear(splinePath.Points[^1].X, 10, 1e-12, "spline end x");

        var closedPolyline = new PolylinePrimitive([
            new PolylineVertex(new WorldPoint2(0, 0)),
            new PolylineVertex(new WorldPoint2(2, 0)),
            new PolylineVertex(new WorldPoint2(1, 1)),
        ], closed: true);
        Assert(GeometryTessellator.Tessellate(closedPolyline, options).Closed, "closed polyline must retain closure semantics");

        AssertThrows<ArgumentOutOfRangeException>(() => new ArcPrimitive(new WorldPoint2(0, 0), 1, 0, 0), "zero arc sweep must fail");
        AssertThrows<ArgumentOutOfRangeException>(() => new PolylineVertex(new WorldPoint2(0, 0), double.NaN), "NaN bulge must fail");
        AssertThrows<ArgumentException>(() => new PolylinePrimitive([new PolylineVertex(new WorldPoint2(0, 0))]), "one-vertex polyline must fail");
        AssertThrows<ArgumentException>(() => new SplinePrimitive(2, [new WorldPoint2(0, 0), new WorldPoint2(1, 1), new WorldPoint2(2, 0)], [0d, 0d, 1d, 0.5d, 1d, 1d]), "decreasing spline knots must fail");
        AssertThrows<ArgumentOutOfRangeException>(() => new GeometryTessellationOptions(double.NaN), "NaN chord error must fail");

        Console.WriteLine("STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS");
        Console.WriteLine("STAGE10_TESSELLATION_PRECISION_TESTS_PASS");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertNear(double actual, double expected, double tolerance, string message)
    {
        if (!double.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
            throw new InvalidOperationException($"{message}: expected={expected:R}, actual={actual:R}, tolerance={tolerance:R}");
    }

    private static void AssertThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }
}
