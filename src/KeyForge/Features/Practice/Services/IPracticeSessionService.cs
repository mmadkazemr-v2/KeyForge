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

    /// <summary>Creates a runtime and challenge for the current exercise.</summary>
    RuntimeExercise StartExercise(PracticeSession session);

    /// <summary>Evaluates captured runtime input and records the generated attempt.</summary>
    /// <param name="session">
    /// The active practice session. Must not be <c>null</c>.
    /// </param>
    /// <param name="runtimeExercise">The active runtime for the current exercise.</param>
    /// <param name="completedAt">UTC time at which input capture finished.</param>
    /// <returns>
    /// A <see cref="SessionResult"/> containing the evaluation outcome and score.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="session"/> or <paramref name="runtimeExercise"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the runtime does not match the current exercise.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the session has already finished.
    /// </exception>
    SessionResult SubmitAttempt(
        PracticeSession session,
        RuntimeExercise runtimeExercise,
        DateTime completedAt);
}
