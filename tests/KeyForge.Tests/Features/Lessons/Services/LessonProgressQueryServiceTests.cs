using KeyForge.Features.Lessons.Models;
using KeyForge.Features.Lessons.Services;
using KeyForge.Features.Progress.Models;
using KeyForge.Features.Progress.Services;
using KeyForge.Infrastructure.Progress.InMemory;

namespace KeyForge.Tests.Features.Lessons.Services;

/// <summary>
/// Tests <see cref="LessonProgressQueryService"/> using in-memory fakes
/// to verify lesson list preparation without touching files or YAML.
/// </summary>
public sealed class LessonProgressQueryServiceTests
{
    private static InMemoryProgressStore CreateProgressStore() => new();

    private static LessonProgressionService CreateProgressionService(
        IReadOnlyList<LessonDefinition> lessons,
        IProgressStore? progressStore = null)
    {
        var catalog = new FakeLessonCatalog(lessons);
        var store = progressStore ?? CreateProgressStore();
        return new LessonProgressionService(catalog, store);
    }

    private static LessonProgressQueryService CreateService(
        IReadOnlyList<LessonDefinition> lessons,
        IProgressStore? progressStore = null)
    {
        var catalog = new FakeLessonCatalog(lessons);
        var store = progressStore ?? CreateProgressStore();
        var progression = new LessonProgressionService(catalog, store);
        return new LessonProgressQueryService(catalog, store, progression);
    }

