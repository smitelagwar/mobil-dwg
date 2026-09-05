using System.Collections.ObjectModel;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Blocks;

public sealed record BlockEntityTemplate(
    RenderGeometryPrimitive Primitive,
    RenderLayerToken Layer,
    RenderStyleToken Style,
    string? Handle = null,
    int? SourceIndex = null);

public sealed class BlockDefinition
{
    private readonly ReadOnlyCollection<BlockEntityTemplate> _entities;
    private readonly ReadOnlyCollection<BlockReference> _nestedReferences;

    public BlockDefinition(
        string name,
        WorldPoint2 basePoint,
        IEnumerable<BlockEntityTemplate> entities,
        IEnumerable<BlockReference>? nestedReferences = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Block name is required.", nameof(name));
        ArgumentNullException.ThrowIfNull(entities);

        Name = name;
        BasePoint = basePoint;
        _entities = Array.AsReadOnly(entities.ToArray());
        _nestedReferences = Array.AsReadOnly(nestedReferences?.ToArray() ?? Array.Empty<BlockReference>());
    }

    public string Name { get; }
    public WorldPoint2 BasePoint { get; }
    public IReadOnlyList<BlockEntityTemplate> Entities => _entities;
    public IReadOnlyList<BlockReference> NestedReferences => _nestedReferences;
}
