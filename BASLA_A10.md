# mobil-dwg — AŞAMA 10 ayrı çalışma hattı başlatıcısı

> **BU DOSYANIN OKUNMASI BİR ÇALIŞTIRMA KOMUTUDUR.**
>
> Kullanıcı yeni bir sohbette yalnızca **`BASLA_A10.md dosyasını oku`** derse, bunu “Android V04–V09 doğrulaması başka sohbette sürerken AŞAMA 10'u izole taslak branch'inde geliştir” komutu olarak kabul et.

Bu komut genel `BASLA.md` içindeki validation önceliğinin bilinçli ve dar kapsamlı istisnasıdır. Validation cursor'ını ilerletmez, VXX evidence kapatmaz ve AŞAMA 11'i açmaz.

## 1. Başlangıç protokolü

1. `smitelagwar/mobil-dwg` gerçek `main` HEAD'ini, açık PR'ları, mevcut A10 branch/PR durumunu ve son CI sonuçlarını doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `ANDROID_DOGRULAMA_PLANI.md`, `gecmis.md` ve `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` dosyalarını oku. Mevcut A10 branch/PR varsa `docs/A10_WORKSTREAM.md` dosyasını o branch/ref'ten; yoksa `main` başlangıç kaydından oku.
3. Çalışma `CHATGPT_REMOTE_GITHUB` bağlamındaysa `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` dosyasını da oku.
4. Mevcut bir A10 branch/PR varsa onu sürdür; yoksa güncel `main`den `stage10-p0-geometry-draft` adlı normal feature branch oluştur. `android-test` üzerinde kod geliştirme.
5. Validation branch'i veya `main` ilerlediyse A10 branch'ine güncel `main`i normal merge ile al; force-push/rewrite yapma.
6. Doğrudan AŞAMA 10 kapsamındaki güvenli taslak işi yürüt. Yalnız özet verip ayrıca `devam` isteme.

## 2. İzin verilen erken A10 kapsamı

- Mevcut production sözleşmelerine bağlanmayan yeni, internal primitive/tessellator tipleri ve saf matematik testleri.
- LINE/ARC/CIRCLE/ELLIPSE/POINT ile LW/POLYLINE + bulge/SPLINE için deterministik örnekleme matematiği.
- SOLID/TRACE/3DFACE vertex order, OCS/extrusion/mirror ve large-coordinate hesapları.
- Mevcut sözleşmeleri yalnız tüketen T1/precision/invalid-input regresyonları; architecture beklentileri değiştirilmez.

World coordinates `double` kalır. ProCad production graph'a alınmaz. ACadSharp entity'leri, MAUI, FilePicker/SAF veya Android lifecycle ayrıntıları Rendering/Core sınırına sızdırılmaz. AŞAMA 11 pan/pinch/viewport işi yazılmaz.

V09 kapanana kadar `RenderSceneEntity`, `IRenderScene`/`ICadRenderer`, `render-scene/v1`, architecture beklentileri/`docs/ARCHITECTURE.md`, `.csproj`/Skia wiring ve fixture/image-golden sözleşmesi dondurulmuştur. Runner açık olsa bile erken A10 bu alanları değiştirmez. Draw-order/clipping/antialias ve gerçek Skia çizim entegrasyonu V09 sonrası integration turuna bırakılır. Validation sonucu her zaman A10 taslağına üstün gelir.

## 3. Branch, test ve durum kuralları

