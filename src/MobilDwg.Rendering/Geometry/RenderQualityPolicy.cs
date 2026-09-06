using System;
using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Rendering.Geometry;

public sealed record RenderQualitySettings(
    double InteractionMaxChordErrorPixels = 1.0,
    double FinalMaxChordErrorPixels = 0.25,
    double TextCullThresholdPixels = 0.5,
    double TextSimplifiedThresholdPixels = 3.0,
    double HatchThinningThresholdPixels = 3.0,
    double FloatPrecisionThresholdPixels = 0.1,
    double HysteresisFactor = 0.20);

public static class RenderQualityPolicy
{
    public static RenderQualitySettings DefaultSettings { get; } = new();

    public static double GetMaxChordError(
        RenderQualityMode mode,
        double worldUnitsPerPixel,
        RenderQualitySettings? settings = null)
    {
        var cfg = settings ?? DefaultSettings;
        var pixelChord = mode == RenderQualityMode.Interaction
            ? cfg.InteractionMaxChordErrorPixels
            : cfg.FinalMaxChordErrorPixels;
        return Math.Max(worldUnitsPerPixel * pixelChord, 1e-12);
    }

    /// <summary>
    /// Computes quantized power-of-two LOD band with +/- 20% hysteresis to prevent LOD thrashing on pinch.
    /// </summary>
    public static int ComputeLodBand(double worldUnitsPerPixel, int? previousLodBand = null, double hysteresis = 0.20)
    {
        if (worldUnitsPerPixel <= 0 || !double.IsFinite(worldUnitsPerPixel)) return 0;

        var continuousLod = Math.Log2(worldUnitsPerPixel);
        var baseBand = (int)Math.Round(continuousLod);

        if (previousLodBand.HasValue)
        {
            var prevBand = previousLodBand.Value;
            var diff = continuousLod - prevBand;
            // Only switch band if change exceeds 0.5 + hysteresis
            if (Math.Abs(diff) < 0.5 + hysteresis)
            {
                return prevBand;
            }
        }

        return baseBand;
    }
}
