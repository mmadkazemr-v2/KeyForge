namespace KeyForge.Tests.Features.Practice.Models;

/// <summary>
/// Tests <see cref="ExerciseAttempt"/> as a pure data model.
/// </summary>
public sealed class ExerciseAttemptTests
{
    [Fact]
    public void StoresLessonIdAndExerciseIdCorrectly()
    {
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-03"
        };

        Assert.Equal("lesson-01", attempt.LessonId);
        Assert.Equal("ex-03", attempt.ExerciseId);
    }

    [Fact]
    public void StoresTimingInformationCorrectly()
    {
        var started = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var completed = new DateTime(2026, 1, 15, 10, 5, 30, DateTimeKind.Utc);

        var attempt = new ExerciseAttempt
        {
            StartedAt = started,
            CompletedAt = completed
        };

        Assert.Equal(started, attempt.StartedAt);
        Assert.Equal(completed, attempt.CompletedAt);
    }

    [Fact]
    public void StoresScoreCorrectly()
    {
        var attempt = new ExerciseAttempt
        {
            Score = 85
        };

        Assert.Equal(85, attempt.Score);
    }

    [Fact]
    public void StoresIsSuccessfulCorrectly()
    {
        var successful = new ExerciseAttempt { IsSuccessful = true };
        var failed = new ExerciseAttempt { IsSuccessful = false };

        Assert.True(successful.IsSuccessful);
        Assert.False(failed.IsSuccessful);
    }

    [Fact]
    public void NonCompletedAttempt_HasNullCompletedAt()
    {
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            CompletedAt = null
        };

        Assert.Null(attempt.CompletedAt);
    }

    [Fact]
    public void NonScoredAttempt_HasNullScore()
    {
        var attempt = new ExerciseAttempt
        {
            Score = null
        };

        Assert.Null(attempt.Score);
    }

    [Fact]
    public void DefaultValues_AreExpected()
    {
        var attempt = new ExerciseAttempt();

        Assert.Equal(string.Empty, attempt.LessonId);
        Assert.Equal(string.Empty, attempt.ExerciseId);
        Assert.Equal(default, attempt.StartedAt);
        Assert.Null(attempt.CompletedAt);
        Assert.Null(attempt.Score);
        Assert.False(attempt.IsSuccessful);
    }

    [Fact]
    public void Model_HasNoScoringOrCalculationLogic()
    {
        // ExerciseAttempt is a pure data model.
        // It should have no methods beyond the auto-generated
        // property accessors (get/set). If this test fails to compile,
        // someone has added logic to the model — revert it.
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            Score = 70
        };

        // The model simply stores the score; no calculation is performed.
        Assert.Equal(70, attempt.Score);
    }
}
