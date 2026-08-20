namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Answers the question "what lessons exist?" by exposing the lesson content
/// catalog. Callers only ever see <see cref="LessonDefinition"/> objects; the
/// storage behind the catalog (files, YAML, paths) is deliberately hidden.
/// </summary>
public interface ILessonCatalog
{
    /// <summary>
    /// Returns all lessons in the catalog ordered by
    /// <see cref="LessonDefinition.Order"/> (lowest first).
    /// </summary>
    /// <returns>A read-only view over the catalog; it cannot be modified by callers.</returns>
    IReadOnlyList<LessonDefinition> GetAll();

    /// <summary>
    /// Finds a lesson by its <see cref="LessonDefinition.Id"/>. Matching is
    /// case-insensitive.
    /// </summary>
    /// <param name="id">The lesson id to look up.</param>
    /// <returns>The matching lesson, or <c>null</c> when no lesson has that id.</returns>
    LessonDefinition? GetById(string id);
}
