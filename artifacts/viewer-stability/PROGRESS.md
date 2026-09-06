# Mobil DWG Kararlı Görüntüleyici İlerleme Kaydı

**Tarih:** 6 Eylül 2026  
**Plan:** `docs/MOBIL_DWG_NIHAI_UYGULAMA_PLANI.md` (6 Eylül 2026 bütünlük denetimli sürüm)  
**Dal:** `codex/viewer-stability-v3`  
**Başlangıç HEAD:** `bbaf7bf84148c16a6d411ffe653c14771ee45848`  

## Aşama Durum Özeti

| Sıra | Aşama Adı | Durum |
|---|---|---|
| 01 | Güncel kaynak tabanı ve ölçüm | TAMAMLANDI |
| 02 | Paket sınırları ve ortak doğrudan Skia painter | TAMAMLANDI |
| 03 | Kamera ve sayısal sözleşme | TAMAMLANDI |
| 04 | Native input ve gesture state machine | TAMAMLANDI |
| 05 | Session, scheduler ve üretim viewer bağlantısı | BAŞLANIYOR |
| 06 | Muhafazakâr bounds ve mekânsal indeks | BAŞLAMADI |
| 07 | Cache, geometri hazırlığı ve kontrollü ayrıntı | BAŞLAMADI |
| 08 | Gerçek dosya açma ve parser köprüsü | BAŞLAMADI |
| 09 | Geometri, koordinat uzayları ve block | BAŞLAMADI |
| 10 | Metin, ölçülendirme ve hatch | BAŞLAMADI |
| 11 | Layout, referanslar ve viewer araçları | BAŞLAMADI |
| 12 | Yaşam döngüsü ve hata kurtarma | BAŞLAMADI |
| 13 | Gerçek uygulama doğruluğu ve performans kabulü | BAŞLAMADI |
| 14 | CI ve sürüm kanıtı | BAŞLAMADI |

---

### Aşama 01 Raporu

