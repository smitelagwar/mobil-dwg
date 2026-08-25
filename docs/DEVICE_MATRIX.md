# Android cihaz matrisi ve provisional benchmark profilleri

İlk kayıt: 2026-08-24  
Android kapsam güncellemesi: 2026-08-25  
Aktif validation: V03

Bu belge emulator ile fiziksel cihaz kanıtını birbirine karıştırmaz. Android Emulator sürekli ve hedefli smoke için kullanılabilir; fiziksel Android slotları beta/release çeşitliliği, gerçek SAF ve performans için ayrıca açıktır. iOS aktif v1 dışında future option olarak korunur.

## Cihaz slotları

| Slot | Platform | Amaç | Minimum özellik | Gerçek cihaz | Durum |
|---|---|---|---|---|---|
| E-API36 | Android Emulator | self-hosted sürekli build/install/launch ve hedefli runtime smoke | `mobil-dwg-api36`, API 36, x86_64 | AVD mevcut | `AVAILABLE / V01_VALIDATED_INFRASTRUCTURE_SMOKE` |
| A-LOW | Android | düşük kaynak / min-supported doğrulama | Android API 24+, arm64 tercih, 3–4 GiB RAM sınıfı | UNKNOWN | `DEFERRED_PHYSICAL_ANDROID` |
| A-CURRENT | Android | güncel orta sınıf ana regresyon | target API 36 sınıfı, arm64, 6+ GiB RAM sınıfı | UNKNOWN | `DEFERRED_PHYSICAL_ANDROID` |
| A-LARGE | Android | büyük corpus/perf karşılaştırma | arm64, 8+ GiB RAM sınıfı | UNKNOWN | `OPTIONAL / NOT_ASSIGNED` |
| I-OLDEST | Future iOS | yeniden etkinleştirilirse en eski gerçek iPhone sınıfı | future Stage 23'te pinlenecek | UNKNOWN | `DEFERRED_FUTURE_IOS` |
| I-CURRENT | Future iOS | yeniden etkinleştirilirse güncel cihaz regresyonu | future supported iOS, arm64 | UNKNOWN | `DEFERRED_FUTURE_IOS` |

E-API36 için V01 authoritative run `32821991333`, job `97721878468` üzerinde emulator/toolchain hattı doğrulandı. Bu sonuç yalnız `Stage01Smoke` infrastructure smoke kanıtıdır; gerçek `MobilDwg.App`, DWG/DXF açma veya viewer fidelity PASS değildir. V04 gerçek installable uygulama kabuğuna geçmeden E-API36 sonucu viewer sonucu diye kullanılmaz.

## V03 Android smoke input seti

V04–V09 doğrulamalarında hak durumu açık küçük test girdisi gerektiğinde manifestteki `android_smoke_set` kullanılır:

- committed 0BSD DXF: `synthetic-turkish-basic-ac1015`;
- validation-time generated 0BSD DWG: `synthetic-turkish-basic-ac1015-dwg`;
- kontrollü negatif committed DXF'ler: `negative-missing-font-ac1015`, `negative-missing-xref-ac1015`.

Generated DWG, committed sentetik DXF'den exact ACadSharp `3.7.1` fixture generator ile üretilir; `AC1015` magic ve `DwgReader` read-back doğrulanır. Bu artifact open-path smoke girdisidir, bağımsız DWG engineering-fidelity goldeni değildir. Remote-pinned ACadSharp sample DWG/DXF corpus'u fidelity/parser regresyonu için korunur fakat mobil-dwg tarafından yeniden dağıtılabilir bundle olarak sınıflandırılmaz.

## Provisional corpus profilleri

Bunlar performans kabul eşiği değil, ölçüm örnekleme sınıfıdır. AŞAMA 20'den önce sayısal FAIL bütçesi uydurulmaz.

| Profil | Dosya sınıfı | Zorunlu ölçümler | Kullanım |
|---|---|---|---|
| P-SMALL | <= 2 MiB veya küçük sentetik/mini fixture | open-to-parse, scene-build, first-frame, peak RSS, warning count | her parser/render smoke |
| P-MEDIUM | >2–20 MiB | aynı metrikler + pan/pinch p50/p95 frame time | fidelity milestone |
| P-LARGE | >20–100 MiB | aynı metrikler + GC/native memory delta + close/reopen cleanup | AŞAMA 20/21 |
| P-ADVERSARIAL | corrupt/truncated veya resource-limit fixture | fail latency, peak RSS, exception/warning category, cleanup | AŞAMA 19/21 |

## Ölçüm kayıt formatı

Her cihaz koşusu en az şunları kaydeder:

- `device_slot`
- gerçek modelin hassas olmayan adı ve OS/API sürümü
- app revision / configuration (`Debug` veya `Release`)
- fixture ID ve manifest hash/provenance
- `file_bytes`
- `parse_ms`
- `scene_build_ms`
- `first_frame_ms`
- `peak_rss_mib`
- frame p50/p95 (renderer/gesture aşamasında)
- warning/error kategorileri
- close/reopen sonrası gözlenen memory delta
- PASS/FAIL'i belirleyen aşama kriteri

Seri numarası, UDID, hesap bilgisi, kullanıcı yolu veya müşteri dosya adı kaydedilmez.

## Şimdiki durum

- E-API36: altyapı olarak kullanılabilir; gerçek viewer gate V04'ten sonra açılır.
- Fiziksel Android: release/beta kapılarında zorunlu farkları kanıtlamak üzere deferred.
- iOS: future/inactive; Android release'i bloke etmez.
- V03: fixture ve test-matrix sözleşmesini doğrular; bu aşamada gereksiz emulator koşusu yapılmaz.
