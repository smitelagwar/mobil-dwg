namespace MobilDwg.Rendering.Text;

public enum CadTextHorizontalAlignment
{
    Left = 0,
    Center = 1,
    Right = 2,
    Aligned = 3,
    Middle = 4,
    Fit = 5,
}

public enum CadTextVerticalAlignment
{
    Baseline = 0,
    Bottom = 1,
    Middle = 2,
    Top = 3,
}

public enum CadTextAttachmentPoint
{
    TopLeft = 1,
    TopCenter = 2,
    TopRight = 3,
    MiddleLeft = 4,
    MiddleCenter = 5,
    MiddleRight = 6,
    BottomLeft = 7,
    BottomCenter = 8,
    BottomRight = 9,
}

[Flags]
public enum CadTextMirrorFlags
{
    None = 0,
    Backward = 1,   // Mirrored in X
    UpsideDown = 2, // Mirrored in Y
}

public static class CadTextAlignmentHelper
{
    public static (CadTextHorizontalAlignment Horizontal, CadTextVerticalAlignment Vertical) FromAttachmentPoint(CadTextAttachmentPoint attachment) =>
        attachment switch
        {
            CadTextAttachmentPoint.TopLeft => (CadTextHorizontalAlignment.Left, CadTextVerticalAlignment.Top),
            CadTextAttachmentPoint.TopCenter => (CadTextHorizontalAlignment.Center, CadTextVerticalAlignment.Top),
            CadTextAttachmentPoint.TopRight => (CadTextHorizontalAlignment.Right, CadTextVerticalAlignment.Top),
            CadTextAttachmentPoint.MiddleLeft => (CadTextHorizontalAlignment.Left, CadTextVerticalAlignment.Middle),
            CadTextAttachmentPoint.MiddleCenter => (CadTextHorizontalAlignment.Center, CadTextVerticalAlignment.Middle),
            CadTextAttachmentPoint.MiddleRight => (CadTextHorizontalAlignment.Right, CadTextVerticalAlignment.Middle),
            CadTextAttachmentPoint.BottomLeft => (CadTextHorizontalAlignment.Left, CadTextVerticalAlignment.Bottom),
            CadTextAttachmentPoint.BottomCenter => (CadTextHorizontalAlignment.Center, CadTextVerticalAlignment.Bottom),
            CadTextAttachmentPoint.BottomRight => (CadTextHorizontalAlignment.Right, CadTextVerticalAlignment.Bottom),
            _ => (CadTextHorizontalAlignment.Left, CadTextVerticalAlignment.Baseline),
        };
}
