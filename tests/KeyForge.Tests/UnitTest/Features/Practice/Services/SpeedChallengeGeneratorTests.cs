namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class SpeedChallengeGeneratorTests
{
    [Fact]
    public void Create_UsesPatternRepetitionsAndSelectedBpm()
    {
        var exercise = Speed(SpeedPattern.Scale, repetitions: 2);

        var challenge = new SpeedChallengeGenerator().Create(exercise, selectedBpm: 60);

        Assert.Equal(["C4", "D4", "E4", "F4", "G4", "C4", "D4", "E4", "F4", "G4"], challenge.Speed!.ExpectedNotes);
        Assert.Equal(1000, challenge.ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
    }

    [Theory]
    [InlineData(SpeedPattern.Arpeggio, "C4", "C5")]
    [InlineData(SpeedPattern.RepeatedNote, "C4", "C4")]
    [InlineData(SpeedPattern.Chromatic, "C4", "E4")]
    public void GetPatternNotes_MapsSupportedPatterns(SpeedPattern pattern, string first, string last)
    {
        var notes = SpeedChallengeGenerator.GetPatternNotes(pattern);

        Assert.Equal(first, notes[0]);
        Assert.Equal(last, notes[^1]);
    }

    [Fact]
    public void Create_InvalidRepetitions_FailsClearly()
    {
        Assert.Throws<InvalidOperationException>(() => new SpeedChallengeGenerator().Create(Speed(SpeedPattern.Scale, 0), 80));
    }

    private static SpeedExerciseDefinition Speed(SpeedPattern pattern, int repetitions) => new()
    {
        Id = "speed-01",
        Pattern = pattern,
        Repetitions = repetitions,
        Tempo = new TempoConfiguration { DefaultBpm = 80 }
    };
}
