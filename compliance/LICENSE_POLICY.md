# mobil-dwg — Dependency ve Asset Lisans Politikası

Bu politika aktif teknik/ürün kabul kuralıdır; eski tamamlanmış uygulama planlarına bağlı değildir. Hukuki görüş değildir.

## GREEN — varsayılan allowlist

Exact artifact/source/provenance ayrıca doğrulanmak şartıyla varsayılan kabul edilebilir SPDX lisansları:

- `MIT`
- `Apache-2.0`
- `BSD-2-Clause`
- `BSD-3-Clause`
- `ISC`
- `0BSD`

GREEN yalnız lisans adı değildir. Exact package version, transitive graph, source revision, native binary/asset provenance ve notice yükümlülüğü bilinmelidir.

## REVIEW — otomatik production kabulü yok

Örnekler:

- LGPL/MPL gibi özel değerlendirme isteyen lisanslar,
- source-pinned fork/submodule graph'ı,
- native binary envanteri tamamlanmamış package,
- package/source lineage eşleşmesi kanıtlanmamış dependency,
- eski/az kullanılan test-only aday,
- yayınlanmamış source-only dependency.

REVIEW runtime graph'a kendiliğinden girmez. Ayrı evidence/ADR gerekir.

## RED — runtime/release blocker

- GPL
- AGPL
- SSPL
- BUSL
- non-commercial lisans
- OSI-permissive olmayan source-available lisans
- proprietary/trial/ücretli CAD SDK veya zorunlu runtime servis lisansı
- lisansı/provenance'ı bilinmeyen package/native binary/font/asset

Autodesk RealDWG, APS/Forge conversion, ticari ODA SDK ve ücretli/trial CAD parser-renderer ürün kapsamı gereği production runtime için yasaktır.

## Zorunlu kanıt alanları

Runtime dependency veya redistributable asset için gerektiğinde:

1. exact package/version veya source commit,
2. resolved transitive dependency graph,
3. package/source artifact hash,
4. license expression/file,
5. source repo/tag/commit,
6. submodule/fork ve upstream farkı,
7. native `.so`, `.aar`, `.jar`, `.framework`, `.dylib` envanteri,
8. font/icon/PAT/fixture/embedded resource provenance,
9. redistribution/notice/source-disclosure/royalty değerlendirmesi,
10. sonuç: `GREEN`, `REVIEW` veya `RED`.

## Paket kilitleme politikası

- Central Package Management kullanılır.
- Direct dependency sürümleri exact'tir; floating/`latest` kullanılmaz.
- Restore lockfile korunur; CI mümkün olduğunda locked mode doğrular.
- Dependency yükseltmesi otomatik değildir.
- Repo lisansı tek başına yeterli değildir; yayımlanan package ve native asset graph'ı ayrıca incelenir.

## Source ve veri firewall'u

- RED/rejected lisanslı kaynaktan kod veya satır-satır port alınmaz.
- Açık internette bulunan DWG/DXF/font/screenshot yeniden dağıtılabilir varsayılmaz.
- Kullanıcı/müşteri CAD dosyaları private kalır ve Git dışında tutulur.
- Proprietary SHX/font dosyaları bundle edilmez.
- Yeni binary/native asset final artifact'e giriyorsa release öncesi gerçek artifact inventory yapılır.

## Açık kaynak referanslardan öğrenme

Başka viewer/CAD projelerinden algoritma fikri, mimari desen, davranış ve performans yaklaşımı incelenebilir. Ancak:

- lisans policy ile uyumsuz kaynaktan kod kopyalanmaz,
- satır-satır port yapılmaz,
- özgün implementasyon yazılır,
- gerekiyorsa kaynak/ilham evidence veya ADR'de belirtilir.

## Değişiklik kuralı

Yeni dependency/asset eklenmesi veya mevcut dependency sürüm değişikliği aynı PR/değişiklik içinde compliance etkisiyle birlikte değerlendirilir. Kanıt tamamlanmadan runtime graph'a kalıcı kabul yapılmaz.
