namespace KeyForge.Tests.ProcessTest.Features.Practice;

/// <summary>
/// Verifies the service composition consumed by the practice page without
/// introducing a Razor component testing dependency.
/// </summary>
public sealed class PracticeSessionUiFlowProcessTests
{
    [Fact]
    public void PracticePageContracts_SupportRetryExplicitAdvanceAndFinalProgress()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-ui",
            Title = "Practice lesson",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises =
            [
                Exercise("exercise-1", "First exercise"),
                Exercise("exercise-2", "Second exercise")
            ]
        };
        var catalog = new FakeLessonCatalog(lesson);
        var progressStore = new InMemoryProgressStore();
        var attemptRecorder = new InMemoryExerciseAttemptRecorder();
        var completionEvaluator = new ExerciseCompletionEvaluator(attemptRecorder);
        var progression = new LessonProgressionService(catalog, progressStore, completionEvaluator);
        var sessionService = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            attemptRecorder);
        var progressUpdate = new ProgressUpdateService(progressStore, catalog, progression);
        var lessonQuery = new LessonProgressQueryService(catalog, progressStore, progression);

        var session = sessionService.StartSession(lesson.Id);

        Assert.NotNull(session);
        Assert.Equal("exercise-1", session.GetCurrentExercise()!.Id);

        var failedResult = sessionService.SubmitAttempt(
            session,
            "exercise-1",
            Attempt(lesson.Id, "exercise-1", score: null, completed: false));
        progressUpdate.UpdateProgress(lesson.Id, failedResult);

        Assert.False(failedResult.IsSuccessful);
        Assert.Equal(0, session.CurrentExerciseIndex);
        Assert.Equal("exercise-1", session.GetCurrentExercise()!.Id);

        var retryResult = sessionService.SubmitAttempt(
            session,
            "exercise-1",
            Attempt(lesson.Id, "exercise-1", score: 82, completed: true));
        progressUpdate.UpdateProgress(lesson.Id, retryResult);

        Assert.True(retryResult.IsSuccessful);
        Assert.Equal(0, session.CurrentExerciseIndex);

        session.Next();

        Assert.Equal("exercise-2", session.GetCurrentExercise()!.Id);

        var finalResult = sessionService.SubmitAttempt(
            session,
            "exercise-2",
            Attempt(lesson.Id, "exercise-2", score: 91, completed: true));
        progressUpdate.UpdateProgress(lesson.Id, finalResult);

        Assert.True(finalResult.IsSuccessful);
        Assert.False(session.IsFinished);

        session.Next();

        Assert.True(session.IsFinished);
        var displayState = Assert.Single(lessonQuery.GetLessons());
        Assert.True(displayState.IsCompleted);
        Assert.Equal(91, displayState.BestScore);
    }

    private static RhythmExerciseDefinition Exercise(string id, string title) => new()
    {
        Id = id,
        Title = title,
        Type = ExerciseType.Rhythm
    };

    private static ExerciseAttempt Attempt(
        string lessonId,
        string exerciseId,
        int? score,
        bool completed) => new()
    {
        LessonId = lessonId,
        ExerciseId = exerciseId,
        StartedAt = DateTime.UtcNow,
        CompletedAt = completed ? DateTime.UtcNow : null,
        Score = score
    };

    private sealed class FakeLessonCatalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];

        public LessonDefinition? GetById(string id) =>
            string.Equals(id, lesson.Id, StringComparison.OrdinalIgnoreCase) ? lesson : null;
    }
}
