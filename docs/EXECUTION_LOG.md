# Execution Log

Bu dosya teknik yürütme geçmişinin kısa indeksidir. Ayrıntılı kanıt `docs/evidence/`, kararlar `docs/ADR/`, aktif checkpoint `DEVAM.md` / `gecmis.md` içindedir. Kanıtsız başarı yazılmaz.

## Implementation özeti

- 2026-08-24 — AŞAMA 00 — `DONE`: execution/evidence/ADR/handoff zemini.
- 2026-08-24 — AŞAMA 01 — `BLOCKED / DEFERRED_EXTERNAL_GATE`: pinned Android toolchain; fiziksel Android dış kapısı açık.
- 2026-08-24 — AŞAMA 02 — `DONE`: dependency/lisans/lockfile; PR #4 merge `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.
- 2026-08-24 — AŞAMA 03 — `DONE`: mini corpus/golden; historical run `32752374980`; PR #5 merge `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.
- 2026-08-24 — AŞAMA 04 — `DONE`: Core/Cad/Rendering/App architecture; run `32755230695`; PR #6 merge `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.
- 2026-08-24 — AŞAMA 05 — `DONE`: ACadSharp 3.7.1 parser baseline GO; run `32760139261`; artifact `9532379884`; PR #7 merge `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.
- 2026-08-24 — AŞAMA 06 — host implementation done / physical FilePicker-SAF deferred; run `32762879583`; artifact `9533538573`.
- 2026-08-24 — AŞAMA 07 — `DONE / NO-GO`: exact ProCad source reuse rejected; run `32766501837`; artifact `9534797361`; PR #9 merge `28cc06c2de5d21f733e29ae69a38395979b6d759`.
- 2026-08-25 — AŞAMA 08 — historical iOS characterization; run `32781026946`; artifact `9540018558`; PR #11 merge `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.
- 2026-08-25 — AŞAMA 09 — `DONE`: RenderScene/kamera/diagnostics; run `32815175055`; artifact `9551137293`; merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- Implementation cursor: AŞAMA 10 — `MAIN'E HENÜZ MERGE EDİLMEDİ`; paralel draft kuralları `docs/A10_WORKSTREAM.md`.

## Android validation V01 — VALIDATED

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`
- claim `INFRASTRUCTURE_SMOKE_ONLY`
- evidence `docs/evidence/android-validation/V01.md`

## Android validation V02 — VALIDATED

- strict exact ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, fallback IxMilia.Dxf `[0.8.4]`
- locked restore/license/hash/vulnerability/Android-native/source boundary PASS
- authoritative run/job `32824397251` / `97729154385`
- artifact `9554326162`, digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`
- evidence `docs/evidence/android-validation/V02.md`

## Android validation V03 — VALIDATED

- fixture/provenance/golden/smoke-set contract hardened; committed hash Git blob bytes üzerinden.
- generated AC1015 DWG writer/read-back smoke evidence, binary golden değil.
- tested head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- tested merge `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- marker `ANDROID_VALIDATION_V03_PASS`
- evidence `docs/evidence/android-validation/V03.md`

## Android validation V04 — VALIDATED

- real Android-only MAUI `MobilDwg.App`, package `com.smitelagwar.mobildwg`, API36 build/install/cold-launch/UI/PID/crash-ANR/liveness PASS.
- tested head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested merge `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832` / `97752997848`
- same-head V02 regression `32832142882` / `97752998222`
- artifact `9557331919`, digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`
- evidence `docs/evidence/android-validation/V04.md`

## 2026-08-25 — İki çalışma hattı / sınırlı A10 taslak kararı

- Validation cursor V04→V09 sırasını korur ve genel `BASLA.md` sohbetinde yürür.
- Yalnız AŞAMA 10, ayrı `stage10-p0-geometry-draft` branch'inde `BASLA_A10.md` protokolüyle önden hazırlanabilir.
- V09 sonrası güncel validated `main` ile integration + Android gate olmadan A10 main merge/DONE yoktur.
- A11 yalnız A10 `DONE ON MAIN` ve boş emulator kuyruğu sonrasında açılır.
- Kalıcı çalışma kaydı: `docs/A10_WORKSTREAM.md`.

## Android validation V05 — VALIDATED

- Production `AcadSharpDocumentReader` V03 redistributable DXF/DWG smoke setiyle gerçek Android `MobilDwg.App` process'i içinde çalıştırıldı.
- İlk portability false-negative: Git Bash/MSYS `/warnaserror` → path dönüşümü; `-warnaserror` ile düzeltildi.
- İkinci portability false-negative: localized `dotnet list package` çıktısını parse eden grep; final gate central props + lockfile + `project.assets.json` ile locale-independent yapıldı.
- final technical head `d1552960d910b1fc6baea00ac14f6971344bd66e`
- main used for authoritative synthetic merge `b5b6a74ebcc9ea16eff4a423c3ff2e7cbb3e748c`
- exact tested synthetic merge `3aa365dd92222ec445a589003fc796ee6290f505`
- authoritative run/job `32836712300` / `97767085940` — SUCCESS
- artifact `9559245377`, 29,657,586 byte; digest `sha256:2453ac4df3b888c6235f240208b4674b834edc550dd1208ce37e34a6506d2b65`
- host mini corpus `9` fixture + `2` derived negative PASS
- `STAGE05_ACADSHARP_PACKAGE_PASS central=[3.7.1] resolved=3.7.1`
- generated AC1015 DWG 8021 byte + DwgReader read-back PASS; run-specific SHA `44394883546bc115104be2dad50ba158abc0978d57439759d6d4273b88ac2122`; binary golden değil
- `V05_PRODUCTION_WRITER_ABSENT_PASS`
- validation APK 30,876,566 byte; SHA-256 `a270689a6bda814b9145601498b075b8a3638dd03d6ed6d9026e293c5e0738b5`
- package install/cold-launch/UI parse/stability PASS; PID `3803`
- marker `ANDROID_VALIDATION_V05_PASS`
- claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`
- same-head V04 regression `32836712245 / 97767085274` — SUCCESS
- same-head V02 regression `32836712385 / 97767086999` — SUCCESS; artifact `9559261198`, digest `sha256:e3d9dafeb576b20b63b06b96ba5b1729c15bece13f7d8426d0967d615841500a`
- evidence `docs/evidence/android-validation/V05.md`

## Sonraki iş

`NEXT_VALIDATION_STAGE = V06 — Android FilePicker/SAF + safe-open/document-service bridge (NOT_STARTED)`.

Bir sonraki validation `devam` yalnız V06'yı açar; aynı turda V07'ye geçmez. Implementation cursor `AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ` olarak ayrı korunur. Bilgisayar/runner kapalı A10 sohbeti yalnız `BASLA_A10.md` ile ayrı draft branch'te yürür.
