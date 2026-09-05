using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Styles;

public readonly record struct ResolvedStyle(
    uint ArgbColor,
    float[]? DashPatternPixels,
    float StrokeWidthPixels,
    bool IsVisible);

public static class CadStyleResolver
{
    public static ResolvedStyle Resolve(
        CadEntityStyle? entityStyle,
        RenderLayerToken layerToken,
        LayerTable layerTable,
        RenderColorContext colorContext,
        double worldUnitsPerPixel,
        double density = 1.0,
        bool displayLineweights = true,
        CadEntityStyle? blockContextStyle = null,
        List<SceneDiagnostic>? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(layerTable);
        ArgumentNullException.ThrowIfNull(colorContext);

        var effectiveStyle = entityStyle ?? CadEntityStyle.Default;

        // 1. Resolve Layer
        if (!layerTable.TryGetLayer(layerToken.Value, out var layer))
        {
            diagnostics?.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Substituted,
                "UNKNOWN_LAYER_FALLBACK",
                $"Layer '{layerToken.Value}' not found in layer table; substituted with Layer 0."));
            layer = layerTable.GetLayer("0");
        }

        // Layer visibility check
        if (!layer.IsRenderable)
        {
            return new ResolvedStyle(0, null, 0, IsVisible: false);
        }

        // 2. Resolve Color
        uint resolvedArgb = effectiveStyle.Color.Resolve(
            colorContext,
            layer.Color,
            blockContextStyle?.Color);

        // 3. Resolve Linetype
        CadLinetype linetype = effectiveStyle.Linetype.Kind switch
        {
            CadLinetypeKind.ByLayer => layer.Linetype,
            CadLinetypeKind.ByBlock => blockContextStyle?.Linetype ?? layer.Linetype,
            _ => effectiveStyle.Linetype,
        };

        if (linetype.Kind == CadLinetypeKind.ByLayer)
        {
            linetype = layer.Linetype;
        }

        if (linetype.IsComplex)
        {
            diagnostics?.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Unsupported,
                "COMPLEX_LINETYPE_FALLBACK",
                $"Complex linetype '{linetype.Name}' is not fully supported; fallen back to continuous or base dash pattern."));
        }

        float[]? dashPatternPixels = null;
        if (linetype.Pattern.Count > 0)
        {
            // Convert drawing units pattern to screen pixels based on worldUnitsPerPixel and LinetypeScale
            double ltScale = Math.Max(1e-4, effectiveStyle.LinetypeScale);
            double unitsPerPixel = Math.Max(1e-9, worldUnitsPerPixel);

            var pixelPattern = new float[linetype.Pattern.Count];

            for (int i = 0; i < linetype.Pattern.Count; i++)
            {
                double lengthInDrawingUnits = Math.Abs(linetype.Pattern[i]) * ltScale;
                double lengthInPixels = lengthInDrawingUnits / unitsPerPixel;

                // Clamp to minimum 1.5 pixel so dashes do not degenerate
                float px = (float)Math.Max(1.5, lengthInPixels);
                pixelPattern[i] = px;
            }

            dashPatternPixels = pixelPattern;
        }

        // 4. Resolve Lineweight
        CadLineweight lineweight = effectiveStyle.Lineweight.Kind switch
        {
            CadLineweightKind.ByLayer => layer.Lineweight,
            CadLineweightKind.ByBlock => blockContextStyle?.Lineweight ?? layer.Lineweight,
            _ => effectiveStyle.Lineweight,
        };

        if (lineweight.Kind == CadLineweightKind.ByLayer)
        {
            lineweight = layer.Lineweight;
        }

        float strokeWidth = lineweight.ToPixels(density, displayLineweights);

        return new ResolvedStyle(resolvedArgb, dashPatternPixels, strokeWidth, IsVisible: true);
    }
}
