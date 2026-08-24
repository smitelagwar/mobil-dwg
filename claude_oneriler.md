# CLAUDE OPUS ÖNERİLERİ V2.0 — GENİŞLETİLMİŞ DERİN ANALİZ
## Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan.md İçin Kapsamlı Eleştiri ve İyileştirme Önerileri

**Tarih:** 24 Ağustos 2026  
**Doküman Tipi:** Bağımsız Teknik Denetim Raporu — Genişletilmiş V2.0  
**Hedef:** Planın zayıf/eksik noktalarını, gizli riskleri ve pratik iyileştirme fırsatlarını tespit etmek  
**Yaklaşım:** Plandaki güçlü yanları onaylamak yerine, öncelikle planın **görmezden geldiği veya hafife aldığı tehlikelere** odaklanmak  
**Kapsam:** Planın tüm 93 bölümü satır satır tarandı, Gemini önerileri ile çakışma kontrol edildi

---

# 0. GENEL DEĞERLENDİRME

Plan, konsept düzeyinde **çok sağlam** bir belge. Özellikle şu noktalar takdire değer:

- Lisans felsefesi tutarlı ve disiplinli
- "Preview-first, editor later" stratejisi doğru
- ACadSharp + SkiaSharp + MAUI hattı mantıklı seçim
- ProCad'e kör bağlanmama kararı olgun bir mühendislik yaklaşımı
- Android gerçek cihaz test önceliği isabetli
- Fallback planı (ProCad başarısız olursa kendi scene layer) var
- Aşama aşama ilerlemeli geliştirme yol haritası mantıklı
- Lisans firewall mekanizması (Bölüm 52) çok değerli
- Error taxonomy (Bölüm 47) iyi tasarlanmış

Ancak plan bazı **kritik konularda ya sessiz kalıyor ya da fazla iyimser**. Bu rapor 30 başlıkta bu boşlukları kapatmayı amaçlıyor.

---

# 1. KRİTİK — ACadSharp'ın ALPHA DURUMU VE "SİLENT ENTITY DROP" RİSKİ

## 1.1 Planın eksik bıraktığı gerçek

Plan, ACadSharp'ı Bölüm 6'da MIT lisanslı güvenilir bir DWG parser olarak tanıtıyor ve Bölüm 8'de "parser'dır, tek başına viewer değildir" uyarısı yapıyor. Ancak **ACadSharp'ın kendi yazarı tarafından resmi olarak "alpha/development stage" olarak sınıflandırıldığı** bilgisi planda yer almıyor.

Bu bilgi bir küçük ayrıntı değil — projenin en temel bileşeninin güvenilirlik profilidir.

Ağustos 2026 itibarıyla bilinen spesifik sorunlar:

| Sorun Kategorisi | Spesifik Durum | Etki Seviyesi |
|---|---|---|
| **Silent entity drop** | TEXT entity'de height=0 olduğunda sessizce atlanıyor | Yüksek — kullanıcı göremez |
| **Silent entity drop** | Karmaşık ATTRIB/ATTDEF entity'leri bazen kayıp | Yüksek — block attribute'lar görünmez |
| **MTEXT ek metin kaybı** | DXF kodu 3 olan ek metin blokları (uzun MTEXT) bazen kayıp | Orta — uzun açıklamalar kesilir |
| **Dimension iç yapısı** | İç içe entity'leri olan DIMENSION'lar tam parse edilemiyor | Yüksek — ölçüler hatalı/eksik |
| **Proxy/custom entity** | Civil 3D, Architecture toolset nesneleri sessizce yok sayılıyor | Orta — hedef kitle mimari/statik olduğu için |
| **Versiyon hassasiyeti** | Bazı DWG versiyonları arasında beklenmedik davranış farkları | Orta — geniş test corpus ile azaltılabilir |
| **Dynamic block** | Dynamic block property'leri tam desteklenmiyor | Düşük — MVP'de kısmi destek yeterli |

## 1.2 Plana eklenmesi gereken uyarı ve savunma mekanizmaları

### Bölüm 6'ya eklenecek uyarı:

```
> [!WARNING]  
> ACadSharp, Ağustos 2026 itibarıyla yazarı tarafından "alpha/development stage" 
> olarak nitelendirilen bir projedir. Bu durum projeyi durdurmak için bir neden 
> DEĞİLDİR — alternatifler (netDXF yalnız DXF destekler, LibreDWG GPL'dir) 
> daha kötüdür. Ancak aşağıdaki savunma mekanizmaları ZORUNLUDUR.
```

### Zorunlu Savunma Mekanizması #1 — Notification Logging:

```csharp
// ACadSharp reader'ında onNotification callback'i MUTLAKA kullanılmalı
// Bu callback, parser'ın sessizce atladığı entity'leri yakalayan TEK mekanizmadır
CadDocument doc = DwgReader.Read(filePath, (sender, e) => 
{
    logger.Warn($"ACadSharp Notification: {e.Message}");
    compatibilityReport.AddWarning(e.Message);
});
```

Bu callback olmadan uygulama entity atlasa bile bundan haberdar olamaz. Bu nedenle bu tek satır, uygulamanın güvenilirliği için **en kritik savunma hattıdır**.

### Zorunlu Savunma Mekanizması #2 — Entity Doğrulama Katmanı:

```
Dosya açıldıktan sonra otomatik kontrol sırası:

Adım 1: ACadSharp'ın okuduğu toplam entity sayısını al
Adım 2: ModelSpace + tüm Layout'lardaki entity sayısını topla
Adım 3: Notification callback'te toplanan uyarı sayısını kontrol et
Adım 4: Şu koşullardan biri gerçekleşirse kullanıcıya uyarı:
   - 5+ notification alındıysa
   - Bilinen proxy entity tipi algılandıysa
   - Parse sırasında exception yakalandıysa (catch edilen)

Uyarı metni:
"Çizim açıldı. Bazı özel CAD nesneleri bu sürümde 
tam görüntülenememiş olabilir. (Detaylar için ⓘ simgesine dokunun)"

Detay ekranında:
- Atlanan entity tipleri listesi
- Eksik font listesi
- Eksik XREF listesi
- Parser notification'ları
```

### Zorunlu Savunma Mekanizması #3 — ACadSharp Versiyon İzleme:

```
Proje boyunca:
- ACadSharp'ın GitHub release'leri takip edilmeli
- Her yeni versiyon çıktığında:
  1. CHANGELOG okunur
  2. Test corpus'u yeni versiyonla çalıştırılır
  3. Entity count regresyonu kontrol edilir
  4. Lisans değişikliği kontrol edilir (MIT kaldı mı?)
  5. Sorun yoksa versiyon güncellenir

Bu izleme manuel olabilir ama düzenli yapılmalı.
ACadSharp'ın yazarını GitHub Sponsors'da desteklemek 
de projenin devamlılığı için stratejik bir yatırımdır.
```

---

# 2. KRİTİK — ProCad'İN "VAR SAYILAN" OLGUNLUĞU VS GERÇEKLİK

## 2.1 Planın eksikliği

Plan ProCad'i çok değerli bir keşif olarak sunuyor (Bölüm 9, 85). Bu doğru. Ancak plan, ProCad'in **gerçek mobil cihazda (özellikle Android)** ne kadar test edildiğine dair **hiçbir somut kanıt** sunmuyor.

ProCad'in repo yapısına bakıldığında:
- **Ana UI shell'i Avalonia tabanlı** — masaüstü öncelikli bir proje
- MAUI kontrolü paket olarak mevcut (`ProCadSharp.Controls.Maui`)
- .NET 10 SDK gereksinimi — MAUI 10'un kendisi de henüz olgunlaşma sürecinde
- **Gerçek Android cihazda kapsamlı test kanıtı belirsiz**
- Repo'nun ana geliştiricisi Wiesław Šoltés, Avalonia ekosistemi uzmanı — MAUI Android deneyimi belirsiz

## 2.2 Plana eklenmeli — ProCad Spike için KESİN ölçüm kriterleri

Bölüm 70 (Aşama 1) çok genel kalıyor. Şu somut kriterler eklenmeli:

```
### ProCad Android Spike — PASS/FAIL Kriterleri (Somut)

| # | Test Adı | PASS Kriteri | FAIL Kriteri | Blocker mi? |
|---|---|---|---|---|
| 1 | ProCad.Controls.Maui Android debug build | Derleme başarılı, 0 hata | Derleme hatası | ✅ Evet |
| 2 | Gerçek telefonda ilk açılış | Crash yok, beyaz ekran yok, ANR yok | Crash veya ANR | ✅ Evet |
| 3 | Basit DWG (<2 MB, az entity) | ≤2 sn'de geometri görünür | 10 sn+ veya eksik geometri | ✅ Evet |
| 4 | Orta DWG (2-10 MB, mimari plan) | ≤5 sn'de geometri görünür | 20 sn+ veya eksik | Hayır |
| 5 | Türkçe TEXT/MTEXT | İ, ğ, ş, ç, ö, ü karakterleri okunur | Bozuk karakter | ✅ Evet |
| 6 | Nested block (3+ seviye) | Doğru konumda render | Yanlış konum veya eksik | Hayır |
| 7 | Hatch (ANSI31 + SOLID fill) | Görünür ve doğru yerde | Boş alan veya crash | Hayır |
| 8 | DIMENSION (linear + aligned) | Ok uçları + metin doğru | Eksik veya bozuk | Hayır |
| 9 | Pan + pinch zoom | 30+ FPS (Release build), takılma yok | <15 FPS sürekli | ✅ Evet |
| 10 | Büyük DWG (15-25 MB) | Açılır (yavaş olabilir) | OOM crash | Hayır |
| 11 | Layer show/hide | Çalışır | Çalışmaz | Hayır |
| 12 | Transitive license audit | Tüm runtime MIT/Apache/BSD | GPL veya belirsiz lisans | ✅ Evet |

Karar tablosu:
- Blocker testlerden 1 bile FAIL → ProCad runtime bağımlılığı REDDEDİLİR
- Non-blocker testlerden 4+ FAIL → ProCad runtime bağımlılığı REDDEDİLİR
- Aksi halde ProCad ana rendering katmanı olarak kabul edilir

REDDEDİLDİĞİNDE:
- ProCad'in kaynak kodu referans olarak kullanılır (MIT lisans izin verir)
- Kendi ince RenderScene katmanımız ACadSharp + SkiaSharp üzerinde kurulur
- ProCad'den yararlı algoritmalar uygun attribution ile adapte edilir
```

