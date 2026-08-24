# Mobil DWG/DXF Görüntüleyici — Nihai Uygulama ve Yürütme Planı

**Plan sürümü:** 1.0  
**Hazırlanma/doğrulama tarihi:** 24 Ağustos 2026  
**Ürün yönü:** Android-first, iOS zorunlu ikinci platform, preview-first, local/offline  
**Hedef:** Kullanıcıya ücretsiz sunulabilen; CAD dosyası başına, kullanıcı başına veya çalışma zamanı başına CAD SDK/API royalty’si doğurmayan; 2D DWG/DXF dosyalarını güvenli ve teknik olarak güvenilir biçimde görüntüleyen çalışan mobil uygulama  
**Kaynak belgeler:** `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan.md`, `chatgpt_oneriler.md`, `claude_oneriler.md`, `gemini_oneriler.md`, `sonnet_5.md`

> Bu belge bir fikir listesi değil, yürütme sırasıdır. Aşağıdaki aşamalar tamamlanmadan ürün “bitti” sayılmaz. “Royalty-free” bir hukuki garanti olarak değil, her release’in gerçek dependency ve dağıtım artifact’leri üzerinde yeniden kanıtlanan teknik/politika kriteri olarak kullanılır.

---

## 1. Yürütme durumu — tek yetkili checkpoint

Bu blok ve aşağıdaki aşama kutuları her çalışma turunun sonunda güncellenir.

```text
CURRENT_STAGE: AŞAMA 03
CURRENT_SUBSTEP: 03.7
STATUS: DONE
LAST_VERIFIED_REVISION: fb2d0982efeab8f78bc78dc82a7a8deb688190f8 — AŞAMA 03 PR #5 doğrulanmış head üzerinden main'e merge edildi
LAST_SUCCESSFUL_COMMAND: GitHub Actions Stage 03 Corpus Audit run 32752374980 / #4 SUCCESS + aynı head Stage 02 Dependency Audit run 32752375058 / #15 SUCCESS + Stage 01 Toolchain Smoke run 32752374956 / #34 SUCCESS
EVIDENCE: docs/evidence/STAGE_03.md; fixtures/manifest/stage03-mini.json; fixtures/manifest/stage03-source-integrity.json; docs/GOLDEN_CONTRACT.md; docs/DEVICE_MATRIX.md; PR #5 merge fb2d0982efeab8f78bc78dc82a7a8deb688190f8; Stage 03 artifact 9529508675 / SHA-256 fd3990d7a3271c015a2f7067a856d5a23434f1ec0449ecff7819b569938e02cf
BLOCKERS: AŞAMA 03 için yok. AŞAMA 01 fiziksel Android install/launch ve iOS erişim envanteri docs/USER_APPROVED_EXECUTION_OVERRIDE.md gereği DEFERRED_EXTERNAL_GATE olarak açık kalır.
NEXT_ACTION: AŞAMA 04 — minimal solution ve mimari sınırlar. AŞAMA 01 dış kapılarını sahte PASS/DONE yapma; aynı turda AŞAMA 05'e geçme.
LAST_UPDATE: 2026-08-24
```

Durum değerleri yalnızca şunlardır:

- `NOT_STARTED`: aşamaya başlanmadı.
- `IN_PROGRESS`: aşama başladı fakat bütün çıkış kriterleri sağlanmadı.
- `BLOCKED`: yalnız kullanıcı donanımı, hesabı, özel test dosyası veya dış platform erişimi olmadan ilerlenemiyorsa.
- `DONE`: bütün çıkış kriterleri gerçek komut/test/artifact kanıtıyla sağlandı.

### “devam” protokolü

Kullanıcı `devam` dediğinde veya projeyi sürdürmeyi istediğinde ajan şu kurallara uyar:

1. Önce bu checkpoint, gerçek dosya ağacı, Git durumu ve kullanıcı değişiklikleri kontrol edilir.
2. `IN_PROGRESS` aşama varsa yalnız o aşamadan devam edilir.
3. Aktif aşama `DONE` ise ilk tamamlanmamış aşama başlatılır.
4. Bir kullanıcı turunda **en fazla bir aşama tamamlanır**. Aşama biterse sonraki aşama aynı turda başlatılmaz.
5. Bir aşama bir turda bitmek zorunda değildir. Bitmezse tamamlanan alt adımlar işaretlenir, durum `IN_PROGRESS` kalır ve tek bir somut `NEXT_ACTION` yazılır.
6. Çıkış kriteri dosya, komut, test veya gerçek cihaz kanıtı olmadan işaretlenmez.
7. Donanım/hesap/test dosyası gerekiyorsa güvenli bağımsız işler bitirilir, sonra blocker açıkça kaydedilir; sahte başarı yazılmaz.
8. Önceki aşamalar gereksiz yere tekrarlanmaz. Her işlem mümkün olduğunca idempotent olur.
9. Plan ile gerçek repo çelişirse gerçek repo esas alınır; plan/checkpoint gerekçesiyle düzeltilir.
10. Dependency kendiliğinden yükseltilmez. Her yükseltme ayrı doğruluk, lisans ve artifact kontrolü gerektirir.
11. Kullanıcıya ait değişiklikler korunur; destructive Git veya dosya işlemi yapılmaz.
12. Her turun sonunda yalnız kısa sonuç, çalıştırılan testler, kalan risk ve sonraki eylem raporlanır.

### Aktif yürütme istisnası — dış erişim kapıları

2026-08-24 kullanıcı onayıyla `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` yürürlüktedir. AŞAMA 01'in gerçek Android cihaz install/launch ve iOS erişim envanteri kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır; bunlar sahte PASS/DONE yapılmaz. Buna rağmen fiziksel cihaz/hesap erişimine bağımlı olmayan sonraki aşamalar `gecmis.md` içindeki `NEXT_WORK_STAGE` sırasıyla yürütülebilir. Bir turda en fazla bir aşama tamamlama kuralı değişmez. Release/beta/final cihaz kapılarında ertelenen dış kanıtlar yeniden zorunlu olarak açılır.

### Token ve test bütçesi

- Önce `rg`, mevcut kod, upstream kaynak/test ve küçük adapter; en son özel implementasyon.
- Uzun loglar konuşmaya dökülmez; hata için ilgili son bölüm ve özet verilir.
- Aynı başarısız test kod/dependency değişmeden tekrar tekrar çalıştırılmaz.
- Bir turda varsayılan olarak bir ilgili build + hedefli unit/smoke testi yapılır.
- Tam corpus, bütün platform build’leri, SBOM ve artifact extraction yalnız tanımlı milestone’larda çalıştırılır.
- Büyük matematik/kod blokları plana kopyalanmaz; gereksinim fixture ve beklenen davranışla tanımlanır.

Test seviyeleri:

| Seviye | İçerik | Ne zaman |
|---|---|---|
| T0 | Restore/build/static kontrol | İlgili kod değişince |
| T1 | Değişen modülün unit testleri | Her uygulama turunda |
| T2 | 1 küçük DWG + 1 küçük DXF smoke | Parser/render/mobil akışı değişince |
| T3 | Mini corpus + gerçek Android Release cihaz testi | Aşama 05, 07, 15, 20 |
| T4 | Tam private/public corpus + iki platform Release + artifact audit | Aşama 21, 26 |

---

## 2. Değiştirilemez ürün şartları

Kullanıcı açıkça değiştirmedikçe:

