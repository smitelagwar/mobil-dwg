# Mobil DWG Görüntüleyici Düzeltme ve Kapanış Durumu

**Tarih:** 6 Eylül 2026  
**Plan:** `docs/GEMINI_SON_DENETIM_DUZELTME_PLANI.md`  
**Dal:** `codex/viewer-stability-v3`  
**Denetlenen / Başlangıç HEAD:** `6a006a5825c280b4464f7e87f35c20633b8315c2`  
**Korunan Kullanıcı Dosyaları:** `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/`, `docs/MOBIL_DWG_NIHAI_UYGULAMA_PLANI.md`, `docs/GEMINI_UYGULAMA_BASLATMA_PROMPTU.md`, `docs/GEMINI_DUZELTME_BASLATMA_PROMPTU.md`, `docs/GEMINI_SON_DENETIM_DUZELTME_PLANI.md`

---

## 1. Aşama Durum Çizelgesi (D01 – D12)

| Aşama | Başlık | Durum | Hedef / Kapsam |
|:---:|:---|:---:|:---|
| **D01** | Güvenilir regresyon zemini ve çalıştırılabilir Android testi | **TAMAMLANDI** | P01–P13 kırmızı regresyonları, Android Instrumentation APK ve yeni gate |
| **D02** | İlk kare, tek paint ve kaybolmayan çizim isteği | **TAMAMLANDI** | P01, P02, P03, P04; Surface generation, tek aktif paint, clock arming |
| **D03** | Atomik kamera, gerçek native input ve final kalite | BAŞLANIYOR | P05; Monoton CameraRevision, UP sonrası final kare |
| **D04** | Açma/iptal/kapatma ve geçici dosya sahipliği | BEKLEMEDE | P10, P11; SafeCadFileCache aktif kopya koruması, coordinator semaphore guard |
| **D05** | Güvenli cache, doğru kalite anahtarı ve sınırlı hazırlık | BEKLEMEDE | P06, P07, P08; LOD toleransı, hatch eviction, raster bitmap yaşam döngüsü |
| **D06** | Gerçek parser hattında koordinatlar, bloklar ve eğriler | BEKLEMEDE | P12, P13; İç içe blok dönüşüm zinciri, dikey elips sınırları |
| **D07** | Stil, görünürlük, çizim sırası ve muhafazakâr bounds | BEKLEMEDE | Gerçek DXF renk/katman/sıralama, muhafazakâr sınır kontrolleri |
| **D08** | Metin, dimension ve hatch'in gerçek çizim doğruluğu | BEKLEMEDE | P09; SKFont gerçek ölçümü, text bounds, hatch desen sürekliliği |
| **D09** | Layout, referanslar ve gerçek UI araçları | BEKLEMEDE | Paper space layouts, INSUNITS koordinat ölçümü, eğrisel snap |
| **D10** | Surface kaybı, arka plan ve düşük bellek | BEKLEMEDE | Context/surface restore, watchdog kontrolü, trim memory |
| **D11** | Üretim yolunda doğruluk, akıcılık ve uzun süre testi | BEKLEMEDE | Gerçek touch fidelity, telemetry, APK üzerinden performans kabulü |
| **D12** | Temiz checkout, gerçek sürüm kanıtı ve kapanış | BEKLEMEDE | CI iş akışı, kilitli bağımlılık doğrulaması, kesin sürüm manifesti |

---

## 2. Kusur Takip Çizelgesi (P01 – P13)

