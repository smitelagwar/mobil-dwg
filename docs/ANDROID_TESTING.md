# mobil-dwg — Android Test Rehberi

Bu dosya bug fix, performans ve viewer geliştirmelerinde kullanılacak güncel Android test rehberidir. Eski Stage/V/A numaraları geliştirme cursor'u değildir.

## Temel kural

Bir değişiklik ancak **değiştirilen davranışı gerçekten ölçen** test ile doğrulanmış sayılır.

- Emulator sonucu fiziksel cihaz sonucu değildir.
- Host build sonucu Android runtime sonucu değildir.
- Ekranın açılması render doğruluğu değildir.
- Canvas/image varlığı doğru pan/zoom, metin veya eksiksiz CAD görünümü değildir.
- `queued`, zero-step, runner-offline veya artifact'siz run PASS değildir.
- Test edilen exact commit SHA kaydedilir.

## Yerel ortam

Toolchain baseline `docs/TOOLCHAIN.md` ve `global.json` ile tanımlanır.

Windows test makinesinde kullanılan temel bileşenler:

- Android AVD: `mobil-dwg-api36`
- hedef emulator: Android API 36
- self-hosted runner gerektiğinde interaktif kullanıcı oturumunda çalıştırılır

Ortam kontrolü:

```powershell
.\scripts\doctor-local-environment.ps1
```

Emulator başlatma / kapatma:

```powershell
.\scripts\start-emulator.ps1
.\scripts\stop-emulator.ps1
```

Solution Release build:

```powershell
dotnet build .\MobilDwg.sln -c Release
```

Platform-neutral harness'lar:

```powershell
dotnet run --project .\tests\MobilDwg.Core.Tests\MobilDwg.Core.Tests.csproj -c Release
dotnet run --project .\tests\MobilDwg.Rendering.Tests\MobilDwg.Rendering.Tests.csproj -c Release
dotnet run --project .\tests\MobilDwg.Architecture.Tests\MobilDwg.Architecture.Tests.csproj -c Release
```

## Ana gerçek-uygulama Android regression kapısı

En geniş güncel Android gerçek-app regression girişi:

```powershell
.\scripts\android-real-app-regression.ps1
```

Bu script gerçek `MobilDwg.App` APK'sını derler, emulator üzerinde kurar/başlatır ve kritik blocker regresyonlarını doğrular. GitHub Actions karşılığı `.github/workflows/android-emulator-test.yml` dosyasıdır ve pahalı olduğu için yalnız `android-test` branch push'unda veya manuel çalıştırılır.

## Hedefli regression araçları

`scripts/a10-*` … `scripts/a22-*` ve `scripts/a26-*` altında geçmişten kalan numaralı gate'lerin bir bölümü hâlâ belirli renderer/parser/device failure mode'larını gerçek uygulamada ölçer. Bunlar **aktif aşama sırası değildir**; marker ve MSBuild isimleri eski kanıtlarla uyumluluk için korunmuştur.

Yeni işte kural:

1. Önce host testini çalıştır.
2. Değiştirilen davranış Android zincirini etkiliyorsa `android-real-app-regression.ps1` çalıştır.
3. Sorun belirli bir alt sisteme aitse ilgili hedefli gate'i ek regresyon aracı olarak kullan.
4. Sırf eski numarası daha yüksek diye bir gate daha güncel kabul edilmez.

Compile-time Android validation runner'ları `src/MobilDwg.App/Validation/` altında tutulur. Normal build'de ilgili MSBuild validation flag'i verilmediği için bu kod yolları aktif değildir; yalnız hedefli regression build'lerinde etkinleşir.

## Değişiklik türüne göre zorunlu doğrulama

### Kamera / pan / pinch / zoom / render scheduling

En az şunlar ölçülür:

- tek parmak pan sırasında kamera ile görüntünün aynı frame zincirinde hareket etmesi,
- yeni görünür alanda parmak bırakılmadan içerik üretilmesi,
- pinch sırasında focal world point drift'i,
- 1→2 ve 2→1 pointer geçişlerinde jump olmaması,
- gesture sonunda ek görsel sıçrama olmaması,
- portrait/landscape resize sonrası viewport doğruluğu,
- p50/p95 frame süresi ve jank,
- büyük sahnede bellek ve render latency.

### Parser / DWG / DXF

- provenance-kayıtlı fixture,
- pozitif ve kontrollü negatif girdiler,
- unsupported/eksik kaynakların sessiz kaybolmaması,
- büyük koordinatlarda `double` hassasiyet,
- corrupt/resource-limit girdilerinde kontrollü failure.

### Stil / text / hatch / block / layout / XREF

Değiştirilen özelliğin host testini çalıştır. Android UI/render zinciri de etkileniyorsa ilgili hedefli Android gate'i ek olarak çalıştır. Eski PASS marker'ı yeni değişikliğin kanıtı değildir.

### Performans / büyük çizim

- p50/p95 gibi dağılım metriklerini kullan,
- peak memory/PSS ve repeat-open/close davranışını izle,
- optimizasyon correctness'i değiştirmemeli,
- ölçülmemiş bottleneck için karmaşıklık ekleme.

## Self-hosted runner

Geçerli runner etiketleri:

- `self-hosted`
- `windows`
- `android-test`
- `mobil-dwg`

Runner kapalıysa queued koşu PASS sayılmaz. Güvenilmeyen üçüncü taraf ref/PR self-hosted runner üzerinde çalıştırılmaz.

## Fiziksel Android

Emulator entegrasyon için güçlü bir gate'tir fakat şu alanlarda fiziksel cihazın yerini tutmaz:

- gerçek touch sampling ve gesture hissi,
- üretici GPU/driver farkları,
- SAF/content-provider farkları,
- termal throttling ve gerçek bellek baskısı,
- background/process death,
- düşük/orta/yüksek cihaz performansı.

Fiziksel cihaz sınıfları ve benchmark kayıt formatı `docs/DEVICE_MATRIX.md` içindedir.

## Kanıt kaydı

Yeni bir ayrı evidence MD dosyası oluşturmak varsayılan değildir. Önemli bir regression/release doğrulamasında en az şu bilgiler PR/commit açıklaması veya CI artifact/log'unda bulunmalıdır:

- exact SHA,
- test ortamı,
- kullanılan komut/gate,
- PASS/FAIL sonucu,
- gerekiyorsa screenshot/log/metric,
- claim sınırı,
- bilinen açık risk.

Tamamlanmış eski ayrıntılı Stage/V evidence dosyaları çalışma ağacından kaldırılmıştır; gerektiğinde Git geçmişinden okunabilir.
