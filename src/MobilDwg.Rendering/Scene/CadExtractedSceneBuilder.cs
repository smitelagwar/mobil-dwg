using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Scene;

public static class CadExtractedSceneBuilder
{
    private static readonly RenderStyleToken ByLayerToken = new("BYLAYER");

    public static RenderScene Build(CadExtractedDocument document, RenderColorContext? colorContext = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        colorContext ??= RenderColorContext.Dark;

        // 1. Build Layer Table
        var layerDefinitions = new List<LayerDefinition>(document.Layers.Count + 1);
        var layerTokenMap = new Dictionary<string, RenderLayerToken>(document.Layers.Count + 2, StringComparer.OrdinalIgnoreCase);

        foreach (var l in document.Layers)
        {
            CadColor layerColor = (l.AciIndex > 0 && l.AciIndex <= 256)
                ? CadColor.FromAci(l.AciIndex)
                : CadColor.FromArgb(l.ArgbColor);

            CadLineweight layerLw = l.Lineweight >= 0
                ? CadLineweight.FromHundredthsOfMm(l.Lineweight)
                : CadLineweight.Default;

            var def = new LayerDefinition(l.Name, layerColor, CadLinetype.Continuous, layerLw, l.IsVisible);
            layerDefinitions.Add(def);
            layerTokenMap[l.Name] = new RenderLayerToken(l.Name);
        }

        if (!layerTokenMap.ContainsKey("0"))
        {
            layerDefinitions.Insert(0, new LayerDefinition("0", CadColor.FromAci(7), CadLinetype.Continuous, CadLineweight.Default, true));
            layerTokenMap["0"] = new RenderLayerToken("0");
        }

        var layerTable = new LayerTable(layerDefinitions);
        var assembler = new RenderSceneAssembler(colorContext);
        assembler.SetLayerTable(layerTable);

        // 2. Add document-level diagnostics
        foreach (var diag in document.Diagnostics)
        {
            var kind = diag.Severity switch
            {
                "Error" => SceneDiagnosticKind.Unsupported,
                "Warning" => SceneDiagnosticKind.Substituted,
                _ => SceneDiagnosticKind.Substituted
            };
            assembler.AddDiagnostic(new SceneDiagnostic(kind, diag.Code, diag.Message));
        }

        // 3. Build Entities
        int fallbackIndex = 0;
        foreach (var entity in document.Entities)
        {
            fallbackIndex++;
            if (!layerTokenMap.TryGetValue(entity.LayerName, out var layerToken))
            {
                layerToken = new RenderLayerToken(entity.LayerName);
                layerTokenMap[entity.LayerName] = layerToken;
            }

            // Resolve CadEntityStyle & StyleToken
            CadColor entityColor = entity.Color.Method switch
            {
                CadColorMethod.ByBlock => CadColor.ByBlock,
                CadColorMethod.Index => CadColor.FromAci(entity.Color.AciIndex),
                CadColorMethod.TrueColor => CadColor.FromArgb(entity.Color.Argb),
                _ => CadColor.ByLayer
            };

            CadLineweight lineweight = entity.Lineweight.ByBlock
                ? CadLineweight.ByBlock
                : entity.Lineweight.ByLayer
                    ? CadLineweight.ByLayer
                    : CadLineweight.FromHundredthsOfMm(entity.Lineweight.ValueHundredthsMm);

            var cadStyle = new CadEntityStyle(
                entityColor,
                CadLinetype.ByLayer,
                lineweight,
                entity.LinetypeScale);

            var styleToken = entity.Color.Method switch
            {
                CadColorMethod.ByBlock => new RenderStyleToken("BYBLOCK"),
                CadColorMethod.Index => new RenderStyleToken($"ACI:{entity.Color.AciIndex}"),
                CadColorMethod.TrueColor => new RenderStyleToken($"TRUECOLOR|#{entity.Color.Argb:X8}"),
                _ => ByLayerToken
            };

            var sourceRef = new RenderSourceReference(
                entity.EntityType.ToString(),
                entity.Handle,
                entity.SourceOrder > 0 ? entity.SourceOrder : fallbackIndex);

            var primitives = new List<RenderGeometryPrimitive>(1);

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

                case CadExtractedEntityType.Ellipse when entity.Points is { Count: >= 1 } && entity.Radius > 0:
                    double ellSweep = entity.EndAngle - entity.StartAngle;
                    if (ellSweep <= 0) ellSweep += Math.PI * 2;
                    double ratio = 1.0;
                    if (entity.Payload is CadEllipsePayload ellPayload && ellPayload.RadiusRatio > 0)
                    {
                        ratio = ellPayload.RadiusRatio;
                    }
                    primitives.Add(new EllipsePrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        entity.Radius,
                        entity.Radius * ratio,
                        entity.Rotation,
                        entity.StartAngle,
                        ellSweep));
                    break;

                case CadExtractedEntityType.Polyline when entity.Vertices is { Count: >= 2 }:
                    var polyPoints = new List<PolylineVertex>(entity.Vertices.Count);
                    foreach (var v in entity.Vertices)
                    {
                        polyPoints.Add(new PolylineVertex(new WorldPoint2(v.X, v.Y), v.Bulge));
                    }
                    bool isClosed = entity.Payload is CadPolylinePayload pl && pl.IsClosed;
                    primitives.Add(new PolylinePrimitive(polyPoints, closed: isClosed));
                    break;

                case CadExtractedEntityType.Spline when entity.Vertices is { Count: >= 2 }:
                    var splinePts = entity.Vertices.Select(v => new WorldPoint2(v.X, v.Y)).ToList();
                    if (entity.Payload is CadSplinePayload sp && sp.ControlPoints.Count >= sp.Degree + 1 && sp.Knots.Count == sp.ControlPoints.Count + sp.Degree + 1)
                    {
                        var ctrl = sp.ControlPoints.Select(p => new WorldPoint2(p.X, p.Y));
                        primitives.Add(new SplinePrimitive(sp.Degree, ctrl, sp.Knots, sp.Weights));
                    }
                    else
                    {
                        var polyVerts = splinePts.Select(p => new PolylineVertex(p, 0.0));
                        primitives.Add(new PolylinePrimitive(polyVerts, closed: false));
                    }
                    break;

                case CadExtractedEntityType.Text when entity.Points is { Count: >= 1 } && !string.IsNullOrEmpty(entity.Text):
                    primitives.Add(new TextPrimitive(
                        entity.Text,
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        height: entity.TextHeight > 0 ? entity.TextHeight : 12.0,
                        rotationRadians: entity.Rotation));
                    break;

                case CadExtractedEntityType.MText when entity.Points is { Count: >= 1 } && !string.IsNullOrEmpty(entity.Text):
                    primitives.Add(new TextPrimitive(
                        entity.Text,
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        height: entity.TextHeight > 0 ? entity.TextHeight : 12.0,
                        rotationRadians: entity.Rotation));
                    break;

                case CadExtractedEntityType.Solid when entity.Vertices is { Count: >= 3 }:
                    var solidPoly = entity.Vertices.Select(v => new PolylineVertex(new WorldPoint2(v.X, v.Y))).ToList();
                    primitives.Add(new PolylinePrimitive(solidPoly, closed: true));
                    break;

                case CadExtractedEntityType.Hatch when entity.Vertices is { Count: >= 2 }:
                    if (entity.Payload is CadHatchPayload hatchPayload && hatchPayload.Loops.Count > 0)
                    {
                        foreach (var loop in hatchPayload.Loops)
                        {
                            if (loop.Count >= 2)
                            {
                                var loopPts = loop.Select(v => new PolylineVertex(new WorldPoint2(v.X, v.Y), v.Bulge)).ToList();
                                primitives.Add(new PolylinePrimitive(loopPts, closed: true));
                            }
                        }
                    }
                    else
                    {
                        var hatchPts = entity.Vertices.Select(v => new PolylineVertex(new WorldPoint2(v.X, v.Y), v.Bulge)).ToList();
                        primitives.Add(new PolylinePrimitive(hatchPts, closed: true));
                    }
                    break;

                case CadExtractedEntityType.Dimension when entity.Points is { Count: >= 2 }:
                    // Draw dimension definition baseline
                    primitives.Add(new LinePrimitive(
                        new WorldPoint2(entity.Points[0].X, entity.Points[0].Y),
                        new WorldPoint2(entity.Points[1].X, entity.Points[1].Y)));
                    if (!string.IsNullOrEmpty(entity.Text))
                    {
                        primitives.Add(new TextPrimitive(
                            entity.Text,
                            new WorldPoint2(entity.Points[1].X, entity.Points[1].Y),
                            height: 10.0));
                    }
                    break;

                case CadExtractedEntityType.Point when entity.Points is { Count: >= 1 }:
                    // Tiny cross marker for point
                    double px = entity.Points[0].X;
                    double py = entity.Points[0].Y;
                    primitives.Add(new LinePrimitive(new WorldPoint2(px - 1, py), new WorldPoint2(px + 1, py)));
                    primitives.Add(new LinePrimitive(new WorldPoint2(px, py - 1), new WorldPoint2(px, py + 1)));
                    break;

                case CadExtractedEntityType.Unsupported or CadExtractedEntityType.Other:
                    // Diagnostic placeholder if points exist
                    if (entity.Points is { Count: >= 1 })
                    {
                        double ux = entity.Points[0].X;
                        double uy = entity.Points[0].Y;
                        primitives.Add(new LinePrimitive(new WorldPoint2(ux - 0.5, uy - 0.5), new WorldPoint2(ux + 0.5, uy + 0.5)));
                        primitives.Add(new LinePrimitive(new WorldPoint2(ux - 0.5, uy + 0.5), new WorldPoint2(ux + 0.5, uy - 0.5)));
                    }
                    break;
            }

            if (primitives.Count > 0)
            {
                assembler.AddEntity(new RenderSceneEntity(
                    new RenderEntityId(entity.Handle),
                    layerToken,
                    styleToken,
                    sourceRef,
                    primitives,
                    cadStyle));
            }
        }

        return assembler.Build();
    }
}
