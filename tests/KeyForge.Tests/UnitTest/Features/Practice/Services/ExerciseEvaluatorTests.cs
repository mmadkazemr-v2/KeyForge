namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class ExerciseEvaluatorTests
{
    private static readonly DateTime StartedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evaluate_CorrectNote_GeneratesSuccessfulFullScore()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("C4"), [Input("C4")]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(ExerciseEvaluationFeedback.Correct, result.Feedback);
        Assert.Equal("C4", result.ExpectedValue);
        Assert.Equal("C4", result.ActualValue);
    }

    [Fact]
    public void Evaluate_IncorrectNote_GeneratesFailedZeroScore()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("C4"), [Input("D4")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.Equal(ExerciseEvaluationFeedback.Incorrect, result.Feedback);
    }

    [Fact]
    public void Evaluate_IsCaseInsensitiveAndTrimsInput()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("F#3"), [Input("  f#3 ")]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Evaluate_WrongInputType_FailsEvenWhenValueMatches()
    {
        var result = new ExerciseEvaluator().Evaluate(
            Challenge("C4"),
            [Input("C4", type: InputEventType.Key)]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Evaluate_NoInput_ReturnsUsefulFailure()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("C4"), []);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.Equal(ExerciseEvaluationFeedback.NoInput, result.Feedback);
        Assert.Equal("C4", result.ExpectedValue);
        Assert.Null(result.ActualValue);
    }

    [Fact]
    public void Evaluate_EmptyChallenge_ReturnsUnsupportedFailure()
    {
        var challenge = new ExerciseChallenge
        {
            ExerciseId = "ex-01",
            ExerciseType = ExerciseType.Rhythm,
            ExpectedInputs = []
        };

        var result = new ExerciseEvaluator().Evaluate(challenge, [Input("beat")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.Equal(ExerciseEvaluationFeedback.UnsupportedChallenge, result.Feedback);
    }

    [Theory]
    [InlineData(InputSource.ComputerKeyboard)]
    [InlineData(InputSource.VirtualPiano)]
    [InlineData(InputSource.Midi)]
    public void Evaluate_DoesNotDependOnInputSource(InputSource source)
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge("C4"), [Input("C4", source)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Evaluate_UsesFirstCapturedEventForMinimalChallenge()
    {
        var result = new ExerciseEvaluator().Evaluate(
            Challenge("C4"),
            [Input("D4"), Input("C4")]);

        Assert.False(result.IsSuccessful);
    }

    [Fact]
    public void Evaluate_NullChallenge_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ExerciseEvaluator().Evaluate(null!, []));
    }

    [Fact]
    public void Evaluate_NullInputCollection_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ExerciseEvaluator().Evaluate(Challenge("C4"), null!));
    }

    [Fact]
    public void ExerciseEvaluator_ImplementsContract()
    {
        IExerciseEvaluator evaluator = new ExerciseEvaluator();

        Assert.IsType<ExerciseEvaluator>(evaluator);
    }

    private static ExerciseChallenge Challenge(string note) => new()
    {
        ExerciseId = "ex-01",
        ExerciseType = ExerciseType.NoteReading,
        ExpectedInputs =
        [
            new ExpectedInput
            {
                Type = InputEventType.Note,
                Value = note
            }
        ]
    };

    private static ExerciseInputEvent Input(
        string value,
        InputSource source = InputSource.ComputerKeyboard,
        InputEventType type = InputEventType.Note) => new()
    {
        Timestamp = StartedAt,
        Source = source,
        Type = type,
        Value = value
    };
}
