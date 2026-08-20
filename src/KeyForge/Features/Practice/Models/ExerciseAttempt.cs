namespace KeyForge.Features.Practice.Models;

/// <summary>
/// Represents the result of practicing one exercise once.
/// <para>
/// This is a pure domain model. It knows nothing about YAML, files,
/// databases, MIDI, or the framework. It captures raw attempt data
/// for future scoring and progression work.
/// </para>
/// </summary>
public sealed class ExerciseAttempt
{
    /// <summary>
    /// Stable identifier of the lesson this attempt belongs to,
    /// matching <see cref="Features.Lessons.Models.LessonDefinition.Id"/>.
    /// </summary>
    public string LessonId { get; set; } = string.Empty;

    /// <summary>
    /// Stable identifier of the exercise within the lesson,
    /// matching <see cref="Features.Exercises.Models.ExerciseDefinition.Id"/>.
    /// </summary>
    public string ExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the learner started this exercise attempt.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the learner finished this exercise attempt.
    /// <c>null</c> when the attempt has not yet been completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Numeric score achieved on this attempt.
    /// <c>null</c> when no score has been assigned yet.
    /// </summary>
    public int? Score { get; set; }

    /// <summary>
    /// Whether the attempt met the minimum requirements to be
    /// considered successful. Determined by future scoring logic.
    /// </summary>
    public bool IsSuccessful { get; set; }
}
