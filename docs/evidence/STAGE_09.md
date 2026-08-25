# AŞAMA 09 Kanıtı — RenderScene, kamera ve diagnostics temeli

> **Tarihsel kapanış:** AŞAMA 09 `DONE` kaydı korunur. Güncel validation cursor'ı `ANDROID_DOGRULAMA_PLANI.md` belirler; V09 yeni doğrulama sonucu ayrı evidence üretir. A10'un sınırlı paralel taslak kuralı tarihsel AŞAMA 09 kapanış kanıtını değiştirmez.

## Durum

`DONE`

AŞAMA 09 çıkış kriterleri gerçek exact .NET `10.0.400` execution üzerinde sağlandı. Bu aşama P0 geometry renderer değildir; parser'dan bağımsız scene/camera/diagnostics temelini ve deterministic semantic snapshot sözleşmesini kapatır.

## Karar ve kapsam

- Implementation kapanışı sırasında kullanılan base `main`: `b0b0620c40ee5d9a0bcb681783c834fe44040afa`.
- Branch: `stage09-render-scene-camera`.
- PR: `#12` — `stage09: establish render scene and camera foundation` — `MERGED`.
- İlk scene/camera implementation head: `5b3f590dca123c3855e8aac7d48f781ba2cdfdb3`.
- Son source/test hardening head: `9a17d333afc0a3df1de856a9a53fae0e74617c29`.
- Yetkili AŞAMA 09 validation head: `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087`.
- Final PR head: `68d08bd3984ef4d1fcca027acb788c4bfcc5e43a`.
- Final merge commit / `main` post-merge head: `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- Validation-head → merge-commit compare: AŞAMA 09 production source/test dosyalarında değişiklik yok; yalnız CI cleanup/workflow, handoff/evidence ve kullanıcı remote-test dokümanları değişti.
- ADR 0002 exact ProCad candidate için `NO-GO` verdiğinden tek production scene yolu **compact özel immutable RenderScene** olarak seçildi.
- ProCad package/source production graph'a eklenmedi; paralel ikinci scene graph oluşturulmadı.
- AŞAMA 10 geometry renderer kapsamına bu turda başlanmadı.

## AŞAMA 09 requirement matrisi

| Gereksinim | Uygulama / test | Durum |
|---|---|---|
| Tek scene implementation | `src/MobilDwg.Rendering/Scene/RenderScene.cs`; compact custom immutable scene | PASS |
| Stable entity ID / bounds / layer-style token / source reference | `Scene/SceneGeometry.cs`; default-value bypass ve duplicate-ID guard'ları | PASS |
| World/document coordinate `double` | `WorldPoint2`, `WorldPoint3`, `WorldBounds2`; camera pipeline `double` | PASS |
| Tek world → view → screen hattı | `Camera/Camera2D.cs` / `CameraTransform`; finite point guards | PASS |
| Core viewport bridge | `Camera2D.ToViewport/FromViewport`; renderer contract ile açık tek adapter | PASS |
| OCS/WCS | `Coordinates/OcsTransform.cs`; normal + oblique round-trip ve scaled normalization | PASS |
| Extents / invalid NaN-Infinity / büyük koordinat | finite-span guards, overflow-safe center, survey precision tests | PASS |
| Scene diagnostics | `Unsupported/Substituted/Dropped/Error`; invalid enum/default entity ID guards | PASS |
| Camera fit/zoom bounds | `Camera2D.Fit`, `ZoomBy`, min/max clamps, invalid default-camera guard | PASS |
| Background/color context | `RenderColorContext`, Dark/Light presets | PASS |
| Deterministic semantic snapshot | insertion-order-independent invariant/round-trip snapshot | PASS |
| ProCad facade sınırı | ADR 0002 exact ProCad candidate'ı reddetti; custom path seçildi | NOT_APPLICABLE |
| T0 restore/build | exact .NET `10.0.400`, Release `/warnaserror` | PASS |
| T1 deterministic tests | precision/OCS/diagnostics/snapshot executable testleri | PASS |
| Mimari regresyon | tam solution restore/build + Core/Rendering/Architecture harness | PASS |

## Precision ve robustness tasarım kuralı

AŞAMA 07'de reddedilen ProCad hattındaki kritik hata, absolute CAD world koordinatının scene sınırında doğrudan `float`a çevrilmesiydi. AŞAMA 09 hattında:

1. world/document geometry `double` tutulur;
2. `WorldToView` camera center'ı `double` olarak çıkarır;
3. view → screen ölçekleme yine `double` yapılır;
4. scene modelinde raw absolute `float` coordinate bulunmaz;
5. finite girdilerin çıkarma/span sırasında `Infinity` üretmesine izin verilmez;
6. çok büyük finite OCS normal vektörleri önce scale edilerek normalize edilir.

Yetkili T1 snapshot'ı survey origin `5,000,000` çevresindeki `0.001` world-unit ayrıntıyı korudu. Snapshot satırı `entity=E-001|...|5000000,-25,5000000.001,100|...` olarak gerçek execution logunda kaydedildi.

## Determinism / immutable boundary

- `RenderScene` entity dizisini stable ID ile ordinal sıralayıp defensive `ReadOnlyCollection` olarak dışarı verir.
- Diagnostics defensive read-only collection kullanır.
- Duplicate stable entity ID assembler seviyesinde reddedilir.
- `RenderEntityId`, layer/style token record-struct'larının `default` ile constructor bypass etmesi scene boundary'de tekrar doğrulanır.
- Source index negatif olamaz; supplied handle boş/whitespace olamaz.
- Snapshot formatı invariant culture ve round-trip double (`R`) formatı kullanır.
- Aynı semantic scene farklı insertion order ile oluşturulduğunda snapshot eşitliği test edilir.
- Eski `STAGE04_RENDER_CONTRACT_TESTS_PASS` marker'ı korunur.

## Yetkili T0/T1 + regresyon kanıtı

Self-hosted Windows runner'ın `android-test` otomasyonu 2026-08-25 tarihinde yeniden çevrimiçi ve çalışır durumda doğrulandı. AŞAMA 09 için yalnız doğrulama amacıyla geçici workflow kullanıldı; kapanıştan sonra workflow branch'ten kaldırıldı.

Yetkili kapanış koşusu:

- Workflow: `Stage 09 Self-Hosted Validation`.
- Run: `32815175055` / `#6`.
- Job: `97701882792`.
- Head: `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087`.
- Runner: self-hosted Windows, labels `[self-hosted, windows, android-test, mobil-dwg]`.
- Sonuç: `SUCCESS`.
- .NET: exact `10.0.400` — `STAGE09_DOTNET_PIN_PASS`.
- Hedefli Release build: `0 Warning`, `0 Error` — `STAGE09_T0_BUILD_PASS`.
- T1 marker'ları:
  - `STAGE04_RENDER_CONTRACT_TESTS_PASS`
  - `STAGE09_RENDER_SCENE_TESTS_PASS`
  - `render-scene/v1`
  - `STAGE09_T1_SCENE_PASS`
