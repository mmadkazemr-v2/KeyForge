namespace KeyForge.Tests.UnitTest.Infrastructure.Yaml.Parsing;

public sealed class LentoVivoContentTests
{
    [Fact]
    public void LentoVivoAdaptedLessons_LoadThroughCatalog_AndGenerateChallengesWithoutReplacingExistingLessons()
    {
        var contentDirectory = FindLessonsDirectory();
        var catalog = new FileSystemLessonCatalog(
            Options.Create(new LessonCatalogOptions { ContentPath = contentDirectory }),
            new YamlLessonParser());

        var sessionThree = Assert.IsType<LessonDefinition>(catalog.GetById("lento-vivo-session-03-adapted"));
        var sessionFour = Assert.IsType<LessonDefinition>(catalog.GetById("lento-vivo-session-04-adapted"));
        var sessionFive = Assert.IsType<LessonDefinition>(catalog.GetById("lento-vivo-session-05-adapted"));

        Assert.NotNull(catalog.GetById("lesson-03"));
        Assert.NotNull(catalog.GetById("lesson-04"));
        Assert.NotNull(catalog.GetById("lesson-05"));
        Assert.Equal(104, sessionFour.Order);
        Assert.Equal(105, sessionFive.Order);
        Assert.Equal(103, sessionThree.Order);
        var generator = new ExerciseChallengeGenerator();
        Assert.All(sessionThree.Exercises, exercise =>
        {
            Assert.Contains("not an exact worksheet transcription", exercise.Description, StringComparison.Ordinal);
            Assert.Equal("lento-vivo-piano", sessionThree.Course!.Id);
        });

        var sessionThreeRhythms = sessionThree.Exercises.OfType<RhythmExerciseDefinition>().ToArray();
        Assert.Equal(2, sessionThreeRhythms.Length);
        Assert.Contains(sessionThreeRhythms, exercise => exercise.Id == "lento-vivo-s03-rh-quarter-pulse");
        Assert.Contains(sessionThreeRhythms, exercise => exercise.Id == "lento-vivo-s03-lh-quarter-pulse");
        Assert.All(sessionThreeRhythms, exercise =>
        {
            Assert.Equal(2, exercise.Measures!.Count);
            Assert.All(exercise.Measures, measure => Assert.Equal(4, measure.Pattern.Count));
            Assert.Equal(40, exercise.Tempo!.MinBpm);
            Assert.Equal(70, exercise.Tempo.MaxBpm);
            Assert.Equal(50, exercise.Tempo.DefaultBpm);
            Assert.Equal([0d, 1200d, 2400d, 3600d, 4800d, 6000d, 7200d, 8400d],
                GetExpectedMilliseconds(generator.Create(exercise)));
            Assert.Equal(1500, generator.Create(exercise, 40).ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
            Assert.Equal(1090.909, generator.Create(exercise, 55).ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds, 3);
            Assert.Equal(857.143, generator.Create(exercise, 70).ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds, 3);
        });

        var adaptedLessons = new[] { sessionThree, sessionFour, sessionFive };
        Assert.All(adaptedLessons, lesson =>
        {
            Assert.Contains("Adapted practice", lesson.Description, StringComparison.Ordinal);
            Assert.All(lesson.Exercises, exercise =>
                Assert.Contains("Adapted practice", exercise.Description, StringComparison.Ordinal));
        });

        foreach (var lesson in adaptedLessons)
        {
            foreach (var exercise in lesson.Exercises)
            {
                var challenge = generator.Create(exercise);
                Assert.Equal(exercise.Id, challenge.ExerciseId);
                Assert.NotEmpty(challenge.ExpectedInputs);
            }
        }

        var sessionFourWhiteKeys = Assert.IsType<SequenceExerciseDefinition>(
            Assert.Single(sessionFour.Exercises, exercise => exercise.Id == "lento-vivo-s04-white-key-walk"));
        Assert.Equal(
            ["C4", "D4", "E4", "F4", "G4", "F4", "E4", "D4", "C4"],
            generator.Create(sessionFourWhiteKeys).ExpectedInputs.Select(input => input.Value));
        Assert.Equal(PracticeHand.Right,
            generator.Create(sessionFourWhiteKeys).Sequence!.Hand);

        var sessionFourRhythm = Assert.IsType<RhythmExerciseDefinition>(
            Assert.Single(sessionFour.Exercises, exercise => exercise.Id == "lento-vivo-s04-quarter-note-pulse"));
        Assert.Equal([0d, 1200d, 2400d, 3600d], GetExpectedMilliseconds(generator.Create(sessionFourRhythm)));

        var sessionFivePentascale = Assert.IsType<SequenceExerciseDefinition>(
            Assert.Single(sessionFive.Exercises, exercise => exercise.Id == "lento-vivo-s05-g-pentascale"));
        Assert.Equal(
            ["G4", "A4", "B4", "C5", "D5", "C5", "B4", "A4", "G4"],
            generator.Create(sessionFivePentascale).ExpectedInputs.Select(input => input.Value));

        var sessionFiveIntervals = Assert.IsType<IntervalExerciseDefinition>(
            Assert.Single(sessionFive.Exercises, exercise => exercise.Id == "lento-vivo-s05-seconds-and-thirds"));
        Assert.Equal(
            ["D4", "E4", "D4", "E4", "D4"],
            generator.Create(sessionFiveIntervals).ExpectedInputs.Select(input => input.Value));

        var sessionFiveTranspose = Assert.IsType<SequenceExerciseDefinition>(
            Assert.Single(sessionFive.Exercises, exercise => exercise.Id == "lento-vivo-s05-transpose-three-notes"));
        Assert.Equal(
            ["C4", "D4", "E4", "G4", "A4", "B4"],
            generator.Create(sessionFiveTranspose).ExpectedInputs.Select(input => input.Value));

        var sessionFiveRhythm = Assert.IsType<RhythmExerciseDefinition>(
            Assert.Single(sessionFive.Exercises, exercise => exercise.Id == "lento-vivo-s05-meter-quarter-half"));
        Assert.Equal(
            [0d, 1200d, 3600d, 4800d, 9600d, 10800d, 12000d],
            GetExpectedMilliseconds(generator.Create(sessionFiveRhythm)));
        Assert.Equal(40, sessionFiveRhythm.Tempo!.MinBpm);
        Assert.Equal(70, sessionFiveRhythm.Tempo.MaxBpm);
        Assert.Equal(50, sessionFiveRhythm.Tempo.DefaultBpm);
        Assert.Equal(
            1500,
            generator.Create(sessionFiveRhythm, selectedBpm: 40)
                .ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
        Assert.Equal(50, sessionFiveRhythm.Tempo.DefaultBpm);
    }

    private static double[] GetExpectedMilliseconds(ExerciseChallenge challenge) =>
        challenge.ExpectedInputs
            .Select(input => input.ExpectedOffset!.Value.TotalMilliseconds)
            .ToArray();

    private static string FindLessonsDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var copiedContent = Path.Combine(directory.FullName, "Content", "Lessons");
            if (File.Exists(Path.Combine(copiedContent, "LentoVivo", "Session03.yaml")))
                return copiedContent;

            var sourceContent = Path.Combine(directory.FullName, "src", "KeyForge", "Content", "Lessons");
            if (File.Exists(Path.Combine(sourceContent, "LentoVivo", "Session03.yaml")))
                return sourceContent;
        }

        throw new DirectoryNotFoundException("Could not locate the KeyForge lesson content directory.");
    }
}
