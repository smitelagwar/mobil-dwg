# MOBİL 2D CAD (DWG/DXF) VIEWER

## Tamamen Ücretsiz / Royalty-Free CAD Teknoloji Yığınıyla Android + iOS Ana Planı

### Android-first • iOS-ready • Preview-first • Future-editor-ready

**Plan tarihi:** 24 Ağustos 2026  
**Proje durumu:** Sıfırdan proje  
**Ana hedef:** 2D mimari ve mühendislik DWG/DXF projelerini Android ve iOS cihazlarda hızlı, doğru, sade ve mümkün olduğunca yerel/offline biçimde açan bir mobil uygulama geliştirmek.  
**Birinci öncelik:** Ön izleme kalitesi ve güvenilirliği.  
**İkinci uzun vadeli hedef:** Aynı mimariyi bozmadan daha sonra seçme, ölçme ve gerçek CAD düzenleme özellikleri ekleyebilmek.  
**Temel ticari şart:** Autodesk RealDWG, Autodesk APS/Forge, ODA ticari SDK, ücretli dönüştürme API'si, kullanıcı başına lisans, dosya başına ücret, bulut dönüşüm ücreti veya gelecekte royalty doğurabilecek herhangi bir CAD çekirdeğine bağımlı olunmayacak.

---

# 0. BU BELGENİN AMACI

Bu MD dosyası yalnızca fikir vermek için değil, projenin ana teknik ve ürün standardı olarak kullanılmak üzere hazırlanmıştır.

Belge başka bir yapay zekâya, Codex benzeri kodlama ajanına veya geliştiriciye verildiğinde aşağıdaki konuları yeniden açıklamak gerekmemelidir:

- uygulamanın neden geliştirildiği,
- hangi kullanıcı problemine odaklandığı,
- neden DWG → DXF dönüşümünün ana yol olmadığı,
- hangi açık kaynak kütüphanelerin kullanılacağı,
- hangi lisansların kabul edilip edilmeyeceği,
- Android ve iOS'un nasıl destekleneceği,
- neden Android gerçek cihaz testlerinin en baştan yapılacağı,
- 2D CAD render motorunun nasıl kurulacağı,
- mimari/statik mühendislik çizimlerinde hangi entity'lerin kritik olduğu,
- gelecekte editörün nasıl eklenebileceği,
- performans, bellek, font, XREF ve layout sorunlarının nasıl ele alınacağı,
- yayınlama sırasında Autodesk veya CAD SDK lisans ücreti çıkmaması için hangi sınırların korunacağı.

Bu plan gerektiğinde başka güçlü modellere verilerek eleştirilebilir. Ancak aşağıdaki **değiştirilemez ürün şartları**, kullanıcı açıkça değiştirmedikçe korunmalıdır.

---

# 1. DEĞİŞTİRİLEMEZ ÜRÜN ŞARTLARI

## 1.1 Ücretli CAD teknolojisi kullanılmayacak

Dağıtılan uygulamanın runtime zincirinde aşağıdakiler **olmayacak**:

- Autodesk RealDWG
- Autodesk APS / Forge tabanlı DWG conversion
- ücretli Autodesk API
- ODA ticari SDK
- ücretli DWG parser
- ücretli server-side conversion hizmeti
- dosya başına ödeme
- aylık CAD API aboneliği
- kullanıcı başına lisans
- royalty
- trial SDK
- yalnızca ticari lisansla yayınlanabilen bileşen
- "non-commercial only" lisanslı bileşen

Amaç:

> Uygulama 1 kullanıcıya da 10 milyon kullanıcıya da ulaştığında CAD dosyasını açtığı için üçüncü taraf bir CAD sağlayıcısına kullanım ücreti doğmamalıdır.

---

## 1.2 Copyleft/GPL bağımlılığı dağıtılan uygulama çekirdeğine alınmayacak

Bu projede gelecekte kaynak kodu açma, App Store dağıtımı veya lisans uyumluluğu konusunda sürpriz yaşamamak için dağıtılan runtime'da varsayılan olarak şu lisanslar kabul edilmeyecektir:

- GPL
- AGPL
- SSPL
- BUSL
- "source available"
- "non-commercial"
- lisansı belirsiz repo
- özel/proprietary lisans

Ayrıca LGPL/MPL gibi daha sınırlı copyleft lisanslar hukuken kullanılabilir olabilse de bu projenin **"lisans konusu ile ileride uğraşmama"** hedefi nedeniyle varsayılan olarak reddedilecektir.

Ana tercih:

- MIT
- BSD-2-Clause
- BSD-3-Clause
- Apache-2.0
- ISC
- 0BSD
- açıkça ticari kullanıma ve dağıtıma izin veren benzer permissive lisanslar

Bu nedenle önceki LibreDWG/MLightCAD-GPL merkezli mimari bu yeni planda **ana üretim yolu değildir**.

---

# 2. "TAMAMEN ÜCRETSİZ" NE ANLAMA GELİYOR?

Burada üç farklı maliyet birbirine karıştırılmamalıdır.

| Kalem | Hedef |
| --- | --- |
| DWG/DXF parser lisansı | **0 USD** |
| CAD renderer lisansı | **0 USD** |
| Autodesk lisansı/API | **0 USD** |
| Dosya başına dönüşüm | **0 USD** |
| Sunucu zorunluluğu | **Yok** |
| Kullanıcı başına royalty | **0 USD** |
| Uygulama içi CAD SDK royalty | **0 USD** |
| Android cihazda geliştirme/test | Ücretsiz yapılabilir |
| Google Play geliştirici hesabı | Mağazanın kendi ücreti olabilir |
| Apple Developer Program | Mağazanın kendi yıllık ücreti olabilir |
| iOS build için Mac/Xcode | Apple platform gereksinimi |

Önemli ayrım:

> **CAD teknolojisinin tamamı royalty-free ve ücretsiz olabilir; fakat Google Play ve Apple App Store kendi geliştirici hesabı/yayınlama ücretlerini ayrıca uygulayabilir.**

Bu ücretler Autodesk veya DWG teknolojisiyle ilgili değildir.

24 Ağustos 2026 itibarıyla standart yayınlama tarafında:

- Google Play geliştirici hesabında tek seferlik kayıt ücreti bulunmaktadır.
- Apple Developer Program'da yıllık üyelik ücreti bulunmaktadır.
- Apple'ın uygun kurumlar için ayrı fee-waiver programları olabilir.
- Mağaza politikaları değişebileceğinden yayınlama tarihinde yeniden kontrol edilmelidir.

Uygulamanın kendisi kullanıcıya **ücretsiz** yayınlanabilir.

Eğer "hiçbir mağaza hesabına dahi para ödememe" şartı oluşursa:

- Android APK doğrudan ücretsiz dağıtılabilir.
- standart App Store dağıtımı Apple'ın güncel üyelik koşullarına bağlıdır.

---

# 3. PROJENİN ASIL KULLANIM ALANI

Bu uygulamanın ilk hedef kitlesi genel amaçlı AutoCAD klonu değildir.

İlk odak:

## Mimari projeler

- kat planları
- mimari akslar
- duvarlar
- kapılar
- pencereler
- mahal yazıları
- ölçülendirmeler
- taramalar
- bloklar
- layer'lar
- paftalar
- layout/paper space

## İnşaat mühendisliği projeleri

- kalıp planları
- kolon aplikasyon planları
- temel planları
- perde/kolon/kiriş çizimleri
- kiriş açılımları
- donatı detayları
- kesitler
- detay paftaları
- akslar
- kotlar
- ölçüler
- yoğun text/MTEXT
- block/INSERT kullanımı
- hatch
- lineweight
- layer

