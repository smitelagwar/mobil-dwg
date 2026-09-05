# AŞAMA 16 Kanıtı — Model / Layout / Paper-Space / Viewport (Model Alanı, Pafta Düzeni, Kağıt Alanı ve Görünüm Pencereleri)

## Durum

`DONE`

AŞAMA 16 çıkış kriterleri platform-neutral C# unit testleri (12/12 PASS) ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama AutoCAD Model Space ve Paper Space (Layout) ayrımını, pafta başlığı ve sınır çerçevesini (`CadLayoutDefinition`), çoklu görünüm pencerelerini (`CadLayoutViewport`), Model -> Kağıt Alanı çift duyarlıklı matris dönüşümünü (merkez ötelemesi, `-TwistAngleRadians` ile rotasyon, `PaperHeight / ViewHeight` ölçekleme), görünüm penceresi bazında katman dondurma geçersiz kılmalarını (`FrozenLayers`), Skia kırpma sınırlarını (`ClipRect` / `ClipPath` ile `ViewportPrimitive`), dejenere viewport korumalarını (`INVALID_VIEWPORT_GEOMETRY`), sıfır-reparse (zero-reparse) bellek içi pafta geçişini (`CadLayoutManager`), ve deterministik sahne envanterini (`LayoutSceneSemanticSnapshot`) kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `4b3f263` (PR #28 ve A15 dokümantasyon tamamlama sonrası).
- Branch: `stage16-model-layout-viewport` (PR #29, merge commit: `b978b84`).
- **CadLayoutDefinition & CadLayoutViewport Veri Modelleri (`MobilDwg.Rendering.Layouts`)**:
  - `CadLayoutViewport`: `ViewportId`, `PaperCenter`, `PaperWidth`, `PaperHeight`, `ViewCenter`, `ViewHeight`, `TwistAngleRadians`, `FrozenLayers`, `ClipBoundary`, `IsActive`, ve `PaperBounds`.
  - `CadLayoutDefinition`: `Name`, `IsModelSpace`, `TabOrder`, `PaperBounds`, `PaperEntities` (antet, başlık bloğu, pafta çerçevesi vb.), ve `Viewports`.
- **Sıfır-Reparse (Zero-Reparse) Pafta Yöneticisi (`CadLayoutManager`)**:
  - Model Space ayrıştırması (`_modelSpaceScene`) bellekte sabit tutulur. Paftalar arası geçiş (`SwitchLayout("Sheet-1")`) disk erişimi veya ACadSharp/CAD dosya yeniden okuması yapmaz.
  - Model Space aktifken (`IsModelSpace == true`): Doğrudan model varlıkları döndürülür.
  - Paper Space aktifken (`IsModelSpace == false`):
    1. Pafta kağıt varlıkları (antet, çerçeve vb.) doğrudan eklenir.
    2. Her bir görünüm penceresi için:
       - Dejenere geometri koruması çalışır (NaN/sonsuz koordinat veya $\le 0$ boyutlar durumunda `INVALID_VIEWPORT_GEOMETRY` uyarısı üretilip viewport atlanır).
       - Modelden Kağıt Alanına dönüşüm matrisi oluşturulur:
         $$ec{P}_{paper} = T(	ext{PaperCenter}) \cdot S(scale) \cdot R(-	heta) \cdot T(-	ext{ViewCenter}) \cdot ec{P}_{model}$$
       - Viewport katman dondurma filtresi uygulanır (`FrozenLayers` içindeki model katmanları bu viewport içinde atlanır).
       - Geometriler dönüştürülüp `ViewportPrimitive` içine paketlenir.
       - Kağıt üzerinde görünüm penceresi çerçeve çizgisi (`VIEWPORT_BORDER`) eklenir.
- **SkiaSharp Viewport Render Entegrasyonu (`SkiaCadRenderer`)**:
  - `ViewportPrimitive` için `DrawViewportPrimitive`:
    - Çokgen kırpma sınırı (`ClipBoundary`) varsa `SKPathBuilder` ile `ClipPath` uygulanır.
    - Dikdörtgen sınırlar için `canvas.ClipRect(clipRect, SKClipOperation.Intersect, antialias: true)` uygulanır.
    - Viewport içindeki iç geometriler kırpma alanı içinde güvenle çizilir ve `canvas.RestoreToCount` ile pafta tuvali eski durumuna getirilir.
- **Deterministik Sahne Envanteri (`LayoutSceneSemanticSnapshot`)**:
  - `schema=layout-scene/v1` formatı ile aktif pafta adı, pafta sayısı, sekme sıralaması, kağıt sınırları, görünüm pencereleri (merkez, boyut, bakış merkezi, twist açısı, dondurulmuş katmanlar) ve nihai sahne varlıkları deterministik sıralamayla doğrulanır.

## AŞAMA 16 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| Model Space Doğrudan Sahne | CadLayoutManager Model layout -> model entities directly | PASS |
| Paper Space Antet & Çerçeve | CadLayoutDefinition.PaperEntities (Title block & border) | PASS |
| Viewport Model -> Kağıt Dönüşümü | CadLayoutManager matrix: translation, twist rotation, scale | PASS |
| Viewport Katman Geçersiz Kılma | CadLayoutViewport.FrozenLayers -> layer excluded from VP | PASS |
| Viewport Twist Açısı (Rotation) | CadLayoutManager -TwistAngleRadians transform | PASS |
| Viewport Skia Kırpma (Clipping) | SkiaCadRenderer DrawViewportPrimitive (ClipRect / ClipPath) | PASS |
| Dejenere Viewport Koruması (0 boyut) | paperWidth <= 0 -> INVALID_VIEWPORT_GEOMETRY diagnostic | PASS |
| Dejenere Viewport Koruması (NaN) | viewHeight NaN -> INVALID_VIEWPORT_GEOMETRY diagnostic | PASS |
| Sıfır-Reparse Pafta Geçişi | SwitchLayout memory-only composition, zero disk re-read | PASS |
| Tek Paftada Çoklu Viewport | Overview + Detail viewports on single sheet layout | PASS |
| Deterministik Snapshot | LayoutSceneSemanticSnapshot (schema=layout-scene/v1) | PASS |
| Host Testleri (Release) | Stage16LayoutViewportTests (12/12 test) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A16Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a16-android-layout-viewport-gate.ps1 | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a16-real-app-layout.png (110,781 byte) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 9804, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- `STAGE16_LAYOUT_VIEWPORT_TESTS_PASS`:
  - Model Space Layout Returns Direct Model Entities: PASS
  - Paper Space Layout Renders Title Block And Border: PASS
  - Viewport Model To Paper Transform: PASS
  - Viewport Layer Override Hides Frozen Layers: PASS
  - Viewport Twist Angle Rotates Model Geometry: PASS
  - Viewport Clipping Applied In Skia: PASS
  - Degenerate Viewport Zero Dimensions Emits Diagnostic: PASS
  - Degenerate Viewport NaN Coordinates Emits Diagnostic: PASS
  - Zero Reparse Layout Switching: PASS
  - Multiple Viewports On Single Sheet: PASS
  - Skia Render Paper Layout With Viewports Produces Pixels: PASS
  - Layout Scene Semantic Snapshot Determinism: PASS
- `STAGE15_DIMENSION_HATCH_TESTS_PASS`
- `STAGE14_TEXT_FONT_TESTS_PASS`
- `STAGE13_LAYER_STYLE_TESTS_PASS`
- `STAGE12_BLOCK_INSERT_TESTS_PASS`
- `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`
- `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`
- `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`
- `STAGE10_P0_SEMANTIC_GOLDEN_PASS`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`

### 2. Android API 36 Emülatör Pafta & Viewport Kabulü
- Cihaz: `sdk_gphone64_x86_64` / Android 16 (API 36) / x86_64 (Seri: `emulator-5554`)
- Paket: `com.smitelagwar.mobildwg`
- Başlatıcı Aktivite: `com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
- Canlı PID: `9804`
- Release APK Boyutu: `39,348,246` byte
- Release APK SHA-256: `54ff0923b025a34b6cdfb553350b5f857e2a589f74379e2899aa8d4e214235e5`
- Ekran Görüntüsü Boyutu: `110,781` byte
- Ekran Görüntüsü SHA-256: `44766684bf7f285a6edd2eff29fcf36b372004dc43abb36f2d87fcf9078e4e62`
- Logcat Belirteçleri:
  - `A16_ANDROID_MODEL_SPACE_PASS`
  - `A16_ANDROID_ZERO_REPARSE_PASS`
  - `A16_ANDROID_LAYER_OVERRIDE_PASS`
  - `A16_ANDROID_DEGENERATE_GUARD_PASS`
  - `A16_LAYOUT_ACTIVE=Sheet-A101`
  - `A16_PAPER_ENTITIES_COUNT=5`
  - `A16_RENDER_PIXELS=47573`
  - `A16_SNAPSHOT_HASH=d0d21650b2849d413e174b29bbec47867201e51b184f4f7fa873763c3293883a`
  - `A16_ANDROID_SKIA_RENDER_PASS bytes=15273 sha256=17df0e10b1a03f47c3e38708ba1c3f5d5e53316d29944a9544cce8f6f0cff4cb`
  - `ANDROID_STAGE16_LAYOUT_VIEWPORT_PASS`
  - `A16_REAL_APP_UI_IMAGE_READY sha256=17df0e10b1a03f47c3e38708ba1c3f5d5e53316d29944a9544cce8f6f0cff4cb`
  - `A16_REAL_APP_UI_STATUS_PASS`
  - `A16_REAL_APP_STABILITY_PASS pid=9804`
  - `CLAIM_LIMIT=A16_LAYOUT_VIEWPORT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`

## Sınır ve İddia Kısıtı (Claim Limit)

```text
CLAIM_LIMIT=A16_LAYOUT_VIEWPORT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
```

AŞAMA 16; Model ve Pafta (Paper-Space) ayrımını, pafta başlığı ve sınır çerçevesi çizimini, çoklu görünüm pencerelerinin Modelden Kağıda dönüşüm matrisini (ölçek, bakış merkezi, twist açısı), viewport katman geçersiz kılmalarını (`FrozenLayers`), Skia kırpma sınırlarını (`ClipRect` / `ClipPath`), dejenere viewport korumalarını (`INVALID_VIEWPORT_GEOMETRY`), sıfır-reparse bellek içi pafta geçişini ve SkiaSharp render çıktısının Android API 36 emülatör üzerinde doğrulanmasını kapsar. DWG/DXF dosya ayrıştırmasından gelen ham pafta ve viewport tanımlarının sahneye bağlanması ve render performans optimizasyonları (quadtree/R-tree mekansal indeksleme, LOD/seviye detaylandırma) sonraki aşamalardadır.

AŞAMA 17 (Büyük Dosya Streaming & Performans / Mekansal İndeksleme / LOD) A16'nın main'e merge edilmesiyle açılmaya hazırdır.
