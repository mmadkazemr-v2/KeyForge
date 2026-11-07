namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class PerformancePedalTests
{
    [Theory]
    [InlineData(PerformanceScoringMode.PedalSustain, 1000, 100)]
    [InlineData(PerformanceScoringMode.KeyHold, 1000, 0)]
    [InlineData(PerformanceScoringMode.OnsetOnly, -1, 100)]
    [InlineData(PerformanceScoringMode.PedalSustain, -1, 0)]
    [InlineData(PerformanceScoringMode.PedalSustain, 1500, 0)]
    public void SustainChangesReleaseOnlyWhenExplicitlyEnabled(PerformanceScoringMode mode, int pedalUp, int score)
    {
        var challenge = new SequenceChallengeGenerator().Create(new SequenceExerciseDefinition
        { Id = "pedal", Tempo = new TempoConfiguration { DefaultBpm = 60 }, Performance = new PerformanceDefinition { ScoringMode = mode, Events = [new() { Pitch = "C4", DurationBeats = 1 }] } });
        var inputs = new List<ExerciseInputEvent> { Pedal(0, true), PerformanceDurationTests.Input("C4", 0, true), PerformanceDurationTests.Input("C4", 500, false) };
        if (pedalUp >= 0) inputs.Add(Pedal(pedalUp, false));
        Assert.Equal(score, new ExerciseEvaluator().Evaluate(challenge, inputs).Score);
    }
    internal static ExerciseInputEvent Pedal(int ms, bool down) => new()
    { Type = InputEventType.ControlChange, Value = down ? "64:127" : "64:0", MidiKind = MidiEventKind.ControlChange, MidiData1 = 64, MidiData2 = down ? 127 : 0, Source = InputSource.Midi, Timestamp = DateTime.UtcNow, Offset = TimeSpan.FromMilliseconds(ms), DeviceId = "keys", MidiChannel = 1 };
}
