namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Manages practice sessions for lessons.
/// <para>
/// The service is responsible for starting sessions and providing
/// navigation operations. It never mutates lesson definitions or progress.
/// </para>
/// </summary>
public interface IPracticeSessionService
{
    /// <summary>
    /// Starts a new practice session for the specified lesson.
    /// </summary>
    /// <param name="lessonId">
    /// The lesson identifier to practice. Must not be <c>null</c>.
    /// </param>
    /// <returns>
    /// A new <see cref="PracticeSession"/> containing the lesson's exercises
    /// in order, or <c>null</c> if the lesson was not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="lessonId"/> is <c>null</c>.
    /// </exception>
    PracticeSession? StartSession(string lessonId);
}
