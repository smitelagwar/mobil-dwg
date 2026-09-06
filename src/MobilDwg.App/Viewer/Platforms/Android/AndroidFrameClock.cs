#if ANDROID
using System;
using Android.Views;

namespace MobilDwg.App.Viewer.Platforms.Android;

public sealed class AndroidFrameClock : IDisposable
{
    private readonly Action _onVsync;
    private readonly FrameCallbackHelper _callbackHelper;
    private readonly global::Android.OS.Handler _mainHandler;
    private Choreographer? _choreographer;
    private bool _scheduled;
    private bool _paused;
    private bool _disposed;

    public bool IsPaused => _paused;

    public void Pause()
    {
        if (_disposed) return;
        _paused = true;
        if (global::Android.OS.Looper.MyLooper() == global::Android.OS.Looper.MainLooper)
        {
            _choreographer?.RemoveFrameCallback(_callbackHelper);
        }
        else
        {
            _mainHandler.Post(() => _choreographer?.RemoveFrameCallback(_callbackHelper));
        }
        _scheduled = false;
    }

    public void Resume()
    {
        if (_disposed) return;
        _paused = false;
    }

    public AndroidFrameClock(Action onVsync)
    {
        _onVsync = onVsync ?? throw new ArgumentNullException(nameof(onVsync));
        _callbackHelper = new FrameCallbackHelper(OnFrame);
        _mainHandler = new global::Android.OS.Handler(global::Android.OS.Looper.MainLooper!);

        if (global::Android.OS.Looper.MyLooper() == global::Android.OS.Looper.MainLooper)
        {
            _choreographer = Choreographer.Instance;
        }
        else
        {
            _mainHandler.Post(() =>
            {
                if (!_disposed)
                {
                    _choreographer = Choreographer.Instance;
                }
            });
        }
    }

    public void RequestFrame()
    {
        if (_disposed || _paused) return;

        if (global::Android.OS.Looper.MyLooper() == global::Android.OS.Looper.MainLooper)
        {
            RequestFrameOnMainThread();
        }
        else
        {
            _mainHandler.Post(RequestFrameOnMainThread);
        }
    }

    private void RequestFrameOnMainThread()
    {
        if (_disposed || _paused || _scheduled) return;
        _scheduled = true;

        _choreographer ??= Choreographer.Instance;
        if (_choreographer != null)
        {
            _choreographer.PostFrameCallback(_callbackHelper);
        }
        else
        {
            _mainHandler.Post(() => OnFrame(global::Android.OS.SystemClock.UptimeMillis() * 1_000_000L));
        }
    }

    private void OnFrame(long frameTimeNanos)
    {
        _scheduled = false;
        if (_disposed) return;
        _onVsync();
    }

    public void Dispose()
    {
        _disposed = true;
        if (global::Android.OS.Looper.MyLooper() == global::Android.OS.Looper.MainLooper)
        {
            _choreographer?.RemoveFrameCallback(_callbackHelper);
        }
        else
        {
            _mainHandler.Post(() => _choreographer?.RemoveFrameCallback(_callbackHelper));
        }
    }

    private sealed class FrameCallbackHelper : Java.Lang.Object, Choreographer.IFrameCallback
    {
        private readonly Action<long> _action;
        public FrameCallbackHelper(Action<long> action) => _action = action;
        public void DoFrame(long frameTimeNanos) => _action(frameTimeNanos);
    }
}
#endif
