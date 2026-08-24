from __future__ import annotations

from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
PLAN = ROOT / "Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md"
HISTORY = ROOT / "gecmis.md"
README = ROOT / "README.md"
LOG = ROOT / "docs/EXECUTION_LOG.md"
DEVAM = ROOT / "DEVAM.md"

DECISION_HEAD = "3f88bec383de895e309e218c08d13e9784562a97"
STAGE06_MERGE = "e3a9c36e04be6c51827926ca17bb1a386c6b1142"
RUN07 = "32766501837"
ARTIFACT07 = "9534797361"
DIGEST07 = "sha256:9cae376fd0cbf2861f006af347483f9de26a6cd49f30b201438a3afdb591e555"


def replace_once(text: str, pattern: str, replacement: str, *, flags: int = 0, label: str) -> str:
    out, count = re.subn(pattern, replacement, text, count=1, flags=flags)
    if count != 1:
        raise RuntimeError(f"{label}: expected exactly one replacement, got {count}")
    return out


# Canonical plan
plan = PLAN.read_text(encoding="utf-8")
checkpoint = f'''```text
CURRENT_STAGE: AŞAMA 07
CURRENT_SUBSTEP: 07.8
STATUS: DONE
LAST_VERIFIED_REVISION: {DECISION_HEAD} — exact pinned ProCad source candidate için source/NuGet lineage, Android build ve deterministic precision gate tamamlandı; karar NO-GO
LAST_SUCCESSFUL_COMMAND: GitHub Actions Stage 07 ProCad Source Spike run {RUN07} / #5 SUCCESS + aynı head Stage 06 Safe Open run 32766501815 / #13 SUCCESS + Stage 02 Dependency Audit run 32766501809 / #44 SUCCESS + Stage 01 Toolchain Smoke run 32766501846 / #63 SUCCESS
EVIDENCE: docs/evidence/STAGE_07.md; docs/ADR/0002-procad-pinned-source-no-go.md; Stage 07 artifact {ARTIFACT07} {DIGEST07}; STAGE07_SOURCE_PIN_PASS; STAGE07_ACAD_LINEAGE_PASS approved_ahead=592; STAGE07_NUGET_011_RESTORE_EXIT=0; STAGE07_FLOAT_PRECISION_BLOCKER_REPRODUCED; STAGE07_SOURCE_BUILD_EXIT=0; STAGE07_MAUI_SMOKE_BUILD_EXIT=0; STAGE07_DECISION_NO_GO_PASS
BLOCKERS: Exact unpatched ProCad candidate survey-origin 1 mm detayı direct double-to-float RenderScene boundary'sinde kaybettiği için production renderer/control reuse NO-GO. Physical Android T3 bu deterministic blocker sonrasında NOT_RUN_AFTER_DETERMINISTIC_BLOCKER ve PASS değildir. AŞAMA 01 ve AŞAMA 06 gerçek cihaz kapıları DEFERRED_EXTERNAL_GATE olarak açık. AŞAMA 09 custom renderer implementation öncesinde ADR 0002 HIGH efor/bakım riski için kullanıcı GO gerekir.
NEXT_ACTION: AŞAMA 08 — erken iOS AOT/native fizibilite smoke. AŞAMA 07 kapanış turunda AŞAMA 08 başlatılmaz.
LAST_UPDATE: 2026-08-24
```'''
plan = replace_once(
    plan,
    r"```text\nCURRENT_STAGE:.*?\n```",
    checkpoint,
    flags=re.S,
    label="plan checkpoint",
)
plan = plan.replace(
    "- [ ] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO — `NEXT`",
    "- [x] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO — `DONE / NO-GO`",
)
plan = plan.replace(
    "- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke",
    "- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke — `NEXT`",
    1,
)
stage07 = '''### AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO

**Amaç:** Hazır renderer/control reuse’unu üretim kodunu bağlamadan ölçmek.

İşler:

- [x] Spike yalnız `spikes/ProCad.Android` içinde exact commit/submodule SHA ile kuruldu; ProCad `f8a862b3e7634e27664fee02ff5d68774b102985`, ACadSharp submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36`, ProEdit `64759b79289a024d08463ed1a9094fdcd9a270df`.
- [x] NuGet 0.1.1 restore graph’ı ile source graph farkı belgelendi. Published `ProCadSharp.Rendering 0.1.1` ACadSharp `>=0.1.1` istediği halde 0.1.1 bulunmadığından `ACadSharp 1.0.0` çözüyor; published MAUI graph Skia `4.147.0-preview.2.1` bandına çıkıyor.
- [x] ProCad fork ACadSharp lineage official upstream’de aynı SHA olarak çözüldü; unresolved değildir. Mobil-dwg approved ACadSharp 3.7.1 source baseline pinned fork’tan `592` official commit ileridedir.
- [x] Pinned ProCad source temporary checkout yalnız `net10.0-android` hedefiyle izole edilerek gerçek Android source build yapıldı: `82 Warning(s)`, `0 Error(s)`. Clean MAUI Release smoke `0 Warning(s)`, `0 Error(s)` ve signed APK üretti; GPU zorunluluğu karar nedeni yapılmadı.
- [ ] Gerçek fiziksel Android Release T3 A/B’de mini corpus first-frame/pan/pinch/Turkish text/nested block/dimension/hatch/layout/close-reopen ölçülmedi: `NOT_RUN_AFTER_DETERMINISTIC_BLOCKER`. Bu PASS değildir; exact candidate gerçek cihazdan önce hard precision blocker ile reddedildi.
- [x] ProCad scene direct `double -> float` koordinat hattı deterministic fixture ile ölçüldü. Origin `100.0` + `0.001` detay korunurken origin `5,000,000.0` + `0.001` float sınırında aynı değere çöktü; observed delta `0.0`, relative error `1.0`. `STAGE07_FLOAT_PRECISION_BLOCKER_REPRODUCED`.
- [x] Runtime/source riskleri kaydedildi: ProCad/ACadSharp/ProEdit pinned license marker MIT; source Skia `3.119.4` ile MAUI view `4.147.0-preview.2.1` mixed/preview bandı; published graph ACadSharp `1.0.0`; MAUI CadViewer one-pointer pan içeriyor fakat pinch implementation bulunmadı.
- [x] `docs/ADR/0002-procad-pinned-source-no-go.md` kararı exact unpatched candidate için `NO-GO`; `docs/evidence/STAGE_07.md` final kanıtı içerir. ProCad production graph’a eklenmedi.

Blocker FAIL: **gerçekleşti** — survey-origin millimetre detay direct double-to-float RenderScene boundary'sinde sistematik P0 fidelity kaybına uğruyor. Build başarısızlığı değildir; pinned Android source build ve clean MAUI Release smoke başarılıdır.  
Test: Final CI `Stage 07 ProCad Source Spike` run `32766501837` / #5 `SUCCESS`; artifact `9534797361`, digest `sha256:9cae376fd0cbf2861f006af347483f9de26a6cd49f30b201438a3afdb591e555`. Aynı decision head Stage 06 run #13, Stage 02 run #44 ve Stage 01 run #63 `SUCCESS`. Physical Android T3 `NOT_RUN_AFTER_DETERMINISTIC_BLOCKER` ve PASS değildir.  
Çıkış: **Sağlandı — NO-GO.** Exact pinned ProCad candidate production reuse için reddedildi. Özel renderer garantili fallback sayılmaz; ADR 0002 P0 kapsamını AŞAMA 09–16 sekiz implementation/fidelity aşaması + sonraki performance/full-corpus gate'leri olarak yeniden maliyetlendirir, efor/bakım riskini `HIGH` kaydeder ve precision-safe upstream patch/rebase yolunu tanımlar. AŞAMA 09 custom renderer implementation öncesinde kullanıcı GO kararı gerekir. AŞAMA 01 ve AŞAMA 06 dış cihaz kapıları açık kalır.

'''
plan = replace_once(
    plan,
    r"### AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO\n.*?(?=### AŞAMA 08 —)",
    stage07,
    flags=re.S,
    label="plan stage07 section",
)
PLAN.write_text(plan, encoding="utf-8")

