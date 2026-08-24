using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using MobilDwg.App.Opening;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

var arguments = ParseArguments(args);
var cacheRoot = Path.GetFullPath(GetRequired(arguments, "--cache-root"));
var dwgPath = Path.GetFullPath(GetRequired(arguments, "--dwg"));
var dxfPath = Path.GetFullPath(GetRequired(arguments, "--dxf"));
var evidencePath = Path.GetFullPath(GetRequired(arguments, "--evidence"));

if (Directory.Exists(cacheRoot))
{
    Directory.Delete(cacheRoot, recursive: true);
}

Directory.CreateDirectory(cacheRoot);

await VerifyActualDwgDxfAsync(Path.Combine(cacheRoot, "actual"), dwgPath, dxfPath);
Console.WriteLine("STAGE06_ACTUAL_DWG_DXF_PASS");

await VerifyQuotaAndDiskGuardsAsync(Path.Combine(cacheRoot, "guards"));
Console.WriteLine("STAGE06_SAFE_COPY_GUARDS_PASS");

await VerifyLastRequestWinsAsync(Path.Combine(cacheRoot, "last-request-wins"));
Console.WriteLine("STAGE06_LAST_REQUEST_WINS_PASS");

await VerifyCancelDiscardsLateParserResultAsync(Path.Combine(cacheRoot, "cancel"));
Console.WriteLine("STAGE06_CANCEL_SEMANTICS_PASS");

Assert(Directory.GetFiles(cacheRoot, "*", SearchOption.AllDirectories).Length == 0,
    "all Stage 06 cache files must be removed after probe disposal");

