namespace KeyForge.Tests.ProcessTest.Features.Practice;

using KeyForge.Components.Practice;

public sealed class DynamicsPracticeProcessTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CalibratedTargetsReachFeedbackAndProgress(bool reversed)
    {
        var lesson = new YamlLessonParser().Parse("""
            id: dynamics-process
            title: Original calibrated dynamics
            order: 1
            unlock: { mode: immediate }
            completion: { requireAllExercises: true }
            exercises:
              - id: dynamics
                type: sequence
                title: Soft then loud
                tempo: { defaultBpm: 60 }
                performance:
                  velocityCalibration: { soft: 30, loud: 90, tolerance: 0.1 }
                  events:
                    - { pitch: C4, onsetBeats: 0, durationBeats: 1, relativeVelocity: 0 }
                    - { pitch: D4, onsetBeats: 1, durationBeats: 1, relativeVelocity: 1 }
            """);
        var recorder = new InMemoryExerciseAttemptRecorder();
        var store = new InMemoryProgressStore();
        var catalog = new Catalog(lesson);
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(), new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);
        for (var i = 0; i < 2; i++)
        {
            var input = MidiInputAdapter.TryCreate(new MidiInputMessage { DeviceId = "keys", DeviceName = "test", Status = 0x90, Data1 = i == 0 ? 60 : 62, Data2 = (i == 0) != reversed ? 30 : 90, TimestampMilliseconds = 12000 + i * 1000, Sequence = i + 1 }, runtime.StartedAt.AddSeconds(3), 12000)!;
            runtime.CaptureInput(input);
        }
        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(3));
        new ProgressUpdateService(store, catalog, progression, recorder).UpdateProgress(lesson.Id, result);
        Assert.Equal(!reversed, result.IsSuccessful);
        Assert.Equal(!reversed, store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Single(recorder.GetAttemptsByLesson(lesson.Id));
    }
    private sealed class Catalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => id == lesson.Id ? lesson : null;
    }
}
