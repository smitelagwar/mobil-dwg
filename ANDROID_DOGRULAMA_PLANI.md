# mobil-dwg — Android geriye dönük doğrulama planı

Bu belge AŞAMA 01–09 arasında geliştirilen kodu Android hedefinde sırayla yeniden doğrulayan yetkili alt plandır. Ana ürün planı `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyasıdır. Tarihsel `docs/evidence/STAGE_XX.md` kayıtları değiştirilmez; yeni sonuçlar `docs/evidence/android-validation/VXX.md` altında tutulur.

## 1. Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ
IMPLEMENTATION_WORKSTREAM: docs/A10_WORKSTREAM.md + varsa açık A10 branch/PR
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_VALIDATION_STAGE: V09
CURRENT_STATUS: NOT_STARTED
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
V06: VALIDATED — REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY
V07: VALIDATED — PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY
V08: VALIDATED — ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED
NEXT_ACTION: Sonraki validation turunda yalnız V09 RenderScene/kamera/diagnostics revalidation hattını başlat; aynı turda A10 merge/DONE veya A11 başlatma
NEXT_IF_TEST_READY: Sonraki turda yalnız V09 validation hattını yürüt
NEXT_IF_TEST_OFFLINE: BASLA_A10.md ile yalnız ayrı branch'te A10 host-independent taslağını yürüt
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
```

iOS kodu ve tarihsel evidence korunur fakat kullanıcı iOS yolunu açıkça yeniden etkinleştirene kadar Mac/Xcode/iPhone/iOS workload/signing/simulator/App Store işi Android'i bloke etmez. V08 yalnız aktif Android production/CI graph izolasyonunu doğruladı; tarihsel iOS characterization iOS PASS olarak yeniden sınıflandırılmadı.

## 2. `BASLA.md` / `devam` protokolü

Kullanıcı `BASLA.md dosyasını oku` veya `devam` dediğinde ajan:

1. Gerçek `main` HEAD, açık PR ve checkpoint'i doğrular.
2. `BASLA.md`, bu dosya, canonical plan, `DEVAM.md`, `gecmis.md`, execution override ve çalışma bağlamına uygun Android test workflow'unu okur.
3. Genel `BASLA.md` komutunda açık VXX bitmediyse doğrudan onu sürdürür. Ayrı A10 sohbeti yalnız kullanıcı `BASLA_A10.md` komutunu verdiğinde açılır.
4. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatır; sonraki aşamayı aynı turda başlatmaz.
5. Emulator fiziksel cihaz sayılmaz; geçici `Stage01Smoke` gerçek viewer sayılmaz; queued/zero-step workflow PASS sayılmaz.
6. Test/evidence olmadan `VALIDATED/DONE` yazmaz.
7. Implementation cursor AŞAMA 10'da validation cursor'dan ayrı korunur. Erken A10 yalnız izole draft branch'inde ilerleyebilir; VXX checkpoint/evidence dosyalarını değiştiremez.
8. Exact tested SHA/PR merge revision, run/job ve artifact evidence'e yazılır.

## 3. Gerçeklik sınıfları

| Kanıt | Kanıtladığı | Kanıtlamadığı |
|---|---|---|
| Restore/build/executable harness | host kod/sözleşme | Android install/UI/runtime |
| Stage01Smoke emulator | runner/SDK/ADB/MAUI infrastructure | gerçek viewer/DWG-DXF işlevi |
| Gerçek MobilDwg.App emulator | test edilen Android app revision runtime akışı | fiziksel üretici/SAF/perf farkı |
| Fiziksel Android | kaydedilen gerçek cihaz senaryosu | test edilmemiş başka cihazlar |
| Fixture/provenance audit | input rights/hash/test sözleşmesi | parser/render fidelity |

## 4. Durumlar

