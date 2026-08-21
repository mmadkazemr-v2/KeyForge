namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Determines whether every exercise in a lesson has been successfully completed
/// by deriving completion state from recorded exercise attempts.
/// <para>
/// The evaluator is read-only. It never mutates attempts, progress or
/// lesson definitions. Exercise completion is defined as having at least
/// one successful attempt for each exercise identified by its
/// <see cref="ExerciseDefinition.Id"/>.
/// </para>
/// </summary>
public interface IExerciseCompletionEvaluator
{
    /// <summary>
    /// Returns <c>true</c> when every exercise in <paramref name="exercises"/>
    /// has at least one successful attempt recorded under
    /// <paramref name="lessonId"/>.
    /// </summary>
    /// <param name="lessonId">
    /// The stable identifier of the lesson to evaluate.
    /// </param>
    /// <param name="exercises">
    /// The full list of exercise definitions belonging to the lesson.
    /// </param>
    /// <returns>
    /// <c>true</c> when all exercises are completed; <c>false</c> otherwise.
    /// An empty <paramref name="exercises"/> list is considered vacuously
    /// complete (all zero exercises are satisfied).
    /// </returns>
    bool AreAllExercisesCompleted(string lessonId, IReadOnlyList<ExerciseDefinition> exercises);
}
