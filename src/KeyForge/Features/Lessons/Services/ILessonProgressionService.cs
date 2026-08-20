namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Evaluates lesson unlock state based on the learner's stored progress.
/// <para>
/// The service is intentionally read-only. It never mutates progress or
/// lesson definitions. Progression logic lives here rather than inside
/// domain models or the progress store.
/// </para>
/// </summary>
public interface ILessonProgressionService
{
    /// <summary>
    /// Determines whether a lesson is currently available to the learner.
    /// </summary>
    /// <param name="lessonId">
    /// The stable identifier of the lesson to evaluate.
    /// </param>
    /// <returns>
    /// <c>true</c> when the lesson is unlocked; <c>false</c> when it is
    /// locked or when the lesson does not exist.
    /// </returns>
    bool IsUnlocked(string lessonId);
}
