# mobil-dwg — Android Geriye Dönük Doğrulama Planı

Bu belge, AŞAMA 01–09 arasında emülatör kullanılmadan geliştirilen kodu sırayla incelemek, eksikleri düzeltmek ve kullanıcının Windows bilgisayarındaki Android Emulator üzerinde gerçek kanıt toplamak için yetkili alt plandır.

Ana ürün ve teknik plan `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyasıdır. Bu belge yalnız aktif **Android doğrulama turunun** sırasını, çevrim içi/çevrim dışı çalışma biçimini ve kanıt kurallarını yönetir. Tarihsel `docs/evidence/STAGE_XX.md` kayıtları geriye dönük değiştirilmez; yeni sonuçlar ayrı Android doğrulama kanıtı olarak eklenir.

## 1. Aktif karar

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION — aktif kapsam, aktif DoD ve sıradaki işlerden çıkarıldı
IMPLEMENTATION_BASELINE: AŞAMA 01–09 kodlandı; AŞAMA 09 tamamlandı
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_VALIDATION_STAGE: V01
CURRENT_STATUS: FIX_REQUIRED
NEXT_ACTION: V01 — executable harness, gerçek kapsam, byte-safe screenshot ve PID/crash/ANR kanıt açıklarını düzelt; ardından exact emulator Release koşusunu al
AFTER_V09: test borcunu temizle ve implementation cursor'ında Android-first tek hatta devam et (başlangıçta AŞAMA 10)
PENDING_EMULATOR_QUEUE: EMPTY
```

iOS kodu veya geçmiş kanıtı silinmez. Shared Core, parser ve renderer katmanları platformdan bağımsız tutulur; Android içine gereksiz platform bağımlılığı sızdırılmaz. Ancak kullanıcı açıkça iOS yolunu yeniden etkinleştirene kadar Mac, Xcode, iPhone, iOS workload, iOS CI, signing, simulator ve App Store işi yapılmaz; bunlar Android aşamalarını bloke etmez.

## 2. `BASLA.md` ve `devam` davranışı

Kullanıcı yeni sohbette yalnız `BASLA.md dosyasını oku` dediğinde ajan:

1. Gerçek `main` HEAD ve açık işi doğrular.
2. `BASLA.md`, bu dosya, `gecmis.md`, canonical plan ve gerçek çalışma bağlamına uygun Android test iş akışını okur.
3. `CURRENT_VALIDATION_STAGE` tamamlanmadıysa doğrudan o doğrulama aşamasına başlar.
4. Yalnız özet verip durmaz. Normal durumda V01–V09 önceliklidir; runner çevrim dışıyken açık VXX için güvenli iş kalmadıysa ayrı implementation cursor'ındaki host-independent kod çalışmasına devam edebilir.
5. V01–V09 tamamlanınca Android test borcu temizlenir ve normal plan AŞAMA 10+ cursor'ında tek hatta devam eder.

Kullanıcı `devam` dediğinde açık doğrulama aşaması sürdürülür. Bir turda en fazla bir doğrulama aşaması veya bir normal implementation aşaması kapatılır; aynı turda iki cursor birden kapatılmaz.

## 3. Gerçeklik sınıfları

Aşağıdaki kanıtlar birbirinin yerine geçmez:

| Kanıt | Neyi kanıtlar | Neyi kanıtlamaz |
|---|---|---|
| Restore/build/unit/architecture testi | Host üzerinde kod ve sözleşme doğruluğu | Android install, lifecycle veya UI davranışı |
| Geçici `Stage01Smoke` APK install/launch | Runner, SDK, emulator, ADB ve MAUI paketleme altyapısı | Gerçek `MobilDwg.App` viewer APK'sı veya DWG/DXF işlevi |
| Gerçek `MobilDwg.App` APK emulator koşusu | Test edilen commit'teki Android uygulama akışı | Fiziksel telefon üretici/SAF/performance çeşitliliği |
| Fiziksel Android cihaz koşusu | Kaydedilen cihaz ve senaryo | Test edilmemiş başka cihazlar veya iOS |

Mevcut `scripts/android-emulator-gate.ps1`, çözüm testlerini çalıştırdıktan sonra geçici `Stage01Smoke` MAUI uygulaması üretip kurmaktadır. Script gerçek `MobilDwg.App` artifact'ine geçirilene kadar `ANDROID_EMULATOR_GATE_PASS`, yalnız emulator altyapı smoke kanıtıdır.

## 4. Durum modeli

