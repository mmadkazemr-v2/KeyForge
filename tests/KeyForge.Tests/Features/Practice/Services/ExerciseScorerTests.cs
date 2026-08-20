namespace KeyForge.Tests.Features.Practice.Services;

/// <summary>
/// Tests <see cref="ExerciseScorer"/> to verify scoring behaviour
/// without touching files or MIDI.
/// </summary>
public sealed class ExerciseScorerTests
{
    private static ExerciseScorer CreateScorer() => new();

    [Fact]
    public void Score_NullScore_ReturnsZero()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = false, Score = null };

        var score = scorer.Score(result);

        Assert.Equal(0, score);
    }

    [Fact]
    public void Score_NormalScore_ReturnsSameValue()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = true, Score = 75 };

        var score = scorer.Score(result);

        Assert.Equal(75, score);
    }

    [Fact]
    public void Score_BelowZero_ClampsToZero()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = true, Score = -10 };

        var score = scorer.Score(result);

        Assert.Equal(0, score);
    }

    [Fact]
    public void Score_AboveHundred_ClampsToHundred()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = true, Score = 150 };

        var score = scorer.Score(result);

        Assert.Equal(100, score);
    }

    [Fact]
    public void Score_ExactlyZero_ReturnsZero()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = false, Score = 0 };

        var score = scorer.Score(result);

        Assert.Equal(0, score);
    }

    [Fact]
    public void Score_ExactlyHundred_ReturnsHundred()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = true, Score = 100 };

        var score = scorer.Score(result);

        Assert.Equal(100, score);
    }

    [Fact]
    public void Score_DoesNotMutateInput()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = true, Score = 85 };

        scorer.Score(result);

        Assert.True(result.IsSuccessful);
        Assert.Equal(85, result.Score);
    }

    [Fact]
    public void Score_IsDeterministic()
    {
        var scorer = CreateScorer();
        var result = new ExerciseEvaluationResult { IsSuccessful = true, Score = 60 };

        var first = scorer.Score(result);
        var second = scorer.Score(result);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Score_NullResult_ThrowsArgumentNullException()
    {
        var scorer = CreateScorer();

        Assert.Throws<ArgumentNullException>(() => scorer.Score(null!));
    }

    [Fact]
    public void ExerciseScorer_ImplementsIExerciseScorer()
    {
        IExerciseScorer scorer = new ExerciseScorer();

        Assert.NotNull(scorer);
    }
}
