# AŞAMA 09 Kanıtı — RenderScene, kamera ve diagnostics temeli

## Durum

`IN_PROGRESS — IMPLEMENTATION_READY / T0_T1_VALIDATION_PENDING_RUNNER`

AŞAMA 09 henüz `DONE` değildir. Uygulama kodu ve hedefli testler yazılmıştır; ancak gerçek derleme/test yürütme kanıtı alınmadan çıkış kriteri kapatılmaz.

## Karar ve kapsam

- Base `main`: `b7926cb1df2b2ff1f32c67033dba73aed1c01523`.
- Branch: `stage09-render-scene-camera`.
- Scene/camera implementation code head: `5b3f590dca123c3855e8aac7d48f781ba2cdfdb3`.
- Son source/test-changing head: `6215f9fbd77028273262bc5b95fd3eece19191d3`.
- Sonraki commitler yalnız evidence ve geçici runner-probe housekeeping içindir; source/test içeriğini değiştirmez.
- PR: `#12` — `stage09: add render scene, camera, and diagnostics foundation`.
- ADR 0002 ProCad exact pinned candidate için `NO-GO` verdiğinden tek production scene yolu **compact özel immutable RenderScene** olarak seçildi.
- ProCad package/source production graph'a eklenmedi; paralel ikinci scene graph oluşturulmadı.
- AŞAMA 10 geometry renderer kapsamına başlanmadı.

## AŞAMA 09 requirement matrisi

| Gereksinim | Uygulama / test | Durum |
|---|---|---|
| Tek scene implementation | `src/MobilDwg.Rendering/Scene/RenderScene.cs`; compact custom immutable scene | IMPLEMENTED |
| Stable entity ID / bounds / layer-style token / source reference | `Scene/SceneGeometry.cs` | IMPLEMENTED |
| World/document coordinate `double` | `WorldPoint2`, `WorldPoint3`, `WorldBounds2`; camera pipeline `double` | IMPLEMENTED |
| Tek world → view → screen hattı | `Camera/Camera2D.cs` / `CameraTransform` | IMPLEMENTED |
| OCS/WCS | `Coordinates/OcsTransform.cs`; normal + oblique round-trip tests | IMPLEMENTED |
| Extents / invalid NaN-Infinity / büyük koordinat | `WorldBounds2` guards + rendering console tests | IMPLEMENTED |
| Scene diagnostics | `Diagnostics/SceneDiagnostics.cs`: Unsupported/Substituted/Dropped/Error; dört taxonomy türünün hedefli testleri | IMPLEMENTED |
| Camera fit/zoom bounds | `Camera2D.Fit`, `ZoomBy`, min/max clamps | IMPLEMENTED |
| Background/color context | `RenderColorContext`, Dark/Light presets | IMPLEMENTED |
| Deterministic semantic snapshot | `Snapshots/RenderSceneSemanticSnapshot.cs`; insertion-order-independent test | IMPLEMENTED |
| ProCad facade sınırı | N/A — ADR 0002 exact ProCad candidate'ı reddetti; custom scene yolu seçildi | NOT_APPLICABLE |

## Precision tasarım kuralı

AŞAMA 07'de reddedilen ProCad hattındaki kritik hata, absolute CAD world koordinatının scene sınırında doğrudan `float`a çevrilmesiydi. AŞAMA 09 hattında:

1. world/document geometry `double` tutulur;
2. `WorldToView` önce camera center'ı `double` olarak çıkarır;
3. view → screen ölçekleme yine `double` olarak yapılır;
4. scene modelinde raw absolute `float` coordinate bulunmaz.

Hedefli regression testi survey origin `5,000,000` çevresinde `0.001` world-unit ayrıntının camera transform sonrasında yaklaşık bir pixel olarak korunmasını ve screen/world round-trip'in `double` hassasiyetini korumasını zorunlu kılar.

## Determinism / immutable boundary

- `RenderScene` entity dizisini stable ID ile ordinal sıralayıp defensive `ReadOnlyCollection` olarak dışarı verir.
- Diagnostics defensive read-only collection kullanır.
- Duplicate stable entity ID assembler seviyesinde reddedilir.
- Snapshot formatı invariant culture ve round-trip double (`R`) formatı kullanır.
- Aynı semantic scene farklı insertion order ile oluşturulduğunda snapshot eşitliği test edilir.
- Eski `STAGE04_RENDER_CONTRACT_TESTS_PASS` marker'ı korunur; AŞAMA 09 test genişletmesi AŞAMA 04 contract harness davranışını sessizce değiştirmez.

## T0/T1 doğrulama durumu

### İlk hosted Ubuntu denemesi

Stage 09 run `32783063933` / #2, job `97609094989`:

- `conclusion=failure`;
- job `steps=[]`;
- `runner_id=0`;
- `runner_name=""`;
- label `ubuntu-latest`.

Bu, checkout/restore/build/test başlamadan runner atanamadığını gösterir. Kod test failure kanıtı değildir.

Aynı PR head ailesinde Stage 01/02/04/05/06/07 Ubuntu workflow'larının da adım başlamadan aynı şekilde hızlı failure vermesi ortak runner-allocation problemiyle uyumludur.

Stage 04 örneği, head `5b3f590dca123c3855e8aac7d48f781ba2cdfdb3`:

- run `32783276606` / #29;
- job `97609756351`;
- `steps=[]`, `runner_id=0`, empty runner name;
- `conclusion=failure`.

### macOS hosted fallback

Stage 09 workflow yalnız platformdan bağımsız `net10.0` Core/Rendering testlerini çalıştırmak üzere `macos-26-arm64` hosted runner'a taşındı.

Güncel source/test validation request:

- Run `32783883913` / #7.
- Job `97611597376`.
- Source/test head: `6215f9fbd77028273262bc5b95fd3eece19191d3`.
- Son gözlem: `queued`, henüz runner/adım yok.

Bu run gerçek T0/T1 sonucu üretmeden AŞAMA 09 kapatılmayacaktır.

### Configured self-hosted runner probe

Repo içinde daha önce tanımlanmış `[self-hosted, windows, android-test, mobil-dwg]` etiketli runner için geçici Stage 09 probe çalıştırıldı.

- PR run `32784140351` / #3.
- Job `97612382891`.
- Son gözlem: `queued`, `steps=null`; uygun çevrimiçi runner atanmadı.
- Probe workflow PR'dan tekrar silindi; production veya kalıcı CI yüzeyine eklenmedi.
- Bu probe PASS değildir ve fiziksel Android cihaz kanıtı değildir.

### Container fallback

Mevcut execution container'ında `dotnet` kurulu değildir. Exact repo pin'i `.NET SDK 10.0.400` için resmi Microsoft download hattı doğrulandı; ancak container'ın dış indirme/network kısıtı nedeniyle SDK payload'ı alınamadı. Bu nedenle farklı SDK ile sahte yerel PASS üretilmedi.

## Beklenen T0/T1 marker'ları

`.github/workflows/stage09-render-scene.yml` başarı durumunda şunları üretir:

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

AŞAMA 01, AŞAMA 06 ve AŞAMA 08'in fiziksel/local dış kapıları değişmeden açık kalır. Bu dosya onların yerine cihaz kanıtı değildir.
