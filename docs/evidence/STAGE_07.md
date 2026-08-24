# AŞAMA 07 Evidence — ProCad source-pinned Android spike ve GO/NO-GO

## Kimlik

- Tarih: 2026-08-24
- Aşama: AŞAMA 07
- Repo: `smitelagwar/mobil-dwg`
- Branch: `stage07-procad-source-spike`
- Başlangıç main: `e3a9c36e04be6c51827926ca17bb1a386c6b1142`
- Final decision head: `3f88bec383de895e309e218c08d13e9784562a97`
- PR: `#9` — `stage07: evaluate pinned ProCad Android reuse`
- Exact ProCad source candidate: `f8a862b3e7634e27664fee02ff5d68774b102985`
- Exact ACadSharp submodule: `0ed79df48de0806af3c3028d0e2826447cbc1d36`
- Exact ProEdit submodule: `64759b79289a024d08463ed1a9094fdcd9a270df`
- Ortam: GitHub hosted Ubuntu 24.04, .NET SDK/workload set `10.0.400`, Microsoft OpenJDK `21.0.12`, Android API 36 / Build-Tools 36.0.0

## Amaç ve sınır

Bu spike hazır ProCad renderer/control reuse'unu ölçmek içindir. Production solution'a ProCad PackageReference veya ProjectReference eklenmedi. Exact pinned source CI temporary checkout altında clone edildi; source graph, NuGet 0.1.1 graph, Android build, precision ve MAUI gesture yüzeyi ayrı ayrı kaydedildi.

AŞAMA 07 sonucu: **NO-GO** — exact unpatched pinned ProCad candidate production renderer/control reuse için reddedildi.

Karar ADR: `docs/ADR/0002-procad-pinned-source-no-go.md`.

## Final CI kanıtı

Final PR head `3f88bec383de895e309e218c08d13e9784562a97` üzerinde:

- Workflow: `Stage 07 ProCad Source Spike`
- Run: `32766501837` / #5
- Sonuç: `SUCCESS`
- Evidence artifact: `9534797361`
- Artifact digest: `sha256:9cae376fd0cbf2861f006af347483f9de26a6cd49f30b201438a3afdb591e555`

Final run marker'ları:

- `STAGE07_SOURCE_PIN_PASS`
- `STAGE07_ACAD_LINEAGE_PASS official_same_sha=0ed79df48de0806af3c3028d0e2826447cbc1d36 approved_ahead=592`
- `STAGE07_SOURCE_GRAPH_PASS`
- `STAGE07_NUGET_011_RESTORE_EXIT=0`
- `STAGE07_NUGET_SOURCE_GRAPH_RECORDED`
- `STAGE07_FLOAT_PRECISION_BLOCKER_REPRODUCED`
- `STAGE07_PINCH_REUSE_GAP_RECORDED`
- `STAGE07_SOURCE_BUILD_EXIT=0`
- `STAGE07_MAUI_SMOKE_BUILD_EXIT=0`
- `STAGE07_DECISION_NO_GO_PASS`

Aynı final PR head regresyonları:

- Stage 06 Safe Open run `32766501815` / #13: `SUCCESS`
- Stage 02 Dependency Audit run `32766501809` / #44: `SUCCESS`
- Stage 01 Toolchain Smoke run `32766501846` / #63: `SUCCESS`

Stage 01 CI fiziksel Android install/launch veya iOS erişim kanıtı değildir.

## Source pin / submodule / lineage

Pinned CI checkout:

- ProCad `f8a862b3e7634e27664fee02ff5d68774b102985`
- `external/ACadSharp` `0ed79df48de0806af3c3028d0e2826447cbc1d36`
- `external/ProEdit` `64759b79289a024d08463ed1a9094fdcd9a270df`
- ACadSharp nested `CSUtilities` `8a0c3260f364362f5b443c1f4c8539a3da406829`
- ProEdit nested Reporting-Services `acc2ee0d1884765e4b5213149430fb063d166719`

ACadSharp fork lineage unresolved değildir: pinned SHA official `DomCR/ACadSharp` geçmişinde aynı SHA olarak doğrulandı. Mobil-dwg approved ACadSharp 3.7.1 source baseline `bbc8b14a92ebfac35bb77c0c1a4af70de90ebb50`, pinned ProCad ACadSharp SHA'sından `592` official commit ileridedir.

Bu fark production parser baseline'ını sessizce ProCad fork'una taşımamamız gerektiğini gösteren ek risktir; NO-GO hard blocker'ı değildir.

## Lisans gözlemi

Pinned source root marker'larında:

- ProCad: MIT
- ACadSharp: MIT
- ProEdit: MIT

Bu spike yalnız evaluated exact source pin için license marker kaydıdır. Release compliance için final runtime/transitive/native/asset artifact audit zorunluluğunu kaldırmaz.

## Source ve NuGet graph farkı

Pinned source graph doğrudan forked ACadSharp ProjectReference'larına bağlıdır. Pinned source merkezi package sürümleri arasında:

- `Microsoft.Maui.Controls 10.0.20`
- `SkiaSharp 3.119.4`
- `SkiaSharp.Views.Maui.Controls 4.147.0-preview.2.1`

Published ProCadSharp 0.1.1 paketleri NuGet üzerinde bulunur ve restore olur. Ancak restore graph:

- `ProCadSharp.Rendering 0.1.1`
- `ACadSharp 1.0.0`
- `SkiaSharp 4.147.0-preview.2.1`
- `SkiaSharp.NativeAssets.Android 4.147.0-preview.2.1`
- `SkiaSharp.Views.Maui.Controls 4.147.0-preview.2.1`

Restore warning:

`NU1603: ProCadSharp.Rendering 0.1.1 depends on ACadSharp (>= 0.1.1) but ACadSharp 0.1.1 was not found. ACadSharp 1.0.0 was resolved instead.`

