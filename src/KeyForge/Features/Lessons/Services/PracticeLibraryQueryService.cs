namespace KeyForge.Features.Lessons.Services;

/// <summary>
/// Groups the current lesson catalog into a practice-library tree. It does not
/// load content independently or create progress; existing queries and
/// successful attempt records remain the sources of truth.
/// </summary>
public sealed class PracticeLibraryQueryService(
    ILessonCatalog catalog,
    ILessonProgressQueryService lessonProgressQuery,
    IExerciseCompletionEvaluator exerciseCompletionEvaluator
) : IPracticeLibraryQueryService
{
    private readonly ILessonCatalog _catalog = catalog;
    private readonly ILessonProgressQueryService _lessonProgressQuery = lessonProgressQuery;
    private readonly IExerciseCompletionEvaluator _exerciseCompletionEvaluator = exerciseCompletionEvaluator;

    public IReadOnlyList<PracticeLibraryCourse> GetCourses()
    {
        var lessonStates = _lessonProgressQuery.GetLessons()
            .ToDictionary(lesson => lesson.Id, StringComparer.OrdinalIgnoreCase);

        var registeredLessons = _catalog.GetAll()
            .Where(lesson => lesson.Exercises.Count > 0)
            .Select(lesson => (Lesson: lesson, Course: lesson.Course ?? CourseMetadata.CreateKeyForgeFundamentals()))
            .ToArray();

        var courses = registeredLessons
            .GroupBy(entry => entry.Course.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => CreateCourse(group.Key, group.ToArray(), lessonStates))
            .OrderBy(course => course.Metadata.Order)
            .ThenBy(course => course.Metadata.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(course => course.Metadata.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Array.AsReadOnly(courses);
    }

    private PracticeLibraryCourse CreateCourse(
        string courseId,
        IReadOnlyList<(LessonDefinition Lesson, CourseMetadata Course)> entries,
        IReadOnlyDictionary<string, LessonListItem> lessonStates)
    {
        var metadata = entries[0].Course;
        var sessions = entries
            .OrderBy(entry => entry.Lesson.Order)
            .Select(entry => CreateSession(entry.Lesson, lessonStates))
            .ToArray();

        return new PracticeLibraryCourse
        {
            Metadata = new CourseMetadata
            {
                Id = courseId,
                Name = metadata.Name,
                Description = metadata.Description,
                Order = metadata.Order
            },
            Sessions = Array.AsReadOnly(sessions)
        };
    }

    private PracticeLibrarySession CreateSession(
        LessonDefinition lesson,
        IReadOnlyDictionary<string, LessonListItem> lessonStates)
    {
        var completedExerciseIds = _exerciseCompletionEvaluator.GetCompletedExerciseIds(
            lesson.Id,
            lesson.Exercises);
        var exercises = lesson.Exercises
            .Select(exercise => new PracticeLibraryExercise
            {
                Id = exercise.Id,
                Title = exercise.Title,
                Description = exercise.Description,
                Type = exercise.Type,
                IsCompleted = completedExerciseIds.Contains(exercise.Id)
            })
            .ToArray();

        return new PracticeLibrarySession
        {
            Lesson = lessonStates[lesson.Id],
            Exercises = Array.AsReadOnly(exercises)
        };
    }
}
