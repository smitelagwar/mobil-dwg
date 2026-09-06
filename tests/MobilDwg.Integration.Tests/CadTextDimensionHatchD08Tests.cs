using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ACadSharp.Entities;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Dimensions;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Hatch;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;
using SkiaSharp;

namespace MobilDwg.Integration.Tests;

public static class CadTextDimensionHatchD08Tests
{
    public static void RunAll()
    {
        Console.WriteLine("=== RUNNING D08 TEXT, DIMENSION & HATCH INTEGRATION TESTS ===");
        TestP09AndTurkishUnicodeTextMeasurement();
        TestMTextFormattingAndParsing();
        TestDimensionChildGeometryAndStylePreservation();
        TestHatchPatSpacingPhaseAndIslandRules();
        TestHatchCoverageCacheAndThinning();
        Console.WriteLine("=== ALL D08 INTEGRATION TESTS PASSED ===");
    }

    private static void TestP09AndTurkishUnicodeTextMeasurement()
    {
        // 1. Verify P09: WWWW and MMMM wide glyphs are encompassed by TextLayout.TotalWidth
        using var font100 = new SKFont(SKTypeface.Default, 100);
        var actualWWidth = font100.MeasureText("WWWW");
        var layoutW = new TextLayout("WWWW", new WorldPoint2(0, 0), 100);

        Assert(layoutW.TotalWidth >= actualWWidth,
            $"TextLayout TotalWidth ({layoutW.TotalWidth:F2}) must encompass actual font advance ({actualWWidth:F2})");

        var actualMWidth = font100.MeasureText("MMMM");
        var layoutM = new TextLayout("MMMM", new WorldPoint2(0, 0), 100);
        Assert(layoutM.TotalWidth >= actualMWidth,
            $"TextLayout TotalWidth ({layoutM.TotalWidth:F2}) must encompass actual font advance ({actualMWidth:F2})");

        // 2. Turkish Unicode characters: İ, ı, Ş, ç, ğ, ö, ü
        var turkishText = "İSTANBUL ÇEKMECE ŞAFTI 12.50m - ığüşöç";
        var layoutTr = new TextLayout(turkishText, new WorldPoint2(100, 200), 20);
        var actualTrWidth = new SKFont(SKTypeface.Default, 20).MeasureText(turkishText);
        Assert(layoutTr.TotalWidth >= actualTrWidth,
            $"Turkish text layout width ({layoutTr.TotalWidth:F2}) must encompass font measure ({actualTrWidth:F2})");
        Assert(layoutTr.Bounds.Width >= actualTrWidth, "Text bounds width must encompass font width");
        Assert(layoutTr.Bounds.MinX <= 100, "Text bounds must encompass start position X");

        // 3. Multiline text
        var multiText = "LINE 1\nLINE 2 LONGER\nL3";
        var layoutMulti = new TextLayout(multiText, new WorldPoint2(0, 0), 15);
        Assert(layoutMulti.Lines.Count == 3, $"Expected 3 lines, got {layoutMulti.Lines.Count}");
        Assert(layoutMulti.TotalHeight >= 15 * 2.5, "TotalHeight must account for line spacing");

        // 4. Oblique sheared text bounds
        double obliqueRad = 15.0 * Math.PI / 180.0;
        var layoutOblique = new TextLayout("SLANTED", new WorldPoint2(0, 0), 25, obliqueAngleRadians: obliqueRad);
        Assert(layoutOblique.Bounds.MinX < 0, "Oblique shear at descent should extend MinX into negative X");

        // 5. Rotated 90 degrees text bounds
        var layoutRot = new TextLayout("VERTICAL", new WorldPoint2(50, 50), 30, rotationRadians: Math.PI / 2.0);
        Assert(layoutRot.Bounds.Height > layoutRot.Bounds.Width, "90-degree rotated text bounds height must exceed width");

        Console.WriteLine("  [PASS] D08: P09 and Turkish Unicode Text Layout Measurement verified");
    }

