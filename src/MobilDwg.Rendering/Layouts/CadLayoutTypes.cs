using System.Collections.ObjectModel;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Layouts;

public sealed record CadLayoutViewport
{
    private readonly ReadOnlyCollection<WorldPoint2>? _clipBoundary;
    private readonly ReadOnlySet<string> _frozenLayers;

    public CadLayoutViewport(
        string viewportId,
        WorldPoint2 paperCenter,
        double paperWidth,
        double paperHeight,
        WorldPoint2 viewCenter,
        double viewHeight,
        double twistAngleRadians = 0d,
        IEnumerable<string>? frozenLayers = null,
        IEnumerable<WorldPoint2>? clipBoundary = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(viewportId)) throw new ArgumentException("Viewport ID is required.", nameof(viewportId));

        ViewportId = viewportId;
        PaperCenter = paperCenter;
        PaperWidth = paperWidth;
        PaperHeight = paperHeight;
        ViewCenter = viewCenter;
        ViewHeight = viewHeight;
        TwistAngleRadians = twistAngleRadians;
        _frozenLayers = new ReadOnlySet<string>(new HashSet<string>(frozenLayers ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase));
        _clipBoundary = clipBoundary != null ? Array.AsReadOnly(clipBoundary.ToArray()) : null;
        IsActive = isActive;

        PaperBounds = new WorldBounds2(
            paperCenter.X - (paperWidth / 2d),
            paperCenter.Y - (paperHeight / 2d),
            paperCenter.X + (paperWidth / 2d),
            paperCenter.Y + (paperHeight / 2d));
    }

    public string ViewportId { get; }
    public WorldPoint2 PaperCenter { get; }
    public double PaperWidth { get; }
    public double PaperHeight { get; }
    public WorldPoint2 ViewCenter { get; }
    public double ViewHeight { get; }
    public double TwistAngleRadians { get; }
    public IReadOnlySet<string> FrozenLayers => _frozenLayers;
    public IReadOnlyList<WorldPoint2>? ClipBoundary => _clipBoundary;
    public bool IsActive { get; }
    public WorldBounds2 PaperBounds { get; }
}

public sealed record CadLayoutDefinition
{
    private readonly ReadOnlyCollection<RenderSceneEntity> _paperEntities;
    private readonly ReadOnlyCollection<CadLayoutViewport> _viewports;

    public CadLayoutDefinition(
        string name,
        bool isModelSpace,
        int tabOrder,
        WorldBounds2 paperBounds,
        IEnumerable<RenderSceneEntity>? paperEntities = null,
        IEnumerable<CadLayoutViewport>? viewports = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Layout name is required.", nameof(name));

        Name = name;
        IsModelSpace = isModelSpace;
        TabOrder = tabOrder;
        PaperBounds = paperBounds;
        _paperEntities = Array.AsReadOnly(paperEntities?.ToArray() ?? Array.Empty<RenderSceneEntity>());
        _viewports = Array.AsReadOnly(viewports?.ToArray() ?? Array.Empty<CadLayoutViewport>());
    }

    public string Name { get; }
    public bool IsModelSpace { get; }
    public int TabOrder { get; }
    public WorldBounds2 PaperBounds { get; }
    public IReadOnlyList<RenderSceneEntity> PaperEntities => _paperEntities;
    public IReadOnlyList<CadLayoutViewport> Viewports => _viewports;
}
