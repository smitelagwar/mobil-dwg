# Kullanıcı Onaylı Yürütme Override'ı

İlk karar: 2026-08-24
Güncel kapsam kararı: 2026-08-25

Durum: `ACTIVE`

Bu belge iki kullanıcı kararını kalıcı olarak kaydeder: dış test ortamı geçici olarak yokken kodlama durmayacak ve aktif v1 yalnız Android olacaktır. Windows/self-hosted Android Emulator artık kuruludur; fakat bilgisayar kapalı veya interaktif runner dinlemiyor olabilir. Fiziksel Android kanıtı ayrıca açıktır. Mac/Xcode/iPhone/Apple Developer işi aktif kapsamdan çıkarılmış ve future option olarak korunmuştur.

## Kural

Yalnız kullanıcı donanımı, hesabı, özel cihaz bağlantısı veya dış platform erişimi gerektiren bir aşama kapısı tamamlanamıyorsa:

1. Eksik kapı `DONE` veya `PASS` olarak gösterilmez.
2. İlgili aşama kendi evidence kaydında `BLOCKED` kalabilir.
3. Eksik kapı `DEFERRED_EXTERNAL_GATE` olarak ayrıca kaydedilir.
4. Kullanıcı bu dış erişimi şu an sağlayamadığını açıkça belirtmişse, bu eksik kapı bağımsız sonraki aşamaların başlamasını tek başına engellemez.
5. Sonraki aşamada eksik dış kapıya teknik olarak bağımlı olmayan işler yapılır.
6. Bir sonraki aktif Android aşama gerçek cihaz/runner/hesap olmadan dürüstçe test edilemiyorsa ilgili test cursor'ı `WAITING_RUNNER` veya `BLOCKED` olur; test sonucu uydurulmaz.
7. Ertelenen dış kapılar release, beta veya kendilerine doğrudan bağımlı milestone öncesinde yeniden açılır ve gerçek kanıt olmadan kapanmaz.
8. Aktif Android Definition of Done değişmez: gerçek Android release cihaz kanıtı olmadan final ürün tamamlanmış sayılmaz. iOS yalnız future track yeniden açılırsa kendi DoD'sini getirir.

## 2026-08-25 Android-only ve runner-offline kararı

1. AŞAMA 01–09 kodu `ANDROID_DOGRULAMA_PLANI.md` içindeki V01–V09 ile sırayla denetlenir.
2. Implementation geçmişi geriye dönük silinmez; yeni hata yeni commit/evidence ile kaydedilir.
3. Bilgisayar veya self-hosted runner çevrim dışıysa gerekli exact SHA `PENDING_EMULATOR_QUEUE` olarak tutulur; aynı test işi tekrar tekrar tetiklenmez.
4. Kod inceleme, host testleri ve güvenli implementasyon işi devam eder. Emulator kanıtı yoksa `VALIDATED/PASS` yazılmaz.
5. Geçici `Stage01Smoke` APK sonucu gerçek `MobilDwg.App` veya viewer işlevi PASS değildir.
6. iOS workflow, workload, simulator, Mac, iPhone, signing ve App Store işi kullanıcı açıkça geri dönene kadar yapılmaz ve Android'i bloke etmez.
7. Shared Core/Cad/Rendering ve adapter sınırları gelecekte iOS dönüşünü mümkün kılacak şekilde platformdan bağımsız tutulur.

## AŞAMA 01 için mevcut ertelenen dış kapılar

- Fiziksel Android cihazda `STAGE01_DEVICE_GATE_PASS`.
- Yerel emulator için V01'de hardened gate + geçerli artifact kanıtı; mevcut sağlık envanteri tek başına yeterli değildir.
- Tarihsel iOS erişim envanteri future iOS track'e taşınmıştır; aktif Android AŞAMA 01/V01 kapısı değildir.

Fiziksel Android kapısı tarihsel AŞAMA 01'i geriye dönük `DONE` yapmaz ve release cihaz matrisinde açık kalır. Emulator V01 bununla karıştırılmaz. Bağımsız işler ve ayrı implementation cursor'ı `devam` komutuyla ilerleyebilir.

## Çakışma halinde öncelik

Bu kullanıcı onaylı override, dış erişim blocker'larının ilerlemeyi tamamen durdurmasına ve aktif platform kapsamına ilişkin güncel karardır. Lisans, original-file immutability, gerçek Android release cihazı ve kanıtsız başarı yasağını kaldırmaz. Eski iki-platform zorunluluğunu aktif Android v1 için geçersiz kılar; iOS geçmişini silmez.

Yeni bir ajan `gecmis.md` sonrasında bu dosyayı okumalı ve eksik dış erişimi sahte başarıya çevirmeden bağımsız işlere devam etmelidir.
