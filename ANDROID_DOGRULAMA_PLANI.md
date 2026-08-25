# mobil-dwg — Android Geriye Dönük Doğrulama Planı

Bu belge, AŞAMA 01–09 arasında geliştirilen kodu Android hedefinde sırayla yeniden doğrulamak, eksikleri düzeltmek ve gerekli yerde gerçek emulator kanıtı toplamak için yetkili alt plandır.

Ana ürün ve teknik plan `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyasıdır. Tarihsel `docs/evidence/STAGE_XX.md` kayıtları geriye dönük değiştirilmez; yeni sonuçlar `docs/evidence/android-validation/VXX.md` altında tutulur.

## 1. Aktif karar

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION — aktif kapsam, aktif DoD ve sıradaki işlerden çıkarıldı
IMPLEMENTATION_BASELINE: AŞAMA 01–09 kodlandı; AŞAMA 09 tamamlandı
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_VALIDATION_STAGE: V03
CURRENT_STATUS: NOT_STARTED
NEXT_ACTION: Yalnız V03'ü başlat — fixture/golden/provenance/private-ignore ve Android test matrisi doğrulaması; aynı turda V04'e geçme
AFTER_V09: test borcunu temizle ve implementation cursor'ında Android-first tek hatta devam et (başlangıçta AŞAMA 10)
PENDING_EMULATOR_QUEUE: EMPTY
```

iOS kodu ve geçmiş evidence silinmez. Shared Core, parser ve renderer katmanları platformdan bağımsız tutulur. Kullanıcı iOS yolunu açıkça yeniden etkinleştirene kadar Mac/Xcode/iPhone/iOS workload/signing/simulator/App Store işi Android'i bloke etmez.

## 2. `BASLA.md` ve `devam` davranışı

Kullanıcı `BASLA.md dosyasını oku` veya `devam` dediğinde ajan:

1. Gerçek `main` HEAD ve açık PR/işi doğrular.
2. `BASLA.md`, bu dosya, canonical plan, `gecmis.md`, `DEVAM.md`, execution override ve çalışma bağlamına uygun Android test workflow'unu okur.
3. Açık VXX aşaması tamamlanmadıysa doğrudan onu sürdürür.
4. Yalnız özet verip durmaz; güvenli kod/test/evidence işini uygular.
5. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; aynı turda sonraki aşama başlatılmaz.
6. V01–V09 tamamlanınca Android test borcu temizlenir ve implementation cursor'ı AŞAMA 10+ hattında sürdürülür.

## 3. Gerçeklik sınıfları

| Kanıt | Neyi kanıtlar | Neyi kanıtlamaz |
|---|---|---|
| Restore/build/unit/architecture testi | Host üzerinde kod ve sözleşme doğruluğu | Android install/lifecycle/UI |
| Geçici `Stage01Smoke` APK | Runner/SDK/emulator/ADB/MAUI altyapısı | Gerçek `MobilDwg.App` viewer veya fidelity |
| Gerçek `MobilDwg.App` emulator koşusu | Test edilen revision'daki Android uygulama akışı | Fiziksel cihaz çeşitliliği |
| Fiziksel Android koşusu | Kaydedilen cihaz/senaryo | Test edilmemiş cihazlar/iOS |

`scripts/android-emulator-gate.ps1` V01 itibarıyla geçici `Stage01Smoke` üretir. V04'te gerçek `MobilDwg.App` installable Android shell kurulana kadar bu sonuç yalnız `INFRASTRUCTURE_SMOKE_ONLY` sayılır.

## 4. Durum modeli

- `NOT_STARTED`: doğrulama başlamadı.
- `CODE_AUDIT`: kod/evidence karşılaştırılıyor.
- `FIX_REQUIRED`: somut açık bulundu.
- `FIX_IN_PROGRESS`: düzeltme uygulanıyor.
- `READY_FOR_EMULATOR`: kod hazır, exact runtime koşusu bekleniyor.
- `WAITING_RUNNER`: gerekli self-hosted koşu runner bekliyor.
- `VALIDATED`: tanımlı gerekli kanıtlar geçti.
- `VALIDATED_WITH_DEFERRED_PHYSICAL`: emulator kapsamı geçti, fiziksel fark açık.
- `SCOPE_ARCHIVED`: aktif Android kapsamı dışındaki tarihsel iş korunuyor.
- `DEFERRED_PHYSICAL_ANDROID`: fiziksel cihaz farkı release matrisine ertelendi.
- `BLOCKED`: güvenli ilerleme mümkün değil.