## 2.3 Ek: ProCad spike sırasında ÖZELLİKLE kontrol edilecekler

```
ProCad'in Android'de çalışma kalitesi belirsiz olduğu için 
spike sırasında şu konulara ÖZEL dikkat:

1. ProCad.Controls.Maui, SKGLView mi SKCanvasView mi kullanıyor?
   - SKGLView → threading dikkat, potansiyel crash
   - SKCanvasView → daha güvenli ama CPU-bound

2. ProCad'in Skia nesnelerini (SKPaint, SKPath) yönetimi
   - Her frame new'liyor mu? → GC baskısı, micro-stutter
   - Pool/reuse yapıyor mu? → daha iyi performans

3. ProCad'in bellek yönetimi
   - Büyük dosya sonrası Close/Dispose düzgün çalışıyor mu?
   - İkinci dosya açıldığında bellek sızıntısı var mı?

4. ProCad'in MAUI lifecycle entegrasyonu
   - App background'a gittiğinde ne oluyor?
   - Screen rotate'te state korunuyor mu?
   - Memory warning geldiğinde cache temizliyor mu?

5. ProCad'in NuGet paketleri preview mi stable mi?
   - Preview paketler production release'te risk
```

---

# 3. KRİTİK — .NET MAUI ANDROID PERFORMANS TUZAKLARI

## 3.1 Planın sessiz kaldığı konu

Plan MAUI'yi doğru gerekçelerle seçiyor (Bölüm 11). Gerekçeler isabetli:
- ACadSharp, ProCad, SkiaSharp hepsi .NET
- Tek codebase Android + iOS
- Native packaging, lifecycle, file picker

Ancak MAUI'nin Android'deki **bilinen performans tuzakları** planda hiç geçmiyor. Bu tuzaklar projeyi engelleyici seviyede etkileyebilir ve habersiz yakalanmak moral bozucudur.

## 3.2 Plana eklenmesi gereken MAUI Android kuralları

```
### MAUI Android Performans Zorunlulukları (Bölüm 11'e ek)

#### Kural 1: HER ZAMAN Release modda performans testi yap
Debug build'de MAUI Android performansı yanıltıcı şekilde kötüdür.
JIT overhead, debugger iletişimi ve eksik optimizasyon nedeniyle 
Debug'da 5 FPS olan uygulama Release'de 45 FPS olabilir.

YANLIŞ:
  "Debug build'de 8 FPS, performans kabul edilemez"
  → Yanlış sonuç çıkardın, Release'de test et

DOĞRU:
  "Release + AOT build'de gerçek telefonda 35 FPS"
  → Bu gerçek performans verisi

#### Kural 2: AOT (Ahead-of-Time) derleme AÇIK olmalı
Android csproj dosyasına:

  <PropertyGroup Condition="$(TargetFramework.Contains('-android')) 
                             and '$(Configuration)' == 'Release'">
    <RunAOTCompilation>true</RunAOTCompilation>
  </PropertyGroup>

eklenmeli. Bu olmadan:
- CAD parse sırasında JIT (Just-in-Time) derleme spike'ları oluşur
- Kullanıcıya "uygulama dondu" hissi verir
- İlk açılışta özellikle kötü ("cold start" JIT penalty)

AOT dezavantajı: APK boyutu artar (~10-20 MB ek)
Ama performans kazancı buna değer.

#### Kural 3: SKCanvasView vs SKGLView kararı bilinçli verilmeli
Plan Bölüm 12'de "SKCanvasView veya uygun GPU-backed Skia view" diyor 
ama bu kararın sonuçları açıklanmıyor.

SKCanvasView (Software):
  ✅ Daha stabil, tahmin edilebilir davranış
  ✅ UI thread'de çalışır — MAUI kontrollerine güvenle erişilir
  ✅ Tüm Android cihazlarda tutarlı
  ❌ CPU-bound, karmaşık çizimlerde yavaş olabilir

SKGLView (GPU/OpenGL ES):
  ✅ GPU hızlandırmalı — teorik olarak daha hızlı
  ❌ PaintSurface olayı background thread'de çalışabilir
  ❌ PaintSurface içinden MAUI UI kontrollerine erişim CRASH yapar
  ❌ "Handler not found" hataları rapor edilmiş
  ❌ Bazı cihazlarda görsel farklılıklar (alpha blending, renk)

ÖNERİ:
  Spike'ta SKCanvasView ile başla.
  Profiling sonrası CPU darboğazı tespit edilirse SKGLView'a geç.
  SKGLView kullanıyorsan PaintSurface handler'ında ASLA
  MAUI kontrol özelliklerine (IsVisible, Width, Height vb.) erişme.

#### Kural 4: GC (Garbage Collector) baskısını minimize et
PaintSurface her frame'de çağrılır. İçinde:

YANLIŞ (her frame yeni nesne):
  void OnPaintSurface(SKPaintSurfaceEventArgs e)
  {
      var paint = new SKPaint { Color = SKColors.Red };  // ❌
      var path = new SKPath();  // ❌
      // ... çizim
  }

DOĞRU (nesne havuzu / field seviyesinde):
  private readonly SKPaint _linePaint = new() { Color = SKColors.Red };
  private readonly SKPath _reusablePath = new();
  
  void OnPaintSurface(SKPaintSurfaceEventArgs e)
  {
      _reusablePath.Reset();  // ✅ Yeniden kullan
      // ... çizim
  }

Aksi halde GC spike'ları "micro-stutter" yaratır.
Kullanıcı pan/zoom sırasında kısa donmalar hisseder.

#### Kural 5: Cold start optimizasyonu
MAUI Android'de cold start (uygulama ilk açılış) masaüstü .NET'e 
göre belirgin şekilde yavaştır (3-8 saniye olabilir).

Çözümler:
  1. Splash screen ile kullanıcıya yükleniyor hissi ver
  2. Lazy initialization — CAD engine'i app açılışında değil 
     ilk dosya açıldığında başlat
  3. AOT derleme cold start'ı iyileştirir
  4. Trimming etkinleştir (kullanılmayan .NET kodunu kes):
     <PublishTrimmed>true</PublishTrimmed>
     DİKKAT: Trimming reflection kullanan kodu kırabilir.
     ACadSharp ve ProCad trimming uyumluluğu TEST EDİLMELİ.

#### Kural 6: Profiler her zaman kullan
MAUI Android performance sorunlarını tahmin etme, ölç.
- Visual Studio Profiler (CPU, Memory, GPU)
- Android Studio Profiler (daha detaylı native profiling)
- dotnet-trace ve dotnet-counters
- SkiaSharp frame time ölçümü (Stopwatch ile OnPaintSurface süresi)
```

---

# 4. KRİTİK — PLANDA HİÇ BAHSEDİLMEYEN: SKCanvasView vs SKGLView KARARI

## 4.1 Neden kritik

Plan Bölüm 12'de SkiaSharp'ı render motoru olarak seçiyor ve "SKCanvasView veya uygun GPU-backed Skia view" diyor. Ancak bu iki seçenek arasındaki fark, uygulamanın **temel rendering mimarisini** belirler ve geri dönüşü zordur.

## 4.2 Plana eklenmeli

```
### Bölüm 12'ye ek: SkiaSharp View Seçim Stratejisi

Karar: Spike'ta ikisini de test et, somut veriye göre seç.

Test protokolü:
1. Aynı DWG dosyasını iki farklı view ile render et
2. Aynı gerçek telefonda FPS karşılaştır
3. Bellek tüketimini karşılaştır
4. Gesture (pan/zoom) akıcılığını karşılaştır
5. Farklı cihazlarda tutarlılığı kontrol et

Eğer SKCanvasView 30+ FPS veriyorsa → SKCanvasView kullan
Eğer SKCanvasView <20 FPS ve SKGLView >30 FPS → SKGLView kullan
Her iki durumda da threading kurallarına uy
```

---

# 5. KRİTİK — PLANDA EKSİK OLAN "FALLBACK DXF-ONLY" STRATEJİSİ

## 5.1 Neden önemli

Plan DWG'yi ana format olarak hedefliyor (doğru). Ancak ACadSharp'ın alpha olduğu ve bazı DWG versiyonlarında sorun çıkarabileceği göz önüne alındığında, belirli dosyalar için **DXF fallback** stratejisi daha net tanımlanmalı.

