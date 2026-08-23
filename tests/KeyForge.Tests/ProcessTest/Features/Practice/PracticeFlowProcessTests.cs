namespace KeyForge.Tests.ProcessTest.Features.Practice;

using KeyForge.Components.Practice;

/// <summary>Exercises the runtime practice pipeline with real services and in-memory stores.</summary>
public sealed class PracticeFlowProcessTests
{
    [Fact]
    public void NoteReadingChallengeToPianoInputAndProgressUpdate_EndToEnd()
    {
        var lesson = Lesson(Note("note-01", "C4-G4"));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;
        var runtime = flow.Session.StartExercise(session);

        for (var index = 0; index < runtime.Challenge.ExpectedInputs.Count; index++)
        {
            var expected = runtime.Challenge.ExpectedInputs[index];
            var occurredAt = runtime.StartedAt.AddMilliseconds(100 + (index * 100));
            runtime.CaptureInput(VirtualPianoInputAdapter.Create(
                MusicalNote.Parse(expected.Value),
                runtime.StartedAt,
                occurredAt));
        }

        var result = flow.Session.SubmitAttempt(
            session,
            runtime,
            runtime.StartedAt.AddSeconds(1));
        flow.Progress.UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(NoteReadingChallengeGenerator.DefaultQuestionCount,
            result.Evaluation.NoteReading!.CorrectAnswerCount);
        var attempt = Assert.Single(flow.Recorder.GetAttemptsByLesson(lesson.Id));
        Assert.Equal(result.Score, attempt.Score);
        Assert.Equal(result.IsSuccessful, attempt.IsSuccessful);
        Assert.Equal(100, flow.Store.GetProgress(lesson.Id)!.BestScore);
    }

    [Fact]
    public void RuntimeChallengeToAttemptRecordingAndProgressUpdate_EndToEnd()
    {
        var lesson = Lesson(Rhythm("ex-01", tempo: 120, patternCount: 1));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;
        var runtime = flow.Session.StartExercise(session);

        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            var offset = expected.ExpectedOffset!.Value;
            runtime.CaptureInput(new ExerciseInputEvent
            {
                Timestamp = runtime.StartedAt + offset,
                Offset = offset,
                Source = InputSource.ComputerKeyboard,
                Type = InputEventType.Beat,
                Value = "beat"
            });
        }

