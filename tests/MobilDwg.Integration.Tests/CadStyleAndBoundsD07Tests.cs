using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Spatial;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Integration.Tests;

public static class CadStyleAndBoundsD07Tests
{
    public static void RunAll()
    {
        Console.WriteLine("=== RUNNING D07 STYLE, VISIBILITY, DRAW ORDER AND BOUNDS TESTS ===");
        TestTrueColorBlackAndAciResolution();
        TestFrozenAndOffLayerVisibility();
        TestLinetypeTableAndEntityInheritance();
        TestDrawOrderAndSortentsFidelity();
        TestWidePolylineConservativeBounds();
        TestBvhStackReuseAndBruteForceEquivalence();
        Console.WriteLine("=== D07 STYLE, VISIBILITY, DRAW ORDER AND BOUNDS TESTS PASSED ===");
    }

    private static void TestTrueColorBlackAndAciResolution()
    {
        var doc = new CadDocument();

        // 1. Layer with TrueColor Black (RGB 0, 0, 0)
        var blackLayer = new Layer("BLACK_LAYER")
        {
            Color = new Color((byte)0, (byte)0, (byte)0) // TrueColor Black
        };
        doc.Layers.Add(blackLayer);

        // 2. Layer with TrueColor Custom (RGB 12, 34, 56)
        var customLayer = new Layer("CUSTOM_COLOR_LAYER")
        {
            Color = new Color((byte)12, (byte)34, (byte)56)
        };
        doc.Layers.Add(customLayer);

        // 3. Entity with TrueColor Black on default layer
        var blackLine = new Line
        {
            StartPoint = new XYZ(0, 0, 0),
            EndPoint = new XYZ(10, 0, 0),
            Color = new Color((byte)0, (byte)0, (byte)0)
        };
        doc.Entities.Add(blackLine);

        // 4. Entity with ACI color 1 (Red)
        var aciLine = new Line
        {
            StartPoint = new XYZ(10, 0, 0),
            EndPoint = new XYZ(20, 0, 0),
            Color = new Color((short)1)
        };
        doc.Entities.Add(aciLine);

        var handle = new AcadSharpDocumentHandle(doc, TimeSpan.Zero, CadFormat.Dxf, "AC1015");
        var extracted = AcadSharpEntityExtractor.Extract(handle);

        var extractedBlackLayer = extracted.Layers.Single(l => l.Name == "BLACK_LAYER");
        if (!extractedBlackLayer.HasTrueColor)
        {
            throw new InvalidOperationException("Layer with RGB(0,0,0) black was not marked with HasTrueColor=true.");
        }
        if (extractedBlackLayer.ArgbColor != 0xFF000000u)
        {
            throw new InvalidOperationException($"Layer with RGB(0,0,0) black Argb was 0x{extractedBlackLayer.ArgbColor:X8}, expected 0xFF000000.");
        }

        var extractedCustomLayer = extracted.Layers.Single(l => l.Name == "CUSTOM_COLOR_LAYER");
        if (!extractedCustomLayer.HasTrueColor)
        {
            throw new InvalidOperationException("Custom color layer was not marked with HasTrueColor=true.");
        }
        uint expectedCustomArgb = 0xFF000000u | (12u << 16) | (34u << 8) | 56u;
        if (extractedCustomLayer.ArgbColor != expectedCustomArgb)
        {
            throw new InvalidOperationException($"Custom color layer Argb was 0x{extractedCustomLayer.ArgbColor:X8}, expected 0x{expectedCustomArgb:X8}.");
        }

        // Build scene and verify resolved colors in Dark context
        var scene = CadExtractedSceneBuilder.Build(extracted, RenderColorContext.Dark);

        // Verify layer definition in table
        var sceneBlackLayer = scene.LayerTable.GetLayer("BLACK_LAYER");
        if (sceneBlackLayer.Color.Kind != CadColorKind.TrueColor || sceneBlackLayer.Color.Argb != 0xFF000000u)
        {
            throw new InvalidOperationException($"Scene LayerTable black layer color was {sceneBlackLayer.Color}, expected TrueColor 0xFF000000.");
        }

        // Verify black line entity
        var entity0 = scene.Entities[0];
        var resolved0 = CadStyleResolver.Resolve(entity0.CadStyle, entity0.Layer, scene.LayerTable, scene.ColorContext, 1.0);
        if (resolved0.ArgbColor != 0xFF000000u)
        {
            throw new InvalidOperationException($"Entity with TrueColor black resolved to 0x{resolved0.ArgbColor:X8}, expected 0xFF000000.");
        }

        // Verify ACI 1 line entity (Red)
        var entity1 = scene.Entities[1];
        var resolved1 = CadStyleResolver.Resolve(entity1.CadStyle, entity1.Layer, scene.LayerTable, scene.ColorContext, 1.0);
        if ((resolved1.ArgbColor & 0x00FF0000u) == 0) // Should have strong red channel
        {
            throw new InvalidOperationException($"Entity with ACI 1 red resolved to 0x{resolved1.ArgbColor:X8}, expected red.");
        }

        Console.WriteLine("  [PASS] TestTrueColorBlackAndAciResolution");
    }

