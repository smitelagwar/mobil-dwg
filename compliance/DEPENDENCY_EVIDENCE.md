# mobil-dwg — Dependency Evidence

Bu belge güncel production dependency sınırını ve önemli provenance kararlarını özetler. Eski Stage numaraları geliştirme akışı değildir.

## Güncel production graph

Central Package Management kaynağı: `Directory.Packages.props`.

| Paket | Exact sürüm | Kullanım | Lisans / karar |
|---|---:|---|---|
| ACadSharp | `3.7.1` | `MobilDwg.Cad` read-only DWG/DXF parser | MIT / `GREEN` |
| SkiaSharp | `4.151.1` | `MobilDwg.Rendering` 2D renderer | MIT; Android native asset graph'ı ayrıca denetlenir |
| Microsoft.Maui.Controls | `10.0.100` | Android uygulama/UI | Microsoft package baseline; exact pin |
| IxMilia.Dxf | `0.8.4` | test/fallback adayı; production graph'a otomatik girmez | MIT / test-only candidate |

Direct dependency sürümleri strict exact NuGet range olarak pinlidir; floating/latest kullanılmaz.

## Production proje sınırı

- `MobilDwg.Core`: harici PackageReference yok.
- `MobilDwg.Cad`: ACadSharp.
- `MobilDwg.Rendering`: SkiaSharp.
- `MobilDwg.App`: Microsoft.Maui.Controls.

Production `src/` altında iOS/MacCatalyst/Windows TFM veya vendored native binary bulunmaması dependency audit tarafından kontrol edilir.

## Android resolved graph lock

Güncel lock kaynağı:

```text
compliance/Stage02.DependencyProbe/packages.lock.json
```

Klasör adı tarihsel kökenlidir; proje halen yalnız ACadSharp + SkiaSharp üzerinden Android resolved graph'ını kilitlemek için kullanılır ve aktif dependency audit'in parçasıdır.

Beklenen Android graph:

- Direct: `ACadSharp 3.7.1`
- Direct: `SkiaSharp 4.151.1`
- Transitive: `SkiaSharp.NativeAssets.Android 4.151.1`

Audit projesi ve committed lockfile `--locked-mode` ile doğrulanır.

## Native artifact sınırı

`SkiaSharp.NativeAssets.Android 4.151.1` için beklenen native girdiler dört Android ABI'sine ait `libSkiaSharp.so` dosyalarıdır:

- android-arm
- android-arm64
- android-x64
- android-x86

Audit, beklenmeyen iOS/macOS/Windows/Linux native girdisini Android graph'ında kabul etmez.

## Paket artifact doğrulaması

`compliance/stage02-package-manifest.json` exact NuGet artifact sonuçlarını tutar. `scripts/stage02-audit-packages.py`:

- central package setini/exact version syntax'ını,
- production PackageReference sınırını,
- lock graph'ını,
- NuGet license expression'larını,
- nupkg SHA-256 değerlerini,
- native entry envanterini

yeniden doğrular ve manifesti deterministik biçimde üretir.

Aktif CI: `.github/workflows/dependency-audit.yml`.

## Kalıcı aday kararları

### ProCad

Exact source-pinned ProCad yeniden kullanım değerlendirmesi üretim için `NO-GO` olarak kalır. Ayrıntılı gerekçe `docs/ADR/0002-procad-pinned-source-no-go.md` içindedir. ProCad source/submodule spike kodu production tree'de tutulmaz.

### IxMilia.Dxf

MIT lisanslı test/fallback adayıdır. Açık bir teknik ihtiyaç ve ayrı doğrulama olmadan production runtime graph'a eklenmez.

### IxMilia.Dwg / IxMilia.Shx

Production runtime dependency değildir. Gelecekte değerlendirilecekse güncel source/package/lisans durumu yeniden doğrulanır; eski snapshot otomatik onay sayılmaz.

## Değişiklik kuralı

Dependency veya redistributable asset değişikliğinde aynı değişiklik içinde:

1. exact version/source revision,
2. transitive graph,
3. license/provenance,
4. native artifact inventory,
5. package manifest/lockfile,
6. `compliance/LICENSE_POLICY.md` sonucu

yeniden doğrulanır.

Eski CI run ID'leri ve tamamlanmış Stage evidence'ları bu yaşayan belgede tutulmaz; gerektiğinde Git geçmişinden erişilir.
