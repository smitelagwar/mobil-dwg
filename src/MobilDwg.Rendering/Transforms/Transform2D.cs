using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Transforms;

public readonly record struct Transform2D
{
    public double M11 { get; }
    public double M12 { get; }
    public double M13 { get; }
    public double M21 { get; }
    public double M22 { get; }
    public double M23 { get; }

    public Transform2D(double m11, double m12, double m13, double m21, double m22, double m23)
    {
        if (!double.IsFinite(m11) || !double.IsFinite(m12) || !double.IsFinite(m13) ||
            !double.IsFinite(m21) || !double.IsFinite(m22) || !double.IsFinite(m23))
        {
            throw new ArgumentOutOfRangeException(nameof(m11), "Matrix components must be finite.");
        }

        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
    }

    public static Transform2D Identity { get; } = new(1d, 0d, 0d, 0d, 1d, 0d);

    public static Transform2D CreateTranslation(double dx, double dy) =>
        new(1d, 0d, dx, 0d, 1d, dy);

    public static Transform2D CreateScale(double sx, double sy) =>
        new(sx, 0d, 0d, 0d, sy, 0d);

    public static Transform2D CreateRotation(double radians)
    {
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return new(cos, -sin, 0d, sin, cos, 0d);
    }

    public static Transform2D CreateBlockTransform(
        WorldPoint2 insertionPoint,
        double scaleX,
        double scaleY,
        double rotationRadians,
        WorldPoint2 basePoint = default)
    {
        // CAD block insertion transformation order:
        // P_world = T(insertionPoint) * R(rotation) * S(scaleX, scaleY) * T(-basePoint) * P_local
        var tBase = CreateTranslation(-basePoint.X, -basePoint.Y);
        var s = CreateScale(scaleX, scaleY);
        var r = CreateRotation(rotationRadians);
        var tInsert = CreateTranslation(insertionPoint.X, insertionPoint.Y);

        return tInsert * r * s * tBase;
    }

    public double Determinant => (M11 * M22) - (M12 * M21);

    public bool IsInverting => Determinant < 0d;

    public double ScaleX => Math.Sqrt((M11 * M11) + (M21 * M21));

    public double ScaleY => Math.Sqrt((M12 * M12) + (M22 * M22));

    public bool IsUniformScale(double tolerance = 1e-9) =>
        Math.Abs(ScaleX - ScaleY) <= tolerance;

    public WorldPoint2 TransformPoint(WorldPoint2 point) => new(
        (M11 * point.X) + (M12 * point.Y) + M13,
        (M21 * point.X) + (M22 * point.Y) + M23);

    public WorldPoint2 TransformVector(WorldPoint2 vector) => new(
        (M11 * vector.X) + (M12 * vector.Y),
        (M21 * vector.X) + (M22 * vector.Y));

    public static Transform2D Multiply(in Transform2D left, in Transform2D right) => new(
        (left.M11 * right.M11) + (left.M12 * right.M21),
        (left.M11 * right.M12) + (left.M12 * right.M22),
        (left.M11 * right.M13) + (left.M12 * right.M23) + left.M13,
        (left.M21 * right.M11) + (left.M22 * right.M21),
        (left.M21 * right.M12) + (left.M22 * right.M22),
        (left.M21 * right.M13) + (left.M22 * right.M23) + left.M23);

    public static Transform2D operator *(in Transform2D left, in Transform2D right) =>
        Multiply(left, right);

    public bool TryInverse(out Transform2D inverse)
    {
        var det = Determinant;
        if (Math.Abs(det) < 1e-15)
        {
            inverse = Identity;
            return false;
        }

        var invDet = 1d / det;
        inverse = new Transform2D(
            M22 * invDet,
            -M12 * invDet,
            ((M12 * M23) - (M22 * M13)) * invDet,
            -M21 * invDet,
            M11 * invDet,
            ((M21 * M13) - (M11 * M23)) * invDet);
        return true;
    }
}
