namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// The type of pattern a speed exercise drills at increasing tempo.
/// </summary>
public enum SpeedPattern
{
    /// <summary>Runs a scale pattern (major/minor scale, or arpeggiated scale fragment).</summary>
    Scale,

    /// <summary>Runs a broken chord / arpeggio pattern.</summary>
    Arpeggio,

    /// <summary>Repeats the same note rapidly.</summary>
    RepeatedNote,

    /// <summary>Moves chromatically by half steps.</summary>
    Chromatic
}
