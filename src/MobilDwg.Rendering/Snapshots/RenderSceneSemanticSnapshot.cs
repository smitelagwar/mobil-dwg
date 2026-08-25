using System.Globalization;
using System.Text;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Snapshots;

public static class RenderSceneSemanticSnapshot
{
    public static string Create(RenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        var sb = new StringBuilder();
        sb.AppendLine("render-scene/v1");
        sb.Append("color=")
            .Append(scene.ColorContext.BackgroundKind)
            .Append('|').Append(scene.ColorContext.BackgroundArgb.ToString("X8", CultureInfo.InvariantCulture))
            .Append('|').Append(scene.ColorContext.DefaultForegroundArgb.ToString("X8", CultureInfo.InvariantCulture))
            .AppendLine();

        if (scene.WorldBounds is { } bounds)
        {
            sb.Append("bounds=")
                .Append(F(bounds.MinX)).Append(',').Append(F(bounds.MinY)).Append(',')
                .Append(F(bounds.MaxX)).Append(',').Append(F(bounds.MaxY)).AppendLine();
        }
        else
        {
            sb.AppendLine("bounds=empty");
        }

        sb.Append("entities=").Append(scene.Entities.Count).AppendLine();
        foreach (var entity in scene.Entities.OrderBy(x => x.Id.Value, StringComparer.Ordinal))
        {
            sb.Append("entity=")
                .Append(E(entity.Id.Value)).Append('|')
                .Append(E(entity.Layer.Value)).Append('|')
                .Append(E(entity.Style.Value)).Append('|')
                .Append(F(entity.Bounds.MinX)).Append(',').Append(F(entity.Bounds.MinY)).Append(',')
                .Append(F(entity.Bounds.MaxX)).Append(',').Append(F(entity.Bounds.MaxY)).Append('|')
                .Append(E(entity.Source.EntityType)).Append('|')
                .Append(E(entity.Source.Handle ?? string.Empty)).Append('|')
                .Append(entity.Source.SourceIndex?.ToString(CultureInfo.InvariantCulture) ?? string.Empty)
                .AppendLine();
        }

        var diagnostics = scene.Diagnostics.Items
            .OrderBy(x => x.Kind)
            .ThenBy(x => x.Code, StringComparer.Ordinal)
            .ThenBy(x => x.EntityId?.Value ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(x => x.Message, StringComparer.Ordinal)
            .ToArray();

        sb.Append("diagnostics=").Append(diagnostics.Length).AppendLine();
        foreach (var diagnostic in diagnostics)
        {
            sb.Append("diagnostic=")
                .Append(diagnostic.Kind).Append('|')
                .Append(E(diagnostic.Code)).Append('|')
                .Append(E(diagnostic.EntityId?.Value ?? string.Empty)).Append('|')
                .Append(E(diagnostic.Message))
                .AppendLine();
        }

        return sb.ToString();
    }

    private static string F(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    private static string E(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("|", "\\|", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal);
}
