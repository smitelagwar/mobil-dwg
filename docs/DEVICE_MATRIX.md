# Fiziksel cihaz matrisi ve provisional benchmark profilleri

İlk kayıt: 2026-08-24
Android kapsam güncellemesi: 2026-08-25
Aşama: AŞAMA 03

Bu belge emulator ile fiziksel cihaz kanıtını birbirine karıştırmaz. Android Emulator kuruludur; fiziksel Android slotları release çeşitliliği için açıktır. iOS slotları aktif v1 dışında future option olarak korunur.

## Cihaz slotları

| Slot | Platform | Amaç | Minimum özellik | Gerçek cihaz | Durum |
|---|---|---|---|---|---|
| E-API36 | Android Emulator | GitHub/self-hosted sürekli build-install-launch ve hedefli runtime smoke | `mobil-dwg-api36`, Pixel 7 profile, API 36, x86_64 | AVD mevcut | `AVAILABLE / V01_FIX_REQUIRED` |
| A-LOW | Android | düşük kaynak / min-supported doğrulama | Android API 24+, arm64 tercih, 3-4 GiB RAM sınıfı | UNKNOWN | DEFERRED_EXTERNAL_GATE |
| A-CURRENT | Android | güncel orta sınıf ana regresyon | target API 36 sınıfı, arm64, 6+ GiB RAM sınıfı | UNKNOWN | DEFERRED_EXTERNAL_GATE |
| A-LARGE | Android | büyük corpus/perf karşılaştırma | arm64, 8+ GiB RAM sınıfı | UNKNOWN | OPTIONAL / NOT_ASSIGNED |
| I-OLDEST | Future iOS | yeniden etkinleştirilirse en eski gerçek iPhone sınıfı | future Stage 23'te pinlenecek | UNKNOWN | DEFERRED_FUTURE_IOS |
| I-CURRENT | Future iOS | yeniden etkinleştirilirse güncel cihaz regresyonu | future supported iOS, arm64 | UNKNOWN | DEFERRED_FUTURE_IOS |

Android emulator smoke için aktiftir fakat fiziksel slotun PASS kanıtı yerine geçmez. iOS simulator/cihaz işi active değildir.

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
- fixture ID ve manifest hash
- `file_bytes`
- `parse_ms`
- `scene_build_ms`
- `first_frame_ms`
- `peak_rss_mib`
- frame p50/p95 (renderer/gesture aşamasında)
- warning/error kategorileri
- close/reopen sonrası gözlenen memory delta
- PASS/FAIL'i belirleyen aşama kriteri

Seri numarası, UDID, Apple ID, kullanıcı yolu veya müşteri dosya adı kaydedilmez.

## Şimdiki durum

AŞAMA 03 için cihaz matrisi tasarımı tamamdır. E-API36 V01 sertleştirmesinden sonra otomatik smoke slotu olur. Fiziksel Android ataması bilinçli olarak açık kalır ve AŞAMA 20–22/final kapılarında zorunludur. Future iOS slotları Android release'i bloke etmez.
