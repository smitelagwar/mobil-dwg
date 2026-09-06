# Mobil DWG/DXF Kararlı Görüntüleyici Sözleşmesi (Viewer Stability Contract)

**Sürüm:** 1.0.0  
**Tarih:** 6 Eylül 2026  
**Durum:** Nihai ve Bağlayıcı Mimari/Davranış Sözleşmesi  

Bu belge, `smitelagwar/mobil-dwg` projesinde parmak hareketini doğrudan takip eden, yakınlaştırırken odağı kaçırmayan, yeni görünen alanları hareket sürerken çizen ve gerçek DWG/DXF dosyalarında özellikleri sessizce kaybetmeyen bir mobil CAD motorunun tüm kalıcı kurallarını (invariants) ve kalite kapılarını tanımlar.

---

## 1. Mimari Katmanlar ve Bağımlılık Sınırları

Uygulama, sorumlulukları kesin olarak ayrılmış dört ana katmandan oluşur:

```
[ MobilDwg.App (net10.0-android36.0 / MAUI) ]
         │               │              │
         ▼               ▼              ▼
[ MobilDwg.Rendering ] [ MobilDwg.Cad ] ──► [ MobilDwg.Core (net10.0) ]
```

1. **`MobilDwg.Core` (net10.0):**
   - Sıfır dış bağımlılık. `SkiaSharp`, `ACadSharp` veya `Microsoft.Maui` kesinlikle referans verilemez.
   - Belgelerin dondurulmuş (immutable) DTO modellerini (`CadExtractedDocument`, `CadDocumentMetadata`), OCS dönüşüm matematiğini (`OcsTransform`), kaynak sınırlarını (`CadResourceBudget`, `CadBudgetGuard`) ve temel arayüzleri içerir.
2. **`MobilDwg.Cad` (net10.0):**
   - Yalnızca `MobilDwg.Core` ve `ACadSharp` bağımlılığına sahiptir.
   - ACadSharp nesnelerini Core DTO'larına (`CadExtractedDocument`) kayıpsız olarak dönüştürür. ACadSharp tipleri bu katmanın dışına sızamaz.
3. **`MobilDwg.Rendering` (net10.0):**
   - Yalnızca `MobilDwg.Core` ve `SkiaSharp` bağımlılığına sahiptir. `ACadSharp` veya `Microsoft.Maui` referansı yasaktır.
   - Kamera (`Camera2D`, `ViewportController`), uzamsal eleme (`StaticSceneBvh`), doğrudan Skia boyayıcısı (`SkiaScenePainter`), hazırlanmış geometri önbelleği (`PreparedGeometryCache`), pafta yönetimi (`CadLayoutManager`), ölçüm denetleyicisi (`MeasurementController`) ve nesne yakalama (`SnapQuery`) bileşenlerini barındırır.
4. **`MobilDwg.App` (net10.0-android36.0):**
   - MAUI ve Android platform entegrasyonu. `CadViewportView`, `AndroidInputAdapter`, `AndroidFrameClock`, `SafeCadFileCache` ve `CadFileOpenCoordinator` bileşenlerini yönetir.
   - `ACadSharp` doğrudan referans verilemez; tüm CAD işlemleri Core DTO ve Reader arayüzü üzerinden yürütülür.

---

## 2. Kamera, Odak Sadakati ve Sayısal Hassasiyet

1. **Çift Duyarlıklı (Double) Koordinat Uzayı:**
   - Kamera merkezi (`Center`), odak noktası ve dünya koordinatları kesinlikle `double` duyarlıkta tutulur.
   - Ekran koordinatlarına dönüşüm anına kadar `float` veya genel 3x3 matrise çevrim yapılmaz; büyük koordinatlı (ör. 5.000.000 m) harita ve kadastro çizimlerinde milimetre seviyesindeki ayrıntıların titreşimi engellenir.
