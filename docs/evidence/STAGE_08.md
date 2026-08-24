# AŞAMA 08 Kanıtı — Erken iOS AOT/native fizibilite karakterizasyonu

## Durum

`DONE — CHARACTERIZATION / RISK_ACCEPTED_FOR_CONTINUATION`

Bu durum **iOS PASS anlamına gelmez**. AŞAMA 08'in plan çıkışındaki ikinci yol uygulanmıştır: dış blocker ve kalan iOS riski gerçek kanıtla kaydedilmiş, kullanıcı tarafından daha önce onaylanan `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` kapsamında cihazdan bağımsız sonraki işlerin ilerlemesine izin verilmiştir. Yerel/managed Mac, gerçek `ios-arm64` AOT ve fiziksel iPhone kanıtları açık kalır.

## Kapsam ve değişmezler

- Production dependency graph değiştirilmedi; bütün kod `spikes/Stage08.iOS` ve Stage 08 CI/harness alanında izoledir.
- Parser baseline: `ACadSharp 3.7.1`.
- Renderer-backend adayı: `SkiaSharp 4.151.1` + `SkiaSharp.NativeAssets.iOS 4.151.1`.
- .NET SDK/workload set: `10.0.400`.
- Test TFM: `net10.0-ios26.5`.
- Installed iOS workload: `26.5.10301/10.0.100`.
- Bu workload ile build'in gerçek Xcode gereksinimi log üzerinden `Xcode 26.6` olarak doğrulandı.

## Yetkili karakterizasyon koşusu

- Esas spike head: `1d7ce7ddc738ba615ad86f19742e3af9f2c78e18`.
- Workflow: `Stage 08 iOS Feasibility`.
- Run: `32776201092` / `#17` — `SUCCESS`.
- Job: `97587720958` — `SUCCESS`.
- Artifact: `9538345504`, `stage08-ios-feasibility-evidence`, 10,588 bytes.
- Artifact digest: `sha256:527956939b41b04b91cc79af04d821238f111d6f8a5a121598809a3830746b72`.
- Evidence JSON classification: `BLOCKED_PARTIAL_EVIDENCE`.

Workflow `SUCCESS`, bütün iOS probe'larının PASS olduğu anlamına gelmez. Harness üç sonucu ayrı karakterize edip gerçek blocker'ları evidence olarak yazdığı için workflow başarılıdır.

## Host ve exact graph kanıtı

Run #17:

- GitHub-hosted image: `macos-26-arm64`, macOS `26.5.2`.
- .NET SDK: `10.0.400`.
- iOS workload: `26.5.10301/10.0.100`.
- Xcode: `26.6`, build `17F113`.
- iOS 26.5 simulator runtime image üzerinde mevcuttu ve iPhone simulator boot edildi.
- Restore graph: `ACadSharp 3.7.1`, `SkiaSharp 4.151.1`, `SkiaSharp.NativeAssets.iOS 4.151.1`.
- `production_graph_modified=false`.

Markers: `STAGE08_HOST_PASS`, `STAGE08_IOS_WORKLOAD_PASS`, `STAGE08_EXACT_GRAPH_RECORDED`.

## Baseline Release sonucu

Baseline Release, iOS SDK'nın desteklediği linker-disable mekanizması `MtouchLink=None` ile denenmiştir. Managed assembly üretimi ilerledi; fakat hosted Xcode toolchain aşamasında:

`xcrun: error: unable to find utility "install_name_tool", not a developer tool or in PATH`

sonucuyla paketleme kesildi.

Evidence:

- `baseline_release.build_exit = 1`
- `baseline_release.simulator_exit = -1`
- `baseline_release.runtime_pass = false`
- blocker: `GITHUB_HOSTED_MACOS26_XCODE26_6_INSTALL_NAME_TOOL_MISSING`

Bu hata ACadSharp/Skia runtime başarısızlığı olarak sınıflandırılmaz. Hosted Xcode bundle'ına symlink/elle araç yerleştirme yapılarak PASS üretilmedi. Aynı pinned hat complete local/managed Mac toolchain üzerinde yeniden çalıştırılmalıdır.

## Trimming/reflection/font karakterizasyonu

Trimming probe warning'leri error'a dönüştürmeden ayrıntılı çalıştırıldı. ACadSharp ve yardımcı assembly hattında gerçek linker uyumluluk riskleri görüldü.

Final evidence sayımları:

- trimmer warning lines: `30`
- reflection-related lines: `12`
- font warning lines: `0`

Görülen warning aileleri arasında `IL2070`, `IL2026`, `IL2087`, `IL2075`, `IL2072`, `IL2090` bulunur. Bunlar `Assembly.GetTypes`, `Type.GetProperties/GetInterface`, `Activator.CreateInstance`, `TypeDescriptor.GetConverter` gibi reflection/dynamic-discovery yollarıyla ilişkilidir.

