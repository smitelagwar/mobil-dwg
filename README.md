# mobil-dwg

Android ve iOS için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi.

Proje şu anda planlama ve teknik doğrulama başlangıcındadır. Uygulama kodu henüz oluşturulmamıştır.

## Yetkili plan

Uygulama aşağıdaki aşamalı plana göre geliştirilecektir:

- [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md)

Diğer Markdown dosyaları araştırma, eleştiri ve önceki plan kayıtlarıdır. Bir çelişki halinde nihai plan esas alınır.

## Temel ürün ilkeleri

- Viewer-first; edit ve save v1 kapsamı dışında
- Android-first, iOS zorunlu ikinci platform
- DWG/DXF doğrudan cihazda ve offline işlenir
- Zorunlu bulut, hesap veya dosya başına servis ücreti yok
- Ücretli CAD SDK/API ve runtime royalty yok
- Dependency, native binary, font ve test fixture lisansları release öncesi denetlenir
- Desteklenmeyen entity, eksik font ve dış referanslar sessizce gizlenmez

## Yürütme

Her `devam` komutunda nihai plandaki aktif aşama yürütülür. Aşama bitmezse sonraki turda aynı yerden sürer; biterse bir sonraki aşama ancak sonraki turda başlar.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız yeniden dağıtım izni kanıtlanmış fixture ve asset'ler açıkça onaylanmış klasörlerde tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler kendi lisanslarına tabidir ve exact release artifact'i için ayrıca kaydedilecektir.
