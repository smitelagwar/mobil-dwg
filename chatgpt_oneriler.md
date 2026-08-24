# MOBİL DWG/DXF MASTER PLAN — SADECE ÖNERİLER

**İncelenen belge:** `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan(1).md`  
**Amaç:** Mevcut planı yeniden yazmak değil; yalnızca planı daha güvenli, daha net ve uzun vadede lisans/royalty sürprizi doğurmayacak hale getirmek için öneriler sunmak.

---

# GENEL DEĞERLENDİRME

Mevcut planın ana yönü korunabilir. Özellikle:

- ACadSharp tabanlı doğrudan DWG/DXF okuma,
- SkiaSharp tabanlı render,
- ProCad'i hazır kod kaynağı olarak değerlendirme,
- Android-first yaklaşımı,
- preview-first yaklaşımı,
- ücretli Autodesk/ODA/RealDWG çözümlerinden kaçınma,
- GPL/AGPL bağımlılıklarını runtime zincirinden uzak tutma,
- local/offline çalışma,
- gelecekte edit özelliğine açık mimari

doğru kararlar.

Aşağıdaki öneriler mevcut planı değiştirmekten çok, **lisans, dependency, release ve teknik doğrulama açısından daha sıkı hale getirmek** içindir.

---

# ÖNERİLER

## 1. “0 royalty” ifadesini mutlak hukuki garanti gibi yazma

Mevcut planda bazı cümleler çok kesin.

Örneğin:

```text
0 royalty
0 Autodesk SDK fee
0 per-file fee
```

bunlar ürün hedefi olarak doğru.

Ancak daha doğru yaklaşım:

> Her public release'te kullanılan exact dependency zinciri ve final APK/AAB/IPA artifact'i denetlenerek CAD görüntüleme nedeniyle kullanıcı başı, dosya başı, runtime, API veya SDK royalty'si bulunmadığı doğrulanır.

Böylece belge hukuki garanti vermek yerine doğrulanabilir teknik bir release kriteri tanımlar.

---

## 2. “Repo MIT ise sorun yok” yaklaşımından özellikle kaçınılmalı

Bir GitHub reposunun kök lisansının MIT olması tek başına yeterli kabul edilmemeli.

Ayrıca kontrol edilmeli:

- transitive NuGet dependency'ler,
- native `.so`, `.framework`, `.dylib` dosyaları,
- git submodule'ler,
- source-vendored kod,
- gömülü fontlar,
- icon/resource dosyaları,
- sample DWG/DXF dosyaları,
- image/PDF decoder'ları,
- text shaping kütüphaneleri.

Ana plana bu ayrım açıkça eklenmeli.

---

## 3. Final release artifact'i gerçek lisans doğrulama kaynağı kabul edilmeli

En önemli eklemelerden biri bu olmalı.

Şu kural önerilir:

> NuGet sayfası veya GitHub repo lisansı ön inceleme içindir. Nihai dağıtım açısından esas olan store'a gönderilecek gerçek APK/AAB/IPA içeriğidir.

Release artifact içinde gerçekten bulunan:

```text
.dll
.so
.framework
dylib
aar
jar
font
asset
embedded resource
```

dosyaları kontrol edilmeli.

---

## 4. SBOM eklenmeli

Mevcut plandaki:

```text
THIRD_PARTY_NOTICES.md
DEPENDENCY_LICENSES.md
```

iyi.

Buna ayrıca SBOM eklenmesini öneriyorum.

Tercihen:

```text
SPDX
veya
CycloneDX
```

formatında.

Amaç:

> Bir release'te uygulamanın içinde tam olarak hangi üçüncü taraf bileşenlerin olduğunu daha sonra tekrar kanıtlayabilmek.

---

## 5. License evidence dosyası eklenmeli

Her dependency için aşağıdaki bilgiler kaydedilmeli:

```text
component
version
commit
source URL
license
license file hash
package hash
runtime included?
native included?
notice required?
royalty/fee?
review result
```

Bu, gelecekte “bu dependency neden kabul edilmişti?” sorusunu ortadan kaldırır.

---

## 6. Exact version pinning daha sıkı olmalı

Mevcut plan dependency pinning konusunda doğru.

Ancak bunu daha açık hale getirmek faydalı olur.

Kullanılmaması önerilenler:

```text
latest
*
floating versions
>= version
```

