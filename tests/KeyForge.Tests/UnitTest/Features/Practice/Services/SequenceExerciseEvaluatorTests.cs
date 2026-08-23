namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class SequenceExerciseEvaluatorTests
{
    private static readonly DateTime StartedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evaluate_SpeedCorrectAndOnTime_GeneratesFullScore()
    {
        var result = new SequenceExerciseEvaluator().Evaluate(Challenge(ExerciseType.Speed, timing: true, "C4", "D4"),
            [Input("C4", 0), Input("D4", 500)], ExerciseType.Speed, requireTiming: true);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(2, result.Speed!.CorrectInputCount);
    }

    [Fact]
    public void Evaluate_SpeedWrongOrderMissingAndExtra_AutomaticallyScores()
    {
        var result = new SequenceExerciseEvaluator().Evaluate(Challenge(ExerciseType.Speed, timing: true, "C4", "D4", "E4"),
            [Input("D4", 0), Input("C4", 500), Input("E4", 1000), Input("E4", 1500)], ExerciseType.Speed, requireTiming: true);

        Assert.False(result.IsSuccessful);
        Assert.Equal(28, result.Score);
        Assert.Equal(1, result.Speed!.CorrectInputCount);
        Assert.Equal(1, result.Speed.ExtraInputCount);
    }

    [Fact]
    public void Evaluate_FingerSequence_MissingInput_IsReportedAndScoredAutomatically()
    {
        var result = new SequenceExerciseEvaluator().Evaluate(Challenge(ExerciseType.FingerIndependence, timing: false, "C4", "D4", "E4"),
            [Input("C4", 0)], ExerciseType.FingerIndependence, requireTiming: false);

        Assert.False(result.IsSuccessful);
        Assert.Equal(33, result.Score);
        Assert.Equal(2, result.FingerIndependence!.MissedInputCount);
    }

    [Theory]
    [InlineData(-120, true)]
    [InlineData(120, true)]
    [InlineData(121, false)]
    public void Evaluate_SpeedTimingTolerance_IsDeterministic(int timingError, bool successful)
    {
        var result = new SequenceExerciseEvaluator().Evaluate(Challenge(ExerciseType.Speed, timing: true, "C4"),
            [Input("C4", timingError)], ExerciseType.Speed, requireTiming: true);

        Assert.Equal(successful, result.IsSuccessful);
        Assert.Equal(successful ? 100 : 0, result.Score);
    }

    [Theory]
    [InlineData(-80, RhythmTimingTendency.Early)]
    [InlineData(80, RhythmTimingTendency.Late)]
    public void Evaluate_SpeedTimingFeedback_ReportsEarlyAndLateHits(int timingError, RhythmTimingTendency expectedTendency)
    {
        var result = new SequenceExerciseEvaluator().Evaluate(Challenge(ExerciseType.Speed, timing: true, "C4"),
            [Input("C4", timingError)], ExerciseType.Speed, requireTiming: true);

        Assert.Equal(expectedTendency, result.Speed!.TimingTendency);
    }

    [Theory]
    [InlineData(InputSource.ComputerKeyboard)]
    [InlineData(InputSource.VirtualPiano)]
    [InlineData(InputSource.Midi)]
    public void Evaluate_FingerSequence_CorrectOrder_IsInputSourceIndependent(InputSource source)
    {
        var result = new SequenceExerciseEvaluator().Evaluate(Challenge(ExerciseType.FingerIndependence, timing: false, "D4", "E4"),
            [Input("D4", 0, source), Input("E4", 300, source)], ExerciseType.FingerIndependence, requireTiming: false);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(2, result.FingerIndependence!.CorrectInputCount);
    }

    private static ExerciseChallenge Challenge(ExerciseType type, bool timing, params string[] notes) => new()
    {
        ExerciseId = "sequence-01",
        ExerciseType = type,
        ExpectedInputs = notes.Select((note, index) => new ExpectedInput
        {
            Type = InputEventType.Note,
            Value = note,
            ExpectedOffset = timing ? TimeSpan.FromMilliseconds(index * 500) : null
        }).ToArray(),
        Speed = type == ExerciseType.Speed ? new SpeedChallengeDetails { Pattern = SpeedPattern.Scale, ExpectedNotes = notes } : null,
        FingerIndependence = type == ExerciseType.FingerIndependence ? new FingerIndependenceChallengeDetails { Hand = PracticeHand.Right, Fingers = [1, 2], ExpectedNotes = notes } : null
    };

    private static ExerciseInputEvent Input(string value, int offset, InputSource source = InputSource.VirtualPiano) => new()
    {
        Timestamp = StartedAt.AddMilliseconds(offset),
        Offset = TimeSpan.FromMilliseconds(offset),
        Source = source,
        Type = InputEventType.Note,
        Value = value
    };
}
