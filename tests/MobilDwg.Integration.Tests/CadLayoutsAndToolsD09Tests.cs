using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Integration.Tests;

public static class CadLayoutsAndToolsD09Tests
{
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"[D09 TEST ASSERTION FAILED] {message}");
        }
    }

    public static void RunAll(string repoRoot)
    {
        Console.WriteLine("=== RUNNING D09 LAYOUTS, REFERENCES AND TOOLS TESTS ===");
        TestMultiPaperLayoutSwitching();
        TestCurvedSnapCalculations();
        TestInsUnitsAndMeasurementFormatting();
        TestReferencePlaceholders();
        TestThemeRetentionWithoutSessionDisposal();
        Console.WriteLine("=== ALL D09 TESTS PASSED SUCCESSFULLY ===");
    }

    private static void TestMultiPaperLayoutSwitching()
    {
        Console.WriteLine("-> Running TestMultiPaperLayoutSwitching...");

        // 1. Model space entities: line on layer "WALLS", line on layer "HIDDEN_IN_VP"
        var modelLayerWalls = new CadExtractedLayer("WALLS", 0xFFFFFFFF, 7, IsVisible: true);
        var modelLayerHidden = new CadExtractedLayer("HIDDEN_IN_VP", 0xFFFF0000, 1, IsVisible: true);
        var layers = new[] { modelLayerWalls, modelLayerHidden };

        var modelEnt1 = new CadExtractedEntity(
            handle: "M1",
            layerName: "WALLS",
            entityType: CadExtractedEntityType.Line,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(0, 0), new CadExtractedPoint(100, 0) });

        var modelEnt2 = new CadExtractedEntity(
            handle: "M2",
            layerName: "HIDDEN_IN_VP",
            entityType: CadExtractedEntityType.Line,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(0, 50), new CadExtractedPoint(100, 50) });

        var modelEntities = new[] { modelEnt1, modelEnt2 };

        // 2. Paper Layout 1: Sheet frame line, 1 standard viewport (no frozen layers)
        var p1SheetLine = new CadExtractedEntity(
            handle: "P1_FRAME",
            layerName: "0",
            entityType: CadExtractedEntityType.Line,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(0, 0), new CadExtractedPoint(297, 210) },
            layoutOwner: "Layout1");

        var vp1 = new CadExtractedViewport(
            Id: "VP1",
            PaperCenter: new CadExtractedPoint(148.5, 105),
            PaperWidth: 200,
            PaperHeight: 150,
            ViewCenter: new CadExtractedPoint(50, 25),
            ViewHeight: 50,
            TwistAngleRadians: 0.0,
            FrozenLayers: null,
            ClipBoundary: null,
            IsActive: true);

        var layout1 = new CadExtractedLayout(
            Name: "Layout1",
            IsModelSpace: false,
            TabOrder: 1,
            PaperBounds: new CadExtractedBounds(0, 0, 297, 210),
            Entities: new[] { p1SheetLine },
            Viewports: new[] { vp1 });

        // 3. Paper Layout 2: Sheet title text, 1 viewport with twist angle, frozen layer "HIDDEN_IN_VP", polygon clip boundary
        var p2Title = new CadExtractedEntity(
            handle: "P2_TITLE",
            layerName: "0",
            entityType: CadExtractedEntityType.Text,
            color: CadEntityColor.ByLayer,
            text: "PROJE DETAY PLANI",
            points: new[] { new CadExtractedPoint(10, 10) },
            layoutOwner: "Layout2");

        var clipPoly = new[]
        {
            new CadExtractedPoint(50, 50),
            new CadExtractedPoint(250, 50),
            new CadExtractedPoint(250, 180),
            new CadExtractedPoint(50, 180)
        };

        var vp2 = new CadExtractedViewport(
            Id: "VP2",
            PaperCenter: new CadExtractedPoint(150, 115),
            PaperWidth: 200,
            PaperHeight: 130,
            ViewCenter: new CadExtractedPoint(50, 25),
            ViewHeight: 40,
            TwistAngleRadians: Math.PI / 4.0, // 45 degrees twist
            FrozenLayers: new[] { "HIDDEN_IN_VP" },
            ClipBoundary: clipPoly,
            IsActive: true);

        var layout2 = new CadExtractedLayout(
            Name: "Layout2",
            IsModelSpace: false,
            TabOrder: 2,
            PaperBounds: new CadExtractedBounds(0, 0, 420, 297),
            Entities: new[] { p2Title },
            Viewports: new[] { vp2 });

        var extractedDoc = new CadExtractedDocument(
            format: "DWG",
            version: "AC1032",
            layers: layers,
            entities: modelEntities,
            minX: 0, minY: 0, maxX: 100, maxY: 50,
            layouts: new[] { layout1, layout2 });

        // Build scenes and layout manager
        var modelScene = CadExtractedSceneBuilder.Build(extractedDoc, RenderColorContext.Dark);
        var layoutDefs = CadExtractedSceneBuilder.BuildLayoutDefinitions(extractedDoc, RenderColorContext.Dark);

        Assert(layoutDefs.Count == 2, $"Expected 2 paper layout definitions, got {layoutDefs.Count}");
        var manager = new CadLayoutManager(modelScene, layoutDefs, "Model");

        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "MultiLayoutTest.dwg");
        using var session = new CadViewerSession(metadata, modelScene, manager, 1000, 800);

        // Verify initial Model Space state
        Assert(session.ActiveLayoutName == "Model", $"Initial layout should be Model, got {session.ActiveLayoutName}");
        Assert(session.Scene.Entities.Count == 2, $"Model space should have 2 entities, got {session.Scene.Entities.Count}");

        // Pan in Model Space
        session.Pan(50, 30);
        var modelCameraAfterPan = session.Camera;

        // Switch to Layout1
        session.SwitchLayout("Layout1");
        Assert(session.ActiveLayoutName == "Layout1", $"Active layout should be Layout1, got {session.ActiveLayoutName}");
        Assert(session.Scene.Entities.Count >= 2, $"Layout1 scene should contain paper line + viewport primitive, got {session.Scene.Entities.Count}");

        // Verify Viewport in Layout1 contains the model space entities
        var vpEntity = session.Scene.Entities.FirstOrDefault(e => e.Geometry.Any(g => g is ViewportPrimitive));
        Assert(vpEntity != null, "Layout1 should compose a ViewportPrimitive");
        var vpPrim = (ViewportPrimitive)vpEntity!.Geometry.First(g => g is ViewportPrimitive);
        Assert(vpPrim.InnerPrimitives.Count == 2, $"Layout1 viewport should display both model entities, got {vpPrim.InnerPrimitives.Count}");

        // Pan in Layout1 to establish camera position
        session.Pan(-100, 40);
        session.ZoomIn(1.5);
        var layout1CameraSaved = session.Camera;

        // Switch to Layout2
        session.SwitchLayout("Layout2");
        Assert(session.ActiveLayoutName == "Layout2", $"Active layout should be Layout2, got {session.ActiveLayoutName}");
        var vp2Entity = session.Scene.Entities.FirstOrDefault(e => e.Geometry.Any(g => g is ViewportPrimitive));
        Assert(vp2Entity != null, "Layout2 should compose a ViewportPrimitive");
        var vp2Prim = (ViewportPrimitive)vp2Entity!.Geometry.First(g => g is ViewportPrimitive);

        // Verify VP2 frozen layer: HIDDEN_IN_VP must be filtered out!
        Assert(vp2Prim.InnerPrimitives.Count == 1, $"Layout2 viewport should filter out HIDDEN_IN_VP, expected 1 primitive, got {vp2Prim.InnerPrimitives.Count}");
        // Verify VP2 clipping boundary
        Assert(vp2Prim.ClipBoundary != null && vp2Prim.ClipBoundary.Count == 4, "Expected polygon clip boundary with 4 points");

        // Verify model line in VP2 was transformed by twist angle (45 degrees rotation)
        var innerLine = vp2Prim.InnerPrimitives.OfType<LinePrimitive>().FirstOrDefault();
        Assert(innerLine != null, "Viewport 2 should contain transformed LinePrimitive");
        Assert(Math.Abs(innerLine!.End.Y - innerLine.Start.Y) > 1.0, "Inner line should be rotated by twist angle");

        // Pan in Layout2
        session.Pan(20, -50);

        // Switch BACK to Layout1 -> Camera memory must be preserved!
        session.SwitchLayout("Layout1");
        Assert(session.ActiveLayoutName == "Layout1", "Layout should be Layout1");
        Assert(Math.Abs(session.Camera.Center.X - layout1CameraSaved.Center.X) < 1e-4, "Layout1 camera Center.X was not preserved across switches");
        Assert(Math.Abs(session.Camera.Center.Y - layout1CameraSaved.Center.Y) < 1e-4, "Layout1 camera Center.Y was not preserved across switches");
        Assert(Math.Abs(session.Camera.WorldUnitsPerPixel - layout1CameraSaved.WorldUnitsPerPixel) < 1e-4, "Layout1 zoom was not preserved across switches");

        // Switch BACK to Model -> Model camera must be preserved!
        session.SwitchLayout("Model");
        Assert(session.ActiveLayoutName == "Model", "Layout should be Model");
        Assert(Math.Abs(session.Camera.Center.X - modelCameraAfterPan.Center.X) < 1e-4, "Model camera Center.X was not preserved");
        Assert(Math.Abs(session.Camera.Center.Y - modelCameraAfterPan.Center.Y) < 1e-4, "Model camera Center.Y was not preserved");

        Console.WriteLine("-> TestMultiPaperLayoutSwitching PASSED.");
    }

    private static void TestCurvedSnapCalculations()
    {
        Console.WriteLine("-> Running TestCurvedSnapCalculations...");

        var layer0 = new CadExtractedLayer("0", 0xFFFFFFFF, 7, IsVisible: true);

        // 1. Polyline with Bulge Arc Snap
        // Point (0, 0) with bulge 1.0 to (100, 0).
        // Semicircle with radius 50, center (50, 0), apex at (50, 50).
        var polyVertices = new[]
        {
            new CadExtractedVertex(0, 0, Bulge: 1.0),
            new CadExtractedVertex(100, 0, Bulge: 0.0),
        };
        var polyEnt = new CadExtractedEntity(
            handle: "P1",
            layerName: "0",
            entityType: CadExtractedEntityType.Polyline,
            color: CadEntityColor.ByLayer,
            vertices: polyVertices);

        // 2. Spline with off-curve control points
        // Control points (0, 0), (50, 100), (100, 0).
        // The curve passes near (50, 50), while (50, 100) is an OFF-CURVE control point.
        var splineCtrlPts = new[]
        {
            new CadPoint3D(0, 0, 0),
            new CadPoint3D(50, 100, 0),
            new CadPoint3D(100, 0, 0)
        };
        var splineKnots = new[] { 0.0, 0.0, 0.0, 1.0, 1.0, 1.0 };
        var splinePayload = new CadSplinePayload(
            Degree: 2,
            IsClosed: false,
            ControlPoints: splineCtrlPts,
            FitPoints: Array.Empty<CadPoint3D>(),
            Knots: splineKnots,
            Weights: null);
        var splineVertices = splineCtrlPts.Select(p => new CadExtractedVertex(p.X, p.Y)).ToArray();
        var splineEnt = new CadExtractedEntity(
            handle: "S1",
            layerName: "0",
            entityType: CadExtractedEntityType.Spline,
            color: CadEntityColor.ByLayer,
            payload: splinePayload,
            vertices: splineVertices);

        // 3. Ellipse
        // Center (200, 200), major axis length 100 along X, radius ratio 0.5 -> minor radius 50.
        // Perimeter top point is (200, 250).
        var ellPayload = new CadEllipsePayload(
            Center: new CadPoint3D(200, 200, 0),
            MajorAxis: new CadPoint3D(100, 0, 0),
            RadiusRatio: 0.5,
            StartParameter: 0.0,
            EndParameter: 2.0 * Math.PI);
        var ellipseEnt = new CadExtractedEntity(
            handle: "E1",
            layerName: "0",
            entityType: CadExtractedEntityType.Ellipse,
            color: CadEntityColor.ByLayer,
            payload: ellPayload,
            points: new[] { new CadExtractedPoint(200, 200) },
            radius: 100.0,
            startAngle: 0.0,
            endAngle: 2.0 * Math.PI,
            rotation: 0.0);

        var doc = new CadExtractedDocument(
            format: "DWG",
            version: "AC1032",
            layers: new[] { layer0 },
            entities: new[] { polyEnt, splineEnt, ellipseEnt },
            minX: 0, minY: -60, maxX: 300, maxY: 260);

        var scene = CadExtractedSceneBuilder.Build(doc, RenderColorContext.Dark);

        // Camera: 1:1 world-to-pixel for simple deterministic distance math
        var camera = new Camera2D(
            1000,
            1000,
            new WorldPoint2(100, 100),
            1.0);

        // A. Polyline Bulge: query near the arc apex (50, -50)
        // Screen point corresponding to world (50, -51) -> 1 pixel away from true arc apex
        var apexScreen = CameraTransform.WorldToScreen(new WorldPoint2(50, -51), camera);
        var snapApex = SnapQuery.FindSnapPoint(apexScreen, camera, scene, scene.LayerTable, snapRadiusDip: 12.0, density: 1.0);
        Assert(snapApex.HasValue, "Should snap near arc apex (50, -50)");
        var apexVal = snapApex!.Value;
        Assert(apexVal.Kind == CadSnapKind.Curve, $"Expected Curve snap at arc apex, got {apexVal.Kind}");
        Assert(Math.Abs(apexVal.WorldPoint.Y - (-50.0)) < 2.0, $"Snap point Y should be near -50.0 on arc, got {apexVal.WorldPoint.Y}");

        // Query near the chord segment at (50, -2) -> chord is Y=0, arc apex is Y=-50, center is (50, 0)
        var chordScreen = CameraTransform.WorldToScreen(new WorldPoint2(50, -2), camera);
        var snapChord = SnapQuery.FindSnapPoint(chordScreen, camera, scene, scene.LayerTable, snapRadiusDip: 12.0, density: 1.0);
        // Center snap at (50, 0) should be found since center is at (50, 0) which is 2 units away!
        Assert(snapChord.HasValue, "Near (50, -2) should find arc Center snap at (50, 0)");
        var chordVal = snapChord!.Value;
        Assert(chordVal.Kind == CadSnapKind.Center, $"Expected Center snap, got {chordVal.Kind}");
        Assert(Math.Abs(chordVal.WorldPoint.X - 50.0) < 1e-4 && Math.Abs(chordVal.WorldPoint.Y - 0.0) < 1e-4, "Center snap must be at (50, 0)");

        // B. Spline: query near off-curve control point (50, 100)
        // Control point is at (50, 100), but curve passes around Y=50.
        // Snap query at (50, 100) should NOT snap because off-curve control points are rejected!
        var cpScreen = CameraTransform.WorldToScreen(new WorldPoint2(50, 100), camera);
        var snapCp = SnapQuery.FindSnapPoint(cpScreen, camera, scene, scene.LayerTable, snapRadiusDip: 12.0, density: 1.0);
        Assert(!snapCp.HasValue, "Must reject off-curve control points of splines");

        // Query near the actual spline curve at (50, 50)
        var curveScreen = CameraTransform.WorldToScreen(new WorldPoint2(50, 50), camera);
        var snapSplineCurve = SnapQuery.FindSnapPoint(curveScreen, camera, scene, scene.LayerTable, snapRadiusDip: 12.0, density: 1.0);
        Assert(snapSplineCurve.HasValue, "Should snap to evaluated spline curve");
        var splineVal = snapSplineCurve!.Value;
        Assert(splineVal.Kind == CadSnapKind.Curve, $"Expected Spline Curve snap, got {splineVal.Kind}");

        // C. Ellipse: query near center (200, 200)
        var ellCenterScreen = CameraTransform.WorldToScreen(new WorldPoint2(200, 201), camera);
        var snapEllCenter = SnapQuery.FindSnapPoint(ellCenterScreen, camera, scene, scene.LayerTable, snapRadiusDip: 12.0, density: 1.0);
        Assert(snapEllCenter.HasValue, "Should snap to ellipse center");
        var ellCenterVal = snapEllCenter!.Value;
        Assert(ellCenterVal.Kind == CadSnapKind.Center, $"Expected Ellipse Center snap, got {ellCenterVal.Kind}");
        Assert(Math.Abs(ellCenterVal.WorldPoint.X - 200.0) < 1e-4 && Math.Abs(ellCenterVal.WorldPoint.Y - 200.0) < 1e-4, "Ellipse center must be (200, 200)");

        // Query near ellipse perimeter at (200, 250)
        var ellPerimeterScreen = CameraTransform.WorldToScreen(new WorldPoint2(200, 251), camera);
        var snapEllPerimeter = SnapQuery.FindSnapPoint(ellPerimeterScreen, camera, scene, scene.LayerTable, snapRadiusDip: 12.0, density: 1.0);
        Assert(snapEllPerimeter.HasValue, "Should snap to ellipse curve");
        var ellPerimVal = snapEllPerimeter!.Value;
        Assert(ellPerimVal.Kind == CadSnapKind.Curve, $"Expected Ellipse Curve snap, got {ellPerimVal.Kind}");

        Console.WriteLine("-> TestCurvedSnapCalculations PASSED.");
    }

    private static void TestInsUnitsAndMeasurementFormatting()
    {
        Console.WriteLine("-> Running TestInsUnitsAndMeasurementFormatting...");

        var mc = new MeasurementController();

        // 1. INSUNITS = 0 (Unspecified / Unitless)
        mc.SetMetadataUnitFromInsUnits(0);
        var formattedDist0 = mc.FormatDistance(123.45);
        Assert(formattedDist0.Contains("çizim birimi"), $"Expected 'çizim birimi' for INSUNITS 0, got '{formattedDist0}'");
        var formattedArea0 = mc.FormatArea(500.0);
        Assert(formattedArea0.Contains("çizim birimi²"), $"Expected 'çizim birimi²' for area INSUNITS 0, got '{formattedArea0}'");

        // 2. INSUNITS = 4 (Millimeters)
        mc.SetMetadataUnitFromInsUnits(4);
        var formattedDist4 = mc.FormatDistance(25.4);
        Assert(formattedDist4.EndsWith("mm"), $"Expected 'mm' for INSUNITS 4, got '{formattedDist4}'");
        var formattedArea4 = mc.FormatArea(100.0);
        Assert(formattedArea4.EndsWith("mm²"), $"Expected 'mm²' for area INSUNITS 4, got '{formattedArea4}'");

        // 3. INSUNITS = 6 (Meters)
        mc.SetMetadataUnitFromInsUnits(6);
        var formattedDist6 = mc.FormatDistance(15.75);
        Assert(formattedDist6.EndsWith("m"), $"Expected 'm' for INSUNITS 6, got '{formattedDist6}'");

        // 4. INSUNITS = 1 (Inches)
        mc.SetMetadataUnitFromInsUnits(1);
        var formattedDist1 = mc.FormatDistance(10.0);
        Assert(formattedDist1.EndsWith("in"), $"Expected 'in' for INSUNITS 1, got '{formattedDist1}'");

        // 5. INSUNITS = 5 (Centimeters)
        mc.SetMetadataUnitFromInsUnits(5);
        var formattedDist5 = mc.FormatDistance(100.0);
        Assert(formattedDist5.EndsWith("cm"), $"Expected 'cm' for INSUNITS 5, got '{formattedDist5}'");

        // 6. INSUNITS = 14 (Decimeters)
        mc.SetMetadataUnitFromInsUnits(14);
        var formattedDist14 = mc.FormatDistance(5.0);
        Assert(formattedDist14.EndsWith("dm"), $"Expected 'dm' for INSUNITS 14, got '{formattedDist14}'");

        // 7. Explicit Unit Override
        mc.ExplicitUnit = "km";
        var formattedDistExplicit = mc.FormatDistance(3.2);
        Assert(formattedDistExplicit.EndsWith("km"), $"Expected 'km' for explicit override, got '{formattedDistExplicit}'");

        // 8. Distance and Area calculations
        mc.Clear();
        mc.Mode = MeasurementMode.Distance;
        mc.AddPoint(new WorldPoint2(0, 0));
        mc.AddPoint(new WorldPoint2(30, 40)); // 3-4-5 triangle -> distance 50
        double dist = mc.CalculateDistance();
        Assert(Math.Abs(dist - 50.0) < 1e-4, $"Expected distance 50.0, got {dist}");

        mc.Clear();
        mc.Mode = MeasurementMode.Area;
        // 100 x 50 rectangle
        mc.AddPoint(new WorldPoint2(0, 0));
        mc.AddPoint(new WorldPoint2(100, 0));
        mc.AddPoint(new WorldPoint2(100, 50));
        mc.AddPoint(new WorldPoint2(0, 50));
        double area = mc.CalculateArea();
        Assert(Math.Abs(area - 5000.0) < 1e-4, $"Expected area 5000.0, got {area}");

        Console.WriteLine("-> TestInsUnitsAndMeasurementFormatting PASSED.");
    }

    private static void TestReferencePlaceholders()
    {
        Console.WriteLine("-> Running TestReferencePlaceholders...");

        // 1. Missing XREF insert
        var xrefPayload = new CadXrefPayload(
            BlockName: "SITE_SURVEY",
            XrefPath: "site_survey.dwg",
            IsResolved: false,
            ResolvedPath: null,
            InsertionPoint: new CadPoint3D(100, 100));

        var xrefEntity = new CadExtractedEntity(
            handle: "XREF_1",
            layerName: "0",
            entityType: CadExtractedEntityType.Insert,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(100, 100) },
            payload: xrefPayload);

        // 2. Missing Raster Image
        var rasterPayload = new CadRasterPayload(
            ReferenceId: "RASTER_1",
            ResolvedPath: null,
            InsertionPoint: new CadPoint3D(0, 0),
            Width: 200,
            Height: 150,
            Rotation: 0.0);

        var rasterEntity = new CadExtractedEntity(
            handle: "RASTER_1",
            layerName: "0",
            entityType: CadExtractedEntityType.Raster,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(0, 0) },
            payload: rasterPayload);

        var layer0 = new CadExtractedLayer("0", 0xFFFFFFFF, 7, IsVisible: true);
        var doc = new CadExtractedDocument(
            format: "DWG",
            version: "AC1032",
            layers: new[] { layer0 },
            entities: new[] { xrefEntity, rasterEntity },
            minX: 0, minY: 0, maxX: 300, maxY: 300);

        var scene = CadExtractedSceneBuilder.Build(doc, RenderColorContext.Dark);

        // Must produce 2 entities with missing reference placeholders, NOT silently dropped!
        Assert(scene.Entities.Count == 2, $"Expected 2 entities for missing references, got {scene.Entities.Count}");

        var xrefPrim = scene.Entities.SelectMany(e => e.Geometry).OfType<ReferencePlaceholderPrimitive>().FirstOrDefault(p => p.ReferenceType == "XREF");
        Assert(xrefPrim != null, "Missing XREF must produce ReferencePlaceholderPrimitive with ReferenceType='XREF'");

        var rasterPrim = scene.Entities.SelectMany(e => e.Geometry).OfType<ReferencePlaceholderPrimitive>().FirstOrDefault(p => p.ReferenceType == "RASTER");
        Assert(rasterPrim != null, "Missing Raster must produce ReferencePlaceholderPrimitive with ReferenceType='RASTER'");

        // Diagnostics must log missing references
        bool hasXrefDiag = false;
        bool hasRasterDiag = false;
        foreach (var d in scene.Diagnostics.Items)
        {
            if (d.Message.Contains("XREF")) hasXrefDiag = true;
            if (d.Message.Contains("Raster")) hasRasterDiag = true;
        }
        Assert(hasXrefDiag, "Scene diagnostics must log unresolved XREF");
        Assert(hasRasterDiag, "Scene diagnostics must log missing raster image");

        Console.WriteLine("-> TestReferencePlaceholders PASSED.");
    }

    private static void TestThemeRetentionWithoutSessionDisposal()
    {
        Console.WriteLine("-> Running TestThemeRetentionWithoutSessionDisposal...");

        var lineEnt = new CadExtractedEntity(
            handle: "L1",
            layerName: "0",
            entityType: CadExtractedEntityType.Line,
            color: CadEntityColor.ByLayer,
            points: new[] { new CadExtractedPoint(10, 10), new CadExtractedPoint(500, 500) });

        var doc = new CadExtractedDocument(
            format: "DWG",
            version: "AC1032",
            layers: new[] { new CadExtractedLayer("0", 0xFFFFFFFF, 7, IsVisible: true) },
            entities: new[] { lineEnt },
            minX: 10, minY: 10, maxX: 500, maxY: 500);

        var scene = CadExtractedSceneBuilder.Build(doc, RenderColorContext.Dark);
        var layoutManager = new CadLayoutManager(scene);
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "ThemeRetention.dwg");

        using var session = new CadViewerSession(metadata, scene, layoutManager, 1000, 1000);

        // Pan and zoom
        session.Pan(150, -75);
        session.ZoomIn(2.0);
        var cameraBeforeTheme = session.Camera;
        var spatialIndexBefore = session.Scene.SpatialIndex;

        // Set measurement point
        session.Measurement.Mode = MeasurementMode.Distance;
        session.Measurement.AddPoint(new WorldPoint2(100, 100));
        Assert(session.Measurement.Points.Count == 1, "Expected 1 measurement point");

        // Switch Theme: Session MUST NOT be disposed!
        session.SetColorContext(RenderColorContext.Light);

        Assert(!session.IsDisposed, "Session must NOT be disposed during theme switch");
        Assert(session.Scene.ColorContext == RenderColorContext.Light, "Scene color context must be updated to Light");
        Assert(session.Camera == cameraBeforeTheme, "Camera must be preserved exactly across theme switch");
        Assert(session.Measurement.Points.Count == 1, "Measurement points must be preserved across theme switch");
        Assert(session.Measurement.Mode == MeasurementMode.Distance, "Measurement mode must be preserved across theme switch");
        Assert(ReferenceEquals(session.Scene.SpatialIndex, spatialIndexBefore), "Spatial BVH must be reused without rebuilding");

        Console.WriteLine("-> TestThemeRetentionWithoutSessionDisposal PASSED.");
    }
}
