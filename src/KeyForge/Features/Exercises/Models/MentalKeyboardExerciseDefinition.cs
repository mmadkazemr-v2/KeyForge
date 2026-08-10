namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises visualising the keyboard mentally,
/// without looking at the hands, to strengthen keyboard geography.
/// </summary>
public sealed class MentalKeyboardExerciseDefinition : ExerciseDefinition
{
    /// <summary>
    /// Starting note for the visualisation, e.g. "C4". Empty means no fixed start.
    /// </summary>
    public string StartingNote { get; set; } = string.Empty;

    /// <summary>Direction the mental navigation moves.</summary>
    public ExerciseDirection Direction { get; set; } = ExerciseDirection.Ascending;

    /// <summary>Number of note-steps to navigate mentally.</summary>
    public int Steps { get; set; } = 4;

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public MentalKeyboardExerciseDefinition() => Type = ExerciseType.MentalKeyboard;
}
