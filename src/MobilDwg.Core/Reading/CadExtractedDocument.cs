namespace MobilDwg.Core.Reading;

public enum CadExtractedEntityType
{
    Line,
    Circle,
    Arc,
    Polyline,
    Text,
    Dimension,
    Hatch,
    Other
}

public readonly record struct CadExtractedPoint(double X, double Y);

public readonly record struct CadExtractedVertex(double X, double Y, double Bulge = 0.0);

public sealed record CadExtractedLayer(string Name, uint ArgbColor, bool IsVisible = true);

public sealed record CadExtractedEntity(
    string Handle,
    string LayerName,
    CadExtractedEntityType EntityType,
    uint? ArgbColor,
    IReadOnlyList<CadExtractedPoint>? Points = null,
    IReadOnlyList<CadExtractedVertex>? Vertices = null,
    double Radius = 0.0,
    double StartAngle = 0.0,
    double EndAngle = 0.0,
    string? Text = null,
    double TextHeight = 1.0,
    double Rotation = 0.0);

public sealed record CadExtractedDocument(
    string Format,
    string Version,
    IReadOnlyList<CadExtractedLayer> Layers,
    IReadOnlyList<CadExtractedEntity> Entities,
    double MinX,
    double MinY,
    double MaxX,
    double MaxY);
