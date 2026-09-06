using System;
using System.Collections.Generic;
using System.Linq;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace CadControlBenchmark.Suites;

public static class GeometryDeepFidelitySuite
{
    public static void Run(Action<string, string, bool, string> record)
    {
        Console.WriteLine("\n=== [SUITE 2] İLERİ CAD GEOMETRİSİ VE ÖZEL VARLIKLAR ===");

        var tessOptions = new GeometryTessellationOptions(0.01, minSegments: 4, maxSegments: 2048, splineSegmentsPerSpan: 12);

        // 1. Derece-3 (Cubic) NURBS B-Spline Çok Noktalı Eğri Örneklemesi
        // Derece 3, 5 Kontrol Noktası -> 5 + 3 + 1 = 9 Düğüm (Knot)
        var spline = new SplinePrimitive(
            degree: 3,
            controlPoints: new[]
            {
                new WorldPoint2(0, 0),
                new WorldPoint2(10, 20),
                new WorldPoint2(30, 25),
                new WorldPoint2(50, 10),
                new WorldPoint2(60, 0)
            },
            knots: new double[] { 0, 0, 0, 0, 0.5, 1, 1, 1, 1 });

        var pStart = spline.Evaluate(0.0);
        var pMid = spline.Evaluate(0.5);
        var pEnd = spline.Evaluate(1.0);

        var splinePath = GeometryTessellator.Tessellate(spline, tessOptions);

        bool splineOk = Math.Abs(pStart.X - 0.0) < 1e-9 && Math.Abs(pStart.Y - 0.0) < 1e-9 &&
                        Math.Abs(pEnd.X - 60.0) < 1e-9 && Math.Abs(pEnd.Y - 0.0) < 1e-9 &&
                        splinePath.Points.Count >= 20;
        record("İleri Geometri", "Derece-3 (Cubic) NURBS B-Spline Örnekleme ve Eğri Doğruluğu", splineOk,
            $"Başlangıç: ({pStart.X:F1},{pStart.Y:F1}), Orta (t=0.5): ({pMid.X:F1},{pMid.Y:F1}), Bitiş: ({pEnd.X:F1},{pEnd.Y:F1}), Örnek Sayısı: {splinePath.Points.Count}");

        // 2. Çift Yönlü Yay Dilimi (S-Curve Bulge) Büküm Noktaları
        // İlk segment +0.5 bulge (saat yönünün tersi yay), ikinci segment -0.5 bulge (saat yönü yay)
        var sCurvePoly = new PolylinePrimitive(new[]
        {
            new PolylineVertex(new WorldPoint2(0, 0), 0.5),   // Yay 1
            new PolylineVertex(new WorldPoint2(50, 0), -0.5), // Yay 2 (Zıt yönde)
            new PolylineVertex(new WorldPoint2(100, 0), 0.0)  // Düz son
        }, closed: false);

        var sCurvePath = GeometryTessellator.Tessellate(sCurvePoly, tessOptions);
        double minY = sCurvePath.Points.Min(p => p.Y);
        double maxY = sCurvePath.Points.Max(p => p.Y);

        bool sCurveOk = maxY > 5.0 && minY < -5.0 && sCurvePath.Points.Count >= 30;
        record("İleri Geometri", "Çift Yönlü Yay Dilimi (S-Curve Bulge Inflections)", sCurveOk,
            $"Yay Salınım Sınırları: Y_min={minY:F2}, Y_max={maxY:F2}, Toplam Segment Noktası: {sCurvePath.Points.Count}");

        // 3. Eğik OCS (Object Coordinate System) Arbitrary Axis Algorithm
        // Normal vektörü N = (1, 2, 3) olan rastgele eğik eksen
        var obliqueOcs = new OcsCoordinateSystem(new Vector3D(1.0, 2.0, 3.0));
        var ocsPoint = new WorldPoint3(123.456, -789.012, 42.0);

        var wcsPoint = obliqueOcs.OcsToWcs(ocsPoint);
        var restoredOcs = obliqueOcs.WcsToOcs(wcsPoint);

        double ocsError = Math.Sqrt(Math.Pow(restoredOcs.X - ocsPoint.X, 2) +
                                    Math.Pow(restoredOcs.Y - ocsPoint.Y, 2) +
                                    Math.Pow(restoredOcs.Z - ocsPoint.Z, 2));
        bool ocsOk = ocsError < 1e-9;
        record("İleri Geometri", "Eğik OCS Normal Vektörü ve Arbitrary Axis Çift Yönlü Dönüşümü", ocsOk,
            $"Normal: ({obliqueOcs.Normal.X:F2},{obliqueOcs.Normal.Y:F2},{obliqueOcs.Normal.Z:F2}), 3D Geri Dönüşüm Hatası: {ocsError:E2}");

        // 4. SOLID / 3DFace 4-Noktalı Düzlemsel Poligon
        var solidPoly = new PolygonPrimitive(new[]
        {
            new WorldPoint2(0, 0),
            new WorldPoint2(100, 0),
            new WorldPoint2(100, 80),
            new WorldPoint2(0, 80)
        });
        var solidPath = GeometryTessellator.Tessellate(solidPoly, tessOptions);

        bool solidOk = solidPath.Closed && solidPath.Filled && solidPath.Points.Count == 4;
        record("İleri Geometri", "SOLID / 3DFACE 4-Noktalı Düzlemsel Dolgulu Poligon", solidOk,
            $"Kapalı: {solidPath.Closed}, Dolgulu: {solidPath.Filled}, Köşe Noktası: {solidPath.Points.Count}");

        // 5. Elips Yay Primitifi (Elliptical Arc) Açı Dilimi
        var ellipseArc = new EllipsePrimitive(
            center: new WorldPoint2(200, 200),
            majorRadius: 80.0,
            minorRadius: 40.0,
            rotationRadians: Math.PI / 6.0, // 30 derece rotasyon
            startParameter: 0.0,
            sweepParameter: Math.PI); // Yarım elips

        var evalStart = ellipseArc.Evaluate(0.0);
        var evalMid = ellipseArc.Evaluate(Math.PI / 2.0);
        var evalEnd = ellipseArc.Evaluate(Math.PI);

        bool ellipseOk = ellipseArc.Bounds.Width > 50 && ellipseArc.Bounds.Height > 30 &&
                         double.IsFinite(evalStart.X) && double.IsFinite(evalMid.X) && double.IsFinite(evalEnd.X);
        record("İleri Geometri", "Döndürülmüş Elips Yayı (Elliptical Arc, Major/Minor Oranı: 2.0)", ellipseOk,
            $"Merkez: (200,200), Alan: {ellipseArc.Bounds.Width:F1}x{ellipseArc.Bounds.Height:F1}, Start=({evalStart.X:F1},{evalStart.Y:F1}), Mid=({evalMid.X:F1},{evalMid.Y:F1})");
    }
}
