# AŞAMA 04 Evidence — minimal solution ve mimari sınırlar

Tarih: 2026-08-24

Durum: `IN_PROGRESS`

Başlangıç main revision: `a18480f55c76658027ba44ade33d1b88c9d4d6d8`.

AŞAMA 01 dış cihaz kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır.

## Hedef

- dört production proje,
- üç test proje,
- BCL-only Core,
- parser/renderer/UI dependency sınırları,
- session ownership,
- diagnostics/compatibility kontratları,
- gerçek cancellation/progress capability modeli,
- otomatik architecture dependency testleri,
- clean restore/build/test.

## Tasarım kararı

Production project set:

- `MobilDwg.Core`
- `MobilDwg.Cad`
- `MobilDwg.Rendering`
- `MobilDwg.App`

Test project set:

- `MobilDwg.Core.Tests`
- `MobilDwg.Rendering.Tests`
- `MobilDwg.Architecture.Tests`

Bu aşamada ACadSharp/SkiaSharp/MAUI concrete dependency production project graph'a eklenmez. `MobilDwg.App` composition boundary olarak başlar ve AŞAMA 06'da aynı proje MAUI shell'e dönüştürülebilir.

## Beklenen CI

Workflow: `Stage 04 Architecture`.

PASS gereksinimleri:

- exact .NET SDK `10.0.400`,
- solution restore,
- Release build warnings-as-errors,
- core contract harness,
- render contract harness,
- architecture dependency harness,
- final `STAGE04_T0_PASS`.

CI sonucu alınmadan AŞAMA 04 `DONE` yapılmaz.
