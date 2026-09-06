using System.Runtime.CompilerServices;
using MobilDwg.Core.Compliance;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Compliance;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Tests;

public static class Stage22ReleaseRcTests
{
    [ModuleInitializer]
    public static void Run()
    {
        TestPackageMetadataAndTargetSdk36();
        TestDependencySbomAndRoyaltyFreeLicenseAudit();
        TestDataSafetyZeroNetworkPermissionsAudit();
        TestTrademarkNoticeAndLegalDisclaimers();
        TestAccessibilityAndDarkLightThemeResolver();
        TestComplianceRcSemanticSnapshotDeterminism();
        TestReleaseRcVerdictGatingBudgets();
        ExportComplianceReports();

        Console.WriteLine("STAGE22_RELEASE_RC_TESTS_PASS");
    }

    private static void ExportComplianceReports()
    {
        var targetDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "a22-android-release-rc");
        Directory.CreateDirectory(targetDir);
        var meta = CadReleaseRcAuditor.GetAuthoritativePackageMetadata();
        var deps = CadReleaseRcAuditor.GetAuthoritativeDependencyInventory();
        var safety = CadReleaseRcAuditor.GetAuthoritativeDataSafety();
        var tm = CadReleaseRcAuditor.GetAuthoritativeTrademarkNotice();
        var acc = CadReleaseRcAuditor.GetAuthoritativeAccessibilityProfile();
        var verdict = CadReleaseRcAuditor.EvaluateReleaseRc(meta, deps, safety, tm, acc, 39_822_256, 38_978_709, 134.0);
        var artifacts = new CadArtifactInventory("com.smitelagwar.mobildwg-Signed.apk", 39_822_256, "apk_sha256", "com.smitelagwar.mobildwg-Signed.aab", 38_978_709, "aab_sha256", true);
        var summary = new CadReleaseRcSummary(meta, deps, safety, tm, acc, artifacts, verdict);
        var snap = ComplianceRcSemanticSnapshot.Create(summary);

