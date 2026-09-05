# mobil-dwg — Proje geçmişi ve AI handoff kaydı

Bu dosya kısa kalıcı tarihçe/checkpoint kaydıdır. Ayrıntılı teknik kanıt `docs/evidence/`, kararlar `docs/ADR/`, Android V01–V09 programı `ANDROID_DOGRULAMA_PLANI.md` içindedir. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni ajan okuma sırası

1. Gerçek `main` HEAD, açık branch/PR ve Actions durumunu doğrula.
2. `BASLA.md` ve `DEVAM.md`.
3. Canonical plan ve `ANDROID_DOGRULAMA_PLANI.md`.
4. `docs/A10_WORKSTREAM.md` ve gerçek açık A10 branch/PR durumu.
5. Son ilgili evidence/ADR.
6. Remote GitHub bağlamındaysa `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md`.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg`, default `main`.
- Aktif v1: Android-only, local/offline, read-only 2D DWG/DXF viewer.
- iOS: `DEFERRED_FUTURE_OPTION`; tarihsel characterization future-only.
- v1 dışı: edit/save/export/cloud/account.
- Original CAD immutable; production writer/save yok.
- ACadSharp `3.7.1` read-only parser baseline `GO`.
- Exact unpatched ProCad production reuse `NO-GO`.

## Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 11 — DONE
ANDROID_VALIDATION_V01_V09: CLOSED / VALIDATED_WITH_CLAIM_LIMITS
LAST_IMPLEMENTATION: AŞAMA 11 — DONE
LAST_IMPLEMENTATION_EVIDENCE: docs/evidence/STAGE_11.md
A11_MAIN_MERGE: 51e8b5b (PR #24)
IMPLEMENTATION_CURSOR: AŞAMA 12 — NOT_STARTED
A10_WORKSTREAM: docs/A10_WORKSTREAM.md (DONE)
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
A12_GATE: OPEN
NEXT_ACTION: Sonraki normal BASLA/devam turunda AŞAMA 12'yi (DWG R13–R2018 Parser Çekirdeği) başlat.
LAST_UPDATE: 2026-09-05
```

## Implementation geçmişi

- AŞAMA 00 — execution/evidence/ADR/handoff zemini — `DONE`.
- AŞAMA 01 — pinned Android toolchain; fiziksel cihaz kapısı — implementation baseline mevcut, external device gate release'e açık.
- AŞAMA 02 — dependency/lisans/lockfile — `DONE`.
- AŞAMA 03 — corpus/golden/matris — `DONE`.
- AŞAMA 04 — Core/Cad/Rendering/App architecture — `DONE`.
- AŞAMA 05 — ACadSharp parser baseline — `DONE / GO`.
- AŞAMA 06 — safe-open implementation — host implementation tamam; physical provider/device fidelity release'e deferred.
- AŞAMA 07 — ProCad exact source spike — `DONE / NO-GO`.
- AŞAMA 08 — iOS characterization — historical/future-only; iOS PASS iddiası yok.
- AŞAMA 09 — immutable RenderScene/camera/diagnostics foundation — `DONE`; historical authoritative run `32815175055`, artifact `9551137293`, main merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- AŞAMA 10 — P0 geometry renderer — `DONE`; PR `#23`, main merge `ddeb975`, evidence `docs/evidence/STAGE_10.md`.
- AŞAMA 11 — Mobil viewport ve gesture — `DONE`; PR `#24`, main merge `51e8b5b`, evidence `docs/evidence/STAGE_11.md`.
- AŞAMA 12–22 — Android viewer/release implementation sırası; cursor AŞAMA 12'de.
- AŞAMA 23–24 — future iOS track, deferred.
- AŞAMA 25–27 — Android beta/freeze/final handoff.

## Android revalidation geçmişi

### V01 — VALIDATED

`INFRASTRUCTURE_SMOKE_ONLY`; evidence `docs/evidence/android-validation/V01.md`.

### V02 — VALIDATED

Strict exact dependency/lockfile/license/hash/vulnerability/Android-native/source boundary; evidence `docs/evidence/android-validation/V02.md`.

### V03 — VALIDATED

Fixture/provenance/golden/Android smoke-set sözleşmesi; evidence `docs/evidence/android-validation/V03.md`.

### V04 — VALIDATED

