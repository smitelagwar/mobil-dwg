# Google Play Console Yayın ve Teslim Kılavuzu

Bu belge, **Mobil DWG** uygulamasının Google Play Console üzerinde yayınlanması için gereken form, metaveri ve uyumluluk bilgilerini içerir.

---

## 1. Temel Uygulama Bilgileri

- **Uygulama Adı**: Mobil DWG
- **Varsayılan Dil**: Türkçe (tr-TR)
- **Paket Kimliği (Package ID)**: `com.smitelagwar.mobildwg`
- **Kategori**: Üretkenlik / Araçlar (Productivity / Tools)
- **Fiyatlandırma**: Ücretsiz (Free, Reklamsız, Aboneliksiz)
- **Min SDK**: 24 (Android 7.0 Nougat)
- **Target SDK**: 36 (Android 16 Vanilla Ice Cream)
- **Paket Türü**: Android App Bundle (`.aab`)

---

## 2. Veri Güvenliği Formu (Data Safety Section)

Mobil DWG %100 çevrimdışı çalışacak şekilde tasarlanmıştır.

- **Veri Toplama veya Paylaşımı Var mı?**: **HAYIR (No)**
  - Uygulama herhangi bir kullanıcı verisi toplamaz.
  - Uygulama herhangi bir üçüncü tarafla veri paylaşmaz.
- **Ağ Erişimi (Internet Permission)**: **YOK (None)**
  - `AndroidManifest.xml` içinde `android.permission.INTERNET` izni kesinlikle bulunmaz.
- **Depolama Modeli**: **Uygulamaya Özel Kapsamlı Depolama (App-Private Scoped Storage)**
  - `MANAGE_EXTERNAL_STORAGE` veya `READ_EXTERNAL_STORAGE` gerektirmez.
  - Dosyalar Android Storage Access Framework (SAF) ve FilePicker aracılığıyla güvenli açılır.
- **Kullanıcı Hesabı veya Giriş**: **YOK**
- **Konum, Kişiler, Kamera, Mikrofon İzni**: **YOK**

---

## 3. İçerik Derecelendirmesi (IARC Content Rating)

- Şiddet, Cinsellik, Küfür, Kumar veya Uyuşturucu: **YOK**
- Kullanıcılar Arası İletişim / Sohbet: **YOK**
- Fiziksel Konum Paylaşımı: **YOK**
- Dijital Malzeme Alımı (In-App Purchases): **YOK**
- Beklenen Sonuç: **PEGI 3 / Everyone (Tüm Yaş Grupları)**

---

## 4. Uygulama Açıklaması ve Tanıtım Metinleri

### Kısa Açıklama (Short Description)
Hızlı, telifsiz ve %100 çevrimdışı 2D DWG/DXF teknik çizim görüntüleyici.

### Tam Açıklama (Full Description)
Mobil DWG, Android cihazınızda AutoCAD® formatındaki 2D DWG ve DXF çizimlerini tamamen yerel, güvenli ve internet bağlantısına ihtiyaç duymadan incelemenizi sağlayan hafif ve modern bir teknik çizim görüntüleyicidir.

**Öne Çıkan Özellikler:**
- **%100 Çevrimdışı ve Güvenli:** Dosyalarınız cihazınızdan asla ayrılmaz, internet izni dahi istemez.
- **Geniş Format Desteği:** AutoCAD R12'den 2018+'e kadar DWG ve DXF dosyalarını destekler.
- **Model ve Pafta (Layout) Desteği:** Model alanı ile Paper-Space paftalar arasında anında geçiş yapın.
- **Katman (Layer) Yönetimi:** Katmanları tek dokunuşla açıp kapatın.
- **Yüksek Performans:** Donanım hızlandırmalı vektör motoru ile akıcı pan, pinch-to-zoom ve fit navigasyonu.
- **Harita ve Kadastro Hassasiyeti:** Çift duyarlıklı (double-precision) koordinat koruması.
- **Türkçe Karakter Desteği:** CP1254 ve UTF-8 kodlama desteği.

*Yasal Bildirim: AutoCAD ve DWG, Autodesk, Inc.'in ticari markalarıdır. Mobil DWG bağımsız bir açık kaynak projedir ve Autodesk, Inc. ile ilişkisi bulunmamaktadır.*
