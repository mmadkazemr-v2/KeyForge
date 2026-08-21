namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Manages practice sessions for lessons.
/// <para>
/// The service orchestrates the full practice pipeline: lesson lookup,
/// unlock validation, attempt evaluation, scoring, and recording.
/// It never mutates lesson definitions or progress stores.
/// </para>
/// </summary>
public sealed class PracticeSessionService : IPracticeSessionService
{
    private readonly ILessonCatalog _lessonCatalog;
    private readonly ILessonProgressionService _progressionService;
    private readonly IExerciseEvaluator _evaluator;
    private readonly IExerciseScorer _scorer;
    private readonly IExerciseAttemptRecorder _recorder;

    /// <summary>
    /// Creates a new <see cref="PracticeSessionService"/>.
    /// </summary>
    public PracticeSessionService(
        ILessonCatalog lessonCatalog,
        ILessonProgressionService progressionService,
        IExerciseEvaluator evaluator,
        IExerciseScorer scorer,
        IExerciseAttemptRecorder recorder)
    {
        ArgumentNullException.ThrowIfNull(lessonCatalog);
        ArgumentNullException.ThrowIfNull(progressionService);
        ArgumentNullException.ThrowIfNull(evaluator);
        ArgumentNullException.ThrowIfNull(scorer);
        ArgumentNullException.ThrowIfNull(recorder);

        _lessonCatalog = lessonCatalog;
        _progressionService = progressionService;
        _evaluator = evaluator;
        _scorer = scorer;
        _recorder = recorder;
    }

    /// <inheritdoc />
    public PracticeSession? StartSession(string lessonId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lessonId);

        var lesson = _lessonCatalog.GetById(lessonId);
        if (lesson is null)
        {
            return null;
        }

        if (!_progressionService.IsUnlocked(lessonId))
        {
            throw new InvalidOperationException(
                $"Lesson '{lessonId}' is locked and cannot be practiced.");
        }

        return new PracticeSession(lesson.Id, lesson.Exercises);
    }

    /// <inheritdoc />
    public SessionResult SubmitAttempt(PracticeSession session, string exerciseId, ExerciseAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(exerciseId);
        ArgumentNullException.ThrowIfNull(attempt);

        if (session.IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot submit an attempt for a session that has already finished.");
        }

        var currentExercise = session.GetCurrentExercise();
        if (currentExercise is null || !string.Equals(currentExercise.Id, exerciseId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Exercise '{exerciseId}' does not match the current exercise '{currentExercise?.Id}' in the session.",
                nameof(exerciseId));
        }

        var evaluation = _evaluator.Evaluate(currentExercise, attempt);
        var score = _scorer.Score(evaluation);

        attempt.IsSuccessful = evaluation.IsSuccessful;
        _recorder.Record(attempt);

        var isExerciseCompleted = _recorder.GetAttemptsByLesson(session.LessonId)
            .Any(a => a.ExerciseId == exerciseId && a.IsSuccessful);

        return new SessionResult
        {
            Evaluation = evaluation,
            Score = score,
            IsExerciseCompleted = isExerciseCompleted
        };
    }
}
