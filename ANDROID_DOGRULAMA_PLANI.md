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
CURRENT_VALIDATION_STAGE: V06
CURRENT_STATUS: NOT_STARTED
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
NEXT_ACTION: Yalnız V06'yı başlat — gerçek MobilDwg.App FilePicker/SAF + safe-open/document-service bridge ve emulator lifecycle kapısı; aynı turda V07'ye geçme
NEXT_IF_TEST_READY: V06 validation hattını yürüt
NEXT_IF_TEST_OFFLINE: BASLA_A10.md ile yalnız ayrı branch'te A10 host-independent taslağını yürüt
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
```

iOS kodu ve tarihsel evidence korunur fakat kullanıcı iOS yolunu açıkça yeniden etkinleştirene kadar Mac/Xcode/iPhone/iOS workload/signing/simulator/App Store işi Android'i bloke etmez.

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

- technical head `d1552960d910b1fc6baea00ac14f6971344bd66e`
- main base in successful synthetic merge `b5b6a74ebcc9ea16eff4a423c3ff2e7cbb3e748c`
- exact tested synthetic merge `3aa365dd92222ec445a589003fc796ee6290f505`
- run/job `32836712300` / `97767085940` — SUCCESS
- artifact `9559245377`, 29,657,586 byte; digest `sha256:2453ac4df3b888c6235f240208b4674b834edc550dd1208ce37e34a6506d2b65`
- mini corpus `9` fixture + `2` derived negative PASS
- package marker `STAGE05_ACADSHARP_PACKAGE_PASS central=[3.7.1] resolved=3.7.1`
- generated DWG `AC1015`, 8021 byte, read-back PASS; run-specific SHA `44394883546bc115104be2dad50ba158abc0978d57439759d6d4273b88ac2122`; binary golden değildir
- `V05_PRODUCTION_WRITER_ABSENT_PASS`
- validation APK 30,876,566 byte; SHA-256 `a270689a6bda814b9145601498b075b8a3638dd03d6ed6d9026e293c5e0738b5`
- install/cold-launch/UI parse/stability PASS; PID `3803`
- marker `ANDROID_VALIDATION_V05_PASS`
- claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`
- same-head V04 regression `32836712245 / 97767085274` SUCCESS
- same-head V02 regression `32836712385 / 97767086999` SUCCESS; artifact `9559261198`, digest `sha256:e3d9dafeb576b20b63b06b96ba5b1729c15bece13f7d8426d0967d615841500a`

V05 render/engineering fidelity, FilePicker/SAF lifecycle veya physical-device PASS değildir.

### V06 — Android FilePicker/SAF ve safe-open — `NOT_STARTED`

- AŞAMA 06 quota/disk/atomic-copy/generation/cancel/cleanup testleri yeniden çalışır.
- Emulator Documents/provider yolundan gerçek app ile redistributable küçük DWG/DXF seçilir.
- Gerçek app FilePicker/SAF → safe-open/document-service bridge çalıştığı kanıtlanır.
- Açma, cancel, hızlı ikinci seçim, rotate, background/foreground, close/reopen, cleanup denenir.
- Üreticiye özgü SAF/fiziksel cihaz farkları `DEFERRED_PHYSICAL_ANDROID` kalır.

Çıkış: emulator üzerinde real-app safe-open PASS; fiziksel fark açık. V07 aynı turda başlatılmaz.

### V07 — ProCad NO-GO ve production graph izolasyonu — `NOT_STARTED`

- ADR 0002 ve pinned source kararı yeniden okunur.
- ProCad production ProjectReference/PackageReference/native graph'a girmediği otomatik doğrulanır.
- `5,000,000 + 0.001` precision regresyonu çalışır.
- Reddedilmiş ProCad adayını emulator üzerinde tekrar kurma.

### V08 — iOS tarihsel arşiv / Android sınırı — `SCOPE_ARCHIVED / ANDROID_GRAPH_CHECK_PENDING`

- AŞAMA 08 historical evidence korunur; iOS workflow/Mac/simulator/iPhone testi çalıştırılmaz.
- Android production/CI graph'ında iOS workload/native zorunluluğu olmadığı doğrulanır.

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
