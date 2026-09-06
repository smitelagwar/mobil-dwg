using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Interaction;
using MobilDwg.Rendering.Performance;
using MobilDwg.Rendering.Scheduling;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Viewer;
#if ANDROID
using MobilDwg.App.Viewer.Platforms.Android;
#endif

namespace MobilDwg.App.Viewer;

public enum CadViewportBackend
{
    OpenGLES,
    Software
}

public enum ViewportLifecycleState
{
    Detached,
    Attached,
    Resumed,
    Paused
}

public sealed class CadViewportView : ContentView
{
    public delegate void ViewportPaintHandler(SKCanvas canvas, int pixelWidth, int pixelHeight, double density);

    private SKGLView? _glView;
    private SKCanvasView? _canvasView;
    private CadViewportBackend _backend = CadViewportBackend.OpenGLES;
    private ViewportLifecycleState _lifecycleState = ViewportLifecycleState.Detached;
    private CadViewerSession? _session;
    private long _surfaceGeneration = 1;
    private int _paintCount;
    private int _lastWidth;
    private int _lastHeight;

#if ANDROID
    private AndroidViewportInputAdapter? _inputAdapter;
    private AndroidFrameClock? _frameClock;
#endif
    private System.Threading.CancellationTokenSource? _watchdogCts;
    private int _watchdogRetries;

    public event ViewportPaintHandler? PaintFrameRequested;
    public event Action<long>? FramePresented;

    public CadViewportBackend CurrentBackend => _backend;
    public ViewportLifecycleState LifecycleState => _lifecycleState;
    public bool IsAttached => _lifecycleState != ViewportLifecycleState.Detached;
    public bool IsResumed => _lifecycleState == ViewportLifecycleState.Resumed;
    public bool IsGlActive => _backend == CadViewportBackend.OpenGLES;
    public int PaintCount => _paintCount;
    public int LastSurfaceWidth => _lastWidth;
    public int LastSurfaceHeight => _lastHeight;
    public CadViewerSession? Session => _session;

    public CadViewportView()
    {
        InitializeGlView();
#if ANDROID
        _frameClock = new AndroidFrameClock(OnVsyncTick);
#endif
    }

    public void BindSession(CadViewerSession? session)
    {
        if (_session != null)
        {
            _session.FrameInvalidated -= OnFrameInvalidated;
#if ANDROID
            _inputAdapter?.CancelAndDetach();
            _inputAdapter = null;
#endif
        }

        _session = session;

        if (_session != null)
        {
            _session.FrameGate.InvalidateSurface(_surfaceGeneration);
            _session.FrameInvalidated += OnFrameInvalidated;
            AttachNativeInput();
            RequestFrame();
        }
    }

    private void OnFrameInvalidated(string reason)
    {
        RequestFrame();
    }

    public void RequestFrame()
    {
        if (_session?.FrameGate.RequestFrame() == true)
        {
            ArmWatchdog();
#if ANDROID
            _frameClock?.RequestFrame();
#else
            InvalidateViewport();
#endif
        }
    }

    public void OnHostPause()
    {
        _lifecycleState = ViewportLifecycleState.Paused;
        DisarmWatchdog();

#if ANDROID
        _inputAdapter?.CancelCurrentGesture();
        _frameClock?.Pause();
#endif
        _session?.InteractionEngine.CancelGesture();
    }

    public void OnHostResume()
    {
        _lifecycleState = ViewportLifecycleState.Resumed;

#if ANDROID
        _frameClock?.Resume();
        AttachNativeInput();
#endif

        if (_session != null)
        {
            _surfaceGeneration++;
            _session.FrameGate.InvalidateSurface(_surfaceGeneration);
            RequestFrame();
        }
    }

    private void ArmWatchdog()
    {
        if (_backend != CadViewportBackend.OpenGLES) return;
        if (!IsVisible || _lifecycleState != ViewportLifecycleState.Resumed) return;
        if (Width <= 0 || Height <= 0) return;
        if (_session == null) return;
        if (_session.InteractionEngine.State != ViewportGestureState.Idle) return;

        DisarmWatchdog();

        var targetGen = _surfaceGeneration;
        var targetSession = _session;
        var cts = new System.Threading.CancellationTokenSource();
        _watchdogCts = cts;

        System.Threading.Tasks.Task.Delay(1000, cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled) return;
            Dispatcher.Dispatch(() =>
            {
                if (_watchdogCts?.Token != cts.Token) return;
                if (_backend != CadViewportBackend.OpenGLES) return;
                if (!IsVisible || _lifecycleState != ViewportLifecycleState.Resumed) return;
                if (Width <= 0 || Height <= 0) return;
                if (_session != targetSession || _session == null) return;
                if (_surfaceGeneration != targetGen) return;
                if (_session.InteractionEngine.State != ViewportGestureState.Idle) return;

                if (!_session.FrameGate.IsFrameAwaitingOrScheduled && !_session.FrameGate.HasActiveTicket) return;

                if (_watchdogRetries == 0)
                {
                    _watchdogRetries++;
#if ANDROID
                    Android.Util.Log.Warn("MobilDwgCAD", $"Watchdog timeout 1 (gen={targetGen}): Re-initializing GL view.");
#endif
                    InitializeGlView();
                    RequestFrame();
                }
                else
                {
#if ANDROID
                    Android.Util.Log.Error("MobilDwgCAD", $"Watchdog timeout 2 (gen={targetGen}): Falling back to CPU software rendering.");
#endif
                    SwitchToSoftware();
                }
            });
        }, System.Threading.Tasks.TaskScheduler.Default);
    }

    private void DisarmWatchdog()
    {
        _watchdogCts?.Cancel();
        _watchdogCts?.Dispose();
        _watchdogCts = null;
    }

    private void OnVsyncTick()
    {
        _session?.FrameGate.MarkAwaitingPaint();
        InvalidateViewport();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
        {
            _lifecycleState = ViewportLifecycleState.Attached;
            AttachNativeInput();
#if ANDROID
            MainActivity.HostPaused -= OnHostPause;
            MainActivity.HostPaused += OnHostPause;
            MainActivity.HostResumed -= OnHostResume;
            MainActivity.HostResumed += OnHostResume;
#endif
            OnHostResume();
        }
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.NewHandler == null)
        {
            _lifecycleState = ViewportLifecycleState.Detached;
            OnHostPause();
#if ANDROID
            MainActivity.HostPaused -= OnHostPause;
            MainActivity.HostResumed -= OnHostResume;
            _inputAdapter?.CancelAndDetach();
            _inputAdapter = null;
#endif
        }
    }

    private void AttachNativeInput()
    {
#if ANDROID
        if (_session == null) return;
        _inputAdapter?.CancelAndDetach();
        _inputAdapter = null;

        Android.Views.View? activeTarget = null;
        if (_backend == CadViewportBackend.OpenGLES && _glView != null)
        {
            activeTarget = _glView.Handler?.PlatformView as Android.Views.View;
        }
        else if (_backend == CadViewportBackend.Software && _canvasView != null)
        {
            activeTarget = _canvasView.Handler?.PlatformView as Android.Views.View;
        }

        if (activeTarget == null)
        {
            activeTarget = Handler?.PlatformView as Android.Views.View;
        }

        if (activeTarget != null)
        {
            _inputAdapter = new AndroidViewportInputAdapter(
                activeTarget,
                _session.InteractionEngine,
                () => (_lastWidth > 0 ? _lastWidth : (int)(Width * (activeTarget.Context?.Resources?.DisplayMetrics?.Density ?? 1.0f)),
                       _lastHeight > 0 ? _lastHeight : (int)(Height * (activeTarget.Context?.Resources?.DisplayMetrics?.Density ?? 1.0f))),
                () => _surfaceGeneration);
        }
#endif
    }

    private void InitializeGlView()
    {
        if (_glView != null)
        {
            _glView.PaintSurface -= OnGlPaintSurface;
            _glView = null;
        }

        _glView = new SKGLView
        {
            IgnorePixelScaling = false,
            HasRenderLoop = false,
            EnableTouchEvents = false,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        _glView.PaintSurface += OnGlPaintSurface;
        _glView.HandlerChanged += (s, e) => AttachNativeInput();
        Content = _glView;
        _backend = CadViewportBackend.OpenGLES;
        _surfaceGeneration++;
        _session?.FrameGate.InvalidateSurface(_surfaceGeneration);
    }

    public void SwitchToSoftware()
    {
        DisarmWatchdog();
        if (_backend == CadViewportBackend.Software) return;

#if ANDROID
        _inputAdapter?.CancelAndDetach();
        _inputAdapter = null;
#endif

        if (_glView != null)
        {
            _glView.PaintSurface -= OnGlPaintSurface;
            _glView = null;
        }

        _canvasView = new SKCanvasView
        {
            IgnorePixelScaling = false,
            EnableTouchEvents = false,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        _canvasView.PaintSurface += OnCpuPaintSurface;
        _canvasView.HandlerChanged += (s, e) => AttachNativeInput();
        Content = _canvasView;
        _backend = CadViewportBackend.Software;
        _surfaceGeneration++;
        _session?.FrameGate.InvalidateSurface(_surfaceGeneration);
        AttachNativeInput();
        RequestFrame();
    }

    public void SwitchToOpenGLES()
    {
        if (_backend == CadViewportBackend.OpenGLES) return;

#if ANDROID
        _inputAdapter?.CancelAndDetach();
        _inputAdapter = null;
#endif

        if (_canvasView != null)
        {
            _canvasView.PaintSurface -= OnCpuPaintSurface;
            _canvasView = null;
        }

        _watchdogRetries = 0;
        InitializeGlView();
        AttachNativeInput();
        RequestFrame();
    }

    public void InvalidateViewport()
    {
        if (_backend == CadViewportBackend.OpenGLES)
        {
            _glView?.InvalidateSurface();
        }
        else
        {
            _canvasView?.InvalidateSurface();
        }
    }

    private static bool IsGlBackendException(Exception ex)
    {
        if (ex.GetType().FullName?.Contains("Skia", StringComparison.OrdinalIgnoreCase) == true) return true;
        var msg = ex.Message ?? string.Empty;
        return msg.Contains("GL", StringComparison.OrdinalIgnoreCase) ||
               msg.Contains("context", StringComparison.OrdinalIgnoreCase) ||
               msg.Contains("GrContext", StringComparison.OrdinalIgnoreCase) ||
               msg.Contains("surface", StringComparison.OrdinalIgnoreCase) ||
               msg.Contains("render target", StringComparison.OrdinalIgnoreCase) ||
               msg.Contains("EGL", StringComparison.OrdinalIgnoreCase);
    }

    private void OnGlPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        if (e.Surface == null || e.Surface.Canvas == null)
        {
#if ANDROID
            Android.Util.Log.Error("MobilDwgCAD", "GL surface or canvas is null! Backend lost. Falling back to software renderer.");
#endif
            Dispatcher.Dispatch(() => SwitchToSoftware());
            return;
        }

        try
        {
            int width = e.BackendRenderTarget.Width;
            int height = e.BackendRenderTarget.Height;
            if (width <= 0 || height <= 0)
            {
                width = e.Info.Width;
                height = e.Info.Height;
            }

            RenderFrameCore(e.Surface.Canvas, width, height);
        }
        catch (Exception ex) when (IsGlBackendException(ex))
        {
#if ANDROID
            Android.Util.Log.Error("MobilDwgCAD", $"GL backend exception: {ex.Message}. Falling back to software renderer.");
#endif
            Dispatcher.Dispatch(() => SwitchToSoftware());
        }
    }

    private void OnCpuPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (e.Surface == null || e.Surface.Canvas == null) return;
        int width = e.Info.Width;
        int height = e.Info.Height;

        RenderFrameCore(e.Surface.Canvas, width, height);
    }

    private void RenderFrameCore(SKCanvas canvas, int width, int height)
    {
        _lastWidth = width;
        _lastHeight = height;
        _paintCount++;

        double density = Width > 0 ? width / Width : 1.0;
        if (density <= 0 || !double.IsFinite(density)) density = 1.0;

        PaintFrameRequested?.Invoke(canvas, width, height, density);

        var session = _session;
        var surfaceGen = _surfaceGeneration;

        if (session == null || width <= 0 || height <= 0)
        {
            canvas.Clear(new SKColor(0x08, 0x0B, 0x11));
            return;
        }

        if (session.ViewportPixelWidth != width || session.ViewportPixelHeight != height)
        {
            session.ResizeViewport(width, height);
        }

        var gate = session.FrameGate;
        FrameTicket? ticket = null;
        RenderSessionLease? lease = null;
        bool frameRendered = false;

        try
        {
            ticket = gate.TryBeginPaint(surfaceGen);
            if (ticket == null)
            {
                // Surface generation obsolete or concurrent ticket; drop safely without disarming watchdog
                return;
            }

            var quality = session.InteractionEngine.State != ViewportGestureState.Idle
                ? RenderQualityMode.Interaction
                : RenderQualityMode.Final;

            lease = session.AcquireRenderLease(surfaceGen, quality);
            var context = new RenderFrameContext(width, height, density, quality);

#if ANDROID
            ViewportTelemetry.Instance.UpdateClockCalibration(Android.OS.SystemClock.UptimeMillis(), Stopwatch.GetTimestamp());
#else
            ViewportTelemetry.Instance.UpdateClockCalibration(Environment.TickCount64, Stopwatch.GetTimestamp());
#endif

            long paintStartTicks = Stopwatch.GetTimestamp();
            SkiaScenePainter.DrawFrame(canvas, lease.Snapshot, context);
            long paintEndTicks = Stopwatch.GetTimestamp();
            frameRendered = true;

            try
            {
                ViewportTelemetry.Instance.Record(
                    inputEventTimeMs: session.InteractionEngine.LastInputEventTimeMs,
                    cameraRevision: session.CameraRevision,
                    frameRequestTicks: gate.LastRequestTicks,
                    paintStartTicks: paintStartTicks,
                    paintEndTicks: paintEndTicks,
                    sceneBuildTicks: 0,
                    indexQueryTicks: 0,
                    entityCount: session.Scene.Entities.Count,
                    primitiveCount: (int)Math.Min(int.MaxValue, session.GeometryCache.TessellationCount),
                    vertexCount: (int)Math.Min(int.MaxValue, session.GeometryCache.TessellationCount * 8),
                    backend: _backend == CadViewportBackend.OpenGLES ? "GL" : "Software",
                    cacheHitCount: (int)Math.Min(int.MaxValue, session.GeometryCache.CacheHits),
                    cacheMissCount: (int)Math.Min(int.MaxValue, session.GeometryCache.CacheMisses),
                    cacheBytes: session.GeometryCache.CurrentSizeBytes + session.ResourceCache.CurrentSizeBytes);
            }
            catch
            {
                // Telemetry buffer exception guard
            }
        }
        catch (ObjectDisposedException)
        {
            // Session closed/retiring while paint started; safe drop
        }
        finally
        {
            lease?.Dispose();
            if (ticket != null)
            {
                bool needNextFrame = gate.EndPaint(ticket);
                if (frameRendered)
                {
                    DisarmWatchdog();
                    FramePresented?.Invoke(surfaceGen);
                }
                if (needNextFrame)
                {
#if ANDROID
                    _frameClock?.RequestFrame();
#else
                    InvalidateViewport();
#endif
                }
            }
        }
    }
}
