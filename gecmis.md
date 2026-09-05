# mobil-dwg — Kısa Proje Geçmişi

Bu dosya yalnız kalıcı tarihçe ve teknik karar özetidir. Aktif iş listesi, cursor veya başlangıç komutu değildir.

## V1 kapanış durumu

```text
ACTIVE_PRODUCT: ANDROID_ONLY
PRODUCT_MODE: LOCAL_OFFLINE_READ_ONLY_2D_DWG_DXF_VIEWER
V1_IMPLEMENTATION: COMPLETED_THROUGH_STAGE_27
LEGACY_ANDROID_REVALIDATION_V01_V09: CLOSED
IOS: DEFERRED_FUTURE_OPTION
PHYSICAL_ANDROID_FIDELITY: NOT_CLAIMED_BY_A27
LAST_V1_EVIDENCE: docs/evidence/STAGE_27.md
DOCUMENTATION_CLEANUP: 2026-09-05
```

Android v1 planı AŞAMA 27 ile tamamlandı. Bundan sonraki geliştirmeler eski aşama sırasının devamı olarak değil, yeni bug fix / kalite / performans / özellik işleri olarak açılır.

## Ana milestone özeti

- AŞAMA 00–04: repo, toolchain, dependency/provenance ve 4 katmanlı mimari temeli.
- AŞAMA 05–09: ACadSharp read-only parser baseline, safe-open, ProCad NO-GO, RenderScene/camera temeli.
- AŞAMA 10–18: geometri renderer, viewport/gesture, block, style, text/font, dimension/hatch, layout/viewport, external reference ve viewer lifecycle.
- AŞAMA 19–22: corrupt/resource guard, performans/memory, corpus regression ve Android release RC.
- AŞAMA 23–24: iOS future track; aktif Android v1 dışında bırakıldı.
- AŞAMA 25–27: beta blocker kapanışı, dependency freeze/final audit ve Android v1 release handoff.
- V01–V09: ilk aşamaların Android graph/runtime üzerinde claim-limited geriye dönük doğrulaması; program kapalıdır.

Ayrıntılı sonuçlar `docs/evidence/` altında, mimari kararlar `docs/ADR/` altında korunur.

## Kalıcı teknik kararlar

- Production hedef Android-only; iOS yalnız açık yeniden etkinleştirme kararıyla döner.
- Uygulama read-only viewer'dır; original CAD immutable, production writer/save yoktur.
- ACadSharp exact `3.7.1` read-only parser baseline olarak kullanılır.
- Exact unpatched ProCad production reuse `NO-GO` olarak kalır.
- World/document coordinates `double` tutulur; büyük koordinat + küçük detay hassasiyeti korunur.
- UI parser entity tiplerine doğrudan bağlanmaz.
- Unsupported/proxy/font/XREF/raster sorunları sessizce kaybolmaz.
- Runtime dependency ve redistributable asset'ler exact provenance/lisans denetimine tabidir.
- Emulator fiziksel Android sonucu değildir.
- Eski PASS marker'ı sonradan değiştirilen davranış için otomatik PASS sayılmaz.

## 2026-09-05 dokümantasyon temizliği

V1 tamamlandıktan sonra yeni işleri yanlış yönlendiren tamamlanmış plan/handoff belgeleri çalışma ağacından kaldırıldı. Bunlara eski `BASLA.md`, `BASLA_A10.md`, `DEVAM.md`, Android V01–V09 doğrulama planı, v1 nihai uygulama planı ve A10 workstream gibi dosyalar dahildir.

Aynı temizlikte eski AI/runner özel talimatları ve yinelenen test rehberleri kaldırılarak Android test bilgisi `docs/ANDROID_TESTING.md` altında birleştirildi.

Silinen dosyalar kaybolmuş tarihçe değildir; gerektiğinde Git geçmişinden görülebilir. Normal yeni işlerde kullanılmamalıdır.

## Bundan sonra gerçek kaynaklar

- Güncel kod: `main`
- Proje girişi: `README.md`
- Mimari: `docs/ARCHITECTURE.md`
- Test: `docs/ANDROID_TESTING.md`
- Golden/fixture: `docs/GOLDEN_CONTRACT.md`
- Toolchain: `docs/TOOLCHAIN.md`
- Compliance: `compliance/`
- Tarihsel kanıt: `docs/evidence/`
- Mimari karar: `docs/ADR/`
