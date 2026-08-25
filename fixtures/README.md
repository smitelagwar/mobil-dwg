# Test fixtures

Bu klasör mobil-dwg test corpus'unun yalnız yeniden dağıtım açısından açıkça sınıflandırılmış bölümünü ve immutable remote referans sözleşmesini tutar.

## Klasör politikası

- `fixtures/manifest/`: fixture kimliği, format/version, boyut, içerik hash'i, provenance/hak bilgisi, feature kapsamı ve beklenen sonuç sözleşmesi.
- `fixtures/public/synthetic/`: mobil-dwg tarafından oluşturulmuş ve ayrı lisans notuyla commit edilmesine izin verilen küçük sentetik CAD fixture'ları.
- `fixtures/private/`: müşteri/kullanıcı/özel test çizimleri. Git tarafından ignore edilir ve repoya giremez.
- Upstream public CAD örnekleri varsayılan olarak bu repoya kopyalanmaz. Manifest, immutable upstream revision + path + hash ile referans verir; CI geçici cache'e indirir.

Bir dosyanın internette erişilebilir olması yeniden dağıtım izni anlamına gelmez. İlgili `sources` veya `rights_profiles` kaydında lisans/redistribution durumu çözülmemişse fixture redistributable smoke setine alınmaz.

CAD fixture bytes kanıtın parçasıdır. `.gitattributes` `*.dxf -text` ve `*.dwg binary` uygular; Windows checkout CRLF dönüşümü manifest hash'lerini değiştiremez.

## Mini corpus

Pozitif çekirdek:

- 4 pinned ACadSharp DWG: R2000, R2004, R2010, R2018.
- 2 pinned ACadSharp ASCII DXF: R2000, R2018.
- 1 committed mobil-dwg sentetik R2000 DXF: basic geometry + Türkçe Unicode escape metni + nested block.

Kontrollü negatifler:

- committed missing-font DXF,
- committed missing-XREF DXF,
- CI'da pinned DWG'den üretilen deterministic truncated DWG,
- CI'da pinned DWG'den üretilen deterministic byte-corruption DWG.

Remote ACadSharp fixture'ları mobil-dwg içine vendored değildir ve `remote-reference-only` kalır. Upstream semantic tree; hatch, block reference, dimension ve paper-space varlığını provenance/evidence olarak doğrular; mobil-dwg parser sonucu golden sayılmaz.

## Android redistributable smoke seti

`fixtures/manifest/stage03-mini.json` içindeki `android_smoke_set` daha sonraki V04–V09 Android doğrulamalarına küçük ve hak durumu açık girdiler sağlar:

- `synthetic-turkish-basic-ac1015`: committed 0BSD DXF;
- `synthetic-turkish-basic-ac1015-dwg`: yukarıdaki DXF'den `scripts/stage03-generate-synthetic-dwg.ps1` ile exact ACadSharp 3.7.1 kullanılarak validation-time üretilen DWG;
- `negative-missing-font-ac1015` ve `negative-missing-xref-ac1015`: committed negatif 0BSD DXF'ler.

Generated DWG binary golden olarak commit edilmez. Generator output'u `AC1015` magic ve `DwgReader` read-back ile doğrulanır; run-specific byte count/SHA-256 evidence artifact'ine yazılır. Bu DWG open-path smoke girdisidir; bağımsız DWG engineering-fidelity goldeni değildir.

## Validator

```bash
python scripts/stage03-validate-fixtures.py \
  --manifest fixtures/manifest/stage03-mini.json \
  --evidence artifacts/stage03-fixture-audit.json
```

Beklenen ana marker'lar:

- `V03_ANDROID_SMOKE_SET_PASS`
- `STAGE03_FIXTURE_AUDIT_PASS`
- `STAGE03_DUAL_HASH_PASS`
- CI kapanışında `ANDROID_VALIDATION_V03_PASS`
