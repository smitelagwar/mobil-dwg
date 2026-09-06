using System.Globalization;
using MobilDwg.Rendering.Blocks;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;

namespace MobilDwg.Rendering.Dimensions;

public static class DimensionBuilder
{
    /// <summary>
    /// Builds dimension geometry following the "Anonymous Dimension Block First (*D...)" rule,
    /// falling back to procedural generation with degenerate checks when no block is available.
    /// </summary>
    /// <summary>
    /// Safely attempts to build a dimension from raw coordinate values, emitting an INVALID_DIMENSION_GEOMETRY diagnostic if non-finite.
    /// </summary>
    public static RenderSceneEntity TryBuildFromRaw(
        string entityId,
        CadDimensionType dimensionType,
        double def1X, double def1Y,
        double def2X, double def2Y,
        double dimLineX, double dimLineY,
        IReadOnlyDictionary<string, BlockDefinition>? blockTable = null,
        string layer = "0",
        CadEntityStyle? cadStyle = null,
        ICollection<SceneDiagnostic>? diagnostics = null)
    {
        var id = new RenderEntityId(entityId);
        if (!double.IsFinite(def1X) || !double.IsFinite(def1Y) ||
            !double.IsFinite(def2X) || !double.IsFinite(def2Y) ||
            !double.IsFinite(dimLineX) || !double.IsFinite(dimLineY))
        {
            diagnostics?.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Unsupported,
                "INVALID_DIMENSION_GEOMETRY",
                "Dimension contains non-finite raw coordinates; skipped.",
                id));
            return new RenderSceneEntity(id, new WorldBounds2(0, 0, 0, 0), new RenderLayerToken(layer), new RenderStyleToken("BYLAYER"), new RenderSourceReference("DIMENSION"), Array.Empty<RenderGeometryPrimitive>(), cadStyle);
        }

        var def = new CadDimensionDefinition(
            dimensionType,
            new WorldPoint2(def1X, def1Y),
            new WorldPoint2(def2X, def2Y),
            new WorldPoint2(dimLineX, dimLineY));

        return BuildDimension(entityId, def, blockTable, layer, cadStyle, diagnostics);
    }

    public static RenderSceneEntity BuildDimension(
        string entityId,
        CadDimensionDefinition def,
        IReadOnlyDictionary<string, BlockDefinition>? blockTable = null,
        string layer = "0",
        CadEntityStyle? cadStyle = null,
        ICollection<SceneDiagnostic>? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(def);
        var id = new RenderEntityId(entityId);
        var layerToken = new RenderLayerToken(layer);
        var styleToken = new RenderStyleToken("BYLAYER");
        var sourceRef = new RenderSourceReference("DIMENSION");

        // 1. Anonymous Dimension Block First (*D...)
        if (!string.IsNullOrWhiteSpace(def.AnonymousBlockName) &&
            blockTable != null &&
            blockTable.TryGetValue(def.AnonymousBlockName, out var anonBlock))
        {
            var reference = new BlockReference(
                def.AnonymousBlockName,
                insertionPoint: new WorldPoint2(0, 0),
                layer: layerToken,
                style: styleToken);

            var expander = new BlockExpander(blockTable.Values);
            var expansion = expander.Expand([reference]);
            if (diagnostics != null && expansion.Diagnostics.Count > 0)
            {
                foreach (var diag in expansion.Diagnostics)
                {
                    diagnostics.Add(diag);
                }
            }

            if (expansion.Entities.Count > 0)
            {
                var combinedPrimitives = expansion.Entities.SelectMany(e => e.Geometry).ToArray();
                if (combinedPrimitives.Length > 0)
                {
                    return new RenderSceneEntity(id, layerToken, styleToken, sourceRef, combinedPrimitives, cadStyle);
                }
            }
        }

        // 2. Procedural Dimension Generation with Degenerate Guard
        var primitives = new List<RenderGeometryPrimitive>();

        // Coordinate validity check
        if (!double.IsFinite(def.DefPoint1.X) || !double.IsFinite(def.DefPoint1.Y) ||
            !double.IsFinite(def.DefPoint2.X) || !double.IsFinite(def.DefPoint2.Y) ||
            !double.IsFinite(def.DimensionLinePoint.X) || !double.IsFinite(def.DimensionLinePoint.Y))
        {
            diagnostics?.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Unsupported,
                "INVALID_DIMENSION_GEOMETRY",
                "Dimension contains non-finite definition points; skipped.",
                id));
            return new RenderSceneEntity(id, new WorldBounds2(0, 0, 0, 0), layerToken, styleToken, sourceRef, Array.Empty<RenderGeometryPrimitive>(), cadStyle);
        }

        var dx = def.DefPoint2.X - def.DefPoint1.X;
        var dy = def.DefPoint2.Y - def.DefPoint1.Y;
        var dist = Math.Sqrt((dx * dx) + (dy * dy));

        if (dist < 1e-6)
        {
            diagnostics?.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Unsupported,
                "DEGENERATE_DIMENSION_POINTS",
                "Dimension definition points coincide; cannot generate dimension line.",
                id));
            return new RenderSceneEntity(id, new WorldBounds2(def.DefPoint1.X, def.DefPoint1.Y, def.DefPoint1.X, def.DefPoint1.Y), layerToken, styleToken, sourceRef, Array.Empty<RenderGeometryPrimitive>(), cadStyle);
        }

        switch (def.DimensionType)
        {
            case CadDimensionType.Radial:
                BuildRadialDimension(def, primitives, id);
                break;

            case CadDimensionType.Diametric:
                BuildDiametricDimension(def, primitives, id);
                break;

            case CadDimensionType.Angular:
                BuildAngularDimension(def, primitives, id);
                break;

            case CadDimensionType.Linear:
            case CadDimensionType.Aligned:
            default:
                BuildLinearAlignedDimension(def, primitives, id, dist, dx, dy);
                break;
        }

        return new RenderSceneEntity(id, layerToken, styleToken, sourceRef, primitives, cadStyle);
    }

    private static void BuildLinearAlignedDimension(
        CadDimensionDefinition def,
        List<RenderGeometryPrimitive> primitives,
        RenderEntityId id,
        double dist,
        double dx,
        double dy)
    {
        double ux, uy, nx, ny, measuredValue;
        if (def.DimensionType == CadDimensionType.Linear)
        {
            var angle = def.RotationRadians;
            ux = Math.Cos(angle);
            uy = Math.Sin(angle);
            nx = -uy;
            ny = ux;
            measuredValue = Math.Abs((dx * ux) + (dy * uy));
        }
        else
        {
            ux = dx / dist;
            uy = dy / dist;
            nx = -uy;
            ny = ux;
            measuredValue = dist;
        }

        if (measuredValue < 1e-6)
        {
            // Degenerate measurement along dimension axis
            return;
        }

        // Project DefPoint1 and DefPoint2 along n to the dimension line passing through DimensionLinePoint
        var p1 = def.DefPoint1;
        var p2 = def.DefPoint2;
        var offset1 = ((def.DimensionLinePoint.X - p1.X) * nx) + ((def.DimensionLinePoint.Y - p1.Y) * ny);
        var offset2 = ((def.DimensionLinePoint.X - p2.X) * nx) + ((def.DimensionLinePoint.Y - p2.Y) * ny);

        var dl1 = new WorldPoint2(p1.X + (offset1 * nx), p1.Y + (offset1 * ny));
        var dl2 = new WorldPoint2(p2.X + (offset2 * nx), p2.Y + (offset2 * ny));

        // Extension lines
        var extOverhang = def.ArrowheadSize * 0.5d;
        var s1 = Math.Abs(offset1) > 1e-6 ? Math.Sign(offset1) : 1d;
        var s2 = Math.Abs(offset2) > 1e-6 ? Math.Sign(offset2) : 1d;
        var ext1End = new WorldPoint2(dl1.X + (s1 * extOverhang * nx), dl1.Y + (s1 * extOverhang * ny));
        var ext2End = new WorldPoint2(dl2.X + (s2 * extOverhang * nx), dl2.Y + (s2 * extOverhang * ny));

        primitives.Add(new LinePrimitive(p1, ext1End));
        primitives.Add(new LinePrimitive(p2, ext2End));

        // Dimension line
        primitives.Add(new LinePrimitive(dl1, dl2));

        // Arrowheads: point along the dimension line segment towards center
        var lineDx = dl2.X - dl1.X;
        var lineDy = dl2.Y - dl1.Y;
        var lineLen = Math.Sqrt((lineDx * lineDx) + (lineDy * lineDy));
        if (lineLen > 1e-6)
        {
            var lUx = lineDx / lineLen;
            var lUy = lineDy / lineLen;
            AddArrowhead(primitives, dl1, new WorldPoint2(dl1.X + (lUx * def.ArrowheadSize), dl1.Y + (lUy * def.ArrowheadSize)), def.ArrowheadSize, def.ArrowStyle);
            AddArrowhead(primitives, dl2, new WorldPoint2(dl2.X - (lUx * def.ArrowheadSize), dl2.Y - (lUy * def.ArrowheadSize)), def.ArrowheadSize, def.ArrowStyle);
        }

        // Dimension text
        var displayText = FormatDimensionText(measuredValue, def.TextOverride);

        var textMid = new WorldPoint2((dl1.X + dl2.X) / 2d, (dl1.Y + dl2.Y) / 2d);
        var textPos = def.TextPosition ?? new WorldPoint2(textMid.X + (nx * def.TextHeight * 0.6d), textMid.Y + (ny * def.TextHeight * 0.6d));
        var textAngle = Math.Atan2(uy, ux);
        if (textAngle > Math.PI / 2d || textAngle <= -Math.PI / 2d)
        {
            textAngle += Math.PI; // Ensure text reads left-to-right
        }

        primitives.Add(new TextPrimitive(
            displayText,
            textPos,
            height: def.TextHeight,
            rotationRadians: textAngle,
            horizontalAlignment: CadTextHorizontalAlignment.Center,
            verticalAlignment: CadTextVerticalAlignment.Bottom));
    }

    private static void BuildRadialDimension(
        CadDimensionDefinition def,
        List<RenderGeometryPrimitive> primitives,
        RenderEntityId id)
    {
        var center = def.CenterPoint ?? def.DefPoint1;
        var chord = def.DefPoint2;
        var dx = chord.X - center.X;
        var dy = chord.Y - center.Y;
        var radius = Math.Sqrt((dx * dx) + (dy * dy));

        if (radius < 1e-6) return;
        var ux = dx / radius;
        var uy = dy / radius;

        // Leader from center to chord point
        primitives.Add(new LinePrimitive(center, chord));
        AddArrowhead(primitives, chord, center, def.ArrowheadSize, def.ArrowStyle);

        var displayText = string.IsNullOrEmpty(def.TextOverride)
            ? $"R{radius.ToString("F2", CultureInfo.InvariantCulture)}"
            : def.TextOverride.Replace("<>", $"R{radius.ToString("F2", CultureInfo.InvariantCulture)}", StringComparison.Ordinal);

        var textPos = def.TextPosition ?? new WorldPoint2(chord.X + (ux * def.TextHeight), chord.Y + (uy * def.TextHeight));
        primitives.Add(new TextPrimitive(
            displayText,
            textPos,
            height: def.TextHeight,
            rotationRadians: Math.Atan2(uy, ux),
            horizontalAlignment: CadTextHorizontalAlignment.Left,
            verticalAlignment: CadTextVerticalAlignment.Middle));
    }

    private static void BuildDiametricDimension(
        CadDimensionDefinition def,
        List<RenderGeometryPrimitive> primitives,
        RenderEntityId id)
    {
        var p1 = def.DefPoint1;
        var p2 = def.DefPoint2;
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        var diameter = Math.Sqrt((dx * dx) + (dy * dy));

        if (diameter < 1e-6) return;
        var ux = dx / diameter;
        var uy = dy / diameter;

        primitives.Add(new LinePrimitive(p1, p2));
        AddArrowhead(primitives, p1, p2, def.ArrowheadSize, def.ArrowStyle);
        AddArrowhead(primitives, p2, p1, def.ArrowheadSize, def.ArrowStyle);

        var displayText = string.IsNullOrEmpty(def.TextOverride)
            ? $"\u00D8{diameter.ToString("F2", CultureInfo.InvariantCulture)}"
            : def.TextOverride.Replace("<>", $"\u00D8{diameter.ToString("F2", CultureInfo.InvariantCulture)}", StringComparison.Ordinal);

        var textMid = new WorldPoint2((p1.X + p2.X) / 2d, (p1.Y + p2.Y) / 2d);
        var textPos = def.TextPosition ?? textMid;

        primitives.Add(new TextPrimitive(
            displayText,
            textPos,
            height: def.TextHeight,
            rotationRadians: Math.Atan2(uy, ux),
            horizontalAlignment: CadTextHorizontalAlignment.Center,
            verticalAlignment: CadTextVerticalAlignment.Bottom));
    }

    private static void BuildAngularDimension(
        CadDimensionDefinition def,
        List<RenderGeometryPrimitive> primitives,
        RenderEntityId id)
    {
        var center = def.CenterPoint ?? def.DefPoint1;
        var p1 = def.DefPoint1;
        var p2 = def.DefPoint2;
        var dimPt = def.DimensionLinePoint;

        var v1x = p1.X - center.X;
        var v1y = p1.Y - center.Y;
        var v2x = p2.X - center.X;
        var v2y = p2.Y - center.Y;

        var a1 = Math.Atan2(v1y, v1x);
        var a2 = Math.Atan2(v2y, v2x);

        var sweep = a2 - a1;
        while (sweep <= 0) sweep += 2 * Math.PI;
        if (sweep > 2 * Math.PI) sweep %= (2 * Math.PI);

        var dimDx = dimPt.X - center.X;
        var dimDy = dimPt.Y - center.Y;
        var radius = Math.Sqrt((dimDx * dimDx) + (dimDy * dimDy));
        if (radius < 1e-6) radius = Math.Max(10.0, def.ArrowheadSize * 4.0);

        // Dimension arc
        primitives.Add(new ArcPrimitive(center, radius, a1, sweep));

        // Arrowheads at both ends of the arc
        var end1 = new WorldPoint2(center.X + (radius * Math.Cos(a1)), center.Y + (radius * Math.Sin(a1)));
        var end2 = new WorldPoint2(center.X + (radius * Math.Cos(a1 + sweep)), center.Y + (radius * Math.Sin(a1 + sweep)));

        var tan1 = new WorldPoint2(-Math.Sin(a1), Math.Cos(a1));
        var tan2 = new WorldPoint2(Math.Sin(a1 + sweep), -Math.Cos(a1 + sweep));

        AddArrowhead(primitives, end1, new WorldPoint2(end1.X + (tan1.X * def.ArrowheadSize), end1.Y + (tan1.Y * def.ArrowheadSize)), def.ArrowheadSize, def.ArrowStyle);
        AddArrowhead(primitives, end2, new WorldPoint2(end2.X + (tan2.X * def.ArrowheadSize), end2.Y + (tan2.Y * def.ArrowheadSize)), def.ArrowheadSize, def.ArrowStyle);

        // Dimension text
        var deg = sweep * (180.0 / Math.PI);
        var displayText = string.IsNullOrEmpty(def.TextOverride)
            ? $"{deg.ToString("F1", CultureInfo.InvariantCulture)}\u00B0"
            : (def.TextOverride.Contains("<>", StringComparison.Ordinal)
                ? def.TextOverride.Replace("<>", $"{deg.ToString("F1", CultureInfo.InvariantCulture)}\u00B0", StringComparison.Ordinal)
                : def.TextOverride);

        var midAngle = a1 + (sweep / 2.0);
        var textRadius = radius + (def.TextHeight * 0.6);
        var textPos = def.TextPosition ?? new WorldPoint2(center.X + (textRadius * Math.Cos(midAngle)), center.Y + (textRadius * Math.Sin(midAngle)));

        primitives.Add(new TextPrimitive(
            displayText,
            textPos,
            height: def.TextHeight,
            rotationRadians: midAngle + (Math.PI / 2.0),
            horizontalAlignment: CadTextHorizontalAlignment.Center,
            verticalAlignment: CadTextVerticalAlignment.Bottom));
    }

    private static void AddArrowhead(
        List<RenderGeometryPrimitive> primitives,
        WorldPoint2 tip,
        WorldPoint2 towards,
        double size,
        CadArrowheadStyle style)
    {
        var dx = towards.X - tip.X;
        var dy = towards.Y - tip.Y;
        var len = Math.Sqrt((dx * dx) + (dy * dy));
        if (len < 1e-9) return;

        var ux = dx / len;
        var uy = dy / len;
        var nx = -uy;
        var ny = ux;

        if (style == CadArrowheadStyle.ArchitecturalTick)
        {
            var tick = size * 0.707d;
            var t1 = new WorldPoint2(tip.X - (tick * (ux + nx)), tip.Y - (tick * (uy + ny)));
            var t2 = new WorldPoint2(tip.X + (tick * (ux + nx)), tip.Y + (tick * (uy + ny)));
            primitives.Add(new LinePrimitive(t1, t2));
        }
        else
        {
            // ClosedFilled triangle
            var baseWidth = size * 0.35d;
            var basePoint = new WorldPoint2(tip.X + (ux * size), tip.Y + (uy * size));
            var corner1 = new WorldPoint2(basePoint.X + (nx * baseWidth), basePoint.Y + (ny * baseWidth));
            var corner2 = new WorldPoint2(basePoint.X - (nx * baseWidth), basePoint.Y - (ny * baseWidth));

            primitives.Add(new PolygonPrimitive([tip, corner1, corner2]));
        }
    }

    private static string FormatDimensionText(double value, string? textOverride)
    {
        var formatted = value.ToString("F2", CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(textOverride)) return formatted;
        return textOverride.Contains("<>", StringComparison.Ordinal)
            ? textOverride.Replace("<>", formatted, StringComparison.Ordinal)
            : textOverride;
    }
}
