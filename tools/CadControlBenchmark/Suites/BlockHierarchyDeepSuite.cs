using System;
using System.Collections.Generic;
using System.Linq;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Transforms;

namespace CadControlBenchmark.Suites;

public static class BlockHierarchyDeepSuite
{
    public static void Run(Action<string, string, bool, string> record)
    {
        Console.WriteLine("\n=== [SUITE 3] DERİN BLOK HİYERARŞİSİ, NİTELİKLER VE MİRAS ===");

        // 1. 30 Seviye İç İçe Blok Hiyerarşisi Zinciri (Chain Matrix Multiplication)
        // Her seviyede (dx=2, dy=1, scale=1.0) ofset verilir. 30 seviye sonunda (60, 30) olmalıdır.
        var blockDefs = new List<BlockDefinition>();
        const int maxDepth = 30;

        for (int d = maxDepth; d >= 1; d--)
        {
            var innerBlockName = $"BLOCK_LEVEL_{d}";
            if (d == maxDepth)
            {
                // En derin blokta tek bir nokta/çizgi
                blockDefs.Add(new BlockDefinition(
                    innerBlockName,
                    basePoint: new WorldPoint2(0, 0),
                    entities: new[]
                    {
                        new BlockEntityTemplate(
                            new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(1, 1)),
                            new RenderLayerToken("0"),
                            new RenderStyleToken("BYLAYER"))
                    }));
            }
            else
            {
                // Bir alt seviyeyi çağıran blok
                var childBlockName = $"BLOCK_LEVEL_{d + 1}";
                blockDefs.Add(new BlockDefinition(
                    innerBlockName,
                    basePoint: new WorldPoint2(0, 0),
                    entities: Array.Empty<BlockEntityTemplate>(),
                    nestedReferences: new[]
                    {
                        new BlockReference(childBlockName, new WorldPoint2(2, 1))
                    }));
            }
        }

        var expander30 = new BlockExpander(blockDefs, new BlockExpansionOptions(MaxNestingDepth: 35));
        var rootRef = new BlockReference("BLOCK_LEVEL_1", new WorldPoint2(2, 1));
        var result30 = expander30.Expand(new[] { rootRef });

        bool depthOk = result30.Entities.Count == 1;
        var endPoint = result30.Entities.FirstOrDefault()?.Geometry.OfType<LinePrimitive>().FirstOrDefault()?.Start;

        // Toplam 30 seviye * (2, 1) = (60, 30)
        bool coordsOk = endPoint.HasValue &&
                        Math.Abs(endPoint.Value.X - 60.0) < 1e-9 &&
                        Math.Abs(endPoint.Value.Y - 30.0) < 1e-9;

        record("Blok Hiyerarşisi", "30 Seviye Derin İç İçe Blok Zinciri ve Kümülatif Matris", depthOk && coordsOk,
            $"30 seviye sonunda hesaplanan uç nokta: ({endPoint?.X:F1}, {endPoint?.Y:F1}), Beklenen: (60.0, 30.0), Tanı Uyarısı: {result30.Diagnostics.Count}");

