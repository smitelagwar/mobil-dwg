# mobil-dwg — Android geriye dönük doğrulama planı

Bu belge AŞAMA 01–09 implementation temelini Android hedefinde yeniden doğrulayan V01–V09 programının yetkili alt planıdır. Ana ürün planı `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`; ayrıntılı kanıtlar `docs/evidence/android-validation/VXX.md` altındadır. Tarihsel `docs/evidence/STAGE_XX.md` kayıtları geriye dönük yeniden yazılmaz.

## 1. Program kapanış checkpoint'i

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 18 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 19 — NOT_STARTED
IMPLEMENTATION_WORKSTREAM: AŞAMA 18 DONE (docs/evidence/STAGE_18.md)
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09 — CLOSED
CURRENT_VALIDATION_STAGE: PROGRAM_CLOSED
CURRENT_STATUS: V01–V09 VALIDATED_WITH_CLAIM_LIMITS
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
V06: VALIDATED — REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY
V07: VALIDATED — PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY
V08: VALIDATED — ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED
V09: VALIDATED — RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_IMPLEMENTATION_STAGE: AŞAMA 19 — Malicious/corrupt input ve resource guards
A19_GATE: OPEN
```

V01–V09'un kapanması Android v1 ürününün tamamlandığı anlamına gelmez. Bu program AŞAMA 01–09 temelinin güncel Android graph/runtime sınırlarında hâlâ geçerli olduğunu claim-limited biçimde kanıtlar. Renderer fidelity, tüm CAD entity kapsamı, fiziksel cihaz performansı ve release DoD sonraki implementation aşamalarındadır.

## 2. `BASLA.md` / `devam` davranışı

1. Her turda gerçek `main` HEAD, açık PR/branch ve Actions durumu okunur.
2. V01–V09 programı artık kapalıdır; yeniden açılmasını gerektiren gerçek regression/dependency değişikliği yoksa normal cursor AŞAMA 10'dur.
3. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; aynı turda sonraki aşama başlatılmaz.
4. `queued`, `steps=null`, `runner_id=0`, boş runner adı veya zero-step job PASS değildir.
5. Emulator fiziksel cihaz değildir; `Stage01Smoke` gerçek viewer değildir.
6. Test/evidence olmadan `VALIDATED`, `READY_TO_MERGE` veya `DONE` yazılmaz.
7. Feature head doğrulandığında merge commit tercih edilir; force-push/force-ref yapılmaz.
8. Kullanıcı future iOS'u açıkça yeniden etkinleştirmedikçe iOS/Mac/Xcode/iPhone işi Android sırasını bloke etmez.
9. A10 `main`e merge edilmeden ve gerçek Android render gate'i geçmeden A11 açılmaz.

## 3. Gerçeklik sınıfları

| Kanıt | Kanıtladığı | Kanıtlamadığı |
|---|---|---|
| Host restore/build/executable test | kaynak kodu ve sözleşme | Android install/UI/runtime |
| Stage01Smoke emulator | runner/SDK/ADB/MAUI altyapısı | gerçek viewer işlevi |
| Gerçek `MobilDwg.App` emulator | exact revision Android app/runtime akışı | fiziksel üretici/provider/perf farkı |
| Build edilmiş gerçek APK | Android composition/packaging | install/UI/render fidelity |
| Fiziksel Android | kayıtlı cihaz senaryosu | test edilmemiş cihazlar |
| Fixture/provenance audit | input rights/hash/test sözleşmesi | parser/render fidelity |

## 4. Authoritative validation sonuçları

### V01 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V01.md`.

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333 / 97721878468`
- artifact `9553530359`
- claim `INFRASTRUCTURE_SMOKE_ONLY`

### V02 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V02.md`.

- exact ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`
- locked restore, license/hash/vulnerability, production source boundary ve Android-native inventory PASS
- ProCad/iOS-only/unknown native production sızıntısı yok

### V03 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V03.md`.

- tested head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- synthetic merge `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875 / 97739039060`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- committed 0BSD DXF + validation-time AC1015 DWG + negative fixture sözleşmesi PASS

### V04 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V04.md`.

- real Android-only MAUI `MobilDwg.App`, `net10.0-android36.0`, package `com.smitelagwar.mobildwg`
- API36 build/install/cold-launch/UI/PID/crash-ANR/liveness PASS
- run/job `32832142832 / 97752997848`
- claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`

### V05 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V05.md`.

- tested head `de39866f8bd71c20fa51b355748ed79884fbb4e6`
- main merge `9013d52702d1cb44e378aeacda46ee51e53caa65`
- run/job `32838507832 / 97772635524` — SUCCESS
- artifact `9561607163`, digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- production `AcadSharpDocumentReader` gerçek Android process içinde DWG/DXF smoke PASS
- writer/save absent
- claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`

### V06 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V06.md`.

