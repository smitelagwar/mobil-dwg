using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Scene;

public static class SampleCadDrawings
{
    public static RenderScene CreateArchitecturalPlan()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        var layerTable = new LayerTable(
        [
            new LayerDefinition("0", CadColor.FromArgb(0xFFFFFFFF), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("WALLS", CadColor.FromArgb(0xFFE2E8F0), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("DOORS", CadColor.FromArgb(0xFF10B981), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("WINDOWS", CadColor.FromArgb(0xFF38BDF8), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("FURNITURE", CadColor.FromArgb(0xFFF59E0B), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("DIMENSIONS", CadColor.FromArgb(0xFF06B6D4), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("ROOM_TEXT", CadColor.FromArgb(0xFFF8FAFC), CadLinetype.Continuous, CadLineweight.Default)
        ]);
        assembler.SetLayerTable(layerTable);

        // 1. Dış ve İç Duvarlar (WALLS)
        var wallLines = new List<RenderGeometryPrimitive>
        {
            // Dış sınır
            new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(1200, 0)),
            new LinePrimitive(new WorldPoint2(1200, 0), new WorldPoint2(1200, 800)),
            new LinePrimitive(new WorldPoint2(1200, 800), new WorldPoint2(0, 800)),
            new LinePrimitive(new WorldPoint2(0, 800), new WorldPoint2(0, 0)),

            // Dış duvar kalınlığı (20cm)
            new LinePrimitive(new WorldPoint2(20, 20), new WorldPoint2(1180, 20)),
            new LinePrimitive(new WorldPoint2(1180, 20), new WorldPoint2(1180, 780)),
            new LinePrimitive(new WorldPoint2(1180, 780), new WorldPoint2(20, 780)),
            new LinePrimitive(new WorldPoint2(20, 780), new WorldPoint2(20, 20)),

            // İç bölme duvarı (Salon / Yatak Odası)
            new LinePrimitive(new WorldPoint2(650, 20), new WorldPoint2(650, 780)),
            new LinePrimitive(new WorldPoint2(670, 20), new WorldPoint2(670, 780)),

            // Mutfak / Koridor bölmesi
            new LinePrimitive(new WorldPoint2(20, 450), new WorldPoint2(450, 450)),
            new LinePrimitive(new WorldPoint2(20, 470), new WorldPoint2(450, 470)),

            // Banyo bölmesi
            new LinePrimitive(new WorldPoint2(450, 450), new WorldPoint2(450, 780)),
            new LinePrimitive(new WorldPoint2(470, 450), new WorldPoint2(470, 780))
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A_WALLS"),
            new RenderLayerToken("WALLS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("WALLS"),
            wallLines));

        // 2. Kapılar ve Açılış Yayları (DOORS)
        var doorPrimitives = new List<RenderGeometryPrimitive>
        {
            // Giriş Kapısı
            new LinePrimitive(new WorldPoint2(500, 20), new WorldPoint2(500, 110)),
            new ArcPrimitive(new WorldPoint2(500, 20), 90, 0, Math.PI / 2),

            // Salon Kapısı
            new LinePrimitive(new WorldPoint2(650, 250), new WorldPoint2(560, 250)),
            new ArcPrimitive(new WorldPoint2(650, 250), 90, Math.PI / 2, Math.PI / 2),

            // Yatak Odası Kapısı
            new LinePrimitive(new WorldPoint2(670, 400), new WorldPoint2(760, 400)),
            new ArcPrimitive(new WorldPoint2(670, 400), 90, 0, Math.PI / 2)
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A_DOORS"),
            new RenderLayerToken("DOORS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("DOORS"),
            doorPrimitives));

        // 3. Pencereler (WINDOWS)
        var windowPrimitives = new List<RenderGeometryPrimitive>
        {
            // Salon Penceresi (Alt cephe)
            new LinePrimitive(new WorldPoint2(150, 0), new WorldPoint2(350, 0)),
            new LinePrimitive(new WorldPoint2(150, 20), new WorldPoint2(350, 20)),
            new LinePrimitive(new WorldPoint2(150, 10), new WorldPoint2(350, 10)),

            // Yatak Odası Penceresi (Sağ cephe)
            new LinePrimitive(new WorldPoint2(1200, 250), new WorldPoint2(1200, 450)),
            new LinePrimitive(new WorldPoint2(1180, 250), new WorldPoint2(1180, 450)),
            new LinePrimitive(new WorldPoint2(1190, 250), new WorldPoint2(1190, 450)),

            // Mutfak Penceresi (Üst cephe)
            new LinePrimitive(new WorldPoint2(150, 800), new WorldPoint2(350, 800)),
            new LinePrimitive(new WorldPoint2(150, 780), new WorldPoint2(350, 780))
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A_WINDOWS"),
            new RenderLayerToken("WINDOWS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("WINDOWS"),
            windowPrimitives));

        // 4. Mobilyalar (FURNITURE)
        var furnPrimitives = new List<RenderGeometryPrimitive>
        {
            // Yemek Masası ve Sandalyeler
            new LinePrimitive(new WorldPoint2(150, 200), new WorldPoint2(350, 200)),
            new LinePrimitive(new WorldPoint2(350, 200), new WorldPoint2(350, 320)),
            new LinePrimitive(new WorldPoint2(350, 320), new WorldPoint2(150, 320)),
            new LinePrimitive(new WorldPoint2(150, 320), new WorldPoint2(150, 200)),
            // Sandalyeler
            new ArcPrimitive(new WorldPoint2(200, 170), 20, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(300, 170), 20, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(200, 350), 20, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(300, 350), 20, 0, Math.PI * 2),

            // Çift Kişilik Yatak (Yatak Odası)
            new LinePrimitive(new WorldPoint2(850, 500), new WorldPoint2(1150, 500)),
            new LinePrimitive(new WorldPoint2(1150, 500), new WorldPoint2(1150, 750)),
            new LinePrimitive(new WorldPoint2(1150, 750), new WorldPoint2(850, 750)),
            new LinePrimitive(new WorldPoint2(850, 750), new WorldPoint2(850, 500)),
            // Yastıklar
            new LinePrimitive(new WorldPoint2(870, 700), new WorldPoint2(980, 700)),
            new LinePrimitive(new WorldPoint2(1020, 700), new WorldPoint2(1130, 700))
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A_FURNITURE"),
            new RenderLayerToken("FURNITURE"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("FURNITURE"),
            furnPrimitives));

        // 5. Mahal İsimleri ve Metinler (ROOM_TEXT)
        var textPrimitives = new List<RenderGeometryPrimitive>
        {
            new TextPrimitive("SALON - 32 m²", new WorldPoint2(180, 120), height: 26),
            new TextPrimitive("EBEVEYN YATAK ODASI - 22 m²", new WorldPoint2(720, 380), height: 24),
            new TextPrimitive("MUTFAK - 14 m²", new WorldPoint2(100, 580), height: 22),
            new TextPrimitive("BANYO & WC - 7 m²", new WorldPoint2(490, 600), height: 20),
            new TextPrimitive("ANTRE / KORİDOR", new WorldPoint2(500, 300), height: 18),
            new TextPrimitive("APARTMAN 3+1 KAT PLANI (1:50)", new WorldPoint2(50, -50), height: 32)
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A_ROOM_TEXT"),
            new RenderLayerToken("ROOM_TEXT"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ROOM_TEXT"),
            textPrimitives));

        // 6. Ölçülendirme Çizgileri (DIMENSIONS)
        var dimPrimitives = new List<RenderGeometryPrimitive>
        {
            // Alt genel ölçü (1200 cm)
            new LinePrimitive(new WorldPoint2(0, -30), new WorldPoint2(1200, -30)),
            new LinePrimitive(new WorldPoint2(0, -20), new WorldPoint2(0, -40)),
            new LinePrimitive(new WorldPoint2(1200, -20), new WorldPoint2(1200, -40)),
            new TextPrimitive("12.00 m", new WorldPoint2(560, -25), height: 18),

            // Sağ genel ölçü (800 cm)
            new LinePrimitive(new WorldPoint2(1230, 0), new WorldPoint2(1230, 800)),
            new LinePrimitive(new WorldPoint2(1220, 0), new WorldPoint2(1240, 0)),
            new LinePrimitive(new WorldPoint2(1220, 800), new WorldPoint2(1240, 800)),
            new TextPrimitive("8.00 m", new WorldPoint2(1245, 380), height: 18)
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("A_DIMENSIONS"),
            new RenderLayerToken("DIMENSIONS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("DIMENSIONS"),
            dimPrimitives));

        return assembler.Build();
    }

    public static RenderScene CreateMechanicalPart()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        var layerTable = new LayerTable(
        [
            new LayerDefinition("0", CadColor.FromArgb(0xFFFFFFFF), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("OUTLINE", CadColor.FromArgb(0xFFFFFFFF), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("CENTERLINES", CadColor.FromArgb(0xFFEF4444), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("BOLT_HOLES", CadColor.FromArgb(0xFF38BDF8), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("HATCH", CadColor.FromArgb(0xFFF59E0B), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("DIMENSIONS", CadColor.FromArgb(0xFF10B981), CadLinetype.Continuous, CadLineweight.Default)
        ]);
        assembler.SetLayerTable(layerTable);

        var outline = new List<RenderGeometryPrimitive>
        {
            // Dış flanş gövdesi (Ø300)
            new ArcPrimitive(new WorldPoint2(300, 300), 200, 0, Math.PI * 2),
            // İç kanal çemberi (Ø180)
            new ArcPrimitive(new WorldPoint2(300, 300), 120, 0, Math.PI * 2),
            // Merkez mil deliği (Ø70)
            new ArcPrimitive(new WorldPoint2(300, 300), 50, 0, Math.PI * 2),
            // Kama kanalı
            new LinePrimitive(new WorldPoint2(285, 345), new WorldPoint2(285, 365)),
            new LinePrimitive(new WorldPoint2(285, 365), new WorldPoint2(315, 365)),
            new LinePrimitive(new WorldPoint2(315, 365), new WorldPoint2(315, 345))
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("M_OUTLINE"),
            new RenderLayerToken("OUTLINE"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("OUTLINE"),
            outline));

        // 8 Cıvata Yuvası
        var boltHoles = new List<RenderGeometryPrimitive>();
        double pcd = 160.0;
        for (int i = 0; i < 8; i++)
        {
            double angle = i * (Math.PI / 4);
            double bx = 300 + pcd * Math.Cos(angle);
            double by = 300 + pcd * Math.Sin(angle);
            boltHoles.Add(new ArcPrimitive(new WorldPoint2(bx, by), 16, 0, Math.PI * 2));
        }

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("M_BOLTS"),
            new RenderLayerToken("BOLT_HOLES"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("BOLT_HOLES"),
            boltHoles));

        // Eksen Çizgileri
        var centerlines = new List<RenderGeometryPrimitive>
        {
            new LinePrimitive(new WorldPoint2(70, 300), new WorldPoint2(530, 300)),
            new LinePrimitive(new WorldPoint2(300, 70), new WorldPoint2(300, 530)),
            // PCD dairesi
            new ArcPrimitive(new WorldPoint2(300, 300), pcd, 0, Math.PI * 2)
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("M_CENTER"),
            new RenderLayerToken("CENTERLINES"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("CENTERLINES"),
            centerlines));

        // Ölçüler ve Notlar
        var dims = new List<RenderGeometryPrimitive>
        {
            new TextPrimitive("MEKANİK BAĞLANTI FLANŞI - DN150", new WorldPoint2(100, 550), height: 26),
            new TextPrimitive("8x Ø32 EŞİT ARALIKLI DELİK (PCD Ø320)", new WorldPoint2(80, 20), height: 18),
            new TextPrimitive("MALZEME: AISI 316L PASLANMAZ ÇELİK", new WorldPoint2(80, -10), height: 16)
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("M_DIMS"),
            new RenderLayerToken("DIMENSIONS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("DIMENSIONS"),
            dims));

        return assembler.Build();
    }

    public static RenderScene CreateSurveyMap()
    {
        var assembler = new RenderSceneAssembler(RenderColorContext.Dark);

        var layerTable = new LayerTable(
        [
            new LayerDefinition("0", CadColor.FromArgb(0xFFFFFFFF), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("PARCELS", CadColor.FromArgb(0xFFFBBF24), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("ROADS", CadColor.FromArgb(0xFF94A3B8), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("SURVEY_POINTS", CadColor.FromArgb(0xFFEF4444), CadLinetype.Continuous, CadLineweight.Default),
            new LayerDefinition("TEXT_LABELS", CadColor.FromArgb(0xFFF8FAFC), CadLinetype.Continuous, CadLineweight.Default)
        ]);
        assembler.SetLayerTable(layerTable);

        // Yol Hatları
        var roads = new List<RenderGeometryPrimitive>
        {
            new LinePrimitive(new WorldPoint2(100, 100), new WorldPoint2(1100, 100)),
            new LinePrimitive(new WorldPoint2(100, 200), new WorldPoint2(1100, 200)),
            new LinePrimitive(new WorldPoint2(550, 200), new WorldPoint2(550, 750)),
            new LinePrimitive(new WorldPoint2(650, 200), new WorldPoint2(650, 750))
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("S_ROADS"),
            new RenderLayerToken("ROADS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("ROADS"),
            roads));

        // Parseller
        var parcels = new List<RenderGeometryPrimitive>
        {
            // Parsel 1
            new LinePrimitive(new WorldPoint2(150, 250), new WorldPoint2(500, 250)),
            new LinePrimitive(new WorldPoint2(500, 250), new WorldPoint2(500, 650)),
            new LinePrimitive(new WorldPoint2(500, 650), new WorldPoint2(150, 650)),
            new LinePrimitive(new WorldPoint2(150, 650), new WorldPoint2(150, 250)),

            // Parsel 2
            new LinePrimitive(new WorldPoint2(700, 250), new WorldPoint2(1050, 250)),
            new LinePrimitive(new WorldPoint2(1050, 250), new WorldPoint2(1050, 650)),
            new LinePrimitive(new WorldPoint2(1050, 650), new WorldPoint2(700, 650)),
            new LinePrimitive(new WorldPoint2(700, 650), new WorldPoint2(700, 250))
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("S_PARCELS"),
            new RenderLayerToken("PARCELS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("PARCELS"),
            parcels));

        // Poligon Noktaları (Kırmızı Üçgen / Noktalar)
        var pts = new List<RenderGeometryPrimitive>
        {
            new ArcPrimitive(new WorldPoint2(150, 250), 8, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(500, 250), 8, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(500, 650), 8, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(150, 650), 8, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(700, 250), 8, 0, Math.PI * 2),
            new ArcPrimitive(new WorldPoint2(1050, 250), 8, 0, Math.PI * 2)
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("S_POINTS"),
            new RenderLayerToken("SURVEY_POINTS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("SURVEY_POINTS"),
            pts));

        // Etiketler
        var labels = new List<RenderGeometryPrimitive>
        {
            new TextPrimitive("ATATÜRK BULVARI (Genişlik: 20m)", new WorldPoint2(350, 140), height: 26),
            new TextPrimitive("ADA: 1420  PARSEL: 1 (1.400 m²)", new WorldPoint2(200, 450), height: 24),
            new TextPrimitive("ADA: 1420  PARSEL: 2 (1.400 m²)", new WorldPoint2(750, 450), height: 24),
            new TextPrimitive("P.101 (Y: 5002150.25, X: 4235100.80)", new WorldPoint2(165, 240), height: 16),
            new TextPrimitive("KADASTRO ÇAP VE İMAR PLANI (ED50 / UTM 3°)", new WorldPoint2(200, 720), height: 28),
            new TextPrimitive("▲ KUZEY", new WorldPoint2(1050, 720), height: 24)
        };

        assembler.AddEntity(new RenderSceneEntity(
            new RenderEntityId("S_LABELS"),
            new RenderLayerToken("TEXT_LABELS"),
            new RenderStyleToken("BYLAYER"),
            new RenderSourceReference("TEXT_LABELS"),
            labels));

        return assembler.Build();
    }
}