| ID | Öncelik | Açıklama | Test Yeri | D01 Durumu | Güncel Durum |
|:---:|:---:|:---|:---|:---:|:---:|
| **P01** | P0 | View surface generation (2) ile gate (1) uyumsuzluğu, ilk kare reddi | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P02** | P0 | İki ardışık `TryBeginPaint` eşzamanlı aktif ticket kabul ediyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P03** | P0 | `session.Zoom` gate'i scheduled yapıyor; host clock kurulamıyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P04** | P1 | UI zoom kamerayı değiştiriyor ancak `CameraRevision` artmıyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P05** | P1 | Aynı konumlu UP yalnız `InteractionEnded` üretiyor; final kare istenmiyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **KIRMIZI** (D03 hedefi) |
| **P06** | P1 | Aynı LOD bandındaki kaba geometri (hata=1.0) hassas istekte (<=0.25) dönüyor | `Rendering.Tests` (`TestP06`) | **KIRMIZI** (Yeniden üretildi) | D05 |
| **P07** | P1 | Hatch girdisi önbellek bütçesini aşıyor; hatch eviction mekanizması eksik | `Rendering.Tests` (`TestP07`) | **KIRMIZI** (Yeniden üretildi) | D05 |
| **P08** | P0 | Bütçeyi aşan raster bitmap `PutRaster` içinde erken dispose ediliyor | `Rendering.Tests` (`TestP08`) | **KIRMIZI** (Yeniden üretildi) | D05 |
| **P09** | P1 | `TextLayout.TotalWidth` gerçek SKFont ölçümünden küçük kalıyor | `Rendering.Tests` (`TestP09`) | **KIRMIZI** (Yeniden üretildi) | D08 |
| **P10** | P0 | Kopyalama sırasında orphan purge aktif henüz kaydedilmemiş dosyayı siliyor | `Integration.Tests` (`TestP10`) | **KIRMIZI** (Yeniden üretildi) | D04 |
| **P11** | P0 | Parse sırasında coordinator dispose edilince semaphore `ObjectDisposedException` veriyor | `Integration.Tests` (`TestP11`) | **KIRMIZI** (Yeniden üretildi) | D04 |
| **P12** | P1 | İç içe blok yerleşiminde ebeveyn dönüşümü miras alınmıyor (55,65 yerine 5,5) | `Integration.Tests` (`TestP12`) | **KIRMIZI** (Yeniden üretildi) | D06 |
| **P13** | P1 | Dikey elips sınır hesabında ana eksen yönelimi hesaba katılmıyor (10x20 yerine 20x10) | `Integration.Tests` (`TestP13`) | **KIRMIZI** (Yeniden üretildi) | D06 |

---

## 3. Aşama D01 Raporu

- **Aşama Adı:** D01 — Güvenilir regresyon zemini ve çalıştırılabilir Android testi
- **Durum:** TAMAMLANDI (Beklenen tüm 13 kırmızı regresyon ve çalıştırılabilir Android Instrumentation başarıyla kuruldu)
- **Eklenen / Değişen Dosyalar:**
  - `tests/MobilDwg.Rendering.Tests/CorrectionRegressionsP01ToP09.cs` (P01–P09 kırmızı regresyonları)
  - `tests/MobilDwg.Integration.Tests/CorrectionRegressionsP10ToP13.cs` (P10–P13 kırmızı regresyonları)
  - `tests/MobilDwg.Rendering.Tests/Program.cs` (`--regressions` parametresi entegrasyonu)
  - `tests/MobilDwg.Integration.Tests/Program.cs` (`--regressions` parametresi entegrasyonu)
  - `tests/MobilDwg.Android.Instrumentation/AndroidManifest.xml` (Ayrı instrumentation test manifesti)
  - `tests/MobilDwg.Android.Instrumentation/MobilDwgTestRunner.cs` (Gerçek Android Instrumentation Runner)
  - `tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj` (Test APK derleme yapılandırması)
  - `scripts/viewer-correction-gate.ps1` (Düzeltme kapı scripti)
  - `docs/VIEWER_DUZELTME_DURUMU.md` (Bu takip ve durum belgesi)

### Çalıştırılan Gerçek Komutlar ve Çıktılar

1. **Masaüstü Rendering Regresyonları (P01–P09):**
   ```powershell
   dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release -- --regressions
   ```
   - **Sonuç:** `Exit Code: 1`
   - **Çıktı:** 9/9 regresyon testi beklendiği gibi KIRMIZI (FAIL) olarak kusurları yakaladı.

2. **Masaüstü Entegrasyon Regresyonları (P10–P13):**
   ```powershell
   dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release -- --regressions
   ```
   - **Sonuç:** `Exit Code: 1`
   - **Çıktı:** 4/4 regresyon testi beklendiği gibi KIRMIZI (FAIL) olarak kusurları yakaladı.

3. **Android Instrumentation Test APK Derleme ve Çalıştırma:**
   ```powershell
   dotnet build tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj -c Release
   adb shell am instrument -w com.smitelagwar.mobildwg.test/com.smitelagwar.mobildwg.test.MobilDwgTestRunner
   ```
   - **Test APK:** `tests/MobilDwg.Android.Instrumentation/bin/Release/net10.0-android36.0/com.smitelagwar.mobildwg.test-Signed.apk`
   - **Çalışan Runner:** `com.smitelagwar.mobildwg.test.MobilDwgTestRunner`
   - **Cihaz Çıktı Dosyaları:**
     - `mobildwg_native_test_result.json` (NATIVE_P01..P05 beklenen kusurlar yakalandı, DXF ve örnek çizim doğrulandı)
     - `mobildwg_sample_first_frame.png`
     - `mobildwg_dxf_screen.png`
     - `mobildwg_touch_screen.png`

