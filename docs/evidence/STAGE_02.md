# AŞAMA 02 Evidence — Dependency/lisans kanıtı ve kilitler

Tarih: 2026-08-24

Durum: `DONE`

AŞAMA 01 gerçek Android/iOS dış erişim kapıları `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` gereği `DEFERRED_EXTERNAL_GATE` olarak açık kalır. Bu dosya yalnız AŞAMA 02'nin bağımsız çıkış kriterlerini kanıtlar.

## Tamamlanan işler

- [x] ACadSharp current stable release, source/license/dependency/submodule hattı canlı doğrulandı.
- [x] SkiaSharp stable package ve Android/iOS native dependency hattı canlı doğrulandı.
- [x] ProCad current source commit, ACadSharp fork submodule SHA, ProEdit submodule SHA ve package/version skew riski kaydedildi.
- [x] IxMilia.Dxf/Dwg/Shx yalnız test/fallback/source-spike scope'unda sınıflandırıldı.
- [x] `compliance/LICENSE_POLICY.md` oluşturuldu.
- [x] `compliance/DEPENDENCY_EVIDENCE.md` oluşturuldu ve exact source/package SHA bilgileri kaydedildi.
- [x] `compliance/RISK_REGISTER.md` oluşturuldu.
- [x] Central Package Management `Directory.Packages.props` ile etkinleştirildi.
- [x] Exact direct package sürümleri pinlendi.
- [x] Minimal Android dependency probe oluşturuldu.
- [x] `packages.lock.json` repoya commit edildi.
- [x] CI restore doğrudan `--locked-mode` ile committed lockfile'ı doğruluyor.
- [x] Exact `.nupkg` SHA-256 + NuGet license expression + native-entry manifest'i repoya commit edildi.
- [x] Unknown/policy-RED package kontrolü PASS.
- [x] Vulnerability kontrolünde mevcut NuGet kaynaklarına göre vulnerable package bulunmadı.
- [x] Stage 01 root-CPM regresyonu yakalandı ve smoke projesi repo dışı temp dizine izole edildi.
- [x] Stage 01 regresyon workflow'u aynı final PR head üzerinde yeniden PASS oldu.
- [x] PR #4 doğrulanmış head üzerinden `main`e merge edildi.

## Aday sınıflandırması

| Bileşen | Exact baseline | AŞAMA 02 sınıfı | Sonuç |
|---|---|---|---|
| ACadSharp | `3.7.1` | `GREEN` | Ana parser adayı; AŞAMA 05 corpus/fidelity gate zorunlu |
| SkiaSharp | `4.151.1` | `REVIEW` | Ana renderer adayı; Android native artifact kanıtı var, final third-party binary inventory release gate'inde zorunlu |
| ProCad | source `f8a862b3e7634e27664fee02ff5d68774b102985` | `REVIEW` | Production default NO-GO; yalnız AŞAMA 07 source-pinned spike |
| IxMilia.Dxf | `0.8.4` | `GREEN` — test/fallback scope | Başlangıç production runtime graph'ına eklenmez |
| IxMilia.Dwg | source `269c8a4858cb0f836a7f3f70ba18a67dbafcb05c` | `REVIEW` | Modern DWG fallback değil |
| IxMilia.Shx | source `4294bfec27b945c56f18c54ae79ff386238475be` | `REVIEW` | Yalnız gelecekte SHX parser spike adayı; font asset değildir |

## Production probe resolved graph

TFM: `net10.0-android36.0`

- Direct `ACadSharp 3.7.1`
- Direct `SkiaSharp 4.151.1`
- Transitive `SkiaSharp.NativeAssets.Android 4.151.1`

Committed lockfile:

- Path: `compliance/Stage02.DependencyProbe/packages.lock.json`
- SHA-256: `880bdb834856010d1a08821e72f539208170c9e8a929e183c17eaf7dee2d362d`

## Exact NuGet artifact kanıtı

