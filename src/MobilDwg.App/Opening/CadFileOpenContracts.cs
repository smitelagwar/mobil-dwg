using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

namespace MobilDwg.App.Opening;

public sealed class CadFileSelection
{
    public CadFileSelection(
        string? displayName,
        long? declaredLength,
        Func<CancellationToken, ValueTask<Stream>> openReadAsync)
    {
        if (declaredLength is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(declaredLength), "Declared length cannot be negative.");
        }

        DisplayName = displayName;
        DeclaredLength = declaredLength;
        OpenReadAsync = openReadAsync ?? throw new ArgumentNullException(nameof(openReadAsync));
    }

    public string? DisplayName { get; }

    public long? DeclaredLength { get; }

    public Func<CancellationToken, ValueTask<Stream>> OpenReadAsync { get; }
}

public sealed class CadFileOpenLimits
{
    public const long DefaultMaxBytes = 256L * 1024 * 1024;
    public const long DefaultReserveFreeBytes = 32L * 1024 * 1024;

    public static CadFileOpenLimits Default { get; } = new(DefaultMaxBytes, DefaultReserveFreeBytes);

    public CadFileOpenLimits(long maxBytes, long reserveFreeBytes)
    {
        if (maxBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBytes), "Maximum byte quota must be positive.");
        }

        if (reserveFreeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reserveFreeBytes), "Free-space reserve cannot be negative.");
        }

        MaxBytes = maxBytes;
        ReserveFreeBytes = reserveFreeBytes;
    }

    public long MaxBytes { get; }

    public long ReserveFreeBytes { get; }
}

public sealed record CadCacheCopyProgress(
    long BytesCopied,
    long? DeclaredLength,
    double? Fraction);

public enum CadFileOpenPhase
{
    Copying = 0,
    Parsing = 1,
    Extracting = 2,
    BuildingScene = 3,
    Ready = 4,
    CancelRequested = 5,
    Superseded = 6,
    Failed = 7,
}

public sealed record CadFileOpenProgress(
    long Generation,
    CadFileOpenPhase Phase,
    CadCacheCopyProgress? Copy = null,
    CadReadProgress? Reader = null,
    string? Message = null);

public enum CadFileOpenDisposition
{
    Ready = 0,
    Cancelled = 1,
    Superseded = 2,
}

public sealed class CadFileOpenResult
{
    public CadFileOpenResult(
        long generation,
        CadFileOpenDisposition disposition,
        CadDocumentMetadata? metadata = null,
        IReadOnlyList<CadDiagnostic>? diagnostics = null,
        IReadOnlyList<CadCompatibilityIssue>? compatibilityIssues = null,
        CadExtractedDocument? extractedDocument = null,
        MobilDwg.Rendering.Scene.RenderScene? preparedScene = null)
    {
        Generation = generation;
        Disposition = disposition;
        Metadata = metadata;
        Diagnostics = diagnostics ?? Array.Empty<CadDiagnostic>();
        CompatibilityIssues = compatibilityIssues ?? Array.Empty<CadCompatibilityIssue>();
        ExtractedDocument = extractedDocument;
        PreparedScene = preparedScene;
    }

    public long Generation { get; }

    public CadFileOpenDisposition Disposition { get; }

    public CadDocumentMetadata? Metadata { get; }

    public IReadOnlyList<CadDiagnostic> Diagnostics { get; }

    public IReadOnlyList<CadCompatibilityIssue> CompatibilityIssues { get; }

    public CadExtractedDocument? ExtractedDocument { get; }

    public MobilDwg.Rendering.Scene.RenderScene? PreparedScene { get; }
}

public sealed class CadFileQuotaExceededException : IOException
{
    public CadFileQuotaExceededException(long observedBytes, long maxBytes)
        : base($"CAD file exceeded the configured byte quota. Observed={observedBytes}, Max={maxBytes}.")
    {
        ObservedBytes = observedBytes;
        MaxBytes = maxBytes;
    }

    public long ObservedBytes { get; }

    public long MaxBytes { get; }
}

public sealed class CadFileInsufficientSpaceException : IOException
{
    public CadFileInsufficientSpaceException(long availableBytes, long reserveBytes)
        : base($"Insufficient free space for safe CAD cache copy. Available={availableBytes}, Reserve={reserveBytes}.")
    {
        AvailableBytes = availableBytes;
        ReserveBytes = reserveBytes;
    }

    public long AvailableBytes { get; }

    public long ReserveBytes { get; }
}
