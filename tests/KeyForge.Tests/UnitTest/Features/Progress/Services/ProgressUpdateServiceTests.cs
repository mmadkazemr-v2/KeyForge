namespace KeyForge.Tests.UnitTest.Features.Progress.Services;

/// <summary>
/// Tests <see cref="ProgressUpdateService"/> using in-memory fakes
/// to verify progress update behavior after exercise attempts.
/// </summary>
public sealed class ProgressUpdateServiceTests
{
    private static RhythmExerciseDefinition Exercise1 { get; } = new()
    {
        Id = "ex-01",
        Title = "Quarter Notes",
        Type = ExerciseType.Rhythm
    };

    private static ProgressUpdateService CreateService(
        ILessonCatalog? catalog = null,
        IProgressStore? progressStore = null,
        ILessonProgressionService? progression = null)
    {
        return new ProgressUpdateService(
            progressStore ?? new InMemoryProgressStore(),
            catalog ?? new FakeLessonCatalog(),
            progression ?? new FakeProgressionService());
    }

    private static SessionResult MakeResult(int score = 80) => new()
    {
        Evaluation = new ExerciseEvaluationResult { IsSuccessful = true, Score = score },
        Score = score
    };

    private static LessonDefinition MakeLesson(
        string id = "lesson-01",
        CompletionRule? completion = null) => new()
    {
        Id = id,
        Exercises = [Exercise1],
        Completion = completion ?? new CompletionRule()
    };

    #region Creation tests