# History / handoff
history = HISTORY.read_text(encoding="utf-8")
read_order_old = "4. `docs/evidence/STAGE_06.md`\n5. `docs/evidence/STAGE_05.md`\n6. `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`"
read_order_new = "4. `docs/evidence/STAGE_07.md` ve `docs/ADR/0002-procad-pinned-source-no-go.md`\n5. `docs/evidence/STAGE_06.md`\n6. `docs/evidence/STAGE_05.md` ve `docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md`"
if read_order_old not in history:
    raise RuntimeError("gecmis read-order pattern missing")
history = history.replace(read_order_old, read_order_new, 1)
history_checkpoint = f'''```text
LAST_COMPLETED_STAGE: AŞAMA 07
DEFERRED_STAGES: AŞAMA 01; AŞAMA 06
STAGE01_STATUS: BLOCKED / DEFERRED_EXTERNAL_GATE — fiziksel Android install/launch ve iOS erişim envanteri açık, DONE değil
STAGE06_STATUS: BLOCKED / DEFERRED_EXTERNAL_GATE — safe-open implementation/CI PASS, fiziksel Android FilePicker/SAF+lifecycle/cache gate açık, DONE değil
STAGE07_STATUS: DONE / NO-GO — exact unpatched ProCad source candidate deterministic precision blocker nedeniyle production reuse için reddedildi
NEXT_WORK_STAGE: AŞAMA 08
NEXT_WORK_STATUS: NOT_STARTED
USER_CONSTRAINT: Kullanıcı şu an fiziksel Android cihaz/gerçek geliştirme makinesi ve Mac/Xcode/iPhone/Apple Developer erişim kanıtlarını sağlayamıyor; temel etkileşimi "devam" demek.
USER_APPROVAL: Dış erişim kapıları sahte PASS/DONE yapılmadan ertelensin; bağımsız aşamalara devam edilsin.
DEFERRED_EXTERNAL_GATES: STAGE01_DEVICE_GATE_PASS; local Android install/launch; iOS erişim envanteri YES/NO/N/A; STAGE06_PHYSICAL_ANDROID_FILEPICKER_DWG_DXF; cancel/rotate/background/close/cache-cleanup
STAGE06_MERGE: PR #8 -> main; merge commit {STAGE06_MERGE}; bu merge fiziksel Stage 06 gate'ini DONE yapmaz
STAGE07_DECISION_HEAD: {DECISION_HEAD}
STAGE07_PR: #9 — stage07: evaluate pinned ProCad Android reuse
STAGE07_DECISION: NO-GO
STAGE07_HARD_BLOCKER: survey-origin 5,000,000 + 1 mm detail direct double-to-float RenderScene boundary'sinde observed delta 0.0
LAST_VERIFIED_STAGE07_CI: Stage 07 ProCad Source Spike run {RUN07} / #5 SUCCESS; source Android build exit=0; clean MAUI Release smoke exit=0; STAGE07_FLOAT_PRECISION_BLOCKER_REPRODUCED; STAGE07_DECISION_NO_GO_PASS; artifact {ARTIFACT07}; {DIGEST07}
LAST_VERIFIED_STAGE06_REGRESSION_CI: Stage 06 Safe Open run 32766501815 / #13 SUCCESS
LAST_VERIFIED_STAGE02_REGRESSION_CI: Stage 02 Dependency Audit run 32766501809 / #44 SUCCESS
LAST_VERIFIED_STAGE01_REGRESSION_CI: Stage 01 Toolchain Smoke run 32766501846 / #63 SUCCESS; physical device evidence değildir.
STAGE09_GO_BARRIER: ADR 0002 custom renderer efor/bakım riskini HIGH olarak kaydeder; AŞAMA 09 implementation öncesinde kullanıcı GO kararı gerekir.
EXECUTION_OVERRIDE: docs/USER_APPROVED_EXECUTION_OVERRIDE.md
NEXT_ACTION: AŞAMA 08 — erken iOS AOT/native fizibilite smoke. AŞAMA 07 kapanış turunda AŞAMA 08 başlatılmaz.
LAST_UPDATE: 2026-08-24
```'''
history = replace_once(
    history,
    r"```text\nLAST_COMPLETED_STAGE:.*?\n```",
    history_checkpoint,
    flags=re.S,
    label="gecmis checkpoint",
)
history = history.replace(
    "- [ ] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO — `NEXT`",
    "- [x] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO — `DONE / NO-GO`",
    1,
)
history = history.replace(
    "- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke",
    "- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke — `NEXT`",
    1,
)
if "### AŞAMA 07 — DONE / NO-GO" not in history:
    history += f'''\n\n### AŞAMA 07 — DONE / NO-GO\n\nExact ProCad source candidate `f8a862b3e7634e27664fee02ff5d68774b102985` production graph'a eklenmeden izole Android spike ile değerlendirildi. ACadSharp submodule lineage official upstream'de çözüldü fakat mobil-dwg approved 3.7.1 source baseline'dan 592 commit geride. Published ProCadSharp 0.1.1 restore graph'ı `NU1603` ile ACadSharp 1.0.0 çözüyor ve Skia 4.147.0-preview.2.1 bandına çıkıyor. Pinned Android source build `82 warning / 0 error`, clean MAUI Release smoke `0 warning / 0 error` ile geçti. Buna rağmen origin 5,000,000 üzerindeki 1 mm detail direct double-to-float RenderScene boundary'sinde observed delta `0.0` olarak çöktü; deterministic P0 fidelity blocker. Physical Android T3 bu hard blocker sonrasında `NOT_RUN_AFTER_DETERMINISTIC_BLOCKER` ve PASS değildir. ADR `docs/ADR/0002-procad-pinned-source-no-go.md`; evidence `docs/evidence/STAGE_07.md`; final decision CI run `{RUN07}` / #5 SUCCESS, artifact `{ARTIFACT07}`, digest `{DIGEST07}`. Özel renderer garantili fallback değildir; AŞAMA 09 implementation öncesinde HIGH efor/bakım riski için kullanıcı GO gerekir.\n'''
