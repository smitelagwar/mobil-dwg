# Mobil DWG/DXF Görüntüleyici — Nihai Uygulama ve Yürütme Planı

**Plan sürümü:** 1.3  
**Son checkpoint:** 25 Ağustos 2026  
**Aktif ürün:** Android-only, local/offline, read-only 2D DWG/DXF viewer  
**iOS:** future option; aktif Android DoD ve sıranın dışında  
**Ürün ilkesi:** preview-first; original CAD immutable; dependency/artifact provenance kanıtlanmadan release yok.

> “Royalty-free”, hukuki garanti veya pazarlama sloganı değil; her release'in gerçek dependency, native asset ve dağıtım artifact'i üzerinde yeniden kanıtlanan teknik/politika kriteridir.

---

## 1. Tek yetkili yürütme checkpoint'i

```text
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_STAGE: V04 — Mimari ve gerçek Android uygulama kabuğu
CURRENT_SUBSTEP: V04.ready
STATUS: NOT_STARTED
LAST_IMPLEMENTED_STAGE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
LAST_ANDROID_VALIDATION_EVIDENCE: docs/evidence/android-validation/V03.md
LAST_V03_TESTED_HEAD: 69e4e842b5426d71453f5f69a01ebba5948d6b9c
LAST_V03_TESTED_PR_MERGE_REVISION: 1171807016e2deacc4f575b7980400b4f8b4708c
LAST_V03_RUN_JOB: 32827625875 / 97739039060
LAST_V03_ARTIFACT: 9555501552; sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a
PENDING_EMULATOR_QUEUE: EMPTY
BLOCKERS: Aktif V04 blocker'ı yok; fiziksel Android release öncesi ayrıca zorunlu; iOS aktif kapsam dışı.
NEXT_ACTION: Yalnız V04'ü başlat — gerçek installable Android MobilDwg.App shell + mimari/emulator gate; aynı turda V05'e geçme.
```

### `devam` protokolü

1. Gerçek `main` HEAD, açık PR, checkpoint ve kullanıcı değişiklikleri doğrulanır.
2. `ANDROID_DOGRULAMA_PLANI.md` V01–V09 bitmediyse açık VXX birinci cursor'dır.
3. Implementation cursor AŞAMA 10'da validation cursor'dan ayrı korunur.
4. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; sonraki aşama aynı turda başlatılmaz.
5. Test/evidence olmadan PASS/DONE yoktur.
6. Emulator fiziksel cihaz değildir; Stage01Smoke real viewer değildir; queued/zero-step workflow PASS değildir.
7. Dependency kendiliğinden yükseltilmez. Version, license, hash, transitive/native graph tekrar kanıtlanır.
8. Destructive/force Git işlemi yapılmaz; kullanıcı değişiklikleri korunur.
9. Her kapanışta validation planı, evidence, `DEVAM.md`, `gecmis.md`, execution log ve bu checkpoint güncellenir.
10. Kullanıcı iOS'u yeniden etkinleştirmedikçe iOS build/spike/signing işi yapılmaz.

---

## 2. Değiştirilemez ürün şartları

Kullanıcı açıkça değiştirmedikçe:

- v1 yalnız **2D viewer**; editor/writer değildir.
- Aktif teslim hedefi yalnız **Android**.
- `.dwg` ve `.dxf` doğrudan cihazda/local akışta okunur; zorunlu cloud conversion yok.
- Temel açma/render local/offline; hesap/login/server zorunlu değil.
- Core viewer kullanıcı için ücretsiz; per-file/per-user/runtime CAD SDK ücreti yok.
- Autodesk RealDWG, ticari ODA SDK, Autodesk cloud conversion veya proprietary/trial CAD SDK kullanılmaz.
- Runtime'da GPL/AGPL/SSPL/BUSL/non-commercial/proprietary/unknown lisans varsayılan NO-GO.
- Default allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD; exact bileşen yine ayrı denetlenir.
- Original drawing değiştirilmez; v1 save/overwrite/save-as yok.
- Unsupported/proxy/font/XREF/raster sessiz kaybolmaz; compatibility raporuna düşer.
- UI parser entity'lerine doğrudan bağlanmaz.
- World/document koordinatları `double`; precision düşüşü tek test edilmiş screen boundary'sinde yapılır.
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