`NOT_STARTED`, `CODE_AUDIT`, `FIX_REQUIRED`, `FIX_IN_PROGRESS`, `IN_PROGRESS_UNVALIDATED`, `CODED_PENDING_HOST_TESTS`, `CODED_PENDING_EMULATOR`, `READY_FOR_EMULATOR`, `WAITING_RUNNER`, `VALIDATED`, `VALIDATED_WITH_DEFERRED_PHYSICAL`, `SCOPE_ARCHIVED`, `DEFERRED_PHYSICAL_ANDROID`, `BLOCKED`.

`IN_PROGRESS_UNVALIDATED`, `CODED_PENDING_HOST_TESTS`, `CODED_PENDING_EMULATOR`, `READY_FOR_EMULATOR` ve `WAITING_RUNNER` PASS değildir.

## 5. Self-hosted runner ve paralel A10 kuralı

- Normal validation source `main` veya VXX feature branch'tir; `android-test` yalnız test taşıyıcısıdır.
- Exact tested SHA/PR merge revision evidence'e yazılır; force-push/force-ref update yapılmaz.
- Runner çevrim dışıysa exact SHA/test `PENDING_EMULATOR_QUEUE` kaydına alınır; aynı queued iş çoğaltılmaz.
- Feature head test edildiğinde merge commit tercih edilir; tested ancestry korunur.
- Workflow `SUCCESS` yalnız gerçekten çalışan adımlar kadar güçlüdür. Hosted job `steps=[]`, `runner_id=0`, boş runner adı ile biterse runner-allocation failure'dır, kod failure değildir.
- Validation hattı V01→V09 sırasını korur ve `main`/VXX evidence üzerinde yetkilidir.
- Kullanıcı ayrı sohbette `BASLA_A10.md dosyasını oku` diyerek yalnız `stage10-p0-geometry-draft` branch'inde sınırlı A10 taslağı yürütebilir.
- Erken A10 yalnız yeni/internal platform-neutral primitive-tessellator matematiği ve saf testlerdir. V09 kapanana kadar mevcut RenderScene/interface/snapshot, architecture, `.csproj`/Skia ve fixture/image-golden sözleşmeleri dondurulur; ProCad, MAUI/FilePicker/lifecycle ve A11 kapsam dışıdır.
- A10 host/hosted kontrolleri sonuçsuzsa `CODED_PENDING_HOST_TESTS`; actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`; actual non-zero-step PASS olup emulator bekliyorsa en fazla `CODED_PENDING_EMULATOR` olur. `main` merge/DONE yoktur.
- V09 sonrası güncel validated `main` ile A10 integration; etkilenen validation/regression ve expected-content içeren real-app API36 render gate geçmeden A10 main'e merge edilmez. A10 `DONE ON MAIN` olmadan A11 açılmaz.

## 6. Validation sırası ve authoritative evidence

### V01 — Toolchain, runner ve emulator altyapısı — `VALIDATED`

Evidence: `docs/evidence/android-validation/V01.md`.

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- .NET 10.0.400, maui-android, OpenJDK 21.0.12, API36/Build-Tools36/ADB37
- claim `INFRASTRUCTURE_SMOKE_ONLY`

### V02 — Dependency, lockfile ve Android artifact sınırı — `VALIDATED`

Evidence: `docs/evidence/android-validation/V02.md`.

- ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`
- locked restore, exact graph, nupkg hash/license, vulnerability, production `src/` boundary ve Android native inventory PASS
- ProCad/iOS-only/unknown native sızıntısı yok

### V03 — Fixture, golden sözleşmesi ve Android test matrisi — `VALIDATED`

Evidence: `docs/evidence/android-validation/V03.md`.

- tested head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- tested PR merge revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- committed 0BSD DXF + validation-time AC1015 DWG + missing-font/missing-XREF negative set
- committed CAD hash Git blob bytes; generated DWG binary golden değildir

### V04 — Mimari ve gerçek Android uygulama kabuğu — `VALIDATED`

Evidence: `docs/evidence/android-validation/V04.md`.

