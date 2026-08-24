# ADR 0002 — Pinned ProCad source candidate production renderer NO-GO

- Durum: Rejected
- Tarih: 2026-08-24
- Aşama: AŞAMA 07
- İlgili revision/PR: ProCad `f8a862b3e7634e27664fee02ff5d68774b102985`; mobil-dwg PR `#9`; final decision head `3f88bec383de895e309e218c08d13e9784562a97`

## Bağlam

AŞAMA 07, hazır renderer/control reuse olasılığını production dependency graph'ını değiştirmeden değerlendirmek için exact ProCad source candidate'ını izole Android spike olarak inceler. Amaç ProCad'ı otomatik olarak kabul etmek değil; exact source/submodule lineage, Android build, NuGet/source graph farkı, gesture reuse yüzeyi, license/native/preview riskleri ve özellikle CAD world-coordinate precision davranışını ölçerek `GO`, `CONDITIONAL-GO` veya `NO-GO` kararı vermektir.

Değerlendirilen exact source pin:

- ProCad: `f8a862b3e7634e27664fee02ff5d68774b102985`
- ACadSharp submodule: `0ed79df48de0806af3c3028d0e2826447cbc1d36`
- ProEdit submodule: `64759b79289a024d08463ed1a9094fdcd9a270df`
- Mobil-dwg approved ACadSharp 3.7.1 source baseline: `bbc8b14a92ebfac35bb77c0c1a4af70de90ebb50`

Production solution'a ProCad PackageReference/ProjectReference eklenmedi.

## Kanıt

### Source pin ve lineage

- Exact ProCad ve submodule SHA'ları CI'da clone/checkout edilip doğrulandı: `STAGE07_SOURCE_PIN_PASS`.
- ProCad'ın ACadSharp submodule commit'i official `DomCR/ACadSharp` geçmişinde aynı SHA olarak çözüldü; lineage unresolved değildir.
- Buna rağmen mobil-dwg'nin approved ACadSharp 3.7.1 source baseline'ı bu pinned submodule'dan `592` official commit ileridedir: `STAGE07_ACAD_LINEAGE_PASS ... approved_ahead=592`.
- Pinned source lisans marker'ları ProCad, ACadSharp ve ProEdit için MIT olarak kaydedildi. Bu spike üçüncü taraf bütün gelecekteki runtime artifact audit'inin yerine geçmez.

### Source Android build

CI, pinned checkout içindeki `ProCad.Controls.Maui` multi-target projesini yalnız temporary checkout'ta `net10.0-android` hedefiyle daraltarak Linux runner'da iOS workload etkisini ayırdı. Upstream source değiştirilmedi ve mobil-dwg production graph'ı değiştirilmedi.

- Pinned ProCad Android source build: `exit=0`; `82 Warning(s)`, `0 Error(s)`.
- Clean MAUI Release smoke app, pinned source project references ile: `exit=0`; `0 Warning(s)`, `0 Error(s)` ve signed APK üretildi.
- Bu nedenle candidate `NO-GO` kararı build başarısızlığına dayanmaz.

### NuGet 0.1.1 ile source graph farkı

Published `ProCadSharp.* 0.1.1` paketleri bulunur ve restore olur; ancak published graph exact source graph ile eşdeğer değildir.

- `ProCadSharp.Rendering 0.1.1`, `ACadSharp >= 0.1.1` ister.
- ACadSharp `0.1.1` bulunmadığı için restore `NU1603` ile `ACadSharp 1.0.0` çözer.
- `ProCadSharp.Controls.Maui 0.1.1` graph'ı `SkiaSharp`, `SkiaSharp.NativeAssets.Android` ve `SkiaSharp.Views.Maui.Controls` için `4.147.0-preview.2.1` çözer.
- Pinned source merkezi sürüm dosyası `SkiaSharp 3.119.4` ile `SkiaSharp.Views.Maui.Controls 4.147.0-preview.2.1` karışımı içerir.

Bu nedenle published NuGet 0.1.1, mobil-dwg'nin approved ACadSharp 3.7.1 parser baseline'ını taşıyan güvenilir replacement graph olarak kabul edilmez.

### Precision blocker

Pinned ProCad RenderScene yolunda CAD point koordinatının doğrudan `Vector2((float)point.X, (float)point.Y)` biçiminde `double`dan `float`a dönüştürüldüğü doğrulandı.

Deterministic precision repro:

| Case | Origin | Detail | Float sonrası observed delta | Sonuç |
|---|---:|---:|---:|---|
| small-building-mm-detail | `100.0` | `0.001` | `0.00099945068359375` | korunuyor |
| survey-origin-mm-detail | `5,000,000.0` | `0.001` | `0.0` | **collapsed** |

Survey-origin case relative delta error `1.0`; precision gate `FAIL`.

Bu sistematik P0 fidelity kaybıdır. Aynı world-coordinate çevresindeki farklı noktalar render scene sınırında aynı float koordinata düşebilir. Gerçek cihaz/görüntü testi bu deterministik veri kaybını geri getiremez.

### Gesture reuse yüzeyi

Pinned MAUI `CadViewer` source'unda one-pointer pan yolu vardır; pinch implementation bulunmadı. Bu tek başına hard blocker değildir fakat doğrudan viewer-control reuse değerini düşüren ek bakım işidir.

