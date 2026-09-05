#if A25_VALIDATION
using System.Security.Cryptography;
using Android.Util;
using MobilDwg.App.Opening;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.App;

public sealed record A25ValidationResult(
    byte[] Png,
    string PngSha256,
    string BlockerSummary,
    string Marker);

public static class A25AndroidValidationRunner
{
    public const string Tag = "MobilDwgA25";

    public static async Task<A25ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A25_ANDROID_VALIDATION_STARTING");
        await Task.Delay(1200);

        var results = new List<string>();

        // B2: Dispose Chain
        try
        {
            var scene = BuildSyntheticScene();
            var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
            var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "synthetic_a25.dxf");
            var session = new CadViewerSession(metadata, scene, layoutManager,
                initialPixelWidth: 1080, initialPixelHeight: 1920);
            session.ZoomToFit();
            session.Pan(10, 10);
            using var surface0 = new SkiaBitmapRenderSurface(1080, 1920);
            await session.RenderAsync(surface0);
            session.Dispose();
            bool threwCorrectly = false;
            try { session.Pan(1, 1); }
            catch (ObjectDisposedException) { threwCorrectly = true; }
            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
            if (threwCorrectly)
            {
                Log.Info(Tag, "A25_DISPOSE_CHAIN_PASS");
                results.Add("B2=PASS");
            }
            else
            {
                Log.Warn(Tag, "A25_DISPOSE_CHAIN_WARN");
                results.Add("B2=WARN_NO_ODE");
            }
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"A25_DISPOSE_CHAIN_FAIL: {ex}");
            results.Add($"B2=FAIL:{ex.GetType().Name}");
        }

        // B3: Cache Purge
        try
        {
            var cacheRoot = Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.CacheDirectory, "mobil-dwg", "open");
            Directory.CreateDirectory(cacheRoot);
            var sentinelPath = Path.Combine(cacheRoot, $"a25_sentinel_{Guid.NewGuid():N}.part");
            await File.WriteAllBytesAsync(sentinelPath, new byte[] { 0xCA, 0xD0, 0xA2, 0x5F });
            bool sentinelExists = File.Exists(sentinelPath);
            var cache = new SafeCadFileCache(cacheRoot);
            cache.PurgeAll();
            bool sentinelGone = !File.Exists(sentinelPath);
            if (sentinelExists && sentinelGone)
            {
                Log.Info(Tag, "A25_CACHE_PURGE_PASS");
                results.Add("B3=PASS");
            }
            else
            {
                Log.Warn(Tag, $"A25_CACHE_PURGE_WARN exists={sentinelExists} gone={sentinelGone}");
                results.Add("B3=WARN");
            }
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"A25_CACHE_PURGE_FAIL: {ex}");
            results.Add($"B3=FAIL:{ex.GetType().Name}");
        }

        // B4: Render Error Surface
        try
        {
            var scene = BuildSyntheticScene();
            var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
            var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "a25_err.dxf");
            var session = new CadViewerSession(metadata, scene, layoutManager);
            session.Dispose();
            bool errorSurfaced = false;
            string errorType = "none";
            try
            {
                using var surface = new SkiaBitmapRenderSurface(1080, 1920);
                await session.RenderAsync(surface);
            }
            catch (Exception ex)
            {
                errorSurfaced = true;
                errorType = ex.GetType().Name;
            }
            if (errorSurfaced)
            {
                Log.Info(Tag, $"A25_RENDER_ERROR_SURFACE_PASS errorType={errorType}");
                results.Add("B4=PASS");
            }
            else
            {
                Log.Warn(Tag, "A25_RENDER_ERROR_SURFACE_WARN");
                results.Add("B4=WARN_NO_ERROR");
            }
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"A25_RENDER_ERROR_SURFACE_FAIL: {ex}");
            results.Add($"B4=FAIL:{ex.GetType().Name}");
        }

        // B5: Coordinator Reset After Error
        try
        {
            var cacheRoot = Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.CacheDirectory, "mobil-dwg", "open");
            var coordinator = new CadFileOpenCoordinator(
                new MobilDwg.Cad.AcadSharp.AcadSharpDocumentReader(),
                new SafeCadFileCache(cacheRoot));
            await coordinator.ResetCurrentSessionAsync();
            bool sessionIsNull = coordinator.CurrentSession is null;
            bool cancelResult = coordinator.CancelCurrentRequest();
            await coordinator.DisposeAsync();
            if (sessionIsNull && !cancelResult)
            {
                Log.Info(Tag, "A25_COORDINATOR_RESET_PASS");
                results.Add("B5=PASS");
            }
            else
            {
                Log.Warn(Tag, $"A25_COORDINATOR_RESET_WARN sessionIsNull={sessionIsNull} cancelResult={cancelResult}");
                results.Add("B5=WARN");
            }
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"A25_COORDINATOR_RESET_FAIL: {ex}");
            results.Add($"B5=FAIL:{ex.GetType().Name}");
        }

        // Final proof-of-life render
        byte[] png;
        string pngSha256;
        try
        {
            var scene = BuildSyntheticScene();
            var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
            var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "a25_final.dxf");
            var session = new CadViewerSession(metadata, scene, layoutManager, 800, 600);
            session.ZoomToFit();
            using var surface = new SkiaBitmapRenderSurface(800, 600);
            await session.RenderAsync(surface);
            png = surface.EncodePng();
            session.Dispose();
            using var sha = SHA256.Create();
            pngSha256 = Convert.ToHexString(sha.ComputeHash(png)).ToLowerInvariant();
            Log.Info(Tag, $"A25_PROOF_PNG_READY bytes={png.Length} sha256={pngSha256}");
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"A25_PROOF_PNG_FAIL: {ex}");
            png = Array.Empty<byte>();
            pngSha256 = "error";
        }

        var blockerSummary = string.Join("|", results);
        bool allPass = results.All(r => r.EndsWith("=PASS", StringComparison.Ordinal));
        var marker = allPass ? "ANDROID_STAGE25_BETA_BLOCKER_PASS" : "ANDROID_STAGE25_BETA_BLOCKER_WARN";
        Log.Info(Tag, $"{marker} blockers={blockerSummary}");
        return new A25ValidationResult(png, pngSha256, blockerSummary, marker);
    }

    private static RenderScene BuildSyntheticScene()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A25-E1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("SYNTHETIC"),
            [
                new LinePrimitive(new WorldPoint2(10, 10), new WorldPoint2(790, 590)),
                new LinePrimitive(new WorldPoint2(790, 10), new WorldPoint2(10, 590)),
                new TextPrimitive("A25 Beta Blocker Gate", new WorldPoint2(400, 300), 20.0),
            ]));
        return assembler.Build();
    }
}
#endif
