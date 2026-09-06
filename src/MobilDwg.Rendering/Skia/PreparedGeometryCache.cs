using System;
using System.Collections.Generic;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Skia;

public sealed class PreparedGeometryEntry
{
    public TessellatedPath Path { get; }
    public WorldPoint2 LocalOrigin { get; }
    public double MaxChordError { get; }
    public int LodBand { get; }
    public double MaxFloatErrorWorld { get; }
    public long EstimatedBytes { get; }
    public long LastAccessSequence { get; set; }

    public PreparedGeometryEntry(
        TessellatedPath path,
        WorldPoint2 localOrigin,
        double maxChordError,
        int lodBand,
        long accessSequence)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        LocalOrigin = localOrigin;
        MaxChordError = maxChordError;
        LodBand = lodBand;
        LastAccessSequence = accessSequence;

        // Calculate float precision error relative to local origin
        double maxErr = 0d;
        for (var i = 0; i < path.Points.Count; i++)
        {
            var p = path.Points[i];
            var dx = p.X - localOrigin.X;
            var dy = p.Y - localOrigin.Y;
            var fx = (float)dx;
            var fy = (float)dy;
            var errX = Math.Abs(dx - (double)fx);
            var errY = Math.Abs(dy - (double)fy);
            var err = Math.Sqrt(errX * errX + errY * errY);
            if (err > maxErr) maxErr = err;
        }

        MaxFloatErrorWorld = maxErr;
        // Estimate memory: 24 bytes per WorldPoint2 + path overhead
        EstimatedBytes = 64 + (path.Points.Count * 24L);
    }

    public bool IsFloatPrecisionSafe(double worldUnitsPerPixel, double maxPixelError = 0.1)
    {
        if (worldUnitsPerPixel <= 0) return false;
        var errorPixels = MaxFloatErrorWorld / worldUnitsPerPixel;
        return errorPixels <= maxPixelError;
    }
}

public sealed class HatchCoverageEntry
{
    public WorldBounds2 CoverageBounds { get; }
    public IReadOnlyList<(WorldPoint2 Start, WorldPoint2 End)> Lines { get; }
    public long StyleRevision { get; }
    public int LodBand { get; }
    public long EstimatedBytes { get; }
    public long LastAccessSequence { get; set; }

    public HatchCoverageEntry(
        WorldBounds2 coverageBounds,
        IReadOnlyList<(WorldPoint2 Start, WorldPoint2 End)> lines,
        long styleRevision,
        int lodBand,
        long accessSequence)
    {
        CoverageBounds = coverageBounds;
        Lines = lines ?? throw new ArgumentNullException(nameof(lines));
        StyleRevision = styleRevision;
        LodBand = lodBand;
        LastAccessSequence = accessSequence;
        EstimatedBytes = 64 + (lines.Count * 32L);
    }
}

public sealed class PreparedGeometryCache : IDisposable
{
    private readonly object _syncLock = new();
    private readonly Dictionary<string, List<PreparedGeometryEntry>> _entries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HatchCoverageEntry> _hatchEntries = new(StringComparer.Ordinal);
    private long _accessCounter;
    private long _currentSizeBytes;
    private bool _disposed;

    public long MaxSizeBytes { get; }
    public long CurrentSizeBytes { get { lock (_syncLock) return _currentSizeBytes; } }

    public long CacheHits;
    public long CacheMisses;
    public long TessellationCount;
    public long EvictionCount;

