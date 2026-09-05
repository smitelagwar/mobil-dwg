using System.Globalization;
using System.Text;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Snapshots;

public static class LayoutSceneSemanticSnapshot
{
    public const string Schema = "layout-scene/v1";

    public static string Create(CadLayoutManager layoutManager)
    {
        ArgumentNullException.ThrowIfNull(layoutManager);

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"schema={Schema}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"active_layout={layoutManager.ActiveLayoutName}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"layouts_count={layoutManager.Layouts.Count}");

        foreach (var layout in layoutManager.Layouts.OrderBy(l => l.TabOrder).ThenBy(l => l.Name, StringComparer.Ordinal))
        {
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"layout={layout.Name}|model={layout.IsModelSpace}|order={layout.TabOrder}|" +
                $"bounds={layout.PaperBounds.MinX:F2},{layout.PaperBounds.MinY:F2},{layout.PaperBounds.MaxX:F2},{layout.PaperBounds.MaxY:F2}|" +
                $"paper_entities={layout.PaperEntities.Count}|viewports={layout.Viewports.Count}");

            foreach (var vp in layout.Viewports.OrderBy(v => v.ViewportId, StringComparer.Ordinal))
            {
                var frozenStr = string.Join(",", vp.FrozenLayers.OrderBy(f => f, StringComparer.Ordinal));
                sb.AppendLine(CultureInfo.InvariantCulture,
                    $"  viewport={vp.ViewportId}|center={vp.PaperCenter.X:F2},{vp.PaperCenter.Y:F2}|" +
                    $"size={vp.PaperWidth:F2}x{vp.PaperHeight:F2}|view_center={vp.ViewCenter.X:F2},{vp.ViewCenter.Y:F2}|" +
                    $"view_height={vp.ViewHeight:F2}|twist={vp.TwistAngleRadians:F3}|frozen=[{frozenStr}]|active={vp.IsActive}");
            }
        }

        var scene = layoutManager.ComposeActiveScene();
        sb.AppendLine(CultureInfo.InvariantCulture, $"composed_entities={scene.Entities.Count}");
        if (scene.WorldBounds is { } b)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"composed_bounds={b.MinX:F2},{b.MinY:F2},{b.MaxX:F2},{b.MaxY:F2}");
        }

        return sb.ToString().TrimEnd();
    }
}
