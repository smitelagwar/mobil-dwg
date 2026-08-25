# Mobil DWG/DXF Görüntüleyici — Nihai Uygulama ve Yürütme Planı

**Plan sürümü:** 1.2
**Son checkpoint güncellemesi:** 25 Ağustos 2026
**Ürün yönü:** Aktif hedef Android-only; iOS future option; preview-first; local/offline
**Hedef:** Kullanıcıya ücretsiz sunulabilen; CAD dosyası/kullanıcı/runtime başına ticari CAD SDK/API royalty’si gerektirmeyen; Android üzerinde 2D DWG/DXF dosyalarını güvenli ve teknik olarak güvenilir biçimde görüntüleyen çalışan mobil uygulama.

> Bu belge fikir listesi değil yürütme sırasıdır. “Royalty-free” hukuki garanti değil; her release’in gerçek dependency ve dağıtım artifact’leri üzerinde yeniden kanıtlanan teknik/politika kriteridir.

---

## 1. Yürütme durumu — tek yetkili checkpoint

```text
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_STAGE: V03 — Fixture, golden sözleşmesi ve Android test matrisi
CURRENT_SUBSTEP: V03.ready
STATUS: NOT_STARTED
LAST_IMPLEMENTED_STAGE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED; Android validation cursor'ından ayrı korunur
LAST_VERIFIED_REVISION: 549770192c181b30db8968cec5c6ac3c2407e133 — V02 authoritative PR merge test revision; sonraki V02 kapanış commitleri evidence/checkpoint niteliğindedir
LAST_HISTORICAL_EVIDENCE: docs/evidence/STAGE_09.md; run 32815175055; artifact 9551137293; PR #12 merge 0a2dd886bbe59698a6d2eb4c99f66e7f9270063a
LAST_ANDROID_VALIDATION_EVIDENCE: docs/evidence/android-validation/V02.md; run 32824397251; job 97729154385; artifact 9554326162
ACTIVE_PLAN: ANDROID_DOGRULAMA_PLANI.md
PENDING_EMULATOR_QUEUE: EMPTY
BLOCKERS: Aktif V03 blocker'ı yok. Fiziksel Android farkları release öncesi açık kalır. iOS aktif kapsam dışıdır ve Android'i bloke etmez.
NEXT_ACTION: Yalnız V03'ü başlat; fixture/golden/provenance/private-ignore ve Android test matrisi sözleşmesini doğrula; aynı turda V04'e geçme.
LAST_UPDATE: 2026-08-25
```

Durum değerleri: `NOT_STARTED`, `IN_PROGRESS`, `BLOCKED`, `DONE`. Android validation alt planında ek olarak `CODE_AUDIT`, `FIX_REQUIRED`, `READY_FOR_EMULATOR`, `WAITING_RUNNER`, `VALIDATED`, `VALIDATED_WITH_DEFERRED_PHYSICAL`, `SCOPE_ARCHIVED`, `DEFERRED_PHYSICAL_ANDROID` kullanılabilir.

### `devam` protokolü

1. Önce gerçek `main` HEAD, açık PR, checkpoint ve kullanıcı değişiklikleri doğrulanır.
2. `ANDROID_DOGRULAMA_PLANI.md` V01–V09 programı bitmediyse açık VXX birinci cursor’dır.
3. Implementation cursor AŞAMA 10’da ayrı tutulur; validation beklerken yalnız güvenli host-independent iş yapılabilir.
4. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; sonraki aşama aynı turda başlatılmaz.
5. Test/evidence olmadan PASS/DONE yazılmaz.
6. Emulator fiziksel cihaz değildir; geçici `Stage01Smoke` gerçek viewer değildir; queued veya zero-step workflow PASS değildir.
7. Dependency kendiliğinden yükseltilmez; her değişiklik license/hash/graph/native evidence ister.
8. Kullanıcı değişiklikleri korunur; destructive/force Git işlemi yapılmaz.
9. Her turun sonunda checkpoint/evidence/execution log/handoff kayıtları güncellenir.
10. Kullanıcı iOS’u yeniden etkinleştirmedikçe iOS build/spike/signing işi yapılmaz.

