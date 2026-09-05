using System.Runtime.CompilerServices;
using System.Text;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Guards;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Snapshots;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Tests;

public static class Stage19ResourceGuardsTests
{
    [ModuleInitializer]
    public static void Run()
    {
        TestPreflightValidDwgMagicPasses();
        TestPreflightInvalidDwgMagicRejected();
        TestPreflightBinaryAndAsciiDxfSignatures();
        TestPreflightNonCadFilesRejectedCleanly();
        TestPreflightZeroByteAndTruncatedHeader();
        TestFileSizeBudgetExceededRejection();
        TestEntityCountBudgetExceededGuard();
        TestBlockInsertNestingDepthBudgetAndCycleDetection();
        TestTextLengthBudgetAndTruncation();
        TestRasterImageDimensionBudgetGuard();
        TestNanInfinityAndExtremeCoordinatesSanityGuard();
        TestBoundedMutationFuzzSmokeZeroCrashesAndSnapshotDeterminism();

        Console.WriteLine("STAGE19_RESOURCE_GUARDS_TESTS_PASS");
    }

    private static void TestPreflightValidDwgMagicPasses()
    {
        string[] validVersions = ["AC1015", "AC1018", "AC1021", "AC1024", "AC1027", "AC1032"];
        foreach (var ver in validVersions)
        {
            byte[] header = Encoding.ASCII.GetBytes(ver + "\0\0\0\0\0\0\0\0\0\0");
            using var ms = new MemoryStream(header);
            var result = CadPreflightInspector.Inspect(ms, "drawing.dwg");

            Assert(result.Status == CadPreflightStatus.Valid, $"Expected valid status for {ver}");
            Assert(result.Format == CadFormat.Dwg, $"Expected DWG format for {ver}");
            Assert(result.Version == ver, $"Expected version {ver}");
            Assert(result.DiagnosticCode == "CAD_PREFLIGHT_DWG_VALID", "Expected CAD_PREFLIGHT_DWG_VALID");
        }
    }

    private static void TestPreflightInvalidDwgMagicRejected()
    {
        byte[] corruptedDwg = Encoding.ASCII.GetBytes("NOTDWG\0\0\0\0\0\0\0\0\0\0");
        using var ms = new MemoryStream(corruptedDwg);
        var result = CadPreflightInspector.Inspect(ms, "corrupted.dwg");

        Assert(result.Status == CadPreflightStatus.InvalidDwgMagic, "Expected InvalidDwgMagic");
        Assert(result.DiagnosticCode == "CAD_INVALID_DWG_MAGIC", "Expected CAD_INVALID_DWG_MAGIC");
    }

    private static void TestPreflightBinaryAndAsciiDxfSignatures()
    {
        // Binary DXF
        byte[] binaryDxf = Encoding.ASCII.GetBytes("AutoCAD Binary DXF\r\n\x1a\0SomeMoreBytes");
        using (var ms = new MemoryStream(binaryDxf))
        {
            var result = CadPreflightInspector.Inspect(ms, "binary.dxf");
            Assert(result.Status == CadPreflightStatus.Valid, "Expected binary DXF valid");
            Assert(result.Format == CadFormat.Dxf, "Expected DXF format");
            Assert(result.DiagnosticCode == "CAD_PREFLIGHT_DXF_BINARY_VALID", "Expected CAD_PREFLIGHT_DXF_BINARY_VALID");
        }

        // ASCII DXF with version header
        string asciiDxf = "0\r\nSECTION\r\n2\r\nHEADER\r\n9\r\n$ACADVER\r\n1\r\nAC1027\r\n0\r\nENDSEC\r\n0\r\nEOF";
        using (var ms = new MemoryStream(Encoding.Latin1.GetBytes(asciiDxf)))
        {
            var result = CadPreflightInspector.Inspect(ms, "plan.dxf");
            Assert(result.Status == CadPreflightStatus.Valid, "Expected ASCII DXF valid");
            Assert(result.Format == CadFormat.Dxf, "Expected DXF format");
            Assert(result.Version == "AC1027", "Expected AC1027 version");
            Assert(result.DiagnosticCode == "CAD_PREFLIGHT_DXF_ASCII_VALID", "Expected CAD_PREFLIGHT_DXF_ASCII_VALID");
        }
    }

