namespace KeyForge.Tests.UnitTest.Features.Lessons;

public class LessonProgressionModelTests
{
    [Fact]
    public void LessonDefinition_Default_UnlocksImmediately()
    {
        var lesson = new LessonDefinition();

        Assert.NotNull(lesson.Unlock);
        Assert.Equal(UnlockMode.Immediate, lesson.Unlock.Mode);
        Assert.Empty(lesson.Unlock.RequiredLessonIds);
    }

    [Fact]
    public void LessonDefinition_Default_HasCompletionRuleWithoutRequirements()
    {
        var lesson = new LessonDefinition();

        Assert.NotNull(lesson.Completion);
        Assert.Null(lesson.Completion.MinimumScore);
        Assert.False(lesson.Completion.RequireAllExercises);
    }

    [Fact]
    public void UnlockRule_RepresentsImmediateUnlock()
    {
        var lesson = new LessonDefinition
        {
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
        };

        Assert.Equal(UnlockMode.Immediate, lesson.Unlock.Mode);
        Assert.Empty(lesson.Unlock.RequiredLessonIds);
    }

    [Fact]
    public void UnlockRule_RepresentsPreviousLessonUnlock()
    {
        var lesson = new LessonDefinition
        {
            Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
        };

        Assert.Equal(UnlockMode.PreviousLessonCompleted, lesson.Unlock.Mode);
        Assert.Empty(lesson.Unlock.RequiredLessonIds);
    }

    [Fact]
    public void UnlockRule_RepresentsMultiplePrerequisites()
    {
        var lesson = new LessonDefinition
        {
            Unlock = new UnlockRule
            {
                Mode = UnlockMode.PrerequisitesCompleted,
                RequiredLessonIds = ["lesson-01", "lesson-02"]
            }
        };

        Assert.Equal(UnlockMode.PrerequisitesCompleted, lesson.Unlock.Mode);
        Assert.Equal(new[] { "lesson-01", "lesson-02" }, lesson.Unlock.RequiredLessonIds);
    }

    [Fact]
    public void CompletionRule_RepresentsMinimumScore()
    {
        var lesson = new LessonDefinition
        {
            Completion = new CompletionRule { MinimumScore = 70 }
        };

        var score = lesson.Completion.MinimumScore;
        Assert.NotNull(score);
        Assert.Equal(70, score.Value);
        Assert.False(lesson.Completion.RequireAllExercises);
    }

    [Fact]
    public void CompletionRule_RepresentsRequireAllExercises()
    {
        var lesson = new LessonDefinition
        {
            Completion = new CompletionRule { RequireAllExercises = true }
        };

        Assert.True(lesson.Completion.RequireAllExercises);
        Assert.Null(lesson.Completion.MinimumScore);
    }

    [Fact]
    public void CompletionRule_RepresentsScoreAndAllExercisesTogether()
    {
        var lesson = new LessonDefinition
        {
            Completion = new CompletionRule { MinimumScore = 70, RequireAllExercises = true }
        };

        var score = lesson.Completion.MinimumScore;
        Assert.NotNull(score);
        Assert.Equal(70, score.Value);
        Assert.True(lesson.Completion.RequireAllExercises);
    }

    [Fact]
    public void LessonIdentity_IsData()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-02",
            Order = 2
        };

        Assert.Equal("lesson-02", lesson.Id);
        Assert.Equal(2, lesson.Order);
    }

    [Fact]
    public void LessonDefinition_Default_HasEmptyExercises()
    {
        var lesson = new LessonDefinition();

        Assert.NotNull(lesson.Exercises);
        Assert.Empty(lesson.Exercises);
    }

    [Fact]
    public void LessonDefinitions_HaveIndependentProgressionRules()
    {
        var lesson1 = new LessonDefinition();
        var lesson2 = new LessonDefinition();

        lesson1.Unlock.Mode = UnlockMode.PreviousLessonCompleted;
        lesson1.Completion.MinimumScore = 80;

        Assert.Equal(
            UnlockMode.Immediate,
            lesson2.Unlock.Mode);

        Assert.Null(
            lesson2.Completion.MinimumScore);
    }
}