## 5.2 Plana eklenmeli

```
### DXF Fallback ve İkinci Parser Stratejisi

ACadSharp belirli bir DWG dosyasını okuyamadığında akış:

1. Kullanıcıya anlamlı hata mesajı ver:
   "Bu DWG dosyası açılamadı. Dosya bozuk olabilir 
   veya desteklenmeyen bir sürüm içeriyor olabilir."

2. Eğer aynı dizinde aynı isimli .dxf varsa öneri göster:
   "Aynı dizinde [dosyaadı.dxf] bulundu. Onu açmayı denemek ister misiniz?"

3. netDXF (MIT lisanslı, yalnız DXF) kütüphanesini 
   yedek DXF parser olarak DEĞERLENDİR:
   - netDXF, DXF konusunda ACadSharp'tan daha olgun ve stabil
   - Ancak DWG desteği YOK
   - Bu ikinci parser değil, yedek güvenlik ağı
   
   Karar: Spike sonrasında ACadSharp'ın DXF read kalitesi yeterliyse
   netDXF eklemeye GEREK YOK. Yetersizse değerlendir.

4. Kullanıcıya "dosyayı DXF olarak dönüştürüp tekrar deneyin" 
   tavsiyesi verilebilir (bu dönüşümü uygulama YAPMAZ, 
   kullanıcı masaüstünde yapar).

Ana yol ACadSharp üzerinden DWG + DXF okumadır.
Fallback stratejisi ana yolu DEĞİŞTİRMEZ, tamamlar.
```

---

# 6. ÖNEMLİ — TÜRKÇE KARAKTERLERİN ÖTESİNDE: ENCODING PROBLEMİ

## 6.1 Planın eksikliği

Bölüm 22 Türkçe karakterleri test etmeyi belirtiyor. Ancak sorun sadece Unicode değil.

Türk mühendislik firmalarından gelen DWG dosyalarının önemli bir kısmı **2000'li yılların başından** kalma. Bu dosyalarda metin Unicode değil, **Windows-1254 (Türkçe code page)** encoding kullanır. Eski AutoCAD versiyonları (R14, R2000, R2004) varsayılan olarak Unicode değil, sistem code page'ini kullanırdı.

## 6.2 Plana eklenmeli (Bölüm 22'ye entegre edilecek)

```
### Code Page ve Encoding Stratejisi — Bölüm 22'ye Ek

Problem:
Eski DWG dosyalarındaki Türkçe metinler Windows-1254 encoding ile 
saklanmıştır. .NET'in modern sürümleri bu encoding'i varsayılan olarak 
TANIMAZ — bu encoding'ler System.Text.CodePagesEncodingProvider 
NuGet paketiyle aktifleştirilmelidir.

ZORUNLU kod (MauiProgram.cs — CreateMauiApp metodunun EN BAŞINA):

  System.Text.Encoding.RegisterProvider(
      System.Text.CodePagesEncodingProvider.Instance
  );

Bu satır olmazsa:
- Eski R2000/R2004 DWG'lerdeki "Kalıp Planı" → "Kal?p Plan?" olur
- "Müşteri: Ahmet Güneş" → "M??teri: Ahmet G?ne?" olur
- Donatı detaylarındaki "Ø16" sembolü kaybolabilir
- Mahal yazıları (SALON, YATAK ODASI, vb.) bozuk görünür

Bu satır projenin ilk günü eklenmeli ve HİÇBİR KOŞULDA kaldırılmamalıdır.
Kod review'da bu satır yoksa → build FAIL yapan bir guard eklenebilir.

DWG header'ında code page bilgisi:
- $DWGCODEPAGE header değişkeni dosyanın encoding'ini belirtir
- Yaygın Türkçe değerler: ANSI_1254, cp1254
- ACadSharp'ın bu değişkeni okuyup doğru encoding'i uygulayıp 
  uygulamadığı spike'ta TEST EDİLMELİ

Test corpus'una mutlaka eklenmesi gereken encoding dosyaları:
- Windows-1254 encoded R2000 DWG (Türkçe metinli)
- Windows-1254 encoded R2004 DWG (SHX font + Türkçe)
- UTF-8 encoded R2018+ DWG (Unicode Türkçe)
- Karışık: bazı text'ler eski encoding, bazıları Unicode
```

---

# 7. ÖNEMLİ — BÜYÜK DOSYA STRATEJİSİ ÇOK SOYUT KALIYOR

## 7.1 Planın zayıflığı

Bölüm 34-39 performans konusunu ele alıyor ama büyük dosya stratejisi soyut. Bölüm 39'da "20 MB DWG çok daha yüksek RAM kullanabilir" deniyor ama somut sayılar yok.

## 7.2 Plana eklenmeli

```
### Somut Bellek Bütçesi Modeli (Bölüm 39'a ek)

Android cihaz bellek limitleri (largeHeap=false / true):
- Düşük segment (~2 GB RAM): ~128 MB / ~256 MB heap limit
- Orta segment (~4 GB RAM): ~256 MB / ~512 MB heap limit
- Üst segment (~8+ GB RAM): ~384 MB / ~768 MB heap limit

DWG dosya boyutu → RAM tüketimi deneysel çarpan tablosu:

| DWG Boyutu | Parse (CadDocument) | Scene Build | Render Cache | TOPLAM Tahmini |
|---|---|---|---|---|
| 1 MB | ~8-15 MB | ~5-10 MB | ~3-5 MB | ~16-30 MB |
| 5 MB | ~40-75 MB | ~25-50 MB | ~15-25 MB | ~80-150 MB |
| 10 MB | ~80-150 MB | ~50-100 MB | ~30-50 MB | ~160-300 MB |
| 20 MB | ~160-300 MB | ~100-200 MB | ~60-100 MB | ~320-600 MB |
| 50 MB | ~400-750 MB | ~250-500 MB | ~150-250 MB | ~800 MB-1.5 GB |

Çarpanı etkileyen faktörler:
- Block sayısı (çok block = çok cache)
- Hatch yoğunluğu (yoğun hatch = çok geometry)
- Text miktarı (çok text = çok font shaping)
- Nested block derinliği (derin = çok transform matrisi)

Somut kurallar:

1. largeHeap="true" AndroidManifest'e EKLENMELİ:
   <application android:largeHeap="true" ...>
   Ama bu sihirli çözüm DEĞİL — sadece limiti artırır

2. Dosya açılmadan ÖNCE boyut uyarısı:
   - 20 MB üstü: "Bu dosya büyük. Açılması biraz sürebilir."
   - 50 MB üstü: "Bu dosya çok büyük. Cihazınızın belleği 
     yetmeyebilir. Devam etmek istiyor musunuz?"
   - 100 MB üstü: "Bu dosya deneysel boyuttadır (100+ MB). 
     Uygulama yanıt vermeyebilir veya kapanabilir."

3. Parse sırasında bellek izleme:
   - GC.GetTotalMemory() ile periyodik kontrol
   - Bellek limitinin %80'ine yaklaşılırsa parse iptal
   - Kullanıcıya: "Bellek yetersiz. Dosya kapatılıyor."

4. MVP'de hedef dosya boyutu desteği:
   - ≤20 MB: Tam destek, sorunsuz çalışmalı
   - 20-50 MB: "Best effort", uyarı ile
   - 50+ MB: Deneysel, crash kabul edilebilir

5. Aşama 4 (Performans) hedefi: 
   50 MB dosyaları güvenilir açabilmek
```

---

# 8. ÖNEMLİ — DIMENSION RENDER SORUNUNUN DERİNLİĞİ

## 8.1 Planın hafife aldığı konu

Bölüm 25 dimension'ı test edilecek entity olarak listeliyor. Ancak DWG'de DIMENSION render etmek **en zor entity rendering problemlerinden biridir**.

## 8.2 DIMENSION'ın karmaşık iç yapısı

```
DWG'de DIMENSION entity'si tek bir çizim nesnesi DEĞİLDİR.
Aslında bir "meta-entity"dir. İç yapısı:

DIMENSION (entity)
├── Geometrik tanım
│   ├── Definition points (ölçülen noktalar)
│   ├── Text position (metin konumu)
│   ├── Rotation (açı)
│   └── Dimension style reference
│
├── *D Anonim Blok (anonymous block) referansı
│   └── Bu bloğun içinde:
│       ├── LINE entity'ler (extension lines, dimension line)
│       ├── SOLID entity'ler (ok uçları — üçgen dolgular)
│       ├── MTEXT entity (ölçü metni — "2450" gibi)
│       └── Bazen ARC (angular dimension için)
│
└── Dimension Style
    ├── Ok tipi (solid, architectural tick, dot, open...)
    ├── Ok boyutu
    ├── Text height
    ├── Text gap
    ├── Extension line offset
    ├── Suppress extension line 1/2
    ├── Text alignment
    ├── Text rotation
    ├── Scale factor
    ├── Tolerans ayarları
    └── 70+ farklı parametre...

AutoCAD'in davranışı:
1. DWG açılırken anonim bloktaki hazır geometriyi kullanır
2. Anonim blok yoksa/eskiyse "REGEN" ile yeniden hesaplar
3. REGEN, dimension style'ın 70+ parametresini işler

Bu uygulama için iki strateji:

**Strateji A: Anonim blok render (MVP için önerilen)**
- ACadSharp'tan DIMENSION'ın *D anonim bloğunu al
- Bu bloğu normal INSERT/block gibi render et
- İçindeki LINE, SOLID, MTEXT ayrı entity olarak çizilir
Avantaj: 
  - Hızlı implementasyon
  - AutoCAD'in hesapladığı sonucu gösterir
  - Dimension style'ın 70+ parametresini bilmemize gerek yok
Dezavantaj:
  - Anonim blok bazen eksik (özellikle DXF dosyalarda)
  - Kullanıcı AutoCAD'de dimension text override yaptıysa 
    anonim blok güncel olmayabilir

**Strateji B: Geometrik tanımdan hesaplama (Aşama 3)**
- DIMENSION parametrelerinden kendi geometrimizi üret
- Tüm dimension style parametrelerini doğru uygulamak gerekir
Avantaj:
  - Her zaman güncel
  - Anonim blok eksik olsa bile çalışır
Dezavantaj:
  - ÇOK karmaşık (70+ parametre)
  - AutoCAD ile birebir uyum zor
  - Geliştirme süresi çok uzun

KARAR:
- MVP (Aşama 2): Strateji A — anonim blok render
- Anonim blok eksikse: Basitleştirilmiş fallback 
  (iki nokta arası düz çizgi + metin)
- Aşama 3: Strateji B araştırması başlar
- Anonim blok eksikliği uyarısı kullanıcıya gösterilir
```

