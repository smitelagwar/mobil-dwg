# mobil-dwg — Evidence Template

Bu şablon gelecekteki önemli bug fix, performance, dependency, parser veya viewer değişikliklerinin doğrulama kaydı içindir. Eski VXX/A10 cursor modeli artık kullanılmaz.

## Kimlik

- Tarih / timezone:
- İş / issue / kısa başlık:
- Durum: `IN_PROGRESS` / `BLOCKED` / `VALIDATED` / `DONE`
- Repo / branch:
- Başlangıç revision:
- Son revision:
- Test edilen exact revision:
- Ortam: host / Android emulator / physical Android / diğer
- Cihaz slotu gerekiyorsa:

## Problem ve beklenen davranış

- Gözlenen problem:
- Beklenen davranış:
- Kök neden:
- Claim sınırı:

## Değişiklik

Değiştirilen dosyalar, mimari etkisi ve nedenleri.

## Komutlar ve testler

| Komut / test / senaryo | Sonuç | Not / metric |
|---|---|---|
|  | `PASS` / `FAIL` / `BLOCKED` |  |

## Artifact / ölçüm

Gerekiyorsa:

- APK/AAB/build artifact:
- SHA-256/checksum:
- screenshot/video/log:
- process/PID/runtime kanıtı:
- parse/scene/first-frame:
- frame p50/p95:
- PSS/native memory:
- entity/primitive count:

## Regression kapsamı

- Etkilenen eski davranışlar:
- Çalıştırılan ilgili historical gate/test:
- Bilerek değiştirilen expectation varsa nedeni:

Eski stage/evidence sonucu yeni implementasyona göre geriye dönük değiştirilmez. Yeni hata yeni commit ve yeni evidence ile kaydedilir.

## Lisans / provenance

Dependency, native binary, font, fixture veya asset eklendiyse:

- exact sürüm / commit,
- source,
- lisans,
- transitive/native etkisi,
- redistribution durumu

kaydedilir.

## Risk / blocker

Bilinen sınırlamalar, test edilemeyen ortamlar ve açık riskler.

Runner çevrim dışıysa PASS yazılmaz; test edilecek exact SHA ve gerekli koşu kaydedilir.

## Sonuç

- Final status:
- Kanıtlanan şey:
- Kanıtlanmayan şey:
- Sonraki somut eylem:
