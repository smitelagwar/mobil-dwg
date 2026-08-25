# mobil-dwg — Risk Register

Snapshot: 2026-08-25

| ID | Risk | Sinyal / kanıt | Etki | Durum | Zorunlu tepki |
|---|---|---|---|---|---|
| R-DEP-001 | ACadSharp belirli DWG/DXF'lerde fidelity kaybı | 3.7.1 mini corpus AŞAMA 05'te geçti; full fidelity/corpus henüz yok | Yanlış/eksik CAD görünümü | `CONTROLLED / OPEN_FOR_FULL_CORPUS` | Exact 3.7.1 pinini koru; V05 ve sonraki fidelity gate'lerinde sistematik hata varsa sürüm A/B ve known-failure listesi |
| R-DEP-002 | ACadSharp source submodule provenance | Source build `CSUtilities` submodule kullanıyor | Source build/notice zinciri eksik kalabilir | `CONTROLLED` | CSUtilities MIT kaydı korunur; production source fork yapılırsa exact submodule SHA ayrıca dondurulur |
| R-DEP-003 | SkiaSharp native artifact/third-party lisans zinciri | NuGet MIT, platform native asset; upstream Skia BSD-3-Clause | Native binary içinde gözden kaçan third-party notice/asset olabilir | `OPEN / REVIEW` | CI package graph + RC artifact extraction + third-party notices; unknown varsa release NO-GO |
| R-DEP-004 | ProCad ACadSharp lineage farkı | AŞAMA 07 exact source/fork lineage kaydedildi; candidate precision blocker ile reddedildi | Official ACadSharp candidate ile davranış/API farkı | `CONTROLLED / NO_GO` | Production graph dışında tut; yalnız açık karar ve yeni evidence ile yeniden aç |
| R-DEP-005 | ProCad MAUI/Skia version skew | AŞAMA 07 source/package graph farkı kaydedildi; production reuse NO-GO | Restore/runtime/preview riski | `CONTROLLED / NO_GO` | Production graph'a ekleme; V07 yalnız karar/graph regresyonunu doğrular |
| R-DEP-006 | ProCad package publication/lineage belirsizliği | Exact source-pinned candidate değerlendirildi ve reddedildi | Reproducibility ve supply-chain riski | `CONTROLLED / NO_GO` | Reddedilmiş package/source hattını runtime'a alma |
| R-DEP-007 | IxMilia.Dxf teknik/yaş riski | Latest NuGet 0.8.4 (2024), source repo daha sonra değişmiş | Test oracle/fallback davranışı eski olabilir | `CONTROLLED` | Runtime'a baştan ekleme; yalnız fixture/test oracle veya ayrı DXF fallback spike |
| R-DEP-008 | IxMilia.Dwg modern DWG kapsamı yetersizliği | Plan ve repo konumu modern production fallback'i desteklemiyor | Yanlış fallback güveni | `CONTROLLED` | Runtime fallback olarak kullanma |
| R-DEP-009 | IxMilia.Shx parser ile font lisansının karıştırılması | Parser MIT olsa da SHX font dosyaları ayrı asset | Proprietary font bundle riski | `CONTROLLED` | Parser lisansı ile font asset provenance ayrı tutulur; AutoCAD SHX bundle edilmez |
| R-DEP-010 | Floating/transitive dependency drift | NuGet alt sınırları ve future restore değişebilir | Tekrar üretilemeyen build | `MITIGATING` | CPM exact direct pins + `packages.lock.json` + CI `--locked-mode` |
| R-DEP-011 | Unknown/RED license runtime graph'a girer | Transitive/native package eklenmesi | Release policy ihlali | `MITIGATING` | Stage 02 CI license allowlist; RC artifact inventory; unknown/RED = fail |
| R-EXT-001 | Fiziksel Android kanıtı eksik | Emulator kuruludur fakat fiziksel cihaz matrisi açık | Üretici SAF/performance/lifecycle farkları doğrulanamaz | `DEFERRED_RELEASE_DEVICE_GATE` | Emulatoru fiziksel cihaz sayma; AŞAMA 20–22/final öncesi gerçek Android kanıtı al |
| R-EXT-002 | Self-hosted runner çevrim dışı veya kanıt gate'i hatalı | Listener yok, queued job, geçici APK, çalışmayan harness veya bozuk artifact | Yanlış PASS ya da gereksiz test kuyruğu | `OPEN / V01_FIX_REQUIRED` | Exact SHA kuyruğu; kod/host testine devam; harness/APK/PNG/PID/crash/ANR açıklarını V01'de düzelt |
| R-EXT-003 | Future iOS erişimi ve fizibilitesi açık | Mac/Xcode/iPhone yok; AŞAMA 08 yalnız partial characterization | Gelecekte iOS'a dönüş ek çalışma ister | `DEFERRED_FUTURE_IOS` | Android release'i bloke etme; shared sınırları koru; yalnız kullanıcı reactivation kararıyla AŞAMA 23–24'ü aç |
