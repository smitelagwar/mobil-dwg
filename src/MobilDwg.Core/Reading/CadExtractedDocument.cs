using System.Collections.ObjectModel;

namespace MobilDwg.Core.Reading;

public enum CadExtractedEntityType
{
    Line,
    Circle,
    Arc,
    Ellipse,
    Polyline,
    Spline,
    Text,
    MText,
    Dimension,
    Hatch,
    Insert,
    Solid,
    Point,
    Raster,
    Other,
    Unsupported
}

public enum CadColorMethod
{
    ByLayer = 0,
    ByBlock = 1,
    Index = 2,
    TrueColor = 3
}

public readonly record struct CadEntityColor(
    CadColorMethod Method,
    short AciIndex = 0,
    uint Argb = 0)
{
    public static CadEntityColor ByLayer { get; } = new(CadColorMethod.ByLayer, 256, 0);
    public static CadEntityColor ByBlock { get; } = new(CadColorMethod.ByBlock, 0, 0);

    public static CadEntityColor FromAci(short aciIndex) =>
        new(CadColorMethod.Index, aciIndex, 0);

    public static CadEntityColor FromTrueColor(uint argb) =>
        new(CadColorMethod.TrueColor, 0, argb | 0xFF000000u);
}

public readonly record struct CadEntityLineweight(
    short ValueHundredthsMm = -1,
    bool ByLayer = true,
    bool ByBlock = false);

public readonly record struct CadEntityTransparency(
    byte Alpha = 255,
    bool ByLayer = true,
    bool ByBlock = false);

public readonly record struct CadPoint3D(double X, double Y, double Z = 0.0);

public readonly record struct CadVector3D(double X, double Y, double Z = 1.0);

public readonly record struct CadExtractedPoint(double X, double Y);

public readonly record struct CadExtractedVertex(
    double X,
    double Y,
    double Bulge = 0.0,
    double StartWidth = 0.0,
    double EndWidth = 0.0);

public readonly record struct CadExtractedBounds(
    double MinX,
    double MinY,
    double MaxX,
    double MaxY);

public sealed record CadExtractedLayer(
    string Name,
    uint ArgbColor,
    short AciIndex = 7,
    bool IsVisible = true,
    bool IsLocked = false,
    string LineType = "Continuous",
    short Lineweight = -1);

public sealed record CadExtractedLinetype(
    string Name,
    string Description,
    IReadOnlyList<double> PatternSegments);

public sealed record CadExtractedTextStyle(
    string Name,
    string FontFile,
    double StandardHeight = 0,
    double WidthFactor = 1.0,
    double ObliqueAngle = 0.0);

public sealed record CadExtractedDimensionStyle(
    string Name,
    double TextHeight = 2.5,
    double ArrowSize = 2.5,
    double ScaleFactor = 1.0);

public sealed record CadExtractedDiagnostic(
    string Code,
    string Severity,
    string Message,
    string? EntityHandle = null);

// Type-Safe Payloads
public sealed record CadLinePayload(
    CadPoint3D Start,
    CadPoint3D End,
    double Thickness = 0.0,
    CadVector3D Normal = default);

public sealed record CadCirclePayload(
    CadPoint3D Center,
    double Radius,
    double Thickness = 0.0,
    CadVector3D Normal = default);

public sealed record CadArcPayload(
    CadPoint3D Center,
    double Radius,
    double StartAngle,
    double EndAngle,
    double Thickness = 0.0,
    CadVector3D Normal = default);

public sealed record CadEllipsePayload(
    CadPoint3D Center,
    CadPoint3D MajorAxis,
    double RadiusRatio,
    double StartParameter,
    double EndParameter,
    CadVector3D Normal = default);

public sealed record CadPolylinePayload(
    IReadOnlyList<CadExtractedVertex> Vertices,
    bool IsClosed,
    double Elevation = 0.0,
    double Thickness = 0.0,
    CadVector3D Normal = default);

public sealed record CadSplinePayload(
    int Degree,
    bool IsClosed,
    IReadOnlyList<CadPoint3D> ControlPoints,
    IReadOnlyList<CadPoint3D> FitPoints,
    IReadOnlyList<double> Knots);

public sealed record CadTextPayload(
    string Text,
    CadPoint3D InsertionPoint,
    double Height,
    double Rotation,
    string? StyleName = null,
    int HorizontalAlignment = 0,
    int VerticalAlignment = 0,
    CadVector3D Normal = default);