---

# 9. ÖNEMLİ — PLANDA HİÇ BAHSEDİLMEYEN: LINETYPE PATTERN RENDERING

## 9.1 Eksiklik

Plan Bölüm 26'da lineweight'i ele alıyor ve linetype'ı listede geçiriyor ama **linetype rendering** konusu derinlemesine ele alınmıyor. Oysa mühendislik çizimlerinde çizgi tipleri hayati önem taşır.

## 9.2 Plana eklenmeli

```
### Linetype Pattern Rendering Stratejisi (Yeni bölüm önerisi)

Mühendislik çizimlerinde çizgi tipleri anlam taşır:
- CONTINUOUS: Düz çizgi → duvar, yapısal eleman, görünen kenar
- DASHED: Kesikli → görünmeyen kenar, aşağıdaki kat
- CENTER: Merkez çizgisi → aks, simetri ekseni
- HIDDEN: Gizli çizgi → alttaki eleman
- PHANTOM: Hayalet çizgi → hareket sınırı
- DOT: Noktalı → referans çizgisi
- Kompleks: Metin + şekil → GAS, ELECTRIC, SU vb.

Yanlış veya eksik linetype rendering:
- Aks çizgisi düz görünürse mühendis "aks yok" sanır
- Hidden çizgi continuous görünürse plan okunabilirliği düşer
- Bu tip hatalar teknik yanıltma olabilir

Render karmaşıklığı seviyeleri:

Seviye 1 (MVP): Basit dash-gap pattern
- SkiaSharp SKPathEffect.CreateDash() kullanımı
- Düz çizgiler (LINE, LWPOLYLINE) için yeterli
- LTSCALE ve entity-level scale çarpanı uygulanır
- Formül: effectiveDash = dashLength × LTSCALE × entityScale

Seviye 2 (Aşama 3): Eğri uyumlu pattern
- Arc, Circle, Spline üzerinde pattern
- SKPathMeasure ile eğri uzunluğu boyunca tessellation
- Pattern eğriye "yapışır", düzleşmez

Seviye 3 (İleri): Kompleks linetype
- Pattern içinde text yerleştirme ("GAS", "ELECTRIC")
- Pattern içinde shape/glyph yerleştirme
- SKPathMeasure.GetPositionAndTangent ile teğet hizalama
- Bu seviye MVP'de KESİNLİKLE gerekmez

LTSCALE çözümleme:
- $LTSCALE: Global çizim geneli çizgi tipi ölçeği
- Entity'nin kendi LineTypeScale özelliği
- $PSLTSCALE: Paper space'te viewport ölçeğine göre ayarlama
- Efektif ölçek = LTSCALE × EntityScale × (PSLTSCALE ? ViewportScale : 1)

MVP'de minimum:
- CONTINUOUS, DASHED, CENTER, HIDDEN tanınmalı
- Basit dash-gap pattern doğru uzunlukta çizilmeli
- LTSCALE uygulanmalı
- Kompleks linetype düz çizgi olarak gösterilebilir + uyarı
```

---

# 10. ÖNEMLİ — PLANDA EKSİK: SOLID, TRACE, 3DFACE ENTITY'LERİ

## 10.1 Planın eksikliği

Bölüm 20 entity öncelik matrisinde SOLID P0 olarak listeleniyor. Ancak SOLID entity'nin ne olduğu ve nasıl render edileceği hiç açıklanmıyor.

## 10.2 Plana eklenmeli

```
### SOLID, TRACE ve Benzer Dolgulu Entity'ler

DWG'de SOLID entity'si (3D katı cisim SOLID DEĞİL) 3 veya 4 
noktadan oluşan düz dolgulu bir alandır. Özellikle şuralarda kullanılır:
- Ok uçları (DIMENSION'ların anonim bloklarında)
- Kalın çizgi simülasyonu (eski çizim tekniği)
- Basit dolgulu alanlar

SOLID entity'nin tuzağı:
AutoCAD'de SOLID'in 4. noktası (P4), P3 ile P4 çapraz 
sıralanır — yani nokta sırası P1-P2-P4-P3 şeklinde 
"butterfly" (kelebek) sıralamasıdır, P1-P2-P3-P4 DEĞİL.

Yanlış render:
  P1 ─── P2        ← Normal dikdörtgen beklersin
  │       │
  P3 ─── P4

Doğru render (AutoCAD sıralaması):
  P1 ─── P2        ← Çapraz sıralama
   ╲   ╱
    ╲ ╱
   ╱ ╲
  P4 ─── P3        ← P3 ve P4 yer değiştirir

Bu tuzak bilinmezse ok uçları papyon şeklinde çizilir.

Render kodu:
  SKPath path = new();
  path.MoveTo(p1);
  path.LineTo(p2);
  path.LineTo(p4);  // DİKKAT: p4, p3 DEĞİL!
  path.LineTo(p3);
  path.Close();
  canvas.DrawPath(path, fillPaint);
```

---

# 11. ÖNEMLİ — ProCad KAYNAK KODU KULLANIM STRATEJİSİ

Bölüm 10 "ProCad'den yalnızca MIT lisanslı ve işimize yarayan mimari/fikir/kod parçaları uygun attribution ile uyarlanabilir" diyor. Ama bu pratikte ne anlama geliyor?

```
### ProCad Kod Kullanım Karar Matrisi

| Kullanım Şekli | Hukuki Durum | Gereksinimler |
|---|---|---|
| NuGet package bağımlılığı | MIT uyumlu | License notice |
| Kaynak koddan dosya kopyalama | MIT uyumlu | Dosya başında MIT copyright notice korunmalı |
| Algoritmik fikir/yaklaşım öğrenme | Tamamen serbest | Attribution iyi niyet (zorunlu değil) |
| Fork + modifiye | MIT uyumlu | Orijinal MIT notice + kendi değişiklik notu |

MIT lisans gereği ZORUNLU olanlar:
1. ProCad'in MIT lisans metninin bir kopyası THIRD_PARTY_NOTICES'ta yer almalı
2. Kopyalanan dosyalardaki orijinal copyright notice KORUNMALI
3. Kendi kodumuzla birleştiriyorsak bile MIT notice kaldırılmamalı

MIT lisans gereği ZORUNLU OLMAYANLAR:
- Kaynak kodu açmak (açmak zorunda DEĞİLSİN)
- ProCad'e geri katkı yapmak (iyi niyet, zorunlu değil)
- Ayrı lisans satın almak
- ProCad yazarından izin almak

Özellikle adapte edilmesi değerli ProCad bileşenleri:
1. Entity → RenderPrimitive dönüşüm mantığı (SceneBuilder)
2. Block/INSERT transform hesaplamaları
3. Hatch boundary → SKPath dönüşümü
4. SHX font rendering pipeline
5. Dimension anonim blok çözümleme
6. Camera/viewport transform matrisi
7. Layer visibility resolve (ByLayer/ByBlock zinciri)

Bu bileşenler sıfırdan yazmak yerine ProCad'den 
öğrenerek veya adapte ederek kullanılabilir.
```

---

# 12. ÖNEMLİ — DARK/LIGHT TEMA RENK ÇELİŞKİSİ

Plan Bölüm 26'da renkleri ele alıyor ve "drawing background light/dark" MVP'de var diyor (Bölüm 16). Ancak CAD renk sistemi ile arka plan etkileşimi çok spesifik kurallar gerektirir.

