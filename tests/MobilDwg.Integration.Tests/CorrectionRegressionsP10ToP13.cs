using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobilDwg.App.Opening;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Integration.Tests;

public static class CorrectionRegressionsP10ToP13
{
    public record RegressionResult(string Id, bool Passed, string Description, string Details);

    public static async Task<List<RegressionResult>> RunAllAsync(string repoRoot, bool throwOnFailures = false)
    {
        var results = new List<RegressionResult>();

        Console.WriteLine("=== RUNNING CORRECTION REGRESSION TESTS P10 TO P13 ===");

        results.Add(await RunTestAsync("P10", "SafeCadFileCache Purge Active Copy Protection", () => TestP10_PurgeActiveCopyProtectionAsync()));
        results.Add(await RunTestAsync("P11", "CadFileOpenCoordinator Dispose Semaphore Guard", () => TestP11_DisposeSemaphoreGuardAsync()));
        results.Add(await RunTestAsync("P12", "AcadSharpEntityExtractor Nested Block Transform Concatenation", () => TestP12_NestedBlockTransformAsync(repoRoot)));
        results.Add(await RunTestAsync("P13", "CadExtractedSceneBuilder Vertical Ellipse Bounds Calculation", () => TestP13_VerticalEllipseBoundsAsync()));

        int passed = results.Count(r => r.Passed);
        int failed = results.Count(r => !r.Passed);

        Console.WriteLine($"=== CORRECTION REGRESSIONS P10-P13 SUMMARY: {passed} PASSED, {failed} FAILED ===");

        if (throwOnFailures && failed > 0)
        {
            throw new InvalidOperationException($"CORRECTION_REGRESSIONS_FAILED: {failed}/{results.Count} integration tests failed. Bugs detected as expected RED regressions.");
        }

        return results;
    }

    private static async Task<RegressionResult> RunTestAsync(string id, string name, Func<Task> testAction)
    {
        try
        {
            await testAction();
            Console.WriteLine($"  [PASS] {id}: {name}");
            return new RegressionResult(id, true, name, "PASS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [FAIL] {id}: {name} - {ex.Message}");
            return new RegressionResult(id, false, name, ex.Message);
        }
    }

    /// <summary>
    /// P10: When SafeCadFileCache.PurgeOrphans is invoked during a file copy progress callback,
    /// it must not delete the completed destination file before lease registration.
    /// In current buggy code: PurgeOrphans deletes the active file before lease is stored!
    /// </summary>
    public static async Task TestP10_PurgeActiveCopyProtectionAsync()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "test-cache-p10-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            var cache = new SafeCadFileCache(tempRoot, availableBytesProvider: _ => long.MaxValue);
            var selection = new CadFileSelection("probe.dxf", 3, _ => ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            await using var copied = await cache.CopyAsync(selection, 1, new DirectProgress<CadCacheCopyProgress>(_ =>
            {
                if (Directory.EnumerateFiles(tempRoot).Any(p => !p.EndsWith(".part", StringComparison.Ordinal)))
                {
                    cache.PurgeOrphans();
                }
            }));

            if (!File.Exists(copied.FilePath))
            {
                throw new InvalidOperationException($"PurgeOrphans() deleted active copied file ({copied.FilePath}) before lease registration. Active copy unprotected.");
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
        }
    }

    /// <summary>
    /// P11: Disposing CadFileOpenCoordinator during an active parse must not leak an unhandled
    /// ObjectDisposedException from SemaphoreSlim or internal cancelation.
    /// In current buggy code: Disposing coordinator throws ObjectDisposedException when reader finishes!
    /// </summary>
    public static async Task TestP11_DisposeSemaphoreGuardAsync()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "test-cache-p11-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            var cache = new SafeCadFileCache(tempRoot, availableBytesProvider: _ => long.MaxValue);
            var reader = new PausedReader();
            var coordinator = new CadFileOpenCoordinator(reader, cache);
            var selection = new CadFileSelection("probe.dxf", 3, _ => ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var pending = coordinator.OpenLatestAsync(selection);
            await reader.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

            await coordinator.DisposeAsync();
            reader.Release.TrySetResult();

            try
            {
                await pending;
            }
            catch (ObjectDisposedException ex)
            {
                throw new InvalidOperationException($"CadFileOpenCoordinator threw ObjectDisposedException on dispose: {ex.ObjectName ?? ex.Message}. Parse gate dispose unprotected.");
            }
            catch (OperationCanceledException)
            {
                // Expected graceful cancellation
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
        }
    }