### Aktif kapsam ve dış kapılar

Aktif ürün Android-only v1’dir. AŞAMA 01–09 implementation geçmişi `ANDROID_DOGRULAMA_PLANI.md` V01–V09 programıyla yeniden doğrulanır. Fiziksel Android’in SAF/performance/üretici farkları emulator PASS’iyle kapatılmaz; release/beta matrisinde yeniden zorunludur. iOS geçmiş evidence ve taşınabilir mimari korunur fakat aktif Android DoD değildir.

---

## 2. Değiştirilemez ürün şartları

Kullanıcı açıkça değiştirmedikçe:

- v1 bir **2D viewer**; editor/writer değildir.
- Aktif v1 teslim hedefi yalnız Android’dir.
- DWG/DXF doğrudan cihazda okunur; zorunlu bulut veya DWG→DXF dönüşümü yoktur.
- Temel açma/render akışı local/offline’dır; hesap/giriş/sunucu gerekmez.
- v1 kullanıcı için ücretsizdir; core CAD özelliği paywall arkasında değildir.
- Autodesk RealDWG, APS/Forge dönüşümü, ticari ODA SDK, ücretli/trial parser-renderer kullanılmaz.
- Runtime’da GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/lisansı belirsiz bileşen yoktur.
- Varsayılan allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD. LGPL/MPL proje politikası gereği varsayılan `RED`; istisna yazılı karar ister.
- Root lisans tek başına yeterli değildir; transitive package/native binary/submodule/vendored code/font/PAT/icon/fixture de denetlenir.
- Original CAD immutable; v1’de save/overwrite yoktur.
- Unsupported/proxy entity, eksik font, XREF/raster sessizce kaybolmaz; compatibility raporuna düşer.
- Uygulama adı/ikonu Autodesk/AutoCAD/DWG markasını ürün markası gibi kullanmaz.
- Google Play hesabı/cihaz/bakım maliyeti “ücretsiz CAD teknolojisi” kapsamı dışındadır.

### v1 zorunlu kapsam

- Yerel `.dwg` ve `.dxf` açma.
- Emulator ile sürekli smoke; beta/release için gerçek Android cihaz.
- Model space; corpus gerektiriyorsa standart paper-space/layout/viewport.
- Pan, pinch zoom, fit extents, orientation.
- Layer listesi ve show/hide.
- Temel 2D geometri, block/INSERT, attribute, text/MTEXT, dimension, hatch.
- CAD renkleri, ByLayer/ByBlock, basit linetype ve lineweight.
- Türkçe text/encoding ve görünür font substitution.
- Loading/progress/cancel talebi, kontrollü hata ve compatibility raporu.
- Local/offline/privacy, close/reopen ve lifecycle güvenilirliği.

v1 dışı: edit/selection/OSNAP/ölçüm, DWG/DXF yazma, PDF/SVG export, cloud/collaboration/account, tam XREF crawler, proprietary proxy tam fidelity, 3D CAD/BIM.

---

## 3. Teknoloji kararları ve gerçeklik

- .NET `10.0.400` / MAUI Android baseline pinlidir.
- ACadSharp `3.7.1` read-only parser baseline `GO`; entity sınıfının varlığı fidelity kanıtı değildir.
- SkiaSharp `4.151.1` renderer/native baseline’dır; native inventory ayrıca denetlenir.
- Exact unpatched ProCad source reuse ADR 0002 ile `NO-GO`; survey-origin `5,000,000 + 0.001` precision blocker kanıtlıdır.
- IxMilia.Dxf `0.8.4` yalnız test/fallback adayıdır; production graph’a varsayılan eklenmez.
- `Task.Run(..., token)` senkron parser’ı hard-cancel etmiş sayılmaz. Cooperative cancellation yoksa sonuç discard edilir ve capability dürüstçe `BeforeStartOnly` kalır.
- GPU/tiling/spatial index/largeHeap/GC hacks varsayılan çözüm değildir; yalnız ölçümlü A/B ile kabul edilir.

### V02 ile düzeltilen exact-version politikası

