# mobil-dwg

Android için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi. iOS aktif v1 kapsamından çıkarılmıştır; shared mimari ileride yeniden etkinleştirilebilecek şekilde korunur.

Implementation AŞAMA 09'a kadar ilerledi. Şimdi ayrı bir Android geriye dönük doğrulama turu V01'den başlıyor; tarihsel AŞAMA 01–09 durumları yeniden yazılmıyor. Normal implementation cursor'ı AŞAMA 10'da korunuyor. Bilgisayar/self-hosted runner çevrim dışıysa emulator kanıtı kuyruğa alınır, güvenli kod ve host testleri devam eder; kanıtsız PASS/DONE yazılmaz.

Mevcut emulator gate'in önemli sınırı vardır: çözüm harness'larını gerçekten çalıştırmayan `dotnet test` çağrısı kullanır ve telefona gerçek `MobilDwg.App` yerine geçici `Stage01Smoke` APK'sı kurar. Mevcut screenshot çıktıları da byte-safe değildir. Bu nedenle eski `ANDROID_EMULATOR_GATE_PASS` sonuçları viewer/parser/render PASS değil, en fazla altyapı smoke kanıtıdır. V01 bu açıkları düzeltmeden kapanmayacaktır.

## Yeni sohbet / yeni AI başlangıcı

Projeyi devralan kişi veya ajan önce şu dosyaları bu sırayla okumalıdır:

1. [BASLA.md](BASLA.md) — tek komutla otomatik başlangıç protokolü.
2. [ANDROID_DOGRULAMA_PLANI.md](ANDROID_DOGRULAMA_PLANI.md) — aktif V01–V09 Android yeniden doğrulama sırası ve çevrimdışı çalışma kuralı.
3. [gecmis.md](gecmis.md) ve [DEVAM.md](DEVAM.md) — iki cursor, test kuyruğu, geçmiş commitler ve sonraki eylem.
4. [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) — Android aktif ürün/teknik planı; korunmuş future iOS bölümleri.
5. [docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md](docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md) ve [docs/ANDROID_TEST_KULLANIM_KILAVUZU.md](docs/ANDROID_TEST_KULLANIM_KILAVUZU.md) — GitHub → self-hosted runner → emulator yolu ve kanıt sınırları.
6. [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), [docs/LOCAL_DEVICE_REVALIDATION.md](docs/LOCAL_DEVICE_REVALIDATION.md) ve [docs/EXECUTION_LOG.md](docs/EXECUTION_LOG.md) — gerçek mimari durum, cihaz ayrımı ve yürütme geçmişi.
7. İlgili [docs/evidence](docs/evidence) ve [docs/ADR](docs/ADR) kaydı — tarihsel kanıt; yeni VXX sonucu ayrı revalidation evidence olarak tutulur.

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

Her `BASLA.md dosyasını oku` veya `devam` komutunda önce açık Android VXX doğrulaması yürütülür. Runner çevrim dışıysa exact SHA test kuyruğuna yazılır ve güvenli host-independent iş implementation cursor'ında ilerleyebilir. Bir turda en fazla bir validation veya implementation aşaması kapatılır; emulator/fiziksel cihaz/geçici smoke kanıtları birbirine karıştırılmaz.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız redistribution/provenance durumu kaydedilmiş public/synthetic fixture ve asset'ler açıkça onaylanmış yollar altında tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler ve test kaynakları kendi lisans/provenance kayıtlarına tabidir.
