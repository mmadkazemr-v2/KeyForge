namespace KeyForge.Features.Lessons.Models;

/// <summary>
/// Determines when a lesson becomes available to the learner.
/// <para>
/// The mode is expressed in lesson content (YAML) and is evaluated by the
/// progression engine against user progress. It is never hard-coded in C#.
/// </para>
/// </summary>
public enum UnlockMode
{
    /// <summary>
    /// The lesson is available from the very start of the course.
    /// </summary>
    Immediate,

    /// <summary>
    /// The lesson unlocks once the previous lesson in the sequence (determined by
    /// <see cref="LessonDefinition.Order"/>) has been completed.
    /// </summary>
    PreviousLessonCompleted,

    /// <summary>
    /// The lesson unlocks once every lesson listed in
    /// <see cref="UnlockRule.RequiredLessonIds"/> has been completed.
    /// </summary>
    PrerequisitesCompleted
}
