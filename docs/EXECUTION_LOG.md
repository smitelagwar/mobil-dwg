# Execution Log

Bu dosya `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` içindeki aşamaların teknik yürütme kanıtlarını tutar. İnsan ve yeni ajan devri için özet durum kökteki `gecmis.md` dosyasındadır.

## Kayıt kuralları

Her çalışma turunda mümkün olduğunca şu bilgiler eklenir:

- tarih ve aktif aşama,
- başlangıç repo revision/branch,
- incelenen veya değiştirilen dosyalar,
- çalıştırılan komut/test ve kısa sonuç,
- ölçülen ortam/toolchain sürümleri,
- oluşturulan commit/PR veya artifact,
- açık risk/blocker,
- tek somut sonraki eylem.

Başarı kanıtsız yazılmaz. Uzun log kopyalanmaz; gerekli özet ve hata bağlamı tutulur. Hassas dosya yolu, müşteri çizim adı/içeriği, secret veya signing materyali kaydedilmez.

---

## 2026-08-24 — AŞAMA 00: Çalışma alanı ve yürütme zemini

Durum: `DONE`

Başlangıç GitHub durumu:

- Repo: `smitelagwar/mobil-dwg`
- Branch: `main`
- Başlangıç revision: `d161b5c4f9ba238f0d2a2e4c92f773535f379487`
- Başlangıç commit mesajı: `docs: add final mobile DWG project plan`
- Uygulama kodu yok; plan ve araştırma belgeleri mevcut.

Doğrulanan repo öğeleri:

- `.gitignore`
- `README.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan.md`
- `chatgpt_oneriler.md`
- `claude_oneriler.md`
- `gemini_oneriler.md`
- `sonnet_5.md`

`.gitignore` incelemesi:

- .NET/IDE build çıktıları ignore ediliyor.
- secret/signing materyali ignore ediliyor.
- private DWG/DXF ve türetilmiş preview/cache öğeleri ignore ediliyor.
- yalnız açıkça incelenmiş `fixtures/public` CAD fixture istisnası var.
- SHX/PAT/LIN/TTF/OTF varlıkları varsayılan olarak ignore; yalnız `assets/approved/**` istisna.
- Bu nedenle AŞAMA 00 kapsamında `.gitignore` değiştirilmedi.

Çalışma ortamı envanteri (bu ChatGPT çalışma konteyneri; kullanıcının fiziksel geliştirme bilgisayarı olarak yorumlanmamalı):

- Disk: yaklaşık 63 GB toplam, 38 GB boş.
- Git: `2.47.3`.
- `dotnet`: PATH üzerinde yok.
- Java: OpenJDK `21.0.11`.
- `adb`: PATH üzerinde yok.
- `/mnt/data` içinde kullanıcıdan yüklenen nihai plan dosyası bulundu ve GitHub’daki dosyanın Git blob SHA’sı ile birebir eşleşti: `a05dc53df058c5355f8576996a33cce704ac19f3`.

AŞAMA 00’da oluşturulan süreklilik kayıtları:

- `gecmis.md`
- `docs/EXECUTION_LOG.md`
- `docs/ADR/0000-template.md`
- `docs/EVIDENCE_TEMPLATE.md`

Plan checkpoint’i AŞAMA 00 `DONE`, sonraki eylem AŞAMA 01 olacak şekilde güncellenmiştir.

Doğrulama özeti:

- GitHub repo geçmişi ve ana belgeler okundu.
- Kaynak araştırma MD dosyaları değiştirilmedi.
- `.gitignore` mevcut haliyle stage gereksinimlerini karşılıyor.
- Repo sürekliliği için kalıcı log/handoff dosyaları eklendi.

Sonraki eylem: AŞAMA 01’de resmi kaynaklardan .NET 10/MAUI/Android exact toolchain sürümlerini canlı doğrulamak ve gerçek geliştirme ortamında kurulum/build/device smoke hattını başlatmak.

---

## 2026-08-24 — AŞAMA 01: .NET/MAUI/Android toolchain ve gerçek telefon

Durum: `BLOCKED`

Başlangıç revision:

- `fe3c8c043e6d373e6313d2e1201cc24992b493a9`
- AŞAMA 00 tamamlanmış, uygulama kodu henüz yoktu.

