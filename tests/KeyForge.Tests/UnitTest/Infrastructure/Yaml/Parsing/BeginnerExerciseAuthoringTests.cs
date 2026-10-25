namespace KeyForge.Tests.UnitTest.Infrastructure.Yaml.Parsing;

public sealed class BeginnerExerciseAuthoringTests
{
    private readonly YamlLessonParser _parser = new();
    private readonly ExerciseChallengeGenerator _generator = new();

    [Fact]
    public void ParseAndGenerate_BeginnerAuthoringFixture_ReachesRuntime()
    {
        const string yaml = """
            id: authoring-fixture
            title: Beginner authoring fixture
            level: beginner
            order: 1
            exercises:
              - id: note-drill
                type: noteReading
                range: "C4-G4"
                count: 10
                randomized: true
              - id: fixed-pattern
                type: sequence
                sequence: [C4, D4, E4, F4, G4]
                hand: right
                fingers: [1, 2, 3, 4, 5]
              - id: c-major
                type: sequence
                scale:
                  root: C4
                  type: major
                  direction: ascending
                  octaves: 1
              - id: d-major
                type: sequence
                scale:
                  root: D4
                  type: major
                  direction: ascending
                  octaves: 1
              - id: c-major-down
                type: sequence
                scale:
                  root: C4
                  type: major
                  direction: descending
                  octaves: 1
              - id: a-minor
                type: sequence
                scale:
                  root: A4
                  type: naturalMinor
                  direction: ascending
                  octaves: 1
              - id: c-major-turnaround
                type: sequence
                scale:
                  root: C4
                  type: major
                  direction: ascendingDescending
                  octaves: 1
              - id: left-hand-pattern
                type: fingerIndependence
                hand: left
                sequence: [C4, D4, E4, F4, G4]
                fingers: [1, 2, 3, 4, 5]
            """;

        var lesson = _parser.Parse(yaml);
        var noteDrill = Assert.IsType<NoteReadingExerciseDefinition>(lesson.Exercises[0]);
        Assert.Equal(10, noteDrill.Count);
        Assert.True(noteDrill.Randomized);
        var randomChallenge = new NoteReadingChallengeGenerator(_ => 0).Create(noteDrill);
        Assert.Equal(10, randomChallenge.ExpectedInputs.Count);
        Assert.All(randomChallenge.ExpectedInputs, input => Assert.Equal("C4", input.Value));

        RuntimeExercise CreateRuntime(ExerciseDefinition definition) => new(
            definition,
            _generator.Create(definition),
            DateTime.UtcNow);

        var fixedRuntime = CreateRuntime(lesson.Exercises[1]);
        Assert.Equal(["C4", "D4", "E4", "F4", "G4"], fixedRuntime.Challenge.Sequence!.ExpectedNotes);
        Assert.Equal(PracticeHand.Right, fixedRuntime.Challenge.Sequence.Hand);
        Assert.Equal([1, 2, 3, 4, 5], fixedRuntime.Challenge.Sequence.Fingers);

        Assert.Equal(["C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5"],
            CreateRuntime(lesson.Exercises[2]).Challenge.Sequence!.ExpectedNotes);
        Assert.Equal(["D4", "E4", "F#4", "G4", "A4", "B4", "C#5", "D5"],
            CreateRuntime(lesson.Exercises[3]).Challenge.Sequence!.ExpectedNotes);
        Assert.Equal(["C5", "B4", "A4", "G4", "F4", "E4", "D4", "C4"],
            CreateRuntime(lesson.Exercises[4]).Challenge.Sequence!.ExpectedNotes);
        Assert.Equal(["A4", "B4", "C5", "D5", "E5", "F5", "G5", "A5"],
            CreateRuntime(lesson.Exercises[5]).Challenge.Sequence!.ExpectedNotes);
        Assert.Equal(["C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5", "B4", "A4", "G4", "F4", "E4", "D4", "C4"],
            CreateRuntime(lesson.Exercises[6]).Challenge.Sequence!.ExpectedNotes);

        var fingerRuntime = CreateRuntime(lesson.Exercises[7]);
        Assert.Equal(PracticeHand.Left, fingerRuntime.Challenge.FingerIndependence!.Hand);
        Assert.Equal([1, 2, 3, 4, 5], fingerRuntime.Challenge.FingerIndependence.Fingers);
    }

