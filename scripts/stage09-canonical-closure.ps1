$ErrorActionPreference = 'Stop'

$path = 'Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md'
$text = Get-Content -Raw -Encoding UTF8 $path

$checkpoint = @'
CURRENT_STAGE: AŞAMA 09
CURRENT_SUBSTEP: 09.complete
STATUS: DONE
LAST_VERIFIED_REVISION: 7bba0b7a6da30dc4b23050872a7a1ef4e90ca087 — exact .NET 10.0.400 self-hosted execution üzerinde AŞAMA 09 targeted T0/T1 ve full Stage 04 architecture regression PASS
LAST_SUCCESSFUL_COMMAND: Stage 09 Self-Hosted Validation run 32815175055 / #6, job 97701882792 SUCCESS — targeted + full solution Release build 0 warning / 0 error
EVIDENCE: docs/evidence/STAGE_09.md; run 32815175055/#6; artifact 9551137293; sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd; STAGE09_DOTNET_PIN_PASS; STAGE09_T0_BUILD_PASS; STAGE09_RENDER_SCENE_TESTS_PASS; STAGE09_T1_SCENE_PASS; STAGE09_STAGE04_REGRESSION_PASS; render-scene/v1
BLOCKERS: AŞAMA 09 blocker yok. AŞAMA 01/AŞAMA 06 gerçek Android ve AŞAMA 08 local Mac/ios-arm64/physical iPhone kapıları DEFERRED_EXTERNAL_GATE olarak açık kalır; AŞAMA 09 bunları kapatmaz.
NEXT_ACTION: AŞAMA 10 — P0 temel geometri renderer'ı — bir sonraki kullanıcı `devam` turunda başlatılır. Bir turda en fazla bir aşama kuralı gereği bu AŞAMA 09 kapanış turunda AŞAMA 10 başlatılmaz.
LAST_UPDATE: 2026-08-25
'@.TrimEnd()

$checkpointStartMarker = 'CURRENT_STAGE:'
$checkpointEndMarker = 'LAST_UPDATE:'
$checkpointStart = $text.IndexOf($checkpointStartMarker, [System.StringComparison]::Ordinal)
if ($checkpointStart -lt 0) { throw 'Checkpoint start not found' }
$checkpointEndLineStart = $text.IndexOf($checkpointEndMarker, $checkpointStart, [System.StringComparison]::Ordinal)
if ($checkpointEndLineStart -lt 0) { throw 'Checkpoint end not found' }
$checkpointEnd = $text.IndexOf("`n", $checkpointEndLineStart, [System.StringComparison]::Ordinal)
if ($checkpointEnd -lt 0) { $checkpointEnd = $text.Length } else { $checkpointEnd += 1 }
$text = $text.Substring(0, $checkpointStart) + $checkpoint + "`r`n" + $text.Substring($checkpointEnd)

$lines = $text -split "`r?`n"
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i].Contains('RenderScene, kamera ve diagnostics temeli') -and $lines[$i].StartsWith('- [ ] ')) {
        $lines[$i] = '- [x] AŞAMA 09 — RenderScene, kamera ve diagnostics temeli — `DONE`'
        break
    }
}
$text = [string]::Join("`r`n", $lines)

$stageNeedle = 'RenderScene, kamera ve diagnostics temeli'
$lastNeedle = $text.LastIndexOf($stageNeedle, [System.StringComparison]::Ordinal)
if ($lastNeedle -lt 0) { throw 'Stage 09 title text not found' }
$stageStart = $text.LastIndexOf('### ', $lastNeedle, [System.StringComparison]::Ordinal)
if ($stageStart -lt 0) { throw 'Stage 09 section start not found' }
$stage10Needle = 'P0 temel geometri renderer'
$stage10Title = $text.IndexOf($stage10Needle, $lastNeedle, [System.StringComparison]::Ordinal)
if ($stage10Title -lt 0) { throw 'Stage 10 title text not found' }
$stageEnd = $text.LastIndexOf('### ', $stage10Title, [System.StringComparison]::Ordinal)
if ($stageEnd -lt 0 -or $stageEnd -le $stageStart) { throw 'Stage 10 section start not found' }

