#if A19_VALIDATION
using System.Security.Cryptography;
using System.Text;
using Android.Util;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Guards;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.App;

public sealed record A19ValidationResult(
    byte[] Png,
    string PngSha256,
    string PreflightSummary,
    string BudgetSummary,
    string Marker);

public static class A19AndroidValidationRunner
{
    public const string Tag = "MobilDwgA19";

    public static async Task<A19ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A19_ANDROID_VALIDATION_STARTING");
        await Task.Delay(250);

        // 1. Preflight Inspector Tests on Android
        byte[] validDwg = Encoding.ASCII.GetBytes("AC1032\0\0\0\0\0\0\0\0\0\0");
        using (var ms = new MemoryStream(validDwg))
        {
            var r = CadPreflightInspector.Inspect(ms, "valid.dwg");
            if (r.Status != CadPreflightStatus.Valid || r.Format != CadFormat.Dwg)
            {
                throw new InvalidOperationException($"A19 preflight valid DWG failed: {r.DiagnosticCode}");
            }
        }

        byte[] fakeZip = [(byte)'P', (byte)'K', 0x03, 0x04, 0x14, 0x00, 0x00, 0x00];
        using (var ms = new MemoryStream(fakeZip))
        {
            var r = CadPreflightInspector.Inspect(ms, "test.zip");
            if (r.Status != CadPreflightStatus.ForeignFormat || r.DiagnosticCode != "CAD_FOREIGN_FORMAT_ZIP_ARCHIVE")
            {
                throw new InvalidOperationException($"A19 preflight foreign zip failed: {r.DiagnosticCode}");
            }
        }

        using (var ms = new MemoryStream(Array.Empty<byte>()))
        {
            var r = CadPreflightInspector.Inspect(ms, "empty.dwg");
            if (r.Status != CadPreflightStatus.EmptyOrTruncated)
            {
                throw new InvalidOperationException($"A19 preflight empty failed: {r.DiagnosticCode}");
            }
        }

        Log.Info(Tag, "A19_ANDROID_PREFLIGHT_PASS");

        // 2. Resource Budget Tests on Android
        var budget = new CadResourceBudget { MaxFileSizeBytes = 100 * 1024 * 1024, MaxEntities = 10_000 };
        var guard = new CadBudgetGuard(budget);

        if (guard.CheckFileSize(200 * 1024 * 1024, out var diagSize))
        {
            throw new InvalidOperationException("A19 file size budget failed to reject 200MB");
        }

        if (guard.CheckEntityCount(15_000, out var diagEntity))
        {
            throw new InvalidOperationException("A19 entity count budget failed to guard 15,000 entities");
        }

        if (guard.CheckRasterDimensions(10_000, 10_000, out var diagRaster))
        {
            throw new InvalidOperationException("A19 raster dimension budget failed to reject 10Kx10K");
        }

        Log.Info(Tag, "A19_ANDROID_BUDGET_GUARDS_PASS");

        // 3. Sanity and Cycle Guards on Android
        double badCoord = double.NaN;
        CadSanityGuards.SanitizeCoordinate(ref badCoord, fallback: 100.0);
        if (double.IsNaN(badCoord) || Math.Abs(badCoord - 100.0) > 1e-6)
        {
            throw new InvalidOperationException("A19 coordinate sanitization failed");
        }

