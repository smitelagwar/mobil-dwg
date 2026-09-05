# Execution Log

Bu dosya teknik yürütme geçmişinin kısa indeksidir. Ayrıntılı kanıt `docs/evidence/`, kararlar `docs/ADR/`, aktif handoff `DEVAM.md` / `gecmis.md` içindedir. Kanıtsız başarı yazılmaz.

## Implementation özeti

- 2026-08-24 — AŞAMA 00 — `DONE`: execution/evidence/ADR/handoff zemini.
- 2026-08-24 — AŞAMA 01 — pinned Android toolchain; physical-device external gate açık.
- 2026-08-24 — AŞAMA 02 — `DONE`: dependency/lisans/lockfile.
- 2026-08-24 — AŞAMA 03 — `DONE`: mini corpus/golden.
- 2026-08-24 — AŞAMA 04 — `DONE`: Core/Cad/Rendering/App architecture.
- 2026-08-24 — AŞAMA 05 — `DONE / GO`: ACadSharp `3.7.1` parser baseline.
- 2026-08-24 — AŞAMA 06 — safe-open implementation; physical provider/device fidelity deferred.
- 2026-08-24 — AŞAMA 07 — `DONE / NO-GO`: exact ProCad source reuse rejected.
- 2026-08-25 — AŞAMA 08 — historical iOS characterization; future-only.
- 2026-08-25 — AŞAMA 09 — `DONE`: RenderScene/camera/diagnostics; run `32815175055`, artifact `9551137293`, merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- Implementation cursor after V09 closure: AŞAMA 10 — `NOT_STARTED`.

## Android validation V01–V08

- V01 `VALIDATED` — `INFRASTRUCTURE_SMOKE_ONLY`; run/job `32821991333 / 97721878468`; artifact `9553530359`.
- V02 `VALIDATED` — exact dependency/lockfile/license/hash/vulnerability/Android-native boundary.
- V03 `VALIDATED` — fixture/provenance/golden/Android smoke-set contract; run/job `32827625875 / 97739039060`; artifact `9555501552`.
- V04 `VALIDATED` — real Android app shell runtime; run/job `32832142832 / 97752997848`; claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`.
- V05 `VALIDATED` — real Android production parser smoke; run/job `32838507832 / 97772635524`; artifact `9561607163`; claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`.
- V06 `VALIDATED` — real FilePicker/SAF safe-open API36 emulator; run/job `32849725110 / 97807551403`; artifact `9564837027`; claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`.
- V07 `VALIDATED` — ProCad NO-GO graph isolation + precision; run/job `32860034697 / 97841446382`; artifact `9567840490`; claim `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`.
- V08 `VALIDATED` — Android production/CI graph iOS isolation; run/job `32862330823 / 97849123497`; artifact `9568747271`; claim `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED`.

## 2026-08-25 — Android validation V09 — VALIDATED

Scope: historical AŞAMA 09 RenderScene/camera/OCS/diagnostics foundation + current Android composition revalidation. A10 geometry renderer implementationi bu turda başlatılmadı.

### İlk diagnostic execution

- PR `#22`, first head `fd2d313ee8d7fb3437a1c8536930aaea1f7547d8`.
- run/job `32864458158 / 97856153440` — real non-zero-step execution.
- V02 prerequisite PASS.
- V09 host/.NET checks başladı; Windows PowerShell 5.1 `.Contains(string, StringComparison)` overload'u bulunamadığı için validation script ürün testlerinden önce durdu.
- classification: validation portability false-negative; production/test failure değil.
- diagnostic artifact `9569504762`, 4,594 byte.
- digest `sha256:7eda4ec7db3d423cdbd476bc4769eebac54ef0527c18656c0fc2bbd0b2eb90f8`.
- fix: contract scan `.IndexOf(token, StringComparison.Ordinal) -lt 0` olarak PowerShell 5.1 uyumlu hale getirildi; production kod/test semantics değişmedi.

### Authoritative execution

- PR `#22`.
- tested head `892315966f895729e866947a838df93350fdfd97`.
- exact checked-out PR synthetic merge `6fea8ba9d1de6811afd0dcace7a2c8b5b6ec573a`.
- head → synthetic merge compare: `files=[]`; yalnız merge ancestry.
- main merge `143ce1a79448f53af81faee9c6e650321047dd37`.
- workflow `Android V09 Render Scene Revalidation`.
- run/job `32864617493 / 97856686115` — `SUCCESS`.
- self-hosted runner `DESKTOP-PKLGPNQ-mobil-dwg-runner`.
- artifact `9569686660`, 11,544 byte.
- artifact digest `sha256:97e55129367ea5b778edf99a6d84939e95f74902db655144d32dbf24ba8aa375`.

### Authoritative markers / results

