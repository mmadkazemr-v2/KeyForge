namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises recognising the distance between two notes.
/// </summary>
public sealed class IntervalExerciseDefinition : ExerciseDefinition
{
    /// <summary>
    /// The intervals covered, expressed with standard abbreviations, e.g. ["P5", "M3", "m7"].
    /// An empty list means no interval constraint.
    /// </summary>
    public List<string> Intervals { get; set; } = [];

    /// <summary>Direction(s) the interval should be recognised in.</summary>
    public ExerciseDirection Direction { get; set; } = ExerciseDirection.Ascending;

    /// <summary>
    /// Fixed starting note for the interval, e.g. "C4". Empty means the start note is free/random.
    /// </summary>
    public string StartingNote { get; set; } = string.Empty;

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public IntervalExerciseDefinition() => Type = ExerciseType.Interval;
}
