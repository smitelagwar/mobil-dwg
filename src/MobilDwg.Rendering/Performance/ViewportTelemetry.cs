using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace MobilDwg.Rendering.Performance;

public readonly record struct ViewportTelemetrySample(
    long SequenceNumber,
    long InputEventTimeMs,
    long CameraRevision,
    long FrameRequestTicks,
    long PaintStartTicks,
    long PaintEndTicks,
    long SceneBuildTicks,
    long IndexQueryTicks,
    int EntityCount,
    int PrimitiveCount,
    int VertexCount,
    string Backend,
    int CacheHitCount,
    int CacheMissCount,
    long CacheBytes,
    long ClockCalibrationUptimeMs,
    long ClockCalibrationTicks)
{
    public double PaintDurationMs => PaintEndTicks >= PaintStartTicks && PaintStartTicks > 0
        ? (PaintEndTicks - PaintStartTicks) * 1000.0 / Stopwatch.Frequency
        : 0.0;

    public double SceneBuildDurationMs => SceneBuildTicks > 0
        ? SceneBuildTicks * 1000.0 / Stopwatch.Frequency
        : 0.0;

    public double IndexQueryDurationMs => IndexQueryTicks > 0
        ? IndexQueryTicks * 1000.0 / Stopwatch.Frequency
        : 0.0;

    public double? CalculateInputToPaintEndMs()
    {
        if (InputEventTimeMs <= 0 || ClockCalibrationUptimeMs <= 0 || ClockCalibrationTicks <= 0 || PaintEndTicks <= 0)
        {
            return null;
        }

        double paintEndUptimeMs = ClockCalibrationUptimeMs +
            ((PaintEndTicks - ClockCalibrationTicks) * 1000.0 / Stopwatch.Frequency);

        return Math.Max(0.0, paintEndUptimeMs - InputEventTimeMs);
    }
}

public sealed class ViewportTelemetry
{
    public const int BufferSize = 4096;

    private readonly object _sync = new();
    private readonly ViewportTelemetrySample[] _buffer = new ViewportTelemetrySample[BufferSize];
    private int _head;
    private int _count;
    private long _totalRecorded;
    private long _overflowCount;

    private long _calibrationUptimeMs;
    private long _calibrationTicks;

    public static ViewportTelemetry Instance { get; } = new();

    public long TotalRecorded
    {
        get { lock (_sync) return _totalRecorded; }
    }

    public long OverflowCount
    {
        get { lock (_sync) return _overflowCount; }
    }

    public int BufferedCount
    {
        get { lock (_sync) return _count; }
    }

    public void UpdateClockCalibration(long uptimeMs, long stopwatchTicks = 0)
    {
        if (stopwatchTicks <= 0)
        {
            stopwatchTicks = Stopwatch.GetTimestamp();
        }

        lock (_sync)
        {
            _calibrationUptimeMs = uptimeMs;
            _calibrationTicks = stopwatchTicks;
        }
    }

    public (long UptimeMs, long Ticks) GetClockCalibration()
    {
        lock (_sync)
        {
            return (_calibrationUptimeMs, _calibrationTicks);
        }
    }

    public void Record(
        long inputEventTimeMs,
        long cameraRevision,
        long frameRequestTicks,
        long paintStartTicks,
        long paintEndTicks,
        long sceneBuildTicks,
        long indexQueryTicks,
        int entityCount,
        int primitiveCount,
        int vertexCount,
        string backend,
        int cacheHitCount,
        int cacheMissCount,
        long cacheBytes)
    {
        lock (_sync)
        {
            long seq = _totalRecorded + 1;
            var sample = new ViewportTelemetrySample(
                SequenceNumber: seq,
                InputEventTimeMs: inputEventTimeMs,
                CameraRevision: cameraRevision,
                FrameRequestTicks: frameRequestTicks,
                PaintStartTicks: paintStartTicks,
                PaintEndTicks: paintEndTicks,
                SceneBuildTicks: sceneBuildTicks,
                IndexQueryTicks: indexQueryTicks,
                EntityCount: entityCount,
                PrimitiveCount: primitiveCount,
                VertexCount: vertexCount,
                Backend: backend ?? "Unknown",
                CacheHitCount: cacheHitCount,
                CacheMissCount: cacheMissCount,
                CacheBytes: cacheBytes,
                ClockCalibrationUptimeMs: _calibrationUptimeMs,
                ClockCalibrationTicks: _calibrationTicks
            );

            if (_count == BufferSize)
            {
                _buffer[_head] = sample;
                _head = (_head + 1) % BufferSize;
                _overflowCount++;
            }
            else
            {
                int index = (_head + _count) % BufferSize;
                _buffer[index] = sample;
                _count++;
            }

            _totalRecorded++;
        }
    }

    public ViewportTelemetrySample[] Drain()
    {
        lock (_sync)
        {
            if (_count == 0)
            {
                return Array.Empty<ViewportTelemetrySample>();
            }

            var result = new ViewportTelemetrySample[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _buffer[(_head + i) % BufferSize];
            }

            _head = 0;
            _count = 0;
            return result;
        }
    }

    public void Reset()
    {
        lock (_sync)
        {
            _head = 0;
            _count = 0;
            _totalRecorded = 0;
            _overflowCount = 0;
        }
    }

    public static string ExportToCsv(IReadOnlyList<ViewportTelemetrySample> samples)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SequenceNumber,InputEventTimeMs,CameraRevision,PaintDurationMs,SceneBuildDurationMs,IndexQueryDurationMs,EntityCount,PrimitiveCount,VertexCount,Backend,CacheHitCount,CacheMissCount,CacheBytes,InputToPaintEndMs");
        foreach (var s in samples)
        {
            sb.Append(s.SequenceNumber).Append(',')
              .Append(s.InputEventTimeMs).Append(',')
              .Append(s.CameraRevision).Append(',')
              .Append(s.PaintDurationMs.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
              .Append(s.SceneBuildDurationMs.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
              .Append(s.IndexQueryDurationMs.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
              .Append(s.EntityCount).Append(',')
              .Append(s.PrimitiveCount).Append(',')
              .Append(s.VertexCount).Append(',')
              .Append(s.Backend).Append(',')
              .Append(s.CacheHitCount).Append(',')
              .Append(s.CacheMissCount).Append(',')
              .Append(s.CacheBytes).Append(',')
              .Append(s.CalculateInputToPaintEndMs()?.ToString("F3", CultureInfo.InvariantCulture) ?? string.Empty)
              .AppendLine();
        }
        return sb.ToString();
    }
}
