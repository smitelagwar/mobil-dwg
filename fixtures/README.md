# Test fixtures

Bu klasör mobil-dwg test corpus'unun yalnız yeniden dağıtım açısından açıkça sınıflandırılmış bölümünü tutar.

## Klasör politikası

- `fixtures/manifest/`: fixture kimliği, format/version, boyut, içerik hash'i, provenance/hak bilgisi, feature kapsamı ve beklenen sonuç sözleşmesi.
- `fixtures/public/synthetic/`: mobil-dwg tarafından üretilmiş ve ayrı lisans notuyla commit edilmesine izin verilen küçük sentetik CAD fixture'ları.
- `fixtures/private/`: müşteri/kullanıcı/özel test çizimleri. Git tarafından ignore edilir ve repoya giremez.
- Upstream public CAD örnekleri varsayılan olarak bu repoya kopyalanmaz. Manifest, immutable upstream revision + path + hash ile referans verir; CI geçici cache'e indirir.

Bir dosyanın internette erişilebilir olması yeniden dağıtım izni anlamına gelmez. İlgili `sources` veya `rights_profiles` kaydında lisans/redistribution durumu çözülmemişse fixture test corpus'una kabul edilmez.

## AŞAMA 03 mini corpus

Pozitif çekirdek:

- 4 pinned ACadSharp DWG: R2000, R2004, R2010, R2018.
- 2 pinned ACadSharp ASCII DXF: R2000, R2018.
- 1 mobil-dwg sentetik R2000 DXF: basic geometry + Türkçe Unicode escape metni + nested block.

Kontrollü negatifler:

- commit edilmiş missing-font DXF,
- commit edilmiş missing-XREF DXF,
- CI'da pinned DWG'den üretilen deterministic truncated DWG,
- CI'da pinned DWG'den üretilen deterministic byte-corruption DWG.

Remote ACadSharp fixture'ları mobil-dwg içine vendored değildir. Upstream `samples/sample_base/save_samples.lsp`, aynı base drawing'in farklı DWG/DXF sürümlerine SAVEAS edildiğini kaydeder. Upstream semantic tree; hatch, block reference, dimension ve paper-space varlığını doğrulamak için provenance/evidence olarak kullanılır; mobil-dwg parser sonucunun kendisi golden sayılmaz.

Validator:

```bash
python scripts/stage03-validate-fixtures.py \
  --manifest fixtures/manifest/stage03-mini.json \
  --evidence artifacts/stage03-fixture-audit.json
```

Başarı token'ı: `STAGE03_FIXTURE_AUDIT_PASS`.