Tarihsel plain CPM sürümlerinin lockfile’da open lower-bound request oluşturduğu V02’de bulundu. Güncel production/test baseline strict exact NuGet range kullanır:

- ACadSharp `[3.7.1]`
- SkiaSharp `[4.151.1]`
- IxMilia.Dxf `[0.8.4]` — test/fallback only

Kalıcı `Stage 02 Dependency Audit` self-hosted Windows gate’i exact requested/resolved graph, locked restore, license/nupkg hash, vulnerability, production `src/` boundary ve Android native inventory’yi doğrular. Yetkili run `32824397251`, job `97729154385`, artifact `9554326162`. Ayrıntı `docs/evidence/android-validation/V02.md`.

---

## 4. Hedef mimari ve bağımlılık sınırları

```text
MAUI App / Android adapters
        │
        ▼
Application + CadSession
        │
        ├── ICadDocumentReader ──► ACadSharp adapter
        │                            └── parse diagnostics
        ▼
Read-only document/session
        │
        ▼
IRenderSceneBuilder ──► immutable RenderScene + CompatibilityReport
        │
        ▼
ICadRenderer ──► Skia renderer ──► Android canvas
                                  └── future iOS adapter boundary
```

Kurallar:

- UI doğrudan ACadSharp/ProCad entity’lerine bağlanmaz.
- Parser document/session tek owner tarafından yönetilir.
- Dosya açma çıplak path’e güvenmez; URI/stream/local-copy handle + generation ID + cancellation talebi kullanır.
- `RenderScene` türetilmiş/reproducible’dır; document’ın yerine geçmez.
- World/document koordinatları `double`; float dönüşüm ancak tek test edilmiş screen boundary’de olur.
- Entity identity/handle korunur; v1 edit kodu taşımaz.
- Compatibility report parse→scene→render kayıp/substitution’ı toplar.
- Production tek scene/renderer yoludur; spike code runtime graph’a sızmaz.
- ProCad Editing/Scripting/Collaboration, ACadSharp writer ve export paketleri v1 artifact’ine girmez.

Mevcut repo production katmanları: `src/MobilDwg.Core`, `src/MobilDwg.Cad`, `src/MobilDwg.Rendering`, `src/MobilDwg.App`. V04’e kadar `MobilDwg.App` installable MAUI Android app değildir; class-library baseline’dır.

---

## 5. Fidelity, compatibility ve performans sözleşmesi

### Dört ayrı başarı durumu

1. Parse success.
2. Scene success.
3. Render success.
4. Engineering fidelity.

Compatibility seviyeleri:

| Seviye | Anlamı |
|---|---|
| C0 | Desteklenmiyor; algılandı ve uyarıldı |
| C1 | Parse edildi; render doğrulanmadı |
| C2 | Yaklaşık render; teknik fidelity garantisi yok ve uyarı var |
| C3 | Golden/semantic testle kabul edildi |
| C4 | Mühendislik-kritik fixture’da ayrıca doğrulandı |

P0 release entity’leri en az C3; dimension/teknik annotation kritik fixture’ları C4 olmalıdır. Yanlış yaklaşık dimension yerine açık warning tercih edilir.

### P0 entity kapsamı

LINE, ARC, CIRCLE, ELLIPSE, LWPOLYLINE/POLYLINE + bulge, SPLINE, POINT, SOLID, TRACE, 3DFACE 2D görünümü, TEXT, MTEXT, INSERT/nested INSERT/ATTRIB/ATTDEF, DIMENSION, HATCH.

P1: LEADER/MLEADER/TOLERANCE, XLINE/RAY/MLINE, TABLE, VIEWPORT/layout, IMAGE/WIPEOUT, basit XREF tespiti. P2 proprietary proxy/ileri 3D/underlay/dynamic-block davranışı; v1’i bloklamaz ama raporlanır.

### Zorunlu doğruluk fixture’ları

OCS/WCS, büyük/negatif koordinatlar, bulge işaretleri, nested rotate/mirror/non-uniform block, Layer0/ByLayer/ByBlock/ACI7/true-color, linetype/lineweight, CP1254+Unicode Turkish text, SHX missing/substitution, dimension familyaları, solid/pattern hatch, SOLID vertex order, layout/viewport, missing XREF/raster/proxy warnings.

