# AŞAMA 15 Kanıtı — Dimension / Leader / Hatch (Ölçülendirme, Lider Çizgileri, Tarama/Hatch ve Skia Render)

## Durum

`DONE`

AŞAMA 15 çıkış kriterleri platform-neutral C# unit testleri (13/13 PASS) ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama AutoCAD anonim blok ilkliği kuralını (`*D...` blok genişletmesi), usulsel (procedural) ölçülendirme üretimini (Hizalı/Aligned, Döndürülmüş Doğrusal/Rotated Linear, Yarıçap/Radial, Çap/Diametric), dejenere ölçü korumalarını (`DEGENERATE_DIMENSION_POINTS`, `INVALID_DIMENSION_GEOMETRY`), ok ucu stillerini (`ClosedFilled`, `ArchitecturalTick`), lider/multileader geometrisini (`LeaderBuilder`), tarama sınır döngüsü otomatik kapanma toleransını (≤ 1 mm) ve kırık sınır teşhisini (`HATCH_BROKEN_BOUNDARY`), iç içe ada doldurma mantığını (Skia `SKPathFillType.EvenOdd` ile Normal island style), ANSI31 kırpılmış desen çizgisi üretimini (`HatchProcessor`), ve SkiaSharp render entegrasyonunu (`SkiaCadRenderer`) kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `42125c1` (PR #27 ve A14 dokümantasyon tamamlama sonrası).
- Branch: `stage15-dimension-hatch`.
- **Anonymous Dimension Block Preference (`*D...`)**:
  - DWG dosyalarında ölçülendirmeler çoğu zaman önceden hesaplanmış bir anonim blok (`*D...`) taşır.
  - `blockTable` içinde bu blok mevcut olduğunda, heuristik tahminlere girmeden doğrudan `BlockExpander.Expand` ile blok açılır; bu sayede AutoCAD ile piksel piksel birebir uyum garanti edilir.
- **Usulsel (Procedural) CAD Ölçülendirme Motoru (`DimensionBuilder`)**:
  - Blok bulunmadığında veya bozuk olduğunda usulsel üretim devreye girer:
    - **Aligned (Hizalı)**: Tanım noktaları arasındaki doğrultuda ölçü çizgisi, uzatma çizgileri (`Extension Lines`), taşma payı (`Overhang`), iki uçta ok uçları ve ortalanmış ölçü metni.
    - **Linear / Rotated (Döndürülmüş Doğrusal)**: Rotasyon açısına (theta) göre yön vektörü u ve normal n projeksiyonu; ölçüm değeri eksen boyunca |(P2 - P1) . u| olarak çift duyarlıklı hesaplanır.
    - **Radial (Yarıçap)**: Merkezden çembere tek ok uçlu lider çizgisi ve `"R"` önekli formatlanmış metin.
    - **Diametric (Çap)**: Çap boyunca iki ok uçlu çizgi ve `"Ø"` önekli formatlanmış metin.
- **Dejenere Ölçü Muhafızları (Degenerate Dimension Guards)**:
  - Çakışık tanım noktaları ($dist < 10^{-6}$) durumunda çizimi bozmak yerine kontrollü `DEGENERATE_DIMENSION_POINTS` teşhisi üretilir ve boş geometri döndürülür.
  - NaN veya sonsuz koordinat içeren ölçülerde `INVALID_DIMENSION_GEOMETRY` teşhisi üretilir ve çizim bütünlüğü korunur.
- **Lider & MultiLeader Geometrisi (`LeaderBuilder`)**:
  - Uç noktasında yönlendirilmiş dolu ok ucu (`ClosedFilled`), kırık çizgili lider yolu, yatay dirsek (`Dogleg Landing Line`) ve açıklama metni (`TextPrimitive`).
