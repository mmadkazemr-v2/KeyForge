using KeyForge.Components.Practice;

namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed partial class LentoVivoAdaptedPracticeProcessTests
{
    [Theory]
    [InlineData("lento-vivo-s03-rh-quarter-pulse")]
    [InlineData("lento-vivo-s03-lh-quarter-pulse")]
    public void SessionThreeAdaptedRhythm_MidiAttacksAreEvaluatedAndRecorded(string exerciseId)
    {
        var contentDirectory = FindLessonsDirectory();
        var catalog = new FileSystemLessonCatalog(
            Options.Create(new LessonCatalogOptions { ContentPath = contentDirectory }),
            new YamlLessonParser());
        var lessonId = "lento-vivo-session-03-adapted";
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var completion = new ExerciseCompletionEvaluator(recorder);
        var progression = new LessonProgressionService(catalog, store, completion);
        var practice = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);

        var session = Assert.IsType<PracticeSession>(practice.StartSession(lessonId));
        if (exerciseId == "lento-vivo-s03-lh-quarter-pulse")
            session.Next();
        var runtime = practice.StartExercise(session);
        Assert.Equal(exerciseId, runtime.Definition.Id);
        var rhythm = Assert.IsType<RhythmExerciseDefinition>(runtime.Definition);
        Assert.Equal(50, rhythm.Tempo!.DefaultBpm);
        Assert.Equal(8, runtime.Challenge.Rhythm!.Events.Count);
        Assert.NotEmpty(runtime.Challenge.ExpectedInputs);

        const double browserOriginMilliseconds = 20_000;
        long sequence = 0;
        foreach (var expected in runtime.Challenge.ExpectedInputs)
        {
            var timestamp = browserOriginMilliseconds + expected.ExpectedOffset!.Value.TotalMilliseconds;
            var midi = new MidiInputMessage
            {
                DeviceId = "test-controller",
                DeviceName = "Simulated MIDI keyboard",
                Status = 0x90,
                Data1 = MusicalNote.Parse("C4").MidiNumber,
                Data2 = 100,
                TimestampMilliseconds = timestamp,
                Sequence = ++sequence
            };
            var input = Assert.IsType<ExerciseInputEvent>(MidiInputAdapter.TryCreateBeat(
                midi, true, runtime.StartedAt.Add(expected.ExpectedOffset.Value), browserOriginMilliseconds));
            runtime.CaptureMidiEvent(input);
            runtime.CaptureInput(input);
        }

        var result = practice.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(15));
        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(8, result.Evaluation.Rhythm!.SuccessfulHitCount);
        Assert.Equal(0, result.Evaluation.Rhythm.MissedBeatCount);
        progress.UpdateProgress(lessonId, result);

        Assert.Single(recorder.GetAttemptsByLesson(lessonId));
        Assert.Equal(1, store.GetProgress(lessonId)!.AttemptCount);
        Assert.Contains(exerciseId, completion.GetCompletedExerciseIds(
            lessonId, catalog.GetById(lessonId)!.Exercises));
    }

    [Fact]
    public void SessionFourWhiteKeySequence_MidiPitchesAreScoredAndRecordedInProgress()
    {
        var contentDirectory = FindLessonsDirectory();
        var catalog = new FileSystemLessonCatalog(
            Options.Create(new LessonCatalogOptions { ContentPath = contentDirectory }),
            new YamlLessonParser());
        var lessonId = "lento-vivo-session-04-adapted";
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var progression = new LessonProgressionService(
            catalog,
            store,
            new ExerciseCompletionEvaluator(recorder));
        var practice = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);

        var correct = SubmitWhiteKeySequence(practice, lessonId, playExpectedNotes: true);
        Assert.Equal("lento-vivo-s04-white-key-walk", correct.Attempt.ExerciseId);
        Assert.True(correct.IsSuccessful);
        Assert.Equal(100, correct.Score);
        progress.UpdateProgress(lessonId, correct);

        var incorrect = SubmitWhiteKeySequence(practice, lessonId, playExpectedNotes: false);
        Assert.False(incorrect.IsSuccessful);
        Assert.Equal(0, incorrect.Score);
        progress.UpdateProgress(lessonId, incorrect);

        Assert.Equal(2, store.GetProgress(lessonId)!.AttemptCount);
        Assert.Equal(100, store.GetProgress(lessonId)!.BestScore);
    }

    [Fact]
    public void SessionFiveMeterPattern_MidiAttacksAreScoredAndRecordedInProgress()
    {
        var contentDirectory = FindLessonsDirectory();
        var catalog = new FileSystemLessonCatalog(
            Options.Create(new LessonCatalogOptions { ContentPath = contentDirectory }),
            new YamlLessonParser());
        var lessonId = "lento-vivo-session-05-adapted";
        var lesson = Assert.IsType<LessonDefinition>(catalog.GetById(lessonId));
        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var progression = new LessonProgressionService(
            catalog,
            store,
            new ExerciseCompletionEvaluator(recorder));
        var practice = new PracticeSessionService(
            catalog,
            progression,
            new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(),
            new ExerciseScorer(),
            recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);
        var rhythmExerciseId = "lento-vivo-s05-meter-quarter-half";

        var correct = SubmitMidiPattern(practice, lessonId, expectedEventLimit: null);
        Assert.Equal(rhythmExerciseId, correct.Attempt.ExerciseId);
        Assert.True(correct.IsSuccessful);
        Assert.Equal(100, correct.Score);
        Assert.Equal(7, correct.Evaluation.Rhythm!.ExpectedBeatCount);
        Assert.Equal(7, correct.Evaluation.Rhythm.SuccessfulHitCount);
        progress.UpdateProgress(lessonId, correct);

        var incomplete = SubmitMidiPattern(practice, lessonId, expectedEventLimit: 2);
        Assert.False(incomplete.IsSuccessful);
        Assert.True(incomplete.Score < RhythmExerciseEvaluator.SuccessScore);
        Assert.Equal(5, incomplete.Evaluation.Rhythm!.MissedBeatCount);
        progress.UpdateProgress(lessonId, incomplete);

        var attempts = recorder.GetAttemptsByLesson(lessonId);
        Assert.Equal(2, attempts.Count);
        Assert.All(attempts, attempt => Assert.Equal(rhythmExerciseId, attempt.ExerciseId));
        Assert.Equal(2, store.GetProgress(lessonId)!.AttemptCount);
        Assert.Equal(100, store.GetProgress(lessonId)!.BestScore);
    }

    private static SessionResult SubmitWhiteKeySequence(
        PracticeSessionService practice,
        string lessonId,
        bool playExpectedNotes)
    {
        var session = Assert.IsType<PracticeSession>(practice.StartSession(lessonId));
        session.Next(); // The first item is the adapted quarter-note pulse.
        var runtime = practice.StartExercise(session);
        Assert.Equal("lento-vivo-s04-white-key-walk", runtime.Definition.Id);

        const double browserOriginMilliseconds = 18_000;
        for (var index = 0; index < runtime.Challenge.ExpectedInputs.Count; index++)
        {
            var expected = runtime.Challenge.ExpectedInputs[index];
            var pitch = playExpectedNotes ? expected.Value : "B3";
            var offset = TimeSpan.FromMilliseconds(250 + index * 200);
            var message = new MidiInputMessage
            {
                DeviceId = "test-controller",
                DeviceName = "Simulated MIDI keyboard",
                Status = 0x90,
                Data1 = MusicalNote.Parse(pitch).MidiNumber,
                Data2 = 100,
                TimestampMilliseconds = browserOriginMilliseconds + offset.TotalMilliseconds,
                Sequence = index + 1
            };
            var input = Assert.IsType<ExerciseInputEvent>(MidiInputAdapter.TryCreate(
                message,
                runtime.StartedAt.Add(offset),
                browserOriginMilliseconds));

            runtime.CaptureMidiEvent(input);
            runtime.CaptureInput(input);
        }

        return practice.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(5));
    }

    private static SessionResult SubmitMidiPattern(
        PracticeSessionService practice,
        string lessonId,
        int? expectedEventLimit)
    {
        var session = Assert.IsType<PracticeSession>(practice.StartSession(lessonId));
        var runtime = practice.StartExercise(session);
        Assert.Equal("lento-vivo-s05-meter-quarter-half", runtime.Definition.Id);

        var expectedInputs = runtime.Challenge.ExpectedInputs;
        var inputsToCapture = expectedEventLimit.HasValue
            ? expectedInputs.Take(expectedEventLimit.Value)
            : expectedInputs;
        const double browserOriginMilliseconds = 12_000;
        long sequence = 0;
        foreach (var expected in inputsToCapture)
        {
            var timestamp = browserOriginMilliseconds + expected.ExpectedOffset!.Value.TotalMilliseconds;
            var message = new MidiInputMessage
            {
                DeviceId = "test-controller",
                DeviceName = "Simulated MIDI keyboard",
                Status = 0x90,
                Data1 = MusicalNote.Parse("C4").MidiNumber,
                Data2 = 100,
                TimestampMilliseconds = timestamp,
                Sequence = ++sequence
            };
            var input = Assert.IsType<ExerciseInputEvent>(MidiInputAdapter.TryCreateBeat(
                message,
                isActive: true,
                runtime.StartedAt.Add(expected.ExpectedOffset.Value),
                browserOriginMilliseconds));

            runtime.CaptureMidiEvent(input);
            runtime.CaptureInput(input);
        }

        return practice.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(20));
    }

    private static string FindLessonsDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var copiedContent = Path.Combine(directory.FullName, "Content", "Lessons");
            if (File.Exists(Path.Combine(copiedContent, "LentoVivo", "Session05.yaml")))
                return copiedContent;

            var sourceContent = Path.Combine(directory.FullName, "src", "KeyForge", "Content", "Lessons");
            if (File.Exists(Path.Combine(sourceContent, "LentoVivo", "Session05.yaml")))
                return sourceContent;
        }

        throw new DirectoryNotFoundException("Could not locate the KeyForge lesson content directory.");
    }
}
