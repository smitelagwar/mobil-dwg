# Mobil DWG kararlı görüntüleyici için nihai uygulama planı

**Hazırlanma tarihi:** 5 Eylül 2026  
**Son bütünlük denetimi:** 6 Eylül 2026 — aşama bağımlılıkları ve uygulanabilirlik düzeltildi; 14 aşama korundu.  
**Hedef:** `smitelagwar/mobil-dwg` — güncel yerel çalışma ağacı  
**Uygulayıcı:** Gemini 3.8  
**Sıra:** 14 aşama; her aşama kendi doğrulamasını geçerek tamamlanır.  
**Bu dosyanın durumu:** Araştırma ve planlama tamamlandı. Aşağıdaki uygulama aşamaları henüz başlatılmadı.

Amaç, parmak hareketini doğrudan takip eden, yakınlaştırırken odağı kaçırmayan, yeni görünen alanları hareket sürerken çizen ve gerçek DWG/DXF dosyalarında özellikleri sessizce kaybetmeyen bir Android uygulamasıdır. Çözüm mevcut .NET MAUI, ACadSharp ve SkiaSharp motorunu geliştirir. Başka bir görüntüleyiciye geçiş veya motoru sıfırdan yazma kararı yoktur.

Bu belge önce bağımsız kod incelemesi ve araştırmayla hazırlanmış planın, ardından verilen dört planın değerlendirilmesinin sonucudur. Eski planlar yürütme talimatı değildir; çatışmalarda bu dosya uygulanır. Buradaki performans sayıları **tasarım hedefi ve kabul eşiğidir; ölçülmüş mevcut sonuç veya endüstriden alınmış sihirli ayar değildir.**

## İnceleme yöntemi ve güncel durum

Bağımsız plan, dört aday dosya açılmadan önce `artifacts/plan-review-2026-09-05/BAGIMSIZ_PLAN_V1.md` içine kaydedildi. Kayıt zamanı **23:02:50 +03:00**, SHA-256:

```text
16d376f3eacb89140ff110bcce90356eef6f14a99a9326863a82a24fe7ac5471
```

Adayların içerikleri bundan sonra okundu. İki Word belgesinin paragrafları ve tabloları çıkarıldı; her biri altı sayfa olarak ayrıca görüntülendi. Kaynak dosyaların hash'leri yerel `candidate-manifest.json` dosyasındadır. Bağımsız taslak sonradan değiştirilmedi.

İnceleme sırasında yerel HEAD ile GitHub main aynıydı:

```text
bbaf7bf84148c16a6d411ffe653c14771ee45848
```

Ancak çalışma ağacı temiz değildi ve **bu değişiklikler esas alındı**:

| Dosya veya klasör | İnceleme anındaki durum |
|---|---|
| `src/MobilDwg.App/MainPage.cs` | Yerelde değiştirilmiş; HEAD'e göre 1132 satırlık diff |
| `src/MobilDwg.Rendering/Scene/RenderScene.cs` | Yerelde değiştirilmiş |
| `MobilDwg.sln` | Yerelde değiştirilmiş |
| `release/SHA256SUMS.txt` | Yerelde değiştirilmiş |
| `tools/CadControlBenchmark/` | Takip edilmeyen yeni benchmark kodu |

Önemli iki dosyanın inceleme hash'i:

```text
MainPage.cs   5be6b7b726850ffb3165ca2e92d9e069ac260583c0f1740ca7f8d201098358ce
RenderScene.cs 691c8352744c3ad3a4c59f672d6d7b6f794b459e55488c4f6d605e3fbe84d280
```

Core, Rendering ve Architecture konsol testleri `dotnet run ... -c Release` ile çalıştırıldı; üçü de **exit code 0** verdi. Bu projeler mevcut hâlleriyle klasik test SDK projesi değildir. `dotnet test` komutunun hata vermemesi testlerin yürüdüğü anlamına gelmez.

API 36 emülatörü `emulator-5554` üzerinde uygulama açık bulundu. Mevcut ekran görüntüsü yerelde saklandı. Kurulu APK ile incelenen kaynakların birebir eşleşmesi kanıtlanmadığı için bu ekran **güncel kodun kabul testi sayılmadı**. Fiziksel cihaz performansı, gerçek çoklu dokunma ve yeni mimari bu planlama oturumunda test edilmedi.

### 6 Eylül bütünlük denetiminde tamamlanan noktalar

Limit kesintisi dosyanın sonunu kaybettirmemişti; 14 aşama, karşılaştırma, kaynaklar ve teslim koşulları dosyadaydı. İkinci okumada aşağıdaki uygulama açıkları bulundu ve ilgili maddelerin içine işlendi:

- Architecture harness'ının tam üç test projesi şartı, Aşama 01'de dördüncü ve Aşama 05'te beşinci projeyi eklemeden önce açık yol listesiyle güncellenecek. Denetim kaldırılmayacak.
- Aşama 02 painter'ının ihtiyaç duyduğu snapshot/context tipleri aynı aşamada tanımlanacak. İlk native instrumentation altyapısı Aşama 05'e alındı; Aşama 13 bunu genişletecek.
- Android DOWN tüketimi, topoloji paketindeki son gerçek hareket, slop altındaki UP, frame callback thread'i, kendiliğinden gelen surface paint'i ve sıfır boyutlu yüzeyin yeniden hazır olması açıklandı.
- Hareketten bağımsız geometri cache'i ile görünür kapsama bağlı hatch hazırlığı ayrıldı. Cache bütçesi, evicted nesnenin yeniden hazırlanması ve başlangıç kaba temsilinin sınırları tutarlı hâle getirildi.
- Yeni surface'te yanlışlıkla Fit, farklı saatleri çıkararak sahte gecikme ölçümü, seyrek input'ta gereksiz FPS şartı ve non-cancellable iş sürerken hemen sıfır lease beklentisi düzeltildi.
- CI'nın dirty kaynak üzerine yerel commit atıldığı için kendiliğinden aynı kaynağı test ettiği varsayımı kaldırıldı. Ardışık uygulama promptu kullanıcı isteğine göre güncellendi.

İlk bağımsız taslak ve 5 Eylül inceleme kanıtları değiştirilmedi. Bu denetim uygulama kodu yazıldığı veya yeni davranışların cihazda doğrulandığı anlamına gelmez.

## Hakem kararı

Aşağıdaki kısa adlar yalnız karşılaştırma içindir:

- **P1:** `MOBIL_DWG_ZOOM_PAN_RENDER_GEMINI_3_8_GUNCEL_REPO_UYGULAMA_PLANI.md`
- **P2:** `implementation_plan.md`
- **P3:** `ENGINE_OPTIMIZATION_SPEC.docx`
- **P4:** `smitelagwar-mobil-dwg-plan.docx`
- **B:** Önce yazılan bağımsız planım.

| Plan | Korunan katkı | Çıkarılan veya düzeltilen bölüm | Sonuç |
|---|---|---|---|
| P1 | Doğrudan Skia yüzeyi, mevcut kamerayı koruma, tek bekleyen frame, draw order, kaynak cache'i, paket ve mimari denetimleri, gerçek dokunma gerekliliği | Temiz çalışma ağacı zorunluluğu; eski yerel durum tespitleri; ilk hareketi yutma; kenar dışına çıkınca gesture reset; yalnız tek pointer'ı cancel etme; ölçülmemiş sabitler; dar parser kapsamı; eksik kaynak ömrü sözleşmesi | En güçlü aday; omurgası alındı, olduğu gibi uygulanmayacak |
| P2 | Bitmap/JPEG maliyetinin ve pivot uyuşmazlığının doğru teşhisi; indeks ve iki kalite seviyesi ihtiyacı | Büyük bitmap, 150 ms debounce, pan clamp, aynı göreli ölçek hatası, 250 ms engel, PNG'yi canlı çözüm sayma, hatalı ağaç, yanlış zoom animasyonu ve test komutu | Teşhis yararlı; önerilen uygulama kodu reddedildi |
| P3 | Odak koruma, 1→2→1 geçişleri, büyük koordinat duyarlılığı, hareket sürerken çizim | Mevcut double kamerayı genel matrisle değiştirme, Dart benzeri örnekleri harfiyen uygulama, sabit %50 overscan, frame başına 0.92 inertia, sıfır hata/60–120 FPS iddiası | Kavramlar alındı; mimari ve sabitler alınmadı |
| P4 | Pointer kimliği, dirty state, odaklı zoom, mekânsal eleme fikri | HTML/CSS, `index.html`, `ViewportTransform.js`, `requestAnimationFrame`, tarayıcı varsayımları ve %75 overscan | Teknoloji yığını yanlış; uygulama planı reddedildi |
| B | Güncel yerel kaynaklar, gerçek parser köprüsündeki kayıplar, native pointer paketleri, session sahipliği, cache/LOD, uçtan uca doğrulama | İlk taslakta yeterince açılmayan paket denetimleri P1'den tamamlandı; bırakma olayındaki son gerçek örnek kuralı netleştirildi; aşırı geniş tek CAD aşaması bölündü | Nihai planın kapsam ve teknik karar tabanı |

### Reddedilen önerilerin somut nedenleri

1. **P2 Bileşen 1:** Her Running olayında yeniden başlatılan 150 ms debounce, sürekli hareket boyunca hiç çalışmayabilir. Pan'ı bitmap sınırında durdurmak da çizimi parmağın gerisinde bırakır. %25 her kenar, %25 fazla toplam bitmap değildir: boyutlar 1.5 kat, piksel alanı **2.25 kat** olur.
2. **P2 Bileşen 2:** `pinchSessionBaseScale * pne.Scale` korunuyor. MAUI `Scale` önceki güncellemeye göre göreli olduğu için asıl ölçek hatası çözülmüyor. Frame oranını 0.85–1.18 aralığına sıkıştırmak yanlış hesabı düzeltmez. [Microsoft pinch sözleşmesi](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/pinch?view=net-maui-10.0)
3. **P2 Bileşen 4:** “Packed Hilbert/STR R-Tree” denilen kod, kökte `ceil(N/16)` çocuk oluşturup çocukları doğrudan yaprak yapıyor. Dengeli çok seviyeli ağaç kurmuyor; 100 bin entity için kökte yaklaşık 6250 kutu taranır. Sonuçları mekânsal sıra ile çizmek de CAD üst üste binme sırasını değiştirir. İddia edilen 5 ms → 0.1 ms kazanımı ölçülmemiştir.
4. **P2 Bileşen 5–7:** JPEG yerine PNG koymak kodlama, byte[] üretimi, çözme ve görüntü kaynak değişimini kaldırmaz. Pattern hatch'i opak solid'e çevirmek arkadaki çizgileri örtebilir. Kalite parametresinin yorum satırında bırakıldığı örnek uygulanabilir bitmiş kod değildir.
5. **P2 Bileşen 6:** Fit'e dönüş önizlemesinde `fitWupp/currentWupp` kullanılması uzaklaşma gerekirken görüntüyü büyütebilir; görüntü ölçeği ters orana ihtiyaç duyar. Farklı zoom limitleriyle kamera kurmadan önce mevcut WUPP'nin aralıkta olduğunun denetlenmemesi de exception üretebilir.
6. **P1 güncellik:** Yerel kod artık `Aspect.Fill`, gerçek viewport ölçüsü ve 1.35 buton çarpanı kullanıyor. P1'deki `AspectFit`, 1080×1080 ve 1.25 tespitleri HEAD sürümüne dayanıyor. Bunlar mevcut hatanın kanıtı olarak kullanılmayacak.
7. **P1 mimari allowlist:** Yalnız iki Viewer dosyasında `SkiaSharp` izni verip aynı zamanda `MauiProgram.cs` içine `using SkiaSharp...` ekletmek kendi içinde çelişkili. Nihai planda bootstrap uzantısı izinli Viewer dosyasındadır.
8. **P1 pointer kuralları:** Topoloji değişiminde baseline yenilemek doğrudur; sonraki gerçek Move'u ayrıca atmak gereksiz hareket kaybıdır. Kabul edilmiş sürüklemede pointer'ın view sınırını aşması geçerli olabilir. Android `ACTION_CANCEL` bütün gesture'ı bitirir; tek ID silmek hayalet pointer bırakabilir. [Android dokunma modeli](https://developer.android.com/develop/ui/views/touch-and-input/gestures/scale)
9. **P1 snapshot ömrü:** Lock altında session referansı alıp lock dışında çizerken eski session'ı hemen dispose etmek güvenli değildir. Immutable referans tek başına kaynak ömrünü korumaz; render lease gerekir. Aynı şekilde değişebilir LayerTable'ı iş parçacıkları arasında paylaşmak snapshot sayılmaz.
10. **P1 GPU fallback:** Yalnız ilk paint içindeki `GRContext == null` koşulu, paint callback hiç gelmezse çalışmaz. Kurulum hatası, ilk frame zaman aşımı ve context kaybı için ayrı, sınırlı kurtarma tanımlandı.
11. **P3/P4 overscan:** Sorgu kutusunu büyütmek, çizilmemiş bölgeyi görüntüye dönüştürmez. Tam ekran clip dışına çizilen içerik bir sonraki frame için otomatik tampon olmaz. %50 her kenar 4 kat; %75 her kenar 6.25 kat sorgu alanıdır. Boşluğu önleyen esas kural, her güncel kameranın görünür alanını hareket sürerken çizmektir.
12. **P3 koordinatlar:** Tek bir global origin çıkarıp bütün koordinatları float yapmak, çok geniş sahnelerin her bölgesinde hassasiyeti garanti etmez. Kamera matrisi de ekran Y yönünü ve double → float sınırını kendiliğinden çözmez.
13. **P3 inertia:** Kare başına `velocity *= 0.92`, 60 ve 120 Hz'de farklı sürede yavaşlar. Üstelik kullanıcı yeni inertia istemedi. İlk kararlı sürümde inertia kapalıdır.
14. **Tüm dar planların ortak açığı:** Kamera sınıfı testlerini geçirmek, dosya açma → extraction → gerçek yüzey zincirini doğrulamaz. Parser köprüsündeki görünür kayıplar düzeltilmeden “pürüzsüz ve güvenilir uygulama” tamamlanmış sayılamaz.

## Güncel koddan doğrulanan sorun haritası

Satır numaraları uygulama sırasında değişeceğinden metodun adı esas alınacaktır.

