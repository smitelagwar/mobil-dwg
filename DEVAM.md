# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff

Bu dosya, yeni bir ChatGPT/AI sohbetinde projeye kaldığı yerden devam etmek için **tek giriş noktasıdır**. Kullanıcının önceki sohbeti veya model belleği mevcut kabul edilmez.

## Yeni AI için doğrudan talimat

1. Bu dosyayı tamamen oku.
2. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunu aç ve gerçek `main` durumunu doğrula.
3. Repo içindeki `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` canonical/yetkili plandır. Bu dosyadaki checkpoint ile canonical plan çelişirse, gerçek GitHub `main` üzerindeki en güncel kayıtları inceleyip çelişkiyi açıkça belirt; sessizce tahmin etme.
4. Kullanıcı yalnız `devam` diyorsa aşağıdaki `NEXT_WORK_STAGE` üzerinden ilerle.
5. Bir kullanıcı turunda en fazla **bir aşama** tamamla. Aynı turda sonraki aşamaya başlama.
6. Fiziksel cihaz/Mac/Apple hesabı gibi kullanıcının şu anda sağlayamadığı dış erişim kapılarını sahte PASS/DONE yapma. Bunlar `DEFERRED_EXTERNAL_GATE` olarak açık kalır; ancak bağımsız aşamaların ilerlemesini engellemez.
7. Her aşamanın sonunda canonical plan checkpoint’ini, `gecmis.md` ve ilgili `docs/evidence/STAGE_XX.md` kaydını gerçek test/CI/commit kanıtıyla güncelle.
8. Kullanıcı değişikliklerini koru; destructive Git işlemi yapma.

---

## Repo kimliği

- GitHub: `smitelagwar/mobil-dwg`
- Default branch: `main`
- Repo: private
- Ürün: Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer
- v1: viewer-only; edit/save/export/cloud/account kapsam dışında

---

## Güncel checkpoint

```text
LAST_COMPLETED_STAGE: AŞAMA 04
NEXT_WORK_STAGE: AŞAMA 05
NEXT_WORK_STATUS: NOT_STARTED
DEFERRED_STAGE: AŞAMA 01
DEFERRED_STAGE_STATUS: BLOCKED / DEFERRED_EXTERNAL_GATE — DONE değil
DEFERRED_EXTERNAL_GATES: fiziksel Android install/launch; STAGE01_DEVICE_GATE_PASS; gerçek Mac/Xcode/iPhone/Apple Developer erişim envanteri
USER_CONSTRAINT: Kullanıcı şu anda dış cihaz/Mac erişim kanıtlarını sağlayamıyor; temel etkileşimi "devam" demek.
USER_APPROVAL: Dış kapılar sahte PASS/DONE yapılmadan ertelensin; bağımsız aşamalara devam edilsin.
NEXT_ACTION: AŞAMA 05 — pinned ACadSharp ile headless parser spike. Aynı turda AŞAMA 06'ya geçme.
LAST_UPDATE: 2026-08-24
```

AŞAMA 04 canonical checkpoint'i `DONE` durumundadır. AŞAMA 04 PR #6 merge commit:

`c01311ccb5c82b7bac023b24ae6a8000ae4655af`

Stage 04 final CI:

- Workflow: `Stage 04 Architecture`
- Run: `32755230695` / #2
- Sonuç: `SUCCESS`
- clean solution restore: PASS
- Release build: `0 Warning(s)`, `0 Error(s)`
- `STAGE04_CORE_CONTRACT_TESTS_PASS`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE04_T0_PASS`

Aynı final head üzerinde:

- Stage 02 Dependency Audit run `32755230688` / #17: `SUCCESS`
- Stage 01 Toolchain Smoke run `32755230683` / #36: `SUCCESS`

Stage 01 CI fiziksel telefon install/launch kanıtı değildir.

---

## Tamamlanan aşamalar

- [x] AŞAMA 00 — çalışma alanı ve yürütme zemini — `DONE`
- [ ] AŞAMA 01 — toolchain + gerçek telefon — `BLOCKED / DEFERRED_EXTERNAL_GATE`; bağımsız CI/toolchain kısmı tamamlandı
- [x] AŞAMA 02 — dependency/lisans kanıtı ve kilitler — `DONE`
- [x] AŞAMA 03 — test corpus’u, golden sözleşmesi ve cihaz matrisi — `DONE`
- [x] AŞAMA 04 — minimal solution ve mimari sınırlar — `DONE`
- [ ] AŞAMA 05 — ACadSharp headless parser spike — `NEXT`

AŞAMA 06 ve sonrası henüz başlatılmadı.

---

## AŞAMA 04 sonunda mevcut mimari

Production projeleri — tam olarak dört:

- `src/MobilDwg.Core`
- `src/MobilDwg.Cad`
- `src/MobilDwg.Rendering`
- `src/MobilDwg.App`

Test projeleri — tam olarak üç:

- `tests/MobilDwg.Core.Tests`
- `tests/MobilDwg.Rendering.Tests`
- `tests/MobilDwg.Architecture.Tests`

Solution: `MobilDwg.sln`.

Dependency yönü:

```text
MobilDwg.Core
   ↑       ↑
   |       |
