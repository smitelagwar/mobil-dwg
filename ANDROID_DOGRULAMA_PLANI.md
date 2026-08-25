# mobil-dwg — Android geriye dönük doğrulama planı

Bu belge AŞAMA 01–09 arasında geliştirilen kodu Android hedefinde sırayla yeniden doğrulayan yetkili alt plandır. Ana ürün planı `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyasıdır. Tarihsel `docs/evidence/STAGE_XX.md` kayıtları değiştirilmez; yeni sonuçlar `docs/evidence/android-validation/VXX.md` altında tutulur.

## 1. Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 01–09 kodlandı; AŞAMA 09 tamamlandı
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_VALIDATION_STAGE: V04
CURRENT_STATUS: NOT_STARTED
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
NEXT_ACTION: Yalnız V04'ü başlat — mimari sınırlar + gerçek installable Android MobilDwg.App kabuğu; aynı turda V05'e geçme
PENDING_EMULATOR_QUEUE: EMPTY
```

iOS kodu ve tarihsel evidence korunur fakat kullanıcı iOS yolunu açıkça yeniden etkinleştirene kadar Mac/Xcode/iPhone/iOS workload/signing/simulator/App Store işi Android'i bloke etmez.

## 2. `BASLA.md` / `devam` protokolü

Kullanıcı `BASLA.md dosyasını oku` veya `devam` dediğinde ajan:

1. Gerçek `main` HEAD, açık PR ve checkpoint'i doğrular.
2. `BASLA.md`, bu dosya, canonical plan, `DEVAM.md`, `gecmis.md`, execution override ve çalışma bağlamına uygun Android test workflow'unu okur.
3. Açık VXX bitmediyse doğrudan onu sürdürür.
4. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatır; sonraki aşamayı aynı turda başlatmaz.
5. Emulator fiziksel cihaz sayılmaz; geçici `Stage01Smoke` gerçek viewer sayılmaz; queued/zero-step workflow PASS sayılmaz.
6. Test/evidence olmadan `VALIDATED/DONE` yazmaz.
7. Implementation cursor AŞAMA 10'da validation cursor'dan ayrı korunur.

## 3. Gerçeklik sınıfları

| Kanıt | Kanıtladığı | Kanıtlamadığı |
|---|---|---|
| Restore/build/executable harness | host kod/sözleşme | Android install/UI/runtime |
| Stage01Smoke emulator | runner/SDK/ADB/MAUI infrastructure | gerçek viewer/DWG-DXF işlevi |
| Gerçek MobilDwg.App emulator | test edilen Android app revision runtime akışı | fiziksel üretici/SAF/perf farkı |
| Fiziksel Android | kaydedilen gerçek cihaz senaryosu | test edilmemiş başka cihazlar |
| Fixture/provenance audit | input rights/hash/test sözleşmesi | parser/render fidelity |

## 4. Durumlar

`NOT_STARTED`, `CODE_AUDIT`, `FIX_REQUIRED`, `FIX_IN_PROGRESS`, `READY_FOR_EMULATOR`, `WAITING_RUNNER`, `VALIDATED`, `VALIDATED_WITH_DEFERRED_PHYSICAL`, `SCOPE_ARCHIVED`, `DEFERRED_PHYSICAL_ANDROID`, `BLOCKED`.

`READY_FOR_EMULATOR` ve `WAITING_RUNNER` PASS değildir.

## 5. Self-hosted runner kuralı

- Normal source `main` veya feature branch'tir.
- `android-test` yalnız test taşıyıcısıdır; product development branch'i değildir.
- Exact tested SHA/PR merge revision evidence'e yazılır.
- Runner çevrim dışıysa aynı queued işi çoğaltma; exact SHA/test `PENDING_EMULATOR_QUEUE` kaydına alınır.
- Force-push/force-ref update yapılmaz.
- Workflow `SUCCESS` yalnız gerçekten çalışan adımlar kadar güçlüdür.

## 6. Validation sırası

### V01 — Toolchain, runner ve emulator altyapısı — `VALIDATED`

Authoritative evidence: `docs/evidence/android-validation/V01.md`.

- Exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`.
- Run/job `32821991333` / `97721878468`.
- .NET 10.0.400, maui-android, OpenJDK 21.0.12 baseline, API 36, Build-Tools 36.0.0, ADB 37.0.1, AVD `mobil-dwg-api36`.
- Core/Rendering/Architecture executable harness marker'ları gerçekten çalıştı.
- Stage01Smoke Release APK install/cold launch, numeric PID, byte-safe PNG, crash/ANR evidence geçti.
- Claim limit: `INFRASTRUCTURE_SMOKE_ONLY`; gerçek MobilDwg.App/viewer PASS değildir.

### V02 — Dependency, lockfile ve Android artifact sınırı — `VALIDATED`

Authoritative evidence: `docs/evidence/android-validation/V02.md`.

- Strict exact NuGet ranges: ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`.
- Locked restore, exact graph, nupkg hash/license, vulnerability ve production `src/` boundary denetimi geçti.
- Android probe graph: ACadSharp 3.7.1 + SkiaSharp 4.151.1 + transitive SkiaSharp.NativeAssets.Android 4.151.1.
- ProCad/iOS-only/unknown native sızıntısı yok.
- Claim limit: dependency/native boundary; viewer/APK/fidelity PASS değildir.

### V03 — Fixture, golden sözleşmesi ve Android test matrisi — `VALIDATED`

Authoritative evidence: `docs/evidence/android-validation/V03.md`.

Final validation:

- tested branch head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`;
- PR merge test revision `1171807016e2deacc4f575b7980400b4f8b4708c`;
- run/job `32827625875` / `97739039060`;
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`.

Geçen ana marker'lar:

- `V03_TOOLCHAIN_AND_SYNTAX_PASS`
- `STAGE03_SYNTHETIC_DWG_PACKAGE_PASS`
- `STAGE03_SYNTHETIC_DWG_READBACK_PASS`
- `V03_ANDROID_SMOKE_SET_PASS ... formats=dwg,dxf`
- `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`
- `STAGE03_DUAL_HASH_PASS fixtures=6`
- `ANDROID_VALIDATION_V03_PASS`

V03'te bulunan ve düzeltilen drift:

1. E-API36 device matrix stale `V01_FIX_REQUIRED` durumundaydı; V01 gerçekliğiyle hizalandı.
2. Redistributable DWG smoke girdisi yoktu; committed 0BSD DXF'den exact ACadSharp 3.7.1 generator ile validation-time AC1015 DWG üretim/read-back sözleşmesi kuruldu.
3. Windows self-hosted worktree CRLF normalization committed DXF working-tree boyutunu değiştirebiliyordu. `.gitattributes` eklendi ve authoritative hash doğrulaması doğrudan `HEAD:<path>` Git blob bytes üzerinden yapıldı.
4. Generated DWG output hash'i runlar arasında değiştiği için binary golden olarak commit edilmedi; source + exact generator + AC1015 magic + DwgReader read-back + run-specific hash provenance sözleşmesi seçildi.

Claim limit: fixture/provenance/rights/golden/test-matrix sözleşmesi; parser, renderer, real app veya physical Android PASS değildir.

### V04 — Mimari ve gerçek Android uygulama kabuğu — `NOT_STARTED`

Amaç: Stage01Smoke ile gerçek viewer arasındaki en kritik boşluğu kapatmak.

Zorunlu işler:

- AŞAMA 04 dependency yönlerini ve tüm executable Core/Rendering/Architecture harness marker'larını yeniden çalıştır.
- Mevcut `src/MobilDwg.App` gerçekliğini oku; bugün installable MAUI Android app değilse bunu açıkça kaydet.
- Android-only aktif hedef için minimal installable `MobilDwg.App` MAUI shell kur; Core/Cad/Rendering sınırlarını koru.
- Gerçek package ID/launcher üret; Stage01Smoke'u viewer sonucu olarak kullanmayı bırak.
- Emulator gate'i gerçek MobilDwg.App APK build/install/launch için geliştir. Infrastructure smoke gerekirse ayrı mod olarak kalabilir.
- Exact app process PID, screenshot, lifecycle, crash/ANR kanıtı al.
- Emulator dışında kalan fiziksel cihaz farkını release matrisi için açık tut.

