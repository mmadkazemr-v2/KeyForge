namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class RhythmExerciseEvaluatorTests
{
    private static readonly DateTime Timestamp = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evaluate_OnTimeHit_IsExcellentAndScoresOneHundred()
    {
        var result = Evaluate([500], [Hit(500)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
        Assert.Equal(RhythmHitAccuracy.Excellent, Assert.Single(result.Rhythm!.Beats).Accuracy);
        Assert.Equal(0, result.Rhythm.AverageTimingErrorMilliseconds);
    }

    [Fact]
    public void Evaluate_EarlyHit_PreservesNegativeTimingError()
    {
        var result = Evaluate([500], [Hit(440)]);

        var beat = Assert.Single(result.Rhythm!.Beats);
        Assert.Equal(-60, beat.TimingError!.Value.TotalMilliseconds);
        Assert.Equal(RhythmHitAccuracy.Good, beat.Accuracy);
        Assert.Equal(RhythmTimingTendency.Early, result.Rhythm.TimingTendency);
    }

    [Fact]
    public void Evaluate_LateHit_PreservesPositiveTimingError()
    {
        var result = Evaluate([500], [Hit(560)]);

        var beat = Assert.Single(result.Rhythm!.Beats);
        Assert.Equal(60, beat.TimingError!.Value.TotalMilliseconds);
        Assert.Equal(RhythmHitAccuracy.Good, beat.Accuracy);
        Assert.Equal(RhythmTimingTendency.Late, result.Rhythm.TimingTendency);
    }

    [Theory]
    [InlineData(40, RhythmHitAccuracy.Excellent, 100)]
    [InlineData(40.1, RhythmHitAccuracy.Good, 80)]
    [InlineData(80, RhythmHitAccuracy.Good, 80)]
    [InlineData(80.1, RhythmHitAccuracy.Acceptable, 60)]
    [InlineData(120, RhythmHitAccuracy.Acceptable, 60)]
    [InlineData(120.1, RhythmHitAccuracy.Miss, 0)]
    public void Evaluate_AppliesToleranceBoundaries(
        double errorMilliseconds,
        RhythmHitAccuracy expectedAccuracy,
        int expectedScore)
    {
        var result = Evaluate([500], [Hit(500 + errorMilliseconds)]);

        Assert.Equal(expectedAccuracy, Assert.Single(result.Rhythm!.Beats).Accuracy);
        Assert.Equal(expectedScore, result.Score);
    }

    [Fact]
    public void Evaluate_MissedBeat_IsReportedAndScoresZero()
    {
        var result = Evaluate([0], []);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Score);
        Assert.Equal(1, result.Rhythm!.MissedBeatCount);
        Assert.Equal(0, result.Rhythm.SuccessfulHitCount);
        Assert.Null(result.Rhythm.AverageTimingErrorMilliseconds);
    }

    [Fact]
    public void Evaluate_ExtraHit_IsReportedAndPenalized()
    {
        var result = Evaluate([0], [Hit(0), Hit(300)]);

        Assert.Equal(95, result.Score);
        Assert.Equal(1, result.Rhythm!.SuccessfulHitCount);
        Assert.Equal(1, result.Rhythm.ExtraHitCount);
    }

    [Fact]
    public void Evaluate_MultipleBeats_MatchesNearestHitsDeterministically()
    {
        var result = Evaluate(
            [0, 500, 1000],
            [Hit(0), Hit(550), Hit(1100)]);

        Assert.Equal(80, result.Score);
        Assert.Equal(3, result.Rhythm!.SuccessfulHitCount);
        Assert.Equal(0, result.Rhythm.MissedBeatCount);
        Assert.Equal([0d, 50d, 100d], result.Rhythm.Beats.Select(
            beat => beat.TimingError!.Value.TotalMilliseconds));
    }

    [Fact]
    public void Evaluate_ManyExtraHits_NeverProducesNegativeScore()
    {
        var inputs = Enumerable.Range(0, 30)
            .Select(index => Hit(500 + index * 200))
            .ToList();

        var result = Evaluate([0], inputs);

        Assert.InRange(result.Score!.Value, 0, 100);
        Assert.Equal(0, result.Score);
    }

    [Theory]
    [InlineData(InputSource.ComputerKeyboard)]
    [InlineData(InputSource.VirtualPiano)]
    [InlineData(InputSource.Midi)]
    public void Evaluate_DoesNotDependOnInputSource(InputSource source)
    {
        var result = Evaluate([0], [Hit(0, source)]);

        Assert.True(result.IsSuccessful);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Evaluate_IgnoresNonBeatInputEvents()
    {
        var result = new RhythmExerciseEvaluator().Evaluate(
            Challenge([0]),
            [Hit(0, type: InputEventType.Note)]);

        Assert.Equal(0, result.Score);
        Assert.Equal(1, result.Rhythm!.MissedBeatCount);
        Assert.Equal(0, result.Rhythm.ExtraHitCount);
    }

    private static ExerciseEvaluationResult Evaluate(
        IReadOnlyList<double> expectedMilliseconds,
        IReadOnlyList<ExerciseInputEvent> inputs) =>
        new RhythmExerciseEvaluator().Evaluate(Challenge(expectedMilliseconds), inputs);

    private static ExerciseChallenge Challenge(IReadOnlyList<double> expectedMilliseconds) => new()
    {
        ExerciseId = "rhythm-01",
        ExerciseType = ExerciseType.Rhythm,
        ExpectedInputs = expectedMilliseconds.Select(milliseconds => new ExpectedInput
        {
            Type = InputEventType.Beat,
            Value = "beat",
            ExpectedOffset = TimeSpan.FromMilliseconds(milliseconds)
        }).ToList()
    };

    private static ExerciseInputEvent Hit(
        double milliseconds,
        InputSource source = InputSource.ComputerKeyboard,
        InputEventType type = InputEventType.Beat) => new()
    {
        Timestamp = Timestamp.AddMilliseconds(milliseconds),
        Offset = TimeSpan.FromMilliseconds(milliseconds),
        Source = source,
        Type = type,
        Value = "beat"
    };
}
