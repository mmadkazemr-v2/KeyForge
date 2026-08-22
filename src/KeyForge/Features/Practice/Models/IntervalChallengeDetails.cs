namespace KeyForge.Features.Practice.Models;

/// <summary>Presentation-neutral context for generated interval questions.</summary>
public sealed class IntervalChallengeDetails
{
    public required IReadOnlyList<IntervalQuestion> Questions { get; init; }
}

/// <summary>One source note and interval instruction within an interval challenge.</summary>
public sealed class IntervalQuestion
{
    public required string SourceNote { get; init; }
    public required string Interval { get; init; }
    public required ExerciseDirection Direction { get; init; }
}
