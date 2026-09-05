# Mobil DWG — Google Play Yayın Kılavuzu

Bu belge mağaza yayını için çalışma taslağıdır. Google Play'e gönderimden hemen önce package, permission, Data Safety, pricing ve feature claim'leri **gerçek release artifact'i üzerinde yeniden doğrulanmalıdır**.

## Temel uygulama bilgileri

- Uygulama adı: Mobil DWG
- Paket kimliği: `com.smitelagwar.mobildwg`
- Varsayılan dil: Türkçe (`tr-TR`)
- Kategori adayı: Productivity / Tools
- Min SDK: 24
- Target SDK: 36
- Yayın paketi: Android App Bundle (`.aab`)

Fiyat, reklam, abonelik ve mağaza politikası yayın anındaki gerçek ürün kararıyla eşleştirilmelidir; eski bir doküman otomatik doğruluk kaynağı değildir.

## Data Safety kontrolü

Mevcut ürün tasarımı local/offline viewer'dır ve tarihsel release evidence internet izni/telemetri olmadan hazırlanmıştır. Yine de her yeni release öncesi final manifest ve dependency graph üzerinde doğrula:

- `android.permission.INTERNET` var mı?
- analytics/telemetry/ad SDK eklendi mi?
- kullanıcı verisi veya identifier toplanıyor mu?
- veri üçüncü tarafa aktarılıyor mu?
- yalnız kullanıcının seçtiği CAD dosyasına mı erişiliyor?

Bu maddelerden biri değiştiyse Data Safety ve `PRIVACY_POLICY.md` aynı release'te güncellenmelidir.

## İçerik derecelendirmesi

Viewer'ın kendisi şiddet, cinsellik, kumar veya kullanıcılar arası iletişim özelliği sunmaz. Ancak Google Play/IARC soruları yayın sırasında gerçek feature set'e göre cevaplanır.

## Mağaza metni için güvenli taslak

### Kısa açıklama

Android cihazınızda DWG ve DXF teknik çizimlerini yerel ve çevrimdışı görüntüleyin.

### Tam açıklama

Mobil DWG, Android cihazlarda 2D DWG ve DXF teknik çizimlerini yerel olarak açmak ve incelemek için geliştirilmiş salt-okunur bir görüntüleyicidir.

Öne çıkan mevcut ürün özellikleri:

- yerel/offline dosya açma,
- 2D DWG/DXF görüntüleme,
- katman görünürlüğü,
- model/layout görüntüleme altyapısı,
- pan, zoom ve fit navigasyonu,
- büyük koordinatlarda `double` tabanlı world-coordinate işleme,
- unsupported/eksik external-resource durumlarını diagnostic olarak yüzeye çıkarma yaklaşımı.

### Claim kuralları

Aşağıdaki ifadeleri gerçek release testi olmadan kullanma:

- “tüm DWG/DXF dosyalarını kusursuz açar”,
- “AutoCAD ile birebir aynı”,
- “%100 uyumluluk”,
- “en hızlı”,
- “kusursuz/akıcı 120 FPS”,
- “donanım hızlandırmalı ve her cihazda akıcı”.

2026-09-05 itibarıyla pan/pinch/render interaction kalitesi üzerinde açık iyileştirme çalışması olduğu için navigasyon performansı mutlak sıfatlarla pazarlanmamalıdır.

## Marka notu

AutoCAD ve DWG, Autodesk, Inc. ile ilişkili ticari markalardır. Mobil DWG bağımsız bir projedir; Autodesk tarafından onaylanmış veya desteklenmiş olduğu izlenimi verilmemelidir.

Projenin kendi dağıtım lisansı açıkça seçilmeden mağaza metninde “açık kaynak uygulama” ifadesi kullanılmamalıdır.

## Yayın öncesi checklist

- signed final AAB gerçek artifact olarak üretildi,
- package ID/version code/version name doğru,
- target/min SDK doğrulandı,
- manifest permission listesi çıkarıldı,
- Data Safety gerçek dependency/permission graph ile eşleşiyor,
- privacy policy feature set ile eşleşiyor,
- third-party notices güncel,
- compatibility/limitations metni güncel,
- screenshot ve mağaza metni gerçekten çalışan release davranışını gösteriyor,
- pan/zoom gibi kullanıcıya görünür claim'ler fiziksel/emulator acceptance ile destekleniyor.
