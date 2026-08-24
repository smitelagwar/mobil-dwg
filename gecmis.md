# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya yeni sohbet veya yeni bir yapay zeka oturumu başladığında projenin nerede kaldığını anlamak için tutulur. Sohbet belleğine güvenilmez; repo kayıtları kalıcı kaynaktır.

## Yeni bir ajan önce ne okumalı?

1. `gecmis.md`
2. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` — dış donanım/hesap blocker'larının kullanıcı onayıyla ertelenmesine ilişkin aktif yürütme istisnası
3. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` — ürün/teknik plan ve aşamalar
4. `docs/EXECUTION_LOG.md` — ayrıntılı teknik yürütme geçmişi
5. `docs/TOOLCHAIN.md`
6. `docs/evidence/STAGE_01.md`
7. `docs/STAGE_01_IOS_ACCESS_INVENTORY.md`
8. Gerekirse `docs/ADR/`

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
LAST_COMPLETED_STAGE: AŞAMA 00
DEFERRED_STAGE: AŞAMA 01
DEFERRED_STAGE_STATUS: BLOCKED — dış erişim bekliyor, DONE değil
NEXT_WORK_STAGE: AŞAMA 02
NEXT_WORK_STATUS: NOT_STARTED
USER_CONSTRAINT: Kullanıcı şu an fiziksel Android cihaz/gerçek geliştirme makinesi ve Mac/Xcode/iPhone/Apple Developer erişim kanıtlarını sağlayamıyor; şu an yapabileceği tek etkileşim "devam" demek.
USER_APPROVAL: Dış erişim kapıları sahte PASS/DONE yapılmadan ertelensin; bağımsız aşamalara devam edilsin.
DEFERRED_EXTERNAL_GATES: STAGE01_DEVICE_GATE_PASS; local Android install/launch; iOS erişim envanteri YES/NO/N/A
LAST_VERIFIED_CI: GitHub Actions run 32742123997 SUCCESS; artifact 9525825066 sha256:bf66fd4c3e7a4b1f6a40ed7c2fd868f298f7087a95f9c29cdfbb8aa82e7f1115; PR #3 merge 9a397065a55c5844993e6ef909438f44ad5aa1f6
EXECUTION_OVERRIDE: docs/USER_APPROVED_EXECUTION_OVERRIDE.md
NEXT_ACTION: Kullanıcı "devam" dediğinde AŞAMA 02'yi başlat; AŞAMA 01'in fiziksel/iOS kapılarını uydurma, silme veya DONE yapma.
LAST_UPDATE: 2026-08-24
```

## Yürütme kuralı — 2026-08-24 kullanıcı revizyonu

Önceki protokol `BLOCKED` AŞAMA 01 varken AŞAMA 02'ye geçmiyordu. Kullanıcı artık fiziksel cihaz/Mac erişimini şu an sağlayamayacağını ve yalnız `devam` diyerek projenin ilerlemesini istediğini açıkça belirtti.

Bu nedenle:

- AŞAMA 01 `DONE` değildir; eksik dış kapıları açık kalır.
- Bu kapılar `DEFERRED_EXTERNAL_GATE` olarak ele alınır.
- Kullanıcı `devam` dediğinde fiziksel erişime bağımlı olmayan bir sonraki aşama başlatılır.
- Bir turda yine en fazla bir aşama tamamlanır.
- Sonraki aşamanın gerçek çıkış kriteri fiziksel cihaz/Mac/hesap gerektiriyorsa, erişimsiz yapılabilecek alt işler bitirilir ve o aşama dürüstçe `BLOCKED` bırakılır.
- Final Definition of Done değişmez; gerçek Android ve iOS cihaz kanıtları release öncesinde zorunludur.

Ayrıntılı kural: `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`.

## Aşama durumu

- [x] AŞAMA 00 — Çalışma alanı ve yürütme zemini — `DONE`
- [ ] AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [ ] AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler — `NEXT`
- [ ] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi
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

### AŞAMA 00

Başlangıçta repo plan/araştırma belgeleri içeriyordu. Repo ve `.gitignore` doğrulandı; kullanıcı belgeleri korundu. `docs/EXECUTION_LOG.md`, `docs/ADR/0000-template.md`, `docs/EVIDENCE_TEMPLATE.md` ve bu handoff kaydı oluşturuldu. Başlangıç doğrulanan revision `d161b5c4f9ba238f0d2a2e4c92f773535f379487`; AŞAMA 00 kapanış plan commit'i `fe3c8c043e6d373e6313d2e1201cc24992b493a9`.

### AŞAMA 01 — tamamlanan bağımsız kısım

Canlı doğrulanan/pinlenen hat:

- .NET SDK/workload set `10.0.400`
- Microsoft OpenJDK `21.0.12`
- Android min API `24`, target/compile API `36`
- Build-Tools `36.0.0`
- Platform-Tools `37.0.1`
- `maui-android`

`global.json`, `docs/TOOLCHAIN.md`, Stage 01 evidence ve GitHub Actions smoke hattı oluşturuldu. Temiz MAUI uygulaması Debug/Release build edildi; manifest API 24/36 doğrulandı. Fiziksel cihaz gate scriptleri eklendi:

- `scripts/stage01-device-gate.ps1`
- `scripts/stage01-device-gate.sh`

Ayrıca iOS erişim envanteri altyapısı eklendi:

- `scripts/stage01-ios-inventory.sh`
- `docs/STAGE_01_IOS_ACCESS_INVENTORY.md`

En güncel CI: run `32742123997` / #20 `SUCCESS`. Artifact `9525825066`, SHA-256 `bf66fd4c3e7a4b1f6a40ed7c2fd868f298f7087a95f9c29cdfbb8aa82e7f1115`. PR #3 merge commit `9a397065a55c5844993e6ef909438f44ad5aa1f6`.

Önemli teknik bulgular:

- Temiz .NET MAUI 10 şablonu Android min API 21 üretir; proje açıkça API 24 pinler.
- `global.json` workload set `10.0.400` seçtiği için doğru kurulum `dotnet workload install maui-android`; eski ek `--version` kullanımı kaldırıldı.
- CI fiziksel telefon kanıtı değildir.

### AŞAMA 01 — ertelenen dış kapılar

- Gerçek geliştirme makinesinde `STAGE01_DEVICE_GATE_PASS`
- Fiziksel Android cihaz `device`, install ve launch
- Mac/Xcode/iPhone/Apple Developer erişim envanteri

Kullanıcı 2026-08-24 tarihinde bunları şu an yapamayacağını açıkça bildirdi. Bu yüzden kapılar açık kalır fakat bağımsız aşamalar artık bloke edilmez.

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
4. Kullanıcı yalnız `devam` diyorsa `NEXT_WORK_STAGE` üzerinden ilerle. Şu anda bu AŞAMA 02'dir.
5. AŞAMA 01 dış erişim kapılarını sahte PASS/DONE yapma; evidence içinde açık tut.
6. Bir turda en fazla bir aşama tamamla.
7. `[LIVE-VERIFY]` noktalarında resmi ve güncel kaynak kullan.
8. Her turun sonunda `gecmis.md`, `docs/EXECUTION_LOG.md`, ilgili evidence ve mümkünse canonical checkpoint'i gerçek durumla güncelle.
9. Release/beta/cihaz bağımlı milestone'lara gelindiğinde ertelenen dış kapıları yeniden aç.

## Bir sonraki tur

Kullanıcı `devam` dediğinde AŞAMA 02 — canlı dependency/lisans kanıtı ve kilitler — başlatılacak. AŞAMA 01'in ertelenen fiziksel/iOS kanıtları açık risk olarak taşınmaya devam edecek.
