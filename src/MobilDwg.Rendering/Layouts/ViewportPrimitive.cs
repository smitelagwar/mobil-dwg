using System.Collections.ObjectModel;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Layouts;

public sealed record ViewportPrimitive : RenderGeometryPrimitive
{
    private readonly ReadOnlyCollection<RenderGeometryPrimitive> _innerPrimitives;
    private readonly ReadOnlyCollection<WorldPoint2>? _clipBoundary;

    public ViewportPrimitive(
        string viewportId,
        WorldBounds2 paperBounds,
        IEnumerable<RenderGeometryPrimitive> innerPrimitives,
        IEnumerable<WorldPoint2>? clipBoundary = null)
    {
        if (string.IsNullOrWhiteSpace(viewportId)) throw new ArgumentException("Viewport ID is required.", nameof(viewportId));
        ArgumentNullException.ThrowIfNull(innerPrimitives);

        ViewportId = viewportId;
        PaperBounds = paperBounds;
        _innerPrimitives = Array.AsReadOnly(innerPrimitives.ToArray());
        _clipBoundary = clipBoundary != null ? Array.AsReadOnly(clipBoundary.ToArray()) : null;
    }

    public string ViewportId { get; }
    public WorldBounds2 PaperBounds { get; }
    public override WorldBounds2 Bounds => PaperBounds;
    public IReadOnlyList<RenderGeometryPrimitive> InnerPrimitives => _innerPrimitives;
    public IReadOnlyList<WorldPoint2>? ClipBoundary => _clipBoundary;
}
