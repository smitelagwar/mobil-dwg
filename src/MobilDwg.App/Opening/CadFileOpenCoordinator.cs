using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.App.Opening;

public sealed class CadFileOpenCoordinator : IAsyncDisposable
{
    private readonly object _sync = new();
    private readonly ICadDocumentReader _reader;
    private readonly SafeCadFileCache _cache;
    private readonly SemaphoreSlim _parseGate = new(1, 1);

    private CancellationTokenSource? _activeRequestCancellation;
    private CadOpenLease? _current;
    private long _generation;
    private bool _disposed;
    private int _activeWorkerCount;

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

    public CadExtractedDocument? CurrentExtractedDocument
    {
        get
        {
            lock (_sync)
            {
                return _current?.ExtractedDocument;
            }
        }
    }

    public RenderScene? CurrentPreparedScene
    {
        get
        {
            lock (_sync)
            {
                return _current?.PreparedScene;
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
        CadExtractedDocument? extractedDocument = null;
        RenderScene? preparedScene = null;
        bool gateAcquired = false;

        Interlocked.Increment(ref _activeWorkerCount);
        try
        {
            // At most one active parse worker at a time; wait for gate
            await _parseGate.WaitAsync(requestCancellation.Token).ConfigureAwait(false);
            gateAcquired = true;

            if (!IsAccepted(generation, requestCancellation.Token))
            {
                return CreateDiscardedResult(generation, requestCancellation.Token);
            }

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
                    Message: "Parsing CAD document on worker thread."));

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
            var (sessionResult, extractedResult, sceneResult) = await Task.Run<(CadDocumentSession? session, CadExtractedDocument? extracted, RenderScene? scene)>(
                    async () =>
                    {
                        CadDocumentSession? session = null;
                        try
                        {
                            using var stream = parseSource.OpenRead();
                            var request = new CadOpenRequest(
                                stream,
                                parseSource.DisplayName,
                                parseSource.Length,
                                LeaveOpen: false);

                            session = await _reader.OpenAsync(request, readerProgress, requestCancellation.Token)
                                .ConfigureAwait(false);

                            if (requestCancellation.IsCancellationRequested)
                            {
                                await session.DisposeAsync().ConfigureAwait(false);
                                session = null;
                                return (null, null, null);
                            }

                            // Extract entity model off UI thread
                            CadExtractedDocument? extracted = null;
                            RenderScene? scene = null;
                            if (session.Handle is MobilDwg.Cad.AcadSharp.AcadSharpDocumentHandle)
                            {
                                extracted = MobilDwg.Cad.AcadSharp.AcadSharpEntityExtractor.Extract(session.Handle);
                                scene = MobilDwg.Rendering.Scene.CadExtractedSceneBuilder.Build(extracted);
                            }

                            var result = (session, extracted, scene);
                            session = null; // Ownership successfully transferred
                            return result;
                        }
                        finally
                        {
                            if (session is not null)
                            {
                                await session.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    },
                    CancellationToken.None)
                .ConfigureAwait(false);

            parsedSession = sessionResult;
            extractedDocument = extractedResult;
            preparedScene = sceneResult;

            if (!IsAccepted(generation, requestCancellation.Token) || parsedSession is null)
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

            var lease = new CadOpenLease(parsedSession, cachedFile, extractedDocument, preparedScene);
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
                new CadFileOpenProgress(generation, CadFileOpenPhase.Ready, Message: "CAD document, extracted geometry, and scene are ready."));

            return new CadFileOpenResult(
                generation,
                CadFileOpenDisposition.Ready,
                current.Metadata,
                current.Diagnostics.ToArray(),
                current.CompatibilityIssues.ToArray(),
                lease.ExtractedDocument,
                lease.PreparedScene);
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
            if (gateAcquired)
            {
                try
                {
                    _parseGate.Release();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            lock (_sync)
            {
                if (ReferenceEquals(_activeRequestCancellation, requestCancellation))
                {
                    _activeRequestCancellation = null;
                }
            }

            requestCancellation.Dispose();

            if (Interlocked.Decrement(ref _activeWorkerCount) == 0 && Volatile.Read(ref _disposed))
            {
                try
                {
                    _parseGate.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }
            }
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

    public async ValueTask ResetCurrentSessionAsync()
    {
        CadOpenLease? current;
        CancellationTokenSource? cancellation;
        lock (_sync)
        {
            if (_disposed) return;
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

    public async ValueTask DisposeAsync()
    {
        CancellationTokenSource? cancellation;
        CadOpenLease? current;
        bool canDisposeGate;

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
            canDisposeGate = _activeWorkerCount == 0;
        }

        cancellation?.Cancel();

        if (canDisposeGate)
        {
            try
            {
                _parseGate.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

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
        private CadExtractedDocument? _extractedDocument;
        private RenderScene? _preparedScene;

        public CadOpenLease(
            CadDocumentSession session,
            CachedCadFile cachedFile,
            CadExtractedDocument? extractedDocument = null,
            RenderScene? preparedScene = null)
        {
            _session = session;
            _cachedFile = cachedFile;
            _extractedDocument = extractedDocument;
            _preparedScene = preparedScene;
        }

        public CadDocumentSession Session =>
            Volatile.Read(ref _session) ?? throw new ObjectDisposedException(nameof(CadOpenLease));

        public CadExtractedDocument? ExtractedDocument => Volatile.Read(ref _extractedDocument);
        public RenderScene? PreparedScene => Volatile.Read(ref _preparedScene);

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

            Interlocked.Exchange(ref _extractedDocument, null);
            Interlocked.Exchange(ref _preparedScene, null);
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
