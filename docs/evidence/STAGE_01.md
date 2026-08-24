# AŞAMA 01 Evidence — .NET/MAUI/Android toolchain ve gerçek telefon

Tarih: 2026-08-24

Durum: `BLOCKED`

AŞAMA 01 tamamen bitmiş değildir. Canlı sürüm/politika doğrulaması ve repo pinleri tamamlandı; gerçek geliştirme makinesinde kurulum, MAUI Debug/Release build ve fiziksel Android cihaz install/launch kanıtı eksiktir.

## Tamamlanan bağımsız işler

- [x] Güncel .NET 10 SDK servicing hattı resmi .NET kaynağından doğrulandı: SDK `10.0.400`, runtime `10.0.11`, release `2026-08-11`.
- [x] Repo kökünde exact SDK ve workload set için `global.json` oluşturuldu.
- [x] .NET 10 Android'in API 36 desteği ve önerilen min API 24 değeri resmi Microsoft kaynağından doğrulandı.
- [x] JDK 21 desteği resmi .NET 10 MAUI/Android kaynağından doğrulandı.
- [x] Microsoft OpenJDK 21 güncel patch'i `21.0.12` olarak doğrulandı.
- [x] Google Play'in 2026-08-31 itibarıyla yeni uygulama/güncellemelerde target API 36 gereksinimi doğrulandı.
- [x] Android SDK Platform 36 revision 1 ve Build-Tools `36.0.0` baseline'ı doğrulandı.
- [x] Stable Android Platform-Tools baseline `37.0.0` olarak doğrulandı; `37.0.1` Canary olduğu için dışlandı.
- [x] Android command-line tools bootstrap artifact build ID `15859902` kaydedildi.
- [x] Toolchain hedefi ve doğrulama kapıları `docs/TOOLCHAIN.md` içine yazıldı.

## Bu çalışma konteynerinde ölçülen durum

Bu ölçümler ChatGPT konteynerine aittir; kullanıcının gerçek geliştirme bilgisayarı değildir.

```text
OS/arch: Linux x86_64
Git: 2.47.3 (önceki AŞAMA 00 ölçümü)
Java: OpenJDK 21.0.11
Dotnet: PATH üzerinde yok
ADB: PATH üzerinde yok
Disk: yaklaşık 38 GB boş
```

.NET SDK 10.0.400 resmi doğrudan indirme adresi web üzerinden doğrulandı, ancak bu çalışma konteynerinin dış DNS/network erişimi kapalı olduğu için binary indirme/kurulum denemesi `Could not resolve host: builds.dotnet.microsoft.com` ile başarısız oldu. Bu hata ürün/toolchain uyumsuzluğu değildir; çalışma konteyneri ağ kısıtıdır.

## Eksik zorunlu kanıtlar

- [ ] Gerçek geliştirme makinesinde .NET SDK `10.0.400` kurulumu ve `dotnet --info`.
- [ ] `maui-android` workload set `10.0.400` kurulumu ve `dotnet workload list`.
- [ ] Microsoft OpenJDK `21.0.12` kurulumu ve `JAVA_HOME` doğrulaması.
- [ ] Android SDK API 36 + Build-Tools `36.0.0` + stable Platform-Tools `37.0.0` kurulumu.
- [ ] Temiz MAUI smoke app Debug build.
- [ ] Temiz MAUI smoke app Release build.
- [ ] Fiziksel Android cihazın `adb devices` çıktısında `device` olarak görünmesi.
- [ ] Smoke app'in fiziksel telefona install edilip açılması.
- [ ] iOS için Mac/Xcode/iPhone/Apple hesap erişiminin yalnız envanterlenmesi.

## Blocker

Bu oturumda kullanıcının gerçek geliştirme makinesine veya fiziksel Android telefonuna USB/ADB erişimi yok. Nihai plan AŞAMA 01 çıkış kriteri gerçek telefonda MAUI uygulamasının çalışmasını zorunlu tuttuğundan `DONE` yazılamaz.

## Sonraki somut eylem

Gerçek geliştirme ortamında `docs/TOOLCHAIN.md` baseline'ına göre .NET 10.0.400 + MAUI Android workload + Microsoft OpenJDK 21.0.12 + Android API 36 araçlarını kur; ardından Debug/Release boş MAUI smoke app build ve fiziksel telefon `adb` install/launch kanıtını kaydet.