- `ACadSharp 3.7.1` `.nupkg` SHA-256: `4f9ca3a5dafd1a18af651312522147a3163999818763d168b4d5f59d6ffc1701`; license MIT; native entry yok.
- `SkiaSharp 4.151.1` `.nupkg` SHA-256: `2d1feef23f28e55864cad8449f7b60abf5d6db1aa61ec07aef837e9e0eaee73e`; license MIT.
- `SkiaSharp.NativeAssets.Android 4.151.1` `.nupkg` SHA-256: `0857f22d4de9f87899675a30312c52801c6ff85e7ca25dc9483a969c43612803`; license MIT; arm/arm64/x64/x86 `libSkiaSharp.so` içerir.
- Committed package manifest SHA-256: `04350e4ea477131ad19f5b06ae28deb0d4c0c1effd107d66178ee7d3d64fb02c`.

## Final Stage 02 CI kanıtı

Workflow: `Stage 02 Dependency Audit`

Final PR-head run:

- Run ID: `32747785867` / #9
- Head: `7daa5d7dc326915700f60396bdf50604bf0601e7`
- Sonuç: `SUCCESS`
- Exact .NET/workload: PASS
- Committed `--locked-mode` restore: PASS
- Lockfile diff: PASS
- Resolved graph: PASS
- Exact package license/hash audit: PASS
- Committed package manifest reproducibility diff: PASS
- Vulnerability check: PASS; mevcut kaynaklara göre vulnerable package yok
- Evidence artifact: `9527769476`
- Artifact digest: `sha256:90d41760e306e13b9977586b9996c1aafdf27f615c2b730bb41d74507b4684f3`

Önceki run `32746969262` / #5 committed lock/manifest modelinin ilk güçlendirilmiş PASS kanıtıydı. #9 final PR head üzerindeki kapanış kapısıdır.

## Stage 01 regresyon kapısı

Kök `Directory.Packages.props` eklendiğinde Stage 01 CI smoke app'i başlangıçta repo altında üretildiği için CPM'yi miras aldı ve `NU1008` verdi. Bu, CAD package uyumsuzluğu değil test izolasyonu hatasıydı.

Düzeltme: Stage 01 GitHub Actions smoke projesi `$RUNNER_TEMP` altında repo ağacı dışında üretilir. İlk düzeltmede workflow-level `${{ runner.temp }}` bağlamının workflow parse/trigger seviyesinde geçersiz olduğu görüldü; path kullanımı step-level `$RUNNER_TEMP` / `${{ runner.temp }}` alanlarına taşındı. Fiziksel Android gate scriptleri zaten OS temp dizini kullanır.

Final aynı-head regresyon koşusu:

- Workflow: `Stage 01 Toolchain Smoke`
- Run ID: `32747785948` / #29
- Head: `7daa5d7dc326915700f60396bdf50604bf0601e7`
- Sonuç: `SUCCESS`
- Isolated clean MAUI project creation: PASS
- Debug build: PASS
- Release build: PASS
- Manifest/API baseline: PASS
- Artifact upload: PASS
- Artifact ID: `9528014030`
- Artifact digest: `sha256:57f01ed14600684a5a9434b9ca2db2b6e32b4a9fac95bee90d455a4595e8421e`

Bu CI fiziksel Android telefon kanıtı değildir. AŞAMA 01 `BLOCKED / DEFERRED_EXTERNAL_GATE` olarak kalır.

## Merge

PR #4: `compliance: establish stage 02 dependency audit`

- Doğrulanmış head: `7daa5d7dc326915700f60396bdf50604bf0601e7`
- Base: `main`
- Merge sonucu: başarılı
- Merge commit: `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`

## Çıkış değerlendirmesi

AŞAMA 02 çıkış kriterleri sağlandı:

- Her aday `GREEN/REVIEW/RED` sınıfına sahip.
- Production direct dependency'de `latest`, floating veya kanıtsız package yok.
- ProCad production graph'a eklenmedi.
- Exact lockfile ve locked restore var.
- NuGet artifact hash/license kontrolü var.
- Unknown/policy-RED resolved package yok.
- Stage 01 root-CPM regresyonu final PR head üzerinde kapatıldı.

AŞAMA 02 `DONE`. Sonraki bağımsız çalışma aşaması AŞAMA 03'tür; aynı kullanıcı turunda AŞAMA 03 başlatılmaz.
