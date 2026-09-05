using System.Security.Cryptography;
using System.Text;
using MobilDwg.Core.Compliance;

namespace MobilDwg.Rendering.Snapshots;

public sealed record ComplianceRcSemanticSnapshot(string Content, string Sha256Hex)
{
    public const string SchemaVersion = "compliance-rc/v1";

    public static ComplianceRcSemanticSnapshot Create(CadReleaseRcSummary summary)
    {
        var sb = new StringBuilder();
        sb.AppendLine(SchemaVersion);

        var p = summary.PackageMeta;
        sb.AppendLine($"package={p.PackageId}|{p.VersionName}|{p.VersionCode}|minSdk={p.MinSdkVersion}|targetSdk={p.TargetSdkVersion}|build={p.BuildType}|ready={p.IsProductionReady}");

        var d = summary.DataSafety;
        sb.AppendLine($"data-safety=offline-only:{d.LocalOfflineOnly}|network:{d.NetworkAccessRequested}|userData:{d.UserDataCollected}|tracking:{d.AnalyticsTrackingEnabled}|ads:{d.AdSdkIntegrated}|storage:{d.StorageModel}");

        var t = summary.Trademark;
        sb.AppendLine($"trademark=autodesk-disclaimed:{t.AutodeskDisclaimer.Contains("Autodesk")}|royalty-free:{!string.IsNullOrWhiteSpace(t.RoyaltyFreeAssurance)}");

        var a = summary.Accessibility;
        sb.AppendLine($"accessibility=screenReader:{a.ScreenReaderSupported}|contrast:{a.HighContrastSupported}|darkLight:{a.DarkLightSupported}|minTouch:{a.MinimumTouchTargetDp}dp");

        sb.AppendLine($"dependencies={summary.Dependencies.Count}");
        foreach (var dep in summary.Dependencies.OrderBy(x => x.PackageName, StringComparer.Ordinal))
        {
            sb.AppendLine($"dependency={dep.PackageName}|{dep.Version}|{dep.License}|rf:{dep.IsRoyaltyFree}|sha:{dep.ProvenanceSha256}");
        }

        var v = summary.Verdict;
        sb.AppendLine($"verdict={v.GateMarker}|isPass={v.IsPass}|score={v.Score}|blockers={v.Blockers.Count}");

        string content = sb.ToString();
        string sha256Hex = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        return new ComplianceRcSemanticSnapshot(content, sha256Hex);
    }
}
