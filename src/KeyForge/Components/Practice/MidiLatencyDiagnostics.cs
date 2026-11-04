namespace KeyForge.Components.Practice;

/// <summary>Bounded browser-side MIDI latency sample summaries.</summary>
public sealed class MidiLatencyDiagnostics
{
    public bool Enabled { get; init; }
    public int Capacity { get; init; }
    public int InvalidSamples { get; init; }
    public MidiLatencyMetric CallbackDelay { get; init; } = new();
    public MidiLatencyMetric CallbackDuration { get; init; } = new();
    public MidiLatencyMetric InteropAckRoundTrip { get; init; } = new();
}

public sealed class MidiLatencyMetric
{
    public int SampleCount { get; init; }
    public double? Median { get; init; }
    public double? P95 { get; init; }
    public double? Maximum { get; init; }
    public int Dropped { get; init; }
    public long? LatestSequence { get; init; }
}