    private static void TestFrozenAndOffLayerVisibility()
    {
        var doc = new CadDocument();

        var frozenLayer = new Layer("FROZEN")
        {
            Flags = LayerFlags.Frozen
        };
        doc.Layers.Add(frozenLayer);

        var offLayer = new Layer("OFF_LAYER")
        {
            IsOn = false
        };
        doc.Layers.Add(offLayer);

        var lineOnFrozen = new Line
        {
            StartPoint = new XYZ(0, 0, 0),
            EndPoint = new XYZ(10, 0, 0),
            Layer = frozenLayer
        };
        doc.Entities.Add(lineOnFrozen);

        var lineOnNormal = new Line
        {
            StartPoint = new XYZ(0, 10, 0),
            EndPoint = new XYZ(10, 10, 0)
        };
        doc.Entities.Add(lineOnNormal);

        var handle = new AcadSharpDocumentHandle(doc, TimeSpan.Zero, CadFormat.Dxf, "AC1015");
        var extracted = AcadSharpEntityExtractor.Extract(handle);

        var extFrozen = extracted.Layers.Single(l => l.Name == "FROZEN");
        if (!extFrozen.IsFrozen) throw new InvalidOperationException("FROZEN layer was not extracted with IsFrozen=true.");
        if (extFrozen.IsVisible) throw new InvalidOperationException("FROZEN layer must have IsVisible=false.");

        var extOff = extracted.Layers.Single(l => l.Name == "OFF_LAYER");
        if (extOff.IsVisible) throw new InvalidOperationException("OFF layer must have IsVisible=false.");

        var scene = CadExtractedSceneBuilder.Build(extracted, RenderColorContext.Dark);

        var layerFrozenDef = scene.LayerTable.GetLayer("FROZEN");
        if (layerFrozenDef.IsRenderable)
        {
            throw new InvalidOperationException("FROZEN layer definition in LayerTable must not be renderable.");
        }

        // Entity on frozen layer must resolve as not visible
        var entity0 = scene.Entities.Single(e => e.Layer.Value == "FROZEN");
        var res0 = CadStyleResolver.Resolve(entity0.CadStyle, entity0.Layer, scene.LayerTable, scene.ColorContext, 1.0);
        if (res0.IsVisible)
        {
            throw new InvalidOperationException("Entity on FROZEN layer must not be visible.");
        }

        Console.WriteLine("  [PASS] TestFrozenAndOffLayerVisibility");
    }

    private static void TestLinetypeTableAndEntityInheritance()
    {
        var doc = new CadDocument();

        var lt = new LineType("CUSTOM_DASH")
        {
            Description = "Custom dashed pattern"
        };
        lt.AddSegment(new LineType.Segment { Length = 15.0 });
        lt.AddSegment(new LineType.Segment { Length = -5.0 });
        doc.LineTypes.Add(lt);

        var layer = new Layer("DASH_LAYER")
        {
            LineType = lt
        };
        doc.Layers.Add(layer);

        var line = new Line
        {
            StartPoint = new XYZ(0, 0, 0),
            EndPoint = new XYZ(100, 0, 0),
            Layer = layer
        };
        doc.Entities.Add(line);

        var handle = new AcadSharpDocumentHandle(doc, TimeSpan.Zero, CadFormat.Dxf, "AC1015");
        var extracted = AcadSharpEntityExtractor.Extract(handle);

        var extLt = extracted.Linetypes.Single(l => l.Name == "CUSTOM_DASH");
        if (extLt.PatternSegments.Count != 2 || extLt.PatternSegments[0] != 15.0 || extLt.PatternSegments[1] != -5.0)
        {
            throw new InvalidOperationException("Custom linetype pattern segments were not extracted correctly.");
        }

        var scene = CadExtractedSceneBuilder.Build(extracted);
        var sceneLayer = scene.LayerTable.GetLayer("DASH_LAYER");
        if (sceneLayer.Linetype.Name != "CUSTOM_DASH" || sceneLayer.Linetype.Pattern.Count != 2)
        {
            throw new InvalidOperationException($"Scene layer linetype was not resolved to CUSTOM_DASH (got '{sceneLayer.Linetype.Name}').");
        }

        var entity = scene.Entities[0];
        var resolved = CadStyleResolver.Resolve(entity.CadStyle, entity.Layer, scene.LayerTable, scene.ColorContext, worldUnitsPerPixel: 1.0);
        if (resolved.DashPatternPixels == null || resolved.DashPatternPixels.Length != 2)
        {
            throw new InvalidOperationException("Resolved style did not compute pixel dash pattern for CUSTOM_DASH.");
        }

        Console.WriteLine("  [PASS] TestLinetypeTableAndEntityInheritance");
    }

