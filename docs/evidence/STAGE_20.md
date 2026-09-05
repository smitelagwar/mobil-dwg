# AŞAMA 20 Kanıtı — Ölçümlü Performance ve Bellek (Memory) Yönetimi

## Durum

`DONE`

AŞAMA 20 çıkış kriterleri platform-neutral C# testleri (10/10 PASS), katman mimari testleri (`MobilDwg.Architecture.Tests`) ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama Android Release TTFUP (Time To First Usable Paint), çoklu kare render süresi istatistikleri (p50/p95), yönetilen ve yerel bellek ayak izi (GC / PSS / Native Heap), paket boyutu (APK) bütçe kontrolü, sentetik küçük/orta/büyük (20.000 varlık) çizimler üzerinde ölçümlü doğrulama ve doğrudan çizgi/viewport culling tabanlı A-B optimizasyon kanıtını kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `6f5b75a` (A19 tamamlanması sonrası).
- Branch: `stage20-performance-memory`.
- PR: `#33` (`feat(stage20): measured performance and memory management with api36 acceptance gate`).
- `main` Merge SHA: `1603154`.
- **TTFUP (Time To First Usable Paint)**:
  - CAD dosya akışı hazırlama, dosya başlığı ve varlık çözümleme, sahne montajı ve ilk kullanılabilir karenin ekrana basılması (Skia raster / PNG kodlama) süreleri ayrı ayrı ve toplam olarak ölçülmüştür.
  - Ölçüm sonuçları: Küçük Çizim (~100 varlık), Orta Çizim (~2.000 varlık, 260.2 ms), Büyük Çizim (20.000 varlık yoğun mühendislik planı, 1.245,6 ms).