```
### CAD Renk ↔ Arka Plan Uyumu (Bölüm 26'ya ek)

AutoCAD ACI (AutoCAD Color Index) renk tablosunda:
- Renk 7: "White/Black" — arka plana göre otomatik ters döner
  - Siyah arka plan → Beyaz
  - Beyaz arka plan → Siyah
  Bu tek renk değişir, diğerleri SABİTTİR.

- Renk 0: ByBlock — bloğun yerleştirildiği INSERT'ın rengini alır
- Renk 256: ByLayer — entity'nin layer'ının rengini alır

Arka plan rengine göre okunabilirlik sorunu:
| ACI Renk | Siyah Arka Plan | Beyaz Arka Plan |
|---|---|---|
| 1 (Kırmızı) | ✅ İyi okunur | ✅ İyi okunur |
| 2 (Sarı) | ✅ İyi okunur | ⚠️ Zor okunur |
| 3 (Yeşil) | ✅ İyi okunur | ⚠️ Zor okunur |
| 4 (Cyan) | ✅ İyi okunur | ⚠️ Zor okunur |
| 5 (Mavi) | ✅ İyi okunur | ✅ İyi okunur |
| 7 (Beyaz) | ✅ Beyaz | ✅ Siyah (otomatik) |

MVP gereksinimleri:
1. Varsayılan arka plan: Koyu (siyah veya #1E1E1E)
   - Çoğu CAD kullanıcısı koyu arka plan bekler
   - ACI renkleri koyu arka planda EN İYİ okunur
2. Renk 7 → arka plana göre otomatik ters çevirme
3. Açık arka plan seçeneği sunulmalı
4. Açık arka planda renk 2,3,4 için opsiyonel:
   - "Koyu renk modu" (ACI renklerini daha koyu tonlara çevir)
   - Veya: arka plan tam beyaz değil #F5F5F5 gibi hafif gri

True Color (24-bit RGB) entity'ler:
- Bunlar ACI renk tablosuna bağlı değil
- Kullanıcının atadığı tam RGB değeri
- Arka plana göre otomatik çevirme YAPILMAZ
- Ama arka planla aynı renkse görünmez olabilir
- Bu edge case ilk MVP'de göz ardı edilebilir
```

---

# 13. ÖNEMLİ — SHX FONT STRATEJİSİ DAHA SOMUT OLMALI

```
### SHX Font Dağıtım Stratejisi — Somut Plan (Bölüm 23'e ek)

Autodesk SHX fontları Autodesk'e aittir — izinsiz dağıtılamaz.
Bu fontların en yaygınları:
- simplex.shx (en yaygın teknik yazı fontu)
- txt.shx (standart yazı)
- romans.shx, romand.shx, romanc.shx
- isocp.shx, isocpeur.shx (ISO teknik yazı)
- complex.shx
- Türkçe eklenmiş: trstd.shx ve benzerleri

Kullanılabilecek permissive alternatifler:

1. ixmilia/shx (MIT) — SHX bytecode parser
   Bu bir PARSER, font dosyası DEĞİL.
   SHX dosyalarını okuyup vektörel glyph verisine çevirir.

2. Hershey font seti (Public Domain)
   1960'lardan kalma vektörel fontlar.
   SHX fontlarının geometrik stiliyle benzerlik taşır.
   Public domain → özgürce dağıtılabilir.
   Dezavantaj: Türkçe karakter desteği eksik olabilir.

3. Noto Sans / Roboto (Apache-2.0 veya OFL)
   Google fontları. Mükemmel Türkçe desteği.
   TTF → SHX tarzı rendering için kullanılabilir.
   Dezavantaj: SHX fontlarından görsel olarak farklı.

4. Custom vektörel font üretimi
   Permissive TTF'den vektörel glyph çıkararak
   kendi SHX-benzeri font setimizi üretebiliriz.
   Zaman alıcı ama en temiz çözüm.

Somut fallback zinciri:

DWG referans ediyor: "simplex.shx"
  ↓ Adım 1: Kullanıcı daha önce bu fontu import etti mi?
    → Evet → kullanıcının import ettiği fontu kullan
  ↓ Hayır
  ↓ Adım 2: Uygulama bundle'ında eşleme var mı?
    → simplex.shx → bundled_simplex_alt (Hershey veya custom)
  ↓ Hayır
  ↓ Adım 3: Generic TTF fallback
    → Noto Sans Mono (genişlik faktörü korunarak)
  ↓
  ↓ Adım 4: Kullanıcıya bilgi göster
    "1 font bulunamadı, benzer font kullanıldı"

Genişlik faktörü (Width Factor) neden önemli:
- SHX fontlar mono-spaced değil
- Her karakterin width factor'ü var
- TTF fallback'te bu factor uygulanmazsa:
  - "Ø16/200" metni "Ø16/200 " olur (fazla boşluk)
  - Veya blok attribute'lar çerçeveden taşar
  - Dimension metinleri kaydırılmış görünür

Gelecekte "Font Yöneticisi" özelliği:
- Kullanıcı kendi SHX dosyalarını uygulamaya import eder
- İmport edilen fontlar cihazda lokal kalır
- SHX dosyaları sunucuya GÖNDERİLMEZ
- Font eşleme tablosu kullanıcı ayarlarında saklanır
```

---

# 14. ÖNEMLİ — HATA KURTARMA (CRASH RECOVERY)

```
### Crash Recovery Stratejisi (Yeni bölüm önerisi)

1. Global exception handler (Bölüm 47'ye ek):
   MauiProgram.cs'te:
   - AppDomain.CurrentDomain.UnhandledException yakalanmalı
   - TaskScheduler.UnobservedTaskException yakalanmalı
   - AndroidEnvironment.UnhandledExceptionRaiser (Android-specific)
   
   Crash anında yapılacaklar:
   a) Son açılan dosya yolunu Preferences'a kaydet
   b) Hata özetini local dosyaya yaz (stack trace dahil)
   c) "last_crash" flag'ini true yap
   d) Mümkünse kontrollü kapanış (Application.Current.Quit())

2. Yeniden açılış güvenliği:
   App açılışında:
   if (Preferences.Get("last_crash", false))
   {
       // Son crash eden dosyayı OTOMATİK AÇMA
       // Kullanıcıya: "Uygulama son kullanımda beklenmedik 
       //   şekilde kapandı. Ana ekrana yönlendiriliyorsunuz."
       Preferences.Set("last_crash", false);
   }

3. OOM koruması:
   Parse başlamadan önce:
   a) Dosya boyutunu kontrol et
   b) Java.Lang.Runtime.GetRuntime().MaxMemory() ile heap limitini al
   c) Java.Lang.Runtime.GetRuntime().FreeMemory() ile boş belleği al
   d) Tahmini gereksinim: dosyaBoyutu × 10
   e) Yeterli bellek yoksa kullanıcıya uyarı

4. Parse timeout:
   ACadSharp.Read() mutlaka CancellationToken ile çağrılmalı.
   
   using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
   try
   {
       await Task.Run(() => DwgReader.Read(path, notification), cts.Token);
   }
   catch (OperationCanceledException)
   {
       ShowError("Dosya çok karmaşık veya büyük. Açma işlemi 
                  zaman aşımına uğradı.");
   }

5. Scene build timeout:
   Parse başarılı olsa bile scene build (SceneBuilder) çok entity 
   içeren dosyalarda uzun sürebilir. Bu da ayrı timeout'a tabi olmalı.
```

---

# 15. ÖNEMLİ — "OPEN WITH" ÖZELLİĞİ MVP'YE ALINMALI

Plan Bölüm 31'de "MVP sonrasında" diyor. **Bu YANLIŞ bir önceliklendirme.**

```
### Neden MVP'de olmalı — Gerçek kullanım senaryosu

Mühendis Ahmet'in günlük iş akışı:
1. Müteahhit WhatsApp'tan "kalip_plani_zemin.dwg" gönderir
2. Ahmet dosyaya dokunur
3. Telefon "Bu dosyayı açacak uygulama yok" der
4. Ahmet dosyayı indirir
5. Uygulamamızı açar
6. "Dosya Aç" → Downloads → dosyayı bulur → Aç
7. Çizim ekranda

Open With olsaydı:
1. Müteahhit WhatsApp'tan "kalip_plani_zemin.dwg" gönderir
2. Ahmet dosyaya dokunur
3. "Şununla aç" → uygulamamız
4. Çizim ekranda

7 adım → 4 adım. Bu %43 azalma.

Teknik zorluk: ÇOK DÜŞÜK
Android:
  AndroidManifest.xml'e birkaç satır XML:
  
  <intent-filter>
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <data android:scheme="content" />
    <data android:scheme="file" />
    <data android:mimeType="application/octet-stream" />
    <data android:pathPattern=".*\\.dwg" />
    <data android:pathPattern=".*\\.DWG" />
    <data android:pathPattern=".*\\.dxf" />
    <data android:pathPattern=".*\\.DXF" />
  </intent-filter>

  + MAUI tarafında intent'ten gelen URI'yi okumak 
    (OnNewIntent override)

iOS:
  Info.plist'te UTType tanımı
  + LSItemContentTypes + CFBundleDocumentTypes

Sonuç: Bu özellik Aşama 2 (Android Preview MVP) görevlerine TAŞINMALI.
Teknik olarak 1-2 saatlik iş, kullanıcı deneyiminde DEV fark.
```

---

# 16. ÖNEMLİ — ACadSharp EKOSİSTEMİNDEN YARARLANMA

```
### ACadSharp Ekosistemi (Yeni bölüm önerisi)

ACadSharp etrafında gelişen ek projeler:

1. ACadSharp.Image — DWG/DXF'i raster görüntüye render eder
   Potansiyel kullanımlar:
   - CI/CD'de headless regression testi (DWG → PNG → diff)
   - Son açılanlar listesinde thumbnail (küçük önizleme)
   - Dosya bilgisi ekranında statik görüntü

2. ACadSharp.Pdf — DWG/DXF'i PDF'e dönüştürür
   Potansiyel kullanımlar:
   - "PDF olarak paylaş" özelliği
   - Ayrı PDF SDK gerektirmez
   - SkiaSharp'ın kendi SKDocument.CreatePdf'i de alternatif

DİKKAT:
- Bu projelerin lisansları AYRICA audit edilmeli
- Transitive dependency'leri kontrol edilmeli
- Core viewer bunlara bağımlı OLMAMALI — opsiyonel eklenti
- Spike'ta DEĞİL, Aşama 3+ sonrasında değerlendirilmeli
```

