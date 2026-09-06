# Mobil DWG — Gemini uygulaması sonrası düzeltme ve kapanış planı

**Denetim:** 6 Eylül 2026. **İncelenen kaynak:** güncel yerel çalışma ağacı, `codex/viewer-stability-v3`, HEAD `6a006a5825c280b4464f7e87f35c20633b8315c2`. **Kapsam:** eski 14 aşamayı yeniden planlamak değil; uygulanmış kodun hatalarını ve eksik üretim bağlantılarını düzeltmek. **Sıra:** D01 → D12. Bu belge uygulama kaynaklarını değiştirmeyen bir denetim sonucudur.

## 1. Denetim kararı

**Mevcut sürüm tamamlanmış kabul edilemez.** Eski Stage 14 kapısı, dört masaüstü test harness'i, locked restore ve normal Android Release derlemesi geçti. Buna rağmen aynı kaynaklardan derlenen APK'da hem uygulamanın kendi apartman örneği hem gerçek public DXF dosyası boş çizim alanı gösterdi. Ayrıca aşağıdaki 13 bağımsız kod kontrolünün tamamında kusur yeniden üretildi. Testlerin geçmesi ile gerçek uygulamanın çalışması arasında açık bir boşluk var.

İyi parçalar korunacak: doğrudan Skia yüzeyi ve ortak painter, double kamera modeli, pointer ID temelli girişin temeli, dengeli statik BVH ve ordinal sıralama, immutable veri aktarımına geçiş, parse işlerini sıraya alan coordinator, lease fikri, mevcut anlamlı geometri ve kamera testleri. Bunlar yeni bir motorla değiştirilmeyecek. Eksik bağlantılar ve yanlış sözleşmeler düzeltilecek.

Bu denetimde kaynak/üretim kodu değiştirilmedi; bağımsız kontroller `artifacts/gemini-review-2026-09-06/` altında oluşturuldu. Fiziksel telefonda performans, gerçek çoklu dokunma instrumentation koşusu, bütün DWG sürümleri ve uzun soak bu denetimde çalıştırılmadı. Bunlar geçmiş PASS ifadelerinden tamamlanmış sayılmıyor. Sınırsız dosya uyumluluğu veya hiç hata çıkmaması garanti edilmez; kapanış aşağıdaki ölçülebilir koşullara bağlıdır.

### Kanıtlar

Yollar depo köküne göredir; denetim klasörü Git tarafından ignore ediliyor. Bu dosyayı başka makineye taşıyorsan özellikle `ReviewProbes` kaynaklarını ve fixture'ı da taşı veya D01'de aynı regresyonları kalıcı testlere geçir.

| Kanıt | Dosya / sonuç |
|---|---|
| Mevcut kapının yeniden çalıştırılması | `artifacts/gemini-review-2026-09-06/gate14-run.log`, `stage14/` — exit 0 |
| Bağımsız kontrollerin kaynakları | `artifacts/gemini-review-2026-09-06/ReviewProbes/Program.cs`, `ReviewProbes.csproj` |
| Kontrol sonuçları | `artifacts/gemini-review-2026-09-06/independent-probes.log` — 13/13 kusur yeniden üretildi |
| Normal Release APK | `src/MobilDwg.App/bin/Release/net10.0-android36.0/com.smitelagwar.mobildwg-Signed.apk` |
| Test edilen APK SHA-256 | `6e6a3ae168a3c71bea61951d6877c8a3a0fb0e39184ae5aeafc18b6689083853` |
| Kurulum | `apk-manifest.json`, `apk-install.log` — emulator-5554 üzerine `adb install -r` başarılı |
| Yerleşik örnek çizim | `sample-screen.png`, `sample-ui.xml` — apartman örneği seçili, çizim alanı boş |
| Gerçek DXF açılışı | `dxf-screen.png`, `dxf-ui.xml`, `android-logcat.txt` — `review_synthetic.dxf` için açılış başarı logu var, çizim yok |
| Gerçek DXF kaynağı | `fixtures/public/synthetic/synthetic_turkish_basic_ac1015.dxf`; cihazın uygulamaya ait external files dizinine farklı adla kopyalandı |
| Elips kontrolü | `artifacts/gemini-review-2026-09-06/vertical-ellipse.dxf` — bu denetim için oluşturulmuş küçük ASCII DXF |

Kontrolleri yeniden çalıştırma:

```powershell
dotnet run --project artifacts/gemini-review-2026-09-06/ReviewProbes/ReviewProbes.csproj -c Release
```

**Dikkat:** Bu yardımcı program kusuru bulduğunda `DEFECT_REPRODUCED` yazar ve normal çıkabilir. Bu bir ürün başarı testi değildir. D01'de beklentileri doğru davranışa çevirerek gerçek test harness'lerine aktar; kırmızı/yeşil test süreci orada yürütülecek.

### Yeniden üretilmiş kusurlar

| ID | Öncelik | Gözlenen sonuç | Ana kaynak / düzeltme |
|---|---|---|---|
| P01 | P0 | Yeni gate generation=1; view constructor GL kurarken generation=2 yapıyor; `TryBeginPaint(2)` reddediliyor. APK'daki boş alanla uyumlu. | `CadViewportView.BindSession/InitializeGlView`, `FrameRequestGate`; D02 |
| P02 | P0 | İki ardışık `TryBeginPaint(1)` iki aktif ticket kabul ediyor. | `FrameRequestGate.TryBeginPaint`; D02 |
| P03 | P0 | `session.Zoom` gate'i Scheduled yapıyor; ardından host'un `RequestFrame` çağrısı false dönüyor ve saat kurulmayabiliyor. | `CadViewerSession.Zoom/Pan/ResizeViewport`, view `RequestFrame`; D02 |
| P04 | P1 | UI zoom kamerayı değiştiriyor; CameraRevision 0 → 0 kalıyor. | `CadViewerSession.CameraRevision`; D03 |
| P05 | P1 | Aynı konumlu UP yalnız `InteractionEnded` üretiyor; host buna abone değil, gerekli Final kare istenmiyor. | `ViewportInteractionEngine`, view; D03 |
| P06 | P1 | Aynı LOD bandındaki hata=1 cache girdisi, gereken hata≤0.25 iken kabul ediliyor. | `PreparedGeometryCache.TryGet`; D05 |
| P07 | P1 | Hatch girdisi cache bütçesi=64 bayt iken toplamı 96 bayta çıkarıyor; eviction hatch'i kapsamıyor. Üretim bağlantısı da henüz yok. | `PreparedGeometryCache.PutHatchCoverage/EvictToBudgetUnderLock`; D05/D08 |
| P08 | P0 | Bütçeden büyük bitmap `PutRaster` içinde dispose ediliyor; painter aynı nesneyi sonra çizmeye çalışıyor. | `RenderResourceCache.PutRaster`, `SkiaScenePainter.DrawRasterImagePrimitive`; D05 |
| P09 | P1 | 100 birim yükseklikte `WWWW` için model genişliği 300; Windows'taki gerçek SKFont ölçümü yaklaşık 373.63. Bounds muhafazakâr değil. | `TextLayout`, `TextLayoutMetrics`; D08 |
| P10 | P0 | Final copy progress anında orphan purge çağrısı, lease'e henüz kaydedilmemiş tamamlanmış dosyayı siliyor. | `SafeCadFileCache.CopyAsync/PurgeOrphans`; D04 |
| P11 | P0 | Aktif parse sırasında coordinator dispose edilince worker'ın `finally` içindeki semaphore release'i `ObjectDisposedException` veriyor. | `CadFileOpenCoordinator.DisposeAsync`; D04 |
| P12 | P1 | Depodaki gerçek DXF'de beklenen iç blok çizgisi `(55,65)→(65,65)`; çıkan `(5,5)→(15,5)`. | `AcadSharpEntityExtractor.ExpandBlockInsert`; D06 |
| P13 | P1 | Major axis `(0,10)`, ratio=0.5 elipste beklenen bounds 10×20; çıkan 20×10. | `ExtractSingleEntity` ELLIPSE, scene builder; D06 |

