#if A14_VALIDATION
using System.Security.Cryptography;
using System.Text;
using Android.Util;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;

namespace MobilDwg.App;

public sealed record A14ValidationResult(
    byte[] Png,
    string PngSha256,
    int TextEntityCount,
    string Marker);

public static class A14AndroidValidationRunner
{
    public const string Tag = "MobilDwgA14";

    public static async Task<A14ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A14_ANDROID_VALIDATION_STARTING");
        await Task.Delay(250);

        // 1. Invariant 1: Turkish Character & Unicode Decoding
        byte[] cp1254Bytes = [0xC7, 0xE7, 0xD0, 0xF0, 0xDD, 0xFD, 0xD6, 0xF6, 0xDE, 0xFE, 0xDC, 0xFC];
        var cp1254Decoded = CadTextEncoding.DecodeCp1254(cp1254Bytes);
        if (cp1254Decoded != "ÇçĞğİıÖöŞşÜü")
        {
            throw new InvalidOperationException($"CP1254 decoding mismatch: '{cp1254Decoded}'");
        }

        var utf8Bytes = Encoding.UTF8.GetBytes("Türkçe: Şanlıurfa, Ağrı, Eskişehir, İzmir, Çankaya, Ödemiş");
        var utf8Decoded = CadTextEncoding.DecodeBytes(utf8Bytes);
        if (!utf8Decoded.Contains("Şanlıurfa", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("UTF-8 decoding failed on Android.");
        }
        Log.Info(Tag, "A14_ANDROID_TURKISH_UNICODE_PASS");

        // 2. Invariant 2: AutoCAD Escape Sequences
        var escapedInput = @"Bina \U+00C7izimi: \U+011Eiri\U+015F, Aci: 45%%d, Boru: %%c100, Tolerans: %%p0.01, Esim: 100%%%";
        var escapedDecoded = CadTextEncoding.DecodeAutoCadEscapes(escapedInput);
        if (!escapedDecoded.Contains("Çizimi: Ğiriş", StringComparison.Ordinal) ||
            !escapedDecoded.Contains("45°", StringComparison.Ordinal) ||
            !escapedDecoded.Contains("Ø100", StringComparison.Ordinal) ||
            !escapedDecoded.Contains("±0.01", StringComparison.Ordinal) ||
            !escapedDecoded.Contains("100%", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"AutoCAD escape decoding failed: '{escapedDecoded}'");
        }
        Log.Info(Tag, "A14_ANDROID_AUTOCAD_ESCAPES_PASS");

        // 3. Invariant 3: Bounded MTEXT Parser
        var mtext = @"\A1;\C2;\H3.5;\Fromans.shx;Zemin Kat\PBirinci Kat\PIkinci Kat";
        var mtextResult = MTextParser.Parse(mtext);
        if (mtextResult.Lines.Count != 3 || mtextResult.ExtractedFontFamily != "romans.shx")
        {
            throw new InvalidOperationException("MTEXT multi-line parsing failed.");
        }

        var deepNestingSb = new StringBuilder();
        for (var i = 0; i < 40; i++) deepNestingSb.Append('{');
        deepNestingSb.Append("Derin Metin");
        for (var i = 0; i < 40; i++) deepNestingSb.Append('}');
        var deepDiagnostics = new List<SceneDiagnostic>();
        var deepResult = MTextParser.Parse(deepNestingSb.ToString(), deepDiagnostics);
        if (!deepDiagnostics.Any(d => d.Code == "MTEXT_NESTING_EXCEEDED") ||
            !deepResult.PlainText.Contains("Derin Metin", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("MTEXT nesting guard failed.");
        }
        Log.Info(Tag, "A14_ANDROID_BOUNDED_MTEXT_PASS");

        // 4. Invariant 4: Audited Font Substitution (Zero Proprietary Font Policy)
        var fontDiagnostics = new List<SceneDiagnostic>();
        var shxTxt = FontSubstitutionResolver.Resolve("txt.shx", fontDiagnostics);
        var shxRomans = FontSubstitutionResolver.Resolve("romans.shx", fontDiagnostics);
        var shxMono = FontSubstitutionResolver.Resolve("monotxt.shx", fontDiagnostics);
        var shxComplex = FontSubstitutionResolver.Resolve("complex.shx", fontDiagnostics);
        var unknownFont = FontSubstitutionResolver.Resolve("custom_arch_font.shx", fontDiagnostics);

        if (shxTxt != "sans-serif" || shxRomans != "sans-serif" ||
            shxMono != "monospace" || shxComplex != "serif" || unknownFont != "sans-serif")
        {
            throw new InvalidOperationException("Font substitution table resolution failed.");
        }
        if (fontDiagnostics.Count < 5 || fontDiagnostics.Any(d => d.Kind != SceneDiagnosticKind.Substituted))
        {
            throw new InvalidOperationException("Font substitution diagnostics failed.");
        }
        Log.Info(Tag, "A14_ANDROID_FONT_SUBSTITUTION_PASS");

        // 5. Invariant 5: Alignment, Rotation & Mirroring
        var tl = CadTextAlignmentHelper.FromAttachmentPoint(CadTextAttachmentPoint.TopLeft);
        var mc = CadTextAlignmentHelper.FromAttachmentPoint(CadTextAttachmentPoint.MiddleCenter);
        var br = CadTextAlignmentHelper.FromAttachmentPoint(CadTextAttachmentPoint.BottomRight);

        if (tl != (CadTextHorizontalAlignment.Left, CadTextVerticalAlignment.Top) ||
            mc != (CadTextHorizontalAlignment.Center, CadTextVerticalAlignment.Middle) ||
            br != (CadTextHorizontalAlignment.Right, CadTextVerticalAlignment.Bottom))
        {
            throw new InvalidOperationException("Text alignment conversion failed.");
        }

        var mirrorText = new TextPrimitive(
            "AYNA",
            new WorldPoint2(50, 50),
            height: 10,
            rotationRadians: Math.PI / 4d,
            mirrorFlags: CadTextMirrorFlags.Backward);

        if (!mirrorText.MirrorFlags.HasFlag(CadTextMirrorFlags.Backward) ||
            mirrorText.Bounds.Width <= 0 || mirrorText.Bounds.Height <= 0)
        {
            throw new InvalidOperationException("Mirrored text primitive bounds failed.");
        }
        Log.Info(Tag, "A14_ANDROID_ALIGNMENT_MIRROR_PASS");

        // 6. Invariant 6: Real Skia CAD Rendering of Text Scene
        var layerTable = new LayerTable();
        layerTable.AddOrUpdate(new LayerDefinition("TEXT_LAYER", CadColor.FromAci(2), CadLinetype.Continuous, CadLineweight.FromMm(0.35)));
        layerTable.AddOrUpdate(new LayerDefinition("FRAME_LAYER", CadColor.FromAci(4), CadLinetype.Continuous, CadLineweight.FromMm(0.50)));
        layerTable.AddOrUpdate(new LayerDefinition("CYAN_TEXT", CadColor.FromAci(4), CadLinetype.Continuous, CadLineweight.Default));
        layerTable.AddOrUpdate(new LayerDefinition("GREEN_TEXT", CadColor.FromAci(3), CadLinetype.Continuous, CadLineweight.Default));

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.SetLayerTable(layerTable);

        // Frame around drawing
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("FRAME-001"),
            new RenderLayerToken("FRAME_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("POLYLINE"),
            [
                new PolylinePrimitive([
                    new PolylineVertex(new WorldPoint2(-50, -50)),
                    new PolylineVertex(new WorldPoint2(150, -50)),
                    new PolylineVertex(new WorldPoint2(150, 100)),
                    new PolylineVertex(new WorldPoint2(-50, 100)),
                ], closed: true)
            ]));

        // Turkish Header
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT-HEADER"),
            new RenderLayerToken("TEXT_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive(
                    "MİMARİ PROJE: GİRİŞ & ŞANTİYE",
                    new WorldPoint2(50, 80),
                    height: 9,
                    horizontalAlignment: CadTextHorizontalAlignment.Center,
                    verticalAlignment: CadTextVerticalAlignment.Middle,
                    requestedFont: "romans.shx")
            ]));

        // Multi-line specification (MTEXT lines)
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT-SPEC-1"),
            new RenderLayerToken("CYAN_TEXT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive(
                    "Boru Capi: \u00D850 mm | Aci: 45\u00B0",
                    new WorldPoint2(-30, 45),
                    height: 6,
                    requestedFont: "simplex.shx")
            ]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT-SPEC-2"),
            new RenderLayerToken("GREEN_TEXT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive(
                    "Tolerans: \u00B10.02 mm | Alan: 120 m2",
                    new WorldPoint2(-30, 25),
                    height: 6,
                    requestedFont: "txt.shx")
            ]));

        // Rotated vertical axis label (90 degrees)
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT-AXIS"),
            new RenderLayerToken("TEXT_LAYER"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive(
                    "AKS 1-A (DİKEY)",
                    new WorldPoint2(-40, -30),
                    height: 6,
                    rotationRadians: Math.PI / 2d,
                    requestedFont: "romans.shx")
            ]));

        // Mirrored text
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT-MIRROR"),
            new RenderLayerToken("CYAN_TEXT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [
                new TextPrimitive(
                    "AYNALI METİN",
                    new WorldPoint2(50, -30),
                    height: 6,
                    mirrorFlags: CadTextMirrorFlags.Backward,
                    requestedFont: "simplex.shx")
            ]));

        var scene = assembler.Build();

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

        if (renderResult.NonBackgroundPixels < 100)
        {
            throw new InvalidOperationException($"Too few non-background pixels rendered: {renderResult.NonBackgroundPixels}");
        }

        var pngSha256 = Convert.ToHexString(SHA256.HashData(pngBytes)).ToLowerInvariant();
        Log.Info(Tag, $"A14_ANDROID_SKIA_TEXT_PNG_PASS bytes={pngBytes.Length} nonBgPixels={renderResult.NonBackgroundPixels} sha256={pngSha256}");
        Log.Info(Tag, "ANDROID_STAGE14_TEXT_FONT_PASS");

        var textEntityCount = scene.Entities.Count;
        return new A14ValidationResult(pngBytes, pngSha256, textEntityCount, "ANDROID_STAGE14_TEXT_FONT_PASS");
    }
}
#endif
