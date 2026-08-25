# Execution Log

Bu dosya teknik yürütme geçmişinin kısa indeksidir. Ayrıntılı kanıt `docs/evidence/`, kararlar `docs/ADR/`, aktif checkpoint `DEVAM.md` / `gecmis.md` içindedir. Kanıtsız başarı yazılmaz.

## Implementation özeti

- 2026-08-24 — AŞAMA 00 — `DONE`: execution/evidence/ADR/handoff zemini.
- 2026-08-24 — AŞAMA 01 — `BLOCKED / DEFERRED_EXTERNAL_GATE`: pinned Android toolchain; fiziksel Android dış kapısı açık.
- 2026-08-24 — AŞAMA 02 — `DONE`: dependency/lisans/lockfile; PR #4 merge `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.
- 2026-08-24 — AŞAMA 03 — `DONE`: mini corpus/golden; run `32752374980`; PR #5 merge `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.
- 2026-08-24 — AŞAMA 04 — `DONE`: Core/Cad/Rendering/App architecture; run `32755230695`; PR #6 merge `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.
- 2026-08-24 — AŞAMA 05 — `DONE`: ACadSharp 3.7.1 parser baseline GO; run `32760139261`; artifact `9532379884`; PR #7 merge `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.
- 2026-08-24 — AŞAMA 06 — host implementation done / physical FilePicker-SAF deferred; run `32762879583`; artifact `9533538573`.
- 2026-08-24 — AŞAMA 07 — `DONE / NO-GO`: exact ProCad source reuse rejected; run `32766501837`; artifact `9534797361`; PR #9 merge `28cc06c2de5d21f733e29ae69a38395979b6d759`.
- 2026-08-25 — AŞAMA 08 — historical iOS characterization; run `32781026946`; artifact `9540018558`; PR #11 merge `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.
- 2026-08-25 — AŞAMA 09 — `DONE`: RenderScene/kamera/diagnostics; run `32815175055`; artifact `9551137293`; merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- Implementation cursor: AŞAMA 10 — `MAIN'E HENÜZ MERGE EDİLMEDİ`; paralel draft kuralları `docs/A10_WORKSTREAM.md`.

## Android validation V01 — VALIDATED

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333 / 97721878468`
- artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`
- claim `INFRASTRUCTURE_SMOKE_ONLY`
- evidence `docs/evidence/android-validation/V01.md`

## Android validation V02 — VALIDATED

- strict exact ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, fallback IxMilia.Dxf `[0.8.4]`
- locked restore/license/hash/vulnerability/Android-native/source boundary PASS
- run/job `32824397251 / 97729154385`
- artifact `9554326162`, digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`
- evidence `docs/evidence/android-validation/V02.md`

## Android validation V03 — VALIDATED

- tested head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`; tested merge `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875 / 97739039060`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- marker `ANDROID_VALIDATION_V03_PASS`
- evidence `docs/evidence/android-validation/V03.md`

## Android validation V04 — VALIDATED

- real Android-only MAUI `MobilDwg.App`; API36 build/install/cold-launch/UI/PID/crash-ANR/liveness PASS
- tested head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`; tested merge `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832 / 97752997848`; artifact `9557331919`, digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`
- evidence `docs/evidence/android-validation/V04.md`

## 2026-08-25 — İki çalışma hattı / sınırlı A10 taslak kararı

- Validation cursor V04→V09 sırasını korur ve genel `BASLA.md` sohbetinde yürür.
- AŞAMA 10 yalnız ayrı `stage10-p0-geometry-draft` branch'inde `BASLA_A10.md` protokolüyle önden hazırlanabilir.
- V09 sonrası güncel validated `main` ile integration + Android gate olmadan A10 main merge/DONE yoktur.
- A11 yalnız A10 `DONE ON MAIN` ve boş emulator kuyruğu sonrasında açılır.

## Android validation V05 — VALIDATED

