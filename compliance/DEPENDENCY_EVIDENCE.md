# mobil-dwg — Dependency Evidence

Snapshot tarihi: 2026-08-24

Bu dosya AŞAMA 02'de canlı doğrulanan dependency/source gerçekliğini kaydeder. `GREEN` lisans/dependency kabulünü ifade eder; parser/renderer fidelity veya production kalitesi kanıtı değildir. Teknik fidelity kapıları sonraki aşamalarda ayrıca geçilir.

## Karar özeti

| Aday | Exact sürüm/revision | Lisans/dependency sonucu | AŞAMA 02 sınıfı | Runtime kararı |
|---|---|---|---|---|
| ACadSharp | NuGet `3.7.1`; source version bump commit `bbc8b14a92ebfac35bb77c0c1a4af70de90ebb50` | NuGet MIT; `net10.0` için ek NuGet dependency yok; source build `CSUtilities` submodule kullanıyor ve onun lisansı MIT | `GREEN` | Ana parser adayı; corpus/fidelity için AŞAMA 05 geçmeden production-approved değil |
| SkiaSharp | NuGet `4.151.1` | NuGet MIT; Android graph'ı `SkiaSharp.NativeAssets.Android 4.151.1`; iOS graph'ı platform native asset taşır; native Skia upstream BSD-3-Clause | `REVIEW` | Ana renderer adayı; final native binary/third-party inventory doğrulanmadan release GREEN değil |
| ProCad | source `f8a862b3e7634e27664fee02ff5d68774b102985` | Repo MIT; `external/ACadSharp` fork submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36`; `external/ProEdit` `64759b79289a024d08463ed1a9094fdcd9a270df`; package graph/source lineage karmaşık | `REVIEW` | Production default NO-GO; yalnız AŞAMA 07 source-pinned izole spike |
| IxMilia.Dxf | NuGet `0.8.4`; current source head `3ab0f9d6d3f14a6f6fa924e111e8e3af1065c567` | NuGet/source MIT | `GREEN` (test/fallback scope) | Baştan runtime'a eklenmez; DXF test oracle/koşullu fallback adayı |
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
- Android resolved graph: `SkiaSharp 4.151.1 -> SkiaSharp.NativeAssets.Android 4.151.1`.
- NuGet metadata iOS tarafında da platform native asset dependency'si taşındığını gösterir.
- Android native asset paketi `4.151.1` NuGet'te MIT olarak yayınlanmış.
- Native renderer upstream Google Skia BSD-3-Clause'dur; bu lisans proje allowlist'indedir.
- Android native package içinde arm, arm64, x64 ve x86 `libSkiaSharp.so` artifact'leri gerçek `.nupkg` taramasında görüldü.
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

Snapshot `Directory.Packages.props` ayrıca şunları içeriyor:

- `SkiaSharp 3.119.4`
- `SkiaSharp.NativeAssets.Linux 3.119.4`
- `SkiaSharp.Views.Maui.Controls 4.147.0-preview.2.1`
- `Microsoft.Maui.Controls 10.0.20`

Bu, ana SkiaSharp ile MAUI view dependency hattının aynı release bandında olmadığını ve MAUI kontrol hattında prerelease dependency bulunduğunu gösterir. Repo README'si `ProCadSharp.*` package ID'lerini tanımlasa da 2026-08-24 NuGet.org exact aramalarında `ProCadSharp.Core`, `ProCadSharp.IO`, `ProCadSharp.Rendering`, `ProCadSharp.Controls.Maui` sonucu bulunmadı.

Karar: `REVIEW`, production default `NO-GO`. AŞAMA 07 yalnız exact source commit/submodule SHA ile izole spike yapabilir; bu graph production'a NuGet veya source olarak kendiliğinden girmez.

## IxMilia adayları

### IxMilia.Dxf

- NuGet latest: `0.8.4`, 2024-06-13.
- NuGet/source license: MIT.
- Current source head 2026-07-28: `3ab0f9d6d3f14a6f6fa924e111e8e3af1065c567`.
- Repo dokümantasyonu bazı entity/write compatibility sınırlamalarını açıkça listeler.

Karar: `GREEN` yalnız test/fallback kapsamı için. Production graph'a baştan eklenmez.

### IxMilia.Dwg

- Current source head 2026-07-28: `269c8a4858cb0f836a7f3f70ba18a67dbafcb05c`.
- Repo `LICENSE.txt`: MIT.
- README DWG parser/writer projesi olduğunu belirtir; nihai plan bunu modern DWG fallback saymaz.

Karar: `REVIEW`, runtime'a eklenmez.

### IxMilia.Shx

- Current source head 2025-09-06: `4294bfec27b945c56f18c54ae79ff386238475be`.
- Repo `LICENSE.txt`: MIT.
- README bunun AutoCAD SHX fontlarını okumaya yönelik bir .NET library olduğunu belirtir; font dosyasının kendisi değildir.

Karar: `REVIEW`, yalnız AŞAMA 14'te gerekirse izole parser spike adayı.

## Exact package pinleri ve lock

Repo kökündeki `Directory.Packages.props` AŞAMA 02 itibarıyla şunları pinler:

- `ACadSharp = 3.7.1`
- `SkiaSharp = 4.151.1`
- `IxMilia.Dxf = 0.8.4` — test/fallback candidate; production probe referansı yok

Production dependency probe yalnız ACadSharp + SkiaSharp kullanır. ProCad production graph'a eklenmez.

Committed lockfile: `compliance/Stage02.DependencyProbe/packages.lock.json`.

Resolved Android graph:

- Direct: `ACadSharp 3.7.1`
- Direct: `SkiaSharp 4.151.1`
- Transitive: `SkiaSharp.NativeAssets.Android 4.151.1`
- TFM: `net10.0-android36.0`

Lockfile SHA-256: `880bdb834856010d1a08821e72f539208170c9e8a929e183c17eaf7dee2d362d`.

CI restore committed lockfile üzerinde doğrudan `dotnet restore --locked-mode` çalıştırır ve lock diff'i sıfır olmak zorundadır.

## Exact NuGet artifact manifest

Committed manifest: `compliance/stage02-package-manifest.json`.

Gerçek NuGet `.nupkg` SHA-256 değerleri:

- `ACadSharp 3.7.1`: `4f9ca3a5dafd1a18af651312522147a3163999818763d168b4d5f59d6ffc1701` — MIT, embedded native entry yok.
- `SkiaSharp 4.151.1`: `2d1feef23f28e55864cad8449f7b60abf5d6db1aa61ec07aef837e9e0eaee73e` — MIT, bu meta-package içinde native entry yok.
- `SkiaSharp.NativeAssets.Android 4.151.1`: `0857f22d4de9f87899675a30312c52801c6ff85e7ca25dc9483a969c43612803` — MIT; dört Android ABI için `libSkiaSharp.so` içerir.

Package manifest SHA-256: `04350e4ea477131ad19f5b06ae28deb0d4c0c1effd107d66178ee7d3d64fb02c`.

CI audit scripti exact nupkg'leri NuGet flat-container üzerinden yeniden indirir, nuspec license expression'ını okur, SHA-256'yı yeniden hesaplar ve committed manifest ile `git diff --exit-code` uygular.

## Final Stage 02 CI kanıtı

Workflow: `Stage 02 Dependency Audit`.

Final PR-head koşusu:

- Run: `32747785867` / #9.
- Head: `7daa5d7dc326915700f60396bdf50604bf0601e7`.
- Sonuç: `SUCCESS`.
- .NET SDK: `10.0.400`.
- Workload set: `10.0.400`; `maui-android` PASS.
- Committed locked restore: PASS.
- Resolved graph kaydı: PASS.
- Exact nupkg license/hash audit: PASS.
- Committed package manifest reproducibility diff: PASS.
- Vulnerability report: mevcut kaynaklara göre vulnerable package yok.
- Evidence artifact upload: PASS.
- Artifact ID: `9527769476`.
- Artifact ZIP digest: `sha256:90d41760e306e13b9977586b9996c1aafdf27f615c2b730bb41d74507b4684f3`.

Aynı head üzerinde Stage 01 regression workflow'u da yeniden çalıştırıldı:

- Workflow: `Stage 01 Toolchain Smoke`.
- Run: `32747785948` / #29.
- Sonuç: `SUCCESS`.
- Repo-root Central Package Management etkisinden kaçınmak için temiz MAUI smoke projesi `$RUNNER_TEMP` altında üretildi.
- Debug build: PASS.
- Release build: PASS.
- Manifest/API baseline: PASS.
- Artifact upload: PASS.
- Artifact ID: `9528014030`.
- Artifact digest: `sha256:57f01ed14600684a5a9434b9ca2db2b6e32b4a9fac95bee90d455a4595e8421e`.

PR #4 doğrulanmış head `7daa5d7dc326915700f60396bdf50604bf0601e7` üzerinden `main`e merge edildi. Merge commit: `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.

