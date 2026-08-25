# mobil-dwg — Android geriye dönük doğrulama planı

Bu belge AŞAMA 01–09 arasında geliştirilen kodu Android hedefinde sırayla yeniden doğrulayan yetkili alt plandır. Ana ürün planı `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` dosyasıdır. Tarihsel `docs/evidence/STAGE_XX.md` kayıtları değiştirilmez; yeni sonuçlar `docs/evidence/android-validation/VXX.md` altında tutulur.

## 1. Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_CURSOR: AŞAMA 10 — MAIN'E HENÜZ MERGE EDİLMEDİ
IMPLEMENTATION_WORKSTREAM: docs/A10_WORKSTREAM.md + varsa açık A10 branch/PR
ACTIVE_PROGRAM: ANDROID_REVALIDATION_01_09
CURRENT_VALIDATION_STAGE: V05
CURRENT_STATUS: NOT_STARTED
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — DEPENDENCY/LOCKFILE/LICENSE/HASH/VULNERABILITY/ANDROID-NATIVE BOUNDARY
V03: VALIDATED — FIXTURE/PROVENANCE/GOLDEN/ANDROID-SMOKE-SET CONTRACT
V04: VALIDATED — REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY
NEXT_ACTION: Yalnız V05'i başlat — ACadSharp parser adapter yolunu gerçek MobilDwg.App içinde V03 DWG/DXF smoke setiyle Android üzerinde doğrula; aynı turda V06'ya geçme
NEXT_IF_TEST_READY: V05 validation hattını yürüt
NEXT_IF_TEST_OFFLINE: BASLA_A10.md ile yalnız ayrı branch'te A10 host-independent taslağını yürüt
A10_MAIN_MERGE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_ANDROID_GATE
A11_GATE: BLOCKED_UNTIL_V04_V09_CLOSED_AND_A10_DONE_ON_MAIN_AND_EMULATOR_QUEUE_EMPTY
PENDING_EMULATOR_QUEUE: EMPTY
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
```

iOS kodu ve tarihsel evidence korunur fakat kullanıcı iOS yolunu açıkça yeniden etkinleştirene kadar Mac/Xcode/iPhone/iOS workload/signing/simulator/App Store işi Android'i bloke etmez.

## 2. `BASLA.md` / `devam` protokolü

Kullanıcı `BASLA.md dosyasını oku` veya `devam` dediğinde ajan:

1. Gerçek `main` HEAD, açık PR ve checkpoint'i doğrular.
2. `BASLA.md`, bu dosya, canonical plan, `DEVAM.md`, `gecmis.md`, execution override ve çalışma bağlamına uygun Android test workflow'unu okur.
3. Genel `BASLA.md` komutunda açık VXX bitmediyse doğrudan onu sürdürür. Ayrı A10 sohbeti yalnız kullanıcı `BASLA_A10.md` komutunu verdiğinde açılır.
4. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatır; sonraki aşamayı aynı turda başlatmaz.
5. Emulator fiziksel cihaz sayılmaz; geçici `Stage01Smoke` gerçek viewer sayılmaz; queued/zero-step workflow PASS sayılmaz.
6. Test/evidence olmadan `VALIDATED/DONE` yazmaz.
7. Implementation cursor AŞAMA 10'da validation cursor'dan ayrı korunur. Erken A10 yalnız izole draft branch'inde ilerleyebilir; VXX checkpoint/evidence dosyalarını değiştiremez.
8. Exact tested SHA/PR merge revision, run/job ve artifact evidence'e yazılır.

## 3. Gerçeklik sınıfları

| Kanıt | Kanıtladığı | Kanıtlamadığı |
|---|---|---|
| Restore/build/executable harness | host kod/sözleşme | Android install/UI/runtime |
| Stage01Smoke emulator | runner/SDK/ADB/MAUI infrastructure | gerçek viewer/DWG-DXF işlevi |
| Gerçek MobilDwg.App emulator | test edilen Android app revision runtime akışı | fiziksel üretici/SAF/perf farkı |
| Fiziksel Android | kaydedilen gerçek cihaz senaryosu | test edilmemiş başka cihazlar |
| Fixture/provenance audit | input rights/hash/test sözleşmesi | parser/render fidelity |

## 4. Durumlar

`NOT_STARTED`, `CODE_AUDIT`, `FIX_REQUIRED`, `FIX_IN_PROGRESS`, `IN_PROGRESS_UNVALIDATED`, `CODED_PENDING_HOST_TESTS`, `CODED_PENDING_EMULATOR`, `READY_FOR_EMULATOR`, `WAITING_RUNNER`, `VALIDATED`, `VALIDATED_WITH_DEFERRED_PHYSICAL`, `SCOPE_ARCHIVED`, `DEFERRED_PHYSICAL_ANDROID`, `BLOCKED`.

`IN_PROGRESS_UNVALIDATED`, `CODED_PENDING_HOST_TESTS`, `CODED_PENDING_EMULATOR`, `READY_FOR_EMULATOR` ve `WAITING_RUNNER` PASS değildir.

## 5. Self-hosted runner kuralı

- Normal source `main` veya feature branch'tir.
- `android-test` yalnız test taşıyıcısıdır; product development branch'i değildir.
- Exact tested SHA/PR merge revision evidence'e yazılır.
- Runner çevrim dışıysa aynı queued işi çoğaltma; exact SHA/test `PENDING_EMULATOR_QUEUE` kaydına alınır.
- Force-push/force-ref update yapılmaz.
- Bir feature head `android-test` taşıyıcısıyla test edildiyse PR merge yöntemi varsayılan olarak **merge commit** olmalıdır. Squash/rebase tested head'i `main` ancestry'sinden çıkarıp sonraki fast-forward taşıyıcı güncellemesini bozabilir. Merge commit kullanılamıyorsa force uygulanmaz; exact-ref `workflow_dispatch` veya güvenli başka bir tetikleme yolu seçilir.
- Workflow `SUCCESS` yalnız gerçekten çalışan adımlar kadar güçlüdür.
- GitHub-hosted job `steps=[]`, `runner_id=0`, boş runner adı ile biterse bu runner-allocation failure'dır; kod/test failure olarak sınıflandırılmaz.

### 5.1 Bilgisayar kapalıyken sınırlı A10 hattı

- Validation hattı V04→V09 sırasını korur ve `main`/VXX evidence üzerinde yetkilidir.
- Kullanıcı zaman kaybetmemek için ayrı sohbette `BASLA_A10.md dosyasını oku` diyebilir. Bu sohbet yalnız `stage10-p0-geometry-draft` normal feature branch'inde çalışır.
- Erken A10 yalnız yeni/internal platform-neutral primitive-tessellator matematiği ve saf testlerdir. V09 kapanana kadar mevcut RenderScene/interface/snapshot, architecture, `.csproj`/Skia ve fixture/image-golden sözleşmeleri dondurulur; ProCad, MAUI/FilePicker/lifecycle ve A11 kapsam dışıdır.
- A10 PR yoksa PC offline iken normal branch push yapılabilir. PR zaten açıksa push `synchronize` olayıyla V04 self-hosted işini açabileceğinden offline push öncesi workflow etkisi giderilir/PR kapatılır; aksi halde push yapılmaz. Runner hazırken PR açılır/güncellenir ve actual non-zero-step hosted/self-hosted sonuç doğrulanır. Billing/capacity blocker'ında kod `CODED_PENDING_HOST_TESTS` kalır.
- Host testleri geçti fakat emulator yoksa A10 en fazla `CODED_PENDING_EMULATOR` olur. `android-test` branch'ini A10 sohbeti hareket ettirmez; `main` merge, `READY_TO_MERGE`, `DONE` ve A11 yasaktır.
- A10 durumu/branch/SHA/test borcu `docs/A10_WORKSTREAM.md` içinde tutulur. A10 draft SHA, V09 sonrası güncel `main` ile oluşturulacak integration SHA'nın kanıtı değildir.
- V09 kapandıktan sonra güncel validated `main` A10 branch'ine alınır; etkilenen V02/V03, V04–V07, V08 Android graph-isolation, V09, A10 T1/golden/C3 ve gerçek-app API 36 emulator render gate exact integration SHA'da geçmeden merge yapılmaz. iOS workflow açılmaz; render kanıtı PID/PNG yanında expected-content/golden/görsel doğrulamadan en az birini içerir.

## 6. Validation sırası

### V01 — Toolchain, runner ve emulator altyapısı — `VALIDATED`

Authoritative evidence: `docs/evidence/android-validation/V01.md`.

- exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- .NET 10.0.400, maui-android, OpenJDK 21.0.12, API 36, Build-Tools 36.0.0, ADB 37.0.1
- Core/Rendering/Architecture executable harness marker'ları PASS
- Stage01Smoke install/cold-launch/PID/PNG/crash-ANR PASS
- claim limit `INFRASTRUCTURE_SMOKE_ONLY`

### V02 — Dependency, lockfile ve Android artifact sınırı — `VALIDATED`

Authoritative evidence: `docs/evidence/android-validation/V02.md`.

- ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`
- locked restore, exact graph, nupkg hash/license, vulnerability ve production `src/` boundary PASS
- Android probe graph: ACadSharp 3.7.1 + SkiaSharp 4.151.1 + SkiaSharp.NativeAssets.Android 4.151.1
- ProCad/iOS-only/unknown native sızıntısı yok
- claim limit dependency/native boundary

