# Evidence Template

Bu şablon normal implementation aşaması ve `ANDROID_DOGRULAMA_PLANI.md` VXX çıkış kriterlerinin kanıtını standartlaştırır. Tarihsel STAGE evidence dosyası yeni VXX sonucu varmış gibi geriye dönük değiştirilmez.

## Kimlik

- Tarih:
- Aşama / alt adım:
- Cursor: `ANDROID_VALIDATION` / `IMPLEMENTATION`
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

## Sonraki eylem

Tek somut `NEXT_ACTION`.
