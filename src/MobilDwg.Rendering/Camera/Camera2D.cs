using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Camera;

public readonly record struct ViewPoint2(double X, double Y);
public readonly record struct ScreenPoint2(double X, double Y);

public readonly record struct Camera2D
{
    public Camera2D(
        int pixelWidth,
        int pixelHeight,
        WorldPoint2 center,
        double worldUnitsPerPixel,
        double minWorldUnitsPerPixel = 1e-12,
        double maxWorldUnitsPerPixel = 1e12)
    {
        if (pixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pixelWidth));
        if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight));
        if (!double.IsFinite(worldUnitsPerPixel) || worldUnitsPerPixel <= 0) throw new ArgumentOutOfRangeException(nameof(worldUnitsPerPixel));
        if (!double.IsFinite(minWorldUnitsPerPixel) || minWorldUnitsPerPixel <= 0) throw new ArgumentOutOfRangeException(nameof(minWorldUnitsPerPixel));
        if (!double.IsFinite(maxWorldUnitsPerPixel) || maxWorldUnitsPerPixel <= 0) throw new ArgumentOutOfRangeException(nameof(maxWorldUnitsPerPixel));
        if (maxWorldUnitsPerPixel < minWorldUnitsPerPixel) throw new ArgumentOutOfRangeException(nameof(maxWorldUnitsPerPixel));
        if (worldUnitsPerPixel < minWorldUnitsPerPixel || worldUnitsPerPixel > maxWorldUnitsPerPixel) throw new ArgumentOutOfRangeException(nameof(worldUnitsPerPixel));

        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        Center = center;
        WorldUnitsPerPixel = worldUnitsPerPixel;
        MinWorldUnitsPerPixel = minWorldUnitsPerPixel;
        MaxWorldUnitsPerPixel = maxWorldUnitsPerPixel;
    }

    public int PixelWidth { get; }
    public int PixelHeight { get; }
    public WorldPoint2 Center { get; }
    public double WorldUnitsPerPixel { get; }
    public double MinWorldUnitsPerPixel { get; }
    public double MaxWorldUnitsPerPixel { get; }

    public Camera2D ZoomBy(double factor)
    {
        if (!double.IsFinite(factor) || factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));
        var next = Math.Clamp(WorldUnitsPerPixel / factor, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
        return new Camera2D(PixelWidth, PixelHeight, Center, next, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
    }

    public static Camera2D Fit(
        WorldBounds2 bounds,
        int pixelWidth,
        int pixelHeight,
        double paddingFraction = 0.05,
        double minWorldUnitsPerPixel = 1e-12,
        double maxWorldUnitsPerPixel = 1e12)
    {
        if (pixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pixelWidth));
        if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight));
        if (!double.IsFinite(paddingFraction) || paddingFraction < 0 || paddingFraction >= 0.5) throw new ArgumentOutOfRangeException(nameof(paddingFraction));
        if (!double.IsFinite(minWorldUnitsPerPixel) || minWorldUnitsPerPixel <= 0) throw new ArgumentOutOfRangeException(nameof(minWorldUnitsPerPixel));
        if (!double.IsFinite(maxWorldUnitsPerPixel) || maxWorldUnitsPerPixel < minWorldUnitsPerPixel) throw new ArgumentOutOfRangeException(nameof(maxWorldUnitsPerPixel));

        var usableWidth = pixelWidth * (1d - (2d * paddingFraction));
        var usableHeight = pixelHeight * (1d - (2d * paddingFraction));
        var byWidth = bounds.Width / usableWidth;
        var byHeight = bounds.Height / usableHeight;
        var scale = Math.Max(byWidth, byHeight);
        if (!double.IsFinite(scale) || scale <= 0)
        {
            scale = minWorldUnitsPerPixel;
        }

        scale = Math.Clamp(scale, minWorldUnitsPerPixel, maxWorldUnitsPerPixel);
        return new Camera2D(pixelWidth, pixelHeight, bounds.Center, scale, minWorldUnitsPerPixel, maxWorldUnitsPerPixel);
    }
}

public static class CameraTransform
{
    public static ViewPoint2 WorldToView(WorldPoint2 world, Camera2D camera) =>
        new(world.X - camera.Center.X, world.Y - camera.Center.Y);

    public static ScreenPoint2 ViewToScreen(ViewPoint2 view, Camera2D camera) =>
        new(
            (camera.PixelWidth / 2d) + (view.X / camera.WorldUnitsPerPixel),
            (camera.PixelHeight / 2d) - (view.Y / camera.WorldUnitsPerPixel));

    public static ScreenPoint2 WorldToScreen(WorldPoint2 world, Camera2D camera) =>
        ViewToScreen(WorldToView(world, camera), camera);

    public static WorldPoint2 ScreenToWorld(ScreenPoint2 screen, Camera2D camera)
    {
        if (!double.IsFinite(screen.X)) throw new ArgumentOutOfRangeException(nameof(screen));
        if (!double.IsFinite(screen.Y)) throw new ArgumentOutOfRangeException(nameof(screen));

        var viewX = (screen.X - (camera.PixelWidth / 2d)) * camera.WorldUnitsPerPixel;
        var viewY = ((camera.PixelHeight / 2d) - screen.Y) * camera.WorldUnitsPerPixel;
        return new WorldPoint2(camera.Center.X + viewX, camera.Center.Y + viewY);
    }
}