- v1 bir **2D viewer** olacaktır; editor veya writer olmayacaktır.
- DWG ve DXF doğrudan cihazda okunur; zorunlu DWG→DXF bulut/ara dönüşümü yoktur.
- Temel açma/render akışı local ve offline’dır; hesap, giriş veya sunucu gerekmez.
- v1 kullanıcı için ücretsizdir; reklam, abonelik, ücretli CAD özelliği veya dosya başına ödeme yoktur.
- Autodesk RealDWG, Autodesk APS/Forge dönüşümü, ticari ODA SDK, ücretli parser/renderer veya trial SDK kullanılmaz.
- Runtime’da GPL, AGPL, SSPL, BUSL, non-commercial, source-available, proprietary veya lisansı belirsiz bileşen bulunmaz.
- LGPL/MPL hukuken imkânsız diye değil, düşük uyumluluk yükü proje politikası nedeniyle varsayılan `RED` sayılır; istisna ayrıca yazılı onay ister.
- Varsayılan allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD. Her exact bileşen yine ayrıca denetlenir.
- Root repo lisansı tek başına yeterli değildir; transitive paketler, native binary’ler, submodule/fork, vendored kod, font/PAT/icon/fixture gibi asset’ler de denetlenir.
- Orijinal kullanıcı dosyası değiştirilmez; v1’de save/overwrite yoktur.
- Unsupported/proxy entity, eksik font, XREF veya raster sessizce yok olmaz; compatibility özeti kullanıcıya gösterilir.
- Uygulama adı ve ikonu Autodesk/AutoCAD/DWG markasını ürün markası gibi kullanmaz. Format uyumluluğu açıklama metninde, release tarihindeki güncel marka kılavuzuna göre ifade edilir.
- Google Play/Apple geliştirici hesabı, Mac/Xcode, cihaz ve bakım maliyetleri “ücretsiz CAD teknolojisi” iddiasının kapsamı dışındadır.

### v1 kapsamı

Zorunlu:

- Yerel `.dwg` ve `.dxf` açma
- Android ve iOS gerçek cihaz
- Model space; corpus gerektiriyorsa standart paper space/layout/viewport
- Pan, pinch zoom, fit extents, orientation
- Layer listesi ve show/hide
- Temel 2D geometri, block/INSERT, attribute, text/MTEXT, dimension, hatch
- CAD renkleri, ByLayer/ByBlock, basit linetype ve lineweight
- Türkçe metin/encoding ve görünür font substitution
- Loading/progress/cancel talebi, kontrollü hata ve compatibility raporu
- Local/offline/privacy, close/reopen ve lifecycle güvenilirliği

v1 dışı:

- Çizim/düzenleme, selection, OSNAP, ölçüm
- DWG/DXF yazma veya farklı kaydetme
- PDF/SVG export
- Collaboration, cloud sync, kullanıcı hesabı
- Tam XREF crawler/otomatik yükleme
- Civil 3D/Architecture proprietary proxy nesnelerinde tam fidelity
- 3D CAD/BIM

---

## 3. Doğrulanmış düzeltmeler ve başlangıç varsayımları

### 3.1 Yerel ortam fotoğrafı

24 Ağustos 2026 kontrolünde:

- Klasör yalnız beş plan/öneri MD dosyası içeriyordu; uygulama kodu yoktu.
- Klasör Git reposu değildi.
- Git kurulu; .NET host/runtime mevcut fakat .NET SDK yoktu.
- Java/JDK ve `adb` PATH üzerinde yoktu.
- Aşama 00–01 bu durumu yeniden kontrol eder; bu tarihli fotoğraf gelecekte gerçek durumun yerine kullanılmaz.

### 3.2 Dependency gerçekliği

| Bileşen | 24.08.2026 doğrulaması | Başlangıç kararı |
|---|---|---|
| .NET 10 / .NET MAUI | .NET 10 LTS; MAUI Android+iOS’u destekliyor, exact SDK/workload pinlenmeli | Ana mobil teknoloji adayı |
| ACadSharp | Resmi repo ve NuGet MIT; güncel aday 3.7.1. 3.6.29 “critical bugs” nedeniyle deprecated | Ana parser adayı; ancak corpus geçmeden approved değil |
| ProCad | Resmi repo mevcut ve MIT; 0.1.x, genç ve düşük saha kullanımlı | Yalnız izole source-pinned spike |
| ProCad NuGet hattı | Rendering paketi gevşek/yanlış ACadSharp alt sınırı çözebiliyor; kaynak repo kendi ACadSharp fork/submodule’una bağlı; MAUI kontrol hattında prerelease Skia view dependency bulunuyor | Mevcut haliyle production için varsayılan NO-GO |
| SkiaSharp | MIT .NET 2D renderer; native dağıtım zinciri ayrıca denetlenmeli | Ana renderer adayı |
| IxMilia.Dxf | MIT, DXF için yararlı ama güncel README kapsam sınırlamaları var | Test oracle’ı/koşullu DXF fallback; baştan runtime’a ekleme |
| IxMilia.Dwg | Erken sürüm ve yalnız legacy R13/R14 kapsamı | Modern DWG fallback değil |
| IxMilia.Shx | MIT kod/parser; font dosyası değildir | Yalnız font spike adayı |

Önemli düzeltmeler:

- Güncel ACadSharp README’sinde “alpha” etiketi veya eski “implement edilmemiş entity” listesi bulunmadığından bunlar gerçek diye yazılmaz. Entity sınıfının varlığı da fidelity kanıtı değildir; tek kanıt pinned sürüm + fixture + beklenen sonuçtur.
- ProCad’in var olmadığı iddiası yanlıştır; ancak var olması production kalitesini kanıtlamaz.
- `Task.Run(..., token)` başlamış senkron parser’ı sihirli biçimde durdurmaz. Gerçek cooperative cancellation yoksa UI iptal talebini kaydeder, sonucu terk edebilir; “hard timeout parser’ı durdurdu” denmez.
- `largeHeap=true`, `GC.Collect()`, SKGLView/GPU, spatial index, tiling, batching ve triangulation varsayılan çözüm değildir. Yalnız profiler + A/B testiyle kabul edilir.
- “60/120 FPS”, “100 MB dosyada garanti”, “AutoCAD ile piksel piksel aynı”, “%100 vektör PDF” ve “yasal olarak kesin risksiz” vaatleri yoktur.

`[LIVE-VERIFY]` işaretli kararlar yürütme gününde resmi kaynaklardan yeniden doğrulanır.  
`[MEASURE]` işaretli optimizasyonlar yalnız ölçülmüş darboğaz varsa açılır.

---

## 4. Hedef mimari ve bağımlılık sınırları

```text
MAUI App / platform adapters
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
ICadRenderer ──► Skia renderer ──► Android / iOS canvas
```

Kurallar:

- UI doğrudan ACadSharp veya ProCad entity’lerine bağlanmaz.
- Parser document/session yaşam döngüsü tek owner tarafından yönetilir.
- Dosya açma API’si çıplak path değil; source URI/bookmark, local-copy handle, display metadata, generation ID ve cancellation talebi taşıyan `OpenRequest` kullanır.
- `RenderScene` türetilmiş ve yeniden üretilebilir; document verisinin yerine geçmez.
- World/document koordinatları `double` tutulur; Skia’ya dönüşüm tek, test edilmiş kamera hattında yapılır.
- Entity identity/handle korunur; gelecekte edit için kapı açık kalır ama v1 edit kodu taşımaz.
- Compatibility raporu parse → scene → render zincirinde veri kaybını ve substitution’ı toplar.
- Renderer seçimi bir ADR ile kilitlenir:
  - **Yol A:** ProCad’in kanıtlanan minimum source modülleri, pinned commit/fork diff ile adapter arkasında.
  - **Yol B:** ACadSharp + ince özel RenderScene + SkiaSharp.
- İki yol aynı anda production mimarisine taşınmaz. Spike kodu runtime graph’a sızmaz.
- ProCad Editing/Scripting/Collaboration, ACadSharp writer ve PDF/export paketleri v1 artifact’ine girmez.

### Başlangıç repo yapısı

Gereksiz proje parçalanması yapılmaz:

```text
src/
  MobileCad.Core/                 # session, scene, geometry, diagnostics contracts
  MobileCad.IO.ACadSharp/         # parser adapter; başka katmanlara sızmaz
  MobileCad.Rendering.Skia/       # renderer ve platformdan bağımsız kamera
  MobileCad.App/                  # .NET MAUI Android/iOS kabuğu
tests/
  MobileCad.UnitTests/
  MobileCad.IntegrationTests/
  MobileCad.GoldenTests/
spikes/
  ProCad.Android/                 # production graph dışında
fixtures/
  public/                         # yalnız redistributable/sentetik
  private/                        # Git ignored
docs/
  ADR/
  ARCHITECTURE.md
  COMPATIBILITY.md
  PERFORMANCE.md
  EXECUTION_LOG.md
compliance/
  LICENSE_POLICY.md
  DEPENDENCY_EVIDENCE.md
  THIRD_PARTY_NOTICES.md
  releases/
```

