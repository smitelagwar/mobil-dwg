# Mobil DWG/DXF Görüntüleyici — Nihai Uygulama ve Yürütme Planı

**Plan sürümü:** 1.6  
**Son checkpoint:** 25 Ağustos 2026  
**Aktif ürün:** Android-only, local/offline, read-only 2D DWG/DXF viewer  
**iOS:** future option; aktif Android DoD ve sıranın dışında  
**Ürün ilkesi:** preview-first; original CAD immutable; dependency/artifact provenance kanıtlanmadan release yok.

> “Royalty-free”, hukuki garanti veya pazarlama sloganı değil; her release'in gerçek dependency, native asset ve dağıtım artifact'i üzerinde yeniden kanıtlanan teknik/politika kriteridir.

---

## 1. Tek yetkili yürütme checkpoint'i

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
ANDROID_REVALIDATION_01_09: CLOSED / VALIDATED_WITH_CLAIM_LIMITS
LAST_VALIDATION_STAGE: V09 — VALIDATED
LAST_VALIDATION_EVIDENCE: docs/evidence/android-validation/V09.md
LAST_IMPLEMENTED_STAGE: AŞAMA 21 — DONE
LAST_IMPLEMENTATION_EVIDENCE: docs/evidence/STAGE_21.md
IMPLEMENTATION_CURSOR: AŞAMA 22 — NOT_STARTED
IMPLEMENTATION_WORKSTREAM: AŞAMA 21 DONE (docs/evidence/STAGE_21.md)
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
V05: VALIDATED — REAL_ANDROID_APP_PARSER_SMOKE_ONLY_NOT_RENDER_FIDELITY
V06: VALIDATED — REAL_ANDROID_APP_FILEPICKER_SAF_SAFE_OPEN_EMULATOR_ONLY_NOT_PHYSICAL_PROVIDER_FIDELITY
V07: VALIDATED — PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY
V08: VALIDATED — ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED
V09: VALIDATED — RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Sonraki normal BASLA/devam turunda AŞAMA 22'yi (Android Release/AAB/compliance RC) başlat.
A20_MAIN_MERGE: 1603154 (PR #33)
A21_MAIN_MERGE: 919888b (PR #34)
A21_GATE: CLOSED / PASSED
A22_GATE: OPEN
```

### `devam` protokolü

1. Gerçek `main` HEAD, açık PR/branch, Actions ve checkpoint doğrulanır.
2. Açık bir claim-invalidating regression yoksa V01–V09 programı yeniden çalıştırılmaz; normal cursor AŞAMA 10'dur.
3. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; sonraki aşama aynı turda başlatılmaz.
4. Test/evidence olmadan PASS/DONE/READY_TO_MERGE yoktur.
5. Emulator fiziksel cihaz değildir; Stage01Smoke gerçek viewer değildir; queued/zero-step workflow PASS değildir.
6. Dependency kendiliğinden yükseltilmez; exact version/license/hash/transitive/native graph tekrar kanıtlanır.
7. Force/destructive Git işlemi yapılmaz; kullanıcı değişiklikleri korunur.
8. Kullanıcı iOS'u yeniden etkinleştirmedikçe iOS build/spike/signing işi yapılmaz.
9. A10 exact integration SHA gerekli regresyonları ve gerçek Android expected-content render gate'ini geçmeden `main` merge edilmez.
10. A10 `DONE ON MAIN` olmadan A11 açılmaz.

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
- Microsoft.Maui.Controls `10.0.100`: gerçek Android app direct dependency; exact `[10.0.100]`, MIT.
- Exact unpatched ProCad source reuse `NO-GO` — ADR 0002; V07 production/resolved graph ve APK izolasyonunu doğruladı; survey-origin `5,000,000 + 0.001` direct-float blocker yeniden üretildi.
- IxMilia.Dxf `[0.8.4]`: test/fallback scope; production runtime'a otomatik alınmaz.
- Production direct NuGet versions strict exact range; lockfile + locked restore zorunlu.
- V08: active Android production/CI graph iOS-specific TFM/RID/native/toolchain zorunluluğundan izole; historical iOS characterization future-only arşivdir.
- V09: immutable RenderScene/camera/OCS/diagnostics sözleşmesi current exact Android composition üzerinde yeniden PASS; deterministic `render-scene/v1` ve survey-origin `0.001` double precision korunuyor.

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

Doğrulanmış katmanlar:

- V04: gerçek repository app shell `src/MobilDwg.App`, `net10.0-android36.0`, package `com.smitelagwar.mobildwg`, API36 install/launch/liveness.
- V05: production `AcadSharpDocumentReader` gerçek Android process içinde DWG/DXF smoke.
- V06: MAUI FilePicker → DocumentsUI/SAF → stream → bounded app-private safe-copy → production parser; external original immutable.
- V07: ProCad production graph dışında; world/document precision `double`.
- V08: historical iOS spike production solution/aktif Android CI dışında.
- V09: RenderScene/camera/OCS/diagnostics deterministic foundation + Android app composition build current exact revision üzerinde PASS.

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
- Validation smoke set: committed 0BSD DXF + exact ACadSharp 3.7.1 generator ile validation-time AC1015 DWG + missing-font/missing-XREF negatif DXF.
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

Validated dependency baseline: ACadSharp 3.7.1, SkiaSharp 4.151.1, SkiaSharp.NativeAssets.Android 4.151.1, Microsoft.Maui.Controls 10.0.100 exact/MIT. V07 ProCad absence; V08 iOS-specific resolved graph/native absence; V09 same-job V02 prerequisite bu sınırları current exact revision üzerinde yeniden doğruladı.

---

## 8. Android geriye dönük validation programı — `CLOSED`

Yetkili ayrıntı: `ANDROID_DOGRULAMA_PLANI.md`.

- V01 `VALIDATED`: infrastructure smoke; real viewer değil.
- V02 `VALIDATED`: dependency/lockfile/license/hash/vulnerability/Android-native boundary.
- V03 `VALIDATED`: fixture/provenance/golden/Android smoke-set contract.
- V04 `VALIDATED`: real installable Android app shell runtime; viewer fidelity değil.
- V05 `VALIDATED`: production ACadSharp reader real Android process smoke; render fidelity değil.
- V06 `VALIDATED`: real FilePicker/SAF safe-open emulator; physical provider fidelity değil.
- V07 `VALIDATED`: ProCad NO-GO production graph isolation + precision regression.
- V08 `VALIDATED`: Android production/CI graph iOS isolation; historical iOS future-only.
- V09 `VALIDATED`: RenderScene/camera/OCS/diagnostics + Android composition revalidation; geometry renderer fidelity değil.

### V09 authoritative closure

- PR `#22`
- tested head `892315966f895729e866947a838df93350fdfd97`
- synthetic merge `6fea8ba9d1de6811afd0dcace7a2c8b5b6ec573a`; tested head'e göre file diff yok
- main merge `143ce1a79448f53af81faee9c6e650321047dd37`
- run/job `32864617493 / 97856686115` — SUCCESS
- artifact `9569686660`, 11,544 byte; digest `sha256:97e55129367ea5b778edf99a6d84939e95f74902db655144d32dbf24ba8aa375`
- same-job V02 prerequisite PASS
- exact .NET `10.0.400`
- targeted RenderScene/Core/Architecture Release builds PASS, zero warnings/errors
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`
- deterministic `render-scene/v1`
- snapshot survey line `5000000.001`
- `V09_SURVEY_ORIGIN_DOUBLE_PRECISION_PASS delta=0.001`
- full solution Release build `0 Warning / 0 Error`
- real Android Release APK `30,913,146` byte; SHA-256 `a0080fb4826cbd6f7fee1d84cac3465c8ebda766bfba245167d73233ab1a40f5`
- marker `ANDROID_VALIDATION_V09_PASS`
- claim `RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY`

İlk V09 diagnostic run/job `32864458158 / 97856153440`, PowerShell 5.1 validation portability false-negative'i nedeniyle ürün testlerinden önce durdu; production/test failure değildir. Diagnostic artifact `9569504762`, digest `sha256:7eda4ec7db3d423cdbd476bc4769eebac54ef0527c18656c0fc2bbd0b2eb90f8`.

---

## 9. Implementation aşamaları

AŞAMA 00–19 tarihsel implementation evidence `docs/evidence/STAGE_XX.md` ve ADR'lerde korunur. AŞAMA 19 PR #32 ile tamamlanmış ve `main`e merge edilmiştir. Normal implementation cursor artık AŞAMA 20'dir.

### AŞAMA 10 — P0 temel geometri renderer'ı — `DONE`

Amaç: LINE/ARC/CIRCLE/ELLIPSE/POINT, polyline+bulge, SPLINE, SOLID/TRACE/3DFACE 2D görünümü için correctness-first renderer temelini kurmak.

Zorunlu sınırlar:

- world/document coordinate `double` kalır;
- OCS/extrusion/mirror/large-coordinate doğru transform edilir;
- draw-order/clipping/antialias baseline açıkça test edilir;
- GPU/batching/tiling erken optimizasyon yok;
- ProCad yok;
- original CAD immutable ve renderer scene-derived çalışır;
- mevcut `render-scene/v1`/architecture sözleşmesi ancak açık gerekçe + regression evidence ile değiştirilir.

A10 merge kapısı:

1. Güncel validated `main` baz alınır.
2. Platform-neutral primitive/tessellator unit/executable testleri PASS.
3. Etkilenen V02/V03, architecture ve V09 regresyonları exact integration SHA'da PASS.
4. Gerçek `MobilDwg.App` API36 renderer integration build/install/launch çalışır.
5. Android render kanıtı PID/PNG/crash/ANR yanında **expected-content** kanıtı içerir: pixel probe, deterministic Android golden veya kayıtlı görsel inceleme.
6. Geometri kabulü C3 hedefiyle fixture bazlı yapılır; yalnız non-blank ekran PASS değildir.
7. PR normal merge commit ile `main`e alınır; post-merge evidence/checkpoint kapanır.
8. Ancak bundan sonra A10 `DONE ON MAIN` olur.

### AŞAMA 11 — Mobil viewport ve gesture — `DONE`

Giriş kapısı: `AŞAMA 10 DONE ON MAIN` + `PENDING_EMULATOR_QUEUE EMPTY`. PR #24 ile tamamlandı.

- pan, pinch zoom, fit extents, gerekirse double-tap fit
- pinch focal point preservation
- min/max zoom/overscroll guards
- portrait/landscape/safe area
- rotation reparse yapmaz
- debug-only frame timing

### AŞAMA 12 — Block/INSERT/attribute — `DONE`

Giriş kapısı: `AŞAMA 11 DONE ON MAIN`. PR #25 ile tamamlandı.

Nested transform order, mirror/non-uniform scale, ATTRIB/ATTDEF, Layer0/ByBlock/ByLayer context, cycle/depth/instance guards.

### AŞAMA 13 — Layer/color/linetype/lineweight — `DONE`

Giriş kapısı: `AŞAMA 12 DONE ON MAIN`. PR #26 ile tamamlandı.

Layer state, ACI/true color, central style resolver, basic linetype/lineweight; unsupported complex style açık warning.

### AŞAMA 14 — TEXT/MTEXT/Türkçe/font/SHX

Giriş kapısı: `AŞAMA 13 DONE ON MAIN`. PR #27 ile tamamlandı.

CP1254/Unicode, TEXT alignment/mirror, bounded MTEXT parser, audited font fallback, visible substitution; proprietary font bundle yok.

### AŞAMA 15 — Dimension/leader/hatch — `DONE`

Giriş kapısı: `AŞAMA 14 DONE ON MAIN`. PR #28 ile tamamlandı.

Önce anonymous dimension block (`*D...`), temel dimension types (Aligned, Rotated Linear, Radial, Diametric), dejenere ölçü korumaları (`DEGENERATE_DIMENSION_POINTS`, `INVALID_DIMENSION_GEOMETRY`), leader/multileader geometrisi, hatch 1mm kapanma toleransı ve kırık sınır teşhisi, EvenOdd ada doldurma, ANSI31 kırpılmış desen çizgisi üretimi.

### AŞAMA 16 — Model/layout/paper-space/viewport — `DONE`

Giriş kapısı: `AŞAMA 15 DONE ON MAIN`. PR #29 ile tamamlandı.

Layout selector, paper-space entity, viewport clip/center/height/twist ve layer override; layout change reparse yapmaz.

### AŞAMA 17 — XREF/raster/underlay/compatibility — `DONE`

Giriş kapısı: `AŞAMA 16 DONE ON MAIN`. PR #30 ile tamamlandı.

Detect/warn; remote auto-download yok; explicit folder grant varsa bounded mapping; external kaynak eksikliği sessiz değil.

### AŞAMA 18 — Tam Android viewer UX/lifecycle — `DONE`

Giriş kapısı: `AŞAMA 17 DONE ON MAIN`. PR #31 ile tamamlandı.

Home/open/loading/viewer/layers/fit/info/warnings/close, recent URI/grant metadata, Back/background/foreground/orientation/process recreation/memory pressure, no-backup, permission/log redaction, accessibility.

### AŞAMA 19 — Malicious/corrupt input ve resource guards — `DONE`

Giriş kapısı: `AŞAMA 18 DONE ON MAIN`. PR #32 ile tamamlandı.

Magic/version preflight (DWG magic, DXF binary/ASCII, empty/truncated, foreign formats rejection), size/entity/depth/text/hatch/raster/XREF budgets, raster decompression bomb protection (15MP), NaN/Infinity/$10^{12}$ coordinate guards, block cycle detection, controlled error codes, bounded mutation/fuzz smoke (15 iterations), deterministic semantic snapshot (`schema=resource-guards/v1`).

### AŞAMA 20 — Ölçümlü performance/memory — `DONE`

Giriş kapısı: `AŞAMA 19 DONE ON MAIN`. PR #33 ile tamamlandı.

Physical Android Release TTFUP/frame p50/p95/PSS/GC/artifact size; small/medium/large corpus; yalnız profiler/A-B evidence ile optimization.

### AŞAMA 21 — Android full corpus regression / beta gate — `DONE`

Giriş kapısı: `AŞAMA 20 DONE ON MAIN`.

Full public/private corpus parse/scene/render/golden, P0/P1 matrix (P0 8/8 C3/C4 %100, P1 4/4 C3 %100, negatives 2/2 C2, C3+ %85.7), harita/kadastro 5.000.000 + 0.001 çift duyarlık korunumu, Debug vs Release / trimming / AOT analizi, APK boyutu (39.8 MB < 45 MB), Dumpsys PSS (134.1 MB < 250 MB), deterministik snapshot (`schema=corpus-regression/v1`) ve API 36 emülatör kabul testi ile beta gate onayı (`ANDROID_STAGE21_BETA_GATE_PASS`). Evidence: `docs/evidence/STAGE_21.md`.

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
| Emulator fazla yorumlanır | real app/process/artifact + expected-content marker olmadan renderer PASS yok |
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