---

# 17. ÖNEMLİ — GOOGLE PLAY VE APP STORE YAYINLAMA DETAYLARINDAKİ EKSİKLER

Plan Bölüm 43-44'te genel çerçeveyi veriyor ama somut gereksinimler eksik.

```
### Google Play Yayınlama Detayları (Bölüm 43'e ek)

1. DATA SAFETY FORMU — ZORUNLU
   2026'da Google, Data Safety formunu doldurmanızı ZORUNLU kılar.
   Uygulamam veri TOPLAMIYOR olsa bile formu doldurmak ZORUNLU.
   
   Bu uygulama için form cevapları:
   - Kişisel veri topluyor musunuz? → HAYIR
   - Dosyaları sunucuya gönderiyor musunuz? → HAYIR
   - Analytics SDK var mı? → HAYIR
   - Crash reporting SDK var mı? → Eğer eklersek EVET olur
   - Cihaz tanımlayıcı topluyor musunuz? → HAYIR
   
   DİKKAT: Gelecekte analytics veya crash reporting eklenirse
   Data Safety formu GÜNCELLENMELİ. Güncellenmezse uygulama
   mağazadan KALDIRILABİLİR.

2. GİZLİLİK POLİTİKASI — ZORUNLU
   Google Play'de Privacy Policy linki ZORUNLU.
   Basit bir web sayfası olabilir:
   "Bu uygulama hiçbir kişisel veri toplamaz. 
   Dosyalarınız cihazınızda kalır ve hiçbir sunucuya gönderilmez."
   GitHub Pages'da ücretsiz barındırılabilir.

3. TARGET SDK — YENİLENMELİ
   Google Play, her yıl minimum target SDK seviyesini artırır.
   Yayınlama tarihinde güncel Target API Level kontrol edilmeli.

4. AAB FORMATI — ZORUNLU
   Google Play artık APK kabul ETMİYOR.
   Android App Bundle (.aab) olarak yüklenmelidir.
   MAUI'de: dotnet publish -f net10.0-android -c Release 
   → çıktı .aab olarak üretilir

5. APP SİGNİNG
   - Keystore dosyası oluşturulmalı ve GÜVENLİ saklanmalı
   - Keystore KAYBOLUIRSA uygulama güncelenemez
   - Google Play App Signing ile Google'a güvenle yedeklenmeli
   - .csproj'da Release konfigürasyonunda signing ayarları

6. STORE LİSTİNG
   - Uygulama adı (nötr, DWG/Autodesk çağrıştırmayan)
   - Kısa açıklama (80 karakter)
   - Uzun açıklama (4000 karakter)
   - En az 2 screenshot (telefon)
   - Feature graphic (1024×500)
   - Uygulama ikonu (512×512)
   - Kategori: Tools veya Productivity

7. İÇERİK DERECELENDİRMESİ
   IARC içerik anketi doldurulmalı.
   Bu uygulama muhtemelen "Everyone" (Herkes) derecesi alır.
```

---

# 18. ORTA — TEST CORPUS STRATEJİSİ

```
### Test Corpus Temin ve Yönetim Stratejisi (Bölüm 49'a ek)

Kaynak 1: Kendi projeler (EN ÖNEMLİ)
- Kullanıcının gerçek mühendislik/mimari projeleri
- ALTMIŞ STANDART test dosyası
- EN AZ 10-15 farklı gerçek proje toplanmalı
- Boyut dağılımı: 3 küçük, 5 orta, 3 büyük, 2 çok büyük

Kaynak 2: Açık kaynak DWG/DXF örnekleri
- ACadSharp'ın kendi test fixture'ları (GitHub repo/test klasörü)
- AutoCAD sample dosyaları
- GitHub'daki açık lisanslı CAD projeleri
  (arama: "dwg sample" filetype:dwg site:github.com)

Kaynak 3: Sentetik test dosyaları
- ACadSharp.DxfWriter ile programatik üretim
- Her P0 entity tipi için ayrı test dosyası
- Edge case dosyaları:
  - Boş dosya (0 entity)
  - Tek entity dosyası
  - 100.000+ entity dosyası
  - İç içe 10 seviye block
  - 50+ layer
  - Yalnız SHX font kullanan dosya
  - Yalnız MTEXT kullanan dosya
  - Yoğun DIMENSION dosyası
  - Yoğun HATCH dosyası

Test dosyası metadata formatı (JSON):
{
  "file": "mimari_kat_plani_3kat.dwg",
  "source": "kendi_proje",
  "dwg_version": "AC1032",
  "file_size_mb": 8.5,
  "expected": {
    "entity_count": 12450,
    "entity_count_tolerance": 0.05,
    "layer_count": 28,
    "block_count": 45,
    "has_paper_space": true,
    "layout_count": 3,
    "has_xref": false,
    "has_shx_fonts": true,
    "shx_fonts": ["simplex.shx", "txt.shx"],
    "has_dimensions": true,
    "has_hatches": true,
    "has_mtext": true,
    "has_turkish_chars": true
  },
  "acceptance": {
    "must_not_crash": true,
    "max_parse_time_ms": 5000,
    "must_contain_entities": ["DIMENSION", "HATCH", "MTEXT", "INSERT"],
    "geometry_must_be_visible": true,
    "turkish_text_must_render": true
  }
}

Test çalıştırma:
- Her commit'te: en az 5 temel dosya parse testi
- Her hafta: tam corpus regression (tüm dosyalar)
- Her ACadSharp version update'te: tam corpus + golden diff
```

---

# 19. ORTA — APK BOYUTU BÜTÇESİ

```
### APK / AAB Boyutu Bütçesi (Yeni bölüm önerisi)

Plan APK boyutunu hiç tartışmıyor. Oysa kullanıcı indirme boyutu
özellikle Türkiye gibi mobil veri maliyetinin hissedildiği 
pazarlarda önemlidir.

Tahmini bileşen boyutları (AAB, tek platform — arm64):
| Bileşen | Tahmini Katkı |
|---|---|
| .NET MAUI runtime + BCL | ~15-20 MB |
| SkiaSharp native library | ~5-8 MB |
| ACadSharp managed DLL | ~2-4 MB |
| HarfBuzzSharp (text shaping) | ~3-5 MB |
| Bundled fontlar (SHX fallback) | ~1-3 MB |
| Uygulama kodu + XAML | ~1-2 MB |
| **Toplam (tek arch, AAB)** | **~27-42 MB** |

AOT derleme etkisi:
- AOT AÇIK: +10-20 MB (native compiled code)
- AOT KAPALI: Daha küçük APK ama daha yavaş runtime

AAB avantajı:
- Google Play, cihaza özel APK üretir
- arm64-v8a cihaza yalnız arm64 native lib gönderilir
- Kullanıcı indirme boyutu APK'dan ~%30-40 küçük

Hedefler:
- AAB upload boyutu: ≤100 MB
- Kullanıcı indirme boyutu (arm64): ≤60 MB
- Trimming ile boyut azaltılabilir AMA:
  ACadSharp ve ProCad trimming uyumluluğu TEST EDİLMELİ
  (reflection kullanan kodlar trimming'de kırılabilir)
```

---

# 20. ORTA — ERİŞİLEBİLİRLİK

```
### Minimum Erişilebilirlik (Yeni bölüm önerisi)

Google Play ve App Store politikaları erişilebilirlik beklentisini artırıyor.

MVP'de minimum:
1. Tüm UI butonlarında anlamlı ContentDescription / AutomationId
   - "Dosya Aç" butonu: "Dosya aç, DWG veya DXF dosyası seçin"
   - "Layers" butonu: "Katman listesi"
   - "Fit" butonu: "Çizimi ekrana sığdır"

2. Yeterli renk kontrastı (WCAG AA: 4.5:1 metin, 3:1 büyük metin)

3. TalkBack (Android) ile temel navigasyon:
   - Ana ekran: Dosya Aç butonuna erişilebilmeli
   - Viewer: Toolbar butonlarına erişilebilmeli
   - CAD canvas kendisi screen reader ile OKUNAMAZ (doğası gereği)

4. Font boyutu: UI metin boyutu sistem ayarına UYUMLU olmalı
   (CAD canvas hariç — çizim metinleri CAD koordinatlarına bağlı)

CAD canvas doğası gereği görsel bir araçtır.
Tam erişilebilirlik gerçekçi değil.
Ama UI kontrolleri erişilebilir olmalı.
```

---

# 21. ORTA — REKABET ANALİZİ

