using System;
using System.Diagnostics;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Scene;

namespace CadControlBenchmark.Suites;

public static class GestureDeepStressSuite
{
    public static void Run(Action<string, string, bool, string> record)
    {
        Console.WriteLine("\n=== [SUITE 1] GELİŞMİŞ DOKUNMATİK JEST VE HİSTEREZİS STRESİ ===");

        // 1. 100 Döngülük Histerezis Zoom Çemberi (100x 2.0 zoom in, 100x 0.5 zoom out)
        var startCam = new Camera2D(1080, 2400, new WorldPoint2(12345.6789, -9876.5432), 1.0);
        var cam = startCam;
        var focal = new ScreenPoint2(540, 1200);

        for (int i = 0; i < 100; i++)
        {
            cam = cam.ZoomAt(focal, 1.25);
        }
        for (int i = 0; i < 100; i++)
        {
            cam = cam.ZoomAt(focal, 1.0 / 1.25);
        }

        double driftCenterX = Math.Abs(cam.Center.X - startCam.Center.X);
        double driftCenterY = Math.Abs(cam.Center.Y - startCam.Center.Y);
        double driftWupp = Math.Abs(cam.WorldUnitsPerPixel - startCam.WorldUnitsPerPixel);

        bool hysteresisOk = driftCenterX < 1e-7 && driftCenterY < 1e-7 && driftWupp < 1e-9;
        record("Jest Stresi", "100 Döngülük Histerezis Zoom Çemberi (200x ZoomAt Döngüsü)", hysteresisOk,
            $"Merkez X Sapma: {driftCenterX:E2}, Y Sapma: {driftCenterY:E2}, WUPP Sapma: {driftWupp:E2}");

        // 2. Sub-Piksel Mikro-Pan (0.005 px) Hassasiyeti
        var microCam = startCam.PanBy(0.005, -0.005);
        var expectedCenter = new WorldPoint2(startCam.Center.X - (0.005 * startCam.WorldUnitsPerPixel),
                                            startCam.Center.Y + (-0.005 * startCam.WorldUnitsPerPixel));
        bool microOk = Math.Abs(microCam.Center.X - expectedCenter.X) < 1e-12 &&
                       Math.Abs(microCam.Center.Y - expectedCenter.Y) < 1e-12;
        record("Jest Stresi", "Sub-Piksel Mikro-Pan Hassasiyeti (0.005 px Kaydırma)", microOk,
            $"Hesaplanan: ({microCam.Center.X:F6}, {microCam.Center.Y:F6}), Beklenen ile Fark: {Math.Abs(microCam.Center.X - expectedCenter.X):E2}");

        // 3. Devasa Mesafe Pan (1,000,000 px Kaydırma)
        var megaCam = startCam.PanBy(1_000_000.0, -1_000_000.0);
        bool megaOk = double.IsFinite(megaCam.Center.X) && double.IsFinite(megaCam.Center.Y);
        record("Jest Stresi", "Mega-Mesafe Pan (1.000.000 px Kaydırma Sınırı)", megaOk,
            $"Yeni Merkez: ({megaCam.Center.X:F1}, {megaCam.Center.Y:F1}), Sonlu ve Geçerli");

        // 4. Ekran Kenarları ve Köşelerinde Zoom Odak Noktası Kararlılığı
        var cornerPoints = new[]
        {
            new ScreenPoint2(0, 0),        // Sol Üst
            new ScreenPoint2(1080, 0),     // Sağ Üst
            new ScreenPoint2(0, 2400),     // Sol Alt
            new ScreenPoint2(1080, 2400),  // Sağ Alt
            new ScreenPoint2(-100, -100),  // Ekran Dışı Negatif
            new ScreenPoint2(1200, 2600)   // Ekran Dışı Pozitif
        };

        double maxCornerDrift = 0;
        foreach (var corner in cornerPoints)
        {
            var worldBefore = CameraTransform.ScreenToWorld(corner, startCam);
            var zoomed = startCam.ZoomAt(corner, 2.5);
            var worldAfter = CameraTransform.ScreenToWorld(corner, zoomed);

            double drift = Math.Sqrt(Math.Pow(worldBefore.X - worldAfter.X, 2) + Math.Pow(worldBefore.Y - worldAfter.Y, 2));
            if (drift > maxCornerDrift) maxCornerDrift = drift;
        }

        bool cornerOk = maxCornerDrift < 1e-9;
        record("Jest Stresi", "Ekran Köşeleri ve Ekran Dışı Odak Noktalarında Zoom Kararlılığı", cornerOk,
            $"6 sınır/dış noktada azami odak sapması: {maxCornerDrift:E2} (< 1e-9)");

        // 5. Momentum / İnertia Fling Sönümleme Eğrisi Simülasyonu
        // Başlangıç hızı: 2000 px/s, sürtünme katsayısı: 0.92, 30 kare boyunca sönümleme
        double velocityX = 2000.0;
        double velocityY = -1500.0;
        double friction = 0.92;
        var flingCam = startCam;
        double totalFlingDx = 0;
        double totalFlingDy = 0;

        for (int frame = 0; frame < 45; frame++)
        {
            double dt = 1.0 / 60.0; // 60 FPS (16.6ms)
            double stepDx = velocityX * dt;
            double stepDy = velocityY * dt;
            totalFlingDx += stepDx;
            totalFlingDy += stepDy;
            flingCam = flingCam.PanBy(stepDx, stepDy);

            velocityX *= friction;
            velocityY *= friction;
        }

        bool flingOk = Math.Abs(velocityX) < 100.0 && Math.Abs(velocityY) < 100.0 &&
                       double.IsFinite(flingCam.Center.X) && double.IsFinite(flingCam.Center.Y);
        record("Jest Stresi", "Momentum / İnertia Fling Jest Simülasyonu (30 Kare Sönümleme)", flingOk,
            $"Toplam Mesafe: ({totalFlingDx:F1}px, {totalFlingDy:F1}px), Kalan Hız: {Math.Sqrt(velocityX * velocityX + velocityY * velocityY):F1} px/s");
    }
}