Dolayısıyla ilk sürümde AutoCAD'in tüm 2D/3D ekosistemini desteklemek amaçlanmayacaktır.

Başarı kriteri:

> Normal 2D mimari ve mühendislik DWG/DXF projelerini telefonda güvenilir, hızlı ve okunabilir biçimde açmak.

---

# 4. ANA MİMARİ KARAR

Yeni projenin ana yığını:

```text
.NET 10 LTS
        ↓
.NET MAUI
        ↓
Android + iOS native uygulama kabuğu
        ↓
ACadSharp
(MIT DWG/DXF parser + CAD veri modeli)
        ↓
RenderScene / SceneGraph ara katmanı
        ↓
SkiaSharp
(MIT 2D renderer)
        ↓
Telefon/tablet ekranı
```

Hazır kodu daha fazla kullanabilmek için ilk değerlendirme:

```text
ProCad
  ├─ ProCad.IO
  ├─ ProCad.Rendering
  ├─ ProCad.Controls.Skia
  ├─ ProCad.Controls.Maui
  └─ ileride ProCad.Editing
```

ProCad de MIT lisanslıdır ve ACadSharp + SkiaSharp + MAUI hattına çok yakın hazır bir mimari sunmaktadır.

---

# 5. NEDEN ARTIK DWG → DXF ANA YOL DEĞİL?

Önceki fikir:

```text
DWG
 ↓
DXF'e dönüştür
 ↓
DXF tekrar parse edilir
 ↓
render
```

Yeni ana yol:

```text
DWG
 ↓
ACadSharp doğrudan okur
 ↓
CadDocument
 ↓
RenderScene
 ↓
SkiaSharp
 ↓
ekran
```

Bu nedenle:

- ara `.dxf` üretmek yok,
- ikinci parse yok,
- gereksiz disk I/O yok,
- dosya boyutunu büyüten ara format yok,
- daha temiz veri modeli,
- gelecekte edit için aynı `CadDocument` kullanılabilir.

DXF desteği ayrıca doğal olarak bulunacaktır:

```text
DWG ──┐
      ├─→ ACadSharp → ortak CAD veri modeli → renderer
DXF ──┘
```

Yani tek viewer iki formatı da açacaktır.

---

# 6. ANA DWG/DXF MOTORU: ACadSharp

Repo:

<https://github.com/DomCR/ACadSharp>

Lisans:

**MIT**

Ana görevleri:

- DWG read
- DWG write
- DXF ASCII read/write
- DXF binary read/write
- CAD entities
- blocks
- layers
- layouts
- styles
- document modification

Bu proje açısından en önemli özelliği:

> Autodesk RealDWG kullanmadan DWG dosyasını doğrudan C#/.NET ortamında okuyabilmesi ve MIT lisanslı olması.

---

# 7. ACadSharp DWG SÜRÜM HEDEFİ

İlk test corpus'u en az şu yaygın DWG ailelerini kapsamalıdır:

- AC1014 / R14
- AC1015 / R2000
- AC1018 / R2004
- AC1021 / R2007
- AC1024 / R2010
- AC1027 / R2013
- AC1032 / R2018 family

İlk ürün hedefi esas olarak güncel 2D mühendislik/mimari dosyalarıdır.

Eski, çok özel veya custom CAD nesneleri ayrı compatibility sınıfı olarak ele alınacaktır.

---

# 8. ÇOK ÖNEMLİ: ACadSharp PARSER'DIR, TEK BAŞINA VIEWER DEĞİLDİR

ACadSharp:

```text
DWG bytes
 ↓
CadDocument
```

işini büyük ölçüde yapar.

Ancak:

```text
CadDocument
 ↓
ekranda AutoCAD'e benzer çizim
```

ayrı bir problemdir.

Bu nedenle uygulamanın başarısında parser kadar renderer önemlidir.

Özellikle:

- nested block
- hatch
- dimensions
- text
- MTEXT
- SHX
- linetype
- lineweight
- ByLayer/ByBlock
- paper space
- viewport
- raster
- underlay

gibi özellikler kaliteli bir render katmanı gerektirir.

Bu nedenle sıfırdan renderer yazmadan önce ProCad incelenecektir.

---

# 9. ÇOK DEĞERLİ AÇIK KAYNAK PROJE: ProCad

Repo:

<https://github.com/wieslawsoltes/ProCad>

Lisans:

**MIT**

Araştırma sırasında bu proje özellikle önem kazanmıştır çünkü doğrudan bizim düşündüğümüz teknoloji hattına yakındır.

Belgelenen yapı:

```text
ACadSharp
   ↓
ProCad.IO
   ↓
ProCad.Rendering
   ↓
ProCad.Controls.Skia
   ↓
ProCad.Controls.Maui
   ↓
Android / iOS
```

Ayrıca geleceğe yönelik:

```text
ProCad.Editing
```

katmanı bulunmaktadır.

Repo aşağıdakiler için hazır kod/altyapı içerir:

- model space
- paper space/layout
- viewport
- linetype
- hatch
- raster image
- PDF underlay
- dynamic block
- XREF
- SHX text
- ACIS ile ilişkili entity handling
- hit testing
- text shaping
- rendering caches
- dimensions
- inserts
- polylines
- MText
- MLeader
- selection/editing altyapısı
- MAUI control
- Skia renderer

Bu nedenle **ProCad ilk teknik spike'ın P0 araştırma kaynağıdır.**

---

# 10. ProCad'E KÖR BAĞLANILMAYACAK

ProCad çok faydalı görünse de 2026 itibarıyla görece genç bir projedir.

Dolayısıyla:

**YANLIŞ yaklaşım:**

```text
Projeyi komple ProCad'e kilitle.
```

**DOĞRU yaklaşım:**

```text
1. ProCad MAUI viewer Android'de gerçek DWG ile çalışıyor mu?
2. Render doğruluğu yeterli mi?
3. Performansı iyi mi?
4. Bağımlılıkları tamamen permissive mi?
5. Production için preview NuGet dependency var mı?
6. Gerekli modülleri ayrı kullanabiliyor muyuz?
```

test edilir.

Başarırsa:

- büyük miktarda kod doğrudan yeniden kullanılabilir.

Başaramazsa:

- ACadSharp korunur,
- SkiaSharp korunur,
- kendi ince RenderScene katmanımız oluşturulur,
- ProCad'den yalnızca MIT lisanslı ve işimize yarayan mimari/fikir/kod parçaları uygun attribution ile uyarlanabilir.

---

# 11. ANA UI/MOBİL FRAMEWORK: .NET MAUI

Öneri:

**.NET 10 LTS + .NET MAUI 10**

Neden?

- ACadSharp C#/.NET.
- ProCad .NET.
- SkiaSharp .NET.
- Android ve iOS tek codebase.
- native app packaging.
- native file picker.
- native lifecycle.
- Android gerçek cihaz debug.
- gelecekte native paylaşım/open-with.
- editör özellikleri için daha sürdürülebilir tek dil/tek mimari.

Bu projede webview tabanlı çözüm yerine .NET MAUI daha doğal hale gelmiştir çünkü ana permissive DWG parser ACadSharp'tır.

---

# 12. RENDER MOTORU: SkiaSharp

Repo:

<https://github.com/mono/SkiaSharp>

Lisans:

**MIT**

SkiaSharp:

- Android
- iOS
- macOS
- Windows

üzerinde çalışan yüksek performanslı 2D grafik API'sidir.

Bu uygulamanın viewport'u:

```text
SKCanvasView
veya uygun GPU-backed Skia view
```

