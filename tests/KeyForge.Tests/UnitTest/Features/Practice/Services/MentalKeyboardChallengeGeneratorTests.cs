namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class MentalKeyboardChallengeGeneratorTests
{
    [Fact]
    public void Create_GeneratesDeterministicMultipleNavigationQuestions()
    {
        var challenge = new MentalKeyboardChallengeGenerator().Create(new MentalKeyboardExerciseDefinition
        {
            Id = "mental-01", StartingNote = "C4", Steps = 4, Direction = ExerciseDirection.Ascending
        });

        Assert.Equal(NoteReadingChallengeGenerator.DefaultQuestionCount, challenge.ExpectedInputs.Count);
        Assert.Equal(["E4", "G#4", "C5", "E5", "G#5"],
            challenge.ExpectedInputs.Select(input => input.Value));
        Assert.Equal(["C4", "E4", "G#4", "C5", "E5"],
            challenge.MentalKeyboard!.Questions.Select(question => question.SourceNote));
        Assert.All(challenge.MentalKeyboard.Questions, question => Assert.Equal(4, question.Steps));
    }

    [Theory]
    [InlineData(ExerciseDirection.Ascending, "E4")]
    [InlineData(ExerciseDirection.Descending, "G#3")]
    public void GetTargetNote_NavigatesChromaticKeyboardPositions(
        ExerciseDirection direction,
        string expected)
    {
        var target = MentalKeyboardChallengeGenerator.GetTargetNote(MusicalNote.Parse("C4"), 4, direction);

        Assert.Equal(expected, target.Value);
    }

    [Fact]
    public void Create_InvalidConfiguration_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new MentalKeyboardChallengeGenerator().Create(new()
        {
            Id = "mental-invalid", StartingNote = "", Steps = 0
        }));
    }
}