`READY_FOR_EMULATOR`, `WAITING_RUNNER`, queued veya zero-step workflow `PASS/VALIDATED` değildir.

## 5. Runner çevrim dışıyken çalışma

Runner çevrim dışıysa kod yazımı otomatik durmaz. Host-independent audit/test/dokümantasyon sürdürülür; gereken exact SHA/workflow/marker `PENDING_EMULATOR_QUEUE` kaydına alınır. Android UI/lifecycle/permission/packaging/native riskleri birikiyorsa en eski anlamlı runtime checkpoint'i görülmeden yüksek-riskli değişiklik yığılmaz. Test sonucu uydurulmaz ve fiziksel Android release kapısı açık kalır.

## 6. GitHub → yerel Android test kuralı

- Yetkili kaynak `main` veya ilgili feature branch'tir.
- `android-test` yalnız test taşıyıcı branch'idir; doğrudan ürün geliştirme branch'i değildir.
- Test edilen exact revision evidence'da kaydedilir.
- Carrier ref force-push edilmez.
- Workflow `SUCCESS`, yalnız gerçekten çalışan adımlar/scriptler kadar güçlüdür.
- GitHub-hosted job `steps=[]` ve `runner_id=0` ile biterse code/test failure diye sınıflandırılmaz.

## 7. V01–V09 doğrulama sırası

### V01 — Toolchain, runner ve emulator altyapısı — `VALIDATED`

V01 gate'in gerçek executable harness'ları yürütmesi, byte-safe screenshot üretmesi, numeric PID zorunluluğu ve package/PID-scoped crash + post-launch ANR evidence sağlaması için sertleştirildi.

Yetkili sonuç:

- Exact tested SHA: `698c6e901672a736f2803894efb5bda34af08212`.
- Run/job: `32821991333` / `97721878468`, Release, self-hosted Windows.
- Toolchain: .NET 10.0.400, `maui-android`, OpenJDK 21.0.12 baseline, Android API 36, Build-Tools 36.0.0, ADB 37.0.1, AVD `mobil-dwg-api36`.
- Executable markers: `STAGE04_CORE_CONTRACT_TESTS_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`.
- Emulator: Android 16/API 36/x86_64/QEMU=1; Stage01Smoke Release install/cold launch `Status: ok`; PID `3374`.
- Screenshot: full PNG signature `89 50 4E 47 0D 0A 1A 0A`.
- Crash/ANR: package/PID crash buffer temiz; lifecycle/draw events mevcut; `dumpsys activity lastanr` evidence kaydedildi.
- Artifact `9553530359`, digest `sha256:ad96924682330a93368c95889d75e8112dff8387170dcdeb17b17e3d72c8e7f7`.
- Claim limit: `INFRASTRUCTURE_SMOKE_ONLY`; gerçek viewer PASS değildir.
- Ayrıntı: `docs/evidence/android-validation/V01.md`.

### V02 — Dependency, lockfile ve Android artifact sınırı — `VALIDATED`

V02 tarihsel package/source/license evidence'ı güncel kodla karşılaştırdı ve “exact pin” iddiasında gerçek bir açıklık buldu: plain CPM sürümleri lockfile'da açık alt sınır (`[3.7.1, )`) üretiyordu. Strict exact NuGet range politikası uygulanarak düzeltildi.

Güncel exact baseline:

- ACadSharp `[3.7.1]`
- SkiaSharp `[4.151.1]`
- test/fallback-only IxMilia.Dxf `[0.8.4]`

Hardened audit şunları zorunlu kılar:

- tek target graph `net10.0-android36.0`;
- direct ACadSharp `3.7.1` + SkiaSharp `4.151.1` ve exact requested ranges;
- tek transitive SkiaSharp.NativeAssets.Android `4.151.1`;
- ProCad/IxMilia sızıntısı olmaması;
- production `src/**/*.csproj` içinde beklenmeyen PackageReference, iOS/MacCatalyst/Windows TFM veya `src/` dışına ProjectReference olmaması;
- `src/` altında vendored `.so/.aar/.jar/.dylib/.framework` bulunmaması;
- ACadSharp/SkiaSharp paketlerinde beklenmeyen native entry olmaması;
- SkiaSharp.NativeAssets.Android içinde yalnız Android arm/arm64/x64/x86 `libSkiaSharp.so` inventory'si;
- exact nupkg SHA-256, license allowlist, reproducible manifest ve NuGet vulnerability kontrolü.

Yetkili kalıcı workflow sonucu:

- Workflow: `Stage 02 Dependency Audit` — self-hosted Windows, `contents: read`.
- Run/job: `32824397251` / `97729154385` — `SUCCESS`.
- Branch head: `50694547e7be43e5ec414cc91b57cbd32faa3c54`.
- Checked-out PR merge ref: `549770192c181b30db8968cec5c6ac3c2407e133`.
- Required markers: `V02_TOOLCHAIN_PASS`, `V02_LOCKED_RESTORE_PASS`, `V02_EXACT_VERSION_POLICY_PASS`, `V02_ANDROID_BOUNDARY_PASS`, `STAGE02_PACKAGE_AUDIT_PASS`, `V02_PACKAGE_AUDIT_PASS`, `V02_VULNERABILITY_PASS`, `ANDROID_VALIDATION_V02_PASS`.
- NuGet vulnerability report: vulnerable package yok.
- Artifact `9554326162`, 6 dosya, 3039 byte; digest `sha256:921847d550b74b566ee056e8a45956db76e3213f892ca512df07eda77a6d504a`.
- Artifact indirildi; summary/resolved graph/vulnerability report incelendi.
- Emulator: V02 için gerekli değil; gerçek installable `MobilDwg.App` henüz yok, bu V04 kapsamıdır.
- Claim limit: dependency/lockfile/license/hash/vulnerability/source-boundary/Android native package boundary; viewer/APK PASS değildir.
- Ayrıntı: `docs/evidence/android-validation/V02.md`.

### V03 — Fixture, golden sözleşmesi ve Android test matrisi — `NOT_STARTED`

- Manifest, provenance, dual hash, private-ignore ve negatif fixture kontrolleri yeniden çalıştırılır.
- Emulator `E-API36` smoke slotu ile fiziksel Android slotları ayrılır.
- Gerçek müşteri/özel çizim ve private corpus repoya alınmaz.
- V04–V09'da kullanılacak redistributable sentetik/public DWG-DXF seti ve beklenen sonuçlar sabitlenir.

Çıkış: V04–V09'un kullanacağı redistributable Android smoke seti ve kanıt sözleşmesi hazırdır.

### V04 — Mimari ve gerçek Android uygulama kabuğu — `NOT_STARTED`

- AŞAMA 04 dependency sınırları, solution build'i ve Core/Rendering/Architecture executable marker'ları yeniden doğrulanır.
- Mevcut `src/MobilDwg.App`'in bugün `net10.0` sınıf kitaplığı olduğu kaydedilir.
- Android-only minimal installable `MobilDwg.App` MAUI shell kurulur; shared Core/Cad/Rendering sınırları korunur.
- Emulator gate geçici smoke yerine gerçek app APK'sını build/install/launch edecek şekilde geliştirilir.
- Gerçek package ID, launcher, PID, crash/ANR ve screenshot evidence alınır.

Çıkış: gerçek `MobilDwg.App` APK emulator üzerinde açılır.

### V05 — ACadSharp parser entegrasyonu — `NOT_STARTED`

- AŞAMA 05 parser/corpus/diagnostics executable testleri yeniden çalıştırılır.
- Gerçek app shell içinde küçük redistributable DWG ve sentetik DXF parser adapter yolundan geçirilir.
- Writer/save API kullanılmadığı ve original input immutable kaldığı doğrulanır.
- Başarı, kontrollü negatif sonuç ve log redaction evidence alınır.

