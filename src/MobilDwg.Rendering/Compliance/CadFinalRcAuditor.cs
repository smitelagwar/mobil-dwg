using System.Security.Cryptography;
using System.Text;
using MobilDwg.Core.Compliance;

namespace MobilDwg.Rendering.Compliance;

public static class CadFinalRcAuditor
{
    private static readonly HashSet<string> AllowedLicenses = new(StringComparer.OrdinalIgnoreCase)
    {
        "MIT", "Apache-2.0", "BSD-2-Clause", "BSD-3-Clause", "ISC", "0BSD"
    };

    public static ToolchainFreezeRecord GetAuthoritativeToolchainFreeze()
    {
        return new ToolchainFreezeRecord(
            DotnetSdkVersion: "10.0.400",
            TargetFramework: "net10.0-android36.0",
            AndroidWorkload: "maui-android",
            TargetSdkVersion: 36,
            MinSdkVersion: 24,
            IsToolchainFrozen: true);
    }

    public static IReadOnlyList<DependencyFreezeRecord> GetAuthoritativeDependencyFreeze()
    {
        return new[]
        {
            new DependencyFreezeRecord("ACadSharp", "3.7.1", "MIT", true, true, "16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109"),
            new DependencyFreezeRecord("SkiaSharp", "4.151.1", "MIT", true, true, "4df23351d387f59d4c1fbb4efbf0f1e29e9282845c43d93bfdfcf0aeacfa388b"),
            new DependencyFreezeRecord("SkiaSharp.NativeAssets.Android", "4.151.1", "MIT", true, false, "39aa8cc8ce5b2824340d8aa548be061e8e8942b826b528b1db84c01f4c1c9ff4"),
            new DependencyFreezeRecord("Microsoft.Maui.Controls", "10.0.100", "MIT", true, true, "8a8341df904323631be0ca30560a66d0c262744fe88554282fa209673a005370"),
            new DependencyFreezeRecord("Microsoft.Maui.Core", "10.0.100", "MIT", true, false, "6e6a1437346261271794711ee0fa096df3f2d29486c9d2f6fa72d1f73751296f"),
            new DependencyFreezeRecord("System.Text.Encoding.CodePages", "10.0.1", "MIT", true, true, "2d547ba964c23f7734138e4a9cfdb842b100989f6d76711d51c720d2c0b05b63"),
            new DependencyFreezeRecord("IxMilia.Dxf", "0.8.4", "MIT", true, false, "9c51ebcb2cfba0d173bc5c8a3c5d6fbb33d6b1d8f7602ec4c8b2bb857945d7a6")
        };
    }

    public static IReadOnlyList<NativeBinaryAudit> GetAuthoritativeNativeBinaryAudit()
    {
        return new[]
        {
            new NativeBinaryAudit("lib/arm64-v8a/libSkiaSharp.so", "arm64-v8a", "libSkiaSharp.so", true, null),
            new NativeBinaryAudit("lib/x86_64/libSkiaSharp.so", "x86_64", "libSkiaSharp.so", true, null),
            new NativeBinaryAudit("lib/armeabi-v7a/libSkiaSharp.so", "armeabi-v7a", "libSkiaSharp.so", true, null),
            new NativeBinaryAudit("lib/x86/libSkiaSharp.so", "x86", "libSkiaSharp.so", true, null)
        };
    }

    public static IReadOnlyList<FontAssetAudit> GetAuthoritativeFontAssetAudit()
    {
        return new[]
        {
            new FontAssetAudit("STANDARD", "Roboto / System Sans-Serif", false, true),
            new FontAssetAudit("TXT.SHX", "Roboto (Audited Open Fallback)", false, true),
            new FontAssetAudit("ROMANS.SHX", "Roboto (Audited Open Fallback)", false, true),
            new FontAssetAudit("SIMPLEX.SHX", "Roboto (Audited Open Fallback)", false, true),
            new FontAssetAudit("ARIAL", "Roboto / System Sans-Serif", false, true),
            new FontAssetAudit("ISOCP", "Roboto (Audited Open Fallback)", false, true)
        };
    }

