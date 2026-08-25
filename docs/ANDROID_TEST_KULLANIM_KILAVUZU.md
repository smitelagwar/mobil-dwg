# Android Emulator Otomatik Test Sistemi Kullanım Kılavuzu
> **Hedef Kitle:** İnsan Geliştiriciler & Yapay Zeka Ajanları (ChatGPT, AntiGravity, Claude, Codex, vb.)  
> **Konum:** `docs/ANDROID_TEST_KULLANIM_KILAVUZU.md`  
> **İlgili Repo:** `smitelagwar/mobil-dwg`

---

## 1. Sistemin Mimarisi ve Temel Prensibi

Bu bilgisayar, `mobil-dwg` projesinin Android çalışmalarını Android 16 (API 36) Emulator üzerinde otomatik doğrulamak için **Self-Hosted Android Test Node** olarak yapılandırılmıştır.

> **Mevcut sınır:** V01 sonucu yalnız `INFRASTRUCTURE_SMOKE_ONLY`dır. V04 gerçek `MobilDwg.App` APK build/install/cold-launch/UI/stability gate'ini geçip PR `#17` ile `main`e merge edilerek `VALIDATED` oldu; parser/render fidelity ve fiziksel Android PASS değildir.

```text
       ┌─────────────────────────────────────────────────────────┐
       │                 GELİŞTİRME AKIŞI                       │
       │                                                         │
       │  Normal main / feature commit'leri                      │
       │  ───────────────► GitHub (çoğunlukla hosted CI)         │
       │                   (Android emulator gate tetiklenmez)   │
       │                                                         │
       │  Emulator için 'android-test' push veya manual dispatch │
       │  ───────────────► GitHub Actions                        │
       └────────────────────────────┬────────────────────────────┘
                                    │
                                    ▼
       ┌─────────────────────────────────────────────────────────┐
       │             BU WINDOWS BİLGİSAYARI (RUNNER)             │
       │                                                         │
       │  1. Self-Hosted Runner (C:\actions-runner) işi yakalar  │
       │  2. mobil-dwg-api36 Emulator'ı kontrol eder (WHPX+GPU)  │
       │  3. Solution build eder; harness marker'larını koşar    │
       │  4. Şimdilik geçici Stage01Smoke APK üretir             │
       │  5. Smoke APK'yı emulator'a 'adb install -r' ile kurar  │
       │  6. Uygulama MainActivity'sini ayağa kaldırır (am start)│
       │  7. PID, bellek, logcat ve ANR/Crash kontrolü yapar     │
       │  8. Ekran görüntüsü ve logları toplar                   │
       └────────────────────────────┬────────────────────────────┘
                                    │
                                    ▼
       ┌─────────────────────────────────────────────────────────┐
       │             GITHUB ACTIONS ARTIFACTS                    │
       │                                                         │
       │  android-emulator-result/                               │
       │    ├── summary.txt       (Test özet karnesi)            │
       │    ├── logcat.txt        (Filtrelenmiş runtime logları) │
       │    ├── meminfo.txt       (Dumpsys RAM kullanım raporu)  │
       │    ├── device-info.txt   (Cihaz & API özellikleri)      │
       │    └── screenshots/                                     │
       │          └── emulator_launch.png (Açılış ekranı)        │
       └─────────────────────────────────────────────────────────┘
```

---

## 2. İnsan Geliştirici (Kullanıcı) İçin Kullanım Rehberi

### A. Bilgisayarı Test Dinleme Moduna Alma (Runner Başlatma)
Bilgisayarınızı test almaya hazır tutmak için:
1. Bir Komut İstemi (CMD) veya PowerShell penceresi açın.
2. Şu komutları girin:
   ```cmd
   cd C:\actions-runner
   run.cmd
   ```
3. Ekranda `Listening for Jobs` yazısını gördüğünüzde pencereyi simge durumuna küçültebilirsiniz. Bilgisayar açık olsa bile bu listener çalışmıyorsa GitHub emulator testi başlayamaz.

> [!IMPORTANT]
> **Neden Windows Servisi Değil de `run.cmd`?**  
> Android Emulator, NVIDIA ekran kartınızdan ve Windows Hypervisor Platform'dan (WHPX) doğrudan GPU hızlandırması alır. Windows Servisleri (Session 0) grafik oturumuna erişemediği için emulator açılışında çöker. Bu yüzden masaüstü oturumunda `run.cmd` çalıştırmak en kararlı ve hızlı yöntemdir.

