namespace KeyForge.Tests.UnitTest.Infrastructure.Yaml.Parsing;

public sealed class BeginnerPracticePackTests
{
    [Fact]
    public void Lesson04_CompilesBeginnerPracticePackWithTimingAndPitchChallenges()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Content", "Lessons", "Lesson04.yaml");
        var lesson = new YamlLessonParser().Parse(File.ReadAllText(path));

        Assert.Equal(5, lesson.Exercises.Count);
        Assert.Equal(ExerciseType.Rhythm, lesson.Exercises[0].Type);
        Assert.Equal(ExerciseType.Rhythm, lesson.Exercises[1].Type);
        Assert.Equal(ExerciseType.Rhythm, lesson.Exercises[2].Type);
        Assert.Equal(ExerciseType.NoteReading, lesson.Exercises[3].Type);
        Assert.Equal(ExerciseType.Speed, lesson.Exercises[4].Type);

        var generator = new ExerciseChallengeGenerator();
        var steady = generator.Create(lesson.Exercises[0]);
        Assert.Equal(8, steady.ExpectedInputs.Count);
        Assert.Equal(Enumerable.Range(0, 8).Select(index => index * 1000d),
            steady.ExpectedInputs.Select(input => input.ExpectedOffset!.Value.TotalMilliseconds));

        var mixed = generator.Create(lesson.Exercises[1]);
        Assert.Equal([0d, 2000d, 3000d, 4000d, 8000d, 9000d, 11000d],
            mixed.ExpectedInputs.Select(input => input.ExpectedOffset!.Value.TotalMilliseconds));

        var rests = generator.Create(lesson.Exercises[2]);
        Assert.Equal(3, rests.ExpectedInputs.Count);
        Assert.True(rests.Rhythm!.Events[1].IsRest);

        var notes = generator.Create(lesson.Exercises[3]);
        Assert.Equal(["C4", "D4", "E4", "D4", "C4", "E4"], notes.ExpectedInputs.Select(input => input.Value));
        Assert.All(notes.ExpectedInputs, input => Assert.Null(input.ExpectedOffset));

        var melody = generator.Create(lesson.Exercises[4]);
        Assert.Equal(["C4", "E4", "D4", "C4"], melody.ExpectedInputs.Select(input => input.Value));
        Assert.Equal([0d, 1000d, 2000d, 3000d], melody.ExpectedInputs.Select(input => input.ExpectedOffset!.Value.TotalMilliseconds));
    }
}
