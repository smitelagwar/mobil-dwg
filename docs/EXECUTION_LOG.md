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
- Central Package Management, committed lockfile, `--locked-mode`, exact `.nupkg` SHA-256/license manifest'i ve vulnerability/reproducibility CI kapısı kuruldu.
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
- Architecture harness tam 4 production/3 test proje sayısını, exact ProjectReference grafını, production dependency sınırlarını ve forbidden dependency terimlerini denetler.

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

---

## 2026-08-24 — AŞAMA 05 — DONE

Başlangıç main: `27f036d5d240c4ca47dd2fcb94c1e72604ed0f8f`.

Yapılanlar:

- Exact ACadSharp `3.7.1` yalnız `src/MobilDwg.Cad` adapter katmanına eklendi.
- NuGet-generated project-aware `src/MobilDwg.Cad/packages.lock.json` commit edildi; final gate `--locked-mode` restore kullanıyor.
- `AcadSharpDocumentReader` DWG magic/DXF signature-header preflight, parser notification, exception, parse timing, missing-font ve missing-XREF compatibility kayıtlarını Core kontratlarına bağlıyor.
- Parser-specific `CadDocument` yalnız `AcadSharpDocumentHandle` arkasında; Core/App/Rendering katmanlarında ACadSharp entity/type dependency'si yok.
- Cancellation `BeforeStartOnly`, progress `StagesOnly`; parser başladıktan sonra cooperative abort veya uydurma yüzde yok.
- `tools/Stage05.ParserProbe` manifest-driven headless corpus validator olarak eklendi.
- Architecture test ACadSharp PackageReference'ını yalnız `MobilDwg.Cad` için allow edip diğer production katmanlarında forbidden dependency guard uyguluyor.
- `.github/workflows/stage05-parser-spike.yml` T3 corpus gate ve evidence artifact üretiyor.

CI sırasında yakalanan gerçek sorunlar:

1. İlk Stage 05 koşusu committed lockfile'da `MobilDwg.Core` ProjectReference kaydı olmadığı için `NU1004` ile düştü. Elle lock formatı tahmin edilmedi; NuGet CI'da `--force-evaluate` ile gerçek lockfile üretti ve artifact'ten alınarak commit edildi.
2. İkinci koşuda parser fixture'ları açtı fakat probe `BLOCK_REFERENCE` semantic alias'ını CLR type adı `BlockReference` varsayımıyla aradığı için 0 saydı. ACadSharp gerçek entity'si `INSERT`/`Insert` olduğu doğrulandı; probe `ObjectName == INSERT` üzerinden semantic alias üretecek şekilde düzeltildi. Bu parser veri kaybı değildi.

Final implementation CI:

- Head `09e26172aa8de9e8c79ae64853a493dab1d0e5b9`.
- Workflow `Stage 05 Parser Spike` run `32759096003` / #8 `SUCCESS`.
- Release build: `0 Warning(s)`, `0 Error(s)`.
- `STAGE04_CORE_CONTRACT_TESTS_PASS`.
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`.
- `STAGE04_ARCHITECTURE_TESTS_PASS`.
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`.
- `STAGE05_MINI_CORPUS_PASS fixtures=9 derived_negatives=2`.
- `STAGE05_T3_PASS`.
- Evidence artifact `9532001644`, digest `sha256:2750ba88141c5724306bb5811173d958c60836806021f2ff1a5b36b011631097`.

Corpus sonucu:

- DWG: AC1015, AC1018, AC1024, AC1032 — PASS.
- ASCII DXF: AC1015, AC1032 — PASS.
- Sentetik Turkish/basic/nested INSERT DXF — exact semantic count PASS.
- Missing-font DXF — `missing-font` compatibility PASS.
- Missing-XREF DXF — `missing-xref` compatibility PASS.
- Derived truncated AC1015 DWG — controlled `EndOfStreamException` PASS.
- Derived corrupt AC1018 DWG — controlled warning PASS.
- Ana upstream DWG/DXF karşılıklarında total block entity `341`; gerekli LINE/CIRCLE/BLOCK_REFERENCE/DIMENSION/HATCH semantiği geçti.