Gerçek installable Android-only MAUI app shell API36 build/install/cold-launch/UI/stability; claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`.

### V05 — VALIDATED

- tested head `de39866f8bd71c20fa51b355748ed79884fbb4e6`
- main merge `9013d52702d1cb44e378aeacda46ee51e53caa65`
- run/job `32838507832 / 97772635524`
- artifact `9561607163`, digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`

### V06 — VALIDATED

- PR `#19`; tested head `ae8682875524157285946724bd70d6ff010f3917`
- main merge `e17e2472f38557552698b8cf9526d6cbf8b25580`
- run/job `32849725110 / 97807551403`
- artifact `9564837027`, digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`

### V07 — VALIDATED

- PR `#20`; tested head `559c1d033bdacedc6900d9ad126e7ab21fd8aa50`
- main merge `4b3b15afe6c95f8393147758b6d16e092ac75a21`
- run/job `32860034697 / 97841446382`
- artifact `9567840490`, digest `sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7`
- ProCad production graph/APK absence + rejected float precision blocker + production double precision regression
- claim `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`

### V08 — VALIDATED

- PR `#21`; tested head `08abd4a1a953e62a2c0cdc3e48329de90e870195`
- main merge `829fd503ba3cd72950b2ec89cfde57f98a1b2417`
- run/job `32862330823 / 97849123497`
- artifact `9568747271`, digest `sha256:6b5172553b65973af7fc3eac4f52f7c14a36048b6861368435bcd2355c062ebd`
- Android production/CI graph iOS isolation
- claim `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED`

### V09 — VALIDATED

V09 historical AŞAMA 09 RenderScene/camera/diagnostics temelini current Android product graph üzerinde yeniden doğruladı; A10 geometry renderer işi başlatılmadı.

Authoritative final:

- PR `#22`
- tested head `892315966f895729e866947a838df93350fdfd97`
- exact checked-out PR synthetic merge `6fea8ba9d1de6811afd0dcace7a2c8b5b6ec573a`; tested head'e göre file diff yok
- main merge `143ce1a79448f53af81faee9c6e650321047dd37`
- run/job `32864617493 / 97856686115` — SUCCESS
- artifact `9569686660`, 11,544 byte
- digest `sha256:97e55129367ea5b778edf99a6d84939e95f74902db655144d32dbf24ba8aa375`
- same-job V02 prerequisite PASS
- exact .NET `10.0.400`
- RenderScene Release build `0 Warning / 0 Error`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`
- deterministic `render-scene/v1`
- snapshot line `entity=E-001|0|BYLAYER|5000000,-25,5000000.001,100|SYNTHETIC|A1|1`
- `V09_RENDER_SCENE_CAMERA_OCS_DIAGNOSTICS_PASS`
- `V09_SEMANTIC_SNAPSHOT_DETERMINISM_PASS`
- `V09_SURVEY_ORIGIN_DOUBLE_PRECISION_PASS delta=0.001`
- `STAGE04_CORE_CONTRACT_TESTS_PASS`
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`
- full solution Release build `0 Warning / 0 Error`
- real Android Release APK `30,913,146` byte; SHA-256 `a0080fb4826cbd6f7fee1d84cac3465c8ebda766bfba245167d73233ab1a40f5`
- marker `ANDROID_VALIDATION_V09_PASS`
- claim `RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY`

İlk V09 run/job `32864458158 / 97856153440`, Windows PowerShell 5.1 validation portability false-negative'i nedeniyle ürün testlerinden önce durdu. Diagnostic artifact `9569504762`, digest `sha256:7eda4ec7db3d423cdbd476bc4769eebac54ef0527c18656c0fc2bbd0b2eb90f8`. Production kaynak/test semantics değiştirilmeden gate PowerShell 5.1 uyumlu hale getirildi.

## Kalıcı teknik kararlar

- Original CAD immutable; production writer/save yok.
- ACadSharp exact `3.7.1` read-only parser baseline `GO`.
- FilePicker/SAF immediate app-private safe-copy zinciri V06 emulator'da doğrulandı; physical provider/device fidelity release gate'tir.
- ProCad production reuse `NO-GO`.
- UI parser entity'lerine doğrudan bağlanmaz.
- World/document coordinates `double`; survey-origin `0.001` precision V07 ve V09'da yeniden doğrulandı.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olmaz.
- Runtime dependency exact pin + lockfile + locked restore + license/native audit kullanır.
- iOS yalnız açık kullanıcı kararıyla yeniden etkinleşir.
- A10 gerçek renderer entegrasyonudur; V09 foundation PASS'i geometri render fidelity PASS'i değildir.