üzerinden kurulabilir.

Render motorunun görevi:

- dünya koordinatı → ekran koordinatı
- pan
- zoom
- clip
- culling
- line
- polyline
- arc
- ellipse
- bezier/spline approximation
- fill/hatch
- text
- block transform
- lineweight
- linetype
- colors
- selection overlay
- future grips

---

# 13. NEDEN SceneGraph / RenderScene ARA KATMANI GEREKLİ?

ACadSharp entity'leri doğrudan UI'de çizdirilmemelidir.

Ana mimari:

```text
DWG/DXF
  ↓
ACadSharp
  ↓
CadDocument
  ↓
SceneBuilder
  ↓
RenderScene
  ↓
SkiaRenderer
```

Böylece:

### Performans

CadDocument her frame tekrar tekrar dolaşılmaz.

### Cache

- block geometry
- hatch
- text
- linetype
- transformed shapes

cache edilebilir.

### Future edit

Document değişince yalnızca ilgili render primitive yeniden oluşturulabilir.

### Parser bağımsızlığı

İleride ACadSharp değişse bile renderer UI bütünüyle yeniden yazılmaz.

### Test

RenderScene bağımsız snapshot testine tabi tutulabilir.

---

# 14. TEMEL KATMANLAR

Önerilen mimari:

```text
App
│
├─ Presentation
│  ├─ Home
│  ├─ FileOpen
│  ├─ Viewer
│  ├─ Layers
│  └─ Settings
│
├─ Cad.Application
│  ├─ CadSession
│  ├─ OpenDocument
│  ├─ CloseDocument
│  ├─ ErrorHandling
│  └─ Metrics
│
├─ Cad.Document
│  ├─ ICadDocumentReader
│  ├─ ACadSharpAdapter
│  ├─ DocumentInfo
│  └─ CompatibilityReport
│
├─ Cad.Scene
│  ├─ SceneBuilder
│  ├─ RenderScene
│  ├─ RenderPrimitive
│  ├─ SpatialIndex
│  └─ Cache
│
├─ Cad.Rendering
│  ├─ SkiaRenderer
│  ├─ Camera2D
│  ├─ Culling
│  ├─ Text
│  └─ Theme
│
├─ Cad.Interaction
│  ├─ Pan
│  ├─ PinchZoom
│  ├─ FitExtents
│  ├─ HitTest
│  └─ Selection
│
└─ Cad.Editing
   ├─ ICommand
   ├─ UndoRedo
   ├─ EditTransaction
   └─ SaveService
```

`Cad.Editing` ilk sürümde kullanıcıya açılmayacaktır.

---

# 15. PREVIEW-FIRST KURALI

İlk sürüm:

**CAD VIEWER**

olacaktır.

**CAD EDITOR** olmayacaktır.

Ön izleme tamamlanmadan edit özelliklerine geçmek yasaktır.

Ön izleme için kalite sırası:

```text
1. Doğru geometri
2. Stabil açılış
3. Hız
4. Mobil pan/zoom
5. Text/font doğruluğu
6. block/hatch/dimension doğruluğu
7. layout/paper space
8. büyük dosya performansı
9. UI
10. edit
```

---

# 16. MVP ÖZELLİKLERİ

## Olacak

- `.dwg` aç
- `.dxf` aç
- tamamen local/offline işlem
- model space
- fit extents
- pan
- pinch zoom
- double tap zoom/fit
- layer listesi
- layer show/hide
- drawing background light/dark
- original CAD colors
- lineweight görüntüleme
- drawing information
- dosya adı
- loading state
- warning state
- unsupported entity özeti
- missing font özeti
- missing reference özeti
- close/reopen
- Android back handling
- landscape
- portrait
- tablet

## ProCad güvenilir şekilde destekliyorsa MVP'ye alınabilecek

- layouts
- paper space
- viewport
- raster image
- XREF resolution
- PDF underlay

Ancak bunlar ana dosyanın açılmasını geciktirmemelidir.

---

# 17. MVP'DE OLMAYACAK

- çizgi çizme
- trim
- extend
- offset
- move
- copy
- rotate
- text edit
- dimension edit
- save DWG
- cloud sync
- hesap
- kullanıcı üyeliği
- proje yönetimi
- yorum sistemi
- collaboration
- Autodesk login
- BIM
- 3D model editor
- AutoCAD ribbon klonu

---

# 18. GELECEKTE EDITÖR EKLENECEĞİ İÇİN BUGÜNDEN YAPILACAKLAR

Editör UI'si şimdilik olmayacak fakat mimari editörü engellemeyecek.

## 18.1 Document authoritative olacak

Gerçek veri:

```text
CadDocument
```

olacak.

RenderScene yalnızca türetilmiş/cache edilmiş görsel model olacak.

## 18.2 Stable entity identity

Seçim ve undo/redo için entity kimlikleri korunmalı.

## 18.3 Hit-test katmanı

İlk sürümde kullanıcıya seçim UI'si açılmasa bile hit-test altyapısı bağımsız tutulmalı.

## 18.4 Command pattern

Gelecekte:

```text
MoveEntityCommand
DeleteEntityCommand
ChangeLayerCommand
EditTextCommand
```

şeklinde komutlar.

## 18.5 Undo / redo

Doğrudan entity mutate eden dağınık UI kodu yazılmamalı.

## 18.6 Dirty-region / scene invalidation

Bir entity değiştiğinde bütün çizim baştan parse edilmemeli.

## 18.7 Save ayrı servis

```text
IFileWriter
DwgWriter
DxfWriter
```

UI'den bağımsız tutulmalı.

---

# 19. GELECEKTE DWG SAVE KONUSUNDA ÇOK ÖNEMLİ KURAL

ACadSharp DWG writer yetenekleri sunuyor.

Ancak:

> Bir dosyayı okumak ile AutoCAD uyumlu biçimde kaydedip hiçbir veriyi bozmamak aynı zorlukta değildir.

Dolayısıyla editör geldiğinde:

### İlk politika

**Orijinal dosyanın üzerine yazma YOK.**

Yalnızca:

```text
Farklı Kaydet
```

kullanılmalı.

### Sonra

DWG round-trip regression corpus oluşturulmalı.

Test:

```text
original.dwg
 ↓ read
CadDocument
 ↓ write
roundtrip.dwg
 ↓ read again
compare
```

Kontrol:

- entity count
- layers
- blocks
- geometry
- handles
- styles
- extents
- text
- dimensions
- layouts

Custom/proxy entity varsa kullanıcı uyarılmalı.

---

# 20. CAD ENTITY ÖNCELİK MATRİSİ

## P0 — Mutlaka doğru görünmeli

- LINE
- ARC
- CIRCLE
- ELLIPSE
- LWPOLYLINE
- POLYLINE
- SPLINE
- TEXT
- MTEXT
- INSERT
- nested INSERT
- ATTRIB
- DIMENSION
- HATCH
- POINT
- SOLID

## P1 — Mühendislik projeleri için yüksek önem

- LEADER
- MLEADER
- TABLE
- VIEWPORT
- IMAGE
- WIPEOUT
- XLINE
- RAY
- MLINE
- TOLERANCE

## P2 — Compatibility

- proxy/custom entities
- advanced underlays
- ACIS
- application-specific objects
- Civil 3D custom objects
- Architecture toolset custom objects

P2 entity'ler ilk sürümün ana hedefini bloke etmeyecek.

---

# 21. BLOCK / INSERT MOTORU

Mimari ve statik DWG'lerde block kullanımı çok yaygındır.

Renderer:

- nested blocks
- translation
- rotation
- scale X/Y
- mirror
- ByBlock color
- ByLayer color
- attributes