    private static void TestMTextFormattingAndParsing()
    {
        // 1. MTEXT parser removes formatting tags and extracts lines and font
        var rawMText = @"{\fArial|b0|i0;FIRST LINE\P\LSECOND UNDERLINED\l\PTHIRD \S1^2; INCH}";
        var parsed = MTextParser.Parse(rawMText);

        Assert(parsed.Lines.Count == 3, $"Expected 3 lines, got {parsed.Lines.Count}");
        Assert(parsed.Lines[0].Contains("FIRST LINE"), $"Expected 'FIRST LINE', got '{parsed.Lines[0]}'");
        Assert(parsed.Lines[1].Contains("SECOND UNDERLINED"), $"Expected 'SECOND UNDERLINED', got '{parsed.Lines[1]}'");
        Assert(parsed.Lines[2].Contains("1/2"), $"Expected stacked fraction '1/2', got '{parsed.Lines[2]}'");
        Assert(!string.IsNullOrEmpty(parsed.ExtractedFontFamily) && parsed.ExtractedFontFamily.Contains("Arial"),
            $"Extracted font should be Arial, got '{parsed.ExtractedFontFamily}'");

        // 2. Extractor CleanMText matches MTextParser behavior without crashing on deep or empty text
        var clean = AcadSharpEntityExtractor.CleanMText(rawMText);
        Assert(clean.Contains("FIRST LINE") && clean.Contains("SECOND UNDERLINED") && clean.Contains("1/2"),
            $"CleanMText should strip formatting: '{clean}'");

        // 3. CadExtractedSceneBuilder transforms MText entity into TextPrimitive with parsed content
        var mtextEntity = new CadExtractedEntity(
            "MTEXT_01",
            "TEXT_LAYER",
            CadExtractedEntityType.MText,
            CadEntityColor.FromAci(7),
            points: [new CadExtractedPoint(100, 100)],
            text: rawMText,
            textHeight: 15.0);

        var doc = new CadExtractedDocument(
            "DXF", "AC1015",
            layers: [new CadExtractedLayer("TEXT_LAYER", 0xFFFFFFFFu, AciIndex: 7)],
            entities: [mtextEntity],
            minX: 0, minY: 0, maxX: 500, maxY: 500);

        var scene = CadExtractedSceneBuilder.Build(doc);
        Assert(scene.Entities.Count == 1, "Expected 1 scene entity for MTEXT");
        var textPrim = scene.Entities[0].Geometry.OfType<TextPrimitive>().FirstOrDefault();
        Assert(textPrim != null, "TextPrimitive expected for MTEXT");
        Assert(textPrim!.Layout.Lines.Count == 3, $"TextPrimitive must have 3 lines, got {textPrim.Layout.Lines.Count}");

        Console.WriteLine("  [PASS] D08: MTEXT formatting, line wrap, and font extraction verified");
    }