### V03 — Fixture, golden sözleşmesi ve Android test matrisi — `VALIDATED`

Authoritative evidence: `docs/evidence/android-validation/V03.md`.

Final validation:

- branch head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- PR merge test revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060`
- artifact `9555501552`
- digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`
- redistributable Android smoke set: committed 0BSD DXF + validation-time AC1015 DWG + missing-font/missing-XREF negative DXF
- committed fixture hash evidence Git blob bytes'a dayanır
- generated DWG writer/read-back smoke evidence'dir; independent engineering-fidelity golden değildir
- marker `ANDROID_VALIDATION_V03_PASS`

### V04 — Mimari ve gerçek Android uygulama kabuğu — `VALIDATED`

Authoritative evidence: `docs/evidence/android-validation/V04.md`.

V04 başlangıcında `MobilDwg.App` yalnız `net10.0` platform-neutral projeydi; installable Android app yoktu. Aynı dördüncü production proje Android-only .NET MAUI executable'a dönüştürüldü; yeni production proje açılmadı.

Gerçek app:

- target `net10.0-android36.0`
- package `com.smitelagwar.mobildwg`
- `MainActivity` + `MainApplication`
- Core/Cad/Rendering dependency yönleri korunuyor
- `Microsoft.Maui.Controls` exact `[10.0.100]`, MIT