doğru uygulamalıdır.

Aynı block 5.000 defa INSERT edilmişse 5.000 ayrı ağır geometry üretmek yerine shared/cache yaklaşımı kullanılmalıdır.

---

# 22. TEXT / MTEXT / TÜRKÇE

Türkçe mühendislik projelerinde:

- Ğ/ğ
- İ/ı
- Ş/ş
- Ç/ç
- Ö/ö
- Ü/ü

kesin test edilir.

Kontrol:

- font family
- text height
- width factor
- rotation
- alignment
- justification
- multiline
- rich MTEXT formatting mümkün olduğu ölçüde
- mirrored text
- block attribute text

---

# 23. SHX FONT STRATEJİSİ

DWG'de en büyük kalite risklerinden biri SHX'tir.

Kural:

- Autodesk'e ait/proprietary SHX font paketleri uygulamayla birlikte izinsiz dağıtılmayacak.
- yalnızca lisansı açıkça uygun fontlar bundle edilecek.
- eksik font varsa fallback uygulanacak.
- kullanıcı ileride kendi font dosyasını seçebilecek.
- font cihazda lokal tutulacak.

Araştırılacak permissive repo:

<https://github.com/ixmilia/shx>

Lisans:

MIT

Ancak ProCad'in mevcut SHX pipeline'ı önce test edilmeli; gereksiz ikinci parser yazılmamalı.

---

# 24. HATCH

Hatch hem görsel doğruluk hem performans için kritik.

Özellikle:

- concrete hatch
- ANSI patterns
- solid fill
- dense hatch
- custom pattern
- clipping
- island

test edilir.

Uzak zoom seviyesinde çok yoğun hatch render'ı LOD ile azaltılabilir.

Öncelik:

> Kullanıcı çizimi uzaktan gezerken FPS kaybetmemeli.

---

# 25. DIMENSION

Mühendislik projelerinde ölçülendirme kritik.

Test:

- linear
- aligned
- angular
- radius
- diameter
- ordinate mümkünse
- dimension style
- arrows
- text override
- scale
- block transform

Dimension render hatası çizimi teknik açıdan yanıltabileceği için basit "yaklaşık çizim" yaklaşımı kullanılmamalıdır.

---

# 26. LAYER / COLOR / LINEWEIGHT

Destek:

- layer name
- on/off
- freeze
- lock yalnız görüntü bilgisi olarak
- ACI color
- true color
- ByLayer
- ByBlock
- lineweight
- linetype

Viewer seçenekleri:

```text
Renkler:
○ Gerçek renkler
○ Siyah-beyaz
```

ileride eklenebilir.

Lineweight:

```text
○ Açık
○ Kapalı
```

kullanıcı tercihi olabilir.

Ancak ilk MVP'de gerçek renk + lineweight doğru render edilmelidir.

---

# 27. MODEL SPACE / PAPER SPACE

Mühendislik paftaları bazen model space'de değil layout üzerinden okunur.

Bu nedenle uzun vadede:

- Model
- Layout1
- Layout2
- ...

sekmesi desteklenmelidir.

ProCad'in paper-space/layout desteği Android spike sırasında özellikle test edilmelidir.

Eğer ilk MVP'de layout renderer yeterli değilse:

- Model Space release blocker olmayabilir,
- ancak roadmap'te P1 olarak kalmalıdır.

---

# 28. XREF

Gerçek projelerde:

```text
ana.dwg
  ↓
mimari.dwg
statik.dwg
harita.dwg
```

gibi XREF bulunabilir.

İlk davranış:

- ana DWG açılır.
- eksik XREF crash üretmez.
- kullanıcıya:
  `2 dış referans bulunamadı`
  uyarısı çıkar.

İleride:

```text
Eksik dosyaları eşleştir
```

özelliği.

Güvenlik:

XREF içindeki remote URL otomatik download edilmez.

---

# 29. RASTER / PDF UNDERLAY

Öncelik ikinci planda.

Destekleniyorsa kullan.

Desteklenmiyorsa:

- ana çizim açılmaya devam eder.
- underlay eksik uyarısı verilir.

PDF renderer eklemek için ücretli SDK kullanılmayacaktır.

---

# 30. DOSYA AÇMA — MOBİL

## Android

Sistem file picker / Storage Access Framework.

Akış:

```text
FilePicker
 ↓
content URI
 ↓
uygulama geçici/cache alanına güvenli stream/copy
 ↓
ACadSharp
```

Dosya doğrudan uzun süreli platform URI stream'ine bağlanmamalıdır.

## iOS

UIDocumentPicker / MAUI FilePicker.

Benzer şekilde dosyanın lifecycle'ı kontrol edilmelidir.

---

# 31. OPEN WITH / PAYLAŞ MENÜSÜ

MVP sonrasında:

Kullanıcı:

```text
WhatsApp / Files / Dosyalar / Drive
 ↓
plan.dwg
 ↓
"Şununla aç"
 ↓
uygulamamız
```

yapabilmeli.

Android:

- intent filters
- MIME/extension handling

iOS:

- document type association

Ancak ilk teknik spike'ı geciktirmemeli.

---

# 32. PARSE UI THREAD'DE YAPILMAYACAK

ACadSharp read işlemi:

```text
UI thread
```

üzerinde uzun süre çalıştırılmamalıdır.

Örnek mimari:

```text
await Task.Run(() => reader.Read(...))
```

veya uygun background pipeline.

Main thread:

- loading UI
- gesture
- lifecycle

için serbest kalmalı.

Aynı anda birden fazla büyük DWG parse edilmemeli.

Default:

```text
ParseConcurrency = 1
```

---

# 33. PERFORMANS ÖLÇÜMÜ

Her açılışta debug telemetry lokal olarak ölçülebilir:

```text
T0 user selected file
T1 local stream/copy ready
T2 parse started
T3 CadDocument ready
T4 RenderScene ready
T5 first useful frame
T6 full fidelity frame
```

Ek:

- file size
- entity count
- layer count
- block count
- memory
- peak memory
- scene primitives
- draw time
- FPS

En önemli metrik:

**TTFUP — Time To First Useful Preview**

---

# 34. BAŞLANGIÇ PERFORMANS HEDEFLERİ

Bunlar garanti değil, başlangıç benchmark hedefidir.

Gerçek Android telefon testlerinden sonra güncellenecek.

| Dosya | İlk anlamlı preview hedefi |
| --- | --- |
| < 5 MB | yaklaşık ≤ 2 sn |
| 5–20 MB | yaklaşık ≤ 5 sn |
| 20–50 MB | yaklaşık ≤ 10 sn veya progressive |
| 50+ MB | cihaz/bellek profiline göre |

Daha önemli kriter:

> Uygulama crash olmamalı.

---

# 35. PROGRESSIVE PREVIEW

Eğer scene build büyük dosyalarda ağırsa:

```text
1. temel line/polyline/arc/circle
2. blocks
3. text/dimension
4. hatch
5. underlay/detail
```

aşamaları değerlendirilebilir.

Ancak ACadSharp/ProCad mevcut pipeline'ı ilk önce benchmark edilmeden karmaşık progressive engine yazılmayacaktır.

---

# 36. VIEWPORT CULLING

Skia canvas'a çizimin tamamını her frame gönderme.

Camera viewport dışındaki entity'ler:

```text
skip
```

edilmeli.

Büyük drawing için spatial index:

- R-tree
- quadtree
- bounding box index

değerlendirilebilir.

ProCad'de hazır hit-test/spatial/caching altyapısı varsa önce o kullanılacaktır.

---

# 37. LOD

