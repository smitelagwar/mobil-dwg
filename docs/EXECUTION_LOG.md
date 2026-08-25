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
- tested PR head revision `de39866f8bd71c20fa51b355748ed79884fbb4e6`
- main merge commit `9013d52702d1cb44e378aeacda46ee51e53caa65`
- authoritative run/job `32838507832` / `97772635524` — SUCCESS
- artifact `9561607163`, 29,656,507 byte; digest `sha256:16359b01f4d3c72847b90227b03b321036495b45f2d65cd34d2c772f14528109`
- host mini corpus `9` fixture + `2` derived negative PASS
- `STAGE05_ACADSHARP_PACKAGE_PASS central=[3.7.1] resolved=3.7.1`
- generated AC1015 DWG 8021 byte + DwgReader read-back PASS; run-specific SHA `0cb734fae8a87ca63562ff7b2e056f835c09f08150cc4345e0a1b5a847cf0099`; binary golden değil
- `V05_PRODUCTION_WRITER_ABSENT_PASS`
- validation APK 30,876,566 byte; SHA-256 `1c0dc516b9e1db6270b4f9d8818c3dff09efb98ebc63b085d914358dc11a12ac`
- package install/cold-launch/UI parse/stability PASS; PID `3835`
- marker `ANDROID_VALIDATION_V05_PASS`
- claim `REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY`
- same-head V04 regression `32838507889 / 97772635962` — SUCCESS; artifact `9561764023`, digest `sha256:b5f8581c4c4290adb83fb243968bb93b7a3991ca14c6658e418468acf76288e8`
- evidence `docs/evidence/android-validation/V05.md`

## Android validation V06 — VALIDATED

- Real `MobilDwg.App` MAUI FilePicker → Android DocumentsUI/SAF → `FileResult.OpenReadAsync()` → app-private safe-copy → production parser zinciri API36 emulator üzerinde doğrulandı.
- Historical Stage06 host probe Android-only app'i `net10.0` host'tan referansladığı için `NU1201` veriyordu; app multi-target yapılmadan production safe-open BCL kaynakları probe'a linklendi.
- DocumentsUI ilk gerçek koşuda `Recent / No items` ekranında kalırken roots drawer açılmadan `Downloads` aranıyordu; failure artifact UI XML'indeki `Show roots` kanıtıyla navigation `Show roots` → `Downloads` → file olarak düzeltildi.
- diagnostic failed run/job `32846335305 / 97796783640`: host probe `NU1201`; artifact `9562991064`, digest `sha256:5951a3ba321cc0f0954cdd688614b0c177f141089c3328f39e272542ea6b66b5`.
- diagnostic failed run/job `32847919780 / 97801845809`: DocumentsUI target file görünürlüğü; artifact `9563560512`, digest `sha256:2d7a310a5e6317923134ad00b17b10578431c5e6c184383574bc5eb7499cd911`.
- tested PR head `ae8682875524157285946724bd70d6ff010f3917`
- tested PR synthetic merge `26b3cdd6ca50d34b98a4806d92f50d4828077d41`
- PR #19 main merge `e17e2472f38557552698b8cf9526d6cbf8b25580`
- authoritative run/job `32849725110` / `97807551403` — SUCCESS
- artifact `9564837027`, 29,743,234 byte; digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- `STAGE06_ACTUAL_DWG_DXF_PASS`, `STAGE06_SAFE_COPY_GUARDS_PASS`, `STAGE06_LAST_REQUEST_WINS_PASS`, `STAGE06_CANCEL_SEMANTICS_PASS`, `STAGE06_T2_HEADLESS_PASS`
- validation APK 30,917,242 byte; SHA-256 `4bcd819def4483fbc076865dd70b10026eb2eae7515c07561a9cdfe02ff9c9a5`
- package/install/cold-launch PASS; real DWG SAF open PASS; second selection DXF/latest-state PASS
- rotate/background-foreground/picker-cancel/close-cleanup/reopen PASS; PID `3876`
- original external CAD bytes immutable PASS
- broad external-storage permission absent; immediate-copy path persistable URI grant gerektirmedi/almadı
- package/PID crash + post-launch ANR/liveness PASS
- marker `ANDROID_VALIDATION_V06_PASS`
- claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`
- same-head V04 regression `32849725215 / 97807552081` — SUCCESS; artifact `9565016182`, digest `sha256:6922f2168334e8312debc2c90cb7905d9db5da58eb8cb10da3f8aadf6e53bb3f`
- same-head V05 regression `32849725272 / 97807552194` — SUCCESS; artifact `9565243977`, digest `sha256:36ada98dd79f7f70e2ef7e63d6d2cb6cec191141421c07bcf41673dded23b492`
- evidence `docs/evidence/android-validation/V06.md`
- physical Android/provider-specific fidelity `DEFERRED_RELEASE_DEVICE_GATE` olarak açık kalır.

## Sonraki iş

`NEXT_VALIDATION_STAGE = V07 — ProCad NO-GO + production graph isolation + precision regression (NOT_STARTED)`.

Bir sonraki validation `devam` yalnız V07'yi açar; aynı turda V08'e geçmez. Implementation cursor `AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ` olarak ayrı korunur. Bilgisayar/runner kapalı A10 sohbeti yalnız `BASLA_A10.md` ile ayrı draft branch'te yürür.