- **Tarama / Hatch İşleme Motoru (`HatchProcessor`, `HatchPrimitive`)**:
  - **Otomatik Kapanma Toleransı**: Başlangıç ve bitiş noktaları arasındaki boşluk $\le 10^{-3}$ birim (1 mm) ise döngü sessizce kapatılır.
  - **Kırık Sınır Teşhisi**: Boşluk toleransı aştığında ($> 10^{-3}$) kontrollü `HATCH_BROKEN_BOUNDARY` teşhisi üretilir ve döngü güvenli şekilde kapatılır.
  - **İç İçe Ada Doldurma (Nested Islands / EvenOdd)**: Skia'nın `SKPathFillType.EvenOdd` dolgu tipi kullanılarak dış sınır dolu, ada oyuk, ada içindeki alt-ada tekrar dolu (AutoCAD `Normal` ada stili) olarak kusursuz doldurulur.
  - **Desen Çizgisi Üretimi (ANSI31 vb.)**: 45 derece eğimli desen çizgileri sınır kutusu üzerinden taranır, döngü poligonlarına göre kırpılır ve maksimum 2,048 çizgi güvenlik bütçesi ile sınırlandırılır.
- **Skia CAD Render Entegrasyonu (`SkiaCadRenderer`)**:
  - `DrawPrimitive` içinde `HatchPrimitive` yakalanarak `SKPathBuilder` ve `SKPathFillType.EvenOdd` ile solid dolgu veya sınır çizgileri + kırpılmış desen çizgileri yüksek kalitede çizilir.
- **Deterministik Sahne Envanteri (`DimensionHatchSemanticSnapshot`)**:
  - `schema=dim-hatch/v1` formatı ile varlık kimliği, katman, desen tipi, döngü sayısı, sınır kutusu ve metin parametreleri deterministik sıralamayla doğrulanır.

## AŞAMA 15 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| Anonymous Block İlkliği (*D...) | DimensionBuilder.BuildDimension, anonymous block expansion | PASS |
| Aligned Ölçülendirme (Procedural) | DimensionBuilder Aligned, 2 extension, dim line, 2 arrows, text | PASS |
| Linear / Rotated Ölçülendirme | DimensionBuilder Linear, angle projection, measurement calc | PASS |
| Radial Ölçülendirme ("R...") | DimensionBuilder Radial, single arrow, "R" prefix | PASS |
| Diametric Ölçülendirme ("Ø...") | DimensionBuilder Diametric, 2 arrows, "Ø" prefix | PASS |
| Dejenere Nokta Muhafızı | dist < 1e-6 -> DEGENERATE_DIMENSION_POINTS | PASS |
| NaN Koordinat Muhafızı | DimensionBuilder.TryBuildFromRaw -> INVALID_DIMENSION_GEOMETRY | PASS |
| Leader & MultiLeader Geometrisi | LeaderBuilder: tip arrow, path segments, dogleg, text | PASS |
| Hatch 1mm Otomatik Kapanma | HatchProcessor.ValidateAndCloseLoop (gap <= 0.001) | PASS |
| Hatch Kırık Sınır Teşhisi | HatchProcessor (gap > 0.001) -> HATCH_BROKEN_BOUNDARY | PASS |
| EvenOdd İç İçe Ada Doldurma | SKPathFillType.EvenOdd, HatchIslandStyle.Normal | PASS |
| ANSI31 Kırpılmış Desen Çizgileri | HatchProcessor.GeneratePatternLines (45 deg, budget <= 2048) | PASS |
| Deterministik Snapshot | DimensionHatchSemanticSnapshot (schema=dim-hatch/v1) | PASS |
| Host Testleri (Release) | Stage15DimensionHatchTests (13/13 test) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A15Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a15-android-dim-hatch-gate.ps1 | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a15-real-app-dim-hatch.png (111,314 byte) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 9288, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- `STAGE15_DIMENSION_HATCH_TESTS_PASS`:
  - Anonymous Dimension Block Preferred Test: PASS
  - Aligned Dimension Procedural Test: PASS
  - Rotated Linear Dimension Procedural Test: PASS
  - Radial Dimension Procedural Test: PASS
  - Diametric Dimension Procedural Test: PASS
  - Degenerate Dimension Identical Defpoints Test: PASS
  - Degenerate Dimension NaN Coordinates Test: PASS
  - Leader & MultiLeader Test: PASS
  - Hatch Auto Closure Within Tolerance Test: PASS
  - Hatch Broken Boundary Diagnostic Test: PASS
  - Hatch EvenOdd Nested Islands Test: PASS
  - Hatch ANSI31 Pattern Lines Generated Test: PASS
  - Dimension Hatch Semantic Snapshot Determinism Test: PASS
