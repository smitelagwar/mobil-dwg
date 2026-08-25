using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Coordinates;

public readonly record struct Vector3D
{
    public Vector3D(double x, double y, double z)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        if (!double.IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(z));
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public double Length
    {
        get
        {
            var scale = Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z)));
            if (scale == 0d) return 0d;
            var sx = X / scale;
            var sy = Y / scale;
            var sz = Z / scale;
            return scale * Math.Sqrt((sx * sx) + (sy * sy) + (sz * sz));
        }
    }

    public Vector3D Normalize()
    {
        // Scale first so a direction made of very large finite components does not
        // overflow merely while computing its Euclidean norm.
        var scale = Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z)));
        if (!double.IsFinite(scale) || scale <= 0d) throw new InvalidOperationException("Cannot normalize zero/invalid vector.");

        var sx = X / scale;
        var sy = Y / scale;
        var sz = Z / scale;
        var scaledLength = Math.Sqrt((sx * sx) + (sy * sy) + (sz * sz));
        if (!double.IsFinite(scaledLength) || scaledLength <= 0d) throw new InvalidOperationException("Cannot normalize zero/invalid vector.");

        return new Vector3D(sx / scaledLength, sy / scaledLength, sz / scaledLength);
    }

    public static double Dot(Vector3D left, Vector3D right) =>
        (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

    public static Vector3D Cross(Vector3D left, Vector3D right) => new(
        (left.Y * right.Z) - (left.Z * right.Y),
        (left.Z * right.X) - (left.X * right.Z),
        (left.X * right.Y) - (left.Y * right.X));
}

public sealed class OcsCoordinateSystem
{
    private static readonly Vector3D UnitY = new(0, 1, 0);
    private static readonly Vector3D UnitZ = new(0, 0, 1);

    public OcsCoordinateSystem(Vector3D extrusionNormal)
    {
        Normal = extrusionNormal.Normalize();

        // AutoCAD arbitrary-axis algorithm threshold. The reference axis avoids
        // a near-parallel cross product when the normal is close to world Z.
        var reference = Math.Abs(Normal.X) < (1d / 64d) && Math.Abs(Normal.Y) < (1d / 64d)
            ? UnitY
            : UnitZ;

        AxisX = Vector3D.Cross(reference, Normal).Normalize();
        AxisY = Vector3D.Cross(Normal, AxisX).Normalize();
    }

    public Vector3D AxisX { get; }
    public Vector3D AxisY { get; }
    public Vector3D Normal { get; }

    public WorldPoint3 OcsToWcs(WorldPoint3 point) => new(
        (AxisX.X * point.X) + (AxisY.X * point.Y) + (Normal.X * point.Z),
        (AxisX.Y * point.X) + (AxisY.Y * point.Y) + (Normal.Y * point.Z),
        (AxisX.Z * point.X) + (AxisY.Z * point.Y) + (Normal.Z * point.Z));

    public WorldPoint3 WcsToOcs(WorldPoint3 point)
    {
        var vector = new Vector3D(point.X, point.Y, point.Z);
        return new WorldPoint3(
            Vector3D.Dot(vector, AxisX),
            Vector3D.Dot(vector, AxisY),
            Vector3D.Dot(vector, Normal));
    }
}
