$ErrorActionPreference = 'Stop'

$utf8 = New-Object System.Text.UTF8Encoding($false)
$path = 'Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md'
$dataPath = 'scripts/stage09-canonical-closure-data.json'
$text = [System.IO.File]::ReadAllText($path, $utf8)
$dataText = [System.IO.File]::ReadAllText($dataPath, $utf8)
$data = $dataText | ConvertFrom-Json

$checkpointStartMarker = 'CURRENT_STAGE:'
$checkpointEndMarker = 'LAST_UPDATE:'
$checkpointStart = $text.IndexOf($checkpointStartMarker, [System.StringComparison]::Ordinal)
if ($checkpointStart -lt 0) { throw 'Checkpoint start not found' }
$checkpointEndLineStart = $text.IndexOf($checkpointEndMarker, $checkpointStart, [System.StringComparison]::Ordinal)
if ($checkpointEndLineStart -lt 0) { throw 'Checkpoint end not found' }
$checkpointEnd = $text.IndexOf("`n", $checkpointEndLineStart, [System.StringComparison]::Ordinal)
if ($checkpointEnd -lt 0) { $checkpointEnd = $text.Length } else { $checkpointEnd += 1 }
$text = $text.Substring(0, $checkpointStart) + [string]$data.checkpoint + "`n" + $text.Substring($checkpointEnd)

$lines = $text -split "`r?`n"
$indexUpdated = $false
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i].Contains([string]$data.stageIndexContains) -and $lines[$i].StartsWith('- [ ] ')) {
        $lines[$i] = [string]$data.stageIndexLine
        $indexUpdated = $true
        break
    }
}
if (-not $indexUpdated) { throw 'Stage index line not found' }
$text = [string]::Join("`n", $lines)

if ($text.Contains([string]$data.goOld)) {
    $text = $text.Replace([string]$data.goOld, [string]$data.goNew)
}

$lastNeedle = $text.LastIndexOf([string]$data.stageTitleNeedle, [System.StringComparison]::Ordinal)
if ($lastNeedle -lt 0) { throw 'Stage section title not found' }
$stageStart = $text.LastIndexOf('### ', $lastNeedle, [System.StringComparison]::Ordinal)
if ($stageStart -lt 0) { throw 'Stage section start not found' }
$nextTitle = $text.IndexOf([string]$data.nextStageTitleNeedle, $lastNeedle, [System.StringComparison]::Ordinal)
if ($nextTitle -lt 0) { throw 'Next stage title not found' }
$stageEnd = $text.LastIndexOf('### ', $nextTitle, [System.StringComparison]::Ordinal)
if ($stageEnd -lt 0 -or $stageEnd -le $stageStart) { throw 'Next stage section start not found' }
$text = $text.Substring(0, $stageStart) + [string]$data.stageSection + $text.Substring($stageEnd)

[System.IO.File]::WriteAllText($path, $text, $utf8)
Write-Host 'STAGE09_CANONICAL_CLOSURE_PATCH_PASS'
