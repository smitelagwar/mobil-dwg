using System;
using System.Collections.Generic;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Spatial;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;

namespace MobilDwg.Rendering.Tests;

public static class SpatialIndexTests
{
    public static void Run()
    {
        TestBvhAgainstBruteForce1000Queries();
        TestBoundaryCases();
        TestDrawOrderPreservationWithSpatialIndex();
        TestTextBoundsAndObliqueShear();
        TestSparseFixtureCandidateRatioAndDensePreservation();
        Console.WriteLine("STAGE06_SPATIAL_INDEX_TESTS_PASS");
    }

    private static void TestBvhAgainstBruteForce1000Queries()
    {
        const int seed = 0x4D445747;
        var rng = new Random(seed);

        const int entityCount = 4000;
        var entities = new List<RenderSceneEntity>(entityCount);

        for (var i = 0; i < entityCount; i++)
        {
            var x = (rng.NextDouble() - 0.5) * 2000.0;
            var y = (rng.NextDouble() - 0.5) * 2000.0;
            var w = rng.NextDouble() * 50.0;
            var h = rng.NextDouble() * 50.0;

            entities.Add(CreateTestEntity($"E_{i:D5}", x, y, x + w, y + h, i));
        }

        var bvh = new StaticSceneBvh(entities, forceBvh: true);
        var bvhResults = new List<int>();
        var bruteResults = new List<int>();
        var metrics = new SpatialQueryMetrics();

        for (var q = 0; q < 1000; q++)
        {
            var qx = (rng.NextDouble() - 0.5) * 2000.0;
            var qy = (rng.NextDouble() - 0.5) * 2000.0;
            var qw = rng.NextDouble() * 300.0;
            var qh = rng.NextDouble() * 300.0;
            var queryBounds = new WorldBounds2(qx, qy, qx + qw, qy + qh);

            // BVH query
            bvh.Query(queryBounds, bvhResults, ref metrics);

            // Brute force query
            bruteResults.Clear();
            for (var i = 0; i < entities.Count; i++)
            {
                if (entities[i].Bounds.Intersects(queryBounds))
                {
                    bruteResults.Add(i);
                }
            }

            Assert(bvhResults.Count == bruteResults.Count,
                $"Query {q}: candidate count mismatch. BVH={bvhResults.Count}, Brute={bruteResults.Count}");

            // Verify no duplicates and identical sequence (order)
            for (var k = 0; k < bvhResults.Count; k++)
            {
                Assert(bvhResults[k] == bruteResults[k],
                    $"Query {q} at index {k}: entity index mismatch. BVH={bvhResults[k]}, Brute={bruteResults[k]}");

                if (k > 0)
                {
                    Assert(bvhResults[k] > bvhResults[k - 1],
                        $"Query {q}: duplicate or non-ascending index at {k}: {bvhResults[k]} <= {bvhResults[k - 1]}");
                }
            }
        }
    }

