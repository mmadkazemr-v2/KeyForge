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

    [Fact]
    public void Create_IntervalRecognition_GeneratesDeterministicQuestionsAndChoices()
    {
        var generator = new EarTrainingChallengeGenerator();
        var exercise = IntervalRecognition(rounds: 4, intervals: ["M3", "P5"]);

        var first = generator.Create(exercise);
        var second = generator.Create(exercise);

        Assert.Equal(4, first.ExpectedInputs.Count);
        Assert.All(first.ExpectedInputs, input => Assert.Equal(InputEventType.Choice, input.Type));
        Assert.Equal(["M3", "P5", "M3", "P5"], first.ExpectedInputs.Select(input => input.Value));
        Assert.Equal(
            first.ExpectedInputs.Select(input => input.Value),
            second.ExpectedInputs.Select(input => input.Value));
        Assert.Equal(["M3", "P5"], first.EarTraining!.IntervalChoices);
        Assert.Equal(EarTrainingTask.IntervalRecognition, first.EarTraining.TaskType);
    }

    [Fact]
    public void Create_IntervalRecognition_CalculatesAscendingTargetsFromValidSources()
    {
        var challenge = new EarTrainingChallengeGenerator().Create(
            IntervalRecognition(rounds: 3, intervals: ["M3"], direction: ExerciseDirection.Ascending));

        var questions = challenge.EarTraining!.IntervalQuestions;
        Assert.Equal(3, questions.Count);
        foreach (var question in questions)
        {
            var source = MusicalNote.Parse(question.SourceNote);
            var target = MusicalNote.Parse(question.TargetNote);
            Assert.True(Enum.IsDefined(question.Direction));
            Assert.InRange(source.MidiNumber, 0, 127);
            Assert.InRange(target.MidiNumber, 0, 127);
            Assert.Equal(source.MidiNumber + 4, target.MidiNumber);
        }
    }

    [Fact]
    public void Create_IntervalRecognition_DescendingDirection_ProducesDescendingTargets()
    {
        var challenge = new EarTrainingChallengeGenerator().Create(
            IntervalRecognition(rounds: 3, intervals: ["P5"], direction: ExerciseDirection.Descending));

        Assert.All(challenge.EarTraining!.IntervalQuestions, question =>
        {
            Assert.Equal(ExerciseDirection.Descending, question.Direction);
            Assert.True(MusicalNote.Parse(question.TargetNote).MidiNumber <
                MusicalNote.Parse(question.SourceNote).MidiNumber);
        });
    }

    [Fact]
    public void Create_IntervalRecognition_BothDirections_AlternatesPerRound()
    {
        var challenge = new EarTrainingChallengeGenerator().Create(
            IntervalRecognition(rounds: 4, intervals: ["P8"], direction: ExerciseDirection.Both));

        Assert.Equal([ExerciseDirection.Ascending, ExerciseDirection.Descending,
                ExerciseDirection.Ascending, ExerciseDirection.Descending],
            challenge.EarTraining!.IntervalQuestions.Select(question => question.Direction));
    }

    [Fact]
    public void Create_IntervalRecognition_EmptyIntervals_UsesFullDefaultChoiceSet()
    {
        var challenge = new EarTrainingChallengeGenerator().Create(
            IntervalRecognition(rounds: 2, intervals: []));

        Assert.Equal(EarTrainingChallengeGenerator.DefaultIntervals, challenge.EarTraining!.IntervalChoices);
        Assert.Equal(12, challenge.EarTraining.IntervalChoices.Count);
    }

    [Theory]
    [InlineData("m2", 1)]
    [InlineData("m3", 3)]
    [InlineData("m6", 8)]
    [InlineData("m7", 10)]
    public void Create_IntervalRecognition_SupportsMinorIntervalAbbreviations(string interval, int semitones)
    {
        var challenge = new EarTrainingChallengeGenerator().Create(
            IntervalRecognition(rounds: 1, intervals: [interval]));

        var question = Assert.Single(challenge.EarTraining!.IntervalQuestions);
        Assert.Equal(interval, question.Interval);
        Assert.Equal(semitones,
            Math.Abs(MusicalNote.Parse(question.TargetNote).MidiNumber -
                MusicalNote.Parse(question.SourceNote).MidiNumber));
    }

    [Fact]
    public void Create_IntervalRecognition_GeneratedNotesRemainWithinConfiguredSourcePoolAndPianoRange()
    {
        var exercise = IntervalRecognition(rounds: 20, intervals: ["m7"]);
        var sources = EarTrainingChallengeGenerator.GetAvailableNotes(exercise)
            .Select(note => note.Value).ToHashSet();

        var challenge = new EarTrainingChallengeGenerator().Create(exercise);

        Assert.All(challenge.EarTraining!.IntervalQuestions, question =>
        {
            Assert.Contains(question.SourceNote, sources);
            Assert.InRange(MusicalNote.Parse(question.TargetNote).MidiNumber, 0, 127);
        });
        Assert.Equal(challenge.EarTraining.AvailableNotes,
            challenge.EarTraining.IntervalQuestions
                .SelectMany(question => new[] { question.SourceNote, question.TargetNote })
                .Distinct());
    }

    [Fact]
    public void Create_IntervalRecognition_InvalidRoundsOrInterval_FailsClearly()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new EarTrainingChallengeGenerator().Create(IntervalRecognition(0, ["M3"])));
        Assert.Throws<InvalidOperationException>(() =>
            new EarTrainingChallengeGenerator().Create(IntervalRecognition(2, ["aug4"])));
    }

    private static EarTrainingExerciseDefinition PitchMatching(int rounds, List<string> keys) => new()
    {
        Id = "ear-01",
        TaskType = EarTrainingTask.PitchMatching,
        Rounds = rounds,
        Keys = keys
    };

    private static EarTrainingExerciseDefinition IntervalRecognition(
        int rounds,
        List<string> intervals,
        ExerciseDirection direction = ExerciseDirection.Ascending) => new()
    {
        Id = "ear-interval-01",
        TaskType = EarTrainingTask.IntervalRecognition,
        Rounds = rounds,
        Intervals = intervals,
        Direction = direction
    };
}
