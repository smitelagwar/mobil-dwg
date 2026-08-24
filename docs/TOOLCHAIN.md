# mobil-dwg — Toolchain Baseline

Bu dosya AŞAMA 01 için canlı doğrulanmış geliştirme zinciri hedefini kaydeder. Sürüm yükseltmeleri otomatik yapılmaz; her değişiklik yeniden doğrulanıp bu dosya ve `global.json` birlikte güncellenir.

## Doğrulama tarihi

- 2026-08-24

## Pinlenen .NET / MAUI hattı

- .NET SDK: `10.0.400`
- .NET runtime servicing seviyesi: `10.0.11`
- SDK release tarihi: 2026-08-11
- Workload update mode: workload-set
- Workload set: `10.0.400`
- Android-first workload: `maui-android`
- SDK çözümleme politikası: exact (`rollForward=disable`)
- Prerelease: kapalı

Repo kökündeki `global.json` bu pinleri uygular.

Kurulum doğrulama komutları:

```bash
dotnet --version
dotnet --info
dotnet workload install maui-android --version 10.0.400
dotnet workload list
```

Not: Workload kurulumu gerçek geliştirme makinesinde henüz kanıtlanmadı; CI smoke kurulumu host/ci kanıtıdır, fiziksel cihaz kapısını karşılamaz.

## Java hattı

- Dağıtım: Microsoft Build of OpenJDK
- Major: `21` LTS
- Pinlenen güncel patch: `21.0.12`
- `JAVA_HOME` zorunlu olarak bu JDK'yı göstermeli.

.NET 10'a özel Android belgeleri JDK 21 ile build desteğini doğrular. Genel MAUI kurulum sayfasındaki bazı kurulum örnekleri hâlâ OpenJDK 17 önerebilir; bu proje nihai plan gereği JDK 21 kullanır ve .NET 10 bunu destekler.

Doğrulama:

```bash
java -version
javac -version
```

CI notu: 2026-08-24 tarihinde `actions/setup-java@v5` Microsoft kataloğu 21.0.12'yi henüz listelemiyordu. Resmi Microsoft OpenJDK indirme sayfası ise 21.0.12 Linux x64 artifact'ini yayınlıyordu. CI bu nedenle resmi `aka.ms` artifact'ini ve resmi SHA-256 dosyasını kullanarak exact JDK'yı kurar; katalog gecikmesi sürümün var olmadığı anlamına gelmez.

## Android SDK hattı

Ürün Android-first ve Google Play'e yeni uygulama olarak çıkacağı için release çizgisi API 36'ya sabitlenmiştir.

- Minimum OS / `SupportedOSPlatformVersion`: Android 7.0, API `24`
- Compile SDK: API `36`
- Target SDK: API `36`
- Android SDK Platform 36: revision `1`
- Android SDK Build-Tools: `36.0.0`
- Android SDK Platform-Tools: `37.0.1` stable

Politika gerekçesi:

- .NET 10 API 21–23'ü Mono ile desteklemeye devam etse de .NET 10 proje şablonları desugaring kaynaklı runtime risklerini azaltmak için API 24'ü önerir.
- Google Play, 31 Ağustos 2026'dan itibaren yeni uygulama ve güncellemelerin Android 16 / API 36 veya üzerini target etmesini ister. Proje bu tarihten yalnız yedi gün önce başlatıldığı için API 35'e geçici olarak bağlanmak yerine doğrudan API 36 hedeflenir.
- Android Developers Platform-Tools release notes, `37.0.1` sürümünü Temmuz 2026 stable release olarak listeler. 2026-08-24 CI'da `sdkmanager --channel=0 "platform-tools"` da aynı sürümü çözdü. Bu gerçek kanıt nedeniyle önceki `37.0.0 stable / 37.0.1 Canary` kaydı düzeltilmiştir.

Önerilen Android SDK paketleri:

```text
platforms;android-36
build-tools;36.0.0
platform-tools
```

`platform-tools` paket kurulumundan sonra `adb version` ile `37.0.1` doğrulanmalıdır. Gelecekte SDK Manager daha yeni stable sürüm sunarsa sessiz yükseltme yapılmaz; baseline revizyonu açılır.

## Android command-line tools bootstrap

2026-08-24 tarihli Android Developers stable indirme sayfasında command-line tools arşiv build ID'si `15859902` olarak yayınlanmıştır.

Örnek Linux artifact:

