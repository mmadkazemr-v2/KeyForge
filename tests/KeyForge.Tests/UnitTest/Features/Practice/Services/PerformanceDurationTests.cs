namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class PerformanceDurationTests
{
    [Theory]
    [InlineData(1000, PerformanceReleaseAccuracy.Correct)]
    [InlineData(700, PerformanceReleaseAccuracy.Early)]
    [InlineData(1300, PerformanceReleaseAccuracy.Late)]
    [InlineData(-1, PerformanceReleaseAccuracy.Missing)]
    public void KeyHoldFeedbackDistinguishesReleaseErrors(int releaseMs, PerformanceReleaseAccuracy accuracy)
    {
        var challenge = Challenge();
        var input = new List<ExerciseInputEvent> { Input("C4", 0, true) };
        if (releaseMs >= 0) input.Add(Input("C4", releaseMs, false));
        var result = new ExerciseEvaluator().Evaluate(challenge, input);
        Assert.Equal(accuracy, result.Performance!.Notes[0].ReleaseAccuracy);
        Assert.Equal(accuracy == PerformanceReleaseAccuracy.Correct ? 100 : 0, result.Score);
    }

    [Fact]
    public void PairingUsesDeviceChannelAndFifoForRepeatedKeys()
    {
        var challenge = Challenge();
        var inputs = new[] { Input("C4", 0, true), Input("C4", 100, true), Input("C4", 1000, false), Input("C4", 1100, false), Input("D4", 1200, false) };
        var result = new ExerciseEvaluator().Evaluate(challenge, inputs);
        Assert.Equal(PerformanceReleaseAccuracy.Correct, result.Performance!.Notes[0].ReleaseAccuracy);
        Assert.Equal(1, result.Performance.ExtraNotes);
        Assert.Equal(1, result.Performance.UnexpectedReleases);
        var wrongChannel = Input("C4", 1000, false, 2);
        Assert.Equal(PerformanceReleaseAccuracy.Missing, new ExerciseEvaluator().Evaluate(challenge, [inputs[0], wrongChannel]).Performance!.Notes[0].ReleaseAccuracy);
    }

    [Fact]
    public void OnsetOnlyRemainsSuccessfulWithoutNoteOff()
    {
        var result = new ExerciseEvaluator().Evaluate(Challenge(PerformanceScoringMode.OnsetOnly), [Input("C4", 0, true)]);
        Assert.Equal(100, result.Score);
        Assert.Equal(PerformanceReleaseAccuracy.NotScored, result.Performance!.Notes[0].ReleaseAccuracy);
    }

    private static ExerciseChallenge Challenge(PerformanceScoringMode mode = PerformanceScoringMode.KeyHold) => new SequenceChallengeGenerator().Create(new SequenceExerciseDefinition
    {
        Id = "hold",
        Tempo = new TempoConfiguration { DefaultBpm = 60 },
        Performance = new PerformanceDefinition { ScoringMode = mode, Events = [new() { Pitch = "C4", DurationBeats = 1 }] }
    });

    internal static ExerciseInputEvent Input(string pitch, int ms, bool on, int channel = 1) => new()
    {
        Value = pitch,
        Offset = TimeSpan.FromMilliseconds(ms),
        Timestamp = DateTime.UtcNow,
        Source = InputSource.Midi,
        Type = on ? InputEventType.Note : InputEventType.NoteOff,
        MidiKind = on ? MidiEventKind.NoteOn : MidiEventKind.NoteOff,
        DeviceId = "keys",
        MidiChannel = channel
    };
}
