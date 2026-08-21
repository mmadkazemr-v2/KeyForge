namespace KeyForge.Tests.Features.Process;

/// <summary>
/// Process-level tests that verify the complete backend practice workflow
/// when real service implementations are wired together.
/// <para>
/// These tests use real implementations for all backend services
/// (<see cref="PracticeSessionService"/>, <see cref="ProgressUpdateService"/>,
/// <see cref="LessonProgressionService"/>, <see cref="ExerciseCompletionEvaluator"/>,
/// <see cref="ExerciseEvaluator"/>, <see cref="ExerciseScorer"/>)
/// backed by in-memory stores. Only the lesson catalog is faked.
/// </para>
/// </summary>
public sealed class PracticeFlowProcessTests
{
    private static RhythmExerciseDefinition Exercise(string id, string title) => new()
    {
        Id = id,
        Title = title,
        Type = ExerciseType.Rhythm
    };

    private static ExerciseAttempt MakeAttempt(
        string lessonId,
        string exerciseId,
        int score,
        bool completed = true) => new()
    {
        LessonId = lessonId,
        ExerciseId = exerciseId,
        StartedAt = DateTime.UtcNow,
        CompletedAt = completed ? DateTime.UtcNow : null,
        Score = completed ? score : null,
        IsSuccessful = false
    };

    /// <summary>
    /// Wires up all real services with shared in-memory stores.
    /// </summary>
    private static (PracticeSessionService session, ProgressUpdateService progress, InMemoryProgressStore store, InMemoryExerciseAttemptRecorder recorder) CreateFlow(
        params LessonDefinition[] lessons)
    {
        var catalog = new FakeLessonCatalog(lessons);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var evaluator = new ExerciseEvaluator();
        var scorer = new ExerciseScorer();
        var completionEvaluator = new ExerciseCompletionEvaluator(recorder);
        var progression = new LessonProgressionService(catalog, store, completionEvaluator);

        var sessionService = new PracticeSessionService(catalog, progression, evaluator, scorer, recorder);
        var progressService = new ProgressUpdateService(store, catalog, progression);

        return (sessionService, progressService, store, recorder);
    }

    private static void SubmitExercise(
        PracticeSessionService sessionService,
        ProgressUpdateService progressService,
        PracticeSession session,
        string lessonId,
        string exerciseId,
        int score)
    {
        var attempt = MakeAttempt(lessonId, exerciseId, score);
        var result = sessionService.SubmitAttempt(session, exerciseId, attempt);
        progressService.UpdateProgress(lessonId, result);
        session.Next();
    }

    [Fact]
    public void CompleteLesson_EndToEnd()
    {
        var ex1 = Exercise("ex-01", "Quarter Notes");
        var ex2 = Exercise("ex-02", "Half Notes");
        var ex3 = Exercise("ex-03", "Whole Notes");

        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Rhythm Basics",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises = [ex1, ex2, ex3]
        };

        var (sessionService, progressService, store, recorder) = CreateFlow(lesson);

        var session = sessionService.StartSession("lesson-01");
        Assert.NotNull(session);
        Assert.Equal("lesson-01", session.LessonId);
        Assert.Equal(3, session.Exercises.Count);

        SubmitExercise(sessionService, progressService, session, "lesson-01", "ex-01", 85);
        SubmitExercise(sessionService, progressService, session, "lesson-01", "ex-02", 90);
        SubmitExercise(sessionService, progressService, session, "lesson-01", "ex-03", 75);

        Assert.True(session.IsFinished);

        var attempts = recorder.GetAttemptsByLesson("lesson-01");
        Assert.Equal(3, attempts.Count);
        Assert.All(attempts, a => Assert.True(a.IsSuccessful));

