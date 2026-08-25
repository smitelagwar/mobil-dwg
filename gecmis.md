# mobil-dwg — Proje geçmişi ve AI handoff kaydı

Bu dosya kısa kalıcı tarihçe/checkpoint kaydıdır. Ayrıntılı teknik kanıt `docs/evidence/`, kararlar `docs/ADR/`, aktif Android doğrulama sırası `ANDROID_DOGRULAMA_PLANI.md` içindedir.

## Yeni ajan okuma sırası

1. Gerçek `main` HEAD ve açık PR'ları doğrula.
2. `BASLA.md`.
3. `DEVAM.md`.
4. `ANDROID_DOGRULAMA_PLANI.md`.
5. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`.
6. Son `docs/evidence/android-validation/VXX.md` ve gerektiğinde tarihsel `docs/evidence/STAGE_XX.md` / `docs/ADR/`.
7. `docs/USER_APPROVED_EXECUTION_OVERRIDE.md`; remote bağlamdaysa `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md`.

## Repo / ürün

- GitHub: `smitelagwar/mobil-dwg` — private, default `main`.
- Aktif v1: Android-only, local/offline, read-only 2D DWG/DXF viewer.
- iOS future option; aktif Android DoD/sırası dışında.
- v1 dışında: edit/save/export/cloud/account.

## Aktif checkpoint

```text
ACTIVE_PRODUCT_TARGET: ANDROID_ONLY
IOS_STATUS: DEFERRED_FUTURE_OPTION
IMPLEMENTATION_BASELINE: AŞAMA 09 — DONE
IMPLEMENTATION_NEXT: AŞAMA 10 — NOT_STARTED
ANDROID_VALIDATION_CURRENT: V04 — NOT_STARTED
PENDING_EMULATOR_QUEUE: EMPTY
V01_EVIDENCE: docs/evidence/android-validation/V01.md; run 32821991333; job 97721878468; artifact 9553530359
V02_EVIDENCE: docs/evidence/android-validation/V02.md; run 32824397251; job 97729154385; artifact 9554326162
V03_EVIDENCE: docs/evidence/android-validation/V03.md; tested head 69e4e842b5426d71453f5f69a01ebba5948d6b9c; PR merge test revision 1171807016e2deacc4f575b7980400b4f8b4708c; run 32827625875; job 97739039060; artifact 9555501552
PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE
NEXT_ACTION: Yalnız V04'ü başlat — real Android MobilDwg.App shell + mimari/emulator gate; aynı turda V05'e geçme.
LAST_UPDATE: 2026-08-25
```

## Yürütme kuralı

Android V01–V09 validation cursor'ı implementation cursor'dan ayrıdır. Bir kullanıcı turunda en fazla bir validation veya implementation aşaması kapanır. Kanıtsız PASS/DONE yoktur. Emulator fiziksel Android değildir. Stage01Smoke gerçek viewer değildir. iOS Android hattını bloke etmez.

## Implementation geçmişi

- AŞAMA 00 — çalışma/yürütme zemini — `DONE`.
- AŞAMA 01 — pinned Android toolchain; fiziksel telefon dış kapısı — `BLOCKED / DEFERRED_EXTERNAL_GATE`.
- AŞAMA 02 — dependency/lisans/lockfile — `DONE`.
- AŞAMA 03 — corpus/golden/matris — `DONE`.
- AŞAMA 04 — minimal solution/mimari sınırlar — `DONE`.
- AŞAMA 05 — ACadSharp parser spike — `DONE`; ADR 0001 `GO`.
- AŞAMA 06 — safe-open implementation; fiziksel FilePicker/SAF kapısı deferred.
- AŞAMA 07 — ProCad exact source spike — `DONE / NO-GO`; ADR 0002.
- AŞAMA 08 — iOS characterization — historical/future; iOS PASS iddiası yok.
- AŞAMA 09 — immutable RenderScene/kamera/diagnostics foundation — `DONE`; authoritative run `32815175055`, artifact `9551137293`, merge `0a2dd886bbe59698a6d2eb4c99f66e7f9270063a`.
- AŞAMA 10 — P0 geometri renderer — `NOT_STARTED`.
- AŞAMA 11–22 — Android viewer/release hattı.
- AŞAMA 23–24 — `DEFERRED_FUTURE_IOS`.
- AŞAMA 25–27 — Android beta/freeze/final handoff.

## Android revalidation geçmişi

### V01 — VALIDATED

Başlangıç emulator gate'i executable harness, screenshot byte güvenliği, PID ve crash/ANR evidence açısından yetersizdi. Sertleştirildi.

- exact tested SHA `698c6e901672a736f2803894efb5bda34af08212`
- run/job `32821991333` / `97721878468`
- artifact `9553530359`
- claim limit `INFRASTRUCTURE_SMOKE_ONLY`

Stage01Smoke yalnız toolchain/runner/emulator/ADB/MAUI infrastructure kanıtıdır. Ayrıntı `docs/evidence/android-validation/V01.md`.

### V02 — VALIDATED

Tarihsel “exact pin” gerçekte NuGet open-lower-bound request üretiyordu. Strict exact ranges getirildi:

- ACadSharp `[3.7.1]`
- SkiaSharp `[4.151.1]`
- test/fallback IxMilia.Dxf `[0.8.4]`

Locked restore, license/hash, vulnerability, production `src/` boundary ve Android native inventory gate'i self-hosted Windows üzerinde geçti. ProCad/iOS-only/unknown native sızıntısı yok.

- authoritative run/job `32824397251` / `97729154385`
- tested PR merge ref `549770192c181b30db8968cec5c6ac3c2407e133`
- artifact `9554326162`
- claim limit dependency/native boundary

Ayrıntı `docs/evidence/android-validation/V02.md`.

### V03 — VALIDATED

V03 güncel repo gerçekliğiyle Stage 03 corpus/golden/test-matrix sözleşmesini yeniden denetledi.

Bulunan drift:

1. E-API36 device matrix V01 sonrası hâlâ `V01_FIX_REQUIRED` gösteriyordu.
2. Remote upstream DWG'ler bilinçli `remote-reference-only` olduğundan daha sonraki Android validation için redistributable DWG smoke girdisi yoktu.
3. Windows persistent self-hosted worktree'de Git LF → CRLF materialization committed DXF working-tree bytes'ını 769'dan 985'e çıkarabiliyordu.

Düzeltmeler:

- Device matrix V01 `INFRASTRUCTURE_SMOKE_ONLY` gerçekliğiyle hizalandı.
- `.gitattributes` ile CAD bytes normalization politikası tanımlandı.
- Committed-fixture authoritative hash kontrolü platform working tree yerine doğrudan `HEAD:<path>` Git blob bytes üzerinden çalışır.
- Manifest/schema `generated_fixtures` + `android_smoke_set` sözleşmesi aldı.
- Committed 0BSD DXF `synthetic-turkish-basic-ac1015` source'undan exact ACadSharp `3.7.1` generator ile AC1015 DWG validation-time üretiliyor; magic ve DwgReader read-back zorunlu.
- Generated DWG hash'inin runlar arasında değiştiği gözlendi. Bu yüzden output binary golden olarak commit edilmedi; source + exact generator/package + magic/read-back + run-specific hash provenance yolu seçildi.

Final authoritative teknik validation:

- branch head `69e4e842b5426d71453f5f69a01ebba5948d6b9c`
- PR merge test revision `1171807016e2deacc4f575b7980400b4f8b4708c`
- run/job `32827625875` / `97739039060` — `SUCCESS`
- artifact `9555501552`, digest `sha256:d964063ba786c61bccdbdbd1c184cf0023e35ee44a1e4b8d33986f1ddebac23a`

Marker'lar:

- `V03_TOOLCHAIN_AND_SYNTAX_PASS`
- `STAGE03_SYNTHETIC_DWG_PACKAGE_PASS`
- `STAGE03_SYNTHETIC_DWG_READBACK_PASS`
- `V03_ANDROID_SMOKE_SET_PASS ... formats=dwg,dxf`
- `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2`
- `STAGE03_DUAL_HASH_PASS fixtures=6`
- `ANDROID_VALIDATION_V03_PASS`

V03 emulator/app/parser/render fidelity testi değildir. Ayrıntı `docs/evidence/android-validation/V03.md`.

## Kalıcı teknik kararlar

- Original CAD immutable; production writer/save yok.
- ACadSharp `3.7.1` read-only parser baseline `GO`.
- Exact unpatched ProCad production reuse `NO-GO`.
- UI parser entity'lerine doğrudan bağlanmaz.
- World/document coordinate hattı `double` precision.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; unknown/policy-RED release blocker.
- Production dependency strict exact range + lockfile/locked restore kullanır.
- Fixture hash evidence Git blob bytes'a dayanır; platform line-ending dönüşümü manifesti değiştirmez.
- Fiziksel Android release öncesi yeniden zorunlu.
- iOS yalnız açık yeni kullanıcı kararıyla etkinleşir.
