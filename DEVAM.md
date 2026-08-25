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
ANDROID_VALIDATION_CURRENT: V07 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — dependency/lockfile/license/hash/vulnerability/Android-native boundary
V03: VALIDATED — fixture/provenance/golden/Android smoke-set contract
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
V06: VALIDATED — REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Sonraki validation turunda yalnız V07'yi başlat — ProCad NO-GO + production graph izolasyonu + precision regression; aynı turda V08'e geçme.
NEXT_IF_TEST_READY: Sonraki BASLA/devam turu V07'yi yürütür.
NEXT_IF_TEST_OFFLINE: Test edilebilir exact V07 SHA varsa queue/WAITING_RUNNER; yoksa gerçek stage durumu korunur. Ayrı sohbet BASLA_A10.md ile A10 draft branch'ini yürütür.
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

## Sonraki validation işi — V07 henüz başlanmadı

- ADR 0002 ve pinned ProCad source kararı yeniden okunacak.
- ProCad'ın production ProjectReference/PackageReference/native graph'a girmediği otomatik doğrulanacak.
- `5,000,000 + 0.001` precision regresyonu çalıştırılacak.
- Reddedilmiş ProCad adayını emulator üzerinde yeniden kurma yapılmayacak.
- V08 aynı V07 turunda başlatılmayacak.

## Paralel A10 yolu

- Normal `BASLA.md`/bu dosya açık V07→V09 validation hattına gider.
- Bilgisayar veya runner kapalıyken kullanıcı başka sohbette `BASLA_A10.md dosyasını oku` der.
- A10 yalnız `stage10-p0-geometry-draft` branch'inde dondurulmuş sözleşmelere dokunmayan host-independent taslak işi yapar.
- Host/GitHub-hosted kontrol sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması + Android gate bekleyen durum `CODED_PENDING_EMULATOR`dır.
- V09 sonrası güncel validated `main` ile integration; etkilenen validation/regression ve real-app API 36 render gate tamamlanmadan A10 `main` merge/DONE olmaz.
- A10 `DONE ON MAIN` olmadan A11 açılmaz.