    [Fact]
    public void ParseAndGenerate_ExplicitNotePoolAndStructuredRange_AreSupported()
    {
        const string yaml = """
            id: note-pools
            exercises:
              - id: pool
                type: noteReading
                notes: [C4, D#4, G4]
                count: 5
              - id: range
                type: noteReading
                noteRange:
                  from: C4
                  to: E4
                count: 3
            """;

        var exercises = _parser.Parse(yaml).Exercises.Cast<NoteReadingExerciseDefinition>().ToArray();
        Assert.Equal(["C4", "D#4", "G4"], NoteReadingChallengeGenerator.ParseRange(exercises[0]).Select(note => note.Value));
        Assert.Equal(["C4", "C#4", "D4", "D#4", "E4"],
            NoteReadingChallengeGenerator.ParseRange(exercises[1]).Select(note => note.Value));
        Assert.Equal(5, _generator.Create(exercises[0]).ExpectedInputs.Count);
    }

    [Fact]
    public void ParseAndGenerate_ExistingLessonFiles_RemainRuntimeCompatible()
    {
        var lessonsDirectory = Path.Combine(AppContext.BaseDirectory, "Content", "Lessons");
        var lessonFiles = Directory.GetFiles(lessonsDirectory, "*.yaml");
        Assert.NotEmpty(lessonFiles);

        foreach (var file in lessonFiles)
        {
            var lesson = _parser.Parse(File.ReadAllText(file));
            foreach (var exercise in lesson.Exercises)
            {
                var challenge = _generator.Create(exercise, exercise.Tempo?.DefaultBpm);
                Assert.Equal(exercise.Id, challenge.ExerciseId);
                _ = new RuntimeExercise(exercise, challenge, DateTime.UtcNow);
            }
        }
    }

    [Theory]
    [InlineData("count: 0", "count")]
    [InlineData("notes: []", "notes")]
    [InlineData("notes: [C4, H4]", "notes")]
    [InlineData("noteRange: { from: G4, to: C4 }", "noteRange")]
    [InlineData("noteRange: { from: 'C-2', to: C4 }", "noteRange.from")]
    [InlineData("sequence: []", "sequence")]
    [InlineData("sequence: [C4, H4]", "sequence")]
    [InlineData("fingers: [1, 6]", "fingers")]
    [InlineData("hand: center", "hand")]
    [InlineData("scale: { root: C4, type: major, direction: sideways, octaves: 1 }", "scale.direction")]
    [InlineData("scale: { root: C4, type: major, octaves: 0 }", "scale.octaves")]
    [InlineData("scale: { root: C9, type: major, octaves: 1 }", "scale.octaves")]
    [InlineData("scale: { root: C8, type: major, octaves: 2 }", "scale.octaves")]
    [InlineData("scale: { type: major, octaves: 1 }", "scale.root")]
    [InlineData("scale: { root: C4, type: exotic, octaves: 1 }", "scale.type")]
    public void Parse_InvalidBeginnerConfiguration_ReportsExerciseAndField(string property, string field)
    {
        var type = property.StartsWith("scale:", StringComparison.Ordinal) || property.StartsWith("sequence:", StringComparison.Ordinal)
            ? "sequence"
            : property.StartsWith("fingers:", StringComparison.Ordinal)
                ? "fingerIndependence"
                : "noteReading";
        var yaml = $"""
            id: invalid-fixture
            exercises:
              - id: invalid-exercise
                type: {type}
                {property}
            """;

        var error = Assert.Throws<YamlLessonParseException>(() => _parser.Parse(yaml));
        Assert.Contains("invalid-exercise", error.Message, StringComparison.Ordinal);
        Assert.Contains(field, error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_FingeringMustMatchAuthoredSequenceLength()
    {
        const string yaml = """
            id: invalid-fingering
            exercises:
              - id: mismatch
                type: sequence
                sequence: [C4, D4]
                fingers: [1]
            """;

        var error = Assert.Throws<YamlLessonParseException>(() => _parser.Parse(yaml));
        Assert.Contains("lesson 'invalid-fingering'", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exercise 'mismatch'", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fingers", error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
