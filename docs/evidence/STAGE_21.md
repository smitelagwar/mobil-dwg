# AŞAMA 21 Kanıtı — Android Full Corpus Regression / Beta Gate

## Durum

`DONE`

AŞAMA 21 çıkış kriterleri platform-neutral C# testleri (7/7 PASS), katman mimari testleri (`MobilDwg.Architecture.Tests`) ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama public/private corpus testleri, P0/P1 uyumluluk matrisi (C0–C4 katmanları), harita/kadastro orjin (5.000.000 + 0,001) çift duyarlıklı sayısal hassasiyet denetimi, Debug vs. Release / Trimming / AOT fark analizi ve `schema=corpus-regression/v1` deterministik snapshot ile beta kapı onayını (`ANDROID_STAGE21_BETA_GATE_PASS`) kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `7069f9a` (A20 tamamlanması sonrası).
- Branch: `stage21-corpus-regression`.
- PR: `#34` (`feat(stage21): full corpus regression and beta gate with api36 acceptance gate`).
- `main` Merge SHA: `919888b`.
- **Corpus Regresyon Kapsamı (14 Aşama / 14 PASS)**:
  1. `synthetic-turkish-basic-ac1015`: Repo içi DXF fikstürü, Türkçe karakterler (Ö, Ç, Ş, İ, Ğ, Ü), iç içe bloklar ve temel CAD varlıkları. (Katman: C3).
  2. `synthetic-turkish-basic-ac1015-dwg`: AC1015 ikili DWG formatında sentetik çizim. (Katman: C3).
  3. `negative-missing-font-ac1015`: Eksik SHX yazı tipi içeren kontrollü negatif fikstür; `missing-font` uyarısı ile kontrollü font fallback. (Katman: C2).
  4. `negative-missing-xref-ac1015`: Eksik harici referans dosyası içeren kontrollü negatif fikstür; `missing-xref` uyarısı ile güvenli atlama. (Katman: C2).
  5. `corpus-p0-geometry`: Line, Arc, Circle, Ellipse, Point, Spline, Solid, LWPolyline varlıklarının tam geometri doğrulaması. (Katman: C4).
  6. `corpus-survey-origin-precision`: 5.000.000,001 m koordinatında kadastro/harita çift duyarlıklı floating-point hassasiyet ve ekran/dünya dönüşüm koruma testi. (Katman: C4).
  7. `corpus-block-insert-hierarchy`: İç içe blok (`INSERT`) hiyerarşisi, homojen olmayan ölçek ve ayna matris dönüşümleri. (Katman: C3).
  8. `corpus-layer-style-matrix`: ACI 1-255 renkleri, TrueColor (24-bit RGB), çizgi tipleri (DASHED, CENTER, CONTINUOUS), çizgi kalınlıkları ve ByLayer kuralları. (Katman: C3).
  9. `corpus-text-turkish-unicode`: Windows-1254 Türkçe kod sayfası, `\U+XXXX` unicode kaçış dizileri, `%%d`, `%%p`, `%%c`, `%%%` MText formatları ve font fallback. (Katman: C3).
  10. `corpus-dimension-hatch`: Aligned, Rotated ve Radial ölçülendirme; EvenOdd dolgu ve ANSI31 tarama desenleri. (Katman: C4).
  11. `corpus-layout-viewport`: Model alanı ve Layout (Paper Space) sınırları, çoklu viewport ve klip pencereleri. (Katman: C3).
  12. `corpus-xref-compatibility`: DWG dış referans (XREF) ve altlık yer tutucu uyumluluk doğrulaması. (Katman: C3).
  13. `corpus-resource-guards`: Sihirli bayt ön kontrolü, ELF/ZIP gibi yabancı formatların reddi ve geçersiz/NaN koordinat korumaları. (Katman: C3).
  14. `corpus-performance-stress`: Orta (780+ varlık) ve büyük ölçekli stres korpusu sahne montaj ve çizim duman testi. (Katman: C3).

