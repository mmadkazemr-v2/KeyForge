namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Combines lesson catalog data with stored progress and progression logic
/// to produce <see cref="LessonListItem"/> instances for the future UI.
/// </summary>
public sealed class LessonProgressQueryService(
    ILessonCatalog catalog,
    IProgressStore progressStore,
    ILessonProgressionService progression
) : ILessonProgressQueryService
{
    private readonly ILessonCatalog _catalog = catalog;
    private readonly IProgressStore _progressStore = progressStore;
    private readonly ILessonProgressionService _progression = progression;

    /// <inheritdoc />
    public IReadOnlyList<LessonListItem> GetLessons()
    {
        var lessons = _catalog.GetAll();
        var items = new LessonListItem[lessons.Count];

        for (var i = 0; i < lessons.Count; i++)
        {
            var lesson = lessons[i];
            var progress = _progressStore.GetProgress(lesson.Id);

            items[i] = new LessonListItem
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Level = lesson.Level,
                Order = lesson.Order,
                EstimatedMinutes = lesson.EstimatedMinutes,
                IsUnlocked = _progression.IsUnlocked(lesson.Id),
                IsCompleted = _progression.IsCompleted(lesson.Id),
                BestScore = progress?.BestScore
            };
        }

        return items;
    }
}