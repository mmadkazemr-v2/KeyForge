namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class DurablePracticeProcessTests
{
    [Fact]
    public void ComparativePracticeSurvivesRestartAndUnlocksProgressWithoutNavigationWrites()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var lesson = new LessonDefinition { Id = "durable", Exercises = [new EarTrainingExerciseDefinition { Id = "compare", TaskType = EarTrainingTask.PitchComparison, Rounds = 2 }] };
            var catalog = new Catalog(lesson);
            var store = new JsonPracticeStore(path);
            var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(store));
            var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(), new ExerciseEvaluator(), new ExerciseScorer(), store);
            var session = service.StartSession(lesson.Id)!;
            var runtime = service.StartExercise(session);
            Assert.False(File.Exists(path));
            foreach (var expected in runtime.Challenge.ExpectedInputs)
                runtime.CaptureInput(new ExerciseInputEvent { Type = InputEventType.Choice, Value = expected.Value, Source = InputSource.ComputerKeyboard, Timestamp = runtime.StartedAt });
            var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(2));
            new ProgressUpdateService(store, catalog, progression, store).UpdateProgress(lesson.Id, result);
            var restored = new JsonPracticeStore(path);
            Assert.Single(restored.GetAttemptsByLesson(lesson.Id));
            Assert.Equal(1, restored.GetProgress(lesson.Id)!.AttemptCount);
            Assert.True(new ExerciseCompletionEvaluator(restored).AreAllExercisesCompleted(lesson.Id, lesson.Exercises));
            Assert.True(new LessonProgressionService(catalog, restored, new ExerciseCompletionEvaluator(restored)).IsCompleted(lesson.Id));
        }
        finally { File.Delete(path); }
    }
    private sealed class Catalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => id == lesson.Id ? lesson : null;
    }
}
