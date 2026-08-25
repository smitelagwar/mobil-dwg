# Execution Log

Bu dosya teknik yürütme geçmişinin kısa indeksidir. Anlık checkpoint için `gecmis.md`/`DEVAM.md`; ayrıntılı kanıt için `docs/evidence/`, kararlar için `docs/ADR/`, dependency kanıtı için `compliance/` esas alınır. Kanıtsız başarı yazılmaz.

## 2026-08-24 — AŞAMA 00 — DONE

- Repo ve kullanıcı belgeleri korunarak execution/evidence/ADR/handoff zemini kuruldu.

## 2026-08-24 — AŞAMA 01 — BLOCKED / DEFERRED_EXTERNAL_GATE

- .NET `10.0.400`, OpenJDK `21.0.12`, Android API 24/36, Build-Tools 36.0.0, Platform-Tools 37.0.1 ve `maui-android` baseline CI'da doğrulandı.
- Fiziksel Android install/launch dış kapısı açık bırakıldı.

## 2026-08-24 — AŞAMA 02 — DONE

- ACadSharp 3.7.1 / SkiaSharp 4.151.1 dependency evidence; CPM, lockfile, nupkg hash/license ve vulnerability audit kuruldu.
- PR #4 merge `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.
- Not: V02 Android revalidation daha sonra plain-version/open-lower-bound pin açığını bulup strict exact NuGet range'e çevirdi; güncel gerçeklik için V02 kaydına bak.

## 2026-08-24 — AŞAMA 03 — DONE

- Redistributable mini corpus/golden sözleşmesi ve negatif fixture hattı kuruldu.
- Final corpus run `32752374980` SUCCESS; PR #5 merge `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.

## 2026-08-24 — AŞAMA 04 — DONE

- Minimal Core/Cad/Rendering/App + test mimarisi ve otomatik dependency boundaries kuruldu.
- Final run `32755230695` SUCCESS; PR #6 merge `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.

## 2026-08-24 — AŞAMA 05 — DONE

- ACadSharp `3.7.1` read-only adapter mini corpus üzerinde doğrulandı.
- Final run `32760139261`, artifact `9532379884`; ADR 0001 parser baseline `GO`.
- PR #7 merge `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.

## 2026-08-24 — AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE

- Safe-open: provider-path bağımsız stream, quota/disk reserve, atomic cache, cleanup, generation/last-request-wins ve cancellation-result-discard uygulandı.
- Final host CI run `32762879583`, artifact `9533538573` SUCCESS.
- Fiziksel Android FilePicker/SAF/lifecycle/cache kapısı açık.

## 2026-08-24 — AŞAMA 07 — DONE / NO-GO

- Exact ProCad source production graph dışında değerlendirildi.
- `5,000,000 + 0.001` detail direct `double→float` scene boundary'sinde kayboldu; ADR 0002 exact unpatched ProCad reuse `NO-GO`.
- Final run `32766501837`, artifact `9534797361`; PR #9 merge `28cc06c2de5d21f733e29ae69a38395979b6d759`.

## 2026-08-25 — AŞAMA 08 — DONE / CHARACTERIZATION

- ACadSharp 3.7.1 + SkiaSharp 4.151.1 iOS hattı tarihsel olarak karakterize edildi; iOS PASS iddiası yok.
- Run `32781026946`, artifact `9540018558`; PR #11 merge `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.
- Android-only kararından sonra iOS future inactive track olarak korunur.

## 2026-08-25 — AŞAMA 09 — DONE

- Custom renderer efor/bakım riski kullanıcı tarafından kabul edildi; production scene yolu compact immutable `RenderScene`.
- Stable IDs/bounds/metadata, double precision coordinate pipeline, Camera2D bridge, survey-origin precision, OCS/WCS, overflow/invalid geometry guards, diagnostics ve deterministic `render-scene/v1` snapshot uygulandı.
- Yetkili self-hosted Windows validation: head `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087`, run `32815175055`, job `97701882792`, SUCCESS; full Release build 0 warning/0 error ve gerekli Core/Rendering/Architecture/Stage05 markers PASS.
- Artifact `9551137293`, digest `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`.
- PR #12 merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.

## 2026-08-25 — Android-only kapsam ve V01–V09 revalidation programı

- Aktif v1 hedefi Android-only yapıldı; iOS future option olarak korundu.
- `ANDROID_DOGRULAMA_PLANI.md` oluşturuldu. Implementation cursor AŞAMA 10'da tutulurken ayrı Android validation cursor V01'den başladı.
- Emulator, fiziksel cihaz ve Stage01Smoke/real-app kanıt sınıfları ayrıştırıldı.