- **P0 / P1 Uyumluluk ve Sadakat Katmanları (Fidelity Tiers)**:
  - P0 Kategorisi (8 aşama): 8/8 PASS (%100) — Hepsi $\ge$ C3 (Temel Geometri ve Kadastro Precision C4 mühendislik düzeyinde).
  - P1 Kategorisi (4 aşama): 4/4 PASS (%100) — Hepsi $\ge$ C3.
  - Negatif/Güvenlik (2 aşama): 2/2 PASS — Kontrollü hata ve uyarı kodlarıyla C2 düzeyinde yakalandı, çökme veya bellek sızıntısı yok.
  - $\ge$ C3 Sadakat Oranı: **%85,7** (Gereksinim: $\ge$ %75,0).

- **Harita/Kadastro Orjin Çift Duyarlık Korunumu**:
  - `(5,000,000.001, 5,000,000)` koordinatındaki 1 mm fark kameradan ekrana ve ekrandan dünyaya geri dönüşümde (`ScreenToWorld`) $10^{-9}$ tolerans ile tam olarak korunmuştur.

- **Debug vs. Release / Trimming / AOT Analizi**:
  - `A21_TRIMMING_AOT_PASS status=verified reflection_and_rendering_symbols_intact`.
  - Kod kırpma (trimming) ve AOT derleme altında SkiaSharp ve ACadSharp yansıma/çizim sembollerinin tam korunduğu doğrulanmıştır.

- **Deterministik Semantik Snapshot (`schema=corpus-regression/v1`)**:
  - SHA256: `8edc8ac4fd5cb3de65e6ff66eb0bcee7254ae842cf3c57dbb2cb1b26054e4152`.

- **Beta Kapı Kararı (`CadBetaGateVerdict`)**:
  - Karar: `ANDROID_STAGE21_BETA_GATE_PASS` (`isPass=True`, `blockers=0`, `score=100/100`).

- **Android API 36 Emülatör Ölçümleri**:
  - İmzalı Release APK Paket Boyutu: **39,806,156 bayt** (~37.96 MB, < 45 MB tavan bütçesi altında).
  - İşletim Sistemi Toplam PSS (`dumpsys meminfo`): **134.1 MB** (< 250 MB tavan bütçesi altında).
  - Süreç ve Kararlılık: PID `13377`, 0 çökme, 0 ANR, süreç sürekliliği tam korundu.
  - Görsel Doğrulama Ekran Görüntüsü: `a21-real-app-corpus.png` (166,890 bayt, PNG imzası geçerli, SHA256: `148d783e8800e391ee75fe1efc49c0961f6570603395c338759b0e95636d6224`).
  - UI Durumu: `uiautomator dump` hiyerarşisinde `ANDROID_STAGE21_CORPUS_REGRESSION_PASS` durumu doğrulandı.

- **Temiz Mimari ve Sınır Koruması**:
  - `MobilDwg.App` katmanında `SkiaSharp` veya `ACadSharp` ad alanlarına doğrudan bağımlılık olmaksızın, `MobilDwg.Architecture.Tests` (%100 PASS) ile sınır kuralları tam korundu.

## AŞAMA 21 Gereksinim Matrisi

| Gereksinim | Doğrulama Mekanizması | Durum |
|---|---|---|
| Full Public/Private Corpus Regression (14/14) | `CadCorpusRegressionSuite.RunFullRegressionAsync` | PASS |
| P0 Uyumluluk $\ge$ C3 / C4 (8/8) | P0 Geometri, Precision, Blok, Katman, Font, Ölçü | PASS |
| P1 Alt Sistem Uyumluluğu (4/4) | Layout, XREF, Resource Guards, Perf Stress | PASS |
| Kontrollü Negatif Fikstürler (2/2 C2) | Missing Font & Missing XREF tanı kodları | PASS |
| Kadastro Orjini Çift Duyarlık (5.000.000 + 0.001) | `TestSurveyOriginDoublePrecisionIntegrity` | PASS |
| Deterministik Snapshot (`corpus-regression/v1`) | `CorpusRegressionSemanticSnapshot.Create` | PASS |
| Debug vs. Release / Trimming / AOT | `A21_TRIMMING_AOT_PASS` | PASS |
| Beta Kapı Kararı | `ANDROID_STAGE21_BETA_GATE_PASS` | PASS |
| Host C# Testleri | `Stage21CorpusRegressionTests` (7/7) | PASS |
| Mimari Katman Testleri | `MobilDwg.Architecture.Tests` | PASS |
| Release APK Paket Boyutu (<45 MB) | 39.80 MB (39,806,156 bayt) | PASS |
| Dumpsys Meminfo PSS (<250 MB) | 134.1 MB | PASS |
| Gerçek Android API 36 Emülatör Kabulü | `scripts/a21-android-corpus-regression-gate.ps1` | PASS |
| Gerçek App UI Doğrulaması | `uiautomator dump` -> `ANDROID_STAGE21_CORPUS_REGRESSION_PASS` | PASS |
| Byte-Safe PNG Ekran Görüntüsü | `a21-real-app-corpus.png` (166,890 bayt) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 13377, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri
- `STAGE21_CORPUS_REGRESSION_TESTS_PASS`:
  - `TestFullCorpusRegressionSummaryPasses`: PASS
  - `TestBetaGateVerdictEvaluation`: PASS
  - `TestSurveyOriginDoublePrecisionIntegrity`: PASS
  - `TestP0EntityFidelityCoverage`: PASS
  - `TestControlledNegativeGuards`: PASS
  - `TestSemanticSnapshotDeterminism`: PASS
  - `TestDebugVsReleasePipelineIntegrity`: PASS

