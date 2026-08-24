#!/usr/bin/env python3
from pathlib import Path

PLAN = Path("Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md")
text = PLAN.read_text(encoding="utf-8")

checkpoint_old = """CURRENT_STAGE: AŞAMA 02
CURRENT_SUBSTEP: 02.6
STATUS: DONE
LAST_VERIFIED_REVISION: e970a7937ac7360420e7195f4e611a1d516972ea — AŞAMA 02 PR #4 merge ve kapanış evidence/handoff/README/EXECUTION_LOG güncellemeleri main üzerinde doğrulandı
LAST_SUCCESSFUL_COMMAND: GitHub Actions Stage 02 Dependency Audit run 32747785867 / #9 SUCCESS + aynı head Stage 01 Toolchain Smoke run 32747785948 / #29 SUCCESS
EVIDENCE: docs/evidence/STAGE_02.md; compliance/DEPENDENCY_EVIDENCE.md; docs/EXECUTION_LOG.md; compliance/Stage02.DependencyProbe/packages.lock.json; compliance/stage02-package-manifest.json; PR #4 merge f0a43db6cc3aee9103f42798fa124da4d1ff39d1; Stage 02 artifact 9527769476 / SHA-256 90d41760e306e13b9977586b9996c1aafdf27f615c2b730bb41d74507b4684f3
BLOCKERS: AŞAMA 02 için yok. AŞAMA 01 fiziksel Android install/launch ve iOS erişim envanteri docs/USER_APPROVED_EXECUTION_OVERRIDE.md gereği DEFERRED_EXTERNAL_GATE olarak açık kalır.
NEXT_ACTION: AŞAMA 03 — test corpus'u, golden sözleşmesi ve cihaz matrisi. AŞAMA 01 dış kapılarını sahte PASS/DONE yapma; aynı turda AŞAMA 04'e geçme.
LAST_UPDATE: 2026-08-24"""

checkpoint_new = """CURRENT_STAGE: AŞAMA 03
CURRENT_SUBSTEP: 03.7
STATUS: DONE
LAST_VERIFIED_REVISION: fb2d0982efeab8f78bc78dc82a7a8deb688190f8 — AŞAMA 03 PR #5 doğrulanmış head üzerinden main'e merge edildi
LAST_SUCCESSFUL_COMMAND: GitHub Actions Stage 03 Corpus Audit run 32752374980 / #4 SUCCESS + aynı head Stage 02 Dependency Audit run 32752375058 / #15 SUCCESS + Stage 01 Toolchain Smoke run 32752374956 / #34 SUCCESS
EVIDENCE: docs/evidence/STAGE_03.md; fixtures/manifest/stage03-mini.json; fixtures/manifest/stage03-source-integrity.json; docs/GOLDEN_CONTRACT.md; docs/DEVICE_MATRIX.md; PR #5 merge fb2d0982efeab8f78bc78dc82a7a8deb688190f8; Stage 03 artifact 9529508675 / SHA-256 fd3990d7a3271c015a2f7067a856d5a23434f1ec0449ecff7819b569938e02cf
BLOCKERS: AŞAMA 03 için yok. AŞAMA 01 fiziksel Android install/launch ve iOS erişim envanteri docs/USER_APPROVED_EXECUTION_OVERRIDE.md gereği DEFERRED_EXTERNAL_GATE olarak açık kalır.
NEXT_ACTION: AŞAMA 04 — minimal solution ve mimari sınırlar. AŞAMA 01 dış kapılarını sahte PASS/DONE yapma; aynı turda AŞAMA 05'e geçme.
LAST_UPDATE: 2026-08-24"""

index_old = """- [ ] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi — `NEXT`
- [ ] AŞAMA 04 — Minimal solution ve mimari sınırlar"""
index_new = """- [x] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi — `DONE`
- [ ] AŞAMA 04 — Minimal solution ve mimari sınırlar — `NEXT`"""

stage_old = """- [ ] `fixtures/manifest` şeması: hash, format/version, boyut, hak/provenance, özellikler, beklenen counts/warnings.
- [ ] Public/synthetic ile private corpus ayrılır; private klasör Git ignored doğrulanır.
- [ ] İlk mini corpus en az 4 DWG (en az iki yaygın version family) + 2 DXF içerir.
- [ ] Set birlikte basic geometry, Turkish text, nested block, dimension, hatch ve mümkünse layout’u kapsar.
- [ ] Corrupt/truncated ve eksik font/XREF kontrollü negatif fixture eklenir.
- [ ] Golden görüntülerin redistribution durumu kaydedilir; yalnız izinli olanlar repoya girer.
- [ ] Android/iOS fiziksel cihaz matrisi ve provisional benchmark profilleri yazılır.

Test: Hash/provenance validator ve fixture erişim smoke.  
Çıkış: Mini corpus + beklenen sonuç manifest’i vardır. Gerekli özel DWG yoksa kullanıcıdan dosya istenir ve aşama eksik kalır."""

stage_new = """- [x] `fixtures/manifest` şeması oluşturuldu; hash, format/version, boyut, hak/provenance, özellikler, beklenen counts/warnings ve golden metadata alanları tanımlandı.
- [x] Public/synthetic/private corpus ayrımı kuruldu; private fixture yolu Git ignored ve validator tarafından enforced.
- [x] İlk mini corpus 4 DWG familyası (R2000/R2004/R2010/R2018) + en az 2 DXF içeriyor; upstream binary'ler immutable ACadSharp revision üzerinden remote-pinned tutuluyor, repoya vendored edilmiyor.
- [x] Set basic geometry, Turkish text, nested block, dimension, hatch ve paper-space/layout feature coverage içeriyor.
- [x] CI-derived truncated/corrupt DWG ile committed missing-font/missing-XREF negatif fixture'ları eklendi.
- [x] Golden görüntü redistribution sözleşmesi `docs/GOLDEN_CONTRACT.md` içinde tanımlandı; izin kanıtı olmadan image golden repoya giremez.
- [x] Android/iOS fiziksel cihaz matrisi ve provisional benchmark profilleri `docs/DEVICE_MATRIX.md` içinde yazıldı; gerçek cihaz slotları erişim yokluğu nedeniyle UNKNOWN/DEFERRED_EXTERNAL_GATE.

Test: GitHub Actions `Stage 03 Corpus Audit` run `32752374980` / #4 SUCCESS; `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`, `STAGE03_DUAL_HASH_PASS fixtures=6`, private-ignore/coverage/version/hash/provenance ve evidence artifact upload PASS. Aynı final head üzerinde Stage 02 run #15 ve Stage 01 run #34 SUCCESS.  
Çıkış: Sağlandı. Mini corpus + beklenen sonuç manifest’i, dual-hash source integrity kaydı, golden contract ve cihaz matrisi mevcut. PR #5 merge commit `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`. AŞAMA 01 dış cihaz kapıları ertelenmiş olarak açık kalır."""

for label, old in (("checkpoint", checkpoint_old), ("index", index_old), ("stage03", stage_old)):
    if text.count(old) != 1:
        raise SystemExit(f"guard failed: {label} expected exactly once, found {text.count(old)}")

text = text.replace(checkpoint_old, checkpoint_new, 1)
text = text.replace(index_old, index_new, 1)
text = text.replace(stage_old, stage_new, 1)

if "CURRENT_STAGE: AŞAMA 03" not in text or "AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi — `DONE`" not in text:
    raise SystemExit("postcondition failed")

PLAN.write_text(text, encoding="utf-8")
print("STAGE03_CANONICAL_CLOSEOUT_PASS")
