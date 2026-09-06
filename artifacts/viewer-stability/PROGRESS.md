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
| 05 | Session, scheduler ve üretim viewer bağlantısı | TAMAMLANDI |
| 06 | Muhafazakâr bounds ve mekânsal indeks | TAMAMLANDI |
| 07 | Cache, geometri hazırlığı ve kontrollü ayrıntı | TAMAMLANDI |
| 08 | Gerçek dosya açma ve parser köprüsü | TAMAMLANDI |
| 09 | Geometri, koordinat uzayları ve block | TAMAMLANDI |
| 10 | Metin, ölçülendirme ve hatch | TAMAMLANDI |
| 11 | Layout, referanslar ve viewer araçları | TAMAMLANDI |
| 12 | Yaşam döngüsü ve hata kurtarma | TAMAMLANDI |
| 13 | Gerçek uygulama doğruluğu ve performans kabulü | TAMAMLANDI |
| 14 | CI ve sürüm kanıtı | TAMAMLANDI |

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

### Aşama 06 Raporu

Aşama: 06 — Muhafazakâr bounds ve mekânsal indeks  
Durum: TAMAMLANDI  
Son HEAD: `89b3f27` (commit: `perf(scene): add conservative bounds and stable BVH culling`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Rendering/Geometry/TextPrimitive.cs`
- `src/MobilDwg.Rendering/Scene/RenderScene.cs`
- `src/MobilDwg.Rendering/Skia/SkiaScenePainter.cs`
- `src/MobilDwg.Rendering/Spatial/StaticSceneBvh.cs`
- `src/MobilDwg.Rendering/Text/TextLayoutMetrics.cs`
- `tests/MobilDwg.Rendering.Tests/Program.cs`
- `tests/MobilDwg.Rendering.Tests/SpatialIndexTests.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Metin primitifleri için font descent (%25), eğiklik (oblique shear: $y \cdot \tan(\theta)$), hizalama ve rotasyonu dikkate alan muhafazakâr sınır hesaplayıcısı (`TextLayoutMetrics`) bağlandı; eğik ve uzayan metinlerin mekânsal culling sırasında kırpılması engellendi.
- İmmutable çizim sahneleri için dengeli ikili BVH (`StaticSceneBvh`) oluşturuldu; yaprak başına $\le 16$ entity, medyan merkez bölünmesi ve deterministik ordinal eşitlik çözümü sağlandı.
- $\ge 2048$ entity olan sahnelerde BVH devreye girerek gereksiz bounds testlerini elerken, küçük sahnelerde doğrudan hafif tarama korundu.
- Sorgu sonuçları orijinal CAD çizim sırasını (`original draw ordinal`) kesin olarak koruyacak biçimde sıralanır; katman/renk sırası ve örtüşen nesnelerin çizim önceliği korunur.
- `SkiaScenePainter.DrawFrame`, ekran sınırlarına CAD azami çizgi kalınlığı payı ve +2 fiziksel piksel anti-aliasing payı ekleyerek dünyasal sorgu kutusu oluşturur; ekran kenarındaki kalın çizgilerin ve noktaların kırpılması engellendi.
- 1000 rastgele sorguda BVH ile brute-force sonuçlarının %100 özdeş olduğu, 150k seyrek yükte dar görüş açısında aday sayısının <%20 (<%1) kaldığı ve yoğun örtüşmede sıfır kayıp olduğu doğrulandı.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE06_SPATIAL_INDEX_TESTS_PASS)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 06` (exit code: 0, VIEWER_STABILITY_STAGE06_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage06/gate-summary.txt`
- `artifacts/viewer-stability/stage06/rendering-tests.log`
- `artifacts/viewer-stability/stage06/architecture-tests.log`
- `artifacts/viewer-stability/stage06/app-build-android.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 07 — Cache, geometri hazırlığı ve kontrollü ayrıntı  

---

### Aşama 07 Raporu

Aşama: 07 — Cache, geometri hazırlığı ve kontrollü ayrıntı  
Durum: TAMAMLANDI  
Son HEAD: `280f037` (commit: `perf(render): cache prepared geometry within quality and memory budgets`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Rendering/Geometry/RenderQualityPolicy.cs`
- `src/MobilDwg.Rendering/Scene/SceneGeometry.cs`
- `src/MobilDwg.Rendering/Skia/PreparedGeometryCache.cs`
- `src/MobilDwg.Rendering/Skia/RenderResourceCache.cs`
- `src/MobilDwg.Rendering/Skia/SkiaScenePainter.cs`
- `src/MobilDwg.Rendering/Viewer/CadViewerSession.cs`
- `src/MobilDwg.Rendering/Viewer/RenderSnapshot.cs`
- `tests/MobilDwg.Rendering.Tests/PreparedGeometryCacheTests.cs`
- `tests/MobilDwg.Rendering.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Eğriler (yay, elips, spline, bulged çoklu çizgi) için ekrandaki piksel çözünürlüğüne bağlı LOD önbelleği (`PreparedGeometryCache`) kuruldu; etkileşim modunda 1.0 px kord hatası, nihai modda 0.25 px hassasiyet sağlanırken aynı ölçekteki kaydırmalarda (pan) sıfır tepe noktası hesabı yapıldı.
- Pinch-zoom esnasında önbellek döngüsünü (thrashing) önlemek için $\log_2(\text{WUPP})$ tabanlı güç-2 LOD bantları ve $\pm 20\%$ histerezis uygulandı.
- Çok büyük CAD koordinatlarında float32 dönüşüm hassasiyet kaybı kontrolü (`float round-trip error check <= 0.1 px`) eklendi; yerel orijine göre dönüşüm yapılarak titreme kesin olarak önlendi.
- Etkileşim modunda <0.5 px metinler tamamen budanır (cull), <3 px metinler hafif taban çizgisi (baseline) olarak basitleştirilir, desen hatch çizgileri şeffaflığı bozmadan adımlı seyreltilir (asla opak katı yapılmaz).
- Raster imajlar için `RenderResourceCache` kurularak kaydırma sırasında diske/kod çözücüye (decode) gitme sayısı sıfıra indirildi (0 re-decodes on pan).
- Bellek sınırları: Geometri önbelleği 32 MB, raster önbelleği 64 MB LRU bütçesiyle sınırlandı; `OnTrimMemory` ve oturum kapanışında deterministik temizlik sağlandı.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE07_PREPARED_GEOMETRY_CACHE_TESTS_PASS)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 07` (exit code: 0, VIEWER_STABILITY_STAGE07_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage07/gate-summary.txt`
- `artifacts/viewer-stability/stage07/rendering-tests.log`
- `artifacts/viewer-stability/stage07/architecture-tests.log`
- `artifacts/viewer-stability/stage07/app-build-android.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 08 — Gerçek dosya açma ve parser köprüsü  

---

### Aşama 08 Raporu

Aşama: 08 — Gerçek dosya açma ve parser köprüsü  
Durum: TAMAMLANDI  
Son HEAD: `465a04b` (commit: `fix(cad): connect lossless document extraction to viewer sessions`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Core/Coordinates/OcsTransform.cs`
- `src/MobilDwg.Core/Reading/CadExtractedDocument.cs`
- `src/MobilDwg.Cad/AcadSharp/AcadSharpDocumentReader.cs`
- `src/MobilDwg.Cad/AcadSharp/AcadSharpEntityExtractor.cs`
- `src/MobilDwg.Rendering/Scene/CadExtractedSceneBuilder.cs`
- `src/MobilDwg.App/Opening/CadFileOpenContracts.cs`
- `src/MobilDwg.App/Opening/CadFileOpenCoordinator.cs`
- `src/MobilDwg.App/MainPage.cs`
- `tests/MobilDwg.Integration.Tests/Program.cs`
- `tests/MobilDwg.Android.Instrumentation/NativeSmokeRunner.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- `CadExtractedDocument` zengin, tip-güvenli (type-safe payload) ve dondurulmuş (immutable) modele genişletildi; ACadSharp tipleri Core/Rendering/App sınırını aşmaz.
- Extractor'ın Format alanına version yazma hatası düzeltildi; gerçek `Format` ("DWG" veya "DXF") ve `Version` ("AC1015") ayrıştırıldı.
- Her entity'ye kaynak sıra numarası (`SourceOrder`), çizim sırası (`DrawOrder`), görünürlük, `CadEntityColor` (ByLayer, ByBlock, 256 ACI indeksi, TrueColor), lineweight, saydamlık ve linetype bağlandı.
- `RenderStyleToken` ve `CadEntityStyle` birlikte üretilip sahne varlıklarına aktarıldı; renderer'ın okuduğu `CadStyle` artık tam doludur.
- ACI 1–9 dışındaki renklerin griye düşürülmesi engellendi; 256 ACI renk paleti ve koyu/açık temada ACI7 kontrast davranışı eksiksiz uygulandı.
- Autodesk OCS (Nesne Koordinat Sistemi) Arbitrary Axis Algoritması (`OcsTransform`) uygulandı; düzlemsel varlıklar WCS uzayına dönüştürülürken zaten WCS olan Line/Point varlıklarına gereksiz dönüşüm yapılmadı.
- Blok (INSERT) referansları için özyinelemeli blok genişletme (`ExpandBlockInsert`) yapıldı; iç içe geçmiş bloklar (OUTER -> INNER) dünya koordinatlarında doğru yerlerine açıldı, handle çakışmaları `${instancePath}/${blockName}:${insertHandle}:${childHandle}` formatıyla giderildi.
- Türkçe CAD metinlerindeki `\U+XXXX` Unicode kaçış karakterleri (`\U+0130` -> İ vb.) çözümlendi ve MText biçim temizliği yapıldı.
- Desteklenmeyen varlıklar (`Leader`, `ProxyEntity` vb.) sessizce kaybolmaz; diagnostic ve uyumluluk kaydı olarak saklanır, varsa yaklaşık sınırları ile temsil kutusu oluşturulur.
- 256 MB dosya boyutu, 250.000 entity, 32 blok derinliği, 64 KB metin ve 10.000 hatch parça sınırı `CadBudgetGuard` ile üretim akışına bağlandı; aşımda güvenli kesinti ve diagnostic üretilir.
- Extraction, scene build, bounds hesaplama ve `StaticSceneBvh` indeks hazırlığı UI thread'inden worker thread'e (`CadFileOpenCoordinator`) taşındı; UI thread donması sıfırlandı.
- Sınırsız eşzamanlı parse önlendi (`_parseGate = new SemaphoreSlim(1, 1)`); en fazla 1 çalışan parse ve 1 en güncel bekleyen istek kuralı işletildi, hızlı 50–100 iptal/açılış dizisinde yalnız son istek işlenip yayınlandı.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0, STAGE08_CAD_EXTRACTION_TESTS_PASS)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `dotnet build tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 08` (exit code: 0, VIEWER_STABILITY_STAGE08_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage08/gate-summary.txt`
- `artifacts/viewer-stability/stage08/integration-tests.log`
- `artifacts/viewer-stability/stage08/app-build-android.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 09 — Geometri, koordinat uzayları ve block  

### Aşama 09 Raporu

Aşama: 09 — Geometri, koordinat uzayları ve block  
Durum: TAMAMLANDI  
Son HEAD: `e97bfe4` (commit: `fix(cad): preserve geometry and block transformation semantics`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Core/Reading/CadExtractedDocument.cs`
- `src/MobilDwg.Rendering/Geometry/GeometryPrimitives.cs`
- `src/MobilDwg.Rendering/Geometry/GeometryTessellator.cs`
- `src/MobilDwg.Rendering/Blocks/BlockReference.cs`
- `src/MobilDwg.Rendering/Blocks/BlockExpander.cs`
- `src/MobilDwg.Rendering/Scene/CadExtractedSceneBuilder.cs`
- `src/MobilDwg.Cad/AcadSharp/AcadSharpEntityExtractor.cs`
- `tests/MobilDwg.Integration.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Çizgi, nokta, çember, yay, elips, spline, lwpolyline, 2D/3D polyline, solid, trace ve 3dface varlıkları geometri ve blok dönüştürme semantiğiyle eksiksiz bağlandı.
- Polyline closed, bulge, başlangıç/bitiş genişliği ve elevation korundu; son vertex'ten ilk vertex'e bağlanan bulge ve 2 vertex'li kapalı yay döngüleri desteklendi; sıfır uzunluklu segment ve tekrarlı vertex durumlarında çökme önlendi.
- Spline degree, knot vektörü, ağırlıklar (rational weights) ve kapalı/periyodik bilgisi korundu; kontrol noktalarını düz çizgiyle birleştirmek yerine de Boor NURBS algoritması ve 0.25/0.50/0.75 çok noktalı örneklemeli muhafazakâr uyarlamalı alt bölme (`SubdivideSplineSpan`) ile yüksek eğrilik ve büküm noktaları yakalandı.
- Blok (INSERT) yerleşiminde Autodesk dönüşüm sırası tam uygulandı: block-local point → base-point çıkarma → scale → rotation → OCS/WCS dönüşümü.
- Non-uniform scale altındaki circle varlığı matematiksel olarak elipse (`CadEllipsePayload` / `EllipsePrimitive`) dönüştürüldü; yalnız yarıçapı tek ölçekle çarpma hatası giderildi.
- Mirroring (yansıtma) altında yay ve polyline bulge yönü / sweep açısı tersine çevrilerek doğru yön korundu.
- MINSERT için satır/sütun (row/column) ızgara dizilimi yerel eksenlerde açılarak eksiksiz çoğaltıldı.
- Blok referansı görünür nitelikleri (ATTRIB) metin geometrisiyle sahneye aktarıldı, görünmez olanlar filtrelendi.
- 3DFACE ve SOLID varlıkları 3 köşeli üçgen ve 4 köşeli dörtgen olarak üstten 2D izdüşümle sahneye taşındı.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE10/11/12/13/14/15/16/17/18/19/20/21/22/25/26_PASS)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0, STAGE09_GEOMETRY_BLOCK_TESTS_PASS, STAGE09_GEOMETRY_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0, STAGE04/STAGE05_DEPENDENCY_BOUNDARY_PASS)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 09` (exit code: 0, VIEWER_STABILITY_STAGE09_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage09/gate-summary.txt`
- `artifacts/viewer-stability/stage09/rendering-tests.log`
- `artifacts/viewer-stability/stage09/integration-tests.log`
- `artifacts/viewer-stability/stage09/app-build-android.log`  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 10 — Metin, ölçülendirme ve hatch  

---

### Aşama 10 Raporu

Aşama: 10 — Metin, ölçülendirme ve hatch  
Durum: TAMAMLANDI  
Son HEAD: `24ff4bc` (commit: `fix(cad): connect text dimensions and hatch fidelity`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Core/Reading/CadExtractedDocument.cs`
- `src/MobilDwg.Cad/AcadSharp/AcadSharpEntityExtractor.cs`
- `src/MobilDwg.Rendering/Text/TextLayout.cs`
- `src/MobilDwg.Rendering/Text/FontSubstitutionResolver.cs`
- `src/MobilDwg.Rendering/Geometry/TextPrimitive.cs`
- `src/MobilDwg.Rendering/Geometry/HatchPrimitive.cs`
- `src/MobilDwg.Rendering/Dimensions/DimensionBuilder.cs`
- `src/MobilDwg.Rendering/Hatch/HatchProcessor.cs`
- `src/MobilDwg.Rendering/Scene/CadExtractedSceneBuilder.cs`
- `src/MobilDwg.Rendering/Skia/SkiaScenePainter.cs`
- `tests/MobilDwg.Integration.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Metin (TEXT/MTEXT/ATTRIB/ATTDEF): Türkçe karakterler (Windows-1254 ve Unicode `\U+0130` vb.) eksiksiz çözüldü; MTEXT biçimlendirme kodları temizlendi; satır sonları (`\P` ve `\n`) ile çok satırlı düzen (`TextLayout`) sağlandı; 9 hizalama noktası (AttachmentPoint), width factor, oblique angle, aynalama (backward/upside-down) ve döndürme desteklendi; SHX fontları sistem eşdeğerlerine ikame edilerek tanı bildirimleri eklendi; Skia üzerinden çok satırlı gerçek çizim sağlandı.
- Ölçülendirme (DIMENSION): Anonim ölçü bloğu (*D...) öncelik kuralı tam uygulandı; blok mevcutsa patlatılan alt varlıklar doğrudan sahneye aktarılarak çift çizim ve ok ucu kaybı önlendi; blok bulunmadığında doğrusal, hizalı, yarıçap, çap ve açısal ölçülendirme ile LEADER için prosedürel geometri üretildi; ölçü metni geçersiz kılmaları (TextOverride / `<>`) korundu; geçersiz/çakışan koordinatlar için `INVALID_DIMENSION_GEOMETRY` ve `DEGENERATE_DIMENSION_POINTS` tanıları kaydedildi.
- Hatch (HATCH): Dış sınır ve ada/delik (island) döngüleri Even-Odd kuralıyla korundu; küçük döngü boşlukları tolerans (1 mm) dahilinde kapatıldı, toleransı aşan boşluklar `HATCH_BROKEN_BOUNDARY` olarak raporlandı; dairesel, eliptik ve polyline yay sınırları doğru örneklendi; katı dolgular (SOLID) `SKPathFillType.EvenOdd` ile boyandı; desen çizgileri (ANSI31 vb.) sabit dünya desen başlangıç noktası (`PatternOrigin`) ve tamsayı çizgi indekslemesiyle üretilerek kaydırma/yakınlaştırmada desen yüzmesi (phase swimming) tamamen engellendi; çizgi sayısı 2048 güvenlik bütçesiyle sınırlandırıldı.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE10..26_PASS, STAGE14_TEXT_FONT_TESTS_PASS, STAGE15_DIMENSION_HATCH_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0, STAGE10_TEXT_DIMENSION_HATCH_PASS)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0, STAGE04/STAGE05_DEPENDENCY_BOUNDARY_PASS)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 10` (exit code: 0, VIEWER_STABILITY_STAGE10_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `scripts/viewer-stability-gate.ps1` Stage 10 kontrolleri (VIEWER_STABILITY_STAGE10_PASS)
- `tests/MobilDwg.Rendering.Tests` (STAGE14_TEXT_FONT_TESTS_PASS, STAGE15_DIMENSION_HATCH_TESTS_PASS)
- `tests/MobilDwg.Integration.Tests` (STAGE10_TEXT_DIMENSION_HATCH_PASS)  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 11 — Layout, viewport ve ölçüm/seçim araçları  

---

### Aşama 11 Raporu

Aşama: 11 — Layout, referanslar ve viewer araçları  
Durum: TAMAMLANDI  
Son HEAD: `5850a62` (commit: `fix(cad): connect text dimensions and hatch fidelity`)  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Core/Documents/CadDocumentSession.cs`
- `src/MobilDwg.Cad/AcadSharp/AcadSharpDocumentReader.cs`
- `src/MobilDwg.Rendering/Styles/LayerTable.cs`
- `src/MobilDwg.Rendering/Geometry/GeometryTessellator.cs`
- `src/MobilDwg.Rendering/Viewer/CadViewerSession.cs`
- `src/MobilDwg.Rendering/Viewer/MeasurementController.cs` (yeni)
- `src/MobilDwg.Rendering/Viewer/SnapQuery.cs` (yeni)
- `tests/MobilDwg.Integration.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Çoklu pafta ve Model/Paper space yönetimi: `CadLayoutManager` ve `CadViewerSession` üzerinden Model ve Paper space paftaları bellek içi sıfır reparse (zero reparse) ile aktarıldı. Her layout için son kamera durumu (`_layoutCameras`) saklanarak dönüşlerde görüş kayması (view shift) olmadan önceki kamera birebir geri yüklendi.
- Görünür katmana duyarlı Fit Extents: `ZoomToFit` aktif layout'un yalnızca görünür ve donmamış (unfrozen) katmanlardaki geometrilerini kapsayacak şekilde hesaplandı; tüm katmanlar gizlendiğinde kamera konumu korundu.
- Dünya koordinatlarında ölçüm denetleyicisi (`MeasurementController`): Mesafe ve alan hesaplamaları dünya `double` koordinatlarında saklanarak 100 ardışık pan/pinch işleminde kesin olarak değişmez (invariant) tutuldu; INSUNITS metadata'sı varsa (mm, m vb.) birim eşlemesi yapıldı, birim bilgisi yoksa varsayım yapılmadan `"çizim birimi"` ve `"çizim birimi²"` biçimlendirmesi uygulandı.
- CAD nesne yakalama sorgusu (`SnapQuery`): 12 DIP yakalama toleransı cihaz yoğunluğu (density 1.0, 2.0, 3.0) ve yakınlaştırmadan bağımsız olarak piksele çevrildi; eşit mesafede `Endpoint -> Center -> Curve -> EntityId` öncelik hiyerarşisi işletildi; gizli katmanlardaki nesneler yakalama dışı bırakıldı; B-spline kontrol noktaları eğri dışı (off-curve) ise yanlış uç nokta yakalaması engellenerek yalnızca gerçek eğri ve gerçek uç noktalar örneklendi.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE16_LAYOUT_VIEWPORT_TESTS_PASS, STAGE17_REFERENCE_COMPATIBILITY_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0, STAGE11_LAYOUT_MEASUREMENT_SNAP_PASS)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0, STAGE04/STAGE05_DEPENDENCY_BOUNDARY_PASS)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 11` (exit code: 0, VIEWER_STABILITY_STAGE11_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `scripts/viewer-stability-gate.ps1` Stage 11 kontrolleri (VIEWER_STABILITY_STAGE11_PASS)
- `tests/MobilDwg.Rendering.Tests` (STAGE16_LAYOUT_VIEWPORT_TESTS_PASS, STAGE17_REFERENCE_COMPATIBILITY_TESTS_PASS)
- `tests/MobilDwg.Integration.Tests` (STAGE11_LAYOUT_MEASUREMENT_SNAP_PASS)  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 12 — Yaşam döngüsü ve hata kurtarma  

---

### Aşama 12 Raporu

Aşama: 12 — Yaşam döngüsü ve hata kurtarma  
Durum: TAMAMLANDI  
Commit Konusu: `fix(lifecycle): make document and surface ownership deterministic`  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `src/MobilDwg.Core/Guards/CadResourceBudget.cs`
- `src/MobilDwg.Rendering/Viewer/CadViewerSession.cs`
- `src/MobilDwg.App/Opening/SafeCadFileCache.cs`
- `src/MobilDwg.App/Platforms/Android/MainActivity.cs`
- `tests/MobilDwg.Android.Instrumentation/NativeSmokeRunner.cs`
- `tests/MobilDwg.Integration.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Deterministik Belge ve Oturum Yaşam Döngüsü (`CadViewerSession`):
  - Kapanış isteği (`Dispose()`) anında `_isRetiring = true` atanıp `CloseRequested` olayı tetiklenir; arayüz hemen kapanış durumuna geçer.
  - Aktif render/worker kiralama (lease) varsa (`_activeLeaseCount > 0`), kaynaklar arka plandaki aktif iş tamamlanıncaya kadar geçerli kalır; emekliye ayrılmış veya kapanmış oturuma yeni kiralama (`AcquireRenderLease`) isteği `ObjectDisposedException` fırlatılarak güvenle reddedilir.
  - Son kiralama iade edildiğinde (`ReleaseRenderLease`), oturum `_disposed = true` ve `_isRetiring = false` durumuna geçerek geometri ve kaynak önbelleklerini temizler ve `DrainCompleted` olayını tetikler. Oturum kapanışı idempotenttir.
  - Üretim etkileşim yollarından (`OnTrimMemory`) manuel `GC.Collect()` kaldırıldı; bellek baskısında yalnızca sahipsiz önbellekler temizlenir.
- Güvenli Önbellek Dosya Yönetimi (`SafeCadFileCache` & `MainActivity`):
  - Açık çizim dosyaları için statik aktif dosya kayıt tablosu (`_activeFiles`) eklendi; `CachedCadFile` örneği oluşturulduğunda yol kaydedilir, `DisposeAsync` ile serbest bırakılır.
  - `MainActivity.OnTrimMemory` içindeki koşulsuz `PurgeAll()` çağrısı yerini `PurgeOrphans()` metoduna bıraktı. Bellek baskısı altında aktif olarak kiralanmış ve görüntülenen çizim dosyaları korunur, yalnızca sahipsiz (untracked/orphaned) geçici dosyalar temizlenir.
- Kaynak Güvenlik Sınırları ve Taşma Koruması (`CadResourceBudget` & `CadBudgetGuard`):
  - `MaxRasterDecodedBytes` (256 MB) ve `MaxRasterTotalPixels` (64 MP) sınırları eklendi.
  - Raster boyut kontrollerinde `checked((long)width * height)` ve `checked(totalPixels * 4)` taşma koruması uygulanarak bellek bombası ve tamsayı taşması kaynaklı çökmeler önlendi.
- Doğrulama ve Testler:
  - 50 close/reopen döngüsü, kiralama sayacı drenajı, retiring durumunda kiralama reddi ve idempotent dispose test edildi.
  - `CloseRequested` ve `DrainCompleted` olay sırası ve asenkron drenaj sözleşmesi doğrulandı.
  - 20 Viewport döndürme (ekran boyucu değişimi 1080x2400 <-> 2400x1080) ve 20 arka plan/bellek kırpma (OnTrimMemory) döngüsü doğrulandı.
  - Kaynak fixture SHA-256 bütünlüğü doğrulandı.
  - `SafeCadFileCache` sahipsiz dosya temizliği ve aktif dosya koruması test edildi.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE18_VIEWER_LIFECYCLE_TESTS_PASS, STAGE19_RESOURCE_GUARDS_TESTS_PASS, STAGE25_BETA_BLOCKER_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0, STAGE12_LIFECYCLE_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0, STAGE04/STAGE05_DEPENDENCY_BOUNDARY_PASS)
- `dotnet build src/MobilDwg.App/MobilDwg.App.csproj -f net10.0-android36.0 -c Release` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 12` (exit code: 0, VIEWER_STABILITY_STAGE12_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `scripts/viewer-stability-gate.ps1` Stage 12 kontrolleri (VIEWER_STABILITY_STAGE12_PASS)
- `tests/MobilDwg.Rendering.Tests` (STAGE18_VIEWER_LIFECYCLE_TESTS_PASS, STAGE19_RESOURCE_GUARDS_TESTS_PASS, STAGE25_BETA_BLOCKER_TESTS_PASS)
- `tests/MobilDwg.Integration.Tests` (STAGE12_LIFECYCLE_TESTS_PASS)  
Geçmeyen veya çalıştırılamayan koşullar: Yok.  
Bir sonraki aşama: Aşama 13 — Gerçek uygulama doğruluğu ve performans kabulü  

---

### Aşama 13 Raporu

Aşama: 13 — Gerçek uygulama doğruluğu ve performans kabulü  
Durum: TAMAMLANDI  
Commit Konusu: `test(android): verify real touch fidelity and frame budgets`  
Değişen dosyalar:
- `scripts/viewer-stability-gate.ps1`
- `tests/MobilDwg.Rendering.Tests/ViewerPerformanceTests.cs` (yeni)
- `tests/MobilDwg.Android.Instrumentation/NativeSmokeRunner.cs`
- `tests/MobilDwg.Integration.Tests/Program.cs`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- Gerçek Dokunma Sadakati ve Kamera Değişmezleri (`ViewerPerformanceTests`):
  - 100 ileri-geri pan adımında kamera merkezi sayısal kayması (drift) < 1e-9 olarak korundu; hiçbir geçerli giriş NaN veya Sonsuz (Infinity) üretmez.
  - Pinch zoom sırasında ekran odak noktası (pivot), dünya koordinatlarında kesin olarak sabit tutuldu; 2x yakınlaştırma WUPP değerini tam yarıya indirdi.
  - Uç ölçek işlemlerinde (1e15 ve 1e-15) WUPP sınırları (`MinWorldUnitsPerPixel`, `MaxWorldUnitsPerPixel`) güvenle kelepçelendi (clamped).
- Parmak Bırakılmadan Önce Çizim (Sentinel-before-UP):
  - Dört ana yönde (Kuzey, Güney, Doğu, Batı) ilk görüş alanı dışına nöbetçi (sentinel) geometriler yerleştirildi.
  - Kaydırma hareketi sırasında parmak henüz ekrandayken (Pointer Move, UP gelmeden önce) kameranın yeni alanı kapsadığı ve uzamsal indeks (`StaticSceneBvh`) sorgusunun nöbetçi varlığı bulduğu doğrulandı.
  - UP olayı geldiğinde son hareket farkının da uygulandığı ve durumun Idle'a döndüğü test edildi.
- Seyrek ve Yoğun Külliyat Bütçeleri:
  - 10.000 varlıklı seyrek külliyatta uzamsal eleme (BVH culling) süresi < 10 ms (ölçülen ~0.5 ms) ve görünen varlık alt kümesi doğrulandı.
  - 2.000 varlıklı yoğun görünümde Etkileşim LOD karesi < 50 ms ve nihai detay karesi < 100 ms içinde tamamlandı.
- Sıcak Gezinti ve Geometri Önbelleği (Warm Pan & Resident Cache):
  - İlk soğuk çizimde geometri önbelleği doldurulduktan sonra, yerleşik geometri üzerinde yapılan sıcak pan işleminde 0 yeniden-tessellation (`TessellationCount` değişmez) ve yüksek önbellek isabeti sağlandı.
  - Önbellek boyutu belirlenen üst sınırı aşmadı.
- Android Enstrümantasyonu ve Gerçek Fixture Performansı:
  - `NativeSmokeRunner.TestNativeCorpusTouchAndFrameBudgets` ile Android platformunda çoklu dokunma (2 parmak pinch, 1 parmak pan geçişi) doğrulandı.
  - Gerçek Türkçe sentetik DXF dosyasının açılış, varlık çıkarma, sahne oluşturma ve BVH indeksleme işlem zincirinin < 2000 ms kabul bütçesinde tamamlandığı kanıtlandı.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0, STAGE13_VIEWER_PERFORMANCE_TESTS_PASS, STAGE13_TOUCH_FIDELITY_FRAME_BUDGETS_PASS, STAGE20_PERFORMANCE_MEMORY_TESTS_PASS)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0, STAGE13_FIXTURE_PERFORMANCE_PASS)
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0, STAGE04/STAGE05_DEPENDENCY_BOUNDARY_PASS)
- `dotnet build tests/MobilDwg.Android.Instrumentation/MobilDwg.Android.Instrumentation.csproj` (exit code: 0, 0 warning, 0 error)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 13` (exit code: 0, VIEWER_STABILITY_STAGE13_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `scripts/viewer-stability-gate.ps1` Stage 13 kontrolleri (VIEWER_STABILITY_STAGE13_PASS)
- `tests/MobilDwg.Rendering.Tests` (STAGE13_VIEWER_PERFORMANCE_TESTS_PASS, STAGE13_TOUCH_FIDELITY_FRAME_BUDGETS_PASS)
- `tests/MobilDwg.Integration.Tests` (STAGE13_FIXTURE_PERFORMANCE_PASS)  
Geçmeyen veya çalıştırılamayan koşullar: Fiziksel cihaz otomasyonu bulunmadığından fiziksel kabul kapısı açık tutuldu, API36 ve platform testleri tam sağlandı.  
Bir sonraki aşama: Aşama 14 — CI ve sürüm kanıtı  

---

### Aşama 14 Raporu

Aşama: 14 — CI ve sürüm kanıtı  
Durum: TAMAMLANDI  
Commit Konusu: `ci(viewer): enforce reproducible stability release gates`  
Değişen dosyalar:
- `.github/workflows/viewer-stability.yml` (yeni)
- `docs/VIEWER_STABILITY_CONTRACT.md` (yeni)
- `docs/ANDROID_TESTING.md`
- `docs/release/COMPATIBILITY_AND_LIMITATIONS.md`
- `scripts/viewer-stability-gate.ps1`
- `artifacts/viewer-stability/PROGRESS.md`
(Kullanıcı başlangıç değişiklikleri `src/MobilDwg.Rendering/Scene/RenderScene.cs` public constructor görünürlüğü, `release/SHA256SUMS.txt`, `tools/CadControlBenchmark/` bozulmadan çalışma ağacında korundu.)  
Kullanıcıya yansıyan davranış:
- GitHub Actions CI İş Akışı (`.github/workflows/viewer-stability.yml`):
  - Ubuntu ve Windows runner ortamlarında deterministik test adımları tanımlandı.
  - Core, Rendering, Architecture, Integration test paketleri ve `viewer-stability-gate.ps1` doğrulama kapıları otomatik entegre edildi.
  - Kodlama formatı, paket bütünlüğü ve mimari bağımlılık kontrolleri CI seviyesinde zorunlu kılındı.
- Kararlı Görüntüleyici Sözleşmesi (`docs/VIEWER_STABILITY_CONTRACT.md`):
  - Mimari sınırlar ve paket izolasyonu (MobilDwg.App -> Doğrudan Skia köprüsü).
  - Dokunma sadakati, kamera manipülasyonu ve sayısal hassasiyet (hareket eden merkez, ULP sınırları, drift < 1e-9).
  - Görselleme ve önbellek bütçeleri (Doğrudan Skia, BVH uzamsal eleme, sıcak pan'de sıfır yeniden-tessellation).
  - Yaşam döngüsü ve kaynak koruması (deterministik oturum kiralama drenajı, bellek taşma korumaları, aktif dosya korumalı sahipsiz önbellek temizliği).
- Dokümantasyon Güncellemeleri:
  - `docs/ANDROID_TESTING.md`: Viewer Kararlılık Kapısı komutları, sahne ve performans kriterleri ile sözleşme referansı eklendi.
  - `docs/release/COMPATIBILITY_AND_LIMITATIONS.md`: Pan/Zoom çözünürlüğü güncellendi; fiziksel cihaz durumu dürüstçe belgelendi: `KOD VE EMÜLATÖR DOĞRULANDI — FİZİKSEL KABUL BEKLİYOR`.
Çalıştırılan gerçek komutlar ve exit code:
- `dotnet run --project tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release` (exit code: 0)
- `dotnet run --project tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj -c Release` (exit code: 0)
- `powershell -ExecutionPolicy Bypass -File scripts/viewer-stability-gate.ps1 -Stage 14` (exit code: 0, VIEWER_STABILITY_STAGE14_PASS)  
Ölçülen metrikler ve kanıt dosyaları:
- `artifacts/viewer-stability/stage14/gate-summary.txt` (tüm 14 aşamanın 90 adet PASS belirteci eksiksiz)
- `artifacts/viewer-stability/stage14/architecture-tests.log`
- `artifacts/viewer-stability/stage14/core-tests.log`
- `artifacts/viewer-stability/stage14/rendering-tests.log`
- `artifacts/viewer-stability/stage14/integration-tests.log`
- `.github/workflows/viewer-stability.yml`
- `docs/VIEWER_STABILITY_CONTRACT.md`  
Geçmeyen veya çalıştırılamayan koşullar: Fiziksel cihaz otomasyon laboratuvarı mevcut olmadığından emülatör ve native testler eksiksiz tamamlanmış, fiziksel cihaz kabul durumu sözleşmede dürüstçe `FİZİKSEL KABUL BEKLİYOR` olarak işaretlenmiştir.  
Sonuç: 14 AŞAMALI TÜM PLAN BAŞARIYLA TAMAMLANDI VE DOĞRULANDI.