        File.WriteAllText(Path.Combine(targetDir, "SBOM.json"), CadReleaseRcAuditor.GenerateSbomJson(deps));
        File.WriteAllText(Path.Combine(targetDir, "LEGAL_NOTICES.txt"), CadReleaseRcAuditor.GenerateThirdPartyNotices(deps, tm));
        File.WriteAllText(Path.Combine(targetDir, "DATA_SAFETY.json"), System.Text.Json.JsonSerializer.Serialize(safety, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText(Path.Combine(targetDir, "COMPLIANCE_RC_SNAPSHOT.json"), snap.Content);
        Console.WriteLine("A22_COMPLIANCE_REPORTS_EXPORTED_PASS");
    }

    private static void TestPackageMetadataAndTargetSdk36()
    {
        var meta = CadReleaseRcAuditor.GetAuthoritativePackageMetadata();
        Assert(meta.PackageId == "com.smitelagwar.mobildwg", $"Invalid PackageId: {meta.PackageId}");
        Assert(meta.AppName == "Mobil DWG", $"Invalid AppName: {meta.AppName}");
        Assert(meta.VersionName == "1.0.0", $"Invalid VersionName: {meta.VersionName}");
        Assert(meta.VersionCode == 1, $"Invalid VersionCode: {meta.VersionCode}");
        Assert(meta.TargetSdkVersion == 36, $"Target SDK must be 36, found {meta.TargetSdkVersion}");
        Assert(meta.MinSdkVersion == 24, $"Min SDK must be 24, found {meta.MinSdkVersion}");
        Assert(meta.IsProductionReady, "Expected IsProductionReady = true");
    }

    private static void TestDependencySbomAndRoyaltyFreeLicenseAudit()
    {
        var deps = CadReleaseRcAuditor.GetAuthoritativeDependencyInventory();
        Assert(deps.Count >= 5, $"Expected at least 5 authoritative dependencies, found {deps.Count}");

        foreach (var d in deps)
        {
            Assert(d.IsRoyaltyFree, $"Dependency {d.PackageName} must be royalty-free");
            Assert(d.IsAudited, $"Dependency {d.PackageName} must be audited");
            Assert(d.License == "MIT" || d.License == "Apache-2.0", $"Dependency {d.PackageName} has unapproved license: {d.License}");
            Assert(!string.IsNullOrWhiteSpace(d.ProvenanceSha256), $"Dependency {d.PackageName} missing provenance hash");
        }

        string sbom = CadReleaseRcAuditor.GenerateSbomText(deps);
        Assert(sbom.Contains("DocumentName: MobilDwg-SBOM-v1.0.0"), "SBOM must contain document name");
        Assert(sbom.Contains("PackageName: ACadSharp"), "SBOM must list ACadSharp");
        Assert(sbom.Contains("PackageName: SkiaSharp"), "SBOM must list SkiaSharp");
    }

    private static void TestDataSafetyZeroNetworkPermissionsAudit()
    {
        var safety = CadReleaseRcAuditor.GetAuthoritativeDataSafety();
        Assert(!safety.NetworkAccessRequested, "Data Safety: network access must not be requested");
        Assert(!safety.UserDataCollected, "Data Safety: user data must not be collected");
        Assert(!safety.AnalyticsTrackingEnabled, "Data Safety: analytics must not be enabled");
        Assert(!safety.AdSdkIntegrated, "Data Safety: ad SDKs must not be integrated");
        Assert(safety.LocalOfflineOnly, "Data Safety: application must be strictly local and offline-only");
        Assert(safety.StorageModel == "AppPrivateScopedStorage", "Data Safety: storage must be scoped and private");
    }

    private static void TestTrademarkNoticeAndLegalDisclaimers()
    {
        var notice = CadReleaseRcAuditor.GetAuthoritativeTrademarkNotice();
        Assert(notice.AutodeskDisclaimer.Contains("Autodesk, Inc."), "Autodesk disclaimer must mention Autodesk, Inc.");
        Assert(notice.AutodeskDisclaimer.Contains("AutoCAD and DWG are trademarks", StringComparison.OrdinalIgnoreCase), "Notice must clarify DWG/AutoCAD trademarks");
        Assert(!string.IsNullOrWhiteSpace(notice.RoyaltyFreeAssurance), "Notice must contain royalty-free assurance");

        var deps = CadReleaseRcAuditor.GetAuthoritativeDependencyInventory();
        string legal = CadReleaseRcAuditor.GenerateThirdPartyNotices(deps, notice);
        Assert(legal.Contains("THIRD-PARTY SOFTWARE NOTICES"), "Legal notices must contain header");
        Assert(legal.Contains("ACadSharp"), "Legal notices must cite ACadSharp");
        Assert(legal.Contains("SkiaSharp"), "Legal notices must cite SkiaSharp");
    }

    private static void TestAccessibilityAndDarkLightThemeResolver()
    {
        var acc = CadReleaseRcAuditor.GetAuthoritativeAccessibilityProfile();
        Assert(acc.ScreenReaderSupported, "Screen reader support must be enabled");
        Assert(acc.HighContrastSupported, "High contrast support must be enabled");
        Assert(acc.DarkLightSupported, "Dark/Light theme support must be enabled");
        Assert(acc.MinimumTouchTargetDp >= 48, $"Touch target must be >= 48dp, found {acc.MinimumTouchTargetDp}");

        // Theme resolver check
        var dark = RenderColorContext.Dark;
        var light = RenderColorContext.Light;
        Assert(dark.BackgroundArgb != light.BackgroundArgb, "Dark and Light themes must have distinct background colors");
        Assert(dark.DefaultForegroundArgb != light.DefaultForegroundArgb, "Dark and Light themes must have distinct foreground colors");
    }

    private static void TestComplianceRcSemanticSnapshotDeterminism()
    {
        var meta = CadReleaseRcAuditor.GetAuthoritativePackageMetadata();
        var deps = CadReleaseRcAuditor.GetAuthoritativeDependencyInventory();
        var safety = CadReleaseRcAuditor.GetAuthoritativeDataSafety();
        var tm = CadReleaseRcAuditor.GetAuthoritativeTrademarkNotice();
        var acc = CadReleaseRcAuditor.GetAuthoritativeAccessibilityProfile();
        var verdict = CadReleaseRcAuditor.EvaluateReleaseRc(meta, deps, safety, tm, acc, 39_000_000, 25_000_000, 130.0);

        var summary = new CadReleaseRcSummary(
            PackageMeta: meta,
            Dependencies: deps,
            DataSafety: safety,
            Trademark: tm,
            Accessibility: acc,
            Artifacts: new CadArtifactInventory("app.apk", 39_000_000, "apk_sha", "app.aab", 25_000_000, "aab_sha", true),
            Verdict: verdict);

        var snap1 = ComplianceRcSemanticSnapshot.Create(summary);
        var snap2 = ComplianceRcSemanticSnapshot.Create(summary);

        Assert(snap1.Sha256Hex == snap2.Sha256Hex, "Snapshot SHA256 must be deterministic and reproducible");
        Assert(snap1.Content.StartsWith("compliance-rc/v1"), "Snapshot must begin with schema version");
        Assert(snap1.Content.Contains("package=com.smitelagwar.mobildwg"), "Snapshot must contain package ID");
        Assert(snap1.Content.Contains("verdict=ANDROID_STAGE22_RELEASE_RC_PASS"), "Snapshot must contain pass verdict");
    }

    private static void TestReleaseRcVerdictGatingBudgets()
    {
        var meta = CadReleaseRcAuditor.GetAuthoritativePackageMetadata();
        var deps = CadReleaseRcAuditor.GetAuthoritativeDependencyInventory();
        var safety = CadReleaseRcAuditor.GetAuthoritativeDataSafety();
        var tm = CadReleaseRcAuditor.GetAuthoritativeTrademarkNotice();
        var acc = CadReleaseRcAuditor.GetAuthoritativeAccessibilityProfile();

        // 1. Nominal case
        var pass = CadReleaseRcAuditor.EvaluateReleaseRc(meta, deps, safety, tm, acc, 39_000_000, 25_000_000, 130.0);
        Assert(pass.IsPass, "Nominal case must pass");
        Assert(pass.GateMarker == "ANDROID_STAGE22_RELEASE_RC_PASS", "Expected pass marker");
        Assert(pass.Score == 100, "Expected 100 score");

        // 2. APK size budget breach (>45MB)
        var failApk = CadReleaseRcAuditor.EvaluateReleaseRc(meta, deps, safety, tm, acc, 48_000_000, 25_000_000, 130.0);
        Assert(!failApk.IsPass, "Oversized APK must fail");
        Assert(failApk.Blockers.Any(b => b.Contains("APK size")), "Expected APK size blocker");

        // 3. AAB size budget breach (>45MB)
        var failAab = CadReleaseRcAuditor.EvaluateReleaseRc(meta, deps, safety, tm, acc, 39_000_000, 48_000_000, 130.0);
        Assert(!failAab.IsPass, "Oversized AAB must fail");
        Assert(failAab.Blockers.Any(b => b.Contains("AAB size")), "Expected AAB size blocker");

        // 4. Memory PSS breach (>250MB)
        var failPss = CadReleaseRcAuditor.EvaluateReleaseRc(meta, deps, safety, tm, acc, 39_000_000, 25_000_000, 260.0);
        Assert(!failPss.IsPass, "High PSS must fail");
        Assert(failPss.Blockers.Any(b => b.Contains("Total PSS")), "Expected PSS blocker");

        // 5. Data safety breach (network requested)
        var unsafeData = safety with { NetworkAccessRequested = true };
        var failData = CadReleaseRcAuditor.EvaluateReleaseRc(meta, deps, unsafeData, tm, acc, 39_000_000, 25_000_000, 130.0);
        Assert(!failData.IsPass, "Network permission request must fail");
        Assert(failData.Blockers.Any(b => b.Contains("INTERNET")), "Expected INTERNET blocker");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Stage 22 assertion failed: {message}");
        }
    }
}