Trim build daha sonra hosted Xcode araç lookup problemiyle de karşılaştı. Bu nedenle iki ayrı gerçek vardır:

1. ACadSharp 3.7.1 hattında trimming uyumluluk riski kanıtlanmıştır.
2. Hosted runner'ın Xcode tool lookup problemi final trimmed app/runtime sonucunu ayrıca engellemiştir.

Evidence blocker: `ACADSHARP_TRIM_COMPATIBILITY`. Bu risk suppress edilerek “temiz PASS” yazılmaz.

## NativeAOT karakterizasyonu

`iossimulator-arm64` için `PublishAot=true` probe'u `NETSDK1203` ile reddedildi: Ahead-of-time compilation bu simulator RID için desteklenmiyor. Bu sonuç **ACadSharp NativeAOT failure değildir**; simulator RID/platform sınırıdır.

Evidence:

- `nativeaot_probe.publish_exit = 1`
- `runtime_pass = false`
- script classification: `PUBLISH_FAILURE_OTHER`

Gerçek NativeAOT fizibilitesi complete Mac üzerinde `ios-arm64`/physical-device hattında yeniden ölçülmelidir.

## Simulator ve fiziksel cihaz durumu

Bundled sentetik DXF'i production `AcadSharpDocumentReader` ile parse edip native Skia bitmap/render/PNG encode yapan probe hazırlanmıştır. Ancak hosted runner toolchain build blocker'ı nedeniyle final simulator parse/Skia marker'ları elde edilmemiştir.

- `STAGE08_IOS_SIMULATOR_PARSE_PASS`: **NOT PROVEN**
- `STAGE08_IOS_SIMULATOR_SKIA_PASS`: **NOT PROVEN**
- fiziksel iPhone: `NOT_RUN_DEFERRED_EXTERNAL_GATE`
- kullanıcı yerel Mac/Xcode envanteri: `PENDING_USER_EVIDENCE`

Simulator başarısı elde edilse bile fiziksel iPhone PASS yerine geçmeyecekti.

## Önceki koşuların sınıflandırılması

Final run #17 dışındaki önceki fail'ler candidate kararı değildir; harness/platform hizalaması sırasında bulunan ve düzeltilen setup sorunlarıdır:

- `macos-15` image gereken yeni Xcode bandını taşımıyordu.
- İlk TFM/Xcode pack eşleştirmesi yanlış seçilmişti; `net10.0-ios26.5` + Xcode 26.6 hattına düzeltildi.
- Probe'daki eski UIKit `UIWindow` constructor kullanımı iOS 26'da `CA1422` üretti; UI gereksinimi kaldırılarak headless AppDelegate yapıldı.
- `PublishTrimmed=false` iOS SDK tarafından geçersizdir; baseline için doğru `MtouchLink=None` kullanıldı.
- Hosted `macos-26`/Xcode 26.6 tool lookup'ta `install_name_tool` ve trimming hattında `clang` bulunamaması dış toolchain blocker'ı olarak kaldı.

## Karar

AŞAMA 08'de seçilen ACadSharp + SkiaSharp hattının iOS tarafında **temel runtime/device fizibilitesi henüz kanıtlanmamıştır**. Buna rağmen exact dependency/workload/Xcode hattı, gerçek trimming riskleri, simulator NativeAOT sınırı ve hosted toolchain blocker'ı yeterli ayrıntıyla karakterize edilmiştir.

Kullanıcının mevcut dış-gate yürütme onayı doğrultusunda karar:

`RISK_ACCEPTED_FOR_INDEPENDENT_CONTINUATION`

Bu karar yalnız AŞAMA 09 ve sonraki cihazdan bağımsız işlerin sırasını bloke etmemek içindir; AŞAMA 23/24 ve final release gate'lerinde iOS gerçek cihaz kanıtı yeniden zorunludur.

## Yeniden açma / zorunlu yerel doğrulama

Aşağıdakiler yapılmadan iOS PASS yazılamaz:

1. Complete local/managed Mac üzerinde exact .NET `10.0.400` + iOS workload `26.5.10301` + uyumlu Xcode hattını doğrula.
2. Baseline Release build'i hosted-runner tool lookup problemi olmadan tamamla.
3. Bundled DXF ile ACadSharp parse ve Skia native offscreen render/encode marker'larını simulator ve ardından fiziksel iPhone'da al.
4. Trimming warning'lerini fixture/runtime davranışıyla çöz veya kontrollü linker preservation stratejisini kanıtla; warning suppress tek başına çözüm değildir.
5. `ios-arm64` Release/AOT publish ve fiziksel iPhone smoke yap.
6. File import, lifecycle, font/resource loading ve memory davranışını gerçek iPhone'da AŞAMA 23/24 kapılarında yeniden doğrula.

Takip kontrol listesi: `docs/LOCAL_DEVICE_REVALIDATION.md`.
