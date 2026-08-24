# Golden ve semantik fixture sözleşmesi

Tarih: 2026-08-24  
Aşama: AŞAMA 03

Amaç, parser/renderer doğruluğunu aynı parser çıktısını kendisiyle kıyaslayan dairesel testlerden ayırmaktır.

## 1. Golden kaynak hiyerarşisi

1. **Fixture manifest beklentisi**: dosyanın bağımsız provenance'ı, format/version'ı, feature kapsamı, beklenen minimum/exact entity count'ları ve warning sözleşmesi.
2. **Bağımsız sentetik bilgi**: mobil-dwg tarafından oluşturulan küçük fixture'larda entity sayısı dosyanın yazım sözleşmesinden gelir.
3. **Pinned upstream referans**: ACadSharp sample set için upstream `sample_base_tree.json` yalnız bağımsız reference evidence'dır. Stage 05'te mobil-dwg parser çıktısı bununla karşılaştırılabilir; parser çıktısından yeni golden üretilip aynı run'da PASS verilmez.
4. **Görüntü goldeni**: renderer kurulmadan görüntü goldeni oluşturulmaz. İlk image golden ancak deterministic viewport/theme/font ayarları ve redistribution hakkı ayrı kaydedildiğinde kabul edilir.

## 2. Count sözleşmesi

`expected.entity_counts.mode`:

- `exact`: sentetik fixture'ın elle bilinen entity sözleşmesi.
- `minimum`: upstream/version dönüşümleri nedeniyle yalnız korunması gereken alt sınırlar.
- Gelecekte `exact-by-independent-reference` eklenebilir; kaynak ve üretim yöntemi manifestte yazılmalıdır.

Count'ın kaynağı test edilen parser'ın aynı document object'i olamaz. Stage 05, parser sonucu ile manifest expectation arasındaki farkı diagnostics olarak raporlar.

## 3. Warning sözleşmesi

Warning sayısına kör eşik uygulanmaz. Manifest üç kategori kullanır:

- `must_include`: kontrollü negatif fixture'ın üretmesi gereken semantik uyarı.
- `must_not_include`: corruption/fatal gibi pozitif fixture'da kabul edilmeyen kategori.
- `may_include`: proxy, unsupported object veya eksik dış kaynak gibi dosyaya bağlı kabul edilebilir kategori.

Parser-specific notification adları Stage 05 adapter'ında bu semantik kategorilere map edilir.

## 4. Görüntü golden politikası

Image golden repo'ya ancak şu alanlar kanıtlıysa girebilir:

- kaynak fixture redistribution izni,
- image'ın kim tarafından/nasıl üretildiği,
- renderer revision ve deterministic render ayarları,
- font/SHX/raster/XREF dış asset hakları,
- `golden.redistribution = permitted`.

Hak durumu `review-required`, `forbidden` veya belirsiz ise görüntü yalnız private/local evidence olarak kalır ve Git'e girmez.

AŞAMA 03'te renderer olmadığı için tüm image golden durumları `not-created`; semantik manifest golden'ı aktiftir.

## 5. Negatif fixture sözleşmesi

- `derived-truncated-ac1015-dwg`: pinned pozitif DWG'nin ilk 4096 byte'ı; controlled failure beklenir.
- `derived-corrupt-ac1018-dwg`: pinned pozitif DWG'de payload byte mutation; controlled failure veya corruption/checksum warning beklenir.
- `negative_missing_font_ac1015.dxf`: açılabilir dosya + missing-font compatibility warning beklenir.
- `negative_missing_xref_ac1015.dxf`: açılabilir dosya + missing-XREF warning beklenir.

Negatiflerin parser-level sonucu AŞAMA 05'te doğrulanacaktır; AŞAMA 03 yalnız fixture'ın deterministik üretim/erişim/hash/provenance kapısını doğrular.
