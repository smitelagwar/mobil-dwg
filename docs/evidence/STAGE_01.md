# AŞAMA 01 Evidence — .NET/MAUI/Android toolchain ve gerçek telefon

Tarih: 2026-08-24

Durum: `BLOCKED`

AŞAMA 01 tamamen bitmiş değildir. Exact toolchain hattı ve temiz MAUI Android Debug/Release build kapısı GitHub Actions üzerinde başarıyla doğrulandı; fakat nihai planın zorunlu gerçek geliştirme makinesi + fiziksel Android cihaz install/launch kapısı bu oturumda gerçekleştirilemedi.

## Tamamlanan bağımsız işler

- [x] Güncel .NET 10 SDK servicing hattı doğrulandı: SDK `10.0.400`, runtime `10.0.11`, release `2026-08-11`.
- [x] Repo kökünde exact SDK/workload-set çözümlemesi `global.json` ile pinlendi: `10.0.400`, `rollForward=disable`, prerelease kapalı.
- [x] Microsoft Build of OpenJDK `21.0.12` LTS exact artifact ile kuruldu ve resmi SHA-256 dosyasıyla doğrulandı.
- [x] Android command-line tools bootstrap build `15859902` kullanıldı.
- [x] Android SDK Platform API `36`, Build-Tools `36.0.0` ve stable Platform-Tools `37.0.1` kuruldu/doğrulandı.
- [x] `maui-android` workload, `global.json` tarafından seçilen workload version `10.0.400` ile başarıyla kuruldu.
- [x] Temiz `.NET MAUI App` projesi oluşturuldu.
- [x] .NET 10 MAUI şablonunun varsayılan Android minimumunun API 21 olduğu gerçek build çıktısında görüldü; proje baseline kararı gereği `SupportedOSPlatformVersion` açıkça `24.0` olarak pinlendi.
- [x] `net10.0-android` Debug build başarıyla tamamlandı: `0 Warning(s), 0 Error(s)`.
- [x] `net10.0-android` Release build başarıyla tamamlandı: `0 Warning(s), 0 Error(s)`.
- [x] Üretilen Android manifest doğrulandı: `minSdkVersion="24"`, `targetSdkVersion="36"`.
- [x] Debug/Release APK çıktıları GitHub Actions artifact olarak yüklendi.
- [x] CI workflow'u PR #1 üzerinden `main` branch'e merge edildi.

## Nihai CI kanıtı

GitHub Actions:

- Workflow: `Stage 01 Toolchain Smoke`
- Run: `32737334339` / run #9
- Sonuç: `SUCCESS`
- PR branch final commit: `49c3e3f2f855c1f7f1cf945049cc5d93805e7003`
- Main merge commit: `83379b24e4ba87f04299f612ae2951ae8d8aec13`
- Runner: Ubuntu 24.04 x64
- .NET SDK: `10.0.400`
- .NET runtime: `10.0.11`
- Microsoft OpenJDK: `21.0.12`
- ADB / Platform-Tools: `37.0.1-15733141`
- Android workload manifest: `Microsoft.NET.Sdk.Android 36.1.69`
- MAUI workload: `maui-android`, workload version `10.0.400`, MAUI manifest `10.0.20/10.0.100`
- Android SDK: API `36`
- Build-Tools: `36.0.0`
- Manifest: `<uses-sdk android:minSdkVersion="24" android:targetSdkVersion="36" />`
- Artifact ID: `9523977201`
- Artifact name: `stage01-maui-android-smoke`
- Artifact size: `57,601,187` bytes
- Artifact ZIP SHA-256: `3fd12ffe750352e9ace5532eaffa8f1cd6619da449bddeb05efb5acfc91dcd41`

## CI sırasında yakalanan ve çözülen sorunlar

- Run #6: manifest testindeki `find ... | head -n 1` zinciri `set -o pipefail` altında `Broken pipe` üretti. Build başarısız değildi; doğrulama scripti deterministik manifest yoluna çevrildi.
- Run #7: gerçek üretilen manifest `minSdkVersion=21 / targetSdkVersion=36` gösterdi. Test gevşetilmedi; proje kararı API 24 minimum olduğu için smoke csproj açıkça `24.0` pinlendi.
- Run #8: Microsoft JDK artifact indirmesi `curl (18)` ile yarıda kesildi. Exact resmi artifact ve checksum korunarak `curl --retry 5 --retry-all-errors` eklendi.
- Run #9: bütün toolchain, workload, Debug/Release build, API 24/36 manifest ve artifact upload adımları PASS.

## Bu ChatGPT çalışma konteynerindeki eski gözlem

Bu ölçümler kullanıcının gerçek geliştirme bilgisayarı değildir:

```text
OS/arch: Linux x86_64
Git: 2.47.3 (AŞAMA 00 ölçümü)
Java: OpenJDK 21.0.11
Dotnet: PATH üzerinde yok
ADB: PATH üzerinde yok
Disk: yaklaşık 38 GB boş
```

Bu konteynerde doğrudan .NET binary indirme denemesi dış DNS kısıtı nedeniyle `Could not resolve host: builds.dotnet.microsoft.com` ile başarısız olmuştu. Bu sonuç artık CI toolchain kanıtını etkilemez; yalnız sohbet konteynerinin ağ kısıtıdır.

## Eksik zorunlu kanıtlar

- [ ] Kullanıcının gerçek geliştirme makinesinde pinlenmiş toolchain'in kurulu/çalışır olduğunun yerel doğrulaması.
- [ ] Fiziksel Android cihazın `adb devices` çıktısında `device` olarak görünmesi.
- [ ] Smoke app'in fiziksel telefona install edilmesi.
- [ ] Smoke app'in fiziksel telefonda launch edilip açıldığının kanıtlanması.
- [ ] iOS için Mac/Xcode/iPhone/Apple Developer erişiminin yalnız envanterlenmesi.

## Blocker

Bu oturumda kullanıcının gerçek geliştirme makinesine veya fiziksel Android telefonuna USB/ADB erişimi yok. CI, build ve packaging hattını güçlü biçimde doğrulasa da nihai plan AŞAMA 01 çıkış kriteri gerçek telefonda uygulamanın çalışmasını zorunlu tuttuğundan `DONE` yazılamaz ve AŞAMA 02 başlatılamaz.

## Sonraki somut eylem

Gerçek geliştirme makinesinde `docs/TOOLCHAIN.md` baseline'ını doğrula; fiziksel Android telefonu bağla; `adb devices` çıktısında `device` durumunu kaydet; smoke uygulamasını install edip launch et; ardından iOS erişim envanterini kapat ve bu evidence dosyasına gerçek cihaz kanıtını ekle.