$stage09 = @'
### AŞAMA 09 — RenderScene, kamera ve diagnostics temeli

**Amaç:** Seçilen yol üzerinde parser’dan bağımsız, test edilebilir sahne çekirdeği.

İşler:

- [x] Tek scene implementasyonu seçildi: ADR 0002 ProCad exact reuse `NO-GO` olduğundan compact özel immutable scene; paralel iki scene graph yok.
- [x] Stable entity ID, bounds, layer/style token ve source reference scene üzerinde modellendi; default-value bypass ve duplicate-ID guard'ları eklendi.
- [x] Document/world koordinatları `double`; world→view→screen tek transform hattıdır; Core `RenderViewport` ile explicit bridge vardır.
- [x] OCS/WCS, extents, invalid NaN/Infinity, finite-overflow ve büyük koordinat hedefli unit-test senaryoları gerçek execution'da geçti; büyük normal vektörleri scaled normalization kullanır.
- [x] Scene build diagnostics `unsupported/substituted/dropped/error` türlerini toplar ve invalid taxonomy reddedilir.
- [x] Camera fit/zoom bounds ve background/color context tanımlandı; invalid/default camera guard'ları eklendi.
- [x] ProCad adapter gereksinimi ADR 0002 nedeniyle `NOT_APPLICABLE`; aynı sınırlar seçilen custom scene yolunda korunur.
- [x] Deterministic `render-scene/v1` semantic snapshot ve insertion-order bağımsızlığı testi gerçek execution'da geçti; Stage 04 render-contract marker'ı korunur.
- [x] Exact .NET `10.0.400` üzerinde T0 restore/build gerçek self-hosted execution ile geçti.
- [x] T1 deterministic scene/camera executable testleri ve full Stage 04 architecture regression gerçek execution ile geçti; evidence artifact/log alındı.

Test: Yetkili kapanış `Stage 09 Self-Hosted Validation` run `32815175055` / #6, job `97701882792`, head `7bba0b7a6da30dc4b23050872a7a1ef4e90ca087`, `SUCCESS`. Targeted ve full solution Release build `0 Warning / 0 Error`. Marker'lar: `STAGE09_DOTNET_PIN_PASS`, `STAGE09_T0_BUILD_PASS`, `STAGE04_CORE_CONTRACT_TESTS_PASS`, `STAGE04_RENDER_CONTRACT_TESTS_PASS`, `STAGE09_RENDER_SCENE_TESTS_PASS`, `render-scene/v1`, `STAGE09_T1_SCENE_PASS`, `STAGE04_ARCHITECTURE_TESTS_PASS`, `STAGE05_DEPENDENCY_BOUNDARY_PASS`, `STAGE04_T0_PASS`, `STAGE09_STAGE04_REGRESSION_PASS`. Artifact `9551137293`, digest `sha256:486c9d0b5a2a35cd4fbb402d9c56ab226a5b6175b8920da95298d18199054ddd`. Survey-origin snapshot `5000000.001` ayrıntısını korudu. Önceki hosted `runner_id=0` kayıtları infrastructure allocation problemi olarak ayrıştırıldı.  
Çıkış: **Sağlandı.** Sentetik scene headless üretilebilir; aynı semantic girdi aynı snapshot'ı verir; precision/OCS/diagnostics/architecture gate'leri gerçek exact toolchain üzerinde PASS. AŞAMA 10 aynı kullanıcı turunda başlatılmaz.

'@

$text = $text.Substring(0, $stageStart) + $stage09 + $text.Substring($stageEnd)
Set-Content -Path $path -Value $text -Encoding UTF8
Write-Host 'STAGE09_CANONICAL_CLOSURE_PATCH_PASS'
