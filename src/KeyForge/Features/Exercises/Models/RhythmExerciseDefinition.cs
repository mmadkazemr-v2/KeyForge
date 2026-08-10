namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises timing, subdivisions and reading rhythm patterns.
/// </summary>
public sealed class RhythmExerciseDefinition : ExerciseDefinition
{
    /// <summary>Meter the exercise is performed in, e.g. "4/4" or "6/8".</summary>
    public string TimeSignature { get; set; } = "4/4";

    /// <summary>
    /// Note values included in the exercise, e.g. ["quarter", "eighth", "sixteenth"].
    /// </summary>
    public List<string> NoteValues { get; set; } = [];

    /// <summary>Number of rhythm patterns (bars/measures) to practise.</summary>
    public int PatternCount { get; set; } = 1;

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public RhythmExerciseDefinition() => Type = ExerciseType.Rhythm;
}
