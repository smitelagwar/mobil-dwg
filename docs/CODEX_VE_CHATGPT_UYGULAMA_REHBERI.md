# Mobil DWG/DXF — Kapsamlı Durum Özeti, Proje Denetimi (Audit) ve Codex/ChatGPT Uygulama Rehberi

**Rapor Tarihi:** 25 Ağustos 2026  
**Aktif Kapsam:** Android-only Offline 2D DWG/DXF Viewer (v1)  
**Hedef:** Bu doküman, projede bugüne kadar yapılanları özetler, mevcut plan ve kod mimarisindeki eksik/riskli noktaları ortaya koyar ve Codex / ChatGPT ile güvenle uygulanabilecek net, token-tasarruflu aksiyon paketleri sunar.

---

## BÖLÜM 1: Şu Ana Kadar Neler Yaptık? (Mevcut Durum Özeti)

Projede iki ana faz başarıyla tamamlanmış ve Android doğrulama fazı yürütülmektedir:

### 1. Tarihsel Temel Geliştirme (AŞAMA 00 – AŞAMA 09)
* **AŞAMA 00 - 01 (Toolchain):** .NET 10 (10.0.400), Android SDK API 36, OpenJDK 21, ADB 37 ile katı araç zinciri kilitlendi.
* **AŞAMA 02 (Bağımlılık & Lisans):** Central Package Management (`Directory.Packages.props`) ile strict exact versions (`[3.7.1]` ACadSharp, `[4.151.1]` SkiaSharp, `[10.0.100]` MAUI) uygulandı. GPL/AGPL/ticari SDK'lar kesin olarak yasaklandı.
* **AŞAMA 03 (Corpus & Fixture Sözleşmesi):** 0BSD lisanslı sentetik DXF kaynakları ve test sırasında üretilen AC1015 DWG dosyaları git blob hash doğrulamasına bağlandı.
* **AŞAMA 04 (Mimari Sınırlar):** 4 katmanlı temiz mimari (`MobilDwg.Core` -> `MobilDwg.Cad` / `MobilDwg.Rendering` -> `MobilDwg.App`) kuruldu ve mimari testlerle (`MobilDwg.Architecture.Tests`) ters bağımlılıklar engellendi.
* **AŞAMA 05 (ACadSharp Parser Spike):** ACadSharp 3.7.1 ile read-only parser adapter entegre edildi (`ADR 0001 GO`).
* **AŞAMA 06 (Safe-Open & Bellek Güvenliği):** `CadFileOpenCoordinator` ve `SafeCadFileCache` ile SAF/Stream üzerinden atomik kopyalama, streaming kota kontrolü ve cancellation mekanizması yazıldı.
* **AŞAMA 07 (ProCad İncelemesi):** ProCad kaynak kodu incelendi; `float` hassasiyet kaybı (`5,000,000 + 0.001` sorunu) nedeniyle doğrudan kullanımı reddedildi (`ADR 0002 NO-GO`).
* **AŞAMA 08 (iOS İzolasyonu):** iOS kodları arşivlendi (`DEFERRED_FUTURE_OPTION`), Android v1 odağı netleştirildi.
* **AŞAMA 09 (RenderScene & Kamera Altyapısı):** `double` hassasiyetli `WorldBounds2`, `RenderScene`, `Camera2D` (pan, pinch-zoom, fit-extents, OCS->WCS Arbitrary Axis Algorithm) ve tanılayıcı/snapshot motoru tamamlandı.

### 2. Android Geriye Dönük Yeniden Doğrulama (V01 – V09)
* **V01 (Altyapı - VALIDATED):** Windows self-hosted runner ve Android API 36 Emulator üzerinde MAUI altyapısı doğrulandı.
* **V02 (Bağımlılık & Native Sınır - VALIDATED):** Android native `.so` dosyaları (`libSkiaSharp.so`) ve NuGet lockfile'ı emulatörde doğrulandı.
* **V03 (Fixture Sözleşmesi - VALIDATED):** Git blob hash ve sentetik DWG üretimi CI üzerinde mühürlendi.
* **V04 (Gerçek Android Uygulama Kabuğu - VALIDATED):** `src/MobilDwg.App` projesi Android-only .NET MAUI executable'a (`com.smitelagwar.mobildwg`) dönüştürüldü; APK API 36 emülatörde cold launch, UI automator ve liveness testlerinden başarıyla geçti (PR #17 `main`e merge edildi).
* **V05 (Gerçek Uygulama İçi Parser - IN_PROGRESS):** ACadSharp parser'ının gerçek Android app içinde DWG/DXF okuması `v05-real-android-parser` branch'inde test edilmektedir.

