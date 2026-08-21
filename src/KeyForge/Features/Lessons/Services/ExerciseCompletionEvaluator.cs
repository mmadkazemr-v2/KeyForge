namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Derives exercise completion state from recorded attempts stored in
/// <see cref="IExerciseAttemptRecorder"/>.
/// <para>
/// An exercise is considered completed when at least one successful attempt
/// exists for it. Multiple attempts (successful or not) do not undo a prior
/// successful completion. Completion is based on exercise identity, not
/// attempt count.
/// </para>
/// </summary>
public sealed class ExerciseCompletionEvaluator : IExerciseCompletionEvaluator
{
    private readonly IExerciseAttemptRecorder _recorder;

    /// <summary>
    /// Creates a new <see cref="ExerciseCompletionEvaluator"/>.
    /// </summary>
    public ExerciseCompletionEvaluator(IExerciseAttemptRecorder recorder)
    {
        ArgumentNullException.ThrowIfNull(recorder);
        _recorder = recorder;
    }

    /// <inheritdoc />
    public bool AreAllExercisesCompleted(string lessonId, IReadOnlyList<ExerciseDefinition> exercises)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        ArgumentNullException.ThrowIfNull(exercises);

        if (exercises.Count == 0)
        {
            return true;
        }

        var attempts = _recorder.GetAttemptsByLesson(lessonId);

        var successfulIds = new HashSet<string>(
            attempts
                .Where(a => a.IsSuccessful)
                .Select(a => a.ExerciseId),
            StringComparer.Ordinal);

        return exercises.All(e => successfulIds.Contains(e.Id));
    }
}
