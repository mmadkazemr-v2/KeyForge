namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Produces a normalized numeric score (0..100) from an exercise evaluation result.
/// <para>
/// The scorer is intentionally stateless and read-only. It never mutates
/// the evaluation result. Scoring logic lives here rather than inside
/// domain models.
/// </para>
/// </summary>
public interface IExerciseScorer
{
    /// <summary>
    /// Returns a score in the range 0..100 based on the evaluation result.
    /// </summary>
    /// <param name="evaluationResult">
    /// The evaluation outcome produced by <see cref="IExerciseEvaluator"/>.
    /// Must not be <c>null</c>.
    /// </param>
    /// <returns>A numeric score clamped to the range 0..100.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="evaluationResult"/> is <c>null</c>.
    /// </exception>
    int Score(ExerciseEvaluationResult evaluationResult);
}
