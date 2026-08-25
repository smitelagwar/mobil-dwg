# Execution Log

Bu dosya `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` aşamalarının teknik yürütme geçmişini özetler. Anlık checkpoint için `gecmis.md` ve `DEVAM.md`; ayrıntılı kanıt için `docs/evidence/`, `docs/ADR/` ve `compliance/` esas alınır. Başarı kanıtsız yazılmaz; secret/signing materyali, müşteri çizimleri ve cihaz kimlikleri kaydedilmez.

## 2026-08-24 — AŞAMA 00 — DONE

- Mevcut `smitelagwar/mobil-dwg` repo ve kullanıcı belgeleri korundu.
- Yürütme planı, evidence/ADR şablonları ve `gecmis.md` ile izlenebilir çalışma zemini kuruldu.

## 2026-08-24 — AŞAMA 01 — BLOCKED / DEFERRED_EXTERNAL_GATE

- Pinned .NET `10.0.400`, OpenJDK `21.0.12`, Android API 24/36, Build-Tools 36.0.0, Platform-Tools 37.0.1 ve `maui-android` hattı CI'da doğrulandı.
- Debug/Release MAUI build ve manifest gate'leri geçti.
- Fiziksel Android install/launch ve iOS erişim envanteri açık dış kapı olarak bırakıldı.

## 2026-08-24 — AŞAMA 02 — DONE

- ACadSharp 3.7.1 dependency/lisans hattı `GREEN`; SkiaSharp 4.151.1 `REVIEW`; ProCad yalnız izole source-pinned spike olarak kaydedildi.
- Central Package Management, lockfile, exact nupkg hash/license ve vulnerability/reproducibility audit kuruldu.
- PR #4 merge: `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.

## 2026-08-24 — AŞAMA 03 — DONE

- 4 DWG familyası + 2 ASCII DXF + sentetik/negatif fixture'larla tekrar üretilebilir mini corpus ve golden sözleşmesi kuruldu.
- Final corpus audit run `32752374980` / #4 SUCCESS.
- PR #5 merge: `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.

## 2026-08-24 — AŞAMA 04 — DONE

- Dört production ve üç test projesinden oluşan minimal mimari kuruldu; Core BCL-only ve dependency yönleri otomatik testle korundu.
- Final Stage04 run `32755230695` / #2 SUCCESS; Release build `0 Warning / 0 Error`.
- PR #6 merge: `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.

## 2026-08-24 — AŞAMA 05 — DONE

- Exact ACadSharp `3.7.1` read-only adapter mini corpus üzerinde doğrulandı.
- Final parser run `32760139261` / #15 SUCCESS; artifact `9532379884`, digest `sha256:f3b31c937186d874a0ed23c045951d465ace5da8fff2f9acc32006c4352e2f60`.
- ADR 0001: parser baseline `GO`; render fidelity garantisi değildir.
- PR #7 merge: `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.

## 2026-08-24 — AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE

- Provider-path bağımsız safe-open, quota/disk reserve, atomic app-private cache, cleanup, generation/last-request-wins ve cancellation-result-discard uygulandı.
- Final CI run `32762879583` / #3 SUCCESS; artifact `9533538573`, digest `sha256:18c7c395e24b6e3d686edef03d3d0ad686c21fad82686704ef38e7e098a25ea3`.
- Fiziksel Android FilePicker/SAF/lifecycle/cache gate açık bırakıldı.

## 2026-08-24 — AŞAMA 07 — DONE / NO-GO

- Exact ProCad source candidate production graph dışında değerlendirildi.
- Origin `5,000,000` + `0.001` detail direct `double→float` scene sınırında `0.0` delta'ya çöktü; deterministic P0 fidelity blocker.
- Final decision run `32766501837` / #5 SUCCESS; artifact `9534797361`, digest `sha256:9cae376fd0cbf2861f006af347483f9de26a6cd49f30b201438a3afdb591e555`.
- ADR 0002: exact unpatched ProCad production reuse `NO-GO`.
- PR #9 merge: `28cc06c2de5d21f733e29ae69a38395979b6d759`.

## 2026-08-25 — AŞAMA 08 — DONE / CHARACTERIZATION; iOS PASS NOT CLAIMED

