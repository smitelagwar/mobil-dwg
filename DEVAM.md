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
ANDROID_VALIDATION_CURRENT: V06 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — dependency/lockfile/license/hash/vulnerability/Android-native boundary
V03: VALIDATED — fixture/provenance/golden/Android smoke-set contract
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Yalnız V06'yı başlat — gerçek MobilDwg.App FilePicker/SAF + safe-open/document-service bridge ve emulator lifecycle kapısı; aynı turda V07'ye geçme.
NEXT_IF_TEST_READY: Bu sohbet V06'yı yürütür.
NEXT_IF_TEST_OFFLINE: Test edilebilir exact V06 SHA varsa queue/WAITING_RUNNER; yoksa gerçek stage durumu korunur. Ayrı sohbet BASLA_A10.md ile A10 draft branch'ini yürütür.
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

- final technical head `d1552960d910b1fc6baea00ac14f6971344bd66e`
- exact tested PR synthetic merge revision `3aa365dd92222ec445a589003fc796ee6290f505`
- authoritative run/job `32836712300` / `97767085940` — SUCCESS
- artifact `9559245377`, 29,657,586 byte
- artifact digest `sha256:2453ac4df3b888c6235f240208b4674b834edc550dd1208ce37e34a6506d2b65`
- Stage05 mini corpus `9` fixture + `2` derived negative PASS.
- ACadSharp package gate `central=[3.7.1] resolved=3.7.1` PASS.
- validation-time AC1015 DWG magic/read-back PASS; run-specific SHA `44394883546bc115104be2dad50ba158abc0978d57439759d6d4273b88ac2122`; binary golden değildir.
- production writer/save yokluğu PASS.
- real validation APK 30,876,566 byte; SHA-256 `a270689a6bda814b9145601498b075b8a3638dd03d6ed6d9026e293c5e0738b5`.
- real app install/cold-launch/UI parse/PID `3803`/stability PASS.
- marker `ANDROID_VALIDATION_V05_PASS`.
- claim limit `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`.
- same-head V04 regression `32836712245 / 97767085274` SUCCESS.
- same-head V02 regression `32836712385 / 97767086999` SUCCESS; artifact `9559261198`, digest `sha256:e3d9dafeb576b20b63b06b96ba5b1729c15bece13f7d8426d0967d615841500a`.

V05 sırasında iki test-gate portability false-negative'i düzeltildi: Git Bash `/warnaserror` path conversion ve localized `dotnet list package` grep. Parser/product failure olarak sınıflandırılmadı.

## V06'da yapılacak iş — henüz başlanmadı

- AŞAMA 06 quota/disk/atomic-copy/generation/cancel/cleanup host testleri yeniden çalıştırılacak.
- Gerçek `MobilDwg.App` FilePicker/SAF/document-service safe-open bridge'i API 36 emulator üzerinde doğrulanacak.
- Küçük redistributable DWG/DXF açma, cancel, hızlı ikinci seçim, rotate/background/foreground, close/reopen ve cleanup davranışı sınanacak.
- Üreticiye özgü SAF ve fiziksel cihaz farkları `DEFERRED_PHYSICAL_ANDROID` kalacak.
- V07 aynı V06 turunda başlatılmayacak.

## Paralel A10 yolu

- Normal `BASLA.md`/bu dosya açık V06→V09 validation hattına gider.
- Bilgisayar veya runner kapalıyken kullanıcı başka sohbette `BASLA_A10.md dosyasını oku` der.
- A10 yalnız `stage10-p0-geometry-draft` branch'inde dondurulmuş sözleşmelere dokunmayan host-independent taslak işi yapar.
- Host/GitHub-hosted kontrol sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması + Android gate bekleyen durum `CODED_PENDING_EMULATOR`dır.
- V09 sonrası güncel validated `main` ile integration; etkilenen validation/regression ve real-app API 36 render gate tamamlanmadan A10 `main` merge/DONE olmaz.
- A10 `DONE ON MAIN` olmadan A11 açılmaz.
