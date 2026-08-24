using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Snapshots;

var viewport = new RenderViewport(
    pixelWidth: 1080,
    pixelHeight: 1920,
    centerX: 100,
    centerY: 200,
    worldUnitsPerPixel: 0.25);

Assert(viewport.PixelWidth == 1080, "viewport width");
Assert(viewport.PixelHeight == 1920, "viewport height");
Assert(viewport.WorldUnitsPerPixel == 0.25, "viewport scale");
AssertThrows<ArgumentOutOfRangeException>(() => new RenderViewport(0, 100, 0, 0, 1), "zero width must fail");
AssertThrows<ArgumentOutOfRangeException>(() => new RenderViewport(100, 0, 0, 0, 1), "zero height must fail");
AssertThrows<ArgumentOutOfRangeException>(() => new RenderViewport(100, 100, double.NaN, 0, 1), "NaN center must fail");
AssertThrows<ArgumentOutOfRangeException>(() => new RenderViewport(100, 100, 0, double.PositiveInfinity, 1), "infinite center must fail");
AssertThrows<ArgumentOutOfRangeException>(() => new RenderViewport(100, 100, 0, 0, 0), "zero scale must fail");
AssertThrows<ArgumentOutOfRangeException>(() => new RenderViewport(100, 100, 0, 0, double.NaN), "NaN scale must fail");

AssertThrows<ArgumentOutOfRangeException>(() => new WorldPoint2(double.NaN, 0), "world NaN must fail");
AssertThrows<ArgumentOutOfRangeException>(() => new WorldBounds2(0, 0, double.PositiveInfinity, 1), "bounds infinity must fail");
AssertThrows<ArgumentOutOfRangeException>(() => new WorldBounds2(2, 0, 1, 1), "inverted bounds must fail");

var surveyCamera = new Camera2D(
    1000,
    1000,
    new WorldPoint2(5_000_000d, 5_000_000d),
    0.001d,
    minWorldUnitsPerPixel: 1e-9,
    maxWorldUnitsPerPixel: 1e9);
var surveyA = CameraTransform.WorldToScreen(new WorldPoint2(5_000_000d, 5_000_000d), surveyCamera);
var surveyB = CameraTransform.WorldToScreen(new WorldPoint2(5_000_000.001d, 5_000_000d), surveyCamera);
AssertNear(surveyB.X - surveyA.X, 1d, 1e-6, "survey-origin 1 mm detail must survive camera transform");
var surveyRoundTrip = CameraTransform.ScreenToWorld(surveyB, surveyCamera);
AssertNear(surveyRoundTrip.X, 5_000_000.001d, 1e-9, "screen/world roundtrip must retain double precision");

var fit = Camera2D.Fit(new WorldBounds2(0, 0, 100, 50), 1000, 500, paddingFraction: 0);
AssertNear(fit.Center.X, 50, 1e-12, "fit center x");
AssertNear(fit.Center.Y, 25, 1e-12, "fit center y");
AssertNear(fit.WorldUnitsPerPixel, 0.1, 1e-12, "fit scale");
var zoomed = fit.ZoomBy(2);
AssertNear(zoomed.WorldUnitsPerPixel, 0.05, 1e-12, "zoom in factor");
var clampedZoom = new Camera2D(100, 100, new WorldPoint2(0, 0), 1, 0.5, 2).ZoomBy(1000);
AssertNear(clampedZoom.WorldUnitsPerPixel, 0.5, 1e-12, "zoom minimum clamp");

var identityOcs = new OcsCoordinateSystem(new Vector3D(0, 0, 1));
var identityPoint = identityOcs.OcsToWcs(new WorldPoint3(12.5, -4.25, 3));
AssertNear(identityPoint.X, 12.5, 1e-12, "OCS identity x");
AssertNear(identityPoint.Y, -4.25, 1e-12, "OCS identity y");
AssertNear(identityPoint.Z, 3, 1e-12, "OCS identity z");

