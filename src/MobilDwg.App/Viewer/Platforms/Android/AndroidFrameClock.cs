#if ANDROID
using System;
using Android.Views;

namespace MobilDwg.App.Viewer.Platforms.Android;

public sealed class AndroidFrameClock : IDisposable
{
    private readonly Action _onVsync;
    private readonly FrameCallbackHelper _callbackHelper;
    private bool _scheduled;
    private bool _disposed;

    public AndroidFrameClock(Action onVsync)
    {
        _onVsync = onVsync ?? throw new ArgumentNullException(nameof(onVsync));
        _callbackHelper = new FrameCallbackHelper(OnFrame);
    }

    public void RequestFrame()
    {
        if (_disposed || _scheduled) return;
        _scheduled = true;
        Choreographer.Instance?.PostFrameCallback(_callbackHelper);
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
        Choreographer.Instance?.RemoveFrameCallback(_callbackHelper);
    }

    private sealed class FrameCallbackHelper : Java.Lang.Object, Choreographer.IFrameCallback
    {
        private readonly Action<long> _action;
        public FrameCallbackHelper(Action<long> action) => _action = action;
        public void DoFrame(long frameTimeNanos) => _action(frameTimeNanos);
    }
}
#endif
