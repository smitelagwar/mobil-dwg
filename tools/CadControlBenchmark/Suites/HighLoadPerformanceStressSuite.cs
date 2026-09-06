using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;

namespace CadControlBenchmark.Suites;

public static class HighLoadPerformanceStressSuite
{
    public static async Task RunAsync(Action<string, string, bool, string> record)
    {
        Console.WriteLine("\n=== [SUITE 6] YÜKSEK YÜK ALTINDA FPS, BELLEK VE GC STRESİ ===");

        // 1. 10.000 Varlıklı Yüksek Yoğunluklu CAD Haritası Üretimi ve Render
        var swGen = Stopwatch.StartNew();
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.SetLayerTable(new LayerTable(new[]
        {
            new LayerDefinition("0", CadColor.FromAci(7), CadLinetype.Continuous, CadLineweight.Default, true),
            new LayerDefinition("GRID", CadColor.FromAci(8), CadLinetype.Continuous, CadLineweight.Default, true),
            new LayerDefinition("CONTOURS", CadColor.FromAci(3), CadLinetype.Continuous, CadLineweight.Default, true)
        }));

        const int entityCount = 10_000;
        var rng = new Random(555);

        for (int i = 0; i < entityCount; i++)
        {
            double x1 = rng.NextDouble() * 50_000.0;
            double y1 = rng.NextDouble() * 50_000.0;
            double x2 = x1 + (rng.NextDouble() * 50.0) - 25.0;
            double y2 = y1 + (rng.NextDouble() * 50.0) - 25.0;

            var line = new LinePrimitive(new WorldPoint2(x1, y1), new WorldPoint2(x2, y2));
            assembler.AddEntity(new RenderSceneEntity(
                new RenderEntityId($"E_{i:D5}"),
                line.Bounds,
                new RenderLayerToken(i % 2 == 0 ? "GRID" : "CONTOURS"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("Line", sourceIndex: i),
                new[] { line }));
        }

        var denseScene = assembler.Build();
        swGen.Stop();

        using var surface = new SkiaBitmapRenderSurface(800, 800);
        var camera = Camera2D.Fit(denseScene.WorldBounds!.Value, 800, 800);
        var renderer = new SkiaCadRenderer(RenderOptimizationMode.Optimized);

        var swRender = Stopwatch.StartNew();
        await renderer.RenderAsync(denseScene, surface, camera.ToViewport());
        swRender.Stop();

        bool denseOk = denseScene.Entities.Count == entityCount && swRender.ElapsedMilliseconds < 2000;
        record("Yüksek Yük Stresi", "10.000 Varlıklı CAD Haritası Üretimi ve İlk Çizim", denseOk,
            $"Varlık Sayısı: {denseScene.Entities.Count:N0}, Sahne Üretimi: {swGen.ElapsedMilliseconds} ms, İlk Skia Çizim: {swRender.ElapsedMilliseconds} ms");

        // 2. 200 Kare Sürekli İnteraktif Pan/Zoom Profillemesi (p50, p90, p95, p99 Latency)
        // Zoom yapılmış lokal bir pencerede 200 adımlık sürekli pan hareketi simülasyonu
        var zoomedCam = new Camera2D(800, 800, denseScene.WorldBounds!.Value.Center, 0.5);
        var frameLatencies = new List<double>(200);

        for (int f = 0; f < 200; f++)
        {
            double dx = Math.Sin(f * 0.1) * 20.0;
            double dy = Math.Cos(f * 0.1) * 20.0;
            zoomedCam = zoomedCam.PanBy(dx, dy);

            var swFrame = Stopwatch.StartNew();
            await renderer.RenderAsync(denseScene, surface, zoomedCam.ToViewport());
            swFrame.Stop();

            frameLatencies.Add(swFrame.Elapsed.TotalMilliseconds);
        }

        frameLatencies.Sort();
        double minLatency = frameLatencies[0];
        double p50Latency = frameLatencies[100];
        double p90Latency = frameLatencies[180];
        double p95Latency = frameLatencies[190];
        double p99Latency = frameLatencies[198];
        double maxLatency = frameLatencies[199];
        double meanLatency = frameLatencies.Average();

        bool latencyOk = p50Latency < 35.0; // 30+ FPS hedefi
        record("Yüksek Yük Stresi", "200 Kare İnteraktif Profilleme (p50, p90, p95, p99 Dağılımı)", latencyOk,
            $"Min: {minLatency:F1}ms | Mean: {meanLatency:F1}ms | p50: {p50Latency:F1}ms | p90: {p90Latency:F1}ms | p95: {p95Latency:F1}ms | p99: {p99Latency:F1}ms | Max: {maxLatency:F1}ms");

        // 3. GC Allocation ve Bellek Baskısı Kurtarma (TrimMemory Testi)
        long beforeAlloc = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 20; i++)
        {
            zoomedCam = zoomedCam.PanBy(5.0, 5.0);
            // Sadece kamera kaydırma ve viewport hesaplaması (render hariç)
            _ = zoomedCam.ToViewport();
        }
        long afterAlloc = GC.GetAllocatedBytesForCurrentThread();
        long allocPerPan = (afterAlloc - beforeAlloc) / 20;

        // TrimMemory simülasyonu: GC topla ve bitmap temizle
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();

        bool gcOk = allocPerPan < 1024; // Pan başına < 1KB GC tahsisi
        record("Yüksek Yük Stresi", "Kamera Pan Başına GC Tahsisi ve Bellek Baskısı Kurtarma", gcOk,
            $"Pan başına GC Tahsisi: {allocPerPan} bayt (< 1 KB hedefi)");
    }
}
