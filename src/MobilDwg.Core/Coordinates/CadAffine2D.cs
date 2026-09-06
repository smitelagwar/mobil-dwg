namespace MobilDwg.Core.Coordinates;

/// <summary>
/// Immutable 2D affine transformation matrix:
/// [ A   B   Tx ]
/// [ C   D   Ty ]
/// [ 0   0   1  ]
/// Mapping (x, y) -> (A*x + B*y + Tx, C*x + D*y + Ty).
/// </summary>
public readonly record struct CadAffine2D
{
    public double A { get; }
    public double B { get; }
    public double C { get; }
    public double D { get; }
    public double Tx { get; }
    public double Ty { get; }

    public CadAffine2D(double a, double b, double c, double d, double tx, double ty)
    {
        A = a;
        B = b;
        C = c;
        D = d;
        Tx = tx;
        Ty = ty;
    }

    public static readonly CadAffine2D Identity = new(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

    public static CadAffine2D Translation(double dx, double dy) =>
        new(1.0, 0.0, 0.0, 1.0, dx, dy);

    public static CadAffine2D Scale(double sx, double sy) =>
        new(sx, 0.0, 0.0, sy, 0.0, 0.0);

    public static CadAffine2D Rotation(double radians)
    {
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);
        return new(cos, -sin, sin, cos, 0.0, 0.0);
    }

    public double Determinant => (A * D) - (B * C);

    public bool IsMirrored => Determinant < 0.0;

    public double ScaleX => Math.Sqrt((A * A) + (C * C));

    public double ScaleY => Math.Sqrt((B * B) + (D * D));

    public double RotationAngle => Math.Atan2(C, A);

    public (double X, double Y) Transform(double x, double y) =>
        ((A * x) + (B * y) + Tx, (C * x) + (D * y) + Ty);

    public (double X, double Y) TransformVector(double vx, double vy) =>
        ((A * vx) + (B * vy), (C * vx) + (D * vy));

    /// <summary>
    /// Multiplies two affine transforms: (left * right)(p) = left(right(p)).
    /// </summary>
    public static CadAffine2D Multiply(in CadAffine2D left, in CadAffine2D right) => new(
        (left.A * right.A) + (left.B * right.C),
        (left.A * right.B) + (left.B * right.D),
        (left.C * right.A) + (left.D * right.C),
        (left.C * right.B) + (left.D * right.D),
        (left.A * right.Tx) + (left.B * right.Ty) + left.Tx,
        (left.C * right.Tx) + (left.D * right.Ty) + left.Ty);

    public static CadAffine2D operator *(in CadAffine2D left, in CadAffine2D right) =>
        Multiply(left, right);
}
