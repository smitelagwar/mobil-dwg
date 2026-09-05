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
        var charCount = Math.Max(1, Text.Length);
        var estimatedWidth = charCount * Height * 0.6d * WidthFactor;
        var estimatedHeight = Height;

        // Alignment offsets
        double offsetX = HorizontalAlignment switch
        {
            CadTextHorizontalAlignment.Center or CadTextHorizontalAlignment.Middle => -estimatedWidth / 2d,
            CadTextHorizontalAlignment.Right => -estimatedWidth,
            _ => 0d,
        };

        double offsetY = VerticalAlignment switch
        {
            CadTextVerticalAlignment.Top => -estimatedHeight,
            CadTextVerticalAlignment.Middle => -estimatedHeight / 2d,
            CadTextVerticalAlignment.Bottom => 0d,
            _ => 0d, // Baseline
        };

        // Mirror adjustments
        var isBackward = MirrorFlags.HasFlag(CadTextMirrorFlags.Backward);
        var isUpsideDown = MirrorFlags.HasFlag(CadTextMirrorFlags.UpsideDown);

        var corners = new (double X, double Y)[4]
        {
            (offsetX, offsetY),
            (offsetX + estimatedWidth, offsetY),
            (offsetX + estimatedWidth, offsetY + estimatedHeight),
            (offsetX, offsetY + estimatedHeight),
        };

        var cos = Math.Cos(RotationRadians);
        var sin = Math.Sin(RotationRadians);
        var worldPoints = new WorldPoint2[4];

        for (var i = 0; i < 4; i++)
        {
            var lx = corners[i].X;
            var ly = corners[i].Y;

            if (isBackward) lx = -lx;
            if (isUpsideDown) ly = -ly;

            var rx = (lx * cos) - (ly * sin);
            var ry = (lx * sin) + (ly * cos);

            worldPoints[i] = new WorldPoint2(Position.X + rx, Position.Y + ry);
        }

        return GeometryBounds.FromPoints(worldPoints);
    }
}
