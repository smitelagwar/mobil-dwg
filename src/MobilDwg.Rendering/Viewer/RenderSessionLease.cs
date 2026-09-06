using System;

namespace MobilDwg.Rendering.Viewer;

public sealed class RenderSessionLease : IDisposable
{
    private readonly CadViewerSession _session;
    private readonly RenderSnapshot _snapshot;
    private bool _disposed;

    internal RenderSessionLease(CadViewerSession session, RenderSnapshot snapshot)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    public RenderSnapshot Snapshot => _snapshot;
    public CadViewerSession Session => _session;

    public void Dispose()
    {
        if (!_disposed)
        {
            _session.ReleaseRenderLease();
            _disposed = true;
        }
    }
}
