namespace KeyForge.Tests.UnitTest.Infrastructure.Persistence.Progress;

/// <summary>
/// Tests <see cref="JsonProgressStore"/> against isolated temporary files.
/// Each test uses its own directory which is removed afterwards, so the
/// application's real progress file is never touched.
/// </summary>
public sealed class JsonProgressStoreTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _progressFile;

    public JsonProgressStoreTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "keyforge-progress-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
        _progressFile = Path.Combine(_testDirectory, "progress.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void MissingFile_BehavesAsEmptyStore()
    {
        var store = CreateStore();

        Assert.Null(store.GetProgress("lesson-01"));
        Assert.Empty(store.GetAllProgress());
    }

    [Fact]
    public void SaveProgress_CanBeRetrievedByLessonId()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 85,
            AttemptCount = 3
        });

        var result = store.GetProgress("lesson-01");

        Assert.NotNull(result);
        Assert.Equal("lesson-01", result.LessonId);
        Assert.True(result.IsCompleted);
        Assert.Equal(85, result.BestScore);
        Assert.Equal(3, result.AttemptCount);
    }

    [Fact]
    public void SaveMultiple_AllCanBeRetrieved()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress { LessonId = "lesson-01" });
        store.SaveProgress(new LessonProgress { LessonId = "lesson-02" });
        store.SaveProgress(new LessonProgress { LessonId = "lesson-03" });

        Assert.Equal(3, store.GetAllProgress().Count);
    }

    [Fact]
    public void SaveProgress_ExistingId_UpdatesInsteadOfDuplicate()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress { LessonId = "lesson-01", AttemptCount = 1 });
        store.SaveProgress(new LessonProgress { LessonId = "lesson-01", AttemptCount = 5 });

        var all = store.GetAllProgress();

        Assert.Single(all);
        Assert.Equal(5, all[0].AttemptCount);
    }

    [Fact]
    public void GetProgress_UnknownId_ReturnsNull()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress { LessonId = "lesson-01" });

        Assert.Null(store.GetProgress("does-not-exist"));
    }

    [Fact]
    public void GetAllProgress_ReturnsAllSavedProgress()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress { LessonId = "a", BestScore = 10 });
        store.SaveProgress(new LessonProgress { LessonId = "b", BestScore = 20 });

        var all = store.GetAllProgress();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Progress_SurvivesNewInstance()
    {
        var store1 = CreateStore();
        store1.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 90
        });

        var store2 = CreateStore();
        var result = store2.GetProgress("lesson-01");

        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
        Assert.Equal(90, result.BestScore);
    }

    [Fact]
    public void SaveProgress_CreatesJsonFile()
    {
        var store = CreateStore();

        Assert.False(File.Exists(_progressFile));

        store.SaveProgress(new LessonProgress { LessonId = "lesson-01" });

        Assert.True(File.Exists(_progressFile));
    }

    private JsonProgressStore CreateStore()
    {
        var options = Options.Create(new ProgressStoreOptions { FilePath = _progressFile });
        return new JsonProgressStore(options);
    }
}
