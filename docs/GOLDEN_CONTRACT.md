# mobil-dwg — Golden ve Fixture Sözleşmesi

Bu belge parser/render doğruluğunda kullanılacak kalıcı test veri kurallarını tanımlar. Eski stage/validation cursor'larından bağımsızdır.

## 1. Golden kaynak hiyerarşisi

1. **Fixture manifest beklentisi:** provenance, format/version, feature kapsamı, beklenen entity/diagnostic sözleşmesi.
2. **Bağımsız sentetik bilgi:** repo tarafından kontrollü biçimde üretilmiş küçük DXF fixture'larda beklenti doğrudan authoring içeriğinden gelir.
3. **Pinned upstream reference:** yalnız exact revision/hash ve kullanım hakkı kayıtlıysa bağımsız referans olabilir.
4. **Generated smoke artifact:** bir araçla türetilen DWG/DXF yalnız format/open-path kanıtı olabilir; aynı writer+reader round-trip kendi başına engineering fidelity goldeni değildir.
5. **Görüntü goldeni:** yalnız deterministic viewport/theme/font/render ayarları ve redistribution hakkı kayıtlıysa kabul edilir.

Test edilen parser veya renderer kendi çıktısını üretip aynı çıktıyı oracle olarak kullanamaz.

## 2. Entity ve semantik beklentiler

Manifest beklentisi mümkün olduğunda:

- `exact`: elle bilinen sentetik fixture sözleşmesi,
- `minimum`: upstream/version farkları nedeniyle korunması gereken alt sınır,
- bağımsız kaynak varsa açıkça kaynağı belirtilmiş exact reference

olarak ifade edilir.

Count'ın veya semantik expectation'ın kaynağı test edilen parser'ın aynı document object'i olamaz.

## 3. Warning / diagnostic sözleşmesi

Salt warning sayısına kör eşik uygulanmaz. Semantik kategoriler tercih edilir:

- `must_include`
- `must_not_include`
- `may_include`

Eksik font, XREF, unsupported/proxy, corruption ve resource-limit durumları birbirine karıştırılmaz. Parser-specific notification adları adapter katmanında semantik kategorilere map edilir.

## 4. Public/synthetic smoke seti

Redistributable smoke girdileri `fixtures/` manifest/provenance kurallarına uymalıdır.

Mevcut temel sentetik set:

- `synthetic-turkish-basic-ac1015.dxf`
- `negative_missing_font_ac1015.dxf`
- `negative_missing_xref_ac1015.dxf`

Generated DWG kullanılıyorsa en az:

- source fixture ID,
- exact generator,
- exact dependency version,
- format magic/read-back,
- run-specific byte count ve SHA-256

kaydedilir.

Generated binary sırf writer output'u olduğu için kalıcı golden sayılmaz.

## 5. Görüntü golden politikası

Bir screenshot/image golden repo'ya ancak şu bilgiler kayıtlıysa girebilir:

- kaynak fixture redistribution izni,
- image'ın nasıl ve hangi revision ile üretildiği,
- viewport boyutu ve density,
- kamera/zoom/fit durumu,
- theme/background,
- font/SHX/raster/XREF dış asset provenance,
- antialias/render backend gibi sonucu etkileyen ayarlar.

Hak durumu belirsiz veya yasaksa image yalnız private/local evidence olarak kalır.

Pan/zoom gibi dinamik davranışlarda tek screenshot yeterli değildir; interaction sırasında frame davranışı ve focal/viewport doğruluğu ayrıca ölçülür.

## 6. Negatif fixture politikası

Negatif fixture kontrollü failure veya beklenen diagnostic üretmelidir. Amaç crash üretmek değil, hatanın sınıflandırılmış ve sınırlı şekilde ele alındığını kanıtlamaktır.

Örnek sınıflar:

- truncated/corrupt DWG/DXF,
- missing font,
- missing XREF,
- unsupported external reference,
- NaN/sonsuz/aşırı koordinat,
- resource-limit / decompression-bomb benzeri girdiler.

## 7. Byte-stability

CAD fixture bytes kanıtın parçasıdır.

- `.gitattributes` CAD binary/text davranışını bozmayacak şekilde korunur.
- Checkout line-ending dönüşümü fixture hash'ini değiştirmemelidir.
- Manifest hash'i platforma göre yeniden yazılmaz.

## 8. Özel/müşteri dosyaları

Gerçek kullanıcı/müşteri DWG-DXF dosyaları, lisansı belirsiz fontlar ve private corpus public repoya eklenmez. Gerçek dünya dosyasıyla yerel test yapılabilir ancak dosyanın kendisi veya tanımlayıcı metadata'sı public artifact'e taşınmaz.

## 9. Regresyon kuralı

Tarihsel golden/evidence yeni implementasyona göre sessizce değiştirilmez. Bir regression gerçekten davranış değişikliği gerektiriyorsa:

1. neden açıkça yazılır,
2. yeni expectation bağımsız olarak doğrulanır,
3. ilgili fixture/golden sözleşmesi aynı değişiklikte güncellenir,
4. eski sonuçların neden artık uygulanmadığı kaydedilir.
