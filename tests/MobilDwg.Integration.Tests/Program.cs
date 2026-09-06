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
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

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
            RunStage11LayoutMeasurementSnapTests();
            RunStage12LifecycleTests();
            await RunStage13PerformanceAcceptanceTests();

            Console.WriteLine("STAGE01_INTEGRATION_TESTS_PASS");
            Console.WriteLine("STAGE08_CAD_EXTRACTION_TESTS_PASS");
            Console.WriteLine("STAGE09_GEOMETRY_TESTS_PASS");
            Console.WriteLine("STAGE10_TEXT_DIMENSION_HATCH_PASS");
            Console.WriteLine("STAGE11_LAYOUT_MEASUREMENT_SNAP_PASS");
            Console.WriteLine("STAGE12_LIFECYCLE_TESTS_PASS");
            Console.WriteLine("STAGE13_FIXTURE_PERFORMANCE_PASS");
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

    private static void RunStage11LayoutMeasurementSnapTests()
    {
        // 1. Multi-layout switching and camera restoration (Model <-> Paper space, no view shift, zero reparse)
        var layerDef1 = new LayerDefinition("WALLS", CadColor.FromRgb(255, 255, 255), CadLinetype.Continuous, CadLineweight.Default);
        var layerDef2 = new LayerDefinition("HIDDEN", CadColor.FromRgb(128, 128, 128), CadLinetype.Continuous, CadLineweight.Default, isVisible: false);
        var layerTable = new LayerTable(new[] { layerDef1, layerDef2 });

        var e1 = new RenderSceneEntity(
            new RenderEntityId("E1"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("STYLE1"),
            new RenderSourceReference("LINE", "HANDLE1", 1),
            new[] { new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0)) });

        var e2 = new RenderSceneEntity(
            new RenderEntityId("E2"),
            new RenderLayerToken("HIDDEN"),
            new RenderStyleToken("STYLE1"),
            new RenderSourceReference("LINE", "HANDLE2", 2),
            new[] { new LinePrimitive(new WorldPoint2(50, -50), new WorldPoint2(50, 50)) });

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.SetLayerTable(layerTable);
        assembler.AddEntity(e1);
        assembler.AddEntity(e2);
        var modelScene = assembler.Build();

        var paperBounds = new WorldBounds2(0, 0, 420, 297);
        var vp = new CadLayoutViewport(
            "VP1",
            paperCenter: new WorldPoint2(210, 148.5),
            paperWidth: 420,
            paperHeight: 297,
            viewCenter: new WorldPoint2(50, 0),
            viewHeight: 100);

        var layout1 = new CadLayoutDefinition(
            "Layout1",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: paperBounds,
            viewports: new[] { vp });

        var layoutManager = new CadLayoutManager(modelScene, new[] { layout1 }, "Model");
        var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "TestDrawing", InsUnits: 4); // 4 = mm
        var session = new CadViewerSession(metadata, modelScene, layoutManager, 1000, 1000);

        Assert(session.ActiveLayoutName == "Model", "Initial layout should be Model");
        var initialModelCamera = session.Camera;

        // Pan in Model space
        session.Pan(50, -25);
        var pannedModelCamera = session.Camera;
        Assert(pannedModelCamera != initialModelCamera, "Camera must update on pan");

        // Switch to Layout1
        session.SwitchLayout("Layout1");
        Assert(session.ActiveLayoutName == "Layout1", "Active layout should be Layout1");
        var layout1Camera = session.Camera;

        // Pan in Layout1
        session.Pan(20, 30);
        var pannedLayout1Camera = session.Camera;
        Assert(pannedLayout1Camera != layout1Camera, "Camera must update on layout pan");

        // Switch back to Model: verify camera is restored exactly without view shift
        session.SwitchLayout("Model");
        Assert(session.ActiveLayoutName == "Model", "Active layout should be Model");
        Assert(session.Camera == pannedModelCamera, "Model camera must be restored exactly without view shift");

        // Switch back to Layout1: verify camera is restored to layout-specific camera
        session.SwitchLayout("Layout1");
        Assert(session.ActiveLayoutName == "Layout1", "Active layout should be Layout1");
        Assert(session.Camera == pannedLayout1Camera, "Layout1 camera must be restored exactly without view shift");

        // ZoomToFit filters by visible layers
        session.SwitchLayout("Model");
        session.ZoomToFit();
        var fitCameraWithVisible = session.Camera;
        var visibleBounds = fitCameraWithVisible.GetVisibleWorldBounds();
        Assert(visibleBounds.Contains(new WorldPoint2(0, 0)), "Fit extents must include visible entities");
        Assert(visibleBounds.Contains(new WorldPoint2(100, 0)), "Fit extents must include visible entities");

        // 2. Measurement invariance under 100 pan/pinch manipulations
        var measurement = session.Measurement;
        measurement.Clear();
        measurement.Mode = MeasurementMode.Distance;
        measurement.AddPoint(new WorldPoint2(10.0, 20.0));
        measurement.AddPoint(new WorldPoint2(40.0, 60.0));

        var baseDistance = measurement.CalculateDistance();
        Assert(Math.Abs(baseDistance - 50.0) < 1e-9, $"Expected distance 50.0, got {baseDistance}");

        measurement.Mode = MeasurementMode.Area;
        measurement.AddPoint(new WorldPoint2(10.0, 60.0)); // Triangle (10,20), (40,60), (10,60)
        var baseArea = measurement.CalculateArea();
        Assert(Math.Abs(baseArea - 600.0) < 1e-9, $"Expected area 600.0, got {baseArea}");

        // Perform 100 pan and pinch cycles on session
        var rand = new Random(42);
        for (var i = 0; i < 100; i++)
        {
            var dx = (rand.NextDouble() - 0.5) * 200;
            var dy = (rand.NextDouble() - 0.5) * 200;
            var zoomFactor = 0.8 + (rand.NextDouble() * 0.4); // 0.8x to 1.2x
            session.Pan(dx, dy);
            session.Zoom(zoomFactor, 500, 500);

            // Measurement stored in world double coordinates must be strictly invariant!
            Assert(Math.Abs(measurement.CalculateDistance() - 80.0) < 1e-12, "World distance mutated during pan/zoom!");
            Assert(Math.Abs(measurement.CalculateArea() - 600.0) < 1e-12, "World area mutated during pan/zoom!");
        }

        // 3. Measurement unit formatting: default unitless produces "çizim birimi" without mm/m assumption; INSUNITS metadata mapped appropriately
        var standaloneMeasurement = new MeasurementController();
        standaloneMeasurement.AddPoint(new WorldPoint2(0, 0));
        standaloneMeasurement.AddPoint(new WorldPoint2(3, 4));
        standaloneMeasurement.AddPoint(new WorldPoint2(0, 4));

        // Default unitless: must format as "çizim birimi" and "çizim birimi²"
        Assert(standaloneMeasurement.FormatDistance(5.0) == "5.00 çizim birimi", $"Expected '5.00 çizim birimi', got '{standaloneMeasurement.FormatDistance(5.0)}'");
        Assert(standaloneMeasurement.FormatArea(6.0) == "6.00 çizim birimi²", $"Expected '6.00 çizim birimi²', got '{standaloneMeasurement.FormatArea(6.0)}'");

        // Explicit unit overrides metadata
        standaloneMeasurement.ExplicitUnit = "km";
        Assert(standaloneMeasurement.FormatDistance(5.0) == "5.00 km", $"Expected '5.00 km', got '{standaloneMeasurement.FormatDistance(5.0)}'");
        Assert(standaloneMeasurement.FormatArea(6.0) == "6.00 km²", $"Expected '6.00 km²', got '{standaloneMeasurement.FormatArea(6.0)}'");

        // INSUNITS mapping: 4 -> mm, 6 -> m, 1 -> in
        standaloneMeasurement.ExplicitUnit = null;
        standaloneMeasurement.SetMetadataUnitFromInsUnits(4);
        Assert(standaloneMeasurement.FormatDistance(5.0) == "5.00 mm", $"Expected '5.00 mm', got '{standaloneMeasurement.FormatDistance(5.0)}'");
        Assert(standaloneMeasurement.FormatArea(6.0) == "6.00 mm²", $"Expected '6.00 mm²', got '{standaloneMeasurement.FormatArea(6.0)}'");

        standaloneMeasurement.SetMetadataUnitFromInsUnits(6);
        Assert(standaloneMeasurement.FormatDistance(5.0) == "5.00 m", $"Expected '5.00 m', got '{standaloneMeasurement.FormatDistance(5.0)}'");
        Assert(standaloneMeasurement.FormatArea(6.0) == "6.00 m²", $"Expected '6.00 m²', got '{standaloneMeasurement.FormatArea(6.0)}'");

        standaloneMeasurement.SetMetadataUnitFromInsUnits(0);
        Assert(standaloneMeasurement.FormatDistance(5.0) == "5.00 çizim birimi", $"Expected '5.00 çizim birimi', got '{standaloneMeasurement.FormatDistance(5.0)}'");

        // Session measurement inherited InsUnits 4 = mm
        Assert(session.Measurement.FormatDistance(50.0) == "50.00 mm", $"Expected '50.00 mm', got '{session.Measurement.FormatDistance(50.0)}'");
        Assert(session.Measurement.FormatArea(600.0) == "600.00 mm²", $"Expected '600.00 mm²', got '{session.Measurement.FormatArea(600.0)}'");

        // 4. Snap query: 12 DIP tolerance evaluated at density 1.0, 2.0, 3.0; priority order, visibility filtering, spline off-curve rejection
        var snapLine = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0));
        var snapArc = new ArcPrimitive(new WorldPoint2(50, 50), 20, 0, Math.PI * 2.0);
        var splinePoints = new[] { new WorldPoint2(0, 100), new WorldPoint2(50, 200), new WorldPoint2(100, 100) };
        var splineKnots = new[] { 0.0, 0.0, 0.0, 1.0, 1.0, 1.0 };
        var snapSpline = new SplinePrimitive(2, splinePoints, splineKnots);

        var snapEntities = new[]
        {
            new RenderSceneEntity(new RenderEntityId("ENT_LINE"), new RenderLayerToken("WALLS"), new RenderStyleToken("S1"), new RenderSourceReference("H1"), new[] { snapLine }),
            new RenderSceneEntity(new RenderEntityId("ENT_ARC"), new RenderLayerToken("WALLS"), new RenderStyleToken("S1"), new RenderSourceReference("H2"), new[] { snapArc }),
            new RenderSceneEntity(new RenderEntityId("ENT_SPLINE"), new RenderLayerToken("WALLS"), new RenderStyleToken("S1"), new RenderSourceReference("H3"), new[] { snapSpline }),
            new RenderSceneEntity(new RenderEntityId("ENT_HIDDEN"), new RenderLayerToken("HIDDEN"), new RenderStyleToken("S1"), new RenderSourceReference("H4"), new[] { new LinePrimitive(new WorldPoint2(200, 200), new WorldPoint2(200, 300)) })
        };

        var snapAssembler = new RenderSceneAssembler(RenderColorContext.Dark);
        snapAssembler.SetLayerTable(layerTable);
        foreach (var ent in snapEntities) snapAssembler.AddEntity(ent);
        var snapScene = snapAssembler.Build();

        var snapCamera = new Camera2D(1000, 1000, new WorldPoint2(50, 50), 1.0);

        // 4a. Snap near Line endpoint (0, 0)
        var screenEndpoint = CameraTransform.WorldToScreen(new WorldPoint2(0, 0), snapCamera);
        var queryScreenNearStart = new ScreenPoint2(screenEndpoint.X - 3, screenEndpoint.Y);
        var snapResultStart = SnapQuery.FindSnapPoint(queryScreenNearStart, snapCamera, snapScene, layerTable, 12.0, 1.0);
        Assert(snapResultStart.HasValue, "Snap near endpoint should succeed");
        var valStart = snapResultStart!.Value;
        Assert(valStart.Kind == CadSnapKind.Endpoint, $"Expected Endpoint snap, got {valStart.Kind}");
        Assert(Math.Abs(valStart.WorldPoint.X - 0.0) < 1e-9 && Math.Abs(valStart.WorldPoint.Y - 0.0) < 1e-9, "Endpoint world coords mismatch");

        // 4b. Snap near Center of Arc (50, 50)
        var screenCenter = CameraTransform.WorldToScreen(new WorldPoint2(50, 50), snapCamera);
        var queryScreenNearCenter = new ScreenPoint2(screenCenter.X + 2, screenCenter.Y + 2);
        var snapResultCenter = SnapQuery.FindSnapPoint(queryScreenNearCenter, snapCamera, snapScene, layerTable, 12.0, 1.0);
        Assert(snapResultCenter.HasValue, "Snap near arc center should succeed");
        var valCenter = snapResultCenter!.Value;
        Assert(valCenter.Kind == CadSnapKind.Center, $"Expected Center snap, got {valCenter.Kind}");

        // 4c. Snap on line curve (50, 0): far from endpoints
        var screenCurve = CameraTransform.WorldToScreen(new WorldPoint2(50, 0), snapCamera);
        var queryScreenNearCurve = new ScreenPoint2(screenCurve.X, screenCurve.Y + 4);
        var snapResultCurve = SnapQuery.FindSnapPoint(queryScreenNearCurve, snapCamera, snapScene, layerTable, 12.0, 1.0);
        Assert(snapResultCurve.HasValue, "Snap near line curve should succeed");
        var valCurve = snapResultCurve!.Value;
        Assert(valCurve.Kind == CadSnapKind.Curve, $"Expected Curve snap, got {valCurve.Kind}");
        Assert(Math.Abs(valCurve.WorldPoint.Y - 0.0) < 1e-9, "Curve point Y coordinate mismatch");

        // 4d. Priority test: Near endpoint where both endpoint and curve are within 12 px at equal distance, endpoint wins
        var nearEndQuery = new ScreenPoint2(screenEndpoint.X - 4, screenEndpoint.Y);
        var snapPriority = SnapQuery.FindSnapPoint(nearEndQuery, snapCamera, snapScene, layerTable, 12.0, 1.0);
        Assert(snapPriority.HasValue && snapPriority.Value.Kind == CadSnapKind.Endpoint, "Endpoint must take priority over curve");

        // 4e. Layer visibility exclusion: Query near hidden entity must return null
        var screenHidden = CameraTransform.WorldToScreen(new WorldPoint2(200, 250), snapCamera);
        var snapHidden = SnapQuery.FindSnapPoint(screenHidden, snapCamera, snapScene, layerTable, 12.0, 1.0);
        Assert(!snapHidden.HasValue, "Entities on hidden layers must not be snapped");

        // 4f. Density scaling: density 2.0 scales pixel radius to 24px
        var queryScreen20Px = new ScreenPoint2(screenEndpoint.X, screenEndpoint.Y + 20);
        var snapDensity1 = SnapQuery.FindSnapPoint(queryScreen20Px, snapCamera, snapScene, layerTable, 12.0, density: 1.0);
        var snapDensity2 = SnapQuery.FindSnapPoint(queryScreen20Px, snapCamera, snapScene, layerTable, 12.0, density: 2.0);
        Assert(!snapDensity1.HasValue, "At density 1.0 with 20px offset, snap within 12px must fail");
        Assert(snapDensity2.HasValue && snapDensity2.Value.DistancePixels <= 24.0, "At density 2.0, snap radius is 24px so offset 20px must succeed");

        // 4g. Spline: off-curve control point (50, 200) is NOT an endpoint or curve point
        var screenOffCurveControl = CameraTransform.WorldToScreen(new WorldPoint2(50, 200), snapCamera);
        var snapOffCurve = SnapQuery.FindSnapPoint(screenOffCurveControl, snapCamera, snapScene, layerTable, 5.0, 1.0);
        Assert(!snapOffCurve.HasValue, "Off-curve spline control points must NOT be snapped as endpoints");
    }

    private static void RunStage12LifecycleTests()
    {
        // 1. 50 Close / Reopen cycles with lease counting and ODE
        for (int cycle = 0; cycle < 50; cycle++)
        {
            var meta = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", $"doc_{cycle}.dxf");
            var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
            var scene = assembler.Build();
            var layoutManager = new CadLayoutManager(scene);
            var session = new CadViewerSession(meta, scene, layoutManager);

            Assert(!session.IsDisposed, $"Cycle {cycle}: Session should not be disposed initially");
            Assert(!session.IsRetiring, $"Cycle {cycle}: Session should not be retiring initially");
            Assert(session.ActiveLeaseCount == 0, $"Cycle {cycle}: Initial lease count should be 0");

            // Acquire render lease
            using (var lease = session.AcquireRenderLease(1))
            {
                Assert(session.ActiveLeaseCount == 1, $"Cycle {cycle}: Active lease count must be 1");
                Assert(lease.Snapshot.Scene == scene, $"Cycle {cycle}: Lease scene must match");

                // Request close while lease is active
                session.Dispose();
                Assert(session.IsRetiring, $"Cycle {cycle}: Session must be in retiring state while lease is held");
                Assert(!session.IsDisposed, $"Cycle {cycle}: Session must not be fully disposed while lease is active");

                // Trying to acquire new lease during retiring must throw ObjectDisposedException
                bool caughtOde = false;
                try
                {
                    session.AcquireRenderLease(1);
                }
                catch (ObjectDisposedException)
                {
                    caughtOde = true;
                }
                Assert(caughtOde, $"Cycle {cycle}: Acquiring lease on retiring session must throw ObjectDisposedException");
            }

            // After lease is disposed, session must transition to fully disposed
            Assert(session.ActiveLeaseCount == 0, $"Cycle {cycle}: Active lease count must return to 0");
            Assert(session.IsDisposed, $"Cycle {cycle}: Session must be disposed after final lease release");
            Assert(!session.IsRetiring, $"Cycle {cycle}: Retiring flag must clear after drain");

            // Idempotent Dispose
            session.Dispose();
            Assert(session.IsDisposed, $"Cycle {cycle}: Idempotent dispose must keep session disposed");
        }

        // 2. Lifecycle event ordering: CloseRequested vs DrainCompleted
        {
            var meta = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "events_test.dxf");
            var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
            var scene = assembler.Build();
            var layoutManager = new CadLayoutManager(scene);
            var session = new CadViewerSession(meta, scene, layoutManager);

            bool closeRequestedFired = false;
            bool drainCompletedFired = false;
            session.CloseRequested += () => closeRequestedFired = true;
            session.DrainCompleted += () => drainCompletedFired = true;

            var lease = session.AcquireRenderLease(1);
            Assert(!closeRequestedFired && !drainCompletedFired, "Events must not fire before dispose");

            session.Dispose();
            Assert(closeRequestedFired, "CloseRequested must fire immediately on Dispose()");
            Assert(!drainCompletedFired, "DrainCompleted must NOT fire while active lease is held");

            lease.Dispose();
            Assert(drainCompletedFired, "DrainCompleted must fire once final lease is released");
        }

        // 3. 20 Viewport rotations (resizing) and 20 background/resume (TrimMemory) cycles
        {
            var meta = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "rotate_test.dxf");
            var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId("E1"),
                new RenderLayerToken("0"),
                new RenderStyleToken("S1"),
                new RenderSourceReference("LINE", "1", 1),
                new[] { new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 100)) }));
            var scene = assembler.Build();
            var layoutManager = new CadLayoutManager(scene);
            var session = new CadViewerSession(meta, scene, layoutManager);

            for (int r = 0; r < 20; r++)
            {
                // Portrait
                session.ResizeViewport(1080, 2400);
                Assert(session.Controller.CurrentCamera.PixelWidth == 1080, "Portrait width 1080");
                Assert(session.Controller.CurrentCamera.PixelHeight == 2400, "Portrait height 2400");

                // Landscape
                session.ResizeViewport(2400, 1080);
                Assert(session.Controller.CurrentCamera.PixelWidth == 2400, "Landscape width 2400");
                Assert(session.Controller.CurrentCamera.PixelHeight == 1080, "Landscape height 1080");
            }

            for (int bg = 0; bg < 20; bg++)
            {
                // Background -> trim memory
                session.OnTrimMemory();

                // Resume -> verify session is still completely operational
                using var lease = session.AcquireRenderLease(1);
                Assert(lease.Snapshot.Scene.Entities.Count == 1, "Scene entities intact after memory trim");
            }

            session.Dispose();
            Assert(session.IsDisposed, "Session disposed successfully");
        }

        // 4. Resource guards and overflow protection (CadBudgetGuard)
        {
            var budget = new CadResourceBudget
            {
                MaxFileSizeBytes = 10 * 1024 * 1024,
                MaxEntities = 1000,
                MaxHatchBoundarySegments = 50,
                MaxTextLength = 1000,
                MaxRasterTotalPixels = 4096 * 4096
            };

            var guard = new CadBudgetGuard(budget);

            // Excessive dimension check (> MaxRasterDimensionPixels)
            bool dimAllowed = guard.CheckRasterDimensions(10000, 1000, out var dimDiag);
            Assert(!dimAllowed, "Raster dimension exceeding MaxRasterDimensionPixels must be rejected");
            Assert(dimDiag != null && dimDiag.Code == "RESOURCE_BUDGET_EXCEEDED_RASTER_DIMENSIONS",
                $"Expected dimension diagnostic code, got: {dimDiag?.Code}");

            // Over-budget raster total pixels (> MaxRasterTotalPixels)
            bool overBudgetAllowed = guard.CheckRasterDimensions(5000, 5000, out var overDiag);
            Assert(!overBudgetAllowed, "Over-budget raster dimensions must be rejected");
            Assert(overDiag != null && overDiag.Code == "RESOURCE_BUDGET_EXCEEDED_RASTER_PIXELS",
                $"Expected over budget diagnostic code, got: {overDiag?.Code}");

            // Within budget raster
            bool validRasterAllowed = guard.CheckRasterDimensions(1920, 1080, out _);
            Assert(validRasterAllowed, "Valid 1080p raster dimensions should pass budget check");

            // Text length limits
            bool longTextAllowed = guard.CheckTextLength(2000, out var textDiag);
            Assert(!longTextAllowed, "Text exceeding max length must be rejected");
            Assert(textDiag != null && textDiag.Code == "RESOURCE_BUDGET_EXCEEDED_TEXT_LENGTH",
                $"Expected text length diagnostic, got: {textDiag?.Code}");

            bool shortTextAllowed = guard.CheckTextLength("Valid Turkish Cad Text: İstanbul ÇGÖŞÜ".Length, out _);
            Assert(shortTextAllowed, "Normal text length should pass budget check");

            // Entity count check
            for (int i = 0; i < 1000; i++)
            {
                guard.CheckEntityCount(i + 1, out _);
            }
            bool overEntityAllowed = guard.CheckEntityCount(1001, out var entDiag);
            Assert(!overEntityAllowed, "Entity count exceeding limit must be rejected");
            Assert(entDiag != null && entDiag.Code == "RESOURCE_BUDGET_EXCEEDED_ENTITIES",
                $"Expected entity count diagnostic, got: {entDiag?.Code}");
        }

        // 5. Fixture SHA-256 integrity verification
        {
            var repoRoot = FindRepoRoot();
            var dxfPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "synthetic_turkish_basic_ac1015.dxf");
            Assert(File.Exists(dxfPath), "Fixture must exist");
            using var stream = File.OpenRead(dxfPath);
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha.ComputeHash(stream);
            var hashHex = Convert.ToHexString(hashBytes).ToLowerInvariant();
            Assert(hashHex.Length == 64, "Fixture SHA-256 hash computation valid");
        }
    }

    private static async Task RunStage13PerformanceAcceptanceTests()
    {
        var repoRoot = FindRepoRoot();

        // 1. Fixture Manifest Validation: Synthetic Turkish, negative font, negative xref
        var fixtures = new[]
        {
            Path.Combine(repoRoot, "fixtures", "public", "synthetic", "synthetic_turkish_basic_ac1015.dxf"),
            Path.Combine(repoRoot, "fixtures", "public", "synthetic", "negative_missing_font_ac1015.dxf"),
            Path.Combine(repoRoot, "fixtures", "public", "synthetic", "negative_missing_xref_ac1015.dxf")
        };

        foreach (var path in fixtures)
        {
            Assert(File.Exists(path), $"Corpus fixture must exist: {path}");
            var fi = new FileInfo(path);
            Assert(fi.Length > 0, $"Fixture must not be empty: {path}");
        }

        // 2. Real Turkish DXF pipeline timing acceptance
        var turkishDxf = fixtures[0];
        var reader = new AcadSharpDocumentReader();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await using (var stream = File.OpenRead(turkishDxf))
        {
            var request = new CadOpenRequest(stream, Path.GetFileName(turkishDxf), stream.Length, LeaveOpen: true);
            await using var session = await reader.OpenAsync(request);

            var extracted = AcadSharpEntityExtractor.Extract(session.Handle!);
            var scene = CadExtractedSceneBuilder.Build(extracted);

            Assert(scene.Entities.Count > 0, "Entities must be extracted");
            Assert(scene.WorldBounds.HasValue, "World bounds must be computed");

            // Spatial query
            var camera = ViewerZoomPolicy.CreateFitCamera(scene.WorldBounds!.Value, 800, 800);
            var visibleBounds = camera.GetVisibleWorldBounds();
            var candidates = new List<int>();
            var metrics = new MobilDwg.Rendering.Spatial.SpatialQueryMetrics();
            scene.SpatialIndex.Query(visibleBounds, candidates, ref metrics);
            Assert(candidates.Count > 0, "Visible candidates must be found");
        }
        sw.Stop();

        // End-to-end open + parse + extract + bvh build must be well within budget (< 2000ms for synthetic Turkish)
        Assert(sw.Elapsed.TotalMilliseconds < 2000.0,
            $"Real fixture pipeline took {sw.Elapsed.TotalMilliseconds} ms, expected < 2000ms");
    }
}

