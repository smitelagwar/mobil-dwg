using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Camera;

public static class ViewerZoomPolicy
{
    public const double DefaultPaddingFraction = 0.05;
    public const double CoordinateLimit = 1e12;
    public const double AbsoluteMinWupp = 1e-12;
    public const double AbsoluteMaxWupp = 1e12;
    public const double DoubleTapZoomFactor = 2.0;
    public const double ButtonZoomFactor = 1.35;

    public static double Ulp(double value)
    {
        var m = Math.Abs(value);
        if (!double.IsFinite(m)) return 0.0;
        return Math.BitIncrement(m) - m;
    }

    public static double CalculateFitWupp(
        WorldBounds2? sceneBounds,
        int pixelWidth,
        int pixelHeight,
        double paddingFraction = DefaultPaddingFraction)
    {
        if (pixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pixelWidth));
        if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight));
        if (!double.IsFinite(paddingFraction) || paddingFraction < 0 || paddingFraction >= 0.5)
            throw new ArgumentOutOfRangeException(nameof(paddingFraction));

        if (!sceneBounds.HasValue)
        {
            return 1.0;
        }

        var bounds = sceneBounds.Value;
        var usableWidth = pixelWidth * (1.0 - (2.0 * paddingFraction));
        var usableHeight = pixelHeight * (1.0 - (2.0 * paddingFraction));
        if (usableWidth <= 0) usableWidth = 1.0;
        if (usableHeight <= 0) usableHeight = 1.0;

        double fitWupp;
        if (bounds.Width <= 0 && bounds.Height <= 0)
        {
            // Single point: use 1 drawing unit virtual extent
            fitWupp = Math.Max(1.0 / usableWidth, 1.0 / usableHeight);
        }
        else if (bounds.Width > 0 && bounds.Height <= 0)
        {
            // Horizontal line: only positive width determines fit
            fitWupp = bounds.Width / usableWidth;
        }
        else if (bounds.Width <= 0 && bounds.Height > 0)
        {
            // Vertical line: only positive height determines fit
            fitWupp = bounds.Height / usableHeight;
        }
        else
        {
            var byWidth = bounds.Width / usableWidth;
            var byHeight = bounds.Height / usableHeight;
            fitWupp = Math.Max(byWidth, byHeight);
        }

        if (!double.IsFinite(fitWupp) || fitWupp <= 0)
        {
            fitWupp = 1.0;
        }

        return Math.Clamp(fitWupp, AbsoluteMinWupp, AbsoluteMaxWupp);
    }

    public static (double MinWupp, double MaxWupp) CalculateZoomLimits(
        WorldBounds2? sceneBounds,
        WorldPoint2 currentCenter,
        WorldPoint2? anchorPoint,
        int pixelWidth,
        int pixelHeight,
        double paddingFraction = DefaultPaddingFraction)
    {
        if (pixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pixelWidth));
        if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight));

        var m = Math.Max(1.0, Math.Max(Math.Abs(currentCenter.X), Math.Abs(currentCenter.Y)));
        if (anchorPoint.HasValue)
        {
            m = Math.Max(m, Math.Max(Math.Abs(anchorPoint.Value.X), Math.Abs(anchorPoint.Value.Y)));
        }

        if (!double.IsFinite(m) || m > CoordinateLimit)
        {
            m = Math.Min(CoordinateLimit, double.IsFinite(m) ? m : 1.0);
        }

        var ulpM = Ulp(m);
        var minWupp = Math.Max(AbsoluteMinWupp, 8.0 * ulpM);

        var fitWupp = CalculateFitWupp(sceneBounds, pixelWidth, pixelHeight, paddingFraction);
        var maxWupp = Math.Max(minWupp, Math.Min(AbsoluteMaxWupp, 16.0 * fitWupp));

        return (minWupp, maxWupp);
    }

    public static Camera2D CreateFitCamera(
        WorldBounds2? sceneBounds,
        int pixelWidth,
        int pixelHeight,
        double paddingFraction = DefaultPaddingFraction)
    {
        if (pixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pixelWidth));
        if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight));

        if (!sceneBounds.HasValue)
        {
            var defaultLimits = CalculateZoomLimits(null, new WorldPoint2(0, 0), null, pixelWidth, pixelHeight, paddingFraction);
            return new Camera2D(pixelWidth, pixelHeight, new WorldPoint2(0, 0), 1.0, defaultLimits.MinWupp, defaultLimits.MaxWupp);
        }

        var bounds = sceneBounds.Value;
        var center = bounds.Center;
        var fitWupp = CalculateFitWupp(bounds, pixelWidth, pixelHeight, paddingFraction);
        var limits = CalculateZoomLimits(bounds, center, null, pixelWidth, pixelHeight, paddingFraction);
        var clampedWupp = Math.Clamp(fitWupp, limits.MinWupp, limits.MaxWupp);

        return new Camera2D(pixelWidth, pixelHeight, center, clampedWupp, limits.MinWupp, limits.MaxWupp);
    }
}