| Mevcut konum | Gözlenen durum | Nihai karşılık |
|---|---|---|
| `MainPage.cs`, PanUpdated | Running yalnız Image taşır; kamera Completed/Canceled'da güncellenir | Native input her örnekte kamerayı günceller; frame hareket sırasında çizilir |
| `MainPage.cs`, PinchUpdated | Göreli Scale sabit tabanla kullanılır; önizleme ve bitiş odağı farklıdır; 250 ms guard vardır | Tek gesture hesabı; iki parmağın önceki/yeni merkezi ve mesafesi |
| `MainPage.cs`, ReRenderAsync | Bitmap → JPEG → MemoryStream → ImageSource; `_renderSeq` yalnız render sayacıdır | Direct DrawFrame; tüm session/surface kimlikleriyle frame gate |
| `MainPage.cs`, CloseActiveDrawing | Aktif renderer işini ve parser oturumunu bu metot doğrudan sonlandırmıyor | Tek session kapanışı ve lease bitiminde kaynak bırakma |
| `MainPage.cs`, OpenSelectionAsync | Extraction ve scene build await sonrası doğrudan yürütülüyor | Lease ile işçi üzerinde hazırlık ve son generation kontrolü |
| `SkiaCadRenderer.RenderAsync` | Her entity taranır; style, path ve bazı kaynaklar tekrar hazırlanır | BVH sorgusu; sınırlı cache; ortak painter |
| `Camera2D` | Temel odaklı zoom, pan ve double koordinat dönüşümü zaten var | Korunur; simultaneous manipulation ve viewer sınırları eklenir |
| `CadExtractedSceneBuilder` | Closed=false; source order ve gerçek CadStyle taşınmıyor; destek listesi dar | Kayıpsız, açık DTO ve üretim entegrasyonu |
| `AcadSharpEntityExtractor` | INSERT Other ankrajı; MTEXT düz Text; sınırlı ACI eşlemesi | Mevcut block/text/style altyapısına gerçek belge verisi |
| `CadLayoutManager` | Viewport içeriği primitive listesine indirgeniyor | İç entity kimliği, stil, sıralama ve viewport override korunur |
| `TextPrimitive.CalculateBounds` | Tahmini karakter genişliği; çizilen gerçek glifle aynı hesap değil | Çizim ve bounds için aynı text layout sonucu |
| `MainActivity.OnTrimMemory` | `SafeCadFileCache.PurgeAll()` bütün geçici dosyaları tarar; aktif lease ayrımı yok | Aktif dosyaları koruyan sahiplik tablosu ve sadece sahipsiz dosya temizliği |
| A11 runner ve benchmark | Kamera çağrısı/sentetik durum gerçek dokunma gibi sunulabiliyor | Katmanına göre test etiketi; native olay enjeksiyonu ve release APK doğrulaması |

## Bağlayıcı mimari ve davranış kararları

### Tek sahip ve tek çizim yolu

```text
Android MotionEvent
  → AndroidViewportInputAdapter
  → ViewportInteractionEngine
  → CadViewerSession içindeki tek ViewportController
  → FrameRequestGate + Android Choreographer
  → RenderSnapshot lease
  → SkiaScenePainter
  → SKGLView / gerekirse SKCanvasView
```

`CadViewerSession` mevcut sınıfın geliştirilmiş hâlidir. MainPage, view ve session üç ayrı kamera tutmayacak. MainPage belge komutlarını ve UI'yı, CadViewportView yüzeyi, session belge/render durumunu yönetir. Mevcut session test API'leri ince uyumluluk metotlarıyla aynı controller'a yönlendirilir; ikinci kamera alanı bırakılmaz.

Canlı viewer'da JPEG/PNG kodlama, Image.Scale/Translation önizlemesi ve her Move için Task.Run bulunmaz. Export, thumbnail ve tarihsel koşullu PNG testleri ortak painter üzerinden çalışmaya devam eder. “Repoda hiç PNG kodu kalmasın” şeklinde genel temizlik yapılmaz.

### Giriş sözleşmesi

| Davranış | Kesin karar |
|---|---|
| Tek parmak | 1:1 pan; touch slop aşıldığında o ana kadarki gerçek yer değiştirme bir kez uygulanır |
| İki parmak | Aynı hesapta pinch ve merkez hareketi; ayrı ikinci pan uygulanmaz |
| 1→2→1 / aynı sayıda ID değişimi | Geçerli pointer kümesinden baseline yenilenir; sonraki gerçek Move kaybedilmez |
| Üç veya daha fazla parmak | Kamera durur, pointer konumları izlenir; tekrar 1/2 parmakta yeni baseline |
| Cancel / odak kaybı | Bütün pointer ve tap adayları temizlenir; son geçerli kamera korunur |
| View dışına sürükleme | View içinde başlamış aktif gesture, Android teslim ettiği sürece izlenir; koordinat sırf sınır dışında diye clamp/reset edilmez |
| Artı / eksi | Ekran merkezinde 1.35 ve 1/1.35 |
| Çift dokunma | Dokunulan noktada 2× yakınlaştırma; Fit ayrı düğmedir |
| Fit | Aktif layout'un geçerli görünür içeriği, her kenarda %5 boşluk |
| Inertia, elastic overscroll, zoom easing, rotation gesture | Bu sürümde kapalı |
| Ölçüm modu | Tek dokunma ölçüm noktasıdır; double-tap zoom devre dışıdır; pan/pinch kullanılabilir |

Çift dokunmanın mevcut “bazen zoom, bazen fit” davranışının değiştirilmesi **bilinçli ürün kararıdır**: aynı hareket her zaman aynı işi yapacaktır. İlgili Stage11 beklentisi, bu gerekçe ve yeni bağımsız testle güncellenir; tarihsel sonuç dosyaları değiştirilmez.

Touch slop, double-tap slop ve süreleri Android `ViewConfiguration` değerlerinden adaptöre enjekte edilir. Host testleri bu değerleri fixture içinde sabitler. `Stopwatch` ile işlenme anı yerine olayın `MotionEvent.EventTime` zamanı kullanılır. Gesture süreleri render gecikmesinden etkilenmez.

### Koordinatlar ve pinch formülü

- Kaynak dünya verisi, kamera merkezi ve WUPP `double` kalır.
- Ekran: X sağa, Y aşağı; dünya: X sağa, Y yukarı.
- Kamera ölçüsü gerçek framebuffer pikselidir. SKGLView için `IgnorePixelScaling=false`, `HasRenderLoop=false`.
- Native `GetX/GetY`, ilgili native view'in yerel piksel konumudur. Render yüzeyi farklı boyuttaysa yalnız adaptörde `rawSurfaceWidth/nativeView.Width` ve yükseklik oranı uygulanır. Native piksel ikinci kez display density ile çarpılmaz.
- MAUI DIP olarak gelen UI konumları varsa ayrı adlandırılmış dönüşümle gerçek yüzey oranından çevrilir. DIP toleransları bir defa piksele dönüştürülür.

İki parmağın önceki merkezi `m0`, güncel merkezi `m1`, mesafeleri `d0,d1`, eski WUPP `w0`, yüzey boyutu `W,H` olsun:

```text
q  = ScreenToWorld(m0, camera0)
f  = d1 / d0
w1 = clamp(w0 / f, minWupp, maxWupp)
Cx = q.X - (m1.X - W/2) * w1
Cy = q.Y + (m1.Y - H/2) * w1
camera1 = Camera(W, H, (Cx,Cy), w1, limits)
```

Bu formül ölçek sınırında da güncel merkezin altındaki dünya noktasını korur. Aynı olaya ayrıca `Pan(m1-m0)` uygulanmaz. Gerçek pozitif faktörü 0.5–2 gibi keyfî olay başına aralıkta kesmek yoktur; seyrek gelen örnekler de aynı sonuca ulaşmalıdır.

`minSpanPx = max(8, 2 * touchSlopPx)`. Önceki veya güncel parmak mesafesi bu değerden küçükse ölçek faktörü 1 olur, merkez hareketi uygulanır ve mesafe baseline'ı yenilenir. NaN/Infinity veya sonlu kamera üretemeyen giriş son geçerli kamerayı değiştirmez; diagnostic ve yeniden baseline oluşturur.

**Bırakma kuralı:** UP/POINTER_UP paketinde daha önce görülmemiş gerçek bir son konum varsa mevcut pointer kümesiyle son incremental örnek bir kez işlenir; sonra ID çıkarılır. Önceki Move ile aynı konumda kamera farkı sıfırdır. Gesture boyunca birikmiş TotalX/TotalY veya toplam scale bırakmada yeniden uygulanmaz. “UP anında mutlak sıfır mutation” şartı yerine **çifte commit yok, gerçek son örnek kaybı yok** şartı kullanılır.

Bu kural gesture state'ini atlamaz: tek parmak hâlâ `TapCandidate` ise ve UP dahil toplam hareket slop'u aşmamışsa kamera değişmez, yalnız geçerli tap üretilir. UP örneğinde slop ilk kez aşılmışsa toplam gerçek yer değiştirme bir kez pan olur; tap olmaz. POINTER_DOWN'da da önce zaten izlenen ID'lerin paketteki son konumları eski state ile bir kez işlenir, sonra yeni ID eklenip baseline kurulur. Yeni parmağın eklenmesi/çıkarılması kaynaklı centroid farkı hareket değildir. Gerçek kamera değeri değişmeyen packet `CameraRevision` artırmaz; biten interaction'ın kalite değişimi ayrı dirty nedenidir.

### Zoom sınırları

Viewer politikası, genel Camera2D tipinden ayrıdır. Kaynak guard'ın `|coordinate| <= 1e12` sınırı korunur. İlk geçerli surface ve her açık Fit komutunda:

```text
fitWupp = geçerli bounds ve yüzde 5 padding ile bulunan WUPP
M       = max(1, abs(cameraCenter.X), abs(cameraCenter.Y), abs(anchor.X), abs(anchor.Y))
ulp(M)  = Math.BitIncrement(M) - M
minWupp = max(1e-12, 8 * ulp(M))
maxWupp = max(minWupp, min(1e12, 16 * fitWupp))
```

Pan/pinch sırasında precision floor güncel merkez/anchor için yeniden denetlenir. Aralık daralırsa eski WUPP önce geçerli aralığa alınır ve yeni kamera aynı odak denklemiyle oluşturulur; constructor'a aralık dışı değer verilmez. Kamera kendiliğinden Fit'e dönmez. Bu, P1'deki bütün sahnenin en uzak koordinatından üretilen sabit 65536× zoom sınırı yerine yerel sayısal doğruluğu korur. `M`, toplama/çarpma ve visible bounds taşması ayrıca denetlenir.

Boş sahne için merkez `(0,0)`, WUPP `1`, boş çizim durumu; **iki boyutu da sıfır olan tek nokta** için bir çizim birimi sanal Fit kapsamı kullanılır. Yalnız bir boyutu sıfır olan yatay/dikey çizginin pozitif boyutu normal Fit'i belirler; küçük bir çizgi gereksiz yere bir dünya birimine büyütülmez. Geçersiz extent otomatik muazzam ölçek üretmez. Uzak fakat geçerli bir entity sessizce atılmaz.

### Frame, kaynak ve önbellek sözleşmesi

