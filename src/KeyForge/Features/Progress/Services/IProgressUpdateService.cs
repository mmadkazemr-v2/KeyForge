namespace KeyForge.Features.Progress.Services;

/// <summary>
/// Updates lesson progress after a practice attempt.
/// <para>
/// The service orchestrates reading the current progress, updating best
/// score and attempt count, determining lesson completion, and persisting
/// the result through <see cref="IProgressStore"/>.
/// </para>
/// </summary>
public interface IProgressUpdateService
{
    /// <summary>
    /// Updates the stored progress for the specified lesson based on the
    /// outcome of a completed exercise attempt.
    /// </summary>
    /// <param name="lessonId">
    /// The stable identifier of the lesson being practiced.
    /// Must not be <c>null</c>.
    /// </param>
    /// <param name="result">
    /// The session result produced by the evaluation/scoring pipeline.
    /// Must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="lessonId"/> or <paramref name="result"/> is <c>null</c>.
    /// </exception>
    void UpdateProgress(string lessonId, SessionResult result);
}