### Kaynak incelemesinde doğrulanan diğer açıklıklar

Bunlar için bütün cihaz senaryoları bu denetimde çalıştırılmış değildir; aşağıdaki satırlar kod bağlantısı ve kontrol akışı bulgularıdır. Gemini D01–D12'de belirtilen gerçek testlerle kapatacak.

| ID | Bulgular | Düzeltme |
|---|---|---|
| K01 | `NativeSmokeRunner` statik sınıf; proje `OutputType=Library`. `Android.App.Instrumentation` giriş noktası ve çalıştırma bağlantısı yok. “Native” testler doğrudan `engine.ProcessPacket` çağırıyor. Sentinel testi ekrandaki pikseli doğrulamıyor. | D01, D11 |
| K02 | GL callback mutable `_session` alanını tekrar tekrar okuyor; `finally` başka session gate'ini bitirebilir. MAUI Width ve state GL callback'inden okunuyor. Session lock'ı native engine mutation'ını kapsamıyor. | D02, D03 |
| K03 | `AndroidFrameClock` çağıran thread'in `Choreographer.Instance` nesnesini alıyor; GL EndPaint buraya gelebiliyor. `_scheduled` UI/GL arasında korunmuyor. `FramePresented` exception halinde bile finally'den geliyor. | D02 |
| K04 | GL yeniden kurulumunda eski handler/paint aboneliklerinin ve context kaynaklarının tam kapanışı yok. Watchdog callback'i generation/lifecycle ile doğrulanmıyor, reddedilen paint bile watchdog'u kapatıyor. GL catch doğrudan MAUI Content değiştirebiliyor. | D02, D10 |
| K05 | Native adapter parent ContentView platform view'ine bağlanıyor; gerçek çizim yüzeyine olay teslimi kanıtlanmamış. Paket generation sürekli 0, engine bunu denetlemiyor. Android ViewConfiguration değerleri kullanılmıyor; suspended state bağlı değil. | D03 |
| K06 | Extract/build exception'ında worker içindeki yerel parser session'ı dıştaki finally sahibine aktarılmadan kaybolabilir. UI Ready sonucunun generation kontrolü eksik. Close yalnız viewer'ı kapatıyor; coordinator'ın parser/file lease'i kalıyor. | D04 |
| K07 | Varsayılan geometri 32 MiB + raster 64 MiB, GPU/text hariç 96 MiB. Raster decode ve cache miss tessellation paint içinde. Raster borrow lease'siz; trim/eviction aktif kullanıcıyı dispose edebilir. Hatch coverage ve telemetry sınıfları üretim akışına bağlı değil. | D05, D08, D11 |
| K08 | Gerçek builder polyline width, entity linetype/transparency, layer frozen gibi bilgileri kullanmıyor; layer ACI seçimi truecolor'ı ezebiliyor. SOLID kapalı outline oluyor. SourceOrder korunması kaynak DRAWORDER semantiğini okuma yerine geçmiyor. | D06, D07 |
| K09 | Root elipste yön kayıp; OCS/WCS ayrımı entity türüne göre eksik. Eğik normalde circle/arc yalnız merkezi dönüştürülüyor. Nonuniform block arc sabit 32 parça; diğer eğrilerde affine şekil kaybı riski var. | D06 |
| K10 | Layout adları çıkarılıyor, gerçek paper-space içerik/viewport sahnesi ana açılışa taşınmıyor. Normal UI hep `new CadLayoutManager(scene)` ile Model açıyor. Painter iç anahtarı `vp:{innerIdx}` farklı viewport'larda çakışıyor. XREF/raster DTO → normal builder hattı tamamlanmamış. | D07, D09 |
| K11 | UI yeni MeasurementController'ı kullanmıyor: eski doğrusal snap, spline kontrol noktaları ve zorunlu cm/m varsayımı duruyor. Metadata yeniden oluşturulurken INSUNITS kayıp. Yeni SnapQuery bile bulge segmentini düz kirişe snap ediyor. Tema değişimi yeni scene/index/session kuruyor. | D09 |
| K12 | CI yalnız kaynak/test marker'larına bakarak native ve performans aşamalarını PASS yapıyor. Stage 01 baseline manifesti temiz checkout'ta yok; ignore edilmiş yerel artifacts'a bağlı. SDK `10.0.x`, global.json tam `10.0.400`; workload/native test/kanıt yükleme adımları eksik. İki dependency hash alanında gerçek hash yerine `...verified` metni var. | D12 |

Eski aşamaların durumu: 01 ve 13–14'ün ölçüm/kapanış iddiası doğrulanmadı; 02 ve 05'in gerçek yüzey bağlantısı başarısız; 03'ün kamera testleri yararlı ama tüm mutation yolları eksik; 04, 07–12 kısmen uygulanmış; 06'nın BVH temeli korunabilir, bounds doğruluğu metin/geometri düzeltmelerine bağlı. Eski PROGRESS dosyasındaki PASS'ler yeni kabul belgesi olarak kullanılamaz.

