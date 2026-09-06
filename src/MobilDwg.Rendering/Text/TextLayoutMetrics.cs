using System;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Text;

public static class TextLayoutMetrics
{
    public static WorldBounds2 CalculateTextBounds(
        string text,
        WorldPoint2 position,
        double height,
        double rotationRadians = 0d,
        double widthFactor = 1d,
        double obliqueAngleRadians = 0d,
        CadTextHorizontalAlignment horizontalAlignment = CadTextHorizontalAlignment.Left,
        CadTextVerticalAlignment verticalAlignment = CadTextVerticalAlignment.Baseline,
        CadTextMirrorFlags mirrorFlags = CadTextMirrorFlags.None)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new WorldBounds2(position.X, position.Y, position.X, position.Y);
        }

        var charCount = Math.Max(1, text.Length);
        // Conservative character width: 0.75 * height * widthFactor safely encloses standard fonts and wide glyphs (W, M)
        var estimatedWidth = charCount * height * 0.75d * widthFactor;
        var estimatedHeight = height;
        var descent = height * 0.25d; // standard font descent below baseline

        // Alignment offsets
        double offsetX = horizontalAlignment switch
        {
            CadTextHorizontalAlignment.Center or CadTextHorizontalAlignment.Middle => -estimatedWidth / 2d,
            CadTextHorizontalAlignment.Right => -estimatedWidth,
            _ => 0d,
        };

        double offsetY = verticalAlignment switch
        {
            CadTextVerticalAlignment.Top => -estimatedHeight,
            CadTextVerticalAlignment.Middle => -estimatedHeight / 2d,
            CadTextVerticalAlignment.Bottom => descent,
            _ => 0d, // Baseline
        };

        var isBackward = mirrorFlags.HasFlag(CadTextMirrorFlags.Backward);
        var isUpsideDown = mirrorFlags.HasFlag(CadTextMirrorFlags.UpsideDown);

        // Oblique angle shear: x_shear = y * tan(obliqueAngle)
        var tanOblique = Math.Tan(obliqueAngleRadians);

        double yMin = offsetY - descent;
        double yMax = offsetY + estimatedHeight;
        double xMin = offsetX;
        double xMax = offsetX + estimatedWidth;

        // 4 corners sheared by oblique angle
        var localCorners = new (double X, double Y)[4]
        {
            (xMin + (yMin * tanOblique), yMin),
            (xMax + (yMin * tanOblique), yMin),
            (xMax + (yMax * tanOblique), yMax),
            (xMin + (yMax * tanOblique), yMax),
        };

        var cos = Math.Cos(rotationRadians);
        var sin = Math.Sin(rotationRadians);
        var worldPoints = new WorldPoint2[4];

        for (var i = 0; i < 4; i++)
        {
            var lx = localCorners[i].X;
            var ly = localCorners[i].Y;

            if (isBackward) lx = -lx;
            if (isUpsideDown) ly = -ly;

            var rx = (lx * cos) - (ly * sin);
            var ry = (lx * sin) + (ly * cos);

            worldPoints[i] = new WorldPoint2(position.X + rx, position.Y + ry);
        }

        return GeometryBounds.FromPoints(worldPoints);
    }
}
