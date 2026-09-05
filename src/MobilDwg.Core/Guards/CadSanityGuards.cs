namespace MobilDwg.Core.Guards;

public static class CadSanityGuards
{
    public const double DefaultCoordinateThreshold = 1e12;

    public static bool IsValidCoordinate(double value, double maxAbsolute = DefaultCoordinateThreshold)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && Math.Abs(value) <= maxAbsolute;
    }

    public static bool IsValidPoint(double x, double y, double maxAbsolute = DefaultCoordinateThreshold)
    {
        return IsValidCoordinate(x, maxAbsolute) && IsValidCoordinate(y, maxAbsolute);
    }

    public static bool SanitizeCoordinate(ref double value, double fallback = 0.0, double maxAbsolute = DefaultCoordinateThreshold)
    {
        if (!IsValidCoordinate(value, maxAbsolute))
        {
            value = fallback;
            return false;
        }
        return true;
    }

    public static bool SanitizePoint(ref double x, ref double y, double fallbackX = 0.0, double fallbackY = 0.0, double maxAbsolute = DefaultCoordinateThreshold)
    {
        bool xValid = SanitizeCoordinate(ref x, fallbackX, maxAbsolute);
        bool yValid = SanitizeCoordinate(ref y, fallbackY, maxAbsolute);
        return xValid && yValid;
    }

    public static bool SanitizeBounds(
        ref double minX, ref double minY, ref double maxX, ref double maxY,
        double fallbackMinX = 0.0, double fallbackMinY = 0.0,
        double fallbackMaxX = 1.0, double fallbackMaxY = 1.0,
        double maxAbsolute = DefaultCoordinateThreshold)
    {
        bool valid = true;
        valid &= SanitizeCoordinate(ref minX, fallbackMinX, maxAbsolute);
        valid &= SanitizeCoordinate(ref minY, fallbackMinY, maxAbsolute);
        valid &= SanitizeCoordinate(ref maxX, fallbackMaxX, maxAbsolute);
        valid &= SanitizeCoordinate(ref maxY, fallbackMaxY, maxAbsolute);

        if (minX > maxX)
        {
            (minX, maxX) = (maxX, minX);
            valid = false;
        }

        if (minY > maxY)
        {
            (minY, maxY) = (maxY, minY);
            valid = false;
        }

        return valid;
    }
}