### Performans ilkeleri

Dosya byte tek metrik değildir; parsed entity, expanded instance, scene primitive, glyph/hatch complexity, raster pixel ve managed/native/PSS birlikte kaydedilir. Debug sonucu release kararı değildir. `[MEASURE]` optimizasyonları baseline→profiler→tek A/B spike→aynı corpus→correctness/bellek/size regression yok→ölçülebilir kazanç sırasını izler. `largeHeap` son çaredir.

---

## 6. Lisans, kaynak ve veri firewall’u

Her runtime dependency/asset için exact version, resolved transitive graph, source URL+commit/tag, package/source hash, license+hash, submodule/fork diff, native inventory, font/icon/PAT/fixture provenance, redistribution/notice/royalty değerlendirmesi ve artifact inclusion kaydı tutulur.

Kurallar:

- Central Package Management + strict exact NuGet range + `packages.lock.json` + locked restore.
- `*`, `latest`, floating veya direct open-lower-bound production dependency yok.
- Unknown license/native binary/asset release blocker.
- Rejected lisanslı kaynaktan kod/test vektörü/satır satır port alınmaz.
- Açık internetteki DWG/DXF/font/screenshot yeniden dağıtılabilir varsayılmaz.
- Müşteri çizimleri private/ignored corpus’ta kalır; hassas path/text loglanmaz.
- Proprietary SHX bundle edilmez; kullanıcı font importu local/app-private olabilir.
- Android RC’de gerçek APK/AAB extract edilip source/license evidence ile karşılaştırılır.
- Release için SBOM + THIRD_PARTY_NOTICES + immutable compliance snapshot gerekir.

---

## 7. Aşamalı uygulama planı

İki cursor vardır: Android validation V01–V09 ve implementation AŞAMA 10+. V01–V09 bitmeden normal cursor AŞAMA 10’dan ilerletilmez; runner beklenirken yalnız güvenli host-independent iş kuralı istisnadır.

### Aşama indeksi

- [x] AŞAMA 00 — Çalışma alanı/yürütme zemini — `DONE`
- [ ] AŞAMA 01 — Toolchain + fiziksel Android — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 02 — Dependency/lisans/lock — `DONE`; Android V02 revalidation ile exact-pin policy sertleştirildi
- [x] AŞAMA 03 — Corpus/golden/matris — `DONE`
- [x] AŞAMA 04 — Minimal solution/mimari — `DONE`
- [x] AŞAMA 05 — ACadSharp parser — `DONE`
- [ ] AŞAMA 06 — Safe-open fiziksel Android kapısı — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 07 — ProCad source spike — `DONE / NO-GO`
- [x] AŞAMA 08 — iOS characterization — `DONE / HISTORICAL; iOS PASS NOT CLAIMED`
- [x] AŞAMA 09 — RenderScene/kamera/diagnostics — `DONE`
- [ ] AŞAMA 10 — P0 temel geometri renderer — `NOT_STARTED`
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
- [ ] AŞAMA 23–24 — `DEFERRED_FUTURE_IOS / ACTIVE_ANDROID_SEQUENCE_OUT`
- [ ] AŞAMA 25 — Android beta ve blocker düzeltmeleri
- [ ] AŞAMA 26 — Android dependency freeze/final audit/RC
- [ ] AŞAMA 27 — Android v1 artifact/yayın-handoff/kapanış

### Tarihsel AŞAMA 00–09

Detaylı execution/evidence yeniden bu plan içine kopyalanmaz. Yetkili kaynaklar `docs/evidence/STAGE_01.md`–`STAGE_09.md`, `docs/EXECUTION_LOG.md`, `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`, `docs/ADR/0002-procad-pinned-source-no-go.md` ve Android revalidation için `docs/evidence/android-validation/` dizinidir.