Uygulamanın kendi dependency tanımlarında mümkün olduğunca exact version kullanılmalı.

Ayrıca:

```text
packages.lock.json
locked restore
```

kullanılması önerilir.

---

## 7. Package hash arşivlenmeli

Sadece:

```text
ACadSharp 3.x.x
```

yazmak yerine kullanılan gerçek NuGet paketinin hash'i de saklanabilir.

Aynı şekilde:

- source commit,
- package SHA-256,
- LICENSE SHA-256

kayıt altına alınabilir.

Bu, supply-chain açısından güçlü bir güvence olur.

---

## 8. ACadSharp için tek “approved version” kavramı eklenmeli

Ana planda ACadSharp sürümü doğrudan “stable version pin” şeklinde geçiyor.

Bunu:

```text
APPROVED_ACADSHARP_VERSION
```

kavramıyla daha açık hale getirmek iyi olur.

Yeni sürüm çıktı diye otomatik upgrade yapılmamalı.

Yeni sürüm:

- DWG corpus,
- DXF corpus,
- performance,
- memory,
- render regression,
- license audit

geçtikten sonra approved olmalı.

---

## 9. ACadSharp'ın read/write yetenekleri ayrı tutulmalı

Viewer tarafında bir DWG sürümünü okuyabilmek ile aynı sürümü kusursuz şekilde yazabilmek aynı şey değil.

Bu nedenle mevcut planın gelecekte edit/save kısmında:

```text
READ SUPPORT
WRITE SUPPORT
```

ayrı tablolar halinde düşünülmesi önerilir.

Özellikle editör aşamasında bu ayrım kritik.

---

## 10. ACadSharp source/submodule denetimi eklenmeli

NuGet dependency graph tek başına yeterli değil.

ACadSharp gibi projelerde source submodule veya compile edilen yardımcı kaynaklar olabilir.

Bu yüzden:

```text
NuGet dependency audit
+
source/submodule audit
```

ayrı ayrı yapılmalı.

---

## 11. ProCad kesin production dependency gibi yazılmamalı

Mevcut plan ProCad konusunda zaten temkinli.

Bu yaklaşım daha da güçlendirilmeli.

ProCad için statü:

```text
Experimental / Spike Candidate
```

gibi düşünülmeli.

Gerçek cihaz testlerinden sonra:

```text
Production Approved
```

statüsü almalı.

---

## 12. ProCad'in genç bir proje olduğu daha belirgin vurgulanmalı

ProCad çok değerli görünüyor çünkü:

- renderer,
- MAUI control,
- block,
- hatch,
- layout,
- SHX,
- hit-test,
- editing

gibi zor işleri hazır sağlayabilir.

Ancak genç olması nedeniyle:

- API stability,
- package stability,
- backwards compatibility,
- dependency churn,
- documentation maturity

ayrıca değerlendirilmelidir.

---

## 13. ProCad’in preview dependency riski açıkça yazılmalı

ProCad MAUI tarafında preview SkiaSharp MAUI dependency kullanımı bulunabiliyor.

Ana planda:

> Preview dependency kullanımı public release öncesinde özellikle değerlendirilmelidir.

şeklinde daha açık bir uyarı olmalı.

---

## 14. ProCad + ACadSharp arasında “tek ACadSharp otoritesi” kuralı eklenmeli

Bu önemli.

Projede aynı anda:

```text
official ACadSharp NuGet
+
ProCad içindeki başka ACadSharp source/fork/version
```

kullanılıyorsa sürüm/model çatışması doğabilir.

Öneri:

> Runtime'da yalnız bir authoritative ACadSharp lineage/version bulunmalı.

---

## 15. ProCad'in `>=` dependency yaklaşımı kontrol altına alınmalı

Bir upstream paket:

```text
ACadSharp >= X
```

diyorsa uygulama kendi build'inde exact versiyonu sabitlemeli.

Aksi halde aynı source code farklı zamanlarda farklı dependency ile restore edilebilir.

---

## 16. ProCad source olarak alınırsa commit pin zorunlu olmalı

NuGet yerine source kullanılırsa:

```text
branch main
```

üzerinden sürekli ilerlemek önerilmez.

Bunun yerine:

```text
exact commit SHA
```

kullanılmalı.

---

## 17. SkiaSharp için root MIT lisansının yeterli olmadığı belirtilmeli

