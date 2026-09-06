namespace MobilDwg.Core.Coordinates;

/// <summary>
/// Implements the AutoCAD Object Coordinate System (OCS) Arbitrary Axis Algorithm.
/// Translates entity coordinates defined relative to an arbitrary Normal vector (extrusion direction)
/// into World Coordinate System (WCS) coordinates.
/// Reference: Autodesk DXF Reference - Arbitrary Axis Algorithm.
/// </summary>
public readonly record struct OcsTransform
{
    public double AxX { get; }
    public double AxY { get; }
    public double AxZ { get; }

    public double AyX { get; }
    public double AyY { get; }
    public double AyZ { get; }

    public double AzX { get; }
    public double AzY { get; }
    public double AzZ { get; }

    public bool IsIdentity { get; }

    public static readonly OcsTransform Identity = new(1, 0, 0, 0, 1, 0, 0, 0, 1, true);

    private OcsTransform(
        double axX, double axY, double axZ,
        double ayX, double ayY, double ayZ,
        double azX, double azY, double azZ,
        bool isIdentity)
    {
        AxX = axX; AxY = axY; AxZ = axZ;
        AyX = ayX; AyY = ayY; AyZ = ayZ;
        AzX = azX; AzY = azY; AzZ = azZ;
        IsIdentity = isIdentity;
    }

    public static OcsTransform FromNormal(double nx, double ny, double nz)
    {
        double lenSq = nx * nx + ny * ny + nz * nz;
        if (lenSq < 1e-12)
        {
            return Identity;
        }

        double invLen = 1.0 / Math.Sqrt(lenSq);
        nx *= invLen;
        ny *= invLen;
        nz *= invLen;

        // If normal is essentially (0, 0, 1), identity
        if (Math.Abs(nx) < 1e-7 && Math.Abs(ny) < 1e-7 && Math.Abs(nz - 1.0) < 1e-7)
        {
            return Identity;
        }

        // Arbitrary Axis Algorithm:
        // If (abs(Nx) < 1/64) and (abs(Ny) < 1/64), then Ax = (0, 1, 0) x N (normalized)
        // Otherwise, Ax = (0, 0, 1) x N (normalized)
        const double threshold = 1.0 / 64.0;
        double axX, axY, axZ;

        if (Math.Abs(nx) < threshold && Math.Abs(ny) < threshold)
        {
            // (0, 1, 0) x (nx, ny, nz) = (nz, 0, -nx)
            axX = nz;
            axY = 0;
            axZ = -nx;
        }
        else
        {
            // (0, 0, 1) x (nx, ny, nz) = (-ny, nx, 0)
            axX = -ny;
            axY = nx;
            axZ = 0;
        }

        double axLen = Math.Sqrt(axX * axX + axY * axY + axZ * axZ);
        if (axLen > 1e-12)
        {
            double invAx = 1.0 / axLen;
            axX *= invAx;
            axY *= invAx;
            axZ *= invAx;
        }
        else
        {
            axX = 1; axY = 0; axZ = 0;
        }

        // Ay = N x Ax = (ny*axZ - nz*axY, nz*axX - nx*axZ, nx*axY - ny*axX)
        double ayX = ny * axZ - nz * axY;
        double ayY = nz * axX - nx * axZ;
        double ayZ = nx * axY - ny * axX;
        double ayLen = Math.Sqrt(ayX * ayX + ayY * ayY + ayZ * ayZ);
        if (ayLen > 1e-12)
        {
            double invAy = 1.0 / ayLen;
            ayX *= invAy;
            ayY *= invAy;
            ayZ *= invAy;
        }
        else
        {
            ayX = 0; ayY = 1; ayZ = 0;
        }

        return new OcsTransform(axX, axY, axZ, ayX, ayY, ayZ, nx, ny, nz, false);
    }

    public (double X, double Y, double Z) Transform(double x, double y, double z = 0.0)
    {
        if (IsIdentity)
        {
            return (x, y, z);
        }

        return (
            x * AxX + y * AyX + z * AzX,
            x * AxY + y * AyY + z * AzY,
            x * AxZ + y * AyZ + z * AzZ
        );
    }

    public (double X, double Y) Transform2D(double x, double y, double elevation = 0.0)
    {
        if (IsIdentity)
        {
            return (x, y);
        }

        var (wx, wy, _) = Transform(x, y, elevation);
        return (wx, wy);
    }
}