---

## BÖLÜM 2: Doğru Yolda mıyız? (Denetim / Audit Bulguları)

> **SONUÇ:** **Evet, stratejik ve mimari olarak çok doğru bir yoldasınız.** Projede sıfır royalty, temiz lisans, sıfır ticari bağımlılık ve yüksek koordinat hassasiyeti (`double`) prensiplerine kusursuz uyulmuştur.

Ancak, sonraki aşamalarda (özellikle V06 ve AŞAMA 10/11) sorun yaşamamak için aşağıdaki **7 risk ve eksik noktaya** dikkat edilmelidir:

### 1. SkiaSharp Render Katmanı & Geometri Primitifleri (AŞAMA 10 Kritik Riski)
* **Bulgu:** `MobilDwg.Rendering.csproj` içinde henüz SkiaSharp referansı yoktur. `RenderSceneEntity` şu an sadece metadata ve Bounds taşımaktadır.
* **Öneri/Önlem:** AŞAMA 10'da geometrik primitifler (`Line2D`, `Polyline2D`, `Circle2D`, `Arc2D`, `Text2D`, `Hatch2D`) `RenderSceneEntity` altına eklenirken, Skia'nın `SKPoint` (float) yapısına dönüşüm yalnızca son render aşamasında (`Camera2D.WorldToScreen`) yapılmalıdır.

### 2. Sentetik Fixture vs Gerçek Dünya Mühendislik Çizimleri (Corpus Riski)
* **Bulgu:** Testler şu anda yalnızca birkaç entity içeren sentetik 0BSD dosyalarla yapılmaktadır.
* **Öneri/Önlem:** Türkiye'deki gerçek inşaat/mimarlık projelerinde (kolon aplikasyon, kalıp planı, donatı detayları) yer alan 50.000+ entity'li çizimlerde bellek (OOM) ve FPS performansını ölçmek için test matrisine orta ölçekli gerçek/anonimleştirilmiş DWG paftaları dahil edilmelidir.

### 3. SHX Font ve Türkçe Karakter Desteği
* **Bulgu:** `AcadSharpDocumentReader.cs` içinde `File.Exists(filename)` kontrolü Android ortamında çalışmaz (çünkü `.shx` fontları dosya sisteminde dağınık değildir).
* **Öneri/Önlem:** Autodesk'in telifli SHX fontları gömülemez. Ancak eksik fontlar için permissive lisanslı açık kaynak font eşleme tablosu (`FontSubstitutionTable`) kurulmalı ve `İ, ı, Ş, ş, Ğ, ğ, Ü, ü, Ö, ö, Ç, ç` karakterleri için ANSI/Windows-1254 (CP1254) ve UTF-8 fallback mekanizması açıkça tanımlanmalıdır.

### 4. Android SAF (Storage Access Framework) & Stream Streaming (V06 Riski)
* **Bulgu:** Android 14/15/16'da SAF üzerinden dosya seçildiğinde `stream.Length` bilinmeyebilir (`-1`).
* **Öneri/Önlem:** `SafeCadFileCache.cs` içindeki akış (streaming) sırasında bellek patlamasını önlemek için `totalBytesRead > MaxQuota` kontrolünün döngü içinde anında abort etmesi korunmalı ve teyit edilmelidir.

### 5. Multi-Thread Parsing & UI Donması (ANR) Önlemi
* **Bulgu:** Büyük DWG dosyalarını ayrıştırmak 2-10 saniye sürebilir.
* **Öneri/Önlem:** ACadSharp ayrıştırma işlemi mutlaka `Task.Run` ile arkaplan iş parçacığında yürütülmeli, UI'da iptal edilebilir `Progress<CadReadProgress>` spinner gösterilmelidir.

---

## BÖLÜM 3: Codex ve ChatGPT İçin Kopyala-Yapıştır Eylem Paketleri

Aşağıdaki prompt blokları, token tasarrufu sağlayacak şekilde doğrudan ChatGPT veya Codex'e verilmek üzere hazırlanmıştır.

---

