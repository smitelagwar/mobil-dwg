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
ANDROID_VALIDATION_CURRENT: V05 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — dependency/lockfile/license/hash/vulnerability/Android-native boundary
V03: VALIDATED — fixture/provenance/golden/Android smoke-set contract
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Yalnız V05'i başlat — ACadSharp parser adapter yolunu gerçek MobilDwg.App içinde V03 DWG/DXF smoke setiyle Android üzerinde doğrula; aynı turda V06'ya geçme.
NEXT_IF_TEST_READY: Bu sohbet V05'i yürütür.
NEXT_IF_TEST_OFFLINE: Test edilebilir exact V05 SHA varsa queue/WAITING_RUNNER; yoksa gerçek stage durumu korunur. Ayrı sohbet BASLA_A10.md ile A10 draft branch'ini yürütür.
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
```

## V01 özeti

Evidence: `docs/evidence/android-validation/V01.md`.

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- toolchain + executable harness + Stage01Smoke emulator infrastructure PASS
- claim limit: `INFRASTRUCTURE_SMOKE_ONLY`; real viewer değil.

## V02 özeti

Evidence: `docs/evidence/android-validation/V02.md`.

- strict exact ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`
- locked restore + license/hash + vulnerability + production graph + Android native boundary PASS
- ProCad/iOS-only/unknown native sızıntısı yok
- claim limit: dependency/native boundary; viewer/fidelity değil.

## V03 özeti

Evidence: `docs/evidence/android-validation/V03.md`.

- branch head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- PR merge test revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060`
- artifact `9555501552`
- digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- redistributable smoke set: committed 0BSD DXF + validation-time AC1015 DWG + missing-font/missing-XREF negative DXF
- authoritative committed-fixture hash: Git blob bytes; generated DWG binary golden değildir.
- claim limit: fixture/provenance/rights/golden/test-matrix.

## V04 özeti

Evidence: `docs/evidence/android-validation/V04.md`.

V04 başlangıcında `MobilDwg.App` yalnız `net10.0` platform-neutral projeydi; gerçek installable Android app yoktu. Mevcut dördüncü production proje Android-only MAUI executable'a dönüştürüldü; beşinci production proje açılmadı.

Gerçek app:

- target `net10.0-android36.0`
- package `com.smitelagwar.mobildwg`
- `MainActivity` + `MainApplication`
- Core/Cad/Rendering dependency sınırları korunuyor
- direct `Microsoft.Maui.Controls` exact `[10.0.100]`, MIT

Final authoritative validation:

- branch head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested PR merge revision `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832` / `97752997848` — SUCCESS
- V02 same-head regression run/job `32832142882` / `97752998222` — SUCCESS
- artifact `9557331919`
- artifact digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- real APK `com.smitelagwar.mobildwg-Signed.apk`, 30,827,130 byte
- APK SHA-256 `60d8d59b3fd452d786519a364875b155d3961c3e4aa210f986c004098789ba42`
- launcher `com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
- cold launch `Status: ok`; PID `3783`
- UI hierarchy, PNG, package/PID crash/ANR ve liveness PASS
- final marker `ANDROID_VALIDATION_V04_PASS`

Claim limit: `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`. V04 parser/render fidelity kanıtlamaz; fiziksel Android hâlâ release/device gate'e deferred.

## V05'te yapılacak iş — henüz başlanmadı

- AŞAMA 05 parser/corpus/diagnostics executable testleri yeniden çalıştırılacak.
- V03 smoke setindeki en az bir DWG ve bir DXF gerçek `MobilDwg.App` içindeki ACadSharp adapter yolundan Android üzerinde parse edilecek.
- Writer/save production graph'a girmeyecek; original input immutable kalacak.
- Pozitif parse + kontrollü negatif + redacted diagnostic evidence alınacak.
- Host-only parser PASS ile real Android app parser PASS karıştırılmayacak.

V05 bu V04 kapanış turunda başlatılmadı.

## Paralel A10 yolu

- Normal `BASLA.md`/bu dosya açık V05→V09 validation hattına gider.
- Bilgisayar veya runner kapalıyken kullanıcı başka sohbette `BASLA_A10.md dosyasını oku` der.
- A10 yalnız `stage10-p0-geometry-draft` branch'inde dondurulmuş sözleşmelere dokunmayan host-independent taslak işi yapar. PC/runner kapalıyken workflow filtreleri kontrol edilir; açık A10 PR'ı yoksa branch commit/push yapılabilir. PR zaten açıksa push `synchronize` sayılacağından önce PR kapatılır/etki güvenle gate edilir, aksi halde offline push yapılmaz.
- Host/GitHub-hosted kontrol sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması + Android gate bekleyen durum `CODED_PENDING_EMULATOR`dır. `main` merge/DONE yoktur; `android-test` branch'ine A10 sohbeti dokunmaz.
- V09 sonrası güncel validated `main` ile integration; V04–V07, V08 Android graph-isolation, V09 ve expected-content içeren real-app API 36 emulator render gate tamamlanır. iOS workflow açılmaz; A10 `DONE ON MAIN` olmadan A11 açılmaz.
