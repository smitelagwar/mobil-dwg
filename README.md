# mobil-dwg

Android ve iOS için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi.

Proje yürütme aşamasındadır. AŞAMA 00, AŞAMA 02, AŞAMA 03, AŞAMA 04 ve AŞAMA 05 tamamlandı. AŞAMA 01'in canlı toolchain doğrulaması, repo pinleri ve CI build hattı tamamlandı; gerçek geliştirme makinesi + fiziksel Android cihaz install/launch ve iOS erişim envanteri olmadığı için AŞAMA 01 `BLOCKED / DEFERRED_EXTERNAL_GATE` durumunda açık kalır. Kullanıcının onayladığı yürütme istisnası gereği fiziksel erişime bağımlı olmayan işler ilerleyebilir. ACadSharp `3.7.1` read-only parser baseline AŞAMA 05 corpus gate'inde `GO` aldı; bu render/engineering fidelity garantisi değildir. Sonraki çalışma aşaması AŞAMA 06'dır.

## Yeni sohbet / yeni AI başlangıcı

Projeyi devralan kişi veya ajan önce şu dosyaları bu sırayla okumalıdır:

1. [gecmis.md](gecmis.md) — güncel çalışma checkpoint'i, geçmiş işler, commitler, blocker ve sonraki eylem.
2. [docs/USER_APPROVED_EXECUTION_OVERRIDE.md](docs/USER_APPROVED_EXECUTION_OVERRIDE.md) — ertelenmiş dış donanım/hesap kapılarının yürütme kuralı.
3. [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) — ürün/teknik plan ve aşama çıkış kriterleri.
4. [docs/evidence/STAGE_05.md](docs/evidence/STAGE_05.md) ve [docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md](docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md) — son tamamlanan bağımsız aşamanın parser/corpus kanıtı ve sürüm kararı.
5. [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) ve [MobilDwg.sln](MobilDwg.sln) — production/test proje sınırları ve dependency yönleri.
6. [docs/evidence/STAGE_04.md](docs/evidence/STAGE_04.md) — minimal solution/mimari kapanış kanıtı.
7. [docs/evidence/STAGE_03.md](docs/evidence/STAGE_03.md), [fixtures/manifest/stage03-mini.json](fixtures/manifest/stage03-mini.json) ve [docs/GOLDEN_CONTRACT.md](docs/GOLDEN_CONTRACT.md) — corpus/golden sözleşmesi.
8. [docs/evidence/STAGE_02.md](docs/evidence/STAGE_02.md) ve [compliance/DEPENDENCY_EVIDENCE.md](compliance/DEPENDENCY_EVIDENCE.md) — dependency/source/artifact kanıtı.
9. [docs/EXECUTION_LOG.md](docs/EXECUTION_LOG.md) — teknik yürütme geçmişi.
10. [docs/TOOLCHAIN.md](docs/TOOLCHAIN.md) ve [docs/evidence/STAGE_01.md](docs/evidence/STAGE_01.md) — pinlenmiş toolchain ve ertelenmiş Stage 01 dış kapıları.

Sohbet veya model hafızası süreklilik kaynağı değildir; repo kayıtları esas alınır.

## Yetkili plan

Uygulama [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) planına göre geliştirilecektir. Diğer Markdown dosyaları araştırma veya geçmiş kayıt niteliğindedir.

## Temel ürün ilkeleri

- Viewer-first; edit ve save v1 kapsamı dışında
- Android-first, iOS zorunlu ikinci platform
- DWG/DXF doğrudan cihazda ve offline işlenir
- Zorunlu bulut, hesap veya dosya başına servis ücreti yok
- Ücretli CAD SDK/API ve runtime royalty yok
- Dependency, native binary, font ve test fixture lisansları release öncesi denetlenir
- Desteklenmeyen entity, eksik font ve dış referanslar sessizce gizlenmez

## Yürütme

Her `devam` komutunda `gecmis.md` içindeki `NEXT_WORK_STAGE` yürütülür. `IN_PROGRESS` aşama varsa yalnız o aşamadan devam edilir. `DEFERRED_EXTERNAL_GATE` durumundaki dış cihaz/hesap kapıları bağımsız aşamaları bloke etmez; sahte PASS/DONE üretilmez. Bir kullanıcı turunda en fazla bir aşama tamamlanır.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız redistribution/provenance durumu kaydedilmiş public/synthetic fixture ve asset'ler açıkça onaylanmış yollar altında tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler ve test kaynakları kendi lisans/provenance kayıtlarına tabidir.
