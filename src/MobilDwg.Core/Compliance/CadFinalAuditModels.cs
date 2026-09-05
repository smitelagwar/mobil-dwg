namespace MobilDwg.Core.Compliance;

public sealed record ToolchainFreezeRecord(
    string DotnetSdkVersion,
    string TargetFramework,
    string AndroidWorkload,
    int TargetSdkVersion,
    int MinSdkVersion,
    bool IsToolchainFrozen);

public sealed record DependencyFreezeRecord(
    string PackageName,
    string ExactVersion,
    string License,
    bool IsAllowlisted,
    bool IsDirectProduction,
    string ProvenanceSha256);

public sealed record NativeBinaryAudit(
    string RelativePath,
    string Abi,
    string LibraryName,
    bool IsApproved,
    string? DisallowedReason);

public sealed record FontAssetAudit(
    string TypefaceName,
    string FallbackFont,
    bool IsBundledProprietaryShx,
    bool IsApproved);

public sealed record FinalAuditVerdict(
    bool IsPass,
    string GateMarker,
    int TotalChecks,
    int PassedChecks,
    IReadOnlyList<string> Blockers);

public sealed record FinalAuditSummary(
    ToolchainFreezeRecord Toolchain,
    IReadOnlyList<DependencyFreezeRecord> Dependencies,
    IReadOnlyList<NativeBinaryAudit> NativeBinaries,
    IReadOnlyList<FontAssetAudit> FontAssets,
    CadDataSafetyDeclaration DataSafety,
    FinalAuditVerdict Verdict);
