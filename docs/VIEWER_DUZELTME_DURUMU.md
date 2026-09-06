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
| **D03** | Atomik kamera, gerçek native input ve final kalite | **TAMAMLANDI** | P05; Monoton CameraRevision, atomik mutasyonlar, Android ViewConfiguration slop, UP sonrası final kare |
| **D04** | Açma/iptal/kapatma ve geçici dosya sahipliği | **TAMAMLANDI** | P10, P11; SafeCadFileCache aktif kopya koruması, coordinator semaphore guard |
| **D05** | Güvenli cache, doğru kalite anahtarı ve sınırlı hazırlık | **TAMAMLANDI** | P06, P07, P08; LOD toleransı, hatch eviction, raster bitmap yaşam döngüsü |
| **D06** | Gerçek parser hattında koordinatlar, bloklar ve eğriler | **TAMAMLANDI** | P12, P13; İç içe blok dönüşüm zinciri, dikey elips sınırları |
| **D07** | Stil, görünürlük, çizim sırası ve muhafazakâr bounds | **TAMAMLANDI** | Gerçek DXF renk/katman/sıralama, muhafazakâr sınır kontrolleri |
| **D08** | Metin, dimension ve hatch'in gerçek çizim doğruluğu | **TAMAMLANDI** | P09; SKFont gerçek ölçümü, text bounds, hatch desen sürekliliği |
| **D09** | Layout, referanslar ve gerçek UI araçları | **TAMAMLANDI** | Paper space layouts, INSUNITS koordinat ölçümü, eğrisel snap |
| **D10** | Surface kaybı, arka plan ve düşük bellek | **TAMAMLANDI** | Context/surface restore, watchdog kontrolü, trim memory |
| **D11** | Üretim yolunda doğruluk, akıcılık ve uzun süre testi | **TAMAMLANDI** (İşlevsel/Emülatör) | Gerçek touch fidelity, telemetry, APK üzerinden performans kabulü |
| **D12** | Temiz checkout, gerçek sürüm kanıtı ve kapanış | **TAMAMLANDI** | CI iş akışı, kilitli bağımlılık doğrulaması, kesin sürüm manifesti |

---

## 2. Kusur Takip Çizelgesi (P01 – P13)

| ID | Öncelik | Açıklama | Test Yeri | D01 Durumu | Güncel Durum |
|:---:|:---:|:---|:---|:---:|:---:|
| **P01** | P0 | View surface generation (2) ile gate (1) uyumsuzluğu, ilk kare reddi | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P02** | P0 | İki ardışık `TryBeginPaint` eşzamanlı aktif ticket kabul ediyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P03** | P0 | `session.Zoom` gate'i scheduled yapıyor; host clock kurulamıyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P04** | P1 | UI zoom kamerayı değiştiriyor ancak `CameraRevision` artmıyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D02) |
| **P05** | P1 | Aynı konumlu UP yalnız `InteractionEnded` üretiyor; final kare istenmiyor | `Rendering.Tests` & `NativeSmokeRunner` | **KIRMIZI** | **YEŞİL** (D03) |
| **P06** | P1 | Aynı LOD bandındaki kaba geometri (hata=1.0) hassas istekte (<=0.25) dönüyor | `Rendering.Tests` (`TestP06`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D05) |
| **P07** | P1 | Hatch girdisi önbellek bütçesini aşıyor; hatch eviction mekanizması eksik | `Rendering.Tests` (`TestP07`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D05) |
| **P08** | P0 | Bütçeyi aşan raster bitmap `PutRaster` içinde erken dispose ediliyor | `Rendering.Tests` (`TestP08`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D05) |
| **P09** | P1 | `TextLayout.TotalWidth` gerçek SKFont ölçümünden küçük kalıyor | `Rendering.Tests` (`TestP09`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D08) |
| **P10** | P0 | Kopyalama sırasında orphan purge aktif henüz kaydedilmemiş dosyayı siliyor | `Integration.Tests` (`TestP10`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D04) |
| **P11** | P0 | Parse sırasında coordinator dispose edilince semaphore `ObjectDisposedException` veriyor | `Integration.Tests` (`TestP11`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D04) |
| **P12** | P1 | İç içe blok yerleşiminde ebeveyn dönüşümü miras alınmıyor (55,65 yerine 5,5) | `Integration.Tests` (`TestP12`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D06) |
| **P13** | P1 | Dikey elips sınır hesabında ana eksen yönelimi hesaba katılmıyor (10x20 yerine 20x10) | `Integration.Tests` (`TestP13`) | **KIRMIZI** (Yeniden üretildi) | **YEŞİL** (D06) |

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

---

## 5. Aşama D03 Raporu

