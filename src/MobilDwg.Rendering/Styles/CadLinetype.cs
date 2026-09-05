namespace MobilDwg.Rendering.Styles;

public enum CadLinetypeKind
{
    Continuous = 0,
    ByLayer = 1,
    ByBlock = 2,
    Pattern = 3,
    Complex = 4,
}

public sealed record CadLinetype
{
    public string Name { get; }
    public string Description { get; }
    public CadLinetypeKind Kind { get; }
    public IReadOnlyList<float> Pattern { get; }
    public bool IsComplex { get; }

    public static CadLinetype Continuous { get; } = new("CONTINUOUS", "Solid line", CadLinetypeKind.Continuous, Array.Empty<float>());
    public static CadLinetype ByLayer { get; } = new("BYLAYER", "Inherited from layer", CadLinetypeKind.ByLayer, Array.Empty<float>());
    public static CadLinetype ByBlock { get; } = new("BYBLOCK", "Inherited from block", CadLinetypeKind.ByBlock, Array.Empty<float>());

    public static CadLinetype Dashed { get; } = new("DASHED", "Dashed __ __ __ __", CadLinetypeKind.Pattern, new[] { 12.7f, -6.35f });
    public static CadLinetype Hidden { get; } = new("HIDDEN", "Hidden _ _ _ _ _ _", CadLinetypeKind.Pattern, new[] { 6.35f, -3.175f });
    public static CadLinetype Center { get; } = new("CENTER", "Center ____ _ ____ _", CadLinetypeKind.Pattern, new[] { 31.75f, -6.35f, 6.35f, -6.35f });
    public static CadLinetype Dot { get; } = new("DOT", "Dot . . . . . . .", CadLinetypeKind.Pattern, new[] { 0.5f, -4.0f });
    public static CadLinetype DashDot { get; } = new("DASHDOT", "DashDot __ . __ .", CadLinetypeKind.Pattern, new[] { 12.7f, -6.35f, 0.5f, -6.35f });
    public static CadLinetype Phantom { get; } = new("PHANTOM", "Phantom ___ _ _ ___", CadLinetypeKind.Pattern, new[] { 31.75f, -6.35f, 6.35f, -6.35f, 6.35f, -6.35f });

    public CadLinetype(
        string name,
        string description,
        CadLinetypeKind kind,
        IEnumerable<float> pattern,
        bool isComplex = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Linetype name is required.", nameof(name));

        Name = name.ToUpperInvariant();
        Description = description ?? string.Empty;
        Kind = kind;
        Pattern = pattern?.ToArray() ?? Array.Empty<float>();
        IsComplex = isComplex;
    }

    public static CadLinetype CreatePattern(string name, string description, IEnumerable<float> pattern)
    {
        return new CadLinetype(name, description, CadLinetypeKind.Pattern, pattern);
    }

    public static CadLinetype CreateComplex(string name, string description, IEnumerable<float>? fallbackPattern = null)
    {
        return new CadLinetype(name, description, CadLinetypeKind.Complex, fallbackPattern ?? Array.Empty<float>(), isComplex: true);
    }

    public override string ToString() => Name;
}