var obliqueOcs = new OcsCoordinateSystem(new Vector3D(1, 2, 3));
var ocsInput = new WorldPoint3(1234.5, -987.25, 42.125);
var wcs = obliqueOcs.OcsToWcs(ocsInput);
var ocsRoundTrip = obliqueOcs.WcsToOcs(wcs);
AssertNear(ocsRoundTrip.X, ocsInput.X, 1e-10, "OCS roundtrip x");
AssertNear(ocsRoundTrip.Y, ocsInput.Y, 1e-10, "OCS roundtrip y");
AssertNear(ocsRoundTrip.Z, ocsInput.Z, 1e-10, "OCS roundtrip z");

var sceneA = BuildSyntheticScene(reverseEntityOrder: false);
var sceneB = BuildSyntheticScene(reverseEntityOrder: true);
var snapshotA = RenderSceneSemanticSnapshot.Create(sceneA);
var snapshotB = RenderSceneSemanticSnapshot.Create(sceneB);
Assert(snapshotA == snapshotB, "same semantic input must produce identical snapshot regardless of insertion order");
Assert(sceneA.Entities.Count == 2, "scene entity count");
Assert(sceneA.Entities[0].Id.Value == "E-001", "stable IDs must define deterministic ordering");
Assert(sceneA.WorldBounds == new WorldBounds2(5_000_000, -25, 5_000_010, 100), "scene bounds union");
Assert(sceneA.Diagnostics.Count(SceneDiagnosticKind.Unsupported) == 1, "unsupported diagnostic count");
Assert(sceneA.Diagnostics.Count(SceneDiagnosticKind.Substituted) == 1, "substituted diagnostic count");
Assert(!sceneA.Diagnostics.HasErrors, "synthetic scene should have no error diagnostics");
Assert(snapshotA.Contains("diagnostic=Unsupported|UNSUPPORTED_PROXY|E-002", StringComparison.Ordinal), "unsupported diagnostic snapshot");
Assert(snapshotA.Contains("diagnostic=Substituted|STYLE_FALLBACK|E-001", StringComparison.Ordinal), "substitution diagnostic snapshot");

var duplicateBuilder = new RenderSceneAssembler();
duplicateBuilder.AddEntity(CreateEntity("DUP", 0, 0, 1, 1));
AssertThrows<InvalidOperationException>(() => duplicateBuilder.AddEntity(CreateEntity("DUP", 1, 1, 2, 2)), "duplicate stable ID must fail");

Console.WriteLine("STAGE09_RENDER_SCENE_TESTS_PASS");
Console.WriteLine(snapshotA);

static RenderScene BuildSyntheticScene(bool reverseEntityOrder)
{
    var builder = new RenderSceneAssembler(RenderColorContext.Dark);
    var first = CreateEntity("E-001", 5_000_000, -25, 5_000_000.001, 100, layer: "0", style: "BYLAYER", handle: "A1", sourceIndex: 1);
    var second = CreateEntity("E-002", 5_000_005, 10, 5_000_010, 20, layer: "SURVEY", style: "TRUECOLOR", handle: "A2", sourceIndex: 2);

    if (reverseEntityOrder)
    {
        builder.AddEntity(second);
        builder.AddEntity(first);
    }
    else
    {
        builder.AddEntity(first);
        builder.AddEntity(second);
    }

    builder.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Substituted, "STYLE_FALLBACK", "Style substituted deterministically.", first.Id));
    builder.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Unsupported, "UNSUPPORTED_PROXY", "Proxy entity retained as compatibility evidence.", second.Id));
    return builder.Build();
}

static RenderSceneEntity CreateEntity(
    string id,
    double minX,
    double minY,
    double maxX,
    double maxY,
    string layer = "0",
    string style = "BYLAYER",
    string? handle = null,
    int? sourceIndex = null) => new(
        new RenderEntityId(id),
        new WorldBounds2(minX, minY, maxX, maxY),
        new RenderLayerToken(layer),
        new RenderStyleToken(style),
        new RenderSourceReference("SYNTHETIC", handle, sourceIndex));

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertNear(double actual, double expected, double tolerance, string message)
{
    if (!double.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
    {
        throw new InvalidOperationException($"{message}: expected={expected:R}, actual={actual:R}, tolerance={tolerance:R}");
    }
}

static void AssertThrows<TException>(Action action, string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}
