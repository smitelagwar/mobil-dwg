using System.Globalization;
using System.Text;
using MobilDwg.Rendering.References;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Snapshots;

public static class ExternalReferenceSemanticSnapshot
{
    public const string Schema = "xref-compat/v1";

    public static string Create(
        IEnumerable<CadExternalReference> references,
        RenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(references);
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"schema={Schema}");

        var refList = references.OrderBy(r => r.ReferenceId, StringComparer.Ordinal).ToList();
        sb.AppendLine(CultureInfo.InvariantCulture, $"references_count={refList.Count}");

        foreach (var r in refList)
        {
            var fileName = Path.GetFileName(r.RawPath.Replace('\\', '/'));
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"ref={r.ReferenceId}|kind={r.Kind}|resolved={r.IsResolved}|file={fileName}|" +
                $"bounds={r.Bounds.MinX:F2},{r.Bounds.MinY:F2},{r.Bounds.MaxX:F2},{r.Bounds.MaxY:F2}|" +
                $"brightness={r.Brightness:F0}|fade={r.Fade:F0}");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"scene_entities={scene.Entities.Count}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"scene_diagnostics={scene.Diagnostics.Items.Count}");

        foreach (var diag in scene.Diagnostics.Items.OrderBy(d => d.Code, StringComparer.Ordinal).ThenBy(d => d.Message, StringComparer.Ordinal))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"diag={diag.Kind}|{diag.Code}|{diag.Message}");
        }

        return sb.ToString();
    }
}
