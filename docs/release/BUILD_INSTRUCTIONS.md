# Mobil DWG — Derleme ve Tekrarlama Yönergeleri (Build Instructions)

Bu belge, **Mobil DWG** projesinin temiz bir ortamda kaynaktan birebir tekrarlanabilir biçimde derlenmesi için gereken ortam gereksinimlerini ve komut adımlarını tanımlar.

---

## 1. Ön Gereksinimler

- **İşletim Sistemi**: Windows 10/11 x64, macOS (Apple Silicon/Intel) veya Linux x64
- **.NET SDK**: `10.0.400` (sabitlenmiş sürüm)
- **.NET MAUI Android Workload**: `maui-android`
- **Android SDK Platform**: API 36 (Android 16) ve Platform-Tools (ADB)
- **Java SDK**: OpenJDK 17 veya 21 (Android derleme aracı için)

İş yükünü doğrulamak ve kurmak için:
```powershell
dotnet workload install maui-android
```

---

## 2. Kilitli Paket Geri Yükleme (Locked Restore)

Bağımlılıkların NuGet üzerinden kilitli ve değişmez sürümleriyle geri yüklenmesi için:
```powershell
dotnet restore compliance/Stage02.DependencyProbe/Stage02.DependencyProbe.csproj --locked-mode
dotnet restore src/MobilDwg.App/MobilDwg.App.csproj
```

---

## 3. Testlerin Çalıştırılması

Tüm platform-neutral mimari ve CAD render sözleşme testlerini koşturmak için:
```powershell
# Mimari ve sınır testleri
dotnet test tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj -c Release

# Core mantık testleri
dotnet test tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj -c Release

# Rendering ve aşama testleri (Stage 04, 09, 10–22, 25, 26)
dotnet run --project tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj -c Release
```

---

## 4. Üretim Android Paketlerinin Derlenmesi

### A. Android App Bundle (AAB — Google Play Yayını İçin)
```powershell
dotnet build src/MobilDwg.App/MobilDwg.App.csproj `
  -c Release `
  -f net10.0-android36.0 `
  -p:AndroidPackageFormat=aab `
  -p:AndroidKeyStore=false
```
Çıktı: `src/MobilDwg.App/bin/Release/net10.0-android36.0/com.smitelagwar.mobildwg.aab`

### B. Bağımsız APK (Doğrudan Cihaza Kurulum / Sideload İçin)
```powershell
dotnet build src/MobilDwg.App/MobilDwg.App.csproj `
  -c Release `
  -f net10.0-android36.0 `
  -p:AndroidKeyStore=false
```
Çıktı: `src/MobilDwg.App/bin/Release/net10.0-android36.0/com.smitelagwar.mobildwg-Signed.apk`

---

## 5. İmzalı Yayın Paketleri (Production Release Signing)

Üretim anahtarıyla imzalamak için `AndroidKeyStore=true` ve keystore parametreleri verilir:
```powershell
dotnet build src/MobilDwg.App/MobilDwg.App.csproj `
  -c Release `
  -f net10.0-android36.0 `
  -p:AndroidPackageFormat=aab `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="path/to/release.keystore" `
  -p:AndroidSigningStorePass="[STORE_PASSWORD]" `
  -p:AndroidSigningKeyAlias="[KEY_ALIAS]" `
  -p:AndroidSigningKeyPass="[KEY_PASSWORD]"
```