- `NOT_STARTED`: doğrulama başlamadı.
- `CODE_AUDIT`: mevcut kod/evidence ile aşama iddiaları karşılaştırılıyor.
- `FIX_REQUIRED`: doğrulamaya başlamadan önce kapatılması gereken somut kod/kanıt açığı bulundu.
- `FIX_IN_PROGRESS`: bulunan eksik veya hata düzeltiliyor.
- `READY_FOR_EMULATOR`: kod ve host testleri hazır; exact commit Android koşusunu bekliyor.
- `WAITING_RUNNER`: kullanıcı bilgisayarı/runner çevrim dışı; test çalışmadı.
- `VALIDATED`: bu aşama için tanımlı gerekli kanıtlar geçti; emulator yalnız o aşamada gerekli olarak işaretlendiyse zorunludur.
- `VALIDATED_WITH_DEFERRED_PHYSICAL`: emulator kapsamı geçti; fiziksel cihaz farkı sonraki gerçek-cihaz kapısında açık.
- `SCOPE_ARCHIVED`: iOS'a ait tarihsel iş korundu fakat aktif Android turunda çalıştırılmayacak.
- `DEFERRED_PHYSICAL_ANDROID`: emulator kapsamı dışında kalan fiziksel cihaz farkı release matrisine taşındı.
- `BLOCKED`: kod, test veya gerçek dış bağımlılık nedeniyle güvenli ilerleme mümkün değil.

`READY_FOR_EMULATOR` ve `WAITING_RUNNER`, `PASS` veya `VALIDATED` değildir.

## 5. Bilgisayar/runner çevrim içi değilken çalışma

Kullanıcının bilgisayarının kapalı olması veya self-hosted runner'ın `Listening for Jobs` durumunda olmaması kod yazmayı durdurmaz.

1. Ajan kod incelemesi, eksik testlerin yazılması, host kontrolleri ve güvenli düzeltmelere devam eder.
2. Çalıştırılması gereken exact commit SHA, workflow/script, konfigürasyon ve beklenen marker `PENDING_EMULATOR_QUEUE` kaydına eklenir.
3. Test sonucu uydurulmaz; aşama `READY_FOR_EMULATOR` veya `WAITING_RUNNER` kalır.
4. Runner döndüğünde en eski riskli kayıt önce çalıştırılır; sonucu geçen commit ile ilişkilendirilir.
5. Android UI, lifecycle, permission, packaging veya native runtime değişiklikleri birikiyorsa en eski emulator checkpoint'i görülmeden yeni yüksek-riskli runtime değişikliği yığılmaz. Platformdan bağımsız Core, dokümantasyon ve test-harness işi devam edebilir.
6. Kuyruk temizlenmeden beta, release veya ilgili runtime aşaması `DONE` yapılmaz.

Bu iki-cursor modeli “bilgisayar kapalıysa hiçbir iş yapma” anlamına gelmez: doğrulama cursor'ı kanıt beklerken implementation cursor'ı güvenli alanda ilerleyebilir. Buna karşılık bir sonraki emülatör sonucunun hangi kodu sınadığı exact SHA ile ayrıştırılır.

Runner çevrim dışıyken aynı başarısız/queued workflow tekrar tekrar tetiklenmez. Bu durum implementation failure sayılmaz; `WAITING_RUNNER` olarak kaydedilir.

## 6. GitHub → yerel emulator taşıma kuralı

- Normal geliştirme ve yetkili kaynak `main` veya ilgili feature branch'tir.
- `android-test` yalnız test taşıyıcı branch'idir; üzerinde doğrudan ürün geliştirilmez.
- Test edilen exact commit SHA kanıtta yazılır.
- `android-test` push veya güvenli `workflow_dispatch` dışında normal `main` push kullanıcının bilgisayarını tetiklemez.
- Taşıyıcı branch güncellemesi fast-forward güvenli değilse force-push yapılmaz; güvenli dispatch/branch yolu seçilir.
- Workflow `SUCCESS` sonucu, yalnız o sürümde gerçekten koşan script ve adımlar kadar güçlüdür.

## 7. V01–V09 doğrulama sırası

### V01 — Toolchain, runner ve emulator altyapısı — `FIX_REQUIRED`

Amaç: AŞAMA 01 iddialarını mevcut bilgisayarda yeniden okumak ve emulator köprüsünün gerçek sınırını kanıtlamak.

