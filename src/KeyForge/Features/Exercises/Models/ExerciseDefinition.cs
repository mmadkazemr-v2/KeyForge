namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// Base type of every exercise inside a lesson.
/// <para>
/// Specialized exercise definitions derive from this type and add
/// type-specific properties. The <see cref="Type"/> property acts as the
/// discriminator used when deserializing a YAML exercise entry back into its
/// concrete derived type.
/// </para>
/// </summary>
public abstract class ExerciseDefinition
{
    /// <summary>
    /// Stable identifier of the exercise, unique within its lesson, e.g. "ex-03".
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Kind of exercise. Must match the concrete derived type
    /// (a <see cref="RhythmExerciseDefinition"/> has <c>Type = Rhythm</c>).
    /// </summary>
    public ExerciseType Type { get; set; }

    /// <summary>Short title shown to the student.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional longer description of what the exercise practises.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Expected duration of the exercise in minutes.
    /// A value of 0 means no fixed duration is set.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Suggested starting tempo in beats per minute.
    /// A value of 0 means tempo is not applicable or not set.
    /// </summary>
    public int Tempo { get; set; }

    /// <summary>Relative difficulty of the exercise.</summary>
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
}