Aynı implementation head regresyonları:

- Stage 04 Architecture run `32759095988` / #11 `SUCCESS`.
- Stage 02 Dependency Audit run `32759095944` / #25 `SUCCESS`.
- Stage 01 Toolchain Smoke run `32759095888` / #44 merge kapısı hazırlanırken ayrıca çalıştırıldı; fiziksel cihaz gate'i değildir.

Parser kararı:

- `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`: ACadSharp `3.7.1` read-only parser baseline `GO`.
- Bu karar render/engineering fidelity garantisi değildir.
- ASCII DXF `unsupported-object` ve yüksek notification hacmi known limitation olarak kaydedildi; sabit warning-count eşiği yok.
- Kritik DXF parse kaybı olmadığı için IxMilia.Dxf fallback spike başlatılmadı.

Ayrıntı: `docs/evidence/STAGE_05.md` ve ADR 0001.

PR #7 `stage05: validate ACadSharp headless parser` doğrulanmış head üzerinden `main`e merge edilmek üzere kapanış branch'inde hazırlandı. Merge commit checkpoint dosyalarına merge sonrasında gerçek SHA ile yazılacaktır.

AŞAMA 01 fiziksel Android install/launch ve iOS erişim envanteri `DEFERRED_EXTERNAL_GATE` olarak açık kalır.

Sonuç: AŞAMA 05 teknik gate'leri `DONE`. Sonraki aşama AŞAMA 06; aynı kullanıcı turunda başlanmaz.


---

## 2026-08-24 — AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE

Başlangıç main: `b0262877b0273c5854e671c95e0c11601dfcd170`.

Yapılanlar:

- `MobilDwg.App` içinde stream-factory tabanlı safe-open kontratları, actual-byte quota, disk reserve, sanitized filename, atomic unique app-private cache copy ve deterministic cleanup eklendi.
- Parse worker thread'e taşındı; parser cooperative cancel desteklemiyorsa hard-stop iddiası yapılmıyor.
- Generation ID / `last request wins` ile stale parser sonuçları dispose edilip commit edilmiyor.
- Gerçek pinned AC1015 DWG + committed sentetik DXF safe-open probe'u geçti; original hash'ler değişmedi.
- Quota, provider declared-size yalanı, disk reserve, source disposal, temp leak, last-request-wins ve cancel-result-discard testleri geçti.
- MAUI Android FilePicker/OpenReadAsync spike'ı generated temiz uygulamada Debug+Release derlendi; minSdk 24, targetSdk 36; broad storage permission yok.

CI sırasında yakalanan gerçek sorunlar:

1. App kullanıcı mesajında parser vendor adı geçtiği için architecture source-boundary guard fail verdi; App parser-agnostic hale getirildi.
2. Static provider-path guard app-private cache normalizationındaki `Path.GetFullPath` çağrısını yanlış pozitif saydı; guard yalnız MAUI provider adapter source'una daraltıldı.

Final implementation CI head `56de020fb1297b8642c4f84c24522bbd723272f8`:

- Stage 06 Safe Open run `32762879583` / #3 `SUCCESS`.
- Stage 04 Architecture run `32762879643` / #22 `SUCCESS`.
- Stage 02 Dependency Audit run `32762879581` / #35 `SUCCESS`.
- Stage 01 Toolchain Smoke run `32762879589` / #54 `SUCCESS`; fiziksel cihaz kanıtı değildir.
- Evidence artifact `9533538573`, digest `sha256:18c7c395e24b6e3d686edef03d3d0ad686c21fad82686704ef38e7e098a25ea3`.

Açık dış kapı: gerçek fiziksel Android telefonda FilePicker/SAF DWG+DXF, metadata/diagnostics, cancel, hızlı ikinci seçim, rotate, background/foreground, close/reopen ve cache leak smoke. Bu nedenle AŞAMA 06 `DONE` değildir; `BLOCKED / DEFERRED_EXTERNAL_GATE` kalır.

Kullanıcı onaylı `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` gereği AŞAMA 06 PR #8 merge edildikten sonra sonraki bağımsız çalışma AŞAMA 07 olabilir; AŞAMA 06 fiziksel gate release/beta/final kapılarında yeniden zorunludur.


