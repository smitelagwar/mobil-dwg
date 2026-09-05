using System.Runtime.CompilerServices;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;

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

        var acceptanceScene = BuildAcceptanceScene(reverseInsertion: false);
        var acceptanceSceneReversed = BuildAcceptanceScene(reverseInsertion: true);
        var snapshot = P0GeometrySemanticSnapshot.Create(acceptanceScene);
        var reversedSnapshot = P0GeometrySemanticSnapshot.Create(acceptanceSceneReversed);
        Assert(snapshot == reversedSnapshot, "source-order P0 semantic golden must be insertion-order independent");
        Assert(snapshot.StartsWith("p0-geometry/v1\n", StringComparison.Ordinal), "P0 semantic snapshot version");
        Assert(snapshot.Contains("primitive=LINE|-30,-20|30,-20", StringComparison.Ordinal), "P0 semantic line golden");
        Assert(snapshot.Contains("primitive=ARC|0,0|18|0|3.141592653589793", StringComparison.Ordinal), "P0 semantic arc golden");
        Assert(snapshot.Contains("primitive=POLYLINE|0|-28,18,0.5;-10,30,0;8,18,0", StringComparison.Ordinal), "P0 bulge semantic golden");
        Assert(snapshot.Contains("primitive=SPLINE|2|-25,0;-5,35;25,5|0,0,0,1,1,1|1,1,1", StringComparison.Ordinal), "P0 spline semantic golden");
        Assert(snapshot.Contains("diagnostic=Dropped|P0_INVALID_GEOMETRY_DROPPED||Invalid source geometry is reported instead of silently rendered.", StringComparison.Ordinal), "invalid geometry must have controlled diagnostic golden");

        using (var surface = new SkiaBitmapRenderSurface(640, 480))
        {
            var camera = Camera2D.Fit(acceptanceScene.WorldBounds!.Value, 640, 480, paddingFraction: 0.08);
            new SkiaCadRenderer().RenderAsync(acceptanceScene, surface, camera.ToViewport()).GetAwaiter().GetResult();
            var background = acceptanceScene.ColorContext.BackgroundArgb;
            var nonBackground = surface.Bitmap.Pixels.Count(pixel => pixel.Alpha != 0 && ToArgb(pixel) != background);
            Assert(nonBackground > 500, $"Skia acceptance render must contain expected foreground content; pixels={nonBackground}");
            var png = surface.EncodePng();
            Assert(png.Length > 1024, "Skia acceptance PNG must be non-trivial");
            Assert(png[0] == 0x89 && png[1] == 0x50 && png[2] == 0x4E && png[3] == 0x47, "Skia acceptance output must be PNG");
        }

        Console.WriteLine("STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS");
        Console.WriteLine("STAGE10_TESSELLATION_PRECISION_TESTS_PASS");
        Console.WriteLine("STAGE10_P0_SEMANTIC_GOLDEN_PASS");
        Console.WriteLine("STAGE10_CONTROLLED_INVALID_GEOMETRY_WARNING_PASS");
        Console.WriteLine("STAGE10_SKIA_EXPECTED_CONTENT_HOST_PASS");
    }

    internal static RenderScene BuildAcceptanceScene(bool reverseInsertion)
    {
        var entities = new[]
        {
            Entity("P0-LINE", "LINE", 1, [new LinePrimitive(new WorldPoint2(-30, -20), new WorldPoint2(30, -20))]),
            Entity("P0-ARC", "ARC", 2, [new ArcPrimitive(new WorldPoint2(0, 0), 18, 0, Math.PI)]),
            Entity("P0-CIRCLE", "CIRCLE", 3, [new ArcPrimitive(new WorldPoint2(30, 15), 10, 0, Math.PI * 2d)]),
            Entity("P0-ELLIPSE", "ELLIPSE", 4, [new EllipsePrimitive(new WorldPoint2(0, -2), 16, 7, Math.PI / 5d)]),
            Entity("P0-POINT", "POINT", 5, [new PointPrimitive(new WorldPoint2(-32, 30))]),
            Entity("P0-LWPOLYLINE", "LWPOLYLINE", 6, [new PolylinePrimitive([
                new PolylineVertex(new WorldPoint2(-28, 18), 0.5),
                new PolylineVertex(new WorldPoint2(-10, 30)),
                new PolylineVertex(new WorldPoint2(8, 18)),
            ])]),
            Entity("P0-SPLINE", "SPLINE", 7, [new SplinePrimitive(2,
                [new WorldPoint2(-25, 0), new WorldPoint2(-5, 35), new WorldPoint2(25, 5)],
                [0d, 0d, 0d, 1d, 1d, 1d])]),
            Entity("P0-SOLID", "SOLID", 8, [new PolygonPrimitive([
                new WorldPoint2(15, 25), new WorldPoint2(35, 25), new WorldPoint2(28, 40), new WorldPoint2(18, 38),
            ])]),
            Entity("P0-TRACE", "TRACE", 9, [new PolygonPrimitive([
                new WorldPoint2(-5, -35), new WorldPoint2(8, -35), new WorldPoint2(10, -28), new WorldPoint2(-8, -28),
            ])]),
            Entity("P0-3DFACE", "3DFACE", 10, [new PolygonPrimitive([
                new WorldPoint2(20, -35), new WorldPoint2(38, -32), new WorldPoint2(34, -22), new WorldPoint2(22, -24),
            ])]),
        };

        var builder = new RenderSceneAssembler(RenderColorContext.Dark);
        foreach (var entity in reverseInsertion ? entities.Reverse() : entities) builder.AddEntity(entity);
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

    private static uint ToArgb(SkiaSharp.SKColor color) =>
        ((uint)color.Alpha << 24) | ((uint)color.Red << 16) | ((uint)color.Green << 8) | color.Blue;

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
