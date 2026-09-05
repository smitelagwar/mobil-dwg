using System.Globalization;
using System.Text;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Snapshots;

public static class TextSceneSemanticSnapshot
{
    public const string Schema = "text-scene/v1";

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
                if (primitive is TextPrimitive text)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture,
                        $"text_entity={entity.Id.Value}|layer={entity.Layer.Value}|text=\"{Escape(text.Text)}\"|" +
                        $"pos={text.Position.X:F3},{text.Position.Y:F3}|h={text.Height:F3}|rot={text.RotationRadians:F3}|" +
                        $"align={text.HorizontalAlignment}:{text.VerticalAlignment}|mirror={text.MirrorFlags}|" +
                        $"font={text.RequestedFont}->{text.ResolvedFont}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string Escape(string text) =>
        text.Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", string.Empty)
            .Replace("\"", "\\\"");
}
