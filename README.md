# mobil-dwg

Android için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi. iOS aktif v1 kapsamından çıkarılmıştır; shared mimari ileride yeniden etkinleştirilebilecek şekilde korunur.

Implementation AŞAMA 09'a kadar tamamlandı. Android geriye dönük doğrulama programı V01–V09 artık kapalıdır; tüm aşamalar kendi claim sınırları içinde `VALIDATED` durumundadır. Implementation cursor AŞAMA 10'dadır ve henüz başlamamış/`main`e merge edilmemiştir. AŞAMA 11, A10 gerçek Android integration gate'i ve `DONE ON MAIN` tamamlanmadan açılamaz.

V01 yalnız infrastructure smoke'u doğruladı. V04 gerçek `MobilDwg.App` APK build/install/cold-launch/UI/stability gate'ini geçti; V05 production ACadSharp parser'ı gerçek Android process içinde doğruladı. V06 gerçek FilePicker/DocumentsUI/SAF → stream → app-private safe-copy → production parser akışını API36 emulator üzerinde doğruladı. V07 exact unpatched ProCad candidate için `NO-GO` kararını ve production graph/precision izolasyonunu yeniden doğruladı. V08 tarihsel iOS kapsamını yeniden açmadan Android production/CI graph'ının iOS-specific TFM/RID/native/toolchain zorunluluğundan izole olduğunu kanıtladı. V09 ise RenderScene/camera/OCS/diagnostics temelini, deterministic `render-scene/v1` snapshot'ını, survey-origin `0.001` double precision'ı, Core/architecture sınırlarını ve gerçek Android app Release composition build'ini current exact revision üzerinde yeniden doğruladı.

V09 claim'i `RENDER_SCENE_CAMERA_DIAGNOSTICS_FOUNDATION_AND_ANDROID_COMPOSITION_REVALIDATION_ONLY_NOT_GEOMETRY_RENDER_FIDELITY` ile sınırlıdır. V09 geometri renderer fidelity, emulator UI/runtime veya fiziksel cihaz PASS'i değildir. Tarihsel iOS AŞAMA 08 karakterizasyonu future option olarak arşivde kalır; iOS PASS değildir.

## Yeni sohbet / yeni AI başlangıcı

Normal proje devamı için [BASLA.md](BASLA.md) kullanılır. Bu dosya gerçek GitHub durumunu okuyup açık validation varsa onu, validation programı kapalıysa sıradaki implementation aşamasını yürütür. Android V01–V09 kapandığı için sıradaki normal implementation cursor AŞAMA 10'dur.

`BASLA_A10.md` yalnız A10'un özel/izole workstream protokolüne ihtiyaç duyulan ayrı çalışma bağlamlarında kullanılabilir. A10 durumu [docs/A10_WORKSTREAM.md](docs/A10_WORKSTREAM.md) üzerinden doğrulanır; hiçbir durumda Android kanıtı olmadan `main` merge veya `DONE` yapılmaz.

Ana kaynaklar [ANDROID_DOGRULAMA_PLANI.md](ANDROID_DOGRULAMA_PLANI.md), [gecmis.md](gecmis.md) ve [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) dosyalarıdır. [DEVAM.md](DEVAM.md) anlık handoff snapshot'ıdır. Sohbet/model hafızası süreklilik kaynağı değildir; repo kayıtları esas alınır.

## Yetkili plan

Uygulama [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) planına göre geliştirilir. V01–V09 kapanış sonuçları [ANDROID_DOGRULAMA_PLANI.md](ANDROID_DOGRULAMA_PLANI.md) ve `docs/evidence/android-validation/` altında korunur.

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

Her `BASLA.md dosyasını oku` veya normal `devam` komutunda gerçek `main`, açık PR/CI ve checkpoint doğrulanır. Android V01–V09 programı kapalı olduğundan bir sonraki normal çalışma AŞAMA 10'u açabilir; fakat V09 kapanış turunun kendisinde A10 başlatılmamıştır. Runner çevrim dışıysa kanıtsız PASS yazılmaz ve aynı test işi çoğaltılmaz. A10 main kapanışı bitmeden A11 açılmaz.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız redistribution/provenance durumu kaydedilmiş public/synthetic fixture ve asset'ler açıkça onaylanmış yollar altında tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler ve test kaynakları kendi lisans/provenance kayıtlarına tabidir.