Canlı doğrulanan toolchain baseline:

- .NET SDK `10.0.400` / runtime servicing `10.0.11` — 2026-08-11 release.
- Workload set `10.0.400`; Android-first workload `maui-android`.
- Microsoft Build of OpenJDK `21.0.12` LTS.
- Android minimum API `24` — proje tarafından açıkça pinlenir; temiz MAUI 10 şablonunun varsayılanı API 21'dir.
- Android compile/target API `36`.
- Android SDK Platform 36 revision `1`.
- Android SDK Build-Tools `36.0.0`.
- Android SDK Platform-Tools `37.0.1` stable.
- Android command-line tools stable download build ID `15859902`.
- Google Play: 2026-08-31 itibarıyla yeni uygulama ve güncellemeler target API 36 veya üzeri olmak zorunda.

Repo değişiklikleri:

- `global.json`: SDK `10.0.400`, workload set `10.0.400`, prerelease kapalı, `rollForward=disable`.
- `docs/TOOLCHAIN.md`: exact toolchain, resmi kaynak snapshot'ı ve fiziksel cihaz kapısı.
- `docs/evidence/STAGE_01.md`: toolchain/build kanıtları ile eksik gerçek cihaz kanıtlarının ayrımı.
- `.github/workflows/stage01-toolchain-smoke.yml`: temiz hosted-runner üzerinde exact toolchain + MAUI Android Debug/Release smoke kapısı.

İlk stage commitleri:

- `15d69e6b5b9e0c20f5ef7b0a742ac25ce5cc9071` — `build: pin .NET 10.0.400 toolchain`
- `467a2fe69366bfc640400d4b2ccbd97309b09189` — `docs: record stage 01 toolchain baseline`
- `658345321d1a76f7f3a9f6e6958e62a6868415a0` — `docs: add stage 01 evidence and blocker`
- `a99ba8d26047598a1b593f864e14769da0980dda` — `docs: log stage 01 toolchain verification`
- `549254b751c87155af5bf5e40cd609d3fe57b710` — `docs: correct Platform-Tools stable baseline`

ChatGPT konteyneri gözlemi:

- Linux x86_64.
- Java `21.0.11` mevcut.
- `dotnet` ve `adb` PATH üzerinde yok.
- .NET 10.0.400 resmi direct-download URL'si doğrulandı; konteyner dış DNS erişimi kapalı olduğundan doğrudan indirme denemesi `Could not resolve host: builds.dotnet.microsoft.com` ile başarısız oldu.
- Bu hata toolchain uyumsuzluğu değildir; ChatGPT çalışma konteynerinin network kısıtıdır.

### GitHub Actions CI smoke doğrulaması

AŞAMA 01'in fiziksel cihazdan bağımsız build/toolchain kısmı GitHub Actions üzerinde temiz Ubuntu runner ile gerçek olarak çalıştırıldı.

CI geliştirme commitleri:

- `075ce5268f3cc3272be0f349f67e6f0237a261a3` — manifest doğrulamasını deterministik yola çevirdi.
- `4430542558f7f3751cd62f224497e400c1e80415` — Android minimum API 24'ü smoke csproj'de açıkça pinledi.
- `49c3e3f2f855c1f7f1cf945049cc5d93805e7003` — exact Microsoft JDK artifact indirmesine retry ekledi, checksum doğrulamasını korudu.
- PR #1 final olarak `83379b24e4ba87f04299f612ae2951ae8d8aec13` merge commit'i ile `main`e alındı.

Ara koşular ve bulgular:

- Run #6 / `32735646108`: .NET/JDK/Android SDK/workload/template/Debug/Release PASS. Manifest adımı `find | head` + `pipefail` nedeniyle script-level `Broken pipe` ile düştü.
- Run #7 / `32736408762`: toolchain + Debug/Release PASS; gerçek üretilen manifest temiz MAUI şablonunun `minSdk=21`, `targetSdk=36` kullandığını gösterdi. Proje policy'si gereği minimum API 24 explicit pinlendi.
- Run #8 / `32737204387`: Microsoft JDK tar indirmesi geçici ağ kesintisiyle `curl (18)` verdi. Resmi exact artifact + SHA256 doğrulaması korunarak retry eklendi.
- Run #9 / `32737334339`: bütün adımlar `SUCCESS`.