## 2. Uygulama kuralları

1. Bu belge düzeltme sırası ve kabul kanıtı için önceliklidir. `docs/MOBIL_DWG_NIHAI_UYGULAMA_PLANI.md` içindeki çelişmeyen kamera formülü, sayısal sınırlar, kalite ve kaynak sözleşmeleri korunur. Eski dört aday plan tekrar uygulanmaz. `docs/VIEWER_STABILITY_CONTRACT.md` varsa aynı kurallarla uyumlu tek sözleşme olarak güncellenir; ikinci rakip motor/input/cache eklenmez.
2. Başlangıçta HEAD, branch, status ve değişecek dosyaların hash'lerini kaydet. Bu denetim sırasında önceden var olan `release/SHA256SUMS.txt` değişikliği, iki untracked plan/prompt dosyası ve `tools/CadControlBenchmark/` korunacak. Kullanıcı değişikliklerini reset/clean ile silme veya topluca commit'e alma.
3. Her aşamanın kendi testini ve ilgili eski regresyonları çalıştır; başarılı aşamadan sonra rutin onay istemeden sıradakine geç. Başarısız teknik önkoşulu atlama. Dış cihaz eksikliği bağımsız kod/CI hazırlığını engellemez; ilgili kabul satırı **ÖLÇÜLMEDİ** kalır.
4. D01 test altyapısını ve kırmızı regresyonları kurar; bilinen ürün testlerinin kırmızı olması D01'in beklenen çıktısıdır. D02–D10 bunları yeşile çevirir. Bütün testlerin daha D01'de geçmesini istemek bu sırayla çelişir.
5. Test adını, dosya varlığını, sabit PASS çıktısını veya helper sınıfının varlığını gerçek davranış kanıtı sayma. Beklenen değer üretim fonksiyonundan türetilmez. Eşik yükselterek, fixture'ı küçülterek veya testi skip ederek kapanış yapılmaz.
6. İlerlemeyi `docs/VIEWER_DUZELTME_DURUMU.md` içine; koşu kanıtlarını `artifacts/viewer-correction/<run-id>/DNN/` içine yaz. Durum belgesi her ID için test adı, gerçek sonuç, kanıt yolu, commit/hash ve kalan engeli içerir. Önceki koşu dosyalarını ezme. Onaysız push/merge/store yayını bu düzeltmenin parçası değildir.

## D01 — Güvenilir regresyon zemini ve çalıştırılabilir Android testi

**Dosyalar:** `tests/MobilDwg.Rendering.Tests/`, `tests/MobilDwg.Integration.Tests/`, `tests/MobilDwg.Android.Instrumentation/`, yeni `scripts/viewer-correction-gate.ps1`, fixture manifesti ve durum belgesi.

- P01–P13'ü doğru davranışı bekleyen kalıcı regresyonlara dönüştür. App opening kaynaklarını Integration.Tests'in mevcut yöntemine uygun test et; üretim kodunun ayrı bir kopyasını yazma. P01/P03'e gerçek host bağlantısını kapsayan Android testini de ekle; yalnız gate simülasyonuyla yetinme.
- Mevcut Android test projesini ayrı test APK'sı üreten, manifestte hedef paketi belirtilmiş `Android.App.Instrumentation` giriş noktası olan runner'a dönüştür. Üretim APK'sına instrumentation component ekleme. Runner `UiAutomation.InjectInputEvent` ile ekrana gerçek zamanlı MotionEvent verir; engine'i doğrudan çağıran testler birim test kategorisinde kalır.
- Runner normal uygulamayı başlatsın, doğru fixture'ı açsın, gerçek viewer ekran sınırlarını bulsun ve **yalnız arka plan olmayan beklenen çizim işaretini** screenshot'ta arasın. App içindeki örnek çizim de test edilsin. Boş ekran mevcut sürümde testi düşürmeli.
- Yeni gate test exit code'larını, beklenen test sayısını, başarısız/atlanmış testleri ve JSON/JUnit sonuçlarını denetlesin. Native test raporu yoksa “native PASS” üretmesin. Eski testleri kaldırma; yeni kapıyı D12'de CI'ye bağla.

**Çıkış:** Runner APK kurulabilir ve `adb shell am instrument -w ...` ile gerçekten çalışır; beklenen kırmızı regresyon listesi kayıtlıdır. Native test component adı/komutu manifestten çıkarılıp rapora yazılır. Üretim uygulamasının dokunma yolunu atlayan test native kabul sayılmaz.

## D02 — İlk kare, tek paint ve kaybolmayan çizim isteği

**Dosyalar:** `Viewer/CadViewportView.cs`, `Viewer/Platforms/Android/AndroidFrameClock.cs`, `Rendering/Scheduling/FrameRequestGate.cs`, `Rendering/Viewer/CadViewerSession.cs`, `MainPage.cs` geçiş örtüsü.

- Belge ve yüzey kimliğini host yönetsin. Session bind, surface recreation ve layout transition aynı kimlik sözleşmesini kullansın; yeni session mevcut geçerli surface generation ile **ilk istekten önce** eşleştirilsin. Salt başlangıç sayısını 1/2 yapmak çözüm değildir; ikinci belge ve fallback de kapsanmalı.
- Gate'i `Idle → Scheduled → AwaitingPaint → Painting` durumlarıyla kur. Bir aktif paint ve bir dirty/pending talep sınırı uygula. Zaten Painting ise ikinci callback ticket alamaz. Scheduled/AwaitingPaint sırasında gelen değişiklik yeni kamera kuyruğu oluşturmaz; paint en güncel snapshot'ı alır. OS'nin kendiliğinden çağırdığı geçerli paint de aynı admission kontrolünden geçer.
- Session mutation'ları tek `FrameInvalidated(reason)` bildirimi yayımlasın; host bu bildirimi **bir kez** gate/clock'a götürsün. Session ve MainPage'in aynı isteği iki kez gate'e yazması kaldırılacak. Zoom, Fit, resize, layer, theme, input ve final-quality aynı yolu kullanacak.
- Paint admission + snapshot + session/resource lease aynı kısa kritik bölümde alınsın. Callback başında yakalanan session/gate/surface kimlikleri immutable yerel değerler olsun; finally mutable `_session` alanına dönmesin. Geçersiz eski callback yeni gate'i değiştiremesin.
- Choreographer UI thread'inde elde edilip aynı UI dispatcher üzerinden post/remove edilsin. GL callback UI nesnesini okumaz/değiştirmez; surface ölçüsü/density/lifecycle UI tarafından yayımlanan snapshot'tan gelir. `.Wait()`/`.Result` kullanılmaz.
- “Paint tamamlandı” ile “ekranda sunuldu” ayrı olaylar olsun. Başarısız/erken reddedilen karede başarı sinyali veya overlay kaldırma yok. Yeni belge/layout örtüsü yalnız eşleşen başarılı karenin sunum sınırıyla kaldırılır; GL paint-end'i OS presentation metriklerine eşit sayma.

