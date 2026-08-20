namespace KeyForge.Features.Practice.Models;

/// <summary>
/// Outcome of submitting an exercise attempt during a practice session.
/// <para>
/// Contains the evaluation result and final score produced by the
/// existing evaluation and scoring pipeline. This is a pure data
/// model; no logic lives here.
/// </para>
/// </summary>
public sealed class SessionResult
{
    /// <summary>
    /// The evaluation outcome produced by <see cref="IExerciseEvaluator"/>.
    /// </summary>
    public required ExerciseEvaluationResult Evaluation { get; init; }

    /// <summary>
    /// Final numeric score (0..100) produced by <see cref="IExerciseScorer"/>.
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Whether the attempt met the exercise requirements.
    /// Convenience property; equivalent to <c>Evaluation.IsSuccessful</c>.
    /// </summary>
    public bool IsSuccessful => Evaluation.IsSuccessful;
}
