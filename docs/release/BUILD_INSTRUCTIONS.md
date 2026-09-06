# Mobil DWG — Build / Release Yönergeleri

Bu belge mevcut Android build hattını özetler. Exact toolchain baseline için `docs/TOOLCHAIN.md` ve `global.json` authoritative kaynaktır.

## Ön gereksinimler

- .NET SDK `10.0.400`
- workload set `10.0.400`
- `maui-android`
- Microsoft OpenJDK 21 (`21.0.12.x` baseline)
- Android SDK Platform 36
- Build-Tools `36.0.0`
- Platform-Tools / ADB

Kontrol:

```powershell
dotnet --version
dotnet --info
dotnet workload list
java -version
adb version
```

Eksik MAUI workload:

```powershell
dotnet workload install maui-android
```

## Restore ve Release build

Android dependency audit projesini locked mode ile doğrula:

```powershell
dotnet restore .\compliance\Stage02.DependencyProbe\Stage02.DependencyProbe.csproj --locked-mode
```

Bu klasör adı tarihsel kökenlidir; proje hâlâ güncel Android dependency graph lock kaynağı olarak kullanılır.

Ana solution:

```powershell
dotnet build .\MobilDwg.sln -c Release
```

## Platform-neutral executable test harness'ları

Bu test projeleri executable harness'tır; `dotnet test` yerine `dotnet run` kullanılır:

```powershell
dotnet run --project .\tests\MobilDwg.Architecture.Tests\MobilDwg.Architecture.Tests.csproj -c Release
dotnet run --project .\tests\MobilDwg.Core.Tests\MobilDwg.Core.Tests.csproj -c Release
dotnet run --project .\tests\MobilDwg.Rendering.Tests\MobilDwg.Rendering.Tests.csproj -c Release
```

Android davranışını değiştiren işler için yalnız host testleri yeterli değildir; `docs/ANDROID_TESTING.md` uygulanır.

## Android AAB

Google Play için unsigned/yerel package build örneği:

```powershell
dotnet build .\src\MobilDwg.App\MobilDwg.App.csproj `
  -c Release `
  -f net10.0-android36.0 `
  -p:AndroidPackageFormat=aab `
  -p:AndroidKeyStore=false
```

Çıktı SDK/MAUI packaging ayrıntısına göre `src/MobilDwg.App/bin/Release/net10.0-android36.0/` altındadır. Dosya adı sabit varsayılmamalı; build sonrasında gerçek artifact doğrulanmalıdır.

## Android APK

```powershell
dotnet build .\src\MobilDwg.App\MobilDwg.App.csproj `
  -c Release `
  -f net10.0-android36.0 `
  -p:AndroidKeyStore=false
```

Artifact boyutu, package ID ve gerekiyorsa install/launch sonucu ayrıca doğrulanır.

## Production signing

Signing secret, parola, private key veya keystore repoya commit edilmez.

```powershell
dotnet build .\src\MobilDwg.App\MobilDwg.App.csproj `
  -c Release `
  -f net10.0-android36.0 `
  -p:AndroidPackageFormat=aab `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="<secure-local-path>" `
  -p:AndroidSigningStorePass="<secret>" `
  -p:AndroidSigningKeyAlias="<alias>" `
  -p:AndroidSigningKeyPass="<secret>"
```

Secret değerleri shell history/log/artifact içine sızdırılmamalıdır.

## Release öncesi minimum kontrol

- exact toolchain ve dependency graph,
- Release build,
- executable Core/Rendering/Architecture harness'ları,
- değiştirilen davranışa özel Android gerçek-app regression,
- package permission/data-safety kontrolü,
- dependency/native/license inventory,
- artifact checksum,
- bilinen limitation'ların güncelliği.

Release APK/AAB ve checksum geçici build artifact'idir; source tree içinde eski binary/checksum kopyaları tutulmaz. Tamamlanmış v1 ayrıntılı kanıtlarına gerekirse Git geçmişinden erişilir.
