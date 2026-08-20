namespace KeyForge.Features.Progress.Services;

/// <summary>
/// Stores and retrieves lesson progress. Implementations are responsible for
/// persisting data; callers only ever see <see cref="LessonProgress"/> objects.
/// <para>
/// The contract is intentionally minimal. Progression logic (scoring rules,
/// completion evaluation, unlock decisions) belongs in a separate service,
/// not in the store.
/// </para>
/// </summary>
public interface IProgressStore
{
    /// <summary>
    /// Retrieves the stored progress for a single lesson, or <c>null</c> when
    /// no progress has been recorded for the given id.
    /// </summary>
    LessonProgress? GetProgress(string lessonId);

    /// <summary>
    /// Returns all progress records currently stored.
    /// </summary>
    IReadOnlyList<LessonProgress> GetAllProgress();

    /// <summary>
    /// Creates or updates the progress record for the lesson identified by
    /// <see cref="LessonProgress.LessonId"/>.
    /// </summary>
    void SaveProgress(LessonProgress progress);
}
