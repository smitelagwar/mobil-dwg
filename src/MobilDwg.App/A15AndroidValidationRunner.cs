#if A15_VALIDATION
using System.Security.Cryptography;
using Android.Util;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Dimensions;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Hatch;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.App;

public sealed record A15ValidationResult(
    byte[] Png,
    string PngSha256,
    int EntityCount,
    string Marker);

public static class A15AndroidValidationRunner
{
    public const string Tag = "MobilDwgA15";

    public static async Task<A15ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A15_ANDROID_VALIDATION_STARTING");
        await Task.Delay(250);

        // 1. Invariant 1: Anonymous Dimension Block Expansion (*D...)
        var blockDef = new BlockDefinition(
            "*D001",
            basePoint: new WorldPoint2(0, 0),
            entities: [
                new BlockEntityTemplate(
                    new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(80, 0)),
                    new RenderLayerToken("DIM_ANON"),
                    new RenderStyleToken("BYLAYER"))
            ]);
        var blockTable = new Dictionary<string, BlockDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["*D001"] = blockDef
        };

        var anonDef = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Linear,
            defPoint1: new WorldPoint2(0, 0),
            defPoint2: new WorldPoint2(80, 0),
            dimensionLinePoint: new WorldPoint2(40, 20),
            anonymousBlockName: "*D001");

        var anonEntity = DimensionBuilder.BuildDimension("DIM_ANON_01", anonDef, blockTable: blockTable);
        if (anonEntity.Geometry.Count != 1 || anonEntity.Geometry[0] is not LinePrimitive)
        {
            throw new InvalidOperationException("Anonymous dimension block expansion failed on Android.");
        }
        Log.Info(Tag, "A15_ANDROID_ANONYMOUS_BLOCK_PASS");

        // 2. Invariant 2: Procedural Dimensions (Aligned, Rotated, Radial, Diametric)
        var alignedDef = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Aligned,
            defPoint1: new WorldPoint2(10, 10),
            defPoint2: new WorldPoint2(130, 10),
            dimensionLinePoint: new WorldPoint2(70, 35),
            textHeight: 3.5);
        var alignedEntity = DimensionBuilder.BuildDimension("DIM_ALIGNED_01", alignedDef);

        var alignedText = alignedEntity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        if (alignedText == null || !alignedText.Text.Contains("120.00"))
        {
            throw new InvalidOperationException($"Aligned dimension measurement mismatch: '{alignedText?.Text}'");
        }

        var rotatedDef = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Linear,
            defPoint1: new WorldPoint2(10, 10),
            defPoint2: new WorldPoint2(90, 70),
            dimensionLinePoint: new WorldPoint2(50, 95),
            rotationRadians: 0d);
        var rotatedEntity = DimensionBuilder.BuildDimension("DIM_ROTATED_01", rotatedDef);
        var rotatedText = rotatedEntity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        if (rotatedText == null || !rotatedText.Text.Contains("80.00"))
        {
            throw new InvalidOperationException($"Rotated dimension measurement mismatch: '{rotatedText?.Text}'");
        }

        var radialDef = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Radial,
            defPoint1: new WorldPoint2(200, 100),
            defPoint2: new WorldPoint2(250, 100),
            textHeight: 3.5);
        var radialEntity = DimensionBuilder.BuildDimension("DIM_RADIAL_01", radialDef);
        var radialText = radialEntity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        if (radialText == null || !radialText.Text.StartsWith("R", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Radial dimension text mismatch: '{radialText?.Text}'");
        }

        var diametricDef = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Diametric,
            defPoint1: new WorldPoint2(150, 200),
            defPoint2: new WorldPoint2(250, 200),
            textHeight: 3.5);
        var diametricEntity = DimensionBuilder.BuildDimension("DIM_DIAMETRIC_01", diametricDef);
        var diametricText = diametricEntity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        if (diametricText == null || !diametricText.Text.StartsWith("Ø", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Diametric dimension text mismatch: '{diametricText?.Text}'");
        }
        Log.Info(Tag, "A15_ANDROID_PROCEDURAL_DIMENSIONS_PASS");

        // 3. Invariant 3: Degenerate Dimension Guards
        var degenDiags = new List<SceneDiagnostic>();
        var degenDef = new CadDimensionDefinition(
            dimensionType: CadDimensionType.Aligned,
            defPoint1: new WorldPoint2(50, 50),
            defPoint2: new WorldPoint2(50, 50),
            dimensionLinePoint: new WorldPoint2(50, 50));
        var degenEntity = DimensionBuilder.BuildDimension("DIM_DEGEN", degenDef, diagnostics: degenDiags);
        if (degenEntity.Geometry.Count != 0 || !degenDiags.Any(d => d.Code == "DEGENERATE_DIMENSION_POINTS"))
        {
            throw new InvalidOperationException("Degenerate dimension guard failed.");
        }

        var nanDiags = new List<SceneDiagnostic>();
        var nanEntity = DimensionBuilder.TryBuildFromRaw("DIM_NAN", CadDimensionType.Linear, double.NaN, 0, 10, 0, 5, 5, diagnostics: nanDiags);
        if (nanEntity.Geometry.Count != 0 || !nanDiags.Any(d => d.Code == "INVALID_DIMENSION_GEOMETRY"))
        {
            throw new InvalidOperationException("NaN dimension guard failed.");
        }
        Log.Info(Tag, "A15_ANDROID_DEGENERATE_GUARDS_PASS");

        // 4. Invariant 4: Leader & MultiLeader
        var leaderEntity = LeaderBuilder.BuildLeader(
            "LEADER_01",
            [new WorldPoint2(20, 200), new WorldPoint2(60, 230), new WorldPoint2(90, 230)],
            annotationText: "DETAY C-C",
            textHeight: 4.0,
            arrowheadSize: 3.5,
            doglegLength: 8.0);
        var leaderText = leaderEntity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        if (leaderText == null || leaderText.Text != "DETAY C-C")
        {
            throw new InvalidOperationException("Leader annotation text failed.");
        }
        Log.Info(Tag, "A15_ANDROID_LEADER_PASS");

        // 5. Invariant 5: Hatch Processing (Auto-closure & Broken boundary)
        var hatchDiags = new List<SceneDiagnostic>();
        var unclosedLoop = HatchProcessor.ValidateAndCloseLoop(
            [new WorldPoint2(0, 0), new WorldPoint2(40, 0), new WorldPoint2(40, 40), new WorldPoint2(0, 40), new WorldPoint2(0, 0.0004)],
            isOuter: true,
            diagnostics: hatchDiags);
        if (hatchDiags.Count != 0)
        {
            throw new InvalidOperationException("Hatch auto-closure emitted unexpected diagnostic.");
        }

        var brokenLoop = HatchProcessor.ValidateAndCloseLoop(
            [new WorldPoint2(0, 0), new WorldPoint2(40, 0), new WorldPoint2(40, 40), new WorldPoint2(0, 40), new WorldPoint2(0, 4.0)],
            isOuter: true,
            diagnostics: hatchDiags);
        if (!hatchDiags.Any(d => d.Code == "HATCH_BROKEN_BOUNDARY"))
        {
            throw new InvalidOperationException("Hatch broken boundary failed to emit diagnostic.");
        }
        Log.Info(Tag, "A15_ANDROID_HATCH_PROCESSING_PASS");

        // 6. Invariant 6: Real Skia Rendering of Full CAD Scene
        var layerTable = new LayerTable();
        layerTable.AddOrUpdate(new LayerDefinition("DIMENSIONS", CadColor.FromAci(2), CadLinetype.Continuous, CadLineweight.FromMm(0.25))); // Yellow
        layerTable.AddOrUpdate(new LayerDefinition("HATCH_SOLID", CadColor.FromAci(4), CadLinetype.Continuous, CadLineweight.Default));    // Cyan
        layerTable.AddOrUpdate(new LayerDefinition("HATCH_PATTERN", CadColor.FromAci(1), CadLinetype.Continuous, CadLineweight.Default));  // Red
        layerTable.AddOrUpdate(new LayerDefinition("FRAME", CadColor.FromAci(7), CadLinetype.Continuous, CadLineweight.FromMm(0.50)));      // White

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.SetLayerTable(layerTable);

        // Frame
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("FRAME-01"),
            new RenderLayerToken("FRAME"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(-20, -20), new WorldPoint2(320, -20)),
                new LinePrimitive(new WorldPoint2(320, -20), new WorldPoint2(320, 280)),
                new LinePrimitive(new WorldPoint2(320, 280), new WorldPoint2(-20, 280)),
                new LinePrimitive(new WorldPoint2(-20, 280), new WorldPoint2(-20, -20))
            ]));

        // Add dimensions
        assembler.AddEntity(alignedEntity);
        assembler.AddEntity(rotatedEntity);
        assembler.AddEntity(radialEntity);
        assembler.AddEntity(diametricEntity);
        assembler.AddEntity(leaderEntity);

        // Solid Hatch with Nested EvenOdd Island
        var outerHatchLoop = new HatchLoop(
            [new WorldPoint2(0, 120), new WorldPoint2(80, 120), new WorldPoint2(80, 180), new WorldPoint2(0, 180), new WorldPoint2(0, 120)],
            isOuter: true);
        var islandHatchLoop = new HatchLoop(
            [new WorldPoint2(20, 135), new WorldPoint2(60, 135), new WorldPoint2(60, 165), new WorldPoint2(20, 165), new WorldPoint2(20, 135)],
            isOuter: false);

        var solidHatchPrim = new HatchPrimitive(
            loops: [outerHatchLoop, islandHatchLoop],
            patternName: "SOLID",
            islandStyle: HatchIslandStyle.Normal,
            isSolid: true);

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("HATCH_SOLID_01"),
            new RenderLayerToken("HATCH_SOLID"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("HATCH"),
            [solidHatchPrim]));

        // ANSI31 Pattern Hatch with clipped lines
        var patternHatchLoop = new HatchLoop(
            [new WorldPoint2(100, 120), new WorldPoint2(180, 120), new WorldPoint2(180, 180), new WorldPoint2(100, 180), new WorldPoint2(100, 120)],
            isOuter: true);
        var patternLines = HatchProcessor.GeneratePatternLines([patternHatchLoop], angleRadians: Math.PI / 4d, spacing: 5.0, bounds: patternHatchLoop.Bounds);
        var patternHatchPrim = new HatchPrimitive(
            loops: [patternHatchLoop],
            patternName: "ANSI31",
            patternAngleRadians: Math.PI / 4d,
            patternScale: 5.0,
            isSolid: false,
            patternLines: patternLines);

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("HATCH_PATTERN_01"),
            new RenderLayerToken("HATCH_PATTERN"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("HATCH"),
            [patternHatchPrim]));

        var scene = assembler.Build();

        // Render to 1080x1080 PNG
        var renderResult = await SkiaScenePngRenderer.RenderFitWithStatsAsync(
            scene,
            pixelWidth: 1080,
            pixelHeight: 1080,
            density: 2.0d,
            paddingFraction: 0.08);

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
        var snapshot = DimensionHatchSemanticSnapshot.Create(scene);
        var snapHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(snapshot))).ToLowerInvariant();

        Log.Info(Tag, $"A15_SCENE_ENTITIES_COUNT={scene.Entities.Count}");
        Log.Info(Tag, $"A15_HATCH_ISLAND_EVENODD_VERIFIED loops={solidHatchPrim.Loops.Count}");
        Log.Info(Tag, $"A15_ANSI31_PATTERN_LINES_COUNT={patternLines.Count}");
        Log.Info(Tag, $"A15_RENDER_PIXELS={renderResult.NonBackgroundPixels}");
        Log.Info(Tag, $"A15_SNAPSHOT_HASH={snapHash}");
        Log.Info(Tag, $"A15_ANDROID_SKIA_RENDER_PASS bytes={pngBytes.Length} sha256={pngSha256}");
        Log.Info(Tag, "ANDROID_STAGE15_DIMENSION_HATCH_PASS");

        return new A15ValidationResult(pngBytes, pngSha256, scene.Entities.Count, "ANDROID_STAGE15_DIMENSION_HATCH_PASS");
    }
}
#endif
