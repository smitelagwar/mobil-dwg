using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Viewer;

public enum RenderQualityMode
{
    Interaction,
    Final
}

public sealed record RenderSnapshot(
    RenderScene Scene,
    LayerTable LayerTable,
    Camera2D Camera,
    long DocumentGeneration = 1,
    long SceneRevision = 1,
    long LayoutRevision = 1,
    long StyleRevision = 1,
    long CameraRevision = 1,
    long SurfaceGeneration = 1,
    RenderQualityMode QualityMode = RenderQualityMode.Final,
    PreparedGeometryCache? GeometryCache = null,
    RenderResourceCache? ResourceCache = null
);

