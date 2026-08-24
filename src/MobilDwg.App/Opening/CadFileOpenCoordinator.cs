using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

namespace MobilDwg.App.Opening;

public sealed class CadFileOpenCoordinator : IAsyncDisposable
{
    private readonly object _sync = new();
    private readonly ICadDocumentReader _reader;
    private readonly SafeCadFileCache _cache;

    private CancellationTokenSource? _activeRequestCancellation;
    private CadOpenLease? _current;
    private long _generation;
    private bool _disposed;

    public CadFileOpenCoordinator(ICadDocumentReader reader, SafeCadFileCache cache)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public long CurrentGeneration => Volatile.Read(ref _generation);

    public CadDocumentSession? CurrentSession
    {
        get
        {
            lock (_sync)
            {
                return _current?.Session;
            }
        }
    }

    public async Task<CadFileOpenResult> OpenLatestAsync(
        CadFileSelection selection,
        IProgress<CadFileOpenProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selection);

        long generation;
        CancellationTokenSource requestCancellation;
        CancellationTokenSource? previousCancellation;

        lock (_sync)
        {
            ThrowIfDisposed();
            generation = checked(++_generation);
            requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            previousCancellation = _activeRequestCancellation;
            _activeRequestCancellation = requestCancellation;
        }

        previousCancellation?.Cancel();

        CachedCadFile? cachedFile = null;
        CadDocumentSession? parsedSession = null;

