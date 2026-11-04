using KeyForge.Components.Practice;

namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class ScalePracticeProcessTests
{
    [Fact]
    public void Lesson05CmajorScales_SyntheticMidiCompletesBothDirectionsAndLessonProgress()
    {
        var lessonPath = Path.Combine(AppContext.BaseDirectory, "Content", "Lessons", "Lesson05.yaml");
        var lesson = new YamlLessonParser().Parse(File.ReadAllText(lessonPath));
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var catalog = new SingleLessonCatalog(lesson);
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var sessions = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);
        var session = sessions.StartSession(lesson.Id)!;
        var expectedDirections = new[]
        {
            new[] { "C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5" },
            new[] { "C5", "B4", "A4", "G4", "F4", "E4", "D4", "C4" }
        };
        var expectedFingers = new[]
        {
            new[] { 1, 2, 3, 1, 2, 3, 4, 5 },
            new[] { 5, 4, 3, 2, 1, 3, 2, 1 }
        };

        foreach (var (expectedNotes, exerciseIndex) in expectedDirections.Select((notes, index) => (notes, index)))
        {
            var definition = Assert.IsType<SequenceExerciseDefinition>(lesson.Exercises[exerciseIndex]);
            Assert.Equal(PracticeHand.Right, definition.Hand);
            Assert.Equal(expectedFingers[exerciseIndex], definition.Fingers);
            var runtime = sessions.StartExercise(session);
            Assert.Equal(expectedNotes, runtime.Challenge.Sequence!.ExpectedNotes);
            Assert.Equal(expectedFingers[exerciseIndex], runtime.Challenge.Sequence.Fingers);
            Assert.Equal(PracticeHand.Right, runtime.Challenge.Sequence.Hand);
            const double browserOrigin = 40_000;
            foreach (var (expectedNote, noteIndex) in expectedNotes.Select((note, index) => (note, index)))
            {
                var note = MusicalNote.Parse(expectedNote);
                var message = new MidiInputMessage
                {
                    DeviceId = "simulated-midi",
                    DeviceName = "Synthetic MIDI",
                    Status = 0x90,
                    Data1 = note.MidiNumber,
                    Data2 = 96,
                    TimestampMilliseconds = browserOrigin + noteIndex * 25,
                    Sequence = noteIndex + 1
                };
                var input = MidiInputAdapter.TryCreate(
                    message,
                    runtime.StartedAt.AddMilliseconds(noteIndex * 25),
                    browserOrigin);

                Assert.NotNull(input);
                runtime.CaptureMidiEvent(input);
                runtime.CaptureInput(input);
            }

            var result = sessions.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
            progress.UpdateProgress(lesson.Id, result);

            Assert.True(result.IsSuccessful);
            Assert.Equal(100, result.Score);
            Assert.True(result.IsExerciseCompleted);
            if (exerciseIndex < expectedDirections.Length - 1) session.Next();
        }

        Assert.True(store.GetProgress(lesson.Id)!.IsCompleted);
        Assert.Equal(2, recorder.GetAttemptsByLesson(lesson.Id).Count);
    }

    private sealed class SingleLessonCatalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => string.Equals(id, lesson.Id, StringComparison.OrdinalIgnoreCase)
            ? lesson
            : null;
    }
}
