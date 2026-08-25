# AŞAMA 06 Evidence — Android güvenli dosya alma ve parse spike

> **Tarihsel kayıt:** Yeni Android emulator doğrulaması `ANDROID_DOGRULAMA_PLANI.md` V06 altında ayrı evidence üretir. Emulator sonucu fiziksel telefon gate'ini geriye dönük kapatmaz; bu dosyadaki eski commit/run gerçekleri değiştirilmez.

## Kimlik

- Tarih: 2026-08-24
- Aşama: AŞAMA 06
- Repo: `smitelagwar/mobil-dwg`
- Branch: `stage06-android-safe-open-spike`
- Başlangıç main: `b0262877b0273c5854e671c95e0c11601dfcd170`
- Doğrulanmış implementation/CI head: `56de020fb1297b8642c4f84c24522bbd723272f8`
- PR: `#8` — `stage06: validate Android safe file open flow`
- Ortam: GitHub hosted Ubuntu 24.04, .NET SDK/workload set `10.0.400`, Android API 36
- Aşama durumu: `BLOCKED / DEFERRED_EXTERNAL_GATE`

## Durumun anlamı

AŞAMA 06'nın fiziksel cihazdan bağımsız, otomatik doğrulanabilir kısmı tamamlandı ve CI'da geçti. Ancak canonical çıkış kriteri gerçek Android telefonda FilePicker/SAF üzerinden DWG ve DXF seçme, metadata/diagnostics gösterme, cancel/rotate/background/close davranışı ve gerçek cihaz temp/cache cleanup kanıtını gerektiriyor.

Kullanıcı şu an fiziksel Android cihaz + gerçek geliştirme makinesi sağlayamadığı için bu dış kapı `DEFERRED_EXTERNAL_GATE` olarak açık kalır. Bu belge AŞAMA 06'yı `DONE` saymaz. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` gereği bu eksik dış kapı, teknik olarak bağımsız AŞAMA 07 çalışmalarını tek başına engellemez.

## Uygulanan güvenli dosya alma hattı

- `CadFileSelection` provider fiziksel path'i değil `OpenReadAsync` benzeri stream factory taşır.
- Provider filename yalnız display metadata olarak ele alınır; path separator'ları atılır, basename sanitize edilir, yalnız `.dwg`/`.dxf` uzantıları korunur.
- Provider'ın bildirdiği dosya boyutuna güvenilmez. Bildirilen boyut yalnız erken ret için kullanılır; gerçek stream byte sayısı ayrıca sayılır ve quota uygulanır.
- Varsayılan stream quota `256 MiB`, boş alan reserve `32 MiB`; ileride AŞAMA 19/20 ölçümleriyle kesin resource guard değerleri yeniden ele alınacaktır.
- App-private cache içinde unique generation+GUID adıyla `.part` dosyası oluşturulur; başarılı flush sonrası aynı dizinde unique final ada taşınır.
- Copy error/cancel/quota/disk-space fail durumlarında `.part` ve final dosya deterministic temizlenir.
- Provider stream ve cache stream yaşam döngüsü sahiplik sınırında dispose edilir.
- Original kullanıcı DWG/DXF'si hiçbir zaman write modunda açılmaz veya değiştirilmez.
- Stage 06 immediate-copy akışında persistable URI grant alınmaz; kalıcı recent-file erişimi AŞAMA 18 kapsamındadır.

## Parse / concurrency davranışı

- Parse orchestration App katmanında parser-agnostic kalır; UI/App katmanına ACadSharp tipi sızmaz.
- Parse `Task.Run(..., CancellationToken.None)` içindeki worker hattında yürütülür; UI thread'i üzerinde senkron parser çalıştırılmaz.
- Reader'ın capability'si `BeforeStartOnly` olduğundan parser başladıktan sonra cooperative hard-stop vaat edilmez.
- Her open request monoton generation ID alır.
- Yeni seçim önceki request token'ını cancel-request olarak işaretler.
- Eski parser sonradan tamamlanırsa session/cache lease dispose edilir ve sonuç commit edilmez: `last request wins`.
- Kullanıcı cancel ister ve parser cooperative durmazsa task “parser durdu” diye erken tamamlanmaz; parser sonucu geldiğinde sonuç terk edilir ve lease temizlenir.

## Headless güvenlik/probe sonuçları

`tools/Stage06.OpenFlowProbe` final head üzerinde gerçek `AcadSharpDocumentReader` ve sentetik/fake non-cooperative reader ile şu kapıları geçti:

- `STAGE06_ACTUAL_DWG_DXF_PASS`
  - Stage 03 pinned AC1015 gerçek DWG
  - committed `synthetic_turkish_basic_ac1015.dxf`
  - safe private-copy → parse → metadata/diagnostics yolu
- `STAGE06_SAFE_COPY_GUARDS_PASS`
  - declared size quota early reject
  - provider'ın küçük boyut bildirmesine rağmen actual-byte quota reject
  - source stream disposal
  - free-space reserve reject
  - failure sonrası temp/final cache leak yok
- `STAGE06_LAST_REQUEST_WINS_PASS`
  - ilk parser cooperative cancel olmadan blokluyken ikinci request Ready olur
  - ilk geç sonuç `Superseded` olur ve handle/cache dispose edilir