## 2026-08-25 — Android validation V01 — VALIDATED

- V01 başlangıç audit'i gate'te gerçek evidence açıkları buldu: executable harness'lar `dotnet test` ile gerçekte çalışmıyor; screenshot byte-safe değil; PID zorunlu değil; crash/ANR evidence yetersiz.
- Gate sertleştirildi: executable `dotnet run` markers, exact toolchain/AVD, byte-safe PNG full signature, live numeric PID, package/PID crash ve post-launch ANR/lifecycle evidence.
- Yetkili exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`; self-hosted Windows Release run `32821991333`, job `97721878468`, SUCCESS.
- Stage01Smoke Android 16/API36 emulator üzerinde install/cold-launch `Status: ok`, PID `3374`; screenshot ve stability evidence PASS.
- Artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`.
- Claim limit `INFRASTRUCTURE_SMOKE_ONLY`; gerçek `MobilDwg.App`/DWG-DXF fidelity PASS değildir.
- PR #14 merge commit `ae4008f87eabb835d41488367e1d92cd76f041b1`.
- Ayrıntı `docs/evidence/android-validation/V01.md`.

## 2026-08-25 — Android validation V02 — VALIDATED

Audit bulgusu ve düzeltme:

- Tarihsel AŞAMA 02 “exact pin” iddiasına karşın CPM plain sürümleri lockfile'da open lower-bound request üretiyordu: `[3.7.1, )`, `[4.151.1, )`.
- CPM strict exact NuGet range'e çevrildi: ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback-only IxMilia.Dxf `[0.8.4]`.
- Lockfile direct requested ranges exact hale geldi.

Hardened V02 gate:

- exact target/resolved/requested graph;
- locked restore + lockfile reproducibility;
- nupkg SHA-256 + license allowlist + manifest reproducibility;
- NuGet vulnerability audit;
- production `src/` PackageReference/TFM/ProjectReference boundary;
- `src/` altında vendored native binary yasağı;
- SkiaSharp Android native ABI inventory;
- ProCad/IxMilia/iOS-only leakage reddi.

Kalıcı workflow:

- `.github/workflows/stage02-dependency-audit.yml` GitHub-hosted `ubuntu-latest` yerine kanıtlanmış self-hosted Windows runner'a taşındı; `permissions: contents: read`, checkout credentials persist edilmez.
- Gate: `scripts/v02-dependency-gate.ps1`.
- Yetkili run `32824397251`, job `97729154385`, `SUCCESS`.
- Branch head `50694547e7be43e5ec414cc91b57cbd32faa3c54`; checkout/test PR merge ref `549770192c181b30db8968cec5c6ac3c2407e133`.
- Marker'lar: `V02_TOOLCHAIN_PASS`, `V02_LOCKED_RESTORE_PASS`, `V02_EXACT_VERSION_POLICY_PASS`, `V02_ANDROID_BOUNDARY_PASS`, `STAGE02_PACKAGE_AUDIT_PASS`, `V02_PACKAGE_AUDIT_PASS`, `V02_VULNERABILITY_PASS`, `ANDROID_VALIDATION_V02_PASS`.
- Resolved graph yalnız ACadSharp 3.7.1, SkiaSharp 4.151.1 ve transitive SkiaSharp.NativeAssets.Android 4.151.1 içerir.
- NuGet mevcut kaynağa göre vulnerable package bildirmedi.
- Artifact `9554326162`, 6 dosya, 3039 byte; digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`.
- Artifact indirildi; summary, resolved graph ve vulnerability raporu incelendi.
- Emulator V02 için çalıştırılmadı: gerçek installable `MobilDwg.App` henüz yok ve bu V04 kapsamı; Stage01Smoke bu aşamaya ek kanıt sağlamaz.
- Claim limit: dependency/lockfile/license/hash/vulnerability/source-boundary/Android-native package boundary; viewer/APK PASS değildir.
- Ayrıntı `docs/evidence/android-validation/V02.md`.

## Sonraki iş

`NEXT_VALIDATION_STAGE = V03 — Fixture, golden sözleşmesi ve Android test matrisi (NOT_STARTED)`.

Kullanıcı bir sonraki `devam` dediğinde yalnız V03 açılır. V03 aynı turda V04'e geçmez. Implementation cursor AŞAMA 10 — NOT_STARTED olarak korunur.
