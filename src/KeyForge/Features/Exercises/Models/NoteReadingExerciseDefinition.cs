namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises reading notes from the staff and finding them on the keyboard.
/// </summary>
public sealed class NoteReadingExerciseDefinition : ExerciseDefinition
{
    /// <summary>Which clef(s) the notes are presented in.</summary>
    public ClefType Clef { get; set; } = ClefType.Treble;

    /// <summary>
    /// The note range covered, as a "low-high" pair, e.g. "C4-G5".
    /// </summary>
    public string Range { get; set; } = string.Empty;

    /// <summary>
    /// The key signature the exercise is written in, e.g. "C major" or "G major".
    /// </summary>
    public string KeySignature { get; set; } = string.Empty;

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public NoteReadingExerciseDefinition() => Type = ExerciseType.NoteReading;
}
