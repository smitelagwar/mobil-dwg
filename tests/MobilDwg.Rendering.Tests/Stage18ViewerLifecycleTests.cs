using System.Runtime.CompilerServices;
using System.Text.Json;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Rendering;
using MobilDwg.Core.Storage;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

internal static class Stage18ViewerLifecycleTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestRecentFilesManagerCapacityAndLruPromotion();
        TestRecentFilesSerializationAndRestoration();
        TestRecentFilesRemovalAndClear();
        TestLogRedactorSanitizesPathsAndUris();
        TestViewerSessionInitializationAndBounds();
        TestViewerSessionZoomAndPan();
        TestViewerSessionResizeViewport();
        TestViewerSessionToggleLayerVisibilityWithoutReparse();
        TestViewerSessionSwitchLayoutWithoutReparse();
        TestViewerSessionTrimMemory();
        TestViewerLifecycleSemanticSnapshotDeterminism();
        TestSkiaRenderSessionProducesExpectedPixels();

        Console.WriteLine("STAGE18_VIEWER_LIFECYCLE_TESTS_PASS");
    }

    private static void TestRecentFilesManagerCapacityAndLruPromotion()
    {
        var manager = new RecentFilesManager();
        for (var i = 1; i <= 12; i++)
        {
            manager.AddOrPromote(new RecentFileEntry(
                $"drawing_{i:D2}.dwg",
                $"/sdcard/Download/drawing_{i:D2}.dwg",
                i * 1024,
                DateTimeOffset.UtcNow.AddMinutes(i)));
        }

        Assert(manager.Entries.Count == 10, "Recent files capacity must be strictly capped at 10");
        Assert(manager.Entries[0].DisplayName == "drawing_12.dwg", "Most recent must be at index 0");
        Assert(manager.Entries[9].DisplayName == "drawing_03.dwg", "Oldest beyond 10 must have been dropped");

        // Re-promote drawing_05
        manager.AddOrPromote(new RecentFileEntry(
            "drawing_05.dwg",
            "/sdcard/Download/drawing_05.dwg",
            5 * 1024,
            DateTimeOffset.UtcNow.AddMinutes(20)));

        Assert(manager.Entries.Count == 10, "Re-promoting must not increase capacity beyond 10");
        Assert(manager.Entries[0].DisplayName == "drawing_05.dwg", "Re-promoted file must move to index 0");
    }

    private static void TestRecentFilesSerializationAndRestoration()
    {
        var manager = new RecentFilesManager();
        manager.AddOrPromote(new RecentFileEntry("arch.dwg", "/storage/arch.dwg", 2048, DateTimeOffset.UtcNow));
        manager.AddOrPromote(new RecentFileEntry("mep.dxf", "/storage/mep.dxf", 4096, DateTimeOffset.UtcNow));

        var json = manager.SerializeJson();
        Assert(!string.IsNullOrWhiteSpace(json), "Serialized JSON must not be empty");

        var restored = RecentFilesManager.DeserializeJson(json);
        Assert(restored.Entries.Count == 2, "Restored entries count must match");
        Assert(restored.Entries[0].DisplayName == "mep.dxf", "Order must be preserved across serialization");
        Assert(restored.Entries[1].DisplayName == "arch.dwg", "Order must be preserved across serialization");

        // Corrupt JSON must not throw and return clean manager
        var fromCorrupt = RecentFilesManager.DeserializeJson("{\"invalid\": true, broken}");
        Assert(fromCorrupt.Entries.Count == 0, "Corrupt json must yield empty manager safely");
    }

    private static void TestRecentFilesRemovalAndClear()
    {
        var manager = new RecentFilesManager();
        manager.AddOrPromote(new RecentFileEntry("doc1.dwg", "/path/doc1.dwg", 100, DateTimeOffset.UtcNow));
        manager.AddOrPromote(new RecentFileEntry("doc2.dwg", "/path/doc2.dwg", 200, DateTimeOffset.UtcNow));

        var removed = manager.Remove("/path/doc1.dwg");
        Assert(removed, "Remove existing path must return true");
        Assert(manager.Entries.Count == 1, "Entries count after remove must be 1");
        Assert(manager.Entries[0].DisplayName == "doc2.dwg", "Remaining item must be doc2");

        manager.Clear();
        Assert(manager.Entries.Count == 0, "Clear must empty entries");
    }

    private static void TestLogRedactorSanitizesPathsAndUris()
    {
        var path = @"C:\Users\SecretUser\SensitiveProjects\Building_A.dwg";
        var redactedPath = LogRedactor.RedactPath(path);
        Assert(redactedPath == "Building_A.dwg", $"Path must only retain file name: got {redactedPath}");

        var unixPath = "/data/user/0/com.secret.app/files/vault/SitePlan.dxf";
        var redactedUnix = LogRedactor.RedactPath(unixPath);
        Assert(redactedUnix == "SitePlan.dxf", $"Unix path must only retain file name: got {redactedUnix}");

        var uri = "content://com.android.providers.media.documents/document/document%3A10023";
        var redactedUri = LogRedactor.RedactUri(uri);
        Assert(redactedUri.StartsWith("content://", StringComparison.Ordinal), "URI redaction must retain scheme");
        Assert(!redactedUri.Contains("SecretUser", StringComparison.Ordinal), "Must not leak user info");
    }

    private static void TestViewerSessionInitializationAndBounds()
    {
        var (session, _) = CreateTestSession();
        Assert(session.Metadata.DisplayName == "SampleFloor.dwg", "Session metadata name");
        Assert(session.ActiveLayoutName == "Model", "Default layout must be Model");
        Assert(session.ViewportPixelWidth == 1080, "Initial viewport width");
        Assert(session.ViewportPixelHeight == 1920, "Initial viewport height");
        Assert(session.Camera.IsValid, "Initial camera must be valid");
        Assert(session.Camera.Center.X > 0, "Initial camera center X must be positive");
    }

    private static void TestViewerSessionZoomAndPan()
    {
        var (session, _) = CreateTestSession();
        var initialScale = session.Camera.WorldUnitsPerPixel;
        var initialCenter = session.Camera.Center;

        // Zoom 2x
        session.Zoom(2.0, 540, 960);
        Assert(session.Camera.WorldUnitsPerPixel < initialScale, "Zooming in must decrease world units per pixel");

        // Pan by 100 screen pixels
        session.Pan(100, -50);
        Assert(session.Camera.Center.X != initialCenter.X, "Pan must update center X");
        Assert(session.Camera.Center.Y != initialCenter.Y, "Pan must update center Y");

        // Zoom to fit
        session.ZoomToFit();
        Assert(session.Camera.IsValid, "ZoomToFit must keep camera valid");
    }

    private static void TestViewerSessionResizeViewport()
    {
        var (session, _) = CreateTestSession();
        var centerBefore = session.Camera.Center;

        // Simulate screen orientation rotation from Portrait (1080x1920) to Landscape (1920x1080)
        session.ResizeViewport(1920, 1080);
        Assert(session.ViewportPixelWidth == 1920, "New width must be 1920");
        Assert(session.ViewportPixelHeight == 1080, "New height must be 1080");
        Assert(session.Camera.Center == centerBefore, "Center must remain stable across resize");
    }

    private static void TestViewerSessionToggleLayerVisibilityWithoutReparse()
    {
        var (session, _) = CreateTestSession();
        var initialWallVisible = session.LayerTable.GetLayer("WALLS").IsVisible;
        Assert(initialWallVisible, "WALLS layer must initially be visible");

        session.ToggleLayerVisibility("WALLS");
        var afterToggle = session.LayerTable.GetLayer("WALLS").IsVisible;
        Assert(!afterToggle, "WALLS layer must now be hidden");

        session.SetLayerVisibility("WALLS", true);
        Assert(session.LayerTable.GetLayer("WALLS").IsVisible, "WALLS layer must be visible again");
    }

    private static void TestViewerSessionSwitchLayoutWithoutReparse()
    {
        var (session, _) = CreateTestSession();
        Assert(session.ActiveLayoutName == "Model", "Initial layout");

        session.SwitchLayout("Sheet-A1");
        Assert(session.ActiveLayoutName == "Sheet-A1", "Active layout must switch to Sheet-A1");
        Assert(session.Camera.IsValid, "Camera must adjust to Sheet-A1 bounds");

        session.SwitchLayout("Model");
        Assert(session.ActiveLayoutName == "Model", "Active layout must switch back to Model");
    }

    private static void TestViewerSessionTrimMemory()
    {
        var (session, _) = CreateTestSession();
        session.OnTrimMemory();
        Assert(session.Camera.IsValid, "Session must stay intact after OnTrimMemory");
    }

    private static void TestViewerLifecycleSemanticSnapshotDeterminism()
    {
        var (session1, recent1) = CreateTestSession();
        var (session2, recent2) = CreateTestSession();

        var snap1 = ViewerLifecycleSemanticSnapshot.Create(session1, recent1);
        var snap2 = ViewerLifecycleSemanticSnapshot.Create(session2, recent2);

        Assert(snap1 == snap2, "Deterministic snapshot must be identical for identical state");
        var hash1 = ViewerLifecycleSemanticSnapshot.ComputeSha256(snap1);
        var hash2 = ViewerLifecycleSemanticSnapshot.ComputeSha256(snap2);
        Assert(hash1 == hash2, "Snapshot hashes must match");
        Assert(snap1.Contains("schema=viewer-lifecycle/v1", StringComparison.Ordinal), "Must contain correct schema header");
    }

    private static void TestSkiaRenderSessionProducesExpectedPixels()
    {
        var (session, _) = CreateTestSession();
        using var surface = new SkiaBitmapRenderSurface(session.ViewportPixelWidth, session.ViewportPixelHeight);

        session.RenderAsync(surface).AsTask().GetAwaiter().GetResult();
        var png = surface.EncodePng();
        Assert(png.Length > 1000, "Rendered PNG must contain real image data");

        // Inspect pixel data to confirm non-background pixels
        var nonBg = CountNonBackgroundPixels(surface.Bitmap, SKColors.Black);
        Assert(nonBg > 100, $"Render must produce non-background drawing pixels: got {nonBg}");
    }

    private static (CadViewerSession Session, RecentFilesManager Recent) CreateTestSession()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        var layerTable = new LayerTable(
        [
            new LayerDefinition("0", CadColor.FromAci(7), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("WALLS", CadColor.FromAci(1), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("DOORS", CadColor.FromAci(2), CadLinetype.Continuous, CadLineweight.Default)
        ]);
        assembler.SetLayerTable(layerTable);

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("W1"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [
                new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0)),
                new LinePrimitive(new WorldPoint2(100, 0), new WorldPoint2(100, 100)),
                new LinePrimitive(new WorldPoint2(100, 100), new WorldPoint2(0, 100)),
                new LinePrimitive(new WorldPoint2(0, 100), new WorldPoint2(0, 0))
            ]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("D1"),
            new RenderLayerToken("DOORS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ARC"),
            [
                new ArcPrimitive(new WorldPoint2(50, 0), radius: 20, startRadians: 0, sweepRadians: Math.PI / 2)
            ]));

        var scene = assembler.Build();

        var sheetLayout = new CadLayoutDefinition(
            "Sheet-A1",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 420, 297),
            paperEntities:
            [
                new RenderSceneEntity(
                    new RenderEntityId("BORDER"),
                    new RenderLayerToken("0"),
                    new RenderStyleToken("BYLAYER"),
                    new RenderSourceReference("POLYLINE"),
                    [
                        new LinePrimitive(new WorldPoint2(10, 10), new WorldPoint2(410, 10)),
                        new LinePrimitive(new WorldPoint2(410, 10), new WorldPoint2(410, 287)),
                        new LinePrimitive(new WorldPoint2(410, 287), new WorldPoint2(10, 287)),
                        new LinePrimitive(new WorldPoint2(10, 287), new WorldPoint2(10, 10))
                    ])
            ],
            viewports:
            [
                new CadLayoutViewport(
                    "VP-1",
                    paperCenter: new WorldPoint2(210, 148),
                    paperWidth: 200,
                    paperHeight: 150,
                    viewCenter: new WorldPoint2(50, 50),
                    viewHeight: 120)
            ]);

        var layoutManager = new CadLayoutManager(scene, [sheetLayout]);

        var metadata = new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "SampleFloor.dwg");

        var session = new CadViewerSession(
            metadata,
            scene,
            layoutManager,
            initialPixelWidth: 1080,
            initialPixelHeight: 1920);

        var recent = new RecentFilesManager();
        recent.AddOrPromote(new RecentFileEntry("SampleFloor.dwg", "/sdcard/SampleFloor.dwg", 10240, DateTimeOffset.UtcNow));

        return (session, recent);
    }

    private static int CountNonBackgroundPixels(SKBitmap bitmap, SKColor background)
    {
        var count = 0;
        for (var y = 0; y < bitmap.Height; y += 4)
        {
            for (var x = 0; x < bitmap.Width; x += 4)
            {
                var p = bitmap.GetPixel(x, y);
                if (p.Alpha > 0 && (p.Red != background.Red || p.Green != background.Green || p.Blue != background.Blue))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Assertion failed: {message}");
        }
    }
}