Final authoritative validation:

- branch head `227ffa49c3095c4328f146acf1a2d9ecc07eb62d`
- tested PR synthetic merge revision `6201be929a636b963235f7da8ee72b0bbf9decf2`
- run/job `32832142832` / `97752997848` — SUCCESS
- same-head V02 regression run/job `32832142882` / `97752998222` — SUCCESS
- artifact `9557331919`, digest `sha256:0ccdb5028b417212f6d428475e8793ebc9d3a8018164c63b2703228dda00c0b4`
- real APK `com.smitelagwar.mobildwg-Signed.apk`, 30,827,130 byte
- APK SHA-256 `60d8d59b3fd452d786519a364875b155d3961c3e4aa210f986c004098789ba42`
- launcher `com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
- cold launch `Status: ok`, PID `3783`
- UI hierarchy, byte-safe PNG, package/PID crash/ANR ve process liveness PASS
- final marker `ANDROID_VALIDATION_V04_PASS`
- claim limit `REAL_APP_SHELL_RUNTIME_ONLY_NOT_VIEWER_FIDELITY`

V04 parser/render fidelity kanıtlamaz ve fiziksel Android release/device kapısı açık kalır.

### V05 — ACadSharp parser entegrasyonu — `NOT_STARTED`

- AŞAMA 05 parser/corpus/diagnostics executable testlerini yeniden çalıştır.
- Gerçek Android app içinde V03 smoke setinden en az bir DWG ve bir DXF parse yolu çağrılır.
- Android üzerinde gerçek `ICadDocumentReader` / ACadSharp adapter yolu çalıştığı kanıtlanır; host-only parser PASS yeterli değildir.
- Writer/save production graph'a girmez; original input immutable kalır.
- Pozitif parse + kontrollü negatif + redacted diagnostic kanıtı alınır.
- Fixture provenance/hashes V03 contract'ına bağlı kalır.

Çıkış: gerçek app revision parser adapter yolunu Android üzerinde DWG ve DXF ile çalıştırır; V06 aynı turda başlatılmaz.

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
- Ayrı A10 draft branch'i varsa validation sözleşmesi ona göre değiştirilmez. V09 sonucu üstün kabul edilir; draft güncel validated `main` ile daha sonra uzlaştırılır.

Çıkış: AŞAMA 01–09 Android revalidation kuyruğu temiz. A10 başlamadıysa normal sırada açılır; draft varsa güncel `main` ile integration + Android gate aşamasına alınır. Bu V09 kapanış turunda A10 merge/DONE veya A11 başlangıcı yapılmaz.

## 7. V09 sonrası uzlaştırma ve uygulama sırası

Aktif sıra AŞAMA 10–22, ardından Android-only AŞAMA 25–27. AŞAMA 23–24 future iOS track'tir. A10 draft varsa önce `docs/A10_WORKSTREAM.md` merge kapısı tamamlanır; A10 `DONE` olmadan A11 açılmaz. Android runtime/UI/packaging değişikliklerinde anlamlı checkpoint'te gerçek app emulator gate çalıştırılır. Fiziksel Android AŞAMA 20–22 ve final release kapılarında tekrar zorunludur.

## 8. Her validation kapanışında güncellenecek kayıtlar

1. Bu dosyanın current VXX checkpoint'i.
2. `docs/evidence/android-validation/VXX.md`.
3. `DEVAM.md` ve `gecmis.md`.
4. Canonical plan checkpoint'i.
5. `docs/EXECUTION_LOG.md` kısa teknik kayıt.
6. Pending Android/emulator işi varsa exact SHA/workflow/expected marker.

Tarihsel `docs/evidence/STAGE_01.md`–`STAGE_09.md` geriye dönük yeniden yazılmaz.
