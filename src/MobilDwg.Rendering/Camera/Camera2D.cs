using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Camera;

public readonly record struct ViewPoint2
{
    public ViewPoint2(double x, double y)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        X = x;
        Y = y;
    }

    public double X { get; }
    public double Y { get; }
}

public readonly record struct ScreenPoint2
{
    public ScreenPoint2(double x, double y)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        X = x;
        Y = y;
    }

    public double X { get; }
    public double Y { get; }
}

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

    public bool IsValid =>
        PixelWidth > 0 &&
        PixelHeight > 0 &&
        double.IsFinite(WorldUnitsPerPixel) &&
        WorldUnitsPerPixel > 0 &&
        double.IsFinite(MinWorldUnitsPerPixel) &&
        MinWorldUnitsPerPixel > 0 &&
        double.IsFinite(MaxWorldUnitsPerPixel) &&
        MaxWorldUnitsPerPixel >= MinWorldUnitsPerPixel &&
        WorldUnitsPerPixel >= MinWorldUnitsPerPixel &&
        WorldUnitsPerPixel <= MaxWorldUnitsPerPixel;

    public Camera2D ZoomBy(double factor)
    {
        EnsureValid();
        if (!double.IsFinite(factor) || factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));
        var next = Math.Clamp(WorldUnitsPerPixel / factor, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
        return new Camera2D(PixelWidth, PixelHeight, Center, next, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
    }

    public Camera2D ZoomAt(ScreenPoint2 focalScreenPoint, double factor)
    {
        EnsureValid();
        if (!double.IsFinite(factor) || factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));
        var worldFocal = CameraTransform.ScreenToWorld(focalScreenPoint, this);
        var nextWupp = Math.Clamp(WorldUnitsPerPixel / factor, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
        var newCenterX = worldFocal.X - ((focalScreenPoint.X - (PixelWidth / 2d)) * nextWupp);
        var newCenterY = worldFocal.Y - (((PixelHeight / 2d) - focalScreenPoint.Y) * nextWupp);
        return new Camera2D(PixelWidth, PixelHeight, new WorldPoint2(newCenterX, newCenterY), nextWupp, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
    }

    public Camera2D PanBy(double deltaScreenX, double deltaScreenY)
    {
        EnsureValid();
        if (!double.IsFinite(deltaScreenX)) throw new ArgumentOutOfRangeException(nameof(deltaScreenX));
        if (!double.IsFinite(deltaScreenY)) throw new ArgumentOutOfRangeException(nameof(deltaScreenY));
        var newCenterX = Center.X - (deltaScreenX * WorldUnitsPerPixel);
        var newCenterY = Center.Y + (deltaScreenY * WorldUnitsPerPixel);
        return new Camera2D(PixelWidth, PixelHeight, new WorldPoint2(newCenterX, newCenterY), WorldUnitsPerPixel, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
    }

    public Camera2D Resize(int newPixelWidth, int newPixelHeight)
    {
        EnsureValid();
        if (newPixelWidth <= 0) throw new ArgumentOutOfRangeException(nameof(newPixelWidth));
        if (newPixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(newPixelHeight));
        return new Camera2D(newPixelWidth, newPixelHeight, Center, WorldUnitsPerPixel, MinWorldUnitsPerPixel, MaxWorldUnitsPerPixel);
    }

    public RenderViewport ToViewport()
    {
        EnsureValid();
        return new RenderViewport(PixelWidth, PixelHeight, Center.X, Center.Y, WorldUnitsPerPixel);
    }

    public WorldBounds2 GetVisibleWorldBounds(double paddingFraction = 0d)
    {
        EnsureValid();
        var halfSpanX = (PixelWidth * (1d + (2d * paddingFraction)) / 2d) * WorldUnitsPerPixel;
        var halfSpanY = (PixelHeight * (1d + (2d * paddingFraction)) / 2d) * WorldUnitsPerPixel;
        return new WorldBounds2(
            Center.X - halfSpanX,
            Center.Y - halfSpanY,
            Center.X + halfSpanX,
            Center.Y + halfSpanY);
    }

    public static Camera2D FromViewport(
        RenderViewport viewport,
        double minWorldUnitsPerPixel = 1e-12,
        double maxWorldUnitsPerPixel = 1e12) => new(
            viewport.PixelWidth,
            viewport.PixelHeight,
            new WorldPoint2(viewport.CenterX, viewport.CenterY),
            viewport.WorldUnitsPerPixel,
            minWorldUnitsPerPixel,
            maxWorldUnitsPerPixel);

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

    private void EnsureValid()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException("Camera state is not valid. Use a validating constructor or Camera2D.Fit.");
        }
    }
}

public static class CameraTransform
{
    public static ViewPoint2 WorldToView(WorldPoint2 world, Camera2D camera)
    {
        EnsureValid(camera);
        return new ViewPoint2(world.X - camera.Center.X, world.Y - camera.Center.Y);
    }

    public static ScreenPoint2 ViewToScreen(ViewPoint2 view, Camera2D camera)
    {
        EnsureValid(camera);
        return new ScreenPoint2(
            (camera.PixelWidth / 2d) + (view.X / camera.WorldUnitsPerPixel),
            (camera.PixelHeight / 2d) - (view.Y / camera.WorldUnitsPerPixel));
    }

    public static ScreenPoint2 WorldToScreen(WorldPoint2 world, Camera2D camera) =>
        ViewToScreen(WorldToView(world, camera), camera);

    public static WorldPoint2 ScreenToWorld(ScreenPoint2 screen, Camera2D camera)
    {
        EnsureValid(camera);
        var viewX = (screen.X - (camera.PixelWidth / 2d)) * camera.WorldUnitsPerPixel;
        var viewY = ((camera.PixelHeight / 2d) - screen.Y) * camera.WorldUnitsPerPixel;
        return new WorldPoint2(camera.Center.X + viewX, camera.Center.Y + viewY);
    }

    private static void EnsureValid(Camera2D camera)
    {
        if (!camera.IsValid)
        {
            throw new ArgumentException("Camera state is invalid.", nameof(camera));
        }
    }
}
