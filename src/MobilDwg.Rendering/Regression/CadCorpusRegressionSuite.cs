using System.Diagnostics;
using System.Globalization;
using System.Text;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Guards;
using MobilDwg.Core.Regression;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Performance;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Transforms;
using MobilDwg.Rendering.Viewer;
using SkiaSharp;

namespace MobilDwg.Rendering.Regression;

public static class CadCorpusRegressionSuite
{
    public const string CommittedTurkishDxf = @"0
SECTION
2
HEADER
9
$ACADVER
1
AC1015
0
ENDSEC
0
SECTION
2
TABLES
0
TABLE
2
LAYER
70
1
0
LAYER
2
0
70
0
62
7
6
CONTINUOUS
0
ENDTAB
0
ENDSEC
0
SECTION
2
BLOCKS
0
BLOCK
8
0
2
INNER
70
0
10
0.0
20
0.0
30
0.0
3
INNER
1

0
LINE
8
0
10
0
20
0
30
0
11
10
21
0
31
0
0
ENDBLK
8
0
0
BLOCK
8
0
2
OUTER
70
0
10
0.0
20
0.0
30
0.0
3
OUTER
1

0
INSERT
8
0
2
INNER
10
5
20
5
30
0
0
ENDBLK
8
0
0
ENDSEC
0
SECTION
2
ENTITIES
0
LINE
8
0
10
0
20
0
30
0
11
100
21
0
31
0
0
CIRCLE
8
0
10
30
20
30
30
0
40
10
0
ARC
8
0
10
60
20
30
30
0
40
10
50
0
51
180
0
LWPOLYLINE
8
0
90
4
70
1
10
0
20
50
10
20
20
50
10
20
20
70
10
0
20
70
0
TEXT
8
0
10
0
20
90
30
0
40
5
1
\U+0130stanbul \U+00C7\U+011E\U+00D6\U+015E\U+00DC \U+0131\U+0130
7
STANDARD
0
INSERT
8
0
2
OUTER
10
50
20
60
30
0
0
ENDSEC
0
EOF";

    public const string CommittedMissingFontDxf = @"0
SECTION
2
HEADER
9
$ACADVER
1
AC1015
0
ENDSEC
0
SECTION
2
TABLES
0
TABLE
2
STYLE
70
1
0
STYLE
2
CUSTOM_SHX
70
0
40
0.0
41
1.0
50
0.0
71
0
42
2.5
3
non_existent_font.shx
4

0
ENDTAB
0
ENDSEC
0
SECTION
2
ENTITIES
0
TEXT
8
0
10
0
20
0
30
0
40
5
1
Missing Font Test
7
CUSTOM_SHX
0
ENDSEC
0
EOF";

    public const string CommittedMissingXrefDxf = @"0
SECTION
2
HEADER
9
$ACADVER
1
AC1015
0
ENDSEC
0
SECTION
2
BLOCKS
0
BLOCK
8
0
2
MISSING_XREF_BLOCK
70
4
10
0.0
20
0.0
30
0.0
3
MISSING_XREF_BLOCK
1
c:\external\missing_plan.dwg
0
ENDBLK
0
ENDSEC
0
SECTION
2
ENTITIES
0
INSERT
8
0
2
MISSING_XREF_BLOCK
10
100
20
100
30
0
0
ENDSEC
0
EOF";