- Exact `main` SHA ve temiz çalışma ağacı kaydedilir.
- `.NET 10.0.400`, `maui-android`, Java 21.0.12, Android API 36, Build-Tools 36.0.0, ADB 37.0.1 ve `mobil-dwg-api36` kontrol edilir.
- `doctor-local-environment.ps1` çalıştırılır; ardından gate'in solution test projelerini gerçekten **yürüttüğü** doğrulanır. Bu repodaki test projeleri özel executable harness'lardır; yalnız `dotnet test MobilDwg.sln` kullanmak marker gövdelerini çalıştırmaz. İlgili `dotnet run --project ...` harness komutları gate'e eklenmeden test PASS yazılmaz.
- `adb exec-out screencap` çıktısı byte-safe biçimde kaydedilir ve PNG magic bytes `89 50 4E 47` doğrulanır; bozuk UTF-16/redirect çıktısı screenshot kanıtı sayılmaz.
- Launcher PID bulunamıyorsa koşu fail olur. Crash kontrolü multiline logcat'i kapsar; ANR iddiası gerçek `dumpsys activity`/dropbox veya eşdeğer kanıt olmadan yazılmaz.
- Bu açıklar düzeltildikten sonra emulator gate'in güvenli bir Release koşusu alınır.
- Run/job/artifact, screenshot, logcat ve `ANDROID_EMULATOR_GATE_PASS` incelenir.
- Kurulan APK'nın geçici `Stage01Smoke` olduğu açıkça kaydedilir; gerçek viewer PASS yazılmaz.
- Tarihsel fiziksel telefon kapısı aktif geliştirmeyi durdurmaz; release öncesi gerçek Android matrisi için açık kalır.

Çıkış: GitHub → self-hosted Windows → emulator hattı exact commit ile çalışır ve geçerli artifact/marker üretir. Runner çevrim dışıysa bu çıkış sağlanmış sayılmaz; V01 `WAITING_RUNNER` kalırken iki-cursor kuralıyla güvenli kod işi sürebilir.

### V02 — Dependency, lockfile ve Android artifact sınırı — `NOT_STARTED`

- AŞAMA 02 package/source/license kanıtı kodla karşılaştırılır.
- Locked restore, resolved graph, vulnerability/license audit ve Android native asset sınırı yeniden çalıştırılır.
- ProCad ve iOS-only paketlerin Android production graph'a sızmadığı doğrulanır.
- Emulator testi yalnız packaging/native graph açısından anlamlı gerçek bir APK varsa çalıştırılır; dokümantasyon değişikliği için gereksiz koşulmaz.

Çıkış: Android resolved graph exact ve policy uyumlu; unknown/floating/rejected runtime bileşen yok.

### V03 — Fixture, golden sözleşmesi ve Android test matrisi — `NOT_STARTED`

- Manifest, provenance, dual hash, private-ignore ve negatif fixture kontrolleri yeniden çalıştırılır.
- Emulator `E-API36` smoke slotu ile fiziksel Android slotları birbirinden ayrılır.
- Gerçek çizim ve private corpus repoya alınmaz.
- Emulator üzerinde ileride açılacak sentetik/public DWG-DXF seti ve beklenen sonuçlar belirlenir.

Çıkış: V04–V09'un kullanacağı redistributable Android smoke seti ve kanıt sözleşmesi hazırdır.

### V04 — Mimari ve gerçek Android uygulama kabuğu — `NOT_STARTED`

- AŞAMA 04 dependency sınırları, solution build'i ve bütün executable Core/Rendering/Architecture harness marker'ları yeniden doğrulanır.
- Mevcut `src/MobilDwg.App` projesinin `net10.0` sınıf kitaplığı olduğu ve bugün installable MAUI APK üretmediği gerçeklikle kaydedilir.
- Android-only aktif hedef için minimal installable `MobilDwg.App` MAUI kabuğu kurulur; shared Core/Cad/Rendering sınırları korunur.
- Emulator gate, geçici smoke yerine gerçek uygulama APK'sını build/install/launch edebilecek şekilde güvenli biçimde geliştirilir. Altyapı smoke gerekirse ayrı mod olarak korunabilir.
- Gerçek package ID, launcher, crash/ANR, screenshot ve app-process kanıtı alınır.

Çıkış: Gerçek `MobilDwg.App` APK'sı emulator üzerinde açılır; Stage01Smoke sonucu artık viewer sonucu diye kullanılmaz.

### V05 — ACadSharp parser entegrasyonu — `NOT_STARTED`

- AŞAMA 05 parser/corpus/diagnostics iddiaları executable testlerle yeniden çalıştırılır.
- Gerçek uygulama kabuğunda en az bir küçük redistributable DWG ve bir sentetik DXF için parse sonucu/metadata/diagnostics yolu çağrılır.
- Writer/save API kullanılmadığı ve original input'un immutable kaldığı doğrulanır.
- Parse başarı, kontrollü negatif sonuç ve log redaction kanıtı alınır.

Çıkış: Test edilen gerçek Android app revision'ı parser adapter yolunu çalıştırır; host-only parser PASS ile karıştırılmaz.

### V06 — Android FilePicker/SAF ve safe-open — `NOT_STARTED`

