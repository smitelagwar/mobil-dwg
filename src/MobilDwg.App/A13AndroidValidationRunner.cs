#if A13_VALIDATION
using System.Security.Cryptography;
using Android.Util;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.App;

public sealed record A13ValidationResult(
    byte[] Png,
    string PngSha256,
    int LayerCount,
    string Marker);

public static class A13AndroidValidationRunner
{
    public const string Tag = "MobilDwgA13";

    public static async Task<A13ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A13_ANDROID_VALIDATION_STARTING");
        await Task.Delay(250);

        // 1. Invariant 1: ACI & TrueColor Resolution
        var aciRed = CadColor.FromAci(1).Resolve(RenderColorContext.Dark);
        var aciYellow = CadColor.FromAci(2).Resolve(RenderColorContext.Dark);
        var aciGreen = CadColor.FromAci(3).Resolve(RenderColorContext.Dark);
        var aciCyan = CadColor.FromAci(4).Resolve(RenderColorContext.Dark);
        var aciBlue = CadColor.FromAci(5).Resolve(RenderColorContext.Dark);
        var aciWhite = CadColor.FromAci(7).Resolve(RenderColorContext.Dark);
        var aciBlack = CadColor.FromAci(7).Resolve(RenderColorContext.Light);
        var trueColor = CadColor.FromRgb(200, 100, 50).Resolve(RenderColorContext.Dark);

        if (aciRed != 0xFFFF0000u || aciYellow != 0xFFFFFF00u || aciGreen != 0xFF00FF00u ||
            aciCyan != 0xFF00FFFFu || aciBlue != 0xFF0000FFu || aciWhite != 0xFFFFFFFFu ||
            aciBlack != 0xFF000000u || trueColor != 0xFFC86432u)
        {
            throw new InvalidOperationException("ACI or TrueColor color mapping failed.");
        }
        Log.Info(Tag, "A13_ANDROID_ACI_TRUECOLOR_PASS");

        // 2. Build Layer Table
        var layerTable = new LayerTable();
        var wallsLayer = new LayerDefinition("WALLS", CadColor.FromAci(1), CadLinetype.Continuous, CadLineweight.FromMm(0.50));
        var hiddenLayer = new LayerDefinition("HIDDEN_DETAILS", CadColor.FromAci(5), CadLinetype.Hidden, CadLineweight.FromMm(0.25));
        var centerLayer = new LayerDefinition("CENTER_LINES", CadColor.FromAci(3), CadLinetype.Center, CadLineweight.FromMm(0.18));
        var frozenLayer = new LayerDefinition("FROZEN_LAYER", CadColor.FromAci(2), CadLinetype.Continuous, CadLineweight.Default, isVisible: false, isFrozen: true);
        var complexLt = CadLinetype.CreateComplex("WATER_LINE", "---WATER---", new[] { 25f, -8f });
        var complexLayer = new LayerDefinition("COMPLEX_LAYER", CadColor.FromAci(6), complexLt, CadLineweight.FromMm(0.35));

        layerTable.AddOrUpdate(wallsLayer);
        layerTable.AddOrUpdate(hiddenLayer);
        layerTable.AddOrUpdate(centerLayer);
        layerTable.AddOrUpdate(frozenLayer);
        layerTable.AddOrUpdate(complexLayer);

        // 3. Invariant 2: ByLayer & ByBlock resolution
        var byLayerResolved = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("WALLS"),
            layerTable,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        if (byLayerResolved.ArgbColor != 0xFFFF0000u || byLayerResolved.StrokeWidthPixels < 1.5f)
        {
            throw new InvalidOperationException("ByLayer resolution failed.");
        }

        var blockContext = new CadEntityStyle(CadColor.FromAci(4), CadLinetype.Dashed, CadLineweight.FromMm(0.60));
        var byBlockResolved = CadStyleResolver.Resolve(
            new CadEntityStyle(CadColor.ByBlock, CadLinetype.ByBlock, CadLineweight.ByBlock),
            new RenderLayerToken("0"),
            layerTable,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0,
            blockContextStyle: blockContext);

        if (byBlockResolved.ArgbColor != 0xFF00FFFFu || byBlockResolved.DashPatternPixels is null)
        {
            throw new InvalidOperationException("ByBlock resolution failed.");
        }
        Log.Info(Tag, "A13_ANDROID_BYLAYER_BYBLOCK_PASS");