Uzak zoom'da:

- çok küçük text
- yoğun hatch
- mikroskobik dimension detail
- tiny entities

geçici olarak azaltılabilir.

Pan/pinch bittikten sonra tam kalite render.

Ama kullanıcı mühendislik çizimi okuduğu için detay kalıcı olarak kaybedilmemelidir.

---

# 38. CACHE

## Render cache

- blocks
- hatch paths
- shaped text
- linetype
- reusable paths

## File cache

İlk MVP:

- recent files metadata
- thumbnail

İleri:

- file hash
- prepared scene cache

Ancak cache invalidation basit tutulmalıdır.

---

# 39. MEMORY

Dosya boyutu tek başına gerçek RAM tüketimi değildir.

Örneğin:

```text
20 MB DWG
```

parse + document + scene + text + paths ile çok daha yüksek RAM kullanabilir.

Önlemler:

- tek parse
- dispose
- scene lifecycle
- image size limits
- cache limits
- block sharing
- avoid duplicate arrays
- background memory cleanup
- low-memory handling

---

# 40. ANDROID GERÇEK TELEFON ANA TEST PLATFORMUDUR

Kullanıcının kendi Android telefonu ilk günden test döngüsüne dahil olacaktır.

Akış:

```text
Kod
 ↓
Android debug build
 ↓
USB / wireless debug
 ↓
gerçek telefon
 ↓
gerçek DWG
 ↓
gözlem + profiler
```

Emulator yalnız yardımcıdır.

Release kalitesi yalnız emulator'a göre belirlenmez.

---

# 41. ANDROID TEST MATRİSİ

En az:

### Cihaz

- kullanıcının ana Android telefonu
- mümkünse orta segment ikinci cihaz
- mümkünse tablet

### Ekran

- portrait
- landscape

### Drawing

- küçük
- orta
- büyük

### Lifecycle

- background
- foreground
- screen rotate
- memory pressure
- tekrar dosya aç
- hızlı art arda file selection

---

# 42. iOS GELİŞTİRME GERÇEĞİ

.NET MAUI ile kodun büyük kısmı ortak olabilir.

Ancak iOS build/sign için Apple toolchain gerekir.

Normal olarak:

- macOS
- Xcode
- Apple signing

gerekecektir.

Kendi iPhone'unda geliştirme/test tarafında ücretsiz Apple hesabıyla belirli test imkanları bulunabilir.

Fakat App Store dağıtımı Apple Developer Program üyeliği gerektirir.

Bu:

> **CAD teknolojisinin ücretli olması değildir; Apple'ın platform dağıtım maliyetidir.**

Planın hiçbir noktasında bunu gizlememek gerekir.

---

# 43. GOOGLE PLAY YAYINLAMA

Uygulama ücretsiz olabilir.

CAD engine:

- ücretsiz
- local
- royalty-free

olacaktır.

Ancak Google Play geliştirici hesabının güncel kayıt koşulları ayrıca vardır.

Release zamanında:

- target SDK
- privacy
- data safety
- store listing
- app signing

gereksinimleri yeniden kontrol edilir.

---

# 44. AUTODESK MARKA KURALI

DWG formatıyla uyumluluk geliştirmek ile Autodesk markasını ürün adı gibi kullanmak aynı şey değildir.

Bu nedenle uygulamanın markası nötr olmalıdır.

Örneğin:

**YANLIŞ / RİSKLİ**

```text
Super DWG Viewer
DWG Mobile Pro
Autodesk Viewer X
AutoCAD Mobile Clone
```

gibi DWG/Autodesk markasını ürün markasının ana unsuru haline getirmekten kaçınılmalıdır.

Daha doğru:

```text
[özgün uygulama adı]

Açıklama:
"2D CAD drawings compatible with DWG and DXF files."
```

Autodesk veya AutoCAD logosu kullanılmaz.

Store metni yayınlanmadan hemen önce Autodesk'in güncel trademark guideline'ı tekrar okunur.

---

# 45. OFFLINE / PRIVACY

Default:

```text
DWG/DXF
 ↓
cihaz içinde
 ↓
parser
 ↓
renderer
```

Dosya dışarı çıkmaz.

Olmayacak:

- analytics'e drawing text gönderme
- server upload
- Autodesk cloud
- conversion cloud
- automatic XREF fetch
- kullanıcı projesini loglama

Bu özellikle mühendislik projeleri için önemli bir ürün avantajıdır.

---

# 46. SECURITY

DWG binary input güvenilir kabul edilmemeli.

Kurallar:

- extension'a kör güvenme
- file size guard
- parser exception boundary
- corrupted file handling
- timeout/cancellation
- path traversal önleme
- XREF remote URL auto-fetch yok
- raster resource limit
- malformed geometry limits
- recursive block depth guard
- huge entity count guard

Crash yerine kontrollü hata.

---

# 47. ERROR TAXONOMY

Örnek:

```text
UnsupportedVersion
CorruptDrawing
UnsupportedEntity
MissingFont
MissingXref
MissingRaster
ParseFailed
SceneBuildFailed
OutOfMemoryRisk
Cancelled
Unknown
```

Kullanıcıya teknik stack trace gösterilmez.

---

# 48. KULLANICI HATA MESAJLARI

## Eksik entity

> Çizim açıldı ancak bazı özel CAD nesneleri görüntülenemedi.

## Font

> Çizim açıldı. 2 font bulunamadığı için benzer font kullanıldı.

## XREF

> Çizim açıldı. 3 dış referans bulunamadı.

## Bozuk dosya

> Dosya okunamadı. DWG/DXF dosyası bozuk veya desteklenmeyen bir sürüm olabilir.

## Bellek

> Bu çizim cihazın kullanılabilir belleği için çok büyük.

---

# 49. TEST CORPUS

Tek örnek DWG ile geliştirme yapılmayacak.

## Boyut

- <1 MB
- 1–5 MB
- 5–20 MB
- 20–50 MB
- 50–100 MB
- 100+ MB deneysel

## Proje tipi

- mimari kat planı
- kalıp planı
- kolon aplikasyon
- temel
- kiriş açılımı
- donatı detay
- kesit
- detay paftası
- yoğun block
- yoğun hatch
- yoğun dimension
- çok layer
- paper space
- XREF
- SHX
- raster

---

# 50. GOLDEN REFERENCE

Her kritik test çiziminin referans görüntüsü bulunmalı.

Karşılaştırma:

- extents
- layer count
- entity count
- blocks
- geometry
- text
- dimensions
- hatches
- colors
- lineweight
- layout

Mümkünse screenshot diff.

AutoCAD/TrueView yalnız geliştirme doğrulama referansı olarak kullanılabilir; uygulamanın runtime'ına dahil edilmez.

---

# 51. AUTOMATED REGRESSION

Her parser/renderer update'te:

- fixture açılıyor mu?
- entity count değişti mi?
- bounding box değişti mi?
- scene primitive count aşırı değişti mi?
- screenshot değişti mi?
- exception çıktı mı?

kontrol edilmeli.

---

# 52. LİSANS FIREWALL

Bu projenin en kritik yayınlama mekanizmalarından biri.

Yeni dependency eklendiğinde PR/commit checklist:

```text
[ ] Repo belli mi?
[ ] Lisans belli mi?
[ ] Ticari kullanıma izin veriyor mu?
[ ] Redistribution serbest mi?
[ ] Runtime royalty var mı?
[ ] Transitive dependency kontrol edildi mi?
[ ] GPL/AGPL var mı?
[ ] Non-commercial şart var mı?
[ ] Notice gerekiyor mu?
[ ] THIRD_PARTY_NOTICES güncellendi mi?
```

