namespace KeyForge.Features.Practice.Models;

/// <summary>Presentation-neutral context for generated octave questions.</summary>
public sealed class OctaveChallengeDetails
{
    public required IReadOnlyList<OctaveQuestion> Questions { get; init; }
}

/// <summary>One source note and octave instruction within an octave challenge.</summary>
public sealed class OctaveQuestion
{
    public required string SourceNote { get; init; }
    public required int Octaves { get; init; }
    public required ExerciseDirection Direction { get; init; }
}
