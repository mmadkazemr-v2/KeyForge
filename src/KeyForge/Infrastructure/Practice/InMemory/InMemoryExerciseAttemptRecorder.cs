using KeyForge.Features.Practice.Models;
using KeyForge.Features.Practice.Services;

namespace KeyForge.Infrastructure.Practice.InMemory;

/// <summary>
/// A simple in-memory exercise attempt recorder backed by a list.
/// <para>
/// Data lives only for the lifetime of the application process and is lost on
/// restart. Replace this with a persistent implementation without changing
/// any consumer code.
/// </para>
/// </summary>
public sealed class InMemoryExerciseAttemptRecorder : IExerciseAttemptRecorder
{
    private readonly object _lock = new();
    private readonly List<ExerciseAttempt> _attempts = [];

    /// <inheritdoc />
    public void Record(ExerciseAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        lock (_lock)
        {
            _attempts.Add(attempt);
        }
    }

    /// <summary>
    /// Returns all recorded attempts. Intended for testing and diagnostics only.
    /// </summary>
    public IReadOnlyList<ExerciseAttempt> GetAllAttempts()
    {
        lock (_lock)
        {
            return [.. _attempts];
        }
    }
}