- `MobilDwg.App`: `net10.0-android36.0`, package `com.smitelagwar.mobildwg`, real `MainActivity`/`MainApplication`
- Core/Cad/Rendering dependency yönleri korunuyor; direct MAUI exact `[10.0.100]`, MIT
- tested head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested merge `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832` / `97752997848`
- artifact `9557331919`, digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- real APK build/install/cold-launch/UI/PID/crash-ANR/liveness PASS
- claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`

### V05 — ACadSharp parser entegrasyonu — `VALIDATED`

Evidence: `docs/evidence/android-validation/V05.md`.

V05 production read-only `AcadSharpDocumentReader` yolunu gerçek Android `MobilDwg.App` process'i içinde V03 DWG/DXF smoke setiyle doğruladı. Validation assets yalnız `V05Validation=true` build'inde paketlenir; production writer/save eklenmedi.

Gate hardening:

- Windows Git Bash `/warnaserror` path conversion false-negative'i `-warnaserror` ile düzeltildi.
- localized `dotnet list package` grep false-negative'i kaldırıldı; exact ACadSharp merkezi props + lockfile + `project.assets.json` ile doğrulanıyor.

Authoritative final:

- tested PR head revision `de39866f8bd71c20fa51b355748ed79884fbb4e6`
- main merge commit `9013d52702d1cb44e378aeacda46ee51e53caa65`
- run/job `32838507832` / `97772635524` — SUCCESS
- artifact `9561607163`, 29,656,507 byte; digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- mini corpus `9` fixture + `2` derived negative PASS
- package marker `STAGE05_ACADSHARP_PACKAGE_PASS central=[3.7.1] resolved=3.7.1`
- generated DWG `AC1015`, 8021 byte, read-back PASS; run-specific SHA `0cb734fae8a87ca63562ff7b2e056f835c09f08150cc4345e0a1b5a847cf0099`; binary golden değildir
- `V05_PRODUCTION_WRITER_ABSENT_PASS`
- validation APK 30,876,566 byte; SHA-256 `1c0dc516b9e1db6270b4f9d8818c3dff09efb98ebc63b085d914358dc11a12ac`
- install/cold-launch/UI parse/stability PASS; PID `3835`
- marker `ANDROID_VALIDATION_V05_PASS`
- claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`
- same-head V04 regression `32838507889 / 97772635962` SUCCESS; artifact `9561764023`, digest `sha256:b5f8581c4c4290adb83fb243968bb93b7a3991ca14c6658e418468acf76288e8`
- same-head V02 regression `32838507864 / 97775556718` SUCCESS
- same-head V03 regression `32838507809 / 97775415411` SUCCESS

V05 render/engineering fidelity, FilePicker/SAF lifecycle veya physical-device PASS değildir.

### V06 — Android FilePicker/SAF ve safe-open — `VALIDATED`

Evidence: `docs/evidence/android-validation/V06.md`.

V06 gerçek `MobilDwg.App` üzerinde MAUI FilePicker → Android DocumentsUI/SAF → `FileResult.OpenReadAsync()` → app-private safe-copy → production parser zincirini API 36 emulator üzerinde doğruladı. Fiziksel cihaz/provider fidelity bu claim'in dışındadır.

Gate hardening sırasında iki test-infrastructure false-negative'i düzeltildi:

- Android-only `MobilDwg.App` projesini `net10.0` host probe'dan referanslayan tarihsel `Stage06.OpenFlowProbe` `NU1201` üretiyordu; probe production safe-open BCL kaynaklarını linkleyecek şekilde ayrıştırıldı, app multi-target yapılmadı.
- DocumentsUI `Recent / No items` ekranında roots drawer açılmadan `Downloads` aranıyordu; artifact UI XML'ine göre `Show roots` → `Downloads` → file navigasyonu eklendi.

Authoritative final:

