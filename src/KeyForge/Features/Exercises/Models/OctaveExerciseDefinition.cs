namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises recognising and/or playing octaves.
/// </summary>
public sealed class OctaveExerciseDefinition : ExerciseDefinition
{
    /// <summary>How many octaves the exercise spans (e.g. 2 = play across two octaves).</summary>
    public int Octaves { get; set; } = 2;

    /// <summary>Direction the octaves are played or recognised in.</summary>
    public ExerciseDirection Direction { get; set; } = ExerciseDirection.Both;

    /// <summary>Starting note of the exercise, e.g. "C3". Empty means no fixed start.</summary>
    public string StartingNote { get; set; } = string.Empty;

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public OctaveExerciseDefinition() => Type = ExerciseType.Octave;
}