        var refB = new BlockReference("B", new WorldPoint2(0, 0));
        var defA = new BlockDefinition("A", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refB]);
        var refA = new BlockReference("A", new WorldPoint2(0, 0));
        var defB = new BlockDefinition("B", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refA]);

        var expander = new BlockExpander([defA, defB]);
        var expandResult = expander.Expand([new BlockReference("A", new WorldPoint2(0, 0))]);
        if (!expandResult.Diagnostics.Any(d => d.Code == "BLOCK_CYCLE_DETECTED"))
        {
            throw new InvalidOperationException("A19 cycle guard failed to detect cyclic block reference");
        }

        Log.Info(Tag, "A19_ANDROID_SANITY_GUARDS_PASS");

        // 4. Bounded Fuzz Smoke on Android
        var rng = new Random(19);
        int fuzzPasses = 0;
        for (int i = 0; i < 15; i++)
        {
            byte[] fuzzed = new byte[128];
            rng.NextBytes(fuzzed);
            using var ms = new MemoryStream(fuzzed);
            var res = CadPreflightInspector.Inspect(ms, $"fuzz_{i}.dwg");
            if (res == null)
            {
                throw new InvalidOperationException($"A19 fuzz iteration {i} returned null");
            }
            fuzzPasses++;
        }

        Log.Info(Tag, $"A19_ANDROID_FUZZ_PASS count={fuzzPasses}");

        // 5. Semantic Snapshot
        var preflightSample = new CadPreflightResult(
            CadPreflightStatus.Valid,
            CadFormat.Dwg,
            "AC1032",
            "CAD_PREFLIGHT_DWG_VALID",
            "Valid AutoCAD DWG header (AC1032).",
            1048576);

        var snapshot = ResourceGuardsSemanticSnapshot.Create(
            preflightSample,
            budget,
            guard.Diagnostics,
            cycleDetected: true,
            nanSanitized: true,
            fuzzTestPasses: fuzzPasses);

        Log.Info(Tag, $"A19_ANDROID_SNAPSHOT_PASS sha256={snapshot.Sha256Hash}");

        // 6. Render Safe CadViewerSession on Android
        var modelScene = BuildProtectedViewerScene();
        var layoutManager = new CadLayoutManager(modelScene, Array.Empty<CadLayoutDefinition>());
        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "GuardedPlan.dwg");

        var session = new CadViewerSession(
            metadata,
            modelScene,
            layoutManager,
            initialPixelWidth: 1080,
            initialPixelHeight: 1080);

        session.ZoomToFit();

        using var surface = new SkiaBitmapRenderSurface(session.ViewportPixelWidth, session.ViewportPixelHeight);
        await session.RenderAsync(surface);

        var pngBytes = surface.EncodePng();
        var pngSha256 = Convert.ToHexStringLower(SHA256.HashData(pngBytes));

        Log.Info(Tag, $"A19_ANDROID_SKIA_RENDER_PASS bytes={pngBytes.Length} sha256={pngSha256}");
        Log.Info(Tag, "A19_REAL_APP_GUARDS_MARKERS_PASS");
        Log.Info(Tag, "ANDROID_STAGE19_RESOURCE_GUARDS_PASS");

        return new A19ValidationResult(
            pngBytes,
            pngSha256,
            "DWG=Valid, Foreign=Rejected, Empty=Rejected",
            "Budget: File=100MB, Entities=10K, Raster=8K",
            "ANDROID_STAGE19_RESOURCE_GUARDS_PASS");
    }

    private static RenderScene BuildProtectedViewerScene()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        // Drawing outer frame
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("FRAME"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(50, 50), new WorldPoint2(950, 50)),
                new LinePrimitive(new WorldPoint2(950, 50), new WorldPoint2(950, 950)),
                new LinePrimitive(new WorldPoint2(950, 950), new WorldPoint2(50, 950)),
                new LinePrimitive(new WorldPoint2(50, 950), new WorldPoint2(50, 50))
            ]));

        // Shield diamond geometry representing guards
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("GUARD_SHIELD"),
            new RenderLayerToken("GUARDS"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(500, 850), new WorldPoint2(800, 600)),
                new LinePrimitive(new WorldPoint2(800, 600), new WorldPoint2(800, 350)),
                new LinePrimitive(new WorldPoint2(800, 350), new WorldPoint2(500, 150)),
                new LinePrimitive(new WorldPoint2(500, 150), new WorldPoint2(200, 350)),
                new LinePrimitive(new WorldPoint2(200, 350), new WorldPoint2(200, 600)),
                new LinePrimitive(new WorldPoint2(200, 600), new WorldPoint2(500, 850))
            ]));

        // Centered checkmark / guard core
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("GUARD_CHECKMARK"),
            new RenderLayerToken("CORE"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(350, 500), new WorldPoint2(470, 380)),
                new LinePrimitive(new WorldPoint2(470, 380), new WorldPoint2(650, 620)),
                new ArcPrimitive(new WorldPoint2(500, 500), 60, 0, Math.PI * 2)
            ]));

        return assembler.Build();
    }
}
#endif
