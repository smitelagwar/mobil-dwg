using System;
using System.Collections.Generic;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Viewer;

public enum CadSnapKind
{
    Endpoint = 0,
    Center = 1,
    Curve = 2,
}

public readonly record struct CadSnapResult(
    WorldPoint2 WorldPoint,
    CadSnapKind Kind,
    double DistancePixels,
    RenderEntityId EntityId);

public static class SnapQuery
{
    public const double DefaultSnapRadiusDip = 12.0;

    public static CadSnapResult? FindSnapPoint(
        ScreenPoint2 screenPoint,
        Camera2D camera,
        RenderScene scene,
        LayerTable layerTable,
        double snapRadiusDip = DefaultSnapRadiusDip,
        double density = 1.0)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(layerTable);
        if (!camera.IsValid) return null;

        var snapRadiusPx = snapRadiusDip * (density > 0 ? density : 1.0);
        var snapRadiusWorld = snapRadiusPx * camera.WorldUnitsPerPixel;
        if (snapRadiusWorld <= 0) return null;

        var worldQueryPt = CameraTransform.ScreenToWorld(screenPoint, camera);
        var queryBounds = new WorldBounds2(
            worldQueryPt.X - snapRadiusWorld,
            worldQueryPt.Y - snapRadiusWorld,
            worldQueryPt.X + snapRadiusWorld,
            worldQueryPt.Y + snapRadiusWorld);

        IEnumerable<RenderSceneEntity> candidateEntities;
        if (scene.SpatialIndex != null)
        {
            var indices = new List<int>();
            var metrics = new MobilDwg.Rendering.Spatial.SpatialQueryMetrics();
            scene.SpatialIndex.Query(queryBounds, indices, ref metrics);
            var sceneEntities = scene.Entities;
            var list = new List<RenderSceneEntity>(indices.Count);
            for (var i = 0; i < indices.Count; i++)
            {
                list.Add(sceneEntities[indices[i]]);
            }
            candidateEntities = list;
        }
        else
        {
            candidateEntities = scene.Entities;
        }

        CadSnapResult? bestResult = null;

        foreach (var entity in candidateEntities)
        {
            if (!layerTable.IsLayerVisible(entity.Layer.Value))
            {
                continue;
            }

            foreach (var primitive in entity.Geometry)
            {
                EvaluatePrimitiveSnap(
                    primitive,
                    entity.Id,
                    worldQueryPt,
                    screenPoint,
                    camera,
                    snapRadiusPx,
                    ref bestResult);
            }
        }