## 2026-08-24 — AŞAMA 07 closure

- PR #8 AŞAMA 06 merge commit doğrulandı: `e3a9c36e04be6c51827926ca17bb1a386c6b1142`. AŞAMA 06 physical Android gate bu merge ile kapanmadı.
- AŞAMA 07 branch/PR #9 exact ProCad `f8a862b3e7634e27664fee02ff5d68774b102985` source candidate'ını production graph'a eklemeden değerlendirdi.
- Final decision head `3f88bec383de895e309e218c08d13e9784562a97`.
- Final `Stage 07 ProCad Source Spike` run `32766501837` / #5 `SUCCESS`; artifact `9534797361`; digest `sha256:9cae376fd0cbf2861f006af347483f9de26a6cd49f30b201438a3afdb591e555`.
- Source Android build `82 Warning(s) / 0 Error(s)`; clean MAUI Release smoke `0 Warning(s) / 0 Error(s)`. Build başarısızlığı karar nedeni değildir.
- ACadSharp source lineage official upstream'de çözüldü; mobil-dwg approved baseline 592 commit ileride. Published ProCadSharp 0.1.1 graph ACadSharp 1.0.0 ve Skia 4.147.0-preview.2.1 çözüyor.
- Deterministic precision gate: origin 5,000,000 + 0.001 detail direct double-to-float scene boundary'sinde observed delta 0.0; systematic P0 fidelity blocker.
- ADR 0002 exact unpatched candidate için `NO-GO`. Physical Android T3 `NOT_RUN_AFTER_DETERMINISTIC_BLOCKER`, PASS değildir.
- ProCad production dependency graph'a eklenmedi. AŞAMA 01 ve 06 dış cihaz gate'leri açık.
- Sonraki bağımsız aşama AŞAMA 08. AŞAMA 09 custom renderer implementation öncesinde kullanıcı GO kararı zorunlu.


## 2026-08-25 — AŞAMA 08 — DONE / CHARACTERIZATION; iOS PASS NOT CLAIMED

- İzole iOS spike production graph değiştirmeden ACadSharp `3.7.1` + SkiaSharp `4.151.1` + `SkiaSharp.NativeAssets.iOS 4.151.1` hattını test etti.
- .NET SDK `10.0.400`, iOS workload `26.5.10301/10.0.100`, Xcode `26.6` exact host hattı doğrulandı.
- Yetkili karakterizasyon: `Stage 08 iOS Feasibility` run `32781026946` / #18 `SUCCESS`; artifact `9540018558`, digest `sha256:1414e3bf5a9800e150019c48f620c64efcd3d5282ac7322ef9a5e5746ab746f7`.
- Evidence classification `BLOCKED_PARTIAL_EVIDENCE`; workflow success tüm probe'ların PASS olduğu anlamına gelmez.
- Baseline Release hosted Xcode 26.6 tool lookup'ta `install_name_tool` bulunamadığı için runtime'a ulaşmadı; hosted Xcode bundle yamalanmadı.
- Trim probe ACadSharp hattında IL2026/IL2070/IL2072/IL2075/IL2087/IL2090 warning ailelerini kaydetti: 30 trimmer, 12 reflection-related, 0 font line.
- Simulator `PublishAot=true` `NETSDK1203` ile unsupported; bu ACadSharp NativeAOT failure olarak sınıflandırılmadı. Gerçek `ios-arm64` AOT future Mac/iPhone gate'idir.
- Fiziksel iPhone `NOT_RUN_DEFERRED_EXTERNAL_GATE`; local Mac inventory `PENDING_USER_EVIDENCE`.
- Kullanıcının mevcut execution override'ı uyarınca dış blocker/risk açıkça kaydedilip bağımsız sonraki işe ilerleme kabul edildi; iOS PASS iddiası yok.
- Future local/device acceptance listesi `docs/LOCAL_DEVICE_REVALIDATION.md` olarak eklendi.
- AŞAMA 09 custom renderer implementation ADR 0002 nedeniyle explicit kullanıcı GO bekler.