**Kabul:** P01–P03 yeşil. İlk ve ikinci belge, CPU/GL yüzeyi, Fit/+/-/layer düğmeleri hiçbir parmak hareketi gerektirmeden görünür sonucu değiştirir. Paint ortasında yeni input en fazla bir sonraki kareyi planlar. Eski belge callback'i yenisini bitiremez. 10 saniye sakin idle'da uygulama kaynaklı istek 0. Boş ekran testi yeşil olmadan sonraki üretim adımına geçme.

## D03 — Atomik kamera, gerçek native input ve final kalite

**Dosyalar:** `ViewportInteractionEngine`, `ViewportController`, `ViewportInputContracts`, `AndroidViewportInputAdapter`, `CadViewerSession`, host.

- Tüm kamera değişiklikleri session'ın aynı mutation kilidinden geçsin. Controller/engine'e UI ve GL tarafından bağımsız mutation açma. Gerçek değer değişince tek monoton CameraRevision artır; Zoom/Fit/Pan/resize dahil. Snapshot alanları aynı anda alınsın.
- Adapter'ı o anda etkin GL/CPU çizim yüzeyinin native view'ine bağla; parent'ta kalacaksa bunun yerine açık ve test edilmiş tek native touch host kullan. Bu planın seçimi etkin yüzey adapter'ıdır. Surface değişince eskisini cancel/detach et, güncel generation ile yenisini bağla. Yerel native pikseli ikinci kez density ile çarpma.
- Touch slop, double-tap slop/time ve tap süresi Android ViewConfiguration'dan gelsin. ID kümesi değişince eski ID'leri bırak ve baseline kur; aynı pointer sayısı aynı pointer kümesi demek değildir. Generation uyuşmazlığı, CANCEL, focus kaybı, detach, pause ve geçersiz koordinat state'i güvenle sonlandırır.
- Eski planın centroid/anchor denklemini koru. UP'daki görülmemiş son delta bir kez uygulanır; aynı konumlu UP'da kamera değişmez. POINTER_DOWN/UP öncesi mevcut pointer'ların son örneği işlenir; ekleme/çıkarma kaynaklı centroid sıçraması pan sayılmaz. 1→2→1 ve 2→3→2 yeniden baseline kurar.
- `InteractionEnded` ayrı quality dirty nedeni üretir. Kamera revision değiştirmeyen UP bile gerekliyse Final kareyi ister. Sabit parmak ve zoom limitindeki değişmeyen paketler gereksiz revision/frame üretmez. Uzun basış tap/çift tap sayılmaz; ölçüm modu çift dokunmayı zoom'a dönüştürmez.

**Kabul:** P04/P05 yeşil; gerçek native pan/pinch, ID değişimi, 3 parmak, cancel, density 1/2/3, yavaş olay teslimi, min-span ve zoom limitleri test edilir. Kamera anchor doğruluğu eski planın toleranslarını korur. Bırakmada çift uygulama ve görüntü sıçraması yok; gereken Final kare gerçekten çizilir.

## D04 — Açma/iptal/kapatma ve geçici dosya sahipliği

**Dosyalar:** `Opening/CadFileOpenCoordinator.cs`, `Opening/SafeCadFileCache.cs`, `Opening/CadFileOpenContracts.cs`, `MainPage.cs`, Android lifecycle köprüsü.

- Coordinator aktif worker Task'ını izlesin. Dispose/close önce nesli geçersizleştirip cancel etsin; worker tamamlanmadan semaphore dispose edilmesin. Cancel edilemeyen parser işi retiring olarak izlenir; UI bekletilmez, işi varmış gibi yok sayma. Kaynak bırakma drain sonrasında yapılır.
- Worker'ın parser session'ı oluşturulduğu andan itibaren try/finally sahibine alınsın. Extract/build/cancel/commit hata yollarında session ve cached file tam bir kez dispose edilsin. Token source cancel/dispose yarışını da aynı sahiplik sözleşmesiyle kapat.
- `.part` ve final dosya, dosya oluşmadan başlayan copy lease'ine kayıtlı olsun. Rename sırasında aktif kayıt atomik aktarılır; progress callback aktif kaydı kaldırmaz. Orphan purge çalışan copy/parse dosyasını silemez. Tüm hata yolları kaydı ve dosyayı temizler; erişim/IO hatası asıl hatayı sessizce örtmez.
- UI, Ready sonucunu uygulamadan hemen önce generation ve kendi yaşam süresini kontrol etsin. Close hem viewer hem coordinator current/pending sahipliğini emekliye ayırır. A→B→C açma dizisinde yalnız C yayımlanır; A/B geç bitince yeniden görünmez.
- Yeni dosya hatası, hâlen geçerli önceki görüntüyü gereksiz `ResetCurrentSessionAsync` ile yok etmesin. Metadata sonucu olduğu gibi aktarılsın; extension'dan yeni metadata üretme. Ana UI'deki synchronous extract/build fallback kaldırılıp aynı worker hattına yönlendirilsin.

**Kabul:** P10/P11 yeşil; copy sırasında purge, parse/extract/build hata enjeksiyonu, hızlı A/B/C, parse sırasında close/dispose, sıfır bayt/bozuk/izin reddi testleri. Drain sonrasında parser handle/copy lease sayısı 0 ve aktif geçici dosya yok; sonradan gelen sonuç ekranı geri açmaz.

## D05 — Güvenli cache, doğru kalite anahtarı ve sınırlı hazırlık

**Dosyalar:** `PreparedGeometryCache`, `RenderResourceCache`, `RenderSessionLease`, `CadViewerSession`, `SkiaScenePainter`, `RenderQualityPolicy`, tek CPU preparation worker.

