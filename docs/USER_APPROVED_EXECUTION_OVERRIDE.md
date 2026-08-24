# Kullanıcı Onaylı Yürütme Override'ı

Tarih: 2026-08-24

Durum: `ACTIVE`

Bu belge, kullanıcının şu an fiziksel Android cihaz + gerçek geliştirme makinesi, Mac/Xcode/iPhone ve Apple Developer erişim kanıtlarını sağlayamadığını; buna rağmen yalnız `devam` diyerek projedeki bağımsız işlerin sürmesini istediğini kalıcı olarak kaydeder.

## Kural

Yalnız kullanıcı donanımı, hesabı, özel cihaz bağlantısı veya dış platform erişimi gerektiren bir aşama kapısı tamamlanamıyorsa:

1. Eksik kapı `DONE` veya `PASS` olarak gösterilmez.
2. İlgili aşama kendi evidence kaydında `BLOCKED` kalabilir.
3. Eksik kapı `DEFERRED_EXTERNAL_GATE` olarak ayrıca kaydedilir.
4. Kullanıcı bu dış erişimi şu an sağlayamadığını açıkça belirtmişse, bu eksik kapı bağımsız sonraki aşamaların başlamasını tek başına engellemez.
5. Sonraki aşamada eksik dış kapıya teknik olarak bağımlı olmayan işler yapılır.
6. Bir sonraki aşama gerçek cihaz/Mac/hesap olmadan dürüstçe test edilemiyorsa o noktada yeniden `BLOCKED` olunur; test sonucu uydurulmaz.
7. Ertelenen dış kapılar release, beta veya kendilerine doğrudan bağımlı milestone öncesinde yeniden açılır ve gerçek kanıt olmadan kapanmaz.
8. Definition of Done değişmez: final ürün Android ve iOS gerçek cihaz kanıtları olmadan tamamlanmış sayılmaz.

## AŞAMA 01 için mevcut ertelenen dış kapılar

- Fiziksel Android cihazda `STAGE01_DEVICE_GATE_PASS`.
- Gerçek geliştirme makinesinde local toolchain/device install/launch kanıtı.
- `docs/STAGE_01_IOS_ACCESS_INVENTORY.md` içindeki Mac/Xcode/iPhone/Apple Developer erişim alanlarının gerçek `YES/NO/N/A` değerleriyle kapanması.

Bunların hiçbirine şu an erişim olmadığı kullanıcı tarafından açıkça bildirildi. Bu nedenle AŞAMA 01 `DONE` değildir; fakat AŞAMA 02 gibi fiziksel cihaz veya Mac gerektirmeyen bağımsız işler `devam` komutuyla başlatılabilir.

## Çakışma halinde öncelik

Bu kullanıcı onaylı override, yalnız yürütme sırasındaki dış erişim blocker'larının ilerlemeyi tamamen durdurmasına ilişkin eski kurala özel bir istisnadır. Nihai planın ürün kapsamı, lisans politikası, gerçek cihaz zorunlulukları ve Definition of Done şartlarını kaldırmaz.

Yeni bir ajan `gecmis.md` sonrasında bu dosyayı okumalı ve eksik dış erişimi sahte başarıya çevirmeden bağımsız işlere devam etmelidir.