- same-job `ANDROID_VALIDATION_V02_PASS`.
- `V09_WINDOWS_VALIDATION_HOST_PASS`.
- `V09_DOTNET_PIN_PASS version=10.0.400`.
- `V09_REQUIRED_TEST_CONTRACT_PRESENT_PASS`.
- RenderScene targeted Release build: `0 Warning / 0 Error`.
- `V09_RENDER_FOUNDATION_BUILD_PASS`.
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`.
- `STAGE09_RENDER_SCENE_TESTS_PASS`.
- deterministic semantic format `render-scene/v1`.
- actual survey snapshot: `entity=E-001|0|BYLAYER|5000000,-25,5000000.001,100|SYNTHETIC|A1|1`.
- deterministic `Unsupported` and `Substituted` diagnostic snapshot lines.
- `V09_RENDER_SCENE_CAMERA_OCS_DIAGNOSTICS_PASS`.
- `V09_SEMANTIC_SNAPSHOT_DETERMINISM_PASS`.
- `V09_SURVEY_ORIGIN_DOUBLE_PRECISION_PASS delta=0.001`.
- Core Release build/test: `STAGE04_CORE_CONTRACT_TESTS_PASS`, `V09_CORE_RENDER_CONTRACT_REGRESSION_PASS`.
- Architecture Release build/test: `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`.
- `V09_CORE_CAD_RENDERING_APP_COMPOSITION_BOUNDARY_PASS`.
- full seven-project solution Release build: `0 Warning / 0 Error`.
- `V09_FULL_SOLUTION_RELEASE_BUILD_PASS`.
- real `MobilDwg.App` Release APK: `30,913,146` byte; SHA-256 `a0080fb4826cbd6f7fee1d84cac3465c8ebda766bfba245167d73233ab1a40f5`.
- `V09_REAL_ANDROID_APP_COMPOSITION_BUILD_PASS`.
- `ANDROID_VALIDATION_V09_PASS`.
- claim `RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY`.

V09 APK install/launch/UI/render-fidelity veya physical-device claim'i değildir. Önceki V04–V06 runtime claim'leri ayrı sınırlarında korunur.

## Program kapanışı (V01–V09)

```text
ANDROID_VALIDATION_V01_V09: CLOSED / VALIDATED_WITH_CLAIM_LIMITS
PENDING_EMULATOR_QUEUE: EMPTY
```

## AŞAMA 10 — P0 Temel Geometri Renderer'ı

- PR: `#23` (`feat(a10): implement P0 geometry renderer and Android render acceptance`) — `MERGED`.
- Base `main`: `3ebf8226b8f133255e65cafdec9f7f26fbe7afbe`.
- PR head SHA: `b9ca27e`.
- Main merge commit: `ddeb975`.
- Exact .NET SDK: `10.0.400`.
- Release build: `0 Warning / 0 Error`.
- Release APK: `39,543,728` byte; SHA-256 `ec35abf74dcefaaa70a29845d32b1791ff3a8160ecb7aad99bcab6c012a89b70`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`).
- Canlı PID: `6257`.
- Beklenen içerik piksel sayısı: `56,163` piksel.
- Ekran görüntüsü: `133,801` byte; SHA-256 `52b14a1e622526163b0ed0e927b7ec0e0a97c9385dc2635d911949c2e1b6ea50`.
- Host test belirteçleri: `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE10_CONTROLLED_INVALID_GEOMETRY_WARNING_PASS`, `STAGE10_SKIA_EXPECTED_CONTENT_HOST_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`.
- Android emülatör belirteçleri: `A10_ANDROID_SEMANTIC_GOLDEN_PASS`, `A10_ANDROID_EXPECTED_CONTENT_PASS pixels=56163`, `A10_ANDROID_PNG_PASS`, `ANDROID_STAGE10_P0_GEOMETRY_RENDER_PASS`, `A10_REAL_APP_UI_IMAGE_READY`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE10_P0_GEOMETRY_RENDER_PASS` doğrulanarak `A10_REAL_APP_UI_RENDER_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı.
- Claim limit: `P0_SYNTHETIC_SCENE_GEOMETRY_RENDERER_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_10.md`.

## AŞAMA 11 — Mobil Viewport ve Gesture

- PR: `#24` (`feat(a11): implement mobile viewport and gesture interaction`) — `MERGED`.
- Base `main`: `7ce9508afbb97fce940584661ea16ff5c95d4f0b`.
- PR head SHA: `6fe3a53`.
- Main merge commit: `51e8b5b`.
- Exact .NET SDK: `10.0.400`.
- Release build: `0 Warning / 0 Error`.
- Release APK: `39,249,942` byte; SHA-256 `ea25e618408134e8e0a417864f6e7c2d7291a9228194b9b1ff0f59e27c9cc644`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`, serial `emulator-5554`).
- Canlı PID: `7085`.
- Ekran görüntüsü: `136,477` byte; SHA-256 `80404ed972891c348a56d43db84937b5638480496c9c9809ab23468823fb2d00`.
- Host test belirteçleri: `STAGE11_VIEWPORT_GESTURE_TESTS_PASS` (Pan, ZoomAt focal preservation, double tap, fit extents, orientation resize no-reparse, telemetry), `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE10_CONTROLLED_INVALID_GEOMETRY_WARNING_PASS`, `STAGE10_SKIA_EXPECTED_CONTENT_HOST_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`.
- Android emülatör belirteçleri: `A11_ANDROID_PAN_PASS`, `A11_ANDROID_FOCAL_PRESERVATION_PASS`, `A11_ANDROID_PINCH_ZOOM_PASS`, `A11_ANDROID_DOUBLE_TAP_PASS`, `A11_ANDROID_FIT_EXTENTS_PASS`, `A11_ANDROID_ORIENTATION_RESIZE_PASS`, `A11_ANDROID_PNG_PASS`, `ANDROID_STAGE11_VIEWPORT_GESTURE_PASS`, `A11_REAL_APP_UI_IMAGE_READY`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE11_VIEWPORT_GESTURE_PASS` doğrulanarak `A11_REAL_APP_UI_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı (`A11_REAL_APP_STABILITY_PASS pid=7085`).
- Claim limit: `A11_VIEWPORT_GESTURE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_11.md`.

```text
IMPLEMENTATION_BASELINE: AŞAMA 11 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 12 — NOT_STARTED
A12_GATE: OPEN
NEXT_ACTION: Sonraki normal BASLA/devam turunda AŞAMA 12'yi (DWG R13–R2018 Parser Çekirdeği) başlat.
```