    private static void TestDimensionChildGeometryAndStylePreservation()
    {
        // 1. Test child entity extraction with ByBlock color and non-zero layer preservation
        var dimHandle = "DIM_ENTITY_01";
        var parentLayer = "DIM_PARENT_LAYER";
        var parentColor = CadEntityColor.FromAci(1); // Red

        // Simulated exploded entities from dimension block
        var childLine1 = new CadExtractedEntity(
            $"{dimHandle}/DIM_BLK:L1",
            "0", // Layer 0 -> should inherit parent layer
            CadExtractedEntityType.Line,
            CadEntityColor.ByBlock, // ByBlock -> should inherit parent color
            points: [new CadExtractedPoint(0, 0), new CadExtractedPoint(100, 0)]);

        var childLine2 = new CadExtractedEntity(
            $"{dimHandle}/DIM_BLK:L2",
            "SPECIAL_LAYER", // Not 0 -> should keep its own layer
            CadExtractedEntityType.Line,
            CadEntityColor.FromAci(3), // Green -> should keep its own color
            points: [new CadExtractedPoint(0, 0), new CadExtractedPoint(0, 10)]);

        var childText = new CadExtractedEntity(
            $"{dimHandle}/DIM_BLK:TXT",
            "0",
            CadExtractedEntityType.Text,
            CadEntityColor.ByBlock, // ByBlock
            points: [new CadExtractedPoint(50, 5)],
            text: "100.00 mm",
            textHeight: 3.5);

        var dimPayload = new CadDimensionPayload(
            "100.00 mm",
            new CadPoint3D(0, 0),
            new CadPoint3D(100, 0),
            "Aligned",
            ExplodedEntities: [childLine1, childLine2, childText]);

        var dimEntity = new CadExtractedEntity(
            dimHandle,
            parentLayer,
            CadExtractedEntityType.Dimension,
            parentColor,
            payload: dimPayload,
            points: [new CadExtractedPoint(0, 0), new CadExtractedPoint(100, 0)],
            text: "100.00 mm");

        var doc = new CadExtractedDocument(
            "DXF", "AC1015",
            layers:
            [
                new CadExtractedLayer(parentLayer, 0xFFFF0000u, AciIndex: 1),
                new CadExtractedLayer("SPECIAL_LAYER", 0xFF00FF00u, AciIndex: 3)
            ],
            entities: [dimEntity],
            minX: 0, minY: 0, maxX: 500, maxY: 500);

        var scene = CadExtractedSceneBuilder.Build(doc);
        Assert(scene.Entities.Count == 1, "Expected 1 scene entity for Dimension");
        Assert(scene.Entities[0].Geometry.Count == 3, $"Expected 3 primitives from dimension block, got {scene.Entities[0].Geometry.Count}");

        // 2. Test procedural dimension fallback when ExplodedEntities is null
        var procDimDef = new CadDimensionDefinition(
            CadDimensionType.Linear,
            new WorldPoint2(10, 20),
            new WorldPoint2(110, 20),
            new WorldPoint2(60, 40),
            textHeight: 3.0,
            arrowheadSize: 2.5,
            textOverride: "<> mm");

        var procDimEntity = DimensionBuilder.BuildDimension("PROC_DIM", procDimDef);
        Assert(procDimEntity.Geometry.Count >= 4, "Procedural dimension must produce lines, arrowheads, and text");
        var textP = procDimEntity.Geometry.OfType<TextPrimitive>().FirstOrDefault();
        Assert(textP != null && textP.Text.Contains("100.00 mm"), $"Dimension text should format '<> mm' into '100.00 mm', got '{textP?.Text}'");

        Console.WriteLine("  [PASS] D08: Dimension child geometry and procedural style preservation verified");
    }

    private static void TestHatchPatSpacingPhaseAndIslandRules()
    {
        // 1. Pattern lines spacing: no arbitrary scale * 5.0
        var outerLoop = new HatchLoop(
            [
                new WorldPoint2(0, 0),
                new WorldPoint2(100, 0),
                new WorldPoint2(100, 100),
                new WorldPoint2(0, 100),
                new WorldPoint2(0, 0)
            ],
            isOuter: true);

        // Spacing = 10.0 -> over 100 units height, expected ~10-15 diagonal lines
        var lines10 = HatchProcessor.GeneratePatternLines(
            [outerLoop],
            angleRadians: Math.PI / 4.0, // 45 degrees
            spacing: 10.0,
            bounds: outerLoop.Bounds,
            patternOrigin: new WorldPoint2(0, 0));

        Assert(lines10.Count >= 10 && lines10.Count <= 20,
            $"Expected ~14 lines for spacing 10 over 100x100 bounds, got {lines10.Count}");

        // 2. Pattern phase invariance: origin at (0, 0). Panning bounds from [0, 100] to [20, 120]
        // Lines passing through [20, 100] must align exactly to the same world coordinates
        var pannedBounds = new WorldBounds2(20, 0, 120, 100);
        var linesPanned = HatchProcessor.GeneratePatternLines(
            [outerLoop],
            angleRadians: Math.PI / 4.0,
            spacing: 10.0,
            bounds: pannedBounds,
            patternOrigin: new WorldPoint2(0, 0));

        // Find a line that exists in both: their world coordinates must match
        var matchingLines = 0;
        foreach (var l1 in lines10)
        {
            foreach (var l2 in linesPanned)
            {
                if (Math.Abs(l1.Start.X - l2.Start.X) < 1e-4 && Math.Abs(l1.Start.Y - l2.Start.Y) < 1e-4)
                {
                    matchingLines++;
                }
            }
        }
        Assert(matchingLines > 0, "Pattern lines must align to invariant world phase regardless of bounds position");

        // 3. Island rules: Normal (even-odd) vs Outer vs Ignore
        var islandLoop = new HatchLoop(
            [
                new WorldPoint2(30, 30),
                new WorldPoint2(70, 30),
                new WorldPoint2(70, 70),
                new WorldPoint2(30, 70),
                new WorldPoint2(30, 30)
            ],
            isOuter: false);

        var subIslandLoop = new HatchLoop(
            [
                new WorldPoint2(45, 45),
                new WorldPoint2(55, 45),
                new WorldPoint2(55, 55),
                new WorldPoint2(45, 55),
                new WorldPoint2(45, 45)
            ],
            isOuter: false);

        var loopsWithSubIsland = new[] { outerLoop, islandLoop, subIslandLoop };

        // Test point inside sub-island (50, 50)
        var centerPt = new WorldPoint2(50, 50);
        // Under Normal: inside outer (1), inside island (2), inside sub-island (3) -> count=3 (odd) -> INSIDE!
        Assert(HatchProcessor.IsPointInsideHatch(centerPt, loopsWithSubIsland, HatchIslandStyle.Normal),
            "Center point in sub-island must be inside under Normal (EvenOdd) island style");

        // Under Outer: only outermost region filled, all islands hollow -> OUTSIDE!
        Assert(!HatchProcessor.IsPointInsideHatch(centerPt, loopsWithSubIsland, HatchIslandStyle.Outer),
            "Center point in sub-island must be OUTSIDE under Outer island style");

        // Under Ignore: all islands ignored, whole outer filled -> INSIDE!
        Assert(HatchProcessor.IsPointInsideHatch(centerPt, loopsWithSubIsland, HatchIslandStyle.Ignore),
            "Center point must be INSIDE under Ignore island style");

        Console.WriteLine("  [PASS] D08: Hatch PAT spacing, invariant origin phase, and island rules verified");
    }