Final run #9 ölçülen değerler:

- Runner: Ubuntu 24.04 x64.
- .NET SDK: `10.0.400`.
- Runtime: `10.0.11`.
- JDK: Microsoft OpenJDK `21.0.12`; resmi checksum kontrolü `OK`.
- Platform-Tools/ADB: `37.0.1-15733141`.
- Android SDK Platform: API `36`.
- Android Build-Tools: `36.0.0`.
- `maui-android` workload installation: `SUCCESS`; workload version `10.0.400`.
- MAUI workload manifest: `10.0.20/10.0.100`.
- Microsoft.NET.Sdk.Android workload manifest: `36.1.69`.
- Smoke csproj Android `SupportedOSPlatformVersion`: `24.0`.
- Debug build: `Build succeeded`, 0 warning, 0 error.
- Release build: `Build succeeded`, 0 warning, 0 error.
- Generated manifest: `<uses-sdk android:minSdkVersion="24" android:targetSdkVersion="36" />`.
- APK artifact upload: `SUCCESS`.
- Artifact ID: `9523977201`.
- Artifact size: `57,601,187` bytes.
- Artifact ZIP SHA-256: `3fd12ffe750352e9ace5532eaffa8f1cd6619da449bddeb05efb5acfc91dcd41`.

CI sonucu: exact toolchain + workload + Debug + Release + API baseline + APK üretimi PASS.

### Fiziksel cihaz gate otomasyonu

Gerçek telefon erişimi bu oturumda yok olduğundan sahte device PASS üretmek yerine, AŞAMA 01'in kalan Android kapısı repo içinde tekrar üretilebilir hale getirildi.

Eklenen/sertleştirilen öğeler:

- `scripts/stage01-device-gate.sh`
- `scripts/stage01-device-gate.ps1`
- `.github/workflows/stage01-toolchain-smoke.yml` içinde Bash + PowerShell parse/syntax kontrolü.
- `docs/TOOLCHAIN.md` içinde Windows/Bash çalıştırma runbook'u.
- Smoke app sabit kimliği: `com.smitelagwar.mobildwg.stage01smoke`.

Gate scriptlerinin zorunlu doğrulamaları:

- .NET SDK exact `10.0.400`.
- `maui-android` mevcut ve workload set `10.0.400` ile çözülüyor.
- Microsoft OpenJDK exact `21.0.12`.
- ADB / Platform-Tools `37.0.1`.
- Android API 36 ve Build-Tools `36.0.0`.
- Yetkili ADB state tam olarak `device`; emulator kabul edilmez.
- Birden çok cihaz varsa `ANDROID_SERIAL` ile açık hedef seçimi gerekir.
- Temiz MAUI app oluşturulur; min API 24 ve sabit ApplicationId pinlenir.
- Debug + Release build geçer.
- Manifest minSdk 24 / targetSdk 36 doğrulanır.
- Debug APK fiziksel telefona kurulur.
- Launcher `adb shell am start -W` ile `Status: ok` üretir.
- PASS özetinde tam ADB seri numarası yazılmaz.

Doğrulama PR'ı:

- PR #2: `ci: verify stage 01 physical device gate`.
- Final head: `9e2c0f71153ca0db936c19a10d2f53dc38cca7ec`.
- GitHub Actions run #17 / `32739952628`: `SUCCESS`.
- Script parse gate: `SUCCESS`.
- Exact toolchain/workload: `SUCCESS`.
- Pinned smoke project creation: `SUCCESS`.
- Debug build: `SUCCESS`.
- Release build: `SUCCESS`.
- Manifest/API/package gate: `SUCCESS`.
- Artifact upload: `SUCCESS`.
- Artifact ID: `9524964656`.
- Artifact size: `57,817,776` bytes.
- Artifact SHA-256: `cfd2221a9a31193c76b4347f633ec062d54abca5117edea887bc46a0926f6d0f`.
- PR #2 merge commit: `9b375af9931a3db23f82e9b983257f29030a7376`.

