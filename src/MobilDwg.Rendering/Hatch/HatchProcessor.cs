using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Hatch;

public static class HatchProcessor
{
    public const double ClosureTolerance = 1e-3; // 1 mm / 0.001 units
    public const int MaxHatchLines = 2048;

    /// <summary>
    /// Validates boundary loop closure, closes minor gaps within tolerance, and emits a diagnostic on broken boundaries.
    /// </summary>
    public static HatchLoop ValidateAndCloseLoop(
        IReadOnlyList<WorldPoint2> points,
        bool isOuter,
        ICollection<SceneDiagnostic>? diagnostics = null,
        RenderEntityId? entityId = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 3)
        {
            throw new ArgumentException("Loop requires at least 3 points.", nameof(points));
        }

        var list = new List<WorldPoint2>(points);
        var first = list[0];
        var last = list[^1];
        var dx = first.X - last.X;
        var dy = first.Y - last.Y;
        var gap = Math.Sqrt((dx * dx) + (dy * dy));

        if (gap > 1e-9)
        {
            if (gap <= ClosureTolerance)
            {
                // Minor gap: auto-close
                list.Add(first);
            }
            else
            {
                // Large broken boundary: record diagnostic warning and close safely
                diagnostics?.Add(new SceneDiagnostic(
                    SceneDiagnosticKind.Unsupported,
                    "HATCH_BROKEN_BOUNDARY",
                    $"Hatch loop has a boundary gap of {gap:F4} units exceeding tolerance {ClosureTolerance}; auto-closed.",
                    entityId));
                list.Add(first);
            }
        }

        return new HatchLoop(list, isOuter);
    }

    /// <summary>
    /// Generates clipped pattern lines for standard CAD hatch patterns (e.g. ANSI31 diagonal lines).
    /// </summary>
    public static List<LinePrimitive> GeneratePatternLines(
        IReadOnlyList<HatchLoop> loops,
        double angleRadians,
        double spacing,
        WorldBounds2 bounds)
    {
        var result = new List<LinePrimitive>();
        if (loops.Count == 0 || spacing <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return result;
        }

        // Expand bounds slightly to cover diagonal sweeps
        var cx = bounds.Center.X;
        var cy = bounds.Center.Y;
        var diag = Math.Sqrt((bounds.Width * bounds.Width) + (bounds.Height * bounds.Height)) * 0.75;

        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);

        // Normal direction perpendicular to hatch lines
        var nx = -sin;
        var ny = cos;

        // Direction along hatch lines
        var dx = cos;
        var dy = sin;

        var lineCount = (int)Math.Min(MaxHatchLines, Math.Ceiling((2d * diag) / spacing));
        for (var step = -lineCount / 2; step <= lineCount / 2; step++)
        {
            if (result.Count >= MaxHatchLines) break;

            var offset = step * spacing;
            var originX = cx + (offset * nx);
            var originY = cy + (offset * ny);

            var lineStart = new WorldPoint2(originX - (diag * dx), originY - (diag * dy));
            var lineEnd = new WorldPoint2(originX + (diag * dx), originY + (diag * dy));

            // Clip line against all loops using 1D parameter intervals
            var intervals = ClipLineToLoops(lineStart, lineEnd, loops);
            foreach (var (t0, t1) in intervals)
            {
                if (t1 - t0 > 1e-6)
                {
                    var p0 = new WorldPoint2(lineStart.X + (t0 * (lineEnd.X - lineStart.X)), lineStart.Y + (t0 * (lineEnd.Y - lineStart.Y)));
                    var p1 = new WorldPoint2(lineStart.X + (t1 * (lineEnd.X - lineStart.X)), lineStart.Y + (t1 * (lineEnd.Y - lineStart.Y)));
                    result.Add(new LinePrimitive(p0, p1));
                }
            }
        }

        return result;
    }

    private static List<(double T0, double T1)> ClipLineToLoops(
        WorldPoint2 start,
        WorldPoint2 end,
        IReadOnlyList<HatchLoop> loops)
    {
        var intersections = new List<double>();
        var lx = end.X - start.X;
        var ly = end.Y - start.Y;

        foreach (var loop in loops)
        {
            var pts = loop.Vertices;
            for (var i = 0; i < pts.Count - 1; i++)
            {
                var p1 = pts[i];
                var p2 = pts[i + 1];

                var ex = p2.X - p1.X;
                var ey = p2.Y - p1.Y;

                var denom = (lx * ey) - (ly * ex);
                if (Math.Abs(denom) < 1e-12) continue;

                var qx = p1.X - start.X;
                var qy = p1.Y - start.Y;

                var t = ((qx * ey) - (qy * ex)) / denom;
                var u = ((qx * ly) - (qy * lx)) / denom;

                if (t >= 0d && t <= 1d && u >= 0d && u <= 1d)
                {
                    intersections.Add(t);
                }
            }
        }

        intersections.Sort();
        var validSegments = new List<(double T0, double T1)>();

        for (var i = 0; i < intersections.Count - 1; i += 2)
        {
            var t0 = intersections[i];
            var t1 = intersections[i + 1];
            var midT = (t0 + t1) / 2d;
            var midPoint = new WorldPoint2(start.X + (midT * lx), start.Y + (midT * ly));

            // Test if midpoint is inside outer boundary and outside islands
            if (IsPointInsideHatch(midPoint, loops))
            {
                validSegments.Add((t0, t1));
            }
        }

        return validSegments;
    }

    public static bool IsPointInsideHatch(WorldPoint2 pt, IReadOnlyList<HatchLoop> loops)
    {
        var insideCount = 0;
        foreach (var loop in loops)
        {
            if (IsPointInPolygon(pt, loop.Vertices))
            {
                insideCount++;
            }
        }
        // Even-odd rule: odd count means inside filled region
        return (insideCount % 2) == 1;
    }

    public static bool IsPointInPolygon(WorldPoint2 pt, IReadOnlyList<WorldPoint2> polygon)
    {
        var inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            if (((polygon[i].Y > pt.Y) != (polygon[j].Y > pt.Y)) &&
                (pt.X < ((polygon[j].X - polygon[i].X) * (pt.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y)) + polygon[i].X))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
