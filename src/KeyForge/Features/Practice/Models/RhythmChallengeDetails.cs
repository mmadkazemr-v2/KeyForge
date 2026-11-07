namespace KeyForge.Features.Practice.Models;

/// <summary>
/// Compiled rhythm timeline used by practice presentation and evaluation.
/// </summary>
public sealed class RhythmChallengeDetails
{
    public required IReadOnlyList<RhythmTimelineEvent> Events { get; init; }
    public required IReadOnlyList<TimeSpan> PulseOffsets { get; init; }
    public required TimeSpan Duration { get; init; }
}