Ana plan:

```text
SkiaSharp — MIT
```

diyor.

Bu doğru ama eksik.

SkiaSharp native grafik stack'inde farklı üçüncü taraf kütüphaneler bulunabilir.

Bu nedenle:

> SkiaSharp root lisansı MIT olsa da final Android/iOS native binary dependency'leri ayrıca denetlenmelidir.

ifadesi eklenmeli.

---

## 18. Native dependency license gate eklenmeli

Özellikle kontrol edilmeli:

```text
Skia native
FreeType
HarfBuzz
ICU
libpng
zlib
image codecs
text shaping libraries
```

Burada amaç bunların kötü olduğu değildir.

Ama her birinin:

- ticari dağıtım hakkı,
- attribution şartı,
- royalty durumu,
- notice şartı

ayrı bilinmeli.

---

## 19. “Royalty-free” ile “yükümlülüksüz” kavramları ayrılmalı

Önemli bir kavramsal düzeltme.

Örneğin bazı permissive lisanslar:

- ücret istemez,
- royalty istemez,
- ancak attribution/notice ister.

Dolayısıyla:

```text
royalty-free
≠
no obligations
```

ana plana eklenebilir.

---

## 20. License allowlist iki katmanlı düşünülebilir

Mevcut allowlist:

```text
MIT
Apache-2.0
BSD-2-Clause
BSD-3-Clause
ISC
0BSD
```

çok güvenli.

Ancak native grafik stack'inde:

```text
zlib
libpng
ICU
FreeType License
```

gibi başka permissive lisanslarla karşılaşılabilir.

Öneri:

### Tier A

Doğrudan kabul edilen:

```text
MIT
Apache-2.0
BSD
ISC
0BSD
```

### Tier B

Tek tek inceleme isteyen ama royalty-free/permissive olabilecek lisanslar.

Bu ayrım belgeyi daha gerçekçi hale getirir.

---

## 21. GPL/AGPL dışındaki reddedilen kaynaklar için AI kod provenance kuralı eklenmeli

AI/Codex ile geliştirme yapılacağı için önemli.

Kodlama ajanı:

- GPL repo kodunu kopyalamamalı,
- satır satır port etmemeli,
- method structure'ı birebir taşımamalı,
- testleri kopyalamamalı,
- büyük lookup tablolarını kopyalamamalı.

Yeni implementation için öncelik:

```text
official specification
documentation
permissive source
independent implementation
```

olmalı.

---

## 22. “GPL repo teknik fikir için incelenebilir” cümlesi biraz daha sıkılaştırılmalı

Mevcut yaklaşım lisans açısından gereksiz risk bırakıyor.

Daha güvenli ifade:

> Reddedilen lisanslı kaynaklar implementation kaynağı olarak kullanılmaz. Teknik davranış mümkünse format specification, resmi dokümantasyon veya permissive implementation üzerinden öğrenilir.

---

## 23. Font lisans politikası yalnız SHX ile sınırlı olmamalı

Kontrol edilmesi gerekenler:

```text
SHX
TTF
OTF
font data
fallback fonts
icon fonts
```

Her bundled fontun redistribution hakkı bilinmeli.

---

## 24. SHX parser lisansı ile SHX font lisansı ayrılmalı

Örneğin MIT lisanslı bir SHX parser kullanmak:

> Kullanılan `.shx` font dosyasının da MIT olduğu

anlamına gelmez.

Bu ayrım plana açıkça yazılmalı.

---

## 25. Autodesk'e ait SHX/font paketleri bundle edilmemeli

Mevcut planda zaten doğru yönde bir madde var.

Bunu daha sert hale getirmek iyi olur:

> Autodesk veya başka CAD yazılımlarının kurulum klasörlerinden alınmış font/resource dosyaları uygulama paketine eklenmez.

---

## 26. Kullanıcının kendi SHX/font dosyasını seçmesi ile uygulamanın onu dağıtması ayrılmalı

Uygulama:

- kullanıcının cihazındaki fontu açabilir,
- local kullanabilir,

ancak onu kendi app bundle'ına dönüştürüp başka kullanıcılara dağıtmamalı.

---

## 27. PAT/hatch pattern dosyaları da lisanslı asset kabul edilmeli

Sadece font değil:

```text
PAT
CTB
STB
line type definitions
icons
textures
sample drawings
```

