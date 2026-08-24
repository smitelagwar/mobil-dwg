# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya yeni sohbet veya yeni bir yapay zeka oturumu başladığında projenin nerede kaldığını anlamak için tutulur. Sohbet belleğine güvenilmez; repo kayıtları kalıcı kaynaktır.

## Yeni bir ajan önce ne okumalı?

1. `gecmis.md`
2. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` — dış donanım/hesap blocker'larının kullanıcı onayıyla ertelenmesine ilişkin aktif yürütme istisnası
3. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` — ürün/teknik plan ve aşamalar
4. `docs/evidence/STAGE_02.md` — son tamamlanan bağımsız aşamanın kapanış kanıtı
5. `compliance/DEPENDENCY_EVIDENCE.md` — exact dependency/source/artifact kanıtları
6. `docs/EXECUTION_LOG.md` — ayrıntılı teknik yürütme geçmişi
7. `docs/TOOLCHAIN.md`
8. `docs/evidence/STAGE_01.md`
9. `docs/STAGE_01_IOS_ACCESS_INVENTORY.md`
10. Gerekirse `docs/ADR/`

Diğer `*_oneriler.md`, `Master_Plan.md` ve benzeri belgeler araştırma/önceki görüş kaynağıdır; ürün kapsamı ve teknik ilkelerde nihai plan esas alınır. Yürütme sırasındaki dış erişim blocker'larının ertelenmesi konusunda `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` kullanıcının daha yeni ve açık talebidir.

## Repo kimliği

- GitHub: `smitelagwar/mobil-dwg`
- Repo: `mobil-dwg`
- Default branch: `main`
- Private repo
- Ürün: Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer
- v1: viewer-only; edit/save/export/cloud/account kapsam dışında

## Aktif checkpoint

```text
LAST_COMPLETED_STAGE: AŞAMA 02
DEFERRED_STAGE: AŞAMA 01
DEFERRED_STAGE_STATUS: BLOCKED / DEFERRED_EXTERNAL_GATE — dış erişim bekliyor, DONE değil
NEXT_WORK_STAGE: AŞAMA 03
NEXT_WORK_STATUS: NOT_STARTED
USER_CONSTRAINT: Kullanıcı şu an fiziksel Android cihaz/gerçek geliştirme makinesi ve Mac/Xcode/iPhone/Apple Developer erişim kanıtlarını sağlayamıyor; şu an yapabileceği temel etkileşim "devam" demek.
USER_APPROVAL: Dış erişim kapıları sahte PASS/DONE yapılmadan ertelensin; bağımsız aşamalara devam edilsin.
DEFERRED_EXTERNAL_GATES: STAGE01_DEVICE_GATE_PASS; local Android install/launch; iOS erişim envanteri YES/NO/N/A
LAST_VERIFIED_STAGE02_CI: GitHub Actions Stage 02 Dependency Audit run 32747785867 / #9 SUCCESS; artifact 9527769476; sha256:90d41760e306e13b9977586b9996c1aafdf27f615c2b730bb41d74507b4684f3
LAST_VERIFIED_STAGE01_REGRESSION_CI: GitHub Actions Stage 01 Toolchain Smoke run 32747785948 / #29 SUCCESS; artifact 9528014030; sha256:57f01ed14600684a5a9434b9ca2db2b6e32b4a9fac95bee90d455a4595e8421e
LAST_STAGE_MERGE: PR #4 -> main; merge commit f0a43db6cc3aee9103f42798fa124da4d1ff39d1
EXECUTION_OVERRIDE: docs/USER_APPROVED_EXECUTION_OVERRIDE.md
NEXT_ACTION: Kullanıcı "devam" dediğinde AŞAMA 03 — test corpus'u, golden sözleşmesi ve cihaz matrisi — başlat; aynı turda AŞAMA 04'e geçme.
LAST_UPDATE: 2026-08-24
```

## Yürütme kuralı — 2026-08-24 kullanıcı revizyonu

AŞAMA 01'in fiziksel Android ve iOS erişim kapıları bu oturumdan doğrulanamıyor. Kullanıcı bunların sahte PASS/DONE yapılmadan ertelenmesini ve fiziksel erişime bağımlı olmayan aşamaların `devam` komutuyla ilerlemesini açıkça onayladı.

Bu nedenle:

- AŞAMA 01 `DONE` değildir; eksik dış kapıları açık kalır.
- Bu kapılar `DEFERRED_EXTERNAL_GATE` olarak ele alınır.
- Kullanıcı `devam` dediğinde fiziksel erişime bağımlı olmayan `NEXT_WORK_STAGE` başlatılır.
- Bir turda en fazla bir aşama tamamlanır.
- Sonraki aşamanın gerçek çıkış kriteri fiziksel cihaz/Mac/hesap gerektiriyorsa, erişimsiz yapılabilecek alt işler bitirilir ve o aşama dürüstçe `BLOCKED` bırakılır.
- Final Definition of Done değişmez; gerçek Android ve iOS cihaz kanıtları release öncesinde zorunludur.

