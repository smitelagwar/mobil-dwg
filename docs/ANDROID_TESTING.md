# mobil-dwg — Android Test Rehberi

Bu dosya gelecekteki bug fix, performans ve viewer geliştirmelerinde kullanılacak tek Android test rehberidir. Eski V01–V09 doğrulama cursor'ları veya A10 paralel çalışma kuralları artık aktif değildir.

## Temel kural

Bir değişiklik ancak **değiştirilen davranışı gerçekten ölçen** test ile doğrulanmış sayılır. Eski bir stage marker'ının PASS olması yeni davranışın doğru olduğunu kanıtlamaz.

- Emulator sonucu fiziksel cihaz sonucu değildir.
- Host build sonucu Android runtime sonucu değildir.
- Ekranın açılması render doğruluğu değildir.
- Canvas/image varlığı doğru pan/zoom, doğru metin veya eksiksiz CAD görünümü değildir.
- `queued`, zero-step, runner-offline veya artifact'siz run PASS değildir.
- Test edilen exact commit SHA kaydedilir.

## Yerel ortam

Repo baseline'ı `docs/TOOLCHAIN.md` ve `global.json` ile tanımlanır.

Windows test makinesinde kullanılan temel bileşenler:

- self-hosted runner: `C:\actions-runner`
- interaktif listener: `C:\actions-runner\run.cmd`
- Android AVD: `mobil-dwg-api36`
- hedef emulator: Android API 36

Emulator grafik/masaüstü oturumuna ihtiyaç duyduğundan runner'ın interaktif kullanıcı oturumunda çalışması tercih edilir.

## Temel komutlar

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

Repo içinde geçmiş geliştirme aşamalarına ait hedefli Android gate scriptleri `scripts/aXX-android-*-gate.ps1` biçiminde korunur. Bunlar regresyon aracı olarak kullanılabilir; yeni bir özellik veya yeni bir bug fix eski scriptin kapsamı dışındaysa ilgili script genişletilir veya yeni odaklı test eklenir.

Özellikle viewer hareketleri için tarihsel başlangıç noktası:

```text
scripts/a11-android-gesture-gate.ps1
```

Bu scriptin geçmişte PASS olması gelecekte değiştirilen gesture/render zincirini otomatik olarak doğrulamaz.

## Değişiklik türüne göre zorunlu doğrulama

### Kamera / pan / pinch / zoom / render scheduling

En az şunlar ölçülür:

- tek parmak pan sırasında kamera ile görüntünün aynı frame zincirinde hareket etmesi,
- yeni görünür alanda parmak bırakılmadan içerik üretilmesi,
- iki parmak pinch sırasında focal world point drift'inin ölçülmesi,
- parmak sayısı 1→2 ve 2→1 değişirken jump olmaması,
- gesture sonunda ek bir görsel sıçrama olmaması,
- portrait/landscape resize sonrası viewport doğruluğu,
- p50/p95 frame süresi ve dropped-frame/jank gözlemi,
- büyük sahnede bellek ve render latency.

### Parser / DWG / DXF

- sentetik/provenance-kayıtlı fixture,
- pozitif ve kontrollü negatif girdiler,
- unsupported/eksik kaynakların sessiz kaybolmaması,
- büyük koordinatlarda `double` hassasiyet,
- corrupt/resource-limit girdilerinde kontrollü failure.

### Stil / text / hatch / block / layout / XREF

Değiştirilen özellik için ilgili `scripts/a12`–`a19` gate'leri ve `docs/evidence/` içindeki geçmiş semantik beklentiler regresyon referansı olarak kullanılır.

### Performans / büyük çizim

- `scripts/a20-android-perf-memory-gate.ps1` ve `scripts/a21-android-corpus-regression-gate.ps1` başlangıç noktasıdır,
- yalnız ortalama süreye bakılmaz; p50/p95, peak memory/PSS ve repeat-open/close davranışı kaydedilir,
- optimizasyon correctness'i değiştirmemelidir.

## Self-hosted runner kullanımı

Runner etiketleri tarihsel olarak:

- `self-hosted`
- `windows`
- `android-test`
- `mobil-dwg`

Runner kapalıysa aynı testi tekrar tekrar kuyruğa sokma. Test edilecek exact SHA'yı kaydet ve makine hazır olduğunda yalnız hâlâ gerekli olan en güncel koşuyu çalıştır.

Self-hosted runner üzerinde güvenilmeyen üçüncü taraf ref/PR çalıştırılmaz.

## Fiziksel Android

Emulator API/uygulama entegrasyonu için güçlü bir gate'tir fakat şu alanlarda fiziksel cihazın yerini tutmaz:

- üreticiye özgü SAF/content-provider davranışı,
- gerçek touch sampling ve gesture hissi,
- GPU/driver farkları,
- termal throttling ve gerçek bellek baskısı,
- background/process death,
- düşük/orta/yüksek cihaz performans sınıfları.

Fiziksel cihaz testi gerektiğinde sonuç `docs/DEVICE_MATRIX.md` içindeki uygun slota eklenir.

## Kanıt kaydı

Önemli bir regression fix veya release etkili değişiklik için `docs/EVIDENCE_TEMPLATE.md` kullanılır. En az:

- exact SHA,
- test ortamı,
- kullanılan komut/script,
- PASS/FAIL çıktısı,
- screenshot/log/metric gerekiyorsa artifact,
- claim sınırı,
- bilinen açık risk

kaydedilir.

## Tarihsel test kayıtları

Tamamlanmış eski uygulama ve doğrulama sonuçları `docs/evidence/` altında korunur. Bunlar değiştirilmez; yeni bir hata bulunursa yeni commit ve yeni kanıt üretilir.
