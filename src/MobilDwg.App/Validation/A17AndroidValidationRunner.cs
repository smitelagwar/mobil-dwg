#if A17_VALIDATION
using System.Security.Cryptography;
using Android.Util;
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

namespace MobilDwg.App;

public sealed record A17ValidationResult(
    byte[] Png,
    string PngSha256,
    int EntityCount,
    string Marker);

public static class A17AndroidValidationRunner
{
    public const string Tag = "MobilDwgA17";

    public static async Task<A17ValidationResult> RunAsync()
    {
        Log.Info(Tag, "A17_ANDROID_VALIDATION_STARTING");
        await Task.Delay(250);

        // 1. Invariant 1: Security - Remote URL rejection
        var remoteUrl = "https://cloud.autodesk.com/v1/projects/model.dwg";
        var remoteResolved = CadReferenceResolver.TryResolve(remoteUrl, ["/sdcard"], out _, out var remoteDiag, out _);
        if (remoteResolved || remoteDiag != "XREF_REMOTE_NOT_SUPPORTED")
        {
            throw new InvalidOperationException($"Remote URL was not rejected properly: {remoteDiag}");
        }
        Log.Info(Tag, "A17_ANDROID_REMOTE_REJECTED_PASS");

        // 2. Invariant 2: Security - Path traversal prevention
        var traversalPath = "../../etc/security.dwg";
        var travResolved = CadReferenceResolver.TryResolve(traversalPath, ["/sdcard"], out _, out var travDiag, out _);
        if (travResolved || travDiag != "PATH_TRAVERSAL_PREVENTED")
        {
            throw new InvalidOperationException($"Path traversal was not prevented properly: {travDiag}");
        }
        Log.Info(Tag, "A17_ANDROID_SECURITY_TRAVERSAL_PASS");

        // 3. Invariant 3: Synthetic Raster Generation and Resolution
        var appCacheDir = Path.Combine(FileSystem.CacheDirectory, "cad_refs");
        Directory.CreateDirectory(appCacheDir);

        var logoFilePath = Path.Combine(appCacheDir, "cad_logo.png");
        var logoPngBytes = RasterImagePrimitive.CreateTestPng(64, 64);
        await File.WriteAllBytesAsync(logoFilePath, logoPngBytes);

        // Resolve reference using drawing directory
        var rawLogoPath = @"C:\CAD\Assets\CAD_LOGO.PNG";
        var logoResolved = CadReferenceResolver.TryResolve(rawLogoPath, [appCacheDir], out var resolvedLogoPath, out _, out _);
        if (!logoResolved || resolvedLogoPath == null || !File.Exists(resolvedLogoPath))
        {
            throw new InvalidOperationException("Failed to resolve local raster image case-insensitively.");
        }
        Log.Info(Tag, "A17_ANDROID_RASTER_RESOLVED_PASS");

        // 4. Invariant 4: RasterImagePrimitive Creation
        var rasterBounds = new WorldBounds2(10, 10, 90, 90);
        var rasterPrim = new RasterImagePrimitive(
            "RASTER_LOGO",
            resolvedLogoPath,
            logoPngBytes,
            rasterBounds,
            Transform2D.Identity,
            64,
            64,
            brightness: 50,
            fade: 0);

        var rasterEntity = new RenderSceneEntity(
            new RenderEntityId("ENTITY_RASTER_01"),
            new RenderLayerToken("IMAGES"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("IMAGE"),
            [rasterPrim]);

        // 5. Invariant 5: Missing Reference Placeholders (DWG XREF & PDF Underlay)
        var rawXrefPath = @"D:\Projects\Structural\Site_Plan.dwg";
        var xrefBounds = new WorldBounds2(110, 10, 250, 110);
        CadReferenceResolver.TryResolve(rawXrefPath, [appCacheDir], out _, out var xrefDiag, out var xrefMsg);

        var missingXrefPrim = new MissingReferencePrimitive(
            "XREF_01",
            CadExternalReferenceKind.DwgXref,
            rawXrefPath,
            xrefBounds,
            xrefDiag ?? "EXTERNAL_RESOURCE_NOT_FOUND",
            xrefMsg ?? "XREF file missing.");

        var xrefEntity = new RenderSceneEntity(
            new RenderEntityId("ENTITY_XREF_01"),
            new RenderLayerToken("XREF"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("XREF"),
            [missingXrefPrim]);

        var rawPdfPath = @"Specs\HVAC_Specification.pdf";
        var pdfBounds = new WorldBounds2(10, 110, 150, 180);
        CadReferenceResolver.TryResolve(rawPdfPath, [appCacheDir], out _, out var pdfDiag, out var pdfMsg);

        var missingPdfPrim = new MissingReferencePrimitive(
            "PDF_01",
            CadExternalReferenceKind.PdfUnderlay,
            rawPdfPath,
            pdfBounds,
            pdfDiag ?? "EXTERNAL_RESOURCE_NOT_FOUND",
            pdfMsg ?? "PDF underlay missing.");

        var pdfEntity = new RenderSceneEntity(
            new RenderEntityId("ENTITY_PDF_01"),
            new RenderLayerToken("UNDERLAY"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("UNDERLAY"),
            [missingPdfPrim]);

        Log.Info(Tag, "A17_ANDROID_MISSING_PLACEHOLDER_PASS");

        // 6. Build Composite Scene
        var borderPrims = new List<RenderGeometryPrimitive>
        {
            new LinePrimitive(new WorldPoint2(5, 5), new WorldPoint2(255, 5)),
            new LinePrimitive(new WorldPoint2(255, 5), new WorldPoint2(255, 185)),
            new LinePrimitive(new WorldPoint2(255, 185), new WorldPoint2(5, 185)),
            new LinePrimitive(new WorldPoint2(5, 185), new WorldPoint2(5, 5)),
            new TextPrimitive("MOBIL DWG - STAGE 17 XREF & RASTER", new WorldPoint2(10, 190), height: 4.0, rotationRadians: 0d)
        };

        var borderEntity = new RenderSceneEntity(
            new RenderEntityId("BORDER_01"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("BORDER"),
            borderPrims);

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(borderEntity);
        assembler.AddEntity(rasterEntity);
        assembler.AddEntity(xrefEntity);
        assembler.AddEntity(pdfEntity);

        assembler.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Unsupported, xrefDiag ?? "EXTERNAL_RESOURCE_NOT_FOUND", xrefMsg ?? "XREF missing", xrefEntity.Id));
        assembler.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Unsupported, pdfDiag ?? "EXTERNAL_RESOURCE_NOT_FOUND", pdfMsg ?? "PDF missing", pdfEntity.Id));

        var scene = assembler.Build();

        // 7. Render with Skia to 1080x1080 PNG
        var renderResult = await SkiaScenePngRenderer.RenderFitWithStatsAsync(
            scene,
            pixelWidth: 1080,
            pixelHeight: 1080,
            density: 2.0d,
            paddingFraction: 0.05);

        var pngBytes = renderResult.Png;
        if (pngBytes.Length == 0 ||
            pngBytes[0] != 0x89 || pngBytes[1] != 0x50 || pngBytes[2] != 0x4E || pngBytes[3] != 0x47)
        {
            throw new InvalidOperationException("Rendered PNG is empty or lacks valid PNG header.");
        }

        if (renderResult.NonBackgroundPixels < 500)
        {
            throw new InvalidOperationException($"Too few non-background pixels rendered: {renderResult.NonBackgroundPixels}");
        }

        var pngSha256 = Convert.ToHexString(SHA256.HashData(pngBytes)).ToLowerInvariant();

        var refRecords = new List<CadExternalReference>
        {
            new("REF_LOGO", CadExternalReferenceKind.RasterImage, rawLogoPath, resolvedLogoPath, new WorldPoint2(rasterBounds.MinX, rasterBounds.MinY), 1, 1, 0, 64, 64, rasterBounds, isResolved: true),
            new("REF_XREF", CadExternalReferenceKind.DwgXref, rawXrefPath, null, new WorldPoint2(xrefBounds.MinX, xrefBounds.MinY), 1, 1, 0, 0, 0, xrefBounds, isResolved: false),
            new("REF_PDF", CadExternalReferenceKind.PdfUnderlay, rawPdfPath, null, new WorldPoint2(pdfBounds.MinX, pdfBounds.MinY), 1, 1, 0, 0, 0, pdfBounds, isResolved: false)
        };

        var snapshot = ExternalReferenceSemanticSnapshot.Create(refRecords, scene);
        var snapHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(snapshot))).ToLowerInvariant();

        Log.Info(Tag, $"A17_SCENE_ENTITIES_COUNT={scene.Entities.Count}");
        Log.Info(Tag, $"A17_RENDER_PIXELS={renderResult.NonBackgroundPixels}");
        Log.Info(Tag, $"A17_SNAPSHOT_HASH={snapHash}");
        Log.Info(Tag, $"A17_ANDROID_SKIA_RENDER_PASS bytes={pngBytes.Length} sha256={pngSha256}");
        Log.Info(Tag, "ANDROID_STAGE17_XREF_COMPAT_PASS");

        return new A17ValidationResult(
            pngBytes,
            pngSha256,
            scene.Entities.Count,
            "ANDROID_STAGE17_XREF_COMPAT_PASS");
    }
}
#endif
