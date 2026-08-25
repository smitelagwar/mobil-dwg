# ChatGPT Sohbeti → GitHub → Yerel Android Emulator Test İş Akışı

> **OKUMA KURALI:** Bu dosya, yalnız çalışma bağlamı aşağıdaki `CHATGPT_REMOTE_GITHUB` tanımına uyuyorsa okunması zorunlu bir yürütme bağlamı belgesidir. Dosyanın okunması zorunludur; içindeki batching, test zamanlaması ve 25 dakikalık zaman kullanım önerileri **zorunlu yürütme kuralları değildir**. Ajan gerçek ihtiyaca göre farklı bir sıra seçebilir.

Aktif AŞAMA 01–09 tekrar turu ve iki-cursor/offline kuralları `ANDROID_DOGRULAMA_PLANI.md` içindedir. Bu dosya o planın taşıma ve test yorumlama biçimini açıklar.

## 1. Bu belge ne zaman geçerli?

Ajan önce içinde çalıştığı ortamı araç erişiminden ve gerçek çalışma biçiminden belirler. Kullanıcıya gereksiz soru sormaz.

### `CHATGPT_REMOTE_GITHUB` — bu belge GEÇERLİ ve OKUNMASI ZORUNLU

Aşağıdakilerin özü birlikte varsa bu bağlam seçilir:

- İş ChatGPT'nin sohbet arayüzünde yürütülüyor.
- Kod/depo değişiklikleri kullanıcının bilgisayarındaki çalışma ağacına doğrudan yazılmıyor; GitHub connector/API üzerinden repoya yapılıyor.
- Ajanın kullanıcının yerel Windows terminaline, Android Emulator'ına veya ADB'sine doğrudan sürekli erişimi yok.
- Yerel Android testleri gerektiğinde GitHub üzerinden kullanıcının self-hosted Windows runner'ına gönderiliyor.

Bu durumda bu dosyayı oku ve aşağıdaki test altyapısının mevcut olduğunu hesaba kat.

### `LOCAL_IDE` — bu belge yürütme için GEÇERSİZ

Aşağıdakilerden biri gerçek çalışma yoluysa bu belge test yürütme modeli olarak uygulanmaz:

- AntiGravity içinde yerel repo + terminal erişimiyle çalışma,
- Visual Studio içindeki Codex eklentisinin yerel çalışma ağacında işlem yapması,
- Codex'in kendi IDE/masaüstü ortamında yerel repo ve terminalle çalışma,
- başka bir yerel ajan/IDE'nin aynı bilgisayarda repo, `dotnet`, emulator ve `adb` komutlarına doğrudan erişmesi.

Bu durumda ajan GitHub round-trip yapmak zorunda değildir; yerel build/test araçlarını doğrudan kullanabilir. Bu dosyanın batching/tetikleme önerilerini zorunlu sayma.

### Karma/şüpheli durum

Kararı ürün adına veya ajan adına göre değil, **gerçek dosya değiştirme ve test çalıştırma yoluna göre** ver:

- Kod GitHub üzerinden değiştiriliyor ve Android testi GitHub → self-hosted runner üzerinden gidiyorsa: `CHATGPT_REMOTE_GITHUB`.
- Kod yerel çalışma ağacında değiştiriliyor ve yerel terminal/ADB doğrudan kullanılabiliyorsa: `LOCAL_IDE`.

Bağlam açıkça tespit edilebiliyorsa kullanıcıya sorma. Gerçekten ayırt edilemiyorsa kısa bir netleştirme iste.

## 2. Kurulu Android test altyapısı

Repo, ChatGPT sohbetinden yapılan GitHub değişikliklerini gerektiğinde kullanıcının Windows bilgisayarındaki Android Emulator üzerinde sınamak için ayrı bir isteğe bağlı test hattına sahiptir.

İlgili parçalar:

- Workflow: `.github/workflows/android-emulator-test.yml`
- Yerel gate: `scripts/android-emulator-gate.ps1`
- Self-hosted runner etiketleri: `self-hosted`, `windows`, `android-test`, `mobil-dwg`
- AVD: `mobil-dwg-api36`
- Android: API 36 / Android 16
- Legacy `.github/workflows/android-emulator-test.yml` normal `main`/feature push'unda çalışmaz; yalnız `android-test` push veya manual dispatch kullanır. Ayrı V02/V03 audit workflow'ları kendi dependency/fixture filtreleri eşleştiğinde `main`/PR üzerinde, PR `#17` ile gelen V04 workflow'u ise ilgili app/Core/Cad/Rendering/architecture yolları değişen PR `opened/synchronize/reopened` olayında self-hosted bilgisayarı kullanabilir.
- Test hattı `android-test` branch'ine push ile veya mevcut workflow'un manuel dispatch yolu ile çalışabilir.
- Workflow diagnostik artifact olarak özet, logcat, meminfo, cihaz bilgisi ve screenshot çıktıları yükleyebilir.

V01 altyapı baseline'ının ve tamamlanan V04'ün sınırları:

- V04 öncesinde `src/MobilDwg.App` installable değildi; V04 ile Android-only MAUI executable'a dönüştürüldü.
- Legacy V01 gate geçici `Stage01Smoke` APK üretip kurar.
- Gate solution build sonrasında executable Core/Rendering/Architecture harness'larını açıkça `dotnet run` ile çalıştırır ve zorunlu marker'ları doğrular.
- Screenshot byte-safe alınır, tam PNG imzası doğrulanır; numeric PID zorunludur ve package/PID crash ile post-launch ANR kontrol edilir.
- V01 exact koşusu bu altyapı kanıtlarını `VALIDATED — INFRASTRUCTURE_SMOKE_ONLY` olarak kapattı. Stage01Smoke yine gerçek app/viewer sonucu değildir.
- V04 gerçek `MobilDwg.App` build/install/cold-launch/UI/stability gate'ini geçip PR `#17` ile `main`e merge edildi ve claim-limited `VALIDATED` oldu. Aktif validation cursor V05'tir.

Bu altyapının gerçek güncel durumu gerektiğinde repo/workflow/run sonuçlarından doğrulanır; bu belge geçmiş bir PASS sonucunu gelecekte otomatik PASS saydırmaz.

## 3. Kritik ayrım: her dosya değişikliğinde test tetikleme

**Her GitHub dosya değişikliğinde, her commit'te veya her küçük düzeltmede yerel Android runner'ı tetiklemek zorunlu değildir ve varsayılan amaç bu değildir.**

Örneğin ajan bir mantıksal iş sırasında üç dosyayı değiştirebilir:

```text
Dosya A değişti
Dosya B değişti
Dosya C değişti
        ↓
Anlamlı bir test noktası oluştu
        ↓
Android test hattı bir kez tetiklendi
```

Aynı şekilde ajan beş küçük değişikliği tek test döngüsünde doğrulayabilir. Bunun tersine, riskli bir değişiklikte tek dosyadan sonra test etmek de doğru olabilir.

Test tetikleme sıklığı **ajanın teknik muhakemesine bırakılmıştır**. Amaç, gereksiz GitHub → PC → emulator → artifact round-trip'lerinden kaçınırken hatayı çok geç yakalamamaktır.

### Runner çevrim dışıysa

PC'nin açık olması tek başına yeterli değildir; interaktif Windows oturumunda `C:\actions-runner\run.cmd` dinliyor olmalıdır. Runner hazır değilse:

1. Yeni emulator workflow'larını art arda kuyruğa sokma.
2. Exact test SHA'sını, gerekli gate/configuration ve beklenen marker'ı `PENDING_EMULATOR_QUEUE` olarak kaydet.
3. Aynı ihtiyacı daha yeni bir SHA karşılıyorsa eski bekleyen kaydı superseded olarak kapat.
4. Validation hattında güvenli kod inceleme/hosted test işini tamamla. Zamanı A10 ile değerlendirmek isteniyorsa aynı sohbeti yön değiştirmek yerine ayrı sohbette `BASLA_A10.md dosyasını oku` komutunu kullan.
5. Runner dönünce en eski hâlâ geçerli riskli checkpoint'i çalıştır; kanıt gelmeden PASS yazma.

## 4. 25 dakikalık High çalışma süresini verimli kullanma önerisi

Kullanıcı ChatGPT High modunda yaklaşık 25 dakikalık aktif çalışma pencereleriyle ilerleyebilir. Bu yüzden remote GitHub bağlamında ağır Android testini her küçük edit sonrası çağırmak zaman kaybına dönüşebilir.