- PR `#19`
- tested PR head revision `ae8682875524157285946724bd70d6ff010f3917`
- tested PR synthetic merge revision `26b3cdd6ca50d34b98a4806d92f50d4828077d41`
- main merge commit `e17e2472f38557552698b8cf9526d6cbf8b25580`
- run/job `32849725110` / `97807551403` — SUCCESS
- artifact `9564837027`, 29,743,234 byte; digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- historical safe-open markers `STAGE06_ACTUAL_DWG_DXF_PASS`, `STAGE06_SAFE_COPY_GUARDS_PASS`, `STAGE06_LAST_REQUEST_WINS_PASS`, `STAGE06_CANCEL_SEMANTICS_PASS`, `STAGE06_T2_HEADLESS_PASS`
- validation APK 30,917,242 byte; SHA-256 `4bcd819def4483fbc076865dd70b10026eb2eae7515c07561a9cdfe02ff9c9a5`
- package `com.smitelagwar.mobildwg`; install/cold-launch PASS
- real DWG SAF open PASS
- real DXF second selection/latest-state PASS
- rotate/background-foreground/picker-cancel/close-cleanup/reopen PASS; PID `3876`
- original external CAD immutable PASS
- broad external-storage permission absent; persistable URI grant not needed/taken for immediate private copy
- package/PID stability and post-launch ANR gate PASS
- marker `ANDROID_VALIDATION_V06_PASS`
- claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`
- same-head V04 regression `32849725215 / 97807552081` SUCCESS; artifact `9565016182`, digest `sha256:6922f2168334e8312debc2c90cb7905d9db5da58eb8cb10da3f8aadf6e53bb3f`
- same-head V05 regression `32849725272 / 97807552194` SUCCESS; artifact `9565243977`, digest `sha256:36ada98dd79f7f70e2ef7e63d6d2cb6cec191141421c07bcf41673dded23b492`

Üreticiye özgü SAF/fiziksel cihaz farkları `DEFERRED_RELEASE_DEVICE_GATE` kalır. V06 render/engineering fidelity veya release readiness claim'i değildir.

### V07 — ProCad NO-GO ve production graph izolasyonu — `VALIDATED`

Evidence: `docs/evidence/android-validation/V07.md`.

V07 exact rejected ProCad adayının ADR 0002 `Rejected / NO-GO` kararını güncel Android production graph'a karşı yeniden doğruladı; reddedilmiş aday yeniden clone/build/install edilmedi.

Authoritative final:

- PR `#20`
- tested PR head revision `559c1d033bdacedc6900d9ad126e7ab21fd8aa50`
- exact checked-out PR synthetic merge revision `bfa728b840f63a5e9db5d5f376d19fb7f32c62f3`
- main merge commit `4b3b15afe6c95f8393147758b6d16e092ac75a21`
- run/job `32860034697` / `97841446382` — SUCCESS
- artifact `9567840490`, 19,293 byte; digest `sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7`
- same-job V02 dependency/lockfile/license/native-boundary prerequisite PASS
- ADR/source-pin, static production graph, restored lockfile/`project.assets.json`, app package graph ProCad isolation PASS
- real current `MobilDwg.App` Release APK 30,913,146 byte; SHA-256 `4605ff85da02e4b45e8d4ae523ae9f5e678a8f596fbbaca23cef77edcab7d450`; ProCad APK entry absent
- rejected direct-float survey-origin blocker reproduced: `5,000,000 + 0.001` → observed single-precision delta `0`
- current production double scalar delta `0.001`; `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `V07_PRODUCTION_DOUBLE_PRECISION_REGRESSION_PASS`
- marker `ANDROID_VALIDATION_V07_PASS`
- claim `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`

V07 sırasında üç Windows PowerShell validation portability false-negative'i düzeltildi: `String.Contains` overload'u, strict-mode JSON property enumeration ve `double.ToString("R", culture)` evidence formatting. Bunlar production failure değildir.

### V08 — iOS tarihsel arşiv / Android graph izolasyonu — `VALIDATED`

Evidence: `docs/evidence/android-validation/V08.md`.

V08 tarihsel iOS workstream'ini yeniden açmadan yalnız aktif Android production/CI graph izolasyonunu doğruladı.

Authoritative final:

- PR `#21`
- tested PR head revision `08abd4a1a953e62a2c0cdc3e48329de90e870195`
- exact checked-out PR synthetic merge revision `8cd31f3d9f5f507108e5b91ddd3577748df5c952`
- main merge commit `829fd503ba3cd72950b2ec89cfde57f98a1b2417`
- run/job `32862330823` / `97849123497` — SUCCESS
- artifact `9568747271`, 19,064 byte; digest `sha256:6b5172553b65973af7fc3eac4f52f7c14a36048b6861368435bcd2355c062ebd`
- same-job V02 dependency/lockfile/license/native-boundary prerequisite PASS
- `MobilDwg.App` target `net10.0-android36.0`; production project/lockfile/solution/central package graph iOS-specific requirement içermiyor
- historical Stage08 iOS workflow manual-only (`workflow_dispatch`); active/non-historical CI macOS/iOS toolchain gerektirmiyor
- Windows .NET SDK `10.0.400`; recorded workload list yalnız `maui-android`
- locked Android restore + resolved target/library graph scan + Release build without Xcode PASS
- Release APK 30,913,146 byte; SHA-256 `7adf8b2495b2eb7389adf48a1f92d9b57f7a0dade56758a0bbefc1b966075f1b`; iOS native/framework entry absent
- marker `ANDROID_VALIDATION_V08_PASS`
- claim `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED`

