# AŞAMA 09 Kanıtı — RenderScene, kamera ve diagnostics temeli

## Durum

`IN_PROGRESS — IMPLEMENTATION_READY / T0_T1_VALIDATION_PENDING_RUNNER`

AŞAMA 09 henüz `DONE` değildir. Uygulama kodu ve hedefli testler yazılmıştır; ancak gerçek derleme/test yürütme kanıtı alınmadan çıkış kriteri kapatılmaz.

## Karar ve kapsam

- Base `main`: `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.
- Branch: `stage09-render-scene-camera`.
- İlk scene/camera implementation head: `5b3f590dca123c3855e8aac7d48f781ba2cdfdb3`.
- Son source/test hardening head: `9a17d333afc0a3df1de856a9a53fae0e74617c29`.
- Stage 09 workflow platform-neutral validation için `ubuntu-latest` kullanır; macOS/iOS kanıtı AŞAMA 08/23 kapsamındadır.
- PR: `#12` — `stage09: add render scene, camera, and diagnostics foundation`.
- ADR 0002 exact ProCad candidate için `NO-GO` verdiğinden tek production scene yolu **compact özel immutable RenderScene** olarak seçildi.
- ProCad package/source production graph'a eklenmedi; paralel ikinci scene graph oluşturulmadı.
- AŞAMA 10 geometry renderer kapsamına başlanmadı.

## AŞAMA 09 requirement matrisi

| Gereksinim | Uygulama / test | Durum |
|---|---|---|
| Tek scene implementation | `src/MobilDwg.Rendering/Scene/RenderScene.cs`; compact custom immutable scene | IMPLEMENTED |
| Stable entity ID / bounds / layer-style token / source reference | `Scene/SceneGeometry.cs`; default-value bypass guard'ları | IMPLEMENTED |
| World/document coordinate `double` | `WorldPoint2`, `WorldPoint3`, `WorldBounds2`; camera pipeline `double` | IMPLEMENTED |
| Tek world → view → screen hattı | `Camera/Camera2D.cs` / `CameraTransform`; finite point guards | IMPLEMENTED |
| Core viewport bridge | `Camera2D.ToViewport/FromViewport`; renderer contract ile açık tek adapter | IMPLEMENTED |
| OCS/WCS | `Coordinates/OcsTransform.cs`; normal + oblique round-trip ve scaled normalization | IMPLEMENTED |
| Extents / invalid NaN-Infinity / büyük koordinat | finite-span guards, overflow-safe center, survey precision tests | IMPLEMENTED |
| Scene diagnostics | `Unsupported/Substituted/Dropped/Error`; invalid enum/default entity ID guards | IMPLEMENTED |
| Camera fit/zoom bounds | `Camera2D.Fit`, `ZoomBy`, min/max clamps, invalid default-camera guard | IMPLEMENTED |
| Background/color context | `RenderColorContext`, Dark/Light presets | IMPLEMENTED |
| Deterministic semantic snapshot | insertion-order-independent invariant/round-trip snapshot | IMPLEMENTED |
| ProCad facade sınırı | N/A — ADR 0002 exact ProCad candidate'ı reddetti | NOT_APPLICABLE |

## Precision ve robustness tasarım kuralı

AŞAMA 07'de reddedilen ProCad hattındaki kritik hata, absolute CAD world koordinatının scene sınırında doğrudan `float`a çevrilmesiydi. AŞAMA 09 hattında:

1. world/document geometry `double` tutulur;
2. `WorldToView` camera center'ı `double` olarak çıkarır;
3. view → screen ölçekleme yine `double` yapılır;
4. scene modelinde raw absolute `float` coordinate bulunmaz;
5. finite girdilerin çıkarma/span sırasında `Infinity` üretmesine izin verilmez;
6. çok büyük finite OCS normal vektörleri önce scale edilerek normalize edilir.

Hedefli regression testi survey origin `5,000,000` çevresinde `0.001` world-unit ayrıntının camera transform sonrasında yaklaşık bir pixel olarak korunmasını ve screen/world round-trip'in `double` hassasiyetini korumasını zorunlu kılar.

## Determinism / immutable boundary