2. **Pinch-Zoom Odak Değişmezliği:**
   - İki parmak arasındaki centroid/focal point, yakınlaştırma boyunca dünyadaki aynı noktaya kilitlenir.
   - MAUI'nin göreli `Scale` çarpanı yerine yerel pointer örneklerinden hesaplanan mutlak mesafe oranı (`currentDistance / initialDistance`) kullanılır.
3. **WUPP Sınırları ve Taşma Koruması:**
   - Dünya birimi / piksel (`WorldUnitsPerPixel - WUPP`) değeri, sahne sınırları ve merkez mesafesine göre `[1e-9, 1e9]` aralığında dinamik olarak kelepçelenir (`Math.Clamp`).
   - Hiçbir geçerli kullanıcı hareketinde kamera koordinatlarında `NaN` veya `Infinity` üretilemez.
   - 100 ardışık ileri-geri kaydırma adımında sayısal kayma (drift) `< 1e-9` olmak zorundadır.

---

## 3. Doğrudan Skia Boyama ve Kare Çizelgeleme

1. **Doğrudan Yüzey Çizimi (Direct Skia Surface):**
   - Çizimler ara Bitmap / JPEG / PNG kodlamasına sokulmaz; doğrudan `SKCanvas` üzerine çizilir.
   - Çizgi varlıkları Skia'nın yerel `DrawLine` çağrısıyla, yaylar `DrawArc` ile boyanarak gereksiz tessellation maliyeti sıfırlanır.
2. **Tek Bekleyen Kare Kuralı (`FrameRequestGate`):**
   - Aynı anda en fazla 1 aktif boyama (`State == Painting`) ve en fazla 1 bekleyen talep (`HasPendingRequest == true`) bulunabilir.
   - Giriş olayları kare hızından hızlı geldiğinde ara kareler atlanır, her zaman en güncel durum (`latest state`) çizilir.
   - Yüzey yeniden yaratıldığında (`SurfaceGeneration` değiştiğinde), eski nesle ait geçersiz callback'ler sessizce düşürülür.
3. **Çizim Sırası (Draw-Order) Korunumu:**
   - Varlıklar uzamsal eleme (BVH) sonrasında kaynak sıra indeksine (`SourceIndex`) ve kararlı ID'ye göre sıralı olarak çizilir; mekânsal sıralama CAD bindirme sırasını bozamaz.

---

## 4. Yerel Dokunma ve Hareket Durum Makinesi

1. **Yerel Pointer Paket Adaptörü (`AndroidInputAdapter`):**
   - Android `MotionEvent` verileri tek birleşik `PointerPacket` yapısına dönüştürülür.
   - Olay zamanı (`EventTime`) Android uptime saatiyle eşleştirilir.
2. **Topoloji Değişimi ve 1 ↔ 2 Parmak Geçişleri:**
   - Parmak sayısı değiştiğinde (ör. 1 parmaktan 2 parmağa geçiş veya parmaklardan birinin kaldırılması), referans taban (`baseline`) anında sıfırlanır; sıçrama veya ani görüş kayması oluşmaz.
   - İkinci parmak kalktığında hareket kesilmeden tek parmak kaydırma (`Pan`) durumuna geri dönülür.
3. **Bırakılmadan Önce Çizim (Sentinel-before-UP):**
   - Parmak ekranda tutulurken hareket ettirildiğinde (`PointerAction.Move`), görüş alanına yeni giren geometriler parmak bırakılmadan önce (`UP` beklenmeden) ekrana yansıtılır.
   - `UP` anında gelen son yerel hareket farkı da kameraya uygulanır.

---

## 5. Uzamsal İndeksleme ve Hazırlanmış Geometri Önbelleği

1. **Muhafazakâr Sınırlar ve Dengeli BVH (`StaticSceneBvh`):**
   - Her varlığın geometrisi (yay, elips, kalın polyline, eğimli metin) için muhafazakâr AABB hesaplanır.
   - Dengeli çok seviyeli SAH BVH ağacı kurulur; 100.000 varlıklı sahnede görüş alanı sorgusu `< 5 ms` sürer.
