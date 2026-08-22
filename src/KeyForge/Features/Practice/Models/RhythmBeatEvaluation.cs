namespace KeyForge.Features.Practice.Models;

/// <summary>Evaluation of one expected beat against an optional captured hit.</summary>
public sealed class RhythmBeatEvaluation
{
    public required int BeatNumber { get; init; }
    public required TimeSpan ExpectedOffset { get; init; }
    public TimeSpan? ActualOffset { get; init; }
    public TimeSpan? TimingError { get; init; }
    public required RhythmHitAccuracy Accuracy { get; init; }
}