- .NET SDK/workload set: `10.0.400`.
- Android min API `24`, target/compile API `36`.
- OpenJDK baseline `21.0.12`, Build-Tools `36.0.0`, Platform-Tools/ADB `37.0.1`.
- ACadSharp `3.7.1`: read-only parser baseline `GO` — ADR 0001; render fidelity garantisi değil.
- SkiaSharp `4.151.1`: renderer dependency; Android native inventory V02'de doğrulandı.
- Exact unpatched ProCad source reuse: `NO-GO` — ADR 0002; survey-origin `5,000,000 + 0.001` precision blocker.
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
- App composition katmanıdır; platform adapter'ları burada.
- `RenderScene` derived/rebuildable'dır; original document değildir.
- CadSession/document handle tek owner tarafından deterministic dispose edilir.
- Safe-open source path varsaymaz; stream/content URI/local private copy kontratı kullanır.
- ProCad production ProjectReference/PackageReference/native graph'a girmez.
- Future iOS dönüşü shared Core/Cad/Rendering katmanlarını fork etmeden adapter sınırından yapılabilmelidir.

---

## 5. Fidelity ve compatibility sözleşmesi

“Dosya açıldı” tek başarı metriği değildir:

1. **Parse success** — controlled document oluşturuldu.
2. **Scene success** — beklenen entity semantiği render scene'e aktarıldı.
3. **Render success** — frame crash/NaN/sonsuz döngü olmadan çizildi.
4. **Engineering fidelity** — position/scale/text/block/dimension/visibility kabul sınırında.

Compatibility seviyesi:

- `C0`: desteklenmiyor; algılandı ve açık uyarı.
- `C1`: parse; render doğrulanmadı.
- `C2`: yaklaşık/substituted; teknik fidelity garantisi yok ve uyarı var.
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
- Committed sentetik CAD `fixtures/public/` altında ve explicit rights profile ile olmalı.
- Private/customer drawing `fixtures/private/` veya izinli external private corpus'ta; Git dışında.
- CAD committed hash evidence working-tree line endingsine değil `HEAD:<path>` Git blob bytes'a dayanır.
- `.gitattributes`: `*.dwg binary`, `*.dxf -text`.
- Android V04–V09 için küçük hak durumu açık smoke set:
  - committed 0BSD DXF `synthetic-turkish-basic-ac1015`;
  - exact ACadSharp 3.7.1 generator ile validation-time üretilen AC1015 DWG `synthetic-turkish-basic-ac1015-dwg`;
  - missing-font/missing-XREF negatif DXF'ler.
- Generated DWG magic + DwgReader read-back zorunlu; run-specific hash evidence tutulur.
- Generated writer/read-back round-trip bağımsız engineering-fidelity golden değildir.
- Image golden yalnız deterministic viewport/theme/font + açık redistribution evidence ile commit edilir.

---

## 7. Dependency / license / artifact firewall

Her runtime dependency/asset için:

- exact version/resolved graph
- source repo/tag/commit
- nupkg/source hash
- license/hash
- transitive/native entries
- submodule/fork diff
- font/icon/PAT/fixture provenance
- redistribution/notice/source disclosure/royalty sonucu
- final artifact'te olup olmadığı

Zorunlu kurallar:

- strict exact direct version + Central Package Management + lockfile + locked restore
- `*`, latest, floating, open lower-bound direct dependency yok
- unknown license/native binary/asset = release blocker
- rejected dependency'nin kaynak kodu production/test vektörü olarak kopyalanmaz
- proprietary AutoCAD SHX/font bundle edilmez
- Android RC'de APK/AAB extraction + SBOM + notices + compliance snapshot zorunlu

V02 authoritative resolved Android probe graph: ACadSharp 3.7.1, SkiaSharp 4.151.1, SkiaSharp.NativeAssets.Android 4.151.1. ProCad/iOS-only leakage yok.

---

## 8. Android geriye dönük validation programı

Yetkili ayrıntı: `ANDROID_DOGRULAMA_PLANI.md`.

- V01 `VALIDATED`: toolchain/self-hosted/emulator/Stage01Smoke infrastructure; real viewer değil.
- V02 `VALIDATED`: dependency/lockfile/license/hash/vulnerability/Android-native boundary.
- V03 `VALIDATED`: fixture/provenance/golden/redistributable Android smoke-set/device matrix.
- **V04 `NOT_STARTED`: real installable MobilDwg.App + architecture/emulator gate.**
- V05: parser in real Android app.
- V06: FilePicker/SAF + safe-open in real app.
- V07: ProCad NO-GO/precision/production isolation.
- V08: iOS historical archive + Android graph isolation.
- V09: RenderScene/camera/diagnostics revalidation.

V03 authoritative run/job `32827625875` / `97739039060`; artifact `9555501552`.

---

## 9. Implementation aşamaları

AŞAMA 00–09 tarihsel implementation evidence `docs/evidence/STAGE_XX.md` ve ADR'lerde korunur. Implementation cursor AŞAMA 10'dadır; V01–V09 tamamlanmadan erken renderer işine atlanmaz.

