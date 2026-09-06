using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Text;

public sealed record TextLineLayout(
    string Text,
    double Width,
    double OffsetX,
    double OffsetY);

public sealed class TextLayout
{
    private const double LineSpacingRatio = 1.33d;
    private const double CharWidthRatio = 0.75d;
    private const double DescentRatio = 0.25d;

    public TextLayout(
        string text,
        WorldPoint2 position,
        double height,
        double rotationRadians = 0d,
        double widthFactor = 1d,
        double obliqueAngleRadians = 0d,
        CadTextHorizontalAlignment horizontalAlignment = CadTextHorizontalAlignment.Left,
        CadTextVerticalAlignment verticalAlignment = CadTextVerticalAlignment.Baseline,
        CadTextMirrorFlags mirrorFlags = CadTextMirrorFlags.None,
        string fontKey = "STANDARD",
        long revision = 1)
    {
        Text = text ?? string.Empty;
        Position = position;
        Height = height > 0 ? height : 1.0;
        RotationRadians = rotationRadians;
        WidthFactor = widthFactor > 0 ? widthFactor : 1.0;
        ObliqueAngleRadians = obliqueAngleRadians;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
        MirrorFlags = mirrorFlags;
        FontKey = fontKey ?? "STANDARD";
        Revision = revision;

        var rawLines = Text.Length == 0
            ? new[] { string.Empty }
            : Text.Replace("\r\n", "\n").Split('\n');

        var lineSpacing = Height * LineSpacingRatio;
        var descent = Height * DescentRatio;
        var lineCount = rawLines.Length;

        var measuredLines = new (string text, double width)[lineCount];
        double maxWidth = 0d;
        for (var i = 0; i < lineCount; i++)
        {
            var lText = rawLines[i];
            var charCount = Math.Max(1, lText.Length);
            var w = charCount * Height * CharWidthRatio * WidthFactor;
            measuredLines[i] = (lText, w);
            if (w > maxWidth) maxWidth = w;
        }

        TotalWidth = maxWidth;
        TotalHeight = Height + ((lineCount - 1) * lineSpacing);

        double offsetY = verticalAlignment switch
        {
            CadTextVerticalAlignment.Top => -Height,
            CadTextVerticalAlignment.Middle => -TotalHeight / 2d,
            CadTextVerticalAlignment.Bottom => descent,
            _ => 0d,
        };

        var lineLayouts = new List<TextLineLayout>(lineCount);
        for (var i = 0; i < lineCount; i++)
        {
            var (lText, lWidth) = measuredLines[i];
            double lineOffX = horizontalAlignment switch
            {
                CadTextHorizontalAlignment.Center or CadTextHorizontalAlignment.Middle => -lWidth / 2d,
                CadTextHorizontalAlignment.Right => -lWidth,
                _ => 0d,
            };

            double lineOffY = offsetY - (i * lineSpacing);
            lineLayouts.Add(new TextLineLayout(lText, lWidth, lineOffX, lineOffY));
        }

        Lines = lineLayouts.AsReadOnly();
        Bounds = CalculateWorldBounds(TotalWidth, TotalHeight, offsetY, descent);
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
    public string FontKey { get; }
    public long Revision { get; }
    public IReadOnlyList<TextLineLayout> Lines { get; }
    public double TotalWidth { get; }
    public double TotalHeight { get; }
    public WorldBounds2 Bounds { get; }

    private WorldBounds2 CalculateWorldBounds(
        double totalWidth,
        double totalHeight,
        double offsetY,
        double descent)
    {
        double xMin = HorizontalAlignment switch
        {
            CadTextHorizontalAlignment.Center or CadTextHorizontalAlignment.Middle => -totalWidth / 2d,
            CadTextHorizontalAlignment.Right => -totalWidth,
            _ => 0d,
        };
        double xMax = xMin + totalWidth;

        double yMax = offsetY + Height;
        double yMin = offsetY - (totalHeight - Height) - descent;

        var tanOblique = Math.Tan(ObliqueAngleRadians);
        var isBackward = MirrorFlags.HasFlag(CadTextMirrorFlags.Backward);
        var isUpsideDown = MirrorFlags.HasFlag(CadTextMirrorFlags.UpsideDown);

        var localCorners = new (double X, double Y)[4]
        {
            (xMin + (yMin * tanOblique), yMin),
            (xMax + (yMin * tanOblique), yMin),
            (xMax + (yMax * tanOblique), yMax),
            (xMin + (yMax * tanOblique), yMax),
        };

        var cos = Math.Cos(RotationRadians);
        var sin = Math.Sin(RotationRadians);
        var worldPoints = new WorldPoint2[4];

        for (var i = 0; i < 4; i++)
        {
            var lx = localCorners[i].X;
            var ly = localCorners[i].Y;

            if (isBackward) lx = -lx;
            if (isUpsideDown) ly = -ly;

            var rx = (lx * cos) - (ly * sin);
            var ry = (lx * sin) + (ly * cos);

            worldPoints[i] = new WorldPoint2(Position.X + rx, Position.Y + ry);
        }

        return GeometryBounds.FromPoints(worldPoints);
    }
}
