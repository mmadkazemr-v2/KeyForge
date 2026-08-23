namespace KeyForge.Features.Practice.Models;

/// <summary>Presentation-neutral context for mental keyboard navigation questions.</summary>
public sealed class MentalKeyboardChallengeDetails
{
    public required IReadOnlyList<MentalKeyboardQuestion> Questions { get; init; }
}

/// <summary>One starting position and navigation instruction on the piano keyboard.</summary>
public sealed class MentalKeyboardQuestion
{
    public required string SourceNote { get; init; }
    public required int Steps { get; init; }
    public required ExerciseDirection Direction { get; init; }
}