---

### B. Tek Komutla Yerel Test Koşma (Lokal Gate)
GitHub'a gitmeden, kendi terminalinizden emulator testini tek komutla çalıştırmak için:
```powershell
.\scripts\android-emulator-gate.ps1 -Configuration Release
```
*(Hızlı derleme için `-Configuration Debug` da verebilirsiniz).*

Bugünkü script:
- Tüm araçları (.NET, Java, ADB, SDK) denetler.
- Emulator açıksa yeniden başlatmadan kullanır, kapalıysa otomatik açar.
- Solution Release build'ini ve executable Core/Rendering/Architecture harness marker'larını açıkça çalıştırır.
- Geçici `Stage01Smoke` APK'yı derler, kurar ve açar; gerçek viewer/FilePicker/render sonucu üretmez.
- Screenshot'ı byte-safe alır ve PNG imzasını doğrular; artifact yine test edilen exact SHA ve scope ile yorumlanır.
- Başarılı olursa `ANDROID_EMULATOR_GATE_PASS` yazar; bu marker mevcut sürümde yalnız infrastructure smoke kapsamındadır.

---

### C. Emulator Yönetim Komutları
- **Ortam Sağlık Taraması (11 bileşen denetimi):**
  ```powershell
  .\scripts\doctor-local-environment.ps1
  ```
- **Emulator'ı Manuel Başlatma:**
  ```powershell
  .\scripts\start-emulator.ps1
  ```
- **Emulator'ı Kapatma:**
  ```powershell
  .\scripts\stop-emulator.ps1
  ```

---

### D. GitHub Web Arayüzünden Test Tetikleme
1. GitHub reponuzda **Actions** sekmesine gidin.
2. Sol menüden **Android Emulator Automated Test Gate** seçin.
3. Sağ üstteki **Run workflow** butonuna tıklayın, `Release` veya `Debug` seçip çalıştırın.
4. Test tamamlandığında sayfanın altındaki **Artifacts** bölümünden `android-emulator-result.zip` dosyasını indirip ekran görüntülerini inceleyebilirsiniz.

---

## 3. Yapay Zeka Ajanları (ChatGPT, AntiGravity, vb.) İçin Kullanım Rehberi

### Senaryo 1: ChatGPT (GitHub Üzerinden Uzak Çalışan Ajan)
ChatGPT doğrudan bu bilgisayarın terminaline erişemez; bu yüzden **GitHub Actions köprüsünü** kullanır:

1. **Geliştirme Yap:** Normal kod değişikliklerini `main` veya ilgili feature branch üzerinde tamamla.
2. **Test Noktasına Gelindiğinde:** GitHub connector/API ile test edilecek exact SHA'yı ve mevcut `android-test` ref'ini oku. Hedef SHA mevcut taşıyıcı ref'in descendant'ıysa ref'i yalnız `force: false` fast-forward ile ilerlet. Değilse force kullanma; uygun manual dispatch/exact-ref yolunu değerlendir veya blocker'ı kaydet.
3. **Runner Otomatik Tetiklenir:** `android-test` branch'ine yapılan push, bu bilgisayardaki `[self-hosted, windows, android-test, mobil-dwg]` runner'ını uyandırır.
4. **Sonucu İncele:** GitHub Actions API veya arayüzünden test run çıktısını ve yüklenen `android-emulator-result` artifact'ini kontrol et.

Feature head `android-test` taşıyıcısıyla sınandıysa PR merge yöntemi varsayılan olarak **merge commit** olmalıdır. Squash/rebase tested head'i `main` ancestry'sinden çıkararak sonraki `force:false` fast-forward taşıyıcı güncellemesini bozabilir. Merge commit kullanılamıyorsa force uygulanmaz; exact-ref manual dispatch veya güvenli eşdeğer tetikleme seçilir.

> [!CAUTION]
> **Kritik Kural:** `android-test` branch'inde asla doğrudan kod geliştirme yapılmaz. Bu branch sırf test edilecek commit'i yerel test makinesine iletmek için bir "taşıyıcı boru hattı"dır.

Runner çevrim dışıysa yeni push'larla job biriktirilmez. Exact SHA `PENDING_EMULATOR_QUEUE` olarak kaydedilir; bilgisayar + interaktif listener döndüğünde yalnız hâlâ geçerli olan checkpoint test edilir. Normal GitHub senkronizasyonu yalnız `main` içindir.