Önerilen fakat zorunlu olmayan yaklaşım:

1. Önce ilgili kodu/repo durumunu incele.
2. Aynı mantıksal alt iş için gerekli değişikliklerin tamamını veya anlamlı bir bölümünü GitHub'da yap.
3. Hızlı/uzak CI veya statik kontroller yeterliyse önce onları kullan.
4. Android ortamında gerçek doğrulama anlamlı hale geldiğinde `android-test` hattını tetikle.
5. Workflow sonucunu, logları ve artifact'leri incele.
6. Hata varsa düzelt ve yalnız gerektiğinde yeniden tetikle.

Ajan isterse daha erken veya daha geç test edebilir. Bu bölüm performans/iş akışı tavsiyesidir; Definition of Done veya aşama çıkış kriterlerini değiştirmez.

## 5. `devam` ve aşamalı çalışma ile ilişkisi

V01–V09 açıkken birinci cursor `ANDROID_DOGRULAMA_PLANI.md` içindeki doğrulamadır; genel `BASLA.md` bu hattı yürütür. Implementation cursor AŞAMA 10'da ayrıca korunur. Runner çevrim dışıyken host-independent A10 işi ancak ayrı `BASLA_A10.md` sohbetinde, `stage10-p0-geometry-draft` branch'inde yürütülür. Bir turda iki cursor birden kapatılmaz ve Android test borcu beta/release'e taşınmaz.

A10 paralel hattının zorunlu sınırı:

- A10 branch'i normal feature branch'tir; `android-test` değildir.
- PC/runner kapalıyken önce workflow path filtreleri okunur. Açık A10 PR'ı yoksa normal branch commit/push ile sürülebilir. A10 PR'ı zaten açıksa branch push'u PR `synchronize` olayıdır ve V04 workflow'u main'e girdikten sonra Core/Rendering değişikliklerinde self-hosted emulator işi açabilir; offline push öncesi PR kapatılır/etki güvenle gate edilir, aksi halde push yapılmaz.
- Runner/test ortamı hazır olduğunda draft PR açılır/güncellenir; GitHub-hosted ve tetiklenen self-hosted kontrollerin actual non-zero-step sonucu doğrulanır. Billing/spending/capacity nedeniyle başlamayan hosted job PASS değildir.
- Runner kapalıyken `.csproj`, dependency/compliance veya fixture/provenance kapsamına girilmez; bunlar V02/V03 self-hosted audit job'ı oluşturabilir.
- Host/GitHub-hosted kontrol sonuçsuzsa `CODED_PENDING_HOST_TESTS`, actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`, hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması + Android gate bekleyen `CODED_PENDING_EMULATOR`dır. `main` merge, `READY_TO_MERGE`, `DONE` ve A11 yasaktır.
- A10 sohbeti validation checkpoint/evidence dosyalarını ve `android-test` branch'ini değiştirmez; branch/SHA/test borcunu `docs/A10_WORKSTREAM.md` içinde tutar.
- V09 kapandıktan sonra güncel validated `main` A10 branch'ine alınır ve exact integration SHA tüm gerekli host/emulator gate'lerinden geçirilir.

Projenin canonical aşama kuralı değişmez:

- Aynı kullanıcı turunda en fazla bir aşama tamamlanır.
- Aktif aşama tamamlanmadan sonraki aşamaya geçilmez.
- Bir aşama tek High turunda bitmek zorunda değildir.
- Aşama bitmediyse kullanıcı `devam` dediğinde aynı aşamadan devam edilir.
- Bu, aşama gerçek çıkış kriterleri sağlanana kadar tekrarlanabilir.

Örnek:

```text
AŞAMA 05 başlatıldı
  ↓
1. tur: kodun bir kısmı / değişikliklerin çoğu yapıldı
  ↓
AŞAMA 05 hâlâ IN_PROGRESS
  ↓
kullanıcı: devam
  ↓
2. tur: kalan implementasyon + hedefli test
  ↓
hâlâ eksikse IN_PROGRESS
  ↓
kullanıcı: devam
  ↓
3. tur: Android gate / regresyon / evidence
  ↓
gerçek çıkış kriterleri sağlanır
  ↓