    private static void TestBoundaryCases()
    {
        var entities = new List<RenderSceneEntity>();

        // 1. Entering from 4 edges of viewport [0, 0, 100, 100]
        // Left edge: [-10, 40, 5, 60]
        entities.Add(CreateTestEntity("EDGE_LEFT", -10, 40, 5, 60, 0));
        // Right edge: [95, 40, 110, 60]
        entities.Add(CreateTestEntity("EDGE_RIGHT", 95, 40, 110, 60, 1));
        // Bottom edge: [40, -10, 60, 5]
        entities.Add(CreateTestEntity("EDGE_BOTTOM", 40, -10, 60, 5, 2));
        // Top edge: [40, 95, 60, 110]
        entities.Add(CreateTestEntity("EDGE_TOP", 40, 95, 60, 110, 3));

        // 2. Long diagonal line crossing root region: [-200, -200, 200, 200]
        entities.Add(CreateTestEntity("DIAGONAL", -200, -200, 200, 200, 4));

        // 3. Entity crossing root center split: [-50, -10, 50, 10]
        entities.Add(CreateTestEntity("ROOT_SPLIT", -50, -10, 50, 10, 5));

        // 4. Zero-area bounds:
        // Point entity at (50, 50)
        entities.Add(CreateTestEntity("POINT_ZERO", 50, 50, 50, 50, 6));
        // Vertical line segment: x = 20, y in [10, 80]
        entities.Add(CreateTestEntity("VLINE_ZERO", 20, 10, 20, 80, 7));
        // Horizontal line segment: y = 20, x in [10, 80]
        entities.Add(CreateTestEntity("HLINE_ZERO", 10, 20, 80, 20, 8));

        // 5. Thick stroke margin test:
        // Viewport is [0, 0, 100, 100]. An entity is placed at [101, 50, 105, 55] (outside [0, 100] by 1 world unit).
        // With stroke margin = 2 world units, expanded query bounds [ -2, -2, 102, 102 ] covers [101, 50, 105, 55].
        entities.Add(CreateTestEntity("STROKE_MARGIN_ENTITY", 101, 50, 105, 55, 9));

        var bvh = new StaticSceneBvh(entities, forceBvh: true);
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();

        // Query viewport [0, 0, 100, 100]
        var vp = new WorldBounds2(0, 0, 100, 100);
        bvh.Query(vp, candidates, ref metrics);

        // EDGE_LEFT (0), EDGE_RIGHT (1), EDGE_BOTTOM (2), EDGE_TOP (3),
        // DIAGONAL (4), ROOT_SPLIT (5), POINT_ZERO (6), VLINE_ZERO (7), HLINE_ZERO (8) must all be candidates
        for (var i = 0; i <= 8; i++)
        {
            Assert(candidates.Contains(i), $"Candidate list must contain entity {entities[i].Id.Value}");
        }

        // STROKE_MARGIN_ENTITY (9) is outside standard viewport [0, 0, 100, 100]
        Assert(!candidates.Contains(9), "STROKE_MARGIN_ENTITY must NOT be in unexpanded viewport query");

        // Now query with expanded stroke margin: [-2, -2, 102, 102]
        var expandedVp = new WorldBounds2(-2, -2, 102, 102);
        bvh.Query(expandedVp, candidates, ref metrics);
        Assert(candidates.Contains(9), "STROKE_MARGIN_ENTITY MUST be in expanded query bounds with stroke margin");
    }

    private static void TestDrawOrderPreservationWithSpatialIndex()
    {
        var entities = new List<RenderSceneEntity>();
        // Create 20 overlapping entities in deterministic order
        for (var i = 0; i < 20; i++)
        {
            entities.Add(CreateTestEntity($"LAYER_DRAW_{i:D2}", 10, 10, 50, 50, i));
        }

        var bvh = new StaticSceneBvh(entities, forceBvh: true);
        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();

        bvh.Query(new WorldBounds2(0, 0, 100, 100), candidates, ref metrics);

        Assert(candidates.Count == 20, "All 20 overlapping entities must be returned");
        for (var i = 0; i < 20; i++)
        {
            Assert(candidates[i] == i, $"Draw ordinal must be strictly preserved: expected {i}, got {candidates[i]}");
        }
    }

    private static void TestTextBoundsAndObliqueShear()
    {
        // Text at origin (0, 0), text "ELEVATION 123.45", height 10, oblique angle 15 degrees in radians
        var obliqueAngleRad = 15.0 * Math.PI / 180.0;
        var textPrim = new TextPrimitive(
            text: "ELEVATION 123.45",
            position: new WorldPoint2(0, 0),
            height: 10.0,
            rotationRadians: 0.0,
            widthFactor: 1.0,
            obliqueAngleRadians: obliqueAngleRad,
            horizontalAlignment: CadTextHorizontalAlignment.Left,
            verticalAlignment: CadTextVerticalAlignment.Baseline);

        var bounds = textPrim.Bounds;

        // Tan(15 deg) ~= 0.267949
        // At height 10, shear at top = 10 * tan(15) ~= 2.679
        // Descent = 0.25 * 10 = 2.5, shear at bottom = -2.5 * tan(15) ~= -0.67
        // So bounds.MinY must be <= -2.5, bounds.MaxY must extend past 10.0
        // bounds.MinX must cover negative shear from descent: bounds.MinX < 0.0
        Assert(bounds.MinY <= -2.5, $"Bounds MinY must cover descent: {bounds.MinY} <= -2.5");
        Assert(bounds.MaxY >= 10.0, $"Bounds MaxY must cover text height: {bounds.MaxY} >= 10.0");
        Assert(bounds.MinX < 0.0, $"Bounds MinX must cover negative shear from descent: {bounds.MinX} < 0.0");

        // Verify that TextLayoutMetrics.CalculateTextBounds and TextPrimitive.Bounds agree
        var directBounds = TextLayoutMetrics.CalculateTextBounds(
            "ELEVATION 123.45",
            new WorldPoint2(0, 0),
            10.0,
            0.0,
            1.0,
            obliqueAngleRad,
            CadTextHorizontalAlignment.Left,
            CadTextVerticalAlignment.Baseline);

        AssertNear(bounds.MinX, directBounds.MinX, 1e-9, "MinX match");
        AssertNear(bounds.MinY, directBounds.MinY, 1e-9, "MinY match");
        AssertNear(bounds.MaxX, directBounds.MaxX, 1e-9, "MaxX match");
        AssertNear(bounds.MaxY, directBounds.MaxY, 1e-9, "MaxY match");

        // Oblique overhang query: a query box that lies strictly in the sheared region [bounds.MinX, -2.5, -0.01, -1.0]
        var shearQuery = new WorldBounds2(bounds.MinX, -2.5, -0.01, -1.0);
        Assert(bounds.Intersects(shearQuery), "Conservative bounds must intersect oblique shear region");
    }

