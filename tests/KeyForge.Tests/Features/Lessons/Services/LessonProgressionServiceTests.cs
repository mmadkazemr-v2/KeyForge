using KeyForge.Features.Lessons.Models;
using KeyForge.Features.Lessons.Services;
using KeyForge.Features.Progress.Models;
using KeyForge.Features.Progress.Services;
using KeyForge.Infrastructure.Progress.InMemory;

namespace KeyForge.Tests.Features.Lessons.Services;

/// <summary>
/// Tests <see cref="LessonProgressionService"/> using in-memory fakes
/// to verify unlock evaluation behaviour without touching files or YAML.
/// </summary>
public sealed class LessonProgressionServiceTests
{
    private static InMemoryProgressStore CreateProgressStore() => new();

    private static LessonProgressionService CreateService(
        IReadOnlyList<LessonDefinition> lessons,
        IProgressStore? progressStore = null)
    {
        var catalog = new FakeLessonCatalog(lessons);
        var store = progressStore ?? CreateProgressStore();
        return new LessonProgressionService(catalog, store);
    }

    [Fact]
    public void IsUnlocked_ImmediateLesson_ReturnsTrue()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Order = 1,
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
        };

        var service = CreateService([lesson]);

        Assert.True(service.IsUnlocked("lesson-01"));
    }

    [Fact]
    public void IsUnlocked_PreviousLessonCompleted_NoProgress_ReturnsFalse()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Order = 2,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var service = CreateService(lessons);

        Assert.False(service.IsUnlocked("lesson-02"));
    }

    [Fact]
    public void IsUnlocked_PreviousLessonCompleted_PreviousIncomplete_ReturnsFalse()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Order = 2,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = false
        });

        var service = CreateService(lessons, progressStore);

        Assert.False(service.IsUnlocked("lesson-02"));
    }

    [Fact]
    public void IsUnlocked_PreviousLessonCompleted_PreviousCompleted_ReturnsTrue()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
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

        Assert.True(service.IsUnlocked("lesson-02"));
    }

    [Fact]
    public void IsUnlocked_PrerequisitesCompleted_OneIncomplete_ReturnsFalse()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Order = 2,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-03",
                Order = 3,
                Unlock = new UnlockRule
                {
                    Mode = UnlockMode.PrerequisitesCompleted,
                    RequiredLessonIds = ["lesson-01", "lesson-02"]
                }
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true
        });

        var service = CreateService(lessons, progressStore);

        Assert.False(service.IsUnlocked("lesson-03"));
    }

    [Fact]
    public void IsUnlocked_PrerequisitesCompleted_AllCompleted_ReturnsTrue()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Order = 2,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-03",
                Order = 3,
                Unlock = new UnlockRule
                {
                    Mode = UnlockMode.PrerequisitesCompleted,
                    RequiredLessonIds = ["lesson-01", "lesson-02"]
                }
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true
        });
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-02",
            IsCompleted = true
        });

        var service = CreateService(lessons, progressStore);

        Assert.True(service.IsUnlocked("lesson-03"));
    }

    [Fact]
    public void IsUnlocked_PrerequisitesCompleted_MissingProgress_ReturnsFalse()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-02",
                Order = 2,
                Unlock = new UnlockRule
                {
                    Mode = UnlockMode.PrerequisitesCompleted,
                    RequiredLessonIds = ["lesson-01"]
                }
            }
        };

        var service = CreateService(lessons);

        Assert.False(service.IsUnlocked("lesson-02"));
    }

    [Fact]
    public void IsUnlocked_UnknownLesson_ReturnsFalse()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            }
        };

        var service = CreateService(lessons);

        Assert.False(service.IsUnlocked("does-not-exist"));
    }

    [Fact]
    public void IsUnlocked_PreviousLessonCompleted_DeterminedByOrder()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-a",
                Order = 10,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-b",
                Order = 5,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-c",
                Order = 20,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-a",
            IsCompleted = true
        });
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-b",
            IsCompleted = false
        });

        var service = CreateService(lessons, progressStore);

        // Previous by Order is lesson-a (Order=10), which is completed
        Assert.True(service.IsUnlocked("lesson-c"));
    }

    [Fact]
    public void IsUnlocked_PreviousLessonCompleted_FirstLesson_Unlocked()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var service = CreateService(lessons);

        // No previous lesson exists, so the first lesson is unlocked
        Assert.True(service.IsUnlocked("lesson-01"));
    }

    [Fact]
    public void IsUnlocked_PrerequisitesCompleted_EmptyList_ReturnsTrue()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule
                {
                    Mode = UnlockMode.PrerequisitesCompleted,
                    RequiredLessonIds = []
                }
            }
        };

        var service = CreateService(lessons);

        Assert.True(service.IsUnlocked("lesson-01"));
    }

    [Fact]
    public void IsUnlocked_PreviousLessonCompleted_NonSequentialOrders()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-x",
                Order = 100,
                Unlock = new UnlockRule { Mode = UnlockMode.Immediate }
            },
            new LessonDefinition
            {
                Id = "lesson-y",
                Order = 500,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            },
            new LessonDefinition
            {
                Id = "lesson-z",
                Order = 999,
                Unlock = new UnlockRule { Mode = UnlockMode.PreviousLessonCompleted }
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-x",
            IsCompleted = true
        });

        var service = CreateService(lessons, progressStore);

        // lesson-y's previous is lesson-x (completed)
        Assert.True(service.IsUnlocked("lesson-y"));

        // lesson-z's previous is lesson-y (not completed)
        Assert.False(service.IsUnlocked("lesson-z"));
    }

    [Fact]
    public void IsUnlocked_DoNotMutateProgressOrLesson()
    {
        var lessons = new[]
        {
            new LessonDefinition
            {
                Id = "lesson-01",
                Order = 1,
                Unlock = new UnlockRule
                {
                    Mode = UnlockMode.PrerequisitesCompleted,
                    RequiredLessonIds = ["prereq"]
                }
            }
        };

        var progressStore = CreateProgressStore();
        progressStore.SaveProgress(new LessonProgress
        {
            LessonId = "prereq",
            IsCompleted = true
        });

        var service = CreateService(lessons, progressStore);
        service.IsUnlocked("lesson-01");

        // Verify lesson was not mutated
        Assert.Equal(UnlockMode.PrerequisitesCompleted, lessons[0].Unlock.Mode);
        Assert.Single(lessons[0].Unlock.RequiredLessonIds);

        // Verify progress was not mutated
        var prereq = progressStore.GetProgress("prereq");
        Assert.NotNull(prereq);
        Assert.True(prereq.IsCompleted);
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
