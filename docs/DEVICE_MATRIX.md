# mobil-dwg — Android Cihaz ve Benchmark Matrisi

Bu belge emulator ile fiziksel cihaz kanıtını ayırır. Eski VXX/stage cursor'ı içermez.

## Cihaz slotları

| Slot | Platform | Amaç | Minimum profil | Durum |
|---|---|---|---|---|
| `E-API36` | Android Emulator | sürekli build/install/runtime ve hedefli regression | `mobil-dwg-api36`, API 36, x86_64 | `AVAILABLE` |
| `A-LOW` | Fiziksel Android | düşük kaynak / min-supported davranış | API 24+, arm64 tercih, 3–4 GiB RAM sınıfı | `UNASSIGNED` |
| `A-CURRENT` | Fiziksel Android | güncel orta sınıf ana regression | arm64, 6+ GiB RAM sınıfı | `UNASSIGNED` |
| `A-LARGE` | Fiziksel Android | büyük corpus/performance | arm64, 8+ GiB RAM sınıfı | `OPTIONAL / UNASSIGNED` |
| `I-OLDEST` | Future iOS | future minimum-device slot | ileride pinlenecek | `DEFERRED_FUTURE_IOS` |
| `I-CURRENT` | Future iOS | future current-device regression | ileride pinlenecek | `DEFERRED_FUTURE_IOS` |

Android v1 tarihsel release handoff API 36 emulator üzerinde doğrulandı; bu fiziksel cihaz gesture/GPU/SAF/performance fidelity iddiası değildir.

## Fiziksel cihazın özellikle gerekli olduğu alanlar

- touch sampling ve pinch/pan hissi,
- üreticiye özgü GPU/driver davranışı,
- SAF/content-provider farkları,
- gerçek memory pressure ve termal throttling,
- background/process death,
- düşük/orta/yüksek cihaz performans farkı,
- uzun süreli repeat-open/close ve lifecycle davranışı.

## Benchmark profilleri

Bunlar tek başına PASS eşiği değil, karşılaştırmalı ölçüm sınıflarıdır.

| Profil | Dosya sınıfı | Zorunlu ölçümler |
|---|---|---|
| `P-SMALL` | <= 2 MiB veya küçük sentetik fixture | parse, scene-build, first-frame, warning/diagnostic |
| `P-MEDIUM` | >2–20 MiB | aynı metrikler + pan/pinch frame p50/p95 |
| `P-LARGE` | >20–100 MiB | aynı metrikler + peak memory/PSS + repeat-open/close |
| `P-ADVERSARIAL` | corrupt/truncated/resource-limit | fail latency, peak memory, diagnostic category, cleanup |

Entity sayısı dosya boyutundan daha belirleyici olabildiği için test raporunda entity/primitive sayısı da kaydedilir.

## Ölçüm kayıt formatı

Her anlamlı cihaz koşusunda mümkün olduğunca:

- `device_slot`
- model sınıfı ve Android/API sürümü
- app commit SHA ve configuration
- fixture ID / provenance
- `file_bytes`
- entity/primitive count
- parse/scene/first-frame süreleri
- frame p50/p95
- peak RSS/PSS/native memory gerekiyorsa
- warning/error kategorileri
- close/reopen memory delta gerekiyorsa
- testin gerçek PASS/FAIL kriteri

kaydedilir.

Seri numarası, hesap bilgisi, kullanıcı yolu veya müşteri dosya adı kaydedilmez.

## Fixture seçimi

Public/synthetic fixture için `docs/GOLDEN_CONTRACT.md` ve `fixtures/` provenance kayıtları uygulanır. Gerçek müşteri çizimleri yerel/private testte kullanılabilir fakat repoya veya public CI artifact'ine yüklenmez.

## Güncelleme kuralı

Yeni bir fiziksel cihaz gerçekten test edildiğinde ilgili slot `UNASSIGNED` bırakılmaz; test tarihi, Android sürümü, commit SHA ve claim sınırı evidence kaydına eklenir. Emulator sonucu fiziksel slotu kapatmaz.
