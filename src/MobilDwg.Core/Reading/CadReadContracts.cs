using MobilDwg.Core.Documents;

namespace MobilDwg.Core.Reading;

public enum CancellationSupport
{
    None = 0,
    BeforeStartOnly = 1,
    Cooperative = 2,
}

public enum ProgressSupport
{
    None = 0,
    StagesOnly = 1,
    Fractional = 2,
}

public sealed record CadReaderCapabilities(
    CancellationSupport Cancellation,
    ProgressSupport Progress);

public enum CadReadStage
{
    Preflight,
    Opening,
    Parsing,
    Normalizing,
    Completed,
}

public sealed record CadReadProgress
{
    public CadReadProgress(CadReadStage stage, double? fraction = null, string? message = null)
    {
        if (fraction is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(fraction), "Fraction must be between 0 and 1.");
        }

        Stage = stage;
        Fraction = fraction;
        Message = message;
    }

    public CadReadStage Stage { get; }

    public double? Fraction { get; }

    public string? Message { get; }
}

public sealed record CadOpenRequest(
    Stream Source,
    string? DisplayName = null,
    long? DeclaredLength = null,
    bool LeaveOpen = false);

public interface ICadDocumentReader
{
    CadReaderCapabilities Capabilities { get; }

    ValueTask<CadDocumentSession> OpenAsync(
        CadOpenRequest request,
        IProgress<CadReadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
