namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Manages practice sessions for lessons.
/// <para>
/// The service is responsible for starting sessions, validating unlock state,
/// accepting exercise attempts, and orchestrating the evaluation/scoring pipeline.
/// It never mutates lesson definitions or progress stores.
/// </para>
/// </summary>
public interface IPracticeSessionService
{
    /// <summary>
    /// Starts a new practice session for the specified lesson.
    /// Validates that the lesson exists and is unlocked.
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
    /// <exception cref="InvalidOperationException">
    /// Thrown when the lesson exists but is locked.
    /// </exception>
    PracticeSession? StartSession(string lessonId);

    /// <summary>
    /// Submits an exercise attempt for evaluation, scoring, and recording.
    /// </summary>
    /// <param name="session">
    /// The active practice session. Must not be <c>null</c>.
    /// </param>
    /// <param name="exerciseId">
    /// The identifier of the exercise being attempted.
    /// Must match the exercise at the session's current position.
    /// </param>
    /// <param name="attempt">
    /// The recorded attempt data. Must not be <c>null</c>.
    /// </param>
    /// <returns>
    /// A <see cref="SessionResult"/> containing the evaluation outcome and score.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="session"/>, <paramref name="exerciseId"/>,
    /// or <paramref name="attempt"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="exerciseId"/> does not match the current
    /// exercise in the session.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the session has already finished.
    /// </exception>
    SessionResult SubmitAttempt(PracticeSession session, string exerciseId, ExerciseAttempt attempt);
}
