# mobil-dwg — Test Fixtures

Bu klasör test corpus'unun yeniden dağıtım açısından açıkça sınıflandırılmış bölümünü ve immutable remote-reference sözleşmesini tutar. Fixture kayıtları aktif bir geliştirme aşaması/cursor değildir; parser/render regresyonlarında tekrar kullanılabilir.

## Klasör politikası

- `fixtures/manifest/`: fixture kimliği, format/version, boyut, hash, provenance/hak bilgisi, feature kapsamı ve beklenen sonuç sözleşmesi.
- `fixtures/public/synthetic/`: mobil-dwg tarafından oluşturulmuş ve ayrı lisans notuyla commit edilmesine izin verilen küçük sentetik CAD fixture'ları.
- `fixtures/private/`: müşteri/kullanıcı/özel test çizimleri. Git tarafından ignore edilir ve repoya giremez.
- Upstream public CAD örnekleri varsayılan olarak repoya kopyalanmaz. Manifest gerektiğinde immutable upstream revision + path + hash ile referans verir.

Bir dosyanın internette erişilebilir olması yeniden dağıtım izni anlamına gelmez. Lisans/redistribution durumu çözülmemiş fixture redistributable smoke setine alınmaz.

CAD fixture bytes kanıtın parçasıdır. `.gitattributes` CAD dosyalarında platform kaynaklı byte değişimini önleyecek şekilde korunur.

## Mini corpus

Tekrar kullanılabilir pozitif referans seti:

- pinned ACadSharp DWG örnekleri: R2000, R2004, R2010, R2018,
- pinned ACadSharp ASCII DXF örnekleri: R2000, R2018,
- committed mobil-dwg sentetik R2000 DXF: basic geometry + Türkçe metin + nested block.

Kontrollü negatifler:

- missing-font DXF,
- missing-XREF DXF,
- gerektiğinde pinned DWG'den üretilen truncated DWG,
- gerektiğinde deterministic byte-corruption DWG.

Remote ACadSharp fixture'ları mobil-dwg içine vendored değildir ve `remote-reference-only` kalır. Upstream semantic bilgi provenance/reference olabilir; mobil-dwg parser çıktısının kendisi bağımsız golden sayılmaz.

## Redistributable Android smoke seti

`fixtures/manifest/corpus.json` içindeki `android_smoke_set` Android/parser smoke testlerinde kullanılır:

- `synthetic-turkish-basic-ac1015`: committed 0BSD DXF,
- `synthetic-turkish-basic-ac1015-dwg`: sentetik DXF'den exact ACadSharp 3.7.1 generator sözleşmesiyle test sırasında üretilebilen DWG,
- `negative-missing-font-ac1015`,
- `negative-missing-xref-ac1015`.

Generated DWG binary bağımsız engineering-fidelity goldeni değildir. Kullanılıyorsa generator/version, format magic/read-back, byte count ve SHA-256 test evidence'ına yazılır.

## Validator

```bash
python scripts/validate-fixtures.py \
  --manifest fixtures/manifest/corpus.json \
  --evidence artifacts/fixture-audit.json
```

Committed fixture hash'lerini ayrıca doğrulamak için:

```bash
python scripts/verify-fixture-integrity.py \
  --audit artifacts/fixture-audit.json \
  --integrity fixtures/manifest/source-integrity.json
```

Fixture/golden genel kuralları için `docs/GOLDEN_CONTRACT.md` esas alınır.
