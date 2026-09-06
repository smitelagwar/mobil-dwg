using System.Collections.ObjectModel;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Blocks;

public sealed record BlockReference
{
    public BlockReference(
        string blockName,
        WorldPoint2 insertionPoint,
        double scaleX = 1d,
        double scaleY = 1d,
        double rotationRadians = 0d,
        RenderLayerToken? layer = null,
        RenderStyleToken? style = null,
        IEnumerable<BlockAttribute>? attributes = null,
        string? handle = null,
        int? sourceIndex = null,
        int columnCount = 1,
        int rowCount = 1,
        double columnSpacing = 0d,
        double rowSpacing = 0d)
    {
        if (string.IsNullOrWhiteSpace(blockName)) throw new ArgumentException("Block name is required.", nameof(blockName));

        BlockName = blockName;
        InsertionPoint = insertionPoint;
        ScaleX = scaleX == 0d ? 1d : scaleX;
        ScaleY = scaleY == 0d ? 1d : scaleY;
        RotationRadians = rotationRadians;
        Layer = layer ?? new RenderLayerToken("0");
        Style = style ?? new RenderStyleToken("BYLAYER");
        Attributes = Array.AsReadOnly(attributes?.ToArray() ?? Array.Empty<BlockAttribute>());
        Handle = handle;
        SourceIndex = sourceIndex;
        ColumnCount = Math.Max(1, columnCount);
        RowCount = Math.Max(1, rowCount);
        ColumnSpacing = columnSpacing;
        RowSpacing = rowSpacing;
    }

    public string BlockName { get; }
    public WorldPoint2 InsertionPoint { get; }
    public double ScaleX { get; }
    public double ScaleY { get; }
    public double RotationRadians { get; }
    public RenderLayerToken Layer { get; }
    public RenderStyleToken Style { get; }
    public IReadOnlyList<BlockAttribute> Attributes { get; }
    public string? Handle { get; }
    public int? SourceIndex { get; }
    public int ColumnCount { get; }
    public int RowCount { get; }
    public double ColumnSpacing { get; }
    public double RowSpacing { get; }
}
