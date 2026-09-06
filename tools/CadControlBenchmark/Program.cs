using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadControlBenchmark.Suites;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

namespace CadControlBenchmark;

public static class Program
{
    private static readonly List<(string Suite, string TestName, bool Passed, string Details)> Results = new();

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("================================================================================");
        Console.WriteLine("   MOBIL DWG - GENİŞLETİLMİŞ VE SPESİFİK CAD TEST ORTAMI & BENCHMARK SÜİTİ");
        Console.WriteLine("   Endüstri & GitHub Standartları: Derin Jest, NURBS, Miras, Pafta, ReDoS, Stres");
        Console.WriteLine("================================================================================\n");

        var swOverall = Stopwatch.StartNew();

        try
        {
            await RunBaseFileIngestionSuite();
            GestureDeepStressSuite.Run(RecordResult);
            GeometryDeepFidelitySuite.Run(RecordResult);
            BlockHierarchyDeepSuite.Run(RecordResult);
            LayoutMultiViewportDeepSuite.Run(RecordResult);
            TextMTextDeepSuite.Run(RecordResult);
            await HighLoadPerformanceStressSuite.RunAsync(RecordResult);
            SecurityBombSuite.Run(RecordResult);
            await MobileRealUserSuite.RunAsync(RecordResult);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL ERROR] Test ortamında beklenmeyen hata: {ex}");
            Console.ResetColor();
            return 1;
        }

        swOverall.Stop();

        PrintSummaryReport(swOverall.ElapsedMilliseconds);

        return Results.All(r => r.Passed) ? 0 : 2;
    }

    private static async Task RunBaseFileIngestionSuite()
    {
        Console.WriteLine("=== [SUITE 0] DOSYA AÇMA, BAŞLIK DOĞRULAMA VE FORMAT KONTROLLERİ ===");

        var repoRoot = FindRepoRoot();
        var reader = new AcadSharpDocumentReader();

        // 1. Gerçek İkili DWG (AC1015)
        var dwgPath = Path.Combine(repoRoot, "artifacts", "stage03", "synthetic_turkish_basic_ac1015.dwg");
        if (File.Exists(dwgPath))
        {
            var swDwg = Stopwatch.StartNew();
            await using var stream = File.OpenRead(dwgPath);
            await using var session = await reader.OpenAsync(new CadOpenRequest(stream, Path.GetFileName(dwgPath), stream.Length, LeaveOpen: true));
            swDwg.Stop();

            bool passed = session.Metadata.Format == CadFormat.Dwg &&
                          session.Metadata.AcadVersion == "AC1015" &&
                          session.Handle != null;
            RecordResult("Dosya Formatı", "Gerçek İkili DWG (AC1015) Açma ve Versiyon Doğrulama", passed,
                $"Format={session.Metadata.Format}, Sürüm={session.Metadata.AcadVersion}, Süre={swDwg.ElapsedMilliseconds} ms");
        }

        // 2. Gerçek Metin DXF (AC1015)
        var dxfPath = Path.Combine(repoRoot, "fixtures", "public", "synthetic", "synthetic_turkish_basic_ac1015.dxf");
        if (File.Exists(dxfPath))
        {
            var swDxf = Stopwatch.StartNew();
            await using var stream = File.OpenRead(dxfPath);
            await using var session = await reader.OpenAsync(new CadOpenRequest(stream, Path.GetFileName(dxfPath), stream.Length, LeaveOpen: true));
            swDxf.Stop();

            bool passed = session.Metadata.Format == CadFormat.Dxf &&
                          session.Metadata.AcadVersion == "AC1015";
            RecordResult("Dosya Formatı", "Gerçek Metin DXF (AC1015) Açma ve Ayrıştırma", passed,
                $"Format={session.Metadata.Format}, Sürüm={session.Metadata.AcadVersion}, Süre={swDxf.ElapsedMilliseconds} ms");
        }

        // 3. Yabancı Formatların Güvenli Reddi
        var invalidPayloads = new Dictionary<string, byte[]>
        {
            ["Windows PE (.exe)"] = Encoding.ASCII.GetBytes("MZ\x90\x00\x03\x00\x00\x00\x04\x00\x00\x00"),
            ["Linux ELF (.so)"] = "\x7fELF\x02\x01\x01\x00\x00\x00\x00\x00"u8.ToArray(),
            ["ZIP Arşivi"] = "PK\x03\x04\x14\x00\x00\x00\x08\x00"u8.ToArray(),
            ["HTML Hata Sayfası"] = "<!DOCTYPE html><html><body><h1>404 Not Found</h1></body></html>"u8.ToArray(),
            ["Boş Dosya (0 bayt)"] = Array.Empty<byte>(),
            ["Bozuk DWG Başlığı"] = "AC9999INVALIDDATA"u8.ToArray()
        };

        int rejectedCount = 0;
        foreach (var (name, payload) in invalidPayloads)
        {
            try
            {
                using var mem = new MemoryStream(payload);
                await reader.OpenAsync(new CadOpenRequest(mem, "test", payload.Length));
            }
            catch (Exception ex) when (ex is InvalidDataException or NotSupportedException or IOException)
            {
                rejectedCount++;
            }
        }

        RecordResult("Dosya Formatı", "Yabancı Formatların Güvenli Reddi (PE, ELF, ZIP, HTML, Boş)",
            rejectedCount == invalidPayloads.Count,
            $"{rejectedCount}/{invalidPayloads.Count} yabancı format çökme olmaksızın güvenle reddedildi.");
    }

    public static void RecordResult(string suite, string testName, bool passed, string details)
    {
        Results.Add((suite, testName, passed, details));

        if (passed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  [PASS] ");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  [FAIL] ");
        }

        Console.ResetColor();
        Console.WriteLine($"{testName}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"         ↳ {details}");
        Console.ResetColor();
    }

    private static void PrintSummaryReport(long totalElapsedMs)
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("                    GENİŞLETİLMİŞ CAD TEST ORTAMI ÖZETİ");
        Console.WriteLine("================================================================================");

        var suites = Results.GroupBy(r => r.Suite).ToList();
        int totalPassed = Results.Count(r => r.Passed);
        int totalTests = Results.Count;

        foreach (var suite in suites)
        {
            int pPassed = suite.Count(r => r.Passed);
            int pTotal = suite.Count();
            string status = pPassed == pTotal ? "TAMAMLANDI (PASS)" : "BAŞARISIZ (FAIL)";
            Console.WriteLine($"  • {suite.Key,-20}: {pPassed,2}/{pTotal,2} Test Geçti [{status}]");
        }

        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine($"  Toplam Başarı Oranı : %{(double)totalPassed / totalTests * 100:F1} ({totalPassed}/{totalTests} Test)");
        Console.WriteLine($"  Toplam Yürütme Süresi: {totalElapsedMs} ms");
        Console.WriteLine("================================================================================\n");
    }

    private static string FindRepoRoot()
    {
        var current = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "MobilDwg.sln")))
            {
                return current;
            }
            current = Directory.GetParent(current)?.FullName;
        }
        return Directory.GetCurrentDirectory();
    }
}
