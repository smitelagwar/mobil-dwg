using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;

namespace MobilDwg.Rendering.Dimensions;

public static class LeaderBuilder
{
    public static RenderSceneEntity BuildLeader(
        string entityId,
        IReadOnlyList<WorldPoint2> vertices,
        string? annotationText,
        double textHeight = 3.0d,
        double arrowheadSize = 2.5d,
        double doglegLength = 4.0d,
        string layer = "0",
        CadEntityStyle? cadStyle = null,
        ICollection<SceneDiagnostic>? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        var id = new RenderEntityId(entityId);
        var layerToken = new RenderLayerToken(layer);
        var styleToken = new RenderStyleToken("BYLAYER");
        var sourceRef = new RenderSourceReference("LEADER");

        if (vertices.Count < 2)
        {
            diagnostics?.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Unsupported,
                "INVALID_LEADER_GEOMETRY",
                "Leader requires at least 2 vertices.",
                id));
            return new RenderSceneEntity(id, new WorldBounds2(0, 0, 0, 0), layerToken, styleToken, sourceRef, Array.Empty<RenderGeometryPrimitive>(), cadStyle);
        }

        var primitives = new List<RenderGeometryPrimitive>();

        // Leader path lines
        for (var i = 0; i < vertices.Count - 1; i++)
        {
            primitives.Add(new LinePrimitive(vertices[i], vertices[i + 1]));
        }

        // Arrowhead at tip (first vertex pointing along first segment)
        var tip = vertices[0];
        var next = vertices[1];
        var dx = next.X - tip.X;
        var dy = next.Y - tip.Y;
        var len = Math.Sqrt((dx * dx) + (dy * dy));

        if (len > 1e-6)
        {
            var ux = dx / len;
            var uy = dy / len;
            var nx = -uy;
            var ny = ux;
            var baseWidth = arrowheadSize * 0.35d;
            var basePt = new WorldPoint2(tip.X + (ux * arrowheadSize), tip.Y + (uy * arrowheadSize));
            var c1 = new WorldPoint2(basePt.X + (nx * baseWidth), basePt.Y + (ny * baseWidth));
            var c2 = new WorldPoint2(basePt.X - (nx * baseWidth), basePt.Y - (ny * baseWidth));
            primitives.Add(new PolygonPrimitive([tip, c1, c2]));
        }

        // Dogleg and annotation text at end
        var last = vertices[^1];
        var prev = vertices[^2];
        var dirX = last.X >= prev.X ? 1d : -1d; // Dogleg goes left or right
        var doglegEnd = new WorldPoint2(last.X + (dirX * doglegLength), last.Y);
        primitives.Add(new LinePrimitive(last, doglegEnd));

        if (!string.IsNullOrEmpty(annotationText))
        {
            var textPos = new WorldPoint2(doglegEnd.X + (dirX * textHeight * 0.3d), doglegEnd.Y);
            var hAlign = dirX > 0 ? CadTextHorizontalAlignment.Left : CadTextHorizontalAlignment.Right;

            primitives.Add(new TextPrimitive(
                annotationText,
                textPos,
                height: textHeight,
                horizontalAlignment: hAlign,
                verticalAlignment: CadTextVerticalAlignment.Middle));
        }

        return new RenderSceneEntity(id, layerToken, styleToken, sourceRef, primitives, cadStyle);
    }
}