Çıkış: gerçek Android app revision parser adapter yolunu çalıştırır; host-only PASS ile karıştırılmaz.

### V06 — Android FilePicker/SAF ve safe-open — `NOT_STARTED`

- AŞAMA 06 quota/disk/atomic-copy/generation/cancel/cleanup testleri yeniden çalıştırılır.
- Emulator Documents/provider yolundan gerçek app içinde küçük DWG/DXF seçilir.
- Açma, iptal, hızlı ikinci seçim, rotate, background/foreground, close/reopen ve cache cleanup denenir.
- Üreticiye özgü/fiziksel cihaz farkları `DEFERRED_PHYSICAL_ANDROID` kalır.

Çıkış: emulator üzerinde gerçek app safe-open yolu geçer; fiziksel Android farkı açık tutulur.

### V07 — ProCad NO-GO ve production graph izolasyonu — `NOT_STARTED`

- Tarihsel pinned SHA, precision fixture ve ADR 0002 yeniden okunur.
- ProCad'ın production ProjectReference/PackageReference/native graph'a girmediği otomatik doğrulanır.
- Survey origin `5,000,000 + 0.001` precision regression çalıştırılır.
- Reddedilmiş ProCad adayını emulator/iOS üzerinde yeniden kurmak gerekmez.

Çıkış: ProCad NO-GO ve custom scene kararı hâlâ kodla tutarlıdır.

### V08 — iOS tarihsel kaydı arşivle, Android sınırını doğrula — `SCOPE_ARCHIVED / ANDROID_GRAPH_CHECK_PENDING`

- AŞAMA 08 tarihsel evidence korunur; iOS workflow/Mac/simulator/iPhone testi çalıştırılmaz.
- Android production/CI graph'ında iOS workload/native asset zorunluluğu olmadığı doğrulanır.
- Shared katmanda gereksiz Android-only sızıntı varsa adapter sınırında düzeltilir; yeni iOS implementasyonu yazılmaz.

Çıkış: `SCOPE_ARCHIVED`; Android iOS blocker taşımadan ilerler ve gelecekte dönüş kapısı korunur.

### V09 — RenderScene, kamera ve diagnostics — `NOT_STARTED`

- AŞAMA 09 T0/T1, semantic snapshot, OCS/WCS, invalid geometry, overflow ve large-coordinate regresyonları yeniden çalıştırılır.
- Gerçek Android app shell'in Core/Cad/Rendering composition sınırı doğrulanır.
- AŞAMA 10 renderer işi erkenden yazılmaz; V09 yalnız scene/camera foundation ve Android runtime linkage smoke ile kapanır.
- Exact app SHA, host marker'ları ve gerekiyorsa emulator artifact'i kaydedilir.

Çıkış: Android V01–V09 revalidation kuyruğu temizlenir; implementation cursor AŞAMA 10+ hattında sürer.

## 8. V09 sonrasında çalışma kuralı

- Aktif implementation sırası AŞAMA 10–22, ardından Android-only AŞAMA 25–27'dir; AŞAMA 23–24 future iOS track'tir.
- Android runtime/UI/lifecycle/packaging/native davranış değiştiğinde anlamlı checkpoint'te gerçek app emulator gate çalıştırılır.
- Bilgisayar kapalıysa host-independent iş sürer; kanıtsız `DONE` yazılmaz.
- Fiziksel Android, emulatorun kanıtlayamadığı SAF/performance/cihaz çeşitliliği için release öncesi yeniden zorunludur.
- iOS yalnız yeni açık kullanıcı kararıyla etkinleşir.

## 9. Her doğrulama aşamasında güncellenecek kayıtlar

1. Bu dosyanın checkpoint'i ve ilgili VXX durumu.
2. `docs/evidence/android-validation/VXX.md`.
3. `gecmis.md` ve `DEVAM.md`.
4. Canonical planın yürütme checkpoint'i.
5. `docs/EXECUTION_LOG.md`.
6. Gerekirse exact SHA/workflow/marker içeren pending runtime queue.

Tarihsel `docs/evidence/STAGE_01.md`–`STAGE_09.md` silinmez veya yeni sonuç varmış gibi geriye dönük değiştirilmez.
