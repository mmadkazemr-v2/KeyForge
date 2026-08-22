namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class NoteReadingChallengeGeneratorTests
{
    [Fact]
    public void Create_GeneratesDeterministicQuestionsInsideConfiguredRange()
    {
        var exercise = NoteReading("C4-G5");
        var generator = new NoteReadingChallengeGenerator();

        var first = generator.Create(exercise);
        var second = generator.Create(exercise);
        var range = NoteReadingChallengeGenerator.ParseRange(exercise)
            .Select(note => note.Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(NoteReadingChallengeGenerator.DefaultQuestionCount, first.ExpectedInputs.Count);
        Assert.Equal(
            first.ExpectedInputs.Select(expected => expected.Value),
            second.ExpectedInputs.Select(expected => expected.Value));
        Assert.All(first.ExpectedInputs, expected => Assert.Contains(expected.Value, range));
    }

    [Fact]
    public void Create_ShortRange_RepeatsValidNotesAcrossQuestions()
    {
        var challenge = new NoteReadingChallengeGenerator().Create(NoteReading("C4-D4"));

        Assert.Equal(["C4", "D4", "C4", "D4", "C4"],
            challenge.ExpectedInputs.Select(expected => expected.Value));
    }

    [Theory]
    [InlineData("C4")]
    [InlineData("G5-C4")]
    [InlineData("C#4-G5")]
    public void Create_InvalidOrUnsupportedRange_Throws(string range)
    {
        Assert.Throws<InvalidOperationException>(() =>
            new NoteReadingChallengeGenerator().Create(NoteReading(range)));
    }

    private static NoteReadingExerciseDefinition NoteReading(string range) => new()
    {
        Id = "note-01",
        Range = range
    };
}
