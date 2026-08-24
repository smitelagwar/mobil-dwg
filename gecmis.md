# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya, yeni bir sohbet veya yeni bir yapay zeka oturumu başladığında projenin nerede kaldığını hızlı ve güvenilir biçimde anlamak için tutulur. Sohbet belleğine güvenilmez; repo içindeki dosyalar tek kalıcı kaynak olarak kabul edilir.

## Yeni bir ajan önce ne okumalı?

Sıra değişmez:

1. `gecmis.md` — nerede kaldık, ne yapıldı, neden yapıldı.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` — tek yetkili uygulama/yürütme planı ve checkpoint.
3. `docs/EXECUTION_LOG.md` — komut, test, revision ve teknik kanıt geçmişi.
4. `docs/TOOLCHAIN.md` — pinlenmiş .NET/MAUI/Android geliştirme zinciri ve cihaz gate runbook'u.
5. `docs/evidence/` — aşama bazlı gerçek kanıt ve blocker kayıtları.
6. Gerekirse `docs/ADR/` — mimari/teknoloji kararlarının gerekçeleri.
7. Diğer `*_oneriler.md`, `Master_Plan.md` ve benzeri dosyalar yalnız araştırma/önceki görüş kaynağıdır; nihai planla çelişirse nihai plan geçerlidir.

## Repo kimliği

- GitHub: `smitelagwar/mobil-dwg`
- Repo adı: `mobil-dwg`
- Varsayılan branch: `main`
- Görünürlük: private
- Ürün: Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer
- v1: viewer-only; edit/save/export/cloud/account kapsam dışında

## Aktif checkpoint

```text
CURRENT_STAGE: AŞAMA 01
STATUS: BLOCKED
NEXT_STAGE: AŞAMA 01 içinde devam
NEXT_ACTION: Gerçek geliştirme makinesinde repo kökünden scripts/stage01-device-gate.ps1 veya scripts/stage01-device-gate.sh çalıştır; STAGE01_DEVICE_GATE_PASS kanıtını al; ardından Mac/Xcode/iPhone/Apple Developer erişim envanterini tamamla.
BLOCKERS: Bu sohbet oturumunda gerçek geliştirme makinesi ve fiziksel Android telefona USB/ADB erişimi yok; Mac/Xcode/iPhone/Apple Developer erişimi de doğrulanmadı.
LAST_VERIFIED_CI: GitHub Actions run 32739952628 SUCCESS; artifact 9524964656 sha256:cfd2221a9a31193c76b4347f633ec062d54abca5117edea887bc46a0926f6d0f; PR #2 merge 9b375af9931a3db23f82e9b983257f29030a7376
LAST_UPDATE: 2026-08-24
```

Önemli yürütme kuralı: Bir kullanıcı turunda en fazla bir aşama tamamlanır. AŞAMA 01 `BLOCKED` olduğundan sonraki `devam` isteğinde AŞAMA 02'ye geçilmez; yalnız AŞAMA 01'in eksik gerçek ortam/cihaz kanıtları tamamlanır.

## Aşama durumu

- [x] AŞAMA 00 — Çalışma alanı ve yürütme zemini
- [ ] AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon — `BLOCKED`
- [ ] AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler
- [ ] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi
- [ ] AŞAMA 04 — Minimal solution ve mimari sınırlar
- [ ] AŞAMA 05 — ACadSharp headless parser spike
- [ ] AŞAMA 06 — Android güvenli dosya alma ve parse spike
- [ ] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO
- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke
- [ ] AŞAMA 09 — RenderScene, kamera ve diagnostics temeli
- [ ] AŞAMA 10 — P0 temel geometri renderer’ı
- [ ] AŞAMA 11 — Mobil viewport ve gesture’lar
- [ ] AŞAMA 12 — Block/INSERT/attribute dönüşümleri
- [ ] AŞAMA 13 — Layer, renk, linetype ve lineweight
- [ ] AŞAMA 14 — TEXT/MTEXT, Türkçe, font ve SHX
- [ ] AŞAMA 15 — Dimension, leader ve hatch doğruluğu
- [ ] AŞAMA 16 — Model space, layout, paper space ve viewport
- [ ] AŞAMA 17 — XREF/raster/underlay ve compatibility raporu
- [ ] AŞAMA 18 — Tam Android viewer UX ve lifecycle
- [ ] AŞAMA 19 — Kötü niyetli/bozuk dosya ve resource guard’ları
- [ ] AŞAMA 20 — Ölçümlü performans ve bellek optimizasyonu
- [ ] AŞAMA 21 — Android tam corpus regresyon ve beta kapısı
- [ ] AŞAMA 22 — Android Release/AAB/compliance RC
- [ ] AŞAMA 23 — iOS toolchain, shared core ve ilk gerçek cihaz
- [ ] AŞAMA 24 — iOS fidelity, lifecycle ve Release archive
- [ ] AŞAMA 25 — Cross-platform beta ve yalnız blocker düzeltmeleri
- [ ] AŞAMA 26 — Dependency freeze, final audit ve RC onayı
- [ ] AŞAMA 27 — v1 artifact, yayın/handoff ve kapanış

## 2026-08-24 — Başlangıçta repo nasıldı?

GitHub’daki ilk doğrulanan proje revision’ı:

- `d161b5c4f9ba238f0d2a2e4c92f773535f379487`
- Commit: `docs: add final mobile DWG project plan`

Bu revision’da uygulama kaynak kodu yoktu. Repo planlama/teknik doğrulama durumundaydı. Aşağıdaki ana belgeler zaten mevcuttu:

- `.gitignore`
- `README.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan.md`
- `chatgpt_oneriler.md`
- `claude_oneriler.md`
- `gemini_oneriler.md`
- `sonnet_5.md`

Nihai planın GitHub blob SHA’sı `a05dc53df058c5355f8576996a33cce704ac19f3` idi. Kullanıcının bu sohbete yüklediği nihai plan dosyasının Git blob SHA’sı da aynı çıktı; yani başlangıçta yerel yüklenen plan ile GitHub’daki nihai plan birebir eşleşiyordu.

## 2026-08-24 — AŞAMA 00’da ne yapıldı?

AŞAMA 00 hedefi izlenebilir yürütme zemini kurmaktı. Gerçek repo incelendiğinde Git deposunun zaten mevcut olduğu görüldü; bu nedenle yeniden `git init` gibi yapay veya destructive işlem yapılmadı.

Yapılanlar:

- GitHub repo, `main` branch ve başlangıç commit’i doğrulandı.
- Mevcut kaynak/araştırma MD belgeleri korundu.
- `.gitignore` tek tek incelendi. Build/IDE çıktıları, secret/signing materyali, private CAD corpus/cache, lisansı ayrıca onaylanmamış font ve CAD asset’lerini ignore ettiği görüldü. Yeterli olduğu için sırf değişiklik üretmek amacıyla düzenlenmedi.
- `docs/EXECUTION_LOG.md` oluşturuldu.
- `docs/ADR/0000-template.md` oluşturuldu.
- `docs/EVIDENCE_TEMPLATE.md` oluşturuldu.
- Bu `gecmis.md` oluşturuldu.
- README ve nihai plan checkpoint’i devir mekanizmasını gösterecek şekilde güncellendi.

### AŞAMA 00 ortam envanteri

Bu değerler ChatGPT’nin 24 Ağustos 2026 tarihli çalışma konteynerine aittir; kullanıcının fiziksel geliştirme bilgisayarı olduğu varsayılmaz:

- Disk: yaklaşık 63 GB toplam, 38 GB boş.
- Git: `2.47.3`.
- `dotnet`: PATH üzerinde yok.
- Java: OpenJDK `21.0.11`.
- `adb`: PATH üzerinde yok.

Eski plan içindeki 24.08.2026 “yerel ortam fotoğrafı” ile bu konteyner arasında fark vardır. Gerçek durum esas alınmıştır. Fiziksel geliştirme ortamı ayrıca ölçülmeden bu konteyner değerleri kurulum kanıtı sayılmaz.

### AŞAMA 00 commit geçmişi

- `1619f043af3c0087794f171b1a0baeb53124685a` — `docs: add stage 00 execution log`
- `a055ab145e5614180b3c28cd307d7563628a515b` — `docs: add ADR template`
- `52a575e50c17dfdd96e91710f27450c870f74a70` — `docs: add evidence template`
- `9d4974c23aa481eaf4b06cb0157069779d84fd88` — `docs: add durable project handoff history`
- `854de13a592331c48c7ce6ac7f03eb248916b0f6` — `docs: link project handoff and stage status`
- `fe3c8c043e6d373e6313d2e1201cc24992b493a9` — `docs: mark stage 00 complete in canonical plan`

## 2026-08-24 — AŞAMA 01’de ne yapıldı?

AŞAMA 01'in canlı doğrulama, repo pinleme, fiziksel cihazdan bağımsız CI build ve gerçek cihaz gate otomasyonu tamamlandı. Nihai gerçek telefon çıkış kapısı tamamlanamadı.

### Canlı doğrulanan baseline

- .NET SDK: `10.0.400`.
- .NET runtime servicing: `10.0.11`.
- .NET release tarihi: `2026-08-11`.
- Workload set: `10.0.400`.
- Android-first MAUI workload: `maui-android`.
- JDK: Microsoft Build of OpenJDK `21.0.12` LTS.
- Android minimum: API `24` — proje explicit pin; temiz MAUI 10 template varsayılanı API 21.
- Android compile/target: API `36`.
- Android SDK Platform 36: revision `1`.
- Android Build-Tools: `36.0.0`.
- Android Platform-Tools: `37.0.1` stable.
- Android command-line tools stable bootstrap build ID: `15859902`.
- Google Play yeni app/update target API 36 zorunluluğu: `2026-08-31`.

### Repo değişiklikleri

- `global.json`: exact `.NET SDK 10.0.400` + workload set `10.0.400`, `rollForward=disable`, prerelease kapalı.
- `docs/TOOLCHAIN.md`: exact toolchain, Android policy seçimi, doğrulama komutları ve fiziksel cihaz gate runbook'u.
- `docs/evidence/STAGE_01.md`: tamamlanan doğrulamalar ile eksik gerçek cihaz kanıtlarının ayrımı.
- `docs/EXECUTION_LOG.md`: AŞAMA 01 teknik logu.
- `.github/workflows/stage01-toolchain-smoke.yml`: temiz runner üzerinde exact toolchain + Debug/Release + manifest/API/package gate ve device-script syntax kontrolü.
- `scripts/stage01-device-gate.sh`: Bash gerçek fiziksel cihaz gate'i.
- `scripts/stage01-device-gate.ps1`: Windows/PowerShell gerçek fiziksel cihaz gate'i.

### AŞAMA 01 ana commitleri

- `15d69e6b5b9e0c20f5ef7b0a742ac25ce5cc9071` — `build: pin .NET 10.0.400 toolchain`
- `467a2fe69366bfc640400d4b2ccbd97309b09189` — `docs: record stage 01 toolchain baseline`
- `658345321d1a76f7f3a9f6e6958e62a6868415a0` — `docs: add stage 01 evidence and blocker`
- `a99ba8d26047598a1b593f864e14769da0980dda` — `docs: log stage 01 toolchain verification`
- `549254b751c87155af5bf5e40cd609d3fe57b710` — Platform-Tools stable baseline düzeltmesi.
- `075ce5268f3cc3272be0f349f67e6f0237a261a3` — deterministik manifest doğrulaması.
- `4430542558f7f3751cd62f224497e400c1e80415` — Android minimum API 24 pin.
- `49c3e3f2f855c1f7f1cf945049cc5d93805e7003` — exact JDK indirme retry hardening.
- `83379b24e4ba87f04299f612ae2951ae8d8aec13` — PR #1 final CI workflow merge commit.
- `c89dd380df75dffdc2857e52d36dc7cd416e813f` / `11dd1cbcb063b50d377ecf512556847c71a8b354` — ilk Bash/PowerShell physical-device gate scriptleri.
- `449f1c4a1cd0dc3e44ede5dd271b6b4a1f1df61f` — CI'da device-gate syntax kontrolü + pinned smoke ApplicationId.
- `9e2c0f71153ca0db936c19a10d2f53dc38cca7ec` — PR #2 final head; device gate exact workload-set kontrolü.
- `9b375af9931a3db23f82e9b983257f29030a7376` — PR #2 physical-device gate automation merge commit.

### GitHub Actions kanıtı

İlk final başarılı build koşusu:

- Workflow: `Stage 01 Toolchain Smoke`
- Run: `32737334339` / #9
- Sonuç: `SUCCESS`
- .NET SDK `10.0.400`, runtime `10.0.11`.
- Microsoft OpenJDK `21.0.12`; resmi artifact SHA-256 doğrulaması PASS.
- Android API 36 + Build-Tools `36.0.0` + Platform-Tools/ADB `37.0.1-15733141` PASS.
- `maui-android` workload version `10.0.400` PASS.
- Smoke project Android `SupportedOSPlatformVersion=24.0`.
- Debug build: 0 warning, 0 error.
- Release build: 0 warning, 0 error.
- Generated manifest: `minSdkVersion=24`, `targetSdkVersion=36`.
- APK artifact ID `9523977201`; size `57,601,187` bytes; ZIP SHA-256 `3fd12ffe750352e9ace5532eaffa8f1cd6619da449bddeb05efb5acfc91dcd41`.

Güncel device-gate dahil final CI koşusu:

- Workflow: `Stage 01 Toolchain Smoke`.
- Run: `32739952628` / #17.
- Sonuç: `SUCCESS`.
- Bash + PowerShell gate parse kontrolü PASS.
- .NET SDK `10.0.400`, JDK `21.0.12`, Platform-Tools `37.0.1`, Android API 36, Build-Tools `36.0.0`, `maui-android` workload PASS.
- Smoke `ApplicationId`: `com.smitelagwar.mobildwg.stage01smoke`.
- Debug build PASS.
- Release build PASS.
- Manifest API/package gate PASS.
- Artifact ID `9524964656`; size `57,817,776` bytes; SHA-256 `cfd2221a9a31193c76b4347f633ec062d54abca5117edea887bc46a0926f6d0f`.

Önemli bulgu: temiz .NET MAUI 10 şablonu Android minimumunu API 21 üretir. Projenin API 24 kararı bu nedenle `.csproj` seviyesinde açıkça pinlenmelidir; yalnız template varsayımına güvenilmez.

### Fiziksel cihaz gate'i nasıl kapanacak?

Gerçek geliştirme makinesinde repo kökünden işletim sistemine uygun script çalıştırılır:

```powershell
.\scripts\stage01-device-gate.ps1
```

veya:

```bash
bash scripts/stage01-device-gate.sh
```

Script exact SDK/workload/JDK/ADB/Android SDK baseline'ını, fiziksel `state=device` hedefini, emülatör dışlamasını, temiz MAUI Debug+Release build'i, manifest 24/36'yı, APK install ve launcher `Status: ok` sonucunu doğrular. Birden çok cihaz varsa `ANDROID_SERIAL` zorunlu açık seçimdir. Kanıt özetinde tam ADB seri numarası yazılmaz.

AŞAMA 01 Android device kapısı ancak çıktı `STAGE01_DEVICE_GATE_PASS` içerirse kapanabilir. CI script parse/build entegrasyonu bu gerçek cihaz sonucunun yerine geçmez.

### Bu sohbet konteynerindeki eski yerel deneme

ChatGPT çalışma konteynerinde `dotnet` ve `adb` yoktu; Java `21.0.11` vardı. Resmi .NET 10.0.400 Linux x64 binary indirme denemesi konteyner dış DNS erişimi kapalı olduğu için `Could not resolve host: builds.dotnet.microsoft.com` ile başarısız olmuştu. Bu ürün/toolchain uyumsuzluğu değildir ve GitHub Actions PASS kanıtını geçersiz kılmaz.

### AŞAMA 01 neden hâlâ BLOCKED?

Nihai plan AŞAMA 01 çıkışı için gerçek telefonda boş MAUI uygulamasının çalışmasını ister. CI ve cihaz-gate otomasyonu fiziksel cihazı ikame etmez. Bu sohbet oturumunda kullanıcının gerçek geliştirme bilgisayarına veya fiziksel Android telefona USB/ADB erişimi yoktur.

Hâlâ eksik kanıtlar:

- Kullanıcının gerçek geliştirme makinesinde `STAGE01_DEVICE_GATE_PASS`.
- Fiziksel Android cihaz `device` kaydı, install ve launch — gate PASS çıktısının parçası.
- Mac/Xcode/iPhone/Apple Developer erişim envanteri.

Bu kanıtlar olmadan AŞAMA 01 `DONE` yazılmaz ve AŞAMA 02 başlatılmaz.

## Değiştirilemez temel teknik kararlar

Nihai plan değiştirilmedikçe yeni ajan şunları varsaymalıdır:

- v1 yalnız 2D viewer; edit/write yok.
- DWG/DXF cihazda ve offline okunur; zorunlu cloud conversion yok.
- Autodesk RealDWG, APS/Forge dönüşümü, ticari ODA SDK, trial/ücretli CAD parser-renderer yok.
- Varsayılan lisans allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD; exact dependency yine ayrıca denetlenir.
- Runtime graph’ta GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/unknown bileşen release blocker’dır.
- ACadSharp ana parser adayıdır ama corpus gate geçmeden production-approved değildir.
- SkiaSharp ana renderer adayıdır; native dağıtım zinciri ayrıca denetlenir.
- ProCad yalnız izole, source-pinned spike ile değerlendirilecek; production’a kendiliğinden girmeyecek.
- UI doğrudan parser entity’lerine bağlanmayacak; parser/scene/renderer sınırları korunacak.
- Unsupported/proxy/font/XREF/raster kaybı sessiz olmayacak; compatibility raporlanacak.
- Original drawing hiçbir koşulda overwrite edilmeyecek.

## Yeni ajan için çalışma protokolü

Bir sonraki ajan veya sohbet şu şekilde devam etmelidir:

1. `gecmis.md`, nihai plan checkpoint’i, `docs/EXECUTION_LOG.md`, `docs/TOOLCHAIN.md` ve aktif stage evidence dosyası okunur.
2. GitHub `main` gerçek durumu ve son commit’ler tekrar doğrulanır; bu dosyadaki eski SHA körlemesine kullanılmaz.
3. Kullanıcı değişiklikleri varsa korunur; destructive Git işlemi yapılmaz.
4. `BLOCKED` veya `IN_PROGRESS` aşama varsa yalnız oradan devam edilir. Mevcut durumda AŞAMA 01 `BLOCKED`; AŞAMA 02'ye geçilmez.
5. Bir turda en fazla bir aşama tamamlanır.
6. Her biten alt adım kanıtla işaretlenir; sahte PASS/DONE yazılmaz.
7. Dependency/toolchain sürümü kendiliğinden yükseltilmez; `[LIVE-VERIFY]` noktaları güncel resmi kaynakla yeniden doğrulanır.
8. Her turun sonunda nihai plan checkpoint’i, `gecmis.md`, `docs/EXECUTION_LOG.md` ve aktif evidence güncellenir.
9. Uzun sohbet belleğine veya modelin kişisel hafızasına güvenilmez; repo kayıtları esas alınır.

## Bir sonraki tur

AŞAMA 01'de devam edilecek. CI/toolchain/build/API/device-script kapısı PASS'tir; fiziksel telefon kapısı değildir. Gerçek geliştirme makinesinde platforma uygun `scripts/stage01-device-gate.*` çalıştırılıp `STAGE01_DEVICE_GATE_PASS` kaydedilmelidir. Ardından iOS erişimi yalnız envanterlenmelidir. Bu dış erişim sağlanmadan AŞAMA 01 tamamlanmış sayılmaz.
