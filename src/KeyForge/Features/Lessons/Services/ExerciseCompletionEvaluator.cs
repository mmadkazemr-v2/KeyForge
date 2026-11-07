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

        var successfulIds = GetCompletedExerciseIds(lessonId, exercises);
        return exercises.All(e => successfulIds.Contains(e.Id));
    }

    /// <inheritdoc />
    public IReadOnlySet<string> GetCompletedExerciseIds(string lessonId, IReadOnlyList<ExerciseDefinition> exercises)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        ArgumentNullException.ThrowIfNull(exercises);

        var registeredIds = exercises
            .Select(exercise => exercise.Id)
            .ToHashSet(StringComparer.Ordinal);

        return _recorder.GetAttemptsByLesson(lessonId)
            .Where(attempt => IsCompletedSuccess(attempt) && registeredIds.Contains(attempt.ExerciseId))
            .Select(attempt => attempt.ExerciseId)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public bool HasSuccessfulAttempt(string lessonId, int? minimumScore = null)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        return _recorder.GetAttemptsByLesson(lessonId).Any(attempt =>
            IsCompletedSuccess(attempt) &&
            (minimumScore is null || attempt.Score >= minimumScore));
    }

    private static bool IsCompletedSuccess(ExerciseAttempt attempt) =>
        attempt is { IsSuccessful: true, CompletedAt: not null, Score: not null };
}
