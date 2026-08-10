namespace KeyForge.Features.Lessons.Models;

/// <summary>
/// The overall skill level a lesson is designed for.
/// Levels are ordered so higher values represent more advanced material.
/// </summary>
public enum LessonLevel
{
    /// <summary>For students with no or very little keyboard experience.</summary>
    Beginner,

    /// <summary>For students who know basic notes, fingering, and simple rhythms.</summary>
    Elementary,

    /// <summary>For students comfortable with common keys and moderate rhythms.</summary>
    Intermediate,

    /// <summary>For students working on advanced techniques and complex patterns.</summary>
    Advanced,

    /// <summary>For near-professional or professional players.</summary>
    Master
}
