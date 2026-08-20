namespace KeyForge.Infrastructure.Content.Lessons.Validation;

/// <summary>
/// Validates lesson content directory, file extensions, and lesson ID uniqueness.
/// </summary>
internal static class LessonContentValidator
{
    private static readonly string[] SupportedExtensions = [".yaml", ".yml"];

    /// <summary>
    /// Validates that the content directory exists.
    /// </summary>
    /// <exception cref="LessonContentDirectoryNotFoundException">
    /// The directory does not exist.
    /// </exception>
    internal static void ValidateDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new LessonContentDirectoryNotFoundException(directoryPath);
        }
    }

    /// <summary>
    /// Returns true if the file has a supported lesson extension (.yaml or .yml).
    /// </summary>
    internal static bool IsLessonFile(string filePath)
        => SupportedExtensions.Contains(
            Path.GetExtension(filePath),
            StringComparer.OrdinalIgnoreCase
        );

    /// <summary>
    /// Validates that no duplicate lesson IDs exist in the loaded lessons.
    /// </summary>
    /// <exception cref="DuplicateLessonIdException">
    /// Two or more files declare the same lesson ID.
    /// </exception>
    internal static void ValidateNoDuplicateIds(IReadOnlyList<(string LessonId, string FilePath)> lessons)
    {
        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (lessonId, filePath) in lessons)
        {
            if (seen.TryGetValue(lessonId, out var originalFile))
            {
                throw new DuplicateLessonIdException(lessonId, originalFile, filePath);
            }

            seen[lessonId] = filePath;
        }
    }
}