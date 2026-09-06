using System;
using System.Collections.Generic;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Spatial;

public struct SpatialQueryMetrics
{
    public int VisitedNodes;
    public int BoundsTests;
    public int CandidateCount;
}

public sealed class BvhNode
{
    public readonly WorldBounds2 Bounds;
    public readonly BvhNode? Left;
    public readonly BvhNode? Right;
    public readonly int[]? EntityIndices;

    public bool IsLeaf => EntityIndices != null;

    public BvhNode(WorldBounds2 bounds, BvhNode left, BvhNode right)
    {
        Bounds = bounds;
        Left = left;
        Right = right;
        EntityIndices = null;
    }

    public BvhNode(WorldBounds2 bounds, int[] entityIndices)
    {
        Bounds = bounds;
        Left = null;
        Right = null;
        EntityIndices = entityIndices;
    }
}

public sealed class StaticSceneBvh
{
    public const int MaxLeafEntities = 16;
    public const int BvhEntityThreshold = 2048;

    [ThreadStatic]
    private static BvhNode[]? t_queryStack;

    private readonly IReadOnlyList<RenderSceneEntity> _entities;
    private readonly BvhNode? _root;
    private readonly int[] _alwaysTestIndices;
    private readonly bool _forceBvh;

    public IReadOnlyList<RenderSceneEntity> Entities => _entities;
    public BvhNode? Root => _root;
    public IReadOnlyList<int> AlwaysTestIndices => _alwaysTestIndices;
    public bool UsesBvh => _forceBvh || _entities.Count >= BvhEntityThreshold;

    public StaticSceneBvh(IReadOnlyList<RenderSceneEntity> entities, bool forceBvh = false)
    {
        _entities = entities ?? throw new ArgumentNullException(nameof(entities));
        _forceBvh = forceBvh;

        var validIndices = new List<int>(entities.Count);
        var alwaysTest = new List<int>();

        for (var i = 0; i < entities.Count; i++)
        {
            var b = entities[i].Bounds;
            if (double.IsFinite(b.MinX) && double.IsFinite(b.MinY) &&
                double.IsFinite(b.MaxX) && double.IsFinite(b.MaxY) &&
                b.MaxX >= b.MinX && b.MaxY >= b.MinY)
            {
                validIndices.Add(i);
            }
            else
            {
                alwaysTest.Add(i);
            }
        }

        _alwaysTestIndices = alwaysTest.ToArray();

        if (validIndices.Count > 0 && (_forceBvh || entities.Count >= BvhEntityThreshold))
        {
            var indicesArray = validIndices.ToArray();
            _root = BuildRecursive(indicesArray, 0, indicesArray.Length);
        }
        else
        {
            _root = null;
        }
    }

    private BvhNode BuildRecursive(int[] indices, int start, int count)
    {
        // Compute bounding box covering all entities in this range
        var bounds = _entities[indices[start]].Bounds;
        for (var i = 1; i < count; i++)
        {
            bounds = bounds.Union(_entities[indices[start + i]].Bounds);
        }

        if (count <= MaxLeafEntities)
        {
            var leafIndices = new int[count];
            Array.Copy(indices, start, leafIndices, 0, count);
            return new BvhNode(bounds, leafIndices);
        }

        // Find centroid extent
        var firstCenter = _entities[indices[start]].Bounds.Center;
        double minCx = firstCenter.X, maxCx = firstCenter.X;
        double minCy = firstCenter.Y, maxCy = firstCenter.Y;

        for (var i = 1; i < count; i++)
        {
            var c = _entities[indices[start + i]].Bounds.Center;
            if (c.X < minCx) minCx = c.X;
            if (c.X > maxCx) maxCx = c.X;
            if (c.Y < minCy) minCy = c.Y;
            if (c.Y > maxCy) maxCy = c.Y;
        }

        var dx = maxCx - minCx;
        var dy = maxCy - minCy;
        var splitX = dx >= dy;

        // Sort indices in [start, start + count) by centroid along the dominant axis
        // Tie-breaker is the original entity index to guarantee deterministic balanced partitions
        Array.Sort(indices, start, count, Comparer<int>.Create((a, b) =>
        {
            var ca = _entities[a].Bounds.Center;
            var cb = _entities[b].Bounds.Center;
            var cmp = splitX ? ca.X.CompareTo(cb.X) : ca.Y.CompareTo(cb.Y);
            if (cmp != 0) return cmp;
            // Secondary axis
            var cmp2 = splitX ? ca.Y.CompareTo(cb.Y) : ca.X.CompareTo(cb.X);
            if (cmp2 != 0) return cmp2;
            // Deterministic tie-breaker: original ordinal
            return a.CompareTo(b);
        }));

        var mid = count / 2;
        var left = BuildRecursive(indices, start, mid);
        var right = BuildRecursive(indices, start + mid, count - mid);

        return new BvhNode(bounds, left, right);
    }

    public void Query(
        WorldBounds2 queryBounds,
        List<int> resultIndices,
        ref SpatialQueryMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(resultIndices);
        resultIndices.Clear();

        if (!UsesBvh || _root == null)
        {
            // Linear search path
            for (var i = 0; i < _entities.Count; i++)
            {
                metrics.BoundsTests++;
                if (_entities[i].Bounds.Intersects(queryBounds))
                {
                    resultIndices.Add(i);
                }
            }

            metrics.CandidateCount = resultIndices.Count;
            return;
        }

        // Always-test entities
        for (var i = 0; i < _alwaysTestIndices.Length; i++)
        {
            var idx = _alwaysTestIndices[i];
            metrics.BoundsTests++;
            if (_entities[idx].Bounds.Intersects(queryBounds))
            {
                resultIndices.Add(idx);
            }
        }

        // Tree traversal using thread-static reusable array stack (allocation-free)
        var stack = t_queryStack ??= new BvhNode[128];
        var stackTop = 0;
        stack[stackTop++] = _root;

        while (stackTop > 0)
        {
            var node = stack[--stackTop];
            stack[stackTop] = null!;
            metrics.VisitedNodes++;
            metrics.BoundsTests++;

            if (!node.Bounds.Intersects(queryBounds))
            {
                continue;
            }

            if (node.IsLeaf)
            {
                var leafEntities = node.EntityIndices!;
                for (var i = 0; i < leafEntities.Length; i++)
                {
                    metrics.BoundsTests++;
                    var entityIdx = leafEntities[i];
                    if (_entities[entityIdx].Bounds.Intersects(queryBounds))
                    {
                        resultIndices.Add(entityIdx);
                    }
                }
            }
            else
            {
                if (stackTop + 2 >= stack.Length)
                {
                    Array.Resize(ref stack, stack.Length * 2);
                    t_queryStack = stack;
                }

                // Push right then left so left is popped first
                if (node.Right != null) stack[stackTop++] = node.Right;
                if (node.Left != null) stack[stackTop++] = node.Left;
            }
        }

        // Sort candidates by original ordinal to preserve exact CAD draw order
        resultIndices.Sort();
        metrics.CandidateCount = resultIndices.Count;
    }
}