    private static void TestSparseFixtureCandidateRatioAndDensePreservation()
    {
        const int seed = 0x4D445747;
        var rng = new Random(seed);

        // 150k sparse fixture distributed over [-50000, 50000] x [-50000, 50000]
        const int sparseCount = 150_000;
        var sparseEntities = new List<RenderSceneEntity>(sparseCount);

        for (var i = 0; i < sparseCount; i++)
        {
            var cx = (rng.NextDouble() - 0.5) * 100_000.0;
            var cy = (rng.NextDouble() - 0.5) * 100_000.0;
            var sz = 5.0 + (rng.NextDouble() * 10.0);
            sparseEntities.Add(CreateTestEntity($"S_{i:D6}", cx - sz, cy - sz, cx + sz, cy + sz, i));
        }

        var sparseBvh = new StaticSceneBvh(sparseEntities);
        Assert(sparseBvh.UsesBvh, "150k entities must trigger BVH usage");

        var candidates = new List<int>();
        var metrics = new SpatialQueryMetrics();

        // Narrow viewport: [-1000, -1000, 1000, 1000] (covers ~0.04% of total world area)
        var narrowVp = new WorldBounds2(-1000, -1000, 1000, 1000);
        sparseBvh.Query(narrowVp, candidates, ref metrics);

        var candidateRatio = (double)candidates.Count / sparseCount;
        // The plan requirement: "150k seyrek yerleşimli kontrollü fixture'ın dar viewport'unda candidate < %20"
        Assert(candidateRatio < 0.20,
            $"Sparse candidate ratio must be < 20%, but was {candidateRatio * 100:F2}% ({candidates.Count}/{sparseCount})");

        // Dense overlapping fixture: 10,000 entities all overlapping in [-10, 10] x [-10, 10]
        const int denseCount = 10_000;
        var denseEntities = new List<RenderSceneEntity>(denseCount);
        for (var i = 0; i < denseCount; i++)
        {
            var x = (rng.NextDouble() - 0.5) * 10.0;
            var y = (rng.NextDouble() - 0.5) * 10.0;
            denseEntities.Add(CreateTestEntity($"D_{i:D5}", x - 2, y - 2, x + 2, y + 2, i));
        }

        var denseBvh = new StaticSceneBvh(denseEntities);
        var denseCandidates = new List<int>();
        var denseMetrics = new SpatialQueryMetrics();

        // Query covering the dense region
        denseBvh.Query(new WorldBounds2(-20, -20, 20, 20), denseCandidates, ref denseMetrics);

        // All 10,000 entities must be preserved with 0 false negatives
        Assert(denseCandidates.Count == denseCount,
            $"Dense fixture must preserve 100% accuracy: expected {denseCount}, got {denseCandidates.Count}");
    }

    private static RenderSceneEntity CreateTestEntity(
        string id,
        double minX,
        double minY,
        double maxX,
        double maxY,
        int sourceIndex) => new(
            new RenderEntityId(id),
            new WorldBounds2(minX, minY, maxX, maxY),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEST", id, sourceIndex));

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"[SpatialIndexTests] Assertion failed: {message}");
        }
    }

    private static void AssertNear(double actual, double expected, double tolerance, string message)
    {
        if (!double.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
        {
            throw new InvalidOperationException($"[SpatialIndexTests] {message}: expected={expected:R}, actual={actual:R}, tolerance={tolerance:R}");
        }
    }
}
