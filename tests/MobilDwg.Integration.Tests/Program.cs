using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Guards;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Dimensions;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Hatch;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Text;
using MobilDwg.Rendering.Transforms;

namespace MobilDwg.Integration.Tests;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var repoRoot = FindRepoRoot();
            var reader = new AcadSharpDocumentReader();

            // 1. Test real DXF fixture with comprehensive extraction assertions
            var dxfPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "synthetic_turkish_basic_ac1015.dxf");
            Assert(File.Exists(dxfPath), $"DXF fixture missing: {dxfPath}");

            await using (var stream = File.OpenRead(dxfPath))
            {
                var request = new CadOpenRequest(stream, Path.GetFileName(dxfPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);

                Assert(session.Metadata.Format == CadFormat.Dxf, $"Expected DXF format, got {session.Metadata.Format}");
                Assert(session.Metadata.AcadVersion == "AC1015", $"Expected AC1015, got {session.Metadata.AcadVersion}");
                Assert(session.Handle != null, "Session handle was null");

                var extracted = AcadSharpEntityExtractor.Extract(session.Handle!);
                Assert(extracted.Entities.Count > 0, $"Expected extracted entities > 0, got {extracted.Entities.Count}");
                Assert(extracted.Layers.Count > 0, $"Expected extracted layers > 0, got {extracted.Layers.Count}");
                Assert(extracted.Format == "DXF", $"Expected extracted document format DXF, got {extracted.Format}");
                Assert(extracted.Version == "AC1015", $"Expected extracted document version AC1015, got {extracted.Version}");

                // Verify Turkish Unicode decoding in text
                bool foundTurkishText = extracted.Entities.Any(e =>
                    e.Text is not null && (e.Text.Contains("İstanbul") || e.Text.Contains("ÇGÖŞÜ") || e.Text.Contains("ıİ")));
                Assert(foundTurkishText, "Turkish CAD text unicode escape sequence (\\U+0130 etc) was not correctly decoded!");

                // Verify nested block expansion and unique IDs (no collisions)
                var handleSet = new HashSet<string>(StringComparer.Ordinal);
                foreach (var ent in extracted.Entities)
                {
                    Assert(handleSet.Add(ent.Handle), $"Duplicate entity handle detected in extracted document: {ent.Handle}");
                }

                bool hasExpandedBlocks = extracted.Entities.Any(e => e.BlockOwner != null);
                Assert(hasExpandedBlocks, "Expected expanded block instance entities from OUTER/INNER insert hierarchy.");

                var scene = CadExtractedSceneBuilder.Build(extracted);
                Assert(scene.Entities.Count > 0, $"Expected scene entities > 0, got {scene.Entities.Count}");
                Assert(scene.WorldBounds.HasValue && scene.WorldBounds.Value.Width > 0 && scene.WorldBounds.Value.Height > 0,
                    $"Expected positive world bounds, got {scene.WorldBounds}");

                // Verify every scene entity has a populated CadEntityStyle
                bool allHaveCadStyle = scene.Entities.All(e => e.CadStyle != null);
                Assert(allHaveCadStyle, "Not all scene entities had a populated CadEntityStyle!");

                // Verify original draw order is maintained
                int lastOrder = -1;
                foreach (var ent in scene.Entities)
                {
                    int ord = ent.Source.SourceIndex ?? int.MaxValue;
                    Assert(ord >= lastOrder, $"Draw order was not monotonically non-decreasing: {ord} < {lastOrder}");
                    lastOrder = ord;
                }
            }

            // 2. Test real DWG fixture if present
            var dwgPath = Path.Combine(repoRoot, "artifacts", "stage03", "synthetic_turkish_basic_ac1015.dwg");
            if (File.Exists(dwgPath))
            {
                await using var stream = File.OpenRead(dwgPath);
                var request = new CadOpenRequest(stream, Path.GetFileName(dwgPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);

                Assert(session.Metadata.Format == CadFormat.Dwg, $"Expected DWG format, got {session.Metadata.Format}");
                Assert(session.Handle != null, "DWG session handle was null");

                var extracted = AcadSharpEntityExtractor.Extract(session.Handle!);
                Assert(extracted.Entities.Count > 0, "DWG extracted entity count was 0");
                Assert(extracted.Format == "DWG", $"Expected extracted document format DWG, got {extracted.Format}");

                var scene = CadExtractedSceneBuilder.Build(extracted);
                Assert(scene.Entities.Count > 0, "DWG scene entity count was 0");
                Assert(scene.Entities.All(e => e.CadStyle != null), "DWG scene entities must have populated CadEntityStyle");
            }

            // 3. Test resource budget guards and truncation
            await using (var stream = File.OpenRead(dxfPath))
            {
                var request = new CadOpenRequest(stream, Path.GetFileName(dxfPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);

                var tightBudget = new CadBudgetGuard(new CadResourceBudget { MaxEntities = 2 });
                var truncated = AcadSharpEntityExtractor.Extract(session.Handle!, tightBudget);

                Assert(truncated.Entities.Count <= 2, $"Expected entity count truncated to <= 2, got {truncated.Entities.Count}");
                Assert(truncated.Diagnostics.Any(d => d.Code == "RESOURCE_BUDGET_EXCEEDED_ENTITIES"),
                    "Expected RESOURCE_BUDGET_EXCEEDED_ENTITIES diagnostic in truncated document.");
                Assert(!truncated.IsFullyCompliant, "Truncated document should not report IsFullyCompliant == true");
            }

            // 4. Test negative fixtures (missing font, missing xref)
            var negFontPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "negative_missing_font_ac1015.dxf");
            if (File.Exists(negFontPath))
            {
                await using var stream = File.OpenRead(negFontPath);
                var request = new CadOpenRequest(stream, Path.GetFileName(negFontPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);
                Assert(session.Handle != null, "Negative font session handle was null");
            }

            var negXrefPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "negative_missing_xref_ac1015.dxf");
            if (File.Exists(negXrefPath))
            {
                await using var stream = File.OpenRead(negXrefPath);
                var request = new CadOpenRequest(stream, Path.GetFileName(negXrefPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);
                Assert(session.Handle != null, "Negative xref session handle was null");
            }

            RunStage09GeometryTests();
            RunStage10TextDimensionHatchTests();

            Console.WriteLine("STAGE01_INTEGRATION_TESTS_PASS");
            Console.WriteLine("STAGE08_CAD_EXTRACTION_TESTS_PASS");
            Console.WriteLine("STAGE09_GEOMETRY_TESTS_PASS");
            Console.WriteLine("STAGE10_TEXT_DIMENSION_HATCH_PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"INTEGRATION_TESTS_FAILED: {ex}");
            return 1;
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "MobilDwg.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not find repo root with MobilDwg.sln");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void RunStage09GeometryTests()
    {
        // 1. Closed polyline with bulge from last vertex to first vertex
        var polyVertices = new[]
        {
            new PolylineVertex(new WorldPoint2(0, 0), bulge: 1.0), // semi-circle to (10, 0)
            new PolylineVertex(new WorldPoint2(10, 0), bulge: 1.0), // semi-circle back to (0, 0)
        };
        var closedBulgePoly = new PolylinePrimitive(polyVertices, closed: true);
        var tessOptions = new GeometryTessellationOptions(maxChordError: 0.01, minSegments: 4, maxSegments: 1024);
        var tessPoly = GeometryTessellator.Tessellate(closedBulgePoly, tessOptions);
        Assert(tessPoly.Closed, "Closed bulge polyline must produce closed tessellation");
        Assert(tessPoly.Points.Count >= 8, $"Expected >= 8 points for two semi-circle bulge segments, got {tessPoly.Points.Count}");
        Assert(closedBulgePoly.Bounds.Height > 0, "Bounds of closed bulge polyline must be positive");

        // 2. Zero-length polyline segment resilience
        var degenerateVerts = new[]
        {
            new PolylineVertex(new WorldPoint2(5, 5), bulge: 0.5),
            new PolylineVertex(new WorldPoint2(5, 5), bulge: 0.0), // identical point
            new PolylineVertex(new WorldPoint2(10, 5), bulge: 0.0),
        };
        var degeneratePoly = new PolylinePrimitive(degenerateVerts, closed: false);
        var tessDegen = GeometryTessellator.Tessellate(degeneratePoly, tessOptions);
        Assert(tessDegen.Points.Count >= 2, "Degenerate polyline must tessellate without crashing");

        // 3. Spline with rational weights and adaptive subdivision
        var ctrlPoints = new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(5, 10),
            new WorldPoint2(10, 0),
        };
        var knots = new double[] { 0, 0, 0, 1, 1, 1 }; // degree 2
        var weights = new double[] { 1.0, 5.0, 1.0 }; // strong weight on peak
        var weightedSpline = new SplinePrimitive(2, ctrlPoints, knots, weights);
        var tessSpline = GeometryTessellator.Tessellate(weightedSpline, tessOptions);
        Assert(tessSpline.Points.Count >= 4, $"Spline tessellation produced {tessSpline.Points.Count} points");
        var maxY = tessSpline.Points.Max(p => p.Y);
        Assert(maxY > 7.0, $"Weighted spline should pull curve towards weight, peak was {maxY}");

        // 4. Block definition with base point offset and non-uniform scale
        var circle = new ArcPrimitive(new WorldPoint2(10, 10), 5.0, 0, Math.PI * 2);
        var line = new LinePrimitive(new WorldPoint2(10, 10), new WorldPoint2(20, 10));
        var blockDef = new BlockDefinition(
            "TEST_NONUNIFORM",
            basePoint: new WorldPoint2(10, 10), // Base point at center
            entities: new[]
            {
                new BlockEntityTemplate(circle, new RenderLayerToken("0"), new RenderStyleToken("BYLAYER")),
                new BlockEntityTemplate(line, new RenderLayerToken("0"), new RenderStyleToken("BYLAYER")),
            });

        // Insert at (100, 200), scaled 2x in X, 1x in Y (non-uniform scale!)
        var nonUniformInsert = new BlockReference(
            "TEST_NONUNIFORM",
            insertionPoint: new WorldPoint2(100, 200),
            scaleX: 2.0,
            scaleY: 1.0,
            rotationRadians: 0);

        var expander = new BlockExpander(new[] { blockDef });
        var expansion = expander.Expand(new[] { nonUniformInsert });
        Assert(expansion.Entities.Count == 2, $"Expected 2 expanded entities, got {expansion.Entities.Count}");

        // The circle under non-uniform scale (sx=2, sy=1) must become EllipsePrimitive with major radius 10, minor radius 5!
        var geomCircle = expansion.Entities[0].Geometry[0];
        Assert(geomCircle is EllipsePrimitive, $"Expected circle under non-uniform scale to become EllipsePrimitive, got {geomCircle.GetType().Name}");
        var ellipse = (EllipsePrimitive)geomCircle;
        Assert(Math.Abs(ellipse.Center.X - 100) < 1e-6 && Math.Abs(ellipse.Center.Y - 200) < 1e-6,
            $"Expected ellipse center at (100, 200), got ({ellipse.Center.X}, {ellipse.Center.Y})");
        Assert(Math.Abs(ellipse.MajorRadius - 10.0) < 1e-6, $"Expected major radius 10.0, got {ellipse.MajorRadius}");
        Assert(Math.Abs(ellipse.MinorRadius - 5.0) < 1e-6, $"Expected minor radius 5.0, got {ellipse.MinorRadius}");

        // The line from (10, 10) to (20, 10) with base point (10, 10):
        // local start (10-10)=0 * 2 = 0 -> 100
        // local end (20-10)=10 * 2 = 20 -> 120
        var geomLine = expansion.Entities[1].Geometry[0];
        Assert(geomLine is LinePrimitive, "Expected LinePrimitive");
        var linePrim = (LinePrimitive)geomLine;
        Assert(Math.Abs(linePrim.Start.X - 100) < 1e-6 && Math.Abs(linePrim.Start.Y - 200) < 1e-6, "Line start mismatch");
        Assert(Math.Abs(linePrim.End.X - 120) < 1e-6 && Math.Abs(linePrim.End.Y - 200) < 1e-6, "Line end mismatch");

        // 5. MINSERT 2x2 Array Expansion
        var arrayInsert = new BlockReference(
            "TEST_NONUNIFORM",
            insertionPoint: new WorldPoint2(0, 0),
            scaleX: 1.0,
            scaleY: 1.0,
            rotationRadians: 0,
            columnCount: 2,
            rowCount: 2,
            columnSpacing: 50.0,
            rowSpacing: 100.0);

        var arrayExpander = new BlockExpander(new[] { blockDef });
        var arrayExpansion = arrayExpander.Expand(new[] { arrayInsert });
        // 2x2 array of 2 entities = 8 entities
        Assert(arrayExpansion.Entities.Count == 8, $"Expected 8 entities for 2x2 MINSERT, got {arrayExpansion.Entities.Count}");

        // 6. Mirrored Arc
        var arc = new ArcPrimitive(new WorldPoint2(0, 0), 10.0, 0, Math.PI / 2.0);
        var mirrorTransform = Transform2D.CreateScale(-1.0, 1.0);
        var mirroredGeom = PrimitiveTransformer.Transform(arc, mirrorTransform);
        Assert(mirroredGeom is ArcPrimitive, "Mirrored arc must remain ArcPrimitive");
        var mirroredArc = (ArcPrimitive)mirroredGeom;
        Assert(mirroredArc.SweepRadians < 0, "Mirrored arc sweep must be negative / clockwise");
        Assert(Math.Abs(Math.Abs(mirroredArc.SweepRadians) - Math.PI / 2.0) < 1e-6, "Mirrored arc sweep magnitude mismatch");

        Console.WriteLine("STAGE09_GEOMETRY_BLOCK_TESTS_PASS");
    }

    private static void RunStage10TextDimensionHatchTests()
    {
        // 1. Turkish text & unicode escapes in Extracted Document -> RenderScene (TextPrimitive)
        var doc = new CadExtractedDocument(
            "DXF",
            "AC1015",
            new[] { new CadExtractedLayer("TEXT_LAYER", 0xFFFFFFFF, 1, true) },
            new[]
            {
                new CadExtractedEntity(
                    "T01",
                    "TEXT_LAYER",
                    CadExtractedEntityType.Text,
                    new CadEntityColor(CadColorMethod.Index, 1, 0),
                    points: new[] { new CadExtractedPoint(10, 20) },
                    text: "İstanbul Şemsiyesi Örtüsü Çiçeği Ağacı Ğülüşü ıİ",
                    textHeight: 12.0,
                    rotation: 0.0,
                    payload: new CadTextPayload(
                        "İstanbul Şemsiyesi Örtüsü Çiçeği Ağacı Ğülüşü ıİ",
                        new CadPoint3D(10, 20, 0),
                        12.0,
                        0.0,
                        FontName: "romans.shx",
                        WidthFactor: 1.0,
                        HorizontalAlignment: 0,
                        VerticalAlignment: 0))
            },
            0, 0, 100, 100);

        var scene = CadExtractedSceneBuilder.Build(doc);
        Assert(scene.Entities.Count == 1, $"Expected 1 entity in scene, got {scene.Entities.Count}");
        var textEntity = scene.Entities[0];
        Assert(textEntity.Geometry.Count == 1, $"Expected 1 primitive, got {textEntity.Geometry.Count}");
        Assert(textEntity.Geometry[0] is TextPrimitive, "Expected TextPrimitive");
        var textPrim = (TextPrimitive)textEntity.Geometry[0];
        Assert(textPrim.Text.Contains("İstanbul", StringComparison.Ordinal), "TextPrimitive text must preserve Turkish characters");
        Assert(textPrim.ResolvedFont == "sans-serif", $"SHX font 'romans.shx' should be resolved to 'sans-serif', got '{textPrim.ResolvedFont}'");
        Assert(textPrim.Layout.Lines.Count == 1, "Expected single line text layout");

        // 2. MTEXT multiline layout with attachment point
        var mtextDoc = new CadExtractedDocument(
            "DXF",
            "AC1015",
            new[] { new CadExtractedLayer("MTEXT_LAYER", 0xFFFFFFFF, 2, true) },
            new[]
            {
                new CadExtractedEntity(
                    "MT01",
                    "MTEXT_LAYER",
                    CadExtractedEntityType.MText,
                    new CadEntityColor(CadColorMethod.Index, 2, 0),
                    points: new[] { new CadExtractedPoint(50, 50) },
                    text: "Birinci Satir\nIkinci Satir\nUcuncu Satir",
                    textHeight: 10.0,
                    rotation: 0.0,
                    payload: new CadTextPayload(
                        "Birinci Satir\nIkinci Satir\nUcuncu Satir",
                        new CadPoint3D(50, 50, 0),
                        10.0,
                        0.0,
                        AttachmentPoint: (int)CadTextAttachmentPoint.MiddleCenter,
                        Lines: new[] { "Birinci Satir", "Ikinci Satir", "Ucuncu Satir" }))
            },
            0, 0, 100, 100);

        var mtextScene = CadExtractedSceneBuilder.Build(mtextDoc);
        var mtextPrim = (TextPrimitive)mtextScene.Entities[0].Geometry[0];
        Assert(mtextPrim.HorizontalAlignment == CadTextHorizontalAlignment.Center, "AttachmentPoint MiddleCenter should map to Center horizontal");
        Assert(mtextPrim.VerticalAlignment == CadTextVerticalAlignment.Middle, "AttachmentPoint MiddleCenter should map to Middle vertical");
        Assert(mtextPrim.Layout.Lines.Count == 3, $"Expected 3 lines in layout, got {mtextPrim.Layout.Lines.Count}");
        Assert(mtextPrim.Bounds.Width > 0, "Bounds width must be positive");
        Assert(mtextPrim.Bounds.Height > 0, "Bounds height must be positive");

        // 3. Dimension with Exploded Anonymous Block vs Procedural Fallback
        // 3a. Exploded block entities: no double-drawing
        var explodedDimDoc = new CadExtractedDocument(
            "DXF",
            "AC1015",
            new[] { new CadExtractedLayer("DIM_LAYER", 0xFFFFFFFF, 3, true) },
            new[]
            {
                new CadExtractedEntity(
                    "DIM01",
                    "DIM_LAYER",
                    CadExtractedEntityType.Dimension,
                    new CadEntityColor(CadColorMethod.Index, 3, 0),
                    points: new[] { new CadExtractedPoint(0, 0), new CadExtractedPoint(100, 0) },
                    payload: new CadDimensionPayload(
                        "100.00",
                        new CadPoint3D(0, 0, 0),
                        new CadPoint3D(50, 10, 0),
                        DimensionType: "Aligned",
                        ExplodedEntities: new[]
                        {
                            new CadExtractedEntity("D_LINE1", "DIM_LAYER", CadExtractedEntityType.Line, new CadEntityColor(CadColorMethod.ByLayer, 0, 0), points: new[] { new CadExtractedPoint(0, 10), new CadExtractedPoint(100, 10) }),
                            new CadExtractedEntity("D_TEXT1", "DIM_LAYER", CadExtractedEntityType.Text, new CadEntityColor(CadColorMethod.ByLayer, 0, 0), points: new[] { new CadExtractedPoint(50, 15) }, text: "100.00")
                        }))
            },
            0, 0, 100, 100);

        var explodedScene = CadExtractedSceneBuilder.Build(explodedDimDoc);
        Assert(explodedScene.Entities.Count == 1, "Expected 1 dimension entity");
        Assert(explodedScene.Entities[0].Geometry.Count == 2, $"Expected exactly 2 exploded primitives (no duplicate procedural geometry), got {explodedScene.Entities[0].Geometry.Count}");
        Assert(explodedScene.Entities[0].Geometry[0] is LinePrimitive, "Expected LinePrimitive in exploded dim");
        Assert(explodedScene.Entities[0].Geometry[1] is TextPrimitive, "Expected TextPrimitive in exploded dim");

        // 3b. Procedural fallback with text override
        var proceduralDimDoc = new CadExtractedDocument(
            "DXF",
            "AC1015",
            new[] { new CadExtractedLayer("DIM_LAYER", 0xFFFFFFFF, 3, true) },
            new[]
            {
                new CadExtractedEntity(
                    "DIM02",
                    "DIM_LAYER",
                    CadExtractedEntityType.Dimension,
                    new CadEntityColor(CadColorMethod.Index, 3, 0),
                    points: new[] { new CadExtractedPoint(0, 0), new CadExtractedPoint(50, 0) },
                    text: "ÖZEL ÖLÇÜ: 50 mm",
                    payload: new CadDimensionPayload(
                        "ÖZEL ÖLÇÜ: 50 mm",
                        new CadPoint3D(0, 0, 0),
                        new CadPoint3D(25, 10, 0),
                        DimensionType: "Aligned",
                        Point1: new CadPoint3D(0, 0, 0),
                        Point2: new CadPoint3D(50, 0, 0),
                        DimLinePoint: new CadPoint3D(25, 10, 0),
                        TextHeight: 3.0,
                        ArrowheadSize: 2.5))
            },
            0, 0, 100, 100);

        var proceduralScene = CadExtractedSceneBuilder.Build(proceduralDimDoc);
        var dimGeom = proceduralScene.Entities[0].Geometry;
        Assert(dimGeom.Any(g => g is TextPrimitive tp && tp.Text == "ÖZEL ÖLÇÜ: 50 mm"), "Dimension text override must be preserved");

        // 3c. Leader
        var leaderDoc = new CadExtractedDocument(
            "DXF",
            "AC1015",
            new[] { new CadExtractedLayer("LEADER_LAYER", 0xFFFFFFFF, 4, true) },
            new[]
            {
                new CadExtractedEntity(
                    "LDR01",
                    "LEADER_LAYER",
                    CadExtractedEntityType.Dimension,
                    new CadEntityColor(CadColorMethod.Index, 4, 0),
                    points: new[] { new CadExtractedPoint(0, 0), new CadExtractedPoint(10, 10) },
                    text: "NOT: DETAY A",
                    payload: new CadDimensionPayload(
                        "NOT: DETAY A",
                        new CadPoint3D(0, 0, 0),
                        new CadPoint3D(10, 10, 0),
                        DimensionType: "Leader",
                        TextHeight: 3.0,
                        ArrowheadSize: 2.0))
            },
            0, 0, 100, 100);

        var leaderScene = CadExtractedSceneBuilder.Build(leaderDoc);
        Assert(leaderScene.Entities[0].Geometry.Any(g => g is TextPrimitive tp && tp.Text == "NOT: DETAY A"), "Leader text must be preserved");

        // 4. Hatch loops, islands and pattern line stability
        var outerLoop = new[]
        {
            new CadExtractedVertex(0, 0),
            new CadExtractedVertex(100, 0),
            new CadExtractedVertex(100, 100),
            new CadExtractedVertex(0, 100),
            new CadExtractedVertex(0, 0)
        };
        var islandLoop = new[]
        {
            new CadExtractedVertex(25, 25),
            new CadExtractedVertex(75, 25),
            new CadExtractedVertex(75, 75),
            new CadExtractedVertex(25, 75),
            new CadExtractedVertex(25, 25)
        };

        var hatchDoc = new CadExtractedDocument(
            "DXF",
            "AC1015",
            new[] { new CadExtractedLayer("HATCH_LAYER", 0xFFFFFFFF, 5, true) },
            new[]
            {
                new CadExtractedEntity(
                    "HATCH01",
                    "HATCH_LAYER",
                    CadExtractedEntityType.Hatch,
                    new CadEntityColor(CadColorMethod.Index, 5, 0),
                    vertices: outerLoop.Concat(islandLoop).ToArray(),
                    payload: new CadHatchPayload(
                        "ANSI31",
                        IsSolid: false,
                        Angle: Math.PI / 4.0,
                        Scale: 1.0,
                        Loops: new[] { outerLoop, islandLoop },
                        Origin: new CadPoint3D(0, 0, 0)))
            },
            0, 0, 100, 100);

        var hatchScene = CadExtractedSceneBuilder.Build(hatchDoc);
        Assert(hatchScene.Entities.Count == 1, "Expected 1 hatch entity");
        Assert(hatchScene.Entities[0].Geometry[0] is HatchPrimitive, "Expected HatchPrimitive");
        var hatchPrim = (HatchPrimitive)hatchScene.Entities[0].Geometry[0];
        Assert(hatchPrim.Loops.Count == 2, $"Expected 2 loops (outer + island), got {hatchPrim.Loops.Count}");
        Assert(hatchPrim.PatternLines.Count > 0, "Pattern lines should be generated for non-solid hatch");

        // Check pattern phase stability: identical origin produces identical lines
        var hatchScene2 = CadExtractedSceneBuilder.Build(hatchDoc);
        var hatchPrim2 = (HatchPrimitive)hatchScene2.Entities[0].Geometry[0];
        Assert(hatchPrim.PatternLines.Count == hatchPrim2.PatternLines.Count, "Pattern line count must be deterministic");
        for (var i = 0; i < hatchPrim.PatternLines.Count; i++)
        {
            Assert(Math.Abs(hatchPrim.PatternLines[i].Start.X - hatchPrim2.PatternLines[i].Start.X) < 1e-9, "Pattern line phase must be invariant");
            Assert(Math.Abs(hatchPrim.PatternLines[i].Start.Y - hatchPrim2.PatternLines[i].Start.Y) < 1e-9, "Pattern line phase must be invariant");
        }
    }
}

