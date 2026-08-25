# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya yeni sohbet veya yeni bir yapay zeka oturumu başladığında projenin nerede kaldığını anlamak için tutulur. Sohbet/model belleğine güvenilmez; repo kayıtları kalıcı kaynaktır.

## Yeni bir ajan önce ne okumalı?

1. `gecmis.md`
2. `DEVAM.md`
3. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`
4. `docs/evidence/STAGE_09.md` ve `docs/ADR/0002-procad-pinned-source-no-go.md`
5. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`
6. `docs/evidence/STAGE_08.md` ve `docs/LOCAL_DEVICE_REVALIDATION.md`
7. `docs/evidence/STAGE_06.md`
8. `docs/evidence/STAGE_05.md` ve `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`
9. `docs/ARCHITECTURE.md`, `MobilDwg.sln`, `docs/EXECUTION_LOG.md`
10. `docs/evidence/STAGE_04.md`, `docs/evidence/STAGE_03.md`, `docs/evidence/STAGE_02.md`, `docs/evidence/STAGE_01.md`

## Repo kimliği

- GitHub: `smitelagwar/mobil-dwg`
- Default branch: `main`
- Private repo
- Ürün: Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer
- v1: viewer-only; edit/save/export/cloud/account kapsam dışında

## Aktif checkpoint

```text
LAST_COMPLETED_STAGE: AŞAMA 09 — DONE
DEFERRED_STAGES: AŞAMA 01; AŞAMA 06; AŞAMA 08 iOS local/device gates
STAGE01_STATUS: BLOCKED / DEFERRED_EXTERNAL_GATE — fiziksel Android install/launch ve iOS erişim envanteri açık, DONE değil
STAGE06_STATUS: BLOCKED / DEFERRED_EXTERNAL_GATE — safe-open implementation/CI PASS, fiziksel Android FilePicker/SAF+lifecycle/cache gate açık, DONE değil
STAGE07_STATUS: DONE / NO-GO — exact unpatched ProCad source candidate deterministic precision blocker nedeniyle production reuse için reddedildi
STAGE08_STATUS: DONE — CHARACTERIZATION / BLOCKED_PARTIAL_EVIDENCE / RISK_ACCEPTED_FOR_CONTINUATION; iOS runtime/device feasibility NOT PROVEN
STAGE08_DECISION_HEAD: 4987fa3e5fadfb113aa3b27ac443da9776864ad5
STAGE08_CI: run 32781026946 / #18 SUCCESS characterization; artifact 9540018558; sha256:1414e3bf5a9800e150019c48f620c64efcd3d5282ac7322ef9a5e5746ab746f7
STAGE08_PHYSICAL_IPHONE: NOT_RUN_DEFERRED_EXTERNAL_GATE
STAGE09_STATUS: DONE
STAGE09_USER_GO: GRANTED / CONSUMED — yeniden istenmez
STAGE09_SOURCE_TEST_HEAD: 9a17d333afc0a3df1de856a9a53fae0e74617c29
STAGE09_PR: #12 — MERGED
STAGE09_FINAL_PR_HEAD: 68d08bd3984ef4d1fcca027acb788c4bfcc5e43a
STAGE09_MERGE_COMMIT: 0a2dd886bbe59698a6d2eb4c99f66e7f9270063a
STAGE09_VALIDATION_HEAD: 7bba0b7a6da30dc4b23050872a7a1ef4e90ca087
STAGE09_POST_VALIDATION_DELTA: validation head -> merge commit yalnız workflow/docs/handoff; AŞAMA 09 production source/test dosyalarında değişiklik yok
STAGE09_VALIDATION_RUN: 32815175055 / #6 SUCCESS
STAGE09_VALIDATION_JOB: 97701882792 SUCCESS
STAGE09_BUILD: targeted Release + full solution Release = 0 warning / 0 error
STAGE09_MARKERS: STAGE09_DOTNET_PIN_PASS; STAGE09_T0_BUILD_PASS; STAGE04_CORE_CONTRACT_TESTS_PASS; STAGE04_RENDER_CONTRACT_TESTS_PASS; STAGE09_RENDER_SCENE_TESTS_PASS; render-scene/v1; STAGE09_T1_SCENE_PASS; STAGE04_ARCHITECTURE_TESTS_PASS; STAGE05_DEPENDENCY_BOUNDARY_PASS; STAGE04_T0_PASS; STAGE09_STAGE04_REGRESSION_PASS
STAGE09_ARTIFACT: 9551137293 / stage09-self-hosted-evidence / 1578 bytes
STAGE09_ARTIFACT_DIGEST: sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd
STAGE09_EVIDENCE: docs/evidence/STAGE_09.md
LOCAL_REVALIDATION: docs/LOCAL_DEVICE_REVALIDATION.md
EXECUTION_OVERRIDE: docs/USER_APPROVED_EXECUTION_OVERRIDE.md
NEXT_WORK_STAGE: AŞAMA 10
NEXT_WORK_STATUS: READY / WAITING_USER_CONTINUE
NEXT_ACTION: kullanıcı `devam` dediğinde yalnız AŞAMA 10 — P0 temel geometri renderer'ı — başlat; aynı turda AŞAMA 11'e geçme.
LAST_UPDATE: 2026-08-25
```

