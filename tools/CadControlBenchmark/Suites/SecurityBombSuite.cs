using System;
using System.Collections.Generic;
using System.Linq;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Guards;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace CadControlBenchmark.Suites;

public static class SecurityBombSuite
{
    public static void Run(Action<string, string, bool, string> record)
    {
        Console.WriteLine("\n=== [SUITE 7] BELLEK BOMBASI, KOTASYON VE GÜVENLİK SINIRLARI ===");

        var budgetGuard = new CadBudgetGuard();

        // 1. Raster Dekompresyon Bombası Koruması (100.000 x 100.000 Piksel / 10 Gigapiksel)
        // Kötü niyetli bir CAD dosyasında minik PNG/JPG başlığıyla devasa bellek tahsisatı tuzağı
        bool bombDetected = !budgetGuard.CheckRasterDimensions(100_000, 100_000, out var bombDiag);
        bool bombCodeMatch = bombDiag?.Code == "RESOURCE_BUDGET_EXCEEDED_RASTER_DIMENSIONS";
        record("Güvenlik & Bomba", "Raster Dekompresyon Bombası Koruması (100.000x100.000 px)", bombDetected && bombCodeMatch,
            $"Bomba Tespit Edildi: {bombDetected}, Tanı Kodu: {bombDiag?.Code}, Mesaj: '{bombDiag?.Message}'");

        // 2. 250.000 Varlık Sahne Kotası Koruması
        bool entityExceeded = !budgetGuard.CheckEntityCount(300_000, out var entityDiag);
        bool entityCodeMatch = entityDiag?.Code == "RESOURCE_BUDGET_EXCEEDED_ENTITIES";
        record("Güvenlik & Bomba", "Maksimum Varlık Sayısı Kotası (250.000 Varlık Limiti)", entityExceeded && entityCodeMatch,
            $"Kota Aşımı Yakalandı: {entityExceeded}, Tanı Kodu: {entityDiag?.Code}");

        // 3. 64 KB Metin Uzunluğu Bombası Koruması
        bool textExceeded = !budgetGuard.CheckTextLength(100_000, out var textDiag);
        bool textCodeMatch = textDiag?.Code == "RESOURCE_BUDGET_EXCEEDED_TEXT_LENGTH";
        record("Güvenlik & Bomba", "Aşırı Metin Uzunluğu Bombası (64 KB Sınırı)", textExceeded && textCodeMatch,
            $"Metin Bombası Yakalandı: {textExceeded}, Tanı Kodu: {textDiag?.Code}");

        // 4. 256 MB Dosya Boyutu Kotası Koruması
        bool fileSizeExceeded = !budgetGuard.CheckFileSize(300 * 1024 * 1024, out var fileDiag);
        bool fileCodeMatch = fileDiag?.Code == "RESOURCE_BUDGET_EXCEEDED_FILE_SIZE";
        record("Güvenlik & Bomba", "Maksimum Dosya Boyutu Kotası (256 MB Limiti)", fileSizeExceeded && fileCodeMatch,
            $"Büyük Dosya Yakalandı: {fileSizeExceeded}, Tanı Kodu: {fileDiag?.Code}");

        // 5. 4 Seviyeli Döngüsel Blok Zinciri Tespiti (A -> B -> C -> A)
        var blockA = new BlockDefinition("CYCLE_A", default, Array.Empty<BlockEntityTemplate>(),
            new[] { new BlockReference("CYCLE_B", default) });
        var blockB = new BlockDefinition("CYCLE_B", default, Array.Empty<BlockEntityTemplate>(),
            new[] { new BlockReference("CYCLE_C", default) });
        var blockC = new BlockDefinition("CYCLE_C", default, Array.Empty<BlockEntityTemplate>(),
            new[] { new BlockReference("CYCLE_A", default) }); // A'ya geri döner

        var expander = new BlockExpander(new[] { blockA, blockB, blockC });
        var cycleResult = expander.Expand(new[] { new BlockReference("CYCLE_A", default) });

        bool cycleDetected = cycleResult.Diagnostics.Any(d => d.Code == "BLOCK_CYCLE_DETECTED");
        record("Güvenlik & Bomba", "4 Seviyeli Döngüsel Blok Zinciri Koruması (A -> B -> C -> A)", cycleDetected,
            $"Döngü Yakalandı: {cycleDetected}, Çökme: 0 (StackOverflow engellendi), Tanı Kodu: {cycleResult.Diagnostics.FirstOrDefault()?.Code}");
    }
}
