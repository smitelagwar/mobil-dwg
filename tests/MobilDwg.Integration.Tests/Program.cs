using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Integration.Tests;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var repoRoot = FindRepoRoot();
            var reader = new AcadSharpDocumentReader();

            // 1. Test real DXF fixture
            var dxfPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "synthetic_turkish_basic_ac1015.dxf");
            Assert(File.Exists(dxfPath), $"DXF fixture missing: {dxfPath}");

            await using (var stream = File.OpenRead(dxfPath))
            {
                var request = new CadOpenRequest(stream, Path.GetFileName(dxfPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);

                Assert(session.Metadata.Format == CadFormat.Dxf, $"Expected DXF format, got {session.Metadata.Format}");
                Assert(session.Metadata.AcadVersion == "AC1015", $"Expected AC1015, got {session.Metadata.AcadVersion}");
                Assert(session.Handle != null, "Session handle was null");

                var extracted = AcadSharpEntityExtractor.Extract(session.Handle!);
                Assert(extracted.Entities.Count > 0, $"Expected extracted entities > 0, got {extracted.Entities.Count}");
                Assert(extracted.Layers.Count > 0, $"Expected extracted layers > 0, got {extracted.Layers.Count}");

                var scene = CadExtractedSceneBuilder.Build(extracted);
                Assert(scene.Entities.Count > 0, $"Expected scene entities > 0, got {scene.Entities.Count}");
                Assert(scene.WorldBounds.HasValue && scene.WorldBounds.Value.Width > 0 && scene.WorldBounds.Value.Height > 0,
                    $"Expected positive world bounds, got {scene.WorldBounds}");
            }

            // 2. Test real DWG fixture if present
            var dwgPath = Path.Combine(repoRoot, "artifacts", "stage03", "synthetic_turkish_basic_ac1015.dwg");
            if (File.Exists(dwgPath))
            {
                await using var stream = File.OpenRead(dwgPath);
                var request = new CadOpenRequest(stream, Path.GetFileName(dwgPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);

                Assert(session.Metadata.Format == CadFormat.Dwg, $"Expected DWG format, got {session.Metadata.Format}");
                Assert(session.Handle != null, "DWG session handle was null");

                var extracted = AcadSharpEntityExtractor.Extract(session.Handle!);
                Assert(extracted.Entities.Count > 0, "DWG extracted entity count was 0");

                var scene = CadExtractedSceneBuilder.Build(extracted);
                Assert(scene.Entities.Count > 0, "DWG scene entity count was 0");
            }

            // 3. Test negative fixtures (missing font, missing xref)
            var negFontPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "negative_missing_font_ac1015.dxf");
            if (File.Exists(negFontPath))
            {
                await using var stream = File.OpenRead(negFontPath);
                var request = new CadOpenRequest(stream, Path.GetFileName(negFontPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);
                Assert(session.Handle != null, "Negative font session handle was null");
            }

            var negXrefPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "negative_missing_xref_ac1015.dxf");
            if (File.Exists(negXrefPath))
            {
                await using var stream = File.OpenRead(negXrefPath);
                var request = new CadOpenRequest(stream, Path.GetFileName(negXrefPath), stream.Length, LeaveOpen: true);
                await using var session = await reader.OpenAsync(request);
                Assert(session.Handle != null, "Negative xref session handle was null");
            }

            Console.WriteLine("STAGE01_INTEGRATION_TESTS_PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"INTEGRATION_TESTS_FAILED: {ex}");
            return 1;
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "MobilDwg.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not find repo root with MobilDwg.sln");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