Bu CI koşusu fiziksel telefon testini ikame etmez. Scriptlerin parse/build entegrasyonu kanıtlanmıştır; `STAGE01_DEVICE_GATE_PASS` hâlâ gerçek geliştirme makinesi ve fiziksel Android cihaz üzerinde alınmalıdır.

### iOS erişim envanteri standardizasyonu

AŞAMA 01'in iOS maddesi kurulum değil yalnız erişim envanteridir. Sohbetten Mac/Xcode/iPhone/Apple Developer erişimi tahmin edilmemesi için repo içine standart kayıt ve helper eklendi:

- `docs/STAGE_01_IOS_ACCESS_INVENTORY.md`
- `scripts/stage01-ios-inventory.sh`

Helper yalnız secretsiz özet üretir: macOS/Xcode erişimi, Xcode sürümü, fiziksel iPhone sayısı, code-signing identity sayısı ve kullanıcının manuel Apple Developer `yes/no` teyidi. Apple ID/e-posta, Team ID, UDID/seri, provisioning profile, certificate private key veya token yazmaz. İncelemede `xcode-select -p` developer path çıktısı da kullanıcı yolu içerebilme ihtimali nedeniyle kaldırıldı.

Aynı turda `docs/TOOLCHAIN.md` içindeki eski `dotnet workload install maui-android --version 10.0.400` komutu düzeltildi. Exact workload set'i `global.json` içindeki `workloadVersion: 10.0.400` seçer; CI'da kanıtlanan kurulum komutu `dotnet workload install maui-android`dır.

Doğrulama:

- PR #3: `docs: standardize stage 01 iOS access inventory`.
- Final head: `3f859b537afcfb7bc792931754bd9714467d84bc`.
- GitHub Actions run #20 / `32742123997`: `SUCCESS`.
- Üç Stage 01 helper script syntax gate: `SUCCESS`.
- Exact Android toolchain/workload regresyonu: `SUCCESS`.
- Debug build: `SUCCESS`.
- Release build: `SUCCESS`.
- Manifest/API/package gate: `SUCCESS`.
- Artifact upload: `SUCCESS`.
- Artifact ID: `9525825066`.
- Artifact size: `57,817,944` bytes.
- Artifact SHA-256: `bf66fd4c3e7a4b1f6a40ed7c2fd868f298f7087a95f9c29cdfbb8aa82e7f1115`.
- PR #3 merge commit: `9a397065a55c5844993e6ef909438f44ad5aa1f6`.

Gerçek iOS erişim alanları hâlâ `UNKNOWN`; helper'ın varlığı erişimin var olduğu anlamına gelmez.

### Eksik zorunlu AŞAMA 01 kanıtları

- Kullanıcının gerçek geliştirme makinesinde pinli toolchain'in yerel doğrulanması.
- Fiziksel Android cihazın `adb devices` çıktısında `device` olması.
- Smoke app'in fiziksel telefona install edilmesi.
- Smoke app'in fiziksel telefonda launch edilmesi.
- `docs/STAGE_01_IOS_ACCESS_INVENTORY.md` içindeki Mac/Xcode/iPhone/Apple Developer alanlarının gerçek `YES/NO/N/A` değerleriyle tamamlanması.

Blocker:

- Bu oturumda kullanıcının gerçek geliştirme makinesine ve fiziksel Android cihazına USB/ADB erişimi yok. Gerçek Mac/Xcode/iPhone/Apple Developer erişimi de bilinmiyor. Nihai plan fiziksel Android cihaz çalıştırmasını zorunlu tuttuğundan AŞAMA 01 `DONE` sayılamaz ve AŞAMA 02 başlatılamaz.

Sonraki eylem: Gerçek geliştirme makinesinde repo kökünden `scripts/stage01-device-gate.ps1` veya `scripts/stage01-device-gate.sh` çalıştır ve `STAGE01_DEVICE_GATE_PASS` çıktısını kaydet. Ardından `docs/STAGE_01_IOS_ACCESS_INVENTORY.md` içindeki erişim alanlarını gerçek bilgiyle kapat; erişilebilir Mac varsa `scripts/stage01-ios-inventory.sh` kullanılabilir. Bu dış kanıtlar tamamlanmadan AŞAMA 02'ye geçme.