da provenance açısından izlenmeli.

---

## 28. Test DWG/DXF dosyalarının telif/provenance durumu eklenmeli

Public repo'ya koyulan her fixture için:

- kaynak,
- izin,
- lisans,
- hash

bilinmeli.

Müşteri/proje DWG'leri public repo'ya konulmamalı.

---

## 29. Private test corpus ile public fixture ayrılmalı

Önerilen kavramsal ayrım:

```text
Private engineering corpus
Public redistributable fixtures
```

Bu hem gizlilik hem telif açısından daha güvenli.

---

## 30. Golden reference screenshot'ların kullanımı sınırlandırılmalı

AutoCAD/TrueView screenshot'ları internal regression için yararlı olabilir.

Ancak:

- App Store screenshot,
- Google Play screenshot,
- public marketing,
- GitHub demo

olarak kullanılmadan önce ayrı IP/trademark değerlendirmesi yapılmalı.

---

## 31. Autodesk DWG marka konusu daha net yazılmalı

Mevcut plan marka konusunda doğru yönde.

Ancak şu ayrım özellikle yazılmalı:

```text
DWG formatıyla uyumlu yazılım yapmak
```

ile

```text
DWG'yi ürün markasının parçası yapmak
```

aynı şey değildir.

---

## 32. Uygulama adında DWG kullanmama yaklaşımı korunmalı

Özgün ürün adı seçmek en güvenli yol.

DWG/DXF:

- store açıklaması,
- desteklenen formatlar,
- teknik uyumluluk metni

içinde kullanılabilir.

---

## 33. “Autodesk ile ilişkili değiliz” tarzı disclosure değerlendirilmesi eklenebilir

Store release öncesinde güncel trademark guidance'a göre:

- Autodesk ile affiliation olmadığını,
- DWG'nin ilgili marka sahibine ait olduğunu

belirten uygun kısa disclosure düşünülebilir.

Bunun exact wording'i release tarihinde tekrar doğrulanmalı.

---

## 34. Marka kılavuzu yalnız proje başında değil, release sırasında tekrar kontrol edilmeli

Bu mevcut planda var ve kesinlikle korunmalı.

Çünkü marka politikaları zaman içinde değişebilir.

---

## 35. Dependency lisansı gelecekte değişirse eski pinned sürümün evidence'i kaybolmamalı

Her approved release için:

- kullanılan LICENSE dosyası,
- commit,
- package,
- hash

lokal/repo evidence içinde tutulmalı.

Sadece “GitHub'da MIT yazıyordu” şeklinde dış web sayfasına güvenilmemeli.

---

## 36. Dependency update sonrası license diff yapılmalı

Yeni sürüm upgrade edildiğinde yalnız:

```text
tests passed
```

yetmemeli.

Ayrıca:

```text
license changed?
new dependency?
removed dependency?
new native binary?
new font?
new asset?
```

kontrol edilmeli.

---

## 37. Dependency graph diff CI'a eklenmeli

Her CAD dependency güncellemesinde önceki release ile yeni release dependency graph karşılaştırılabilir.

Unexpected dependency çıkarsa review zorunlu olmalı.

---

## 38. Unknown license = release blocker kuralı önerilir

Bir dependency veya asset için lisans belirlenemiyorsa:

```text
UNKNOWN
```

olarak bırakılmamalı.

Karar:

```text
UNKNOWN = NO-GO
```

olmalı.

---

## 39. Unknown native binary = release blocker olmalı

APK/IPA içinde kaynağı bilinmeyen:

```text
.so
framework
dylib
```

varsa release durdurulmalı.

---

## 40. Third-party notice üretimi mümkün olduğunca otomatikleştirilmeli

Elle güncelleme unutulabilir.

CI mümkün olduğunca:

- dependency inventory,
- license metadata,
- notice generation

işlerini otomatikleştirmeli.

Manuel review yine korunmalı.

---

## 41. Compliance klasörü eklenebilir

Repo içinde ayrı:

```text
compliance/
```

klasörü olması düzen açısından faydalı.

Burada:

```text
policy
approved dependencies
rejected dependencies
third-party notices
SBOM
license evidence
release evidence
```

tutulabilir.

---

## 42. Release başına compliance snapshot tutulmalı

Örneğin:

```text
release-1.0.0
release-1.0.1
release-1.1.0
```