Özet gerçeklik: ACadSharp read-only parser `GO`; ProCad exact unpatched reuse `NO-GO`; safe-open host implementation tamam ama fiziksel SAF/lifecycle gate açık; iOS yalnız historical characterization; RenderScene foundation gerçek self-hosted T0/T1 evidence ile tamamlandı. V01 emulator/toolchain altyapısı `INFRASTRUCTURE_SMOKE_ONLY` olarak doğrulandı; V02 dependency/native boundary doğrulandı. Aktif sonraki validation V03’tür.

### AŞAMA 10 — P0 temel geometri renderer’ı

**Amaç:** Temel 2D geometriyi doğru, sade Skia baseline ile çizmek.

- [ ] LINE/ARC/CIRCLE/ELLIPSE/POINT.
- [ ] LW/POLYLINE + bulge; SPLINE için pinned parser verisini doğrulayan tessellation.
- [ ] SOLID/TRACE/3DFACE 2D görünümü ve vertex order.
- [ ] OCS/extrusion, mirror ve büyük koordinat fixture’ları.
- [ ] Draw order, clipping ve antialias baseline.
- [ ] Batching/GPU/tiling yok; önce correctness ve baseline.

Test: T1 + küçük golden/semantic diff.  
Çıkış: P0 basic fixture C3; invalid geometry controlled warning.

### AŞAMA 11 — Mobil viewport ve gesture’lar

- [ ] Pan, pinch zoom, fit extents; finger focal-point korunur.
- [ ] Min/max zoom/overscroll guard.
- [ ] Portrait/landscape/safe area; rotation reparse etmez.
- [ ] Gesture sırasında geçici düşük kalite kalıcı detail kaybı yaratmaz.
- [ ] Frame timing yalnız debug diagnostics.

Test: gesture unit + gerçek telefon smoke/frame baseline.  
Çıkış: küçük/orta fixture navigation stabil.

### AŞAMA 12 — Block/INSERT/attribute dönüşümleri

- [ ] Translation/rotation/scale/mirror matrix order.
- [ ] Nested block + non-uniform scale.
- [ ] ATTRIB/ATTDEF placement + stable identity.
- [ ] Layer0/ByBlock/ByLayer parent context.
- [ ] Cycle/depth/expanded-instance guard.
- [ ] Ölçülü shared geometry; ağır instance kopyası yok.

Test: transform conformance + nested golden.  
Çıkış: block fixture C3; cycle warning.

### AŞAMA 13 — Layer, renk, linetype ve lineweight

- [ ] Layer on/off/frozen.
- [ ] ACI/true-color/ACI7 light-dark.
- [ ] ByLayer/ByBlock/Layer0 resolver tek merkez.
- [ ] Basit linetype + entity scale + LTSCALE.
- [ ] Lineweight screen/plot semantiği ayrılır.
- [ ] Complex shape/text linetype C0/C2 raporu.

Test: style resolver unit + golden.  
Çıkış: style fixture C3; layer toggle reparse etmez.

### AŞAMA 14 — TEXT/MTEXT, Türkçe, font ve SHX

- [ ] Codepage; CP1254 + Unicode.
- [ ] TEXT height/width/rotation/alignment/justification/mirror.
- [ ] MTEXT stateful tokenizer/minimum formatting; nested formatting regex ile kör silinmez.
- [ ] Font resolver exact→audited mapping→system fallback.
- [ ] Substitution compatibility/UI’a düşer; extents etkisi test edilir.
- [ ] SHX capability izole fixture ile ölçülür; full custom interpreter son çare.
- [ ] Bundle font exact license/hash/notice; kullanıcı importu app-private.

Test: Turkish/SHX/text metrics semantic + golden.  
Çıkış: Turkish text bozulmaz; missing font sessiz değildir; P0 text C3.

### AŞAMA 15 — Dimension, leader ve hatch doğruluğu

- [ ] DIMENSION’da önce mevcut `*D` anonymous block yolu.
- [ ] Linear/aligned/angular/radius/diameter + override/arrows/scale fixture.
- [ ] LEADER/MLEADER/TOLERANCE ayrı support seviyesi.
- [ ] Solid/pattern/dense hatch, island, clipping, corrupt boundary.
- [ ] Hatch LOD/triangulation yalnız profiler/fidelity A/B sonrası.
- [ ] Yanlış dimension yerine C0/C2 warning.

