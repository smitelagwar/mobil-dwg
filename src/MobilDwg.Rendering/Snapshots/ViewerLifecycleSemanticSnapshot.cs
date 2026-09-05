using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MobilDwg.Core.Storage;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Rendering.Snapshots;

public static class ViewerLifecycleSemanticSnapshot
{
    public const string SchemaVersion = "viewer-lifecycle/v1";

    public static string Create(CadViewerSession session, RecentFilesManager? recentFiles = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"schema={SchemaVersion}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"document={session.Metadata.DisplayName ?? "unknown"}|{session.Metadata.Format}|{session.Metadata.AcadVersion ?? "?"}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"layout={session.ActiveLayoutName}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"camera={session.Camera.Center.X:F4},{session.Camera.Center.Y:F4}|scale={session.Camera.WorldUnitsPerPixel:G6}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"viewport={session.ViewportPixelWidth}x{session.ViewportPixelHeight}");

        var visibleLayers = session.LayerTable.Layers.Count(l => l.IsVisible);
        var hiddenLayers = session.LayerTable.Layers.Count(l => !l.IsVisible);
        sb.AppendLine(CultureInfo.InvariantCulture, $"layers=total:{session.LayerTable.Layers.Count}|visible:{visibleLayers}|hidden:{hiddenLayers}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"diagnostics={session.Diagnostics.Count}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"compatibility={session.CompatibilityIssues.Count}");

        if (recentFiles != null)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"recent_count={recentFiles.Entries.Count}");
            foreach (var rf in recentFiles.Entries)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"recent={rf.DisplayName}|{rf.SizeBytes}");
            }
        }

        return sb.ToString();
    }

    public static string ComputeSha256(string snapshot)
    {
        var bytes = Encoding.UTF8.GetBytes(snapshot);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}