        var result = flow.Session.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(2));
        flow.Progress.UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(4, result.Evaluation.Rhythm!.SuccessfulHitCount);
        var attempt = Assert.Single(flow.Recorder.GetAttemptsByLesson(lesson.Id));
        Assert.Equal(100, attempt.Score);
        Assert.True(attempt.IsSuccessful);
        var progress = flow.Store.GetProgress(lesson.Id);
        Assert.NotNull(progress);
        Assert.Equal(1, progress.AttemptCount);
        Assert.Equal(100, progress.BestScore);
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public void FailedInput_DoesNotCompleteExerciseOrLesson()
    {
        var lesson = Lesson(Note("ex-01", "C4-G4"));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;

        var result = Submit(flow, session, "D4", updateProgress: true);

        Assert.False(result.IsSuccessful);
        Assert.False(result.IsExerciseCompleted);
        Assert.False(flow.Store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.False(Assert.Single(flow.Recorder.GetAttemptsByLesson(lesson.Id)).IsSuccessful);
    }

    [Fact]
    public void FailedThenSuccessfulRetry_CompletesCurrentExerciseWithoutAdvancing()
    {
        var lesson = Lesson(Note("ex-01", "C4-G4"));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;

        var failed = Submit(flow, session, "D4", updateProgress: true);
        var successful = Submit(flow, session, "C4", updateProgress: true);

        Assert.False(failed.IsSuccessful);
        Assert.True(successful.IsSuccessful);
        Assert.True(successful.IsExerciseCompleted);
        Assert.Equal(0, session.CurrentExerciseIndex);
        Assert.True(flow.Store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Equal(2, flow.Recorder.GetAttemptsByLesson(lesson.Id).Count);
    }

    [Fact]
    public void PartialCompletion_DoesNotCompleteMultiExerciseLesson()
    {
        var lesson = Lesson(
            Note("ex-01", "C4-G4"),
            Note("ex-02", "D4-A4"));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;

        Submit(flow, session, "C4", updateProgress: true);
        session.Next();

        Assert.False(session.IsFinished);
        Assert.False(flow.Store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Single(flow.Recorder.GetAttemptsByLesson(lesson.Id));
    }

    [Fact]
    public void RuntimeForNonCurrentExercise_DoesNotEnterPersistenceFlow()
    {
        var lesson = Lesson(
            Note("ex-01", "C4-G4"),
            Note("ex-02", "D4-A4"));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;
        var other = lesson.Exercises[1];
        var runtime = new RuntimeExercise(
            other,
            new ExerciseChallengeGenerator().Create(other),
            DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            flow.Session.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1)));
        Assert.Empty(flow.Recorder.GetAttemptsByLesson(lesson.Id));
        Assert.Null(flow.Store.GetProgress(lesson.Id));
    }

    [Fact]
    public void RetryThenExplicitNext_CompletesSessionWithCorrectStateTransitions()
    {
        var lesson = Lesson(
            Note("ex-01", "C4-G4"),
            Note("ex-02", "D4-A4"));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;

        Submit(flow, session, "D4", updateProgress: true);
        Assert.Equal("ex-01", session.GetCurrentExercise()!.Id);
        Submit(flow, session, "C4", updateProgress: true);
        Assert.Equal("ex-01", session.GetCurrentExercise()!.Id);

        session.Next();
        Assert.Equal("ex-02", session.GetCurrentExercise()!.Id);
        Submit(flow, session, "D4", updateProgress: true);
        Assert.False(session.IsFinished);

        session.Next();
        Assert.True(session.IsFinished);
        Assert.True(flow.Store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Equal(3, flow.Recorder.GetAttemptsByLesson(lesson.Id).Count);
    }

    [Fact]
    public void SuccessfulAttemptFollowedByFailedRetry_DoesNotUndoCompletion()
    {
        var lesson = Lesson(Note("ex-01", "C4-G4"));
        var flow = CreateFlow(lesson);
        var session = flow.Session.StartSession(lesson.Id)!;

        Submit(flow, session, "C4", updateProgress: true);
        var retry = Submit(flow, session, "D4", updateProgress: true);

        Assert.False(retry.IsSuccessful);
        Assert.True(retry.IsExerciseCompleted);
        Assert.True(flow.Store.GetProgress(lesson.Id)!.IsCompleted);
    }

    private static SessionResult Submit(
        Flow flow,
        PracticeSession session,
        string answer,
        bool updateProgress)
    {
        var runtime = flow.Session.StartExercise(session);
        var isCorrectAttempt = string.Equals(
            answer,
            runtime.Challenge.ExpectedInputs[0].Value,
            StringComparison.OrdinalIgnoreCase);
        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            runtime.CaptureInput(Input(
                runtime,
                isCorrectAttempt ? expected.Value : answer,
                InputSource.ComputerKeyboard));
        }
        var result = flow.Session.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
        if (updateProgress)
        {
            flow.Progress.UpdateProgress(session.LessonId, result);
        }

        return result;
    }

    private static Flow CreateFlow(params LessonDefinition[] lessons)
    {
        var catalog = new FakeLessonCatalog(lessons);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var completionEvaluator = new ExerciseCompletionEvaluator(recorder);
        var progression = new LessonProgressionService(catalog, store, completionEvaluator);
        var session = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            recorder);

        return new Flow(
            session,
            new ProgressUpdateService(store, catalog, progression),
            store,
            recorder);
    }

    private static LessonDefinition Lesson(params ExerciseDefinition[] exercises) => new()
    {
        Id = "lesson-01",
        Title = "Runtime lesson",
        Order = 1,
        Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
        Completion = new CompletionRule { RequireAllExercises = true },
        Exercises = exercises.ToList()
    };

    private static NoteReadingExerciseDefinition Note(string id, string range) => new()
    {
        Id = id,
        Title = "Read note",
        Range = range
    };

    private static RhythmExerciseDefinition Rhythm(string id, int tempo, int patternCount) => new()
    {
        Id = id,
        Title = "Keep the beat",
        Tempo = new TempoConfiguration { DefaultBpm = tempo },
        TimeSignature = "4/4",
        PatternCount = patternCount,
        NoteValues = ["quarter"]
    };

    private static ExerciseInputEvent Input(
        RuntimeExercise runtime,
        string answer,
        InputSource source) => new()
    {
        Timestamp = runtime.StartedAt.AddMilliseconds(350),
        Source = source,
        Type = InputEventType.Note,
        Value = answer
    };

    private sealed record Flow(
        PracticeSessionService Session,
        ProgressUpdateService Progress,
        InMemoryProgressStore Store,
        InMemoryExerciseAttemptRecorder Recorder);

    private sealed class FakeLessonCatalog(IEnumerable<LessonDefinition> lessons) : ILessonCatalog
    {
        private readonly IReadOnlyList<LessonDefinition> _lessons = lessons.ToList();

        public IReadOnlyList<LessonDefinition> GetAll() => _lessons;

        public LessonDefinition? GetById(string id) => _lessons.FirstOrDefault(
            lesson => string.Equals(lesson.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