        try
        {
            ReportIfAccepted(
                generation,
                requestCancellation.Token,
                progress,
                new CadFileOpenProgress(generation, CadFileOpenPhase.Copying, Message: "Copying selected CAD stream into private cache."));

            var copyProgress = progress is null
                ? null
                : new ForwardProgress<CadCacheCopyProgress>(copy =>
                    ReportIfAccepted(
                        generation,
                        requestCancellation.Token,
                        progress,
                        new CadFileOpenProgress(generation, CadFileOpenPhase.Copying, Copy: copy)));

            cachedFile = await _cache.CopyAsync(
                    selection,
                    generation,
                    copyProgress,
                    requestCancellation.Token)
                .ConfigureAwait(false);

            if (!IsAccepted(generation, requestCancellation.Token))
            {
                await cachedFile.DisposeAsync().ConfigureAwait(false);
                cachedFile = null;
                return CreateDiscardedResult(generation, requestCancellation.Token);
            }

            ReportIfAccepted(
                generation,
                requestCancellation.Token,
                progress,
                new CadFileOpenProgress(
                    generation,
                    CadFileOpenPhase.Parsing,
                    Message: "Parsing on a worker thread; the configured parser may not support cooperative cancellation after parsing begins."));

            var readerProgress = progress is null
                ? null
                : new ForwardProgress<CadReadProgress>(readerUpdate =>
                    ReportIfAccepted(
                        generation,
                        requestCancellation.Token,
                        progress,
                        new CadFileOpenProgress(
                            generation,
                            CadFileOpenPhase.Parsing,
                            Reader: readerUpdate,
                            Message: readerUpdate.Message)));

            var parseSource = cachedFile;
            parsedSession = await Task.Run(
                    async () =>
                    {
                        using var stream = parseSource.OpenRead();
                        var request = new CadOpenRequest(
                            stream,
                            parseSource.DisplayName,
                            parseSource.Length,
                            LeaveOpen: false);
                        return await _reader.OpenAsync(request, readerProgress, requestCancellation.Token)
                            .ConfigureAwait(false);
                    },
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!IsAccepted(generation, requestCancellation.Token))
            {
                await parsedSession.DisposeAsync().ConfigureAwait(false);
                parsedSession = null;
                await cachedFile.DisposeAsync().ConfigureAwait(false);
                cachedFile = null;
                ReportSupersededIfApplicable(generation, requestCancellation.Token, progress);
                return CreateDiscardedResult(generation, requestCancellation.Token);
            }

            var lease = new CadOpenLease(parsedSession, cachedFile);
            CadOpenLease? previousLease;
            var committed = TryCommit(generation, requestCancellation.Token, lease, out previousLease);
            if (!committed)
            {
                await lease.DisposeAsync().ConfigureAwait(false);
                parsedSession = null;
                cachedFile = null;
                ReportSupersededIfApplicable(generation, requestCancellation.Token, progress);
                return CreateDiscardedResult(generation, requestCancellation.Token);
            }

            parsedSession = null;
            cachedFile = null;

            if (previousLease is not null)
            {
                await previousLease.DisposeAsync().ConfigureAwait(false);
            }

            var current = lease.Session;
            ReportIfAccepted(
                generation,
                requestCancellation.Token,
                progress,
                new CadFileOpenProgress(generation, CadFileOpenPhase.Ready, Message: "CAD metadata and diagnostics are ready."));

            return new CadFileOpenResult(
                generation,
                CadFileOpenDisposition.Ready,
                current.Metadata,
                current.Diagnostics.ToArray(),
                current.CompatibilityIssues.ToArray());
        }
        catch (OperationCanceledException) when (requestCancellation.IsCancellationRequested)
        {
            if (parsedSession is not null)
            {
                await parsedSession.DisposeAsync().ConfigureAwait(false);
                parsedSession = null;
            }

            if (cachedFile is not null)
            {
                await cachedFile.DisposeAsync().ConfigureAwait(false);
                cachedFile = null;
            }

            ReportSupersededIfApplicable(generation, requestCancellation.Token, progress);
            return CreateDiscardedResult(generation, requestCancellation.Token);
        }
        catch
        {
            if (parsedSession is not null)
            {
                await parsedSession.DisposeAsync().ConfigureAwait(false);
            }

            if (cachedFile is not null)
            {
                await cachedFile.DisposeAsync().ConfigureAwait(false);
            }

            if (!IsCurrentGeneration(generation))
            {
                return new CadFileOpenResult(generation, CadFileOpenDisposition.Superseded);
            }

            progress?.Report(new CadFileOpenProgress(
                generation,
                CadFileOpenPhase.Failed,
                Message: "The current CAD open request failed."));
            throw;
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_activeRequestCancellation, requestCancellation))
                {
                    _activeRequestCancellation = null;
                }
            }

            requestCancellation.Dispose();
        }
    }

    public bool CancelCurrentRequest()
    {
        CancellationTokenSource? cancellation;
        lock (_sync)
        {
            if (_disposed)
            {
                return false;
            }

            cancellation = _activeRequestCancellation;
        }

        if (cancellation is null)
        {
            return false;
        }

        cancellation.Cancel();
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        CancellationTokenSource? cancellation;
        CadOpenLease? current;

        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            checked
            {
                _generation++;
            }

            cancellation = _activeRequestCancellation;
            _activeRequestCancellation = null;
            current = _current;
            _current = null;
        }

        cancellation?.Cancel();

        if (current is not null)
        {
            await current.DisposeAsync().ConfigureAwait(false);
        }
    }

    private bool TryCommit(
        long generation,
        CancellationToken cancellationToken,
        CadOpenLease lease,
        out CadOpenLease? previousLease)
    {
        lock (_sync)
        {
            if (_disposed || cancellationToken.IsCancellationRequested || _generation != generation)
            {
                previousLease = null;
                return false;
            }

            previousLease = _current;
            _current = lease;
            return true;
        }
    }

    private bool IsAccepted(long generation, CancellationToken cancellationToken)
    {
        return !cancellationToken.IsCancellationRequested && IsCurrentGeneration(generation);
    }

    private bool IsCurrentGeneration(long generation)
    {
        lock (_sync)
        {
            return !_disposed && _generation == generation;
        }
    }

    private void ReportIfAccepted(
        long generation,
        CancellationToken cancellationToken,
        IProgress<CadFileOpenProgress>? progress,
        CadFileOpenProgress update)
    {
        if (progress is not null && IsAccepted(generation, cancellationToken))
        {
            progress.Report(update);
        }
    }

    private void ReportSupersededIfApplicable(
        long generation,
        CancellationToken cancellationToken,
        IProgress<CadFileOpenProgress>? progress)
    {
        if (progress is null || cancellationToken.IsCancellationRequested || IsCurrentGeneration(generation))
        {
            return;
        }

        progress.Report(new CadFileOpenProgress(
            generation,
            CadFileOpenPhase.Superseded,
            Message: "A newer file selection superseded this parse result."));
    }

    private CadFileOpenResult CreateDiscardedResult(long generation, CancellationToken cancellationToken)
    {
        var disposition = IsCurrentGeneration(generation) && cancellationToken.IsCancellationRequested
            ? CadFileOpenDisposition.Cancelled
            : CadFileOpenDisposition.Superseded;
        return new CadFileOpenResult(generation, disposition);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed class CadOpenLease : IAsyncDisposable
    {
        private CadDocumentSession? _session;
        private CachedCadFile? _cachedFile;

        public CadOpenLease(CadDocumentSession session, CachedCadFile cachedFile)
        {
            _session = session;
            _cachedFile = cachedFile;
        }

        public CadDocumentSession Session =>
            Volatile.Read(ref _session) ?? throw new ObjectDisposedException(nameof(CadOpenLease));

        public async ValueTask DisposeAsync()
        {
            var session = Interlocked.Exchange(ref _session, null);
            if (session is not null)
            {
                await session.DisposeAsync().ConfigureAwait(false);
            }

            var cachedFile = Interlocked.Exchange(ref _cachedFile, null);
            if (cachedFile is not null)
            {
                await cachedFile.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private sealed class ForwardProgress<T> : IProgress<T>
    {
        private readonly Action<T> _forward;

        public ForwardProgress(Action<T> forward)
        {
            _forward = forward;
        }

        public void Report(T value)
        {
            _forward(value);
        }
    }
}
