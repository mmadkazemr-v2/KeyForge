namespace KeyForge.Tests.Features.Practice.Services;

/// <summary>
/// Tests <see cref="PracticeSessionService"/> and <see cref="PracticeSession"/>
/// using in-memory fakes to verify practice session behaviour.
/// </summary>
public sealed class PracticeSessionServiceTests
{
    private static readonly RhythmExerciseDefinition Exercise1 = new()
    {
        Id = "ex-01",
        Title = "Quarter Notes",
        Type = ExerciseType.Rhythm
    };

    private static readonly NoteReadingExerciseDefinition Exercise2 = new()
    {
        Id = "ex-02",
        Title = "Treble Clef",
        Type = ExerciseType.NoteReading
    };

    private static readonly SpeedExerciseDefinition Exercise3 = new()
    {
        Id = "ex-03",
        Title = "Speed Drill",
        Type = ExerciseType.Speed
    };

    private static ILessonCatalog CreateCatalog(params LessonDefinition[] lessons)
    {
        return new FakeLessonCatalog(lessons);
    }

    private static PracticeSessionService CreateService(
        ILessonCatalog? catalog = null,
        ILessonProgressionService? progression = null,
        IExerciseEvaluator? evaluator = null,
        IExerciseScorer? scorer = null,
        IExerciseAttemptRecorder? recorder = null)
    {
        return new PracticeSessionService(
            catalog ?? CreateCatalog(),
            progression ?? new FakeProgressionService(),
            evaluator ?? new ExerciseEvaluator(),
            scorer ?? new ExerciseScorer(),
            recorder ?? new FakeAttemptRecorder());
    }

    #region StartSession tests

