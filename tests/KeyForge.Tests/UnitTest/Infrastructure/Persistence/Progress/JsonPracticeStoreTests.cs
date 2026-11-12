namespace KeyForge.Tests.UnitTest.Infrastructure.Persistence.Progress;

public sealed class JsonPracticeStoreTests
{
    [Fact]
    public void SeparateProfileFilesKeepAttemptsAndProgressIsolated()
    {
        var directory = Path.Combine(Path.GetTempPath(), "KeyForgeProfiles", Guid.NewGuid().ToString("N"));
        var firstPath = Path.Combine(directory, "profile-one.json");
        var secondPath = Path.Combine(directory, "profile-two.json");
        try
        {
            var first = new JsonPracticeStore(firstPath);
            var second = new JsonPracticeStore(secondPath);
            first.RecordIfNew(new ExerciseAttempt
            {
                LessonId = "lesson",
                ExerciseId = "exercise",
                Score = 88,
                IsSuccessful = true,
                CompletedAt = DateTime.UtcNow
            });

            Assert.Single(first.GetAttemptsByLesson("lesson"));
            Assert.Empty(second.GetAttemptsByLesson("lesson"));
            Assert.Equal(88, first.GetProgress("lesson")!.BestScore);
            Assert.Null(second.GetProgress("lesson"));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void RestartPreservesAttemptsProgressAndIdentityDeduplication()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonPracticeStore(path);
            Assert.Empty(store.GetAllProgress());
            Assert.Empty(store.GetAttemptsByLesson("lesson"));
            Assert.False(File.Exists(path)); // Reading/navigation never writes.
            var attempt = new ExerciseAttempt { LessonId = "lesson", ExerciseId = "exercise", Score = 100, IsSuccessful = true, CompletedAt = DateTime.UtcNow };
            Assert.True(store.RecordIfNew(attempt));
            store.Record(new ExerciseAttempt { LessonId = "lesson", ExerciseId = "exercise", Score = 0, CompletedAt = DateTime.UtcNow });
            store.SaveProgress(new LessonProgress { LessonId = "lesson", IsCompleted = true, AttemptCount = 2, BestScore = 100 });
            var restarted = new JsonPracticeStore(path);
            Assert.False(restarted.RecordIfNew(attempt));
            Assert.Equal(2, restarted.GetAttemptsByLesson("lesson").Count);
            Assert.Single(restarted.GetAttemptsByLesson("lesson"), a => a.IsSuccessful);
            Assert.True(restarted.GetProgress("lesson")!.IsCompleted);
            Assert.Equal(2, restarted.GetProgress("lesson")!.AttemptCount);
            restarted.GetProgress("lesson")!.AttemptCount = 999;
            Assert.Equal(2, store.GetProgress("lesson")!.AttemptCount);
            File.WriteAllText(path, "broken");
            Assert.Throws<InvalidDataException>(() => restarted.GetAllProgress());
            File.WriteAllText(path, "");
            Assert.Empty(restarted.GetAllProgress());
        }
        finally { File.Delete(path); }
    }
}
