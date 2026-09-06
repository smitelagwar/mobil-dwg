using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;

internal static class Stage14TextTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestTurkishCharEncodingAndCp1254();
        TestAutoCadUnicodeEscapeSequences();
        TestAutoCadSpecialSymbolCodes();
        TestBoundedMTextParserBasic();
        TestBoundedMTextParserNestingAndDepthGuard();
        TestFontSubstitutionTableKnownShx();
        TestFontSubstitutionUnknownFallback();
        TestTextAlignmentCalculations();
        TestTextMirrorFlagsAndRotation();
        TestTextWorldBoundsCalculation();
        TestSkiaTextRenderDarkAndLight();
        TestTextSceneSemanticSnapshotDeterminism();

        Console.WriteLine("STAGE14_TEXT_FONT_TESTS_PASS");
    }

    private static void TestTurkishCharEncodingAndCp1254()
    {
        // Windows-1254 (CP1254) byte values for Turkish characters:
        // Ç: 0xC7, ç: 0xE7
        // Ğ: 0xD0, ğ: 0xF0
        // İ: 0xDD, ı: 0xFD
        // Ö: 0xD6, ö: 0xF6
        // Ş: 0xDE, ş: 0xFE
        // Ü: 0xDC, ü: 0xFC
        byte[] cp1254Bytes = [0xC7, 0xE7, 0xD0, 0xF0, 0xDD, 0xFD, 0xD6, 0xF6, 0xDE, 0xFE, 0xDC, 0xFC];
        var decoded = CadTextEncoding.DecodeCp1254(cp1254Bytes);
        Assert(decoded == "ÇçĞğİıÖöŞşÜü", $"CP1254 exact decode failed: got '{decoded}'");

        // Test auto-detect with invalid UTF-8 bytes (falling back to CP1254)
        var autoFallback = CadTextEncoding.DecodeBytes(cp1254Bytes);
        Assert(autoFallback == "ÇçĞğİıÖöŞşÜü", "Auto-fallback to CP1254 failed");

        // Test auto-detect with valid UTF-8 bytes
        var utf8Bytes = Encoding.UTF8.GetBytes("Türkçe Metin: Çankaya, Eskişehir, İzmir, Ağrı, Şanlıurfa, Ödemiş");
        var utf8Decoded = CadTextEncoding.DecodeBytes(utf8Bytes);
        Assert(utf8Decoded == "Türkçe Metin: Çankaya, Eskişehir, İzmir, Ağrı, Şanlıurfa, Ödemiş", "UTF-8 decode failed");
    }

    private static void TestAutoCadUnicodeEscapeSequences()
    {
        var input = @"Bina \U+00C7izimi: \U+011Eiri\U+015F Kap\U+0131s\U+0131, \U+00D6n Cephe, \U+00DCst Kat";
        var decoded = CadTextEncoding.DecodeAutoCadEscapes(input);
        Assert(decoded == "Bina Çizimi: Ğiriş Kapısı, Ön Cephe, Üst Kat", $"Unicode escape failed: '{decoded}'");

        var lowerInput = @"\U+00e7\U+011f\U+0131\U+00f6\U+015f\U+00fc";
        var lowerDecoded = CadTextEncoding.DecodeAutoCadEscapes(lowerInput);
        Assert(lowerDecoded == "çğıöşü", $"Lowercase hex escape failed: '{lowerDecoded}'");
    }

    private static void TestAutoCadSpecialSymbolCodes()
    {
        var input = @"Aci: 45%%d, Tolerans: %%p0.05, Boru Capi: %%c50 mm, Esim: 100%%%";
        var decoded = CadTextEncoding.DecodeAutoCadEscapes(input);
        Assert(decoded == "Aci: 45\u00B0, Tolerans: \u00B10.05, Boru Capi: \u00D850 mm, Esim: 100%", $"Symbol decode failed: '{decoded}'");

        // Test overscore / underscore stripping
        var toggles = @"%%uAlti Cizili%%u ve %%oUstu Cizili%%o";
        var toggleDecoded = CadTextEncoding.DecodeAutoCadEscapes(toggles);
        Assert(toggleDecoded == "Alti Cizili ve Ustu Cizili", $"Toggle strip failed: '{toggleDecoded}'");
    }

    private static void TestBoundedMTextParserBasic()
    {
        var mtext = @"\A1;\C1;\H2.5;\Fromans.shx;Ilk Satir\PIkinci Satir\PUcuncu Satir";
        var result = MTextParser.Parse(mtext);

        Assert(result.Lines.Count == 3, $"Expected 3 lines, got {result.Lines.Count}");
        Assert(result.Lines[0] == "Ilk Satir", $"Line 0 mismatch: '{result.Lines[0]}'");
        Assert(result.Lines[1] == "Ikinci Satir", $"Line 1 mismatch: '{result.Lines[1]}'");
        Assert(result.Lines[2] == "Ucuncu Satir", $"Line 2 mismatch: '{result.Lines[2]}'");
        Assert(result.ExtractedFontFamily == "romans.shx", $"Extracted font mismatch: '{result.ExtractedFontFamily}'");
        Assert(result.PlainText == "Ilk Satir\nIkinci Satir\nUcuncu Satir", $"PlainText mismatch: '{result.PlainText}'");

        // Test non-breaking space and escaped braces
        var special = @"Bir\~Iki\{\}Uc";
        var specialResult = MTextParser.Parse(special);
        Assert(specialResult.PlainText == "Bir Iki{}Uc", $"Special escape failed: '{specialResult.PlainText}'");

        // Test stacked fraction
        var fraction = @"\S1^2;";
        var fracResult = MTextParser.Parse(fraction);
        Assert(fracResult.PlainText == "1/2", $"Fraction failed: '{fracResult.PlainText}'");
    }

    private static void TestBoundedMTextParserNestingAndDepthGuard()
    {
        // 1. Normal nested group
        var nested = @"{Grup 1 {Grup 2 {Grup 3 Derin}}}";
        var nestedResult = MTextParser.Parse(nested);
        Assert(nestedResult.PlainText == "Grup 1 Grup 2 Grup 3 Derin", $"Normal nesting failed: '{nestedResult.PlainText}'");

        // 2. Excessively deep nesting exceeding budget of 32
        var sb = new StringBuilder();
        for (var i = 0; i < 40; i++) sb.Append('{');
        sb.Append("Asiri Derin");
        for (var i = 0; i < 40; i++) sb.Append('}');

        var diagnostics = new List<SceneDiagnostic>();
        var deepResult = MTextParser.Parse(sb.ToString(), diagnostics);

        Assert(diagnostics.Any(d => d.Code == "MTEXT_NESTING_EXCEEDED"), "Expected MTEXT_NESTING_EXCEEDED diagnostic");
        Assert(deepResult.PlainText.Contains("Asiri Derin", StringComparison.Ordinal), "Deep text content must be retained");
    }

    private static void TestFontSubstitutionTableKnownShx()
    {
        var diagnostics = new List<SceneDiagnostic>();

        var resolvedTxt = FontSubstitutionResolver.Resolve("txt.shx", diagnostics);
        Assert(resolvedTxt == "sans-serif", $"txt.shx mapped to {resolvedTxt}");

        var resolvedRomans = FontSubstitutionResolver.Resolve("romans.shx", diagnostics);
        Assert(resolvedRomans == "sans-serif", $"romans.shx mapped to {resolvedRomans}");

        var resolvedSimplex = FontSubstitutionResolver.Resolve("simplex.shx", diagnostics);
        Assert(resolvedSimplex == "sans-serif", $"simplex.shx mapped to {resolvedSimplex}");

        var resolvedMono = FontSubstitutionResolver.Resolve("monotxt.shx", diagnostics);
        Assert(resolvedMono == "monospace", $"monotxt.shx mapped to {resolvedMono}");

        var resolvedComplex = FontSubstitutionResolver.Resolve("complex.shx", diagnostics);
        Assert(resolvedComplex == "serif", $"complex.shx mapped to {resolvedComplex}");

        Assert(diagnostics.Count >= 5, "Expected diagnostics for all SHX substitutions");
        Assert(diagnostics.All(d => d.Kind == SceneDiagnosticKind.Substituted), "All font diagnostics must be Substituted");
    }

    private static void TestFontSubstitutionUnknownFallback()
    {
        var diagnostics = new List<SceneDiagnostic>();
        var resolved = FontSubstitutionResolver.Resolve("UnknownCustomCadFont_xyz.shx", diagnostics);

        Assert(resolved == "sans-serif", "Unknown font must safely fall back to sans-serif");
        Assert(diagnostics.Any(d => d.Code == "FONT_SUBSTITUTION"), "Expected fallback diagnostic");
    }

    private static void TestTextAlignmentCalculations()
    {
        var tl = CadTextAlignmentHelper.FromAttachmentPoint(CadTextAttachmentPoint.TopLeft);
        Assert(tl == (CadTextHorizontalAlignment.Left, CadTextVerticalAlignment.Top), "TopLeft mismatch");

        var mc = CadTextAlignmentHelper.FromAttachmentPoint(CadTextAttachmentPoint.MiddleCenter);
        Assert(mc == (CadTextHorizontalAlignment.Center, CadTextVerticalAlignment.Middle), "MiddleCenter mismatch");

        var br = CadTextAlignmentHelper.FromAttachmentPoint(CadTextAttachmentPoint.BottomRight);
        Assert(br == (CadTextHorizontalAlignment.Right, CadTextVerticalAlignment.Bottom), "BottomRight mismatch");
    }

    private static void TestTextMirrorFlagsAndRotation()
    {
        var text = new TextPrimitive(
            "TEST",
            new WorldPoint2(100, 100),
            height: 10,
            rotationRadians: Math.PI / 2d, // 90 degrees CCW
            widthFactor: 1d,
            obliqueAngleRadians: 0d,
            horizontalAlignment: CadTextHorizontalAlignment.Left,
            verticalAlignment: CadTextVerticalAlignment.Baseline,
            mirrorFlags: CadTextMirrorFlags.Backward);

        Assert(text.MirrorFlags.HasFlag(CadTextMirrorFlags.Backward), "Backward mirror flag missing");
        Assert(!text.MirrorFlags.HasFlag(CadTextMirrorFlags.UpsideDown), "UpsideDown flag should not be set");
        Assert(text.Bounds.Width > 0, "Text bounds width must be positive");
        Assert(text.Bounds.Height > 0, "Text bounds height must be positive");
    }

    private static void TestTextWorldBoundsCalculation()
    {
        var text = new TextPrimitive(
            "ODAM 101",
            new WorldPoint2(50, 50),
            height: 5,
            rotationRadians: 0d,
            widthFactor: 1d,
            horizontalAlignment: CadTextHorizontalAlignment.Left,
            verticalAlignment: CadTextVerticalAlignment.Bottom);

        Assert(text.Bounds.MinX >= 49.999d, "Bounds MinX mismatch");
        Assert(text.Bounds.MinY >= 49.999d, "Bounds MinY mismatch");
        Assert(text.Bounds.MaxY >= 54.999d, "Bounds MaxY should reflect text height");
        Assert(text.Bounds.Width > text.Height, "Width of multi-char text should exceed height");
    }

    private static void TestSkiaTextRenderDarkAndLight()
    {
        // Build a scene with Turkish text and MTEXT
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        var turkishText = new TextPrimitive(
            "TÜRKÇE CAD YAZISI: ŞİĞÖÜç",
            new WorldPoint2(0, 0),
            height: 10,
            requestedFont: "romans.shx");

        var multiLineText = new TextPrimitive(
            "Satir 1\nSatir 2",
            new WorldPoint2(0, -25),
            height: 8,
            requestedFont: "txt.shx");

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("T-001"),
            new RenderLayerToken("TEXT_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [turkishText]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("T-002"),
            new RenderLayerToken("TEXT_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("MTEXT"),
            [multiLineText]));

        var scene = assembler.Build();

        // Render on dark context
        var darkResult = SkiaScenePngRenderer.RenderFitWithStatsAsync(scene, 800, 600).AsTask().GetAwaiter().GetResult();
        Assert(darkResult.Png.Length > 0, "Dark render PNG must not be empty");
        Assert(darkResult.NonBackgroundPixels > 0, "Dark render must have non-background pixels");

        // Verify PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
        Assert(darkResult.Png[0] == 0x89 && darkResult.Png[1] == 0x50 && darkResult.Png[2] == 0x4E && darkResult.Png[3] == 0x47, "Invalid PNG magic");

        // Render on light context
        var lightAssembler = new RenderSceneAssembler(RenderColorContext.Light);
        lightAssembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("T-001"),
            new RenderLayerToken("TEXT_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [turkishText]));
        var lightScene = lightAssembler.Build();

        var lightResult = SkiaScenePngRenderer.RenderFitWithStatsAsync(lightScene, 800, 600).AsTask().GetAwaiter().GetResult();
        Assert(lightResult.Png.Length > 0, "Light render PNG must not be empty");
        Assert(lightResult.NonBackgroundPixels > 0, "Light render must have non-background pixels");
    }

    private static void TestTextSceneSemanticSnapshotDeterminism()
    {
        var assembler = new RenderSceneAssembler();
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT-A"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("Alpha", new WorldPoint2(10, 20), height: 5, requestedFont: "simplex.shx")]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT-B"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("Beta", new WorldPoint2(30, 40), height: 5, requestedFont: "romans.shx")]));

        var scene = assembler.Build();
        var snapshot1 = TextSceneSemanticSnapshot.Create(scene);
        var snapshot2 = TextSceneSemanticSnapshot.Create(scene);

        Assert(snapshot1 == snapshot2, "Semantic snapshot must be deterministic");
        Assert(snapshot1.Contains("schema=text-scene/v1", StringComparison.Ordinal), "Snapshot must have correct schema");
        Assert(snapshot1.Contains("text_entity=TXT-A", StringComparison.Ordinal), "Snapshot must contain TXT-A");
        Assert(snapshot1.Contains("text_entity=TXT-B", StringComparison.Ordinal), "Snapshot must contain TXT-B");
        Assert(snapshot1.Contains("font=simplex.shx->sans-serif", StringComparison.Ordinal), "Snapshot must record font mapping");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Assertion failed: {message}");
        }
    }
}
