namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Produces a normalized numeric score (0..100) from an exercise evaluation result.
/// <para>
/// Scoring rule:
/// <list type="number">
///   <item>If the evaluation result has no score, return 0.</item>
///   <item>Otherwise, clamp the score to the range 0..100.</item>
/// </list>
/// This is a deterministic, stateless transformation.
/// Future versions may add exercise-type-specific scoring algorithms.
/// </para>
/// </summary>
public sealed class ExerciseScorer : IExerciseScorer
{
    private const int MinScore = 0;
    private const int MaxScore = 100;

    /// <inheritdoc />
    public int Score(ExerciseEvaluationResult evaluationResult)
    {
        ArgumentNullException.ThrowIfNull(evaluationResult);

        return evaluationResult.Score is null
            ? MinScore
            : Math.Clamp(evaluationResult.Score.Value, MinScore, MaxScore);
    }
}