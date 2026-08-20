namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Manages practice sessions for lessons.
/// <para>
/// The service creates sessions from lesson definitions obtained via
/// <see cref="ILessonCatalog"/>. It never mutates the lesson definitions
/// or progress stores.
/// </para>
/// </summary>
public sealed class PracticeSessionService : IPracticeSessionService
{
    private readonly ILessonCatalog _lessonCatalog;

    /// <summary>
    /// Creates a new <see cref="PracticeSessionService"/>.
    /// </summary>
    /// <param name="lessonCatalog">
    /// The lesson catalog to retrieve lesson definitions from.
    /// Must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="lessonCatalog"/> is <c>null</c>.
    /// </exception>
    public PracticeSessionService(ILessonCatalog lessonCatalog)
    {
        ArgumentNullException.ThrowIfNull(lessonCatalog);
        _lessonCatalog = lessonCatalog;
    }

    /// <inheritdoc />
    public PracticeSession? StartSession(string lessonId)
    {
        ArgumentNullException.ThrowIfNull(lessonId);

        var lesson = _lessonCatalog.GetById(lessonId);
        if (lesson is null)
        {
            return null;
        }

        return new PracticeSession(lesson.Id, lesson.Exercises);
    }
}
