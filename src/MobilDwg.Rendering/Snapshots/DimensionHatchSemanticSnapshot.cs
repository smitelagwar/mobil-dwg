using System.Globalization;
using System.Text;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Snapshots;

public static class DimensionHatchSemanticSnapshot
{
    public const string Schema = "dim-hatch/v1";

    public static string Create(RenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"schema={Schema}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"entities={scene.Entities.Count}");

        foreach (var entity in scene.Entities.OrderBy(e => e.Id.Value, StringComparer.Ordinal))
        {
            foreach (var primitive in entity.Geometry)
            {
                if (primitive is HatchPrimitive hatch)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture,
                        $"hatch_entity={entity.Id.Value}|layer={entity.Layer.Value}|pattern={hatch.PatternName}|" +
                        $"solid={hatch.IsSolid}|loops={hatch.Loops.Count}|pattern_lines={hatch.PatternLines.Count}|" +
                        $"bounds={hatch.Bounds.MinX:F3},{hatch.Bounds.MinY:F3},{hatch.Bounds.MaxX:F3},{hatch.Bounds.MaxY:F3}");
                }
                else if (primitive is TextPrimitive text && entity.Source.EntityType == "DIMENSION")
                {
                    sb.AppendLine(CultureInfo.InvariantCulture,
                        $"dim_entity={entity.Id.Value}|layer={entity.Layer.Value}|dim_text=\"{text.Text}\"|" +
                        $"pos={text.Position.X:F3},{text.Position.Y:F3}|rot={text.RotationRadians:F3}");
                }
                else if (primitive is TextPrimitive leaderText && entity.Source.EntityType == "LEADER")
                {
                    sb.AppendLine(CultureInfo.InvariantCulture,
                        $"leader_entity={entity.Id.Value}|layer={entity.Layer.Value}|annotation=\"{leaderText.Text}\"|" +
                        $"pos={leaderText.Position.X:F3},{leaderText.Position.Y:F3}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }
}
