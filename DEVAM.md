# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff

Bu dosya yeni bir ChatGPT/AI oturumunda projeye kaldığı yerden devam etmek için tek giriş noktasıdır. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunun gerçek `main` HEAD'ini ve açık PR'larını doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `ANDROID_DOGRULAMA_PLANI.md`, `gecmis.md`, `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` ve aktif Android validation evidence dosyasını oku.
3. GitHub üzerinden çalışılıyor ve yerel terminal/ADB doğrudan erişilebilir değilse çalışma bağlamı `CHATGPT_REMOTE_GITHUB`'dır; `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` okunur.
4. Android V01–V09 doğrulama cursor'ını öncelikli sürdür. Implementation cursor ayrı tutulur.
5. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; aynı turda sonraki aşama başlatılmaz.
6. Emulator fiziksel cihaz sayılmaz; geçici `Stage01Smoke` gerçek viewer sayılmaz; queued/zero-step workflow PASS sayılmaz.
7. Her aşama sonunda exact SHA, run/job/artifact ve claim limit gerçek kanıtla kaydedilir.
8. Production dependency evidence olmadan yükseltilmez; ProCad production graph'a geri sokulmaz.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg` — private, default `main`.
- Aktif v1 hedefi: Android-only, local/offline, read-only 2D DWG/DXF viewer.
- iOS: gelecekte yeniden açılabilecek mimari seçenek; aktif kapsam dışı.
- v1 dışında: edit/save/export/cloud/account.

## Güncel checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
ANDROID_VALIDATION_PROGRAM: V01–V09
ANDROID_VALIDATION_CURRENT: V03 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — dependency/lockfile/license/hash/vulnerability/Android-native boundary only
NEXT_ACTION: Yalnız V03'ü başlat — fixture/golden/provenance/private-ignore ve Android test matrisi doğrulaması; aynı turda V04'e geçme.
```

## Android V01 özeti

V01 `VALIDATED`. Exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`; self-hosted Windows Android Emulator Release run `32821991333`, job `97721878468`. .NET `10.0.400`, `maui-android`, OpenJDK 21.0.12 baseline, Android API 36, Build-Tools 36.0.0, ADB 37.0.1 ve AVD `mobil-dwg-api36` doğrulandı. Core/Rendering/Architecture executable harness marker'ları gerçekten yürütüldü. Temporary `Stage01Smoke` APK Android 16/API 36 emulator üzerinde cold-launch `Status: ok` verdi; PID `3374`, byte-safe PNG, crash buffer ve ANR/lifecycle evidence doğrulandı. Artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`.

Claim limit: yalnız emulator/toolchain altyapı smoke. Gerçek `MobilDwg.App` veya DWG/DXF fidelity PASS değildir. Ayrıntı: `docs/evidence/android-validation/V01.md`.

## Android V02 özeti

V02 `VALIDATED`. Tarihsel AŞAMA 02'de “exact pin” denmesine rağmen plain CPM sürümlerinin lockfile'da açık alt sınır (`[3.7.1, )`) ürettiği bulundu ve düzeltildi. Central Package Management artık strict exact NuGet range kullanır:

- ACadSharp `[3.7.1]`
- SkiaSharp `[4.151.1]`
- test/fallback-only IxMilia.Dxf `[0.8.4]`

Hardened audit exact graph, exact requested range, license/nupkg hash, vulnerability, production `src/` PackageReference/TFM sınırı, vendored native binary yasağı ve Android native inventory'yi zorunlu doğrular. Android probe graph yalnız ACadSharp `3.7.1`, SkiaSharp `4.151.1` ve transitive SkiaSharp.NativeAssets.Android `4.151.1` içerir. ProCad, IxMilia ve iOS-only bileşen sızıntısı bulunmadı.

Yetkili kalıcı workflow: `Stage 02 Dependency Audit`, self-hosted Windows. Run `32824397251`, job `97729154385`, `SUCCESS`; branch head `50694547e7be43e5ec414cc91b57cbd32faa3c54`, tested PR merge ref `549770192c181b30db8968cec5c6ac3c2407e133`. Marker'lar: `V02_TOOLCHAIN_PASS`, `V02_LOCKED_RESTORE_PASS`, `V02_EXACT_VERSION_POLICY_PASS`, `V02_ANDROID_BOUNDARY_PASS`, `STAGE02_PACKAGE_AUDIT_PASS`, `V02_PACKAGE_AUDIT_PASS`, `V02_VULNERABILITY_PASS`, `ANDROID_VALIDATION_V02_PASS`.

Artifact `9554326162`, digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`, 6 dosya. Artifact indirildi ve resolved graph/summary/vulnerability raporu incelendi. V02 için emulator gerekmedi: gerçek installable `MobilDwg.App` henüz yok; bu V04 işidir.

Claim limit: V02 yalnız dependency, lockfile, license/hash, vulnerability, production source boundary ve Android native package sınırını kanıtlar. Viewer/APK PASS değildir. Ayrıntı: `docs/evidence/android-validation/V02.md`.

## Implementation baseline — AŞAMA 09

AŞAMA 09 `DONE`. Kullanıcı ADR 0002'de custom renderer efor/bakım riskini kabul etti. Exact unpatched ProCad reuse `NO-GO`; production scene yolu compact özel immutable `RenderScene`.

Temel sözleşmeler: stable entity ID/bounds/layer/style/source metadata; `double` precision world→view→screen hattı; Camera2D/RenderViewport explicit bridge; survey origin `5,000,000 + 0.001` regression; OCS/WCS dönüşümleri; overflow/invalid geometry guard'ları; diagnostics; deterministic `render-scene/v1` snapshot. Yetkili A09 validation run `32815175055`, artifact `9551137293`; merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.

## Kalıcı kararlar

- ACadSharp `3.7.1`: read-only parser baseline `GO`.
- ProCad exact unpatched candidate: production reuse `NO-GO`.
- Original CAD immutable; overwrite/writer yolu yok.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olarak gizlenmez.
- UI parser entity'lerine doğrudan bağlanmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; unknown/policy-RED release blocker.
- Fiziksel Android farkları release öncesi gerçek cihaz kapısında açık kalır.
- iOS yalnız yeni açık kullanıcı kararıyla yeniden etkinleşir.
- Bir turda en fazla bir aşama tamamlanır.
