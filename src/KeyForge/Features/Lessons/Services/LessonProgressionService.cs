namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Evaluates lesson unlock state based on the learner's stored progress.
/// </summary>
public sealed class LessonProgressionService(
    ILessonCatalog catalog,
    IProgressStore progressStore,
    IExerciseCompletionEvaluator exerciseCompletionEvaluator
) : ILessonProgressionService
{
    private readonly ILessonCatalog _catalog = catalog;
    private readonly IProgressStore _progressStore = progressStore;
    private readonly IExerciseCompletionEvaluator _exerciseCompletionEvaluator = exerciseCompletionEvaluator;

    /// <inheritdoc />
    public bool IsUnlocked(string lessonId)
    {
        ArgumentNullException.ThrowIfNull(lessonId);

        var lesson = _catalog.GetById(lessonId);

        if (lesson is null)
        {
            return false;
        }

        return lesson.Unlock.Mode switch
        {
            UnlockMode.Immediate => true,
            UnlockMode.PreviousLessonCompleted => IsPreviousCompleted(lesson),
            UnlockMode.PrerequisitesCompleted => ArePrerequisitesCompleted(lesson.Unlock),
            _ => false
        };
    }

    /// <inheritdoc />
    public bool IsCompleted(string lessonId)
    {
        ArgumentNullException.ThrowIfNull(lessonId);

        var lesson = _catalog.GetById(lessonId);

        if (lesson is null)
        {
            return false;
        }

        var progress = _progressStore.GetProgress(lessonId);

        if (progress is null)
        {
            return false;
        }

        if (lesson.Completion.MinimumScore is { } minScore &&
            !_exerciseCompletionEvaluator.HasSuccessfulAttempt(lessonId, minScore) &&
            !(progress.IsCompleted && progress.BestScore >= minScore))
        {
            return false;
        }

        // Trust an already completed state after the current score requirement passes.
        // BestScore is monotonic, so a lower retry cannot revoke legitimate completion.
        if (progress.IsCompleted)
        {
            return true;
        }

        return lesson.Completion.RequireAllExercises
            ? _exerciseCompletionEvaluator.AreAllExercisesCompleted(lessonId, lesson.Exercises)
            : _exerciseCompletionEvaluator.HasSuccessfulAttempt(lessonId);
    }

    private bool IsPreviousCompleted(LessonDefinition lesson)
    {
        var allLessons = _catalog.GetAll();
        LessonDefinition? previous = null;

        for (var i = allLessons.Count - 1; i >= 0; i--)
        {
            if (allLessons[i].Order >= lesson.Order)
            {
                continue;
            }

            previous = allLessons[i];

            break;
        }

        return previous is null || IsPrerequisiteMet(previous.Id);
    }

    private bool ArePrerequisitesCompleted(UnlockRule unlock)
    {
        return unlock.RequiredLessonIds.All(IsPrerequisiteMet);
    }

    private bool IsPrerequisiteMet(string lessonId)
    {
        return IsCompleted(lessonId);
    }
}
