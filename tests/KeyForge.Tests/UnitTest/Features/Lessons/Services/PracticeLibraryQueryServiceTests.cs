namespace KeyForge.Tests.UnitTest.Features.Lessons.Services;

public sealed class PracticeLibraryQueryServiceTests
{
    [Fact]
    public void GetCourses_GroupsLegacyAndExplicitContent_AndDerivesCountsFromSuccessfulExerciseAttempts()
    {
        var lentoCourse = new CourseMetadata
        {
            Id = "lento-vivo-piano",
            Name = "لنتوویوو — دوره پیانو",
            Description = "LentoVivo piano practice",
            Order = 20
        };
        var legacyLesson = Lesson("lesson-01", "Foundations", 1,
            [Exercise("legacy-first"), Exercise("legacy-second")]);
        var lentoSessionFive = Lesson("lento-session-05", "Session 05", 105,
            [Exercise("lento-s05-one"), Exercise("lento-s05-two")], course: lentoCourse);
        var lentoSessionFour = Lesson("lento-session-04", "Session 04", 104,
            [Exercise("lento-s04-one"), Exercise("lento-s04-two")], course: lentoCourse);
        var emptyCourseLesson = Lesson("empty-collection", "Empty", 300, [],
            new CourseMetadata { Id = "empty-course", Name = "Empty", Order = 1 });
        var catalog = new FakeLessonCatalog(legacyLesson, lentoSessionFive, lentoSessionFour, emptyCourseLesson);
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(Attempt(legacyLesson.Id, "legacy-first", successful: true));
        recorder.Record(Attempt(legacyLesson.Id, "legacy-second", successful: false));
        recorder.Record(Attempt(lentoSessionFive.Id, "lento-s05-one", successful: true));
        var progressStore = new InMemoryProgressStore();
        var completion = new ExerciseCompletionEvaluator(recorder);
        var progression = new LessonProgressionService(catalog, progressStore, completion);
        var lessonProgress = new LessonProgressQueryService(catalog, progressStore, progression);
        var library = new PracticeLibraryQueryService(catalog, lessonProgress, completion);
        var attemptCountBeforeQuery = recorder.GetAllAttempts().Count;

        var courses = library.GetCourses();

        Assert.Equal(2, courses.Count);
        Assert.Equal(["keyforge-fundamentals", "lento-vivo-piano"], courses.Select(course => course.Metadata.Id));
        Assert.Equal("KeyForge Fundamentals", courses[0].Metadata.Name);
        Assert.Equal("لنتوویوو — دوره پیانو", courses[1].Metadata.Name);
        Assert.Equal(1, courses[0].CompletedExerciseCount);
        Assert.Equal(2, courses[0].ExerciseCount);
        Assert.Equal(1, courses[1].CompletedExerciseCount);
        Assert.Equal(4, courses[1].ExerciseCount);

        Assert.Equal(["lento-session-04", "lento-session-05"],
            courses[1].Sessions.Select(session => session.Lesson.Id));
        Assert.Equal(0, courses[1].Sessions[0].CompletedExerciseCount);
        Assert.Equal(1, courses[1].Sessions[1].CompletedExerciseCount);
        Assert.False(courses[1].Sessions[1].Exercises[1].IsCompleted);
        Assert.Equal(attemptCountBeforeQuery, recorder.GetAllAttempts().Count);
    }

    [Fact]
    public void GetCourses_EmptyCatalogReturnsNoEmptyCourseNodes()
    {
        var catalog = new FakeLessonCatalog();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var progressStore = new InMemoryProgressStore();
        var completion = new ExerciseCompletionEvaluator(recorder);
        var progression = new LessonProgressionService(catalog, progressStore, completion);
        var lessonProgress = new LessonProgressQueryService(catalog, progressStore, progression);

        var library = new PracticeLibraryQueryService(catalog, lessonProgress, completion);

        Assert.Empty(library.GetCourses());
    }

    private static LessonDefinition Lesson(
        string id,
        string title,
        int order,
        IReadOnlyList<ExerciseDefinition> exercises,
        CourseMetadata? course = null) => new()
        {
            Id = id,
            Title = title,
            Order = order,
            Course = course,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises = exercises.ToList()
        };

    private static SequenceExerciseDefinition Exercise(string id) => new()
    {
        Id = id,
        Title = id,
        Sequence = ["C4"]
    };

    private static ExerciseAttempt Attempt(string lessonId, string exerciseId, bool successful) => new()
    {
        LessonId = lessonId,
        ExerciseId = exerciseId,
        StartedAt = DateTime.UtcNow.AddMinutes(-1),
        CompletedAt = DateTime.UtcNow,
        Score = successful ? 80 : 20,
        IsSuccessful = successful
    };

    private sealed class FakeLessonCatalog(params LessonDefinition[] lessons) : ILessonCatalog
    {
        private readonly IReadOnlyList<LessonDefinition> _lessons = Array.AsReadOnly(lessons);

        public IReadOnlyList<LessonDefinition> GetAll() => _lessons;

        public LessonDefinition? GetById(string id) => _lessons.FirstOrDefault(lesson =>
            string.Equals(lesson.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
