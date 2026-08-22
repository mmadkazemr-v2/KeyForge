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

    [Fact]
    public void Create_MissingIntervals_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new IntervalChallengeGenerator().Create(new()
        {
            Id = "interval-invalid", StartingNote = "C4"
        }));
    }
}
