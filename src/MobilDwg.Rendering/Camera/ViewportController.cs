using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Camera;

public sealed class ViewportController
{
    private Camera2D _camera;
    private WorldBounds2? _sceneBounds;
    private bool _isInteracting;
    private long _updateCount;

    public ViewportController(Camera2D initialCamera, WorldBounds2? sceneBounds = null)
    {
        if (!initialCamera.IsValid)
        {
            throw new ArgumentException("Initial camera must be valid.", nameof(initialCamera));
        }

        _camera = initialCamera;
        _sceneBounds = sceneBounds;
    }

    public Camera2D CurrentCamera => _camera;

    public WorldBounds2? SceneBounds => _sceneBounds;

    public bool IsInteracting => _isInteracting;

    public long UpdateCount => _updateCount;

    public void SetSceneBounds(WorldBounds2 bounds)
    {
        _sceneBounds = bounds;
    }

    public void BeginInteraction()
    {
        _isInteracting = true;
    }

    public void EndInteraction()
    {
        _isInteracting = false;
    }

    public Camera2D Pan(double deltaScreenX, double deltaScreenY)
    {
        _camera = _camera.PanBy(deltaScreenX, deltaScreenY);
        _updateCount++;
        return _camera;
    }

    public Camera2D PinchZoom(ScreenPoint2 focalPoint, double scaleFactor)
    {
        _camera = _camera.ZoomAt(focalPoint, scaleFactor);
        _updateCount++;
        return _camera;
    }

    public Camera2D ZoomIn(double factor = 2.0)
    {
        _camera = _camera.ZoomBy(factor);
        _updateCount++;
        return _camera;
    }

    public Camera2D ZoomOut(double factor = 2.0)
    {
        _camera = _camera.ZoomBy(1.0 / factor);
        _updateCount++;
        return _camera;
    }

    public Camera2D DoubleTap(ScreenPoint2 tapPoint, double zoomMultiplier = 2.0)
    {
        if (_sceneBounds.HasValue)
        {
            var fitCamera = Camera2D.Fit(_sceneBounds.Value, _camera.PixelWidth, _camera.PixelHeight);
            // If already significantly zoomed in compared to fit extents, reset to fit extents
            if (_camera.WorldUnitsPerPixel < fitCamera.WorldUnitsPerPixel * 0.7)
            {
                _camera = fitCamera;
                _updateCount++;
                return _camera;
            }
        }

        // Otherwise zoom in at the tap point
        _camera = _camera.ZoomAt(tapPoint, zoomMultiplier);
        _updateCount++;
        return _camera;
    }

    public Camera2D FitExtents(double paddingFraction = 0.05)
    {
        if (!_sceneBounds.HasValue)
        {
            return _camera;
        }

        _camera = Camera2D.Fit(_sceneBounds.Value, _camera.PixelWidth, _camera.PixelHeight, paddingFraction);
        _updateCount++;
        return _camera;
    }

    public Camera2D Resize(int newPixelWidth, int newPixelHeight)
    {
        _camera = _camera.Resize(newPixelWidth, newPixelHeight);
        _updateCount++;
        return _camera;
    }
}
