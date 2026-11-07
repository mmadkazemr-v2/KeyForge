namespace KeyForge.Features.Lessons.Models;

/// <summary>A course node in the hierarchical practice library.</summary>
public sealed class PracticeLibraryCourse
{
    public required CourseMetadata Metadata { get; init; }
    public required IReadOnlyList<PracticeLibrarySession> Sessions { get; init; }

    public int ExerciseCount => Sessions.Sum(session => session.ExerciseCount);
    public int CompletedExerciseCount => Sessions.Sum(session => session.CompletedExerciseCount);
}

/// <summary>A lesson/session node with its existing lesson state and exercise children.</summary>
public sealed class PracticeLibrarySession
{
    public required LessonListItem Lesson { get; init; }
    public required IReadOnlyList<PracticeLibraryExercise> Exercises { get; init; }

    public int ExerciseCount => Exercises.Count;
    public int CompletedExerciseCount => Exercises.Count(exercise => exercise.IsCompleted);
}

/// <summary>An exercise node in the practice library, linked by its existing lesson/exercise IDs.</summary>
public sealed class PracticeLibraryExercise
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;
    public ExerciseType Type { get; init; }
    public bool IsCompleted { get; init; }
}