```
### Rekabet Analizi (Yeni bölüm önerisi)

Play Store'daki mevcut DWG viewer'lar ve konumlanmamız:

| Uygulama | Model | DWG Açma | Offline | Gizlilik | Reklam |
|---|---|---|---|---|---|
| AutoCAD Mobile (Autodesk) | Abonelik ($$$) | Mükemmel | Kısmen | Bulut zorunlu | Yok |
| DWG FastView | Ücretsiz | İyi | Kısmen | Endişeli | Var |
| CAD Reader vb. | Freemium | Değişken | Çoğu hayır | Server-side | Var |
| **Bizim uygulamamız** | **Tamamen ücretsiz** | **ACadSharp** | **%100 offline** | **%100 lokal** | **Yok** |

Bizim benzersiz değer önerimiz (Store listing'de vurgulanacak):
✅ Tamamen ücretsiz — reklam yok, abonelik yok, uygulama içi satın alma yok
✅ Tamamen offline — internet gerekmez
✅ Tamamen gizli — dosyalarınız cihazınızdan ASLA çıkmaz
✅ Açık kaynak bileşenler — güvenilir ve şeffaf teknoloji
✅ Royalty-free — sürdürülebilir, yarın ücretli olmayacak

Bu konumlanma, özellikle gizlilik ve maliyet hassasiyeti olan 
mühendislik firmaları için çok güçlü bir farklılaşmadır.
```

---

# 22. ORTA — SÜRDÜRÜLENEBILIRLIK NOTU

```
### Uzun Vadeli Sürdürülebilirlik (Yeni bölüm önerisi)

Uygulama kullanıcıya ücretsiz. CAD teknolojisi royalty-free.
Ancak uzun vadede bakım (güncelleme, yeni AutoCAD versiyonu 
desteği, bug fix) geliştirici zamanı gerektirir.

İleride düşünülebilecek modeller (şimdi karar verilmesi GEREKMİYOR):

1. Freemium: Ücretsiz viewer + ücretli pro özellikler
   - Ölçüm aracı (pro)
   - PDF export (pro)  
   - Edit özellikleri (pro)
   - Viewer hep ücretsiz kalır

2. İsteğe bağlı bağış / destek

3. Sponsorluk (mühendislik firmalarından)

4. Tamamen hobi projesi olarak bakım

ÖNEMLİ: Bu kararlar MVP'yi GECİKTİRMEMELİ.
İlk sürüm tamamen ücretsiz çıkmalı.
Monetizasyon Aşama 5+ sonrasına bırakılmalı.
```

---

# 23. DÜŞÜK AMA ÖNEMLİ — UYGULAMA İKONU VE MARKA

```
### İkon ve Marka Stratejisi (Bölüm 82'ye ek)

Uygulama adı kuralları (Plan zaten bunları belirtiyor):
- Özgün ad, DWG/Autodesk çağrıştırmayan
- DWG/DXF yalnız açıklamada format uyumluluğu olarak

İkon stratejisi (PLANDA EKSİK):
- AutoCAD veya DWG logosunu çağrıştırmamalı
- Autodesk trademark ihlali riski
- Önerilen semboller:
  - Geometrik: pergel, gönye, ızgara
  - Teknik: blueprint tarzı çizgi dokusu
  - Soyut: çizim katmanlarını temsil eden şekiller

Splash screen:
- Uygulama ikonu + uygulama adı
- Loading indicator (spinner veya progress)
- "Yükleniyor..." metni
- Arka plan koyu (CAD uygulaması hissi)
```

---

# 24. PLANIN AŞAMA YAPISINA ELEŞTİRİ VE İYİLEŞTİRME

Plan 6 aşamalı bir geliştirme yol haritası sunuyor (Bölüm 70-75). Yapı iyi ama bazı eksikler var:

```
### Aşama Yapısına Önerilen Düzeltmeler

Aşama 1 (Spike) — İYİ, ama eksik:
+ Eksik: SKCanvasView vs SKGLView A/B testi eklenmeli
+ Eksik: Release + AOT build performans testi eklenmeli
+ Eksik: Encoding (CP1254) testi eklenmeli

Aşama 2 (MVP) — İYİ, ama eksik:
+ Eksik: "Open With" (intent filter) eklenmeli
+ Eksik: Dark/light tema renk 7 çevirimi
+ Eksik: Crash recovery mekanizması
+ Eksik: Data Safety form hazırlığı
+ Eksik: Privacy policy sayfası

Aşama 3 (Doğruluk) — İYİ, ama öncelik sırası eksik:
  Şu sıra önerilir:
  1. Dimension anonim blok render (en görünür sorun)
  2. Nested block doğruluğu
  3. Hatch doğruluğu
  4. MTEXT rich formatting
  5. SHX font fallback iyileştirme
  6. Linetype pattern rendering
  7. Turkish text encoding doğrulaması
  8. Layout/viewport (zaman kalırsa)

Aşama 4 (Performans) — İYİ

Aşama 5 (Release) — EKSİK:
+ Eksik: Google Play Data Safety formu
+ Eksik: Privacy policy
+ Eksik: AAB build + app signing
+ Eksik: Store listing (screenshots, descriptions)
+ Eksik: İçerik derecelendirmesi (IARC)
+ Eksik: Keystore yedekleme stratejisi

Aşama 6 (Edit) — İYİ, erken başlanmaması doğru
```

---

# 25. BİLİNEN RİSKLER VE AZALTMA PLANLARI

```
### Kapsamlı Risk Tablosu (Yeni bölüm önerisi)

| # | Risk | Olasılık | Etki | Azaltma Stratejisi |
|---|---|---|---|---|
| R1 | ACadSharp belirli DWG'leri sessizce hatalı okur | Yüksek | Yüksek | onNotification logging, entity count doğrulama, geniş test corpus, kullanıcı uyarı mekanizması |
| R2 | ProCad Android'de production kalitesinde çalışmaz | Orta | Orta | PASS/FAIL spike kriterleri, fallback plan hazır |
| R3 | MAUI Android'de beklenmedik performans sorunları | Orta | Yüksek | Release+AOT testi, SKGLView threading kuralları, GC optimizasyonu |
| R4 | SHX font eksikliğinde metin okunmaz | Yüksek | Orta | TTF fallback zinciri, kullanıcı font import, width factor koruması |
| R5 | Büyük dosyalarda OOM crash | Yüksek | Yüksek | Dosya boyutu uyarısı, bellek bütçesi, largeHeap, parse timeout |
| R6 | Autodesk trademark ihlali iddiası | Düşük | Yüksek | Nötr uygulama adı/ikon, trademark guideline review, "DWG compatible" ifadesi dikkatli kullanım |
| R7 | ACadSharp projesinin terk edilmesi | Düşük | Çok Yüksek | Fork hazırlığı, community monitoring, sponsor desteği |
| R8 | Google Play policy değişikliği | Düşük | Orta | APK direct distribution yedek planı, F-Droid değerlendirmesi |
| R9 | SkiaSharp MAUI Android breaking change | Orta | Yüksek | Dependency pinning, regression testi, versiyon yükseltme ihtiyatlı |
| R10 | Dimension/Hatch render hataları | Yüksek | Orta | Aşamalı geliştirme, anonim blok stratejisi, golden reference |
| R11 | SOLID entity butterfly sıralaması hatası | Orta | Orta | P3/P4 sıralama kuralının bilinmesi, unit test |
| R12 | Eski DWG encoding sorunu | Yüksek | Orta | CP1254 registration, encoding test corpus |
| R13 | Trimming ACadSharp/ProCad'i kırar | Orta | Orta | Trimming testi, gerekirse TrimmerRootAssembly ekleme |
| R14 | Cold start çok yavaş (5+ saniye) | Orta | Düşük | AOT, lazy init, splash screen |
| R15 | Keystore kaybı (güncelleme yapılamaz) | Düşük | Çok Yüksek | Keystore yedekleme, Google Play App Signing |
```

---

# 26. PLANDA EKSİK: SceneBuilder ENTITY → PRIMITIVE DÖNÜŞÜM TABLOSU

Plan Bölüm 13-14'te SceneBuilder'ı tanımlıyor ama her entity'nin hangi render primitive'e dönüşeceği belirtilmiyor.

```
### Entity → RenderPrimitive Dönüşüm Tablosu (Bölüm 14'e ek)

| CAD Entity | Render Primitive | Skia Çizim |
|---|---|---|
| LINE | LinePrimitive | canvas.DrawLine() |
| ARC | ArcPrimitive | canvas.DrawArc() veya path.ArcTo() |
| CIRCLE | CirclePrimitive | canvas.DrawCircle() |
| ELLIPSE | EllipsePrimitive | canvas.DrawOval() (rotated) |
| LWPOLYLINE | PolylinePrimitive | path.MoveTo/LineTo/ArcTo |
| POLYLINE | PolylinePrimitive | path.MoveTo/LineTo/ArcTo |
| SPLINE | SplinePrimitive | path.CubicTo() (tessellated) |
| TEXT | TextPrimitive | canvas.DrawText() |
| MTEXT | MTextPrimitive | Multi-line canvas.DrawText() |
| INSERT | GroupPrimitive | canvas.Save/Translate/Rotate/Scale/Restore |
| HATCH | FillPrimitive | canvas.DrawPath(fillPaint) |
| DIMENSION | GroupPrimitive (*D blok) | Anonim bloğun entity'leri |
| SOLID | FillPrimitive | canvas.DrawPath(4-point, butterfly order) |
| POINT | PointPrimitive | canvas.DrawPoint() veya küçük çarpı |
| LEADER | PolylinePrimitive + TextPrimitive | path + text |
| MLEADER | GroupPrimitive | Karmaşık, Aşama 3 |
| TABLE | GroupPrimitive | LINE'lar + TEXT'ler, Aşama 3 |
| VIEWPORT | ClipPrimitive | canvas.ClipRect/ClipPath |
| IMAGE | RasterPrimitive | canvas.DrawImage() |
| WIPEOUT | FillPrimitive | Beyaz/arka plan dolgulu alan |

Bu tablo, renderer geliştirirken hangi entity'nin 
hangi Skia API'sine eşleneceğini hızlıca bulmayı sağlar.
```

