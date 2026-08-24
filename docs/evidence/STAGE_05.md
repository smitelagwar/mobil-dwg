# AŞAMA 05 Evidence — ACadSharp headless parser spike

## Kimlik

- Tarih: 2026-08-24
- Aşama: AŞAMA 05
- Repo: `smitelagwar/mobil-dwg`
- Branch: `stage05-acadsharp-parser-spike`
- Başlangıç main: `27f036d5d240c4ca47dd2fcb94c1e72604ed0f8f`
- İlk parser implementation doğrulaması: `09e26172aa8de9e8c79ae64853a493dab1d0e5b9`
- Doğrulanmış final PR head: `80cdaf49d3ad4298f3b1d56fe1dbac89b352ec7f`
- PR: `#7` — `stage05: validate ACadSharp headless parser`
- Main merge commit: `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`
- Parser: ACadSharp `3.7.1`
- Ortam: GitHub hosted Ubuntu 24.04, .NET SDK/workload set `10.0.400`

## Değişiklik

- `MobilDwg.Cad` içine yalnız read-only parser adapter amacıyla ACadSharp `3.7.1` eklendi.
- NuGet tarafından üretilen `src/MobilDwg.Cad/packages.lock.json` commit edildi; final gate `--locked-mode` kullanıyor.
- `AcadSharpDocumentReader` DWG/DXF preflight, parser notifications, exception/timing, missing-font/missing-XREF ve compatibility sınıflandırmasını Core kontratlarına bağlıyor.
- Parser document yalnız `AcadSharpDocumentHandle` arkasında tutuluyor; Core/App/Rendering ACadSharp entity/type'larına bağlanmıyor.
- Cancellation `BeforeStartOnly`, progress `StagesOnly`; parser başladıktan sonra cooperative cancellation veya sahte yüzde ilan edilmiyor.
- `tools/Stage05.ParserProbe` Stage 03 manifestini gerçek fixture cache üzerinde doğruluyor.
- Architecture test ACadSharp PackageReference'ını yalnız `MobilDwg.Cad` için allow ediyor ve diğer production katmanlarına sızıntıyı reddediyor.
- `.github/workflows/stage05-parser-spike.yml` locked restore + Release build + T3 mini corpus gate + evidence artifact çalıştırıyor.

## Komutlar ve testler

| Komut/Test | Sonuç | Not |
|---|---|---|
| Stage 03 fixture integrity download/audit | PASS | `STAGE03_FIXTURE_AUDIT_PASS fixtures=9 derived_negatives=2` |
| `dotnet restore src/MobilDwg.Cad/MobilDwg.Cad.csproj --locked-mode` | PASS | committed NuGet-generated lockfile |
| Solution Release build `/warnaserror` | PASS | `0 Warning(s)`, `0 Error(s)` |
| Core contract tests | PASS | `STAGE04_CORE_CONTRACT_TESTS_PASS` |
| Rendering contract tests | PASS | `STAGE04_RENDER_CONTRACT_TESTS_PASS` |
| Architecture tests | PASS | `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS` |
| AC1015/AC1018/AC1024/AC1032 DWG | PASS | her fixture manifest minimum semantic count'larını geçti |
| AC1015/AC1032 ASCII DXF | PASS | manifest minimum semantic count'larını geçti |
| Turkish/basic/nested INSERT synthetic DXF | PASS | exact count contract geçti |
| Missing-font synthetic DXF | PASS | `missing-font` compatibility code |
| Missing-XREF synthetic DXF | PASS | `missing-xref` compatibility code |
| Derived truncated AC1015 DWG | PASS | controlled `EndOfStreamException` |
| Derived corrupt AC1018 DWG | PASS | crash yerine controlled warning |
| Mini corpus aggregate | PASS | `STAGE05_MINI_CORPUS_PASS fixtures=9 derived_negatives=2` |
| Package graph | PASS | direct ACadSharp requested/resolved `3.7.1` |
| Stage 05 final gate | PASS | `STAGE05_T3_PASS` |

## Final Stage 05 CI kanıtı

Final PR head `80cdaf49d3ad4298f3b1d56fe1dbac89b352ec7f` üzerinde:

- Workflow: `Stage 05 Parser Spike`
- Run: `32760139261` / #15
- Sonuç: `SUCCESS`
- Evidence artifact: `9532379884`
- Artifact digest: `sha256:f3b31c937186d874a0ed23c045951d465ace5da8fff2f9acc32006c4352e2f60`

Aynı final PR head regresyonları:

