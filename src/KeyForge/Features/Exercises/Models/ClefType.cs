namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// The staff clef used by a note-reading exercise.
/// </summary>
public enum ClefType
{
    /// <summary>Treble (G) clef, typically read with the right hand.</summary>
    Treble,

    /// <summary>Bass (F) clef, typically read with the left hand.</summary>
    Bass,

    /// <summary>Both treble and bass clefs together on a grand staff.</summary>
    Grand
}
