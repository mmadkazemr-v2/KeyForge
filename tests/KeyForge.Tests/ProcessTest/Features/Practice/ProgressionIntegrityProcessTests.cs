namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class ProgressionIntegrityProcessTests
{
    [Fact]
    public void PassingAttemptCompletesLessonUnlocksNextAndLowerRetryKeepsProgress()
    {
        var lessonOne = new LessonDefinition
        {
            Id = "lesson-01",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true, MinimumScore = 70 },
            Exercises = [new NoteReadingExerciseDefinition { Id = "exercise-01", Range = "C4-C4" }]
        };
        var lessonTwo = new LessonDefinition
        {
            Id = "lesson-02",
            Order = 2,
            Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted },
            Exercises = [new NoteReadingExerciseDefinition { Id = "exercise-02", Range = "D4-D4" }]
        };
        var catalog = new FakeLessonCatalog(lessonOne, lessonTwo);
        var progressStore = new InMemoryProgressStore();
        var attempts = new InMemoryExerciseAttemptRecorder();
        var progression = new LessonProgressionService(
            catalog,
            progressStore,
            new ExerciseCompletionEvaluator(attempts));
        var sessions = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseChallengeGenerator(),
            new SequenceEvaluator(
            [
                new ExerciseEvaluationResult { IsSuccessful = true, Score = 95 },
                new ExerciseEvaluationResult { IsSuccessful = true, Score = 30 }
            ]),
            new ExerciseScorer(),
            attempts);
        var progress = new ProgressUpdateService(progressStore, catalog, progression, attempts);

        Assert.False(progression.IsUnlocked(lessonTwo.Id));
        var session = sessions.StartSession(lessonOne.Id)!;
        var firstRuntime = sessions.StartExercise(session);
        var firstResult = sessions.SubmitAttempt(session, firstRuntime, firstRuntime.StartedAt.AddSeconds(1));
        progress.UpdateProgress(lessonOne.Id, firstResult);

        Assert.True(firstResult.IsExerciseCompleted);
        Assert.True(progression.IsCompleted(lessonOne.Id));
        Assert.True(progression.IsUnlocked(lessonTwo.Id));

        var retryRuntime = sessions.StartExercise(session);
        var retryResult = sessions.SubmitAttempt(session, retryRuntime, retryRuntime.StartedAt.AddSeconds(1));
        progress.UpdateProgress(lessonOne.Id, retryResult);
        progress.UpdateProgress(lessonOne.Id, retryResult);

        var stored = progressStore.GetProgress(lessonOne.Id)!;
        Assert.True(stored.IsCompleted);
        Assert.Equal(95, stored.BestScore);
        Assert.Equal(2, stored.AttemptCount);
        Assert.True(progression.IsUnlocked(lessonTwo.Id));
    }

    private sealed class SequenceEvaluator(IReadOnlyList<ExerciseEvaluationResult> results) : IExerciseEvaluator
    {
        private readonly Queue<ExerciseEvaluationResult> _results = new(results);

        public ExerciseEvaluationResult Evaluate(
            ExerciseChallenge challenge,
            IReadOnlyList<ExerciseInputEvent> inputEvents) => _results.Dequeue();
    }

    private sealed class FakeLessonCatalog(params LessonDefinition[] lessons) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => lessons;

        public LessonDefinition? GetById(string id) =>
            lessons.FirstOrDefault(lesson => string.Equals(lesson.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
