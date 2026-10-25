namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class EarTrainingPracticeProcessTests
{
    [Fact]
    public void PitchMatchingChallengeToAttemptAndProgress_EndToEnd()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-ear",
            Title = "Ear lesson",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises =
            [
                new EarTrainingExerciseDefinition
                {
                    Id = "ear-01",
                    Title = "Match pitch",
                    TaskType = EarTrainingTask.PitchMatching,
                    Rounds = 3,
                    Keys = ["C major"]
                }
            ]
        };
        var catalog = new FakeLessonCatalog(lesson);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);

        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            runtime.CaptureInput(new ExerciseInputEvent
            {
                Timestamp = runtime.StartedAt.AddMilliseconds(100),
                Offset = TimeSpan.FromMilliseconds(100),
                Source = InputSource.VirtualPiano,
                Type = InputEventType.Note,
                Value = expected.Value
            });
        }

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
        progress.UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(3, result.Evaluation.EarTraining!.CorrectAnswerCount);
        Assert.Equal(100, Assert.Single(recorder.GetAttemptsByLesson(lesson.Id)).Score);
        Assert.Equal(100, store.GetProgress(lesson.Id)!.BestScore);
    }

    private sealed class FakeLessonCatalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => string.Equals(id, lesson.Id, StringComparison.OrdinalIgnoreCase) ? lesson : null;
    }
}
