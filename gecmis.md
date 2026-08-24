# mobil-dwg — Proje Geçmişi ve AI Handoff Kaydı

Bu dosya, yeni bir sohbet veya yeni bir yapay zeka oturumu başladığında projenin nerede kaldığını hızlı ve güvenilir biçimde anlamak için tutulur. Sohbet belleğine güvenilmez; repo içindeki dosyalar tek kalıcı kaynak olarak kabul edilir.

## Yeni bir ajan önce ne okumalı?

Sıra değişmez:

1. `gecmis.md` — nerede kaldık, ne yapıldı, neden yapıldı.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` — tek yetkili uygulama/yürütme planı ve checkpoint.
3. `docs/EXECUTION_LOG.md` — komut, test, revision ve teknik kanıt geçmişi.
4. Gerekirse `docs/ADR/` — mimari/teknoloji kararlarının gerekçeleri.
5. Diğer `*_oneriler.md`, `Master_Plan.md` ve benzeri dosyalar yalnız araştırma/önceki görüş kaynağıdır; nihai planla çelişirse nihai plan geçerlidir.

## Repo kimliği

- GitHub: `smitelagwar/mobil-dwg`
- Repo adı: `mobil-dwg`
- Varsayılan branch: `main`
- Görünürlük: private
- Ürün: Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer
- v1: viewer-only; edit/save/export/cloud/account kapsam dışında

## Aktif checkpoint

```text
CURRENT_STAGE: AŞAMA 00
STATUS: DONE
NEXT_STAGE: AŞAMA 01
NEXT_ACTION: Resmi kaynaklardan güncel .NET 10 SDK/MAUI/Android toolchain sürümlerini LIVE-VERIFY et; exact sürümleri belirle ve gerçek geliştirme ortamında kurulum/build/device smoke hattını başlat.
BLOCKERS: Yok
LAST_UPDATE: 2026-08-24
```

Önemli yürütme kuralı: Bir kullanıcı turunda en fazla bir aşama tamamlanır. AŞAMA 00 bittiği için sonraki `devam` isteğinde AŞAMA 01 başlanır; aynı turda AŞAMA 02’ye geçilmez.

## Aşama durumu

- [x] AŞAMA 00 — Çalışma alanı ve yürütme zemini
- [ ] AŞAMA 01 — .NET/MAUI/Android toolchain ve gerçek telefon
- [ ] AŞAMA 02 — Canlı dependency/lisans kanıtı ve kilitler
- [ ] AŞAMA 03 — Test corpus’u, golden sözleşmesi ve cihaz matrisi
- [ ] AŞAMA 04 — Minimal solution ve mimari sınırlar
- [ ] AŞAMA 05 — ACadSharp headless parser spike
- [ ] AŞAMA 06 — Android güvenli dosya alma ve parse spike
- [ ] AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO
- [ ] AŞAMA 08 — Erken iOS AOT/native fizibilite smoke
- [ ] AŞAMA 09 — RenderScene, kamera ve diagnostics temeli
- [ ] AŞAMA 10 — P0 temel geometri renderer’ı
- [ ] AŞAMA 11 — Mobil viewport ve gesture’lar
- [ ] AŞAMA 12 — Block/INSERT/attribute dönüşümleri
- [ ] AŞAMA 13 — Layer, renk, linetype ve lineweight
- [ ] AŞAMA 14 — TEXT/MTEXT, Türkçe, font ve SHX
- [ ] AŞAMA 15 — Dimension, leader ve hatch doğruluğu
- [ ] AŞAMA 16 — Model space, layout, paper space ve viewport
- [ ] AŞAMA 17 — XREF/raster/underlay ve compatibility raporu
- [ ] AŞAMA 18 — Tam Android viewer UX ve lifecycle
- [ ] AŞAMA 19 — Kötü niyetli/bozuk dosya ve resource guard’ları
- [ ] AŞAMA 20 — Ölçümlü performans ve bellek optimizasyonu
- [ ] AŞAMA 21 — Android tam corpus regresyon ve beta kapısı
- [ ] AŞAMA 22 — Android Release/AAB/compliance RC
- [ ] AŞAMA 23 — iOS toolchain, shared core ve ilk gerçek cihaz
- [ ] AŞAMA 24 — iOS fidelity, lifecycle ve Release archive
- [ ] AŞAMA 25 — Cross-platform beta ve yalnız blocker düzeltmeleri
- [ ] AŞAMA 26 — Dependency freeze, final audit ve RC onayı
- [ ] AŞAMA 27 — v1 artifact, yayın/handoff ve kapanış

## 2026-08-24 — Başlangıçta repo nasıldı?

GitHub’daki ilk doğrulanan proje revision’ı:

- `d161b5c4f9ba238f0d2a2e4c92f773535f379487`
- Commit: `docs: add final mobile DWG project plan`

Bu revision’da uygulama kaynak kodu yoktu. Repo planlama/teknik doğrulama durumundaydı. Aşağıdaki ana belgeler zaten mevcuttu:

- `.gitignore`
- `README.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`
- `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Master_Plan.md`
- `chatgpt_oneriler.md`
- `claude_oneriler.md`
- `gemini_oneriler.md`
- `sonnet_5.md`

Nihai planın GitHub blob SHA’sı `a05dc53df058c5355f8576996a33cce704ac19f3` idi. Kullanıcının bu sohbete yüklediği nihai plan dosyasının Git blob SHA’sı da aynı çıktı; yani başlangıçta yerel yüklenen plan ile GitHub’daki nihai plan birebir eşleşiyordu.

## 2026-08-24 — AŞAMA 00’da ne yapıldı?

AŞAMA 00 hedefi izlenebilir yürütme zemini kurmaktı. Gerçek repo incelendiğinde Git deposunun zaten mevcut olduğu görüldü; bu nedenle yeniden `git init` gibi yapay veya destructive işlem yapılmadı.

Yapılanlar:

- GitHub repo, `main` branch ve başlangıç commit’i doğrulandı.
- Mevcut kaynak/araştırma MD belgeleri korundu.
- `.gitignore` tek tek incelendi. Build/IDE çıktıları, secret/signing materyali, private CAD corpus/cache, lisansı ayrıca onaylanmamış font ve CAD asset’lerini ignore ettiği görüldü. Yeterli olduğu için sırf değişiklik üretmek amacıyla düzenlenmedi.
- `docs/EXECUTION_LOG.md` oluşturuldu; bundan sonra teknik komut/test/evidence geçmişi burada tutulacak.
- `docs/ADR/0000-template.md` oluşturuldu; önemli teknoloji/mimari kararları kanıtlarıyla kaydetmek için kullanılacak.
- `docs/EVIDENCE_TEMPLATE.md` oluşturuldu; aşama çıkış kriterleri ve test kanıtları standart formatta tutulacak.
- Bu `gecmis.md` oluşturuldu; yeni sohbet/yeni AI için kalıcı handoff/checkpoint kaydı olacak.
- README ve nihai plan checkpoint’i bu devir mekanizmasını gösterecek şekilde güncellenecek/güncellendi.

### AŞAMA 00 ortam envanteri

Bu değerler ChatGPT’nin 24 Ağustos 2026 tarihli çalışma konteynerine aittir; kullanıcının fiziksel geliştirme bilgisayarı olduğu varsayılmaz:

- Disk: yaklaşık 63 GB toplam, 38 GB boş.
- Git: `2.47.3`.
- `dotnet`: PATH üzerinde yok.
- Java: OpenJDK `21.0.11`.
- `adb`: PATH üzerinde yok.

Eski plan içindeki 24.08.2026 “yerel ortam fotoğrafı” ile bu konteyner arasında fark vardır. Gerçek durum esas alınmıştır. AŞAMA 01’de kullanılacak fiziksel geliştirme ortamı yeniden ölçülmeden bu konteyner değerleri kurulum kanıtı sayılmaz.

### AŞAMA 00 commit geçmişi

- `1619f043af3c0087794f171b1a0baeb53124685a` — `docs: add stage 00 execution log`
- `a055ab145e5614180b3c28cd307d7563628a515b` — `docs: add ADR template`
- `52a575e50c17dfdd96e91710f27450c870f74a70` — `docs: add evidence template`

Bu dosyanın ve sonraki checkpoint güncellemelerinin commit SHA’ları yeni kayıtlarla birlikte `docs/EXECUTION_LOG.md` veya bu kronolojiye eklenmelidir.

## Değiştirilemez temel teknik kararlar

Nihai plan değiştirilmedikçe yeni ajan şunları varsaymalıdır:

- v1 yalnız 2D viewer; edit/write yok.
- DWG/DXF cihazda ve offline okunur; zorunlu cloud conversion yok.
- Autodesk RealDWG, APS/Forge dönüşümü, ticari ODA SDK, trial/ücretli CAD parser-renderer yok.
- Varsayılan lisans allowlist: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD; exact dependency yine ayrıca denetlenir.
- Runtime graph’ta GPL/AGPL/SSPL/BUSL/non-commercial/source-available/proprietary/unknown bileşen release blocker’dır.
- ACadSharp ana parser adayıdır ama corpus gate geçmeden production-approved değildir.
- SkiaSharp ana renderer adayıdır; native dağıtım zinciri ayrıca denetlenir.
- ProCad yalnız izole, source-pinned spike ile değerlendirilecek; production’a kendiliğinden girmeyecek.
- UI doğrudan parser entity’lerine bağlanmayacak; parser/scene/renderer sınırları korunacak.
- Unsupported/proxy/font/XREF/raster kaybı sessiz olmayacak; compatibility raporlanacak.
- Original drawing hiçbir koşulda overwrite edilmeyecek.

## Yeni ajan için çalışma protokolü

Bir sonraki ajan veya sohbet şu şekilde devam etmelidir:

1. `gecmis.md`, nihai plan checkpoint’i ve `docs/EXECUTION_LOG.md` okunur.
2. GitHub `main` gerçek durumu ve son commit’ler tekrar doğrulanır; bu dosyadaki eski SHA körlemesine kullanılmaz.
3. Kullanıcı değişiklikleri varsa korunur; destructive Git işlemi yapılmaz.
4. `IN_PROGRESS` aşama varsa yalnız oradan devam edilir. Mevcut durumda AŞAMA 00 `DONE`, bu yüzden AŞAMA 01 başlanmalıdır.
5. Bir turda en fazla bir aşama tamamlanır.
6. Her biten alt adım kanıtla işaretlenir; sahte PASS/DONE yazılmaz.
7. Dependency sürümü kendiliğinden yükseltilmez; `[LIVE-VERIFY]` noktaları güncel resmi kaynakla yeniden doğrulanır.
8. Her turun sonunda nihai plan checkpoint’i, `gecmis.md` ve gerekirse `docs/EXECUTION_LOG.md` güncellenir.
9. Uzun sohbet belleğine veya modelin kişisel hafızasına güvenilmez; repo kayıtları esas alınır.

## Bir sonraki tur

AŞAMA 01 başlanacak. İlk alt iş, resmi Microsoft/.NET/MAUI/Android belgelerinden 24 Ağustos 2026 itibarıyla desteklenen exact .NET 10 SDK patch’i, MAUI workload hattı, JDK 21 ve Android SDK/platform-tools gereksinimlerini canlı doğrulamaktır. Ardından gerçek geliştirme ortamında `global.json`, workload kurulumu, boş MAUI Debug/Release build ve fiziksel Android cihaz `adb` install/launch kanıtı gerekir. Fiziksel cihaz erişimi olmadan AŞAMA 01 `DONE` sayılamaz.