---

# 27. PLANDA EKSİK: ByLayer/ByBlock RENK ÇÖZÜMLEME ZİNCİRİ

Plan Bölüm 26'da ByLayer ve ByBlock'u listeliyor ama çözümleme algoritması yok.

```
### ByLayer / ByBlock Renk Çözümleme Algoritması (Bölüm 26'ya ek)

Bir entity'nin gerçek ekran rengini belirlemek:

function ResolveColor(entity, parentInsert, document):
  
  entityColor = entity.Color
  
  if entityColor == ByLayer (256):
    // Entity kendi layer'ının rengini alır
    layer = document.Layers[entity.Layer]
    return layer.Color
  
  if entityColor == ByBlock (0):
    // Entity, kendisini içeren INSERT'ın rengini alır
    if parentInsert != null:
      return ResolveColor(parentInsert, parentInsert.Parent, document)
    else:
      // Block dışında ByBlock → genellikle beyaz/siyah (renk 7)
      return Color7_BasedOnBackground()
  
  // Doğrudan atanmış renk (ACI 1-255 veya True Color)
  return entityColor

DİKKAT:
- Nested block'larda ByBlock zinciri birden fazla seviye olabilir
- ByLayer, entity'nin kendi layer'ına bakar, INSERT'ın layer'ına DEĞİL
- ByBlock, INSERT'ın rengine bakar
- True Color (24-bit RGB) atanmışsa ByLayer/ByBlock geçersiz
- Frozen veya Off layer'daki entity'ler render EDİLMEZ

Bu mantık yanlış uygulanırsa:
- Tüm çizim tek renk görünebilir
- Block içindeki entity'ler yanlış renkte çıkar
- Layer rengini değiştirince block'lar etkilenmez (hata)
```

---

# 28. PLANDA EKSİK: MAUI PAGE LIFECYCLE VE CAD SESSION YAŞAM DÖNGÜSÜ

Plan Bölüm 60'ta CadSession'ı tanımlıyor ama MAUI Android lifecycle ile entegrasyonu yok.

```
### CadSession × MAUI Android Lifecycle (Bölüm 60'a ek)

Android lifecycle olayları ve CadSession etkileşimi:

OnResume (Uygulama ön plana geldi):
  - Skia canvas'ı yeniden invalidate et
  - Render cache'i kontrol et (dispose edilmiş olabilir)
  - Timer'ları yeniden başlat

OnPause (Uygulama arka plana gitti):
  - Render timer'larını durdur
  - Gereksiz CPU çalışmasını durdur
  - Durumu kaydet (son zoom/pan pozisyonu)

OnStop (Uygulama görünmez):
  - Render cache'in bir kısmını serbest bırak
  - Bellekte gereksiz bitmap tutma

OnDestroy (Uygulama kapanıyor):
  - CadSession.Dispose() çağır
  - Tüm Skia nesnelerini (SKPaint, SKPath, SKPicture) dispose et
  - Cache temizle
  - Temp dosyaları sil

Configuration Change (Ekran döndürme):
  - CadDocument ve RenderScene KORUNMALI (yeniden parse YASAK)
  - Yalnız Camera2D viewport yeni ekran boyutuna göre güncellenecek
  - SKCanvasView/SKGLView yeniden oluşturulabilir — sorun değil

Memory Warning (Sistem bellek baskısı):
  - Render cache'i agresif temizle
  - Block cache'i küçült
  - Hatch tessellation cache'i sil
  - Text shaping cache'i sil
  - Gerekirse: "Bellek yetersiz" uyarısı göster

Bu lifecycle entegrasyonu doğru yapılmazsa:
- Ekran döndürmede çizim kaybolur
- Arka plandan dönüşte siyah ekran
- Bellek sızıntısı (her rotate'te yeni parse)
- Sistem bellek baskısında OOM crash
```

---

# 29. CHATGPT'YE VERİLECEK DİREKTİF

Aşağıdaki metni ChatGPT'ye planın güncellenmesi için ver:

```
Bu belge Claude Opus'un Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan.md
üzerine yaptığı genişletilmiş V2.0 bağımsız teknik denetim raporudur.

Lütfen bu önerileri ana plana entegre et. Öncelik sırası:

KRİTİK (mutlaka eklenmeli):
1. ACadSharp alpha durumu + silent entity drop — onNotification zorunluluğu (Bölüm 6'ya)
2. ProCad spike PASS/FAIL tablosu (Bölüm 70'e)
3. MAUI Android performans kuralları: Release mode, AOT, SKCanvasView vs SKGLView, GC (Bölüm 11'e)
4. SKCanvasView vs SKGLView seçim stratejisi (Bölüm 12'ye)
5. DXF fallback stratejisi (yeni bölüm)

ÖNEMLİ (güçlü önerilir):
6. Code page 1254 encoding kaydı (Bölüm 22'ye)
7. Somut bellek bütçesi modeli (Bölüm 39'a)
8. DIMENSION rendering stratejisi — anonim blok vs hesaplama (Bölüm 25'e)
9. Linetype pattern rendering stratejisi (yeni bölüm)
10. SOLID entity butterfly sıralaması (Bölüm 20'ye)
11. Open With MVP'ye alınmalı (Bölüm 31 → Bölüm 71)
12. Dark/light tema renk 7 çevrimi (Bölüm 26'ya)
13. ByLayer/ByBlock renk çözümleme algoritması (Bölüm 26'ya)
14. Crash recovery mekanizması (yeni bölüm)
15. Google Play Data Safety + signing detayları (Bölüm 43'e)
16. MAUI lifecycle × CadSession entegrasyonu (Bölüm 60'a)

ORTA (önerilir):
17. SHX font somut fallback zinciri (Bölüm 23'e)
18. Test corpus temin ve otomasyon stratejisi (Bölüm 49'a)
19. APK boyutu bütçesi (yeni bölüm)
20. Entity → RenderPrimitive dönüşüm tablosu (Bölüm 14'e)
21. Aşama yapısına düzeltmeler (Bölüm 70-75)
22. Bilinen riskler tablosu (yeni bölüm)
23. Rekabet analizi (yeni bölüm)
24. Erişilebilirlik minimum (yeni bölüm)
25. Uygulama ikonu stratejisi (Bölüm 82'ye)
26. ACadSharp ekosistemi (yeni bölüm)
27. Sürdürülebilirlik notu (yeni bölüm)

Bu öneriler planın ana felsefesini DEĞİŞTİRMİYOR.
Permissive lisans, preview-first, offline-first korunuyor.
Öneriler planın pratik uygulanabilirliğini ve risk yönetimini güçlendiriyor.
```

---

# 30. ÖZET: PLANIN GÜÇLÜ VE ZAYIF YANLARI

## ✅ Güçlü yanlar (KORU, değiştirme)
- Lisans felsefesi kusursuz, disiplinli ve tutarlı
- Teknoloji seçimi (ACadSharp + SkiaSharp + MAUI) isabetli
- Preview-first stratejisi doğru ve disiplinli
- Fallback planı var (ProCad başarısız olursa)
- Android-first yaklaşımı pratik ve gerçekçi
- Lisans firewall mekanizması (Bölüm 52) çok değerli
- "Masaüstü CAD'i telefona sıkıştırma" uyarısı (Bölüm 64) önemli
- Error taxonomy (Bölüm 47) iyi tasarlanmış
- Kullanıcı hata mesajları (Bölüm 48) kullanıcı dostu

## ⚠️ Güçlendirmesi gereken yanlar
- ACadSharp alpha gerçeği kabul edilip savunma mekanizmaları eklenmeli
- MAUI Android performans bilgisi (AOT, SKView seçimi, GC) eklenmeli
- Bellek/performans bütçeleri somut sayılarla desteklenmeli
- Encoding, linetype, dimension, SOLID gibi teknik detaylar derinleştirilmeli
- ByLayer/ByBlock renk çözümleme algoritması eklenmeli
- ProCad spike kriterleri somutlaştırılmalı

## ❌ Eksik olan yanlar (YENİ bölüm olarak eklenmeli)
- Risk yönetimi tablosu
- Crash recovery stratejisi
- Rekabet analizi
- APK boyutu bütçesi
- Open With'in MVP'ye alınması
- Erişilebilirlik minimum gereksinimleri
- Google Play Data Safety ve signing detayları
- Entity → RenderPrimitive dönüşüm tablosu
- MAUI lifecycle × CadSession entegrasyonu
- SKCanvasView vs SKGLView seçim stratejisi
- Sürdürülebilirlik notu

---

## Son söz

> Bu plan **%85 oranında çok sağlam** bir belge. Yukarıdaki 30 başlıktaki öneriler 
> planı **%85'ten %97+'ye** taşımak için. Kalan %3 ancak gerçek Android cihazda 
> ilk DWG açıldığında ortaya çıkacak ve deneyimle kapatılacak.
> 
> Planın en büyük gücü: **ne yapılacağını** çok iyi bilmesi.
> 
> Bu raporun katkısı: **nasıl yapılacağını** ve **nelerin ters gidebileceğini** 
> somutlaştırmak.
