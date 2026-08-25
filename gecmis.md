# mobil-dwg — Proje geçmişi ve AI handoff kaydı

Bu dosya kısa kalıcı tarihçe/checkpoint kaydıdır. Ayrıntılı teknik kanıt `docs/evidence/`, kararlar `docs/ADR/`, aktif Android doğrulama sırası `ANDROID_DOGRULAMA_PLANI.md` içindedir. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni ajan okuma sırası

1. Gerçek `main` HEAD, açık branch/PR ve Actions durumunu doğrula.
2. Validation sohbetiyse `BASLA.md` + `DEVAM.md`; A10 ayrı sohbetiyse `BASLA_A10.md` + mevcut A10 ref'indeki `docs/A10_WORKSTREAM.md`.
3. `ANDROID_DOGRULAMA_PLANI.md`.
4. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`.
5. Son validation evidence ve gerektiğinde tarihsel `docs/evidence/STAGE_XX.md` / `docs/ADR/`.
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
ANDROID_VALIDATION_CURRENT: V09 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01_EVIDENCE: docs/evidence/android-validation/V01.md; run 32821991333; job 97721878468; artifact 9553530359
V02_EVIDENCE: docs/evidence/android-validation/V02.md; run 32824397251; job 97729154385; artifact 9554326162
V03_EVIDENCE: docs/evidence/android-validation/V03.md; tested head 69e4e842b5426d71453f5f69a01ebba5948d6b9c; tested merge 1171807016e2deacc4f575b7980400b4f8b4708c; run 32827625875; job 97739039060; artifact 9555501552
V04_EVIDENCE: docs/evidence/android-validation/V04.md; tested head 227ffa49c3095c4328f146acf1a2d9ecc07eb62d; tested merge 6201be929a636b963235f7da8ee72b0bbf9decf2; run 32832142832; job 97752997848; artifact 9557331919
V05_EVIDENCE: docs/evidence/android-validation/V05.md; tested head de39866f8bd71c20fa51b355748ed79884fbb4e6; main merge 9013d52702d1cb44e378aeacda46ee51e53caa65; run 32838507832; job 97772635524; artifact 9561607163
V06_EVIDENCE: docs/evidence/android-validation/V06.md; tested head ae8682875524157285946724bd70d6ff010f3917; tested merge 26b3cdd6ca50d34b98a4806d92f50d4828077d41; main merge e17e2472f38557552698b8cf9526d6cbf8b25580; run 32849725110; job 97807551403; artifact 9564837027
V07_EVIDENCE: docs/evidence/android-validation/V07.md; tested head 559c1d033bdacedc6900d9ad126e7ab21fd8aa50; tested merge bfa728b840f63a5e9db5d5f376d19fb7f32c62f3; main merge 4b3b15afe6c95f8393147758b6d16e092ac75a21; run 32860034697; job 97841446382; artifact 9567840490
V08_EVIDENCE: docs/evidence/android-validation/V08.md; tested head 08abd4a1a953e62a2c0cdc3e48329de90e870195; tested merge 8cd31f3d9f5f507108e5b91ddd3577748df5c952; main merge 829fd503ba3cd72950b2ec89cfde57f98a1b2417; run 32862330823; job 97849123497; artifact 9568747271
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Sonraki validation turunda yalnız V09 RenderScene/kamera/diagnostics revalidation hattını başlat; aynı turda A10 merge/DONE veya A11 başlatma.
NEXT_IF_TEST_READY: BASLA.md hattında sonraki tur yalnız V09.
NEXT_IF_TEST_OFFLINE: Ayrı BASLA_A10.md sohbetinde yalnız A10 draft branch'i.
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
LAST_UPDATE: 2026-08-25
```

## Yürütme kuralı

Android V01–V09 validation cursor'ı implementation cursor'dan ayrıdır. Normal `BASLA.md` açık VXX'i yürütür; yalnız `BASLA_A10.md` ayrı branch'te A10 taslağını açar. Bir kullanıcı turunda en fazla bir validation/implementation aşaması kapatılır. Kanıtsız PASS/DONE yoktur; queued/zero-step job PASS değildir. A10 main merge/DONE olmadan A11 yoktur. Tarihsel iOS hattı kullanıcı açıkça yeniden etkinleştirmedikçe Android'i bloke etmez.

## Implementation geçmişi

- AŞAMA 00 — çalışma/yürütme zemini — `DONE`.
- AŞAMA 01 — pinned Android toolchain; fiziksel telefon dış kapısı — `BLOCKED / DEFERRED_EXTERNAL_GATE`.
- AŞAMA 02 — dependency/lisans/lockfile — `DONE`.
- AŞAMA 03 — corpus/golden/matris — `DONE`.
- AŞAMA 04 — minimal solution/mimari sınırlar — `DONE`.
- AŞAMA 05 — ACadSharp parser spike — `DONE`; ADR 0001 `GO`.
- AŞAMA 06 — safe-open implementation; real-app emulator bridge V06'da doğrulandı; physical provider/device fidelity release gate'e deferred.
- AŞAMA 07 — ProCad exact source spike — `DONE / NO-GO`; ADR 0002; current production isolation/precision kararı V07'de yeniden doğrulandı.
- AŞAMA 08 — iOS characterization — historical/future; iOS PASS iddiası yok; V08 Android graph isolation `VALIDATED`.
- AŞAMA 09 — immutable RenderScene/kamera/diagnostics foundation — `DONE`; authoritative run `32815175055`, artifact `9551137293`, merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- AŞAMA 10 — P0 geometri renderer — `MAIN'E HENÜZ MERGE EDİLMEDİ`; yalnız `docs/A10_WORKSTREAM.md` kurallarıyla ayrı workstream.
- AŞAMA 11–22 — Android viewer/release hattı; A11, V04–V09 + A10 `DONE ON MAIN` tamamlanana kadar kilitli.
- AŞAMA 23–24 — `DEFERRED_FUTURE_IOS`.
- AŞAMA 25–27 — Android beta/freeze/final handoff.

