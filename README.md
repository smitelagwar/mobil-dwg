# mobil-dwg

Android için local/offline çalışan, read-only 2D DWG/DXF görüntüleyici.

## Güncel durum

- Android v1 geliştirme planı AŞAMA 27 ile tamamlandı.
- Eski V01–V09 doğrulama programı ve A10 paralel workstream kapandı.
- Bundan sonraki işler **yeni bug fix, kalite, performans ve viewer geliştirmesi** olarak ele alınır; eski stage cursor'ı devam ettirilmez.
- Aktif platform Android'dir. iOS yalnız ileride açıkça yeniden etkinleştirilirse ele alınır.
- Son v1 handoff claim'i fiziksel cihaz fidelity iddiası içermez; geçmiş kanıtlar `docs/evidence/` altındadır.

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

Eski `BASLA.md`, `DEVAM.md`, A10 workstream veya VXX cursor dosyaları artık yoktur. Yeni bir AI/ajan ya da geliştirici şu sırayı kullanmalıdır:

1. Gerçek `main` kodunu ve ilgili dosyaları oku.
2. [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) ile dependency sınırlarını kontrol et.
3. Render/parser doğruluğu etkileniyorsa [`docs/GOLDEN_CONTRACT.md`](docs/GOLDEN_CONTRACT.md) ve geçmiş `docs/evidence/` kayıtlarını incele.
4. Dependency/asset değişiyorsa `compliance/` belgelerini kontrol et.
5. Android davranışı değişiyorsa [`docs/ANDROID_TESTING.md`](docs/ANDROID_TESTING.md) üzerinden değişikliğe özel doğrulama yap.
6. Yeni iş için gerekiyorsa yeni, küçük ve işe özel plan/evidence oluştur; tamamlanmış eski planları tekrar yürütme.

Sohbet/model belleği repo gerçekliğinin yerine geçmez.

## Yaşayan dokümantasyon

| Dosya | Amaç |
|---|---|
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Güncel production katmanları ve dependency sınırları |
| [`docs/GOLDEN_CONTRACT.md`](docs/GOLDEN_CONTRACT.md) | Fixture, semantic golden ve render doğruluk kuralları |
| [`docs/TOOLCHAIN.md`](docs/TOOLCHAIN.md) | Pinli build/toolchain baseline |
| [`docs/ANDROID_TESTING.md`](docs/ANDROID_TESTING.md) | Tek Android/emulator/fiziksel cihaz test rehberi |
| [`docs/DEVICE_MATRIX.md`](docs/DEVICE_MATRIX.md) | Emulator ve fiziksel cihaz test sınıfları |
| [`docs/EVIDENCE_TEMPLATE.md`](docs/EVIDENCE_TEMPLATE.md) | Yeni önemli değişiklikler için kanıt formatı |
| [`compliance/LICENSE_POLICY.md`](compliance/LICENSE_POLICY.md) | Dependency/asset lisans politikası |
| [`compliance/DEPENDENCY_EVIDENCE.md`](compliance/DEPENDENCY_EVIDENCE.md) | Dependency provenance/evidence |
| [`compliance/RISK_REGISTER.md`](compliance/RISK_REGISTER.md) | Güncel açık teknik/ürün riskleri |
| [`gecmis.md`](gecmis.md) | Kısa v1 tarihçesi ve kalıcı teknik karar özeti |

`docs/ADR/` mimari kararları, `docs/evidence/` ise tarihsel doğrulama kanıtlarını içerir. Bu kayıtlar geçmişi korumak için tutulur; aktif iş listesi değildir.

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

## Tarihsel planlar

2026-09-05 dokümantasyon temizliğinde tamamlanmış başlangıç/handoff/validation/implementation plan dosyaları çalışma ağacından kaldırıldı. Gerektiğinde Git geçmişinden erişilebilir; normal geliştirmede okunmaları veya yeniden oluşturulmaları gerekmez.