için ayrı evidence.

Böylece eski release'in hangi dependency zinciriyle oluşturulduğu kaybolmaz.

---

## 43. “Final binary inspection” checklist'e eklenmeli

Mevcut zero-royalty checklist iyi ama şu maddeler eklenmeli:

```text
final APK inspected
final AAB inspected
final IPA inspected
native binaries mapped
fonts mapped
assets mapped
SBOM generated
unknown components = 0
```

---

## 44. ProCad'in yalnız gereken modülleri kullanılmalı

Viewer için gerekli olmayan modüller runtime'a alınmamalı.

Amaç:

```text
minimum dependency surface
```

olmalı.

Her gereksiz modül:

- app size,
- attack surface,
- dependency count,
- license burden,
- regression risk

arttırır.

---

## 45. Editing modülleri ilk viewer release'e dahil edilmemeli

Edit-ready mimari korunabilir.

Ancak edit runtime dependency'leri daha ilk viewer APK'sına koymak gereksiz olabilir.

Feature flag kapalı olsa bile binary içinde dependency bulunuyorsa lisans ve attack surface yükü devam eder.

---

## 46. Feature flag ile dependency exclusion aynı şey değildir

Önemli nokta:

```text
EnableEditing = false
```

demek edit dependency'sinin binary'de olmadığı anlamına gelmez.

Bu nedenle gerçekten gerekmeyen modüller build'ten çıkarılmalı.

---

## 47. PDF underlay ileride eklenirse ayrı lisans denetimi yapılmalı

“CAD dışı özellik” diye lisans firewall dışında bırakılmamalı.

PDF renderer:

- open-source license,
- native code,
- redistribution,
- patent/codec,
- notice

açısından ayrıca denetlenmeli.

---

## 48. Raster image decoder'ları da aynı firewall'dan geçmeli

JPEG/PNG/WebP/TIFF gibi destekler eklenirse bunların native decoder'ları da dependency inventory'ye girmeli.

---

## 49. HarfBuzz/text shaping ayrı dependency olarak görünür hale getirilmeli

Text render kalitesi için önemli.

Ancak “Skia'nın parçası” diye gözden kaçırılmamalı.

---

## 50. Telemetry default olarak kapalı tutulabilir

Mevcut offline/privacy yaklaşımı güçlü.

Daha da net:

```text
default cloud upload = none
default CAD telemetry = none
default analytics = none
```

olabilir.

---

## 51. Crash reporting zorunlu SaaS bağımlılığı haline getirilmemeli

İleride crash reporting eklenirse:

- free tier,
- usage limits,
- privacy,
- SDK license,
- gelecekte ücret riski

incelenmeli.

Viewer'ın çalışması bu servise bağlı olmamalı.

---

## 52. Core CAD path tamamen local kalmalı

Şu yol:

```text
Open
Parse
Render
Pan
Zoom
Layer
```

hiçbir şekilde:

- server,
- license server,
- paid API,
- cloud conversion,
- Autodesk account,
- ODA account

istememeli.

Bu mevcut planda doğru ve korunmalı.

---

## 53. Server yalnız opsiyonel feature için bile eklenirse core'dan ayrılmalı

İleride:

- sync,
- collaboration,
- cloud backup

eklenirse CAD viewer core dependency'si haline gelmemeli.

---

## 54. Android debug başarısı production başarısı sayılmamalı

Gerçek cihaz testi doğru.

Ancak ayrıca:

```text
Android Debug
Android Release
Android AAB
```

ayrı doğrulanmalı.

---

## 55. iOS AOT/trimming riski ayrı yazılmalı

.NET/MAUI parser/render stack:

- reflection,
- dynamic type loading,
- resource discovery

kullanıyorsa iOS AOT/trimming davranışı değişebilir.

Bu nedenle iOS release build ayrıca test edilmeli.

---

## 56. Android/iOS aynı CAD core'u kullanmalı ama native farklar ayrıca test edilmeli

Aynı C# core önemli.

Ancak:

- font loading,
- file URI handling,
- native Skia,
- memory,
- AOT,
- GPU behavior

platforma göre farklı olabilir.

---

## 57. Parser crash boundary korunmalı

DWG trusted input kabul edilmemeli.

Mevcut security bölümü doğru.

Ek olarak fuzz/corrupt corpus düşünülebilir.

---

