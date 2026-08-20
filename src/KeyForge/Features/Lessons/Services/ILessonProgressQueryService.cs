namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Prepares lesson information combined with the learner's progress state
/// for display in a list/grid UI. The service is intentionally read-only;
/// it never mutates progress or lesson definitions.
/// <para>
/// The returned <see cref="LessonListItem"/> objects contain no infrastructure
/// concerns (YAML, filesystem, database). The future UI depends only on this
/// contract and the view model.
/// </para>
/// </summary>
public interface ILessonProgressQueryService
{
    /// <summary>
    /// Returns all lessons from the catalog, ordered by
    /// <see cref="LessonDefinition.Order"/> (lowest first), with each lesson's
    /// current unlock/completion state and best score populated.
    /// <para>
    /// All lessons are included regardless of lock state. Missing progress is
    /// treated as "no progress" (unlocked flag may still be true, completed
    /// is false, best score is null).
    /// </para>
    /// </summary>
    /// <returns>
    /// A read-only list of <see cref="LessonListItem"/> instances. The list
    /// is ordered by <see cref="LessonListItem.Order"/>.
    /// </returns>
    IReadOnlyList<LessonListItem> GetLessons();
}
