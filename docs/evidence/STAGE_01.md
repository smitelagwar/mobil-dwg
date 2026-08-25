# AŞAMA 01 Evidence — .NET/MAUI/Android toolchain ve gerçek telefon

> **Tarihsel kayıt:** Bu dosya AŞAMA 01'in o tarihteki sonucunu korur. 25.08.2026 itibarıyla aktif Android tekrar turu `ANDROID_DOGRULAMA_PLANI.md` V01'dir; iOS envanteri future option, fiziksel Android ise release cihaz kapısıdır. Aşağıdaki eski “sonraki aşama başlatılamaz” ifadeleri güncel execution override'dan önceki anı anlatır.

Tarih: 2026-08-24

Durum: `BLOCKED`

AŞAMA 01 tamamen bitmiş değildir. Exact toolchain hattı, temiz MAUI Android Debug/Release build kapısı, fiziksel Android cihaz kapısını çalıştıracak repo scriptleri ve iOS erişim envanteri yardımcıları GitHub Actions üzerinde doğrulandı; fakat nihai planın zorunlu gerçek geliştirme makinesi + fiziksel Android cihaz install/launch kapısı ve gerçek iOS erişim envanteri bu oturumda gerçekleştirilemedi.

## Tamamlanan bağımsız işler

- [x] Güncel .NET 10 SDK servicing hattı doğrulandı: SDK `10.0.400`, runtime `10.0.11`, release `2026-08-11`.
- [x] Repo kökünde exact SDK/workload-set çözümlemesi `global.json` ile pinlendi: `10.0.400`, `rollForward=disable`, prerelease kapalı.
- [x] Microsoft Build of OpenJDK `21.0.12` LTS exact artifact ile kuruldu ve resmi SHA-256 dosyasıyla doğrulandı.
- [x] Android command-line tools bootstrap build `15859902` kullanıldı.
- [x] Android SDK Platform API `36`, Build-Tools `36.0.0` ve stable Platform-Tools `37.0.1` kuruldu/doğrulandı.
- [x] `maui-android` workload, `global.json` tarafından seçilen workload version `10.0.400` ile başarıyla kuruldu.
- [x] `docs/TOOLCHAIN.md` içindeki eski/yanlış `dotnet workload install maui-android --version 10.0.400` komutu kaldırıldı; CI'da doğrulanmış hat `global.json` workload set pinine dayanarak `dotnet workload install maui-android` olarak kaydedildi.
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
- [x] iOS erişim envanteri için `docs/STAGE_01_IOS_ACCESS_INVENTORY.md` ve `scripts/stage01-ios-inventory.sh` eklendi.
- [x] iOS helper Apple hesabına login olmaz; Apple ID/e-posta, Team ID, UDID/seri numarası, certificate private key, provisioning profile veya token yazmaz. Yalnız macOS/Xcode erişimi, Xcode sürümü, fiziksel iPhone sayısı, code-signing identity sayısı ve kullanıcının manuel Apple Developer `yes/no` teyidini özetler.
- [x] iOS helper'dan kullanıcı yolu içerebilecek `xcode-select -p` çıktısı kaldırıldı.
- [x] Bash/PowerShell Android gate scriptleri ve Bash iOS inventory helper parse/syntax kontrolü Stage 01 CI workflow'una eklendi ve PASS oldu.
- [x] Device-gate ve iOS inventory runbook'ları `docs/TOOLCHAIN.md` içine eklendi.
- [x] Device-gate otomasyonu PR #2; iOS inventory standardizasyonu PR #3 üzerinden `main` branch'e merge edildi.

## Güncel nihai CI kanıtı

GitHub Actions:

- Workflow: `Stage 01 Toolchain Smoke`
- Run: `32742123997` / run #20
- Sonuç: `SUCCESS`
- PR: `#3` — `docs: standardize stage 01 iOS access inventory`
- PR head commit: `3f859b537afcfb7bc792931754bd9714467d84bc`
- Main merge commit: `9a397065a55c5844993e6ef909438f44ad5aa1f6`
- Stage 01 script syntax gate: `SUCCESS` (`stage01-device-gate.sh`, `stage01-device-gate.ps1`, `stage01-ios-inventory.sh`)
- Runner: Ubuntu 24.04 x64
- .NET SDK: `10.0.400`
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
- Artifact ID: `9525825066`
- Artifact name: `stage01-maui-android-smoke`
- Artifact size: `57,817,944` bytes
- Artifact ZIP SHA-256: `bf66fd4c3e7a4b1f6a40ed7c2fd868f298f7087a95f9c29cdfbb8aa82e7f1115`