- Exact ACadSharp 3.7.1 + SkiaSharp 4.151.1 iOS hattı karakterize edildi.
- Yetkili run `32781026946` / #18 SUCCESS characterization; artifact `9540018558`, digest `sha256:1414e3bf5a9800e150019c48f620c64efcd3d5282ac7322ef9a5e5746ab746f7`.
- Hosted Xcode tool lookup blocker, trimming/reflection riskleri ve simulator NativeAOT sınırı kaydedildi.
- Fiziksel iPhone/local Mac gate açık; iOS PASS iddiası yok.
- PR #11 merge: `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.

## 2026-08-25 — AŞAMA 09 — DONE

Başlangıç/karar:

- ADR 0002 sonrası custom renderer efor/bakım riski kullanıcı tarafından açıkça kabul edildi.
- Tek production scene yolu compact özel immutable `RenderScene` seçildi; ProCad production graph'a eklenmedi.
- Source/test hardening head: `9a17d333afc0a3df1de856a9a53fae0e74617c29`.
- `main` daha sonra Android emulator automation commit'i `b0b0620c40ee5d9a0bcb681783c834fe44040afa` ile ilerledi; bu kullanıcı değişiklikleri A09 branch'ine merge-parent `259793da3828a291c6611700202bbbfcc02652a5` ile aynen korundu.

Uygulanan temel:

- stable entity ID/bounds/layer/style/source metadata;
- immutable deterministic scene;
- world/document ve world→view→screen hattında `double` precision;
- `Camera2D ↔ RenderViewport` explicit bridge;
- survey origin 5,000,000 + 0.001 precision regression;
- finite span/subtraction overflow ve NaN/Infinity guards;
- OCS/WCS arbitrary-axis transform + scaled normalization;
- Unsupported/Substituted/Dropped/Error diagnostics;
- fit/zoom bounds + dark/light color context;
- deterministic `render-scene/v1` semantic snapshot.

Hosted runner ayrımı:

- Standard `ubuntu-latest`, `macos-26` ve `ubuntu-slim` denemeleri bir süre checkout öncesi `steps=[]`, `runner_id=0` ile kesildi; bunlar compile/test failure olarak sınıflandırılmadı.
- `main` üzerindeki dedicated `android-test` automation daha sonra self-hosted Windows runner'ın çevrimiçi olduğunu kanıtladı: Android Emulator Automated Test Gate run `32814581056` SUCCESS.

İlk gerçek A09 execution:

- Head `37ebf1e54f3fe63b199252f97aae97ea72dee130`.
- Run `32815005461`, job `97701406863`, SUCCESS.
- Exact .NET `10.0.400`; targeted Release build `0 Warning / 0 Error`; T0/T1 ve deterministic snapshot PASS.
- Artifact `9551083791`, digest `sha256:33da24c645ba225856ca05778e93f940d6a978defd9a45a7c2788fd6720cce3a`.

Yetkili kapanış validation'ı:

- Head `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087`.
- `Stage 09 Self-Hosted Validation` run `32815175055` / #6.
- Job `97701882792` — SUCCESS.
- Exact .NET `10.0.400`: `STAGE09_DOTNET_PIN_PASS`.
- Targeted Release build: `0 Warning / 0 Error`, `STAGE09_T0_BUILD_PASS`.
- T1: `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `render-scene/v1`, `STAGE09_T1_SCENE_PASS`.
- Snapshot survey detail'i `5000000.001` olarak korudu.
- Full solution restore/build: `0 Warning / 0 Error`.
- Regression: `STAGE04_CORE_CONTRACT_TESTS_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `STAGE04_T0_PASS`, `STAGE09_STAGE04_REGRESSION_PASS`.
- Artifact `9551137293`, `stage09-self-hosted-evidence`, 1,578 bytes.
- Digest `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`.

Post-merge closure audit:

- PR #12 final head `68d08bd3984ef4d1fcca027acb788c4bfcc5e43a` üzerinden merge edildi; merge commit / `main` head `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087..0a2dd886bbe59698a6d2eb4c99f66e7f9270063a` compare'ında A09 production source/test dosyası değişmedi; yalnız workflow cleanup/hardening, evidence/handoff ve remote Android test dokümanı değişti.
- Kalıcı Stage09 workflow'unun `push` kapsamına `main` eklendi; gelecekte doğrudan main'e gelen Stage09 source/test değişiklikleri de regression gate'ini tetikler.

Cleanup:

