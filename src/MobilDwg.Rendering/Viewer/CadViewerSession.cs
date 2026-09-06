using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Scheduling;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Viewer;

public sealed class CadViewerSession : IDisposable
{
    private readonly object _stateLock = new();
    private readonly SkiaCadRenderer _renderer = new();
    private readonly ViewportController _controller;
    private readonly ViewportInteractionEngine _interactionEngine;
    private readonly FrameRequestGate _frameGate = new();
    private readonly PreparedGeometryCache _geometryCache = new();
    private readonly RenderResourceCache _resourceCache = new();
    private readonly Dictionary<string, Camera2D> _layoutCameras = new(StringComparer.OrdinalIgnoreCase);

    private RenderScene _activeScene;
    private LayerTable _layerTable;
    private string _currentLayoutName;
    private long _documentGeneration = 1;
    private long _sceneRevision = 1;
    private long _layoutRevision = 1;
    private long _styleRevision = 1;
    private int _activeLeaseCount;
    private bool _isRetiring;
    private bool _disposed;

    public CadDocumentMetadata Metadata { get; }
    public RenderScene ModelScene { get; }
    public CadLayoutManager LayoutManager { get; }
    public LayerTable LayerTable => _layerTable;
    public MeasurementController Measurement { get; } = new();
    public IReadOnlyList<CadDiagnostic> Diagnostics { get; }
    public IReadOnlyList<CadCompatibilityIssue> CompatibilityIssues { get; }

    public ViewportController Controller => _controller;
    public ViewportInteractionEngine InteractionEngine => _interactionEngine;
    public FrameRequestGate FrameGate => _frameGate;
    public SkiaCadRenderer Renderer => _renderer;
    public PreparedGeometryCache GeometryCache => _geometryCache;
    public RenderResourceCache ResourceCache => _resourceCache;

    public Camera2D Camera => _controller.CurrentCamera;
    public int ViewportPixelWidth => _controller.CurrentCamera.PixelWidth;
    public int ViewportPixelHeight => _controller.CurrentCamera.PixelHeight;

    public long DocumentGeneration => _documentGeneration;
    public long SceneRevision => _sceneRevision;
    public long LayoutRevision => _layoutRevision;
    public long StyleRevision => _styleRevision;
    public long CameraRevision => _interactionEngine.CameraRevision;
    public int ActiveLeaseCount => _activeLeaseCount;
    public bool IsRetiring => _isRetiring;
    public bool IsDisposed => _disposed;

    public event Action? CloseRequested;
    public event Action? DrainCompleted;

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
        _layerTable = new LayerTable(modelScene.LayerTable.Layers);
        Diagnostics = diagnostics ?? Array.Empty<CadDiagnostic>();
        CompatibilityIssues = compatibilityIssues ?? Array.Empty<CadCompatibilityIssue>();
        Measurement.SetMetadataUnitFromInsUnits(metadata.InsUnits);

        var pixelW = Math.Max(100, initialPixelWidth);
        var pixelH = Math.Max(100, initialPixelHeight);

        // Initial active scene and camera from active layout bounds
        _activeScene = LayoutManager.ComposeActiveScene();
        var bounds = _activeScene.WorldBounds ?? new WorldBounds2(0, 0, 100, 100);
        var initialCamera = ViewerZoomPolicy.CreateFitCamera(bounds, pixelW, pixelH, ViewerZoomPolicy.DefaultPaddingFraction);

