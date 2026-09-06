using System.Collections.ObjectModel;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Geometry;

public enum HatchIslandStyle
{
    Normal = 0, // Alternating even-odd nesting (outer filled, island hollow, sub-island filled)
    Outer = 1,  // Only outermost area filled, all islands completely hollow
    Ignore = 2, // All islands ignored, entire outer boundary filled
}

public sealed record HatchLoop
{
    private readonly ReadOnlyCollection<WorldPoint2> _vertices;

    public HatchLoop(IEnumerable<WorldPoint2> vertices, bool isOuter = true)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        var copy = vertices.ToArray();
        if (copy.Length < 3) throw new ArgumentException("A hatch loop requires at least three vertices.", nameof(vertices));
        _vertices = Array.AsReadOnly(copy);
        IsOuter = isOuter;
        Bounds = GeometryBounds.FromPoints(copy);
    }

    public IReadOnlyList<WorldPoint2> Vertices => _vertices;
    public bool IsOuter { get; }
    public WorldBounds2 Bounds { get; }
}

public sealed record HatchPrimitive : RenderGeometryPrimitive
{
    private readonly ReadOnlyCollection<HatchLoop> _loops;
    private readonly ReadOnlyCollection<LinePrimitive> _patternLines;

    public HatchPrimitive(
        IEnumerable<HatchLoop> loops,
        string patternName = "SOLID",
        double patternAngleRadians = 0d,
        double patternScale = 1d,
        HatchIslandStyle islandStyle = HatchIslandStyle.Normal,
        bool isSolid = true,
        IEnumerable<LinePrimitive>? patternLines = null,
        WorldPoint2 patternOrigin = default)
    {
        ArgumentNullException.ThrowIfNull(loops);
        var loopCopy = loops.ToArray();
        if (loopCopy.Length == 0) throw new ArgumentException("Hatch requires at least one boundary loop.", nameof(loops));

        _loops = Array.AsReadOnly(loopCopy);
        PatternName = patternName ?? "SOLID";
        PatternAngleRadians = patternAngleRadians;
        PatternScale = patternScale > 0 ? patternScale : 1d;
        IslandStyle = islandStyle;
        IsSolid = isSolid;
        PatternOrigin = patternOrigin;
        _patternLines = Array.AsReadOnly(patternLines?.ToArray() ?? Array.Empty<LinePrimitive>());

        var bounds = loopCopy[0].Bounds;
        for (var i = 1; i < loopCopy.Length; i++)
        {
            bounds = bounds.Union(loopCopy[i].Bounds);
        }
        Bounds = bounds;
    }

    public IReadOnlyList<HatchLoop> Loops => _loops;
    public string PatternName { get; }
    public double PatternAngleRadians { get; }
    public double PatternScale { get; }
    public HatchIslandStyle IslandStyle { get; }
    public bool IsSolid { get; }
    public WorldPoint2 PatternOrigin { get; }
    public IReadOnlyList<LinePrimitive> PatternLines => _patternLines;
    public override WorldBounds2 Bounds { get; }
}