- production `AcadSharpDocumentReader` real Android app process içinde V03 DXF/DWG smoke setiyle PASS
- tested head `de39866f8bd71c20fa51b355748ed79884fbb4e6`; main merge `9013d52702d1cb44e378aeacda46ee51e53caa65`
- run/job `32838507832 / 97772635524`; artifact `9561607163`, digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- package `ACadSharp 3.7.1`; writer/save absent; install/cold-launch/UI parse/stability PASS
- marker `ANDROID_VALIDATION_V05_PASS`; claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`
- evidence `docs/evidence/android-validation/V05.md`

## Android validation V06 — VALIDATED

- real `MobilDwg.App` FilePicker → DocumentsUI/SAF → private safe-copy → production parser PASS
- diagnostic `32846335305 / 97796783640`: host probe `NU1201`; artifact `9562991064`
- diagnostic `32847919780 / 97801845809`: DocumentsUI roots navigation; artifact `9563560512`
- tested head `ae8682875524157285946724bd70d6ff010f3917`; synthetic merge `26b3cdd6ca50d34b98a4806d92f50d4828077d41`; main merge `e17e2472f38557552698b8cf9526d6cbf8b25580`
- run/job `32849725110 / 97807551403`; artifact `9564837027`, digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- DWG/DXF selection + lifecycle/cleanup/immutability PASS; marker `ANDROID_VALIDATION_V06_PASS`
- claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`
- evidence `docs/evidence/android-validation/V06.md`

## Android validation V07 — VALIDATED

- dedicated workflow aynı job içinde V02 prerequisite + production graph/APK/precision gate'ini gerçek non-zero-step çalıştırdı
- tested head `559c1d033bdacedc6900d9ad126e7ab21fd8aa50`; synthetic merge `bfa728b840f63a5e9db5d5f376d19fb7f32c62f3`; main merge `4b3b15afe6c95f8393147758b6d16e092ac75a21`
- run/job `32860034697 / 97841446382`; artifact `9567840490`, digest `sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7`
- ProCad/ProCadSharp production graph/APK absence PASS; rejected direct-float delta `0`; production double delta `0.001` PASS
- marker `ANDROID_VALIDATION_V07_PASS`; claim `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`
- evidence `docs/evidence/android-validation/V07.md`

## Android validation V08 — VALIDATED

- V08 yalnız Android production/CI graph izolasyonunu doğruladı; tarihsel iOS workstream'i yeniden açılmadı.
- first diagnostic run/job `32862117992 / 97848411995`: V02 ve static/CI/locked restore kontrolleri geçti; raw `project.assets.json` içindeki cross-platform NuGet package-file inventory yanlışlıkla resolved Android graph sayıldığı için false-positive oluştu.
- diagnostic artifact `9568592189`, 44,605 byte; digest `sha256:98bfa8c20530579ada137f3c1dda0d6244a93f3d7ea1b1360a9e8f302fbde9fd`.
- gate actual `targets`, `project.frameworks` ve resolved library kimliklerini denetleyecek şekilde düzeltildi.
- tested head `08abd4a1a953e62a2c0cdc3e48329de90e870195`
- exact checked-out synthetic merge `8cd31f3d9f5f507108e5b91ddd3577748df5c952`
- PR #21 main merge `829fd503ba3cd72950b2ec89cfde57f98a1b2417`
- authoritative run/job `32862330823 / 97849123497` — SUCCESS
- artifact `9568747271`, 19,064 byte; digest `sha256:6b5172553b65973af7fc3eac4f52f7c14a36048b6861368435bcd2355c062ebd`
- same-job V02 dependency/lockfile/license/native-boundary prerequisite PASS
- app TFM `net10.0-android36.0`; production project/lockfile/solution/central package graph iOS-specific requirement içermiyor
- historical Stage08 iOS workflow `workflow_dispatch` only; active/non-historical CI macOS/iOS toolchain gerektirmiyor
- Windows .NET SDK `10.0.400`; recorded workload list yalnız `maui-android`; locked restore + Release build without Xcode PASS
- resolved Android graph iOS target/library absence PASS
- Release APK 30,913,146 byte; SHA-256 `7adf8b2495b2eb7389adf48a1f92d9b57f7a0dade56758a0bbefc1b966075f1b`; iOS native/framework entry yok
- marker `ANDROID_VALIDATION_V08_PASS`
- claim `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED`
- evidence `docs/evidence/android-validation/V08.md`
- historical iOS characterization future-only arşivdir; V08 iOS PASS/simulator/device/AOT claim'i değildir.

## Sonraki iş

`NEXT_VALIDATION_STAGE = V09 — RenderScene, camera and diagnostics revalidation (NOT_STARTED)`.

Bir sonraki validation `devam` yalnız V09'u açar. Bu V08 kapanış turunda V09, A10 merge/DONE veya A11 başlatılmaz. Implementation cursor `AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ` olarak ayrı korunur.
