namespace KeyForge.Infrastructure.Content.Lessons.Exceptions;

/// <summary>
/// Thrown when two or more lesson files declare the same lesson id.
/// <para>
/// The catalog treats duplicate ids as broken content and fails instead of
/// silently letting one file win.
/// </para>
/// </summary>
public class DuplicateLessonIdException : Exception
{
    /// <summary>The duplicated lesson id.</summary>
    public string LessonId { get; }

    /// <summary>Full path of the first file that declared the id.</summary>
    public string OriginalFilePath { get; }

    /// <summary>Full path of the conflicting file that declared the id again.</summary>
    public string DuplicateFilePath { get; }

    public DuplicateLessonIdException(string lessonId, string originalFilePath, string duplicateFilePath)
        : base($"Duplicate lesson id '{lessonId}' found in '{originalFilePath}' and '{duplicateFilePath}'.")
    {
        LessonId = lessonId;
        OriginalFilePath = originalFilePath;
        DuplicateFilePath = duplicateFilePath;
    }
}