        var progress = store.GetProgress("lesson-01");
        Assert.NotNull(progress);
        Assert.Equal(3, progress.AttemptCount);
        Assert.Equal(90, progress.BestScore);
        Assert.True(progress.IsCompleted);
    }

    [Fact]
    public void FailedAttempt_DoesNotCompleteExerciseOrLesson()
    {
        var ex1 = Exercise("ex-01", "Quarter Notes");

        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Single Exercise",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises = [ex1]
        };

        var (sessionService, progressService, store, recorder) = CreateFlow(lesson);

        var session = sessionService.StartSession("lesson-01");
        Assert.NotNull(session);

        var attempt = MakeAttempt("lesson-01", "ex-01", 0, completed: false);
        var result = sessionService.SubmitAttempt(session, "ex-01", attempt);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);

        progressService.UpdateProgress("lesson-01", result);

        var attempts = recorder.GetAttemptsByLesson("lesson-01");
        Assert.Single(attempts);
        Assert.False(attempts[0].IsSuccessful);

        var progress = store.GetProgress("lesson-01");
        Assert.NotNull(progress);
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public void FailedThenSuccessfulAttempt_CompletesExercise()
    {
        var ex1 = Exercise("ex-01", "Quarter Notes");

        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Retry Lesson",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises = [ex1]
        };

        var (sessionService, progressService, store, recorder) = CreateFlow(lesson);

        var session = sessionService.StartSession("lesson-01");
        Assert.NotNull(session);

        var failedAttempt = MakeAttempt("lesson-01", "ex-01", 0, completed: false);
        var failedResult = sessionService.SubmitAttempt(session, "ex-01", failedAttempt);
        progressService.UpdateProgress("lesson-01", failedResult);

        var progressAfterFail = store.GetProgress("lesson-01");
        Assert.NotNull(progressAfterFail);
        Assert.False(progressAfterFail.IsCompleted);

        var successfulAttempt = MakeAttempt("lesson-01", "ex-01", 85, completed: true);
        var result = sessionService.SubmitAttempt(session, "ex-01", successfulAttempt);
        progressService.UpdateProgress("lesson-01", result);

        var attempts = recorder.GetAttemptsByLesson("lesson-01");
        Assert.Equal(2, attempts.Count);
        Assert.False(attempts[0].IsSuccessful);
        Assert.True(attempts[1].IsSuccessful);

        var progress = store.GetProgress("lesson-01");
        Assert.NotNull(progress);
        Assert.True(progress.IsCompleted);
    }

    [Fact]
    public void PartialCompletion_DoesNotCompleteLesson()
    {
        var ex1 = Exercise("ex-01", "Quarter Notes");
        var ex2 = Exercise("ex-02", "Half Notes");
        var ex3 = Exercise("ex-03", "Whole Notes");

        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Three Exercises",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises = [ex1, ex2, ex3]
        };

        var (sessionService, progressService, store, recorder) = CreateFlow(lesson);

        var session = sessionService.StartSession("lesson-01");
        Assert.NotNull(session);

        SubmitExercise(sessionService, progressService, session, "lesson-01", "ex-01", 85);
        SubmitExercise(sessionService, progressService, session, "lesson-01", "ex-02", 90);

        Assert.False(session.IsFinished);

        var progress = store.GetProgress("lesson-01");
        Assert.NotNull(progress);
        Assert.False(progress.IsCompleted);

        var attempts = recorder.GetAttemptsByLesson("lesson-01");
        Assert.Equal(2, attempts.Count);
        Assert.All(attempts, a => Assert.True(a.IsSuccessful));
    }

    [Fact]
    public void InvalidAttempt_DoesNotEnterFlow()
    {
        var ex1 = Exercise("ex-01", "Quarter Notes");
        var ex2 = Exercise("ex-02", "Half Notes");

        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Two Exercises",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises = [ex1, ex2]
        };

        var (sessionService, progressService, store, recorder) = CreateFlow(lesson);

        var session = sessionService.StartSession("lesson-01");
        Assert.NotNull(session);

        var invalidAttempt = MakeAttempt("lesson-01", "ex-02", 50);

        Assert.Throws<ArgumentException>(() =>
            sessionService.SubmitAttempt(session, "ex-02", invalidAttempt));

        Assert.Empty(recorder.GetAttemptsByLesson("lesson-01"));
        Assert.Null(store.GetProgress("lesson-01"));
    }

    #region Fakes

    private sealed class FakeLessonCatalog : ILessonCatalog
    {
        private readonly Dictionary<string, LessonDefinition> _lessons;

        public FakeLessonCatalog(params LessonDefinition[] lessons)
        {
            _lessons = lessons.ToDictionary(l => l.Id, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<LessonDefinition> GetAll() =>
            _lessons.Values.ToList().AsReadOnly();

        public LessonDefinition? GetById(string id) =>
            _lessons.TryGetValue(id, out var lesson) ? lesson : null;
    }

    #endregion
}