    private static void TestDrawOrderAndSortentsFidelity()
    {
        var doc = new CadDocument();

        var line1 = new Line { StartPoint = new XYZ(0, 0, 0), EndPoint = new XYZ(10, 0, 0) }; // natural 1
        var line2 = new Line { StartPoint = new XYZ(0, 1, 0), EndPoint = new XYZ(10, 1, 0) }; // natural 2
        var line3 = new Line { StartPoint = new XYZ(0, 2, 0), EndPoint = new XYZ(10, 2, 0) }; // natural 3

        doc.Entities.Add(line1);
        doc.Entities.Add(line2);
        doc.Entities.Add(line3);

        // Set SortEntitiesTable to order line2 (sortHandle 100), line3 (sortHandle 200), line1 (sortHandle 300)
        var modelSpace = doc.BlockRecords["*Model_Space"];
        var sortTable = modelSpace.CreateSortEntitiesTable();
        sortTable.Add(line1, 300);
        sortTable.Add(line2, 100);
        sortTable.Add(line3, 200);

        var handle = new AcadSharpDocumentHandle(doc, TimeSpan.Zero, CadFormat.Dxf, "AC1015");
        var extracted = AcadSharpEntityExtractor.Extract(handle);

        // Extracted entities must be ordered by sort handle: line2 (100) first, line3 (200) second, line1 (300) third
        if (extracted.Entities.Count != 3) throw new InvalidOperationException("Expected 3 entities.");
        if (extracted.Entities[0].Handle != line2.Handle.ToString("X"))
        {
            throw new InvalidOperationException($"Expected first entity to be line2 (handle {line2.Handle:X}), got {extracted.Entities[0].Handle}. Draw order not applied.");
        }
        if (extracted.Entities[1].Handle != line3.Handle.ToString("X"))
        {
            throw new InvalidOperationException($"Expected second entity to be line3 (handle {line3.Handle:X}), got {extracted.Entities[1].Handle}. Draw order not applied.");
        }
        if (extracted.Entities[2].Handle != line1.Handle.ToString("X"))
        {
            throw new InvalidOperationException($"Expected third entity to be line1 (handle {line1.Handle:X}), got {extracted.Entities[2].Handle}. Draw order not applied.");
        }

        var scene = CadExtractedSceneBuilder.Build(extracted);
        if (scene.Entities[0].Id.Value != line2.Handle.ToString("X") ||
            scene.Entities[1].Id.Value != line3.Handle.ToString("X") ||
            scene.Entities[2].Id.Value != line1.Handle.ToString("X"))
        {
            throw new InvalidOperationException("Scene entities do not match SortEntitiesTable draw order.");
        }

        // Query spatial index - returned candidates must strictly preserve this draw order!
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();
        scene.SpatialIndex.Query(new WorldBounds2(-10, -10, 20, 20), candidates, ref metrics);

        if (candidates.Count != 3 || candidates[0] != 0 || candidates[1] != 1 || candidates[2] != 2)
        {
            throw new InvalidOperationException($"Spatial query did not return candidates in exact draw order: [{string.Join(", ", candidates)}]");
        }

        Console.WriteLine("  [PASS] TestDrawOrderAndSortentsFidelity");
    }