## 58. Recursive block limit yalnız güvenlik değil DoS koruması olarak da düşünülmeli

Kötü hazırlanmış DWG:

- çok derin INSERT recursion,
- devasa hatch,
- milyonlarca entity,
- malformed extents

ile uygulamayı kilitleyebilir.

Mevcut guard yaklaşımı korunmalı ve benchmark edilmeli.

---

## 59. File size guard tek başına yeterli değil

1 MB DWG bile çok büyük expanded scene oluşturabilir.

Bu yüzden:

```text
file size
entity count
block expansion
raster size
scene primitive count
RAM estimate
```

birlikte değerlendirilmeli.

---

## 60. Dynamic block desteği ayrı compatibility seviyesi olarak görülebilir

ProCad destekliyorsa değerli.

Ancak normal block/INSERT doğruluğu ile aynı release blocker seviyesinde olmak zorunda değil.

---

## 61. Proxy/custom entity davranışı kullanıcıya açık olmalı

Viewer custom entity'yi görüntüleyemiyorsa sessizce yok saymak teknik olarak yanıltıcı olabilir.

Warning summary iyi fikir ve korunmalı.

---

## 62. “Çizim açıldı” ile “teknik olarak güvenilir” ayrı acceptance kriterleri olmalı

Mevcut plan bunu zaten büyük ölçüde yapıyor.

Daha da net:

```text
Parse success
Render success
Engineering fidelity
```

ayrı ölçülmeli.

---

## 63. Dimension doğruluğu kritik kalmalı

Dimension yalnız görsel detay değildir.

Yanlış dimension çizmek teknik kullanıcı için tehlikeli olabilir.

Bu yüzden P0/P1 önceliği korunmalı.

---

## 64. Text fallback kullanıcıya görünür olmalı

Eksik font varsa:

> benzer font kullanıldı

uyarısı mevcut plandaki gibi korunmalı.

Çünkü text genişliği/yerleşimi değişebilir.

---

## 65. SHX fallback ölçüsel yerleşimi bozabilir

Fallback font yalnız karakteri göstermekle kalmaz.

- width,
- baseline,
- alignment,
- text extents

değişebilir.

Golden testlerde bu da karşılaştırılmalı.

---

## 66. Layout/paper-space MVP sonrasına bırakılabilir ama compatibility raporunda görünmeli

Bir DWG yalnız layout üzerinden anlamlıysa model-space açılması kullanıcı için eksik olabilir.

Bu nedenle desteklenmiyorsa uyarı verilmeli.

---

## 67. XREF için remote URL auto-fetch yasağı korunmalı

Hem privacy hem security hem maliyet açısından doğru.

Ek olarak:

> local sibling file resolution

opsiyonel olarak düşünülebilir.

---

## 68. XREF dosyaları da ayrı trusted/untrusted input olarak parse edilmeli

Ana dosya güvenlik kuralları XREF'e de uygulanmalı.

---

## 69. Raster image path traversal kontrolü eklenmeli

External raster reference:

```text
../../...
```

gibi yollarla beklenmeyen dosyalara erişmeye çalışmamalı.

---

## 70. Sample benchmark hedefleri garanti gibi yazılmamalı

Mevcut:

```text
<5 MB ≈ 2 s
5–20 MB ≈ 5 s
```

gibi hedefler benchmark target olarak kalmalı.

SLA/garanti gibi algılanmamalı.

---

## 71. TTFUP metriği kesinlikle korunmalı

Bu plandaki en iyi performans fikirlerinden biri.

Kullanıcı açısından:

> Dosya tamamen hazır olmadan ne kadar sürede anlamlı bir çizim görüyorum?

çok önemli.

---

## 72. Progressive preview yalnız gerçek ihtiyaç varsa uygulanmalı

Mevcut planın “önce benchmark, sonra karmaşıklık ekle” yaklaşımı doğru.

Korunmalı.

---

## 73. Spatial index implementation sıfırdan yazılmadan önce ProCad tekrar kontrol edilmeli

Bu genel “hazır kod kullan” ilkesine uyuyor.

---

## 74. SceneGraph abstraction korunmalı

ACadSharp entity'lerini UI'ya doğrudan bağlamamak doğru.

Bu:

- parser değişimi,
- caching,
- testing,
- future edit

için çok değerli.

---

## 75. RenderScene yalnız derived model olarak kalmalı