Test: T3 engineering mini corpus + human golden/semantic review.  
Çıkış: release dimension C4; hatch P0 C3.

### AŞAMA 16 — Model space, layout, paper space ve viewport

- [ ] Model/layout selector + active layout metadata.
- [ ] Paper-space, clip, view-center/height/twist transform.
- [ ] Viewport-specific layer override/freeze.
- [ ] Layout change reparse etmez.
- [ ] Corpus için teknik olarak zorunlu layout support release blocker olabilir.

Test: layout semantic/golden + gerçek Android navigation.  
Çıkış: standart layout C3.

### AŞAMA 17 — XREF/raster/underlay ve compatibility raporu

- [ ] XREF adı/yolu tespit; remote URL auto-download yok.
- [ ] Varsayılan tespit + missing warning; explicit folder grant olmadan sibling crawler yok.
- [ ] Opsiyonel folder mapping explicit grant + canonical path + cycle/depth/byte guard.
- [ ] Raster varsayılan tespit+warning; render ancak E3 + traversal/pixel/decode budget kanıtıyla.
- [ ] PDF/ileri underlay v1’de render edilmez; C0 warning.
- [ ] Compatibility ekranı parse/scene/render/font/XREF/raster/proxy özetini sunar.

Test: missing/present external + traversal negative.  
Çıkış: dış kaynak problemi crash/sessiz kayıp değildir.

### AŞAMA 18 — Tam Android viewer UX ve lifecycle

- [ ] Home/Open/loading/viewer/layer/fit/file-info/warnings/close.
- [ ] Recent files güvenli metadata/URI grant politikası; cache path kalıcı kaynak değildir.
- [ ] Back/foreground/background/orientation/process recreation/memory pressure.
- [ ] Session/camera korunur; rotation reparse etmez; deterministic dispose/cache cleanup.
- [ ] Safe recovery marker; imported CAD/thumbnail/scene backup exclusion.
- [ ] Gereksiz `INTERNET` permission yok; log path/filename/drawing text redacted.
- [ ] Open-With/share dar intent spike.
- [ ] Light/dark/tablet/safe-area/accessibility.

Test: gerçek telefon lifecycle matrix + T2.  
Çıkış: günlük aç/incele/kapat akışı stabil.

### AŞAMA 19 — Kötü niyetli/bozuk dosya ve resource guard’ları

- [ ] Extension + magic/version + bounded preflight.
- [ ] File/entity/block/scene/hatch/text/raster/XREF budget.
- [ ] NaN/Infinity/extreme extents/recursion guard.
- [ ] Corrupt/truncated/oversized controlled error taxonomy.
- [ ] Cancellation limitation dürüstçe belgelenir.
- [ ] Redacted diagnostics export.
- [ ] Küçük bounded mutation/fuzz smoke.

Test: negative corpus + guard unit.  
Çıkış: bilinen kötü input crash/ANR yerine kontrollü sonuç.

### AŞAMA 20 — Ölçümlü performans ve bellek optimizasyonu

- [ ] Android Release fiziksel cihaz TTFUP/frame p50-p95/PSS/native/managed/GC/artifact baseline.
- [ ] Küçük/orta/büyük corpus + 5 repeat-open.
- [ ] Profiler ile tek en büyük bottleneck; tek optimizasyon A/B.
- [ ] Gerektikçe culling→shared cache→LOD→spatial index/GPU.
- [ ] Precision için local-origin yalnız ölçümle.
- [ ] Cache shedding/final budgets; `largeHeap` yalnız ADR ile son çare.

Test: T3 benchmark + correctness diff.  
Çıkış: `PERFORMANCE.md` cihaz/eşik/ölçüm içerir.

### AŞAMA 21 — Android tam corpus regresyon ve beta kapısı

- [ ] Full private/public corpus parse/scene/render/golden.
- [ ] P0/P1 entity + DWG version compatibility matrix.
- [ ] Ana fiziksel telefon, mümkünse ikinci Android/tablet.
- [ ] Debug/Release/trimming/AOT/artifact-size kontrolü.
- [ ] Açık P0/P1 bug ve C0/C2 limitations sınıflandırılır.

