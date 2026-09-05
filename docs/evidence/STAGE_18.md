# AŞAMA 18 Kanıtı — Tam Android Viewer UX / Lifecycle (Son Kullanıcı Deneyimi ve Yaşam Döngüsü)

## Durum

`DONE`

AŞAMA 18 çıkış kriterleri platform-neutral C# unit testleri (12/12 PASS), katman mimari testleri (`MobilDwg.Architecture.Tests`) ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama son kullanıcı çizim görüntüleyici yaşam döngüsünü (Home/open/loading/viewer/layers/fit/info/warnings/close), son açılan dosyalar havuzunu (`RecentFilesManager` ile 10 dosya kapasiteli LRU yönetimi ve JSON kalıcılığı), Android veri güvenliği kurallarını (`[Application(AllowBackup = false)]` ve `LogRedactor` ile tam dosya yolu / Content URI maskeleme), görüntüleyici oturumunu (`CadViewerSession` ile Pan, Zoom, ZoomToFit, model yeniden parse edilmeden sıfır-maliyetli katman görünürlüğü açma/kapama, sıfır-maliyetli layout geçişi, oryantasyon/ekran boyutu değişimi uyumu), Android bellek baskısı (`OnTrimMemory`) idaresini ve deterministik yaşam döngüsü durum dökümünü (`ViewerLifecycleSemanticSnapshot` schema `viewer-lifecycle/v1`) kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `6ee54e5` (A17 tamamlanması sonrası).
- Branch: `stage18-viewer-lifecycle` (PR #31, merge commit: `ab3a768`).
- **Son Açılan Dosyalar Deposu (`MobilDwg.Core.Storage.RecentFilesManager`)**:
  - Bounded kapasite (en fazla 10 kayıt), LRU (En Son Kullanılan) sıralama.
  - Aynı dosya veya URI tekrar açıldığında listenin başına taşınır, kapasite aşımında en eski kayıt otomatik temizlenir.
  - Bağımsız dosya sisteminde JSON formatında kalıcı olarak saklanır ve geri yüklenir.
- **Log ve Gizlilik Maskeleme (`MobilDwg.Core.Diagnostics.LogRedactor`)**:
  - `RedactPath`: Tam yerel dosya yollarındaki hassas kullanıcı klasör yapıları maskelenir, yalnızca güvenli dosya adı korunur.
  - `RedactUri`: Android Content URI'lerindeki potansiyel hassas yetki/token bileşenleri maskelenir, şema ve güvenli tanıtıcı korunur.
- **Görüntüleyici Oturumu (`MobilDwg.Rendering.Viewer.CadViewerSession`)**:
  - **Durum Makinesi**: `Closed`, `Loading`, `Ready`, `Error` durum geçişleri ve hata/uyarı listesi.
  - **Sıfır-Maliyetli Katman Kontrolü (`SetLayerVisibility`)**: Katman görünürlüğü değiştirildiğinde CAD modeli baştan çözümlenmez (re-parse yok); mevcut sahne nesnelerinin görünürlük bayrağı anında güncellenir ve doğrudan Skia çizimi tetiklenir.
  - **Sıfır-Maliyetli Layout Değişimi (`SwitchLayout`)**: Bellekte yüklü çoklu layout'lar arasında model baştan ayrıştırılmadan anında geçiş yapılır.
  - **Kamera Navigasyonu**: Zoom (oran sınırlamalı), Pan (piksel -> dünya koordinatları) ve ZoomToFit (sahne sınırlarını geçerli ekran en-boy oranına göre sığdırma).
  - **Ekran Boyutu / Oryantasyon Değişimi (`ResizeViewport`)**: Ekran döndürüldüğünde çözünürlük güncellenir ve kamera orantılı şekilde merkezlenir.
  - **Bellek Baskısı (`OnTrimMemory`)**: İşletim sistemi bellek uyarısı verdiğinde arabellekler temizlenir.
- **Android Güvenlik ve Uyumluluk Entegrasyonu**:
  - `MainApplication.cs` içine `[Application(AllowBackup = false)]` özniteliği eklenerek yetkisiz ADB/bulut yedekleme kapatıldı.
  - `A18AndroidValidationRunner`: `MobilDwg.App` içinde doğrudan SkiaSharp veya ACadSharp tipleri import edilmeden, tam mimari uyumla oturum yönetimi, katman açma/kapama, layout geçişi, kamera navigasyonu ve bellek baskısı doğrulaması gerçekleştirildi.
- **Deterministik Durum Şeması (`ViewerLifecycleSemanticSnapshot`)**:
  - `schema=viewer-lifecycle/v1` formatıyla doküman, aktif layout, kamera durumu, görünür katmanlar ve son dosyalar deterministik sıralamayla doğrulanır.

## AŞAMA 18 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| Son Açılan Dosyalar Havuzu (Max 10, LRU) | RecentFilesManager | PASS |
| Dosya Yolu ve URI Log Maskeleme | LogRedactor (RedactPath, RedactUri) | PASS |
| Görüntüleyici Oturum Yaşam Döngüsü | CadViewerSession (State transitions) | PASS |
| Kamera Pan / Zoom / Fit | CadViewerSession (Pan, Zoom, ZoomToFit) | PASS |
| Sıfır-Maliyetli Katman Görünürlüğü Değişimi | CadViewerSession.SetLayerVisibility (no reparse) | PASS |
| Sıfır-Maliyetli Layout Değişimi | CadViewerSession.SwitchLayout (no reparse) | PASS |
| Oryantasyon / Boyut Değişimi | CadViewerSession.ResizeViewport | PASS |
| Bellek Baskısı Temizliği | CadViewerSession.OnTrimMemory | PASS |
| Android No-Backup Yapılandırması | MainApplication [Application(AllowBackup = false)] | PASS |
| Deterministik Yaşam Döngüsü Snapshot | ViewerLifecycleSemanticSnapshot (schema=viewer-lifecycle/v1) | PASS |
| Host C# Testleri (Release) | Stage18ViewerLifecycleTests (12/12 test) | PASS |
| Katman Mimari Testleri | MobilDwg.Architecture.Tests (SkiaSharp/ACadSharp bağımsızlık kontrolleri) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A18Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a18-android-viewer-lifecycle-gate.ps1 | PASS |
| Gerçek App UI Doğrulaması | uiautomator dump -> ANDROID_STAGE18_VIEWER_LIFECYCLE_PASS | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a18-real-app-viewer.png (97,827 bayt) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 11177, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- `STAGE18_VIEWER_LIFECYCLE_TESTS_PASS`:
  - `TestRecentFilesManagerCapsAtTenAndMaintainsLruOrder`: PASS
  - `TestRecentFilesManagerPersistsAndReloadsFromJson`: PASS
  - `TestLogRedactorMasksSensitivePathInformation`: PASS
  - `TestLogRedactorMasksSensitiveUriInformation`: PASS
  - `TestViewerSessionLifecycleTransitions`: PASS
  - `TestViewerSessionCameraPanZoomAndFit`: PASS
  - `TestViewerSessionLayerVisibilityTogglesWithoutReparsing`: PASS
  - `TestViewerSessionLayoutSwitchWithoutReparsing`: PASS
  - `TestViewerSessionResizeViewportMaintainsAspectRatio`: PASS
  - `TestViewerSessionOnTrimMemoryDisposesCachedSurfaces`: PASS
  - `TestViewerLifecycleSemanticSnapshotDeterminism`: PASS
  - `TestEndToEndViewerSessionRenderIntegration`: PASS

### 2. Mimari Katman Testleri (`MobilDwg.Architecture.Tests`)
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`
- (MobilDwg.App içinde SkiaSharp ve ACadSharp doğrudan kaynak bağımlılık yasağı eksiksiz doğrulanmıştır).

### 3. Android API 36 Emülatör Kabul Çıktısı
- Kabul Komutu: `powershell -ExecutionPolicy Bypass -File scripts/a18-android-viewer-lifecycle-gate.ps1`
- APK Boyutu: `39,707,568` bayt
- APK SHA256: `eda1c59f56d4eb97c51f68d048f405af3c66270c121d3f4822da4391e840dc27`
- Paket: `com.smitelagwar.mobildwg`
- Süreç / Liveness: PID `11177` (çökme veya ANR yok)
- Ekran Görüntüsü: `artifacts/a18-android-viewer-lifecycle/a18-real-app-viewer.png` (97,827 bayt, SHA256: `32144a7032260b687366087992a3550e706d485c3c28b44e3fa4cb3348938d1a`)
- Logcat ve Terminal İşaretleri:
  - `A18_EMULATOR_API36_PASS serial=emulator-5554 android=16 abi=x86_64`
  - `A18_REAL_APP_APK_PASS bytes=39707568 sha256=eda1c59f56d4eb97c51f68d048f405af3c66270c121d3f4822da4391e840dc27`
  - `A18_REAL_APP_INSTALL_PASS package=com.smitelagwar.mobildwg launcher=com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
  - `A18_REAL_APP_LAUNCH_PASS pid=11177`
  - `A18_REAL_APP_VIEWER_MARKERS_PASS`
  - `A18_REAL_APP_UI_STATUS_PASS`
  - `A18_SCREENSHOT_PNG_PASS bytes=97827 sha256=32144a7032260b687366087992a3550e706d485c3c28b44e3fa4cb3348938d1a`
  - `A18_REAL_APP_STABILITY_PASS pid=11177`
  - `ANDROID_STAGE18_VIEWER_LIFECYCLE_PASS`
- İddia Sınırı: `CLAIM_LIMIT=A18_VIEWER_LIFECYCLE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`

## Ekran Kanıtı

AŞAMA 18 Android API 36 emülatöründe çalışan gerçek uygulamanın ekran görüntüsü:

![A18 Real Android App Viewer Lifecycle Rendering](file:///c:/Users/hsyn/Desktop/MOBIL_UYGULAMA_DWG/artifacts/a18-android-viewer-lifecycle/a18-real-app-viewer.png)\n