Gerçek document state:

```text
CadDocument
```

olmalı.

Bu gelecek editör için doğru temel.

---

## 76. Stable entity identity korunmalı

Selection, measurement, edit, undo/redo için gerekli.

---

## 77. Original file immutable yaklaşımı kesinlikle korunmalı

Gelecekte writer geldiğinde:

```text
Save As Copy
```

önce kullanılmalı.

Bu en güvenli politika.

---

## 78. DWG writer gelecekte ayrı approval almalı

Viewer'da kullanılan parser approved oldu diye writer otomatik approved kabul edilmemeli.

---

## 79. Round-trip test yalnız entity count ile sınırlı olmamalı

Kontrol:

- blocks,
- layers,
- handles,
- styles,
- text,
- dimensions,
- layouts,
- extents,
- proxy/custom data

içermeli.

---

## 80. Unsupported write version sessizce başka DWG sürümüne dönüştürülmemeli

Kullanıcıya hedef format açıkça gösterilmeli.

---

## 81. “AutoCAD uyumlu” gibi iddialar çok dikkatli kullanılmalı

Gerçek test matrisi neyi destekliyorsa yalnız o kadar iddia edilmeli.

---

## 82. Compatibility matrix public dokümana dönüştürülebilir

Örneğin:

```text
DWG R14 — tested
DWG 2000 — tested
DWG 2007 — tested
...
```

Bu kullanıcı beklentisini doğru yönetir.

---

## 83. “%100 tüm DWG'ler” gibi iddialardan kaçınılmalı

Mevcut plan bunu doğru şekilde yapıyor.

Korunmalı.

---

## 84. Civil 3D / Architecture proxy objeleri ayrı kategoride kalmalı

Normal 2D mimari/statik hedefi bloklamamalı.

---

## 85. Production dependency listesi çok kısa tutulmalı

Viewer'ın ilk release'i için amaç:

> mümkün olan en az third-party dependency ile maksimum render doğruluğu.

Bu hem lisans hem stabilite açısından avantajlı.

---

## 86. Yeni dependency eklemek “özellik eklemek” kadar ciddi kabul edilmeli

Her dependency:

- security,
- license,
- maintenance,
- size,
- compatibility

yükü getirir.

---

## 87. Otomatik dependency updater doğrudan merge yapmamalı

Dependabot/Renovate benzeri araç kullanılabilir ancak CAD/render paketlerinde yalnız PR açmalı.

Merge:

- test,
- visual regression,
- license diff

sonrası yapılmalı.

---

## 88. Release candidate sonrası dependency freeze yapılmalı

Store'a gidecek build belirlendikten sonra dependency değiştirilmemeli.

Değişirse yeniden regression/compliance çalışmalı.

---

## 89. Release build reproducible olmaya çalışmalı

Aynı source + lockfile mümkün olduğunca aynı dependency setini üretmeli.

Bu uzun vadeli bakım için çok değerlidir.

---

## 90. App bundle içindeki license/notice dosyalarının kullanıcıya erişilebilir olması düşünülebilir

Örneğin:

```text
Ayarlar
→ Açık Kaynak Lisansları
```

sayfası.

Bu hem attribution hem şeffaflık açısından iyi olabilir.

---

## 91. THIRD_PARTY_NOTICES yalnız repo içinde bırakılmamalı

License şartı gerekiyorsa app distribution içinde de uygun şekilde sunulmalı.

Exact yöntem kullanılan lisanslara göre belirlenmeli.

---

## 92. Legal/compliance review ayrı release checklist maddesi olabilir

Bu mutlaka ücretli hukuk hizmeti demek değildir.

Ama:

```text
technical license review complete
```

şeklinde bağımsız bir check bulunması iyi olur.

---

## 93. “Tamamen ücretsiz” ifadesinin kapsamı tek yerde tanımlanmalı

Belgede farklı bölümlerde tekrar tekrar anlatmak yerine başta çok net:

> CAD viewer teknolojisi bakımından per-user/per-file/runtime/SDK/API royalty veya zorunlu ücret yok.

şeklinde tanımlanmalı.

Store ücretleri bir kez kapsam dışında belirtilip konu kapatılabilir.

---

## 94. Kullanıcı sayısı arttığında maliyet doğurmayan mimari hedefi özellikle korunmalı

Core viewer tamamen local olduğu için:

