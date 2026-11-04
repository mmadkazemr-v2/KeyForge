namespace KeyForge.Tests.UnitTest.Infrastructure.Yaml.Parsing;

public sealed class RhythmAuthoringTests
{
    private readonly YamlLessonParser _parser = new();
    private readonly RhythmChallengeGenerator _generator = new();

    [Fact]
    public void FixtureYaml_CompilesAndEvaluatesEveryAuthoredExercise()
    {
        var yaml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Rhythm", "LessonRhythm.yaml"));
        var lesson = _parser.Parse(yaml);
        Assert.Equal(4, lesson.Exercises.Count);
        foreach (var definition in lesson.Exercises.Cast<RhythmExerciseDefinition>())
        {
            var challenge = _generator.Create(definition);
            var runtime = new RuntimeExercise(definition, challenge, DateTime.UtcNow);
            foreach (var expected in challenge.ExpectedInputs)
            {
                runtime.CaptureInput(new ExerciseInputEvent
                {
                    Timestamp = runtime.StartedAt + expected.ExpectedOffset!.Value,
                    Offset = expected.ExpectedOffset.Value,
                    Source = InputSource.ComputerKeyboard,
                    Type = InputEventType.Beat,
                    Value = "beat"
                });
            }
            var result = new RhythmExerciseEvaluator().Evaluate(challenge, runtime.InputEvents);
            Assert.Equal(100, result.Score);
            Assert.Equal(0, result.Rhythm!.ExtraHitCount);
        }
    }

    [Fact]
    public void ParseAndGenerate_BeginnerDurationsMetersAndRests_CompileToRuntimeTimeline()
    {
        const string yaml = """
            id: rhythm-fixture
            exercises:
              - id: quarters
                type: rhythm
                timeSignature: 4/4
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: quarter }
                  - { duration: quarter }
                  - { duration: quarter }
                  - { duration: quarter }
              - id: mixed
                type: rhythm
                timeSignature: 4/4
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: quarter }
                  - { duration: eighth }
                  - { duration: eighth }
                  - { duration: quarter }
                  - { duration: quarter }
              - id: quarter-rest
                type: rhythm
                timeSignature: 4/4
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: quarter }
                  - { duration: quarter, rest: true }
                  - { duration: quarter }
                  - { duration: quarter }
              - id: three-four
                type: rhythm
                timeSignature: 3/4
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: quarter }
                  - { duration: quarter }
                  - { duration: quarter }
              - id: six-eight
                type: rhythm
                timeSignature: 6/8
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: eighth }
                  - { duration: eighth }
                  - { duration: eighth }
                  - { duration: eighth }
                  - { duration: eighth }
                  - { duration: eighth }
              - id: dotted
                type: rhythm
                timeSignature: 4/4
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: dottedQuarter }
                  - { duration: eighth }
                  - { duration: dottedEighth }
                  - { duration: sixteenth }
                  - { duration: quarter }
              - id: dotted-half
                type: rhythm
                timeSignature: 4/4
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: dottedHalf }
                  - { duration: eighth }
                  - { duration: eighth }
              - id: sixteenths
                type: rhythm
                timeSignature: 4/4
                tempo: { defaultBpm: 60 }
                pattern:
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
                  - { duration: sixteenth }
            """;

        var lesson = _parser.Parse(yaml);
        RuntimeExercise Runtime(string id)
        {
            var definition = Assert.IsType<RhythmExerciseDefinition>(lesson.Exercises.Single(item => item.Id == id));
            var challenge = _generator.Create(definition);
            return new RuntimeExercise(definition, challenge, DateTime.UtcNow);
        }

        var quarters = Runtime("quarters");
        Assert.Equal([0d, 1000d, 2000d, 3000d], Offsets(quarters));
        Assert.Equal(4000, quarters.Challenge.Rhythm!.Duration.TotalMilliseconds);

        var mixed = Runtime("mixed");
        Assert.Equal([0d, 1000d, 1500d, 2000d, 3000d], Offsets(mixed));
        Assert.Equal([0, 4, 6, 8, 12], mixed.Challenge.Rhythm!.Events.Select(item => item.StartSixteenthUnits));

        var rest = Runtime("quarter-rest");
        Assert.Equal([0d, 2000d, 3000d], Offsets(rest));
        Assert.True(rest.Challenge.Rhythm!.Events[1].IsRest);
        Assert.Equal(4000, rest.Challenge.Rhythm.Duration.TotalMilliseconds);

        Assert.Equal([0d, 1000d, 2000d], Offsets(Runtime("three-four")));
        var sixEight = Runtime("six-eight");
        Assert.Equal([0d, 500d, 1000d, 1500d, 2000d, 2500d], Offsets(sixEight));
        Assert.Equal(3000, sixEight.Challenge.Rhythm!.Duration.TotalMilliseconds);
        Assert.Equal([0d, 500d, 1000d, 1500d, 2000d, 2500d],
            sixEight.Challenge.Rhythm.PulseOffsets.Select(offset => offset.TotalMilliseconds));

        var dotted = Runtime("dotted");
        Assert.Equal([0d, 1500d, 2000d, 2750d, 3000d], Offsets(dotted));
        Assert.Equal([0, 6, 8, 11, 12], dotted.Challenge.Rhythm!.Events.Select(item => item.StartSixteenthUnits));
        Assert.Equal(4000, dotted.Challenge.Rhythm!.Duration.TotalMilliseconds);
        var dottedHalf = Runtime("dotted-half");
        Assert.Equal([0d, 3000d, 3500d], Offsets(dottedHalf));
        Assert.Equal(4000, dottedHalf.Challenge.Rhythm!.Duration.TotalMilliseconds);
        Assert.Equal(16, Runtime("sixteenths").Challenge.Rhythm!.Events.Count);
    }

