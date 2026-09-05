# AŞAMA 13 Kanıtı — Layer / Color / Linetype / Lineweight (Katman Yönetimi, ACI / TrueColor, ByLayer / ByBlock, Çizgi Tipleri, Çizgi Kalınlıkları ve Merkezi Stil Çözümleyici)

## Durum

`DONE`

AŞAMA 13 çıkış kriterleri platform-neutral C# unit testleri ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama katman durum yönetimini (`LayerTable`, `LayerDefinition`), ACI 1–255 ve TrueColor renk çözümlemesini (`CadColor`), standart çizgi tipi desenlerini ve karmaşık çizgi tipi geri çekilmesini (`CadLinetype`), milimetrik çizgi kalınlığı dönüşümünü (`CadLineweight`), merkezi stil çözümleyiciyi (`CadStyleResolver`) ve SkiaSharp render entegrasyonunu (`SkiaCadRenderer`) kapatır; tam DWG/DXF dosya parse-to-scene entegrasyonu ve fiziksel cihaz doğrulaması sonraki aşamalardadır.

## Kapsam ve Kararlar

- Base `main` HEAD: `9861b0d729ba28dc8dbfb813be00b0caf31ca8d6`.
- Branch: `stage13-layer-style`.
- PR: `#26` — `feat(a13): implement layer, color, linetype, and lineweight styling`.
- `CadColor` renk modeli:
  - 256'lık standart AutoCAD ACI renk tablosu.
  - 24-bit TrueColor RGB doğrudan renk tanımlama.
  - `ByLayer` ve `ByBlock` dinamik renk mirası.
  - ACI 7 (White/Black) dinamik arka plan kontrast uyarlaması: Koyu temada beyaz (`0xFFFFFFFF`), açık temada siyah (`0xFF000000`).
- `CadLinetype` çizgi tipi modeli:
  - Sürekli çizgi (`Continuous`), `ByLayer` ve `ByBlock` linetype desteği.
  - Standart AutoCAD kesikli çizgi desenleri (`Dashed`, `Hidden`, `Center`, `Dot`, `DashDot`, `Phantom`).
  - Çizim birimlerindeki desen uzunluklarının ekran pikseline ölçeklenmesi (`LinetypeScale` / `worldUnitsPerPixel`).
  - Karmaşık (metinli veya şekilli) çizgi tiplerinde sistem çökmesi olmaksızın denetimli uyarı (`COMPLEX_LINETYPE_FALLBACK`) ile sürekli/temel desene geri çekilme.
- `CadLineweight` çizgi kalınlığı modeli:
  - Standart ISO milimetre değerleri (0.00mm - 2.11mm).
  - Ekran DPI ve cihaz yoğunluğuna (`density`) göre milimetreden piksele dinamik dönüşüm.
  - `displayLineweights = false` durumunda hairline (1 piksel) modu.
- `LayerDefinition` & `LayerTable` katman tablosu:
  - Katman özellikleri: Ad, Görünürlük (`IsVisible`), Dondurulma (`IsFrozen`), Kilit (`IsLocked`), Renk, Çizgi Tipi, Çizgi Kalınlığı.
  - Katman 0 her zaman varsayılan olarak mevcuttur.
  - Çalışma zamanında katman açma/kapama (`SetLayerVisibility`) ve dondurma/çözme (`SetLayerFrozen`).
  - `IsRenderable = IsVisible && !IsFrozen` kuralı.
- `CadStyleResolver` merkezi stil çözümleyici:
  - Varlık stili, katman tablosu, blok referansı bağlamı ve renk bağlamını birleştirerek nihai çizim stilini (`ResolvedStyle`) üretir.
  - Bilinmeyen/bulunmayan katmanlarda denetimli `UNKNOWN_LAYER_FALLBACK` uyarısı vererek Katman 0'a yönlendirir.
- `SkiaCadRenderer` entegrasyonu:
  - Render döngüsünde görünmeyen veya dondurulmuş katmanlardaki varlıkları otomatik olarak atlar.
  - Çözümlenen renk ve çizgi kalınlığını `SKPaint` nesnesine uygular.
  - Kesikli çizgi desenlerini `SKPathEffect.CreateDash` ile Skia yoluna uygular.
- Deterministik format `layer-style/v1` (`LayerStyleSemanticSnapshot`).

