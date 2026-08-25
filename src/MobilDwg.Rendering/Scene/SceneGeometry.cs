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
    public RenderSceneEntity(
        RenderEntityId id,
        WorldBounds2 bounds,
        RenderLayerToken layer,
        RenderStyleToken style,
        RenderSourceReference source)
    {
        // record structs can be default-constructed without invoking their validating constructors.
        // Enforce the invariant again at the immutable scene boundary.
        if (string.IsNullOrWhiteSpace(id.Value)) throw new ArgumentException("Stable entity ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(layer.Value)) throw new ArgumentException("Layer token is required.", nameof(layer));
        if (string.IsNullOrWhiteSpace(style.Value)) throw new ArgumentException("Style token is required.", nameof(style));
        ArgumentNullException.ThrowIfNull(source);

        Id = id;
        Bounds = bounds;
        Layer = layer;
        Style = style;
        Source = source;
    }

    public RenderEntityId Id { get; }
    public WorldBounds2 Bounds { get; }
    public RenderLayerToken Layer { get; }
    public RenderStyleToken Style { get; }
    public RenderSourceReference Source { get; }
}
