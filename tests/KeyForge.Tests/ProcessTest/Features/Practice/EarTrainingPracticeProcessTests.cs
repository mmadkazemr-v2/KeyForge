namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class EarTrainingPracticeProcessTests
{
    [Fact]
    public void Lesson03_AllRequiredExercisesIncludingChordRecognition_CanCompleteProgression()
    {
        var contentPath = Path.Combine(AppContext.BaseDirectory, "Content", "Lessons");
        var parser = new YamlLessonParser();
        var lesson01 = parser.Parse(File.ReadAllText(Path.Combine(contentPath, "Lesson01.yaml")));
        var lesson02 = parser.Parse(File.ReadAllText(Path.Combine(contentPath, "Lesson02.yaml")));
        var lesson03 = parser.Parse(File.ReadAllText(Path.Combine(contentPath, "Lesson03.yaml")));
        var catalog = new FakeLessonCatalog(lesson01, lesson02, lesson03);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        SeedCompletedPrerequisite(lesson01, store);
        SeedCompletedPrerequisite(lesson02, store);
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var sessions = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);
        var session = sessions.StartSession("lesson-03")!;

        Assert.Equal(10, new ExerciseChallengeGenerator().Create((EarTrainingExerciseDefinition)lesson03.Exercises[0]).ExpectedInputs.Count);
        foreach (var exercise in lesson03.Exercises)
        {
            var runtime = sessions.StartExercise(session);
            Assert.NotEmpty(runtime.Challenge.ExpectedInputs);
            if (exercise is EarTrainingExerciseDefinition { TaskType: EarTrainingTask.ChordRecognition })
            {
                Assert.Equal(10, runtime.Challenge.EarTraining!.ChordQuestions.Count);
                Assert.Contains("major", runtime.Challenge.ExpectedInputs.Select(input => input.Value));
                Assert.Contains("minor", runtime.Challenge.ExpectedInputs.Select(input => input.Value));
            }

            foreach (var expected in runtime.Challenge.ExpectedInputs)
            {
                runtime.CaptureInput(new ExerciseInputEvent
                {
                    Timestamp = runtime.StartedAt + (expected.ExpectedOffset ?? TimeSpan.FromMilliseconds(100)),
                    Offset = expected.ExpectedOffset ?? TimeSpan.FromMilliseconds(100),
                    Source = InputSource.ComputerKeyboard,
                    Type = expected.Type,
                    Value = expected.Value
                });
            }

            var result = sessions.SubmitAttempt(session, runtime, runtime.StartedAt.AddMinutes(1));
            progress.UpdateProgress(lesson03.Id, result);
            Assert.True(result.IsSuccessful, $"Exercise {exercise.Id} should pass with expected answers.");
            Assert.Equal(100, result.Score);
            Assert.True(result.IsExerciseCompleted);
            if (exercise is EarTrainingExerciseDefinition { TaskType: EarTrainingTask.ChordRecognition })
            {
                Assert.Equal(10, result.Evaluation.EarTraining!.CorrectAnswerCount);
                Assert.Contains(recorder.GetAttemptsByLesson("lesson-03"), attempt =>
                    attempt.ExerciseId == exercise.Id && attempt.IsSuccessful && attempt.Score == 100);
            }
            session.Next();
        }

        Assert.True(store.GetProgress("lesson-03")!.IsCompleted);
        Assert.Equal(lesson03.Exercises.Count, recorder.GetAttemptsByLesson("lesson-03").Count);
    }

    [Fact]
    public void Lesson03_FailedChordRecognitionAttempt_DoesNotCompleteRequiredLesson()
    {
        var contentPath = Path.Combine(AppContext.BaseDirectory, "Content", "Lessons");
        var lesson01 = new YamlLessonParser().Parse(File.ReadAllText(Path.Combine(contentPath, "Lesson01.yaml")));
        var lesson02 = new YamlLessonParser().Parse(File.ReadAllText(Path.Combine(contentPath, "Lesson02.yaml")));
        var lesson03 = new YamlLessonParser().Parse(File.ReadAllText(Path.Combine(contentPath, "Lesson03.yaml")));
        var catalog = new FakeLessonCatalog(lesson01, lesson02, lesson03);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        SeedCompletedPrerequisite(lesson01, store);
        SeedCompletedPrerequisite(lesson02, store);
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var sessions = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var session = sessions.StartSession("lesson-03")!;
        var runtime = sessions.StartExercise(session);
        var challenge = runtime.Challenge;
        Assert.Equal(EarTrainingTask.ChordRecognition, challenge.EarTraining!.TaskType);
        runtime.CaptureInput(new ExerciseInputEvent
        {
            Timestamp = runtime.StartedAt.AddMilliseconds(100),
            Offset = TimeSpan.FromMilliseconds(100),
            Source = InputSource.ComputerKeyboard,
            Type = InputEventType.Choice,
            Value = challenge.ExpectedInputs[0].Value == "major" ? "minor" : "major"
        });

        var result = sessions.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));

        Assert.False(result.IsSuccessful);
        Assert.Contains(recorder.GetAttemptsByLesson("lesson-03"), attempt =>
            attempt.ExerciseId == "ex-01" && !attempt.IsSuccessful && attempt.Score == 0);
        Assert.False(progression.IsCompleted("lesson-03"));
    }

    [Fact]
    public void PitchMatchingChallengeToAttemptAndProgress_EndToEnd()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-ear",
            Title = "Ear lesson",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises =
            [
                new EarTrainingExerciseDefinition
                {
                    Id = "ear-01",
                    Title = "Match pitch",
                    TaskType = EarTrainingTask.PitchMatching,
                    Rounds = 3,
                    Keys = ["C major"]
                }
            ]
        };
        var catalog = new SingleLessonCatalog(lesson);
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
        var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);
        var session = service.StartSession(lesson.Id)!;
        var runtime = service.StartExercise(session);

        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            runtime.CaptureInput(new ExerciseInputEvent
            {
                Timestamp = runtime.StartedAt.AddMilliseconds(100),
                Offset = TimeSpan.FromMilliseconds(100),
                Source = InputSource.VirtualPiano,
                Type = InputEventType.Note,
                Value = expected.Value
            });
        }

        var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(1));
        progress.UpdateProgress(lesson.Id, result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(3, result.Evaluation.EarTraining!.CorrectAnswerCount);
        Assert.Equal(100, Assert.Single(recorder.GetAttemptsByLesson(lesson.Id)).Score);
        Assert.Equal(100, store.GetProgress(lesson.Id)!.BestScore);
    }

    private sealed class SingleLessonCatalog(LessonDefinition lesson) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => [lesson];
        public LessonDefinition? GetById(string id) => string.Equals(id, lesson.Id, StringComparison.OrdinalIgnoreCase) ? lesson : null;
    }

    private static void SeedCompletedPrerequisite(LessonDefinition lesson, InMemoryProgressStore store) =>
        store.SaveProgress(new LessonProgress { LessonId = lesson.Id, IsCompleted = true, BestScore = 100 });

    private sealed class FakeLessonCatalog(params LessonDefinition[] lessons) : ILessonCatalog
    {
        public IReadOnlyList<LessonDefinition> GetAll() => lessons.OrderBy(lesson => lesson.Order).ToArray();
        public LessonDefinition? GetById(string id) => lessons.FirstOrDefault(lesson =>
            string.Equals(lesson.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
