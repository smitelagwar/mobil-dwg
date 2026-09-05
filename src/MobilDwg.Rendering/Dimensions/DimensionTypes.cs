using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Dimensions;

public enum CadDimensionType
{
    Aligned = 0,
    Linear = 1,
    Radial = 2,
    Diametric = 3,
    Angular = 4,
    Ordinate = 5,
}

public enum CadArrowheadStyle
{
    ClosedFilled = 0,      // Standard AutoCAD filled triangle
    ArchitecturalTick = 1, // 45-degree slash mark
    Dot = 2,
    Open = 3,
}

public sealed record CadDimensionDefinition
{
    public CadDimensionDefinition(
        CadDimensionType dimensionType,
        WorldPoint2 defPoint1,
        WorldPoint2 defPoint2,
        WorldPoint2 dimensionLinePoint = default,
        WorldPoint2? textPosition = null,
        WorldPoint2? centerPoint = null,
        double rotationRadians = 0d,
        string? textOverride = null,
        string? anonymousBlockName = null,
        double arrowheadSize = 2.5d,
        double textHeight = 3.0d,
        CadArrowheadStyle arrowStyle = CadArrowheadStyle.ClosedFilled)
    {
        DimensionType = dimensionType;
        DefPoint1 = defPoint1;
        DefPoint2 = defPoint2;
        DimensionLinePoint = dimensionLinePoint;
        TextPosition = textPosition;
        CenterPoint = centerPoint;
        RotationRadians = rotationRadians;
        TextOverride = textOverride;
        AnonymousBlockName = anonymousBlockName;
        ArrowheadSize = arrowheadSize > 0 ? arrowheadSize : 2.5d;
        TextHeight = textHeight > 0 ? textHeight : 3.0d;
        ArrowStyle = arrowStyle;
    }

    public CadDimensionType DimensionType { get; }
    public WorldPoint2 DefPoint1 { get; }
    public WorldPoint2 DefPoint2 { get; }
    public WorldPoint2 DimensionLinePoint { get; }
    public WorldPoint2? TextPosition { get; }
    public WorldPoint2? CenterPoint { get; }
    public double RotationRadians { get; }
    public string? TextOverride { get; }
    public string? AnonymousBlockName { get; }
    public double ArrowheadSize { get; }
    public double TextHeight { get; }
    public CadArrowheadStyle ArrowStyle { get; }
}
