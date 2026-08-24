# AŞAMA 04 Evidence — minimal solution ve mimari sınırlar

Tarih: 2026-08-24

Durum: `DONE`

Başlangıç main revision: `a18480f55c76658027ba44ade33d1b88c9d4d6d8`.

AŞAMA 01 dış cihaz kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır.

## Tamamlanan yapı

Production project set — tam olarak dört proje:

- `src/MobilDwg.Core`
- `src/MobilDwg.Cad`
- `src/MobilDwg.Rendering`
- `src/MobilDwg.App`

Test project set — tam olarak üç proje:

- `tests/MobilDwg.Core.Tests`
- `tests/MobilDwg.Rendering.Tests`
- `tests/MobilDwg.Architecture.Tests`

Solution: `MobilDwg.sln`.

Repo-geneli compile policy: `Directory.Build.props` ile nullable/implicit usings/warnings-as-errors/deterministic build.

## Dependency yönü

- `MobilDwg.Core`: ProjectReference yok, PackageReference yok.
- `MobilDwg.Cad`: yalnız `MobilDwg.Core`.
- `MobilDwg.Rendering`: yalnız `MobilDwg.Core`.
- `MobilDwg.App`: `Core`, `Cad`, `Rendering`.
- Stage 04 production csproj'lerinde doğrudan PackageReference yok.
- Core kaynaklarında MAUI/SkiaSharp/ACadSharp bağımlılığı yok.
- App parser entity veya Skia type'ına doğrudan bağlanmıyor.

AŞAMA 04'te ACadSharp/SkiaSharp/MAUI concrete dependency production project graph'a eklenmedi. `MobilDwg.App` platform-bağımsız composition boundary olarak başlatıldı; AŞAMA 06'da aynı proje MAUI/Android shell'e dönüştürülebilir, beşinci production proje açılmaz.

Ayrıntı: `docs/ARCHITECTURE.md`.

## Kontratlar

Core içinde tanımlandı:

- `ICadDocumentReader`
- `CadDocumentSession`
- `ICadDocumentHandle`
- `CadDocumentMetadata`
- diagnostics ve compatibility kayıtları
- `IRenderSceneBuilder`
- `ICadRenderer`
- `IRenderScene`
- `IRenderSurface`
- `RenderViewport`

`CadDocumentSession` parser-specific handle'ın sahibidir ve `IAsyncDisposable` üzerinden handle'ı idempotent biçimde tam bir kez dispose eder. Concrete parser entity UI boundary'sine sızmaz.

## Cancellation/progress doğruluğu

Reader capability modeli gerçek desteği ayrı ayrı ilan eder:

Cancellation:

- `None`
- `BeforeStartOnly`
- `Cooperative`

Progress:

- `None`
- `StagesOnly`
- `Fractional`

Adapter cooperative parser abort sağlamıyorsa `Cooperative` ilan edemez. Gerçek fraction bilinmiyorsa `CadReadProgress.Fraction = null`; sahte yüzde üretilmez. Fraction değeri varsa `[0,1]` dışı değer reddedilir.

## Test modeli

Yeni test framework dependency'si eklenmedi. Üç deterministic executable harness CI'da çalıştırılır:

- Core contract/session ownership testleri.
- Render contract/viewport invariant testleri.
- Architecture project-count/reference/package/forbidden-dependency testleri.

Script: `scripts/stage04-test.sh`.

## İlk CI hatası ve düzeltme

Stage 04 run #1, solution kodundan önce repo `global.json` içindeki workload set `10.0.400` temiz runner'da kurulu olmadığı için `MSB4242` ile FAIL oldu.

Bu hata architecture kodu değil repo-geneli workload pin davranışıydı. Workflow'a önce `dotnet workload install maui-android` ve exact workload-set doğrulaması eklendi. Production projelerine MAUI PackageReference eklenmedi.

## Final Stage 04 CI

Workflow: `Stage 04 Architecture`.

- Run ID: `32755230695` / #2
- Head: `00fc7d5e04e521b421a9e4646bff9e6a7c82d6d1`
- Sonuç: `SUCCESS`
- exact .NET SDK `10.0.400`: PASS
- workload set `10.0.400` + `maui-android`: PASS
- solution restore: PASS
- Release build: PASS — `0 Warning(s)`, `0 Error(s)`
- Core contract harness: `STAGE04_CORE_CONTRACT_TESTS_PASS`
- Render contract harness: `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- Architecture harness: `STAGE04_ARCHITECTURE_TESTS_PASS`
- Final T0 marker: `STAGE04_T0_PASS`

Stage 04 için ayrı binary artifact gerekmiyor; kanıt source + workflow logs + merge revision'dır.

## Regresyon kapıları

Aynı final PR head üzerinde:

- `Stage 02 Dependency Audit` run `32755230688` / #17: `SUCCESS`; artifact `9530581424`, digest `sha256:58da9bfdb4ad4c59672b368673d0f27cfbd2e7b3a7b8157c003618e9671d4593`.
- `Stage 01 Toolchain Smoke` run `32755230683` / #36: `SUCCESS`; Debug/Release/manifest/artifact PASS; artifact `9530813909`, digest `sha256:bf31fd5a4aa2268e768137f5fe19dfe8b37f13fb206eb4a616e07b77b1d2382e`.

Stage 01 CI fiziksel telefon install/launch kanıtı değildir; dış kapılar açık kalır.

## Merge

PR #6: `feat: establish Stage 04 minimal architecture`

- Doğrulanmış head: `00fc7d5e04e521b421a9e4646bff9e6a7c82d6d1`
- Merge sonucu: başarılı
- Merge commit: `c01311ccb5c82b7bac023b24ae6a8000ae4655af`

## Çıkış değerlendirmesi

AŞAMA 04 çıkış kriterleri sağlandı:

- dört production / üç test proje var;
- gerekli reader/session/render/diagnostics/compatibility kontratları var;
- Core BCL-only ve MAUI/Skia/ACadSharp bağımsız;
- cancellation/progress capability modeli gerçek destek düzeyini yanlış temsil etmiyor;
- dependency yönleri otomatik test ediliyor;
- clean restore + Release build + T0/contract/architecture testleri PASS.

AŞAMA 04 `DONE`. Sonraki bağımsız çalışma aşaması AŞAMA 05'tir; aynı kullanıcı turunda AŞAMA 05 başlatılmaz.