- `STAGE14_TEXT_FONT_TESTS_PASS`
- `STAGE13_LAYER_STYLE_TESTS_PASS`
- `STAGE12_BLOCK_INSERT_TESTS_PASS`
- `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`
- `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`
- `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`
- `STAGE10_P0_SEMANTIC_GOLDEN_PASS`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`

### 2. Android API 36 Emülatör Dimension & Hatch Kabulü
- Cihaz: `sdk_gphone64_x86_64` / Android 16 (API 36) / x86_64 (Seri: `emulator-5554`)
- Paket: `com.smitelagwar.mobildwg`
- Başlatıcı Aktivite: `com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
- Canlı PID: `9288`
- Release APK Boyutu: `39,340,054` byte
- Release APK SHA-256: `9a7531eb8c9b4946ba24374c5d31655cc765a9855cffe21b8bd3109b5f42617b`
- Ekran Görüntüsü Boyutu: `111,314` byte
- Ekran Görüntüsü SHA-256: `bfe4fac1932f7c2168f529afa2bd1454bcbb2873a5c3250986d1b86d1c6c6b4d`
- Logcat Belirteçleri:
  - `A15_ANDROID_ANONYMOUS_BLOCK_PASS`
  - `A15_ANDROID_PROCEDURAL_DIMENSIONS_PASS`
  - `A15_ANDROID_DEGENERATE_GUARDS_PASS`
  - `A15_ANDROID_LEADER_PASS`
  - `A15_ANDROID_HATCH_PROCESSING_PASS`
  - `A15_SCENE_ENTITIES_COUNT=8`
  - `A15_HATCH_ISLAND_EVENODD_VERIFIED loops=2`
  - `A15_ANSI31_PATTERN_LINES_COUNT=19`
  - `A15_RENDER_PIXELS=58280`
  - `A15_SNAPSHOT_HASH=3edb1660f76aaf46a751593fb4bb0d0cf27aa5845267a1f01ddbd222a6a45578`
  - `A15_ANDROID_SKIA_RENDER_PASS bytes=16859 sha256=bc28ab30f1f6ede833aac316a61f00d3a790b90339ea3c63a63dc5de32f3015b`
  - `ANDROID_STAGE15_DIMENSION_HATCH_PASS`
  - `A15_REAL_APP_UI_IMAGE_READY sha256=bc28ab30f1f6ede833aac316a61f00d3a790b90339ea3c63a63dc5de32f3015b`
  - `A15_REAL_APP_UI_STATUS_PASS`
  - `A15_REAL_APP_STABILITY_PASS pid=9288`
  - `CLAIM_LIMIT=A15_DIMENSION_HATCH_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`

## Sınır ve İddia Kısıtı (Claim Limit)

```text
CLAIM_LIMIT=A15_DIMENSION_HATCH_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
```

AŞAMA 15; anonim blok açılımını, usulsel ölçülendirme geometrisini (Aligned, Rotated, Radial, Diametric), dejenere ölçü korumalarını, lider/multileader çizimini, tarama sınır döngüsü otomatik kapanmasını ve kırık sınır teşhisini, EvenOdd ada doldurmayı, ANSI31 desen çizgisi üretimini ve SkiaSharp render çıktısının Android API 36 emülatör üzerinde doğrulanmasını kapsar. DWG/DXF dosya ayrıştırmasından gelen ham varlıkların bu modellerle tam uçtan uca sahneye bağlanması AŞAMA 16'dadır.

AŞAMA 16 (Scene Assembly & Performance Optimization) A15'in main'e merge edilmesiyle açılmaya hazırdır.
