namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class NoteReadingExerciseEvaluatorTests
{
    private static readonly DateTime StartedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evaluate_AllCorrect_GeneratesSuccessfulFullScore()
    {
        var result = new NoteReadingExerciseEvaluator().Evaluate(
            Challenge("C4", "D4", "E4"),
            [Input("C4"), Input("D4"), Input("E4")]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(3, result.NoteReading!.CorrectAnswerCount);
        Assert.Equal(0, result.NoteReading.IncorrectAnswerCount);
    }

    [Fact]
    public void Evaluate_WrongAndMissingAnswers_GenerateAutomaticPartialScore()
    {
        var result = new NoteReadingExerciseEvaluator().Evaluate(
            Challenge("C4", "D4", "E4", "F4", "G4"),
            [Input("C4"), Input("D4"), Input("F4")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(40, result.Score);
        Assert.Equal(2, result.NoteReading!.CorrectAnswerCount);
        Assert.Equal(3, result.NoteReading.IncorrectAnswerCount);
    }

    [Theory]
    [InlineData(InputSource.ComputerKeyboard)]
    [InlineData(InputSource.VirtualPiano)]
    [InlineData(InputSource.Midi)]
    public void Evaluate_CorrectNote_DoesNotDependOnInputSource(InputSource source)
    {
        var result = new NoteReadingExerciseEvaluator().Evaluate(
            Challenge("F#4"),
            [Input("F#4", source)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void IsCorrect_PreservesOctave()
    {
        var expected = new ExpectedInput { Type = InputEventType.Note, Value = "C4" };

        Assert.False(NoteReadingExerciseEvaluator.IsCorrect(expected, Input("C5")));
    }

    private static ExerciseChallenge Challenge(params string[] notes) => new()
    {
        ExerciseId = "note-01",
        ExerciseType = ExerciseType.NoteReading,
        ExpectedInputs = notes.Select(note => new ExpectedInput
        {
            Type = InputEventType.Note,
            Value = note
        }).ToArray()
    };

    private static ExerciseInputEvent Input(
        string value,
        InputSource source = InputSource.VirtualPiano) => new()
    {
        Timestamp = StartedAt,
        Source = source,
        Type = InputEventType.Note,
        Value = value
    };
}