var evidence = new
{
    schema_version = 1,
    stage = "06",
    actual_dwg_dxf = "PASS",
    stream_quota = "PASS",
    declared_length_not_trusted = "PASS",
    free_space_reserve = "PASS",
    atomic_unique_cache = "PASS",
    source_stream_disposal = "PASS",
    original_immutable = "PASS",
    last_request_wins = "PASS",
    non_cooperative_cancel_result_discard = "PASS",
    deterministic_cleanup = "PASS",
};

Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);
await File.WriteAllTextAsync(
    evidencePath,
    JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

Console.WriteLine("STAGE06_T2_HEADLESS_PASS");

static async Task VerifyActualDwgDxfAsync(string root, string dwgPath, string dxfPath)
{
    Directory.CreateDirectory(root);
    var beforeDwg = HashFile(dwgPath);
    var beforeDxf = HashFile(dxfPath);
    var progress = new CollectingProgress<CadFileOpenProgress>();

    await using var coordinator = new CadFileOpenCoordinator(
        new AcadSharpDocumentReader(),
        new SafeCadFileCache(
            root,
            new CadFileOpenLimits(16L * 1024 * 1024, 0),
            _ => long.MaxValue));

    var dwg = await coordinator.OpenLatestAsync(
        CreateFileSelection(dwgPath, "../../provider\\survey.dwg"),
        progress);
    Assert(dwg.Disposition == CadFileOpenDisposition.Ready, "DWG safe-open result must be Ready");
    Assert(dwg.Metadata?.Format == CadFormat.Dwg, "DWG safe-open metadata must report DWG");
    Assert(Directory.GetFiles(root).Length == 1, "exactly one current cache file must exist after DWG open");
    AssertNoPartFiles(root);
    Assert(Path.GetFileName(Directory.GetFiles(root)[0]).Contains("survey.dwg", StringComparison.Ordinal),
        "provider filename must be reduced to a safe basename");

    var dxf = await coordinator.OpenLatestAsync(
        CreateFileSelection(dxfPath, "folder/çizim<>.dxf"),
        progress);
    Assert(dxf.Disposition == CadFileOpenDisposition.Ready, "DXF safe-open result must be Ready");
    Assert(dxf.Metadata?.Format == CadFormat.Dxf, "DXF safe-open metadata must report DXF");
    Assert(Directory.GetFiles(root).Length == 1,
        "replacing the current session must dispose the previous session and cached file");
    AssertNoPartFiles(root);

    Assert(progress.Values.Any(value => value.Phase == CadFileOpenPhase.Copying), "copy progress must be reported");
    Assert(progress.Values.Any(value => value.Phase == CadFileOpenPhase.Parsing), "parse stage progress must be reported");
    Assert(progress.Values.Any(value => value.Phase == CadFileOpenPhase.Ready), "ready progress must be reported");

    Assert(HashFile(dwgPath).SequenceEqual(beforeDwg), "original DWG bytes must remain unchanged");
    Assert(HashFile(dxfPath).SequenceEqual(beforeDxf), "original DXF bytes must remain unchanged");

    await coordinator.DisposeAsync();
    Assert(Directory.GetFiles(root).Length == 0, "current cache file must be deleted on coordinator disposal");
}

static async Task VerifyQuotaAndDiskGuardsAsync(string root)
{
    Directory.CreateDirectory(root);

    var neverOpened = false;
    var declaredOversize = new CadFileSelection(
        "oversize.dwg",
        2048,
        _ =>
        {
            neverOpened = true;
            return ValueTask.FromResult<Stream>(new MemoryStream(new byte[16], writable: false));
        });

    var strictCache = new SafeCadFileCache(
        root,
        new CadFileOpenLimits(1024, 0),
        _ => long.MaxValue);

    await ExpectThrowsAsync<CadFileQuotaExceededException>(
        () => strictCache.CopyAsync(declaredOversize, 1).AsTask());
    Assert(!neverOpened, "declared length over quota must fail before opening provider stream");

    var trackingStream = new TrackingReadStream(new byte[2048]);
    var liedAboutSize = new CadFileSelection(
        "lied-size.dxf",
        8,
        _ => ValueTask.FromResult<Stream>(trackingStream));

    await ExpectThrowsAsync<CadFileQuotaExceededException>(
        () => strictCache.CopyAsync(liedAboutSize, 2).AsTask());
    Assert(trackingStream.WasDisposed, "provider stream must be disposed when actual bytes exceed quota");
    Assert(Directory.GetFiles(root).Length == 0, "quota failure must not leak .part or final cache files");

    var diskSourceOpened = false;
    var diskBlockedCache = new SafeCadFileCache(
        root,
        new CadFileOpenLimits(1024, 128),
        _ => 128);
    var diskSelection = new CadFileSelection(
        "disk.dwg",
        null,
        _ =>
        {
            diskSourceOpened = true;
            return ValueTask.FromResult<Stream>(new MemoryStream(new byte[8], writable: false));
        });

    await ExpectThrowsAsync<CadFileInsufficientSpaceException>(
        () => diskBlockedCache.CopyAsync(diskSelection, 3).AsTask());
    Assert(!diskSourceOpened, "free-space reserve failure must happen before provider stream open");
    Assert(Directory.GetFiles(root).Length == 0, "disk-space rejection must not create cache files");
}

static async Task VerifyLastRequestWinsAsync(string root)
{
    Directory.CreateDirectory(root);
    var reader = new BlockingReader();
    var cache = new SafeCadFileCache(
        root,
        new CadFileOpenLimits(1024 * 1024, 0),
        _ => long.MaxValue);

    await using var coordinator = new CadFileOpenCoordinator(reader, cache);
    var first = coordinator.OpenLatestAsync(CreateMemorySelection("first.dxf", 32));
    await reader.FirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

    var second = await coordinator.OpenLatestAsync(CreateMemorySelection("second.dxf", 48));
    Assert(second.Disposition == CadFileOpenDisposition.Ready, "second request must become current");

    reader.ReleaseFirst();
    var firstResult = await first;
    Assert(firstResult.Disposition == CadFileOpenDisposition.Superseded,
        "late first parser result must be discarded after second selection");
    Assert(reader.GetHandle(1).WasDisposed, "superseded parser session must be disposed");
    Assert(!reader.GetHandle(2).WasDisposed, "current parser session must remain alive until coordinator disposal");
    Assert(Directory.GetFiles(root).Length == 1, "only the latest generation cache file may remain");
    AssertNoPartFiles(root);

    await coordinator.DisposeAsync();
    Assert(reader.GetHandle(2).WasDisposed, "current session must be disposed on coordinator disposal");
    Assert(Directory.GetFiles(root).Length == 0, "last-request-wins cache must be empty after disposal");
}

static async Task VerifyCancelDiscardsLateParserResultAsync(string root)
{
    Directory.CreateDirectory(root);
    var reader = new BlockingReader();
    var cache = new SafeCadFileCache(
        root,
        new CadFileOpenLimits(1024 * 1024, 0),
        _ => long.MaxValue);

    await using var coordinator = new CadFileOpenCoordinator(reader, cache);
    var open = coordinator.OpenLatestAsync(CreateMemorySelection("cancel.dxf", 64));
    await reader.FirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

    Assert(coordinator.CancelCurrentRequest(), "cancel request must be recorded while parser is active");
    Assert(!open.IsCompleted, "non-cooperative parser must not be falsely reported as already stopped");

    reader.ReleaseFirst();
    var result = await open;
    Assert(result.Disposition == CadFileOpenDisposition.Cancelled,
        "parser result finishing after cancel request must be discarded");
    Assert(reader.GetHandle(1).WasDisposed, "cancelled late parser result session must be disposed");
    Assert(Directory.GetFiles(root).Length == 0, "cancelled open must not leave a cache file");
}

static CadFileSelection CreateFileSelection(string path, string displayName)
{
    var length = new FileInfo(path).Length;
    return new CadFileSelection(
        displayName,
        length,
        cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Stream stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 128 * 1024,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            return ValueTask.FromResult(stream);
        });
}

