#if A10_VALIDATION
using System.Security.Cryptography;
using Android.Util;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Snapshots;

namespace MobilDwg.App;

internal sealed record A10AndroidValidationResult(
    string Marker,
    byte[] Png,
    int NonBackgroundPixels,
    string PngSha256,
    string SemanticSnapshot);

internal static class A10AndroidValidationRunner
{
    internal const string Tag = "MobilDwgA10";

    public static async Task<A10AndroidValidationResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var scene = BuildAcceptanceScene();
        var semantic = P0GeometrySemanticSnapshot.Create(scene);
        Require(semantic.StartsWith("p0-geometry/v1\n", StringComparison.Ordinal), "semantic snapshot version");
        Require(semantic.Contains("primitive=LINE|-30,-20|30,-20", StringComparison.Ordinal), "line semantic golden");
        Require(semantic.Contains("primitive=POLYLINE|0|-28,18,0.5;-10,30,0;8,18,0", StringComparison.Ordinal), "bulge semantic golden");
        Require(semantic.Contains("diagnostic=Dropped|P0_INVALID_GEOMETRY_DROPPED||Invalid source geometry is reported instead of silently rendered.", StringComparison.Ordinal), "controlled invalid geometry diagnostic");
        Log.Info(Tag, "A10_ANDROID_SEMANTIC_GOLDEN_PASS");

        var render = await SkiaScenePngRenderer.RenderFitWithStatsAsync(
            scene,
            pixelWidth: 900,
            pixelHeight: 900,
            density: 1d,
            paddingFraction: 0.08,
            cancellationToken: cancellationToken);

        Require(render.NonBackgroundPixels > 1000, $"expected-content pixel threshold; actual={render.NonBackgroundPixels}");
        Log.Info(Tag, $"A10_ANDROID_EXPECTED_CONTENT_PASS pixels={render.NonBackgroundPixels}");

        var png = render.Png;
        Require(png.Length > 2048, $"PNG byte threshold; actual={png.Length}");
        Require(png.Length >= 8 && png[0] == 0x89 && png[1] == 0x50 && png[2] == 0x4E && png[3] == 0x47, "PNG signature");
        var sha = Convert.ToHexString(SHA256.HashData(png)).ToLowerInvariant();
        Log.Info(Tag, $"A10_ANDROID_PNG_PASS bytes={png.Length} sha256={sha}");

        const string marker = "ANDROID_STAGE10_P0_GEOMETRY_RENDER_PASS";
        Log.Info(Tag, marker);
        Log.Info(Tag, "CLAIM_LIMIT=P0_SYNTHETIC_SCENE_GEOMETRY_RENDERER_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY");
        return new A10AndroidValidationResult(marker, png, render.NonBackgroundPixels, sha, semantic);
    }

    private static RenderScene BuildAcceptanceScene()
    {
        var builder = new RenderSceneAssembler(RenderColorContext.Dark);
        builder.AddEntity(Entity("P0-LINE", "LINE", 1, [new LinePrimitive(new WorldPoint2(-30, -20), new WorldPoint2(30, -20))]));
        builder.AddEntity(Entity("P0-ARC", "ARC", 2, [new ArcPrimitive(new WorldPoint2(0, 0), 18, 0, Math.PI)]));
        builder.AddEntity(Entity("P0-CIRCLE", "CIRCLE", 3, [new ArcPrimitive(new WorldPoint2(30, 15), 10, 0, Math.PI * 2d)]));
        builder.AddEntity(Entity("P0-ELLIPSE", "ELLIPSE", 4, [new EllipsePrimitive(new WorldPoint2(0, -2), 16, 7, Math.PI / 5d)]));
        builder.AddEntity(Entity("P0-POINT", "POINT", 5, [new PointPrimitive(new WorldPoint2(-32, 30))]));
        builder.AddEntity(Entity("P0-LWPOLYLINE", "LWPOLYLINE", 6, [new PolylinePrimitive([
            new PolylineVertex(new WorldPoint2(-28, 18), 0.5),
            new PolylineVertex(new WorldPoint2(-10, 30)),
            new PolylineVertex(new WorldPoint2(8, 18)),
        ])]));
        builder.AddEntity(Entity("P0-SPLINE", "SPLINE", 7, [new SplinePrimitive(2,
            [new WorldPoint2(-25, 0), new WorldPoint2(-5, 35), new WorldPoint2(25, 5)],
            [0d, 0d, 0d, 1d, 1d, 1d])]));
        builder.AddEntity(Entity("P0-SOLID", "SOLID", 8, [new PolygonPrimitive([
            new WorldPoint2(15, 25), new WorldPoint2(35, 25), new WorldPoint2(28, 40), new WorldPoint2(18, 38),
        ])]));
        builder.AddEntity(Entity("P0-TRACE", "TRACE", 9, [new PolygonPrimitive([
            new WorldPoint2(-5, -35), new WorldPoint2(8, -35), new WorldPoint2(10, -28), new WorldPoint2(-8, -28),
        ])]));
        builder.AddEntity(Entity("P0-3DFACE", "3DFACE", 10, [new PolygonPrimitive([
            new WorldPoint2(20, -35), new WorldPoint2(38, -32), new WorldPoint2(34, -22), new WorldPoint2(22, -24),
        ])]));
        builder.AddDiagnostic(new SceneDiagnostic(
            SceneDiagnosticKind.Dropped,
            "P0_INVALID_GEOMETRY_DROPPED",
            "Invalid source geometry is reported instead of silently rendered."));
        return builder.Build();
    }

    private static RenderSceneEntity Entity(string id, string type, int sourceIndex, IEnumerable<RenderGeometryPrimitive> geometry) => new(
        new RenderEntityId(id),
        new RenderLayerToken("0"),
        new RenderStyleToken("BYLAYER"),
        new RenderSourceReference(type, handle: id, sourceIndex: sourceIndex),
        geometry);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"A10 validation failed: {message}");
    }
}
#endif
