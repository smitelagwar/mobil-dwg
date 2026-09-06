using MobilDwg.Rendering.Viewer;

namespace MobilDwg.Rendering.Skia;

public sealed record RenderFrameContext(
    int PixelWidth,
    int PixelHeight,
    double Density = 1.0,
    RenderQualityMode QualityMode = RenderQualityMode.Final,
    bool EnableOptimization = true
);