- `RenderScene` entity dizisini stable ID ile ordinal sıralayıp defensive `ReadOnlyCollection` olarak dışarı verir.
- Diagnostics defensive read-only collection kullanır.
- Duplicate stable entity ID assembler seviyesinde reddedilir.
- `RenderEntityId`, layer/style token record-struct'larının `default` ile constructor bypass etmesi scene boundary'de tekrar doğrulanır.
- Source index negatif olamaz; supplied handle boş/whitespace olamaz.
- Snapshot formatı invariant culture ve round-trip double (`R`) formatı kullanır.
- Aynı semantic scene farklı insertion order ile oluşturulduğunda snapshot eşitliği test edilir.
- Eski `STAGE04_RENDER_CONTRACT_TESTS_PASS` marker'ı korunur.

## T0/T1 doğrulama durumu

### Hosted runner allocation bulgusu

İlk Ubuntu denemeleri, doğru `macos-26` fallback denemeleri ve daha sonra tekrar standard `ubuntu-latest` üzerinde aynı davranışı gösterdi: job checkout başlamadan `steps=[]`, `runner_id=0`, empty runner name ile failure oldu. Bu, compile/test failure kanıtı değildir.

Önemli kayıtlar:

- Ubuntu ilk Stage 09 run `32783063933` / #2, job `97609094989`: pre-step failure.
- macOS doğru label run `32786600644` / #14:
  - attempt 1 job `97619697255`;
  - attempt 2 job `97619957457`;
  - attempt 3 job `97631138677`;
  - üçünde de `steps=[]`, `runner_id=0`, label `macos-26`.
- Linux standard runner doğrulaması head `a456f0e058346b3df2ee704ca2bb810a86d1c767`:
  - run `32790863975` / #18;
  - job `97631981506`;
  - `ubuntu-latest`, `steps=[]`, `runner_id=0`.
- Son source/test hardening head `9a17d333afc0a3df1de856a9a53fae0e74617c29`:
  - Stage 09 run `32791241242` / #28;
  - job `97633067562`;
  - yine runner/adım başlamadan failure.

Aynı head ailelerinde Stage 01/02/04/05/06/07/08 workflow'larının da pre-step failure/queue davranışı göstermesi problemi A09 kaynak koduna özgü olmaktan çıkarır.

2026-08-25 kontrolünde resmi GitHub status sayfası Actions hizmetini operational gösteriyordu. Bu nedenle genel bir GitHub Actions outage iddia edilmez. Repo/account billing/quota/policy veya kapasite gibi daha özel bir root cause connector kanıtıyla doğrulanamadığından tahmin edilmez.

### Configured self-hosted runner probe

Repo içinde daha önce tanımlanmış `[self-hosted, windows, android-test, mobil-dwg]` etiketli runner için geçici Stage 09 probe çalıştırıldı.

- PR run `32784140351` / #3.
- Job `97612382891`.
- Uygun çevrimiçi runner atanmadı; job queued kaldı.
- Probe workflow PR'dan tekrar silindi; kalıcı CI yüzeyine eklenmedi.
- Bu probe PASS değildir ve fiziksel Android cihaz kanıtı değildir.

### Container fallback

Mevcut execution container'ında `dotnet` kurulu değildir. Exact `.NET SDK 10.0.400` Microsoft `dotnet/core` 10.0.11 release metadata'sında Linux x64 binary URL + SHA-512 ile doğrulandı. Container dış DNS/network erişimi olmadığından SDK payload'ı indirilemedi. Farklı SDK ile sahte yerel PASS üretilmedi.

## Beklenen T0/T1 marker'ları

`.github/workflows/stage09-render-scene.yml` gerçek runner aldığında başarı durumunda şunları üretir:

- `STAGE09_DOTNET_PIN_PASS`
- `STAGE09_T0_BUILD_PASS`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`
- `STAGE09_T1_SCENE_PASS`
- `render-scene/v1` semantic snapshot

Artifact: `stage09-render-scene-evidence` / `stage09-render-scene.log`.

## Açık çıkış kriteri

AŞAMA 09 exit kriteri şu anda **sağlanmış sayılmaz**. Gerekli kalan adım:

1. exact .NET `10.0.400` üzerinde Stage 09 T0 restore/build + T1 deterministic scene/camera tests gerçek runner'da çalışacak;
2. varsa compiler/test hataları aynı AŞAMA 09 branch'inde düzeltilecek;
3. marker'lar ve artifact doğrulanacak;
4. bundan sonra canonical checkpoint/gecmis/DEVAM `DONE` olarak güncellenip PR #12 merge edilebilecek.

AŞAMA 01, AŞAMA 06 ve AŞAMA 08'in fiziksel/local dış kapıları değişmeden açık kalır. AŞAMA 10 bu doğrulama gelmeden başlatılmaz.
