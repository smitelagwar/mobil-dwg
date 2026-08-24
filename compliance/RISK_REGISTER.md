# mobil-dwg — Risk Register

Snapshot: 2026-08-24

| ID | Risk | Sinyal / kanıt | Etki | Durum | Zorunlu tepki |
|---|---|---|---|---|---|
| R-DEP-001 | ACadSharp belirli DWG/DXF'lerde fidelity kaybı | Package teknik kapsamı geniş olsa da corpus doğrulaması henüz yok | Yanlış/eksik CAD görünümü | `OPEN` | 3.7.1 AŞAMA 05 mini corpus gate; sistematik hata varsa sürüm A/B ve known-failure listesi |
| R-DEP-002 | ACadSharp source submodule provenance | Source build `CSUtilities` submodule kullanıyor | Source build/notice zinciri eksik kalabilir | `CONTROLLED` | CSUtilities MIT kaydı korunur; production source fork yapılırsa exact submodule SHA ayrıca dondurulur |
| R-DEP-003 | SkiaSharp native artifact/third-party lisans zinciri | NuGet MIT, platform native asset; upstream Skia BSD-3-Clause | Native binary içinde gözden kaçan third-party notice/asset olabilir | `OPEN / REVIEW` | CI package graph + RC artifact extraction + third-party notices; unknown varsa release NO-GO |
| R-DEP-004 | ProCad ACadSharp lineage farkı | ProCad `wieslawsoltes/ACadSharp` fork submodule'una pinli | Official ACadSharp candidate ile davranış/API farkı | `OPEN / REVIEW` | AŞAMA 07 exact SHA diff; production'a kendiliğinden ekleme yok |
| R-DEP-005 | ProCad MAUI/Skia version skew | Snapshot: SkiaSharp 3.119.4, Maui view 4.147.0-preview.2.1 | Restore/runtime/preview riski | `OPEN / REVIEW` | AŞAMA 07 source-pinned spike; production default NO-GO |
| R-DEP-006 | ProCad package publication/lineage belirsizliği | README `ProCadSharp.*` yayın akışını tanımlar; NuGet exact aramada paket bulunmadı | Reproducibility ve supply-chain riski | `OPEN / REVIEW` | Yalnız source commit/submodule SHA kullan; package bulunursa ayrıca exact artifact audit |
| R-DEP-007 | IxMilia.Dxf teknik/yaş riski | Latest NuGet 0.8.4 (2024), source repo daha sonra değişmiş | Test oracle/fallback davranışı eski olabilir | `CONTROLLED` | Runtime'a baştan ekleme; yalnız fixture/test oracle veya ayrı DXF fallback spike |
| R-DEP-008 | IxMilia.Dwg modern DWG kapsamı yetersizliği | Plan ve repo konumu modern production fallback'i desteklemiyor | Yanlış fallback güveni | `CONTROLLED` | Runtime fallback olarak kullanma |
| R-DEP-009 | IxMilia.Shx parser ile font lisansının karıştırılması | Parser MIT olsa da SHX font dosyaları ayrı asset | Proprietary font bundle riski | `CONTROLLED` | Parser lisansı ile font asset provenance ayrı tutulur; AutoCAD SHX bundle edilmez |
| R-DEP-010 | Floating/transitive dependency drift | NuGet alt sınırları ve future restore değişebilir | Tekrar üretilemeyen build | `MITIGATING` | CPM exact direct pins + `packages.lock.json` + CI `--locked-mode` |
| R-DEP-011 | Unknown/RED license runtime graph'a girer | Transitive/native package eklenmesi | Release policy ihlali | `MITIGATING` | Stage 02 CI license allowlist; RC artifact inventory; unknown/RED = fail |
| R-EXT-001 | AŞAMA 01 gerçek cihaz/Mac kapıları eksik | Kullanıcı şu an dış erişim sağlayamıyor | Release/device davranışı doğrulanamaz | `DEFERRED_EXTERNAL_GATE` | `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`; bağımsız işler sürer, release öncesi gerçek kanıt zorunlu |
