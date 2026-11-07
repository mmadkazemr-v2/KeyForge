namespace KeyForge.Tests.ProcessTest.Features.Practice;

using KeyForge.Components.Practice;

public sealed class ArpeggioPracticeProcessTests
{
    [Fact]
    public void AuthoredInversionReachesEvaluationAndProgress()
    {
        var lesson = new YamlLessonParser().Parse("""
            id: arpeggio-test
            title: Original arpeggio
            order: 1
            unlock: { mode: immediate }
            completion: { requireAllExercises: true }
            exercises:
              - id: arpeggio
                type: sequence
                title: First inversion
                arpeggio: { root: C4, quality: major, inversion: 1, octaves: 1, direction: ascending }
                hand: right
                fingers: [1, 2, 4, 5]
            """);
        var catalog = new Catalog(lesson);
        var recorder = new InMemoryExerciseAttemptRecorder();
        var store = new InMemoryProgressStore();
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(), new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);
        Assert.Equal(new[] { "E4", "G4", "C5", "E5" }, runtime.Challenge.Sequence!.ExpectedNotes);
        for (var index = 0; index < runtime.Challenge.ExpectedInputs.Count; index++)
        {
            var input = MidiInputAdapter.TryCreate(new MidiInputMessage { DeviceId = "keys", DeviceName = "test", Status = 0x90, Data1 = MusicalNote.Parse(runtime.Challenge.ExpectedInputs[index].Value).MidiNumber, Data2 = 90, TimestampMilliseconds = 12000 + index * 1000, Sequence = index + 1 }, runtime.StartedAt.AddSeconds(10), 12000)!;
            runtime.CaptureInput(input);
        }
        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(10));
        new ProgressUpdateService(store, catalog, progression, recorder).UpdateProgress(lesson.Id, result);
        Assert.Equal(100, result.Score);
        Assert.True(store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Single(recorder.GetAttemptsByLesson(lesson.Id));
    }
    private sealed class Catalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => id == lesson.Id ? lesson : null;
    }
}
