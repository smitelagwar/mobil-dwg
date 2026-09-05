# AŞAMA 12 Kanıtı — Block / INSERT / Attribute (Blok Tanımları, Referanslar, Matris Dönüşümleri, Layer 0 / ByBlock Mirası ve Nitelikler)

## Durum

`DONE`

AŞAMA 12 çıkış kriterleri platform-neutral C# unit testleri ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama 2D afin dönüşüm matrisleri (`Transform2D`), geometrik ilkel dönüşüm motoru (`PrimitiveTransformer`), blok tanımları (`BlockDefinition`), blok referansları (`BlockReference`), nitelikler (`BlockAttribute`) ve döngü/derinlik/bütçe korumalı blok genişletme motorunu (`BlockExpander`) kapatır; tam DWG/DXF dosya parse-to-scene entegrasyonu ve fiziksel cihaz doğrulaması sonraki aşamalardadır.

## Kapsam ve Kararlar

- Base `main` HEAD: `62104933af8ca1066c9870dce6099c8732f65fb7`.
- Branch: `stage12-block-insert`.
- PR: `#25` — `feat(a12): implement block, insert, and attribute expansion`.
- `Transform2D` 2D afin dönüşüm matrisi:
  - Double hassasiyetli matris çarpımı ($T_{parent} \times T_{child}$), ölçekleme, öteleme, döndürme ve aynalama (mirror) işlemleri.
  - Vektör ve nokta dönüşümleri, matris tersi (`Invert()`) ve determinant hesabı.
- `PrimitiveTransformer` ilkel dönüşüm motoru:
  - `Point`, `Line`, `Arc`, `Ellipse`, `Polyline`, `Spline`, `Polygon` dönüşümleri.
  - Uniform olmayan ölçeklemede (`scaleX != scaleY`) çember/yayların elips/eliptik yaya dönüşmesi.
  - Aynalama ($s_x < 0 \oplus s_y < 0$) durumunda yay ve eliptik yayların yön ve açı dönüşüm düzeltmesi (`CounterClockwise` / sweep yön değişimi).
- `BlockDefinition`, `BlockReference`, `BlockAttribute` modelleri:
  - Blok adı, taban noktası (`BasePoint`), şablon varlıkları ve iç içe blok referansları.
  - Ekleme noktası, ölçek vektörü ($s_x, s_y$), dönme açısı, katman ve ByBlock renk/stil özellikleri.
  - Tag, text, position, height, rotation ve gizlilik (`IsInvisible`) nitelik desteği.
- `BlockExpander` blok genişletme motoru:
  - İç içe blokların hiyerarşik genişletilmesi ve matris kompozisyonu ($T_{acc} \times T_{ref}$).
  - Katman 0 kuralı (Layer 0 inheritance): Şablon varlığı `"0"` katmanındaysa referansın katmanını miras alır, aksi halde kendi katmanını korur.
  - ByBlock mirası: Varlık stili `ByBlock` ise referansın rengini ve çizgi tipini miras alır.
  - Görünür niteliklerin (`ATTRIB`) genişletilmiş sahneye metin/geometri olarak dahil edilmesi.
  - Üçlü koruma muhafızları (Guards):
    1. Döngüsel referans muhafızı (`BLOCK_CYCLE_DETECTED`).
    2. Maksimum özyineleme derinliği muhafızı (`BLOCK_DEPTH_EXCEEDED`, varsayılan 32).
    3. Maksimum varlık genişletme bütçesi muhafızı (`BLOCK_EXPANSION_BUDGET_EXCEEDED`, varsayılan 100,000).
- Gerçek `MobilDwg.App` API 36 üzerinde blok kabul yolu:
  - Sentetik iç içe blok hiyerarşisi oluşturulur, genişletilir ve 5 temel invariant doğrulanır.
  - Genişletilen sahne SkiaSharp ile 1024x768 PNG olarak render edilir ve MAUI UI'a bağlanır.
  - Logcat belirteçleri ve ekran görüntüsü kanıtları kaydedilir.

