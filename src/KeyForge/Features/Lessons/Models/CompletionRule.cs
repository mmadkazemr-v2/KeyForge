namespace KeyForge.Features.Lessons.Models;

/// <summary>
/// Describes what the learner must achieve for a lesson to count as completed.
/// <para>
/// The rule lives in the lesson content and is evaluated by the progression
/// engine against user progress. New requirements can be added as additional
/// strongly typed, optional properties without changing the lesson structure.
/// </para>
/// </summary>
public class CompletionRule
{
    /// <summary>
    /// Minimum score (0-100) required to complete the lesson.
    /// Null means no minimum score is required.
    /// </summary>
    public int? MinimumScore { get; set; }

    /// <summary>
    /// When true, every exercise defined in the lesson must be completed
    /// before the lesson counts as completed.
    /// </summary>
    public bool RequireAllExercises { get; set; }
}
