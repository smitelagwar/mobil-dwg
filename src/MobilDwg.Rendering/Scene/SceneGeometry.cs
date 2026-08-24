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
    public WorldPoint2 Center => new((MinX + MaxX) / 2d, (MinY + MaxY) / 2d);

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

public sealed record RenderSourceReference(
    string EntityType,
    string? Handle = null,
    int? SourceIndex = null)
{
    public string EntityType { get; init; } = string.IsNullOrWhiteSpace(EntityType)
        ? throw new ArgumentException("Entity type is required.", nameof(EntityType))
        : EntityType;
}

public sealed record RenderSceneEntity(
    RenderEntityId Id,
    WorldBounds2 Bounds,
    RenderLayerToken Layer,
    RenderStyleToken Style,
    RenderSourceReference Source);
