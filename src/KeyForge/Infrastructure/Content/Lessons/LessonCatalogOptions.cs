namespace KeyForge.Infrastructure.Content.Lessons;

/// <summary>
/// Configuration for <see cref="FileSystemLessonCatalog"/>.
/// </summary>
public class LessonCatalogOptions
{
    /// <summary>Configuration section that carries these options, e.g. "KeyForge:LessonCatalog".</summary>
    public const string SectionName = "KeyForge:LessonCatalog";

    /// <summary>
    /// Relative or absolute path of the directory that holds the lesson files.
    /// Relative paths are resolved against the current working directory
    /// (which for the web application is the content root).
    /// </summary>
    public string ContentPath { get; set; } = "Content/Lessons";
}