### PAKET 1: V05 Kapanışı ve V06 (Android Safe-Open & SAF) Hazırlığı
*(Bu promptu V05 testi tamamlandığında ChatGPT/Codex'e verin)*

```markdown
GÖREV: Android V05 revalidation aşamasını tamamla ve V06 (FilePicker/SAF & Safe-Open) aşamasına geç.

TALİMATLAR:
1. `docs/evidence/android-validation/V05.md` dosyasını gerçek Android API 36 emülatör parse kanıtlarıyla oluştur/güncelle.
2. `gecmis.md`, `ANDROID_DOGRULAMA_PLANI.md`, `DEVAM.md` ve `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyalarındaki checkpoint'leri V05 VALIDATED olarak işaretle.
3. CURRENT_STAGE değerini V06 yap.
4. V06 için `src/MobilDwg.App/Opening/` altındaki `CadFileOpenCoordinator` ve `SafeCadFileCache` bileşenlerini Android FilePicker / Documents provider üzerinden gerçek bir DWG/DXF açma akışına bağlayacak emülatör gate betiğini hazırla.
5. Hiçbir dosyada GPL kütüphane veya CAD writer kodu ekleme.
```

---

### PAKET 2: AŞAMA 10 (P0 Geometri Primitifleri & Tessellation) Hazırlığı
*(Bu promptu V09 revalidation tamamlandıktan veya offline A10 çalışmasına başlarken verin)*

```markdown
GÖREV: AŞAMA 10 — P0 Geometri Modeli ve SkiaSharp Öncesi Tessellation Altyapısını Kur.

TALİMATLAR:
1. `src/MobilDwg.Rendering/Scene/` altına aşağıdaki 2D primitif yapılarını ekle:
   - `LineGeometry` (Start: WorldPoint2, End: WorldPoint2)
   - `PolylineGeometry` (Points: IReadOnlyList<WorldPoint2>, IsClosed: bool, Bulges: IReadOnlyList<double>?)
   - `CircleGeometry` (Center: WorldPoint2, Radius: double)
   - `ArcGeometry` (Center: WorldPoint2, Radius: double, StartAngle: double, EndAngle: double)
   - `TextGeometry` (InsertionPoint: WorldPoint2, Text: string, Height: double, Rotation: double)
2. Tüm geometrik koordinatları `double` olarak koru.
3. `RenderSceneEntity` kaydına bu geometrileri tutan `IGeometry2D` veya `SceneGeometryData` alanını ekle.
4. `Bulge` (yaylı polyline segmentleri) için arc tessellation fonksiyonunu `double` hassasiyetle yaz.
5. Unit testlerini `tests/MobilDwg.Rendering.Tests/Program.cs` içine ekle ve tüm testlerin PASS olduğunu doğrula.
```

---

### PAKET 3: Türkçe Karakter & SHX Font Eşleme (Substitution) Modülü
*(Bu promptu metin renderına geçerken Codex/ChatGPT'ye verin)*

```markdown
GÖREV: CAD Metinleri ve Türkçe Karakterler İçin Güvenli Font Eşleme (Font Substitution) Tablosu Geliştir.

TALİMATLAR:
1. `src/MobilDwg.Rendering/` veya `src/MobilDwg.Cad/` altında `FontSubstitutionResolver.cs` sınıfı oluştur.
2. Bilinen AutoCAD SHX fontlarını (`txt.shx`, `romans.shx`, `simplex.shx`, `isocp.shx`) sistemde yüklü standart permissive fontlara (Roboto, OpenSans veya sistem sans-serif) eşleyen statik sözlük kur.
3. Windows-1254 (Türkçe CAD çizimleri) ve UTF-8 metin kodlamalarını destekleyen güvenli string çözümleyici ekle.
4. Eksik font durumunda sessizce çökme yerine `CadCompatibilityIssue` (MissingFont) üret ve eşdeğer font ile metni çizilebilir tut.
```

---

## BÖLÜM 4: Hızlı Kontrol Listesi (Cheat-Sheet)

| Kontrol Noktası | Durum | Risk Seviyesi | Eylem |
|---|---|---|---|
| **Lisans & Bağımlılıklar** | Kusursuz (MIT/Apache) | DÜŞÜK | Koru, yeni NuGet eklerken strict exact syntax `[x.y.z]` kullan. |
| **Mimari Katman Sınırları** | Kusursuz (4 proje) | DÜŞÜK | Core'a UI/Cad bağımlılığı sokma. |
| **Koordinat Hassasiyeti** | Kusursuz (`double`) | ORTA | Skia'ya aktarırken float dönüşümünü sadece ekran pikseli aşamasında yap. |
| **SAF / Dosya Açma** | İyi | DÜŞÜK | Android 14+ testlerinde bilinmeyen stream uzunluğu durumunu kontrol et. |
| **Font & Türkçe Karakter** | Geliştirilecek | ORTA | Paket 3 promptunu uygula. |
| **Gerçek DWG Testleri** | Geliştirilecek | YÜKSEK | V09 sonrası gerçek 50k+ entity'li mimari paftalarla performans testi yap. |
