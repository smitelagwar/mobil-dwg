# Gemini düzeltme başlangıç promptu

Güncel yerel mobil-dwg deposunda çalış. docs/GEMINI_SON_DENETIM_DUZELTME_PLANI.md dosyasının tamamını oku ve D01'den D12'ye sırayla uygula. Bu yeni bir plan yazma isteği değil: önceki uygulamanın bağımsız denetimde bulunan hatalarını kodda düzeltme ve gerçek kanıtla kapatma görevidir.

Önce HEAD/status ve mevcut kullanıcı değişikliklerini kaydet. Denetlenen HEAD 6a006a5825c280b4464f7e87f35c20633b8315c2; daha yeni değişiklik varsa ilgili bulguları güncel kodla eşleştir, eski hatayı körlemesine yeniden ekleme. Kullanıcı değişikliklerini koru. Bu düzeltme belgesinin sırası ve kabul kuralları öncelikli; eski nihai planın çelişmeyen matematik ve kalite sözleşmelerini koru. Eski dört aday plana veya eski PASS raporlarına dönme.

D01'de 13 bağımsız bulguyu doğru davranışı bekleyen kırmızı regresyonlara dönüştür ve gerçekten çalıştırılabilir ayrı Android instrumentation APK'sını kur. Bilinen kırmızı ürün testleri D01'in beklenen çıktısıdır. D02–D10'da sırasıyla düzelt, D11–D12'de normal Release APK ve temiz checkout üzerinde kanıtla. Özellikle ilk kare, çizim isteğinin kaybolması, UP beklemeden yeni bölgenin görünmesi, kaynak ömrü ve gerçek DXF geometri hatalarını atlama.

Her aşamada ilgili kodu ve testleri tamamla, sonucu docs/VIEWER_DUZELTME_DURUMU.md içine gerçek test adı/komut/çıktı/kanıt/commit ile kaydet; ardından rutin onay beklemeden sıradaki aşamaya geç. Teknik başarısızlığı atlama. Sınıf/dosya varlığı, sabit PASS yazısı, engine'e doğrudan paket göndermek veya yalnız derleme başarısı native uygulama doğrulaması değildir. Testi gevşetme, fixture'ı küçülterek eşiği geçme, test çalışmadan başarı yazma.

Emülatör ve yerel araçları kullan. Fiziksel cihaz veya gerçek dosya gibi dış doğrulama eksikse tam olarak hangi kapının neden açık olduğunu yaz, bağımsız işleri tamamla; ölçülmeyeni başarılı sayma. İlgisiz dosyaları reset/clean ile silme ve topluca commit'e alma. Bu görev push/merge/store yayını içermiyor.

Son teslimde değişen davranışları, kapanan P/K maddelerini, gerçek Android/CI test sonuçlarını, test edilen APK yolunu ve SHA-256'sını, varsa yalnız kalan dış doğrulama engelini bildir. Yeni bir genel plan çıkarma; açık kod hatasını ilgili aşamada giderip aynı kabul testini tekrar çalıştır. Şimdi D01'e başla ve sırasıyla devam et.
