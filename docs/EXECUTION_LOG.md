# Execution Log

Bu dosya `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` içindeki aşamaların teknik yürütme kanıtlarını tutar. İnsan ve yeni ajan devri için özet durum kökteki `gecmis.md` dosyasındadır.

## Kayıt kuralları

Her çalışma turunda mümkün olduğunca şu bilgiler eklenir:

- tarih ve aktif aşama,
- başlangıç repo revision/branch,
- incelenen veya değiştirilen dosyalar,
- çalıştırılan komut/test ve kısa sonuç,
- ölçülen ortam/toolchain sürümleri,
- oluşturulan commit/PR veya artifact,
- açık risk/blocker,
- tek somut sonraki eylem.

Başarı kanıtsız yazılmaz. Uzun log kopyalanmaz; gerekli özet ve hata bağlamı tutulur. Hassas dosya yolu, müşteri çizim adı/içeriği, secret veya signing materyali kaydedilmez.

---

## 2026-08-24 — AŞAMA 00: Çalışma alanı ve yürütme zemini

Durum: `DONE`

Başlangıç GitHub durumu:

- Repo: `smitelagwar/mobil-dwg`
- Branch: `main`
- Başlangıç revision: `d161b5c4f9ba238f0d2a2e4c92f773535f379487`
- Başlangıç commit mesajı: `docs: add final mobile DWG project plan`
- Uygulama kodu yok; plan ve araştırma belgeleri mevcut.

Doğrulanan repo öğeleri:

- `.gitignore`
- `README.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan.md`
- `chatgpt_oneriler.md`
- `claude_oneriler.md`
- `gemini_oneriler.md`
- `sonnet_5.md`

`.gitignore` incelemesi:

- .NET/IDE build çıktıları ignore ediliyor.
- secret/signing materyali ignore ediliyor.
- private DWG/DXF ve türetilmiş preview/cache öğeleri ignore ediliyor.
- yalnız açıkça incelenmiş `fixtures/public` CAD fixture istisnası var.
- SHX/PAT/LIN/TTF/OTF varlıkları varsayılan olarak ignore; yalnız `assets/approved/**` istisna.
- Bu nedenle AŞAMA 00 kapsamında `.gitignore` değiştirilmedi.

Çalışma ortamı envanteri (bu ChatGPT çalışma konteyneri; kullanıcının fiziksel geliştirme bilgisayarı olarak yorumlanmamalı):

- Disk: yaklaşık 63 GB toplam, 38 GB boş.
- Git: `2.47.3`.
- `dotnet`: PATH üzerinde yok.
- Java: OpenJDK `21.0.11`.
- `adb`: PATH üzerinde yok.
- `/mnt/data` içinde kullanıcıdan yüklenen nihai plan dosyası bulundu ve GitHub’daki dosyanın Git blob SHA’sı ile birebir eşleşti: `a05dc53df058c5355f8576996a33cce704ac19f3`.

AŞAMA 00’da oluşturulan süreklilik kayıtları:

- `gecmis.md`
- `docs/EXECUTION_LOG.md`
- `docs/ADR/0000-template.md`
- `docs/EVIDENCE_TEMPLATE.md`

Plan checkpoint’i AŞAMA 00 `DONE`, sonraki eylem AŞAMA 01 olacak şekilde güncellenmiştir.

Doğrulama özeti:

- GitHub repo geçmişi ve ana belgeler okundu.
- Kaynak araştırma MD dosyaları değiştirilmedi.
- `.gitignore` mevcut haliyle stage gereksinimlerini karşılıyor.
- Repo sürekliliği için kalıcı log/handoff dosyaları eklendi.

Sonraki eylem: AŞAMA 01’de resmi kaynaklardan .NET 10/MAUI/Android exact toolchain sürümlerini canlı doğrulamak ve gerçek geliştirme ortamında kurulum/build/device smoke hattını başlatmak.

---

## 2026-08-24 — AŞAMA 01: .NET/MAUI/Android toolchain ve gerçek telefon

Durum: `BLOCKED`

Başlangıç revision:

- `fe3c8c043e6d373e6313d2e1201cc24992b493a9`
- AŞAMA 00 tamamlanmış, uygulama kodu henüz yoktu.

Canlı doğrulanan toolchain baseline:

- .NET SDK `10.0.400` / runtime servicing `10.0.11` — 2026-08-11 release.
- Workload set `10.0.400`; Android-first workload `maui-android`.
- Microsoft Build of OpenJDK `21.0.12` LTS.
- Android minimum API `24`.
- Android compile/target API `36`.
- Android SDK Platform 36 revision `1`.
- Android SDK Build-Tools `36.0.0`.
- Android SDK Platform-Tools `37.0.0` stable; `37.0.1` Canary olduğu için baseline dışında.
- Android command-line tools stable download build ID `15859902`.
- Google Play: 2026-08-31 itibarıyla yeni uygulama ve güncellemeler target API 36 veya üzeri olmak zorunda.

Repo değişiklikleri:

- `global.json` oluşturuldu; SDK `10.0.400`, workload set `10.0.400`, prerelease kapalı ve `rollForward=disable`.
- `docs/TOOLCHAIN.md` oluşturuldu; exact toolchain, resmi kaynak snapshot'ı ve fiziksel cihaz kapısı kaydedildi.
- `docs/evidence/STAGE_01.md` oluşturuldu; tamamlanan doğrulamalar ve eksik gerçek cihaz kanıtları ayrıldı.

Commitler:

- `15d69e6b5b9e0c20f5ef7b0a742ac25ce5cc9071` — `build: pin .NET 10.0.400 toolchain`
- `467a2fe69366bfc640400d4b2ccbd97309b09189` — `docs: record stage 01 toolchain baseline`
- `658345321d1a76f7f3a9f6e6958e62a6868415a0` — `docs: add stage 01 evidence and blocker`

Çalışma konteyneri gözlemi:

- Linux x86_64.
- Java `21.0.11` mevcut.
- `dotnet` ve `adb` PATH üzerinde yok.
- .NET 10.0.400 resmi direct-download URL'si doğrulandı; ancak konteyner dış DNS erişimi kapalı olduğundan indirme denemesi `Could not resolve host: builds.dotnet.microsoft.com` ile başarısız oldu.
- Bu hata toolchain uyumsuzluğu değildir; ChatGPT çalışma konteynerinin network kısıtıdır.

Eksik zorunlu AŞAMA 01 kanıtları:

- Gerçek geliştirme makinesinde .NET 10.0.400 ve MAUI Android workload kurulumu.
- Microsoft OpenJDK 21.0.12 + `JAVA_HOME` doğrulaması.
- Android API 36 / Build-Tools 36.0.0 / Platform-Tools 37.0.0 kurulumu.
- Temiz MAUI smoke app Debug ve Release build.
- Fiziksel Android cihazın `adb devices` çıktısında `device` olması.
- Smoke app install/launch gerçek telefon kanıtı.
- iOS Mac/Xcode/iPhone/Apple hesap erişim envanteri.

Blocker:

- Bu oturumda kullanıcının gerçek geliştirme makinesine ve fiziksel Android cihazına USB/ADB erişimi yok. Nihai plan fiziksel cihaz çalıştırmasını zorunlu tuttuğundan AŞAMA 01 `DONE` sayılamaz.

Sonraki eylem: Gerçek geliştirme ortamında `docs/TOOLCHAIN.md` baseline'ına göre toolchain'i kurup boş MAUI uygulamasını Debug/Release build etmek ve fiziksel Android cihazda `adb` install/launch kanıtını kaydetmek.
