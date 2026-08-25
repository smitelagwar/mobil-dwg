# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya projenin kalıcı, kısa tarihçe/checkpoint kaydıdır. Ayrıntılı teknik kanıtın asıl kaynağı `docs/evidence/`, kararların kaynağı `docs/ADR/`, aktif Android doğrulama sırasının kaynağı `ANDROID_DOGRULAMA_PLANI.md` dosyasıdır.

## Yeni ajan okuma sırası

1. Gerçek `main` HEAD ve açık PR'ları doğrula.
2. `BASLA.md`.
3. `DEVAM.md`.
4. `ANDROID_DOGRULAMA_PLANI.md`.
5. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`.
6. Aktif `docs/evidence/android-validation/VXX.md` ve önceki validation evidence.
7. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`, çalışma bağlamına göre `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md`.
8. Gereken tarihsel `docs/evidence/STAGE_XX.md` / `docs/ADR/` kayıtları.

## Repo ve ürün

- GitHub: `smitelagwar/mobil-dwg` — private, default `main`.
- Aktif v1 ürün: Android-only local/offline read-only 2D DWG/DXF viewer.
- iOS: future option; aktif sıra/DoD dışında.
- v1 dışında: edit/save/export/cloud/account.

## Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
ANDROID_VALIDATION_PLAN: ANDROID_DOGRULAMA_PLANI.md
ANDROID_VALIDATION_CURRENT: V03 — NOT_STARTED
ANDROID_VALIDATION_NEXT: V03 — fixture, golden sözleşmesi ve Android test matrisi
PENDING_EMULATOR_QUEUE: EMPTY
V01_EVIDENCE: docs/evidence/android-validation/V01.md; tested SHA 698c6e901672a736f2803894efb5bda34af08212; run 32821991333; job 97721878468; artifact 9553530359
V02_EVIDENCE: docs/evidence/android-validation/V02.md; branch head 50694547e7be43e5ec414cc91b57cbd32faa3c54; tested PR merge ref 549770192c181b30db8968cec5c6ac3c2407e133; run 32824397251; job 97729154385; artifact 9554326162
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Yalnız V03'ü başlat; aynı turda V04'e geçme.
LAST_UPDATE: 2026-08-25
```

## Yürütme kuralı

Android revalidation V01–V09 ayrı cursor'dır. Implementation cursor AŞAMA 10'da korunur. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır. Runner çevrim dışıysa kanıtsız PASS yazılmaz; host-independent güvenli iş sürdürülebilir. Emulator fiziksel Android yerine geçmez. `Stage01Smoke` gerçek viewer değildir. iOS aktif Android hattını bloke etmez.

## Implementation aşama durumu

- [x] AŞAMA 00 — çalışma alanı/yürütme zemini — `DONE`
- [ ] AŞAMA 01 — toolchain + fiziksel telefon dış kapısı — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 02 — dependency/lisans/lockfile — `DONE`
- [x] AŞAMA 03 — corpus/golden/matris — `DONE`
- [x] AŞAMA 04 — minimal solution/mimari sınırlar — `DONE`
- [x] AŞAMA 05 — ACadSharp parser spike — `DONE`
- [ ] AŞAMA 06 — Android safe-open fiziksel kapısı — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 07 — ProCad source spike — `DONE / NO-GO`
- [x] AŞAMA 08 — iOS characterization — `DONE / HISTORICAL; iOS PASS NOT CLAIMED`
- [x] AŞAMA 09 — RenderScene/kamera/diagnostics — `DONE`
- [ ] AŞAMA 10 — P0 geometri renderer — `NOT_STARTED`
- [ ] AŞAMA 11–22 — Android viewer geliştirme ve release hazırlığı
- [ ] AŞAMA 23–24 — `DEFERRED_FUTURE_IOS`
- [ ] AŞAMA 25–27 — Android beta/freeze/final handoff

## Tarihsel implementation özeti

### AŞAMA 00–04

Repo/yürütme standardı kuruldu; pinned .NET/Android toolchain ve CI baseline oluşturuldu. Central Package Management, dependency evidence, lockfile ve audit geldi. Redistributable mini corpus/golden sözleşmesi oluşturuldu. Core/Cad/Rendering/App ve test projeleriyle mimari sınırlar kuruldu. Ayrıntı ilgili `docs/evidence/STAGE_00..04` kayıtlarındadır.

### AŞAMA 05 — parser baseline

ACadSharp `3.7.1` read-only adapter mini corpus üzerinde doğrulandı. ADR 0001 sonucu `GO`; render fidelity garantisi değildir. PR #7 merge `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.

### AŞAMA 06 — safe-open

