using KeyForge.Components.Practice;

namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed partial class LentoVivoAdaptedPracticeProcessTests
{
    public static IEnumerable<object[]> WorksheetCases()
    {
        (string Suffix, int[] Beats, int Total)[] patterns =
        [
            ("01-rh", [1, 1, 1, 1, 1, 1, 2, 2, 2, 2], 14),
            ("01-lh", [1, 1, 2, 2, 2, 2], 10),
            ("02-rh", [2, 1, 1, 2, 1, 2, 1, 1, 1], 12),
            ("02-lh", [2, 1, 1, 1, 1, 1, 2, 1, 2], 12),
            ("03-rh", [3, 1, 1, 3, 1, 3], 12),
            ("03-lh", [3, 1, 1, 3, 3, 1], 12),
            ("04-rh", [4, 1, 1, 2, 3, 4], 15),
            ("04-lh", [4, 1, 1, 2, 1], 9)
        ];
        foreach (var pattern in patterns)
            foreach (var bpm in new[] { 40, 55, 60, 70 })
                yield return [pattern.Suffix, pattern.Beats, pattern.Total, bpm];
    }

    [Theory]
    [MemberData(nameof(WorksheetCases))]
    public void Worksheet_YamlToMidiToProgress(string suffix, int[] beats, int total, int bpm)
    {
        const string lessonId = "lento-vivo-session-03-adapted";
        var id = "lento-vivo-s03-section-" + suffix;
        var catalog = new FileSystemLessonCatalog(
            Options.Create(new LessonCatalogOptions { ContentPath = FindLessonsDirectory() }), new YamlLessonParser());
        var lesson = catalog.GetById(lessonId)!;
        Assert.Equal(10, lesson.Exercises.Count);
        Assert.Equal(10, lesson.Exercises.Select(e => e.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        var definition = Assert.IsType<RhythmExerciseDefinition>(Assert.Single(lesson.Exercises, e => e.Id == id));
        definition.Validate();
        Assert.True(definition.Unmetered);
        Assert.Null(definition.Measures);
        Assert.Equal(beats, definition.Pattern!.Select(p => RhythmDurationUnits.GetSixteenthUnits(p.Duration) / 4));
        Assert.Equal(40, definition.Tempo!.MinBpm);
        Assert.Equal(70, definition.Tempo.MaxBpm);
        Assert.Equal(4, definition.Metronome!.CountInBeats);
        Assert.False(definition.Metronome.Audio);

        var store = new InMemoryProgressStore();
        var recorder = new InMemoryExerciseAttemptRecorder();
        var completion = new ExerciseCompletionEvaluator(recorder);
        var progression = new LessonProgressionService(catalog, store, completion);
        var practice = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(),
            new ExerciseEvaluator(), new ExerciseScorer(), recorder);
        var progress = new ProgressUpdateService(store, catalog, progression, recorder);
        var library = new PracticeLibraryQueryService(catalog,
            new LessonProgressQueryService(catalog, store, progression), completion);
        var node = Assert.Single(Assert.Single(library.GetCourses(), c => c.Metadata.Id == "lento-vivo-piano")
            .Sessions, s => s.Lesson.Id == lessonId);
        Assert.Contains(node.Exercises, e => e.Id == id);
        Assert.Contains(node.Exercises, e => e.Id == "lento-vivo-s03-rh-quarter-pulse");
        Assert.Contains(node.Exercises, e => e.Id == "lento-vivo-s03-lh-quarter-pulse");

        // Retry each performance against a new runtime, retaining attempts and best score.
        foreach (var variant in new[] { "correct", "early", "late", "missing", "extra", "relative", "drift" })
        {
            var session = practice.StartSession(lessonId)!;
            while (session.GetCurrentExercise()!.Id != id) session.Next();
            var runtime = practice.StartExercise(session);
            runtime.SetSelectedBpm(bpm);
            var timeline = runtime.Challenge.Rhythm!;
            Assert.InRange(Math.Abs(total * 60000d / bpm - timeline.Duration.TotalMilliseconds), 0, 0.0001);
            Assert.Equal(total, timeline.PulseOffsets.Count);
            var position = 0;
            for (var i = 0; i < beats.Length; i++)
            {
                Assert.InRange(Math.Abs(position * 60000d / bpm - timeline.Events[i].StartOffset.TotalMilliseconds), 0, 0.0001);
                Assert.InRange(Math.Abs(beats[i] * 60000d / bpm - timeline.Events[i].Length.TotalMilliseconds), 0, 0.0001);
                Assert.Equal(0, timeline.Events[i].MeasureNumber);
                Assert.Equal(InputEventType.Beat, runtime.Challenge.ExpectedInputs[i].Type);
                Assert.Equal(timeline.Events[i].StartOffset, runtime.Challenge.ExpectedInputs[i].ExpectedOffset);
                position += beats[i];
            }
            const double origin = 20000;
            long sequence = 0;
            void Capture(double offset)
            {
                var message = new MidiInputMessage
                {
                    DeviceId = "simulated",
                    Status = 0x90,
                    Data1 = 48 + (int)(sequence % 24),
                    Data2 = 96,
                    TimestampMilliseconds = origin + offset,
                    Sequence = ++sequence
                };
                var input = Assert.IsType<ExerciseInputEvent>(MidiInputAdapter.TryCreateBeat(
                    message, true, runtime.StartedAt.AddMilliseconds(offset), origin,
                    isTimingReference: (variant is "relative" or "drift") && sequence == 1));
                runtime.CaptureMidiEvent(input);
                runtime.CaptureInput(input);
            }
            if (variant != "missing")
            {
                var index = 0;
                foreach (var expected in runtime.Challenge.ExpectedInputs)
                {
                    var drift = variant == "drift" ? 40 * index : 0;
                    Capture(expected.ExpectedOffset!.Value.TotalMilliseconds + (variant == "early" && sequence > 0 ? -100 : variant == "late" ? 100 : drift));
                    index++;
                }
            }
            if (variant == "extra") Capture(300);
            var result = practice.SubmitAttempt(session, runtime, runtime.StartedAt.Add(timeline.Duration));
            progress.UpdateProgress(lessonId, result);
            if (variant is "correct" or "relative")
            {
                Assert.True(result.IsSuccessful);
                Assert.Equal(100, result.Score);
                Assert.Equal(beats.Length, result.Evaluation.Rhythm!.SuccessfulHitCount);
                if (variant == "relative")
                {
                    Assert.Equal(RhythmHitAccuracy.Unverified, result.Evaluation.Rhythm.Beats[0].Accuracy);
                    Assert.Null(result.Evaluation.Rhythm.Beats[0].TimingError);
                }
            }
            else if (variant == "missing")
            {
                Assert.False(result.IsSuccessful);
                Assert.Equal(0, result.Score);
                Assert.Equal(beats.Length, result.Evaluation.Rhythm!.MissedBeatCount);
            }
            else
            {
                Assert.True(result.Score < 100);
                if (variant == "drift")
                {
                    Assert.False(result.IsSuccessful);
                    Assert.True(result.Evaluation.Rhythm!.Beats.Skip(1).Where(beat => beat.TimingError.HasValue)
                        .Select(beat => beat.TimingError!.Value.TotalMilliseconds).Zip(
                            result.Evaluation.Rhythm.Beats.Skip(2).Where(beat => beat.TimingError.HasValue)
                                .Select(beat => beat.TimingError!.Value.TotalMilliseconds), (a, b) => b > a).All(value => value));
                }
                if (variant == "extra") Assert.Equal(1, result.Evaluation.Rhythm!.ExtraHitCount);
                else if (variant != "drift") Assert.Equal(variant == "early" ? -100d * (beats.Length - 1) / beats.Length : 100,
                    result.Evaluation.Rhythm!.AverageTimingErrorMilliseconds!.Value, 3);
            }
        }
        Assert.Equal(7, recorder.GetAttemptsByLesson(lessonId).Count);
        Assert.Contains(recorder.GetAttemptsByLesson(lessonId), attempt => attempt.IsSuccessful && attempt.Score == 100);
        Assert.Contains(recorder.GetAttemptsByLesson(lessonId), attempt => !attempt.IsSuccessful && attempt.Score == 0);
        Assert.Equal(7, store.GetProgress(lessonId)!.AttemptCount);
        Assert.Equal(100, store.GetProgress(lessonId)!.BestScore);
        Assert.Contains(id, completion.GetCompletedExerciseIds(lessonId, lesson.Exercises));
        Assert.False(store.GetProgress(lessonId)!.IsCompleted);
    }
}
