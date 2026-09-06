#if ANDROID
using System;
using System.Collections.Generic;
using Android.Views;
using AndroidView = Android.Views.View;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Interaction;

namespace MobilDwg.App.Viewer.Platforms.Android;

public sealed class AndroidViewportInputAdapter : IDisposable
{
    private readonly AndroidView _nativeView;
    private readonly ViewportInteractionEngine _engine;
    private readonly Func<(int Width, int Height)> _surfaceSizeProvider;
    private readonly Func<long>? _surfaceGenerationProvider;
    private bool _disposed;

    public AndroidViewportInputAdapter(
        AndroidView nativeView,
        ViewportInteractionEngine engine,
        Func<(int Width, int Height)> surfaceSizeProvider,
        Func<long>? surfaceGenerationProvider = null)
    {
        _nativeView = nativeView ?? throw new ArgumentNullException(nameof(nativeView));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _surfaceSizeProvider = surfaceSizeProvider ?? throw new ArgumentNullException(nameof(surfaceSizeProvider));
        _surfaceGenerationProvider = surfaceGenerationProvider;

        // Apply Android ViewConfiguration values to engine configuration
        if (_nativeView.Context != null)
        {
            var vc = ViewConfiguration.Get(_nativeView.Context);
            if (vc != null)
            {
                _engine.Configuration = _engine.Configuration with
                {
                    TouchSlopPx = vc.ScaledTouchSlop,
                    DoubleTapSlopPx = vc.ScaledDoubleTapSlop,
                    DoubleTapTimeoutMs = ViewConfiguration.DoubleTapTimeout,
                    TapTimeoutMs = ViewConfiguration.TapTimeout,
                    LongPressTimeoutMs = ViewConfiguration.LongPressTimeout
                };
            }
        }

        _nativeView.Touch += OnTouch;
        _nativeView.ViewDetachedFromWindow += OnDetachedFromWindow;
        _nativeView.FocusChange += OnFocusChange;
    }

    public AndroidView NativeView => _nativeView;
    public ViewportInteractionEngine Engine => _engine;

    private void OnDetachedFromWindow(object? sender, AndroidView.ViewDetachedFromWindowEventArgs e)
    {
        _engine.CancelGesture();
    }

    private void OnFocusChange(object? sender, AndroidView.FocusChangeEventArgs e)
    {
        if (!e.HasFocus)
        {
            _engine.CancelGesture();
        }
    }

    private void OnTouch(object? sender, AndroidView.TouchEventArgs e)
    {
        if (_disposed)
        {
            e.Handled = false;
            return;
        }

        var motionEvent = e.Event;
        if (motionEvent == null)
        {
            e.Handled = false;
            return;
        }

        // Always claim ownership on active viewer from DOWN
        e.Handled = true;

        var actionMasked = motionEvent.ActionMasked;
        int actionIndex = motionEvent.ActionIndex;
        int actionPointerId = motionEvent.GetPointerId(actionIndex);

        if (actionMasked == MotionEventActions.Down)
        {
            _nativeView.Parent?.RequestDisallowInterceptTouchEvent(true);
        }
        else if (actionMasked == MotionEventActions.Up || actionMasked == MotionEventActions.Cancel)
        {
            _nativeView.Parent?.RequestDisallowInterceptTouchEvent(false);
        }

        var (surfaceW, surfaceH) = _surfaceSizeProvider();
        // Native coordinates: _nativeView.Width and Height are in native pixels.
        // Surface physical pixels are (surfaceW, surfaceH).
        // Only scale if the native view dimensions differ from the surface pixel dimensions.
        // NEVER multiply by DisplayMetrics.Density twice!
        double scaleX = _nativeView.Width > 0 && surfaceW > 0 ? (double)surfaceW / _nativeView.Width : 1.0;
        double scaleY = _nativeView.Height > 0 && surfaceH > 0 ? (double)surfaceH / _nativeView.Height : 1.0;

        long currentGen = _surfaceGenerationProvider?.Invoke() ?? 0L;

        // Process historical events if available for high-frequency precision
        int historySize = motionEvent.HistorySize;
        if (historySize > 0 && actionMasked == MotionEventActions.Move)
        {
            for (int h = 0; h < historySize; h++)
            {
                long histTime = motionEvent.GetHistoricalEventTime(h);
                var histPointers = new List<PointerSample>(motionEvent.PointerCount);
                for (int p = 0; p < motionEvent.PointerCount; p++)
                {
                    int pId = motionEvent.GetPointerId(p);
                    double px = motionEvent.GetHistoricalX(p, h) * scaleX;
                    double py = motionEvent.GetHistoricalY(p, h) * scaleY;
                    histPointers.Add(new PointerSample(pId, new ScreenPoint2(px, py)));
                }

                var histPacket = new PointerPacket(
                    PointerAction.Move,
                    actionPointerId,
                    actionIndex,
                    histTime,
                    histPointers,
                    currentGen);
                _engine.ProcessPacket(histPacket);
            }
        }

        // Current event sample
        var pointers = new List<PointerSample>(motionEvent.PointerCount);
        for (int p = 0; p < motionEvent.PointerCount; p++)
        {
            int pId = motionEvent.GetPointerId(p);
            double px = motionEvent.GetX(p) * scaleX;
            double py = motionEvent.GetY(p) * scaleY;
            pointers.Add(new PointerSample(pId, new ScreenPoint2(px, py)));
        }

        PointerAction action = actionMasked switch
        {
            MotionEventActions.Down => PointerAction.Down,
            MotionEventActions.PointerDown => PointerAction.PointerDown,
            MotionEventActions.Move => PointerAction.Move,
            MotionEventActions.PointerUp => PointerAction.PointerUp,
            MotionEventActions.Up => PointerAction.Up,
            MotionEventActions.Cancel => PointerAction.Cancel,
            _ => PointerAction.Move
        };

        var packet = new PointerPacket(
            action,
            actionPointerId,
            actionIndex,
            motionEvent.EventTime,
            pointers,
            currentGen);

        _engine.ProcessPacket(packet);
    }

    public void CancelCurrentGesture()
    {
        _engine.CancelGesture();
    }

    public void CancelAndDetach()
    {
        _engine.CancelGesture();
        Dispose();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _nativeView.Touch -= OnTouch;
            _nativeView.ViewDetachedFromWindow -= OnDetachedFromWindow;
            _nativeView.FocusChange -= OnFocusChange;
            _nativeView.Parent?.RequestDisallowInterceptTouchEvent(false);
            _disposed = true;
        }
    }
}
#endif
