# mobil-dwg — Yeni sohbet için tek dosyalık handoff

Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` gerçek `main` HEAD'ini ve açık PR'ları doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `ANDROID_DOGRULAMA_PLANI.md`, `gecmis.md`, `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` ve son Android validation evidence dosyasını oku.
3. GitHub üzerinden çalışılıyor ve yerel terminal/ADB doğrudan yoksa bağlam `CHATGPT_REMOTE_GITHUB`; `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` okunur.
4. Android V01–V09 validation cursor'ı önceliklidir; implementation cursor ayrı tutulur.
5. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; aynı turda sonraki aşama başlatılmaz.
6. Emulator fiziksel cihaz değildir; `Stage01Smoke` gerçek viewer değildir; queued/zero-step workflow PASS değildir.
7. Her kapanış exact revision + run/job/artifact + claim limit ile kaydedilir.
8. Production dependency evidence olmadan yükseltilmez; ProCad production graph'a geri sokulmaz.
9. Bu validation sohbetidir. Bilgisayar/runner kapalıyken A10 önden çalışması ayrı sohbette `BASLA_A10.md` ile ve `docs/A10_WORKSTREAM.md` sahipliğinde yürütülür.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg` — private, default `main`.
- Aktif v1: Android-only, local/offline, read-only 2D DWG/DXF viewer.
- iOS: `DEFERRED_FUTURE_OPTION`; aktif Android hattını bloke etmez.
- v1 dışı: edit/save/export/cloud/account.

## Güncel checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ
IMPLEMENTATION_WORKSTREAM: docs/A10_WORKSTREAM.md + varsa açık A10 branch/PR
ANDROID_VALIDATION_PROGRAM: V01–V09
ANDROID_VALIDATION_CURRENT: V09 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — dependency/lockfile/license/hash/vulnerability/Android-native boundary
V03: VALIDATED — fixture/provenance/golden/Android smoke-set contract
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
V06: VALIDATED — REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY
V07: VALIDATED — PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY
V08: VALIDATED — ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Sonraki validation turunda yalnız V09 RenderScene/kamera/diagnostics revalidation hattını başlat; aynı turda A10 merge/DONE veya A11 başlatma.
NEXT_IF_TEST_READY: Sonraki BASLA/devam turu yalnız V09'u yürütür.
NEXT_IF_TEST_OFFLINE: Test edilebilir exact V09 SHA varsa queue/WAITING_RUNNER; yoksa gerçek stage durumu korunur. Ayrı sohbet BASLA_A10.md ile A10 draft branch'ini yürütür.
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
```

## Android validation özeti

### V01 — VALIDATED

Evidence: `docs/evidence/android-validation/V01.md`.

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- claim limit `INFRASTRUCTURE_SMOKE_ONLY`

### V02 — VALIDATED

Evidence: `docs/evidence/android-validation/V02.md`.

- strict exact dependency/lockfile/license/hash/vulnerability/Android-native boundary PASS
- ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`
- ProCad/iOS-only/unknown native sızıntısı yok.

### V03 — VALIDATED

Evidence: `docs/evidence/android-validation/V03.md`.

- tested head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- tested PR merge revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- redistributable smoke set: committed 0BSD DXF + validation-time AC1015 DWG + missing-font/missing-XREF negatives.
- generated DWG binary golden değildir.

### V04 — VALIDATED

Evidence: `docs/evidence/android-validation/V04.md`.

- real Android-only MAUI `MobilDwg.App`, `net10.0-android36.0`, package `com.smitelagwar.mobildwg`
- tested head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested PR merge revision `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832` / `97752997848`
- artifact `9557331919`, digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- claim limit `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`.

### V05 — VALIDATED

Evidence: `docs/evidence/android-validation/V05.md`.

