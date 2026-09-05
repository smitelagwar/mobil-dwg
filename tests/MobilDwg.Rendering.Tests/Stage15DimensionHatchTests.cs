using System.Globalization;
using System.Runtime.CompilerServices;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Dimensions;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Hatch;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;

internal static class Stage15DimensionHatchTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestAnonymousDimensionBlockPreferred();
        TestAlignedDimensionProcedural();
        TestRotatedLinearDimensionProcedural();
        TestRadialDimensionProcedural();
        TestDiametricDimensionProcedural();
        TestDegenerateDimensionIdenticalDefpoints();
        TestDegenerateDimensionNanCoordinates();
        TestLeaderAndMultiLeader();
        TestHatchAutoClosureWithinTolerance();
        TestHatchBrokenBoundaryDiagnostic();
        TestHatchEvenOddNestedIslands();
        TestHatchANSI31PatternLinesGenerated();
        TestDimensionHatchSemanticSnapshotDeterminism();

        Console.WriteLine("STAGE15_DIMENSION_HATCH_TESTS_PASS");
    }

    private static void TestAnonymousDimensionBlockPreferred()
    {
        var blockDef = new BlockDefinition(
            "*D100",
            basePoint: new WorldPoint2(0, 0),
            entities: [
                new BlockEntityTemplate(
                    new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(50, 0)),
                    new RenderLayerToken("0"),
                    new RenderStyleToken("BYLAYER"))
            ]);

        var blockTable = new Dictionary<string, BlockDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["*D100"] = blockDef
        };

        var def = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Linear,
            defPoint1: new WorldPoint2(0, 0),
            defPoint2: new WorldPoint2(50, 0),
            dimensionLinePoint: new WorldPoint2(25, 10),
            anonymousBlockName: "*D100");

        var diagnostics = new List<SceneDiagnostic>();
        var entity = DimensionBuilder.BuildDimension("DIM_ANON", def, blockTable: blockTable, diagnostics: diagnostics);

        Assert(entity.Geometry.Count == 1, "Anonymous block expansion should yield exactly 1 entity primitive.");
        Assert(entity.Geometry[0] is LinePrimitive, "Expected line primitive from anonymous dimension block.");
        Assert(diagnostics.Count == 0, "No diagnostics expected for valid anonymous block.");
    }

    private static void TestAlignedDimensionProcedural()
    {
        var def = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Aligned,
            defPoint1: new WorldPoint2(10, 20),
            defPoint2: new WorldPoint2(110, 20),
            dimensionLinePoint: new WorldPoint2(60, 40),
            textHeight: 2.5);

        var entity = DimensionBuilder.BuildDimension("DIM_ALIGNED", def);

        Assert(entity.Geometry.Count >= 4, "Procedural dimension must produce lines, arrowheads, and text.");
        var textPrim = entity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        Assert(textPrim != null, "TextPrimitive must be generated for aligned dimension.");
        Assert(textPrim!.Text.Contains("100.00"), $"Dimension text should format measurement '100.00', got '{textPrim.Text}'");
        
        var arrowheadPrims = entity.Geometry.OfType<PolygonPrimitive>().ToList();
        Assert(arrowheadPrims.Count == 2, $"Expected 2 arrowhead polygons, got {arrowheadPrims.Count}");
    }

    private static void TestRotatedLinearDimensionProcedural()
    {
        var def = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Linear,
            defPoint1: new WorldPoint2(0, 0),
            defPoint2: new WorldPoint2(60, 80),
            dimensionLinePoint: new WorldPoint2(30, 100),
            rotationRadians: 0d);

        var entity = DimensionBuilder.BuildDimension("DIM_ROTATED", def);

        Assert(entity.Geometry.Count >= 4, "Rotated linear dimension must produce geometry.");
        var textPrim = entity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        Assert(textPrim != null, "TextPrimitive expected.");
        Assert(textPrim!.Text.Contains("60.00"), $"Rotated linear dimension should project measurement along angle, got '{textPrim!.Text}'");
    }

    private static void TestRadialDimensionProcedural()
    {
        var def = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Radial,
            defPoint1: new WorldPoint2(50, 50),
            defPoint2: new WorldPoint2(80, 50),
            textHeight: 3.0);

        var entity = DimensionBuilder.BuildDimension("DIM_RADIAL", def);

        var textPrim = entity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        Assert(textPrim != null, "TextPrimitive expected for radial dimension.");
        Assert(textPrim!.Text.StartsWith("R", StringComparison.Ordinal), $"Radial dimension text must start with 'R', got '{textPrim.Text}'");
        Assert(entity.Geometry.OfType<PolygonPrimitive>().Count() == 1, "Radial dimension should have 1 arrowhead.");
    }

    private static void TestDiametricDimensionProcedural()
    {
        var def = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Diametric,
            defPoint1: new WorldPoint2(20, 50),
            defPoint2: new WorldPoint2(80, 50),
            textHeight: 3.0);

        var entity = DimensionBuilder.BuildDimension("DIM_DIAMETRIC", def);

        var textPrim = entity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        Assert(textPrim != null, "TextPrimitive expected for diametric dimension.");
        Assert(textPrim!.Text.StartsWith("Ø", StringComparison.Ordinal), $"Diametric dimension text must start with 'Ø', got '{textPrim.Text}'");
        Assert(entity.Geometry.OfType<PolygonPrimitive>().Count() == 2, "Diametric dimension should have 2 arrowheads.");
    }

    private static void TestDegenerateDimensionIdenticalDefpoints()
    {
        var def = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Aligned,
            defPoint1: new WorldPoint2(25, 25),
            defPoint2: new WorldPoint2(25, 25),
            dimensionLinePoint: new WorldPoint2(25, 25));

        var diagnostics = new List<SceneDiagnostic>();
        var entity = DimensionBuilder.BuildDimension("DIM_DEGENERATE_1", def, diagnostics: diagnostics);

        Assert(entity.Geometry.Count == 0, "Degenerate dimension must produce empty geometry.");
        Assert(diagnostics.Any(d => d.Code == "DEGENERATE_DIMENSION_POINTS" || d.Code == "INVALID_DIMENSION_GEOMETRY"), "Expected DEGENERATE_DIMENSION_POINTS or INVALID_DIMENSION_GEOMETRY diagnostic.");
    }

    private static void TestDegenerateDimensionNanCoordinates()
    {
        var diagnostics = new List<SceneDiagnostic>();
        var entity = DimensionBuilder.TryBuildFromRaw(
            "DIM_DEGENERATE_NAN",
            CadDimensionType.Linear,
            def1X: double.NaN, def1Y: 0,
            def2X: 10, def2Y: 0,
            dimLineX: 5, dimLineY: 5,
            diagnostics: diagnostics);

        Assert(entity.Geometry.Count == 0, "NaN dimension must produce empty geometry.");
        Assert(diagnostics.Any(d => d.Code == "INVALID_DIMENSION_GEOMETRY"), "Expected INVALID_DIMENSION_GEOMETRY diagnostic.");
    }

    private static void TestLeaderAndMultiLeader()
    {
        var vertices = new[]
        {
            new WorldPoint2(10, 10),
            new WorldPoint2(30, 40),
            new WorldPoint2(60, 40)
        };

        var entity = LeaderBuilder.BuildLeader(
            "LEADER_01",
            vertices,
            annotationText: "DETAY A-A",
            textHeight: 3.5,
            arrowheadSize: 3.0,
            doglegLength: 10.0);

        Assert(entity.Geometry.Count >= 3, "Leader must contain arrow, polyline/lines, and text.");
        var textPrim = entity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        Assert(textPrim != null, "Leader must have annotation text.");
        Assert(textPrim!.Text == "DETAY A-A", $"Leader annotation text mismatch: '{textPrim.Text}'");
        Assert(entity.Geometry.OfType<PolygonPrimitive>().Any(), "Leader must have arrowhead polygon.");
    }

    private static void TestHatchAutoClosureWithinTolerance()
    {
        // Boundary with tiny gap (0.0005 units <= 0.001 tolerance)
        var rawVertices = new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(100, 0),
            new WorldPoint2(100, 100),
            new WorldPoint2(0, 100),
            new WorldPoint2(0, 0.0005)
        };

        var diagnostics = new List<SceneDiagnostic>();
        var loop = HatchProcessor.ValidateAndCloseLoop(rawVertices, isOuter: true, diagnostics: diagnostics);

        Assert(loop != null, "Loop should be successfully closed within tolerance.");
        Assert(diagnostics.Count == 0, "No error diagnostics should be emitted for gap within tolerance.");
        var first = loop!.Vertices[0];
        var last = loop.Vertices[^1];
        AssertNear(first.X, last.X, 1e-6, "Loop should be closed at start point x");
        AssertNear(first.Y, last.Y, 1e-6, "Loop should be closed at start point y");
    }

    private static void TestHatchBrokenBoundaryDiagnostic()
    {
        // Boundary with large gap (5 units > 0.001 tolerance)
        var rawVertices = new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(100, 0),
            new WorldPoint2(100, 100),
            new WorldPoint2(0, 100),
            new WorldPoint2(0, 5.0)
        };

        var diagnostics = new List<SceneDiagnostic>();
        var loop = HatchProcessor.ValidateAndCloseLoop(rawVertices, isOuter: true, diagnostics: diagnostics);

        Assert(diagnostics.Any(d => d.Code == "HATCH_BROKEN_BOUNDARY"), "Expected HATCH_BROKEN_BOUNDARY diagnostic for gap > tolerance.");
    }

    private static void TestHatchEvenOddNestedIslands()
    {
        var outerVertices = new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(200, 0),
            new WorldPoint2(200, 200),
            new WorldPoint2(0, 200),
            new WorldPoint2(0, 0)
        };

        var islandVertices = new[]
        {
            new WorldPoint2(50, 50),
            new WorldPoint2(150, 50),
            new WorldPoint2(150, 150),
            new WorldPoint2(50, 150),
            new WorldPoint2(50, 50)
        };

        var outerLoop = new HatchLoop(outerVertices, isOuter: true);
        var islandLoop = new HatchLoop(islandVertices, isOuter: false);

        var hatchPrim = new HatchPrimitive(
            loops: [outerLoop, islandLoop],
            patternName: "SOLID",
            islandStyle: HatchIslandStyle.Normal,
            isSolid: true);

        Assert(hatchPrim.Loops.Count == 2, "Hatch must have 2 loops.");
        Assert(hatchPrim.IslandStyle == HatchIslandStyle.Normal, "Island style must be Normal (EvenOdd).");

        // Skia rendering test of solid hatch with island
        var entity = new RenderSceneEntity(
            new RenderEntityId("HATCH_EVENODD"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("HATCH"),
            [hatchPrim]);

        var builder = new RenderSceneAssembler(RenderColorContext.Dark);
        builder.AddEntity(entity);
        var scene = builder.Build();

        var renderResult = SkiaScenePngRenderer.RenderFitWithStatsAsync(scene, 400, 400).AsTask().GetAwaiter().GetResult();
        Assert(renderResult.NonBackgroundPixels > 1000, "Rendered hatch must produce non-background pixels.");
    }

    private static void TestHatchANSI31PatternLinesGenerated()
    {
        var outerVertices = new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(100, 0),
            new WorldPoint2(100, 100),
            new WorldPoint2(0, 100),
            new WorldPoint2(0, 0)
        };
        var loop = new HatchLoop(outerVertices, isOuter: true);

        var patternLines = HatchProcessor.GeneratePatternLines(
            [loop],
            angleRadians: Math.PI / 4d, // 45 degrees
            spacing: 10.0,
            bounds: loop.Bounds);

        Assert(patternLines.Count > 0, "ANSI31 pattern should generate lines.");
        Assert(patternLines.Count <= 2048, "Pattern lines must not exceed safety budget.");

        // Check that lines are roughly at 45 degree slope (dx ~ dy)
        foreach (var line in patternLines)
        {
            var dx = Math.Abs(line.End.X - line.Start.X);
            var dy = Math.Abs(line.End.Y - line.Start.Y);
            AssertNear(dx, dy, 1e-4, "45 degree pattern line must have equal absolute dx and dy.");
        }
    }

    private static void TestDimensionHatchSemanticSnapshotDeterminism()
    {
        var dimDef = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Aligned,
            defPoint1: new WorldPoint2(0, 0),
            defPoint2: new WorldPoint2(50, 0),
            dimensionLinePoint: new WorldPoint2(25, 10));
        var dimEntity = DimensionBuilder.BuildDimension("DIM_01", dimDef);

        var hatchLoop = new HatchLoop(
            [new WorldPoint2(0, 0), new WorldPoint2(10, 0), new WorldPoint2(10, 10), new WorldPoint2(0, 10), new WorldPoint2(0, 0)],
            isOuter: true);
        var hatchEntity = new RenderSceneEntity(
            new RenderEntityId("HATCH_01"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("HATCH"),
            [new HatchPrimitive([hatchLoop], patternName: "SOLID", isSolid: true)]);

        var sceneA = new RenderSceneAssembler(RenderColorContext.Dark);
        sceneA.AddEntity(dimEntity);
        sceneA.AddEntity(hatchEntity);

        var sceneB = new RenderSceneAssembler(RenderColorContext.Dark);
        sceneB.AddEntity(hatchEntity);
        sceneB.AddEntity(dimEntity);

        var snapA = DimensionHatchSemanticSnapshot.Create(sceneA.Build());
        var snapB = DimensionHatchSemanticSnapshot.Create(sceneB.Build());

        Assert(snapA == snapB, "Snapshot must be deterministic and invariant to insertion order.");
        Assert(snapA.Contains("schema=dim-hatch/v1", StringComparison.Ordinal), "Snapshot schema tag required.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertNear(double actual, double expected, double tolerance, string message)
    {
        if (!double.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
        {
            throw new InvalidOperationException($"{message}: expected={expected:R}, actual={actual:R}, tolerance={tolerance:R}");
        }
    }
}
