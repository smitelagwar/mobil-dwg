using System;
using System.Collections.Generic;
using SkiaSharp;

namespace MobilDwg.Rendering.Skia;

public sealed class RasterDecodeEntry : IDisposable
{
    public SKBitmap Bitmap { get; }
    public int Width => Bitmap.Width;
    public int Height => Bitmap.Height;
    public long ByteSize { get; }
    public long LastAccessSequence { get; set; }

    public RasterDecodeEntry(SKBitmap bitmap, long accessSequence)
    {
        Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        ByteSize = (long)bitmap.Width * bitmap.Height * bitmap.BytesPerPixel;
        LastAccessSequence = accessSequence;
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }
}

public sealed class RenderResourceCache : IDisposable
{
    private readonly object _syncLock = new();
    private readonly Dictionary<string, RasterDecodeEntry> _rasterCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SKPathEffect> _pathEffectCache = new(StringComparer.Ordinal);
    private long _accessCounter;
    private long _currentRasterBytes;
    private bool _disposed;

    public long MaxRasterBytes { get; }
    public long CurrentRasterBytes { get { lock (_syncLock) return _currentRasterBytes; } }
    public long CurrentSizeBytes => CurrentRasterBytes;

    public long RasterDecodeHits;
    public long RasterDecodeMisses;

    public RenderResourceCache(long maxRasterBytes = 64 * 1024 * 1024) // 64 MB default
    {
        if (maxRasterBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxRasterBytes));
        MaxRasterBytes = maxRasterBytes;
    }

    public bool TryGetRaster(string resourceKey, out SKBitmap? bitmap)
    {
        ArgumentNullException.ThrowIfNull(resourceKey);
        bitmap = null;

        lock (_syncLock)
        {
            if (_disposed) return false;

            if (_rasterCache.TryGetValue(resourceKey, out var entry))
            {
                entry.LastAccessSequence = ++_accessCounter;
                System.Threading.Interlocked.Increment(ref RasterDecodeHits);
                bitmap = entry.Bitmap;
                return true;
            }

            System.Threading.Interlocked.Increment(ref RasterDecodeMisses);
            return false;
        }
    }

    public bool PutRaster(string resourceKey, SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(bitmap);

        long byteSize = (long)bitmap.Width * bitmap.Height * bitmap.BytesPerPixel;
        if (byteSize <= 0 || byteSize > MaxRasterBytes)
        {
            return false;
        }

        lock (_syncLock)
        {
            if (_disposed) return false;

            if (_rasterCache.TryGetValue(resourceKey, out var existing))
            {
                _currentRasterBytes -= existing.ByteSize;
                existing.Dispose();
                _rasterCache.Remove(resourceKey);
            }

            if (_currentRasterBytes + byteSize > MaxRasterBytes)
            {
                EvictRasterToBudgetUnderLock(byteSize);
            }

            if (_currentRasterBytes + byteSize > MaxRasterBytes)
            {
                return false;
            }

            var entry = new RasterDecodeEntry(bitmap, ++_accessCounter);
            _rasterCache[resourceKey] = entry;
            _currentRasterBytes += entry.ByteSize;
            return true;
        }
    }

    private void EvictRasterToBudgetUnderLock(long neededBytes = 0)
    {
        long targetLimit = Math.Max(0, MaxRasterBytes - neededBytes);
        if (_currentRasterBytes <= targetLimit) return;

        var entries = new List<(string Key, long LastAccess)>();
        foreach (var (k, v) in _rasterCache)
        {
            entries.Add((k, v.LastAccessSequence));
        }

        entries.Sort((a, b) => a.LastAccess.CompareTo(b.LastAccess));

        for (var i = 0; i < entries.Count && _currentRasterBytes > targetLimit; i++)
        {
            var key = entries[i].Key;
            if (_rasterCache.Remove(key, out var entry))
            {
                _currentRasterBytes -= entry.ByteSize;
                entry.Dispose();
            }
        }
    }

    public SKPathEffect? GetOrCreateDashEffect(float[] pattern)
    {
        if (pattern == null || pattern.Length == 0) return null;

        // Fast key generation
        var key = pattern.Length switch
        {
            1 => $"{pattern[0]:F2}",
            2 => $"{pattern[0]:F2}_{pattern[1]:F2}",
            4 => $"{pattern[0]:F2}_{pattern[1]:F2}_{pattern[2]:F2}_{pattern[3]:F2}",
            _ => string.Join('_', pattern)
        };

        lock (_syncLock)
        {
            if (_disposed) return null;

            if (_pathEffectCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            if (_pathEffectCache.Count >= 256)
            {
                // Prune if too many distinct dynamic patterns
                foreach (var (_, pe) in _pathEffectCache)
                {
                    pe.Dispose();
                }
                _pathEffectCache.Clear();
            }

            var created = SKPathEffect.CreateDash(pattern, 0);
            if (created != null)
            {
                _pathEffectCache[key] = created;
            }
            return created;
        }
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            foreach (var (_, entry) in _rasterCache)
            {
                entry.Dispose();
            }
            _rasterCache.Clear();
            _currentRasterBytes = 0;

            foreach (var (_, effect) in _pathEffectCache)
            {
                effect.Dispose();
            }
            _pathEffectCache.Clear();
        }
    }

    public void Dispose()
    {
        lock (_syncLock)
        {
            if (_disposed) return;
            _disposed = true;
            Clear();
        }
    }
}
