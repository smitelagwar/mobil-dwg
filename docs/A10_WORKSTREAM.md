# AŞAMA 10 paralel çalışma hattı kaydı

Bu dosya, Android V04–V09 geriye dönük doğrulaması sürerken ayrı branch'te yapılabilecek sınırlı AŞAMA 10 taslak çalışmasının tek sahipli durum kaydıdır. Başlatma komutu `BASLA_A10.md dosyasını oku` şeklindedir.

## Güncel checkpoint

```text
WORKSTREAM: A10_P0_GEOMETRY_DRAFT
STATUS: DONE
BRANCH: stage10-p0-geometry-draft (MERGED)
PR: #23 (MERGED)
MAIN_MERGE_SHA: ddeb975
BASE_MAIN_SHA: 3ebf8226b8f133255e65cafdec9f7f26fbe7afbe
LOCAL_OR_AGENT_HOST_TESTS: PASS
ANDROID_EMULATOR: PASS (pixels=56163, PNG screenshot, PID=6257, no crash/ANR)
EVIDENCE: docs/evidence/STAGE_10.md
A11_GATE: OPEN
NEXT_ACTION: AŞAMA 10 tamamlandı (DONE). Sıradaki normal çalışma AŞAMA 11 (Mobil viewport ve gesture)'dir.
```

Bu başlangıç kaydı A10'un başladığı anlamına gelmez. Gerçek branch/commit oluştuğunda A10 sohbeti bu dosyayı kendi branch'inde günceller.

## Sahiplik

- A10 sohbeti: A10 source/test dosyaları, bu kayıt ve ileride `docs/evidence/STAGE_10.md`.
- Validation sohbeti: V04–V09 branch'leri, `android-test`, `ANDROID_DOGRULAMA_PLANI.md`, VXX evidence ve ortak checkpoint/handoff dosyaları.
- A10 sohbeti V04–V09 sürerken validation kayıtlarını değiştirmez; validation sohbeti de A10 draft branch'inde kod geliştirmez.

## Durum sözleşmesi

| Durum | Anlamı | `main` merge |
|---|---|---|
| `NOT_STARTED` | A10 branch/commit yok | Yasak |
| `IN_PROGRESS_UNVALIDATED` | A10 ayrı branch'te geliştiriliyor; executable test sonucu henüz yok | Yasak |
| `CODED_PENDING_HOST_TESTS` | Kod taslağı hazır; host/hosted test işi başlamadı veya dış CI blocker'ında | Yasak |
| `FIX_REQUIRED / FIX_IN_PROGRESS` | Actual test FAIL bulundu; exact failure kaydedildi ve düzeltme sürüyor | Yasak |
| `CODED_PENDING_EMULATOR` | Kod ve tüm zorunlu host/GitHub-hosted kontroller actual non-zero-step PASS; V04–V09 uzlaştırması ve Android integration/emulator kanıtı bekliyor | Yasak |
| `READY_TO_MERGE` | V04–V09 kapalı, güncel main entegre, exact A10 integration SHA tüm gerekli gate'lerden geçti | İzinli |
| `DONE` | Doğrulanmış PR main'e merge ve post-merge/evidence kapanışı tamam | Tamamlandı |

Zorunlu host/GitHub-hosted kontrol sonuçsuz/zero-step/external blocker ise `CODED_PENDING_HOST_TESTS`; actual FAIL ise `FIX_REQUIRED/FIX_IN_PROGRESS`; hepsi actual non-zero-step PASS olduğunda V04–V09 uzlaştırması ve Android integration/emulator kapılarını bekleyen durum `CODED_PENDING_EMULATOR`dır. Bunların hiçbiri `PASS`, `VALIDATED`, `READY_TO_MERGE` veya `DONE` değildir.

## Bekleyen Android test kaydı

Bekleyen iş varsa aşağıdaki alanları doldur; yoksa `ANDROID_EMULATOR: NOT_QUEUED` veya kapanışta `EMPTY` kullan:

```text
CURSOR: A10_DRAFT
SOURCE_BRANCH:
SOURCE_SHA:
BASE_MAIN_SHA:
WORKFLOW_OR_SCRIPT:
CONFIGURATION: Release
EXPECTED_MARKERS:
MERGE_ALLOWED: false
BLOCKED_BY: V04_V09_PROGRAM + LATEST_MAIN_INTEGRATION + A10_ANDROID_GATE
SUPERSEDED_BY: NONE
```

A10 draft SHA'sı, V09 sonrası güncel `main` ile oluşturulan integration SHA'nın yerine geçmez.

## Güvenli erken kapsam

- Yalnız yeni/internal ve platform-neutral primitive/tessellator matematiği.
- World coordinates `double`; GPU/batching/tiling yok, correctness first.
- V09 kapanana kadar `RenderSceneEntity`, `IRenderScene`/`ICadRenderer`, `render-scene/v1`, architecture beklentileri/docs, `.csproj`/Skia wiring ve fixture/image-golden sözleşmesi değişmez.
- ProCad, MAUI, FilePicker/SAF, lifecycle ve AŞAMA 11 gesture işi yok.
- Draw-order/clipping/antialias ve gerçek Skia entegrasyonu V09 sonrası integration turundadır.

## Merge öncesi uzlaştırma

V09 kapandıktan sonra:

1. Güncel doğrulanmış `main`, A10 draft branch'ine force kullanmadan alınır.
2. Validation değişiklikleriyle çatışma varsa validation sözleşmesi korunur ve A10 uyarlanır.
3. Etkilenen V02/V03, V04–V07, V08 Android graph-isolation, V09 ve A10 testleri exact integration SHA'da çalıştırılır; iOS workflow açılmaz.
4. Gerçek `MobilDwg.App` API 36 emulator render smoke; PID/PNG/crash/ANR yanında non-blank/expected-content pixel probe, Android golden veya kayıtlı görsel inceleme ile kanıtlanır.
5. Yalnız bundan sonra `READY_TO_MERGE`; main merge + post-merge evidence sonrasında `DONE` yazılır.