        // 2. Layer 0 Miras Kuralı (Inheritance Semantics)
        // Blok içindeki varlık "Layer 0" üzerindedir; INSERT referansı "MİMARİ" katmanında ise,
        // genişletilen varlık "MİMARİ" katmanını devralmalıdır.
        var layer0Def = new BlockDefinition(
            "BLOCK_LAYER0",
            basePoint: new WorldPoint2(0, 0),
            entities: new[]
            {
                new BlockEntityTemplate(
                    new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(10, 0)),
                    new RenderLayerToken("0"),
                    new RenderStyleToken("BYLAYER")),
                new BlockEntityTemplate(
                    new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(0, 10)),
                    new RenderLayerToken("SABİT_KATMAN"), // Kendi katmanında kalmalı
                    new RenderStyleToken("BYLAYER"))
            });

        var expanderLayer = new BlockExpander(new[] { layer0Def });
        var insertMimari = new BlockReference(
            "BLOCK_LAYER0",
            new WorldPoint2(0, 0),
            layer: new RenderLayerToken("MİMARİ_DUVAR"));

        var layerResult = expanderLayer.Expand(new[] { insertMimari });
        var inheritedEntity = layerResult.Entities.FirstOrDefault(e => e.Layer.Value == "MİMARİ_DUVAR");
        var explicitEntity = layerResult.Entities.FirstOrDefault(e => e.Layer.Value == "SABİT_KATMAN");

        bool layerInheritanceOk = inheritedEntity != null && explicitEntity != null;
        record("Blok Hiyerarşisi", "Layer 0 Mirası (INSERT Katmanını Otomatik Devralma)", layerInheritanceOk,
            $"Layer 0 Varlık -> '{inheritedEntity?.Layer.Value}' devraldı; Sabit Varlık -> '{explicitEntity?.Layer.Value}' korundu.");

        // 3. ByBlock Stil ve Renk Mirası
        // Blok içindeki varlık "BYBLOCK" stilindedir; INSERT referansı "TRUECOLOR|#FF00FF00" stilinde ise devralmalıdır.
        var byBlockDef = new BlockDefinition(
            "BLOCK_BYBLOCK",
            basePoint: new WorldPoint2(0, 0),
            entities: new[]
            {
                new BlockEntityTemplate(
                    new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(5, 5)),
                    new RenderLayerToken("0"),
                    new RenderStyleToken("BYBLOCK"))
            });

        var expanderStyle = new BlockExpander(new[] { byBlockDef });
        var insertStyle = new BlockReference(
            "BLOCK_BYBLOCK",
            new WorldPoint2(0, 0),
            style: new RenderStyleToken("TRUECOLOR|#FF00FF00"));

        var styleResult = expanderStyle.Expand(new[] { insertStyle });
        var resolvedStyleEntity = styleResult.Entities.FirstOrDefault();

        bool styleInheritanceOk = resolvedStyleEntity != null &&
                                  resolvedStyleEntity.Style.Value == "TRUECOLOR|#FF00FF00";
        record("Blok Hiyerarşisi", "ByBlock Stil Mirası (Üst Blok Rengini Devralma)", styleInheritanceOk,
            $"ByBlock Varlık -> '{resolvedStyleEntity?.Style.Value}' stilini başarıyla devraldı.");

        // 4. Blok Nitelikleri (ATTRIB) Ayrıştırma ve Yerleşimi
        var attribDef = new BlockDefinition(
            "KAPI_SEMBOLÜ",
            basePoint: new WorldPoint2(0, 0),
            entities: new[]
            {
                new BlockEntityTemplate(
                    new LinePrimitive(new WorldPoint2(0, 0), new WorldPoint2(80, 0)),
                    new RenderLayerToken("0"),
                    new RenderStyleToken("BYLAYER"))
            });

        var expanderAttrib = new BlockExpander(new[] { attribDef });
        var insertWithAttrib = new BlockReference(
            "KAPI_SEMBOLÜ",
            new WorldPoint2(100, 200),
            attributes: new[]
            {
                new BlockAttribute("KOD", "K-205_ÖZEL", new WorldPoint2(40, 10), Height: 2.5)
            });

        var attribResult = expanderAttrib.Expand(new[] { insertWithAttrib });
        var attribEntity = attribResult.Entities.FirstOrDefault(e => e.Id.Value.StartsWith("ATTRIB-KOD"));
        var pointPrim = attribEntity?.Geometry.OfType<PointPrimitive>().FirstOrDefault();

        bool attribOk = attribResult.TotalAttributesIncluded == 1 &&
                        pointPrim != null &&
                        Math.Abs(pointPrim.Position.X - 40.0) < 1e-9 &&
                        Math.Abs(pointPrim.Position.Y - 10.0) < 1e-9;
        record("Blok Hiyerarşisi", "Blok Nitelikleri (ATTRIB) Değer ve Konum Dönüşümü", attribOk,
            $"Nitelik Varlığı: '{attribEntity?.Id.Value}', Pozisyon: ({pointPrim?.Position.X:F1}, {pointPrim?.Position.Y:F1}), Dahil Edilen Nitelik: {attribResult.TotalAttributesIncluded}");
    }
}
