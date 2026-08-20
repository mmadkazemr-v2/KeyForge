namespace KeyForge.Infrastructure.Progress.InMemory;

/// <summary>
/// A simple in-memory progress store backed by a dictionary.
/// <para>
/// Data lives only for the lifetime of the application process and is lost on
/// restart. Replace this with a persistent implementation (JSON, SQLite, etc.)
/// without changing any consumer code.
/// </para>
/// </summary>
public sealed class InMemoryProgressStore : IProgressStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, LessonProgress> _progress = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public LessonProgress? GetProgress(string lessonId)
    {
        ArgumentNullException.ThrowIfNull(lessonId);

        lock (_lock)
        {
            return _progress.GetValueOrDefault(lessonId);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<LessonProgress> GetAllProgress()
    {
        lock (_lock)
        {
            return [.. _progress.Values];
        }
    }

    /// <inheritdoc />
    public void SaveProgress(LessonProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        lock (_lock)
        {
            _progress[progress.LessonId] = progress;
        }
    }
}
