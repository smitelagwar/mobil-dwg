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
- Implementation next: AŞAMA 10 — `NOT_STARTED`.

## Android validation V01 — VALIDATED

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`
- Stage01Smoke toolchain/emulator/PID/PNG/crash-ANR PASS
- claim `INFRASTRUCTURE_SMOKE_ONLY`
- PR #14 merge `ae4008f87eabb835d41488367e1d92cd76f041b1`
- evidence `docs/evidence/android-validation/V01.md`

## Android validation V02 — VALIDATED

- strict exact ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, fallback IxMilia.Dxf `[0.8.4]`
- locked restore/license/hash/vulnerability/Android-native/source boundary PASS
- run/job `32824397251` / `97729154385`
- tested PR merge ref `549770192c181b30db8968cec5c6ac3c2407e133`
- artifact `9554326162`, digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`
- PR #15 merge `1c5254ef55c9e704a33d1f103a9027911e82bf89`
- evidence `docs/evidence/android-validation/V02.md`

## Android validation V03 — VALIDATED

- Android fixture/provenance/golden/smoke-set contract hardened.
- committed CAD hash Git blob bytes üzerinden doğrulanıyor; generated AC1015 DWG writer/read-back smoke evidence, binary golden değil.
- branch head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- tested PR merge revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- marker `ANDROID_VALIDATION_V03_PASS`
- evidence `docs/evidence/android-validation/V03.md`

## Android validation V04 — VALIDATED

- `MobilDwg.App` platform-neutral `net10.0` projeden gerçek Android-only MAUI executable'a dönüştürüldü; beşinci production proje açılmadı.
- target `net10.0-android36.0`; package `com.smitelagwar.mobildwg`; real launcher/activity/application/UI shell.
- `Microsoft.Maui.Controls` exact `[10.0.100]`, MIT; V02 boundary buna göre güncellendi.
- ilk iki V04 run gate PowerShell bug'ları nedeniyle FAIL oldu; product build/crash failure değildi ve PASS sayılmadı.
- final branch head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested PR synthetic merge revision `6201be929a636b963235f7da8ee72b0bbf9decf2`
- final run/job `32832142832` / `97752997848` — SUCCESS
- same-head V02 regression `32832142882` / `97752998222` — SUCCESS
- real app Release build `0 Warning(s)`, `0 Error(s)`
- signed APK 30,827,130 byte; SHA-256 `60d8d59b3fd452d786519a364875b155d3961c3e4aa210f986c004098789ba42`
- package install PASS; cold launch `Status: ok`; PID `3783`
- UI hierarchy + byte-safe PNG + crash/ANR + liveness PASS
- artifact `9557331919`, digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- marker `ANDROID_VALIDATION_V04_PASS`
- claim `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`
- evidence `docs/evidence/android-validation/V04.md`

## Sonraki iş

`NEXT_VALIDATION_STAGE = V05 — ACadSharp parser entegrasyonu (NOT_STARTED)`.

Bir sonraki `devam` yalnız V05'i açar; aynı turda V06'ya geçmez. Implementation cursor `AŞAMA 10 — NOT_STARTED` kalır.
