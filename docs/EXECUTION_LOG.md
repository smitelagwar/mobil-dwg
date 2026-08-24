# Execution Log

Bu dosya `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` aşamalarının teknik yürütme geçmişini özetler. Anlık checkpoint için kökteki `gecmis.md`; ayrıntılı kanıt için `docs/evidence/` ve `compliance/` esas alınır. Başarı kanıtsız yazılmaz; müşteri çizimleri, secret/signing materyali, cihaz seri/UDID gibi hassas bilgiler kaydedilmez.

## 2026-08-24 — AŞAMA 00 — DONE

- Repo `smitelagwar/mobil-dwg`, default `main` doğrulandı.
- Başlangıç revision `d161b5c4f9ba238f0d2a2e4c92f773535f379487`.
- Yüklenen nihai plan ile başlangıç GitHub plan blob'u eşleşti: Git blob `a05dc53df058c5355f8576996a33cce704ac19f3`.
- `.gitignore` build/temp/private CAD/signing/font korumaları açısından yeterli bulundu.
- `docs/EXECUTION_LOG.md`, ADR/evidence şablonları ve `gecmis.md` oluşturuldu.
- Kapanış plan commit'i `fe3c8c043e6d373e6313d2e1201cc24992b493a9`.

## 2026-08-24 — AŞAMA 01 — BLOCKED / DEFERRED_EXTERNAL_GATE

Fiziksel cihazdan bağımsız hat tamamlandı:

- .NET SDK/workload set `10.0.400`.
- Microsoft OpenJDK `21.0.12`.
- Android min API 24, target/compile API 36.
- Build-Tools `36.0.0`, Platform-Tools/ADB `37.0.1`.
- `maui-android` workload.
- `global.json`, `docs/TOOLCHAIN.md`, Stage 01 evidence, GitHub Actions smoke hattı, Android cihaz gate scriptleri ve iOS inventory helper eklendi.
- PR #1 merge `83379b24e4ba87f04299f612ae2951ae8d8aec13`.
- PR #2 merge `9b375af9931a3db23f82e9b983257f29030a7376`.
- PR #3 merge `9a397065a55c5844993e6ef909438f44ad5aa1f6`.

AŞAMA 03 final PR head üzerindeki son regresyon kanıtı:

- Stage 01 Toolchain Smoke run `32752374956` / #34 `SUCCESS`.
- Debug + Release + manifest/API + artifact upload PASS.
- Artifact `9529753917`, digest `sha256:6067ccf1cc6e696a100e110b164cfafb5da614779f8315cfce8670e6fdda9a3e`.

Bu CI fiziksel telefon kanıtı değildir. Gerçek `STAGE01_DEVICE_GATE_PASS`, Android install/launch ve iOS erişim envanteri açık dış kapılardır. Kullanıcı onayıyla `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` bağımsız aşamaların ilerlemesine izin verir.

## 2026-08-24 — AŞAMA 02 — DONE

- ACadSharp 3.7.1 `GREEN` dependency/lisans adayı; fidelity AŞAMA 05'e bırakıldı.
- SkiaSharp 4.151.1 `REVIEW`, Android native graph kaydedildi.
- ProCad exact source snapshot/fork/submodule zinciri `REVIEW`, production default `NO-GO`, yalnız AŞAMA 07 source-pinned spike.
- IxMilia.Dxf 0.8.4 yalnız test/fallback; Dwg/Shx source-only `REVIEW`.
- Central Package Management, committed lockfile, `--locked-mode`, exact `.nupkg` SHA-256/license/native-entry manifest'i ve vulnerability/reproducibility CI kapısı kuruldu.
- Stage 01 root-CPM regresyonu yakalandı; smoke app `$RUNNER_TEMP` altında izole edildi.
- PR #4 merge commit `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.
- Final Stage 02 run `32747785867` / #9 `SUCCESS`; artifact `9527769476`, digest `sha256:90d41760e306e13b9977586b9996c1aafdf27f615c2b730bb41d74507b4684f3`.
- AŞAMA 03 final head regresyonu run `32752375058` / #15 `SUCCESS`; artifact `9529546355`, digest `sha256:c528be8af15d8089da3bdc60feccd2ede404d8dfa2015630a3218d1190e49642`.

Ayrıntı: `docs/evidence/STAGE_02.md`, `compliance/DEPENDENCY_EVIDENCE.md`.

## 2026-08-24 — AŞAMA 03 — DONE

Başlangıç main: `bc720198fab0cd7bd9718e59e22ed36bea9fee0a`.

Yapılanlar:

- Fixture manifest şeması ve mini corpus contract oluşturuldu.
- 4 DWG familyası + 2 ASCII DXF ACadSharp immutable revision üzerinden remote-pinned olarak tanımlandı; binary'ler mobil-dwg reposuna vendored edilmedi.
- Upstream source revision `592d70a7bf0eaffbd932d23900f289b4e6305832`.
- Mobil-dwg sentetik Türkçe/basic/nested-block DXF ile missing-font/missing-XREF negatifleri eklendi; sentetik fixture'lar 0BSD notice ile ayrıştırıldı.
- Truncated/corrupt DWG negatifleri CI'da deterministic türetilir.
- `fixtures/manifest/stage03-source-integrity.json` ile upstream Git blob SHA1 + SHA-256 dual-hash modeli kuruldu.
- `docs/GOLDEN_CONTRACT.md` ve `docs/DEVICE_MATRIX.md` oluşturuldu.
- Private fixture Git-ignore guard ve fixture validator CI'ya bağlandı.

