# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya yeni sohbet veya yeni bir yapay zeka oturumu başladığında projenin nerede kaldığını anlamak için tutulur. Sohbet/model belleğine güvenilmez; repo kayıtları kalıcı kaynaktır.

## Yeni bir ajan önce ne okumalı?

1. `gecmis.md`
2. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`
3. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`
4. `docs/evidence/STAGE_05.md`
5. `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`
6. `docs/ARCHITECTURE.md` ve `MobilDwg.sln`
7. `docs/evidence/STAGE_04.md`
8. `docs/evidence/STAGE_03.md`, `fixtures/manifest/stage03-mini.json`, `fixtures/manifest/stage03-source-integrity.json`, `docs/GOLDEN_CONTRACT.md`, `docs/DEVICE_MATRIX.md`
9. `docs/evidence/STAGE_02.md` ve `compliance/DEPENDENCY_EVIDENCE.md`
10. `docs/EXECUTION_LOG.md`
11. `docs/TOOLCHAIN.md`, `docs/evidence/STAGE_01.md`, `docs/STAGE_01_IOS_ACCESS_INVENTORY.md`

## Repo kimliği

- GitHub: `smitelagwar/mobil-dwg`
- Default branch: `main`
- Private repo
- Ürün: Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer
- v1: viewer-only; edit/save/export/cloud/account kapsam dışında

## Aktif checkpoint

```text
LAST_COMPLETED_STAGE: AŞAMA 05
DEFERRED_STAGE: AŞAMA 01
DEFERRED_STAGE_STATUS: BLOCKED / DEFERRED_EXTERNAL_GATE — dış erişim bekliyor, DONE değil
NEXT_WORK_STAGE: AŞAMA 06
NEXT_WORK_STATUS: NOT_STARTED
USER_CONSTRAINT: Kullanıcı şu an fiziksel Android cihaz/gerçek geliştirme makinesi ve Mac/Xcode/iPhone/Apple Developer erişim kanıtlarını sağlayamıyor; temel etkileşimi "devam" demek.
USER_APPROVAL: Dış erişim kapıları sahte PASS/DONE yapılmadan ertelensin; bağımsız aşamalara devam edilsin.
DEFERRED_EXTERNAL_GATES: STAGE01_DEVICE_GATE_PASS; local Android install/launch; iOS erişim envanteri YES/NO/N/A
LAST_VERIFIED_STAGE05_CI: Stage 05 Parser Spike run 32760139261 / #15 SUCCESS; locked restore; Release build 0 warning / 0 error; STAGE05_DEPENDENCY_BOUNDARY_PASS; STAGE05_MINI_CORPUS_PASS fixtures=9 derived_negatives=2; STAGE05_T3_PASS; artifact 9532379884; sha256:f3b31c937186d874a0ed23c045951d465ace5da8fff2f9acc32006c4352e2f60
LAST_VERIFIED_STAGE04_REGRESSION_CI: Stage 04 Architecture run 32760139230 / #18 SUCCESS
LAST_VERIFIED_STAGE02_REGRESSION_CI: Stage 02 Dependency Audit run 32760139219 / #32 SUCCESS
LAST_VERIFIED_STAGE01_REGRESSION_CI: Stage 01 Toolchain Smoke run 32760139285 / #51 SUCCESS; physical device evidence değildir.
STAGE05_IMPLEMENTATION_HEAD: 09e26172aa8de9e8c79ae64853a493dab1d0e5b9
STAGE05_FINAL_PR_HEAD: 80cdaf49d3ad4298f3b1d56fe1dbac89b352ec7f
STAGE05_PR: #7 — stage05: validate ACadSharp headless parser
LAST_STAGE_MERGE: PR #7 -> main; merge commit bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f
STAGE05_MERGE: bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f
EXECUTION_OVERRIDE: docs/USER_APPROVED_EXECUTION_OVERRIDE.md
NEXT_ACTION: AŞAMA 06 — Android güvenli dosya alma ve parse spike. Bu AŞAMA 05 kapanış turunda AŞAMA 06 başlatılmaz.
LAST_UPDATE: 2026-08-24
```

## Yürütme kuralı

AŞAMA 01'in gerçek Android/iOS dış erişim kapıları kullanıcı tarafından şimdilik ertelendi. Bunlar sahte PASS/DONE yapılmaz; `DEFERRED_EXTERNAL_GATE` olarak açık tutulur. Fiziksel erişime bağımlı olmayan sonraki aşamalar `NEXT_WORK_STAGE` sırasıyla ilerler. Bir turda en fazla bir aşama tamamlanır. Release/beta/final cihaz kapılarında ertelenmiş dış kanıtlar yeniden zorunlu olur.

## Aşama durumu

