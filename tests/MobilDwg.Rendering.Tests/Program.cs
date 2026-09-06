using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Tests;

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
AssertThrows<ArgumentOutOfRangeException>(() => new WorldBounds2(-double.MaxValue, 0, double.MaxValue, 1), "overflowing finite bounds span must fail");
var largeFiniteBounds = new WorldBounds2(1e308, 1e308, 1.1e308, 1.1e308);
Assert(double.IsFinite(largeFiniteBounds.Center.X), "large finite bounds center x must stay finite");
Assert(double.IsFinite(largeFiniteBounds.Center.Y), "large finite bounds center y must stay finite");
AssertNear(largeFiniteBounds.Center.X / 1e308, 1.05d, 1e-12, "overflow-safe large bounds center x");

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
AssertThrows<ArgumentException>(() => CameraTransform.WorldToScreen(new WorldPoint2(0, 0), default), "default camera must fail at transform boundary");
AssertThrows<ArgumentOutOfRangeException>(() => new ViewPoint2(double.PositiveInfinity, 0), "infinite view point must fail");
var extremeCamera = new Camera2D(100, 100, new WorldPoint2(1e308, 0), 1, 1e-12, 1e12);
AssertThrows<ArgumentOutOfRangeException>(() => CameraTransform.WorldToView(new WorldPoint2(-1e308, 0), extremeCamera), "overflowing world-to-view delta must fail instead of propagating infinity");

var fit = Camera2D.Fit(new WorldBounds2(0, 0, 100, 50), 1000, 500, paddingFraction: 0);
AssertNear(fit.Center.X, 50, 1e-12, "fit center x");
AssertNear(fit.Center.Y, 25, 1e-12, "fit center y");
AssertNear(fit.WorldUnitsPerPixel, 0.1, 1e-12, "fit scale");
var zoomed = fit.ZoomBy(2);
AssertNear(zoomed.WorldUnitsPerPixel, 0.05, 1e-12, "zoom in factor");
var clampedZoom = new Camera2D(100, 100, new WorldPoint2(0, 0), 1, 0.5, 2).ZoomBy(1000);
AssertNear(clampedZoom.WorldUnitsPerPixel, 0.5, 1e-12, "zoom minimum clamp");
var fitViewport = fit.ToViewport();
var fitFromViewport = Camera2D.FromViewport(fitViewport);
Assert(fitFromViewport.PixelWidth == fit.PixelWidth, "camera/viewport width bridge");
Assert(fitFromViewport.PixelHeight == fit.PixelHeight, "camera/viewport height bridge");
AssertNear(fitFromViewport.Center.X, fit.Center.X, 1e-12, "camera/viewport center x bridge");
AssertNear(fitFromViewport.Center.Y, fit.Center.Y, 1e-12, "camera/viewport center y bridge");
AssertNear(fitFromViewport.WorldUnitsPerPixel, fit.WorldUnitsPerPixel, 1e-12, "camera/viewport scale bridge");

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
var hugeNormalOcs = new OcsCoordinateSystem(new Vector3D(1e300, 2e300, 3e300));
AssertNear(hugeNormalOcs.Normal.Length, 1d, 1e-12, "very large finite extrusion normal must normalize without overflow");
AssertThrows<InvalidOperationException>(() => new OcsCoordinateSystem(default), "zero/default extrusion normal must fail");

AssertThrows<ArgumentOutOfRangeException>(() => new RenderSourceReference("LINE", sourceIndex: -1), "negative source index must fail");
AssertThrows<ArgumentException>(() => new RenderSourceReference("LINE", handle: "   "), "blank supplied handle must fail");
AssertThrows<ArgumentException>(
    () => new RenderSceneEntity(default, new WorldBounds2(0, 0, 1, 1), new RenderLayerToken("0"), new RenderStyleToken("BYLAYER"), new RenderSourceReference("LINE")),
    "default stable entity ID must fail at scene boundary");
AssertThrows<ArgumentException>(
    () => new RenderSceneEntity(new RenderEntityId("E"), new WorldBounds2(0, 0, 1, 1), default, new RenderStyleToken("BYLAYER"), new RenderSourceReference("LINE")),
    "default layer token must fail at scene boundary");
AssertThrows<ArgumentException>(
    () => new RenderSceneEntity(new RenderEntityId("E"), new WorldBounds2(0, 0, 1, 1), new RenderLayerToken("0"), default, new RenderSourceReference("LINE")),
    "default style token must fail at scene boundary");

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

var diagnosticBuilder = new RenderSceneAssembler();
diagnosticBuilder.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Unsupported, "U", "unsupported"));
diagnosticBuilder.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Substituted, "S", "substituted"));
diagnosticBuilder.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Dropped, "D", "dropped"));
diagnosticBuilder.AddDiagnostic(new SceneDiagnostic(SceneDiagnosticKind.Error, "E", "error"));
var diagnosticScene = diagnosticBuilder.Build();
Assert(diagnosticScene.Diagnostics.Count(SceneDiagnosticKind.Unsupported) == 1, "unsupported taxonomy");
Assert(diagnosticScene.Diagnostics.Count(SceneDiagnosticKind.Substituted) == 1, "substituted taxonomy");
Assert(diagnosticScene.Diagnostics.Count(SceneDiagnosticKind.Dropped) == 1, "dropped taxonomy");
Assert(diagnosticScene.Diagnostics.Count(SceneDiagnosticKind.Error) == 1, "error taxonomy");
Assert(diagnosticScene.Diagnostics.HasErrors, "error diagnostic must set HasErrors");
AssertThrows<ArgumentOutOfRangeException>(() => new SceneDiagnostic((SceneDiagnosticKind)99, "BAD", "bad taxonomy"), "unknown diagnostic taxonomy must fail");
AssertThrows<ArgumentException>(() => new SceneDiagnostic(SceneDiagnosticKind.Error, "BAD_ID", "bad entity id", default(RenderEntityId)), "default diagnostic entity ID must fail");

var duplicateBuilder = new RenderSceneAssembler();
duplicateBuilder.AddEntity(CreateEntity("DUP", 0, 0, 1, 1));
AssertThrows<InvalidOperationException>(() => duplicateBuilder.AddEntity(CreateEntity("DUP", 1, 1, 2, 2)), "duplicate stable ID must fail");

ViewportCameraTests.Run();
ViewportInteractionTests.Run();

Console.WriteLine("STAGE04_RENDER_CONTRACT_TESTS_PASS");
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