Diagnostic:

- run/job `32862117992 / 97848411995`: raw `project.assets.json` package-file inventory içindeki cross-platform iOS files yanlışlıkla resolved Android dependency sayıldı; V02/static/CI/locked restore kontrolleri geçmişti.
- artifact `9568592189`, 44,605 byte; digest `sha256:98bfa8c20530579ada137f3c1dda0d6244a93f3d7ea1b1360a9e8f302fbde9fd`.
- gate actual `targets`, `project.frameworks` ve resolved library identities üzerinden düzeltildi.

Tarihsel `docs/evidence/STAGE_08.md` characterization future-only arşivdir; V08 iOS PASS, simulator/device veya iOS AOT claim'i değildir.

### V09 — RenderScene, kamera ve diagnostics — `NOT_STARTED`

- AŞAMA 09 T0/T1, semantic snapshot, OCS/WCS, invalid geometry, overflow, large-coordinate regresyonları yeniden çalıştır.
- Real app Core/Cad/Rendering composition sınırı doğrulanır.
- Ayrı A10 draft varsa V09 sonucu üstündür; draft daha sonra güncel validated `main` ile uzlaştırılır.

Çıkış: AŞAMA 01–09 Android revalidation kuyruğu temiz. Bu V09 kapanış turunda A10 merge/DONE veya A11 başlangıcı yapılmaz.

## 7. V09 sonrası uzlaştırma ve uygulama sırası

Aktif sıra AŞAMA 10–22, ardından Android-only AŞAMA 25–27. AŞAMA 23–24 future iOS track'tir. A10 draft varsa önce `docs/A10_WORKSTREAM.md` merge kapısı tamamlanır; A10 `DONE` olmadan A11 açılmaz. Android runtime/UI/packaging değişikliklerinde anlamlı checkpoint'te gerçek app emulator gate çalıştırılır. Fiziksel Android AŞAMA 20–22 ve final release kapılarında tekrar zorunludur.

## 8. Her validation kapanışında güncellenecek kayıtlar

1. Bu dosyanın current VXX checkpoint'i.
2. `docs/evidence/android-validation/VXX.md`.
3. `DEVAM.md` ve `gecmis.md`.
4. Canonical plan checkpoint'i.
5. `docs/EXECUTION_LOG.md` kısa teknik kayıt.
6. Pending Android/emulator işi varsa exact SHA/workflow/expected marker.

Tarihsel `docs/evidence/STAGE_01.md`–`STAGE_09.md` geriye dönük yeniden yazılmaz.