## Yürütme kuralı

AŞAMA 01'in gerçek Android/iOS dış erişim kapıları kullanıcı tarafından şimdilik ertelendi. Bunlar sahte PASS/DONE yapılmaz; `DEFERRED_EXTERNAL_GATE` olarak açık tutulur. Fiziksel erişime bağımlı olmayan sonraki aşamalar sırayla ilerler. Bir turda en fazla bir aşama tamamlanır. Release/beta/final cihaz kapılarında ertelenmiş dış kanıtlar yeniden zorunlu olur.

AŞAMA 09 için ADR 0002'deki yüksek efor/bakım riski kullanıcı tarafından açıkça kabul edildi ve stage gerçek T0/T1 kanıtıyla kapatıldı. Bu GO yeniden istenmez. AŞAMA 10 için ayrı bir GO bariyeri tanımlı değildir.

## Aşama durumu

- [x] AŞAMA 00 — Çalışma alanı ve yürütme zemini — `DONE`
- [ ] AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler — `DONE`
- [x] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi — `DONE`
- [x] AŞAMA 04 — Minimal solution ve mimari sınırlar — `DONE`
- [x] AŞAMA 05 — ACadSharp headless parser spike — `DONE`
- [ ] AŞAMA 06 — Android güvenli dosya alma ve parse spike — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO — `DONE / NO-GO`
- [x] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke — `DONE / CHARACTERIZATION; iOS PASS NOT CLAIMED`
- [x] AŞAMA 09 — RenderScene, kamera ve diagnostics temeli — `DONE`
- [ ] AŞAMA 10 — P0 temel geometri renderer’ı — `NOT_STARTED`
- [ ] AŞAMA 11 — Mobil viewport ve gesture’lar
- [ ] AŞAMA 12 — Block/INSERT/attribute dönüşümleri
- [ ] AŞAMA 13 — Layer, renk, linetype ve lineweight
- [ ] AŞAMA 14 — TEXT/MTEXT, Türkçe, font ve SHX
- [ ] AŞAMA 15 — Dimension, leader ve hatch doğruluğu
- [ ] AŞAMA 16 — Model space, layout, paper space ve viewport
- [ ] AŞAMA 17 — XREF/raster/underlay ve compatibility raporu
- [ ] AŞAMA 18 — Tam Android viewer UX ve lifecycle
- [ ] AŞAMA 19 — Kötü niyetli/bozuk dosya ve resource guard’ları
- [ ] AŞAMA 20 — Ölçümlü performans ve bellek optimizasyonu
- [ ] AŞAMA 21 — Android tam corpus regresyon ve beta kapısı
- [ ] AŞAMA 22 — Android Release/AAB/compliance RC
- [ ] AŞAMA 23 — iOS toolchain, shared core ve ilk gerçek cihaz
- [ ] AŞAMA 24 — iOS fidelity, lifecycle ve Release archive
- [ ] AŞAMA 25 — Cross-platform beta ve yalnız blocker düzeltmeleri
- [ ] AŞAMA 26 — Dependency freeze, final audit ve RC onayı
- [ ] AŞAMA 27 — v1 artifact, yayın/handoff ve kapanış