- Tam solution Release restore/build: `0 Warning`, `0 Error`.
- Mimari/regresyon marker'ları:
  - `STAGE04_CORE_CONTRACT_TESTS_PASS`
  - `STAGE04_RENDER_CONTRACT_TESTS_PASS`
  - `STAGE04_ARCHITECTURE_TESTS_PASS`
  - `STAGE05_DEPENDENCY_BOUNDARY_PASS`
  - `STAGE04_T0_PASS`
  - `STAGE09_STAGE04_REGRESSION_PASS`
- Artifact: `9551137293`, `stage09-self-hosted-evidence`, 1,578 bytes.
- Artifact digest: `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`.
- Artifact files: `stage09-render-scene.log`, `stage09-stage04-regression.log`.

İlk başarılı hedefli self-hosted koşu `32815005461`, job `97701406863`, artifact `9551083791`, digest `sha256:33da24c645ba225856ca05778e93f940d6a978defd9a45a7c2788fd6720cce3a` idi. Yetkili kapanış kanıtı daha geniş Stage 04 regresyonunu da içeren run `32815175055` / #6'dır.

## Önceki hosted runner blocker geçmişi

AŞAMA 09 kodundan bağımsız olarak standard `ubuntu-latest`, `macos-26` ve `ubuntu-slim` hosted job'ları bir süre checkout başlamadan `steps=[]`, `runner_id=0` ile kesildi. Bunlar compile/test failure değildir. Self-hosted runner yeniden çevrimiçi olduğunda aynı AŞAMA 09 kodu gerçek checkout/restore/build/test üzerinde PASS verdi. Böylece önceki kayıtların implementation failure olmadığı ayrıştırılmış oldu.

Önemli eski kayıtlar:

- Ubuntu run `32791364379` / #30, rerun job `97690824454` — pre-step allocation failure.
- macOS run `32786600644` / #14 attempts 1/2/3 — pre-step allocation failure.
- `ubuntu-slim` run `32811281420` / #32, job `97690952636` — pre-step allocation failure.

## CI cleanup

- Geçici `.github/workflows/stage09-self-hosted-validation.yml` yalnız AŞAMA 09 kapanış kanıtını üretmek için kullanıldı ve PASS sonrasında branch'ten kaldırıldı.
- Kalıcı `.github/workflows/stage09-render-scene.yml` platform-independent uzun vadeli CI için `ubuntu-latest` kullanır; post-merge closure ile hem `main` push hem de Stage 09 feature-branch/PR değişikliklerini dinler.
- Current `main` üzerindeki Android emulator automation dosyaları korunmuştur; AŞAMA 09 onları değiştirmez.

## Çıkış

AŞAMA 09 çıkış kriteri **sağlandı**:

- sentetik scene headless üretildi;
- aynı semantic girdi deterministic `render-scene/v1` snapshot üretti;
- large-survey-origin `0.001` detay `double` hattında korundu;
- exact .NET `10.0.400` T0/T1 geçti;
- full Stage 04 architecture regression geçti;
- production graph'a ProCad eklenmedi;
- PR #12 `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a` merge commit'i ile `main`e alındı;
- yetkili validation head ile final merge commit arasında AŞAMA 09 source/test farkı olmadığı doğrulandı.

AŞAMA 01, AŞAMA 06 ve AŞAMA 08'in fiziksel/local dış kapıları değişmeden açık kalır. Bir turda en fazla bir aşama kuralı gereği AŞAMA 10 bu kapanış turunda başlatılmaz.
