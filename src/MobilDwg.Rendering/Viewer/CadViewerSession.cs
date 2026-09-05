using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Viewer;

public sealed class CadViewerSession : IDisposable
{
    private readonly SkiaCadRenderer _renderer = new();
    private bool _disposed;

    public CadDocumentMetadata Metadata { get; }
    public RenderScene ModelScene { get; }
    public CadLayoutManager LayoutManager { get; }
    public LayerTable LayerTable { get; }
    public IReadOnlyList<CadDiagnostic> Diagnostics { get; }
    public IReadOnlyList<CadCompatibilityIssue> CompatibilityIssues { get; }
    public Camera2D Camera { get; private set; }
    public int ViewportPixelWidth { get; private set; }
    public int ViewportPixelHeight { get; private set; }
    public SkiaCadRenderer Renderer => _renderer;

    public CadViewerSession(
        CadDocumentMetadata metadata,
        RenderScene modelScene,
        CadLayoutManager layoutManager,
        int initialPixelWidth = 1080,
        int initialPixelHeight = 1920,
        IReadOnlyList<CadDiagnostic>? diagnostics = null,
        IReadOnlyList<CadCompatibilityIssue>? compatibilityIssues = null)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        ModelScene = modelScene ?? throw new ArgumentNullException(nameof(modelScene));
        LayoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
        LayerTable = new LayerTable(modelScene.LayerTable.Layers);
        Diagnostics = diagnostics ?? Array.Empty<CadDiagnostic>();
        CompatibilityIssues = compatibilityIssues ?? Array.Empty<CadCompatibilityIssue>();

        ViewportPixelWidth = Math.Max(100, initialPixelWidth);
        ViewportPixelHeight = Math.Max(100, initialPixelHeight);

        // Initial camera centered on active layout bounds
        var activeScene = LayoutManager.ComposeActiveScene();
        var bounds = activeScene.WorldBounds ?? new WorldBounds2(0, 0, 100, 100);
        Camera = Camera2D.Fit(bounds, ViewportPixelWidth, ViewportPixelHeight, paddingFraction: 0.05);
    }

    public string ActiveLayoutName => LayoutManager.ActiveLayout.Name;

    public void ResizeViewport(int width, int height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (width <= 0 || height <= 0) return;

        ViewportPixelWidth = width;
        ViewportPixelHeight = height;
        Camera = Camera.Resize(width, height);
    }

    public void ZoomToFit(double paddingFraction = 0.05)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var activeScene = LayoutManager.ComposeActiveScene();
        var bounds = activeScene.WorldBounds ?? new WorldBounds2(0, 0, 100, 100);
        Camera = Camera2D.Fit(bounds, ViewportPixelWidth, ViewportPixelHeight, paddingFraction);
    }

    public void Pan(double deltaScreenX, double deltaScreenY)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Camera = Camera.PanBy(deltaScreenX, deltaScreenY);
    }

    public void Zoom(double factor, double focalScreenX, double focalScreenY)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Camera = Camera.ZoomAt(new ScreenPoint2(focalScreenX, focalScreenY), factor);
    }

    public void ToggleLayerVisibility(string layerName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (string.IsNullOrWhiteSpace(layerName)) return;

        if (LayerTable.TryGetLayer(layerName, out var layer))
        {
            LayerTable.SetLayerVisibility(layerName, !layer.IsVisible);
        }
    }

    public void SetLayerVisibility(string layerName, bool isVisible)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (string.IsNullOrWhiteSpace(layerName)) return;

        LayerTable.SetLayerVisibility(layerName, isVisible);
    }

    public void SwitchLayout(string layoutName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        LayoutManager.SwitchLayout(layoutName);
        ZoomToFit();
    }

    public ValueTask RenderAsync(SkiaBitmapRenderSurface surface, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(surface);

        var activeScene = LayoutManager.ComposeActiveScene();

        // Create updated scene with current session layer table
        var sceneWithCurrentLayers = new RenderScene(
            activeScene.Entities,
            activeScene.Diagnostics,
            activeScene.ColorContext,
            LayerTable);

        return _renderer.RenderAsync(sceneWithCurrentLayers, surface, Camera.ToViewport(), cancellationToken);
    }

    public void OnTrimMemory()
    {
        // Safe drop of any cached transient structures
        GC.Collect(1, GCCollectionMode.Optimized, blocking: false);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
