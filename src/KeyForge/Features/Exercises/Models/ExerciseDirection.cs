namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// The direction an exercise moves across the keyboard or in pitch.
/// Shared by interval, octave and mental-keyboard exercises.
/// </summary>
public enum ExerciseDirection
{
    /// <summary>Moves from lower to higher pitch.</summary>
    Ascending,

    /// <summary>Moves from higher to lower pitch.</summary>
    Descending,

    /// <summary>Alternates between ascending and descending.</summary>
    Both
}