- A10 yalnız `stage10-p0-geometry-draft` branch'inde geliştirilir; V04–V09 bitmeden `main`e merge edilmez.
- PC/runner kapalıyken önce güncel workflow path filtrelerini oku. Açık A10 PR'ı yoksa branch'e commit/push yapılabilir. A10 PR'ı zaten açıksa branch push'u PR `synchronize` olayıdır ve V04 merge edildikten sonra Core/Rendering değişikliklerinde self-hosted V04 emulator job'ı açabilir; offline push'tan önce PR'ı kapat/etkiyi güvenle gate et, bunu yapamıyorsan push yapma. Test borcunu `CODED_PENDING_HOST_TESTS` olarak kaydet.
- Runner/test ortamı hazır olduğunda draft PR açılır veya güncellenir. GitHub-hosted Release/Stage04/Stage09 ve tetiklenen self-hosted kontrollerin actual non-zero-step sonuçları doğrulanır. Hosted kapasite, billing/spending limiti veya başka bir dış nedenle iş başlamazsa bunu PASS sayma; exact run/blocker'ı kaydet.
- `.csproj`, package/dependency, fixture/provenance veya Android runtime değişikliği erken kapsamda yasaktır; V09 sonrası integration turunda ilgili V02/V03/self-hosted etkisiyle ele alınır.
- A10 sohbeti `android-test` branch'ini hareket ettirmez. Bu taşıyıcı branch validation/test koordinatörünün sahipliğindedir.
- Zorunlu host/GitHub-hosted kontrol sonuçsuz/zero-step/external blocker ise `CODED_PENDING_HOST_TESTS`; actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`; hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması ve Android gate'i bekleyen durum `CODED_PENDING_EMULATOR`dır. `PASS`, `READY_TO_MERGE` veya `DONE` yazılmaz.
- A10 branch head SHA'sı, base `main` SHA'sı, host testleri ve bekleyen Android gate `docs/A10_WORKSTREAM.md` içinde tutulur.
- VXX checkpoint, `ANDROID_DOGRULAMA_PLANI.md`, VXX evidence, `DEVAM.md` ve `gecmis.md` validation sohbetinin sahipliğindedir; A10 sohbeti paralel çalışırken bu ortak checkpoint dosyalarını değiştirmez.

Durum sırası:

```text
NOT_STARTED
  -> IN_PROGRESS_UNVALIDATED
  -> CODED_PENDING_HOST_TESTS
  -> FIX_REQUIRED -> FIX_IN_PROGRESS -> testleri tekrarla
  -> CODED_PENDING_EMULATOR
  -> READY_TO_MERGE
  -> DONE
```

## 4. A10 merge ve DONE kapısı

`READY_TO_MERGE` ancak aşağıdakilerin tamamıyla yazılabilir:

1. V04–V09 programı tanımlı kapalı durumdadır; V08 yalnız Android graph-isolation kontrolüdür, iOS workflow/Mac/Xcode testi açılmaz.
2. Güncel doğrulanmış `main`, A10 branch'ine alınmıştır; test edilen integration SHA kaydedilmiştir.
3. Etkilenen V02/V03, V04–V07, V08 Android graph-isolation ve V09 regresyonları exact integration SHA üzerinde geçmiştir.
4. A10 T1 + semantic/golden + C3 kabulü ve controlled invalid-geometry warning kanıtı geçmiştir.
5. Gerçek `MobilDwg.App`, API 36 emulator üzerinde A10 P0 fixture render yoluyla build/install/cold-launch edilmiştir; PID, byte-safe PNG, crash ve ANR yanında non-blank/expected-content pixel probe, Android golden karşılaştırması veya kaydedilmiş görsel incelemeden en az biri alınmıştır.
6. A10 için bekleyen emulator kuyruğu boştur.

`DONE` yalnız doğrulanmış A10 PR'ı `main`e merge edildikten, post-merge sonuç/eşdeğerlik doğrulandıktan ve `docs/evidence/STAGE_10.md` exact SHA/run/job/artifact ile kapatıldıktan sonra yazılır.

Bu merge yasağı GitHub branch protection ile teknik olarak enforce edilmiyor; private repo planındaki mevcut ruleset kısıtı nedeniyle prosedürel kapıdır. Ajan merge yöntemi ve durum kurallarına uymak zorundadır.

## 5. AŞAMA 11 kilidi

```text
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
```

AŞAMA 11 aynı A10 kapanış turunda başlatılmaz. Kullanıcı aynı A10 sohbetinde `devam` dediğinde A10 `DONE` değilse yine A10 sürdürülür.