Aşama: 01 — Güncel kaynak tabanı ve ölçüm  
Durum: TAMAMLANDI  
Başlangıç kaynak manifesti ve HEAD: `artifacts/viewer-stability/stage01/source-baseline-manifest.json`, HEAD `bbaf7bf84148c16a6d411ffe653c14771ee45848`  
Son HEAD: `b9ab591fee44400bd0dd34f73fa9a786cc866861`  
Değişen dosyalar:
- `MobilDwg.sln`
- `docs/ARCHITECTURE.md`
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Rendering/Performance/ViewportTelemetry.cs`
- `tests/MobilDwg.Architecture.Tests/Program.cs`
- `tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj`
- `tests/MobilDwg.Integration.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.App/MainPage.cs`, `src/MobilDwg.Rendering/Scene/RenderScene.cs`, `release/SHA256SUMS.txt` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış: Gerçek CAD dosya açma/çözümleme harness'ı (`MobilDwg.Integration.Tests`), 4096 örnekli sıfır-tahsisli telemetri halka tamponu (`ViewportTelemetry`) ve tüm aşamaları denetleyen `scripts/viewer-stability-gate.ps1` kuruldu.  
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 01` (exit code: 0)  
Fixture / APK / lock hash'leri:
- `fixtures/public/synthetic/synthetic_turkish_basic_ac1015.dxf`
- `artifacts/stage03/synthetic_turkish_basic_ac1015.dwg`
- `artifacts/viewer-stability/stage01/source-baseline-manifest.json`  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage01/gate-summary.txt`
- `artifacts/viewer-stability/stage01/architecture-tests.log`
- `artifacts/viewer-stability/stage01/core-tests.log`
- `artifacts/viewer-stability/stage01/rendering-tests.log`
- `artifacts/viewer-stability/stage01/integration-tests.log`
- `artifacts/viewer-stability/stage01/benchmark-classification.md`  
Geçmeyen veya çalıştırılamayan koşullar: Yok. (Fiziksel cihaz testleri Aşama 13 kapsamında yapılacaktır.)  
Bir sonraki aşama: Aşama 02 — Ortak painter ve doğrudan Skia yüzeyi  

---

### Aşama 02 Raporu

Aşama: 02 — Ortak painter ve doğrudan Skia yüzeyi  
Durum: TAMAMLANDI  
Son HEAD: `d96c294d13540eb487f9855590983cf434a94ec3` (commit: `refactor(render): introduce audited direct Skia painter`)  
Değişen dosyalar:
- `Directory.Packages.props`
- `compliance/DEPENDENCY_EVIDENCE.md`
- `compliance/stage02-package-manifest.json`
- `docs/ARCHITECTURE.md`
- `scripts/stage02-audit-packages.py`
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.App/MobilDwg.App.csproj`
- `src/MobilDwg.App/packages.lock.json`
- `src/MobilDwg.App/MauiProgram.cs`
- `src/MobilDwg.App/Viewer/CadViewportView.cs`
- `src/MobilDwg.App/Viewer/ViewerHostingExtensions.cs`
- `src/MobilDwg.Rendering/Compliance/CadFinalRcAuditor.cs`
- `src/MobilDwg.Rendering/Compliance/CadReleaseRcAuditor.cs`
- `src/MobilDwg.Rendering/Skia/RenderFrameContext.cs`
- `src/MobilDwg.Rendering/Skia/SkiaCadRenderer.cs`
- `src/MobilDwg.Rendering/Skia/SkiaScenePainter.cs`
- `src/MobilDwg.Rendering/Viewer/RenderSnapshot.cs`
- `tests/MobilDwg.Architecture.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.App/MainPage.cs`, `src/MobilDwg.Rendering/Scene/RenderScene.cs`, `release/SHA256SUMS.txt` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Doğrudan Skia çizim motoru (`SkiaScenePainter.DrawFrame`) ayrıştırıldı; ara bitmap ve PNG/JPEG kodlama maliyeti olmadan doğrudan GPU destekli SKCanvas'a çizim yapılıyor.
- `CadViewportView` (MAUI ContentView) ve `.UseCadViewport()` köprüsü kuruldu. SKGLView birincil, SKCanvasView güvenli yedekleme olarak bağlandı. `IgnorePixelScaling=false`, `HasRenderLoop=false`, `EnableTouchEvents=false` parametreleri garanti altına alındı.
- Paket sürümleri `SkiaSharp.Views.Maui.Controls 4.151.1` CPM ve `packages.lock.json` ile donduruldu.
- Mimari kuralı `AssertAppSkiaBridge` doğrulamasıyla App projesinin SkiaSharp'a yalnızca `CadViewportView` ve `ViewerHostingExtensions` üzerinden temas etmesi sağlandı.
Çalıştırılan gerçek komutlar ve exit code:
- `python scripts/stage02-audit-packages.py` (exit code: 0, STAGE02_PACKAGE_AUDIT_PASS)
- `dotnet restore --locked-mode src/MobilDwg.App/MobilDwg.App.csproj` (exit code: 0)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 02` (exit code: 0, VIEWER_STABILITY_STAGE02_PASS)  
Fixture / APK / lock hash'leri:
- `src/MobilDwg.App/packages.lock.json`
- `compliance/stage02-package-manifest.json`  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage02/gate-summary.txt`
- `artifacts/viewer-stability/stage02/stage02-package-audit.log`
- `artifacts/viewer-stability/stage02/app-build-android.log`
- `artifacts/viewer-stability/stage02/architecture-tests.log`
- `artifacts/viewer-stability/stage02/rendering-tests.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 03 — Kamera ve sayısal sözleşme  

---

### Aşama 03 Raporu

Aşama: 03 — Kamera ve sayısal sözleşme  
Durum: TAMAMLANDI  
Son HEAD: `cb158f1181292fa07b7b13be6025219e917d23d9` (commit: `fix(camera): define focal manipulation and precision limits`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Rendering/Camera/Camera2D.cs`
- `src/MobilDwg.Rendering/Camera/ViewportController.cs`
- `src/MobilDwg.Rendering/Camera/ViewerZoomPolicy.cs`
- `src/MobilDwg.Rendering/Interaction/ViewportInputContracts.cs`
- `tests/MobilDwg.Rendering.Tests/Program.cs`
- `tests/MobilDwg.Rendering.Tests/Stage11ViewportGestureTests.cs`
- `tests/MobilDwg.Rendering.Tests/ViewportCameraTests.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.App/MainPage.cs`, `src/MobilDwg.Rendering/Scene/RenderScene.cs`, `release/SHA256SUMS.txt` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Hareket eden merkez (moving centroid) formülü `Camera2D.Manipulate(previousCentroid, currentCentroid, factor)` ile uygulandı. İki parmakla yakınlaştırıp kaydırırken odak noktası parmakların altından kaçmaz.
- `ViewerZoomPolicy` ile ULP tabanlı yerel sayısal hassasiyet (`minWupp = max(1e-12, 8 * ulp(M))`, `maxWupp = min(1e12, 16 * fitWupp)`) kuralları bağlandı; 5.000.000 kadastro koordinatında 1 mm (0.001) detay titremesiz çizilebilir kılındı.
- Boş sahne, tek noktalı sahne (1 birim sanal Fit) ve tek boyutlu (yatay/dikey) sahneler için Fit kararlılığı sağlandı.
- Çift dokunma (DoubleTap) her zaman odak noktasında 2× yakınlaştırma yapacak şekilde kesinleştirildi; Fit eylemi ayrı buton olarak korundu.
- 15, 30, 60 ve 120 Hz giriş hızlarında aynı hareket yolunun tam aynı geometrik sonucu ürettiği (örnekleme hızı bağımsızlığı) kanıtlandı.
- 1000 kontrollü pinch in/out döngüsünde toplam sapmanın <1e-6 px kaldığı (kriter: <= 0.5 px) doğrulandı.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE03_VIEWPORT_CAMERA_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 03` (exit code: 0, VIEWER_STABILITY_STAGE03_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage03/gate-summary.txt`
- `artifacts/viewer-stability/stage03/rendering-tests.log`
- `artifacts/viewer-stability/stage03/architecture-tests.log`
- `artifacts/viewer-stability/stage03/core-tests.log`
- `artifacts/viewer-stability/stage03/integration-tests.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 04 — Native input ve gesture state machine  

---

### Aşama 04 Raporu

Aşama: 04 — Native input ve gesture state machine  
Durum: TAMAMLANDI  
Son HEAD: `f57c810a97aaef7ee9d5e305e913a6977926bda3` (commit: `fix(input): unify native pointer packet handling`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.App/Viewer/Platforms/Android/AndroidViewportInputAdapter.cs`
- `src/MobilDwg.Rendering/Interaction/ViewportInteractionEngine.cs`
- `tests/MobilDwg.Rendering.Tests/Program.cs`
- `tests/MobilDwg.Rendering.Tests/ViewportInteractionTests.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.App/MainPage.cs`, `src/MobilDwg.Rendering/Scene/RenderScene.cs`, `release/SHA256SUMS.txt` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- MAUI Pan/Pinch yarışlarını ve 250 ms debounce gecikmelerini ortadan kaldıran tek gesture durum makinesi (`ViewportInteractionEngine`) ve Android MotionEvent adaptörü (`AndroidViewportInputAdapter`) kuruldu.
- Parmaklar eklenirken veya ayrılırken (1→2→1, 2→3→2, ID yer değiştirmesi) baseline sıçramasız yenileniyor; sonraki gerçek hareket örneği kaybolmuyor.
- Touch slop (hareket eşiği) aşıldığında o ana kadarki yer değiştirme tam bir kez uygulanıyor; slop altındaki küçük kıpırdamalarda kamera titremesi önleniyor.
- Son bırakma (UP/POINTER_UP) paketindeki koordinat farkı sıfır ise sıfır delta; yeni koordinat varsa son delta tam bir kez işleniyor; çifte commit veya birikmiş toplam yer değiştirme hatası önlendi.
- Çift dokunma Android zaman ve slop koşullarıyla tanınıp 2× yakınlaştırma yapıyor; ölçüm modu seçildiğinde çift dokunma zoom'u devre dışı kalarak tek dokunma ölçüm noktası olarak aktarılıyor.
- View sınırlarının dışına çıkan ve geri gelen hareketler kırpılmadan veya sıfırlanmadan izleniyor.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE04_VIEWPORT_INTERACTION_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 04` (exit code: 0, VIEWER_STABILITY_STAGE04_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage04/gate-summary.txt`
- `artifacts/viewer-stability/stage04/rendering-tests.log`
- `artifacts/viewer-stability/stage04/architecture-tests.log`
- `artifacts/viewer-stability/stage04/core-tests.log`
- `artifacts/viewer-stability/stage04/integration-tests.log`
- `artifacts/viewer-stability/stage04/app-build-android.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 05 — Session, scheduler ve üretim viewer bağlantısı  

---

### Aşama 05 Raporu

Aşama: 05 — Session, scheduler ve üretim viewer bağlantısı  
Durum: TAMAMLANDI  
Son HEAD: `e0d86807c49f4de7e86546ffd635118c730a42b8` (commit: `fix(viewer): render live snapshots with bounded scheduling`)  
Değişen dosyalar:
- `MobilDwg.sln`
- `docs/ARCHITECTURE.md`
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.App/MainPage.cs`
- `src/MobilDwg.App/Viewer/CadViewportView.cs`
- `src/MobilDwg.App/Viewer/Platforms/Android/AndroidFrameClock.cs`
- `src/MobilDwg.Rendering/Camera/ViewportController.cs`
- `src/MobilDwg.Rendering/Scheduling/FrameRequestGate.cs`
- `src/MobilDwg.Rendering/Viewer/CadViewerSession.cs`
- `src/MobilDwg.Rendering/Viewer/RenderSessionLease.cs`
- `tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj`
- `tests/MobilDwg.Android.Instrumentation/NativeSmokeRunner.cs`
- `tests/MobilDwg.Architecture.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs`, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- MAUI `Image` ve JPEG tabanlı `ReRenderAsync` render döngüsü kaldırıldı; yerine donanım hızlandırmalı doğrudan Skia GL/CPU yüzeyi (`CadViewportView`) bağlandı.
- Canlı oturumda tek kaynak `CadViewerSession` ve `ViewportController` bağlandı; MainPage'in `_viewportController` alanı `CadViewerSession.Controller` delegasyonuna bağlandı.
- `FrameRequestGate` ile en fazla 1 pending + 1 in-flight frame tutularak unbounded task/kuyruk oluşması engellendi.
- Snapshot kiralama (`RenderSessionLease`) ile frame render esnasında kilit (lock) tutulmadan GC tahsissiz ve mutasyonsuz çizim sağlandı.
- Android platformunda `Choreographer` vsync saati (`AndroidFrameClock`) ile ekran yenileme hızında dirty invalidation bağlandı.
- Belge/layout geçişlerinde eski native frame'in yeni belge adı altında parlamasını önlemek için `_transitionOverlay` (donanımsal geçiş örtüsü) uygulandı; ilk generation frame sunulana kadar örtü aktif kalıyor.
- GPU context hatası ve 1000 ms unresponsiveness durumunda otomatik CPU yüzeyine (`SKCanvasView`) geçiş watchdog'u eklendi.
- Ayrı test APK'sı (`tests/MobilDwg.Android.Instrumentation/`) solution'a bağlandı; `Architecture.Tests` sözleşmesi tam beş test projesiyle güncellenip kilitlendi.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0, STAGE05_DEPENDENCY_BOUNDARY_PASS)
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `dotnet build tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 05` (exit code: 0, VIEWER_STABILITY_STAGE05_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage05/gate-summary.txt`
- `artifacts/viewer-stability/stage05/architecture-tests.log`
- `artifacts/viewer-stability/stage05/core-tests.log`
- `artifacts/viewer-stability/stage05/rendering-tests.log`
- `artifacts/viewer-stability/stage05/integration-tests.log`
- `artifacts/viewer-stability/stage05/stage02-package-audit.log`
- `artifacts/viewer-stability/stage05/app-build-android.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 06 — Muhafazakâr bounds ve mekânsal indeks  

---
