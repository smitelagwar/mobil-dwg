# mobil-dwg

Android için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi. iOS aktif v1 kapsamından çıkarılmıştır; shared mimari ileride yeniden etkinleştirilebilecek şekilde korunur.

Implementation AŞAMA 09'a kadar ilerledi. Android geriye dönük doğrulamada V01–V04 tamamlandı; aktif cursor V05 parser entegrasyonudur. Tarihsel AŞAMA 01–09 durumları yeniden yazılmaz. Implementation cursor AŞAMA 10'da ayrı tutulur; bilgisayar/self-hosted runner kapalıyken yalnız A10 ayrı draft branch'inde önden kodlanabilir, fakat Android kanıtı olmadan `main`e merge veya `DONE` yapılamaz.

V01 sonucu yalnız `INFRASTRUCTURE_SMOKE_ONLY` kapsamındadır. V04 gerçek `MobilDwg.App` APK build/install/cold-launch/UI/stability gate'ini geçip PR `#17` ile `main`e merge edildi; claim sınırı `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`dır.

## Yeni sohbet / yeni AI başlangıcı

İki girişten **yalnız biri** seçilir; iki başlatıcı aynı sohbette birlikte okunup çalıştırılmaz:

- Emulatorlü V04–V09 validation için [BASLA.md](BASLA.md) komutu kullanılır.
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

## Yürütme

Her `BASLA.md dosyasını oku` veya validation sohbetindeki `devam` komutunda açık Android VXX doğrulaması yürütülür. Runner çevrim dışıysa yalnız test edilebilir exact SHA kuyruğa yazılır; henüz SHA yoksa mevcut stage durumu korunur. Ayrı `BASLA_A10.md` sohbetinde sonuçsuz host/hosted kontrol `CODED_PENDING_HOST_TESTS`, actual FAIL `FIX_REQUIRED/FIX_IN_PROGRESS`, tüm host kontrolleri geçince V04–V09 uzlaştırması + Android gate bekleyen durum `CODED_PENDING_EMULATOR` olur. A10 main kapanışı bitmeden A11 açılmaz.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız redistribution/provenance durumu kaydedilmiş public/synthetic fixture ve asset'ler açıkça onaylanmış yollar altında tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler ve test kaynakları kendi lisans/provenance kayıtlarına tabidir.
