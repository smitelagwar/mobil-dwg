using System.Globalization;
using System.Text;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Snapshots;

public static class P0GeometrySemanticSnapshot
{
    public static string Create(RenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        var sb = new StringBuilder();
        sb.Append("p0-geometry/v1\n");
        sb.Append("entities=").Append(scene.Entities.Count).Append('\n');

        foreach (var entity in scene.Entities)
        {
            sb.Append("entity=")
                .Append(E(entity.Id.Value)).Append('|')
                .Append(entity.Source.SourceIndex?.ToString(CultureInfo.InvariantCulture) ?? string.Empty).Append('|')
                .Append(E(entity.Source.EntityType)).Append('|')
                .Append(E(entity.Layer.Value)).Append('|')
                .Append(E(entity.Style.Value)).Append('|')
                .Append(entity.Geometry.Count)
                .Append('\n');

            foreach (var primitive in entity.Geometry)
            {
                sb.Append("primitive=").Append(Describe(primitive)).Append('\n');
            }
        }

        var diagnostics = scene.Diagnostics.Items
            .OrderBy(item => item.Kind)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.EntityId?.Value ?? string.Empty, StringComparer.Ordinal)
            .ToArray();
        sb.Append("diagnostics=").Append(diagnostics.Length).Append('\n');
        foreach (var diagnostic in diagnostics)
        {
            sb.Append("diagnostic=")
                .Append(diagnostic.Kind).Append('|')
                .Append(E(diagnostic.Code)).Append('|')
                .Append(E(diagnostic.EntityId?.Value ?? string.Empty)).Append('|')
                .Append(E(diagnostic.Message))
                .Append('\n');
        }

        return sb.ToString();
    }

    private static string Describe(RenderGeometryPrimitive primitive) => primitive switch
    {
        PointPrimitive point => $"POINT|{P(point.Position)}",
        LinePrimitive line => $"LINE|{P(line.Start)}|{P(line.End)}",
        ArcPrimitive arc => $"ARC|{P(arc.Center)}|{F(arc.Radius)}|{F(arc.StartRadians)}|{F(arc.SweepRadians)}",
        EllipsePrimitive ellipse => $"ELLIPSE|{P(ellipse.Center)}|{F(ellipse.MajorRadius)}|{F(ellipse.MinorRadius)}|{F(ellipse.RotationRadians)}|{F(ellipse.StartParameter)}|{F(ellipse.SweepParameter)}",
        PolylinePrimitive polyline => $"POLYLINE|{(polyline.Closed ? 1 : 0)}|{string.Join(";", polyline.Vertices.Select(vertex => $"{P(vertex.Position)},{F(vertex.Bulge)}"))}",
        PolygonPrimitive polygon => $"POLYGON|{string.Join(";", polygon.Vertices.Select(P))}",
        SplinePrimitive spline => $"SPLINE|{spline.Degree}|{string.Join(";", spline.ControlPoints.Select(P))}|{string.Join(",", spline.Knots.Select(F))}|{string.Join(",", spline.Weights.Select(F))}",
        _ => throw new NotSupportedException($"Unsupported P0 semantic primitive: {primitive.GetType().Name}"),
    };

    private static string P(WorldPoint2 point) => $"{F(point.X)},{F(point.Y)}";
    private static string F(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    private static string E(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("|", "\\|", StringComparison.Ordinal)
        .Replace(";", "\\;", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal);
}
