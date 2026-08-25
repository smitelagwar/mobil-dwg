# Local / Real-Device Revalidation Checklist

Bu belge emulator ve fiziksel Android kanıtını birbirinden ayırır. Aktif V01–V09 ayrıntısı `ANDROID_DOGRULAMA_PLANI.md` içindedir; buradaki fiziksel cihaz satırları emulator PASS ile kapanmaz.

Temel kural: **CI kanıtı cihaz kanıtı değildir.** Daha önce CI ile doğrulanan implementation tekrar sıfırdan yazılmaz; ilgili commit/artifact checkout edilip gerçek ortamda yeniden doğrulanır. Audit sırasında hata bulunursa tarihsel evidence silinmez; düzeltme yeni commit ve yeni kanıtla kaydedilir.

## Durum değerleri

- `PENDING_LOCAL`: Yerel/gerçek cihaz doğrulaması henüz yapılmadı.
- `FIX_REQUIRED`: Güvenilir testten önce kapatılması gereken otomasyon/kanıt açığı bulundu.
- `CONFIRMED`: Mevcut implementation ilgili gerçek ortamda aynen doğrulandı.
- `FIXED_AND_CONFIRMED`: Revalidation sırasında hata bulundu, düzeltildi ve tekrar doğrulandı.
- `STILL_DEFERRED`: Gerekli cihaz/hesap/ortam hâlâ yok.
- `NOT_APPLICABLE`: Aday daha önce deterministic hard blocker ile reddedildiği için ilgili cihaz testi artık karar için gerekli değil; aday yeniden açılırsa tekrar zorunlu olur.

## E0 — Aktif Android emulator revalidation lane

| V aşaması | Kapsam | Emulator gereksinimi | Güncel durum |
|---|---|---|---|
| V01 | Toolchain, runner, gate ve kanıt altyapısı | Zorunlu | `VALIDATED — INFRASTRUCTURE_SMOKE_ONLY` |
| V02 | Dependency/license/lockfile | Gereksiz | `VALIDATED` |
| V03 | Fixture/hash/golden sözleşmesi | Gereksiz | `VALIDATED` |
| V04 | Mimari + gerçek installable Android app shell | Zorunlu | `VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY` |
| V05 | ACadSharp parser/corpus | Headless + gerçek app linkage | `NOT_STARTED` |
| V06 | FilePicker/SAF/safe-open/lifecycle/cache | Zorunlu | `NOT_STARTED` |
| V07 | ProCad NO-GO ve graph izolasyonu | Uygulanmaz | `NOT_STARTED` |
| V08 | Tarihsel iOS kaydı / Android graph izolasyonu | iOS testi çalıştırılmaz | `SCOPE_ARCHIVED` hedefi |
| V09 | RenderScene/kamera/diagnostics | Host harness; Android linkage gerekirse | `NOT_STARTED` |

V01 sonucu yalnız geçici `Stage01Smoke` altyapı smoke'udur. V04 gerçek `MobilDwg.App` runtime gate'ini geçip merge edilerek `VALIDATED` oldu; parser/render fidelity ve fiziksel Android kanıtı ayrıca açıktır.

## R0 — Temiz geliştirme ortamı

- Fresh clone; `main` ve doğrulanacak release/audit commit SHA kaydedilir.
- .NET SDK/workload set `10.0.400` doğrulanır.
- Android: Microsoft OpenJDK `21.0.12`, API/compile/target 36, Build-Tools `36.0.0`, Platform-Tools/adb, `maui-android`.
- Clean restore/build/unit/architecture/dependency audit çalıştırılır.
- Yerel değişiklik olmadan sonuç ve tool versions kaydedilir.

## R1 — AŞAMA 01 fiziksel Android kapısı

- USB debugging açık fiziksel Android bağlanır.
- `adb devices` gerçek cihazı `device` durumunda görür; emulator sonucu kabul edilmez.
- Windows: `./scripts/stage01-device-gate.ps1`; Bash: `bash scripts/stage01-device-gate.sh`.
- Beklenen marker: `STAGE01_DEVICE_GATE_PASS`.
- Debug/Release install + launch, minSdk 24 / targetSdk 36 ve launcher durumu doğrulanır.

## R2 — AŞAMA 06 gerçek FilePicker/SAF kapısı

