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
- V02 daha sonra plain-version/open-lower-bound pin açığını strict exact NuGet range ile düzeltti.

## 2026-08-24 — AŞAMA 03 — DONE

- Mini corpus/golden sözleşmesi ve negatif fixture hattı kuruldu.
- Final historical corpus run `32752374980` SUCCESS; PR #5 merge `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.
- V03 daha sonra cross-platform byte/provenance ve Android redistributable smoke-set sözleşmesini sertleştirdi.

## 2026-08-24 — AŞAMA 04 — DONE

- Minimal Core/Cad/Rendering/App + test mimarisi ve otomatik dependency boundaries kuruldu.
- Final run `32755230695` SUCCESS; PR #6 merge `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.

## 2026-08-24 — AŞAMA 05 — DONE

- ACadSharp `3.7.1` read-only adapter mini corpus üzerinde doğrulandı.
- Final run `32760139261`, artifact `9532379884`; ADR 0001 parser baseline `GO`.
- PR #7 merge `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.

## 2026-08-24 — AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE

- Provider-path bağımsız safe-open, quota/disk reserve, atomic cache, cleanup, generation/last-request-wins ve cancellation-result-discard uygulandı.
- Final host CI run `32762879583`, artifact `9533538573` SUCCESS.
- Fiziksel Android FilePicker/SAF/lifecycle/cache kapısı açık.

## 2026-08-24 — AŞAMA 07 — DONE / NO-GO

- Exact ProCad source production graph dışında değerlendirildi.
- `5,000,000 + 0.001` detail direct `double→float` scene boundary'sinde kayboldu; ADR 0002 exact unpatched ProCad reuse `NO-GO`.
- Final run `32766501837`, artifact `9534797361`; PR #9 merge `28cc06c2de5d21f733e29ae69a38395979b6d759`.

## 2026-08-25 — AŞAMA 08 — DONE / CHARACTERIZATION

- ACadSharp 3.7.1 + SkiaSharp 4.151.1 iOS hattı tarihsel karakterize edildi; iOS PASS iddiası yok.
- Run `32781026946`, artifact `9540018558`; PR #11 merge `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.
- Android-only kararından sonra iOS future inactive track.

## 2026-08-25 — AŞAMA 09 — DONE

- Compact immutable `RenderScene`; stable IDs/bounds/metadata, double precision coordinate pipeline, Camera2D bridge, survey-origin precision, OCS/WCS, overflow/invalid geometry guards, diagnostics ve deterministic `render-scene/v1` snapshot uygulandı.
- Yetkili self-hosted validation head `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087`, run `32815175055`, job `97701882792`, SUCCESS; Release build 0 warning/0 error ve gerekli Core/Rendering/Architecture/Stage05 marker'ları PASS.
- Artifact `9551137293`, digest `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`.
- PR #12 merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.

## 2026-08-25 — Android-only V01–V09 revalidation başlangıcı

- Aktif v1 hedefi Android-only oldu; iOS future option olarak korundu.
- Ayrı Android validation cursor V01'den başladı; implementation cursor AŞAMA 10'da tutuldu.
- Emulator, fiziksel cihaz ve Stage01Smoke/real-app evidence sınıfları ayrıştırıldı.

## 2026-08-25 — Android validation V01 — VALIDATED

- Gate'te executable harness, screenshot byte güvenliği, PID ve crash/ANR evidence açıkları bulundu ve düzeltildi.
- Exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`; self-hosted Windows Release run/job `32821991333` / `97721878468` SUCCESS.
- Stage01Smoke Android 16/API36 emulator install/cold-launch, PID, byte-safe PNG, crash ve ANR/lifecycle evidence geçti.
- Artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`.
- Claim limit `INFRASTRUCTURE_SMOKE_ONLY`.
- PR #14 merge `ae4008f87eabb835d41488367e1d92cd76f041b1`.
- Ayrıntı `docs/evidence/android-validation/V01.md`.