    /// <summary>
    /// P12: Nested block insert transformations must be concatenated.
    /// In synthetic_turkish_basic_ac1015.dxf, outer block is at (50, 60), inner block is at (5, 5), line is (0,0)->(10,0).
    /// Line start must be (55, 65).
    /// In current buggy code: inner block does not inherit parent block transform!
    /// </summary>
    public static async Task TestP12_NestedBlockTransformAsync(string repoRoot)
    {
        var dxfPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "synthetic_turkish_basic_ac1015.dxf");
        var reader = new AcadSharpDocumentReader();

        await using var input = File.OpenRead(dxfPath);
        await using var parsed = await reader.OpenAsync(new CadOpenRequest(input, Path.GetFileName(dxfPath), input.Length, LeaveOpen: true));

        var doc = AcadSharpEntityExtractor.Extract(parsed.Handle!);
        var nested = doc.Entities.Single(e => e.BlockOwner == "INNER" && e.EntityType == CadExtractedEntityType.Line);

        double x0 = nested.Points![0].X;
        double y0 = nested.Points![0].Y;

        if (Math.Abs(x0 - 55.0) > 1e-6 || Math.Abs(y0 - 65.0) > 1e-6)
        {
            throw new InvalidOperationException($"Nested block insert coordinates incorrect: expected (55, 65), got ({x0:F2}, {y0:F2}). Nested block transform not concatenated.");
        }
    }

    /// <summary>
    /// P13: Vertical ellipse bounds calculation in CadExtractedSceneBuilder must account for major axis orientation.
    /// Major axis (0, 10), ratio 0.5 -> expected width = 10, height = 20.
    /// In current buggy code: bounds calculation assumes horizontal major axis, computing inverted width/height!
    /// </summary>
    public static async Task TestP13_VerticalEllipseBoundsAsync()
    {
        var ellipseDxf = string.Join("\n", new[]
        {
            "0", "SECTION", "2", "HEADER", "9", "$ACADVER", "1", "AC1015", "0", "ENDSEC",
            "0", "SECTION", "2", "ENTITIES",
            "0", "ELLIPSE", "8", "0",
            "10", "0", "20", "0", "30", "0",
            "11", "0", "21", "10", "31", "0",
            "40", "0.5",
            "41", "0", "42", "6.283185307179586",
            "0", "ENDSEC", "0", "EOF", ""
        });

        var reader = new AcadSharpDocumentReader();
        await using var input = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(ellipseDxf));
        await using var parsed = await reader.OpenAsync(new CadOpenRequest(input, "vertical-ellipse.dxf", input.Length, LeaveOpen: true));

        var doc = AcadSharpEntityExtractor.Extract(parsed.Handle!);
        var rendered = CadExtractedSceneBuilder.Build(doc);
        var b = rendered.WorldBounds!.Value;

        if (Math.Abs(b.Width - 10.0) > 1e-6 || Math.Abs(b.Height - 20.0) > 1e-6)
        {
            throw new InvalidOperationException($"Vertical ellipse bounds incorrect: expected width=10, height=20; got width={b.Width:F2}, height={b.Height:F2}. Major axis orientation not handled.");
        }
    }

    private sealed class DirectProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }

    private sealed class EmptyHandle : ICadDocumentHandle
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class PausedReader : ICadDocumentReader
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CadReaderCapabilities Capabilities => new(CancellationSupport.BeforeStartOnly, ProgressSupport.None);

        public async ValueTask<CadDocumentSession> OpenAsync(
            CadOpenRequest request,
            IProgress<CadReadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Release.Task;
            return new CadDocumentSession(new EmptyHandle(), new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "probe"));
        }
    }
}