    [Fact]
    public void UpdateProgress_NoExistingProgress_CreatesProgress()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult());

        var progress = store.GetProgress("lesson-01");
        Assert.NotNull(progress);
        Assert.Equal("lesson-01", progress.LessonId);
    }

    [Fact]
    public void UpdateProgress_ExistingProgress_UpdatesProgress()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 50,
            AttemptCount = 1
        });
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult(75));

        var progress = store.GetProgress("lesson-01");
        Assert.NotNull(progress);
        Assert.Equal(75, progress.BestScore);
        Assert.Equal(2, progress.AttemptCount);
    }

    #endregion

    #region BestScore tests

    [Fact]
    public void UpdateProgress_HigherScore_ReplacesBestScore()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 60
        });
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult(90));

        Assert.Equal(90, store.GetProgress("lesson-01")!.BestScore);
    }

    [Fact]
    public void UpdateProgress_LowerScore_DoesNotReplaceBestScore()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 80
        });
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult(70));

        Assert.Equal(80, store.GetProgress("lesson-01")!.BestScore);
    }

    [Fact]
    public void UpdateProgress_FirstScore_IsStoredCorrectly()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult(75));

        Assert.Equal(75, store.GetProgress("lesson-01")!.BestScore);
    }

    #endregion

    #region AttemptCount tests

    [Fact]
    public void UpdateProgress_AttemptCount_IncrementsOnFirstAttempt()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult());

        Assert.Equal(1, store.GetProgress("lesson-01")!.AttemptCount);
    }

    [Fact]
    public void UpdateProgress_AttemptCount_IncrementsOnSubsequentAttempts()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            AttemptCount = 3
        });
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult(85));

        Assert.Equal(4, store.GetProgress("lesson-01")!.AttemptCount);
    }

    #endregion

    #region Completion tests

    [Fact]
    public void UpdateProgress_CompletedLesson_RemainsCompleted()
    {
        var catalog = new FakeLessonCatalog(MakeLesson(
            completion: new CompletionRule { MinimumScore = 70 }));
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 80
        });
        var progression = new FakeProgressionService(isCompleted: true);
        var service = CreateService(catalog: catalog, progressStore: store, progression: progression);

        service.UpdateProgress("lesson-01", MakeResult(85));

        Assert.True(store.GetProgress("lesson-01")!.IsCompleted);
    }

    [Fact]
    public void UpdateProgress_CompletionRulesMet_SetsCompleted()
    {
        var catalog = new FakeLessonCatalog(MakeLesson(
            completion: new CompletionRule { MinimumScore = 70 }));
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = false,
            BestScore = 60
        });
        var progression = new FakeProgressionService(isCompleted: true);
        var service = CreateService(catalog: catalog, progressStore: store, progression: progression);

        service.UpdateProgress("lesson-01", MakeResult(75));

        Assert.True(store.GetProgress("lesson-01")!.IsCompleted);
    }

    [Fact]
    public void UpdateProgress_CompletionRulesNotMet_SetsNotCompleted()
    {
        var catalog = new FakeLessonCatalog(MakeLesson(
            completion: new CompletionRule { MinimumScore = 70 }));
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 80
        });
        var progression = new FakeProgressionService(isCompleted: false);
        var service = CreateService(catalog: catalog, progressStore: store, progression: progression);

        service.UpdateProgress("lesson-01", MakeResult(50));

        Assert.False(store.GetProgress("lesson-01")!.IsCompleted);
    }

    [Fact]
    public void UpdateProgress_RequireAllExercises_NotBypassed()
    {
        var catalog = new FakeLessonCatalog(MakeLesson(
            completion: new CompletionRule { RequireAllExercises = true }));
        var store = new InMemoryProgressStore();
        var progression = new FakeProgressionService(isCompleted: false);
        var service = CreateService(catalog: catalog, progressStore: store, progression: progression);

        service.UpdateProgress("lesson-01", MakeResult(100));

        Assert.False(store.GetProgress("lesson-01")!.IsCompleted);
    }

    #endregion

    #region Dependency and data preservation tests

    [Fact]
    public void UpdateProgress_UsesIProgressStore_NotConcreteType()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new SpyProgressStore();
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("lesson-01", MakeResult());

        Assert.True(store.SaveProgressCalled);
    }

    [Fact]
    public void UpdateProgress_ExistingData_NotLost()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var store = new InMemoryProgressStore();
        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 85,
            AttemptCount = 5,
            IsCompleted = true
        });
        var progression = new FakeProgressionService(isCompleted: true);
        var service = CreateService(catalog: catalog, progressStore: store, progression: progression);

        service.UpdateProgress("lesson-01", MakeResult(70));

        var progress = store.GetProgress("lesson-01");
        Assert.NotNull(progress);
        Assert.Equal("lesson-01", progress.LessonId);
        Assert.Equal(85, progress.BestScore);
        Assert.Equal(6, progress.AttemptCount);
        Assert.True(progress.IsCompleted);
    }

    #endregion

    #region Error handling tests

    [Fact]
    public void UpdateProgress_NullLessonId_ThrowsArgumentNullException()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() =>
            service.UpdateProgress(null!, MakeResult()));
    }

    [Fact]
    public void UpdateProgress_NullResult_ThrowsArgumentNullException()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() =>
            service.UpdateProgress("lesson-01", null!));
    }

    [Fact]
    public void UpdateProgress_UnknownLesson_DoesNotThrow()
    {
        var catalog = new FakeLessonCatalog();
        var store = new InMemoryProgressStore();
        var service = CreateService(catalog: catalog, progressStore: store);

        service.UpdateProgress("nonexistent", MakeResult());

        Assert.Null(store.GetProgress("nonexistent"));
    }

    #endregion

    #region ProgressionService integration tests

    [Fact]
    public void UpdateProgress_CallsProgressionService()
    {
        var catalog = new FakeLessonCatalog(MakeLesson());
        var progression = new FakeProgressionService(isCompleted: true);
        var service = CreateService(catalog: catalog, progression: progression);

        service.UpdateProgress("lesson-01", MakeResult());

        Assert.True(progression.IsCompletedCalled);
        Assert.Equal("lesson-01", progression.LastLessonId);
    }

    #endregion

    #region DI resolution test

    [Fact]
    public void Di_CanResolveProgressUpdateService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IProgressStore, InMemoryProgressStore>();
        services.AddSingleton<ILessonCatalog, FakeLessonCatalog>();
        services.AddSingleton<ILessonProgressionService, FakeProgressionService>();
        services.AddSingleton<IProgressUpdateService, ProgressUpdateService>();
        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IProgressUpdateService>();

        Assert.NotNull(service);
        Assert.IsType<ProgressUpdateService>(service);
    }

    #endregion

    #region Fakes

    private sealed class FakeLessonCatalog : ILessonCatalog
    {
        private readonly Dictionary<string, LessonDefinition> _lessons;

        public FakeLessonCatalog() : this([]) { }

        public FakeLessonCatalog(params LessonDefinition[] lessons)
        {
            _lessons = lessons.ToDictionary(l => l.Id, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<LessonDefinition> GetAll() =>
            _lessons.Values.ToList().AsReadOnly();

        public LessonDefinition? GetById(string id) =>
            _lessons.TryGetValue(id, out var lesson) ? lesson : null;
    }

    private sealed class FakeProgressionService : ILessonProgressionService
    {
        private readonly bool _isCompleted;

        public bool IsCompletedCalled { get; private set; }
        public string? LastLessonId { get; private set; }

        public FakeProgressionService() : this(false) { }

        public FakeProgressionService(bool isCompleted)
        {
            _isCompleted = isCompleted;
        }

        public bool IsUnlocked(string lessonId) => true;

        public bool IsCompleted(string lessonId)
        {
            IsCompletedCalled = true;
            LastLessonId = lessonId;
            return _isCompleted;
        }
    }

    private sealed class SpyProgressStore : IProgressStore
    {
        private readonly Dictionary<string, LessonProgress> _store = new(StringComparer.OrdinalIgnoreCase);

        public bool SaveProgressCalled { get; private set; }

        public LessonProgress? GetProgress(string lessonId) =>
            _store.TryGetValue(lessonId, out var p) ? p : null;

        public IReadOnlyList<LessonProgress> GetAllProgress() =>
            _store.Values.ToList().AsReadOnly();

        public void SaveProgress(LessonProgress progress)
        {
            SaveProgressCalled = true;
            _store[progress.LessonId] = progress;
        }
    }

    #endregion
}