Yeni proje ancak gerçek bağımsızlık/test ihtiyacı oluşursa eklenir. `Editing`, `Export`, `Collaboration` projeleri v1’de oluşturulmaz.

---

## 5. Fidelity, compatibility ve performans sözleşmesi

### 5.1 Dört ayrı başarı durumu

“Dosya açıldı” tek başına başarı değildir:

1. **Parse success:** Dosya kontrollü biçimde document’a dönüştü.
2. **Scene success:** Beklenen entity’ler render primitive’lerine dönüştü.
3. **Render success:** Frame crash/NaN/sonsuz döngü olmadan çizildi.
4. **Engineering fidelity:** Konum, ölçü, metin, block, dimension ve görünürlük referansla kabul sınırında.

Compatibility seviyeleri:

| Seviye | Anlamı |
|---|---|
| C0 | Desteklenmiyor; algılandı ve kullanıcı uyarıldı |
| C1 | Parse edildi; render doğrulanmadı |
| C2 | Yaklaşık render; teknik fidelity garantisi yok ve uyarı var |
| C3 | Golden/semantic testle görsel olarak kabul edildi |
| C4 | Mühendislik açısından kritik fixture’da ayrıca doğrulandı |

P0 release entity’leri en az C3, dimension ve teknik annotation fixture’ları C4 olmalıdır. Yanlış görünen “yaklaşık dimension” yerine açık uyarı tercih edilir.

### 5.2 Entity önceliği

P0:

- LINE, ARC, CIRCLE, ELLIPSE
- LWPOLYLINE/POLYLINE ve bulge
- SPLINE
- POINT, SOLID, TRACE, 3DFACE’in 2D görünümü
- TEXT, MTEXT
- INSERT, nested INSERT, ATTRIB/ATTDEF
- DIMENSION
- HATCH

P1:

- LEADER/MLEADER, TOLERANCE
- XLINE, RAY, MLINE
- TABLE
- VIEWPORT/layout
- IMAGE/WIPEOUT
- Basit XREF tespiti

P2 — v1’i bloklamaz fakat raporlanır:

- Proxy/custom entity
- Civil 3D ve AutoCAD Architecture özel nesneleri
- ACIS/ileri 3D, ileri underlay ve dynamic block davranışları

### 5.3 Zorunlu doğruluk fixture’ları

- OCS→WCS/extrusion normal
- Büyük ve negatif koordinatlar; precision
- Polyline bulge: sıfır, pozitif, negatif, yarım/tam daireye yakın
- Nested block; rotate, mirror ve non-uniform scale
- Layer 0, ByLayer, ByBlock, ACI 7, true color
- Basit dashed linetype, LTSCALE ve lineweight
- Türkçe CP1254 ve Unicode TEXT/MTEXT; alignment/width factor
- SHX eksik/mevcut/substitution
- Linear/aligned/angular/radius/diameter dimension; `*D` anonymous block
- Solid/dense/pattern hatch, island ve bozuk boundary
- SOLID vertex order
- Layout/viewport clip ve viewport layer override
- Eksik XREF/raster/proxy ve controlled warning

### 5.4 Provisional performans hedefleri

Dosya boyutu tek ölçü değildir. Şunlar birlikte kaydedilir: file byte, parsed entity, expanded block instance, scene primitive, glyph, hatch segment/vertex, raster pixel, peak managed/native/PSS bellek.

Corpus ilk benchmarkından sonra hedefler cihaz bazında bir kez sabitlenir. Başlangıç hedefleri garanti değildir:

| Profil | İlk anlamlı görüntü (TTFUP) | Etkileşim hedefi — Android Release |
|---|---|---|
| Küçük: ≤10 bin primitive | ≤2 sn hedef | p95 frame ≤16.7 ms hedef, ≤33.3 ms minimum |
| Orta: 10–100 bin | ≤5 sn hedef | p95 frame ≤33.3 ms minimum |
| Büyük: 100–500 bin | ≤10 sn veya görünür ilerleme/kademeli sonuç | ANR/OOM yok; ölçümlü LOD/culling gerekebilir |
| Extreme: >500 bin | Ürün garantisi yok | Crash yerine kontrollü uyarı/ret |

- Debug performansı release kararı değildir.
- Referans: kullanıcının ana telefonu + mümkünse en az bir 4–6 GB RAM orta/alt segment fiziksel Android; iOS için en az bir desteklenen gerçek iPhone.
- Repeat-open testinde kapanıştan sonra PSS/native bellek monoton büyümemeli; ilk kapanış baseline’ına göre beş döngü sonrası provisional tolerans +%15’tir ve Aşama 20’de cihaz bazında kesinleştirilir.
- `largeHeap` varsayılan kapalıdır. Cache bütçesi ve hard guard değerleri gerçek `memoryClass`/profiler sonucuyla belirlenir.

### 5.5 Optimizasyon kabul kuralı

`[MEASURE]` adayları: SKGLView/GPU, primitive batching, R-tree/quadtree/BVH, tiling/SKPicture, adaptive tessellation, local-origin shift, LOD, HarfBuzz, hatch triangulation, trimming, `largeHeap`.

Her biri için zorunlu sıra:

```text
baseline → profiler kanıtı → küçük izole A/B spike → aynı corpus
→ doğruluk/bellek/size regresyonu yok → ölçülebilir kazanç → ADR ile kabul
```

Kazanç kanıtlanmazsa kod/dependency eklenmez.

---

## 6. Lisans, kaynak ve veri firewall’u

Her runtime dependency/asset için kayıt:

- exact package version ve resolved transitive graph
- source repo URL ve commit/tag
- package/source SHA-256
- license dosyası ve hash’i
- submodule, fork ve upstream diff
- native `.so`, `.aar`, `.jar`, `.framework`, `.dylib`
- font, icon, PAT, fixture ve embedded resource provenance
- redistribution/notice/source disclosure/royalty değerlendirmesi
- runtime artifact’e dahil olup olmadığı
- reviewer sonucu: `GREEN`, `REVIEW`, `RED`

Kurallar:

- Central Package Management + exact version + `packages.lock.json` + locked restore.
- `*`, `latest`, floating veya uygulamanın doğrudan dependency’sinde açık alt sınır yok.
- Unknown license/native binary/asset = release blocker.
- Scanner yardımcıdır; dual-license, vendored/native/source ve asset’ler manuel kanıt ister.
- Rejected lisanslı kaynaktan kod, test vektörü veya satır satır port alınmaz. Algoritma gerekiyorsa permissive kaynak veya kamuya açık teknik spesifikasyon kullanılır ve provenance yazılır.
- Açık internette bulunan DWG/DXF, font veya screenshot yeniden dağıtılabilir sayılmaz.
- Müşteri/kullanıcı çizimleri yalnız izinli private corpus’ta, Git dışında ve hassas yol/metin loglanmadan tutulur.
- Uygulamaya yalnız açık redistribution izni kanıtlanmış fontlar gömülür. AutoCAD kurulumundan alınan SHX dosyaları bundle edilmez. Kullanıcının kendi fontunu lokal seçmesi dağıtım sayılmaz.
- RC’de gerçek APK/AAB/IPA içeriği çıkarılır; kaynak/license evidence ile karşılaştırılır.
- Release için CycloneDX veya SPDX SBOM, notices ve immutable compliance snapshot oluşturulur.

---

## 7. Aşamalı uygulama planı

### Aşama indeksi

- [x] AŞAMA 00 — Çalışma alanı ve yürütme zemini
- [ ] AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler — `DONE`
- [x] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi — `DONE`
- [ ] AŞAMA 04 — Minimal solution ve mimari sınırlar — `NEXT`
- [ ] AŞAMA 05 — ACadSharp headless parser spike
- [ ] AŞAMA 06 — Android güvenli dosya alma ve parse spike
- [ ] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO
- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke
- [ ] AŞAMA 09 — RenderScene, kamera ve diagnostics temeli
- [ ] AŞAMA 10 — P0 temel geometri renderer’ı
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
- [ ] AŞAMA 23 — iOS toolchain, shared core ve ilk gerçek cihaz
- [ ] AŞAMA 24 — iOS fidelity, lifecycle ve Release archive
- [ ] AŞAMA 25 — Cross-platform beta ve yalnız blocker düzeltmeleri
- [ ] AŞAMA 26 — Dependency freeze, final audit ve RC onayı
- [ ] AŞAMA 27 — v1 artifact, yayın/handoff ve kapanış

