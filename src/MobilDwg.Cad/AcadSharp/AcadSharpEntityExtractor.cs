using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.Tables;
using MobilDwg.Core.Coordinates;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Guards;
using MobilDwg.Core.Reading;

namespace MobilDwg.Cad.AcadSharp;

public static class AcadSharpEntityExtractor
{
    private static readonly Regex CadUnicodeRegex = new(@"\\U\+([0-9A-Fa-f]{4})", RegexOptions.Compiled);

    public static CadExtractedDocument Extract(
        ICadDocumentHandle handle,
        CadBudgetGuard? budgetGuard = null)
    {
        if (handle is not AcadSharpDocumentHandle acadHandle)
        {
            throw new ArgumentException("Handle was not created by the ACadSharp adapter.", nameof(handle));
        }

        budgetGuard ??= new CadBudgetGuard(CadResourceBudget.Default);
        var document = acadHandle.Document;
        var diagnostics = new List<CadExtractedDiagnostic>();

        // 1. Format & Version Resolution
        string format = acadHandle.Format switch
        {
            CadFormat.Dxf => "DXF",
            CadFormat.Dwg => "DWG",
            _ => "DWG"
        };
        string version = acadHandle.AcadVersion ?? document.Header?.Version.ToString() ?? "Unknown";

        // 2. Extract Layers
        var layerList = new List<CadExtractedLayer>(document.Layers.Count + 1);
        var layerNames = new HashSet<string>(document.Layers.Count + 1, StringComparer.OrdinalIgnoreCase);

        foreach (var layer in document.Layers)
        {
            var name = layer.Name ?? "0";
            if (layerNames.Add(name))
            {
                uint argb = 0xFFCCCCCC; // Default light gray
                short aci = layer.Color.Index;
                if (layer.Color.TrueColor != 0)
                {
                    argb = 0xFF000000 | (uint)(layer.Color.TrueColor & 0x00FFFFFF);
                }
                else if (aci > 0 && aci <= 256)
                {
                    argb = GetAciArgb(aci);
                }

                layerList.Add(new CadExtractedLayer(
                    name,
                    argb,
                    aci,
                    layer.IsOn,
                    false,
                    layer.LineType?.Name ?? "Continuous",
                    (short)layer.LineWeight));
            }
        }

        if (!layerNames.Contains("0"))
        {
            layerList.Insert(0, new CadExtractedLayer("0", 0xFFFFFFFF, 7, true));
        }

        // 3. Extract LineTypes
        var linetypes = new List<CadExtractedLinetype>();
        foreach (var lt in document.LineTypes)
        {
            var segments = lt.Segments?.Select(s => s.Length).ToArray() ?? Array.Empty<double>();
            linetypes.Add(new CadExtractedLinetype(lt.Name, lt.Description ?? string.Empty, segments));
        }

        // 4. Extract TextStyles
        var textStyles = new List<CadExtractedTextStyle>();
        foreach (var ts in document.TextStyles)
        {
            textStyles.Add(new CadExtractedTextStyle(
                ts.Name,
                ts.Filename ?? string.Empty,
                ts.Height,
                ts.Width > 0 ? ts.Width : 1.0,
                ts.ObliqueAngle));
        }

        // 5. Extract DimensionStyles
        var dimStyles = new List<CadExtractedDimensionStyle>();
        foreach (var ds in document.DimensionStyles)
        {
            dimStyles.Add(new CadExtractedDimensionStyle(
                ds.Name,
                ds.TextHeight > 0 ? ds.TextHeight : 2.5,
                ds.ArrowSize > 0 ? ds.ArrowSize : 2.5,
                ds.LinearScaleFactor > 0 ? ds.LinearScaleFactor : 1.0));
        }

        // 6. Extract Block Definitions Dictionary
        var blockDefs = new Dictionary<string, IReadOnlyList<CadExtractedEntity>>(StringComparer.OrdinalIgnoreCase);
        foreach (var blk in document.BlockRecords)
        {
            if (blk.Name.StartsWith('*') && !blk.Name.StartsWith("*D", StringComparison.OrdinalIgnoreCase))
            {
                // Skip anonymous system blocks other than dimensions
                continue;
            }

            var blkEntities = new List<CadExtractedEntity>();
            int blkOrder = 0;
            foreach (var ent in blk.Entities)
            {
                blkOrder++;
                var extracted = ExtractSingleEntity(
                    ent,
                    blkOrder,
                    blockOwner: blk.Name,
                    diagnostics: diagnostics,
                    budgetGuard: budgetGuard);

                if (extracted is not null)
                {
                    blkEntities.Add(extracted);
                }
            }
            blockDefs[blk.Name] = blkEntities.AsReadOnly();
        }

        // 7. Extract ModelSpace Entities (with block expansion and OCS transforms)
        var extractedEntities = new List<CadExtractedEntity>(document.Entities.Count * 2);
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        void UpdateBounds(double x, double y)
        {
            if (!double.IsFinite(x) || !double.IsFinite(y)) return;
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }

        int entityOrder = 0;
        foreach (var entity in document.Entities)
        {
            entityOrder++;

            if (!budgetGuard.CheckEntityCount(extractedEntities.Count + 1, out var quotaDiag))
            {
                diagnostics.Add(new CadExtractedDiagnostic(
                    quotaDiag!.Code,
                    quotaDiag.Severity.ToString(),
                    quotaDiag.Message));
                break;
            }

            if (entity is Insert insert)
            {
                // Expand block instance
                ExpandBlockInsert(
                    insert,
                    document,
                    parentInstancePath: $"doc:{insert.Handle:X}",
                    currentDepth: 1,
                    inheritedLayer: insert.Layer?.Name ?? "0",
                    inheritedColor: ResolveColor(insert),
                    extractedEntities: extractedEntities,
                    refOrder: ref entityOrder,
                    updateBounds: UpdateBounds,
                    diagnostics: diagnostics,
                    budgetGuard: budgetGuard,
                    activeBlockChain: new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
            else
            {
                var extracted = ExtractSingleEntity(
                    entity,
                    entityOrder,
                    blockOwner: null,
                    diagnostics: diagnostics,
                    budgetGuard: budgetGuard);

                if (extracted is not null)
                {
                    extractedEntities.Add(extracted);
                    UpdateEntityBounds(extracted, UpdateBounds);
                }
            }
        }

        if (minX > maxX)
        {
            minX = 0; minY = 0; maxX = 100; maxY = 100;
        }

        // Collect Layout Names
        var layoutNames = document.Layouts?.Select(l => l.Name).ToArray() ?? Array.Empty<string>();

        // Metadata
        var metadata = new CadExtractedMetadata(
            format,
            version,
            null,
            Units: document.Header?.InsUnits.ToString() ?? "Unitless",
            Measurement: 0.0);

        return new CadExtractedDocument(
            format,
            version,
            layerList.AsReadOnly(),
            extractedEntities.AsReadOnly(),
            minX, minY, maxX, maxY,
            metadata: metadata,
            linetypes: linetypes.AsReadOnly(),
            textStyles: textStyles.AsReadOnly(),
            dimensionStyles: dimStyles.AsReadOnly(),
            blockDefinitions: new ReadOnlyDictionary<string, IReadOnlyList<CadExtractedEntity>>(blockDefs),
            diagnostics: diagnostics.AsReadOnly(),
            layoutNames: layoutNames);
    }

    private static void ExpandBlockInsert(
        Insert insert,
        CadDocument document,
        string parentInstancePath,
        int currentDepth,
        string inheritedLayer,
        CadEntityColor inheritedColor,
        List<CadExtractedEntity> extractedEntities,
        ref int refOrder,
        Action<double, double> updateBounds,
        List<CadExtractedDiagnostic> diagnostics,
        CadBudgetGuard budgetGuard,
        HashSet<string> activeBlockChain)
    {
        var block = insert.Block;
        if (block is null || string.IsNullOrWhiteSpace(block.Name))
        {
            return;
        }

        if (!budgetGuard.CheckBlockDepth(currentDepth, out var depthDiag))
        {
            diagnostics.Add(new CadExtractedDiagnostic(
                depthDiag!.Code,
                depthDiag.Severity.ToString(),
                depthDiag.Message,
                insert.Handle.ToString("X")));
            return;
        }

        if (!activeBlockChain.Add(block.Name))
        {
            diagnostics.Add(new CadExtractedDiagnostic(
                "BLOCK_CYCLE_DETECTED",
                "Warning",
                $"Recursive block cycle detected for block '{block.Name}' at {parentInstancePath}.",
                insert.Handle.ToString("X")));
            return;
        }

        try
        {
            // Insert transform
            var ocs = OcsTransform.FromNormal(insert.Normal.X, insert.Normal.Y, insert.Normal.Z);
            double insX = insert.InsertPoint.X;
            double insY = insert.InsertPoint.Y;
            double insZ = insert.InsertPoint.Z;
            double scaleX = insert.XScale != 0 ? insert.XScale : 1.0;
            double scaleY = insert.YScale != 0 ? insert.YScale : 1.0;
            double scaleZ = insert.ZScale != 0 ? insert.ZScale : 1.0;
            double rotRad = insert.Rotation;
            double cos = Math.Cos(rotRad);
            double sin = Math.Sin(rotRad);

            // Block definition base point subtraction
            double bx = block.BlockEntity?.BasePoint.X ?? 0.0;
            double by = block.BlockEntity?.BasePoint.Y ?? 0.0;

            int colCount = Math.Max(1, (int)insert.ColumnCount);
            int rowCount = Math.Max(1, (int)insert.RowCount);
            double colSpacing = insert.ColumnSpacing;
            double rowSpacing = insert.RowSpacing;

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    double gridX = c * colSpacing;
                    double gridY = r * rowSpacing;
                    double rotGridX = (gridX * cos) - (gridY * sin);
                    double rotGridY = (gridX * sin) + (gridY * cos);

                    (double X, double Y) TransformPoint(double lx, double ly)
                    {
                        // 1. Base point subtraction
                        double px = lx - bx;
                        double py = ly - by;
                        // 2. Scale
                        double sx = px * scaleX;
                        double sy = py * scaleY;
                        // 3. Rotate
                        double rx = (sx * cos) - (sy * sin);
                        double ry = (sx * sin) + (sy * cos);
                        // 4. Translate via OCS + grid offset
                        var (ox, oy, _) = ocs.Transform(rx, ry, 0);
                        return (ox + insX + rotGridX, oy + insY + rotGridY);
                    }

                    foreach (var child in block.Entities)
                    {
                        refOrder++;

                        if (!budgetGuard.CheckEntityCount(extractedEntities.Count + 1, out var quotaDiag))
                        {
                            diagnostics.Add(new CadExtractedDiagnostic(
                                quotaDiag!.Code,
                                quotaDiag.Severity.ToString(),
                                quotaDiag.Message));
                            break;
                        }

                        if (child is Insert nestedInsert)
                        {
                            string nestedPath = (colCount > 1 || rowCount > 1)
                                ? $"{parentInstancePath}/{block.Name}:{nestedInsert.Handle:X}_c{c}_r{r}"
                                : $"{parentInstancePath}/{block.Name}:{nestedInsert.Handle:X}";

                            ExpandBlockInsert(
                                nestedInsert,
                                document,
                                parentInstancePath: nestedPath,
                                currentDepth: currentDepth + 1,
                                inheritedLayer: string.Equals(nestedInsert.Layer?.Name, "0", StringComparison.OrdinalIgnoreCase) ? inheritedLayer : nestedInsert.Layer?.Name ?? inheritedLayer,
                                inheritedColor: nestedInsert.Color.IsByBlock ? inheritedColor : ResolveColor(nestedInsert),
                                extractedEntities: extractedEntities,
                                refOrder: ref refOrder,
                                updateBounds: updateBounds,
                                diagnostics: diagnostics,
                                budgetGuard: budgetGuard,
                                activeBlockChain: activeBlockChain);
                            continue;
                        }

                        // Transform child entity
                        string suffix = (colCount > 1 || rowCount > 1) ? $"_c{c}_r{r}" : string.Empty;
                        string childHandleStr = $"{parentInstancePath}/{block.Name}:{insert.Handle:X}:{child.Handle:X}{suffix}";
                        string childLayer = string.Equals(child.Layer?.Name, "0", StringComparison.OrdinalIgnoreCase)
                            ? inheritedLayer
                            : child.Layer?.Name ?? inheritedLayer;
                        CadEntityColor childColor = child.Color.IsByBlock ? inheritedColor : ResolveColor(child);

                        var transformedChild = TransformAndExtractEntity(
                            child,
                            childHandleStr,
                            childLayer,
                            childColor,
                            refOrder,
                            block.Name,
                            TransformPoint,
                            scaleX,
                            scaleY,
                            rotRad,
                            diagnostics,
                            budgetGuard);

                        if (transformedChild is not null)
                        {
                            extractedEntities.Add(transformedChild);
                            UpdateEntityBounds(transformedChild, updateBounds);
                        }
                    }

                    // Expand visible attributes attached to this insert on root instance
                    if (c == 0 && r == 0 && insert.Attributes != null)
                    {
                        foreach (var attr in insert.Attributes)
                        {
                            if (attr.IsInvisible) continue;
                            refOrder++;
                            var (atx, aty) = TransformPoint(attr.InsertPoint.X, attr.InsertPoint.Y);
                            string attrText = DecodeCadText(attr.Value);
                            double ah = (attr.Height > 0 ? attr.Height : 10.0) * Math.Abs(scaleY);
                            double arot = attr.Rotation + rotRad;
                            string attrHandle = $"{parentInstancePath}/{block.Name}:{insert.Handle:X}:ATTR:{attr.Handle:X}";

                            var attrEntity = new CadExtractedEntity(
                                attrHandle,
                                inheritedLayer,
                                CadExtractedEntityType.Text,
                                inheritedColor,
                                sourceOrder: refOrder,
                                blockOwner: block.Name,
                                payload: new CadTextPayload(attrText, new CadPoint3D(atx, aty), ah, arot, attr.Style?.Name),
                                points: new[] { new CadExtractedPoint(atx, aty) },
                                text: attrText,
                                textHeight: ah,
                                rotation: arot);

                            extractedEntities.Add(attrEntity);
                            UpdateEntityBounds(attrEntity, updateBounds);
                        }
                    }
                }
            }
        }
        finally
        {
            activeBlockChain.Remove(block.Name);
        }
    }