AŞAMA 05 DONE
```

Ajan aynı aşamanın iç işlerini farklı biçimde bölebilir. Örneğin ilk turda değişiklik + test birlikte yapılabilir veya ilk tur yalnız implementasyona ayrılabilir. Bu esneklik V04→V09 aşama sırasını, A10 merge kapısını veya A11 kilidini kaldırmaz. Önemli olan aktif aşamayı atlamamak, sahte `DONE` yazmamak ve gerçek kanıt toplamaktır.

## 6. Remote Android testini ne zaman düşünmeli?

Aşağıdaki durumlarda tetikleme özellikle değerlidir:

- MAUI/Android uygulama kabuğu veya lifecycle değiştiyse,
- Android'e özel dosya alma/URI/permission davranışı değiştiyse,
- renderer veya UI davranışının Android runtime'da doğrulanması gerekiyorsa,
- install/launch/crash/ANR riski olan değişiklik yapıldıysa,
- bir aşamanın çıkış kriteri Android emulator/device kanıtı istiyorsa,
- anlamlı bir değişiklik grubu tamamlanıp checkpoint'e gelindiyse.

Saf dokümantasyon, yalnız host-independent Core kodu veya zaten yeterli unit/architecture testleriyle kapanabilen küçük değişikliklerde emulator testi gereksiz olabilir.

## 7. Tetikleme mantığı

Remote ChatGPT bağlamında normal geliştirme branch'lerinin bilgisayarı gereksiz yere çalıştırmaması esastır.

Legacy `android-emulator-test.yml` tetikleyicileri:

- `main` push: Android emulator gate'ini **tetiklemez**.
- normal feature branch push: Android emulator gate'ini **tetiklemez**.
- `android-test` branch push: Android emulator workflow'unu tetikler.
- `workflow_dispatch`: kullanıcı/uygun araç üzerinden manuel test yolu olarak bulunabilir.
- Yalnız Markdown değişikliklerinden oluşan `android-test` push'u ağır gate'i tetiklemez.
- Aynı repo için workflow concurrency yalnız bir aktif ve en güncel bekleyen checkpoint'i tutarak stale iş birikimini sınırlar; çalışan emulator işi yarıda kesilmez.

Normal “GitHub ile senkronize et” işlemi yalnız `main`i günceller; `android-test` branch'i her senkronizasyonda oynatılmaz. Bu branch yalnız anlamlı emulator checkpoint'i içindir. Dokümantasyon-only değişikliklerin ağır gate'i tetiklemesi gerekmez.

Bu izolasyon repo içindeki bütün self-hosted işleri kapsayan genel bir iddia değildir. V02 dependency ve V03 corpus workflow'ları kendi path filtreleri eşleştiğinde `main`/PR üzerinde self-hosted Windows runner ister. PR `#17` ile gelen `android-v04-validation.yml` de app/Core/Cad/Rendering/architecture yollarına dokunan açık PR push'unu `pull_request:synchronize` olarak görüp self-hosted emulator çalıştırabilir. A10 offline branch/PR etkisi işe başlamadan kontrol edilir.

Ajan test istediğinde, test edilecek **tam commit SHA** belirgin olmalıdır. `android-test` branch'i yalnız test taşıyıcısıdır; normal geliştirme branch'i olarak kullanılmaz.

Branch/ref güncellemesi destructive biçimde yapılmaz. Test edilecek commit ile branch geçmişi fast-forward uyumlu değilse zorla ref hareketi yapmak yerine güvenli bir tetikleme yolu seç veya durumu açıkça değerlendir.

Bir feature head `android-test` taşıyıcısıyla test edildiyse PR varsayılan olarak **merge commit** yöntemiyle birleştirilir. Squash/rebase tested head'i `main` ancestry'sinden çıkarıp sonraki `force:false` fast-forward taşıyıcı güncellemesini bozabilir. Merge commit kullanılamıyorsa force uygulanmaz; exact-ref manual dispatch veya güvenli eşdeğer tetikleme kullanılır.

Self-hosted runner yalnız repo sahibinin kontrol ettiği commit'i çalıştırır. Workflow read-only contents izni ve credentials persist etmeyen checkout kullanır; üçüncü taraf PR/ref'i Windows kullanıcısı üzerinde çalıştırılmaz.

## 8. Workflow sonucunu nasıl yorumlamalı?

Android workflow'un `SUCCESS` olması yalnız gerçekten çalıştırdığı gate'lerin geçtiğini kanıtlar.