### AŞAMA 00 — Çalışma alanı ve yürütme zemini

**Amaç:** Mevcut belgeleri koruyarak izlenebilir çalışma zemini kurmak.

İşler:

- [x] Dosya ağacı, disk alanı, Git ve mevcut toolchain yeniden envanterlendi.
- [x] GitHub’daki mevcut `smitelagwar/mobil-dwg` reposu ve `main` geçmişi doğrulandı; kullanıcı belgeleri korunarak devam edildi. Repo zaten başlatılmış olduğundan yeniden `git init` yapılmadı.
- [x] Build/temp/private corpus/signing secret’ları için mevcut `.gitignore` incelendi ve yeterli bulundu; gereksiz değişiklik yapılmadı.
- [x] `docs/EXECUTION_LOG.md`, `docs/ADR/0000-template.md`, `docs/EVIDENCE_TEMPLATE.md` ve proje devri için kökte `gecmis.md` oluşturuldu.
- [x] Bu planın checkpoint’i gerçek durumla güncellendi.

Test: Git status ve ignore dry-run.  
Çıkış: Kaynak araştırma MD’leri değişmeden korunur; yalnız bu nihai planın checkpoint’i yürütme durumu için güncellenir. İzlenebilir repo, yürütme günlüğü ve tek aktif checkpoint vardır.

### AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon

**Amaç:** CAD dependency eklemeden önce geliştirme zincirini kanıtlamak.

İşler:

- [x] `[LIVE-VERIFY]` Desteklenen güncel .NET 10 SDK patch’i resmi kaynaktan doğrulandı ve `global.json` ile pinlendi: SDK/workload set `10.0.400`.
- [x] MAUI/Android workload, Microsoft OpenJDK 21.0.12, Android SDK API 36 / Build-Tools 36.0.0 / stable Platform-Tools 37.0.1 exact hattı GitHub Actions temiz runner üzerinde kuruldu ve kaydedildi.
- [x] Minimum Android API `24`, target/compile SDK `36` olarak kaydedildi; temiz MAUI 10 template varsayılan min API 21 olduğundan `SupportedOSPlatformVersion=24.0` açıkça pinlendi.
- [x] Temiz MAUI smoke app `net10.0-android` Debug ve Release derlendi; güncel CI run `32739952628` exact toolchain/workload, pinned `ApplicationId=com.smitelagwar.mobildwg.stage01smoke`, Debug/Release, manifest `minSdk=24 / targetSdk=36` ve APK artifact kapılarını geçti.
- [x] Fiziksel cihaz kapısı için Windows PowerShell ve Bash gate scriptleri eklendi; CI üzerinde syntax/parse doğrulaması PASS oldu. Scriptler exact toolchain/workload, fiziksel `state=device`, emulator dışlama, Debug/Release build, manifest 24/36, install ve launcher `Status: ok` koşullarını zorunlu kılar.
- [ ] `adb` ile kullanıcının gerçek telefonuna yüklenir ve açılır; gerçek geliştirme makinesinde `STAGE01_DEVICE_GATE_PASS` alınır.
- [ ] iOS için Mac/Xcode/iPhone/Apple Developer erişimi yalnız envanterlenir; henüz kurulum yapılmaz.

Test: `dotnet --info`, workload list, Android Debug/Release build, manifest/API/package baseline ve device-gate script syntax kontrolü en güncel regresyon run `32747785948` / #29 üzerinde PASS. Root Central Package Management etkisinden kaçınmak için temiz MAUI smoke projesi `$RUNNER_TEMP` altında izole edildi. Fiziksel Android install/launch ancak gerçek cihazdaki `STAGE01_DEVICE_GATE_PASS` ile kapanır.  
Çıkış: Gerçek telefonda boş MAUI uygulaması çalışır; exact toolchain kanıtı vardır. Telefon/iOS erişimi olmadığından aşama `BLOCKED / DEFERRED_EXTERNAL_GATE` kalır; `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` bağımsız sonraki aşamaların ilerlemesine izin verir.

### AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler

**Amaç:** Kod yazmadan önce kullanılabilecek exact teknoloji çizgisini belirlemek.

İşler:

- [x] ACadSharp `3.7.1` current stable release, source/license/dependency/submodule hattı resmi kaynaklardan kaydedildi; dependency/lisans sınıfı `GREEN`, fidelity kararı AŞAMA 05'e bırakıldı.
- [x] SkiaSharp `4.151.1` ve Android native asset graph’ı incelendi; exact `.nupkg` hash/native-entry kanıtı kaydedildi; final native third-party inventory nedeniyle `REVIEW`.
- [x] ProCad source snapshot `f8a862b3e7634e27664fee02ff5d68774b102985`, ACadSharp fork submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36` ve ProEdit `64759b79289a024d08463ed1a9094fdcd9a270df` kaydedildi; production default `NO-GO`, yalnız AŞAMA 07 source-pinned spike.
- [x] IxMilia.Dxf `0.8.4` test/fallback scope'unda `GREEN`; IxMilia.Dwg/Shx source-only `REVIEW` olarak raporlandı.
- [x] `compliance/LICENSE_POLICY.md`, `compliance/DEPENDENCY_EVIDENCE.md`, `compliance/RISK_REGISTER.md` ve `docs/evidence/STAGE_02.md` hazırlandı.
- [x] Central Package Management, exact versions, committed `packages.lock.json`, `--locked-mode` restore, exact `.nupkg` SHA-256/license manifest'i ve CI vulnerability/reproducibility kapısı kuruldu.

Test: GitHub Actions `Stage 02 Dependency Audit` run `32747785867` / #9 SUCCESS; committed locked restore, resolved graph, exact nupkg license/hash audit, manifest diff, vulnerability check ve evidence artifact PASS. Aynı final PR head üzerinde `Stage 01 Toolchain Smoke` run `32747785948` / #29 SUCCESS; root-CPM regresyonu kapandı.  
Çıkış: Sağlandı. Her aday `GREEN/REVIEW/RED`; floating/latest production dependency yok; unknown/policy-RED resolved package yok; ProCad production graph’a eklenmedi. PR #4 merge commit `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.

### AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi

**Amaç:** Parser/renderer kararlarını göz kararı yerine tekrar üretilebilir veriyle vermek.

İşler:

- [x] `fixtures/manifest` şeması oluşturuldu; hash, format/version, boyut, hak/provenance, özellikler, beklenen counts/warnings ve golden metadata alanları tanımlandı.
- [x] Public/synthetic/private corpus ayrımı kuruldu; private fixture yolu Git ignored ve validator tarafından enforced.
- [x] İlk mini corpus 4 DWG familyası (R2000/R2004/R2010/R2018) + en az 2 DXF içeriyor; upstream binary'ler immutable ACadSharp revision üzerinden remote-pinned tutuluyor, repoya vendored edilmiyor.
- [x] Set basic geometry, Turkish text, nested block, dimension, hatch ve paper-space/layout feature coverage içeriyor.
- [x] CI-derived truncated/corrupt DWG ile committed missing-font/missing-XREF negatif fixture'ları eklendi.
- [x] Golden görüntü redistribution sözleşmesi `docs/GOLDEN_CONTRACT.md` içinde tanımlandı; izin kanıtı olmadan image golden repoya giremez.
- [x] Android/iOS fiziksel cihaz matrisi ve provisional benchmark profilleri `docs/DEVICE_MATRIX.md` içinde yazıldı; gerçek cihaz slotları erişim yokluğu nedeniyle UNKNOWN/DEFERRED_EXTERNAL_GATE.

Test: GitHub Actions `Stage 03 Corpus Audit` run `32752374980` / #4 SUCCESS; `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`, `STAGE03_DUAL_HASH_PASS fixtures=6`, private-ignore/coverage/version/hash/provenance ve evidence artifact upload PASS. Aynı final head üzerinde Stage 02 run #15 ve Stage 01 run #34 SUCCESS.  
Çıkış: Sağlandı. Mini corpus + beklenen sonuç manifest’i, dual-hash source integrity kaydı, golden contract ve cihaz matrisi mevcut. PR #5 merge commit `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`. AŞAMA 01 dış cihaz kapıları ertelenmiş olarak açık kalır.

