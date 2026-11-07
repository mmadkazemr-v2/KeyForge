namespace KeyForge.Tests.ProcessTest.Features.Practice;

using KeyForge.Components.Practice;

public sealed class PolyphonicPracticeProcessTests
{
    [Theory]
    [InlineData("correct", true)]
    [InlineData("missing-right", false)]
    [InlineData("missing-left", false)]
    [InlineData("pitch", false)]
    [InlineData("timing", false)]
    [InlineData("early-release", false)]
    [InlineData("extra", true)]
    [InlineData("restart", true)]
    public void TwoPartsUseCombinedMidiAndUpdateProgress(string scenario, bool successful)
    {
        const string yaml = """
            id: two-parts
            title: Original independent parts
            order: 1
            unlock: { mode: immediate }
            completion: { requireAllExercises: true }
            exercises:
              - id: phrase
                type: sequence
                title: Bass and melody
                tempo: { defaultBpm: 60 }
                performance:
                  scoringMode: keyHold
                  events:
                    - { pitch: C3, onsetBeats: 0, durationBeats: 4, voice: bass, hand: left }
                    - { pitch: C4, onsetBeats: 0, durationBeats: 1, voice: melody, hand: right }
                    - { pitch: D4, onsetBeats: 1, durationBeats: 1, voice: melody, hand: right }
                    - { pitch: E4, onsetBeats: 2, durationBeats: 1, voice: melody, hand: right }
                    - { pitch: F4, onsetBeats: 3, durationBeats: 1, voice: melody, hand: right }
            """;
        var lesson = new YamlLessonParser().Parse(yaml);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var catalog = new Catalog(lesson);
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(), new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);
        long sequence = 0;
        void Add(string pitch, double milliseconds, bool on)
        {
            var input = MidiInputAdapter.TryCreate(new MidiInputMessage
            {
                DeviceId = "keys",
                DeviceName = "test",
                Status = on ? 0x90 : 0x80,
                Data1 = MusicalNote.Parse(pitch).MidiNumber,
                Data2 = on ? 90 : 0,
                TimestampMilliseconds = 12000 + milliseconds,
                Sequence = ++sequence
            }, runtime.StartedAt.AddSeconds(30), 12000)!;
            runtime.CaptureMidiEvent(input);
            runtime.CaptureInput(input);
        }
        if (scenario == "restart")
        {
            Add("G4", 0, true);
            runtime.ResetCapturedInputs();
            Assert.Empty(runtime.MidiEvents);
            Assert.Empty(recorder.GetAttemptsByLesson(lesson.Id));
        }
        foreach (var item in runtime.Challenge.Performance!.Events.Reverse()) // Simultaneous chord ordering is not an identity constraint.
        {
            if (scenario == "missing-right" && item.Pitch == "C4" || scenario == "missing-left" && item.Pitch == "C3") continue;
            var pitch = scenario == "pitch" && item.Pitch == "C4" ? "G4" : item.Pitch!;
            var offset = item.Onset.TotalMilliseconds + (scenario == "timing" && item.Pitch == "C4" ? 400 : 0);
            Add(pitch, offset, true);
            Add(pitch, offset + item.Duration.TotalMilliseconds - (scenario == "early-release" && item.Pitch == "C4" ? 400 : 0), false);
        }
        if (scenario == "extra") { Add("G5", 500, true); Add("G5", 1500, false); }
        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(30));
        new ProgressUpdateService(store, catalog, progression, recorder).UpdateProgress(lesson.Id, result);
        Assert.Equal(successful, result.IsSuccessful);
        Assert.Equal(successful, store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Single(recorder.GetAttemptsByLesson(lesson.Id));
        Assert.Equal(1, store.GetProgress(lesson.Id)!.AttemptCount);
        Assert.Contains(result.Evaluation.Performance!.Notes, n => n.Voice == "bass" && n.Hand == PracticeHand.Left);
        if (scenario == "extra") Assert.Equal(95, result.Score);
        if (scenario is "correct" or "restart") Assert.Equal(100, result.Score);
    }
    private sealed class Catalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => id == lesson.Id ? lesson : null;
    }
}
