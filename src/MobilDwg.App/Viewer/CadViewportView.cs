using System;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

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
    private int _paintCount;
    private int _lastWidth;
    private int _lastHeight;

    public event ViewportPaintHandler? PaintFrameRequested;

    public CadViewportBackend CurrentBackend => _backend;
    public bool IsGlActive => _backend == CadViewportBackend.OpenGLES;
    public int PaintCount => _paintCount;
    public int LastSurfaceWidth => _lastWidth;
    public int LastSurfaceHeight => _lastHeight;

    public CadViewportView()
    {
        InitializeGlView();
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
    }

    public void SwitchToSoftware()
    {
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
    }

    public void SwitchToOpenGLES()
    {
        if (_backend == CadViewportBackend.OpenGLES) return;

        if (_canvasView != null)
        {
            _canvasView.PaintSurface -= OnCpuPaintSurface;
            _canvasView = null;
        }

        InitializeGlView();
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
        int width = e.BackendRenderTarget.Width;
        int height = e.BackendRenderTarget.Height;
        if (width <= 0 || height <= 0)
        {
            width = e.Info.Width;
            height = e.Info.Height;
        }

        _lastWidth = width;
        _lastHeight = height;
        _paintCount++;

        double density = Width > 0 ? width / Width : 1.0;
        if (density <= 0 || !double.IsFinite(density)) density = 1.0;

        PaintFrameRequested?.Invoke(e.Surface.Canvas, width, height, density);
    }

    private void OnCpuPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        int width = e.Info.Width;
        int height = e.Info.Height;

        _lastWidth = width;
        _lastHeight = height;
        _paintCount++;

        double density = Width > 0 ? width / Width : 1.0;
        if (density <= 0 || !double.IsFinite(density)) density = 1.0;

        PaintFrameRequested?.Invoke(e.Surface.Canvas, width, height, density);
    }
}