### AŞAMA 04 — Minimal solution ve mimari sınırlar

**Amaç:** Parser ve renderer’ı UI’dan ayıran küçük, derlenebilir iskelet.

İşler:

- [ ] Dört production projesi ve üç test projesi oluşturulur; v1 dışı proje açılmaz.
- [ ] `ICadDocumentReader`, session owner, `IRenderSceneBuilder`, `ICadRenderer`, diagnostics ve compatibility kontratları tanımlanır.
- [ ] Core katmanı MAUI/Skia/ACadSharp’a referans vermez.
- [ ] Cancellation/progress API’si gerçek destek düzeyini yanlış temsil etmeyecek şekilde modellenir.
- [ ] Architecture dependency tests eklenir.

Test: T0 + kontrat unit testleri.  
Çıkış: Solution temiz restore/build/test geçer; dependency yönleri otomatik test edilir.

### AŞAMA 05 — ACadSharp headless parser spike

**Amaç:** Pinned ACadSharp ile gerçek DWG/DXF okuma ve diagnostics’i UI’dan bağımsız kanıtlamak.

İşler:

- [ ] Exact candidate paket adapter projesine eklenir; writer/API’leri kullanılmaz.
- [ ] Format magic/version preflight, reader notifications, exceptions ve timing rapora bağlanır.
- [ ] Mini corpus headless açılır; layer/block/layout/entity type dağılımı manifest ile karşılaştırılır.
- [ ] Unsupported/proxy ve notification severity sınıflandırılır; sabit “uyarı sayısı eşiği” kullanılmaz.
- [ ] Aynı document’tan türetilen iki count’ın kaybı kanıtlamadığı kabul edilir; golden beklentiyle kıyaslanır.
- [ ] Approved sürüm ADR’si ve known-failure listesi yazılır.

Test: T3 mini corpus headless regression.  
Çıkış: DWG/DXF parse yolu, diagnostics ve sürüm kararı kanıtlıdır. Kritik corpus kaybında önce ACadSharp sürüm karşılaştırılır; IxMilia yalnız DXF için koşullu spike olur.

### AŞAMA 06 — Android güvenli dosya alma ve parse spike

**Amaç:** Gerçek content URI’den dosyayı güvenli biçimde alıp UI’ı kilitlemeden parse etmek.

İşler:

- [ ] MAUI FilePicker/Android SAF content URI akışı uygulanır.
- [ ] Provider filename sanitize, bildirilen boyuta güvenmeyen stream byte quota, boş disk kontrolü, progress, atomic unique cache file, stream disposal ve deterministic cleanup eklenir.
- [ ] Persistable grant yalnız gerçekten gerekirse ve platform kurallarına uygun alınır.
- [ ] Parse UI thread dışında çalışır; cooperative cancellation yoksa kullanıcıya yanlış “parser durdu” sözü verilmez.
- [ ] Her açma isteği generation ID taşır; hızlı ikinci seçimde eski parse sonucu UI’a yazılmaz (`last request wins`).
- [ ] Metadata/diagnostics ekranına kadar gerçek telefonda DWG ve DXF açılır.
- [ ] Original dosya hiçbir koşulda yazılmaz.

Test: Android Debug + Release; küçük DWG/DXF, cancel UI, rotate/background/close.  
Çıkış: Gerçek telefon yerel dosyayı güvenle okur, parse sonucu/uyarısı gösterir, temp dosya sızıntısı yoktur.

### AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO

**Amaç:** Hazır renderer/control reuse’unu üretim kodunu bağlamadan ölçmek.

İşler:

- [ ] Spike yalnız `spikes/ProCad.Android` içinde exact commit/submodule SHA ile kurulur.
- [ ] NuGet 0.1.x restore graph’ı ile source graph farkı belgelenir; gevşek ACadSharp lineage kabul edilmez.
- [ ] ProCad fork’u ile official approved ACadSharp diff/API davranışı incelenir.
- [ ] SKCanvasView baseline ile gerçek Android Release build yapılır; GPU zorunlu kılınmaz.
- [ ] Mini corpus’ta ilk frame, pan/pinch, Turkish text, nested block, dimension, hatch, layout ve close/reopen ölçülür.
- [ ] ProCad scene’deki float koordinat hattı; küçük bina, büyük survey origin’i ve milimetre detay fixture’larıyla precision gate’inden geçer veya source patch/NO-GO olur.
- [ ] Runtime graph license/native/preview riskleri denetlenir.
- [ ] ADR üç sonuçtan birini verir: minimum ProCad source modülleriyle `GO`, özel renderer için yeniden maliyetlendirme gerektiren `CONDITIONAL-GO` veya teknik/lisans `NO-GO`.

Blocker FAIL: build/runtime crash/ANR, unresolved ACadSharp lineage, unknown/rejected license, sistematik P0 fidelity kaybı, dispose/lifecycle arızası.  
Test: T3 gerçek Android Release A/B.  
Çıkış: Kanıtlı GO/CONDITIONAL-GO/NO-GO ADR. ProCad geçmezse özel renderer “garantili fallback” sayılmaz; P0 kapsamı, tahmini efor, bakım riski ve alternatif upstream patch yolu yazılır ve kullanıcı GO kararı olmadan Aşama 09’a geçilmez. Spike dependency’si production graph’a kendiliğinden girmez.

### AŞAMA 08 — Erken iOS AOT/native fizibilite smoke

**Amaç:** Android’e aylarca yatırım yapmadan seçilen dependency hattının iOS Release/AOT’ta temel olarak çalışabildiğini kanıtlamak.

İşler:

- [ ] Aşama 01’de envanterlenen Mac/Xcode/iPhone erişimi yeniden doğrulanır.
- [ ] Seçilen exact parser/renderer hattı Mac’te restore/build edilir; gevşek veya Android-only dependency kabul edilmez.
- [ ] En küçük sentetik DWG/DXF veya deterministic scene iOS Release/AOT ile açılıp tek frame render edilir.
- [ ] Trimming, reflection, native Skia loading ve font resource uyarıları kaydedilir.
- [ ] Gerçek iPhone varsa smoke yapılır; yoksa simulator yalnız kısmi kanıt olarak yazılır.
- [ ] Mac/iPhone yoksa kullanıcıya iki seçenek sunulur: aşamayı `BLOCKED` tutmak veya Android geliştirmesine açıkça belgelenmiş iOS riskiyle devam etmek. Risk kabulü iOS’u tamamlanmış saymaz.

Test: iOS Release/AOT build + mümkünse gerçek cihazda küçük open/render.  
Çıkış: Seçilen mimarinin iOS’ta temel fizibilitesi kanıtlıdır veya dış blocker/risk kabulü açıkça kaydedilmiştir.

### AŞAMA 09 — RenderScene, kamera ve diagnostics temeli

**Amaç:** Seçilen yol üzerinde parser’dan bağımsız, test edilebilir sahne çekirdeği.

İşler:

- [ ] Tek scene implementasyonu seçilir: ProCad GO ise onun scene’i üzerinde ince facade; değilse compact özel immutable scene. Paralel iki scene graph oluşturulmaz.
- [ ] Stable entity ID, bounds, layer/style token ve source reference facade/scene üzerinde modellenir.
- [ ] Document/world koordinatları `double`; world→view→screen tek transform hattıdır.
- [ ] OCS/WCS, extents, invalid NaN/Infinity ve büyük koordinat unit testleri eklenir.
- [ ] Scene build diagnostics unsupported/substituted/dropped/error türlerini toplar.
- [ ] Camera fit/zoom bounds ve background/color context tanımlanır.
- [ ] Seçilen ProCad yolunda da bu sınırlar adapter ile korunur.

Test: T0/T1, deterministic scene snapshot.  
Çıkış: Sentetik scene headless üretilebilir; aynı girdi aynı semantik snapshot’ı verir.

### AŞAMA 10 — P0 temel geometri renderer’ı

**Amaç:** Temel 2D geometriyi doğru, sade Skia baseline ile çizmek.

İşler:

- [ ] LINE/ARC/CIRCLE/ELLIPSE/POINT.
- [ ] LW/POLYLINE + bulge; SPLINE için pinned parser verisini doğrulayan tessellation.
- [ ] SOLID/TRACE/3DFACE 2D görünümü ve vertex order.
- [ ] OCS/extrusion, mirror ve büyük koordinat fixture’ları.
- [ ] Draw order, clipping ve antialias baseline.
- [ ] Batching/GPU/tiling eklenmez; önce doğruluk ve baseline ölçülür.

