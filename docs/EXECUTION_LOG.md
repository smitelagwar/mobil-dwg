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
