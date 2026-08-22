namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class OctaveChallengeGeneratorTests
{
    [Fact]
    public void Create_GeneratesMultipleOctaveAwareQuestions()
    {
        var challenge = new OctaveChallengeGenerator().Create(new OctaveExerciseDefinition
        {
            Id = "octave-01",
            StartingNote = "C3",
            Octaves = 2,
            Direction = ExerciseDirection.Both
        });

        Assert.Equal(NoteReadingChallengeGenerator.DefaultQuestionCount, challenge.ExpectedInputs.Count);
        Assert.Equal(["C5", "C1", "C5", "C1", "C5"],
            challenge.ExpectedInputs.Select(input => input.Value));
        Assert.Equal([ExerciseDirection.Ascending, ExerciseDirection.Descending,
            ExerciseDirection.Ascending, ExerciseDirection.Descending, ExerciseDirection.Ascending],
            challenge.Octave!.Questions.Select(question => question.Direction));
    }

    [Fact]
    public void GetTargetNote_PreservesPitchClassAndChangesExactOctave()
    {
        var target = OctaveChallengeGenerator.GetTargetNote(
            MusicalNote.Parse("D3"), 2, ExerciseDirection.Ascending);

        Assert.Equal("D5", target.Value);
    }

    [Fact]
    public void Create_NonPositiveOctaves_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new OctaveChallengeGenerator().Create(new()
        {
            Id = "octave-invalid", StartingNote = "C3", Octaves = 0
        }));
    }
}