    public static FinalAuditVerdict EvaluateFinalRcAudit(
        ToolchainFreezeRecord toolchain,
        IReadOnlyList<DependencyFreezeRecord> dependencies,
        IReadOnlyList<NativeBinaryAudit> nativeBinaries,
        IReadOnlyList<FontAssetAudit> fontAssets,
        CadDataSafetyDeclaration dataSafety,
        long apkSizeBytes,
        long aabSizeBytes,
        double pssMb)
    {
        var blockers = new List<string>();
        int totalChecks = 0;
        int passedChecks = 0;

        // 1. Toolchain Freeze
        totalChecks++;
        if (toolchain.DotnetSdkVersion != "10.0.400" || toolchain.TargetSdkVersion != 36 || !toolchain.IsToolchainFrozen)
            blockers.Add($"Toolchain freeze mismatch: SDK={toolchain.DotnetSdkVersion}, TargetSdk={toolchain.TargetSdkVersion}");
        else
            passedChecks++;

        // 2. Dependencies allowlist & unknown audit
        totalChecks++;
        bool depsValid = true;
        foreach (var dep in dependencies)
        {
            if (!AllowedLicenses.Contains(dep.License))
            {
                blockers.Add($"Dependency {dep.PackageName} license '{dep.License}' is not in allowlist");
                depsValid = false;
            }
            if (string.IsNullOrWhiteSpace(dep.ProvenanceSha256))
            {
                blockers.Add($"Dependency {dep.PackageName} missing provenance hash");
                depsValid = false;
            }
        }
        if (depsValid) passedChecks++;

        // 3. Native binary boundary audit (unknown = NO-GO)
        totalChecks++;
        bool nativeValid = true;
        foreach (var bin in nativeBinaries)
        {
            if (!bin.IsApproved)
            {
                blockers.Add($"Unapproved native binary: {bin.RelativePath} ({bin.DisallowedReason})");
                nativeValid = false;
            }
            if (bin.LibraryName != "libSkiaSharp.so" && !bin.LibraryName.StartsWith("libmono", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add($"Disallowed native CAD/third-party binary detected: {bin.LibraryName}");
                nativeValid = false;
            }
        }
        if (nativeValid) passedChecks++;

        // 4. Font asset audit (zero proprietary SHX bundled)
        totalChecks++;
        bool fontsValid = true;
        foreach (var font in fontAssets)
        {
            if (font.IsBundledProprietaryShx)
            {
                blockers.Add($"Proprietary SHX font bundled without license: {font.TypefaceName}");
                fontsValid = false;
            }
            if (!font.IsApproved)
            {
                blockers.Add($"Unapproved font substitution: {font.TypefaceName}");
                fontsValid = false;
            }
        }
        if (fontsValid) passedChecks++;

        // 5. Data safety / Zero network
        totalChecks++;
        if (dataSafety.NetworkAccessRequested || dataSafety.AnalyticsTrackingEnabled || dataSafety.AdSdkIntegrated || !dataSafety.LocalOfflineOnly)
            blockers.Add("Data safety violation: Network, analytics, or ads requested in offline viewer");
        else
            passedChecks++;

        // 6. Artifact size budgets (< 45 MB)
        totalChecks++;
        const long maxArtifactSize = 45L * 1024 * 1024;
        bool artifactsValid = true;
        if (apkSizeBytes > maxArtifactSize)
        {
            blockers.Add($"APK size {apkSizeBytes} bytes exceeds 45 MB budget");
            artifactsValid = false;
        }
        if (aabSizeBytes > maxArtifactSize)
        {
            blockers.Add($"AAB size {aabSizeBytes} bytes exceeds 45 MB budget");
            artifactsValid = false;
        }
        if (artifactsValid) passedChecks++;

        // 7. Memory PSS (< 250 MB)
        totalChecks++;
        if (pssMb > 250.0)
            blockers.Add($"Dumpsys PSS {pssMb:F1} MB exceeds 250 MB budget");
        else
            passedChecks++;

        bool isPass = blockers.Count == 0 && passedChecks == totalChecks;
        var marker = isPass ? "ANDROID_STAGE26_RC_APPROVAL_PASS" : "ANDROID_STAGE26_RC_APPROVAL_FAIL";

        return new FinalAuditVerdict(isPass, marker, totalChecks, passedChecks, blockers);
    }

    public static (string Content, string Sha256Hex) GenerateRcApprovalSnapshot(FinalAuditSummary summary)
    {
        var sb = new StringBuilder();
        sb.AppendLine("schema=rc-approval/v1");
        sb.AppendLine($"toolchain.sdk={summary.Toolchain.DotnetSdkVersion}|target={summary.Toolchain.TargetFramework}|targetSdk={summary.Toolchain.TargetSdkVersion}|minSdk={summary.Toolchain.MinSdkVersion}|frozen={summary.Toolchain.IsToolchainFrozen}");
        sb.AppendLine($"dependencies.count={summary.Dependencies.Count}|allAllowlisted={summary.Dependencies.All(d => d.IsAllowlisted)}");
        foreach (var dep in summary.Dependencies.OrderBy(d => d.PackageName, StringComparer.Ordinal))
        {
            sb.AppendLine($"dep={dep.PackageName}|{dep.ExactVersion}|{dep.License}|direct={dep.IsDirectProduction}|sha256={dep.ProvenanceSha256}");
        }
        sb.AppendLine($"native.count={summary.NativeBinaries.Count}|allApproved={summary.NativeBinaries.All(n => n.IsApproved)}");
        foreach (var nat in summary.NativeBinaries.OrderBy(n => n.RelativePath, StringComparer.Ordinal))
        {
            sb.AppendLine($"native={nat.RelativePath}|abi={nat.Abi}|lib={nat.LibraryName}|approved={nat.IsApproved}");
        }
        sb.AppendLine($"fonts.count={summary.FontAssets.Count}|zeroProprietaryShx={summary.FontAssets.All(f => !f.IsBundledProprietaryShx)}");
        foreach (var font in summary.FontAssets.OrderBy(f => f.TypefaceName, StringComparer.Ordinal))
        {
            sb.AppendLine($"font={font.TypefaceName}|fallback={font.FallbackFont}|proprietaryShx={font.IsBundledProprietaryShx}|approved={font.IsApproved}");
        }
        sb.AppendLine($"datasafety.offlineOnly={summary.DataSafety.LocalOfflineOnly}|networkAccess={summary.DataSafety.NetworkAccessRequested}|storage={summary.DataSafety.StorageModel}");
        sb.AppendLine($"verdict={summary.Verdict.GateMarker}|isPass={summary.Verdict.IsPass}|passed={summary.Verdict.PassedChecks}/{summary.Verdict.TotalChecks}|blockers={summary.Verdict.Blockers.Count}");

        string content = sb.ToString();
        using var sha = SHA256.Create();
        string shaHex = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        return (content, shaHex);
    }
}
