using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Styles;

public enum CadColorKind
{
    ByLayer = 0,
    ByBlock = 1,
    Aci = 2,
    TrueColor = 3,
}

public readonly record struct CadColor
{
    private static readonly uint[] s_aciPalette = InitializeAciPalette();

    public CadColorKind Kind { get; }
    public int AciIndex { get; }
    public uint Argb { get; }

    public static CadColor ByLayer { get; } = new(CadColorKind.ByLayer, 256, 0);
    public static CadColor ByBlock { get; } = new(CadColorKind.ByBlock, 0, 0);

    private CadColor(CadColorKind kind, int aciIndex, uint argb)
    {
        Kind = kind;
        AciIndex = aciIndex;
        Argb = argb;
    }

    public static CadColor FromAci(int aciIndex)
    {
        if (aciIndex is < 0 or > 256)
            throw new ArgumentOutOfRangeException(nameof(aciIndex), "ACI color index must be between 0 and 256.");

        if (aciIndex == 0) return ByBlock;
        if (aciIndex == 256) return ByLayer;

        return new CadColor(CadColorKind.Aci, aciIndex, s_aciPalette[aciIndex]);
    }

    public static CadColor FromRgb(byte r, byte g, byte b)
    {
        uint argb = 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b;
        return new CadColor(CadColorKind.TrueColor, -1, argb);
    }

    public static CadColor FromArgb(uint argb)
    {
        return new CadColor(CadColorKind.TrueColor, -1, argb | 0xFF000000u);
    }

    public uint Resolve(
        RenderColorContext colorContext,
        CadColor? layerColor = null,
        CadColor? blockColor = null)
    {
        ArgumentNullException.ThrowIfNull(colorContext);

        switch (Kind)
        {
            case CadColorKind.ByLayer:
                if (layerColor.HasValue && layerColor.Value.Kind != CadColorKind.ByLayer)
                {
                    return layerColor.Value.Resolve(colorContext, null, blockColor);
                }
                return colorContext.DefaultForegroundArgb;

            case CadColorKind.ByBlock:
                if (blockColor.HasValue && blockColor.Value.Kind != CadColorKind.ByBlock)
                {
                    return blockColor.Value.Resolve(colorContext, layerColor, null);
                }
                return colorContext.DefaultForegroundArgb;

            case CadColorKind.Aci:
                // ACI 7 (White/Black) is a special dynamic contrast color in AutoCAD
                if (AciIndex == 7)
                {
                    return colorContext.BackgroundKind == RenderBackgroundKind.Light
                        ? 0xFF000000u // Black on light background
                        : 0xFFFFFFFFu; // White on dark background
                }
                return s_aciPalette[AciIndex];

            case CadColorKind.TrueColor:
                return Argb;

            default:
                return colorContext.DefaultForegroundArgb;
        }
    }

    public override string ToString() => Kind switch
    {
        CadColorKind.ByLayer => "BYLAYER",
        CadColorKind.ByBlock => "BYBLOCK",
        CadColorKind.Aci => $"ACI:{AciIndex}",
        CadColorKind.TrueColor => $"#{Argb:X8}",
        _ => "UNKNOWN",
    };

    private static uint[] InitializeAciPalette()
    {
        var p = new uint[257];
        p[0] = 0xFF000000u; // ByBlock
        p[1] = 0xFFFF0000u; // Red
        p[2] = 0xFFFFFF00u; // Yellow
        p[3] = 0xFF00FF00u; // Green
        p[4] = 0xFF00FFFFu; // Cyan
        p[5] = 0xFF0000FFu; // Blue
        p[6] = 0xFFFF00FFu; // Magenta
        p[7] = 0xFFFFFFFFu; // White / Black contrast
        p[8] = 0xFF808080u; // Dark Gray
        p[9] = 0xFFC0C0C0u; // Light Gray

        // Standard AutoCAD color wheel generator for indices 10 to 249
        for (int i = 10; i < 250; i++)
        {
            int hueIndex = (i - 10) / 10; // 0..23 (each 15 degrees)
            int shadeIndex = (i - 10) % 10; // 0..9
            double hue = hueIndex * 15.0; // degrees

            double lightness = shadeIndex switch
            {
                0 => 1.0,
                1 => 0.9,
                2 => 0.8,
                3 => 0.7,
                4 => 0.6,
                5 => 0.5,
                6 => 0.4,
                7 => 0.3,
                8 => 0.2,
                9 => 0.1,
                _ => 0.5,
            };

            p[i] = HsvToRgb(hue, 1.0, lightness);
        }

        // Grayscale ramp for indices 250 to 255
        p[250] = 0xFF333333u;
        p[251] = 0xFF505050u;
        p[252] = 0xFF696969u;
        p[253] = 0xFF828282u;
        p[254] = 0xFFBEBEBEu;
        p[255] = 0xFFFFFFFFu;
        p[256] = 0xFFFFFFFFu; // ByLayer

        return p;
    }

    private static uint HsvToRgb(double hue, double sat, double val)
    {
        double c = val * sat;
        double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
        double m = val - c;

        double r1 = 0, g1 = 0, b1 = 0;
        if (hue < 60) { r1 = c; g1 = x; }
        else if (hue < 120) { r1 = x; g1 = c; }
        else if (hue < 180) { g1 = c; b1 = x; }
        else if (hue < 240) { g1 = x; b1 = c; }
        else if (hue < 300) { r1 = x; b1 = c; }
        else { r1 = c; b1 = x; }

        byte r = (byte)Math.Clamp((int)((r1 + m) * 255), 0, 255);
        byte g = (byte)Math.Clamp((int)((g1 + m) * 255), 0, 255);
        byte b = (byte)Math.Clamp((int)((b1 + m) * 255), 0, 255);

        return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b;
    }
}
