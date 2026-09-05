namespace MobilDwg.Core.Compliance;

public sealed record CadPackageMetadata(
    string PackageId,
    string AppName,
    string VersionName,
    int VersionCode,
    int MinSdkVersion,
    int TargetSdkVersion,
    string BuildType,
    bool IsProductionReady);

public sealed record CadDependencyEntry(
    string PackageName,
    string Version,
    string License,
    bool IsRoyaltyFree,
    bool IsAudited,
    string ProvenanceSha256);

public sealed record CadDataSafetyDeclaration(
    bool NetworkAccessRequested,
    bool UserDataCollected,
    bool AnalyticsTrackingEnabled,
    bool AdSdkIntegrated,
    bool LocalOfflineOnly,
    string StorageModel);

public sealed record CadTrademarkNotice(
    string LegalDisclaimer,
    string AutodeskDisclaimer,
    string CopyrightNotice,
    string RoyaltyFreeAssurance);

public sealed record CadAccessibilityProfile(
    bool ScreenReaderSupported,
    bool HighContrastSupported,
    bool DarkLightSupported,
    int MinimumTouchTargetDp);

public sealed record CadArtifactInventory(
    string ApkPath,
    long ApkSizeBytes,
    string ApkSha256,
    string AabPath,
    long AabSizeBytes,
    string AabSha256,
    bool IsSigned);

public sealed record CadReleaseRcVerdict(
    bool IsPass,
    string GateMarker,
    int Score,
    IReadOnlyList<string> Blockers);

public sealed record CadReleaseRcSummary(
    CadPackageMetadata PackageMeta,
    IReadOnlyList<CadDependencyEntry> Dependencies,
    CadDataSafetyDeclaration DataSafety,
    CadTrademarkNotice Trademark,
    CadAccessibilityProfile Accessibility,
    CadArtifactInventory Artifacts,
    CadReleaseRcVerdict Verdict);
