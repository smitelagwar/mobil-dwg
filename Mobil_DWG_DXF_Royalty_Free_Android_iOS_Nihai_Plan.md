# Mobil DWG/DXF Görüntüleyici — Nihai Uygulama ve Yürütme Planı

**Plan sürümü:** 1.5  
**Son checkpoint:** 25 Ağustos 2026  
**Aktif ürün:** Android-only, local/offline, read-only 2D DWG/DXF viewer  
**iOS:** future option; aktif Android DoD ve sıranın dışında  
**Ürün ilkesi:** preview-first; original CAD immutable; dependency/artifact provenance kanıtlanmadan release yok.

> “Royalty-free”, hukuki garanti veya pazarlama sloganı değil; her release'in gerçek dependency, native asset ve dağıtım artifact'i üzerinde yeniden kanıtlanan teknik/politika kriteridir.

---

## 1. Tek yetkili yürütme checkpoint'i

```text
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_STAGE: V08 — iOS tarihsel arşiv / Android graph izolasyonu
CURRENT_SUBSTEP: V08.android_graph_check_pending
STATUS: SCOPE_ARCHIVED / ANDROID_GRAPH_CHECK_PENDING — NOT_STARTED
LAST_IMPLEMENTED_STAGE: AŞAMA 09 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ
IMPLEMENTATION_WORKSTREAM: docs/A10_WORKSTREAM.md + varsa açık A10 branch/PR
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
V06: VALIDATED — REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY
V07: VALIDATED — PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY
LAST_ANDROID_VALIDATION_EVIDENCE: docs/evidence/android-validation/V07.md
LAST_V07_TESTED_HEAD: 559c1d033bdacedc6900d9ad126e7ab21fd8aa50
LAST_V07_TESTED_PR_MERGE_REVISION: bfa728b840f63a5e9db5d5f376d19fb7f32c62f3
LAST_V07_MAIN_MERGE_COMMIT: 4b3b15afe6c95f8393147758b6d16e092ac75a21
LAST_V07_RUN_JOB: 32860034697 / 97841446382
LAST_V07_ARTIFACT: 9567840490; sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
BLOCKERS: Aktif V08 Android graph-isolation blocker'ı yok; tarihsel iOS kapsamı arşivde; fiziksel Android release öncesi ayrıca zorunlu.
NEXT_ACTION: Sonraki validation turunda yalnız V08 Android production/CI graph isolation kontrolünü başlat; iOS workflow/Mac/simulator/iPhone işini yeniden açma ve aynı turda V09'a geçme.
NEXT_IF_TEST_OFFLINE: BASLA_A10.md ile yalnız izole A10 draft branch'inde host-independent kod/test işi yap.
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
```

### `devam` protokolü

1. Gerçek `main` HEAD, açık PR, checkpoint ve kullanıcı değişiklikleri doğrulanır.
2. `ANDROID_DOGRULAMA_PLANI.md` V01–V09 bitmediyse açık VXX birinci cursor'dır.
3. Implementation cursor AŞAMA 10'da validation cursor'dan ayrı korunur.
4. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; sonraki aşama aynı turda başlatılmaz.
5. Test/evidence olmadan PASS/DONE yoktur.
6. Emulator fiziksel cihaz değildir; Stage01Smoke real viewer değildir; queued/zero-step workflow PASS değildir.
7. Dependency kendiliğinden yükseltilmez; version/license/hash/transitive/native graph tekrar kanıtlanır.
8. Destructive/force Git işlemi yapılmaz; kullanıcı değişiklikleri korunur.
9. Her kapanışta validation planı, evidence, `DEVAM.md`, `gecmis.md`, execution log ve bu checkpoint güncellenir.
10. Kullanıcı iOS'u yeniden etkinleştirmedikçe iOS build/spike/signing işi yapılmaz.
11. Genel `BASLA.md` açık VXX'i yürütür. Yalnız açık `BASLA_A10.md` komutu A10'u ayrı `stage10-p0-geometry-draft` branch'inde başlatabilir.
12. Erken A10 host/GitHub-hosted kontrolü sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması + Android gate bekleyen `CODED_PENDING_EMULATOR` olur. V04–V09 kapanmadan `main` merge/DONE yoktur ve A11 açılmaz.

