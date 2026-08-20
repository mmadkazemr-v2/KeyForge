namespace KeyForge.Infrastructure.Content.Lessons;

/// <summary>
/// A static, read-only lesson catalog backed by YAML lesson files on disk.
/// <para>
/// The catalog discovers lesson files in the configured content directory,
/// parses each one with the existing <see cref="IYamlLessonParser"/> and keeps
/// the resulting <see cref="LessonDefinition"/> objects in memory for the
/// lifetime of the process. Content is loaded once at construction; adding or
/// editing files requires a restart.
/// </para>
/// </summary>
public sealed class FileSystemLessonCatalog : ILessonCatalog
{
    private readonly IReadOnlyList<LessonDefinition> _lessons;
    private readonly IReadOnlyDictionary<string, LessonDefinition> _lessonsById;

    public FileSystemLessonCatalog(IOptions<LessonCatalogOptions> options, IYamlLessonParser parser)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(parser);

        var contentDirectory = Path.GetFullPath(options.Value.ContentPath);
        LessonContentValidator.ValidateDirectory(contentDirectory);

        _lessons = LoadLessons(contentDirectory, parser);
        _lessonsById = _lessons.ToDictionary(lesson => lesson.Id, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public IReadOnlyList<LessonDefinition> GetAll() => _lessons;

    /// <inheritdoc />
    public LessonDefinition? GetById(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _lessonsById.GetValueOrDefault(id);
    }

    private static IReadOnlyList<LessonDefinition> LoadLessons(string contentDirectory, IYamlLessonParser parser)
    {
        var lessonFiles = Directory
            .EnumerateFiles(contentDirectory)
            .Where(LessonContentValidator.IsLessonFile)
            .OrderBy(file => file, StringComparer.Ordinal)
            .ToArray();

        var lessons = new List<LessonDefinition>(lessonFiles.Length);
        var entries = new List<(string LessonId, string FilePath)>(lessonFiles.Length);

        foreach (var file in lessonFiles)
        {
            var lesson = LoadLesson(file, parser);
            entries.Add((lesson.Id, file));
            lessons.Add(lesson);
        }

        LessonContentValidator.ValidateNoDuplicateIds(entries);

        return lessons
            .OrderBy(lesson => lesson.Order)
            .ToList()
            .AsReadOnly();
    }

    private static LessonDefinition LoadLesson(string file, IYamlLessonParser parser)
    {
        string yaml;
        try
        {
            yaml = File.ReadAllText(file);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new LessonContentLoadException(file, ex);
        }

        try
        {
            return parser.Parse(yaml);
        }
        catch (YamlLessonParseException ex)
        {
            throw new LessonContentLoadException(file, ex);
        }
    }
}