Test: T4 Android.  
Çıkış: P0 blocker yok; beta build hazır.

### AŞAMA 22 — Android Release/AAB/compliance RC

- [ ] App name/package/icon/versioning kilidi.
- [ ] `[LIVE-VERIFY]` target SDK/Play/Data Safety/privacy/store requirements.
- [ ] Turkish/English UI + accessibility + privacy/about/open-source screens.
- [ ] Signing secret repo/log/chat’e girmez.
- [ ] Signed APK+AAB + gerçek cihaz smoke.
- [ ] Backup exclusion + permission audit.
- [ ] Artifact DLL/SO/JAR/font/asset inventory ↔ dependency evidence.
- [ ] SBOM + notices + compliance snapshot.
- [ ] Autodesk trademark/compatibility wording live-verify.

Test: signed Release smoke + artifact audit.  
Çıkış: installable Android RC; unknown artifact yok.

### AŞAMA 23–24 — Future iOS track

`DEFERRED_FUTURE_IOS / ACTIVE_ANDROID_SEQUENCE_OUT`. Kullanıcı iOS’u açıkça yeniden etkinleştirene kadar Android AŞAMA 25’i bloke etmez. Reactivation olursa gerçek Mac/iPhone/AOT/lifecycle/corpus/archive DoD ayrı plan revizyonuyla yeniden açılır; simulator gerçek iPhone PASS sayılmaz.

### AŞAMA 25 — Android beta ve yalnız blocker düzeltmeleri

- [ ] İzinli küçük beta grubunda gerçek dosyalar.
- [ ] Feedback: fixture hash/build/reproduce/compatibility/expected-actual.
- [ ] Yalnız crash/privacy/P0 fidelity/open/lifecycle/ciddi performance blocker.
- [ ] Yeni edit/export/XREF crawler yok.
- [ ] Her fix hedefli test; milestone sonunda full corpus.

Test: fix T1/T2; kapanış T4.  
Çıkış: açık release blocker yok.

### AŞAMA 26 — Android dependency freeze, final audit ve RC onayı

- [ ] Dependency/toolchain freeze; lock/resolved graph diff sıfır.
- [ ] Full corpus/lifecycle/performance/signed artifact smoke.
- [ ] APK/AAB inventory + SBOM + license/source/native/font/asset evidence.
- [ ] Unknown/rejected dependency, analytics/upload/debug endpoint/secret/proprietary asset aranır.
- [ ] Privacy/store/target SDK/marka guideline `[LIVE-VERIFY]`.
- [ ] Release notes/compatibility/privacy/support final.

Test: T4 final gate.  
Çıkış: Android RC `GREEN`; unknown = NO-GO.

### AŞAMA 27 — Android v1 artifact, yayın/handoff ve kapanış

- [ ] Final APK/AAB/checksum/build instructions.
- [ ] Store hesabı varsa submission; yoksa store-ready package/checklist ve açık blocker.
- [ ] Clean machine/CI locked restore+build+test.
- [ ] Version/tag/release snapshot kullanıcı onayıyla; otomatik push yok.
- [ ] Usage/privacy/compatibility/notices/known-limitations/support docs.
- [ ] Bu checkpoint `DONE`; Definition of Done tek tek kapanır.

Test: final install/open/close smoke + checksum.  
Çıkış: gerçek Android cihazda çalışan, exact kaynaklardan reproducible, denetlenmiş ücretsiz Android viewer v1.

---

## 8. Risk kaydı ve zorunlu tepki

