using System.Collections.ObjectModel;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Diagnostics;

public enum SceneDiagnosticKind
{
    Unsupported = 0,
    Substituted = 1,
    Dropped = 2,
    Error = 3,
}

public sealed record SceneDiagnostic(
    SceneDiagnosticKind Kind,
    string Code,
    string Message,
    RenderEntityId? EntityId = null)
{
    public string Code { get; init; } = string.IsNullOrWhiteSpace(Code)
        ? throw new ArgumentException("Diagnostic code is required.", nameof(Code))
        : Code;

    public string Message { get; init; } = string.IsNullOrWhiteSpace(Message)
        ? throw new ArgumentException("Diagnostic message is required.", nameof(Message))
        : Message;
}

public sealed class SceneDiagnostics
{
    private readonly ReadOnlyCollection<SceneDiagnostic> _items;

    public SceneDiagnostics(IEnumerable<SceneDiagnostic>? items = null)
    {
        _items = Array.AsReadOnly(items?.ToArray() ?? Array.Empty<SceneDiagnostic>());
    }

    public IReadOnlyList<SceneDiagnostic> Items => _items;
    public bool HasErrors => _items.Any(x => x.Kind == SceneDiagnosticKind.Error);
    public int Count(SceneDiagnosticKind kind) => _items.Count(x => x.Kind == kind);
}
