# mobil-dwg — Toolchain Baseline

Bu dosya mevcut pinli build hattını tanımlar. Otomatik/floating yükseltme yapılmaz; bir pin değişirse build, Android runtime ve dependency/compliance kontrolleri yeniden çalıştırılır.

## .NET / MAUI

`global.json` authoritative kaynaktır:

- .NET SDK: `10.0.400`
- `rollForward`: `disable`
- prerelease: kapalı
- workload set: `10.0.400`
- aktif workload: `maui-android`

Kurulum kontrolü:

```powershell
dotnet --version
dotnet --info
dotnet workload list
```

Workload kurulumu repo pinine göre yapılır:

```powershell
dotnet workload install maui-android
```

## NuGet production pinleri

`Directory.Packages.props` authoritative kaynaktır:

- ACadSharp `[3.7.1]`
- SkiaSharp `[4.151.1]`
- Microsoft.Maui.Controls `[10.0.100]`
- IxMilia.Dxf `[0.8.4]` yalnız test/fallback adayıdır; production runtime baseline değildir.

Direct dependency sürümleri exact tutulur. Lockfile/locked restore ve lisans/native graph politikası `compliance/` altındadır.

## Java

- Microsoft Build of OpenJDK 21
- pinlenen hat: `21.0.12.x`
- `JAVA_HOME` doğru JDK'yı göstermelidir.

Kontrol:

```powershell
java -version
javac -version
```

## Android

- minimum API: `24`
- compile/target API: `36`
- Build-Tools: `36.0.0`
- Platform-Tools/ADB baseline: `37.0.1`
- test AVD: `mobil-dwg-api36`

Gerekli SDK paketleri:

```text
platforms;android-36
build-tools;36.0.0
platform-tools
```

Yeni Android SDK/Build-Tools/Platform-Tools sürümü görünmesi sessiz yükseltme gerekçesi değildir. Upgrade ayrı change olarak yapılır.

## Yerel ortam kontrolü

Windows geliştirme/test makinesinde:

```powershell
.\scripts\doctor-local-environment.ps1
```

Release build:

```powershell
dotnet build .\MobilDwg.sln -c Release
```

Android emulator/fiziksel cihaz test akışı `docs/ANDROID_TESTING.md` içindedir.

## Dependency yükseltme kuralı

Bir package/toolchain yükseltmesinde en az:

1. exact yeni sürüm,
2. source/package provenance,
3. lisans ve transitive/native graph,
4. locked restore,
5. full Release build,
6. etkilenen parser/render/Android regresyonları,
7. artifact boyutu/bellek/performance etkisi gerekiyorsa ölçüm

kaydedilir.

Sırf `latest` olduğu için yükseltme yapılmaz.

## Platform kapsamı

Aktif build hattı Android'dir. iOS workload/Xcode/signing bağımlılığı kullanıcı iOS'u açıkça yeniden etkinleştirmedikçe Android geliştirme makinesinin zorunlu parçası değildir.
