# mobil-dwg — Dependency Evidence

Snapshot tarihi: 2026-08-24

Bu dosya AŞAMA 02'de canlı doğrulanan dependency/source gerçekliğini kaydeder. `GREEN` lisans/dependency kabulünü ifade eder; parser/renderer fidelity veya production kalitesi kanıtı değildir. Teknik fidelity kapıları sonraki aşamalarda ayrıca geçilir.

## Karar özeti

| Aday | Exact sürüm/revision | Lisans/dependency sonucu | AŞAMA 02 sınıfı | Runtime kararı |
|---|---|---|---|---|
| ACadSharp | NuGet `3.7.1`; source version bump commit `bbc8b14a92ebfac35bb77c0c1a4af70de90ebb50` | NuGet MIT; `net10.0` için ek NuGet dependency yok; source build `CSUtilities` submodule kullanıyor ve onun lisansı MIT | `GREEN` | Ana parser adayı; corpus/fidelity için AŞAMA 05 geçmeden production-approved değil |
| SkiaSharp | NuGet `4.151.1` | NuGet MIT; `net10.0-android36.0` graph'ı `SkiaSharp.NativeAssets.Android >=4.151.1`, iOS graph'ı `SkiaSharp.NativeAssets.iOS >=4.151.1`; native Skia upstream BSD-3-Clause | `REVIEW` | Ana renderer adayı; final native binary/third-party inventory doğrulanmadan release GREEN değil |
| ProCad | source `f8a862b3e7634e27664fee02ff5d68774b102985` | Repo MIT; `external/ACadSharp` fork submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36`; `external/ProEdit` `64759b79289a024d08463ed1a9094fdcd9a270df`; package graph/source lineage karmaşık | `REVIEW` | Production default NO-GO; yalnız AŞAMA 07 source-pinned izole spike |
| IxMilia.Dxf | NuGet `0.8.4`; current source head `3ab0f9d6d3f14a6f6fa924e111e8e3af1065c567` | NuGet MIT | `GREEN` (test/fallback scope) | Baştan runtime'a eklenmez; DXF test oracle/koşullu fallback adayı |
| IxMilia.Dwg | source head `269c8a4858cb0f836a7f3f70ba18a67dbafcb05c` | Repo MIT; 2026-08-24 NuGet aramasında güncel paket kanıtı bulunmadı | `REVIEW` | Modern DWG fallback olarak kullanılmaz |
| IxMilia.Shx | source head `4294bfec27b945c56f18c54ae79ff386238475be` | Repo MIT; 2026-08-24 NuGet aramasında paket kanıtı bulunmadı | `REVIEW` | Yalnız gelecekte SHX parser spike adayı; herhangi bir font dosyası değildir |

## ACadSharp 3.7.1

Canlı kaynaklar:

- NuGet: `https://www.nuget.org/packages/ACadSharp/`
- Source: `https://github.com/DomCR/ACadSharp`
- License: `https://github.com/DomCR/ACadSharp/blob/master/LICENSE`
- Source submodule declaration: `src/CSUtilities -> https://github.com/DomCR/CSUtilities.git`

Doğrulananlar:

- NuGet latest stable: `3.7.1`, 2026-08-18.
- NuGet license: MIT.
- NuGet `net10.0` dependency group: `No dependencies`.
- NuGet package `net10.0-android` ve `net10.0-ios` ile framework compatibility hesaplıyor.
- Source version bump commit: `bbc8b14a92ebfac35bb77c0c1a4af70de90ebb50`, 2026-08-17.
- Source build `src/CSUtilities` submodule ister; `DomCR/CSUtilities` license MIT.
- `3.6.29` NuGet'te critical bugs nedeniyle deprecated; kullanılmayacak.

Karar: `GREEN` dependency/lisans adayı. Bu karar parser fidelity garantisi değildir; AŞAMA 05 mini corpus gate zorunludur.

## SkiaSharp 4.151.1

Canlı kaynaklar:

- NuGet: `https://www.nuget.org/packages/SkiaSharp/`
- Native Android: `https://www.nuget.org/packages/SkiaSharp.NativeAssets.Android/`
- Source: `https://github.com/mono/SkiaSharp`
- Skia upstream: `https://github.com/google/skia`

Doğrulananlar:

- Latest stable: `4.151.1`, 2026-08-05. `4.152.0-preview.1.1` prerelease olduğu için seçilmedi.
- NuGet/SkiaSharp repo license: MIT.
- `net10.0-android36.0` dependency: `SkiaSharp.NativeAssets.Android >= 4.151.1`.
- `net10.0-ios26.0` dependency: `SkiaSharp.NativeAssets.iOS >= 4.151.1`.
- Android native asset paketi `4.151.1` NuGet'te MIT olarak yayınlanmış.
- Native renderer upstream Google Skia BSD-3-Clause'dur; bu lisans proje allowlist'indedir.
- Native `.so`/framework içeriğinin third-party notice/artifact inventory doğrulaması final artifact üzerinde yine gereklidir.

Karar: `REVIEW`. Sürüm pinlenebilir ve spike/build için kullanılabilir; native artifact inventory tamamlanmadan release-level `GREEN` yazılmaz.

## ProCad source graph

Canlı kaynaklar:

- Repo: `https://github.com/wieslawsoltes/ProCad`
- Snapshot commit: `f8a862b3e7634e27664fee02ff5d68774b102985`.
- Repo license: MIT.
- `.gitmodules`: ACadSharp fork + ProEdit.

Snapshot submodule SHA'ları:

- `external/ACadSharp` -> `wieslawsoltes/ACadSharp` @ `0ed79df48de0806af3c3028d0e2826447cbc1d36`.
- `external/ProEdit` -> `wieslawsoltes/ProEdit` @ `64759b79289a024d08463ed1a9094fdcd9a270df`.

Ayrıca snapshot `Directory.Packages.props` içinde:

- `SkiaSharp 3.119.4`
- `SkiaSharp.NativeAssets.Linux 3.119.4`
- `SkiaSharp.Views.Maui.Controls 4.147.0-preview.2.1`
- `Microsoft.Maui.Controls 10.0.20`

Bu, ana SkiaSharp ile MAUI view dependency hattının aynı release bandında olmadığını ve MAUI kontrol hattında prerelease dependency bulunduğunu gösterir. Repo README'si `ProCadSharp.*` package ID'lerini tanımlasa da 2026-08-24 NuGet.org exact aramalarında `ProCadSharp.Core`, `ProCadSharp.IO`, `ProCadSharp.Rendering`, `ProCadSharp.Controls.Maui` sonucu bulunmadı.

Karar: `REVIEW`, production default `NO-GO`. AŞAMA 07 yalnız exact source commit/submodule SHA ile izole spike yapabilir; bu graph production'a NuGet veya source olarak kendiliğinden girmez.

## IxMilia adayları

### IxMilia.Dxf

- NuGet latest: `0.8.4`, 2024-06-13.
- NuGet license: MIT.
- Current source head 2026-07-28: `3ab0f9d6d3f14a6f6fa924e111e8e3af1065c567`.
- Repo dokümantasyonu bazı entity/write compatibility sınırlamalarını açıkça listeler.

Karar: `GREEN` yalnız test/fallback kapsamı için. Production graph'a baştan eklenmez.

### IxMilia.Dwg

- Current source head 2026-07-28: `269c8a4858cb0f836a7f3f70ba18a67dbafcb05c`.
- Repo MIT.
- README DWG parser/writer projesi olduğunu belirtir; nihai plan bunu modern DWG fallback saymaz.

Karar: `REVIEW`, runtime'a eklenmez.

### IxMilia.Shx

- Current source head 2025-09-06: `4294bfec27b945c56f18c54ae79ff386238475be`.
- Repo MIT.
- README bunun AutoCAD SHX fontlarını okumaya yönelik bir .NET library olduğunu belirtir; font dosyasının kendisi değildir.

Karar: `REVIEW`, yalnız AŞAMA 14'te gerekirse izole parser spike adayı.

## Exact package pinleri

Repo kökündeki `Directory.Packages.props` AŞAMA 02 itibarıyla şunları exact pinler:

- `ACadSharp = 3.7.1`
- `SkiaSharp = 4.151.1`
- `IxMilia.Dxf = 0.8.4` — test/fallback candidate; production probe referansı yok

Production dependency probe yalnız ACadSharp + SkiaSharp kullanır. ProCad production graph'a eklenmez.

## Artifact/restore evidence

`compliance/Stage02.DependencyProbe` ve `.github/workflows/stage02-dependency-audit.yml` exact Android restore graph'ını üretmek, lockfile'ı doğrulamak ve resolved package license/hash manifest'i oluşturmak için kullanılır.

CI tamamlandığında aşağıdaki alanlar gerçek run sonucu ile doldurulur:

- Final Stage 02 CI run: `PENDING`
- `packages.lock.json`: `PENDING`
- Resolved direct/transitive package list: `PENDING`
- Downloaded `.nupkg` SHA-256 manifest: `PENDING`
- Unknown/RED license check: `PENDING`

Bu alanlar PASS olmadan AŞAMA 02 `DONE` yapılmaz.
