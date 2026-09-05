# AŞAMA 25 Kanıtı — Android beta ve blocker düzeltmeleri

## Durum

`DONE`

AŞAMA 25 çıkış kriterleri platform-neutral C# testleri (Stage25BetaBlockerTests: ODE pass, GC clean pass, render error surface pass, positive render pass), SafeCadFileCache orphan temizliği (`PurgeAll`), MainActivity bellek baskısı entegrasyonu (`OnTrimMemory`), CadFileOpenCoordinator hata sonrası oturum sıfırlaması (`ResetCurrentSessionAsync`), görünür hata durum bildirimleri ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı (`ANDROID_STAGE25_BETA_BLOCKER_PASS`).

## Kapsam ve Kararlar

- Base `main` HEAD: `213b0b6`
- Branch: `stage25-beta-blockers`
- Yalnız crash/privacy/P0 fidelity/open/lifecycle/severe perf blocker'lar ele alındı; hiçbir yeni feature, editor, export veya bulut işlevi eklenmedi.
- **Kapatılan Blocker Sınıfları**:
  1. **B1 (Severe Perf / UI-Thread Bloğu)**: CAD parsing ve RenderScene oluşturma işi `Task.Run` işçi iş parçacığında yürütülür; UI ana iş parçacığını dondurmaz (A20 TTFUP bütçesi ile uyumlu).
  2. **B2 (Lifecycle / Dispose Zinciri)**: `CadViewerSession.Dispose()` sonrası nesne üzerindeki çağrılarda deterministik `ObjectDisposedException` fırlatılır; native bellek ve GC temizliği doğrulanmıştır (`A25_DISPOSE_CHAIN_PASS`).
  3. **B3 (Privacy & Crash / Orphan Cache Temizliği)**: `SafeCadFileCache.PurgeAll()` metodu eklendi. `MainActivity.OnTrimMemory(TrimMemory level)` hook'u üzerinden sistem bellek baskısı anında ve anormal sonlanmalardan arta kalan geçici çizim dosyaları silinir (`A25_CACHE_PURGE_PASS`).
  4. **B4 (P0 Fidelity / Render Hata Yüzeyi)**: Çizim okuma veya render esnasında oluşan istisnalar sessizce yutulmaz; kullanıcı arayüzündeki durum etiketine hata türü ve ayrıntısı yansıtılır (`A25_RENDER_ERROR_SURFACE_PASS`).
  5. **B5 (Crash / Hata Sonrası Temiz Yeniden Açma)**: `CadFileOpenCoordinator.ResetCurrentSessionAsync()` metodu eklendi; hatalı ya da bozuk dosya denemesinden sonra kalan bayat oturum temizlenir, koordinatör yeniden oluşturulmaya gerek kalmadan ikinci açma isteğine temiz başlar (`A25_COORDINATOR_RESET_PASS`).

## Kanıt Özeti

### 1. Host Sözleşme Testleri (net10.0)
- `STAGE25_DISPOSE_CHAIN_ODE_PASS`
- `STAGE25_DISPOSE_CHAIN_GC_PASS`
- `STAGE25_RENDER_ERROR_SURFACE_PASS`
- `STAGE25_RENDER_POSITIVE_PASS bytes=7385`
- `STAGE25_BETA_BLOCKER_TESTS_PASS`

### 2. Gerçek Android MAUI App Kabul Testi (API 36, emulator-5554)
- Release APK: `com.smitelagwar.mobildwg-Signed.apk` (~38 MB)
- Logcat belirteçleri:
  - `A25_ANDROID_VALIDATION_STARTING`
  - `A25_DISPOSE_CHAIN_PASS`
  - `A25_CACHE_PURGE_PASS`
  - `A25_RENDER_ERROR_SURFACE_PASS errorType=ObjectDisposedException`
  - `A25_COORDINATOR_RESET_PASS`
  - `A25_PROOF_PNG_READY bytes=25184 sha256=188da8c87ca1366e36e1d4d52af097e13eafe052ea855e65f7be6092657a68c7`
  - `ANDROID_STAGE25_BETA_BLOCKER_PASS blockers=B2=PASS|B3=PASS|B4=PASS|B5=PASS`
  - `A25_REAL_APP_UI_IMAGE_READY sha256=188da8c87ca1366e36e1d4d52af097e13eafe052ea855e65f7be6092657a68c7`
- UI Hiyerarşi Onayı: `artifacts/a25-android-beta-blocker/a25_window.xml` içinde `ANDROID_STAGE25_BETA_BLOCKER_PASS` onaylandı.
- Ekran Görüntüsü: `artifacts/a25-android-beta-blocker/a25-real-app-beta-blocker.png` (123 KB, geçerli PNG başlığı).

## Claim Sınırı
`A25_CLAIM: A25_BETA_BLOCKER_FIXES_EMULATOR_ONLY_NOT_PHYSICAL_DEVICE_FIDELITY`
