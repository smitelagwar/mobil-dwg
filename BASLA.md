# mobil-dwg — Yeni Sohbet Otomatik Başlatma Dosyası

> **BU DOSYANIN OKUNMASI BİR ÇALIŞTIRMA KOMUTUDUR.**
>
> Kullanıcı yeni sohbette yalnızca **`BASLA.md dosyasını oku`** derse, bunu “projeyi gerçek GitHub durumundan doğrula ve kaldığı yerden çalışmaya devam et” komutu olarak kabul et.
>
> Dosyayı okuyup sadece özet çıkarma, kullanıcıdan ayrıca `devam` isteme ve “ne yapmak istersin?” diye sorma. Aşağıdaki protokolü uygula ve mevcut aşamadaki işi doğrudan yürüt.

## 1. Repo

- GitHub repo: `smitelagwar/mobil-dwg`
- Default branch: `main`
- Repo private.
- Ürün: Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer.
- v1: viewer-only. Edit/write/save/export/cloud/account kapsam dışıdır.

GitHub connector kullanılabiliyorsa gerçek repo durumunu mutlaka connector üzerinden oku. Sohbet/model belleğini proje durumu için kaynak kabul etme.

## 2. Başlangıç protokolü — soru sormadan uygula

`BASLA.md` okunduğu anda sırayla:

