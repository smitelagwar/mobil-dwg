using System.Globalization;
using System.Runtime.CompilerServices;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;

internal static class Stage16LayoutViewportTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestModelSpaceLayoutReturnsDirectModelEntities();
        TestPaperSpaceLayoutRendersTitleBlockAndBorder();
        TestViewportModelToPaperTransform();
        TestViewportLayerOverrideHidesFrozenLayers();
        TestViewportTwistAngleRotatesModelGeometry();
        TestViewportClippingAppliedInSkia();
        TestDegenerateViewportZeroDimensionsEmitsDiagnostic();
        TestDegenerateViewportNanCoordinatesEmitsDiagnostic();
        TestZeroReparseLayoutSwitching();
        TestMultipleViewportsOnSingleSheet();
        TestSkiaRenderPaperLayoutWithViewportsProducesPixels();
        TestLayoutSceneSemanticSnapshotDeterminism();

        Console.WriteLine("STAGE16_LAYOUT_VIEWPORT_TESTS_PASS");
    }

    private static void TestModelSpaceLayoutReturnsDirectModelEntities()
    {
        var modelScene = CreateSampleModelScene();
        var manager = new CadLayoutManager(modelScene);

        Assert(manager.ActiveLayoutName == "Model", "Default layout must be Model.");
        Assert(manager.ActiveLayout.IsModelSpace, "Model layout must have IsModelSpace = true.");

        var composed = manager.ComposeActiveScene();
        Assert(ReferenceEquals(composed, modelScene), "Composing Model Space must return ModelSpaceScene directly.");
    }

    private static void TestPaperSpaceLayoutRendersTitleBlockAndBorder()
    {
        var modelScene = CreateSampleModelScene();

        var titleBlockEntity = new RenderSceneEntity(
            new RenderEntityId("TITLE-01"),
            new RenderLayerToken("TITLE"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive("PROJE: FABRIKA BINASI", new WorldPoint2(350, 20), height: 5.0)
            ]);

        var layout = new CadLayoutDefinition(
            "A3-Layout",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 420, 297),
            paperEntities: [titleBlockEntity]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "A3-Layout");
        var composed = manager.ComposeActiveScene();

        Assert(composed.Entities.Any(e => e.Id.Value == "TITLE-01"), "Paper space entity must be included in composed scene.");
    }

    private static void TestViewportModelToPaperTransform()
    {
        var modelEntity = new RenderSceneEntity(
            new RenderEntityId("LINE-01"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0))]);

        var asm = new RenderSceneAssembler();
        asm.AddEntity(modelEntity);
        var modelScene = asm.Build();

        // Viewport on paper at (200, 150), paper size 100x50.
        // Viewing model center (50, 0) with view height 50. Scale = 50 / 50 = 1.
        var viewport = new CadLayoutViewport(
            "VP1",
            paperCenter: new WorldPoint2(200, 150),
            paperWidth: 100,
            paperHeight: 50,
            viewCenter: new WorldPoint2(50, 0),
            viewHeight: 50);

        var layout = new CadLayoutDefinition(
            "Sheet1",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 400, 300),
            viewports: [viewport]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "Sheet1");
        var composed = manager.ComposeActiveScene();

        var vpEntity = composed.Entities.FirstOrDefault(e => e.Source.EntityType == "VIEWPORT");
        Assert(vpEntity != null, "Composed scene must contain VIEWPORT entity.");

        var vpPrim = vpEntity!.Geometry.OfType<ViewportPrimitive>().FirstOrDefault();
        Assert(vpPrim != null, "Viewport entity must contain ViewportPrimitive.");
        Assert(vpPrim!.InnerPrimitives.Count == 1, "Viewport should contain 1 transformed line.");

        var line = vpPrim.InnerPrimitives[0] as LinePrimitive;
        Assert(line != null, "Inner primitive must be LinePrimitive.");

        // Model (0, 0) -> Paper: dx = -50, scale = 1 -> px = 200 - 50 = 150, py = 150
        // Model (100, 0) -> Paper: dx = +50, scale = 1 -> px = 200 + 50 = 250, py = 150
        AssertNear(line!.Start.X, 150.0, 1e-5, "Line start X in paper coords");
        AssertNear(line.Start.Y, 150.0, 1e-5, "Line start Y in paper coords");
        AssertNear(line.End.X, 250.0, 1e-5, "Line end X in paper coords");
        AssertNear(line.End.Y, 150.0, 1e-5, "Line end Y in paper coords");
    }

    private static void TestViewportLayerOverrideHidesFrozenLayers()
    {
        var wallEntity = new RenderSceneEntity(
            new RenderEntityId("WALL-01"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(50, 0))]);

        var dimEntity = new RenderSceneEntity(
            new RenderEntityId("DIM-01"),
            new RenderLayerToken("DIMENSIONS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 10), new WorldPoint2(50, 10))]);

        var asm = new RenderSceneAssembler();
        asm.AddEntity(wallEntity);
        asm.AddEntity(dimEntity);
        var modelScene = asm.Build();

        // Viewport with DIMENSIONS layer frozen
        var viewport = new CadLayoutViewport(
            "VP_FROZEN",
            paperCenter: new WorldPoint2(100, 100),
            paperWidth: 80,
            paperHeight: 60,
            viewCenter: new WorldPoint2(25, 5),
            viewHeight: 20,
            frozenLayers: ["DIMENSIONS"]);

        var layout = new CadLayoutDefinition(
            "Layout1",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 300, 200),
            viewports: [viewport]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "Layout1");
        var composed = manager.ComposeActiveScene();

        var vpEntity = composed.Entities.First(e => e.Source.EntityType == "VIEWPORT");
        var vpPrim = (ViewportPrimitive)vpEntity.Geometry[0];

        // WALLS should be present, DIMENSIONS must be excluded
        Assert(vpPrim.InnerPrimitives.Count == 1, "Frozen layer DIMENSIONS must be omitted from viewport.");
    }

    private static void TestViewportTwistAngleRotatesModelGeometry()
    {
        var modelEntity = new RenderSceneEntity(
            new RenderEntityId("HORIZ-LINE"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(10, 0))]);

        var asm = new RenderSceneAssembler();
        asm.AddEntity(modelEntity);
        var modelScene = asm.Build();

        // 90-degree twist (Math.PI / 2)
        var viewport = new CadLayoutViewport(
            "VP_TWIST",
            paperCenter: new WorldPoint2(100, 100),
            paperWidth: 50,
            paperHeight: 50,
            viewCenter: new WorldPoint2(0, 0),
            viewHeight: 20,
            twistAngleRadians: Math.PI / 2d);

        var layout = new CadLayoutDefinition(
            "LayoutTwist",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 200, 200),
            viewports: [viewport]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "LayoutTwist");
        var composed = manager.ComposeActiveScene();

        var vpEntity = composed.Entities.First(e => e.Source.EntityType == "VIEWPORT");
        var vpPrim = (ViewportPrimitive)vpEntity.Geometry[0];
        var line = (LinePrimitive)vpPrim.InnerPrimitives[0];

        // Horizontal line along X rotated by -90 degrees (-pi/2) rotates to -Y direction
        // Start: (100, 100)
        // End: X remains ~100, Y changes by -scale * 10 = -2.5 * 10 = -25
        AssertNear(line.Start.X, 100.0, 1e-4, "Twisted line start X");
        AssertNear(line.Start.Y, 100.0, 1e-4, "Twisted line start Y");
        AssertNear(line.End.X, 100.0, 1e-4, "Twisted line end X (aligned vertically)");
        AssertNear(Math.Abs(line.End.Y - line.Start.Y), 25.0, 1e-4, "Twisted line length along Y");
    }

    private static void TestViewportClippingAppliedInSkia()
    {
        var vp = new ViewportPrimitive(
            "VP_CLIP",
            paperBounds: new WorldBounds2(50, 50, 150, 150),
            innerPrimitives: [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(200, 200))]);

        var entity = new RenderSceneEntity(
            new RenderEntityId("VP_ENTITY"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("VIEWPORT"),
            [vp]);

        var asm = new RenderSceneAssembler(RenderColorContext.Dark);
        asm.AddEntity(entity);
        var scene = asm.Build();

        // Fit render must execute without crashing and clip within bounds
        var result = SkiaScenePngRenderer.RenderFitWithStatsAsync(scene, 300, 300).AsTask().GetAwaiter().GetResult();
        Assert(result.NonBackgroundPixels > 0, "Clipped viewport must render pixels.");
    }

    private static void TestDegenerateViewportZeroDimensionsEmitsDiagnostic()
    {
        var modelScene = CreateSampleModelScene();

        var zeroVp = new CadLayoutViewport(
            "VP_ZERO",
            paperCenter: new WorldPoint2(50, 50),
            paperWidth: 0, // Degenerate zero width
            paperHeight: 50,
            viewCenter: new WorldPoint2(0, 0),
            viewHeight: 10);

        var layout = new CadLayoutDefinition(
            "LayoutZero",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 100, 100),
            viewports: [zeroVp]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "LayoutZero");
        var diags = new List<SceneDiagnostic>();
        var composed = manager.ComposeActiveScene(diagnostics: diags);

        Assert(diags.Any(d => d.Code == "INVALID_VIEWPORT_GEOMETRY"), "Zero width viewport must emit INVALID_VIEWPORT_GEOMETRY.");
        Assert(!composed.Entities.Any(e => e.Id.Value.Contains("VP_ZERO")), "Degenerate viewport must be skipped.");
    }

    private static void TestDegenerateViewportNanCoordinatesEmitsDiagnostic()
    {
        var modelScene = CreateSampleModelScene();

        var nanVp = new CadLayoutViewport(
            "VP_NAN",
            paperCenter: new WorldPoint2(50, 50),
            paperWidth: 50,
            paperHeight: 50,
            viewCenter: new WorldPoint2(0, 0),
            viewHeight: double.NaN);

        var layout = new CadLayoutDefinition(
            "LayoutNaN",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 100, 100),
            viewports: [nanVp]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "LayoutNaN");
        var diags = new List<SceneDiagnostic>();
        var composed = manager.ComposeActiveScene(diagnostics: diags);

        Assert(diags.Any(d => d.Code == "INVALID_VIEWPORT_GEOMETRY"), "NaN coordinate viewport must emit INVALID_VIEWPORT_GEOMETRY.");
    }

    private static void TestZeroReparseLayoutSwitching()
    {
        var modelScene = CreateSampleModelScene();

        var layout1 = new CadLayoutDefinition("Plan-1", isModelSpace: false, tabOrder: 1, paperBounds: new WorldBounds2(0, 0, 300, 200));
        var layout2 = new CadLayoutDefinition("Plan-2", isModelSpace: false, tabOrder: 2, paperBounds: new WorldBounds2(0, 0, 420, 297));

        var manager = new CadLayoutManager(modelScene, [layout1, layout2]);

        // Switch to Plan-1
        manager.SwitchLayout("Plan-1");
        Assert(manager.ActiveLayoutName == "Plan-1", "Switched to Plan-1");

        // Switch to Plan-2
        manager.SwitchLayout("Plan-2");
        Assert(manager.ActiveLayoutName == "Plan-2", "Switched to Plan-2");

        // Switch back to Model
        manager.SwitchLayout("Model");
        Assert(manager.ActiveLayoutName == "Model", "Switched back to Model");
        Assert(ReferenceEquals(manager.ComposeActiveScene(), modelScene), "ModelSpaceScene instance must be preserved across switches.");
    }

    private static void TestMultipleViewportsOnSingleSheet()
    {
        var modelScene = CreateSampleModelScene();

        // 2 viewports: Overview (1:100) and Detail (1:20)
        var vpOverview = new CadLayoutViewport(
            "VP_OVERVIEW",
            paperCenter: new WorldPoint2(100, 150),
            paperWidth: 120,
            paperHeight: 90,
            viewCenter: new WorldPoint2(50, 50),
            viewHeight: 100);

        var vpDetail = new CadLayoutViewport(
            "VP_DETAIL",
            paperCenter: new WorldPoint2(280, 150),
            paperWidth: 120,
            paperHeight: 90,
            viewCenter: new WorldPoint2(20, 20),
            viewHeight: 20);

        var layout = new CadLayoutDefinition(
            "MultiSheet",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 420, 297),
            viewports: [vpOverview, vpDetail]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "MultiSheet");
        var composed = manager.ComposeActiveScene();

        var viewports = composed.Entities.Where(e => e.Source.EntityType == "VIEWPORT").ToList();
        Assert(viewports.Count == 2, $"Expected 2 viewports, got {viewports.Count}");
    }

    private static void TestSkiaRenderPaperLayoutWithViewportsProducesPixels()
    {
        var modelEntity = new RenderSceneEntity(
            new RenderEntityId("MODEL_BOX"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0)),
                new LinePrimitive(new WorldPoint2(100, 0), new WorldPoint2(100, 100)),
                new LinePrimitive(new WorldPoint2(100, 100), new WorldPoint2(0, 100)),
                new LinePrimitive(new WorldPoint2(0, 100), new WorldPoint2(0, 0))
            ]);

        var asm = new RenderSceneAssembler(RenderColorContext.Dark);
        asm.AddEntity(modelEntity);
        var modelScene = asm.Build();

        var vp = new CadLayoutViewport(
            "VP1",
            paperCenter: new WorldPoint2(210, 148),
            paperWidth: 180,
            paperHeight: 120,
            viewCenter: new WorldPoint2(50, 50),
            viewHeight: 100);

        var sheetBorder = new RenderSceneEntity(
            new RenderEntityId("SHEET_BORDER"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(10, 10), new WorldPoint2(410, 10)),
                new LinePrimitive(new WorldPoint2(410, 10), new WorldPoint2(410, 287)),
                new LinePrimitive(new WorldPoint2(410, 287), new WorldPoint2(10, 287)),
                new LinePrimitive(new WorldPoint2(10, 287), new WorldPoint2(10, 10))
            ]);

        var layout = new CadLayoutDefinition(
            "A3_SHEET",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 420, 297),
            paperEntities: [sheetBorder],
            viewports: [vp]);

        var manager = new CadLayoutManager(modelScene, [layout], activeLayoutName: "A3_SHEET");
        var composedScene = manager.ComposeActiveScene();

        var renderResult = SkiaScenePngRenderer.RenderFitWithStatsAsync(composedScene, 500, 350).AsTask().GetAwaiter().GetResult();
        Assert(renderResult.Png.Length > 0, "Rendered PNG must not be empty.");
        Assert(renderResult.NonBackgroundPixels > 500, $"Rendered non-background pixels should exceed 500, got {renderResult.NonBackgroundPixels}");
    }

    private static void TestLayoutSceneSemanticSnapshotDeterminism()
    {
        var modelScene = CreateSampleModelScene();
        var vp1 = new CadLayoutViewport("VP1", new WorldPoint2(100, 100), 50, 50, new WorldPoint2(0, 0), 20);
        var layout1 = new CadLayoutDefinition("Layout1", isModelSpace: false, tabOrder: 1, paperBounds: new WorldBounds2(0, 0, 200, 200), viewports: [vp1]);

        var managerA = new CadLayoutManager(modelScene, [layout1], activeLayoutName: "Layout1");
        var managerB = new CadLayoutManager(modelScene, [layout1], activeLayoutName: "Layout1");

        var snapA = LayoutSceneSemanticSnapshot.Create(managerA);
        var snapB = LayoutSceneSemanticSnapshot.Create(managerB);

        Assert(snapA == snapB, "Snapshot must be deterministic.");
        Assert(snapA.Contains("schema=layout-scene/v1", StringComparison.Ordinal), "Snapshot schema tag required.");
        Assert(snapA.Contains("active_layout=Layout1", StringComparison.Ordinal), "Active layout tag required.");
    }

    private static RenderScene CreateSampleModelScene()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("SAMPLE-01"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(50, 50))]));
        return assembler.Build();
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
