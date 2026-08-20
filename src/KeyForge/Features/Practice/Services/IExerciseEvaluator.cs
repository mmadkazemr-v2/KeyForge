namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Evaluates a single exercise attempt against its definition to determine
/// whether the attempt was successful and what score was achieved.
/// <para>
/// The service is intentionally stateless and read-only. It never mutates
/// the exercise definition or the attempt. Evaluation logic lives here
/// rather than inside domain models.
/// </para>
/// </summary>
public interface IExerciseEvaluator
{
    /// <summary>
    /// Evaluates an exercise attempt against its definition.
    /// </summary>
    /// <param name="exercise">
    /// The exercise definition that was attempted. Must not be <c>null</c>.
    /// </param>
    /// <param name="attempt">
    /// The recorded attempt data. Must not be <c>null</c>.
    /// </param>
    /// <returns>
    /// An <see cref="ExerciseEvaluationResult"/> containing the evaluation outcome.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exercise"/> or <paramref name="attempt"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="attempt"/> does not belong to <paramref name="exercise"/>.
    /// </exception>
    ExerciseEvaluationResult Evaluate(ExerciseDefinition exercise, ExerciseAttempt attempt);
}
