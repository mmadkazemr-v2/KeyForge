namespace KeyForge.Infrastructure.Content.Lessons;

/// <summary>
/// Thrown when the configured lesson content directory does not exist.
/// </summary>
public class LessonContentDirectoryNotFoundException : Exception
{
    /// <summary>The full path that was expected to hold the lesson files.</summary>
    public string DirectoryPath { get; }

    public LessonContentDirectoryNotFoundException(string directoryPath)
        : base($"The lesson content directory was not found: '{directoryPath}'.")
    {
        DirectoryPath = directoryPath;
    }
}