- [x] AŞAMA 00 — Çalışma alanı ve yürütme zemini — `DONE`
- [ ] AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon — `BLOCKED / DEFERRED_EXTERNAL_GATE`
- [x] AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler — `DONE`
- [x] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi — `DONE`
- [x] AŞAMA 04 — Minimal solution ve mimari sınırlar — `DONE`
- [x] AŞAMA 05 — ACadSharp headless parser spike — `DONE`
- [ ] AŞAMA 06 — Android güvenli dosya alma ve parse spike — `NEXT`
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

Repo ve `.gitignore` doğrulandı; kullanıcı belgeleri korundu. `docs/EXECUTION_LOG.md`, `docs/ADR/0000-template.md`, `docs/EVIDENCE_TEMPLATE.md` ve `gecmis.md` oluşturuldu. Başlangıç revision `d161b5c4f9ba238f0d2a2e4c92f773535f379487`; AŞAMA 00 kapanış plan commit'i `fe3c8c043e6d373e6313d2e1201cc24992b493a9`.

### AŞAMA 01 — bağımsız kısım tamamlandı, dış kapılar ertelendi

Pinli hat: .NET SDK/workload set `10.0.400`, Microsoft OpenJDK `21.0.12`, Android min API `24`, target/compile API `36`, Build-Tools `36.0.0`, Platform-Tools `37.0.1`, `maui-android`. CI temiz MAUI Debug/Release ve manifest 24/36 kapılarını geçti. Fiziksel cihaz gate scriptleri ve iOS inventory helper eklendi. Gerçek telefon install/launch ve gerçek iOS erişim envanteri hâlâ açık dış kapıdır. AŞAMA 05 final PR head regresyonu `Stage 01 Toolchain Smoke` run `32760139285` / #51 `SUCCESS`; bu CI fiziksel cihaz kanıtı değildir.

### AŞAMA 02 — DONE

Exact dependency/compliance hattı kuruldu. ACadSharp `3.7.1` dependency/lisans açısından `GREEN`; fidelity/parse kabulü AŞAMA 05'e bırakılmıştı. SkiaSharp `4.151.1` `REVIEW`; ProCad source `f8a862b3e7634e27664fee02ff5d68774b102985` yalnız AŞAMA 07 source-pinned spike ve production default `NO-GO`; IxMilia.Dxf `0.8.4` yalnız test/fallback. Central Package Management, committed lockfile, exact `.nupkg` hash/license manifest'i ve CI audit kuruldu. PR #4 merge commit `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.

### AŞAMA 03 — DONE

Tekrar üretilebilir corpus/golden sözleşmesi kuruldu:

- 4 remote-pinned DWG familyası: R2000/AC1015, R2004/AC1018, R2010/AC1024, R2018/AC1032.
- 2 remote-pinned ASCII DXF: R2000 ve R2018.
- Mobil-dwg tarafından yazılmış 0BSD sentetik DXF: Türkçe/basic/nested-block.
- Missing-font ve missing-XREF sentetik negatifleri.
- CI-derived deterministic truncated ve corrupt DWG negatifleri.
- Upstream fixture'lar `DomCR/ACadSharp` immutable revision `592d70a7bf0eaffbd932d23900f289b4e6305832` üzerinden remote-reference-only; mobil-dwg reposuna vendored edilmez.
- `fixtures/manifest/schema.json`, `stage03-mini.json`, `stage03-source-integrity.json`, golden contract, device matrix ve iki validator scripti eklendi.
- Private fixture path Git-ignore guard ile korunur.
- Final Stage 03 run `32752374980` / #4 SUCCESS; `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`, `STAGE03_DUAL_HASH_PASS fixtures=6`.
- Aynı head Stage 02 run #15 ve Stage 01 run #34 SUCCESS.
- PR #5 merge commit `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`.

Ayrıntı: `docs/evidence/STAGE_03.md`.

### AŞAMA 04 — DONE

Minimal derlenebilir mimari iskelet kuruldu:

- Dört production proje: `MobilDwg.Core`, `MobilDwg.Cad`, `MobilDwg.Rendering`, `MobilDwg.App`.
- Üç test proje: `MobilDwg.Core.Tests`, `MobilDwg.Rendering.Tests`, `MobilDwg.Architecture.Tests`.
- `MobilDwg.Core` BCL-only; ProjectReference ve PackageReference yok; MAUI/SkiaSharp/ACadSharp bağımlılığı yok.
- `MobilDwg.Cad` ve `MobilDwg.Rendering` yalnız Core'a; `MobilDwg.App` Core/Cad/Rendering sınırlarına bağımlı.
- `ICadDocumentReader`, `CadDocumentSession`, `ICadDocumentHandle`, diagnostics/compatibility, `IRenderSceneBuilder`, `ICadRenderer`, render surface/viewport kontratları eklendi.
- Session concrete parser handle'ın tek sahibi olarak idempotent `IAsyncDisposable` yaşam döngüsü sağlar.
- Cancellation desteği `None/BeforeStartOnly/Cooperative`, progress desteği `None/StagesOnly/Fractional` olarak açık capability modeliyle tanımlandı; bilinmeyen yüzde `null` kalır.
- Architecture harness tam 4 production/3 test proje sayısını, exact ProjectReference yönlerini ve dependency sınırlarını otomatik test eder.
- Final `Stage 04 Architecture` run `32755230695` / #2 SUCCESS: solution restore, Release build `0 Warning(s) / 0 Error(s)`, `STAGE04_CORE_CONTRACT_TESTS_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE04_T0_PASS`.
- AŞAMA 05 final PR head regresyonu `Stage 04 Architecture` run `32760139230` / #18 `SUCCESS`.
- PR #6 merge commit `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.

