using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Text;

namespace MobilDwg.Rendering.Geometry;

public sealed record TextPrimitive : RenderGeometryPrimitive
{
    public TextPrimitive(
        string text,
        WorldPoint2 position,
        double height,
        double rotationRadians = 0d,
        double widthFactor = 1d,
        double obliqueAngleRadians = 0d,
        CadTextHorizontalAlignment horizontalAlignment = CadTextHorizontalAlignment.Left,
        CadTextVerticalAlignment verticalAlignment = CadTextVerticalAlignment.Baseline,
        CadTextMirrorFlags mirrorFlags = CadTextMirrorFlags.None,
        string requestedFont = "STANDARD",
        string? resolvedFont = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!double.IsFinite(height) || height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Text height must be finite and positive.");
        if (!double.IsFinite(rotationRadians)) throw new ArgumentOutOfRangeException(nameof(rotationRadians));
        if (!double.IsFinite(widthFactor) || widthFactor <= 0) throw new ArgumentOutOfRangeException(nameof(widthFactor), "Width factor must be positive.");
        if (!double.IsFinite(obliqueAngleRadians)) throw new ArgumentOutOfRangeException(nameof(obliqueAngleRadians));

        Text = text;
        Position = position;
        Height = height;
        RotationRadians = rotationRadians;
        WidthFactor = widthFactor;
        ObliqueAngleRadians = obliqueAngleRadians;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
        MirrorFlags = mirrorFlags;
        RequestedFont = requestedFont;
        ResolvedFont = resolvedFont ?? FontSubstitutionResolver.Resolve(requestedFont);
        Bounds = CalculateBounds();
    }

    public string Text { get; }
    public WorldPoint2 Position { get; }
    public double Height { get; }
    public double RotationRadians { get; }
    public double WidthFactor { get; }
    public double ObliqueAngleRadians { get; }
    public CadTextHorizontalAlignment HorizontalAlignment { get; }
    public CadTextVerticalAlignment VerticalAlignment { get; }
    public CadTextMirrorFlags MirrorFlags { get; }
    public string RequestedFont { get; }
    public string ResolvedFont { get; }
    public override WorldBounds2 Bounds { get; }

    private WorldBounds2 CalculateBounds()
    {
        return TextLayoutMetrics.CalculateTextBounds(
            Text,
            Position,
            Height,
            RotationRadians,
            WidthFactor,
            ObliqueAngleRadians,
            HorizontalAlignment,
            VerticalAlignment,
            MirrorFlags);
    }
}
