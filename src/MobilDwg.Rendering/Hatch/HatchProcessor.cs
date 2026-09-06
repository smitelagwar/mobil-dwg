using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Hatch;

public static class HatchProcessor
{
    public const double ClosureTolerance = 1e-3; // 1 mm / 0.001 units
    public const int MaxHatchLines = 2048;

    /// <summary>
    /// Validates boundary loop closure based on CAD closed loop semantics.
    /// In CAD, a loop of N vertices is closed by connecting vertex N-1 to vertex 0.
    /// If the last vertex duplicates the first, the redundant vertex is safely trimmed.
    /// Real boundary gaps are reported via diagnostics without corrupting valid closed polygons.
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
    /// Uses a fixed world pattern origin and integer line indexing so pattern phase remains invariant during pan and zoom.
    /// </summary>
    public static List<LinePrimitive> GeneratePatternLines(
        IReadOnlyList<HatchLoop> loops,
        double angleRadians,
        double spacing,
        WorldBounds2 bounds,
        WorldPoint2 patternOrigin = default)
    {
        var result = new List<LinePrimitive>();
        if (loops.Count == 0 || spacing <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return result;
        }

        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);

        // Direction along lines: D = (cos, sin)
        var dx = cos;
        var dy = sin;

        // Normal direction perpendicular to hatch lines: N = (-sin, cos)
        var nx = -sin;
        var ny = cos;

        // Project the 4 corners of bounds onto the normal axis relative to patternOrigin:
        // u = (X - Origin.X) * nx + (Y - Origin.Y) * ny
        // v = (X - Origin.X) * dx + (Y - Origin.Y) * dy
        var corners = new[]
        {
            new WorldPoint2(bounds.MinX, bounds.MinY),
            new WorldPoint2(bounds.MaxX, bounds.MinY),
            new WorldPoint2(bounds.MaxX, bounds.MaxY),
            new WorldPoint2(bounds.MinX, bounds.MaxY),
        };

        double minU = double.MaxValue;
        double maxU = double.MinValue;
        double minV = double.MaxValue;
        double maxV = double.MinValue;

        for (var i = 0; i < 4; i++)
        {
            var relX = corners[i].X - patternOrigin.X;
            var relY = corners[i].Y - patternOrigin.Y;
            var u = (relX * nx) + (relY * ny);
            var v = (relX * dx) + (relY * dy);
            if (u < minU) minU = u;
            if (u > maxU) maxU = u;
            if (v < minV) minV = v;
            if (v > maxV) maxV = v;
        }

        // k is the integer line index: line passes at distance (k * spacing) from patternOrigin
        var kStart = (int)Math.Floor(minU / spacing);
        var kEnd = (int)Math.Ceiling(maxU / spacing);

        var totalLines = kEnd - kStart + 1;
        var stride = 1;
        if (totalLines > MaxHatchLines)
        {
            stride = (int)Math.Ceiling((double)totalLines / MaxHatchLines);
        }

        // Segment length along D to safely span the coverage bounds
        var diagMargin = spacing * 1.5;
        var v0 = minV - diagMargin;
        var v1 = maxV + diagMargin;

        for (var k = kStart; k <= kEnd; k += stride)
        {
            if (result.Count >= MaxHatchLines) break;

            var uOffset = k * spacing;
            var pBaseX = patternOrigin.X + (uOffset * nx);
            var pBaseY = patternOrigin.Y + (uOffset * ny);

            var lineStart = new WorldPoint2(pBaseX + (v0 * dx), pBaseY + (v0 * dy));
            var lineEnd = new WorldPoint2(pBaseX + (v1 * dx), pBaseY + (v1 * dy));

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
