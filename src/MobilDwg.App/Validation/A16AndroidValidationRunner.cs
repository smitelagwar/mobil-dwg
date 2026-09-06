#if A16_VALIDATION
using System.Security.Cryptography;
using Android.Util;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.App;

public sealed record A16ValidationResult(
    byte[] Png,
    string PngSha256,
    string ActiveLayoutName,
    int EntityCount,
    string Marker);

public static class A16AndroidValidationRunner
{
    public const string Tag = "MobilDwgA16";

    public static async Task<A16ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A16_ANDROID_VALIDATION_STARTING");
        await Task.Delay(250);

        // 1. Setup Sample Model Space Scene
        var modelAssembler = new RenderSceneAssembler(RenderColorContext.Dark);

        // Wall entity (layer: WALLS)
        modelAssembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("WALL-01"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("POLYLINE"),
            [
                new LinePrimitive(new WorldPoint2(10, 10), new WorldPoint2(190, 10)),
                new LinePrimitive(new WorldPoint2(190, 10), new WorldPoint2(190, 150)),
                new LinePrimitive(new WorldPoint2(190, 150), new WorldPoint2(10, 150)),
                new LinePrimitive(new WorldPoint2(10, 150), new WorldPoint2(10, 10))
            ]));

        // Equipment entity (layer: EQUIPMENT)
        modelAssembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("EQUIP-01"),
            new RenderLayerToken("EQUIPMENT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ARC"),
            [
                new ArcPrimitive(new WorldPoint2(100, 80), radius: 30, startRadians: 0d, sweepRadians: Math.PI * 2)
            ]));

        // Dimension entity (layer: DIMENSIONS)
        modelAssembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DIM-01"),
            new RenderLayerToken("DIMENSIONS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("DIMENSION"),
            [
                new LinePrimitive(new WorldPoint2(10, 5), new WorldPoint2(190, 5)),
                new TextPrimitive("180.00", new WorldPoint2(100, 2), height: 3.0, rotationRadians: 0d)
            ]));

        var modelScene = modelAssembler.Build();

        // 2. Invariant 1: Model Space Composition
        var manager = new CadLayoutManager(modelScene);
        if (manager.ActiveLayoutName != "Model" || !manager.ActiveLayout.IsModelSpace)
        {
            throw new InvalidOperationException("Default layout is not Model space.");
        }

        var directModelScene = manager.ComposeActiveScene();
        if (directModelScene.Entities.Count != 3)
        {
            throw new InvalidOperationException($"Expected 3 model entities, got {directModelScene.Entities.Count}");
        }
        Log.Info(Tag, "A16_ANDROID_MODEL_SPACE_PASS");

        // 3. Invariant 2: Paper Space Layout with Border, Title Block & Viewports
        // Paper layout: A3 sheet (420 x 297 mm)
        var paperBorderPrims = new List<RenderGeometryPrimitive>
        {
            // Sheet outline
            new LinePrimitive(new WorldPoint2(5, 5), new WorldPoint2(415, 5)),
            new LinePrimitive(new WorldPoint2(415, 5), new WorldPoint2(415, 292)),
            new LinePrimitive(new WorldPoint2(415, 292), new WorldPoint2(5, 292)),
            new LinePrimitive(new WorldPoint2(5, 292), new WorldPoint2(5, 5)),
            // Title block box at bottom right
            new LinePrimitive(new WorldPoint2(250, 5), new WorldPoint2(250, 50)),
            new LinePrimitive(new WorldPoint2(250, 50), new WorldPoint2(415, 50)),
            new TextPrimitive("PROJECT: MOBIL DWG CAD", new WorldPoint2(260, 35), height: 5.0, rotationRadians: 0d),
            new TextPrimitive("STAGE 16: VIEWPORT & PAPER SPACE", new WorldPoint2(260, 20), height: 4.0, rotationRadians: 0d),
            new TextPrimitive("SHEET: A-101 | SCALE: AS NOTED", new WorldPoint2(260, 10), height: 3.5, rotationRadians: 0d)
        };

        var paperBorderEntity = new RenderSceneEntity(
            new RenderEntityId("PAPER-BORDER-01"),
            new RenderLayerToken("TITLEBLOCK"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("SHEET_BORDER"),
            paperBorderPrims);

        // Viewport 1: Overview (frozen layer: DIMENSIONS)
        var vpOverview = new CadLayoutViewport(
            "VP_OVERVIEW",
            paperCenter: new WorldPoint2(130, 160),
            paperWidth: 220,
            paperHeight: 180,
            viewCenter: new WorldPoint2(100, 80),
            viewHeight: 200,
            twistAngleRadians: 0d,
            frozenLayers: ["DIMENSIONS"]);

        // Viewport 2: Detail (zoomed 2x, rotated 45 deg twist)
        var vpDetail = new CadLayoutViewport(
            "VP_DETAIL",
            paperCenter: new WorldPoint2(320, 160),
            paperWidth: 140,
            paperHeight: 140,
            viewCenter: new WorldPoint2(100, 80),
            viewHeight: 80,
            twistAngleRadians: Math.PI / 4d,
            clipBoundary: [
                new WorldPoint2(250, 90),
                new WorldPoint2(390, 90),
                new WorldPoint2(390, 230),
                new WorldPoint2(250, 230)
            ]);

        // Degenerate Viewport 3: zero dimension guard
        var vpDegenerate = new CadLayoutViewport(
            "VP_DEGENERATE",
            paperCenter: new WorldPoint2(50, 50),
            paperWidth: 50,
            paperHeight: 50,
            viewCenter: new WorldPoint2(0, 0),
            viewHeight: double.NaN);

        var sheetLayout = new CadLayoutDefinition(
            "Sheet-A101",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 420, 297),
            paperEntities: [paperBorderEntity],
            viewports: [vpOverview, vpDetail, vpDegenerate]);

        var multiLayoutManager = new CadLayoutManager(
            modelScene,
            layouts: [sheetLayout],
            activeLayoutName: "Model");

        // 4. Invariant 3: Zero-Reparse Layout Switching
        // Switch to Sheet-A101
        multiLayoutManager.SwitchLayout("Sheet-A101");
        if (multiLayoutManager.ActiveLayoutName != "Sheet-A101")
        {
            throw new InvalidOperationException("Failed to switch active layout to Sheet-A101.");
        }

        var paperDiagnostics = new List<SceneDiagnostic>();
        var sheetScene = multiLayoutManager.ComposeActiveScene(diagnostics: paperDiagnostics);

        // Switch back to Model and back to Sheet-A101 in memory (zero reparse)
        multiLayoutManager.SwitchLayout("Model");
        var modelScene2 = multiLayoutManager.ComposeActiveScene();
        if (modelScene2.Entities.Count != 3)
        {
            throw new InvalidOperationException("Zero-reparse switch back to Model failed.");
        }
        multiLayoutManager.SwitchLayout("Sheet-A101");
        Log.Info(Tag, "A16_ANDROID_ZERO_REPARSE_PASS");

        // 5. Invariant 4: Viewport Layer Override Verification
        var overviewPrim = sheetScene.Entities
            .Where(e => e.Source.EntityType == "VIEWPORT")
            .SelectMany(e => e.Geometry)
            .OfType<ViewportPrimitive>()
            .FirstOrDefault(v => v.ViewportId == "VP_OVERVIEW");
        if (overviewPrim == null)
        {
            throw new InvalidOperationException("Overview viewport primitive missing from composed paper scene.");
        }
        // DIMENSIONS layer must NOT be in overviewPrim inner primitives
        // Count should be walls (4 lines) + equip (1 arc) = 5
        if (overviewPrim.InnerPrimitives.Count != 5)
        {
            throw new InvalidOperationException($"Frozen layer override failed. Expected 5 primitives, got {overviewPrim.InnerPrimitives.Count}");
        }
        Log.Info(Tag, "A16_ANDROID_LAYER_OVERRIDE_PASS");

        // 6. Invariant 5: Degenerate Guard
        var degenerateDiag = paperDiagnostics.FirstOrDefault(d => d.Code == "INVALID_VIEWPORT_GEOMETRY");
        if (degenerateDiag == null)
        {
            throw new InvalidOperationException("Degenerate viewport did not emit INVALID_VIEWPORT_GEOMETRY diagnostic.");
        }
        if (sheetScene.Entities.Any(e => e.Source.EntityType == "VIEWPORT" && e.Id.Value.Contains("VP_DEGENERATE")))
        {
            throw new InvalidOperationException("Degenerate viewport entity was unexpectedly composed.");
        }
        Log.Info(Tag, "A16_ANDROID_DEGENERATE_GUARD_PASS");

        // 7. Invariant 6: Real Skia CAD Rendering to PNG
        var renderResult = await SkiaScenePngRenderer.RenderFitWithStatsAsync(
            sheetScene,
            pixelWidth: 1080,
            pixelHeight: 1080,
            density: 2.0d,
            paddingFraction: 0.05);

        var pngBytes = renderResult.Png;
        if (pngBytes.Length == 0 ||
            pngBytes[0] != 0x89 || pngBytes[1] != 0x50 || pngBytes[2] != 0x4E || pngBytes[3] != 0x47)
        {
            throw new InvalidOperationException("Rendered PNG is empty or lacks valid PNG header.");
        }

        if (renderResult.NonBackgroundPixels < 500)
        {
            throw new InvalidOperationException($"Too few non-background pixels rendered: {renderResult.NonBackgroundPixels}");
        }

        var pngSha256 = Convert.ToHexString(SHA256.HashData(pngBytes)).ToLowerInvariant();
        var snapshot = LayoutSceneSemanticSnapshot.Create(multiLayoutManager);
        var snapHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(snapshot))).ToLowerInvariant();

        Log.Info(Tag, $"A16_LAYOUT_ACTIVE={multiLayoutManager.ActiveLayoutName}");
        Log.Info(Tag, $"A16_PAPER_ENTITIES_COUNT={sheetScene.Entities.Count}");
        Log.Info(Tag, $"A16_RENDER_PIXELS={renderResult.NonBackgroundPixels}");
        Log.Info(Tag, $"A16_SNAPSHOT_HASH={snapHash}");
        Log.Info(Tag, $"A16_ANDROID_SKIA_RENDER_PASS bytes={pngBytes.Length} sha256={pngSha256}");
        Log.Info(Tag, "ANDROID_STAGE16_LAYOUT_VIEWPORT_PASS");

        return new A16ValidationResult(
            pngBytes,
            pngSha256,
            multiLayoutManager.ActiveLayoutName,
            sheetScene.Entities.Count,
            "ANDROID_STAGE16_LAYOUT_VIEWPORT_PASS");
    }
}
#endif
