using System.Collections.ObjectModel;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Transforms;

namespace MobilDwg.Rendering.References;

public sealed record MissingReferencePrimitive : RenderGeometryPrimitive
{
    public MissingReferencePrimitive(
        string referenceId,
        CadExternalReferenceKind kind,
        string rawPath,
        WorldBounds2 placeholderBounds,
        string diagnosticCode,
        string diagnosticMessage)
    {
        if (string.IsNullOrWhiteSpace(referenceId)) throw new ArgumentException("Reference ID is required.", nameof(referenceId));
        if (string.IsNullOrWhiteSpace(rawPath)) throw new ArgumentException("Raw path is required.", nameof(rawPath));

        ReferenceId = referenceId;
        Kind = kind;
        RawPath = rawPath;
        PlaceholderBounds = placeholderBounds;
        DiagnosticCode = diagnosticCode;
        DiagnosticMessage = diagnosticMessage;

        var fileName = Path.GetFileName(rawPath.Replace('\\', '/'));
        Label = $"[{kind.ToString().ToUpperInvariant()}: {fileName} - {diagnosticCode}]";
    }

    public string ReferenceId { get; }
    public CadExternalReferenceKind Kind { get; }
    public string RawPath { get; }
    public WorldBounds2 PlaceholderBounds { get; }
    public string DiagnosticCode { get; }
    public string DiagnosticMessage { get; }
    public string Label { get; }

    public override WorldBounds2 Bounds => PlaceholderBounds;

    public IReadOnlyList<LinePrimitive> GenerateBorderLines()
    {
        var b = PlaceholderBounds;
        return
        [
            new LinePrimitive(new WorldPoint2(b.MinX, b.MinY), new WorldPoint2(b.MaxX, b.MinY)),
            new LinePrimitive(new WorldPoint2(b.MaxX, b.MinY), new WorldPoint2(b.MaxX, b.MaxY)),
            new LinePrimitive(new WorldPoint2(b.MaxX, b.MaxY), new WorldPoint2(b.MinX, b.MaxY)),
            new LinePrimitive(new WorldPoint2(b.MinX, b.MaxY), new WorldPoint2(b.MinX, b.MinY))
        ];
    }

    public IReadOnlyList<LinePrimitive> GenerateCrossLines()
    {
        var b = PlaceholderBounds;
        return
        [
            new LinePrimitive(new WorldPoint2(b.MinX, b.MinY), new WorldPoint2(b.MaxX, b.MaxY)),
            new LinePrimitive(new WorldPoint2(b.MinX, b.MaxY), new WorldPoint2(b.MaxX, b.MinY))
        ];
    }
}

public sealed record RasterImagePrimitive : RenderGeometryPrimitive
{
    private readonly ReadOnlyCollection<WorldPoint2>? _clipBoundary;

    public RasterImagePrimitive(
        string referenceId,
        string? resolvedPath,
        byte[]? imageBytes,
        WorldBounds2 imageBounds,
        Transform2D transform,
        int pixelWidth,
        int pixelHeight,
        IEnumerable<WorldPoint2>? clipBoundary = null,
        double brightness = 50d,
        double contrast = 50d,
        double fade = 0d)
    {
        if (string.IsNullOrWhiteSpace(referenceId)) throw new ArgumentException("Reference ID is required.", nameof(referenceId));
        if (resolvedPath == null && (imageBytes == null || imageBytes.Length == 0))
        {
            throw new ArgumentException("Either resolved file path or non-empty image bytes must be provided.");
        }

        ReferenceId = referenceId;
        ResolvedPath = resolvedPath;
        ImageBytes = imageBytes;
        ImageBounds = imageBounds;
        Transform = transform;
        PixelWidth = pixelWidth > 0 ? pixelWidth : 1;
        PixelHeight = pixelHeight > 0 ? pixelHeight : 1;
        _clipBoundary = clipBoundary != null ? Array.AsReadOnly(clipBoundary.ToArray()) : null;
        Brightness = Math.Clamp(brightness, 0d, 100d);
        Contrast = Math.Clamp(contrast, 0d, 100d);
        Fade = Math.Clamp(fade, 0d, 100d);
    }

    public string ReferenceId { get; }
    public string? ResolvedPath { get; }
    public byte[]? ImageBytes { get; }
    public WorldBounds2 ImageBounds { get; }
    public Transform2D Transform { get; }
    public int PixelWidth { get; }
    public int PixelHeight { get; }
    public IReadOnlyList<WorldPoint2>? ClipBoundary => _clipBoundary;
    public double Brightness { get; }
    public double Contrast { get; }
    public double Fade { get; }

    public override WorldBounds2 Bounds => ImageBounds;

    public static byte[] CreateTestPng(int width = 64, int height = 64)
    {
        using var bitmap = new SkiaSharp.SKBitmap(width, height, SkiaSharp.SKColorType.Rgba8888, SkiaSharp.SKAlphaType.Premul);
        using var canvas = new SkiaSharp.SKCanvas(bitmap);
        canvas.Clear(SkiaSharp.SKColors.MidnightBlue);

        using var paintGold = new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.Gold };
        using var paintCyan = new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.Cyan };

        var step = Math.Max(1, width / 4);
        for (var x = 0; x < width; x += step)
        {
            for (var y = 0; y < height; y += step)
            {
                if (((x / step) + (y / step)) % 2 == 0)
                {
                    canvas.DrawRect(x, y, step, step, paintGold);
                }
                else
                {
                    canvas.DrawRect(x, y, step, step, paintCyan);
                }
            }
        }

        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}

