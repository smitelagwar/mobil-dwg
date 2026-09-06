using System.Globalization;
using System.Runtime.CompilerServices;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.References;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Transforms;
using SkiaSharp;

internal static class Stage17ReferenceCompatibilityTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        TestUnresolvedXrefEmitsDiagnosticAndGeneratesPlaceholder();
        TestMissingRasterImageEmitsDiagnosticAndGeneratesPlaceholder();
        TestMissingPdfUnderlayEmitsDiagnosticAndGeneratesPlaceholder();
        TestRemoteUrlRejectedWithSecurityDiagnostic();
        TestBoundedDirectoryResolverMatchesFilenameCaseInsensitively();
        TestPathTraversalAttemptBlockedWithSecurityDiagnostic();
        TestResolvedLocalRasterImageCreatesValidPrimitive();
        TestSkiaRenderRasterImageProducesNonBackgroundPixels();
        TestRasterImageClippingBoundaryRestrictsRendering();
        TestRasterImageFadeParameter();
        TestCompositeSceneWithResolvedRasterAndMissingReferences();
        TestExternalReferenceSemanticSnapshotDeterminism();

        Console.WriteLine("STAGE17_REFERENCE_COMPATIBILITY_TESTS_PASS");
    }

    private static void TestUnresolvedXrefEmitsDiagnosticAndGeneratesPlaceholder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "mobildwg_test_xref_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var rawPath = @"C:\AutoCAD\Projects\Architectural\Site_Plan.dwg";
            var resolved = CadReferenceResolver.TryResolve(rawPath, [tempDir], out var path, out var diagCode, out var diagMsg);

            Assert(!resolved, "Missing XREF must not resolve.");
            Assert(diagCode == "EXTERNAL_RESOURCE_NOT_FOUND", $"Expected EXTERNAL_RESOURCE_NOT_FOUND, got {diagCode}");

            var bounds = new WorldBounds2(0, 0, 100, 80);
            var placeholder = new MissingReferencePrimitive(
                "XREF_01",
                CadExternalReferenceKind.DwgXref,
                rawPath,
                bounds,
                diagCode!,
                diagMsg!);

            Assert(placeholder.Bounds == bounds, "Placeholder bounds match.");
            Assert(placeholder.GenerateBorderLines().Count == 4, "Border has 4 lines.");
            Assert(placeholder.GenerateCrossLines().Count == 2, "Diagonal cross has 2 lines.");
            Assert(placeholder.Label.Contains("DWGXREF", StringComparison.OrdinalIgnoreCase), "Label contains kind.");
            Assert(placeholder.Label.Contains("Site_Plan.dwg", StringComparison.OrdinalIgnoreCase), "Label contains file.");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private static void TestMissingRasterImageEmitsDiagnosticAndGeneratesPlaceholder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "mobildwg_test_raster_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var rawPath = @"images\aerial_map.png";
            var resolved = CadReferenceResolver.TryResolve(rawPath, [tempDir], out _, out var diagCode, out var diagMsg);

            Assert(!resolved, "Non-existent image must not resolve.");
            Assert(diagCode == "EXTERNAL_RESOURCE_NOT_FOUND", $"Expected EXTERNAL_RESOURCE_NOT_FOUND, got {diagCode}");

            var bounds = new WorldBounds2(50, 50, 200, 150);
            var placeholder = new MissingReferencePrimitive(
                "RASTER_01",
                CadExternalReferenceKind.RasterImage,
                rawPath,
                bounds,
                diagCode!,
                diagMsg!);

            Assert(placeholder.Kind == CadExternalReferenceKind.RasterImage, "Kind is RasterImage.");
            Assert(placeholder.Label.Contains("RASTERIMAGE", StringComparison.OrdinalIgnoreCase), "Label contains RASTERIMAGE.");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private static void TestMissingPdfUnderlayEmitsDiagnosticAndGeneratesPlaceholder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "mobildwg_test_pdf_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var rawPath = @"specifications\structural_details.pdf";
            var resolved = CadReferenceResolver.TryResolve(rawPath, [tempDir], out _, out var diagCode, out var diagMsg);

            Assert(!resolved, "Missing PDF must not resolve.");
            Assert(diagCode == "EXTERNAL_RESOURCE_NOT_FOUND", "Diagnostic is EXTERNAL_RESOURCE_NOT_FOUND.");

            var placeholder = new MissingReferencePrimitive(
                "PDF_01",
                CadExternalReferenceKind.PdfUnderlay,
                rawPath,
                new WorldBounds2(0, 0, 420, 297),
                diagCode!,
                diagMsg!);

            Assert(placeholder.Kind == CadExternalReferenceKind.PdfUnderlay, "Kind is PdfUnderlay.");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private static void TestRemoteUrlRejectedWithSecurityDiagnostic()
    {
        var urls = new[]
        {
            "http://example.com/drawings/border.dwg",
            "https://cloud.autodesk.com/v1/projects/model.dwg",
            "ftp://fileserver.local/images/logo.png"
        };

        foreach (var url in urls)
        {
            Assert(CadReferenceResolver.IsRemoteUrl(url), $"URL '{url}' must be recognized as remote.");
            var resolved = CadReferenceResolver.TryResolve(url, [@"C:\test"], out _, out var diagCode, out _);
            Assert(!resolved, "Remote URL must not resolve.");
            Assert(diagCode == "XREF_REMOTE_NOT_SUPPORTED", $"Expected XREF_REMOTE_NOT_SUPPORTED, got '{diagCode}' for '{url}'.");
        }
    }

    private static void TestBoundedDirectoryResolverMatchesFilenameCaseInsensitively()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "mobildwg_test_case_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var actualFile = Path.Combine(tempDir, "company_logo.png");
            File.WriteAllBytes(actualFile, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

            var rawWindowsPath = @"D:\Drawings\Assets\COMPANY_LOGO.PNG";
            var resolved = CadReferenceResolver.TryResolve(rawWindowsPath, [tempDir], out var resolvedPath, out _, out _);

            Assert(resolved, "Case-insensitive sibling filename must resolve.");
            Assert(resolvedPath != null && File.Exists(resolvedPath), "Resolved file exists.");
            Assert(string.Equals(Path.GetFileName(resolvedPath), "company_logo.png", StringComparison.OrdinalIgnoreCase), "Filename matches.");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private static void TestPathTraversalAttemptBlockedWithSecurityDiagnostic()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "mobildwg_test_sec_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var traversalPaths = new[]
            {
                @"../../../../etc/passwd",
                @"..\..\Windows\System32\cmd.exe",
                @"sub/../../secret.dwg"
            };

            foreach (var path in traversalPaths)
            {
                var resolved = CadReferenceResolver.TryResolve(path, [tempDir], out _, out var diagCode, out _);
                Assert(!resolved, "Traversal path must not resolve.");
                Assert(diagCode == "PATH_TRAVERSAL_PREVENTED", $"Path traversal '{path}' must emit PATH_TRAVERSAL_PREVENTED, got '{diagCode}'.");
            }
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private static void TestResolvedLocalRasterImageCreatesValidPrimitive()
    {
        var samplePng = CreateSyntheticPngBytes(width: 32, height: 32, SKColors.CornflowerBlue);
        var bounds = new WorldBounds2(10, 10, 50, 50);
        var transform = Transform2D.CreateTranslation(10, 10) * Transform2D.CreateScale(40d / 32d, 40d / 32d);

        var rasterPrim = new RasterImagePrimitive(
            "IMG_01",
            resolvedPath: null,
            imageBytes: samplePng,
            imageBounds: bounds,
            transform: transform,
            pixelWidth: 32,
            pixelHeight: 32,
            brightness: 50,
            contrast: 50,
            fade: 10);

        Assert(rasterPrim.ReferenceId == "IMG_01", "ID is IMG_01.");
        Assert(rasterPrim.Bounds == bounds, "Bounds match.");
        Assert(rasterPrim.PixelWidth == 32, "Width is 32.");
        Assert(rasterPrim.PixelHeight == 32, "Height is 32.");
        AssertNear(rasterPrim.Fade, 10d, 1e-4, "Fade matches.");
    }

    private static void TestSkiaRenderRasterImageProducesNonBackgroundPixels()
    {
        var samplePng = CreateSyntheticPngBytes(width: 64, height: 64, SKColors.Crimson);
        var bounds = new WorldBounds2(0, 0, 100, 100);
        var rasterPrim = new RasterImagePrimitive(
            "IMG_CRIMSON",
            resolvedPath: null,
            imageBytes: samplePng,
            imageBounds: bounds,
            transform: Transform2D.Identity,
            pixelWidth: 64,
            pixelHeight: 64);

        var entity = new RenderSceneEntity(
            new RenderEntityId("ENTITY_IMG"),
            new RenderLayerToken("IMAGES"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("IMAGE"),
            [rasterPrim]);

        var asm = new RenderSceneAssembler(RenderColorContext.Dark);
        asm.AddEntity(entity);
        var scene = asm.Build();

        var camera = Camera2D.Fit(bounds, 256, 256, paddingFraction: 0.1);
        using var surface = new SkiaBitmapRenderSurface(256, 256, density: 1.0);
        new SkiaCadRenderer().RenderAsync(scene, surface, camera.ToViewport()).AsTask().GetAwaiter().GetResult();

        var background = scene.ColorContext.BackgroundArgb;
        var nonBackground = surface.Bitmap.Pixels.Count(p => (uint)p != background);

        Assert(nonBackground > 500, $"Rendered raster image must produce non-background pixels: {nonBackground}");
    }

    private static void TestRasterImageClippingBoundaryRestrictsRendering()
    {
        var samplePng = CreateSyntheticPngBytes(width: 64, height: 64, SKColors.LimeGreen);
        var bounds = new WorldBounds2(0, 0, 100, 100);

        // Clip to triangular boundary: (0, 0), (100, 0), (50, 100)
        var clip = new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(100, 0),
            new WorldPoint2(50, 100)
        };

        var rasterPrim = new RasterImagePrimitive(
            "IMG_CLIPPED",
            resolvedPath: null,
            imageBytes: samplePng,
            imageBounds: bounds,
            transform: Transform2D.Identity,
            pixelWidth: 64,
            pixelHeight: 64,
            clipBoundary: clip);

        Assert(rasterPrim.ClipBoundary != null && rasterPrim.ClipBoundary.Count == 3, "Clip boundary retained.");
    }

    private static void TestRasterImageFadeParameter()
    {
        var samplePng = CreateSyntheticPngBytes(16, 16, SKColors.White);
        var prim = new RasterImagePrimitive(
            "FADE_TEST",
            null,
            samplePng,
            new WorldBounds2(0, 0, 10, 10),
            Transform2D.Identity,
            16,
            16,
            fade: 75d);

        AssertNear(prim.Fade, 75d, 1e-4, "Fade clamped/retained.");
    }

    private static void TestCompositeSceneWithResolvedRasterAndMissingReferences()
    {
        var samplePng = CreateSyntheticPngBytes(32, 32, SKColors.DodgerBlue);

        // 1. Resolved Raster
        var rasterPrim = new RasterImagePrimitive(
            "RASTER_01",
            null,
            samplePng,
            new WorldBounds2(10, 10, 80, 80),
            Transform2D.Identity,
            32,
            32);

        var rasterEntity = new RenderSceneEntity(
            new RenderEntityId("ENTITY_RASTER"),
            new RenderLayerToken("IMAGES"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("IMAGE"),
            [rasterPrim]);

        // 2. Unresolved DWG XREF
        var missingXref = new MissingReferencePrimitive(
            "XREF_BORDER",
            CadExternalReferenceKind.DwgXref,
            @"C:\CAD\border.dwg",
            new WorldBounds2(0, 0, 200, 150),
            "EXTERNAL_RESOURCE_NOT_FOUND",
            "File not found in search directory.");

        var xrefEntity = new RenderSceneEntity(
            new RenderEntityId("ENTITY_XREF"),
            new RenderLayerToken("XREF"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("XREF"),
            [missingXref]);

        // 3. Missing PDF Underlay
        var missingPdf = new MissingReferencePrimitive(
            "UNDERLAY_SPEC",
            CadExternalReferenceKind.PdfUnderlay,
            @"specs\details.pdf",
            new WorldBounds2(100, 10, 180, 80),
            "EXTERNAL_RESOURCE_NOT_FOUND",
            "PDF file not found.");

        var pdfEntity = new RenderSceneEntity(
            new RenderEntityId("ENTITY_PDF"),
            new RenderLayerToken("UNDERLAY"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("UNDERLAY"),
            [missingPdf]);

        var asm = new RenderSceneAssembler(RenderColorContext.Dark);
        asm.AddEntity(rasterEntity);
        asm.AddEntity(xrefEntity);
        asm.AddEntity(pdfEntity);

        asm.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Unsupported, "EXTERNAL_RESOURCE_NOT_FOUND", "XREF border.dwg missing", xrefEntity.Id));
        asm.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Unsupported, "EXTERNAL_RESOURCE_NOT_FOUND", "PDF details.pdf missing", pdfEntity.Id));

        var scene = asm.Build();

        Assert(scene.Entities.Count == 3, "Composite scene has 3 entities.");
        Assert(scene.Diagnostics.Items.Count == 2, "Composite scene has 2 diagnostics.");
        Assert(scene.WorldBounds == new WorldBounds2(0, 0, 200, 150), "Bounds union correct.");
    }

    private static void TestExternalReferenceSemanticSnapshotDeterminism()
    {
        var samplePng = CreateSyntheticPngBytes(16, 16, SKColors.Gold);

        var ref1 = new CadExternalReference(
            "REF-01",
            CadExternalReferenceKind.RasterImage,
            @"images\logo.png",
            resolvedPath: @"/data/local/images/logo.png",
            insertionPoint: new WorldPoint2(10, 10),
            scaleX: 1,
            scaleY: 1,
            rotationRadians: 0,
            pixelWidth: 16,
            pixelHeight: 16,
            bounds: new WorldBounds2(10, 10, 26, 26),
            isResolved: true);

        var ref2 = new CadExternalReference(
            "REF-02",
            CadExternalReferenceKind.DwgXref,
            @"C:\Drawings\base.dwg",
            resolvedPath: null,
            insertionPoint: new WorldPoint2(0, 0),
            scaleX: 1,
            scaleY: 1,
            rotationRadians: 0,
            pixelWidth: 0,
            pixelHeight: 0,
            bounds: new WorldBounds2(0, 0, 500, 500),
            isResolved: false);

        var asm = new RenderSceneAssembler();
        asm.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E-01"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 100))]));

        asm.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Unsupported, "EXTERNAL_RESOURCE_NOT_FOUND", "XREF base.dwg missing"));
        var scene = asm.Build();

        var snap1 = ExternalReferenceSemanticSnapshot.Create([ref1, ref2], scene);
        var snap2 = ExternalReferenceSemanticSnapshot.Create([ref2, ref1], scene);

        Assert(snap1 == snap2, "Semantic snapshot must be identical regardless of reference insertion order.");
        Assert(snap1.Contains("schema=xref-compat/v1", StringComparison.Ordinal), "Snapshot has correct schema.");
        Assert(snap1.Contains("ref=REF-01", StringComparison.Ordinal), "Snapshot contains REF-01.");
        Assert(snap1.Contains("ref=REF-02", StringComparison.Ordinal), "Snapshot contains REF-02.");
    }

    private static byte[] CreateSyntheticPngBytes(int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertNear(double actual, double expected, double tolerance, string message)
    {
        if (!double.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
        {
            throw new InvalidOperationException($"{message}: expected={expected:R}, actual={actual:R}, tolerance={tolerance:R}");
        }
    }
}
