using MobilDwg.Core.Reading;
using MobilDwg.Core.Rendering;

namespace MobilDwg.App;

public sealed record AppCompositionBoundary(
    ICadDocumentReader Reader,
    IRenderSceneBuilder SceneBuilder,
    ICadRenderer Renderer);
