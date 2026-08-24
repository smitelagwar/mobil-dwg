using MobilDwg.Core.Diagnostics;

namespace MobilDwg.Core.Documents;

public enum CadFormat
{
    Unknown = 0,
    Dwg = 1,
    Dxf = 2,
}

public sealed record CadDocumentMetadata(
    CadFormat Format,
    string? AcadVersion,
    string? DisplayName);

public interface ICadDocumentHandle : IAsyncDisposable
{
}

public sealed class CadDocumentSession : IAsyncDisposable
{
    private ICadDocumentHandle? _handle;

    public CadDocumentSession(
        ICadDocumentHandle handle,
        CadDocumentMetadata metadata,
        IReadOnlyList<CadDiagnostic>? diagnostics = null,
        IReadOnlyList<CadCompatibilityIssue>? compatibilityIssues = null)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Diagnostics = diagnostics ?? Array.Empty<CadDiagnostic>();
        CompatibilityIssues = compatibilityIssues ?? Array.Empty<CadCompatibilityIssue>();
    }

    public CadDocumentMetadata Metadata { get; }

    public IReadOnlyList<CadDiagnostic> Diagnostics { get; }

    public IReadOnlyList<CadCompatibilityIssue> CompatibilityIssues { get; }

    public ICadDocumentHandle Handle =>
        Volatile.Read(ref _handle) ?? throw new ObjectDisposedException(nameof(CadDocumentSession));

    public bool IsDisposed => Volatile.Read(ref _handle) is null;

    public async ValueTask DisposeAsync()
    {
        var handle = Interlocked.Exchange(ref _handle, null);
        if (handle is not null)
        {
            await handle.DisposeAsync().ConfigureAwait(false);
        }
    }
}
