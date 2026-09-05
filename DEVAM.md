# mobil-dwg — Yeni sohbet için tek dosyalık handoff

Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` gerçek `main` HEAD'ini, açık branch/PR'ları ve Actions durumunu doğrula.
2. `BASLA.md`, canonical plan, `ANDROID_DOGRULAMA_PLANI.md`, `gecmis.md`, `docs/A10_WORKSTREAM.md` ve gerekiyorsa son evidence dosyasını oku.
3. GitHub connector üzerinden çalışılıyor ve yerel terminal/ADB doğrudan yoksa bağlam `CHATGPT_REMOTE_GITHUB`; `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` zorunlu bağlam belgesidir.
4. Android V01–V09 revalidation programı kapanmıştır. Gerçek regression yeniden açılmasını gerektirmiyorsa sonraki normal implementation cursor AŞAMA 10'dur.
5. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; aynı turda sonraki aşama başlatılmaz.
6. Emulator fiziksel cihaz değildir; queued/zero-step job PASS değildir; evidence olmadan `DONE/READY_TO_MERGE` yoktur.
7. ProCad production graph'a alınmaz; original CAD immutable; production writer/save yoktur.

## Güncel checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 21 — DONE
ANDROID_VALIDATION_PROGRAM: V01–V09 — CLOSED
ANDROID_VALIDATION_STATUS: VALIDATED_WITH_CLAIM_LIMITS
LAST_IMPLEMENTATION: AŞAMA 21 — DONE
LAST_IMPLEMENTATION_EVIDENCE: docs/evidence/STAGE_21.md
A21_CLAIM: A21_FULL_CORPUS_REGRESSION_API36_ONLY_NOT_PHYSICAL_DEVICE_FIDELITY
IMPLEMENTATION_CURSOR: AŞAMA 22 — NOT_STARTED
A10_WORKSTREAM: docs/A10_WORKSTREAM.md (DONE)
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
A22_GATE: OPEN
NEXT_ACTION: Sonraki normal BASLA/devam turunda AŞAMA 22'yi (Android accessibility, dark/light theme, localization) başlat.
```

## Validation özeti

- V01 — `VALIDATED`: `INFRASTRUCTURE_SMOKE_ONLY`.
- V02 — `VALIDATED`: exact dependency/lockfile/license/hash/vulnerability/Android-native boundary.
- V03 — `VALIDATED`: fixture/provenance/golden/Android smoke-set contract.
- V04 — `VALIDATED`: gerçek Android app shell runtime; viewer fidelity değil.
- V05 — `VALIDATED`: production ACadSharp parser gerçek Android process smoke; render fidelity değil.
- V06 — `VALIDATED`: real FilePicker/SAF safe-open API36 emulator; physical provider fidelity değil.
- V07 — `VALIDATED`: ProCad NO-GO production graph isolation + precision regression.
- V08 — `VALIDATED`: Android production/CI graph iOS isolation; historical iOS future-only.
- V09 — `VALIDATED`: RenderScene/camera/OCS/diagnostics foundation + Android composition revalidation; geometry renderer fidelity değil.

### V09 authoritative

- PR `#22`
- tested head `892315966f895729e866947a838df93350fdfd97`
- synthetic merge `6fea8ba9d1de6811afd0dcace7a2c8b5b6ec573a`; file diff yok
- main merge `143ce1a79448f53af81faee9c6e650321047dd37`
- run/job `32864617493 / 97856686115` — SUCCESS
- artifact `9569686660`, 11,544 byte; digest `sha256:97e55129367ea5b778edf99a6d84939e95f74902db655144d32dbf24ba8aa375`
- exact .NET `10.0.400`
- same-job V02 prerequisite PASS
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`
- `render-scene/v1`
- semantic snapshot survey line `5000000.001` PASS
- `V09_SURVEY_ORIGIN_DOUBLE_PRECISION_PASS delta=0.001`
- Core/architecture/dependency composition regressions PASS
- full solution Release `0 Warning / 0 Error`
- real Android Release APK 30,913,146 byte; SHA-256 `a0080fb4826cbd6f7fee1d84cac3465c8ebda766bfba245167d73233ab1a40f5`
- marker `ANDROID_VALIDATION_V09_PASS`

İlk V09 run/job `32864458158 / 97856153440`, Windows PowerShell 5.1 `.Contains(string, StringComparison)` portability false-negative'i nedeniyle ürün testlerinden önce durdu; production/test failure değildir. Diagnostic artifact `9569504762`, digest `sha256:7eda4ec7db3d423cdbd476bc4769eebac54ef0527c18656c0fc2bbd0b2eb90f8`.

## A10 giriş kapısı

A10 henüz başlamadı. Bir sonraki turda live GitHub tekrar okunur; branch yoksa güncel `main`den normal feature branch açılır. A10 doğrulaması en az:

- platform-neutral primitive/tessellator correctness,
- world/document `double` precision,
- etkilenen V02/V03/architecture/V09 regresyonları,
- current real `MobilDwg.App` API36 render gate,
- PID/PNG/crash/ANR yanında expected-content kanıtı

içermeden `READY_TO_MERGE` değildir. A10 `DONE ON MAIN` olmadan A11 başlatılmaz.
