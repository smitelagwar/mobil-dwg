using System;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Interaction;
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

public sealed class CadViewportView : ContentView
{
    public delegate void ViewportPaintHandler(SKCanvas canvas, int pixelWidth, int pixelHeight, double density);

    private SKGLView? _glView;
    private SKCanvasView? _canvasView;
    private CadViewportBackend _backend = CadViewportBackend.OpenGLES;
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
            _session.InteractionEngine.CameraChanged -= OnCameraChanged;
#if ANDROID
            _inputAdapter?.Dispose();
            _inputAdapter = null;
#endif
        }

        _session = session;

        if (_session != null)
        {
            _session.InteractionEngine.CameraChanged += OnCameraChanged;
            AttachNativeInput();
            RequestFrame();
        }
    }

    private void OnCameraChanged(Camera2D camera)
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

    private void ArmWatchdog()
    {
        if (_backend != CadViewportBackend.OpenGLES || !IsVisible || Width <= 0 || Height <= 0) return;
        DisarmWatchdog();
        var cts = new System.Threading.CancellationTokenSource();
        _watchdogCts = cts;
        System.Threading.Tasks.Task.Delay(1000, cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled) return;
            Dispatcher.Dispatch(() =>
            {
                if (_backend == CadViewportBackend.OpenGLES && _watchdogRetries == 0)
                {
                    _watchdogRetries++;
                    InitializeGlView();
                    RequestFrame();
                }
                else if (_backend == CadViewportBackend.OpenGLES)
                {
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
        InvalidateViewport();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        AttachNativeInput();
    }

    private void AttachNativeInput()
    {
#if ANDROID
        if (_session == null) return;
        _inputAdapter?.Dispose();
        _inputAdapter = null;

        var platformView = Handler?.PlatformView as Android.Views.View;
        if (platformView != null)
        {
            _inputAdapter = new AndroidViewportInputAdapter(
                platformView,
                _session.InteractionEngine,
                () => (_lastWidth > 0 ? _lastWidth : (int)(Width * (platformView.Context?.Resources?.DisplayMetrics?.Density ?? 1.0f)),
                       _lastHeight > 0 ? _lastHeight : (int)(Height * (platformView.Context?.Resources?.DisplayMetrics?.Density ?? 1.0f))));
        }
#endif
    }

    private void InitializeGlView()
    {
        _glView = new SKGLView
        {
            IgnorePixelScaling = false,
            HasRenderLoop = false,
            EnableTouchEvents = false,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        _glView.PaintSurface += OnGlPaintSurface;
        Content = _glView;
        _backend = CadViewportBackend.OpenGLES;
        _surfaceGeneration++;
        _session?.FrameGate.InvalidateSurface(_surfaceGeneration);
    }

    public void SwitchToSoftware()
    {
        DisarmWatchdog();
        if (_backend == CadViewportBackend.Software) return;

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

    private void OnGlPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        DisarmWatchdog();
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
        catch (Exception)
        {
            SwitchToSoftware();
        }
    }

    private void OnCpuPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        DisarmWatchdog();
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

        if (_session == null || width <= 0 || height <= 0)
        {
            canvas.Clear(new SKColor(0x08, 0x0B, 0x11));
            return;
        }

        if (_session.ViewportPixelWidth != width || _session.ViewportPixelHeight != height)
        {
            _session.ResizeViewport(width, height);
        }

        var ticket = _session.FrameGate.TryBeginPaint(_surfaceGeneration);
        if (ticket == null)
        {
            return;
        }

        try
        {
            var quality = _session.InteractionEngine.State != ViewportGestureState.Idle
                ? RenderQualityMode.Interaction
                : RenderQualityMode.Final;

            using var lease = _session.AcquireRenderLease(_surfaceGeneration, quality);
            var context = new RenderFrameContext(width, height, density, quality);
            SkiaScenePainter.DrawFrame(canvas, lease.Snapshot, context);
        }
        finally
        {
            bool needNextFrame = _session.FrameGate.EndPaint(ticket);
            FramePresented?.Invoke(_surfaceGeneration);
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
