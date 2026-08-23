namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class MentalKeyboardExerciseEvaluatorTests
{
    private static readonly DateTime Timestamp = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evaluate_CorrectSelections_GeneratesAutomaticScore()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("E4", "G#4"), [Input("E4"), Input("G#4")]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(2, result.MentalKeyboard!.CorrectAnswerCount);
    }

    [Fact]
    public void Evaluate_WrongPitchAndWrongOctave_Fail()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("C5", "E4"), [Input("D5"), Input("E5")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.Equal(2, result.MentalKeyboard!.IncorrectAnswerCount);
    }

    [Theory]
    [InlineData(InputSource.ComputerKeyboard)]
    [InlineData(InputSource.VirtualPiano)]
    [InlineData(InputSource.Midi)]
    public void Evaluate_CorrectKeyboardPosition_DoesNotDependOnInputSource(InputSource source)
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("F#4"), [Input("F#4", source)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    private static ExerciseChallenge Challenge(params string[] expected) => new()
    {
        ExerciseId = "mental-01",
        ExerciseType = ExerciseType.MentalKeyboard,
        ExpectedInputs = expected.Select(note => new ExpectedInput { Type = InputEventType.Note, Value = note }).ToArray()
    };

    private static ExerciseInputEvent Input(string value, InputSource source = InputSource.VirtualPiano) => new()
    {
        Timestamp = Timestamp,
        Source = source,
        Type = InputEventType.Note,
        Value = value
    };
}
