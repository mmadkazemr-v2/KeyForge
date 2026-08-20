namespace KeyForge.Infrastructure.Persistence.Progress;

/// <summary>
/// Configuration for <see cref="JsonProgressStore"/>.
/// </summary>
public class ProgressStoreOptions
{
    /// <summary>Configuration section that carries these options.</summary>
    public const string SectionName = "KeyForge:ProgressStore";

    /// <summary>
    /// Relative or absolute path of the JSON file that holds the progress data.
    /// Relative paths are resolved against the current working directory.
    /// </summary>
    public string FilePath { get; set; } = "progress.json";
}