Ayrıntılı kural: `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`.

## Aşama durumu

- [x] AŞAMA 00 — Çalışma alanı ve yürütme zemini — `DONE`
- [ ] AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler — `DONE`
- [ ] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi — `NEXT`
- [ ] AŞAMA 04 — Minimal solution ve mimari sınırlar
- [ ] AŞAMA 05 — ACadSharp headless parser spike
- [ ] AŞAMA 06 — Android güvenli dosya alma ve parse spike
- [ ] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO
- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke
- [ ] AŞAMA 09 — RenderScene, kamera ve diagnostics temeli
- [ ] AŞAMA 10 — P0 temel geometri renderer’ı
- [ ] AŞAMA 11 — Mobil viewport ve gesture’lar
- [ ] AŞAMA 12 — Block/INSERT/attribute dönüşümleri
- [ ] AŞAMA 13 — Layer, renk, linetype ve lineweight
- [ ] AŞAMA 14 — TEXT/MTEXT, Türkçe, font ve SHX
- [ ] AŞAMA 15 — Dimension, leader ve hatch doğruluğu
- [ ] AŞAMA 16 — Model space, layout, paper space ve viewport
- [ ] AŞAMA 17 — XREF/raster/underlay ve compatibility raporu
- [ ] AŞAMA 18 — Tam Android viewer UX ve lifecycle
- [ ] AŞAMA 19 — Kötü niyetli/bozuk dosya ve resource guard’ları
- [ ] AŞAMA 20 — Ölçümlü performans ve bellek optimizasyonu
- [ ] AŞAMA 21 — Android tam corpus regresyon ve beta kapısı
- [ ] AŞAMA 22 — Android Release/AAB/compliance RC
- [ ] AŞAMA 23 — iOS toolchain, shared core ve ilk gerçek cihaz
- [ ] AŞAMA 24 — iOS fidelity, lifecycle ve Release archive
- [ ] AŞAMA 25 — Cross-platform beta ve yalnız blocker düzeltmeleri
- [ ] AŞAMA 26 — Dependency freeze, final audit ve RC onayı
- [ ] AŞAMA 27 — v1 artifact, yayın/handoff ve kapanış

## Tarihçe özeti

### AŞAMA 00 — DONE

Başlangıçta repo plan/araştırma belgeleri içeriyordu. Repo ve `.gitignore` doğrulandı; kullanıcı belgeleri korundu. `docs/EXECUTION_LOG.md`, `docs/ADR/0000-template.md`, `docs/EVIDENCE_TEMPLATE.md` ve bu handoff kaydı oluşturuldu. Başlangıç doğrulanan revision `d161b5c4f9ba238f0d2a2e4c92f773535f379487`; AŞAMA 00 kapanış plan commit'i `fe3c8c043e6d373e6313d2e1201cc24992b493a9`.

### AŞAMA 01 — bağımsız kısım tamamlandı, dış kapılar ertelendi

Canlı doğrulanan/pinlenen hat:

- .NET SDK/workload set `10.0.400`
- Microsoft OpenJDK `21.0.12`
- Android min API `24`, target/compile API `36`
- Build-Tools `36.0.0`
- Platform-Tools `37.0.1`
- `maui-android`

`global.json`, `docs/TOOLCHAIN.md`, Stage 01 evidence ve GitHub Actions smoke hattı oluşturuldu. Temiz MAUI uygulaması Debug/Release build edildi; manifest API 24/36 doğrulandı. Fiziksel cihaz gate scriptleri ve iOS erişim envanteri altyapısı eklendi.

Stage 02 sırasında root Central Package Management regresyonuna karşı Stage 01 smoke hattı yeniden izole edildi. Final regresyon run `32747785948` / #29 `SUCCESS`; clean MAUI creation, Debug, Release, manifest/API ve artifact upload PASS. Artifact `9528014030`, SHA-256 `57f01ed14600684a5a9434b9ca2db2b6e32b4a9fac95bee90d455a4595e8421e`.

Bu CI fiziksel telefon kanıtı değildir. Açık dış kapılar:

- Gerçek geliştirme makinesinde `STAGE01_DEVICE_GATE_PASS`
- Fiziksel Android cihaz `device`, install ve launch
- Mac/Xcode/iPhone/Apple Developer erişim envanteri

### AŞAMA 02 — DONE

