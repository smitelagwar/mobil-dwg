#!/usr/bin/env python3
from pathlib import Path

PLAN = Path("Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md")
text = PLAN.read_text(encoding="utf-8")

replacements = [
    (
        """CURRENT_STAGE: AŞAMA 03
CURRENT_SUBSTEP: 03.7
STATUS: DONE
LAST_VERIFIED_REVISION: fb2d0982efeab8f78bc78dc82a7a8deb688190f8 — AŞAMA 03 PR #5 doğrulanmış head üzerinden main'e merge edildi
LAST_SUCCESSFUL_COMMAND: GitHub Actions Stage 03 Corpus Audit run 32752374980 / #4 SUCCESS + aynı head Stage 02 Dependency Audit run 32752375058 / #15 SUCCESS + Stage 01 Toolchain Smoke run 32752374956 / #34 SUCCESS
EVIDENCE: docs/evidence/STAGE_03.md; fixtures/manifest/stage03-mini.json; fixtures/manifest/stage03-source-integrity.json; docs/GOLDEN_CONTRACT.md; docs/DEVICE_MATRIX.md; PR #5 merge fb2d0982efeab8f78bc78dc82a7a8deb688190f8; Stage 03 artifact 9529508675 / SHA-256 fd3990d7a3271c015a2f7067a856d5a23434f1ec0449ecff7819b569938e02cf
BLOCKERS: AŞAMA 03 için yok. AŞAMA 01 fiziksel Android install/launch ve iOS erişim envanteri docs/USER_APPROVED_EXECUTION_OVERRIDE.md gereği DEFERRED_EXTERNAL_GATE olarak açık kalır.
NEXT_ACTION: AŞAMA 04 — minimal solution ve mimari sınırlar. AŞAMA 01 dış kapılarını sahte PASS/DONE yapma; aynı turda AŞAMA 05'e geçme.
LAST_UPDATE: 2026-08-24""",
        """CURRENT_STAGE: AŞAMA 04
CURRENT_SUBSTEP: 04.5
STATUS: DONE
LAST_VERIFIED_REVISION: c01311ccb5c82b7bac023b24ae6a8000ae4655af — AŞAMA 04 PR #6 doğrulanmış head üzerinden main'e merge edildi
LAST_SUCCESSFUL_COMMAND: GitHub Actions Stage 04 Architecture run 32755230695 / #2 SUCCESS + aynı head Stage 02 Dependency Audit run 32755230688 / #17 SUCCESS + Stage 01 Toolchain Smoke run 32755230683 / #36 SUCCESS
EVIDENCE: docs/evidence/STAGE_04.md; docs/ARCHITECTURE.md; MobilDwg.sln; PR #6 merge c01311ccb5c82b7bac023b24ae6a8000ae4655af; STAGE04_CORE_CONTRACT_TESTS_PASS; STAGE04_RENDER_CONTRACT_TESTS_PASS; STAGE04_ARCHITECTURE_TESTS_PASS; STAGE04_T0_PASS
BLOCKERS: AŞAMA 04 için yok. AŞAMA 01 fiziksel Android install/launch ve iOS erişim envanteri docs/USER_APPROVED_EXECUTION_OVERRIDE.md gereği DEFERRED_EXTERNAL_GATE olarak açık kalır.
NEXT_ACTION: AŞAMA 05 — pinned ACadSharp ile headless parser spike. AŞAMA 01 dış kapılarını sahte PASS/DONE yapma; aynı turda AŞAMA 06'ya geçme.
LAST_UPDATE: 2026-08-24""",
    ),
    (
        "- [ ] AŞAMA 04 — Minimal solution ve mimari sınırlar — `NEXT`",
        "- [x] AŞAMA 04 — Minimal solution ve mimari sınırlar — `DONE`",
    ),
    (
        "- [ ] AŞAMA 05 — ACadSharp headless parser spike",
        "- [ ] AŞAMA 05 — ACadSharp headless parser spike — `NEXT`",
    ),
    (
        """### AŞAMA 04 — Minimal solution ve mimari sınırlar

**Amaç:** Parser ve renderer’ı UI’dan ayıran küçük, derlenebilir iskelet.

İşler:

- [ ] Dört production projesi ve üç test projesi oluşturulur; v1 dışı proje açılmaz.
- [ ] `ICadDocumentReader`, session owner, `IRenderSceneBuilder`, `ICadRenderer`, diagnostics ve compatibility kontratları tanımlanır.
- [ ] Core katmanı MAUI/Skia/ACadSharp’a referans vermez.
- [ ] Cancellation/progress API’si gerçek destek düzeyini yanlış temsil etmeyecek şekilde modellenir.
- [ ] Architecture dependency tests eklenir.

Test: T0 + kontrat unit testleri.  
Çıkış: Solution temiz restore/build/test geçer; dependency yönleri otomatik test edilir.""",
        """### AŞAMA 04 — Minimal solution ve mimari sınırlar

**Amaç:** Parser ve renderer’ı UI’dan ayıran küçük, derlenebilir iskelet.

İşler:

- [x] Dört production projesi (`MobilDwg.Core`, `MobilDwg.Cad`, `MobilDwg.Rendering`, `MobilDwg.App`) ve üç test projesi oluşturuldu; v1 dışı production/test proje açılmadı.
- [x] `ICadDocumentReader`, session owner (`CadDocumentSession` + `ICadDocumentHandle`), `IRenderSceneBuilder`, `ICadRenderer`, diagnostics ve compatibility kontratları tanımlandı.
- [x] Core katmanı BCL-only tutuldu; MAUI/SkiaSharp/ACadSharp referansı ve ProjectReference/PackageReference yok.
- [x] Cancellation/progress API’si `None/BeforeStartOnly/Cooperative` ve `None/StagesOnly/Fractional` capability seviyeleriyle gerçek destek düzeyini yanlış temsil etmeyecek şekilde modellendi; bilinmeyen fraction `null` kalır.
- [x] Architecture dependency tests eklendi; tam proje sayısı, exact ProjectReference yönleri, Stage 04 production PackageReference yokluğu ve forbidden Core/App dependency terimleri otomatik denetlenir.

Test: GitHub Actions `Stage 04 Architecture` run `32755230695` / #2 SUCCESS; exact .NET/workload set, clean solution restore, Release build `0 Warning(s) / 0 Error(s)`, `STAGE04_CORE_CONTRACT_TESTS_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE04_T0_PASS`. Aynı final head Stage 02 run #17 ve Stage 01 run #36 SUCCESS.  
Çıkış: Sağlandı. Solution temiz restore/build/test geçer; dependency yönleri otomatik test edilir. PR #6 merge commit `c01311ccb5c82b7bac023b24ae6a8000ae4655af`.""",
    ),
]

for old, new in replacements:
    count = text.count(old)
    if count != 1:
        raise SystemExit(f"guard failed: expected exactly one marker, got {count}: {old[:80]!r}")
    text = text.replace(old, new)

PLAN.write_text(text, encoding="utf-8")
print("STAGE04_CANONICAL_CLOSEOUT_READY")
