using System.Collections.Concurrent;

namespace KeyForge.Infrastructure.Persistence.Progress;

/// <summary>
/// A simple JSON-file-backed progress store.
/// <para>
/// Progress is held in memory and persisted to a single JSON file on every
/// write. The file is created lazily on the first <see cref="SaveProgress"/>
/// call. If the file does not exist the store behaves as empty.
/// </para>
/// </summary>
public sealed class JsonProgressStore : IProgressStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly object _persistLock = new();
    private readonly ConcurrentDictionary<string, LessonProgress> _progress = new(StringComparer.OrdinalIgnoreCase);

    public JsonProgressStore(IOptions<ProgressStoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _filePath = Path.GetFullPath(options.Value.FilePath);
        Load();
    }

    /// <inheritdoc />
    public LessonProgress? GetProgress(string lessonId)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        return _progress.GetValueOrDefault(lessonId);
    }

    /// <inheritdoc />
    public IReadOnlyList<LessonProgress> GetAllProgress() =>
        [.. _progress.Values];

    /// <inheritdoc />
    public void SaveProgress(LessonProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress[progress.LessonId] = progress;

        lock (_persistLock)
        {
            Persist();
        }
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        var json = File.ReadAllText(_filePath);
        var list = JsonSerializer.Deserialize<List<LessonProgress>>(json, SerializerOptions);

        if (list is null)
        {
            return;
        }

        foreach (var entry in list)
        {
            _progress[entry.LessonId] = entry;
        }
    }

    private void Persist()
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var list = _progress.Values.ToList();
        var json = JsonSerializer.Serialize(list, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }
}