| Risk | Tepki |
|---|---|
| ACadSharp fidelity kaybı | Sürüm A/B + fixture; warning; sistematikse parser gate yeniden açılır |
| ProCad lineage/precision/olgunluk | Production NO-GO korunur; upstream patch ancak ayrı evidence ile |
| Renderer efor büyümesi | P0 bitirilir, P1/P2 warning ile ertelenir; edit/export eklenmez |
| SHX/font sorunu | Görünür substitution + audited fallback; proprietary bundle yok |
| OOM/ANR | Guard/controlled reject + profiler-based optimization; largeHeap son çare |
| Corpus lisans/gizlilik | Dağıtım durdurulur; private/ignored provenance düzeltilir |
| Emulatorun fazla yorumlanması | Gerçek app/marker/artifact yoksa viewer PASS yazılmaz |
| Self-hosted runner offline | Exact SHA queue; host-safe iş sürer; kanıtsız VALIDATED yok |
| Unknown native/transitive asset | Release NO-GO |
| Dependency terk edilmesi | Pinned source archive + adapter üzerinden kontrollü alternatif |
| Marka/store policy değişimi | Release günü resmi kaynaktan live-verify |
| Scope creep | Backlog’a taşı; viewer DoD bitmeden başlatma |

---

## 9. Definition of Done

Android v1 ancak aşağıdakilerin tamamı gerçek evidence ile sağlandığında biter:

- [ ] Gerçek Android cihazda local DWG/DXF açılıyor; emulator smoke ayrıca mevcut.
- [ ] P0 geometry/block/text/dimension/hatch acceptance matrix geçiyor.
- [ ] Pan/pinch/fit/layer/lifecycle stabil.
- [ ] Unsupported/proxy/font/XREF/raster sorunları sessiz değil.
- [ ] Adversarial/corrupt corpus kontrollü davranıyor.
- [ ] Performance/memory hedefleri referans cihazlarda ölçülmüş ve geçilmiş ya da kontrollü limit var.
- [ ] Full corpus Android Release artifact üzerinde geçiyor.
- [ ] Original immutable; cloud/upload/account zorunluluğu yok.
- [ ] Runtime dependency/native/font/asset zincirinde unknown/policy-RED yok.
- [ ] APK/AAB inventory + SBOM + notices + evidence eşleşiyor.
- [ ] CAD SDK/API için per-user/per-file/runtime royalty veya zorunlu servis ücreti saptanmamış.
- [ ] v1 kullanıcı için ücretsiz.
- [ ] Signed/store-ready artifact + checksum + build/use docs teslim edilmiş.
- [ ] Bilinen compatibility sınırları dürüstçe yayımlanabilir.

“Bütün DWG’leri AutoCAD ile piksel piksel aynı gösterir” bir DoD değildir ve vaat edilmez.

---

## 10. v1 sonrası backlog — plan bitmeden başlanmaz

1. Read-only selection/properties.
2. Ölçüm: mesafe/alan/koordinat + unit validation.
3. User-granted proje klasöründen tam XREF resolution.
4. İleri paper-space/complex linetype/underlay.
5. PDF/SVG export için ayrı fidelity/license spike.
6. Command/undo-redo tabanlı editor.
7. Save-as-copy + DWG/DXF round-trip corpus; original overwrite varsayılan kapalı.

Feature flag dependency exclusion değildir; v1 dışı dependency gizlice runtime graph’a eklenmez.

---

## 11. Resmi başlangıç kaynakları

Yürütme gününde live-verify edilir:

- ACadSharp: https://github.com/DomCR/ACadSharp
- ACadSharp NuGet: https://www.nuget.org/packages/ACadSharp/
- ProCad: https://github.com/wieslawsoltes/ProCad
- SkiaSharp: https://github.com/mono/SkiaSharp
- .NET MAUI 10: https://learn.microsoft.com/dotnet/maui/?view=net-maui-10.0
- .NET support policy: https://dotnet.microsoft.com/platform/support/policy
- IxMilia DXF: https://github.com/ixmilia/dxf
- Android SAF: https://developer.android.com/training/data-storage/shared/documents-files
- Android memory: https://developer.android.com/topic/performance/memory-overview
- Autodesk trademark guidance: https://www.autodesk.com/company/legal-notices-trademarks/trademarks/guidelines-for-use
- Google Play Console help: https://support.google.com/googleplay/android-developer/
- Apple Developer yalnız future iOS reactivation için: https://developer.apple.com/programs/

---

## Nihai teknik ilke

> Doğrudan oku; cihazda işle; eksikliği saklama; önce doğruluğu kanıtla; sonra yalnız ölçülmüş darboğazı optimize et; final artifact’in tamamının kaynağını ve lisansını gösterebilmeden release yapma.
