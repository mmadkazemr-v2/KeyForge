namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// Relative difficulty of an exercise within a lesson.
/// Useful for ordering exercises and adapting the session to the student.
/// </summary>
public enum Difficulty
{
    /// <summary>Accessible to beginners; little coordination required.</summary>
    Easy,

    /// <summary>Requires basic coordination and some preparation.</summary>
    Medium,

    /// <summary>Requires good coordination and control.</summary>
    Hard,

    /// <summary>Requires significant skill and endurance.</summary>
    Expert
}
