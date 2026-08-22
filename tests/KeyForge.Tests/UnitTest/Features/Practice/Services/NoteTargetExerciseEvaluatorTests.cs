namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class NoteTargetExerciseEvaluatorTests
{
    private static readonly DateTime Timestamp = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evaluate_IntervalCorrectAnswers_GeneratesAutomaticFullScore()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge(ExerciseType.Interval, "D4", "E4"),
            [Input("D4", InputSource.VirtualPiano), Input("E4", InputSource.ComputerKeyboard)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(2, result.Interval!.CorrectAnswerCount);
    }

    [Fact]
    public void Evaluate_OctaveWrongOctave_FailsEvenWhenPitchClassMatches()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge(ExerciseType.Octave, "C5"), [Input("C4")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.Single(result.Octave!.Questions);
        Assert.False(result.Octave.Questions[0].IsCorrect);
    }

    [Theory]
    [InlineData(InputSource.ComputerKeyboard)]
    [InlineData(InputSource.VirtualPiano)]
    [InlineData(InputSource.Midi)]
    public void Evaluate_TargetNote_DoesNotDependOnInputSource(InputSource source)
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge(ExerciseType.Octave, "G5"), [Input("G5", source)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    private static ExerciseChallenge Challenge(ExerciseType type, params string[] expected) => new()
    {
        ExerciseId = "target-01",
        ExerciseType = type,
        ExpectedInputs = expected.Select(note => new ExpectedInput { Type = InputEventType.Note, Value = note }).ToArray()
    };

    private static ExerciseInputEvent Input(string note, InputSource source = InputSource.VirtualPiano) => new()
    {
        Timestamp = Timestamp,
        Source = source,
        Type = InputEventType.Note,
        Value = note
    };
}
