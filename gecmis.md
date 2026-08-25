# mobil-dwg — Proje geçmişi ve AI handoff kaydı

Bu dosya kısa kalıcı tarihçe/checkpoint kaydıdır. Ayrıntılı teknik kanıt `docs/evidence/`, kararlar `docs/ADR/`, aktif Android doğrulama sırası `ANDROID_DOGRULAMA_PLANI.md` içindedir.

## Yeni ajan okuma sırası

1. Gerçek `main` HEAD, açık branch/PR ve Actions durumunu doğrula.
2. Validation sohbetiyse `BASLA.md` + `DEVAM.md`; A10 ayrı sohbetiyse `BASLA_A10.md` + mevcut A10 ref'indeki `docs/A10_WORKSTREAM.md`.
3. `ANDROID_DOGRULAMA_PLANI.md`.
4. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`.
5. Seçilen hatta ait son evidence ve gerektiğinde tarihsel `docs/evidence/STAGE_XX.md` / `docs/ADR/`.
6. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`; remote bağlamdaysa `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md`.

`BASLA.md` ile `BASLA_A10.md` aynı sohbette iki ayrı execution komutu olarak birlikte çalıştırılmaz.

## Repo / ürün

- GitHub: `smitelagwar/mobil-dwg` — private, default `main`.
- Aktif v1: Android-only, local/offline, read-only 2D DWG/DXF viewer.
- iOS: `DEFERRED_FUTURE_OPTION`; aktif Android DoD/sırası dışında.
- v1 dışında: edit/save/export/cloud/account.

## Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ
IMPLEMENTATION_WORKSTREAM: docs/A10_WORKSTREAM.md + varsa açık A10 branch/PR
ANDROID_VALIDATION_CURRENT: V06 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01_EVIDENCE: docs/evidence/android-validation/V01.md; run 32821991333; job 97721878468; artifact 9553530359
V02_EVIDENCE: docs/evidence/android-validation/V02.md; run 32824397251; job 97729154385; artifact 9554326162
V03_EVIDENCE: docs/evidence/android-validation/V03.md; tested head 69e4e842b5426d71453f5f69a01ebba5948d6b9c; tested merge 1171807016e2deacc4f575b7980400b4f8b4708c; run 32827625875; job 97739039060; artifact 9555501552
V04_EVIDENCE: docs/evidence/android-validation/V04.md; tested head 227ffa49c3095c4328f146acf1a2d9ecc07eb62d; tested merge 6201be929a636b963235f7da8ee72b0bbf9decf2; run 32832142832; job 97752997848; artifact 9557331919
V05_EVIDENCE: docs/evidence/android-validation/V05.md; tested head de39866f8bd71c20fa51b355748ed79884fbb4e6; main merge 9013d52702d1cb44e378aeacda46ee51e53caa65; run 32838507832; job 97772635524; artifact 9561607163
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Yalnız V06'yı başlat — real MobilDwg.App FilePicker/SAF + safe-open/document-service bridge; aynı turda V07'ye geçme.
NEXT_IF_TEST_READY: BASLA.md hattında V06.
NEXT_IF_TEST_OFFLINE: Ayrı BASLA_A10.md sohbetinde yalnız A10 draft branch'i.
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
LAST_UPDATE: 2026-08-25
```

## Yürütme kuralı

Android V01–V09 validation cursor'ı implementation cursor'dan ayrıdır. Normal `BASLA.md` açık VXX'i yürütür; yalnız `BASLA_A10.md` ayrı branch'te A10 taslağını açar. A10 host/GitHub-hosted kontrolü sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi geçtiğinde V04–V09 uzlaştırması + Android gate bekleyen `CODED_PENDING_EMULATOR` olur. A10 main merge/DONE olmadan A11 yoktur. Kanıtsız PASS/DONE yoktur; iOS Android hattını bloke etmez.

## Implementation geçmişi

- AŞAMA 00 — çalışma/yürütme zemini — `DONE`.
- AŞAMA 01 — pinned Android toolchain; fiziksel telefon dış kapısı — `BLOCKED / DEFERRED_EXTERNAL_GATE`.
- AŞAMA 02 — dependency/lisans/lockfile — `DONE`.
- AŞAMA 03 — corpus/golden/matris — `DONE`.
- AŞAMA 04 — minimal solution/mimari sınırlar — `DONE`.
- AŞAMA 05 — ACadSharp parser spike — `DONE`; ADR 0001 `GO`.
- AŞAMA 06 — safe-open implementation; fiziksel FilePicker/SAF kapısı deferred.
- AŞAMA 07 — ProCad exact source spike — `DONE / NO-GO`; ADR 0002.
- AŞAMA 08 — iOS characterization — historical/future; iOS PASS iddiası yok.
- AŞAMA 09 — immutable RenderScene/kamera/diagnostics foundation — `DONE`; authoritative run `32815175055`, artifact `9551137293`, merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- AŞAMA 10 — P0 geometri renderer — `MAIN'E HENÜZ MERGE EDİLMEDİ`; paralel draft yalnız `docs/A10_WORKSTREAM.md` kurallarıyla.
- AŞAMA 11–22 — Android viewer/release hattı; A11, V04–V09 + A10 `DONE ON MAIN` tamamlanana kadar kilitli.
- AŞAMA 23–24 — `DEFERRED_FUTURE_IOS`.
- AŞAMA 25–27 — Android beta/freeze/final handoff.

