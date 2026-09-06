using System.Xml.Linq;

var repoRoot = FindRepoRoot();
var srcRoot = Path.Combine(repoRoot, "src");
var testsRoot = Path.Combine(repoRoot, "tests");

var productionProjects = Directory.GetFiles(srcRoot, "*.csproj", SearchOption.AllDirectories);
var testProjects = Directory.GetFiles(testsRoot, "*.csproj", SearchOption.AllDirectories);

Assert(productionProjects.Length == 4,
    $"expected exactly 4 production projects under src/, got {productionProjects.Length}");

var expectedTestProjects = new[]
{
    "tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj",
    "tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj",
    "tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj",
    "tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj"
};

var actualTestProjects = testProjects
    .Select(p => Path.GetRelativePath(repoRoot, p).Replace('\\', '/'))
    .Order(StringComparer.Ordinal)
    .ToArray();

Assert(actualTestProjects.SequenceEqual(expectedTestProjects.Order(StringComparer.Ordinal)),
    $"test projects must match exact 4 projects, got: [{string.Join(", ", actualTestProjects)}]");

AssertProjectReferences("src/MobilDwg.Core/MobilDwg.Core.csproj", []);
AssertProjectReferences("src/MobilDwg.Cad/MobilDwg.Cad.csproj",
    ["src/MobilDwg.Core/MobilDwg.Core.csproj"]);
AssertProjectReferences("src/MobilDwg.Rendering/MobilDwg.Rendering.csproj",
    ["src/MobilDwg.Core/MobilDwg.Core.csproj"]);
AssertProjectReferences("src/MobilDwg.App/MobilDwg.App.csproj",
    [
        "src/MobilDwg.Core/MobilDwg.Core.csproj",
        "src/MobilDwg.Cad/MobilDwg.Cad.csproj",
        "src/MobilDwg.Rendering/MobilDwg.Rendering.csproj",
    ]);

AssertProjectReferences("tests/MobilDwg.Core.Tests/MobilDwg.Core.Tests.csproj",
    ["src/MobilDwg.Core/MobilDwg.Core.csproj"]);
AssertProjectReferences("tests/MobilDwg.Rendering.Tests/MobilDwg.Rendering.Tests.csproj",
    [
        "src/MobilDwg.Core/MobilDwg.Core.csproj",
        "src/MobilDwg.Rendering/MobilDwg.Rendering.csproj"
    ]);
AssertProjectReferences("tests/MobilDwg.Architecture.Tests/MobilDwg.Architecture.Tests.csproj", []);
AssertProjectReferences("tests/MobilDwg.Integration.Tests/MobilDwg.Integration.Tests.csproj",
    [
        "src/MobilDwg.Core/MobilDwg.Core.csproj",
        "src/MobilDwg.Cad/MobilDwg.Cad.csproj",
        "src/MobilDwg.Rendering/MobilDwg.Rendering.csproj"
    ]);

AssertPackageReferences("src/MobilDwg.Core/MobilDwg.Core.csproj", []);
AssertPackageReferences("src/MobilDwg.Cad/MobilDwg.Cad.csproj", ["ACadSharp"]);
AssertPackageReferences("src/MobilDwg.Rendering/MobilDwg.Rendering.csproj", ["SkiaSharp"]);
AssertPackageReferences("src/MobilDwg.App/MobilDwg.App.csproj", ["Microsoft.Maui.Controls"]);

AssertProjectProperty("src/MobilDwg.App/MobilDwg.App.csproj", "TargetFramework", "net10.0-android36.0");
AssertProjectProperty("src/MobilDwg.App/MobilDwg.App.csproj", "OutputType", "Exe");
AssertProjectProperty("src/MobilDwg.App/MobilDwg.App.csproj", "UseMaui", "true", ignoreCase: true);
AssertProjectProperty("src/MobilDwg.App/MobilDwg.App.csproj", "SingleProject", "true", ignoreCase: true);
AssertProjectProperty("src/MobilDwg.App/MobilDwg.App.csproj", "ApplicationId", "com.smitelagwar.mobildwg");
AssertProjectProperty("src/MobilDwg.App/MobilDwg.App.csproj", "SupportedOSPlatformVersion", "24.0");

