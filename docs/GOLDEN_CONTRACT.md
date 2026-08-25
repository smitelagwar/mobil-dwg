# Golden ve semantik fixture sözleşmesi

Tarih: 2026-08-25  
Aktif validation: V03

Amaç, parser/renderer doğruluğunu aynı parser/writer çıktısını kendisiyle kıyaslayan dairesel testlerden ayırmak ve Android smoke girdilerinin hak/provenance sınırını açık tutmaktır.

## 1. Golden kaynak hiyerarşisi

1. **Fixture manifest beklentisi:** dosyanın bağımsız provenance'ı, format/version'ı, feature kapsamı, beklenen minimum/exact entity count'ları ve warning sözleşmesi.
2. **Bağımsız sentetik bilgi:** mobil-dwg tarafından elle oluşturulan küçük DXF fixture'larda entity sözleşmesi doğrudan authoring içeriğinden gelir.
3. **Pinned upstream referans:** ACadSharp sample set için upstream `sample_base_tree.json` yalnız bağımsız reference evidence'dır. mobil-dwg parser çıktısından yeni golden üretilip aynı parser'a doğruluk kanıtı yapılmaz.
4. **Generated smoke artifact:** sentetik DXF'den araçla türetilen DWG, yalnız format/open-path smoke girdisidir. Writer + reader round-trip kendi başına engineering fidelity goldeni değildir.
5. **Görüntü goldeni:** renderer kurulmadan görüntü goldeni oluşturulmaz. İlk image golden ancak deterministic viewport/theme/font ayarları ve redistribution hakkı ayrı kaydedildiğinde kabul edilir.

## 2. Count sözleşmesi

`expected.entity_counts.mode`:

- `exact`: sentetik fixture'ın elle bilinen entity sözleşmesi.
- `minimum`: upstream/version dönüşümleri nedeniyle yalnız korunması gereken alt sınırlar.
- Gelecekte `exact-by-independent-reference` eklenebilir; kaynak ve üretim yöntemi manifestte yazılmalıdır.

Count'ın kaynağı test edilen parser'ın aynı document object'i olamaz. Parser sonucu ile manifest expectation arasındaki fark diagnostics olarak raporlanır.

Generated `synthetic-turkish-basic-ac1015-dwg` için bağımsız semantik kaynak, committed `synthetic-turkish-basic-ac1015` DXF authoring sözleşmesidir. ACadSharp writer/read-back yalnız generated artifact'in yapısal smoke geçerliliğini kanıtlar; dönüştürülmüş DWG bytes veya writer'ın kendi document modeli independent golden sayılmaz.

## 3. Warning sözleşmesi

Warning sayısına kör eşik uygulanmaz. Manifest üç kategori kullanır:

- `must_include`: kontrollü negatif fixture'ın üretmesi gereken semantik uyarı.
- `must_not_include`: corruption/fatal gibi pozitif fixture'da kabul edilmeyen kategori.
- `may_include`: proxy, unsupported object veya eksik dış kaynak gibi dosyaya bağlı kabul edilebilir kategori.

Parser-specific notification adları adapter katmanında bu semantik kategorilere map edilir.

## 4. Android smoke seti

Manifestteki `android_smoke_set`, yalnız hak durumu açık ve yeniden üretilebilir girdileri içerir:

- committed 0BSD DXF `synthetic-turkish-basic-ac1015`;
- exact ACadSharp `3.7.1` generator ile validation sırasında üretilen 0BSD DWG `synthetic-turkish-basic-ac1015-dwg`;
- committed missing-font ve missing-XREF negatif DXF'leri.

Generated DWG için zorunlu minimum kanıt:

- source fixture ID ve rights profile;
- exact generator script;
- exact ACadSharp `3.7.1` dependency;
- `AC1015` DWG magic;
- aynı run'da `DwgReader` read-back;
- run-specific byte count ve SHA-256 artifact evidence.

Generated DWG binary repo'ya golden olarak commit edilmez. DWG container metadata'sının byte-for-byte deterministik olduğu ayrıca kanıtlanmadıkça hash eşitliği yeniden üretilebilirlik kriteri yapılmaz. Daha sonraki Android testinde gereken DWG aynı pinned generator sözleşmesiyle yeniden üretilir.

Remote ACadSharp binary sample'ları immutable revision/hash ile test corpus'unda kullanılabilir; ancak `remote-reference-only` hak politikası nedeniyle mobil-dwg'nin redistributable Android smoke bundle'ı sayılmaz.

## 5. Görüntü golden politikası

Image golden repo'ya ancak şu alanlar kanıtlıysa girebilir:

- kaynak fixture redistribution izni,
- image'ın kim tarafından/nasıl üretildiği,
- renderer revision ve deterministic render ayarları,
- font/SHX/raster/XREF dış asset hakları,
- `golden.redistribution = permitted`.

Hak durumu `review-required`, `forbidden` veya belirsiz ise görüntü yalnız private/local evidence olarak kalır ve Git'e girmez.

V03'te gerçek renderer olmadığı için image golden durumları `not-created`; semantik manifest golden'ı aktiftir.

## 6. Negatif fixture sözleşmesi

- `derived-truncated-ac1015-dwg`: pinned pozitif DWG'nin ilk 4096 byte'ı; controlled failure beklenir.
- `derived-corrupt-ac1018-dwg`: pinned pozitif DWG'de payload byte mutation; controlled failure veya corruption/checksum warning beklenir.
- `negative_missing_font_ac1015.dxf`: açılabilir dosya + missing-font compatibility warning beklenir.
- `negative_missing_xref_ac1015.dxf`: açılabilir dosya + missing-XREF warning beklenir.

V03 yalnız fixture'ın deterministik erişim/hash/provenance/redistribution kapısını doğrular. Parser-level negatif sonuçlar ilgili parser/runtime validation aşamasında ayrıca kanıtlanır.

## 7. Byte-stability kuralı

CAD fixture bytes kanıtın parçasıdır. `.gitattributes` ile `*.dxf -text` ve `*.dwg binary` uygulanır; Windows/Linux checkout line-ending dönüşümü fixture hash'ini değiştiremez. Manifest hash'i checkout platformuna göre yeniden yazılmaz.
