namespace KeyForge.Tests.Features.Practice.Services;

/// <summary>
/// Tests <see cref="IExerciseAttemptRecorder"/> using the
/// <see cref="InMemoryExerciseAttemptRecorder"/> implementation.
/// </summary>
public sealed class ExerciseAttemptRecorderTests
{
    private static InMemoryExerciseAttemptRecorder CreateRecorder() => new();

    [Fact]
    public void Record_ValidAttempt_IsStored()
    {
        var recorder = CreateRecorder();
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            Score = 80,
            IsSuccessful = true
        };

        recorder.Record(attempt);

        var all = recorder.GetAllAttempts();
        Assert.Single(all);
        Assert.Same(attempt, all[0]);
    }

    [Fact]
    public void Record_NullAttempt_ThrowsArgumentNullException()
    {
        var recorder = CreateRecorder();

        Assert.Throws<ArgumentNullException>(() => recorder.Record(null!));
    }

    [Fact]
    public void Record_MultipleAttempts_AllStoredInOrder()
    {
        var recorder = CreateRecorder();
        var first = new ExerciseAttempt { LessonId = "l1", ExerciseId = "e1" };
        var second = new ExerciseAttempt { LessonId = "l1", ExerciseId = "e2" };
        var third = new ExerciseAttempt { LessonId = "l2", ExerciseId = "e1" };

        recorder.Record(first);
        recorder.Record(second);
        recorder.Record(third);

        var all = recorder.GetAllAttempts();
        Assert.Equal(3, all.Count);
        Assert.Same(first, all[0]);
        Assert.Same(second, all[1]);
        Assert.Same(third, all[2]);
    }

    [Fact]
    public void Record_AttemptWithNullCompletedAt_IsStored()
    {
        var recorder = CreateRecorder();
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = null
        };

        recorder.Record(attempt);

        var stored = recorder.GetAllAttempts();
        Assert.Single(stored);
        Assert.Null(stored[0].CompletedAt);
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
    public void Record_DoesNotMutateAttempt()
    {
        var recorder = CreateRecorder();
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            Score = 75,
            IsSuccessful = false
        };

        recorder.Record(attempt);

        // Verify the original object was not mutated
        Assert.Equal("lesson-01", attempt.LessonId);
        Assert.Equal("ex-01", attempt.ExerciseId);
        Assert.Equal(75, attempt.Score);
        Assert.False(attempt.IsSuccessful);
    }

    [Fact]
    public async Task Record_ThreadSafe_ConcurrentCallsDoNotThrow()
    {
        var recorder = CreateRecorder();
        var tasks = new Task[10];

        for (var i = 0; i < tasks.Length; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                recorder.Record(new ExerciseAttempt
                {
                    LessonId = $"lesson-{index}",
                    ExerciseId = $"ex-{index}"
                });
            });
        }

        await Task.WhenAll(tasks);

        var all = recorder.GetAllAttempts();
        Assert.Equal(10, all.Count);
    }
}