AssertForbiddenSourceTerms(
    "src/MobilDwg.Core",
    ["Microsoft.Maui", "SkiaSharp", "ACadSharp"]);
AssertForbiddenSourceTerms(
    "src/MobilDwg.Rendering",
    ["ACadSharp"]);
AssertForbiddenSourceTerms(
    "src/MobilDwg.App",
    ["SkiaSharp", "ACadSharp"]);

Console.WriteLine("STAGE04_ARCHITECTURE_TESTS_PASS");
Console.WriteLine("STAGE05_DEPENDENCY_BOUNDARY_PASS");
Console.WriteLine("V04_REAL_ANDROID_APP_PROJECT_PASS");

void AssertProjectReferences(string projectPath, IReadOnlyCollection<string> expected)
{
    var fullPath = Path.Combine(repoRoot, projectPath);
    var document = XDocument.Load(fullPath);
    var actual = document
        .Descendants("ProjectReference")
        .Select(element => element.Attribute("Include")?.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Select(value => NormalizeRelativeReference(projectPath, value!))
        .Order(StringComparer.Ordinal)
        .ToArray();

    var expectedOrdered = expected.Order(StringComparer.Ordinal).ToArray();

    Assert(actual.SequenceEqual(expectedOrdered, StringComparer.Ordinal),
        $"{projectPath} references [{string.Join(", ", actual)}], expected [{string.Join(", ", expectedOrdered)}]");
}

void AssertPackageReferences(string projectPath, IReadOnlyCollection<string> expected)
{
    var document = XDocument.Load(Path.Combine(repoRoot, projectPath));
    var actual = document
        .Descendants("PackageReference")
        .Select(element => element.Attribute("Include")?.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Order(StringComparer.Ordinal)
        .ToArray();
    var expectedOrdered = expected.Order(StringComparer.Ordinal).ToArray();

    Assert(actual.SequenceEqual(expectedOrdered, StringComparer.Ordinal),
        $"{projectPath} package references [{string.Join(", ", actual)}], expected [{string.Join(", ", expectedOrdered)}]");
}

void AssertProjectProperty(string projectPath, string propertyName, string expected, bool ignoreCase = false)
{
    var document = XDocument.Load(Path.Combine(repoRoot, projectPath));
    var values = document
        .Descendants(propertyName)
        .Select(element => (element.Value ?? string.Empty).Trim())
        .Where(value => value.Length > 0)
        .ToArray();
    Assert(values.Length == 1,
        $"{projectPath} must contain exactly one {propertyName}; got [{string.Join(", ", values)}]");

    var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    Assert(string.Equals(values[0], expected, comparison),
        $"{projectPath} {propertyName} is '{values[0]}', expected '{expected}'");
}

void AssertForbiddenSourceTerms(string relativeDirectory, IReadOnlyCollection<string> forbiddenTerms)
{
    var directory = Path.Combine(repoRoot, relativeDirectory);
    foreach (var file in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
    {
        var rel = Path.GetRelativePath(directory, file);
        if (rel.StartsWith("bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            rel.StartsWith("obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var text = File.ReadAllText(file);
        foreach (var term in forbiddenTerms)
        {
            Assert(!text.Contains(term, StringComparison.Ordinal),
                $"{relativeDirectory} source must not depend on {term}: {Path.GetRelativePath(repoRoot, file)}");
        }
    }
}

string NormalizeRelativeReference(string projectPath, string include)
{
    var projectDirectory = Path.GetDirectoryName(Path.Combine(repoRoot, projectPath))!;
    var target = Path.GetFullPath(Path.Combine(projectDirectory, include));
    return Path.GetRelativePath(repoRoot, target).Replace('\\', '/');
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
