using System.Text;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Snapshots;

public static class LayerStyleSemanticSnapshot
{
    public static string Create(RenderScene scene, double worldUnitsPerPixel = 1.0, double density = 1.0)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        sb.AppendLine("format=layer-style/v1");

        // Layers in deterministic sorted order
        foreach (var layer in scene.LayerTable.Layers)
        {
            string vis = layer.IsVisible ? "VISIBLE" : "HIDDEN";
            string frz = layer.IsFrozen ? "FROZEN" : "THAWED";
            sb.AppendLine($"layer={layer.Name}|{vis}|{frz}|{layer.Color}|{layer.Linetype.Name}|{layer.Lineweight}");
        }

        // Entities with resolved style
        foreach (var entity in scene.Entities)
        {
            var resolved = CadStyleResolver.Resolve(
                entity.CadStyle,
                entity.Layer,
                scene.LayerTable,
                scene.ColorContext,
                worldUnitsPerPixel,
                density);

            string vis = resolved.IsVisible ? "VISIBLE" : "HIDDEN";
            string pattern = resolved.DashPatternPixels is { Length: > 0 } ? "DASHED" : "SOLID";
            sb.AppendLine($"resolved={entity.Id.Value}|{entity.Layer.Value}|#{resolved.ArgbColor:X8}|{pattern}|{resolved.StrokeWidthPixels.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}px|{vis}");
        }

        return sb.ToString().TrimEnd();
    }
}
