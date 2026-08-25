# mobil-dwg

Android ve iOS için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi.

Proje yürütme aşamasındadır. AŞAMA 00, AŞAMA 02, AŞAMA 03, AŞAMA 04, AŞAMA 05 ve AŞAMA 07 tamamlandı. AŞAMA 01'in toolchain/CI kısmı tamamlandı fakat gerçek Android install/launch ve iOS erişim envanteri `BLOCKED / DEFERRED_EXTERNAL_GATE` olarak açık. AŞAMA 06'nın safe-open/MAUI Android CI kısmı geçti fakat gerçek telefon FilePicker/SAF+lifecycle/cache gate'i `BLOCKED / DEFERRED_EXTERNAL_GATE`. AŞAMA 07'de exact pinned ProCad candidate Android source build ve clean MAUI Release smoke ile derlendi, ancak survey-origin 1 mm detay direct `double→float` RenderScene sınırında deterministik olarak çöktüğü için production reuse kararı `NO-GO`. ProCad production graph'a eklenmedi. ACadSharp `3.7.1` read-only parser baseline AŞAMA 05'te `GO` kalır. Sonraki bağımsız çalışma aşaması AŞAMA 08'dir; AŞAMA 09 custom renderer implementation öncesinde ADR 0002'deki HIGH efor/bakım riski için kullanıcı GO gerekir.

## Yeni sohbet / yeni AI başlangıcı

Projeyi devralan kişi veya ajan önce şu dosyaları bu sırayla okumalıdır:

1. [gecmis.md](gecmis.md) — güncel çalışma checkpoint'i, geçmiş işler, commitler, blocker ve sonraki eylem.
2. [docs/USER_APPROVED_EXECUTION_OVERRIDE.md](docs/USER_APPROVED_EXECUTION_OVERRIDE.md) — ertelenmiş dış donanım/hesap kapılarının yürütme kuralı.
3. [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) — ürün/teknik plan ve aşama çıkış kriterleri.
4. [docs/evidence/STAGE_07.md](docs/evidence/STAGE_07.md) ve [docs/ADR/0002-procad-pinned-source-no-go.md](docs/ADR/0002-procad-pinned-source-no-go.md) — ProCad NO-GO precision/source/NuGet kanıtı.
5. [docs/evidence/STAGE_06.md](docs/evidence/STAGE_06.md) — geçen CI/safe-open kısmı ve açık fiziksel Android dış kapısı.
6. [docs/evidence/STAGE_05.md](docs/evidence/STAGE_05.md) ve [docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md](docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md) — parser/corpus kanıtı ve sürüm kararı.
6. [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) ve [MobilDwg.sln](MobilDwg.sln) — production/test proje sınırları ve dependency yönleri.
7. [docs/evidence/STAGE_04.md](docs/evidence/STAGE_04.md) — minimal solution/mimari kapanış kanıtı.
8. [docs/evidence/STAGE_03.md](docs/evidence/STAGE_03.md), [fixtures/manifest/stage03-mini.json](fixtures/manifest/stage03-mini.json) ve [docs/GOLDEN_CONTRACT.md](docs/GOLDEN_CONTRACT.md) — corpus/golden sözleşmesi.
9. [docs/evidence/STAGE_02.md](docs/evidence/STAGE_02.md) ve [compliance/DEPENDENCY_EVIDENCE.md](compliance/DEPENDENCY_EVIDENCE.md) — dependency/source/artifact kanıtı.
10. [docs/EXECUTION_LOG.md](docs/EXECUTION_LOG.md) — teknik yürütme geçmişi.
11. [docs/TOOLCHAIN.md](docs/TOOLCHAIN.md) ve [docs/evidence/STAGE_01.md](docs/evidence/STAGE_01.md) — pinlenmiş toolchain ve ertelenmiş Stage 01 dış kapıları.
12. [docs/ANDROID_TEST_KULLANIM_KILAVUZU.md](docs/ANDROID_TEST_KULLANIM_KILAVUZU.md) — Self-Hosted Windows runner ve Android Emulator test kılavuzu.

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
