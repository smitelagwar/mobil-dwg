using System.Runtime.CompilerServices;
using MobilDwg.Core.Documents;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Rendering.Tests;

/// <summary>
/// AŞAMA 25 — Android Beta ve Blocker Düzeltmeleri
/// Host-side (net10.0) contract tests verifying B2 and B4 blocker fixes within
/// the MobilDwg.Rendering layer. B3 (SafeCadFileCache.PurgeAll) and B5
/// (CadFileOpenCoordinator.ResetCurrentSessionAsync) are verified on-device via
/// A25AndroidValidationRunner because those types are in MobilDwg.App which
/// targets net10.0-android36.0 and cannot be referenced from net10.0 host tests.
/// </summary>
public static class Stage25BetaBlockerTests
{
    [ModuleInitializer]
    public static void Run()
    {
        TestDisposeChain_SessionThrowsOdeAfterDispose_B2();
        TestDisposeChain_GcCleanAfterSessionDispose_B2();
        TestRenderError_SurfacesExceptionOnDisposedSession_B4();
        TestRenderError_SurfacesBitmapOnLiveSession_B4Positive();

        Console.WriteLine("STAGE25_BETA_BLOCKER_TESTS_PASS");
    }

    // ─── B2: Dispose Chain — ObjectDisposedException ──────────────────────────
    private static void TestDisposeChain_SessionThrowsOdeAfterDispose_B2()
    {
        var scene = BuildSyntheticScene();
        var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "b2_ode.dxf");
        var session = new CadViewerSession(metadata, scene, layoutManager, 800, 600);
        session.ZoomToFit();

        // Render before dispose — must succeed
        using var surfacePre = new SkiaBitmapRenderSurface(800, 600);
        session.RenderAsync(surfacePre).AsTask().GetAwaiter().GetResult();

        session.Dispose();

        bool threwOde = false;
        try { session.Pan(1, 1); }
        catch (ObjectDisposedException) { threwOde = true; }

        if (!threwOde)
        {
            throw new InvalidOperationException(
                "B2 FAIL: CadViewerSession.Pan did not throw ObjectDisposedException after Dispose().");
        }

        Console.WriteLine("STAGE25_DISPOSE_CHAIN_ODE_PASS");
    }

    // ─── B2: Dispose Chain — GC clean ────────────────────────────────────────
    private static void TestDisposeChain_GcCleanAfterSessionDispose_B2()
    {
        for (var i = 0; i < 5; i++)
        {
            var scene = BuildSyntheticScene();
            var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
            var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", $"b2_gc_{i}.dxf");
            var session = new CadViewerSession(metadata, scene, layoutManager, 400, 300);
            using var surface = new SkiaBitmapRenderSurface(400, 300);
            session.RenderAsync(surface).AsTask().GetAwaiter().GetResult();
            session.Dispose();
        }

        // GC must run without exceptions — verifies no dangling finalizer
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        Console.WriteLine("STAGE25_DISPOSE_CHAIN_GC_PASS");
    }

    // ─── B4: Render Error Surface — disposed session must throw ───────────────
    private static void TestRenderError_SurfacesExceptionOnDisposedSession_B4()
    {
        var scene = BuildSyntheticScene();
        var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "b4_err.dxf");
        var session = new CadViewerSession(metadata, scene, layoutManager, 400, 300);
        session.Dispose();

        bool errorSurfaced = false;
        try
        {
            using var surface = new SkiaBitmapRenderSurface(400, 300);
            session.RenderAsync(surface).AsTask().GetAwaiter().GetResult();
        }
        catch (ObjectDisposedException) { errorSurfaced = true; }
        catch (Exception ex)
        {
            errorSurfaced = true;
            Console.WriteLine($"STAGE25_RENDER_ERROR_SURFACE_NOTE type={ex.GetType().Name}");
        }

        if (!errorSurfaced)
        {
            throw new InvalidOperationException(
                "B4 FAIL: RenderAsync on disposed session did not surface any error.");
        }

        Console.WriteLine("STAGE25_RENDER_ERROR_SURFACE_PASS");
    }

    // ─── B4: Positive — render on live session produces non-empty bitmap ──────
    private static void TestRenderError_SurfacesBitmapOnLiveSession_B4Positive()
    {
        var scene = BuildSyntheticScene();
        var layoutManager = new CadLayoutManager(scene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dxf, "AC1015", "b4_live.dxf");
        var session = new CadViewerSession(metadata, scene, layoutManager, 600, 400);
        session.ZoomToFit();

        using var surface = new SkiaBitmapRenderSurface(600, 400);
        session.RenderAsync(surface).AsTask().GetAwaiter().GetResult();

        var png = surface.EncodePng();
        if (png.Length == 0)
        {
            throw new InvalidOperationException("B4 FAIL: RenderAsync produced empty PNG on live session.");
        }

        session.Dispose();
        Console.WriteLine($"STAGE25_RENDER_POSITIVE_PASS bytes={png.Length}");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static RenderScene BuildSyntheticScene()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A25-E1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("SYNTHETIC"),
            [
                new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 100)),
                new LinePrimitive(new WorldPoint2(100, 0), new WorldPoint2(0, 100)),
                new TextPrimitive("A25 Beta Blocker", new WorldPoint2(50, 50), 8.0),
            ]));
        return assembler.Build();
    }
}