MobilDwg.Cad   MobilDwg.Rendering
       \       /
        \     /
       MobilDwg.App
```

Kurallar:

- `MobilDwg.Core`: ProjectReference yok, PackageReference yok; BCL-only.
- `MobilDwg.Cad`: yalnız Core’a referans verir.
- `MobilDwg.Rendering`: yalnız Core’a referans verir.
- `MobilDwg.App`: Core/Cad/Rendering composition boundary.
- UI parser entity’lerine doğrudan bağlanmaz.
- AŞAMA 04’te ACadSharp/SkiaSharp/MAUI production graph’a eklenmedi.

Core kontratları mevcut:

- `ICadDocumentReader`
- `CadDocumentSession`
- `ICadDocumentHandle`
- `CadDocumentMetadata`
- diagnostics/compatibility kayıtları
- `IRenderSceneBuilder`
- `ICadRenderer`
- `IRenderScene`
- `IRenderSurface`
- `RenderViewport`

Cancellation/progress capability modeli:

- Cancellation: `None`, `BeforeStartOnly`, `Cooperative`
- Progress: `None`, `StagesOnly`, `Fractional`
- Gerçek fraction bilinmiyorsa `null`; sahte yüzde üretilemez.

Ayrıntı için repo içinde:

- `docs/ARCHITECTURE.md`
- `docs/evidence/STAGE_04.md`

---

## AŞAMA 02 dependency kararları

Production parser adayı:

- ACadSharp `3.7.1`
- Lisans/dependency açısından `GREEN`
- Fidelity/production onayı **henüz yok**; AŞAMA 05 corpus gate sonucuna bağlı

Renderer adayı:

- SkiaSharp `4.151.1`
- Native artifact zinciri nedeniyle `REVIEW`

ProCad:

- production default `NO-GO`
- yalnız AŞAMA 07 source-pinned spike
- source revision: `f8a862b3e7634e27664fee02ff5d68774b102985`

IxMilia.Dxf `0.8.4`:

- yalnız test/fallback adayı
- AŞAMA 05’te ACadSharp kritik DXF kaybı gösterirse koşullu spike düşünülebilir
- otomatik production dependency değildir

Package sürümleri `Directory.Packages.props` ile merkezi olarak pinlidir. Stage 02 compliance kayıtları:

- `docs/evidence/STAGE_02.md`
- `compliance/DEPENDENCY_EVIDENCE.md`

---

## AŞAMA 03 corpus/golden temeli

Mini corpus ve doğrulama altyapısı hazırdır.

Remote-pinned DWG familyaları:

- R2000 / AC1015
- R2004 / AC1018
- R2010 / AC1024
- R2018 / AC1032

Remote-pinned ASCII DXF:

- R2000
- R2018

Mobil-dwg sentetik fixture’ları:

- Türkçe/basic/nested-block pozitif DXF
- missing-font negatif DXF
- missing-XREF negatif DXF
- CI-derived deterministic truncated DWG
- CI-derived deterministic corrupt DWG

Upstream fixture source:

- `DomCR/ACadSharp`
- immutable revision `592d70a7bf0eaffbd932d23900f289b4e6305832`
- upstream binary fixture’lar mobil-dwg reposuna vendored edilmez; immutable source + dual hash ile doğrulanır

Önemli dosyalar:

- `fixtures/manifest/stage03-mini.json`
- `fixtures/manifest/stage03-source-integrity.json`
- `fixtures/manifest/schema.json`
- `docs/GOLDEN_CONTRACT.md`
- `docs/DEVICE_MATRIX.md`
- `docs/evidence/STAGE_03.md`

---

# ŞİMDİ YAPILACAK: AŞAMA 05 — ACadSharp headless parser spike

## Amaç

Pinned ACadSharp ile gerçek DWG/DXF okuma ve diagnostics’i UI’dan tamamen bağımsız biçimde kanıtlamak.

## Yapılacak işler

- [ ] Exact ACadSharp `3.7.1` yalnız `MobilDwg.Cad` adapter projesine eklenir. Writer/save API’leri kullanılmaz.
- [ ] Format magic/version preflight uygulanır.
- [ ] Reader notifications, exceptions ve parse timing Core diagnostics/compatibility modeline bağlanır.
- [ ] `ICadDocumentReader` için ACadSharp adapter oluşturulur; parser-specific entity/type UI/Core boundary’sine sızdırılmaz.
- [ ] Adapter gerçek cancellation/progress desteğini abartmaz. Parser cooperative abort sağlamıyorsa `Cooperative` ilan edilmez.
- [ ] Stage 03 mini corpus headless açılır.
- [ ] Layer/block/layout/entity type dağılımları golden/manifest beklentileriyle karşılaştırılır.
- [ ] Unsupported/proxy ve reader notification’ları severity/classification ile raporlanır; sabit “uyarı sayısı <= N” gibi anlamsız bir eşik kullanılmaz.
- [ ] Aynı parsed document’tan türetilen iki farklı count’ın tek başına veri kaybı olmadığını kanıtlamadığı kabul edilir; golden beklentiyle kıyas gerekir.
- [ ] Corrupt/truncated/missing-font/missing-XREF gibi negatifler deterministic sonuç verir; exception/diagnostic sessizce yutulmaz.
- [ ] Approved/rejected parser sürüm kararı için ADR yazılır.
- [ ] Known-failure listesi oluşturulur; gerçek corpus başarısızlıkları gizlenmez.
- [ ] Headless regression CI workflow’u eklenir ve T3 corpus gate çalıştırılır.

## AŞAMA 05 test/çıkış kriteri

Test:

`T3 mini corpus headless regression`

AŞAMA 05 yalnız şu durumda `DONE` olabilir:

- DWG ve DXF headless parse yolu gerçek corpus ile kanıtlı,
- diagnostics/compatibility/exception/timing kaydı gerçek,
- corpus golden karşılaştırması gerçek CI’da PASS veya açıkça kabul edilmiş known-failure kararıyla belgeli,
- ACadSharp `3.7.1` için GO/NO-GO/conditional karar ADR’de kayıtlı,
- kritik corpus kaybı varsa önce ACadSharp sürüm karşılaştırması yapılmış; IxMilia yalnız DXF için koşullu spike olarak değerlendirilmiş,
- clean restore/build/test ve Stage 05 CI kanıtı mevcut.

Bu çıkış kriterleri sağlanmadan AŞAMA 05 `DONE` yazılmaz ve AŞAMA 06 başlatılmaz.

---

## Değiştirilemez proje ilkeleri

- v1 yalnız 2D viewer; editor/writer/save/export yok.
- DWG/DXF cihazda ve offline okunur; zorunlu cloud conversion yok.
- Autodesk RealDWG, APS/Forge conversion, ticari ODA SDK, trial/ücretli CAD SDK yok.
- Runtime lisans allowlist varsayılanı: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD.
- GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/unknown runtime dependency release blocker’dır.
- Original CAD dosyası immutable; overwrite edilmez.
- Unsupported/proxy/font/XREF/raster problemleri sessiz kayıp olarak gizlenmez.
- UI parser entity’lerine doğrudan bağlanmaz.
- Optimizasyon yalnız ölçülmüş bottleneck üzerinde yapılır.
- Production parser onayı yalnız gerçek corpus kanıtından sonra verilir.

---

## Toolchain baseline

- .NET SDK: `10.0.400`
- workload set: `10.0.400`
- Microsoft OpenJDK: `21.0.12`
- Android min API: `24`
- target/compile API: `36`
- Android Build-Tools: `36.0.0`
- Platform-Tools / adb: `37.0.1`
- `maui-android` workload

Bu değerlerin değiştirilmesi gerekiyorsa planın `[LIVE-VERIFY]` kuralına göre güncel resmi kaynak ve CI kanıtı gerekir.

---

## Yeni sohbet için önerilen tek mesaj

Bu dosyayı yeni sohbete ekleyip şu mesaj yeterlidir:

> `@GitHub içindeki smitelagwar/mobil-dwg reposunda DEVAM.md dosyasını tek handoff kaynağı olarak oku, gerçek main durumunu doğrula ve kaldığımız yerden devam et.`

Bundan sonra kullanıcı yalnız `devam` diyebilir.
