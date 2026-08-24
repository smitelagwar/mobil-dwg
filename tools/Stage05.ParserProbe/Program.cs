using System.Text.Json;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

var options = ParseArgs(args);
var repoRoot = FindRepoRoot();
var manifestPath = Path.GetFullPath(Path.Combine(repoRoot, options.Manifest));
var cacheRoot = Path.GetFullPath(options.Cache);
var evidencePath = Path.GetFullPath(options.Evidence);

using var manifestDocument = JsonDocument.Parse(File.ReadAllText(manifestPath));
var root = manifestDocument.RootElement;
var reader = new AcadSharpDocumentReader();
var failures = new List<string>();
var fixtureEvidence = new List<object>();
var derivedEvidence = new List<object>();

foreach (var fixture in root.GetProperty("fixtures").EnumerateArray())
{
    var id = fixture.GetProperty("id").GetString() ?? throw new InvalidDataException("fixture id missing");
    var format = fixture.GetProperty("format").GetString() ?? throw new InvalidDataException($"{id}: format missing");
    var acadVersion = fixture.GetProperty("acad_version").GetString() ?? throw new InvalidDataException($"{id}: acad_version missing");
    var storage = fixture.GetProperty("storage");
    var mode = storage.GetProperty("mode").GetString() ?? throw new InvalidDataException($"{id}: storage mode missing");
    var path = mode switch
    {
        "remote-pinned" => Path.Combine(cacheRoot, "remote", $"{id}.{format}"),
        "committed" => Path.Combine(repoRoot, storage.GetProperty("path").GetString() ?? throw new InvalidDataException($"{id}: committed path missing")),
        "private-local" => null,
        _ => throw new InvalidDataException($"{id}: unsupported storage mode {mode}"),
    };

    if (path is null)
    {
        continue;
    }

    try
    {
        await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var session = await reader.OpenAsync(new CadOpenRequest(stream, Path.GetFileName(path), stream.Length, LeaveOpen: true));
        var snapshot = AcadSharpDocumentInspection.Snapshot(session.Handle);
        var categories = GetCategories(session);

        Assert(session.Metadata.Format == (format == "dwg" ? CadFormat.Dwg : CadFormat.Dxf),
            $"{id}: metadata format {session.Metadata.Format} != {format}");
        Assert(string.Equals(session.Metadata.AcadVersion, acadVersion, StringComparison.OrdinalIgnoreCase),
            $"{id}: parsed version {session.Metadata.AcadVersion ?? "<null>"} != {acadVersion}");
        Assert(!session.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            $"{id}: successful parse returned error-severity diagnostic");

        ValidateExpectedCounts(id, fixture.GetProperty("expected").GetProperty("entity_counts"), snapshot.EntityCounts);
        ValidateWarnings(id, fixture.GetProperty("expected").GetProperty("warnings"), categories);

        fixtureEvidence.Add(new
        {
            id,
            status = "success",
            metadata = new { format = session.Metadata.Format.ToString(), acadVersion = session.Metadata.AcadVersion },
            parseMilliseconds = Math.Round(snapshot.ParseMilliseconds, 3),
            snapshot.LayerCount,
            snapshot.BlockCount,
            snapshot.LayoutCount,
            snapshot.ModelSpaceEntityCount,
            snapshot.TotalBlockEntityCount,
            entityCounts = snapshot.EntityCounts.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value),
            diagnosticSummary = session.Diagnostics.GroupBy(d => d.Severity.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            compatibilityCodes = session.CompatibilityIssues.Select(issue => issue.Code).Distinct(StringComparer.Ordinal).Order().ToArray(),
        });

        Console.WriteLine($"STAGE05_FIXTURE_PASS id={id} version={session.Metadata.AcadVersion} entities={snapshot.TotalBlockEntityCount} ms={snapshot.ParseMilliseconds:F1}");
    }
    catch (Exception ex)
    {
        failures.Add($"{id}: {ex.GetType().Name}: {ex.Message}");
        fixtureEvidence.Add(new { id, status = "failure", exception = ex.GetType().FullName, message = ex.Message });
        Console.Error.WriteLine($"STAGE05_FIXTURE_FAIL id={id} type={ex.GetType().Name} message={ex.Message}");
    }
}

