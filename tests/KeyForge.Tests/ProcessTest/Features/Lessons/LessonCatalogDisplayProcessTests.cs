namespace KeyForge.Tests.ProcessTest.Features.Lessons;

/// <summary>
/// Process-level tests verifying the display contract consumed by the
/// Lesson Catalog page. Wires the real catalog/progression/query services
/// together and verifies the per-lesson state matrix
/// (<see cref="LessonListItem.IsUnlocked"/>, <see cref="LessonListItem.IsCompleted"/>,
/// <see cref="LessonListItem.BestScore"/>) that the UI renders.
/// </summary>
public sealed class LessonCatalogDisplayProcessTests
{
    [Fact]
    public void GetLessons_MixedLessonStates_ExposesCorrectStateForDisplay()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "Keyboard Foundations",
                Description = "Introduction to keyboard geography.",
                Level = LessonLevel.Beginner,
                Order = 1,
                EstimatedMinutes = 30,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Title = "Rhythm Basics",
                Order = 2,
                EstimatedMinutes = 25,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            },
            new LessonDefinition
            {
                Id = "lesson-03",
                Title = "Note Reading",
                Order = 3,
                EstimatedMinutes = 20,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 90,
            AttemptCount = 4
        });

        var query = CreateQueryService(lessons, store);

        var items = query.GetLessons();

        Assert.Equal(3, items.Count);

        // Completed lesson: unlocked, completed, best score shown
        Assert.True(items[0].IsUnlocked);
        Assert.True(items[0].IsCompleted);
        Assert.Equal(90, items[0].BestScore);

        // Next lesson unlocked by previous completion: available but not started
        Assert.True(items[1].IsUnlocked);
        Assert.False(items[1].IsCompleted);
        Assert.Null(items[1].BestScore);

        // Locked lesson: still listed but marked locked
        Assert.False(items[2].IsUnlocked);
        Assert.False(items[2].IsCompleted);
        Assert.Null(items[2].BestScore);
    }

    [Fact]
    public void GetLessons_EmptyCatalog_ReturnsNoLessonsForDisplay()
    {
        var query = CreateQueryService([], new InMemoryProgressStore());

        var items = query.GetLessons();

        Assert.Empty(items);
    }

    private static LessonProgressQueryService CreateQueryService(
        IReadOnlyList<LessonDefinition> lessons,
        InMemoryProgressStore store)
    {
        var catalog = new FakeLessonCatalog(lessons);
        var completionEvaluator = new ExerciseCompletionEvaluator(new InMemoryExerciseAttemptRecorder());
        var progression = new LessonProgressionService(catalog, store, completionEvaluator);
        return new LessonProgressQueryService(catalog, store, progression);
    }

    private sealed class FakeLessonCatalog : ILessonCatalog
    {
        private readonly IReadOnlyList<LessonDefinition> _lessons;

        public FakeLessonCatalog(IReadOnlyList<LessonDefinition> lessons)
        {
            _lessons = [.. lessons.OrderBy(l => l.Order)];
        }

        public IReadOnlyList<LessonDefinition> GetAll() => _lessons;

        public LessonDefinition? GetById(string id) =>
            _lessons.FirstOrDefault(l =>
                string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
