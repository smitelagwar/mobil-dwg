using System.Collections.ObjectModel;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.References;

public enum CadExternalReferenceKind
{
    DwgXref = 0,
    RasterImage = 1,
    PdfUnderlay = 2,
    DwfUnderlay = 3,
    DgnUnderlay = 4,
    PointCloud = 5,
    OleObject = 6
}

public sealed record CadExternalReference
{
    public CadExternalReference(
        string referenceId,
        CadExternalReferenceKind kind,
        string rawPath,
        string? resolvedPath,
        WorldPoint2 insertionPoint,
        double scaleX,
        double scaleY,
        double rotationRadians,
        double pixelWidth,
        double pixelHeight,
        WorldBounds2 bounds,
        bool isResolved,
        IEnumerable<WorldPoint2>? clipBoundary = null,
        double brightness = 50d,
        double contrast = 50d,
        double fade = 0d)
    {
        if (string.IsNullOrWhiteSpace(referenceId)) throw new ArgumentException("Reference ID is required.", nameof(referenceId));
        if (string.IsNullOrWhiteSpace(rawPath)) throw new ArgumentException("Raw path is required.", nameof(rawPath));

        ReferenceId = referenceId;
        Kind = kind;
        RawPath = rawPath;
        ResolvedPath = resolvedPath;
        InsertionPoint = insertionPoint;
        ScaleX = scaleX;
        ScaleY = scaleY;
        RotationRadians = rotationRadians;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        Bounds = bounds;
        IsResolved = isResolved;
        ClipBoundary = clipBoundary != null ? Array.AsReadOnly(clipBoundary.ToArray()) : null;
        Brightness = Math.Clamp(brightness, 0d, 100d);
        Contrast = Math.Clamp(contrast, 0d, 100d);
        Fade = Math.Clamp(fade, 0d, 100d);
    }

    public string ReferenceId { get; }
    public CadExternalReferenceKind Kind { get; }
    public string RawPath { get; }
    public string? ResolvedPath { get; }
    public WorldPoint2 InsertionPoint { get; }
    public double ScaleX { get; }
    public double ScaleY { get; }
    public double RotationRadians { get; }
    public double PixelWidth { get; }
    public double PixelHeight { get; }
    public WorldBounds2 Bounds { get; }
    public bool IsResolved { get; }
    public IReadOnlyList<WorldPoint2>? ClipBoundary { get; }
    public double Brightness { get; }
    public double Contrast { get; }
    public double Fade { get; }
}