- `Stage 04 Architecture` run `32760139230` / #18: `SUCCESS`
- `Stage 02 Dependency Audit` run `32760139219` / #32: `SUCCESS`
- `Stage 01 Toolchain Smoke` run `32760139285` / #51: `SUCCESS`. Bu CI clean MAUI/toolchain regresyonudur; fiziksel Android cihaz install/launch veya iOS erişim kanıtı değildir.

PR #7 bu doğrulanmış head üzerinden `main`e merge edildi. Merge commit: `bbe5b62224ae6e7fdaebd1c1c6ace87418f09b9f`.

## Corpus ölçüm özeti

Upstream ana setin DWG/DXF karşılıklarında gerekli semantic dağılım korunuyor. Dört DWG ve iki ASCII DXF fixture'ında:

- Model-space entity: `163`
- Total block entity: `341`
- Block-reference/INSERT: `14`
- DIMENSION: `11`
- HATCH: `8`
- LINE: `98`
- CIRCLE: `13`
- Layout: `4`
- Block record: `27`

Layer sayısı AC1015/AC1018 için `21`, AC1024/AC1032 için `19` olarak parse edildi; aynı version familyasının DWG/DXF karşılıkları eşleşiyor.

Final PR-head GitHub hosted runner koşusundaki parse timing tanısaldır, performans garantisi değildir:

| Fixture | Parse ms |
|---|---:|
| AC1015 DWG | 554.8 |
| AC1018 DWG | 113.7 |
| AC1024 DWG | 92.0 |
| AC1032 DWG | 80.7 |
| AC1015 ASCII DXF | 554.3 |
| AC1032 ASCII DXF | 407.7 |
| Synthetic Turkish/basic DXF | 7.2 |

## Diagnostics / compatibility bulguları

- Upstream fixture'larda CI host üzerinde SHX dosyaları mevcut olmadığı için `missing-font` compatibility kaydı görülebilir; proprietary SHX bundle edilmedi.
- ASCII DXF fixture'larında `unsupported-object` compatibility sınıfı görülebilir. Buna rağmen manifestin kritik entity semantiği geçti; fixed warning-count eşiği uygulanmadı.
- AC1015 ASCII DXF notification hacmi yüksektir. Notification sayısı tek başına fidelity başarısızlığı sayılmadı; semantic/golden beklentiler ayrı doğrulandı.
- Derived corrupt AC1018 DWG parser tarafından warning ile açılabiliyor; AŞAMA 19 input/resource guard'ları ayrıca gereklidir.
- Derived truncated AC1015 DWG kontrollü `EndOfStreamException` üretiyor.

## Lisans / provenance

- ACadSharp `3.7.1`: MIT; AŞAMA 02 `GREEN` dependency/lisans adayı.
- Exact package content hash lockfile'da sabit.
- Stage 02 exact nupkg SHA-256/license/native-entry kanıtı değişmedi.
- Yeni native binary, font veya proprietary asset eklenmedi.
- Upstream test binary'leri mobil-dwg reposuna vendored edilmedi; Stage 03 immutable revision + hash doğrulamasıyla CI cache'e indirildi.

## Parser kararı

`docs/ADR/0001-acadsharp-3.7.1-parser-baseline.md` kararı: ACadSharp `3.7.1` **read-only parser baseline GO**.

Bu GO şu anlama gelmez:

- render fidelity kanıtlandı,
- bütün DWG/DXF dosyaları destekleniyor,
- unsupported/proxy entity'ler doğru render edilecek,
- parser cooperative cancellation destekliyor,
- performans hedefleri gerçek mobil cihazda geçti.

Kritik corpus parse kaybı görülmediği için bu aşamada ACadSharp sürüm A/B veya IxMilia.Dxf fallback spike başlatılmadı.

## Known failures / risk

- Missing external fontlar compatibility raporunda kalır; AŞAMA 14 font/SHX çözümünü ele alır.
- Unsupported/proxy notification'ların parse sonrası scene/render etkisi AŞAMA 09+ ve compatibility aşamalarında fixture bazında doğrulanmalıdır.
- Corrupt dosyanın warning ile kısmen açılması güvenli input kabulü değildir; AŞAMA 19 guard'ları gerekir.
- Parser başladıktan sonra hard/cooperative cancel kanıtı yoktur.
- AŞAMA 01 fiziksel Android install/launch ve gerçek iOS erişim envanteri `DEFERRED_EXTERNAL_GATE` olarak açık kalır; bu belge onları PASS/DONE saymaz.

## Sonraki eylem

AŞAMA 05 `DONE` ve PR #7 `main`e merge edilmiştir. İlk bağımsız çalışma aşaması AŞAMA 06 — Android güvenli dosya alma ve parse spike — olacaktır. Bu AŞAMA 05 kapanış turunda AŞAMA 06 başlatılmaz.
