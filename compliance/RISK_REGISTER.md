# mobil-dwg — Aktif Risk Register

Son güncelleme: 2026-09-06

Bu dosya yalnız hâlâ anlamlı olan ürün/teknik riskleri taşır. Kapanmış tarihsel Stage/V doğrulama kayıtları çalışma ağacında tutulmaz; gerektiğinde Git geçmişi ve `docs/HISTORY.md` kullanılır.

| ID | Risk | Etki | Durum | Zorunlu tepki |
|---|---|---|---|---|
| `R-VIEW-001` | Mevcut viewer pan/pinch sırasında gerçek kamerayı her frame render etmek yerine görüntü katmanını geçici transform edip gesture sonunda yeniden render edebiliyor | Hareket sırasında yeni görünür alan boş/eski kalabilir; release anında görsel sıçrama oluşabilir | `OPEN` | Interaction/render zincirini canlı camera state + frame-scheduled rendering modeline geçir; gesture sırasında görünür alanı doğrula |
| `R-VIEW-002` | MAUI DIP, physical pixel, display density, image layout/AspectFit ve camera viewport koordinatlarının karışması | Pinch focal drift, zoom merkezinin kaçması, farklı cihazlarda tutarsızlık | `OPEN` | Tek bir açık screen-coordinate contract kullan; focal world-point preservation'ı gerçek UI zincirinde test et |
| `R-VIEW-003` | Gesture testlerinin yalnız camera matematiğini doğrulayıp gerçek touch → UI → renderer zincirini kapsamaması | Unit test PASS iken cihazda gesture bozuk olabilir | `OPEN` | Gerçek emulator/fiziksel interaction acceptance ekle; 1→2→1 pointer transition ve jump/drift ölç |
| `R-PERF-001` | Büyük scene'de entity görünürlük seçimi/çizim maliyeti ölçeklenebilir | Pan/zoom jank, yüksek p95 frame time | `OPEN / MEASURE_FIRST` | Önce profil/metric; gerekirse spatial index, caching, LOD veya batching ekle; correctness regression yapma |
| `R-PERF-002` | Interaction sırasında tam kalite render maliyeti düşük/orta cihazlarda frame budget'ı aşabilir | Touch hissi gecikir | `OPEN` | Preview/refinement veya progressive kalite ancak ölçümle ve görüntü doğruluğu korunarak uygulanır |
| `R-DEP-001` | ACadSharp 3.7.1 bazı gerçek dünya DWG/DXF'lerde fidelity sınırına sahip olabilir | Eksik/yanlış CAD görünümü | `CONTROLLED` | Exact pini koru; sistematik hata varsa bağımsız fixture/corpus ile A/B doğrula; sessiz fallback yapma |
| `R-DEP-002` | SkiaSharp native/third-party artifact zinciri değişebilir | License/native runtime riski | `CONTROLLED / REVIEW_ON_CHANGE` | Package/version değişiminde transitive/native inventory ve notice kontrolü |
| `R-DEP-003` | Reddedilmiş ProCad veya başka CAD viewer kaynaklarından kod kopyalanması | Lisans/politika ve bakım riski | `CONTROLLED / NO_GO_FOR_COPY` | Fikir/algoritma deseni incelenebilir; satır-satır port/kopya yok; özgün implementasyon |
| `R-EXT-001` | Android v1 kabulü ağırlıklı API 36 emulator doğrulamasına dayanır; fiziksel cihaz matrisi açık | Gerçek touch/GPU/SAF/thermal/perf farkları bilinmez | `OPEN_PHYSICAL_DEVICE_COVERAGE` | Önemli viewer değişikliklerinde en az güncel fiziksel Android slotunda doğrulama; emulatoru fiziksel cihaz sayma |
| `R-EXT-002` | Self-hosted runner çevrim dışı veya eski marker yanlış yorumlanabilir | Yanlış PASS / test kuyruğu | `CONTROLLED` | Exact SHA; zero-step/queued PASS değil; yalnız değişikliği gerçekten kapsayan gate kullan |
| `R-IOS-001` | iOS aktif değil ama shared sınırlar future dönüş için korunuyor | Android odaklı refactor future portability'yi zorlaştırabilir | `DEFERRED_FUTURE_IOS` | Android'i bloke etme; Core/Cad/Rendering'e gereksiz Android-only bağımlılık yayma |
| `R-DATA-001` | Gerçek müşteri CAD/font/asset'in repo veya public artifact'e sızması | Gizlilik/telif riski | `CONTROLLED` | Private corpus Git dışında; public fixture yalnız provenance/redistribution kanıtlı |

## Risk kapatma kuralı

Bir risk yalnız kod değiştiği için `CLOSED` olmaz. İlgili failure mode'u doğrudan sınayan test/ölçüm PASS olmalı ve claim sınırı kaydedilmelidir.
