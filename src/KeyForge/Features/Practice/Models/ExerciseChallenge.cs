namespace KeyForge.Features.Practice.Models;

/// <summary>A generated question for one runtime exercise.</summary>
public sealed class ExerciseChallenge
{
    public required string ExerciseId { get; init; }
    public required ExerciseType ExerciseType { get; init; }
    public required IReadOnlyList<ExpectedInput> ExpectedInputs { get; init; }
    public IntervalChallengeDetails? Interval { get; init; }
    public OctaveChallengeDetails? Octave { get; init; }
    public MentalKeyboardChallengeDetails? MentalKeyboard { get; init; }
    public EarTrainingChallengeDetails? EarTraining { get; init; }
    public SpeedChallengeDetails? Speed { get; init; }
    public FingerIndependenceChallengeDetails? FingerIndependence { get; init; }
    public SequenceChallengeDetails? Sequence { get; init; }
}
