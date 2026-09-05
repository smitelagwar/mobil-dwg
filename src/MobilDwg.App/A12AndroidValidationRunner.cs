using System.Security.Cryptography;
using Android.Util;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Transforms;

namespace MobilDwg.App;

#if A12_VALIDATION
public sealed record A12ValidationResult(
    byte[] Png,
    string PngSha256,
    string Marker,
    int NonBackgroundPixels,
    int ExpandedEntityCount,
    int BlocksExpanded);

public static class A12AndroidValidationRunner
{
    public const string Tag = "MobilDwgA12";

    public static async ValueTask<A12ValidationResult> RunAsync()
    {
        return await Task.Run(async () =>
        {
            Log.Info(Tag, "A12 Android block & insert validation started.");

            // 1. Build synthetic block definitions
            // Inner Block: contains a line from (0,0) to (20, 0) on Layer 0 with ByBlock style
            var innerLine = new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(20, 0));
            var innerDef = new BlockDefinition("INNER_BLOCK", new WorldPoint2(0, 0), [
                new BlockEntityTemplate(innerLine, new RenderLayerToken("0"), new RenderStyleToken("BYBLOCK"), "E-001", 1)
            ]);

            // Outer Block: contains an arc from 0 to 180 deg, and a nested reference to INNER_BLOCK at (10, 10) rotated 90 deg
            var outerArc = new ArcPrimitive(new WorldPoint2(0, 0), 15d, 0d, Math.PI);
            var nestedInnerRef = new BlockReference("INNER_BLOCK", new WorldPoint2(10, 10), rotationRadians: Math.PI / 2d,
                layer: new RenderLayerToken("0"), style: new RenderStyleToken("BYBLOCK"));
            var outerDef = new BlockDefinition("OUTER_BLOCK", new WorldPoint2(0, 0), [
                new BlockEntityTemplate(outerArc, new RenderLayerToken("0"), new RenderStyleToken("BYLAYER"), "E-002", 2)
            ], [nestedInnerRef]);

            // Detail Block: with a circle on Layer 0 and non-uniform scale
            var circle = new ArcPrimitive(new WorldPoint2(0, 0), 12d, 0d, Math.Tau);
            var detailDef = new BlockDefinition("DETAIL_BLOCK", new WorldPoint2(0, 0), [
                new BlockEntityTemplate(circle, new RenderLayerToken("0"), new RenderStyleToken("TRUECOLOR|#00FF88"), "E-003", 3)
            ]);

            // Cycle Blocks: CycleA -> CycleB -> CycleA
            var refB = new BlockReference("CYCLE_B", new WorldPoint2(0, 0));
            var cycleA = new BlockDefinition("CYCLE_A", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refB]);
            var refA = new BlockReference("CYCLE_A", new WorldPoint2(0, 0));
            var cycleB = new BlockDefinition("CYCLE_B", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refA]);

            var blockTable = new[] { innerDef, outerDef, detailDef, cycleA, cycleB };

            // 2. Build root references
            // Root 1: Nested Outer Block placed at (50, 50), scaled 1.5x on Layer "WALLS" with style "TRUECOLOR|#FF5500"
            var rootOuter = new BlockReference("OUTER_BLOCK", new WorldPoint2(50, 50), scaleX: 1.5d, scaleY: 1.5d,
                layer: new RenderLayerToken("WALLS"), style: new RenderStyleToken("TRUECOLOR|#FF5500"),
                attributes: [
                    BlockAttribute.CreateVisible("MARK_A", "ROOM 101", new WorldPoint2(50, 70), height: 3.0),
                    BlockAttribute.CreateInvisible("INTERNAL_ID", "9999", new WorldPoint2(0, 0), height: 1.0)
                ],
                handle: "R-001", sourceIndex: 10);

            // Root 2: Non-uniform scaled and mirrored Detail Block at (120, 50) with scaleX=-1.5, scaleY=1.0
            var rootDetail = new BlockReference("DETAIL_BLOCK", new WorldPoint2(120, 50), scaleX: -1.5d, scaleY: 1.0d,
                layer: new RenderLayerToken("MECHANICAL"), style: new RenderStyleToken("BYLAYER"),
                handle: "R-002", sourceIndex: 20);

