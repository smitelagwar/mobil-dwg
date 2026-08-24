# AŞAMA 01 Evidence — .NET/MAUI/Android toolchain ve gerçek telefon

Tarih: 2026-08-24

Durum: `BLOCKED`

AŞAMA 01 tamamen bitmiş değildir. Exact toolchain hattı, temiz MAUI Android Debug/Release build kapısı ve fiziksel cihaz kapısını çalıştıracak repo scriptleri GitHub Actions üzerinde doğrulandı; fakat nihai planın zorunlu gerçek geliştirme makinesi + fiziksel Android cihaz install/launch kapısı bu oturumda gerçekleştirilemedi.

## Tamamlanan bağımsız işler

- [x] Güncel .NET 10 SDK servicing hattı doğrulandı: SDK `10.0.400`, runtime `10.0.11`, release `2026-08-11`.
- [x] Repo kökünde exact SDK/workload-set çözümlemesi `global.json` ile pinlendi: `10.0.400`, `rollForward=disable`, prerelease kapalı.
- [x] Microsoft Build of OpenJDK `21.0.12` LTS exact artifact ile kuruldu ve resmi SHA-256 dosyasıyla doğrulandı.
- [x] Android command-line tools bootstrap build `15859902` kullanıldı.
- [x] Android SDK Platform API `36`, Build-Tools `36.0.0` ve stable Platform-Tools `37.0.1` kuruldu/doğrulandı.
- [x] `maui-android` workload, `global.json` tarafından seçilen workload version `10.0.400` ile başarıyla kuruldu.
- [x] Temiz `.NET MAUI App` projesi oluşturuldu.
- [x] .NET 10 MAUI şablonunun varsayılan Android minimumunun API 21 olduğu gerçek build çıktısında görüldü; proje baseline kararı gereği `SupportedOSPlatformVersion` açıkça `24.0` olarak pinlendi.
- [x] Smoke app için çakışma riskini azaltan sabit `ApplicationId` pinlendi: `com.smitelagwar.mobildwg.stage01smoke`.
- [x] `net10.0-android` Debug build başarıyla tamamlandı.
- [x] `net10.0-android` Release build başarıyla tamamlandı.
- [x] Üretilen Android manifest doğrulandı: `minSdkVersion="24"`, `targetSdkVersion="36"`, pinned package/application ID.
- [x] Debug/Release APK çıktıları GitHub Actions artifact olarak yüklendi.
- [x] Fiziksel cihaz gate'i için `scripts/stage01-device-gate.sh` ve `scripts/stage01-device-gate.ps1` eklendi.
- [x] Cihaz-gate scriptleri exact SDK `10.0.400`, workload set `10.0.400`, JDK `21.0.12`, ADB `37.0.1`, API 36, Build-Tools `36.0.0`, fiziksel `state=device`, Debug/Release build, manifest 24/36, install ve launcher `Status: ok` koşullarını zorunlu kılıyor.
- [x] Gate scriptleri emülatörü reddediyor; birden çok cihazda `ANDROID_SERIAL` ile açık seçim istiyor ve kanıt çıktısına tam ADB seri numarasını yazmıyor.
- [x] Bash ve PowerShell gate scriptlerinin parse/syntax kontrolü Stage 01 CI workflow'una eklendi ve PASS oldu.
- [x] Device-gate runbook'u `docs/TOOLCHAIN.md` içine eklendi.
- [x] Device-gate otomasyonu PR #2 üzerinden `main` branch'e merge edildi.

## Güncel nihai CI kanıtı

GitHub Actions:

