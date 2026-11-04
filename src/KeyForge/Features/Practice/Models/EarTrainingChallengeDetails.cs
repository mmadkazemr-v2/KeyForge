namespace KeyForge.Features.Practice.Models;

/// <summary>Presentation-neutral data for an ear-training challenge.</summary>
public sealed class EarTrainingChallengeDetails
{
    public required EarTrainingTask TaskType { get; init; }
    public required IReadOnlyList<string> AvailableNotes { get; init; }

    /// <summary>Interval abbreviations offered as answer choices for interval recognition.</summary>
    public IReadOnlyList<string> IntervalChoices { get; init; } = [];

    /// <summary>One melodic-interval question per round for interval recognition.</summary>
    public IReadOnlyList<EarTrainingIntervalQuestion> IntervalQuestions { get; init; } = [];

    /// <summary>One root-position triad and its quality for chord recognition.</summary>
    public IReadOnlyList<EarTrainingChordQuestion> ChordQuestions { get; init; } = [];

    /// <summary>Chord-quality answers offered for chord recognition.</summary>
    public IReadOnlyList<string> ChordChoices { get; init; } = [];
}

/// <summary>A close-position triad used as an auditory chord-recognition stimulus.</summary>
public sealed class EarTrainingChordQuestion
{
    public required string RootNote { get; init; }
    public required string Quality { get; init; }
    public required IReadOnlyList<string> Notes { get; init; }
}

/// <summary>One source/target note pair with the expected melodic interval.</summary>
public sealed class EarTrainingIntervalQuestion
{
    public required string SourceNote { get; init; }
    public required string TargetNote { get; init; }
    public required string Interval { get; init; }
    public required ExerciseDirection Direction { get; init; }
}