public sealed record CadDimensionPayload(
    string? Text,
    CadPoint3D DefinitionPoint,
    CadPoint3D MiddlePoint,
    string? DimensionType = null,
    string? StyleName = null,
    IReadOnlyList<CadExtractedEntity>? ExplodedEntities = null);

public sealed record CadHatchPayload(
    string PatternName,
    bool IsSolid,
    double Angle,
    double Scale,
    IReadOnlyList<IReadOnlyList<CadExtractedVertex>> Loops,
    CadVector3D Normal = default);

public sealed record CadInsertPayload(
    string BlockName,
    CadPoint3D InsertionPoint,
    double ScaleX,
    double ScaleY,
    double ScaleZ,
    double Rotation,
    IReadOnlyList<CadExtractedEntity>? ExplodedEntities = null,
    CadVector3D Normal = default);

public sealed record CadSolidPayload(
    CadPoint3D P1,
    CadPoint3D P2,
    CadPoint3D P3,
    CadPoint3D P4,
    CadVector3D Normal = default);

public sealed record CadPointPayload(CadPoint3D Location);

public sealed record CadRasterPayload(
    string? ReferenceId,
    string? ResolvedPath,
    CadPoint3D InsertionPoint,
    double Width,
    double Height,
    double Rotation,
    CadVector3D Normal = default);

public sealed record CadUnsupportedPayload(
    string TypeName,
    string Reason,
    CadExtractedBounds? ApproxBounds = null);

public sealed record CadExtractedMetadata(
    string Format,
    string Version,
    string? DisplayName,
    string Units = "Unitless",
    double Measurement = 0.0);

public sealed record CadExtractedEntity
{
    public CadExtractedEntity(
        string handle,
        string layerName,
        CadExtractedEntityType entityType,
        CadEntityColor color,
        int sourceOrder = 0,
        int drawOrder = 0,
        bool isVisible = true,
        CadEntityLineweight lineweight = default,
        CadEntityTransparency transparency = default,
        string? linetype = null,
        double linetypeScale = 1.0,
        string? blockOwner = null,
        string? layoutOwner = null,
        object? payload = null,
        uint? argbColor = null,
        IReadOnlyList<CadExtractedPoint>? points = null,
        IReadOnlyList<CadExtractedVertex>? vertices = null,
        double radius = 0.0,
        double startAngle = 0.0,
        double endAngle = 0.0,
        string? text = null,
        double textHeight = 1.0,
        double rotation = 0.0)
    {
        Handle = handle ?? string.Empty;
        LayerName = layerName ?? "0";
        EntityType = entityType;
        Color = color;
        SourceOrder = sourceOrder;
        DrawOrder = drawOrder;
        IsVisible = isVisible;
        Lineweight = lineweight;
        Transparency = transparency;
        Linetype = linetype;
        LinetypeScale = linetypeScale;
        BlockOwner = blockOwner;
        LayoutOwner = layoutOwner;
        Payload = payload;

        // Legacy backwards-compatibility properties
        ArgbColor = argbColor ?? (color.Method == CadColorMethod.TrueColor ? color.Argb : null);
        Points = points;
        Vertices = vertices;
        Radius = radius;
        StartAngle = startAngle;
        EndAngle = endAngle;
        Text = text;
        TextHeight = textHeight;
        Rotation = rotation;
    }

    // Legacy constructor overload
    public CadExtractedEntity(
        string handle,
        string layerName,
        CadExtractedEntityType entityType,
        uint? argbColor,
        IReadOnlyList<CadExtractedPoint>? points = null,
        IReadOnlyList<CadExtractedVertex>? vertices = null,
        double radius = 0.0,
        double startAngle = 0.0,
        double endAngle = 0.0,
        string? text = null,
        double textHeight = 1.0,
        double rotation = 0.0)
        : this(
            handle,
            layerName,
            entityType,
            argbColor.HasValue ? CadEntityColor.FromTrueColor(argbColor.Value) : CadEntityColor.ByLayer,
            argbColor: argbColor,
            points: points,
            vertices: vertices,
            radius: radius,
            startAngle: startAngle,
            endAngle: endAngle,
            text: text,
            textHeight: textHeight,
            rotation: rotation)
    {
    }