- Cache hit için aynı band yeterli olmayacak: kaydedilmiş **gerçek/geçerli üst hata sınırı** istenen chord error'ı sağlamalı. Interaction 1 px, Final 0.25 px; segment limiti dolduysa bu hata sınırı sağlandı diye kaydetme. Önceki LOD bandını taşıyarak mevcut %20 hysteresis'i gerçekten uygula.
- Anahtarlar document/scene kimliği, entity instance yolu, primitive index, gerekli layout/transform/style/font revision ve kaliteyi kapsasın. `vp:{innerIdx}` kaldırılır; farklı viewport geometrileri aynı girdiyi alamaz. En fazla iki yararlı LOD; LRU doğru girdiyi ve hatch girdilerini de çıkarır.
- Tek toplam viewer cache bütçesi: `clamp(memoryClassMiB / 8, 16, 64) MiB`, low-RAM=16 MiB. Geometri+raster+text+GPU cache tahmini birlikte sayılır. Parser, aktif frame geçici belleği ve retiring bytes ayrıca raporlanır; cache hesabına saklanmaz. Pinned kaynakları zorla dispose ederek bütçe tutturma; yeni cache admission reddedilsin.
- Raster/cache kullanımı lease ile olsun. `TryAcquire`/release olmadan raw SKBitmap paylaşma. Bütçeye sığmayan tek bitmap cache'e alınmaz; decode öncesi boyut/byte guard ve uygun downsample uygulanır, geçici kullanımın sahibi frame lease olur. Cache Put başarısı/ownership transferi açık dönsün. Eviction/trim/close yalnız son kullanımdan sonra dispose eder.
- Dosya okuma, raster decode ve pahalı eğri hazırlığını paint'ten çıkar. Tek aktif hazırlık + latest bekleyen talep; eski nesil sonucu yayımlanmaz. Scene commit öncesi sınırlı kaba geometri hazır olsun; yeni görülen bölgede eldeki geçerli kaba geometri hemen çizilsin, ince kalite arkadan gelsin. Hazırlık beklenirken boş bölge veya UP bekleme yok.
- Dünya koordinatı double kalır. Yerel origin sonrası float kullanımı 0.1 px sınırını sağlamıyorsa clip/rebase veya double dönüşüm uygula. Özellikle büyük merkez/radius iptalinde native arc, uzun line uçları ve tüm fast path'ler reference yoluyla karşılaştırılır. Cache uygunluğu belirlenmeden float cast yapma.

**Kabul:** P06/P07/P08 yeşil; aynı-band interaction→final doğru; iki viewport cache çakışması yok; büyük raster/trim sırasında aktif bitmap canlı; bütçeye sığan sıcak pan'da tekrar tessellation/decode 0. Yeni alan ve kapasite aşan gezinme ayrı ölçülür, ikisinde de parmak havaya kalkmadan geometri görünür.

## D06 — Gerçek parser hattında koordinatlar, bloklar ve eğriler

**Dosyalar:** `Cad/AcadSharp/AcadSharpEntityExtractor.cs`, Core extraction DTO/payload'ları, `CadExtractedSceneBuilder`, `OcsTransform`, `PrimitiveTransformer`, geometri/bounds testleri.

- Tek affine dönüşüm zinciri kur: parent × insert-local × entity-local. Block base point, insertion, rotation, scale/mirror, MINSERT satır/sütun offset'i ve OCS bu zincirde bir kez uygulanır. `ExpandBlockInsert` recursive çağrısı parent transform'u taşır. Rendering'deki yardımcı block testleri gerçek extractor doğruluğunun yerine geçmez.
- Koordinat uzayını entity türüne göre uygula: LINE/ELLIPSE WCS verisini tekrar OCS'ye sokma; OCS kullanan circle/arc/polyline/text için normal/elevation korunur. Z/basis bilgisi DTO'da kaybolmadan 2D görünüşe projekte edilir. Eğik düzlemde circle'ın izdüşümü ellipse veya dejenere eğri olabilir.
- Ellipse DTO major/minor basis vektörleri gerçek yönü taşısın. Nonuniform/shear dönüşmüş circle, arc, ellipse ve bulge için iki basis vektörlü parametrik eğri kullan; gerekirse primitive ekle. Sabit 32 parça ile kaynağı kalıcı doğrusal geometriye dönüştürme. Adaptive tessellation D05 ekran hata bütçesini kullanır.
- Polyline bulge/width, spline degree/knots/weights ve closed/periodic semantiği uçtan uca korunur. Geçersiz spline sessizce kontrol poligonuymuş gibi tam doğru gösterilmez; diagnostic ile açık fallback. SOLID filled polygon olur; vertex sırası ve dejenerasyon doğru ele alınır.
- Array expansion, block recursion, attribute, vertex, glyph ve hatch allocation'larından **önce** mevcut CadBudgetGuard limitlerini uygula. Limit aşımında tüm genişleme güvenli sonlanır; yalnız iç foreach'ten çıkıp milyonlarca boş row/column dolaşma. Recursive block döngüsü diagnostic üretir. Guard kusurlu entity'yi tüm uygulamayı çökertmeden raporlar.

**Kabul:** P12 public DXF `(55,65)→(65,65)` ve P13 10×20 bounds testleri yeşil. Gerçek dosyada en az üç seviyeli dönmüş/ölçekli/mirrored block, nonzero base point, MINSERT, eğik OCS circle/arc, rotated ellipse, nonuniform bulge ve rational spline oracle'ları eklenir. Beklenen noktalar analitik veya bağımsız referanstan gelir. Optimize ve reference görüntü aynı geometridir.

## D07 — Stil, görünürlük, çizim sırası ve muhafazakâr bounds

**Dosyalar:** extractor/DTO, `CadExtractedSceneBuilder`, `Styles/*`, `RenderScene`, `StaticSceneBvh`, dimension/viewport child style aktarımı.

- Layer truecolor/ACI türünü açık taşı; RGB=0 siyahı “truecolor yok” sayma. BYLAYER/BYBLOCK, layer 0 inheritance, linetype pattern/scale, lineweight, transparency, invisible/off/frozen bilgisi gerçek dosyadan painter'a ulaşsın. Layer veya entity linetype'ını zorla Continuous/ByLayer yapma.
- Kaynağın desteklediği DRAWORDER/sortents sırasını DTO'da sabit ordinal'e çöz. BVH traversal sırası bunu değiştirmez. Block, dimension ve viewport içeriği kendi child style/ordinal'ini taşır; parent'a flatten ederken renk/katman kaybolmaz.
- Bounds geometrinin bütün eğrisini, polyline width'ü, gerçek text glyph sınırlarını ve kalın stroke payını kapsar. Bilinmeyen bounds'lu desteklenmeyen entity sessizce cull edilmez; kontrollü diagnostic/placeholder yolu kullanılır. Rastgele origin cross ekleme.
- Mevcut dengeli BVH korunur. Layer/theme değişiminde indeks yeniden kurulmaz. Frame query buffer/stack yeniden kullanılabilir veya havuzlanır; “allocation-free” yorumunun yanında her sorguda `new BvhNode[64]` kalmaz. D08 font bounds değişimi ilgili index revision'ını geçersizleştirir.

