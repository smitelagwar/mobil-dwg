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

## 2026-08-25 iki çalışma hattı ve sınırlı A10 kararı

Kullanıcı, bilgisayar/self-hosted runner kapalıyken zaman kaybetmemek; bilgisayar açıkken V04–V09 emülatörlü doğrulamasına geriden ve sıralı biçimde devam etmek istemektedir. Bu izin aşağıdaki kesin sınırlarla geçerlidir:

1. Genel `BASLA.md` sohbeti V04→V09 validation hattının sahibidir. `ANDROID_DOGRULAMA_PLANI.md`, VXX evidence, ortak checkpoint ve `android-test` taşıyıcı branch'i bu hatta aittir.
2. Ayrı A10 sohbeti yalnız kullanıcı `BASLA_A10.md dosyasını oku` dediğinde açılır ve `stage10-p0-geometry-draft` adlı normal feature branch'te çalışır.
3. Erken A10 kapsamı yalnız yeni/internal platform-independent primitive-tessellator matematiği ve saf testlerdir. V09 kapanana kadar mevcut RenderScene/interface/snapshot, architecture, `.csproj`/Skia ve fixture/image-golden sözleşmeleri dondurulur. A11, MAUI/FilePicker/lifecycle ve ProCad dahil değildir.
4. PC/runner kapalıyken açık A10 PR'ı yoksa branch commit/push ile sürebilir. PR zaten açıksa push `synchronize` olayıdır; önce PR kapatılır/etki güvenle gate edilir, aksi halde offline push yapılmaz. Uygun test ortamı geldiğinde host/hosted kontroller çalıştırılır; zero-step/billing/capacity sonucu PASS değildir.
5. A10 host/GitHub-hosted kontrolü sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması + Android gate bekleyen `CODED_PENDING_EMULATOR` olur. `PASS`, `READY_TO_MERGE`, `DONE` veya `main` merge yazılamaz/yapılamaz.
6. A10 sohbeti `android-test` branch'ini hareket ettirmez ve V04–V09 ortak checkpoint dosyalarını değiştirmez. Kendi branch/SHA/test borcunu `docs/A10_WORKSTREAM.md` içinde tutar.
7. V09 kapandıktan sonra güncel validated `main`, A10 branch'ine force kullanmadan alınır. Exact integration SHA üzerinde etkilenen V02/V03, V04–V07, V08 Android graph-isolation, V09, A10 acceptance ve expected-content içeren gerçek `MobilDwg.App` API 36 emulator render gate geçmeden merge yapılmaz; iOS workflow açılmaz.
8. AŞAMA 11 yalnız V04–V09 kapalı, A10 `DONE ON MAIN` ve emulator kuyruğu boş olduğunda sonraki kullanıcı turunda açılır.

Bu karar “bütün planı emülatörsüz A27'ye kadar kodla” izni değildir. V09 geriye dönük doğrulaması bitene kadar önden çalışma penceresi yalnız AŞAMA 10 ile sınırlıdır.

## AŞAMA 01 için mevcut ertelenen dış kapılar

- Fiziksel Android cihazda `STAGE01_DEVICE_GATE_PASS`.
- Yerel emulator V01 gate'i `CLOSED — INFRASTRUCTURE_SMOKE_ONLY`; gerçek `MobilDwg.App` runtime kanıtı V04'te claim-limited `VALIDATED` olmuştur.
- Tarihsel iOS erişim envanteri future iOS track'e taşınmıştır; aktif Android AŞAMA 01/V01 kapısı değildir.

Fiziksel Android kapısı tarihsel AŞAMA 01'i geriye dönük `DONE` yapmaz ve release cihaz matrisinde açık kalır. Emulator V01 bununla karıştırılmaz. Bağımsız işler ve ayrı implementation cursor'ı `devam` komutuyla ilerleyebilir.

## Çakışma halinde öncelik

Bu kullanıcı onaylı override, dış erişim blocker'larının ilerlemeyi tamamen durdurmasına ve aktif platform kapsamına ilişkin güncel karardır. Lisans, original-file immutability, gerçek Android release cihazı ve kanıtsız başarı yasağını kaldırmaz. Eski iki-platform zorunluluğunu aktif Android v1 için geçersiz kılar; iOS geçmişini silmez.

Yeni bir ajan `gecmis.md` sonrasında bu dosyayı okumalı ve eksik dış erişimi sahte başarıya çevirmeden bağımsız işlere devam etmelidir.