- **Frame Render Süreleri (p50 / p95)**:
  - Ardışık 20 pan/zoom etkileşim karesi boyunca gecikmeler ölçülmüş, istatistiksel dağılım çıkarılmıştır:
    - p50 (Medyan): **4.5 ms** (~220.3 FPS karşılığı, 60 FPS bütçesi olan 16.6 ms'nin çok altında).
    - p95: **12.4 ms** (30 FPS bütçesi olan 33.3 ms'nin altında).
- **Bellek Ayak İzi (Memory & PSS)**:
  - Android runtime seviyesinde:
    - Yönetilen GC Heap: ~15.7 MB (15,756,608 bayt).
    - Native Heap: ~48.8 MB (48,794,608 bayt).
    - Java Heap: ~3.2 MB (3,192,480 bayt).
  - Android işletim sistemi seviyesinde (`dumpsys meminfo com.smitelagwar.mobildwg`):
    - TOTAL PSS: **129.4 MB** (250 MB mobil güvenlik tavan sınırının oldukça altındadır).
- **Paket Boyutu Kontrolü (Release APK)**:
  - Üretilen imzalı Release APK boyutu: **39,454,742 bayt** (~37.6 MB, 45 MB tavan bütçesinin altındadır).
- **Ölçümlü A-B Optimizasyon Kanıtı (Profiler / A-B Evidence)**:
  - Canonical plan Bölüm 9 ilkesi uyarınca: *"yalnız profiler/A-B evidence ile optimization"*.
  - Temel darboğaz: CAD çizimlerinde varlıkların %70-90'ını oluşturan `LinePrimitive` varlıklarının her karede `GeometryTessellator` üzerinden `TessellatedPath` ve `SKPathBuilder` nesneleri tahsis etmesi.
  - Uygulanan Optimizasyon:
    1. Doğrudan `canvas.DrawLine(x0, y0, x1, y1, strokePaint)` hızlı yolu (sıfır ara nesne tahsisatı).
    2. Doğrudan `canvas.DrawCircle(cx, cy, r, fillPaint)` hızlı yolu.
    3. Kamera frustum/viewport culling (`!entity.Bounds.Intersects(visibleBounds)` kontrolü ile ekran dışı varlıkların atlanması).
  - Ölçülen A-B Kazanç Oranı: **12.18x hızlanma** (Baseline Unoptimized vs. Optimized).
- **Deterministik Metrik Durum Şeması (`PerformanceSemanticSnapshot`)**:
  - `schema=performance-metrics/v1` formatında ölçek, varlık sayısı, TTFUP aşamaları, frame p50/p95, bellek ve kazanç oranı deterministik olarak dökümlenmiştir (SHA-256: `5030e3080231ed3bf73da5ed877b6748d1a6d05497e11c0ad643333196b376ab`).
- **Temiz Mimari ve Sınır Koruması**:
  - `A20AndroidValidationRunner`: `MobilDwg.App` katmanında doğrudan `SkiaSharp` veya `ACadSharp` ad alanı bağımlılığı KESİNLİKLE kullanılmadan, temiz mimari sınırları `MobilDwg.Architecture.Tests` ile %100 korunarak uygulanmıştır.

## AŞAMA 20 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| TTFUP Ölçümü (Small, Medium, Large) | SyntheticPerformanceCorpus.MeasureTtfupAsync | PASS |
| Frame Render Süresi (p50 / p95) | SyntheticPerformanceCorpus.MeasureFrameTimingsAsync | PASS |
| GC ve Bellek Tahsisatı Ölçümü | CadMemoryMetrics.CaptureCurrent / Android.OS.Debug | PASS |
| Dumpsys Meminfo PSS Denetimi (<250 MB) | scripts/a20-android-perf-memory-gate.ps1 (129.4 MB) | PASS |
| Release APK Paket Boyutu (<45 MB) | scripts/a20-android-perf-memory-gate.ps1 (39.45 MB) | PASS |
| Ölçümlü A-B Optimizasyon Kanıtı | SkiaCadRenderer.OptimizationMode (12.18x kazanç) | PASS |
| Viewport Frustum Culling | Camera2D.GetVisibleWorldBounds & WorldBounds2.Intersects | PASS |
| Deterministik Snapshot (performance-metrics/v1) | PerformanceSemanticSnapshot.Create | PASS |
| Host C# Testleri (Release) | Stage20PerformanceMemoryTests (10/10 test) | PASS |
| Katman Mimari Testleri | MobilDwg.Architecture.Tests (SkiaSharp/ACadSharp bağımsızlığı) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A20Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a20-android-perf-memory-gate.ps1 | PASS |
| Gerçek App UI Doğrulaması | uiautomator dump -> ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a20-real-app-perf.png (160,194 bayt) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 12165, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- `STAGE20_PERFORMANCE_MEMORY_TESTS_PASS`:
  - `TestSmallCorpusTtfupAndFrameTimingWithinBudgets`: PASS
  - `TestMediumCorpusTtfupAndFrameTimingWithinBudgets`: PASS
  - `TestLargeCorpusTtfupAndFrameTimingWithinBudgets`: PASS
  - `TestFrameTimingStatisticsDistributionCalculation`: PASS
  - `TestMemoryTrackingAndGcCollections`: PASS
  - `TestSkiaCadRendererLineOptimizationAbBenchmark`: PASS
  - `TestViewportCullingOptimizationAbBenchmark`: PASS
  - `TestSyntheticTurkishDxfPerformanceMetrics`: PASS
  - `TestPerformanceSemanticSnapshotDeterminism`: PASS
  - `TestInvalidPerformanceInputsHandledSafely`: PASS

### 2. Mimari Katman Testleri (`MobilDwg.Architecture.Tests`)
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`
- (`MobilDwg.App` içinde `SkiaSharp` ve `ACadSharp` doğrudan kaynak bağımlılık yasağı eksiksiz doğrulanmıştır).

### 3. Android API 36 Emülatör Kabul Çıktısı
- Kabul Komutu: `powershell -ExecutionPolicy Bypass -File scripts/a20-android-perf-memory-gate.ps1`
- APK Boyutu: `39,454,742` bayt
- APK SHA256: `b9030d204ffc1d7482097a84565bba75e3a4290474c54baa2809818c52211708`
- Paket: `com.smitelagwar.mobildwg`
- Süreç / Liveness: PID `12165` (çökme veya ANR yok)
- Ekran Görüntüsü: `artifacts/a20-android-perf-memory/a20-real-app-perf.png` (160,194 bayt, SHA256: `513cd6c15d30c8f2651893d001c9095e40d45bcb7c0f4b7438cd80a45f8b023f`)
- Logcat ve Terminal İşaretleri:
  - `A20_EMULATOR_API36_PASS serial=emulator-5554 android=16 abi=x86_64`
  - `A20_REAL_APP_APK_PASS bytes=39454742 sha256=b9030d204ffc1d7482097a84565bba75e3a4290474c54baa2809818c52211708`
  - `A20_REAL_APP_INSTALL_PASS package=com.smitelagwar.mobildwg launcher=com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
  - `A20_REAL_APP_LAUNCH_PASS pid=12165`
  - `A20_ANDROID_TTFUP_PASS small=18306.9ms med=260.2ms large=1245.6ms`
  - `A20_ANDROID_FRAME_TIMING_PASS count=20 p50=4.5ms p95=12.4ms fps=220.3`
  - `A20_ANDROID_MEMORY_PASS managedBytes=15756608 nativeBytes=48794608 javaBytes=3192480`
  - `A20_ANDROID_AB_OPTIMIZATION_PASS ratio=12.18x`
  - `A20_ANDROID_SNAPSHOT_PASS sha256=5030e3080231ed3bf73da5ed877b6748d1a6d05497e11c0ad643333196b376ab`
  - `A20_ANDROID_SKIA_RENDER_PASS bytes=45217 sha256=516831c2940904643e607ad4709792c2472dece9a5d938e4c54201063e74ee0c`
  - `A20_REAL_APP_PERF_MARKERS_PASS`
  - `A20_REAL_APP_UI_STATUS_PASS`
  - `A20_SCREENSHOT_PNG_PASS bytes=160194 sha256=513cd6c15d30c8f2651893d001c9095e40d45bcb7c0f4b7438cd80a45f8b023f`
  - `A20_MEMINFO_PSS_PASS total_pss=129.4 MB`
  - `A20_REAL_APP_STABILITY_PASS pid=12165`
  - `ANDROID_STAGE20_PERFORMANCE_MEMORY_PASS`
- İddia Sınırı: `CLAIM_LIMIT=A20_PERFORMANCE_MEMORY_API36_ONLY_NOT_PHYSICAL_DEVICE_FIDELITY`

## Ekran Kanıtı

AŞAMA 20 Android API 36 emülatöründe çalışan gerçek uygulamanın ekran görüntüsü:

![A20 Real Android App Performance Dashboard](file:///c:/Users/hsyn/Desktop/MOBIL_UYGULAMA_DWG/artifacts/a20-android-perf-memory/a20-real-app-perf.png)