Test: T1 + küçük golden/semantic diff.  
Çıkış: P0 basic fixture’ları C3; NaN/invalid geometry kontrollü warning olur.

### AŞAMA 11 — Mobil viewport ve gesture’lar

**Amaç:** CAD hissinde stabil kamera etkileşimi.

İşler:

- [ ] Pan, pinch zoom, fit extents ve isteğe bağlı double-tap fit.
- [ ] Parmak odak noktası korunur; min/max zoom ve overscroll guard.
- [ ] Portrait/landscape/safe area; rotation yeniden parse etmez.
- [ ] Gesture sırasında düşük kalite ancak detay kalıcı kaybolmadan kullanılabilir.
- [ ] Frame timing overlay yalnız debug/diagnostic build’de.

Test: Gesture unit + gerçek telefon smoke ve frame baseline.  
Çıkış: Küçük/orta fixture’da navigation doğru ve donmadan çalışır; performans sayıları kaydedilir.

### AŞAMA 12 — Block/INSERT/attribute dönüşümleri

**Amaç:** Mimari/statik çizimlerin ana tekrar mekanizmasını doğru çözmek.

İşler:

- [ ] Translation/rotation/scale/mirror matris sırası.
- [ ] Nested block ve non-uniform scale.
- [ ] ATTRIB/ATTDEF text placement ve stable identity.
- [ ] Layer 0 ile ByBlock/ByLayer parent context.
- [ ] Cycle ve depth/expanded-instance guard.
- [ ] Aynı block definition için ölçülü shared geometry; entity başına ağır kopya yok.

Test: Transform conformance + nested golden.  
Çıkış: Block fixture’ları C3, attribute/teknik etiketler kayıp değil; cycle kontrollü warning.

### AŞAMA 13 — Layer, renk, linetype ve lineweight

**Amaç:** Çizimin teknik görsel hiyerarşisini doğru göstermek.

İşler:

- [ ] Layer on/off/frozen ve görünürlük filtresi.
- [ ] ACI, true color, ACI 7 light/dark background davranışı.
- [ ] ByLayer/ByBlock/Layer 0 nested çözümleyici tek merkezde.
- [ ] Basit linetype, entity scale, global LTSCALE.
- [ ] Lineweight aç/kapat ve screen/plot semantiği açıkça ayrılır.
- [ ] Complex text/shape linetype C0/C2 raporu; v1’i şişirmez.

Test: Style resolver unit + golden.  
Çıkış: Style fixture’ları C3; layer toggling sahneyi yeniden parse etmeden çalışır.

### AŞAMA 14 — TEXT/MTEXT, Türkçe, font ve SHX

**Amaç:** Teknik not ve ölçü metnini okunur, izlenebilir fallback ile göstermek.

İşler:

- [ ] Header/codepage çözümleme; CP1254 ve Unicode fixture.
- [ ] TEXT height, width, rotation, alignment, justification ve mirror.
- [ ] MTEXT için stateful tokenizer/minimum formatting; regex ile nested format silinmez.
- [ ] Font resolver exact isim → açık lisanslı mapping → sistem fallback sırası.
- [ ] Her substitution compatibility raporuna ve UI’a düşer; text extents etkisi test edilir.
- [ ] ProCad/IxMilia SHX kabiliyeti izole fixture ile ölçülür; full özel SHX interpreter son çaredir.
- [ ] Bundle edilen her font exact license/hash/notice kaydı alır; kullanıcı font importu app-private kalır.

Test: Turkish/SHX/text-metrics semantic + golden.  
Çıkış: Türkçe metin bozulmaz; eksik font sessiz değildir; P0 text C3.

### AŞAMA 15 — Dimension, leader ve hatch doğruluğu

**Amaç:** Mühendislik açısından yanıltıcı render riskini kapatmak.

İşler:

- [ ] DIMENSION için önce mevcut `*D` anonymous block render edilir; gerekmedikçe formatter sıfırdan yazılmaz.
- [ ] Linear/aligned/angular/radius/diameter, text override, arrows ve scale fixture’ları.
- [ ] LEADER/MLEADER/TOLERANCE destek seviyesi ayrı ölçülür; approximated öğe etiketlenir.
- [ ] Solid/pattern/dense hatch, island, clipping ve bozuk boundary.
- [ ] Hatch LOD/triangulation yalnız profiler/fidelity gerekçesiyle ayrı A/B sonrası eklenebilir.
- [ ] Yanlış dimension göstermek yerine C0/C2 warning politikası uygulanır.

Test: T3 engineering mini corpus + golden/semantic human review.  
Çıkış: Release corpus dimension C4; hatch P0 fixture’ları C3; desteklenmeyen annotation görünür warning.

### AŞAMA 16 — Model space, layout, paper space ve viewport

**Amaç:** Pafta üzerinden okunan gerçek projeleri desteklemek.

İşler:

- [ ] Model/layout seçici ve active layout metadata.
- [ ] Paper-space entity’leri, rectangular/polygon clip, view center/height/twist transform.
- [ ] Viewport-specific layer override/freeze.
- [ ] Layout değişimi document’ı yeniden parse etmez.
- [ ] Corpus’ta layout teknik anlam için zorunluysa v1 release blocker; değilse destek seviyesi açıkça belgelenir.

Test: Layout semantic + golden ve gerçek Android navigation.  
Çıkış: Standart corpus layout’u C3; unsupported viewport detayı uyarı verir.

### AŞAMA 17 — XREF/raster/underlay ve compatibility raporu

**Amaç:** Eksik dış kaynakları crash veya gizli veri erişimi olmadan yönetmek.

İşler:

- [ ] XREF adı/yolu tespit edilir; remote URL otomatik indirilmez.
- [ ] v1 varsayılanı tespit + eksik warning; kullanıcı klasör izni olmadan sibling crawler yapılmaz.
- [ ] Opsiyonel klasör eşleştirme yalnız explicit grant, canonical path, cycle/depth/byte guard ile.
- [ ] Raster v1 varsayılanı tespit + warning’dir. Gerçek raster render ancak mobil E3 kanıtı, path traversal koruması ve pixel/dimension/decode budget ile alınabilir.
- [ ] PDF/ileri underlay v1’de render edilmez; drawing açılmaya devam eder ve açık C0 warning gösterir.
- [ ] Kullanıcı compatibility ekranı parse/scene/render, font, XREF/raster ve proxy özetini sunar.

Test: Missing/present external fixture, path traversal negatif test.  
Çıkış: Dış kaynak problemi ana çizimi çökertmez veya sessizce gizlemez.

### AŞAMA 18 — Tam Android viewer UX ve lifecycle

**Amaç:** Teknik motoru günlük kullanılabilir Android uygulamasına dönüştürmek.

İşler:

- [ ] Home/Open, loading/progress, viewer, layer sheet, fit, file info, warnings, close.
- [ ] Recent files yalnız güvenli metadata/bookmark; hassas içerik/thumb varsayılan değil.
- [ ] Android recent politikası: persistable URI mümkünse grant; değilse “dosyayı yeniden seç” durumu. Cache path kalıcı kaynak gibi saklanmaz.
- [ ] Android Back, foreground/background, orientation, process recreation ve memory pressure.
- [ ] Session/camera korunur; rotation’da reparse yok; deterministic dispose/cache cleanup.
- [ ] Crash sırasında dosya yazmaya güvenmek yerine önceden session marker; sonraki açılışta safe recovery.
- [ ] Imported DWG/DXF, filename, thumbnail ve prepared scene Android Auto Backup’tan `data-extraction-rules`/no-backup alanlarıyla çıkarılır; persistent thumbnail v1’de varsayılan kapalıdır.
- [ ] Final manifestte core viewer için gereksiz `INTERNET` izni bulunmaz; loglar path, filename, drawing text ve entity içeriğini redakte eder.
- [ ] Open With/share ayrı dar intent spike; aşırı geniş `application/octet-stream` yakalama yok.
- [ ] Light/dark, tablet/safe area ve minimum erişilebilir touch/labels.