## 2026-08-25 — Android validation V02 — VALIDATED

- Historical “exact pin” CPM sürümlerinin open-lower-bound request ürettiği bulundu.
- Strict exact ranges: ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`.
- Locked restore, exact resolved/requested graph, nupkg SHA/license, vulnerability, production `src/` boundary ve Android native inventory gate'i kuruldu.
- Authoritative run/job `32824397251` / `97729154385`, tested PR merge ref `549770192c181b30db8968cec5c6ac3c2407e133`, SUCCESS.
- Artifact `9554326162`, digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`.
- Resolved graph yalnız ACadSharp 3.7.1, SkiaSharp 4.151.1, SkiaSharp.NativeAssets.Android 4.151.1; vulnerable package yok; ProCad/iOS leakage yok.
- Claim limit dependency/lockfile/license/hash/vulnerability/source/native boundary.
- PR #15 merge `1c5254ef55c9e704a33d1f103a9027911e82bf89`.
- Ayrıntı `docs/evidence/android-validation/V02.md`.

## 2026-08-25 — Android validation V03 — VALIDATED

Audit bulguları:

- `docs/DEVICE_MATRIX.md` E-API36 hâlâ stale `V01_FIX_REQUIRED` durumundaydı.
- Tarihsel remote-pinned DWG corpus'u `remote-reference-only`; Android validation için hak durumu açık redistributable DWG smoke girdisi yoktu.
- Self-hosted Windows persistent worktree committed DXF'i LF→CRLF materialize ederek working-tree boyutunu 769'dan 985'e çıkarabiliyordu; manifest Git blob hash'i doğruydu.

Düzeltmeler:

- E-API36 `V01_VALIDATED_INFRASTRUCTURE_SMOKE` gerçekliğiyle hizalandı.
- `.gitattributes`: `*.dwg binary`, `*.dxf -text`.
- Authoritative committed-fixture hash'i platform working tree yerine `HEAD:<path>` Git blob bytes üzerinden doğrulanıyor.
- Schema/manifest `generated_fixtures` ve `android_smoke_set` sözleşmesi aldı.
- Redistributable smoke set: committed 0BSD DXF + exact ACadSharp 3.7.1 generator ile validation-time üretilen AC1015 DWG + committed missing-font/missing-XREF negatif DXF'ler.
- Generated DWG için AC1015 magic ve DwgReader read-back zorunlu. Output hash'i runlar arasında değiştiği için binary golden olarak commit edilmedi; run-specific hash evidence tutuluyor.
- `docs/GOLDEN_CONTRACT.md` generated writer/read-back ile independent engineering-fidelity golden arasındaki sınırı açıkça tanımlıyor.

Authoritative final technical validation:

- branch head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- tested PR merge revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- workflow `Stage 03 Corpus Audit`
- run/job `32827625875` / `97739039060` — SUCCESS
- generated DWG final run: AC1015, 8021 byte, SHA-256 `aa9752ecb6d95af163d5542f7626a2f52959645852712a01f64e923231cc9afb`, read-back PASS
- `V03_ANDROID_SMOKE_SET_PASS ... formats=dwg,dxf`
- `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`
- `STAGE03_DUAL_HASH_PASS fixtures=6`
- `ANDROID_VALIDATION_V03_PASS`
- artifact `9555501552`, 7549-byte ZIP, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`

V03'te emulator çalıştırılmadı; real installable `MobilDwg.App` V04 kapsamıdır. Claim limit fixture/hash/provenance/rights/golden/test-matrix contract'tır; real app/parser/render/fidelity PASS değildir.

Ayrıntı `docs/evidence/android-validation/V03.md`.

## Sonraki iş

`NEXT_VALIDATION_STAGE = V04 — Mimari ve gerçek Android uygulama kabuğu (NOT_STARTED)`.

Bir sonraki `devam` yalnız V04'ü açar. V04 aynı turda V05'e geçmez. Implementation cursor `AŞAMA 10 — NOT_STARTED` olarak korunur.
