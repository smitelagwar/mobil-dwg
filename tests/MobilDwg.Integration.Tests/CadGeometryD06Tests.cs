using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Integration.Tests;

public static class CadGeometryD06Tests
{
    public static void RunAll()
    {
        Console.WriteLine("=== RUNNING D06 GEOMETRY AND BLOCK TESTS ===");
        TestThreeLevelNestedBlockWithRotationAndScale();
        TestMinsertGridExpansion();
        TestRotatedEllipseBoundsCalculation();
        TestSolidFilledPolygonConversion();
        Console.WriteLine("=== D06 GEOMETRY AND BLOCK TESTS PASSED ===");
    }

    private static void TestThreeLevelNestedBlockWithRotationAndScale()
    {
        // Construct an in-memory CAD document with 3 levels of nested blocks:
        // Level 3 ("L3"): contains LINE (0,0) -> (10,0). BasePoint = (2, 3).
        // Level 2 ("L2"): inserts L3 at (20, 10), rot = 90 deg (pi/2), scale = (2, 2). BasePoint = (0, 0).
        // Level 1 (ModelSpace): inserts L2 at (100, 200), rot = 0, scale = (1, 1). BasePoint = (0, 0).
        var doc = new CadDocument();

        var b3 = new BlockRecord("L3");
        b3.BlockEntity.BasePoint = new XYZ(2, 3, 0);
        var line = new Line
        {
            StartPoint = new XYZ(0, 0, 0),
            EndPoint = new XYZ(10, 0, 0)
        };
        b3.Entities.Add(line);
        doc.BlockRecords.Add(b3);

        var b2 = new BlockRecord("L2");
        b2.BlockEntity.BasePoint = new XYZ(0, 0, 0);
        var ins3 = new Insert(b3)
        {
            InsertPoint = new XYZ(20, 10, 0),
            Rotation = Math.PI / 2.0,
            XScale = 2.0,
            YScale = 2.0,
            ZScale = 2.0
        };
        b2.Entities.Add(ins3);
        doc.BlockRecords.Add(b2);

        var ins2 = new Insert(b2)
        {
            InsertPoint = new XYZ(100, 200, 0),
            Rotation = 0.0,
            XScale = 1.0,
            YScale = 1.0,
            ZScale = 1.0
        };
        doc.Entities.Add(ins2);

        var extracted = AcadSharpEntityExtractor.Extract(new AcadSharpDocumentHandle(doc, TimeSpan.Zero, CadFormat.Dxf, "AC1015"));
        var extractedLine = extracted.Entities.Single(e => e.EntityType == CadExtractedEntityType.Line);

        // Analytical transformation verification:
        // Point in L3: P_local = (0, 0)
        // 1. Subtract L3 basepoint (2, 3): (-2, -3)
        // 2. Scale by 2: (-4, -6)
        // 3. Rotate by +90 deg: rx = (-4)*0 - (-6)*1 = 6, ry = (-4)*1 + (-6)*0 = -4
        // 4. Translate by L3 insertion (20, 10): (26, 6) in L2 space
        // 5. Subtract L2 basepoint (0, 0): (26, 6)
        // 6. Scale by 1, rotate by 0, translate by (100, 200): (126, 206) in World space!
        double expectedStartX = 126.0;
        double expectedStartY = 206.0;

        // For end point: (10, 0)
        // 1. (-2+10, -3) = (8, -3)
        // 2. Scale by 2: (16, -6)
        // 3. Rotate 90: (6, 16)
        // 4. Translate by (20, 10): (26, 26)
        // 5. Translate by (100, 200): (126, 226)
        double expectedEndX = 126.0;
        double expectedEndY = 226.0;

        double actualStartX = extractedLine.Points![0].X;
        double actualStartY = extractedLine.Points![0].Y;
        double actualEndX = extractedLine.Points![1].X;
        double actualEndY = extractedLine.Points![1].Y;

        if (Math.Abs(actualStartX - expectedStartX) > 1e-6 || Math.Abs(actualStartY - expectedStartY) > 1e-6)
        {
            throw new InvalidOperationException($"3-level nested block start mismatch: expected ({expectedStartX}, {expectedStartY}), got ({actualStartX}, {actualStartY})");
        }

        if (Math.Abs(actualEndX - expectedEndX) > 1e-6 || Math.Abs(actualEndY - expectedEndY) > 1e-6)
        {
            throw new InvalidOperationException($"3-level nested block end mismatch: expected ({expectedEndX}, {expectedEndY}), got ({actualEndX}, {actualEndY})");
        }

        Console.WriteLine("  [PASS] TestThreeLevelNestedBlockWithRotationAndScale");
    }