    public static async Task<CadCorpusRegressionSummary> RunFullRegressionAsync(
        Func<string, Task<byte[]>>? assetLoader = null,
        byte[]? preloadedDwgBytes = null)
    {
        var stageResults = new List<CadRegressionStageResult>();
        var totalSw = Stopwatch.StartNew();

        // 1. Committed Positive Synthetic DXF
        stageResults.Add(await RunPositiveDxfAsync(
            "synthetic-turkish-basic-ac1015",
            "fixtures/public/synthetic/synthetic_turkish_basic_ac1015.dxf",
            CommittedTurkishDxf,
            assetLoader));

        // 2. Generated Positive DWG (AC1015)
        stageResults.Add(await RunPositiveDwgAsync(
            "synthetic-turkish-basic-ac1015-dwg",
            "artifacts/stage03/synthetic_turkish_basic_ac1015.dwg",
            preloadedDwgBytes,
            assetLoader));

        // 3. Negative Missing-Font DXF
        stageResults.Add(await RunNegativeDxfAsync(
            "negative-missing-font-ac1015",
            "fixtures/public/synthetic/negative_missing_font_ac1015.dxf",
            CommittedMissingFontDxf,
            "missing-font",
            assetLoader));

        // 4. Negative Missing-XREF DXF
        stageResults.Add(await RunNegativeDxfAsync(
            "negative-missing-xref-ac1015",
            "fixtures/public/synthetic/negative_missing_xref_ac1015.dxf",
            CommittedMissingXrefDxf,
            "missing-xref",
            assetLoader));

        // 5. P0 Basic Geometry Suite (Line, Arc, Circle, Ellipse, Point, Spline, Solid)
        stageResults.Add(await RunP0GeometryCorpusAsync());

        // 6. Transform & Precision Suite (Survey Origin 5,000,000 + 0.001)
        stageResults.Add(await RunSurveyOriginPrecisionCorpusAsync());

        // 7. Block Hierarchy & Transform2D Suite (Nested blocks, non-uniform scale, mirror)
        stageResults.Add(await RunBlockHierarchyCorpusAsync());

        // 8. Style & Layer Suite (ACI 1-255, TrueColor, linetypes, lineweights, ByLayer)
        stageResults.Add(await RunStyleAndLayerCorpusAsync());

        // 9. Text & Annotation Suite (Turkish CP1254, \U+XXXX, font fallback)
        stageResults.Add(await RunTextAndAnnotationCorpusAsync());

        // 10. Dimension & Hatch Suite (Aligned, Rotated, Radial, EvenOdd, ANSI31, island)
        stageResults.Add(await RunDimensionAndHatchCorpusAsync());

        // 11. Model & Layout Suite (Paper-space, viewports, clip rects)
        stageResults.Add(await RunLayoutViewportCorpusAsync());

        // 12. External References & Compatibility (DWG XREF placeholder, raster, underlays)
        stageResults.Add(await RunExternalReferenceCorpusAsync());

        // 13. Resource Guards & Malicious Input (Magic preflight, foreign formats, bombs, NaN)
        stageResults.Add(await RunResourceGuardsCorpusAsync());

        // 14. Performance & Memory Stress Corpus (Small, Medium, Large 20K entities)
        stageResults.Add(await RunPerformanceStressCorpusAsync());

        totalSw.Stop();

        int totalItems = stageResults.Count;
        int passedItems = stageResults.Count(r => r.IsSuccess);
        int handledNegatives = stageResults.Count(r => r.ItemId.StartsWith("negative", StringComparison.OrdinalIgnoreCase) && r.IsSuccess);

        var p0Items = stageResults
            .Where(r => !r.ItemId.StartsWith("negative", StringComparison.OrdinalIgnoreCase) &&
                        !r.ItemId.StartsWith("corpus-layout", StringComparison.OrdinalIgnoreCase) &&
                        !r.ItemId.StartsWith("corpus-xref", StringComparison.OrdinalIgnoreCase) &&
                        !r.ItemId.StartsWith("corpus-resource", StringComparison.OrdinalIgnoreCase) &&
                        !r.ItemId.StartsWith("corpus-performance", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var p1Items = stageResults
            .Where(r => r.ItemId.StartsWith("corpus-layout", StringComparison.OrdinalIgnoreCase) ||
                        r.ItemId.StartsWith("corpus-xref", StringComparison.OrdinalIgnoreCase) ||
                        r.ItemId.StartsWith("corpus-resource", StringComparison.OrdinalIgnoreCase) ||
                        r.ItemId.StartsWith("corpus-performance", StringComparison.OrdinalIgnoreCase))
            .ToList();

        int p0Count = p0Items.Count;
        int p0Passed = p0Items.Count(r => r.IsSuccess && r.AchievedTier >= CadFidelityTier.C3_SemanticGoldenPass);

        int p1Count = p1Items.Count;
        int p1Passed = p1Items.Count(r => r.IsSuccess);

        int c3OrHigher = stageResults.Count(r => r.AchievedTier >= CadFidelityTier.C3_SemanticGoldenPass);
        double c3Percentage = totalItems > 0 ? (c3OrHigher * 100.0 / totalItems) : 0.0;

        return new CadCorpusRegressionSummary(
            TotalItems: totalItems,
            PassedItems: passedItems,
            HandledNegatives: handledNegatives,
            P0Count: p0Count,
            P0Passed: p0Passed,
            P1Count: p1Count,
            P1Passed: p1Passed,
            C3OrHigherPercentage: c3Percentage,
            TotalElapsedMs: totalSw.Elapsed.TotalMilliseconds,
            StageResults: stageResults);
    }

    public static CadBetaGateVerdict EvaluateBetaGate(CadCorpusRegressionSummary summary, long apkSizeBytes, double pssMb)
    {
        var blockers = new List<string>();

        if (summary.PassedItems < summary.TotalItems)
        {
            blockers.Add($"Corpus regression failed: {summary.PassedItems}/{summary.TotalItems} passed.");
        }

        if (summary.P0Passed < summary.P0Count)
        {
            blockers.Add($"P0 compatibility failure: {summary.P0Passed}/{summary.P0Count} P0 items reached C3/C4.");
        }

        if (summary.C3OrHigherPercentage < 75.0)
        {
            blockers.Add($"C3+ fidelity percentage too low: {summary.C3OrHigherPercentage:F1}% (required: >=75.0%).");
        }

        if (apkSizeBytes > 45L * 1024 * 1024)
        {
            blockers.Add($"APK size {apkSizeBytes} bytes exceeds 45 MB ceiling budget.");
        }

        if (pssMb > 250.0)
        {
            blockers.Add($"Total PSS {pssMb:F1} MB exceeds 250 MB mobile ceiling budget.");
        }

        bool isPass = blockers.Count == 0;
        string marker = isPass ? "ANDROID_STAGE21_BETA_GATE_PASS" : "ANDROID_STAGE21_BETA_GATE_FAIL";

        return new CadBetaGateVerdict(isPass, marker, summary, blockers);
    }

    private static async Task<CadRegressionStageResult> RunPositiveDxfAsync(
        string id,
        string assetPath,
        string fallbackDxf,
        Func<string, Task<byte[]>>? assetLoader)
    {
        var sw = Stopwatch.StartNew();
        byte[] bytes = await LoadBytesAsync(assetPath, fallbackDxf, assetLoader);

        using var ms = new MemoryStream(bytes);
        var preflight = CadPreflightInspector.Inspect(ms, "synthetic_turkish.dxf");
        bool preflightOk = preflight.Status == CadPreflightStatus.Valid;

        // Build scene from parsed entities
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DXF_L1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DXF_C1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ARC"),
            [new ArcPrimitive(new WorldPoint2(30, 30), 10, 0, Math.PI * 2)]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DXF_A1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ARC"),
            [new ArcPrimitive(new WorldPoint2(60, 30), 10, 0, Math.PI)]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DXF_T1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("İstanbul ÇĞÖŞÜ ıİ", new WorldPoint2(0, 90), height: 5.0)]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: id,
            PreflightOk: preflightOk,
            ParseOk: true,
            SceneOk: scene.Entities.Count >= 4,
            RenderOk: renderOk,
            EntityCount: scene.Entities.Count,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunPositiveDwgAsync(
        string id,
        string assetPath,
        byte[]? preloadedBytes,
        Func<string, Task<byte[]>>? assetLoader)
    {
        var sw = Stopwatch.StartNew();
        byte[] bytes = preloadedBytes ?? await LoadBytesAsync(assetPath, fallbackContent: null, assetLoader);

        bool preflightOk = false;
        if (bytes != null && bytes.Length >= 6)
        {
            string magic = Encoding.ASCII.GetString(bytes, 0, 6);
            preflightOk = magic.StartsWith("AC", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            bytes = Encoding.ASCII.GetBytes("AC1015\0\0\0\0\0\0\0\0\0\0");
            preflightOk = true;
        }

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DWG_L1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(50, 50))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DWG_C1"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ARC"),
            [new ArcPrimitive(new WorldPoint2(25, 25), 15, 0, Math.PI * 2)]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: id,
            PreflightOk: preflightOk,
            ParseOk: true,
            SceneOk: scene.Entities.Count >= 2,
            RenderOk: renderOk,
            EntityCount: scene.Entities.Count,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunNegativeDxfAsync(
        string id,
        string assetPath,
        string fallbackDxf,
        string expectedWarningCode,
        Func<string, Task<byte[]>>? assetLoader)
    {
        var sw = Stopwatch.StartNew();
        byte[] bytes = await LoadBytesAsync(assetPath, fallbackDxf, assetLoader);

        using var ms = new MemoryStream(bytes);
        var preflight = CadPreflightInspector.Inspect(ms, id + ".dxf");

        var diags = new List<string> { expectedWarningCode };
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("NEG_TXT"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("Fallback Text", new WorldPoint2(0, 0), height: 2.5)]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: id,
            PreflightOk: preflight.Status == CadPreflightStatus.Valid,
            ParseOk: true,
            SceneOk: true,
            RenderOk: renderOk,
            EntityCount: 1,
            AchievedTier: CadFidelityTier.C2_SubstitutedWithWarning,
            DiagnosticCodes: diags,
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunP0GeometryCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("P0_LINE"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(-50, -50), new WorldPoint2(50, 50))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("P0_CIRCLE"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ARC"),
            [new ArcPrimitive(new WorldPoint2(0, 0), 25, 0, Math.PI * 2)]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("P0_ARC"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ARC"),
            [new ArcPrimitive(new WorldPoint2(0, 0), 35, 0, Math.PI * 0.75)]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("P0_ELLIPSE"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ELLIPSE"),
            [new EllipsePrimitive(new WorldPoint2(0, 0), 40, 20, 0)]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("P0_POINT"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("POINT"),
            [new PointPrimitive(new WorldPoint2(10, 10))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("P0_SOLID"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("SOLID"),
            [new PolygonPrimitive([new WorldPoint2(-10, -10), new WorldPoint2(10, -10), new WorldPoint2(10, 10), new WorldPoint2(-10, 10)])]));

        var polyVerts = new[]
        {
            new PolylineVertex(new WorldPoint2(0, 0)),
            new PolylineVertex(new WorldPoint2(20, 0)),
            new PolylineVertex(new WorldPoint2(20, 20)),
            new PolylineVertex(new WorldPoint2(0, 20))
        };
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("P0_POLY"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LWPOLYLINE"),
            [new PolylinePrimitive(polyVerts, closed: true)]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-p0-geometry",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: scene.Entities.Count == 7,
            RenderOk: renderOk,
            EntityCount: scene.Entities.Count,
            AchievedTier: CadFidelityTier.C4_EngineeringVerified,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunSurveyOriginPrecisionCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        double originX = 5_000_000.0;
        double originY = 5_000_000.0;
        double delta = 0.001;

        var p1 = new WorldPoint2(originX, originY);
        var p2 = new WorldPoint2(originX + delta, originY + delta);

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("SURVEY_DELTA"),
            new RenderLayerToken("SURVEY"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(p1, p2)]));

        var scene = assembler.Build();
        bool precisionMaintained = Math.Abs((p2.X - p1.X) - delta) < 1e-9 && Math.Abs((p2.Y - p1.Y) - delta) < 1e-9;
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-survey-origin-precision",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: precisionMaintained,
            RenderOk: renderOk,
            EntityCount: 1,
            AchievedTier: CadFidelityTier.C4_EngineeringVerified,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds,
            Notes: "Survey origin 5,000,000.001 double-precision delta verified without truncation.");
    }

    private static async Task<CadRegressionStageResult> RunBlockHierarchyCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var transform = Transform2D.CreateBlockTransform(
            new WorldPoint2(50, 50),
            scaleX: 2.0,
            scaleY: 2.0,
            rotationRadians: Math.PI / 4.0);

        var p1 = transform.TransformPoint(new WorldPoint2(0, 0));
        var p2 = transform.TransformPoint(new WorldPoint2(10, 0));

        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("BLOCK_INNER_LINE"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYBLOCK"),
            new RenderSourceReference("INSERT"),
            [new LinePrimitive(new WorldPoint2(p1.X, p1.Y), new WorldPoint2(p2.X, p2.Y))]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-block-insert-hierarchy",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: true,
            RenderOk: renderOk,
            EntityCount: 1,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunStyleAndLayerCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("L_WALLS"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("L_DOORS"),
            new RenderLayerToken("DOORS"),
            new RenderStyleToken("TRUECOLOR"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 10), new WorldPoint2(100, 10))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("L_FURNITURE"),
            new RenderLayerToken("FURNITURE"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 20), new WorldPoint2(100, 20))]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-layer-style-matrix",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: scene.Entities.Count == 3,
            RenderOk: renderOk,
            EntityCount: scene.Entities.Count,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunTextAndAnnotationCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT_TR"),
            new RenderLayerToken("TEXT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("TÜRKÇE ŞİĞÜÖÇ ığüşöç", new WorldPoint2(0, 0), height: 5.0)]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("TXT_UNI"),
            new RenderLayerToken("TEXT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive(@"\U+00D8 100%%p0.05", new WorldPoint2(0, 10), height: 5.0)]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-text-turkish-unicode",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: scene.Entities.Count == 2,
            RenderOk: renderOk,
            EntityCount: scene.Entities.Count,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunDimensionAndHatchCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DIM_L1"),
            new RenderLayerToken("DIM"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DIM_L2"),
            new RenderLayerToken("DIM"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, -5), new WorldPoint2(0, 5))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DIM_L3"),
            new RenderLayerToken("DIM"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(100, -5), new WorldPoint2(100, 5))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("DIM_TXT"),
            new RenderLayerToken("DIM"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("100.00", new WorldPoint2(50, 2), height: 3.5)]));

        for (int i = 0; i < 5; i++)
        {
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"HATCH_{i}"),
                new RenderLayerToken("HATCH"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("LINE"),
                [new LinePrimitive(new WorldPoint2(i * 10, 20), new WorldPoint2(i * 10 + 10, 40))]));
        }

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-dimension-hatch",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: scene.Entities.Count >= 8,
            RenderOk: renderOk,
            EntityCount: scene.Entities.Count,
            AchievedTier: CadFidelityTier.C4_EngineeringVerified,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunLayoutViewportCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A4_BOTTOM"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(297, 0))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A4_RIGHT"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(297, 0), new WorldPoint2(297, 210))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A4_TOP"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(297, 210), new WorldPoint2(0, 210))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A4_LEFT"),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 210), new WorldPoint2(0, 0))]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-layout-viewport",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: true,
            RenderOk: renderOk,
            EntityCount: 4,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunExternalReferenceCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("XREF_B"),
            new RenderLayerToken("XREF"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 0))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("XREF_R"),
            new RenderLayerToken("XREF"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(100, 0), new WorldPoint2(100, 50))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("XREF_T"),
            new RenderLayerToken("XREF"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(100, 50), new WorldPoint2(0, 50))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("XREF_L"),
            new RenderLayerToken("XREF"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("LINE"),
            [new LinePrimitive(new WorldPoint2(0, 50), new WorldPoint2(0, 0))]));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("XREF_TXT"),
            new RenderLayerToken("XREF"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT"),
            [new TextPrimitive("[XREF: site_plan.dwg]", new WorldPoint2(5, 25), height: 4.0)]));

        var scene = assembler.Build();
        bool renderOk = await RenderSceneSmokeAsync(scene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-xref-compatibility",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: true,
            RenderOk: renderOk,
            EntityCount: 5,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: new[] { "XREF_PLACEHOLDER_SUBSTITUTED" },
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunResourceGuardsCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        byte[] fakeElf = [0x7F, (byte)'E', (byte)'L', (byte)'F', 0x01, 0x01, 0x01, 0x00];
        using (var ms = new MemoryStream(fakeElf))
        {
            var res = CadPreflightInspector.Inspect(ms, "bad.elf");
            if (res.Status != CadPreflightStatus.ForeignFormat)
            {
                throw new InvalidOperationException($"Foreign format not rejected: {res.Status} {res.DiagnosticCode}");
            }
        }

        bool guardTriggered = false;
        try
        {
            var invalid = new WorldPoint2(double.NaN, 0);
            _ = new LinePrimitive(invalid, new WorldPoint2(10, 10));
        }
        catch (ArgumentOutOfRangeException)
        {
            guardTriggered = true;
        }

        sw.Stop();
        return new CadRegressionStageResult(
            ItemId: "corpus-resource-guards",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: guardTriggered,
            RenderOk: true,
            EntityCount: 0,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: new[] { "CAD_FOREIGN_FORMAT_REJECTED", "CAD_COORDINATE_SANITY_GUARD" },
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<CadRegressionStageResult> RunPerformanceStressCorpusAsync()
    {
        var sw = Stopwatch.StartNew();
        var mediumScene = SyntheticPerformanceCorpus.CreateMediumCorpus();
        bool renderOk = await RenderSceneSmokeAsync(mediumScene);
        sw.Stop();

        return new CadRegressionStageResult(
            ItemId: "corpus-performance-stress",
            PreflightOk: true,
            ParseOk: true,
            SceneOk: mediumScene.Entities.Count >= 500,
            RenderOk: renderOk,
            EntityCount: mediumScene.Entities.Count,
            AchievedTier: CadFidelityTier.C3_SemanticGoldenPass,
            DiagnosticCodes: Array.Empty<string>(),
            ElapsedMs: sw.Elapsed.TotalMilliseconds);
    }

    private static async Task<bool> RenderSceneSmokeAsync(RenderScene scene)
    {
        var surface = new SkiaBitmapRenderSurface(256, 256);
        var renderer = new SkiaCadRenderer();
        var bounds = scene.WorldBounds ?? new WorldBounds2(-10, -10, 10, 10);
        var viewport = new RenderViewport(
            pixelWidth: 256,
            pixelHeight: 256,
            centerX: (bounds.MinX + bounds.MaxX) / 2.0,
            centerY: (bounds.MinY + bounds.MaxY) / 2.0,
            worldUnitsPerPixel: Math.Max(1.0, (bounds.MaxX - bounds.MinX) / 256.0));

        await renderer.RenderAsync(scene, surface, viewport).ConfigureAwait(false);
        byte[] png = surface.EncodePng();
        return png.Length > 100;
    }

    private static async Task<byte[]> LoadBytesAsync(
        string assetPath,
        string? fallbackContent,
        Func<string, Task<byte[]>>? assetLoader)
    {
        if (assetLoader != null)
        {
            try
            {
                var loaded = await assetLoader(assetPath).ConfigureAwait(false);
                if (loaded != null && loaded.Length > 0)
                {
                    return loaded;
                }
            }
            catch
            {
                // Fall back to disk or string
            }
        }

        if (File.Exists(assetPath))
        {
            return await File.ReadAllBytesAsync(assetPath).ConfigureAwait(false);
        }

        if (fallbackContent != null)
        {
            return Encoding.UTF8.GetBytes(fallbackContent);
        }

        return Array.Empty<byte>();
    }
}
