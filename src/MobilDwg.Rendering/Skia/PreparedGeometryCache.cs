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
                // D05: Cache hit için aynı band yeterli olmayacak: kaydedilmiş gerçek/geçerli üst hata sınırı istenen chord error'ı sağlamalı.
                PreparedGeometryEntry? match = null;

                // First look for an exact lodBand match that satisfies the required chord error
                for (var i = 0; i < lodList.Count; i++)
                {
                    var entry = lodList[i];
                    if (entry.LodBand == lodBand && entry.MaxChordError <= requiredChordError)
                    {
                        match = entry;
                        break;
                    }
                }

                // If no exact lodBand match, check if any entry satisfies the required chord error
                if (match == null)
                {
                    for (var i = 0; i < lodList.Count; i++)
                    {
                        var entry = lodList[i];
                        if (entry.MaxChordError <= requiredChordError)
                        {
                            if (match == null || entry.MaxChordError > match.MaxChordError)
                            {
                                match = entry;
                            }
                        }
                    }
                }

                if (match != null)
                {
                    match.LastAccessSequence = ++_accessCounter;
                    System.Threading.Interlocked.Increment(ref CacheHits);
                    result = match;
                    return true;
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

        var entry = new PreparedGeometryEntry(path, localOrigin, chordError, lodBand, 0);
        if (entry.EstimatedBytes > MaxSizeBytes) return;

        lock (_syncLock)
        {
            if (_disposed) return;

            entry.LastAccessSequence = ++_accessCounter;

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

        var entry = new HatchCoverageEntry(coverageBounds, lines, styleRevision, lodBand, 0);
        if (entry.EstimatedBytes > MaxSizeBytes) return;

        lock (_syncLock)
        {
            if (_disposed) return;

            entry.LastAccessSequence = ++_accessCounter;

            var fullKey = $"{sceneRevision}:{hatchKey}";
            if (_hatchEntries.TryGetValue(fullKey, out var oldEntry))
            {
                _currentSizeBytes -= oldEntry.EstimatedBytes;
            }

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

    private sealed class LruCandidate
    {
        public bool IsHatch { get; }
        public string Key { get; }
        public long LastAccess { get; }
        public long Bytes { get; }

        public LruCandidate(bool isHatch, string key, long lastAccess, long bytes)
        {
            IsHatch = isHatch;
            Key = key;
            LastAccess = lastAccess;
            Bytes = bytes;
        }
    }

    private void EvictToBudgetUnderLock()
    {
        if (_currentSizeBytes <= MaxSizeBytes) return;

        var allItems = new List<LruCandidate>();
        foreach (var (k, list) in _entries)
        {
            for (var i = 0; i < list.Count; i++)
            {
                allItems.Add(new LruCandidate(false, k, list[i].LastAccessSequence, list[i].EstimatedBytes));
            }
        }
        foreach (var (k, hatchEntry) in _hatchEntries)
        {
            allItems.Add(new LruCandidate(true, k, hatchEntry.LastAccessSequence, hatchEntry.EstimatedBytes));
        }

        allItems.Sort((a, b) => a.LastAccess.CompareTo(b.LastAccess));

        for (var i = 0; i < allItems.Count && _currentSizeBytes > MaxSizeBytes; i++)
        {
            var candidate = allItems[i];
            if (candidate.IsHatch)
            {
                if (_hatchEntries.Remove(candidate.Key, out var removedHatch))
                {
                    _currentSizeBytes -= removedHatch.EstimatedBytes;
                    System.Threading.Interlocked.Increment(ref EvictionCount);
                }
            }
            else
            {
                if (_entries.TryGetValue(candidate.Key, out var list))
                {
                    var matchIdx = list.FindIndex(e => e.LastAccessSequence == candidate.LastAccess);
                    if (matchIdx >= 0)
                    {
                        _currentSizeBytes -= list[matchIdx].EstimatedBytes;
                        list.RemoveAt(matchIdx);
                        if (list.Count == 0)
                        {
                            _entries.Remove(candidate.Key);
                        }
                        System.Threading.Interlocked.Increment(ref EvictionCount);
                    }
                }
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
