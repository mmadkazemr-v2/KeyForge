using KeyForge.Features.Lessons.Models;
using KeyForge.Features.Lessons.Services;
using KeyForge.Infrastructure.Yaml.Parsing;
using Microsoft.Extensions.Options;

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
    private static readonly string[] LessonFileExtensions = [".yaml", ".yml"];

    private readonly IReadOnlyList<LessonDefinition> _lessons;
    private readonly IReadOnlyDictionary<string, LessonDefinition> _lessonsById;

    public FileSystemLessonCatalog(IOptions<LessonCatalogOptions> options, IYamlLessonParser parser)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(parser);

        var contentDirectory = Path.GetFullPath(options.Value.ContentPath);

        if (!Directory.Exists(contentDirectory))
        {
            throw new LessonContentDirectoryNotFoundException(contentDirectory);
        }

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

    /// <summary>
    /// Discovers, parses and validates every lesson file in the directory.
    /// Files are processed in a deterministic (name) order so that duplicate-id
    /// reporting is stable. Lessons are ordered by <see cref="LessonDefinition.Order"/>.
    /// </summary>
    private static IReadOnlyList<LessonDefinition> LoadLessons(string contentDirectory, IYamlLessonParser parser)
    {
        var lessonFiles = Directory
            .EnumerateFiles(contentDirectory)
            .Where(IsLessonFile)
            .OrderBy(file => file, StringComparer.Ordinal)
            .ToArray();

        var lessons = new List<LessonDefinition>(lessonFiles.Length);
        var filesById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in lessonFiles)
        {
            var lesson = LoadLesson(file, parser);

            if (filesById.TryGetValue(lesson.Id, out var originalFile))
            {
                throw new DuplicateLessonIdException(lesson.Id, originalFile, file);
            }

            filesById[lesson.Id] = file;
            lessons.Add(lesson);
        }

        return lessons
            .OrderBy(lesson => lesson.Order)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Reads a lesson file and parses it, adding file context to any failure so
    /// the problematic file can be identified.
    /// </summary>
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

    private static bool IsLessonFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);

        foreach (var supported in LessonFileExtensions)
        {
            if (extension.Equals(supported, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