### AŞAMA 10 — P0 temel geometri renderer'ı — `NOT_STARTED`

Amaç: sade Skia baseline ile temel 2D geometri.

- LINE/ARC/CIRCLE/ELLIPSE/POINT.
- LW/POLYLINE + bulge; SPLINE controlled tessellation.
- SOLID/TRACE/3DFACE 2D vertex order.
- OCS/extrusion/mirror/large-coordinate fixture.
- draw order, clipping, antialias baseline.
- GPU/batching/tiling yok; önce correctness.

Test: T1 + küçük semantic/golden diff.  
Çıkış: P0 basic fixture C3; invalid geometry controlled warning.

### AŞAMA 11 — Mobil viewport ve gesture

- pan, pinch zoom, fit extents, gerekirse double-tap fit
- pinch focal point preservation
- min/max zoom/overscroll guards
- portrait/landscape/safe area
- rotation reparse yapmaz
- debug-only frame timing

Çıkış: küçük/orta fixture navigation stabil; gerçek Android frame baseline kaydedilir.

### AŞAMA 12 — Block/INSERT/attribute

- translation/rotation/scale/mirror matrix order
- nested block/non-uniform scale
- ATTRIB/ATTDEF placement + stable identity
- Layer 0 + ByBlock/ByLayer parent context
- cycle/depth/expanded-instance guards
- measured shared geometry reuse

Çıkış: block fixture C3; attribute kaybı yok; cycle controlled warning.

### AŞAMA 13 — Layer, color, linetype, lineweight

- layer on/off/frozen
- ACI/true-color/ACI7 light-dark
- centralized ByLayer/ByBlock/Layer0 resolver
- basic linetype + scale/LTSCALE
- lineweight toggle ve screen/plot semantic ayrımı
- complex shape/text linetype unsupported/substituted warning

Çıkış: style fixture C3; layer toggle reparse yapmaz.

### AŞAMA 14 — TEXT/MTEXT, Türkçe, font, SHX

- header/codepage; CP1254 + Unicode
- TEXT height/width/rotation/alignment/justification/mirror
- MTEXT stateful minimum parser; nested formatting regex ile kör silinmez
- font resolver exact → audited mapping → system fallback
- substitution compatibility raporuna düşer
- SHX capability izole spike; full interpreter son çare
- bundled font exact license/hash/notice

Çıkış: Türkçe bozulmaz; missing font sessiz değil; P0 text C3.

### AŞAMA 15 — Dimension, leader, hatch

- önce existing `*D` anonymous dimension block render
- linear/aligned/angular/radius/diameter + override/arrows/scale
- leader/MLEADER/tolerance ayrı support seviyesi
- solid/pattern/dense hatch + island + broken boundary
- profiler olmadan heavy triangulation yok
- yanlış dimension yerine C0/C2 warning

Çıkış: release dimension fixture C4; hatch P0 C3.

### AŞAMA 16 — Model space, layout, paper space, viewport

- model/layout selector + active metadata
- paper-space entity + viewport clip/center/height/twist
- viewport layer override/freeze
- layout change reparse yapmaz
- corpus'ta teknik anlam için gerekli layout blocker olarak ele alınır

Çıkış: standard layout C3 veya açık compatibility warning.

### AŞAMA 17 — XREF/raster/underlay ve compatibility raporu

- XREF/path detection; remote auto-download yok
- default: detect + missing warning
- optional directory mapping yalnız explicit user grant + traversal/cycle/depth/byte guards
- raster default detect/warn; render ancak bounded decode evidence ile
- PDF/advanced underlay v1'de C0 olabilir
- user-facing compatibility summary

Çıkış: missing external kaynak ana drawing'i çökertmez/sessiz gizlemez.

### AŞAMA 18 — Tam Android viewer UX/lifecycle

- home/open/loading/viewer/layers/fit/file-info/warnings/close
- recent file güvenli URI/grant metadata; cache path persistent source değildir
- Android Back/background/foreground/orientation/process recreation/memory pressure
- rotation reparse yapmaz; deterministic dispose/cache cleanup
- backup exclusion; sensitive drawing/thumb no-backup
- core viewer için gereksiz INTERNET permission yok
- path/filename/drawing text log redaction
- light/dark/tablet/touch accessibility

Çıkış: günlük open-inspect-close akışı gerçek cihazda stabil.

### AŞAMA 19 — Malicious/corrupt input ve resource guards

- extension + magic/version + bounded preflight
- file/entity/block depth/instance/scene/hatch/text/raster/XREF budgets
- NaN/Infinity/extreme extents/cycle guards
- corrupt/truncated/oversized controlled error taxonomy
- cooperative cancellation yoksa dürüst limitation
- targeted bounded fuzz/mutation smoke

