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
| 03 | Kamera ve sayısal sözleşme | BAŞLANIYOR |
| 04 | Native input ve gesture state machine | BAŞLAMADI |
| 05 | Session, scheduler ve üretim viewer bağlantısı | BAŞLAMADI |
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
Son HEAD: bekliyor (commit: `refactor(render): introduce audited direct Skia painter`)  
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


