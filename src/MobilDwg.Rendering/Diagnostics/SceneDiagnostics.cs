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

public sealed record SceneDiagnostic
{
    public SceneDiagnostic(
        SceneDiagnosticKind kind,
        string code,
        string message,
        RenderEntityId? entityId = null)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Diagnostic code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Diagnostic message is required.", nameof(message));
        if (entityId is { } id && string.IsNullOrWhiteSpace(id.Value)) throw new ArgumentException("Diagnostic entity ID cannot be an invalid default value.", nameof(entityId));

        Kind = kind;
        Code = code;
        Message = message;
        EntityId = entityId;
    }

    public SceneDiagnosticKind Kind { get; }
    public string Code { get; }
    public string Message { get; }
    public RenderEntityId? EntityId { get; }
}

public sealed class SceneDiagnostics
{
    private readonly ReadOnlyCollection<SceneDiagnostic> _items;

    public SceneDiagnostics(IEnumerable<SceneDiagnostic>? items = null)
    {
        var array = items?.ToArray() ?? Array.Empty<SceneDiagnostic>();
        foreach (var item in array)
        {
            ArgumentNullException.ThrowIfNull(item);
        }

        _items = Array.AsReadOnly(array);
    }

    public IReadOnlyList<SceneDiagnostic> Items => _items;
    public bool HasErrors => _items.Any(x => x.Kind == SceneDiagnosticKind.Error);
    public int Count(SceneDiagnosticKind kind) => _items.Count(x => x.Kind == kind);
}