    private static void TestPreflightNonCadFilesRejectedCleanly()
    {
        // PE executable
        byte[] pe = [(byte)'M', (byte)'Z', 0x90, 0x00, 0x03, 0x00, 0x00, 0x00];
        using (var ms = new MemoryStream(pe))
        {
            var result = CadPreflightInspector.Inspect(ms, "malicious.exe");
            Assert(result.Status == CadPreflightStatus.ForeignFormat, "Expected ForeignFormat for PE");
            Assert(result.DiagnosticCode == "CAD_FOREIGN_FORMAT_PE_EXECUTABLE", "Expected PE diagnostic");
        }

        // ELF binary
        byte[] elf = [0x7F, (byte)'E', (byte)'L', (byte)'F', 0x02, 0x01, 0x01, 0x00];
        using (var ms = new MemoryStream(elf))
        {
            var result = CadPreflightInspector.Inspect(ms, "binary.elf");
            Assert(result.Status == CadPreflightStatus.ForeignFormat, "Expected ForeignFormat for ELF");
            Assert(result.DiagnosticCode == "CAD_FOREIGN_FORMAT_ELF_EXECUTABLE", "Expected ELF diagnostic");
        }

        // ZIP archive
        byte[] zip = [(byte)'P', (byte)'K', 0x03, 0x04, 0x14, 0x00, 0x00, 0x00];
        using (var ms = new MemoryStream(zip))
        {
            var result = CadPreflightInspector.Inspect(ms, "archive.zip");
            Assert(result.Status == CadPreflightStatus.ForeignFormat, "Expected ForeignFormat for ZIP");
            Assert(result.DiagnosticCode == "CAD_FOREIGN_FORMAT_ZIP_ARCHIVE", "Expected ZIP diagnostic");
        }

        // PDF document
        byte[] pdf = [(byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-', (byte)'1', (byte)'.', (byte)'7'];
        using (var ms = new MemoryStream(pdf))
        {
            var result = CadPreflightInspector.Inspect(ms, "document.pdf");
            Assert(result.Status == CadPreflightStatus.ForeignFormat, "Expected ForeignFormat for PDF");
            Assert(result.DiagnosticCode == "CAD_FOREIGN_FORMAT_PDF_DOCUMENT", "Expected PDF diagnostic");
        }

        // HTML document
        byte[] html = Encoding.UTF8.GetBytes("<!DOCTYPE html><html><body>Error</body></html>");
        using (var ms = new MemoryStream(html))
        {
            var result = CadPreflightInspector.Inspect(ms, "index.html");
            Assert(result.Status == CadPreflightStatus.ForeignFormat, "Expected ForeignFormat for HTML");
            Assert(result.DiagnosticCode == "CAD_FOREIGN_FORMAT_HTML", "Expected HTML diagnostic");
        }
    }

    private static void TestPreflightZeroByteAndTruncatedHeader()
    {
        // 0-byte empty stream
        using (var ms = new MemoryStream(Array.Empty<byte>()))
        {
            var result = CadPreflightInspector.Inspect(ms, "empty.dwg");
            Assert(result.Status == CadPreflightStatus.EmptyOrTruncated, "Expected EmptyOrTruncated for 0 bytes");
            Assert(result.DiagnosticCode == "CAD_EMPTY_STREAM", "Expected CAD_EMPTY_STREAM");
        }

        // 3-byte truncated stream
        using (var ms = new MemoryStream(new byte[] { (byte)'A', (byte)'C', (byte)'1' }))
        {
            var result = CadPreflightInspector.Inspect(ms, "truncated.dwg");
            Assert(result.Status == CadPreflightStatus.EmptyOrTruncated, "Expected EmptyOrTruncated for 3 bytes");
            Assert(result.DiagnosticCode == "CAD_TRUNCATED_HEADER", "Expected CAD_TRUNCATED_HEADER");
        }
    }

    private static void TestFileSizeBudgetExceededRejection()
    {
        var budget = new CadResourceBudget { MaxFileSizeBytes = 100 * 1024 * 1024 }; // 100 MB
        var guard = new CadBudgetGuard(budget);

        bool pass = guard.CheckFileSize(50 * 1024 * 1024, out var diagOk);
        Assert(pass, "50 MB should pass 100 MB budget");
        Assert(diagOk == null, "Diagnostic should be null on pass");

        bool fail = guard.CheckFileSize(150 * 1024 * 1024, out var diagFail);
        Assert(!fail, "150 MB should fail 100 MB budget");
        Assert(diagFail != null, "Diagnostic required on failure");
        Assert(diagFail!.Code == "RESOURCE_BUDGET_EXCEEDED_FILE_SIZE", "Expected RESOURCE_BUDGET_EXCEEDED_FILE_SIZE");
        Assert(diagFail.Severity == DiagnosticSeverity.Error, "Expected Error severity");
    }

    private static void TestEntityCountBudgetExceededGuard()
    {
        var budget = new CadResourceBudget { MaxEntities = 5000 };
        var guard = new CadBudgetGuard(budget);

        bool ok = guard.CheckEntityCount(4999, out _);
        Assert(ok, "4999 should be within budget");

        bool exceeded = guard.CheckEntityCount(5001, out var diag);
        Assert(!exceeded, "5001 should exceed budget");
        Assert(diag != null && diag.Code == "RESOURCE_BUDGET_EXCEEDED_ENTITIES", "Expected RESOURCE_BUDGET_EXCEEDED_ENTITIES");
    }

    private static void TestBlockInsertNestingDepthBudgetAndCycleDetection()
    {
        // 1. Cycle detection: Block A references Block B, Block B references Block A
        var refB = new BlockReference("BLOCK_B", new WorldPoint2(0, 0));
        var defA = new BlockDefinition("BLOCK_A", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refB]);

        var refA = new BlockReference("BLOCK_A", new WorldPoint2(0, 0));
        var defB = new BlockDefinition("BLOCK_B", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [refA]);

        var root = new BlockReference("BLOCK_A", new WorldPoint2(0, 0));
        var expander = new BlockExpander([defA, defB]);

        var result = expander.Expand([root]);
        Assert(result.Diagnostics.Any(d => d.Code == "BLOCK_CYCLE_DETECTED"), "Expected BLOCK_CYCLE_DETECTED");

        // 2. Depth budget: Chain of blocks with max depth = 2
        var defD3 = new BlockDefinition("D3", new WorldPoint2(0, 0), [
            new BlockEntityTemplate(new PointPrimitive(new WorldPoint2(0, 0)), new RenderLayerToken("0"), new RenderStyleToken("BYLAYER"))
        ]);
        var defD2 = new BlockDefinition("D2", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [new BlockReference("D3", new WorldPoint2(0, 0))]);
        var defD1 = new BlockDefinition("D1", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [new BlockReference("D2", new WorldPoint2(0, 0))]);
        var defD0 = new BlockDefinition("D0", new WorldPoint2(0, 0), Array.Empty<BlockEntityTemplate>(), [new BlockReference("D1", new WorldPoint2(0, 0))]);

        var shallowExpander = new BlockExpander([defD0, defD1, defD2, defD3], new BlockExpansionOptions(MaxNestingDepth: 2));
        var shallowResult = shallowExpander.Expand([new BlockReference("D0", new WorldPoint2(0, 0))]);

        Assert(shallowResult.Diagnostics.Any(d => d.Code == "BLOCK_DEPTH_EXCEEDED"), "Expected BLOCK_DEPTH_EXCEEDED");
    }

    private static void TestTextLengthBudgetAndTruncation()
    {
        var budget = new CadResourceBudget { MaxTextLength = 1000 };
        var guard = new CadBudgetGuard(budget);

        bool ok = guard.CheckTextLength(500, out _);
        Assert(ok, "500 chars should pass");

        bool exceeded = guard.CheckTextLength(1500, out var diag);
        Assert(!exceeded, "1500 chars should fail budget");
        Assert(diag != null && diag.Code == "RESOURCE_BUDGET_EXCEEDED_TEXT_LENGTH", "Expected RESOURCE_BUDGET_EXCEEDED_TEXT_LENGTH");
    }

    private static void TestRasterImageDimensionBudgetGuard()
    {
        var budget = new CadResourceBudget { MaxRasterDimensionPixels = 4096, MaxRasterTotalPixels = 15_000_000 };
        var guard = new CadBudgetGuard(budget);

        bool normal = guard.CheckRasterDimensions(1920, 1080, out _);
        Assert(normal, "1080p raster should pass");

        bool oversizedDim = guard.CheckRasterDimensions(8192, 100, out var diagDim);
        Assert(!oversizedDim, "8192 width should exceed 4096 max dimension");
        Assert(diagDim != null && diagDim.Code == "RESOURCE_BUDGET_EXCEEDED_RASTER_DIMENSIONS", "Expected RESOURCE_BUDGET_EXCEEDED_RASTER_DIMENSIONS");

        bool bomb = guard.CheckRasterDimensions(4000, 4000, out var diagBomb); // 16M pixels > 15M, each dim <= 4096
        Assert(!bomb, "16 MP should exceed 15 MP total budget");
        Assert(diagBomb != null && diagBomb.Code == "RESOURCE_BUDGET_EXCEEDED_RASTER_PIXELS", "Expected RESOURCE_BUDGET_EXCEEDED_RASTER_PIXELS");
    }

    private static void TestNanInfinityAndExtremeCoordinatesSanityGuard()
    {
        double nanCoord = double.NaN;
        double infCoord = double.PositiveInfinity;
        double extremeCoord = 1e15; // Beyond 1e12

        Assert(!CadSanityGuards.IsValidCoordinate(nanCoord), "NaN should be invalid");
        Assert(!CadSanityGuards.IsValidCoordinate(infCoord), "Infinity should be invalid");
        Assert(!CadSanityGuards.IsValidCoordinate(extremeCoord), "Extreme coordinate should be invalid");

        bool sanitizedNan = CadSanityGuards.SanitizeCoordinate(ref nanCoord, fallback: 42.0);
        Assert(!sanitizedNan, "SanitizeCoordinate should report invalid original");
        Assert(Math.Abs(nanCoord - 42.0) < 1e-9, "NaN should be replaced by fallback");

        double minX = double.NaN, minY = 10.0, maxX = -50.0, maxY = double.PositiveInfinity;
        bool validBounds = CadSanityGuards.SanitizeBounds(ref minX, ref minY, ref maxX, ref maxY);
        Assert(!validBounds, "SanitizeBounds should detect anomalies");
        Assert(!double.IsNaN(minX) && !double.IsInfinity(maxY), "Bounds should be finite after sanitization");
        Assert(minX <= maxX && minY <= maxY, "Bounds min should be <= max after normalization");
    }

    private static void TestBoundedMutationFuzzSmokeZeroCrashesAndSnapshotDeterminism()
    {
        // Deterministic fuzz smoke: generate corrupted variants of DWG header
        int fuzzPasses = 0;
        var rng = new Random(42);
        for (int i = 0; i < 20; i++)
        {
            byte[] mutated = new byte[64];
            rng.NextBytes(mutated);

            // Sometimes inject partial magic
            if (i % 2 == 0)
            {
                mutated[0] = (byte)'A';
                mutated[1] = (byte)'C';
            }

            using var ms = new MemoryStream(mutated);
            // Preflight must never throw an unhandled exception
            var result = CadPreflightInspector.Inspect(ms, $"fuzz_{i}.dwg");
            Assert(result != null, "Preflight must return a valid result");
            fuzzPasses++;
        }

        // Snapshot determinism verification
        var preflight = new CadPreflightResult(
            CadPreflightStatus.Valid,
            CadFormat.Dwg,
            "AC1032",
            "CAD_PREFLIGHT_DWG_VALID",
            "Valid AutoCAD DWG header (AC1032).",
            1048576);

        var budget = CadResourceBudget.Default;
        var diagnostics = new List<CadDiagnostic>
        {
            new("RESOURCE_BUDGET_EXCEEDED_ENTITIES", DiagnosticSeverity.Warning, "Entity count reached limit."),
            new("CAD_PREFLIGHT_DWG_VALID", DiagnosticSeverity.Info, "Preflight check passed.")
        };

        var snapshot1 = ResourceGuardsSemanticSnapshot.Create(preflight, budget, diagnostics, cycleDetected: false, nanSanitized: true, fuzzTestPasses: fuzzPasses);
        var snapshot2 = ResourceGuardsSemanticSnapshot.Create(preflight, budget, diagnostics, cycleDetected: false, nanSanitized: true, fuzzTestPasses: fuzzPasses);

        Assert(snapshot1.Sha256Hash == snapshot2.Sha256Hash, "Snapshot hash must be strictly deterministic");
        Assert(snapshot1.Text.Contains("schema=resource-guards/v1"), "Snapshot must specify resource-guards/v1 schema");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Assertion failed: {message}");
        }
    }
}