using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Blocks;

public sealed record BlockAttribute(
    string Tag,
    string Text,
    WorldPoint2 Position,
    double Height,
    double RotationRadians = 0d,
    bool IsInvisible = false)
{
    public static BlockAttribute CreateVisible(string tag, string text, WorldPoint2 position, double height, double rotationRadians = 0d) =>
        new(tag, text, position, height, rotationRadians, IsInvisible: false);

    public static BlockAttribute CreateInvisible(string tag, string text, WorldPoint2 position, double height) =>
        new(tag, text, position, height, RotationRadians: 0d, IsInvisible: true);
}
