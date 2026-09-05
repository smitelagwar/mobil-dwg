# AŞAMA 17 Kanıtı — XREF / Raster Image / Underlay / External References & Compatibility (Dış Referanslar, Raster Görseller ve Uyumluluk)

## Durum

`DONE`

AŞAMA 17 çıkış kriterleri platform-neutral C# unit testleri (12/12 PASS), katman mimari testleri (`MobilDwg.Architecture.Tests`) ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama AutoCAD DWG XREF (Dış Referanslar), Raster Görseller (PNG, JPG, BMP), PDF/DWF/DGN altlıkları (Underlays), güvenlik korumalarını (uzak URL indirme engeli `XREF_REMOTE_NOT_SUPPORTED`, yol geçişi/directory traversal engeli `PATH_TRAVERSAL_PREVENTED`), büyük/küçük harf duyarsız yerel dosya çözümlemesini (`CadReferenceResolver`), eksik referanslar için görsel yer tutucu geometrisi ve tanıları (`MissingReferencePrimitive` ile sınır kutusu, köşegen çarpı, uyarı etiketi), SkiaSharp raster görsel render motorunu (`RasterImagePrimitive` ile kırpma sınırı `ClipBoundary`, parlaklık/kontrast/solma `Fade`), ve deterministik referans envanterini (`ExternalReferenceSemanticSnapshot` schema `xref-compat/v1`) kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `1736fb5` (A16 tamamlanması sonrası).
- Branch: `stage17-xref-compat` (PR #30, merge commit: `dd9727b`).
- **Referans Türleri ve Modelleri (`MobilDwg.Rendering.References`)**:
  - `CadExternalReferenceKind`: `DwgXref`, `RasterImage`, `PdfUnderlay`, `DwfUnderlay`, `DgnUnderlay`, `PointCloud`, `OleObject`.
  - `CadExternalReference`: Referans kimliği, türü, orijinal dosya yolu, sınır kutusu, yerleşim noktası ve ölçek.
- **Güvenli Referans Çözümleyici (`CadReferenceResolver`)**:
  - **Uzak URL Engeli**: `http://`, `https://`, `ftp://` gibi uzaktan indirme şemaları güvenlik gereği hiçbir ağ çağrısı yapılmadan doğrudan engellenir (`XREF_REMOTE_NOT_SUPPORTED`).
  - **Yol Geçişi (Path Traversal) Engeli**: Referans yollarında `..` kullanılarak yetkili arama dizinleri dışına çıkılmaya çalışıldığında engellenir (`PATH_TRAVERSAL_PREVENTED`).
  - **Büyük/Küçük Harf Duyarsız Çözümleme**: Çizim dizini ve verilen yetkili klasörler içinde dosya adları Linux/Android ortamında dahi Windows CAD dosyalarıyla uyumlu şekilde büyük/küçük harf duyarsız eşleştirilir.
- **Eksik Referans Görsel Yer Tutucusu (`MissingReferencePrimitive`)**:
  - Referans bulunamadığında sessizce başarısız olunmaz (`EXTERNAL_RESOURCE_NOT_FOUND`).
  - Çizimde tam referans sınır kutusunda dikdörtgen çerçeve, köşegen çarpı (`X`) çizgileri ve referans tipini/adını belirten uyarı etiketi (`[TYPE: Name - REASON]`) çizilir.
- **SkiaSharp Raster Render Motoru (`SkiaCadRenderer` & `RasterImagePrimitive`)**:
  - `RasterImagePrimitive`: Görüntü baytları veya yerel yolu, dünya sınırları, 2D dönüşüm, kırpma poligonu (`ClipBoundary`), parlaklık/kontrast ve solma (`Fade`).
  - `DrawRasterImagePrimitive`: `SKBitmap.Decode`, `canvas.ClipPath` (veya `ClipRect`), `SKSamplingOptions(SKFilterMode.Linear)`, ve solma/saydamlık alfa kanalı ile gerçek piksel çizimi gerçekleştirilir.
- **Deterministik Referans Envanteri (`ExternalReferenceSemanticSnapshot`)**:
  - `schema=xref-compat/v1` formatı ile çözümlenmiş ve eksik tüm dış referanslar, yolları, durumları, tanı kodları ve sınırları deterministik sıralamayla doğrulanır.

## AŞAMA 17 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| Dış Referans Tespiti ve Tipleri | CadReferenceTypes (DwgXref, RasterImage, PdfUnderlay vb.) | PASS |
| Uzak URL Otomatik İndirme Engeli | CadReferenceResolver (http/https/ftp -> XREF_REMOTE_NOT_SUPPORTED) | PASS |
| Path Traversal Güvenlik Engeli | CadReferenceResolver (.. dışarı çıkış -> PATH_TRAVERSAL_PREVENTED) | PASS |
| Büyük/Küçük Harf Duyarsız Eşleme | CadReferenceResolver (case-insensitive local matching) | PASS |
| Eksik Referans Yer Tutucusu (Çerçeve + Çarpı + Etiket) | MissingReferencePrimitive + SkiaCadRenderer DrawMissingReferencePrimitive | PASS |
| Raster Görsel Skia Çizimi | RasterImagePrimitive + SkiaCadRenderer DrawRasterImagePrimitive | PASS |
| Raster Kırpma Poligonu (Clipping) | RasterImagePrimitive.ClipBoundary + canvas.ClipPath | PASS |
| Raster Solma / Fade Parametresi | RasterImagePrimitive.Fade -> Alpha transparency blending | PASS |
| Deterministik Snapshot | ExternalReferenceSemanticSnapshot (schema=xref-compat/v1) | PASS |
| Host C# Testleri (Release) | Stage17ReferenceCompatibilityTests (12/12 test) | PASS |
| Katman Mimari Testleri | MobilDwg.Architecture.Tests (SkiaSharp/ACadSharp bağımsızlık kontrolleri) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A17Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a17-android-xref-compat-gate.ps1 | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a17-real-app-xref.png (155,161 byte) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 10383, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- `STAGE17_REFERENCE_COMPATIBILITY_TESTS_PASS`:
  - `TestUnresolvedXrefEmitsDiagnosticAndGeneratesPlaceholder`: PASS
  - `TestMissingRasterImageEmitsDiagnosticAndGeneratesPlaceholder`: PASS
  - `TestMissingPdfUnderlayEmitsDiagnosticAndGeneratesPlaceholder`: PASS
  - `TestRemoteUrlRejectedWithSecurityDiagnostic`: PASS
  - `TestBoundedDirectoryResolverMatchesFilenameCaseInsensitively`: PASS
  - `TestPathTraversalAttemptBlockedWithSecurityDiagnostic`: PASS
  - `TestResolvedLocalRasterImageCreatesValidPrimitive`: PASS
  - `TestSkiaRenderRasterImageProducesNonBackgroundPixels`: PASS
  - `TestRasterImageClippingBoundaryRestrictsRendering`: PASS
  - `TestRasterImageFadeParameter`: PASS
  - `TestCompositeSceneWithResolvedRasterAndMissingReferences`: PASS
  - `TestExternalReferenceSemanticSnapshotDeterminism`: PASS

### 2. Mimari Katman Testleri (`MobilDwg.Architecture.Tests`)
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`
- (MobilDwg.App içinde SkiaSharp ve ACadSharp doğrudan kaynak bağımlılık yasağı eksiksiz doğrulanmıştır).

### 3. Android API 36 Emülatör Kabul Çıktısı
- Kabul Komutu: `powershell -ExecutionPolicy Bypass -File scripts/a17-android-xref-compat-gate.ps1`
- APK Boyutu: `39,389,206` bayt
- APK SHA256: `376cdfd354b35e587e4bf2d8e2317a377a77f29446a1d85f4005983c4b2c3c3c`
- Paket: `com.smitelagwar.mobildwg`
- Süreç / Liveness: PID `10383` (çökme veya ANR yok)
- Ekran Görüntüsü: `artifacts/a17-android-xref-compat/a17-real-app-xref.png` (155,161 bayt, SHA256: `9c011820b640080acfd3500272a0a578a57b9b381d874fe459af14b289ed59f5`)
- Logcat İşaretleri:
  - `A17_ANDROID_REMOTE_REJECTED_PASS`
  - `A17_ANDROID_SECURITY_TRAVERSAL_PASS`
  - `A17_ANDROID_SKIA_RENDER_PASS`
  - `A17_REAL_APP_XREF_COMPAT_MARKERS_PASS`
  - `A17_REAL_APP_UI_STATUS_PASS`
  - `A17_REAL_APP_STABILITY_PASS pid=10383`
  - `ANDROID_STAGE17_XREF_COMPAT_PASS`
- İddia Sınırı: `CLAIM_LIMIT=A17_XREF_COMPAT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`

## Ekran Kanıtı

AŞAMA 17 Android API 36 emülatöründe çalışan gerçek uygulamanın ekran görüntüsü:

![A17 Real Android App XREF / Raster / Underlay Rendering](file:///c:/Users/hsyn/Desktop/MOBIL_UYGULAMA_DWG/artifacts/a17-android-xref-compat/a17-real-app-xref.png)
