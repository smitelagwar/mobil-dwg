using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Scene;

public static class CadExtractedSceneBuilder
{
    public static RenderScene Build(CadExtractedDocument document, RenderColorContext? colorContext = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        colorContext ??= RenderColorContext.Dark;

        var layerDefinitions = new List<LayerDefinition>();
        foreach (var l in document.Layers)
        {
            var color = CadColor.FromArgb(l.ArgbColor);
            layerDefinitions.Add(new LayerDefinition(l.Name, color, CadLinetype.Continuous, CadLineweight.Default, l.IsVisible));
        }

        if (!layerDefinitions.Any(l => l.Name == "0"))
        {
            layerDefinitions.Insert(0, new LayerDefinition("0", CadColor.FromArgb(0xFFFFFFFF), CadLinetype.Continuous, CadLineweight.Default, true));
        }

        var layerTable = new LayerTable(layerDefinitions);
        var assembler = new RenderSceneAssembler(colorContext);
        assembler.SetLayerTable(layerTable);

        int id = 0;
        foreach (var entity in document.Entities)
        {
            id++;
            var layerToken = new RenderLayerToken(entity.LayerName);
            var styleToken = entity.ArgbColor.HasValue
                ? new RenderStyleToken($"TRUECOLOR|#{entity.ArgbColor.Value:X8}")
                : new RenderStyleToken("BYLAYER");

            var primitives = new List<RenderGeometryPrimitive>();

            switch (entity.EntityType)
            {
                case CadExtractedEntityType.Line when entity.Points is { Count: >= 2 }:
                    primitives.Add(new LinePrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        new WorldPoint2(entity.Points[1].X, entity.Points[1].Y)));
                    break;

                case CadExtractedEntityType.Circle when entity.Points is { Count: >= 1 } && entity.Radius > 0:
                    primitives.Add(new ArcPrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        entity.Radius,
                        0,
                        Math.PI * 2));
                    break;

                case CadExtractedEntityType.Arc when entity.Points is { Count: >= 1 } && entity.Radius > 0:
                    double sweep = entity.EndAngle - entity.StartAngle;
                    if (sweep <= 0) sweep += Math.PI * 2;
                    primitives.Add(new ArcPrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        entity.Radius,
                        entity.StartAngle,
                        sweep));
                    break;

                case CadExtractedEntityType.Polyline when entity.Vertices is { Count: >= 2 }:
                    var polyPoints = new List<PolylineVertex>();
                    foreach (var v in entity.Vertices)
                    {
                        polyPoints.Add(new PolylineVertex(new WorldPoint2(v.X, v.Y), v.Bulge));
                    }
                    primitives.Add(new PolylinePrimitive(polyPoints, false));
                    break;

                case CadExtractedEntityType.Text when entity.Points is { Count: >= 1 } && !string.IsNullOrEmpty(entity.Text):
                    primitives.Add(new TextPrimitive(
                        entity.Text,
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        height: entity.TextHeight > 0 ? entity.TextHeight : 12.0,
                        rotationRadians: entity.Rotation));
                    break;
            }

            if (primitives.Count > 0)
            {
                assembler.AddEntity(new RenderSceneEntity(
                    new RenderEntityId($"E_{id}"),
                    layerToken,
                    styleToken,
                    new RenderSourceReference(entity.EntityType.ToString()),
                    primitives));
            }
        }

        return assembler.Build();
    }
}