foreach (var derived in root.GetProperty("negative_derivations").EnumerateArray())
{
    var id = derived.GetProperty("id").GetString() ?? throw new InvalidDataException("derived id missing");
    var path = Path.Combine(cacheRoot, "derived-negative", $"{id}.dwg");
    var expectedResult = derived.GetProperty("expected").GetProperty("open_result").GetString() ?? string.Empty;

    CadDocumentSession? session = null;
    Exception? parseException = null;
    try
    {
        await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        session = await reader.OpenAsync(new CadOpenRequest(stream, Path.GetFileName(path), stream.Length, LeaveOpen: true));
    }
    catch (Exception ex)
    {
        parseException = ex;
    }

    if (parseException is not null)
    {
        derivedEvidence.Add(new { id, status = "controlled-failure", exception = parseException.GetType().FullName, message = parseException.Message });
        Console.WriteLine($"STAGE05_NEGATIVE_PASS id={id} outcome=failure type={parseException.GetType().Name}");
        continue;
    }

    if (session is null)
    {
        failures.Add($"{id}: parser returned neither session nor exception");
        derivedEvidence.Add(new { id, status = "failure", message = "parser returned neither session nor exception" });
        continue;
    }

    await using (session)
    {
        try
        {
            var categories = GetCategories(session);
            var hasWarning = session.Diagnostics.Any(d => d.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error)
                || session.CompatibilityIssues.Count > 0;

            Assert(expectedResult == "controlled-failure-or-warning",
                $"{id}: parser unexpectedly succeeded; expected {expectedResult}");
            Assert(hasWarning,
                $"{id}: parser succeeded without any warning/compatibility signal on corrupted input");

            derivedEvidence.Add(new
            {
                id,
                status = "controlled-warning",
                diagnosticSummary = session.Diagnostics.GroupBy(d => d.Severity.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                categories = categories.Order().ToArray(),
            });
            Console.WriteLine($"STAGE05_NEGATIVE_PASS id={id} outcome=warning");
        }
        catch (Exception ex)
        {
            failures.Add($"{id}: {ex.GetType().Name}: {ex.Message}");
            derivedEvidence.Add(new { id, status = "failure", exception = ex.GetType().FullName, message = ex.Message });
            Console.Error.WriteLine($"STAGE05_NEGATIVE_FAIL id={id} type={ex.GetType().Name} message={ex.Message}");
        }
    }
}

Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);
var evidence = new
{
    schemaVersion = 1,
    parser = new { package = "ACadSharp", version = "3.7.1" },
    manifest = Path.GetRelativePath(repoRoot, manifestPath).Replace('\\', '/'),
    generatedAtUtc = DateTimeOffset.UtcNow,
    fixtures = fixtureEvidence,
    derivedNegatives = derivedEvidence,
    failures,
};
File.WriteAllText(evidencePath, JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

if (failures.Count > 0)
{
    throw new InvalidOperationException("Stage 05 corpus failures:\n" + string.Join("\n", failures));
}

Console.WriteLine($"STAGE05_MINI_CORPUS_PASS fixtures={fixtureEvidence.Count} derived_negatives={derivedEvidence.Count}");

static HashSet<string> GetCategories(CadDocumentSession session)
{
    var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var issue in session.CompatibilityIssues)
    {
        categories.Add(issue.Code);
    }

    foreach (var diagnostic in session.Diagnostics)
    {
        if (diagnostic.Severity == DiagnosticSeverity.Error)
        {
            categories.Add("fatal-corrupt");
        }

        var text = $"{diagnostic.Code} {diagnostic.Message}";
        if (text.Contains("proxy", StringComparison.OrdinalIgnoreCase))
        {
            categories.Add("proxy-object");
        }
        if (text.Contains("not supported", StringComparison.OrdinalIgnoreCase)
            || text.Contains("unsupported", StringComparison.OrdinalIgnoreCase))
        {
            categories.Add("unsupported-object");
        }
        if (text.Contains("external", StringComparison.OrdinalIgnoreCase)
            || text.Contains("xref", StringComparison.OrdinalIgnoreCase))
        {
            categories.Add("external-reference");
        }
        if (text.Contains("missing", StringComparison.OrdinalIgnoreCase)
            || text.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            categories.Add("missing-resource");
        }
    }

    return categories;
}

static void ValidateExpectedCounts(string id, JsonElement expected, IReadOnlyDictionary<string, int> actual)
{
    var mode = expected.GetProperty("mode").GetString() ?? "minimum";
    foreach (var property in expected.EnumerateObject())
    {
        if (property.NameEquals("mode"))
        {
            continue;
        }

        var expectedCount = property.Value.GetInt32();
        actual.TryGetValue(property.Name, out var actualCount);
        if (mode == "exact")
        {
            Assert(actualCount == expectedCount,
                $"{id}: entity {property.Name} count {actualCount} != exact {expectedCount}");
        }
        else
        {
            Assert(actualCount >= expectedCount,
                $"{id}: entity {property.Name} count {actualCount} < minimum {expectedCount}");
        }
    }
}

static void ValidateWarnings(string id, JsonElement expected, HashSet<string> actual)
{
    if (expected.TryGetProperty("must_include", out var mustInclude))
    {
        foreach (var item in mustInclude.EnumerateArray())
        {
            var category = item.GetString() ?? string.Empty;
            Assert(actual.Contains(category), $"{id}: required warning category missing: {category}");
        }
    }

    if (expected.TryGetProperty("must_not_include", out var mustNotInclude))
    {
        foreach (var item in mustNotInclude.EnumerateArray())
        {
            var category = item.GetString() ?? string.Empty;
            Assert(!actual.Contains(category), $"{id}: forbidden warning category observed: {category}");
        }
    }
}

static (string Manifest, string Cache, string Evidence) ParseArgs(string[] args)
{
    string? manifest = null;
    string? cache = null;
    string? evidence = null;

    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--manifest" when i + 1 < args.Length:
                manifest = args[++i];
                break;
            case "--cache" when i + 1 < args.Length:
                cache = args[++i];
                break;
            case "--evidence" when i + 1 < args.Length:
                evidence = args[++i];
                break;
            default:
                throw new ArgumentException($"Unknown or incomplete argument: {args[i]}");
        }
    }

    return (
        manifest ?? "fixtures/manifest/stage03-mini.json",
        cache ?? throw new ArgumentException("--cache is required"),
        evidence ?? throw new ArgumentException("--evidence is required"));
}

static string FindRepoRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "MobilDwg.sln")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("repository root containing MobilDwg.sln was not found");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