## Android revalidation geçmişi

### V01 — VALIDATED

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333 / 97721878468`; artifact `9553530359`
- claim `INFRASTRUCTURE_SMOKE_ONLY`

### V02 — VALIDATED

Strict exact dependency policy, locked restore, license/hash, vulnerability, production `src/` boundary ve Android native inventory PASS. ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`; ProCad/iOS-only/unknown native sızıntısı yok.

### V03 — VALIDATED

- tested head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`; tested merge `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875 / 97739039060`; artifact `9555501552`; digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- committed 0BSD DXF + validation-time generated/read-back AC1015 DWG + missing-font/missing-XREF negatives; generated DWG binary golden değildir.

### V04 — VALIDATED

- real Android-only MAUI `MobilDwg.App`, package `com.smitelagwar.mobildwg`
- tested head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`; tested merge `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832 / 97752997848`; artifact `9557331919`; digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`.

### V05 — VALIDATED

- tested head `de39866f8bd71c20fa51b355748ed79884fbb4e6`; main merge `9013d52702d1cb44e378aeacda46ee51e53caa65`
- run/job `32838507832 / 97772635524`; artifact `9561607163`; digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- production ACadSharp 3.7.1 reader real Android app process içinde mini-corpus PASS; writer/save absent; real install/cold-launch/UI parse/stability PASS.
- marker `ANDROID_VALIDATION_V05_PASS`; claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`.

### V06 — VALIDATED

- PR #19; tested head `ae8682875524157285946724bd70d6ff010f3917`; synthetic merge `26b3cdd6ca50d34b98a4806d92f50d4828077d41`; main merge `e17e2472f38557552698b8cf9526d6cbf8b25580`
- run/job `32849725110 / 97807551403`; artifact `9564837027`; digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- MAUI FilePicker → DocumentsUI/SAF → private safe-copy → production parser, DWG/DXF selection, lifecycle/cleanup/immutability PASS.
- marker `ANDROID_VALIDATION_V06_PASS`; claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`.

### V07 — VALIDATED

- PR #20; tested head `559c1d033bdacedc6900d9ad126e7ab21fd8aa50`; synthetic merge `bfa728b840f63a5e9db5d5f376d19fb7f32c62f3`; main merge `4b3b15afe6c95f8393147758b6d16e092ac75a21`
- run/job `32860034697 / 97841446382`; artifact `9567840490`; digest `sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7`
- ProCad/ProCadSharp production/resolved graph + APK absence PASS; rejected `5,000,000 + 0.001` direct-float delta `0`; production double delta `0.001` PASS.
- marker `ANDROID_VALIDATION_V07_PASS`; claim `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`.

### V08 — VALIDATED

- PR #21; tested head `08abd4a1a953e62a2c0cdc3e48329de90e870195`; synthetic merge `8cd31f3d9f5f507108e5b91ddd3577748df5c952`; main merge `829fd503ba3cd72950b2ec89cfde57f98a1b2417`
- authoritative run/job `32862330823 / 97849123497` — SUCCESS
- artifact `9568747271`, 19,064 byte; digest `sha256:6b5172553b65973af7fc3eac4f52f7c14a36048b6861368435bcd2355c062ebd`
- V02 prerequisite PASS; `MobilDwg.App` `net10.0-android36.0`; production project/lockfile/solution/central package graph iOS-specific requirement içermiyor.
- historical Stage08 iOS workflow manual-only; active CI macOS/iOS toolchain zorunluluğu taşımıyor.
- Windows .NET `10.0.400`; recorded workload list yalnız `maui-android`; locked restore + Release build without Xcode PASS.
- APK 30,913,146 byte; SHA-256 `7adf8b2495b2eb7389adf48a1f92d9b57f7a0dade56758a0bbefc1b966075f1b`; iOS native/framework entry yok.
- diagnostic run/job `32862117992 / 97848411995`: cross-platform NuGet package file inventory false-positive; artifact `9568592189`, digest `sha256:98bfa8c20530579ada137f3c1dda0d6244a93f3d7ea1b1360a9e8f302fbde9fd`.
- marker `ANDROID_VALIDATION_V08_PASS`; claim `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED`.

Historical AŞAMA 08 iOS characterization future-only arşivdir; V08 iOS PASS/simulator/device/AOT claim'i değildir.

## Kalıcı teknik kararlar

- Original CAD immutable; production writer/save yok.
- ACadSharp `3.7.1` read-only parser baseline `GO`.
- FilePicker/SAF stream → immediate app-private safe-copy → parser zinciri V06 API36 emulator'da doğrulandı; physical provider/device fidelity ayrı release gate'tir.
- Exact unpatched ProCad production reuse `NO-GO`; V07 current graph/APK izolasyonunu ve precision blocker'ı yeniden kanıtladı.
- UI parser entity'lerine doğrudan bağlanmaz; world/document coordinate hattı `double` precision.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olmaz.
- Production dependency strict exact range + lockfile/locked restore kullanır; unknown native/transitive asset release blocker.
- Gerçek Android app shell `MobilDwg.App`; Stage01Smoke yalnız infrastructure prerequisite'tir.
- Fiziksel Android release öncesi yeniden zorunlu.
- iOS yalnız açık yeni kullanıcı kararıyla etkinleşir; V08 Android production/CI graph'ının iOS-specific gereksinimlerden izole olduğunu kanıtladı.
