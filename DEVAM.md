# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff

Bu dosya yeni bir ChatGPT/AI oturumunda projeye kaldığı yerden devam etmek için tek giriş noktasıdır. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunu, gerçek `main` HEAD'ini ve açık PR'ları doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `ANDROID_DOGRULAMA_PLANI.md`, `gecmis.md`, `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` ve aktif/tarihsel evidence dosyasını oku.
3. **Çalışma bağlamını gerçek araç erişimine göre sınıflandır.** Kod/depo değişiklikleri ChatGPT sohbetinden GitHub üzerinden yapılıyor ve yerel repo/terminal/ADB'ye doğrudan erişim yoksa `CHATGPT_REMOTE_GITHUB` bağlamıdır; bu durumda `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` dosyasını okumak zorunludur. Dosyanın okunması zorunlu olsa da içindeki batching/test sıklığı/zaman yönetimi önerileri zorunlu değildir. Yerel IDE/ajan gerçek çalışma ağacı + terminal/ADB erişimiyle çalışıyorsa `LOCAL_IDE` bağlamıdır ve remote test modeli yürütme için geçersizdir.
4. Açık Android V01–V09 doğrulamasını birinci cursor olarak sürdür. Runner çevrim dışı ve doğrulama için güvenli iş kalmadıysa implementation cursor'ında host-independent kod yazılabilir; Android kanıtı beklemede kalır.
5. Bir kullanıcı turunda en fazla bir doğrulama veya implementation aşaması tamamla; aynı turda sonraki aşamayı başlatma.
6. Emulatoru fiziksel cihaz, geçici smoke APK'yı gerçek viewer ve queued runner işini PASS sayma. iOS aktif kapsam dışıdır.
7. Her aşama sonunda iki cursor'ı, test kuyruğunu ve yeni/tarihsel evidence kayıtlarını gerçek CI/commit/artifact kanıtıyla güncelle.
8. Production dependency'yi evidence olmadan yükseltme veya ProCad'ı tekrar graph'a sokma.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg` (private), default `main`.
- Aktif v1 Android-only local/offline 2D DWG/DXF viewer; iOS future option olarak mimari düzeyde korunur.
- v1 viewer-only; edit/save/export/cloud/account yok.

## Çalışma bağlamı notu

`CHATGPT_REMOTE_GITHUB` bağlamında mevcut Android/self-hosted test altyapısı her küçük GitHub değişikliğinde çalıştırılmak zorunda değildir. Ajan aynı mantıksal işte birkaç düşük-riskli değişikliği tamamlayıp sonra ilgili test hattını bir kez tetikleyebilir; riskli tek bir değişiklikte hemen test etmeyi de seçebilir. Bu batching davranışı öneridir, zorunlu değildir.

PC açık olsa bile `C:\actions-runner\run.cmd` dinlemiyorsa remote emulator işi çalışmaz. Böyle durumda aynı workflow'u çoğaltma; exact SHA'yı test kuyruğuna al, host testleri/kod işini sürdür ve gerçek kanıt olmadan `VALIDATED/DONE` yazma.

Ayrıntılı remote test modeli: `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md`.

