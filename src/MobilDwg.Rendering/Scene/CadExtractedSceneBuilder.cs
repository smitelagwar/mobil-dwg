using MobilDwg.Core.Reading;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Dimensions;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Hatch;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Text;

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
            ConvertExtractedEntityToPrimitives(entity, primitives, cadStyle);

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

    private static void ConvertExtractedEntityToPrimitives(
        CadExtractedEntity entity,
        List<RenderGeometryPrimitive> primitives,
        CadEntityStyle? cadStyle)
    {
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

            case CadExtractedEntityType.Text:
            case CadExtractedEntityType.MText:
            case CadExtractedEntityType.Attrib:
            case CadExtractedEntityType.AttDef:
                if (!string.IsNullOrEmpty(entity.Text) && entity.Points is { Count: >= 1 })
                {
                    var textPayload = entity.Payload as CadTextPayload;
                    var hAlign = CadTextHorizontalAlignment.Left;
                    var vAlign = CadTextVerticalAlignment.Baseline;
                    var mirror = CadTextMirrorFlags.None;
                    var widthFactor = 1.0;
                    var obliqueAngle = 0.0;
                    var fontName = "STANDARD";

                    if (textPayload != null)
                    {
                        if (textPayload.AttachmentPoint > 0)
                        {
                            (hAlign, vAlign) = CadTextAlignmentHelper.FromAttachmentPoint((CadTextAttachmentPoint)textPayload.AttachmentPoint);
                        }
                        else
                        {
                            hAlign = (CadTextHorizontalAlignment)textPayload.HorizontalAlignment;
                            vAlign = (CadTextVerticalAlignment)textPayload.VerticalAlignment;
                        }

                        mirror = (CadTextMirrorFlags)textPayload.MirrorFlags;
                        if (textPayload.WidthFactor > 0) widthFactor = textPayload.WidthFactor;
                        obliqueAngle = textPayload.ObliqueAngle;
                        if (!string.IsNullOrEmpty(textPayload.FontName)) fontName = textPayload.FontName;
                    }

                    double textHeight = entity.TextHeight > 0 ? entity.TextHeight : 12.0;
                    var pos = new WorldPoint2(entity.Points[0].X, entity.Points[0].Y);

                    primitives.Add(new TextPrimitive(
                        entity.Text,
                        pos,
                        height: textHeight,
                        rotationRadians: entity.Rotation,
                        widthFactor: widthFactor,
                        obliqueAngleRadians: obliqueAngle,
                        horizontalAlignment: hAlign,
                        verticalAlignment: vAlign,
                        mirrorFlags: mirror,
                        requestedFont: fontName));
                }
                break;

            case CadExtractedEntityType.Solid when entity.Vertices is { Count: >= 3 }:
                var solidPoly = entity.Vertices.Select(v => new PolylineVertex(new WorldPoint2(v.X, v.Y))).ToList();
                primitives.Add(new PolylinePrimitive(solidPoly, closed: true));
                break;

            case CadExtractedEntityType.Hatch:
                if (entity.Payload is CadHatchPayload hatchPayload && hatchPayload.Loops.Count > 0)
                {
                    var hatchLoops = new List<HatchLoop>(hatchPayload.Loops.Count);
                    for (var li = 0; li < hatchPayload.Loops.Count; li++)
                    {
                        var rawLoop = hatchPayload.Loops[li];
                        if (rawLoop.Count < 3) continue;

                        var loopPts = rawLoop.Select(v => new WorldPoint2(v.X, v.Y)).ToList();
                        var validatedLoop = HatchProcessor.ValidateAndCloseLoop(
                            loopPts,
                            isOuter: li == 0);
                        hatchLoops.Add(validatedLoop);
                    }

                    if (hatchLoops.Count > 0)
                    {
                        var unionBounds = hatchLoops[0].Bounds;
                        for (var bi = 1; bi < hatchLoops.Count; bi++)
                        {
                            unionBounds = unionBounds.Union(hatchLoops[bi].Bounds);
                        }

                        var origin = new WorldPoint2(hatchPayload.Origin.X, hatchPayload.Origin.Y);
                        var patternLines = !hatchPayload.IsSolid
                            ? HatchProcessor.GeneratePatternLines(
                                hatchLoops,
                                hatchPayload.Angle,
                                Math.Max(0.5, hatchPayload.Scale > 0 ? hatchPayload.Scale * 5.0 : 5.0),
                                unionBounds,
                                origin)
                            : null;

                        primitives.Add(new HatchPrimitive(
                            hatchLoops,
                            patternName: hatchPayload.PatternName,
                            patternAngleRadians: hatchPayload.Angle,
                            patternScale: hatchPayload.Scale > 0 ? hatchPayload.Scale : 1.0,
                            islandStyle: HatchIslandStyle.Normal,
                            isSolid: hatchPayload.IsSolid,
                            patternLines: patternLines,
                            patternOrigin: origin));
                    }
                }
                else if (entity.Vertices is { Count: >= 3 })
                {
                    var loopPts = entity.Vertices.Select(v => new WorldPoint2(v.X, v.Y)).ToList();
                    var loop = HatchProcessor.ValidateAndCloseLoop(loopPts, isOuter: true);
                    primitives.Add(new HatchPrimitive(
                        new[] { loop },
                        patternName: "SOLID",
                        isSolid: true));
                }
                break;

            case CadExtractedEntityType.Dimension:
                if (entity.Payload is CadDimensionPayload dimPayload)
                {
                    if (dimPayload.ExplodedEntities != null && dimPayload.ExplodedEntities.Count > 0)
                    {
                        foreach (var child in dimPayload.ExplodedEntities)
                        {
                            ConvertExtractedEntityToPrimitives(child, primitives, cadStyle);
                        }
                    }
                    else if (string.Equals(dimPayload.DimensionType, "Leader", StringComparison.OrdinalIgnoreCase) && entity.Points is { Count: >= 2 })
                    {
                        var leaderPts = entity.Points.Select(p => new WorldPoint2(p.X, p.Y)).ToList();
                        var leaderEntity = LeaderBuilder.BuildLeader(
                            entity.Handle,
                            leaderPts,
                            entity.Text,
                            textHeight: dimPayload.TextHeight > 0 ? dimPayload.TextHeight : 3.0,
                            arrowheadSize: dimPayload.ArrowheadSize > 0 ? dimPayload.ArrowheadSize : 2.5,
                            layer: entity.LayerName,
                            cadStyle: cadStyle);
                        primitives.AddRange(leaderEntity.Geometry);
                    }
                    else
                    {
                        var dimType = dimPayload.DimensionType switch
                        {
                            "Linear" => CadDimensionType.Linear,
                            "Aligned" => CadDimensionType.Aligned,
                            "Radial" => CadDimensionType.Radial,
                            "Diametric" => CadDimensionType.Diametric,
                            "Angular" => CadDimensionType.Angular,
                            "Ordinate" => CadDimensionType.Ordinate,
                            _ => CadDimensionType.Aligned
                        };

                        var p1 = new WorldPoint2(dimPayload.Point1.X, dimPayload.Point1.Y);
                        var p2 = new WorldPoint2(dimPayload.Point2.X, dimPayload.Point2.Y);
                        var dimLinePt = new WorldPoint2(dimPayload.DimLinePoint.X, dimPayload.DimLinePoint.Y);
                        var textPos = dimPayload.TextPosition != default ? new WorldPoint2(dimPayload.TextPosition.X, dimPayload.TextPosition.Y) : (WorldPoint2?)null;
                        var centerPt = dimPayload.CenterPoint != default ? new WorldPoint2(dimPayload.CenterPoint.X, dimPayload.CenterPoint.Y) : (WorldPoint2?)null;

                        var dimDef = new CadDimensionDefinition(
                            dimType,
                            p1,
                            p2,
                            dimLinePt,
                            textPos,
                            centerPt,
                            dimPayload.Rotation,
                            textOverride: entity.Text,
                            anonymousBlockName: dimPayload.BlockName,
                            arrowheadSize: dimPayload.ArrowheadSize > 0 ? dimPayload.ArrowheadSize : 2.5,
                            textHeight: dimPayload.TextHeight > 0 ? dimPayload.TextHeight : 3.0);

                        var dimSceneEntity = DimensionBuilder.BuildDimension(
                            entity.Handle,
                            dimDef,
                            blockTable: null,
                            layer: entity.LayerName,
                            cadStyle: cadStyle);
                        primitives.AddRange(dimSceneEntity.Geometry);
                    }
                }
                else if (entity.Points is { Count: >= 2 })
                {
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
                }
                break;

            case CadExtractedEntityType.Point when entity.Points is { Count: >= 1 }:
                double px = entity.Points[0].X;
                double py = entity.Points[0].Y;
                primitives.Add(new LinePrimitive(new WorldPoint2(px - 1, py), new WorldPoint2(px + 1, py)));
                primitives.Add(new LinePrimitive(new WorldPoint2(px, py - 1), new WorldPoint2(px, py + 1)));
                break;

            case CadExtractedEntityType.Unsupported or CadExtractedEntityType.Other:
                if (entity.Points is { Count: >= 1 })
                {
                    double ux = entity.Points[0].X;
                    double uy = entity.Points[0].Y;
                    primitives.Add(new LinePrimitive(new WorldPoint2(ux - 0.5, uy - 0.5), new WorldPoint2(ux + 0.5, uy + 0.5)));
                    primitives.Add(new LinePrimitive(new WorldPoint2(ux - 0.5, uy + 0.5), new WorldPoint2(ux + 0.5, uy - 0.5)));
                }
                break;
        }
    }
}
