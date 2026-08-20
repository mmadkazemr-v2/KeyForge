using KeyForge.Features.Progress.Models;
using KeyForge.Features.Progress.Services;
using KeyForge.Infrastructure.Progress.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace KeyForge.Tests.Infrastructure.Progress.InMemory;

/// <summary>
/// Tests the <see cref="InMemoryProgressStore"/> behavior and verifies the
/// application's DI configuration resolves the intended implementation.
/// </summary>
public sealed class InMemoryProgressStoreTests
{
    private static InMemoryProgressStore CreateStore() => new();

    [Fact]
    public void GetProgress_UnknownLesson_ReturnsNull()
    {
        var store = CreateStore();

        var result = store.GetProgress("lesson-01");

        Assert.Null(result);
    }

    [Fact]
    public void SaveProgress_CanBeRetrievedByLessonId()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true,
            BestScore = 85
        });

        var result = store.GetProgress("lesson-01");

        Assert.NotNull(result);
        Assert.Equal("lesson-01", result.LessonId);
        Assert.True(result.IsCompleted);
        Assert.Equal(85, result.BestScore);
    }

    [Fact]
    public void SaveProgress_ExistingId_UpdatesExistingProgress()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 60,
            IsCompleted = false
        });

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 90,
            IsCompleted = true
        });

        var result = store.GetProgress("lesson-01");

        Assert.NotNull(result);
        Assert.Equal(90, result.BestScore);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public void ProgressForDifferentLessonIds_RemainsIndependent()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 70
        });

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-02",
            BestScore = 95
        });

        var p1 = store.GetProgress("lesson-01");
        var p2 = store.GetProgress("lesson-02");

        Assert.NotNull(p1);
        Assert.NotNull(p2);
        Assert.Equal(70, p1.BestScore);
        Assert.Equal(95, p2.BestScore);
    }

    [Fact]
    public void SaveProgress_BestScore_IsPreserved()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            BestScore = 42
        });

        var result = store.GetProgress("lesson-01");

        Assert.NotNull(result);
        Assert.Equal(42, result.BestScore);
    }

    [Fact]
    public void SaveProgress_IsCompleted_IsPreserved()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress
        {
            LessonId = "lesson-01",
            IsCompleted = true
        });

        var result = store.GetProgress("lesson-01");

        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public void GetAllProgress_DoesNotExposeMutableCollection()
    {
        var store = CreateStore();

        store.SaveProgress(new LessonProgress { LessonId = "lesson-01" });
        store.SaveProgress(new LessonProgress { LessonId = "lesson-02" });

        var all = store.GetAllProgress();
        var mutableView = Assert.IsAssignableFrom<IList<LessonProgress>>(all);

        Assert.Throws<NotSupportedException>(() =>
            mutableView.Add(new LessonProgress { LessonId = "lesson-03" }));

        Assert.Equal(2, store.GetAllProgress().Count);
    }

    [Fact]
    public void Di_CanResolveProgressStore()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IProgressStore, InMemoryProgressStore>();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IProgressStore>();

        Assert.NotNull(store);
        Assert.IsType<InMemoryProgressStore>(store);
    }
}