2. **Yerleşik Geometri Önbelleği (`PreparedGeometryCache`):**
   - B-spline ve bulge içeren yaylı polyline gibi ağır geometrilerin tessellation sonuçları 32 MB LRU önbellekte saklanır.
   - Sıcak kaydırma (`warm pan`) sırasında yerleşik varlıklar için yeniden-tessellation sayısı **kesin olarak 0'dır**.
3. **Etkileşim LOD ve İki Aşamalı Kalite Politikası:**
   - Hızlı hareket sırasında hafif basitleştirilmiş temsil (`RenderQualityMode.Interaction`) ile kare bütçesi korunur.
   - Hareket durduğunda (`Idle`) 200 ms içinde nihai detay (`RenderQualityMode.Final`) çizilerek tam netlik sağlanır.

---

## 6. Kayıpsız CAD Çıkarımı ve Format Doğruluğu

1. **OCS Dönüşüm Matematiği (`OcsTransform`):**
   - Düzlemsel varlıklar (Arc, Circle, Polyline, Text, Solid) Autodesk OCS Arbitrary Axis algoritmasıyla WCS uzayına dönüştürülür.
   - Doğrudan WCS tanımlı olan Line ve Point varlıklarına gereksiz dönüşüm uygulanmaz.
2. **Özyinelemeli Blok Genişletme (`BlockExpander`):**
   - İç içe blok yerleşimleri (Nested INSERT) hiyerarşik olarak açılır; temel nokta çıkarma, ölçek, döndürme ve normal yönü sırayla işletilir.
   - Eşit olmayan ölçekleme (`Non-uniform scale`) altındaki çemberler matematiksel olarak elipse dönüştürülür; yarıçap bozulması önlenir.
   - Yansıtma (Mirroring) altındaki yay ve polyline bulge açıları yön korunumu için tersine çevrilir.
3. **Türkçe Karakter ve Metin Biçimlendirme:**
   - CAD çizimlerindeki `\U+XXXX` Unicode dizilimleri (`\U+0130` -> İ vb.) çözülür.
   - MTEXT kontrol dizilimleri (`\P`, `\f`, `\A` vb.) parse edilerek çok satırlı `TextLayout` düzenine aktarılır; ekranda ham kontrol karakteri gösterilmez.
4. **Hatch Dolgu ve Desen Stabilitesi:**
   - Sınır döngüleri Even-Odd kuralıyla boyanır; ada/delik hiyerarşisi korunur.
   - Desen çizgileri (ANSI31 vb.) sabit dünya desen başlangıcı (`PatternOrigin`) ve tamsayı stride ile üretilir; pan/zoom sırasında desen yüzmesi (phase swimming) sıfırdır.

---

## 7. Paftalar, Ölçüm ve Nesne Yakalama

1. **Çoklu Pafta (Layout) ve Kamera Korunumu:**
   - Model space ve Paper space paftaları bellek içi sıfır reparse (`zero reparse`) ile anında değiştirilir.
   - Her paftanın son kamera durumu saklanır; pafta dönüşünde görüş kayması olmaksızın eski kamera geri yüklenir.
2. **Dünya Koordinatlarında Ölçüm:**
   - Mesafe ve alan ölçümleri ekran piksellerinden değil, dünya `double` koordinatlarından hesaplanır; pan/pinch hareketlerinde ölçüm değeri kesinlikle değişmez.
   - Dosya birimi tanımsızsa varsayım yapılmaz, `"çizim birimi"` etiketi kullanılır.