    [Theory]
    [InlineData("whole", 1, 16)]
    [InlineData("half", 2, 8)]
    [InlineData("eighth", 8, 2)]
    [InlineData("sixteenth", 16, 1)]
    public void ParseAndGenerate_UniformAuthoredMeasuresHaveExactMusicalPositions(string duration, int count, int units)
    {
        var yaml = "id: duration-fixture\nexercises:\n  - id: basic-duration\n    type: rhythm\n    timeSignature: 4/4\n    tempo: { defaultBpm: 60 }\n    pattern:\n" +
                   string.Join("\n", Enumerable.Repeat($"      - {{ duration: {duration} }}", count));
        var definition = Assert.IsType<RhythmExerciseDefinition>(Assert.Single(_parser.Parse(yaml).Exercises));

        var challenge = _generator.Create(definition);

        Assert.Equal(Enumerable.Range(0, count).Select(index => index * units),
            challenge.Rhythm!.Events.Select(item => item.StartSixteenthUnits));
        Assert.Equal(Enumerable.Range(0, count).Select(index => index * units * 250d),
            challenge.ExpectedInputs.Select(item => item.ExpectedOffset!.Value.TotalMilliseconds));
        Assert.Equal(4000, challenge.Rhythm.Duration.TotalMilliseconds);
    }

    [Theory]
    [InlineData(60, 0, 1000, 1500, 2000, 3000)]
    [InlineData(80, 0, 750, 1125, 1500, 2250)]
    [InlineData(120, 0, 500, 750, 1000, 1500)]
    public void Generate_MixedPattern_UsesQuarterNoteBpmAndExactMusicalPositions(
        int bpm,
        double first,
        double second,
        double third,
        double fourth,
        double fifth)
    {
        const string yaml = """
            id: tempo-fixture
            exercises:
              - id: mixed
                type: rhythm
                timeSignature: 4/4
                pattern:
                  - { duration: quarter }
                  - { duration: eighth }
                  - { duration: eighth }
                  - { duration: quarter }
                  - { duration: quarter }
            """;
        var exercise = Assert.IsType<RhythmExerciseDefinition>(Assert.Single(_parser.Parse(yaml).Exercises));

        var challenge = _generator.Create(exercise, bpm);

        Assert.Equal([first, second, third, fourth, fifth],
            challenge.ExpectedInputs.Select(input => input.ExpectedOffset!.Value.TotalMilliseconds));
    }

    [Fact]
    public void ParseAndGenerate_MultipleMeasures_ContinuesWithoutResettingTimeline()
    {
        const string yaml = """
            id: measures-fixture
            exercises:
              - id: two-bars
                type: rhythm
                timeSignature: 2/4
                tempo: { defaultBpm: 120 }
                measures:
                  - pattern:
                      - { duration: half }
                  - pattern:
                      - { duration: quarter }
                      - { duration: quarter }
            """;

        var exercise = Assert.IsType<RhythmExerciseDefinition>(Assert.Single(_parser.Parse(yaml).Exercises));
        var challenge = _generator.Create(exercise);

        Assert.Equal([0d, 1000d, 1500d], challenge.ExpectedInputs.Select(input => input.ExpectedOffset!.Value.TotalMilliseconds));
        Assert.Equal([1, 2, 2], challenge.Rhythm!.Events.Select(item => item.MeasureNumber));
        Assert.Equal(2000, challenge.Rhythm.Duration.TotalMilliseconds);
    }

