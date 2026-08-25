# mobil-dwg

Android için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi. iOS aktif v1 kapsamından çıkarılmıştır; shared mimari ileride yeniden etkinleştirilebilecek şekilde korunur.

Implementation AŞAMA 09'a kadar ilerledi. Android geriye dönük doğrulamada V01–V08 tamamlandı; aktif cursor V09 `NOT_STARTED` durumundadır. Tarihsel AŞAMA 01–09 durumları yeniden yazılmaz. Implementation cursor AŞAMA 10'da ayrı tutulur; bilgisayar/self-hosted runner kapalıyken yalnız A10 ayrı draft branch'inde önden kodlanabilir, fakat Android kanıtı olmadan `main`e merge veya `DONE` yapılamaz.

V01 sonucu yalnız `INFRASTRUCTURE_SMOKE_ONLY` kapsamındadır. V04 gerçek `MobilDwg.App` APK build/install/cold-launch/UI/stability gate'ini geçti; V05 production ACadSharp parser'ı gerçek Android process içinde doğruladı. V06 gerçek `MobilDwg.App` FilePicker/DocumentsUI/SAF → stream → app-private safe-copy → production parser akışını API36 emulator üzerinde DWG/DXF ve lifecycle/cleanup senaryolarıyla doğruladı. V07 exact unpatched ProCad candidate için `NO-GO` kararını yeniden doğruladı; ProCad/ProCadSharp'ın production/resolved Android graph ve Release APK'da bulunmadığını, rejected direct-float `5,000,000 + 0.001` precision blocker'ının sürdüğünü ve production double hattının `0.001` detayı koruduğunu kanıtladı. V08 ise tarihsel iOS kapsamını yeniden açmadan Android production/CI graph'ının iOS-specific TFM/RID/native/toolchain zorunluluğundan izole olduğunu, Windows üzerinde yalnız Android workload ile locked restore + Release build yapılabildiğini ve Android APK'da iOS native/framework girdisi bulunmadığını doğruladı.

V07 claim'i `PROCAD_NO_GO_PRODUCTION_GRAPH_ISOLATION_AND_PRECISION_REGRESSION_ONLY`; V08 claim'i `ANDROID_PRODUCTION_CI_GRAPH_IOS_ISOLATION_ONLY_HISTORICAL_IOS_SCOPE_ARCHIVED` ile sınırlıdır. Tarihsel iOS AŞAMA 08 karakterizasyonu future option olarak arşivde kalır; iOS PASS değildir.

## Yeni sohbet / yeni AI başlangıcı

İki girişten **yalnız biri** seçilir; iki başlatıcı aynı sohbette birlikte okunup çalıştırılmaz:

- Emulatorlü V01–V09 validation için [BASLA.md](BASLA.md) komutu kullanılır.
- Bilgisayar/runner kapalıyken sınırlı A10 taslağı için ayrı sohbette [BASLA_A10.md](BASLA_A10.md) komutu kullanılır; durum [docs/A10_WORKSTREAM.md](docs/A10_WORKSTREAM.md) ve açık A10 branch/PR üzerinden okunur.

Seçilen başlatıcı gerekli yetkili plan, checkpoint, workflow ve evidence dosyalarının devamını kendi protokolüne göre okur. İki hattın ortak ana kaynakları [ANDROID_DOGRULAMA_PLANI.md](ANDROID_DOGRULAMA_PLANI.md), [gecmis.md](gecmis.md) ve [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) dosyalarıdır. [DEVAM.md](DEVAM.md) yalnız validation sohbetinin anlık snapshot'ıdır.

Sohbet veya model hafızası süreklilik kaynağı değildir; repo kayıtları esas alınır.

## Yetkili plan

Uygulama [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) planına göre geliştirilir. Aktif AŞAMA 01–09 Android tekrar turu için [ANDROID_DOGRULAMA_PLANI.md](ANDROID_DOGRULAMA_PLANI.md) yetkili alt plandır. Öneri/master dosyaları araştırma, `docs/evidence` kayıtları tarihsel kanıt niteliğindedir.

## Temel ürün ilkeleri

- Viewer-first; edit ve save v1 kapsamı dışında
- Aktif v1 Android-only; iOS future option ve adapter sınırları korunmuş
- DWG/DXF doğrudan cihazda ve offline işlenir
- Zorunlu bulut, hesap veya dosya başına servis ücreti yok
- Ücretli CAD SDK/API ve runtime royalty yok
- Dependency, native binary, font ve test fixture lisansları release öncesi denetlenir
- Desteklenmeyen entity, eksik font ve dış referanslar sessizce gizlenmez
- Original CAD immutable kalır; FilePicker/SAF içeriği immediate app-private safe-copy üzerinden işlenir

## Yürütme

Her `BASLA.md dosyasını oku` veya validation sohbetindeki `devam` komutunda açık Android VXX doğrulaması yürütülür. V08 `VALIDATED`; sonraki açık aşama V09 `NOT_STARTED` durumundadır ve aynı V08 kapanış turunda başlatılmaz. Tarihsel iOS workflow/Mac/simulator/iPhone kapsamı kullanıcı tarafından açıkça yeniden etkinleştirilmedikçe açılmaz. Runner çevrim dışıysa yalnız test edilebilir exact SHA kuyruğa yazılır; henüz SHA yoksa mevcut stage durumu korunur. Ayrı `BASLA_A10.md` sohbetinde sonuçsuz host/hosted kontrol `CODED_PENDING_HOST_TESTS`, actual FAIL `FIX_REQUIRED/FIX_IN_PROGRESS`, tüm host kontrolleri geçince V04–V09 uzlaştırması + Android gate bekleyen durum `CODED_PENDING_EMULATOR` olur. A10 main kapanışı bitmeden A11 açılmaz.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız redistribution/provenance durumu kaydedilmiş public/synthetic fixture ve asset'ler açıkça onaylanmış yollar altında tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler ve test kaynakları kendi lisans/provenance kayıtlarına tabidir.
