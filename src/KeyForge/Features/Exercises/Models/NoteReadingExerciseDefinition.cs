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

    /// <summary>Structured chromatic range; additive alternative to the legacy range string.</summary>
    public PitchRangeDefinition? NoteRange { get; set; }

    /// <summary>Explicit note pool. Null means use the configured range.</summary>
    public List<string>? Notes { get; set; }

    /// <summary>Number of notes to generate; defaults to the legacy five-question drill.</summary>
    public int Count { get; set; } = 5;

    /// <summary>True selects random notes, false cycles deterministically; null preserves legacy spread.</summary>
    public bool? Randomized { get; set; }

    /// <summary>
    /// The key signature the exercise is written in, e.g. "C major" or "G major".
    /// </summary>
    public string KeySignature { get; set; } = string.Empty;

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public NoteReadingExerciseDefinition() => Type = ExerciseType.NoteReading;
}
