using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Scene;

public static class CadExtractedSceneBuilder
{
    private static readonly RenderStyleToken ByLayerToken = new("BYLAYER");
    private static readonly RenderSourceReference RefLine = new("Line");
    private static readonly RenderSourceReference RefCircle = new("Circle");
    private static readonly RenderSourceReference RefArc = new("Arc");
    private static readonly RenderSourceReference RefPolyline = new("Polyline");
    private static readonly RenderSourceReference RefText = new("Text");
    private static readonly RenderSourceReference RefOther = new("Other");

    public static RenderScene Build(CadExtractedDocument document, RenderColorContext? colorContext = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        colorContext ??= RenderColorContext.Dark;

        var layerDefinitions = new List<LayerDefinition>(document.Layers.Count + 1);
        var layerTokenMap = new Dictionary<string, RenderLayerToken>(document.Layers.Count + 2, StringComparer.OrdinalIgnoreCase);

        foreach (var l in document.Layers)
        {
            var color = CadColor.FromArgb(l.ArgbColor);
            layerDefinitions.Add(new LayerDefinition(l.Name, color, CadLinetype.Continuous, CadLineweight.Default, l.IsVisible));
            layerTokenMap[l.Name] = new RenderLayerToken(l.Name);
        }

        if (!layerTokenMap.ContainsKey("0"))
        {
            layerDefinitions.Insert(0, new LayerDefinition("0", CadColor.FromArgb(0xFFFFFFFF), CadLinetype.Continuous, CadLineweight.Default, true));
            layerTokenMap["0"] = new RenderLayerToken("0");
        }

        var layerTable = new LayerTable(layerDefinitions);
        var assembler = new RenderSceneAssembler(colorContext);
        assembler.SetLayerTable(layerTable);

        int id = 0;
        foreach (var entity in document.Entities)
        {
            id++;
            if (!layerTokenMap.TryGetValue(entity.LayerName, out var layerToken))
            {
                layerToken = new RenderLayerToken(entity.LayerName);
                layerTokenMap[entity.LayerName] = layerToken;
            }

            var styleToken = entity.ArgbColor.HasValue
                ? new RenderStyleToken($"TRUECOLOR|#{entity.ArgbColor.Value:X8}")
                : ByLayerToken;

            var primitives = new List<RenderGeometryPrimitive>(1);
            RenderSourceReference sourceRef;

            switch (entity.EntityType)
            {
                case CadExtractedEntityType.Line when entity.Points is { Count: >= 2 }:
                    primitives.Add(new LinePrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        new WorldPoint2(entity.Points[1].X, entity.Points[1].Y)));
                    sourceRef = RefLine;
                    break;

                case CadExtractedEntityType.Circle when entity.Points is { Count: >= 1 } && entity.Radius > 0:
                    primitives.Add(new ArcPrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        entity.Radius,
                        0,
                        Math.PI * 2));
                    sourceRef = RefCircle;
                    break;

                case CadExtractedEntityType.Arc when entity.Points is { Count: >= 1 } && entity.Radius > 0:
                    double sweep = entity.EndAngle - entity.StartAngle;
                    if (sweep <= 0) sweep += Math.PI * 2;
                    primitives.Add(new ArcPrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        entity.Radius,
                        entity.StartAngle,
                        sweep));
                    sourceRef = RefArc;
                    break;

                case CadExtractedEntityType.Polyline when entity.Vertices is { Count: >= 2 }:
                    var polyPoints = new List<PolylineVertex>(entity.Vertices.Count);
                    foreach (var v in entity.Vertices)
                    {
                        polyPoints.Add(new PolylineVertex(new WorldPoint2(v.X, v.Y), v.Bulge));
                    }
                    primitives.Add(new PolylinePrimitive(polyPoints, false));
                    sourceRef = RefPolyline;
                    break;

                case CadExtractedEntityType.Text when entity.Points is { Count: >= 1 } && !string.IsNullOrEmpty(entity.Text):
                    primitives.Add(new TextPrimitive(
                        entity.Text,
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        height: entity.TextHeight > 0 ? entity.TextHeight : 12.0,
                        rotationRadians: entity.Rotation));
                    sourceRef = RefText;
                    break;

                default:
                    sourceRef = RefOther;
                    break;
            }

            if (primitives.Count > 0)
            {
                assembler.AddEntity(new RenderSceneEntity(
                    new RenderEntityId(entity.Handle),
                    layerToken,
                    styleToken,
                    sourceRef,
                    primitives));
            }
        }

        return assembler.Build();
    }
}
