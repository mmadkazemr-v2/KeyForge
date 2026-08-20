namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Records the result of a single exercise attempt.
/// <para>
/// The service is the single entry point for persisting attempt data.
/// Implementations may store attempts in memory, on disk, or in a
/// database. Consumers depend only on this contract.
/// </para>
/// </summary>
public interface IExerciseAttemptRecorder
{
    /// <summary>
    /// Records a completed or in-progress exercise attempt.
    /// </summary>
    /// <param name="attempt">
    /// The attempt data to record. Must not be <c>null</c>.
    /// </param>
    void Record(ExerciseAttempt attempt);
}
