namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class RhythmChallengeGeneratorTests
{
    [Theory]
    [InlineData(120, 500)]
    [InlineData(80, 750)]
    [InlineData(60, 1000)]
    public void CalculateBeatInterval_ConvertsBpmToMilliseconds(int bpm, double expectedMilliseconds)
    {
        var interval = RhythmChallengeGenerator.CalculateBeatInterval(bpm);

        Assert.Equal(expectedMilliseconds, interval.TotalMilliseconds, precision: 6);
    }

    [Fact]
    public void Create_GeneratesExpectedBeatsFromMeterAndPatternCount()
    {
        var exercise = Rhythm(tempo: 120, timeSignature: "4/4", patternCount: 2);

        var challenge = new RhythmChallengeGenerator().Create(exercise);

        Assert.Equal(8, challenge.ExpectedInputs.Count);
        Assert.All(challenge.ExpectedInputs, input => Assert.Equal(InputEventType.Beat, input.Type));
        Assert.Equal(0, challenge.ExpectedInputs[0].ExpectedOffset!.Value.TotalMilliseconds);
        Assert.Equal(500, challenge.ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
        Assert.Equal(3500, challenge.ExpectedInputs[7].ExpectedOffset!.Value.TotalMilliseconds);
    }

    [Fact]
    public void Create_UsesTimeSignatureNumeratorAsPulseCount()
    {
        var challenge = new RhythmChallengeGenerator().Create(
            Rhythm(tempo: 120, timeSignature: "6/8", patternCount: 1));

        Assert.Equal(6, challenge.ExpectedInputs.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateBeatInterval_InvalidBpm_ThrowsArgumentOutOfRangeException(int bpm)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RhythmChallengeGenerator.CalculateBeatInterval(bpm));
    }

    [Theory]
    [InlineData("four-four")]
    [InlineData("0/4")]
    [InlineData("4/0")]
    public void GetBeatsPerPattern_InvalidMeter_ThrowsInvalidOperationException(string meter)
    {
        Assert.Throws<InvalidOperationException>(() =>
            RhythmChallengeGenerator.GetBeatsPerPattern(meter));
    }

    [Fact]
    public void GetBeatsPerPattern_EmptyMeter_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            RhythmChallengeGenerator.GetBeatsPerPattern(string.Empty));
    }

    [Fact]
    public void Create_InvalidPatternCount_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new RhythmChallengeGenerator().Create(
                Rhythm(tempo: 120, timeSignature: "4/4", patternCount: 0)));
    }

    private static RhythmExerciseDefinition Rhythm(
        int tempo,
        string timeSignature,
        int patternCount) => new()
    {
        Id = "rhythm-01",
        Tempo = tempo,
        TimeSignature = timeSignature,
        PatternCount = patternCount
    };
}
