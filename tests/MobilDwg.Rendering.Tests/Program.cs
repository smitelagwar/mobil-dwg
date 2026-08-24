using MobilDwg.Core.Rendering;

var viewport = new RenderViewport(
    pixelWidth: 1080,
    pixelHeight: 1920,
    centerX: 100,
    centerY: 200,
    worldUnitsPerPixel: 0.25);

Assert(viewport.PixelWidth == 1080, "viewport width");
Assert(viewport.PixelHeight == 1920, "viewport height");
Assert(viewport.WorldUnitsPerPixel == 0.25, "viewport scale");

AssertThrows(() => new RenderViewport(0, 100, 0, 0, 1), "zero width must fail");
AssertThrows(() => new RenderViewport(100, 0, 0, 0, 1), "zero height must fail");
AssertThrows(() => new RenderViewport(100, 100, 0, 0, 0), "zero scale must fail");
AssertThrows(() => new RenderViewport(100, 100, 0, 0, double.NaN), "NaN scale must fail");

Console.WriteLine("STAGE04_RENDER_CONTRACT_TESTS_PASS");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertThrows(Action action, string message)
{
    try
    {
        action();
    }
    catch (ArgumentOutOfRangeException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}
