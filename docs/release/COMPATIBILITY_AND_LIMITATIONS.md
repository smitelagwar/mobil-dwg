# Uyumluluk ve Bilinen Kısıtlar (Compatibility & Limitations)

Bu belge, Mobil DWG v1.0.0 sürümünün desteklediği CAD formatlarını, varlık (entity) türlerini ve bilinen teknik kısıtları tanımlar.

---

## 1. Desteklenen CAD Dosya Formatları

- **AutoCAD DWG**: AC1009 (R11/R12), AC1012 (R13), AC1014 (R14), AC1015 (2000), AC1018 (2004), AC1021 (2007), AC1024 (2010), AC1027 (2013), AC1032 (2018+) sürümleri.
- **AutoCAD DXF**: ASCII ve Binary biçimli tüm DXF sürümleri.

---

## 2. Desteklenen 2D Geometri ve CAD Varlıkları

| Varlık Türü | Sadakat Düzeyi | Notlar |
|---|---|---|
| **LINE, POINT** | C4 (Mühendislik) | Çift duyarlıklı koordinat koruması ($10^{-9}$ tolerans). |
| **LWPOLYLINE, POLYLINE** | C4 (Mühendislik) | Doğrusal ve yay (bulge) segmentleri, kapalı/açık döngüler. |
| **CIRCLE, ARC, ELLIPSE** | C4 (Mühendislik) | Yarıçap ve açı bazlı deterministik tessellation. |
| **TEXT, MTEXT** | C3 (Doğrulanmış) | Windows-1254 Türkçe karakter desteği, Unicode kaçışları (`\U+XXXX`), format kodları (`%%d`, `%%p`, `%%c`). |
| **BLOCK / INSERT / ATTRIB** | C3 (Doğrulanmış) | İçiçe bloklar, 2D afin dönüşümler, döndürme, orantısız ölçekleme ve ayna dönüşümleri. |
| **DIMENSION, LEADER** | C3 (Doğrulanmış) | Aligned, Rotated Linear, Radial, Diametric ölçülendirmeler. |
| **HATCH (Tarama)** | C3 (Doğrulanmış) | Katı (Solid) dolgu, ANSI31 çizgisel desenler, EvenOdd ada algılama. |
| **LAYOUT / VIEWPORT** | C3 (Doğrulanmış) | Model Space ve Paper Space paftaları, çoklu viewport dönüşümü, kırpma sınırları. |
| **XREF & RASTER IMAGE** | C3 (Doğrulanmış) | PNG, JPG, BMP raster altlıkları ve yerel XREF referansları. |

---

## 3. Bilinen Teknik Kısıtlar (Known Limitations)

1. **Yalnızca 2D Görüntüleyici (Viewer-First)**:
   - 3D tel kafes, mesh, katı modelleme (ACIS/SAT) veya 3D kamera dönüşümleri v1 kapsamında desteklenmez.
   - Çizim düzenleme, yeni eleman çizme veya DWG/DXF olarak kaydetme/dışa aktarma işlevi yoktur (salt-okunurdur).
2. **Font Paketleme ve SHX Dosyaları**:
   - Telif hakkı saklı Autodesk ticari SHX font dosyaları uygulamayla birlikte paketlenmez.
   - Eksik veya tescilli SHX fontları yerine açık kaynaklı `Roboto` ve sistem sans-serif fontları denetimli biçimde ikame edilir.
3. **Uzak XREF İndirme Yasağı**:
   - İnternet erişimi bulunmadığı için uzak URL veya bulut bağlantılı dış referanslar otomatik indirilmez; yalnızca yerel cihazdaki eşleşen dosyalar taranır.
4. **Proxy ve Özel Nesneler**:
   - Üçüncü taraf Civil 3D veya Architecture eklentilerine ait özel proxy nesneleri yalnızca standart CAD grafik ilkel sınırları çerçevesinde görüntülenir.
