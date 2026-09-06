using MobilDwg.Rendering.Camera;

namespace MobilDwg.Rendering.Interaction;

public enum ViewportGestureState
{
    Idle = 0,
    TapCandidate = 1,
    Pan = 2,
    Pinch = 3,
    MultiTouchHold = 4,
    Suspended = 5,
}

public enum PointerAction
{
    Down = 0,
    Up = 1,
    Move = 2,
    PointerDown = 3,
    PointerUp = 4,
    Cancel = 5,
}

public readonly record struct PointerSample(int Id, ScreenPoint2 Position);

public sealed class PointerPacket
{
    public PointerPacket(
        PointerAction action,
        int actionPointerId,
        int actionIndex,
        long eventTimeMs,
        IReadOnlyList<PointerSample> pointers,
        int surfaceGeneration)
    {
        Action = action;
        ActionPointerId = actionPointerId;
        ActionIndex = actionIndex;
        EventTimeMs = eventTimeMs;
        Pointers = pointers ?? throw new ArgumentNullException(nameof(pointers));
        SurfaceGeneration = surfaceGeneration;
    }

    public PointerAction Action { get; }
    public int ActionPointerId { get; }
    public int ActionIndex { get; }
    public long EventTimeMs { get; }
    public IReadOnlyList<PointerSample> Pointers { get; }
    public int SurfaceGeneration { get; }
}

public sealed record ViewportInputConfiguration
{
    public static ViewportInputConfiguration Default { get; } = new();

    public double TouchSlopPx { get; init; } = 8.0;
    public double DoubleTapSlopPx { get; init; } = 24.0;
    public long DoubleTapTimeoutMs { get; init; } = 300;
    public double MinSpanPx => Math.Max(8.0, 2.0 * TouchSlopPx);
    public double ZoomButtonFactor { get; init; } = ViewerZoomPolicy.ButtonZoomFactor;
    public double DoubleTapZoomFactor { get; init; } = ViewerZoomPolicy.DoubleTapZoomFactor;
    public double FitPaddingFraction { get; init; } = ViewerZoomPolicy.DefaultPaddingFraction;
}