        return bestResult;
    }

    private static void EvaluatePrimitiveSnap(
        RenderGeometryPrimitive primitive,
        RenderEntityId entityId,
        WorldPoint2 worldQueryPt,
        ScreenPoint2 screenPoint,
        Camera2D camera,
        double maxRadiusPx,
        ref CadSnapResult? bestResult)
    {
        switch (primitive)
        {
            case LinePrimitive line:
                ConsiderPoint(line.Start, CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                ConsiderPoint(line.End, CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                var closestLinePt = ClosestPointOnSegment(worldQueryPt, line.Start, line.End);
                ConsiderPoint(closestLinePt, CadSnapKind.Curve, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                break;

            case ArcPrimitive arc:
                ConsiderPoint(arc.Center, CadSnapKind.Center, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                if (Math.Abs(arc.SweepRadians) < Math.PI * 2.0 - 1e-4)
                {
                    var pStart = new WorldPoint2(arc.Center.X + (arc.Radius * Math.Cos(arc.StartRadians)), arc.Center.Y + (arc.Radius * Math.Sin(arc.StartRadians)));
                    var pEnd = new WorldPoint2(arc.Center.X + (arc.Radius * Math.Cos(arc.StartRadians + arc.SweepRadians)), arc.Center.Y + (arc.Radius * Math.Sin(arc.StartRadians + arc.SweepRadians)));
                    ConsiderPoint(pStart, CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                    ConsiderPoint(pEnd, CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                }
                var dx = worldQueryPt.X - arc.Center.X;
                var dy = worldQueryPt.Y - arc.Center.Y;
                var dist = Math.Sqrt((dx * dx) + (dy * dy));
                if (dist > 1e-9)
                {
                    var angle = Math.Atan2(dy, dx);
                    if (IsAngleOnArc(angle, arc.StartRadians, arc.SweepRadians))
                    {
                        var curvePt = new WorldPoint2(arc.Center.X + (arc.Radius * (dx / dist)), arc.Center.Y + (arc.Radius * (dy / dist)));
                        ConsiderPoint(curvePt, CadSnapKind.Curve, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                    }
                }
                break;

            case PolylinePrimitive poly:
                for (var i = 0; i < poly.Vertices.Count; i++)
                {
                    ConsiderPoint(poly.Vertices[i].Position, CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                    if (i < poly.Vertices.Count - 1 || poly.Closed)
                    {
                        var nextIdx = (i + 1) % poly.Vertices.Count;
                        var p1 = poly.Vertices[i].Position;
                        var p2 = poly.Vertices[nextIdx].Position;
                        var closest = ClosestPointOnSegment(worldQueryPt, p1, p2);
                        ConsiderPoint(closest, CadSnapKind.Curve, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                    }
                }
                break;

            case PointPrimitive pt:
                ConsiderPoint(pt.Position, CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                break;

            case SplinePrimitive spline:
                var tessellated = GeometryTessellator.Tessellate(spline, GeometryTessellationOptions.Default);
                if (tessellated.Points.Count > 0)
                {
                    ConsiderPoint(tessellated.Points[0], CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                    ConsiderPoint(tessellated.Points[^1], CadSnapKind.Endpoint, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                }
                for (var ti = 0; ti < tessellated.Points.Count - 1; ti++)
                {
                    var segPt = ClosestPointOnSegment(worldQueryPt, tessellated.Points[ti], tessellated.Points[ti + 1]);
                    ConsiderPoint(segPt, CadSnapKind.Curve, entityId, screenPoint, camera, maxRadiusPx, ref bestResult);
                }
                break;
        }
    }

    public static WorldPoint2 ClosestPointOnSegment(WorldPoint2 pt, WorldPoint2 p1, WorldPoint2 p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        var lenSq = (dx * dx) + (dy * dy);
        if (lenSq < 1e-12) return p1;

        var t = (((pt.X - p1.X) * dx) + ((pt.Y - p1.Y) * dy)) / lenSq;
        t = Math.Clamp(t, 0.0, 1.0);
        return new WorldPoint2(p1.X + (t * dx), p1.Y + (t * dy));
    }

    private static bool IsAngleOnArc(double angle, double start, double sweep)
    {
        if (Math.Abs(sweep) >= Math.PI * 2.0 - 1e-4) return true;
        var normStart = NormalizeAngle(start);
        var normAngle = NormalizeAngle(angle);
        if (sweep > 0)
        {
            var diff = NormalizeAngle(normAngle - normStart);
            return diff <= sweep + 1e-5;
        }
        else
        {
            var diff = NormalizeAngle(normStart - normAngle);
            return diff <= -sweep + 1e-5;
        }
    }

    private static double NormalizeAngle(double a)
    {
        a %= (2.0 * Math.PI);
        if (a < 0) a += (2.0 * Math.PI);
        return a;
    }

    private static void ConsiderPoint(
        WorldPoint2 candidateWorld,
        CadSnapKind kind,
        RenderEntityId entityId,
        ScreenPoint2 screenQuery,
        Camera2D camera,
        double maxRadiusPx,
        ref CadSnapResult? bestResult)
    {
        var candidateScreen = CameraTransform.WorldToScreen(candidateWorld, camera);
        var dx = candidateScreen.X - screenQuery.X;
        var dy = candidateScreen.Y - screenQuery.Y;
        var distPx = Math.Sqrt((dx * dx) + (dy * dy));

        if (distPx > maxRadiusPx) return;

        var candidate = new CadSnapResult(candidateWorld, kind, distPx, entityId);

        if (bestResult == null)
        {
            bestResult = candidate;
            return;
        }

        var current = bestResult.Value;
        if (Math.Abs(distPx - current.DistancePixels) > 1.0)
        {
            if (distPx < current.DistancePixels)
            {
                bestResult = candidate;
            }
        }
        else
        {
            if (candidate.Kind < current.Kind)
            {
                bestResult = candidate;
            }
            else if (candidate.Kind == current.Kind)
            {
                if (string.CompareOrdinal(candidate.EntityId.Value, current.EntityId.Value) < 0)
                {
                    bestResult = candidate;
                }
            }
        }
    }
}