Lisansı belirsiz paket:

**REDDİ.**

---

# 53. İZİN VERİLEN DEFAULT LİSANSLAR

Allowlist başlangıcı:

```text
MIT
Apache-2.0
BSD-2-Clause
BSD-3-Clause
ISC
0BSD
```

Diğer lisanslar tek tek inceleme gerektirir.

Bu konservatif politika bilerek seçilmiştir.

---

# 54. RUNTIME'DA OLMAYACAK PROJELER

Bu yeni stratejide aşağıdaki projeler araştırma/referans olarak faydalı olsa da dağıtılan runtime'ın ana dependency'si yapılmayacaktır:

- LibreDWG — GPL
- MLightCAD LibreDWG DWG converter — GPL zinciri
- GPL tabanlı DWG converter'lar
- libdxfrw — GPL
- LibreCAD runtime code — GPL
- ticari ODA SDK
- Autodesk RealDWG

Ama açık algoritma/teknik fikir araştırması sırasında lisans sınırları gözetilerek incelenebilir.

---

# 55. THIRD_PARTY_NOTICES

Repo içinde:

```text
THIRD_PARTY_NOTICES.md
DEPENDENCY_LICENSES.md
```

bulunmalı.

Örnek:

```text
ACadSharp
MIT
https://github.com/DomCR/ACadSharp

SkiaSharp
MIT
https://github.com/mono/SkiaSharp

.NET MAUI
MIT
https://github.com/dotnet/maui

ProCad
MIT
https://github.com/wieslawsoltes/ProCad
```

Her version pinlenmeli.

---

# 56. DEPENDENCY PINNING

Production'da kontrolsüz:

```text
*
latest
floating
```

kullanılmamalı.

CAD engine ve renderer upgrade:

- ayrı commit/PR
- test corpus
- regression
- license check
- Android device benchmark

sonrası merge.

---

# 57. ProCad TRANSITIVE DEPENDENCY AUDIT

ProCad MIT olsa bile yalnız root repo lisansına bakmak yetmez.

Özellikle:

- ACadSharp
- SkiaSharp
- MAUI
- HarfBuzzSharp
- diğer runtime paketleri

ayrı ayrı incelenmelidir.

İlk spike'ın çıkış kriterlerinden biri:

> Dağıtılan Android APK içindeki tüm runtime bağımlılıklarının kabul edilen lisans listesine uyması.

---

# 58. PREVIEW/NIGHTLY PACKAGE RİSKİ

ProCad'in güncel geliştirme ağacında bazı package sürümleri preview olabilir.

Production release:

- mümkünse stable dependency
- veya tam pinlenmiş ve uzun testten geçmiş sürüm

kullanmalıdır.

Preview package otomatik olarak kötü değildir; fakat ilk public release'te gereksiz risk alınmayacaktır.

---

# 59. PROJE REPO YAPISI

Öneri:

```text
src/
├─ MobileCad.App/
├─ MobileCad.Application/
├─ MobileCad.Document/
├─ MobileCad.Scene/
├─ MobileCad.Rendering/
├─ MobileCad.Interaction/
├─ MobileCad.Editing/
└─ MobileCad.Platform/

tests/
├─ MobileCad.Document.Tests/
├─ MobileCad.Scene.Tests/
├─ MobileCad.Rendering.Tests/
├─ MobileCad.Golden.Tests/
└─ fixtures/

docs/
├─ ARCHITECTURE.md
├─ COMPATIBILITY.md
├─ PERFORMANCE.md
├─ LICENSE_POLICY.md
└─ THIRD_PARTY_NOTICES.md
```

---

# 60. CadSession

Viewer state merkezi olmalı.

Örnek kavram:

```csharp
public interface ICadSession
{
    CadDocument Document { get; }
    RenderScene Scene { get; }
    Camera2D Camera { get; }

    Task OpenAsync(string path);
    Task CloseAsync();

    void FitExtents();
    void SetLayerVisibility(string id, bool visible);
}
```

Gerçek API uygulama sırasında güncel kütüphanelere göre düzenlenir.

---

# 61. PARSER ABSTRACTION

UI doğrudan:

```csharp
DwgReader
```

çağırmamalıdır.

Araya:

```csharp
ICadDocumentReader
```

konulmalı.

Örneğin:

```text
ACadSharpReader
```

implementation.

Böylece ileride parser değiştirmek gerekirse UI etkilenmez.

---

# 62. RENDERER ABSTRACTION

Aynı şekilde:

```text
IRenderSceneBuilder
ICadRenderer
```

katmanları.

ProCad kullanılıyorsa adapter üzerinden uygulamaya bağlanabilir.

Böylece ProCad upstream değişirse tüm UI çökmemiş olur.

---

# 63. FEATURE FLAGS

Gelecek edit özellikleri:

```text
EnableSelection
EnableMeasurement
EnableEditing
EnableDwgSave
```

gibi merkezi capability sistemi ile yönetilebilir.

Preview release'te:

```text
EnableEditing = false
```

---

# 64. UI TASARIM PRENSİBİ

Masaüstü CAD programını telefona sıkıştırma.

Ana ekran:

```text
[ Dosya Aç ]

Son Açılanlar
```

Viewer:

```text
← Dosya adı             ⋮

       DRAWING

Fit     Layers     Görünüm
```

Daha fazlası gerekiyorsa bottom sheet.

---

# 65. MOBİL GESTURE

- pinch → zoom
- drag → pan
- double tap → zoom/fit opsiyonel
- iki parmak hareketleri çakışmasız
- long press ileride selection context
- screen edge safe area

Web sayfası scroll hissi kesinlikle olmamalı.

---

# 66. FUTURE MEASUREMENT

Editör öncesinde en mantıklı ikinci ürün fazı ölçüm olabilir.

- mesafe
- polyline length
- alan
- koordinat

Ancak ölçüm için drawing units doğru çözülmelidir.

Bu yüzden unit metadata başlangıçtan korunmalıdır.

---

# 67. FUTURE SELECTION

Hit test bugün scene mimarisine dahil edilirse ileride:

- entity tap
- properties
- layer info
- length
- area
- handle/id
- edit

kolaylaşır.

Fakat ilk UI'da selection şart değildir.

---

# 68. FUTURE EDIT AŞAMASI

Preview v1 stabil olduktan sonra ayrı proje fazı:

1. Select
2. Delete
3. Move
4. Copy
5. Rotate
6. Line/polyline
7. Text
8. Layer
9. Undo/redo
10. Save-as-copy

Daha sonra:

- trim
- extend
- offset
- dimension

---

# 69. ORIGINAL-FILE SAFETY

Edit özelliği geldiğinde:

```text
Original.dwg
```

varsayılan olarak immutable kabul edilir.

Kullanıcı açıkça istemedikçe overwrite yapılmaz.

Bu yaklaşım parser/writer edge-case'lerinde veri kaybını engeller.

---

# 70. AŞAMA 1 — LİSANS + ANDROID TEKNİK SPIKE

## Amaç

Tek satır ürün UI'si geliştirmeden önce şu hipotezi kanıtla:

> Tamamen permissive lisanslı .NET stack ile gerçek Android telefonda DWG doğrudan açılıp kaliteli çizilebiliyor.

## Görevler

1. Yeni temiz repo.
2. .NET 10 + MAUI 10.
3. ACadSharp stable version pin.
4. SkiaSharp stable version pin.
5. ProCad güncel commit/tag incele.
6. `ProCad.Controls.Maui` minimum viewer spike.
7. Android debug build.
8. Kullanıcının gerçek telefonuna yükle.
9. 5 gerçek DWG aç.
10. En az 1 DXF aç.
11. pan.
12. pinch.
13. fit extents.
14. block.
15. text.
16. dimension.
17. hatch.
18. layer.
19. transitive license audit.
20. APK runtime dependency audit.