Ayrıntı: `docs/evidence/STAGE_04.md` ve `docs/ARCHITECTURE.md`.

### AŞAMA 05 — DONE

Pinned ACadSharp ile headless parser baseline gerçek corpus üzerinde doğrulandı:

- ACadSharp `3.7.1` yalnız `MobilDwg.Cad` adapter projesine eklendi; Core/App/Rendering katmanlarına ACadSharp tipi sızmıyor.
- NuGet-generated `src/MobilDwg.Cad/packages.lock.json` commit edildi ve final gate `--locked-mode` restore kullanıyor.
- `AcadSharpDocumentReader` DWG/DXF preflight, notifications, exceptions, parse timing ve compatibility kayıtlarını Core kontratlarına bağlıyor.
- Parser cancellation capability `BeforeStartOnly`, progress `StagesOnly`; parser başladıktan sonra sahte cooperative cancellation/yüzde yok.
- Stage 03 mini corpus'un 9 fixture'ı + 2 derived negative gerçek CI'da geçti.
- 4 DWG familyası ve 2 ASCII DXF karşılığında total block entity `341`; manifestin LINE/CIRCLE/BLOCK_REFERENCE/DIMENSION/HATCH minimum semantiği geçti.
- Sentetik Türkçe/basic DXF exact count sözleşmesini geçti.
- Missing-font ve missing-XREF negatifleri görünür compatibility kodu üretti.
- Truncated AC1015 DWG kontrollü `EndOfStreamException`; corrupt AC1018 DWG controlled warning üretti.
- ASCII DXF notification/`unsupported-object` kayıtları known limitation olarak tutuldu; fixed warning-count eşiği kullanılmadı.
- `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`: read-only parser baseline `GO`; render fidelity onayı değildir.
- Final PR head `80cdaf49d3ad4298f3b1d56fe1dbac89b352ec7f` üzerinde `Stage 05 Parser Spike` run `32760139261` / #15 `SUCCESS`; artifact `9532379884`, digest `sha256:f3b31c937186d874a0ed23c045951d465ace5da8fff2f9acc32006c4352e2f60`.
- Aynı final PR head Stage 04 run #18, Stage 02 run #32 ve Stage 01 run #51 `SUCCESS`.
- PR #7 doğrulanmış final head üzerinden `main`e merge edildi. Merge commit: `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.

Ayrıntı: `docs/evidence/STAGE_05.md` ve `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`.

## Değiştirilemez temel teknik kararlar

- v1 yalnız 2D viewer; edit/write yok.
- DWG/DXF cihazda/offline okunur; zorunlu cloud conversion yok.
- Autodesk RealDWG, APS/Forge dönüşümü, ticari ODA SDK, trial/ücretli CAD parser-renderer yok.
- Varsayılan runtime lisans allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD; exact dependency yine denetlenir.
- Runtime graph'ta GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/unknown release blocker'dır.
- ACadSharp `3.7.1` read-only parser baseline `GO`; bu karar render/engineering fidelity garantisi değildir.
- SkiaSharp renderer adayıdır; native dağıtım zinciri ayrıca denetlenir.
- ProCad yalnız source-pinned izole spike; otomatik production dependency değildir.
- UI parser entity'lerine doğrudan bağlanmaz.
- Unsupported/proxy/font/XREF/raster kaybı sessiz olmaz.
- Original drawing overwrite edilmez.

## Yeni ajan için protokol

1. Bu dosyayı ve execution override'ı oku.
2. Gerçek `main` durumunu doğrula.
3. Kullanıcı değişikliklerini koru; destructive Git işlemi yapma.
4. Kullanıcı yalnız `devam` diyorsa `NEXT_WORK_STAGE` üzerinden ilerle. Şu anda bu AŞAMA 06'dır.
5. AŞAMA 01 dış erişim kapılarını sahte PASS/DONE yapma.
6. Bir turda en fazla bir aşama tamamla.
7. `[LIVE-VERIFY]` noktalarında resmi/güncel kaynak kullan.
8. Her turun sonunda `gecmis.md`, ilgili stage evidence ve canonical checkpoint'i gerçek durumla güncelle.

## Bir sonraki tur

Kullanıcı `devam` dediğinde yalnız AŞAMA 06 — Android güvenli dosya alma ve parse spike — başlatılır. Aynı turda AŞAMA 07'ye geçilmez.