Çıkış: gerçek `MobilDwg.App` APK test edilen exact revision'da E-API36 üzerinde açılır. Viewer fidelity henüz V05+ kapsamıdır.

### V05 — ACadSharp parser entegrasyonu — `NOT_STARTED`

- AŞAMA 05 parser/corpus/diagnostics executable testlerini yeniden çalıştır.
- Gerçek Android app içinde V03 smoke setinden en az bir DWG ve bir DXF parse yolu çağrılır.
- Writer/save production graph'a girmez; original input immutable kalır.
- Pozitif parse + kontrollü negatif + redacted diagnostic kanıtı alınır.
- Host-only parser PASS ile Android app parse PASS karıştırılmaz.

Çıkış: gerçek app revision parser adapter yolunu Android üzerinde çalıştırır.

### V06 — Android FilePicker/SAF ve safe-open — `NOT_STARTED`

- AŞAMA 06 quota/disk/atomic-copy/generation/cancel/cleanup testleri yeniden çalışır.
- Emulator Documents/provider yolundan gerçek app ile küçük DWG/DXF seçilir.
- Açma, cancel, hızlı ikinci seçim, rotate, background/foreground, close/reopen, cleanup denenir.
- Üreticiye özgü SAF/fiziksel cihaz farkları `DEFERRED_PHYSICAL_ANDROID` kalır.

Çıkış: emulator üzerinde real-app safe-open PASS; fiziksel fark açık.

### V07 — ProCad NO-GO ve production graph izolasyonu — `NOT_STARTED`

- ADR 0002 ve pinned source kararı yeniden okunur.
- ProCad'ın production ProjectReference/PackageReference/native graph'a girmediği otomatik doğrulanır.
- `5,000,000 + 0.001` precision regresyonu çalışır.
- Reddedilmiş ProCad adayını emulator üzerinde tekrar kurma.

Çıkış: NO-GO ve custom scene yolu kodla hâlâ tutarlı.

### V08 — iOS tarihsel arşiv / Android sınırı — `SCOPE_ARCHIVED / ANDROID_GRAPH_CHECK_PENDING`

- AŞAMA 08 historical evidence korunur; iOS workflow/Mac/simulator/iPhone testi çalıştırılmaz.
- Android production/CI graph'ında iOS workload/native zorunluluğu olmadığı doğrulanır.
- Shared katman Android-only sızıntı taşıyorsa adapter sınırında düzeltilir; yeni iOS implementasyonu yazılmaz.

Çıkış: `SCOPE_ARCHIVED`; iOS Android blocker'ı değildir.

### V09 — RenderScene, kamera ve diagnostics — `NOT_STARTED`

- AŞAMA 09 T0/T1, semantic snapshot, OCS/WCS, invalid geometry, overflow, large-coordinate regresyonları yeniden çalıştır.
- Real app Core/Cad/Rendering composition sınırını doğrula.
- AŞAMA 10 renderer işi erkenden yazılmaz.

Çıkış: AŞAMA 01–09 Android revalidation kuyruğu temiz; implementation cursor AŞAMA 10'dan sürer.

## 7. V09 sonrası uygulama sırası

Aktif sıra AŞAMA 10–22, ardından Android-only AŞAMA 25–27. AŞAMA 23–24 future iOS track'tir. Android runtime/UI/packaging değişikliklerinde anlamlı checkpoint'te gerçek app emulator gate çalıştırılır. Fiziksel Android AŞAMA 20–22 ve final release kapılarında tekrar zorunludur.

## 8. Her validation kapanışında güncellenecek kayıtlar

1. Bu dosyanın current VXX checkpoint'i.
2. `docs/evidence/android-validation/VXX.md`.
3. `DEVAM.md` ve `gecmis.md`.
4. Canonical plan checkpoint'i.
5. `docs/EXECUTION_LOG.md` kısa teknik kayıt.
6. Pending Android/emulator işi varsa exact SHA/workflow/expected marker.

Tarihsel `docs/evidence/STAGE_01.md`–`STAGE_09.md` geriye dönük yeniden yazılmaz.
