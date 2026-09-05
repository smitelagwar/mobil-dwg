using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Transforms;

public static class PrimitiveTransformer
{
    private static readonly GeometryTessellationOptions DefaultOptions = new(maxChordError: 0.01, minSegments: 4, maxSegments: 2048);

    public static RenderGeometryPrimitive Transform(RenderGeometryPrimitive primitive, Transform2D transform)
    {
        ArgumentNullException.ThrowIfNull(primitive);

        return primitive switch
        {
            PointPrimitive point => new PointPrimitive(transform.TransformPoint(point.Position)),
            LinePrimitive line => new LinePrimitive(
                transform.TransformPoint(line.Start),
                transform.TransformPoint(line.End)),
            PolygonPrimitive polygon => TransformPolygon(polygon, transform),
            PolylinePrimitive polyline => TransformPolyline(polyline, transform),
            ArcPrimitive arc => TransformArc(arc, transform),
            EllipsePrimitive ellipse => TransformEllipse(ellipse, transform),
            SplinePrimitive spline => TransformSpline(spline, transform),
            _ => throw new NotSupportedException($"Primitive type {primitive.GetType().Name} cannot be transformed.")
        };
    }

    private static PolygonPrimitive TransformPolygon(PolygonPrimitive polygon, Transform2D transform)
    {
        var transformedVertices = polygon.Vertices.Select(v => transform.TransformPoint(v)).ToArray();
        return new PolygonPrimitive(transformedVertices);
    }

    private static RenderGeometryPrimitive TransformPolyline(PolylinePrimitive polyline, Transform2D transform)
    {
        if (transform.IsUniformScale())
        {
            var isInv = transform.IsInverting;
            var transformedVertices = polyline.Vertices
                .Select(v => new PolylineVertex(
                    transform.TransformPoint(v.Position),
                    isInv ? -v.Bulge : v.Bulge))
                .ToArray();
            return new PolylinePrimitive(transformedVertices, polyline.Closed);
        }

        // Non-uniform scale distorts bulge arcs into elliptical segments.
        // Tessellating first preserves exact geometry.
        var path = GeometryTessellator.Tessellate(polyline, DefaultOptions);
        var transformedPoints = path.Points.Select(p => transform.TransformPoint(p)).ToArray();
        var vertices = transformedPoints.Select(p => new PolylineVertex(p, 0d)).ToArray();
        return new PolylinePrimitive(vertices, polyline.Closed);
    }

    private static RenderGeometryPrimitive TransformArc(ArcPrimitive arc, Transform2D transform)
    {
        var newCenter = transform.TransformPoint(arc.Center);
        var isFullCircle = Math.Abs(arc.SweepRadians) >= GeometryMath.Tau - 1e-6;

        if (transform.IsUniformScale())
        {
            var scale = transform.ScaleX;
            if (isFullCircle)
            {
                return new ArcPrimitive(newCenter, arc.Radius * scale, 0d, GeometryMath.Tau);
            }

            var vStart = new WorldPoint2(Math.Cos(arc.StartRadians), Math.Sin(arc.StartRadians));
            var vEnd = new WorldPoint2(Math.Cos(arc.StartRadians + arc.SweepRadians), Math.Sin(arc.StartRadians + arc.SweepRadians));

            var tvStart = transform.TransformVector(vStart);
            var tvEnd = transform.TransformVector(vEnd);

            var startAngle = Math.Atan2(tvStart.Y, tvStart.X);
            if (startAngle < 0d) startAngle += GeometryMath.Tau;

            var endAngle = Math.Atan2(tvEnd.Y, tvEnd.X);
            if (endAngle < 0d) endAngle += GeometryMath.Tau;

            double sweepAngle;
            if (transform.IsInverting)
            {
                // Mirroring flips winding direction
                sweepAngle = startAngle - endAngle;
                if (sweepAngle <= 0d) sweepAngle += GeometryMath.Tau;
                sweepAngle = -sweepAngle;
            }
            else
            {
                sweepAngle = endAngle - startAngle;
                if (sweepAngle <= 0d) sweepAngle += GeometryMath.Tau;
            }

            return new ArcPrimitive(newCenter, arc.Radius * scale, startAngle, sweepAngle);
        }

        // Non-uniform scale:
        // Full circle becomes an ellipse
        if (isFullCircle)
        {
            var m11 = transform.M11;
            var m12 = transform.M12;
            var m21 = transform.M21;
            var m22 = transform.M22;

            var a = (m11 * m11) + (m12 * m12);
            var b = (m11 * m21) + (m12 * m22);
            var c = (m21 * m21) + (m22 * m22);

            var delta = Math.Sqrt(Math.Max(0d, ((a - c) * (a - c)) + (4d * b * b)));
            var lambda1 = (a + c + delta) / 2d;
            var lambda2 = Math.Max(0d, (a + c - delta) / 2d);

            var majorRadius = arc.Radius * Math.Sqrt(Math.Max(1e-12, lambda1));
            var minorRadius = arc.Radius * Math.Sqrt(Math.Max(1e-12, lambda2));

            var rotation = 0.5d * Math.Atan2(2d * b, a - c);

            return new EllipsePrimitive(newCenter, majorRadius, minorRadius, rotation);
        }

        // Partial arc under non-uniform scale: tessellate to preserve exact geometry
        var arcPath = GeometryTessellator.Tessellate(arc, DefaultOptions);
        var transformedArcPoints = arcPath.Points.Select(p => transform.TransformPoint(p)).ToArray();
        var arcVertices = transformedArcPoints.Select(p => new PolylineVertex(p, 0d)).ToArray();
        return new PolylinePrimitive(arcVertices, closed: false);
    }

    private static RenderGeometryPrimitive TransformEllipse(EllipsePrimitive ellipse, Transform2D transform)
    {
        var ellipsePath = GeometryTessellator.Tessellate(ellipse, DefaultOptions);
        var transformedPoints = ellipsePath.Points.Select(p => transform.TransformPoint(p)).ToArray();
        var vertices = transformedPoints.Select(p => new PolylineVertex(p, 0d)).ToArray();
        return new PolylinePrimitive(vertices, closed: ellipsePath.Closed);
    }

    private static SplinePrimitive TransformSpline(SplinePrimitive spline, Transform2D transform)
    {
        var transformedControls = spline.ControlPoints.Select(p => transform.TransformPoint(p)).ToArray();
        return new SplinePrimitive(spline.Degree, transformedControls, spline.Knots, spline.Weights);
    }
}
