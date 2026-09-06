# mobil-dwg — Güncel Mimari Sözleşmesi

Bu belge mevcut production dependency sınırıdır. Yeni geliştirmeler bu sınırı bilinçsizce bozmamalıdır.

## Production projeleri

### `src/MobilDwg.Core`

- Domain/contracts ve teknoloji-bağımsız temel tipler.
- ProjectReference yok.
- PackageReference yok.
- MAUI, SkiaSharp ve ACadSharp referansı yok.

### `src/MobilDwg.Cad`

- CAD parser/adapter katmanı.
- `MobilDwg.Core` referansı.
- ACadSharp production parser dependency'si burada tutulur.
- Parser'a özgü concrete tipler App/UI sınırına sızdırılmaz.

### `src/MobilDwg.Rendering`

- RenderScene, kamera, geometry, style, text, layout ve Skia rendering katmanı.
- `MobilDwg.Core` referansı.
- SkiaSharp dependency'si burada tutulur.
- World/document verisi `double` kalır; float/GPU tipine dönüşüm yalnız render sınırında yapılır.

### `src/MobilDwg.App`

- Android .NET MAUI composition/UI katmanı.
- `Core`, `Cad` ve `Rendering` projelerine referans verir.
- MAUI dependency'si burada tutulur.
- Dosya seçimi, viewer lifecycle, UI state ve composition bu katmandadır.
- Parser entity tipleri veya parser internal model'i UI API'si haline getirilmez.

`src/MobilDwg.App/Validation/` altındaki dosyalar normal kullanıcı özelliği değildir. Bunlar hedefli Android regression build'lerinde MSBuild flag'leriyle açılan compile-time validation runner'larıdır; normal production build'de ilgili kod yolları etkin değildir.

Dependency yönü:

```text
MobilDwg.Core
   ↑       ↑
MobilDwg.Cad   MobilDwg.Rendering
        ↑       ↑
          MobilDwg.App
```

`Core` dış teknolojiye doğru bağımlılık taşımaz.

## Temel kontratlar

`MobilDwg.Core` ve ortak boundary'ler aşağıdaki sorumlulukları ayırır:

- `ICadDocumentReader`
- document/session/handle sahipliği
- diagnostics ve compatibility kayıtları
- render scene / render surface / viewport kontratları
- cancellation ve progress capability tanımı

Bir parser gerçek cooperative cancellation sağlamıyorsa capability bunu iddia edemez. Kesirli progress bilinmiyorsa sahte yüzde üretilmez.

## Veri ve hassasiyet kuralları

- World/document coordinates `double`.
- Büyük survey koordinatlarında küçük detay kaybına yol açacak erken `float` dönüşümü yasaktır.
- Original CAD dosyası immutable kalır.
- Parser sonucu doğrudan UI state olarak tutulmaz; render için normalize edilmiş scene/primitive sınırı kullanılır.
- Unsupported veya compatibility problemi diagnostics üzerinden görünür tutulur.

## Rendering sınırı

Rendering katmanı:

- kamera/world-screen dönüşümünü,
- viewport culling'i,
- geometry tessellation/draw işlemlerini,
- style/text/hatch/layout/reference davranışını

merkezi şekilde yönetmelidir.

Yeni viewer surface, GPU backend, render scheduler, spatial index veya cache sistemi eklenirse mevcut scene/camera sözleşmesi bypass edilmemeli; architecture değişikliği gerekiyorsa bu dosya aynı değişiklikte güncellenmelidir.

## Test ve CI yapısı

Platform-neutral doğrulama üç executable harness ile yapılır:

- `tests/MobilDwg.Core.Tests`
- `tests/MobilDwg.Rendering.Tests`
- `tests/MobilDwg.Architecture.Tests`

Ana host CI: `.github/workflows/ci.yml`.

Android gerçek-app regression: `.github/workflows/android-emulator-test.yml` + `scripts/android-real-app-regression.ps1`.

Dependency/lisans graph denetimi: `.github/workflows/dependency-audit.yml`.

Numaralı bazı Android gate/marker'ları geçmiş regresyonlarla karşılaştırılabilirliği korumak için bulunmaktadır; bunlar geliştirme aşaması/cursor'u değildir.

Android test politikası: `docs/ANDROID_TESTING.md`.

## Future iOS

Aktif production hedef Android'dir. Core/Cad/Rendering mümkün olduğunca platform-neutral tutulur; iOS workload, signing veya platform implementation kullanıcı açıkça yeniden etkinleştirmedikçe Android graph'ına zorunlu dependency olarak eklenmez.

## Değişiklik kontrolü

Aşağıdakiler mimari değişiklik sayılır ve bilinçli inceleme gerektirir:

- yeni production proje,
- dependency yönünün değişmesi,
- Core'a harici package eklenmesi,
- App'in parser concrete tiplerine bağlanması,
- Rendering dışına Skia-specific render mantığının yayılması,
- world coordinate precision'ın `double` dışına düşmesi,
- local/offline viewer akışına zorunlu servis/cloud dependency eklenmesi.
