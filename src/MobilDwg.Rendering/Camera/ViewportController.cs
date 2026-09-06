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
        if (sceneBounds.HasValue)
        {
            UpdateLimitsForCurrentCenter();
        }
    }

    public Camera2D CurrentCamera => _camera;

    public WorldBounds2? SceneBounds => _sceneBounds;

    public bool IsInteracting => _isInteracting;

    public long UpdateCount => _updateCount;

    public void SetSceneBounds(WorldBounds2 bounds)
    {
        _sceneBounds = bounds;
        UpdateLimitsForCurrentCenter();
    }

    public void SetCamera(Camera2D camera)
    {
        if (!camera.IsValid)
        {
            throw new ArgumentException("Camera must be valid.", nameof(camera));
        }

        var old = _camera;
        _camera = camera;
        UpdateLimitsForCurrentCenter();
        if (_camera != old)
        {
            _updateCount++;
        }
    }

    public void BeginInteraction()
    {
        _isInteracting = true;
    }

    public void EndInteraction()
    {
        _isInteracting = false;
    }

    public Camera2D Manipulate(ScreenPoint2 previousCentroid, ScreenPoint2 currentCentroid, double factor)
    {
        if (factor == 1.0 && previousCentroid.X == currentCentroid.X && previousCentroid.Y == currentCentroid.Y)
        {
            return _camera;
        }

        var old = _camera;
        var anchorWorld = CameraTransform.ScreenToWorld(previousCentroid, _camera);
        var (minWupp, maxWupp) = ViewerZoomPolicy.CalculateZoomLimits(
            _sceneBounds, _camera.Center, anchorWorld, _camera.PixelWidth, _camera.PixelHeight);

        var currentWupp = Math.Clamp(_camera.WorldUnitsPerPixel, minWupp, maxWupp);
        if (currentWupp != _camera.WorldUnitsPerPixel || _camera.MinWorldUnitsPerPixel != minWupp || _camera.MaxWorldUnitsPerPixel != maxWupp)
        {
            _camera = new Camera2D(_camera.PixelWidth, _camera.PixelHeight, _camera.Center, currentWupp, minWupp, maxWupp);
        }

        _camera = _camera.Manipulate(previousCentroid, currentCentroid, factor, minWupp, maxWupp);
        EnforceCoordinateGuard();
        if (_camera != old)
        {
            _updateCount++;
        }
        return _camera;
    }

    public Camera2D Pan(double deltaScreenX, double deltaScreenY)
    {
        if (deltaScreenX == 0 && deltaScreenY == 0)
        {
            return _camera;
        }

        var old = _camera;
        _camera = _camera.PanBy(deltaScreenX, deltaScreenY);
        EnforceCoordinateGuard();
        UpdateLimitsForCurrentCenter();
        if (_camera != old)
        {
            _updateCount++;
        }
        return _camera;
    }

    public Camera2D PinchZoom(ScreenPoint2 focalPoint, double scaleFactor)
    {
        return Manipulate(focalPoint, focalPoint, scaleFactor);
    }

    public Camera2D ZoomIn(double factor = ViewerZoomPolicy.ButtonZoomFactor)
    {
        var centerScreen = new ScreenPoint2(_camera.PixelWidth / 2d, _camera.PixelHeight / 2d);
        return Manipulate(centerScreen, centerScreen, factor);
    }

    public Camera2D ZoomOut(double factor = ViewerZoomPolicy.ButtonZoomFactor)
    {
        var centerScreen = new ScreenPoint2(_camera.PixelWidth / 2d, _camera.PixelHeight / 2d);
        return Manipulate(centerScreen, centerScreen, 1.0 / factor);
    }

    public Camera2D DoubleTap(ScreenPoint2 tapPoint, double zoomMultiplier = ViewerZoomPolicy.DoubleTapZoomFactor)
    {
        // DoubleTap is always a 2x zoom at the tap point per audited specification; Fit is a separate control.
        return Manipulate(tapPoint, tapPoint, zoomMultiplier);
    }

    public Camera2D FitExtents(double paddingFraction = ViewerZoomPolicy.DefaultPaddingFraction)
    {
        if (!_sceneBounds.HasValue)
        {
            return _camera;
        }

        var old = _camera;
        _camera = ViewerZoomPolicy.CreateFitCamera(_sceneBounds.Value, _camera.PixelWidth, _camera.PixelHeight, paddingFraction);
        if (_camera != old)
        {
            _updateCount++;
        }
        return _camera;
    }

    public Camera2D Resize(int newPixelWidth, int newPixelHeight)
    {
        var old = _camera;
        var (minWupp, maxWupp) = ViewerZoomPolicy.CalculateZoomLimits(
            _sceneBounds, _camera.Center, null, newPixelWidth, newPixelHeight);
        var clampedWupp = Math.Clamp(_camera.WorldUnitsPerPixel, minWupp, maxWupp);
        _camera = new Camera2D(newPixelWidth, newPixelHeight, _camera.Center, clampedWupp, minWupp, maxWupp);
        if (_camera != old)
        {
            _updateCount++;
        }
        return _camera;
    }

    private void EnforceCoordinateGuard()
    {
        var cx = Math.Clamp(_camera.Center.X, -ViewerZoomPolicy.CoordinateLimit, ViewerZoomPolicy.CoordinateLimit);
        var cy = Math.Clamp(_camera.Center.Y, -ViewerZoomPolicy.CoordinateLimit, ViewerZoomPolicy.CoordinateLimit);
        if (cx != _camera.Center.X || cy != _camera.Center.Y)
        {
            _camera = new Camera2D(_camera.PixelWidth, _camera.PixelHeight, new WorldPoint2(cx, cy), _camera.WorldUnitsPerPixel, _camera.MinWorldUnitsPerPixel, _camera.MaxWorldUnitsPerPixel);
        }
    }

    private void UpdateLimitsForCurrentCenter()
    {
        var (minWupp, maxWupp) = ViewerZoomPolicy.CalculateZoomLimits(
            _sceneBounds, _camera.Center, null, _camera.PixelWidth, _camera.PixelHeight);
        if (_camera.MinWorldUnitsPerPixel != minWupp || _camera.MaxWorldUnitsPerPixel != maxWupp)
        {
            var clampedWupp = Math.Clamp(_camera.WorldUnitsPerPixel, minWupp, maxWupp);
            _camera = new Camera2D(_camera.PixelWidth, _camera.PixelHeight, _camera.Center, clampedWupp, minWupp, maxWupp);
        }
    }
}
