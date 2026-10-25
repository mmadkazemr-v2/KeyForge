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
    private readonly ConcurrentDictionary<Guid, byte> _recordedAttemptIds = new();
    private readonly object _sync = new();

    /// <inheritdoc />
    public void Record(ExerciseAttempt attempt) => RecordIfNew(attempt);

    /// <inheritdoc />
    public bool RecordIfNew(ExerciseAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        lock (_sync)
        {
            if (!_recordedAttemptIds.TryAdd(attempt.Id, 0))
            {
                return false;
            }

            _attempts.Enqueue(attempt);
            return true;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<ExerciseAttempt> GetAttemptsByLesson(string lessonId)
    {
        lock (_sync)
        {
            return [.. _attempts.Where(a => string.Equals(a.LessonId, lessonId, StringComparison.Ordinal))];
        }
    }

    /// <summary>
    /// Returns all recorded attempts in insertion order.
    /// Intended for testing and diagnostics only.
    /// </summary>
    public IReadOnlyList<ExerciseAttempt> GetAllAttempts()
    {
        lock (_sync)
        {
            return [.. _attempts];
        }
    }
}
