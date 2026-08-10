namespace KeyForge.Features.Lessons.Models;

/// <summary>
/// Describes when a lesson becomes available to the learner.
/// <para>
/// The rule lives in the lesson content so that adding or reordering lessons
/// never requires changing application code. The progression engine evaluates
/// the rule against the learner's stored progress.
/// </para>
/// </summary>
public class UnlockRule
{
    /// <summary>
    /// The strategy that controls when the lesson unlocks.
    /// Defaults to <see cref="UnlockMode.Immediate"/> so a lesson with no
    /// explicit unlock rule is available right away.
    /// </summary>
    public UnlockMode Mode { get; set; } = UnlockMode.Immediate;

    /// <summary>
    /// Identifiers of the lessons that must be completed before this lesson
    /// unlocks. Only relevant when <see cref="Mode"/> is
    /// <see cref="UnlockMode.PrerequisitesCompleted"/>; empty in all other modes.
    /// </summary>
    public List<string> RequiredLessonIds { get; set; } = [];
}
