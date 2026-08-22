namespace KeyForge.Tests.ProcessTest.Features.Practice;

/// <summary>Verifies the service composition consumed by the practice page.</summary>
public sealed class PracticeSessionUiFlowProcessTests
{
    [Theory]
    [InlineData(ExerciseType.Interval)]
    [InlineData(ExerciseType.Octave)]
    public void PracticePageContracts_RecordAutomaticIntervalAndOctaveAttempts(ExerciseType exerciseType)
    {
        ExerciseDefinition exercise = exerciseType == ExerciseType.Interval
            ? new IntervalExerciseDefinition
            {
                Id = "target-exercise", StartingNote = "C4", Intervals = ["M3"],
                Direction = ExerciseDirection.Ascending
            }
            : new OctaveExerciseDefinition
            {
                Id = "target-exercise", StartingNote = "C3", Octaves = 2,
                Direction = ExerciseDirection.Ascending
            };
        var lesson = new LessonDefinition
        {
            Id = "target-lesson", Title = "Target lesson", Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true }, Exercises = [exercise]
        };
        var catalog = new FakeLessonCatalog(lesson);
        var progressStore = new InMemoryProgressStore();
        var attempts = new InMemoryExerciseAttemptRecorder();
        var completion = new ExerciseCompletionEvaluator(attempts);
        var progression = new LessonProgressionService(catalog, progressStore, completion);
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(), new ExerciseScorer(), attempts);
        var progress = new ProgressUpdateService(progressStore, catalog, progression);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);

        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            runtime.CaptureInput(new ExerciseInputEvent
            {
                Timestamp = runtime.StartedAt.AddMilliseconds(200), Source = InputSource.VirtualPiano,
                Type = InputEventType.Note, Value = expected.Value
            });
        }

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
        progress.UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(0, session.CurrentExerciseIndex);
        Assert.True(Assert.Single(attempts.GetAllAttempts()).IsSuccessful);
        Assert.Equal(100, Assert.Single(progressStore.GetAllProgress()).BestScore);
    }

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
                Note("exercise-1", "C4-G4"),
                Note("exercise-2", "D4-A4")
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
            new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            attemptRecorder);
        var progressUpdate = new ProgressUpdateService(progressStore, catalog, progression);
        var lessonQuery = new LessonProgressQueryService(catalog, progressStore, progression);
        var session = sessionService.StartSession(lesson.Id)!;

        var failedResult = Submit(sessionService, session, "D4");
        progressUpdate.UpdateProgress(lesson.Id, failedResult);
        Assert.False(failedResult.IsSuccessful);
        Assert.Equal("exercise-1", session.GetCurrentExercise()!.Id);

        var retryResult = Submit(sessionService, session, "C4");
        progressUpdate.UpdateProgress(lesson.Id, retryResult);
        Assert.True(retryResult.IsSuccessful);
        Assert.Equal(0, session.CurrentExerciseIndex);

        session.Next();
        Assert.Equal("exercise-2", session.GetCurrentExercise()!.Id);

        var finalResult = Submit(sessionService, session, "D4");
        progressUpdate.UpdateProgress(lesson.Id, finalResult);
        Assert.True(finalResult.IsSuccessful);
        Assert.False(session.IsFinished);

        session.Next();
        Assert.True(session.IsFinished);
        var displayState = Assert.Single(lessonQuery.GetLessons());
        Assert.True(displayState.IsCompleted);
        Assert.Equal(100, displayState.BestScore);
    }

    private static SessionResult Submit(
        PracticeSessionService service,
        PracticeSession session,
        string answer)
    {
        var runtime = service.StartExercise(session);
        var isCorrectAttempt = string.Equals(
            answer,
            runtime.Challenge.ExpectedInputs[0].Value,
            StringComparison.OrdinalIgnoreCase);
        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            runtime.CaptureInput(new ExerciseInputEvent
            {
                Timestamp = runtime.StartedAt.AddMilliseconds(200),
                Source = InputSource.ComputerKeyboard,
                Type = InputEventType.Note,
                Value = isCorrectAttempt ? expected.Value : answer
            });
        }
        return service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
    }

    private static NoteReadingExerciseDefinition Note(string id, string range) => new()
    {
        Id = id,
        Title = "Read note",
        Range = range
    };

    private sealed class FakeLessonCatalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];

        public LessonDefinition? GetById(string id) =>
            string.Equals(id, lesson.Id, StringComparison.OrdinalIgnoreCase) ? lesson : null;
    }
}
