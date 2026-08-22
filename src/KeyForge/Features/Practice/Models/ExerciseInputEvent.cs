namespace KeyForge.Features.Practice.Models;

/// <summary>A normalized user input captured during a live exercise.</summary>
public sealed class ExerciseInputEvent
{
    public required DateTime Timestamp { get; init; }
    /// <summary>
    /// Monotonic elapsed time from the active challenge timeline. Browser
    /// adapters should derive this from a high-resolution monotonic clock.
    /// </summary>
    public TimeSpan Offset { get; init; }
    public required InputSource Source { get; init; }
    public required InputEventType Type { get; init; }
    public required string Value { get; init; }
}
