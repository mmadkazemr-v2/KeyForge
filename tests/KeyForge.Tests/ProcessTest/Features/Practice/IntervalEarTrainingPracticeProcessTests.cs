using KeyForge.Components.Practice;

namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class IntervalEarTrainingPracticeProcessTests
{
    [Fact]
    public void IntervalRecognitionChallengeToAudioRequestAttemptAndProgress_EndToEnd()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-ear-interval",
            Title = "Interval ear lesson",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises =
            [
                new EarTrainingExerciseDefinition
                {
                    Id = "ear-interval-01",
                    Title = "Recognise intervals",
                    TaskType = EarTrainingTask.IntervalRecognition,
                    Rounds = 3,
                    Intervals = ["M3", "P5"],
                    Direction = ExerciseDirection.Ascending,
                    Keys = []
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
        var details = runtime.Challenge.EarTraining!;
        var questions = details.IntervalQuestions;

        Assert.Equal(3, questions.Count);
        Assert.Equal(["M3", "P5", "M3"], runtime.Challenge.ExpectedInputs.Select(input => input.Value));
        Assert.All(runtime.Challenge.ExpectedInputs, input => Assert.Equal(InputEventType.Choice, input.Type));

        // The UI plays each generated question through the shared audio abstraction.
        Assert.All(questions, question =>
        {
            var sequence = IntervalRecognitionWorkspace.BuildPlaybackSequence(question);
            Assert.Equal(2, sequence.Count);
            Assert.Equal(question.SourceNote, sequence[0].Note);
            Assert.Equal(0, sequence[0].DelayMilliseconds);
            Assert.Equal(question.TargetNote, sequence[1].Note);
            Assert.Equal(700, sequence[1].DelayMilliseconds);

            var expectedTargetMidi = MusicalNote.Parse(question.SourceNote).MidiNumber +
                IntervalSemitones(question.Interval);
            Assert.Equal(expectedTargetMidi, MusicalNote.Parse(question.TargetNote).MidiNumber);
            Assert.InRange(MusicalNote.Parse(question.TargetNote).MidiNumber, 0, 127);
        });

        for (var index = 0; index < questions.Count; index++)
        {
            var timestamp = runtime.StartedAt.AddMilliseconds(500 * (index + 1));
            runtime.CaptureInput(new ExerciseInputEvent
            {
                Timestamp = timestamp,
                Offset = timestamp - runtime.StartedAt,
                Source = InputSource.ComputerKeyboard,
                Type = InputEventType.Choice,
                Value = questions[index].Interval
            });
        }

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(2));
        progress.UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(3, result.Evaluation.EarTraining!.CorrectAnswerCount);
        var attempt = Assert.Single(recorder.GetAttemptsByLesson(lesson.Id));
        Assert.Equal(100, attempt.Score);
        Assert.True(attempt.IsSuccessful);
        Assert.Equal(100, store.GetProgress(lesson.Id)!.BestScore);
    }

    private static int IntervalSemitones(string interval) => interval switch
    {
        "M3" => 4,
        "P5" => 7,
        _ => throw new InvalidOperationException($"Unexpected interval '{interval}'.")
    };

    private sealed class FakeLessonCatalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => string.Equals(id, lesson.Id, StringComparison.OrdinalIgnoreCase) ? lesson : null;
    }
}
