namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class SequenceTranspositionTests
{
    [Theory]
    [InlineData(2, "D4,F#4,A4")]
    [InlineData(-2, "A#3,D4,F4")]
    [InlineData(12, "C5,E5,G5")]
    public void ExplicitSemitonesPreserveSource(int semitones, string expected)
    {
        var source = new[] { "C4", "E4", "G4" };
        Assert.Equal(expected.Split(','), PitchSequenceBuilder.Transpose(source, semitones));
        Assert.Equal(new[] { "C4", "E4", "G4" }, source);
    }
    [Fact]
    public void PerformanceTransposesPitchesWithoutMovingTimeline()
    {
        var exercise = new SequenceExerciseDefinition { Id = "transpose", TransposeSemitones = 2, Tempo = new TempoConfiguration { DefaultBpm = 60 }, Performance = new PerformanceDefinition { Events = [new() { Pitch = "C4", OnsetBeats = 2, DurationBeats = 3 }] } };
        var challenge = new SequenceChallengeGenerator().Create(exercise);
        Assert.Equal("D4", challenge.Performance!.Events[0].Pitch);
        Assert.Equal(TimeSpan.FromSeconds(2), challenge.Performance.Events[0].Onset);
        Assert.Equal(TimeSpan.FromSeconds(3), challenge.Performance.Events[0].Duration);
        Assert.Equal("C4", exercise.Performance.Events[0].Pitch);
        Assert.Throws<InvalidOperationException>(() => PitchSequenceBuilder.Transpose(["G9"], 2));
        Assert.Throws<InvalidOperationException>(() => PitchSequenceBuilder.Transpose(["C4"], 49));
    }
}