- AŞAMA 06 source akışı, quota/disk/atomic-copy/generation/cancel/cleanup testleri yeniden çalıştırılır.
- Emulator Documents/provider akışından gerçek uygulamada küçük DWG ve DXF seçilir.
- Açma, iptal, hızlı ikinci seçim, rotate, background/foreground, close/reopen ve cache cleanup denenir.
- Emulatorun kanıtlayamadığı üreticiye özgü SAF ve fiziksel cihaz farkları `DEFERRED_PHYSICAL_ANDROID` kalır.

Çıkış: Emulator üzerinde gerçek app safe-open yolu geçer; fiziksel telefon farkı dürüstçe açık tutulur.

### V07 — ProCad NO-GO ve production graph izolasyonu — `NOT_STARTED`

- Tarihsel pinned SHA, deterministic precision fixture ve ADR kararı yeniden okunur.
- ProCad'ın production ProjectReference/PackageReference/native artifact graph'ına girmediği otomatik doğrulanır.
- Survey-origin `5,000,000 + 0.001` precision regresyonu çalıştırılır.
- Reddedilmiş ProCad adayını emulator üzerinde yeniden kurmak veya iOS spike yapmak gerekmez.

Çıkış: NO-GO kararı ve custom scene yolu hâlâ kodla tutarlı; gereksiz runtime dependency yok.

### V08 — iOS tarihsel kaydını arşivle, Android sınırını doğrula — `SCOPE_ARCHIVED / ANDROID_GRAPH_CHECK_PENDING`

- AŞAMA 08'in geçmiş evidence/karakterizasyon kaydı korunur; iOS workflow, Mac, simulator, AOT veya iPhone testi çalıştırılmaz.
- Android production/CI graph'ında iOS workload/native asset zorunluluğu olmadığı doğrulanır.
- Shared katmanlarda gelecekte iOS dönüşünü gereksiz yere engelleyen Android-only sızıntı bulunursa adapter sınırında düzeltilir; yeni iOS implementasyonu yazılmaz.

Çıkış: `SCOPE_ARCHIVED`; aktif Android plan iOS blocker'ı taşımadan ilerler ve geri dönüş kapısı korunur.

### V09 — RenderScene, kamera ve diagnostics — `NOT_STARTED`

- AŞAMA 09 T0/T1, semantic snapshot, OCS/WCS, invalid geometry, overflow ve large-coordinate regresyonları yeniden çalıştırılır.
- Gerçek Android app kabuğunun Core/Cad/Rendering composition sınırı doğrulanır.
- Renderer AŞAMA 10 işi erkenden yazılmaz; V09 yalnız scene/camera foundation ve Android runtime linkage smoke ile kapanır.
- Exact app SHA, host test marker'ları ve gerekiyorsa emulator artifact'i kaydedilir.

Çıkış: AŞAMA 01–09 Android geriye dönük doğrulama kuyruğu temizdir; implementation cursor'ı hiç ilerlemediyse AŞAMA 10 güvenli biçimde başlayabilir, ilerlediyse kayıtlı mevcut aşamadan tek hatta sürer.

## 8. V09 sonrasında AŞAMA 10–27 çalışma kuralı

- Aktif sıra: AŞAMA 10–22, ardından Android-only AŞAMA 25–27. AŞAMA 23–24 future iOS track olarak atlanır.
- Her aşama önce kod/host testleriyle ilerler; Android runtime, UI, lifecycle, packaging veya native davranış değiştiğinde anlamlı checkpoint'te gerçek app emulator gate'i çalıştırılır.
- Bilgisayar kapalıysa kod yazımı sürer ve test borcu kuyruğa alınır; kanıtsız `DONE` yazılmaz.
- Fiziksel Android, AŞAMA 20–22 ve final release kapılarında emulatorun yerine geçemeyeceği ölçüm/SAF/cihaz çeşitliliği için yeniden zorunludur.
- iOS yalnız kullanıcının yeni açık kararıyla yeniden etkinleşir. O durumda AŞAMA 08 riskleri ve future AŞAMA 23–24 ayrı plan revizyonuyla açılır; Android v1 geçmişi yeniden yazılmaz.

## 9. Her doğrulama aşamasında güncellenecek kayıtlar

1. Bu dosyanın checkpoint'i ve ilgili VXX durumu.
2. Yeni `docs/evidence/android-validation/VXX.md` kanıtı.
3. `gecmis.md` ve `DEVAM.md` aktif checkpoint'i.
4. Canonical planın yürütme checkpoint'i.
5. `docs/EXECUTION_LOG.md` kısa teknik kayıt.
6. Gerçek test kuyruğu varsa exact SHA/workflow/expected marker ve runner durumu.

Tarihsel `docs/evidence/STAGE_01.md`–`STAGE_09.md` dosyaları silinmez veya yeni sonuç varmış gibi geriye dönük düzeltilmez.