HISTORY.write_text(history, encoding="utf-8")

# README
readme = README.read_text(encoding="utf-8")
readme = replace_once(
    readme,
    r"Proje yürütme aşamasındadır\..*?Sonraki bağımsız çalışma aşaması AŞAMA 07'dir\.",
    "Proje yürütme aşamasındadır. AŞAMA 00, AŞAMA 02, AŞAMA 03, AŞAMA 04, AŞAMA 05 ve AŞAMA 07 tamamlandı. AŞAMA 01'in toolchain/CI kısmı tamamlandı fakat gerçek Android install/launch ve iOS erişim envanteri `BLOCKED / DEFERRED_EXTERNAL_GATE` olarak açık. AŞAMA 06'nın safe-open/MAUI Android CI kısmı geçti fakat gerçek telefon FilePicker/SAF+lifecycle/cache gate'i `BLOCKED / DEFERRED_EXTERNAL_GATE`. AŞAMA 07'de exact pinned ProCad candidate Android source build ve clean MAUI Release smoke ile derlendi, ancak survey-origin 1 mm detay direct `double→float` RenderScene sınırında deterministik olarak çöktüğü için production reuse kararı `NO-GO`. ProCad production graph'a eklenmedi. ACadSharp `3.7.1` read-only parser baseline AŞAMA 05'te `GO` kalır. Sonraki bağımsız çalışma aşaması AŞAMA 08'dir; AŞAMA 09 custom renderer implementation öncesinde ADR 0002'deki HIGH efor/bakım riski için kullanıcı GO gerekir.",
    flags=re.S,
    label="README status",
)
readme = readme.replace(
    "4. [docs/evidence/STAGE_06.md](docs/evidence/STAGE_06.md) — AŞAMA 06'nın geçen CI/safe-open kısmı ve açık fiziksel Android dış kapısı.\n5. [docs/evidence/STAGE_05.md](docs/evidence/STAGE_05.md)",
    "4. [docs/evidence/STAGE_07.md](docs/evidence/STAGE_07.md) ve [docs/ADR/0002-procad-pinned-source-no-go.md](docs/ADR/0002-procad-pinned-source-no-go.md) — ProCad NO-GO precision/source/NuGet kanıtı.\n5. [docs/evidence/STAGE_06.md](docs/evidence/STAGE_06.md) — geçen CI/safe-open kısmı ve açık fiziksel Android dış kapısı.\n6. [docs/evidence/STAGE_05.md](docs/evidence/STAGE_05.md)",
    1,
)
README.write_text(readme, encoding="utf-8")