    private static void TestWidePolylineConservativeBounds()
    {
        // A polyline from (10, 20) to (50, 20) with width = 8.0.
        // Centerline bounds: [10, 20, 50, 20]
        // Conservative bounds with width=8.0 (half-width 4.0):
        // MinX = 10 - 4 = 6
        // MaxX = 50 + 4 = 54
        // MinY = 20 - 4 = 16
        // MaxY = 20 + 4 = 24
        var vertices = new[]
        {
            new CadExtractedVertex(10, 20, StartWidth: 8.0, EndWidth: 8.0),
            new CadExtractedVertex(50, 20, StartWidth: 8.0, EndWidth: 8.0),
        };

        var polyEntity = new CadExtractedEntity(
            "POLY1",
            "0",
            CadExtractedEntityType.Polyline,
            CadEntityColor.ByLayer,
            sourceOrder: 1,
            vertices: vertices,
            payload: new CadPolylinePayload(vertices, IsClosed: false));

        var extracted = new CadExtractedDocument(
            "DXF", "AC1015",
            new[] { new CadExtractedLayer("0", 0xFFFFFFFF, 7, true) },
            new[] { polyEntity },
            minX: 0, minY: 0, maxX: 100, maxY: 100);

        var scene = CadExtractedSceneBuilder.Build(extracted);
        var scenePoly = scene.Entities[0];

        var b = scenePoly.Bounds;
        if (Math.Abs(b.MinX - 6.0) > 1e-6 || Math.Abs(b.MaxX - 54.0) > 1e-6 ||
            Math.Abs(b.MinY - 16.0) > 1e-6 || Math.Abs(b.MaxY - 24.0) > 1e-6)
        {
            throw new InvalidOperationException($"Polyline bounds not expanded by half-width: expected [6, 16, 54, 24], got [{b.MinX}, {b.MinY}, {b.MaxX}, {b.MaxY}].");
        }

        // Test spatial query on the margin: box [7, 17, 9, 19] does NOT intersect centerline Y=20,
        // but DOES intersect the thick boundary Y in [16, 24]!
        var queryBox = new WorldBounds2(7, 17, 9, 19);
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();
        scene.SpatialIndex.Query(queryBox, candidates, ref metrics);

        if (candidates.Count != 1)
        {
            throw new InvalidOperationException("Spatial query missed wide polyline at edge boundary!");
        }

        Console.WriteLine("  [PASS] TestWidePolylineConservativeBounds");
    }

    private static void TestBvhStackReuseAndBruteForceEquivalence()
    {
        // Construct 2500 entities across a 1000x1000 space to force BVH tree construction (BvhEntityThreshold = 2048)
        var rnd = new Random(42);
        var entities = new List<RenderSceneEntity>(2500);
        var layerToken = new RenderLayerToken("0");
        var styleToken = new RenderStyleToken("BYLAYER");

        for (int i = 0; i < 2500; i++)
        {
            double x = rnd.NextDouble() * 1000.0;
            double y = rnd.NextDouble() * 1000.0;
            double w = 2.0 + rnd.NextDouble() * 10.0;
            double h = 2.0 + rnd.NextDouble() * 10.0;

            var bounds = new WorldBounds2(x, y, x + w, y + h);
            var geom = new RenderGeometryPrimitive[]
            {
                new LinePrimitive(new WorldPoint2(x, y), new WorldPoint2(x + w, y + h))
            };
            var source = new RenderSourceReference("Line", $"E{i:D4}", i + 1);
            entities.Add(new RenderSceneEntity(new RenderEntityId($"E{i:D4}"), bounds, layerToken, styleToken, source, geom));
        }

        var bvh = new StaticSceneBvh(entities, forceBvh: true);
        if (!bvh.UsesBvh)
        {
            throw new InvalidOperationException("StaticSceneBvh must use BVH tree for >= 2048 entities.");
        }

        // Test 200 random viewports covering center, corners, thin slices, and wide areas
        var bvhCandidates = new List<int>();
        var bruteForceCandidates = new List<int>();
        var metrics = new SpatialQueryMetrics();

        for (int q = 0; q < 200; q++)
        {
            double qx = rnd.NextDouble() * 1000.0;
            double qy = rnd.NextDouble() * 1000.0;
            double qw = 50.0 + rnd.NextDouble() * 200.0;
            double qh = 50.0 + rnd.NextDouble() * 200.0;

            var qBounds = new WorldBounds2(qx, qy, qx + qw, qy + qh);

            // 1. BVH Query
            bvh.Query(qBounds, bvhCandidates, ref metrics);

            // 2. Brute Force Query (ground truth)
            bruteForceCandidates.Clear();
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].Bounds.Intersects(qBounds))
                {
                    bruteForceCandidates.Add(i);
                }
            }

            // Compare candidate counts and ordering
            if (bvhCandidates.Count != bruteForceCandidates.Count)
            {
                throw new InvalidOperationException($"BVH query {q} candidate count mismatch: BVH returned {bvhCandidates.Count}, brute-force found {bruteForceCandidates.Count}.");
            }

            for (int k = 0; k < bvhCandidates.Count; k++)
            {
                if (bvhCandidates[k] != bruteForceCandidates[k])
                {
                    throw new InvalidOperationException($"BVH query {q} candidate [{k}] mismatch: BVH={bvhCandidates[k]}, brute-force={bruteForceCandidates[k]}. Draw order altered!");
                }
            }
        }

        Console.WriteLine("  [PASS] TestBvhStackReuseAndBruteForceEquivalence (200 queries on 2500 entities match brute force 100%)");
    }
}