**Kabul:** Gerçek DXF renk/linetype/transparency/frozen/solid/overlap örneklerinde reference karşılaştırması; viewport'un dört kenarında kalın çizgi ve geniş polyline kesilmiyor. Deterministik BVH sonucu brute-force muhafazakâr sonuç kümesini kaçırmıyor; görüntü çizim sırası aynı. 250k yükte doğruluk bozulmuyor. Metnin font ölçümüne bağlı kenar testleri bu aşamada regresyon olarak eklenir; P09 ile birlikte D08'de yeşile çevrilir ve D07/D08 birleşik kapısı orada kapanır.

## D08 — Metin, dimension ve hatch'in gerçek çizim doğruluğu

**Dosyalar:** `TextLayout`, `TextLayoutMetrics`, `FontSubstitutionResolver`, `MTextParser`, `HatchProcessor`, dimension aktarımı, painter/preparation worker.

- Karakter sayısı ×0.75 tahminini kaldır. Ölçüm ve çizim aynı çözümlenmiş font/glyph-run/advance/bounds bilgisini kullansın. Font yoksa seçilen substitution görünür diagnostic ve cache key'e girsin. Türkçe Unicode, combining karakter, alignment, width factor, mirror ve oblique aynı layout verisinden çizilsin.
- MTEXT formatting/line wrap ve desteklenen stiller parser payload'dan gerçek layout'a aktarılsın. Desteklenmeyen formatting açık diagnostic olur. Metin bounds'u font revision değişince yeniden hazırlanır; her frame font/layout yeniden kurulmaz.
- Dimension için gerçek generated block/child geometry ve style korunur. Desteklenen ölçü türlerinde source DIMSTYLE, unit format ve text override uygulanır. Sentetik DimensionBuilder testinin geçmesi gerçek DIMENSION dosyası için kabul değildir.
- Hatch PAT/loop/edge verisini koru. Pattern origin/phase dünya koordinatında sabit; yalnız güncel görünür kapsam için worker coverage hazırlar. Farklı kenar türleri ve island/fill rule doğru uygulanır. `scale*5` uydurma spacing ve satır sayısına göre 2/4 stride kaldırılır; thinning projekte spacing<3 px kuralına ve sabit dünya line index'ine bağlıdır.
- Hatch coverage cache'ini gerçek painter'a bağla. Panning sırasında eldeki geçerli boundary/coverage temsilini çiz; yeni alanda bütün geometriyi UP'a kadar saklama. Final kalite sınırlı hazırlanır ve D05 admission/lease/bütçesine uyar.

**Kabul:** P09 yeşil; W/M/İ/ı/Ş, multiline, rotated/mirrored/oblique text için ölçüm ve boyanan sınırlar uyumlu. Gerçek dimension dosyasında bilinen mesafe/etiket/style doğru. Hatch ada/arc loop/pattern origin testinde pan ve LOD geçişinde desen yüzmüyor, delikler kapanmıyor. Bütçe aşılınca başarı uydurulmuyor.

## D09 — Layout, referanslar ve gerçek UI araçları

**Dosyalar:** extraction DTO'ları/builder, `Layouts/*`, `References/*`, `MainPage.cs`, `MeasurementController`, `SnapQuery`, `MainActivity.cs`.

- Layout adlarıyla yetinme: model ve paper-space entity koleksiyonları, paper ölçüsü, viewport transform/twist/clip ve görünürlük normal açılışa taşınsın. Aynı scene builder/renderer kullanılır. UI layout seçimi gerçek sahneyi değiştirir; layout başına kamera saklanır, yeni surface boyutuna yeniden uyarlanır.
- XREF ve raster nesneleri typed DTO'dan resolver'a ve painter'a ulaşsın. Göreli referanslar yalnız kullanıcının seçtiği erişilebilir kapsamda çözülür. Eksik/döngüsel/desteklenmeyen referans belirgin placeholder ve diagnostic üretir; sessiz atlama yok. Raster origin/U/V transform/clip korunur; decode D05'teki worker ve lease yolundadır.
- UI ölçüm noktalarını session.Measurement ve düzeltilmiş SnapQuery üzerinden işle. Snap aktif layout ve görünür layer'larda, 12 DIP yarıçapının bir defa piksele dönüşümüyle yapılır. Bulge için kirişe değil gerçek eğriye, spline için kontrol noktası yerine gerçek eğri/endpoint'e snap et. Deterministik en yakın geçerli aday seçilir.
- Reader metadata'sı INSUNITS ile taşınır. Bilinmeyen birimde “çizim birimi”; bilinen birimde doğru etiket/değer; kullanıcı açıkça birim seçerse anlamı ve gerekli dönüşüm açık olur. cm/m varsayımını kaldır. Tema değişimi yalnız color/style snapshot günceller; kamera/layout/ölçüm modu/layer override korunur ve scene/BVH/session yeniden kurulmaz.
- Android `ACTION_VIEW content://` ile gelen gerçek dosya, ContentResolver stream'i üzerinden aynı coordinator'a girsin. Sadece test amaçlı `open_cad` extra'sını desteklemek genel dosya açma desteği değildir. URI izin yaşamı mevcut picker/cache sözleşmesiyle yönetilsin.

**Kabul:** Gerçek iki-paper-layout/twisted-clipped viewport dosyası; mevcut/eksik raster-XREF; dış dosya yöneticisinden açma; metre/mm/unknown-unit ölçüm ve curved snap testleri. Tema/katman/layout değişimi sonrası durum korunur; hiçbir UI eylemi ek parmak hareketi beklemez. Pencerede doğru format gösterilir; DXF “DWG” diye etiketlenmez, ölçülmemiş süre “0 ms” başarı metriği olarak sunulmaz.

## D10 — Surface kaybı, arka plan ve düşük bellek

**Dosyalar:** viewport, Android frame/input adapters, `App/Window` lifecycle bağlantıları, `MainActivity.OnTrimMemory`, session/resource owners.