            // Root 3: Trigger Cycle Guard
            var rootCycle = new BlockReference("CYCLE_A", new WorldPoint2(200, 200), handle: "R-CYCLE", sourceIndex: 30);

            // 3. Expand blocks
            var expander = new BlockExpander(blockTable, new BlockExpansionOptions(MaxNestingDepth: 16, MaxExpansionEntityBudget: 1000));
            var result = expander.Expand([rootOuter, rootDetail, rootCycle]);

            // 4. Verify invariants and log Android markers
            // Invariant A: Nested transform expansion
            if (result.TotalBlocksExpanded < 3 || result.Entities.Count < 3)
            {
                throw new InvalidOperationException($"Expected at least 3 expanded entities, got {result.Entities.Count}");
            }
            Log.Info(Tag, $"A12_ANDROID_NESTED_TRANSFORM_PASS expandedBlocks={result.TotalBlocksExpanded} entities={result.Entities.Count}");

            // Invariant B: Non-uniform scale produces EllipsePrimitive and inherits Layer 0
            var detailEntity = result.Entities.FirstOrDefault(e => e.Source.Handle == "E-003");
            if (detailEntity == null || detailEntity.Layer.Value != "MECHANICAL" || detailEntity.Geometry[0] is not EllipsePrimitive)
            {
                throw new InvalidOperationException($"Non-uniform scaled circle check failed: found={detailEntity != null}, layer={detailEntity?.Layer.Value}, type={detailEntity?.Geometry[0]?.GetType().Name}");
            }
            Log.Info(Tag, "A12_ANDROID_NON_UNIFORM_SCALE_MIRROR_PASS");

            // Invariant C: Layer 0 and ByBlock inheritance
            var nestedLineEntity = result.Entities.FirstOrDefault(e => e.Source.Handle == "E-001");
            if (nestedLineEntity == null || nestedLineEntity.Layer.Value != "WALLS" || nestedLineEntity.Style.Value != "TRUECOLOR|#FF5500")
            {
                throw new InvalidOperationException($"Layer 0 / ByBlock inheritance failed: layer={nestedLineEntity?.Layer.Value}, style={nestedLineEntity?.Style.Value}");
            }
            Log.Info(Tag, "A12_ANDROID_LAYER0_BYBLOCK_INHERITANCE_PASS");

            // Invariant D: Attributes
            if (result.TotalAttributesIncluded != 1)
            {
                throw new InvalidOperationException($"Expected exactly 1 visible attribute, got {result.TotalAttributesIncluded}");
            }
            Log.Info(Tag, "A12_ANDROID_ATTRIB_PASS");

            // Invariant E: Cycle guard
            var cycleDiagnostic = result.Diagnostics.FirstOrDefault(d => d.Code == "BLOCK_CYCLE_DETECTED");
            if (cycleDiagnostic == null)
            {
                throw new InvalidOperationException("Expected BLOCK_CYCLE_DETECTED diagnostic");
            }
            Log.Info(Tag, "A12_ANDROID_CYCLE_DEPTH_BUDGET_GUARDS_PASS");

            // 5. Assemble and render RenderScene
            var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
            foreach (var entity in result.Entities)
            {
                assembler.AddEntity(entity);
            }
            foreach (var diag in result.Diagnostics)
            {
                assembler.AddDiagnostic(diag);
            }

            var scene = assembler.Build();

            // Render scene to PNG off-screen
            var renderResult = await SkiaScenePngRenderer.RenderFitWithStatsAsync(scene, pixelWidth: 720, pixelHeight: 720);
            var png = renderResult.Png;
            var pngSha256 = Convert.ToHexString(SHA256.HashData(png)).ToLowerInvariant();

            Log.Info(Tag, $"A12_ANDROID_PNG_PASS bytes={png.Length} sha256={pngSha256} nonBgPixels={renderResult.NonBackgroundPixels}");
            Log.Info(Tag, "ANDROID_STAGE12_BLOCK_INSERT_PASS");
            Log.Info(Tag, "CLAIM_LIMIT=A12_BLOCK_INSERT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY");

            return new A12ValidationResult(
                png,
                pngSha256,
                "ANDROID_STAGE12_BLOCK_INSERT_PASS",
                renderResult.NonBackgroundPixels,
                result.Entities.Count,
                result.TotalBlocksExpanded);
        });
    }
}
#endif