## Güncel checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
ANDROID_VALIDATION_PROGRAM: V01–V09
ANDROID_VALIDATION_CURRENT: V02 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
CURRENT_GATE_TRUTH: V01 hardened gate gerçek executable harness'ları çalıştırıyor; Stage01Smoke Release APK build/install/launch yapıyor; numeric PID zorunlu; byte-safe PNG ve package/PID crash + post-launch ANR kanıtı doğrulanıyor
CURRENT_GATE_CLAIM_LIMIT: INFRASTRUCTURE_SMOKE_ONLY; Stage01Smoke gerçek MobilDwg.App/viewer fidelity kanıtı değildir
NEXT_ACTION: Yalnız V02'yi başlat — dependency/lockfile/license-vulnerability policy ve Android artifact boundary doğrulaması; aynı turda V03'e geçme.
```

## Android V01 özeti

V01 `VALIDATED`. Exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`, self-hosted Windows Android Emulator Release run `32821991333`, job `97721878468`. .NET `10.0.400`, `maui-android`, OpenJDK 21.0.12 baseline, Android API 36, Build-Tools 36.0.0, ADB 37.0.1 ve `mobil-dwg-api36` doğrulandı. Core/Rendering/Architecture executable harness marker'ları gerçekten yürütüldü. Stage01Smoke APK Android 16 emulator üzerinde cold-launch `Status: ok` verdi; PID `3374`, byte-safe PNG, boş crash buffer ve `lastanr`/events kanıtı doğrulandı. Artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`. Screenshot indirildi ve açıldı. Kapsam yalnız `INFRASTRUCTURE_SMOKE_ONLY`; gerçek viewer PASS değildir. Ayrıntı: `docs/evidence/android-validation/V01.md`.

## AŞAMA 09 özeti

Kullanıcı ADR 0002'deki özel renderer yüksek efor/bakım riskini kabul etti. Exact ProCad reuse `NO-GO` olduğundan tek production scene yolu compact özel immutable scene seçildi; ProCad production graph'a eklenmedi.

Tamamlanan foundation:

- Stable entity ID, bounds, layer/style token ve parser source reference.
- `default` record-struct metadata bypass ve duplicate-ID guard'ları.
- World/document coordinates ve world→view→screen hattında `double` precision.
- `Camera2D` ile Core `RenderViewport` arasında explicit adapter.
- Survey origin `5,000,000` çevresinde `0.001` detail precision regression.
- Finite span/subtraction overflow guards ve overflow-safe center.
- OCS/WCS arbitrary-axis transform, oblique round-trip ve büyük finite normal için scaled normalization.
- Unsupported/Substituted/Dropped/Error scene diagnostics.
- Camera fit, zoom clamps ve dark/light color context.
- Stable-ID sıralı immutable scene ve deterministic `render-scene/v1` semantic snapshot.

Yetkili kapanış validation'ı self-hosted Windows runner üzerinde exact .NET `10.0.400` ile gerçek checkout/restore/build/run yaptı. Hedefli A09 Release build ve tam solution Release build `0 warning / 0 error`; Core/Rendering/Architecture ve Stage 05 dependency-boundary regresyonları geçti. Artifact `9551137293`, digest `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`. PR #12 final head `68d08bd3984ef4d1fcca027acb788c4bfcc5e43a` üzerinden `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a` merge commit'i ile `main`e alındı. Validation head → merge commit compare'ında A09 production source/test dosyası değişmedi.

Önceki `ubuntu-latest`, `macos-26` ve `ubuntu-slim` zero-step/runner_id=0 kayıtları hosted runner allocation problemiydi; self-hosted gerçek execution PASS ile A09 implementation failure olmadığı ayrıştırıldı. Geçici self-hosted A09 workflow'u kapanıştan sonra branch'ten kaldırıldı; kalıcı Stage 09 workflow'u `ubuntu-latest` üzerinde bırakıldı ve post-merge closure ile `main` push kapsamı eklendi.

## Önceki kritik kararlar

- ACadSharp `3.7.1` read-only parser baseline: GO.
- Exact unpatched ProCad source candidate: NO-GO; large-survey-origin mm detail direct double→float scene boundary'sinde çöker.
- AŞAMA 08 iOS characterization tarihsel olarak korunur; aktif Android v1'de yeniden çalıştırılmaz.
- AŞAMA 01/AŞAMA 06 fiziksel Android farkları release öncesine deferred; iOS gate'leri future track'te inactive'dir.

## Değiştirilemez ilkeler

- Original CAD immutable; overwrite yok.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olarak gizlenmez.
- UI parser entity'lerine doğrudan bağlanmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; policy-RED/unknown release blocker.
- Gerçek cihaz veya test yürütme kanıtı yoksa PASS yazılmaz.
- Bir turda en fazla bir aşama tamamlanır.