### CI

Final decision head `3f88bec383de895e309e218c08d13e9784562a97` üzerinde:

- `Stage 07 ProCad Source Spike` run `32766501837` / #5: `SUCCESS`
- Artifact: `9534797361`
- Artifact digest: `sha256:9cae376fd0cbf2861f006af347483f9de26a6cd49f30b201438a3afdb591e555`
- `STAGE07_SOURCE_BUILD_EXIT=0`
- `STAGE07_MAUI_SMOKE_BUILD_EXIT=0`
- `STAGE07_FLOAT_PRECISION_BLOCKER_REPRODUCED`
- `STAGE07_DECISION_NO_GO_PASS`

Aynı PR head regresyonları:

- Stage 06 Safe Open run `32766501815` / #13: `SUCCESS`
- Stage 02 Dependency Audit run `32766501809` / #44: `SUCCESS`
- Stage 01 Toolchain Smoke run `32766501846` / #63: `SUCCESS`; fiziksel cihaz kanıtı değildir.

## Karar

Exact unpatched ProCad source candidate `f8a862b3e7634e27664fee02ff5d68774b102985` için production renderer/control reuse kararı **NO-GO**.

Hard blocker:

- survey-origin millimetre detayın direct `double -> float` RenderScene boundary'sinde deterministik olarak çökmesi.

Bu karar şu anlamlara gelmez:

- ProCad projesinin genel olarak değersiz veya derlenemez olduğu,
- özel renderer'ın otomatik olarak doğru veya düşük riskli olduğu,
- precision patch uygulanmış yeni bir ProCad revision'ın sonsuza kadar reddedildiği.

Production graph'a ProCad eklenmeyecek. Published `ProCadSharp.* 0.1.1` paketleri de source candidate yerine kullanılmayacak.

## P0 fallback kapsamı ve efor yeniden maliyetlendirmesi

Özel renderer **garantili fallback değildir**. ProCad NO-GO sonrasında planlanan kendi renderer hattı ayrıca kullanıcı GO kararı ister.

P0 kapsamı plan seviyesinde:

- AŞAMA 09: RenderScene, kamera ve diagnostics temeli
- AŞAMA 10: temel 2D geometry
- AŞAMA 11: viewport/gesture
- AŞAMA 12: block/INSERT/attribute transforms
- AŞAMA 13: layer/color/linetype/lineweight
- AŞAMA 14: TEXT/MTEXT/Türkçe/font/SHX
- AŞAMA 15: dimension/leader/hatch
- AŞAMA 16: model/layout/paper-space/viewport

Tahmini efor sınıfı: **HIGH**. Calendar-day tahmini verilmez; cihaz/corpus fidelity kanıtı olmadan gün sayısı uydurulmaz. Mevcut plan sekiz ayrı implementation/fidelity aşaması ve sonrasında AŞAMA 20 performans + AŞAMA 21 tam Android corpus gate'i gerektirir.

Bakım riski: **HIGH**. CAD transform semantics, font/text, dimension/hatch, layout/viewport, large-coordinate precision ve Skia/native lifecycle davranışları mobil-dwg tarafından sahiplenilmiş olur.

## Alternatif upstream patch yolu

ProCad ancak yeni bir exact candidate olarak yeniden açılabilir. Minimum yeniden değerlendirme koşulları:

1. World-coordinate scene hattında raw absolute `float` saklama kaldırılır; örneğin double-precision scene veya origin-rebased/local-coordinate model ile survey-origin mm fixture precision gate'i geçer.
2. ACadSharp fork/submodule mobil-dwg approved parser baseline'ına yeniden tabanlanır veya diff/API/fidelity sonucu ayrıca kanıtlanır.
3. SkiaSharp source/package version bandı tek ve approved exact çizgide hizalanır; preview/native riskleri yeniden audit edilir.
4. MAUI pinch + lifecycle/dispose davranışı eklenir veya mobil-dwg tarafından ayrı adapter ile kanıtlanır.
5. Stage 03 mini corpus ve gerçek Android Release T3 A/B yeniden çalıştırılır.

Bu koşullar ayrı bir source-pinned revision/PR olarak değerlendirilmelidir; mevcut rejected revision sessizce patch'lenip production'a alınmaz.

## Sonuçlar ve riskler

- ProCad production dependency graph'a girmez; mevcut dört production proje mimarisi korunur.
- AŞAMA 07 physical Android T3, deterministik hard blocker exact candidate'ı önceden reddettiği için `NOT_RUN_AFTER_DETERMINISTIC_BLOCKER` olarak kaydedilir; PASS değildir ve genel gerçek-cihaz gereksinimlerini kapatmaz.
- AŞAMA 01 ve AŞAMA 06 physical Android kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır.
- AŞAMA 08 iOS feasibility bağımsız sırada yürütülebilir; ancak AŞAMA 09 özel renderer implementation'ına geçmeden önce bu NO-GO sonrası efor/risk için kullanıcı GO kararı gereklidir.

## Yeniden açma koşulu

Precision-safe, lineage-aligned, exact dependency graph'ı audit edilmiş yeni ProCad revision'ı yukarıdaki koşulları sağlarsa bu ADR supersede edilebilir. Sadece daha yeni sürüm numarası veya başarılı build yeniden açma için yeterli değildir.