- **Aşama Adı:** D03 — Atomik kamera, gerçek native input ve final kalite
- **Durum:** TAMAMLANDI (P05 yeşile çevrildi, atomik kamera ve native input sözleşmeleri Android emülatöründe ve masaüstünde doğrulandı)
- **Eklenen / Değişen Dosyalar:**
  - `src/MobilDwg.Rendering/Interaction/ViewportInputContracts.cs` (`TapTimeoutMs`, `LongPressTimeoutMs` eklendi, `SurfaceGeneration` `long` tipine hizalandı)
  - `src/MobilDwg.Rendering/Camera/ViewportController.cs` (Kamera mutasyonlarında gereksiz revizyon artışını önleyen eşitlik kontrolleri ve manipülasyon identity guard)
  - `src/MobilDwg.Rendering/Interaction/ViewportInteractionEngine.cs` (Thread synchronization lock desteği, yüzey nesli uyuşmazlığında güvenli sıfırlama, uç koordinat sanity kontrolü, pointer ID kümesi değişiminde yumuşak baseline kurulumu, uzun basış tap/double-tap izolasyonu, durağan hareket/clamped zoom revizyon filtresi, UP anında kamera değişmese dahi `CameraChanged` ve `InteractionEnded` yayımı ile final kare talebi)
  - `src/MobilDwg.Rendering/Viewer/CadViewerSession.cs` (Kamera mutasyonlarının `_stateLock` altında atomikleştirilmesi, `NotifyFrameInvalidated("FinalQuality")` tetiklenmesi, `SceneBounds` ve `SetCamera` dışa aktarımı)
  - `src/MobilDwg.App/Viewer/Platforms/Android/AndroidViewportInputAdapter.cs` (Etkin Skia native view'e bağlanma, Android `ViewConfiguration` slop/timeout parametrelerinin okunması, view detach ve focus loss olaylarında temiz iptal/ayrılma, native koordinatların density ile mükerrer çarpımının engellenmesi)
  - `src/MobilDwg.App/Viewer/CadViewportView.cs` (Backend değişimlerinde ve session unbind anında adapter'ın temizlenmesi, handler değişiminde adapter'ın yeniden bağlanması)
  - `src/MobilDwg.App/MainPage.cs` (`_session.SetCamera` ve `_session.SceneBounds` atomik API kullanımı, sample card otomasyon ID'si ve intent desteği)
  - `tests/MobilDwg.Rendering.Tests/ViewportInteractionTests.cs` (6 yeni D03 unit testi: uzun basış, durağan hareket, zoom sınırları, nesil uyuşmazlığı, geçersiz koordinat, atomik mutasyonlar)
  - `tests/MobilDwg.Android.Instrumentation/MobilDwgTestRunner.cs` (Kontrollü kaydırma, fling önleme ve örnek çizim test kararlılığı)

### Test ve Doğrulama Kanıtları:
- **Masaüstü Testleri:**
  - `P05`: `[PASS] ViewportInteractionEngine Action.Up Final Frame Trigger`
  - `ViewportInteractionTests`: Tüm 6 D03 testi ve tüm mevcut testler yeşil (0 hata).
- **Android Instrumentation (emulator-5554 / API 36):**
  - `NATIVE_P01_FIRST_SURFACE_GENERATION`: `PASS`
  - `NATIVE_P02_CONCURRENT_PAINT_GUARD`: `PASS`
  - `NATIVE_P03_HOST_CLOCK_ARMING`: `PASS`
  - `NATIVE_P04_CAMERA_REVISION`: `PASS`
  - `NATIVE_P05_FINAL_FRAME_NOTIFICATION`: `PASS`
  - `NATIVE_SAMPLE_DRAWING_FIRST_FRAME`: `PASS` (2526 çizim pikseli başarıyla boyandı)
  - `NATIVE_DXF_OPEN_RENDER`: `PASS` (56757 çizim pikseli başarıyla boyandı)
  - `NATIVE_HOST_TOUCH_CLOCK_ARM`: `PASS` (Gerçek native drag ve UI zoom düğmesi tetiklendi)
  - `NATIVE_ENGINE_SMOKE_TESTS`: `PASS`
  - `INSTRUMENTATION_CODE: -1` (0 hata, 9/9 PASS)
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`

### Kapanış Kararı:
D03 aşaması tüm kabul kriterleriyle başarıyla tamamlanmıştır. Bir sonraki aşama: **D04 — Açma/iptal/kapatma ve geçici dosya sahipliği**.

---

## 6. Aşama D04 Raporu

- **Aşama Adı:** D04 — Açma/iptal/kapatma ve geçici dosya sahipliği
- **Durum:** TAMAMLANDI (P10 ve P11 yeşile çevrildi, tüm D04 yaşam döngüsü ve dosya sahipliği sözleşmeleri doğrulandı)
- **Eklenen / Değişen Dosyalar:**
  - `src/MobilDwg.App/Opening/SafeCadFileCache.cs` (Kopyalama başlamadan önce `.part` geçici dosyasının ve rename öncesinde nihai dosyanın `_activeFiles` kaydına eklenmesi; orphan purge'ün aktif kopyalanan dosyaları silmesini engelleyen atomik geçiş; hata yollarında tam unregister ve delete garantisi)
  - `src/MobilDwg.App/Opening/CadFileOpenCoordinator.cs` (`_activeWorkerCount` takibi; aktif worker çalışırken semaphore'un dispose edilmesini engelleyen guard; worker tamamlandığında `_disposed` kontrolüyle güvenli semaphore imhası; `Task.Run` içinde parser `session` nesnesinin extract/build hata yollarında sızmasını önleyen kesin `try ... finally` sahipliği; `ResetCurrentSessionAsync` çağrısında neslin atomik artırılması ve bekleyen iptallerin tetiklenmesi)
  - `src/MobilDwg.App/MainPage.cs` (Açma hatası veya iptal durumunda hâlen ekranda olan mevcut çizimi silen gereksiz `ResetCurrentSessionAsync` çağrılarının kaldırılması; UI thread'deki senkron extraction/build fallback'inin kaldırılması; `DisplayCadSceneAsync` metoduna coordinator'dan gelen orijinal `result.Metadata` nesnesinin doğrudan aktarılması; `CloseActiveDrawing` sırasında coordinator sahipliğinin temizlenmesi)
  - `tests/MobilDwg.Integration.Tests/CadOpenCoordinatorD04Tests.cs` (D04 için 4 yeni entegrasyon testi: Hızlı A→B→C açma sırasında yalnız C'nin kabul edilmesi, parse sürerken kapatma/sıfırlama, sıfır-bayt/bozuk dosya akışı, drain sonrası sıfır aktif lease doğrulaması)
  - `tests/MobilDwg.Integration.Tests/Program.cs` (Yeni D04 testlerinin entegrasyon koşusuna bağlanması)

### Test ve Doğrulama Kanıtları:
- **Masaüstü Entegrasyon Testleri:**
  - `P10`: `[PASS] SafeCadFileCache Purge Active Copy Protection`
  - `P11`: `[PASS] CadFileOpenCoordinator Dispose Semaphore Guard`
  - `TestRapidSequenceA_B_C_OnlyCCommitted`: `[PASS]`
  - `TestCloseDuringActiveParse`: `[PASS]`
  - `TestCorruptAndZeroByteStreams`: `[PASS]`
  - `TestDrainAfterDisposeLeavesZeroActiveLeases`: `[PASS]`
  - Tüm standart entegrasyon testleri: `STAGE01_INTEGRATION_TESTS_PASS`
- **Android Instrumentation (emulator-5554 / API 36):**
  - Tüm 9 native Android testi eksiksiz `PASS` (failed=0, code -1).
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`

### Kapanış Kararı:
D04 aşaması başarıyla tamamlanmış, P10 ve P11 çözülmüş ve tüm dosya sahipliği kabul kriterleri yerine getirilmiştir. Bir sonraki aşama: **D05 — Güvenli cache, doğru kalite anahtarı ve sınırlı hazırlık**.

---

## 7. Aşama D05 Raporu

- **Aşama Adı:** D05 — Güvenli cache, doğru kalite anahtarı ve sınırlı hazırlık
- **Durum:** TAMAMLANDI (P06, P07 ve P08 yeşile çevrildi; LOD hata toleransı, hatch eviction, raster kabul ve çizim döngüsü korumaları doğrulandı)
- **Eklenen / Değişen Dosyalar:**
  - `src/MobilDwg.Rendering/Skia/PreparedGeometryCache.cs` (Önbellekten çekilen geometrinin `MaxChordError <= requiredChordError` koşulunu sağlaması garantilendi; bütçeyi aşan tekil girdilerin `Put` ve `PutHatchCoverage` öncesinde reddedilmesi sağlandı; LRU tahliye algoritması hem geometri listelerini hem de tarama `_hatchEntries` girdilerini kapsayacak şekilde birleşik `LastAccessSequence` sırasına alındı)
  - `src/MobilDwg.Rendering/Skia/RenderResourceCache.cs` (`PutRaster` metodu `bool` kabul sonucu dönecek şekilde güncellendi; bütçeyi aşan raster görsellerinin kabul edilmeyerek çağıranın nesnesinin erken dispose edilmesi engellendi; yeni görsel eklenmeden önce eski girdilerin tahliye edilmesi sağlandı)
  - `src/MobilDwg.Rendering/Skia/SkiaScenePainter.cs` (`DrawViewportPrimitive` metodu `primitiveKey` parametresi alarak iç anahtarları `vp:{innerIdx}` yerine `{primitiveKey}:inner:{innerIdx}` biçiminde kapsamlandırdı ve çoklu viewport çarpışması engellendi; `PutRaster` kabul sonucu `isCached` durumuna bağlanarak çizim sırasında bitmap'in canlı kalması sağlandı)
  - `docs/VIEWER_DUZELTME_DURUMU.md` (Durum belgesi güncellemesi)

### Çözülen Kusurlar ve Uygulanan Değişiklikler:
1. **P06 (LOD Kalite Toleransı ve Chord Error Koruması):**
   - `PreparedGeometryCache.TryGet`, salt `entry.LodBand == lodBand` eşitliğini yeterli görmeyip `entry.MaxChordError <= requiredChordError` koşulunu zorunlu kıldı. Kaba LOD girdisinin ince kalite beklentisinde sahte önbellek isabeti (cache hit) vermesi engellendi.
2. **P07 (Tarama Önbellek Bütçesi ve Eviction):**
   - `PreparedGeometryCache.PutHatchCoverage`, bütçeden büyük tekil girdileri reddeder; `EvictToBudgetUnderLock`, hem `_entries` hem de `_hatchEntries` koleksiyonlarını `LastAccessSequence` sırasına dizerek LRU tahliyesini her iki tür için uyguladı ve `CurrentSizeBytes <= MaxSizeBytes` kuralını sağladı.
3. **P08 (Raster Yaşam Döngüsü ve Bütçe Kabulü):**
   - `RenderResourceCache.PutRaster`, bütçeyi aşan bitmap nesnelerini doğrudan dispose etmeyip kabulü reddeder (`false`). Çağıran `SkiaScenePainter`, çizimi gerçekleştirdikten sonra sahipliği (`!isCached`) yöneterek `finally` bloğunda dispose eder.
4. **K10 Viewport Önbellek Çakışması:**
   - `SkiaScenePainter.DrawViewportPrimitive` içindeki `vp:{innerIdx}` anahtarı, ebeveyn viewport anahtarıyla birleştirilerek birden fazla görünüm penceresinin önbelleklerinin birbirini ezmesi önlendi.

### Test ve Doğrulama Kanıtları:
- **Masaüstü Regresyon Testleri:**
  - `P06`: `[PASS] PreparedGeometryCache Coarse LOD Rejection`
  - `P07`: `[PASS] PreparedGeometryCache Hatch Eviction Under Memory Budget`
  - `P08`: `[PASS] RenderResourceCache Raster Bitmap Lifetime Protection`
  - Tüm hazırlık ve geometri önbellek testleri: `STAGE07_PREPARED_GEOMETRY_CACHE_TESTS_PASS`
- **Android Instrumentation (emulator-5554 / API 36):**
  - Tüm 9 native Android testi eksiksiz `PASS` (failed=0, code -1).
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`
- **Düzeltme Kapısı (Viewer Correction Gate):**
  - `RunId`: `D05-validation`
  - `Exit Code`: 0
  - P01–P08, P10, P11 yeşil. Kalan bilinen regresyonlar yalnız D06 (P12, P13) ve D08 (P09).

### Kapanış Kararı:
D05 aşaması tüm kabul kriterleriyle başarıyla tamamlanmıştır. Bir sonraki aşama: **D06 — Gerçek parser hattında koordinatlar, bloklar ve eğriler**.

---

## 8. Aşama D06 Raporu

- **Aşama Adı:** D06 — Gerçek parser hattında koordinatlar, bloklar ve eğriler
- **Durum:** TAMAMLANDI (P12 ve P13 yeşile çevrildi; iç içe blok afin zinciri, dikey elips yönelimi, SOLID dolgu poligon dönüşümü ve MINSERT kota korumaları doğrulandı)
- **Eklenen / Değişen Dosyalar:**
  - `src/MobilDwg.Core/Coordinates/CadAffine2D.cs` (2D Afin dönüşüm matrisi: `Identity`, `Translation`, `Scale`, `Rotation`, `Multiply`, `Transform`, `TransformVector`, `ScaleX`, `ScaleY`, `RotationAngle`, `IsMirrored` metotları ve analitik doğruluğu)
  - `src/MobilDwg.Cad/AcadSharp/AcadSharpEntityExtractor.cs` (`ExpandBlockInsert` metoduna `CadAffine2D parentTransform` parametresi eklendi; ebeveyn ve yerel afin matrisler M_parent * M_local olarak birleştirildi; iç içe çağrılarda ve `TransformAndExtractEntity` adımında bu birleşik afin matris kullanıldı; MINSERT için >100,000 örnek kota koruması ve bütçe aşımında döngü kırma eklendi; `Ellipse` WCS koordinatlarından okunup ana eksen yönelimi atan2(my, mx) açısıyla doğru çıkarıldı)
  - `src/MobilDwg.Rendering/Scene/CadExtractedSceneBuilder.cs` (`CadExtractedEntityType.Solid` için önceden çizgisel döngü olan `PolylinePrimitive` yerine içi dolu 4 köşeli `PolygonPrimitive` üretilmesi sağlandı)
  - `src/MobilDwg.Cad/AcadSharp/AcadSharpDocumentReader.cs` (`AcadSharpDocumentHandle` yapıcı metodu test erişimi için public yapıldı)
  - `tests/MobilDwg.Integration.Tests/CadGeometryD06Tests.cs` (D06 için 4 kapsamlı analitik geometri testi: 3 seviyeli iç içe blok dönüşümü, MINSERT ızgara açılımı, dikey ve eğik elips sınır denklemleri, SOLID poligon dolgu doğrulaması)
  - `tests/MobilDwg.Integration.Tests/Program.cs` (`CadGeometryD06Tests` koşusunun entegrasyon hattına bağlanması)
  - `docs/VIEWER_DUZELTME_DURUMU.md` (Durum belgesi güncellemesi)

### Çözülen Kusurlar ve Uygulanan Değişiklikler:
1. **P12 (İç İçe Blok Dönüşümlerinin Zincirlenmesi):**
   - Eski kodda `ExpandBlockInsert`, iç içe blokları açarken üst bloğun dönüşümünü (`parentTransform`) aktarmıyor, iç bloğu sadece kendi yerel koordinatlarıyla açıyordu (`(55, 65)` yerine `(5, 5)`).
   - `CadAffine2D` matris çarpımı ile M_parent * M_local birleşik afin matrisi oluşturuldu ve tüm seviyelere doğru analitik sırayla (ölçek, döndürme, temel nokta farkı, öteleme) uygulandı.
2. **P13 (Dikey / Eğik Elips Sınır Hesabı):**
   - Eski kodda elips ana eksen yönelimi (`atan2(my, mx)`) çıkarılmıyor ve sahne oluşturucuya `rotation: 0` iletiliyordu. Bu sebeple dikey elipsin genişlik ve yüksekliği ters hesaplanıyordu (`10x20` yerine `20x10`).
   - Elips ana eksen yönelim açısı hesaplanarak sahne nesnesine aktarıldı ve `CadExtractedSceneBuilder` elips için doğru extremum sınırlarını üretti.
3. **MINSERT Güvenlik Sınırı:**
   - Aşırı büyük satır/sütun matrisleri (örn. `rows * cols > 100,000`) doğrudan reddedilip tanı uyarısı verilir. Varlık kotası dolduğunda ızgara döngüsünden güvenli biçimde çıkılır.
4. **AutoCAD SOLID Dönüşümü:**
   - SOLID varlıkları `PolylinePrimitive` yerine içi taranan/dolu `PolygonPrimitive` olarak sahneye aktarıldı.

### Test ve Doğrulama Kanıtları:
- **Masaüstü Regresyon Testleri:**
  - `P12`: `[PASS] AcadSharpEntityExtractor Nested Block Transform Concatenation`
  - `P13`: `[PASS] CadExtractedSceneBuilder Vertical Ellipse Bounds Calculation`
  - `TestThreeLevelNestedBlockWithRotationAndScale`: `[PASS]`
  - `TestMinsertGridExpansion`: `[PASS]`
  - `TestRotatedEllipseBoundsCalculation`: `[PASS]`
  - `TestSolidFilledPolygonConversion`: `[PASS]`
  - Tüm geometri ve blok entegrasyon testleri: `STAGE09_GEOMETRY_BLOCK_TESTS_PASS`
- **Android Instrumentation (emulator-5554 / API 36):**
  - Tüm 9 native Android testi eksiksiz `PASS` (failed=0, code -1).
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`
- **Düzeltme Kapısı (Viewer Correction Gate):**
  - `RunId`: `D06-validation`
  - `Exit Code`: 0
  - P01–P08, P10, P11, P12, P13 yeşil. Kalan tek bilinen regresyon: D08 kapsamındaki P09 (`TextLayout.TotalWidth`).

### Kapanış Kararı:
D06 aşaması tüm kabul kriterleriyle başarıyla tamamlanmıştır. Bir sonraki aşama: **D07 — Stil, görünürlük, çizim sırası ve muhafazakâr bounds**.

---

## 9. Aşama D07 Raporu

- **Aşama Adı:** D07 — Stil, görünürlük, çizim sırası ve muhafazakâr bounds
- **Durum:** TAMAMLANDI (TrueColor/ACI ayrımı, RGB=0 siyah koruması, donmuş/kapalı katman ve varlık görünürlüğü, doküman çizgi tipi tablosu ve aktarımı, SortEntitiesTable/DRAWORDER ordinal çözümü, geniş polyline muhafazakâr sınırları ve StaticSceneBvh sıfır-tahsisli yığın havuzlaması doğrulandı)
- **Eklenen / Değişen Dosyalar:**
  - `src/MobilDwg.Core/Reading/CadExtractedDocument.cs` (`CadExtractedLayer` kaydına `IsFrozen = false` ve `HasTrueColor = false` alanları eklendi; `CadExtractedEntity` yapıcısında `drawOrder == 0` durumunda `DrawOrder = sourceOrder` varsayılan ordinal ataması sağlandı)
  - `src/MobilDwg.Cad/AcadSharp/AcadSharpEntityExtractor.cs` (Katman çıkarma adımında `IsFrozen`, `IsLocked`, `IsOn`, `HasTrueColor` açıkça okundu; `ResolveColor` metodunda `entity.Color.IsTrueColor` kontrolü yapılarak RGB=0 siyah rengin "truecolor yok" sayılarak ACI 7'ye düşmesi engellendi; `document.Entities` üzerinde `BlockRecord.SortEntitiesTable` tablosu taranarak AutoCAD `DRAWORDER` sırası sabit ordinale dönüştürüldü)
  - `src/MobilDwg.Rendering/Styles/CadEntityStyle.cs` (`byte Alpha = 255` saydamlık kanalı eklendi)
  - `src/MobilDwg.Rendering/Styles/CadStyleResolver.cs` (`effectiveStyle.Alpha < 255` durumunda alfa modülasyonu uygulanarak Skia'ya tam doğru saydamlık rengi aktarılması sağlandı)
  - `src/MobilDwg.Rendering/Geometry/GeometryPrimitives.cs` (`PolylinePrimitive` ve `GeometryBounds.ForPolyline` metoduna `maxWidth` parametresi eklendi; kalın/geniş polylineler için sınır kutusu W/2 yarı-genişlik payıyla dışa genişletilerek muhafazakâr bounds garantilendi)
  - `src/MobilDwg.Rendering/Spatial/StaticSceneBvh.cs` (Sorgu başına yapılan `new BvhNode[64]` tahsisi kaldırılarak `[ThreadStatic] t_queryStack` havuzuna bağlandı; referans sızıntısını önlemek için yığından çekilen slotlar sıfırlandı ve otomatik kapasite artırımı eklendi; sıfır GC tahsisi sağlandı)
  - `src/MobilDwg.Rendering/Scene/CadExtractedSceneBuilder.cs` (Dokümandaki `Linetypes` koleksiyonundan `CadLinetype` tablosu oluşturuldu; katman rengi çözümünde `HasTrueColor` önceliği verilerek ACI ezmesi engellendi; görünür olmayan varlıklar (`!entity.IsVisible`) filtrelendi; `Polyline` için köşe genişliklerinden `maxWidth` hesaplanıp `PolylinePrimitive`'e aktarıldı; `Unsupported` varlıklar için `ApproxBounds` varsa sınırlayıcı kutu üretilip tanı kaydı tutuldu)
  - `tests/MobilDwg.Integration.Tests/CadStyleAndBoundsD07Tests.cs` (D07 için 6 kapsamlı test: TrueColor RGB=0 siyah ve ACI 1 kırmızı çözümü, donmuş ve kapalı katman görünürlük kısıtı, özel çizgi tipi deseni ve piksel dönüşümü, SortEntitiesTable sıra doğruluğu, geniş polyline kenar sınır ve uzamsal kesişim testi, 2500 varlıkta brute-force ile %100 birebir eşleşen 200 BVH sorgusu)
  - `tests/MobilDwg.Integration.Tests/Program.cs` (`CadStyleAndBoundsD07Tests` koşusunun entegrasyon hattına bağlanması)
  - `docs/VIEWER_DUZELTME_DURUMU.md` (Durum belgesi güncellemesi)

### Çözülen Kusurlar ve Uygulanan Değişiklikler:
1. **TrueColor vs ACI Ayrımı ve RGB=0 Siyah Koruması:**
   - AutoCAD DXF/DWG formatında TrueColor tanımlı katmanlarda geriye dönük uyumluluk için bir ACI indeksi (örn. 7) de bulunur. Eski kod `l.AciIndex > 0` kontrolüyle TrueColor'ı eziyordu. Artık `l.HasTrueColor` önceliklidir.
   - RGB(0,0,0) değeri `TrueColor != 0` kontrolü nedeniyle "renk yok" sayılarak atlanıyordu; doğrudan `entity.Color.IsTrueColor` bayrağı ve R/G/B baytları okunarak siyah rengin korunması sağlandı.
2. **Görünürlük, Donmuş Katmanlar ve Invisibility:**
   - `LayerFlags.Frozen` katmanları `IsVisible = false` ve `IsRenderable = false` olarak işaretlendi; Skia çizim hattında bu katmandaki varlıkların boyanması engellendi.
   - Varlık seviyesindeki `entity.IsInvisible` bayrağı DTO'ya taşındı ve çizim oluşturucusunda dikkate alındı.
3. **DRAWORDER ve SortEntities Çözümü:**
   - ModelSpace üzerindeki `SortEntitiesTable` taranarak varlıklar AutoCAD sort handle sırasına dizildi ve DTO'da sabit `DrawOrder` ordinali aldı.
   - `RenderSceneAssembler` ve `StaticSceneBvh` sorgu sonuçları bu ordinal sırasını koruyarak Skia'nın doğru katmanlaşma sırasıyla çizmesini sağladı.
4. **Muhafazakâr Sınırlar (Conservative Bounds):**
   - Genişliği olan polylinelerde (`StartWidth`/`EndWidth`) merkez çizgisi sınırları değil, W/2 yarı-genişliği kadar genişletilmiş muhafazakâr sınır kutusu hesaplandı. Ekran/viewport kenarlarında kalın çizgilerin kırpılması (culling) önlendi.
5. **StaticSceneBvh Sıfır-Tahsisli Yığın:**
   - Her sorguda 64 elemanlık dizi oluşturulması yerine thread-static `BvhNode[]` dizisi havuzlandı ve sorgu anında sıfır ek bellek tahsis edildi.

### Test ve Doğrulama Kanıtları:
- **Masaüstü Entegrasyon Testleri:**
  - `TestTrueColorBlackAndAciResolution`: `[PASS]`
  - `TestFrozenAndOffLayerVisibility`: `[PASS]`
  - `TestLinetypeTableAndEntityInheritance`: `[PASS]`
  - `TestDrawOrderAndSortentsFidelity`: `[PASS]`
  - `TestWidePolylineConservativeBounds`: `[PASS]`
  - `TestBvhStackReuseAndBruteForceEquivalence`: `[PASS]` (2500 varlık üzerinde 200 BVH sorgusu brute-force ile %100 birebir eşleşti)
  - `P10` - `P13`: `[PASS]` (4/4 entegrasyon regresyon testi yeşil)
  - Tüm önceki aşama testleri: `STAGE01`, `STAGE08`, `STAGE09`, `STAGE10`, `STAGE11`, `STAGE12`, `STAGE13` eksiksiz `PASS`.
- **Masaüstü Rendering Regresyonları:**
  - P01–P08: 8/8 `PASS`. Tek kalan kırmızı test beklenen D08 hedefi P09 (`TextLayout.TotalWidth`).
- **Android Instrumentation (emulator-5554 / API 36):**
  - Tüm 9 native Android testi eksiksiz `PASS` (failed=0, code -1).
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`
- **Düzeltme Kapısı (Viewer Correction Gate):**
  - `RunId`: `D07-validation`
  - `Exit Code`: 0
  - Native, Core, Architecture, Integration testleri 0 hata ile yeşil.

### Kapanış Kararı:
D07 aşaması tüm kabul kriterleriyle başarıyla tamamlanmıştır. Bir sonraki aşama: **D08 — Metin, dimension ve hatch'in gerçek çizim doğruluğu**.

---

## 10. Aşama D08 Raporu

- **Aşama Adı:** D08 — Metin, dimension ve hatch'in gerçek çizim doğruluğu
- **Durum:** TAMAMLANDI (P09 yeşile çevrildi; 13/13 tüm kusurlar YEŞİL; SKFont gerçek ölçümü ve muhafazakâr metin sınırları, Türkçe Unicode karakterler, MTEXT zengin ayrıştırma/biçimlendirme, DIMENSION alt varlık stil/katman koruması ve yordamsal üretim yedeği, HATCH gerçek PAT ölçeği, dünya orijinli faz sürekliliği, EvenOdd/Outer/Ignore ada kuralları, ekranda 3px altı çizgi seyreltme kuralı ve PreparedGeometryCache HatchCoverage bağlantısı doğrulandı)
- **Eklenen / Değişen Dosyalar:**
  - `src/MobilDwg.Rendering/Text/TextLayout.cs` (`CharWidthRatio = 0.75d` karakter-sayısı tahmini kaldırıldı; font çözümlemesi üzerinden gerçek `SKFont(typeface, Height)` ile `MeasureText` ve glyph bounds ölçümüne geçildi; iniş (descent) ve satır aralığı font metriklerine bağlandı; muhafazakâr sınırlar analitik köşe dönüşümüyle hesaplandı)
  - `src/MobilDwg.Rendering/Text/TextLayoutMetrics.cs` (`CalculateTextBounds` metodu doğrudan `TextLayout` sınıfına delege edilerek uzamsal indeks ve çizim sınırları birebir eşitlendi)
  - `src/MobilDwg.Cad/AcadSharp/AcadSharpEntityExtractor.cs` (`CleanMText` metoduna `\X` satır sonu, `\~` bölünemez boşluk, `\L\l\O\o\K\k` stil geçişleri, `\S...^...;` kesir dönüştürme eklendi; `dim.Block.Entities` içindeki çocuk varlıklar çıkarılırken çocuk katmanın "0" olmaması durumunda çocuğun kendi katmanı ve `ByBlock` olmaması durumunda çocuğun kendi rengi korunarak ebeveyne kaba ezme engellendi)
  - `src/MobilDwg.Rendering/Scene/CadExtractedSceneBuilder.cs` (`CadExtractedEntityType.MText` için `MTextParser.Parse` entegrasyonu sağlandı ve font ailesi ile biçimlendirilmiş metin korundu; HATCH için uydurma `scale * 5.0` çarpanı kaldırılıp doğrudan gerçek desen ölçeğine (`hatchPayload.Scale > 0 ? hatchPayload.Scale : 1.0`) geçildi; HATCH için `HatchIslandStyle.Normal` ve dünya orijini korundu)
  - `src/MobilDwg.Rendering/Hatch/HatchProcessor.cs` (`GeneratePatternLines` ve `IsPointInsideHatch` metotlarına `HatchIslandStyle` (Normal, Outer, Ignore) desteği eklendi; maksimum çizgi bütçesinde hesaplanan `stride` kStart modülo hizalamasıyla dünya orijinine sabitlendi, böylece kaydırma ve yakınlaştırmada desenin yüzmesi önlendi)
  - `src/MobilDwg.Rendering/Scene/SceneGeometry.cs` (`WorldBounds2` yapısına `Intersect(WorldBounds2 other)` metodu eklendi)
  - `src/MobilDwg.Rendering/Skia/SkiaScenePainter.cs` (`DrawPrimitive` içinden `DrawHatchPrimitive` çağrısına `geometryCache`, `primitiveKey`, `sceneRevision`, `lodBand` parametreleri aktarıldı; rastgele satır sayısına dayalı `stride` kaldırılıp ekranda çizgi aralığı 3px altına indiğinde analitik seyreltme kuralı (`thinningStep = ceil(3.0 / projectedSpacing)`) uygulandı; `PreparedGeometryCache.TryGetHatchCoverage` ve `PutHatchCoverage` üretim çizim döngüsüne bağlandı; pan esnasında mevcut sınırlar anında boyanarak UP bekleme engellendi)
  - `tests/MobilDwg.Integration.Tests/CadTextDimensionHatchD08Tests.cs` (D08 için 5 kapsamlı entegrasyon testi: P09 geniş glifler (WWWW/MMMM) ve Türkçe Unicode metin sınırları, MTEXT biçimlendirme ve kesir ayrıştırma, DIMENSION çocuk stil/katman koruması ve yordamsal üretim yedeği, HATCH PAT ölçeği, dünya orijin faz sürekliliği ve ada kuralları, HATCH önbellek ve 3px seyreltme doğrulaması)
  - `tests/MobilDwg.Integration.Tests/Program.cs` (`CadTextDimensionHatchD08Tests` testlerinin entegrasyon test hattına bağlanması)
  - `docs/VIEWER_DUZELTME_DURUMU.md` (Durum belgesi güncellemesi)

### Çözülen Kusurlar ve Uygulanan Değişiklikler:
1. **P09 (TextLayout Sınırlarının Gerçek Font Ölçümünü Kapsaması):**
   - Eski kodda `CharWidthRatio = 0.75` ile karakter sayısı üzerinden genişlik tahmin ediliyordu. "WWWW" gibi geniş gliflerde gerçek font genişliği (373.63), tahmin edilen genişlikten (300.0) belirgin biçimde büyüktü ve uzamsal indeks kırpılmasına (culling) yol açıyordu.
   - Gerçek `SKFont.MeasureText` ve glif sınırları kullanılarak `TextLayout.TotalWidth` ve `Bounds` muhafazakâr biçimde hesaplandı. P09 yeşile döndü.
2. **MTEXT Zengin Biçimlendirme ve Türkçe Unicode Desteği:**
   - AutoCAD MTEXT etiketleri (`\P`, `\X`, `\~`, `\L`, `\O`, `\S`, `\F`) hem extractor hem de scene builder seviyesinde tam ayrıştırıldı.
   - Türkçe karakterler (`İ`, `ı`, `Ş`, `ç`, `ğ`, `ö`, `ü`) font ölçüm ve çizim döngüsünde tam doğrulandı.
3. **DIMENSION Katman ve Stil Koruması:**
   - AutoCAD anonim blok (`*D...`) barındıran ölçülendirmelerde, blok içerisindeki varlıkların kendi katmanları ve renkleri (ByBlock / ByLayer kurallarına göre) doğru korunarak ebeveyne ezdirilmedi.
   - Bloksuz ölçülendirmeler için Linear, Aligned, Radial ve Diametric yordamsal geometri ve `<> mm` metin ezme (override) desteği sağlandı.
4. **HATCH PAT Ölçeği, Faz Sürekliliği, Ada Kuralları ve Önbellek:**
   - Uydurma `scale * 5.0` çizgi aralığı kaldırıldı.
   - Desen çizgileri dünya koordinat orijinine göre k modülo ile indekslenerek kaydırma (pan) ve yakınlaştırma (zoom) esnasında desenin kayması/yüzmesi önlendi.
   - Normal (EvenOdd), Outer ve Ignore ada kuralları analitik olarak uygulandı.
   - Ekranda çizgi aralığı 3px altına indiğinde Moiré deseni ve siyahlaşmayı önleyen deterministik seyreltme kuralı uygulandı.
   - `PreparedGeometryCache` içindeki `PutHatchCoverage` / `TryGetHatchCoverage` `SkiaScenePainter`'a bağlandı.

### Test ve Doğrulama Kanıtları:
- **Masaüstü Regresyon Testleri (Rendering.Tests):**
  - `P01–P09`: **9/9 PASS** (P09 dahil tüm rendering regresyonları yeşil).
  - Tüm rendering aşama testleri: `STAGE10`, `STAGE11`, `STAGE12`, `STAGE13`, `STAGE14`, `STAGE15`, `STAGE16`, `STAGE17`, `STAGE18`, `STAGE19`, `STAGE20`, `STAGE21`, `STAGE22`, `STAGE25`, `STAGE26`, `STAGE03`, `STAGE04`, `STAGE06`, `STAGE07`, `STAGE09` eksiksiz `PASS`.
- **Masaüstü Entegrasyon Testleri (Integration.Tests):**
  - `P10–P13`: **4/4 PASS** (Tüm entegrasyon regresyonları yeşil).
  - `D08` Testleri:
    - `[PASS] D08: P09 and Turkish Unicode Text Layout Measurement verified`
    - `[PASS] D08: MTEXT formatting, line wrap, and font extraction verified`
    - `[PASS] D08: Dimension child geometry and procedural style preservation verified`
    - `[PASS] D08: Hatch PAT spacing, invariant origin phase, and island rules verified`
    - `[PASS] D08: Hatch coverage cache and 3px screen thinning rule verified`
  - Tüm önceki aşama testleri: `STAGE01`, `STAGE08`, `STAGE09`, `STAGE10`, `STAGE11`, `STAGE12`, `STAGE13` eksiksiz `PASS`.
- **Android Instrumentation (emulator-5554 / API 36):**
  - Tüm 9 native Android testi eksiksiz `PASS` (failed=0, code -1).
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`
- **Düzeltme Kapısı (Viewer Correction Gate):**
  - `RunId`: `D08-validation`
  - `Exit Code`: 0
  - `All Passed: True`
  - `Native Executed: True, Native Passed: True`
  - P01–P13: **13/13 KUSURUN TAMAMI YEŞİL!**

### Kapanış Kararı:
D08 aşaması tüm kabul kriterleriyle başarıyla tamamlanmıştır. Bir sonraki aşama: **D09 — Layout, referanslar ve gerçek UI araçları**.

---

## 11. Aşama D09 Raporu: Layout, Referanslar ve Gerçek UI Araçları

**Tarih:** 6 Eylül 2026  
**Durum:** **TAMAMLANDI**

### Gerçekleştirilen İyileştirmeler:
1. **Paper-Space Layouts & Viewport Sahnesi:**
   - `CadExtractedDocument` içerisine `CadExtractedLayout` ve `CadExtractedViewport` veri yapıları eklendi.
   - `AcadSharpEntityExtractor` hem model alanı hem de kağıt alanı (paper-space) bloklarını ve varlıklarını, layout boyutlarını, viewport merkez/twist/clipping sınırlarını ayrıştıracak şekilde güncellendi.
   - `CadExtractedSceneBuilder.BuildLayoutDefinitions` ile model uzayındaki geometri paper koordinatlarına dönüştürülüp, viewport twist açısı (`Matrix3x3.CreateRotation(-viewport.TwistAngleRadians)`), poligon kırpma sınırı ve dondurulmuş katman filtreleriyle `ViewportPrimitive` olarak paketlendi.
   - `CadViewerSession.SwitchLayout(name)` ile layout bazında kamera konumu (`_layoutCameras`) bellekte tutularak belgeyi baştan parse etmeden anında geçiş sağlandı.
   - `MainPage.cs` üzerinde yüzen adaya Layout butonu (`_navLayoutButton`) ve interaktif layout seçim bottom sheet modal'ı (`_layoutModalView`) bağlandı.
2. **Dış Referanslar (XREF ve Raster) Desteği:**
   - `CadXrefPayload` ve `CadRasterPayload` DTO'ları tamamlandı.
   - Çözümlenemeyen veya eksik olan XREF ve Raster görsellerinin sessizce yutulması (silent drop) engellendi; `CadExtractedSceneBuilder` seviyesinde `ReferencePlaceholderPrimitive` üretildi ve `UNRESOLVED_XREF` ile `MISSING_RASTER` tanı kodları (SceneDiagnostic) sahneye kaydedildi.
   - `SkiaScenePainter` ve `GeometryTessellator` eksik referanslar için kutu çerçevesi, köşegen çarpı ve referans türü/adı etiketini (`[XREF: name]` / `[RASTER: name]`) çizecek şekilde güncellendi.
3. **SnapQuery ve Hassas Ölçüm Araçları:**
   - Snap yakalama yarıçapı 12 DIP olarak tek bir defa ekran yoğunluğu (`12.0 * density`) ile piksel yarıçapına çevrildi.
   - **Eğrisel Snap:** Polyline bulge yayları için düz kiriş yerine analitik yay tepesi ve yay eğrisi üzerindeki en yakın nokta hesaplandı; merkez yakalaması (Center) eklendi.
   - **Spline Eğrisi:** Kontrol noktaları eğri dışındaysa (off-curve) snap reddedildi; eğrinin kendisi (`GeometryTessellator.Tessellate`) üzerinden örneklenen gerçek eğri noktalarına Curve snap sağlandı.
   - **Elips Snap:** Elips merkezi (Center) ve elips çevresi (Curve) hassas parametrik yay üzerinde yakalandı.
   - `MainPage.OnSingleTap` ölçüm noktası ekleme akışı doğrudan `_session.Measurement.AddScreenPointWithSnap` metoduna bağlandı.
4. **INSUNITS ve UI Format Doğrulamaları:**
   - `MeasurementController` içerisine INSUNITS (0..20) tam eşlemesi eklendi. 0 (Belirtilmemiş) değeri için `"çizim birimi"` ve `"çizim birimi²"`, 4 için `"mm"`, 6 için `"m"` formatlaması uygulandı. Sabit cm/m varsayımları kaldırıldı.
   - Başlık çubuğundaki dosya rozeti DXF dosyaları için `"DXF {version}"`, DWG dosyaları için `"DWG {version}"` olarak doğru formatı yansıtacak şekilde düzeltildi.
   - Ölçülmemiş yanıltıcı `"0 ms"` gecikme etiketi gizlendi.
5. **Tema Değişimi (Theme Mode Switching):**
   - Tema değişimi sırasında session'ı dispose edip BVH ve kamerayı sıfırlayan yıkıcı döngü kaldırıldı; `Session.SetColorContext` ile sahne, BVH, layout yöneticisi, kamera ve ölçüm modları korunarak canlı tema geçişi sağlandı.
6. **Android `ACTION_VIEW content://` Entegrasyonu:**
   - `MainActivity.cs` `Intent.ActionView` ile gelen `content://` ve `file://` URI'larını `ContentResolver.OpenInputStream` üzerinden okuyup `SafeCadFileCache.CopyAsync` ile güvenli şekilde önbelleğe alan ve `CadFileRequested` tetikleyen üretim yoluna kavuşturuldu.

### Test ve Doğrulama Kanıtları:
- **D09 Entegrasyon Testleri (`CadLayoutsAndToolsD09Tests.cs`):**
  - `TestMultiPaperLayoutSwitching`: **PASS** (Çoklu paper layout, viewport twist, frozen layer, polygon clip ve kamera kalıcılığı doğrulandı).
  - `TestCurvedSnapCalculations`: **PASS** (Bulge arc tepesi, chord segment ayrımı, spline kontrol noktası reddi ve gerçek eğri snap'i, elips merkez ve eğri snap'i doğrulandı).
  - `TestInsUnitsAndMeasurementFormatting`: **PASS** (INSUNITS 0..20 ve mesafe/alan birimleri doğrulandı).
  - `TestReferencePlaceholders`: **PASS** (Eksik XREF ve Raster için placeholder geometri ve tanı doğrulaması).
  - `TestThemeRetentionWithoutSessionDisposal`: **PASS** (Tema değişiminde kamera, BVH ve ölçüm durumunun korunması doğrulandı).
- **Masaüstü Test Harness:**
  - `Integration.Tests`: **HEPSİ GEÇTİ** (D09 dahil tüm testler exit code 0).
  - `Core.Tests`: **PASS**
  - `Architecture.Tests`: **PASS**
  - `Rendering.Tests`: **PASS** (P01–P09 dahil tüm aşamalar yeşil).
- **Android Native Instrumentation (emulator-5554 / API 36):**
  - Tüm 9 native Android testi eksiksiz `PASS` (failed=0, code -1).
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`
- **Düzeltme Kapısı (Viewer Correction Gate):**
  - `RunId`: `D09-validation`
  - `Exit Code`: 0
  - `All Passed: True`
  - `Native Executed: True, Native Passed: True`
  - `VIEWER_CORRECTION_GATE_PASS`

### Kapanış Kararı:
D09 aşaması tüm kabul kriterleriyle başarıyla tamamlanmıştır. Bir sonraki aşama: **D10 — Surface kaybı, arka plan ve düşük bellek (K12)**.

---

## 12. Aşama D10 Raporu: Surface Kaybı, Arka Plan ve Düşük Bellek (K12)

**Tarih:** 6 Eylül 2026  
**Durum:** **TAMAMLANDI**

### Gerçekleştirilen İyileştirmeler:
1. **Açık Yaşam Döngüsü Durumları (Host Lifecycle State Machine):**
   - `CadViewportView` içerisine `ViewportLifecycleState` (`Detached`, `Attached`, `Resumed`, `Paused`) durum makinesi eklendi.
   - `AndroidFrameClock` sınıfına `Pause()`, `Resume()` ve `IsPaused` kontrolü eklendi; arka plana geçişte Choreographer callback kaydı durdurularak gereksiz CPU/GPU döngüleri engellendi.
   - `AndroidViewportInputAdapter` içerisine `CancelCurrentGesture()` metodu eklenerek arka plana geçişte etkin jestler anında iptal edildi ve girdi durumu temizlendi.
   - `MainActivity.cs` üzerinde `OnPause()` ve `OnResume()` yaşam döngüsü metodları `HostPaused` ve `HostResumed` statik olaylarına bağlandı; `CadViewportView` bu olaylara abone olarak arka planda saatleri ve watchdog'u durdurup resume anında yeni surface snapshot'ı ile temiz ilk kareyi talep eder hale getirildi.
2. **Sağlam Watchdog Kontrolü ve Güvenli Fallback:**
   - Watchdog zaman aşımı 1000 ms olarak belirlendi.
   - Watchdog callback anında; view'ın görünür, eklenmiş (attached), devam eden (resumed), pozitif boyutlu, oturum ve surface jenerasyon token'ının eşleştiği, jest durumunun `Idle` olduğu ve gerçekten yerine getirilmemiş bir kare isteğinin (`IsFrameAwaitingOrScheduled || HasActiveTicket`) bulunduğu koşullarını katı şekilde denetler.
   - İlk zaman aşımında GL yüzeyini bir defa yeniden başlatır (`_watchdogRetries = 1`). İkinci zaman aşımında aynı oturum için kalıcı olarak Software (SKCanvasView) çizim moduna geçer.
   - `DisarmWatchdog()` çağrısı yalnızca `frameRendered == true` olduğunda çalışacak şekilde guard altına alındı; reddedilen veya boş dönen çizimlerin watchdog'u yanıltıcı şekilde kapatması önlendi.
3. **GL Hata Sınıflandırması ve Context Restore:**
   - `IsGlBackendException` sınıflandırıcısı ile GPU context, EGL, GrContext ve Skia donanım hataları açıkça ayırt edildi; bu hatalar loglanarak UI iş parçacığında güvenle Software fallback'e yönlendirildi.
   - Geometrik ve programlama hatalarının GPU arızası gibi sessizce yutulması engellendi.
   - Eski `SKGLView` ve `SKCanvasView` bileşenleri temizlenirken `PaintSurface` olay abonelikleri kaldırılarak askıda kalan bağlamların sızıntı yapması önlendi.
4. **Düşük Bellek (TrimMemory) ve Kaynak Temizliği:**
   - `MainActivity.OnTrimMemory(TrimMemory level)` MAUI uygulama döngüsüne bağlandı ve `MainActivity.LowMemoryTrimmed` olayı oluşturuldu.
   - `MainPage.cs` `OnTrimMemory` bildirimini aktif `CadViewerSession.OnTrimMemory()` metoduna yönlendirdi.
   - `CadViewerSession.OnTrimMemory()` çağrıldığında hem `PreparedGeometryCache` hem de `RenderResourceCache` 0 bayta kadar tahliye edilerek tüm önbellekler temizlendi.
   - `MainPage.OnHandlerChanging` içerisinde statik event abonelikleri (`MainActivity.CadFileRequested` ve `MainActivity.LowMemoryTrimmed`) sayfa ayrılırken eksiksiz kaldırıldı, bellek sızıntıları önlendi.

### Test ve Doğrulama Kanıtları:
- **D10 Entegrasyon Testleri (`CadLifecycleD10Tests.cs`):**
  - `TestFrameRequestGateAwaitingAndTicketInspection`: **PASS** (FrameRequestGate durum geçişleri ve aktif ticket takibi).
  - `TestWatchdogDecisionLogic`: **PASS** (Boşta reddetme, aktif jestte reddetme, eski jenerasyonda reddetme, 1. zaman aşımında GL yeniden başlatma, 2. zaman aşımında kalıcı Software geçişi).
  - `TestActiveSessionMemoryTrimClearsAllCaches`: **PASS** (Düşük bellekte her iki önbelleğin 0 bayta boşaltılması).
  - `TestDrainAfterRetiringWithActiveLeases`: **PASS** (Aktif lease'lerin elden çıkarılma sonrası güvenle tahliyesi).
  - `TestGlErrorClassification`: **PASS** (GPU/EGL hataları ile genel kodlama hatalarının sınıflandırılması).
  - `TestConcurrentFrameTicketsAndSurfaceGenerationsUnderLifecycleChanges`: **PASS** (Hızlı arka plan/ön plan geçişlerinde geçersiz jenerasyonların reddi).
- **Masaüstü Test Harness:**
  - `Architecture.Tests`: **PASS** (`STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`)
  - `Core.Tests`: **PASS**
  - `Rendering.Tests`: **PASS** (Tüm 30+ aşama ve P01–P09 regresyonları eksiksiz yeşil).
  - `Integration.Tests`: **PASS** (D01–D10 tüm entegrasyon testleri exit code 0).
- **Android Native Instrumentation (emulator-5554 / API 36):**
  - Tüm 9 native Android testi eksiksiz `PASS` (failed=0, code -1).
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`
- **Düzeltme Kapısı (Viewer Correction Gate):**
  - `RunId`: `20260906-145558`
  - `Exit Code`: 0
  - `All Passed: True`
  - `Native Executed: True, Native Passed: True`
  - `VIEWER_CORRECTION_GATE_PASS`

### Kapanış Kararı:
D10 aşaması tüm kabul kriterleriyle başarıyla tamamlanmıştır. Bir sonraki aşama: **D11 — Üretim yolunda doğruluk, akıcılık ve uzun süre testi**.

---

## 13. Aşama D11 Raporu: Üretim Yolunda Doğruluk, Akıcılık ve Uzun Süre Testi

**Tarih:** 6 Eylül 2026  
**Durum:** **TAMAMLANDI (İşlevsel ve Emülatör Doğrulaması)**  
*Fiziksel Cihaz Metrikleri:* **ÖLÇÜLMEDİ (BEKLEMEDE: Fiziksel referans telefon koşusu gerektirir)**

### Gerçekleştirilen İyileştirmeler:
1. **Uçtan Uca Viewport Telemetri Zinciri (Input -> Camera -> Request -> Paint):**
   - `ViewportInteractionEngine.LastInputEventTimeMs` alanı ile native Android `motionEvent.EventTime` (uptime millis) girdi anı yakalandı.
   - `FrameRequestGate.LastRequestTicks` ile kare talep anı yüksek çözünürlüklü monoton `Stopwatch` saatiyle damgalandı.
   - `CadViewportView.RenderFrameCore` içerisinde `paintStartTicks` ve `paintEndTicks` ölçülerek `ViewportTelemetry.Instance.Record(...)` çağrısı entegre edildi.
   - Android platformunda `SystemClock.UptimeMillis()` ile `Stopwatch.GetTimestamp()` monoton saat kalibrasyonu (`UpdateClockCalibration`) her çizimde güncellendi.
   - `ViewportTelemetrySample.CalculateInputToPaintEndMs()` ile dokunma anından Skia GL çizim bitişine kadar olan gerçek gecikme (Input-to-Paint Latency) hesaplandı ve `mobildwg_telemetry.csv` olarak dışa aktarıldı.
2. **Parmak Basılı Tutulurken (Sustained Hold) 4 Yönlü Sentinel Doğrulaması:**
   - Parmak bırakılmadan (`PointerAction.Up` gönderilmeden önce) aktif jest sürerken ekran dışında kalan nesnelerin (Kuzey, Güney, Doğu, Batı) sırayla görünür alana girmesi ve BVH uzamsal indeksinden çekilmesi sağlandı.
   - Android Runtime üzerinde çalışan `MobilDwgTestRunner` içerisine `RunNativeSustainedHoldSentinelTestAsync` eklendi; sürükleme sırasında UP gönderilmeden `UiAutomation.TakeScreenshot()` ile ekran görüntüsü alındı ve `49,956` piksel çizim yapıldığı (`mobildwg_sustained_hold_screen.png`) kanıtlandı.
3. **Corpus Ölçek Bütçeleri (10k, 50k, 150k, 250k Varlık):**
   - `CadProductionD11Tests.cs` test paketi oluşturuldu.
   - 10k, 50k, 150k ve 250k sentetik varlık koleksiyonları üzerinde BVH inşası ve uzamsal sorgu süreleri (tümü < 80ms) doğrulandı.
   - 150k varlıklı sahnede çizim süresi şartname sınırları (< 500ms) içinde kalarak doğrulandı.
   - Public sentetik DXF fikstürleri için SHA-256 bütünlük kontrolü ve korumalı ayrıştırma test edildi.
4. **Sakinleşme ve Sıfır Sahte Çizim (Settle Frame Inactivity):**
   - Dokunma eylemi sonlanıp boşta (Idle) kalındığında ve hareket delta'sı 0 olduğunda gereksiz kare taleplerinin engellendiği (`frameRequests == 0`), aşırı zoom limitine ulaşıldığında kameranın kilitlenip sahte kare oluşturmadığı doğrulandı.
5. **Bellek Soak ve Lease Tahliyesi (Memory Soak & Lease Drain):**
   - 10 ısınma + 30 ölçümlü (toplam 40 döngü) tam belge oturumu açma/çizme/kapama testi yapıldı.
   - Her döngü sonunda `session.ActiveLeaseCount == 0`, `GeometryCache` ve `ResourceCache` tahliyesinin eksiksiz çalıştığı, live-owner sızıntısı olmadığı kanıtlandı.
   - Android Runtime üzerinde 15 ardışık trim/dispose soak döngüsü `NATIVE_MEMORY_DRAIN_SOAK` ile yeşile bağlandı.

### Test ve Doğrulama Kanıtları:
- **D11 Entegrasyon Testleri (`CadProductionD11Tests.cs`):**
  - `TestSparseAndDenseCorpusScaleBudgets`: **PASS** (10k, 50k, 150k, 250k ölçek bütçeleri).
  - `TestInputToPaintTelemetryChain`: **PASS** (Girdi-çizim telemetri zinciri, monoton saat kalibrasyonu, CSV çıktısı).
  - `TestSustainedHoldFourDirectionSentinels`: **PASS** (UP öncesi 4 yönlü sentinel görünürlük ve BVH sorgusu).
  - `TestSettleFrameInactivity`: **PASS** (0 delta pan ve limit zoom durumunda gereksiz çizim üretilmeme).
  - `TestMemorySoakAndLeaseDrain`: **PASS** (40 döngü bellek soak, 0 aktif lease, 0 sızıntı).
  - `TestCorpusFixtureManifestAndIntegrityAsync`: **PASS** (Fikstür SHA-256 kontrolü).
- **Masaüstü Test Harness:**
  - `Architecture.Tests`: **PASS** (`STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`)
  - `Core.Tests`: **PASS**
  - `Rendering.Tests`: **PASS** (Tüm 30+ aşama ve P01–P09 regresyonları eksiksiz yeşil).
  - `Integration.Tests`: **PASS** (D01–D11 tüm entegrasyon testleri exit code 0).
- **Android Native Instrumentation (emulator-5554 / API 36):**
  - Tüm 12 native Android testi eksiksiz `PASS` (failed=0, code -1):
    - `NATIVE_P01_FIRST_SURFACE_GENERATION`: **PASS**
    - `NATIVE_P02_CONCURRENT_PAINT_GUARD`: **PASS**
    - `NATIVE_P03_HOST_CLOCK_ARMING`: **PASS**
    - `NATIVE_P04_CAMERA_REVISION`: **PASS**
    - `NATIVE_P05_FINAL_FRAME_NOTIFICATION`: **PASS**
    - `NATIVE_SAMPLE_DRAWING_FIRST_FRAME`: **PASS** (2526 çizim pikseli)
    - `NATIVE_DXF_OPEN_RENDER`: **PASS** (56,757 çizim pikseli)
    - `NATIVE_HOST_TOUCH_CLOCK_ARM`: **PASS**
    - `NATIVE_ENGINE_SMOKE_TESTS`: **PASS**
    - `NATIVE_SUSTAINED_HOLD_SENTINEL`: **PASS** (UP öncesi aktif basılı tutma altında 49,956 piksel çizim)
    - `NATIVE_TELEMETRY_RECORDING`: **PASS** (Girdi gecikmesi ve çizim süresi kaydedildi, `mobildwg_telemetry.csv` üretildi)
    - `NATIVE_MEMORY_DRAIN_SOAK`: **PASS** (15 Android oturum döngüsü, 0 sızıntı)
  - `STAGE05_NATIVE_INSTRUMENTATION_PASS`
  - `STAGE13_NATIVE_TOUCH_FIDELITY_PASS`
- **Düzeltme Kapısı (Viewer Correction Gate):**
  - `RunId`: `20260906-150451`
  - `Exit Code`: 0
  - `All Passed: True`
  - `Native Executed: True, Native Passed: True`
  - `VIEWER_CORRECTION_GATE_PASS`

### Kapanış Kararı:
D11 aşaması işlevsel kod ve emülatör kabul kriterleriyle başarıyla tamamlanmıştır. Fiziksel cihaz metrikleri şartname gereğince "ÖLÇÜLMEDİ (Fiziksel Cihaz Beklemede)" olarak kayıt altına alınmıştır. Bir sonraki aşama: **D12 — Temiz checkout, gerçek sürüm kanıtı ve kapanış**.

---

## 14. Aşama D12 Raporu

- **Aşama Adı:** D12 — Temiz checkout, gerçek sürüm kanıtı ve kapanış
- **Durum:** **TAMAMLANDI** (13/13 kusur kapalı, kilitli bağımlılık bütünlüğü sağlandı, CI yapılandırıldı, Release APK üretildi ve native kapı yeşil)
- **Yapılan İyileştirmeler ve Doğrulamalar:**
  1. **CI İş Akışı ve Exact SDK Pini (`.github/workflows/viewer-stability.yml`):**
     - `10.0.x` sürüm belirsizliği ve SDK çelişkisi giderildi. `global.json` içerisindeki kesin pin (`10.0.100`) bağlandı.
     - Java 17 ve `maui-android` iş yükü açıkça yapılandırıldı.
     - `dotnet restore --locked-mode` ile derleme öncesi bağımlılık kilit kontrolü şart koşuldu.
     - `desktop-verification` (Architecture, Core, Rendering, Integration testleri) ve `android-release-and-instrumentation` (Release APK oluşturma, emulator çalıştırma ve instrumentation) işleri ayrılarak artifact saklama eklendi.
  2. **Paket ve Bağımlılık Provenance Manifestosu:**
     - `CadReleaseRcAuditor.cs` ve `CadFinalRcAuditor.cs` içerisindeki tüm `...verified` yer tutucuları kaldırıldı; her NuGet paketinin gerçek ve kesin SHA-256 sağlama toplamları koda işlendi.
     - `packages.lock.json` ile tüm çözümlerde `dotnet restore --locked-mode` hatasız ve uyarısız doğrulandı.
  3. **Mobil Uygulama Intent ve Gecikmeli Başlatma Güvenliği:**
     - `MainActivity.cs`: `CadFileRequested` olayına sayfa (`MainPage`) henüz bağlanmadan önce gelen CAD açma talepleri (`open_cad` extra veya `ACTION_VIEW`) için kuyruk mekanizması (`_pendingCadFile`) eklendi; soğuk başlatmada niyetlerin kaybolması engellendi.
     - `MobilDwgTestRunner.cs`: Örnek çizim kartı aramasında hem ViewId (`com.smitelagwar.mobildwg:id/sample-card-apartman`) hem de metin desteği sağlandı; fallback intent'te `SingleTop` ile activity yaşam döngüsü korundu.
  4. **Üretim (Release) Paketleri ve Kanıtları:**
     - **Ana Uygulama Release APK:**
       - Yol: `src/MobilDwg.App/bin/Release/net10.0-android36.0/com.smitelagwar.mobildwg-Signed.apk`
       - Boyut: `40,416,765` bayt (~38.54 MB, şartname 45 MB tavan bütçesinin altında)
       - SHA-256: `a0303833b420652bc943cef37aad748d627d9ed4e694762696a9e73a8ee1e10c`
     - **Enstrümantasyon Test Release APK:**
       - Yol: `tests/MobilDwg.Android.Instrumentation/bin/Release/net10.0-android36.0/com.smitelagwar.mobildwg.test-Signed.apk`
       - Boyut: `19,462,938` bayt
       - SHA-256: `2dac00c062b65544f1cff500b2dfa0ab865179e25366b759000cc96ed84bd3eb`
  5. **Nihai Kapı Doğrulaması (Viewer Correction Gate):**
     - Komut: `powershell -ExecutionPolicy Bypass -File scripts/viewer-correction-gate.ps1 -Tag D12-final-closure -RequireNative -ReinstallApk`
     - `RunId`: `20260906-181441`
     - Masaüstü Testleri:
       - `ArchitectureTests`: **PASS** (ExitCode: 0)
       - `CoreTests`: **PASS** (ExitCode: 0)
       - `RenderingTests`: **PASS** (ExitCode: 0)
       - `IntegrationTests`: **PASS** (ExitCode: 0)
     - Android API 36 Enstrümantasyon Testleri (`emulator-5554`):
       - `NATIVE_P01_FIRST_SURFACE_GENERATION`: **PASS**
       - `NATIVE_P02_CONCURRENT_PAINT_GUARD`: **PASS**
       - `NATIVE_P03_HOST_CLOCK_ARMING`: **PASS**
       - `NATIVE_P04_CAMERA_REVISION`: **PASS**
       - `NATIVE_P05_FINAL_FRAME_NOTIFICATION`: **PASS**
       - `NATIVE_SAMPLE_DRAWING_FIRST_FRAME`: **PASS** (2711 çizim pikseli)
       - `NATIVE_DXF_OPEN_RENDER`: **PASS** (56,757 çizim pikseli)
       - `NATIVE_HOST_TOUCH_CLOCK_ARM`: **PASS**
       - `NATIVE_ENGINE_SMOKE_TESTS`: **PASS**
       - `NATIVE_SUSTAINED_HOLD_SENTINEL`: **PASS** (49,956 çizim pikseli)
       - `NATIVE_TELEMETRY_RECORDING`: **PASS** (`mobildwg_telemetry.csv`)
       - `NATIVE_MEMORY_DRAIN_SOAK`: **PASS** (15 Android oturum döngüsü, 0 sızıntı)
     - Sonuç: `Code=-1`, `FailedTestsCount=0`, `All Passed: True`, `Native Passed: True` -> `VIEWER_CORRECTION_GATE_PASS`.

---

## 15. Genel Kapanış ve Nihai Değerlendirme

| Kapsam | Durum | Kanıt & Açıklama |
|:---|:---:|:---|
| **D01–D10 Düzeltmeleri** | **TAMAMLANDI** | P01–P13 regresyonlarının tümü giderildi, 4 ayrı test projesinde 100+ senaryo ile doğrulandı. |
| **D11 Üretim Doğruluğu** | **TAMAMLANDI** | 10k–250k varlık bütçeleri, UP öncesi basılı tutma sentinelleri, telemetri döngüsü ve 40 döngü bellek drain soak doğrulandı. |
| **D12 Sürüm & Kapı** | **TAMAMLANDI** | Locked restore, Release APK üretimi, exact nupkg/APK SHA-256 ve CI yapılandırması tamamlandı. |
| **Android Enstrümantasyon** | **TAMAMLANDI** | Android API 36 (x86_64) üzerinde 12/12 yerel test hatasız geçti (`VIEWER_CORRECTION_GATE_PASS`). |
| **Fiziksel Cihaz Metrikleri** | **ÖLÇÜLMEDİ (Beklemede)** | Şartname 216/229 gereğince fiziksel donanım testleri donanım sağlandığında çalıştırılmak üzere "ÖLÇÜLMEDİ / BEKLEMEDE" olarak dürüstçe kayıt altına alınmıştır; emülatör sonuçları fiziksel cihaz yerine ikame edilmemiştir. |
