# mobil-dwg — Proje geçmişi ve AI handoff kaydı

Bu dosya kısa kalıcı tarihçe/checkpoint kaydıdır. Ayrıntılı teknik kanıt `docs/evidence/`, kararlar `docs/ADR/`, aktif Android doğrulama sırası `ANDROID_DOGRULAMA_PLANI.md` içindedir.

## Yeni ajan okuma sırası

1. Gerçek `main` HEAD, açık branch/PR ve Actions durumunu doğrula.
2. Validation sohbetiyse yalnız `BASLA.md` + `DEVAM.md`; A10 ayrı sohbetiyse yalnız `BASLA_A10.md` + mevcut A10 branch/ref'indeki `docs/A10_WORKSTREAM.md` girişini kullan.
3. `ANDROID_DOGRULAMA_PLANI.md`.
4. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`.
5. Seçilen hatta ait son VXX/A10 evidence ve gerektiğinde tarihsel `docs/evidence/STAGE_XX.md` / `docs/ADR/`.
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
ANDROID_VALIDATION_CURRENT: V05 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01_EVIDENCE: docs/evidence/android-validation/V01.md; run 32821991333; job 97721878468; artifact 9553530359
V02_EVIDENCE: docs/evidence/android-validation/V02.md; run 32824397251; job 97729154385; artifact 9554326162
V03_EVIDENCE: docs/evidence/android-validation/V03.md; tested head 69e4e842b5426d71453f5f69a01ebba5948d6b9c; PR merge test revision 1171807016e2deacc4f575b7980400b4f8b4708c; run 32827625875; job 97739039060; artifact 9555501552
V04_EVIDENCE: docs/evidence/android-validation/V04.md; tested head 227ffa49c3095c4328f146acf1a2d9ecc07eb62d; PR merge test revision 6201be929a636b963235f7da8ee72b0bbf9decf2; run 32832142832; job 97752997848; artifact 9557331919
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Yalnız V05'i başlat — gerçek MobilDwg.App içinde ACadSharp parser adapter + V03 DWG/DXF smoke seti; aynı turda V06'ya geçme.
NEXT_IF_TEST_READY: BASLA.md hattında V05.
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
- AŞAMA 10 — P0 geometri renderer — `NOT_STARTED`; offline/parallel taslak yalnız `docs/A10_WORKSTREAM.md` kurallarıyla.
- AŞAMA 11–22 — Android viewer/release hattı; A11, V04–V09 + A10 `DONE ON MAIN` tamamlanana kadar kilitli.
- AŞAMA 23–24 — `DEFERRED_FUTURE_IOS`.
- AŞAMA 25–27 — Android beta/freeze/final handoff.

## Android revalidation geçmişi

### V01 — VALIDATED

- exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- claim limit `INFRASTRUCTURE_SMOKE_ONLY`

Stage01Smoke yalnız toolchain/runner/emulator/ADB/MAUI infrastructure kanıtıdır. Ayrıntı `docs/evidence/android-validation/V01.md`.

### V02 — VALIDATED

Tarihsel “exact pin” gerçekte NuGet open-lower-bound request üretiyordu. Strict exact ranges getirildi:

- ACadSharp `[3.7.1]`
- SkiaSharp `[4.151.1]`
- test/fallback IxMilia.Dxf `[0.8.4]`

Locked restore, license/hash, vulnerability, production `src/` boundary ve Android native inventory gate'i self-hosted Windows üzerinde geçti. ProCad/iOS-only/unknown native sızıntısı yok.

- authoritative run/job `32824397251` / `97729154385`
- tested PR merge ref `549770192c181b30db8968cec5c6ac3c2407e133`
- artifact `9554326162`

Ayrıntı `docs/evidence/android-validation/V02.md`.

### V03 — VALIDATED

V03 Stage 03 corpus/golden/test-matrix sözleşmesini Android için sertleştirdi:

- E-API36 matrix V01 gerçekliğiyle hizalandı.
- `.gitattributes`: `*.dwg binary`, `*.dxf -text`.
- authoritative committed-fixture hash Git blob bytes üzerinden doğrulanıyor.
- committed 0BSD DXF source'undan exact ACadSharp 3.7.1 generator ile validation-time AC1015 DWG üretiliyor ve DwgReader read-back zorunlu.
- generated DWG binary golden değildir; source + generator + magic/read-back + run-specific hash evidence kullanılır.

Final:

- branch head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- tested PR merge revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060` — SUCCESS
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- final marker `ANDROID_VALIDATION_V03_PASS`

Ayrıntı `docs/evidence/android-validation/V03.md`.

### V04 — VALIDATED

Başlangıçta `src/MobilDwg.App` installable Android app değildi; yalnız `net10.0` platform-neutral projeydi. Aynı proje Android-only MAUI executable'a dönüştürüldü; dört-production-project sınırı korunarak beşinci proje açılmadı.

Gerçek app:

- `net10.0-android36.0`
- `com.smitelagwar.mobildwg`
- `MainActivity` + `MainApplication`
- Core/Cad/Rendering project boundaries korunuyor
- `Microsoft.Maui.Controls` exact `[10.0.100]`, MIT

Gate hardening sırasında iki ürün-dışı PowerShell bug'ı bulundu: scalar `.Count` ve built-in read-only `$PID` değişkeni. İlk iki run bu gate bug'ları nedeniyle FAIL oldu; ürün crash/build failure olarak sınıflandırılmaz ve authoritative PASS değildir.

Final authoritative validation:

- branch head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested PR synthetic merge revision `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832` / `97752997848` — SUCCESS
- same-head V02 regression `32832142882` / `97752998222` — SUCCESS
- artifact `9557331919`, 29,878,096-byte ZIP
- artifact digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- signed APK `com.smitelagwar.mobildwg-Signed.apk`, 30,827,130 byte
- APK SHA-256 `60d8d59b3fd452d786519a364875b155d3961c3e4aa210f986c004098789ba42`
- launcher `com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
- cold launch `Status: ok`, PID `3783`
- UI hierarchy, byte-safe PNG, package/PID crash/ANR, process liveness PASS
- marker `ANDROID_VALIDATION_V04_PASS`
- claim limit `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`

V04 parser/render engineering fidelity veya fiziksel Android PASS değildir. Ayrıntı `docs/evidence/android-validation/V04.md`.

## Kalıcı teknik kararlar

- Original CAD immutable; production writer/save yok.
- ACadSharp `3.7.1` read-only parser baseline `GO`.
- Exact unpatched ProCad production reuse `NO-GO`.
- UI parser entity'lerine doğrudan bağlanmaz.
- World/document coordinate hattı `double` precision.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; unknown/policy-RED release blocker.
- Production dependency strict exact range + lockfile/locked restore kullanır.
- Fixture hash evidence Git blob bytes'a dayanır; platform line-ending dönüşümü manifesti değiştirmez.
- Gerçek Android app shell artık repository `MobilDwg.App` projesidir; Stage01Smoke yalnız infrastructure prerequisite'tir.
- Fiziksel Android release öncesi yeniden zorunlu.
- iOS yalnız açık yeni kullanıcı kararıyla etkinleşir.