---

## 2. Değiştirilemez ürün şartları

Kullanıcı açıkça değiştirmedikçe:

- v1 yalnız **2D viewer**; editor/writer değildir.
- Aktif teslim hedefi yalnız **Android**.
- `.dwg` ve `.dxf` local/device akışında doğrudan okunur; zorunlu cloud conversion yok.
- Temel açma/render local/offline; hesap/login/server zorunlu değil.
- Core viewer kullanıcı için ücretsiz; per-file/per-user/runtime CAD SDK ücreti yok.
- Autodesk RealDWG, ticari ODA SDK, Autodesk cloud conversion veya proprietary/trial CAD SDK kullanılmaz.
- Runtime'da GPL/AGPL/SSPL/BUSL/non-commercial/proprietary/unknown lisans varsayılan NO-GO.
- Default allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD; exact bileşen yine ayrı denetlenir.
- Original drawing değiştirilmez; v1 save/overwrite/save-as yok.
- Unsupported/proxy/font/XREF/raster sessiz kaybolmaz; compatibility raporuna düşer.
- UI parser entity'lerine doğrudan bağlanmaz.
- World/document koordinatları `double`; precision düşüşü yalnız test edilmiş screen boundary'sinde yapılır.
- App adı/ikonu Autodesk/AutoCAD/DWG markasını ürün markası gibi kullanmaz.

### v1 zorunlu kapsam

- local DWG ve DXF açma
- model space; corpus gerektirirse temel paper-space/layout/viewport
- pan, pinch zoom, fit extents, orientation
- layer list/show-hide
- temel 2D geometry, block/INSERT, attributes
- text/MTEXT, dimension, hatch
- CAD color/ByLayer/ByBlock, basic linetype/lineweight
- Türkçe text/encoding ve görünür font substitution
- loading/progress/cancel talebi, controlled error, compatibility raporu
- lifecycle/close/reopen/cache cleanup
- emulator smoke + beta/release fiziksel Android kanıtı

### v1 dışında

- edit/draw/OSNAP/measurement
- DWG/DXF writing
- PDF/SVG export
- cloud sync/account/collaboration
- full automatic XREF crawler
- proprietary proxy object full fidelity
- 3D CAD/BIM

---

## 3. Doğrulanmış teknoloji kararları

- .NET SDK/workload set `10.0.400`.
- Android min API `24`, target/compile API `36`.
- OpenJDK baseline `21.0.12`, Build-Tools `36.0.0`, Platform-Tools/ADB `37.0.1`.
- ACadSharp `3.7.1`: read-only parser baseline `GO` — ADR 0001; V05 gerçek Android parser smoke PASS; render fidelity garantisi değil.
- SkiaSharp `4.151.1`: renderer dependency; Android native inventory V02'de doğrulandı.
- Microsoft.Maui.Controls `10.0.100`: gerçek Android app direct dependency; exact `[10.0.100]`, MIT; V04'te doğrulandı.
- Exact unpatched ProCad source reuse `NO-GO` — ADR 0002; V07 güncel production/resolved graph ve APK izolasyonunu doğruladı; survey-origin `5,000,000 + 0.001` direct-float blocker yeniden üretildi.
- IxMilia.Dxf `[0.8.4]`: test/fallback scope; production runtime'a otomatik alınmaz.
- Production direct NuGet versions strict exact range; lockfile + locked restore zorunlu.

---

## 4. Hedef mimari

```text
Android MAUI App / platform adapters
        │
        ▼
Application + CadSession
        │
        ├── ICadDocumentReader ──► ACadSharp adapter
        │                            └── parse diagnostics
        ▼
Read-only document handle / metadata
        │
        ▼
IRenderSceneBuilder ──► immutable RenderScene + CompatibilityReport
        │
        ▼
ICadRenderer ──► Skia renderer ──► Android canvas/view
```