Sonuç: published 0.1.1 graph exact source graph ile eşdeğer değildir ve mobil-dwg'nin approved ACadSharp 3.7.1 baseline'ını koruyan drop-in production dependency değildir.

## Android build kanıtı

ProCad `ProCad.Controls.Maui` kaynak projesi `net10.0-android;net10.0-ios;net10.0-maccatalyst` multi-target tanımlıdır. Linux runner'da iOS workload etkisini candidate Android build sonucundan ayırmak için yalnız temporary checkout'ta TargetFrameworks `net10.0-android` olarak daraltıldı. Commit edilen upstream source değiştirilmedi.

Sonuç:

- Pinned source Android build: `exit=0`
- Build summary: `82 Warning(s)`, `0 Error(s)`
- Warning'lar başlıca pinned ACadSharp/Skia API obsolete/unreachable/analyzer warning'larıdır; decision hard blocker olarak kullanılmadı.
- Clean generated MAUI Release smoke: `exit=0`
- Clean smoke build summary: `0 Warning(s)`, `0 Error(s)`
- Signed Android APK artifact içinde üretildi.

Bu nedenle exact candidate'ın NO-GO kararı build başarısızlığı değildir.

## Precision gate — hard blocker

Pinned ProCad scene dönüşümünde world CAD point doğrudan:

`Vector2((float)point.X, (float)point.Y)`

biçiminde float'a daraltılır.

Deterministic precision fixture sonuçları:

| Fixture | Origin | Detail | float(origin) | float(origin+detail) | Observed delta | Relative delta error | Sonuç |
|---|---:|---:|---:|---:|---:|---:|---|
| small-building-mm-detail | 100.0 | 0.001 | 100.0 | 100.0009994506836 | 0.00099945068359375 | 0.0005493164062500208 | PASS |
| survey-origin-mm-detail | 5,000,000.0 | 0.001 | 5,000,000.0 | 5,000,000.0 | 0.0 | 1.0 | **FAIL / COLLAPSED** |

Aggregate: `precision_gate=FAIL`.

Bu sistematik P0 fidelity blocker'dır. World koordinatındaki 1 mm fark RenderScene boundary'sinde kaybolduğu için gerçek Android ekran testi kaybolan veriyi geri getiremez.

## Gesture reuse gap

Pinned MAUI `CadViewer` source one-pointer pan akışına sahiptir. Pinch implementation source'da bulunmadı (`maui_pinch_path_present=false`). Bu ek reuse/maintenance gap'idir; hard blocker precision kaybıdır.

## Fiziksel Android T3

`physical_android_t3 = NOT_RUN_AFTER_DETERMINISTIC_BLOCKER`

Bu değer `PASS` değildir. Exact unpatched candidate, gerçek cihaz A/B'den önce deterministic precision hard blocker ile teknik olarak reddedildiği için kullanıcıda bulunmayan fiziksel Android erişimini zorlayıp anlamsız cihaz sonucu uydurulmadı.

AŞAMA 01 fiziksel install/launch ve AŞAMA 06 gerçek FilePicker/SAF/lifecycle/cache kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır.

## Production dependency sınırı

`production_graph_modified=false`.

ProCad/ProCadSharp production solution'a eklenmedi. Existing ACadSharp 3.7.1 parser baseline ve Stage 04 architecture sınırları korunur.

## Karar

Exact unpatched ProCad source candidate: **NO-GO**.

Hard blocker:

- survey-origin + millimetre detail için direct double-to-float RenderScene boundary'sinde sistematik P0 fidelity kaybı.

Ek riskler:

- ProCad ACadSharp source pin'i approved 3.7.1 source baseline'dan 592 official commit geride,
- published 0.1.1 graph ACadSharp 1.0.0'a düşüyor,
- source/package Skia sürüm bandı preview/mixed,
- pinned MAUI control pinch reuse sağlamıyor,
- source build 82 upstream warning taşıyor.

## Özel renderer fallback yeniden maliyetlendirmesi

Özel renderer **garantili fallback değildir** ve AŞAMA 09'a geçmeden kullanıcı GO kararı gerektirir.

Planlanan P0 renderer kapsamı AŞAMA 09–16'daki sekiz ayrı implementation/fidelity aşamasıdır: scene/camera/diagnostics; temel geometry; viewport/gesture; block/INSERT/attribute; layer/color/linetype/lineweight; text/MTEXT/font/SHX; dimension/leader/hatch; model/layout/paper-space/viewport. Ardından AŞAMA 20 performance ve AŞAMA 21 full Android corpus gate'i gelir.

Efor sınıfı: **HIGH**. Calendar-day tahmini fiziksel cihaz ve gerçek fidelity ölçümü olmadan uydurulmadı.

Bakım riski: **HIGH**. Coordinate precision, CAD transforms, fonts/text, dimension/hatch, layouts/viewports ve Skia/native lifecycle sorumluluğu mobil-dwg'de kalacaktır.

Alternatif upstream patch yolu ADR 0002'de tanımlıdır: precision-safe scene/rebasing, ACadSharp baseline rebase/diff, exact Skia band alignment, pinch/lifecycle tamamlanması ve yeni exact revision üzerinde corpus + gerçek Android T3 yeniden testi.

## Sonraki eylem

AŞAMA 07 decision evidence teknik olarak tamamlanmıştır. Kapanış checkpoint'leri Stage 07 `DONE / NO-GO`, AŞAMA 08 `NEXT` olarak güncellenir; AŞAMA 08 bu kapanış turunda başlatılmaz.

AŞAMA 09 custom renderer implementation öncesinde ADR 0002'deki HIGH effort/maintenance risk için kullanıcı GO kararı zorunludur.