- PR `#19`; tested head `ae8682875524157285946724bd70d6ff010f3917`
- synthetic merge `26b3cdd6ca50d34b98a4806d92f50d4828077d41`
- main merge `e17e2472f38557552698b8cf9526d6cbf8b25580`
- run/job `32849725110 / 97807551403` — SUCCESS
- artifact `9564837027`, digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- real FilePicker/DocumentsUI/SAF → stream → private safe-copy → production parser + lifecycle/cleanup/immutability PASS
- claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`

### V07 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V07.md`.

- PR `#20`; tested head `559c1d033bdacedc6900d9ad126e7ab21fd8aa50`
- synthetic merge `bfa728b840f63a5e9db5d5f376d19fb7f32c62f3`
- main merge `4b3b15afe6c95f8393147758b6d16e092ac75a21`
- run/job `32860034697 / 97841446382` — SUCCESS
- artifact `9567840490`, digest `sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7`
- ProCad/ProCadSharp production/resolved graph + Release APK absence PASS
- rejected direct-float survey delta `0`; production double delta `0.001`
- claim `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`

### V08 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V08.md`.

- PR `#21`; tested head `08abd4a1a953e62a2c0cdc3e48329de90e870195`
- synthetic merge `8cd31f3d9f5f507108e5b91ddd3577748df5c952`
- main merge `829fd503ba3cd72950b2ec89cfde57f98a1b2417`
- run/job `32862330823 / 97849123497` — SUCCESS
- artifact `9568747271`, digest `sha256:6b5172553b65973af7fc3eac4f52f7c14a36048b6861368435bcd2355c062ebd`
- Android production/CI graph iOS-specific TFM/RID/native/toolchain zorunluluğundan izole
- Windows + `maui-android` ile locked restore/Release build PASS; Android APK'da iOS framework/native entry yok
- claim `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED`

### V09 — `VALIDATED`

Evidence: `docs/evidence/android-validation/V09.md`.

- PR `#22`
- tested head `892315966f895729e866947a838df93350fdfd97`
- exact checked-out synthetic merge `6fea8ba9d1de6811afd0dcace7a2c8b5b6ec573a`; tested head'e göre file diff yok
- main merge `143ce1a79448f53af81faee9c6e650321047dd37`
- authoritative run/job `32864617493 / 97856686115` — SUCCESS
- artifact `9569686660`, 11,544 byte; digest `sha256:97e55129367ea5b778edf99a6d84939e95f74902db655144d32dbf24ba8aa375`
- same-job V02 prerequisite PASS
- exact .NET `10.0.400`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `render-scene/v1`
- deterministic semantic snapshot and survey-origin line `5000000.001` PASS
- `V09_SURVEY_ORIGIN_DOUBLE_PRECISION_PASS delta=0.001`
- Core/architecture/dependency composition regressions PASS
- full solution Release build `0 Warning`, `0 Error`
- real `MobilDwg.App` Release APK `30,913,146` byte; SHA-256 `a0080fb4826cbd6f7fee1d84cac3465c8ebda766bfba245167d73233ab1a40f5`
- marker `ANDROID_VALIDATION_V09_PASS`
- claim `RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY`

V09'un ilk run/job'u `32864458158 / 97856153440` Windows PowerShell 5.1 `.Contains(string, StringComparison)` overload portability false-negative'i nedeniyle gate'in ürün testlerine ulaşmadan durdu; production/test failure değildir. Diagnostic artifact `9569504762`, digest `sha256:7eda4ec7db3d423cdbd476bc4769eebac54ef0527c18656c0fc2bbd0b2eb90f8`. Gate `IndexOf(..., StringComparison.Ordinal)` ile düzeltildi ve exact yeni head authoritative PASS aldı.

## 5. Program kapanış sonucu

```text
ANDROID_VALIDATION_V01_V09: CLOSED
VALIDATION_DEBT: NONE_WITHIN_RECORDED_CLAIMS
PENDING_EMULATOR_QUEUE: EMPTY
NEXT_IMPLEMENTATION_STAGE: AŞAMA 16 — NOT_STARTED
```

AŞAMA 10 başladığında güncel validated `main` baz alınır. A10 exact integration SHA üzerinde etkilediği dependency/fixture/architecture/V09 regresyonları ve gerçek `MobilDwg.App` API36 render gate'i çalıştırılır. Render gate yalnız PID/PNG/crash/ANR değil, non-blank/expected-content pixel probe, Android golden veya kayıtlı görsel incelemeden en az bir expected-content kanıtı içerir. Bu kanıt olmadan A10 `READY_TO_MERGE/DONE` değildir.

Fiziksel Android release/device kapısı açık kalır. Future iOS yalnız açık kullanıcı kararıyla yeniden etkinleştirilir.
