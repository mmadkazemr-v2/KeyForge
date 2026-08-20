namespace KeyForge.Features.Practice.Models;

/// <summary>
/// The outcome of evaluating a single exercise attempt against its definition.
/// <para>
/// This is a pure data model produced by <see cref="Services.IExerciseEvaluator"/>.
/// It contains only the information necessary for the current evaluation step.
/// </para>
/// </summary>
public sealed class ExerciseEvaluationResult
{
    /// <summary>
    /// Whether the attempt met the minimum requirements to be
    /// considered successful for this exercise type.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Numeric score achieved on this attempt, derived from the
    /// attempt data and exercise definition.
    /// <c>null</c> when the score could not be determined
    /// (e.g. the attempt was incomplete).
    /// </summary>
    public int? Score { get; init; }
}