Kurallar:

- Core BCL-only kalır; MAUI/ACadSharp/Skia sızıntısı yok.
- Parser yalnız Cad adapter arkasında.
- Rendering parser document type'ına bağlanmaz.
- App composition/platform adapter katmanıdır.
- `RenderScene` derived/rebuildable'dır; original document değildir.
- CadSession/document handle deterministic dispose edilir.
- Safe-open source path varsaymaz; stream/content URI/local private copy kontratı kullanır.
- ProCad production ProjectReference/PackageReference/native graph'a girmez.
- Future iOS dönüşü shared Core/Cad/Rendering katmanlarını fork etmeden adapter sınırından yapılabilmelidir.

V04 itibarıyla gerçek repository app shell:

- `src/MobilDwg.App`
- `net10.0-android36.0`
- package `com.smitelagwar.mobildwg`
- gerçek `MainActivity` / `MainApplication`
- API36 emulator build/install/cold-launch/UI/liveness PASS

V05 itibarıyla gerçek app validation build'i production `AcadSharpDocumentReader` ile V03 redistributable DXF/DWG smoke inputs'ı Android process içinde parse eder; validation asset/gate'i production writer/save özelliği değildir.

V06 itibarıyla gerçek `MainPage` MAUI FilePicker'ı production `MauiCadFilePickerAdapter` üzerinden stream olarak safe-open coordinator'a bağlar. Seçilen provider içeriği app-private cache'e bounded/atomic kopyalanır, production parser yalnız private copy üzerinde çalışır ve original external CAD immutable kalır. API36 emulator üzerinde DWG/DXF selection, second selection, cancel, rotate, background/foreground, close-cleanup ve reopen akışları PASS aldı; physical provider/device fidelity release gate'e deferred'dır.

V07 itibarıyla exact rejected ProCad candidate production graph dışında tutulmaya devam eder. Current production `src`, lockfile/resolved assets, app package graph ve Release APK üzerinde ProCad/ProCadSharp yokluğu doğrulanmış; world/document precision hattının `double` survey-origin regresyonu yeniden geçmiştir.

---

## 5. Fidelity ve compatibility sözleşmesi

“Dosya açıldı” tek başarı metriği değildir:

1. **Parse success** — controlled document oluşturuldu.
2. **Scene success** — beklenen entity semantiği render scene'e aktarıldı.
3. **Render success** — frame crash/NaN/sonsuz döngü olmadan çizildi.
4. **Engineering fidelity** — position/scale/text/block/dimension/visibility kabul sınırında.

Compatibility:

- `C0`: desteklenmiyor; algılandı ve açık uyarı.
- `C1`: parse; render doğrulanmadı.
- `C2`: approximate/substituted; teknik fidelity garantisi yok ve uyarı var.
- `C3`: semantic/golden kabul edildi.
- `C4`: mühendislik-kritik fixture ile ayrıca doğrulandı.

P0 release entity'leri en az C3; teknik dimension/annotation fixture'ları C4 hedefler. Yanlış yaklaşık dimension yerine açık warning tercih edilir.

### P0 entity sırası

- LINE, ARC, CIRCLE, ELLIPSE, POINT
- LWPOLYLINE/POLYLINE + bulge
- SPLINE
- SOLID, TRACE, 3DFACE 2D görünümü
- TEXT, MTEXT
- INSERT/nested INSERT/ATTRIB/ATTDEF
- DIMENSION
- HATCH

### Zorunlu precision/transform fixture sınıfları

- OCS→WCS / extrusion normal
- büyük/negatif koordinatlar
- survey origin `5,000,000 + 0.001`
- polyline bulge işaretleri
- nested block rotation/mirror/non-uniform scale
- Layer 0/ByLayer/ByBlock
- Türkçe CP1254/Unicode text
- dimension anonymous block
- hatch island/broken boundary
- layout/viewport clip
- missing font/XREF/raster/proxy warning

---

## 6. Fixture ve golden firewall

