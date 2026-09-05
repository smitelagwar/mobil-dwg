namespace MobilDwg.Rendering.Styles;

public enum CadLineweightKind
{
    ByLayer = 0,
    ByBlock = 1,
    Default = 2,
    Exact = 3,
}

public readonly record struct CadLineweight
{
    public CadLineweightKind Kind { get; }
    public int ValueInHundredthsOfMm { get; }

    public static CadLineweight ByLayer { get; } = new(CadLineweightKind.ByLayer, -1);
    public static CadLineweight ByBlock { get; } = new(CadLineweightKind.ByBlock, -2);
    public static CadLineweight Default { get; } = new(CadLineweightKind.Default, 25); // Default 0.25mm

    private CadLineweight(CadLineweightKind kind, int valueInHundredthsOfMm)
    {
        Kind = kind;
        ValueInHundredthsOfMm = valueInHundredthsOfMm;
    }

    public static CadLineweight FromHundredthsOfMm(int value)
    {
        if (value == -1) return ByLayer;
        if (value == -2) return ByBlock;
        if (value == -3) return Default;
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Lineweight value cannot be negative.");

        return new CadLineweight(CadLineweightKind.Exact, value);
    }

    public static CadLineweight FromMm(double millimeters)
    {
        if (millimeters < 0) throw new ArgumentOutOfRangeException(nameof(millimeters), "Lineweight millimeters cannot be negative.");
        int hundredths = (int)Math.Round(millimeters * 100.0);
        return new CadLineweight(CadLineweightKind.Exact, hundredths);
    }

    public float ToPixels(double density = 1.0, bool displayLineweights = true)
    {
        if (!displayLineweights)
        {
            return 1.0f;
        }

        int hundredths = Kind switch
        {
            CadLineweightKind.Default => 25,
            CadLineweightKind.Exact => ValueInHundredthsOfMm,
            _ => 25,
        };

        if (hundredths <= 0)
        {
            return 1.0f;
        }

        // Millimeters to pixels: mm * (96 DPI / 25.4 mm/inch) * density
        double mm = hundredths / 100.0;
        double px = mm * (96.0 / 25.4) * density;
        return (float)Math.Max(1.0, px);
    }

    public override string ToString() => Kind switch
    {
        CadLineweightKind.ByLayer => "BYLAYER",
        CadLineweightKind.ByBlock => "BYBLOCK",
        CadLineweightKind.Default => "DEFAULT",
        CadLineweightKind.Exact => (ValueInHundredthsOfMm / 100.0).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "mm",
        _ => "UNKNOWN",
    };
}