## Stage 01 CPM regresyonu ve düzeltme

Kök `Directory.Packages.props` ilk eklendiğinde Stage 01 CI'daki repo-altı `.smoke` MAUI template'i CPM'yi miras aldı ve `NU1008` ile düştü. Bu dependency uyumsuzluğu değil, test izolasyonu hatasıydı.

Düzeltme: Stage 01 CI smoke projesi repo ağacı dışında `$RUNNER_TEMP` altında üretilir. İlk düzeltmede workflow-level `${{ runner.temp }}` kullanımının workflow parse/trigger seviyesinde geçersiz olduğu görüldü; kullanım step-level `$RUNNER_TEMP` / `${{ runner.temp }}` alanlarına taşındı. Fiziksel cihaz gate scriptleri zaten sistem temp dizini altında proje ürettiği için root CPM'den bağımsızdır. Final Stage 01 run `32747785948` bu düzeltmenin Debug/Release/manifest/artifact hattını bozmadığını kanıtladı.

## AŞAMA 02 sonucu

Durum: `DONE`.

- Unknown veya policy-RED NuGet dependency tespit edilmedi.
- ACadSharp `GREEN` dependency/lisans adayıdır, ancak fidelity açısından AŞAMA 05 geçmeden production-approved değildir.
- SkiaSharp `REVIEW` olarak pinlidir; Android native package lisansı/hash/native entry kanıtı vardır, final binary third-party inventory release gate'inde tekrar açılır.
- ProCad `REVIEW` ve production default `NO-GO`; yalnız AŞAMA 07 source-pinned spike.
- IxMilia.Dxf test/fallback scope'unda `GREEN`; Dwg/Shx `REVIEW` ve runtime dışında.
- Floating/latest production dependency yok; committed lock + locked restore kapısı vardır.
- Stage 01 root-CPM regresyonu aynı PR head üzerinde kapatıldı.
- Sonraki çalışma aşaması: AŞAMA 03 — test corpus'u, golden sözleşmesi ve cihaz matrisi.
