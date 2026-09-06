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
        CadTextMirrorFlags mirrorFlags = CadTextMirrorFlags.None,
        string fontKey = "STANDARD")
    {
        if (string.IsNullOrEmpty(text))
        {
            return new WorldBounds2(position.X, position.Y, position.X, position.Y);
        }

        var layout = new TextLayout(
            text,
            position,
            height,
            rotationRadians,
            widthFactor,
            obliqueAngleRadians,
            horizontalAlignment,
            verticalAlignment,
            mirrorFlags,
            fontKey);

        return layout.Bounds;
    }
}