        _controller = new ViewportController(initialCamera, bounds);
        _interactionEngine = new ViewportInteractionEngine(_controller);
        _currentLayoutName = LayoutManager.ActiveLayout.Name;
        _layoutCameras[_currentLayoutName] = initialCamera;
    }

    public string ActiveLayoutName => LayoutManager.ActiveLayout.Name;

    public RenderSessionLease AcquireRenderLease(
        long surfaceGeneration,
        RenderQualityMode qualityMode = RenderQualityMode.Final)
    {
        lock (_stateLock)
        {
            if (_disposed || _isRetiring)
            {
                throw new ObjectDisposedException(nameof(CadViewerSession), "Session is retiring or disposed; cannot acquire new render lease.");
            }

            _activeLeaseCount++;

            // Create immutable snapshot under lock
            var snapshot = new RenderSnapshot(
                Scene: _activeScene,
                LayerTable: new LayerTable(_layerTable.Layers),
                Camera: _controller.CurrentCamera,
                DocumentGeneration: _documentGeneration,
                SceneRevision: _sceneRevision,
                LayoutRevision: _layoutRevision,
                StyleRevision: _styleRevision,
                CameraRevision: _interactionEngine.CameraRevision,
                SurfaceGeneration: surfaceGeneration,
                QualityMode: qualityMode,
                GeometryCache: _geometryCache,
                ResourceCache: _resourceCache);

            return new RenderSessionLease(this, snapshot);
        }
    }

    internal void ReleaseRenderLease()
    {
        Action? onDrained = null;
        lock (_stateLock)
        {
            _activeLeaseCount--;
            if (_activeLeaseCount <= 0 && _isRetiring)
            {
                CompleteDisposalUnderLock(out onDrained);
            }
        }

        onDrained?.Invoke();
    }

    public void ResizeViewport(int width, int height)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (width <= 0 || height <= 0) return;

            _controller.Resize(width, height);
            _frameGate.RequestFrame();
        }
    }

    public void ZoomToFit(double paddingFraction = ViewerZoomPolicy.DefaultPaddingFraction)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var activeScene = LayoutManager.ComposeActiveScene();

            // Only consider entities on visible layers
            WorldBounds2? visibleBounds = null;
            foreach (var entity in activeScene.Entities)
            {
                if (_layerTable.IsLayerVisible(entity.Layer.Value) && entity.Bounds.Width >= 0 && entity.Bounds.Height >= 0)
                {
                    visibleBounds = visibleBounds == null ? entity.Bounds : visibleBounds.Value.Union(entity.Bounds);
                }
            }

            if (visibleBounds.HasValue)
            {
                _controller.SetSceneBounds(visibleBounds.Value);
                _controller.FitExtents(paddingFraction);
                _layoutCameras[_currentLayoutName] = _controller.CurrentCamera;
                _frameGate.RequestFrame();
            }
        }
    }

    public void Pan(double deltaScreenX, double deltaScreenY)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _controller.Pan(deltaScreenX, deltaScreenY);
            _layoutCameras[_currentLayoutName] = _controller.CurrentCamera;
            _frameGate.RequestFrame();
        }
    }

    public void Zoom(double factor, double focalScreenX, double focalScreenY)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _controller.PinchZoom(new ScreenPoint2(focalScreenX, focalScreenY), factor);
            _layoutCameras[_currentLayoutName] = _controller.CurrentCamera;
            _frameGate.RequestFrame();
        }
    }

    public void ToggleLayerVisibility(string layerName)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (string.IsNullOrWhiteSpace(layerName)) return;

            if (_layerTable.TryGetLayer(layerName, out var layer))
            {
                SetLayerVisibility(layerName, !layer.IsVisible);
            }
        }
    }

    public void SetLayerVisibility(string layerName, bool isVisible)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (string.IsNullOrWhiteSpace(layerName)) return;

            var newTable = new LayerTable(_layerTable.Layers);
            newTable.SetLayerVisibility(layerName, isVisible);
            _layerTable = newTable;
            _styleRevision++;
            _frameGate.RequestFrame();
        }
    }

    public void SwitchLayout(string layoutName)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (string.IsNullOrWhiteSpace(layoutName)) return;

            // Save camera of current layout
            _layoutCameras[_currentLayoutName] = _controller.CurrentCamera;

            LayoutManager.SwitchLayout(layoutName);
            _currentLayoutName = layoutName;
            _activeScene = LayoutManager.ComposeActiveScene();
            _layoutRevision++;
            _sceneRevision++;

            var bounds = _activeScene.WorldBounds ?? new WorldBounds2(0, 0, 100, 100);
            _controller.SetSceneBounds(bounds);

            // Restore previous camera if visited, or initialize with Fit
            if (_layoutCameras.TryGetValue(layoutName, out var savedCamera))
            {
                _controller.SetCamera(savedCamera);
            }
            else
            {
                _controller.FitExtents();
                _layoutCameras[layoutName] = _controller.CurrentCamera;
            }

            _frameGate.RequestFrame();
        }
    }

    public ValueTask RenderAsync(SkiaBitmapRenderSurface surface, CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(surface);

            var activeScene = LayoutManager.ComposeActiveScene();
            var sceneWithCurrentLayers = new RenderScene(
                activeScene.Entities,
                activeScene.Diagnostics,
                activeScene.ColorContext,
                _layerTable);

            return _renderer.RenderAsync(sceneWithCurrentLayers, surface, Camera.ToViewport(), cancellationToken);
        }
    }

    public void OnTrimMemory()
    {
        lock (_stateLock)
        {
            if (_disposed) return;
            _geometryCache.Clear();
            _resourceCache.Clear();
        }
    }

    public void Dispose()
    {
        Action? onClosed = null;
        Action? onDrained = null;
        lock (_stateLock)
        {
            if (_disposed || _isRetiring) return;
            _isRetiring = true;
            onClosed = CloseRequested;
            if (_activeLeaseCount <= 0)
            {
                CompleteDisposalUnderLock(out onDrained);
            }
        }

        onClosed?.Invoke();
        onDrained?.Invoke();
    }

    private void CompleteDisposalUnderLock(out Action? onDrained)
    {
        _disposed = true;
        _isRetiring = false;
        _geometryCache.Dispose();
        _resourceCache.Dispose();
        _frameGate.Reset();
        onDrained = DrainCompleted;
    }
}
