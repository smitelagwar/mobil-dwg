using System.Runtime.CompilerServices;
using MobilDwg.Core.Compliance;
using MobilDwg.Rendering.Compliance;

namespace MobilDwg.Rendering.Tests;

public static class Stage26FinalAuditTests
{
    [ModuleInitializer]
    public static void Run()
    {
        TestToolchainFreezeAudit();
        TestDependencyFreezeAllowlistAudit();
        TestNativeBinaryBoundaryAudit();
        TestFontAssetSubstitutionAudit();
        TestFinalRcVerdictEvaluation();
        TestRcApprovalDeterministicSnapshot();

        Console.WriteLine("STAGE26_FINAL_AUDIT_TESTS_PASS");
    }

    private static void TestToolchainFreezeAudit()
    {
        var toolchain = CadFinalRcAuditor.GetAuthoritativeToolchainFreeze();
        Assert(toolchain.DotnetSdkVersion == "10.0.400", $"Expected SDK 10.0.400, found {toolchain.DotnetSdkVersion}");
        Assert(toolchain.TargetSdkVersion == 36, $"Expected TargetSdk 36, found {toolchain.TargetSdkVersion}");
        Assert(toolchain.MinSdkVersion == 24, $"Expected MinSdk 24, found {toolchain.MinSdkVersion}");
        Assert(toolchain.IsToolchainFrozen, "Toolchain must be marked frozen");
        Console.WriteLine("STAGE26_TOOLCHAIN_FREEZE_PASS");
    }

    private static void TestDependencyFreezeAllowlistAudit()
    {
        var deps = CadFinalRcAuditor.GetAuthoritativeDependencyFreeze();
        Assert(deps.Count >= 6, $"Expected at least 6 dependencies, found {deps.Count}");

        foreach (var d in deps)
        {
            Assert(d.IsAllowlisted, $"Dependency {d.PackageName} is not allowlisted");
            Assert(d.License == "MIT" || d.License == "Apache-2.0", $"Disallowed license: {d.License}");
            Assert(!string.IsNullOrWhiteSpace(d.ExactVersion), $"Missing exact version: {d.PackageName}");
            Assert(!string.IsNullOrWhiteSpace(d.ProvenanceSha256), $"Missing provenance hash: {d.PackageName}");
        }

        Assert(deps.Any(d => d.PackageName == "ACadSharp" && d.ExactVersion == "3.7.1"), "ACadSharp 3.7.1 required");
        Assert(deps.Any(d => d.PackageName == "SkiaSharp" && d.ExactVersion == "4.151.1"), "SkiaSharp 4.151.1 required");
        Console.WriteLine("STAGE26_DEPENDENCY_FREEZE_PASS");
    }

    private static void TestNativeBinaryBoundaryAudit()
    {
        var binaries = CadFinalRcAuditor.GetAuthoritativeNativeBinaryAudit();
        Assert(binaries.Count >= 4, $"Expected at least 4 ABIs for SkiaSharp, found {binaries.Count}");

        foreach (var b in binaries)
        {
            Assert(b.IsApproved, $"Unapproved binary: {b.RelativePath}");
            Assert(b.LibraryName == "libSkiaSharp.so", $"Only libSkiaSharp.so allowed, found: {b.LibraryName}");
        }
        Console.WriteLine("STAGE26_NATIVE_ASSET_AUDIT_PASS");
    }

    private static void TestFontAssetSubstitutionAudit()
    {
        var fonts = CadFinalRcAuditor.GetAuthoritativeFontAssetAudit();
        Assert(fonts.Count >= 5, $"Expected at least 5 font audits, found {fonts.Count}");

        foreach (var f in fonts)
        {
            Assert(!f.IsBundledProprietaryShx, $"Proprietary SHX must not be bundled: {f.TypefaceName}");
            Assert(f.IsApproved, $"Unapproved font substitution: {f.TypefaceName}");
            Assert(!string.IsNullOrWhiteSpace(f.FallbackFont), $"Missing fallback font for {f.TypefaceName}");
        }
        Console.WriteLine("STAGE26_FONT_SUBSTITUTION_AUDIT_PASS");
    }

