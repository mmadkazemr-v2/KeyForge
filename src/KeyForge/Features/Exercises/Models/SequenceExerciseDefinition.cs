namespace KeyForge.Features.Exercises.Models;

/// <summary>A pitch-only sequence authored as pitches or derived from a scale.</summary>
public sealed class SequenceExerciseDefinition : ExerciseDefinition
{
    /// <summary>Ordered pitches; ignored only when a scale configuration is supplied.</summary>
    public List<string>? Sequence { get; set; }
    public ScaleConfiguration? Scale { get; set; }
    public PracticeHand Hand { get; set; } = PracticeHand.Either;
    public List<int> Fingers { get; set; } = [];

    public SequenceExerciseDefinition() => Type = ExerciseType.Sequence;
}
