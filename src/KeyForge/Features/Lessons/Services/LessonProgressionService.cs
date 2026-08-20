namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Evaluates lesson unlock state based on the learner's stored progress.
/// </summary>
public sealed class LessonProgressionService : ILessonProgressionService
{
    private readonly ILessonCatalog _catalog;
    private readonly IProgressStore _progressStore;

    public LessonProgressionService(ILessonCatalog catalog, IProgressStore progressStore)
    {
        _catalog = catalog;
        _progressStore = progressStore;
    }

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

        if (!progress.IsCompleted)
        {
            return false;
        }

        if (lesson.Completion.RequireAllExercises)
        {
            return false;
        }

        if (lesson.Completion.MinimumScore is { } minScore)
        {
            if (progress.BestScore is null || progress.BestScore < minScore)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsPreviousCompleted(LessonDefinition lesson)
    {
        var allLessons = _catalog.GetAll();
        LessonDefinition? previous = null;

        for (var i = allLessons.Count - 1; i >= 0; i--)
        {
            if (allLessons[i].Order < lesson.Order)
            {
                previous = allLessons[i];
                break;
            }
        }

        if (previous is null)
        {
            return true;
        }

        return IsPrerequisiteMet(previous.Id);
    }

    private bool ArePrerequisitesCompleted(UnlockRule unlock)
    {
        foreach (var requiredId in unlock.RequiredLessonIds)
        {
            if (!IsPrerequisiteMet(requiredId))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsPrerequisiteMet(string lessonId)
    {
        var progress = _progressStore.GetProgress(lessonId);
        return progress is not null && progress.IsCompleted;
    }
}
