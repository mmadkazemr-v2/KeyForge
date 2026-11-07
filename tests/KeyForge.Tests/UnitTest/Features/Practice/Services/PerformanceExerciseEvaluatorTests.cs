namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class PerformanceExerciseEvaluatorTests
{
    [Theory]
    [InlineData(PerformanceTimingReference.FirstAttack, 100)]
    [InlineData(PerformanceTimingReference.WorkspaceStart, 0)]
    public void TimingReferenceIsExplicit(PerformanceTimingReference reference, int score)
    {
        var exercise = new SequenceExerciseDefinition
        {
            Id = "timing",
            Tempo = new TempoConfiguration { DefaultBpm = 60 },
            Performance = new PerformanceDefinition
            {
                TimingReference = reference,
                Events = [new() { Pitch = "C4", DurationBeats = 1 }]
            }
        };
        var challenge = new SequenceChallengeGenerator().Create(exercise);
        var result = new ExerciseEvaluator().Evaluate(challenge, [new ExerciseInputEvent
        { Type = InputEventType.Note, Value = "C4", Source = InputSource.Midi, Timestamp = DateTime.UtcNow, Offset = TimeSpan.FromSeconds(2) }]);
        Assert.Equal(score, result.Score);
    }

    [Fact]
    public void OverlapAndRestAreRepresentedWithoutMutatingDefinition()
    {
        var performance = new PerformanceDefinition { Events = [new() { Pitch = "C3", DurationBeats = 4, Voice = "bass", Hand = PracticeHand.Left }, new() { Pitch = "C4", DurationBeats = 1 }, new() { OnsetBeats = 1, DurationBeats = 1 }] };
        var exercise = new SequenceExerciseDefinition { Id = "overlap", Performance = performance, Tempo = new TempoConfiguration { DefaultBpm = 60 } };
        var challenge = new SequenceChallengeGenerator().Create(exercise);
        Assert.Equal(2, challenge.ExpectedInputs.Count);
        Assert.Equal(3, challenge.Performance!.Events.Count);
        Assert.Equal(TimeSpan.FromSeconds(4), challenge.Performance.Events[0].Duration);
        performance.Events[0].Pitch = "D3";
        Assert.Equal("C3", challenge.Performance.Events[0].Pitch);
    }

    [Fact]
    public void AmbiguousDuplicatePitchAtOneOnsetIsRejected()
    {
        var definition = new PerformanceDefinition { Events = [new() { Pitch = "C4", DurationBeats = 1 }, new() { Pitch = "C4", DurationBeats = 2 }] };
        Assert.Throws<InvalidOperationException>(definition.Validate);
    }
}
