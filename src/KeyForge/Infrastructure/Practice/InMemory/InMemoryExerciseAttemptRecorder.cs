namespace KeyForge.Infrastructure.Practice.InMemory;

/// <summary>
/// A simple in-memory exercise attempt recorder backed by a concurrent queue.
/// <para>
/// Data lives only for the lifetime of the application process and is lost on
/// restart. Replace this with a persistent implementation without changing
/// any consumer code.
/// </para>
/// </summary>
public sealed class InMemoryExerciseAttemptRecorder : IExerciseAttemptRecorder
{
    private readonly ConcurrentQueue<ExerciseAttempt> _attempts = new();

    /// <inheritdoc />
    public void Record(ExerciseAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        _attempts.Enqueue(attempt);
    }

    /// <inheritdoc />
    public IReadOnlyList<ExerciseAttempt> GetAttemptsByLesson(string lessonId) =>
        [.. _attempts.Where(a => string.Equals(a.LessonId, lessonId, StringComparison.Ordinal))];

    /// <summary>
    /// Returns all recorded attempts in insertion order.
    /// Intended for testing and diagnostics only.
    /// </summary>
    public IReadOnlyList<ExerciseAttempt> GetAllAttempts() =>
        [.. _attempts];
}