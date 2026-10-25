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
    private readonly IExerciseAttemptRecorder _attemptRecorder;
    private readonly object _updateLock = new();

    /// <summary>
    /// Creates a new <see cref="ProgressUpdateService"/>.
    /// </summary>
    public ProgressUpdateService(
        IProgressStore progressStore,
        ILessonCatalog lessonCatalog,
        ILessonProgressionService progressionService,
        IExerciseAttemptRecorder attemptRecorder)
    {
        ArgumentNullException.ThrowIfNull(progressStore);
        ArgumentNullException.ThrowIfNull(lessonCatalog);
        ArgumentNullException.ThrowIfNull(progressionService);
        ArgumentNullException.ThrowIfNull(attemptRecorder);

        _progressStore = progressStore;
        _lessonCatalog = lessonCatalog;
        _progressionService = progressionService;
        _attemptRecorder = attemptRecorder;
    }

    /// <inheritdoc />
    public void UpdateProgress(string lessonId, SessionResult result)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        ArgumentNullException.ThrowIfNull(result);
        if (result.Attempt.CompletedAt is null || result.Attempt.Score is null)
        {
            return;
        }

        lock (_updateLock)
        {
            var lesson = _lessonCatalog.GetById(lessonId);
            if (lesson is null)
            {
                return;
            }

            if (!string.Equals(lessonId, result.Attempt.LessonId, StringComparison.Ordinal))
            {
                throw new ArgumentException("The attempt lesson ID must match the progress lesson ID.", nameof(lessonId));
            }

            // The practice service may already have recorded this attempt. Recorder identity makes
            // this safe and lets progress updates recover from a missed record call.
            _attemptRecorder.RecordIfNew(result.Attempt);

            var prior = _progressStore.GetProgress(lessonId);
            var progress = prior ?? new LessonProgress { LessonId = lessonId };
            var attempts = _attemptRecorder.GetAttemptsByLesson(lessonId)
                .Where(attempt => attempt.CompletedAt is not null && attempt.Score is not null)
                .ToArray();

            progress.AttemptCount = attempts.Length;
            var attemptsBestScore = attempts.Max(attempt => attempt.Score);
            if (attemptsBestScore is not null &&
                (progress.BestScore is null || attemptsBestScore.Value > progress.BestScore.Value))
            {
                progress.BestScore = attemptsBestScore;
            }

            // Save the updated projection before querying completion; some stores return snapshots.
            _progressStore.SaveProgress(progress);
            progress.IsCompleted = progress.IsCompleted || _progressionService.IsCompleted(lessonId);

            _progressStore.SaveProgress(progress);
        }
    }
}