1. Tüm pointer'ları aynı anda içeren bir native olay paketi atomik işlenir. State lock yalnız mutation ve snapshot/lease alma sırasında tutulur; çizim, extraction ve dispose lock altında yapılmaz.
2. Snapshot kimliği: `DocumentGeneration, SceneRevision, LayoutRevision, StyleRevision, CameraRevision, SurfaceGeneration, PixelSize, QualityMode`. Alanlar aynı state snapshot'ından gelir.
3. En fazla bir aktif paint ve bir bekleyen invalidation vardır. Input kamerayı günceller; Choreographer bir sonraki ekran fırsatına tek callback planlar. Ara kamera durumları kuyruğa alınmaz. [Android Choreographer](https://developer.android.com/reference/android/view/Choreographer)
4. Paint callback girişinde alınan snapshot ile tamamlanabilir. Bu sırada kamera ilerlemişse hemen en güncel durum için bir sonraki frame istenir; bitmiş ara frame yüzünden sonsuz discard döngüsü kurulmaz. Başka belge, eski surface veya eski layout'a ait async sonuç yeni session'a yayınlanamaz. Native yüzeye gönderilmiş frame sonradan generation kontrolüyle geri alınamaz; belge/layout geçişinin görsel sınırı Aşama 05'teki geçiş örtüsüyle korunur.
5. Paint exception, erken dönüş ve surface kaybında pending bayrağı `finally` ile çözülür. Reset sonrası eski callback'in yeni gate'i sıfırlamaması generation ile sağlanır.
6. Session değiştirilirken eski oturum “retiring” olur. Aktif paint/preparation lease'leri bitince kaynaklar kendi sahip thread'lerinde bırakılır. Referansı alındı diye bitmap/path hemen dispose edilmez.
7. `LayerTable` güncellemesi immutable style snapshot yayımlar. Layer/theme değişimi geometry index'i yeniden kurmaz. Geometri, font metric veya layout değişimi etkilenen bounds/cache revision'ını değiştirir.
8. Worker yalnız CAD extraction, indeks ve CPU geometri hazırlığı yapar. GPU context, SKCanvas ve GPU'ya bağlı nesneler worker'a gönderilmez. GPU çizimi kendi callback'inde yürür.
9. Ek viewer cache bütçesi toplamı `clamp(AndroidMemoryClassMiB / 8, 16, 64) MiB`; low-RAM cihazda 16 MiB. Bu toplam geometri, raster, text ve GPU cache tahminlerini kapsar; parser belge belleği ayrı ölçülür. LRU eviction yalnız kullanımda olmayan kaynağı bırakır. Ölçülen toplam maliyet bütçeye sığmayan tek nesne cache'e alınmaz.
10. Önceki bitmap'i büyüterek boşluk örtme yoktur. Her paint güncel visible bounds'u sorgular. Stroke/glif güvenlik payı dışındaki spekülatif overscan başlangıçta **0**; bir gelecekteki optimizasyon, bu doğruluk kuralını değiştiremez.
11. Frame callback'in UI thread'inde çalıştığı varsayılmaz. `AndroidFrameClock` callback ekleme/kaldırmayı UI dispatcher üzerinden yapar; GL callback'i yalnız lease'li immutable state okur. View ölçüsü, visibility, density ve lifecycle bilgisi UI tarafından yayımlanır; GL thread'i MAUI kontrollerini okumaz/değiştirmez. UI veya GL thread'i karşı tarafı `.Wait()`/`.Result` ile beklemez.
12. OS/Skia surface yaratma, resize veya expose nedeniyle talep edilmeden paint çağırabilir. Her callback geçerli surface generation ve gerçek boyutlarla aynı gate'e girer; uygulama request'i olmaması boş/yanlış frame sebebi değildir. `scheduled`, `awaitingPaint`, `painting` ayrı durumlarıdır; tek boolean ile hepsi temsil edilmez. Paint admission ile snapshot aynı kritik bölgede alınır. Eski/duplicate completion yeni ticket'ı tamamlayamaz.

## Aşamaların özeti

| Sıra | Tamamlanacak sonuç |
|---|---|
| 01 | Güncel kaynak tabanı, gerçek test ve ölçüm altyapısı |
| 02 | Paket sınırları ve ortak doğrudan Skia painter |
| 03 | Kamera, koordinat ve zoom matematiğinin kesinleştirilmesi |
| 04 | Native çoklu dokunma ve tek gesture state machine |
| 05 | Tek session, frame scheduler ve canlı viewer entegrasyonu |
| 06 | Güvenilir bounds ve draw order koruyan BVH |
| 07 | Bellek bütçeli hazırlık, cache ve kontrollü LOD |
| 08 | Gerçek dosya açma ve kayıpsız parser köprüsü |
| 09 | Temel geometri, OCS ve block doğruluğu |
| 10 | Metin, ölçülendirme ve hatch doğruluğu |
| 11 | Layout, referanslar, Fit, katmanlar ve ölçüm araçları |
| 12 | Dosya ve yüzey yaşam döngüsü, kaynak temizliği, hata kurtarma |
| 13 | Gerçek APK'da uçtan uca doğruluk ve performans kabulü |
| 14 | CI, sürüm kanıtları ve kalıcı regresyon kilidi |

**İlk görünür iyileştirme hedefi Aşama 05'tir:** parmak bırakmadan doğru pan/pinch ve yeni alan çizimi. Sonraki aşamalar büyük dosya akıcılığını, gerçek CAD doğruluğunu ve tekrar açılış güvenilirliğini tamamlar. Aşama 05 sonunda bütün uygulama bitmiş ilan edilmez.

**Aşama bağımlılık kuralı:** Yeni kullanılan tip ve test altyapısı, ilk kullanıldığı aşamada derlenebilir ve çalışabilir olmalıdır. Aşama 02 minimal snapshot/context ve sentetik surface smoke'u kurar; Aşama 05 session sahipliğini ve gerçek native smoke runner'ını tamamlar. Aşama 06–07 mevcut primitive modeliyle doğrulanır; Aşama 08–11 yeni veri tiplerini bağladıkça ilgili bounds/cache testleri aynı değişiklikte genişletilir. Sonraki aşamaya TODO bırakarak önceki aşamanın zorunlu kontrolü geçmiş sayılmaz.

## Aşama 01 — Güncel kaynak tabanı ve ölçüm

**Amaç:** Eski GitHub dosyasına yanlış düzeltme uygulamamak ve gerçek uygulama davranışını ölçebilmek.

**Dosyalar:** Yeni `scripts/viewer-stability-gate.ps1`, `src/MobilDwg.Rendering/Performance/ViewportTelemetry.cs`, `tests/MobilDwg.Integration.Tests/`; mevcut `tests/MobilDwg.Architecture.Tests/Program.cs`, `MobilDwg.sln`, `docs/ARCHITECTURE.md`; mevcut benchmark yalnız inceleme ve uygun testlerinin taşınması için.

**Uygulama sırası:**

1. `git status --short`, HEAD, upstream SHA, değiştirilmiş dosya diff'leri ve takip edilmeyen kaynakların SHA-256 manifestini kaydet. Snapshot içine kullanıcıya ait CAD dosyası, keystore ve gizli veri alma. Diske bakmadan remote main üzerinden başlamayı yasakla.
2. Var olan değişiklikleri koruyarak `codex/viewer-stability-v3` dalını mevcut çalışma ağacından oluştur. Aynı ad varsa başlangıç manifestiyle eşleşen devam dalını kullan; ilgisiz aynı adlı dal varsa `codex/viewer-stability-v3-YYYYMMDD-HHMMSS` adlı yeni dal oluştur, hiçbir dalı üzerine yazma. `reset --hard`, otomatik stash/drop veya kullanıcı değişikliklerini kaybettiren temiz checkout yapma.
3. Bu plandaki hash'ler değişmişse etkilenen metotları yeniden eşleştir, başlangıç manifestini güncelle. Sırf yeni commit geldi diye çalışmayı tamamen durdurma. Davranış/mimari çelişki varsa etkilenen maddeyi kanıtıyla işaretle; eski satır numarasına kör patch uygulama.
4. Core/Rendering/Architecture konsol testlerini ayrı ayrı çalıştır; sıfır exit code ve beklenen test bitiş işaretini birlikte ara. Test dosyası eklenince konsol `Program.cs` çağrı listesine bağlanmasını denetle.
5. `MobilDwg.Integration.Tests` test projesi Core, Cad ve Rendering'e referans veren konsol harness olsun. Gerçek fixture dosyasını okuyup extractor/builder çıktısını denetleyebilsin; üretim dependency yönünü değiştirmesin. Aynı değişiklikte Architecture `Program.cs` içindeki `testProjects.Length == 3` şartını Core.Tests, Rendering.Tests, Architecture.Tests ve Integration.Tests'in **dört exact csproj yolu** ile değiştir; her projenin referans yönünü doğrula. Yeni projeyi solution ve gate'e bağla. Aşama 05'te instrumentation eklendiğinde liste tam beş yol olacak; sınırsız proje izni verme. Kaynak denetimi `bin/obj` gibi derleme çıktısını değil gerçek proje kaynaklarını tarasın; gizli alt klasöre izin veren geniş source allowlist kullanma. `docs/ARCHITECTURE.md` test sözleşmesini aynı aşamada güncelle.
6. Mevcut `CadControlBenchmark` testlerini sınıflandır: üretim sınıfını çağıran ölçüm / model simülasyonu / sabit değer kontrolü. Sabit UI ölçüleri, bağımsız inertia döngüsü ve yerel bool ile idle simülasyonu gerçek uygulama kanıtı olamaz. Faydalı testleri koru, adlarını ölçtüğü katmana göre düzelt.
7. Telemetri için sabit boyutlu 4096 örnekli ring buffer ve taşan örnek sayacı kullan. Input event time, camera revision, frame request, paint start/end, scene/index süreleri, entity/primitive/vertex adedi, backend, cache hit/miss/bytes kaydet. UI, GL ve worker yazarı ile test okuyucusu için kısa kilitli write/drain kullan; I/O kilit dışında, sıcak yolda olay başına string/JSON yok. Uzun performans koşusunda ölçüm verisini test tarafından periyodik boşalt; sadece son 120 frame üzerinden p95 iddiası üretme. Taşan veya korelasyonu kayıp koşu başarı ölçümü değildir.
8. Etkileşim sırasında her frame label/log yazma. HUD en fazla 4 Hz güncellensin; JSON/CSV kanıt test sırasında toplu alınsın. Dosya yolları ve belge adları yerine özel fixture ID kullan.
9. Sürelerin clock domain'ini kaydet. Android gesture ve Input→Paint ilişkisi `MotionEvent.EventTime` / `SystemClock.UptimeMillis()` ortak tabanında hesaplanır. İç işlemlerin hassas süreleri aynı `Stopwatch` clock'unda ölçülebilir; bu tick değeri doğrudan EventTime'dan çıkarılmaz. Perfetto sunum ilişkilendirmesi trace clock snapshot/kalibrasyonuyla yapılır; suspend/resume sonrasında eşleme yenilenir. UtcNow veya deep sleep'i farklı sayan elapsedRealtime ile doğrudan çıkarma yoktur. [MotionEvent zaman tabanı](https://developer.android.com/reference/android/view/MotionEvent#getEventTime()), [SystemClock](https://developer.android.com/reference/android/os/SystemClock).

**Geçiş koşulları:**

- [ ] Kullanıcının yerel değişiklikleri snapshot manifestinde; kayıp dosya yok.
- [ ] Üç mevcut harness gerçekten çalıştı; sonuç ve exit code saklandı.
- [ ] Parser entegrasyon test harness'i gerçek dosya okuyabiliyor.
- [ ] Yeni proje eklendikten sonra dört harness birlikte geçiyor; sayısal proje kontrolü kaldırılmadan exact proje listesi güncellenmiş.
- [ ] Ölçülen sürelerin anlamı ayrı: parse, extraction, scene build, paint, sunum ve input gecikmesi.
- [ ] APK kaynağı bilinmeyen emülatör görüntüsü baseline performans sonucu olarak kullanılmadı.

**Commit konusu:** `test(viewer): establish source baseline and telemetry`

## Aşama 02 — Ortak painter ve doğrudan Skia yüzeyi

**Amaç:** Canlı çizimi codec ve MAUI Image güncelleme zincirinden ayırmak.

**Dosyalar:** `Directory.Packages.props`, App csproj/lock, `MauiProgram.cs`, `docs/ARCHITECTURE.md`; yeni `App/Viewer/CadViewportView.cs`, `App/Viewer/ViewerHostingExtensions.cs`, `Rendering/Skia/SkiaScenePainter.cs`, `Rendering/Skia/RenderFrameContext.cs`, `Rendering/Viewer/RenderSnapshot.cs`; mevcut `SkiaCadRenderer.cs`, architecture/package denetimleri ve dependency kayıtları.

**Uygulama sırası:**

1. `SkiaSharp.Views.Maui.Controls [4.151.1]` App'e ekle. Mevcut .NET 10.0.400, MAUI 10.0.100, ACadSharp 3.7.1 ve SkiaSharp 4.151.1 pinlerini koru. App `packages.lock.json` restore ile üretilecek; elle yazılmayacak. Paket resmî olarak vardır; gerçek Android restore/build yine zorunludur. [NuGet 4.151.1](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls/4.151.1)
2. `ViewerHostingExtensions.UseCadViewport()` içinde `.UseSkiaSharp()` çağrısını sakla. `MauiProgram` yalnız `.UseCadViewport()` kullanır. App'te doğrudan Skia kullanım izni `Viewer/CadViewportView.cs` ve `Viewer/ViewerHostingExtensions.cs` ile sınırlıdır. Gerekli test probe'u Skia API çağırmadan view'in tanılama arayüzünü kullanır.
3. Architecture harness'ında yeni App paketini ve bu iki exact path'i denetle. App'in diğer dosyalarında Skia, App genelinde parser'ın concrete `ACadSharp` tipleri yasak kalır. Native Android adaptörü MAUI view'in `Handler.PlatformView` değerini `Android.Views.View` üzerinden ele alır.
4. `SkiaScenePainter.DrawFrame(SKCanvas, RenderSnapshot, RenderFrameContext)` senkron çekirdeğini çıkar. Snapshot ve context'in gereken minimal immutable tiplerini **bu aşamada** oluştur; Aşama 05'te tanımlanacak tipe bağımlı derlenmeyen kod bırakma. Snapshot mevcut scene, kopyalanmış style/layer görünümü ve kamerayı içerir; session revision/lease yönetimi Aşama 05'te bağlanır. Mevcut renk, geometri, text, hatch, raster ve clip davranışını bu aşamada değiştirme. `RenderFrameContext` yüzey boyutu/density, quality ve render thread'ine ait resource cache/buffer'ları taşır.
5. `SkiaCadRenderer.RenderAsync` bitmap adapter olarak aynı painter'ı çağırsın. Offscreen PNG/JPEG yardımcılarını koru. İki farklı geometri çizim implementasyonu oluşturma.
6. CadViewportView içinde önce SKGLView oluştur; `IgnorePixelScaling=false`, `HasRenderLoop=false`, **`EnableTouchEvents=false`** kullan. Native input Aşama 04'te bağlanacağı için Skia touch ile ikinci gesture yolu açılmasın. SKCanvasView ihtiyaç doğduğunda oluşturulan kurtarma yüzeyidir; görünmez ama sürekli bağlı ikinci yüzey tutulmaz.
7. Gerçek surface boyutunu callback'ten al; GL için `RawInfo`, CPU için ilgili surface info doğrulanır. Skia'ya ait yüzey/context callback dışına taşınmaz. İlk belge/layout kamerası henüz yoksa ilk Fit boyutlar geçerli olduktan sonra yapılır. Var olan belgenin surface yeniden kurulması, resume veya CPU'ya geçişi Fit gerekçesi değildir; center/WUPP korunur. Sentetik GL/CPU smoke'u yalnız validation build'inde çağrılan surface host bağlantısıyla çalıştır; normal kullanıcı akışına test çizimi ekleme.
8. `scripts/stage02-audit-packages.py` yeni central/package setini ve **App lock graph'ını** doğrulasın. Legacy probe graph'ını bozma. NuGet artifact hash, resolved graph ve native inventory gerçek restore çıktısından alınsın; varsayılan transitive isimleri elle uydurma. Resolved Skia Views ailesi 4.151.1, MAUI ailesi mevcut pinle uyumlu olmalı.
9. `compliance/DEPENDENCY_EVIDENCE.md`, ilgili manifest, `CadReleaseRcAuditor`, `CadFinalRcAuditor` ve notices yeni UI paketini tanısın. Repo politikasını atlatan audit skip ekleme. Bu, yeni parser lisansı veya motor değişimi değildir.
10. Mimari bridge ve paket sınırını `docs/ARCHITECTURE.md` içinde aynı değişiklikte güncelle; Aşama 14'e kadar eski yasak metni bırakma. App lock restore girdileri ve test/üretim build configuration'ları kayda girsin; validation build'i normal Release lock grafiğini sessiz değiştirmesin.

**Geçiş koşulları:**

- [ ] Locked restore, Android Release build, architecture ve dependency audit başarılı.
- [ ] Doğrudan canvas'ta sentetik çizim hem GL hem CPU yüzeyinde görülebiliyor.
- [ ] Mevcut bitmap testleri ortak painter'dan geçiyor; geometri algoritması iki kopyaya ayrılmadı.
- [ ] UI-only bridge sınırı testi doğru dosyaları kapsıyor; bootstrap kendi kuralını ihlal etmiyor.
- [ ] Normal canlı surface denemesinde JPEG/PNG kodlama sayısı sıfır.

**Commit konusu:** `refactor(render): introduce audited direct Skia painter`

## Aşama 03 — Kamera ve sayısal sözleşme

**Amaç:** Aynı input, olay sıklığı veya density farkından bağımsız aynı kamerayı üretsin.

**Dosyalar:** `Camera2D.cs`, `ViewportController.cs`; yeni `Camera/ViewerZoomPolicy.cs`, `Interaction/ViewportInputContracts.cs`, `tests/MobilDwg.Rendering.Tests/ViewportCameraTests.cs`.

**Uygulama sırası:**

1. Yukarıdaki moving-centroid formülünü `Camera2D.Manipulate(previousCentroid,currentCentroid,factor)` olarak ekle. Mevcut ZoomAt/PanBy dönüşümleri korunur.
2. ViewerZoomPolicy'de belirtilen Fit, ULP ve min/max hesaplarını uygula. Kameranın geçerli constructor sözleşmesini gevşetme; geçersiz input engine sınırında elenir.
3. `FitExtents`, `ZoomIn/Out`, `DoubleTap`, resize ve layout kamera saklama yollarının hepsi aynı policy'yi kullanır. DoubleTap daima odakta 2× olur. Resize center/WUPP korur; kendi başına Fit yapmaz.
4. İlk boş/tek noktalı/ince uzun sahne durumlarını açıkça ele al. Bounds merkezi hesaplarken mevcut taşmayı önleyen half-before-add yöntemini koru.
5. Testlerde screen/world round-trip, odaklı zoom, eşzamanlı iki parmak kaydırma ve clamp'te odak korunumunu gerçek formülden bağımsız bilinen nokta beklentileriyle denetle.
6. Olay sıklığı testinde aynı fiziksel yolu 15, 30, 60, 120 Hz örnekle; doğrusal pan ve sabit merkezli pinch'in son geometrik sonucu eşleşsin. Geçerli büyük faktorleri keyfî event clamp ile kırpma.

**Geçiş koşulları:**

- [ ] Adım başına odak hatası ≤0.25 fiziksel piksel; kontrollü 1000 pinch in/out çiftinde toplam drift ≤0.5 piksel.
- [ ] `5e6` merkez çevresindeki `0.001` dünya birimi detay ve `1e12` sınır örnekleri sonlu, tanımlı davranış üretiyor. Yetersiz temsil hassasiyeti sessiz titreme yerine zoom sınırında kalıyor.
- [ ] Köşeler, negatif ekran koordinatı, merkez, çok küçük span ve min/max zoom testleri başarılı.
- [ ] Fit/double-tap/resize sonrası limitler yanlış default'a dönmüyor.
- [ ] Mevcut Stage11 değişikliği yalnız bilinçli double-tap ürün davranışıyla açıklanmış; diğer regresyonlar korunmuş.

**Commit konusu:** `fix(camera): define focal manipulation and precision limits`

## Aşama 04 — Native input ve gesture state machine

**Amaç:** MAUI pan/pinch yarışlarını kaldırmak, gerçek parmak kimliğini ve son hareket örneğini korumak.

**Dosyalar:** Yeni `Rendering/Interaction/ViewportInteractionEngine.cs`, `App/Viewer/Platforms/Android/AndroidViewportInputAdapter.cs`, `ViewportInteractionTests.cs`; Aşama 03 input sözleşmeleri.

**Uygulama sırası:**

1. Native surface view'in Touch olayına handler bağlanınca abone ol, handler değişince eski aboneliği kaldır. Aynı view'de Skia Touch veya MAUI Pan/Pinch/TapRecognizer çalıştırma. Etkin viewer üzerinde geçerli DOWN'dan itibaren `e.Handled=true` ile gesture dizisini sahiplen; slop aşılana kadar false dönme. Kamera hareketinin slop beklemesi, event sahipliğinin beklemesi değildir. [Android DOWN tüketimi](https://developer.android.com/develop/ui/views/touch-and-input/gestures/detector)
2. `PointerPacket` içinde action, ActionIndex'in işaret ettiği **PointerId**, event time, bütün aktif ID/konumlar ve surface generation gönder. Olay nesnesi callback sonrası saklanmaz; değerler kendi buffer'ına kopyalanır.
3. Bir MotionEvent içindeki bütün pointer konumlarını aynı packet olarak işle. History varsa örnekleri zaman sırasında aynı küme ile işle; böylece sınır geçişleri ve son örnek izlenebilir. ID ile index'i birbirine karıştırma.
4. State'ler `Idle`, `TapCandidate`, `Pan`, `Pinch`, `MultiTouchHold`, `Suspended` olsun. Bölümdeki geçiş kurallarını uygula. 1→2 veya 2→1 sonrasında olay kümesinin mevcut konumları baseline olur; `suppressNextMove` yoktur.
5. `ACTION_CANCEL` tüm input durumunu temizler. Native Up son örneği işler, ID'yi çıkarır. 1→2→1 zinciri arasında tap adayı yeniden canlandırılmaz; bütün parmaklar kalkmadan yeni tap dizisi başlamaz.
6. Viewer'ın sahiplendiği DOWN'da parent'a `RequestDisallowInterceptTouchEvent(true)` ile gesture'ı bırakmamasını bildir; son UP/Cancel/detach'ta serbest bırak. View üzerinde başlayan hareketin kenar dışı konumları geçerlidir. Sistem gesture iptali ve parent tarafından gelen Cancel normal kapanış yoludur. Modal, toolbar veya ölçüm dışı UI'ya ait DOWN viewer tarafından sahiplenilmez.
7. Normal modda ilk tap'e çizimi değiştiren eylem bağlama; ikinci tap Android zaman/mesafe koşullarıyla zoom olur. Ölçüm modunda ikinci tap zoom'u devre dışı bırakılır.
8. 250 ms pinch kilidi, Task.Delay/ContinueWith input state temizliği, Image.Anchor ve Image.Scale alanları bu akışta bulunmaz. Eski üretim handler'ları Aşama 05 bağlantısında kaldırılır.

**Geçiş koşulları:**

- [ ] 1→2→1, 2→3→2, aynı sayıda ID değişimi, sıra değişen PointerIndex, Cancel, hızlı yeniden dokunma testleri başarılı.
- [ ] PointerDown/PointerUp topoloji değişikliği tek başına görüntüyü sıçratmıyor.
- [ ] Önceki Move ile aynı konumda UP kamera farkı 0; farklı son koordinatta yalnız son delta bir kez uygulanıyor.
- [ ] Sabit tutulan ikinci parmak zaman aşımıyla yanlışlıkla silinmiyor.
- [ ] Native piksel → surface oranı dışında density çarpımı yok; 1×/1.5×/2×/3× senaryoları doğru.
- [ ] View dışına çıkıp geri gelen aktif pointer kamerayı kilitlemiyor.
- [ ] Slop altındaki DOWN→UP pan üretmiyor; ilk DOWN tüketildiği için sonraki MOVE/UP geliyor; POINTER_DOWN içindeki eski pointer hareketi kaybolmuyor.

**Commit konusu:** `fix(input): unify native pointer packet handling`

## Aşama 05 — Session, scheduler ve üretim viewer bağlantısı

**Amaç:** Kamera hareket sürerken gerçek görüntüye yansısın; eski belge/frame sonucu geri gelmesin.

**Dosyalar:** `CadViewerSession.cs`, `MainPage.cs`, `CadViewportView.cs`, Aşama 02 `Viewer/RenderSnapshot.cs`; yeni `Scheduling/FrameRequestGate.cs`, `Viewer/RenderSessionLease.cs`, `App/Viewer/Platforms/Android/AndroidFrameClock.cs`, `tests/MobilDwg.Android.Instrumentation/`; Architecture/solution/gate güncellemesi ve ilgili testler.

**Uygulama sırası:**

1. CadViewerSession içinde tek ViewportController, aktif scene/layout, immutable style snapshot, revision ve kaynak sahipliğini birleştir. MainPage'in `_viewportController` alanını ve view içindeki olası ikinci camera alanını kaldır. Geçişte eski session metotları aynı controller'a delegasyon yapar.
2. Vsync isteği scene/kamera snapshot'ını önceden dondurmaz; paint callback admission'ında snapshot alma ve camera revision yakalama tek kısa kritik bölgede yapılır. Paint ticket'ın revision'ı çizilen snapshot'ın revision'ıdır; önce gate sonra farklı zamanda kamera okuyup yanlış etiketleme yapılmaz. İlk/yenilenmiş yüzey boyutu güncellemesiyle kamera ölçüsü tutarlı olmadan çizme; gerekli UI boyut bildirimi sonrasında tek güncel invalidation üret.
3. UI thread'de alınan Choreographer callback'i tek invalidation üretir. Pending paint bitmeden yenisi başlamaz; yeni input yalnız latest state'i değiştirir. Paint completion yeni revision varsa bir callback daha planlar. Dirty nedenleri camera dışında style, scene, surface, overlay ve final quality'yi kapsar.
4. Paint `try/finally` içinde ticket'ı bitirir ve lease'i bırakır. Suspend/close/recreate eski callback'leri generation ile etkisiz kılar. Çizim sırasında state lock tutulmaz.
5. MainPage normal CAD Image'ını CadViewportView ile değiştir. ReRenderAsync, preview translation/scale, ayrı MAUI pan/pinch/tap ve release-total commit yolunu normal viewer'dan kaldır. Thumbnail/export ve `#if Axx_VALIDATION` yollarını koru.
6. Buton, katman, tema, Fit ve ölçüm overlay'i session komutlarını çağırır. Katman mutasyonu frame'de kullanılan snapshot'ı değiştirmez. Bir tema değişiminde bütün entity listesini yeniden sıralayan RenderScene oluşturma.
7. Viewer görünür ve ölçülmüş olmadan tam ekran DeviceDisplay boyutuyla ilk kamera oluşturma. Yüzey hazır değilse “hazırlanıyor” durumu; **belge/layout kamerası henüz kurulmamışsa** ilk geçerli surface'te tek Fit. Orientation, backend değişimi ve surface recreate sırasında center/WUPP korunur, input state resetlenir ve yeni boyutla frame istenir. Sıfır boyut/gizlenme/pause `awaitingPaint` durumunu bırakır ve callback'i kaldırır; tekrar hazır olma olayı dirty isteği kendisi üretir, eski pending bayrağını beklemez.
8. GPU kurulumu exception verirse veya gerçek GL paint sırasında kullanılabilir GRContext yoksa CPU yüzeyine geç. Ek olarak, yalnız attached + visible + nonzero size + resumed koşullarında beklenen paint callback'i **1000 ms** içinde başlamazsa surface kurulumu bir kez yenilenir; ikinci başarısızlıkta CPU seçilir. Bu, ilk frame yanında sonradan kaybolan `awaitingPaint` callback'ini de kapsar. Watchdog tek outstanding ticket/generation'a bağlıdır; callback başlayınca veya yüzey askıya alınınca iptal olur, idle polling yapmaz. Bu süre gesture debounce değildir ve performans başarısı sayılmaz. CPU da çizemezse açık renderer hatası gösterilir; sonsuz retry yoktur.
9. Context kaybında surface generation artır, GPU cache'i geçersiz kıl, mevcut double sahneden bir kez yeniden oluştur; başarısızsa aynı host ömrü boyunca CPU'da kal. Normal CAD geometri exception'ını “GPU arızası” diye backend değiştirerek gizleme.
10. Model henüz Aşama 08'deki geniş DTO'ya geçmeden mevcut scene bu session'a bağlanır. Böylece kullanıcı zoom/pan düzeltmesini erken doğrulayabilir; parser kapsamı daha sonra tamamlanır.
11. Belge/layout değişiminde eski native frame'in kısa süre yeni belge adı altında görünmesini engelle: UI değişimi sırasında yüzey üstüne opak, hafif bir geçiş örtüsü koy; yeni state'i atomik bağla, yeni generation frame'ini iste. Örtü ancak yeni generation'ın native yüzey içeriğine bağlandığı güncelleme bildirimiyle kalkar; yalnız `PaintEnd` yeterli değildir. GL texture güncellemesi ve CPU native draw tamamlanması host içinde generation ile ilişkilendirilir; bu bildirimin fiziksel ekran sunum zamanını tek başına kanıtladığı söylenmez, görünür sıralama native screenshot/trace ile ayrıca doğrulanır. Native listener izlenirken Skia'nın mevcut listener/lifecycle zinciri ezilmez; wrapper kullanılırsa bütün çağrılar asıl listener'a iletilir ve detach'ta eski bağlantı geri konur. Eski paint kaynakları lease ile tamamlanır; UI thread'inde paint bitmesini bekleme ve bitmap kopyasıyla geçiş yapma. Kamera pan/pinch'inde bu örtü kullanılmaz.
12. Aşama 13 native test altyapısının ilk çalışan sürümünü burada kur: ayrı, yalnız test amaçlı .NET Android APK `com.smitelagwar.mobildwg.instrumentation`, kendi paketini hedefleyen Instrumentation ve `UiAutomation` ile üretim uygulamasına sistem üzerinden dokunma gönderir. Ayrı test süreci uygulama oturumunu reflection veya controller çağrısıyla değiştirmez. Runner, gesture'ları worker test thread'inde gerçek zamanlı gönderir; UI thread'inde sleep/wait yoktur. `adb shell am instrument -w` sonucu, yürütülen assertion sayısı ve test sonucu birlikte denetlenir. Architecture exact test listesi beş projeye çıkarılır. İlk set: native pan, pinch, 1→2→1, Cancel, dört yön sentinel-before-UP, GL/CPU ve resize smoke. Aşama 13'ü bekleyerek bu aşamadaki native geçiş koşullarını atlama. Test telemetry/probe derleme bayrağı normal Release'e dışarıdan çağrılabilir kontrol kapısı eklemez.

**Geçiş koşulları:**

- [ ] Gerçek native tek parmak hareketinde UP gelmeden CameraRevision ve çizilmiş revision ilerliyor.
- [ ] Görüş dışında başlayan sentinel çizgiler görünür alana girince parmak hâlâ aşağıdayken görünüyor.
- [ ] En fazla bir aktif paint ve bir pending invalidation; 100 input olayını eski kamera kuyruğu izlemiyor.
- [ ] Close/A→B açılışı sırasında A'nın biten frame veya async sonucu B'ye yayınlanmıyor; dispose edilmiş nesne kullanımı yok.
- [ ] Bir paint exception'ından sonra scheduler kilitli kalmıyor.
- [ ] 5 saniye sakinleştikten sonraki 10 saniyede uygulamanın istediği idle frame sayısı 0. OS kaynaklı surface callback'leri ayrı sayılıyor.
- [ ] GL ve zorlanmış CPU fallback için pan, pinch, resize ve close/reopen smoke başarılı.
- [ ] Sıfır→geçerli yüzey boyutu, talep edilmeden gelen paint, callback kaybı ve eski completion yeni frame'i kilitlemiyor; UI/GL thread erişim ve deadlock testleri başarılı.
- [ ] Belge/layout değişiminin ilk görünür yeni frame'i doğru generation'dan; yüzey yeniden kurulunca kullanıcı kamerası Fit'e dönmüyor.

**Commit konusu:** `fix(viewer): render live snapshots with bounded scheduling`

## Aşama 06 — Muhafazakâr bounds ve mekânsal indeks

**Amaç:** Hız kazanırken ekrana yeni giren çizgileri, metni veya CAD çizim sırasını kaybetmemek.

**Dosyalar:** `SceneGeometry.cs`, geometri bounds'ları, `TextPrimitive.cs`, `RenderScene.cs`; yeni `Spatial/StaticSceneBvh.cs`, `Text/TextLayoutMetrics.cs`, `SpatialIndexTests.cs`; painter sorgu yolu.

**Uygulama sırası:**

1. İndeksten önce primitive bounds'un gerçek geometriyi kapsadığını doğrula: bulge yay uçları/ekstremleri, elips dönüşü, spline control hull/geçerli eğri, kapalı polyline, mirrored insert, hatch ve clip kapsamı. Text bounds aynı font/layout ölçümünü kullanır; italik/oblique, hizalama, descent ve rotasyon hesaba katılır.
2. Query ekran kapsamına resolved style'ın **azami çizim taşması + 2 fiziksel piksel** AA payı ekle; sonra WUPP ile dünyaya dönüştür. Mevcut round cap/join için yarım stroke/point yarıçapı yeterlidir; square cap veya miter join kullanılan yolda cap ve miter-limit uzaması ayrıca hesaba katılır. Dünya birimi polyline genişliği ve block transform'u geometrik bounds'a dahildir; yalnız ekran stroke'u diye ele alınmaz. Azami taşma style revision'ında hazır olur, her pan'da bütün entity'leri tarayarak hesaplanmaz. Metin geometrik bounds'u ayrıca doğru olmalıdır. %50–75 genel padding kullanma.
3. Geçerli muhafazakâr bounds üretilemeyen ama çizilebilir entity'yi `alwaysTest` listesinde tut; onu indeks hatası yüzünden sessiz silme. Yapısal olarak geçersiz kaynak verisi extractor diagnostic üretir, NaN bounds oluşturmaya zorlanmaz.
4. Immutable sahne için **binary BVH** seçildi. R-Tree, quadtree ve grid arasında seçim yapılmayacak. Her düğüm toplam AABB; en uzun merkez dağılımı ekseninde median bölünme; yaprak en fazla 16 entity; eşitlikte original ordinal. Diziler ve dengeli median bölünme kullan; her entity yalnız bir yaprakta yer alır.
5. 2048'den az entity için lineer sorgu, daha fazlası için BVH. İndeks sahne hazırlığında worker'da bir kez üretilir; hazır scene+index birlikte yayımlanır. Kullanıcının hâlihazırda açık sahnesi yeni belge hazırlanırken çalışmaya devam eder. Her pan için indeks hazırlığı veya input'u yeniden kapatma yoktur.
6. Query sonucu caller-owned `List<int>`/buffer'a yazılır; ardından **original draw ordinal** sırası kullanılır. İndeks geometriyi katman/renge göre global sıralamaz. Gerçek CAD DrawOrder verisi varsa Aşama 08'de ordinal ona göre hazırlanır.
7. Candidate, visited node, bounds test ve painted primitive sayıları ayrı ölçülür. Tüm entity'ler aynı ekranı kaplıyorsa hiçbir indeks az candidate garantisi veremez; O(log N) veya %20 candidate şartı bütün sahnelere uygulanmaz.
8. Layer visibility/theme değişimi indeks rebuild etmez; geometri/font-layout değişimi etkilenen scene revision'ında rebuild/refit gerektirir. Eski indeks yeni geometri revision'ıyla kullanılamaz.

**Geçiş koşulları:**

- [ ] Sabit seed `0x4D445747`, 1000 viewport sorgusunda BVH ve brute-force candidate set'i birebir aynı; duplicate yok.
- [ ] Dört yönden giren entity, uzun çapraz çizgi, kök bölgeyi geçen entity, sıfır alanlı bounds ve ekran kenarındaki kalın stroke doğru.
- [ ] Örtüşen renk/solid/text fixture'ında indeks açık/kapalı çizim sırası aynı.
- [ ] Font/oblique taşması olan text kırpılmıyor; text bounds ve draw layout aynı hesap sonucundan geliyor.
- [ ] 150k seyrek yerleşimli kontrollü fixture'ın dar viewport'unda candidate < %20; yoğun örtüşme fixture'ında doğruluk korunuyor, sahte azaltma yapılmıyor.

**Commit konusu:** `perf(scene): add conservative bounds and stable BVH culling`

## Aşama 07 — Cache, geometri hazırlığı ve kontrollü ayrıntı

**Amaç:** Pan sırasında değişmeyen geometriyi tekrar üretmemek; performansı ayrıntıları yanlış çizerek kazanmamak.

**Dosyalar:** Yeni `Skia/RenderResourceCache.cs`, `Skia/PreparedGeometryCache.cs`, `Geometry/RenderQualityPolicy.cs`; painter, tessellator, text/raster kaynakları ve performans testleri.

**Uygulama sırası:**

1. Toplam byte bütçesi ve LRU politikasını yukarıdaki sözleşmeyle uygula. Cache anahtarı belge/geometry revision, entity/primitive ID, LOD, font signature ve gerektiğinde style/backend generation içerir. Aynı ReferenceId'nin farklı dosya imzasıyla yeniden kullanılmasına izin verme.
2. Tessellation sonucu double yerel koordinatlar + double origin ile tutulur. Görünüşten bağımsız spline/bulge geometri ve hatch boundary/parametre cache'inin anahtarına pan ekleme. Cache'te mevcut ve toleransı yeterli aynı kaynak tekrar hazırlanmaz. Viewport'a kırpılmış hatch çizgi seti ise **coverage world bounds + LOD + pattern/style revision** taşır; yeni görünür alan coverage dışındaysa aynı sonuç tam kapsam diye kullanılamaz. Aşama 10 güncel kesişim için yeni segment hazırlayabilir; bu iş immutable boundary'yi tekrar tessellate etmek değildir. Sadece dispatch/hasBulge saklamakla ağır tessellation cache'i tamamlanmış sayılmaz.
3. Kaydedilmiş SKPath/vertex için float round-trip hata tahmini güncel WUPP'de **0.1 px** üstündeyse o cache kullanılmaz; double'dan screen'e mevcut güvenli çizim yolu kullanılır veya daha küçük yerel parça hazırlanır. Tek global origin'e güvenme. Çok uzun çizgileri double alanda viewport'a clip et; dash phase gerçek hat uzunluğuyla korunur.
4. Eğri kalitesi: interaction max chord error **1 px**, final **0.25 px**. LOD anahtarı güç-of-two WUPP bandına yuvarlanır; tekrar kullanım ancak kayıtlı hata yeni istenen toleransı sağlıyorsa geçerlidir. Band çevresinde ±%20 hysteresis ile titremeyi önle; toleransı aşan kaba cache zorla tutulmaz.
5. Bir primitive için en fazla iki kullanılabilir LOD sakla. Gereksiz tam-sahne SKPicture, binlerce kontrolsüz path veya sınırsız zoom cache biriktirme. SKPicture seçilen temel çözüm değildir.
6. Text layout ve raster decode cache'lenir. Raster header/dimensions kontrolü decode'dan önce yapılır; görünür boyuta uygun downsample sonucu üretilebilir. Resident kaynak için her frame dosya okuma/decode yoktur. LRU ile çıkarılmış kaynağın yeniden görünmesinde bir kez decode/preparation yapılması meşrudur; bu nedenle bütün belge ömrüne “yalnız bir decode” şartı koyma. Typeface/cache sahipliği açıkça belirlenir; paylaşılan font başkasının çizimi sırasında dispose edilmez. Raster downsample native decode sırasında destekleniyorsa kullan; önce tam çözünürlükte decode edip sonra küçültmek decode belleği koruması değildir. Güvenli decode bütçesine sığmayan kaynak placeholder/diagnostic olur.
7. Pattern hatch'i opak solid'e çevirme. Interaction'da exact boundary + düzenli seyreltilmiş pattern kullan; ekran aralığı 3 px'den küçük çizgi aileleri deterministik stride ile seyreltilir. Finalde exact pattern hedeflenir. İşlem bütçesi aşıldıysa sınır korunur ve ayrıntının hazırlanıyor/kısıtlı olduğu açıkça bildirilir; partial sonuç tam sadakat sayılmaz.
8. Text 3 px altında interaction'da baseline/ince kapsam temsili kullanılabilir; finalde mevcut 0.5 px alt piksel eşiği dışında yazı hazırlanır. Seçili/ölçümle ilgili nesne sadeleştirilmez. Görünür bir bölgenin bütün geometrisi performans için atılmaz.
9. Her painter callback aldığı snapshot'ın viewport'unu sorgular; frame ortasında daha yeni kamerayı ayrıca okumaz. İlk sahne publish edilmeden, desteklenen primitive için sınırları ve düşük maliyetli temel çizim temsili worker'da hazırlanmış olur. Scene hazırsa cache miss'te bu temsil aynı frame'de kullanılır; pahalı tam tessellation/decode paint callback'inde senkron başlatılmaz. Temsil ayrıntı toleransını sağlayamıyorsa durum `PREPARING/LIMITED` olur; final veya tam sadakat diye sayılmaz. Raster henüz hazır değilse bounds placeholder'ı, hatch'te doğru boundary ve mevcut coverage çizilir. Yeni alanın bütün geometrisi boş bırakılamaz. Worker tek aktif hazırlık ve bir latest talep ile sınırlıdır; kamera değiştikçe bütün hazırlığı iptal edip hiç sonuç üretemeyen döngü kurulmaz. Temel sahne temsili parser/normalize model belleği olarak ayrı ölçülür; yüksek ayrıntı cache'ini bu adla bütçe dışına çıkarma.
10. Son parmak kalkınca final-quality frame hemen talep edilir; input engelleyen 150/250 ms bekleme yoktur. Final detay hazırlanırken yeni input gelirse yeni kamera önceliklidir. Bitmiş eski ayrıntı sadece eşleşen geometry/style kimliğine cache sonucu olabilir.
11. Cache hesabı yalnız managed sözlük boyutu değildir. Native SKPath/SKBitmap/text maliyeti ve Skia GPU resource cache kullanımı kaydedilir; aynı byte iki sahipte tekrar sayılmaz. Yerel SkiaSharp 4.151.1 API'sindeki `GRContext.GetResourceCacheUsage` / `SetResourceCacheLimit` yalnız geçerli context'in render thread'inde kullanılır; context bütçesi toplam viewer cache bütçesinin içinden ayrılır. Framebuffer ve hâlen çizilen frame'in bırakılamayan çalışma kaynakları cache diye gösterilmez, ayrı native/GPU çalışma belleği olarak raporlanır. Aktif ve retiring session'ın tutulan cache'leri ortak bütçeyi paylaşır; evict edilemeyen lease varken yeni yüksek ayrıntı cache kabulü bekler, kaynak zorla dispose edilmez. Ek CPU/GPU alt havuzlarına toplam bütçeyi ayrı ayrı tam tahsis etme.

**Geçiş koşulları:**

- [ ] Cache bütçesine sığan, önceden ısıtılmış aynı kapsamda 100 pan frame'inde resident eğri tekrar tessellate edilmiyor; resident raster decode tekrarı 0. Yeni kapsam/LOD ve eviction miss'leri ayrı sayılıyor.
- [ ] Cache açık/kapalı final görüntüler tolerans içinde; stroke/dash/text zoom ile yanlış kalınlaşmıyor.
- [ ] Cache live bytes bütçe içinde; lease bitmeden dispose yok; kapanışta session'a ait cache sahipliği sıfırlanıyor.
- [ ] LOD sınırı çevresindeki pinch'te sürekli kalite gidip gelmesi yok.
- [ ] Interaction temsili arkadaki çizgileri opak hatch ile örtmüyor; final eksikse durum saklanmıyor.
- [ ] Pan sırasında kaynak geometri ve measured dünya koordinatları değişmiyor.
- [ ] İlk defa görünen veya LRU'dan çıkarılmış bölgeye pan, parmak kalkmasını beklemiyor; pahalı cache miss UI/GL paint callback'ini bloke etmiyor. Hatch coverage dışını eski segment setiyle doldurulmuş saymıyor.

**Commit konusu:** `perf(render): cache prepared geometry within quality and memory budgets`

## Aşama 08 — Gerçek dosya açma ve parser köprüsü

**Amaç:** Renderer'da var olan özellikler gerçek dosya açıldığında da aynı veriye ulaşsın; sessiz kayıplar görünür olsun.

**Dosyalar:** `Core/Reading/CadExtractedDocument.cs`, `Cad/AcadSharp/AcadSharpEntityExtractor.cs`, `Rendering/Scene/CadExtractedSceneBuilder.cs`, `App/Opening/CadFileOpenCoordinator.cs`, MainPage dosya açma bağlantısı; Integration.Tests.

**Uygulama sırası:**

1. Mevcut dar enum + optional alan yığınını, type-safe payload'ları olan immutable extracted document modeline genişlet. Parser concrete tipleri Core/App/Rendering'e çıkmaz. Mevcut minimal DTO'yu kullanan testlere gerektiğinde adapter ver; üretimde iki farklı extraction yolu bırakma.
2. Belge seviyesinde gerçek Format, CAD sürümü, unit metadata, model/paper space, block definitions, layers/linetypes/text/dimension styles ve diagnostics taşınır. Extractor'ın Format alanına version yazmasını düzelt.
3. Her entity'de source handle, owner/block/layout kimliği, kaynak sırası, varsa CAD draw-order sırası, visibility, renk yöntemi, TrueColor/ACI, ByLayer/ByBlock, transparency, linetype/scale ve lineweight taşınır. `RenderStyleToken` yazıp renderer'ın okuduğu `CadStyle` alanını boş bırakma.
4. ACI 1–9 dışını tek gri renge indirgeme. Mevcut `CadColor` ve style resolver'ın doğrulanmış tablolarını kullan. Açık/koyu temada ACI7 ve gerçek TrueColor davranışını ayır.
5. 2D'ye indirgemeden önce Z/elevation/normal ve entity koordinat uzayını koru. OCS dönüşümü yalnız ilgili entity tipinde uygulanır; zaten WCS olan line'a tekrar OCS uygulama. Detaylar Aşama 09'daki fixture'larla kilitlenecek. [Autodesk OCS açıklaması](https://help.autodesk.com/cloudhelp/2024/ENU/AutoCAD-DXF/files/GUID-D99F1509-E4E4-47A3-8691-92EA07DC88F5.htm)
6. Extraction, scene build, bounds ve indeks hazırlığını UI dışına taşı. Parser handle'ı worker işi bitene kadar tutan lease al; hızlı B açılışı A'nın handle'ını A extraction bitmeden dispose edemez. Sonuç UI'ya ancak aynı open generation ve session isteği hâlâ geçerliyse atomik bağlanır.
7. Parser'ın desteklemediği cooperative cancellation'ı varmış gibi sunma. İptal UI'ya hemen yansır, geç sonuç kullanılmaz; zorla thread öldürülmez. Aynı anda sınırsız non-cancellable parse başlatma: bir çalışan parse, bir en yeni bekleyen open talebi.
8. Unsupported entity için tip, handle/yerel ID, neden ve varsa güvenilir bounds diagnostic kaydı üret. Güvenilir bounds varsa placeholder; yoksa compatibility sayımı. Reader tarafından görülmeyen ve extractor tarafından atılan nesneler ayrı sayılır. “Yüklendi” durumuna destek özeti eklenir.
9. Mevcut 256 MiB dosya, 250k entity, 32 block derinliği, metin/hatch/raster guard limitlerini **gerçek üretim zincirine** bağla. Entity kotası genişlemiş block instance/primitive maliyetini de sayar. Sessiz kırpma yerine kısmi gösterim ve limit nedeni bildir; bu dosya tam uyumlu kabul edilmez.
10. Style/layout/theme değişimi reparse yapmaz. Orijinal DWG/DXF bytes değiştirilmez; yalnız normalize edilmiş bellek modeli hazırlanır.

**Geçiş koşulları:**

- [ ] Gerçek küçük DWG ve elle hazırlanmış DXF üzerinden Format/version/source order/style değerleri bağımsız beklentilere uyuyor.
- [ ] Aynı handle'ın farklı INSERT instance'ları çakışmıyor; ID `belge + instance yolu + source handle` mantığıyla ayrışıyor.
- [ ] Unsupported ve resource-limit nesneleri sessizce kaybolmuyor.
- [ ] 100 hızlı A→B→C open/cancel dizisinde yalnız son kabul edilen belge yayımlanıyor; kaynak ömrü hatası yok.
- [ ] Extraction/build UI thread'i tutmuyor; parse bitmeden iptal komutu kullanıcıya yansıyor.
- [ ] Gerçek dosya integration testleri yalnız RenderScene'i elle kuran testlerden ayrı yürütülüyor.

**Commit konusu:** `fix(cad): connect lossless document extraction to viewer sessions`

## Aşama 09 — Geometri, koordinat uzayları ve block

**Amaç:** Gerçek dosyadaki çizgiler ve bloklar zoom düzeyinden bağımsız doğru yer, biçim ve stille çizilsin.

**Dosyalar:** Extractor/builder, `Geometry/*`, `Coordinates/OcsTransform.cs`, `Transforms/*`, `Blocks/*`; Geometry ve Integration testleri.

**Uygulama sırası:**

1. Gerçek extraction yoluna LINE, POINT, ARC, CIRCLE, ELLIPSE, SPLINE, LWPOLYLINE, 2D/3D POLYLINE, SOLID, TRACE ve 3DFACE payload'larını bağla. Mevcut renderer/tessellator implementasyonlarını doğrulayarak kullan.
2. Polyline `Closed`, bulge, başlangıç/bitiş genişliği ve elevation kaybolmaz. Son vertex'ten ilk vertex'e bulge da geçerlidir. 3D polyline bulge'sız kendi WCS yolundan gider. Sıfır uzunluklu segment ve tekrarlı vertex kontrollü işlenir.
3. Spline degree, knot vector, weights ve kapalı/periyodik bilgisi korunur; yalnız control point'leri düz çizgiyle birleştirmek spline desteği sayılmaz. Geçerli knot/weight kontrollerini mevcut guard'a bağla.
4. INSERT artık Other ankrajı değildir. Block base point, insertion point, XYZ scale, rotation, normal, row/column array ve instance attributes taşınır. `BlockExpander`/`PrimitiveTransformer` kullanılır; recursion cycle ve expansion maliyeti denetlenir. [Autodesk INSERT alanları](https://help.autodesk.com/cloudhelp/2021/ENU/AutoCAD-DXF/files/GUID-28FA4CFB-9D5E-4880-9F11-36C97578252F.htm)
5. Transform sırası block-local point → base-point subtraction → scale → rotation → OCS/WCS insertion dönüşümü olacak; row/column offset ilgili block yerel eksenlerinde uygulanır. Nonuniform scale altındaki circle/arc ellipse veya toleranslı curve temsiline dönüşür; yalnız radius'u tek scale ile çarpma.
6. Mirroring yön/sweep, layer0 inheritance, ByBlock/ByLayer, görünmez attributes ve nested insert override'ları final resolved style'a ulaşır. Hazır block'ı her frame yeniden expand etme.
7. Mevcut ürün 2D viewer'dır. 3D FACE/çizgiler desteklenen görünüşe açık projeksiyonla çizilir; 3D solid/BRep için yeni 3D motor bu plana eklenmez. Bu içerikler compatibility raporunda dürüstçe sınıflandırılır.
8. Geometri değişiminden sonra Aşama 06 bounds ve Aşama 07 cache anahtarlarını aynı revision ile güncelle. Source order veya instance sırası index traversal'a bırakılmaz.
9. Tessellator'ın mevcut sabit segment sayısını hata toleransı kanıtı sayma. Spline ve dönüşmüş eğrilerde conservative subdivision/error denetimi yap; 4096 gibi segment sınırına ulaşınca tolerans sağlanmadıysa guard/limited diagnostic üret. Hata örneklemesinin yalnız orta noktada eğriyi kaçırmadığını yüksek eğrilik ve weighted spline fixture'ıyla sınırla. Kaynak veriyi değiştiren sabit “0.001 = 1 mm” varsayımı kullanma; sayısal tolerans koordinat ULP'si ve tanımlı geometrik işlemle ilişkilendirilsin, ekran LOD toleransı ayrı kalsın.

**Geçiş koşulları:**

- [ ] Gerçek DXF fixture'ları için kapalı/bulge polyline, döndürülmüş elips, weighted spline ve non-default OCS koordinatları bağımsız sayısal beklentiyle aynı.
- [ ] Nested + mirrored + nonuniform INSERT, array ve attribute sahnelerinde eksik/çift çizim yok.
- [ ] Aynı geometri 0.25/1 px tolerans sözleşmesiyle tüm zoom seviyelerinde tutarlı; culling sınırında kayıp yok.
- [ ] Cycle, aşırı block depth ve expansion kotası kontrollü diagnostic verir; stack overflow yok.
- [ ] Desteklenen entity tipleri gerçek open→extract→scene→painter yolundan sınandı.

**Commit konusu:** `fix(cad): preserve geometry and block transformation semantics`

## Aşama 10 — Metin, ölçülendirme ve hatch

**Amaç:** Uygulama akıcı çalışırken yazı, ölçü ve tarama içeriği yanlış veya eksik gösterilmesin.

**Dosyalar:** `Text/*`, `TextPrimitive.cs`, `Dimensions/*`, `Hatch/*`, extractor/builder ve painter; metin/dimension/hatch Integration testleri.

**Uygulama sırası:**

1. TEXT/MTEXT/ATTRIB/ATTDEF ayrımını extraction'da koru. Text height, width factor, oblique, alignment, attachment, rotation, mirror ve font adı taşınır. MTEXT kontrol dizileri ekrana ham `\P` vb. olarak basılmaz; mevcut MTextParser run/paragraph modeli gerçek metne bağlanır.
2. Text layout sonucu hem Draw hem bounds için ortak nesnedir. Font değişirse layout/bounds revision değişir. Türkçe `İ ı Ş ş Ğ ğ Ç ç Ö ö Ü ü`, Unicode, satır sonu ve eksik glyph senaryolarını sabit içerikle doğrula.
3. FontSubstitutionResolver mevcut izinli font politikasını uygular. Eksik SHX/font varsayılan başka fontla değiştirildiyse diagnostic ve compatibility bilgisi görünür olur. Lisansı belirsiz cihaz/AutoCAD fontlarını bundle etme; yeni font gerekiyorsa repo politikasındaki kanıtıyla eklenir.
4. DIMENSION'da kaynakta geçerli dimension block varsa onu style/instance semantiğiyle göster; aynı anda yeniden builder üretip iki kere çizme. Block yoksa mevcut DimensionBuilder'ı gerçek type/style/override verisiyle kullan. Linear/aligned/angular/radial/diameter ve leader fixture'larıyla doğrula. Ölçü metni override'ını kaynak geometric ölçüyle sessiz değiştirme.
5. HATCH loop'ları, hole/island, bulge/curve boundary, pattern scale/angle/origin korunur. İç delikler ve fill rule doğru; hatch boundary'sini sadece ilk loop'a indirgeme.
6. Pattern çizgilerini tüm dünya boyunca üretme. Viewport ve hatch boundary'nin kesişimine göre, kararlı dünya pattern origin/phase ile üret ve clip et. Böylece pan yaparken pattern kaymaz. Maksimum boundary/iş bütçesi guard'a bağlıdır.
7. Hareket sırasında Aşama 07 kalite politikası uygulanır; final görüntüde desteklenen text/dimension/hatch ayrıntıları tamamlanır. Final üretim bütçesi aşıldıysa “tam kalite” etiketi verilmez.
8. Hatch boundary kapanışını kaynak loop/closed semantiğine göre kur. Son vertex'in ilk vertex ile yinelenmemesi tek başına bozuk boundary değildir. Açıkça kopuk kenarları birim varsayan `ClosureTolerance=1e-3` ile sessiz tamir etme; endpoint sayısal eşdeğerliği ile gerçek açıklığı ayır, gerçek boşlukta diagnostic ve kısmi temsil kullan. Dünya pattern origin/phase ve line index stride'ı coverage değişirken sabit kalır; viewport merkezini pattern origin yapma. Coverage cache'in hazırlanması Aşama 07'deki resident geometri tekrarından ayrı ölçülür.

**Geçiş koşulları:**

- [ ] Türkçe ve MTEXT fixture'ı gerçek parser üzerinden okunur; satır, hizalama, rotation ve text bounds doğru.
- [ ] Eksik font/glyph raporlanır; uygulama çökmez, yanlış “font birebir” iddiası yok.
- [ ] Dimension block ve builder alternatifi aynı nesneyi çift çizmez; override örnekleri korunur.
- [ ] Delikli, eğrisel sınırlı ve sık pattern hatch'te dolgu kaçması yok; pan sırasında pattern phase sabit.
- [ ] Text/hatch hazırlanırken gesture akışı bloklanmıyor; final refine belirlenen performans kapısında tamamlanıyor.

**Commit konusu:** `fix(cad): connect text dimensions and hatch fidelity`

## Aşama 11 — Layout, referanslar ve viewer araçları

**Amaç:** Katmanlar, paftalar, Fit ve ölçüm aynı kamera ve aynı CAD verisi üzerinden çalışsın.

**Dosyalar:** `Layouts/*`, `References/*`, `CadViewerSession.cs`, MainPage araç bağlantıları; yeni `Viewer/MeasurementController.cs`, `Viewer/SnapQuery.cs`; Integration testleri.

**Uygulama sırası:**

1. Model ve paper layout'ları gerçek belge DTO'sundan oluştur. Her layout için son kamera saklanır. İlk girişte Fit, sonraki dönüşte geçerli eski kamera restore edilir; explicit Fit haricinde kendiliğinden view değiştirme.
2. Viewport composition, primitive listesini tek renkle çizmek yerine source entity/instance ID, resolved style, draw order ve viewport frozen layer bilgisini koruyan render commands üretir. View center, view height, paper center/size, twist ve clip uygulanır. Her frame bütün model entity'lerini yeniden transform etme.
3. Paper viewport için ilgili model görünür bölgesini model BVH'dan sorgula; sonra viewport clip ile çiz. Bir layout içindeki iki viewport'ta aynı model entity'nin farklı stil/ölçekle görünmesi cache anahtarlarında ayrıştırılır.
4. XREF/raster çözümü mevcut CadReferenceResolver ve güvenli dosya erişimi üzerinden çalışır. Eksik, döngüsel veya desteklenmeyen referans görünür placeholder/diagnostic üretir. DWG içindeki keyfî path otomatik olarak geniş cihaz erişimi veya internet isteği açmaz. Raster decode Aşama 07 cache'ine gider.
5. Fit aktif layout'un o an render edilebilir içeriğini kapsar. Gizli/frozen katmanları Fit hesabına katma. Bütün katmanlar gizliyse kamera korunur ve “Görünür katman yok” gösterilir. Uzak gerçek entity'yi istatistiksel outlier diye silme; kullanıcı layer/selection ile kapsamı daraltabilir.
6. Layer/theme toggle aynı immutable geometry/index'i kullanır; style revision artar ve frame istenir. Hızlı toggle sırasında eski stil sonucu geri dönmez. Tüm katmanları aç/kapat ile tek katman düğmeleri aynı API'yi kullanır.
7. Ölçüm UI'dan ayrılır; dünya `double` noktaları üzerinden mesafe hesaplanır. DOSYA birim metadata'sı yok veya unitless ise “çizim birimi” yazılır; mm/metre varsayılmaz. Kullanıcı birim seçerse session'a explicit tercih olarak kaydedilir. INSUNITS, kaynak birim bilgisidir; tek başına çizimin doğru ölçekte çizildiğini kanıtlamaz. [Autodesk INSUNITS](https://help.autodesk.com/cloudhelp/2022/ENU/AutoCAD-Core/files/GUID-A58A87BB-482B-4042-A00A-EEF55A2B4FD8.htm)
8. Snap yarıçapı **12 DIP**; adaptörde piksele dönüştürülür ve BVH ile yerel sorgu yapılır. Endpoint, center ve gerçek curve üzerindeki uygun nokta adayları değerlendirilir. Spline control point'i otomatik olarak eğri üzerindeki nokta kabul etme. Invisible/frozen/clipped entity'ye snap yoktur; eşit mesafede tür önceliği endpoint→center→curve, sonra stable ID'dir.
9. Pan/pinch sırasında measurement noktaları dünyada sabit kalır; overlay her frame aynı snapshot kamerasıyla çizilir. Buton/modallar alttaki native yüzeye yanlışlıkla dokunma iletmez. TalkBack açıklamaları ve en az 48 DIP dokunma hedefi gerçek UI üzerinden doğrulanır.

**Geçiş koşulları:**

- [ ] Gerçek paper-space dosyada iki farklı viewport, twist, clipping ve viewport frozen layer doğru.
- [ ] Model↔layout dönüşü kamera ve layer durumunu tutarlı restore ediyor; parser tekrar çağrılmıyor.
- [ ] Eksik XREF/raster placeholder'ı tanımlı; çözüm sonrası ilgili resource revision yenileniyor.
- [ ] Fit boş/tek nokta/ince uzun/uzak entity/gizli katman örneklerinde deterministik.
- [ ] Ölçümün dünya değeri 100 pan/pinch işleminde değişmiyor; birim yokken mm/metre etiketi yok.
- [ ] Snap ekran toleransı density/zoom'dan bağımsız; görünmez geometry'ye yapışma ve spline control-point hatası yok.

**Commit konusu:** `fix(viewer): unify layouts references and measurement tools`

## Aşama 12 — Yaşam döngüsü ve hata kurtarma

**Amaç:** Çizim kapatma, hızlı dosya açma, arka plana geçiş ve bellek baskısı yeni hata üretmesin.

**Dosyalar:** Session/lease, `CadFileOpenCoordinator`, `SafeCadFileCache`, `MainActivity`, `MainApplication`, App lifecycle bağlantıları; lifecycle ve resource testleri.

**Uygulama sırası:**

1. Close tek merkezden pointer/tap, frame callback, preparation token, overlay, scene/index/cache ve parser open lease'ini emekliye ayırır. UI'da kapanış hemen görünür; aktif native/worker işi varsa kaynaklar iş tamamlanıp lease bırakılınca dispose edilir. Kapanmış/retiring session'a sonuç kabul edilmez. Dispose idempotent olur. `CloseRequested` ile `DrainCompleted` ayrı olaylardır; non-cancellable parser işi sürerken hemen sıfır lease iddiası veya zorla handle dispose yoktur. Test, kendi kontrollü işini serbest bırakıp drain sonucunu timeout ile bekler; kullanıcı arayüzü drain bekleyerek bloke olmaz.
2. `OnTrimMemory` içinde koşulsuz `PurgeAll()` çağrısını kaldır. Aktif cache dosyalarının lease kaydı tutulur; sadece kullanımda olmayan sahipsiz geçiciler temizlenir. Trim anında açık dosya silinmemeli. Renderer cache'i önce kullanılmayan ayrıntıları düşürür; kaynak belge/model bu yüzden bozulmaz.
3. Uygulamanın production etkileşim yolunda manuel `GC.Collect()` ile performans düzeltmesi yapma. Dispose ve bütçeyi düzelt. Bellek testi gerektiğinde zorunlu GC ile ayrı teşhis yapabilir; bu sonuç normal runtime başarısı diye kullanılmaz.
4. Background/pause input ve frame clock'u askıya alır; resume yeni surface generation ile gerekli frame'i ister. Orientation, density değişimi ve pencere yeniden bağlanması subscription çoğaltmaz. Gesture yarıda kalmışsa geri dönüşte eski pointer'lar canlanmaz.
5. Android process recreation için yalnız belge erişim kimliği, son layout/camera ve kullanıcı ayarları saklanır; parser handle/GPU kaynakları saklanmaz. SAF sağlayıcı izin veriyorsa kalıcı okuma izni korunur. Erişim kalmamışsa uygulama açılır ve yeniden dosya seçimi ister; boş/crash ekranı oluşmaz.
6. Recent files'da silinmiş/taşınmış dosya, izin kaybı ve bozuk içerik ayrı hata olarak gösterilir. Boş, truncated, yanlış uzantılı ve desteklenmeyen sürümlü dosya negatif fixture testleriyle sınıflandırılır.
7. Yeni dosya başarısız olduğunda önceki çalışan drawing session korunur; kullanıcı explicit Close yapmışsa kapalı kalır. Hata sonrasında kontroller etkileşime açık ve tekrar deneme mümkündür.
8. Parse/open sırasında indeterminate progress gerçek durum adını gösterir; bilinmeyen yüzde uydurulmaz. Cancel, copy/extract destekliyorsa işi iptal eder; non-cancellable parser'da sadece sonucu kullanmama davranışı açıkça korunur.
9. Resource sınırları dosya bytes, genişlemiş entity/vertex sayısı, hatch segmenti, text uzunluğu, raster decoded bytes ve cache bytes için ayrı uygulanır. Decode öncesinde çarpımlar checked/taşma denetimli olur. 8192×8192 bir raster'ın yüz milyonlarca byte tutabileceği hesaplanmadan cache'e kabul edilmez.

**Geçiş koşulları:**

- [ ] 50 close/reopen, 100 rapid open/cancel, 20 rotate ve 20 background/resume döngüsünde crash/ANR/disposed-object hatası yok.
- [ ] Aktif belge açıkken trim-memory bildirimi belgenin cache dosyasını silmiyor; sahipsiz dosyalar temizleniyor.
- [ ] Kapanış ve kontrollü aktif işlerin drain'i sonunda session sahiplik sayacı ve render lease sayacı 0'a iniyor; drain sırasında kaynaklar geçerli, eski callback yeni session'ı etkilemiyor.
- [ ] GPU recreate/fallback, süreç yeniden yaratma ve SAF izin kaybında kullanıcı yeniden çalışabilir duruma dönüyor.
- [ ] Geçerli kaynak fixture SHA-256'sı tüm testlerden sonra değişmemiş.

**Commit konusu:** `fix(lifecycle): make document and surface ownership deterministic`

## Aşama 13 — Gerçek uygulama doğruluğu ve performans kabulü

**Amaç:** Motorun kendi simülasyonunu değil, gerçek Android girişinden görünen görüntüye kadar uygulamayı doğrulamak.

**Dosyalar:** Aşama 05'te kurulan `tests/MobilDwg.Android.Instrumentation/` genişlemesi; yeni `tests/MobilDwg.Rendering.Tests/ViewerPerformanceTests.cs`, Integration.Tests fixture manifesti; `scripts/viewer-stability-gate.ps1` genişlemesi ve mevcut Android test dokümanı.

### Test yolu

1. Aşama 05'teki ayrı test-only .NET Android instrumentation runner'ını tam corpus ve lifecycle dizilerine genişlet; ikinci runner oluşturma. `Android.App.Instrumentation` ve `UiAutomation.InjectInputEvent` ile DOWN/POINTER_DOWN/MOVE/POINTER_UP/UP/CANCEL paketlerini gerçek native input yoluna gönder. Touchscreen source, pointer ID ve action index doğru olsun. EventTime/DownTime Android uptime tabanında, örnekler gerçek zamanlı olsun; bir saniyelik hareketi beklemeden tek burst ile gönderip akıcılık ölçme. Test ekran koordinatlarını gerçek viewer'ın ekran konumu/insets'inden türet, native view-local adapter koordinatlarıyla karıştırma. Üretime instrumentation bileşeni ekleme. [Android UiAutomation](https://developer.android.com/reference/android/app/UiAutomation)
2. `adb shell input swipe` yalnız tek parmak kanıtıdır. İki pointer'ı doğrudan ViewportInteractionEngine'e vermek host/integration yardımcı testidir; native pinch testinin yerine geçmez.
3. Gesture fixture'larında ilk viewport dışında dört yönde sentinel geometry ve merkez/köşelerde işaretli hedefler olsun. Kamera beklenen sentinel alanını gösterirken pointer hâlâ aşağıda tutulur; aynı anda frame/PixelCopy veya instrumentation screenshot alınır. UP sonrası tek görüntü yeterli değildir.
4. Test-only telemetry ile Input→Camera→Request→Paint zamanlarını Aşama 01 clock sözleşmesiyle bağla. Gerçek görünür sunum için Android FrameTimeline/Perfetto ve screenshot/video kullan. Yalnız SKCanvas PaintMs veya Surface.Flush süresine “ekrana ulaşma süresi” deme. GL içeriğinde Android vitals/gfxinfo'nun hangi kısmı kapsadığını doğrula. Her gesture/sentinel bir input sequence ID, camera revision ve sunum kanıtıyla eşleşir. API/cihaz gerekli sunum izini sağlamıyorsa metrik ölçülmedi kalır; PaintEnd ile doldurulmaz. [Android render ölçümü](https://developer.android.com/topic/performance/vitals/render)
5. Görsel oracle, test edilen renderer'ın aynı çıktısı olamaz. Küçük DXF'lerde elle bilinen koordinat/semantik; gerçek DWG'de izinli bağımsız referans görüntüsü veya güvenilir dış çıktı kullanılır. Writer+aynı-reader round-trip yalnız smoke'tur. Özel CAD dosyaları ve türev görüntüleri public repoya eklenmez.
6. Piksel karşılaştırması bütün ekrana yayılan ortalama benzerlikle sınırlanmaz. Sentinel bölgeleri, text bölgeleri, entity completeness ve kenar geometrisi ayrı ölçülür. Aynı backend/pinned font için kesin beklenen örnekler; backendler arasında AA farkı için en fazla 1 px kenar bandı toleransı kullanılır. Eksik çizgi bu toleransla maskelenmez.

### Zorunlu corpus

| Fixture grubu | İçerik ve amaç |
|---|---|
| Mikro | Elle bilinen çizgi, yay, kapalı/bulge polyline; dört kenar sentinel; köşe pinch; text bounds |
| CAD semantik | Nested/nonuniform/mirrored block, OCS, weighted spline, MTEXT, dimension, delikli hatch, iki paper viewport, eksik referans |
| Seyrek yük | 10k, 50k, 150k, 250k entity; sabit seed; geniş dünyada %5–15 görünür bölüm |
| Yoğun yük | Aynı sayılarda üst üste binme, uzun polyline, çok metin ve hatch; Fit görünüşünde pahalı bütün alan |
| Büyük koordinat | 5e6 ve guard sınırına yakın koordinatlar; küçük yerel detay; farklı birimler |
| Gerçek özel dosya | Sorunu gösteren kullanıcı DWG/DXF'leri yerelde; her dosya için gerçek özellik ve darboğaz dökümü |
| Negatif | Truncated/boş/yanlış sürüm, cycle, aşırı vertex, hatch ve raster, kayıp dosya/izin |

Entity sayısı tek başına yük değildir. Her fixture toplam/visible primitive, vertex, glyph, hatch line, block expansion ve raster decoded byte miktarını da kaydeder. 150k basit çizgiyi hızlı çizmek 150k ağır spline/text desteğini kanıtlamaz.

### Sayısal kabul

Fiziksel referans cihaz: testte mevcut olan en düşük kapasiteli desteklenen Android cihaz, model/SoC/RAM/GPU/OS/display Hz ile manifestte sabitlenir. İlk hedef 60 Hz'dir. Fiziksel cihaz yoksa bu satırlar **ÖLÇÜLMEDİ** kalır; Gemini eşiği emülatöre göre değiştiremez.

| Ölçüm | Kabul koşulu |
|---|---|
| Kamera/odak | Aşama 03 drift sınırları; tüm geçerli input'ta NaN/Infinity 0 |
| Yeni bölge | Desteklenen fixture'da ekrana giren sentinel'ın gecikmesi input→sunum bütçesi içinde; UP bekleme yok |
| Küçük/orta ve 150k seyrek dar görünüm | 60 Hz cihazda sürekli dirty hareket boyunca sunum slot farkı p95 ≤1 vsync, p99 ≤2 vsync; yaklaşık 16.7/33.3 ms |
| Yoğun büyük Fit görünümü | Kontrollü interaction LOD ile p95 ≤2 vsync; 60 Hz'de yaklaşık 33.3 ms, bu sınıf 60 FPS diye etiketlenmez |
| Input→görünür kamera sonucu | p95 ≤50 ms, p99 ≤100 ms; ölçüm OS sunumuyla ilişkilendirilmiş |
| Donma | Isınma sonrası viewer kaynaklı >100 ms frame/input duraklaması 0; crash/ANR 0 |
| Final ayrıntıya geçiş | Son hareketten sonra küçük/orta ≤200 ms, büyük desteklenen fixture ≤500 ms; yeni gesture gelirse kamera öncelikli |
| Queue | Aktif paint ≤1, bekleyen invalidation ≤1; hazırlık işi ≤1 + latest bekleyen talep |
| Sıcak pan | Isıtılmış, bütçeye sığan kapsamda geçerli resident geometry için tessellation/decode tekrarı 0; yeni coverage/LOD/eviction miss ayrı; cache bütçesi ihlali 0 |
| Idle | Sakinleşme sonrası 10 saniyede uygulama kaynaklı request 0 |
| Kaynak ömrü | Close sonrası ilgili aktif işlerin drain'i tamamlandığında owner/lease sayacı 0; disposed resource kullanımı 0 |

Her performans koşusu **10 saniye ısınma + 60 saniye kayıt**, üç tekrar yapılır. Tekrarlarda median yanında en kötü p95/p99 da raporlanır; sadece en iyi koşu seçilmez. Fixed 60/120 Hz, density ve yavaş input delivery testleri ayrılır. Emülatörün 6 GB RAM/8 core ayarı telefon performansının kanıtı değildir.

Sunum slot ölçümü sabit display modunda OS sunum takviminden çıkarılır; 33.333 ms'nin 33.3'e yuvarlanması bir başarısızlık sebebi değildir. Frame aralığı kapısı, en az ekran yenileme hızı kadar teslim edilen ve kamerayı sürekli değiştiren input koşusuna aittir. 15/30 Hz input, sabit parmak veya zoom sınırında değişmeyen kamera için sahte 60 FPS üretmek amacıyla sürekli çizim açılmaz; bu koşulda odak doğruluğu, gelen input'un gecikmesi ve dirty/idle doğruluğu ölçülür. Küçük/orta sınıfı 10k/50k fixture'larıdır; gerçek dosyalar yukarıdaki primitive/vertex/glyph maliyetiyle ayrıca sınıflandırılır. Cache kapasitesini aşan gezinme ve ilk görülen alan koşusu, sıcak resident koşusundan ayrı raporlanır; ikisinde de sentinel-before-UP şartı geçerlidir.

Bellek için managed heap, native/GPU tahmini ve Android PSS ayrı toplanır. 10 ısınma close/reopen döngüsünden sonra 30 döngü daha ölçülür. Kapalı durum örneği kontrollü işlerin `DrainCompleted` olayından sonra alınır; devam eden parser'ın belleği sızıntı diye sayılmaz veya tablodan gizlenmez, retiring bytes ayrıca raporlanır. Son 5 kapalı durumun medyan PSS'si ilk 5 kapalı duruma göre `max(16 MiB, %5)` üstünde kalıyorsa inceleme/başarısızlık açılır; bunun yanında artan live owner/cache sayısı doğrudan fail'dir. Üretimde zorla GC çalıştırıp sızıntıyı saklama. En az **30 dakika** pan/pinch/layout/open/close soak koşusu uygulanır.

**Geçiş koşulları:**

- [ ] API36 emülatörde gerçek tek ve çoklu native dokunma, sentinel-before-UP ve lifecycle testleri başarılı.
- [ ] Desteklenen minimum API24 yolu ve API36 üzerinde yüzey/open smoke doğrulanmış; mevcut emülatör değiştirilmeden ek test ortamı kullanılmış.
- [ ] Seyrek ve yoğun corpus ayrı raporlanmış; sayısal eşikler sağlanmış veya ilgili cihaz/fixture için açık başarısızlık kaydı var.
- [ ] Her destek iddiasının gerçek dosya entegrasyon kanıtı var; renderer-only fixture başarıları ayrı.
- [ ] Görsel oracle bağımsız; beklenen resmi yeni renderer çıktısıyla sessiz değiştirme yok.
- [ ] Fiziksel cihaz yoksa kod/emu doğrulaması kaydedilmiş, fiziksel kabul kapısı açık bırakılmış; tam ürün kabulü verilmemiş.

**Commit konusu:** `test(android): verify real touch fidelity and frame budgets`

## Aşama 14 — CI ve sürüm kanıtı

**Amaç:** Bir sonraki değişiklikte aynı sorunları yeniden bulmayı kullanıcıya bırakmamak.

**Dosyalar:** Yeni `.github/workflows/viewer-stability.yml`; gate script'i; `docs/ARCHITECTURE.md`, `docs/ANDROID_TESTING.md`, `docs/GOLDEN_CONTRACT.md`, `docs/release/COMPATIBILITY_AND_LIMITATIONS.md`; yeni tek `docs/VIEWER_STABILITY_CONTRACT.md` ve run bazlı evidence.

**Uygulama sırası:**

1. Her PR için Core/Rendering/Architecture/Integration harness, locked restore, dependency audit ve Android build koşar. Test komutunun gerçek harness çağrısını ve nonzero exit code yayılımını workflow denetler.
2. Input/surface/painter/scene değişikliklerinde API36 native gesture smoke zorunludur. Daha uzun corpus/soak testleri release öncesi çalışır. Fiziksel cihaz sonucu otomasyon yokken manuel kanıtla ilişkilendirilir; yoksa pending kalır.
3. Planın kalıcı invariants'ını tek `VIEWER_STABILITY_CONTRACT.md` içine taşı. Testlerin komutları mevcut ANDROID_TESTING dokümanına eklenir. Silinmiş eski BASLA/DEVAM dosyaları veya tarihsel stage cursor'ları yeniden oluşturulmaz.
4. Release/final auditor, notices ve native dependency inventory son resolved graph ile eşleşir. Mevcut sign/version/release politikası korunur. Kullanıcıya ait uncommitted SHA256SUMS değişikliği eski APK'yı yeniymiş gibi doğrulamak için kullanılmaz.
5. Dağıtılacak normal Release APK ayrıca hash'lenir, kurulur ve gerçek open/pan/pinch/close smoke testinden geçer. Instrumentation veya validation bayraklı başka APK'nın geçmesi final APK'nın geçtiği anlamına gelmez. Kaynak HEAD + dirty source manifest + lock hash + APK hash + fixture hash + cihaz bilgisi birlikte kaydedilir.
6. Düzeltme başarısı sadece commit mesajından veya log marker'dan çıkarılmaz. Sayısal ölçüm dosyaları, native input akışı ve görsel kanıt mevcut olur. Her marker ölçülmüş assertion sonucunda yazılır.
7. Rapor desteklenen özellikleri, kısmi/unsupported durumları ve fiziksel cihaz kabulünü açıkça listeler. Parser desteklemediği 3D/proxy içeriği sonradan “tam DWG uyumu” başlığı altında gizleme.
8. Bu aşama yayınlamaya hazır, incelenebilir değişiklik kümesi üretir. Push/merge/tag/store yayını yalnız o işlem kullanıcı tarafından ayrıca istendiğinde yapılır; plan hazırlama veya yerel doğrulama tek başına yayınlama talimatı değildir.
9. Yerel dirty başlangıç dosyaları final davranışın önkoşuluysa yalnız Gemini commit'lerini temiz HEAD'e uygulayan CI aynı kaynağı test etmez. Başlangıçta zaten var olan **gerekli** kullanıcı değişiklikleri manifest/diff ile açıkça ilişkilendirilir ve kullanıcı bunların commit'e alınmasını yetkilendirdiyse kaynak geçmişine ayrı, dürüst kayıtla dahil edilir. Yetki yoksa silme/topluca commit yok; temiz checkout yeniden üretilebilirliği açık kalır, “CI doğrulandı/yayına hazır” denmez. Gerekli kaynaklar geçmişte olduğunda aynı commit'in ayrı temiz checkout'unda build ve smoke doğrulanır. Özel CAD/keystore snapshot veya commit'e alınmaz; ilk çalışma ağacı değişmeden kalır.

**Son kabul koşulları:**

- [ ] Aşama 01–12 teknik kapıları ve Aşama 13 doğruluk kapıları tamam.
- [ ] Fiziksel hedef cihaz performansı ölçülmüş ve geçmiş; yoksa final durum yalnız **KOD VE EMÜLATÖR DOĞRULANDI — FİZİKSEL KABUL BEKLİYOR**.
- [ ] Final normal Release APK test edilen kaynak ve lock graph'ıyla eşleşiyor.
- [ ] Aynı commit'in temiz checkout'u gerekli kaynak değişikliklerini içeriyor ve doğrulanmış; dirty yerel dosyalara gizli bağımlılık yok.
- [ ] Regression testleri CI'da gerçekten çalışıyor; hatalı fixture sıfır testle PASS veremiyor.
- [ ] Kaynak CAD dosyaları değişmemiş ve özel içerik public artifact'e girmemiş.
- [ ] Güncel uygulama kısıtları ve başarısız fixture'lar dürüstçe kayıtlı.

**Commit konusu:** `ci(viewer): enforce reproducible stability release gates`

## Uygulama ve devam kuralları

1. Güncel kullanıcı talebi, Aşama 01'den başlayıp 14'e kadar sırayla ilerlemektir. Her aşamayı ilgili test ve kanıtıyla tamamla, kısa rapor ver, ardından tekrar rutin onay istemeden sonraki aşamaya geç. Kullanıcı daha sonra belirli bir aşamayla sınırlandırırsa yeni kapsamı uygula. Başarısız teknik önkoşulu atlama. Fiziksel cihaz/izin gibi dış doğrulama eksikliğinde ilgili kapı açık kalır; bu eksikliğe bağımlı olmayan sonraki hazırlık ve CI işlerini yapabilirsin, ancak aşama veya ürün için tam kabul veremezsin.
2. Bir aşamadaki başarısız test, eşik büyütme, kapıyı yorum satırına alma veya testi silmeyle giderilemez. Implementasyonu düzelt. Beklentinin gerçekten yanlış olduğu bağımsız kaynakla kanıtlanırsa gerekçeyi test değişikliğiyle birlikte kaydet.
3. Alt aşamalar 14 aşamayı atlatan paralel mimari dallar değildir. Eski planlardan ikinci input/scheduler/cache yaklaşımı karıştırma.
4. Sürüm, sınıf, paket API'si veya platform bilgisi plan uygulanırken değişmişse gerçek dosyayı doğrula. Olmayan API'yi hayal ederek derlenmiş gibi raporlama. Mevcut pinle desteklenmeyen somut durum için kanıtı ve bu plandaki CPU/diagnostic kurtarma yolunu kullan; sessiz dependency upgrade yapma.
5. Yalnız bu işin dosyalarını commit'e al. Kullanıcının ilgisiz çalışma ağacı değişikliklerini silme veya `git add .` ile toplama. Her aşama sonunda değişiklik ve kanıtı ayrı okunabilir commit'te tut; başlangıç kullanıcı değişiklikleriyle yeni çalışma ayrımı manifestte görünür olsun.
6. Her aşama sonunda aşağıdaki raporu doldur. Sadece “PASS” yazmak yeterli değildir. `artifacts/viewer-stability/PROGRESS.md` mevcut ilerleme kaydı olsun; run bazlı kanıtları aynı kökte ayrı klasörlere yaz, önceki koşuyu üzerine yazma. Bağlam/oturum kesilirse bu kayıt + gerçek kod + kanıtı birlikte doğrulayarak devam et. Bu yerel kayıt tarihsel BASLA/DEVAM altyapısının geri getirilmesi değildir; kalıcı invariants tek `VIEWER_STABILITY_CONTRACT.md` dosyasındadır.

```markdown
Aşama: NN — ad
Durum: TAMAMLANDI / BAŞARISIZ / DIŞ DOĞRULAMA BEKLİYOR
Başlangıç kaynak manifesti ve HEAD:
Son HEAD / henüz commit edilmediyse diff hash:
Değişen dosyalar:
Kullanıcıya yansıyan davranış:
Çalıştırılan gerçek komutlar ve exit code:
Fixture / APK / lock hash'leri:
Ölçülen metrikler ve kanıt dosyaları:
Geçmeyen veya çalıştırılamayan koşullar:
Bir sonraki aşama:
```

**Gemini'ye ilk uygulama mesajı:**

> `docs/MOBIL_DWG_NIHAI_UYGULAMA_PLANI.md` dosyasının 6 Eylül bütünlük denetimli sürümü nihai plandır. Önce tamamını oku, ardından Aşama 01'den başlayıp Aşama 14'e kadar sırayla uygula; rutin geçiş onayı bekleme. Güncel yerel çalışma ağacını ve kullanıcı değişikliklerini koru. Eski dört planı yürütme talimatı olarak kullanma. Her aşamanın gerçek test ve kanıtını tamamla; başarısız teknik önkoşulu atlama, test gevşetme ve başarı uydurma. Dış doğrulama eksikse kaydet ve bağımsız işleri sürdür; tam kabul verme. İlerlemeyi `artifacts/viewer-stability/PROGRESS.md` içine kaydet. Ayrıntılı başlangıç mesajı `docs/GEMINI_UYGULAMA_BASLATMA_PROMPTU.md` dosyasındadır.

## Araştırma kaynakları ve alınan kararlar

Ana araştırma 5 Eylül 2026'da yapıldı. 6 Eylül bütünlük denetiminde güncel yerel architecture/source sınırları, Android DOWN tüketimi, saat tabanları, instrumentation ve Skia GL handler yeniden kontrol edildi. GitHub kodu davranış ve mimari araştırmasıdır; yabancı motor ayarları doğrudan taşınmadı. Projenin mevcut lisans politikasına göre uygunsuz kaynaktan satır satır port yapılmayacak. Bu bir yeni ticari SDK edinme veya lisans değişimi planı değildir.

| Birincil kaynak | Doğrulanan nokta | Bu plandaki kullanım |
|---|---|---|
| [MAUI pinch sözleşmesi](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/pinch?view=net-maui-10.0) | Scale son güncellemeye göre göreli | Mevcut bug'ın teşhisi; son çözüm native packet hesabı |
| [Android drag ve scale](https://developer.android.com/develop/ui/views/touch-and-input/gestures/scale) | Pointer ID/index, çoklu dokunma geçişleri | Native adaptör ve Cancel/topoloji testleri |
| [Android MotionEvent](https://developer.android.com/reference/android/view/MotionEvent) | Event time, history, pointer packets | İşlenme zamanı yerine olay zamanı; tam pointer kümesi |
| [Android gesture detector](https://developer.android.com/develop/ui/views/touch-and-input/gestures/detector) ve [OnTouchListener](https://developer.android.com/reference/android/view/View.OnTouchListener) | DOWN tüketimi sonraki gesture olaylarının teslimini etkiler | Slop kararı ile input sahipliğini ayırma |
| [Android SystemClock](https://developer.android.com/reference/android/os/SystemClock) | Uptime ve elapsed time farklı clock sözleşmeleridir | Gesture/paint ölçümlerinde ortak clock; trace kalibrasyonu |
| [Android Instrumentation](https://developer.android.com/reference/android/app/Instrumentation) | Test runner ve UiAutomation erişimi | Ayrı test APK'sı, gerçek zamanlı native smoke'un Aşama 05'te kurulması |
| [Android Choreographer](https://developer.android.com/reference/android/view/Choreographer) | Ekran frame fırsatına bağlı callback | Dirty durumda tek invalidation |
| [Android UiAutomation](https://developer.android.com/reference/android/app/UiAutomation) | Input event enjeksiyonu | Gerçek native pinch testi |
| [Android slow rendering](https://developer.android.com/topic/performance/vitals/render) | Frame ölçüm araçları ve kapsamları | PaintMs ile sunum/FPS'yi ayırma |
| [Skia MAUI paketi 4.151.1](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls/4.151.1) | Kullanılacak UI paketinin varlığı | Mevcut Skia sürüm ailesini koruma |
| [Skia Android GL handler](https://github.com/mono/SkiaSharp/blob/2d57ce7046722c6864d28eb449ca30690a38c583/source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs) | WhenDirty, native koordinat/IgnorePixelScaling, GL callback | Surface/pixel/thread sözleşmesi; bu source SHA'nın NuGet artifact'iyle eşleşmesi restore denetiminde ayrıca kaydedilir |
| [Skia Android touch handler](https://github.com/mono/SkiaSharp/blob/2d57ce7046722c6864d28eb449ca30690a38c583/source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs) | Tek MotionEvent'i pointer başına ayrı callback'e çeviriyor; Cancel tek callback olabilir | Tek paket native adaptör tercihi; aynı input'u iki kanaldan işleme yok |
| [Mapsui ManipulationTracker](https://github.com/Mapsui/Mapsui/blob/7af71dbf2730322416b46b8fb58524a57f5356be/Mapsui/Manipulations/ManipulationTracker.cs) | Önceki/yeni centroid, distance ratio, parmak sayısı değişiminde reset | Aynı ilke; ayrıca ID değişimini de izleyen özgün engine |
| [Mapsui MAUI MapControl](https://github.com/Mapsui/Mapsui/blob/7af71dbf2730322416b46b8fb58524a57f5356be/Mapsui.UI.Maui/MapControl.cs) | SKGLView/SKCanvasView ve on-demand invalidation | Yüzey mimarisinin pratik referansı; hareketsiz pointer'a 500 ms stale timeout alınmadı |
| [dxf-viewer DxfScene](https://github.com/vagran/dxf-viewer/blob/c2578b979e2b87e740d44e644b153b89cc59d707/src/DxfScene.js) | Scene origin ve çizim batch hazırlığı | Hazırlanmış geometri, yerel origin; CAD order değiştiren global batch alınmadı |
| [dxf-viewer DxfViewer](https://github.com/vagran/dxf-viewer/blob/c2578b979e2b87e740d44e644b153b89cc59d707/src/DxfViewer.js) | Tek parmak pan, iki parmak dolly-pan, change'de Render | Hareket sürerken çizim; `zoomSpeed=3` ve mouse sabitleri alınmadı |
| [LibreCAD viewport](https://github.com/LibreCAD/LibreCAD/blob/c78561d7a17f5cd5ba829bf01fb92995fe45cdfb/librecad/src/lib/gui/lc_graphicviewport.cpp) | Viewport değişim bildirimi, Fit ve zoom işlevleri | Davranış incelemesi; integer offset ve kaynak kod portu alınmadı |
| [ACadSharp](https://github.com/DomCR/ACadSharp) ve yerel 3.7.1 XML API | BlockRecords, Layouts, Insert alanları ve entity stilleri parser'da mevcut | Parser değiştirmek yerine yerel extractor köprüsünü tamamlama |
| [Autodesk OCS](https://help.autodesk.com/cloudhelp/2024/ENU/AutoCAD-DXF/files/GUID-D99F1509-E4E4-47A3-8691-92EA07DC88F5.htm), [INSERT](https://help.autodesk.com/cloudhelp/2021/ENU/AutoCAD-DXF/files/GUID-28FA4CFB-9D5E-4880-9F11-36C97578252F.htm), [INSUNITS](https://help.autodesk.com/cloudhelp/2022/ENU/AutoCAD-Core/files/GUID-A58A87BB-482B-4042-A00A-EEF55A2B4FD8.htm) | Kaynak format semantiği | Koordinat, blok ve birim testleri; ticari runtime SDK kullanılmaz |

## Hazırlık kanıtları

Bu bölüm uygulama aşamalarının yapıldığı iddiası değildir. Plan hazırlanırken üretilen yerel kanıtlar:

```text
artifacts/plan-review-2026-09-05/
  BAGIMSIZ_PLAN_V1.md
  candidate-manifest.json
  candidate-1.txt ... candidate-4.txt
  core-baseline.log
  rendering-baseline.log
  architecture-baseline.log
  emulator-before.png
  ENGINE_OPTIMIZATION_SPEC.pdf
  smitelagwar-mobil-dwg-plan.pdf
```

`artifacts/` Git dışında kalır. İçinde özel çizim adı/görüntüsü bulunabilecek emülatör ekranı public repo veya public CI artifact'ine taşınmayacaktır.

Uygulamanın her olası DWG ve her cihazda hiç hata vermeyeceği garanti edilemez. Bu planın bitiş tanımı; tanımlı destek kapsamında native dokunma, gerçek dosya doğruluğu, görünür frame gecikmesi, kaynak ömrü ve final APK'nın **aynı ölçülebilir kapılardan sürekli geçmesidir**. Hedef, yeni kusurları kullanıcının tekrar tekrar keşfetmesine bırakmamaktır.
