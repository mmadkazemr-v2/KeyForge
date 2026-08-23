namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class EarTrainingChallengeGeneratorTests
{
    [Fact]
    public void Create_PitchMatching_GeneratesConfiguredNumberOfDeterministicNotes()
    {
        var exercise = PitchMatching(rounds: 5, keys: ["C major"]);
        var generator = new EarTrainingChallengeGenerator();

        var first = generator.Create(exercise);
        var second = generator.Create(exercise);

        Assert.Equal(5, first.ExpectedInputs.Count);
        Assert.Equal(["C4", "D4", "E4", "F4", "G4"], first.ExpectedInputs.Select(input => input.Value));
        Assert.Equal(first.ExpectedInputs.Select(input => input.Value), second.ExpectedInputs.Select(input => input.Value));
        Assert.Equal(EarTrainingTask.PitchMatching, first.EarTraining!.TaskType);
    }

    [Fact]
    public void Create_PitchMatching_UsesAllConfiguredKeys()
    {
        var challenge = new EarTrainingChallengeGenerator().Create(
            PitchMatching(rounds: 14, keys: ["C major", "G major"]));

        Assert.Contains("C4", challenge.EarTraining!.AvailableNotes);
        Assert.Contains("G4", challenge.EarTraining.AvailableNotes);
        Assert.Contains("F#5", challenge.EarTraining.AvailableNotes);
        Assert.All(challenge.ExpectedInputs, input => Assert.Contains(input.Value, challenge.EarTraining.AvailableNotes));
    }

    [Fact]
    public void Create_EmptyKeys_UsesUnconstrainedChromaticOctave()
    {
        var challenge = new EarTrainingChallengeGenerator().Create(PitchMatching(12, []));

        Assert.Equal(12, challenge.EarTraining!.AvailableNotes.Count);
        Assert.Contains("C#4", challenge.EarTraining.AvailableNotes);
    }

    [Fact]
    public void Create_UnsupportedTask_ReturnsSafeUnsupportedChallenge()
    {
        var challenge = new EarTrainingChallengeGenerator().Create(new EarTrainingExerciseDefinition
        {
            Id = "ear-01",
            TaskType = EarTrainingTask.ChordRecognition,
            Rounds = 4
        });

        Assert.Empty(challenge.ExpectedInputs);
        Assert.Equal(EarTrainingTask.ChordRecognition, challenge.EarTraining!.TaskType);
    }

    [Fact]
    public void Create_InvalidRoundsOrKey_FailsClearly()
    {
        Assert.Throws<InvalidOperationException>(() => new EarTrainingChallengeGenerator().Create(PitchMatching(0, ["C major"])));
        Assert.Throws<InvalidOperationException>(() => new EarTrainingChallengeGenerator().Create(PitchMatching(1, ["not-a-key"])));
    }

    private static EarTrainingExerciseDefinition PitchMatching(int rounds, List<string> keys) => new()
    {
        Id = "ear-01",
        TaskType = EarTrainingTask.PitchMatching,
        Rounds = rounds,
        Keys = keys
    };
}
