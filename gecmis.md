# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya yeni sohbet veya yeni bir yapay zeka oturumu başladığında projenin nerede kaldığını anlamak için tutulur. Sohbet/model belleğine güvenilmez; repo kayıtları kalıcı kaynaktır.

## Yeni bir ajan önce ne okumalı?

1. `gecmis.md`
2. `DEVAM.md`
3. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`
4. `ANDROID_DOGRULAMA_PLANI.md`
5. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` ve çalışma bağlamına göre `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md`
6. `docs/evidence/STAGE_09.md`, `docs/ADR/0002-procad-pinned-source-no-go.md` ve `docs/LOCAL_DEVICE_REVALIDATION.md`
7. `docs/evidence/STAGE_06.md`
8. `docs/evidence/STAGE_05.md` ve `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`
9. `docs/ARCHITECTURE.md`, `MobilDwg.sln`, `docs/EXECUTION_LOG.md`
10. `docs/evidence/STAGE_04.md`, `docs/evidence/STAGE_03.md`, `docs/evidence/STAGE_02.md`, `docs/evidence/STAGE_01.md`

## Repo kimliği

- GitHub: `smitelagwar/mobil-dwg`
- Default branch: `main`
- Private repo
- Aktif ürün: Android-only local/offline 2D DWG/DXF viewer; iOS future option
- v1: viewer-only; edit/save/export/cloud/account kapsam dışında

## Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION — aktif sıra ve Android DoD dışında
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
ANDROID_VALIDATION_PLAN: ANDROID_DOGRULAMA_PLANI.md
ANDROID_VALIDATION_CURRENT: V02 — NOT_STARTED
ANDROID_VALIDATION_NEXT: V02 — dependency, lockfile ve Android artifact sınırı
PENDING_EMULATOR_QUEUE: EMPTY
CURRENT_GATE_TRUTH: V01 hardened Stage01Smoke infrastructure gate exact Release run'da gerçek executable harness, byte-safe PNG, live PID ve package/PID crash + post-launch ANR kanıtıyla geçti
CURRENT_GATE_CLAIM_LIMIT: INFRASTRUCTURE_SMOKE_ONLY — MobilDwg.App/viewer PASS değil
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
STAGE08_IOS: HISTORICAL_CHARACTERIZATION / FUTURE_INACTIVE
STAGE09_HISTORICAL_EVIDENCE: docs/evidence/STAGE_09.md; run 32815175055/#6; artifact 9551137293; merge 0a2dd886bbe59698a6d2eb4c99f66e7f9270063a
V01_EVIDENCE: docs/evidence/android-validation/V01.md; tested SHA 698c6e901672a736f2803894efb5bda34af08212; run 32821991333; job 97721878468; artifact 9553530359
NEXT_ACTION: Yalnız V02'yi başlat — locked dependency graph, license/vulnerability policy ve Android artifact boundary doğrulaması; aynı turda V03'e geçme.
LAST_UPDATE: 2026-08-25
```

## Yürütme kuralı

AŞAMA 01–09 implementation geçmişi değiştirilmeden korunur; Android doğrulama cursor'ı V01'den başlayıp sırayla ilerler. Runner çevrim dışıysa exact SHA test kuyruğuna alınır ve host-independent kod/test işi implementation cursor'ında sürdürülebilir. Emulator sonucu uydurulmaz; fiziksel Android release öncesi yeniden zorunludur. iOS future option'dır ve aktif Android sırasını bloke etmez.

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
- [ ] AŞAMA 23 — iOS toolchain, shared core ve ilk gerçek cihaz — `DEFERRED_FUTURE_IOS / ACTIVE_SEQUENCE_OUT`
- [ ] AŞAMA 24 — iOS fidelity, lifecycle ve Release archive — `DEFERRED_FUTURE_IOS / ACTIVE_SEQUENCE_OUT`
- [ ] AŞAMA 25 — Android beta ve yalnız blocker düzeltmeleri
- [ ] AŞAMA 26 — Android dependency freeze, final audit ve RC onayı
- [ ] AŞAMA 27 — Android v1 artifact, yayın/handoff ve kapanış

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

### Android revalidation V01 — VALIDATED

V01'in başlangıçta `FIX_REQUIRED` olmasının nedeni gerçek kanıt boşluklarıydı: gate yalnız `dotnet test` çağrısıyla executable harness gövdelerini doğrulamıyor, PowerShell redirect screenshot'ı byte-safe üretmiyor ve PID/crash/ANR iddiaları yeterince sert değildi. Gate bu açıklar için düzeltildi.

Yetkili V01 koşusu exact SHA `698c6e901672a736f2803894efb5bda34af08212`, self-hosted Windows run `32821991333`, job `97721878468`, `SUCCESS`. Release solution build geçti; Core/Rendering/Architecture executable harness marker'ları gerçekten çalıştı. `Stage01Smoke` signed APK Android 16 / API 36 emulator üzerinde kuruldu, cold-launch `Status: ok` verdi ve PID `3374` zorunlu olarak bulundu. Screenshot tam PNG imzasıyla byte-safe doğrulandı, artifact indirildi ve görsel açıldı. Package/PID crash buffer boştu; post-launch events gerçek lifecycle/draw akışını gösterdi ve `dumpsys activity lastanr` boot'tan beri ANR olmadığını bildirdi.

Artifact `9553530359`, 7 dosya, 271043 byte; digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`. Sonuç yalnız `INFRASTRUCTURE_SMOKE_ONLY`; `Stage01Smoke` gerçek `MobilDwg.App` veya DWG/DXF fidelity kanıtı değildir. Ayrıntı: `docs/evidence/android-validation/V01.md`.

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
3. Kullanıcı yalnız `devam` diyorsa `ANDROID_DOGRULAMA_PLANI.md` içindeki açık **V02** üzerinden ilerle.
4. Runner çevrim dışıysa gereken Android kanıtını çoğaltma; exact SHA'yı kuyruğa al ve güvenli host-independent iş varsa implementation cursor'ında sürdürebilirsin.
5. Bir turda en fazla bir validation veya implementation aşaması tamamla; aynı turda sonraki aşamaya geçme.
6. Emulatoru fiziksel Android, `Stage01Smoke` uygulamasını viewer veya queued işi PASS sayma. iOS işi başlatma.
7. Dependency kendiliğinden yükseltme ve ProCad'ı production graph'a geri sokma.
8. Her turun sonunda iki cursor'ı, test kuyruğunu, ilgili evidence ve canonical checkpoint'i gerçek durumla güncelle.

## Bir sonraki tur

Kullanıcı `devam` veya `BASLA.md dosyasını oku` dediğinde **V02 — Dependency, lockfile ve Android artifact sınırı** başlatılır. V01 tekrar çalıştırılmaz; yalnız V02'nin package/source/license geçmişi ile güncel locked restore, resolved graph, vulnerability/license audit ve Android native/runtime artifact sınırı karşılaştırılır. ProCad ve iOS-only bileşenlerin Android production graph'a sızmadığı kanıtlanır. Bu turda V03 başlatılmaz. AŞAMA 10 implementation cursor'ı ayrı olarak korunur.
