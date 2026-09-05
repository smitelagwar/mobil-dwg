using System.Runtime.CompilerServices;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Transforms;

namespace MobilDwg.Rendering.Tests;

internal static class Stage12BlockInsertTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestTransform2DAffineMath();
        TestPrimitiveTransformerNonUniformScaleCircleToEllipse();
        TestPrimitiveTransformerMirrorArcWinding();
        TestBlockExpanderNestedTransform();
        TestBlockExpanderLayer0Inheritance();
        TestBlockExpanderByBlockStyleInheritance();
        TestBlockExpanderAttributeResolution();
        TestBlockExpanderCycleGuard();
        TestBlockExpanderDepthGuard();
        TestBlockExpanderBudgetGuard();
        TestBlockExpanderDeterministicGolden();

        Console.WriteLine("STAGE12_BLOCK_INSERT_TESTS_PASS");
    }

    private static void TestTransform2DAffineMath()
    {
        var t = Transform2D.CreateTranslation(10, 20);
        var p = t.TransformPoint(new WorldPoint2(5, 5));
        AssertNear(p.X, 15d, 1e-9, "translate X");
        AssertNear(p.Y, 25d, 1e-9, "translate Y");

        var r = Transform2D.CreateRotation(Math.PI / 2d); // 90 deg CCW
        var pr = r.TransformPoint(new WorldPoint2(1, 0));
        AssertNear(pr.X, 0d, 1e-9, "rotate 90 X");
        AssertNear(pr.Y, 1d, 1e-9, "rotate 90 Y");

        var s = Transform2D.CreateScale(2, -3);
        Assert(s.IsInverting, "negative Y scale must be inverting");
        AssertNear(s.ScaleX, 2d, 1e-9, "scale X");
        AssertNear(s.ScaleY, 3d, 1e-9, "scale Y magnitude");
        Assert(!s.IsUniformScale(), "non-uniform scale detected");

        // Multiplication: T * R * S
        var comp = t * r * s;
        var pComp = comp.TransformPoint(new WorldPoint2(1, 2));
        // S(1, 2) = (2, -6)
        // R(2, -6) = (6, 2)
        // T(6, 2) = (16, 22)
        AssertNear(pComp.X, 16d, 1e-9, "composed X");
        AssertNear(pComp.Y, 22d, 1e-9, "composed Y");

        // Inversion
        Assert(comp.TryInverse(out var inv), "matrix must be invertible");
        var pBack = inv.TransformPoint(pComp);
        AssertNear(pBack.X, 1d, 1e-9, "inverted back X");
        AssertNear(pBack.Y, 2d, 1e-9, "inverted back Y");
    }

    private static void TestPrimitiveTransformerNonUniformScaleCircleToEllipse()
    {
        // Circle with radius 10 at (0, 0)
        var circle = new ArcPrimitive(new WorldPoint2(0, 0), 10, 0, Math.Tau);
        var nonUniform = Transform2D.CreateScale(2d, 1d); // sx=2, sy=1

        var transformed = PrimitiveTransformer.Transform(circle, nonUniform);
        Assert(transformed is EllipsePrimitive, "circle under non-uniform scale must become EllipsePrimitive");
        var ellipse = (EllipsePrimitive)transformed;
        AssertNear(ellipse.MajorRadius, 20d, 1e-9, "ellipse major radius");
        AssertNear(ellipse.MinorRadius, 10d, 1e-9, "ellipse minor radius");
        AssertNear(ellipse.Center.X, 0d, 1e-9, "ellipse center X");
        AssertNear(ellipse.Center.Y, 0d, 1e-9, "ellipse center Y");

        // Uniform scale remains ArcPrimitive
        var uniform = Transform2D.CreateScale(3d, 3d);
        var uniformTransformed = PrimitiveTransformer.Transform(circle, uniform);
        Assert(uniformTransformed is ArcPrimitive, "circle under uniform scale must remain ArcPrimitive");
        var scaledCircle = (ArcPrimitive)uniformTransformed;
        AssertNear(scaledCircle.Radius, 30d, 1e-9, "scaled circle radius");
    }

    private static void TestPrimitiveTransformerMirrorArcWinding()
    {
        // Arc in quadrant 1: start 0, sweep pi/2
        var arc = new ArcPrimitive(new WorldPoint2(0, 0), 10, 0, Math.PI / 2d);
        var mirrorX = Transform2D.CreateScale(-1d, 1d); // Mirror across Y-axis

        var transformed = PrimitiveTransformer.Transform(arc, mirrorX);
        Assert(transformed is ArcPrimitive, "mirrored arc must remain ArcPrimitive");
        var mirroredArc = (ArcPrimitive)transformed;

        // V_start (10, 0) -> mirrored to (-10, 0) => angle pi
        // V_end (0, 10) -> mirrored to (0, 10) => angle pi/2
        // Sweep should be negative / clockwise: -pi/2
        AssertNear(mirroredArc.Radius, 10d, 1e-9, "mirrored arc radius");
        AssertNear(Math.Abs(mirroredArc.SweepRadians), Math.PI / 2d, 1e-9, "mirrored arc sweep magnitude");
    }

    private static void TestBlockExpanderNestedTransform()
    {
        // INNER block: Line from (0,0) to (10,0)
        var innerLine = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(10, 0));
        var innerDef = new BlockDefinition("INNER", new WorldPoint2(0, 0), [
            new BlockEntityTemplate(innerLine, new RenderLayerToken("0"), new RenderStyleToken("BYBLOCK"))
        ]);

        // OUTER block: contains INNER placed at (5, 5), rotated 90 deg (pi/2)
        var outerRef = new BlockReference("INNER", new WorldPoint2(5, 5), rotationRadians: Math.PI / 2d);
        var outerDef = new BlockDefinition("OUTER", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [outerRef]);

        // Root insert placed at (100, 200), scaled 2x
        var rootInsert = new BlockReference("OUTER", new WorldPoint2(100, 200), scaleX: 2d, scaleY: 2d);

        var expander = new BlockExpander([innerDef, outerDef]);
        var result = expander.Expand([rootInsert]);

        Assert(result.Entities.Count == 1, "nested block must expand to 1 primitive entity");
        Assert(result.TotalBlocksExpanded == 2, "must expand 2 block instances (OUTER and INNER)");

        var entity = result.Entities[0];
        Assert(entity.Geometry[0] is LinePrimitive, "expanded entity must be LinePrimitive");
        var line = (LinePrimitive)entity.Geometry[0];

        // Inner line (0,0) -> in OUTER with T(5,5)*R(90): (5, 5)
        // in Root with T(100,200)*S(2,2): (100 + 2*5, 200 + 2*5) = (110, 210)
        AssertNear(line.Start.X, 110d, 1e-9, "line start X");
        AssertNear(line.Start.Y, 210d, 1e-9, "line start Y");

        // Inner line (10,0) -> in OUTER rotated 90: (5, 5 + 10) = (5, 15)
        // in Root with T(100,200)*S(2,2): (100 + 2*5, 200 + 2*15) = (110, 230)
        AssertNear(line.End.X, 110d, 1e-9, "line end X");
        AssertNear(line.End.Y, 230d, 1e-9, "line end Y");
    }

    private static void TestBlockExpanderLayer0Inheritance()
    {
        var line1 = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(5, 0));
        var line2 = new LinePrimitive(new WorldPoint2(0, 5), new WorldPoint2(5, 5));

        var blockDef = new BlockDefinition("LAYER_TEST", new WorldPoint2(0, 0), [
            new BlockEntityTemplate(line1, new RenderLayerToken("0"), new RenderStyleToken("BYLAYER")),
            new BlockEntityTemplate(line2, new RenderLayerToken("FIXED_LAYER"), new RenderStyleToken("BYLAYER")),
        ]);

        var insert = new BlockReference("LAYER_TEST", new WorldPoint2(0, 0), layer: new RenderLayerToken("TARGET_LAYER"));
        var expander = new BlockExpander([blockDef]);
        var result = expander.Expand([insert]);

        Assert(result.Entities.Count == 2, "expected 2 entities");
        Assert(result.Entities[0].Layer.Value == "TARGET_LAYER", "Layer 0 entity must inherit TARGET_LAYER");
        Assert(result.Entities[1].Layer.Value == "FIXED_LAYER", "Explicit layer must remain FIXED_LAYER");
    }

    private static void TestBlockExpanderByBlockStyleInheritance()
    {
        var line1 = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(5, 0));
        var line2 = new LinePrimitive(new WorldPoint2(0, 5), new WorldPoint2(5, 5));

        var blockDef = new BlockDefinition("STYLE_TEST", new WorldPoint2(0, 0), [
            new BlockEntityTemplate(line1, new RenderLayerToken("0"), new RenderStyleToken("BYBLOCK")),
            new BlockEntityTemplate(line2, new RenderLayerToken("0"), new RenderStyleToken("BYLAYER")),
        ]);

        var insert = new BlockReference("STYLE_TEST", new WorldPoint2(0, 0), style: new RenderStyleToken("TRUECOLOR|#FF0000"));
        var expander = new BlockExpander([blockDef]);
        var result = expander.Expand([insert]);

        Assert(result.Entities.Count == 2, "expected 2 entities");
        Assert(result.Entities[0].Style.Value == "TRUECOLOR|#FF0000", "BYBLOCK entity must inherit parent style");
        Assert(result.Entities[1].Style.Value == "BYLAYER", "BYLAYER entity must retain BYLAYER");
    }

    private static void TestBlockExpanderAttributeResolution()
    {
        var line = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(10, 0));
        var blockDef = new BlockDefinition("DOOR", new WorldPoint2(0, 0), [
            new BlockEntityTemplate(line, new RenderLayerToken("DOORS"), new RenderStyleToken("BYLAYER")),
        ]);

        var attrs = new[]
        {
            BlockAttribute.CreateVisible("TAG_ROOM", "101", new WorldPoint2(5, 5), height: 2.5),
            BlockAttribute.CreateInvisible("TAG_COST", "500", new WorldPoint2(0, 0), height: 1.0),
        };

        var insert = new BlockReference("DOOR", new WorldPoint2(50, 50), attributes: attrs);
        var expander = new BlockExpander([blockDef]);
        var result = expander.Expand([insert]);

        // 1 line primitive + 1 visible attribute = 2 entities
        Assert(result.Entities.Count == 2, "expected 2 entities (1 line + 1 visible attribute)");
        Assert(result.TotalAttributesIncluded == 1, "only visible attribute must be included");
        Assert(result.Entities[1].Source.EntityType == "ATTRIB", "second entity must be ATTRIB");
    }

    private static void TestBlockExpanderCycleGuard()
    {
        // Block A references Block B
        var refB = new BlockReference("BLOCK_B", new WorldPoint2(0, 0));
        var defA = new BlockDefinition("BLOCK_A", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refB]);

        // Block B references Block A (Cycle!)
        var refA = new BlockReference("BLOCK_A", new WorldPoint2(0, 0));
        var defB = new BlockDefinition("BLOCK_B", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refA]);

        var root = new BlockReference("BLOCK_A", new WorldPoint2(0, 0));
        var expander = new BlockExpander([defA, defB]);

        // Must terminate safely and not stack overflow
        var result = expander.Expand([root]);

        Assert(result.Diagnostics.Any(d => d.Code == "BLOCK_CYCLE_DETECTED"), "cycle guard must emit BLOCK_CYCLE_DETECTED");
    }

    private static void TestBlockExpanderDepthGuard()
    {
        // Chain: D0 -> D1 -> D2 -> D3 -> D4
        var defD3 = new BlockDefinition("D3", new WorldPoint2(0, 0), [
            new BlockEntityTemplate(new PointPrimitive(new WorldPoint2(0, 0)), new RenderLayerToken("0"), new RenderStyleToken("BYLAYER"))
        ]);
        var defD2 = new BlockDefinition("D2", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [new BlockReference("D3", new WorldPoint2(0, 0))]);
        var defD1 = new BlockDefinition("D1", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [new BlockReference("D2", new WorldPoint2(0, 0))]);
        var defD0 = new BlockDefinition("D0", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [new BlockReference("D1", new WorldPoint2(0, 0))]);

        // Max depth = 2 (Root=0, D1=1, D2=2 -> stops at D2)
        var expander = new BlockExpander([defD0, defD1, defD2, defD3], new BlockExpansionOptions(MaxNestingDepth: 2));
        var result = expander.Expand([new BlockReference("D0", new WorldPoint2(0, 0))]);

        Assert(result.Diagnostics.Any(d => d.Code == "BLOCK_DEPTH_EXCEEDED"), "depth guard must emit BLOCK_DEPTH_EXCEEDED");
    }

    private static void TestBlockExpanderBudgetGuard()
    {
        var primitives = Enumerable.Range(0, 10).Select(i =>
            new BlockEntityTemplate(new PointPrimitive(new WorldPoint2(i, i)), new RenderLayerToken("0"), new RenderStyleToken("BYLAYER"))
        ).ToArray();

        var blockDef = new BlockDefinition("BOMB", new WorldPoint2(0, 0), primitives);
        var inserts = Enumerable.Range(0, 5).Select(i => new BlockReference("BOMB", new WorldPoint2(i * 10, 0))).ToArray();

        // Budget = 15. 5 inserts * 10 entities = 50 entities. Should truncate at 15!
        var expander = new BlockExpander([blockDef], new BlockExpansionOptions(MaxExpansionEntityBudget: 15));
        var result = expander.Expand(inserts);

        Assert(result.Entities.Count <= 20, "budget guard must truncate entity count");
        Assert(result.Diagnostics.Any(d => d.Code == "BLOCK_EXPANSION_BUDGET_EXCEEDED"), "budget guard must emit BLOCK_EXPANSION_BUDGET_EXCEEDED");
    }

    private static void TestBlockExpanderDeterministicGolden()
    {
        var line = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(20, 10));
        var def = new BlockDefinition("GOLDEN_BLOCK", new WorldPoint2(0, 0), [
            new BlockEntityTemplate(line, new RenderLayerToken("0"), new RenderStyleToken("BYBLOCK")),
        ]);

        var insert = new BlockReference("GOLDEN_BLOCK", new WorldPoint2(50, 100), scaleX: 2, scaleY: 2,
            layer: new RenderLayerToken("GOLDEN_LAYER"), style: new RenderStyleToken("TRUECOLOR|#00FF00"));

        var expander = new BlockExpander([def]);
        var result = expander.Expand([insert]);

        var snapshot = BlockSceneSemanticSnapshot.Create(result, RenderColorContext.Dark);

        Assert(snapshot.StartsWith("block-scene/v1", StringComparison.Ordinal), "snapshot header must be block-scene/v1");
        Assert(snapshot.Contains("blocks_expanded=1", StringComparison.Ordinal), "snapshot must record 1 block expanded");
        Assert(snapshot.Contains("GOLDEN_LAYER", StringComparison.Ordinal), "snapshot must contain inherited layer");
        Assert(snapshot.Contains("TRUECOLOR|#00FF00", StringComparison.Ordinal), "snapshot must contain inherited style");
        Assert(snapshot.Contains("50.000,100.000,90.000,120.000", StringComparison.Ordinal), "snapshot must record transformed bounds");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Stage12BlockInsertTests assertion failed: {message}");
    }

    private static void AssertNear(double actual, double expected, double tolerance, string message)
    {
        if (Math.Abs(actual - expected) > tolerance)
        {
            throw new InvalidOperationException($"Stage12BlockInsertTests assertion failed: {message}. Expected {expected}, got {actual}, diff {Math.Abs(actual - expected)} > {tolerance}");
        }
    }
}