4. **Düzeltme Kapısı (Viewer Correction Gate):**
   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts/viewer-correction-gate.ps1 -AllowRegressions
   ```
   - **Sonuç:** `Exit Code: 0` (Bilinen regresyonlar tespit edildi, tüm kanıtlar `artifacts/viewer-correction/` altına kaydedildi).

### Kapanış Kararı:
D01 aşaması başarıyla tamamlanmıştır. Bilinen tüm 13 kusur güvenilir kırmızı testlere bağlanmış ve Android çalışma ortamında test edilebilirlik kanıtlanmıştır. Bir sonraki aşama: **D02 — İlk kare, tek paint ve kaybolmayan çizim isteği**.

---

## 4. Aşama D02 Raporu

- **Aşama Adı:** D02 — İlk kare, tek paint ve kaybolmayan çizim isteği
- **Durum:** TAMAMLANDI (P01, P02, P03 ve P04 tam olarak yeşile çevrildi)
- **Eklenen / Değişen Dosyalar:**
  - `src/MobilDwg.Rendering/Scheduling/FrameRequestGate.cs` (Tek aktif bilet ve yüzey nesli adaptasyonu)
  - `src/MobilDwg.Rendering/Viewer/CadViewerSession.cs` (Kamera revizyon artışı, FrameInvalidated yayımı, gate isteği temizliği)
  - `src/MobilDwg.App/Viewer/CadViewportView.cs` (Yüzey nesli senkronizasyonu, FrameInvalidated aboneliği, thread-safe çizim yerelleri)
  - `docs/VIEWER_DUZELTME_DURUMU.md` (Durum belgesi güncellemesi)

### Çözülen Kusurlar ve Uygulanan Değişiklikler:
1. **P01 (İlk Yüzey Nesli ve İlk Kare Kabulü):**
   - `CadViewportView` oluşturulduğunda artan yüzey nesli (2), `BindSession` sırasında `_session.FrameGate.InvalidateSurface(_surfaceGeneration)` çağrılarak kapıya senkronize edildi.
   - `FrameRequestGate.TryBeginPaint`, `surfaceGeneration > _currentSurfaceGeneration` durumunda nesli güncelleyerek geçerli ilk kare talebini kabul eder hale getirildi.
2. **P02 (Eşzamanlı Çizim Bileti Koruması):**
   - `FrameRequestGate.TryBeginPaint` içine aktif bir çizim sürerken (`_activeTicketId != 0` veya `State == FrameGateState.Painting`) ikinci bilet talebini kesin olarak `null` döndüren koruma eklendi.
3. **P03 (Host Çerçeve Saati Kurulumu):**
   - `CadViewerSession.Zoom`, `Pan`, `ZoomToFit`, `ResizeViewport`, `SetLayerVisibility` ve `SwitchLayout` metotlarındaki doğrudan `_frameGate.RequestFrame()` çağrıları kaldırıldı.
   - Bu mutasyonlar `FrameInvalidated(reason)` olayı tetikler; `CadViewportView` bu olayı yakalayarak `RequestFrame()` üzerinden kapıyı tek merkezden yönetir ve saat (clock) başarıyla kurulur.
4. **P04 (Zoom ve Mutasyonlarda CameraRevision Artışı):**
   - `CadViewerSession` içinde bağımsız `_cameraRevision` alanı tutularak `Zoom`, `Pan`, `ZoomToFit`, `ResizeViewport`, `SwitchLayout` ve `InteractionEngine.CameraChanged` tetiklendiğinde revizyon monoton olarak artırıldı.

### Test ve Doğrulama Kanıtları:
- **Masaüstü Regresyon Testleri:**
  - `P01`: `[PASS] FrameRequestGate First Surface Generation Admission`
  - `P02`: `[PASS] FrameRequestGate Concurrent Paint Ticket Guard`
  - `P03`: `[PASS] CadViewerSession Zoom Host Frame Clock Arming`
  - `P04`: `[PASS] CadViewerSession CameraRevision Increment on Zoom`
- **Android Instrumentation (Gerçek Cihaz / Emülatör Çalışması):**
  - `test_NATIVE_P01_FIRST_SURFACE_GENERATION`: `PASS`
  - `test_NATIVE_P02_CONCURRENT_PAINT_GUARD`: `PASS`
  - `test_NATIVE_P03_HOST_CLOCK_ARMING`: `PASS`
  - `test_NATIVE_P04_CAMERA_REVISION`: `PASS`
  - `test_NATIVE_SAMPLE_DRAWING_FIRST_FRAME`: `PASS` (522 çizim pikseli başarıyla boyandı)
  - `test_NATIVE_DXF_OPEN_RENDER`: `PASS` (56757 çizim pikseli başarıyla boyandı)

### Kapanış Kararı:
D02 aşaması başarıyla tamamlanmış ve P01, P02, P03, P04 doğrulanmıştır. Bir sonraki aşama: **D03 — Atomik kamera, gerçek native input ve final kalite**.
