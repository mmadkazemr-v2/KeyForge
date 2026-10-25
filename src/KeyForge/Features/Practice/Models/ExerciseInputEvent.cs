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
    /// <summary>Optional input velocity supplied by a capable adapter such as MIDI.</summary>
    public int? Velocity { get; init; }
    public MidiEventKind? MidiKind { get; init; }
    public string? DeviceId { get; init; }
    public string? DeviceName { get; init; }
    public int? MidiChannel { get; init; }
    public int? MidiData1 { get; init; }
    public int? MidiData2 { get; init; }
    /// <summary>Monotonic browser timestamp captured at the original MIDI event.</summary>
    public double? BrowserTimestampMilliseconds { get; init; }
    /// <summary>Increasing order assigned in the browser MIDI message handler.</summary>
    public long? Sequence { get; init; }
}

public enum MidiEventKind
{
    NoteOn,
    NoteOff,
    ControlChange,
    PitchBend
}