- tested head `de39866f8bd71c20fa51b355748ed79884fbb4e6`
- main merge commit `9013d52702d1cb44e378aeacda46ee51e53caa65`
- authoritative run/job `32838507832` / `97772635524` — SUCCESS
- artifact `9561607163`, 29,656,507 byte
- artifact digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- Stage05 mini corpus `9` fixture + `2` derived negative PASS.
- ACadSharp package gate `central=[3.7.1] resolved=3.7.1` PASS.
- validation-time AC1015 DWG magic/read-back PASS; run-specific SHA `0cb734fae8a87ca63562ff7b2e056f835c09f08150cc4345e0a1b5a847cf0099`; binary golden değildir.
- production writer/save yokluğu PASS.
- real validation APK 30,876,566 byte; SHA-256 `1c0dc516b9e1db6270b4f9d8818c3dff09efb98ebc63b085d914358dc11a12ac`.
- real app install/cold-launch/UI parse/PID `3835`/stability PASS.
- marker `ANDROID_VALIDATION_V05_PASS`.
- claim limit `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`.

### V06 — VALIDATED

Evidence: `docs/evidence/android-validation/V06.md`.

- PR `#19`
- tested head `ae8682875524157285946724bd70d6ff010f3917`
- tested PR synthetic merge revision `26b3cdd6ca50d34b98a4806d92f50d4828077d41`
- main merge commit `e17e2472f38557552698b8cf9526d6cbf8b25580`
- authoritative run/job `32849725110` / `97807551403` — SUCCESS
- artifact `9564837027`, 29,743,234 byte
- artifact digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- historical Stage06 actual DWG/DXF, safe-copy guards, last-request-wins, cancel semantics ve T2 headless markers PASS.
- real validation APK 30,917,242 byte; SHA-256 `4bcd819def4483fbc076865dd70b10026eb2eae7515c07561a9cdfe02ff9c9a5`.
- MAUI FilePicker → DocumentsUI/SAF → `OpenReadAsync()` → private safe-copy → production parser DWG PASS.
- ikinci gerçek seçim DXF/latest-state PASS.
- rotate/background-foreground/picker cancel/close cleanup/reopen PASS; PID `3876`.
- original external DWG/DXF immutable PASS.
- broad external-storage permission yok; immediate-copy için persistable URI grant alınmadı/gerekmedi.
- marker `ANDROID_VALIDATION_V06_PASS`.
- claim limit `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`.
- same-head V04 regression `32849725215 / 97807552081` SUCCESS; artifact `9565016182`, digest `sha256:6922f2168334e8312debc2c90cb7905d9db5da58eb8cb10da3f8aadf6e53bb3f`.
- same-head V05 regression `32849725272 / 97807552194` SUCCESS; artifact `9565243977`, digest `sha256:36ada98dd79f7f70e2ef7e63d6d2cb6cec191141421c07bcf41673dded23b492`.

V06 sırasında iki test-infrastructure false-negative'i düzeltildi: Android-only app'i referanslayan host probe `NU1201` bağı ve DocumentsUI roots drawer navigasyonu. Fiziksel Android/provider-specific fidelity hâlâ `DEFERRED_RELEASE_DEVICE_GATE`.

### V07 — VALIDATED

Evidence: `docs/evidence/android-validation/V07.md`.

- PR `#20`
- tested head `559c1d033bdacedc6900d9ad126e7ab21fd8aa50`
- exact checked-out PR synthetic merge `bfa728b840f63a5e9db5d5f376d19fb7f32c62f3`
- main merge commit `4b3b15afe6c95f8393147758b6d16e092ac75a21`
- authoritative run/job `32860034697` / `97841446382` — SUCCESS
- artifact `9567840490`, 19,293 byte
- artifact digest `sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7`
- same-job V02 dependency/lockfile/license/native-boundary prerequisite PASS.
- ADR 0002 + exact rejected ProCad source pin consistency PASS.
- production static graph, locked/resolved assets, app package graph ve Release APK ProCad/ProCadSharp absence PASS.
- real current Release APK 30,913,146 byte; SHA-256 `4605ff85da02e4b45e8d4ae523ae9f5e678a8f596fbbaca23cef77edcab7d450`.
- rejected `5,000,000 + 0.001` direct-float boundary observed delta `0`.
- current production double scalar delta `0.001`; rendering survey-origin regression PASS.
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `V07_PRODUCTION_DOUBLE_PRECISION_REGRESSION_PASS`.
- marker `ANDROID_VALIDATION_V07_PASS`.
- claim limit `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`.