Özellikle mevcut `scripts/android-emulator-gate.ps1` sürümünün neyi build/install ettiğini çalıştırma anında oku. V01-hardened sürüm solution build'i ve executable harness marker'larını gerçekten çalıştırır; Android install/launch smoke için geçici temiz `Stage01Smoke` MAUI uygulaması üretir. Bu nedenle:

- `runner PASS`, `emulator PASS`, `build/install/launch PASS` altyapının çalıştığını kanıtlayabilir;
- fakat gerçek `MobilDwg.App` viewer APK'sı test edilmediyse bunu **gerçek viewer işlevi PASS** diye yorumlama.

İlgili `dotnet run --project ...` marker'ları görülmeden “solution tests passed” iddiası kabul edilmez. V01-hardened gate bu marker'ları, PNG magic bytes'ı, numeric PID'yi ve package/PID crash + post-launch ANR koşullarını zorunlu kılar. Gelecekte script değişirse exact tested revision tekrar okunur; eski V01 sonucu yeni sürüme otomatik taşınmaz.

Proje ilerledikçe gate gerçek `MobilDwg.App` artifact'ini kuracak şekilde geliştirilirse o günkü script/workflow içeriği esas alınır.

Her testte mümkün olduğunda şu kanıtları kontrol et:

- workflow run sonucu,
- job/step sonucu,
- test edilen commit SHA,
- `ANDROID_EMULATOR_GATE_PASS` veya ilgili başarısızlık marker'ı,
- logcat/crash/ANR bulguları,
- screenshot,
- meminfo/performance çıktısı gerekiyorsa,
- artifact upload sonucu.

Artifact veya log eksikse sonucu gereğinden güçlü yorumlama.

## 9. Yerel IDE bağlamına geçilirse

Ajan çalışma sırasında artık yerel repo + terminal + ADB/emulator erişimi kazandıysa, sonraki iş için bağlamı `LOCAL_IDE` olarak yeniden sınıflandırabilir.

Bu durumda:

- `android-test` branch round-trip zorunlu değildir,
- doğrudan `scripts/android-emulator-gate.ps1` veya daha hedefli yerel komutlar çalıştırılabilir,
- GitHub yalnız kaynak kontrolü/CI/handoff amacıyla kullanılabilir.

Bu belgeyi okuyup bilmek hata değildir; yalnız remote GitHub test modelini yerel çalışmaya zorla uygulama.

## 10. Bu belgenin normatif gücü

Bu dosyanın **okunması**, `CHATGPT_REMOTE_GITHUB` bağlamında zorunludur.

Aşağıdakiler zorunlu değildir:

- tam olarak kaç dosyadan sonra test yapılacağı,
- tam olarak kaç dakikanın implementasyona veya teste ayrılacağı,
- bir High turunda önce kod mu test mi yapılacağı,
- her turda Android gate çalıştırılması,
- örneklerdeki batching sırası.

Bunlar ajan için verimlilik bilgisi ve kullanılabilir altyapı bilgisidir.

Zorunlu proje kuralları hâlâ canonical plan, gerçek repo durumu, aktif stage checkpoint'i, evidence/ADR kayıtları ve kullanıcı tarafından açıkça verilmiş yürütme kararlarından gelir.

## 11. Kısa karar özeti

```text
Yerel IDE + yerel terminal/ADB var mı?
  EVET → LOCAL_IDE → bu remote test modeli uygulanmaz.
  HAYIR
    ↓
ChatGPT sohbeti GitHub üzerinden repo değiştiriyor mu?
  EVET → CHATGPT_REMOTE_GITHUB
           ↓
         BU DOSYAYI OKU
           ↓
         PC + interaktif runner hazır mı?
           EVET → BASLA.md validation hattı → açık VXX'i sırayla yürüt
                    ↓
                  Anlamlı exact SHA'da Android gate ve artifact kanıtı
           HAYIR → validation SHA'sını gerekiyorsa pending kaydet
                    ↓
                  Ayrı sohbet + BASLA_A10.md
                    ↓
                  Yalnız A10 branch commit/push; offline iken self-hosted iş açacak PR yok
                    ↓
                  En fazla CODED_PENDING_HOST_TESTS; main merge/DONE/A11 yok
  HAYIR → mevcut gerçek çalışma ortamına göre hareket et.
```
