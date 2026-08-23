namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class FingerIndependenceChallengeGeneratorTests
{
    [Fact]
    public void Create_MapsConfiguredFingersHandAndTempo()
    {
        var exercise = new FingerIndependenceExerciseDefinition
        {
            Id = "finger-01",
            Hand = PracticeHand.Left,
            Fingers = [2, 3, 4],
            Tempo = new TempoConfiguration { DefaultBpm = 60 }
        };

        var challenge = new FingerIndependenceChallengeGenerator().Create(exercise, selectedBpm: 120);

        Assert.Equal(["D4", "E4", "F4"], challenge.FingerIndependence!.ExpectedNotes);
        Assert.Equal(PracticeHand.Left, challenge.FingerIndependence.Hand);
        Assert.Equal(500, challenge.ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
    }

    [Fact]
    public void Create_EmptyFingers_UsesAllFiveInstructionalNotes()
    {
        var challenge = new FingerIndependenceChallengeGenerator().Create(new FingerIndependenceExerciseDefinition { Id = "finger-01" }, null);

        Assert.Equal([1, 2, 3, 4, 5], challenge.FingerIndependence!.Fingers);
        Assert.Equal(["C4", "D4", "E4", "F4", "G4"], challenge.FingerIndependence.ExpectedNotes);
        Assert.All(challenge.ExpectedInputs, input => Assert.Null(input.ExpectedOffset));
    }

    [Fact]
    public void Create_InvalidFinger_FailsClearly()
    {
        var exercise = new FingerIndependenceExerciseDefinition { Id = "finger-01", Fingers = [0] };
        Assert.Throws<InvalidOperationException>(() => new FingerIndependenceChallengeGenerator().Create(exercise, null));
    }
}
