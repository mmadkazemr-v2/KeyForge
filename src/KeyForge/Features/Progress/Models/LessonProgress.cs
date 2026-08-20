namespace KeyForge.Features.Progress.Models;

/// <summary>
/// Represents the learner's recorded progress for a single lesson.
/// <para>
/// This is a pure domain model. It knows nothing about JSON, YAML, files,
/// databases or the framework. The persistence layer is responsible for
/// converting this object to and from its storage representation.
/// </para>
/// </summary>
public class LessonProgress
{
    /// <summary>
    /// Stable identifier of the lesson this progress belongs to,
    /// matching <see cref="Features.Lessons.Models.LessonDefinition.Id"/>.
    /// </summary>
    public string LessonId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the learner has met the lesson's completion requirements.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// The highest score the learner has achieved across all attempts.
    /// <c>null</c> when no scored attempt has been recorded yet.
    /// </summary>
    public int? BestScore { get; set; }

    /// <summary>
    /// Total number of times the learner has attempted this lesson.
    /// </summary>
    public int AttemptCount { get; set; }
}
