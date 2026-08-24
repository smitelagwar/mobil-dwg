# mobil-dwg — Dependency ve Asset Lisans Politikası

Doğrulama tarihi: 2026-08-24

Bu politika `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` içindeki royalty-free ve düşük uyumluluk yükü hedefini uygulanabilir release kapılarına çevirir. Hukuki görüş değildir; teknik/ürün kabul politikasıdır.

## Sınıflandırma

### GREEN — varsayılan allowlist

Aşağıdaki SPDX lisansları, exact artifact/source/provenance doğrulaması ayrıca yapılmak şartıyla varsayılan olarak kabul edilebilir:

- `MIT`
- `Apache-2.0`
- `BSD-2-Clause`
- `BSD-3-Clause`
- `ISC`
- `0BSD`

GREEN yalnız lisans adı demek değildir. Exact package version, resolved transitive graph, source repository/revision, native binary/asset provenance ve notice yükümlülüğü bilinmelidir.

### REVIEW — otomatik production kabulü yok

Aşağıdakiler REVIEW olur:

- LGPL veya MPL gibi proje politikası gereği özel değerlendirme isteyen lisanslar,
- source-pinned fork/submodule graph'ı,
- native binary içeren paketlerde artifact inventory henüz tamamlanmamışsa,
- package ile source lineage eşleşmesi kanıtlanmamışsa,
- eski/az kullanılan/test-only adaylar,
- yayınlanmamış source-only dependency adayları.

REVIEW runtime graph'a kendiliğinden girmez. Ayrı evidence/ADR gerekir.

### RED — runtime/release blocker

Aşağıdakiler runtime graph ve final artifact için RED'dir:

- GPL
- AGPL
- SSPL
- BUSL
- non-commercial lisans
- source-available ama OSI-permissive olmayan lisans
- proprietary/trial/ücretli CAD SDK veya zorunlu runtime servis lisansı
- lisansı bilinmeyen package/native binary/font/asset

Autodesk RealDWG, APS/Forge conversion, ticari ODA SDK ve ücretli/trial CAD parser-renderer ürün kapsamı gereği ayrıca yasaktır.

## Zorunlu kanıt alanları

Runtime veya redistributable asset için en geç release gate'inde şunlar kayıtlı olmalıdır:

1. Exact package/version veya source commit.
2. Resolved transitive dependency graph.
3. Package/source artifact hash.
4. License expression/file ve mümkünse hash.
5. Source repo/tag/commit.
6. Submodule/fork ve upstream farkı.
7. Native `.so`, `.aar`, `.jar`, `.framework`, `.dylib` envanteri.
8. Font/icon/PAT/fixture/embedded resource provenance.
9. Redistribution/notice/source-disclosure/royalty değerlendirmesi.
10. Sonuç: `GREEN`, `REVIEW` veya `RED`.

## Paket kilitleme politikası

- Central Package Management kullanılır.
- Direct dependency sürümleri exact'tir; `*`, floating, `latest` veya açık alt sınır kullanılmaz.
- Restore lockfile zorunludur ve CI `--locked-mode` ile doğrular.
- Dependency yükseltmesi otomatik değildir; yeni sürüm ayrı doğruluk/lisans/native artifact denetimi ister.
- Bir dependency'nin repo lisansı tek başına yeterli değildir; yayımlanan NuGet ve native asset graph'ı ayrıca incelenir.

## Source ve veri firewall'u

- RED/rejected lisanslı kaynaktan kod veya satır-satır port alınmaz.
- Açık internette bulunan DWG/DXF/font/screenshot yeniden dağıtılabilir varsayılmaz.
- Müşteri/kullanıcı CAD dosyaları private corpus'ta ve Git dışında kalır.
- Proprietary SHX/font dosyaları bundle edilmez.
- Final APK/AAB/IPA gerçek içerik envanteri source/package evidence ile karşılaştırılmadan release yapılmaz.