## Android revalidation geçmişi

### V01 — VALIDATED

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- claim limit `INFRASTRUCTURE_SMOKE_ONLY`

Stage01Smoke yalnız infrastructure kanıtıdır. Ayrıntı `docs/evidence/android-validation/V01.md`.

### V02 — VALIDATED

Strict exact dependency policy, locked restore, license/hash, vulnerability, production `src/` boundary ve Android native inventory gate'i geçti. ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`; ProCad/iOS-only/unknown native sızıntısı yok. Ayrıntı `docs/evidence/android-validation/V02.md`.

### V03 — VALIDATED

Fixture/provenance/golden/test-matrix sözleşmesi Android için sertleştirildi. Committed CAD hash Git blob bytes üzerinden doğrulanır; committed 0BSD DXF + validation-time generated/read-back AC1015 DWG + missing-font/missing-XREF negative set kullanılır. Generated DWG binary golden değildir.

- tested head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- tested merge `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`

Ayrıntı `docs/evidence/android-validation/V03.md`.

### V04 — VALIDATED

`src/MobilDwg.App` gerçek Android-only MAUI executable'a dönüştürüldü (`net10.0-android36.0`, package `com.smitelagwar.mobildwg`). API36 build/install/cold-launch/UI/PID/crash-ANR/liveness geçti.

- tested head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested merge `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832` / `97752997848`
- artifact `9557331919`, digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`

Ayrıntı `docs/evidence/android-validation/V04.md`.

### V05 — VALIDATED

Production read-only `AcadSharpDocumentReader` gerçek Android `MobilDwg.App` process'i içinde V03 redistributable DWG/DXF smoke setiyle doğrulandı.

Gate hardening sırasında iki ürün-dışı false-negative bulundu ve düzeltildi:

- Git Bash/MSYS `/warnaserror` anahtarını path'e dönüştürüyordu; `-warnaserror` kullanıldı.
- localized `dotnet list package` çıktısı eski grep'i bozuyordu; final package doğrulaması merkezi exact props + lockfile + project.assets graph üzerinden locale-independent yapıldı.

Authoritative final:

- tested PR head revision `de39866f8bd71c20fa51b355748ed79884fbb4e6`
- main merge `9013d52702d1cb44e378aeacda46ee51e53caa65`
- run/job `32838507832` / `97772635524` — SUCCESS
- artifact `9561607163`, 29,656,507 byte
- digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- host mini-corpus `9` fixture + `2` derived negative PASS
- `STAGE05_ACADSHARP_PACKAGE_PASS central=[3.7.1] resolved=3.7.1`
- generated AC1015 DWG 8021 byte, DwgReader read-back PASS; run-specific hash `0cb734fae8a87ca63562ff7b2e056f835c09f08150cc4345e0a1b5a847cf0099`; binary golden değildir
- `V05_PRODUCTION_WRITER_ABSENT_PASS`
- validation APK 30,876,566 byte; SHA-256 `1c0dc516b9e1db6270b4f9d8818c3dff09efb98ebc63b085d914358dc11a12ac`
- real app install/cold-launch/UI parse/stability PASS; PID `3835`
- marker `ANDROID_VALIDATION_V05_PASS`
- claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`
- same-head V04 regression `32838507889 / 97772635962` SUCCESS; artifact `9561764023`, digest `sha256:b5f8581c4c4290adb83fb243968bb93b7a3991ca14c6658e418468acf76288e8`
- same-head V02 regression `32838507864 / 97775556718` SUCCESS
- same-head V03 regression `32838507809 / 97775415411` SUCCESS

Ayrıntı `docs/evidence/android-validation/V05.md`.

## Kalıcı teknik kararlar

- Original CAD immutable; production writer/save yok.
- ACadSharp `3.7.1` read-only parser baseline `GO` ve gerçek Android V05 smoke ile doğrulandı.
- Exact unpatched ProCad production reuse `NO-GO`.
- UI parser entity'lerine doğrudan bağlanmaz.
- World/document coordinate hattı `double` precision.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; unknown/policy-RED release blocker.
- Production dependency strict exact range + lockfile/locked restore kullanır.
- Fixture hash evidence Git blob bytes'a dayanır; platform line-ending dönüşümü manifesti değiştirmez.
- Gerçek Android app shell repository `MobilDwg.App` projesidir; Stage01Smoke yalnız infrastructure prerequisite'tir.
- Fiziksel Android release öncesi yeniden zorunlu.
- iOS yalnız açık yeni kullanıcı kararıyla etkinleşir.
