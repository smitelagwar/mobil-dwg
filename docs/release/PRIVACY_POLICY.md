# Gizlilik Politikası — Mobil DWG

**Son Güncelleme:** 5 Eylül 2026

Bu politika mevcut local/offline ürün tasarımını açıklar. Yeni bir release'te permission, analytics, telemetry, reklam SDK'sı veya veri akışı değişirse bu belge yayın öncesi yeniden güncellenmelidir.

## Veri toplama ve ağ kullanımı

Mevcut Mobil DWG tasarımı:

- kişisel bilgi, cihaz kimliği veya kullanım analitiği toplamaz,
- reklam/analytics/telemetry SDK'sı kullanmayı hedeflemez,
- CAD dosyalarını uzak bir sunucuya yüklemez,
- temel görüntüleme için internet bağlantısı gerektirmez.

Final release öncesi `AndroidManifest.xml`, resolved dependency graph ve packaged artifact üzerinde `android.permission.INTERNET` ve olası veri-toplayan SDK'lar tekrar doğrulanmalıdır.

## DWG / DXF dosyaları

Kullanıcının açıkça seçtiği DWG/DXF dosyaları cihaz üzerinde işlenir.

Uygulama, Android Storage Access Framework / FilePicker üzerinden verilen erişimi kullanarak seçilen içeriği okumaya çalışır ve güvenli işleme akışında gerektiğinde app-private cache/kopya oluşturabilir.

- Dosyalar uygulama tarafından bir cloud servisine yüklenmez.
- Orijinal CAD dosyasının üzerine yazılmaz.
- App-private geçici veriler normal close/reset/cleanup akışlarında temizlenir ve ayrıca Android'in uygulama/cache yaşam döngüsü sınırları içindedir.

Beklenmeyen process termination gibi durumlarda “uygulama kapandığı anda her byte kesin silinir” şeklinde garanti verilmez; app-private storage başka uygulamalara açık genel paylaşım alanı değildir.

## Sistem izinleri

Temel dosya açma davranışı, kullanıcının sistem dosya seçicisinde açıkça seçtiği belgeye erişim üzerinden tasarlanmıştır. Geniş kapsamlı tüm-depolama erişimi ürün hedefi değildir.

Final manifest izin listesi her release öncesi doğrulanmalıdır.

## Üçüncü taraf bileşenler

Kullanılan açık kaynak dependency'ler için `THIRD_PARTY_NOTICES.md` ve repo içindeki `compliance/` kayıtları geçerlidir. Yeni dependency eklenmesi bu politikanın veri-toplama iddialarını etkileyebilir ve ayrıca incelenmelidir.

## Çocukların gizliliği

Uygulamanın mevcut işlevi kullanıcı hesabı, sosyal özellik veya kişisel veri toplama mekanizması içermez. Mağaza yaş/hedef-kitle beyanları ilgili mağaza politikalarına göre ayrıca yapılır.

## İletişim

Proje ile ilgili soru ve geri bildirimler repo üzerinden iletilebilir:

`https://github.com/smitelagwar/mobil-dwg`
