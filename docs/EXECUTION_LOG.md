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

## AŞAMA 12 — Block / INSERT / Attribute

- PR: `#25` (`feat(a12): implement block, insert, and attribute expansion`) — `MERGED`.
- Base `main`: `62104933af8ca1066c9870dce6099c8732f65fb7`.
- PR head SHA: `e1ae799`.
- Main merge commit: `4752a17`.
- Exact .NET SDK: `10.0.400`.
- Release build: `0 Warning / 0 Error`.
- Release APK: `39,270,422` byte; SHA-256 `b9acd8f1de0d847b2ac5a6492d587b0594782d77fab9fbd1c6c0bc2dafd8c155`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`, serial `emulator-5554`).
- Canlı PID: `7926`.
- Ekran görüntüsü: `96,079` byte; SHA-256 `7ef1abd77ce9f0d052775e9aeb79cea81bae2eba078205ba41cc2ecc0b9761b5`.
- Host test belirteçleri: `STAGE12_BLOCK_INSERT_TESTS_PASS` (Transform2D, Non-uniform scale, mirror, nested hierarchy, Layer 0, ByBlock, attributes, cycle guard, depth guard, budget guard, semantic golden snapshot), `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`, `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`.
- Android emülatör belirteçleri: `A12_ANDROID_NESTED_TRANSFORM_PASS`, `A12_ANDROID_NON_UNIFORM_SCALE_MIRROR_PASS`, `A12_ANDROID_LAYER0_BYBLOCK_INHERITANCE_PASS`, `A12_ANDROID_ATTRIB_PASS`, `A12_ANDROID_CYCLE_DEPTH_BUDGET_GUARDS_PASS`, `A12_ANDROID_PNG_PASS`, `ANDROID_STAGE12_BLOCK_INSERT_PASS`, `A12_REAL_APP_UI_IMAGE_READY`, `A12_REAL_APP_UI_STATUS_PASS`, `A12_REAL_APP_STABILITY_PASS`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE12_BLOCK_INSERT_PASS` doğrulanarak `A12_REAL_APP_UI_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı (`A12_REAL_APP_STABILITY_PASS pid=7926`).
- Claim limit: `CLAIM_LIMIT=A12_BLOCK_INSERT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_12.md`.

## AŞAMA 13 — Layer / Color / Linetype / Lineweight

- PR: `#26` (`feat(a13): implement layer, color, linetype, and lineweight styling`) — `MERGED`.
- Base `main`: `9861b0d729ba28dc8dbfb813be00b0caf31ca8d6`.
- PR head SHA: `f2126fe`.
- Main merge commit: `e60e498`.
- Exact .NET SDK: `10.0.400`.
- Release build: `0 Warning / 0 Error`.
- Release APK: `39,282,710` byte; SHA-256 `530571d2a269ccb52d8752b73de693deb69dab67b6137309f3845158c0bb0b6c`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`, serial `emulator-5554`).
- Canlı PID: `8595`.
- Ekran görüntüsü: `86,413` byte; SHA-256 `f51bbfa1d5536c5c1ccd61be120831a7bdaaa0171812b6813bc5845774ee79b0`.
- Host test belirteçleri: `STAGE13_LAYER_STYLE_TESTS_PASS` (ACI 1-255 palette lookup, ACI 7 dynamic contrast inversion, TrueColor RGB, ByLayer inheritance, ByBlock inheritance, Layer visibility toggle, Layer freeze toggle, standard linetypes, complex linetype fallback warning, lineweight pixel conversion, unknown layer fallback, deterministic layer-style semantic snapshot), `STAGE12_BLOCK_INSERT_TESTS_PASS`, `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`, `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`.
- Android emülatör belirteçleri: `A13_ANDROID_ACI_TRUECOLOR_PASS`, `A13_ANDROID_BYLAYER_BYBLOCK_PASS`, `A13_ANDROID_LAYER_VISIBILITY_FREEZE_PASS`, `A13_ANDROID_LINETYPE_LINEWEIGHT_PASS`, `A13_ANDROID_COMPLEX_STYLE_WARNING_PASS`, `A13_ANDROID_PNG_PASS`, `ANDROID_STAGE13_LAYER_STYLE_PASS`, `A13_REAL_APP_UI_IMAGE_READY`, `A13_REAL_APP_UI_STATUS_PASS`, `A13_REAL_APP_STABILITY_PASS`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE13_LAYER_STYLE_PASS` doğrulanarak `A13_REAL_APP_UI_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı (`A13_REAL_APP_STABILITY_PASS pid=8595`).
- Claim limit: `CLAIM_LIMIT=A13_LAYER_STYLE_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_13.md`.

### 2026-09-05: AŞAMA 14 — TEXT / MTEXT / Türkçe / Font / SHX Tamamlandı ve Merge Edildi (PR #27)

- PR: `#27` (`feat(a14): implement text, mtext, turkish encoding, and font substitution`).
- `main` merge commit: `3803f25`.
- Release APK: `39,621,552` byte; SHA-256 `2f853f638daba940b45f1685b92cb92750a52f09e16d0017e312c0430fe33b90`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`, serial `emulator-5554`).
- Canlı PID: `8961`.
- Ekran görüntüsü: `113,001` byte; SHA-256 `ad06c5c032d89f0f6000188aa1f3f895da3af930208cc76623ac778d9b05c3f7`.
- Host test belirteçleri: `STAGE14_TEXT_FONT_TESTS_PASS` (Turkish character encoding CP1254/UTF-8, AutoCAD Unicode escapes `\U+XXXX`, AutoCAD symbols `%%d`, `%%p`, `%%c`, `%%%`, bounded MTEXT parser with `\P`, MTEXT nesting guard `MTEXT_NESTING_EXCEEDED`, zero-proprietary font substitution table `txt.shx`, `romans.shx`, `simplex.shx`, `isocp.shx` -> `sans-serif`, unknown font fallback, text alignment calculations, text mirror flags and rotation, text world bounds calculation, SkiaSharp text rendering dark and light themes, deterministic text-scene semantic snapshot), `STAGE13_LAYER_STYLE_TESTS_PASS`, `STAGE12_BLOCK_INSERT_TESTS_PASS`, `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`, `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`.
- Android emülatör belirteçleri: `A14_ANDROID_TURKISH_UNICODE_PASS`, `A14_ANDROID_AUTOCAD_ESCAPES_PASS`, `A14_ANDROID_BOUNDED_MTEXT_PASS`, `A14_ANDROID_FONT_SUBSTITUTION_PASS`, `A14_ANDROID_ALIGNMENT_MIRROR_PASS`, `A14_ANDROID_SKIA_TEXT_PNG_PASS`, `ANDROID_STAGE14_TEXT_FONT_PASS`, `A14_REAL_APP_UI_IMAGE_READY`, `A14_REAL_APP_UI_STATUS_PASS`, `A14_REAL_APP_STABILITY_PASS`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE14_TEXT_FONT_PASS` doğrulanarak `A14_REAL_APP_UI_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı (`A14_REAL_APP_STABILITY_PASS pid=8961`).
- Claim limit: `CLAIM_LIMIT=A14_TEXT_FONT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_14.md`.

### 2026-09-05: AŞAMA 15 — Dimension / Leader / Hatch Tamamlandı ve Merge Edildi (PR #28)

- PR: `#28` (`feat(rendering): Stage 15 - Dimension, Leader, Hatch and Skia Render`).
- `main` merge commit: `c99ba86`.
- Release APK: `39,340,054` byte; SHA-256 `9a7531eb8c9b4946ba24374c5d31655cc765a9855cffe21b8bd3109b5f42617b`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`, serial `emulator-5554`).
- Canlı PID: `9288`.
- Ekran görüntüsü: `111,314` byte; SHA-256 `bfe4fac1932f7c2168f529afa2bd1454bcbb2873a5c3250986d1b86d1c6c6b4d`.
- Host test belirteçleri: `STAGE15_DIMENSION_HATCH_TESTS_PASS` (13/13 test: anonymous dimension block preference, procedural aligned/rotated/radial/diametric dimensions, degenerate defpoints guards, NaN coordinate guard, leader & multileader geometry, hatch auto-closure tolerance <= 1mm, broken boundary diagnostic > 1mm, EvenOdd nested islands normal style, ANSI31 clipped pattern line generation, deterministic dimension-hatch semantic snapshot `schema=dim-hatch/v1`), `STAGE14_TEXT_FONT_TESTS_PASS`, `STAGE13_LAYER_STYLE_TESTS_PASS`, `STAGE12_BLOCK_INSERT_TESTS_PASS`, `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`, `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`.
- Android emülatör belirteçleri: `A15_ANDROID_ANONYMOUS_BLOCK_PASS`, `A15_ANDROID_PROCEDURAL_DIMENSIONS_PASS`, `A15_ANDROID_DEGENERATE_GUARDS_PASS`, `A15_ANDROID_LEADER_PASS`, `A15_ANDROID_HATCH_PROCESSING_PASS`, `A15_SCENE_ENTITIES_COUNT=8`, `A15_HATCH_ISLAND_EVENODD_VERIFIED loops=2`, `A15_ANSI31_PATTERN_LINES_COUNT=19`, `A15_RENDER_PIXELS=58280`, `A15_SNAPSHOT_HASH=3edb1660f76aaf46a751593fb4bb0d0cf27aa5845267a1f01ddbd222a6a45578`, `A15_ANDROID_SKIA_RENDER_PASS bytes=16859 sha256=bc28ab30f1f6ede833aac316a61f00d3a790b90339ea3c63a63dc5de32f3015b`, `ANDROID_STAGE15_DIMENSION_HATCH_PASS`, `A15_REAL_APP_UI_IMAGE_READY`, `A15_REAL_APP_UI_STATUS_PASS`, `A15_REAL_APP_STABILITY_PASS`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE15_DIMENSION_HATCH_PASS` doğrulanarak `A15_REAL_APP_UI_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı (`A15_REAL_APP_STABILITY_PASS pid=9288`).
- Claim limit: `CLAIM_LIMIT=A15_DIMENSION_HATCH_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_15.md`.

### 2026-09-05: AŞAMA 16 — Model / Layout / Paper-Space / Viewport Tamamlandı ve Merge Edildi (PR #29)

- PR: `#29` (`feat(stage16): model/layout paper-space and viewport rendering`).
- `main` merge commit: `b978b84`.
- Release APK: `39,348,246` byte; SHA-256 `54ff0923b025a34b6cdfb553350b5f857e2a589f74379e2899aa8d4e214235e5`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`, serial `emulator-5554`).
- Canlı PID: `9804`.
- Ekran görüntüsü: `110,781` byte; SHA-256 `44766684bf7f285a6edd2eff29fcf36b372004dc43abb36f2d87fcf9078e4e62`.
- Host test belirteçleri: `STAGE16_LAYOUT_VIEWPORT_TESTS_PASS` (12/12 test: model space direct entities, paper space title block & border, viewport model-to-paper transform matrix with scale, center translation and twist angle rotation, viewport layer overrides via frozen layers, viewport Skia clipping with ClipRect/ClipPath, degenerate viewport zero dimensions guard `INVALID_VIEWPORT_GEOMETRY`, degenerate viewport NaN coordinates guard, zero-reparse layout switching, multiple viewports on single sheet, Skia render paper layout with viewports, deterministic layout scene semantic snapshot `schema=layout-scene/v1`), `STAGE15_DIMENSION_HATCH_TESTS_PASS`, `STAGE14_TEXT_FONT_TESTS_PASS`, `STAGE13_LAYER_STYLE_TESTS_PASS`, `STAGE12_BLOCK_INSERT_TESTS_PASS`, `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`, `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`.
- Android emülatör belirteçleri: `A16_ANDROID_MODEL_SPACE_PASS`, `A16_ANDROID_ZERO_REPARSE_PASS`, `A16_ANDROID_LAYER_OVERRIDE_PASS`, `A16_ANDROID_DEGENERATE_GUARD_PASS`, `A16_LAYOUT_ACTIVE=Sheet-A101`, `A16_PAPER_ENTITIES_COUNT=5`, `A16_RENDER_PIXELS=47573`, `A16_SNAPSHOT_HASH=d0d21650b2849d413e174b29bbec47867201e51b184f4f7fa873763c3293883a`, `A16_ANDROID_SKIA_RENDER_PASS bytes=15273 sha256=17df0e10b1a03f47c3e38708ba1c3f5d5e53316d29944a9544cce8f6f0cff4cb`, `ANDROID_STAGE16_LAYOUT_VIEWPORT_PASS`, `A16_REAL_APP_UI_IMAGE_READY`, `A16_REAL_APP_UI_STATUS_PASS`, `A16_REAL_APP_STABILITY_PASS`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE16_LAYOUT_VIEWPORT_PASS` doğrulanarak `A16_REAL_APP_UI_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı (`A16_REAL_APP_STABILITY_PASS pid=9804`).
- Claim limit: `CLAIM_LIMIT=A16_LAYOUT_VIEWPORT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_16.md`.

### 2026-09-05: AŞAMA 17 — XREF / Raster Image / Underlay / External References & Compatibility Tamamlandı ve Merge Edildi (PR #30)

- PR: `#30` (`feat(stage17): xref raster underlay references and compatibility rendering`).
- `main` merge commit: `dd9727b`.
- Release APK: `39,389,206` byte; SHA-256 `376cdfd354b35e587e4bf2d8e2317a377a77f29446a1d85f4005983c4b2c3c3c`.
- Android Emulator: `sdk_gphone64_x86_64` (Android 16 / API 36 / `x86_64`, serial `emulator-5554`).
- Canlı PID: `10383`.
- Ekran görüntüsü: `155,161` byte; SHA-256 `9c011820b640080acfd3500272a0a578a57b9b381d874fe459af14b289ed59f5`.
- Host test belirteçleri: `STAGE17_REFERENCE_COMPATIBILITY_TESTS_PASS` (12/12 test: unresolved XREF emits diagnostic and generates placeholder border+cross+label, missing raster image placeholder, missing PDF underlay placeholder, remote URL rejected `XREF_REMOTE_NOT_SUPPORTED` with zero network calls, bounded directory resolver case-insensitive file matching, path traversal attempt blocked `PATH_TRAVERSAL_PREVENTED`, resolved local raster image primitive creation, Skia render raster image non-background pixels, raster clipping boundary, raster fade/transparency, composite scene with resolved raster and missing references, deterministic external reference snapshot `schema=xref-compat/v1`), `STAGE16_LAYOUT_VIEWPORT_TESTS_PASS`, `STAGE15_DIMENSION_HATCH_TESTS_PASS`, `STAGE14_TEXT_FONT_TESTS_PASS`, `STAGE13_LAYER_STYLE_TESTS_PASS`, `STAGE12_BLOCK_INSERT_TESTS_PASS`, `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`, `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`, `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`, `STAGE10_P0_SEMANTIC_GOLDEN_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`.
- Katman Mimari Testleri: `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `V04_REAL_ANDROID_APP_PROJECT_PASS`.
- Android emülatör belirteçleri: `A17_ANDROID_REMOTE_REJECTED_PASS`, `A17_ANDROID_SECURITY_TRAVERSAL_PASS`, `A17_RENDER_PIXELS=43820`, `A17_SNAPSHOT_HASH=1c4f5ea78d46db132488880e46b9a89c9e88bf4b98687a412e8b28cf9ff7036a`, `A17_ANDROID_SKIA_RENDER_PASS bytes=17942 sha256=d32ef7ef0ad3ad0f70a1a8c04ec4283bba8c919d71457199c4c478a87383fc44`, `ANDROID_STAGE17_XREF_COMPAT_PASS`, `A17_REAL_APP_UI_IMAGE_READY`, `A17_REAL_APP_UI_STATUS_PASS`, `A17_REAL_APP_STABILITY_PASS`.
- UI Doğrulaması: `window.xml` hiyerarşisinde `ANDROID_STAGE17_XREF_COMPAT_PASS` doğrulanarak `A17_REAL_APP_UI_STATUS_PASS` alındı.
- Kararlılık: Paket ve PID kapsamında crash/ANR yok, uygulama canlı kaldı (`A17_REAL_APP_STABILITY_PASS pid=10383`).
- Claim limit: `CLAIM_LIMIT=A17_XREF_COMPAT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`.
- Kanıt belgesi: `docs/evidence/STAGE_17.md`.

```text
IMPLEMENTATION_BASELINE: AŞAMA 17 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 18 — NOT_STARTED
A18_GATE: OPEN
NEXT_ACTION: Sonraki normal BASLA/devam turunda AŞAMA 18'i (Tam Android viewer UX / lifecycle) başlat.
```


