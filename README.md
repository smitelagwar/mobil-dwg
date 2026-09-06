# mobil-dwg

Android için local/offline çalışan, read-only 2D DWG/DXF görüntüleyici.

## Güncel durum

- Android v1 geliştirme programı tamamlandı.
- Eski Stage/V/A doğrulama numaralandırmaları kapalıdır; yeni işler bug fix, kalite, performans ve viewer geliştirmesi olarak ele alınır.
- Aktif platform Android'dir. iOS yalnız ileride açıkça yeniden etkinleştirilirse ele alınır.
- Emulator doğrulaması fiziksel cihaz fidelity iddiası değildir.

## Teknoloji ve sınırlar

- .NET 10 / .NET MAUI Android
- ACadSharp `3.7.1` — read-only DWG/DXF parser adapter
- SkiaSharp `4.151.1` — 2D rendering
- Android target API 36, minimum API 24
- `double` world/document coordinates

Production katmanları:

```text
MobilDwg.Core
   ↑       ↑
MobilDwg.Cad   MobilDwg.Rendering
        ↑       ↑
          MobilDwg.App
```

Ayrıntı: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)

## Değiştirilemez temel ilkeler

Kullanıcı açıkça değiştirmedikçe:

- Orijinal DWG/DXF immutable kalır; production writer/save yoktur.
- Temel açma ve görüntüleme local/offline çalışır; zorunlu cloud conversion yoktur.
- UI doğrudan parser entity tiplerine bağlanmaz.
- World/document koordinatlarında `double` hassasiyet korunur.
- Unsupported entity, eksik font, XREF/raster veya compatibility problemi sessizce gizlenmez.
- Ücretli/trial CAD SDK veya zorunlu runtime servis lisansı eklenmez.
- GPL/AGPL/SSPL/BUSL/non-commercial/proprietary/unknown runtime dependency release graph'ına alınmaz.
- Performans optimizasyonu correctness'i değiştiremez; ölçülmüş bottleneck üzerinden yapılır.

Lisans politikası: [`compliance/LICENSE_POLICY.md`](compliance/LICENSE_POLICY.md)

## Yeni bir işe başlarken

Yeni bir AI/ajan ya da geliştirici şu sırayı kullanmalıdır:

1. Gerçek `main` kodunu ve ilgili dosyaları oku.
2. [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) ile dependency sınırlarını kontrol et.
3. Render/parser doğruluğu etkileniyorsa [`docs/GOLDEN_CONTRACT.md`](docs/GOLDEN_CONTRACT.md) ve ilgili güncel testleri incele.
4. Dependency/asset değişiyorsa `compliance/` belgelerini kontrol et.
5. Android davranışı değişiyorsa [`docs/ANDROID_TESTING.md`](docs/ANDROID_TESTING.md) üzerinden değişikliğe özel doğrulama yap.
6. Tamamlanmış eski Stage/V/A akışlarını yeniden başlatma.

Sohbet/model belleği repo gerçekliğinin yerine geçmez.

## Yaşayan dokümantasyon

| Dosya | Amaç |
|---|---|
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Güncel production katmanları ve dependency sınırları |
| [`docs/GOLDEN_CONTRACT.md`](docs/GOLDEN_CONTRACT.md) | Fixture, semantic golden ve render doğruluk kuralları |
| [`docs/TOOLCHAIN.md`](docs/TOOLCHAIN.md) | Pinli build/toolchain baseline |
| [`docs/ANDROID_TESTING.md`](docs/ANDROID_TESTING.md) | Android/emulator/fiziksel cihaz test rehberi |
| [`docs/DEVICE_MATRIX.md`](docs/DEVICE_MATRIX.md) | Emulator ve fiziksel cihaz test/benchmark sınıfları |
| [`docs/HISTORY.md`](docs/HISTORY.md) | Kısa proje tarihçesi ve kalıcı teknik karar özeti |
| [`compliance/LICENSE_POLICY.md`](compliance/LICENSE_POLICY.md) | Dependency/asset lisans politikası |
| [`compliance/DEPENDENCY_EVIDENCE.md`](compliance/DEPENDENCY_EVIDENCE.md) | Dependency provenance/evidence |
| [`compliance/RISK_REGISTER.md`](compliance/RISK_REGISTER.md) | Güncel açık teknik/ürün riskleri |

`docs/ADR/` kalıcı mimari kararları içerir. Ayrıntılı eski aşama/validation kanıtları çalışma ağacında tutulmaz; gerektiğinde Git geçmişinden okunur.

## Fixture ve özel dosya politikası

- Gerçek müşteri/kullanıcı DWG-DXF dosyaları repoya eklenmez.
- Proprietary SHX/font, signing key, token veya secret commit edilmez.
- Public/synthetic fixture yalnız provenance ve yeniden dağıtım hakkı kayıtlıysa kullanılır.
- Fixture sözleşmesi için [`fixtures/README.md`](fixtures/README.md) ve [`fixtures/public/synthetic/NOTICE.md`](fixtures/public/synthetic/NOTICE.md) esas alınır.

## Build

Pinli SDK kurulu ortamda:

```powershell
dotnet build .\MobilDwg.sln -c Release
```

Android testleri ve cihaz ayrımı için [`docs/ANDROID_TESTING.md`](docs/ANDROID_TESTING.md) kullanılır.

## Geçmiş geliştirme kayıtları

Tamamlanmış başlangıç/handoff/validation planları ve ayrıntılı Stage/V evidence dosyaları normal çalışma ağacından çıkarılmıştır. Git geçmişi bu kayıtları korur; yeni geliştirmede bunların yeniden oluşturulması veya kaldığı yerden devam ettirilmesi gerekmez.
