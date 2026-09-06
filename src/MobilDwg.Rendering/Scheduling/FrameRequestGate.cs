using System;

namespace MobilDwg.Rendering.Scheduling;

public enum FrameGateState
{
    Idle,
    Scheduled,
    AwaitingPaint,
    Painting
}

public sealed record FrameTicket(long TicketId, long SurfaceGeneration, long RequestTimeMs);

public sealed class FrameRequestGate
{
    private readonly object _sync = new();
    private FrameGateState _state = FrameGateState.Idle;
    private bool _hasPendingRequest;
    private long _currentSurfaceGeneration = 1;
    private long _nextTicketId;
    private long _activeTicketId;
    private long _lastRequestTimeMs;
    private long _lastRequestTicks;
    private int _requestedFrameCount;
    private int _paintedFrameCount;

    public FrameGateState State
    {
        get { lock (_sync) return _state; }
    }

    public long CurrentSurfaceGeneration
    {
        get { lock (_sync) return _currentSurfaceGeneration; }
    }

    public long LastRequestTicks
    {
        get { lock (_sync) return _lastRequestTicks; }
    }

    public int RequestedFrameCount
    {
        get { lock (_sync) return _requestedFrameCount; }
    }

    public int PaintedFrameCount
    {
        get { lock (_sync) return _paintedFrameCount; }
    }

    public bool HasPendingRequest
    {
        get { lock (_sync) return _hasPendingRequest; }
    }

    public bool IsFrameAwaitingOrScheduled
    {
        get
        {
            lock (_sync)
            {
                return _state is FrameGateState.Scheduled or FrameGateState.AwaitingPaint || _hasPendingRequest;
            }
        }
    }

    public bool HasActiveTicket
    {
        get
        {
            lock (_sync)
            {
                return _activeTicketId != 0 || _state == FrameGateState.Painting;
            }
        }
    }

    public bool RequestFrame(long nowMs = 0)
    {
        lock (_sync)
        {
            _requestedFrameCount++;
            _lastRequestTimeMs = nowMs;
            _lastRequestTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            if (_state == FrameGateState.Idle)
            {
                _state = FrameGateState.Scheduled;
                return true;
            }

            // If already Scheduled, AwaitingPaint, or Painting, record single pending request for latest state
            _hasPendingRequest = true;
            return false;
        }
    }

    public void MarkAwaitingPaint()
    {
        lock (_sync)
        {
            if (_state == FrameGateState.Scheduled)
            {
                _state = FrameGateState.AwaitingPaint;
            }
        }
    }

    public FrameTicket? TryBeginPaint(long surfaceGeneration, long nowMs = 0)
    {
        lock (_sync)
        {
            if (_activeTicketId != 0 || _state == FrameGateState.Painting)
            {
                return null;
            }

            if (surfaceGeneration < _currentSurfaceGeneration)
            {
                // Obsolete surface callback; drop
                return null;
            }

            if (surfaceGeneration > _currentSurfaceGeneration)
            {
                _currentSurfaceGeneration = surfaceGeneration;
            }

            _state = FrameGateState.Painting;
            _activeTicketId = ++_nextTicketId;
            return new FrameTicket(_activeTicketId, _currentSurfaceGeneration, nowMs);
        }
    }

    public bool EndPaint(FrameTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        lock (_sync)
        {
            if (ticket.TicketId != _activeTicketId || ticket.SurfaceGeneration != _currentSurfaceGeneration)
            {
                // Mismatched or obsolete completion
                return false;
            }

            _paintedFrameCount++;
            _activeTicketId = 0;

            if (_hasPendingRequest)
            {
                _hasPendingRequest = false;
                _state = FrameGateState.Scheduled;
                return true; // schedule next frame immediately for latest state
            }

            _state = FrameGateState.Idle;
            return false;
        }
    }

    public void InvalidateSurface(long newSurfaceGeneration)
    {
        lock (_sync)
        {
            _currentSurfaceGeneration = newSurfaceGeneration;
            _state = FrameGateState.Idle;
            _hasPendingRequest = false;
            _activeTicketId = 0;
        }
    }

    public void Reset()
    {
        lock (_sync)
        {
            _state = FrameGateState.Idle;
            _hasPendingRequest = false;
            _activeTicketId = 0;
        }
    }
}
