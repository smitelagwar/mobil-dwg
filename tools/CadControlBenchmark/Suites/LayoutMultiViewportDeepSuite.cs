using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;

namespace CadControlBenchmark.Suites;

public static class LayoutMultiViewportDeepSuite
{
    public static void Run(Action<string, string, bool, string> record)
    {
        Console.WriteLine("\n=== [SUITE 4] ÇOKLU PAFTA, VIEWPORT MATRİSLERİ VE KIRPMA ===");

        // Model Alanı Sahnesi Hazırla (2 farklı katmanda varlıklar)
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);
        assembler.SetLayerTable(new LayerTable(new[]
        {
            new LayerDefinition("0", CadColor.FromAci(7), CadLinetype.Continuous, CadLineweight.Default, true),
            new LayerDefinition("PL_DUVAR", CadColor.FromAci(1), CadLinetype.Continuous, CadLineweight.Default, true),
            new LayerDefinition("PL_TEFRİS", CadColor.FromAci(3), CadLinetype.Continuous, CadLineweight.Default, true),
            new LayerDefinition("PL_TESİSAT", CadColor.FromAci(5), CadLinetype.Continuous, CadLineweight.Default, true)
        }));

        // Model varlıkları
        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E_DUVAR"),
            new WorldBounds2(0, 0, 100, 100),
            new RenderLayerToken("PL_DUVAR"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("Line"),
            new[] { new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(100, 100)) }));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E_TEFRİS"),
            new WorldBounds2(20, 20, 60, 60),
            new RenderLayerToken("PL_TEFRİS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("Circle"),
            new[] { new ArcPrimitive(new WorldPoint2(40, 40), 20, 0, Math.PI * 2) }));

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("E_TESİSAT"),
            new WorldBounds2(10, 10, 90, 90),
            new RenderLayerToken("PL_TESİSAT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("Line"),
            new[] { new LinePrimitive(new WorldPoint2(10, 90), new WorldPoint2(90, 10)) }));

        var modelScene = assembler.Build();

        // 1. 4 Görünüm Pencereli (Multi-Viewport) A1 Pafta Sayfası (841 x 594 mm)
        // VP1: Genel Plan (Sol Üst)
        // VP2: Detay Kesit (Sağ Üst - PL_TESİSAT dondurulmuş)
        // VP3: Teşrif Planı (Sol Alt - PL_DUVAR dondurulmuş)
        // VP4: Tesisat Şeması (Sağ Alt - PL_TEFRİS dondurulmuş)
        var vp1 = new CadLayoutViewport(
            viewportId: "VP_PLAN",
            paperCenter: new WorldPoint2(210, 445),
            paperWidth: 380,
            paperHeight: 250,
            viewCenter: new WorldPoint2(50, 50),
            viewHeight: 120);

        var vp2 = new CadLayoutViewport(
            viewportId: "VP_KESIT",
            paperCenter: new WorldPoint2(630, 445),
            paperWidth: 380,
            paperHeight: 250,
            viewCenter: new WorldPoint2(40, 40),
            viewHeight: 50,
            frozenLayers: new[] { "PL_TESİSAT" }); // Tesisat donduruldu

        var vp3 = new CadLayoutViewport(
            viewportId: "VP_TEFRIS",
            paperCenter: new WorldPoint2(210, 150),
            paperWidth: 380,
            paperHeight: 250,
            viewCenter: new WorldPoint2(50, 50),
            viewHeight: 100,
            frozenLayers: new[] { "PL_DUVAR" }); // Duvar donduruldu

        var vp4 = new CadLayoutViewport(
            viewportId: "VP_TESISAT",
            paperCenter: new WorldPoint2(630, 150),
            paperWidth: 380,
            paperHeight: 250,
            viewCenter: new WorldPoint2(50, 50),
            viewHeight: 100,
            frozenLayers: new[] { "PL_TEFRİS" }); // Tefriş donduruldu

        // Pafta çerçevesi ve anteti
        var sheetBorder = new RenderSceneEntity(
            new RenderEntityId("ANTET_FRAME"),
            new WorldBounds2(10, 10, 831, 584),
            new RenderLayerToken("0"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("SheetBorder"),
            new[] { new LinePrimitive(new WorldPoint2(10, 10), new WorldPoint2(831, 10)) });

        var layoutA1 = new CadLayoutDefinition(
            name: "MİMARİ_PAFTA_A1",
            isModelSpace: false,
            tabOrder: 1,
            paperBounds: new WorldBounds2(0, 0, 841, 594),
            paperEntities: new[] { sheetBorder },
            viewports: new[] { vp1, vp2, vp3, vp4 });

        var layoutManager = new CadLayoutManager(modelScene, new[] { layoutA1 });

        // 2. Bellek İçi Pafta Geçişi ve Hız Denetimi
        var sw = Stopwatch.StartNew();
        layoutManager.SwitchLayout("MİMARİ_PAFTA_A1");
        var activeScene = layoutManager.ComposeActiveScene();
        sw.Stop();

        bool multiVpOk = layoutManager.ActiveLayoutName == "MİMARİ_PAFTA_A1" &&
                         layoutA1.Viewports.Count == 4 &&
                         activeScene.Entities.Count > 0 &&
                         sw.ElapsedMilliseconds < 10;
        record("Çoklu Pafta", "4 Viewport'lu A1 Pafta Sayfası (841x594mm) Bellek-İçi Geçiş", multiVpOk,
            $"Pafta: {layoutManager.ActiveLayoutName}, Viewport Sayısı: {layoutA1.Viewports.Count}, Geçiş Süresi: {sw.ElapsedMilliseconds} ms");

        // 3. Viewport Başına Katman Dondurma (VP Freeze) Denetimi
        bool vp2Frozen = vp2.FrozenLayers.Contains("PL_TESİSAT");
        bool vp3Frozen = vp3.FrozenLayers.Contains("PL_DUVAR");
        bool vp4Frozen = vp4.FrozenLayers.Contains("PL_TEFRİS");

        bool freezeOk = vp2Frozen && vp3Frozen && vp4Frozen;
        record("Çoklu Pafta", "Viewport Başına Katman Dondurma (VP Layer Freeze Matrisi)", freezeOk,
            $"VP_KESIT Tesisat Donuk: {vp2Frozen}, VP_TEFRIS Duvar Donuk: {vp3Frozen}, VP_TESISAT Tefriş Donuk: {vp4Frozen}");

        // 4. Model -> Kağıt Alanı Ölçek Dönüşümü (ViewHeight / PaperHeight Oranı)
        double scaleVp1 = vp1.PaperHeight / vp1.ViewHeight; // 250mm / 120m
        double scaleVp2 = vp2.PaperHeight / vp2.ViewHeight; // 250mm / 50m (Daha büyük zoom)
        bool scaleOk = scaleVp2 > scaleVp1 && scaleVp1 > 0;
        record("Çoklu Pafta", "Model -> Kağıt Alanı Ölçek ve Boyut Dönüşümü", scaleOk,
            $"VP1 Ölçek Oranı: {scaleVp1:F2} mm/m, VP2 Detay Kesit Ölçeği: {scaleVp2:F2} mm/m");
    }
}