### 2. Mimari Katman Testleri (`MobilDwg.Architecture.Tests`)
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`

### 3. Android API 36 Emülatör Kabul Çıktısı
- Kabul Komutu: `powershell -ExecutionPolicy Bypass -File scripts/a21-android-corpus-regression-gate.ps1`
- APK Boyutu: `39,806,156` bayt
- APK SHA256: `cbd0ea099db7ca58f95b8a0c083c6482c1bc8a3d99202f532c996b371c9ec249`
- Paket: `com.smitelagwar.mobildwg`
- Süreç / Liveness: PID `13377` (çökme veya ANR yok)
- Ekran Görüntüsü: `artifacts/a21-android-corpus-regression/a21-real-app-corpus.png` (166,890 bayt, SHA256: `148d783e8800e391ee75fe1efc49c0961f6570603395c338759b0e95636d6224`)
- Logcat ve Terminal İşaretleri:
  - `A21_EMULATOR_API36_PASS serial=emulator-5554 android=16 abi=x86_64`
  - `A21_REAL_APP_APK_PASS bytes=39806156 sha256=cbd0ea099db7ca58f95b8a0c083c6482c1bc8a3d99202f532c996b371c9ec249`
  - `A21_REAL_APP_INSTALL_PASS package=com.smitelagwar.mobildwg launcher=com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
  - `A21_REAL_APP_LAUNCH_PASS pid=13377`
  - `A21_CORPUS_REGRESSION_PASS count=14 passed=14 negatives=2`
  - `A21_P0_P1_MATRIX_PASS p0=8/8 p1=4/4 c3_pct=85.7`
  - `A21_BETA_GATE_VERDICT_PASS marker=ANDROID_STAGE21_BETA_GATE_PASS isPass=True`
  - `A21_TRIMMING_AOT_PASS status=verified reflection_and_rendering_symbols_intact`
  - `A21_SNAPSHOT_PASS sha256=8edc8ac4fd5cb3de65e6ff66eb0bcee7254ae842cf3c57dbb2cb1b26054e4152`
  - `A21_ANDROID_SKIA_RENDER_PASS bytes=51088 sha256=bf6a23465af7e23ebed922bd0d5be4cc8165dbcc0c22075470fbe809cf8e5303`
  - `A21_REAL_APP_STABILITY_PASS pid=13377`
  - `ANDROID_STAGE21_CORPUS_REGRESSION_PASS`
  - `A21_REAL_APP_UI_IMAGE_READY sha256=bf6a23465af7e23ebed922bd0d5be4cc8165dbcc0c22075470fbe809cf8e5303`
  - `A21_REAL_APP_REGRESSION_MARKERS_PASS`
  - `A21_REAL_APP_UI_STATUS_PASS`
  - `A21_SCREENSHOT_PNG_PASS bytes=166890 sha256=148d783e8800e391ee75fe1efc49c0961f6570603395c338759b0e95636d6224`
  - `A21_MEMINFO_PSS_PASS total_pss=134.1 MB`
  - `A21_REAL_APP_STABILITY_PASS pid=13377`
  - `ANDROID_STAGE21_CORPUS_REGRESSION_PASS`