    [Fact]
    public void StartSession_ValidUnlockedLesson_ReturnsSession()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Test Lesson",
            Exercises = [Exercise1, Exercise2]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);

        var session = service.StartSession("lesson-01");

        Assert.NotNull(session);
        Assert.Equal("lesson-01", session.LessonId);
        Assert.Equal(2, session.Exercises.Count);
    }

    [Fact]
    public void StartSession_UnknownLesson_ReturnsNull()
    {
        var catalog = CreateCatalog();
        var service = CreateService(catalog: catalog);

        var session = service.StartSession("unknown");

        Assert.Null(session);
    }

    [Fact]
    public void StartSession_LockedLesson_ThrowsInvalidOperationException()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var progression = new FakeProgressionService(isUnlocked: false);
        var service = CreateService(catalog: catalog, progression: progression);

        var ex = Assert.Throws<InvalidOperationException>(() => service.StartSession("lesson-01"));
        Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StartSession_ExerciseOrderPreserved()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2, Exercise3]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);

        var session = service.StartSession("lesson-01")!;

        Assert.Equal("ex-01", session.Exercises[0].Id);
        Assert.Equal("ex-02", session.Exercises[1].Id);
        Assert.Equal("ex-03", session.Exercises[2].Id);
    }

    [Fact]
    public void StartSession_FirstExerciseIsCurrent()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);

        var session = service.StartSession("lesson-01")!;

        Assert.Equal(0, session.CurrentExerciseIndex);
        Assert.Equal("ex-01", session.GetCurrentExercise()!.Id);
    }

    [Fact]
    public void StartSession_NullLessonId_ThrowsArgumentNullException()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.StartSession(null!));
    }

    [Fact]
    public void StartSession_DoesNotMutateLesson()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Original Title",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);

        service.StartSession("lesson-01");

        Assert.Equal("lesson-01", lesson.Id);
        Assert.Equal("Original Title", lesson.Title);
        Assert.Single(lesson.Exercises);
    }

    [Fact]
    public void PracticeSession_ImplementsIPracticeSessionService()
    {
        IPracticeSessionService service = CreateService();

        Assert.NotNull(service);
    }

    #endregion

    #region Navigation tests

    [Fact]
    public void Next_MovesToNextExercise()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2, Exercise3]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        session.Next();

        Assert.Equal(1, session.CurrentExerciseIndex);
        Assert.Equal("ex-02", session.GetCurrentExercise()!.Id);
    }

    [Fact]
    public void Next_AtLastExercise_SessionFinished()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        session.Next();
        session.Next();

        Assert.True(session.IsFinished);
        Assert.Null(session.GetCurrentExercise());
    }

    [Fact]
    public void Next_PastEnd_DoesNotThrow()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        session.Next();
        session.Next();
        session.Next();

        Assert.True(session.IsFinished);
    }

    [Fact]
    public void IsFinished_EmptyLesson_ReturnsTrue()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = []
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);

        var session = service.StartSession("lesson-01")!;

        Assert.True(session.IsFinished);
        Assert.Null(session.GetCurrentExercise());
    }

    #endregion

    #region SubmitAttempt tests

    [Fact]
    public void SubmitAttempt_CompletedAttempt_ReturnsSuccessfulResult()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Score = 85
        };

        var result = service.SubmitAttempt(session, "ex-01", attempt);

        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.Equal(85, result.Score);
        Assert.True(result.Evaluation.IsSuccessful);
        Assert.Equal(85, result.Evaluation.Score);
    }

    [Fact]
    public void SubmitAttempt_IncompleteAttempt_ReturnsUnsuccessfulWithZeroScore()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            StartedAt = DateTime.UtcNow,
            CompletedAt = null,
            Score = null
        };

        var result = service.SubmitAttempt(session, "ex-01", attempt);

        Assert.NotNull(result);
        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void SubmitAttempt_EvaluatorIsCalled()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var evaluator = new FakeEvaluator();
        var service = CreateService(catalog: catalog, evaluator: evaluator);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01"
        };

        service.SubmitAttempt(session, "ex-01", attempt);

        Assert.True(evaluator.EvaluateCalled);
        Assert.Same(Exercise1, evaluator.LastExercise);
        Assert.Same(attempt, evaluator.LastAttempt);
    }

    [Fact]
    public void SubmitAttempt_ScorerIsCalled()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var scorer = new FakeScorer();
        var service = CreateService(catalog: catalog, scorer: scorer);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = DateTime.UtcNow,
            Score = 90
        };

        service.SubmitAttempt(session, "ex-01", attempt);

        Assert.True(scorer.ScoreCalled);
        Assert.NotNull(scorer.LastResult);
    }

    [Fact]
    public void SubmitAttempt_RecorderIsCalled()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var recorder = new FakeAttemptRecorder();
        var service = CreateService(catalog: catalog, recorder: recorder);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = DateTime.UtcNow,
            Score = 70
        };

        service.SubmitAttempt(session, "ex-01", attempt);

        Assert.Single(recorder.Recorded);
        Assert.Same(attempt, recorder.Recorded[0]);
    }

    [Fact]
    public void SubmitAttempt_WrongExerciseId_ThrowsArgumentException()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-02"
        };

        var ex = Assert.Throws<ArgumentException>(() =>
            service.SubmitAttempt(session, "ex-02", attempt));
        Assert.Contains("ex-01", ex.Message);
    }

    [Fact]
    public void SubmitAttempt_FinishedSession_ThrowsInvalidOperationException()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;
        session.Next();

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01"
        };

        Assert.Throws<InvalidOperationException>(() =>
            service.SubmitAttempt(session, "ex-01", attempt));
    }

    [Fact]
    public void SubmitAttempt_NullSession_ThrowsArgumentNullException()
    {
        var service = CreateService();
        var attempt = new ExerciseAttempt();

        Assert.Throws<ArgumentNullException>(() =>
            service.SubmitAttempt(null!, "ex-01", attempt));
    }

    [Fact]
    public void SubmitAttempt_NullExerciseId_ThrowsArgumentNullException()
    {
        var service = CreateService();
        var session = new PracticeSession("lesson-01", [Exercise1]);
        var attempt = new ExerciseAttempt();

        Assert.Throws<ArgumentNullException>(() =>
            service.SubmitAttempt(session, null!, attempt));
    }

    [Fact]
    public void SubmitAttempt_NullAttempt_ThrowsArgumentNullException()
    {
        var service = CreateService();
        var session = new PracticeSession("lesson-01", [Exercise1]);

        Assert.Throws<ArgumentNullException>(() =>
            service.SubmitAttempt(session, "ex-01", null!));
    }

    [Fact]
    public void SubmitAttempt_DoesNotMutateLessonOrExercise()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Original",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = DateTime.UtcNow,
            Score = 80
        };

        service.SubmitAttempt(session, "ex-01", attempt);

        Assert.Equal("lesson-01", lesson.Id);
        Assert.Equal("Original", lesson.Title);
        Assert.Single(lesson.Exercises);
        Assert.Equal("ex-01", Exercise1.Id);
        Assert.Equal("Quarter Notes", Exercise1.Title);
    }

    [Fact]
    public void SubmitAttempt_ScoreIsClampedThroughScorer()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog: catalog);
        var session = service.StartSession("lesson-01")!;

        var attempt = new ExerciseAttempt
        {
            LessonId = "lesson-01",
            ExerciseId = "ex-01",
            CompletedAt = DateTime.UtcNow,
            Score = 150
        };

        var result = service.SubmitAttempt(session, "ex-01", attempt);

        Assert.Equal(100, result.Score);
    }

    #endregion

    #region Fakes

    private sealed class FakeLessonCatalog : ILessonCatalog
    {
        private readonly Dictionary<string, LessonDefinition> _lessons;

        public FakeLessonCatalog(LessonDefinition[] lessons)
        {
            _lessons = lessons.ToDictionary(l => l.Id, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<LessonDefinition> GetAll() => _lessons.Values.ToList().AsReadOnly();

        public LessonDefinition? GetById(string id) =>
            _lessons.TryGetValue(id, out var lesson) ? lesson : null;
    }

    private sealed class FakeProgressionService : ILessonProgressionService
    {
        private readonly bool _isUnlocked;

        public FakeProgressionService(bool isUnlocked = true)
        {
            _isUnlocked = isUnlocked;
        }

        public bool IsUnlocked(string lessonId) => _isUnlocked;
        public bool IsCompleted(string lessonId) => false;
    }

    private sealed class FakeEvaluator : IExerciseEvaluator
    {
        public bool EvaluateCalled { get; private set; }
        public ExerciseDefinition? LastExercise { get; private set; }
        public ExerciseAttempt? LastAttempt { get; private set; }

        public ExerciseEvaluationResult Evaluate(ExerciseDefinition exercise, ExerciseAttempt attempt)
        {
            EvaluateCalled = true;
            LastExercise = exercise;
            LastAttempt = attempt;

            return new ExerciseEvaluationResult
            {
                IsSuccessful = attempt.CompletedAt.HasValue && attempt.Score.HasValue,
                Score = attempt.Score
            };
        }
    }

    private sealed class FakeScorer : IExerciseScorer
    {
        public bool ScoreCalled { get; private set; }
        public ExerciseEvaluationResult? LastResult { get; private set; }

        public int Score(ExerciseEvaluationResult evaluationResult)
        {
            ScoreCalled = true;
            LastResult = evaluationResult;
            return evaluationResult.Score ?? 0;
        }
    }

    private sealed class FakeAttemptRecorder : IExerciseAttemptRecorder
    {
        public List<ExerciseAttempt> Recorded { get; } = [];

        public void Record(ExerciseAttempt attempt)
        {
            Recorded.Add(attempt);
        }
    }

    #endregion
}
