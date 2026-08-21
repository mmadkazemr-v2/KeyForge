namespace KeyForge.Tests.UnitTest.Infrastructure.Practice.InMemory;

/// <summary>
/// Tests the <see cref="InMemoryExerciseAttemptRecorder"/> behavior and
/// verifies the application's DI configuration resolves the intended
/// implementation.
/// </summary>
public sealed class InMemoryExerciseAttemptRecorderTests
{
    private static InMemoryExerciseAttemptRecorder CreateRecorder() => new();

    [Fact]
    public void Record_OneAttempt_Succeeds()
    {
        var recorder = CreateRecorder();
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow
        };

        recorder.Record(attempt);

        var all = recorder.GetAllAttempts();
        Assert.Single(all);
        Assert.Same(attempt, all[0]);
    }

    [Fact]
    public void Record_RecordedAttempt_IsRetrievable()
    {
        var recorder = CreateRecorder();
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            Score = 85,
            IsSuccessful = true
        };

        recorder.Record(attempt);

        var stored = recorder.GetAllAttempts()[0];
        Assert.Equal("lesson-01", stored.LessonId);
        Assert.Equal("ex-01", stored.ExerciseId);
        Assert.Equal(85, stored.Score);
        Assert.True(stored.IsSuccessful);
    }

    [Fact]
    public void Record_MultipleAttemptsForSameExerciseId_ArePreserved()
    {
        var recorder = CreateRecorder();
        var first = new ExerciseAttempt { LessonId = "lesson-01", ExerciseId = "ex-01", Score = 60 };
        var second = new ExerciseAttempt { LessonId = "lesson-01", ExerciseId = "ex-01", Score = 80 };
        var third = new ExerciseAttempt { LessonId = "lesson-01", ExerciseId = "ex-01", Score = 95 };

        recorder.Record(first);
        recorder.Record(second);
        recorder.Record(third);

        var all = recorder.GetAllAttempts();
        Assert.Equal(3, all.Count);
        Assert.Equal(60, all[0].Score);
        Assert.Equal(80, all[1].Score);
        Assert.Equal(95, all[2].Score);
    }

    [Fact]
    public void Record_AttemptsForDifferentExercises_RemainIndependent()
    {
        var recorder = CreateRecorder();
        var piano = new ExerciseAttempt { LessonId = "lesson-01", ExerciseId = "piano-ex-01" };
        var rhythm = new ExerciseAttempt { LessonId = "lesson-01", ExerciseId = "rhythm-ex-01" };

        recorder.Record(piano);
        recorder.Record(rhythm);

        var all = recorder.GetAllAttempts();
        Assert.Equal(2, all.Count);
        Assert.Equal("piano-ex-01", all[0].ExerciseId);
        Assert.Equal("rhythm-ex-01", all[1].ExerciseId);
    }

    [Fact]
    public void Record_PreservesInsertionOrder()
    {
        var recorder = CreateRecorder();
        var attempts = new[]
        {
            new ExerciseAttempt { LessonId = "l1", ExerciseId = "e3" },
            new ExerciseAttempt { LessonId = "l2", ExerciseId = "e1" },
            new ExerciseAttempt { LessonId = "l1", ExerciseId = "e2" },
            new ExerciseAttempt { LessonId = "l2", ExerciseId = "e4" }
        };

        foreach (var a in attempts)
            recorder.Record(a);

        var all = recorder.GetAllAttempts();
        Assert.Equal(4, all.Count);
        for (var i = 0; i < attempts.Length; i++)
            Assert.Same(attempts[i], all[i]);
    }

    [Fact]
    public void Record_DoesNotMutateSuppliedObject()
    {
        var recorder = CreateRecorder();
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            Score = 70,
            IsSuccessful = false,
            StartedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        recorder.Record(attempt);

        Assert.Equal("lesson-01", attempt.LessonId);
        Assert.Equal("ex-01", attempt.ExerciseId);
        Assert.Equal(70, attempt.Score);
        Assert.False(attempt.IsSuccessful);
        Assert.Equal(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), attempt.StartedAt);
        Assert.Null(attempt.CompletedAt);
    }

    [Fact]
    public void GetAllAttempts_EmptyStore_ReturnsEmptyList()
    {
        var recorder = CreateRecorder();

        var all = recorder.GetAllAttempts();

        Assert.NotNull(all);
        Assert.Empty(all);
    }

    [Fact]
    public void Di_ResolvesRecorderAsInMemoryImplementation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IExerciseAttemptRecorder, InMemoryExerciseAttemptRecorder>();
        var provider = services.BuildServiceProvider();

        var recorder = provider.GetRequiredService<IExerciseAttemptRecorder>();

        Assert.NotNull(recorder);
        Assert.IsType<InMemoryExerciseAttemptRecorder>(recorder);
    }
}
