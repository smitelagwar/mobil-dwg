using System.Globalization;
using System.Text;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Snapshots;

public static class BlockSceneSemanticSnapshot
{
    public static string Create(BlockExpansionResult result, RenderColorContext? colorContext = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        var context = colorContext ?? RenderColorContext.Dark;

        var sb = new StringBuilder();
        sb.AppendLine("block-scene/v1");
        sb.AppendLine(CultureInfo.InvariantCulture, $"color={context.BackgroundKind}|{context.BackgroundArgb:X8}|{context.DefaultForegroundArgb:X8}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"blocks_expanded={result.TotalBlocksExpanded}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"attributes_included={result.TotalAttributesIncluded}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"entities={result.Entities.Count}");

        foreach (var entity in result.Entities)
        {
            var b = entity.Bounds;
            var primitiveType = entity.Geometry.Count > 0 ? entity.Geometry[0].GetType().Name : "None";
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"entity={entity.Id.Value}|{entity.Layer.Value}|{entity.Style.Value}|{b.MinX:F3},{b.MinY:F3},{b.MaxX:F3},{b.MaxY:F3}|{primitiveType}");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"diagnostics={result.Diagnostics.Count}");
        foreach (var diag in result.Diagnostics)
        {
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"diagnostic={diag.Kind}|{diag.Code}|{diag.EntityId?.Value ?? "none"}|{diag.Message}");
        }

        return sb.ToString().TrimEnd();
    }
}
