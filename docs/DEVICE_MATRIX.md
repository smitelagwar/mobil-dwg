# Fiziksel cihaz matrisi ve provisional benchmark profilleri

Tarih: 2026-08-24  
Aşama: AŞAMA 03

Bu belge cihaz varmış gibi kanıt üretmez. Kullanıcının mevcut fiziksel Android/iOS erişimi AŞAMA 01'de `DEFERRED_EXTERNAL_GATE` durumundadır. Buradaki satırlar test slotu ve ölçüm sözleşmesidir; gerçek model/OS bilgisi ancak cihaz erişimi olduğunda doldurulur.

## Cihaz slotları

| Slot | Platform | Amaç | Minimum özellik | Gerçek cihaz | Durum |
|---|---|---|---|---|---|
| A-LOW | Android | düşük kaynak / min-supported doğrulama | Android API 24+, arm64 tercih, 3-4 GiB RAM sınıfı | UNKNOWN | DEFERRED_EXTERNAL_GATE |
| A-CURRENT | Android | güncel orta sınıf ana regresyon | target API 36 sınıfı, arm64, 6+ GiB RAM sınıfı | UNKNOWN | DEFERRED_EXTERNAL_GATE |
| A-LARGE | Android | büyük corpus/perf karşılaştırma | arm64, 8+ GiB RAM sınıfı | UNKNOWN | OPTIONAL / NOT_ASSIGNED |
| I-OLDEST | iOS | desteklenecek en eski gerçek iPhone sınıfı | exact minimum iOS Stage 23'te toolchain ile pinlenecek | UNKNOWN | DEFERRED_EXTERNAL_GATE |
| I-CURRENT | iOS | güncel gerçek cihaz regresyonu | güncel supported iOS, arm64 | UNKNOWN | DEFERRED_EXTERNAL_GATE |

Emulator/simulator ilgili platform smoke için yardımcı olabilir; fiziksel slotun PASS kanıtı yerine geçmez.

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

AŞAMA 03 için cihaz matrisi tasarımı tamamdır; fiziksel cihaz ataması kullanıcı erişimi olmadığı için bilinçli olarak boş bırakılmıştır. Bu boşluk AŞAMA 03 corpus/golden sözleşmesini bloke etmez fakat gerçek cihaz gerektiren Stage 05/07/08/21+ kapılarında yeniden zorunlu hale gelir.