    public PreparedGeometryCache(long maxSizeBytes = 32 * 1024 * 1024) // 32 MB default
    {
        if (maxSizeBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxSizeBytes));
        MaxSizeBytes = maxSizeBytes;
    }

    public bool TryGet(
        long sceneRevision,
        string primitiveKey,
        int lodBand,
        double requiredChordError,
        out PreparedGeometryEntry? result)
    {
        ArgumentNullException.ThrowIfNull(primitiveKey);
        result = null;

        lock (_syncLock)
        {
            if (_disposed) return false;

            var fullKey = $"{sceneRevision}:{primitiveKey}";
            if (_entries.TryGetValue(fullKey, out var lodList))
            {
                // Find matching or finer LOD (chord error <= requiredChordError)
                for (var i = 0; i < lodList.Count; i++)
                {
                    var entry = lodList[i];
                    if (entry.LodBand == lodBand || entry.MaxChordError <= requiredChordError)
                    {
                        entry.LastAccessSequence = ++_accessCounter;
                        System.Threading.Interlocked.Increment(ref CacheHits);
                        result = entry;
                        return true;
                    }
                }
            }

            System.Threading.Interlocked.Increment(ref CacheMisses);
            return false;
        }
    }

    public void Put(
        long sceneRevision,
        string primitiveKey,
        int lodBand,
        TessellatedPath path,
        double chordError,
        WorldPoint2 localOrigin)
    {
        ArgumentNullException.ThrowIfNull(primitiveKey);
        ArgumentNullException.ThrowIfNull(path);

        lock (_syncLock)
        {
            if (_disposed) return;

            var fullKey = $"{sceneRevision}:{primitiveKey}";
            if (!_entries.TryGetValue(fullKey, out var lodList))
            {
                lodList = new List<PreparedGeometryEntry>(2);
                _entries[fullKey] = lodList;
            }

            // At most 2 LOD levels per primitive
            if (lodList.Count >= 2)
            {
                // Remove the least recently accessed LOD
                var oldestIdx = 0;
                var oldestSeq = lodList[0].LastAccessSequence;
                for (var i = 1; i < lodList.Count; i++)
                {
                    if (lodList[i].LastAccessSequence < oldestSeq)
                    {
                        oldestSeq = lodList[i].LastAccessSequence;
                        oldestIdx = i;
                    }
                }

                _currentSizeBytes -= lodList[oldestIdx].EstimatedBytes;
                lodList.RemoveAt(oldestIdx);
                System.Threading.Interlocked.Increment(ref EvictionCount);
            }

            var entry = new PreparedGeometryEntry(path, localOrigin, chordError, lodBand, ++_accessCounter);
            lodList.Add(entry);
            _currentSizeBytes += entry.EstimatedBytes;

            EvictToBudgetUnderLock();
        }
    }

    public bool TryGetHatchCoverage(
        long sceneRevision,
        string hatchKey,
        WorldBounds2 requiredBounds,
        int lodBand,
        long styleRevision,
        out HatchCoverageEntry? result)
    {
        ArgumentNullException.ThrowIfNull(hatchKey);
        result = null;

        lock (_syncLock)
        {
            if (_disposed) return false;

            var fullKey = $"{sceneRevision}:{hatchKey}";
            if (_hatchEntries.TryGetValue(fullKey, out var entry))
            {
                if (entry.StyleRevision == styleRevision &&
                    entry.LodBand == lodBand &&
                    entry.CoverageBounds.Contains(requiredBounds))
                {
                    entry.LastAccessSequence = ++_accessCounter;
                    System.Threading.Interlocked.Increment(ref CacheHits);
                    result = entry;
                    return true;
                }
            }

            return false;
        }
    }

    public void PutHatchCoverage(
        long sceneRevision,
        string hatchKey,
        WorldBounds2 coverageBounds,
        IReadOnlyList<(WorldPoint2 Start, WorldPoint2 End)> lines,
        int lodBand,
        long styleRevision)
    {
        ArgumentNullException.ThrowIfNull(hatchKey);
        ArgumentNullException.ThrowIfNull(lines);

        lock (_syncLock)
        {
            if (_disposed) return;

            var fullKey = $"{sceneRevision}:{hatchKey}";
            if (_hatchEntries.TryGetValue(fullKey, out var oldEntry))
            {
                _currentSizeBytes -= oldEntry.EstimatedBytes;
            }

            var entry = new HatchCoverageEntry(coverageBounds, lines, styleRevision, lodBand, ++_accessCounter);
            _hatchEntries[fullKey] = entry;
            _currentSizeBytes += entry.EstimatedBytes;

            EvictToBudgetUnderLock();
        }
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            _entries.Clear();
            _hatchEntries.Clear();
            _currentSizeBytes = 0;
        }
    }

    private void EvictToBudgetUnderLock()
    {
        if (_currentSizeBytes <= MaxSizeBytes) return;

        // Evict oldest entries until size is within budget
        var allKeys = new List<(string Key, long LastAccess, long Bytes)>();
        foreach (var (k, list) in _entries)
        {
            for (var i = 0; i < list.Count; i++)
            {
                allKeys.Add((k, list[i].LastAccessSequence, list[i].EstimatedBytes));
            }
        }

        allKeys.Sort((a, b) => a.LastAccess.CompareTo(b.LastAccess));

        for (var i = 0; i < allKeys.Count && _currentSizeBytes > MaxSizeBytes; i++)
        {
            var targetKey = allKeys[i].Key;
            if (_entries.TryGetValue(targetKey, out var list) && list.Count > 0)
            {
                _currentSizeBytes -= list[0].EstimatedBytes;
                list.RemoveAt(0);
                if (list.Count == 0)
                {
                    _entries.Remove(targetKey);
                }
                System.Threading.Interlocked.Increment(ref EvictionCount);
            }
        }
    }

    public void Dispose()
    {
        lock (_syncLock)
        {
            if (_disposed) return;
            _disposed = true;
            _entries.Clear();
            _hatchEntries.Clear();
            _currentSizeBytes = 0;
        }
    }
}
