# mobil-dwg architecture boundary — AŞAMA 04

Bu belge AŞAMA 04'te kurulan minimal production sınırlarını tanımlar. Bu aşamada parser veya renderer implementation paketi production projelerine eklenmez.

## Production projeleri

Tam olarak dört production projesi vardır:

1. `src/MobilDwg.Core`
   - BCL-only domain/contracts.
   - ProjectReference yok.
   - PackageReference yok.
   - MAUI, SkiaSharp veya ACadSharp referansı yok.

2. `src/MobilDwg.Cad`
   - CAD parser adapter boundary.
   - Yalnız `MobilDwg.Core` referansı.
   - ACadSharp concrete adapter AŞAMA 05'te bu sınırın içine eklenir.

3. `src/MobilDwg.Rendering`
   - Render scene builder/renderer adapter boundary.
   - Yalnız `MobilDwg.Core` referansı.
   - SkiaSharp implementation sonraki renderer aşamalarında bu sınırın içine eklenir.

4. `src/MobilDwg.App`
   - Composition/UI boundary.
   - `Core`, `Cad`, `Rendering` referansları.
   - Parser entity veya Skia type'ına doğrudan bağlanmaz.
   - AŞAMA 04'te platform bağımsız class library olarak başlar. AŞAMA 06'da aynı proje MAUI/Android shell'e dönüştürülür; beşinci production proje açılmaz.

Dependency yönü:

```text
MobilDwg.Core
   ↑       ↑
MobilDwg.Cad   MobilDwg.Rendering
        ↑       ↑
          MobilDwg.App
```

`Core` dış teknolojiye doğru bağımlılık taşımaz.

## Kontratlar

`MobilDwg.Core` şu boundary'leri taşır:

- `ICadDocumentReader`
- `CadDocumentSession` + `ICadDocumentHandle`
- diagnostics/compatibility kayıtları
- `IRenderSceneBuilder`
- `ICadRenderer`
- `IRenderScene` / `IRenderSurface`
- `RenderViewport`

Session, parser-specific handle'ın tek sahibidir ve handle'ı tam bir kez dispose eder. UI concrete parser entity'sini görmez.

## Cancellation ve progress doğruluğu

`ICadDocumentReader.Capabilities` gerçek destek düzeyini açıkça ilan eder:

- Cancellation: `None`, `BeforeStartOnly`, `Cooperative`
- Progress: `None`, `StagesOnly`, `Fractional`

Bir adapter cooperative parser abort sağlayamıyorsa `Cooperative` ilan edemez. Kesir bilinmiyorsa `CadReadProgress.Fraction` `null` kalır; sahte yüzde üretilmez.

## Test projeleri

Tam olarak üç test projesi vardır ve Stage 04'te yeni test framework dependency'si eklenmez:

- `MobilDwg.Core.Tests`
- `MobilDwg.Rendering.Tests`
- `MobilDwg.Architecture.Tests`

Bunlar deterministic executable test harness'larıdır. CI `scripts/stage04-test.sh` ile restore, Release build ve üç harness'ı çalıştırır. Architecture harness proje sayısını, ProjectReference yönlerini, PackageReference yokluğunu ve Core/App forbidden dependency terimlerini otomatik denetler.

Standart bir test framework'ü daha sonra gerekirse ayrı dependency/lisans değerlendirmesiyle eklenebilir.