Authoritative contract: `fixtures/manifest/stage03-mini.json`, `docs/GOLDEN_CONTRACT.md`, `docs/evidence/android-validation/V03.md`.

- Public upstream CAD sample internetten erişilebilir diye redistributable sayılmaz.
- ACadSharp binary sample corpus immutable revision + hash ile `remote-reference-only`.
- Committed sentetik CAD `fixtures/public/` altında explicit rights profile ile olmalı.
- Private/customer drawing Git'e commit edilmez.
- CAD committed hash evidence `HEAD:<path>` Git blob bytes'a dayanır.
- `.gitattributes`: `*.dwg binary`, `*.dxf -text`.
- V04–V09 redistributable smoke set: committed 0BSD DXF + exact ACadSharp 3.7.1 generator ile validation-time AC1015 DWG + missing-font/missing-XREF negatif DXF.
- Generated DWG magic + DwgReader read-back zorunlu; output binary golden değildir.
- Image golden yalnız deterministic viewport/theme/font + açık redistribution evidence ile commit edilir.

---

## 7. Dependency / license / artifact firewall

Her runtime dependency/asset için exact version/resolved graph, source, nupkg/source hash, license, transitive/native entries, redistribution sonucu ve final artifact varlığı kaydedilir.

Zorunlu:

- strict exact direct version + CPM + lockfile + locked restore
- floating/latest/open lower-bound yok
- unknown license/native binary/asset = release blocker
- rejected dependency kaynak kodu production/test vektörü olarak kopyalanmaz
- proprietary AutoCAD SHX/font bundle edilmez
- Android RC'de APK/AAB extraction + SBOM + notices + compliance snapshot zorunlu

V02 probe graph: ACadSharp 3.7.1, SkiaSharp 4.151.1, SkiaSharp.NativeAssets.Android 4.151.1. V04 gerçek app direct graph'e Microsoft.Maui.Controls 10.0.100 exact/MIT ekledi. V05 final technical head'de dependency/corpus regresyonları ayrıca PASS aldı. V06 production dependency baseline'ını değiştirmedi; FilePicker/safe-open doğrulaması mevcut MAUI + ACadSharp graph'ı üzerinde yapıldı. V07 aynı exact production graph'ı yeniden restore edip ProCad/ProCadSharp'ın static source/project, lockfile/resolved assets, app package graph ve Release APK'da bulunmadığını doğruladı.

---

## 8. Android geriye dönük validation programı

Yetkili ayrıntı: `ANDROID_DOGRULAMA_PLANI.md`.

- V01 `VALIDATED`: toolchain/self-hosted/emulator/Stage01Smoke infrastructure; real viewer değil.
- V02 `VALIDATED`: dependency/lockfile/license/hash/vulnerability/Android-native boundary.
- V03 `VALIDATED`: fixture/provenance/golden/redistributable Android smoke-set/device matrix.
- V04 `VALIDATED`: gerçek installable `MobilDwg.App` MAUI shell; API36 build/install/cold-launch/PID/UI/PNG/crash-ANR/liveness PASS; viewer fidelity değil.
- V05 `VALIDATED`: production ACadSharp reader real Android app process içinde V03 DXF/DWG smoke setiyle PASS; render fidelity değil.
- V06 `VALIDATED`: real-app FilePicker/DocumentsUI/SAF → stream → app-private safe-copy → production parser; lifecycle/cleanup/immutability PASS; claim emulator-only, physical provider fidelity değil.
- V07 `VALIDATED`: exact rejected ProCad NO-GO; current production/resolved graph + APK isolation; deterministic direct-float precision blocker and production double regression PASS.
- **V08 `SCOPE_ARCHIVED / ANDROID_GRAPH_CHECK_PENDING — NOT_STARTED`: iOS historical archive + Android graph isolation.**
- V09: RenderScene/camera/diagnostics revalidation.

V06 authoritative:

- PR `#19`
- tested PR head `ae8682875524157285946724bd70d6ff010f3917`
- tested PR synthetic merge `26b3cdd6ca50d34b98a4806d92f50d4828077d41`
- main merge `e17e2472f38557552698b8cf9526d6cbf8b25580`
- run/job `32849725110` / `97807551403` — SUCCESS
- artifact `9564837027`, 29,743,234 bytes; digest `sha256:a88eaf46d7cc2090111cb18ce81c3a1d9b56eaed08bdfd070fb0a22be74194a0`
- validation APK `30,917,242` bytes; SHA-256 `4bcd819def4483fbc076865dd70b10026eb2eae7515c07561a9cdfe02ff9c9a5`
- real DWG SAF open + second-selection DXF + rotate/background-foreground/picker-cancel/close-cleanup/reopen + original input immutability PASS
- same-head V04 run/job `32849725215 / 97807552081` SUCCESS; artifact `9565016182`, digest `sha256:6922f2168334e8312debc2c90cb7905d9db5da58eb8cb10da3f8aadf6e53bb3f`
- same-head V05 run/job `32849725272 / 97807552194` SUCCESS; artifact `9565243977`, digest `sha256:36ada98dd79f7f70e2ef7e63d6d2cb6cec191141421c07bcf41673dded23b492`
- marker `ANDROID_VALIDATION_V06_PASS`
- claim `REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY`

V07 authoritative:

- PR `#20`
- tested PR head `559c1d033bdacedc6900d9ad126e7ab21fd8aa50`
- exact checked-out PR synthetic merge `bfa728b840f63a5e9db5d5f376d19fb7f32c62f3`
- main merge `4b3b15afe6c95f8393147758b6d16e092ac75a21`
- run/job `32860034697` / `97841446382` — SUCCESS
- artifact `9567840490`, 19,293 bytes; digest `sha256:bb2de209e3f6aecf74dc0d17dc9cf996a795cbeb8975a418f90d99d0d267d0b7`
- Release APK `30,913,146` bytes; SHA-256 `4605ff85da02e4b45e8d4ae523ae9f5e678a8f596fbbaca23cef77edcab7d450`; ProCad entry absent
- rejected direct-float survey delta `0`; production double survey delta `0.001`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `V07_PRODUCTION_DOUBLE_PRECISION_REGRESSION_PASS`
- marker `ANDROID_VALIDATION_V07_PASS`
- claim `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`

---

## 9. Implementation aşamaları

AŞAMA 00–09 tarihsel implementation evidence `docs/evidence/STAGE_XX.md` ve ADR'lerde korunur. Implementation cursor AŞAMA 10'dadır fakat `main`e henüz merge edilmemiştir. V04–V09 validation hattı ana/öncelikli sıradır; bilgisayar veya runner kapalıyken yalnız `BASLA_A10.md` protokolüyle izole branch'te sınırlı A10 taslağı hazırlanabilir. Bu taslak validation sonucu, `main` revision veya tamamlanmış aşama sayılmaz.

### AŞAMA 10 — P0 temel geometri renderer'ı — `MAIN'E HENÜZ MERGE EDİLMEDİ`

LINE/ARC/CIRCLE/ELLIPSE/POINT, polyline+bulge, SPLINE, SOLID/TRACE/3DFACE 2D; OCS/extrusion/mirror/large-coordinate; draw-order/clipping/AA baseline. Önce correctness; GPU/batching/tiling yok.

Paralel erken çalışma sınırı:

