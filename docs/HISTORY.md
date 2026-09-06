# mobil-dwg — Project History

Bu dosya yalnızca kalıcı proje geçmişini ve bugün hâlâ geçerli olan teknik kararları özetler. Aktif iş listesi, aşama cursor'u veya uygulanacak plan değildir.

## V1 kapanış özeti

Android v1 geliştirmesi 2026'da tamamlandı. Geçmiş geliştirme süreci Stage 01–27, V01–V09 ve A10–A27 adlarında çok sayıda doğrulama adımı kullandı. Bu adlar artık yeni geliştirme sürecinin parçası değildir.

Güncel ürün sınırı:

- Android-only
- local/offline
- read-only 2D DWG/DXF viewer
- production writer/save yok
- zorunlu cloud conversion yok

## Kalıcı teknik kararlar

- Parser baseline: ACadSharp `3.7.1`, read-only kullanım.
- Renderer baseline: SkiaSharp `4.151.1`.
- Exact unpatched ProCad production reuse kararı `NO-GO`; ayrıntı `docs/ADR/0002-procad-pinned-source-no-go.md`.
- World/document koordinatları `double` tutulur.
- UI parser entity tiplerine doğrudan bağlanmaz.
- Unsupported/proxy/font/XREF/raster problemleri sessizce gizlenmez.
- Orijinal DWG/DXF immutable kalır.
- Dependency ve redistributable asset'ler lisans/provenance politikasına tabidir.
- Emulator sonucu fiziksel cihaz fidelity kanıtı sayılmaz.

## Tarihsel çalışma ağacı temizliği

V1 tamamlandıktan sonra eski başlangıç/handoff/validation planları kaldırıldı. 2026-09-06 repository sadeleştirmesinde ayrıntılı `docs/evidence/STAGE_*` ve `docs/evidence/android-validation/V*` kayıtları da çalışma ağacından çıkarıldı.

Bu kayıtlar kaybolmuş değildir: Git geçmişinde ilgili commit'lerde bulunabilir. Güncel geliştirme sırasında normalde okunmaları veya yeniden oluşturulmaları gerekmez.

Aynı nedenle yeni bug fix, performans veya özellik işleri eski Stage/V/A numaralandırmasına devam etmez. İş, doğrudan mevcut kod ve güncel testler üzerinden ele alınır.

## Güncel kaynaklar

- Proje girişi: `README.md`
- Mimari: `docs/ARCHITECTURE.md`
- Android test yaklaşımı: `docs/ANDROID_TESTING.md`
- Golden/fixture kuralları: `docs/GOLDEN_CONTRACT.md`
- Toolchain: `docs/TOOLCHAIN.md`
- Cihaz/benchmark matrisi: `docs/DEVICE_MATRIX.md`
- Mimari kararlar: `docs/ADR/`
- Lisans ve dependency politikası: `compliance/`
- Release belgeleri: `docs/release/`

Git geçmişi, eski ayrıntılı aşama kanıtlarının tek arşividir; çalışma ağacı yalnız güncel sistemi anlatmalıdır.
