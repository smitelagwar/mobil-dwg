namespace MobilDwg.Rendering.Styles;

public sealed record CadEntityStyle(
    CadColor Color,
    CadLinetype Linetype,
    CadLineweight Lineweight,
    double LinetypeScale = 1.0)
{
    public static CadEntityStyle Default { get; } = new(CadColor.ByLayer, CadLinetype.ByLayer, CadLineweight.ByLayer);
}
