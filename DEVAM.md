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
AŞAMA_09_USER_GO: GRANTED — kullanıcı custom renderer implementation başlangıcını açıkça onayladı
AŞAMA_09_BRANCH: stage09-render-scene-camera
AŞAMA_09_PR: #12 — OPEN / NOT_MERGED
AŞAMA_09_LATEST_SOURCE_TEST_HEAD: 9a17d333afc0a3df1de856a9a53fae0e74617c29
AŞAMA_09_LATEST_WORKFLOW_HEAD: 0c5aa84bf491ec24c4409c35ffad83dd159b9290
AŞAMA_09_IMPLEMENTED: compact immutable RenderScene; stable entity/layer/style/source metadata; double camera pipeline; RenderViewport bridge; large-origin precision regression; finite-overflow guards; OCS/WCS scaled normalization; diagnostics; fit/zoom/color context; deterministic semantic snapshot
AŞAMA_09_VALIDATION: NOT_EXECUTED — hiçbir hosted/self-hosted denemede checkout/build/test step'i başlamadı
AŞAMA_09_UBUNTU_RUN: 32791364379 / #30; rerun attempt 2 job 97690824454; ubuntu-latest; steps=[]; runner_id=0
AŞAMA_09_MACOS_RUN: 32786600644 / #14; macos-26; attempts 1/2/3 all pre-step failure
AŞAMA_09_SLIM_RUN: 32811281420 / #32; job 97690952636; ubuntu-slim; steps=[]; runner_id=0
AŞAMA_09_SELF_HOSTED_PROBE: 32784140351 / #3; suitable online runner not assigned; temporary workflow removed
AŞAMA_09_EVIDENCE: docs/evidence/STAGE_09.md
NEXT_WORK_STAGE: AŞAMA 09
NEXT_ACTION: obtain any real exact .NET 10.0.400 execution environment; run Stage 09 T0 restore/build + T1 deterministic tests; fix compiler/test defects if any; only then close/merge PR #12
AŞAMA_10_STATUS: NOT_STARTED
```

## AŞAMA 09 özeti

Kullanıcı özel renderer efor/bakım riskini kabul ederek AŞAMA 09'un başlamasını açıkça onayladı. ADR 0002 sonrası tek production scene yolu compact özel immutable scene olarak seçildi; ProCad production graph'a eklenmedi.

Uygulanan foundation:

- Stable entity ID, bounds, layer/style token ve parser source reference.
- `default` record-struct metadata bypass'ları immutable scene sınırında tekrar doğrulanır.
- World/document coordinates ve world→view→screen hattında `double` precision.
- `Camera2D` ile Core `RenderViewport` arasında explicit adapter; ikinci gizli kamera hattı yok.
- Survey origin `5,000,000` çevresinde `0.001` detay regression testi.
- Finite girdilerin span/subtraction sırasında `Infinity` üretmesine karşı guards; büyük same-sign bounds center overflow-safe hesaplanır.
- OCS/WCS arbitrary-axis transform, oblique round-trip ve çok büyük finite normal için scaled normalization.
- Unsupported/Substituted/Dropped/Error scene diagnostics; invalid taxonomy/default entity ID guard'ları.
- Camera fit, zoom clamps, invalid default-camera guard ve dark/light color context.
- Stable-ID sıralı immutable scene ve deterministic `render-scene/v1` semantic snapshot.
- Eski `STAGE04_RENDER_CONTRACT_TESTS_PASS` marker'ı test harness'ta korunur.

Validation henüz PASS değildir. Standard Linux (`ubuntu-latest`), doğru macOS (`macos-26`) ve ayrı lightweight container pool (`ubuntu-slim`) hosted job'ları checkout başlamadan `steps=[]`, `runner_id=0` ile kesildi. En güncel ayrı-pool denemesi Stage 09 run `32811281420`/#32, job `97690952636` üzerinde aynı sonucu verdi. Configured self-hosted Windows runner probe'u da uygun online runner bulamadı ve geçici workflow silindi. Bu semptom compile/test failure değildir; billing/quota/policy/capacity gibi özel root cause kanıtlanmadığından tahmin edilmez.

Exact `.NET SDK 10.0.400` resmi Microsoft release metadata'sında doğrulandı; fakat mevcut execution container'ında SDK/compiler yok ve dış payload indirme yolu tamamlanamadı. Farklı SDK ile sahte PASS üretilmedi.

Bu nedenle PR #12 merge edilmedi ve AŞAMA 10 başlatılmadı. Yeni runner label'ları deneyerek tekrar zinciri üretme; bundan sonraki somut kapı gerçek exact .NET `10.0.400` execution environment'tır.

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
