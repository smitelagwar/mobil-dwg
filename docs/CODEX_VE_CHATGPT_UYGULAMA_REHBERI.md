# Mobil DWG/DXF — Kapsamlı Durum Özeti, Proje Denetimi (Audit) ve Codex/ChatGPT Uygulama Rehberi

**Rapor Tarihi:** 25 Ağustos 2026  
**Aktif Kapsam:** Android-only Offline 2D DWG/DXF Viewer (v1)  
**Hedef:** Bu doküman, projede bugüne kadar yapılanları özetler, mevcut plan ve kod mimarisindeki eksik/riskli noktaları ortaya koyar ve Codex / ChatGPT ile güvenle uygulanabilecek net, token-tasarruflu aksiyon paketleri sunar.

> **Yetki sınırı:** Bu dosya öneri/inceleme belgesidir; yürütme checkpoint'i değildir. Gerçek `main`, açık PR/Actions, `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `ANDROID_DOGRULAMA_PLANI.md` ve ilgili evidence her zaman üstündür. Prompt paketleri körlemesine uygulanmaz; açık validation PR'ının sahip olduğu checkpoint/evidence başka sohbet veya Codex tarafından paralel biçimde yeniden yazılmaz.

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

Ancak, sonraki aşamalarda (özellikle V06 ve AŞAMA 10/11) sorun yaşamamak için aşağıdaki **5 risk ve eksik noktaya** dikkat edilmelidir:

### 1. SkiaSharp Render Katmanı & Geometri Primitifleri (AŞAMA 10 Kritik Riski)
* **Bulgu:** `MobilDwg.Rendering.csproj` içinde henüz SkiaSharp referansı yoktur. `RenderSceneEntity` şu an sadece metadata ve Bounds taşımaktadır.
* **Öneri/Önlem:** AŞAMA 10'da geometrik primitifler (`Line2D`, `Polyline2D`, `Circle2D`, `Arc2D`, `Text2D`, `Hatch2D`) `RenderSceneEntity` altına eklenirken, Skia'nın `SKPoint` (float) yapısına dönüşüm yalnızca son render aşamasında (`Camera2D.WorldToScreen`) yapılmalıdır.

### 2. Sentetik Fixture vs Gerçek Dünya Mühendislik Çizimleri (Corpus Riski)
* **Bulgu:** Testler şu anda yalnızca birkaç entity içeren sentetik 0BSD dosyalarla yapılmaktadır.
* **Öneri/Önlem:** 50.000+ entity bellek/FPS ölçümü AŞAMA 20/21 performans kapısında yapılmalıdır. Kullanıcıya ait veya yalnız “anonimleştirildiği” varsayılan özel pafta repoya/CI artifact'ine konmaz; yalnız açık yeniden dağıtım izni/provenance kanıtlı corpus ya da kontrollü sentetik üretim kullanılır.

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

### PAKET 1: V05 Kapanışı; V06'yı Yalnız Sonraki Tur İçin Hazır Bırakma
*(Bu promptu V05 testi tamamlandığında ChatGPT/Codex'e verin)*

```markdown
GÖREV: Android V05 revalidation aşamasını gerçek kanıtla kapat; V06'yı aynı turda başlatma.

TALİMATLAR:
1. `docs/evidence/android-validation/V05.md` dosyasını gerçek Android API 36 emülatör parse kanıtlarıyla oluştur/güncelle.
2. `gecmis.md`, `ANDROID_DOGRULAMA_PLANI.md`, `DEVAM.md` ve `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyalarındaki checkpoint'leri V05 VALIDATED olarak işaretle.
3. Yalnız bütün zorunlu actual/non-zero-step sonuçlar ve exact evidence doğrulandıysa sonraki cursor'ı `V06 — NOT_STARTED` yap.
4. Bu kapanış turunda V06 source/workflow/gate kodu yazma; sonraki kullanıcı turuna net `NEXT_ACTION` bırak.
5. Hiçbir dosyada GPL kütüphane veya CAD writer kodu ekleme; queued/zero-step sonucu PASS sayma.
```

---

### PAKET 2: AŞAMA 10 (P0 Geometri Primitifleri & Tessellation) Hazırlığı
*(V09 öncesi yalnız aşağıdaki dar offline kapsam; mevcut sözleşme/Skia entegrasyonu ancak V09 sonrasında)*

```markdown
GÖREV: AŞAMA 10 — canonical `BASLA_A10.md` sınırları içinde P0 geometri taslağını hazırla.

TALİMATLAR:
1. Önce `BASLA_A10.md` ve `docs/A10_WORKSTREAM.md` dosyalarını oku; yalnız ayrı `stage10-p0-geometry-draft` branch'inde çalış.
2. V09 kapanmadan yalnız yeni/internal, platform-neutral ve mevcut sözleşmelerden bağımsız saf geometri matematiği/testleri ekle. Aday yapılar:
   - `LineGeometry` (Start: WorldPoint2, End: WorldPoint2)
   - `PolylineGeometry` (Points: IReadOnlyList<WorldPoint2>, IsClosed: bool, Bulges: IReadOnlyList<double>?)
   - `CircleGeometry` (Center: WorldPoint2, Radius: double)
   - `ArcGeometry` (Center: WorldPoint2, Radius: double, StartAngle: double, EndAngle: double)
   - `TextGeometry` (InsertionPoint: WorldPoint2, Text: string, Height: double, Rotation: double)
3. Tüm geometrik koordinatları `double` olarak koru; `Bulge` tessellation saf matematik olarak test edilebilir.
4. V09 kapanmadan `RenderSceneEntity`, `IRenderScene`/`ICadRenderer`, snapshot/architecture, `.csproj`/Skia wiring veya fixture/image-golden sözleşmelerini değiştirme.
5. V09 sonrasında güncel validated `main` ile integration yap; ancak o turda mevcut scene sözleşmesine bağlama ve Skia ekran-piksel dönüşümünü değerlendir. Android gate olmadan merge/DONE yoktur.
```

---

### PAKET 3: Türkçe Karakter & SHX Font Eşleme (Substitution) Modülü
*(Bu promptu yalnız metin-render aşaması açıldığında verin; V05/V06 veya erken A10 kapsamında uygulamayın)*

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

---

## Codex değerlendirme kararı — 25 Ağustos 2026

- Dış `CODEX_ONERILER.md` içindeki `3aa365d`, `V05 CODED_PENDING_EMULATOR`, boş V05 evidence oluşturma ve ana checkpoint'i elle değiştirme önerileri artık güncel değildir. Açık PR `#18` kendi gerçek V05 evidence'ını ve V06-next checkpoint'ini oluşturmuştur; bu iş `main` üzerinde tekrarlanmaz.
- PR `#18` kapanmadan onun source/evidence/checkpoint dosyalarına paralel düzeltme yapılmaz. Sonuçlar queued iken `VALIDATED/DONE` türetilmez; PR sahibi sohbet actual sonuçları izler.
- Şu an uygulanacak yeni production kodu yoktur. SAF, font/encoding, background parsing ve büyük-corpus maddeleri ilgili V06+/AŞAMA 10+/performans aşamasında yeniden doğrulanacak önerilerdir.
- Doğru yol korunuyor: Android-only, VXX sıralı validation, ayrı ve dar A10 draft hattı, iOS future option, A11 kapısı kapalı.