Çıkış: bilinen kötü input crash/ANR yerine controlled error/ret.

### AŞAMA 20 — Ölçümlü performance/memory

- Android Release physical device TTFUP/frame p50/p95/managed/native/PSS/GC/artifact size
- small/medium/large corpus + 5 repeat-open
- profiler ile tek dominant bottleneck seçilir
- culling/cache/LOD/spatial index/GPU/local-origin yalnız A/B evidence ile
- final resource budgets cihaz bazında sabitlenir
- `largeHeap` son çare ve ADR ister

Çıkış: `PERFORMANCE.md` ölçüm/eşik/cihaz kanıtı içerir.

### AŞAMA 21 — Android full corpus regression / beta gate

- full public/private corpus parse/scene/render/golden
- P0/P1 compatibility matrix
- ana physical Android, mümkünse ikinci cihaz/tablet
- Debug/Release/trimming/AOT/artifact size farkı
- P0 blocker yok; C0/C2 limitations kullanıcı metnine yansır

Çıkış: Android beta build hazır.

### AŞAMA 22 — Android Release/AAB/compliance RC

- final app name/package/icon/versioning
- live-verify target SDK/Play/Data Safety/privacy policy
- accessibility + OSS licenses screen
- secure signing; secret repo/chat/log'a girmez
- signed APK+AAB + physical smoke
- backup/permission audit
- artifact DLL/SO/JAR/font/asset inventory
- SBOM + THIRD_PARTY_NOTICES + compliance snapshot
- trademark/store wording review

Çıkış: installable Android RC; compliance GREEN; unknown artifact yok.

### AŞAMA 23–24 — Future iOS track

`DEFERRED_FUTURE_IOS / ACTIVE_ANDROID_SEQUENCE_OUT`.

Kullanıcı açıkça yeniden etkinleştirmeden Mac/Xcode/iPhone/iOS AOT/archive işi yapılmaz. Yeniden açılırsa Stage08 historical risks sıfır varsayımla değerlendirilir; gerçek iPhone olmadan PASS yok.

### AŞAMA 25 — Android beta ve yalnız blocker düzeltmeleri

- izinli gerçek kullanım
- report format: fixture hash, build, reproduce, compatibility report, expected/actual
- yalnız crash/privacy/P0 fidelity/open/lifecycle/severe perf blocker
- yeni feature/edit/export/XREF crawler eklenmez
- targeted test; milestone sonunda full corpus

### AŞAMA 26 — Dependency freeze / final audit / RC approval

- toolchain/dependency freeze; lockfile/resolved graph diff sıfır
- full corpus/lifecycle/perf/signed artifact smoke
- real APK/AAB inventory + SBOM/license/source/native/font/asset match
- unknown/rejected dependency, analytics/upload/debug endpoint/secret/proprietary asset scan
- store/privacy/target SDK/trademark live-verify
- release notes/compatibility/support final

Çıkış: Android RC GREEN; herhangi unknown = NO-GO.

### AŞAMA 27 — Android v1 artifact / yayın / handoff

- final APK/AAB + checksums + build instructions
- store account varsa submission; yoksa store-ready package/checklist
- clean machine/CI locked restore + build/test reproduction
- user-approved tag/release snapshot
- usage/privacy/compatibility/notices/known limitations/support docs
- plan checkpoint `DONE`

Çıkış: gerçek Android cihazda çalışan, exact source'dan yeniden üretilebilir, audited ücretsiz viewer v1.

---

## 10. Risk kaydı

| Risk | Zorunlu tepki |
|---|---|
| ACadSharp fixture fidelity farkı | pinned A/B + independent fixture; sistematikse parser gate yeniden açılır |
| ProCad precision/lineage risk | NO-GO korunur; upstream patch ancak yeni evidence/ADR ile |
| Renderer scope büyür | P0 bitir; P1/P2 warning ile ertelenebilir; edit/export ekleme |
| SHX/font farklılığı | visible substitution + audited fallback; proprietary bundle yok |
| OOM/ANR | controlled resource guard; profiler tabanlı culling/cache; largeHeap son çare |
| Corpus rights belirsiz | redistribution durur; private/remote-reference policy; evidence çözülmeden commit/bundle yok |
| Emulator fazla yorumlanır | real app/process/artifact marker olmadan viewer PASS yok |
| Self-hosted runner offline | exact SHA queue; aynı işi spamleme; kanıtsız PASS yok |
| Unknown native/transitive asset | release NO-GO |
| Dependency terk edilir | pinned source archive + adapter sayesinde kontrollü fork/alternative spike |
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
