namespace KeyForge.Infrastructure.Persistence.Progress;

/// <summary>Atomic JSON snapshots for one anonymous browser profile. Single application process only.</summary>
public sealed class JsonPracticeStore : IProgressStore, IExerciseAttemptRecorder
{
    private static readonly ConcurrentDictionary<string, object> Locks = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path;
    private readonly object _sync;
    public JsonPracticeStore(string path)
    {
        _path = Path.GetFullPath(path);
        _sync = Locks.GetOrAdd(_path, _ => new object());
    }
    public sealed class Snapshot
    {
        public List<ExerciseAttempt> Attempts { get; set; } = [];
        public List<LessonProgress> Progress { get; set; } = [];
    }
    private Snapshot Read()
    {
        if (!File.Exists(_path)) return new();
        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object || !document.RootElement.TryGetProperty("Attempts", out _) || !document.RootElement.TryGetProperty("Progress", out _))
                throw new JsonException("Missing practice snapshot fields.");
            var data = JsonSerializer.Deserialize<Snapshot>(json);
            if (data is null || data.Attempts is null || data.Progress is null ||
                data.Attempts.Any(a => a is null || a.Id == Guid.Empty || string.IsNullOrWhiteSpace(a.LessonId) || string.IsNullOrWhiteSpace(a.ExerciseId)) ||
                data.Progress.Any(p => p is null || string.IsNullOrWhiteSpace(p.LessonId)))
                throw new JsonException("Invalid practice snapshot.");
            return data;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Practice storage is corrupted. Restore the profile file from a backup before continuing.", ex);
        }
    }
    private void Write(Snapshot data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporary = _path + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, data);
                stream.Flush(true);
            }
            File.Move(temporary, _path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public LessonProgress? GetProgress(string lessonId)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        lock (_sync) return Read().Progress.FirstOrDefault(p => p.LessonId == lessonId);
    }
    public IReadOnlyList<LessonProgress> GetAllProgress() { lock (_sync) return Read().Progress; }
    public void SaveProgress(LessonProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        lock (_sync)
        {
            var data = Read();
            var prior = data.Progress.FirstOrDefault(p => p.LessonId == progress.LessonId);
            if (prior is not null)
            {
                progress.IsCompleted |= prior.IsCompleted;
                progress.AttemptCount = Math.Max(progress.AttemptCount, prior.AttemptCount);
                if (prior.BestScore is not null && (progress.BestScore is null || prior.BestScore > progress.BestScore))
                    progress.BestScore = prior.BestScore;
            }
            data.Progress.RemoveAll(p => p.LessonId == progress.LessonId);
            data.Progress.Add(progress);
            Write(data);
        }
    }
    public void Record(ExerciseAttempt attempt) => RecordIfNew(attempt);
    public bool RecordIfNew(ExerciseAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        lock (_sync)
        {
            var data = Read();
            if (data.Attempts.Any(a => a.Id == attempt.Id)) return false;
            data.Attempts.Add(attempt);
            if (attempt.CompletedAt is not null && attempt.Score is not null)
            {
                var progress = data.Progress.FirstOrDefault(p => p.LessonId == attempt.LessonId);
                if (progress is null)
                {
                    progress = new LessonProgress { LessonId = attempt.LessonId };
                    data.Progress.Add(progress);
                }
                var scored = data.Attempts.Where(a => a.LessonId == attempt.LessonId && a.CompletedAt is not null && a.Score is not null).ToArray();
                progress.AttemptCount = scored.Length;
                progress.BestScore = scored.Max(a => a.Score);
            }
            Write(data);
            return true;
        }
    }
    public IReadOnlyList<ExerciseAttempt> GetAttemptsByLesson(string lessonId)
    {
        ArgumentNullException.ThrowIfNull(lessonId);
        lock (_sync) return Read().Attempts.Where(a => a.LessonId == lessonId).ToArray();
    }
}
