namespace KeyForge.Features.Lessons.Models;

/// <summary>
/// Top-level definition of a practice lesson loaded from a YAML lesson file.
/// A lesson groups a sequence of heterogeneous exercises into one practice session.
/// </summary>
public class LessonDefinition
{
    /// <summary>
    /// Stable, machine-readable identifier of the lesson, e.g. "lesson-01".
    /// Used for routing, referencing and progress tracking.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-readable title shown to the student.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional short description of the lesson's goals.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Skill level this lesson targets.</summary>
    public LessonLevel Level { get; set; } = LessonLevel.Beginner;

    /// <summary>
    /// Display/sort order of the lesson within its course or category.
    /// Lower values are shown first.
    /// </summary>
    public int Order { get; set; }

    /// <summary>Estimated total practice time of the lesson in minutes.</summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>
    /// Describes when this lesson becomes available to the learner.
    /// Expressed in content and evaluated by the progression engine.
    /// </summary>
    public UnlockRule Unlock { get; set; } = new();

    /// <summary>
    /// Describes what must be achieved for this lesson to count as completed.
    /// Expressed in content and evaluated by the progression engine.
    /// </summary>
    public CompletionRule Completion { get; set; } = new();

    /// <summary>
    /// The exercises that make up the lesson.
    /// Items are polymorphic; each entry deserializes into a concrete
    /// <see cref="ExerciseDefinition"/> subclass based on its <c>Type</c> discriminator.
    /// </summary>
    public List<ExerciseDefinition> Exercises { get; set; } = [];
}