## AŞAMA 13 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| ACI 1-255 Renk Tablosu | CadColor.FromAci, ACI lookup table | PASS |
| ACI 7 Dinamik Arka Plan Kontrastı | Koyu temada beyaz (#FFF), açık temada siyah (#000) | PASS |
| TrueColor 24-bit RGB Desteği | CadColor.FromRgb, FromArgb | PASS |
| ByLayer Renk, Linetype, Lineweight Mirası | CadStyleResolver ByLayer miras zinciri | PASS |
| ByBlock Renk, Linetype, Lineweight Mirası | CadStyleResolver blok bağlam miras zinciri | PASS |
| Katman Durum Yönetimi (LayerTable) | AddOrUpdate, TryGetLayer, Katman 0 garantisi | PASS |
| Katman Görünürlük ve Dondurma Kontrolü | IsVisible, IsFrozen -> IsRenderable filtreleme | PASS |
| Standart Kesikli Çizgi Desenleri | Dashed, Hidden, Center, Dot, DashDot, Phantom | PASS |
| Karmaşık Çizgi Tipi Uyarı ve Geri Çekilme | COMPLEX_LINETYPE_FALLBACK SceneDiagnostic | PASS |
| Milimetrik Çizgi Kalınlığı Dönüşümü | mm -> piksel dönüşümü, hairline modu | PASS |
| SkiaSharp Render Entegrasyonu | SkiaCadRenderer stil ve çizgi efekti uygulama | PASS |
| Deterministik Katman/Stil Envanteri | LayerStyleSemanticSnapshot formatı layer-style/v1 | PASS |
| Host Testleri (Release) | Stage13LayerStyleTests (12/12 test) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A13Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a13-android-style-gate.ps1 | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a13-real-app-style.png (86,413 byte) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 8595, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- STAGE13_LAYER_STYLE_TESTS_PASS:
  - ACI Color Palette Test: PASS
  - ACI 7 Dynamic Contrast Inversion Test: PASS
  - TrueColor Resolution Test: PASS
  - ByLayer Resolution Test: PASS
  - ByBlock Resolution Test: PASS
  - Layer Visibility Toggle Test: PASS
  - Layer Freeze Toggle Test: PASS
  - Standard Linetypes Test: PASS
  - Complex Linetype Fallback Test: PASS
  - Lineweight Pixel Conversion Test: PASS
  - Unknown Layer Fallback Test: PASS
  - Layer Style Semantic Snapshot Golden Test: PASS
- STAGE12_BLOCK_INSERT_TESTS_PASS
- STAGE11_VIEWPORT_GESTURE_TESTS_PASS
- STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS
- STAGE10_TESSELLATION_PRECISION_TESTS_PASS
- STAGE10_P0_SEMANTIC_GOLDEN_PASS
- STAGE04_RENDER_CONTRACT_TESTS_PASS
- STAGE09_RENDER_SCENE_TESTS_PASS
- STAGE04_ARCHITECTURE_TESTS_PASS
- STAGE05_DEPENDENCY_BOUNDARY_PASS
- V04_REAL_ANDROID_APP_PROJECT_PASS

### 2. Android API 36 Emülatör Stil Kabulü
- Cihaz: sdk_gphone64_x86_64 / Android 16 (API 36) / x86_64 (Seri: emulator-5554)
- Paket: com.smitelagwar.mobildwg
- Başlatıcı Aktivite: com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity
- Canlı PID: 8595
- Release APK Boyutu: 39,282,710 byte
- Release APK SHA-256: 530571d2a269ccb52d8752b73de693deb69dab67b6137309f3845158c0bb0b6c
- Ekran Görüntüsü Boyutu: 86,413 byte
- Ekran Görüntüsü SHA-256: f51bbfa1d5536c5c1ccd61be120831a7bdaaa0171812b6813bc5845774ee79b0
- Logcat Belirteçleri:
  - A13_ANDROID_ACI_TRUECOLOR_PASS
  - A13_ANDROID_BYLAYER_BYBLOCK_PASS
  - A13_ANDROID_LAYER_VISIBILITY_FREEZE_PASS
  - A13_ANDROID_LINETYPE_LINEWEIGHT_PASS
  - A13_ANDROID_COMPLEX_STYLE_WARNING_PASS
  - A13_ANDROID_PNG_PASS bytes=
  - ANDROID_STAGE13_LAYER_STYLE_PASS
  - A13_REAL_APP_UI_IMAGE_READY sha256=
  - A13_REAL_APP_UI_STATUS_PASS
  - A13_REAL_APP_STABILITY_PASS
  - CLAIM_LIMIT=A13_LAYER_STYLE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY

## Sınır ve İddia Kısıtı (Claim Limit)

```text
CLAIM_LIMIT=A13_LAYER_STYLE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
```

AŞAMA 13 katman durum yönetimi, renk çözümleme (ACI/TrueColor/ByLayer/ByBlock), çizgi tipi desenleri, çizgi kalınlığı ölçekleme ve Skia render entegrasyonunun sentetik RenderScene üzerinde çalıştığını ve Android API 36 emülatör üzerinde MAUI arayüzünde doğru gösterimini kanıtlar. Bu aşama DWG/DXF parser nesnelerinin doğrudan RenderScene'e haritalanmasını (AŞAMA 14–16) veya fiziksel cihaz dokunmatik gecikme testlerini kapsamaz.

AŞAMA 14 (TEXT / MTEXT / Türkçe / Font / SHX) A13'ün main'e merge edilmesiyle açılmaya hazırdır.
