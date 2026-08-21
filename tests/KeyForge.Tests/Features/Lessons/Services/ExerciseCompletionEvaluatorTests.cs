namespace KeyForge.Tests.Features.Lessons.Services;

/// <summary>
/// Tests <see cref="ExerciseCompletionEvaluator"/> using an in-memory
/// exercise attempt recorder to verify exercise completion derivation.
/// </summary>
public sealed class ExerciseCompletionEvaluatorTests
{
    private static RhythmExerciseDefinition Exercise(string id) => new()
    {
        Id = id,
        Title = $"Exercise {id}",
        Type = ExerciseType.Rhythm
    };

    private static ExerciseAttempt SuccessfulAttempt(
        string lessonId, string exerciseId) => new()
    {
        LessonId = lessonId,
        ExerciseId = exerciseId,
        StartedAt = DateTime.UtcNow,
        CompletedAt = DateTime.UtcNow,
        Score = 80,
        IsSuccessful = true
    };

    private static ExerciseAttempt FailedAttempt(
        string lessonId, string exerciseId) => new()
    {
        LessonId = lessonId,
        ExerciseId = exerciseId,
        StartedAt = DateTime.UtcNow,
        CompletedAt = DateTime.UtcNow,
        Score = 30,
        IsSuccessful = false
    };

    private static ExerciseCompletionEvaluator CreateEvaluator(
        InMemoryExerciseAttemptRecorder? recorder = null)
    {
        return new ExerciseCompletionEvaluator(recorder ?? new InMemoryExerciseAttemptRecorder());
    }

    #region Basic completion tests

    [Fact]
    public void AreAllExercisesCompleted_AllSuccessful_ReturnsTrue()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-02"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01"), Exercise("ex-02") };

        Assert.True(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    [Fact]
    public void AreAllExercisesCompleted_OneIncomplete_ReturnsFalse()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(FailedAttempt("lesson-01", "ex-02"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01"), Exercise("ex-02") };

        Assert.False(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    [Fact]
    public void AreAllExercisesCompleted_NoAttempts_ReturnsFalse()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01"), Exercise("ex-02") };

        Assert.False(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    [Fact]
    public void AreAllExercisesCompleted_EmptyExercises_ReturnsTrue()
    {
        var evaluator = CreateEvaluator();

        Assert.True(evaluator.AreAllExercisesCompleted("lesson-01", []));
    }

    #endregion

    #region Failed attempt behavior

    [Fact]
    public void AreAllExercisesCompleted_FailedAttempt_DoesNotComplete()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(FailedAttempt("lesson-01", "ex-01"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01") };

        Assert.False(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    [Fact]
    public void AreAllExercisesCompleted_FailedThenSuccessful_Completes()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(FailedAttempt("lesson-01", "ex-01"));
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01") };

        Assert.True(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    [Fact]
    public void AreAllExercisesCompleted_SuccessfulThenFailed_RemainsCompleted()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(FailedAttempt("lesson-01", "ex-01"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01") };

        Assert.True(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    #endregion

    #region Identity and count behavior

    [Fact]
    public void AreAllExercisesCompleted_MultipleSuccessfulAttempts_NoIssue()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01") };

        Assert.True(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    [Fact]
    public void AreAllExercisesCompleted_BasedOnExerciseIds_NotAttemptCount()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-01"));
        recorder.Record(SuccessfulAttempt("lesson-01", "ex-02"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition>
        {
            Exercise("ex-01"),
            Exercise("ex-02"),
            Exercise("ex-03")
        };

        Assert.False(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    [Fact]
    public void AreAllExercisesCompleted_UnknownExerciseAttempts_DoNotCompleteLesson()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(SuccessfulAttempt("lesson-01", "unknown-ex"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01") };

        Assert.False(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    #endregion

    #region Lesson isolation

    [Fact]
    public void AreAllExercisesCompleted_DifferentLesson_DoesNotAffect()
    {
        var recorder = new InMemoryExerciseAttemptRecorder();
        recorder.Record(SuccessfulAttempt("lesson-02", "ex-01"));
        var evaluator = CreateEvaluator(recorder);
        var exercises = new List<ExerciseDefinition> { Exercise("ex-01") };

        Assert.False(evaluator.AreAllExercisesCompleted("lesson-01", exercises));
    }

    #endregion

    #region Argument validation

    [Fact]
    public void AreAllExercisesCompleted_NullLessonId_ThrowsArgumentNullException()
    {
        var evaluator = CreateEvaluator();

        Assert.Throws<ArgumentNullException>(() =>
            evaluator.AreAllExercisesCompleted(null!, []));
    }

    [Fact]
    public void AreAllExercisesCompleted_NullExercises_ThrowsArgumentNullException()
    {
        var evaluator = CreateEvaluator();

        Assert.Throws<ArgumentNullException>(() =>
            evaluator.AreAllExercisesCompleted("lesson-01", null!));
    }

    #endregion

    #region DI resolution test

    [Fact]
    public void Di_CanResolveExerciseCompletionEvaluator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IExerciseAttemptRecorder, InMemoryExerciseAttemptRecorder>();
        services.AddSingleton<IExerciseCompletionEvaluator, ExerciseCompletionEvaluator>();
        var provider = services.BuildServiceProvider();

        var evaluator = provider.GetRequiredService<IExerciseCompletionEvaluator>();

        Assert.NotNull(evaluator);
        Assert.IsType<ExerciseCompletionEvaluator>(evaluator);
    }

    #endregion
}