# Execution log append
log = LOG.read_text(encoding="utf-8")
if "## 2026-08-24 — AŞAMA 07 closure" not in log:
    log += f'''\n\n## 2026-08-24 — AŞAMA 07 closure\n\n- PR #8 AŞAMA 06 merge commit doğrulandı: `{STAGE06_MERGE}`. AŞAMA 06 physical Android gate bu merge ile kapanmadı.\n- AŞAMA 07 branch/PR #9 exact ProCad `f8a862b3e7634e27664fee02ff5d68774b102985` source candidate'ını production graph'a eklemeden değerlendirdi.\n- Final decision head `{DECISION_HEAD}`.\n- Final `Stage 07 ProCad Source Spike` run `{RUN07}` / #5 `SUCCESS`; artifact `{ARTIFACT07}`; digest `{DIGEST07}`.\n- Source Android build `82 Warning(s) / 0 Error(s)`; clean MAUI Release smoke `0 Warning(s) / 0 Error(s)`. Build başarısızlığı karar nedeni değildir.\n- ACadSharp source lineage official upstream'de çözüldü; mobil-dwg approved baseline 592 commit ileride. Published ProCadSharp 0.1.1 graph ACadSharp 1.0.0 ve Skia 4.147.0-preview.2.1 çözüyor.\n- Deterministic precision gate: origin 5,000,000 + 0.001 detail direct double-to-float scene boundary'sinde observed delta 0.0; systematic P0 fidelity blocker.\n- ADR 0002 exact unpatched candidate için `NO-GO`. Physical Android T3 `NOT_RUN_AFTER_DETERMINISTIC_BLOCKER`, PASS değildir.\n- ProCad production dependency graph'a eklenmedi. AŞAMA 01 ve 06 dış cihaz gate'leri açık.\n- Sonraki bağımsız aşama AŞAMA 08. AŞAMA 09 custom renderer implementation öncesinde kullanıcı GO kararı zorunlu.\n'''
