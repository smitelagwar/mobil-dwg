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

Dependency audit projesini locked mode ile doğrula:

```powershell
dotnet restore .\compliance\Stage02.DependencyProbe\Stage02.DependencyProbe.csproj --locked-mode
```

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

Android davranışını değiştiren işler için yalnız bu host testleri yeterli değildir; `docs/ANDROID_TESTING.md` uygulanır.

## Android AAB

Google Play için unsigned/yerel package build örneği:

```powershell
dotnet build .\src\MobilDwg.App\MobilDwg.App.csproj `
  -c Release `
  -f net10.0-android36.0 `
  -p:AndroidPackageFormat=aab `
  -p:AndroidKeyStore=false
```

Çıktı yolu SDK/MAUI packaging ayrıntısına göre `src/MobilDwg.App/bin/Release/net10.0-android36.0/` altındadır. Dosya adı sabit varsayılmamalı; üretilen artifact build sonrasında gerçek dizinden doğrulanmalıdır.

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

Örnek:

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
- değiştirilen davranışa özel Android gate,
- package permission/data-safety kontrolü,
- dependency/native/license inventory,
- artifact checksum,
- bilinen limitation'ların güncelliği.

Tarihsel v1 package/evidence kayıtları `docs/evidence/` ve `release/` altında korunur; yeni build onların byte-for-byte aynısı varsayılmaz.
