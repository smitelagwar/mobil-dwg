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
    private readonly object? _syncRoot;
    private readonly Dictionary<int, ScreenPoint2> _activePointers = new();

    private ViewportInputConfiguration _configuration;
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
    private long _currentSurfaceGeneration;
    private long _lastInputEventTimeMs;

    public ViewportInteractionEngine(
        ViewportController controller,
        ViewportInputConfiguration? configuration = null,
        object? syncRoot = null)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _configuration = configuration ?? ViewportInputConfiguration.Default;
        _syncRoot = syncRoot;
    }

    public ViewportController Controller => _controller;
    public object? SyncRoot => _syncRoot;
    public ViewportInputConfiguration Configuration
    {
        get => _configuration;
        set => _configuration = value ?? ViewportInputConfiguration.Default;
    }
    public ViewportGestureState State => _state;
    public int ActivePointerCount => _activePointers.Count;
    public long CameraRevision => _cameraRevision;
    public long CurrentSurfaceGeneration => _currentSurfaceGeneration;
    public long LastInputEventTimeMs => _lastInputEventTimeMs;

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

    public void CancelGesture()
    {
        if (_syncRoot != null)
        {
            lock (_syncRoot)
            {
                HandleCancel();
            }
            return;
        }
        HandleCancel();
    }

    public void Suspend()
    {
        if (_syncRoot != null)
        {
            lock (_syncRoot)
            {
                _activePointers.Clear();
                _hasDoubleTapCandidate = false;
                _state = ViewportGestureState.Suspended;
                _controller.EndInteraction();
                InteractionEnded?.Invoke();
            }
            return;
        }

        _activePointers.Clear();
        _hasDoubleTapCandidate = false;
        _state = ViewportGestureState.Suspended;
        _controller.EndInteraction();
        InteractionEnded?.Invoke();
    }

    public void Resume()
    {
        if (_syncRoot != null)
        {
            lock (_syncRoot)
            {
                if (_state == ViewportGestureState.Suspended)
                {
                    _state = ViewportGestureState.Idle;
                }
            }
            return;
        }

        if (_state == ViewportGestureState.Suspended)
        {
            _state = ViewportGestureState.Idle;
        }
    }

    public void InvalidateSurfaceGeneration(long newGeneration)
    {
        if (_syncRoot != null)
        {
            lock (_syncRoot)
            {
                if (_currentSurfaceGeneration != newGeneration)
                {
                    _currentSurfaceGeneration = newGeneration;
                    HandleCancel();
                }
            }
            return;
        }

        if (_currentSurfaceGeneration != newGeneration)
        {
            _currentSurfaceGeneration = newGeneration;
            HandleCancel();
        }
    }

    public InteractionResult ProcessPacket(PointerPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (_syncRoot != null)
        {
            lock (_syncRoot)
            {
                return ProcessPacketCore(packet);
            }
        }

        return ProcessPacketCore(packet);
    }

    private InteractionResult ProcessPacketCore(PointerPacket packet)
    {
        _lastInputEventTimeMs = packet.EventTimeMs;

        // Coordinate sanity check
        foreach (var sample in packet.Pointers)
        {
            if (!double.IsFinite(sample.Position.X) || !double.IsFinite(sample.Position.Y) ||
                Math.Abs(sample.Position.X) > 1e7 || Math.Abs(sample.Position.Y) > 1e7)
            {
                HandleCancel();
                return new InteractionResult(
                    Handled: false,
                    CameraChanged: false,
                    Camera: _controller.CurrentCamera,
                    State: _state,
                    SingleTapDetected: false,
                    TapPosition: null,
                    DoubleTapDetected: false,
                    DoubleTapPosition: null);
            }
        }

        // Generation check
        if (packet.Action == PointerAction.Down)
        {
            if (packet.SurfaceGeneration != 0)
            {
                _currentSurfaceGeneration = packet.SurfaceGeneration;
            }
        }
        else if (_currentSurfaceGeneration != 0 && packet.SurfaceGeneration != 0 && packet.SurfaceGeneration != _currentSurfaceGeneration)
        {
            HandleCancel();
            return new InteractionResult(
                Handled: false,
                CameraChanged: false,
                Camera: _controller.CurrentCamera,
                State: _state,
                SingleTapDetected: false,
                TapPosition: null,
                DoubleTapDetected: false,
                DoubleTapPosition: null);
        }

        bool singleTap = false;
        ScreenPoint2? tapPos = null;
        bool doubleTap = false;
        ScreenPoint2? doubleTapPos = null;

        bool wasInGesture = _state == ViewportGestureState.Pan || _state == ViewportGestureState.Pinch;
        var cameraBefore = _controller.CurrentCamera;

        switch (packet.Action)
        {
            case PointerAction.Down:
                HandleDown(packet);
                break;

            case PointerAction.PointerDown:
                HandlePointerDown(packet);
                break;

            case PointerAction.Move:
                HandleMove(packet);
                break;

            case PointerAction.PointerUp:
                HandlePointerUp(packet);
                break;

            case PointerAction.Up:
                HandleUp(packet, out singleTap, out tapPos, out doubleTap, out doubleTapPos);
                break;

            case PointerAction.Cancel:
                HandleCancel();
                break;
        }

        var cameraAfter = _controller.CurrentCamera;
        bool cameraChanged = (cameraAfter != cameraBefore);

        if (cameraChanged)
        {
            _cameraRevision++;
            CameraChanged?.Invoke(cameraAfter);
        }
        else if (packet.Action == PointerAction.Up && wasInGesture)
        {
            // Even if camera revision did not change on release, notify host so final high-quality paint is scheduled
            CameraChanged?.Invoke(cameraAfter);
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
            Camera: cameraAfter,
            State: _state,
            SingleTapDetected: singleTap,
            TapPosition: tapPos,
            DoubleTapDetected: doubleTap,
            DoubleTapPosition: doubleTapPos);
    }

    private void HandleDown(PointerPacket packet)
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
    }

    private void HandlePointerDown(PointerPacket packet)
    {
        // 1. Process movement of existing tracked pointers prior to new pointer addition
        ApplyIncrementalMove(packet, ignorePointerId: packet.ActionPointerId);

        // 2. Add new pointer
        var newSample = FindPointer(packet, packet.ActionPointerId);
        _activePointers[packet.ActionPointerId] = newSample.Position;

        // Cancel tap candidate on multi-touch
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
    }

    private void HandleMove(PointerPacket packet)
    {
        ApplyIncrementalMove(packet);
    }

    private void HandlePointerUp(PointerPacket packet)
    {
        // 1. Apply any final movement before removing pointer
        ApplyIncrementalMove(packet);

        // 2. Remove leaving pointer
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
    }

    private void HandleUp(
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

        var sample = FindPointer(packet, packet.ActionPointerId);

        if (_state == ViewportGestureState.TapCandidate)
        {
            var dist = Distance(sample.Position, _initialDownPosition);
            long duration = packet.EventTimeMs - _initialDownTimeMs;

            if (dist > _configuration.TouchSlopPx)
            {
                // Slop exceeded on final UP sample -> commit pan
                var dx = sample.Position.X - _initialDownPosition.X;
                var dy = sample.Position.Y - _initialDownPosition.Y;
                if (dx != 0 || dy != 0)
                {
                    _controller.Pan(dx, dy);
                }
                _hasDoubleTapCandidate = false;
            }
            else if (duration > _configuration.LongPressTimeoutMs)
            {
                // Long press past threshold -> neither single tap nor double tap
                _hasDoubleTapCandidate = false;
            }
            else
            {
                // Valid tap candidate within slop and time
                if (!_isMeasurementMode && _hasDoubleTapCandidate &&
                    (packet.EventTimeMs - _lastTapTimeMs <= _configuration.DoubleTapTimeoutMs) &&
                    Distance(sample.Position, _lastTapPosition) <= _configuration.DoubleTapSlopPx)
                {
                    doubleTap = true;
                    doubleTapPos = sample.Position;
                    _controller.DoubleTap(sample.Position, _configuration.DoubleTapZoomFactor);
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
            }
        }
        else if (_state == ViewportGestureState.Pinch)
        {
            ApplyIncrementalMove(packet);
        }

        _activePointers.Clear();
        _state = ViewportGestureState.Idle;
        _controller.EndInteraction();
        InteractionEnded?.Invoke();
    }

    private void HandleCancel()
    {
        _activePointers.Clear();
        _hasDoubleTapCandidate = false;
        _state = ViewportGestureState.Idle;
        _controller.EndInteraction();
        InteractionEnded?.Invoke();
    }

    private void ApplyIncrementalMove(PointerPacket packet, int? ignorePointerId = null)
    {
        // Check if pointer ID set matches active pointers (excluding any pointer marked ignorePointerId)
        if (ignorePointerId == null && packet.Pointers.Count > 0)
        {
            bool idsMatch = packet.Pointers.Count == _activePointers.Count;
            if (idsMatch)
            {
                foreach (var s in packet.Pointers)
                {
                    if (!_activePointers.ContainsKey(s.Id))
                    {
                        idsMatch = false;
                        break;
                    }
                }
            }

            if (!idsMatch)
            {
                // Pointer ID set changed: clear old pointers and establish fresh baseline
                _activePointers.Clear();
                foreach (var s in packet.Pointers)
                {
                    _activePointers[s.Id] = s.Position;
                }

                if (_activePointers.Count == 1)
                {
                    _state = ViewportGestureState.Pan;
                    _prevPanPoint = _activePointers.Values.First();
                }
                else if (_activePointers.Count == 2)
                {
                    _state = ViewportGestureState.Pinch;
                    EstablishPinchBaseline();
                }
                else if (_activePointers.Count >= 3)
                {
                    _state = ViewportGestureState.MultiTouchHold;
                }
                else
                {
                    _state = ViewportGestureState.Idle;
                    _controller.EndInteraction();
                    InteractionEnded?.Invoke();
                }
                return;
            }
        }

        // Update positions of tracked pointers
        foreach (var sample in packet.Pointers)
        {
            if (ignorePointerId.HasValue && sample.Id == ignorePointerId.Value) continue;

            if (_activePointers.ContainsKey(sample.Id))
            {
                _activePointers[sample.Id] = sample.Position;
            }
        }

        if (_state == ViewportGestureState.TapCandidate)
        {
            if (_activePointers.Count > 0)
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
                    }
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
            }
        }
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
