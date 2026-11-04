namespace KeyForge.Tests.ProcessTest.Features.Practice;

using KeyForge.Components.Practice;

public sealed class RhythmAuthoringProcessTests
{
    [Fact]
    public void AuthoredNonUniformRhythm_BrowserRelativeMidiHits_EvaluateAndRecordAttempt()
    {
        const string yaml = """
            id: rhythm-process
            title: Rhythm process
            order: 1
            unlock: { mode: immediate }
            completion: { requireAllExercises: true }
            exercises:
              - id: mixed-rhythm
                type: rhythm
                title: Mixed rhythm
                timeSignature: 4/4
                tempo: { defaultBpm: 120 }
                pattern:
                  - { duration: quarter }
                  - { duration: eighth }
                  - { duration: eighth }
                  - { duration: quarter }
                  - { duration: quarter }
            """;
        var lesson = new YamlLessonParser().Parse(yaml);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var catalog = new OneLessonCatalog(lesson);
        var completion = new ExerciseCompletionEvaluator(recorder);
        var progression = new LessonProgressionService(catalog, store, completion);
        var sessions = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);

        var session = sessions.StartSession(lesson.Id)!;
        var runtime = sessions.StartExercise(session);
        Assert.Equal([0d, 500d, 750d, 1000d, 1500d],
            runtime.Challenge.ExpectedInputs.Select(item => item.ExpectedOffset!.Value.TotalMilliseconds));

        const double browserOriginMilliseconds = 12_000;
        var deliveryTime = runtime.StartedAt.AddSeconds(20);
        foreach (var (expected, sequence) in runtime.Challenge.ExpectedInputs.Select((item, index) => (item, index)))
        {
            var browserEventTimestamp = browserOriginMilliseconds + expected.ExpectedOffset!.Value.TotalMilliseconds;
            var message = new MidiInputMessage
            {
                DeviceId = "virtual-midi",
                DeviceName = "Test MIDI Source",
                Status = 0x90,
                Data1 = 60,
                Data2 = 90,
                TimestampMilliseconds = browserEventTimestamp,
                Sequence = sequence + 1
            };
            var input = MidiInputAdapter.TryCreateBeat(
                message,
                isActive: true,
                deliveryTime.AddMilliseconds(sequence * 200),
                browserOriginMilliseconds);
            Assert.NotNull(input);
            runtime.CaptureInput(input);
        }

        var result = sessions.SubmitAttempt(session, runtime, deliveryTime.AddSeconds(3));
        progress.UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(5, result.Evaluation.Rhythm!.SuccessfulHitCount);
        Assert.Equal(0, result.Evaluation.Rhythm.ExtraHitCount);
        Assert.Single(recorder.GetAttemptsByLesson(lesson.Id));
        Assert.True(store.GetProgress(lesson.Id)!.IsCompleted);
    }

    private sealed class OneLessonCatalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) =>
            string.Equals(lesson.Id, id, StringComparison.OrdinalIgnoreCase) ? lesson : null;
    }
}