1. `smitelagwar/mobil-dwg` reposunun gerçek `main` HEAD’ini doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyasını oku. Bu **canonical/yetkili plandır**.
3. `gecmis.md` dosyasını oku. Buradaki aktif checkpoint, son tamamlanan aşama, `NEXT_WORK_STAGE`, açık blocker’lar ve son CI/merge kanıtlarını doğrula.
4. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` dosyasını oku ve dış cihaz/hesap kapılarının yürütme kuralını uygula.
5. Eğer `gecmis.md` veya canonical plan bir `IN_PROGRESS` aşama gösteriyorsa **o aşamadan devam et**. Yeni aşama başlatma.
6. Aşama `DONE` ve bir `NEXT_WORK_STAGE` varsa, **NEXT_WORK_STAGE’i doğrudan başlat**.
7. Aktif aşama için ilgili `docs/evidence/STAGE_XX.md`, mimari/compliance/fixture dosyaları, açık PR’lar ve GitHub Actions koşularını gerektiği kadar oku.
8. Açık PR veya yarım CI run varsa yeni iş açmadan önce onların gerçek durumunu kontrol et ve kaldığı yerden devam et.
9. Kullanıcıdan daha önce verilmiş bilgiyi tekrar isteme. Fiziksel erişim gerektirmeyen işi mümkün olduğunca tamamla.
10. Bir kullanıcı turunda en fazla **bir aşama** tamamla. Aynı turda sonraki aşamaya başlama.

## 3. En önemli davranış kuralı

Kullanıcı yalnızca:

`BASLA.md dosyasını oku`

dediğinde cevap davranışı şu olmalıdır:

- Dosyayı oku.
- GitHub `main` durumunu doğrula.
- Canonical plan + `gecmis.md` checkpoint’ini çöz.
- Mevcut/sonraki aşamayı belirle.
- **Doğrudan uygulamaya başla.**

Yalnız “dosyayı okudum”, “AŞAMA X’te kalmışız” veya “devam etmemi ister misin?” şeklinde durma.

## 4. Çelişki çözümü

Kaynaklar arasında çelişki varsa sessizce tahmin etme.

Öncelik:

1. Gerçek GitHub `main` üzerindeki kod/commit/PR/CI durumu.
2. Canonical plan: `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`.
3. `gecmis.md` aktif checkpoint/handoff.
4. İlgili `docs/evidence/STAGE_XX.md` ve compliance/test kayıtları.
5. `DEVAM.md` yalnız yardımcı snapshot olabilir; daha güncel gerçek repo kayıtlarının önüne geçmez.

Çelişki güvenli biçimde çözülebiliyorsa aynı turda kayıtları senkronize et ve çalışmaya devam et. Gerçek bir blocker değilse kullanıcıya karar yükleme.

## 5. Kullanıcının mevcut dış erişim kısıtı

AŞAMA 01’de kalan gerçek fiziksel kapılar kullanıcı tarafından şimdilik sağlanamıyor:

- fiziksel Android cihazda install/launch,
- `STAGE01_DEVICE_GATE_PASS`,
- gerçek Mac/Xcode/iPhone/Apple Developer erişim envanteri.

Bunları **sahte PASS/DONE yapma**. `BLOCKED / DEFERRED_EXTERNAL_GATE` olarak açık tut.

Ancak bu dış kapılar, fiziksel erişime bağımlı olmayan sonraki aşamaları engellemez. Kullanıcı daha önce bağımsız aşamalara devam edilmesini açıkça onaylamıştır.

Release/beta/final aşamalarında plan bu kanıtları yeniden zorunlu kılıyorsa o zaman gerçek blocker olarak ele al.

## 6. Değiştirilemez proje ilkeleri

- v1 yalnız 2D viewer; editor/writer/save/export yok.
- DWG/DXF cihazda ve offline okunur; zorunlu cloud conversion yok.
- Autodesk RealDWG, APS/Forge conversion, ticari ODA SDK, trial/ücretli CAD SDK yok.
- Varsayılan runtime lisans allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD.
- GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/unknown runtime dependency release blocker’dır.
- ACadSharp corpus gate geçmeden production-approved değildir.
- ProCad yalnız source-pinned izole spike; otomatik production dependency değildir.
- UI parser entity’lerine doğrudan bağlanmaz.
- Original CAD dosyası immutable; overwrite edilmez.
- Unsupported/proxy/font/XREF/raster problemleri sessizce gizlenmez.
- Optimizasyon yalnız ölçülmüş bottleneck üzerinde yapılır.
- Başarı kanıtsız PASS/DONE yazılmaz.

## 7. Git ve kullanıcı dosyaları

- Kullanıcı değişikliklerini koru.
- Destructive Git işlemi yapma.
- Gerçek müşteri/kullanıcı DWG-DXF, private corpus, font, signing key, token veya secret repoya ekleme.
- Public/synthetic fixture yalnız provenance/lisans politikasıyla uyumluysa eklenebilir.
- PR/CI kullanılıyorsa doğrulanmamış head’i merge etme.

## 8. Her aşama sonunda zorunlu kapanış

Aşama gerçekten tamamlandığında aynı turda:

1. İlgili `docs/evidence/STAGE_XX.md` dosyasını gerçek run/commit/artifact sonuçlarıyla güncelle.
2. `gecmis.md` içindeki `LAST_COMPLETED_STAGE`, `NEXT_WORK_STAGE`, CI/merge ve blocker kayıtlarını güncelle.
3. Canonical plan checkpoint’ini ve ilgili aşama checkbox’larını gerçek durumla güncelle.
4. README/handoff girişinde eski aşama bilgisi varsa düzelt.
5. `docs/EXECUTION_LOG.md` teknik geçmişini güncelle.
6. `DEVAM.md` tutuluyorsa checkpoint snapshot’ını güncelle; ancak `BASLA.md` genel bootstrap protokolüdür ve aşama numarası içermez, normalde değiştirilmesi gerekmez.
7. O turda sonraki aşamaya başlama.

Aşama tamamlanmamışsa `DONE` yazma; gerçek `IN_PROGRESS` veya `BLOCKED` durumunu ve tam sonraki eylemi kalıcı kayıtlara geçir.

## 9. Toolchain/dependency gerçekliği

Pinler veya güncel sürüm/politika gerektiğinde canonical planın `[LIVE-VERIFY]` kuralını uygula. Güncel resmi kaynakları doğrula; geçmiş sohbet bilgisini “latest” kabul etme.

Mevcut repo pinlerinin değiştirilmesi ancak gerçek gerekçe + build/test/compliance kanıtıyla yapılmalıdır.

## 10. Bu dosyanın kullanım şekli

Yeni sohbette kullanıcının yazması gereken tek cümle:

> **BASLA.md dosyasını oku**

Bu cümle tek başına yeterlidir. Ek olarak “kaldığımız yerden devam et”, “GitHub’a bak” veya `devam` yazması gerekmez.
