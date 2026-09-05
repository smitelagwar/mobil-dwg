# AŞAMA 11 Kanıtı — Mobil Viewport ve Gesture (Pan, Pinch Zoom, Double Tap, Fit Extents, Orientation Resize)

## Durum

`DONE`

AŞAMA 11 çıkış kriterleri platform-neutral C# unit testleri ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama mobil viewport yönetimi, odak noktası korumalı pinch zoom, kesintisiz pan, double-tap, fit extents ve CAD reparse olmaksızın oryantasyon boyut değişimini kapatır; CAD parse-to-scene ve fiziksel cihaz doğrulaması sonraki aşamalardadır.

## Kapsam ve Kararlar

- Base `main` HEAD: `7ce9508afbb97fce940584661ea16ff5c95d4f0b`.
- Branch: `stage11-viewport-gesture`.
- PR: `#24` — `feat(a11): implement mobile viewport and gesture interaction`.
- `Camera2D` viewport ve koordinat dönüşümleri:
  - `PanBy(double deltaScreenX, double deltaScreenY)`: Piksel kaydırmasını dünya koordinat vektörüne dönüştürerek kamerayı kaydırır.
  - `ZoomAt(double screenPivotX, double screenPivotY, double zoomFactor)`: Dokunma/pinch merkezini (focal point) sabit tutarak dünya koordinatını tam olarak korur (`|worldBefore - worldAfter| < 1e-4`).
  - `Resize(int newWidth, int newHeight, bool maintainWorldCenter)`: Ekran boyutu veya cihaz oryantasyonu değiştiğinde dünya merkezini koruyarak kamerayı yeniden boyutlandırır.
- `ViewportController` jest denetleyicisi:
  - Mobil dokunmatik jestler için birleştirilmiş yönetim state machine'i: Pan, Pinch Zoom, Double-Tap, Fit Extents, Resize.
  - Etkileşim telemetrisi (`InteractionTelemetry`) ile jest ve çizim sayaçları.
- `SkiaCadRenderer` kamera ile çizim:
  - `RenderCameraWithStatsAsync`: CAD belgesi yeniden taranmadan/çözümlenmeden (Zero CAD Reparse), doğrudan mevcut `RenderScene` üzerinden yeni `Camera2D` viewport ile render alma.
- Gerçek `MobilDwg.App` API 36 üzerinde jest kabul yolu:
  - Sentetik sahne üzerinde pan, focal-preserved pinch zoom, double tap, fit extents ve orientation resize ardışık olarak koşturulur.
  - Her jest adımında pikseller ve dönüşüm matrisleri doğrulanır.
  - Son durum PNG olarak kaydedilip MAUI UI'a aktarılır.
- Ekran görüntüsü ve logcat kabul kanıtları alındı.

## AŞAMA 11 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| Pan jesti (ekran delta -> dünya kaydırma) | Camera2D.PanBy, ViewportController.PanBy | PASS |
| Pinch zoom ve odak noktası koruma (focal preservation) | Camera2D.ZoomAt, ViewportController.PinchZoomAt | PASS |
| Double-tap hızlı zoom / reset | ViewportController.HandleDoubleTap | PASS |
| Fit-extents (sahne sınırlarına sığdırma) | Camera2D.FitExtents, ViewportController.FitExtents | PASS |
| Oryantasyon / pencere boyutu değişimi (No CAD reparse) | Camera2D.Resize, ViewportController.ResizeViewport | PASS |
| Dünya/ekran koordinat double hassasiyeti | Camera2D double matris dönüşümleri | PASS |
| SkiaSharp kamera entegrasyonu | SkiaCadRenderer.RenderCameraWithStatsAsync | PASS |
| Host testleri | Stage11ViewportGestureTests, Rendering & Architecture testleri | PASS |
| Gerçek Android app derleme & paketleme | MobilDwg.App net10.0-android36.0 Release APK (A11Validation=true) | PASS |
| Gerçek Android API 36 emülatör jest kabulü | scripts/a11-android-gesture-gate.ps1 | PASS |
| Byte-safe PNG ekran görüntüsü | a11-real-app-gesture.png (136,477 byte) | PASS |
| Bellek, liveness, ANR/Crash denetimi | PID 7085, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- STAGE11_VIEWPORT_GESTURE_TESTS_PASS:
  - Camera Pan Test: PASS
  - Camera Pinch Zoom Focal Point Preservation Test: PASS
  - ViewportController Double Tap Test: PASS
  - Fit Extents Test: PASS
  - Orientation Resize No Reparse Test: PASS
  - ViewportController Telemetry Test: PASS
- STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS
- STAGE10_TESSELLATION_PRECISION_TESTS_PASS
- STAGE10_P0_SEMANTIC_GOLDEN_PASS
- STAGE10_CONTROLLED_INVALID_GEOMETRY_WARNING_PASS
- STAGE10_SKIA_EXPECTED_CONTENT_HOST_PASS
- STAGE04_RENDER_CONTRACT_TESTS_PASS
- STAGE09_RENDER_SCENE_TESTS_PASS
- STAGE04_ARCHITECTURE_TESTS_PASS
- STAGE05_DEPENDENCY_BOUNDARY_PASS
- V04_REAL_ANDROID_APP_PROJECT_PASS

### 2. Android API 36 Emülatör Jest Kabulü
- Cihaz: sdk_gphone64_x86_64 / Android 16 (API 36) / x86_64 (Seri: emulator-5554)
- Paket: com.smitelagwar.mobildwg
- Başlatıcı Aktivite: com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity
- Canlı PID: 7085
- Release APK Boyutu: 39,249,942 byte
- Release APK SHA-256: ea25e618408134e8e0a417864f6e7c2d7291a9228194b9b1ff0f59e27c9cc644
- Ekran Görüntüsü Boyutu: 136,477 byte
- Ekran Görüntüsü SHA-256: 80404ed972891c348a56d43db84937b5638480496c9c9809ab23468823fb2d00
- Logcat Belirteçleri:
  - A11_ANDROID_PAN_PASS
  - A11_ANDROID_FOCAL_PRESERVATION_PASS
  - A11_ANDROID_PINCH_ZOOM_PASS
  - A11_ANDROID_DOUBLE_TAP_PASS
  - A11_ANDROID_FIT_EXTENTS_PASS
  - A11_ANDROID_ORIENTATION_RESIZE_PASS
  - A11_ANDROID_PNG_PASS
  - ANDROID_STAGE11_VIEWPORT_GESTURE_PASS
  - A11_REAL_APP_UI_IMAGE_READY
  - CLAIM_LIMIT=A11_VIEWPORT_GESTURE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY

## Sınır ve İddia Kısıtı (Claim Limit)

```text
CLAIM_LIMIT=A11_VIEWPORT_GESTURE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
```

AŞAMA 11 mobil viewport ve jest sisteminin (pan, focal preserving pinch zoom, double tap, fit extents, orientation resize) sentetik RenderScene üzerinde çalıştığını ve Android API 36 emülatör üzerinde MAUI arayüzünde doğru gösterimini kanıtlar. Bu aşama DWG/DXF parser nesnelerinin doğrudan RenderScene'e haritalanmasını (AŞAMA 12–16) veya fiziksel cihaz dokunmatik gecikme testlerini kapsamaz.

AŞAMA 12 (DWG R13–R2018 Parser Çekirdeği) A11'in maine merge edilmesiyle açılmaya hazırdır.
