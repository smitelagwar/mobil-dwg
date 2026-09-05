using ACadSharp;
using ACadSharp.Entities;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

namespace MobilDwg.Cad.AcadSharp;

public static class AcadSharpEntityExtractor
{
    public static CadExtractedDocument Extract(ICadDocumentHandle handle)
    {
        if (handle is not AcadSharpDocumentHandle acadHandle)
        {
            throw new ArgumentException("Handle was not created by the ACadSharp adapter.", nameof(handle));
        }

        var document = acadHandle.Document;
        var layerList = new List<CadExtractedLayer>();
        var layerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Extract Layers
        foreach (var layer in document.Layers)
        {
            var name = layer.Name ?? "0";
            if (layerNames.Add(name))
            {
                uint argb = 0xFFCCCCCC; // Default light gray
                if (layer.Color.TrueColor != 0)
                {
                    argb = 0xFF000000 | (uint)(layer.Color.TrueColor & 0x00FFFFFF);
                }
                else if (layer.Color.Index > 0)
                {
                    argb = MapAciToArgb(layer.Color.Index);
                }
                layerList.Add(new CadExtractedLayer(name, argb, layer.IsOn));
            }
        }

        if (!layerNames.Contains("0"))
        {
            layerList.Insert(0, new CadExtractedLayer("0", 0xFFFFFFFF, true));
        }

        // 2. Extract Entities
        var extracted = new List<CadExtractedEntity>();
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

        uint? GetEntityColor(Entity entity)
        {
            if (entity.Color.TrueColor != 0)
            {
                return 0xFF000000 | (uint)(entity.Color.TrueColor & 0x00FFFFFF);
            }
            if (entity.Color.Index > 0 && entity.Color.Index < 256)
            {
                return MapAciToArgb(entity.Color.Index);
            }
            return null; // ByLayer
        }

        int entityIndex = 0;
        foreach (var entity in document.Entities)
        {
            entityIndex++;
            var layer = entity.Layer?.Name ?? "0";
            var color = GetEntityColor(entity);
            var handleStr = entity.Handle.ToString("X");

            if (entity is Line line)
            {
                var p1 = new CadExtractedPoint(line.StartPoint.X, line.StartPoint.Y);
                var p2 = new CadExtractedPoint(line.EndPoint.X, line.EndPoint.Y);
                UpdateBounds(p1.X, p1.Y);
                UpdateBounds(p2.X, p2.Y);

                extracted.Add(new CadExtractedEntity(
                    handleStr,
                    layer,
                    CadExtractedEntityType.Line,
                    color,
                    Points: new[] { p1, p2 }));
            }
            else if (entity is Circle circle)
            {
                var cx = circle.Center.X;
                var cy = circle.Center.Y;
                var r = circle.Radius;
                UpdateBounds(cx - r, cy - r);
                UpdateBounds(cx + r, cy + r);

                extracted.Add(new CadExtractedEntity(
                    handleStr,
                    layer,
                    CadExtractedEntityType.Circle,
                    color,
                    Points: new[] { new CadExtractedPoint(cx, cy) },
                    Radius: r,
                    StartAngle: 0,
                    EndAngle: Math.PI * 2));
            }
            else if (entity is Arc arc)
            {
                var cx = arc.Center.X;
                var cy = arc.Center.Y;
                var r = arc.Radius;
                UpdateBounds(cx - r, cy - r);
                UpdateBounds(cx + r, cy + r);

                extracted.Add(new CadExtractedEntity(
                    handleStr,
                    layer,
                    CadExtractedEntityType.Arc,
                    color,
                    Points: new[] { new CadExtractedPoint(cx, cy) },
                    Radius: r,
                    StartAngle: arc.StartAngle,
                    EndAngle: arc.EndAngle));
            }
            else if (entity is LwPolyline lwPoly)
            {
                var vertices = new List<CadExtractedVertex>();
                foreach (var v in lwPoly.Vertices)
                {
                    UpdateBounds(v.Location.X, v.Location.Y);
                    vertices.Add(new CadExtractedVertex(v.Location.X, v.Location.Y, v.Bulge));
                }

                if (vertices.Count >= 2)
                {
                    extracted.Add(new CadExtractedEntity(
                        handleStr,
                        layer,
                        CadExtractedEntityType.Polyline,
                        color,
                        Vertices: vertices));
                }
            }
            else if (entity is TextEntity text)
            {
                var tx = text.InsertPoint.X;
                var ty = text.InsertPoint.Y;
                UpdateBounds(tx, ty);

                extracted.Add(new CadExtractedEntity(
                    handleStr,
                    layer,
                    CadExtractedEntityType.Text,
                    color,
                    Points: new[] { new CadExtractedPoint(tx, ty) },
                    Text: text.Value,
                    TextHeight: text.Height > 0 ? text.Height : 10.0,
                    Rotation: text.Rotation));
            }
            else if (entity is MText mtext)
            {
                var tx = mtext.InsertPoint.X;
                var ty = mtext.InsertPoint.Y;
                UpdateBounds(tx, ty);

                extracted.Add(new CadExtractedEntity(
                    handleStr,
                    layer,
                    CadExtractedEntityType.Text,
                    color,
                    Points: new[] { new CadExtractedPoint(tx, ty) },
                    Text: mtext.Value,
                    TextHeight: mtext.Height > 0 ? mtext.Height : 10.0,
                    Rotation: mtext.Rotation));
            }
            else if (entity is Insert insert)
            {
                // Simple insert anchor
                var ix = insert.InsertPoint.X;
                var iy = insert.InsertPoint.Y;
                UpdateBounds(ix, iy);

                extracted.Add(new CadExtractedEntity(
                    handleStr,
                    layer,
                    CadExtractedEntityType.Other,
                    color,
                    Points: new[] { new CadExtractedPoint(ix, iy) },
                    Text: insert.Block?.Name ?? "BLOCK"));
            }
        }

        if (minX > maxX)
        {
            minX = 0; minY = 0; maxX = 100; maxY = 100;
        }

        return new CadExtractedDocument(
            document.Header?.Version.ToString() ?? "DWG",
            document.Header?.Version.ToString() ?? "Unknown",
            layerList,
            extracted,
            minX, minY, maxX, maxY);
    }

    private static uint MapAciToArgb(short aci)
    {
        return aci switch
        {
            1 => 0xFFFF0000, // Red
            2 => 0xFFFFFF00, // Yellow
            3 => 0xFF00FF00, // Green
            4 => 0xFF00FFFF, // Cyan
            5 => 0xFF0000FF, // Blue
            6 => 0xFFFF00FF, // Magenta
            7 => 0xFFFFFFFF, // White
            8 => 0xFF808080, // Dark Gray
            9 => 0xFFC0C0C0, // Light Gray
            _ => 0xFFE2E8F0  // Neutral Slate
        };
    }
}