- `STAGE06_CANCEL_SEMANTICS_PASS`
  - cancel request parser'ı sahte biçimde bitmiş saymaz
  - geç parser sonucu `Cancelled` disposition ile terk edilir
- `STAGE06_T2_HEADLESS_PASS`
- `STAGE06_STREAM_NOT_PATH_PASS`
- `STAGE06_NO_PERSISTABLE_GRANT_NEEDED_PASS`

Original DWG/DXF SHA-256 değerleri probe öncesi/sonrası aynı kaldı.

## MAUI Android FilePicker spike

`spikes/Stage06.Android/Stage06MainPage.cs` kaynak spike'ı temiz `dotnet new maui` uygulamasına CI sırasında enjekte edildi:

- `FilePicker.Default.PickAsync(...)`
- seçilen dosya için `FileResult.OpenReadAsync()`
- provider `FullPath` bağımlılığı yok
- app-private `FileSystem.Current.CacheDirectory` altına safe copy
- metadata/diagnostics/compatibility count UI özeti
- explicit cancel-request metni: parser başladıysa hard-stop sözü yok
- close ile session + cache cleanup
- broad external-storage permission eklenmedi

Persistable grant alınmadı; Stage 06'nın immediate private-copy modeli için gerekmedi. Kalıcı recent access ayrıca AŞAMA 18'de explicit olarak ele alınacak.

## Android build / manifest kanıtı

Final `Stage 06 Safe Open` run `32762879583` / #3:

- Sonuç: `SUCCESS`
- Solution Release build: `0 Warning(s)`, `0 Error(s)`
- Stage06 probe build: `0 Warning(s)`, `0 Error(s)`
- Generated MAUI Android Debug build: `0 Warning(s)`, `0 Error(s)` + `STAGE06_ANDROID_DEBUG_BUILD_PASS`
- Generated MAUI Android Release build: `0 Warning(s)`, `0 Error(s)` + `STAGE06_ANDROID_RELEASE_BUILD_PASS`
- Manifest: `minSdkVersion=24`, `targetSdkVersion=36`
- Package: `com.smitelagwar.mobildwg.stage06smoke`
- `READ_EXTERNAL_STORAGE`, `WRITE_EXTERNAL_STORAGE`, `MANAGE_EXTERNAL_STORAGE`: absent
- `STAGE06_NO_BROAD_STORAGE_PERMISSION_PASS`
- `STAGE06_CI_GATE_PASS`

Evidence artifact:

- Artifact ID: `9533538573`
- Digest: `sha256:18c7c395e24b6e3d686edef03d3d0ad686c21fad82686704ef38e7e098a25ea3`
- İçerik: Stage 06 evidence JSON, Stage 03 fixture audit, generated Android manifest, Debug/Release APK'lar.

## Aynı final head regresyonları

- `Stage 04 Architecture` run `32762879643` / #22: `SUCCESS`
- `Stage 02 Dependency Audit` run `32762879581` / #35: `SUCCESS`
- `Stage 01 Toolchain Smoke` run `32762879589` / #54: `SUCCESS`

Stage 01 CI fiziksel Android install/launch kanıtı değildir.

## CI sırasında yakalanan ve düzeltilen sorunlar

1. İlk head'de App kullanıcı mesajı `ACadSharp` adını içerdiği için Stage 04 architecture source-boundary guard fail verdi. Kod derlenmişti fakat mimari kural doğru şekilde ihlali yakaladı. App mesajı parser-agnostic hale getirildi.
2. İkinci Stage 06 koşusunda bütün headless safe-open probe'ları geçti; fakat static guard app-private cache root normalization için kullanılan `Path.GetFullPath` çağrılarını yanlışlıkla provider physical-path bağımlılığı saydı. Guard yalnız gerçek MAUI FilePicker adapter source'una daraltıldı; final #3 run geçti.

## Açık dış kapı — fiziksel Android

Aşağıdakiler gerçek fiziksel Android telefon + gerçek geliştirme makinesi olmadan kanıtlanamaz ve bu nedenle açık kalır:

- MAUI FilePicker/Android SAF UI'dan gerçek yerel DWG seçme ve metadata/diagnostics görme.
- Aynı şekilde gerçek DXF seçme ve metadata/diagnostics görme.
- Cancel UI smoke.
- Hızlı ikinci seçim / stale result'ın gerçek UI'da görünmemesi.
- Rotate sırasında crash/reparse davranışı.
- Background/foreground ve close/reopen.
- Gerçek cihazda app-private temp/cache leak kontrolü.
- Stage 01'in ayrı `STAGE01_DEVICE_GATE_PASS` install/launch kapısı.

Bu maddeler `PASS` veya `DONE` değildir.

## Karar / sonraki eylem

AŞAMA 06'nın cihazdan bağımsız implementation ve CI kısmı **PASS**; aşama bütünü **`BLOCKED / DEFERRED_EXTERNAL_GATE`**.

Kullanıcı onaylı execution override gereği PR #8 doğrulanmış head üzerinden `main`e merge edilebilir. Merge sonrası ilk bağımsız çalışma aşaması **AŞAMA 07 — ProCad source-pinned Android spike ve GO/NO-GO** olur. AŞAMA 06 fiziksel cihaz kapısı release/beta veya ilgili milestone öncesinde yeniden açılacaktır.
