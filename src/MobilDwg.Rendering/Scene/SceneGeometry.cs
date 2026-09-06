using System.Collections.ObjectModel;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Scene;

public readonly record struct WorldPoint2
{
    public WorldPoint2(double x, double y)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        X = x;
        Y = y;
    }

    public double X { get; }
    public double Y { get; }
}

public readonly record struct WorldPoint3
{
    public WorldPoint3(double x, double y, double z)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        if (!double.IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(z));
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
}

public readonly record struct WorldBounds2
{
    public WorldBounds2(double minX, double minY, double maxX, double maxY)
    {
        if (!double.IsFinite(minX)) throw new ArgumentOutOfRangeException(nameof(minX));
        if (!double.IsFinite(minY)) throw new ArgumentOutOfRangeException(nameof(minY));
        if (!double.IsFinite(maxX)) throw new ArgumentOutOfRangeException(nameof(maxX));
        if (!double.IsFinite(maxY)) throw new ArgumentOutOfRangeException(nameof(maxY));
        if (maxX < minX) throw new ArgumentOutOfRangeException(nameof(maxX));
        if (maxY < minY) throw new ArgumentOutOfRangeException(nameof(maxY));

        var width = maxX - minX;
        var height = maxY - minY;
        if (!double.IsFinite(width)) throw new ArgumentOutOfRangeException(nameof(maxX), "Bounds width must remain finite.");
        if (!double.IsFinite(height)) throw new ArgumentOutOfRangeException(nameof(maxY), "Bounds height must remain finite.");

        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public double MinX { get; }
    public double MinY { get; }
    public double MaxX { get; }
    public double MaxY { get; }
    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;

    // Half-before-add avoids overflow when two large same-sign finite coordinates are averaged.
    public WorldPoint2 Center => new((MinX / 2d) + (MaxX / 2d), (MinY / 2d) + (MaxY / 2d));

    public WorldBounds2 Union(WorldBounds2 other) => new(
        Math.Min(MinX, other.MinX),
        Math.Min(MinY, other.MinY),
        Math.Max(MaxX, other.MaxX),
        Math.Max(MaxY, other.MaxY));

    public bool Intersects(WorldBounds2 other) =>
        MinX <= other.MaxX && MaxX >= other.MinX &&
        MinY <= other.MaxY && MaxY >= other.MinY;

    public bool Contains(WorldPoint2 point) =>
        point.X >= MinX && point.X <= MaxX &&
        point.Y >= MinY && point.Y <= MaxY;

    public bool Contains(WorldBounds2 other) =>
        other.MinX >= MinX && other.MaxX <= MaxX &&
        other.MinY >= MinY && other.MaxY <= MaxY;
}

public readonly record struct RenderEntityId
{
    public RenderEntityId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Stable entity ID is required.", nameof(value));
        Value = value;
    }

    public string Value { get; }
    public override string ToString() => Value;
}

public readonly record struct RenderLayerToken
{
    public RenderLayerToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Layer token is required.", nameof(value));
        Value = value;
    }

    public string Value { get; }
    public override string ToString() => Value;
}

public readonly record struct RenderStyleToken
{
    public RenderStyleToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Style token is required.", nameof(value));
        Value = value;
    }

    public string Value { get; }
    public override string ToString() => Value;
}

public sealed record RenderSourceReference
{
    public RenderSourceReference(string entityType, string? handle = null, int? sourceIndex = null)
    {
        if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (handle is not null && string.IsNullOrWhiteSpace(handle)) throw new ArgumentException("Handle cannot be blank when supplied.", nameof(handle));
        if (sourceIndex is < 0) throw new ArgumentOutOfRangeException(nameof(sourceIndex));

        EntityType = entityType;
        Handle = handle;
        SourceIndex = sourceIndex;
    }

    public string EntityType { get; }
    public string? Handle { get; }
    public int? SourceIndex { get; }
}

public sealed record RenderSceneEntity
{
    private readonly ReadOnlyCollection<RenderGeometryPrimitive> _geometry;

    public RenderSceneEntity(
        RenderEntityId id,
        WorldBounds2 bounds,
        RenderLayerToken layer,
        RenderStyleToken style,
        RenderSourceReference source)
        : this(id, bounds, layer, style, source, Array.Empty<RenderGeometryPrimitive>(), null)
    {
    }

    public RenderSceneEntity(
        RenderEntityId id,
        WorldBounds2 bounds,
        RenderLayerToken layer,
        RenderStyleToken style,
        RenderSourceReference source,
        IEnumerable<RenderGeometryPrimitive> geometry)
        : this(id, bounds, layer, style, source, geometry, null)
    {
    }

    public RenderSceneEntity(
        RenderEntityId id,
        WorldBounds2 bounds,
        RenderLayerToken layer,
        RenderStyleToken style,
        RenderSourceReference source,
        IEnumerable<RenderGeometryPrimitive> geometry,
        CadEntityStyle? cadStyle)
    {
        if (string.IsNullOrWhiteSpace(id.Value)) throw new ArgumentException("Stable entity ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(layer.Value)) throw new ArgumentException("Layer token is required.", nameof(layer));
        if (string.IsNullOrWhiteSpace(style.Value)) throw new ArgumentException("Style token is required.", nameof(style));
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(geometry);

        var geometryCopy = geometry.ToArray();
        if (geometryCopy.Any(item => item is null)) throw new ArgumentException("Geometry collection cannot contain null items.", nameof(geometry));

        Id = id;
        Bounds = bounds;
        Layer = layer;
        Style = style;
        Source = source;
        CadStyle = cadStyle;
        _geometry = Array.AsReadOnly(geometryCopy);
    }

    public RenderSceneEntity(
        RenderEntityId id,
        RenderLayerToken layer,
        RenderStyleToken style,
        RenderSourceReference source,
        IEnumerable<RenderGeometryPrimitive> geometry,
        CadEntityStyle? cadStyle = null)
        : this(id, CalculateGeometryBounds(geometry), layer, style, source, geometry, cadStyle)
    {
    }

    public RenderEntityId Id { get; }
    public WorldBounds2 Bounds { get; }
    public RenderLayerToken Layer { get; }
    public RenderStyleToken Style { get; }
    public RenderSourceReference Source { get; }
    public CadEntityStyle? CadStyle { get; }
    public IReadOnlyList<RenderGeometryPrimitive> Geometry => _geometry;

    private static WorldBounds2 CalculateGeometryBounds(IEnumerable<RenderGeometryPrimitive> geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        var copy = geometry.ToArray();
        if (copy.Length == 0) throw new ArgumentException("At least one geometry primitive is required when bounds are inferred.", nameof(geometry));
        if (copy.Any(item => item is null)) throw new ArgumentException("Geometry collection cannot contain null items.", nameof(geometry));

        var bounds = copy[0].Bounds;
        for (var i = 1; i < copy.Length; i++) bounds = bounds.Union(copy[i].Bounds);
        return bounds;
    }
}
