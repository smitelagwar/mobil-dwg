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
- Aktif ürün: Android-only local/offline 2D DWG/DXF viewer. iOS yalnız future option; shared mimari geri dönüşe açık tutulur.
- v1: viewer-only. Edit/write/save/export/cloud/account kapsam dışıdır.

GitHub connector kullanılabiliyorsa gerçek repo durumunu mutlaka connector üzerinden oku. Sohbet/model belleğini proje durumu için kaynak kabul etme.

## 2. Başlangıç protokolü — soru sormadan uygula

`BASLA.md` okunduğu anda sırayla:

1. `smitelagwar/mobil-dwg` reposunun gerçek `main` HEAD’ini doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyasını oku. Bu **canonical/yetkili plandır**.
3. `ANDROID_DOGRULAMA_PLANI.md` dosyasını oku. V01–V09 Android geriye dönük doğrulama programı tamamlanmadıysa bu dosyadaki test cursor'ı birinci iş sırasıdır.
4. `gecmis.md` dosyasını oku. Buradaki ayrı implementation/doğrulama cursor'larını, açık blocker/test kuyruğunu ve son CI/merge kanıtlarını doğrula.
5. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` dosyasını oku ve dış cihaz/runner kapılarının yürütme kuralını uygula.
6. **Gerçek çalışma bağlamını otomatik sınıflandır:** Kod değişiklikleri ChatGPT sohbetinden GitHub connector/API üzerinden yapılıyor ve ajan kullanıcının yerel repo/terminal/ADB ortamında doğrudan çalışmıyorsa bağlam `CHATGPT_REMOTE_GITHUB` sayılır. Bu bağlamda `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` dosyasını **okumadan implementasyona başlama**. AntiGravity, Visual Studio + Codex, Codex IDE veya başka bir yerel ajan gerçek yerel çalışma ağacı + terminal/ADB erişimiyle çalışıyorsa bağlam `LOCAL_IDE` sayılır.
7. Açık V01–V09 doğrulaması varsa onu doğrudan sürdür. Runner çevrim dışıysa exact test SHA'sını kuyruğa al; kanıt beklerken güvenli kod/host-test işi varsa implementation cursor'ında devam et.
8. Doğrulama programı bittiyse açık normal `IN_PROGRESS` aşamayı, yoksa `NEXT_IMPLEMENTATION_STAGE` işini başlat.
9. Aktif iş için ilgili `docs/evidence/android-validation/VXX.md` veya tarihsel `docs/evidence/STAGE_XX.md`, mimari/compliance/fixture dosyaları, açık PR’lar ve GitHub Actions koşularını gerektiği kadar oku.
10. Açık PR veya yarım CI run varsa yeni iş açmadan önce gerçek durumunu kontrol et. Runner-offline queued işi implementation failure sayma ve aynı koşuyu çoğaltma.
11. Kullanıcıdan daha önce verilmiş bilgiyi tekrar isteme. Fiziksel erişim gerektirmeyen işi mümkün olduğunca tamamla.
12. Bir kullanıcı turunda en fazla **bir doğrulama veya implementation aşaması** tamamla. Aynı turda sonraki aşamaya başlama.

Aktif Android işinde `docs/evidence/STAGE_08.md`, `docs/STAGE_01_IOS_ACCESS_INVENTORY.md`, iOS spike/script ve AŞAMA 23–24 ayrıntılarını rutin olarak yükleme. Bunlar yalnız V08 graph-isolation kontrolünde gerektiği kadar veya kullanıcı future iOS'u açıkça yeniden etkinleştirirse okunur.

### 2.1 Çalışma bağlamı nasıl anlaşılır?

Kararı ürün adına veya kullanılan model adına göre değil, **gerçek dosya değiştirme ve test çalıştırma yoluna göre** ver:

- ChatGPT sohbeti → GitHub üzerinden dosya değişikliği → gerektiğinde GitHub üzerinden self-hosted Windows/Android Emulator testi: `CHATGPT_REMOTE_GITHUB`.
- Yerel IDE/ajan → yerel repo dosyalarına doğrudan yazma → yerel terminal, emulator veya ADB'yi doğrudan çalıştırma: `LOCAL_IDE`.

Bağlam araçlardan açıkça anlaşılabiliyorsa kullanıcıya sorma. Gerçekten ayırt edilemiyorsa yalnız o zaman kısa bir netleştirme iste.

## 3. En önemli davranış kuralı

Kullanıcı yalnızca:

`BASLA.md dosyasını oku`

dediğinde cevap davranışı şu olmalıdır:

- Dosyayı oku.
- GitHub `main` durumunu doğrula.
- Canonical plan + `ANDROID_DOGRULAMA_PLANI.md` + `gecmis.md` iki-cursor checkpoint’ini çöz.
- Çalışma bağlamını sınıflandır; `CHATGPT_REMOTE_GITHUB` ise `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` dosyasını da zorunlu olarak oku.
- Açık VXX doğrulamasını; yoksa mevcut/sonraki implementation aşamasını belirle.
- **Doğrudan uygulamaya başla.**

Yalnız “dosyayı okudum”, “AŞAMA X’te kalmışız” veya “devam etmemi ister misin?” şeklinde durma.

## 4. Çelişki çözümü

Kaynaklar arasında çelişki varsa sessizce tahmin etme.

Öncelik:

1. Gerçek GitHub `main` üzerindeki kod/commit/PR/CI durumu.
2. Canonical plan: `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`.
3. Aktif Android doğrulaması için `ANDROID_DOGRULAMA_PLANI.md`.
4. `gecmis.md` aktif checkpoint/handoff.
5. İlgili yeni/tarihsel evidence ve compliance/test kayıtları.
6. `DEVAM.md` yalnız yardımcı snapshot olabilir; daha güncel gerçek repo kayıtlarının önüne geçmez.

`docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` ürün kapsamını, stage çıkış kriterlerini veya canonical planı değiştirmez. Yalnız `CHATGPT_REMOTE_GITHUB` bağlamındaki mevcut test altyapısını ve verimli kullanım seçeneklerini açıklar.

Çelişki güvenli biçimde çözülebiliyorsa aynı turda kayıtları senkronize et ve çalışmaya devam et. Gerçek bir blocker değilse kullanıcıya karar yükleme.

## 5. Kullanıcının mevcut test ortamı ve dış erişim kısıtı

Aşağıdaki gerçekler ayrı tutulur:

- Windows Android Emulator ve self-hosted runner kuruludur; test için bilgisayar açık, interaktif oturum aktif ve `C:\actions-runner\run.cmd` dinliyor olmalıdır.
- Fiziksel Android cihaz `STAGE01_DEVICE_GATE_PASS` henüz açık release/cihaz farkı kanıtıdır.
- iOS/Mac/Xcode/iPhone/Apple Developer işi aktif Android kapsamından çıkarılmış, future option olarak dondurulmuştur.

Emulatoru fiziksel telefon; geçici `Stage01Smoke` APK'sını gerçek viewer; queued/offline runner'ı PASS sayma.

Runner veya bilgisayar çevrim dışıysa test SHA'sını `PENDING_EMULATOR_QUEUE` olarak kaydet ve güvenli kod/host-test işine devam et. Aynı test işini tekrar tekrar kuyruğa sokma.

Release/beta/final Android aşamalarında fiziksel cihaz ve boş test kuyruğu yeniden zorunludur. iOS yalnız kullanıcı açıkça yeniden etkinleştirirse ayrı blocker olur.

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

## 8. Her doğrulama/implementation aşaması sonunda zorunlu kapanış

Aşama gerçekten tamamlandığında aynı turda:

1. Yeni doğrulama için `docs/evidence/android-validation/VXX.md`; normal iş için ilgili `docs/evidence/STAGE_XX.md` dosyasını gerçek run/commit/artifact sonuçlarıyla güncelle. Tarihsel evidence geriye dönük yeniden yazılmaz.
2. `gecmis.md` içindeki ayrı validation/implementation cursor'larını, test kuyruğunu, CI/merge ve blocker kayıtlarını güncelle.
3. `ANDROID_DOGRULAMA_PLANI.md` ile canonical plan checkpoint'ini gerçek durumla güncelle.
4. README/handoff girişinde eski aşama bilgisi varsa düzelt.
5. `docs/EXECUTION_LOG.md` teknik geçmişini güncelle.
6. `DEVAM.md` tutuluyorsa checkpoint snapshot’ını güncelle; ancak `BASLA.md` genel bootstrap protokolüdür ve aşama numarası içermez, normalde değiştirilmesi gerekmez.
7. O turda sonraki aşamaya başlama.

Aşama tamamlanmamışsa `DONE/VALIDATED` yazma; gerçek `FIX_IN_PROGRESS`, `READY_FOR_EMULATOR`, `WAITING_RUNNER`, `IN_PROGRESS` veya `BLOCKED` durumunu ve tam sonraki eylemi kalıcı kayıtlara geçir.

## 9. Toolchain/dependency gerçekliği

Pinler veya güncel sürüm/politika gerektiğinde canonical planın `[LIVE-VERIFY]` kuralını uygula. Güncel resmi kaynakları doğrula; geçmiş sohbet bilgisini “latest” kabul etme.

Mevcut repo pinlerinin değiştirilmesi ancak gerçek gerekçe + build/test/compliance kanıtıyla yapılmalıdır.

## 10. Bu dosyanın kullanım şekli

Yeni sohbette kullanıcının yazması gereken tek cümle:

> **BASLA.md dosyasını oku**

Bu cümle tek başına yeterlidir. Ek olarak “kaldığımız yerden devam et”, “GitHub’a bak” veya `devam` yazması gerekmez.