Bilgisayar kapalıyken paralel A10 geliştirmesi gerekiyorsa ayrı sohbette `BASLA_A10.md dosyasını oku` denir. A10 yalnız `stage10-p0-geometry-draft` branch'inde ilerler. Workflow filtreleri kontrol edilir: açık A10 PR'ı yoksa normal branch commit/push yapılabilir; PR zaten açıksa branch push'u `pull_request:synchronize` sayılıp V04 sonrası Core/Rendering için self-hosted emulator işi açabileceğinden önce PR kapatılır/etki güvenle gate edilir, aksi halde offline push yapılmaz. Host testleri yoksa durum `CODED_PENDING_HOST_TESTS` kalır. A10 sohbeti `android-test` branch'ini hareket ettirmez; Android kanıtı olmadan `main` merge/DONE yoktur.

---

### Senaryo 2: Yerel Ajan (AntiGravity, VS Code Codex / Cursor)
Ajan doğrudan kullanıcının bilgisayarında ve yerel terminal erişimine sahipse:

1. GitHub round-trip yapmasına gerek yoktur.
2. Doğrudan PowerShell üzerinden yerel gate scriptini çağırır:
   ```powershell
   & .\scripts\android-emulator-gate.ps1 -Configuration Release
   ```
3. `artifacts/android-emulator-result/summary.txt` ve `logcat.txt` dosyalarını doğrudan diskten okuyarak doğrulama yapar.

---

## 4. Sistem ve Donanım Parametreleri Referansı

| Parametre | Değer / Yol |
|---|---|
| **İşletim Sistemi** | Windows 11 Home 64-bit (AMD Ryzen 5 7640HS, NVIDIA RTX 4060) |
| **Sanallaştırma** | Windows Hypervisor Platform (WHPX); mevcut AVD'de `hw.gpu.enabled=no`, GPU Host Direct kanıtlanmış varsayılmaz |
| **.NET SDK** | `10.0.400` (`C:\Program Files\dotnet\sdk\10.0.400`) |
| **MAUI Workload** | `maui-android` (`10.0.20/10.0.100`) |
| **Java JDK** | `Microsoft OpenJDK 21.0.12.1` (`JAVA_HOME`) |
| **Android SDK** | Android API 36 (Android 16 Baklava) |
| **Android Build-Tools** | `36.0.0` |
| **ADB Sürümü** | `37.0.1` |
| **Hedef AVD** | `mobil-dwg-api36` (Google APIs x86_64, Pixel 7) |
| **Runner Konumu** | `C:\actions-runner` (v2.336.0) |
| **Runner Etiketleri** | `self-hosted`, `windows`, `android-test`, `mobil-dwg` |
| **Workflow Dosyası** | `.github/workflows/android-emulator-test.yml` |
| **Yerel Gate Scripti** | `scripts/android-emulator-gate.ps1` |

---

## 5. Güvenlik ve Uyumluluk Kuralları

1. **Gizlilik ve Sır Güvenliği:** Runner tokenları, şifreler, kullanıcı özel yolları veya özel DWG çizimleri asla repoya, commit geçmişine veya loglara yazılmaz.
2. **Fiziksel Cihaz Ayrımı:** Bu altyapı Android Emulator için geliştirilmiştir. `scripts/stage01-device-gate.ps1` fiziksel telefon gereksinimi için değiştirilmeden korunmaktadır.
3. **CI İzolasyonu:** Legacy `android-emulator-test.yml` açısından normal `main`/feature push tetikleyici değildir; `android-test` push veya manuel `workflow_dispatch` işi başlatır. Ayrı V02/V03 self-hosted audit workflow'ları kendi path filtreleri eşleştiğinde `main`/PR üzerinde çalışabilir. PR `#17` ile gelen V04 workflow'u da app/Core/Cad/Rendering/architecture yolları değişen açık PR push'unda `pull_request:synchronize` üzerinden self-hosted emulator çalıştırabilir; PC kapalı çalışma planlanırken bütün bu filtreler kontrol edilir.
4. **Güvenilen kod:** Self-hosted Windows kullanıcısı üzerinde yalnız repo sahibi tarafından kontrol edilen commit çalıştırılır; üçüncü taraf PR/ref'i bu runner'a gönderilmez.
