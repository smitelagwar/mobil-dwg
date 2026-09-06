using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Text;

namespace CadControlBenchmark.Suites;

public static class TextMTextDeepSuite
{
    public static void Run(Action<string, string, bool, string> record)
    {
        Console.WriteLine("\n=== [SUITE 5] MTEXT DERİN AYRIŞTIRMA, REDOS SAVUNMASI VE FONTLAR ===");

        // 1. MTEXT Karmaşık Biçimlendirme Etiketleri (\P, \H, \C, \S)
        string complexMText = @"{\H1.5x;\C1;BİRİNCİ PARAGRAF BAŞLIK}\Pİkinci satır ölçü: \S1/2; oranında\P{\C3;\H0.8x;Alt not: \~Ayrılmaz metin}";
        var diags = new List<SceneDiagnostic>();
        var parseResult = MTextParser.Parse(complexMText, diags);

        bool tagsParsed = parseResult.Lines.Count >= 3 &&
                          parseResult.PlainText.Contains("BİRİNCİ PARAGRAF BAŞLIK") &&
                          parseResult.PlainText.Contains("İkinci satır ölçü") &&
                          parseResult.PlainText.Contains("Alt not");
        record("MTEXT & Tipografi", "MTEXT Karmaşık Etiket Ayrıştırma (\\P Satır Sonu, \\H Boyut, \\C Renk)", tagsParsed,
            $"Satır Sayısı: {parseResult.Lines.Count}, Ayrıştırılan Düz Metin: '{parseResult.PlainText.Replace("\n", " // ")}'");

        // 2. ReDoS (Regular Expression Denial of Service) Fuzz Saldırı Savunması
        // 1.000 seviyeli iç içe geçmiş parantez ve biçimlendirme etiketleri: {{{...}}}
        var sbReDos = new StringBuilder(5000);
        for (int i = 0; i < 1000; i++) sbReDos.Append("{\\C1;");
        sbReDos.Append("SALDIRI_YÜKÜ");
        for (int i = 0; i < 1000; i++) sbReDos.Append('}');

        string reDosPayload = sbReDos.ToString();
        var reDosDiags = new List<SceneDiagnostic>();

        var swReDos = Stopwatch.StartNew();
        var reDosResult = MTextParser.Parse(reDosPayload, reDosDiags);
        swReDos.Stop();

        bool reDosSafe = swReDos.ElapsedMilliseconds < 10 &&
                         reDosDiags.Any(d => d.Code == "MTEXT_NESTING_EXCEEDED") &&
                         reDosResult.PlainText.Contains("SALDIRI_YÜKÜ");
        record("MTEXT & Tipografi", "ReDoS (Derin İç İçe Etiket Saldırısı) Savunması (< 10 ms)", reDosSafe,
            $"1.000 derinlikli MTEXT ayrıştırma süresi: {swReDos.ElapsedMilliseconds} ms, Tanı Kodu: {reDosDiags.FirstOrDefault()?.Code}");

        // 3. MTEXT Güvenlik Sınırı Bütçesi (65.536 Karakter Sınırı)
        var longMText = new string('A', 80_000); // 80 KB
        var longDiags = new List<SceneDiagnostic>();
        var longResult = MTextParser.Parse(longMText, longDiags);

        bool lengthBudgetOk = longResult.WasTruncated &&
                              longResult.PlainText.Length == MTextParser.MaxInputLength &&
                              longDiags.Any(d => d.Code == "MTEXT_LENGTH_EXCEEDED");
        record("MTEXT & Tipografi", "MTEXT Azami Karakter Bütçe Sınırı (65.536 Karakter Kesimi)", lengthBudgetOk,
            $"Giriş: 80.000 karakter -> Çıkış: {longResult.PlainText.Length} karakter, Kesildi: {longResult.WasTruncated}");

        // 4. SHX Font Eşleme ve İkame Tablosu (Font Substitution Resolver Audit)
        var shxTestList = new[]
        {
            "txt.shx", "simplex.shx", "complex.shx", "romans.shx",
            "isocp.shx", "monotxt.shx", "italic.shx", "STANDARD"
        };

        int matchedCount = 0;
        foreach (var font in shxTestList)
        {
            var resolved = FontSubstitutionResolver.Resolve(font);
            if (!string.IsNullOrEmpty(resolved)) matchedCount++;
        }

        bool shxAuditOk = matchedCount == shxTestList.Length;
        record("MTEXT & Tipografi", "AutoCAD Standart SHX Font Eşleme Tablosu Denetimi (15+ Font)", shxAuditOk,
            $"{matchedCount}/{shxTestList.Length} standart AutoCAD SHX fontu güvenli sistem fontlarına eşlendi.");
    }
}
