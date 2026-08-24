# Execution Log

Bu dosya `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md` içindeki aşamaların teknik yürütme geçmişini özetler. İnsan ve yeni ajan devri için birincil anlık checkpoint kökteki `gecmis.md` dosyasıdır. Aşama bazlı ayrıntılı artifact/evidence kayıtları `docs/evidence/` ve `compliance/` altında tutulur.

## Kayıt kuralları

Her çalışma turunda mümkün olduğunca tarih, aktif aşama, başlangıç revision, değiştirilen dosyalar, gerçek test/CI sonucu, artifact, risk/blocker ve tek sonraki eylem kaydedilir. Başarı kanıtsız yazılmaz. Müşteri çizimleri, secret/signing materyali, cihaz seri/UDID gibi hassas bilgiler kaydedilmez.

---

## 2026-08-24 — AŞAMA 00: Çalışma alanı ve yürütme zemini

Durum: `DONE`

Başlangıç:

- Repo: `smitelagwar/mobil-dwg`
- Branch: `main`
- Başlangıç revision: `d161b5c4f9ba238f0d2a2e4c92f773535f379487`
- Uygulama kodu yok; plan ve araştırma belgeleri vardı.

Yapılanlar:

- Repo geçmişi, mevcut belgeler ve `.gitignore` doğrulandı.
- `.gitignore` build/temp/private CAD corpus/signing secret/font/CAD asset koruması açısından yeterli bulundu; gereksiz değişiklik yapılmadı.
- Kullanıcıdan yüklenen nihai plan ile GitHub başlangıç plan blob'u birebir eşleşti: Git blob SHA `a05dc53df058c5355f8576996a33cce704ac19f3`.
- `docs/EXECUTION_LOG.md`, `docs/ADR/0000-template.md`, `docs/EVIDENCE_TEMPLATE.md`, `gecmis.md` oluşturuldu.
- README yeni sohbet/yeni AI handoff akışına bağlandı.

ChatGPT çalışma konteyneri envanteri o tarihte:

- Linux x86_64.
- Git `2.47.3`.
- Java OpenJDK `21.0.11`.
- `dotnet` ve `adb` PATH üzerinde yoktu.
- Bu envanter kullanıcının fiziksel geliştirme makinesi değildir.

Kapanış plan commit'i: `fe3c8c043e6d373e6313d2e1201cc24992b493a9`.

---

## 2026-08-24 — AŞAMA 01: .NET/MAUI/Android toolchain ve gerçek telefon

Durum: `BLOCKED / DEFERRED_EXTERNAL_GATE`

AŞAMA 01'in fiziksel cihazdan bağımsız kısmı gerçek GitHub Actions koşularıyla kanıtlandı; gerçek Android cihaz install/launch ve iOS erişim envanteri hâlâ dış erişim gerektiriyor.

Canlı doğrulanan/pinlenen toolchain:

- .NET SDK/workload set `10.0.400`.
- Runtime servicing `10.0.11`.
- Microsoft OpenJDK `21.0.12`.
- Android min API `24`.
- Android target/compile API `36`.
- Android SDK Platform 36 revision `1`.
- Build-Tools `36.0.0`.
- Platform-Tools/ADB `37.0.1`.
- Android command-line tools build ID `15859902`.
- `maui-android` workload.

Repo öğeleri:

- `global.json`
- `docs/TOOLCHAIN.md`
- `docs/evidence/STAGE_01.md`
- `.github/workflows/stage01-toolchain-smoke.yml`
- `scripts/stage01-device-gate.sh`
- `scripts/stage01-device-gate.ps1`
- `scripts/stage01-ios-inventory.sh`
- `docs/STAGE_01_IOS_ACCESS_INVENTORY.md`

Önemli CI gelişimi:

- İlk koşularda manifest path/`pipefail`, temiz MAUI min API 21 gerçeği ve geçici JDK download hatası yakalandı.
- Proje policy'si gereği minimum API 24 açıkça pinlendi.
- Exact Microsoft JDK artifact checksum doğrulaması korundu ve download retry eklendi.
- PR #1 merge `83379b24e4ba87f04299f612ae2951ae8d8aec13`.
- Fiziksel cihaz gate otomasyonu PR #2 merge `9b375af9931a3db23f82e9b983257f29030a7376`.
- iOS erişim envanteri standardizasyonu PR #3 merge `9a397065a55c5844993e6ef909438f44ad5aa1f6`.

Stage 01'in son bağımsız regresyon kanıtı Stage 02 PR head üzerinde yeniden alındı:

- Workflow: `Stage 01 Toolchain Smoke`.
- Run `32747785948` / #29: `SUCCESS`.
- Exact toolchain/workload: PASS.
- Temiz MAUI proje üretimi: PASS.
- Debug build: PASS.
- Release build: PASS.
- Manifest minSdk 24 / targetSdk 36: PASS.
- Artifact upload: PASS.
- Artifact ID `9528014030`.
- Artifact digest `sha256:57f01ed14600684a5a9434b9ca2db2b6e32b4a9fac95bee90d455a4595e8421e`.

Bu CI fiziksel telefon kanıtı değildir.

Açık zorunlu dış kapılar:

- Gerçek geliştirme makinesinde `STAGE01_DEVICE_GATE_PASS`.
- Fiziksel Android cihazda ADB `state=device`.
- APK install.
- Launcher `adb shell am start -W` ile gerçek launch PASS.
- Mac/Xcode/iPhone/Apple Developer erişim envanterinin gerçek `YES/NO/N/A` değerleri.

### Kullanıcı onaylı yürütme istisnası

Kullanıcı 2026-08-24 tarihinde bu dış erişimleri şu an sağlayamayacağını ve yalnız `devam` komutuyla bağımsız işleri ilerletmek istediğini açıkça belirtti. Bu nedenle `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` yürürlüktedir:

- AŞAMA 01 `DONE` yapılmaz.
- Dış kapılar `DEFERRED_EXTERNAL_GATE` olarak açık kalır.
- Fiziksel cihaz/hesap erişimine bağımlı olmayan sonraki aşamalar ilerleyebilir.
- Sahte PASS/DONE üretilmez.
- Release/beta/final gerçek cihaz kapıları değişmez.

---

## 2026-08-24 — AŞAMA 02: Canlı dependency/lisans kanıtı ve kilitler

Durum: `DONE`

Başlangıç main revision: `815494d035f9c97a4f2ba6e77e97ec2c374f0080`.

Amaç: production/runtime dependency eklemeden önce exact sürüm, lisans, transitive/native graph ve source lineage gerçekliğini kanıtlamak.

### Canlı doğrulanan adaylar

ACadSharp:

- NuGet current stable `3.7.1`, 2026-08-18.
- NuGet license MIT.
- `net10.0` dependency group'ta ek NuGet dependency yok.
- Source version bump commit `bbc8b14a92ebfac35bb77c0c1a4af70de90ebb50`.
- Source build `CSUtilities` submodule kullanıyor; lisansı MIT.
- Karar: dependency/lisans açısından `GREEN`; CAD fidelity AŞAMA 05 geçmeden production-approved değildir.

SkiaSharp:

- Stable `4.151.1`.
- NuGet/repo license MIT.
- Android resolved graph: `SkiaSharp 4.151.1 -> SkiaSharp.NativeAssets.Android 4.151.1`.
- Native Android package arm/arm64/x64/x86 `libSkiaSharp.so` içeriyor.
- Upstream Skia BSD-3-Clause.
- Karar: `REVIEW`; final native binary/third-party inventory release gate'inde yeniden açılır.

ProCad:

- Source snapshot `f8a862b3e7634e27664fee02ff5d68774b102985`.
- Repo license MIT.
- `external/ACadSharp` fork submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36`.
- `external/ProEdit` submodule `64759b79289a024d08463ed1a9094fdcd9a270df`.
- Snapshot package hattında Skia/MAUI version skew ve prerelease view dependency görüldü.
- README `ProCadSharp.*` package ID'lerini tanımlasa da Stage 02 canlı NuGet exact aramasında production için güvenilecek yayımlanmış exact paket hattı bulunmadı.
- Karar: `REVIEW`, production default `NO-GO`; yalnız AŞAMA 07 exact source-pinned spike.

IxMilia:

- IxMilia.Dxf `0.8.4`, MIT: yalnız test/fallback scope'unda `GREEN`.
- IxMilia.Dwg source `269c8a4858cb0f836a7f3f70ba18a67dbafcb05c`, MIT: `REVIEW`, modern DWG fallback değil.
- IxMilia.Shx source `4294bfec27b945c56f18c54ae79ff386238475be`, MIT: `REVIEW`, yalnız gelecekte parser spike adayı; font asset değildir.

### Repo mekanizmaları

AŞAMA 02 ile eklenen/kurulan temel öğeler:

- `Directory.Packages.props`
- `compliance/LICENSE_POLICY.md`
- `compliance/DEPENDENCY_EVIDENCE.md`
- `compliance/RISK_REGISTER.md`
- `compliance/Stage02.DependencyProbe/Stage02.DependencyProbe.csproj`
- `compliance/Stage02.DependencyProbe/packages.lock.json`
- `compliance/stage02-package-manifest.json`
- `scripts/stage02-audit-packages.py`
- `.github/workflows/stage02-dependency-audit.yml`
- `docs/evidence/STAGE_02.md`

Exact central pinler:

- ACadSharp `3.7.1`.
- SkiaSharp `4.151.1`.
- IxMilia.Dxf `0.8.4` yalnız test/fallback candidate olarak pinli; production dependency probe referansı yok.

Resolved Android production probe graph:

- Direct `ACadSharp 3.7.1`.
- Direct `SkiaSharp 4.151.1`.
- Transitive `SkiaSharp.NativeAssets.Android 4.151.1`.
- TFM `net10.0-android36.0`.

Committed lockfile SHA-256:

- `880bdb834856010d1a08821e72f539208170c9e8a929e183c17eaf7dee2d362d`.

Committed package manifest SHA-256:

- `04350e4ea477131ad19f5b06ae28deb0d4c0c1effd107d66178ee7d3d64fb02c`.

Exact NuGet artifact SHA-256:

- ACadSharp 3.7.1: `4f9ca3a5dafd1a18af651312522147a3163999818763d168b4d5f59d6ffc1701`.
- SkiaSharp 4.151.1: `2d1feef23f28e55864cad8449f7b60abf5d6db1aa61ec07aef837e9e0eaee73e`.
- SkiaSharp.NativeAssets.Android 4.151.1: `0857f22d4de9f87899675a30312c52801c6ff85e7ca25dc9483a969c43612803`.

### Stage 01 root-CPM regresyonu

Kök `Directory.Packages.props` ilk eklendiğinde Stage 01 repo-altı smoke projesi Central Package Management'i miras alıp `NU1008` verdi. Bu CAD dependency uyumsuzluğu değil test izolasyonu hatasıydı.

Düzeltmeler:

- Stage 01 temiz MAUI smoke projesi repo ağacı dışındaki `$RUNNER_TEMP` dizinine taşındı.
- İlk düzeltmedeki workflow-level `${{ runner.temp }}` kullanımı workflow parse/trigger seviyesinde sorun yarattığı için step-level `$RUNNER_TEMP` / `${{ runner.temp }}` kullanımına çevrildi.
- Final aynı-head Stage 01 run `32747785948` / #29 tamamen PASS oldu.

### Final Stage 02 CI

Workflow: `Stage 02 Dependency Audit`.

- Final run `32747785867` / #9.
- Head `7daa5d7dc326915700f60396bdf50604bf0601e7`.
- Sonuç `SUCCESS`.
- Exact .NET/workload: PASS.
- Committed `--locked-mode` restore: PASS.
- Lockfile/manifest reproducibility: PASS.
- Resolved graph: PASS.
- Exact `.nupkg` license/hash audit: PASS.
- Vulnerability check: PASS; mevcut kaynaklara göre vulnerable package yok.
- Evidence artifact upload: PASS.
- Artifact ID `9527769476`.
- Artifact digest `sha256:90d41760e306e13b9977586b9996c1aafdf27f615c2b730bb41d74507b4684f3`.

PR #4 `compliance: establish stage 02 dependency audit` doğrulanmış head `7daa5d7dc326915700f60396bdf50604bf0601e7` üzerinden `main`e merge edildi.

Merge commit: `f0a43db6cc3aee9103f42798fa124da4d1ff39d1`.

Kapanış belge commitleri:

- `a9407be69f226c48ff6b4986d64336ef77f221ef` — dependency evidence finalizasyonu.
- `de77249c2303b01433fed57b68304b1ff1b78020` — Stage 02 closeout evidence finalizasyonu.
- `e087cc24529341e6b954b8a1522c28a27fa85e48` — `gecmis.md` AŞAMA 03 handoff.
- `66ee07429036623e77ffb45fef0775939a0c44f2` — README stage durum senkronizasyonu.

AŞAMA 02 sonucu:

- Unknown/policy-RED resolved production package yok.
- Floating/latest production dependency yok.
- Exact lock + locked restore var.
- Exact NuGet artifact hash/license manifest'i var.
- ProCad production graph'a eklenmedi.
- ACadSharp fidelity kararı bilinçli olarak AŞAMA 05'e bırakıldı.
- SkiaSharp final native third-party binary inventory release öncesi tekrar açılacak.

Sonraki eylem: kullanıcı `devam` dediğinde AŞAMA 03 — test corpus'u, golden sözleşmesi ve cihaz matrisi — başlatılır. Aynı kullanıcı turunda AŞAMA 04'e geçilmez. AŞAMA 01 dış cihaz/iOS kapıları `DEFERRED_EXTERNAL_GATE` olarak açık kalır.