Provider-path bağımsız stream, actual-byte quota, disk reserve, atomic app-private cache, cleanup, generation/last-request-wins ve cancellation-result-discard uygulandı. Host CI geçti; gerçek fiziksel Android FilePicker/SAF/lifecycle/cache kapısı release öncesi açıktır.

### AŞAMA 07 — ProCad NO-GO

Exact pinned ProCad source production graph dışında incelendi. `5,000,000 + 0.001` survey-origin detail'i direct `double→float` scene boundary'sinde kayboldu. ADR 0002 sonucu exact unpatched ProCad production reuse `NO-GO`. PR #9 merge `28cc06c2de5d21f733e29ae69a38395979b6d759`.

### AŞAMA 08 — iOS characterization

ACadSharp/SkiaSharp iOS hattı tarihsel olarak karakterize edildi; fiziksel iPhone/Mac gate kapanmadı ve iOS PASS iddiası yok. Aktif Android-only kararından sonra bu track future option olarak arşivlendi.

### AŞAMA 09 — RenderScene foundation

Custom renderer efor/bakım riski kullanıcı tarafından kabul edildi. Compact immutable `RenderScene`; stable IDs/bounds/layer-style/source metadata; double precision world→view→screen; Camera2D/RenderViewport bridge; OCS/WCS; overflow/invalid geometry guards; diagnostics; dark/light context; deterministic `render-scene/v1` snapshot uygulandı.

Yetkili validation run `32815175055`, job `97701882792`, artifact `9551137293`; merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`. Ayrıntı `docs/evidence/STAGE_09.md`.

## Android revalidation geçmişi

### V01 — VALIDATED

Başlangıç gate'inde dört evidence açığı bulundu: executable harness'lar `dotnet test` ile gerçekte yürümüyor, screenshot byte-safe değil, PID zorunlu değil, crash/ANR evidence zayıf. Gate sertleştirildi.

Yetkili exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`; self-hosted Windows Release run `32821991333`, job `97721878468`, `SUCCESS`. Toolchain, executable markers, Stage01Smoke install/cold-launch, PID `3374`, byte-safe PNG, package/PID crash ve post-launch ANR/lifecycle evidence geçti. Artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`.

Claim limit `INFRASTRUCTURE_SMOKE_ONLY`; gerçek `MobilDwg.App`/fidelity PASS değildir. PR #14 ile V01 kapanışı `main`e alındı. Ayrıntı `docs/evidence/android-validation/V01.md`.

### V02 — VALIDATED

Tarihsel AŞAMA 02'de direct package'lar “exact” diye sınıflandırılmıştı; ancak plain CPM sürümleri lockfile'da `[3.7.1, )` / `[4.151.1, )` open lower-bound request üretmişti. V02 bunu gerçek strict exact NuGet range'e çevirdi:

- ACadSharp `[3.7.1]`
- SkiaSharp `[4.151.1]`
- IxMilia.Dxf `[0.8.4]` — test/fallback only

Audit sertleştirildi: exact target/resolved/requested graph; license/nupkg hash; manifest reproducibility; vulnerability; `src/` PackageReference/TFM/ProjectReference boundary; vendored native yasağı; SkiaSharp Android native ABI inventory; ProCad/IxMilia/iOS leakage kontrolleri zorunlu hale geldi.

Kalıcı `Stage 02 Dependency Audit` self-hosted Windows runner'a taşındı. Yetkili run `32824397251`, job `97729154385`, `SUCCESS`; branch head `50694547e7be43e5ec414cc91b57cbd32faa3c54`; tested PR merge ref `549770192c181b30db8968cec5c6ac3c2407e133`. Artifact `9554326162`, digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`. Artifact indirildi; summary/resolved graph/vulnerability raporu incelendi. Resolved graph yalnız ACadSharp 3.7.1, SkiaSharp 4.151.1 ve SkiaSharp.NativeAssets.Android 4.151.1 içeriyor; vulnerable package yok.

V02 için emulator gerekmemiştir; gerçek installable `MobilDwg.App` V04 işidir. Claim limit dependency/lockfile/license/hash/vulnerability/source/native boundary'dir. Ayrıntı `docs/evidence/android-validation/V02.md`.

## Kalıcı teknik kararlar

- Original CAD immutable; overwrite/writer yok.
- ACadSharp `3.7.1` read-only parser baseline `GO`.
- Exact unpatched ProCad production reuse `NO-GO`.
- UI parser entity'lerine doğrudan bağlanmaz.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olarak gizlenmez.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; unknown/policy-RED release blocker.
- Production package sürümleri strict exact NuGet range ile pinlenir; locked restore zorunludur.
- Fiziksel Android release öncesi yeniden zorunludur.
- iOS yalnız açık yeni kullanıcı kararıyla etkinleşir.
