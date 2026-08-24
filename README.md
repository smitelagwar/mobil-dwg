# mobil-dwg

Android ve iOS için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi.

Proje yürütme aşamasındadır. AŞAMA 00 tamamlandı. AŞAMA 01'in canlı toolchain doğrulaması ve repo pinleri tamamlandı; gerçek geliştirme makinesi + fiziksel Android cihaz build/install/launch kapısı olmadığı için AŞAMA 01 şu anda `BLOCKED` durumundadır. AŞAMA 02 henüz başlamadı.

## Yeni sohbet / yeni AI başlangıcı

Projeyi devralan kişi veya ajan önce şu dosyaları bu sırayla okumalıdır:

1. [gecmis.md](gecmis.md) — güncel çalışma checkpoint'i, geçmiş işler, commitler, blocker ve sonraki eylem.
2. [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) — tek yetkili yürütme planı ve aşama çıkış kriterleri.
3. [docs/EXECUTION_LOG.md](docs/EXECUTION_LOG.md) — teknik komut/test/evidence geçmişi.
4. [docs/TOOLCHAIN.md](docs/TOOLCHAIN.md) — pinlenmiş .NET/MAUI/JDK/Android geliştirme zinciri.
5. [docs/evidence/STAGE_01.md](docs/evidence/STAGE_01.md) — aktif aşamanın tamamlanan ve eksik kanıtları.

Sohbet veya model hafızası süreklilik kaynağı değildir; repo kayıtları esas alınır. Planın checkpoint bloğu ile gerçek repo durumu geçici olarak çelişirse planın kendi protokolü gereği gerçek repo ve `gecmis.md` çalışma durumu esas alınır; checkpoint ilk güvenli tam-dosya güncellemesinde yeniden senkronize edilir.

## Yetkili plan

Uygulama aşağıdaki aşamalı plana göre geliştirilecektir:

- [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md)

Diğer Markdown dosyaları araştırma, eleştiri ve önceki plan kayıtlarıdır. Ürün kapsamı/çıkış kriterleri konusunda nihai plan geçerlidir; anlık çalışma durumu için `gecmis.md` ve evidence dosyaları kullanılır.

## Temel ürün ilkeleri

- Viewer-first; edit ve save v1 kapsamı dışında
- Android-first, iOS zorunlu ikinci platform
- DWG/DXF doğrudan cihazda ve offline işlenir
- Zorunlu bulut, hesap veya dosya başına servis ücreti yok
- Ücretli CAD SDK/API ve runtime royalty yok
- Dependency, native binary, font ve test fixture lisansları release öncesi denetlenir
- Desteklenmeyen entity, eksik font ve dış referanslar sessizce gizlenmez

## Yürütme

Her `devam` komutunda aktif aşama yürütülür. `BLOCKED` veya `IN_PROGRESS` aşama varsa sonraki aşamaya geçilmez. Bir kullanıcı turunda en fazla bir aşama tamamlanır.

Her turun sonunda `gecmis.md`, `docs/EXECUTION_LOG.md`, aktif `docs/evidence/` kaydı ve mümkün olduğunda nihai plan checkpoint'i güncellenir.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız yeniden dağıtım izni kanıtlanmış fixture ve asset'ler açıkça onaylanmış klasörlerde tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler kendi lisanslarına tabidir ve exact release artifact'i için ayrıca kaydedilecektir.