- Dosya: `commandlinetools-linux-15859902_latest.zip`
- SHA-256: `4e4c464f145a7512b57d088ac6c278c03c9eea610886b35a5e0804e74eedf583`

Bu arşiv yalnız `sdkmanager` bootstrap içindir; release artifact dependency'si değildir.

## Fiziksel cihaz kapısı

AŞAMA 01 ancak aşağıdakilerin tamamı gerçek geliştirme makinesi + fiziksel Android cihaz üzerinde kanıtlanırsa `DONE` olabilir:

1. `dotnet --info` exact SDK/workload set'i gösterir.
2. `java -version` JDK 21 hattını gösterir.
3. `adb version` pinlenen stable platform-tools hattını gösterir.
4. Android API 36 platform ve Build-Tools 36.0.0 kurulu görünür.
5. Temiz MAUI smoke app Debug build geçer.
6. Aynı smoke app Release build geçer.
7. `adb devices` fiziksel telefonu `device` olarak görür.
8. Uygulama fiziksel telefona yüklenip açılır.

Bu sohbet çalışma konteynerinde fiziksel telefon yoktur; bu nedenle AŞAMA 01'in cihaz kapısı burada tamamlanamaz.

### Otomatik cihaz-gate scriptleri

Repo, gerçek geliştirme makinesindeki zorunlu kontrolleri aynı sırayla uygulayan iki script içerir:

- Windows / PowerShell: `scripts/stage01-device-gate.ps1`
- Bash: `scripts/stage01-device-gate.sh`

Windows örneği:

```powershell
.\scripts\stage01-device-gate.ps1
```

Bash örneği:

```bash
bash scripts/stage01-device-gate.sh
```

Birden fazla yetkili ADB cihazı bağlıysa hedef açıkça seçilir:

```powershell
$env:ANDROID_SERIAL = '<adb-serial>'
.\scripts\stage01-device-gate.ps1
```

```bash
ANDROID_SERIAL='<adb-serial>' bash scripts/stage01-device-gate.sh
```

Scriptler aşağıdaki durumlarda FAIL verir: exact .NET/JDK/ADB sürümü uyuşmazlığı, eksik API 36/Build-Tools 36.0.0, eksik `maui-android`, `unauthorized/offline` cihaz, emülatör, birden çok belirsiz cihaz, Debug/Release build hatası, manifest 24/36 uyuşmazlığı, APK install veya launcher hatası.

PASS halinde temiz MAUI smoke uygulaması `com.smitelagwar.mobildwg.stage01smoke` kimliğiyle üretilir; Android minimum API 24 açıkça pinlenir; Debug ve Release build edilir; Debug APK fiziksel cihaza kurulur ve launcher `Status: ok` ile açılır. Kanıt çıktısı tam ADB seri numarasını yazmaz.

Bu scriptlerin sözdizimi CI'da doğrulanır; fakat `STAGE01_DEVICE_GATE_PASS` yalnız gerçek fiziksel cihazda çalıştırıldığında AŞAMA 01 kanıtı sayılır.

## iOS envanteri

AŞAMA 01'de yalnız erişim durumu kaydedilir. iOS kurulum/gerçek cihaz işi AŞAMA 08 ve AŞAMA 23'te yapılacaktır. Bu turda Mac/Xcode/iPhone/Apple Developer erişimi kanıtlanmamıştır.

## Resmi kaynaklar — 2026-08-24 snapshot

- .NET 10 download: `https://dotnet.microsoft.com/en-us/download/dotnet/10.0`
- .NET MAUI .NET 10 yenilikleri / Android API 36 + JDK 21 + min API 24: `https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0`
- .NET `global.json`: `https://learn.microsoft.com/dotnet/core/tools/global-json`
- .NET workload install: `https://learn.microsoft.com/dotnet/core/tools/dotnet-workload-install`
- Microsoft OpenJDK download: `https://learn.microsoft.com/java/openjdk/download`
- Android 16 SDK setup: `https://developer.android.com/about/versions/16/setup-sdk`
- Android SDK Platform releases: `https://developer.android.com/tools/releases/platforms`
- Android Platform-Tools releases: `https://developer.android.com/tools/releases/platform-tools`
- Android SDK Manager stable-channel behavior: `https://developer.android.com/tools/sdkmanager`
- Android Studio / command-line tools downloads: `https://developer.android.com/studio/`
- Google Play target API requirements: `https://support.google.com/googleplay/android-developer/answer/11926878`
