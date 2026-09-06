# Mobil DWG — Uyumluluk ve Bilinen Kısıtlar

Bu belge uygulamanın mevcut hedefini ve güvenle iddia edilebilecek destek sınırlarını açıklar. DWG/DXF ekosistemi geniş olduğu için “her dosya kusursuz açılır” iddiası yapılmaz.

## Ürün kapsamı

- Android, local/offline, read-only 2D DWG/DXF viewer.
- Düzenleme, yeni entity oluşturma, DWG/DXF save/export v1 kapsamında yoktur.
- 3D solid/ACIS/SAT, tam CAD authoring ve plot fidelity hedefi değildir.

## DWG / DXF

Production parser baseline ACadSharp `3.7.1`'dir. Repo test/evidence seti eski ve modern AutoCAD sürümlerinden çeşitli DWG/DXF girdilerini kapsar; ancak gerçek destek entity kombinasyonu, custom/proxy object, font, XREF, raster ve dosyanın sağlık durumuna bağlıdır.

Bu nedenle:

- “tüm DXF sürümleri” veya “her DWG eksiksiz” şeklinde sınırsız garanti verilmez,
- desteklenmeyen/eksik içerik mümkün olduğunca diagnostic/compatibility kaydıyla görünür tutulur,
- yeni bir format/version claim'i gerçek corpus/evidence ile doğrulanmadan release metnine eklenmez.

## Güçlü desteklenen 2D alanlar

Mevcut renderer ve historical evidence aşağıdaki sınıflar için implementasyon içerir:

- LINE / POINT
- LWPOLYLINE / POLYLINE
- CIRCLE / ARC / ELLIPSE / SPLINE
- BLOCK / INSERT / ATTRIB
- TEXT / MTEXT
- DIMENSION / LEADER
- HATCH
- layer / color / linetype / lineweight
- Model Space / Paper Space / layout / viewport
- yerel XREF ve desteklenen raster image yolları

Gerçek fidelity çizimin özelliklerine göre değişebilir. Tarihsel stage evidence bir sınıfın implementasyonunu kanıtlar; her gerçek dünya dosyasının birebir AutoCAD görünümünü garanti etmez.

## Koordinat hassasiyeti

World/document koordinatları `double` tutulur. Büyük survey koordinatlarında erken `float` dönüşümünden kaçınılır. Bununla birlikte son rasterization, ekran çözünürlüğü, antialias ve GPU/backend sınırları görsel piksel sonucunu etkileyebilir.

## Font ve SHX

- Proprietary Autodesk SHX font dosyaları uygulamayla bundle edilmez.
- Uygun font bulunamazsa substitution/fallback uygulanabilir.
- Bu nedenle metin metriği, genişlik, satır kırılımı ve görünüm desktop AutoCAD ile birebir aynı olmayabilir.
- Font fidelity kritik dosyalar ayrıca test edilmelidir.

## XREF / raster / external reference

- Zorunlu internet erişimi yoktur; uzak URL'den external reference otomatik indirilmez.
- Yerel external reference erişimi Android'in seçilmiş dosya/provider erişim sınırlarına tabidir.
- Eksik veya erişilemeyen referans sessizce başarılı kabul edilmez.

## Proxy / custom object

Civil 3D, Architecture veya üçüncü taraf eklentilere ait proxy/custom object'lerin tam semantiği garanti edilmez. Standartlaşmış grafik bilgi çıkarılabiliyorsa kısmi görünüm mümkün olabilir; aksi halde compatibility diagnostic beklenir.

## Pan / zoom ve etkileşim performansı

6 Eylül 2026 tarihli 14 aşamalı Kararlılık Planı (`docs/VIEWER_STABILITY_CONTRACT.md`) kapsamında pan/pinch/render etkileşim zinciri kökten yenilenmiştir:
- Doğrudan Skia çizimi ve tek bekleyen kare kapısı (`FrameRequestGate`) ile ara bitmap/PNG gecikmesi kaldırılmıştır.
- Yerel pointer adaptörü (`AndroidInputAdapter`) ve çoklu dokunma durum makinesi (`ViewportInteractionEngine`) ile 1↔2 parmak geçişlerinde sıçrama (jump) önlenmiş, odak kayması (focal drift) `< 1e-9` seviyesine indirilmiştir.
- Bırakılmadan önce çizim (sentinel-before-UP) sözleşmesiyle parmak hareketi sürerken yeni görünür alanlar ekrana yansıtılmaktadır.
- Yerleşik geometri önbelleği (`PreparedGeometryCache`) ile sıcak kaydırmada sıfır yeniden-tessellation sağlanmıştır.

*Kayıtlı Kısıt:* Kod ve API 36 emülatör düzeyinde doğrulama tamamlanmış olup, fiziksel cihaz kabul kapısı otomasyon dışı fiziksel cihaz testleri için açık tutulmaktadır.

## Fiziksel cihaz sınırı

API 36 emulator güçlü bir integration test ortamıdır fakat fiziksel Android cihazdaki:

- touch sampling,
- GPU/driver,
- SAF/provider,
- termal/memory pressure

farklarını bütünüyle kanıtlamaz. Fiziksel cihaz sonuçları `docs/DEVICE_MATRIX.md` üzerinden ayrı tutulur.
