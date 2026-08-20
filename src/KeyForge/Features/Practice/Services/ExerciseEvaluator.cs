namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Evaluates an exercise attempt against its definition using the data
/// currently available in the domain models.
/// <para>
/// The evaluation rule for this first version is:
/// <list type="number">
///   <item>The attempt must belong to the given exercise (ID match).</item>
///   <item>The attempt must be completed (<see cref="ExerciseAttempt.CompletedAt"/> has a value).</item>
///   <item>The attempt must have a score (<see cref="ExerciseAttempt.Score"/> has a value).</item>
///   <item>If all conditions are met, the attempt is considered successful.</item>
/// </list>
/// This is the smallest generic rule consistent with the current model.
/// Future versions may add exercise-type-specific evaluation logic.
/// </para>
/// </summary>
public sealed class ExerciseEvaluator : IExerciseEvaluator
{
    /// <inheritdoc />
    public ExerciseEvaluationResult Evaluate(ExerciseDefinition exercise, ExerciseAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(attempt);

        if (!string.Equals(exercise.Id, attempt.ExerciseId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Attempt ExerciseId '{attempt.ExerciseId}' does not match exercise Id '{exercise.Id}'.",
                nameof(attempt));
        }

        if (attempt.CompletedAt is null || attempt.Score is null)
        {
            return new ExerciseEvaluationResult { IsSuccessful = false, Score = null };
        }

        return new ExerciseEvaluationResult
        {
            IsSuccessful = true,
            Score = attempt.Score
        };
    }
}
