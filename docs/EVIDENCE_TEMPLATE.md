# Evidence Template

Bu şablon normal implementation aşaması ve `ANDROID_DOGRULAMA_PLANI.md` VXX çıkış kriterlerinin kanıtını standartlaştırır. Tarihsel STAGE evidence dosyası yeni VXX sonucu varmış gibi geriye dönük değiştirilmez.

## Kimlik

- Tarih:
- Aşama / alt adım:
- Cursor: `ANDROID_VALIDATION` / `IMPLEMENTATION`
- Workstream durumu: `IN_PROGRESS_UNVALIDATED` / `FIX_REQUIRED` / `FIX_IN_PROGRESS` / `CODED_PENDING_HOST_TESTS` / `CODED_PENDING_EMULATOR` / `READY_FOR_EMULATOR` / `READY_TO_MERGE` / `WAITING_RUNNER` / `VALIDATED` / `DONE`
- Repo / branch:
- Başlangıç revision:
- Son revision:
- Test edilen exact revision:
- Ortam / cihaz:
- Runner durumu:
- Kanıt kapsamı: host / infrastructure smoke / real app emulator / physical Android

## Değişiklik

Değiştirilen dosyalar ve amaçları.

## Komutlar ve testler

| Komut/Test | Sonuç | Not |
|---|---|---|
|  | PASS/FAIL/BLOCKED/WAITING_RUNNER |  |

## Artifact / ölçüm

- Build/artifact:
- Hash/checksum:
- Artifact format/okunabilirlik doğrulaması:
- Gerçek app/package adı ve process PID (runtime testi ise):
- Süre/bellek/frame metriği gerekiyorsa:

## Lisans / provenance

Dependency, native binary, font, fixture veya asset eklendiyse exact kaynak ve lisans kanıtı.

## Risk / blocker

Bilinen sınırlamalar ve nedenleri.

Runner çevrim dışıysa `PENDING_EMULATOR_QUEUE` için exact SHA, workflow/script, configuration ve beklenen marker burada yazılır; PASS yazılmaz.

A10 paralel draft kaydıysa ayrıca base `main` SHA, source branch/head SHA, `merge_allowed: false`, `blocked_by: V04_V09_PROGRAM + LATEST_MAIN_INTEGRATION + A10_ANDROID_GATE` ve varsa `superseded_by` yazılır. Draft SHA, V09 sonrası integration SHA yerine geçmez.

## Sonraki eylem

Tek somut `NEXT_ACTION`.