    private static void TestMinsertGridExpansion()
    {
        // 2 rows, 3 columns MINSERT with rotation and spacing
        var doc = new CadDocument();
        var blk = new BlockRecord("BOX");
        blk.BlockEntity.BasePoint = new XYZ(0, 0, 0);
        blk.Entities.Add(new Line { StartPoint = new XYZ(0, 0, 0), EndPoint = new XYZ(1, 0, 0) });
        doc.BlockRecords.Add(blk);

        var mins = new Insert(blk)
        {
            InsertPoint = new XYZ(10, 20, 0),
            ColumnCount = 3,
            RowCount = 2,
            ColumnSpacing = 5,
            RowSpacing = 10,
            Rotation = 0
        };
        doc.Entities.Add(mins);

        var extracted = AcadSharpEntityExtractor.Extract(new AcadSharpDocumentHandle(doc, TimeSpan.Zero, CadFormat.Dxf, "AC1015"));
        var lines = extracted.Entities.Where(e => e.EntityType == CadExtractedEntityType.Line).ToList();

        if (lines.Count != 6)
        {
            throw new InvalidOperationException($"MINSERT 2x3 expected 6 entities, got {lines.Count}");
        }

        // Verify cell (col=2, row=1): grid offset is (2*5, 1*10) = (10, 10). Insert is (10, 20) -> (20, 30)
        bool foundTargetCell = lines.Any(l => Math.Abs(l.Points![0].X - 20.0) < 1e-6 && Math.Abs(l.Points![0].Y - 30.0) < 1e-6);
        if (!foundTargetCell)
        {
            throw new InvalidOperationException("MINSERT grid target cell (col=2, row=1) at (20, 30) not found.");
        }

        Console.WriteLine("  [PASS] TestMinsertGridExpansion");
    }

    private static void TestRotatedEllipseBoundsCalculation()
    {
        // Ellipse at (50, 50), Major axis (20, 0) rotated by 45 degrees -> (14.142, 14.142), ratio = 0.5
        double rot = Math.PI / 4.0;
        double majR = 20.0;
        double minR = 10.0;
        var center = new WorldPoint2(50, 50);

        var ellipse = new EllipsePrimitive(center, majR, minR, rot);
        var bounds = ellipse.Bounds;

        // Analytical extremum of rotated ellipse:
        // x(t) = cx + a cos(t) cos(rot) - b sin(t) sin(rot)
        // dx/dt = -a sin(t) cos(rot) - b cos(t) sin(rot) = 0 => tan(t) = - (b/a) tan(rot)
        // Semi-axis bounds: dx_max = sqrt(a^2 cos^2(rot) + b^2 sin^2(rot))
        double expectedHalfWidth = Math.Sqrt((majR * majR * Math.Cos(rot) * Math.Cos(rot)) + (minR * minR * Math.Sin(rot) * Math.Sin(rot)));
        double expectedHalfHeight = Math.Sqrt((majR * majR * Math.Sin(rot) * Math.Sin(rot)) + (minR * minR * Math.Cos(rot) * Math.Cos(rot)));

        double expectedWidth = expectedHalfWidth * 2.0;
        double expectedHeight = expectedHalfHeight * 2.0;

        if (Math.Abs(bounds.Width - expectedWidth) > 1e-5 || Math.Abs(bounds.Height - expectedHeight) > 1e-5)
        {
            throw new InvalidOperationException($"Rotated ellipse bounds mismatch: expected {expectedWidth:F3}x{expectedHeight:F3}, got {bounds.Width:F3}x{bounds.Height:F3}");
        }

        Console.WriteLine("  [PASS] TestRotatedEllipseBoundsCalculation");
    }

    private static void TestSolidFilledPolygonConversion()
    {
        var doc = new CadDocument();
        var solid = new Solid
        {
            FirstCorner = new XYZ(0, 0, 0),
            SecondCorner = new XYZ(10, 0, 0),
            ThirdCorner = new XYZ(0, 10, 0),
            FourthCorner = new XYZ(10, 10, 0)
        };
        doc.Entities.Add(solid);

        var extracted = AcadSharpEntityExtractor.Extract(new AcadSharpDocumentHandle(doc, TimeSpan.Zero, CadFormat.Dxf, "AC1015"));
        var scene = CadExtractedSceneBuilder.Build(extracted);

        var entity = scene.Entities.Single();
        if (entity.Geometry[0] is not PolygonPrimitive poly)
        {
            throw new InvalidOperationException($"SOLID must be converted to PolygonPrimitive, got {entity.Geometry[0].GetType().Name}");
        }

        if (poly.Vertices.Count != 4)
        {
            throw new InvalidOperationException($"SOLID PolygonPrimitive must have 4 vertices, got {poly.Vertices.Count}");
        }

        Console.WriteLine("  [PASS] TestSolidFilledPolygonConversion");
    }
}