```text
10 kullanıcı
10.000 kullanıcı
10.000.000 kullanıcı
```

arasında CAD backend maliyeti değişmemeli.

Bu ürünün en güçlü yönlerinden biri.

---

## 95. Uygulamanın ücretsiz olması ile open source olması ayrılmalı

Plan:

- ücretsiz dağıtım,
- proprietary application code

seçeneğini açık bırakıyor.

Bu nedenle permissive dependency tercihinin mantığı doğru.

---

## 96. LGPL/MPL reddi politika tercihi olarak yazılmalı, hukuki zorunluluk gibi değil

Daha doğru ifade:

> Bu lisanslar bazı koşullarda kullanılabilir olabilir; ancak projenin lisans yönetimini minimumda tutma hedefi nedeniyle policy gereği varsayılan olarak reddedilir.

Bu ifade daha teknik ve doğru.

---

## 97. “Commercial use allowed” tek kriter olmamalı

Ayrıca:

```text
redistribution allowed?
modification allowed?
binary distribution allowed?
notice required?
source disclosure?
network/source requirement?
royalty?
field-of-use restriction?
```

kontrol edilmeli.

---

## 98. License scanner çıktısına kör güvenilmemeli

Otomatik scanner:

- yardımcı araçtır,
- nihai karar değildir.

Özellikle:

- dual license,
- generated source,
- vendored native source

manuel review isteyebilir.

---

## 99. Dual-license dependency için seçilen lisans açıkça kaydedilmeli

Bir dependency:

```text
MIT OR GPL
```

gibi dual-license ise hangi lisans altında kullanıldığı evidence dosyasına yazılmalı.

---

## 100. “0 CAD fee” hedefi planın en üstteki değiştirilemez şartlarından biri olarak kalmalı

Bu bütün teknik seçimlerde filtre görevi görmeli.

Her yeni öneri için soru:

> Bu teknoloji uygulama büyüdüğünde CAD dosyası açmak için sonradan zorunlu lisans/royalty/API maliyeti çıkarabilir mi?

Cevap:

```text
evet
belirsiz
```

ise default karar RED olmalı.

---

# EN KRİTİK 12 ÖNERİNİN KISA ÖZETİ

Ana planı yapan ChatGPT özellikle aşağıdaki 12 noktayı plana işlemeli:

1. `0 royalty` ifadesini release evidence ile doğrulanan kriter olarak tanımla.
2. Root MIT lisansına güvenmek yerine transitive/native/source/asset zincirini denetle.
3. Final APK/AAB/IPA'yı nihai compliance kaynağı yap.
4. SBOM oluştur.
5. Exact dependency pin + lockfile + locked restore kullan.
6. ACadSharp için tek approved exact version tanımla.
7. ProCad'i production core değil, spike-first candidate olarak tut.
8. ProCad + ACadSharp için tek authoritative ACadSharp lineage/version kullan.
9. SkiaSharp native dependency lisanslarını ayrıca denetle.
10. Font/SHX/PAT/sample DWG/DXF dahil bütün asset'lerde provenance tut.
11. GPL/AGPL/rejected source code'un AI tarafından port edilmesini engelleyen source firewall ekle.
12. Release öncesi unknown dependency/native binary/license varsa otomatik NO-GO uygula.

---

# SONUÇ

Mevcut ana planın teknik yönünü baştan tasarlamak gerekmiyor.

En önemli geliştirme:

> Planın “permissive açık kaynak kullanıyoruz” seviyesinden, “store'a gönderilen gerçek uygulamanın içindeki bütün kod, native binary ve asset'lerin kaynağını ve lisansını kanıtlayabiliyoruz” seviyesine çıkarılmasıdır.

Bu yapıldığında:

- ACadSharp,
- ProCad,
- SkiaSharp,
- .NET MAUI

hattı çok daha kontrollü değerlendirilebilir ve kullanıcının asıl şartı olan:

> **DWG/DXF dosyası açtığı için ileride Autodesk/ODA/CAD SDK/API/royalty ödemek zorunda kalmamak**

hedefine çok daha sağlam şekilde yaklaşılır.

Bu dosyanın amacı yeni bir plan oluşturmak değildir. Buradaki maddeler yalnızca mevcut master planın eleştirilmesi, sıkılaştırılması ve revize edilmesi için önerilerdir.
