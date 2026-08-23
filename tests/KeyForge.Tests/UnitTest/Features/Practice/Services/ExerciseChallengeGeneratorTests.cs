namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class ExerciseChallengeGeneratorTests
{
    [Fact]
    public void Create_NoteReading_GeneratesMultipleNotesInsideRange()
    {
        var exercise = NoteReading("C4-G5");

        var challenge = new ExerciseChallengeGenerator().Create(exercise);

        Assert.Equal(exercise.Id, challenge.ExerciseId);
        Assert.Equal(ExerciseType.NoteReading, challenge.ExerciseType);
        Assert.Equal(NoteReadingChallengeGenerator.DefaultQuestionCount, challenge.ExpectedInputs.Count);
        Assert.All(challenge.ExpectedInputs, expected => Assert.Equal(InputEventType.Note, expected.Type));
        Assert.Equal("C4", challenge.ExpectedInputs[0].Value);
        Assert.Equal("G5", challenge.ExpectedInputs[^1].Value);
    }

    [Fact]
    public void Create_NoteReading_TrimsRangeValues()
    {
        var challenge = new ExerciseChallengeGenerator().Create(NoteReading(" F3 - C5 "));

        Assert.Equal("F3", challenge.ExpectedInputs[0].Value);
    }

    [Fact]
    public void Create_EarTrainingPitchMatching_GeneratesRuntimeChallenge()
    {
        var exercise = new EarTrainingExerciseDefinition { Id = "ear-01" };

        var challenge = new ExerciseChallengeGenerator().Create(exercise);

        Assert.Equal(exercise.Id, challenge.ExerciseId);
        Assert.Equal(ExerciseType.EarTraining, challenge.ExerciseType);
        Assert.NotEmpty(challenge.ExpectedInputs);
        Assert.Equal(EarTrainingTask.PitchMatching, challenge.EarTraining!.TaskType);
    }

    [Fact]
    public void Create_InvalidNoteRange_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ExerciseChallengeGenerator().Create(NoteReading(string.Empty)));
    }

    [Fact]
    public void Create_NullExercise_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ExerciseChallengeGenerator().Create(null!));
    }

    private static NoteReadingExerciseDefinition NoteReading(string range) => new()
    {
        Id = "note-01",
        Range = range
    };
}
