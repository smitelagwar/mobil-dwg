using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Guards;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
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

            Console.WriteLine("STAGE01_INTEGRATION_TESTS_PASS");
            Console.WriteLine("STAGE08_CAD_EXTRACTION_TESTS_PASS");
            Console.WriteLine("STAGE09_GEOMETRY_TESTS_PASS");
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
}
