# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff

Bu dosya yeni bir ChatGPT/AI oturumunda projeye kaldığı yerden devam etmek için tek giriş noktasıdır. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunu, gerçek `main` HEAD'i ve açık PR'ları doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `gecmis.md`, `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` ve aktif aşama evidence dosyasını oku.
3. Açık `IN_PROGRESS` aşama varsa yeni aşama başlatmadan yalnız onu sürdür.
4. Bir kullanıcı turunda en fazla bir aşama tamamla.
5. Fiziksel cihaz/Mac/Apple hesabı gibi dış kapıları sahte PASS/DONE yapma.
6. Production dependency'yi evidence olmadan yükseltme veya ProCad'ı tekrar graph'a sokma.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg` (private), default `main`.
- Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer.
- v1 viewer-only; edit/save/export/cloud/account yok.

## Güncel checkpoint

```text
LAST_COMPLETED_STAGE: AŞAMA 08 — CHARACTERIZATION / RISK_ACCEPTED_FOR_CONTINUATION; iOS PASS NOT CLAIMED
CURRENT_STAGE: AŞAMA 09
CURRENT_STAGE_STATUS: IN_PROGRESS — IMPLEMENTATION_READY / T0_T1_VALIDATION_PENDING_RUNNER
DEFERRED_STAGES: AŞAMA 01; AŞAMA 06; AŞAMA 08 local Mac/ios-arm64/physical iPhone gates
AŞAMA_01: BLOCKED / DEFERRED_EXTERNAL_GATE — gerçek Android install/launch + iOS erişim envanteri
AŞAMA_06: BLOCKED / DEFERRED_EXTERNAL_GATE — safe-open CI PASS; gerçek telefon FilePicker/SAF+lifecycle/cache gate açık
AŞAMA_07: DONE / NO-GO — exact unpatched ProCad candidate precision blocker nedeniyle production reuse için reddedildi
AŞAMA_08: DONE / CHARACTERIZATION — evidence BLOCKED_PARTIAL_EVIDENCE; iOS runtime/device PASS yok
AŞAMA_09_BRANCH: stage09-render-scene-camera
AŞAMA_09_PR: #12
AŞAMA_09_SOURCE_TEST_HEAD: 6215f9fbd77028273262bc5b95fd3eece19191d3
AŞAMA_09_WORKFLOW_FIX_HEAD: b6e3b5c825810c70e4ada750f576672ebe25d99d
AŞAMA_09_IMPLEMENTED: compact immutable RenderScene; stable entity/layer/style/source metadata; double camera pipeline; large-origin precision regression; OCS/WCS; diagnostics; fit/zoom/color context; deterministic semantic snapshot
AŞAMA_09_VALIDATION: NOT_RUN — hosted jobs reach no steps; runner_id=0
AŞAMA_09_FINAL_HOSTED_RUN: 32786600644 / #14; macos-26; attempts 1 and 2 both pre-step failure
AŞAMA_09_FINAL_HOSTED_JOBS: 97619697255; 97619957457
AŞAMA_09_SELF_HOSTED_PROBE: 32784140351 / #3; suitable online runner not assigned; temporary workflow removed
AŞAMA_09_EVIDENCE: docs/evidence/STAGE_09.md
NEXT_ACTION: obtain any real exact .NET 10.0.400 execution environment; run Stage 09 T0 restore/build + T1 deterministic tests; fix compiler/test defects if any; only then close/merge PR #12
AŞAMA_10_STATUS: NOT_STARTED
```

## AŞAMA 09 özeti

Kullanıcı özel renderer efor/bakım riskini kabul ederek AŞAMA 09'un başlamasını açıkça onayladı. ADR 0002 sonrası tek production scene yolu compact özel immutable scene olarak seçildi; ProCad production graph'a eklenmedi.

Uygulanan foundation:

- Stable entity ID, bounds, layer/style token ve parser source reference.
- World/document coordinates ve world→view→screen hattında `double` precision.
- Survey origin `5,000,000` çevresinde `0.001` detay regression testi.
- OCS/WCS arbitrary-axis transform ve oblique round-trip testi.
- NaN/Infinity/extents guards.
- Unsupported/Substituted/Dropped/Error scene diagnostics.
- Camera fit, zoom clamps ve dark/light color context.
- Stable-ID sıralı immutable scene ve deterministic `render-scene/v1` semantic snapshot.
- Eski `STAGE04_RENDER_CONTRACT_TESTS_PASS` marker'ı test harness'ta korunur.

Validation henüz PASS değildir. Ubuntu hosted koşuları pre-step `runner_id=0`; ilk macOS fallback'ta yanlış `macos-26-arm64` label'ı fark edilip repoda AŞAMA 08'de kullanılan doğru `macos-26` label'ına düzeltildi. Buna rağmen doğru label ile run `32786600644`/#14 attempt 1 ve explicit rerun attempt 2 de `steps=[]`, `runner_id=0`, empty runner name ile kesildi. Root cause'un billing/quota/capacity olduğu kanıtlanmadığından tahmin edilmez. Configured self-hosted Windows runner probe'u da uygun online runner bulamadı ve geçici workflow silindi.

Bu nedenle PR #12 merge edilmedi ve AŞAMA 10 başlatılmadı.

## Önceki kritik kararlar

- ACadSharp `3.7.1` read-only parser baseline: GO.
- Exact unpatched ProCad source candidate: NO-GO; large-survey-origin mm detail direct double→float scene boundary'sinde çöker.
- AŞAMA 08 iOS characterization: risk kaydı tamamlandı fakat iOS runtime/device PASS kanıtlanmadı.
- AŞAMA 01/AŞAMA 06 gerçek Android ve AŞAMA 08 local iOS gates deferred olarak açık kalır.

## Değiştirilemez ilkeler

- Original CAD immutable; overwrite yok.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olarak gizlenmez.
- UI parser entity'lerine doğrudan bağlanmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; policy-RED/unknown release blocker.
- Gerçek cihaz veya test yürütme kanıtı yoksa PASS yazılmaz.
- Bir turda en fazla bir aşama tamamlanır.
