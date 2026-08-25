# mobil-dwg — Yeni sohbet için tek dosyalık handoff

Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` gerçek `main` HEAD'ini ve açık PR'ları doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `ANDROID_DOGRULAMA_PLANI.md`, `gecmis.md`, `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` ve son Android validation evidence dosyasını oku.
3. GitHub üzerinden çalışılıyor ve yerel terminal/ADB doğrudan yoksa bağlam `CHATGPT_REMOTE_GITHUB`; `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` okunur.
4. Android V01–V09 validation cursor'ı önceliklidir; implementation cursor ayrı tutulur.
5. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapatılır; aynı turda sonraki aşama başlatılmaz.
6. Emulator fiziksel cihaz değildir; `Stage01Smoke` gerçek viewer değildir; queued/zero-step workflow PASS değildir.
7. Her kapanış exact revision + run/job/artifact + claim limit ile kaydedilir.
8. Production dependency evidence olmadan yükseltilmez; ProCad production graph'a geri sokulmaz.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg` — private, default `main`.
- Aktif v1: Android-only, local/offline, read-only 2D DWG/DXF viewer.
- iOS: future option; aktif Android kapsamı dışında.
- v1 dışı: edit/save/export/cloud/account.

## Güncel checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
ANDROID_VALIDATION_PROGRAM: V01–V09
ANDROID_VALIDATION_CURRENT: V04 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01: VALIDATED — INFRASTRUCTURE_SMOKE_ONLY
V02: VALIDATED — dependency/lockfile/license/hash/vulnerability/Android-native boundary
V03: VALIDATED — fixture/provenance/golden/Android smoke-set contract
NEXT_ACTION: Yalnız V04'ü başlat — gerçek installable Android MobilDwg.App kabuğu + mimari/emulator gate; aynı turda V05'e geçme.
```

## V01 özeti

Evidence: `docs/evidence/android-validation/V01.md`.

- tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- toolchain + executable harness + Stage01Smoke emulator infrastructure PASS
- claim limit: `INFRASTRUCTURE_SMOKE_ONLY`; real viewer değil.

## V02 özeti

Evidence: `docs/evidence/android-validation/V02.md`.

- strict exact ACadSharp `[3.7.1]`, SkiaSharp `[4.151.1]`, test/fallback IxMilia.Dxf `[0.8.4]`
- locked restore + license/hash + vulnerability + production graph + Android native boundary PASS
- ProCad/iOS-only/unknown native sızıntısı yok
- claim limit: dependency/native boundary; viewer/fidelity değil.

## V03 özeti

Evidence: `docs/evidence/android-validation/V03.md`.

Authoritative final technical validation:

- branch head: `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- PR merge test revision: `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job: `32827625875` / `97739039060`
- artifact: `9555501552`
- digest: `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`

Ana marker'lar:

- `V03_TOOLCHAIN_AND_SYNTAX_PASS`
- `STAGE03_SYNTHETIC_DWG_PACKAGE_PASS`
- `STAGE03_SYNTHETIC_DWG_READBACK_PASS`
- `V03_ANDROID_SMOKE_SET_PASS ... formats=dwg,dxf`
- `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`
- `STAGE03_DUAL_HASH_PASS fixtures=6`
- `ANDROID_VALIDATION_V03_PASS`

V03'te düzeltilen önemli drift:

1. E-API36 device matrix V01 sonrası güncellenmemişti.
2. Hak durumu açık redistributable DWG smoke girdisi yoktu. Committed 0BSD sentetik DXF'den exact ACadSharp 3.7.1 generator ile AC1015 DWG validation-time üretilip read-back doğrulanıyor.
3. Windows persistent worktree CRLF dönüşümü committed DXF working-tree bytes'ını değiştirebiliyordu. `.gitattributes` eklendi; authoritative hash doğrulaması `HEAD:<path>` Git blob bytes üzerinden yapılıyor.
4. Generated DWG aynı 8021-byte yapı/magic/read-back sonucu verse de runlar arasında SHA değişti. Binary golden commit edilmedi; source + exact generator + magic + read-back + run-specific hash provenance sözleşmesi kullanılıyor.

Android smoke seti:

- committed 0BSD DXF `synthetic-turkish-basic-ac1015`
- generated 0BSD-source DWG `synthetic-turkish-basic-ac1015-dwg`
- committed negative DXF'ler: missing-font + missing-XREF

Claim limit: fixture/provenance/rights/golden/test-matrix. Real app/parser/renderer/emulator runtime/fiziksel Android PASS değildir.

## V04'te yapılacak iş — henüz başlanmadı

- AŞAMA 04 dependency yönleri ve tüm executable architecture/Core/Rendering harness'ları tekrar çalıştırılacak.
- Mevcut `src/MobilDwg.App` gerçekliği incelenecek.
- Android-only hedef için minimal gerçek MAUI `MobilDwg.App` installable APK kabuğu kurulacak.
- Core/Cad/Rendering sınırları korunacak.
- Emulator gate Stage01Smoke yerine gerçek app APK build/install/launch yapacak.
- Gerçek package ID, launcher, app PID, lifecycle, screenshot, crash/ANR evidence alınacak.
- Bu V04 yalnız gerçek app shell/runtime bridge'i kanıtlayacak; DWG/DXF parser fidelity V05'e bırakılacak.

V04 bu V03 kapanış turunda başlatılmadı.
