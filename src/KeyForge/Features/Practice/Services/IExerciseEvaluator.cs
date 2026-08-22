namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Evaluates normalized runtime input against a generated challenge.
/// <para>
/// The service is intentionally stateless and read-only. It never mutates
/// the challenge or captured events. Evaluation logic lives here rather than
/// inside input adapters or presentation components.
/// </para>
/// </summary>
public interface IExerciseEvaluator
{
    /// <summary>
    /// Evaluates captured input events against a runtime challenge.
    /// </summary>
    /// <param name="challenge">The generated runtime challenge.</param>
    /// <param name="inputEvents">Normalized input events captured by the runtime.</param>
    /// <returns>
    /// An <see cref="ExerciseEvaluationResult"/> containing the evaluation outcome.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="challenge"/> or <paramref name="inputEvents"/> is <c>null</c>.
    /// </exception>
    ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents);
}