    public string Handle { get; }
    public string LayerName { get; }
    public CadExtractedEntityType EntityType { get; }
    public CadEntityColor Color { get; }
    public int SourceOrder { get; }
    public int DrawOrder { get; }
    public bool IsVisible { get; }
    public CadEntityLineweight Lineweight { get; }
    public CadEntityTransparency Transparency { get; }
    public string? Linetype { get; }
    public double LinetypeScale { get; }
    public string? BlockOwner { get; }
    public string? LayoutOwner { get; }
    public object? Payload { get; }

    // Legacy fields for backward compatibility
    public uint? ArgbColor { get; }
    public IReadOnlyList<CadExtractedPoint>? Points { get; }
    public IReadOnlyList<CadExtractedVertex>? Vertices { get; }
    public double Radius { get; }
    public double StartAngle { get; }
    public double EndAngle { get; }
    public string? Text { get; }
    public double TextHeight { get; }
    public double Rotation { get; }
}

public sealed record CadExtractedDocument
{
    public CadExtractedDocument(
        string format,
        string version,
        IReadOnlyList<CadExtractedLayer> layers,
        IReadOnlyList<CadExtractedEntity> entities,
        double minX,
        double minY,
        double maxX,
        double maxY,
        CadExtractedMetadata? metadata = null,
        IReadOnlyList<CadExtractedLinetype>? linetypes = null,
        IReadOnlyList<CadExtractedTextStyle>? textStyles = null,
        IReadOnlyList<CadExtractedDimensionStyle>? dimensionStyles = null,
        IReadOnlyDictionary<string, IReadOnlyList<CadExtractedEntity>>? blockDefinitions = null,
        IReadOnlyList<CadExtractedDiagnostic>? diagnostics = null,
        IReadOnlyList<string>? layoutNames = null)
    {
        Format = format;
        Version = version;
        Layers = layers ?? Array.Empty<CadExtractedLayer>();
        Entities = entities ?? Array.Empty<CadExtractedEntity>();
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
        Metadata = metadata ?? new CadExtractedMetadata(format, version, null);
        Linetypes = linetypes ?? Array.Empty<CadExtractedLinetype>();
        TextStyles = textStyles ?? Array.Empty<CadExtractedTextStyle>();
        DimensionStyles = dimensionStyles ?? Array.Empty<CadExtractedDimensionStyle>();
        BlockDefinitions = blockDefinitions ?? ReadOnlyDictionary<string, IReadOnlyList<CadExtractedEntity>>.Empty;
        Diagnostics = diagnostics ?? Array.Empty<CadExtractedDiagnostic>();
        LayoutNames = layoutNames ?? Array.Empty<string>();
    }

    public string Format { get; }
    public string Version { get; }
    public IReadOnlyList<CadExtractedLayer> Layers { get; }
    public IReadOnlyList<CadExtractedEntity> Entities { get; }
    public double MinX { get; }
    public double MinY { get; }
    public double MaxX { get; }
    public double MaxY { get; }
    public CadExtractedMetadata Metadata { get; }
    public IReadOnlyList<CadExtractedLinetype> Linetypes { get; }
    public IReadOnlyList<CadExtractedTextStyle> TextStyles { get; }
    public IReadOnlyList<CadExtractedDimensionStyle> DimensionStyles { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<CadExtractedEntity>> BlockDefinitions { get; }
    public IReadOnlyList<CadExtractedDiagnostic> Diagnostics { get; }
    public IReadOnlyList<string> LayoutNames { get; }

    public int SupportedEntityCount =>
        Entities.Count(e => e.EntityType != CadExtractedEntityType.Other && e.EntityType != CadExtractedEntityType.Unsupported);

    public int UnsupportedEntityCount =>
        Entities.Count(e => e.EntityType == CadExtractedEntityType.Other || e.EntityType == CadExtractedEntityType.Unsupported);

    public bool IsFullyCompliant =>
        Diagnostics.All(d => !string.Equals(d.Severity, "Warning", StringComparison.OrdinalIgnoreCase) &&
                             !string.Equals(d.Severity, "Error", StringComparison.OrdinalIgnoreCase));

    public string GetSummaryString() =>
        $"{Entities.Count} entities ({SupportedEntityCount} supported, {UnsupportedEntityCount} unsupported), {Layers.Count} layers";
}