    private static void TestFinalRcVerdictEvaluation()
    {
        var toolchain = CadFinalRcAuditor.GetAuthoritativeToolchainFreeze();
        var deps = CadFinalRcAuditor.GetAuthoritativeDependencyFreeze();
        var native = CadFinalRcAuditor.GetAuthoritativeNativeBinaryAudit();
        var fonts = CadFinalRcAuditor.GetAuthoritativeFontAssetAudit();
        var safety = CadReleaseRcAuditor.GetAuthoritativeDataSafety();

        // 1. Nominal case
        var pass = CadFinalRcAuditor.EvaluateFinalRcAudit(toolchain, deps, native, fonts, safety, 39_000_000, 25_000_000, 130.0);
        Assert(pass.IsPass, "Nominal audit must pass");
        Assert(pass.GateMarker == "ANDROID_STAGE26_RC_APPROVAL_PASS", "Marker must match pass");
        Assert(pass.Blockers.Count == 0, "Nominal case must have zero blockers");

        // 2. Oversized APK (>45MB)
        var failApk = CadFinalRcAuditor.EvaluateFinalRcAudit(toolchain, deps, native, fonts, safety, 50_000_000, 25_000_000, 130.0);
        Assert(!failApk.IsPass, "Oversized APK must fail");
        Assert(failApk.Blockers.Any(b => b.Contains("APK size")), "Expected APK size blocker");

        // 3. Unapproved Native Binary
        var dirtyNative = native.Concat(new[] { new NativeBinaryAudit("lib/x86/libRealDwg.so", "x86", "libRealDwg.so", false, "Commercial CAD SDK") }).ToList();
        var failNative = CadFinalRcAuditor.EvaluateFinalRcAudit(toolchain, deps, dirtyNative, fonts, safety, 39_000_000, 25_000_000, 130.0);
        Assert(!failNative.IsPass, "Dirty native binary must fail");

        // 4. Bundled Proprietary SHX
        var dirtyFonts = fonts.Concat(new[] { new FontAssetAudit("AUTOCAD.SHX", "None", true, false) }).ToList();
        var failFonts = CadFinalRcAuditor.EvaluateFinalRcAudit(toolchain, deps, native, dirtyFonts, safety, 39_000_000, 25_000_000, 130.0);
        Assert(!failFonts.IsPass, "Bundled proprietary SHX must fail");

        Console.WriteLine("STAGE26_VERDICT_EVALUATION_PASS");
    }

    private static void TestRcApprovalDeterministicSnapshot()
    {
        var toolchain = CadFinalRcAuditor.GetAuthoritativeToolchainFreeze();
        var deps = CadFinalRcAuditor.GetAuthoritativeDependencyFreeze();
        var native = CadFinalRcAuditor.GetAuthoritativeNativeBinaryAudit();
        var fonts = CadFinalRcAuditor.GetAuthoritativeFontAssetAudit();
        var safety = CadReleaseRcAuditor.GetAuthoritativeDataSafety();
        var verdict = CadFinalRcAuditor.EvaluateFinalRcAudit(toolchain, deps, native, fonts, safety, 39_000_000, 25_000_000, 130.0);

        var summary = new FinalAuditSummary(toolchain, deps, native, fonts, safety, verdict);
        var snap1 = CadFinalRcAuditor.GenerateRcApprovalSnapshot(summary);
        var snap2 = CadFinalRcAuditor.GenerateRcApprovalSnapshot(summary);

        Assert(snap1.Sha256Hex == snap2.Sha256Hex, "Snapshot SHA256 must be deterministic");
        Assert(snap1.Content.StartsWith("schema=rc-approval/v1"), "Snapshot must start with schema=rc-approval/v1");
        Assert(snap1.Content.Contains("toolchain.sdk=10.0.400"), "Snapshot must record SDK version");
        Assert(snap1.Content.Contains("verdict=ANDROID_STAGE26_RC_APPROVAL_PASS"), "Snapshot must contain pass verdict");
        Console.WriteLine("STAGE26_SNAPSHOT_DETERMINISM_PASS");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Stage 26 Assertion Failed: {message}");
        }
    }
}