Test: Gerçek telefon lifecycle matrix + T2.  
Çıkış: Kullanıcı günlük aç/incele/kapat akışını yapar; temp/cache/session sızıntısı yok.

### AŞAMA 19 — Kötü niyetli/bozuk dosya ve resource guard’ları

**Amaç:** DWG/DXF’i untrusted input olarak kontrollü işlemek.

İşler:

- [ ] Extension yanında magic/version ve bounded stream preflight.
- [ ] File bytes, entity, block depth/instances, scene primitive, hatch vertices, text length, raster pixels ve XREF toplam budget.
- [ ] NaN/Infinity/extreme extents ve recursion/cycle guard.
- [ ] Corrupt/truncated/oversized fixture’da kontrollü error taxonomy.
- [ ] Parser cooperative cancellation yoksa limitation belgelenir; UI watchdog hard kill gibi sunulmaz.
- [ ] Hassas path/text loglanmaz; local diagnostics export kullanıcı onaylı ve redacted.
- [ ] Küçük bounded mutation/fuzz smoke; cihazı saatlerce tüketen fuzz bu aşamanın parçası değildir.

Test: Negative corpus + targeted guard unit tests.  
Çıkış: Bilinen kötü input sınıfları crash/ANR yerine hata veya güvenli ret üretir.

### AŞAMA 20 — Ölçümlü performans ve bellek optimizasyonu

**Amaç:** Yalnız gerçek darboğazları düzeltmek ve final bütçeleri sabitlemek.

İşler:

- [ ] Android Release gerçek cihazda T0–T6/TTFUP, frame p50/p95, managed/native/PSS, GC ve artifact size baseline.
- [ ] Küçük/orta/büyük corpus ve beş repeat-open döngüsü ölçülür.
- [ ] En büyük darboğaz profiler ile seçilir; aynı anda tek optimizasyon spike edilir.
- [ ] Gerekirse viewport culling → shared cache → LOD → spatial index/GPU sırasıyla, A/B kanıtıyla değerlendirilir.
- [ ] Precision sorunu ölçülürse local-origin tek transform hattına eklenir.
- [ ] Cache shedding ve final resource budgets cihaz bazında yazılır; `largeHeap` ancak diğer yollar yetersiz ve kanıtlıysa ADR ister.
- [ ] Regresyon yoksa kabul; kazanım yoksa spike production’a alınmaz.

Test: T3 benchmark + correctness diff.  
Çıkış: `PERFORMANCE.md` ölçümler/cihazlar/final eşikler içerir; provisional hedeflerin geçip geçmediği nettir.

### AŞAMA 21 — Android tam corpus regresyon ve beta kapısı

**Amaç:** “Bende açıldı” yerine destek matrisiyle Android beta kararı.

İşler:

- [ ] Full private/public corpus parse/scene/render/golden çalışır.
- [ ] P0/P1 entity ve DWG version compatibility matrix güncellenir.
- [ ] En az ana telefon; mümkünse ikinci fiziksel Android/tablet matrisi çalışır.
- [ ] Debug/Release farkı, trimming/AOT davranışı ve artifact size kontrol edilir.
- [ ] Açık P0/P1 bug’lar sınıflandırılır; C0/C2 özellikler kullanıcı metnine yansır.

Test: T4’ün Android kısmı.  
Çıkış: P0 blocker yok; bilinen sınırlamalar dürüst ve fixture kanıtlı; beta build hazır.

### AŞAMA 22 — Android Release/AAB/compliance RC

**Amaç:** Store/sideload için imzalanabilir, denetlenmiş Android release candidate.

İşler:

- [ ] Özgün uygulama adı, package ID, ikon ve versioning kullanıcı kararıyla kilitlenir.
- [ ] `[LIVE-VERIFY]` Target SDK, Play policy, Data Safety, privacy ve store gereksinimleri resmi kaynaklardan kontrol edilir.
- [ ] Turkish/English temel UI, accessibility labels/touch sizes, privacy/about/open-source licenses ekranı.
- [ ] Release signing anahtarı güvenli oluşturulur/yedeklenir; secret repo/log/chat’e girmez.
- [ ] Signed APK + AAB üretilir ve gerçek cihazda kurulup smoke edilir.
- [ ] Backup exclusion kuralları ve gereksiz permission yokluğu artifact/manifest üzerinde doğrulanır.
- [ ] Artifact çıkarılır; DLL/SO/JAR/font/asset envanteri dependency evidence ile karşılaştırılır.
- [ ] SBOM, THIRD_PARTY_NOTICES ve Android compliance snapshot oluşturulur.
- [ ] Marka/store açıklamasında uyumluluk iddiası güncel Autodesk guideline’a göre kontrol edilir.

Test: Signed Release smoke + artifact audit.  
Çıkış: Android RC installable ve compliance `GREEN`; unknown artifact yok.

### AŞAMA 23 — iOS toolchain, shared core ve ilk gerçek cihaz

**Amaç:** Windows’ta varsayım üretmeden iOS hattını gerçekten kurmak.

İşler:

- [ ] Mac, desteklenen Xcode/.NET workload ve exact sürümler hazırlanır.
- [ ] iPhone erişimi/signing yöntemi kaydedilir; App Store üyeliği gerekiyorsa açık blocker olur.
- [ ] Shared core/tests Mac’te build edilir; platform fork’u minimum tutulur.
- [ ] iOS file importer/security-scoped URL/app-private cache akışı uygulanır.
- [ ] Skia render, gestures ve küçük DWG/DXF gerçek iPhone’da çalışır.
- [ ] Release AOT/trimming/reflection/resource loading problemleri erken test edilir.

Test: iOS Debug + Release gerçek device smoke.  
Çıkış: Gerçek iPhone DWG/DXF açar ve gezilir. Mac/iPhone yoksa aşama `BLOCKED`; simülatör başarı sayılmaz.

### AŞAMA 24 — iOS fidelity, lifecycle ve Release archive

**Amaç:** Android’de kanıtlanan ürünü iOS’ta eşdeğer güvenilirliğe taşımak.

İşler:

- [ ] Font/encoding, file URI, native Skia, memory ve AOT platform farkları giderilir.
- [ ] Background/foreground, orientation, memory warning, safe area, dark/light.
- [ ] Imported drawing/cache dosyaları iCloud backup dışında tutulur; security-scoped bookmark erişilemezse kullanıcıdan yeniden seçim istenir.
- [ ] Mini ardından tam corpus’un uygulanabilir kısmı gerçek iPhone’da çalışır.
- [ ] Performance ve repeat-open ölçülür; Android eşikleri kör kopyalanmaz.
- [ ] Signed archive/IPA süreci, privacy ve open-source notices doğrulanır.
- [ ] iPad mevcutsa layout/tablet smoke; yoksa açık test boşluğu.

Test: iOS Release corpus + lifecycle + artifact inventory.  
Çıkış: iOS RC gerçek cihazda blocker’sız; signed archive ve compliance snapshot vardır.

### AŞAMA 25 — Cross-platform beta ve yalnız blocker düzeltmeleri

**Amaç:** Scope creep olmadan gerçek kullanım geri bildirimiyle release’i sertleştirmek.

İşler:

- [ ] İzinli küçük beta grubu veya kullanıcı cihazlarında günlük mimari/statik dosyalar denenir.
- [ ] Geri bildirim formatı: fixture hash, platform/build, reproduce, compatibility report, beklenen/gerçek.
- [ ] Yalnız crash, veri gizliliği, P0 fidelity, açma/lifecycle ve ciddi performans blocker’ları düzeltilir.
- [ ] Yeni özellik/edit/export/XREF crawler bu aşamaya alınmaz.
- [ ] Her düzeltmede hedefli test; milestone sonunda tam corpus.

Test: Fix bazlı T1/T2; kapanışta T4.  
Çıkış: Açık release blocker yok; bilinen sınırlamalar güncel.

### AŞAMA 26 — Dependency freeze, final audit ve RC onayı

**Amaç:** Store’a/sunuma gidecek exact iki artifact’i dondurmak.

İşler:

