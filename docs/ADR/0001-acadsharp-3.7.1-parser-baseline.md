# ADR 0001 — ACadSharp 3.7.1 parser baseline

- Durum: Accepted for read-only parser baseline
- Tarih: 2026-08-24
- Aşama: AŞAMA 05

## Bağlam

v1 viewer için DWG ve DXF doğrudan cihazda, read-only ve offline okunmalıdır. Parser UI/Core sınırına entity tiplerini sızdırmamalı; writer/save API'leri production akışına girmemelidir. AŞAMA 02'de ACadSharp `3.7.1` MIT lisans/dependency açısından `GREEN` aday olarak pinlendi, fakat fidelity/production parser kararı gerçek corpus'a bırakıldı.

AŞAMA 05 mini corpus'u şu parser ailelerini doğruladı:

- DWG: AC1015, AC1018, AC1024, AC1032
- ASCII DXF: AC1015, AC1032
- sentetik AC1015 DXF: Türkçe/basic/nested INSERT
- negatif AC1015 DXF: missing font ve missing XREF
- deterministic derived negatives: truncated AC1015 DWG ve byte-corrupt AC1018 DWG

Final candidate head: `09e26172aa8de9e8c79ae64853a493dab1d0e5b9`.

GitHub Actions `Stage 05 Parser Spike` run `32759096003` / #8 sonucu `SUCCESS`:

- committed `packages.lock.json` ile `--locked-mode` restore PASS
- Release build: `0 Warning(s)`, `0 Error(s)`
- `STAGE04_CORE_CONTRACT_TESTS_PASS`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `STAGE05_MINI_CORPUS_PASS fixtures=9 derived_negatives=2`
- `STAGE05_T3_PASS`

Evidence artifact: `9532001644`, digest `sha256:2750ba88141c5724306bb5811173d958c60836806021f2ff1a5b36b011631097`.

## Karar

ACadSharp `3.7.1`, v1 için **read-only DWG/DXF parser baseline olarak GO** kabul edilir.

Kapsam sınırları:

1. ACadSharp yalnız `MobilDwg.Cad` adapter katmanında PackageReference olarak bulunur.
2. `MobilDwg.Core`, `MobilDwg.Rendering` ve `MobilDwg.App` ACadSharp tiplerine doğrudan bağlanmaz.
3. Writer/save API'leri kullanılmaz.
4. Cancellation capability `BeforeStartOnly`; parser başladıktan sonra cooperative abort varmış gibi davranılmaz.
5. Progress capability `StagesOnly`; uydurma yüzde üretilmez.
6. Bu ADR yalnız parse yolu + diagnostics/compatibility + manifest semantiği içindir. Render/engineering fidelity kararı değildir.
7. Dependency kendiliğinden yükseltilmez. `3.7.1` dışındaki sürüm ayrı corpus/lisans/artifact kontrolü gerektirir.

## Kanıt özeti

Upstream ACadSharp fixture setinin DWG/DXF karşılıklarında manifestin kritik semantic beklentileri geçti. Dört DWG familyası ile iki ASCII DXF örneğinde toplam block entity sayısı `341`; gerekli LINE/CIRCLE/INSERT-BLOCK_REFERENCE/DIMENSION/HATCH semantiği bulundu. DWG ve karşılık DXF'lerde temel dağılım aynı kaldı.

Parse süreleri GitHub hosted Ubuntu runner'daki tek koşunun tanısal değerleridir; performans garantisi değildir:

- AC1015 DWG: 518.4 ms
- AC1018 DWG: 111.0 ms
- AC1024 DWG: 87.5 ms
- AC1032 DWG: 76.8 ms
- AC1015 ASCII DXF: 532.5 ms
- AC1032 ASCII DXF: 441.8 ms

Sentetik Türkçe/basic DXF `6.2 ms` civarında açıldı ve exact semantic count sözleşmesini geçti. Missing-font fixture `missing-font`, missing-XREF fixture `missing-xref` compatibility kodu üretti.

Derived truncated DWG kontrollü `EndOfStreamException` ile başarısız oldu. Derived corrupt DWG crash yerine warning ile açıldı; sonuç evidence'ta açıkça kayıtlıdır.

## Known failures / sınırlamalar

Bunlar parser baseline GO kararını bozmaz, fakat sonraki aşamalarda gizlenemez:

- Upstream fixtures ortamda SHX font kaynağı bulunmadığı için `missing-font` compatibility kaydı üretebilir. Bu, bundle edilmiş proprietary font olmadığı mevcut headless CI ortamının beklenen sonucudur.
- ASCII DXF örneklerinde ACadSharp notification hacmi yüksektir ve `unsupported-object` compatibility sınıfı görülebilir. Sabit warning-count eşiği kullanılmaz; kritik semantic count/golden beklentisi ayrı doğrulanır.
- Corrupt AC1018 türevi parser tarafından tamamen reddedilmek yerine warning ile açılabilir. AŞAMA 19 resource/corrupt guard'ları bu davranışı ayrıca sıkılaştıracaktır.
- AŞAMA 05 scene/render fidelity kanıtlamaz. Entity sınıfının parse edilmesi görsel veya engineering doğruluğu garanti etmez.
- Parser başladıktan sonra cooperative cancellation kanıtı yoktur.

## Alternatifler

- ACadSharp sürüm karşılaştırması: Bu corpus'ta kritik kayıp görülmediği için gerekli olmadı.
- IxMilia.Dxf fallback spike: Kritik DXF parse kaybı görülmediği için başlatılmadı ve runtime graph'a eklenmedi.
- ProCad: Bu ADR'nin konusu değildir; AŞAMA 07 source-pinned spike olarak ayrı değerlendirilir.

## Sonuçlar

AŞAMA 06 ve sonraki adapter tüketicileri `ICadDocumentReader` üzerinden ACadSharp `3.7.1` baseline'ını kullanabilir. Her yeni fixture kaybı parser gate'ini yeniden açabilir; özellikle unsupported/proxy notification ile semantic/golden kayıp birbirinden ayrılmalıdır.
