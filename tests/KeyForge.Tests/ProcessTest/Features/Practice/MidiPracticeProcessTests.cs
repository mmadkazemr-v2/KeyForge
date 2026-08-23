using KeyForge.Components.Practice;

namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class MidiPracticeProcessTests
{
    [Fact]
    public void MidiNoteToRuntimeEvaluationAttemptAndProgress_EndToEnd()
    {
        var exercise = new NoteReadingExerciseDefinition
        {
            Id = "note-midi-01",
            Clef = ClefType.Treble,
            Range = "C4-C4"
        };
        var lesson = new LessonDefinition
        {
            Id = "lesson-midi", Title = "MIDI", Order = 1,
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
            var input = MidiInputAdapter.TryCreate(new MidiInputMessage
            {
                DeviceId = "controller", Status = 0x90,
                Data1 = MusicalNote.Parse(expected.Value).MidiNumber,
                Data2 = 110, TimestampMilliseconds = 0
            }, runtime.StartedAt, runtime.StartedAt.AddMilliseconds(100));

            runtime.CaptureInput(input!);
        }
        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
        new ProgressUpdateService(store, catalog, progression).UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.All(runtime.InputEvents, input =>
        {
            Assert.Equal(InputSource.Midi, input.Source);
            Assert.Equal(110, input.Velocity);
        });
        Assert.Equal(1, store.GetProgress(lesson.Id)!.AttemptCount);
        Assert.Equal(100, store.GetProgress(lesson.Id)!.BestScore);
    }

    private sealed class Catalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => id == lesson.Id ? lesson : null;
    }
}