Önceki run #17 / `32739952628` fiziksel Android cihaz gate otomasyonunu doğrulayan tarihsel kanıttır; run #20 iOS inventory helper, workload komut düzeltmesi ve mevcut Android regresyon hattını birlikte doğrulayan en güncel CI kanıtıdır.

## CI sırasında yakalanan ve çözülen sorunlar

- Run #6: manifest testindeki `find ... | head -n 1` zinciri `set -o pipefail` altında `Broken pipe` üretti. Build başarısız değildi; doğrulama scripti deterministik manifest yoluna çevrildi.
- Run #7: gerçek üretilen manifest `minSdkVersion=21 / targetSdkVersion=36` gösterdi. Test gevşetilmedi; proje kararı API 24 minimum olduğu için smoke csproj açıkça `24.0` pinlendi.
- Run #8: Microsoft JDK artifact indirmesi `curl (18)` ile yarıda kesildi. Exact resmi artifact ve checksum korunarak retry eklendi.
- Run #9: bütün toolchain, workload, Debug/Release build, API 24/36 manifest ve artifact upload adımları PASS.
- Run #17: fiziksel cihaz gate scriptlerinin parse kontrolü, pinned `ApplicationId`, exact toolchain, workload, Debug/Release build, 24/36 manifest/package doğrulaması ve artifact upload birlikte PASS.
- PR #3 incelemesi: iOS helper başlangıçta `xcode-select -p` developer path'ini yazıyordu. Kullanıcı yolu içerebilme ihtimali nedeniyle bu çıktı merge öncesi kaldırıldı.
- Run #20: üç Stage 01 helper scriptinin syntax gate'i ve mevcut exact Android toolchain/build/manifest hattı birlikte PASS.

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
- [ ] `docs/STAGE_01_IOS_ACCESS_INVENTORY.md` içindeki Mac/Xcode/iPhone/Apple Developer erişim alanlarının gerçek `YES/NO/N/A` değerleriyle doldurulması.

## Blocker

Bu oturumda kullanıcının gerçek geliştirme makinesine veya fiziksel Android telefonuna USB/ADB erişimi yok. Gerçek Mac/Xcode/iPhone/Apple Developer erişim durumu da bilinmiyor. CI build/packaging ve helper script entegrasyonunu doğrulasa da fiziksel cihaz veya kullanıcı erişimi yerine geçmez. Nihai plan AŞAMA 01 çıkış kriteri gerçek Android telefonda uygulamanın çalışmasını zorunlu tuttuğundan `DONE` yazılamaz ve AŞAMA 02 başlatılamaz.

## Sonraki somut eylem

Gerçek geliştirme makinesinde repo kökünden işletim sistemine uygun Android gate'i çalıştır:

```powershell
.\scripts\stage01-device-gate.ps1
```

veya:

```bash
bash scripts/stage01-device-gate.sh
```

Çıktıda `STAGE01_DEVICE_GATE_PASS`, `device_state=device,physical`, `debug_build=PASS`, `release_build=PASS`, `install=PASS`, `launch=PASS` görülmeden fiziksel cihaz kapısı kapanmaz.

Ardından iOS erişim envanterini gerçek bilgiyle kapat. Erişilebilir Mac varsa:

```bash
APPLE_DEVELOPER_ACCESS=yes bash scripts/stage01-ios-inventory.sh
```

veya erişim yoksa `APPLE_DEVELOPER_ACCESS=no`. Mac yoksa `docs/STAGE_01_IOS_ACCESS_INVENTORY.md` manuel olarak gerçek `YES/NO/N/A` değerleriyle doldurulur. Bu iki dış kanıt tamamlandığında AŞAMA 01 checkpoint'i yeniden değerlendirilir.
