# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff

Bu dosya yeni bir ChatGPT/AI oturumunda projeye kaldığı yerden devam etmek için tek giriş noktasıdır. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunu, gerçek `main` HEAD'i ve açık PR'ları doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `gecmis.md`, `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` ve son tamamlanan/aktif aşama evidence dosyasını oku.
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
LAST_COMPLETED_STAGE: AŞAMA 09 — DONE
DEFERRED_STAGES: AŞAMA 01; AŞAMA 06; AŞAMA 08 local Mac/ios-arm64/physical iPhone gates
AŞAMA_01: BLOCKED / DEFERRED_EXTERNAL_GATE — gerçek Android install/launch + iOS erişim envanteri
AŞAMA_06: BLOCKED / DEFERRED_EXTERNAL_GATE — safe-open CI PASS; gerçek telefon FilePicker/SAF+lifecycle/cache gate açık
AŞAMA_07: DONE / NO-GO — exact unpatched ProCad candidate precision blocker nedeniyle production reuse için reddedildi
AŞAMA_08: DONE / CHARACTERIZATION — evidence BLOCKED_PARTIAL_EVIDENCE; iOS runtime/device PASS yok
AŞAMA_09: DONE — compact immutable RenderScene + double camera + diagnostics + deterministic snapshot
AŞAMA_09_USER_GO: GRANTED / CONSUMED — yeniden istenmez
AŞAMA_09_PR: #12 — kapanış/merge için hazırlanmış branch stage09-render-scene-camera
AŞAMA_09_SOURCE_TEST_HEAD: 9a17d333afc0a3df1de856a9a53fae0e74617c29
AŞAMA_09_VALIDATION_HEAD: 7bba0b7a6da30dc4b23050872a7a1ef4e90ca087
AŞAMA_09_VALIDATION_RUN: 32815175055 / #6 SUCCESS
AŞAMA_09_VALIDATION_JOB: 97701882792 SUCCESS
AŞAMA_09_ARTIFACT: 9551137293; stage09-self-hosted-evidence; 1578 bytes
AŞAMA_09_ARTIFACT_DIGEST: sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd
AŞAMA_09_MARKERS: STAGE09_DOTNET_PIN_PASS; STAGE09_T0_BUILD_PASS; STAGE04_RENDER_CONTRACT_TESTS_PASS; STAGE09_RENDER_SCENE_TESTS_PASS; render-scene/v1; STAGE09_T1_SCENE_PASS; STAGE09_STAGE04_REGRESSION_PASS
AŞAMA_09_BUILD: targeted + full solution Release 0 warning / 0 error
AŞAMA_09_EVIDENCE: docs/evidence/STAGE_09.md
AŞAMA_10_STATUS: NOT_STARTED
NEXT_WORK_STAGE: AŞAMA 10
NEXT_WORK_STATUS: READY / WAITING_USER_CONTINUE
NEXT_ACTION: kullanıcı `devam` dediğinde yalnız AŞAMA 10 — P0 temel geometri renderer'ı — başlat; aynı turda AŞAMA 11'e geçme.
```

## AŞAMA 09 özeti

Kullanıcı ADR 0002'deki özel renderer yüksek efor/bakım riskini kabul etti. Exact ProCad reuse `NO-GO` olduğundan tek production scene yolu compact özel immutable scene seçildi; ProCad production graph'a eklenmedi.

Tamamlanan foundation:

- Stable entity ID, bounds, layer/style token ve parser source reference.
- `default` record-struct metadata bypass ve duplicate-ID guard'ları.
- World/document coordinates ve world→view→screen hattında `double` precision.
- `Camera2D` ile Core `RenderViewport` arasında explicit adapter.
- Survey origin `5,000,000` çevresinde `0.001` detail precision regression.
- Finite span/subtraction overflow guards ve overflow-safe center.
- OCS/WCS arbitrary-axis transform, oblique round-trip ve büyük finite normal için scaled normalization.
- Unsupported/Substituted/Dropped/Error scene diagnostics.
- Camera fit, zoom clamps ve dark/light color context.
- Stable-ID sıralı immutable scene ve deterministic `render-scene/v1` semantic snapshot.

Yetkili kapanış validation'ı self-hosted Windows runner üzerinde exact .NET `10.0.400` ile gerçek checkout/restore/build/run yaptı. Hedefli A09 Release build ve tam solution Release build `0 warning / 0 error`; Core/Rendering/Architecture ve Stage 05 dependency-boundary regresyonları geçti. Artifact `9551137293`, digest `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`.

Önceki `ubuntu-latest`, `macos-26` ve `ubuntu-slim` zero-step/runner_id=0 kayıtları hosted runner allocation problemiydi; self-hosted gerçek execution PASS ile A09 implementation failure olmadığı ayrıştırıldı. Geçici self-hosted A09 workflow'u kapanıştan sonra branch'ten kaldırıldı; kalıcı Stage 09 workflow'u `ubuntu-latest` üzerinde bırakıldı.

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