## Tarihçe özeti

### AŞAMA 00 — DONE

Repo ve `.gitignore` doğrulandı; kullanıcı belgeleri korundu. `docs/EXECUTION_LOG.md`, ADR/evidence şablonları ve `gecmis.md` oluşturuldu.

### AŞAMA 01 — BLOCKED / DEFERRED_EXTERNAL_GATE

Pinned toolchain: .NET SDK/workload set `10.0.400`, Microsoft OpenJDK `21.0.12`, Android min API `24`, target/compile `36`, Build-Tools `36.0.0`, Platform-Tools `37.0.1`, `maui-android`. CI Debug/Release ve manifest gate'leri geçti. Gerçek telefon install/launch ve iOS erişim envanteri halen dış kapıdır.

### AŞAMA 02 — DONE

Exact dependency/compliance hattı kuruldu. ACadSharp `3.7.1` dependency/lisans açısından `GREEN`; SkiaSharp `4.151.1` `REVIEW`; ProCad yalnız source-pinned spike; IxMilia.Dxf yalnız test/fallback. Central Package Management, committed lockfile, exact nupkg hash/license manifest ve audit kuruldu. PR #4 merge `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.

### AŞAMA 03 — DONE

Tekrar üretilebilir mini corpus/golden sözleşmesi kuruldu: 4 DWG familyası, 2 ASCII DXF, 0BSD sentetik Türkçe/basic/nested-block fixture'ları, missing-font/missing-XREF negatifleri ve CI-derived corrupt/truncated DWG. PR #5 merge `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.

### AŞAMA 04 — DONE

Dört production ve üç test projesinden oluşan minimal mimari kuruldu. Core BCL-only tutuldu; parser/render/UI bağımlılık sınırları otomatik architecture harness ile sabitlendi. PR #6 merge `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.

### AŞAMA 05 — DONE

ACadSharp `3.7.1` yalnız `MobilDwg.Cad` adapter'ına eklendi. Gerçek mini corpus + negatif fixture'lar geçti. Read-only parser baseline `GO`; render fidelity garantisi değildir. Final PR #7 merge `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.

### AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE

Safe-open implementation tamamlandı: provider-path bağımsız stream, actual-byte quota, disk reserve, atomic app-private cache, cleanup, worker parse, generation ID/last-request-wins ve cancellation-result-discard. Final CI run `32762879583` SUCCESS, artifact `9533538573`. Gerçek telefon FilePicker/SAF+lifecycle/cache gate açık.

### AŞAMA 07 — DONE / NO-GO

Exact ProCad source candidate `f8a862b3e7634e27664fee02ff5d68774b102985` production graph dışında değerlendirildi. Android source build başarılı olsa da origin `5,000,000` + `0.001` detail direct `double→float` scene boundary'sinde `0.0` delta'ya çöktü. Deterministic P0 fidelity blocker nedeniyle exact unpatched ProCad reuse `NO-GO`. PR #9 merge `28cc06c2de5d21f733e29ae69a38395979b6d759`.

### AŞAMA 08 — DONE / CHARACTERIZATION; iOS PASS NOT CLAIMED

Exact ACadSharp 3.7.1 + SkiaSharp 4.151.1 iOS hattı karakterize edildi. Run `32781026946` / #18, artifact `9540018558`, digest `sha256:1414e3bf5a9800e150019c48f620c64efcd3d5282ac7322ef9a5e5746ab746f7`. Hosted Xcode tool lookup blocker, ACadSharp trimming/reflection riskleri ve simulator NativeAOT limiti kaydedildi. Fiziksel iPhone/local Mac gate açık. PR #11 merge `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.

### AŞAMA 09 — DONE

ADR 0002 sonrası kullanıcı custom renderer efor/bakım riskini kabul etti. Tek production scene yolu compact özel immutable `RenderScene` seçildi; ProCad production graph'a eklenmedi.

Uygulanan temel:

- stable entity ID, bounds, layer/style token ve source reference;
- world/document ve world→view→screen hattında `double` precision;
- `Camera2D` ↔ Core `RenderViewport` explicit bridge;
- survey origin `5,000,000` + `0.001` precision regression;
- finite span/subtraction overflow guard'ları ve overflow-safe center;
- OCS/WCS arbitrary-axis transform, oblique round-trip ve scaled normalization;
- Unsupported/Substituted/Dropped/Error diagnostics;
- camera fit/zoom clamps ve dark/light color context;
- stable-ID sıralı immutable scene ve deterministic `render-scene/v1` semantic snapshot;
- default metadata bypass ve duplicate ID guard'ları.

Hosted Ubuntu/macOS/slim runner'lar bir süre checkout öncesi `runner_id=0` ile kesildi. Sonradan main'e eklenen dedicated `android-test` automation sayesinde self-hosted Windows runner'ın çevrimiçi olduğu doğrulandı. A09 için geçici validator ile gerçek exact .NET `10.0.400` execution yapıldı.

Yetkili kapanış: head `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087`, run `32815175055`/#6, job `97701882792`, `SUCCESS`. Hedefli ve full solution Release build `0 warning / 0 error`. T0/T1, deterministic snapshot, Core/Rendering/Architecture ve Stage05 dependency-boundary regresyon marker'ları geçti. Artifact `9551137293`, digest `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`. Geçici self-hosted A09 workflow'u PASS sonrası kaldırıldı; kalıcı Stage09 workflow `ubuntu-latest` olarak bırakıldı ve post-merge closure'da `main` push kapsamı eklendi. PR #12 final head `68d08bd3984ef4d1fcca027acb788c4bfcc5e43a` üzerinden merge edildi; merge commit `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`. Validation head ile merge commit arasındaki compare'da A09 production source/test değişikliği yoktur.

Ayrıntı: `docs/evidence/STAGE_09.md`.

## Değiştirilemez temel teknik kararlar

- v1 yalnız 2D viewer; edit/write yok.
- DWG/DXF cihazda/offline okunur; zorunlu cloud conversion yok.
- Autodesk RealDWG, APS/Forge dönüşümü, ticari ODA SDK, trial/ücretli CAD parser-renderer yok.
- Varsayılan runtime lisans allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD; exact dependency yine denetlenir.
- Runtime graph'ta GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/unknown release blocker'dır.
- ACadSharp `3.7.1` read-only parser baseline `GO`; render/engineering fidelity garantisi değildir.
- Exact unpatched ProCad production reuse `NO-GO`; production graph'a geri sokulmaz.
- World/document geometry ve kamera hattı `double` kalır; raw absolute world coordinate scene sınırında `float`a düşürülmez.
- UI parser entity'lerine doğrudan bağlanmaz.
- Unsupported/proxy/font/XREF/raster kaybı sessiz olmaz.
- Original drawing overwrite edilmez.

## Yeni ajan için protokol

1. Gerçek `main` HEAD ve açık PR durumunu doğrula.
2. Kullanıcı değişikliklerini koru; destructive Git işlemi yapma.
3. Kullanıcı yalnız `devam` diyorsa `NEXT_WORK_STAGE` olan **AŞAMA 10** üzerinden ilerle.
4. Bir turda en fazla bir aşama tamamla; AŞAMA 10 biterse AŞAMA 11'i aynı turda başlatma.
5. AŞAMA 01/AŞAMA 06/AŞAMA 08 dış erişim kapılarını sahte PASS/DONE yapma.
6. Dependency kendiliğinden yükseltme ve ProCad'ı production graph'a geri sokma.
7. Her turun sonunda `gecmis.md`, `DEVAM.md`, ilgili evidence ve canonical checkpoint'i gerçek durumla güncelle.

## Bir sonraki tur

Kullanıcı `devam` dediğinde yalnız **AŞAMA 10 — P0 temel geometri renderer'ı** başlatılır. Kanonik iş listesi: LINE/ARC/CIRCLE/ELLIPSE/POINT; LW/POLYLINE + bulge; SPLINE tessellation; SOLID/TRACE/3DFACE 2D görünümü; OCS/extrusion/mirror/large-coordinate fixture'ları; draw order/clipping/antialias baseline. Batching/GPU/tiling eklenmez. T1 + küçük golden/semantic diff ile doğrulanır.
