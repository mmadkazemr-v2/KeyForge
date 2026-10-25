namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class IntervalChallengeGeneratorTests
{
    [Fact]
    public void Create_GeneratesDeterministicIntervalQuestionsAndTargets()
    {
        var exercise = new IntervalExerciseDefinition
        {
            Id = "interval-01",
            StartingNote = "C4",
            Intervals = ["M2", "M3"],
            Direction = ExerciseDirection.Both
        };

        var challenge = new IntervalChallengeGenerator().Create(exercise);

        Assert.Equal(NoteReadingChallengeGenerator.DefaultQuestionCount, challenge.ExpectedInputs.Count);
        Assert.Equal(["D4", "G#3", "D4", "G#3", "D4"],
            challenge.ExpectedInputs.Select(input => input.Value));
        Assert.Equal([ExerciseDirection.Ascending, ExerciseDirection.Descending,
            ExerciseDirection.Ascending, ExerciseDirection.Descending, ExerciseDirection.Ascending],
            challenge.Interval!.Questions.Select(question => question.Direction));
        Assert.All(challenge.Interval.Questions, question => Assert.Equal("C4", question.SourceNote));
    }

    [Theory]
    [InlineData("P5", ExerciseDirection.Ascending, "G4")]
    [InlineData("P5", ExerciseDirection.Descending, "F3")]
    public void GetTargetNote_CalculatesConfiguredDirection(
        string interval,
        ExerciseDirection direction,
        string expected)
    {
        var target = IntervalChallengeGenerator.GetTargetNote(MusicalNote.Parse("C4"), interval, direction);

        Assert.Equal(expected, target.Value);
    }

    [Theory]
    [InlineData("P1", "C4")]
    [InlineData("m2", "C#4")]
    [InlineData("M2", "D4")]
    [InlineData("m3", "D#4")]
    [InlineData("M3", "E4")]
    [InlineData("P4", "F4")]
    [InlineData("P5", "G4")]
    [InlineData("m6", "G#4")]
    [InlineData("M6", "A4")]
    [InlineData("m7", "A#4")]
    [InlineData("M7", "B4")]
    [InlineData("P8", "C5")]
    public void GetTargetNote_SupportsMinorMajorAndPerfectAbbreviations(string interval, string expected)
    {
        var target = IntervalChallengeGenerator.GetTargetNote(
            MusicalNote.Parse("C4"), interval, ExerciseDirection.Ascending);

        Assert.Equal(expected, target.Value);
    }

    [Fact]
    public void IsSupportedInterval_DistinguishesMinorFromMajorByCase()
    {
        Assert.True(IntervalChallengeGenerator.IsSupportedInterval("m3"));
        Assert.True(IntervalChallengeGenerator.IsSupportedInterval("M3"));
        Assert.False(IntervalChallengeGenerator.IsSupportedInterval("aug4"));
        Assert.False(IntervalChallengeGenerator.IsSupportedInterval(null));
        Assert.Throws<InvalidOperationException>(() =>
            IntervalChallengeGenerator.GetTargetNote(MusicalNote.Parse("C4"), "aug4", ExerciseDirection.Ascending));
    }

    [Fact]
    public void ResolveDirection_AlternatesOnlyWhenConfiguredForBoth()
    {
        Assert.Equal([ExerciseDirection.Ascending, ExerciseDirection.Descending,
                ExerciseDirection.Ascending],
            Enumerable.Range(0, 3).Select(index =>
                IntervalChallengeGenerator.ResolveDirection(ExerciseDirection.Both, index)));
        Assert.Equal(ExerciseDirection.Descending,
            IntervalChallengeGenerator.ResolveDirection(ExerciseDirection.Descending, 1));
    }

    [Fact]
    public void Create_MissingIntervals_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new IntervalChallengeGenerator().Create(new()
        {
            Id = "interval-invalid", StartingNote = "C4"
        }));
    }
}