- Attach/resume ve detach/pause için açık host durumları kur. Arka planda frameclock/watchdog/preparation talepleri durur; input CANCEL olur. Resume'da geçerli yeni surface snapshot'ı ile yalnız gereken ilk kare istenir. Static `CadFileRequested` aboneliği sayfa ömrü sonunda kaldırılır.
- Watchdog görünür, attached, resumed, pozitif ölçülü ve gerçekten bekleyen ticket için çalışır; token+generation+ticket callback anında yeniden kontrol edilir. Süre 1000 ms; ilk geçerli timeout GL'yi bir kez yeniden kurar, ikincisi aynı belge oturumunda kalıcı CPU fallback yapar. Yeni gesture veya stale timeout yeniden surface oluşturmaz.
- GL context/surface hatası loglanıp UI dispatcher üzerinden fallback yapılır. Her painter exception'ını GPU sorunu diye sessiz yutma; geometry/IO/programlama hatası sınıflandırılıp ilgili test düşer. Başarısız paint watchdog'u başarı sayarak kapatmaz.
- Eski GL/CPU yüzeyi event/input/handler/context sahipliğiyle kapatılır; aktif frame drain olmadan kaynağı serbest bırakma. Düşük bellek aktif session'ın toplam cache'ine bağlanır, sadece orphan dosya purge ile geçiştirilmez. Kaynaklar ait olduğu thread'de son lease bırakılınca dispose edilir.

**Kabul:** Gerçek native pause/resume, rotation/resize, focus loss, yüzey yeniden yaratma, zorlanmış GL failure→CPU, paint sürerken close/trim. Eski callback/süre aşımı yeni belgeyi etkilemez; use-after-dispose, exception yutma, çoğalan event aboneliği ve açık frameclock kalmaz.

## D11 — Üretim yolunda doğruluk, akıcılık ve uzun süre testi

**Dosyalar:** gerçek instrumentation runner, `ViewportTelemetry`, painter/input/coordinator ölçüm bağlantıları, `scripts/viewer-correction-gate.ps1`, fixture manifesti ve kanıt raporu.

- Telemetry gerçek Input→Camera→Request→Paint zincirine bağlanır. Monoton saatler kalibre edilir; document/surface/camera/input sequence ID kaydedilir. PaintEnd ayrı, OS presentation ayrı ölçülür. Gerçek sunum için FrameTimeline/Perfetto ve screenshot/video ilişkilendirmesi kullan; `gfxinfo` tek başına GL içeriğinin kanıtı değildir. Ölçülemeyen alan null/ÖLÇÜLMEDİ olur.
- Parmak basılı tutulurken daha önce görünmeyen dört yöndeki sentinel sırayla görünür alana girsin. Runner UP göndermeden screenshot alıp sentinel'ı kontrol eder; yalnız BVH candidate bulundu testi yetmez. Native input zaman damgaları gerçek uptime ve gerçek zamanlı teslimata dayanır.
- Corpus: 10k/50k/150k/250k seyrek entity, yoğun Fit, text/hatch, ağır eğri, uzun çizgi/büyük koordinat, gerçek küçük/orta/büyük DWG ve DXF, layout/reference, bozuk/unsupported/quota dosyaları. Entity yanında primitive/vertex/glyph miktarı ve kaynak SHA-256 da kayıtlı olsun. Private çizim repo/CI'ye konulmaz; erişilemiyorsa ilgili gerçek dosya testi açık kalır.
- Eski planın kabul eşikleri korunur: 60 Hz referans cihazda küçük/orta ve 150k seyrek görünüm sunum slot farkı p95≤1 vsync, p99≤2; yoğun Fit p95≤2 vsync; input→görünür sonuç p95≤50 ms, p99≤100 ms. Isınma sonrası viewer kaynaklı >100 ms duraklama, crash/ANR 0. Final kalite küçük/orta≤200 ms, büyük desteklenen≤500 ms. Her kayıt 10 s ısınma +60 s ölçüm, üç tekrar; en kötü p95/p99 da raporlanır.
- 15/30 Hz input, sabit parmak ve zoom limitinde sahte FPS için sürekli render açılmaz. Sıcak resident pan, ilk görülen alan ve cache kapasitesi aşan rota ayrı değerlendirilir. Sakinleşme sonrası 10 s uygulama frame request=0; active paint≤1, pending≤1.
- Bellek: 10 ısınma close/reopen +30 ölçülen döngü. Drain sonrası owner/lease 0; managed/native/GPU/PSS ve retiring bytes ayrı. Son 5 kapalı durum medyan PSS, ilk 5'e göre `max(16 MiB, %5)` üstünde kalırsa fail/inceleme. Artan live-owner sayısı doğrudan fail. En az 30 dakika pan/pinch/layout/open/close soak; üretimde zorla GC ile gizleme yok.

**Kabul:** D01–D10 doğruluk testleri normal Release APK'da geçer. Emülatör işlevsel kanıt sağlar; mevcut 6 GB/8 core emülatör telefon performansının yerine geçmez. Fiziksel cihaz yoksa cihaz metrikleri ÖLÇÜLMEDİ, ürün tam performans onayı **BEKLEMEDE** kalır; bunun için yeni mimari plan çıkarılmaz, yalnız hazır koşu gerçek cihazda tamamlanır.

## D12 — Temiz checkout, gerçek sürüm kanıtı ve kapanış

**Dosyalar:** `.github/workflows/viewer-stability.yml`, eski/yeni gate script'leri, global/package locks, dependency audit, release manifesti, durum ve uyumluluk belgeleri.

