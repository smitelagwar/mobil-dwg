using System.Runtime.CompilerServices;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;

internal static class Stage13LayerStyleTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestAciColorPalette();
        TestAci7ContrastInversion();
        TestTrueColorResolution();
        TestByLayerResolution();
        TestByBlockResolution();
        TestLayerVisibilityToggle();
        TestLayerFreezeToggle();
        TestStandardLinetypes();
        TestComplexLinetypeFallback();
        TestLineweightPixelConversion();
        TestUnknownLayerFallback();
        TestLayerStyleSemanticSnapshot();

        Console.WriteLine("STAGE13_LAYER_STYLE_TESTS_PASS");
    }

    private static void TestAciColorPalette()
    {
        // Standard ACI primary colors
        Assert(CadColor.FromAci(1).Resolve(RenderColorContext.Dark) == 0xFFFF0000u, "ACI 1 must be Red");
        Assert(CadColor.FromAci(2).Resolve(RenderColorContext.Dark) == 0xFFFFFF00u, "ACI 2 must be Yellow");
        Assert(CadColor.FromAci(3).Resolve(RenderColorContext.Dark) == 0xFF00FF00u, "ACI 3 must be Green");
        Assert(CadColor.FromAci(4).Resolve(RenderColorContext.Dark) == 0xFF00FFFFu, "ACI 4 must be Cyan");
        Assert(CadColor.FromAci(5).Resolve(RenderColorContext.Dark) == 0xFF0000FFu, "ACI 5 must be Blue");
        Assert(CadColor.FromAci(6).Resolve(RenderColorContext.Dark) == 0xFFFF00FFu, "ACI 6 must be Magenta");
        Assert(CadColor.FromAci(8).Resolve(RenderColorContext.Dark) == 0xFF808080u, "ACI 8 must be Dark Gray");
        Assert(CadColor.FromAci(9).Resolve(RenderColorContext.Dark) == 0xFFC0C0C0u, "ACI 9 must be Light Gray");
        Assert(CadColor.FromAci(255).Resolve(RenderColorContext.Dark) == 0xFFFFFFFFu, "ACI 255 must be White");
    }

    private static void TestAci7ContrastInversion()
    {
        var aci7 = CadColor.FromAci(7);
        var whiteOnDark = aci7.Resolve(RenderColorContext.Dark);
        var blackOnLight = aci7.Resolve(RenderColorContext.Light);

        Assert(whiteOnDark == 0xFFFFFFFFu, "ACI 7 on dark background must resolve to White");
        Assert(blackOnLight == 0xFF000000u, "ACI 7 on light background must resolve to Black");
    }

    private static void TestTrueColorResolution()
    {
        var customRgb = CadColor.FromRgb(123, 45, 67);
        var resolved = customRgb.Resolve(RenderColorContext.Dark);
        Assert(resolved == 0xFF7B2D43u, "TrueColor RGB must preserve exact components");
    }

    private static void TestByLayerResolution()
    {
        var table = new LayerTable();
        var wallsLayer = new LayerDefinition(
            "WALLS",
            color: CadColor.FromAci(1), // Red
            linetype: CadLinetype.Continuous,
            lineweight: CadLineweight.FromMm(0.50));
        table.AddOrUpdate(wallsLayer);

        var entityStyle = CadEntityStyle.Default; // ByLayer color, linetype, lineweight
        var resolved = CadStyleResolver.Resolve(
            entityStyle,
            new RenderLayerToken("WALLS"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0,
            density: 1.0);

        Assert(resolved.IsVisible, "Entity on visible layer must be visible");
        Assert(resolved.ArgbColor == 0xFFFF0000u, "ByLayer entity must inherit layer Red color");
        Assert(resolved.DashPatternPixels is null, "Continuous linetype should have null dash pattern");
        Assert(resolved.StrokeWidthPixels > 1.5f, "0.50mm lineweight must resolve to thicker stroke width");
    }

    private static void TestByBlockResolution()
    {
        var table = new LayerTable();
        var blockContext = new CadEntityStyle(
            Color: CadColor.FromAci(4), // Cyan
            Linetype: CadLinetype.Dashed,
            Lineweight: CadLineweight.FromMm(0.70));

        var entityStyle = new CadEntityStyle(
            Color: CadColor.ByBlock,
            Linetype: CadLinetype.ByBlock,
            Lineweight: CadLineweight.ByBlock);

        var resolved = CadStyleResolver.Resolve(
            entityStyle,
            new RenderLayerToken("0"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0,
            density: 1.0,
            displayLineweights: true,
            blockContextStyle: blockContext);

        Assert(resolved.ArgbColor == 0xFF00FFFFu, "ByBlock entity must inherit Cyan from block context");
        Assert(resolved.DashPatternPixels is { Length: > 0 }, "ByBlock entity must inherit Dashed linetype from block context");
        Assert(resolved.StrokeWidthPixels > 2.0f, "ByBlock entity must inherit 0.70mm lineweight from block context");
    }

    private static void TestLayerVisibilityToggle()
    {
        var table = new LayerTable();
        table.AddOrUpdate(new LayerDefinition("HIDDEN_LAYER", isVisible: false));

        var resolved = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("HIDDEN_LAYER"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        Assert(!resolved.IsVisible, "Entity on invisible layer must not be visible");

        table.SetLayerVisibility("HIDDEN_LAYER", true);
        var resolvedAfterToggle = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("HIDDEN_LAYER"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        Assert(resolvedAfterToggle.IsVisible, "Entity on toggled-visible layer must become visible");
    }

    private static void TestLayerFreezeToggle()
    {
        var table = new LayerTable();
        table.AddOrUpdate(new LayerDefinition("FROZEN_LAYER", isVisible: true, isFrozen: true));

        var resolved = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("FROZEN_LAYER"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        Assert(!resolved.IsVisible, "Entity on frozen layer must not be visible");

        table.SetLayerFrozen("FROZEN_LAYER", false);
        var resolvedThawed = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("FROZEN_LAYER"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        Assert(resolvedThawed.IsVisible, "Entity on thawed layer must become visible");
    }

    private static void TestStandardLinetypes()
    {
        var table = new LayerTable();
        var hiddenLinetype = CadLinetype.Hidden;

        var entityStyle = new CadEntityStyle(
            Color: CadColor.FromAci(3),
            Linetype: hiddenLinetype,
            Lineweight: CadLineweight.Default);

        var resolved = CadStyleResolver.Resolve(
            entityStyle,
            new RenderLayerToken("0"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0);

        Assert(resolved.DashPatternPixels is not null, "Hidden linetype must produce dash pattern");
        Assert(resolved.DashPatternPixels!.Length == 2, "Hidden linetype pattern must have 2 intervals (dash and space)");
        Assert(resolved.DashPatternPixels[0] > 0, "Dash pixel length must be positive");
    }

    private static void TestComplexLinetypeFallback()
    {
        var table = new LayerTable();
        var complexLt = CadLinetype.CreateComplex("GAS_LINE", "----GAS----", new[] { 20f, -5f });

        var entityStyle = new CadEntityStyle(
            Color: CadColor.FromAci(1),
            Linetype: complexLt,
            Lineweight: CadLineweight.Default);

        var diagnostics = new List<SceneDiagnostic>();
        var resolved = CadStyleResolver.Resolve(
            entityStyle,
            new RenderLayerToken("0"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0,
            diagnostics: diagnostics);

        Assert(resolved.IsVisible, "Complex linetype must remain renderable");
        Assert(diagnostics.Any(d => d.Code == "COMPLEX_LINETYPE_FALLBACK"), "Must emit audited COMPLEX_LINETYPE_FALLBACK diagnostic");
    }

    private static void TestLineweightPixelConversion()
    {
        var lw0 = CadLineweight.FromMm(0.00);
        var lw25 = CadLineweight.FromMm(0.25);
        var lw100 = CadLineweight.FromMm(1.00);

        Assert(lw0.ToPixels(density: 1.0, displayLineweights: true) >= 1.0f, "0mm lineweight must clamp to at least 1px");
        Assert(lw25.ToPixels(density: 1.0, displayLineweights: true) < lw100.ToPixels(density: 1.0, displayLineweights: true), "1.00mm must be wider than 0.25mm");
        Assert(lw100.ToPixels(density: 1.0, displayLineweights: false) == 1.0f, "Lineweight with display=false must return hairline 1.0px");
    }

    private static void TestUnknownLayerFallback()
    {
        var table = new LayerTable();
        var diagnostics = new List<SceneDiagnostic>();

        var resolved = CadStyleResolver.Resolve(
            CadEntityStyle.Default,
            new RenderLayerToken("NON_EXISTENT_LAYER"),
            table,
            RenderColorContext.Dark,
            worldUnitsPerPixel: 1.0,
            diagnostics: diagnostics);

        Assert(resolved.IsVisible, "Entity on missing layer must fall back to Layer 0 and remain visible");
        Assert(diagnostics.Any(d => d.Code == "UNKNOWN_LAYER_FALLBACK"), "Must emit UNKNOWN_LAYER_FALLBACK diagnostic");
    }

    private static void TestLayerStyleSemanticSnapshot()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddLayer(new LayerDefinition("WALLS", CadColor.FromAci(1), CadLinetype.Continuous, CadLineweight.FromMm(0.50)));
        assembler.AddLayer(new LayerDefinition("HIDDEN_LINES", CadColor.FromAci(5), CadLinetype.Hidden, CadLineweight.FromMm(0.25)));
        assembler.AddLayer(new LayerDefinition("FROZEN_LAYER", CadColor.FromAci(2), CadLinetype.Continuous, CadLineweight.Default, isVisible: false, isFrozen: true));

        var line1 = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(10, 0));
        var line2 = new LinePrimitive(new WorldPoint2(0, 10), new WorldPoint2(10, 10));
        var line3 = new LinePrimitive(new WorldPoint2(0, 20), new WorldPoint2(10, 20));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E-WALL-1"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            new[] { line1 }));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E-HIDDEN-1"),
            new RenderLayerToken("HIDDEN_LINES"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            new[] { line2 }));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E-FROZEN-1"),
            new RenderLayerToken("FROZEN_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            new[] { line3 }));

        var scene = assembler.Build();
        var snapshot = LayerStyleSemanticSnapshot.Create(scene);

        Assert(snapshot.StartsWith("format=layer-style/v1"), "Snapshot must start with format header");
        Assert(snapshot.Contains("layer=WALLS|VISIBLE|THAWED|ACI:1|CONTINUOUS|0.50mm"), "Snapshot must contain WALLS layer");
        Assert(snapshot.Contains("layer=HIDDEN_LINES|VISIBLE|THAWED|ACI:5|HIDDEN|0.25mm"), "Snapshot must contain HIDDEN_LINES layer");
        Assert(snapshot.Contains("layer=FROZEN_LAYER|HIDDEN|FROZEN|ACI:2|CONTINUOUS|DEFAULT"), "Snapshot must contain FROZEN_LAYER layer");
        Assert(snapshot.Contains("resolved=E-WALL-1|WALLS|#FFFF0000|SOLID|"), "E-WALL-1 must resolve to Red solid");
        Assert(snapshot.Contains("resolved=E-HIDDEN-1|HIDDEN_LINES|#FF0000FF|DASHED|"), "E-HIDDEN-1 must resolve to Blue dashed");
        Assert(snapshot.Contains("resolved=E-FROZEN-1|FROZEN_LAYER|#00000000|SOLID|0.0px|HIDDEN"), "E-FROZEN-1 must resolve to Hidden");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }
}
