namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class EarTrainingExerciseEvaluatorTests
{
    private static readonly DateTime StartedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evaluate_CorrectPitchAnswers_GeneratesFullAutomaticScore()
    {
        var result = new EarTrainingExerciseEvaluator().Evaluate(Challenge("C4", "E4"), [Input("C4"), Input("E4")]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(2, result.EarTraining!.CorrectAnswerCount);
    }

    [Fact]
    public void Evaluate_WrongPitchAnswers_GeneratesFailureAndAutomaticScore()
    {
        var result = new EarTrainingExerciseEvaluator().Evaluate(Challenge("C4", "E4", "G4"), [Input("C4"), Input("D4"), Input("A4")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(33, result.Score);
        Assert.Equal(1, result.EarTraining!.CorrectAnswerCount);
    }

    [Theory]
    [InlineData(InputSource.ComputerKeyboard)]
    [InlineData(InputSource.VirtualPiano)]
    [InlineData(InputSource.Midi)]
    public void Evaluate_CorrectNote_DoesNotDependOnInputSource(InputSource source)
    {
        var result = new EarTrainingExerciseEvaluator().Evaluate(Challenge("F#4"), [Input("F#4", source)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Evaluate_UnsupportedChallenge_IsSafe()
    {
        var challenge = new ExerciseChallenge
        {
            ExerciseId = "ear-01",
            ExerciseType = ExerciseType.EarTraining,
            ExpectedInputs = [],
            EarTraining = new EarTrainingChallengeDetails { TaskType = EarTrainingTask.ChordRecognition, AvailableNotes = [] }
        };

        var result = new EarTrainingExerciseEvaluator().Evaluate(challenge, []);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.Equal(ExerciseEvaluationFeedback.UnsupportedChallenge, result.Feedback);
    }

    private static ExerciseChallenge Challenge(params string[] notes) => new()
    {
        ExerciseId = "ear-01",
        ExerciseType = ExerciseType.EarTraining,
        ExpectedInputs = notes.Select(note => new ExpectedInput { Type = InputEventType.Note, Value = note }).ToArray(),
        EarTraining = new EarTrainingChallengeDetails { TaskType = EarTrainingTask.PitchMatching, AvailableNotes = notes }
    };

    private static ExerciseInputEvent Input(string value, InputSource source = InputSource.VirtualPiano) => new()
    {
        Timestamp = StartedAt,
        Source = source,
        Type = InputEventType.Note,
        Value = value
    };
}
