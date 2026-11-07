namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class PerformanceDynamicsTests
{
    [Theory]
    [InlineData(20, 80, 50, true)]
    [InlineData(60, 120, 90, true)]
    [InlineData(20, 80, 80, false)]
    [InlineData(20, 80, null, false)]
    public void DynamicsAreRelativeToExplicitKeyboardCalibration(int soft, int loud, int? velocity, bool correct)
    {
        var definition = new PerformanceDefinition { VelocityCalibration = new VelocityCalibrationDefinition { Soft = soft, Loud = loud, Tolerance = 0.1 }, Events = [new() { Pitch = "C4", DurationBeats = 1, RelativeVelocity = 0.5 }] };
        var challenge = new SequenceChallengeGenerator().Create(new SequenceExerciseDefinition { Id = "dynamics", Tempo = new TempoConfiguration { DefaultBpm = 60 }, Performance = definition });
        var input = new ExerciseInputEvent { Type = InputEventType.Note, Value = "C4", Source = InputSource.Midi, MidiKind = MidiEventKind.NoteOn, Timestamp = DateTime.UtcNow, Velocity = velocity };
        var result = new ExerciseEvaluator().Evaluate(challenge, [input]);
        Assert.Equal(correct, result.Performance!.Notes[0].IsVelocityCorrect);
        Assert.Equal(correct ? 100 : 0, result.Score);
        definition.VelocityCalibration.Loud = soft;
        Assert.Throws<InvalidOperationException>(definition.Validate);
    }

    [Fact]
    public void VelocityTargetsRequireCalibrationAndCannotBeAttachedToRests()
    {
        var definition = new PerformanceDefinition { Events = [new() { Pitch = "C4", DurationBeats = 1, RelativeVelocity = 0.5 }] };
        Assert.Throws<InvalidOperationException>(definition.Validate);
        definition.VelocityCalibration = new VelocityCalibrationDefinition { Soft = 30, Loud = 90 };
        definition.Events[0].RelativeVelocity = 2;
        Assert.Throws<InvalidOperationException>(definition.Validate);
    }
}