- Ayrı branch: `stage10-p0-geometry-draft`; `android-test` geliştirme branch'i değildir.
- V04–V09 sürerken yalnız yeni/internal platform-neutral primitive-tessellator matematiği ve saf testler yapılır. V09 kapanana kadar mevcut RenderScene/interface/snapshot, architecture, `.csproj`/Skia ve fixture/image-golden sözleşmeleri dondurulur; A11, MAUI/FilePicker/lifecycle ve ProCad kapsam dışıdır.
- Host/hosted build-harness yoksa `CODED_PENDING_HOST_TESTS`; bu kontroller gerçekten geçse bile Android gate öncesi en ileri durum `CODED_PENDING_EMULATOR`dır. `main` merge/DONE yasaktır.
- V09 sonrasında güncel validated `main` branch'e alınır. Etkilenen V02/V03, V04–V07, V08 Android graph-isolation, V09 ve A10 T1/semantic-golden/C3 exact integration SHA üzerinde geçer; iOS workflow açılmaz. Gerçek `MobilDwg.App` API36 render kanıtı PID/PNG/crash/ANR yanında expected-content pixel probe, Android golden veya kayıtlı görsel incelemeden en az birini içerir.
- A10 yalnız doğrulanmış PR main'e merge, post-merge kontrol ve `docs/evidence/STAGE_10.md` kapanışı sonrasında `DONE` olur.

### AŞAMA 11 — Mobil viewport ve gesture

Giriş kapısı: `V04–V09 PROGRAM CLOSED` + `AŞAMA 10 DONE ON MAIN` + `PENDING_EMULATOR_QUEUE EMPTY`. AŞAMA 11, A10 kapanış turunda başlatılmaz.

- pan, pinch zoom, fit extents, gerekirse double-tap fit
- pinch focal point preservation
- min/max zoom/overscroll guards
- portrait/landscape/safe area
- rotation reparse yapmaz
- debug-only frame timing

Çıkış: küçük/orta fixture navigation stabil; gerçek Android frame baseline kaydedilir.

### AŞAMA 12 — Block/INSERT/attribute

Nested transform order, mirror/non-uniform scale, ATTRIB/ATTDEF, Layer0/ByBlock/ByLayer context, cycle/depth/instance guards.

### AŞAMA 13 — Layer/color/linetype/lineweight

Layer state, ACI/true color, central style resolver, basic linetype/lineweight; unsupported complex style açık warning.

### AŞAMA 14 — TEXT/MTEXT/Türkçe/font/SHX

CP1254/Unicode, TEXT alignment/mirror, bounded MTEXT parser, audited font fallback, visible substitution; proprietary font bundle yok.

### AŞAMA 15 — Dimension/leader/hatch

Önce anonymous dimension block, temel dimension types, hatch island/broken boundary; yanlış dimension yerine warning.

### AŞAMA 16 — Model/layout/paper-space/viewport

Layout selector, paper-space entity, viewport clip/center/height/twist ve layer override; layout change reparse yapmaz.

### AŞAMA 17 — XREF/raster/underlay/compatibility

Detect/warn; remote auto-download yok; explicit folder grant varsa bounded mapping; external kaynak eksikliği sessiz değil.

### AŞAMA 18 — Tam Android viewer UX/lifecycle

Home/open/loading/viewer/layers/fit/info/warnings/close, recent URI/grant metadata, Back/background/foreground/orientation/process recreation/memory pressure, no-backup, permission/log redaction, accessibility.

### AŞAMA 19 — Malicious/corrupt input ve resource guards

Magic/version preflight, size/entity/depth/scene/text/hatch/raster/XREF budgets, NaN/Infinity/cycle guards, corrupt/truncated controlled errors, bounded mutation/fuzz smoke.

### AŞAMA 20 — Ölçümlü performance/memory

Physical Android Release TTFUP/frame p50/p95/PSS/GC/artifact size; small/medium/large corpus; yalnız profiler/AB evidence ile optimization.

### AŞAMA 21 — Android full corpus regression / beta gate

Full public/private corpus parse/scene/render/golden, P0/P1 matrix, physical Android, Debug/Release/trimming/AOT/artifact farkları.

### AŞAMA 22 — Android Release/AAB/compliance RC

Final package/icon/version, live target SDK/Play/Data Safety/privacy, accessibility/licenses, secure signing, signed APK+AAB physical smoke, artifact inventory, SBOM/notices/compliance/trademark review.

### AŞAMA 23–24 — Future iOS track

`DEFERRED_FUTURE_IOS / ACTIVE_ANDROID_SEQUENCE_OUT`. Kullanıcı yeniden etkinleştirmeden Mac/Xcode/iPhone/iOS işi yapılmaz.