- Gerçek content provider üzerinden DWG ve DXF seçilir; provider fiziksel path'ine güvenilmediği doğrulanır.
- Büyük dosya/declared-size mismatch, actual-byte quota ve disk reserve davranışı denenir.
- Hızlı iki seçimde generation-id / last-request-wins doğrulanır.
- Cancel talebi, rotate, background/foreground, close/reopen ve cache cleanup gözlenir.
- Orijinal DWG/DXF hash'i önce/sonra değişmemelidir.
- Uygulama restart/process recreation sonrası stale cache/session kontrol edilir.

## R3 — Future iOS erken feasibility kapısı — aktif değil

Bu bölüm yalnız kullanıcı iOS'u açıkça yeniden etkinleştirirse uygulanır; Android V01–V09 veya Android release'i bloke etmez.

- Complete local/managed Mac üzerinde exact .NET `10.0.400` ve pinned iOS workload doğrulanır.
- Workload'un gerçek Xcode requirement'ı ile seçili Xcode eşleştirilir; hosted CI bundle workaround'u kullanılmaz.
- Baseline iOS Release build tamamlanır.
- Sentetik Stage 08 fixture production ACadSharp adapter ile parse edilir.
- Native Skia offscreen draw + PNG encode gerçekten çalıştırılır.
- Trimming warning'leri yeniden kaydedilir; ACadSharp reflection paths için preservation/fix yaklaşımı runtime fixture ile doğrulanır.
- `ios-arm64` Release/AOT publish yapılır; simulator NativeAOT sonucu gerçek-device AOT kanıtı sayılmaz.
- Fiziksel iPhone install/launch/open/render smoke tamamlanır.
- Font/resource/native library loading ayrıca kontrol edilir.

## R4 — Sonraki renderer/lifecycle/performance kapıları

AŞAMA 09 ve sonrasında ertelenen her fiziksel/device-dependent kriter bu belgeye yeni satır olarak eklenir. Özellikle:

- pan/pinch/fit/orientation ve touch behavior,
- process death/background/foreground,
- memory pressure, PSS/native memory ve repeat-open,
- büyük koordinat/mm-detail fixture,
- eksik font/XREF/raster/proxy diagnostics,
- full corpus ve gerçek cihaz Release performansı.

## R5 — Future iOS tam kabul — aktif değil

AŞAMA 23/24 sırasında:

- gerçek iPhone file importer/security-scoped URL,
- lifecycle/orientation/memory warning,
- trimming/AOT/native Skia,
- mini ve uygulanabilir full corpus,
- Release archive/signing,
- backup/privacy/resource davranışı

ayrı gerçek cihaz kanıtıyla kapanır.

## Deferred-gate matrisi

| Aşama | Ertelenen kriter | Gerekli ortam/donanım | Beklenen kanıt | Durum |
|---|---|---|---|---|
| 01 | Fiziksel Android install/launch | Gerçek Android + adb + pinned toolchain | `STAGE01_DEVICE_GATE_PASS`, cihaz/tool/version kaydı | `PENDING_LOCAL` |
| 01 | Future iOS erişim envanteri | Mac/Xcode/iPhone/Apple Developer durumu | Future reactivation kaydı | `DEFERRED_FUTURE_IOS` |
| 06 | FilePicker/SAF + lifecycle/cache | Gerçek Android | DWG/DXF open, cancel/race/rotate/background/close/cache evidence | `PENDING_LOCAL` |
| 07 | ProCad physical T3 | Gerçek Android | Exact candidate A/B | `NOT_APPLICABLE` — candidate deterministic precision blocker ile reddedildi; yeniden açılırsa zorunlu |
| 08 | Baseline iOS Release + parse + Skia | Complete Mac + simulator | Stage08 parse/Skia markers | `DEFERRED_FUTURE_IOS` |
| 08 | Trimming/AOT | Complete Mac + `ios-arm64` | linker evidence + Release/AOT publish | `DEFERRED_FUTURE_IOS` |
| 08 | Fiziksel iPhone smoke | Gerçek iPhone + signing | install/launch/parse/render evidence | `DEFERRED_FUTURE_IOS` |

## Audit kayıt formatı

Her tamamlanan satır için en az şunlar kaydedilir:

- tarih ve timezone,
- repo commit SHA,
- cihaz modeli / OS version veya Mac/Xcode version,
- exact komut/workflow/script,
- PASS/FAIL marker veya artifact,
- varsa bulunan hata ve düzeltme commit'i,
- final durum (`CONFIRMED`, `FIXED_AND_CONFIRMED`, vb.).

Revalidation geçmiş kanıtı geçersiz saymaz; hangi ortamda neyin gerçekten doğrulandığını kesinleştirir.