LOG.write_text(log, encoding="utf-8")

# DEVAM.md is a generated single-file handoff; replace stale Stage 04-era snapshot with current state.
devam = f'''# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff\n\nBu dosya, yeni bir ChatGPT/AI oturumunda projeye kaldığı yerden devam etmek için tek giriş noktasıdır. Repo kayıtları sohbet/model belleğinden üstündür.\n\n## Yeni AI için doğrudan talimat\n\n1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunu ve gerçek `main` HEAD'i doğrula.\n2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `gecmis.md` ve `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` dosyalarını oku.\n3. AŞAMA 07 için `docs/evidence/STAGE_07.md` ve `docs/ADR/0002-procad-pinned-source-no-go.md` dosyalarını oku.\n4. Kullanıcı yalnız `devam` diyorsa `NEXT_WORK_STAGE` üzerinden ilerle.\n5. Bir kullanıcı turunda en fazla bir aşama tamamla; aynı turda sonraki aşamayı başlatma.\n6. Fiziksel cihaz/Mac/Apple hesabı gibi kullanıcının sağlayamadığı dış kapıları sahte PASS/DONE yapma; `DEFERRED_EXTERNAL_GATE` bırak.\n7. Her aşama sonunda canonical checkpoint, `gecmis.md` ve `docs/evidence/STAGE_XX.md` kaydını gerçek CI/commit/artifact kanıtıyla güncelle.\n8. Production dependency'yi evidence olmadan yükseltme veya ProCad'ı tekrar graph'a sokma.\n\n## Repo / ürün\n\n- Repo: `smitelagwar/mobil-dwg` (private), default `main`.\n- Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer.\n- v1 viewer-only; edit/save/export/cloud/account yok.\n\n## Güncel checkpoint\n\n```text\nLAST_COMPLETED_STAGE: AŞAMA 07\nDEFERRED_STAGES: AŞAMA 01; AŞAMA 06\nAŞAMA_01: BLOCKED / DEFERRED_EXTERNAL_GATE — gerçek Android install/launch + iOS erişim envanteri\nAŞAMA_06: BLOCKED / DEFERRED_EXTERNAL_GATE — safe-open CI PASS; gerçek telefon FilePicker/SAF+lifecycle/cache gate açık\nAŞAMA_07: DONE / NO-GO — exact unpatched ProCad candidate systematic precision blocker nedeniyle production reuse için reddedildi\nNEXT_WORK_STAGE: AŞAMA 08\nNEXT_WORK_STATUS: NOT_STARTED\nSTAGE06_MERGE: PR #8 -> main `{STAGE06_MERGE}`\nSTAGE07_DECISION_HEAD: `{DECISION_HEAD}`\nSTAGE07_PR: #9\nSTAGE07_CI: run `{RUN07}` / #5 SUCCESS; artifact `{ARTIFACT07}`; `{DIGEST07}`\nSTAGE07_HARD_BLOCKER: origin 5,000,000 + 1 mm detail double->float RenderScene boundary'sinde delta 0.0\nSTAGE07_PHYSICAL_ANDROID_T3: NOT_RUN_AFTER_DETERMINISTIC_BLOCKER — PASS değil\nSTAGE09_GO_BARRIER: custom renderer effort/maintenance risk HIGH; AŞAMA 09 implementation öncesinde kullanıcı GO gerekir\nNEXT_ACTION: AŞAMA 08 — erken iOS AOT/native fizibilite smoke.\n```\n\n## Tamamlanan / açık aşamalar\n\n- AŞAMA 00 — DONE\n- AŞAMA 01 — BLOCKED / DEFERRED_EXTERNAL_GATE\n- AŞAMA 02 — DONE\n- AŞAMA 03 — DONE\n- AŞAMA 04 — DONE\n- AŞAMA 05 — DONE; ACadSharp 3.7.1 read-only parser baseline GO\n- AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE; cihazdan bağımsız safe-open/Android build CI PASS\n- AŞAMA 07 — DONE / NO-GO; ProCad production reuse rejected\n- AŞAMA 08 — NEXT\n\n## AŞAMA 07 özeti\n\nExact candidate:\n\n- ProCad `f8a862b3e7634e27664fee02ff5d68774b102985`\n- ACadSharp submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36`\n- ProEdit `64759b79289a024d08463ed1a9094fdcd9a270df`\n\nLineage official upstream'de çözüldü ancak approved ACadSharp 3.7.1 source baseline 592 commit ileride. Pinned source Android build başarılı (`82 warning / 0 error`); clean MAUI Release smoke başarılı (`0 warning / 0 error`). Published ProCadSharp 0.1.1 restore graph ACadSharp 1.0.0 ve Skia 4.147.0-preview.2.1 çözüyor; source graph ile eşdeğer değil.\n\nHard blocker: ProCad scene boundary'sinde CAD world point doğrudan float Vector2'ye daralıyor. Origin 100 + 1 mm detay korunurken origin 5,000,000 + 1 mm detay float'ta aynı değere düşüyor; observed delta 0.0. Bu systematic P0 fidelity loss. Exact unpatched candidate `NO-GO`; production graph'a eklenmez.\n\nÖzel renderer garantili fallback değildir. ADR 0002, AŞAMA 09–16 renderer/fidelity kapsamını ve sonraki performance/full-corpus gate'lerini HIGH effort/maintenance risk olarak kaydeder. AŞAMA 09 implementation'dan önce kullanıcı GO gerekir.\n\n## Değiştirilemez ilkeler\n\n- Original CAD immutable; overwrite yok.\n- Unsupported/proxy/font/XREF/raster sessiz kayıp olarak gizlenmez.\n- UI parser entity'lerine doğrudan bağlanmaz.\n- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; policy-RED/unknown release blocker.\n- Gerçek cihaz kanıtı yoksa cihaz PASS yazılmaz.\n- Bir turda en fazla bir aşama tamamlanır.\n'''
DEVAM.write_text(devam, encoding="utf-8")

print("STAGE07_CHECKPOINT_SYNC_PASS")