    [Fact]
    public void Evaluate_HitDuringRestWindow_IsCountedAsExtraWhileRestCreatesNoTarget()
    {
        var exercise = new RhythmExerciseDefinition
        {
            Id = "rest-check",
            TimeSignature = "4/4",
            Tempo = new TempoConfiguration { DefaultBpm = 60 },
            Pattern =
            [
                new() { Duration = RhythmDuration.Quarter },
                new() { Duration = RhythmDuration.Quarter, Rest = true },
                new() { Duration = RhythmDuration.Quarter },
                new() { Duration = RhythmDuration.Quarter }
            ]
        };
        var challenge = _generator.Create(exercise);
        var result = new RhythmExerciseEvaluator().Evaluate(challenge,
        [
            Hit(0), Hit(1000), Hit(2000), Hit(3000)
        ]);

        Assert.Equal(3, challenge.ExpectedInputs.Count);
        Assert.Equal(95, result.Score);
        Assert.Equal(3, result.Rhythm!.SuccessfulHitCount);
        Assert.Equal(1, result.Rhythm.ExtraHitCount);
        Assert.Equal(0, result.Rhythm.MissedBeatCount);
    }

    [Fact]
    public void ParseAndGenerate_LegacyLessonFilesRemainCompatible()
    {
        var lessonsDirectory = Path.Combine(AppContext.BaseDirectory, "Content", "Lessons");
        foreach (var file in Directory.GetFiles(lessonsDirectory, "*.yaml"))
        {
            var lesson = _parser.Parse(File.ReadAllText(file));
            foreach (var exercise in lesson.Exercises.OfType<RhythmExerciseDefinition>())
            {
                var challenge = _generator.Create(exercise, exercise.Tempo?.DefaultBpm);
                if (exercise.Pattern is not null || exercise.Measures is not null)
                {
                    Assert.Equal(challenge.Rhythm!.Events.Count(item => !item.IsRest), challenge.ExpectedInputs.Count);
                }
                else
                {
                    Assert.Equal(exercise.PatternCount * RhythmMeter.Parse(exercise.TimeSignature).Numerator,
                        challenge.ExpectedInputs.Count);
                }
            }
        }
    }

    [Theory]
    [InlineData("timeSignature: 4/16", "timeSignature")]
    [InlineData("timeSignature: ''", "timeSignature")]
    [InlineData("timeSignature: not-a-meter", "timeSignature")]
    [InlineData("tempo: { defaultBpm: 0 }", "tempo")]
    [InlineData("tempo: { defaultBpm: -30 }", "tempo")]
    [InlineData("timeSignature: 4/4\npattern: []", "pattern")]
    [InlineData("timeSignature: 4/4\npattern:\n  - { duration: thirtySecond }", "duration")]
    [InlineData("timeSignature: 4/4\npattern:\n  - { duration: dottedSixteenth }", "duration")]
    [InlineData("timeSignature: 4/4\npattern:\n  - { duration: dottedQuarter }", "pattern")]
    [InlineData("timeSignature: 4/4\npattern:\n  - { duration: quarter, rest: maybe }\n  - { duration: quarter }\n  - { duration: quarter }\n  - { duration: quarter }", "rest")]
    [InlineData("timeSignature: 4/4\npattern:\n  - { duration: quarter }\n  - { duration: quarter }\n  - { duration: quarter }", "pattern")]
    [InlineData("measures: []", "measures")]
    [InlineData("measures:\n  - pattern: []", "measures[0].pattern")]
    [InlineData("measures:\n  - pattern:\n      - { duration: whole }\n      - { duration: quarter }", "measures[0].pattern")]
    [InlineData("pattern: [{ duration: whole }]\nmeasures: []", "pattern")]
    [InlineData("patternCount: 2\npattern: [{ duration: whole }]", "patternCount")]
    public void Parse_InvalidAuthoredRhythm_ReportsLessonExerciseAndField(string config, string field)
    {
        var yaml = $"""
            id: invalid-rhythm
            exercises:
              - id: bad-pattern
                type: rhythm
                {config.Replace("\n", "\n    ", StringComparison.Ordinal)}
            """;

        var error = Assert.Throws<YamlLessonParseException>(() => _parser.Parse(yaml));
        Assert.Contains("invalid-rhythm", error.Message, StringComparison.Ordinal);
        Assert.Contains("bad-pattern", error.Message, StringComparison.Ordinal);
        Assert.Contains(field, error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static double[] Offsets(RuntimeExercise runtime) => runtime.Challenge.ExpectedInputs
        .Select(input => input.ExpectedOffset!.Value.TotalMilliseconds).ToArray();

    private static ExerciseInputEvent Hit(double milliseconds) => new()
    {
        Timestamp = DateTime.UnixEpoch.AddMilliseconds(milliseconds),
        Offset = TimeSpan.FromMilliseconds(milliseconds),
        Source = InputSource.Midi,
        Type = InputEventType.Beat,
        Value = "beat"
    };
}