## Bu aşamada yok

- güzel UI
- iOS
- edit
- recent files
- cloud
- account

## Çıkış kriteri

**GO**:

- gerçek telefonda açılıyor,
- temel mühendislik projesi anlaşılır/doğru,
- lisans zinciri kabul edilebilir,
- kritik crash yok.

**NO-GO / Fallback**:

ProCad yeterli değilse:

```text
ACadSharp
+
kendi ince RenderScene
+
SkiaSharp
```

yoluna geç.

**LibreDWG/GPL fallback'e geçme.**

Ücretsiz/permissive lisans şartı korunur.

---

# 71. AŞAMA 2 — ANDROID PREVIEW MVP

## Amaç

Spike'ı gerçek mobil viewer'a dönüştür.

## Görevler

- Home
- FilePicker
- CadSession
- loading
- error taxonomy
- Viewer
- pan/pinch
- fit
- layer panel
- theme
- file info
- close/dispose
- orientation
- recent files metadata
- warning summary

## Kabul

Kullanıcı telefonda günlük olarak proje açıp inceleyebilmeli.

---

# 72. AŞAMA 3 — MÜHENDİSLİK/MİMARİ ÇİZİM DOĞRULUĞU

## Amaç

"Görüntü açıldı" seviyesinden "güvenilir ön izleme" seviyesine çık.

## Odak

- nested blocks
- dimensions
- hatches
- MTEXT
- Turkish text
- SHX
- linetype
- lineweight
- ByLayer
- ByBlock
- layouts
- viewport
- XREF warning
- raster

## Kabul

Önceden tanımlı gerçek proje corpus'u golden reference'a karşı kabul sınırlarını geçmeli.

---

# 73. AŞAMA 4 — PERFORMANS / BÜYÜK DOSYA

## Amaç

Orta ve büyük mühendislik paftalarında akıcı viewer.

## Görevler

- profiler
- TTFUP
- parse background thread
- scene caching
- block cache
- text cache
- viewport culling
- spatial index
- LOD
- hatch optimizasyonu
- memory budget
- lifecycle
- repeat-open leak test

## Kabul

Gerçek Android cihazlarda benchmark tablosu çıkar.

---

# 74. AŞAMA 5 — ANDROID RELEASE HARDENING + iOS PORT

## Android

- release APK/AAB
- privacy
- crash safety
- dependency/license lock
- trademark review
- store listing
- app signing
- no debug code
- regression corpus

## iOS

- MAUI iOS build
- Mac/Xcode
- iPhone real device
- iPad
- file picker
- memory
- gestures
- lifecycle
- layout
- signing
- App Store requirements

## Kabul

Android ve iOS aynı core CAD pipeline'ı kullanmalı.

Platform-specific fork mümkün olduğunca küçük tutulmalı.

---

# 75. AŞAMA 6 — PREVIEW v1 SONRASI EDIT-READY PROGRAM

Bu aşama preview release'i geciktirmeyecek.

Önce:

- selection
- measurement
- properties

Sonra:

- command system
- undo/redo
- basic edit
- save-as-copy
- DWG/DXF roundtrip validation

Edit özellikleri ayrı capability olarak açılır.

---

# 76. HER AŞAMADA STOP GATE

Her aşama sonunda:

1. DWG gerçekten doğru açılıyor mu?
2. Yeni dependency lisans riski getirdi mi?
3. Android telefonda crash var mı?
4. Performans geriledi mi?
5. Parser/render abstraction bozuldu mu?
6. Edit-ready mimari korunuyor mu?

Kritik problem varsa yeni özellik eklenmez.

---

# 77. CODEX / AI İÇİN ÇALIŞMA KURALI

Bu MD bir kodlama ajanına verildiğinde:

## Önce mevcut kodu kullan

Sıra:

```text
1. ACadSharp API
2. ProCad mevcut renderer/control
3. SkiaSharp mevcut primitive
4. upstream issue/example
5. küçük adapter
6. ancak en son custom implementation
```

## Sıfırdan yazma

Mevcut düzgün çözüm varken:

- DWG parser yazma
- DXF parser yazma
- SHX parser yazma
- hatch engine yazma
- block engine yazma
- dimension formatter yazma

yasaktır.

---

# 78. TOKEN TASARRUFU STRATEJİSİ

AI'nın değeri:

> Zaten yazılmış açık kaynak kodu bulup doğru şekilde birleştirmek.

Her issue için:

```text
Repo search
↓
source inspection
↓
existing tests
↓
issue tracker
↓
minimal patch
```

Custom 2.000 satır kod üretmek en son seçenek.

---

# 79. FORK STRATEJİSİ

Başlangıçta ACadSharp fork etme.

NuGet kullan.

ProCad için:

- package yeterli ise package
- mobil paket olgun değilse pinned source/project reference

değerlendirilebilir.

Fork gerekiyorsa:

- upstream remote
- pinned commit
- patch list
- attribution
- small diff

korunur.

---

# 80. CI

Minimum:

```text
dotnet restore
dotnet build
dotnet test
```

Ek:

- license allowlist check
- test DWG parse
- golden scene tests
- Android build
- dependency graph diff

CAD package update otomatik merge edilmez.

---

# 81. RELEASE ÖNCESİ ZERO-ROYALTY CHECKLIST

Public release'ten önce:

```text
[ ] Autodesk SDK yok
[ ] Autodesk cloud conversion yok
[ ] ODA commercial SDK yok
[ ] GPL runtime dependency yok
[ ] AGPL runtime dependency yok
[ ] non-commercial dependency yok
[ ] unknown-license dependency yok
[ ] ACadSharp license recorded
[ ] SkiaSharp license recorded
[ ] MAUI license recorded
[ ] ProCad/used modules recorded
[ ] transitive packages audited
[ ] THIRD_PARTY_NOTICES complete
[ ] trademark review complete
[ ] no Autodesk logos
[ ] app name neutral
[ ] CAD files remain local
```

Bu checklist geçmeden store release yapılmaz.

---

# 82. ÜRÜN ADI KONUSU

Uygulamanın adı daha sonra seçilmelidir.

Ama marka stratejisi:

```text
Özgün ürün adı
```

olmalı.

DWG/DXF yalnız açıklamada desteklenen format olarak yazılmalıdır.

Örnek:

> "Fast offline 2D CAD viewer compatible with DWG and DXF drawings."

Bu ifade release tarihinde hukuk/marka yönergelerine göre tekrar kontrol edilir.

---

# 83. BAŞARI TANIMI

v1 başarılı sayılırsa:

- Autodesk hesabı istemiyor.
- internet istemiyor.
- DWG doğrudan açıyor.
- DXF açıyor.
- Android ve iOS.
- gerçek mimari/statik proje doğru görünüyor.
- pan/pinch akıcı.
- text okunuyor.
- dimensions doğru.
- hatch kabul edilebilir.
- blocks doğru.
- layers çalışıyor.
- büyük dosyada mümkün olduğunca stabil.
- kullanıcı dosyası cihazdan çıkmıyor.
- CAD teknolojisi için royalty yok.
- uygulama ücretsiz yayınlanabilir.
- editör için mimari çıkmaz sokakta değil.

---

# 84. NEYİ GARANTİ ETMİYORUZ?

Açık kaynak DWG stack ile baştan şu iddia yapılmayacak:

> Dünyadaki bütün DWG dosyalarını AutoCAD ile piksel piksel %100 aynı açar.

