using System.Collections.ObjectModel;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Transforms;

namespace MobilDwg.Rendering.Blocks;

public sealed record BlockExpansionOptions(
    int MaxNestingDepth = 32,
    int MaxExpansionEntityBudget = 50000);

public sealed record BlockExpansionResult(
    IReadOnlyList<RenderSceneEntity> Entities,
    IReadOnlyList<SceneDiagnostic> Diagnostics,
    int TotalBlocksExpanded,
    int TotalAttributesIncluded);

public sealed class BlockExpander
{
    private readonly IReadOnlyDictionary<string, BlockDefinition> _blockTable;
    private readonly BlockExpansionOptions _options;
    private readonly List<RenderSceneEntity> _expanded = new();
    private readonly List<SceneDiagnostic> _diagnostics = new();
    private readonly HashSet<string> _activePath = new(StringComparer.OrdinalIgnoreCase);
    private int _blocksExpanded;
    private int _attributesIncluded;
    private int _entityCounter;

    public BlockExpander(
        IEnumerable<BlockDefinition> blockDefinitions,
        BlockExpansionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(blockDefinitions);
        _blockTable = blockDefinitions.ToDictionary(b => b.Name, b => b, StringComparer.OrdinalIgnoreCase);
        _options = options ?? new BlockExpansionOptions();
    }

    public BlockExpansionResult Expand(IEnumerable<BlockReference> blockReferences)
    {
        ArgumentNullException.ThrowIfNull(blockReferences);

        foreach (var reference in blockReferences)
        {
            ExpandReference(reference, Transform2D.Identity, reference.Layer, reference.Style, depth: 0);
        }

        return new BlockExpansionResult(
            Array.AsReadOnly(_expanded.ToArray()),
            Array.AsReadOnly(_diagnostics.ToArray()),
            _blocksExpanded,
            _attributesIncluded);
    }

    private void ExpandReference(
        BlockReference reference,
        in Transform2D parentTransform,
        RenderLayerToken inheritedLayer,
        RenderStyleToken inheritedStyle,
        int depth)
    {
        if (_expanded.Count >= _options.MaxExpansionEntityBudget)
        {
            _diagnostics.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Dropped,
                "BLOCK_EXPANSION_BUDGET_EXCEEDED",
                $"Block expansion budget ({_options.MaxExpansionEntityBudget}) reached.",
                new RenderEntityId(reference.Handle ?? reference.BlockName)));
            return;
        }

        if (depth >= _options.MaxNestingDepth)
        {
            _diagnostics.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Dropped,
                "BLOCK_DEPTH_EXCEEDED",
                $"Maximum block nesting depth ({_options.MaxNestingDepth}) exceeded at '{reference.BlockName}'.",
                new RenderEntityId(reference.Handle ?? reference.BlockName)));
            return;
        }

        if (!_blockTable.TryGetValue(reference.BlockName, out var blockDef))
        {
            _diagnostics.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Unsupported,
                "BLOCK_DEFINITION_MISSING",
                $"Referenced block '{reference.BlockName}' is not defined.",
                new RenderEntityId(reference.Handle ?? reference.BlockName)));
            return;
        }

        if (!_activePath.Add(reference.BlockName))
        {
            _diagnostics.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Dropped,
                "BLOCK_CYCLE_DETECTED",
                $"Circular block reference detected for '{reference.BlockName}'.",
                new RenderEntityId(reference.Handle ?? reference.BlockName)));
            return;
        }

        _blocksExpanded++;

        try
        {
            var localTransform = Transform2D.CreateBlockTransform(
                reference.InsertionPoint,
                reference.ScaleX,
                reference.ScaleY,
                reference.RotationRadians,
                blockDef.BasePoint);

            var effectiveTransform = parentTransform * localTransform;

            // Effective layer for Layer 0 inheritance:
            // If the reference specifies a layer other than 0, that becomes the container layer.
            var containerLayer = string.Equals(reference.Layer.Value, "0", StringComparison.OrdinalIgnoreCase)
                ? inheritedLayer
                : reference.Layer;

            // Effective style for ByBlock inheritance:
            // If the reference specifies a style other than BYBLOCK, that becomes the container style.
            var containerStyle = string.Equals(reference.Style.Value, "BYBLOCK", StringComparison.OrdinalIgnoreCase)
                ? inheritedStyle
                : reference.Style;

            // 1. Expand template entities in this block
            foreach (var template in blockDef.Entities)
            {
                if (_expanded.Count >= _options.MaxExpansionEntityBudget)
                {
                    _diagnostics.Add(new SceneDiagnostic(
                        SceneDiagnosticKind.Dropped,
                        "BLOCK_EXPANSION_BUDGET_EXCEEDED",
                        $"Block expansion budget reached.",
                        new RenderEntityId(template.Handle ?? reference.BlockName)));
                    break;
                }

                // Layer 0 inheritance rule:
                // An entity inside a block on Layer "0" adopts the container layer.
                var effectiveEntityLayer = string.Equals(template.Layer.Value, "0", StringComparison.OrdinalIgnoreCase)
                    ? containerLayer
                    : template.Layer;

                // ByBlock style inheritance rule:
                // An entity inside a block with ByBlock style adopts the container style.
                var effectiveEntityStyle = string.Equals(template.Style.Value, "BYBLOCK", StringComparison.OrdinalIgnoreCase)
                    ? containerStyle
                    : template.Style;

                // Transform primitive
                var transformedPrimitive = PrimitiveTransformer.Transform(template.Primitive, effectiveTransform);

                var id = new RenderEntityId($"BLK-{reference.BlockName}-{++_entityCounter:D5}");
                var entity = new RenderSceneEntity(
                    id,
                    effectiveEntityLayer,
                    effectiveEntityStyle,
                    new RenderSourceReference("INSERT_PRIMITIVE", template.Handle, template.SourceIndex ?? reference.SourceIndex),
                    new[] { transformedPrimitive });

                _expanded.Add(entity);
            }

            // 2. Expand visible attributes attached to this reference
            foreach (var attr in reference.Attributes)
            {
                if (attr.IsInvisible) continue;

                var transformedPos = parentTransform.TransformPoint(attr.Position);
                _attributesIncluded++;

                // Attribute text represented as marker/point geometry
                var attrPoint = new PointPrimitive(transformedPos);
                var id = new RenderEntityId($"ATTRIB-{attr.Tag}-{++_entityCounter:D5}");
                var entity = new RenderSceneEntity(
                    id,
                    containerLayer,
                    containerStyle,
                    new RenderSourceReference("ATTRIB", reference.Handle, reference.SourceIndex),
                    new[] { attrPoint });

                _expanded.Add(entity);
            }

            // 3. Expand nested block references
            foreach (var nestedRef in blockDef.NestedReferences)
            {
                ExpandReference(nestedRef, effectiveTransform, containerLayer, containerStyle, depth + 1);
            }
        }
        finally
        {
            _activePath.Remove(reference.BlockName);
        }
    }
}
