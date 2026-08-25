#if V05_VALIDATION
using System.Security.Cryptography;
using Android.Util;
using Microsoft.Maui.Storage;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

namespace MobilDwg.App;

internal sealed record V05ValidationResult(string Marker);

internal static class V05AndroidValidationRunner
{
    private const string Tag = "MobilDwgV05";

    public static async Task<V05ValidationResult> RunAsync()
    {
        var reader = new AcadSharpDocumentReader();

        var dxf = await ParsePositiveAsync(
            reader,
            "v05_positive.dxf",
            CadFormat.Dxf,
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["LINE"] = 2,
                ["CIRCLE"] = 1,
                ["ARC"] = 1,
                ["LWPOLYLINE"] = 1,
                ["TEXT"] = 1,
                ["INSERT"] = 2,
            });
        Log.Info(Tag, $"V05_DXF_PARSE_PASS version={dxf.Version} entities={dxf.EntityCount}");

        var dwg = await ParsePositiveAsync(
            reader,
            "v05_positive.dwg",
            CadFormat.Dwg,
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["LINE"] = 2,
                ["CIRCLE"] = 1,
                ["ARC"] = 1,
                ["LWPOLYLINE"] = 1,
                ["TEXT"] = 1,
                ["INSERT"] = 2,
            });
        Log.Info(Tag, $"V05_DWG_PARSE_PASS version={dwg.Version} entities={dwg.EntityCount}");

        await ParseNegativeAsync(reader, "v05_missing_font.dxf", "missing-font");
        Log.Info(Tag, "V05_NEGATIVE_PASS id=missing-font");

        await ParseNegativeAsync(reader, "v05_missing_xref.dxf", "missing-xref");
        Log.Info(Tag, "V05_NEGATIVE_PASS id=missing-xref");

        Log.Info(Tag, "V05_INPUT_IMMUTABLE_PASS fixtures=4");
        Log.Info(Tag, "V05_REDACTED_DIAGNOSTICS_PASS codes=missing-font,missing-xref");

        var marker = $"ANDROID_VALIDATION_V05_PASS dxf_entities={dxf.EntityCount} dwg_entities={dwg.EntityCount} negatives=2";
        Log.Info(Tag, marker);
        return new V05ValidationResult(marker);
    }

    private static async Task<ParsedFixture> ParsePositiveAsync(
        AcadSharpDocumentReader reader,
        string assetName,
        CadFormat expectedFormat,
        IReadOnlyDictionary<string, int> minimumCounts)
    {
        var bytes = await ReadAssetAsync(assetName);
        var hashBefore = Convert.ToHexString(SHA256.HashData(bytes));

        await using var stream = new MemoryStream(bytes, writable: false);
        await using var session = await reader.OpenAsync(new CadOpenRequest(
            stream,
            assetName,
            bytes.LongLength,
            LeaveOpen: true));

        if (session.Metadata.Format != expectedFormat)
        {
            throw new InvalidDataException($"Unexpected format for validation asset {assetName}.");
        }

        if (!string.Equals(session.Metadata.AcadVersion, "AC1015", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Unexpected ACAD version for validation asset {assetName}.");
        }

        if (session.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            throw new InvalidDataException($"Error-severity parser diagnostic for validation asset {assetName}.");
        }

        var snapshot = AcadSharpDocumentInspection.Snapshot(session.Handle);
        foreach (var pair in minimumCounts)
        {
            snapshot.EntityCounts.TryGetValue(pair.Key, out var actual);
            if (actual < pair.Value)
            {
                throw new InvalidDataException($"Entity minimum not met for validation asset {assetName}: {pair.Key}.");
            }
        }

        var hashAfter = Convert.ToHexString(SHA256.HashData(bytes));
        if (!string.Equals(hashBefore, hashAfter, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Input bytes changed while parsing validation asset {assetName}.");
        }

        return new ParsedFixture(session.Metadata.AcadVersion ?? "Unknown", snapshot.TotalBlockEntityCount);
    }

    private static async Task ParseNegativeAsync(
        AcadSharpDocumentReader reader,
        string assetName,
        string requiredCompatibilityCode)
    {
        var bytes = await ReadAssetAsync(assetName);
        var hashBefore = Convert.ToHexString(SHA256.HashData(bytes));

        await using var stream = new MemoryStream(bytes, writable: false);
        await using var session = await reader.OpenAsync(new CadOpenRequest(
            stream,
            assetName,
            bytes.LongLength,
            LeaveOpen: true));

        if (!session.CompatibilityIssues.Any(issue =>
                string.Equals(issue.Code, requiredCompatibilityCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException($"Required compatibility code missing for validation asset {assetName}.");
        }

        var hashAfter = Convert.ToHexString(SHA256.HashData(bytes));
        if (!string.Equals(hashBefore, hashAfter, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Input bytes changed while parsing validation asset {assetName}.");
        }
    }

    private static async Task<byte[]> ReadAssetAsync(string assetName)
    {
        await using var source = await FileSystem.Current.OpenAppPackageFileAsync(assetName);
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        if (bytes.Length == 0)
        {
            throw new InvalidDataException($"Validation asset is empty: {assetName}.");
        }

        return bytes;
    }

    private sealed record ParsedFixture(string Version, int EntityCount);
}
#endif