- Geçici `.github/workflows/stage09-self-hosted-validation.yml` PASS sonrası branch'ten kaldırıldı.
- Kalıcı `.github/workflows/stage09-render-scene.yml` `ubuntu-latest` olarak bırakıldı ve `main` push kapsamı eklendi.
- AŞAMA 01/AŞAMA 06/AŞAMA 08 dış cihaz/local gate'leri değişmeden açık.
- Bir turda en fazla bir aşama kuralı gereği AŞAMA 10 bu kapanış turunda başlatılmadı.

Ayrıntı: `docs/evidence/STAGE_09.md`.

## 2026-08-25 — Android-only kapsam ve V01–V09 revalidation başlangıcı

- Kullanıcı aktif v1 hedefini Android-only yaptı; iOS evidence/adapter sınırları future option olarak korundu, AŞAMA 23–24 aktif sıradan çıkarıldı.
- `ANDROID_DOGRULAMA_PLANI.md` oluşturuldu. Tarihsel implementation AŞAMA 09'da, normal implementation cursor'ı AŞAMA 10'da kalır; yeni Android validation cursor'ı V01'dir.
- Runner çevrim dışıyken exact SHA test kuyruğuna alınır; güvenli kod/host test işi devam eder ve kanıtsız `VALIDATED/DONE` yazılmaz.
- Repo denetimi mevcut emulator gate'te dört doğruluk açığı buldu: executable test harness gövdeleri `dotnet test` ile çalışmıyor; kurulan APK geçici `Stage01Smoke`; screenshot byte-safe değil; PID/crash/ANR kontrolü stability iddiasını karşılamıyor.
- Stage 08 iOS workflow'u Android/Core değişikliklerinde macOS kaynağı tüketmemesi için manual `workflow_dispatch`-only yapıldı.
- Bu tur plan/dokümantasyon ve trigger policy turudur; V01 teknik düzeltmeleri/koşusu başlatılmadı ve eski emulator sonucu viewer PASS sayılmadı.

## 2026-08-25 — Android validation V01 — VALIDATED

- Çalışma bağlamı `CHATGPT_REMOTE_GITHUB`; exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`.
- Self-hosted Windows Android Emulator Release run `32821991333`, job `97721878468`, `SUCCESS`.
- Environment doctor .NET `10.0.400`, `maui-android`, Microsoft OpenJDK 21.0.12 baseline, Android API 36, Build-Tools 36.0.0, ADB 37.0.1 ve AVD `mobil-dwg-api36` kontrollerini geçti.
- Gate tam solution Release build'ini yaptı ve Core/Rendering/Architecture executable harness'larını `dotnet run` ile gerçekten yürüttü; gerekli Stage04/Stage05/Stage09 marker'ları doğrulandı.
- Temporary `Stage01Smoke` signed APK Android 16 / API 36 / x86_64 emulator üzerinde kuruldu; cold launch `Status: ok`, live PID `3374`.
- Screenshot byte-safe yakalandı ve tam PNG imzası `89 50 4E 47 0D 0A 1A 0A` doğrulandı. Artifact indirildi ve screenshot görsel olarak açıldı; çalışan MAUI Stage01Smoke UI görüldü, crash dialog yoktu.
- Package/PID crash buffer boştu; post-launch events create/start/resume/draw akışını gösterdi; `dumpsys activity lastanr` boot'tan beri ANR olmadığını bildirdi.
- Artifact `9553530359`, 7 dosya, 271043 byte; digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`.
- Önceki V01 denemelerindeki doctor-output assertion ve Windows PowerShell UTF-8 parser hataları gerçek PASS sayılmadı; yalnız yukarıdaki exact koşu yetkilidir.
- Karar: `V01 VALIDATED`, fakat claim limit `INFRASTRUCTURE_SMOKE_ONLY`. Bu sonuç gerçek `MobilDwg.App`, DWG/DXF fidelity veya fiziksel Android PASS değildir.
- Ayrıntı: `docs/evidence/android-validation/V01.md`.

## Sonraki iş

`NEXT_VALIDATION_STAGE = V02 — Dependency, lockfile ve Android artifact sınırı (NOT_STARTED)`.

Kullanıcı `devam` veya `BASLA.md dosyasını oku` dediğinde yalnız V02 açılır. Güncel package/source/license kanıtı; locked restore, resolved dependency graph, vulnerability/license policy ve Android runtime/native artifact sınırı yeniden doğrulanır. ProCad ve iOS-only bileşenlerin Android production graph'a sızmadığı kanıtlanır. Bir turda en fazla bir validation aşaması kuralı nedeniyle V03 aynı turda başlatılmaz. Implementation cursor'ı AŞAMA 10'da korunur.