Final Stage 03 CI:

- Workflow `Stage 03 Corpus Audit` run `32752374980` / #4 `SUCCESS`.
- Head `bcc2f32c31e7c6d26d154d3e308bf662c41f34e6`.
- `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`.
- `STAGE03_DUAL_HASH_PASS fixtures=6`.
- Evidence artifact `9529508675`, digest `sha256:fd3990d7a3271c015a2f7067a856d5a23434f1ec0449ecff7819b569938e02cf`.

Aynı head regresyonları:

- Stage 02 run `32752375058` / #15 `SUCCESS`.
- Stage 01 run `32752374956` / #34 `SUCCESS`.

PR #5 `test: establish Stage 03 corpus contract` doğrulanmış head üzerinden `main`e merge edildi.

Merge commit: `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.

Sonuç: AŞAMA 03 `DONE`. Sonraki aşama AŞAMA 04; aynı kullanıcı turunda başlanmadı.

---

## 2026-08-24 — AŞAMA 04 — DONE

Başlangıç main: `a18480f55c76658027ba44ade33d1b88c9d4d6d8`.

Yapılanlar:

- `MobilDwg.sln` oluşturuldu.
- Tam olarak dört production proje kuruldu: `MobilDwg.Core`, `MobilDwg.Cad`, `MobilDwg.Rendering`, `MobilDwg.App`.
- Tam olarak üç test proje kuruldu: `MobilDwg.Core.Tests`, `MobilDwg.Rendering.Tests`, `MobilDwg.Architecture.Tests`.
- Core BCL-only tutuldu; ProjectReference/PackageReference, MAUI, SkiaSharp ve ACadSharp dependency yok.
- Dependency yönü otomatik testle sabitlendi: Cad -> Core, Rendering -> Core, App -> Core/Cad/Rendering.
- `ICadDocumentReader`, session owner (`CadDocumentSession` + `ICadDocumentHandle`), diagnostics/compatibility, `IRenderSceneBuilder`, `ICadRenderer`, render surface/viewport kontratları tanımlandı.
- Session parser-specific handle'ı idempotent `IAsyncDisposable` ile tam bir kez dispose eder.
- Cancellation capability `None/BeforeStartOnly/Cooperative`; progress capability `None/StagesOnly/Fractional` olarak modellenerek sahte cooperative cancellation veya sahte yüzde engellendi.
- `Directory.Build.props`, `docs/ARCHITECTURE.md`, `scripts/stage04-test.sh` ve `.github/workflows/stage04-architecture.yml` eklendi.
- Architecture harness tam 4 production/3 test proje sayısını, exact ProjectReference grafını, production PackageReference yokluğunu ve Core/App forbidden dependency terimlerini denetler.

İlk CI bulgusu:

- Stage 04 Architecture run `32755135364` / #1 `FAILURE`.
- Hata solution kodundan önce `global.json` workload set `10.0.400` temiz runner'da kurulu olmadığı için `MSB4242` idi.
- Workflow önce `dotnet workload install maui-android` çalıştırıp exact workload setini doğrulayacak şekilde düzeltildi; production projelerine MAUI dependency eklenmedi.

Final Stage 04 CI:

- Workflow `Stage 04 Architecture` run `32755230695` / #2 `SUCCESS`.
- Final head `00fc7d5e04e521b421a9e4646bff9e6a7c82d6d1`.
- Exact .NET SDK/workload set `10.0.400`: PASS.
- Solution restore: PASS.
- Release build: PASS, `0 Warning(s)`, `0 Error(s)`.
- `STAGE04_CORE_CONTRACT_TESTS_PASS`.
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`.
- `STAGE04_ARCHITECTURE_TESTS_PASS`.
- `STAGE04_T0_PASS`.

Aynı head regresyonları:

- Stage 02 Dependency Audit run `32755230688` / #17 `SUCCESS`; artifact `9530581424`, digest `sha256:58da9bfdb4ad4c59672b368673d0f27cfbd2e7b3a7b8157c003618e9671d4593`.
- Stage 01 Toolchain Smoke run `32755230683` / #36 `SUCCESS`; Debug/Release/manifest/artifact PASS; artifact `9530813909`, digest `sha256:bf31fd5a4aa2268e768137f5fe19dfe8b37f13fb206eb4a616e07b77b1d2382e`.

PR #6 `feat: establish Stage 04 minimal architecture` doğrulanmış head üzerinden `main`e merge edildi.

Merge commit: `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.

AŞAMA 01 fiziksel Android install/launch ve iOS erişim envanteri `DEFERRED_EXTERNAL_GATE` olarak açık kalır.

Sonuç: AŞAMA 04 `DONE`. Sonraki aşama AŞAMA 05; aynı kullanıcı turunda başlanmadı.
