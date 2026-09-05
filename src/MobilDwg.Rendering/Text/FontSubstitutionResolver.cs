using System.Collections.ObjectModel;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Scene;
using SkiaSharp;

namespace MobilDwg.Rendering.Text;

public static class FontSubstitutionResolver
{
    public const string DefaultFontFamily = "sans-serif";
    public const string MonospaceFontFamily = "monospace";
    public const string SerifFontFamily = "serif";

    private static readonly Dictionary<string, string> SubstitutionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // AutoCAD standard SHX fonts
        { "txt.shx", DefaultFontFamily },
        { "txt", DefaultFontFamily },
        { "monotxt.shx", MonospaceFontFamily },
        { "monotxt", MonospaceFontFamily },
        { "romans.shx", DefaultFontFamily },
        { "romans", DefaultFontFamily },
        { "romand.shx", DefaultFontFamily },
        { "romand", DefaultFontFamily },
        { "romant.shx", DefaultFontFamily },
        { "romant", DefaultFontFamily },
        { "simplex.shx", DefaultFontFamily },
        { "simplex", DefaultFontFamily },
        { "complex.shx", SerifFontFamily },
        { "complex", SerifFontFamily },
        { "italic.shx", DefaultFontFamily },
        { "italic", DefaultFontFamily },
        { "italicc.shx", DefaultFontFamily },
        { "italicc", DefaultFontFamily },
        { "isocp.shx", DefaultFontFamily },
        { "isocp", DefaultFontFamily },
        { "isocp2.shx", DefaultFontFamily },
        { "isocp3.shx", DefaultFontFamily },
        { "isoct.shx", DefaultFontFamily },
        { "isoct", DefaultFontFamily },
        { "isoct2.shx", DefaultFontFamily },
        { "isoct3.shx", DefaultFontFamily },
        { "gothice.shx", DefaultFontFamily },
        { "gothicg.shx", DefaultFontFamily },
        { "gothici.shx", DefaultFontFamily },
        { "syastro.shx", DefaultFontFamily },
        { "symap.shx", DefaultFontFamily },
        { "symath.shx", DefaultFontFamily },
        { "symeteo.shx", DefaultFontFamily },
        { "symusic.shx", DefaultFontFamily },
        { "STANDARD", DefaultFontFamily },

        // Common TTF fonts mapped to system families
        { "arial.ttf", DefaultFontFamily },
        { "arial", DefaultFontFamily },
        { "times.ttf", SerifFontFamily },
        { "times new roman", SerifFontFamily },
        { "times", SerifFontFamily },
        { "courier.ttf", MonospaceFontFamily },
        { "courier new", MonospaceFontFamily },
        { "courier", MonospaceFontFamily },
        { "tahoma.ttf", DefaultFontFamily },
        { "tahoma", DefaultFontFamily },
        { "verdana.ttf", DefaultFontFamily },
        { "verdana", DefaultFontFamily },
        { "calibri.ttf", DefaultFontFamily },
        { "calibri", DefaultFontFamily },
    };

    public static string Resolve(
        string? requestedFont,
        ICollection<SceneDiagnostic>? diagnostics = null,
        RenderEntityId? entityId = null)
    {
        if (string.IsNullOrWhiteSpace(requestedFont))
        {
            return DefaultFontFamily;
        }

        var normalized = Path.GetFileName(requestedFont.Trim());

        if (SubstitutionMap.TryGetValue(normalized, out var mapped))
        {
            if (normalized.EndsWith(".shx", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(normalized, mapped, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics?.Add(new SceneDiagnostic(
                    SceneDiagnosticKind.Substituted,
                    "FONT_SUBSTITUTION",
                    $"AutoCAD font '{requestedFont}' substituted with standard system font '{mapped}'.",
                    entityId));
            }
            return mapped;
        }

        // Unknown font fallback
        diagnostics?.Add(new SceneDiagnostic(
            SceneDiagnosticKind.Substituted,
            "FONT_SUBSTITUTION",
            $"Unknown font '{requestedFont}' substituted with fallback '{DefaultFontFamily}'.",
            entityId));

        return DefaultFontFamily;
    }

    public static SKTypeface GetSkiaTypeface(string resolvedFamily)
    {
        return SKTypeface.FromFamilyName(resolvedFamily) ?? SKTypeface.Default;
    }
}