3. **12 DIP Toleranslı Nesne Yakalama (`SnapQuery`):**
   - Ekran toleransı cihaz yoğunluğuna (DIP -> Piksel) uyarlanır.
   - Eşit mesafede öncelik kuralı: `Endpoint -> Center -> Curve -> EntityId`.
   - B-spline kontrol noktaları eğri üzerinde değilse yanlış uç nokta yakalaması engellenir.

---

## 8. Yaşam Döngüsü ve Hata Kurtarma

1. **Deterministik Oturum Kapanışı ve Kiralama Drenajı:**
   - Oturum kapanışında (`Dispose()`) `_isRetiring = true` atanır ve `CloseRequested` tetiklenir; yeni kiralama (`AcquireRenderLease`) isteği `ObjectDisposedException` ile reddedilir.
   - Aktif kiralamalar tamamlanıp serbest bırakıldığında (`_activeLeaseCount == 0`), oturum `_disposed = true` durumuna geçer, önbellekleri temizler ve `DrainCompleted` olayını tetikler.
2. **Bellek Baskısı ve Önbellek Dosya Güvenliği (`SafeCadFileCache`):**
   - Açık belgeler için aktif dosya kayıt tablosu tutulur.
   - `OnTrimMemory` sırasında yalnızca sahipsiz geçici dosyalar (`PurgeOrphans`) temizlenir; o an açık olan çizim dosyası asla silinmez.
   - Üretim etkileşim yollarında manuel `GC.Collect()` çağrısı yasaktır.
3. **Kaynak Sınırları ve Taşma Koruması:**
   - 256 MB dosya, 250.000 entity, 64 KB metin, 64 MP / 256 MB raster bütçeleri `CadBudgetGuard` ile denetlenir.
   - Raster boyut hesaplamalarında `checked((long)width * height * 4)` taşma denetimi uygulanır.

---

## 9. Sürüm Doğrulama Kapıları (Release Gates)

Her sürüm ve değişiklik aşağıdaki 14 aşamalı kararlılık kapısından geçmek zorundadır:

| Aşama | Konu | Temel Kabul Eşiği |
|---|---|---|
| **01** | Telemetri ve Saat Tabanı | Core/Rendering/Architecture harness exit code 0 |
| **02** | Doğrudan Skia Boyama | Android Release derlemesi 0 uyarı, 0 hata |
| **03** | Kamera & Sayısal Sözleşme | 100 pan adımı drift < 1e-9, NaN/Infinity 0 |
| **04** | Yerel Giriş & Durum Makinesi | 1↔2 parmak topoloji geçişi, Cancel sıfırlaması |
| **05** | Çizelgeleme & Frame Gate | Aktif boyama ≤ 1, tek bekleyen talep |
| **06** | Muhafazakâr BVH Eleme | 100k varlıkta sorgu süresi < 5 ms |
| **07** | Geometri Önbelleği & LOD | Sıcak pan'da 0 yeniden-tessellation |
| **08** | Kayıpsız CAD Çıkarımı | Tip-güvenli DTO, 256 ACI renk paleti |
| **09** | Geometri & Blok Semantiği | Non-uniform elips, ayna yay yönü, spline alt bölme |
| **10** | Metin, Ölçü ve Hatch | Türkçe Unicode, Even-Odd delikli hatch, invariant phase |
| **11** | Paftalar & Ölçüm/Snap | Sıfır reparse pafta geçişi, 12 DIP snap hiyerarşisi |
| **12** | Yaşam Döngüsü & Kaynaklar | Kiralama drenajı, sahipsiz cache temizliği, taşma koruması |
| **13** | Gerçek Dokunma & Performans | Sentinel-before-UP, 60 Hz kare bütçesi |
| **14** | CI & Kararlılık Sözleşmesi | Otomatik CI iş akışı, tüm harness'ların doğrulanması |

*Fiziksel cihaz otomasyonu bulunmayan ortamlarda sürüm statüsü `KOD VE EMÜLATÖR DOĞRULANDI — FİZİKSEL KABUL BEKLİYOR` olarak işaretlenir.*