### AŞAMA 25 — Android beta ve blocker düzeltmeleri

Yalnız crash/privacy/P0 fidelity/open/lifecycle/severe perf blocker; yeni feature/edit/export eklenmez.

### AŞAMA 26 — Dependency freeze / final audit / RC approval

Toolchain/dependency freeze, full regression/perf/signed artifact smoke, APK/AAB inventory+SBOM/license/source/native/font/asset match; unknown = NO-GO.

### AŞAMA 27 — Android v1 artifact / yayın / handoff

Final APK/AAB/checksums/build instructions, store-ready/submission, clean reproduction, user-approved release snapshot, privacy/compatibility/notices/limitations/support docs.

---

## 10. Risk kaydı

| Risk | Zorunlu tepki |
|---|---|
| ACadSharp fixture fidelity farkı | pinned A/B + independent fixture; sistematikse parser gate yeniden açılır |
| ProCad precision/lineage risk | NO-GO korunur; upstream patch ancak yeni evidence/ADR ile |
| Renderer scope büyür | P0 bitir; P1/P2 warning ile ertelenebilir; edit/export ekleme |
| SHX/font farklılığı | visible substitution + audited fallback; proprietary bundle yok |
| OOM/ANR | controlled resource guard; profiler tabanlı optimization; largeHeap son çare |
| Corpus rights belirsiz | redistribution durur; evidence çözülmeden commit/bundle yok |
| Emulator fazla yorumlanır | real app/process/artifact marker olmadan viewer PASS yok |
| Self-hosted runner offline | exact SHA queue; aynı işi spamleme; kanıtsız PASS yok |
| Unknown native/transitive asset | release NO-GO |
| Dependency terk edilir | pinned source archive + adapter sayesinde controlled alternative spike |
| Scope creep | backlog; v1 DoD bitmeden başlama |

---

## 11. Android v1 Definition of Done

Plan ancak tamamı gerçek evidence ile sağlandığında `DONE`:

- [ ] Gerçek Android cihazda local DWG ve DXF açılıyor; emulator smoke ayrıca mevcut.
- [ ] P0 geometry/block/text/dimension/hatch acceptance matrix geçiyor.
- [ ] Pan/pinch/fit/layer/lifecycle stabil.
- [ ] Unsupported/proxy/font/XREF/raster sessiz değil.
- [ ] Corrupt/adversarial corpus controlled behavior üretiyor; crash/ANR blocker yok.
- [ ] Physical-device performance/memory budgets kaydedildi ve kabul edildi.
- [ ] Full corpus Android Release regression geçiyor.
- [ ] Original drawing immutable; cloud/account zorunlu değil.
- [ ] Runtime dependency/native/font/asset chain'de unknown/policy-RED yok.
- [ ] APK/AAB inventory, SBOM, notices ve release evidence eşleşiyor.
- [ ] CAD SDK/API per-user/per-file/runtime royalty/mandatory service fee saptanmadı.
- [ ] Core viewer kullanıcı için ücretsiz.
- [ ] Signed/store-ready artifact, checksum, build/use docs teslim.
- [ ] Known compatibility limits yayımlanabilir metinde dürüstçe belirtilmiş.

“Tüm DWG'leri AutoCAD ile piksel piksel aynı gösterir” bir DoD değildir ve vaat edilmez.

---

## 12. v1 sonrası backlog — plan bitmeden başlanmaz

1. read-only entity selection/properties
2. measurement: distance/area/coordinate + unit validation
3. user-granted project folder full XREF resolution
4. advanced paper-space/complex linetype/underlay
5. PDF/SVG export için ayrı fidelity/license spike
6. command/undo-redo editor
7. save-as-copy + round-trip corpus; original overwrite yine default kapalı

---

## Nihai teknik ilke

> Doğrudan oku; cihazda işle; eksikliği saklama; önce doğruluğu kanıtla; sonra yalnız ölçülmüş darboğazı optimize et; final artifact'in tamamının kaynağını ve lisansını gösterebilmeden release yapma.