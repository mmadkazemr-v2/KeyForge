namespace KeyForge.Infrastructure.Content.Lessons;

/// <summary>
/// Thrown when a single lesson file cannot be turned into a lesson, for example
/// because it is missing, cannot be read, or contains invalid YAML. The
/// original parser or IO exception is preserved as the inner exception.
/// </summary>
public class LessonContentLoadException : Exception
{
    /// <summary>Full path of the lesson file that failed to load.</summary>
    public string FilePath { get; }

    public LessonContentLoadException(string filePath, Exception innerException)
        : base($"Failed to load lesson '{Path.GetFileName(filePath)}'.", innerException)
    {
        FilePath = filePath;
    }
}
