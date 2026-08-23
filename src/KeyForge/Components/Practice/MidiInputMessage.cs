namespace KeyForge.Components.Practice;

/// <summary>Raw standard MIDI message data received from the browser input adapter.</summary>
public sealed class MidiInputMessage
{
    public required string DeviceId { get; init; }
    public required int Status { get; init; }
    public required int Data1 { get; init; }
    public required int Data2 { get; init; }
    public double TimestampMilliseconds { get; init; }
}
