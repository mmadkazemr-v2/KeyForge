namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class SequencePracticeProcessTests
{
    [Theory]
    [InlineData(ExerciseType.Speed)]
    [InlineData(ExerciseType.FingerIndependence)]
    public void SequenceRuntimeToAttemptAndProgress_EndToEnd(ExerciseType exerciseType)
    {
        ExerciseDefinition exercise = exerciseType == ExerciseType.Speed
            ? new SpeedExerciseDefinition { Id = "speed-01", Pattern = SpeedPattern.RepeatedNote, Repetitions = 2, Tempo = new TempoConfiguration { DefaultBpm = 120 } }
            : new FingerIndependenceExerciseDefinition { Id = "finger-01", Fingers = [1, 2], Hand = PracticeHand.Right, Tempo = new TempoConfiguration { DefaultBpm = 120 } };
        var lesson = new LessonDefinition
        {
            Id = "lesson-sequence", Title = "Sequence", Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true }, Exercises = [exercise]
        };
        var catalog = new Catalog(lesson);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(), new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);

        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            runtime.CaptureInput(new ExerciseInputEvent { Timestamp = runtime.StartedAt.Add(expected.ExpectedOffset ?? TimeSpan.Zero), Offset = expected.ExpectedOffset ?? TimeSpan.Zero, Source = InputSource.VirtualPiano, Type = InputEventType.Note, Value = expected.Value });
        }

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(2));

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(100, Assert.Single(recorder.GetAttemptsByLesson(lesson.Id)).Score);
    }

    private sealed class Catalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => id == lesson.Id ? lesson : null;
    }
}