    private static CadExtractedEntity? ExtractSingleEntity(
        Entity entity,
        int sourceOrder,
        string? blockOwner,
        List<CadExtractedDiagnostic> diagnostics,
        CadBudgetGuard budgetGuard)
    {
        string handleStr = entity.Handle.ToString("X");
        string layer = entity.Layer?.Name ?? "0";
        var color = ResolveColor(entity);
        var lineweight = new CadEntityLineweight((short)entity.LineWeight, entity.LineWeight == LineWeightType.ByLayer, entity.LineWeight == LineWeightType.ByBlock);
        var transparency = new CadEntityTransparency((byte)Math.Clamp(entity.Transparency.Value, (short)0, (short)255), entity.Transparency.IsByLayer, entity.Transparency.IsByBlock);
        string? linetype = entity.LineType?.Name;
        double linetypeScale = entity.LineTypeScale > 0 ? entity.LineTypeScale : 1.0;

        if (entity is Line line)
        {
            // Lines are in WCS
            var p1 = new CadExtractedPoint(line.StartPoint.X, line.StartPoint.Y);
            var p2 = new CadExtractedPoint(line.EndPoint.X, line.EndPoint.Y);
            var payload = new CadLinePayload(
                new CadPoint3D(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z),
                new CadPoint3D(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z),
                line.Thickness);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Line, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { p1, p2 });
        }
        else if (entity is Circle circle)
        {
            var ocs = OcsTransform.FromNormal(circle.Normal.X, circle.Normal.Y, circle.Normal.Z);
            var (cx, cy) = ocs.Transform2D(circle.Center.X, circle.Center.Y, circle.Center.Z);
            var payload = new CadCirclePayload(
                new CadPoint3D(cx, cy, circle.Center.Z),
                circle.Radius,
                circle.Thickness);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Circle, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { new CadExtractedPoint(cx, cy) },
                radius: circle.Radius,
                startAngle: 0,
                endAngle: Math.PI * 2);
        }
        else if (entity is Arc arc)
        {
            var ocs = OcsTransform.FromNormal(arc.Normal.X, arc.Normal.Y, arc.Normal.Z);
            var (cx, cy) = ocs.Transform2D(arc.Center.X, arc.Center.Y, arc.Center.Z);
            var payload = new CadArcPayload(
                new CadPoint3D(cx, cy, arc.Center.Z),
                arc.Radius, arc.StartAngle, arc.EndAngle, arc.Thickness);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Arc, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { new CadExtractedPoint(cx, cy) },
                radius: arc.Radius,
                startAngle: arc.StartAngle,
                endAngle: arc.EndAngle);
        }
        else if (entity is Ellipse ellipse)
        {
            var ocs = OcsTransform.FromNormal(ellipse.Normal.X, ellipse.Normal.Y, ellipse.Normal.Z);
            var (cx, cy) = ocs.Transform2D(ellipse.Center.X, ellipse.Center.Y, ellipse.Center.Z);
            var (mx, my) = ocs.Transform2D(ellipse.MajorAxisEndPoint.X, ellipse.MajorAxisEndPoint.Y, ellipse.MajorAxisEndPoint.Z);
            var payload = new CadEllipsePayload(
                new CadPoint3D(cx, cy, ellipse.Center.Z),
                new CadPoint3D(mx, my, ellipse.MajorAxisEndPoint.Z),
                ellipse.RadiusRatio, ellipse.StartParameter, ellipse.EndParameter);

            double r = Math.Sqrt(mx * mx + my * my);
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Ellipse, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { new CadExtractedPoint(cx, cy) },
                radius: r,
                startAngle: ellipse.StartParameter,
                endAngle: ellipse.EndParameter);
        }
        else if (entity is LwPolyline lwPoly)
        {
            var ocs = OcsTransform.FromNormal(lwPoly.Normal.X, lwPoly.Normal.Y, lwPoly.Normal.Z);
            var vertices = new List<CadExtractedVertex>(lwPoly.Vertices.Count);
            foreach (var v in lwPoly.Vertices)
            {
                var (wx, wy) = ocs.Transform2D(v.Location.X, v.Location.Y, lwPoly.Elevation);
                vertices.Add(new CadExtractedVertex(wx, wy, v.Bulge, v.StartWidth, v.EndWidth));
            }

            var payload = new CadPolylinePayload(vertices, lwPoly.IsClosed, lwPoly.Elevation, lwPoly.Thickness);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Polyline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                vertices: vertices);
        }
        else if (entity is Polyline2D poly2D)
        {
            var ocs = OcsTransform.FromNormal(poly2D.Normal.X, poly2D.Normal.Y, poly2D.Normal.Z);
            var vertices = new List<CadExtractedVertex>(poly2D.Vertices.Count);
            foreach (var v in poly2D.Vertices)
            {
                var (wx, wy) = ocs.Transform2D(v.Location.X, v.Location.Y, poly2D.Elevation);
                vertices.Add(new CadExtractedVertex(wx, wy, v.Bulge, v.StartWidth, v.EndWidth));
            }

            var payload = new CadPolylinePayload(vertices, poly2D.IsClosed, poly2D.Elevation, poly2D.Thickness);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Polyline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                vertices: vertices);
        }
        else if (entity is Polyline3D poly3D)
        {
            var vertices = new List<CadExtractedVertex>(poly3D.Vertices.Count);
            foreach (var v in poly3D.Vertices)
            {
                vertices.Add(new CadExtractedVertex(v.Location.X, v.Location.Y));
            }

            var payload = new CadPolylinePayload(vertices, poly3D.IsClosed);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Polyline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                vertices: vertices);
        }
        else if (entity is Spline spline)
        {
            var ctrlPts = spline.ControlPoints?.Select(p => new CadPoint3D(p.X, p.Y, p.Z)).ToArray() ?? Array.Empty<CadPoint3D>();
            var fitPts = spline.FitPoints?.Select(p => new CadPoint3D(p.X, p.Y, p.Z)).ToArray() ?? Array.Empty<CadPoint3D>();
            var knots = spline.Knots?.ToArray() ?? Array.Empty<double>();
            var weights = spline.Weights?.ToArray() ?? Array.Empty<double>();

            var payload = new CadSplinePayload(spline.Degree, spline.IsClosed, ctrlPts, fitPts, knots, weights.Length > 0 ? weights : null);

            var vertices = (ctrlPts.Length > 0 ? ctrlPts : fitPts).Select(p => new CadExtractedVertex(p.X, p.Y)).ToArray();

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Spline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                vertices: vertices);
        }
        else if (entity is TextEntity text)
        {
            var ocs = OcsTransform.FromNormal(text.Normal.X, text.Normal.Y, text.Normal.Z);
            var (tx, ty) = ocs.Transform2D(text.InsertPoint.X, text.InsertPoint.Y);
            string decodedVal = DecodeCadText(text.Value);

            if (decodedVal.Length > budgetGuard.Budget.MaxTextLength)
            {
                budgetGuard.CheckTextLength(decodedVal.Length, out var textDiag);
                if (textDiag is not null)
                {
                    diagnostics.Add(new CadExtractedDiagnostic(textDiag.Code, textDiag.Severity.ToString(), textDiag.Message, handleStr));
                }
                decodedVal = decodedVal.Substring(0, budgetGuard.Budget.MaxTextLength);
            }

            var payload = new CadTextPayload(
                decodedVal,
                new CadPoint3D(tx, ty, text.InsertPoint.Z),
                text.Height > 0 ? text.Height : 10.0,
                text.Rotation,
                text.Style?.Name);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Text, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { new CadExtractedPoint(tx, ty) },
                text: decodedVal,
                textHeight: text.Height > 0 ? text.Height : 10.0,
                rotation: text.Rotation);
        }
        else if (entity is MText mtext)
        {
            var ocs = OcsTransform.FromNormal(mtext.Normal.X, mtext.Normal.Y, mtext.Normal.Z);
            var (tx, ty) = ocs.Transform2D(mtext.InsertPoint.X, mtext.InsertPoint.Y);
            string cleanVal = CleanMText(mtext.Value);

            if (cleanVal.Length > budgetGuard.Budget.MaxTextLength)
            {
                budgetGuard.CheckTextLength(cleanVal.Length, out var textDiag);
                if (textDiag is not null)
                {
                    diagnostics.Add(new CadExtractedDiagnostic(textDiag.Code, textDiag.Severity.ToString(), textDiag.Message, handleStr));
                }
                cleanVal = cleanVal.Substring(0, budgetGuard.Budget.MaxTextLength);
            }

            var payload = new CadTextPayload(
                cleanVal,
                new CadPoint3D(tx, ty, mtext.InsertPoint.Z),
                mtext.Height > 0 ? mtext.Height : 10.0,
                mtext.Rotation,
                mtext.Style?.Name);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.MText, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { new CadExtractedPoint(tx, ty) },
                text: cleanVal,
                textHeight: mtext.Height > 0 ? mtext.Height : 10.0,
                rotation: mtext.Rotation);
        }
        else if (entity is Dimension dim)
        {
            string? dimText = DecodeCadText(dim.Text);
            var defPt = new CadPoint3D(dim.DefinitionPoint.X, dim.DefinitionPoint.Y, dim.DefinitionPoint.Z);
            var midPt = new CadPoint3D(dim.InsertionPoint.X, dim.InsertionPoint.Y, dim.InsertionPoint.Z);

            var payload = new CadDimensionPayload(
                dimText,
                defPt,
                midPt,
                dim.GetType().Name,
                dim.Style?.Name);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Dimension, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { new CadExtractedPoint(defPt.X, defPt.Y), new CadExtractedPoint(midPt.X, midPt.Y) },
                text: dimText);
        }
        else if (entity is Hatch hatch)
        {
            var ocs = OcsTransform.FromNormal(hatch.Normal.X, hatch.Normal.Y, hatch.Normal.Z);
            var loops = new List<IReadOnlyList<CadExtractedVertex>>();
            int totalSegs = 0;

            foreach (var path in hatch.Paths)
            {
                var loopVertices = new List<CadExtractedVertex>();
                if (path.Edges.Count > 0)
                {
                    totalSegs += path.Edges.Count;
                    foreach (var edge in path.Edges)
                    {
                        if (edge is Hatch.BoundaryPath.Line lineEdge)
                        {
                            var (sx, sy) = ocs.Transform2D(lineEdge.Start.X, lineEdge.Start.Y, hatch.Elevation);
                            var (ex, ey) = ocs.Transform2D(lineEdge.End.X, lineEdge.End.Y, hatch.Elevation);
                            loopVertices.Add(new CadExtractedVertex(sx, sy));
                            loopVertices.Add(new CadExtractedVertex(ex, ey));
                        }
                        else if (edge is Hatch.BoundaryPath.Arc arcEdge)
                        {
                            var (cx, cy) = ocs.Transform2D(arcEdge.Center.X, arcEdge.Center.Y, hatch.Elevation);
                            loopVertices.Add(new CadExtractedVertex(cx, cy));
                        }
                        else if (edge is Hatch.BoundaryPath.Polyline polyEdge)
                        {
                            foreach (var v in polyEdge.Vertices)
                            {
                                var (px, py) = ocs.Transform2D(v.X, v.Y, hatch.Elevation);
                                loopVertices.Add(new CadExtractedVertex(px, py));
                            }
                        }
                    }
                }

                if (loopVertices.Count > 0)
                {
                    loops.Add(loopVertices.AsReadOnly());
                }
            }

            if (!budgetGuard.CheckHatchSegments(totalSegs, out var hatchDiag))
            {
                diagnostics.Add(new CadExtractedDiagnostic(
                    hatchDiag!.Code, hatchDiag.Severity.ToString(), hatchDiag.Message, handleStr));
            }

            string patternName = hatch.Pattern?.Name ?? "SOLID";
            var payload = new CadHatchPayload(
                patternName,
                hatch.IsSolid,
                hatch.PatternAngle,
                hatch.PatternScale,
                loops.AsReadOnly());

            var flatVertices = loops.SelectMany(l => l).ToArray();
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Hatch, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                vertices: flatVertices);
        }
        else if (entity is Solid solid)
        {
            var ocs = OcsTransform.FromNormal(solid.Normal.X, solid.Normal.Y, solid.Normal.Z);
            var (p1x, p1y) = ocs.Transform2D(solid.FirstCorner.X, solid.FirstCorner.Y, solid.FirstCorner.Z);
            var (p2x, p2y) = ocs.Transform2D(solid.SecondCorner.X, solid.SecondCorner.Y, solid.SecondCorner.Z);
            var (p3x, p3y) = ocs.Transform2D(solid.ThirdCorner.X, solid.ThirdCorner.Y, solid.ThirdCorner.Z);
            var (p4x, p4y) = ocs.Transform2D(solid.FourthCorner.X, solid.FourthCorner.Y, solid.FourthCorner.Z);

            var payload = new CadSolidPayload(
                new CadPoint3D(p1x, p1y, solid.FirstCorner.Z),
                new CadPoint3D(p2x, p2y, solid.SecondCorner.Z),
                new CadPoint3D(p3x, p3y, solid.ThirdCorner.Z),
                new CadPoint3D(p4x, p4y, solid.FourthCorner.Z));

            bool isTri = Math.Abs(p4x - p3x) < 1e-9 && Math.Abs(p4y - p3y) < 1e-9;
            var vertices = isTri
                ? new[]
                {
                    new CadExtractedVertex(p1x, p1y),
                    new CadExtractedVertex(p2x, p2y),
                    new CadExtractedVertex(p3x, p3y)
                }
                : new[]
                {
                    new CadExtractedVertex(p1x, p1y),
                    new CadExtractedVertex(p2x, p2y),
                    new CadExtractedVertex(p4x, p4y),
                    new CadExtractedVertex(p3x, p3y)
                };

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Solid, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                vertices: vertices);
        }
        else if (entity is Face3D face)
        {
            var p1 = new CadPoint3D(face.FirstCorner.X, face.FirstCorner.Y, face.FirstCorner.Z);
            var p2 = new CadPoint3D(face.SecondCorner.X, face.SecondCorner.Y, face.SecondCorner.Z);
            var p3 = new CadPoint3D(face.ThirdCorner.X, face.ThirdCorner.Y, face.ThirdCorner.Z);
            var p4 = new CadPoint3D(face.FourthCorner.X, face.FourthCorner.Y, face.FourthCorner.Z);

            bool isTri = Math.Abs(p4.X - p3.X) < 1e-9 && Math.Abs(p4.Y - p3.Y) < 1e-9 && Math.Abs(p4.Z - p3.Z) < 1e-9;
            var vertices = isTri
                ? new[]
                {
                    new CadExtractedVertex(p1.X, p1.Y),
                    new CadExtractedVertex(p2.X, p2.Y),
                    new CadExtractedVertex(p3.X, p3.Y)
                }
                : new[]
                {
                    new CadExtractedVertex(p1.X, p1.Y),
                    new CadExtractedVertex(p2.X, p2.Y),
                    new CadExtractedVertex(p3.X, p3.Y),
                    new CadExtractedVertex(p4.X, p4.Y)
                };

            var payload = new CadSolidPayload(p1, p2, p3, p4);
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Solid, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                vertices: vertices);
        }
        else if (entity is Point pt)
        {
            var payload = new CadPointPayload(new CadPoint3D(pt.Location.X, pt.Location.Y, pt.Location.Z));
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Point, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload,
                points: new[] { new CadExtractedPoint(pt.Location.X, pt.Location.Y) });
        }
        else
        {
            // Unsupported entity: record diagnostic with handle and type
            string typeName = entity.ObjectName ?? entity.GetType().Name;
            diagnostics.Add(new CadExtractedDiagnostic(
                $"UNSUPPORTED_ENTITY_{typeName.ToUpperInvariant()}",
                "Info",
                $"Entity of type '{typeName}' is retained as compatibility evidence.",
                handleStr));

            var payload = new CadUnsupportedPayload(typeName, "Retained as compatibility evidence.");
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Unsupported, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockOwner,
                payload: payload);
        }
    }

    private static CadExtractedEntity? TransformAndExtractEntity(
        Entity child,
        string handleStr,
        string layer,
        CadEntityColor color,
        int sourceOrder,
        string blockName,
        Func<double, double, (double X, double Y)> transform,
        double scaleX,
        double scaleY,
        double rotation,
        List<CadExtractedDiagnostic> diagnostics,
        CadBudgetGuard budgetGuard)
    {
        var lineweight = new CadEntityLineweight((short)child.LineWeight, child.LineWeight == LineWeightType.ByLayer, child.LineWeight == LineWeightType.ByBlock);
        var transparency = new CadEntityTransparency((byte)Math.Clamp(child.Transparency.Value, (short)0, (short)255), child.Transparency.IsByLayer, child.Transparency.IsByBlock);
        string? linetype = child.LineType?.Name;
        double linetypeScale = child.LineTypeScale > 0 ? child.LineTypeScale : 1.0;
        bool isMirrored = (scaleX * scaleY) < 0;

        if (child is Line line)
        {
            var (p1x, p1y) = transform(line.StartPoint.X, line.StartPoint.Y);
            var (p2x, p2y) = transform(line.EndPoint.X, line.EndPoint.Y);
            var payload = new CadLinePayload(new CadPoint3D(p1x, p1y), new CadPoint3D(p2x, p2y));

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Line, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                points: new[] { new CadExtractedPoint(p1x, p1y), new CadExtractedPoint(p2x, p2y) });
        }
        else if (child is Circle circle)
        {
            var (cx, cy) = transform(circle.Center.X, circle.Center.Y);
            double absSx = Math.Abs(scaleX);
            double absSy = Math.Abs(scaleY);

            if (Math.Abs(absSx - absSy) < 1e-6)
            {
                // Uniform scale -> Circle
                double r = circle.Radius * absSx;
                var payload = new CadCirclePayload(new CadPoint3D(cx, cy), r);

                return new CadExtractedEntity(
                    handleStr, layer, CadExtractedEntityType.Circle, color,
                    sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                    linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                    payload: payload,
                    points: new[] { new CadExtractedPoint(cx, cy) },
                    radius: r,
                    startAngle: 0,
                    endAngle: Math.PI * 2);
            }
            else
            {
                // Non-uniform scale -> Ellipse
                double rx = circle.Radius * absSx;
                double ry = circle.Radius * absSy;
                double rmaj = Math.Max(rx, ry);
                double rmin = Math.Min(rx, ry);
                double ratio = rmaj > 0 ? rmin / rmaj : 1.0;
                double rot = rx >= ry ? rotation : rotation + (Math.PI / 2.0);
                double mx = rmaj * Math.Cos(rot);
                double my = rmaj * Math.Sin(rot);

                var payload = new CadEllipsePayload(
                    new CadPoint3D(cx, cy),
                    new CadPoint3D(mx, my),
                    ratio,
                    0,
                    Math.PI * 2);

                return new CadExtractedEntity(
                    handleStr, layer, CadExtractedEntityType.Ellipse, color,
                    sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                    linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                    payload: payload,
                    points: new[] { new CadExtractedPoint(cx, cy) },
                    radius: rmaj,
                    startAngle: 0,
                    endAngle: Math.PI * 2,
                    rotation: rot);
            }
        }
        else if (child is Arc arc)
        {
            var (cx, cy) = transform(arc.Center.X, arc.Center.Y);
            double absSx = Math.Abs(scaleX);
            double absSy = Math.Abs(scaleY);

            if (Math.Abs(absSx - absSy) < 1e-6)
            {
                // Uniform scale -> Arc
                double r = arc.Radius * absSx;
                double sa = arc.StartAngle + rotation;
                double ea = arc.EndAngle + rotation;

                if (isMirrored)
                {
                    double sweep = arc.EndAngle - arc.StartAngle;
                    if (sweep <= 0) sweep += Math.PI * 2;
                    sa = Math.PI - ea;
                    ea = sa + sweep;
                }

                var payload = new CadArcPayload(new CadPoint3D(cx, cy), r, sa, ea);
                return new CadExtractedEntity(
                    handleStr, layer, CadExtractedEntityType.Arc, color,
                    sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                    linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                    payload: payload,
                    points: new[] { new CadExtractedPoint(cx, cy) },
                    radius: r,
                    startAngle: sa,
                    endAngle: ea);
            }
            else
            {
                // Non-uniform scale -> tessellated curved polyline
                int segments = 32;
                double sweep = arc.EndAngle - arc.StartAngle;
                if (sweep <= 0) sweep += Math.PI * 2;
                var arcVerts = new List<CadExtractedVertex>(segments + 1);

                for (int i = 0; i <= segments; i++)
                {
                    double t = (double)i / segments;
                    double a = arc.StartAngle + (sweep * t);
                    double lx = arc.Center.X + (arc.Radius * Math.Cos(a));
                    double ly = arc.Center.Y + (arc.Radius * Math.Sin(a));
                    var (wx, wy) = transform(lx, ly);
                    arcVerts.Add(new CadExtractedVertex(wx, wy));
                }

                var payload = new CadPolylinePayload(arcVerts, IsClosed: false);
                return new CadExtractedEntity(
                    handleStr, layer, CadExtractedEntityType.Polyline, color,
                    sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                    linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                    payload: payload,
                    vertices: arcVerts);
            }
        }
        else if (child is Ellipse ellipse)
        {
            var (cx, cy) = transform(ellipse.Center.X, ellipse.Center.Y);
            var (mx, my) = transform(ellipse.Center.X + ellipse.MajorAxisEndPoint.X, ellipse.Center.Y + ellipse.MajorAxisEndPoint.Y);
            double majX = mx - cx;
            double majY = my - cy;
            double r = Math.Sqrt((majX * majX) + (majY * majY));
            double rot = Math.Atan2(majY, majX);

            var payload = new CadEllipsePayload(
                new CadPoint3D(cx, cy),
                new CadPoint3D(majX, majY),
                ellipse.RadiusRatio,
                ellipse.StartParameter,
                ellipse.EndParameter);

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Ellipse, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                points: new[] { new CadExtractedPoint(cx, cy) },
                radius: r,
                startAngle: ellipse.StartParameter,
                endAngle: ellipse.EndParameter,
                rotation: rot);
        }
        else if (child is LwPolyline lwPoly)
        {
            var vertices = new List<CadExtractedVertex>(lwPoly.Vertices.Count);
            foreach (var v in lwPoly.Vertices)
            {
                var (wx, wy) = transform(v.Location.X, v.Location.Y);
                double b = isMirrored ? -v.Bulge : v.Bulge;
                vertices.Add(new CadExtractedVertex(wx, wy, b, v.StartWidth * Math.Abs(scaleX), v.EndWidth * Math.Abs(scaleX)));
            }

            var payload = new CadPolylinePayload(vertices, lwPoly.IsClosed);
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Polyline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                vertices: vertices);
        }
        else if (child is Polyline2D poly2D)
        {
            var vertices = new List<CadExtractedVertex>(poly2D.Vertices.Count);
            foreach (var v in poly2D.Vertices)
            {
                var (wx, wy) = transform(v.Location.X, v.Location.Y);
                double b = isMirrored ? -v.Bulge : v.Bulge;
                vertices.Add(new CadExtractedVertex(wx, wy, b, v.StartWidth * Math.Abs(scaleX), v.EndWidth * Math.Abs(scaleX)));
            }

            var payload = new CadPolylinePayload(vertices, poly2D.IsClosed);
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Polyline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                vertices: vertices);
        }
        else if (child is Polyline3D poly3D)
        {
            var vertices = new List<CadExtractedVertex>(poly3D.Vertices.Count);
            foreach (var v in poly3D.Vertices)
            {
                var (wx, wy) = transform(v.Location.X, v.Location.Y);
                vertices.Add(new CadExtractedVertex(wx, wy));
            }

            var payload = new CadPolylinePayload(vertices, poly3D.IsClosed);
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Polyline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                vertices: vertices);
        }
        else if (child is Spline spline)
        {
            var ctrlPts = spline.ControlPoints?.Select(p =>
            {
                var (tx, ty) = transform(p.X, p.Y);
                return new CadPoint3D(tx, ty, p.Z);
            }).ToArray() ?? Array.Empty<CadPoint3D>();

            var fitPts = spline.FitPoints?.Select(p =>
            {
                var (tx, ty) = transform(p.X, p.Y);
                return new CadPoint3D(tx, ty, p.Z);
            }).ToArray() ?? Array.Empty<CadPoint3D>();

            var knots = spline.Knots?.ToArray() ?? Array.Empty<double>();
            var weights = spline.Weights?.ToArray() ?? Array.Empty<double>();

            var payload = new CadSplinePayload(spline.Degree, spline.IsClosed, ctrlPts, fitPts, knots, weights.Length > 0 ? weights : null);
            var vertices = (ctrlPts.Length > 0 ? ctrlPts : fitPts).Select(p => new CadExtractedVertex(p.X, p.Y)).ToArray();

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Spline, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                vertices: vertices);
        }
        else if (child is TextEntity text)
        {
            var (tx, ty) = transform(text.InsertPoint.X, text.InsertPoint.Y);
            double h = (text.Height > 0 ? text.Height : 10.0) * Math.Abs(scaleY);
            double rot = text.Rotation + rotation;
            string decodedVal = DecodeCadText(text.Value);

            var payload = new CadTextPayload(decodedVal, new CadPoint3D(tx, ty), h, rot, text.Style?.Name);
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Text, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                points: new[] { new CadExtractedPoint(tx, ty) },
                text: decodedVal,
                textHeight: h,
                rotation: rot);
        }
        else if (child is MText mtext)
        {
            var (tx, ty) = transform(mtext.InsertPoint.X, mtext.InsertPoint.Y);
            double h = (mtext.Height > 0 ? mtext.Height : 10.0) * Math.Abs(scaleY);
            double rot = mtext.Rotation + rotation;
            string cleanVal = CleanMText(mtext.Value);

            var payload = new CadTextPayload(cleanVal, new CadPoint3D(tx, ty), h, rot, mtext.Style?.Name);
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.MText, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                points: new[] { new CadExtractedPoint(tx, ty) },
                text: cleanVal,
                textHeight: h,
                rotation: rot);
        }
        else if (child is Solid solid)
        {
            var (p1x, p1y) = transform(solid.FirstCorner.X, solid.FirstCorner.Y);
            var (p2x, p2y) = transform(solid.SecondCorner.X, solid.SecondCorner.Y);
            var (p3x, p3y) = transform(solid.ThirdCorner.X, solid.ThirdCorner.Y);
            var (p4x, p4y) = transform(solid.FourthCorner.X, solid.FourthCorner.Y);

            bool isTri = Math.Abs(p4x - p3x) < 1e-9 && Math.Abs(p4y - p3y) < 1e-9;
            var vertices = isTri
                ? new[]
                {
                    new CadExtractedVertex(p1x, p1y),
                    new CadExtractedVertex(p2x, p2y),
                    new CadExtractedVertex(p3x, p3y)
                }
                : new[]
                {
                    new CadExtractedVertex(p1x, p1y),
                    new CadExtractedVertex(p2x, p2y),
                    new CadExtractedVertex(p4x, p4y),
                    new CadExtractedVertex(p3x, p3y)
                };

            var payload = new CadSolidPayload(
                new CadPoint3D(p1x, p1y, solid.FirstCorner.Z),
                new CadPoint3D(p2x, p2y, solid.SecondCorner.Z),
                new CadPoint3D(p3x, p3y, solid.ThirdCorner.Z),
                new CadPoint3D(p4x, p4y, solid.FourthCorner.Z));

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Solid, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                vertices: vertices);
        }
        else if (child is Face3D face)
        {
            var (p1x, p1y) = transform(face.FirstCorner.X, face.FirstCorner.Y);
            var (p2x, p2y) = transform(face.SecondCorner.X, face.SecondCorner.Y);
            var (p3x, p3y) = transform(face.ThirdCorner.X, face.ThirdCorner.Y);
            var (p4x, p4y) = transform(face.FourthCorner.X, face.FourthCorner.Y);

            bool isTri = Math.Abs(p4x - p3x) < 1e-9 && Math.Abs(p4y - p3y) < 1e-9;
            var vertices = isTri
                ? new[]
                {
                    new CadExtractedVertex(p1x, p1y),
                    new CadExtractedVertex(p2x, p2y),
                    new CadExtractedVertex(p3x, p3y)
                }
                : new[]
                {
                    new CadExtractedVertex(p1x, p1y),
                    new CadExtractedVertex(p2x, p2y),
                    new CadExtractedVertex(p3x, p3y),
                    new CadExtractedVertex(p4x, p4y)
                };

            var payload = new CadSolidPayload(
                new CadPoint3D(p1x, p1y, face.FirstCorner.Z),
                new CadPoint3D(p2x, p2y, face.SecondCorner.Z),
                new CadPoint3D(p3x, p3y, face.ThirdCorner.Z),
                new CadPoint3D(p4x, p4y, face.FourthCorner.Z));

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Solid, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                vertices: vertices);
        }
        else if (child is Point pt)
        {
            var (px, py) = transform(pt.Location.X, pt.Location.Y);
            var payload = new CadPointPayload(new CadPoint3D(px, py, pt.Location.Z));
            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Point, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                points: new[] { new CadExtractedPoint(px, py) });
        }
        else if (child is Hatch hatch)
        {
            var loops = new List<IReadOnlyList<CadExtractedVertex>>();
            foreach (var path in hatch.Paths)
            {
                var loopVertices = new List<CadExtractedVertex>();
                foreach (var edge in path.Edges)
                {
                    if (edge is Hatch.BoundaryPath.Line lineEdge)
                    {
                        var (sx, sy) = transform(lineEdge.Start.X, lineEdge.Start.Y);
                        var (ex, ey) = transform(lineEdge.End.X, lineEdge.End.Y);
                        loopVertices.Add(new CadExtractedVertex(sx, sy));
                        loopVertices.Add(new CadExtractedVertex(ex, ey));
                    }
                    else if (edge is Hatch.BoundaryPath.Polyline polyEdge)
                    {
                        foreach (var v in polyEdge.Vertices)
                        {
                            var (px, py) = transform(v.X, v.Y);
                            loopVertices.Add(new CadExtractedVertex(px, py));
                        }
                    }
                }
                if (loopVertices.Count > 0)
                {
                    loops.Add(loopVertices.AsReadOnly());
                }
            }

            var flatVertices = loops.SelectMany(l => l).ToArray();
            var payload = new CadHatchPayload(
                hatch.Pattern?.Name ?? "SOLID",
                hatch.IsSolid,
                hatch.PatternAngle,
                hatch.PatternScale,
                loops.AsReadOnly());

            return new CadExtractedEntity(
                handleStr, layer, CadExtractedEntityType.Hatch, color,
                sourceOrder: sourceOrder, lineweight: lineweight, transparency: transparency,
                linetype: linetype, linetypeScale: linetypeScale, blockOwner: blockName,
                payload: payload,
                vertices: flatVertices);
        }

        return null;
    }

    private static void UpdateEntityBounds(CadExtractedEntity entity, Action<double, double> updateBounds)
    {
        if (entity.Points is { Count: > 0 })
        {
            foreach (var p in entity.Points)
            {
                if (entity.Radius > 0)
                {
                    updateBounds(p.X - entity.Radius, p.Y - entity.Radius);
                    updateBounds(p.X + entity.Radius, p.Y + entity.Radius);
                }
                else
                {
                    updateBounds(p.X, p.Y);
                }
            }
        }
        if (entity.Vertices is { Count: > 0 })
        {
            foreach (var v in entity.Vertices)
            {
                updateBounds(v.X, v.Y);
            }
        }
    }

    private static CadEntityColor ResolveColor(Entity entity)
    {
        if (entity.Color.IsByBlock)
        {
            return CadEntityColor.ByBlock;
        }

        if (entity.Color.TrueColor != 0)
        {
            uint argb = 0xFF000000 | (uint)(entity.Color.TrueColor & 0x00FFFFFF);
            return CadEntityColor.FromTrueColor(argb);
        }

        if (entity.Color.Index > 0 && entity.Color.Index <= 256)
        {
            if (entity.Color.Index == 256)
            {
                return CadEntityColor.ByLayer;
            }
            return CadEntityColor.FromAci(entity.Color.Index);
        }

        return CadEntityColor.ByLayer;
    }

    private static readonly uint[] s_aciPalette = InitializeAciPalette();

    public static uint GetAciArgb(short aci)
    {
        if (aci is < 0 or > 256) return 0xFFFFFFFF;
        return s_aciPalette[aci];
    }

    private static uint[] InitializeAciPalette()
    {
        var p = new uint[257];
        p[0] = 0xFF000000u; // ByBlock
        p[1] = 0xFFFF0000u; // Red
        p[2] = 0xFFFFFF00u; // Yellow
        p[3] = 0xFF00FF00u; // Green
        p[4] = 0xFF00FFFFu; // Cyan
        p[5] = 0xFF0000FFu; // Blue
        p[6] = 0xFFFF00FFu; // Magenta
        p[7] = 0xFFFFFFFFu; // White
        p[8] = 0xFF808080u; // Dark Gray
        p[9] = 0xFFC0C0C0u; // Light Gray

        for (int i = 10; i < 250; i++)
        {
            int hueIndex = (i - 10) / 10;
            int shadeIndex = (i - 10) % 10;
            double hue = hueIndex * 15.0;

            double lightness = shadeIndex switch
            {
                0 => 1.0,
                1 => 0.9,
                2 => 0.8,
                3 => 0.7,
                4 => 0.6,
                5 => 0.5,
                6 => 0.4,
                7 => 0.3,
                8 => 0.2,
                9 => 0.1,
                _ => 0.5,
            };

            p[i] = HsvToRgb(hue, 1.0, lightness);
        }

        p[250] = 0xFF333333u;
        p[251] = 0xFF505050u;
        p[252] = 0xFF696969u;
        p[253] = 0xFF828282u;
        p[254] = 0xFFBEBEBEu;
        p[255] = 0xFFFFFFFFu;
        p[256] = 0xFFFFFFFFu; // ByLayer

        return p;
    }

    private static uint HsvToRgb(double hue, double sat, double val)
    {
        double c = val * sat;
        double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
        double m = val - c;

        double r1 = 0, g1 = 0, b1 = 0;
        if (hue < 60) { r1 = c; g1 = x; }
        else if (hue < 120) { r1 = x; g1 = c; }
        else if (hue < 180) { g1 = c; b1 = x; }
        else if (hue < 240) { g1 = x; b1 = c; }
        else if (hue < 300) { r1 = x; b1 = c; }
        else { r1 = c; b1 = x; }

        byte r = (byte)Math.Clamp((int)((r1 + m) * 255), 0, 255);
        byte g = (byte)Math.Clamp((int)((g1 + m) * 255), 0, 255);
        byte b = (byte)Math.Clamp((int)((b1 + m) * 255), 0, 255);

        return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b;
    }

    public static string DecodeCadText(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        if (!input.Contains("\\U+", StringComparison.OrdinalIgnoreCase))
        {
            return input;
        }

        return CadUnicodeRegex.Replace(input, match =>
        {
            if (ushort.TryParse(match.Groups[1].ValueSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort codePoint))
            {
                return ((char)codePoint).ToString();
            }
            return match.Value;
        });
    }

    public static string CleanMText(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var decoded = DecodeCadText(input);
        decoded = decoded.Replace("\\P", "\n", StringComparison.OrdinalIgnoreCase);

        var sb = new StringBuilder(decoded.Length);
        for (int i = 0; i < decoded.Length; i++)
        {
            char c = decoded[i];
            if (c == '\\' && i + 1 < decoded.Length)
            {
                char next = decoded[i + 1];
                if (next is 'A' or 'H' or 'C' or 'f' or 'F' or 'Q' or 'W' or 'T' or 'S')
                {
                    int semicolon = decoded.IndexOf(';', i);
                    if (semicolon > i)
                    {
                        i = semicolon;
                        continue;
                    }
                }
                else if (next is '\\' or '{' or '}')
                {
                    sb.Append(next);
                    i++;
                    continue;
                }
            }
            else if (c == '{' || c == '}')
            {
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
