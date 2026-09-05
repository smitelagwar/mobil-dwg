# AŞAMA 10 Kanıtı — P0 Temel Geometri Renderer'ı

## Durum

`DONE`

AŞAMA 10 çıkış kriterleri platform-neutral C# unit/snapshot testleri ve gerçek `MobilDwg.App` API 36 Android Emulator render kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama P0 geometri çizim temelini kapatır; CAD parse-to-scene ve fiziksel cihaz doğrulaması sonraki aşamalardadır.

## Kapsam ve Kararlar

- Base `main` HEAD: `3ebf8226b8f133255e65cafdec9f7f26fbe7afbe`.
- Branch: `stage10-p0-geometry-draft`.
- PR: `#23` — `feat(a10): implement P0 geometry renderer and Android render acceptance`.
- Platform-neutral P0 geometri primitifleri ve deterministik tessellation:
  - `LINE`, `ARC`, `CIRCLE`, `ELLIPSE`, `POINT`
  - `LWPOLYLINE` / `POLYLINE` (+ bulge yay segmentleri)
  - `SPLINE` (de Boor / B-spline örnekleme)
  - `SOLID`, `TRACE`, `3DFACE` (2D düzlem çokgen temsili)
- Dünya koordinatları `double` hassasiyetinde tutuldu; Skia ekran sınırına kadar `float` dönüşümü yapılmadı.
- Immutable geometri yapıları `RenderSceneEntity`'ye eklendi.
- CAD çizim sırası `SourceIndex` üzerinden deterministik olarak korundu.
- `SkiaCadRenderer`: Kırpma (clipping) ve antialiasing ile SkiaSharp tabanlı renderer sağlandı.
- Deterministik `p0-geometry/v1` anlamsal snapshot ve golden testi eklendi.
- Kontrollü geçersiz geometri tanı uyarıları (`GeometryDiscarded`, `GeometryDegenerated`) doğrulandı.
- Gerçek `MobilDwg.App` API 36 üzerinde render yolu: `RenderScene -> SkiaCadRenderer -> PNG -> MAUI Image`.
- Beklenen piksel sayısı (expected-content pixel probe) kanıtı alındı.

## AŞAMA 10 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| LINE / ARC / CIRCLE / ELLIPSE / POINT primitifleri | GeometryPrimitives.cs, Stage10GeometryTests.cs | PASS |
| LWPOLYLINE / POLYLINE bulge tessellation | GeometryTessellator.cs, TessellatePolyline | PASS |
| SPLINE deterministik örnekleme | GeometryTessellator.cs, TessellateSpline | PASS |
| SOLID / TRACE / 3DFACE 2D çokgen | PolygonGeometry, GeometryTessellator.cs | PASS |
| World/document double hassasiyeti | WorldPoint2, WorldBounds2 double pipeline | PASS |
| SkiaSharp çizici entegrasyonu | SkiaCadRenderer.cs, clipping & antialias | PASS |
| Çizim sırası (SourceIndex) | RenderScene.Entities sıralı çizim | PASS |
| Deterministik semantic snapshot | P0GeometrySemanticSnapshot.cs, format p0-geometry/v1 | PASS |
| Geçersiz geometri kontrolleri | STAGE10_CONTROLLED_INVALID_GEOMETRY_WARNING_PASS | PASS |
| Host testleri | 10-host-validation, Rendering & Architecture testleri | PASS |
| Gerçek Android app derleme & paketleme | MobilDwg.App net10.0-android36.0 Release APK (A10Validation=true) | PASS |
| Gerçek Android API 36 emülatör render | scripts/a10-android-render-gate.ps1 | PASS |
| Beklenen içerik piksel kanıtı | 56,163 piksel (A10_ANDROID_EXPECTED_CONTENT_PASS) | PASS |
| Byte-safe PNG ekran görüntüsü | 10-real-app-render.png (133,801 byte) | PASS |
| Bellek, liveness, ANR/Crash denetimi | PID 6257, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
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

### 2. Android API 36 Emülatör Render Kabulü
- Cihaz: sdk_gphone64_x86_64 / Android 16 (API 36) / x86_64 (Seri: emulator-5554)
- Paket: com.smitelagwar.mobildwg
- Başlatıcı Aktivite: com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity
- Canlı PID: 6257
- Release APK Boyutu: 39,543,728 byte
- Release APK SHA-256: ec35abf74dcefaaa70a29845d32b1791ff3a8160ecb7aad99bcab6c012a89b70
- Beklenen İçerik Piksel Sayısı: 56,163 piksel
- Ekran Görüntüsü Boyutu: 133,801 byte
- Ekran Görüntüsü SHA-256: 52b14a1e622526163b0ed0e927b7ec0e0a97c9385dc2635d911949c2e1b6ea50
- Logcat Belirteçleri:
  - A10_ANDROID_SEMANTIC_GOLDEN_PASS
  - A10_ANDROID_EXPECTED_CONTENT_PASS pixels=56163
  - A10_ANDROID_PNG_PASS bytes=37504 sha256=8bee037b017adae0380daf45fa4558a238ec45a0c90e0c6d4de43df599d649df
  - ANDROID_STAGE10_P0_GEOMETRY_RENDER_PASS
  - CLAIM_LIMIT=P0_SYNTHETIC_SCENE_GEOMETRY_RENDERER_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
  - A10_REAL_APP_UI_IMAGE_READY sha256=8bee037b017adae0380daf45fa4558a238ec45a0c90e0c6d4de43df599d649df

## Sınır ve İddia Kısıtı (Claim Limit)

`	ext
CLAIM_LIMIT=P0_SYNTHETIC_SCENE_GEOMETRY_RENDERER_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
`

AŞAMA 10 geometri renderer motorunun sentetik RenderScene verisi üzerinde deterministik çizimini ve Android emülatörde MAUI arayüzünde doğru gösterimini kanıtlar. Bu aşama DWG/DXF parser nesnelerinin doğrudan RenderScene'e haritalanmasını (AŞAMA 12–16) veya fiziksel cihaz performans testlerini kapsamaz.

AŞAMA 11 (Mobil viewport ve pan/pinch zoom jestleri) A10'un maine merge edilmesiyle açılmaya hazırdır.
