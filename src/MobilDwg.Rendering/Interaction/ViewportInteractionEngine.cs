using System;
using System.Collections.Generic;
using System.Linq;
using MobilDwg.Rendering.Camera;

namespace MobilDwg.Rendering.Interaction;

public readonly record struct InteractionResult(
    bool Handled,
    bool CameraChanged,
    Camera2D Camera,
    ViewportGestureState State,
    bool SingleTapDetected,
    ScreenPoint2? TapPosition,
    bool DoubleTapDetected,
    ScreenPoint2? DoubleTapPosition);

public sealed class ViewportInteractionEngine
{
    private readonly ViewportController _controller;
    private readonly ViewportInputConfiguration _configuration;
    private readonly Dictionary<int, ScreenPoint2> _activePointers = new();

    private ViewportGestureState _state = ViewportGestureState.Idle;
    private ScreenPoint2 _initialDownPosition;
    private long _initialDownTimeMs;
    private ScreenPoint2 _prevPanPoint;
    private ScreenPoint2 _prevCentroid;
    private double _prevSpan;

    private ScreenPoint2 _lastTapPosition;
    private long _lastTapTimeMs;
    private bool _hasDoubleTapCandidate;
    private bool _isMeasurementMode;

    private long _cameraRevision;

    public ViewportInteractionEngine(
        ViewportController controller,
        ViewportInputConfiguration? configuration = null)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _configuration = configuration ?? ViewportInputConfiguration.Default;
    }

    public ViewportController Controller => _controller;
    public ViewportInputConfiguration Configuration => _configuration;
    public ViewportGestureState State => _state;
    public int ActivePointerCount => _activePointers.Count;
    public long CameraRevision => _cameraRevision;

    public bool IsMeasurementMode
    {
        get => _isMeasurementMode;
        set => _isMeasurementMode = value;
    }

    public event Action<Camera2D>? CameraChanged;
    public event Action<ScreenPoint2>? SingleTapDetected;
    public event Action<ScreenPoint2>? DoubleTapDetected;
    public event Action? InteractionStarted;
    public event Action? InteractionEnded;

    public InteractionResult ProcessPacket(PointerPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        bool cameraChanged = false;
        bool singleTap = false;
        ScreenPoint2? tapPos = null;
        bool doubleTap = false;
        ScreenPoint2? doubleTapPos = null;

        switch (packet.Action)
        {
            case PointerAction.Down:
                cameraChanged = HandleDown(packet);
                break;

            case PointerAction.PointerDown:
                cameraChanged = HandlePointerDown(packet);
                break;

            case PointerAction.Move:
                cameraChanged = HandleMove(packet);
                break;

            case PointerAction.PointerUp:
                cameraChanged = HandlePointerUp(packet);
                break;

            case PointerAction.Up:
                cameraChanged = HandleUp(packet, out singleTap, out tapPos, out doubleTap, out doubleTapPos);
                break;

            case PointerAction.Cancel:
                HandleCancel();
                break;
        }

        if (cameraChanged)
        {
            _cameraRevision++;
            CameraChanged?.Invoke(_controller.CurrentCamera);
        }

        if (singleTap && tapPos.HasValue)
        {
            SingleTapDetected?.Invoke(tapPos.Value);
        }

        if (doubleTap && doubleTapPos.HasValue)
        {
            DoubleTapDetected?.Invoke(doubleTapPos.Value);
        }

        return new InteractionResult(
            Handled: true,
            CameraChanged: cameraChanged,
            Camera: _controller.CurrentCamera,
            State: _state,
            SingleTapDetected: singleTap,
            TapPosition: tapPos,
            DoubleTapDetected: doubleTap,
            DoubleTapPosition: doubleTapPos);
    }

    private bool HandleDown(PointerPacket packet)
    {
        _activePointers.Clear();
        var sample = FindPointer(packet, packet.ActionPointerId);
        _activePointers[packet.ActionPointerId] = sample.Position;

        _state = ViewportGestureState.TapCandidate;
        _initialDownPosition = sample.Position;
        _initialDownTimeMs = packet.EventTimeMs;
        _prevPanPoint = sample.Position;

        _controller.BeginInteraction();
        InteractionStarted?.Invoke();
        return false;
    }

    private bool HandlePointerDown(PointerPacket packet)
    {
        bool cameraChanged = false;

        // Process any pending movement of already tracked pointers with current state
        cameraChanged = ApplyIncrementalMove(packet);

        // Add new pointer
        var newSample = FindPointer(packet, packet.ActionPointerId);
        _activePointers[packet.ActionPointerId] = newSample.Position;

        // Cancel tap candidates on multi-touch
        _hasDoubleTapCandidate = false;

        if (_activePointers.Count == 2)
        {
            _state = ViewportGestureState.Pinch;
            EstablishPinchBaseline();
        }
        else if (_activePointers.Count >= 3)
        {
            _state = ViewportGestureState.MultiTouchHold;
        }

        return cameraChanged;
    }

    private bool HandleMove(PointerPacket packet)
    {
        return ApplyIncrementalMove(packet);
    }

    private bool HandlePointerUp(PointerPacket packet)
    {
        bool cameraChanged = false;

        // Apply any final movement before removing pointer
        cameraChanged = ApplyIncrementalMove(packet);

        _activePointers.Remove(packet.ActionPointerId);

        if (_activePointers.Count == 2)
        {
            _state = ViewportGestureState.Pinch;
            EstablishPinchBaseline();
        }
        else if (_activePointers.Count == 1)
        {
            _state = ViewportGestureState.Pan;
            _prevPanPoint = _activePointers.Values.First();
        }
        else if (_activePointers.Count == 0)
        {
            _state = ViewportGestureState.Idle;
            _controller.EndInteraction();
            InteractionEnded?.Invoke();
        }

        return cameraChanged;
    }

    private bool HandleUp(
        PointerPacket packet,
        out bool singleTap,
        out ScreenPoint2? tapPos,
        out bool doubleTap,
        out ScreenPoint2? doubleTapPos)
    {
        singleTap = false;
        tapPos = null;
        doubleTap = false;
        doubleTapPos = null;
        bool cameraChanged = false;

        var sample = FindPointer(packet, packet.ActionPointerId);

        if (_state == ViewportGestureState.TapCandidate)
        {
            var dist = Distance(sample.Position, _initialDownPosition);
            if (dist > _configuration.TouchSlopPx)
            {
                // Slop exceeded on final UP sample -> commit pan
                var dx = sample.Position.X - _initialDownPosition.X;
                var dy = sample.Position.Y - _initialDownPosition.Y;
                if (dx != 0 || dy != 0)
                {
                    _controller.Pan(dx, dy);
                    cameraChanged = true;
                }
                _hasDoubleTapCandidate = false;
            }
            else
            {
                // Tap detected
                if (!_isMeasurementMode && _hasDoubleTapCandidate &&
                    (packet.EventTimeMs - _lastTapTimeMs <= _configuration.DoubleTapTimeoutMs) &&
                    Distance(sample.Position, _lastTapPosition) <= _configuration.DoubleTapSlopPx)
                {
                    doubleTap = true;
                    doubleTapPos = sample.Position;
                    _controller.DoubleTap(sample.Position, _configuration.DoubleTapZoomFactor);
                    cameraChanged = true;
                    _hasDoubleTapCandidate = false;
                }
                else
                {
                    singleTap = true;
                    tapPos = sample.Position;
                    _lastTapPosition = sample.Position;
                    _lastTapTimeMs = packet.EventTimeMs;
                    _hasDoubleTapCandidate = true;
                }
            }
        }
        else if (_state == ViewportGestureState.Pan)
        {
            var dx = sample.Position.X - _prevPanPoint.X;
            var dy = sample.Position.Y - _prevPanPoint.Y;
            if (dx != 0 || dy != 0)
            {
                _controller.Pan(dx, dy);
                cameraChanged = true;
            }
        }
        else if (_state == ViewportGestureState.Pinch)
        {
            cameraChanged = ApplyIncrementalMove(packet);
        }

        _activePointers.Clear();
        _state = ViewportGestureState.Idle;
        _controller.EndInteraction();
        InteractionEnded?.Invoke();

        return cameraChanged;
    }

    private void HandleCancel()
    {
        _activePointers.Clear();
        _hasDoubleTapCandidate = false;
        _state = ViewportGestureState.Idle;
        _controller.EndInteraction();
        InteractionEnded?.Invoke();
    }

    private bool ApplyIncrementalMove(PointerPacket packet)
    {
        bool cameraChanged = false;

        // Update positions of tracked pointers
        foreach (var sample in packet.Pointers)
        {
            if (_activePointers.ContainsKey(sample.Id))
            {
                _activePointers[sample.Id] = sample.Position;
            }
        }

        if (_state == ViewportGestureState.TapCandidate)
        {
            var pos = _activePointers.Values.First();
            var dist = Distance(pos, _initialDownPosition);
            if (dist > _configuration.TouchSlopPx)
            {
                _state = ViewportGestureState.Pan;
                _hasDoubleTapCandidate = false;
                var dx = pos.X - _initialDownPosition.X;
                var dy = pos.Y - _initialDownPosition.Y;
                if (dx != 0 || dy != 0)
                {
                    _controller.Pan(dx, dy);
                    _prevPanPoint = pos;
                    cameraChanged = true;
                }
            }
        }
        else if (_state == ViewportGestureState.Pan)
        {
            if (_activePointers.Count >= 1)
            {
                var pos = _activePointers.Values.First();
                var dx = pos.X - _prevPanPoint.X;
                var dy = pos.Y - _prevPanPoint.Y;
                if (dx != 0 || dy != 0)
                {
                    _controller.Pan(dx, dy);
                    _prevPanPoint = pos;
                    cameraChanged = true;
                }
            }
        }
        else if (_state == ViewportGestureState.Pinch)
        {
            if (_activePointers.Count == 2)
            {
                var p0 = _activePointers.Values.First();
                var p1 = _activePointers.Values.Last();
                var currCentroid = new ScreenPoint2((p0.X + p1.X) / 2d, (p0.Y + p1.Y) / 2d);
                var currSpan = Distance(p0, p1);

                double factor;
                if (_prevSpan < _configuration.MinSpanPx || currSpan < _configuration.MinSpanPx)
                {
                    // Span below threshold: translate centroid only, factor 1.0, reset baseline
                    factor = 1.0;
                }
                else
                {
                    factor = currSpan / _prevSpan;
                }

                _controller.Manipulate(_prevCentroid, currCentroid, factor);
                _prevCentroid = currCentroid;
                _prevSpan = currSpan;
                cameraChanged = true;
            }
        }

        return cameraChanged;
    }

    private void EstablishPinchBaseline()
    {
        if (_activePointers.Count >= 2)
        {
            var p0 = _activePointers.Values.First();
            var p1 = _activePointers.Values.Skip(1).First();
            _prevCentroid = new ScreenPoint2((p0.X + p1.X) / 2d, (p0.Y + p1.Y) / 2d);
            _prevSpan = Distance(p0, p1);
        }
    }

    private static PointerSample FindPointer(PointerPacket packet, int id)
    {
        foreach (var sample in packet.Pointers)
        {
            if (sample.Id == id) return sample;
        }

        if (packet.Pointers.Count > 0)
        {
            return packet.Pointers[0];
        }

        return new PointerSample(id, new ScreenPoint2(0, 0));
    }

    private static double Distance(ScreenPoint2 a, ScreenPoint2 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
