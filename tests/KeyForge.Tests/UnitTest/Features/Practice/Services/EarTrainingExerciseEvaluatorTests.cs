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
    public void Evaluate_CorrectIntervalChoices_GeneratesFullAutomaticScore()
    {
        var result = new EarTrainingExerciseEvaluator().Evaluate(
            IntervalChallenge("M3", "P5"),
            [ChoiceInput("M3"), ChoiceInput("P5")]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(2, result.EarTraining!.CorrectAnswerCount);
    }

    [Fact]
    public void Evaluate_IncorrectIntervalChoice_GeneratesFailureAndAutomaticScore()
    {
        var result = new EarTrainingExerciseEvaluator().Evaluate(
            IntervalChallenge("M3", "m2", "P5"),
            [ChoiceInput("M3"), ChoiceInput("M2"), ChoiceInput("P4")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(33, result.Score);
        Assert.Equal(1, result.EarTraining!.CorrectAnswerCount);
    }

    [Fact]
    public void Evaluate_IntervalAnswers_AreCaseSensitiveBetweenMinorAndMajor()
    {
        var result = new EarTrainingExerciseEvaluator().Evaluate(
            IntervalChallenge("M3"),
            [ChoiceInput("m3")]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Evaluate_IntervalAnswer_MustBeChoiceTyped()
    {
        var noteTypedAnswer = new ExerciseInputEvent
        {
            Timestamp = StartedAt,
            Source = InputSource.VirtualPiano,
            Type = InputEventType.Note,
            Value = "M3"
        };

        var result = new EarTrainingExerciseEvaluator().Evaluate(
            IntervalChallenge("M3"), [noteTypedAnswer]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
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

    private static ExerciseChallenge IntervalChallenge(params string[] intervals) => new()
    {
        ExerciseId = "ear-interval-01",
        ExerciseType = ExerciseType.EarTraining,
        ExpectedInputs = intervals.Select(interval => new ExpectedInput { Type = InputEventType.Choice, Value = interval }).ToArray(),
        EarTraining = new EarTrainingChallengeDetails
        {
            TaskType = EarTrainingTask.IntervalRecognition,
            AvailableNotes = [],
            IntervalChoices = intervals
        }
    };

    private static ExerciseInputEvent Input(string value, InputSource source = InputSource.VirtualPiano) => new()
    {
        Timestamp = StartedAt,
        Source = source,
        Type = InputEventType.Note,
        Value = value
    };

    private static ExerciseInputEvent ChoiceInput(string value, InputSource source = InputSource.ComputerKeyboard) => new()
    {
        Timestamp = StartedAt,
        Source = source,
        Type = InputEventType.Choice,
        Value = value
    };
}