- [ ] Dependency/toolchain freeze; lockfile ve resolved graph diff sıfır.
- [ ] Android+iOS full corpus, lifecycle, performance ve signed artifact smoke son kez çalışır.
- [ ] Gerçek APK/AAB/IPA/archive inventory; SBOM; license/source/native/font/asset evidence karşılaştırılır.
- [ ] Unknown/rejected dependency, analytics/upload, debug endpoint, secret veya proprietary asset aranır.
- [ ] Privacy, store, target SDK/Xcode ve Autodesk trademark yönergeleri `[LIVE-VERIFY]` edilir.
- [ ] Release notes, compatibility matrix, privacy policy ve support metni final olur.

Test: T4 tam final gate.  
Çıkış: İki platform RC `GREEN`; herhangi bir unknown = NO-GO.

### AŞAMA 27 — v1 artifact, yayın/handoff ve kapanış

**Amaç:** Çalışan uygulamayı tekrar üretilebilir biçimde teslim etmek.

İşler:

- [ ] Final Android APK/AAB ve iOS archive/IPA, checksum ve build talimatları üretilir.
- [ ] Kullanıcı store hesaplarını sağladıysa submission yapılır; sağlamadıysa store-ready paket/checklist teslim edilir ve yayın alt hedefi açıkça blokeli kalır.
- [ ] Clean machine/CI locked restore + build/test yolu doğrulanır.
- [ ] Version/tag/release snapshot yöntemi kullanıcı onayıyla uygulanır; otomatik push yapılmaz.
- [ ] Kullanım, privacy, compatibility, third-party notices, known limitations ve support belgeleri tamamlanır.
- [ ] Bu plan checkpoint’i `DONE`; aşağıdaki Definition of Done tek tek işaretlenir.

Test: Final install/open/close smoke ve checksum doğrulaması.  
Çıkış: Gerçek Android ve iOS cihazda çalışan, exact kaynaklardan yeniden üretilebilen, denetlenmiş ücretsiz viewer v1 teslim edilmiştir.

---

## 8. Risk kaydı ve zorunlu tepkiler

| Risk | Erken sinyal | Zorunlu tepki |
|---|---|---|
| ACadSharp belirli dosyada fidelity kaybı | Golden/count/notification farkı | Sürüm A/B, fixture issue; kullanıcı warning; sistematikse parser gate yeniden açılır |
| ProCad lineage/preview/olgunluk riski | Fork diff, restore mismatch, crash/fidelity | NO-GO veya upstream patch; özel renderer ancak yeniden maliyetlendirme ve kullanıcı GO kararıyla |
| Renderer eforu büyür | P0 handler eksikleri | P0’ı bitir, P1/P2’yi warning ile ertele; edit/export ekleme |
| SHX/font yerleşimi bozulur | Missing/substitution ve text extents diff | Görünür uyarı, kullanıcı font importu, audited fallback; proprietary bundle yok |
| Büyük çizimde OOM/ANR | PSS/frame/primitive artışı | Guard + controlled ret; profiler tabanlı culling/cache; largeHeap son çare |
| Test corpus lisans/gizlilik sorunu | Provenance yok | Repodan çıkarma değil önce dağıtımı durdurma; private/ignored taşıma kullanıcı onayıyla; evidence düzeltme |
| Android/iOS platform farkı | AOT/font/URI/native crash | Platform adapter; shared core’u fork etmeme; ayrı release gate |
| Mac/iPhone/store hesabı yok | Aşama 08/23’te erişim yok | `BLOCKED`; Android’i bozma veya iOS’u simülatörle tamamlandı sayma; erteleme ancak kullanıcının açık risk kabulüyle plan revizyonudur |
| Unknown native/transitive asset | Artifact inventory eşleşmiyor | Release NO-GO; kaynak/lisans bulunana veya bileşen çıkarılana kadar dur |
| Dependency terk edilir | Release/issue activity düşer | Pinned sürüm + source archive; adapter sayesinde kontrollü fork/alternatif spike |
| Marka/policy değişir | Store review veya yeni guideline | Release gününde yalnız resmi kaynakla yeniden doğrula |
| Scope creep | Edit/export/cloud isteği v1’e girer | Backlog’a taşı; viewer DoD bitmeden başlatma |

---

## 9. Definition of Done — uygulama ne zaman gerçekten bitti?

Plan yalnız aşağıdakilerin tamamı gerçek kanıtla sağlandığında tamamlanır:

- [ ] Android ve iOS gerçek cihazda local DWG ve DXF açıyor.
- [ ] P0 geometry/block/text/dimension/hatch kabul matrisi geçiyor.
- [ ] Pan/pinch/fit/layer toggle ve lifecycle stabil.
- [ ] Unsupported/proxy/font/XREF/raster sorunları sessiz değil.
- [ ] Sertifikalı corrupt/adversarial corpus crash/ANR yerine kontrollü davranıyor; yakalanamayan process-fatal runtime sınırları belgelenmiş.
- [ ] Final performans/bellek hedefleri referans cihazlarda kaydedilmiş ve geçilmiş veya dürüst kontrollü limit uygulanmış.
- [ ] Full corpus regresyonu iki platform Release artifact’i için geçiyor.
- [ ] Original drawing immutable; cloud/upload/account zorunluluğu yok.
- [ ] Runtime dependency/native/font/asset zincirinde unknown veya policy-RED bileşen yok.
- [ ] APK/AAB/IPA inventory, SBOM, notices ve release evidence eşleşiyor.
- [ ] CAD SDK/API için per-user/per-file/runtime royalty veya zorunlu servis ücreti saptanmamış.
- [ ] Uygulama v1’de kullanıcı için ücretsiz; core özellik paywall arkasında değil.
- [ ] Signed/store-ready artifact, checksum, build ve kullanım belgeleri teslim edilmiş.
- [ ] Bilinen compatibility sınırları yayımlanabilir metinde dürüstçe belirtilmiş.

“Tüm DWG’leri AutoCAD ile piksel piksel aynı gösterir” bir DoD değildir ve vaat edilmez.

---

## 10. v1 sonrası backlog — bu plan bitmeden başlanmaz

Öncelik sırası:

1. Read-only entity selection ve properties
2. Ölçüm: mesafe/alan/koordinat; unit doğrulamasıyla
3. Kullanıcı-granted proje klasöründen tam XREF resolution
4. İleri paper-space/complex linetype/underlay
5. PDF/SVG export için ayrı fidelity ve lisans spike
6. Command/undo-redo tabanlı editor
7. Save-as-copy ve DWG/DXF round-trip corpus; orijinal üzerine yazma yine varsayılan kapalı

Bu özellikler v1 dependency graph’ına feature flag ile bile gizlice eklenmez; feature flag dependency exclusion değildir.

---

## 11. Resmi başlangıç kaynakları

Yürütme gününde live-verify edilir:

- ACadSharp repo/README/license: https://github.com/DomCR/ACadSharp
- ACadSharp NuGet: https://www.nuget.org/packages/ACadSharp/
- ACadSharp reader notifications: https://github.com/DomCR/ACadSharp/blob/master/docs/articles/samples/reading.md
- ProCad repo/license: https://github.com/wieslawsoltes/ProCad
- ProCad dependency/submodule kaynakları: https://github.com/wieslawsoltes/ProCad/blob/master/.gitmodules
- SkiaSharp repo/license: https://github.com/mono/SkiaSharp
- .NET MAUI 10 docs: https://learn.microsoft.com/dotnet/maui/?view=net-maui-10.0
- .NET MAUI installation: https://learn.microsoft.com/dotnet/maui/get-started/installation?view=net-maui-10.0
- .NET support policy: https://dotnet.microsoft.com/platform/support/policy
- IxMilia DXF: https://github.com/ixmilia/dxf
- IxMilia DWG: https://github.com/ixmilia/dwg
- IxMilia SHX: https://github.com/ixmilia/shx
- Android Storage Access Framework: https://developer.android.com/training/data-storage/shared/documents-files
- Android app memory guidance: https://developer.android.com/topic/performance/memory-overview
- Autodesk trademark guidance: https://www.autodesk.com/company/legal-notices-trademarks/trademarks/guidelines-for-use
- Apple Developer: https://developer.apple.com/programs/
- Google Play Console help: https://support.google.com/googleplay/android-developer/

---

## Nihai teknik ilke

> Doğrudan oku; cihazda işle; eksikliği saklama; önce doğruluğu kanıtla; sonra yalnız ölçülmüş darboğazı optimize et; final artifact’in tamamının kaynağını ve lisansını gösterebilmeden release yapma.