V07 sırasında üç Windows PowerShell validation portability false-negative'i düzeltildi (`String.Contains` overload, strict-mode JSON property enumeration, `double` evidence formatting); production failure değildir. Reddedilmiş ProCad candidate yeniden build/install edilmedi.

### V08 — VALIDATED

Evidence: `docs/evidence/android-validation/V08.md`.

- PR `#21`
- tested head `08abd4a1a953e62a2c0cdc3e48329de90e870195`
- exact checked-out PR synthetic merge `8cd31f3d9f5f507108e5b91ddd3577748df5c952`
- main merge commit `829fd503ba3cd72950b2ec89cfde57f98a1b2417`
- authoritative run/job `32862330823` / `97849123497` — SUCCESS
- artifact `9568747271`, 19,064 byte
- artifact digest `sha256:6b5172553b65973af7fc3eac4f52f7c14a36048b6861368435bcd2355c062ebd`
- same-job V02 dependency/native-boundary prerequisite PASS.
- `MobilDwg.App` Android-only `net10.0-android36.0`; production project/lockfile/solution/central package graph iOS-specific requirement içermiyor.
- historical `Stage 08 iOS Feasibility` workflow manual-only kaldı; aktif CI macOS/iOS toolchain zorunluluğu taşımıyor.
- Windows host: .NET SDK `10.0.400`; recorded workload list yalnız `maui-android`.
- locked Android restore, resolved Android target/library graph scan ve Release build PASS; Xcode gerekmedi.
- Release APK 30,913,146 byte; SHA-256 `7adf8b2495b2eb7389adf48a1f92d9b57f7a0dade56758a0bbefc1b966075f1b`; iOS native/framework entry yok.
- marker `ANDROID_VALIDATION_V08_PASS`.
- claim limit `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED`.
- diagnostic run/job `32862117992 / 97848411995`: raw cross-platform NuGet package-file inventory yanlışlıkla resolved Android graph sayıldığı için false-positive; artifact `9568592189`, digest `sha256:98bfa8c20530579ada137f3c1dda0d6244a93f3d7ea1b1360a9e8f302fbde9fd`.

Historical AŞAMA 08 iOS characterization future-only arşiv olarak korunur; V08 iOS PASS, simulator/device veya iOS AOT claim'i değildir.

## Sonraki validation işi — V09 henüz başlanmadı

- AŞAMA 09 T0/T1, semantic snapshot, OCS/WCS, invalid geometry, overflow ve large-coordinate regresyonları yeniden çalıştırılacak.
- Real app Core/Cad/Rendering composition sınırı doğrulanacak.
- Ayrı A10 draft varsa V09 sonucu üstündür; draft daha sonra güncel validated `main` ile uzlaştırılır.
- Bu V08 kapanış turunda V09 başlatılmaz; A10 merge/DONE veya A11 başlangıcı yapılmaz.

## Paralel A10 yolu

- Normal `BASLA.md`/bu dosya açık V09 validation hattına gider.
- Bilgisayar veya runner kapalıyken kullanıcı başka sohbette `BASLA_A10.md dosyasını oku` der.
- A10 yalnız `stage10-p0-geometry-draft` branch'inde dondurulmuş sözleşmelere dokunmayan host-independent taslak işi yapar.
- Host/GitHub-hosted kontrol sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması + Android gate bekleyen durum `CODED_PENDING_EMULATOR`dır.
- V09 sonrası güncel validated `main` ile integration; etkilenen validation/regression ve real-app API 36 render gate tamamlanmadan A10 `main` merge/DONE olmaz.
- A10 `DONE ON MAIN` olmadan A11 açılmaz.
