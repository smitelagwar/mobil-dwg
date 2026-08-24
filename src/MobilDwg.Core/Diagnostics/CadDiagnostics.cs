namespace MobilDwg.Core.Diagnostics;

public enum DiagnosticSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
}

public sealed record CadDiagnostic(
    string Code,
    DiagnosticSeverity Severity,
    string Message,
    string? EntityType = null);

public enum CompatibilityIssueKind
{
    UnsupportedEntity,
    ProxyObject,
    MissingFont,
    MissingExternalReference,
    MissingRaster,
    MissingUnderlay,
}

public sealed record CadCompatibilityIssue(
    CompatibilityIssueKind Kind,
    string Code,
    string Message,
    string? Resource = null);
