# AŞAMA 03 Evidence — test corpus, golden sözleşmesi ve cihaz matrisi

Tarih: 2026-08-24

Durum: `IN_PROGRESS`

AŞAMA 01 dış cihaz kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır. AŞAMA 03'te gerçek cihaz varmış gibi kayıt üretilmez.

## Hazırlanan yapı

- `fixtures/manifest/schema.json`
- `fixtures/manifest/stage03-mini.json`
- `fixtures/README.md`
- `fixtures/public/synthetic/NOTICE.md`
- üç küçük sentetik DXF fixture
- `docs/GOLDEN_CONTRACT.md`
- `docs/DEVICE_MATRIX.md`
- `scripts/stage03-validate-fixtures.py`
- `.github/workflows/stage03-corpus-audit.yml`

## Mini corpus tasarımı

Pinned public remote set:

- DWG R2000 / AC1015
- DWG R2004 / AC1018
- DWG R2010 / AC1024
- DWG R2018 / AC1032
- ASCII DXF R2000 / AC1015
- ASCII DXF R2018 / AC1032

Kaynak: `DomCR/ACadSharp` immutable revision `592d70a7bf0eaffbd932d23900f289b4e6305832`.

Bu upstream binary'ler mobil-dwg reposuna kopyalanmaz. Root MIT lisansı doğrulanmış ve sample-specific exception görülmemiş olsa da binary fixture'ların ayrı authorship bildirimi olmadığı için daha muhafazakâr `remote-reference-only` politikası uygulanır.

Sentetik committed set:

- Turkish text + basic geometry + nested block
- missing-font negative
- missing-XREF negative

CI-derived negative set:

- truncated DWG
- deterministic byte-corrupt DWG

## Upstream semantic evidence

ACadSharp `samples/sample_base/save_samples.lsp` aynı base drawing'i farklı DWG/DXF sürümlerine SAVEAS eder. `sample_base_tree.json` içinde en az:

- 8 Hatch,
- 13 BlockReference,
- 12 Dimension türü,
- 3 paper-space block record

gözlendi. Bunlar mobil-dwg parser sonucu değildir; yalnız fixture feature provenance kanıtıdır.

## Golden ilkeleri

- semantik manifest aktif golden kaynağıdır;
- sentetik fixture count'ları bağımsız olarak elle bilinmektedir;
- parser çıktısından aynı parser için golden türetilmez;
- image golden renderer kurulana ve redistribution hakkı doğrulanana kadar oluşturulmaz.

## Cihaz matrisi

`docs/DEVICE_MATRIX.md` fiziksel slotları ve P-SMALL/P-MEDIUM/P-LARGE/P-ADVERSARIAL ölçüm profillerini tanımlar. Gerçek cihaz modelleri kullanıcı erişimi olmadığı için `UNKNOWN / DEFERRED_EXTERNAL_GATE` kalır.

## Kapanış için beklenen kanıt

GitHub Actions `Stage 03 Corpus Audit` aşağıdakileri PASS etmelidir:

- manifest/schema structural policy,
- private fixture ignore,
- pinned remote download erişimi,
- size + Git blob SHA1 doğrulaması,
- committed synthetic SHA-256 doğrulaması,
- DWG/DXF magic/version smoke,
- required feature/version coverage,
- deterministic truncated/corrupt fixture üretimi,
- evidence artifact upload.

PASS olmadan AŞAMA 03 `DONE` yapılmaz.
