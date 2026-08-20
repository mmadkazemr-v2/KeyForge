namespace KeyForge.Tests.Features.Practice.Services;

/// <summary>
/// Tests <see cref="ExerciseEvaluator"/> using in-memory fakes
/// to verify exercise evaluation behaviour without touching files or MIDI.
/// </summary>
public sealed class ExerciseEvaluatorTests
{
    private static ExerciseEvaluator CreateEvaluator() => new();

    [Fact]
    public void Evaluate_CompletedAttemptWithScore_ReturnsSuccessful()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01",
            Title = "Quarter Notes"
        };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Score = 85
        };

        var result = evaluator.Evaluate(exercise, attempt);

        Assert.True(result.IsSuccessful);
        Assert.Equal(85, result.Score);
    }

    [Fact]
    public void Evaluate_IncompleteAttempt_ReturnsUnsuccessful()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01",
            Title = "Quarter Notes"
        };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            CompletedAt = null,
            Score = 85
        };

        var result = evaluator.Evaluate(exercise, attempt);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_CompletedAttemptWithoutScore_ReturnsUnsuccessful()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01",
            Title = "Quarter Notes"
        };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Score = null
        };

        var result = evaluator.Evaluate(exercise, attempt);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.Score);
    }

    [Fact]
    public void Evaluate_MismatchedExerciseId_ThrowsArgumentException()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01",
            Title = "Quarter Notes"
        };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-99",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Score = 85
        };

        var ex = Assert.Throws<ArgumentException>(() => evaluator.Evaluate(exercise, attempt));
        Assert.Equal("attempt", ex.ParamName);
    }

    [Fact]
    public void Evaluate_NullExercise_ThrowsArgumentNullException()
    {
        var evaluator = CreateEvaluator();
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01"
        };

        Assert.Throws<ArgumentNullException>(() => evaluator.Evaluate(null!, attempt));
    }

    [Fact]
    public void Evaluate_NullAttempt_ThrowsArgumentNullException()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01"
        };

        Assert.Throws<ArgumentNullException>(() => evaluator.Evaluate(exercise, null!));
    }

    [Fact]
    public void Evaluate_DoesNotMutateExercise()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01",
            Title = "Original Title",
            Duration = 5,
            Tempo = 80
        };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Score = 90
        };

        evaluator.Evaluate(exercise, attempt);

        Assert.Equal("ex-01", exercise.Id);
        Assert.Equal("Original Title", exercise.Title);
        Assert.Equal(5, exercise.Duration);
        Assert.Equal(80, exercise.Tempo);
    }

    [Fact]
    public void Evaluate_DoesNotMutateAttempt()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01"
        };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 1, 15, 10, 5, 30, DateTimeKind.Utc),
            Score = 75
        };

        evaluator.Evaluate(exercise, attempt);

        Assert.Equal("lesson-01", attempt.LessonId);
        Assert.Equal("ex-01", attempt.ExerciseId);
        Assert.Equal(new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc), attempt.StartedAt);
        Assert.Equal(new DateTime(2026, 1, 15, 10, 5, 30, DateTimeKind.Utc), attempt.CompletedAt);
        Assert.Equal(75, attempt.Score);
    }

    [Fact]
    public void Evaluate_DifferentExerciseTypes_FollowSameRule()
    {
        var evaluator = CreateEvaluator();

        var noteReading = new NoteReadingExerciseDefinition
        {
            Id = "ex-nr-01",
            Title = "Note Reading"
        };
        var speed = new SpeedExerciseDefinition
        {
            Id = "ex-sp-01",
            Title = "Speed Drill"
        };

        var completedWithScore = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-nr-01",
            CompletedAt = DateTime.UtcNow,
            Score = 90
        };
        var completedSpeed = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-sp-01",
            CompletedAt = DateTime.UtcNow,
            Score = 70
        };

        var resultNr = evaluator.Evaluate(noteReading, completedWithScore);
        var resultSp = evaluator.Evaluate(speed, completedSpeed);

        Assert.True(resultNr.IsSuccessful);
        Assert.Equal(90, resultNr.Score);
        Assert.True(resultSp.IsSuccessful);
        Assert.Equal(70, resultSp.Score);
    }

    [Fact]
    public void Evaluate_ScorePreservedInResult()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition
        {
            Id = "ex-01"
        };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = DateTime.UtcNow,
            Score = 42
        };

        var result = evaluator.Evaluate(exercise, attempt);

        Assert.Equal(42, result.Score);
    }

    [Fact]
    public void ExerciseEvaluator_ImplementsIExerciseEvaluator()
    {
        IExerciseEvaluator evaluator = new ExerciseEvaluator();

        Assert.NotNull(evaluator);
    }

    [Fact]
    public void Evaluate_AllExerciseTypes_FollowSameRule()
    {
        var evaluator = CreateEvaluator();

        var exercises = new ExerciseDefinition[]
        {
            new RhythmExerciseDefinition { Id = "ex-rhythm" },
            new NoteReadingExerciseDefinition { Id = "ex-note" },
            new EarTrainingExerciseDefinition { Id = "ex-ear" },
            new IntervalExerciseDefinition { Id = "ex-interval" },
            new OctaveExerciseDefinition { Id = "ex-octave" },
            new FingerIndependenceExerciseDefinition { Id = "ex-finger" },
            new MentalKeyboardExerciseDefinition { Id = "ex-mental" },
            new SpeedExerciseDefinition { Id = "ex-speed" }
        };

        foreach (var exercise in exercises)
        {
            var attempt = new ExerciseAttempt
            {
                LessonId = "lesson-01",
                ExerciseId = exercise.Id,
                CompletedAt = DateTime.UtcNow,
                Score = 80
            };

            var result = evaluator.Evaluate(exercise, attempt);

            Assert.True(result.IsSuccessful);
            Assert.Equal(80, result.Score);
        }
    }

    [Fact]
    public void Evaluate_ScoreZero_ReturnsSuccessfulWithZero()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition { Id = "ex-01" };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = DateTime.UtcNow,
            Score = 0
        };

        var result = evaluator.Evaluate(exercise, attempt);

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Evaluate_ScoreHundred_ReturnsSuccessfulWithHundred()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition { Id = "ex-01" };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = DateTime.UtcNow,
            Score = 100
        };

        var result = evaluator.Evaluate(exercise, attempt);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Evaluate_CompletedAtPastTimestamp_ReturnsSuccessful()
    {
        var evaluator = CreateEvaluator();
        var exercise = new RhythmExerciseDefinition { Id = "ex-01" };
        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 1, 1, 10, 5, 0, DateTimeKind.Utc),
            Score = 75
        };

        var result = evaluator.Evaluate(exercise, attempt);

        Assert.True(result.IsSuccessful);
        Assert.Equal(75, result.Score);
    }
}
