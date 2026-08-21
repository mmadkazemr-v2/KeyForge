namespace KeyForge.Features.Lessons.Models;

/// <summary>
/// A read-only view model that combines lesson metadata with the learner's
/// current progress state. Designed for list/grid UI scenarios where the
/// future UI needs to show whether each lesson is unlocked, completed, and
/// the learner's best score.
/// <para>
/// This is a pure view model. It contains no persistence, YAML, or
/// infrastructure concerns. It is populated by
/// <see cref="Services.ILessonProgressQueryService"/>.
/// </para>
/// </summary>
public sealed class LessonListItem
{
    /// <summary>
    /// Stable, machine-readable identifier of the lesson.
    /// Matches <see cref="LessonDefinition.Id"/>.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable title shown to the student.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Optional short description of the lesson's goals.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Skill level this lesson targets.
    /// </summary>
    public LessonLevel Level { get; init; }

    /// <summary>
    /// Display/sort order of the lesson within its course or category.
    /// Lower values are shown first.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Estimated total practice time of the lesson in minutes.
    /// </summary>
    public int EstimatedMinutes { get; init; }

    /// <summary>
    /// Whether the lesson is currently available to the learner.
    /// A locked lesson still appears in the list but cannot be started.
    /// </summary>
    public bool IsUnlocked { get; init; }

    /// <summary>
    /// Whether the learner has met the lesson's completion requirements.
    /// A completed lesson is always unlocked.
    /// </summary>
    public bool IsCompleted { get; init; }

    /// <summary>
    /// The highest score the learner has achieved across all attempts.
    /// <c>null</c> when no scored attempt has been recorded yet.
    /// </summary>
    public int? BestScore { get; init; }
}