        // 4. Invariant 3: Layer visibility & freeze checks
        var frozenResolved = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("FROZEN_LAYER"),
            layerTable,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        if (frozenResolved.IsVisible)
        {
            throw new InvalidOperationException("Frozen layer must not be visible.");
        }
        Log.Info(Tag, "A13_ANDROID_LAYER_VISIBILITY_FREEZE_PASS");

        // 5. Invariant 4: Linetype & lineweight
        var hiddenResolved = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("HIDDEN_DETAILS"),
            layerTable,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        if (hiddenResolved.DashPatternPixels is not { Length: 2 })
        {
            throw new InvalidOperationException("Hidden linetype dash pattern failed.");
        }
        Log.Info(Tag, "A13_ANDROID_LINETYPE_LINEWEIGHT_PASS");

        // 6. Invariant 5: Complex style warning
        var complexDiagnostics = new List<MobilDwg.Rendering.Diagnostics.SceneDiagnostic>();
        var complexResolved = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("COMPLEX_LAYER"),
            layerTable,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0,
            diagnostics: complexDiagnostics);

        if (!complexDiagnostics.Any(d => d.Code == "COMPLEX_LINETYPE_FALLBACK"))
        {
            throw new InvalidOperationException("Complex linetype warning was not emitted.");
        }
        Log.Info(Tag, "A13_ANDROID_COMPLEX_STYLE_WARNING_PASS");

        // 7. Assemble Synthetic Scene for Skia Rendering
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.SetLayerTable(layerTable);

        // WALLS (Red rectangle)
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("WALL-1"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("POLYLINE"),
            new[] {
                new PolylinePrimitive(new[] {
                    new PolylineVertex(new WorldPoint2(-50, -50)),
                    new PolylineVertex(new WorldPoint2(50, -50)),
                    new PolylineVertex(new WorldPoint2(50, 50)),
                    new PolylineVertex(new WorldPoint2(-50, 50)),
                }, closed: true)
            }));

        // HIDDEN_DETAILS (Blue dashed horizontal lines)
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("HIDDEN-1"),
            new RenderLayerToken("HIDDEN_DETAILS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            new[] {
                new LinePrimitive(new WorldPoint2(-40, -20), new WorldPoint2(40, -20)),
                new LinePrimitive(new WorldPoint2(-40, 20), new WorldPoint2(40, 20)),
            }));

        // CENTER_LINES (Green dash-dot crosshairs)
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CENTER-1"),
            new RenderLayerToken("CENTER_LINES"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            new[] {
                new LinePrimitive(new WorldPoint2(0, -60), new WorldPoint2(0, 60)),
                new LinePrimitive(new WorldPoint2(-60, 0), new WorldPoint2(60, 0)),
            }));

        // CONTRAST_LAYER (White circle at center)
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("CONTRAST-1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("CIRCLE"),
            new[] {
                new ArcPrimitive(new WorldPoint2(0, 0), 15.0, 0, Math.PI * 2)
            }));

        // FROZEN_LAYER (Should be skipped by renderer!)
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("FROZEN-1"),
            new RenderLayerToken("FROZEN_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            new[] {
                new LinePrimitive(new WorldPoint2(-100, -100), new WorldPoint2(100, 100))
            }));

        var scene = assembler.Build();

        // 8. Render Scene via Skia
        var renderResult = await SkiaScenePngRenderer.RenderFitWithStatsAsync(
            scene,
            pixelWidth: 1024,
            pixelHeight: 768,
            density: 2.0);

        if (renderResult.NonBackgroundPixels < 100)
        {
            throw new InvalidOperationException($"Rendered scene had too few foreground pixels: {renderResult.NonBackgroundPixels}");
        }

        var pngBytes = renderResult.Png;
        var pngSha256 = Convert.ToHexString(SHA256.HashData(pngBytes)).ToLowerInvariant();
        Log.Info(Tag, $"A13_ANDROID_PNG_PASS bytes={pngBytes.Length} pixels={renderResult.NonBackgroundPixels} sha256={pngSha256}");

        const string passMarker = "ANDROID_STAGE13_LAYER_STYLE_PASS";
        Log.Info(Tag, passMarker);
        Log.Info(Tag, "CLAIM_LIMIT=A13_LAYER_STYLE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY");

        return new A13ValidationResult(
            pngBytes,
            pngSha256,
            layerTable.Layers.Count,
            passMarker);
    }
}
#endif
