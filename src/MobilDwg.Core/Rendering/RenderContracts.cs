using MobilDwg.Core.Documents;

namespace MobilDwg.Core.Rendering;

public interface IRenderScene
{
}

public interface IRenderSurface
{
    int PixelWidth { get; }

    int PixelHeight { get; }

    double Density { get; }
}

public readonly record struct RenderViewport
{
    public RenderViewport(
        int pixelWidth,
        int pixelHeight,
        double centerX,
        double centerY,
        double worldUnitsPerPixel)
    {
        if (pixelWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pixelWidth));
        }

        if (pixelHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pixelHeight));
        }

        if (!double.IsFinite(worldUnitsPerPixel) || worldUnitsPerPixel <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(worldUnitsPerPixel));
        }

        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        CenterX = centerX;
        CenterY = centerY;
        WorldUnitsPerPixel = worldUnitsPerPixel;
    }

    public int PixelWidth { get; }

    public int PixelHeight { get; }

    public double CenterX { get; }

    public double CenterY { get; }

    public double WorldUnitsPerPixel { get; }
}

public sealed record RenderSceneBuildOptions(bool IncludePaperSpace = true);

public interface IRenderSceneBuilder
{
    ValueTask<IRenderScene> BuildAsync(
        CadDocumentSession session,
        RenderSceneBuildOptions options,
        CancellationToken cancellationToken = default);
}

public interface ICadRenderer
{
    ValueTask RenderAsync(
        IRenderScene scene,
        IRenderSurface surface,
        RenderViewport viewport,
        CancellationToken cancellationToken = default);
}