Canlı dependency/lisans/source denetimi ve exact lock modeli tamamlandı.

Exact ana pinler:

- ACadSharp `3.7.1` — `GREEN` dependency/lisans adayı; fidelity AŞAMA 05'te kanıtlanacak.
- SkiaSharp `4.151.1` — `REVIEW`; Android native asset kanıtı var, final binary/third-party inventory release öncesi tekrar açılacak.
- IxMilia.Dxf `0.8.4` — yalnız test/fallback scope'unda `GREEN`.
- ProCad source `f8a862b3e7634e27664fee02ff5d68774b102985` — `REVIEW`, production default `NO-GO`, yalnız AŞAMA 07 source-pinned spike.
- IxMilia.Dwg/Shx — `REVIEW`, runtime dışında.

Kalıcı mekanizmalar:

- `Directory.Packages.props`
- `compliance/Stage02.DependencyProbe/packages.lock.json`
- `compliance/stage02-package-manifest.json`
- `compliance/DEPENDENCY_EVIDENCE.md`
- `compliance/LICENSE_POLICY.md`
- `compliance/RISK_REGISTER.md`
- `scripts/stage02-audit-packages.py`
- `.github/workflows/stage02-dependency-audit.yml`
- `docs/evidence/STAGE_02.md`

Resolved Android dependency graph: ACadSharp 3.7.1 + SkiaSharp 4.151.1 + transitive SkiaSharp.NativeAssets.Android 4.151.1. Unknown/policy-RED package yok; committed lock + `--locked-mode`; exact `.nupkg` SHA-256/license/native-entry manifest'i var.

Final Stage 02 CI run `32747785867` / #9 `SUCCESS`. Evidence artifact `9527769476`, SHA-256 `90d41760e306e13b9977586b9996c1aafdf27f615c2b730bb41d74507b4684f3`.

PR #4 doğrulanmış head `7daa5d7dc326915700f60396bdf50604bf0601e7` üzerinden merge edildi. Merge commit `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.

Ayrıntı ve artifact hashleri: `docs/evidence/STAGE_02.md` ve `compliance/DEPENDENCY_EVIDENCE.md`.

## Değiştirilemez temel teknik kararlar

- v1 yalnız 2D viewer; edit/write yok.
- DWG/DXF cihazda/offline okunur; zorunlu cloud conversion yok.
- Autodesk RealDWG, APS/Forge dönüşümü, ticari ODA SDK, trial/ücretli CAD parser-renderer yok.
- Varsayılan lisans allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD; exact dependency yine denetlenir.
- Runtime graph’ta GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/unknown bileşen release blocker'dır.
- ACadSharp parser adayıdır; corpus gate geçmeden production-approved değildir.
- SkiaSharp renderer adayıdır; native dağıtım zinciri ayrıca denetlenir.
- ProCad yalnız source-pinned izole spike; otomatik production dependency değildir.
- UI parser entity'lerine doğrudan bağlanmaz.
- Unsupported/proxy/font/XREF/raster kaybı sessiz olmaz.
- Original drawing overwrite edilmez.

## Yeni ajan için protokol

1. Bu dosyayı ve `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` dosyasını oku.
2. Gerçek `main` durumunu doğrula; eski SHA'lara körlemesine güvenme.
3. Kullanıcı değişikliklerini koru; destructive Git işlemi yapma.
4. Kullanıcı yalnız `devam` diyorsa `NEXT_WORK_STAGE` üzerinden ilerle. Şu anda bu AŞAMA 03'tür.
5. AŞAMA 01 dış erişim kapılarını sahte PASS/DONE yapma; evidence içinde açık tut.
6. Bir turda en fazla bir aşama tamamla.
7. `[LIVE-VERIFY]` noktalarında resmi ve güncel kaynak kullan.
8. Her turun sonunda `gecmis.md`, ilgili evidence kaydı ve mümkün olan diğer yürütme kayıtlarını gerçek durumla güncelle.
9. Nihai plan checkpoint'i veya monolitik execution log gerçek repo durumunu geçici olarak geriden takip ederse, `gecmis.md` + stage evidence + gerçek `main` durumu çalışma kaynağıdır; güvenli tam-dosya güncellemesinde yeniden senkronize edilir.
10. Release/beta/cihaz bağımlı milestone'lara gelindiğinde ertelenen dış kapıları yeniden aç.

## Bir sonraki tur

Kullanıcı `devam` dediğinde AŞAMA 03 — test corpus'u, golden sözleşmesi ve cihaz matrisi — başlatılacak. AŞAMA 01'in ertelenen fiziksel/iOS kanıtları açık risk olarak taşınmaya devam edecek. AŞAMA 04 aynı turda başlatılmayacak.
