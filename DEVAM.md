# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff

Bu dosya, yeni bir ChatGPT/AI oturumunda projeye kaldığı yerden devam etmek için tek giriş noktasıdır. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunu ve gerçek `main` HEAD'i doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `gecmis.md` ve `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` dosyalarını oku.
3. AŞAMA 07 için `docs/evidence/STAGE_07.md` ve `docs/ADR/0002-procad-pinned-source-no-go.md` dosyalarını oku.
4. Kullanıcı yalnız `devam` diyorsa `NEXT_WORK_STAGE` üzerinden ilerle.
5. Bir kullanıcı turunda en fazla bir aşama tamamla; aynı turda sonraki aşamayı başlatma.
6. Fiziksel cihaz/Mac/Apple hesabı gibi kullanıcının sağlayamadığı dış kapıları sahte PASS/DONE yapma; `DEFERRED_EXTERNAL_GATE` bırak.
7. Her aşama sonunda canonical checkpoint, `gecmis.md` ve `docs/evidence/STAGE_XX.md` kaydını gerçek CI/commit/artifact kanıtıyla güncelle.
8. Production dependency'yi evidence olmadan yükseltme veya ProCad'ı tekrar graph'a sokma.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg` (private), default `main`.
- Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer.
- v1 viewer-only; edit/save/export/cloud/account yok.

## Güncel checkpoint

```text
LAST_COMPLETED_STAGE: AŞAMA 07
DEFERRED_STAGES: AŞAMA 01; AŞAMA 06
AŞAMA_01: BLOCKED / DEFERRED_EXTERNAL_GATE — gerçek Android install/launch + iOS erişim envanteri
AŞAMA_06: BLOCKED / DEFERRED_EXTERNAL_GATE — safe-open CI PASS; gerçek telefon FilePicker/SAF+lifecycle/cache gate açık
AŞAMA_07: DONE / NO-GO — exact unpatched ProCad candidate systematic precision blocker nedeniyle production reuse için reddedildi
NEXT_WORK_STAGE: AŞAMA 08
NEXT_WORK_STATUS: NOT_STARTED
STAGE06_MERGE: PR #8 -> main `e3a9c36e04be6c51827926ca17bb1a386c6b1142`
STAGE07_DECISION_HEAD: `3f88bec383de895e309e218c08d13e9784562a97`
STAGE07_PR: #9
STAGE07_CI: run `32766501837` / #5 SUCCESS; artifact `9534797361`; `sha256:9cae376fd0cbf2861f006af347483f9de26a6cd49f30b201438a3afdb591e555`
STAGE07_HARD_BLOCKER: origin 5,000,000 + 1 mm detail double->float RenderScene boundary'sinde delta 0.0
STAGE07_PHYSICAL_ANDROID_T3: NOT_RUN_AFTER_DETERMINISTIC_BLOCKER — PASS değil
STAGE09_GO_BARRIER: custom renderer effort/maintenance risk HIGH; AŞAMA 09 implementation öncesinde kullanıcı GO gerekir
NEXT_ACTION: AŞAMA 08 — erken iOS AOT/native fizibilite smoke.
```

## Tamamlanan / açık aşamalar

- AŞAMA 00 — DONE
- AŞAMA 01 — BLOCKED / DEFERRED_EXTERNAL_GATE
- AŞAMA 02 — DONE
- AŞAMA 03 — DONE
- AŞAMA 04 — DONE
- AŞAMA 05 — DONE; ACadSharp 3.7.1 read-only parser baseline GO
- AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE; cihazdan bağımsız safe-open/Android build CI PASS
- AŞAMA 07 — DONE / NO-GO; ProCad production reuse rejected
- AŞAMA 08 — NEXT

## AŞAMA 07 özeti

Exact candidate:

- ProCad `f8a862b3e7634e27664fee02ff5d68774b102985`
- ACadSharp submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36`
- ProEdit `64759b79289a024d08463ed1a9094fdcd9a270df`

Lineage official upstream'de çözüldü ancak approved ACadSharp 3.7.1 source baseline 592 commit ileride. Pinned source Android build başarılı (`82 warning / 0 error`); clean MAUI Release smoke başarılı (`0 warning / 0 error`). Published ProCadSharp 0.1.1 restore graph ACadSharp 1.0.0 ve Skia 4.147.0-preview.2.1 çözüyor; source graph ile eşdeğer değil.

Hard blocker: ProCad scene boundary'sinde CAD world point doğrudan float Vector2'ye daralıyor. Origin 100 + 1 mm detay korunurken origin 5,000,000 + 1 mm detay float'ta aynı değere düşüyor; observed delta 0.0. Bu systematic P0 fidelity loss. Exact unpatched candidate `NO-GO`; production graph'a eklenmez.

Özel renderer garantili fallback değildir. ADR 0002, AŞAMA 09–16 renderer/fidelity kapsamını ve sonraki performance/full-corpus gate'lerini HIGH effort/maintenance risk olarak kaydeder. AŞAMA 09 implementation'dan önce kullanıcı GO gerekir.

## Değiştirilemez ilkeler

- Original CAD immutable; overwrite yok.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olarak gizlenmez.
- UI parser entity'lerine doğrudan bağlanmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; policy-RED/unknown release blocker.
- Gerçek cihaz kanıtı yoksa cihaz PASS yazılmaz.
- Bir turda en fazla bir aşama tamamlanır.
