$ErrorActionPreference = 'Stop'

$utf8 = New-Object System.Text.UTF8Encoding($false)
$dataPath = 'scripts/stage09-postmerge-closure-data.json'
$dataText = [System.IO.File]::ReadAllText($dataPath, $utf8)
$data = $dataText | ConvertFrom-Json

foreach ($op in $data.operations) {
    $path = [string]$op.path
    $old = [string]$op.old
    $new = [string]$op.new

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing target file: $path"
    }

    $text = [System.IO.File]::ReadAllText($path, $utf8)
    $text = $text.Replace("`r`n", "`n")

    if (-not $text.Contains($old)) {
        throw "Expected Stage09 closure text not found in $path"
    }

    $text = $text.Replace($old, $new)
    [System.IO.File]::WriteAllText($path, $text, $utf8)
    Write-Host "PATCHED $path"
}

Write-Host 'STAGE09_POSTMERGE_CLOSURE_PATCH_PASS'
