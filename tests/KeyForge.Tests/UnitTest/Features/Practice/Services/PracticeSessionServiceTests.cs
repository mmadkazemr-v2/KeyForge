namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class PracticeSessionServiceTests
{
    [Fact]
    public void StartSession_ValidUnlockedLesson_PreservesExerciseOrder()
    {
        var lesson = Lesson(Note("ex-01", "C4-G4"), Note("ex-02", "D4-A4"));
        var service = CreateService([lesson]);

        var session = service.StartSession(lesson.Id);

        Assert.NotNull(session);
        Assert.Equal(lesson.Id, session.LessonId);
        Assert.Equal(["ex-01", "ex-02"], session.Exercises.Select(x => x.Id));
        Assert.Equal("ex-01", session.GetCurrentExercise()!.Id);
    }

    [Fact]
    public void StartSession_UnknownLesson_ReturnsNull()
    {
        Assert.Null(CreateService([]).StartSession("missing"));
    }

    [Fact]
    public void StartSession_LockedLesson_ThrowsInvalidOperationException()
    {
        var lesson = Lesson(Note());
        var service = CreateService([lesson], isUnlocked: false);

        Assert.Throws<InvalidOperationException>(() => service.StartSession(lesson.Id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StartSession_InvalidId_ThrowsArgumentException(string? lessonId)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateService([]).StartSession(lessonId!));
    }

    [Fact]
    public void StartSession_DoesNotMutateDefinition()
    {
        var exercise = Note();
        var lesson = Lesson(exercise);

        CreateService([lesson]).StartSession(lesson.Id);

        Assert.Equal("C4-G4", exercise.Range);
        Assert.Same(exercise, Assert.Single(lesson.Exercises));
    }

    [Fact]
    public void StartExercise_GeneratesChallengeForCurrentExercise()
    {
        var service = CreateService([Lesson(Note())]);
        var session = service.StartSession("lesson-01")!;

        var runtime = service.StartExercise(session);

        Assert.Same(session.GetCurrentExercise(), runtime.Definition);
        Assert.Equal(NoteReadingChallengeGenerator.DefaultQuestionCount, runtime.Challenge.ExpectedInputs.Count);
        Assert.Equal("C4", runtime.Challenge.ExpectedInputs[0].Value);
        Assert.Empty(runtime.InputEvents);
    }

    [Fact]
    public void StartExercise_UsesTempoDefaultAndRecordsRuntimeSelectionOnAttempt()
    {
        var exercise = new RhythmExerciseDefinition
        {
            Id = "rhythm-01",
            TimeSignature = "4/4",
            PatternCount = 1,
            Tempo = new TempoConfiguration
            {
                DefaultBpm = 100,
                AllowUserChange = true,
                MinBpm = 60,
                MaxBpm = 140,
                Step = 5
            }
        };
        var service = CreateService([Lesson(exercise)], evaluator: new FixedEvaluator(success: true, rawScore: 80));
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);

        Assert.Equal(100, runtime.SelectedBpm);
        runtime.SetSelectedBpm(80);
        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));

        Assert.Equal(80, result.Attempt.TempoBpm);
        Assert.Equal(100, exercise.Tempo.DefaultBpm);
    }

    [Fact]
    public void StartExercise_FinishedSession_ThrowsInvalidOperationException()
    {
        var service = CreateService([Lesson(Note())]);
        var session = service.StartSession("lesson-01")!;
        session.Next();

        Assert.Throws<InvalidOperationException>(() => service.StartExercise(session));
    }

    [Fact]
    public void StartExercise_NullSession_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CreateService([]).StartExercise(null!));
    }

    [Fact]
    public void SubmitAttempt_CorrectInput_CreatesSystemGeneratedAttempt()
    {
        var recorder = new FakeAttemptRecorder();
        var service = CreateService([Lesson(Note())], recorder: recorder);
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);
        CaptureAnswerSet(runtime, "C4");
        var completedAt = runtime.StartedAt.AddSeconds(1);

        var result = service.SubmitAttempt(session, runtime, completedAt);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(100, result.Attempt.Score);
        Assert.True(result.Attempt.IsSuccessful);
        Assert.Equal(session.LessonId, result.Attempt.LessonId);
        Assert.Equal("ex-01", result.Attempt.ExerciseId);
        Assert.Equal(runtime.StartedAt, result.Attempt.StartedAt);
        Assert.Equal(completedAt, result.Attempt.CompletedAt);
        Assert.Same(result.Attempt, Assert.Single(recorder.Recorded));
    }

    [Fact]
    public void SubmitAttempt_IncorrectInput_GeneratesFailedZeroScore()
    {
        var service = CreateService([Lesson(Note())]);
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);
        CaptureAnswerSet(runtime, "D4");

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.False(result.Attempt.IsSuccessful);
        Assert.Equal(0, result.Attempt.Score);
    }

    [Fact]
    public void SubmitAttempt_NoInput_RecordsFailedAttempt()
    {
        var recorder = new FakeAttemptRecorder();
        var service = CreateService([Lesson(Note())], recorder: recorder);
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));

        Assert.Equal(ExerciseEvaluationFeedback.NoInput, result.Evaluation.Feedback);
        Assert.False(Assert.Single(recorder.Recorded).IsSuccessful);
    }

    [Fact]
    public void SubmitAttempt_UsesScorerForFinalAttemptScore()
    {
        var evaluator = new FixedEvaluator(success: true, rawScore: 175);
        var service = CreateService([Lesson(Note())], evaluator: evaluator);
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));

        Assert.Equal(100, result.Score);
        Assert.Equal(100, result.Attempt.Score);
    }

    [Fact]
    public void SubmitAttempt_DoesNotAutomaticallyAdvance()
    {
        var service = CreateService([Lesson(Note(), Note("ex-02", "D4-A4"))]);
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);
        CaptureAnswerSet(runtime, "C4");

        service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));

        Assert.Equal(0, session.CurrentExerciseIndex);
        Assert.Equal("ex-01", session.GetCurrentExercise()!.Id);
    }

    [Fact]
    public void SubmitAttempt_RetryStaysCurrentAndRecordsEachAttempt()
    {
        var recorder = new FakeAttemptRecorder();
        var service = CreateService([Lesson(Note())], recorder: recorder);
        var session = service.StartSession("lesson-01")!;

        var failedRuntime = service.StartExercise(session);
        CaptureAnswerSet(failedRuntime, "D4");
        service.SubmitAttempt(session, failedRuntime, failedRuntime.StartedAt.AddSeconds(1));

        var retryRuntime = service.StartExercise(session);
        CaptureAnswerSet(retryRuntime, "C4");
        var result = service.SubmitAttempt(session, retryRuntime, retryRuntime.StartedAt.AddSeconds(1));

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, recorder.Recorded.Count);
        Assert.Equal(0, session.CurrentExerciseIndex);
    }

    [Fact]
    public void SubmitAttempt_FailedRetryAfterSuccess_RemainsCompleted()
    {
        var service = CreateService([Lesson(Note())]);
        var session = service.StartSession("lesson-01")!;
        var success = service.StartExercise(session);
        CaptureAnswerSet(success, "C4");
        service.SubmitAttempt(session, success, success.StartedAt.AddSeconds(1));

        var failure = service.StartExercise(session);
        CaptureAnswerSet(failure, "D4");
        var result = service.SubmitAttempt(session, failure, failure.StartedAt.AddSeconds(1));

        Assert.False(result.IsSuccessful);
        Assert.True(result.IsExerciseCompleted);
    }

    [Fact]
    public void SubmitAttempt_RuntimeForDifferentExercise_ThrowsWithoutRecording()
    {
        var recorder = new FakeAttemptRecorder();
        var service = CreateService([Lesson(Note(), Note("ex-02", "D4-A4"))], recorder: recorder);
        var session = service.StartSession("lesson-01")!;
        var other = Note("ex-02", "D4-A4");
        var runtime = new RuntimeExercise(
            other,
            new ExerciseChallengeGenerator().Create(other),
            DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1)));
        Assert.Empty(recorder.Recorded);
    }

    [Fact]
    public void SubmitAttempt_CompletionBeforeStart_ThrowsWithoutRecording()
    {
        var recorder = new FakeAttemptRecorder();
        var service = CreateService([Lesson(Note())], recorder: recorder);
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.SubmitAttempt(session, runtime, runtime.StartedAt.AddTicks(-1)));
        Assert.Empty(recorder.Recorded);
    }

    [Fact]
    public void SubmitAttempt_FinishedSession_ThrowsWithoutRecording()
    {
        var recorder = new FakeAttemptRecorder();
        var service = CreateService([Lesson(Note())], recorder: recorder);
        var session = service.StartSession("lesson-01")!;
        var runtime = service.StartExercise(session);
        session.Next();

        Assert.Throws<InvalidOperationException>(() =>
            service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1)));
        Assert.Empty(recorder.Recorded);
    }

    [Fact]
    public void SubmitAttempt_NullSession_ThrowsArgumentNullException()
    {
        var exercise = Note();
        var runtime = new RuntimeExercise(
            exercise,
            new ExerciseChallengeGenerator().Create(exercise),
            DateTime.UtcNow);

        Assert.Throws<ArgumentNullException>(() =>
            CreateService([]).SubmitAttempt(null!, runtime, runtime.StartedAt));
    }

    [Fact]
    public void SubmitAttempt_NullRuntime_ThrowsArgumentNullException()
    {
        var session = new PracticeSession("lesson-01", [Note()]);

        Assert.Throws<ArgumentNullException>(() =>
            CreateService([]).SubmitAttempt(session, null!, DateTime.UtcNow));
    }

    [Fact]
    public void FullLifecycle_SubmitNextAndFinish()
    {
        var recorder = new FakeAttemptRecorder();
        var service = CreateService(
            [Lesson(Note(), Note("ex-02", "D4-A4"))],
            recorder: recorder);
        var session = service.StartSession("lesson-01")!;

        SubmitCorrect(service, session, "C4");
        session.Next();
        SubmitCorrect(service, session, "D4");
        session.Next();

        Assert.True(session.IsFinished);
        Assert.Null(session.GetCurrentExercise());
        Assert.Equal(2, recorder.Recorded.Count);
        Assert.Throws<InvalidOperationException>(() => service.StartExercise(session));
    }

    [Fact]
    public void PracticeSessionService_ImplementsContract()
    {
        IPracticeSessionService service = CreateService([]);
        Assert.IsType<PracticeSessionService>(service);
    }

    private static void SubmitCorrect(
        PracticeSessionService service,
        PracticeSession session,
        string note)
    {
        var runtime = service.StartExercise(session);
        CaptureAnswerSet(runtime, note);
        service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
    }

    private static PracticeSessionService CreateService(
        IReadOnlyList<LessonDefinition> lessons,
        bool isUnlocked = true,
        IExerciseEvaluator? evaluator = null,
        IExerciseAttemptRecorder? recorder = null) => new(
            new FakeLessonCatalog(lessons),
            new FakeProgressionService(isUnlocked),
            new ExerciseChallengeGenerator(),
            evaluator ?? new ExerciseEvaluator(),
            new ExerciseScorer(),
            recorder ?? new FakeAttemptRecorder());

    private static LessonDefinition Lesson(params ExerciseDefinition[] exercises) => new()
    {
        Id = "lesson-01",
        Title = "Runtime lesson",
        Exercises = exercises.ToList()
    };

    private static NoteReadingExerciseDefinition Note(
        string id = "ex-01",
        string range = "C4-G4") => new()
    {
        Id = id,
        Title = "Read the note",
        Range = range
    };

    private static ExerciseInputEvent Input(RuntimeExercise runtime, string value) => new()
    {
        Timestamp = runtime.StartedAt.AddMilliseconds(250),
        Source = InputSource.ComputerKeyboard,
        Type = InputEventType.Note,
        Value = value
    };

    private static void CaptureAnswerSet(RuntimeExercise runtime, string answer)
    {
        var isCorrectAttempt = string.Equals(
            answer,
            runtime.Challenge.ExpectedInputs[0].Value,
            StringComparison.OrdinalIgnoreCase);

        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            runtime.CaptureInput(Input(runtime, isCorrectAttempt ? expected.Value : "C8"));
        }
    }

    private sealed class FakeLessonCatalog(IReadOnlyList<LessonDefinition> lessons) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => lessons;

        public LessonDefinition? GetById(string id) => lessons.FirstOrDefault(
            lesson => string.Equals(lesson.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FakeProgressionService(bool isUnlocked) : ILessonProgressionService
    {
        public bool IsUnlocked(string lessonId) => isUnlocked;
        public bool IsCompleted(string lessonId) => false;
    }

    private sealed class FixedEvaluator(bool success, int rawScore) : IExerciseEvaluator
    {
        public ExerciseEvaluationResult Evaluate(
            ExerciseChallenge challenge,
            IReadOnlyList<ExerciseInputEvent> inputEvents) => new()
        {
            IsSuccessful = success,
            Score = rawScore,
            Feedback = success
                ? ExerciseEvaluationFeedback.Correct
                : ExerciseEvaluationFeedback.Incorrect
        };
    }

    private sealed class FakeAttemptRecorder : IExerciseAttemptRecorder
    {
        public List<ExerciseAttempt> Recorded { get; } = [];

        public void Record(ExerciseAttempt attempt) => Recorded.Add(attempt);

        public IReadOnlyList<ExerciseAttempt> GetAttemptsByLesson(string lessonId) => Recorded
            .Where(attempt => string.Equals(attempt.LessonId, lessonId, StringComparison.Ordinal))
            .ToList();
    }
}