- Workflow: `Stage 01 Toolchain Smoke`
- Run: `32739952628` / run #17
- Sonuç: `SUCCESS`
- PR: `#2` — `ci: verify stage 01 physical device gate`
- PR head commit: `9e2c0f71153ca0db936c19a10d2f53dc38cca7ec`
- Main merge commit: `9b375af9931a3db23f82e9b983257f29030a7376`
- Bash + PowerShell device-gate parse kontrolü: `SUCCESS`
- Runner: Ubuntu 24.04 x64
- .NET SDK: `10.0.400`
- .NET runtime: `10.0.11`
- Microsoft OpenJDK: `21.0.12`
- ADB / Platform-Tools: `37.0.1`
- Workload set: `10.0.400`
- MAUI workload: `maui-android`
- Android SDK: API `36`
- Build-Tools: `36.0.0`
- Smoke ApplicationId: `com.smitelagwar.mobildwg.stage01smoke`
- Manifest: `minSdkVersion=24`, `targetSdkVersion=36`, pinned package ID
- Debug build: `SUCCESS`
- Release build: `SUCCESS`
- Artifact upload: `SUCCESS`
- Artifact ID: `9524964656`
- Artifact name: `stage01-maui-android-smoke`
- Artifact size: `57,817,776` bytes
- Artifact ZIP SHA-256: `cfd2221a9a31193c76b4347f633ec062d54abca5117edea887bc46a0926f6d0f`

Önceki final build kanıtı run #9 / `32737334339` ve artifact `9523977201` tarihsel kanıt olarak geçerlidir; run #17 cihaz-gate otomasyonu ve pinned ApplicationId değişikliklerini de içeren daha güncel doğrulamadır.

## CI sırasında yakalanan ve çözülen sorunlar

- Run #6: manifest testindeki `find ... | head -n 1` zinciri `set -o pipefail` altında `Broken pipe` üretti. Build başarısız değildi; doğrulama scripti deterministik manifest yoluna çevrildi.
- Run #7: gerçek üretilen manifest `minSdkVersion=21 / targetSdkVersion=36` gösterdi. Test gevşetilmedi; proje kararı API 24 minimum olduğu için smoke csproj açıkça `24.0` pinlendi.
- Run #8: Microsoft JDK artifact indirmesi `curl (18)` ile yarıda kesildi. Exact resmi artifact ve checksum korunarak `curl --retry 5 --retry-all-errors` eklendi.
- Run #9: bütün toolchain, workload, Debug/Release build, API 24/36 manifest ve artifact upload adımları PASS.
- Run #17: fiziksel cihaz gate scriptlerinin parse kontrolü, pinned `ApplicationId`, exact toolchain, workload, Debug/Release build, 24/36 manifest/package doğrulaması ve artifact upload birlikte PASS.

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

Bu konteynerde doğrudan .NET binary indirme denemesi dış DNS kısıtı nedeniyle `Could not resolve host: builds.dotnet.microsoft.com` ile başarısız olmuştu. Bu sonuç CI toolchain kanıtını etkilemez; yalnız sohbet konteynerinin ağ kısıtıdır.

## Eksik zorunlu kanıtlar

- [ ] Kullanıcının gerçek geliştirme makinesinde pinlenmiş toolchain'in kurulu/çalışır olduğunun yerel doğrulaması.
- [ ] Fiziksel Android cihazın `adb devices` çıktısında `device` olarak görünmesi.
- [ ] Smoke app'in fiziksel telefona install edilmesi.
- [ ] Smoke app'in fiziksel telefonda launch edilip açıldığının kanıtlanması.
- [ ] iOS için Mac/Xcode/iPhone/Apple Developer erişiminin yalnız envanterlenmesi.

## Blocker

Bu oturumda kullanıcının gerçek geliştirme makinesine veya fiziksel Android telefonuna USB/ADB erişimi yok. CI, build, packaging ve cihaz-gate otomasyonunu doğrulasa da fiziksel cihaz scriptini gerçek telefon üzerinde çalıştıramaz. Nihai plan AŞAMA 01 çıkış kriteri gerçek telefonda uygulamanın çalışmasını zorunlu tuttuğundan `DONE` yazılamaz ve AŞAMA 02 başlatılamaz.

## Sonraki somut eylem

Gerçek geliştirme makinesinde repo kökünden işletim sistemine uygun gate'i çalıştır:

```powershell
.\scripts\stage01-device-gate.ps1
```

veya:

```bash
bash scripts/stage01-device-gate.sh
```

Çıktıda `STAGE01_DEVICE_GATE_PASS`, `device_state=device,physical`, `debug_build=PASS`, `release_build=PASS`, `install=PASS`, `launch=PASS` görülmeden fiziksel cihaz kapısı kapanmaz. Sonrasında iOS Mac/Xcode/iPhone/Apple Developer erişim envanteri kaydedilir ve AŞAMA 01 checkpoint'i yeniden değerlendirilir.
