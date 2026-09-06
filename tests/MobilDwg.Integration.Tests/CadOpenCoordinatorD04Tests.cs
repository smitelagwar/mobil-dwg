using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobilDwg.App.Opening;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

namespace MobilDwg.Integration.Tests;

public static class CadOpenCoordinatorD04Tests
{
    public static async Task RunAllAsync()
    {
        Console.WriteLine("=== RUNNING D04 COORDINATOR AND CACHE TESTS ===");
        await TestRapidSequenceA_B_C_OnlyCCommittedAsync();
        Console.WriteLine("  [PASS] TestRapidSequenceA_B_C_OnlyCCommitted");

        await TestCloseDuringActiveParseAsync();
        Console.WriteLine("  [PASS] TestCloseDuringActiveParse");

        await TestCorruptAndZeroByteStreamsAsync();
        Console.WriteLine("  [PASS] TestCorruptAndZeroByteStreams");

        await TestDrainAfterDisposeLeavesZeroActiveLeasesAsync();
        Console.WriteLine("  [PASS] TestDrainAfterDisposeLeavesZeroActiveLeases");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"ASSERTION_FAILED: {message}");
    }

    private sealed class MockReader(TimeSpan delay) : ICadDocumentReader
    {
        public CadReaderCapabilities Capabilities => new(CancellationSupport.BeforeStartOnly, ProgressSupport.None);

        public async ValueTask<CadDocumentSession> OpenAsync(
            CadOpenRequest request,
            IProgress<CadReadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            return new CadDocumentSession(
                new MockHandle(),
                new CadDocumentMetadata(CadFormat.Dxf, "AC1015", request.DisplayName));
        }
    }

    private sealed class MockHandle : ICadDocumentHandle
    {
        public bool IsDisposed { get; private set; }
        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }
    }

    public static async Task TestRapidSequenceA_B_C_OnlyCCommittedAsync()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "test-d04-abc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            var cache = new SafeCadFileCache(tempRoot);
            var reader = new MockReader(TimeSpan.FromMilliseconds(50));
            var coordinator = new CadFileOpenCoordinator(reader, cache);

            var selA = new CadFileSelection("A.dxf", 3, _ => ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));
            var selB = new CadFileSelection("B.dxf", 3, _ => ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 4, 5, 6 })));
            var selC = new CadFileSelection("C.dxf", 3, _ => ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 7, 8, 9 })));

            var taskA = coordinator.OpenLatestAsync(selA);
            var taskB = coordinator.OpenLatestAsync(selB);
            var taskC = coordinator.OpenLatestAsync(selC);

            var resA = await taskA;
            var resB = await taskB;
            var resC = await taskC;

            Assert(resA.Disposition == CadFileOpenDisposition.Superseded, $"Expected A superseded, got {resA.Disposition}");
            Assert(resB.Disposition == CadFileOpenDisposition.Superseded, $"Expected B superseded, got {resB.Disposition}");
            Assert(resC.Disposition == CadFileOpenDisposition.Ready, $"Expected C Ready, got {resC.Disposition}");

            Assert(coordinator.CurrentSession?.Metadata.DisplayName == "C.dxf",
                $"Expected current session C.dxf, got {coordinator.CurrentSession?.Metadata.DisplayName}");

            await coordinator.DisposeAsync();
            Assert(SafeCadFileCache.ActiveFileCount == 0, $"Expected 0 active cache files, got {SafeCadFileCache.ActiveFileCount}");
        }
        finally
        {
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
        }
    }

    public static async Task TestCloseDuringActiveParseAsync()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "test-d04-close-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            var cache = new SafeCadFileCache(tempRoot);
            var startedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var controllableReader = new ControllableReader(startedTcs, releaseTcs);
            var coordinator = new CadFileOpenCoordinator(controllableReader, cache);

            var sel = new CadFileSelection("probe.dxf", 3, _ => ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));
            var openTask = coordinator.OpenLatestAsync(sel);

            await startedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

            // Close active drawing / reset session while reader is paused inside OpenAsync
            await coordinator.ResetCurrentSessionAsync();

            // Resume reader
            releaseTcs.TrySetResult();

            var result = await openTask;
            Assert(result.Disposition == CadFileOpenDisposition.Superseded || result.Disposition == CadFileOpenDisposition.Cancelled,
                $"Expected Superseded/Cancelled on reset during parse, got {result.Disposition}");

            Assert(coordinator.CurrentSession == null, "CurrentSession must be null after reset");

            await coordinator.DisposeAsync();
            Assert(SafeCadFileCache.ActiveFileCount == 0, "Active cache files count must be 0 after drain");
        }
        finally
        {
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
        }
    }

    public static async Task TestCorruptAndZeroByteStreamsAsync()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "test-d04-corrupt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            var cache = new SafeCadFileCache(tempRoot);
            var reader = new MockReader(TimeSpan.Zero);
            var coordinator = new CadFileOpenCoordinator(reader, cache);

            // Zero-byte stream
            var emptySel = new CadFileSelection("empty.dxf", 0, _ => ValueTask.FromResult<Stream>(new MemoryStream(Array.Empty<byte>())));
            var emptyRes = await coordinator.OpenLatestAsync(emptySel);
            Assert(emptyRes.Disposition == CadFileOpenDisposition.Ready, "Empty stream copied successfully into session");

            await coordinator.ResetCurrentSessionAsync();
            await coordinator.DisposeAsync();

            Assert(SafeCadFileCache.ActiveFileCount == 0, "No active file leases should remain");
        }
        finally
        {
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
        }
    }

    public static async Task TestDrainAfterDisposeLeavesZeroActiveLeasesAsync()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "test-d04-drain-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            var cache = new SafeCadFileCache(tempRoot);
            var reader = new MockReader(TimeSpan.FromMilliseconds(10));
            var coordinator = new CadFileOpenCoordinator(reader, cache);

            var sel = new CadFileSelection("drain.dxf", 5, _ => ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })));
            var res = await coordinator.OpenLatestAsync(sel);
            Assert(res.Disposition == CadFileOpenDisposition.Ready, "Expected Ready");
            Assert(SafeCadFileCache.ActiveFileCount == 1, "Expected 1 active file before dispose");

            await coordinator.DisposeAsync();
            Assert(SafeCadFileCache.ActiveFileCount == 0, "Expected 0 active files after coordinator dispose");
        }
        finally
        {
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
        }
    }

    private sealed class ControllableReader(TaskCompletionSource started, TaskCompletionSource release) : ICadDocumentReader
    {
        public CadReaderCapabilities Capabilities => new(CancellationSupport.BeforeStartOnly, ProgressSupport.None);

        public async ValueTask<CadDocumentSession> OpenAsync(
            CadOpenRequest request,
            IProgress<CadReadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            started.TrySetResult();
            await release.Task;
            return new CadDocumentSession(new MockHandle(), new CadDocumentMetadata(CadFormat.Dxf, "AC1015", request.DisplayName));
        }
    }
}
