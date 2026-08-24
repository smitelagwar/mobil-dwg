# AŞAMA 03 Evidence — test corpus, golden sözleşmesi ve cihaz matrisi

Tarih: 2026-08-24

Durum: `DONE`

AŞAMA 01 dış cihaz kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır. AŞAMA 03'te gerçek cihaz varmış gibi kayıt üretilmedi.

## Tamamlanan yapı

- [x] `fixtures/manifest/schema.json`
- [x] `fixtures/manifest/stage03-mini.json`
- [x] `fixtures/manifest/stage03-source-integrity.json`
- [x] `fixtures/README.md`
- [x] `fixtures/public/synthetic/NOTICE.md`
- [x] üç küçük sentetik DXF fixture
- [x] `docs/GOLDEN_CONTRACT.md`
- [x] `docs/DEVICE_MATRIX.md`
- [x] `scripts/stage03-validate-fixtures.py`
- [x] `scripts/stage03-verify-integrity.py`
- [x] `.github/workflows/stage03-corpus-audit.yml`

## Mini corpus

Pinned public remote set, kaynak `DomCR/ACadSharp` immutable revision `592d70a7bf0eaffbd932d23900f289b4e6305832`:

- DWG R2000 / AC1015
- DWG R2004 / AC1018
- DWG R2010 / AC1024
- DWG R2018 / AC1032
- ASCII DXF R2000 / AC1015
- ASCII DXF R2018 / AC1032

Upstream binary'ler mobil-dwg reposuna vendored edilmez. Root MIT lisansı doğrulandı; sample-specific istisna görülmedi. Buna rağmen binary fixture authorship ayrımı nedeniyle muhafazakâr `remote-reference-only` politikası uygulanır.

Committed sentetik set:

- Türkçe metin + basic geometry + nested block
- missing-font negative
- missing-XREF negative

CI-derived negative set:

- truncated DWG
- deterministic byte-corrupt DWG

## Hash/provenance modeli

- Upstream remote fixture'lar immutable source revision + Git blob SHA1 ile kilitlidir.
- CI artifact'ından gözlenen SHA-256 değerleri `fixtures/manifest/stage03-source-integrity.json` içine commit edildi.
- Final CI hem Git blob SHA1 hem SHA-256 doğrulaması yapar.
- Committed sentetik fixture'lar SHA-256 ile doğrulanır.
- Private fixture yolları Git ignored olmak zorundadır; validator tracked private CAD görürse FAIL verir.

## Upstream semantik kanıt

ACadSharp `samples/sample_base/save_samples.lsp` aynı base drawing'i farklı DWG/DXF sürümlerine SAVEAS eder. `sample_base_tree.json` içinde en az 8 Hatch, 13 BlockReference, 12 Dimension türü ve 3 paper-space block record gözlendi. Bunlar mobil-dwg parser sonucu değildir; yalnız fixture feature/provenance kanıtıdır.

## Golden sözleşmesi

- Semantik manifest aktif golden kaynağıdır.
- Sentetik fixture count'ları bağımsız olarak elle tanımlanmıştır.
- Aynı parser çıktısından aynı parser için golden türetilmez.
- Image golden renderer kurulana ve redistribution hakkı doğrulanana kadar oluşturulmaz.
- Her image golden için kaynak fixture, render ayarı, checksum ve redistribution durumu kaydedilmek zorundadır.

## Cihaz matrisi

`docs/DEVICE_MATRIX.md` Android/iOS fiziksel slotlarını ve P-SMALL/P-MEDIUM/P-LARGE/P-ADVERSARIAL provisional benchmark profillerini tanımlar. Gerçek cihaz modelleri kullanıcı erişimi olmadığı için `UNKNOWN / DEFERRED_EXTERNAL_GATE` kalır; bu AŞAMA 03'ün corpus/contract çıkış kriterini engellemez.

## Final Stage 03 CI

Workflow: `Stage 03 Corpus Audit`

Final PR-head run:

- Run ID: `32752374980` / #4
- Head: `bcc2f32c31e7c6d26d154d3e308bf662c41f34e6`
- Sonuç: `SUCCESS`
- Manifest JSON / Python syntax: PASS
- Corpus access, size, hash, provenance, magic/version: PASS
- Coverage ve private-ignore: PASS
- Derived truncated/corrupt fixture üretimi: PASS
- Dual hash doğrulaması: `STAGE03_DUAL_HASH_PASS fixtures=6`
- Fixture audit: `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`
- Evidence artifact ID: `9529508675`
- Artifact digest: `sha256:fd3990d7a3271c015a2f7067a856d5a23434f1ec0449ecff7819b569938e02cf`

## Regresyon kapıları

Aynı final PR head üzerinde:

- `Stage 02 Dependency Audit` run `32752375058` / #15: `SUCCESS`; artifact `9529546355`, digest `sha256:c528be8af15d8089da3bdc60feccd2ede404d8dfa2015630a3218d1190e49642`.
- `Stage 01 Toolchain Smoke` run `32752374956` / #34: `SUCCESS`; Debug/Release/manifest/artifact PASS; artifact `9529753917`, digest `sha256:6067ccf1cc6e696a100e110b164cfafb5da614779f8315cfce8670e6fdda9a3e`.

Stage 01'in bu CI sonucu fiziksel telefon kanıtı değildir; dış kapılar açık kalır.

## Merge

PR #5: `test: establish Stage 03 corpus contract`

- Doğrulanmış head: `bcc2f32c31e7c6d26d154d3e308bf662c41f34e6`
- Base: `main`
- Merge sonucu: başarılı
- Merge commit: `fb2d0982efeab8f78bc78dc82a7a8deb688190f8`

## Çıkış değerlendirmesi

AŞAMA 03 çıkış kriterleri sağlandı:

- en az 4 DWG / 2 DXF ve çoklu version family mevcut;
- basic geometry, Türkçe metin, nested block, dimension, hatch ve paper-space/layout feature coverage manifestte mevcut;
- corrupt/truncated, missing-font ve missing-XREF negatifleri tanımlı;
- hash/provenance validator gerçek bytes üzerinde PASS;
- public/synthetic/private ayrımı ve private Git-ignore guard var;
- golden redistribution sözleşmesi var;
- cihaz matrisi ve provisional benchmark profilleri var.

AŞAMA 03 `DONE`. Sonraki bağımsız çalışma aşaması AŞAMA 04'tür; aynı kullanıcı turunda AŞAMA 04 başlatılmaz.