Özellikle:

- Civil 3D özel objeleri
- AutoCAD Architecture özel objeleri
- üçüncü parti proxy objects
- proprietary custom entities
- eksik proprietary fonts
- karmaşık underlay

fark yaratabilir.

Ama ürünün hedefi zaten:

> **2D mimari ve mühendislik projelerinin yüksek çoğunluğunu çok iyi açmak.**

Bu gerçekçi ve güçlü bir hedeftir.

---

# 85. EN KRİTİK YENİ KEŞİF: ProCad İLE İŞİN BÜYÜK KISMI HAZIR OLABİLİR

Önceki düşünce:

```text
ACadSharp
↓
renderer'ı büyük ölçüde kendimiz yaz
```

Araştırma sonrası daha iyi olasılık:

```text
ACadSharp
↓
ProCad.IO / Rendering
↓
ProCad.Controls.Skia
↓
ProCad.Controls.Maui
↓
Android/iOS
```

Bu Android gerçek cihaz spike'ında doğrulanmalıdır.

Eğer iyi çalışırsa:

- renderer kodu,
- entity handlers,
- SHX,
- layout,
- hit test,
- edit altyapısı

gibi çok sayıda zor işte token ve geliştirme süresi ciddi azalabilir.

Bu yüzden Aşama 1'in ilk işi **ProCad'i gerçek telefonda sınamak**tır.

---

# 86. FALLBACK KARARI

ProCad başarısız olsa bile proje başarısız değildir.

Ana garantili çekirdek yaklaşım:

```text
ACadSharp (MIT)
+
SkiaSharp (MIT)
+
kendi application/scene layer
```

korunur.

Bu nedenle ürün bir genç GitHub reposuna rehin bırakılmaz.

---

# 87. NEDEN BU YOL ÖNCEKİ LIBREDWG YOLUNDAN DAHA UYGUN?

Önceki LibreDWG çözümü:

- teknik olarak güçlü,
- doğrudan DWG okuyabiliyor,
- fakat GPL.

Yeni hedef:

> Gelecekte Android/iOS store release sırasında CAD çekirdeğinin copyleft/lisans yükümlülüğü ile uğraşmamak.

Bu nedenle:

```text
ACadSharp MIT
+
SkiaSharp MIT
+
MAUI MIT
+
ProCad MIT
```

stratejisi ürün hedefiyle daha uyumludur.

---

# 88. ÖNERİLEN NİHAİ MİMARİ

```text
┌──────────────────────────────────────────────┐
│               MOBILE CAD APP                 │
│          .NET MAUI 10 / .NET 10              │
├──────────────────────────────────────────────┤
│ Home / File Picker / Viewer / Layers         │
├──────────────────────────────────────────────┤
│                 CadSession                   │
├──────────────────────────────────────────────┤
│              Document Adapter                │
│                                             │
│               ACadSharp MIT                 │
│            DWG + DXF direct read            │
├──────────────────────────────────────────────┤
│             Render Scene Layer               │
│                                             │
│       ProCad MIT where proven useful        │
│          or thin custom adapter             │
├──────────────────────────────────────────────┤
│             SkiaSharp MIT                    │
│        high-performance 2D render            │
├──────────────────────────────────────────────┤
│ Pan / Pinch / Fit / Layers / Theme           │
├──────────────────────────────────────────────┤
│ Future: HitTest / Select / Command / Edit    │
└──────────────────────────────────────────────┘
```

---

# 89. ANA ÜRÜN PRENSİBİ

> **Önce kusursuza yakın viewer; sonra editor.**

Daha açık biçimde:

> Kullanıcı `.dwg` dosyasına dokunduğunda uygulama hızlı açılacak, mimari/statik çizim okunabilir ve teknik olarak güvenilir görünecek, telefonda akıcı gezilecek ve dosya cihazdan çıkmayacak.

Bu başarı sağlanmadan editör için özellik yarışına girilmeyecektir.

---

# 90. GELECEK MODELE VERİLECEK ELEŞTİRİ PROMPTU

Bu MD başka bir güçlü modele verildiğinde şu talep eklenebilir:

> Bu planı kör biçimde onaylama. 24 Ağustos 2026 veya değerlendirme yaptığın gün itibarıyla ACadSharp, ProCad, SkiaSharp ve .NET MAUI projelerinin güncel repo, dependency, lisans, issue ve release durumlarını tekrar kontrol et. Amacımız Android ve iOS'ta yayınlanabilen, Autodesk/ODA/ücretli API/royalty kullanmayan ve runtime dependency zincirinde GPL/AGPL gibi copyleft lisanslara ihtiyaç duymayan bir 2D DWG/DXF viewer geliştirmek. Özellikle ProCad'in MAUI + Skia renderer'ının ne kadarını doğrudan yeniden kullanabileceğimizi araştır. Daha olgun, MIT/Apache/BSD lisanslı ve gerçek DWG read + render konusunda daha az kod gerektiren bir alternatif varsa kanıtlarıyla öner. Lisansı belirsiz veya ileride ücret doğurabilecek hiçbir bileşeni önerme. Ön izleme kalitesi birinci öncelik, ileride editör eklenebilmesi ikinci öncelik. Android gerçek cihaz testini temel doğrulama noktası olarak koru.

---

# 91. ARAŞTIRILAN ANA KAYNAKLAR

## ACadSharp

<https://github.com/DomCR/ACadSharp>

<https://github.com/DomCR/ACadSharp/blob/master/LICENSE>

<https://www.nuget.org/packages/ACadSharp/>

## ProCad

<https://github.com/wieslawsoltes/ProCad>

## SkiaSharp

<https://github.com/mono/SkiaSharp>

<https://github.com/mono/SkiaSharp/blob/main/LICENSE.md>

## .NET MAUI

<https://github.com/dotnet/maui>

<https://learn.microsoft.com/dotnet/maui/>

## SHX

<https://github.com/ixmilia/shx>

## Autodesk trademark guidance

<https://www.autodesk.com/company/legal-notices-trademarks/trademarks/guidelines-for-use>

## Apple Developer

<https://developer.apple.com/programs/>

<https://developer.apple.com/support/compare-memberships/>

## Google Play developer

<https://support.google.com/googleplay/android-developer/>

---

# 92. NİHAİ KARAR

Bu proje şu şekilde başlatılmalıdır:

### Çekirdek

**ACadSharp — MIT**

### Mobil

**.NET 10 + .NET MAUI — permissive/open source**

### Renderer

**SkiaSharp — MIT**

### Hazır CAD render/edit katmanı adayı

**ProCad — MIT**

### Ana platform

**Android first**

### İlk gerçek doğrulama

**Kullanıcının kendi Android telefonu**

### İkinci platform

**iOS**

### Dağıtım modeli

**Uygulama kullanıcıya ücretsiz olabilir.**

### CAD teknolojisi maliyeti

**0 royalty / 0 Autodesk SDK fee / 0 per-file fee**

### Ağ gereksinimi

**Yok — local/offline default**

### Gelecek

**Edit-ready architecture, ancak preview tamamlandıktan sonra edit özellikleri.**

---

# 93. PROJE SLOGANI / TEKNİK İLKE

> **DWG'yi dönüştürme; doğrudan oku.**
>
> **CAD motorunu satın alma; permissive açık kaynak kullan.**
>
> **Dosyayı buluta gönderme; cihazda aç.**
>
> **Önce viewer'ı kusursuzlaştır; sonra editörü ekle.**
>
> **Bugün seçilen hiçbir dependency yarın kullanıcı sayısı arttığında royalty çıkarmamalı.**
