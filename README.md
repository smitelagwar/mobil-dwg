# mobil-dwg

Android ve iOS için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi.

Proje yürütme aşamasındadır. AŞAMA 00 ve AŞAMA 02 tamamlandı. AŞAMA 01'in canlı toolchain doğrulaması, repo pinleri ve CI build hattı tamamlandı; gerçek geliştirme makinesi + fiziksel Android cihaz install/launch ve iOS erişim envanteri olmadığı için AŞAMA 01 `BLOCKED / DEFERRED_EXTERNAL_GATE` durumunda açık kalır. Kullanıcının onayladığı yürütme istisnası gereği fiziksel erişime bağımlı olmayan işler ilerleyebilir. Sonraki çalışma aşaması AŞAMA 03'tür.

## Yeni sohbet / yeni AI başlangıcı

Projeyi devralan kişi veya ajan önce şu dosyaları bu sırayla okumalıdır:

1. [gecmis.md](gecmis.md) — güncel çalışma checkpoint'i, geçmiş işler, commitler, blocker ve sonraki eylem.
2. [docs/USER_APPROVED_EXECUTION_OVERRIDE.md](docs/USER_APPROVED_EXECUTION_OVERRIDE.md) — ertelenmiş dış donanım/hesap kapılarının yürütme kuralı.
3. [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) — ürün/teknik plan ve aşama çıkış kriterleri.
4. [docs/evidence/STAGE_02.md](docs/evidence/STAGE_02.md) — son tamamlanan bağımsız aşamanın kapanış kanıtı.
5. [compliance/DEPENDENCY_EVIDENCE.md](compliance/DEPENDENCY_EVIDENCE.md) — exact dependency/source/artifact kanıtı.
6. [docs/EXECUTION_LOG.md](docs/EXECUTION_LOG.md) — önceki teknik yürütme geçmişi.
7. [docs/TOOLCHAIN.md](docs/TOOLCHAIN.md) ve [docs/evidence/STAGE_01.md](docs/evidence/STAGE_01.md) — pinlenmiş toolchain ve ertelenmiş Stage 01 dış kapıları.

Sohbet veya model hafızası süreklilik kaynağı değildir; repo kayıtları esas alınır. Planın checkpoint bloğu ile gerçek repo durumu geçici olarak çelişirse planın kendi protokolü gereği gerçek `main`, `gecmis.md` ve ilgili stage evidence çalışma durumu esas alınır; checkpoint ilk güvenli tam-dosya güncellemesinde yeniden senkronize edilir.

## Yetkili plan

Uygulama aşağıdaki aşamalı plana göre geliştirilecektir:

- [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md)

Diğer Markdown dosyaları araştırma, eleştiri ve önceki plan kayıtlarıdır. Ürün kapsamı/çıkış kriterleri konusunda nihai plan geçerlidir; anlık çalışma durumu için `gecmis.md`, gerçek `main` ve evidence dosyaları kullanılır.

## Temel ürün ilkeleri

- Viewer-first; edit ve save v1 kapsamı dışında
- Android-first, iOS zorunlu ikinci platform
- DWG/DXF doğrudan cihazda ve offline işlenir
- Zorunlu bulut, hesap veya dosya başına servis ücreti yok
- Ücretli CAD SDK/API ve runtime royalty yok
- Dependency, native binary, font ve test fixture lisansları release öncesi denetlenir
- Desteklenmeyen entity, eksik font ve dış referanslar sessizce gizlenmez

## Yürütme

Her `devam` komutunda `gecmis.md` içindeki `NEXT_WORK_STAGE` yürütülür. `IN_PROGRESS` aşama varsa yalnız o aşamadan devam edilir. Dış donanım/hesap erişimi nedeniyle `DEFERRED_EXTERNAL_GATE` olarak açık bırakılmış bir aşama, `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` kapsamındaki bağımsız aşamaları bloke etmez. Sahte PASS/DONE üretilmez. Bir kullanıcı turunda en fazla bir aşama tamamlanır.

Her turun sonunda `gecmis.md`, aktif/son stage evidence kaydı ve mümkün olan diğer yürütme kayıtları gerçek durumla güncellenir.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız yeniden dağıtım izni kanıtlanmış fixture ve asset'ler açıkça onaylanmış klasörlerde tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler kendi lisanslarına tabidir ve exact release artifact'i için ayrıca kaydedilecektir.
