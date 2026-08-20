namespace KeyForge.Features.Practice.Models;

/// <summary>
/// Represents an active practice session for a single lesson.
/// <para>
/// This is a pure domain model. It knows nothing about YAML, files,
/// databases, MIDI, or the framework. It tracks the learner's position
/// within a lesson's exercise sequence.
/// </para>
/// </summary>
public sealed class PracticeSession
{
    private readonly IReadOnlyList<ExerciseDefinition> _exercises;

    /// <summary>
    /// Stable identifier of the lesson being practiced,
    /// matching <see cref="Features.Lessons.Models.LessonDefinition.Id"/>.
    /// </summary>
    public string LessonId { get; }

    /// <summary>
    /// The ordered exercises in this session, derived from the lesson definition.
    /// This list is immutable after construction.
    /// </summary>
    public IReadOnlyList<ExerciseDefinition> Exercises => _exercises;

    /// <summary>
    /// Zero-based index of the current exercise in the sequence.
    /// </summary>
    public int CurrentExerciseIndex { get; private set; }

    /// <summary>
    /// Whether all exercises have been visited (current index is past the last exercise).
    /// </summary>
    public bool IsFinished => CurrentExerciseIndex >= _exercises.Count;

    /// <summary>
    /// Creates a new practice session with the given ordered exercises.
    /// </summary>
    /// <param name="lessonId">The lesson identifier.</param>
    /// <param name="exercises">The ordered exercises to practice.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="lessonId"/> or <paramref name="exercises"/> is <c>null</c>.
    /// </exception>
    public PracticeSession(string lessonId, IReadOnlyList<ExerciseDefinition> exercises)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        ArgumentNullException.ThrowIfNull(exercises);

        LessonId = lessonId;
        _exercises = exercises;
        CurrentExerciseIndex = 0;
    }

    /// <summary>
    /// Returns the current exercise, or <c>null</c> if the session is finished.
    /// </summary>
    public ExerciseDefinition? GetCurrentExercise() =>
        IsFinished ? null : _exercises[CurrentExerciseIndex];

    /// <summary>
    /// Advances to the next exercise in the sequence.
    /// Does nothing if the session is already finished.
    /// </summary>
    public void Next()
    {
        if (!IsFinished)
        {
            CurrentExerciseIndex++;
        }
    }
}
