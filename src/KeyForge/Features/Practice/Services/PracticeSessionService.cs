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
    private readonly IExerciseChallengeGenerator _challengeGenerator;
    private readonly IExerciseEvaluator _evaluator;
    private readonly IExerciseScorer _scorer;
    private readonly IExerciseAttemptRecorder _recorder;

    /// <summary>
    /// Creates a new <see cref="PracticeSessionService"/>.
    /// </summary>
    public PracticeSessionService(
        ILessonCatalog lessonCatalog,
        ILessonProgressionService progressionService,
        IExerciseChallengeGenerator challengeGenerator,
        IExerciseEvaluator evaluator,
        IExerciseScorer scorer,
        IExerciseAttemptRecorder recorder)
    {
        ArgumentNullException.ThrowIfNull(lessonCatalog);
        ArgumentNullException.ThrowIfNull(progressionService);
        ArgumentNullException.ThrowIfNull(challengeGenerator);
        ArgumentNullException.ThrowIfNull(evaluator);
        ArgumentNullException.ThrowIfNull(scorer);
        ArgumentNullException.ThrowIfNull(recorder);

        _lessonCatalog = lessonCatalog;
        _progressionService = progressionService;
        _challengeGenerator = challengeGenerator;
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
    public RuntimeExercise StartExercise(PracticeSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot start an exercise for a session that has already finished.");
        }

        var currentExercise = session.GetCurrentExercise();
        if (currentExercise is null)
        {
            throw new InvalidOperationException("The session has no current exercise.");
        }

        return new RuntimeExercise(
            currentExercise,
            _challengeGenerator.Create(currentExercise),
            DateTime.UtcNow);
    }

    /// <inheritdoc />
    public SessionResult SubmitAttempt(
        PracticeSession session,
        RuntimeExercise runtimeExercise,
        DateTime completedAt)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(runtimeExercise);

        if (session.IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot submit an attempt for a session that has already finished.");
        }

        var currentExercise = session.GetCurrentExercise();
        if (currentExercise is null ||
            !string.Equals(currentExercise.Id, runtimeExercise.Definition.Id, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Runtime exercise '{runtimeExercise.Definition.Id}' does not match the current exercise '{currentExercise?.Id}'.",
                nameof(runtimeExercise));
        }

        if (completedAt < runtimeExercise.StartedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedAt),
                "An attempt cannot complete before its runtime starts.");
        }

        var evaluation = _evaluator.Evaluate(
            runtimeExercise.Challenge,
            runtimeExercise.InputEvents);
        var score = _scorer.Score(evaluation);

        var attempt = new ExerciseAttempt
        {
            LessonId = session.LessonId,
            ExerciseId = currentExercise.Id,
            StartedAt = runtimeExercise.StartedAt,
            CompletedAt = completedAt,
            Score = score,
            IsSuccessful = evaluation.IsSuccessful
        };

        _recorder.Record(attempt);

        var isExerciseCompleted = _recorder.GetAttemptsByLesson(session.LessonId)
            .Any(a => a.ExerciseId == currentExercise.Id && a.IsSuccessful);

        return new SessionResult
        {
            Attempt = attempt,
            Evaluation = evaluation,
            Score = score,
            IsExerciseCompleted = isExerciseCompleted
        };
    }
}