- CI tam SDK/workload sürümünü global.json'dan kurar; `10.0.x` ile exact-pin çelişkisi kaldırılır. Android SDK/JDK/workload kurulumu açık olsun. Temiz checkout kendi run baseline'ını üretir; ignore edilmiş eski Stage 01 artifacts'ını önkoşul yapmaz. Tarihî baseline üretim testinin başarı girdisi değildir.
- Desktop test/build job ve API36 emulator instrumentation job çalışsın. Test APK gerçekten build/install/run edilir. JSON/JUnit sonuçları, başarısızlık screenshot/logcat'ı, fixture manifesti ve APK hash'i artifact olarak yüklenir. Eksik gerçek DWG fixture'ı sessiz File.Exists skip ile PASS olmaz; uygun fixture sağlanır veya release gate açık kalır.
- Kaynak dosyası varlığı yalnız mimari kontrol olarak kalabilir; native/performance PASS'i üretemez. Bilerek bozulan first-frame/sentinel testi gate'i düşürmeli. Tüm testlerin skip edilmesi veya instrumentation başlamaması başarısızlıktır.
- Paket provenance manifestindeki `...verified` metinlerini kaldır; gerçek nupkg hash'i, lock contentHash'i ve kullanılan algoritma açık alanlarda doğrulanır. 64 karakter biçimi tek başına doğrulama değildir; gerçek dosya karşılaştırması gerekir. APK SHA-256 build'den hesaplanır, kaynak revision/dirty manifesti ve test edilen APK ile eşleşir.
- Gerekli yerel özel patch'i olmayan temiz checkout'ta locked restore, release build, desktop ve emulator testlerini çalıştır. Test edilen normal APK'da validation-only renderer/input yolu etkin olmamalı. Son kaynak değişiminden önceki APK sonucu yeni sürümün kanıtı sayılmaz.
- Eski PROGRESS tarihçesini silme; hatalı kabul iddialarına bu denetim ve yeni kanıt bağlantısı ekle. `VIEWER_DUZELTME_DURUMU.md` P01–P13, K01–K12, D01–D12 ve eski planın kalan kabul satırlarıyla eşleşsin. Desteklenmeyen özellikler görünür/gerçek uyumluluk tablosunda belirtilsin; desteklenmesi kararlaştırılmış temel özellikleri sessizce kapsamdan çıkarma.

**Kod düzeltmelerinin kapanış koşulu:** Açık P0/P1 kusur yok; D01–D10 teknik ve D11 gerçek uygulama doğruluk kapıları yeşil; temiz checkout CI ve test edilmiş APK hash'i eşleşiyor. **Tam ürün kabulü** ayrıca fiziksel performans ve soak kanıtının geçmesini gerektirir. Bunlar ÖLÇÜLMEDİ ise yalnız “kod ve emülatör doğrulaması tamamlandı, fiziksel kabul bekliyor” denebilir. “Her şey tamamlandı/kusursuz” denemez. Başarısız kapıya yeni genel plan yazmak yerine ilgili DNN'yi düzeltip aynı testi yeniden çalıştır.

## 3. Kaynak dayanakları

- Choreographer thread'e bağlıdır; `getInstance()` çağıran Looper'ın örneğini verir. UI thread üzerinden tek clock sahipliği bu nedenle zorunlu. [Android Choreographer](https://developer.android.com/reference/android/view/Choreographer)
- Native testin uygulama olay yolunu çalıştırması için instrumentation ve input injection gerekir. [Android Instrumentation](https://developer.android.com/reference/android/app/Instrumentation), [UiAutomation](https://developer.android.com/reference/android/app/UiAutomation), [.NET Android Instrumentation](https://learn.microsoft.com/en-us/dotnet/api/android.app.instrumentation?view=net-android-35.0)
- ELLIPSE center ve relative major-axis endpoint WCS'dedir; yeniden OCS dönüşümü genel kural olamaz. [Autodesk ELLIPSE DXF](https://help.autodesk.com/cloudhelp/2018/ENU/AutoCAD-DXF/files/GUID-107CB04F-AD4D-4D2F-8EC9-AC90888063AB.htm), [Autodesk OCS](https://help.autodesk.com/cloudhelp/2024/ENU/AutoCAD-DXF/files/GUID-D99F1509-E4E4-47A3-8691-92EA07DC88F5.htm)
- GL çizim süresi ile görünür sunum aynı metrik değildir. [Android render ölçümü](https://developer.android.com/topic/performance/vitals/render), [FrameTimeline](https://perfetto.dev/docs/data-sources/frametimeline)

## 4. Gemini'ye verilecek başlangıç promptu

```text
Güncel yerel mobil-dwg deposunda çalış. docs/GEMINI_SON_DENETIM_DUZELTME_PLANI.md dosyasının tamamını oku ve D01'den D12'ye sırayla uygula. Bu yeni bir plan yazma isteği değil: önceki uygulamanın bağımsız denetimde bulunan hatalarını kodda düzeltme ve gerçek kanıtla kapatma görevidir.

Önce HEAD/status ve mevcut kullanıcı değişikliklerini kaydet. Denetlenen HEAD 6a006a5825c280b4464f7e87f35c20633b8315c2; daha yeni değişiklik varsa ilgili bulguları güncel kodla eşleştir, eski hatayı körlemesine yeniden ekleme. Kullanıcı değişikliklerini koru. Bu düzeltme belgesinin sırası ve kabul kuralları öncelikli; eski nihai planın çelişmeyen matematik ve kalite sözleşmelerini koru. Eski dört aday plana veya eski PASS raporlarına dönme.

D01'de 13 bağımsız bulguyu doğru davranışı bekleyen kırmızı regresyonlara dönüştür ve gerçekten çalıştırılabilir ayrı Android instrumentation APK'sını kur. Bilinen kırmızı ürün testleri D01'in beklenen çıktısıdır. D02–D10'da sırasıyla düzelt, D11–D12'de normal Release APK ve temiz checkout üzerinde kanıtla. Özellikle ilk kare, çizim isteğinin kaybolması, UP beklemeden yeni bölgenin görünmesi, kaynak ömrü ve gerçek DXF geometri hatalarını atlama.

Her aşamada ilgili kodu ve testleri tamamla, sonucu docs/VIEWER_DUZELTME_DURUMU.md içine gerçek test adı/komut/çıktı/kanıt/commit ile kaydet; ardından rutin onay beklemeden sıradaki aşamaya geç. Teknik başarısızlığı atlama. Sınıf/dosya varlığı, sabit PASS yazısı, engine'e doğrudan paket göndermek veya yalnız derleme başarısı native uygulama doğrulaması değildir. Testi gevşetme, fixture'ı küçülterek eşiği geçme, test çalışmadan başarı yazma.

Emülatör ve yerel araçları kullan. Fiziksel cihaz veya gerçek dosya gibi dış doğrulama eksikse tam olarak hangi kapının neden açık olduğunu yaz, bağımsız işleri tamamla; ölçülmeyeni başarılı sayma. İlgisiz dosyaları reset/clean ile silme ve topluca commit'e alma. Bu görev push/merge/store yayını içermiyor.

Son teslimde değişen davranışları, kapanan P/K maddelerini, gerçek Android/CI test sonuçlarını, test edilen APK yolunu ve SHA-256'sını, varsa yalnız kalan dış doğrulama engelini bildir. Yeni bir genel plan çıkarma; açık kod hatasını ilgili aşamada giderip aynı kabul testini tekrar çalıştır. Şimdi D01'e başla ve sırasıyla devam et.
```
