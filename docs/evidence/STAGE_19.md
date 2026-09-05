# AŞAMA 19 Kanıtı — Kötü Niyetli / Bozuk Girdi ve Kaynak Sınır Muhafızları (Resource Guards)

## Durum

`DONE`

AŞAMA 19 çıkış kriterleri platform-neutral C# unit testleri (12/12 PASS), katman mimari testleri (`MobilDwg.Architecture.Tests`) ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama dosya formatı ve sihirli sayı (magic number) ön doğrulamasını (`CadPreflightInspector`), sistem kaynak karmaşıklık sınırlarını (`CadResourceBudget` ve `CadBudgetGuard`), sayısal koordinat ve sonsuzluk/NaN muhafızlarını (`CadSanityGuards`), blok döngüsel özyineleme tespitini (`CadBudgetGuard.CheckBlockDepthAndCycle`), deterministik durum dökümünü (`ResourceGuardsSemanticSnapshot` schema `resource-guards/v1`) ve sınırlı mutasyon/fuzz duman testlerini kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `7d8d3b9` (A18 tamamlanması sonrası).
- Branch: `stage19-resource-guards` (PR #32, merge commit: `9483e7a`).
- **Ön Uçus / Format Muayenesi (`MobilDwg.Core.Guards.CadPreflightInspector`)**:
  - DWG format sihirli sayıları (`AC1015`, `AC1018`, `AC1021`, `AC1024`, `AC1027`, `AC1032`) doğrulanır; geçersiz DWG başlıkları kontrollü `InvalidDwgMagic` koduyla reddedilir.
  - DXF ikili (`AutoCAD Binary DXF\r\n\x1a\x00`) ve ASCII (`SECTION`, `HEADER`, `0\r\nSECTION`) imzaları doğrulanır.
  - Sıfır baytlı veya 6 bayttan kısa kesilmiş dosyalar `EmptyOrTruncated` koduyla tespit edilir.
  - Yabancı / zararlı formatlar (PE/Windows `.exe` `MZ`, ELF/Linux `\x7fELF`, ZIP/JAR `PK\x03\x04`, PDF `%PDF-`, HTML `<html` / `<!DOC`, PNG `\x89PNG`, JPEG `\xff\xd8\xff`, BMP `BM`) erken aşamada güvenle tespit edilerek `ForeignFormat` koduyla güvenli şekilde engellenir.
- **Kaynak Karmaşıklık Sınırları (`MobilDwg.Core.Guards.CadResourceBudget`)**:
  - Maksimum dosya boyutu: 256 MB (mobil bellek patlamasını engeller).
  - Maksimum varlık sayısı: 250,000 varlık.
  - Maksimum blok iç içe geçme derinliği: 32 seviye.
  - Blok döngü tespiti: Bloklar arası döngüsel referanslar (`A -> B -> A`) set tabanlı zincir takibiyle tespit edilir ve `BLOCK_CYCLE_DETECTED` koduyla kesilir.
  - Maksimum metin uzunluğu: 64 KB (aşırı uzun metinler deterministik olarak kısaltılır ve uyarı eklenir).
  - Maksimum tarama (hatch) sınır segmenti: 10,000 segment.
  - Maksimum raster görüntü boyutu: 4,096 x 4,096 piksel.
  - Raster dekompresyon bombası (decompression bomb) koruması: Toplam piksel sayısı 15,000,000 (15 MP) üzerinde olan görüntüler bellek tüketimini önlemek için engellenir.
  - Maksimum XREF referans sayısı: 100 harici referans.
- **Sayısal Koordinat ve NaN/Sonsuzluk Muhafızları (`MobilDwg.Core.Guards.CadSanityGuards`)**:
  - `double.NaN`, `double.PositiveInfinity` ve `double.NegativeInfinity` koordinatları tespit edilir ve kontrollü şekilde güvenli yedek değere dönüştürülür.
  - Aşırı büyük koordinatlar (mutlak değeri $10^{12}$ eşiğini aşan) taşma ve sayısal kararsızlığı önlemek için güvenli sınıra kelepçelenir (clamp).
  - Bounding box sınırları geçersizliklere karşı (Min > Max, sonsuz veya NaN) otomatik onarılır.
- **Deterministik Durum Şeması (`ResourceGuardsSemanticSnapshot`)**:
  - `schema=resource-guards/v1` formatında dosya boyutu, format, varlık sayısı, blok derinliği, tarama segmentleri, raster boyutları, sanitize edilen koordinatlar ve teşhis kayıtları deterministik olarak dökümlenir.
- **Sınırlı Fuzz Duman Doğrulaması**:
  - 15 iterasyonluk rastgele bayt mutasyonu ve bozuk başlık testleri sonucunda sistemin hiçbir çökme yaşamadan (zero crashes) kontrollü hata kodu ürettiği doğrulanmıştır.
- **Android Uyumluluk ve Temiz Mimari**:
  - `A19AndroidValidationRunner`: `MobilDwg.App` katmanında doğrudan SkiaSharp veya ACadSharp bağımlılığı olmadan temiz mimari sınırları korunarak gerçek Android API 36 emülatörü üzerinde koşulmuştur.

## AŞAMA 19 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| DWG Magic / Sürüm Doğrulaması (AC10xx) | CadPreflightInspector.Inspect | PASS |
| DXF İkili ve ASCII İmza Doğrulaması | CadPreflightInspector.Inspect | PASS |
| Yabancı Format Engelleme (PE, ELF, ZIP, PDF, HTML, İmajlar) | CadPreflightInspector.Inspect (ForeignFormat) | PASS |
| Boş ve Kesilmiş Dosya Kontrolü (<6 bayt) | CadPreflightInspector.Inspect (EmptyOrTruncated) | PASS |
| Dosya Boyutu ve Varlık Sayısı Bütçeleri | CadBudgetGuard.CheckFileSize, CheckEntityCount | PASS |
| Blok Yuvalama Derinliği ve Döngü Tespiti | CadBudgetGuard.CheckBlockDepthAndCycle | PASS |
| Metin Uzunluğu ve Tarama Segment Bütçeleri | CadBudgetGuard.SanitizeText, CheckHatchSegments | PASS |
| Raster Boyut ve Dekompresyon Bombası Koruması | CadBudgetGuard.CheckRasterDimensions | PASS |
| NaN, Sonsuzluk ve $10^{12}$ Koordinat Muhafızı | CadSanityGuards.SanitizeCoordinate, SanitizeBounds | PASS |
| Deterministik Resource Guards Snapshot | ResourceGuardsSemanticSnapshot (schema=resource-guards/v1) | PASS |
| Sınırlı Fuzz/Mutasyon Duman Testi (15 iterasyon) | Stage19ResourceGuardsTests / A19AndroidValidationRunner | PASS |
| Host C# Testleri (Release) | Stage19ResourceGuardsTests (12/12 test) | PASS |
| Katman Mimari Testleri | MobilDwg.Architecture.Tests (SkiaSharp/ACadSharp bağımsızlık kontrolleri) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A19Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a19-android-resource-guards-gate.ps1 | PASS |
| Gerçek App UI Doğrulaması | uiautomator dump -> ANDROID_STAGE19_RESOURCE_GUARDS_PASS | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a19-real-app-guards.png (116,176 bayt) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 11834, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- `STAGE19_RESOURCE_GUARDS_TESTS_PASS`:
  - `TestPreflightValidDwgMagicPasses`: PASS
  - `TestPreflightInvalidDwgMagicRejected`: PASS
  - `TestPreflightBinaryAndAsciiDxfSignatures`: PASS
  - `TestPreflightNonCadFilesRejectedCleanly`: PASS
  - `TestPreflightZeroByteAndTruncatedHeader`: PASS
  - `TestFileSizeBudgetExceededRejection`: PASS
  - `TestEntityCountBudgetExceededGuard`: PASS
  - `TestBlockInsertNestingDepthBudgetAndCycleDetection`: PASS
  - `TestTextLengthBudgetAndTruncation`: PASS
  - `TestRasterImageDimensionBudgetGuard`: PASS
  - `TestNanInfinityAndExtremeCoordinatesSanityGuard`: PASS
  - `TestBoundedMutationFuzzSmokeZeroCrashesAndSnapshotDeterminism`: PASS

### 2. Mimari Katman Testleri (`MobilDwg.Architecture.Tests`)
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`
- (MobilDwg.App içinde SkiaSharp ve ACadSharp doğrudan kaynak bağımlılık yasağı eksiksiz doğrulanmıştır).

### 3. Android API 36 Emülatör Kabul Çıktısı
- Kabul Komutu: `powershell -ExecutionPolicy Bypass -File scripts/a19-android-resource-guards-gate.ps1`
- APK Boyutu: `39,417,878` bayt
- APK SHA256: `dd7ccd6001f4851ca59281ff71b49d4b3b82f4f8d5393724779d1bd12ab6da7e`
- Paket: `com.smitelagwar.mobildwg`
- Süreç / Liveness: PID `11834` (çökme veya ANR yok)
- Ekran Görüntüsü: `artifacts/a19-android-resource-guards/a19-real-app-guards.png` (116,176 bayt, SHA256: `cfadf755a2815c015d733f466d66b16002c55b7a67fa3869a9e6ff329544dcc9`)
- Logcat ve Terminal İşaretleri:
  - `A19_EMULATOR_API36_PASS serial=emulator-5554 android=16 abi=x86_64`
  - `A19_REAL_APP_APK_PASS bytes=39417878 sha256=dd7ccd6001f4851ca59281ff71b49d4b3b82f4f8d5393724779d1bd12ab6da7e`
  - `A19_REAL_APP_INSTALL_PASS package=com.smitelagwar.mobildwg launcher=com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
  - `A19_REAL_APP_LAUNCH_PASS pid=11834`
  - `A19_ANDROID_PREFLIGHT_PASS`
  - `A19_ANDROID_BUDGET_GUARDS_PASS`
  - `A19_ANDROID_SANITY_GUARDS_PASS`
  - `A19_ANDROID_FUZZ_PASS count=15`
  - `A19_ANDROID_SNAPSHOT_PASS`
  - `A19_ANDROID_SKIA_RENDER_PASS`
  - `A19_REAL_APP_GUARDS_MARKERS_PASS`
  - `A19_REAL_APP_UI_STATUS_PASS`
  - `A19_SCREENSHOT_PNG_PASS bytes=116176 sha256=cfadf755a2815c015d733f466d66b16002c55b7a67fa3869a9e6ff329544dcc9`
  - `A19_REAL_APP_STABILITY_PASS pid=11834`
  - `ANDROID_STAGE19_RESOURCE_GUARDS_PASS`
- İddia Sınırı: `CLAIM_LIMIT=A19_RESOURCE_GUARDS_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`

## Ekran Kanıtı

AŞAMA 19 Android API 36 emülatöründe çalışan gerçek uygulamanın ekran görüntüsü:

![A19 Real Android App Resource Guards Rendering](file:///c:/Users/hsyn/Desktop/MOBIL_UYGULAMA_DWG/artifacts/a19-android-resource-guards/a19-real-app-guards.png)