static CadFileSelection CreateMemorySelection(string displayName, int length)
{
    return new CadFileSelection(
        displayName,
        length,
        cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<Stream>(new MemoryStream(new byte[length], writable: false));
        });
}

static byte[] HashFile(string path)
{
    using var stream = File.OpenRead(path);
    return SHA256.HashData(stream);
}

static void AssertNoPartFiles(string root)
{
    Assert(!Directory.GetFiles(root, "*.part", SearchOption.AllDirectories).Any(),
        "atomic cache copy must not leave .part files");
}

static async Task<TException> ExpectThrowsAsync<TException>(Func<Task> action)
    where TException : Exception
{
    try
    {
        await action();
    }
    catch (TException exception)
    {
        return exception;
    }

    throw new InvalidOperationException($"Expected exception {typeof(TException).Name} was not thrown.");
}

static Dictionary<string, string> ParseArguments(string[] values)
{
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var index = 0; index < values.Length; index += 2)
    {
        if (index + 1 >= values.Length || !values[index].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException("Arguments must be provided as --name value pairs.");
        }

        result[values[index]] = values[index + 1];
    }

    return result;
}

static string GetRequired(IReadOnlyDictionary<string, string> values, string key)
{
    return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException($"Missing required argument {key}.");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

sealed class CollectingProgress<T> : IProgress<T>
{
    private readonly ConcurrentQueue<T> _values = new();

    public IReadOnlyCollection<T> Values => _values.ToArray();

    public void Report(T value)
    {
        _values.Enqueue(value);
    }
}

sealed class TrackingReadStream : MemoryStream
{
    public TrackingReadStream(byte[] buffer)
        : base(buffer, writable: false)
    {
    }

    public bool WasDisposed { get; private set; }

    protected override void Dispose(bool disposing)
    {
        WasDisposed = true;
        base.Dispose(disposing);
    }
}

sealed class BlockingReader : ICadDocumentReader
{
    private readonly ConcurrentDictionary<int, TrackingHandle> _handles = new();
    private readonly TaskCompletionSource _firstStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _releaseFirst = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _calls;

    public CadReaderCapabilities Capabilities { get; } = new(
        CancellationSupport.BeforeStartOnly,
        ProgressSupport.StagesOnly);

    public TaskCompletionSource FirstStarted => _firstStarted;

    public async ValueTask<CadDocumentSession> OpenAsync(
        CadOpenRequest request,
        IProgress<CadReadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var call = Interlocked.Increment(ref _calls);
        progress?.Report(new CadReadProgress(CadReadStage.Parsing, message: "blocking probe parser"));

        if (call == 1)
        {
            _firstStarted.TrySetResult();
            await _releaseFirst.Task.ConfigureAwait(false);
        }

        var handle = new TrackingHandle();
        _handles[call] = handle;
        return new CadDocumentSession(
            handle,
            new CadDocumentMetadata(CadFormat.Dxf, "AC1015", request.DisplayName),
            Array.Empty<CadDiagnostic>(),
            Array.Empty<CadCompatibilityIssue>());
    }

    public void ReleaseFirst()
    {
        _releaseFirst.TrySetResult();
    }

    public TrackingHandle GetHandle(int call)
    {
        return _handles.TryGetValue(call, out var handle)
            ? handle
            : throw new InvalidOperationException($"No tracking handle exists for reader call {call}.");
    }
}

sealed class TrackingHandle : ICadDocumentHandle
{
    public bool WasDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        WasDisposed = true;
        return ValueTask.CompletedTask;
    }
}
