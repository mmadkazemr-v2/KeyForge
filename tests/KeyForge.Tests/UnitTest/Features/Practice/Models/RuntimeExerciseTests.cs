namespace KeyForge.Tests.UnitTest.Features.Practice.Models;

public sealed class RuntimeExerciseTests
{
    private static readonly DateTime StartedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CaptureInput_PreservesNormalizedEventData()
    {
        var runtime = Runtime();
        var input = new ExerciseInputEvent
        {
            Timestamp = StartedAt.AddMilliseconds(45),
            Source = InputSource.VirtualPiano,
            Type = InputEventType.Note,
            Value = "C4"
        };

        runtime.CaptureInput(input);

        Assert.Same(input, Assert.Single(runtime.InputEvents));
        Assert.Equal(InputSource.VirtualPiano, runtime.InputEvents[0].Source);
    }

    [Fact]
    public void CaptureInput_AllowsMultipleTimingEvents()
    {
        var runtime = Runtime(InputEventType.Beat, expectedOffset: TimeSpan.FromSeconds(2));

        runtime.CaptureInput(Input("beat", StartedAt.AddMilliseconds(2045), InputEventType.Beat));
        runtime.CaptureInput(Input("beat", StartedAt.AddMilliseconds(4010), InputEventType.Beat));

        Assert.Equal(2, runtime.InputEvents.Count);
        Assert.Equal(TimeSpan.FromSeconds(2), runtime.Challenge.ExpectedInputs[0].ExpectedOffset);
    }

    [Fact]
    public void CaptureInput_BeforeStart_ThrowsWithoutMutation()
    {
        var runtime = Runtime();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            runtime.CaptureInput(Input("C4", StartedAt.AddTicks(-1))));
        Assert.Empty(runtime.InputEvents);
    }

    [Fact]
    public void CaptureInput_NegativeOffset_ThrowsWithoutMutation()
    {
        var runtime = Runtime();
        var input = Input("C4", StartedAt);
        input = new ExerciseInputEvent
        {
            Timestamp = input.Timestamp,
            Offset = TimeSpan.FromMilliseconds(-1),
            Source = input.Source,
            Type = input.Type,
            Value = input.Value
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => runtime.CaptureInput(input));
        Assert.Empty(runtime.InputEvents);
    }

    [Fact]
    public void Constructor_MismatchedChallenge_ThrowsArgumentException()
    {
        var exercise = new NoteReadingExerciseDefinition { Id = "note-01", Range = "C4-G4" };
        var challenge = Challenge("different", InputEventType.Note, null);

        Assert.Throws<ArgumentException>(() => new RuntimeExercise(exercise, challenge, StartedAt));
    }

    [Fact]
    public void SetSelectedBpm_RegeneratesRuntimeChallengeWithoutMutatingDefinition()
    {
        var definition = new RhythmExerciseDefinition
        {
            Id = "rhythm-01",
            TimeSignature = "4/4",
            PatternCount = 1,
            Tempo = new TempoConfiguration
            {
                DefaultBpm = 120,
                AllowUserChange = true,
                MinBpm = 60,
                MaxBpm = 140,
                Step = 5
            }
        };
        var generator = new RhythmChallengeGenerator();
        var runtime = new RuntimeExercise(
            definition,
            generator.Create(definition, 120),
            StartedAt,
            120,
            bpm => generator.Create(definition, bpm));

        runtime.SetSelectedBpm(60);

        Assert.Equal(60, runtime.SelectedBpm);
        Assert.Equal(120, definition.Tempo.DefaultBpm);
        Assert.Equal(1000, runtime.Challenge.ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
    }

    private static RuntimeExercise Runtime(
        InputEventType type = InputEventType.Note,
        TimeSpan? expectedOffset = null)
    {
        var exercise = new NoteReadingExerciseDefinition { Id = "note-01", Range = "C4-G4" };
        return new RuntimeExercise(exercise, Challenge(exercise.Id, type, expectedOffset), StartedAt);
    }

    private static ExerciseChallenge Challenge(
        string exerciseId,
        InputEventType type,
        TimeSpan? expectedOffset) => new()
    {
        ExerciseId = exerciseId,
        ExerciseType = ExerciseType.NoteReading,
        ExpectedInputs =
        [
            new ExpectedInput { Type = type, Value = "C4", ExpectedOffset = expectedOffset }
        ]
    };

    private static ExerciseInputEvent Input(
        string value,
        DateTime timestamp,
        InputEventType type = InputEventType.Note) => new()
    {
        Timestamp = timestamp,
        Source = InputSource.ComputerKeyboard,
        Type = type,
        Value = value
    };
}
