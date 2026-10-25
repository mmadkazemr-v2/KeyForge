namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises playing a pattern at increasingly high tempos.
/// <see cref="ExerciseDefinition.Tempo"/> holds the optional starting-tempo configuration;
/// <see cref="TargetTempo"/> holds the tempo to work toward.
/// </summary>
public sealed class SpeedExerciseDefinition : ExerciseDefinition
{
    /// <summary>Which pattern is drilled for speed.</summary>
    public SpeedPattern Pattern { get; set; } = SpeedPattern.Scale;

    /// <summary>Final tempo (in BPM) the exercise works toward.</summary>
    public int TargetTempo { get; set; }

    /// <summary>Number of times the pattern should be repeated per set.</summary>
    public int Repetitions { get; set; } = 10;

    /// <summary>Optional authored pitch order used instead of the legacy pattern.</summary>
    public List<string>? Sequence { get; set; }

    /// <summary>Optional formula-based scale used instead of the legacy pattern.</summary>
    public ScaleConfiguration? Scale { get; set; }

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public SpeedExerciseDefinition() => Type = ExerciseType.Speed;
}
