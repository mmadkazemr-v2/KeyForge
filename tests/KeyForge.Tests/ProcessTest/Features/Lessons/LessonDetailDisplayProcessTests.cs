namespace KeyForge.Tests.ProcessTest.Features.Lessons;

/// <summary>
/// Verifies the catalog and progress-query contracts consumed by the Lesson Detail page.
/// Razor rendering remains compile-validated without introducing a component-test dependency.
/// </summary>
public sealed class LessonDetailDisplayProcessTests
{
    [Fact]
    public void ExistingUnlockedLesson_ExposesMetadataExercisesAndStartEligibility()
    {
        var lesson = CreateLesson("lesson-details", UnlockMode.Immediate);
        lesson.Exercises.Add(new RhythmExerciseDefinition
        {
            Id = "rhythm-01",
            Title = "Steady Pulse",
            Description = "Keep a consistent beat.",
            Duration = 5,
            Tempo = 80,
            Difficulty = Difficulty.Easy
        });
        var (catalog, query) = CreateServices([lesson], new InMemoryProgressStore());

        var definition = catalog.GetById("lesson-details");
        var state = Assert.Single(query.GetLessons());

        Assert.NotNull(definition);
        Assert.Equal("Lesson lesson-details", definition.Title);
        Assert.Equal("A focused piano lesson.", definition.Description);
        Assert.Equal(LessonLevel.Beginner, definition.Level);
        Assert.Equal(20, definition.EstimatedMinutes);
        var exercise = Assert.Single(definition.Exercises);
        Assert.Equal(ExerciseType.Rhythm, exercise.Type);
        Assert.Equal("Steady Pulse", exercise.Title);
        Assert.Equal(5, exercise.Duration);
        Assert.Equal(80, exercise.Tempo);
        Assert.True(state.IsUnlocked);
    }

    [Fact]
    public void CompletedLesson_ExposesCompletedStateAndBestScore()
    {
        var lesson = CreateLesson("lesson-completed", UnlockMode.Immediate);
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = lesson.Id,
            IsCompleted = true,
            BestScore = 94,
            AttemptCount = 2
        });
        var (_, query) = CreateServices([lesson], store);

        var state = Assert.Single(query.GetLessons());

        Assert.True(state.IsUnlocked);
        Assert.True(state.IsCompleted);
        Assert.Equal(94, state.BestScore);
    }

    [Fact]
    public void LockedLesson_IsExposedButIsNotEligibleToStart()
    {
        var prerequisite = CreateLesson("lesson-prerequisite", UnlockMode.Immediate, order: 1);
        var locked = CreateLesson("lesson-locked", UnlockMode.PreviousLessonCompleted, order: 2);
        var (catalog, query) = CreateServices([prerequisite, locked], new InMemoryProgressStore());

        var definition = catalog.GetById(locked.Id);
        var state = query.GetLessons().Single(item => item.Id == locked.Id);

        Assert.NotNull(definition);
        Assert.False(state.IsUnlocked);
        Assert.False(state.IsCompleted);
    }

    [Fact]
    public void MissingLesson_IsNotExposedByDetailContracts()
    {
        var existing = CreateLesson("lesson-existing", UnlockMode.Immediate);
        var (catalog, query) = CreateServices([existing], new InMemoryProgressStore());

        var definition = catalog.GetById("lesson-missing");
        var state = query.GetLessons().SingleOrDefault(item => item.Id == "lesson-missing");

        Assert.Null(definition);
        Assert.Null(state);
    }

    private static LessonDefinition CreateLesson(string id, UnlockMode unlockMode, int order = 1) => new()
    {
        Id = id,
        Title = $"Lesson {id}",
        Description = "A focused piano lesson.",
        Level = LessonLevel.Beginner,
        Order = order,
        EstimatedMinutes = 20,
        Unlock = new UnlockRule { Mode = unlockMode }
    };

    private static (FakeLessonCatalog Catalog, LessonProgressQueryService Query) CreateServices(
        IReadOnlyList<LessonDefinition> lessons,
        InMemoryProgressStore store)
    {
        var catalog = new FakeLessonCatalog(lessons);
        var completionEvaluator = new ExerciseCompletionEvaluator(new InMemoryExerciseAttemptRecorder());
        var progression = new LessonProgressionService(catalog, store, completionEvaluator);
        return (catalog, new LessonProgressQueryService(catalog, store, progression));
    }

    private sealed class FakeLessonCatalog : ILessonCatalog
    {
        private readonly IReadOnlyList<LessonDefinition> _lessons;

        public FakeLessonCatalog(IReadOnlyList<LessonDefinition> lessons)
        {
            _lessons = [.. lessons.OrderBy(lesson => lesson.Order)];
        }

        public IReadOnlyList<LessonDefinition> GetAll() => _lessons;

        public LessonDefinition? GetById(string id) =>
            _lessons.FirstOrDefault(lesson =>
                string.Equals(lesson.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
