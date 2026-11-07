namespace KeyForge.Tests.ProcessTest.Features.Practice;

using KeyForge.Components.Practice;

public sealed class PerformancePracticeProcessTests
{
    public const string Yaml = """
        id: performance-test
        title: Original test phrase
        order: 1
        unlock: { mode: immediate }
        completion: { requireAllExercises: true }
        exercises:
          - id: phrase
            type: sequence
            title: Pitched timing
            tempo: { defaultBpm: 60 }
            performance:
              events:
                - { pitch: C4, onsetBeats: 0, durationBeats: 1 }
                - { pitch: D4, onsetBeats: 1, durationBeats: 2 }
                - { onsetBeats: 3, durationBeats: 1 }
                - { pitch: E4, onsetBeats: 4, durationBeats: 3 }
                - { pitch: F4, onsetBeats: 7, durationBeats: 4 }
        """;

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void YamlToMidiEvaluationAttemptAndProgress(bool incorrect, bool scoreHolds)
    {
        var lesson = new YamlLessonParser().Parse(scoreHolds ? Yaml.Replace("performance:", "performance:\n      scoringMode: keyHold") : Yaml);
        var recorder = new InMemoryExerciseAttemptRecorder();
        var store = new InMemoryProgressStore();
        var catalog = new Catalog(lesson);
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(), new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);
        Assert.Empty(recorder.GetAttemptsByLesson(lesson.Id));
        var sequence = 0;
        foreach (var item in runtime.Challenge.Performance!.Events.Where(e => e.Pitch is not null))
        {
            var input = MidiInputAdapter.TryCreate(new MidiInputMessage
            {
                DeviceId = "test",
                DeviceName = "test",
                Status = 0x90,
                Data1 = MusicalNote.Parse(incorrect ? "G4" : item.Pitch!).MidiNumber,
                Data2 = 90,
                TimestampMilliseconds = 12000 + 500 + item.Onset.TotalMilliseconds,
                Sequence = ++sequence
            }, runtime.StartedAt.AddSeconds(20), 12000)!;
            runtime.CaptureMidiEvent(input);
            runtime.CaptureInput(input);
            if (scoreHolds)
            {
                var release = MidiInputAdapter.TryCreate(new MidiInputMessage
                {
                    DeviceId = "test",
                    DeviceName = "test",
                    Status = 0x90,
                    Data1 = input.MidiData1!.Value,
                    Data2 = 0,
                    TimestampMilliseconds = input.BrowserTimestampMilliseconds!.Value + item.Duration.TotalMilliseconds,
                    Sequence = ++sequence
                }, runtime.StartedAt.AddSeconds(20), 12000)!;
                runtime.CaptureMidiEvent(release);
                runtime.CaptureInput(release);
            }
        }
        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(30));
        new ProgressUpdateService(store, catalog, progression, recorder).UpdateProgress(lesson.Id, result);
        Assert.Equal(!incorrect, result.IsSuccessful);
        Assert.Equal(incorrect ? 0 : 100, result.Score);
        Assert.Single(recorder.GetAttemptsByLesson(lesson.Id));
        Assert.Equal(!incorrect, store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Equal(1, store.GetProgress(lesson.Id)!.AttemptCount);
    }

    [Theory]
    [InlineData("durationBeats: 1", "durationBeats: 0")]
    [InlineData("onsetBeats: 0", "onsetBeats: -1")]
    [InlineData("pitch: C4", "pitch: unknown")]
    public void InvalidAuthoringIsRejected(string before, string after) =>
        Assert.Throws<YamlLessonParseException>(() => new YamlLessonParser().Parse(Yaml.Replace(before, after)));

    private sealed class Catalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => id == lesson.Id ? lesson : null;
    }
}