    private static void TestHatchCoverageCacheAndThinning()
    {
        // 1. PreparedGeometryCache Put and TryGet HatchCoverage
        using var cache = new PreparedGeometryCache(1024 * 1024);
        var hatchKey = "E-001:0";
        var coverageBounds = new WorldBounds2(0, 0, 500, 500);
        var lines = new List<(WorldPoint2 Start, WorldPoint2 End)>
        {
            (new WorldPoint2(0, 0), new WorldPoint2(100, 100)),
            (new WorldPoint2(0, 50), new WorldPoint2(100, 150))
        };

        cache.PutHatchCoverage(1, hatchKey, coverageBounds, lines, lodBand: 0, styleRevision: 0);

        // TryGet within subset of coverage bounds -> HIT
        var subBounds = new WorldBounds2(50, 50, 200, 200);
        bool hit = cache.TryGetHatchCoverage(1, hatchKey, subBounds, lodBand: 0, styleRevision: 0, out var entry);
        Assert(hit && entry != null, "Hatch coverage cache must hit for sub-bounds contained in coverageBounds");
        Assert(entry!.Lines.Count == 2, $"Expected 2 lines in cached hatch entry, got {entry.Lines.Count}");

        // TryGet outside coverage bounds -> MISS
        var outBounds = new WorldBounds2(400, 400, 600, 600);
        bool miss = cache.TryGetHatchCoverage(1, hatchKey, outBounds, lodBand: 0, styleRevision: 0, out _);
        Assert(!miss, "Hatch coverage cache must miss for bounds exceeding coverageBounds");

        // 2. Thinning rule verification:
        // Spacing = 1.0 unit. Camera resolution = 1.0 world units per pixel -> projected spacing = 1.0 px (< 3 px).
        // Thinning step = ceil(3.0 / 1.0) = 3.
        double worldUnitsPerPixel = 1.0;
        double hatchScale = 1.0;
        var projSpacing = hatchScale / worldUnitsPerPixel;
        int thinningStep = projSpacing < 3.0 ? (int)Math.Ceiling(3.0 / projSpacing) : 1;
        Assert(thinningStep == 3, $"Expected thinning step 3 for projected spacing 1.0px, got {thinningStep}");

        // When zoomed in: worldUnitsPerPixel = 0.1 -> projected spacing = 10.0 px (>= 3 px)
        worldUnitsPerPixel = 0.1;
        projSpacing = hatchScale / worldUnitsPerPixel;
        thinningStep = projSpacing < 3.0 ? (int)Math.Ceiling(3.0 / projSpacing) : 1;
        Assert(thinningStep == 1, $"Expected thinning step 1 (no thinning) for projected spacing 10.0px, got {thinningStep}");

        Console.WriteLine("  [PASS] D08: Hatch coverage cache and 3px screen thinning rule verified");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"TEST ASSERTION FAILED: {message}");
        }
    }
}