## AŞAMA 12 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| 2D Afin dönüşüm matrisi (Transform2D) | Double-precision afin matris, Invert, Multiply | PASS |
| Geometrik ilkel dönüşüm (PrimitiveTransformer) | Çember/yay, non-uniform scale -> Elips, Mirror flip | PASS |
| Blok modeli (BlockDefinition, Reference, Attribute) | Taban noktası, referans ölçek/rotasyon/öteleme, ATTRIB | PASS |
| Blok genişletme motoru (BlockExpander) | Hiyerarşik matris kompozisyonu ve varlık düzleştirme | PASS |
| Katman 0 Miras Kuralı (Layer 0 Inheritance) | Template layer "0" -> ref layer mirası; diğerleri korunur | PASS |
| ByBlock Miras Kuralı (ByBlock Inheritance) | Template ByBlock color/style -> ref color/style mirası | PASS |
| ATTRIB Nitelik Görünürlüğü | Görünür niteliklerin sahneye aktarımı, gizlilerin filtrelenmesi | PASS |
| Döngüsel Referans Koruması | Blok döngülerinde BLOCK_CYCLE_DETECTED fırlatılması | PASS |
| Derinlik ve Bütçe Koruması | MaxDepth ve MaxEntities aşıldığında güvenli fırlatma | PASS |
| Deterministik Blok Sahnesi Envanteri | BlockSceneSemanticSnapshot formatı `block-scene/v1` | PASS |
| Host Testleri (Release) | Stage12BlockInsertTests (11/11 test) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A12Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a12-android-block-gate.ps1 | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a12-real-app-block.png (96,079 byte) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 7926, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- STAGE12_BLOCK_INSERT_TESTS_PASS:
  - Transform2D Matrix Operations Test: PASS
  - PrimitiveTransformer NonUniform Scale Test: PASS
  - PrimitiveTransformer Mirror Test: PASS
  - Nested Block Hierarchy Expansion Test: PASS
  - Layer 0 Inheritance Test: PASS
  - ByBlock Style Inheritance Test: PASS
  - Attribute Inclusion and Filtering Test: PASS
  - Circular Reference Guard Test: PASS
  - Recursion Depth Guard Test: PASS
  - Expansion Budget Guard Test: PASS
  - Block Semantic Snapshot Golden Test: PASS
- STAGE11_VIEWPORT_GESTURE_TESTS_PASS
- STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS
- STAGE10_TESSELLATION_PRECISION_TESTS_PASS
- STAGE10_P0_SEMANTIC_GOLDEN_PASS
- STAGE04_RENDER_CONTRACT_TESTS_PASS
- STAGE09_RENDER_SCENE_TESTS_PASS
- STAGE04_ARCHITECTURE_TESTS_PASS
- STAGE05_DEPENDENCY_BOUNDARY_PASS
- V04_REAL_ANDROID_APP_PROJECT_PASS

### 2. Android API 36 Emülatör Blok Kabulü
- Cihaz: sdk_gphone64_x86_64 / Android 16 (API 36) / x86_64 (Seri: emulator-5554)
- Paket: com.smitelagwar.mobildwg
- Başlatıcı Aktivite: com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity
- Canlı PID: 7926
- Release APK Boyutu: 39,270,422 byte
- Release APK SHA-256: b9acd8f1de0d847b2ac5a6492d587b0594782d77fab9fbd1c6c0bc2dafd8c155
- Ekran Görüntüsü Boyutu: 96,079 byte
- Ekran Görüntüsü SHA-256: 7ef1abd77ce9f0d052775e9aeb79cea81bae2eba078205ba41cc2ecc0b9761b5
- Logcat Belirteçleri:
  - A12_ANDROID_NESTED_TRANSFORM_PASS
  - A12_ANDROID_NON_UNIFORM_SCALE_MIRROR_PASS
  - A12_ANDROID_LAYER0_BYBLOCK_INHERITANCE_PASS
  - A12_ANDROID_ATTRIB_PASS
  - A12_ANDROID_CYCLE_DEPTH_BUDGET_GUARDS_PASS
  - A12_ANDROID_PNG_PASS
  - ANDROID_STAGE12_BLOCK_INSERT_PASS
  - A12_REAL_APP_UI_IMAGE_READY
  - A12_REAL_APP_UI_STATUS_PASS
  - A12_REAL_APP_STABILITY_PASS
  - CLAIM_LIMIT=A12_BLOCK_INSERT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY

## Sınır ve İddia Kısıtı (Claim Limit)

```text
CLAIM_LIMIT=A12_BLOCK_INSERT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
```

AŞAMA 12 blok tanımlama, INSERT referansları, afin dönüşüm, Layer 0/ByBlock mirası, ATTRIB nitelikleri ve koruma mekanizmalarının sentetik RenderScene üzerinde çalıştığını ve Android API 36 emülatör üzerinde MAUI arayüzünde doğru gösterimini kanıtlar. Bu aşama DWG/DXF parser blok nesnelerinin doğrudan RenderScene'e haritalanmasını (AŞAMA 13–16) veya fiziksel cihaz dokunmatik gecikme testlerini kapsamaz.

AŞAMA 13 (Layer, Color, Linetype, Lineweight) A12'nin main'e merge edilmesiyle açılmaya hazırdır.