    [Fact]
    public void GetLessons_ReturnsAllLessons()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Title = "Second Lesson",
                Order = 2
            }
        };

        var service = CreateService(lessons);
        var result = service.GetLessons();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetLessons_PreservesLessonOrder()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-c",
                Title = "Third",
                Order = 30
            },
            new LessonDefinition
            {
                Id = "lesson-a",
                Title = "First",
                Order = 10
            },
            new LessonDefinition
            {
                Id = "lesson-b",
                Title = "Second",
                Order = 20
            }
        };

        var service = CreateService(lessons);
        var result = service.GetLessons();

        Assert.Equal("lesson-a", result[0].Id);
        Assert.Equal("lesson-b", result[1].Id);
        Assert.Equal("lesson-c", result[2].Id);
    }

    [Fact]
    public void GetLessons_UnlockedLesson_IsMarkedUnlocked()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            }
        };

        var service = CreateService(lessons);
        var result = service.GetLessons();

        Assert.Single(result);
        Assert.True(result[0].IsUnlocked);
    }

    [Fact]
    public void GetLessons_LockedLesson_IsMarkedLocked()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Title = "Second Lesson",
                Order = 2,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var service = CreateService(lessons);
        var result = service.GetLessons();

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
    }

    [Fact]
    public void GetLessons_CompletedLesson_IsMarkedCompleted()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1,
                Completion = new CompletionRule()
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 85
        });

        var service = CreateService(lessons, progressStore);
        var result = service.GetLessons();

        Assert.Single(result);
        Assert.True(result[0].IsCompleted);
    }

    [Fact]
    public void GetLessons_MissingProgress_IsHandledCorrectly()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            }
        };

        var service = CreateService(lessons);
        var result = service.GetLessons();

        Assert.Single(result);
        Assert.True(result[0].IsUnlocked);
        Assert.False(result[0].IsCompleted);
        Assert.Null(result[0].BestScore);
    }

    [Fact]
    public void GetLessons_ReturnsBestScoreWhenProgressExists()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = false,
            BestScore = 72
        });

        var service = CreateService(lessons, progressStore);
        var result = service.GetLessons();

        Assert.Single(result);
        Assert.Equal(72, result[0].BestScore);
    }

    [Fact]
    public void GetLessons_DoesNotMutateLessonOrProgressData()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "First Lesson",
            Order = 1,
            Description = "Original description",
            Level = LessonLevel.Intermediate,
            EstimatedMinutes = 15,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { MinimumScore = 70 }
        };

        var progress = new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 90,
            AttemptCount = 5
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(progress);

        var service = CreateService([lesson], progressStore);
        service.GetLessons();

        // Verify lesson was not mutated
        Assert.Equal("First Lesson", lesson.Title);
        Assert.Equal("Original description", lesson.Description);
        Assert.Equal(LessonLevel.Intermediate, lesson.Level);
        Assert.Equal(15, lesson.EstimatedMinutes);
        Assert.Equal(UnlockMode.Immediate, lesson.Unlock.Mode);
        Assert.Equal(70, lesson.Completion.MinimumScore);

        // Verify progress was not mutated
        var storedProgress = progressStore.GetProgress("lesson-01");
        Assert.NotNull(storedProgress);
        Assert.True(storedProgress.IsCompleted);
        Assert.Equal(90, storedProgress.BestScore);
        Assert.Equal(5, storedProgress.AttemptCount);
    }

    [Fact]
    public void GetLessons_UnlockedButNotCompleted_IsAvailableToStart()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Title = "Second Lesson",
                Order = 2,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true
        });

        var service = CreateService(lessons, progressStore);
        var result = service.GetLessons();

        Assert.Equal(2, result.Count);

        // First lesson is completed and unlocked
        Assert.True(result[0].IsUnlocked);
        Assert.True(result[0].IsCompleted);

        // Second lesson is unlocked (previous completed) but not completed
        Assert.True(result[1].IsUnlocked);
        Assert.False(result[1].IsCompleted);
    }

    [Fact]
    public void GetLessons_CompletedLesson_RemainsUnlocked()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1,
                Completion = new CompletionRule()
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true
        });

        var service = CreateService(lessons, progressStore);
        var result = service.GetLessons();

        Assert.Single(result);
        Assert.True(result[0].IsCompleted);
        Assert.True(result[0].IsUnlocked);
    }

    [Fact]
    public void GetLessons_LockedLessonStillAppearsInList()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "First Lesson",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Title = "Second Lesson",
                Order = 2,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            },
            new LessonDefinition
            {
                Id = "lesson-03",
                Title = "Third Lesson",
                Order = 3,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var service = CreateService(lessons);
        var result = service.GetLessons();

        Assert.Equal(3, result.Count);
        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
        Assert.False(result[2].IsUnlocked);
    }

    [Fact]
    public void GetLessons_PopulatesMetadataCorrectly()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Title = "Scale Practice",
                Description = "Practice major scales",
                Level = LessonLevel.Intermediate,
                Order = 5,
                EstimatedMinutes = 20
            }
        };

        var service = CreateService(lessons);
        var result = service.GetLessons();

        Assert.Single(result);
        Assert.Equal("lesson-01", result[0].Id);
        Assert.Equal("Scale Practice", result[0].Title);
        Assert.Equal("Practice major scales", result[0].Description);
        Assert.Equal(LessonLevel.Intermediate, result[0].Level);
        Assert.Equal(5, result[0].Order);
        Assert.Equal(20, result[0].EstimatedMinutes);
    }

    [Fact]
    public void GetLessons_EmptyCatalog_ReturnsEmptyList()
    {
        var service = CreateService([]);
        var result = service.GetLessons();

        Assert.Empty(result);
    }

    /// <summary>
    /// Minimal in-memory implementation of <see cref="ILessonCatalog"/>
    /// for testing purposes only.
    /// </summary>
    private sealed class FakeLessonCatalog : ILessonCatalog
    {
        private readonly IReadOnlyList<LessonDefinition> _lessons;

        public FakeLessonCatalog(IReadOnlyList<LessonDefinition> lessons)
        {
            _lessons = [.. lessons.OrderBy(l => l.Order)];
        }

        public IReadOnlyList<LessonDefinition> GetAll() => _lessons;

        public LessonDefinition? GetById(string id) =>
            _lessons.FirstOrDefault(l =>
                string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
