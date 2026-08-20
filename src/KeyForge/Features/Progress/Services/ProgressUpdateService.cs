namespace KeyForge.Features.Progress.Services;

/// <summary>
/// Updates lesson progress after a practice attempt.
/// <para>
/// The service reads the current progress from <see cref="IProgressStore"/>,
/// updates best score and attempt count, determines lesson completion by
/// consulting <see cref="ILessonProgressionService"/>, and persists the result.
/// </para>
/// </summary>
public sealed class ProgressUpdateService : IProgressUpdateService
{
    private readonly IProgressStore _progressStore;
    private readonly ILessonCatalog _lessonCatalog;
    private readonly ILessonProgressionService _progressionService;

    /// <summary>
    /// Creates a new <see cref="ProgressUpdateService"/>.
    /// </summary>
    public ProgressUpdateService(
        IProgressStore progressStore,
        ILessonCatalog lessonCatalog,
        ILessonProgressionService progressionService)
    {
        ArgumentNullException.ThrowIfNull(progressStore);
        ArgumentNullException.ThrowIfNull(lessonCatalog);
        ArgumentNullException.ThrowIfNull(progressionService);

        _progressStore = progressStore;
        _lessonCatalog = lessonCatalog;
        _progressionService = progressionService;
    }

    /// <inheritdoc />
    public void UpdateProgress(string lessonId, SessionResult result)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        ArgumentNullException.ThrowIfNull(result);

        var lesson = _lessonCatalog.GetById(lessonId);
        if (lesson is null)
        {
            return;
        }

        var progress = _progressStore.GetProgress(lessonId)
                       ?? new LessonProgress { LessonId = lessonId };

        progress.AttemptCount++;

        if (result.Score > progress.BestScore.GetValueOrDefault())
        {
            progress.BestScore = result.Score;
        }

        progress.IsCompleted = _progressionService.IsCompleted(lessonId);

        _progressStore.SaveProgress(progress);
    }
}
