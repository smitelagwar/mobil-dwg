param(
    [string]$InputDxf = "fixtures/public/synthetic/synthetic_turkish_basic_ac1015.dxf",
    [string]$OutputDwg = "artifacts/stage03/synthetic_turkish_basic_ac1015.dwg"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$inputPath = if ([System.IO.Path]::IsPathRooted($InputDxf)) { $InputDxf } else { Join-Path $repoRoot $InputDxf }
$outputPath = if ([System.IO.Path]::IsPathRooted($OutputDwg)) { $OutputDwg } else { Join-Path $repoRoot $OutputDwg }

if (-not (Test-Path -LiteralPath $inputPath -PathType Leaf)) {
    throw "Synthetic DXF source not found: $inputPath"
}

$dotnetVersion = (& dotnet --version).Trim()
if ($dotnetVersion -ne "10.0.400") {
    throw "Expected .NET SDK 10.0.400, got $dotnetVersion"
}

$outputDir = Split-Path -Parent $outputPath
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
if (Test-Path -LiteralPath $outputPath) {
    Remove-Item -LiteralPath $outputPath -Force
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("mobil-dwg-stage03-dwg-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

try {
    $projectPath = Join-Path $tempRoot "Stage03SyntheticDwgGenerator.csproj"
    $programPath = Join-Path $tempRoot "Program.cs"

    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ACadSharp" Version="[3.7.1]" />
  </ItemGroup>
</Project>
'@ | Set-Content -LiteralPath $projectPath -Encoding UTF8

    @'
using ACadSharp.IO;

if (args.Length != 2)
{
    throw new ArgumentException("Expected input DXF and output DWG paths.");
}

string input = Path.GetFullPath(args[0]);
string output = Path.GetFullPath(args[1]);

using (DxfReader reader = new DxfReader(input))
{
    var document = reader.Read();
    using (DwgWriter writer = new DwgWriter(output, document))
    {
        writer.Write();
    }
}

byte[] prefix = File.ReadAllBytes(output).Take(6).ToArray();
string magic = System.Text.Encoding.ASCII.GetString(prefix);
if (magic != "AC1015")
{
    throw new InvalidDataException($"Expected AC1015 DWG magic, got {magic}.");
}

using (DwgReader verifyReader = new DwgReader(output))
{
    _ = verifyReader.Read();
}

var info = new FileInfo(output);
if (info.Length <= 6)
{
    throw new InvalidDataException("Generated DWG is unexpectedly small.");
}

Console.WriteLine($"STAGE03_SYNTHETIC_DWG_GENERATED magic={magic} bytes={info.Length}");
Console.WriteLine("STAGE03_SYNTHETIC_DWG_READBACK_PASS");
'@ | Set-Content -LiteralPath $programPath -Encoding UTF8

    & dotnet restore $projectPath --nologo
    if ($LASTEXITCODE -ne 0) { throw "Synthetic DWG generator restore failed." }

    $packageJson = & dotnet list $projectPath package --format json
    if ($LASTEXITCODE -ne 0) { throw "Synthetic DWG generator package graph failed." }
    $packageText = ($packageJson | Out-String)
    if ($packageText -notmatch '"id"\s*:\s*"ACadSharp"' -or $packageText -notmatch '"resolvedVersion"\s*:\s*"3\.7\.1"') {
        throw "Synthetic DWG generator did not resolve exact ACadSharp 3.7.1."
    }
    Write-Host "STAGE03_SYNTHETIC_DWG_PACKAGE_PASS"

    & dotnet run --project $projectPath --no-restore --configuration Release -- $inputPath $outputPath
    if ($LASTEXITCODE -ne 0) { throw "Synthetic DWG generation failed." }
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
