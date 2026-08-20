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
    private readonly object _lock = new();
    private Dictionary<string, LessonProgress> _progress = new(StringComparer.OrdinalIgnoreCase);

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

        lock (_lock)
        {
            return _progress.GetValueOrDefault(lessonId);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<LessonProgress> GetAllProgress()
    {
        lock (_lock)
        {
            return _progress.Values.ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public void SaveProgress(LessonProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        lock (_lock)
        {
            _progress[progress.LessonId] = progress;
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

        _progress = new Dictionary<string, LessonProgress>(list.Count, StringComparer.OrdinalIgnoreCase);

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
